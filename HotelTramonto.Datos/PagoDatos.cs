
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class PagoDatos
    {
        // Insertar un pago y devolver su ID
        public int Insertar(
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

            object? resultado = comando.ExecuteScalar();

            if (resultado == null || resultado == DBNull.Value)
            {
                throw new Exception(
                    "No se pudo obtener el ID del pago registrado.");
            }

            return Convert.ToInt32(resultado);
        }

        // Actualizar el estado de un pago
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

        // Listar pagos de una reserva
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

        // Registrar el saldo pendiente de alojamiento
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

        // Registrar el pago de servicios adicionales
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
