using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class ServicioDatos
    {
        public void Insertar(
            string nombre,
            string? descripcion,
            decimal precio)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Servicio_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@descripcion",
                string.IsNullOrWhiteSpace(descripcion)
                    ? DBNull.Value
                    : descripcion);
            comando.Parameters.AddWithValue("@precio", precio);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Modificar(
            int idServicio,
            string nombre,
            string? descripcion,
            decimal precio)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Servicio_Modificar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_servicio", idServicio);
            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@descripcion",
                string.IsNullOrWhiteSpace(descripcion)
                    ? DBNull.Value
                    : descripcion);
            comando.Parameters.AddWithValue("@precio", precio);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Desactivar(int idServicio)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Servicio_Desactivar", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_servicio", idServicio);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable Listar()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Servicio_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}