using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class DetalleServicioDatos : IDetalleServicioDatos
    {
        public void Insertar(
            int idServicios,
            int idReserva,
            int cantidad,
            decimal precioUnitario,
            int idHabitacion, Guid idOperacion)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleServicio_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.Add("@id_servicios", SqlDbType.Int).Value = idServicios;
            comando.Parameters.Add("@id_reserva", SqlDbType.Int).Value = idReserva;
            comando.Parameters.Add("@cantidad", SqlDbType.Int).Value = cantidad;
            SqlParameter precio = comando.Parameters.Add("@precio_unitario", SqlDbType.Decimal);
            precio.Precision = 10;
            precio.Scale = 2;
            precio.Value = precioUnitario;

            comando.Parameters.Add("@id_habitacion", SqlDbType.Int).Value = idHabitacion;
            comando.Parameters.Add("@id_operacion", SqlDbType.UniqueIdentifier).Value = idOperacion;
            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable ListarPorReserva(int idReserva, int? idHabitacion = null)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_DetalleServicio_ListarPorReserva", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.Add("@id_reserva", SqlDbType.Int).Value = idReserva;

            comando.Parameters.Add("@id_habitacion", SqlDbType.Int).Value =
                (object?)idHabitacion ?? DBNull.Value;
            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}


