using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class ReporteDatos
    {
        public DataTable ObtenerOcupacion(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reporte_Ocupacion", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_desde", fechaDesde.Date);
            comando.Parameters.AddWithValue("@fecha_hasta", fechaHasta.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataSet ObtenerIngresos(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reporte_Ingresos", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_desde", fechaDesde.Date);
            comando.Parameters.AddWithValue("@fecha_hasta", fechaHasta.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataSet datos = new DataSet();
            adaptador.Fill(datos);

            return datos;
        }

        public DataSet ObtenerEstadisticasReservas(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reporte_EstadisticasReservas", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_desde", fechaDesde.Date);
            comando.Parameters.AddWithValue("@fecha_hasta", fechaHasta.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataSet datos = new DataSet();
            adaptador.Fill(datos);

            return datos;
        }

        public DataTable ObtenerHabitacionesMasUtilizadas(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reporte_HabitacionesMasUtilizadas", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_desde", fechaDesde.Date);
            comando.Parameters.AddWithValue("@fecha_hasta", fechaHasta.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }

        public DataTable ObtenerServiciosMasUtilizados(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Reporte_ServiciosMasUtilizados", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_desde", fechaDesde.Date);
            comando.Parameters.AddWithValue("@fecha_hasta", fechaHasta.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }
    }
}