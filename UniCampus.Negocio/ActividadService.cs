using System;
using System.Collections.Generic;
using UniCampus.Datos;
using UniCampus.Entidades;

namespace UniCampus.Negocio
{
    /// <summary>
    /// Reglas de negocio de las actividades académicas: validaciones de campos,
    /// coherencia de fechas/horas, cupos y estados. No conoce controles visuales ni SQL.
    /// </summary>
    public class ActividadService
    {
        public const int MaxTitulo = 150;
        public const int MaxDescripcion = 1000;
        public const int MaxLugar = 100;
        public const int CupoMaximo = 10000;

        private readonly IActividadRepository _repo;
        private readonly ITipoActividadRepository _tipos;

        public ActividadService() : this(new ActividadRepository(), new TipoActividadRepository()) { }

        public ActividadService(IActividadRepository repo, ITipoActividadRepository tipos)
        {
            _repo = repo;
            _tipos = tipos;
        }

        public List<Actividad> Listar(FiltroActividad filtro)
        {
            return _repo.Listar(filtro);
        }

        public Actividad ObtenerPorId(int actividadId)
        {
            return actividadId <= 0 ? null : _repo.ObtenerPorId(actividadId);
        }

        public EstadisticasActividad ObtenerEstadisticas()
        {
            return _repo.ObtenerEstadisticas();
        }

        /// <summary>Valida y devuelve la lista de errores (vacía si todo es correcto).</summary>
        public List<string> Validar(Actividad a)
        {
            var errores = new List<string>();
            if (a == null) { errores.Add("No se recibieron los datos de la actividad."); return errores; }

            string titulo = (a.Titulo ?? string.Empty).Trim();
            string descripcion = (a.Descripcion ?? string.Empty).Trim();
            string lugar = (a.Lugar ?? string.Empty).Trim();

            if (titulo.Length == 0) errores.Add("El título de la actividad es obligatorio.");
            else if (titulo.Length > MaxTitulo) errores.Add("El título no puede superar " + MaxTitulo + " caracteres.");

            if (descripcion.Length == 0) errores.Add("La descripción es obligatoria.");
            else if (descripcion.Length > MaxDescripcion) errores.Add("La descripción no puede superar " + MaxDescripcion + " caracteres.");

            if (a.TipoId <= 0) errores.Add("Seleccione el tipo de actividad.");

            if (a.FechaInicio == default(DateTime)) errores.Add("La fecha de inicio es obligatoria.");
            if (a.FechaFin == default(DateTime)) errores.Add("La fecha de fin es obligatoria.");
            if (a.FechaInicio != default(DateTime) && a.FechaFin != default(DateTime) && a.FechaFin.Date < a.FechaInicio.Date)
                errores.Add("La fecha de fin no puede ser anterior a la fecha de inicio.");

            if (a.HoraInicio < TimeSpan.Zero || a.HoraInicio >= TimeSpan.FromDays(1)) errores.Add("La hora de inicio no es válida.");
            if (a.HoraFin < TimeSpan.Zero || a.HoraFin >= TimeSpan.FromDays(1)) errores.Add("La hora de fin no es válida.");
            if (a.HoraFin <= a.HoraInicio) errores.Add("La hora de fin debe ser posterior a la hora de inicio.");

            if (Array.IndexOf(CatalogoActividad.Modalidades, a.Modalidad) < 0) errores.Add("La modalidad seleccionada no es válida.");

            if (lugar.Length == 0) errores.Add("El lugar es obligatorio.");
            else if (lugar.Length > MaxLugar) errores.Add("El lugar no puede superar " + MaxLugar + " caracteres.");

            if (a.Cupo <= 0) errores.Add("El cupo debe ser un número mayor que cero.");
            else if (a.Cupo > CupoMaximo) errores.Add("El cupo no puede superar " + CupoMaximo + " personas.");

            if (Array.IndexOf(CatalogoActividad.Estados, a.Estado) < 0) errores.Add("El estado seleccionado no es válido.");

            return errores;
        }

        /// <summary>Registra una actividad nueva (ActividadId = 0) o actualiza una existente.</summary>
        public ResultadoOperacion Guardar(Actividad a)
        {
            var errores = Validar(a);
            var resultado = new ResultadoOperacion();
            foreach (string e in errores) resultado.AgregarError(e);
            if (!resultado.Exito) return resultado;

            // Normalizar texto
            a.Titulo = a.Titulo.Trim();
            a.Descripcion = a.Descripcion.Trim();
            a.Lugar = a.Lugar.Trim();

            // El tipo debe existir en el catálogo
            bool tipoExiste = false;
            foreach (TipoActividad t in _tipos.Listar())
            {
                if (t.TipoId == a.TipoId) { tipoExiste = true; break; }
            }
            if (!tipoExiste) return ResultadoOperacion.Falla("El tipo de actividad seleccionado no existe.");

            if (_repo.ExisteDuplicado(a.Titulo, a.FechaInicio, a.ActividadId))
                return ResultadoOperacion.Falla("Ya existe una actividad con el mismo título y fecha de inicio.");

            if (a.ActividadId <= 0)
            {
                int id = _repo.Insertar(a);
                return ResultadoOperacion.Ok("La actividad se registró correctamente.", id);
            }

            Actividad actual = _repo.ObtenerPorId(a.ActividadId);
            if (actual == null) return ResultadoOperacion.Falla("La actividad que intenta actualizar no existe.");

            if (a.Cupo < actual.Inscritos)
                return ResultadoOperacion.Falla("El cupo no puede ser menor que los estudiantes ya inscritos (" + actual.Inscritos + ").");

            _repo.Actualizar(a);
            return ResultadoOperacion.Ok("La actividad se actualizó correctamente.", a.ActividadId);
        }

        /// <summary>Eliminación lógica: la actividad queda en estado Inactiva.</summary>
        public ResultadoOperacion Desactivar(int actividadId)
        {
            if (actividadId <= 0) return ResultadoOperacion.Falla("Seleccione una actividad válida.");
            Actividad a = _repo.ObtenerPorId(actividadId);
            if (a == null) return ResultadoOperacion.Falla("La actividad no existe.");
            if (a.Estado == CatalogoActividad.EstadoInactiva)
                return ResultadoOperacion.Falla("La actividad ya se encuentra desactivada.");

            _repo.Desactivar(actividadId);
            return ResultadoOperacion.Ok("La actividad se desactivó correctamente.", actividadId);
        }
    }
}
