# Plan de pruebas funcionales

Base de datos recién creada con `03_BaseDatos.sql`. Marque el resultado y adjunte captura en el informe (sin credenciales).

| # | Prueba | Acción | Resultado esperado | Resultado obtenido |
|---|---|---|---|---|
| 1 | Registro válido | Llenar todos los campos y **Guardar** | Mensaje de éxito; aparece en listado y en SQL Server; indicador *Total* +1 | ☐ |
| 2 | Título vacío | Guardar sin título | Mensaje de validación; no inserta | ☐ |
| 3 | Cupo cero | Cupo = 0 | Se rechaza (“mayor que cero”) | ☐ |
| 4 | Cupo negativo | Cupo = -5 | Se rechaza | ☐ |
| 5 | Cupo no numérico / vacío | Cupo vacío | Mensaje “debe ser un número entero” | ☐ |
| 6 | Fechas incoherentes | Fin anterior a inicio | Mensaje de validación | ☐ |
| 7 | Horas incoherentes | Hora fin ≤ hora inicio | Mensaje de validación | ☐ |
| 8 | Tipo sin seleccionar | Dejar “Seleccione…” | Mensaje de validación | ☐ |
| 9 | Duplicado | Repetir título y fecha de inicio existentes | “Ya existe una actividad…” | ☐ |
| 10 | Buscar por nombre | Escribir “Taller” → Buscar | Solo actividades con “Taller” | ☐ |
| 11 | Filtrar por tipo/estado/fecha | Combinar filtros | Resultados correctos; *Quitar filtros* restaura | ☐ |
| 12 | Búsqueda sin resultados | Texto inexistente | “No hay actividades que coincidan…” | ☐ |
| 13 | Ver detalle | Botón **Ver** | Se llenan Detalles, Inscritos y Vista del estudiante | ☐ |
| 14 | Editar | **Editar**, cambiar lugar, Guardar | Título “Editar…”; se actualiza el registro | ☐ |
| 15 | Cupo menor a inscritos | Editar “Taller de Programación Web” con cupo 2 | Se rechaza (hay 3 inscritos) | ☐ |
| 16 | Desactivar | **Desactivar** → confirmar | Estado *Inactiva*; ya no cuenta en *Total* | ☐ |
| 17 | Inscribir | Ver una actividad, elegir estudiante, **Inscribir** | Aparece en la tabla; el estudiante sale de la lista | ☐ |
| 18 | Cupo agotado | “Taller de Git y GitHub” (cupo 3, 3 inscritos) → inscribir | “No hay cupos disponibles” | ☐ |
| 19 | Inscripción duplicada | Inscribir al mismo estudiante dos veces (usar dos pestañas) | Mensaje de duplicado; solo 1 fila en BD | ☐ |
| 20 | Actividad finalizada | Ver “Encuentro de Investigación” | Panel de inscripción oculto; “Inscripciones cerradas” | ☐ |
| 21 | Quitar inscripción | **Quitar** → confirmar | Se elimina; cupos disponibles +1 | ☐ |
| 22 | Inyección SQL | Título `'; DROP TABLE Actividad;--` | Se guarda como texto; la tabla sigue existiendo | ☐ |
| 23 | XSS | Título `<script>alert(1)</script>` | Se muestra como texto, no se ejecuta | ☐ |
| 24 | Fallo de BD | Detener SQL Server / cadena incorrecta | Mensaje controlado, sin trazas técnicas | ☐ |

## Verificación en SQL Server

```sql
SELECT TOP 5 * FROM dbo.Actividad ORDER BY ActividadId DESC;
SELECT ActividadId, COUNT(*) AS Inscritos FROM dbo.Inscripcion GROUP BY ActividadId;
```
