
using System;
using System.Data;
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
            // Paleta Hotel Tramonto
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color marfil = Color.FromArgb(255, 252, 247);
            Color dorado = Color.FromArgb(198, 161, 91);
            Color texto = Color.FromArgb(66, 51, 43);

            // Ventana
            Text = "Hotel Tramonto | Gestión de pagos";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);

            // Inicializar TODOS los controles antes de configurarlos
            lblIdReserva = new Label();
            numIdReserva = new NumericUpDown();
            dgvPagos = new DataGridView();
            btnConsultar = new Button();
            btnRegistrarSaldo = new Button();
            btnCerrar = new Button();
            btnRegistrarServicios = new Button();

            // Título
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

            // ID de reserva
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

            // Botones superiores
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

            // Tabla
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
            dgvPagos.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
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
            dgvPagos.DefaultCellStyle.ForeColor = texto;
            dgvPagos.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvPagos.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);
            dgvPagos.DefaultCellStyle.SelectionForeColor = marron;

            dgvPagos.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            // Formato del importe cada vez que se cargan pagos
            dgvPagos.DataBindingComplete += (sender, e) =>
            {
                if (dgvPagos.Columns.Contains("monto"))
                {
                    dgvPagos.Columns["monto"].DefaultCellStyle.Format = "N2";
                    dgvPagos.Columns["monto"].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
                }
            };

            // Botón cerrar
            EstilizarBotonPago(btnCerrar, "Cerrar", dorado);
            btnCerrar.ForeColor = marron;
            btnCerrar.Size = new Size(130, 42);
            btnCerrar.Location = new Point(
                ClientSize.Width - 165,
                ClientSize.Height - 70);
            btnCerrar.Anchor =
                AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (sender, e) => Close();

            // Agregar cada control una sola vez
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
            boton.ForeColor = Color.FromArgb(255, 252, 247);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(105, 73, 52);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
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
                    MessageBox.Show(
                        "No se encontraron pagos para esa reserva.",
                        "Consulta de pagos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al consultar los pagos: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void btnRegistrarSaldo_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = (int)numIdReserva.Value;

                using Form selector = new Form
                {
                    Text = "Método de pago",
                    Width = 320,
                    Height = 180,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                Label lblMetodo = new Label
                {
                    Text = "Seleccioná el método de pago:",
                    Left = 15,
                    Top = 15,
                    Width = 260
                };

                ComboBox cmbMetodo = new ComboBox
                {
                    Left = 15,
                    Top = 45,
                    Width = 270,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                cmbMetodo.Items.AddRange(new object[]
                {
            "EFECTIVO",
            "TARJETA",
            "TRANSFERENCIA"
                });
                cmbMetodo.SelectedIndex = 0;

                Button btnAceptar = new Button
                {
                    Text = "Registrar",
                    Left = 95,
                    Top = 85,
                    Width = 100,
                    DialogResult = DialogResult.OK
                };

                selector.Controls.Add(lblMetodo);
                selector.Controls.Add(cmbMetodo);
                selector.Controls.Add(btnAceptar);
                selector.AcceptButton = btnAceptar;

                if (selector.ShowDialog(this) != DialogResult.OK)
                    return;

                DialogResult confirmacion = MessageBox.Show(
                    "¿Confirmás que recibiste o verificaste el pago del saldo pendiente?",
                    "Confirmar pago",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

                DataTable resultado = negocio.RegistrarSaldoAlojamiento(idReserva, idUsuario,cmbMetodo.SelectedItem!.ToString()!);

                dgvPagos.DataSource = negocio.ListarPorReserva(idReserva);

                MessageBox.Show("Saldo registrado correctamente.","Gestión de pagos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo registrar el saldo: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        
private void btnRegistrarServicios_Click(object sender, EventArgs e)
{
    try
    {
        int idReserva = (int)numIdReserva.Value;

        using Form selector = new Form
        {
            Text = "Método de pago",
            Width = 320,
            Height = 180,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        Label lblMetodo = new Label
        {
            Text = "Seleccioná el método de pago:",
            Left = 15,
            Top = 15,
            Width = 260
        };

        ComboBox cmbMetodo = new ComboBox
        {
            Left = 15,
            Top = 45,
            Width = 270,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        cmbMetodo.Items.AddRange(new object[]
        {
            "EFECTIVO",
            "TARJETA",
            "TRANSFERENCIA"
        });
        cmbMetodo.SelectedIndex = 0;

        Button btnAceptar = new Button
        {
            Text = "Registrar",
            Left = 95,
            Top = 85,
            Width = 100,
            DialogResult = DialogResult.OK
        };

        selector.Controls.Add(lblMetodo);
        selector.Controls.Add(cmbMetodo);
        selector.Controls.Add(btnAceptar);
        selector.AcceptButton = btnAceptar;

        if (selector.ShowDialog(this) != DialogResult.OK)
            return;

        DialogResult confirmacion = MessageBox.Show(
            "¿Confirmás que recibiste o verificaste el pago de los servicios adicionales?",
            "Confirmar pago",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmacion != DialogResult.Yes)
            return;

        negocio.RegistrarServicios(
            idReserva,
            idUsuario,
            cmbMetodo.SelectedItem!.ToString()!);

        dgvPagos.DataSource = negocio.ListarPorReserva(idReserva);

        MessageBox.Show(
            "Pago de servicios adicionales registrado correctamente.",
            "Gestión de pagos",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "No se pudo registrar el pago de servicios: " + ex.Message,
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

    }
}
