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

            Button[] botonesMenu =
            {
        btnHabitaciones,
        btnReservas,
        btnHuespedes,
        btnPagos,
        btnReportes
    };

            int[] posicionesY = { 160, 215, 270, 325, 380 };

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


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using SqlConnection conexion = ConexionBD.ObtenerConexion();
                conexion.Open();

                MessageBox.Show("Conexión exitosa con SQL Server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
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
                    MessageBox.Show("No hay roles registrados.");
                    return;
                }

                string mensaje = "";

                foreach (System.Data.DataRow fila in roles.Rows)
                {
                    mensaje += $"{fila["id_rol"]} - {fila["nombre_rol"]}\n";
                }

                MessageBox.Show(mensaje, "Roles registrados");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al listar los roles: " + ex.Message);
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

                dgvUsuarios.DataSource = null;
                dgvUsuarios.DataSource = usuarios;

                // Ocultar el ID interno
                if (dgvUsuarios.Columns.Contains("id_usuario"))
                    dgvUsuarios.Columns["id_usuario"].Visible = false;

                // Ocultar el ID del rol
                if (dgvUsuarios.Columns.Contains("id_rol"))
                    dgvUsuarios.Columns["id_rol"].Visible = false;

                // Cambiar el encabezado del rol
                if (dgvUsuarios.Columns.Contains("nombre_rol"))
                    dgvUsuarios.Columns["nombre_rol"].HeaderText = "Rol";

                // Mostrar el estado como texto
                if (dgvUsuarios.Columns.Contains("activo"))
                {
                    dgvUsuarios.Columns["activo"].Visible = false;

                    if (!dgvUsuarios.Columns.Contains("estadoVisual"))
                    {
                        DataGridViewTextBoxColumn columnaEstado =
                            new DataGridViewTextBoxColumn();

                        columnaEstado.Name = "estadoVisual";
                        columnaEstado.HeaderText = "Estado";
                        columnaEstado.ReadOnly = true;

                        dgvUsuarios.Columns.Add(columnaEstado);
                    }

                    foreach (DataGridViewRow fila in dgvUsuarios.Rows)
                    {
                        if (fila.IsNewRow)
                            continue;

                        object valor = fila.Cells["activo"].Value;

                        bool activo = valor != null
                            && valor != DBNull.Value
                            && Convert.ToBoolean(valor);

                        fila.Cells["estadoVisual"].Value =
                            activo ? "Activo" : "Inactivo";
                    }
                }

                // Nombres de columnas más claros
                if (dgvUsuarios.Columns.Contains("id_usuario"))
                    dgvUsuarios.Columns["id_usuario"].HeaderText = "ID";

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
                MessageBox.Show(
                    ex.Message,
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los usuarios: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                MessageBox.Show("Seleccione un usuario de la tabla.");
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
                MessageBox.Show("Seleccione un usuario de la tabla.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            string nombre = dgvUsuarios.CurrentRow.Cells["nombre"].Value?.ToString() ?? "";

            DialogResult respuesta = MessageBox.Show(
                $"¿Está seguro de desactivar al usuario {nombre}?",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                negocio.Desactivar(idUsuario, rolUsuario);

                MessageBox.Show("Usuario desactivado correctamente.");

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo desactivar el usuario: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnReactivarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null ||
                dgvUsuarios.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione un usuario de la tabla.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            string nombre = dgvUsuarios.CurrentRow.Cells["nombre"].Value?.ToString() ?? "";

            DialogResult respuesta = MessageBox.Show(
                $"¿Está seguro de reactivar al usuario {nombre}?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                negocio.Reactivar(idUsuario, rolUsuario);

                MessageBox.Show(
                    "Usuario reactivado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar el usuario: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
            FormHabitaciones formulario = new FormHabitaciones();
            formulario.ShowDialog();
        }

        private void btnReservas_Click(object sender, EventArgs e)
        {
            using (FormReservas formulario = new FormReservas(idUsuario))
            {
                formulario.ShowDialog();
            }
        }

        private void btnHuespedes_Click(object sender, EventArgs e)
        {
            using (FormHuespedes formulario = new FormHuespedes())
            {
                formulario.ShowDialog();
            }
        }

        private void btnPagos_Click(object sender, EventArgs e)
        {
            using (FormPagos formulario = new FormPagos(idUsuario))
            {
                formulario.ShowDialog();
            }
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            using (FormReportes formulario = new FormReportes())
            {
                formulario.ShowDialog(this);
            }
        }
    }
}