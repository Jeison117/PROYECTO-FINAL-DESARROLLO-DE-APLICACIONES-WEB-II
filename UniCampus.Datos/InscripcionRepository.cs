using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UniCampus.Entidades;

namespace UniCampus.Datos
{
    /// <summary>Códigos de resultado de la operación atómica de inscripción.</summary>
    public enum ResultadoInscripcion
    {
        Correcta = 0,
        ActividadNoExiste = 1,
        ActividadNoDisponible = 2,
        EstudianteNoExiste = 3,
        Duplicada = 4,
        SinCupo = 5
    }

    public interface IInscripcionRepository
    {
        List<Inscripcion> ListarPorActividad(int actividadId);
        ResultadoInscripcion Inscribir(int actividadId, int estudianteId);
        bool Cancelar(int inscripcionId);
    }

    public class InscripcionRepository : IInscripcionRepository
    {
        public List<Inscripcion> ListarPorActividad(int actividadId)
        {
            const string sql =
                "SELECT i.InscripcionId, i.ActividadId, i.EstudianteId, i.FechaInscripcion, e.Nombre, e.Programa " +
                "FROM dbo.Inscripcion i INNER JOIN dbo.Estudiante e ON e.EstudianteId = i.EstudianteId " +
                "WHERE i.ActividadId = @ActividadId ORDER BY i.FechaInscripcion, i.InscripcionId";
            var lista = new List<Inscripcion>();
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                cn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new Inscripcion
                        {
                            InscripcionId = (int)rd["InscripcionId"],
                            ActividadId = (int)rd["ActividadId"],
                            EstudianteId = (int)rd["EstudianteId"],
                            FechaInscripcion = (DateTime)rd["FechaInscripcion"],
                            EstudianteNombre = (string)rd["Nombre"],
                            Programa = (string)rd["Programa"]
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Inscribe de forma atómica. La actividad se bloquea (UPDLOCK, HOLDLOCK) dentro de la
        /// transacción, de modo que dos solicitudes simultáneas no puedan superar el cupo.
        /// </summary>
        public ResultadoInscripcion Inscribir(int actividadId, int estudianteId)
        {
            using (SqlConnection cn = Conexion.Crear())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int cupo;
                        string estado;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT Cupo, Estado FROM dbo.Actividad WITH (UPDLOCK, HOLDLOCK) WHERE ActividadId = @ActividadId",
                            cn, tx))
                        {
                            Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                if (!rd.Read()) { tx.Rollback(); return ResultadoInscripcion.ActividadNoExiste; }
                                cupo = rd.GetInt32(0);
                                estado = rd.GetString(1);
                            }
                        }

                        if (estado != CatalogoActividad.EstadoActiva && estado != CatalogoActividad.EstadoProxima)
                        {
                            tx.Rollback();
                            return ResultadoInscripcion.ActividadNoDisponible;
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.Estudiante WHERE EstudianteId = @EstudianteId AND Activo = 1", cn, tx))
                        {
                            Conexion.Param(cmd, "@EstudianteId", SqlDbType.Int, estudianteId);
                            if ((int)cmd.ExecuteScalar() == 0) { tx.Rollback(); return ResultadoInscripcion.EstudianteNoExiste; }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.Inscripcion WHERE ActividadId = @ActividadId AND EstudianteId = @EstudianteId",
                            cn, tx))
                        {
                            Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                            Conexion.Param(cmd, "@EstudianteId", SqlDbType.Int, estudianteId);
                            if ((int)cmd.ExecuteScalar() > 0) { tx.Rollback(); return ResultadoInscripcion.Duplicada; }
                        }

                        int inscritos;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.Inscripcion WHERE ActividadId = @ActividadId", cn, tx))
                        {
                            Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                            inscritos = (int)cmd.ExecuteScalar();
                        }
                        if (inscritos >= cupo) { tx.Rollback(); return ResultadoInscripcion.SinCupo; }

                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO dbo.Inscripcion (ActividadId, EstudianteId) VALUES (@ActividadId, @EstudianteId)",
                            cn, tx))
                        {
                            Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                            Conexion.Param(cmd, "@EstudianteId", SqlDbType.Int, estudianteId);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return ResultadoInscripcion.Correcta;
                    }
                    catch (SqlException ex)
                    {
                        try { tx.Rollback(); } catch (InvalidOperationException) { }
                        // 2627/2601: violación de la restricción UNIQUE (inscripción duplicada concurrente)
                        if (ex.Number == 2627 || ex.Number == 2601) return ResultadoInscripcion.Duplicada;
                        throw;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch (InvalidOperationException) { }
                        throw;
                    }
                }
            }
        }

        public bool Cancelar(int inscripcionId)
        {
            const string sql = "DELETE FROM dbo.Inscripcion WHERE InscripcionId = @InscripcionId";
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                Conexion.Param(cmd, "@InscripcionId", SqlDbType.Int, inscripcionId);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
