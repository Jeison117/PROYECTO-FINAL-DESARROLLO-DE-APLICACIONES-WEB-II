using System.Collections.Generic;
using UniCampus.Datos;
using UniCampus.Entidades;

namespace UniCampus.Negocio
{
    /// <summary>Reglas de inscripción: cupos, duplicados y estado de la actividad.</summary>
    public class InscripcionService
    {
        private readonly IInscripcionRepository _repo;
        private readonly IEstudianteRepository _estudiantes;

        public InscripcionService() : this(new InscripcionRepository(), new EstudianteRepository()) { }

        public InscripcionService(IInscripcionRepository repo, IEstudianteRepository estudiantes)
        {
            _repo = repo;
            _estudiantes = estudiantes;
        }

        public List<Inscripcion> ListarPorActividad(int actividadId)
        {
            return actividadId <= 0 ? new List<Inscripcion>() : _repo.ListarPorActividad(actividadId);
        }

        public List<Estudiante> ListarEstudiantesDisponibles(int actividadId)
        {
            return actividadId <= 0 ? new List<Estudiante>() : _estudiantes.ListarNoInscritos(actividadId);
        }

        public ResultadoOperacion Inscribir(int actividadId, int estudianteId)
        {
            if (actividadId <= 0) return ResultadoOperacion.Falla("Seleccione primero una actividad.");
            if (estudianteId <= 0) return ResultadoOperacion.Falla("Seleccione el estudiante que desea inscribir.");

            switch (_repo.Inscribir(actividadId, estudianteId))
            {
                case ResultadoInscripcion.Correcta:
                    return ResultadoOperacion.Ok("El estudiante fue inscrito correctamente.");
                case ResultadoInscripcion.ActividadNoExiste:
                    return ResultadoOperacion.Falla("La actividad no existe.");
                case ResultadoInscripcion.ActividadNoDisponible:
                    return ResultadoOperacion.Falla("La actividad no admite inscripciones (está finalizada o inactiva).");
                case ResultadoInscripcion.EstudianteNoExiste:
                    return ResultadoOperacion.Falla("El estudiante no existe o no está activo.");
                case ResultadoInscripcion.Duplicada:
                    return ResultadoOperacion.Falla("El estudiante ya está inscrito en esta actividad.");
                case ResultadoInscripcion.SinCupo:
                    return ResultadoOperacion.Falla("No hay cupos disponibles: la actividad alcanzó su capacidad.");
                default:
                    return ResultadoOperacion.Falla("No fue posible completar la inscripción.");
            }
        }

        public ResultadoOperacion Cancelar(int inscripcionId)
        {
            if (inscripcionId <= 0) return ResultadoOperacion.Falla("Seleccione una inscripción válida.");
            return _repo.Cancelar(inscripcionId)
                ? ResultadoOperacion.Ok("La inscripción fue cancelada.")
                : ResultadoOperacion.Falla("La inscripción no existe o ya fue cancelada.");
        }
    }

    public class CatalogoService
    {
        private readonly ITipoActividadRepository _tipos;

        public CatalogoService() : this(new TipoActividadRepository()) { }

        public CatalogoService(ITipoActividadRepository tipos)
        {
            _tipos = tipos;
        }

        public List<TipoActividad> ListarTipos()
        {
            return _tipos.Listar();
        }
    }
}
