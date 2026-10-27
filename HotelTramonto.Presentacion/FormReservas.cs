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
        public FormReservas()
        {
            InitializeComponent();
            ConfigurarComboEstado();
            CargarReservas();
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
                    DataGridViewAutoSizeColumnsMode.Fill;
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

    }
}
