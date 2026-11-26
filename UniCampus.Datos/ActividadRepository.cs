using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UniCampus.Entidades;

namespace UniCampus.Datos
{
    public interface IActividadRepository
    {
        List<Actividad> Listar(FiltroActividad filtro);
        Actividad ObtenerPorId(int actividadId);
        int Insertar(Actividad actividad);
        bool Actualizar(Actividad actividad);
        bool Desactivar(int actividadId);
        bool ExisteDuplicado(string titulo, DateTime fechaInicio, int actividadIdExcluir);
        EstadisticasActividad ObtenerEstadisticas();
    }

    /// <summary>
    /// Acceso a datos de Actividad. Todas las consultas usan parámetros ADO.NET;
    /// nunca se concatena texto del usuario dentro del SQL.
    /// </summary>
    public class ActividadRepository : IActividadRepository
    {
        private const string SelectBase =
            "SELECT a.ActividadId, a.Titulo, a.Descripcion, a.TipoId, t.Nombre AS TipoNombre, " +
            "       a.FechaInicio, a.FechaFin, a.HoraInicio, a.HoraFin, a.Modalidad, a.Lugar, a.Cupo, a.Estado, " +
            "       (SELECT COUNT(*) FROM dbo.Inscripcion i WHERE i.ActividadId = a.ActividadId) AS Inscritos " +
            "FROM dbo.Actividad a " +
            "INNER JOIN dbo.TipoActividad t ON t.TipoId = a.TipoId ";

        public List<Actividad> Listar(FiltroActividad filtro)
        {
            if (filtro == null) filtro = new FiltroActividad();

            const string sql = SelectBase +
                "WHERE (@Texto IS NULL OR a.Titulo LIKE '%' + @Texto + '%') " +
                "  AND (@TipoId IS NULL OR a.TipoId = @TipoId) " +
                "  AND (@Estado IS NULL OR a.Estado = @Estado) " +
                "  AND (@Fecha IS NULL OR @Fecha BETWEEN a.FechaInicio AND a.FechaFin) " +
                "ORDER BY a.FechaInicio DESC, a.ActividadId DESC";

            var lista = new List<Actividad>();
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                string texto = string.IsNullOrWhiteSpace(filtro.Texto) ? null : filtro.Texto.Trim();
                string estado = string.IsNullOrWhiteSpace(filtro.Estado) ? null : filtro.Estado;
                Conexion.Param(cmd, "@Texto", SqlDbType.NVarChar, texto, 150);
                Conexion.Param(cmd, "@TipoId", SqlDbType.Int, filtro.TipoId);
                Conexion.Param(cmd, "@Estado", SqlDbType.NVarChar, estado, 20);
                Conexion.Param(cmd, "@Fecha", SqlDbType.Date, filtro.Fecha);

                cn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) lista.Add(Mapear(rd));
                }
            }
            return lista;
        }

        public Actividad ObtenerPorId(int actividadId)
        {
            const string sql = SelectBase + "WHERE a.ActividadId = @ActividadId";
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                cn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    return rd.Read() ? Mapear(rd) : null;
                }
            }
        }

        public int Insertar(Actividad a)
        {
            const string sql =
                "INSERT INTO dbo.Actividad (Titulo, Descripcion, TipoId, FechaInicio, FechaFin, HoraInicio, HoraFin, Modalidad, Lugar, Cupo, Estado) " +
                "VALUES (@Titulo, @Descripcion, @TipoId, @FechaInicio, @FechaFin, @HoraInicio, @HoraFin, @Modalidad, @Lugar, @Cupo, @Estado); " +
                "SELECT CAST(SCOPE_IDENTITY() AS int);";

            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                AgregarParametros(cmd, a);
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public bool Actualizar(Actividad a)
        {
            const string sql =
                "UPDATE dbo.Actividad SET Titulo = @Titulo, Descripcion = @Descripcion, TipoId = @TipoId, " +
                "FechaInicio = @FechaInicio, FechaFin = @FechaFin, HoraInicio = @HoraInicio, HoraFin = @HoraFin, " +
                "Modalidad = @Modalidad, Lugar = @Lugar, Cupo = @Cupo, Estado = @Estado " +
                "WHERE ActividadId = @ActividadId";

            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                AgregarParametros(cmd, a);
                Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, a.ActividadId);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>Eliminación lógica: conserva el historial de inscripciones.</summary>
        public bool Desactivar(int actividadId)
        {
            const string sql = "UPDATE dbo.Actividad SET Estado = 'Inactiva' WHERE ActividadId = @ActividadId";
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ExisteDuplicado(string titulo, DateTime fechaInicio, int actividadIdExcluir)
        {
            const string sql =
                "SELECT COUNT(*) FROM dbo.Actividad " +
                "WHERE Titulo = @Titulo AND FechaInicio = @FechaInicio AND ActividadId <> @ActividadId";
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                Conexion.Param(cmd, "@Titulo", SqlDbType.NVarChar, titulo, 150);
                Conexion.Param(cmd, "@FechaInicio", SqlDbType.Date, fechaInicio);
                Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadIdExcluir);
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public EstadisticasActividad ObtenerEstadisticas()
        {
            const string sql =
                "SELECT " +
                " (SELECT COUNT(*) FROM dbo.Actividad WHERE Estado <> 'Inactiva') AS Total, " +
                " (SELECT COUNT(*) FROM dbo.Inscripcion i INNER JOIN dbo.Actividad a ON a.ActividadId = i.ActividadId WHERE a.Estado <> 'Inactiva') AS Inscritos, " +
                " (SELECT COUNT(*) FROM dbo.Actividad WHERE Estado = 'Proxima') AS Proximas, " +
                " (SELECT COUNT(*) FROM dbo.Actividad WHERE Estado = 'Finalizada') AS Finalizadas";

            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    rd.Read();
                    return new EstadisticasActividad
                    {
                        Total = rd.GetInt32(0),
                        EstudiantesInscritos = rd.GetInt32(1),
                        Proximas = rd.GetInt32(2),
                        Finalizadas = rd.GetInt32(3)
                    };
                }
            }
        }

        private static void AgregarParametros(SqlCommand cmd, Actividad a)
        {
            Conexion.Param(cmd, "@Titulo", SqlDbType.NVarChar, a.Titulo, 150);
            Conexion.Param(cmd, "@Descripcion", SqlDbType.NVarChar, a.Descripcion, 1000);
            Conexion.Param(cmd, "@TipoId", SqlDbType.Int, a.TipoId);
            Conexion.Param(cmd, "@FechaInicio", SqlDbType.Date, a.FechaInicio.Date);
            Conexion.Param(cmd, "@FechaFin", SqlDbType.Date, a.FechaFin.Date);
            Conexion.Param(cmd, "@HoraInicio", SqlDbType.Time, a.HoraInicio);
            Conexion.Param(cmd, "@HoraFin", SqlDbType.Time, a.HoraFin);
            Conexion.Param(cmd, "@Modalidad", SqlDbType.NVarChar, a.Modalidad, 20);
            Conexion.Param(cmd, "@Lugar", SqlDbType.NVarChar, a.Lugar, 100);
            Conexion.Param(cmd, "@Cupo", SqlDbType.Int, a.Cupo);
            Conexion.Param(cmd, "@Estado", SqlDbType.NVarChar, a.Estado, 20);
        }

        private static Actividad Mapear(SqlDataReader rd)
        {
            return new Actividad
            {
                ActividadId = (int)rd["ActividadId"],
                Titulo = (string)rd["Titulo"],
                Descripcion = (string)rd["Descripcion"],
                TipoId = (int)rd["TipoId"],
                TipoNombre = (string)rd["TipoNombre"],
                FechaInicio = (DateTime)rd["FechaInicio"],
                FechaFin = (DateTime)rd["FechaFin"],
                HoraInicio = (TimeSpan)rd["HoraInicio"],
                HoraFin = (TimeSpan)rd["HoraFin"],
                Modalidad = (string)rd["Modalidad"],
                Lugar = (string)rd["Lugar"],
                Cupo = (int)rd["Cupo"],
                Estado = (string)rd["Estado"],
                Inscritos = (int)rd["Inscritos"]
            };
        }
    }
}
