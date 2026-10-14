using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class DetalleReservaDatos
    {
        public void Insertar(
            int idReserva,
            int idHabitacion,
            decimal precioNoche,
            int cantidadPersonas)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleReserva_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_reserva", idReserva);
            comando.Parameters.AddWithValue("@id_habitacion", idHabitacion);
            comando.Parameters.AddWithValue("@precio_noche", precioNoche);
            comando.Parameters.AddWithValue("@cantidad_personas", cantidadPersonas);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Modificar(
            int idDetalleReserva,
            int idHabitacion,
            decimal precioNoche,
            int cantidadPersonas)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleReserva_Modificar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_detalle_reserva", idDetalleReserva);
            comando.Parameters.AddWithValue("@id_habitacion", idHabitacion);
            comando.Parameters.AddWithValue("@precio_noche", precioNoche);
            comando.Parameters.AddWithValue("@cantidad_personas", cantidadPersonas);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable ListarPorReserva(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleReserva_ListarPorReserva", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}