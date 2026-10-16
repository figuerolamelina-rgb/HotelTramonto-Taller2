using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class TipoHabitacionDatos
    {
        public void Insertar(
            string nombre,
            string? descripcion,
            decimal precioNoche,
            int capacidad)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_TipoHabitacion_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@nombre", nombre);
            comando.Parameters.AddWithValue("@descripcion",
                string.IsNullOrWhiteSpace(descripcion)
                    ? DBNull.Value
                    : descripcion);

            comando.Parameters.AddWithValue("@precio_noche", precioNoche);
            comando.Parameters.AddWithValue("@capacidad", capacidad);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Modificar(
            int idTipoHabitacion,
            string nombre,
            string? descripcion,
            decimal precioNoche,
            int capacidad)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_TipoHabitacion_Modificar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue(
                "@id_tipoHabitacion",
                idTipoHabitacion);

            comando.Parameters.AddWithValue("@nombre", nombre);

            comando.Parameters.AddWithValue("@descripcion",
                string.IsNullOrWhiteSpace(descripcion)
                    ? DBNull.Value
                    : descripcion);

            comando.Parameters.AddWithValue("@precio_noche", precioNoche);
            comando.Parameters.AddWithValue("@capacidad", capacidad);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Desactivar(int idTipoHabitacion)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_TipoHabitacion_Desactivar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue(
                "@id_tipoHabitacion",
                idTipoHabitacion);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable Listar()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_TipoHabitacion_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}