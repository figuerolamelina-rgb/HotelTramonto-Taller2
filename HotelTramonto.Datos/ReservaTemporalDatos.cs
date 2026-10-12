
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class ReservaTemporalDatos
    {
        // Crear reserva temporal y devolver su ID
        public int Insertar(
            int idHuesped,
            int idHabitacion,
            DateTime fechaEntrada,
            DateTime fechaSalida,
            string modalidadPago)
        {
            using SqlConnection conexion =
                ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand(
                    "sp_ReservaTemporal_Insertar",
                    conexion);

            comando.CommandType =
                CommandType.StoredProcedure;

            comando.Parameters.AddWithValue(
                "@id_huesped", idHuesped);

            comando.Parameters.AddWithValue(
                "@id_habitacion", idHabitacion);

            comando.Parameters.AddWithValue(
                "@fecha_entrada", fechaEntrada.Date);

            comando.Parameters.AddWithValue(
                "@fecha_salida", fechaSalida.Date);

            comando.Parameters.AddWithValue(
                "@modalidad_pago", modalidadPago);

            conexion.Open();

            object? resultado = comando.ExecuteScalar();

            return Convert.ToInt32(resultado);
        }

        // Actualizar estado de la reserva temporal
        public void ActualizarEstado(
            int idReservaTemporal,
            string estado)
        {
            using SqlConnection conexion =
                ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand(
                    "sp_ReservaTemporal_ActualizarEstado",
                    conexion);

            comando.CommandType =
                CommandType.StoredProcedure;

            comando.Parameters.AddWithValue(
                "@id_reserva_temporal",
                idReservaTemporal);

            comando.Parameters.AddWithValue(
                "@estado",
                estado);

            conexion.Open();

            comando.ExecuteNonQuery();
        }

        // Marcar como expiradas las reservas vencidas
        public void ExpirarVencidas()
        {
            using SqlConnection conexion =
                ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand(
                    "sp_ReservaTemporal_ExpirarVencidas",
                    conexion);

            comando.CommandType =
                CommandType.StoredProcedure;

            conexion.Open();

            comando.ExecuteNonQuery();
        }

        // Listar reservas temporales pendientes
        public DataTable ListarPendientes()
        {
            using SqlConnection conexion =
                ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand(
                    "sp_ReservaTemporal_ListarPendientes",
                    conexion);

            comando.CommandType =
                CommandType.StoredProcedure;

            using SqlDataAdapter adaptador =
                new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();

            adaptador.Fill(tabla);

            return tabla;
        }
    }
}
