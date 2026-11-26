using System.Collections.Generic;
using UniCampus.Entidades;

namespace UniCampus.Web.Views
{
    /// <summary>
    /// Contrato de la vista (MVP). El Presenter solo conoce esta interfaz,
    /// nunca la página .aspx ni sus controles.
    /// </summary>
    public interface IActividadView
    {
        // ---- Datos del formulario (texto crudo tal como lo escribe el usuario) ----
        int ActividadId { get; set; }
        string Titulo { get; set; }
        string Descripcion { get; set; }
        int TipoId { get; set; }
        string FechaInicio { get; set; }
        string FechaFin { get; set; }
        string HoraInicio { get; set; }
        string HoraFin { get; set; }
        string Modalidad { get; set; }
        string Lugar { get; set; }
        string Cupo { get; set; }
        string Estado { get; set; }

        // ---- Filtros de búsqueda ----
        string FiltroTexto { get; set; }
        int FiltroTipoId { get; set; }
        string FiltroEstado { get; set; }
        string FiltroFecha { get; set; }

        // ---- Selección actual ----
        int ActividadSeleccionadaId { get; set; }
        int EstudianteSeleccionadoId { get; }

        // ---- Salidas hacia la pantalla ----
        void MostrarTipos(List<TipoActividad> tipos);
        void MostrarActividades(List<Actividad> actividades);
        void MostrarEstadisticas(EstadisticasActividad estadisticas);
        void MostrarDetalle(Actividad actividad);
        void MostrarInscritos(List<Inscripcion> inscritos);
        void MostrarEstudiantesDisponibles(List<Estudiante> estudiantes);
        void LimpiarFormulario();
        void MostrarExito(string mensaje);
        void MostrarError(string mensaje);
        void MostrarErrores(IList<string> errores);
    }
}
