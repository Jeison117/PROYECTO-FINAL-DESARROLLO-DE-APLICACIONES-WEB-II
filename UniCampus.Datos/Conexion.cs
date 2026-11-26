using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UniCampus.Datos
{
    /// <summary>
    /// Centraliza la apertura de conexiones y utilidades ADO.NET.
    /// La cadena de conexión se lee de la configuración de la aplicación host (Web.config);
    /// nunca se escribe aquí ni se versiona con credenciales.
    /// </summary>
    public static class Conexion
    {
        public const string NombreCadena = "UniCampusDB";

        public static string ObtenerCadena()
        {
            ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings[NombreCadena];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                throw new InvalidOperationException(
                    "No se encontró la cadena de conexión '" + NombreCadena + "' en la configuración.");
            }
            return cs.ConnectionString;
        }

        public static SqlConnection Crear()
        {
            return new SqlConnection(ObtenerCadena());
        }

        /// <summary>Agrega un parámetro tipado; null se convierte en DBNull.</summary>
        public static SqlParameter Param(SqlCommand cmd, string nombre, SqlDbType tipo, object valor, int tamano = 0)
        {
            SqlParameter p = tamano > 0
                ? new SqlParameter(nombre, tipo, tamano)
                : new SqlParameter(nombre, tipo);
            p.Value = valor ?? DBNull.Value;
            cmd.Parameters.Add(p);
            return p;
        }
    }
}
