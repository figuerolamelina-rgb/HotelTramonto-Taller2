
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using HotelTramonto.Presentacion.Servicios;

namespace HotelTramonto.Presentacion
{
    public partial class FormPagos : Form
    {
        private readonly string rolOperacion;
        private readonly PagoNegocios negocio;
        private readonly HuespedNegocios huespedNegocio;
        private readonly ReservaNegocios reservaNegocio;

        private readonly int idUsuario;

        private TextBox txtDNI;
        private Button btnBuscarHuesped;
        private Label lblDatosHuesped;
        private DataGridView dgvReservas;
        private DataGridView dgvPagos;
        private DataGridView dgvConsumos;

        private int idHuespedSeleccionado;
        private readonly SeleccionReservaPagos seleccion = new();
        private int idReservaSeleccionada => seleccion.IdReserva;
        private bool cargandoReservas;
        private NumericUpDown numIdReserva = new NumericUpDown();

        private Label lblTituloConsumos;
        private Label lblTotalConsumido;
        private Label lblTotalPagadoServicios;
        private Label lblPendienteServicios;

        private readonly DetalleServicioNegocios detalleNegocio;
        private Button btnRegistrarSaldo;
        private Button btnCerrar;
        private Button btnRegistrarServicios;
        private Button btnAgregarConsumo;
        private Button btnGenerarComprobante;
        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        public FormPagos(int idUsuario, string rol)
        {
            PermisosOperacion.Exigir(rol);
            rolOperacion = rol;
            negocio = new PagoNegocios(rol);
            huespedNegocio = new HuespedNegocios(rol);
            reservaNegocio = new ReservaNegocios(rol);
            detalleNegocio = new DetalleServicioNegocios(rol);
            InitializeComponent();
            PresentacionTablas.Conectar(this);

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario autenticado no es válido.");

            this.idUsuario = idUsuario;

            ConfigurarFormulario();
            LimpiarBusqueda();
            Shown += (s, e) => ReanudarCobros();
        }

        private void ConfigurarFormulario()
        {
            Text = "Hotel Tramonto | Gestión de pagos";
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 750);

            txtDNI = new TextBox();
            btnBuscarHuesped = new Button();
            lblDatosHuesped = new Label();

            dgvReservas = new DataGridView();
            dgvPagos = new DataGridView();
            dgvConsumos = new DataGridView();

            lblTituloConsumos = new Label();
            lblTotalConsumido = new Label();
            lblTotalPagadoServicios = new Label();
            lblPendienteServicios = new Label();
            btnRegistrarSaldo = new Button();
            btnCerrar = new Button();
            btnRegistrarServicios = new Button();
            btnAgregarConsumo = new Button();
            btnGenerarComprobante = new Button();

            Label lblTitulo = new Label
            {
                Name = "lblTituloPagos",
                Text = "GESTIÓN DE PAGOS",
                Font = new Font("Georgia", 25F, FontStyle.Bold),
                ForeColor = dorado,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(30, 25)
            };

            // Búsqueda por DNI
            Label lblDNI = new Label
            {
                Text = "DNI del huésped:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = marron,
                BackColor = Color.Transparent,
                Location = new Point(35, 105),
                Size = new Size(125, 25)
            };

            txtDNI.Location = new Point(165, 100);
            txtDNI.Size = new Size(150, 32);
            txtDNI.Font = new Font("Segoe UI", 10F);
            txtDNI.BackColor = marfil;
            txtDNI.ForeColor = marron;
            txtDNI.MaxLength = 15;

            EstilizarBotonPago(btnBuscarHuesped, "Buscar huésped", marron);
            btnBuscarHuesped.Location = new Point(330, 96);
            btnBuscarHuesped.Size = new Size(155, 40);
            btnBuscarHuesped.Click += btnBuscarHuesped_Click;

            // Datos del huésped
            lblDatosHuesped.Location = new Point(35, 145);
            lblDatosHuesped.Size = new Size(900, 28);
            lblDatosHuesped.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDatosHuesped.ForeColor = marron;
            lblDatosHuesped.BackColor = Color.Transparent;

            Label lblTituloReservas = new Label
            {
                Text = "RESERVAS DEL HUÉSPED",
                Location = new Point(35, 175),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = marron,
                BackColor = Color.Transparent
            };

            Label lblTituloPagos = new Label
            {
                Text = "HISTORIAL DE PAGOS",
                Location = new Point(35, 340),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = marron,
                BackColor = Color.Transparent
            };

            // Grilla de reservas
            dgvReservas.Location = new Point(35, 200);
            dgvReservas.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                150);

            dgvReservas.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvReservas.ReadOnly = true;
            dgvReservas.AllowUserToAddRows = false;
            dgvReservas.AllowUserToDeleteRows = false;
            dgvReservas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReservas.MultiSelect = false;
            dgvReservas.RowHeadersVisible = false;
            dgvReservas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReservas.SelectionChanged += (s, e) => CargarSeleccionReserva();
            dgvReservas.BackgroundColor = marfil;
            dgvReservas.BorderStyle = BorderStyle.None;
            dgvReservas.EnableHeadersVisualStyles = false;

            dgvReservas.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvReservas.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvReservas.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvReservas.DefaultCellStyle.BackColor = marfil;
            dgvReservas.DefaultCellStyle.ForeColor = marron;
            dgvReservas.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);
            dgvReservas.DefaultCellStyle.SelectionForeColor = marron;

            dgvReservas.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            EstilizarBotonPago(btnRegistrarSaldo, "Registrar saldo", dorado);
            btnRegistrarSaldo.ForeColor = marron;
            btnRegistrarSaldo.Location = new Point(455, 96);
            btnRegistrarSaldo.Size = new Size(155, 40);
            btnRegistrarSaldo.Click += btnRegistrarSaldo_Click;

            EstilizarBotonPago(
                btnRegistrarServicios,
                "Registrar servicios",
                marronClaro);

            btnRegistrarServicios.Location = new Point(625, 96);
            btnRegistrarServicios.Size = new Size(175, 40);
            btnRegistrarServicios.Click += btnRegistrarServicios_Click;

            // Botón Agregar consumo
            EstilizarBotonPago(
                btnAgregarConsumo,
                "Agregar consumo",
                dorado);

            btnAgregarConsumo.ForeColor = marron;
            btnAgregarConsumo.Location = new Point(815, 96);
            btnAgregarConsumo.Size = new Size(165, 40);
            btnAgregarConsumo.Click += btnAgregarConsumo_Click;
            dgvPagos.Location = new Point(35, 380);
            dgvPagos.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                130);

            dgvPagos.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvPagos.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvPagos.ReadOnly = true;
            dgvPagos.AllowUserToAddRows = false;
            dgvPagos.AllowUserToDeleteRows = false;
            dgvPagos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPagos.MultiSelect = false;
            dgvPagos.RowHeadersVisible = false;
            dgvPagos.ScrollBars = ScrollBars.Both;
            dgvPagos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPagos.RowTemplate.Height = 36;
            dgvPagos.ColumnHeadersHeight = 42;

            dgvPagos.BackgroundColor = marfil;
            dgvPagos.BorderStyle = BorderStyle.None;
            dgvPagos.GridColor = Color.FromArgb(229, 218, 202);
            dgvPagos.EnableHeadersVisualStyles = false;

            dgvPagos.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvPagos.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvPagos.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvPagos.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvPagos.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvPagos.DefaultCellStyle.BackColor = marfil;
            dgvPagos.DefaultCellStyle.ForeColor = marron;
            dgvPagos.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvPagos.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);
            dgvPagos.DefaultCellStyle.SelectionForeColor = marron;

            dgvPagos.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            dgvPagos.DataBindingComplete += (sender, e) =>
            {
                // Encabezados de las columnas
                if (dgvPagos.Columns.Contains("id_pago"))
                    dgvPagos.Columns["id_pago"].HeaderText = "N.º de pago";

                if (dgvPagos.Columns.Contains("id_reserva"))
                    dgvPagos.Columns["id_reserva"].HeaderText = "N.º de reserva";

                if (dgvPagos.Columns.Contains("id_usuario"))
                    dgvPagos.Columns["id_usuario"].HeaderText = "ID usuario";

                if (dgvPagos.Columns.Contains("nombre_usuario"))
                    dgvPagos.Columns["nombre_usuario"].HeaderText = "Nombre usuario";

                if (dgvPagos.Columns.Contains("apellido_usuario"))
                    dgvPagos.Columns["apellido_usuario"].HeaderText = "Apellido usuario";

                if (dgvPagos.Columns.Contains("monto"))
                {
                    dgvPagos.Columns["monto"].HeaderText = "Importe";

                    dgvPagos.Columns["monto"].DefaultCellStyle.Format = "N2";

                    dgvPagos.Columns["monto"].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
                }

                if (dgvPagos.Columns.Contains("metodo_pago"))
                    dgvPagos.Columns["metodo_pago"].HeaderText = "Método de pago";

                if (dgvPagos.Columns.Contains("estado"))
                    dgvPagos.Columns["estado"].HeaderText = "Estado";

                if (dgvPagos.Columns.Contains("tipo_pago"))
                    dgvPagos.Columns["tipo_pago"].HeaderText = "Concepto";

                if (dgvPagos.Columns.Contains("fecha_pago"))
                    dgvPagos.Columns["fecha_pago"].HeaderText = "Fecha de pago";

                dgvPagos.CellFormatting += (sender, e) =>
                {
                    if (dgvPagos.Columns[e.ColumnIndex].Name == "tipo_pago"
                        && e.Value != null)
                    {
                        string tipo = Convert.ToString(e.Value);

                        switch (tipo)
                        {
                            case "PAGO_INICIAL":
                                e.Value = "Pago inicial";
                                break;

                            case "SALDO_ALOJAMIENTO":
                                e.Value = "Saldo de alojamiento";
                                break;

                            case "SERVICIOS_ADICIONALES":
                                e.Value = "Servicios adicionales";
                                break;
                        }

                        e.FormattingApplied = true;
                    }
                };


                // Anchos de columnas para mejorar la lectura
                dgvPagos.Columns["id_pago"].Width = 75;
                dgvPagos.Columns["id_reserva"].Width = 90;
                dgvPagos.Columns["id_usuario"].Width = 80;
                dgvPagos.Columns["nombre_usuario"].Width = 115;
                dgvPagos.Columns["apellido_usuario"].Width = 115;
                dgvPagos.Columns["monto"].Width = 110;
                dgvPagos.Columns["metodo_pago"].Width = 125;
                dgvPagos.Columns["estado"].Width = 100;
                dgvPagos.Columns["tipo_pago"].Width = 190;
                dgvPagos.Columns["fecha_pago"].Width = 150;

                // Evitar saltos de línea en encabezados y celdas
                dgvPagos.ColumnHeadersDefaultCellStyle.WrapMode =
                    DataGridViewTriState.False;

                dgvPagos.DefaultCellStyle.WrapMode =
                    DataGridViewTriState.False;
            };

            // Botón Generar comprobante
            EstilizarBotonPago(
                btnGenerarComprobante,
                "Generar comprobante PDF",
                marron);

            btnGenerarComprobante.Location = new Point(995, 96);
            btnGenerarComprobante.Size = new Size(220, 40);
            btnGenerarComprobante.Click += btnGenerarComprobante_Click;

            // Título de consumos
            lblTituloConsumos.Text = "CONSUMOS DE LA RESERVA";
            lblTituloConsumos.Location = new Point(35, 550);
            lblTituloConsumos.Size = new Size(350, 30);
            lblTituloConsumos.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTituloConsumos.ForeColor = marron;
            lblTituloConsumos.BackColor = Color.Transparent;

            // Grilla de consumos
            dgvConsumos.Location = new Point(35, 590);
            dgvConsumos.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                60);

            dgvConsumos.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvConsumos.ReadOnly = true;
            dgvConsumos.AllowUserToAddRows = false;
            dgvConsumos.AllowUserToDeleteRows = false;
            dgvConsumos.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvConsumos.MultiSelect = false;
            dgvConsumos.RowHeadersVisible = false;
            dgvConsumos.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvConsumos.RowTemplate.Height = 32;
            dgvConsumos.ColumnHeadersHeight = 38;

            dgvConsumos.BackgroundColor = marfil;
            dgvConsumos.BorderStyle = BorderStyle.None;
            dgvConsumos.GridColor =
                Color.FromArgb(229, 218, 202);
            dgvConsumos.EnableHeadersVisualStyles = false;

            dgvConsumos.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvConsumos.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvConsumos.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvConsumos.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvConsumos.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvConsumos.DefaultCellStyle.BackColor = marfil;
            dgvConsumos.DefaultCellStyle.ForeColor = marron;
            dgvConsumos.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F);

            dgvConsumos.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvConsumos.DefaultCellStyle.SelectionForeColor = marron;

            dgvConsumos.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            // Resumen de servicios
            lblTotalConsumido.Location = new Point(35, 665);
            lblTotalConsumido.Size = new Size(300, 30);
            lblTotalConsumido.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTotalConsumido.ForeColor = marron;

            lblTotalPagadoServicios.Location = new Point(350, 655);
            lblTotalPagadoServicios.Size = new Size(300, 30);
            lblTotalPagadoServicios.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTotalPagadoServicios.ForeColor = marron;

            lblPendienteServicios.Location = new Point(665, 655);
            lblPendienteServicios.Size = new Size(350, 30);
            lblPendienteServicios.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPendienteServicios.ForeColor = dorado;
            EstilizarBotonPago(btnCerrar, "Cerrar", dorado);
            btnCerrar.ForeColor = marron;
            btnCerrar.Size = new Size(130, 38);
            btnCerrar.Location = new Point(
                ClientSize.Width - 165,
                ClientSize.Height - 55);
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (sender, e) => Close();


            // ============================================
            // DISTRIBUCIÓN RESPONSIVE CON TABLELAYOUTPANEL
            // ============================================

            TableLayoutPanel panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = crema,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(20),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            panelPrincipal.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 8F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 27F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));

            // 1. Título principal
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Location = Point.Empty;
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            panelPrincipal.Controls.Add(lblTitulo, 0, 0);

            // 2. Barra de búsqueda y botones
            FlowLayoutPanel panelAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = crema,
                Padding = new Padding(0, 5, 0, 5)
            };

            lblDNI.Margin = new Padding(5, 10, 5, 5);
            txtDNI.Margin = new Padding(5, 5, 10, 5);

            btnBuscarHuesped.Margin = new Padding(5);
            btnRegistrarSaldo.Margin = new Padding(5);
            btnRegistrarServicios.Margin = new Padding(5);
            btnAgregarConsumo.Margin = new Padding(5);
            btnGenerarComprobante.Margin = new Padding(5);

            panelAcciones.Controls.Add(lblDNI);
            panelAcciones.Controls.Add(txtDNI);
            panelAcciones.Controls.Add(btnBuscarHuesped);
            numIdReserva.Minimum = 1;
            numIdReserva.Maximum = int.MaxValue;
            numIdReserva.Width = 90;
            var btnConsultarId = new Button();
            EstilizarBotonPago(btnConsultarId, "Consultar ID", marron);
            btnConsultarId.Size = new Size(125, 40);
            btnConsultarId.Click += (s, e) => ConsultarPorId();
            panelAcciones.Controls.Add(new Label { Text = "N.º reserva", AutoSize = true, Margin = new Padding(5, 12, 5, 0) });
            panelAcciones.Controls.Add(numIdReserva);
            panelAcciones.Controls.Add(btnConsultarId);
            txtDNI.TextChanged += (s, e) => LimpiarBusqueda();
            numIdReserva.ValueChanged += (s, e) => LimpiarBusqueda();
            panelAcciones.Controls.Add(btnRegistrarSaldo);
            panelAcciones.Controls.Add(btnRegistrarServicios);
            panelAcciones.Controls.Add(btnAgregarConsumo);
            panelAcciones.Controls.Add(btnGenerarComprobante);

            panelPrincipal.Controls.Add(panelAcciones, 0, 1);

            // 3. Datos del huésped
            lblDatosHuesped.Dock = DockStyle.Fill;
            lblDatosHuesped.TextAlign = ContentAlignment.MiddleLeft;

            panelPrincipal.Controls.Add(lblDatosHuesped, 0, 2);

            // 4. Sección de reservas
            TableLayoutPanel panelReservas = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = crema,
                Padding = new Padding(0, 3, 0, 3)
            };

            panelReservas.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 28F));

            panelReservas.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            panelReservas.Controls.Add(lblTituloReservas, 0, 0);

            lblTituloReservas.Dock = DockStyle.Fill;
            dgvReservas.Dock = DockStyle.Fill;

            panelReservas.Controls.Add(dgvReservas, 0, 1);

            panelPrincipal.Controls.Add(panelReservas, 0, 3);

            // 5. Sección de pagos
            TableLayoutPanel panelPagos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = crema,
                Padding = new Padding(0, 3, 0, 3)
            };

            panelPagos.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 28F));

            panelPagos.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            lblTituloPagos.Dock = DockStyle.Fill;
            dgvPagos.Dock = DockStyle.Fill;

            panelPagos.Controls.Add(lblTituloPagos, 0, 0);
            panelPagos.Controls.Add(dgvPagos, 0, 1);

            panelPrincipal.Controls.Add(panelPagos, 0, 4);

            // 6. Sección de consumos
            TableLayoutPanel panelConsumos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = crema,
                Padding = new Padding(0, 3, 0, 3)
            };

            panelConsumos.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 28F));

            panelConsumos.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            lblTituloConsumos.Dock = DockStyle.Fill;
            dgvConsumos.Dock = DockStyle.Fill;

            panelConsumos.Controls.Add(lblTituloConsumos, 0, 0);
            panelConsumos.Controls.Add(dgvConsumos, 0, 1);

            panelPrincipal.Controls.Add(panelConsumos, 0, 5);

            // 7. Resumen de importes y botón cerrar
            TableLayoutPanel panelResumen = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = crema
            };

            panelResumen.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25F));

            panelResumen.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25F));

            panelResumen.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30F));

            panelResumen.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20F));

            lblTotalConsumido.Dock = DockStyle.Fill;
            lblTotalPagadoServicios.Dock = DockStyle.Fill;
            lblPendienteServicios.Dock = DockStyle.Fill;

            lblTotalConsumido.TextAlign = ContentAlignment.MiddleLeft;
            lblTotalPagadoServicios.TextAlign = ContentAlignment.MiddleLeft;
            lblPendienteServicios.TextAlign = ContentAlignment.MiddleLeft;

            btnCerrar.Anchor = AnchorStyles.None;
            btnCerrar.Dock = DockStyle.Fill;
            btnCerrar.Margin = new Padding(8);

            panelResumen.Controls.Add(lblTotalConsumido, 0, 0);
            panelResumen.Controls.Add(lblTotalPagadoServicios, 1, 0);
            panelResumen.Controls.Add(lblPendienteServicios, 2, 0);
            panelResumen.Controls.Add(btnCerrar, 3, 0);

            panelPrincipal.Controls.Add(panelResumen, 0, 6);

            // Agregar el contenedor principal al formulario
            Controls.Add(panelPrincipal);

            panelPrincipal.BringToFront();

        }

        private void EstilizarBotonPago(
            Button boton,
            string textoBoton,
            Color fondo)
        {
            boton.Text = textoBoton;
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.BackColor = fondo;
            boton.ForeColor = fondo == dorado ? marron : marfil;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(105, 73, 52);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }
        private void btnBuscarHuesped_Click(object sender, EventArgs e)
        {
            LimpiarBusqueda(); // También ante DNI inválido o consulta fallida, nunca usar el destino anterior.
            try
            {
                string dni = txtDNI.Text.Trim();
                if (!Validaciones.DNIValido(dni))
                {
                    MostrarMensaje("DNI inválido", "Ingresá entre 7 y 15 dígitos.");
                    return;
                }
                DataTable huesped = huespedNegocio.BuscarPorDNI(dni);
                if (huesped.Rows.Count == 0)
                {
                    MostrarMensaje("Huésped no encontrado", "No existe un huésped con ese DNI.");
                    return;
                }
                DataRow datos = huesped.Rows[0];
                idHuespedSeleccionado = Convert.ToInt32(datos["id_huesped"]);
                lblDatosHuesped.Text = $"Huésped: {datos["nombre"]} {datos["apellido"]} | DNI: {datos["dni"]}";
                MostrarReservas(reservaNegocio.ListarPorHuesped(idHuespedSeleccionado));
                if (dgvReservas.Rows.Count == 0)
                    MostrarMensaje("Sin reservas", "El huésped no tiene reservas definitivas.");
            }
            catch (Exception ex)
            {
                LimpiarBusqueda();
                MostrarMensaje("Error al buscar huésped", RegistroErrores.Mensaje(ex), true);
            }
        }

        private void CargarSeleccionReserva()
        {
            if (cargandoReservas) return;
            seleccion.Deseleccionar();
            LimpiarDetalle();
            if (dgvReservas.SelectedRows.Count != 1) return;
            try
            {
                int id = Convert.ToInt32(dgvReservas.SelectedRows[0].Cells["id_reserva"].Value);
                seleccion.Seleccionar(id);
                var pagos = negocio.ListarPorReserva(id);
                dgvPagos.DataSource = pagos;
                CargarConsumosYResumen(id, pagos);
                ActualizarAcciones();
            }
            catch (Exception ex)
            {
                seleccion.Deseleccionar();
                LimpiarDetalle();
                MostrarMensaje("Error al seleccionar reserva", RegistroErrores.Mensaje(ex), true);
            }
        }

        private void LimpiarBusqueda()
        {
            seleccion.Limpiar();
            idHuespedSeleccionado = 0;
            cargandoReservas = true;
            try { dgvReservas.DataSource = null; }
            finally { cargandoReservas = false; }
            lblDatosHuesped.Text = "Buscá un huésped y seleccioná una reserva, o consultá por ID.";
            LimpiarDetalle();
        }

        private void LimpiarDetalle()
        {
            dgvPagos.DataSource = null;
            dgvConsumos.DataSource = null;
            lblTotalConsumido.Text = "Total consumido: —";
            lblTotalPagadoServicios.Text = "Total pagado: —";
            lblPendienteServicios.Text = "Pendiente: —";
            ActualizarAcciones();
        }

        private void ActualizarAcciones()
        {
            btnRegistrarSaldo.Enabled = btnRegistrarServicios.Enabled = btnAgregarConsumo.Enabled = idReservaSeleccionada > 0;
            btnGenerarComprobante.Enabled = dgvPagos.Rows.Count > 0;
        }

        private void MostrarReservas(DataTable reservas)
        {
            seleccion.Cargar(reservas);
            cargandoReservas = true;
            try
            {
                dgvReservas.DataSource = reservas;
                dgvReservas.ClearSelection();
            }
            finally { cargandoReservas = false; }
            LimpiarDetalle();
        }

        private void ConsultarPorId()
        {
            int id = (int)numIdReserva.Value;
            LimpiarBusqueda();
            try
            {
                var todas = reservaNegocio.Listar();
                var reservas = todas.Clone();
                foreach (DataRow fila in todas.Rows)
                    if (Convert.ToInt32(fila["id_reserva"]) == id) reservas.ImportRow(fila);
                MostrarReservas(reservas);
                if (reservas.Rows.Count == 0)
                {
                    MostrarMensaje("Reserva no encontrada", "No existe una reserva definitiva con ese ID.");
                    return;
                }
                lblDatosHuesped.Text = $"Reserva N.º {id} — consulta por ID";
                dgvReservas.Rows[0].Selected = true;
            }
            catch (Exception ex) { LimpiarBusqueda(); MostrarMensaje("Consulta no disponible", RegistroErrores.Mensaje(ex), true); }
        }

        private int ObtenerIdReservaSeleccionada()
        {
            if (idReservaSeleccionada <= 0)
            {
                MostrarMensaje(
                    "Seleccionar reserva",
                    "Primero buscá un huésped y seleccioná una de sus reservas.");

                return 0;
            }

            return idReservaSeleccionada;
        }


        private void MostrarMensaje(
            string titulo,
            string detalle,
            bool esError = false,
            int? idPago = null)
        {
            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(440, 220);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(440, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 18),
                    Size = new Size(410, 35),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(400, 105),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnAceptar = CrearBotonDialogo(
                    "Aceptar", dorado, marron);

                btnAceptar.Size = new Size(110, 35);
                btnAceptar.DialogResult = DialogResult.OK;

                // Si hay un ID de pago, mostrar también el botón PDF.
                if (idPago.HasValue && !esError)
                {
                    btnAceptar.Location = new Point(265, 175);

                    Button btnComprobante = CrearBotonDialogo(
                        "Generar comprobante PDF", marron, Color.White);

                    btnComprobante.Location = new Point(35, 175);
                    btnComprobante.Size = new Size(210, 35);

                    btnComprobante.Click += (s, e) =>
                    {
                        GenerarComprobantePago(idPago.Value);
                    };

                    mensaje.Controls.Add(btnComprobante);
                }
                else
                {
                    btnAceptar.Location = new Point(165, 175);
                }

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;
                mensaje.ShowDialog(this);
            }
        }


        private void GenerarComprobantePago(int idPago)
        {
            try
            {
                DataTable comprobante = negocio.ObtenerComprobante(idPago);

                if (comprobante == null || comprobante.Rows.Count == 0)
                {
                    throw new Exception(
                        "No se encontraron los datos del comprobante.");
                }

                using (SaveFileDialog guardar = new SaveFileDialog())
                {
                    guardar.Title = "Guardar comprobante de pago";
                    guardar.Filter = "Archivo PDF (*.pdf)|*.pdf";
                    guardar.FileName = $"Comprobante_Pago_{idPago}.pdf";
                    guardar.DefaultExt = "pdf";
                    guardar.AddExtension = true;

                    if (guardar.ShowDialog(this) != DialogResult.OK)
                        return;

                    HotelTramonto.Presentacion.Servicios.ComprobantePagoPdf.Generar(
                        comprobante.Rows[0],
                        guardar.FileName);

                    MostrarMensaje(
                        "Comprobante generado",
                        "El comprobante de pago se generó correctamente.\n\n" +
                        "Archivo: " + guardar.FileName);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al generar comprobante",
                    "No se pudo generar el comprobante PDF.\n\n" + RegistroErrores.Mensaje(ex),
                    true);
            }
        }

        private bool MostrarConfirmacion(
            string titulo,
            string detalle)
        {
            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(500, 310);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(440, 7),
                    BackColor = dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 18),
                    Size = new Size(470, 35),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(25, 60),
                    Size = new Size(450, 165),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnSi = CrearBotonDialogo(
                    "Sí, confirmar", dorado, marron);

                btnSi.Location = new Point(65, 255);
                btnSi.Size = new Size(145, 38);
                btnSi.DialogResult = DialogResult.Yes;

                Button btnNo = CrearBotonDialogo(
                    "No, cancelar", marronClaro, Color.White);

                btnNo.Location = new Point(270, 255);
                btnNo.Size = new Size(145, 38);
                btnNo.DialogResult = DialogResult.No;

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

        private Button CrearBotonDialogo(
            string texto,
            Color fondo,
            Color textoColor)
        {
            Button boton = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = textoColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);

            return boton;
        }

        private bool MostrarSelectorMetodoPago(out string metodoSeleccionado)
        {
            metodoSeleccionado = "";

            using (Form selector = new Form())
            {
                selector.Text = "Hotel Tramonto | Método de pago";
                selector.StartPosition = FormStartPosition.CenterParent;
                selector.FormBorderStyle = FormBorderStyle.FixedDialog;
                selector.MaximizeBox = false;
                selector.MinimizeBox = false;
                selector.ShowInTaskbar = false;
                selector.ClientSize = new Size(360, 185);
                selector.BackColor = crema;
                selector.ForeColor = marron;
                selector.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(360, 7),
                    BackColor = dorado
                };

                Label lblTitulo = new Label
                {
                    Text = "SELECCIONAR MÉTODO DE PAGO",
                    Location = new Point(15, 18),
                    Size = new Size(330, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };

                Label lblMetodo = new Label
                {
                    Text = "Método de pago:",
                    Location = new Point(20, 58),
                    AutoSize = true,
                    ForeColor = marron
                };

                ComboBox cmbMetodo = new ComboBox
                {
                    Location = new Point(20, 82),
                    Size = new Size(320, 30),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = marfil,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 10F)
                };

                cmbMetodo.Items.AddRange(new object[]
                {
                    "EFECTIVO",
                    "TARJETA",
                    "TRANSFERENCIA"
                });

                cmbMetodo.SelectedIndex = 0;

                Button btnAceptar = CrearBotonDialogo(
                    "Continuar", dorado, marron);

                btnAceptar.Location = new Point(65, 130);
                btnAceptar.Size = new Size(110, 35);
                btnAceptar.DialogResult = DialogResult.OK;

                Button btnCancelar = CrearBotonDialogo(
                    "Cancelar", marronClaro, Color.White);

                btnCancelar.Location = new Point(190, 130);
                btnCancelar.Size = new Size(110, 35);
                btnCancelar.DialogResult = DialogResult.Cancel;

                selector.Controls.Add(encabezado);
                selector.Controls.Add(lblTitulo);
                selector.Controls.Add(lblMetodo);
                selector.Controls.Add(cmbMetodo);
                selector.Controls.Add(btnAceptar);
                selector.Controls.Add(btnCancelar);

                selector.AcceptButton = btnAceptar;
                selector.CancelButton = btnCancelar;

                if (selector.ShowDialog(this) != DialogResult.OK)
                    return false;

                if (cmbMetodo.SelectedItem == null)
                {
                    MostrarMensaje(
                        "Método requerido",
                        "Seleccioná un método de pago.");

                    return false;
                }

                metodoSeleccionado = cmbMetodo.SelectedItem.ToString()!;
                return true;
            }
        }

        private void CargarConsumosYResumen(
        int idReserva,
        DataTable pagos)
        {
            // Consultar consumos de la reserva
            DataTable consumos =
                detalleNegocio.ListarPorReserva(idReserva);

            dgvConsumos.DataSource = consumos;

            decimal totalConsumido = 0;
            decimal totalPagado = 0;

            // Calcular total de consumos
            foreach (DataRow fila in consumos.Rows)
            {
                totalConsumido += Convert.ToDecimal(
                    fila["subtotal"]);
            }

            // Calcular únicamente pagos aprobados
            // correspondientes a servicios adicionales
            foreach (DataRow fila in pagos.Rows)
            {
                string tipoPago =
                    Convert.ToString(fila["tipo_pago"]);

                string estado =
                    Convert.ToString(fila["estado"]);

                if (tipoPago == "SERVICIOS_ADICIONALES" &&
                    estado == "APROBADO")
                {
                    totalPagado += Convert.ToDecimal(
                        fila["monto"]);
                }
            }

            decimal pendiente =
                Math.Max(0, totalConsumido - totalPagado);

            // Formato monetario argentino
            var cultura =
                new System.Globalization.CultureInfo("es-AR");

            lblTotalConsumido.Text =
                "Total consumido: " +
                "$ " + totalConsumido.ToString("N2", cultura);

            lblTotalPagadoServicios.Text =
                "Total pagado: " +
                "$ " + totalPagado.ToString("N2", cultura);

            lblPendienteServicios.Text =
                "Pendiente: " +
               "$ " + pendiente.ToString("N2", cultura);

            // Formato de columnas monetarias
            if (dgvConsumos.Columns.Contains("precio_unitario"))
            {
                dgvConsumos.Columns["precio_unitario"]
                   .DefaultCellStyle.Format = "N2";
            }

            if (dgvConsumos.Columns.Contains("subtotal"))
            {
                dgvConsumos.Columns["subtotal"]
                   .DefaultCellStyle.Format = "N2";
            }

            // Encabezados más claros
            if (dgvConsumos.Columns.Contains("nombre_servicio"))
                dgvConsumos.Columns["nombre_servicio"].HeaderText = "Servicio";

            if (dgvConsumos.Columns.Contains("cantidad"))
                dgvConsumos.Columns["cantidad"].HeaderText = "Cantidad";

            if (dgvConsumos.Columns.Contains("precio_unitario"))
                dgvConsumos.Columns["precio_unitario"].HeaderText = "Precio unitario";

            if (dgvConsumos.Columns.Contains("subtotal"))
                dgvConsumos.Columns["subtotal"].HeaderText = "Subtotal";

            // Ocultar identificadores internos
            string[] columnasOcultas =
            {
        "id_detalle_servicio",
        "id_reserva",
        "id_servicios",
        "id_habitacion"
    };

            foreach (string columna in columnasOcultas)
            {
                if (dgvConsumos.Columns.Contains(columna))
                    dgvConsumos.Columns[columna].Visible = false;
            }
        }

        private string ClaveCobro(bool servicios) => (servicios ? "servicios:" : "alojamiento:") + idUsuario;

        private void ReanudarCobros()
        {
            foreach (bool servicios in new[] { false, true })
            {
                try
                {
                    var solicitud = DiarioOperaciones.Actual().Leer<SolicitudCobro>(ClaveCobro(servicios));
                    if (solicitud != null && MostrarConfirmacion("Cobro pendiente",
                        $"¿Reanudar cobro de {(servicios ? "servicios" : "alojamiento")} de la reserva #{solicitud.IdReserva}, método {solicitud.Metodo}?"))
                        EjecutarCobro(solicitud);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (
                    HotelTramonto.Datos.ErroresOperacionSql.Clasificar(ex.Number) == HotelTramonto.Entidades.EstadoResultadoOperacion.RechazoDefinitivo)
                {
                    MostrarMensaje("Pago rechazado", HotelTramonto.Datos.ErroresOperacionSql.Mensaje(ex.Number), true);
                }
                catch (Exception)
                {
                    MostrarMensaje("Cobro pendiente",
                        "No se pudo reanudar. Conservá la operación y verificá su estado antes de cobrar otra vez.", true);
                }
            }
        }

        private void EjecutarCobro(SolicitudCobro solicitud)
        {
            var diario = DiarioOperaciones.Actual();
            btnRegistrarServicios.Enabled = btnRegistrarSaldo.Enabled = false;
            try
            {
                var resultado = diario.Ejecutar(ClaveCobro(solicitud.Servicios), solicitud, contexto =>
                    solicitud.Servicios
                        ? negocio.RegistrarServicios(solicitud.IdReserva, solicitud.IdUsuario, solicitud.Metodo, contexto.Id)
                        : negocio.RegistrarSaldoAlojamiento(solicitud.IdReserva, solicitud.IdUsuario, solicitud.Metodo, contexto.Id));
                if (resultado.Valor.Rows.Count == 0)
                    throw new InvalidOperationException("No se recibió el resultado del pago.");
                var pago = resultado.Valor.Rows[0];
                if (idReservaSeleccionada != solicitud.IdReserva)
                {
                    LimpiarBusqueda();
                    txtDNI.Clear();
                    numIdReserva.Value = solicitud.IdReserva;
                    seleccion.Recuperar(solicitud.IdReserva);
                    lblDatosHuesped.Text = $"Reserva N.º {solicitud.IdReserva} — operación recuperada";
                }
                var pagos = negocio.ListarPorReserva(solicitud.IdReserva);
                dgvPagos.DataSource = pagos;
                CargarConsumosYResumen(solicitud.IdReserva, pagos);
                MostrarMensaje("Pago registrado",
                    $"Pago N.º {pago["id_pago"]}\nReserva: {solicitud.IdReserva}\n" +
                    $"Importe: {pago["monto_pagado"]}\nMétodo: {pago["metodo_pago"]}", false, Convert.ToInt32(pago["id_pago"]));
                diario.Confirmar(ClaveCobro(solicitud.Servicios), resultado.Id);
            }
            finally { ActualizarAcciones(); }
        }

        private void SolicitarCobro(bool servicios)
        {
            try
            {
                int idReserva = ObtenerIdReservaSeleccionada();
                if (idReserva <= 0) return;
                if (!MostrarSelectorMetodoPago(out string metodo)) return;
                if (!MostrarConfirmacion("Confirmar pago",
                    $"Reserva N.º {idReserva}.\nConcepto: {(servicios ? "servicios adicionales" : "saldo de alojamiento")}.\nMétodo: {metodo}.\nSe cobrará el saldo completo actualizado.\n¿Confirmás que recibiste o verificaste el pago?")) return;
                EjecutarCobro(new SolicitudCobro(idReserva, idUsuario, metodo, servicios));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (
                HotelTramonto.Datos.ErroresOperacionSql.Clasificar(ex.Number) == HotelTramonto.Entidades.EstadoResultadoOperacion.RechazoDefinitivo)
            {
                MostrarMensaje("Pago rechazado", HotelTramonto.Datos.ErroresOperacionSql.Mensaje(ex.Number), true);
            }
            catch (Exception)
            {
                MostrarMensaje("Pago no confirmado",
                    "No se confirmó el resultado. Si quedó pendiente, reabrí Pagos para reanudar la misma operación.", true);
            }
        }

        private void btnRegistrarSaldo_Click(object sender, EventArgs e) => SolicitarCobro(false);
        private void btnRegistrarServicios_Click(object sender, EventArgs e) => SolicitarCobro(true);

        private void btnAgregarConsumo_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = ObtenerIdReservaSeleccionada();

                if (idReserva <= 0)
                    return;

                using (FormRegistrarConsumos formulario =
                    new FormRegistrarConsumos(idReserva, rolOperacion))
                {
                    formulario.ShowDialog(this);
                    DataTable pagos = negocio.ListarPorReserva(idReserva);

                    dgvPagos.DataSource = pagos;

                    CargarConsumosYResumen(idReserva, pagos);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al abrir consumos",
                    "No se pudo abrir el registro de consumos.\n\n" +
                    RegistroErrores.Mensaje(ex),
                    true);
            }
        }
        private void btnGenerarComprobante_Click(object sender, EventArgs e)
        {
            try
            {
                // Verificar que haya un pago seleccionado
                if (dgvPagos.SelectedRows.Count == 0)
                {
                    MostrarMensaje(
                        "Seleccionar pago",
                        "Seleccioná un pago de la grilla para generar el comprobante.");

                    return;
                }

                // Obtener el ID del pago seleccionado
                DataGridViewRow filaSeleccionada =
                    dgvPagos.SelectedRows[0];

                int idPago = Convert.ToInt32(
                    filaSeleccionada.Cells["id_pago"].Value);

                // Obtener los datos del comprobante
                DataTable datosComprobante =
                    negocio.ObtenerComprobante(idPago);

                if (datosComprobante.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Comprobante no disponible",
                        "No se encontró un pago aprobado para generar el comprobante.");

                    return;
                }

                // Elegir dónde guardar el PDF
                using (SaveFileDialog guardar = new SaveFileDialog())
                {
                    guardar.Title = "Guardar comprobante de pago";
                    guardar.Filter = "Archivo PDF (*.pdf)|*.pdf";
                    guardar.DefaultExt = "pdf";
                    guardar.AddExtension = true;
                    guardar.FileName = $"Comprobante_Pago_{idPago}.pdf";

                    if (guardar.ShowDialog(this) != DialogResult.OK)
                        return;

                    // Generar el comprobante
                    ComprobantePagoPdf.Generar(
                        datosComprobante.Rows[0],
                        guardar.FileName);

                    MostrarMensaje(
                        "Comprobante generado",
                        "El comprobante se generó correctamente.\n\n" +
                        "Ubicación:\n" + guardar.FileName);
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al generar comprobante",
                    "No se pudo generar el comprobante.\n\n" + RegistroErrores.Mensaje(ex),
                    true);
            }
        }
    }
}
