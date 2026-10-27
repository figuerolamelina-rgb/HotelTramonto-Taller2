
using System;
using System.Data;
using System.Windows.Forms;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class FormModificarHabitacion : Form
    {
        private readonly int idHabitacion;

        public FormModificarHabitacion(int idHabitacion)
        {
            InitializeComponent();

            this.idHabitacion = idHabitacion;

            ConfigurarCombos();
            CargarDatosHabitacion();
        }

        private void ConfigurarCombos()
        {
            try
            {
                TipoHabitacionNegocios negocio =
                    new TipoHabitacionNegocios();

                DataTable tipos = negocio.Listar();

                cmbTipoHabitacion.DataSource = tipos;
                cmbTipoHabitacion.DisplayMember = "nombre";
                cmbTipoHabitacion.ValueMember = "id_tipoHabitacion";
                cmbTipoHabitacion.DropDownStyle =
                    ComboBoxStyle.DropDownList;

                cmbEstado.Items.Clear();
                cmbEstado.Items.Add("DISPONIBLE");
                cmbEstado.Items.Add("OCUPADA");
                cmbEstado.Items.Add("MANTENIMIENTO");
                cmbEstado.DropDownStyle =
                    ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los tipos de habitación: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void CargarDatosHabitacion()
        {
            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();
                DataTable resultado = negocio.BuscarPorId(idHabitacion);

                DataRow habitacion = resultado.Rows[0];

                cmbTipoHabitacion.SelectedValue =
                    Convert.ToInt32(habitacion["tipo_habitacion"]);

                txtPiso.Text = habitacion["piso"] == DBNull.Value
                    ? ""
                    : habitacion["piso"].ToString();

                txtNumero.Text = habitacion["numero"].ToString();

                cmbEstado.SelectedItem = habitacion["estado"].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los datos de la habitación: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Close();
            }
        }

        private void btnGuardarCambios_Click(object sender, EventArgs e)
        {
            if (cmbTipoHabitacion.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un tipo de habitación.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                MessageBox.Show("Ingrese el número de habitación.");
                txtNumero.Focus();
                return;
            }

            if (cmbEstado.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un estado.");
                return;
            }

            int tipoHabitacion = Convert.ToInt32(
                cmbTipoHabitacion.SelectedValue);

            int? piso = null;

            if (!string.IsNullOrWhiteSpace(txtPiso.Text))
            {
                if (!int.TryParse(txtPiso.Text.Trim(), out int pisoIngresado)
                    || pisoIngresado < 0)
                {
                    MessageBox.Show(
                        "Ingrese un número de piso válido, igual o mayor que cero.");
                    txtPiso.Focus();
                    return;
                }

                piso = pisoIngresado;
            }

            string numero = txtNumero.Text.Trim();
            string estado = cmbEstado.SelectedItem.ToString()!;

            try
            {
                HabitacionNegocios negocio = new HabitacionNegocios();

                negocio.Modificar(
                    idHabitacion,
                    tipoHabitacion,
                    piso,
                    numero,
                    estado);

                MessageBox.Show(
                    "Habitación modificada correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo modificar la habitación: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
