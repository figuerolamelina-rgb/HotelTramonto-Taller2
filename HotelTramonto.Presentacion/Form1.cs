using Microsoft.Data.SqlClient;
using HotelTramonto.Datos;

namespace HotelTramonto.Presentacion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
    }
    }