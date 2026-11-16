
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

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

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
                etiqueta.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }

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

            txtContrasena.UseSystemPasswordChar = true;

            cmbRol.Location = new Point(170, 280);
            cmbRol.Size = new Size(230, 28);
            cmbRol.BackColor = marfil;
            cmbRol.ForeColor = marron;
            cmbRol.Font = new Font("Segoe UI", 10F);
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;

            btnGuardar.Text = "Guardar usuario";
            btnGuardar.Location = new Point(170, 345);
            btnGuardar.Size = new Size(145, 40);
            btnGuardar.BackColor = dorado;
            btnGuardar.ForeColor = marron;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(218, 185, 119);
            btnGuardar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.UseVisualStyleBackColor = false;

            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(330, 345);
            btnCancelar.Size = new Size(120, 40);
            btnCancelar.BackColor = marronClaro;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);
            btnCancelar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.DialogResult = DialogResult.Cancel;

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private void MostrarMensaje(
            string mensaje,
            string titulo = "Hotel Tramonto",
            MessageBoxIcon icono = MessageBoxIcon.Information)
        {
            using (Form ventana = new Form())
            {
                ventana.Text = titulo;
                ventana.Size = new Size(440, 220);
                ventana.MinimumSize = new Size(440, 220);
                ventana.MaximumSize = new Size(440, 220);
                ventana.StartPosition = FormStartPosition.CenterParent;
                ventana.FormBorderStyle = FormBorderStyle.FixedDialog;
                ventana.MaximizeBox = false;
                ventana.MinimizeBox = false;
                ventana.ShowInTaskbar = false;
                ventana.BackColor = crema;
                ventana.Font = new Font("Segoe UI", 10F);

                Panel panelSuperior = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 9,
                    BackColor = dorado
                };

                string simbolo;
                Color colorSimbolo;

                switch (icono)
                {
                    case MessageBoxIcon.Error:
                        simbolo = "!";
                        colorSimbolo = Color.FromArgb(160, 55, 45);
                        break;

                    case MessageBoxIcon.Warning:
                        simbolo = "⚠";
                        colorSimbolo = dorado;
                        break;

                    case MessageBoxIcon.Question:
                        simbolo = "?";
                        colorSimbolo = dorado;
                        break;

                    default:
                        simbolo = "✓";
                        colorSimbolo = marron;
                        break;
                }

                Label lblIcono = new Label
                {
                    AutoSize = false,
                    Location = new Point(22, 28),
                    Size = new Size(45, 45),
                    Font = new Font("Segoe UI Symbol", 25F, FontStyle.Bold),
                    ForeColor = colorSimbolo,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = simbolo
                };

                Label lblMensaje = new Label
                {
                    AutoSize = false,
                    Location = new Point(78, 25),
                    Size = new Size(325, 90),
                    Text = mensaje,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 10F),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Size = new Size(110, 36),
                    Location = new Point(294, 130),
                    BackColor = dorado,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK,
                    UseVisualStyleBackColor = false
                };

                btnAceptar.FlatAppearance.BorderSize = 0;
                btnAceptar.FlatAppearance.MouseOverBackColor =
                    Color.FromArgb(218, 185, 119);

                ventana.Controls.Add(panelSuperior);
                ventana.Controls.Add(lblIcono);
                ventana.Controls.Add(lblMensaje);
                ventana.Controls.Add(btnAceptar);

                ventana.AcceptButton = btnAceptar;
                ventana.ShowDialog(this);
            }
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
                MostrarMensaje(
                    "Error al cargar los roles:\n" + ex.Message,
                    "Error",
                    MessageBoxIcon.Error);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbRol.SelectedIndex == -1)
                {
                    MostrarMensaje(
                        "Debe seleccionar un rol.",
                        "Rol requerido",
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

                MostrarMensaje(
                    "Usuario registrado correctamente.",
                    "Registro exitoso");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "No se pudo registrar el usuario:\n" + ex.Message,
                    "Error al registrar",
                    MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void label5_Click(object sender, EventArgs e) { }
    }
}
