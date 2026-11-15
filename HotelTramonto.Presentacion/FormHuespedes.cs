
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormHuespedes : Form
    {
        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        private readonly string rolHistorial;
        private readonly Button btnHistorial = new Button();

        public FormHuespedes() : this("") { }

        public FormHuespedes(string rol)
        {
            rolHistorial = rol?.Trim().ToUpperInvariant() ?? "";
            InitializeComponent();
            Controls.Add(btnHistorial);
            btnHistorial.Click += ConsultarHistorial;
            AplicarEstilo();
            CargarHuespedes();
        }

        private void AplicarEstilo()
        {
            Text = "Gestión de huéspedes - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);

            Label lblTitulo = new Label
            {
                Name = "lblTituloHuespedes",
                Text = "GESTIÓN DE HUÉSPEDES",
                Location = new Point(25, 18),
                AutoSize = true,
                ForeColor = dorado,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            };

            Label lblSubtitulo = new Label
            {
                Name = "lblSubtituloHuespedes",
                Text = "Administración de huéspedes registrados",
                Location = new Point(28, 60),
                AutoSize = true,
                ForeColor = crema,
                Font = new Font("Segoe UI", 10F)
            };

            Panel panelTitulo = new Panel
            {
                Name = "panelTituloHuespedes",
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = marron
            };

            panelTitulo.Controls.Add(lblTitulo);
            panelTitulo.Controls.Add(lblSubtitulo);
            Controls.Add(panelTitulo);
            panelTitulo.BringToFront();

            EstilizarBoton(btnNuevoHuesped, "＋ Nuevo huésped", dorado, marron);
            EstilizarBoton(btnModificarHuesped, "Modificar huésped", marronClaro, Color.White);
            EstilizarBoton(btnDesactivarHuesped, "Desactivar huésped", marronClaro, Color.White);
            EstilizarBoton(btnBuscarDNI, "Buscar por DNI", marronClaro, Color.White);
            EstilizarBoton(btnActualizar, "Actualizar listado", marronClaro, Color.White);
            EstilizarBoton(btnReactivarHuesped, "Reactivar huésped", marronClaro, Color.White);

            EstilizarBoton(btnHistorial, "Historial de reservas", dorado, marron);
            btnHistorial.Enabled = rolHistorial.Trim().ToUpperInvariant() is "ADMINISTRADOR" or "RECEPCIONISTA";

            dgvHuespedes.BackgroundColor = marfil;
            dgvHuespedes.BorderStyle = BorderStyle.None;
            dgvHuespedes.GridColor = Color.FromArgb(222, 211, 194);
            dgvHuespedes.EnableHeadersVisualStyles = false;
            dgvHuespedes.ColumnHeadersHeight = 42;
            dgvHuespedes.RowHeadersVisible = false;
            dgvHuespedes.Font = new Font("Segoe UI", 10F);
            dgvHuespedes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHuespedes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHuespedes.MultiSelect = false;
            dgvHuespedes.ReadOnly = true;
            dgvHuespedes.AllowUserToAddRows = false;
            dgvHuespedes.AllowUserToDeleteRows = false;

            dgvHuespedes.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;
            dgvHuespedes.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHuespedes.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(8, 0, 8, 0);

            dgvHuespedes.DefaultCellStyle.BackColor = marfil;
            dgvHuespedes.DefaultCellStyle.ForeColor = marron;
            dgvHuespedes.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(226, 211, 184);
            dgvHuespedes.DefaultCellStyle.SelectionForeColor = marron;
            dgvHuespedes.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            dgvHuespedes.AlternatingRowsDefaultCellStyle.BackColor = crema;

            Resize += (s, e) => AcomodarControles();
            AcomodarControles();
        }

        private void ConsultarHistorial(object? sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MostrarMensaje("Huésped no seleccionado", "Seleccioná un huésped para consultar su historial.");
                return;
            }
            try
            {
                int id = Convert.ToInt32(dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);
                DataTable historial = new HotelTramonto.Negocios.HuespedNegocios()
                    .HistorialReservas(id, rolHistorial);
                using Form ventana = new Form
                {
                    Text = "Historial de reservas del huésped #" + id,
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(1100, 600),
                    BackColor = crema,
                    Font = new Font("Segoe UI", 10F)
                };
                Label aviso = new Label
                {
                    Dock = DockStyle.Top, Height = 85, Padding = new Padding(12),
                    Text = historial.Rows.Count == 0 ? "El huésped no tiene reservas definitivas." :
                        "Una fila por reserva y habitación asociada. Las fechas de entrada/salida son previstas. " +
                        "Check-in y check-out corresponden a la reserva; no acreditan ocupación real por habitación. " +
                        "El número y tipo de habitación son los del catálogo actual.",
                    ForeColor = marron
                };
                DataGridView tabla = new DataGridView
                {
                    Dock = DockStyle.Fill, DataSource = historial,
                    ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                    RowHeadersVisible = false, BackgroundColor = marfil,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
                };
                ventana.Controls.Add(tabla);
                ventana.Controls.Add(aviso);
                ventana.ShowDialog(this);
            }
            catch (UnauthorizedAccessException)
            {
                MostrarMensaje("Acceso restringido", "No tiene permisos para consultar el historial.");
            }
            catch (Exception)
            {
                MostrarMensaje("Historial no disponible",
                    "No se pudo consultar el historial. Verificá la configuración y la instalación del procedimiento.", true);
            }
        }

        private void EstilizarBoton(
            Button boton, string texto, Color fondo, Color textoColor)
        {
            boton.Text = texto;
            boton.BackColor = fondo;
            boton.ForeColor = textoColor;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(211, 178, 119);
            boton.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(174, 139, 76);
            boton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }

        private void AcomodarControles()
        {
            int margen = 25;
            int anchoDisponible = ClientSize.Width - margen * 2;
            int anchoBoton = Math.Max(120, (anchoDisponible - 24) / 3);
            int altoBoton = 38;
            int separacion = 12;
            int inicioY = 120;

            btnNuevoHuesped.SetBounds(
                margen, inicioY, anchoBoton, altoBoton);

            btnModificarHuesped.SetBounds(
                margen + anchoBoton + separacion,
                inicioY, anchoBoton, altoBoton);

            btnDesactivarHuesped.SetBounds(
                margen + (anchoBoton + separacion) * 2,
                inicioY, anchoBoton, altoBoton);

            int segundaFilaY = inicioY + altoBoton + 10;

            btnBuscarDNI.SetBounds(
                margen, segundaFilaY, anchoBoton, altoBoton);

            btnActualizar.SetBounds(
                margen + anchoBoton + separacion,
                segundaFilaY, anchoBoton, altoBoton);

            btnReactivarHuesped.SetBounds(
                margen + (anchoBoton + separacion) * 2,
                segundaFilaY, anchoBoton, altoBoton);

            int terceraFilaY = segundaFilaY + altoBoton + 10;
            btnHistorial.SetBounds(margen, terceraFilaY, anchoBoton, altoBoton);
            int tablaY = terceraFilaY + altoBoton + 18;

            dgvHuespedes.SetBounds(
                margen,
                tablaY,
                anchoDisponible,
                Math.Max(150, ClientSize.Height - tablaY - 25));

            dgvHuespedes.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
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
                    Size = new Size(360, 45),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(145, 120),
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
                    Size = new Size(360, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9F)
                };

                Button btnSi = new Button
                {
                    Text = "Sí",
                    Location = new Point(85, 115),
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
                    Location = new Point(215, 115),
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

        private void CargarHuespedes()
        {
            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                dgvHuespedes.DataSource = negocio.Listar();

                dgvHuespedes.ReadOnly = true;
                dgvHuespedes.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
                dgvHuespedes.MultiSelect = false;
                dgvHuespedes.AllowUserToAddRows = false;
                dgvHuespedes.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                if (dgvHuespedes.Columns.Contains("id_huesped"))
                {
                    dgvHuespedes.Columns["id_huesped"].Visible = false;
                }

                if (dgvHuespedes.Columns.Contains("activo"))
                {
                    dgvHuespedes.Columns["activo"].HeaderText = "Estado";

                    string nombreColumnaEstado = "estadoVisual";

                    if (!dgvHuespedes.Columns.Contains(nombreColumnaEstado))
                    {
                        DataGridViewTextBoxColumn columnaEstado =
                            new DataGridViewTextBoxColumn
                            {
                                Name = nombreColumnaEstado,
                                HeaderText = "Estado",
                                ReadOnly = true,
                                SortMode = DataGridViewColumnSortMode.NotSortable
                            };

                        dgvHuespedes.Columns.Add(columnaEstado);
                    }

                    foreach (DataGridViewRow fila in dgvHuespedes.Rows)
                    {
                        object valor = fila.Cells["activo"].Value;

                        if (valor != null && valor != DBNull.Value)
                        {
                            bool activo = Convert.ToBoolean(valor);

                            fila.Cells[nombreColumnaEstado].Value =
                                activo ? "Activo" : "Inactivo";
                        }
                        else
                        {
                            fila.Cells[nombreColumnaEstado].Value = "Sin datos";
                        }
                    }

                    dgvHuespedes.Columns["activo"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar los huéspedes",
                    "No se pudieron cargar los huéspedes.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarHuespedes();
        }

        private void btnNuevoHuesped_Click(object sender, EventArgs e)
        {
            using (FormNuevoHuesped formulario = new FormNuevoHuesped())
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHuespedes();
                }
            }
        }

        private void btnModificarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MostrarMensaje(
                    "Huésped no seleccionado",
                    "Seleccioná un huésped para modificar.");

                return;
            }

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                using (FormModificarHuesped formulario =
                    new FormModificarHuesped(idHuesped))
                {
                    if (formulario.ShowDialog() == DialogResult.OK)
                    {
                        CargarHuespedes();
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al modificar el huésped",
                    "No se pudo modificar el huésped.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

        private void btnDesactivarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MostrarMensaje(
                    "Huésped no seleccionado",
                    "Seleccioná un huésped para desactivar.");

                return;
            }

            bool activo = Convert.ToBoolean(
                dgvHuespedes.CurrentRow.Cells["activo"].Value);

            if (!activo)
            {
                MostrarMensaje(
                    "Huésped ya desactivado",
                    "El huésped seleccionado ya está desactivado.");

                return;
            }

            bool confirmacion = MostrarConfirmacion(
                "Confirmar desactivación",
                "¿Querés desactivar al huésped seleccionado?");

            if (!confirmacion)
                return;

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                new HotelTramonto.Negocios.HuespedNegocios()
                    .Desactivar(idHuesped);

                MostrarMensaje(
                    "Huésped desactivado",
                    "El huésped se desactivó correctamente.");

                CargarHuespedes();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al desactivar",
                    "No se pudo desactivar el huésped.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

        private void btnReactivarHuesped_Click(object sender, EventArgs e)
        {
            if (dgvHuespedes.CurrentRow == null)
            {
                MostrarMensaje(
                    "Huésped no seleccionado",
                    "Seleccioná un huésped para reactivar.");

                return;
            }

            bool activo = Convert.ToBoolean(
                dgvHuespedes.CurrentRow.Cells["activo"].Value);

            if (activo)
            {
                MostrarMensaje(
                    "Huésped ya activo",
                    "El huésped seleccionado ya está activo.");

                return;
            }

            bool confirmacion = MostrarConfirmacion(
                "Confirmar reactivación",
                "¿Querés reactivar al huésped seleccionado?");

            if (!confirmacion)
                return;

            try
            {
                int idHuesped = Convert.ToInt32(
                    dgvHuespedes.CurrentRow.Cells["id_huesped"].Value);

                new HotelTramonto.Negocios.HuespedNegocios()
                    .Reactivar(idHuesped);

                MostrarMensaje(
                    "Huésped reactivado",
                    "El huésped se reactivó correctamente.");

                CargarHuespedes();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al reactivar",
                    "No se pudo reactivar el huésped.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

        private void btnBuscarDNI_Click(object sender, EventArgs e)
        {
            BuscarHuespedPorDNI();
        }

        private void btnBuscarDNI_Click_1(object sender, EventArgs e)
        {
            BuscarHuespedPorDNI();
        }


        private string MostrarEntradaDNI()
        {
            string dniIngresado = "";

            using (Form formulario = new Form())
            {
                formulario.Text = "Buscar huésped";
                formulario.StartPosition = FormStartPosition.CenterParent;
                formulario.FormBorderStyle = FormBorderStyle.FixedDialog;
                formulario.MaximizeBox = false;
                formulario.MinimizeBox = false;
                formulario.ShowInTaskbar = false;
                formulario.ClientSize = new Size(400, 205);
                formulario.BackColor = crema;
                formulario.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(400, 7),
                    BackColor = dorado
                };

                Label lblTitulo = new Label
                {
                    Text = "BUSCAR HUÉSPED",
                    Location = new Point(20, 20),
                    Size = new Size(360, 32),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold)
                };

                Label lblDNI = new Label
                {
                    Text = "Ingresá el DNI del huésped:",
                    Location = new Point(30, 65),
                    AutoSize = true,
                    ForeColor = marron,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };

                TextBox txtDNI = new TextBox
                {
                    Location = new Point(30, 92),
                    Size = new Size(340, 30),
                    Font = new Font("Segoe UI", 11F),
                    BackColor = marfil,
                    ForeColor = marron,
                    BorderStyle = BorderStyle.FixedSingle,
                    MaxLength = 8
                };

                Button btnBuscar = new Button
                {
                    Text = "Buscar",
                    Location = new Point(85, 145),
                    Size = new Size(105, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand
                };

                Button btnCancelarBusqueda = new Button
                {
                    Text = "Cancelar",
                    Location = new Point(210, 145),
                    Size = new Size(105, 35),
                    BackColor = marronClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel,
                    Cursor = Cursors.Hand
                };

                btnBuscar.FlatAppearance.BorderSize = 0;
                btnCancelarBusqueda.FlatAppearance.BorderSize = 0;

                formulario.Controls.Add(encabezado);
                formulario.Controls.Add(lblTitulo);
                formulario.Controls.Add(lblDNI);
                formulario.Controls.Add(txtDNI);
                formulario.Controls.Add(btnBuscar);
                formulario.Controls.Add(btnCancelarBusqueda);

                formulario.AcceptButton = btnBuscar;
                formulario.CancelButton = btnCancelarBusqueda;

                if (formulario.ShowDialog(this) == DialogResult.OK)
                    dniIngresado = txtDNI.Text.Trim();
            }

            return dniIngresado;
        }


        private void BuscarHuespedPorDNI()
        {
            string dni = MostrarEntradaDNI();

            if (string.IsNullOrWhiteSpace(dni))
                return;

            if (!System.Text.RegularExpressions.Regex.IsMatch(dni, @"^\d{1,8}$"))
            {
                MostrarMensaje(
                    "DNI inválido",
                    "Ingresá un DNI numérico de hasta 8 dígitos.");

                return;
            }

            try
            {
                HotelTramonto.Negocios.HuespedNegocios negocio =
                    new HotelTramonto.Negocios.HuespedNegocios();

                DataTable resultado = negocio.BuscarPorDNI(dni);

                dgvHuespedes.DataSource = resultado;

                if (dgvHuespedes.Columns.Contains("id_huesped"))
                    dgvHuespedes.Columns["id_huesped"].Visible = false;

                if (resultado.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Huésped no encontrado",
                        "No se encontró ningún huésped con ese DNI.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al buscar el huésped",
                    "No se pudo realizar la búsqueda.\n\nDetalle: " + ex.Message,
                    true);
            }
        }

    }
}
