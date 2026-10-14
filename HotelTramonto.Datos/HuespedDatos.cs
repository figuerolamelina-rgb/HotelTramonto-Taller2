using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class HuespedDatos
    {
        public void Insertar(
            string nombre,
            string apellido,
            string dni,
            string? telefono,
            string? email,
            string nacionalidad)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Huesped_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@apellido", apellido);
            comando.Parameters.AddWithValue("@dni", dni);
            comando.Parameters.AddWithValue("@telefono",
                string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : telefono);
            comando.Parameters.AddWithValue("@email",
                string.IsNullOrWhiteSpace(email) ? DBNull.Value : email);
            comando.Parameters.AddWithValue("@nacionalidad", nacionalidad);

            conexion.Open();
            comando.ExecuteNonQuery();
        }


        public void Modificar(
            int idHuesped,
            string nombre,
            string apellido,
            string dni,
            string? telefono,
            string? email,
            string nacionalidad)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Huesped_Modificar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_huesped", idHuesped);
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@apellido", apellido);
            comando.Parameters.AddWithValue("@dni", dni);
            comando.Parameters.AddWithValue("@telefono",
                string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : telefono);
            comando.Parameters.AddWithValue("@email",
                string.IsNullOrWhiteSpace(email) ? DBNull.Value : email);
            comando.Parameters.AddWithValue("@nacionalidad", nacionalidad);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Desactivar(int idHuesped)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Huesped_Desactivar", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_huesped", idHuesped);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable Listar()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Huesped_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable BuscarPorDNI(string dni)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando = new SqlCommand("sp_Huesped_BuscarPorDNI", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@dni", dni);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }



    }
}