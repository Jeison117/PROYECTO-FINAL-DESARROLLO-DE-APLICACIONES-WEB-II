<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Actividades.aspx.cs" Inherits="UniCampus.Web.Actividades" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>UniCampus | Registro de Actividades Académicas</title>
    <link href="Content/site.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <header class="topbar">
        <div class="marca">
            <span class="marca-icono">&#127891;</span>
            <div>
                <strong>UniCampus</strong>
                <span>Sistema de Gestión Académica</span>
            </div>
        </div>
    </header>

    <div class="layout">
        <nav class="sidebar" aria-label="Navegación principal">
            <a class="activo" href="#inicio">Actividades Académicas</a>
            <a href="#formulario">Nueva actividad</a>
            <a href="#listado">Listado y búsqueda</a>
            <a href="#detalle">Detalle</a>
            <a href="#inscritos">Inscripciones</a>
        </nav>

        <main class="contenido" id="inicio">
            <section class="encabezado">
                <h1>Registro de Actividades Académicas</h1>
                <p>Administra, registra y consulta las actividades académicas de la institución.</p>
            </section>

            <asp:Panel ID="pnlMensaje" runat="server" Visible="false" role="alert">
                <asp:Literal ID="litMensaje" runat="server" />
            </asp:Panel>

            <!-- Indicadores -->
            <section class="tarjetas">
                <div class="tarjeta t-azul"><span class="num"><asp:Literal ID="litTotal" runat="server" Text="0" /></span><span>Total de actividades</span></div>
                <div class="tarjeta t-verde"><span class="num"><asp:Literal ID="litInscritosTotal" runat="server" Text="0" /></span><span>Estudiantes inscritos</span></div>
                <div class="tarjeta t-amarillo"><span class="num"><asp:Literal ID="litProximas" runat="server" Text="0" /></span><span>Actividades próximas</span></div>
                <div class="tarjeta t-rojo"><span class="num"><asp:Literal ID="litFinalizadas" runat="server" Text="0" /></span><span>Actividades finalizadas</span></div>
            </section>

            <div class="rejilla-dos">
                <!-- Formulario -->
                <section class="panel" id="formulario">
                    <h2><asp:Literal ID="litTituloFormulario" runat="server" Text="Nueva Actividad Académica" /></h2>
                    <asp:HiddenField ID="hfActividadId" runat="server" Value="0" />

                    <div class="form-grid">
                        <div class="campo c-ancho">
                            <label for="txtTitulo">Título de la actividad <em>*</em></label>
                            <asp:TextBox ID="txtTitulo" runat="server" MaxLength="150" />
                        </div>
                        <div class="campo">
                            <label for="ddlTipo">Tipo de actividad <em>*</em></label>
                            <asp:DropDownList ID="ddlTipo" runat="server" />
                        </div>

                        <div class="campo c-ancho">
                            <label for="txtDescripcion">Descripción <em>*</em></label>
                            <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000" />
                        </div>

                        <div class="campo">
                            <label for="txtFechaInicio">Fecha de inicio <em>*</em></label>
                            <asp:TextBox ID="txtFechaInicio" runat="server" TextMode="Date" />
                        </div>
                        <div class="campo">
                            <label for="txtFechaFin">Fecha de fin <em>*</em></label>
                            <asp:TextBox ID="txtFechaFin" runat="server" TextMode="Date" />
                        </div>
                        <div class="campo">
                            <label for="txtHoraInicio">Hora de inicio <em>*</em></label>
                            <asp:TextBox ID="txtHoraInicio" runat="server" TextMode="Time" />
                        </div>
                        <div class="campo">
                            <label for="txtHoraFin">Hora de fin <em>*</em></label>
                            <asp:TextBox ID="txtHoraFin" runat="server" TextMode="Time" />
                        </div>
                        <div class="campo">
                            <label for="ddlModalidad">Modalidad <em>*</em></label>
                            <asp:DropDownList ID="ddlModalidad" runat="server">
                                <asp:ListItem Value="Presencial" Text="Presencial" />
                                <asp:ListItem Value="Virtual" Text="Virtual" />
                                <asp:ListItem Value="Hibrida" Text="Híbrida" />
                            </asp:DropDownList>
                        </div>
                        <div class="campo">
                            <label for="txtCupo">Cupos disponibles <em>*</em></label>
                            <asp:TextBox ID="txtCupo" runat="server" TextMode="Number" />
                        </div>
                        <div class="campo">
                            <label for="txtLugar">Lugar <em>*</em></label>
                            <asp:TextBox ID="txtLugar" runat="server" MaxLength="100" />
                        </div>
                        <div class="campo">
                            <label for="ddlEstado">Estado <em>*</em></label>
                            <asp:DropDownList ID="ddlEstado" runat="server">
                                <asp:ListItem Value="Activa" Text="Activa" />
                                <asp:ListItem Value="Proxima" Text="Próxima" />
                                <asp:ListItem Value="Finalizada" Text="Finalizada" />
                                <asp:ListItem Value="Inactiva" Text="Inactiva" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="acciones">
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-primario" OnClick="btnGuardar_Click" />
                        <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" CssClass="btn btn-claro" OnClick="btnLimpiar_Click" CausesValidation="false" />
                    </div>
                </section>

                <!-- Listado -->
                <section class="panel" id="listado">
                    <h2>Listado de Actividades Académicas</h2>

                    <div class="filtros">
                        <asp:TextBox ID="txtBuscar" runat="server" placeholder="Buscar por nombre..." MaxLength="150" />
                        <asp:DropDownList ID="ddlFiltroTipo" runat="server" />
                        <asp:DropDownList ID="ddlFiltroEstado" runat="server">
                            <asp:ListItem Value="" Text="Todos los estados" />
                            <asp:ListItem Value="Activa" Text="Activa" />
                            <asp:ListItem Value="Proxima" Text="Próxima" />
                            <asp:ListItem Value="Finalizada" Text="Finalizada" />
                            <asp:ListItem Value="Inactiva" Text="Inactiva" />
                        </asp:DropDownList>
                        <asp:TextBox ID="txtFiltroFecha" runat="server" TextMode="Date" ToolTip="Actividades que ocurren en esta fecha" />
                        <asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn btn-primario" OnClick="btnBuscar_Click" CausesValidation="false" />
                        <asp:Button ID="btnLimpiarBusqueda" runat="server" Text="Quitar filtros" CssClass="btn btn-claro" OnClick="btnLimpiarBusqueda_Click" CausesValidation="false" />
                    </div>

                    <asp:GridView ID="gvActividades" runat="server" AutoGenerateColumns="false" CssClass="tabla"
                        DataKeyNames="ActividadId" AllowPaging="true" PageSize="5" GridLines="None"
                        OnRowCommand="gvActividades_RowCommand" OnPageIndexChanging="gvActividades_PageIndexChanging"
                        OnRowDataBound="gvActividades_RowDataBound" EmptyDataText="No hay actividades que coincidan con la búsqueda.">
                        <PagerStyle CssClass="paginador" />
                        <Columns>
                            <asp:BoundField DataField="ActividadId" HeaderText="#" />
                            <asp:BoundField DataField="Titulo" HeaderText="Título" />
                            <asp:BoundField DataField="TipoNombre" HeaderText="Tipo" />
                            <asp:BoundField DataField="FechaInicio" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" HtmlEncode="false" />
                            <asp:TemplateField HeaderText="Cupos">
                                <ItemTemplate><%# Eval("Inscritos") %>/<%# Eval("Cupo") %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate><span class='badge <%# ClaseEstado(Eval("Estado")) %>'><%# TextoEstado(Eval("Estado")) %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" CommandName="Ver" CommandArgument='<%# Eval("ActividadId") %>' CssClass="icono ver" ToolTip="Ver detalle" CausesValidation="false">Ver</asp:LinkButton>
                                    <asp:LinkButton runat="server" CommandName="Editar" CommandArgument='<%# Eval("ActividadId") %>' CssClass="icono editar" ToolTip="Editar" CausesValidation="false">Editar</asp:LinkButton>
                                    <asp:LinkButton runat="server" CommandName="Desactivar" CommandArgument='<%# Eval("ActividadId") %>' CssClass="icono borrar" ToolTip="Desactivar"
                                        OnClientClick="return confirm('¿Desea desactivar esta actividad?');" CausesValidation="false">Desactivar</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <p class="resumen"><asp:Literal ID="litResumen" runat="server" /></p>
                </section>
            </div>

            <div class="rejilla-tres">
                <!-- Detalle -->
                <section class="panel" id="detalle">
                    <h2>Detalles de la Actividad</h2>
                    <asp:Literal ID="litDetalle" runat="server" />
                </section>

                <!-- Inscritos -->
                <section class="panel" id="inscritos">
                    <h2><asp:Literal ID="litInscritosTitulo" runat="server" Text="Estudiantes Inscritos (0)" /></h2>

                    <asp:Panel ID="pnlInscribir" runat="server" CssClass="inscribir" Visible="false">
                        <asp:DropDownList ID="ddlEstudiante" runat="server" />
                        <asp:Button ID="btnInscribir" runat="server" Text="Inscribir estudiante" CssClass="btn btn-verde" OnClick="btnInscribir_Click" CausesValidation="false" />
                    </asp:Panel>

                    <asp:GridView ID="gvInscritos" runat="server" AutoGenerateColumns="false" CssClass="tabla"
                        DataKeyNames="InscripcionId" GridLines="None" OnRowCommand="gvInscritos_RowCommand"
                        EmptyDataText="Aún no hay estudiantes inscritos.">
                        <Columns>
                            <asp:BoundField DataField="EstudianteNombre" HeaderText="Nombre" />
                            <asp:BoundField DataField="Programa" HeaderText="Programa" />
                            <asp:BoundField DataField="FechaInscripcion" HeaderText="Fecha inscripción" DataFormatString="{0:dd/MM/yyyy}" HtmlEncode="false" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" CommandName="Cancelar" CommandArgument='<%# Eval("InscripcionId") %>' CssClass="icono borrar"
                                        OnClientClick="return confirm('¿Cancelar esta inscripción?');" CausesValidation="false">Quitar</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </section>

                <!-- Vista del estudiante -->
                <section class="panel" id="vista-estudiante">
                    <h2>Vista de la Actividad (como la ve el estudiante)</h2>
                    <asp:Literal ID="litVistaEstudiante" runat="server" />
                </section>
            </div>
        </main>
    </div>
</form>
</body>
</html>
