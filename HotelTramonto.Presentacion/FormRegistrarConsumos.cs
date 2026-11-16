
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
        private readonly string rolOperacion;
        private readonly int idReserva;
        private SolicitudConsumo? consumoPendiente;

        private readonly ServicioNegocios servicioNegocio =
            new ServicioNegocios();

        private readonly DetalleServicioNegocios detalleNegocio;

        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private readonly ComboBox cmbHabitacion = new ComboBox();
        private readonly CheckBox filtrarHabitacion = new CheckBox();
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

        public FormRegistrarConsumos(int idReserva, string rol)
        {
            PermisosOperacion.Exigir(rol);
            rolOperacion = rol;
            detalleNegocio = new DetalleServicioNegocios(rol);
            if (idReserva <= 0)
                throw new ArgumentException("El ID de reserva no es válido.");

            this.idReserva = idReserva;

            InitializeComponent();
            PresentacionTablas.Conectar(this);
            ConfigurarFormulario();
            CargarHabitaciones();
            CargarProductos();
            CargarConsumos();
            Shown += (s, e) => ReanudarConsumo();
        }

        private void ConfigurarFormulario()
        {
            Text = "Hotel Tramonto | Registrar consumos";
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(950, 650);
            AutoScroll = true;
            AutoScrollMinSize = new Size(1070, 660);
            Size = new Size(1100, 720);

            Label lblTitulo = new Label
            {
                Text = "REGISTRO DE CONSUMOS",
                Font = new Font("Georgia", 23F, FontStyle.Bold),
                ForeColor = dorado,
                AutoSize = true,
                Location = new Point(30, 20)
            };

            lblReserva = new Label
            {
                Text = "Reserva N.º: " + idReserva,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = marron,
                AutoSize = true,
                Location = new Point(35, 75)
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
            Label habitacion = CrearEtiqueta("Habitación de la reserva", 280, 185);
            cmbHabitacion.SetBounds(280, 210, 220, 30);
            cmbHabitacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHabitacion.BackColor = marfil;
            cmbHabitacion.SelectedIndexChanged += (s, e) =>
            {
                if (filtrarHabitacion.Checked) CargarConsumos();
            };
            filtrarHabitacion.Text = "Consultar solo esta habitación";
            filtrarHabitacion.SetBounds(530, 210, 330, 30);
            filtrarHabitacion.CheckedChanged += (s, e) => CargarConsumos();
            Controls.Add(habitacion);
            Controls.Add(cmbHabitacion);
            Controls.Add(filtrarHabitacion);

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

        private void CargarHabitaciones()
        {
            try
            {
                cmbHabitacion.DisplayMember = "numero_habitacion";
                cmbHabitacion.ValueMember = "id_habitacion";
                cmbHabitacion.DataSource = detalleNegocio.HabitacionesDeReserva(idReserva);
                cmbHabitacion.SelectedIndex = -1;
            }
            catch (Exception)
            {
                btnAgregar.Enabled = false;
                MostrarMensaje("Habitaciones no disponibles",
                    "No se pudieron consultar las habitaciones de la reserva.", true);
            }
        }

        private void BloquearSolicitud(bool bloquear)
        {
            cmbHabitacion.Enabled = !bloquear;
            cmbCategoria.Enabled = !bloquear;
            cmbProducto.Enabled = !bloquear;
            nudCantidad.Enabled = !bloquear;
            btnAgregar.Text = bloquear ? "Reintentar consumo" : "Agregar consumo";
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
                tablaProductos = servicioNegocio.ListarParaConsumo(rolOperacion);

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
                    "No se pudieron cargar los productos.\n\n" + RegistroErrores.Mensaje(ex),
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
            if (consumoPendiente != null)
            {
                EjecutarConsumo(consumoPendiente);
                return;
            }
            if (cmbProducto.SelectedItem is not DataRowView producto)
            {
                MostrarMensaje(
                    "Producto requerido",
                    "Seleccioná un producto del catálogo.");
                return;
            }

            if (cmbHabitacion.SelectedValue is not int idHabitacion)
            {
                MostrarMensaje("Habitación requerida", "Seleccioná una habitación de esta reserva.");
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
                "Habitación: " + cmbHabitacion.Text + "\nProducto: " + nombre +
                "\nCantidad: " + cantidad +
                "\nSubtotal: " +
                (precioSeleccionado * cantidad).ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("es-AR")) +
                "\n\n¿Confirmás el registro de este consumo?";

            if (!MostrarConfirmacion("Confirmar consumo", detalle))
                return;

            EjecutarConsumo(new SolicitudConsumo(idServicio, idReserva, cantidad,
                precioSeleccionado, idHabitacion));
        }

        private string ClaveConsumo => "consumo:" + idReserva;

        private void ReanudarConsumo()
        {
            try
            {
                var solicitud = DiarioOperaciones.Actual().Leer<SolicitudConsumo>(ClaveConsumo);
                consumoPendiente = solicitud;
                if (solicitud != null) BloquearSolicitud(true);
                if (solicitud != null && MostrarConfirmacion("Consumo pendiente",
                    $"¿Reanudar consumo guardado de habitación #{solicitud.IdHabitacion}, servicio #{solicitud.IdServicio}, " +
                    $"cantidad {solicitud.Cantidad}, precio {solicitud.Precio:C2}?"))
                    EjecutarConsumo(solicitud);
            }
            catch (Exception)
            {
                MostrarMensaje("Operación pendiente", "No se pudo recuperar el consumo. Conservá el diario para revisión.", true);
            }
        }

        private void EjecutarConsumo(SolicitudConsumo solicitud)
        {
            consumoPendiente = solicitud;
            BloquearSolicitud(true);
            btnAgregar.Enabled = false;
            try
            {
                var diario = DiarioOperaciones.Actual();
                var resultado = diario.Ejecutar(ClaveConsumo, solicitud, contexto =>
                {
                    detalleNegocio.Insertar(solicitud.IdServicio, solicitud.IdReserva, solicitud.Cantidad,
                        solicitud.Precio, solicitud.IdHabitacion, contexto.Id);
                    return true;
                });
                MostrarMensaje("Consumo registrado", "El consumo se registró correctamente.");
                diario.Confirmar(ClaveConsumo, resultado.Id);
                consumoPendiente = null;
                BloquearSolicitud(false);
                nudCantidad.Value = 1;
                CargarConsumos();
                ActualizarSubtotal();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number >= 51313 && ex.Number <= 51318)
            {
                consumoPendiente = null;
                BloquearSolicitud(false);
                CargarProductos();
                MostrarMensaje("Consumo rechazado", HotelTramonto.Datos.ErroresOperacionSql.Mensaje(ex.Number), true);
            }
            catch (Exception)
            {
                MostrarMensaje("Resultado no confirmado",
                    "Reabrí esta reserva para recuperar la misma operación pendiente. No ingreses otra solicitud del consumo.", true);
            }
            finally { btnAgregar.Enabled = true; }
        }

        private void CargarConsumos()
        {
            try
            {
                tablaConsumos =
                    detalleNegocio.ListarPorReserva(idReserva,
                        filtrarHabitacion.Checked && cmbHabitacion.SelectedValue is int habitacion
                            ? habitacion : null);

                dgvConsumos.DataSource = tablaConsumos;

                if (dgvConsumos.Columns.Contains("id_detalle_servicio"))
                    dgvConsumos.Columns["id_detalle_servicio"].Visible = false;

                if (dgvConsumos.Columns.Contains("id_habitacion"))
                    dgvConsumos.Columns["id_habitacion"]!.Visible = false;
                if (dgvConsumos.Columns.Contains("numero_habitacion"))
                    dgvConsumos.Columns["numero_habitacion"]!.HeaderText = "Habitación";
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
                    RegistroErrores.Mensaje(ex),
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
    }
}
