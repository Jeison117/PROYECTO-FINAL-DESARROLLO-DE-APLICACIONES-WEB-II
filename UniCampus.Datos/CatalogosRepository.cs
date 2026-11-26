using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UniCampus.Entidades;

namespace UniCampus.Datos
{
    public interface ITipoActividadRepository
    {
        List<TipoActividad> Listar();
    }

    public interface IEstudianteRepository
    {
        /// <summary>Estudiantes que aún no están inscritos en la actividad indicada.</summary>
        List<Estudiante> ListarNoInscritos(int actividadId);
    }

    public class TipoActividadRepository : ITipoActividadRepository
    {
        public List<TipoActividad> Listar()
        {
            const string sql = "SELECT TipoId, Nombre, Descripcion FROM dbo.TipoActividad ORDER BY Nombre";
            var lista = new List<TipoActividad>();
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new TipoActividad
                        {
                            TipoId = (int)rd["TipoId"],
                            Nombre = (string)rd["Nombre"],
                            Descripcion = rd["Descripcion"] == System.DBNull.Value ? null : (string)rd["Descripcion"]
                        });
                    }
                }
            }
            return lista;
        }
    }

    public class EstudianteRepository : IEstudianteRepository
    {
        public List<Estudiante> ListarNoInscritos(int actividadId)
        {
            const string sql =
                "SELECT e.EstudianteId, e.Nombre, e.Correo, e.Programa FROM dbo.Estudiante e " +
                "WHERE e.Activo = 1 AND NOT EXISTS (" +
                "  SELECT 1 FROM dbo.Inscripcion i WHERE i.EstudianteId = e.EstudianteId AND i.ActividadId = @ActividadId) " +
                "ORDER BY e.Nombre";
            var lista = new List<Estudiante>();
            using (SqlConnection cn = Conexion.Crear())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                Conexion.Param(cmd, "@ActividadId", SqlDbType.Int, actividadId);
                cn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new Estudiante
                        {
                            EstudianteId = (int)rd["EstudianteId"],
                            Nombre = (string)rd["Nombre"],
                            Correo = (string)rd["Correo"],
                            Programa = (string)rd["Programa"]
                        });
                    }
                }
            }
            return lista;
        }
    }
}
