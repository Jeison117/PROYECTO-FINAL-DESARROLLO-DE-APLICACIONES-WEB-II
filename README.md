# UniCampus – Plataforma de Registro de Actividades Académicas

**Corporación Universitaria Americana**
Asignatura: Desarrollo de Aplicaciones Web II · Proyecto final del tercer corte
Docente: Ingeniera Cindy Fuentes

## Integrantes

- Jeison Castillo
- Yovier Carballo
- Alvaro Avila
- Esneider Barrozo
- Junior de la Hoz

**Repositorio GitHub:** (https://github.com/Jeison117/PROYECTO-FINAL-DESARROLLO-DE-APLICACIONES-WEB-II.git)

## Descripción

Aplicación web que permite gestionar las actividades académicas de la institución: registro, consulta, búsqueda, actualización y desactivación, así como la inscripción de estudiantes con control de cupos.

## Tecnologías

C# · ASP.NET Web Forms (.NET Framework 4.8.1) · ADO.NET · SQL Server 2016 o superior · Visual Studio 2022

## Arquitectura

Se aplica una arquitectura de tres capas, con el patrón Model–View–Presenter (MVP) en la capa de presentación.

| Proyecto | Responsabilidad |
|---|---|
| `UniCampus.Entidades` | Objetos de datos compartidos entre capas. |
| `UniCampus.Datos` | Acceso a SQL Server mediante consultas parametrizadas. |
| `UniCampus.Negocio` | Validaciones y reglas de negocio. |
| `UniCampus.Web` | Interfaz Web Forms, vista (`IActividadView`) y presentador (`ActividadPresenter`). |

Flujo de dependencias: Web → Negocio → Datos → SQL Server. Mayor detalle en `docs/ARQUITECTURA.md`; casos de prueba en `docs/PLAN_DE_PRUEBAS.md`.

## Instrucciones de ejecución

1. Ejecutar `BaseDatos/03_BaseDatos.sql` en SQL Server Management Studio. El script crea la base `UniCampusDB`, sus tablas y los datos de prueba, y puede ejecutarse más de una vez sin duplicar información.
2. En `UniCampus.Web/Web.config`, ajustar únicamente el servidor de la cadena de conexión `UniCampusDB` (por defecto `.\SQLEXPRESS`). La conexión usa autenticación de Windows; no se almacenan credenciales.
3. Abrir `UniCampus.sln` en Visual Studio, establecer `UniCampus.Web` como proyecto de inicio y ejecutar con F5.

## Funcionalidades

- Registro, edición y desactivación de actividades.
- Listado paginado con búsqueda y filtros por nombre, tipo, estado y fecha.
- Consulta del detalle de cada actividad.
- Inscripción de estudiantes con control de cupos y sin duplicados.
- Mensajes de éxito, validación y error.

**Alcance:** las reseñas y valoraciones, de carácter opcional, no se incluyen en esta entrega.

## Seguridad

- Validación en el servidor y consultas SQL parametrizadas.
- Codificación de la salida HTML para prevenir XSS.
- Inscripción mediante transacción que evita superar el cupo ante solicitudes simultáneas.
- Sin contraseñas ni secretos en el repositorio.
