
using System;
using System.Data;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormModificarHuesped : Form
    {
        private readonly int idHuesped;
        private readonly HuespedNegocios negocio = new HuespedNegocios();

        public FormModificarHuesped(int idHuesped)
        {
            InitializeComponent();

            if (idHuesped <= 0)
                throw new ArgumentException("El huésped seleccionado no es válido.");

            this.idHuesped = idHuesped;

            CargarDatosHuesped();
        }

        private void CargarDatosHuesped()
        {
            try
            {
                DataTable tabla = negocio.Listar();

                DataRow[] filas = tabla.Select(
                    "id_huesped = " + idHuesped);

                if (filas.Length == 0)
                {
                    MessageBox.Show(
                        "No se encontró el huésped seleccionado.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                DataRow huesped = filas[0];

                txtNombre.Text = Convert.ToString(huesped["nombre"]) ?? "";
                txtApellido.Text = Convert.ToString(huesped["apellido"]) ?? "";
                txtDNI.Text = Convert.ToString(huesped["dni"]) ?? "";
                txtTelefono.Text = Convert.ToString(huesped["telefono"]) ?? "";
                txtEmail.Text = Convert.ToString(huesped["email"]) ?? "";
                txtNacionalidad.Text = Convert.ToString(huesped["nacionalidad"]) ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los datos del huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                negocio.Modificar(
                    idHuesped,
                    txtNombre.Text,
                    txtApellido.Text,
                    txtDNI.Text,
                    txtTelefono.Text,
                    txtEmail.Text,
                    txtNacionalidad.Text);

                MessageBox.Show(
                    "Huésped modificado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo modificar el huésped: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
