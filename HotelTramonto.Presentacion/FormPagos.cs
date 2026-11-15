
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
        private readonly PagoNegocios negocio = new PagoNegocios();
        private readonly int idUsuario;

        private NumericUpDown numIdReserva;
        private DataGridView dgvPagos;
        private DataGridView dgvConsumos;

        private Label lblTituloConsumos;
        private Label lblTotalConsumido;
        private Label lblTotalPagadoServicios;
        private Label lblPendienteServicios;

        private readonly DetalleServicioNegocios detalleNegocio =
            new DetalleServicioNegocios();
        private Button btnConsultar;
        private Button btnRegistrarSaldo;
        private Button btnCerrar;
        private Button btnRegistrarServicios;
        private Button btnAgregarConsumo;
        private Label lblIdReserva;
        private Button btnGenerarComprobante;
        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        public FormPagos(int idUsuario)
        {
            InitializeComponent();

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario autenticado no es válido.");

            this.idUsuario = idUsuario;

            ConfigurarFormulario();
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

            lblIdReserva = new Label();
            numIdReserva = new NumericUpDown();
            dgvPagos = new DataGridView();
            dgvConsumos = new DataGridView();

            lblTituloConsumos = new Label();
            lblTotalConsumido = new Label();
            lblTotalPagadoServicios = new Label();
            lblPendienteServicios = new Label();
            btnConsultar = new Button();
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

            lblIdReserva.Text = "ID de reserva:";
            lblIdReserva.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblIdReserva.ForeColor = marron;
            lblIdReserva.BackColor = Color.Transparent;
            lblIdReserva.Location = new Point(35, 105);
            lblIdReserva.Size = new Size(110, 25);

            numIdReserva.Location = new Point(150, 100);
            numIdReserva.Size = new Size(120, 32);
            numIdReserva.Minimum = 1;
            numIdReserva.Maximum = 1000000;
            numIdReserva.Font = new Font("Segoe UI", 10F);
            numIdReserva.BackColor = marfil;
            numIdReserva.ForeColor = marron;

            EstilizarBotonPago(btnConsultar, "Consultar pagos", marron);
            btnConsultar.Location = new Point(290, 96);
            btnConsultar.Size = new Size(150, 40);
            btnConsultar.Click += btnConsultar_Click;

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
            dgvPagos.Location = new Point(35, 165);
            dgvPagos.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                190);

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
            dgvPagos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
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

                // Mostrar conceptos de pago más descriptivos
                if (dgvPagos.Columns.Contains("tipo_pago"))
                {
                    foreach (DataGridViewRow fila in dgvPagos.Rows)
                    {
                        string tipo = Convert.ToString(
                            fila.Cells["tipo_pago"].Value);

                        switch (tipo)
                        {
                            case "PAGO_INICIAL":
                                fila.Cells["tipo_pago"].Value = "Pago inicial";
                                break;

                            case "SALDO_ALOJAMIENTO":
                                fila.Cells["tipo_pago"].Value = "Saldo de alojamiento";
                                break;

                            case "SERVICIOS_ADICIONALES":
                                fila.Cells["tipo_pago"].Value = "Servicios adicionales";
                                break;
                        }
                    }
                }

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
            lblTituloConsumos.Location = new Point(35, 370);
            lblTituloConsumos.Size = new Size(350, 30);
            lblTituloConsumos.Font =
                new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTituloConsumos.ForeColor = marron;
            lblTituloConsumos.BackColor = Color.Transparent;

            // Grilla de consumos
            dgvConsumos.Location = new Point(35, 405);
            dgvConsumos.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                175);

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
            lblTotalConsumido.Location = new Point(35, 610);
            lblTotalConsumido.Size = new Size(300, 30);
            lblTotalConsumido.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTotalConsumido.ForeColor = marron;

            lblTotalPagadoServicios.Location = new Point(350, 610);
            lblTotalPagadoServicios.Size = new Size(300, 30);
            lblTotalPagadoServicios.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTotalPagadoServicios.ForeColor = marron;

            lblPendienteServicios.Location = new Point(665, 610);
            lblPendienteServicios.Size = new Size(350, 30);
            lblPendienteServicios.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPendienteServicios.ForeColor = dorado;
            EstilizarBotonPago(btnCerrar, "Cerrar", dorado);
            btnCerrar.ForeColor = marron;
            btnCerrar.Size = new Size(130, 42);
            btnCerrar.Location = new Point(
                ClientSize.Width - 165,
                ClientSize.Height - 70);
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (sender, e) => Close();

            Controls.Add(lblTitulo);
            Controls.Add(lblIdReserva);
            Controls.Add(numIdReserva);
            Controls.Add(btnConsultar);
            Controls.Add(btnRegistrarSaldo);
            Controls.Add(btnRegistrarServicios);
            Controls.Add(btnAgregarConsumo);
            Controls.Add(dgvPagos);
            Controls.Add(lblTituloConsumos);
            Controls.Add(dgvConsumos);
            Controls.Add(btnGenerarComprobante);

            Controls.Add(lblTotalConsumido);
            Controls.Add(lblTotalPagadoServicios);
            Controls.Add(lblPendienteServicios);

            Controls.Add(btnCerrar);

            lblTitulo.BringToFront();
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

                btnAceptar.Location = new Point(165, 175);
                btnAceptar.Size = new Size(110, 35);
                btnAceptar.DialogResult = DialogResult.OK;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
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
                    BackColor = dorado
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
                    Location = new Point(25, 60),
                    Size = new Size(390, 65),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnSi = CrearBotonDialogo(
                    "Sí, confirmar", dorado, marron);

                btnSi.Location = new Point(65, 155);
                btnSi.Size = new Size(145, 38);
                btnSi.DialogResult = DialogResult.Yes;

                Button btnNo = CrearBotonDialogo(
                    "No, cancelar", marronClaro, Color.White);

                btnNo.Location = new Point(230, 155);
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

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = (int)numIdReserva.Value;

                // Consultar historial de pagos
                DataTable pagos = negocio.ListarPorReserva(idReserva);

                dgvPagos.DataSource = pagos;

                // Consultar consumos y calcular saldo pendiente
                CargarConsumosYResumen(idReserva, pagos);

                if (pagos.Rows.Count == 0 &&
                    dgvConsumos.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Consulta de reserva",
                        "No se encontraron pagos ni consumos para esta reserva.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al consultar",
                    "No se pudo consultar la información de la reserva.\n\n" +
                    ex.Message,
                    true);
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
        "id_servicios"
    };

            foreach (string columna in columnasOcultas)
            {
                if (dgvConsumos.Columns.Contains(columna))
                    dgvConsumos.Columns[columna].Visible = false;
            }
        }
        private void btnRegistrarSaldo_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = (int)numIdReserva.Value;

                if (!MostrarSelectorMetodoPago(out string metodo))
                    return;

                bool confirmacion = MostrarConfirmacion(
                    "Confirmar pago",
                    "¿Confirmás que recibiste o verificaste el pago del saldo pendiente?");

                if (!confirmacion)
                    return;

                negocio.RegistrarSaldoAlojamiento(
                    idReserva,
                    idUsuario,
                    metodo);

                dgvPagos.DataSource = negocio.ListarPorReserva(idReserva);
                CargarConsumosYResumen(
                idReserva,
                negocio.ListarPorReserva(idReserva));

                MostrarMensaje(
                    "Pago registrado",
                    "El saldo de alojamiento se registró correctamente.");
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar el saldo",
                    "No se pudo registrar el saldo.\n\n" + ex.Message,
                    true);
            }
        }

        private void btnRegistrarServicios_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = (int)numIdReserva.Value;

                if (!MostrarSelectorMetodoPago(out string metodo))
                    return;

                bool confirmacion = MostrarConfirmacion(
                    "Confirmar pago de servicios",
                    "¿Confirmás que recibiste o verificaste el pago de los servicios adicionales?");

                if (!confirmacion)
                    return;

                // Registrar el pago
                negocio.RegistrarServicios(
                    idReserva,
                    idUsuario,
                    metodo);

                // Actualizar la grilla
                DataTable pagos = negocio.ListarPorReserva(idReserva);
                dgvPagos.DataSource = pagos;
                CargarConsumosYResumen(idReserva, pagos);

                // Buscar el último pago aprobado de servicios adicionales
                DataView vista = new DataView(pagos);

                vista.RowFilter =
                    "tipo_pago = 'SERVICIOS_ADICIONALES' AND estado = 'APROBADO'";

                vista.Sort = "id_pago DESC";

                if (vista.Count == 0)
                {
                    throw new Exception(
                        "No se encontró el pago de servicios registrado.");
                }

                // Obtener los datos reales del pago
                decimal monto = Convert.ToDecimal(vista[0]["monto"]);
                string metodoPago = vista[0]["metodo_pago"].ToString();

                string importe = monto.ToString(
                    "C2",
                    new System.Globalization.CultureInfo("es-AR"));

                // Mostrar confirmación detallada
                string detalle =
                    "El pago de servicios adicionales se registró correctamente.\n\n" +
                    "Reserva: N.º " + idReserva + "\n" +
                    "Importe: " + importe + "\n" +
                    "Método de pago: " + metodoPago;

                MostrarMensaje(
                    "Pago registrado",
                    detalle);
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar servicios",
                    "No se pudo registrar el pago de servicios.\n\n" + ex.Message,
                    true);
            }
        }

        private void btnAgregarConsumo_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = (int)numIdReserva.Value;

                using (FormRegistrarConsumos formulario =
                    new FormRegistrarConsumos(idReserva))
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
                    ex.Message,
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
                    "No se pudo generar el comprobante.\n\n" + ex.Message,
                    true);
            }
        }
    }
}
