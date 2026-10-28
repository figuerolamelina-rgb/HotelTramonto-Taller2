
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

        public FormReportes()
        {
            InitializeComponent();
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            Text = "Reportes - Hotel Tramonto";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            Label lblDesde = new Label
            {
                Text = "Fecha desde:",
                Left = 20,
                Top = 22,
                Width = 100
            };

            dtpDesde = new DateTimePicker
            {
                Left = 120,
                Top = 18,
                Width = 150,
                Format = DateTimePickerFormat.Short
            };

            Label lblHasta = new Label
            {
                Text = "Fecha hasta:",
                Left = 285,
                Top = 22,
                Width = 100
            };

            dtpHasta = new DateTimePicker
            {
                Left = 385,
                Top = 18,
                Width = 150,
                Format = DateTimePickerFormat.Short
            };

            Label lblTipo = new Label
            {
                Text = "Reporte:",
                Left = 550,
                Top = 22,
                Width = 65
            };

            cmbReporte = new ComboBox
            {
                Left = 615,
                Top = 18,
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
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

            btnConsultar = new Button
            {
                Text = "Consultar",
                Left = 850,
                Top = 16,
                Width = 110,
                Height = 30
            };
            btnConsultar.Click += btnConsultar_Click;

            dgvReporte = new DataGridView
            {
                Left = 20,
                Top = 65,
                Width = 940,
                Height = 430,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            lblEstado = new Label
            {
                Left = 20,
                Top = 510,
                Width = 700,
                Height = 30,
                Text = "Seleccioná un reporte y un rango de fechas."
            };

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Left = 850,
                Top = 510,
                Width = 110,
                Height = 30
            };
            btnCerrar.Click += (sender, e) => Close();

            Controls.Add(lblDesde);
            Controls.Add(dtpDesde);
            Controls.Add(lblHasta);
            Controls.Add(dtpHasta);
            Controls.Add(lblTipo);
            Controls.Add(cmbReporte);
            Controls.Add(btnConsultar);
            Controls.Add(dgvReporte);
            Controls.Add(lblEstado);
            Controls.Add(btnCerrar);
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;

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
                        MessageBox.Show("Seleccioná un reporte.");
                        return;
                }

                dgvReporte.DataSource = tabla;
                lblEstado.Text = $"Registros encontrados: {tabla.Rows.Count}";

                if (tabla.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "El reporte no devolvió resultados para ese período.",
                        "Reportes",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo generar el reporte: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
