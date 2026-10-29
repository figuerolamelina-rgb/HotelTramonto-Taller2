
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelTramonto.Negocios;

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

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

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
            Text = "Nueva reserva - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(700, 620);
            MinimumSize = new Size(716, 659);
            MaximumSize = new Size(716, 659);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);

            Controls.Clear();

            // Encabezado
            Panel panelTitulo = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(700, 90),
                BackColor = marron
            };

            Label lblTitulo = new Label
            {
                Text = "NUEVA RESERVA",
                Location = new Point(25, 12),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold)
            };

            Label lblSubtitulo = new Label
            {
                Text = "Completá los datos de alojamiento y pago",
                Location = new Point(28, 53),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 9.5F)
            };

            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(lblSubtitulo);
            Controls.Add(panelTitulo);

            // Panel del formulario
            Panel panelFormulario = new Panel
            {
                Location = new Point(20, 105),
                Size = new Size(660, 430),
                BackColor = marfil,
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(panelFormulario);

            int etiquetaX = 25;
            int campoX = 225;

            // Huésped
            AgregarEtiqueta(panelFormulario, "Huésped", etiquetaX, 15);
            ConfigurarCombo(panelFormulario, cmbHuesped, campoX, 12, 400);

            // Habitación
            AgregarEtiqueta(panelFormulario, "Habitación", etiquetaX, 60);
            ConfigurarCombo(panelFormulario, cmbHabitacion, campoX, 57, 400);

            // Fechas
            AgregarEtiqueta(panelFormulario, "Fecha de entrada", etiquetaX, 105);
            ConfigurarFecha(panelFormulario, dtpEntrada, campoX, 102);

            AgregarEtiqueta(panelFormulario, "Fecha de salida", etiquetaX, 150);
            ConfigurarFecha(panelFormulario, dtpSalida, campoX, 147);

            // Cantidad de personas
            AgregarEtiqueta(panelFormulario, "Cantidad de personas", etiquetaX, 195);

            nudPersonas.Location = new Point(campoX, 192);
            nudPersonas.Size = new Size(120, 28);
            nudPersonas.Minimum = 1;
            nudPersonas.Maximum = 20;
            nudPersonas.Value = 1;
            nudPersonas.BackColor = marfil;
            nudPersonas.ForeColor = marron;
            nudPersonas.Font = new Font("Segoe UI", 10F);

            panelFormulario.Controls.Add(nudPersonas);

            // Modalidad de pago
            AgregarEtiqueta(panelFormulario, "Modalidad de pago", etiquetaX, 240);
            ConfigurarCombo(panelFormulario, cmbModalidadPago, campoX, 237, 400);

            cmbModalidadPago.Items.AddRange(new object[]
            {
        "Pago total",
        "Seña del 50 %"
            });

            cmbModalidadPago.SelectedIndex = 0;

            // Método de pago
            AgregarEtiqueta(panelFormulario, "Método de pago", etiquetaX, 285);
            ConfigurarCombo(panelFormulario, cmbMetodoPago, campoX, 282, 400);

            cmbMetodoPago.Items.AddRange(new object[]
            {
        "EFECTIVO",
        "TARJETA",
        "TRANSFERENCIA"
            });

            cmbMetodoPago.SelectedIndex = 0;

            // Resumen del alojamiento
            Panel panelTotal = new Panel
            {
                Location = new Point(20, 325),
                Size = new Size(618, 78),
                BackColor = crema
            };

            Label lblResumen = new Label
            {
                Text = "RESUMEN DEL ALOJAMIENTO",
                Location = new Point(12, 7),
                AutoSize = true,
                ForeColor = marronClaro,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblTotal.Location = new Point(12, 30);
            lblTotal.Size = new Size(594, 43);
            lblTotal.ForeColor = marron;
            lblTotal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            panelTotal.Controls.Add(lblResumen);
            panelTotal.Controls.Add(lblTotal);
            panelFormulario.Controls.Add(panelTotal);

            // Botones visibles en la parte inferior
            btnGuardar = CrearBoton("Confirmar reserva", dorado, marron);
            btnGuardar.Location = new Point(220, 555);
            btnGuardar.Size = new Size(190, 40);
            btnGuardar.Click += btnGuardar_Click;
            Controls.Add(btnGuardar);

            btnCancelar = CrearBoton("Cancelar", marronClaro, Color.White);
            btnCancelar.Location = new Point(425, 555);
            btnCancelar.Size = new Size(125, 40);
            btnCancelar.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }


        private void AgregarEtiqueta(
            Control contenedor, string texto, int izquierda, int arriba)
        {
            Label etiqueta = new Label
            {
                Text = texto,
                Location = new Point(izquierda, arriba + 4),
                AutoSize = true,
                ForeColor = marron,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            contenedor.Controls.Add(etiqueta);
        }

        private void ConfigurarCombo(
            Control contenedor, ComboBox combo,
            int izquierda, int arriba, int ancho)
        {
            combo.Location = new Point(izquierda, arriba);
            combo.Size = new Size(ancho, 30);
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.BackColor = marfil;
            combo.ForeColor = marron;
            combo.Font = new Font("Segoe UI", 10F);

            contenedor.Controls.Add(combo);
        }

        private void ConfigurarFecha(
            Control contenedor, DateTimePicker fecha,
            int izquierda, int arriba)
        {
            fecha.Location = new Point(izquierda, arriba);
            fecha.Size = new Size(180, 30);
            fecha.Format = DateTimePickerFormat.Custom;
            fecha.CustomFormat = "dd/MM/yyyy";
            fecha.MinDate = DateTime.Today;
            fecha.Value = DateTime.Today.AddDays(1);
            fecha.Font = new Font("Segoe UI", 10F);
            fecha.CalendarForeColor = marron;

            contenedor.Controls.Add(fecha);
        }

        private Button CrearBoton(
            string texto, Color fondo, Color textoColor)
        {
            Button boton = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = textoColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(174, 139, 76);

            return boton;
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

                if (!estado.Equals(
                    "MANTENIMIENTO", StringComparison.OrdinalIgnoreCase))
                {
                    disponiblesParaSeleccion.ImportRow(fila);
                }
            }

            if (!disponiblesParaSeleccion.Columns.Contains(
                "descripcion_habitacion"))
            {
                disponiblesParaSeleccion.Columns.Add(
                    "descripcion_habitacion", typeof(string));
            }

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
                lblTotal.Text =
                    "Seleccioná una habitación para calcular el importe.";
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

            decimal precioNoche =
                Convert.ToDecimal(tipoEncontrado["precio_noche"]);

            int noches = Math.Max(
                1, (dtpSalida.Value.Date - dtpEntrada.Value.Date).Days);

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

                int idTipo = Convert.ToInt32(
                    habitacionSeleccionada["tipo_habitacion"]);

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
                    MessageBox.Show(
                        "La salida debe ser posterior a la entrada.");
                    return;
                }

                decimal precioNoche =
                    Convert.ToDecimal(tipoEncontrado["precio_noche"]);

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
