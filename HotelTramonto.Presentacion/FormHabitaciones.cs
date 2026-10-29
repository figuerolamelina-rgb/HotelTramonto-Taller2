
using System;
using System.Windows.Forms;
using HotelTramonto.Negocios;
using System.Drawing;

namespace HotelTramonto.Presentacion
{
    public partial class FormHabitaciones : Form
    {
        public FormHabitaciones()
        {
            InitializeComponent();
            AplicarEstilo();
            CargarHabitaciones();
        }


        private void AplicarEstilo()
        {
            // Paleta Hotel Tramonto
            Color marron = Color.FromArgb(74, 48, 39);
            Color marronClaro = Color.FromArgb(91, 62, 49);
            Color crema = Color.FromArgb(245, 239, 229);
            Color marfil = Color.FromArgb(255, 252, 247);
            Color dorado = Color.FromArgb(198, 161, 91);
            Color texto = Color.FromArgb(66, 51, 43);

            // Ventana
            Text = "Hotel Tramonto | Habitaciones";
            BackColor = crema;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);

            // Título
            Label lblTitulo = new Label
            {
                Name = "lblTituloHabitaciones",
                Text = "GESTIÓN DE HABITACIONES",
                Font = new Font("Georgia", 25F, FontStyle.Bold),
                ForeColor = dorado,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(35, 30)
            };

            // Botones
            EstilizarBotonHabitacion(
                btnNuevaHabitacion, "＋ Nueva habitación", dorado, marron);

            EstilizarBotonHabitacion(
                btnModificarHabitacion, "Modificar habitación", marron, marfil);

            EstilizarBotonHabitacion(
                btnDesactivarHabitacion, "Desactivar habitación", marronClaro, marfil);

            EstilizarBotonHabitacion(
                btnReactivarHabitacion, "Reactivar habitación", marronClaro, marfil);

            btnNuevaHabitacion.Location = new Point(35, 100);
            btnNuevaHabitacion.Size = new Size(180, 42);

            btnModificarHabitacion.Location = new Point(230, 100);
            btnModificarHabitacion.Size = new Size(180, 42);

            btnDesactivarHabitacion.Location = new Point(425, 100);
            btnDesactivarHabitacion.Size = new Size(190, 42);

            btnReactivarHabitacion.Location = new Point(630, 100);
            btnReactivarHabitacion.Size = new Size(180, 42);

            // Tabla de habitaciones
            dgvHabitaciones.Location = new Point(35, 170);
            dgvHabitaciones.Size = new Size(
                ClientSize.Width - 70,
                ClientSize.Height - 205);

            dgvHabitaciones.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            dgvHabitaciones.ReadOnly = true;
            dgvHabitaciones.AllowUserToAddRows = false;
            dgvHabitaciones.AllowUserToDeleteRows = false;
            dgvHabitaciones.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvHabitaciones.MultiSelect = false;
            dgvHabitaciones.RowHeadersVisible = false;
            dgvHabitaciones.ScrollBars = ScrollBars.Both;
            dgvHabitaciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHabitaciones.RowTemplate.Height = 36;
            dgvHabitaciones.ColumnHeadersHeight = 42;

            dgvHabitaciones.BackgroundColor = marfil;
            dgvHabitaciones.BorderStyle = BorderStyle.None;
            dgvHabitaciones.GridColor = Color.FromArgb(229, 218, 202);
            dgvHabitaciones.EnableHeadersVisualStyles = false;

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.BackColor = marron;
            dgvHabitaciones.ColumnHeadersDefaultCellStyle.ForeColor = dorado;
            dgvHabitaciones.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvHabitaciones.ColumnHeadersDefaultCellStyle.SelectionBackColor = marron;
            dgvHabitaciones.ColumnHeadersDefaultCellStyle.SelectionForeColor = dorado;

            dgvHabitaciones.DefaultCellStyle.BackColor = marfil;
            dgvHabitaciones.DefaultCellStyle.ForeColor = texto;
            dgvHabitaciones.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgvHabitaciones.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(238, 222, 190);

            dgvHabitaciones.DefaultCellStyle.SelectionForeColor = marron;

            dgvHabitaciones.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 243, 235);

            // Mantener el título visible
            Controls.Add(lblTitulo);
            lblTitulo.BringToFront();
        }

        private void EstilizarBotonHabitacion(
            Button boton,
            string textoBoton,
            Color fondo,
            Color colorTexto)
        {
            boton.Text = textoBoton;
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


        private void CargarHabitaciones()
        {
            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                dgvHabitaciones.DataSource = negocio.Listar();
                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar las habitaciones: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnNuevaHabitacion_Click(object sender, EventArgs e)
        {
            using (FormNuevaHabitacion formulario = new FormNuevaHabitacion())
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHabitaciones();
                }
            }
        }


        private void btnModificarHabitacion_Click(object sender, EventArgs e)
        {
            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione una habitación de la tabla.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow.Cells["id_habitacion"].Value);

            using (FormModificarHabitacion formulario =
                new FormModificarHabitacion(idHabitacion))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    CargarHabitaciones();
                }
            }
        }

        private void btnDesactivarHabitacion_Click(object sender, EventArgs e)
        {
            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione una habitación de la tabla.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow.Cells["id_habitacion"].Value);

            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de que desea desactivar esta habitación?",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                negocio.Desactivar(idHabitacion);

                MessageBox.Show(
                    "Habitación desactivada correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHabitaciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo desactivar la habitación: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnReactivarHabitacion_Click(object sender, EventArgs e)
        {
            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione una habitación de la tabla.");
                return;
            }

            int idHabitacion = Convert.ToInt32(
                dgvHabitaciones.CurrentRow.Cells["id_habitacion"].Value);

            bool activa = Convert.ToBoolean(
                dgvHabitaciones.CurrentRow.Cells["activo"].Value);

            if (activa)
            {
                MessageBox.Show("La habitación seleccionada ya está activa.");
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Desea reactivar la habitación seleccionada?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                negocio.Reactivar(idHabitacion);

                MessageBox.Show(
                    "Habitación reactivada correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarHabitaciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar la habitación: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void ActualizarBotones()
        {
            if (dgvHabitaciones.CurrentRow == null ||
                dgvHabitaciones.CurrentRow.IsNewRow)
            {
                btnModificarHabitacion.Enabled = false;
                btnDesactivarHabitacion.Enabled = false;
                btnReactivarHabitacion.Enabled = false;
                return;
            }

            bool activa = Convert.ToBoolean(
                dgvHabitaciones.CurrentRow.Cells["activo"].Value);

            btnModificarHabitacion.Enabled = activa;
            btnDesactivarHabitacion.Enabled = activa;
            btnReactivarHabitacion.Enabled = !activa;
        }

        private void dgvHabitaciones_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotones();
        }
    }
}
