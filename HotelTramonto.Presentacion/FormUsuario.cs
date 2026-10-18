using HotelTramonto.Datos;
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
    public partial class FormUsuario : Form
    {
        public FormUsuario()
        {
            InitializeComponent();
            CargarRoles();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void CargarRoles()
        {
            try
            {
                RolDatos datos = new RolDatos();
                DataTable roles = datos.Listar();

                cmbRol.DataSource = roles;
                cmbRol.DisplayMember = "nombre_rol";
                cmbRol.ValueMember = "id_rol";
                cmbRol.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los roles: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbRol.SelectedIndex == -1)
                {
                    MessageBox.Show("Debe seleccionar un rol.");
                    return;
                }

                UsuarioNegocios negocio = new UsuarioNegocios();

                negocio.Insertar(
                    Convert.ToInt32(cmbRol.SelectedValue),
                    txtNombre.Text,
                    txtApellido.Text,
                    txtDni.Text,
                    txtEmail.Text,
                    txtContrasena.Text,
                    txtTelefono.Text);

                MessageBox.Show(
                    "Usuario registrado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo registrar el usuario: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
