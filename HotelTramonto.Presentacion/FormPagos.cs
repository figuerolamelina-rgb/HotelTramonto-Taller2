
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
            Text = "Gestión de pagos";
            Width = 900;
            Height = 500;
            StartPosition = FormStartPosition.CenterScreen;

            lblIdReserva = new Label
            {
                Text = "ID de reserva:",
                Left = 20,
                Top = 25,
                Width = 100
            };

            numIdReserva = new NumericUpDown
            {
                Left = 125,
                Top = 20,
                Width = 120,
                Minimum = 1,
                Maximum = 1000000
            };

            btnConsultar = new Button
            {
                Text = "Consultar pagos",
                Left = 265,
                Top = 18,
                Width = 140,
                Height = 30
            };
            btnConsultar.Click += btnConsultar_Click;

            dgvPagos = new DataGridView
            {
                Left = 20,
                Top = 65,
                Width = 840,
                Height = 330,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Left = 760,
                Top = 410,
                Width = 100,
                Height = 30
            };
            btnCerrar.Click += (sender, e) => Close();


            btnRegistrarSaldo = new Button
            {
                Text = "Registrar saldo",
                Left = 420,
                Top = 18,
                Width = 140,
                Height = 30
            };

            btnRegistrarSaldo.Click += btnRegistrarSaldo_Click;

            btnRegistrarServicios = new Button
            {
                Text = "Registrar servicios",
                Left = 575,
                Top = 18,
                Width = 150,
                Height = 30
            };

            btnRegistrarServicios.Click += btnRegistrarServicios_Click;

            Controls.Add(lblIdReserva);
            Controls.Add(numIdReserva);
            Controls.Add(btnConsultar);
            Controls.Add(btnRegistrarSaldo);
            Controls.Add(btnRegistrarServicios);
            Controls.Add(dgvPagos);
            Controls.Add(btnCerrar);

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
