using System;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HotelTramonto.Presentacion
{
    public partial class Form1 : Form
    {
        private readonly string rolUsuario;
        private readonly int idUsuario;

        public Form1(string rol, int idUsuario)
        {
            InitializeComponent();
            AplicarEstilo();

            rolUsuario = rol.Trim().ToUpperInvariant();
            this.idUsuario = idUsuario;

            bool esAdministrador = rolUsuario == "ADMINISTRADOR";

            bool puedeVerReportes =
                rolUsuario == "ADMINISTRADOR" ||
                rolUsuario == "GERENTE";

            btnReportes.Visible = puedeVerReportes;
            bool operativo = PermisosOperacion.PuedeOperar(rolUsuario);
            btnPagos.Visible = operativo;
            btnReservas.Visible = operativo;
            btnHuespedes.Visible = operativo;
            btnServicios.Visible = esAdministrador;

            btnNuevoUsuario.Visible = esAdministrador;
            btnModificarUsuario.Visible = esAdministrador;
            btnDesactivarUsuario.Visible = esAdministrador;
            dgvUsuarios.Visible = esAdministrador;
            btnProbarConexion.Visible = esAdministrador;
            btnListarRoles.Visible = esAdministrador;
            btnReactivarUsuario.Visible = esAdministrador;

            CargarUsuarios();
        }


        private void AplicarEstilo()
        {
            // Paleta Hotel Tramonto
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color marfil = Color.FromArgb(255, 252, 247);
            Color dorado = Color.FromArgb(198, 161, 91);
            Color texto = Color.FromArgb(66, 51, 43);

            // Ventana
            Text = "Hotel Tramonto | Gestión";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);

            // Menú lateral
            Panel pnlMenu = new Panel
            {
                Name = "pnlMenu",
                BackColor = marron,
                Location = new Point(0, 0),
                Size = new Size(250, ClientSize.Height),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left
            };

            Controls.Add(pnlMenu);
            pnlMenu.SendToBack();

            // Separador dorado
            Panel lineaDorada = new Panel
            {
                Name = "lineaDorada",
                BackColor = dorado,
                Location = new Point(250, 0),
                Size = new Size(3, ClientSize.Height),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left
            };

            Controls.Add(lineaDorada);
            lineaDorada.BringToFront();

            // Marca del hotel en el menú
            Label lblMarca = new Label
            {
                Name = "lblMarcaHotel",
                Text = "HOTEL\nTRAMONTO",
                Font = new Font("Georgia", 21F, FontStyle.Bold),
                ForeColor = dorado,
                BackColor = marron,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(15, 28),
                Size = new Size(215, 78)
            };

            Controls.Add(lblMarca);
            lblMarca.BringToFront();

            Label lblSubMarca = new Label
            {
                Name = "lblSubMarca",
                Text = "GESTIÓN HOTELERA",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = crema,
                BackColor = marron,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(15, 105),
                Size = new Size(215, 25)
            };

            Controls.Add(lblSubMarca);
            lblSubMarca.BringToFront();

            // Título principal
            lblTitulo.Text = "HOTEL TRAMONTO";
            lblTitulo.Font = new Font("Georgia", 27F, FontStyle.Bold);
            lblTitulo.ForeColor = dorado;
            lblTitulo.BackColor = Color.Transparent;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(290, 28);

            // Sección de administración
            Label lblAdministracion = new Label
            {
                Name = "lblAdministracion",
                Text = "GESTIÓN DE USUARIOS",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = marron,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(290, 113)
            };

            Controls.Add(lblAdministracion);
            lblAdministracion.BringToFront();

            // Botones de navegación
            EstilizarBoton(btnHabitaciones, "Habitaciones");
            EstilizarBoton(btnReservas, "Reservas");
            EstilizarBoton(btnHuespedes, "Huéspedes");
            EstilizarBoton(btnPagos, "Pagos");
            EstilizarBoton(btnReportes, "Reportes");
            EstilizarBoton(btnServicios, "Servicios");

            Button[] botonesMenu =
            {
                btnHabitaciones,
                btnReservas,
                btnHuespedes,
                btnPagos,
                btnReportes,
                btnServicios
            };

            int[] posicionesY = { 160, 215, 270, 325, 380, 435 };

            for (int i = 0; i < botonesMenu.Length; i++)
            {
                botonesMenu[i].Location = new Point(25, posicionesY[i]);
                botonesMenu[i].Size = new Size(195, 42);
                botonesMenu[i].BackColor = marronClaro;
                botonesMenu[i].TextAlign = ContentAlignment.MiddleLeft;
                botonesMenu[i].Padding = new Padding(15, 0, 0, 0);
                botonesMenu[i].BringToFront();
            }

            // Cerrar sesión al pie del menú
            EstilizarBoton(btnCerrarSesion, "Cerrar sesión");
            btnCerrarSesion.Location = new Point(25, 455);
            btnCerrarSesion.Size = new Size(195, 42);
            btnCerrarSesion.BackColor = dorado;
            btnCerrarSesion.ForeColor = marron;
            btnCerrarSesion.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;
            btnCerrarSesion.Location = new Point(
                25, ClientSize.Height - 75);

            // Acciones administrativas
            EstilizarBoton(btnNuevoUsuario, "Nuevo usuario");
            EstilizarBoton(btnModificarUsuario, "Modificar usuario");
            EstilizarBoton(btnDesactivarUsuario, "Desactivar usuario");
            EstilizarBoton(btnReactivarUsuario, "Reactivar usuario");
            EstilizarBoton(btnProbarConexion, "Probar conexión");
            EstilizarBoton(btnListarRoles, "Listar roles");

            btnNuevoUsuario.Location = new Point(290, 150);
            btnModificarUsuario.Location = new Point(435, 150);
            btnDesactivarUsuario.Location = new Point(590, 150);
            btnReactivarUsuario.Location = new Point(745, 150);

            btnNuevoUsuario.Size = new Size(135, 40);
            btnModificarUsuario.Size = new Size(145, 40);
            btnDesactivarUsuario.Size = new Size(145, 40);
            btnReactivarUsuario.Size = new Size(145, 40);

            btnProbarConexion.Location = new Point(290, 200);
            btnListarRoles.Location = new Point(435, 200);
            btnProbarConexion.Size = new Size(135, 36);
            btnListarRoles.Size = new Size(145, 36);

            // Tabla de usuarios
            dgvUsuarios.Location = new Point(290, 255);
            dgvUsuarios.Size = new Size(
                Math.Max(400, ClientSize.Width - 325),
                Math.Max(200, ClientSize.Height - 285));

            dgvUsuarios.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvUsuarios.BackgroundColor = marfil;
            dgvUsuarios.BorderStyle = BorderStyle.None;
            dgvUsuarios.GridColor = Color.FromArgb(229, 218, 202);
            dgvUsuarios.EnableHeadersVisualStyles = false;
            dgvUsuarios.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsuarios.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None;
            dgvUsuarios.RowHeadersVisible = false;
            dgvUsuarios.RowTemplate.Height = 36;
            dgvUsuarios.ColumnHeadersHeight = 42;
            dgvUsuarios.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.MultiSelect = false;

            dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvUsuarios.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                marron;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                dorado;

            dgvUsuarios.DefaultCellStyle.BackColor = marfil;
            dgvUsuarios.DefaultCellStyle.ForeColor = texto;
            dgvUsuarios.DefaultCellStyle.Font =
                new Font("Segoe UI", 9.5F);
            dgvUsuarios.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);
            dgvUsuarios.DefaultCellStyle.SelectionForeColor = marron;
            dgvUsuarios.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            // El panel y las etiquetas quedan detrás de los controles
            pnlMenu.SendToBack();
            lblMarca.BringToFront();
            lblSubMarca.BringToFront();
            lblTitulo.BringToFront();
            lblAdministracion.BringToFront();
            lineaDorada.BringToFront();

            // Los botones y la tabla deben quedar por encima del fondo
            foreach (Button boton in botonesMenu)
                boton.BringToFront();

            btnCerrarSesion.BringToFront();
            btnNuevoUsuario.BringToFront();
            btnModificarUsuario.BringToFront();
            btnDesactivarUsuario.BringToFront();
            btnReactivarUsuario.BringToFront();
            btnProbarConexion.BringToFront();
            btnListarRoles.BringToFront();
            dgvUsuarios.BringToFront();
        }

        private void EstilizarBoton(Button boton, string texto)
        {
            boton.Text = texto;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(105, 73, 52);

            boton.BackColor = Color.FromArgb(74, 48, 39);
            boton.ForeColor = Color.FromArgb(255, 252, 247);
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.Size = new Size(135, 40);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
            boton.TextAlign = ContentAlignment.MiddleCenter;
        }


        private void MostrarMensaje(
            string titulo,
            string detalle,
            bool esError = false)
        {
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color dorado = Color.FromArgb(198, 161, 91);

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
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
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

        private bool MostrarConfirmacion(string titulo, string detalle)
        {
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color dorado = Color.FromArgb(198, 161, 91);

            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(400, 180);
                mensaje.BackColor = crema;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(400, 7),
                    BackColor = dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(370, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
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

                Button btnSi = new Button
                {
                    Text = "Sí",
                    Location = new Point(85, 120),
                    Size = new Size(100, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand
                };

                Button btnNo = new Button
                {
                    Text = "No",
                    Location = new Point(215, 120),
                    Size = new Size(100, 35),
                    BackColor = marronClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand
                };

                btnSi.FlatAppearance.BorderSize = 0;
                btnNo.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnSi);
                mensaje.Controls.Add(btnNo);

                mensaje.AcceptButton = btnSi;
                mensaje.CancelButton = btnNo;

                return mensaje.ShowDialog(this) == DialogResult.Yes;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using SqlConnection conexion = ConexionBD.ObtenerConexion();
                conexion.Open();

                MostrarMensaje("Conexión exitosa", "La conexión con SQL Server se estableció correctamente.");
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error de conexión", "No se pudo conectar con SQL Server.\n\nDetalle: " + ex.Message, true);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                RolDatos datos = new RolDatos();
                var roles = datos.Listar();

                if (roles.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Sin roles registrados",
                        "No hay roles registrados en el sistema.");
                    return;
                }

                // Ordenar los roles por su identificador, de menor a mayor.
                System.Data.DataView vista = roles.DefaultView;
                vista.Sort = "id_rol ASC";
                roles = vista.ToTable();

                string mensaje = "";

                foreach (System.Data.DataRow fila in roles.Rows)
                {
                    mensaje += $"{fila["id_rol"]} - {fila["nombre_rol"]}\n";
                }

                MostrarMensaje("Roles registrados", mensaje);
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al listar los roles",
                    "No se pudieron listar los roles.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

        private void CargarUsuarios()
        {
            if (rolUsuario != "ADMINISTRADOR")
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                DataTable usuarios = negocio.Listar(rolUsuario);

                // Crear la columna visual del estado antes de cargar la tabla.
                if (usuarios.Columns.Contains("activo"))
                {
                    if (!usuarios.Columns.Contains("estadoVisual"))
                    {
                        usuarios.Columns.Add("estadoVisual", typeof(string));
                    }

                    foreach (DataRow fila in usuarios.Rows)
                    {
                        bool activo = fila["activo"] != DBNull.Value &&
                                      Convert.ToBoolean(fila["activo"]);

                        fila["estadoVisual"] = activo ? "Activo" : "Inactivo";
                    }
                }

                // Cargar los datos en la tabla.
                dgvUsuarios.DataSource = null;
                dgvUsuarios.DataSource = usuarios;

                // Ocultar identificadores internos.
                if (dgvUsuarios.Columns.Contains("id_usuario"))
                    dgvUsuarios.Columns["id_usuario"].Visible = false;

                if (dgvUsuarios.Columns.Contains("id_rol"))
                    dgvUsuarios.Columns["id_rol"].Visible = false;

                if (dgvUsuarios.Columns.Contains("activo"))
                    dgvUsuarios.Columns["activo"].Visible = false;

                // Configurar columna Estado.
                if (dgvUsuarios.Columns.Contains("estadoVisual"))
                {
                    dgvUsuarios.Columns["estadoVisual"].HeaderText = "Estado";
                    dgvUsuarios.Columns["estadoVisual"].ReadOnly = true;
                    dgvUsuarios.Columns["estadoVisual"].DisplayIndex = 0;
                }

                // Encabezados.
                if (dgvUsuarios.Columns.Contains("nombre_rol"))
                    dgvUsuarios.Columns["nombre_rol"].HeaderText = "Rol";

                if (dgvUsuarios.Columns.Contains("nombre"))
                    dgvUsuarios.Columns["nombre"].HeaderText = "Nombre";

                if (dgvUsuarios.Columns.Contains("apellido"))
                    dgvUsuarios.Columns["apellido"].HeaderText = "Apellido";

                if (dgvUsuarios.Columns.Contains("dni"))
                    dgvUsuarios.Columns["dni"].HeaderText = "DNI";

                if (dgvUsuarios.Columns.Contains("email"))
                    dgvUsuarios.Columns["email"].HeaderText = "Email";

                if (dgvUsuarios.Columns.Contains("telefono"))
                    dgvUsuarios.Columns["telefono"].HeaderText = "Teléfono";

                if (dgvUsuarios.Columns.Contains("fecha_alta"))
                    dgvUsuarios.Columns["fecha_alta"].HeaderText = "Fecha de alta";
            }
            catch (UnauthorizedAccessException ex)
            {
                MostrarMensaje(
                    "Acceso denegado",
                    ex.Message);
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar los usuarios",
                    "No se pudieron cargar los usuarios.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnNuevoUsuario_Click(object sender, EventArgs e)
        {
            using FormUsuario formulario = new FormUsuario(rolUsuario);

            if (formulario.ShowDialog(this) == DialogResult.OK)
            {
                CargarUsuarios();
            }
        }

        private void btnModificarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null ||
                dgvUsuarios.CurrentRow.IsNewRow)
            {
                MostrarMensaje("Usuario no seleccionado", "Seleccioná un usuario de la tabla para continuar.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            using FormModificarUsuario formulario = new FormModificarUsuario(idUsuario, rolUsuario);

            if (formulario.ShowDialog(this) == DialogResult.OK)
            {
                CargarUsuarios();
            }
        }

        private void btnDesactivarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null ||
                dgvUsuarios.CurrentRow.IsNewRow)
            {
                MostrarMensaje("Usuario no seleccionado", "Seleccioná un usuario de la tabla para continuar.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            string nombre = dgvUsuarios.CurrentRow.Cells["nombre"].Value?.ToString() ?? "";

            bool respuesta = MostrarConfirmacion(
                "Confirmar desactivación",
                $"¿Querés desactivar al usuario {nombre}?");

            if (!respuesta)
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                negocio.Desactivar(idUsuario, rolUsuario);

                MostrarMensaje("Usuario desactivado", "El usuario se desactivó correctamente.");

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al desactivar usuario",
                    "No se pudo desactivar el usuario.\n\nDetalle: " + ex.Message,
                    true);
            }
        }


        private void btnReactivarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null ||
                dgvUsuarios.CurrentRow.IsNewRow)
            {
                MostrarMensaje("Usuario no seleccionado", "Seleccioná un usuario de la tabla para continuar.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            string nombre = dgvUsuarios.CurrentRow.Cells["nombre"].Value?.ToString() ?? "";

            bool respuesta = MostrarConfirmacion(
                "Confirmar reactivación",
                $"¿Querés reactivar al usuario {nombre}?");

            if (!respuesta)
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                negocio.Reactivar(idUsuario, rolUsuario);

                MostrarMensaje(
                    "Usuario reactivado",
                    "El usuario se reactivó correctamente.");

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al reactivar usuario",
                    "No se pudo reactivar el usuario.\n\nDetalle: " + ex.Message,
                    true);
            }
        }


        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            FormLogin login = new FormLogin();
            login.Show();
            this.Close();
        }


        private void btnHabitaciones_Click(object sender, EventArgs e)
        {
            using (FormHabitaciones formulario =
                new FormHabitaciones(rolUsuario))
            {
                formulario.ShowDialog(this);
            }
        }


        private void btnReservas_Click(object sender, EventArgs e)
        {
            using (FormReservas formulario = new FormReservas(idUsuario, rolUsuario))
            {
                formulario.ShowDialog();
            }
        }

        private void btnHuespedes_Click(object sender, EventArgs e)
        {
            using (FormHuespedes formulario = new FormHuespedes(rolUsuario))
            {
                formulario.ShowDialog();
            }
        }

        private void btnPagos_Click(object sender, EventArgs e)
        {
            using (FormPagos formulario = new FormPagos(idUsuario, rolUsuario))
            {
                formulario.ShowDialog();
            }
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            using (FormReportes formulario = new FormReportes(rolUsuario))
            {
                formulario.ShowDialog(this);
            }
        }

        private void btnServicios_Click(object sender, EventArgs e)
        {
            if (rolUsuario != "ADMINISTRADOR")
            {
                MostrarMensaje(
                    "Acceso denegado",
                    "No tenés permisos para gestionar los servicios.",
                    true);
                return;
            }

            using (FormServicios formulario = new FormServicios(rolUsuario))
            {
                formulario.ShowDialog(this);
            }
        }
    }
}