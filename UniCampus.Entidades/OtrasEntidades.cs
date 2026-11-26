using System;
using System.Collections.Generic;

namespace UniCampus.Entidades
{
    public class TipoActividad
    {
        public int TipoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class Estudiante
    {
        public int EstudianteId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Programa { get; set; }
    }

    public class Inscripcion
    {
        public int InscripcionId { get; set; }
        public int ActividadId { get; set; }
        public int EstudianteId { get; set; }
        public DateTime FechaInscripcion { get; set; }

        // Datos para mostrar (resultado del JOIN con Estudiante)
        public string EstudianteNombre { get; set; }
        public string Programa { get; set; }
    }

    /// <summary>Criterios de búsqueda del listado de actividades.</summary>
    public class FiltroActividad
    {
        public string Texto { get; set; }
        public int? TipoId { get; set; }
        public string Estado { get; set; }
        public DateTime? Fecha { get; set; }
    }

    /// <summary>Indicadores de las tarjetas superiores.</summary>
    public class EstadisticasActividad
    {
        public int Total { get; set; }
        public int EstudiantesInscritos { get; set; }
        public int Proximas { get; set; }
        public int Finalizadas { get; set; }
    }

    /// <summary>
    /// Resultado comprensible que devuelve la capa de negocio a la presentación
    /// (éxito, mensaje y lista de errores de validación).
    /// </summary>
    public class ResultadoOperacion
    {
        public ResultadoOperacion()
        {
            Errores = new List<string>();
        }

        public bool Exito { get { return Errores.Count == 0; } }
        public string Mensaje { get; set; }
        public int Id { get; set; }
        public List<string> Errores { get; private set; }

        public void AgregarError(string error)
        {
            Errores.Add(error);
        }

        public static ResultadoOperacion Ok(string mensaje, int id = 0)
        {
            return new ResultadoOperacion { Mensaje = mensaje, Id = id };
        }

        public static ResultadoOperacion Falla(string error)
        {
            var r = new ResultadoOperacion();
            r.AgregarError(error);
            return r;
        }
    }
}
