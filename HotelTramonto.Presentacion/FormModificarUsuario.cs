
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using HotelTramonto.Datos;

namespace HotelTramonto.Presentacion
{
    public partial class FormModificarUsuario : Form
    {
        private readonly int idUsuario;
        private readonly string rolUsuario;

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        public FormModificarUsuario(int idUsuario, string rol)
        {
            InitializeComponent();

            this.idUsuario = idUsuario;
            rolUsuario = rol;

            AplicarEstilo();
            CargarRoles();
            CargarDatosUsuario();
        }

        private void AplicarEstilo()
        {
            Text = "Modificar usuario - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);

            Panel panelTitulo = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 90),
                BackColor = marron
            };

            Label lblEncabezado = new Label
            {
                Text = "MODIFICAR USUARIO",
                Location = new Point(25, 14),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold)
            };

            Label lblSubtitulo = new Label
            {
                Text = "Actualizá los datos del usuario",
                Location = new Point(28, 55),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 9.5F)
            };

            panelTitulo.Controls.Add(lblEncabezado);
            panelTitulo.Controls.Add(lblSubtitulo);
            Controls.Add(panelTitulo);
            panelTitulo.BringToFront();

            Panel panelDatos = new Panel
            {
                Location = new Point(20, 105),
                Size = new Size(500, 305),
                BackColor = marfil,
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(panelDatos);

            Label[] etiquetas =
            {
                label1, label2, label3, label4, label5, label6
            };

            Control[] campos =
            {
                txtNombre, txtApellido, txtDni,
                txtEmail, txtTelefono, cmbRol
            };

            string[] nombres =
            {
                "Nombre", "Apellido", "DNI",
                "Email", "Teléfono", "Rol"
            };

            for (int i = 0; i < etiquetas.Length; i++)
            {
                etiquetas[i].Text = nombres[i];
                etiquetas[i].Location = new Point(25, 18 + i * 44);
                etiquetas[i].AutoSize = true;
                etiquetas[i].ForeColor = marron;
                etiquetas[i].Font = new Font(
                    "Segoe UI", 10F, FontStyle.Bold);

                campos[i].Location = new Point(145, 14 + i * 44);
                campos[i].Size = new Size(320, 28);
                campos[i].Font = new Font("Segoe UI", 10F);
                campos[i].ForeColor = marron;
                campos[i].BackColor = marfil;

                panelDatos.Controls.Add(etiquetas[i]);
                panelDatos.Controls.Add(campos[i]);
            }

            txtNombre.BorderStyle = BorderStyle.FixedSingle;
            txtApellido.BorderStyle = BorderStyle.FixedSingle;
            txtDni.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtTelefono.BorderStyle = BorderStyle.FixedSingle;

            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;

            EstilizarBoton(
                btnGuardarCambios,
                "Guardar cambios",
                dorado,
                marron);

            btnGuardarCambios.Location = new Point(185, 430);
            btnGuardarCambios.Size = new Size(165, 40);


            Button btnCambiarContrasena = new Button();

            EstilizarBoton(
                btnCambiarContrasena,
                "Cambiar contraseña",
                marronClaro,
                Color.White);

            btnCambiarContrasena.Location = new Point(20, 430);
            btnCambiarContrasena.Size = new Size(155, 40);

            btnCambiarContrasena.Click += (s, e) =>
            {
                using (FormCambiarContrasena formulario =
                    new FormCambiarContrasena(idUsuario))
                {
                    formulario.ShowDialog(this);
                }
            };
            Controls.Add(btnCambiarContrasena);


            EstilizarBoton(
                btnCancelar,
                "Cancelar",
                marronClaro,
                Color.White);

            btnCancelar.Location = new Point(360, 430);
            btnCancelar.Size = new Size(160, 40);

            btnCancelar.Click -= btnCancelar_Click;
            btnCancelar.Click += btnCancelar_Click;

            AcceptButton = btnGuardarCambios;
            CancelButton = btnCancelar;

            ClientSize = new Size(540, 490);
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

                Label lblTituloMensaje = new Label
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
                    Size = new Size(360, 45),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(145, 120),
                    Size = new Size(110, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI", 9F, FontStyle.Bold),
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
                    "Error al cargar los roles",
                    "No se pudieron cargar los roles.\n\nDetalle: "
                    + ex.Message,
                    true);
            }
        }

        private void CargarDatosUsuario()
        {
            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                DataTable usuarios = negocio.Listar(rolUsuario);

                DataRow[] filas = usuarios.Select(
                    $"id_usuario = {idUsuario}");

                if (filas.Length == 0)
                {
                    MostrarMensaje(
                        "Usuario no encontrado",
                        "No se encontró el usuario seleccionado.",
                        true);

                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                DataRow usuario = filas[0];

                txtNombre.Text = usuario["nombre"].ToString();
                txtApellido.Text = usuario["apellido"].ToString();
                txtDni.Text = usuario["dni"].ToString();
                txtEmail.Text = usuario["email"].ToString();

                txtTelefono.Text =
                    usuario["telefono"] == DBNull.Value
                        ? ""
                        : usuario["telefono"].ToString();

                cmbRol.SelectedValue =
                    Convert.ToInt32(usuario["id_rol"]);
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar el usuario",
                    "No se pudieron cargar los datos del usuario.\n\nDetalle: "
                    + ex.Message,
                    true);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // Se conserva el evento existente del Designer.
        }

        private void btnGuardarCambios_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbRol.SelectedIndex == -1)
                {
                    MostrarMensaje(
                        "Rol requerido",
                        "Seleccioná un rol para el usuario.");

                    cmbRol.Focus();
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
                    txtTelefono.Text,
                    rolUsuario);

                MostrarMensaje(
                    "Usuario modificado",
                    "Los datos del usuario se actualizaron correctamente.");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al modificar el usuario",
                    "No se pudieron guardar los cambios.\n\nDetalle: "
                    + ex.Message,
                    true);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
