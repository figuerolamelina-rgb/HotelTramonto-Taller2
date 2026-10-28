using HotelTramonto.Negocios;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }


        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string contrasena = txtContrasena.Text;

                UsuarioNegocios negocio = new UsuarioNegocios();

                DataRow? usuario = negocio.Autenticar(email, contrasena);

                if (usuario == null)
                {
                    MessageBox.Show(
                        "Email o contraseña incorrectos, o usuario inactivo.",
                        "Error de acceso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string nombre = Convert.ToString(usuario["nombre"]) ?? "";
                string apellido = Convert.ToString(usuario["apellido"]) ?? "";
                string rol = Convert.ToString(usuario["nombre_rol"]) ?? "";
                int idUsuario = Convert.ToInt32(usuario["id_usuario"]);

                MessageBox.Show(
                 $"Bienvenido/a, {nombre} {apellido}.\nRol: {rol}",
                "Acceso correcto",
                MessageBoxButtons.OK,
                 MessageBoxIcon.Information);

                Form1 formularioPrincipal = new Form1(rol, idUsuario);
                formularioPrincipal.Show();
                this.Hide();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Ocurrió un error al intentar iniciar sesión.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
