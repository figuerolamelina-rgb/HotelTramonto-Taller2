using HotelTramonto.Entidades;
using HotelTramonto.Negocios;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormReservas : Form
    {
        private readonly int idUsuario;

        public FormReservas(int idUsuario)
        {
            InitializeComponent();
            AplicarEstilo();

            this.idUsuario = idUsuario;

            ConfigurarComboEstado();
            CargarReservas();

            Button btnNuevaReserva = new Button
            {
                Name = "btnNuevaReserva",
                Text = "＋ Nueva reserva",
                Size = new Size(165, 42),
                Location = new Point(35, 80),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(198, 161, 91),
                ForeColor = Color.FromArgb(74, 48, 39),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btnNuevaReserva.FlatAppearance.BorderSize = 0;
            btnNuevaReserva.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(218, 185, 119);

            btnNuevaReserva.Click += btnNuevaReserva_Click;
            Controls.Add(btnNuevaReserva);
            btnNuevaReserva.BringToFront();
        }


        private void AplicarEstilo()
        {
            // Paleta Hotel Tramonto
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color marfil = Color.FromArgb(255, 252, 247);
            Color dorado = Color.FromArgb(198, 161, 91);
            Color texto = Color.FromArgb(66, 51, 43);
            Color cremaAlternativo = Color.FromArgb(248, 243, 235);

            // Ventana
            Text = "Hotel Tramonto | Reservas";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);

            // Título
            lblTitulo.Text = "GESTIÓN DE RESERVAS";
            lblTitulo.Font = new Font("Georgia", 25F, FontStyle.Bold);
            lblTitulo.ForeColor = dorado;
            lblTitulo.BackColor = Color.Transparent;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(35, 25);

            // Etiquetas de fechas
            foreach (Label etiqueta in new[] { lblFechaDesde, lblFechaHasta })
            {
                etiqueta.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                etiqueta.ForeColor = marron;
                etiqueta.BackColor = Color.Transparent;
            }

            lblFechaDesde.Location = new Point(35, 135);
            lblFechaHasta.Location = new Point(35, 180);

            // Selectores de fecha
            foreach (DateTimePicker fecha in new[] { dtpFechaDesde, dtpFechaHasta })
            {
                fecha.Font = new Font("Segoe UI", 10F);
                fecha.CalendarMonthBackground = marfil;
                fecha.Format = DateTimePickerFormat.Custom;
                fecha.CustomFormat = "dd/MM/yyyy";
                fecha.Width = 150;
            }

            dtpFechaDesde.Location = new Point(115, 130);
            dtpFechaHasta.Location = new Point(115, 175);

            // Filtro por estado
            cmbEstado.Font = new Font("Segoe UI", 10F);
            cmbEstado.BackColor = marfil;
            cmbEstado.ForeColor = marron;
            cmbEstado.Location = new Point(390, 130);
            cmbEstado.Size = new Size(180, 32);

            EstilizarBotonReserva(btnFiltrarEstado, "Filtrar estado", marron);
            btnFiltrarEstado.Location = new Point(585, 128);
            btnFiltrarEstado.Size = new Size(145, 38);

            EstilizarBotonReserva(btnFiltrarFechas, "Filtrar fechas", marron);
            btnFiltrarFechas.Location = new Point(390, 175);
            btnFiltrarFechas.Size = new Size(180, 38);

            // Acciones de ingreso y egreso
            EstilizarBotonReserva(btnCheckOut, "Registrar Check-out", marron);
            btnCheckOut.Location = new Point(755, 128);
            btnCheckOut.Size = new Size(180, 38);

            EstilizarBotonReserva(btnCheckIn, "Registrar Check-in", marron);
            btnCheckIn.Location = new Point(755, 175);
            btnCheckIn.Size = new Size(180, 38);

            // Consultar saldo
            EstilizarBotonReserva(btnConsultarSaldo, "Consultar saldo", dorado);
            btnConsultarSaldo.ForeColor = marron;
            btnConsultarSaldo.Location = new Point(35, 225);
            btnConsultarSaldo.Size = new Size(165, 40);

            // Tabla de reservas
            dgvReservas.Location = new Point(35, 285);
            dgvReservas.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                Math.Max(150, ClientSize.Height - 375));

            dgvReservas.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvReservas.BackgroundColor = marfil;
            dgvReservas.BorderStyle = BorderStyle.None;
            dgvReservas.GridColor = Color.FromArgb(229, 218, 202);
            dgvReservas.EnableHeadersVisualStyles = false;
            dgvReservas.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvReservas.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvReservas.MultiSelect = false;
            dgvReservas.ReadOnly = true;
            dgvReservas.RowHeadersVisible = false;
            dgvReservas.RowTemplate.Height = 36;
            dgvReservas.ColumnHeadersHeight = 42;
            dgvReservas.AllowUserToAddRows = false;

            dgvReservas.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvReservas.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvReservas.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvReservas.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvReservas.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvReservas.DefaultCellStyle.BackColor = marfil;
            dgvReservas.DefaultCellStyle.ForeColor = texto;
            dgvReservas.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F);
            dgvReservas.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);
            dgvReservas.DefaultCellStyle.SelectionForeColor = marron;

            dgvReservas.AlternatingRowsDefaultCellStyle.BackColor =
                cremaAlternativo;

            // Acciones inferiores
            EstilizarBotonReserva(btnCancelarReserva, "Cancelar reserva", marronClaro);
            btnCancelarReserva.Location = new Point(35, ClientSize.Height - 70);
            btnCancelarReserva.Size = new Size(170, 42);
            btnCancelarReserva.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;

            EstilizarBotonReserva(btnActualizar, "Actualizar", marron);
            btnActualizar.Location = new Point(225, ClientSize.Height - 70);
            btnActualizar.Size = new Size(130, 42);
            btnActualizar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;

            EstilizarBotonReserva(btnCerrar, "Cerrar", dorado);
            btnCerrar.ForeColor = marron;
            btnCerrar.Location = new Point(
                ClientSize.Width - 165,
                ClientSize.Height - 70);
            btnCerrar.Size = new Size(130, 42);
            btnCerrar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Right;
        }

        private void EstilizarBotonReserva(
            Button boton,
            string textoBoton,
            Color fondo)
        {
            boton.Text = textoBoton;
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.BackColor = fondo;
            boton.ForeColor = Color.FromArgb(255, 252, 247);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(105, 73, 52);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }



        private void CargarReservas()
        {
            try
            {
                ReservaNegocios negocio = new ReservaNegocios();

                dgvReservas.DataSource = negocio.Listar();

                dgvReservas.ReadOnly = true;
                dgvReservas.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
                dgvReservas.MultiSelect = false;
                dgvReservas.AllowUserToAddRows = false;
                dgvReservas.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.DisplayedCells;
                dgvReservas.ScrollBars = ScrollBars.Both;

                // Formato del importe total
                if (dgvReservas.Columns.Contains("total"))
                {
                    dgvReservas.Columns["total"].DefaultCellStyle.Format = "N2";
                    dgvReservas.Columns["total"].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
                }

                // Dar más espacio a las observaciones
                if (dgvReservas.Columns.Contains("observaciones"))
                {
                    dgvReservas.Columns["observaciones"].Width = 240;
                    dgvReservas.Columns["observaciones"].MinimumWidth = 180;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar las reservas: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarReservas();
            MessageBox.Show(
               "Listado de reservas actualizado.",
               "Hotel Tramonto",
               MessageBoxButtons.OK,
               MessageBoxIcon.Information);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void btnCancelarReserva_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null ||
                dgvReservas.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione una reserva de la tabla.");
                return;
            }

            int idReserva = Convert.ToInt32(
                dgvReservas.CurrentRow.Cells["id_reserva"].Value);

            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de que desea cancelar la reserva seleccionada?",
                "Confirmar cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                ReservaNegocios negocio = new ReservaNegocios();
                negocio.Cancelar(idReserva);

                MessageBox.Show(
                    "Reserva cancelada correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cancelar la reserva: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void ConfigurarComboEstado()
        {
            cmbEstado.Items.Clear();

            cmbEstado.Items.Add("TODOS");
            cmbEstado.Items.Add("CONFIRMADA");
            cmbEstado.Items.Add("CHECKIN");
            cmbEstado.Items.Add("FINALIZADA");
            cmbEstado.Items.Add("CANCELADA");

            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.SelectedIndex = 0;
        }


        private void btnFiltrarEstado_Click(object sender, EventArgs e)
        {
            try
            {
                ReservaNegocios negocio = new ReservaNegocios();

                if (cmbEstado.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un estado.");
                    return;
                }

                string estado = cmbEstado.SelectedItem.ToString()!;

                if (estado == "TODOS")
                {
                    CargarReservas();
                }
                else
                {
                    dgvReservas.DataSource = negocio.ListarPorEstado(estado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al filtrar las reservas: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnFiltrarFechas_Click(object sender, EventArgs e)
        {
            if (dtpFechaDesde.Value.Date > dtpFechaHasta.Value.Date)
            {
                MessageBox.Show(
                    "La fecha Desde no puede ser posterior a la fecha Hasta.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ReservaNegocios negocio = new ReservaNegocios();

                dgvReservas.DataSource = negocio.ListarPorFechas(
                    dtpFechaDesde.Value.Date,
                    dtpFechaHasta.Value.Date);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al filtrar las reservas por fechas: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnConsultarSaldo_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reserva.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                ReservaNegocios negocio = new ReservaNegocios();
                DataTable resultado = negocio.ConsultarSaldo(idReserva);

                if (resultado.Rows.Count > 0)
                {
                    string mensaje = "";

                    foreach (DataColumn columna in resultado.Columns)
                    {
                        mensaje += columna.ColumnName + ": " +
                            resultado.Rows[0][columna] + "\n";
                    }

                    MessageBox.Show(mensaje,
                        "Saldo de la reserva",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró información del saldo.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar el saldo: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reserva.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                ReservaNegocios negocio = new ReservaNegocios();
                negocio.CheckIn(idReserva);

                MessageBox.Show("Check-in registrado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el check-in: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reserva.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                ReservaNegocios negocio = new ReservaNegocios();
                negocio.CheckOut(idReserva);

                MessageBox.Show("Check-out registrado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarReservas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el check-out: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnNuevaReserva_Click(object sender, EventArgs e)
        {
            using (FormNuevaReserva formulario = new FormNuevaReserva(idUsuario))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarReservas();
                }
            }
        }


    }
}
