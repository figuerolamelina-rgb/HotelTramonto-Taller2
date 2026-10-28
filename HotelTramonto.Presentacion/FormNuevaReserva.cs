
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using System.Linq;

namespace HotelTramonto.Presentacion
{
    public partial class FormNuevaReserva : Form
    {
        private readonly int idUsuario;

        private readonly HuespedNegocios huespedNegocios = new HuespedNegocios();
        private readonly HabitacionNegocios habitacionNegocios = new HabitacionNegocios();
        private readonly TipoHabitacionNegocios tipoNegocios = new TipoHabitacionNegocios();

        private readonly ReservaTemporalNegocios temporalNegocios = new ReservaTemporalNegocios();
        private readonly ReservaNegocios reservaNegocios = new ReservaNegocios();
        private readonly PagoNegocios pagoNegocios = new PagoNegocios();

        private ComboBox cmbHuesped = new ComboBox();
        private ComboBox cmbHabitacion = new ComboBox();
        private DateTimePicker dtpEntrada = new DateTimePicker();
        private DateTimePicker dtpSalida = new DateTimePicker();
        private NumericUpDown nudPersonas = new NumericUpDown();
        private ComboBox cmbModalidadPago = new ComboBox();
        private ComboBox cmbMetodoPago = new ComboBox();
        private Label lblTotal = new Label();
        private Button btnGuardar = new Button();
        private Button btnCancelar = new Button();

        private DataTable tiposHabitacion = new DataTable();

        public FormNuevaReserva(int idUsuario)
        {
            InitializeComponent();

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario autenticado no es válido.");

            this.idUsuario = idUsuario;

            ConfigurarFormulario();
            CargarHuespedes();
            CargarHabitaciones();
            ConfigurarEventos();
            ActualizarTotal();
        }

        private void ConfigurarFormulario()
        {
            Text = "Nueva reserva";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 570);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Clear();

            AgregarEtiqueta("Huésped:", 25, 25);
            ConfigurarCombo(cmbHuesped, 190, 22, 280);

            AgregarEtiqueta("Habitación:", 25, 75);
            ConfigurarCombo(cmbHabitacion, 190, 72, 280);

            AgregarEtiqueta("Fecha de entrada:", 25, 125);
            ConfigurarFecha(dtpEntrada, 190, 122);

            AgregarEtiqueta("Fecha de salida:", 25, 175);
            ConfigurarFecha(dtpSalida, 190, 172);

            AgregarEtiqueta("Cantidad de personas:", 25, 225);
            nudPersonas.Location = new Point(190, 222);
            nudPersonas.Width = 100;
            nudPersonas.Minimum = 1;
            nudPersonas.Maximum = 20;
            nudPersonas.Value = 1;
            Controls.Add(nudPersonas);

            AgregarEtiqueta("Modalidad de pago:", 25, 275);
            ConfigurarCombo(cmbModalidadPago, 190, 272, 280);
            cmbModalidadPago.Items.Add("Pago total");
            cmbModalidadPago.Items.Add("Seña del 50 %");
            cmbModalidadPago.SelectedIndex = 0;

            AgregarEtiqueta("Método de pago:", 25, 325);
            ConfigurarCombo(cmbMetodoPago, 190, 322, 280);
            cmbMetodoPago.Items.AddRange(new object[]
            {
                "EFECTIVO",
                "TARJETA",
                "TRANSFERENCIA"
            });
            cmbMetodoPago.SelectedIndex = 0;

            lblTotal.Location = new Point(25, 380);
            lblTotal.Size = new Size(445, 50);
            lblTotal.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            Controls.Add(lblTotal);

            btnGuardar.Text = "Crear reserva";
            btnGuardar.Location = new Point(190, 455);
            btnGuardar.Size = new Size(130, 35);
            btnGuardar.Click += btnGuardar_Click;
            Controls.Add(btnGuardar);

            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(330, 455);
            btnCancelar.Size = new Size(100, 35);
            btnCancelar.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancelar);

            CancelButton = btnCancelar;
        }

        private void AgregarEtiqueta(string texto, int izquierda, int arriba)
        {
            Label etiqueta = new Label
            {
                Text = texto,
                Location = new Point(izquierda, arriba + 3),
                AutoSize = true
            };

            Controls.Add(etiqueta);
        }

        private void ConfigurarCombo(ComboBox combo, int izquierda, int arriba, int ancho)
        {
            combo.Location = new Point(izquierda, arriba);
            combo.Width = ancho;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(combo);
        }

        private void ConfigurarFecha(DateTimePicker fecha, int izquierda, int arriba)
        {
            fecha.Location = new Point(izquierda, arriba);
            fecha.Width = 180;
            fecha.Format = DateTimePickerFormat.Short;
            fecha.MinDate = DateTime.Today;
            fecha.Value = DateTime.Today.AddDays(1);
            Controls.Add(fecha);
        }

        private void CargarHuespedes()
        {
            DataTable tabla = huespedNegocios.Listar();
            DataTable activos = tabla.Clone();

            foreach (DataRow fila in tabla.Rows)
            {
                if (Convert.ToBoolean(fila["activo"]))
                    activos.ImportRow(fila);
            }

            if (!activos.Columns.Contains("nombre_completo"))
                activos.Columns.Add("nombre_completo", typeof(string));

            foreach (DataRow fila in activos.Rows)
            {
                fila["nombre_completo"] =
                    fila["apellido"] + ", " + fila["nombre"] +
                    " - DNI " + fila["dni"];
            }

            cmbHuesped.DataSource = activos;
            cmbHuesped.DisplayMember = "nombre_completo";
            cmbHuesped.ValueMember = "id_huesped";
        }

        private void CargarHabitaciones()
        {
            DataTable habitaciones = habitacionNegocios.Listar();

            tiposHabitacion = tipoNegocios.Listar();
           

            DataTable disponiblesParaSeleccion = habitaciones.Clone();

            foreach (DataRow fila in habitaciones.Rows)
            {
                string estado = Convert.ToString(fila["estado"]) ?? "";

                if (!estado.Equals("MANTENIMIENTO", StringComparison.OrdinalIgnoreCase))
                    disponiblesParaSeleccion.ImportRow(fila);
            }

            if (!disponiblesParaSeleccion.Columns.Contains("descripcion_habitacion"))
                disponiblesParaSeleccion.Columns.Add("descripcion_habitacion", typeof(string));

            foreach (DataRow fila in disponiblesParaSeleccion.Rows)
            {
                fila["descripcion_habitacion"] =
                    "N.º " + fila["numero"] +
                    " - " + fila["tipo"] +
                    " - Piso " + fila["piso"];
            }

            cmbHabitacion.DataSource = disponiblesParaSeleccion;
            cmbHabitacion.DisplayMember = "descripcion_habitacion";
            cmbHabitacion.ValueMember = "id_habitacion";
        }

        private void ConfigurarEventos()
        {
            cmbHabitacion.SelectedIndexChanged += (s, e) => ActualizarTotal();
            cmbModalidadPago.SelectedIndexChanged += (s, e) => ActualizarTotal();

            nudPersonas.ValueChanged += (s, e) => ActualizarTotal();

            dtpEntrada.ValueChanged += (s, e) =>
            {
                dtpSalida.MinDate = dtpEntrada.Value.Date.AddDays(1);

                if (dtpSalida.Value.Date <= dtpEntrada.Value.Date)
                    dtpSalida.Value = dtpSalida.MinDate;

                ActualizarTotal();
            };

            dtpSalida.ValueChanged += (s, e) => ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            if (lblTotal == null)
                return;

            if (cmbHabitacion.SelectedItem is not DataRowView habitacion ||
                cmbModalidadPago.SelectedIndex < 0)
            {
                lblTotal.Text = "Seleccioná una habitación para calcular el importe.";
                return;
            }

            int idTipo = Convert.ToInt32(habitacion["tipo_habitacion"]);

            DataRow tipoEncontrado = null;

            foreach (DataRow filaTipo in tiposHabitacion.Rows)
            {
                if (Convert.ToInt32(filaTipo["id_tipoHabitacion"]) == idTipo)
                {
                    tipoEncontrado = filaTipo;
                    break;
                }
            }

            if (tipoEncontrado == null)
            {
                lblTotal.Text = "No se encontró el tipo de habitación.";
                return;
            }

   

            decimal precioNoche = Convert.ToDecimal(tipoEncontrado["precio_noche"]);
            int noches = Math.Max(1, (dtpSalida.Value.Date - dtpEntrada.Value.Date).Days);
            decimal total = precioNoche * noches;
            decimal pagoInicial = cmbModalidadPago.SelectedIndex == 0
                ? total
                : Math.Round(total / 2m, 2);

            lblTotal.Text =
                $"Noches: {noches} | Total: {total:C2}\n" +
                $"Pago inicial: {pagoInicial:C2} | Saldo: {(total - pagoInicial):C2}";
        }


        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbHuesped.SelectedValue == null ||
                cmbHabitacion.SelectedValue == null ||
                cmbModalidadPago.SelectedIndex < 0 ||
                cmbMetodoPago.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Completá todos los campos.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idHuesped = Convert.ToInt32(cmbHuesped.SelectedValue);
                int idHabitacion = Convert.ToInt32(cmbHabitacion.SelectedValue);
                int cantidadPersonas = Convert.ToInt32(nudPersonas.Value);

                DateTime entrada = dtpEntrada.Value.Date;
                DateTime salida = dtpSalida.Value.Date;

                string modalidad = cmbModalidadPago.SelectedIndex == 0
                    ? "PAGO_TOTAL"
                    : "SENA_50";

                string metodo = cmbMetodoPago.SelectedItem.ToString()!;

                DataRowView habitacionSeleccionada =
                    (DataRowView)cmbHabitacion.SelectedItem;

                int idTipo = Convert.ToInt32(habitacionSeleccionada["tipo_habitacion"]);

                DataRow tipoEncontrado = null;

                foreach (DataRow filaTipo in tiposHabitacion.Rows)
                {
                    if (Convert.ToInt32(filaTipo["id_tipoHabitacion"]) == idTipo)
                    {
                        tipoEncontrado = filaTipo;
                        break;
                    }
                }

                if (tipoEncontrado == null)
                {
                    MessageBox.Show("No se encontró el tipo de habitación.");
                    return;
                }

                int capacidad = Convert.ToInt32(tipoEncontrado["capacidad"]);

                if (cantidadPersonas > capacidad)
                {
                    MessageBox.Show(
                        $"La habitación admite como máximo {capacidad} persona(s).",
                        "Capacidad excedida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                int noches = (salida - entrada).Days;
                if (noches <= 0)
                {
                    MessageBox.Show("La salida debe ser posterior a la entrada.");
                    return;
                }

                decimal precioNoche = Convert.ToDecimal(tipoEncontrado["precio_noche"]);
                decimal total = precioNoche * noches;
                decimal pagoInicial = modalidad == "PAGO_TOTAL"
                    ? total
                    : Math.Round(total / 2m, 2);

                DialogResult confirmacion = MessageBox.Show(
                    $"Huésped: {cmbHuesped.Text}\n" +
                    $"Habitación: {cmbHabitacion.Text}\n" +
                    $"Entrada: {entrada:dd/MM/yyyy}\n" +
                    $"Salida: {salida:dd/MM/yyyy}\n" +
                    $"Personas: {cantidadPersonas}\n" +
                    $"Total: {total:C2}\n" +
                    $"Pago inicial: {pagoInicial:C2}\n\n" +
                    "¿Confirmás que recibiste o verificaste este pago?",
                    "Confirmar reserva y pago",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes)
                    return;

                int idTemporal = temporalNegocios.Insertar(
                    idHuesped,
                    idHabitacion,
                    entrada,
                    salida,
                    modalidad);

                pagoNegocios.Insertar(
                    null,
                    idTemporal,
                    idUsuario,
                    pagoInicial,
                    metodo,
                    "APROBADO",
                    "PAGO_INICIAL");

                DataTable resultado = reservaNegocios.ConfirmarReserva(
                    idTemporal,
                    cantidadPersonas);

                if (resultado.Rows.Count > 0)
                {
                    DataRow reserva = resultado.Rows[0];

                    string mensaje =
                        $"Reserva creada correctamente.\n" +
                        $"Número de reserva: {reserva["id_reserva"]}\n" +
                        $"Total: {Convert.ToDecimal(reserva["total_alojamiento"]):C2}\n" +
                        $"Pago inicial: {Convert.ToDecimal(reserva["pago_inicial"]):C2}\n" +
                        $"Saldo pendiente: {Convert.ToDecimal(reserva["saldo_alojamiento"]):C2}";

                    MessageBox.Show(
                        mensaje,
                        "Reserva confirmada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "No se obtuvo información de la reserva confirmada.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo crear la reserva.\n\nDetalle: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}
