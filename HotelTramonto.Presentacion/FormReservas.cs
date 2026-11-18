
using HotelTramonto.Entidades;
using HotelTramonto.Negocios;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormReservas : Form
    {
        private readonly int idUsuario;

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private Button btnNuevaReserva;
        private Label lblEstado;
        private Panel pnlFiltros;
        private Label lblTituloFiltros;
        private Button btnDeshacerCheckIn;
        public FormReservas(int idUsuario)
        {
            InitializeComponent();

            this.idUsuario = idUsuario;

            AplicarEstilo();

            ConfigurarComboEstado();
            CargarReservas();

            CrearBotonNuevaReserva();
        }


        private void AplicarEstilo()
        {
            Color texto = Color.FromArgb(66, 51, 43);
            Color cremaAlternativo = Color.FromArgb(248, 243, 235);

            // VENTANA
            Text = "Hotel Tramonto | Reservas";
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);

            // TÍTULO
            lblTitulo.Text = "GESTIÓN DE RESERVAS";
            lblTitulo.Font = new Font("Georgia", 25F, FontStyle.Bold);
            lblTitulo.ForeColor = dorado;
            lblTitulo.BackColor = Color.Transparent;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(35, 25);

            // PANEL DE FILTROS
            pnlFiltros = new Panel
            {
                Name = "pnlFiltros",
                Location = new Point(35, 115),
                Size = new Size(820, 155),
                BackColor = marfil,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            Controls.Add(pnlFiltros);
            pnlFiltros.SendToBack();

            lblTituloFiltros = new Label
            {
                Text = "Filtros de búsqueda",
                Location = new Point(20, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = marron,
                BackColor = Color.Transparent
            };

            pnlFiltros.Controls.Add(lblTituloFiltros);

            // FECHAS
            foreach (Label etiqueta in new[] { lblFechaDesde, lblFechaHasta })
            {
                etiqueta.Font = new Font("Segoe UI", 10F);
                etiqueta.ForeColor = marron;
                etiqueta.BackColor = Color.Transparent;
            }

            lblFechaDesde.Text = "Desde:";
            lblFechaHasta.Text = "Hasta:";

            lblFechaDesde.Location = new Point(20, 60);
            lblFechaHasta.Location = new Point(360, 60);

            foreach (DateTimePicker fecha in new[] { dtpFechaDesde, dtpFechaHasta })
            {
                fecha.Font = new Font("Segoe UI", 10F);
                fecha.CalendarMonthBackground = marfil;
                fecha.Format = DateTimePickerFormat.Custom;
                fecha.CustomFormat = "dd/MM/yyyy";
                fecha.Width = 190;
            }

            dtpFechaDesde.Location = new Point(80, 56);
            dtpFechaHasta.Location = new Point(420, 56);

            // ESTADO
            lblEstado = new Label
            {
                Text = "Estado:",
                Location = new Point(20, 105),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = marron,
                BackColor = Color.Transparent
            };

            cmbEstado.Font = new Font("Segoe UI", 10F);
            cmbEstado.BackColor = crema;
            cmbEstado.ForeColor = marron;
            cmbEstado.Size = new Size(300, 32);
            cmbEstado.Location = new Point(80, 101);
            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;

            // BOTONES DE FILTROS
            EstilizarBotonReserva(btnFiltrarFechas, "Filtrar fechas", marron);
            btnFiltrarFechas.Location = new Point(650, 53);
            btnFiltrarFechas.Size = new Size(145, 36);

            EstilizarBotonReserva(btnFiltrarEstado, "Filtrar estado", marron);
            btnFiltrarEstado.Location = new Point(400, 98);
            btnFiltrarEstado.Size = new Size(145, 36);

            // AGREGAR CONTROLES A LA TARJETA
            pnlFiltros.Controls.Add(lblFechaDesde);
            pnlFiltros.Controls.Add(lblFechaHasta);
            pnlFiltros.Controls.Add(dtpFechaDesde);
            pnlFiltros.Controls.Add(dtpFechaHasta);
            pnlFiltros.Controls.Add(lblEstado);
            pnlFiltros.Controls.Add(cmbEstado);
            pnlFiltros.Controls.Add(btnFiltrarFechas);
            pnlFiltros.Controls.Add(btnFiltrarEstado);

            // ACCIONES OPERATIVAS

            EstilizarBotonReserva(btnCheckIn, "Check-in", marronClaro);
            btnCheckIn.Location = new Point(35, 290);
            btnCheckIn.Size = new Size(130, 40);

            EstilizarBotonReserva(btnCheckOut, "Check-out", marronClaro);
            btnCheckOut.Location = new Point(180, 290);
            btnCheckOut.Size = new Size(130, 40);

            btnDeshacerCheckIn = new Button();

            EstilizarBotonReserva(
                btnDeshacerCheckIn,
                "Deshacer Check-in",
                Color.FromArgb(160, 75, 65));

            btnDeshacerCheckIn.Location = new Point(325, 290);
            btnDeshacerCheckIn.Size = new Size(180, 40);

            btnDeshacerCheckIn.Click += btnDeshacerCheckIn_Click;

            EstilizarBotonReserva(btnConsultarSaldo, "Consultar saldo", marronClaro);
            btnConsultarSaldo.Location = new Point(520, 290);
            btnConsultarSaldo.Size = new Size(155, 40);

            Controls.Add(btnDeshacerCheckIn);

            // TABLA DE RESERVAS
            dgvReservas.Location = new Point(35, 350);

            dgvReservas.Size = new Size(
                ClientSize.Width - 70,
                ClientSize.Height - 440);

            dgvReservas.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvReservas.BackgroundColor = marfil;
            dgvReservas.BorderStyle = BorderStyle.None;
            dgvReservas.GridColor = Color.FromArgb(229, 218, 202);
            dgvReservas.EnableHeadersVisualStyles = false;

            dgvReservas.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;

            dgvReservas.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvReservas.MultiSelect = false;
            dgvReservas.ReadOnly = true;
            dgvReservas.RowHeadersVisible = false;
            dgvReservas.RowTemplate.Height = 36;
            dgvReservas.ColumnHeadersHeight = 42;
            dgvReservas.AllowUserToAddRows = false;
            dgvReservas.ScrollBars = ScrollBars.Both;

            dgvReservas.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvReservas.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvReservas.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvReservas.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvReservas.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvReservas.DefaultCellStyle.BackColor = marfil;
            dgvReservas.DefaultCellStyle.ForeColor = texto;
            dgvReservas.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgvReservas.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvReservas.DefaultCellStyle.SelectionForeColor = marron;

            dgvReservas.AlternatingRowsDefaultCellStyle.BackColor =
                cremaAlternativo;

            // BOTONES INFERIORES
            EstilizarBotonReserva(
                btnCancelarReserva,
                "Cancelar reserva",
                marronClaro);

            btnCancelarReserva.Location = new Point(
                35,
                ClientSize.Height - 75);

            btnCancelarReserva.Size = new Size(175, 42);

            btnCancelarReserva.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;

            EstilizarBotonReserva(btnActualizar, "Actualizar", marron);

            btnActualizar.Location = new Point(
                230,
                ClientSize.Height - 75);

            btnActualizar.Size = new Size(140, 42);

            btnActualizar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Left;

            EstilizarBotonReserva(btnCerrar, "Cerrar", dorado);
            btnCerrar.ForeColor = marron;

            btnCerrar.Location = new Point(
                ClientSize.Width - 165,
                ClientSize.Height - 75);

            btnCerrar.Size = new Size(130, 42);

            btnCerrar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Right;
        }



        private void CrearBotonNuevaReserva()
        {
            btnNuevaReserva = new Button
            {
                Name = "btnNuevaReserva",
                Text = "＋ Nueva reserva",
                Size = new Size(175, 45),
                Location = new Point(ClientSize.Width - 210, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = dorado,
                ForeColor = marron,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = false
            };

            btnNuevaReserva.FlatAppearance.BorderSize = 0;
            btnNuevaReserva.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(218, 185, 119);

            btnNuevaReserva.Click += btnNuevaReserva_Click;

            Controls.Add(btnNuevaReserva);
            btnNuevaReserva.BringToFront();
        }


        private void EstilizarBotonReserva(
            Button boton,
            string textoBoton,
            Color fondo)
        {
            boton.Text = textoBoton;
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.BackColor = fondo;
            boton.ForeColor = marfil;
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
            string mensaje,
            string titulo = "Hotel Tramonto",
            MessageBoxIcon icono = MessageBoxIcon.Information)
        {
            using (Form ventana = new Form())
            {
                Font fuente = new Font("Segoe UI", 10F);

                bool contenidoLargo =
                    mensaje.Length > 100 ||
                    mensaje.Split('\n').Length > 3;

                int anchoVentana = contenidoLargo ? 560 : 440;
                int anchoTexto = anchoVentana - 115;

                Size tamanoTexto = TextRenderer.MeasureText(
                    mensaje,
                    fuente,
                    new Size(anchoTexto, 1000),
                    TextFormatFlags.WordBreak);

                int altoTexto = Math.Max(30, tamanoTexto.Height + 10);
                int altoVentana = Math.Max(200, altoTexto + 130);

                altoVentana = Math.Min(altoVentana, 350);

                ventana.Text = titulo;
                ventana.Size = new Size(anchoVentana, altoVentana);
                ventana.MinimumSize = new Size(anchoVentana, altoVentana);
                ventana.MaximumSize = new Size(anchoVentana, altoVentana);
                ventana.StartPosition = FormStartPosition.CenterParent;
                ventana.FormBorderStyle = FormBorderStyle.FixedDialog;
                ventana.MaximizeBox = false;
                ventana.MinimizeBox = false;
                ventana.ShowInTaskbar = false;
                ventana.BackColor = crema;
                ventana.Font = fuente;

                Panel panelSuperior = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 9,
                    BackColor = dorado
                };

                Label lblIcono = new Label
                {
                    AutoSize = false,
                    Location = new Point(22, 30),
                    Size = new Size(45, 45),
                    Font = new Font("Segoe UI Symbol", 25F, FontStyle.Bold),
                    ForeColor = icono == MessageBoxIcon.Error
                        ? Color.FromArgb(160, 55, 45)
                        : icono == MessageBoxIcon.Warning
                            ? dorado
                            : marron,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = icono == MessageBoxIcon.Error
                        ? "!"
                        : icono == MessageBoxIcon.Warning
                            ? "⚠"
                            : icono == MessageBoxIcon.Question
                                ? "?"
                                : "✓"
                };


                Label lblMensaje = new Label
                {
                    Location = new Point(115, contenidoLargo ? 25 : 40),
                    Size = new Size(
                    contenidoLargo ? anchoTexto : 290,
                    contenidoLargo
                        ? Math.Min(altoTexto, altoVentana - 105)
                        : 65),
                    AutoSize = false,
                    Text = mensaje,
                    ForeColor = marron,
                    BackColor = crema,
                    Font = fuente,
                    TextAlign = ContentAlignment.MiddleLeft
                };


                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Size = new Size(110, 36),
                    Location = new Point(
                        anchoVentana - 155,
                        altoVentana - 90),
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

        private DialogResult MostrarConfirmacion(
            string mensaje,
            string titulo = "Confirmar acción")
        {
            using (Form ventana = new Form())
            {
                ventana.Text = titulo;
                ventana.Size = new Size(460, 230);
                ventana.MinimumSize = new Size(460, 230);
                ventana.MaximumSize = new Size(460, 230);
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

                Label lblIcono = new Label
                {
                    AutoSize = false,
                    Location = new Point(25, 35),
                    Size = new Size(45, 45),
                    Text = "?",
                    Font = new Font("Georgia", 27F, FontStyle.Bold),
                    ForeColor = dorado,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Label lblMensaje = new Label
                {
                    AutoSize = false,
                    Location = new Point(85, 25),
                    Size = new Size(335, 90),
                    Text = mensaje,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 10F),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Button btnSi = new Button
                {
                    Text = "Sí, continuar",
                    Size = new Size(125, 36),
                    Location = new Point(155, 135),
                    BackColor = dorado,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.Yes,
                    UseVisualStyleBackColor = false
                };

                Button btnNo = new Button
                {
                    Text = "Cancelar",
                    Size = new Size(110, 36),
                    Location = new Point(290, 135),
                    BackColor = marronClaro,
                    ForeColor = marfil,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.No,
                    UseVisualStyleBackColor = false
                };

                btnSi.FlatAppearance.BorderSize = 0;
                btnNo.FlatAppearance.BorderSize = 0;

                ventana.Controls.Add(panelSuperior);
                ventana.Controls.Add(lblIcono);
                ventana.Controls.Add(lblMensaje);
                ventana.Controls.Add(btnSi);
                ventana.Controls.Add(btnNo);

                ventana.AcceptButton = btnSi;
                ventana.CancelButton = btnNo;

                return ventana.ShowDialog(this);
            }
        }

        private void CargarReservas()
        {
            try
            {
                ReservaNegocios negocio = new ReservaNegocios();
                dgvReservas.DataSource = negocio.Listar();
                
                ConfigurarColumnasReservas();

                dgvReservas.ReadOnly = true;
                dgvReservas.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;

                dgvReservas.MultiSelect = false;
                dgvReservas.AllowUserToAddRows = false;

                dgvReservas.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.DisplayedCells;

                dgvReservas.ScrollBars = ScrollBars.Both;

                if (dgvReservas.Columns.Contains("total"))
                {
                    dgvReservas.Columns["total"].DefaultCellStyle.Format = "N2";
                    dgvReservas.Columns["total"].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
                }

                if (dgvReservas.Columns.Contains("observaciones"))
                {
                    dgvReservas.Columns["observaciones"].Width = 240;
                    dgvReservas.Columns["observaciones"].MinimumWidth = 180;
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar las reservas:\n" + ex.Message,
                    "Error",
                    MessageBoxIcon.Error);
            }
        }




        private void ConfigurarColumnasReservas()
        {
            if (dgvReservas.Columns.Count == 0)
                return;

            // Ocultar identificadores internos
            if (dgvReservas.Columns.Contains("id_huesped"))
                dgvReservas.Columns["id_huesped"].Visible = false;

            if (dgvReservas.Columns.Contains("id_usuario"))
                dgvReservas.Columns["id_usuario"].Visible = false;

            // Identificación de la reserva
            CambiarEncabezado("id_reserva", "N.º de reserva");

            // Datos del huésped
            CambiarEncabezado("nombre_huesped", "Nombre del huésped");
            CambiarEncabezado("apellido_huesped", "Apellido del huésped");
            
            CambiarEncabezado("habitaciones", "Habitación");
            
            // Datos del usuario que registró la reserva
            CambiarEncabezado("nombre_usuario", "Nombre del usuario");
            CambiarEncabezado("apellido_usuario", "Apellido del usuario");

            // Fechas de la reserva
            CambiarEncabezado("fecha_entrada", "Fecha de entrada");
            CambiarEncabezado("fecha_salida", "Fecha de salida");
            CambiarEncabezado("fecha_creacion", "Fecha de creación");
            CambiarEncabezado("fecha_checkin", "Fecha de check-in");
            CambiarEncabezado("fecha_checkout", "Fecha de check-out");

            // Otros datos
            CambiarEncabezado("estado", "Estado");
            CambiarEncabezado("total", "Importe total");
            CambiarEncabezado("observaciones", "Observaciones");

            // Formato del importe
            if (dgvReservas.Columns.Contains("total"))
            {
                dgvReservas.Columns["total"].DefaultCellStyle.Format = "N2";

                dgvReservas.Columns["total"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            // Ancho de observaciones
            if (dgvReservas.Columns.Contains("observaciones"))
            {
                dgvReservas.Columns["observaciones"].Width = 240;
                dgvReservas.Columns["observaciones"].MinimumWidth = 180;
            }
        }


        private void CambiarEncabezado(string nombreColumna, string encabezado)
        {
            if (dgvReservas.Columns.Contains(nombreColumna))
            {
                dgvReservas.Columns[nombreColumna].HeaderText = encabezado;
            }
        }


        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarReservas();

            MostrarMensaje(
                "Listado de reservas actualizado.",
                "Reservas actualizadas");
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnCancelarReserva_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null ||
                dgvReservas.CurrentRow.IsNewRow)
            {
                MostrarMensaje(
                    "Seleccione una reserva de la tabla.",
                    "Reserva no seleccionada",
                    MessageBoxIcon.Warning);
                return;
            }

            int idReserva = Convert.ToInt32(
                dgvReservas.CurrentRow.Cells["id_reserva"].Value);

            DialogResult confirmacion = MostrarConfirmacion(
                "¿Está seguro de que desea cancelar la reserva seleccionada?",
                "Confirmar cancelación");

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                ReservaNegocios negocio = new ReservaNegocios();
                negocio.Cancelar(idReserva);

                MostrarMensaje(
                    "Reserva cancelada correctamente.",
                    "Reserva cancelada");

                CargarReservas();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "No se pudo cancelar la reserva:\n" + ex.Message,
                    "Error al cancelar",
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigurarComboEstado()
        {
            cmbEstado.Items.Clear();

            cmbEstado.Items.Add("TODOS");
            cmbEstado.Items.Add("CONFIRMADA");
            cmbEstado.Items.Add("CHECKIN");
            cmbEstado.Items.Add("FINALIZADA");
            cmbEstado.Items.Add("CANCELADA");

            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.SelectedIndex = 0;
        }

        private void btnFiltrarEstado_Click(object sender, EventArgs e)
        {
            try
            {
                ReservaNegocios negocio = new ReservaNegocios();

                if (cmbEstado.SelectedItem == null)
                {
                    MostrarMensaje(
                        "Seleccione un estado.",
                        "Filtro de reservas",
                        MessageBoxIcon.Warning);
                    return;
                }

                string estado = cmbEstado.SelectedItem.ToString();

                if (estado == "TODOS")
                {
                    CargarReservas();
                }
                else
                {
                    dgvReservas.DataSource = negocio.ListarPorEstado(estado);
                    ConfigurarColumnasReservas();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al filtrar las reservas:\n" + ex.Message,
                    "Error de filtro",
                    MessageBoxIcon.Error);
            }
        }

        private void btnFiltrarFechas_Click(object sender, EventArgs e)
        {
            if (dtpFechaDesde.Value.Date > dtpFechaHasta.Value.Date)
            {
                MostrarMensaje(
                    "La fecha Desde no puede ser posterior a la fecha Hasta.",
                    "Rango de fechas inválido",
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ReservaNegocios negocio = new ReservaNegocios();

                dgvReservas.DataSource = negocio.ListarPorFechas(
                    dtpFechaDesde.Value.Date,
                    dtpFechaHasta.Value.Date);

                ConfigurarColumnasReservas();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al filtrar las reservas por fechas:\n" + ex.Message,
                    "Error de filtro",
                    MessageBoxIcon.Error);
            }
        }

        private void btnConsultarSaldo_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MostrarMensaje(
                    "Seleccione una reserva.",
                    "Reserva no seleccionada",
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                ReservaNegocios negocio = new ReservaNegocios();
                DataTable resultado = negocio.ConsultarSaldo(idReserva);

                if (resultado.Rows.Count > 0)
                {
                    DataRow fila = resultado.Rows[0];

                    decimal totalAlojamiento =
                        Convert.ToDecimal(fila["total_alojamiento"]);

                    decimal pagadoAlojamiento =
                        Convert.ToDecimal(fila["pagado_alojamiento"]);

                    decimal saldoAlojamiento =
                        Convert.ToDecimal(fila["saldo_alojamiento"]);

                    decimal totalServicios =
                        Convert.ToDecimal(fila["total_servicios"]);

                    CultureInfo cultura = CultureInfo.GetCultureInfo("es-AR");

                    StringBuilder mensaje = new StringBuilder();

                    mensaje.AppendLine("Reserva N°: " + fila["id_reserva"]);
                    mensaje.AppendLine();
                    mensaje.AppendLine("Total alojamiento: " +
                        totalAlojamiento.ToString("C2", cultura));

                    mensaje.AppendLine("Pagado alojamiento: " +
                        pagadoAlojamiento.ToString("C2", cultura));

                    mensaje.AppendLine("Saldo alojamiento: " +
                        saldoAlojamiento.ToString("C2", cultura));

                    mensaje.AppendLine("Total servicios: " +
                        totalServicios.ToString("C2", cultura));

                    MostrarMensaje(
                        mensaje.ToString(),
                        "Detalle del saldo");
                }
                else
                {
                    MostrarMensaje(
                        "No se encontró información del saldo.",
                        "Saldo no disponible",
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al consultar el saldo:\n" + ex.Message,
                    "Error al consultar saldo",
                    MessageBoxIcon.Error);
            }
        }

        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MostrarMensaje(
                    "Seleccione una reserva.",
                    "Reserva no seleccionada",
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                ReservaNegocios negocio = new ReservaNegocios();
                negocio.CheckIn(idReserva);

                MostrarMensaje(
                    "Check-in registrado correctamente.",
                    "Check-in registrado");

                CargarReservas();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar el check-in:\n" + ex.Message,
                    "Error de check-in",
                    MessageBoxIcon.Error);
            }
        }

        private void btnDeshacerCheckIn_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null ||
                dgvReservas.CurrentRow.IsNewRow)
            {
                MostrarMensaje(
                    "Seleccione una reserva.",
                    "Reserva no seleccionada",
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                string estado = Convert.ToString(
                    dgvReservas.CurrentRow.Cells["estado"].Value);

                if (estado != "CHECKIN")
                {
                    MostrarMensaje(
                        "Solo se puede deshacer el check-in de una reserva que esté en estado CHECKIN.",
                        "Operación no permitida",
                        MessageBoxIcon.Warning);
                    return;
                }

                DialogResult confirmacion = MostrarConfirmacion(
                    "¿Está seguro de que desea deshacer el check-in de la reserva N° "
                    + idReserva + "?\n\nLa reserva volverá al estado CONFIRMADA.",
                    "Confirmar deshacer check-in");

                if (confirmacion != DialogResult.Yes)
                    return;

                ReservaNegocios negocio = new ReservaNegocios();

                negocio.DeshacerCheckIn(idReserva);

                MostrarMensaje(
                    "El check-in de la reserva N° " + idReserva +
                    " se deshizo correctamente.\nLa reserva volvió al estado CONFIRMADA.",
                    "Check-in deshecho");

                CargarReservas();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "No se pudo deshacer el check-in:\n" + ex.Message,
                    "Error al deshacer check-in",
                    MessageBoxIcon.Error);
            }
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            if (dgvReservas.CurrentRow == null)
            {
                MostrarMensaje(
                    "Seleccione una reserva.",
                    "Reserva no seleccionada",
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idReserva = Convert.ToInt32(
                    dgvReservas.CurrentRow.Cells["id_reserva"].Value);

                ReservaNegocios negocio = new ReservaNegocios();
                negocio.CheckOut(idReserva);

                MostrarMensaje(
                    "Check-out registrado correctamente.",
                    "Check-out registrado");

                CargarReservas();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar el check-out:\n" + ex.Message,
                    "Error de check-out",
                    MessageBoxIcon.Error);
            }
        }

        private void btnNuevaReserva_Click(object sender, EventArgs e)
        {
            using (FormNuevaReserva formulario = new FormNuevaReserva(idUsuario))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarReservas();
                }
            }
        }
    }
}
