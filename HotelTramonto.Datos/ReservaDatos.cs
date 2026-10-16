using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class ReservaDatos
    {



        public void Cancelar(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_Cancelar", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable Listar()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable ListarPorHuesped(int idHuesped)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_ListarPorHuesped", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_huesped", idHuesped);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable ListarPorFechas(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_ListarPorFechas", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_desde", fechaDesde.Date);
            comando.Parameters.AddWithValue("@fecha_hasta", fechaHasta.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable ListarPorEstado(string estado)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_ListarPorEstado", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@estado", estado);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable ConsultarSaldo(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_ConsultarSaldo", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable CheckIn(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_CheckIn", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable CheckOut(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reserva_CheckOut", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable ConfirmarReserva(
            int idReservaTemporal,
            int cantidadPersonas)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_ConfirmarReserva", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue(
                "@id_reserva_temporal",
                idReservaTemporal);

            comando.Parameters.AddWithValue(
                "@cantidad_personas",
                cantidadPersonas);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}
