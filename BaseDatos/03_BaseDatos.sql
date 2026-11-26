/* =====================================================================
   UniCampus - Registro de Actividades Académicas
   Script reproducible: crea la base, la estructura y datos de prueba.
   Es idempotente: se puede ejecutar varias veces sin duplicar datos.
   Motor: SQL Server 2016 o superior (Express/LocalDB incluidos).
   ===================================================================== */
SET NOCOUNT ON;
GO

IF DB_ID(N'UniCampusDB') IS NULL
    CREATE DATABASE UniCampusDB;
GO

USE UniCampusDB;
GO

/* ---------------------------------------------------------------------
   1. ESTRUCTURA
   --------------------------------------------------------------------- */

-- Catálogo de tipos de actividad (Taller, Conferencia, ...)
IF OBJECT_ID(N'dbo.TipoActividad', N'U') IS NULL
CREATE TABLE dbo.TipoActividad (
    TipoId      INT IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(50)  NOT NULL,
    Descripcion NVARCHAR(200) NULL,
    CONSTRAINT PK_TipoActividad PRIMARY KEY (TipoId),
    CONSTRAINT UQ_TipoActividad_Nombre UNIQUE (Nombre)
);
GO

-- Estudiantes / participantes
IF OBJECT_ID(N'dbo.Estudiante', N'U') IS NULL
CREATE TABLE dbo.Estudiante (
    EstudianteId INT IDENTITY(1,1) NOT NULL,
    Nombre       NVARCHAR(100) NOT NULL,
    Correo       NVARCHAR(150) NOT NULL,
    Programa     NVARCHAR(100) NOT NULL,
    Activo       BIT NOT NULL CONSTRAINT DF_Estudiante_Activo DEFAULT (1),
    CONSTRAINT PK_Estudiante PRIMARY KEY (EstudianteId),
    CONSTRAINT UQ_Estudiante_Correo UNIQUE (Correo)
);
GO

-- Entidad central
IF OBJECT_ID(N'dbo.Actividad', N'U') IS NULL
CREATE TABLE dbo.Actividad (
    ActividadId INT IDENTITY(1,1) NOT NULL,
    Titulo      NVARCHAR(150)  NOT NULL,
    Descripcion NVARCHAR(1000) NOT NULL,
    TipoId      INT            NOT NULL,
    FechaInicio DATE           NOT NULL,
    FechaFin    DATE           NOT NULL,
    HoraInicio  TIME(0)        NOT NULL,
    HoraFin     TIME(0)        NOT NULL,
    Modalidad   NVARCHAR(20)   NOT NULL,
    Lugar       NVARCHAR(100)  NOT NULL,
    Cupo        INT            NOT NULL,
    Estado      NVARCHAR(20)   NOT NULL CONSTRAINT DF_Actividad_Estado DEFAULT (N'Activa'),
    CONSTRAINT PK_Actividad PRIMARY KEY (ActividadId),
    CONSTRAINT FK_Actividad_Tipo FOREIGN KEY (TipoId) REFERENCES dbo.TipoActividad (TipoId),
    CONSTRAINT CK_Actividad_Cupo      CHECK (Cupo > 0),
    CONSTRAINT CK_Actividad_Fechas    CHECK (FechaFin >= FechaInicio),
    CONSTRAINT CK_Actividad_Horas     CHECK (HoraFin > HoraInicio),
    CONSTRAINT CK_Actividad_Modalidad CHECK (Modalidad IN (N'Presencial', N'Virtual', N'Hibrida')),
    CONSTRAINT CK_Actividad_Estado    CHECK (Estado IN (N'Activa', N'Proxima', N'Finalizada', N'Inactiva')),
    CONSTRAINT CK_Actividad_Titulo    CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0)
);
GO

-- Relación estudiante - actividad (sin duplicados)
IF OBJECT_ID(N'dbo.Inscripcion', N'U') IS NULL
CREATE TABLE dbo.Inscripcion (
    InscripcionId    INT IDENTITY(1,1) NOT NULL,
    ActividadId      INT NOT NULL,
    EstudianteId     INT NOT NULL,
    FechaInscripcion DATETIME2(0) NOT NULL CONSTRAINT DF_Inscripcion_Fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Inscripcion PRIMARY KEY (InscripcionId),
    CONSTRAINT FK_Inscripcion_Actividad  FOREIGN KEY (ActividadId)  REFERENCES dbo.Actividad (ActividadId),
    CONSTRAINT FK_Inscripcion_Estudiante FOREIGN KEY (EstudianteId) REFERENCES dbo.Estudiante (EstudianteId),
    CONSTRAINT UQ_Inscripcion_Actividad_Estudiante UNIQUE (ActividadId, EstudianteId)
);
GO

-- Índices de apoyo a las búsquedas del listado
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Actividad_Estado_Fecha' AND object_id = OBJECT_ID(N'dbo.Actividad'))
    CREATE INDEX IX_Actividad_Estado_Fecha ON dbo.Actividad (Estado, FechaInicio);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Actividad_Tipo' AND object_id = OBJECT_ID(N'dbo.Actividad'))
    CREATE INDEX IX_Actividad_Tipo ON dbo.Actividad (TipoId);
GO

/* ---------------------------------------------------------------------
   2. DATOS DE PRUEBA (no sensibles, ficticios)
   --------------------------------------------------------------------- */

INSERT INTO dbo.TipoActividad (Nombre, Descripcion)
SELECT v.Nombre, v.Descripcion
FROM (VALUES
    (N'Taller',       N'Espacio práctico de aprendizaje'),
    (N'Conferencia',  N'Charla magistral con invitado'),
    (N'Seminario',    N'Sesión de discusión especializada'),
    (N'Curso',        N'Formación estructurada de varias sesiones'),
    (N'Encuentro',    N'Reunión académica o investigativa')
) AS v(Nombre, Descripcion)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TipoActividad t WHERE t.Nombre = v.Nombre);
GO

INSERT INTO dbo.Estudiante (Nombre, Correo, Programa)
SELECT v.Nombre, v.Correo, v.Programa
FROM (VALUES
    (N'Ana Torres',     N'ana.torres@ejemplo.edu',     N'Ingeniería de Sistemas'),
    (N'Carlos Pérez',   N'carlos.perez@ejemplo.edu',   N'Diseño Gráfico'),
    (N'Laura Gómez',    N'laura.gomez@ejemplo.edu',    N'Administración'),
    (N'Miguel Rojas',   N'miguel.rojas@ejemplo.edu',   N'Ingeniería de Sistemas'),
    (N'Sofía Herrera',  N'sofia.herrera@ejemplo.edu',  N'Psicología'),
    (N'Daniel Castro',  N'daniel.castro@ejemplo.edu',  N'Contaduría Pública'),
    (N'Valentina Ruiz', N'valentina.ruiz@ejemplo.edu', N'Diseño Gráfico'),
    (N'Andrés Molina',  N'andres.molina@ejemplo.edu',  N'Derecho')
) AS v(Nombre, Correo, Programa)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Estudiante e WHERE e.Correo = v.Correo);
GO

INSERT INTO dbo.Actividad (Titulo, Descripcion, TipoId, FechaInicio, FechaFin, HoraInicio, HoraFin, Modalidad, Lugar, Cupo, Estado)
SELECT v.Titulo, v.Descripcion, t.TipoId, v.FechaInicio, v.FechaFin, v.HoraInicio, v.HoraFin, v.Modalidad, v.Lugar, v.Cupo, v.Estado
FROM (VALUES
    (N'Taller de Programación Web',  N'Espacio práctico para reforzar conceptos de programación web con ASP.NET.', N'Taller',      '2026-05-15', '2026-05-16', '08:00', '12:00', N'Presencial', N'Laboratorio 3',      30,  N'Activa'),
    (N'Conferencia de IA',           N'Charla sobre aplicaciones de la inteligencia artificial en la educación.',   N'Conferencia', '2026-05-20', '2026-05-20', '10:00', '12:00', N'Hibrida',    N'Auditorio Principal', 100, N'Proxima'),
    (N'Seminario de Bases de Datos', N'Modelado relacional, normalización y consultas con SQL Server.',              N'Seminario',   '2026-06-05', '2026-06-06', '14:00', '18:00', N'Presencial', N'Aula 204',           50,  N'Proxima'),
    (N'Curso de Diseño UX/UI',       N'Fundamentos de experiencia e interfaz de usuario con prototipado.',           N'Curso',       '2026-06-10', '2026-06-24', '18:00', '20:00', N'Virtual',    N'Plataforma Teams',    40,  N'Proxima'),
    (N'Encuentro de Investigación',  N'Presentación de avances de los semilleros de investigación.',                 N'Encuentro',   '2026-04-15', '2026-04-15', '09:00', '13:00', N'Presencial', N'Sala de Juntas',      80,  N'Finalizada'),
    (N'Taller de Git y GitHub',      N'Control de versiones y trabajo colaborativo con repositorios remotos.',      N'Taller',      '2026-11-12', '2026-11-12', '15:00', '17:00', N'Virtual',    N'Plataforma Teams',    3,   N'Activa')
) AS v(Titulo, Descripcion, Tipo, FechaInicio, FechaFin, HoraInicio, HoraFin, Modalidad, Lugar, Cupo, Estado)
INNER JOIN dbo.TipoActividad t ON t.Nombre = v.Tipo
WHERE NOT EXISTS (SELECT 1 FROM dbo.Actividad a WHERE a.Titulo = v.Titulo AND a.FechaInicio = v.FechaInicio);
GO

-- Inscripciones de ejemplo (la última actividad tiene cupo 3 para probar "cupo agotado")
INSERT INTO dbo.Inscripcion (ActividadId, EstudianteId)
SELECT a.ActividadId, e.EstudianteId
FROM (VALUES
    (N'Taller de Programación Web', N'ana.torres@ejemplo.edu'),
    (N'Taller de Programación Web', N'carlos.perez@ejemplo.edu'),
    (N'Taller de Programación Web', N'laura.gomez@ejemplo.edu'),
    (N'Taller de Git y GitHub',     N'miguel.rojas@ejemplo.edu'),
    (N'Taller de Git y GitHub',     N'sofia.herrera@ejemplo.edu'),
    (N'Taller de Git y GitHub',     N'daniel.castro@ejemplo.edu')
) AS v(Titulo, Correo)
INNER JOIN dbo.Actividad  a ON a.Titulo = v.Titulo
INNER JOIN dbo.Estudiante e ON e.Correo = v.Correo
WHERE NOT EXISTS (SELECT 1 FROM dbo.Inscripcion i WHERE i.ActividadId = a.ActividadId AND i.EstudianteId = e.EstudianteId);
GO

/* ---------------------------------------------------------------------
   3. VERIFICACIÓN RÁPIDA
   --------------------------------------------------------------------- */
SELECT a.ActividadId, a.Titulo, t.Nombre AS Tipo, a.FechaInicio, a.Cupo, a.Estado,
       (SELECT COUNT(*) FROM dbo.Inscripcion i WHERE i.ActividadId = a.ActividadId) AS Inscritos
FROM dbo.Actividad a INNER JOIN dbo.TipoActividad t ON t.TipoId = a.TipoId
ORDER BY a.ActividadId;
GO
