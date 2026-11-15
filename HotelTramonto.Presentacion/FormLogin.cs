using HotelTramonto.Negocios;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormLogin : Form
    {
        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        public FormLogin()
        {
            InitializeComponent();
            AplicarEstilo();
        }

        private void AplicarEstilo()
        {
            Text = "Hotel Tramonto - Iniciar sesión";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 600);
            MinimumSize = new Size(1016, 639);
            MaximumSize = new Size(1016, 639);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);

            Controls.Clear();

            // Panel izquierdo con la fotografía
            Panel panelImagen = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(550, 600),
                BackColor = marron
            };

            PictureBox foto = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = System.Drawing.Image.FromFile(
                System.IO.Path.Combine(
                    Application.StartupPath,
                    "Resources",
                    "Imagen de ChatGPT 28 oct 2026, 21_16_09.jpg"
                )),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = marron
            };

            panelImagen.Controls.Add(foto);

            // Franja inferior sobre la imagen
            Panel panelFranja = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                BackColor = marron
            };

            Label lblNombreHotel = new Label
            {
                Text = "HOTEL TRAMONTO",
                ForeColor = dorado,
                Font = new Font("Georgia", 23F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 55
            };

            Label lblDescripcion = new Label
            {
                Text = "Descanso, confort y una experiencia única",
                ForeColor = crema,
                Font = new Font("Segoe UI", 10F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 35
            };

            panelFranja.Controls.Add(lblDescripcion);
            panelFranja.Controls.Add(lblNombreHotel);
            panelImagen.Controls.Add(panelFranja);

            // Panel derecho para iniciar sesión
            Panel panelLogin = new Panel
            {
                Location = new Point(550, 0),
                Size = new Size(450, 600),
                BackColor = marfil
            };

            Label lblBienvenida = new Label
            {
                Text = "¡Bienvenido/a!",
                Location = new Point(45, 105),
                Size = new Size(360, 45),
                ForeColor = marron,
                Font = new Font("Georgia", 23F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubtitulo = new Label
            {
                Text = "Ingresá tus datos para continuar",
                Location = new Point(45, 155),
                Size = new Size(360, 30),
                ForeColor = marronClaro,
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblEmailNuevo = new Label
            {
                Text = "Correo electrónico",
                Location = new Point(55, 220),
                AutoSize = true,
                ForeColor = marron,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtEmail.Location = new Point(55, 248);
            txtEmail.Size = new Size(340, 34);
            txtEmail.Font = new Font("Segoe UI", 11F);
            txtEmail.BackColor = crema;
            txtEmail.ForeColor = marron;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.PlaceholderText = "Ingresá tu correo";

            Label lblContrasenaNueva = new Label
            {
                Text = "Contraseña",
                Location = new Point(55, 305),
                AutoSize = true,
                ForeColor = marron,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtContrasena.Location = new Point(55, 333);
            txtContrasena.Size = new Size(340, 34);
            txtContrasena.Font = new Font("Segoe UI", 11F);
            txtContrasena.BackColor = crema;
            txtContrasena.ForeColor = marron;
            txtContrasena.BorderStyle = BorderStyle.FixedSingle;
            txtContrasena.UseSystemPasswordChar = true;
            txtContrasena.PlaceholderText = "Ingresá tu contraseña";

            EstilizarBoton(
                btnIngresar,
                "INGRESAR",
                dorado,
                marron);

            btnIngresar.Location = new Point(55, 395);
            btnIngresar.Size = new Size(340, 45);

            EstilizarBoton(
                btnSalir,
                "Salir",
                marronClaro,
                Color.White);

            btnSalir.Location = new Point(155, 465);
            btnSalir.Size = new Size(140, 38);

            panelLogin.Controls.Add(lblBienvenida);
            panelLogin.Controls.Add(lblSubtitulo);
            panelLogin.Controls.Add(lblEmailNuevo);
            panelLogin.Controls.Add(txtEmail);
            panelLogin.Controls.Add(lblContrasenaNueva);
            panelLogin.Controls.Add(txtContrasena);
            panelLogin.Controls.Add(btnIngresar);
            panelLogin.Controls.Add(btnSalir);

            Controls.Add(panelImagen);
            Controls.Add(panelLogin);

            AcceptButton = btnIngresar;
            CancelButton = btnSalir;

            txtEmail.TabIndex = 0;
            txtContrasena.TabIndex = 1;
            btnIngresar.TabIndex = 2;
            btnSalir.TabIndex = 3;
        }

        private void EstilizarBoton(
            Button boton,
            string texto,
            Color fondo,
            Color textoColor)
        {
            boton.Text = texto;
            boton.BackColor = fondo;
            boton.ForeColor = textoColor;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(174, 139, 76);
            boton.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
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
                mensaje.ClientSize = new Size(380, 165);
                mensaje.BackColor = crema;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(380, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(350, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(340, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(135, 115),
                    Size = new Size(110, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };

                btnAceptar.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;
                mensaje.ShowDialog(this);
            }
        }


        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string contrasena = txtContrasena.Text;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasena))
                {
                    MostrarMensaje("Datos incompletos", "Ingresá tu correo electrónico y tu contraseña.", true);
                    return;
                }

                UsuarioNegocios negocio = new UsuarioNegocios();

                DataRow? usuario = negocio.Autenticar(email, contrasena);

                if (usuario == null)
                {
                    MostrarMensaje(
                        "Error de acceso",
                        "Email o contraseña incorrectos, o usuario inactivo.",
                        true);

                    return;
                }

                string nombre = Convert.ToString(usuario["nombre"]) ?? "";
                string apellido = Convert.ToString(usuario["apellido"]) ?? "";
                string rol = Convert.ToString(usuario["nombre_rol"]) ?? "";
                int idUsuario = Convert.ToInt32(usuario["id_usuario"]);

                MostrarMensaje(
                    "¡Acceso correcto!",
                    $"Bienvenido/a, {nombre} {apellido}.\nRol: {rol}");

                Form1 formularioPrincipal = new Form1(rol, idUsuario);
                formularioPrincipal.Show();
                Hide();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "Error de inicio de sesión",
                    "No pudimos iniciar sesión. Intentá nuevamente o consultá al responsable.",
                    true);
            }
        }


        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Se conserva por compatibilidad con el Designer.
        }
    }
}
