
using HotelTramonto.Datos;
using HotelTramonto.Negocios;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormUsuario : Form
    {
        private readonly string rolUsuario;

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

        public FormUsuario(string rol)
        {
            InitializeComponent();

            rolUsuario = rol;

            AplicarEstilo();
            CargarRoles();
        }

        private void AplicarEstilo()
        {
            Text = "Registrar usuario - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = crema;
            ForeColor = marron;
            ClientSize = new Size(800, 450);
            Font = new Font("Segoe UI", 10F);

            // Título
            label1.Text = "Nombre";
            label2.Text = "Apellido";
            label3.Text = "DNI";
            label4.Text = "Email";
            label5.Text = "Contraseña";
            label6.Text = "Teléfono";
            label7.Text = "Rol";

            Label[] etiquetas =
            {
                label1, label2, label3, label4, label5, label6, label7
            };

            foreach (Label etiqueta in etiquetas)
            {
                etiqueta.ForeColor = marron;
                etiqueta.Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold);
            }

            // Campos de texto
            TextBox[] campos =
            {
                txtNombre, txtApellido, txtDni,
                txtEmail, txtContrasena, txtTelefono
            };

            foreach (TextBox campo in campos)
            {
                campo.BackColor = marfil;
                campo.ForeColor = marron;
                campo.BorderStyle = BorderStyle.FixedSingle;
                campo.Font = new Font("Segoe UI", 10F);
                campo.Size = new Size(230, 28);
            }

            txtNombre.Location = new Point(170, 40);
            txtApellido.Location = new Point(170, 80);
            txtDni.Location = new Point(170, 120);
            txtEmail.Location = new Point(170, 160);
            txtContrasena.Location = new Point(170, 200);
            txtTelefono.Location = new Point(170, 240);

            label1.Location = new Point(55, 43);
            label2.Location = new Point(55, 83);
            label3.Location = new Point(55, 123);
            label4.Location = new Point(55, 163);
            label5.Location = new Point(55, 203);
            label6.Location = new Point(55, 243);
            label7.Location = new Point(55, 283);

            // Ocultar contraseña
            txtContrasena.UseSystemPasswordChar = true;

            // Rol
            cmbRol.Location = new Point(170, 280);
            cmbRol.Size = new Size(230, 28);
            cmbRol.BackColor = marfil;
            cmbRol.ForeColor = marron;
            cmbRol.Font = new Font("Segoe UI", 10F);
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;

            // Guardar
            btnGuardar.Text = "Guardar usuario";
            btnGuardar.Location = new Point(170, 345);
            btnGuardar.Size = new Size(145, 40);
            btnGuardar.BackColor = dorado;
            btnGuardar.ForeColor = marron;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.UseVisualStyleBackColor = false;

            // Cancelar
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(330, 345);
            btnCancelar.Size = new Size(120, 40);
            btnCancelar.BackColor = marronClaro;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.DialogResult = DialogResult.Cancel;

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
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
                    MessageBox.Show(
                        "Debe seleccionar un rol.",
                        "Hotel Tramonto",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
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
                    txtTelefono.Text,
                    rolUsuario);

                MessageBox.Show(
                    "Usuario registrado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
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

        private void label1_Click(object sender, EventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void label5_Click(object sender, EventArgs e) { }
    }
}
