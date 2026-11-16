
using System;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using System.Drawing;

namespace HotelTramonto.Presentacion
{
    public partial class FormHabitaciones : Form
    {
        // Rol del usuario que inició sesión.
        private readonly string rolUsuario;

        private bool EsAdministrador =>
            PermisosHabitaciones.PuedeAdministrar(rolUsuario);

        // Constructor para el diseñador de Windows Forms.
        public FormHabitaciones() : this(string.Empty)
        {
        }

        public FormHabitaciones(string rolUsuario)
        {
            this.rolUsuario = rolUsuario?.Trim() ?? string.Empty;

            InitializeComponent();
            PresentacionTablas.Conectar(this);
            AplicarEstilo();
            ConfigurarPermisos();
            CargarHabitaciones();
        }

        private void ConfigurarPermisos()
        {
            // Solo el administrador puede gestionar habitaciones.
            btnNuevaHabitacion.Visible = EsAdministrador;
            btnModificarHabitacion.Visible = EsAdministrador;
            btnDesactivarHabitacion.Visible = EsAdministrador;
            btnReactivarHabitacion.Visible = EsAdministrador;
        }

        private bool TienePermisoDeGestion()
        {
            if (EsAdministrador)
                return true;

            MostrarMensaje(
                "Acceso denegado",
                "Solo el administrador puede gestionar habitaciones.",
                true);

            return false;
        }

        private void AplicarEstilo()
        {
            // Paleta Hotel Tramonto.
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color marfil = Color.FromArgb(255, 252, 247);
            Color dorado = Color.FromArgb(198, 161, 91);
            Color texto = Color.FromArgb(66, 51, 43);

            // Ventana.
            Text = "Hotel Tramonto | Habitaciones";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);

            // Título.
            Label lblTitulo = new Label
            {
                Name = "lblTituloHabitaciones",
                Text = "GESTIÓN DE HABITACIONES",
                Font = new Font("Georgia", 25F, FontStyle.Bold),
                ForeColor = dorado,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(35, 30)
            };

            // Botones.
            EstilizarBotonHabitacion(
                btnNuevaHabitacion,
                "＋ Nueva habitación",
                dorado,
                marron);

            EstilizarBotonHabitacion(
                btnModificarHabitacion,
                "Modificar habitación",
                marron,
                marfil);

            EstilizarBotonHabitacion(
                btnDesactivarHabitacion,
                "Desactivar habitación",
                marronClaro,
                marfil);

            EstilizarBotonHabitacion(
                btnReactivarHabitacion,
                "Reactivar habitación",
                marronClaro,
                marfil);

            btnNuevaHabitacion.Location = new Point(35, 100);
            btnNuevaHabitacion.Size = new Size(180, 42);

            btnModificarHabitacion.Location = new Point(230, 100);
            btnModificarHabitacion.Size = new Size(180, 42);

            btnDesactivarHabitacion.Location = new Point(425, 100);
            btnDesactivarHabitacion.Size = new Size(190, 42);

            btnReactivarHabitacion.Location = new Point(630, 100);
            btnReactivarHabitacion.Size = new Size(180, 42);

            // Tabla de habitaciones.
            dgvHabitaciones.Location = new Point(35, 170);

            dgvHabitaciones.Size = new Size(
                ClientSize.Width - 70,
                ClientSize.Height - 205);

            dgvHabitaciones.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvHabitaciones.ReadOnly = true;
            dgvHabitaciones.AllowUserToAddRows = false;
            dgvHabitaciones.AllowUserToDeleteRows = false;

            dgvHabitaciones.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvHabitaciones.MultiSelect = false;
            dgvHabitaciones.RowHeadersVisible = false;
            dgvHabitaciones.ScrollBars = ScrollBars.Both;

            dgvHabitaciones.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvHabitaciones.RowTemplate.Height = 36;
            dgvHabitaciones.ColumnHeadersHeight = 42;

            dgvHabitaciones.BackgroundColor = marfil;
            dgvHabitaciones.BorderStyle = BorderStyle.None;

            dgvHabitaciones.GridColor =
                Color.FromArgb(229, 218, 202);

            dgvHabitaciones.EnableHeadersVisualStyles = false;

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.BackColor =
                marron;

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.ForeColor =
                dorado;

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                marron;

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                dorado;

            dgvHabitaciones.DefaultCellStyle.BackColor = marfil;
            dgvHabitaciones.DefaultCellStyle.ForeColor = texto;

            dgvHabitaciones.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F);

            dgvHabitaciones.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvHabitaciones.DefaultCellStyle.SelectionForeColor =
                marron;

            dgvHabitaciones.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            // Mantener el título visible.
            Controls.Add(lblTitulo);
            lblTitulo.BringToFront();
        }

        private void EstilizarBotonHabitacion(
            Button boton,
            string textoBoton,
            Color fondo,
            Color colorTexto)
        {
            boton.Text = textoBoton;
            boton.Font = new Font(
                "Segoe UI", 9.5F, FontStyle.Bold);

            boton.BackColor = fondo;
            boton.ForeColor = colorTexto;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;

            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);

            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(105, 73, 52);

            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
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

                mensaje.StartPosition =
                    FormStartPosition.CenterParent;

                mensaje.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

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

                Label lblTitulo = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(350, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font(
                        "Segoe UI", 12F, FontStyle.Bold)
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
                    Font = new Font(
                        "Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };

                btnAceptar.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTitulo);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;
                mensaje.ShowDialog(this);
            }
        }

        private bool MostrarConfirmacion(
            string titulo,
            string detalle)
        {
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color dorado = Color.FromArgb(198, 161, 91);

            bool confirmado = false;

            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";

                mensaje.StartPosition =
                    FormStartPosition.CenterParent;

                mensaje.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

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
                    Size = new Size(360, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnSi = new Button
                {
                    Text = "Sí",
                    Location = new Point(85, 115),
                    Size = new Size(100, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand
                };

                Button btnNo = new Button
                {
                    Text = "No",
                    Location = new Point(215, 115),
                    Size = new Size(100, 35),
                    BackColor = marronClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand
                };

                btnSi.FlatAppearance.BorderSize = 0;
                btnNo.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTitulo);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnSi);
                mensaje.Controls.Add(btnNo);

                mensaje.AcceptButton = btnSi;
                mensaje.CancelButton = btnNo;

                confirmado =
                    mensaje.ShowDialog(this) == DialogResult.Yes;
            }

            return confirmado;
        }

        private void CargarHabitaciones()
        {
            try
            {
                HabitacionNegocios negocio =
                    new HabitacionNegocios();

                dgvHabitaciones.DataSource = negocio.Listar();

                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar las habitaciones",
                    "No se pudieron cargar las habitaciones.\n\nDetalle: "
                    + RegistroErrores.Mensaje(ex),
                    true);
            }
        }

        private void btnNuevaHabitacion_Click(
            object sender,
            EventArgs e)
        {
            if (!TienePermisoDeGestion())
                return;

            using (FormNuevaHabitacion formulario =
                new FormNuevaHabitacion(rolUsuario))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHabitaciones();
                }
            }
        }

        private void btnModificarHabitacion_Click(
            object sender,
            EventArgs e)
        {
            if (!TienePermisoDeGestion())
                return;

            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MostrarMensaje(
                    "Habitación no seleccionada",
                    "Seleccioná una habitación de la tabla para continuar.");

                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow
                    .Cells["id_habitacion"].Value);

            using (FormModificarHabitacion formulario =
                new FormModificarHabitacion(
                    idHabitacion,
                    rolUsuario))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHabitaciones();
                }
            }
        }

        private void btnDesactivarHabitacion_Click(
            object sender,
            EventArgs e)
        {
            if (!TienePermisoDeGestion())
                return;

            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MostrarMensaje(
                    "Habitación no seleccionada",
                    "Seleccioná una habitación de la tabla para continuar.");

                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow
                    .Cells["id_habitacion"].Value);

            bool confirmacion = MostrarConfirmacion(
                "Confirmar desactivación",
                "¿Estás seguro de que querés desactivar esta habitación?");

            if (!confirmacion)
                return;

            try
            {
                HabitacionNegocios negocio =
                    new HabitacionNegocios();

                negocio.Desactivar(
                    idHabitacion,
                    rolUsuario);

                MostrarMensaje(
                    "Habitación desactivada",
                    "La habitación se desactivó correctamente.");

                CargarHabitaciones();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al desactivar",
                    "No se pudo desactivar la habitación.\n\nDetalle: "
                    + RegistroErrores.Mensaje(ex),
                    true);
            }
        }

        private void btnReactivarHabitacion_Click(
            object sender,
            EventArgs e)
        {
            if (!TienePermisoDeGestion())
                return;

            try
            {
                // La tabla principal contiene solo activas. Elegir la habitación
                // en el formulario de inactivas, con el mismo rol autenticado.
                using (FormReactivarHabitacion formulario = new FormReactivarHabitacion(rolUsuario))
                {
                    formulario.ShowDialog(this);
                }
                CargarHabitaciones();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al reactivar", "No se pudo abrir la lista de habitaciones inactivas.\n\n" + RegistroErrores.Mensaje(ex), true);
            }
        }

        private void ActualizarBotones()
        {
            if (!EsAdministrador)
            {
                btnNuevaHabitacion.Enabled = false;
                btnModificarHabitacion.Enabled = false;
                btnDesactivarHabitacion.Enabled = false;
                btnReactivarHabitacion.Enabled = false;
                return;
            }

            btnNuevaHabitacion.Enabled = true;
            btnReactivarHabitacion.Enabled = true;

            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                btnModificarHabitacion.Enabled = false;
                btnDesactivarHabitacion.Enabled = false;
                return;
            }

            bool activa = Convert.ToBoolean(
                dgvHabitaciones.CurrentRow
                    .Cells["activo"].Value);

            btnModificarHabitacion.Enabled = activa;
            btnDesactivarHabitacion.Enabled = activa;
            // Reactivar usa su propia lista; no depende de seleccionar una activa.
        }

        private void dgvHabitaciones_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ActualizarBotones();
        }
    }
}
