using Microsoft.Data.SqlClient;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;

namespace HotelTramonto.Presentacion
{
    public partial class Form1 : Form
    {
        private readonly string rolUsuario;
        private readonly int idUsuario;

        public Form1(string rol, int idUsuario)
        {
            InitializeComponent();

            rolUsuario = rol.Trim().ToUpperInvariant();
            this.idUsuario = idUsuario;

            bool esAdministrador = rolUsuario == "ADMINISTRADOR";

            btnNuevoUsuario.Visible = esAdministrador;
            btnModificarUsuario.Visible = esAdministrador;
            btnDesactivarUsuario.Visible = esAdministrador;

            dgvUsuarios.Visible = esAdministrador;

            btnProbarConexion.Visible = esAdministrador;
            btnListarRoles.Visible = esAdministrador;
            btnReactivarUsuario.Visible = esAdministrador;

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
            if (rolUsuario != "ADMINISTRADOR")
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();

                dgvUsuarios.DataSource = negocio.Listar(rolUsuario);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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
            using FormUsuario formulario = new FormUsuario(rolUsuario);

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

            using FormModificarUsuario formulario = new FormModificarUsuario(idUsuario, rolUsuario);

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
                negocio.Desactivar(idUsuario, rolUsuario);

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


        private void btnReactivarUsuario_Click(object sender, EventArgs e)
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
                $"¿Está seguro de reactivar al usuario {nombre}?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                UsuarioNegocios negocio = new UsuarioNegocios();
                negocio.Reactivar(idUsuario, rolUsuario);

                MessageBox.Show(
                    "Usuario reactivado correctamente.",
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar el usuario: " + ex.Message,
                    "Hotel Tramonto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            FormLogin login = new FormLogin();
            login.Show();
            this.Close();
        }

        private void btnHabitaciones_Click(object sender, EventArgs e)
        {
            FormHabitaciones formulario = new FormHabitaciones();
            formulario.ShowDialog();
        }

        private void btnReservas_Click(object sender, EventArgs e)
        {
            using (FormReservas formulario = new FormReservas(idUsuario))
            {
                formulario.ShowDialog();
            }
        }

        private void btnHuespedes_Click(object sender, EventArgs e)
        {
            using (FormHuespedes formulario = new FormHuespedes())
            {
                formulario.ShowDialog();
            }
        }

        private void btnPagos_Click(object sender, EventArgs e)
        {
            using (FormPagos formulario = new FormPagos(idUsuario))
            {
                formulario.ShowDialog();
            }
        }

    }
}