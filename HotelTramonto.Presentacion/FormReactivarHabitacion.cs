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
    public partial class FormReactivarHabitacion : Form
    {
        public FormReactivarHabitacion()
        {
            InitializeComponent();
            CargarHabitacionesInactivas();
        }



        private void CargarHabitacionesInactivas()
        {
            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();

                DataTable tabla = negocio.ListarInactivas();

                MessageBox.Show(
                    "Habitaciones inactivas encontradas: " + tabla.Rows.Count);

                dgvHabitacionesInactivas.DataSource = tabla;

                dgvHabitacionesInactivas.ReadOnly = true;
                dgvHabitacionesInactivas.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
                dgvHabitacionesInactivas.MultiSelect = false;
                dgvHabitacionesInactivas.AllowUserToAddRows = false;
                dgvHabitacionesInactivas.AutoSizeColumnsMode =
                 DataGridViewAutoSizeColumnsMode.None;

                dgvHabitacionesInactivas.AutoSizeRowsMode =
                    DataGridViewAutoSizeRowsMode.AllCells;

                dgvHabitacionesInactivas.ScrollBars =
                    ScrollBars.Both;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar las habitaciones inactivas: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void btnReactivar_Click(object sender, EventArgs e)
        {
            if (dgvHabitacionesInactivas.CurrentRow == null ||
                dgvHabitacionesInactivas.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione una habitación de la tabla.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitacionesInactivas.CurrentRow.Cells["id_habitacion"].Value);

            DialogResult confirmacion = MessageBox.Show(
                "¿Desea reactivar la habitación seleccionada?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                negocio.Reactivar(idHabitacion);

                MessageBox.Show(
                    "Habitación reactivada correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHabitacionesInactivas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar la habitación: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
