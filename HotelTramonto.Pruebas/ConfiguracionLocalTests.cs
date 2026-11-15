using HotelTramonto.Datos;
using HotelTramonto.Negocios;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace HotelTramonto.Pruebas;

// Cambia una variable de proceso: no ejecutar junto a otras pruebas.
[TestClass]
[DoNotParallelize]
public class ConfiguracionLocalTests
{
    private string? rutaAnterior;
    private string ruta = string.Empty;

    [TestInitialize]
    public void Preparar()
    {
        rutaAnterior = Environment.GetEnvironmentVariable("HOTELTRAMONTO_CONFIG");
        ruta = Path.Combine(Path.GetTempPath(), $"tramonto-{Guid.NewGuid():N}.json");
        Environment.SetEnvironmentVariable("HOTELTRAMONTO_CONFIG", ruta);
    }

    [TestCleanup]
    public void Limpiar()
    {
        Environment.SetEnvironmentVariable("HOTELTRAMONTO_CONFIG", rutaAnterior);
        File.Delete(ruta);
    }

    [TestMethod]
    public void ConfiguracionLocal_UsaInstanciaYAutenticacionWindowsSinAbrirConexion()
    {
        File.WriteAllText(ruta, JsonSerializer.Serialize(new
        {
            Servidor = @".\SQLEXPRESS",
            BaseDatos = "HotelTramontoDB"
        }));

        using var conexion = ConexionBD.ObtenerConexion();
        var cadena = new SqlConnectionStringBuilder(conexion.ConnectionString);
        Assert.AreEqual(@".\SQLEXPRESS", cadena.DataSource);
        Assert.AreEqual("HotelTramontoDB", cadena.InitialCatalog);
        Assert.IsTrue(cadena.IntegratedSecurity);
        Assert.IsFalse(cadena.TrustServerCertificate);
        Assert.AreEqual(SqlConnectionEncryptOption.Mandatory, cadena.Encrypt);
        Assert.IsFalse(cadena.PersistSecurityInfo);
        Assert.AreEqual(System.Data.ConnectionState.Closed, conexion.State);
    }

    [TestMethod]
    public void ConfiguracionLocal_PermiteCertificadoLocalSoloExplicitamente()
    {
        File.WriteAllText(ruta,
            """{"Servidor":"localhost","BaseDatos":"HotelTramontoDB","ConfiarCertificadoServidor":true}""");
        using var conexion = ConexionBD.ObtenerConexion();
        Assert.IsTrue(new SqlConnectionStringBuilder(conexion.ConnectionString).TrustServerCertificate);
    }

    [TestMethod]
    public void ConfiguracionLocal_NoReinterpretaServidorComoCadenaDeConexion()
    {
        const string servidor = "localhost;Integrated Security=false";
        File.WriteAllText(ruta, JsonSerializer.Serialize(new { Servidor = servidor, BaseDatos = "HotelTramontoDB" }));
        using var conexion = ConexionBD.ObtenerConexion();
        var cadena = new SqlConnectionStringBuilder(conexion.ConnectionString);
        Assert.AreEqual(servidor, cadena.DataSource);
        Assert.IsTrue(cadena.IntegratedSecurity);
    }

    [TestMethod]
    public void ConfiguracionLocal_ArchivoAusenteNoRevelaRuta()
    {
        var error = Assert.Throws<InvalidOperationException>(() => ConexionBD.ObtenerConexion());
        Assert.DoesNotContain(ruta, error.Message);
        Assert.IsNull(error.InnerException);
    }

    [TestMethod]
    [DataRow("{")]
    [DataRow("null")]
    [DataRow("{}")]
    [DataRow("""{"Servidor":" ","BaseDatos":"HotelTramontoDB"}""")]
    [DataRow("{\"Servidor\":\"localhost\",\"BaseDatos\":\"\"}")]
    [DataRow("{\"Servidor\":\"localhost\",\"BaseDatos\":\"HotelTramontoDB\",\"UserId\":\"no-admitido\"}")]
    public void ConfiguracionLocal_RechazaArchivosInvalidos(string json)
    {
        File.WriteAllText(ruta, json);
        Assert.Throws<InvalidOperationException>(() => ConexionBD.ObtenerConexion());
    }

    [TestMethod]
    public void UsuarioNegocios_RechazaSieteCaracteresAntesDeAccederASql()
    {
        string corta = Guid.NewGuid().ToString("N")[..7];
        Assert.Throws<ArgumentException>(() => new UsuarioNegocios().CambiarContrasena(1, corta));
    }

    [TestMethod]
    public void UsuarioDatos_RechazaLongitudInsuficienteAntesDeAccederASql()
    {
        var datos = new UsuarioDatos();
        string valorTemporal = Guid.NewGuid().ToString("N");
        Assert.Throws<ArgumentException>(() => datos.CambiarContrasena(1, valorTemporal, 7));
        Assert.Throws<ArgumentException>(() => datos.Insertar(1, "Nombre", "Apellido", "12345678",
            "persona@example.invalid", valorTemporal, 7, null));
    }
}
