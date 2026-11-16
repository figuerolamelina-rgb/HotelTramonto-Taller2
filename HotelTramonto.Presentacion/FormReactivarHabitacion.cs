
using HotelTramonto.Negocios;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HotelTramonto.Presentacion
{
    public partial class FormReactivarHabitacion : Form
    {
        private readonly Color marron = Color.FromArgb(74, 48, 39);
        private readonly Color marronClaro = Color.FromArgb(91, 62, 49);
        private readonly Color crema = Color.FromArgb(245, 239, 229);
        private readonly Color marfil = Color.FromArgb(255, 252, 247);
        private readonly Color dorado = Color.FromArgb(198, 161, 91);

        // Rol del usuario que inició sesión.
        private readonly string rolUsuario;

        private bool EsAdministrador =>
            PermisosHabitaciones.PuedeAdministrar(rolUsuario);

        // Constructor para el diseñador de Windows Forms.
        public FormReactivarHabitacion()
            : this(string.Empty)
        {
        }

        // Constructor que recibe el rol del usuario.
        public FormReactivarHabitacion(string rolUsuario)
        {
            this.rolUsuario =
                rolUsuario?.Trim().ToUpperInvariant()
                ?? string.Empty;

            InitializeComponent();
            AplicarEstilo();

            // Solo el administrador puede reactivar.
            btnReactivar.Enabled = EsAdministrador;

            CargarHabitacionesInactivas();
        }

        private void AplicarEstilo()
        {
            Text = "Reactivar habitación - Hotel Tramonto";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = crema;
            ForeColor = marron;
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(800, 500);

            dgvHabitacionesInactivas.BackgroundColor = marfil;
            dgvHabitacionesInactivas.BorderStyle = BorderStyle.None;
            dgvHabitacionesInactivas.GridColor =
                Color.FromArgb(229, 218, 202);

            dgvHabitacionesInactivas.EnableHeadersVisualStyles = false;

            dgvHabitacionesInactivas.ColumnHeadersDefaultCellStyle.BackColor =
                marron;

            dgvHabitacionesInactivas.ColumnHeadersDefaultCellStyle.ForeColor =
                dorado;

            dgvHabitacionesInactivas.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvHabitacionesInactivas.DefaultCellStyle.BackColor = marfil;
            dgvHabitacionesInactivas.DefaultCellStyle.ForeColor = marron;

            dgvHabitacionesInactivas.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvHabitacionesInactivas.DefaultCellStyle.SelectionForeColor =
                marron;

            dgvHabitacionesInactivas.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            EstilizarBoton(
                btnReactivar,
                "Reactivar habitación",
                dorado,
                marron);

            EstilizarBoton(
                btnCancelar,
                "Cancelar",
                marronClaro,
                Color.White);
        }

        private void EstilizarBoton(
            Button boton,
            string texto,
            Color fondo,
            Color textoColor)
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

            boton.Font = new Font(
                "Segoe UI",
                9.5F,
                FontStyle.Bold);

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
                mensaje.StartPosition =
                    FormStartPosition.CenterParent;

                mensaje.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(420, 185);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;
                mensaje.Font = new Font("Segoe UI", 10F);

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
                    TextAlign =
                        ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font(
                        "Segoe UI",
                        12F,
                        FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(380, 65),
                    TextAlign =
                        ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnAceptar = new Button
                {
                    Text = "Aceptar",
                    Location = new Point(155, 135),
                    Size = new Size(110, 35),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };

                btnAceptar.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTitulo);
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

                mensaje.StartPosition =
                    FormStartPosition.CenterParent;

                mensaje.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                mensaje.MaximizeBox = false;
                mensaje.MinimizeBox = false;
                mensaje.ShowInTaskbar = false;
                mensaje.ClientSize = new Size(420, 205);
                mensaje.BackColor = crema;
                mensaje.ForeColor = marron;
                mensaje.Font = new Font("Segoe UI", 10F);

                Panel encabezado = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(420, 7),
                    BackColor = dorado
                };

                Label lblTitulo = new Label
                {
                    Text = titulo,
                    Location = new Point(15, 18),
                    Size = new Size(390, 35),
                    TextAlign =
                        ContentAlignment.MiddleCenter,
                    ForeColor = marron,
                    Font = new Font(
                        "Segoe UI",
                        12F,
                        FontStyle.Bold)
                };

                Label lblDetalle = new Label
                {
                    Text = detalle,
                    Location = new Point(20, 55),
                    Size = new Size(380, 55),
                    TextAlign =
                        ContentAlignment.MiddleCenter,
                    ForeColor = marronClaro,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnSi = new Button
                {
                    Text = "Sí, reactivar",
                    Location = new Point(65, 140),
                    Size = new Size(130, 38),
                    BackColor = dorado,
                    ForeColor = marron,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    DialogResult = DialogResult.Yes,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };

                Button btnNo = new Button
                {
                    Text = "Cancelar",
                    Location = new Point(225, 140),
                    Size = new Size(130, 38),
                    BackColor = marronClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    DialogResult = DialogResult.No,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };

                btnSi.FlatAppearance.BorderSize = 0;
                btnNo.FlatAppearance.BorderSize = 0;

                mensaje.Controls.Add(encabezado);
                mensaje.Controls.Add(lblTitulo);
                mensaje.Controls.Add(lblDetalle);
                mensaje.Controls.Add(btnSi);
                mensaje.Controls.Add(btnNo);

                mensaje.AcceptButton = btnSi;
                mensaje.CancelButton = btnNo;

                return mensaje.ShowDialog(this)
                    == DialogResult.Yes;
            }
        }

        private void CargarHabitacionesInactivas()
        {
            try
            {
                HabitacionNegocios negocio =
                    new HabitacionNegocios();

                DataTable tabla =
                    negocio.ListarInactivas();

                dgvHabitacionesInactivas.DataSource = tabla;

                dgvHabitacionesInactivas.ReadOnly = true;

                dgvHabitacionesInactivas.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;

                dgvHabitacionesInactivas.MultiSelect = false;

                dgvHabitacionesInactivas.AllowUserToAddRows =
                    false;

                dgvHabitacionesInactivas.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.None;

                dgvHabitacionesInactivas.AutoSizeRowsMode =
                    DataGridViewAutoSizeRowsMode.AllCells;

                dgvHabitacionesInactivas.ScrollBars =
                    ScrollBars.Both;

                if (tabla.Rows.Count == 0)
                {
                    MostrarMensaje(
                        "Habitaciones inactivas",
                        "No se encontraron habitaciones inactivas.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al cargar habitaciones",
                    "No se pudieron cargar las habitaciones inactivas.\n\n"
                    + ex.Message,
                    true);
            }
        }

        private void btnReactivar_Click(
            object sender,
            EventArgs e)
        {
            // Comprobar permiso antes de reactivar.
            if (!EsAdministrador)
            {
                MostrarMensaje(
                    "Acceso denegado",
                    "Solo el administrador puede reactivar habitaciones.",
                    true);

                return;
            }

            if (dgvHabitacionesInactivas.CurrentRow == null ||
                dgvHabitacionesInactivas.CurrentRow.IsNewRow)
            {
                MostrarMensaje(
                    "Selección requerida",
                    "Seleccioná una habitación de la tabla.");

                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitacionesInactivas.CurrentRow
                    .Cells["id_habitacion"].Value);

            bool confirmacion = MostrarConfirmacion(
                "Confirmar reactivación",
                "¿Deseás reactivar la habitación seleccionada?");

            if (!confirmacion)
                return;

            try
            {
                HabitacionNegocios negocio =
                    new HabitacionNegocios();

                // Enviar el rol a la capa de Negocios.
                negocio.Reactivar(
                    idHabitacion,
                    rolUsuario);

                MostrarMensaje(
                    "¡Habitación reactivada!",
                    "La habitación se reactivó correctamente.");

                CargarHabitacionesInactivas();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "Error al reactivar",
                    "No se pudo reactivar la habitación.\n\n"
                    + ex.Message,
                    true);
            }
        }

        private void btnCancelar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }
    }
}
