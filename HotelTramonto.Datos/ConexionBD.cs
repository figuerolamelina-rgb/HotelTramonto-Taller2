using Microsoft.Data.SqlClient;

namespace HotelTramonto.Datos
{
    public class ConexionBD
    {

        private static readonly string cadenaConexion =
            @"Server=FUN;Database=HotelTramontoDB;Trusted_Connection=True;TrustServerCertificate=True;";


        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}