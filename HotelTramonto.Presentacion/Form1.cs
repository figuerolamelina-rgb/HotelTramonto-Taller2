using Microsoft.Data.SqlClient;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CargarUsuarios();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using SqlConnection conexion = ConexionBD.ObtenerConexion();
                conexion.Open();

                MessageBox.Show("Conexión exitosa con SQL Server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                RolDatos datos = new RolDatos();
                var roles = datos.Listar();

                if (roles.Rows.Count == 0)
                {
                    MessageBox.Show("No hay roles registrados.");
                    return;
                }

                string mensaje = "";

                foreach (System.Data.DataRow fila in roles.Rows)
                {
                    mensaje += $"{fila["id_rol"]} - {fila["nombre_rol"]}\n";
                }

                MessageBox.Show(mensaje, "Roles registrados");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al listar los roles: " + ex.Message);
            }
        }
        private void CargarUsuarios()
        {
            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();

                dgvUsuarios.DataSource = negocio.Listar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los usuarios: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnNuevoUsuario_Click(object sender, EventArgs e)
        {
            using FormUsuario formulario = new FormUsuario();

            if (formulario.ShowDialog(this) == DialogResult.OK)
            {
                CargarUsuarios();
            }
        }

        private void btnModificarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null ||
                dgvUsuarios.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione un usuario de la tabla.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            using FormModificarUsuario formulario =
                new FormModificarUsuario(idUsuario);

            if (formulario.ShowDialog(this) == DialogResult.OK)
            {
                CargarUsuarios();
            }
        }

        private void btnDesactivarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null ||
                dgvUsuarios.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione un usuario de la tabla.");
                return;
            }

            int idUsuario = Convert.ToInt32(
                dgvUsuarios.CurrentRow.Cells["id_usuario"].Value);

            string nombre = dgvUsuarios.CurrentRow.Cells["nombre"].Value?.ToString() ?? "";

            DialogResult respuesta = MessageBox.Show(
                $"¿Está seguro de desactivar al usuario {nombre}?",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                negocio.Desactivar(idUsuario);

                MessageBox.Show("Usuario desactivado correctamente.");

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo desactivar el usuario: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}