using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class DetalleServicioDatos
    {
        public void Insertar(
            int idServicios,
            int idReserva,
            int cantidad,
            decimal precioUnitario)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleServicio_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_servicios", idServicios);
            comando.Parameters.AddWithValue("@id_reserva", idReserva);
            comando.Parameters.AddWithValue("@cantidad", cantidad);
            comando.Parameters.AddWithValue("@precio_unitario", precioUnitario);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable ListarPorReserva(int idReserva)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleServicio_ListarPorReserva", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_reserva", idReserva);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}


