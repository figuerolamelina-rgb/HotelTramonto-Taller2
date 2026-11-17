using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public static class ConfiguracionUiTemporal
{
    // Lee únicamente configuración; no abre la conexión ni modifica el archivo privado.
    public static string Crear(IdentidadCopia copia, SqlConnection configuracionActual)
    {
        IdentidadCopia.ExigirNueva(copia.Ejecucion);
        var actual = new SqlConnectionStringBuilder(configuracionActual.ConnectionString);
        if (!actual.IntegratedSecurity ||
            !actual.DataSource.Equals(IdentidadCopia.Servidor, StringComparison.OrdinalIgnoreCase) ||
            !actual.InitialCatalog.Equals("HotelTramontoDB_Pruebas", StringComparison.Ordinal))
            throw new InvalidOperationException("La configuración de origen debe ser la instancia autorizada y HotelTramontoDB_Pruebas con autenticación Windows.");
        return JsonSerializer.Serialize(new
        {
            Servidor = IdentidadCopia.Servidor,
            BaseDatos = copia.Nombre,
            // Conserva la decisión de confianza del archivo local; no debilita la conexión.
            ConfiarCertificadoServidor = actual.TrustServerCertificate
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
