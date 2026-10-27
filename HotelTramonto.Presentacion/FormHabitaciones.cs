
using System;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormHabitaciones : Form
    {
        public FormHabitaciones()
        {
            InitializeComponent();
            CargarHabitaciones();
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
