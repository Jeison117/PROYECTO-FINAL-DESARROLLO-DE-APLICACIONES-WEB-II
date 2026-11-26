using System;

namespace UniCampus.Entidades
{
    /// <summary>
    /// Entidad central del sistema: una actividad académica.
    /// Es un objeto plano (DTO) que viaja entre las capas; no depende de controles visuales.
    /// </summary>
    public class Actividad
    {
        public int ActividadId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int TipoId { get; set; }
        public string TipoNombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Modalidad { get; set; }
        public string Lugar { get; set; }
        public int Cupo { get; set; }
        public string Estado { get; set; }

        /// <summary>Cantidad de estudiantes inscritos (calculada en la consulta).</summary>
        public int Inscritos { get; set; }

        public int CuposDisponibles
        {
            get { return Math.Max(0, Cupo - Inscritos); }
        }
    }

    /// <summary>Valores permitidos para los campos de lista de la actividad.</summary>
    public static class CatalogoActividad
    {
        public const string EstadoActiva = "Activa";
        public const string EstadoProxima = "Proxima";
        public const string EstadoFinalizada = "Finalizada";
        public const string EstadoInactiva = "Inactiva";

        public static readonly string[] Estados =
            { EstadoActiva, EstadoProxima, EstadoFinalizada, EstadoInactiva };

        public static readonly string[] Modalidades =
            { "Presencial", "Virtual", "Hibrida" };

        public static string TextoEstado(string estado)
        {
            return estado == EstadoProxima ? "Próxima" : estado;
        }

        public static string TextoModalidad(string modalidad)
        {
            return modalidad == "Hibrida" ? "Híbrida" : modalidad;
        }
    }
}
