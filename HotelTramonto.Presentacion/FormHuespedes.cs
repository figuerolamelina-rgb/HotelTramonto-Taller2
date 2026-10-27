using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormHuespedes : Form
    {
        public FormHuespedes()
        {
            InitializeComponent();
            CargarHuespedes();
        }



        private void CargarHuespedes()
        {
            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                dgvHuespedes.DataSource = negocio.Listar();

                dgvHuespedes.ReadOnly = true;
                dgvHuespedes.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
                dgvHuespedes.MultiSelect = false;
                dgvHuespedes.AllowUserToAddRows = false;
                dgvHuespedes.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los huéspedes: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarHuespedes();
        }


        private void btnNuevoHuesped_Click(object sender, EventArgs e)
        {
            using (FormNuevoHuesped formulario = new FormNuevoHuesped())
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHuespedes();
                }
            }
        }


        private void btnModificarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un huésped para modificar.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                using (FormModificarHuesped formulario =
                    new FormModificarHuesped(idHuesped))
                {
                    if (formulario.ShowDialog() == DialogResult.OK)
                    {
                        CargarHuespedes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al modificar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnDesactivarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un huésped para desactivar.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool activo = Convert.ToBoolean(
                dgvHuespedes.CurrentRow.Cells["activo"].Value);

            if (!activo)
            {
                MessageBox.Show(
                    "El huésped ya está desactivado.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de que desea desactivar al huésped seleccionado?",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                new HotelTramonto.Negocios.HuespedNegocios()
                    .Desactivar(idHuesped);

                MessageBox.Show(
                    "Huésped desactivado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHuespedes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo desactivar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnReactivarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un huésped para reactivar.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool activo = Convert.ToBoolean(
                dgvHuespedes.CurrentRow.Cells["activo"].Value);

            if (activo)
            {
                MessageBox.Show(
                    "El huésped seleccionado ya está activo.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Desea reactivar al huésped seleccionado?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                new HotelTramonto.Negocios.HuespedNegocios()
                    .Reactivar(idHuesped);

                MessageBox.Show(
                    "Huésped reactivado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHuespedes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnBuscarDNI_Click(object sender, EventArgs e)
        {
            string dni = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el DNI del huésped:",
                "Buscar huésped por DNI");

            if (string.IsNullOrWhiteSpace(dni))
                return;

            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                DataTable resultado = negocio.BuscarPorDNI(dni.Trim());

                dgvHuespedes.DataSource = resultado;

                if (resultado.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No se encontró ningún huésped con ese DNI.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al buscar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnBuscarDNI_Click_1(object sender, EventArgs e)
        {
            string dni = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el DNI del huésped:",
                "Buscar huésped por DNI");

            if (string.IsNullOrWhiteSpace(dni))
                return;

            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                DataTable resultado = negocio.BuscarPorDNI(dni.Trim());

                dgvHuespedes.DataSource = resultado;

                if (resultado.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No se encontró ningún huésped con ese DNI.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al buscar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}
