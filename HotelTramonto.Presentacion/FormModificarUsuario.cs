using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using HotelTramonto.Datos;

namespace HotelTramonto.Presentacion
{
    public partial class FormModificarUsuario : Form
    {
        private readonly int idUsuario;
        public FormModificarUsuario(int idUsuario)
        {
            InitializeComponent();
            this.idUsuario = idUsuario;
            CargarRoles();
            CargarDatosUsuario();
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

        private void CargarDatosUsuario()
        {
            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                DataTable usuarios = negocio.Listar();

                DataRow[] filas = usuarios.Select(
                    $"id_usuario = {idUsuario}");

                if (filas.Length == 0)
                {
                    MessageBox.Show("No se encontró el usuario seleccionado.");
                    this.Close();
                    return;
                }

                DataRow usuario = filas[0];

                txtNombre.Text = usuario["nombre"].ToString();
                txtApellido.Text = usuario["apellido"].ToString();
                txtDni.Text = usuario["dni"].ToString();
                txtEmail.Text = usuario["email"].ToString();
                txtTelefono.Text = usuario["telefono"] == DBNull.Value
                    ? ""
                    : usuario["telefono"].ToString();

                cmbRol.SelectedValue = Convert.ToInt32(usuario["id_rol"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los datos del usuario: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnGuardarCambios_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbRol.SelectedIndex == -1)
                {
                    MessageBox.Show("Debe seleccionar un rol.");
                    return;
                }

                UsuarioNegocios negocio = new UsuarioNegocios();

                negocio.Modificar(
                    idUsuario,
                    Convert.ToInt32(cmbRol.SelectedValue),
                    txtNombre.Text,
                    txtApellido.Text,
                    txtDni.Text,
                    txtEmail.Text,
                    txtTelefono.Text);

                MessageBox.Show(
                    "Usuario modificado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron guardar los cambios: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
