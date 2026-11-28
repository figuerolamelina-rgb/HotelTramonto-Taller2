
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using ScottPlot.WinForms;

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
        private TabControl tabReporteSimple;
        private TabPage paginaDatosReporte;
        private TabPage paginaGraficoReporte;
        private FormsPlot plotReporte;
        private TabControl tabEstadisticas;
        private DataGridView dgvResumenReservas;
        private DataGridView dgvReservasPorTipo;
        private DataGridView dgvDemandaMensual;
        private TabPage paginaGraficoDemanda;
        private FormsPlot plotDemanda;
        private TabPage paginaGraficoReservas;
        private FormsPlot plotReservas;
        // Pestañas del reporte de ingresos.
        private TabControl tabIngresos;
        private DataGridView dgvResumenIngresos;
        private DataGridView dgvIngresosPorMetodo;
        private DataGridView dgvIngresosPorTipo;
        private DataGridView dgvPagosTemporales;
        private FormsPlot plotIngresos;
        private TabPage paginaGraficoIngresos;
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

            // Pestañas para los reportes simples: datos y gráficos.
            tabReporteSimple = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Visible = true
            };

            paginaDatosReporte = new TabPage("Datos");
            paginaGraficoReporte = new TabPage("Gráfico");

            paginaDatosReporte.BackColor = marfil;
            paginaGraficoReporte.BackColor = marfil;

            plotReporte = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            paginaDatosReporte.Controls.Add(dgvReporte);
            paginaGraficoReporte.Controls.Add(plotReporte);

            tabReporteSimple.TabPages.Add(paginaDatosReporte);
            tabReporteSimple.TabPages.Add(paginaGraficoReporte);

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

            // Pestaña con gráfico de reservas por estado.
            paginaGraficoReservas = new TabPage("Reservas por estado");
            paginaGraficoReservas.BackColor = marfil;

            plotReservas = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            paginaGraficoReservas.Controls.Add(plotReservas);

            tabEstadisticas.TabPages.Add(paginaGraficoReservas);

            // Pestaña de evolución mensual de reservas.
            paginaGraficoDemanda = new TabPage("Demanda mensual - Gráfico");
            paginaGraficoDemanda.BackColor = marfil;

            plotDemanda = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            paginaGraficoDemanda.Controls.Add(plotDemanda);

            tabEstadisticas.TabPages.Add(paginaGraficoDemanda);

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

            paginaGraficoIngresos = new TabPage("Gráfico de ingresos");
            paginaGraficoIngresos.BackColor = marfil;

            plotIngresos = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            paginaGraficoIngresos.Controls.Add(plotIngresos);

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
            tabIngresos.TabPages.Add(paginaGraficoIngresos);

            // Panel inferior.
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

            panelContenido.Controls.Add(tabReporteSimple);
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

        private void AplicarEstiloGrafico(FormsPlot control)
        {
            var fondo = ScottPlot.Color.FromColor(marfil);
            var texto = ScottPlot.Color.FromColor(marron);
            var grilla = ScottPlot.Color.FromColor(
                Color.FromArgb(222, 211, 194));

            control.Plot.FigureBackground.Color = fondo;
            control.Plot.DataBackground.Color = fondo;

            control.Plot.Axes.Color(texto);
            control.Plot.Grid.MajorLineColor = grilla;

            control.Refresh();
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


        private void MostrarGraficoOcupacion(DataTable tabla)
        {
            plotReporte.Plot.Clear();

            if (tabla.Rows.Count == 0)
                return;

            DataRow fila = tabla.Rows[0];

            double nochesDisponibles =
                Convert.ToDouble(fila["noches_disponibles"]);

            double nochesOcupadas =
                Convert.ToDouble(fila["noches_ocupadas"]);

            double nochesLibres =
                Math.Max(0, nochesDisponibles - nochesOcupadas);

            double porcentaje =
                Convert.ToDouble(fila["porcentaje_ocupacion"]);

            if (nochesDisponibles > 0)
            {
                var porciones = new List<ScottPlot.PieSlice>
        {
            new ScottPlot.PieSlice
            {
                Value = nochesOcupadas,
                FillColor = ScottPlot.Color.FromColor(dorado),
                Label = $"Ocupadas: {nochesOcupadas:N0}",
                LegendText = "Noches ocupadas"
            },
            new ScottPlot.PieSlice
            {
                Value = nochesLibres,
                FillColor = ScottPlot.Color.FromColor(crema),
                Label = $"Disponibles: {nochesLibres:N0}",
                LegendText = "Noches disponibles"
            }
        };

                var grafico = plotReporte.Plot.Add.Pie(porciones);

                grafico.DonutFraction = 0.45;
                grafico.SliceLabelDistance = 1.2;

                plotReporte.Plot.Axes.AutoScale();
                plotReporte.Plot.ShowLegend();

                // Porcentaje en el centro de la dona, sin recuadro.
                var textoPorcentaje = plotReporte.Plot.Add.Text(
                    $"{porcentaje:N2}%",
                    0,
                    0
                );

                textoPorcentaje.LabelFontSize = 20;
                textoPorcentaje.LabelBold = true;
                textoPorcentaje.LabelFontColor =
                    ScottPlot.Color.FromColor(marron);
                textoPorcentaje.Alignment =
                    ScottPlot.Alignment.MiddleCenter;
            }

            plotReporte.Plot.Axes.Frameless();
            plotReporte.Plot.HideGrid();

            plotReporte.Plot.Title(
                $"Ocupación del hotel: {porcentaje:N2}%");

            AplicarEstiloGrafico(plotReporte);
        }


        private void MostrarGraficoIngresos(DataTable tabla)
        {
            plotIngresos.Plot.Clear();

            if (tabla.Rows.Count == 0)
            {
                plotIngresos.Plot.Title("No hay ingresos por método de pago");
                plotIngresos.Refresh();
                return;
            }

            int cantidad = tabla.Rows.Count;

            double[] valores = new double[cantidad];
            double[] posiciones = new double[cantidad];
            string[] etiquetas = new string[cantidad];

            for (int i = 0; i < cantidad; i++)
            {
                DataRow fila = tabla.Rows[i];

                posiciones[i] = i;
                etiquetas[i] = fila["metodo_pago"].ToString() ?? "";

                valores[i] = Convert.ToDouble(fila["total_ingresos"]);
            }

            var barras = plotIngresos.Plot.Add.Bars(valores);
            barras.Color = ScottPlot.Color.FromColor(dorado);
            barras.Horizontal = true;

            for (int i = 0; i < cantidad; i++)
            {
                barras.Bars[i].Position = posiciones[i];

                barras.Bars[i].Label =
                    valores[i].ToString("C2",
                        System.Globalization.CultureInfo.GetCultureInfo("es-AR"));
            }

            barras.ValueLabelStyle.FontSize = 12;
            barras.ValueLabelStyle.Bold = true;

            plotIngresos.Plot.Axes.Left.SetTicks(
                posiciones,
                etiquetas);

            plotIngresos.Plot.Axes.Margins(left: 0, right: 0.2);

            plotIngresos.Plot.Title("Ingresos por método de pago");
            plotIngresos.Plot.XLabel("Importe recaudado");
            plotIngresos.Plot.HideGrid();

            AplicarEstiloGrafico(plotIngresos);
        }

        private void MostrarGraficoReservas(DataTable tabla)
        {
            plotReservas.Plot.Clear();

            if (tabla.Rows.Count == 0)
            {
                plotReservas.Plot.Title("No hay reservas para el período seleccionado");
                plotReservas.Refresh();
                return;
            }

            DataRow fila = tabla.Rows[0];

            string[] etiquetas =
            {
        "Confirmadas",
        "En Check-in",
        "Finalizadas",
        "Canceladas"
    };

            string[] columnas =
            {
        "confirmadas",
        "en_checkin",
        "finalizadas",
        "canceladas"
    };

            double[] valores = new double[columnas.Length];
            double[] posiciones = { 0, 1, 2, 3 };

            for (int i = 0; i < columnas.Length; i++)
            {
                valores[i] = fila[columnas[i]] == DBNull.Value
                    ? 0
                    : Convert.ToDouble(fila[columnas[i]]);
            }

            var barras = plotReservas.Plot.Add.Bars(valores);
            barras.Color = ScottPlot.Color.FromColor(dorado);
            barras.Horizontal = true;

            for (int i = 0; i < valores.Length; i++)
            {
                barras.Bars[i].Position = posiciones[i];
                barras.Bars[i].Label = valores[i].ToString("N0");
            }

            barras.ValueLabelStyle.FontSize = 13;
            barras.ValueLabelStyle.Bold = true;

            plotReservas.Plot.Axes.Left.SetTicks(posiciones, etiquetas);

            plotReservas.Plot.Axes.Margins(left: 0, right: 0.2);

            plotReservas.Plot.Title("Cantidad de reservas por estado");
            plotReservas.Plot.XLabel("Cantidad de reservas");
            plotReservas.Plot.HideGrid();

            AplicarEstiloGrafico(plotReservas);
        }

        private void MostrarGraficoDemanda(DataTable tabla)
        {
            plotDemanda.Plot.Clear();

            if (tabla.Rows.Count == 0)
            {
                plotDemanda.Plot.Title("No hay reservas para el período seleccionado");
                plotDemanda.Refresh();
                return;
            }

            int cantidad = tabla.Rows.Count;

            double[] posiciones = new double[cantidad];
            double[] valores = new double[cantidad];
            string[] etiquetas = new string[cantidad];

            var cultura = System.Globalization.CultureInfo.GetCultureInfo("es-AR");

            for (int i = 0; i < cantidad; i++)
            {
                DataRow fila = tabla.Rows[i];

                int anio = Convert.ToInt32(fila["anio"]);
                int mes = Convert.ToInt32(fila["mes"]);

                posiciones[i] = i;
                valores[i] = Convert.ToDouble(fila["cantidad_reservas"]);

                etiquetas[i] = new DateTime(anio, mes, 1)
                    .ToString("MMM yyyy", cultura);
            }

            var linea = plotDemanda.Plot.Add.Scatter(posiciones, valores);

            linea.Color = ScottPlot.Color.FromColor(dorado);
            linea.LineWidth = 3;
            linea.MarkerSize = 9;

            plotDemanda.Plot.Axes.Bottom.SetTicks(posiciones, etiquetas);

            plotDemanda.Plot.Axes.Margins(left: 0.05, right: 0.05);

            plotDemanda.Plot.Title("Evolución mensual de reservas");
            plotDemanda.Plot.XLabel("Mes");
            plotDemanda.Plot.YLabel("Cantidad de reservas");

            plotDemanda.Plot.HideGrid();

            AplicarEstiloGrafico(plotDemanda);
        }

        private void MostrarGraficoHabitaciones(DataTable tabla)
        {
            plotReporte.Plot.Clear();
            plotReporte.Plot.Axes.Left.IsVisible = true;
            plotReporte.Plot.Axes.Bottom.IsVisible = true;

            if (tabla.Rows.Count == 0)
            {
                plotReporte.Plot.Title("No hay habitaciones para mostrar");
                plotReporte.Refresh();
                return;
            }

            int cantidad = tabla.Rows.Count;

            double[] posiciones = new double[cantidad];
            double[] valores = new double[cantidad];
            string[] etiquetas = new string[cantidad];

            for (int i = 0; i < cantidad; i++)
            {
                DataRow fila = tabla.Rows[i];

                posiciones[i] = i;

                etiquetas[i] =
                    "Hab. " + fila["numero_habitacion"].ToString();

                valores[i] =
                    Convert.ToDouble(fila["noches_ocupadas"]);
            }

            var barras = plotReporte.Plot.Add.Bars(valores);
            barras.Color = ScottPlot.Color.FromColor(dorado);
            barras.Horizontal = true;

            for (int i = 0; i < cantidad; i++)
            {
                barras.Bars[i].Position = posiciones[i];

                barras.Bars[i].Label =
                    valores[i].ToString("N0") + " noches";
            }

            barras.ValueLabelStyle.FontSize = 12;
            barras.ValueLabelStyle.Bold = true;

            plotReporte.Plot.Axes.Left.SetTicks(
            posiciones,
            etiquetas);

            // El eje horizontal comienza siempre en cero.
            double maximo = valores.Max();

            if (maximo <= 0)
                maximo = 1;

            plotReporte.Plot.Axes.SetLimits(
             left: 0,
             right: maximo * 1.20,
             bottom: -0.5,
             top: cantidad - 0.5);

            plotReporte.Plot.Title("Habitaciones más utilizadas");
            plotReporte.Plot.XLabel("Cantidad de noches ocupadas");

            plotReporte.Plot.HideGrid();

            AplicarEstiloGrafico(plotReporte);
        }

        private void MostrarGraficoServicios(DataTable tabla)
        {
            plotReporte.Plot.Clear();
            plotReporte.Plot.Axes.Left.IsVisible = true;
            plotReporte.Plot.Axes.Bottom.IsVisible = true;

            if (tabla.Rows.Count == 0)
            {
                plotReporte.Plot.Title("No hay servicios registrados");
                plotReporte.Refresh();
                return;
            }

            int cantidad = tabla.Rows.Count;

            double[] posiciones = new double[cantidad];
            double[] valores = new double[cantidad];
            string[] etiquetas = new string[cantidad];

            var cultura = System.Globalization.CultureInfo
                .GetCultureInfo("es-AR");

            for (int i = 0; i < cantidad; i++)
            {
                DataRow fila = tabla.Rows[i];

                posiciones[i] = i;
                etiquetas[i] = fila["servicio"].ToString() ?? "";

                valores[i] = Convert.ToDouble(
                    fila["cantidad_total_consumida"]);
            }

            var barras = plotReporte.Plot.Add.Bars(valores);
            barras.Color = ScottPlot.Color.FromColor(dorado);
            barras.Horizontal = true;

            for (int i = 0; i < cantidad; i++)
            {
                barras.Bars[i].Position = posiciones[i];

                decimal ingresos = Convert.ToDecimal(
                    tabla.Rows[i]["ingresos_generados"]);

                barras.Bars[i].Label =
                    valores[i].ToString("N0") +
                    " | " + ingresos.ToString("C2", cultura);
            }

            barras.ValueLabelStyle.FontSize = 11;
            barras.ValueLabelStyle.Bold = true;

            plotReporte.Plot.Axes.Left.SetTicks(
            posiciones,
            etiquetas);

            double maximo = valores.Max();

            if (maximo <= 0)
                maximo = 1;

            plotReporte.Plot.Title("Servicios más utilizados");
            plotReporte.Plot.XLabel("Cantidad consumida");

            plotReporte.Plot.HideGrid();

            AplicarEstiloGrafico(plotReporte);

            plotReporte.Plot.Axes.SetLimits(
                left: 0,
                right: maximo * 1.60,
                bottom: -0.5,
                top: cantidad - 0.5);

            plotReporte.Refresh();
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
                    MostrarGraficoIngresos(datos.Tables[1]);

                    ConfigurarEncabezados(dgvResumenIngresos);
                    ConfigurarEncabezados(dgvIngresosPorMetodo);
                    ConfigurarEncabezados(dgvIngresosPorTipo);
                    ConfigurarEncabezados(dgvPagosTemporales);
                    
                    tabReporteSimple.Visible = false;
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

                    MostrarGraficoReservas(datos.Tables[0]);

                    MostrarGraficoDemanda(datos.Tables[2]);

                    ConfigurarEncabezados(dgvResumenReservas);
                    ConfigurarEncabezados(dgvReservasPorTipo);
                    ConfigurarEncabezados(dgvDemandaMensual);
                    tabReporteSimple.Visible = false;
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
                tabReporteSimple.Visible = true;
                tabReporteSimple.BringToFront();
                tabReporteSimple.SelectedTab = paginaDatosReporte;

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
               
                if (cmbReporte.SelectedIndex == 3)
                {
                    MostrarGraficoHabitaciones(tabla);
                }

                if (cmbReporte.SelectedIndex == 4)
                {
                    MostrarGraficoServicios(tabla);
                }

                if (cmbReporte.SelectedIndex == 0)
                {
                    MostrarGraficoOcupacion(tabla);
                }

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
