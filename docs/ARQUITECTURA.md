# Arquitectura de tres capas + MVP

## Dependencias entre proyectos

```
UniCampus.Web ──► UniCampus.Negocio ──► UniCampus.Datos ──► SQL Server
      │                  │                    │
      └──────────────────┴────────────────────┴──► UniCampus.Entidades
```

La capa de datos no referencia controles visuales; la capa de negocio no referencia `System.Web`; el Presenter no conoce `System.Data`.

## MVP dentro de la presentación

| Rol | Clase | Qué hace |
|---|---|---|
| View | `Actividades.aspx` / `.aspx.cs` (implementa `IActividadView`) | Lee y escribe controles; lanza eventos. Sin SQL ni reglas. |
| Contrato | `Views/IActividadView.cs` | Propiedades del formulario y métodos `Mostrar…` que el Presenter puede invocar. |
| Presenter | `Presenters/ActividadPresenter.cs` | Convierte texto → entidad, llama a los servicios, decide qué mensaje/lista mostrar. |

## Flujo de «Guardar»

1. El usuario completa el formulario y pulsa **Guardar** (`btnGuardar_Click`).
2. La página llama `ActividadPresenter.Guardar()`.
3. El Presenter lee los datos desde `IActividadView` y construye una `Actividad` (detecta fechas/horas/números ilegibles).
4. `ActividadService.Guardar` valida campos y reglas (título, descripción, fechas, horas, cupo, estado, duplicado, cupo ≥ inscritos al editar).
5. Si hay errores devuelve `ResultadoOperacion` con la lista; el Presenter ordena a la vista mostrarlos y **no** se inserta.
6. Si es válido, `ActividadRepository.Insertar/Actualizar` ejecuta SQL parametrizado.
7. El Presenter limpia el formulario, muestra el mensaje de éxito y refresca listado, indicadores y detalle.

## Flujo de inscripción (control de cupo)

`InscripcionService.Inscribir` → `InscripcionRepository.Inscribir`: abre una transacción `SERIALIZABLE`, bloquea la fila de la actividad (`UPDLOCK, HOLDLOCK`), verifica estado, estudiante, duplicado y cupo, e inserta; cualquier fallo hace *rollback*. La restricción `UNIQUE (ActividadId, EstudianteId)` es la última barrera ante concurrencia.

## Modelo de datos

```
TipoActividad 1───* Actividad 1───* Inscripcion *───1 Estudiante
```

| Tabla | Clave | Restricciones destacadas |
|---|---|---|
| TipoActividad | TipoId | Nombre único |
| Estudiante | EstudianteId | Correo único, `Activo` |
| Actividad | ActividadId | FK a TipoActividad; `Cupo > 0`; `FechaFin ≥ FechaInicio`; `HoraFin > HoraInicio`; modalidad y estado en lista |
| Inscripcion | InscripcionId | FK a Actividad y Estudiante; `UNIQUE(ActividadId, EstudianteId)` |

> Si en su diagrama entidad-relación existente ya hay tablas equivalentes (usuarios/personas, tipos, inscripciones), ajuste los nombres en `03_BaseDatos.sql` y en los repositorios en lugar de duplicarlas, y actualice este diagrama.
