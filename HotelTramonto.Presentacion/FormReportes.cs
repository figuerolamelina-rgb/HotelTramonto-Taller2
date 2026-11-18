
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormReportes : Form
    {
        private readonly ReporteNegocios negocio = new ReporteNegocios();

        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private ComboBox cmbReporte;
        private Button btnConsultar;
        private Button btnCerrar;
        private DataGridView dgvReporte;
        private TabControl tabEstadisticas;
        private DataGridView dgvResumenReservas;
        private DataGridView dgvReservasPorTipo;
        private DataGridView dgvDemandaMensual;
        // Pestañas del reporte de ingresos.
        private TabControl tabIngresos;
        private DataGridView dgvResumenIngresos;
        private DataGridView dgvIngresosPorMetodo;
        private DataGridView dgvIngresosPorTipo;
        private DataGridView dgvPagosTemporales;
        private Label lblEstado;
        private Label lblTitulo;
        private Label lblSubtitulo;

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        public FormReportes()
        {
            InitializeComponent();
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            Text = "Reportes - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);

            Panel panelTitulo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = marron
            };

            lblTitulo = new Label
            {
                Text = "REPORTES Y ESTADÍSTICAS",
                Location = new Point(25, 17),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            };

            lblSubtitulo = new Label
            {
                Text = "Análisis de ocupación, ingresos y actividad del hotel",
                Location = new Point(28, 59),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 10F)
            };

            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(lblSubtitulo);

            Panel panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = marfil,
                Padding = new Padding(20, 12, 20, 10)
            };

            Label lblDesde = CrearEtiqueta("Fecha desde");
            lblDesde.Location = new Point(20, 12);

            dtpDesde = new DateTimePicker
            {
                Location = new Point(20, 38),
                Size = new Size(150, 30),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 10F),
                CalendarForeColor = marron
            };

            Label lblHasta = CrearEtiqueta("Fecha hasta");
            lblHasta.Location = new Point(190, 12);

            dtpHasta = new DateTimePicker
            {
                Location = new Point(190, 38),
                Size = new Size(150, 30),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 10F),
                CalendarForeColor = marron
            };

            Label lblTipo = CrearEtiqueta("Tipo de reporte");
            lblTipo.Location = new Point(360, 12);

            cmbReporte = new ComboBox
            {
                Location = new Point(360, 38),
                Size = new Size(270, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = marfil,
                ForeColor = marron,
                Font = new Font("Segoe UI", 10F)
            };

            cmbReporte.Items.AddRange(new object[]
            {
                "Ocupación",
                "Ingresos",
                "Estadísticas de reservas",
                "Habitaciones más utilizadas",
                "Servicios más utilizados"
            });

            cmbReporte.SelectedIndex = 0;

            btnConsultar = CrearBoton(
                "Consultar reporte", dorado, marron);

            btnConsultar.Location = new Point(650, 35);
            btnConsultar.Size = new Size(165, 38);
            btnConsultar.Click += btnConsultar_Click;

            panelFiltros.Controls.AddRange(new Control[]
            {
                lblDesde, dtpDesde,
                lblHasta, dtpHasta,
                lblTipo, cmbReporte,
                btnConsultar
            });

            dgvReporte = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = marfil,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(222, 211, 194),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.DisplayedCells,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 42,
                RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 10F),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            dgvReporte.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvReporte.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvReporte.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvReporte.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dgvReporte.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(8, 0, 8, 0);

            dgvReporte.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvReporte.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvReporte.DefaultCellStyle.BackColor = marfil;
            dgvReporte.DefaultCellStyle.ForeColor = marron;
            dgvReporte.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(226, 211, 184);
            dgvReporte.DefaultCellStyle.SelectionForeColor = marron;
            dgvReporte.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            dgvReporte.AlternatingRowsDefaultCellStyle.BackColor = crema;
            // Pestañas para el reporte de estadísticas de reservas.
            tabEstadisticas = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Visible = false
            };

            dgvResumenReservas = CrearGrillaEstadisticas();
            dgvReservasPorTipo = CrearGrillaEstadisticas();
            dgvDemandaMensual = CrearGrillaEstadisticas();

            TabPage paginaResumen = new TabPage("Resumen general");
            TabPage paginaTipos = new TabPage("Por tipo de habitación");
            TabPage paginaDemanda = new TabPage("Demanda mensual");

            paginaResumen.BackColor = marfil;
            paginaTipos.BackColor = marfil;
            paginaDemanda.BackColor = marfil;

            paginaResumen.Controls.Add(dgvResumenReservas);
            paginaTipos.Controls.Add(dgvReservasPorTipo);
            paginaDemanda.Controls.Add(dgvDemandaMensual);

            tabEstadisticas.TabPages.Add(paginaResumen);
            tabEstadisticas.TabPages.Add(paginaTipos);
            tabEstadisticas.TabPages.Add(paginaDemanda);
            // Pestañas para el reporte de ingresos.
            tabIngresos = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Visible = false
            };

            dgvResumenIngresos = CrearGrillaEstadisticas();
            dgvIngresosPorMetodo = CrearGrillaEstadisticas();
            dgvIngresosPorTipo = CrearGrillaEstadisticas();
            dgvPagosTemporales = CrearGrillaEstadisticas();

            TabPage paginaResumenIngresos =
                new TabPage("Resumen general");

            TabPage paginaMetodo =
                new TabPage("Por método de pago");

            TabPage paginaTipoPago =
                new TabPage("Por tipo de pago");

            TabPage paginaTemporales =
                new TabPage("Reservas temporales");

            paginaResumenIngresos.BackColor = marfil;
            paginaMetodo.BackColor = marfil;
            paginaTipoPago.BackColor = marfil;
            paginaTemporales.BackColor = marfil;

            paginaResumenIngresos.Controls.Add(dgvResumenIngresos);
            paginaMetodo.Controls.Add(dgvIngresosPorMetodo);
            paginaTipoPago.Controls.Add(dgvIngresosPorTipo);
            paginaTemporales.Controls.Add(dgvPagosTemporales);

            tabIngresos.TabPages.Add(paginaResumenIngresos);
            tabIngresos.TabPages.Add(paginaMetodo);
            tabIngresos.TabPages.Add(paginaTipoPago);
            tabIngresos.TabPages.Add(paginaTemporales);
            Panel panelInferior = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = marron,
                Padding = new Padding(20, 10, 20, 10)
            };

            lblEstado = new Label
            {
                Text = "Seleccioná un reporte y un rango de fechas.",
                AutoSize = false,
                Location = new Point(20, 18),
                Size = new Size(650, 28),
                ForeColor = crema,
                Font = new Font("Segoe UI", 10F)
            };

            btnCerrar = CrearBoton("Cerrar", marronClaro, Color.White);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Size = new Size(115, 38);
            btnCerrar.Location = new Point(
                panelInferior.Width - 145, 12);

            btnCerrar.Click += (sender, e) => Close();

            panelInferior.Resize += (sender, e) =>
            {
                btnCerrar.Location = new Point(
                    panelInferior.ClientSize.Width - btnCerrar.Width - 20,
                    12);
            };

            panelInferior.Controls.Add(lblEstado);
            panelInferior.Controls.Add(btnCerrar);

            Panel panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = marfil
            };

            panelContenido.Controls.Add(dgvReporte);
            panelContenido.Controls.Add(tabEstadisticas);
            panelContenido.Controls.Add(tabIngresos);

            // Orden de acoplamiento.
            Controls.Add(panelContenido);
            Controls.Add(panelInferior);
            Controls.Add(panelFiltros);
            Controls.Add(panelTitulo);

            dtpHasta.Value = DateTime.Today;
            dtpDesde.Value = DateTime.Today.AddMonths(-1);
        }
        private DataGridView CrearGrillaEstadisticas()
        {
            DataGridView grilla = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = marfil,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(222, 211, 194),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.DisplayedCells,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 42,
                RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 10F),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            grilla.ColumnHeadersDefaultCellStyle.BackColor = marron;
            grilla.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            grilla.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);

            grilla.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            grilla.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(8, 0, 8, 0);

            grilla.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            grilla.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            grilla.DefaultCellStyle.BackColor = marfil;
            grilla.DefaultCellStyle.ForeColor = marron;

            grilla.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(226, 211, 184);

            grilla.DefaultCellStyle.SelectionForeColor = marron;
            grilla.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            grilla.AlternatingRowsDefaultCellStyle.BackColor = crema;

            return grilla;
        }


        private void ConfigurarEncabezados(DataGridView grilla)
        {
            if (grilla.Columns.Count == 0)
                return;

            foreach (DataGridViewColumn columna in grilla.Columns)
            {
                string nombre = columna.Name;

                switch (nombre.ToLowerInvariant())
                {
                    case "fecha_desde":
                        columna.HeaderText = "Fecha desde";
                        break;

                    case "fecha_hasta":
                        columna.HeaderText = "Fecha hasta";
                        break;

                    case "habitaciones_activas":
                        columna.HeaderText = "Habitaciones activas";
                        break;

                    case "noches_disponibles":
                        columna.HeaderText = "Noches disponibles";
                        break;

                    case "noches_ocupadas":
                        columna.HeaderText = "Noches ocupadas";
                        break;

                    case "porcentaje_ocupacion":
                        columna.HeaderText = "Porcentaje de ocupación";
                        break;

                    default:
                        // Convertir automáticamente nombres con guiones bajos.
                        string encabezado = nombre.Replace("_", " ");

                        encabezado = System.Globalization.CultureInfo
                            .GetCultureInfo("es-AR")
                            .TextInfo.ToTitleCase(encabezado.ToLowerInvariant());

                        columna.HeaderText = encabezado;
                        break;
                }
            }
        }

        private Label CrearEtiqueta(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                ForeColor = marron,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private Button CrearBoton(
            string texto,
            Color fondo,
            Color textoColor)
        {
            Button boton = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = textoColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(174, 139, 76);

            return boton;
        }

        private void MostrarMensaje(
            string titulo,
            string detalle,
            bool esError = false)
        {
            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(440, 190);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(440, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 18),
                    Size = new Size(410, 35),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(400, 70),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnAceptar = CrearBoton(
                    "Aceptar", dorado, marron);

                btnAceptar.Location = new Point(165, 140);
                btnAceptar.Size = new Size(110, 35);
                btnAceptar.DialogResult = DialogResult.OK;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;
                mensaje.ShowDialog(this);
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;

                if (desde > hasta)
                {
                    MostrarMensaje(
                        "Rango de fechas inválido",
                        "La fecha inicial no puede ser posterior a la fecha final.");

                    return;
                }
                // Reporte de ingresos: mostrar los cuatro resultados.
                if (cmbReporte.SelectedIndex == 1)
                {
                    DataSet datos = negocio.ObtenerIngresos(desde, hasta);

                    if (datos.Tables.Count < 4)
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió los cuatro resultados esperados.");
                    }
                    dgvResumenIngresos.DataSource = datos.Tables[0];
                    dgvIngresosPorMetodo.DataSource = datos.Tables[1];
                    dgvIngresosPorTipo.DataSource = datos.Tables[2];
                    dgvPagosTemporales.DataSource = datos.Tables[3];

                    ConfigurarEncabezados(dgvResumenIngresos);
                    ConfigurarEncabezados(dgvIngresosPorMetodo);
                    ConfigurarEncabezados(dgvIngresosPorTipo);
                    ConfigurarEncabezados(dgvPagosTemporales);

                    dgvReporte.Visible = false;
                    tabEstadisticas.Visible = false;
                    tabIngresos.Visible = true;

                    tabIngresos.BringToFront();

                    lblEstado.Text =
                        $"Reporte: {cmbReporte.SelectedItem} | " +
                        $"Resumen: {datos.Tables[0].Rows.Count} | " +
                        $"Métodos: {datos.Tables[1].Rows.Count} | " +
                        $"Tipos: {datos.Tables[2].Rows.Count} | " +
                        $"Temporales: {datos.Tables[3].Rows.Count}";

                    int registros =
                        datos.Tables[0].Rows.Count +
                        datos.Tables[1].Rows.Count +
                        datos.Tables[2].Rows.Count +
                        datos.Tables[3].Rows.Count;

                    if (registros == 0)
                    {
                        MostrarMensaje(
                            "Sin resultados",
                            "El reporte no devolvió información para ese período.");
                    }

                    return;
                }
                // Estadísticas de reservas: mostrar las tres tablas.
                if (cmbReporte.SelectedIndex == 2)
                {
                    DataSet datos =
                        negocio.ObtenerEstadisticasReservas(desde, hasta);

                    if (datos.Tables.Count < 3)
                    {
                        throw new InvalidOperationException(
                            "El procedimiento no devolvió los tres resultados esperados.");
                    }

                    dgvResumenReservas.DataSource = datos.Tables[0];
                    dgvReservasPorTipo.DataSource = datos.Tables[1];
                    dgvDemandaMensual.DataSource = datos.Tables[2];

                    ConfigurarEncabezados(dgvResumenReservas);
                    ConfigurarEncabezados(dgvReservasPorTipo);
                    ConfigurarEncabezados(dgvDemandaMensual);
                    dgvReporte.Visible = false;
                    tabIngresos.Visible = false;
                    tabEstadisticas.Visible = true;
                    tabEstadisticas.BringToFront();

                    int registros =
                        datos.Tables[0].Rows.Count +
                        datos.Tables[1].Rows.Count +
                        datos.Tables[2].Rows.Count;

                    lblEstado.Text =
                        $"Reporte: {cmbReporte.SelectedItem} | " +
                        $"Resumen: {datos.Tables[0].Rows.Count} | " +
                        $"Tipos: {datos.Tables[1].Rows.Count} | " +
                        $"Meses: {datos.Tables[2].Rows.Count}";

                    if (registros == 0)
                    {
                        MostrarMensaje(
                            "Sin resultados",
                            "El reporte no devolvió información para ese período.");
                    }

                    return;
                }

                // Los demás reportes utilizan la grilla original.
                tabEstadisticas.Visible = false;
                tabIngresos.Visible = false;
                dgvReporte.Visible = true;
                dgvReporte.BringToFront();

                DataTable tabla;

                switch (cmbReporte.SelectedIndex)
                {
                    case 0:
                        tabla = negocio.ObtenerOcupacion(desde, hasta);
                        break;

                    case 1:
                        tabla = negocio.ObtenerIngresos(desde, hasta).Tables[0];
                        break;

                    case 3:
                        tabla = negocio.ObtenerHabitacionesMasUtilizadas(
                            desde, hasta);
                        break;

                    case 4:
                        tabla = negocio.ObtenerServiciosMasUtilizados(
                            desde, hasta);
                        break;

                    default:
                        MostrarMensaje(
                            "Reporte requerido",
                            "Seleccioná un tipo de reporte.");

                        return;
                }

                dgvReporte.DataSource = tabla;
                ConfigurarEncabezados(dgvReporte);

                lblEstado.Text =
                    $"Reporte: {cmbReporte.SelectedItem} | " +
                    $"Registros encontrados: {tabla.Rows.Count}";

                if (tabla.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Sin resultados",
                        "El reporte no devolvió resultados para ese período.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al generar el reporte",
                    "No se pudo generar el reporte.\n\n" + ex.Message,
                    true);
            }
        }
    }
}
