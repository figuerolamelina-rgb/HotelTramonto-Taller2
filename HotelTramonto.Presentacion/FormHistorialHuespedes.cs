
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using System.Linq;

namespace HotelTramonto.Presentacion
{
    public class FormHistorialHuespedes : Form
    {
        private readonly int idHuesped;
        private readonly string rol;
        private readonly string nombre;
        private readonly string apellido;
        private readonly string dni;

        private readonly Color marron =
            Color.FromArgb(74, 48, 39);

        private readonly Color marronClaro =
            Color.FromArgb(91, 62, 49);

        private readonly Color crema =
            Color.FromArgb(245, 239, 229);

        private readonly Color marfil =
            Color.FromArgb(255, 252, 247);

        private readonly Color dorado =
            Color.FromArgb(198, 161, 91);

        private DataGridView dgvHistorial;
        private Label lblResumen;

        public FormHistorialHuespedes(
            int idHuesped,
            string nombre,
            string apellido,
            string dni, string rol)
        {
            PermisosOperacion.Exigir(rol);
            this.rol = rol;
            this.idHuesped = idHuesped;
            PresentacionTablas.Conectar(this);
            this.nombre = nombre;
            this.apellido = apellido;
            this.dni = dni;

            ConfigurarFormulario();
            CargarHistorial();
        }

        private void ConfigurarFormulario()
        {
            Text = "Hotel Tramonto | Historial del huésped";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1150, 650);
            MinimumSize = new Size(900, 500);
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);

            Panel panelTitulo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = marron
            };

            Label lblTitulo = new Label
            {
                Text = "HISTORIAL DEL HUÉSPED",
                Location = new Point(25, 15),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font(
                    "Georgia", 22F, FontStyle.Bold)
            };

            Label lblHuesped = new Label
            {
                Text = "Huésped: " + nombre + " " +
                       apellido + "  |  DNI: " + dni,
                Location = new Point(28, 65),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold)
            };

            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(lblHuesped);

            dgvHistorial = new DataGridView
            {
                Location = new Point(25, 150),
                Size = new Size(
                    ClientSize.Width - 50,
                    ClientSize.Height - 235),

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right,

                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                RowHeadersVisible = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,

                BackgroundColor = marfil,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(222, 211, 194),
                EnableHeadersVisualStyles = false
            };

            dgvHistorial.ColumnHeadersHeight = 42;
            dgvHistorial.RowTemplate.Height = 35;

            dgvHistorial.ColumnHeadersDefaultCellStyle.BackColor =
                marron;

            dgvHistorial.ColumnHeadersDefaultCellStyle.ForeColor =
                dorado;

            dgvHistorial.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvHistorial.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                marron;

            dgvHistorial.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                dorado;

            dgvHistorial.DefaultCellStyle.BackColor = marfil;
            dgvHistorial.DefaultCellStyle.ForeColor = marron;

            dgvHistorial.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(226, 211, 184);

            dgvHistorial.DefaultCellStyle.SelectionForeColor =
                marron;

            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor =
                crema;

            lblResumen = new Label
            {
                Location = new Point(25, 120),
                AutoSize = true,
                ForeColor = marron,
                Font = new Font(
                    "Segoe UI", 9.5F, FontStyle.Bold)
            };

            Button btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(125, 38),
                Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Right,

                BackColor = dorado,
                ForeColor = marron,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI", 9.5F, FontStyle.Bold),

                Cursor = Cursors.Hand
            };

            btnCerrar.FlatAppearance.BorderSize = 0;

            btnCerrar.Location = new Point(
                ClientSize.Width - 150,
                ClientSize.Height - 65);

            btnCerrar.Click += (s, e) => Close();

            Controls.Add(panelTitulo);
            Controls.Add(lblResumen);
            Controls.Add(dgvHistorial);
            Controls.Add(btnCerrar);

            panelTitulo.BringToFront();
        }

        private void CargarHistorial()
        {
            try
            {
                HuespedNegocios negocio =
                    new HuespedNegocios(rol);

                DataTable historial =
                    negocio.ObtenerHistorial(idHuesped);

                dgvHistorial.DataSource = historial;

                ConfigurarColumnas();

                lblResumen.Text =
                    "Reservas: " + historial.AsEnumerable().Select(f => f.Field<int>("id_reserva")).Distinct().Count() +
                    " | Una fila por reserva y habitación; número y tipo del catálogo actual. Fechas previstas y marcas de la reserva.";

                if (historial.Rows.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "Este huésped todavía no tiene reservas registradas.",
                        "Historial vacío",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo cargar el historial.\n\n" +
                    RegistroErrores.Mensaje(ex),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnas()
        {
            if (dgvHistorial.Columns.Contains("id_reserva"))
            {
                dgvHistorial.Columns["id_reserva"].HeaderText =
                    "N.º de reserva";
            }

            if (dgvHistorial.Columns.Contains("entrada_prevista"))
            {
                dgvHistorial.Columns["entrada_prevista"].HeaderText =
                    "Entrada";

                dgvHistorial.Columns["entrada_prevista"]
                    .DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (dgvHistorial.Columns.Contains("salida_prevista"))
            {
                dgvHistorial.Columns["salida_prevista"].HeaderText =
                    "Salida";

                dgvHistorial.Columns["salida_prevista"]
                    .DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (dgvHistorial.Columns.Contains("numero_habitacion_actual"))
            {
                dgvHistorial.Columns["numero_habitacion_actual"].HeaderText =
                    "Habitación";
            }

            if (dgvHistorial.Columns.Contains("piso_habitacion"))
            {
                dgvHistorial.Columns["piso_habitacion"].HeaderText =
                    "Piso";
            }

            if (dgvHistorial.Columns.Contains("estado"))
            {
                dgvHistorial.Columns["estado"].HeaderText =
                    "Estado";
            }

            if (dgvHistorial.Columns.Contains("total"))
            {
                dgvHistorial.Columns["total"].HeaderText =
                    "Importe";

                dgvHistorial.Columns["total"]
                    .DefaultCellStyle.Format = "N2";

                dgvHistorial.Columns["total"]
                    .DefaultCellStyle.FormatProvider =
                    new CultureInfo("es-AR");

                dgvHistorial.Columns["total"]
                    .DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvHistorial.Columns.Contains("observaciones"))
            {
                DataGridViewColumn columna =
                    dgvHistorial.Columns["observaciones"];

                columna.HeaderText = "Observaciones";

                columna.MinimumWidth = 180;
                columna.DefaultCellStyle.WrapMode =
                    DataGridViewTriState.True;
            }

            foreach (string columna in new[] { "id_huesped", "id_habitacion", "dni", "nombre", "apellido" })
                if (dgvHistorial.Columns.Contains(columna)) dgvHistorial.Columns[columna].Visible = false;
            // Permitir que las filas aumenten su altura
            // cuando las observaciones ocupen varias líneas.
            dgvHistorial.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.AllCells;

            // Ocultar fechas técnicas internas.
            if (dgvHistorial.Columns.Contains("fecha_creacion"))
                dgvHistorial.Columns["fecha_creacion"].Visible = false;

            if (dgvHistorial.Columns.Contains("checkin_reserva"))
                dgvHistorial.Columns["checkin_reserva"].Visible = true;

            if (dgvHistorial.Columns.Contains("checkout_reserva"))
                dgvHistorial.Columns["checkout_reserva"].Visible = true;
        }
    }
}
