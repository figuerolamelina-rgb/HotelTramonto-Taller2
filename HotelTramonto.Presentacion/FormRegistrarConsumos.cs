
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormRegistrarConsumos : Form
    {
        private int idReserva;

        private readonly HabitacionNegocios habitacionNegocio =
            new HabitacionNegocios();

        private ComboBox cmbHabitacion;
        private Label lblHabitacion;

        private readonly ServicioNegocios servicioNegocio =
            new ServicioNegocios();

        private readonly DetalleServicioNegocios detalleNegocio =
            new DetalleServicioNegocios();

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private ComboBox cmbCategoria;
        private ComboBox cmbProducto;
        private NumericUpDown nudCantidad;
        private Label lblPrecio;
        private Label lblSubtotal;
        private Label lblReserva;
        private DataGridView dgvConsumos;
        private Button btnAgregar;
        private Button btnCerrar;

        private DataTable tablaProductos;
        private DataTable tablaConsumos;

        private decimal precioSeleccionado = 0;

        public FormRegistrarConsumos()
        {
            idReserva = 0;

            InitializeComponent();
            ConfigurarFormulario();
            CargarProductos();

            CargarHabitaciones();
            btnAgregar.Enabled = false;
        }

        public FormRegistrarConsumos(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("El ID de reserva no es válido.");

            this.idReserva = idReserva;

            InitializeComponent();
            ConfigurarFormulario();

            lblHabitacion.Visible = false;
            cmbHabitacion.Visible = false;

            CargarProductos();
            CargarConsumos();
        }

        private void ConfigurarFormulario()
        {
            Text = "Hotel Tramonto | Registrar consumos";
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(950, 650);
            Size = new Size(1100, 720);

            Label lblTitulo = new Label
            {
                Text = "REGISTRO DE CONSUMOS",
                Font = new Font("Georgia", 23F, FontStyle.Bold),
                ForeColor = dorado,
                AutoSize = true,
                Location = new Point(30, 20)
            };

            lblHabitacion = CrearEtiqueta("Habitación", 35, 75);

            cmbHabitacion = new ComboBox
            {
                Location = new Point(140, 70),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = marfil,
                ForeColor = marron
            };

            cmbHabitacion.SelectedIndexChanged +=
                cmbHabitacion_SelectedIndexChanged;

            lblReserva = new Label
            {
                Text = idReserva > 0
                    ? "Reserva N.º: " + idReserva
                    : "Seleccioná una habitación",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = marron,
                AutoSize = true,
                Location = new Point(470, 75)
            };

            Label lblCategoria = CrearEtiqueta("Categoría", 35, 120);
            cmbCategoria = new ComboBox
            {
                Location = new Point(35, 148),
                Size = new Size(220, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = marfil,
                ForeColor = marron
            };
            cmbCategoria.SelectedIndexChanged += cmbCategoria_SelectedIndexChanged;

            Label lblProducto = CrearEtiqueta("Producto o servicio", 280, 120);
            cmbProducto = new ComboBox
            {
                Location = new Point(280, 148),
                Size = new Size(330, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = marfil,
                ForeColor = marron,
                DisplayMember = "nombre",
                ValueMember = "id_servicio"
            };
            cmbProducto.SelectedIndexChanged += cmbProducto_SelectedIndexChanged;

            Label lblCantidad = CrearEtiqueta("Cantidad", 635, 120);
            nudCantidad = new NumericUpDown
            {
                Location = new Point(635, 148),
                Size = new Size(110, 30),
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                BackColor = marfil,
                ForeColor = marron
            };
            nudCantidad.ValueChanged += nudCantidad_ValueChanged;

            Label lblPrecioUnitario = CrearEtiqueta("Precio unitario", 770, 120);
            lblPrecio = new Label
            {
                Text = "$ 0,00",
                Location = new Point(770, 150),
                Size = new Size(130, 25),
                ForeColor = marron,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            Label lblSubtotalTitulo = CrearEtiqueta("Subtotal", 910, 120);
            lblSubtotal = new Label
            {
                Text = "$ 0,00",
                Location = new Point(910, 150),
                Size = new Size(140, 25),
                ForeColor = dorado,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            btnAgregar = CrearBoton("Agregar consumo", dorado, marron);
            btnAgregar.Location = new Point(35, 205);
            btnAgregar.Size = new Size(175, 40);
            btnAgregar.Click += btnAgregar_Click;

            Label lblListado = new Label
            {
                Text = "CONSUMOS DE LA RESERVA",
                Location = new Point(35, 275),
                AutoSize = true,
                ForeColor = marron,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            dgvConsumos = new DataGridView
            {
                Location = new Point(35, 310),
                Size = new Size(1015, 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = marfil,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(229, 218, 202),
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 35 },
                ColumnHeadersHeight = 40
            };

            dgvConsumos.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvConsumos.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvConsumos.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvConsumos.DefaultCellStyle.BackColor = marfil;
            dgvConsumos.DefaultCellStyle.ForeColor = marron;
            dgvConsumos.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);
            dgvConsumos.DefaultCellStyle.SelectionForeColor = marron;

            dgvConsumos.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            btnCerrar = CrearBoton("Cerrar", marronClaro, Color.White);
            btnCerrar.Location = new Point(900, 610);
            btnCerrar.Size = new Size(150, 40);
            btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrar.Click += (s, e) => Close();

            Controls.Add(lblTitulo);
            Controls.Add(lblReserva);
            Controls.Add(lblHabitacion);
            Controls.Add(cmbHabitacion);
            Controls.Add(lblCategoria);
            Controls.Add(cmbCategoria);
            Controls.Add(lblProducto);
            Controls.Add(cmbProducto);
            Controls.Add(lblCantidad);
            Controls.Add(nudCantidad);
            Controls.Add(lblPrecioUnitario);
            Controls.Add(lblPrecio);
            Controls.Add(lblSubtotalTitulo);
            Controls.Add(lblSubtotal);
            Controls.Add(btnAgregar);
            Controls.Add(lblListado);
            Controls.Add(dgvConsumos);
            Controls.Add(btnCerrar);
        }

        private Label CrearEtiqueta(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = marron,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
        }

        private Button CrearBoton(
            string texto,
            Color fondo,
            Color colorTexto)
        {
            Button boton = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = colorTexto,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);

            return boton;
        }

        private void CargarProductos()
        {
            try
            {
                tablaProductos = servicioNegocio.ListarParaConsumo();

                cmbCategoria.Items.Clear();
                cmbCategoria.Items.Add("Todas");

                foreach (DataRow fila in tablaProductos.Rows)
                {
                    string categoria = fila["categoria"].ToString();

                    if (!cmbCategoria.Items.Contains(categoria))
                        cmbCategoria.Items.Add(categoria);
                }

                cmbCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error",
                    "No se pudieron cargar los productos.\n\n" + ex.Message,
                    true);
            }
        }
        private void CargarHabitaciones()
        {
            try
            {
                DataTable habitaciones =
                    habitacionNegocio.ListarConReservaCheckIn();

                if (!habitaciones.Columns.Contains("descripcion"))
                    habitaciones.Columns.Add("descripcion", typeof(string));

                foreach (DataRow fila in habitaciones.Rows)
                {
                    fila["descripcion"] =
                        "Habitación " + fila["numero"] +
                        " - Piso " + fila["piso"] +
                        " - " + fila["nombre_huesped"] +
                        " " + fila["apellido_huesped"];
                }

                cmbHabitacion.DataSource = null;
                cmbHabitacion.DisplayMember = "descripcion";
                cmbHabitacion.ValueMember = "id_habitacion";
                cmbHabitacion.DataSource = habitaciones;
                cmbHabitacion.SelectedIndex = -1;

                if (habitaciones.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Sin habitaciones ocupadas",
                        "No hay habitaciones con reservas que tengan check-in realizado.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar habitaciones",
                    "No se pudieron cargar las habitaciones.\n\n" + ex.Message,
                    true);
            }
        }
        private void FiltrarProductos()
        {
            if (tablaProductos == null)
                return;

            DataView vista = new DataView(tablaProductos);

            string categoria =
                cmbCategoria.SelectedItem?.ToString() ?? "Todas";

            if (categoria != "Todas")
            {
                vista.RowFilter =
                    "categoria = '" +
                    categoria.Replace("'", "''") + "'";
            }

            cmbProducto.DataSource = null;
            cmbProducto.DataSource = vista;
            cmbProducto.DisplayMember = "nombre";
            cmbProducto.ValueMember = "id_servicio";

            ActualizarPrecio();
        }

        private void cmbCategoria_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            FiltrarProductos();
        }

        private void cmbProducto_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ActualizarPrecio();
        }

        private void ActualizarPrecio()
        {
            if (cmbProducto.SelectedItem is DataRowView fila)
            {
                precioSeleccionado =
                    Convert.ToDecimal(fila["precio"]);

                lblPrecio.Text = precioSeleccionado.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("es-AR"));
            }
            else
            {
                precioSeleccionado = 0;
                lblPrecio.Text = "$ 0,00";
            }

            ActualizarSubtotal();
        }

        private void nudCantidad_ValueChanged(
            object sender,
            EventArgs e)
        {
            ActualizarSubtotal();
        }

        private void ActualizarSubtotal()
        {
            decimal subtotal = precioSeleccionado * nudCantidad.Value;

            lblSubtotal.Text = subtotal.ToString(
                "C2",
                CultureInfo.GetCultureInfo("es-AR"));
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (idReserva <= 0)
            {
                MostrarMensaje(
                    "Habitación requerida",
                    "Primero seleccioná una habitación con una reserva activa.");
                return;
            }

            if (cmbProducto.SelectedItem is not DataRowView producto)
            {
                MostrarMensaje(
                    "Producto requerido",
                    "Seleccioná un producto del catálogo.");
                return;
            }

            int idServicio = Convert.ToInt32(producto["id_servicio"]);
            int cantidad = (int)nudCantidad.Value;

            if (precioSeleccionado <= 0)
            {
                MostrarMensaje(
                    "Precio inválido",
                    "El producto no tiene un precio válido.");
                return;
            }

            string nombre = producto["nombre"].ToString();

            string detalle =
                "Producto: " + nombre +
                "\nCantidad: " + cantidad +
                "\nSubtotal: " +
                (precioSeleccionado * cantidad).ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("es-AR")) +
                "\n\n¿Confirmás el registro de este consumo?";

            if (!MostrarConfirmacion("Confirmar consumo", detalle))
                return;

            try
            {
                detalleNegocio.Insertar(
                    idServicio,
                    idReserva,
                    cantidad,
                    precioSeleccionado);

                MostrarMensaje(
                    "Consumo registrado",
                    "El consumo de " + nombre +
                    " se registró correctamente.");

                nudCantidad.Value = 1;
                CargarConsumos();
                ActualizarSubtotal();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar",
                    "No se pudo registrar el consumo.\n\n" + ex.Message,
                    true);
            }
        }

        private void CargarConsumos()
        {
            if (idReserva <= 0)
            {
                dgvConsumos.DataSource = null;
                return;
            }

            try
            {
                tablaConsumos =
                    detalleNegocio.ListarPorReserva(idReserva);

                dgvConsumos.DataSource = tablaConsumos;

                if (dgvConsumos.Columns.Contains("id_detalle_servicio"))
                    dgvConsumos.Columns["id_detalle_servicio"].Visible = false;

                if (dgvConsumos.Columns.Contains("id_reserva"))
                    dgvConsumos.Columns["id_reserva"].Visible = false;

                if (dgvConsumos.Columns.Contains("id_servicios"))
                    dgvConsumos.Columns["id_servicios"].Visible = false;

                if (dgvConsumos.Columns.Contains("nombre_servicio"))
                    dgvConsumos.Columns["nombre_servicio"].HeaderText = "Producto";

                if (dgvConsumos.Columns.Contains("cantidad"))
                    dgvConsumos.Columns["cantidad"].HeaderText = "Cantidad";

                if (dgvConsumos.Columns.Contains("precio_unitario"))
                {
                    dgvConsumos.Columns["precio_unitario"].HeaderText =
                        "Precio unitario";

                    dgvConsumos.Columns["precio_unitario"].DefaultCellStyle.Format =
                        "C2";

                    dgvConsumos.Columns["precio_unitario"].DefaultCellStyle.FormatProvider =
                        CultureInfo.GetCultureInfo("es-AR");
                }

                if (dgvConsumos.Columns.Contains("subtotal"))
                {
                    dgvConsumos.Columns["subtotal"].HeaderText = "Subtotal";
                    dgvConsumos.Columns["subtotal"].DefaultCellStyle.Format =
                        "C2";

                    dgvConsumos.Columns["subtotal"].DefaultCellStyle.FormatProvider =
                        CultureInfo.GetCultureInfo("es-AR");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al consultar consumos",
                    "No se pudieron consultar los consumos.\n\n" +
                    ex.Message,
                    true);
            }
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
                mensaje.ClientSize = new Size(420, 190);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(420, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTitulo = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 18),
                    Size = new Size(390, 35),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(380, 75),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = CrearBoton("Aceptar", dorado, marron);
                btnAceptar.Location = new Point(155, 140);
                btnAceptar.Size = new Size(110, 35);
                btnAceptar.DialogResult = DialogResult.OK;

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
            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(440, 230);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(440, 7),
                    BackColor = dorado
                };

                Label lblTitulo = new Label
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
                    Size = new Size(400, 95),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnSi = CrearBoton("Sí, confirmar", dorado, marron);
                btnSi.Location = new Point(65, 165);
                btnSi.Size = new Size(145, 38);
                btnSi.DialogResult = DialogResult.Yes;

                Button btnNo = CrearBoton("Cancelar", marronClaro, Color.White);
                btnNo.Location = new Point(230, 165);
                btnNo.Size = new Size(145, 38);
                btnNo.DialogResult = DialogResult.No;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTitulo);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnSi);
                mensaje.Controls.Add(btnNo);

                mensaje.AcceptButton = btnSi;
                mensaje.CancelButton = btnNo;

                return mensaje.ShowDialog(this) == DialogResult.Yes;
            }
        }

        private void cmbHabitacion_SelectedIndexChanged(
    object sender,
    EventArgs e)
        {
            if (cmbHabitacion.SelectedItem is DataRowView habitacion)
            {
                idReserva = Convert.ToInt32(habitacion["id_reserva"]);

                lblReserva.Text = "Reserva N.º: " + idReserva;

                btnAgregar.Enabled = true;

                CargarConsumos();
            }
            else
            {
                idReserva = 0;

                lblReserva.Text = "Seleccioná una habitación";

                btnAgregar.Enabled = false;

                dgvConsumos.DataSource = null;
            }
        }
    }
}
