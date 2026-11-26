using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UniCampus.Entidades;
using UniCampus.Web.Presenters;
using UniCampus.Web.Views;

namespace UniCampus.Web
{
    /// <summary>
    /// Vista (MVP): solo lee/escribe controles y delega toda la lógica en el Presenter.
    /// No contiene SQL, validaciones de negocio ni acceso a datos.
    /// </summary>
    public partial class Actividades : Page, IActividadView
    {
        private ActividadPresenter _presenter;

        protected void Page_Load(object sender, EventArgs e)
        {
            _presenter = new ActividadPresenter(this);
            if (!IsPostBack)
            {
                _presenter.Inicializar();
            }
        }

        // ================================================================ eventos -> Presenter
        protected void btnGuardar_Click(object sender, EventArgs e) { _presenter.Guardar(); }

        protected void btnLimpiar_Click(object sender, EventArgs e) { _presenter.Limpiar(); }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            gvActividades.PageIndex = 0;
            _presenter.Buscar();
        }

        protected void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            gvActividades.PageIndex = 0;
            _presenter.LimpiarBusqueda();
        }

        protected void gvActividades_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvActividades.PageIndex = e.NewPageIndex;
            _presenter.Buscar();
        }

        protected void gvActividades_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) return;

            switch (e.CommandName)
            {
                case "Ver": _presenter.Ver(id); break;
                case "Editar": _presenter.Editar(id); break;
                case "Desactivar": _presenter.Desactivar(id); break;
            }
        }

        protected void gvActividades_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int id = (int)gvActividades.DataKeys[e.Row.RowIndex].Value;
                if (id == ActividadSeleccionadaId) e.Row.CssClass = "fila-seleccionada";
            }
        }

        protected void btnInscribir_Click(object sender, EventArgs e) { _presenter.Inscribir(); }

        protected void gvInscritos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (e.CommandName == "Cancelar" &&
                int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id))
            {
                _presenter.CancelarInscripcion(id);
            }
        }

        // ================================================================ IActividadView: formulario
        public int ActividadId
        {
            get { int v; return int.TryParse(hfActividadId.Value, out v) ? v : 0; }
            set
            {
                hfActividadId.Value = value.ToString(CultureInfo.InvariantCulture);
                litTituloFormulario.Text = value > 0 ? "Editar Actividad Académica" : "Nueva Actividad Académica";
            }
        }

        public string Titulo { get { return txtTitulo.Text; } set { txtTitulo.Text = value; } }
        public string Descripcion { get { return txtDescripcion.Text; } set { txtDescripcion.Text = value; } }

        public int TipoId
        {
            get { int v; return int.TryParse(ddlTipo.SelectedValue, out v) ? v : 0; }
            set { SeleccionarValor(ddlTipo, value.ToString(CultureInfo.InvariantCulture)); }
        }

        public string FechaInicio { get { return txtFechaInicio.Text; } set { txtFechaInicio.Text = value; } }
        public string FechaFin { get { return txtFechaFin.Text; } set { txtFechaFin.Text = value; } }
        public string HoraInicio { get { return txtHoraInicio.Text; } set { txtHoraInicio.Text = value; } }
        public string HoraFin { get { return txtHoraFin.Text; } set { txtHoraFin.Text = value; } }
        public string Modalidad { get { return ddlModalidad.SelectedValue; } set { SeleccionarValor(ddlModalidad, value); } }
        public string Lugar { get { return txtLugar.Text; } set { txtLugar.Text = value; } }
        public string Cupo { get { return txtCupo.Text; } set { txtCupo.Text = value; } }
        public string Estado { get { return ddlEstado.SelectedValue; } set { SeleccionarValor(ddlEstado, value); } }

        // ================================================================ IActividadView: filtros y selección
        public string FiltroTexto { get { return txtBuscar.Text; } set { txtBuscar.Text = value; } }

        public int FiltroTipoId
        {
            get { int v; return int.TryParse(ddlFiltroTipo.SelectedValue, out v) ? v : 0; }
            set { SeleccionarValor(ddlFiltroTipo, value.ToString(CultureInfo.InvariantCulture)); }
        }

        public string FiltroEstado { get { return ddlFiltroEstado.SelectedValue; } set { SeleccionarValor(ddlFiltroEstado, value); } }
        public string FiltroFecha { get { return txtFiltroFecha.Text; } set { txtFiltroFecha.Text = value; } }

        public int ActividadSeleccionadaId
        {
            get { object o = ViewState["ActividadSeleccionadaId"]; return o == null ? 0 : (int)o; }
            set { ViewState["ActividadSeleccionadaId"] = value; }
        }

        public int EstudianteSeleccionadoId
        {
            get { int v; return int.TryParse(ddlEstudiante.SelectedValue, out v) ? v : 0; }
        }

        // ================================================================ IActividadView: salidas
        public void MostrarTipos(List<TipoActividad> tipos)
        {
            ddlTipo.Items.Clear();
            ddlTipo.Items.Add(new ListItem("Seleccione...", "0"));
            ddlFiltroTipo.Items.Clear();
            ddlFiltroTipo.Items.Add(new ListItem("Todos los tipos", "0"));

            foreach (TipoActividad t in tipos)
            {
                string id = t.TipoId.ToString(CultureInfo.InvariantCulture);
                ddlTipo.Items.Add(new ListItem(t.Nombre, id));
                ddlFiltroTipo.Items.Add(new ListItem(t.Nombre, id));
            }
        }

        public void MostrarActividades(List<Actividad> actividades)
        {
            // Si tras filtrar/desactivar la página actual ya no existe, volver a la última válida.
            int paginas = actividades.Count == 0 ? 1 : (actividades.Count - 1) / gvActividades.PageSize + 1;
            if (gvActividades.PageIndex >= paginas) gvActividades.PageIndex = paginas - 1;

            gvActividades.DataSource = actividades;
            gvActividades.DataBind();

            if (actividades.Count == 0)
            {
                litResumen.Text = "Mostrando 0 registros";
            }
            else
            {
                int desde = gvActividades.PageIndex * gvActividades.PageSize + 1;
                int hasta = Math.Min(desde + gvActividades.PageSize - 1, actividades.Count);
                litResumen.Text = "Mostrando " + desde + " a " + hasta + " de " + actividades.Count + " registros";
            }
        }

        public void MostrarEstadisticas(EstadisticasActividad e)
        {
            litTotal.Text = e.Total.ToString(CultureInfo.InvariantCulture);
            litInscritosTotal.Text = e.EstudiantesInscritos.ToString(CultureInfo.InvariantCulture);
            litProximas.Text = e.Proximas.ToString(CultureInfo.InvariantCulture);
            litFinalizadas.Text = e.Finalizadas.ToString(CultureInfo.InvariantCulture);
        }

        public void MostrarDetalle(Actividad a)
        {
            pnlInscribir.Visible = a != null && (a.Estado == CatalogoActividad.EstadoActiva || a.Estado == CatalogoActividad.EstadoProxima);

            if (a == null)
            {
                const string vacio = "<p class=\"vacio\">Use el botón <strong>Ver</strong> del listado para consultar una actividad.</p>";
                litDetalle.Text = vacio;
                litVistaEstudiante.Text = vacio;
                return;
            }

            var sb = new StringBuilder();
            sb.Append("<dl class=\"detalle\">");
            Fila(sb, "Título", Enc(a.Titulo));
            Fila(sb, "Tipo", Enc(a.TipoNombre));
            Fila(sb, "Descripción", Enc(a.Descripcion));
            Fila(sb, "Fecha", Rango(a.FechaInicio.ToString("dd/MM/yyyy"), a.FechaFin.ToString("dd/MM/yyyy")));
            Fila(sb, "Hora", Rango(a.HoraInicio.ToString(@"hh\:mm"), a.HoraFin.ToString(@"hh\:mm")));
            Fila(sb, "Modalidad", Enc(CatalogoActividad.TextoModalidad(a.Modalidad)));
            Fila(sb, "Lugar", Enc(a.Lugar));
            Fila(sb, "Cupos", a.Inscritos + " inscritos de " + a.Cupo);
            Fila(sb, "Estado", "<span class=\"badge " + ClaseEstado(a.Estado) + "\">" + Enc(TextoEstado(a.Estado)) + "</span>");
            sb.Append("</dl>");
            litDetalle.Text = sb.ToString();

            string disponibilidad;
            if (a.Estado != CatalogoActividad.EstadoActiva && a.Estado != CatalogoActividad.EstadoProxima)
                disponibilidad = "<p class=\"aviso cerrada\">Inscripciones cerradas</p>";
            else if (a.CuposDisponibles == 0)
                disponibilidad = "<p class=\"aviso cerrada\">Cupo completo</p>";
            else
                disponibilidad = "<p class=\"aviso abierta\">Inscripciones abiertas</p>";

            litVistaEstudiante.Text =
                "<div class=\"vista-est\"><h3>" + Enc(a.Titulo) + "</h3>" +
                "<p><span class=\"badge b-tipo\">" + Enc(a.TipoNombre) + "</span> " +
                "<span class=\"badge " + ClaseEstado(a.Estado) + "\">" + Enc(TextoEstado(a.Estado)) + "</span></p>" +
                "<p>" + Rango(a.FechaInicio.ToString("dd/MM/yyyy"), a.FechaFin.ToString("dd/MM/yyyy")) + "</p>" +
                "<p>" + Rango(a.HoraInicio.ToString(@"hh\:mm"), a.HoraFin.ToString(@"hh\:mm")) + " &middot; " + Enc(CatalogoActividad.TextoModalidad(a.Modalidad)) + "</p>" +
                "<p>" + Enc(a.Lugar) + "</p>" +
                "<p>Cupos disponibles: " + a.CuposDisponibles + " de " + a.Cupo + "</p>" +
                "<p class=\"desc\">" + Enc(a.Descripcion) + "</p>" + disponibilidad + "</div>";
        }

        public void MostrarInscritos(List<Inscripcion> inscritos)
        {
            gvInscritos.DataSource = inscritos;
            gvInscritos.DataBind();
            litInscritosTitulo.Text = "Estudiantes Inscritos (" + inscritos.Count + ")";
        }

        public void MostrarEstudiantesDisponibles(List<Estudiante> estudiantes)
        {
            ddlEstudiante.Items.Clear();
            ddlEstudiante.Items.Add(new ListItem("Seleccione estudiante...", "0"));
            foreach (Estudiante e in estudiantes)
            {
                ddlEstudiante.Items.Add(new ListItem(e.Nombre + " - " + e.Programa,
                    e.EstudianteId.ToString(CultureInfo.InvariantCulture)));
            }
            btnInscribir.Enabled = estudiantes.Count > 0;
        }

        public void LimpiarFormulario()
        {
            ActividadId = 0;
            txtTitulo.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            SeleccionarValor(ddlTipo, "0");
            txtFechaInicio.Text = string.Empty;
            txtFechaFin.Text = string.Empty;
            txtHoraInicio.Text = string.Empty;
            txtHoraFin.Text = string.Empty;
            SeleccionarValor(ddlModalidad, "Presencial");
            txtCupo.Text = string.Empty;
            txtLugar.Text = string.Empty;
            SeleccionarValor(ddlEstado, "Activa");
        }

        public void MostrarExito(string mensaje)
        {
            pnlMensaje.CssClass = "mensaje exito";
            litMensaje.Text = Enc(mensaje);
            pnlMensaje.Visible = true;
        }

        public void MostrarError(string mensaje)
        {
            pnlMensaje.CssClass = "mensaje error";
            litMensaje.Text = Enc(mensaje);
            pnlMensaje.Visible = true;
        }

        public void MostrarErrores(IList<string> errores)
        {
            pnlMensaje.CssClass = "mensaje error";
            var sb = new StringBuilder("<strong>Revise los siguientes puntos:</strong><ul>");
            foreach (string err in errores) sb.Append("<li>").Append(Enc(err)).Append("</li>");
            sb.Append("</ul>");
            litMensaje.Text = sb.ToString();
            pnlMensaje.Visible = true;
        }

        // ================================================================ ayudas de presentación
        protected static string TextoEstado(object estado)
        {
            return CatalogoActividad.TextoEstado(Convert.ToString(estado));
        }

        protected static string ClaseEstado(object estado)
        {
            switch (Convert.ToString(estado))
            {
                case CatalogoActividad.EstadoActiva: return "b-activa";
                case CatalogoActividad.EstadoProxima: return "b-proxima";
                case CatalogoActividad.EstadoFinalizada: return "b-finalizada";
                default: return "b-inactiva";
            }
        }

        private static void SeleccionarValor(ListControl lista, string valor)
        {
            ListItem item = lista.Items.FindByValue(valor ?? string.Empty);
            lista.ClearSelection();
            if (item != null) item.Selected = true;
        }

        private static string Enc(string texto) { return HttpUtility.HtmlEncode(texto ?? string.Empty); }

        private static string Rango(string a, string b) { return a == b ? a : a + " - " + b; }

        private static void Fila(StringBuilder sb, string etiqueta, string htmlValor)
        {
            sb.Append("<dt>").Append(etiqueta).Append("</dt><dd>").Append(htmlValor).Append("</dd>");
        }
    }
}
