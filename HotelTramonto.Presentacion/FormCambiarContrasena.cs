
using System;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public class FormCambiarContrasena : Form
    {
        private readonly int idUsuario;

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private TextBox txtContrasena;
        private TextBox txtConfirmar;
        private Button btnGuardar;
        private Button btnCancelar;

        public FormCambiarContrasena(int idUsuario)
        {
            this.idUsuario = idUsuario;

            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            Text = "Cambiar contraseña - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 330);
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);

            Panel encabezado = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(460, 85),
                BackColor = marron
            };

            Label titulo = new Label
            {
                Text = "CAMBIAR CONTRASEÑA",
                Location = new Point(22, 15),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold)
            };

            Label subtitulo = new Label
            {
                Text = "Ingresá la nueva contraseña del usuario",
                Location = new Point(25, 53),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 9F)
            };

            encabezado.Controls.Add(titulo);
            encabezado.Controls.Add(subtitulo);
            Controls.Add(encabezado);

            Panel panel = new Panel
            {
                Location = new Point(20, 105),
                Size = new Size(420, 145),
                BackColor = marfil,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblContrasena = new Label
            {
                Text = "Nueva contraseña",
                Location = new Point(20, 22),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtContrasena = new TextBox
            {
                Location = new Point(190, 18),
                Size = new Size(205, 28),
                UseSystemPasswordChar = true
            };

            Label lblConfirmar = new Label
            {
                Text = "Confirmar contraseña",
                Location = new Point(20, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtConfirmar = new TextBox
            {
                Location = new Point(190, 68),
                Size = new Size(205, 28),
                UseSystemPasswordChar = true
            };

            Label lblAyuda = new Label
            {
                Text = "La contraseña debe tener al menos 8 caracteres.",
                Location = new Point(20, 112),
                AutoSize = true,
                ForeColor = marronClaro,
                Font = new Font("Segoe UI", 8.5F)
            };

            panel.Controls.Add(lblContrasena);
            panel.Controls.Add(txtContrasena);
            panel.Controls.Add(lblConfirmar);
            panel.Controls.Add(txtConfirmar);
            panel.Controls.Add(lblAyuda);

            Controls.Add(panel);

            btnGuardar = CrearBoton(
                "Guardar",
                dorado,
                marron);

            btnGuardar.Location = new Point(105, 270);
            btnGuardar.Size = new Size(115, 38);
            btnGuardar.Click += btnGuardar_Click;

            btnCancelar = CrearBoton(
                "Cancelar",
                marronClaro,
                Color.White);

            btnCancelar.Location = new Point(240, 270);
            btnCancelar.Size = new Size(115, 38);

            btnCancelar.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private Button CrearBoton(
            string texto,
            Color fondo,
            Color colorTexto)
        {
            Button boton = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = colorTexto,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);

            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(174, 139, 76);

            return boton;
        }

        private void MostrarMensaje(
            string titulo,
            string detalle,
            bool esError = false)
        {
            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(400, 175);
                mensaje.BackColor = crema;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(400, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTitulo = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(370, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font(
                        "Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(360, 50),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = CrearBoton(
                    "Aceptar",
                    dorado,
                    marron);

                btnAceptar.Location = new Point(145, 120);
                btnAceptar.Size = new Size(110, 35);
                btnAceptar.DialogResult = DialogResult.OK;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTitulo);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;

                mensaje.ShowDialog(this);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string contrasena = txtContrasena.Text;
            string confirmar = txtConfirmar.Text;

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                MostrarMensaje(
                    "Contraseña requerida",
                    "Ingresá la nueva contraseña.");

                txtContrasena.Focus();
                return;
            }

            if (contrasena.Length < 8)
            {
                MostrarMensaje(
                    "Contraseña inválida",
                    "La contraseña debe tener al menos 8 caracteres.");

                txtContrasena.Focus();
                return;
            }

            if (contrasena != confirmar)
            {
                MostrarMensaje(
                    "Error de confirmación",
                    "Las contraseñas no coinciden.");

                txtConfirmar.Clear();
                txtConfirmar.Focus();
                return;
            }

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();

                negocio.CambiarContrasena(
                    idUsuario,
                    contrasena);

                MostrarMensaje(
                    "Operación exitosa",
                    "La contraseña se actualizó correctamente.");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cambiar contraseña",
                    "No se pudo cambiar la contraseña.\n\n"
                    + ex.Message,
                    true);
            }
        }
    }
}
