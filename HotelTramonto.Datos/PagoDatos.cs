using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class PagoDatos
    {
        public void Insertar(
            int? idReserva,
            int? idReservaTemporal,
            int idUsuario,
            decimal monto,
            string metodoPago,
            string estado,
            string tipoPago)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Pago_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_reserva",
                idReserva.HasValue ? idReserva.Value : DBNull.Value);

            comando.Parameters.AddWithValue("@id_reserva_temporal",
                idReservaTemporal.HasValue ? idReservaTemporal.Value : DBNull.Value);

            comando.Parameters.AddWithValue("@id_usuario", idUsuario);
            comando.Parameters.AddWithValue("@monto", monto);
            comando.Parameters.AddWithValue("@metodo_pago", metodoPago);
            comando.Parameters.AddWithValue("@estado", estado);
            comando.Parameters.AddWithValue("@tipo_pago", tipoPago);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void ActualizarEstado(
            int idPago,
            string estado)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Pago_ActualizarEstado", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_pago", idPago);
            comando.Parameters.AddWithValue("@estado", estado);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable ListarPorReserva(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Pago_ListarPorReserva", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable RegistrarSaldoAlojamiento(
            int idReserva,
            int idUsuario,
            string metodoPago)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Pago_RegistrarSaldoAlojamiento", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_reserva", idReserva);
            comando.Parameters.AddWithValue("@id_usuario", idUsuario);
            comando.Parameters.AddWithValue("@metodo_pago", metodoPago);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable RegistrarServicios(
            int idReserva,
            int idUsuario,
            string metodoPago)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Pago_RegistrarServicios", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_reserva", idReserva);
            comando.Parameters.AddWithValue("@id_usuario", idUsuario);
            comando.Parameters.AddWithValue("@metodo_pago", metodoPago);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}