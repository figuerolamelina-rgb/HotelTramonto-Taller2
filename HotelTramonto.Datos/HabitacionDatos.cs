using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class HabitacionDatos
    {
        public void Insertar(
            int tipoHabitacion,
            int? piso,
            string numero,
            string estado)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_Insertar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@tipo_habitacion", tipoHabitacion);
            comando.Parameters.AddWithValue("@piso",
                piso.HasValue ? piso.Value : DBNull.Value);
            comando.Parameters.AddWithValue("@numero", numero);
            comando.Parameters.AddWithValue("@estado", estado);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Modificar(
            int idHabitacion,
            int tipoHabitacion,
            int? piso,
            string numero,
            string estado)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_Modificar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_habitacion", idHabitacion);
            comando.Parameters.AddWithValue("@tipo_habitacion", tipoHabitacion);
            comando.Parameters.AddWithValue("@piso",
                piso.HasValue ? piso.Value : DBNull.Value);
            comando.Parameters.AddWithValue("@numero", numero);
            comando.Parameters.AddWithValue("@estado", estado);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void Desactivar(int idHabitacion)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_Desactivar", conexion);

            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@id_habitacion", idHabitacion);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public DataTable Listar()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }


        public DataTable BuscarPorId(int idHabitacion)
        {
            DataTable habitaciones = Listar();

            DataRow[] filas = habitaciones.Select(
                $"id_habitacion = {idHabitacion}");

            DataTable resultado = habitaciones.Clone();

            foreach (DataRow fila in filas)
            {
                resultado.ImportRow(fila);
            }

            return resultado;
        }

        public DataTable ConsultarDisponibilidad(
            DateTime fechaEntrada,
            DateTime fechaSalida)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();
            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_ConsultarDisponibilidad", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@fecha_entrada", fechaEntrada.Date);
            comando.Parameters.AddWithValue("@fecha_salida", fechaSalida.Date);

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }


        
        public DataTable ListarInactivas()
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_ListarInactivas", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador = new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();
            adaptador.Fill(tabla);

            return tabla;
        }




        public void Reactivar(int idHabitacion)
        {
            using SqlConnection conexion = ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand("sp_Habitacion_Reactivar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.AddWithValue("@id_habitacion", idHabitacion);

            conexion.Open();
            comando.ExecuteNonQuery();
        }



    }
}