using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class ReservaTemporalDatos
    {
        public void Insertar(
            int idHuesped,
            int idHabitacion,
            DateTime fechaEntrada,
            DateTime fechaSalida)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_ReservaTemporal_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_huesped", idHuesped);
            comando.Parameters.AddWithValue("@id_habitacion", idHabitacion);
            comando.Parameters.AddWithValue("@fecha_entrada", fechaEntrada.Date);
            comando.Parameters.AddWithValue("@fecha_salida", fechaSalida.Date);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void ActualizarEstado(
            int idReservaTemporal,
            string estadoPago)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_ReservaTemporal_ActualizarEstado", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue(
                "@id_reserva_temporal",
                idReservaTemporal);

            comando.Parameters.AddWithValue(
                "@estadoPago",
                estadoPago);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void ExpirarVencidas()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_ReservaTemporal_ExpirarVencidas", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable ListarPendientes()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_ReservaTemporal_ListarPendientes", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}