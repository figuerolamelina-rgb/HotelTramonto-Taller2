
using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class RolDatos
    {
        // Obtener todos los roles activos
        public DataTable Listar()
        {
            using SqlConnection conexion =
                ConexionBD.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand("dbo.sp_Rol_Listar", conexion);

            comando.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adaptador =
                new SqlDataAdapter(comando);

            DataTable tabla = new DataTable();

            adaptador.Fill(tabla);

            return tabla;
        }
    }
}
