
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormServicios : Form
    {
        private readonly string rolUsuario;
        private readonly ServicioNegocios negocio = new ServicioNegocios();
        private readonly HabitacionNegocios habitacionNegocio =
    new HabitacionNegocios();
        private DataTable tablaServicios;
        private int idServicioSeleccionado = 0;
        private bool mostrandoInactivos = false;

        private readonly string[] categorias =
        {
            "Almuerzo",
            "Merienda",
            "Cena",
            "Tragos",
            "Pesca guiada",
            "Incluido",
            "Otros"
        };

        public FormServicios(string rol)
        {
            InitializeComponent();

            rolUsuario = rol.Trim().ToUpperInvariant();

            if (rolUsuario != "ADMINISTRADOR")
            {
                MessageBox.Show(
                    "No tenés permisos para gestionar los servicios.",
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Load += (s, e) => Close();
                return;
            }

            AplicarEstilo();
            CargarCategorias();
            CargarServicios();
            LimpiarCampos();
        }

        private void AplicarEstilo()
        {
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color marfil = Color.FromArgb(255, 252, 247);
            Color dorado = Color.FromArgb(198, 161, 91);
            Color texto = Color.FromArgb(66, 51, 43);

            Text = "Hotel Tramonto | Gestión de servicios";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);

            Label lblTitulo = new Label
            {
                Name = "lblTituloServicios",
                Text = "GESTIÓN DE SERVICIOS",
                Font = new Font("Georgia", 25F, FontStyle.Bold),
                ForeColor = dorado,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(35, 25)
            };

            Controls.Add(lblTitulo);
            lblTitulo.BringToFront();

            EstilizarBoton(btnNuevo, dorado, marron);
            EstilizarBoton(btnGuardar, marron, marfil);
            EstilizarBoton(btnModificar, marronClaro, marfil);
            EstilizarBoton(btnDesactivar, marronClaro, marfil);
            EstilizarBoton(btnLimpiar, marronClaro, marfil);
            EstilizarBoton(btnVerInactivos, marronClaro, marfil);

            Button btnRegistrarConsumos = new Button
            {
                Name = "btnRegistrarConsumos",
                Text = "Registrar consumos",
                Location = new Point(835, 215),
                Size = new Size(175, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            EstilizarBoton(btnRegistrarConsumos, dorado, marron);

            btnRegistrarConsumos.Click += btnRegistrarConsumos_Click;

            Controls.Add(btnRegistrarConsumos);
            btnVerInactivos.Location = new Point(835, 165);
            btnVerInactivos.Size = new Size(145, 40);
            btnNuevo.Location = new Point(35, 165);
            btnGuardar.Location = new Point(195, 165);
            btnModificar.Location = new Point(355, 165);
            btnDesactivar.Location = new Point(515, 165);
            btnLimpiar.Location = new Point(675, 165);

            btnNuevo.Size = new Size(145, 40);
            btnGuardar.Size = new Size(145, 40);
            btnModificar.Size = new Size(145, 40);
            btnDesactivar.Size = new Size(145, 40);
            btnLimpiar.Size = new Size(145, 40);

            lblNombre.ForeColor = marron;
            lblDescripcion.ForeColor = marron;
            lblCategoria.ForeColor = marron;
            lblPrecio.ForeColor = marron;
            lblFiltro.ForeColor = marron;

            TextBox[] cajas = { txtNombre, txtDescripcion };

            foreach (TextBox caja in cajas)
            {
                caja.Font = new Font("Segoe UI", 10F);
                caja.BackColor = marfil;
                caja.ForeColor = texto;
                caja.BorderStyle = BorderStyle.FixedSingle;
            }

            ComboBox[] combos = { cmbCategoria, cmbFiltroCategoria };

            foreach (ComboBox combo in combos)
            {
                combo.Font = new Font("Segoe UI", 10F);
                combo.BackColor = marfil;
                combo.ForeColor = texto;
                combo.FlatStyle = FlatStyle.Flat;
            }

            nudPrecio.Font = new Font("Segoe UI", 10F);
            nudPrecio.BackColor = marfil;
            nudPrecio.ForeColor = texto;

            dgvServicios.Location = new Point(35, 280);
            dgvServicios.Size = new Size(
                ClientSize.Width - 70,
                ClientSize.Height - 320);

            dgvServicios.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvServicios.ReadOnly = true;
            dgvServicios.AllowUserToAddRows = false;
            dgvServicios.AllowUserToDeleteRows = false;
            dgvServicios.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvServicios.MultiSelect = false;
            dgvServicios.RowHeadersVisible = false;
            dgvServicios.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvServicios.RowTemplate.Height = 36;
            dgvServicios.ColumnHeadersHeight = 42;

            dgvServicios.BackgroundColor = marfil;
            dgvServicios.BorderStyle = BorderStyle.None;
            dgvServicios.GridColor = Color.FromArgb(229, 218, 202);
            dgvServicios.EnableHeadersVisualStyles = false;

            dgvServicios.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvServicios.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvServicios.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvServicios.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvServicios.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvServicios.DefaultCellStyle.BackColor = marfil;
            dgvServicios.DefaultCellStyle.ForeColor = texto;
            dgvServicios.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgvServicios.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvServicios.DefaultCellStyle.SelectionForeColor = marron;

            dgvServicios.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);
        }


        private void EstilizarBoton(
            Button boton,
            Color fondo,
            Color colorTexto)
        {
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.BackColor = fondo;
            boton.ForeColor = colorTexto;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(126, 91, 65);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(105, 73, 52);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }

        private void CargarCategorias()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.AddRange(categorias);

            cmbFiltroCategoria.Items.Clear();
            cmbFiltroCategoria.Items.Add("Todas");
            cmbFiltroCategoria.Items.AddRange(categorias);

            cmbFiltroCategoria.SelectedIndex = 0;
        }


        private void CargarServicios()
        {
            try
            {
                if (mostrandoInactivos)
                    tablaServicios = negocio.ListarInactivos(rolUsuario);
                else
                    tablaServicios = negocio.Listar(rolUsuario);

                dgvServicios.DataSource = null;
                dgvServicios.DataSource = tablaServicios;

                if (dgvServicios.Columns.Contains("id_servicio"))
                    dgvServicios.Columns["id_servicio"].Visible = false;

                if (dgvServicios.Columns.Contains("nombre"))
                    dgvServicios.Columns["nombre"].HeaderText = "Servicio";

                if (dgvServicios.Columns.Contains("descripcion"))
                    dgvServicios.Columns["descripcion"].HeaderText = "Descripción";

                if (dgvServicios.Columns.Contains("categoria"))
                    dgvServicios.Columns["categoria"].HeaderText = "Categoría";

                if (dgvServicios.Columns.Contains("precio"))
                {
                    dgvServicios.Columns["precio"].HeaderText = "Precio";
                    dgvServicios.Columns["precio"].DefaultCellStyle.Format = "C2";
                    dgvServicios.Columns["precio"].DefaultCellStyle.FormatProvider =
                        CultureInfo.GetCultureInfo("es-AR");
                }

                if (dgvServicios.Columns.Contains("activo"))
                    dgvServicios.Columns["activo"].Visible = false;

                btnVerInactivos.Text = mostrandoInactivos
                    ? "Ver activos"
                    : "Ver inactivos";

                AplicarFiltro();
                LimpiarCampos();
                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar los servicios",
                    "No se pudieron cargar los servicios.\n\nDetalle: " +
                    ex.Message,
                    true);
            }
        }


        private void AplicarFiltro()
        {
            if (tablaServicios == null)
                return;

            string categoria = cmbFiltroCategoria.SelectedItem?.ToString() ?? "Todas";

            if (categoria == "Todas")
                tablaServicios.DefaultView.RowFilter = "";
            else
                tablaServicios.DefaultView.RowFilter =
                    "categoria = '" + categoria.Replace("'", "''") + "'";
        }


        private void LimpiarCampos()
        {
            idServicioSeleccionado = 0;

            txtNombre.Clear();
            txtDescripcion.Clear();
            nudPrecio.Value = 0;

            if (cmbCategoria.Items.Count > 0)
                cmbCategoria.SelectedIndex = 0;

            dgvServicios.ClearSelection();

            btnGuardar.Enabled = !mostrandoInactivos;
            btnNuevo.Enabled = !mostrandoInactivos;
            btnModificar.Enabled = false;
            btnDesactivar.Enabled = false;

            btnDesactivar.Text = mostrandoInactivos
                ? "Reactivar"
                : "Desactivar";
        }



        private void ActualizarBotones()
        {
            bool seleccionado =
                dgvServicios.CurrentRow != null &&
                !dgvServicios.CurrentRow.IsNewRow;

            btnGuardar.Enabled = !mostrandoInactivos;
            btnNuevo.Enabled = !mostrandoInactivos;
            btnLimpiar.Enabled = true;

            btnModificar.Enabled = seleccionado && !mostrandoInactivos;
            btnDesactivar.Enabled = seleccionado;

            btnDesactivar.Text = mostrandoInactivos
                ? "Reactivar"
                : "Desactivar";
        }


        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarMensaje(
                    "Dato obligatorio",
                    "Ingresá el nombre del servicio.");
                txtNombre.Focus();
                return false;
            }

            if (cmbCategoria.SelectedIndex < 0)
            {
                MostrarMensaje(
                    "Categoría obligatoria",
                    "Seleccioná una categoría.");
                cmbCategoria.Focus();
                return false;
            }

            if (nudPrecio.Value <= 0)
            {
                MostrarMensaje(
                    "Precio inválido",
                    "El precio debe ser mayor que cero.");
                nudPrecio.Focus();
                return false;
            }

            return true;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            txtNombre.Focus();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                negocio.Insertar(
                    txtNombre.Text.Trim(),
                    txtDescripcion.Text.Trim(),
                    nudPrecio.Value,
                    cmbCategoria.SelectedItem.ToString(),
                    rolUsuario);

                MostrarMensaje(
                    "Servicio registrado",
                    "El servicio se registró correctamente.");

                CargarServicios();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al registrar",
                    "No se pudo registrar el servicio.\n\nDetalle: " +
                    ex.Message,
                    true);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (idServicioSeleccionado <= 0)
            {
                MostrarMensaje(
                    "Servicio no seleccionado",
                    "Seleccioná un servicio de la tabla.");
                return;
            }

            if (!ValidarCampos())
                return;

            try
            {
                negocio.Modificar(
                    idServicioSeleccionado,
                    txtNombre.Text.Trim(),
                    txtDescripcion.Text.Trim(),
                    nudPrecio.Value,
                    cmbCategoria.SelectedItem.ToString(),
                    rolUsuario);

                MostrarMensaje(
                    "Servicio modificado",
                    "Los datos del servicio se actualizaron correctamente.");

                CargarServicios();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al modificar",
                    "No se pudo modificar el servicio.\n\nDetalle: " +
                    ex.Message,
                    true);
            }
        }


        private void btnDesactivar_Click(object sender, EventArgs e)
        {
            if (idServicioSeleccionado <= 0)
            {
                MostrarMensaje(
                    "Servicio no seleccionado",
                    "Seleccioná un servicio de la tabla.");
                return;
            }

            if (mostrandoInactivos)
            {
                bool confirmarReactivacion = MostrarConfirmacion(
                    "Confirmar reactivación",
                    "¿Querés reactivar este servicio?");

                if (!confirmarReactivacion)
                    return;

                try
                {
                    negocio.Reactivar(idServicioSeleccionado, rolUsuario);

                    MostrarMensaje(
                        "Servicio reactivado",
                        "El servicio se reactivó correctamente.");

                    CargarServicios();
                }
                catch (Exception ex)
                {
                    MostrarMensaje(
                        "Error al reactivar",
                        "No se pudo reactivar el servicio.\n\nDetalle: " +
                        ex.Message,
                        true);
                }

                return;
            }

            bool confirmarDesactivacion = MostrarConfirmacion(
                "Confirmar desactivación",
                "¿Estás seguro de que querés desactivar este servicio?");

            if (!confirmarDesactivacion)
                return;

            try
            {
                negocio.Desactivar(idServicioSeleccionado, rolUsuario);

                MostrarMensaje(
                    "Servicio desactivado",
                    "El servicio se desactivó correctamente.");

                CargarServicios();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al desactivar",
                    "No se pudo desactivar el servicio.\n\nDetalle: " +
                    ex.Message,
                    true);
            }
        }


        private void dgvServicios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvServicios.CurrentRow == null ||
                dgvServicios.CurrentRow.IsNewRow)
            {
                ActualizarBotones();
                return;
            }

            try
            {
                DataGridViewRow fila = dgvServicios.CurrentRow;

                idServicioSeleccionado =
                    Convert.ToInt32(fila.Cells["id_servicio"].Value);

                txtNombre.Text =
                    fila.Cells["nombre"].Value?.ToString() ?? "";

                txtDescripcion.Text =
                    fila.Cells["descripcion"].Value == DBNull.Value
                        ? ""
                        : fila.Cells["descripcion"].Value?.ToString() ?? "";

                cmbCategoria.SelectedItem =
                    fila.Cells["categoria"].Value?.ToString();

                decimal precio = Convert.ToDecimal(
                    fila.Cells["precio"].Value);

                nudPrecio.Value = Math.Min(
                    nudPrecio.Maximum,
                    Math.Max(nudPrecio.Minimum, precio));

                ActualizarBotones();
            }
            catch
            {
                idServicioSeleccionado = 0;
                ActualizarBotones();
            }
        }

        private void cmbFiltroCategoria_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            AplicarFiltro();
            LimpiarCampos();
        }

        private void MostrarMensaje(
            string titulo,
            string detalle,
            bool esError = false)
        {
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color dorado = Color.FromArgb(198, 161, 91);

            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(400, 175);
                mensaje.BackColor = crema;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(400, 7),
                    BackColor = esError
                        ? Color.FromArgb(160, 65, 55)
                        : dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(370, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(360, 55),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(145, 125),
                    Size = new Size(110, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };

                btnAceptar.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTituloMensaje);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnAceptar);

                mensaje.AcceptButton = btnAceptar;
                mensaje.ShowDialog(this);
            }
        }

        private bool MostrarConfirmacion(string titulo, string detalle)
        {
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color dorado = Color.FromArgb(198, 161, 91);

            using (Form mensaje = new Form())
            {
                mensaje.Text = "Hotel Tramonto";
                mensaje.StartPosition = FormStartPosition.CenterParent;
                mensaje.FormBorderStyle = FormBorderStyle.FixedDialog;
                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(400, 180);
                mensaje.BackColor = crema;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(400, 7),
                    BackColor = dorado
                };

                Label lblTituloMensaje = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 20),
                    Size = new Size(370, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(360, 45),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnSi = new Button
                {
                    Text = "Sí",
                    Location = new Point(85, 120),
                    Size = new Size(100, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand
                };

                Button btnNo = new Button
                {
                    Text = "No",
                    Location = new Point(215, 120),
                    Size = new Size(100, 35),
                    BackColor = marronClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand
                };

                btnSi.FlatAppearance.BorderSize = 0;
                btnNo.FlatAppearance.BorderSize = 0;

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

        private void btnVerInactivos_Click(object sender, EventArgs e)
        {
            mostrandoInactivos = !mostrandoInactivos;

            LimpiarCampos();
            CargarServicios();
        }

        private void btnRegistrarConsumos_Click(object sender, EventArgs e)
        {
            using (FormRegistrarConsumos formulario =
                new FormRegistrarConsumos())
            {
                formulario.ShowDialog(this);
            }
        }
    }
}
