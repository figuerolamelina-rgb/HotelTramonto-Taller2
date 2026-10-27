using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormNuevoHuesped : Form
    {
        public FormNuevoHuesped()
        {
            InitializeComponent();
        }


        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                negocio.Insertar(
                    txtNombre.Text,
                    txtApellido.Text,
                    txtDNI.Text,
                    txtTelefono.Text,
                    txtEmail.Text,
                    txtNacionalidad.Text
                );

                MessageBox.Show(
                    "Huésped registrado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo registrar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
