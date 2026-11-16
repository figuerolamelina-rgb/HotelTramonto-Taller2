
using HotelTramonto.Negocios;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace HotelTramonto.Presentacion
{
    public partial class FormHabitaciones : Form
    {
        private readonly string rolUsuario;

        private ComboBox cmbEstado;
        private ComboBox cmbTipo;
        private ComboBox cmbPiso;
        private CheckBox chkSoloActivas;
        private Button btnLimpiarFiltros;

        private DataTable habitacionesOriginales;

        public FormHabitaciones() : this("RECEPCIONISTA")
        {
        }

        public FormHabitaciones(string rol)
        {
            InitializeComponent();

            rolUsuario = (rol ?? "").Trim().ToUpperInvariant();

            AplicarEstilo();
            AplicarPermisos();
            CargarHabitaciones();
        }

        private void AplicarPermisos()
        {
            bool esRecepcionista = rolUsuario == "RECEPCIONISTA";

            // El recepcionista solamente puede consultar.
            btnNuevaHabitacion.Visible = !esRecepcionista;
            btnModificarHabitacion.Visible = !esRecepcionista;
            btnDesactivarHabitacion.Visible = !esRecepcionista;
            btnReactivarHabitacion.Visible = !esRecepcionista;
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
            Text = "Hotel Tramonto | Habitaciones";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);

            // Título
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

            // Botones
            EstilizarBotonHabitacion(
                btnNuevaHabitacion, "＋ Nueva habitación", dorado, marron);

            EstilizarBotonHabitacion(
                btnModificarHabitacion, "Modificar habitación", marron, marfil);

            EstilizarBotonHabitacion(
                btnDesactivarHabitacion, "Desactivar habitación", marronClaro, marfil);

            EstilizarBotonHabitacion(
                btnReactivarHabitacion, "Reactivar habitación", marronClaro, marfil);

            btnNuevaHabitacion.Location = new Point(35, 100);
            btnNuevaHabitacion.Size = new Size(180, 42);

            btnModificarHabitacion.Location = new Point(230, 100);
            btnModificarHabitacion.Size = new Size(180, 42);

            btnDesactivarHabitacion.Location = new Point(425, 100);
            btnDesactivarHabitacion.Size = new Size(190, 42);

            btnReactivarHabitacion.Location = new Point(630, 100);
            btnReactivarHabitacion.Size = new Size(180, 42);


            // ============================================
            // FILTROS DE HABITACIONES
            // ============================================

            Label lblFiltro = new Label
            {
                Text = "FILTRAR HABITACIONES:",
                Location = new Point(35, 165),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = marron
            };

            Label lblEstado = new Label
            {
                Text = "Estado:",
                Location = new Point(35, 200),
                AutoSize = true,
                ForeColor = texto
            };

            cmbEstado = new ComboBox
            {
                Location = new Point(95, 195),
                Size = new Size(170, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbEstado.Items.AddRange(new object[]
            {
    "Todos",
    "Disponibles",
    "Ocupadas",
    "Mantenimiento"
            });

            cmbEstado.SelectedIndex = 0;

            Label lblTipo = new Label
            {
                Text = "Tipo:",
                Location = new Point(290, 200),
                AutoSize = true,
                ForeColor = texto
            };

            cmbTipo = new ComboBox
            {
                Location = new Point(335, 195),
                Size = new Size(160, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbTipo.Items.Add("Todos");
            cmbTipo.SelectedIndex = 0;

            Label lblPiso = new Label
            {
                Text = "Piso:",
                Location = new Point(520, 200),
                AutoSize = true,
                ForeColor = texto
            };

            cmbPiso = new ComboBox
            {
                Location = new Point(565, 195),
                Size = new Size(110, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbPiso.Items.Add("Todos");
            cmbPiso.SelectedIndex = 0;

            chkSoloActivas = new CheckBox
            {
                Text = "Solo habitaciones activas",
                Location = new Point(700, 198),
                AutoSize = true,
                Checked = true,
                ForeColor = texto
            };

            btnLimpiarFiltros = new Button
            {
                Text = "Limpiar filtros",
                Location = new Point(930, 190),
                Size = new Size(130, 35)
            };

            EstilizarBotonHabitacion(
                btnLimpiarFiltros, "Limpiar filtros", marronClaro, marfil);

            Controls.Add(lblFiltro);
            Controls.Add(lblEstado);
            Controls.Add(cmbEstado);
            Controls.Add(lblTipo);
            Controls.Add(cmbTipo);
            Controls.Add(lblPiso);
            Controls.Add(cmbPiso);
            Controls.Add(chkSoloActivas);
            Controls.Add(btnLimpiarFiltros);

            cmbEstado.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cmbTipo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cmbPiso.SelectedIndexChanged += (s, e) => AplicarFiltros();

            chkSoloActivas.CheckedChanged += (s, e) => AplicarFiltros();

            btnLimpiarFiltros.Click += (s, e) =>
            {
                cmbEstado.SelectedIndex = 0;
                cmbTipo.SelectedIndex = 0;
                cmbPiso.SelectedIndex = 0;
                chkSoloActivas.Checked = true;

                AplicarFiltros();
            };

            // Tabla de habitaciones
            dgvHabitaciones.Location = new Point(35, 245);
            dgvHabitaciones.Size = new Size(
                ClientSize.Width - 70,
                ClientSize.Height - 280);

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
            dgvHabitaciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHabitaciones.RowTemplate.Height = 36;
            dgvHabitaciones.ColumnHeadersHeight = 42;

            dgvHabitaciones.BackgroundColor = marfil;
            dgvHabitaciones.BorderStyle = BorderStyle.None;
            dgvHabitaciones.GridColor = Color.FromArgb(229, 218, 202);
            dgvHabitaciones.EnableHeadersVisualStyles = false;

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvHabitaciones.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvHabitaciones.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvHabitaciones.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvHabitaciones.DefaultCellStyle.BackColor = marfil;
            dgvHabitaciones.DefaultCellStyle.ForeColor = texto;
            dgvHabitaciones.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgvHabitaciones.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvHabitaciones.DefaultCellStyle.SelectionForeColor = marron;

            dgvHabitaciones.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            // Mantener el título visible
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
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
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

                Label lblTitulo = new Label
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
                mensaje.Controls.Add(lblTitulo);
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
            bool confirmado = false;

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

                Label lblTitulo = new Label
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
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
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
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
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

                confirmado = mensaje.ShowDialog(this) == DialogResult.Yes;
            }

            return confirmado;
        }


        private void CargarHabitaciones()
        {
            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();

                // Guardar los datos originales para filtrar sin consultar SQL nuevamente.
                habitacionesOriginales = negocio.Listar();

                CargarOpcionesFiltros();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar las habitaciones",
                    "No se pudieron cargar las habitaciones.\n\nDetalle: "
                    + ex.Message,
                    true);
            }
        }


        private void CargarOpcionesFiltros()
        {
            cmbTipo.Items.Clear();
            cmbPiso.Items.Clear();

            cmbTipo.Items.Add("Todos");
            cmbPiso.Items.Add("Todos");

            List<string> tipos = new List<string>();
            List<string> pisos = new List<string>();

            foreach (DataRow fila in habitacionesOriginales.Rows)
            {
                string tipo = fila["tipo"]?.ToString()?.Trim() ?? "";
                string piso = fila["piso"]?.ToString()?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(tipo) &&
                    !tipos.Contains(tipo))
                {
                    tipos.Add(tipo);
                }

                if (!string.IsNullOrWhiteSpace(piso) &&
                    !pisos.Contains(piso))
                {
                    pisos.Add(piso);
                }
            }

            tipos.Sort();
            pisos.Sort();

            foreach (string tipo in tipos)
                cmbTipo.Items.Add(tipo);

            foreach (string piso in pisos)
                cmbPiso.Items.Add(piso);

            cmbTipo.SelectedIndex = 0;
            cmbPiso.SelectedIndex = 0;
        }


        private void AplicarFiltros()
        {
            if (habitacionesOriginales == null)
                return;

            DataTable filtrada = habitacionesOriginales.Clone();

            string estadoSeleccionado =
                cmbEstado.SelectedItem?.ToString() ?? "Todos";

            string tipoSeleccionado =
                cmbTipo.SelectedItem?.ToString() ?? "Todos";

            string pisoSeleccionado =
                cmbPiso.SelectedItem?.ToString() ?? "Todos";

            bool soloActivas = chkSoloActivas.Checked;

            foreach (DataRow fila in habitacionesOriginales.Rows)
            {
                bool activa = Convert.ToBoolean(fila["activo"]);

                string estado = fila["estado"]?.ToString()?.Trim() ?? "";
                string tipo = fila["tipo"]?.ToString()?.Trim() ?? "";
                string piso = fila["piso"]?.ToString()?.Trim() ?? "";

                // Filtro de habitaciones activas.
                if (soloActivas && !activa)
                    continue;

                // Filtro de estado.
                if (estadoSeleccionado == "Disponibles")
                {
                    if (!activa ||
                        !estado.Equals("DISPONIBLE",
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                else if (estadoSeleccionado == "Ocupadas")
                {
                    if (!estado.Equals("OCUPADA",
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                else if (estadoSeleccionado == "Mantenimiento")
                {
                    if (!estado.Equals("MANTENIMIENTO",
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                // Filtro de tipo.
                if (tipoSeleccionado != "Todos" &&
                    !tipo.Equals(tipoSeleccionado,
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                // Filtro de piso.
                if (pisoSeleccionado != "Todos" &&
                    piso != pisoSeleccionado)
                    continue;

                filtrada.ImportRow(fila);
            }

            dgvHabitaciones.DataSource = filtrada;

            ConfigurarColumnasHabitaciones();
            ActualizarBotones();
        }


        private void ConfigurarColumnasHabitaciones()
        {
            if (dgvHabitaciones.Columns.Count == 0)
                return;

            dgvHabitaciones.Columns["id_habitacion"].Visible = false;
            dgvHabitaciones.Columns["tipo_habitacion"].Visible = false;

            dgvHabitaciones.Columns["tipo"].HeaderText = "Tipo de habitación";
            dgvHabitaciones.Columns["piso"].HeaderText = "Piso";
            dgvHabitaciones.Columns["numero"].HeaderText = "N.º de habitación";
            dgvHabitaciones.Columns["estado"].HeaderText = "Estado";
            dgvHabitaciones.Columns["activo"].HeaderText = "Activa";

            dgvHabitaciones.Columns["tipo"].DisplayIndex = 0;
            dgvHabitaciones.Columns["piso"].DisplayIndex = 1;
            dgvHabitaciones.Columns["numero"].DisplayIndex = 2;
            dgvHabitaciones.Columns["estado"].DisplayIndex = 3;
            dgvHabitaciones.Columns["activo"].DisplayIndex = 4;
        }


        private void btnNuevaHabitacion_Click(object sender, EventArgs e)
        {
            if (!PuedeModificarHabitaciones())
                return;

            using (FormNuevaHabitacion formulario = new FormNuevaHabitacion())
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHabitaciones();
                }
            }
        }


        private void btnModificarHabitacion_Click(object sender, EventArgs e)
        {
            if (!PuedeModificarHabitaciones())
                return;

            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MostrarMensaje("Habitación no seleccionada", "Seleccioná una habitación de la tabla para continuar.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow.Cells["id_habitacion"].Value);

            using (FormModificarHabitacion formulario =
                new FormModificarHabitacion(idHabitacion))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHabitaciones();
                }
            }
        }

        private void btnDesactivarHabitacion_Click(object sender, EventArgs e)
        {
            if (!PuedeModificarHabitaciones())
                return;

            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MostrarMensaje("Habitación no seleccionada", "Seleccioná una habitación de la tabla para continuar.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow.Cells["id_habitacion"].Value);

            bool confirmacion = MostrarConfirmacion(
                                "Confirmar desactivación",
                                "¿Estás seguro de que querés desactivar esta habitación?");

            if (!confirmacion)
                return;
           

            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                negocio.Desactivar(idHabitacion);

                MostrarMensaje("Habitación desactivada", "La habitación se desactivó correctamente.");

                CargarHabitaciones();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al desactivar", "No se pudo desactivar la habitación.\n\nDetalle: " + ex.Message, true);
            }
        }

        private void btnReactivarHabitacion_Click(object sender, EventArgs e)
        {
            if (!PuedeModificarHabitaciones())
                return;

            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MostrarMensaje("Habitación no seleccionada", "Seleccioná una habitación de la tabla para continuar.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow.Cells["id_habitacion"].Value);

            bool activa = Convert.ToBoolean(
                dgvHabitaciones.CurrentRow.Cells["activo"].Value);

            if (activa)
            {
                MostrarMensaje("Habitación ya activa","La habitación seleccionada ya se encuentra activa.");
                return;
            }

            bool confirmacion = MostrarConfirmacion("Confirmar reactivación", "¿Desea reactivar la habitación seleccionada?");

            if (!confirmacion)
                return;

            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                negocio.Reactivar(idHabitacion);

                MostrarMensaje("Habitación reactivada", "La habitación se reactivó correctamente.");

                CargarHabitaciones();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al reactivar","No se pudo reactivar la habitación.\n\nDetalle: " + ex.Message,true);
            }
        }


        private void ActualizarBotones()
        {
            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                btnModificarHabitacion.Enabled = false;
                btnDesactivarHabitacion.Enabled = false;
                btnReactivarHabitacion.Enabled = false;
                return;
            }

            bool activa = Convert.ToBoolean(
                dgvHabitaciones.CurrentRow.Cells["activo"].Value);

            btnModificarHabitacion.Enabled = activa;
            btnDesactivarHabitacion.Enabled = activa;
            btnReactivarHabitacion.Enabled = !activa;
        }

        private void dgvHabitaciones_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }

        private bool PuedeModificarHabitaciones()
        {
            if (rolUsuario == "RECEPCIONISTA")
            {
                MostrarMensaje(
                    "Acceso denegado",
                    "No tenés permisos para modificar la disponibilidad de las habitaciones.",
                    true);

                return false;
            }

            return true;
        }
    }
}
