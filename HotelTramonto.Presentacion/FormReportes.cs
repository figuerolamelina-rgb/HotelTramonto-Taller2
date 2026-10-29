
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

            // Encabezado
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

            // Panel de filtros
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

            btnConsultar = CrearBoton("Consultar reporte", dorado, marron);
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

            // Tabla de resultados
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
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

            // Barra inferior
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

            // Orden de acoplamiento: inferior, tabla, filtros y encabezado.
            Controls.Add(dgvReporte);
            Controls.Add(panelInferior);
            Controls.Add(panelFiltros);
            Controls.Add(panelTitulo);

            // Fechas iniciales
            dtpHasta.Value = DateTime.Today;
            dtpDesde.Value = DateTime.Today.AddMonths(-1);
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

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;

                if (desde > hasta)
                {
                    MessageBox.Show(
                        "La fecha desde no puede ser posterior a la fecha hasta.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                DataTable tabla;

                switch (cmbReporte.SelectedIndex)
                {
                    case 0:
                        tabla = negocio.ObtenerOcupacion(desde, hasta);
                        break;

                    case 1:
                        tabla = negocio.ObtenerIngresos(desde, hasta).Tables[0];
                        break;

                    case 2:
                        tabla = negocio.ObtenerEstadisticasReservas(desde, hasta).Tables[0];
                        break;

                    case 3:
                        tabla = negocio.ObtenerHabitacionesMasUtilizadas(desde, hasta);
                        break;

                    case 4:
                        tabla = negocio.ObtenerServiciosMasUtilizados(desde, hasta);
                        break;

                    default:
                        MessageBox.Show(
                            "Seleccioná un reporte.",
                            "Hotel Tramonto",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                }

                dgvReporte.DataSource = tabla;

                lblEstado.Text =
                    $"Reporte: {cmbReporte.SelectedItem}  |  " +
                    $"Registros encontrados: {tabla.Rows.Count}";

                if (tabla.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "El reporte no devolvió resultados para ese período.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo generar el reporte: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
