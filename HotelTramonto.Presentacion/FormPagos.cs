
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormPagos : Form
    {
        private readonly PagoNegocios negocio = new PagoNegocios();
        private readonly int idUsuario;

        private NumericUpDown numIdReserva;
        private DataGridView dgvPagos;
        private Button btnConsultar;
        private Button btnRegistrarSaldo;
        private Button btnCerrar;
        private Button btnRegistrarServicios;
        private Label lblIdReserva;

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
            MinimumSize = new Size(1000, 650);

            lblIdReserva = new Label();
            numIdReserva = new NumericUpDown();
            dgvPagos = new DataGridView();
            btnConsultar = new Button();
            btnRegistrarSaldo = new Button();
            btnCerrar = new Button();
            btnRegistrarServicios = new Button();

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

            dgvPagos.Location = new Point(35, 165);
            dgvPagos.Size = new Size(
                Math.Max(400, ClientSize.Width - 70),
                Math.Max(200, ClientSize.Height - 260));

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
            dgvPagos.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;
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
                if (dgvPagos.Columns.Contains("monto"))
                {
                    dgvPagos.Columns["monto"].DefaultCellStyle.Format = "N2";
                    dgvPagos.Columns["monto"].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
                }
            };

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
            Controls.Add(dgvPagos);
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
                mensaje.ClientSize = new Size(440, 190);
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
                    Size = new Size(400, 70),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnAceptar = CrearBotonDialogo(
                    "Aceptar", dorado, marron);

                btnAceptar.Location = new Point(165, 140);
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

                DataTable pagos = negocio.ListarPorReserva(idReserva);
                dgvPagos.DataSource = pagos;

                if (pagos.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Consulta de pagos",
                        "No se encontraron pagos para esa reserva.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al consultar",
                    "No se pudieron consultar los pagos.\n\n" + ex.Message,
                    true);
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

                negocio.RegistrarServicios(
                    idReserva,
                    idUsuario,
                    metodo);

                dgvPagos.DataSource = negocio.ListarPorReserva(idReserva);

                MostrarMensaje(
                    "Pago registrado",
                    "El pago de servicios adicionales se registró correctamente.");
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar servicios",
                    "No se pudo registrar el pago de servicios.\n\n" + ex.Message,
                    true);
            }
        }
    }
}
