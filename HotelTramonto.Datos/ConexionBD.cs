using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotelTramonto.Datos
{
    public class ConexionBD
    {
        public static SqlConnection ObtenerConexion()
        {
            // La configuración pertenece a la computadora, no al repositorio ni al ejecutable.
            string ruta = Environment.GetEnvironmentVariable("HOTELTRAMONTO_CONFIG")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "HotelTramonto", "conexion.local.json");

            try
            {
                var opciones = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
                };
                var configuracion = JsonSerializer.Deserialize<ConfiguracionConexion>(
                    File.ReadAllText(ruta), opciones);

                if (configuracion == null
                    || string.IsNullOrWhiteSpace(configuracion.Servidor)
                    || string.IsNullOrWhiteSpace(configuracion.BaseDatos))
                {
                    throw new InvalidOperationException("Completá el servidor y la base de datos en la configuración local de Hotel Tramonto.");
                }

                var cadena = new SqlConnectionStringBuilder
                {
                    DataSource = configuracion.Servidor.Trim(),
                    InitialCatalog = configuracion.BaseDatos.Trim(),
                    IntegratedSecurity = true,
                    Encrypt = SqlConnectionEncryptOption.Mandatory,
                    TrustServerCertificate = configuracion.ConfiarCertificadoServidor,
                    PersistSecurityInfo = false
                };
                return new SqlConnection(cadena.ConnectionString);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                or JsonException or ArgumentException or NotSupportedException)
            {
                // No incluir contenido del archivo, rutas privadas ni detalles del proveedor.
                throw new InvalidOperationException(
                    "No se pudo leer la configuración local de Hotel Tramonto. Revisá el archivo de conexión de esta computadora.");
            }
        }

        private sealed class ConfiguracionConexion
        {
            public string? Servidor { get; set; }
            public string? BaseDatos { get; set; }
            public bool ConfiarCertificadoServidor { get; set; }
        }
    }
}
