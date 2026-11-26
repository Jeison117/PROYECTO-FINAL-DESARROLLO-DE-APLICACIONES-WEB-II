using System;
using System.Collections.Generic;
using System.Globalization;
using UniCampus.Entidades;
using UniCampus.Negocio;
using UniCampus.Web.Views;

namespace UniCampus.Web.Presenters
{
    /// <summary>
    /// Presenter (MVP): recoge los datos de la vista, los convierte a entidades,
    /// llama a los servicios de negocio y le indica a la vista qué mostrar.
    /// No contiene SQL ni referencias a controles de ASP.NET.
    /// </summary>
    public class ActividadPresenter
    {
        private static readonly string[] FormatosFecha = { "yyyy-MM-dd", "dd/MM/yyyy" };
        private static readonly string[] FormatosHora = { @"hh\:mm", @"hh\:mm\:ss", @"h\:mm" };

        private readonly IActividadView _vista;
        private readonly ActividadService _actividades;
        private readonly InscripcionService _inscripciones;
        private readonly CatalogoService _catalogo;

        public ActividadPresenter(IActividadView vista)
            : this(vista, new ActividadService(), new InscripcionService(), new CatalogoService()) { }

        public ActividadPresenter(IActividadView vista, ActividadService actividades,
                                  InscripcionService inscripciones, CatalogoService catalogo)
        {
            if (vista == null) throw new ArgumentNullException("vista");
            _vista = vista;
            _actividades = actividades;
            _inscripciones = inscripciones;
            _catalogo = catalogo;
        }

        // ------------------------------------------------------------------ carga inicial
        public void Inicializar()
        {
            Ejecutar(delegate
            {
                _vista.MostrarTipos(_catalogo.ListarTipos());
                _vista.LimpiarFormulario();
                RefrescarTodo();
            });
        }

        // ------------------------------------------------------------------ guardar / limpiar
        public void Guardar()
        {
            Ejecutar(delegate
            {
                Actividad actividad;
                var erroresFormato = new List<string>();
                if (!TryConstruirActividad(out actividad, erroresFormato))
                {
                    _vista.MostrarErrores(erroresFormato);
                    return;
                }

                ResultadoOperacion resultado = _actividades.Guardar(actividad);
                if (!resultado.Exito)
                {
                    _vista.MostrarErrores(resultado.Errores);
                    return;
                }

                _vista.ActividadSeleccionadaId = resultado.Id;
                _vista.LimpiarFormulario();
                _vista.MostrarExito(resultado.Mensaje);
                RefrescarTodo();
            });
        }

        public void Limpiar()
        {
            Ejecutar(delegate { _vista.LimpiarFormulario(); });
        }

        // ------------------------------------------------------------------ listado / búsqueda
        public void Buscar()
        {
            Ejecutar(delegate { RefrescarListado(); });
        }

        public void LimpiarBusqueda()
        {
            Ejecutar(delegate
            {
                _vista.FiltroTexto = string.Empty;
                _vista.FiltroTipoId = 0;
                _vista.FiltroEstado = string.Empty;
                _vista.FiltroFecha = string.Empty;
                RefrescarListado();
            });
        }

        // ------------------------------------------------------------------ detalle / edición / desactivar
        public void Ver(int actividadId)
        {
            Ejecutar(delegate
            {
                Actividad a = _actividades.ObtenerPorId(actividadId);
                if (a == null) { _vista.MostrarError("La actividad solicitada no existe."); return; }
                _vista.ActividadSeleccionadaId = a.ActividadId;
                RefrescarDetalle();
            });
        }

        public void Editar(int actividadId)
        {
            Ejecutar(delegate
            {
                Actividad a = _actividades.ObtenerPorId(actividadId);
                if (a == null) { _vista.MostrarError("La actividad solicitada no existe."); return; }

                _vista.ActividadId = a.ActividadId;
                _vista.Titulo = a.Titulo;
                _vista.Descripcion = a.Descripcion;
                _vista.TipoId = a.TipoId;
                _vista.FechaInicio = a.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                _vista.FechaFin = a.FechaFin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                _vista.HoraInicio = a.HoraInicio.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
                _vista.HoraFin = a.HoraFin.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
                _vista.Modalidad = a.Modalidad;
                _vista.Lugar = a.Lugar;
                _vista.Cupo = a.Cupo.ToString(CultureInfo.InvariantCulture);
                _vista.Estado = a.Estado;

                _vista.ActividadSeleccionadaId = a.ActividadId;
                RefrescarDetalle();
            });
        }

        public void Desactivar(int actividadId)
        {
            Ejecutar(delegate
            {
                ResultadoOperacion r = _actividades.Desactivar(actividadId);
                if (!r.Exito) { _vista.MostrarErrores(r.Errores); return; }

                if (_vista.ActividadId == actividadId) _vista.LimpiarFormulario();
                _vista.MostrarExito(r.Mensaje);
                RefrescarTodo();
            });
        }

        // ------------------------------------------------------------------ inscripciones
        public void Inscribir()
        {
            Ejecutar(delegate
            {
                ResultadoOperacion r = _inscripciones.Inscribir(_vista.ActividadSeleccionadaId, _vista.EstudianteSeleccionadoId);
                if (!r.Exito) { _vista.MostrarErrores(r.Errores); RefrescarDetalle(); return; }

                _vista.MostrarExito(r.Mensaje);
                RefrescarTodo();
            });
        }

        public void CancelarInscripcion(int inscripcionId)
        {
            Ejecutar(delegate
            {
                ResultadoOperacion r = _inscripciones.Cancelar(inscripcionId);
                if (!r.Exito) { _vista.MostrarErrores(r.Errores); return; }

                _vista.MostrarExito(r.Mensaje);
                RefrescarTodo();
            });
        }

        // ------------------------------------------------------------------ utilidades privadas
        private void RefrescarTodo()
        {
            RefrescarListado();
            _vista.MostrarEstadisticas(_actividades.ObtenerEstadisticas());
            RefrescarDetalle();
        }

        private void RefrescarListado()
        {
            var filtro = new FiltroActividad
            {
                Texto = _vista.FiltroTexto,
                TipoId = _vista.FiltroTipoId > 0 ? (int?)_vista.FiltroTipoId : null,
                Estado = _vista.FiltroEstado
            };

            DateTime fecha;
            if (TryParseFecha(_vista.FiltroFecha, out fecha)) filtro.Fecha = fecha;

            _vista.MostrarActividades(_actividades.Listar(filtro));
        }

        private void RefrescarDetalle()
        {
            int id = _vista.ActividadSeleccionadaId;
            Actividad a = id > 0 ? _actividades.ObtenerPorId(id) : null;

            _vista.MostrarDetalle(a);
            if (a == null)
            {
                _vista.MostrarInscritos(new List<Inscripcion>());
                _vista.MostrarEstudiantesDisponibles(new List<Estudiante>());
                return;
            }
            _vista.MostrarInscritos(_inscripciones.ListarPorActividad(id));
            _vista.MostrarEstudiantesDisponibles(_inscripciones.ListarEstudiantesDisponibles(id));
        }

        /// <summary>
        /// Convierte el texto del formulario en una entidad. Solo detecta errores de formato
        /// (fecha/hora/número ilegibles); las reglas de negocio las valida el servicio.
        /// </summary>
        private bool TryConstruirActividad(out Actividad actividad, List<string> errores)
        {
            actividad = new Actividad
            {
                ActividadId = _vista.ActividadId,
                Titulo = _vista.Titulo,
                Descripcion = _vista.Descripcion,
                TipoId = _vista.TipoId,
                Modalidad = _vista.Modalidad,
                Lugar = _vista.Lugar,
                Estado = _vista.Estado
            };

            DateTime fi, ff;
            TimeSpan hi, hf;
            int cupo;

            if (!TryParseFecha(_vista.FechaInicio, out fi)) errores.Add("La fecha de inicio es obligatoria y debe tener un formato válido.");
            if (!TryParseFecha(_vista.FechaFin, out ff)) errores.Add("La fecha de fin es obligatoria y debe tener un formato válido.");
            if (!TryParseHora(_vista.HoraInicio, out hi)) errores.Add("La hora de inicio es obligatoria y debe tener un formato válido (HH:mm).");
            if (!TryParseHora(_vista.HoraFin, out hf)) errores.Add("La hora de fin es obligatoria y debe tener un formato válido (HH:mm).");
            if (!int.TryParse((_vista.Cupo ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out cupo))
                errores.Add("El cupo debe ser un número entero.");

            if (errores.Count > 0) return false;

            actividad.FechaInicio = fi;
            actividad.FechaFin = ff;
            actividad.HoraInicio = hi;
            actividad.HoraFin = hf;
            actividad.Cupo = cupo;
            return true;
        }

        private static bool TryParseFecha(string texto, out DateTime fecha)
        {
            return DateTime.TryParseExact((texto ?? string.Empty).Trim(), FormatosFecha,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }

        private static bool TryParseHora(string texto, out TimeSpan hora)
        {
            return TimeSpan.TryParseExact((texto ?? string.Empty).Trim(), FormatosHora,
                CultureInfo.InvariantCulture, out hora);
        }

        /// <summary>Ejecuta una acción y convierte cualquier fallo inesperado en un mensaje controlado.</summary>
        private void Ejecutar(Action accion)
        {
            try
            {
                accion();
            }
            catch (InvalidOperationException ex)
            {
                _vista.MostrarError("Error de configuración o de conexión: " + ex.Message);
            }
            catch (Exception)
            {
                // El detalle técnico no se muestra al usuario (evita filtrar información interna).
                _vista.MostrarError("No fue posible completar la operación. Verifique la conexión con la base de datos e intente de nuevo.");
            }
        }
    }
}
