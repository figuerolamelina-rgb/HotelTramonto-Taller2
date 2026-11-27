using System.Text.Json;
using HotelTramonto.VerificacionSql;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Pruebas;

[TestClass]
public class ConfiguracionUiTemporalTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ConfiguracionUi_SoloCopiaAcreditadaSinCredencialesNiCambioDeConfianza(bool confiar)
    {
        var identidad = new IdentidadCopia(Guid.NewGuid());
        using var actual = new SqlConnection($@"Server=LAPTOP-5TSJJUP8\SQLEXPRESS;Database=HotelTramontoDB_Pruebas;Integrated Security=True;TrustServerCertificate={confiar}");
        using var json = JsonDocument.Parse(ConfiguracionUiTemporal.Crear(identidad, actual));
        Assert.AreEqual(identidad.Nombre, json.RootElement.GetProperty("BaseDatos").GetString());
        Assert.AreEqual(confiar, json.RootElement.GetProperty("ConfiarCertificadoServidor").GetBoolean());
        Assert.AreEqual(3, json.RootElement.EnumerateObject().Count());
        Assert.AreEqual(System.Data.ConnectionState.Closed, actual.State);
    }

    [TestMethod]
    [DataRow("HotelTramontoDB", @"LAPTOP-5TSJJUP8\SQLEXPRESS")]
    [DataRow("HotelTramontoDB_Pruebas", @"OTRO\SQLEXPRESS")]
    public void ConfiguracionUi_RechazaOrigenFueraDeAmbito(string baseDatos, string servidor)
    {
        using var actual = new SqlConnection($"Server={servidor};Database={baseDatos};Integrated Security=True");
        Assert.ThrowsExactly<InvalidOperationException>(() => ConfiguracionUiTemporal.Crear(new IdentidadCopia(Guid.NewGuid()), actual));
    }

    [TestMethod]
    public void ConfiguracionUi_SinGuidNoCreaDestino()
    {
        using var actual = new SqlConnection(@"Server=LAPTOP-5TSJJUP8\SQLEXPRESS;Database=HotelTramontoDB_Pruebas;Integrated Security=True");
        Assert.ThrowsExactly<InvalidOperationException>(() => ConfiguracionUiTemporal.Crear(new IdentidadCopia(Guid.Empty), actual));
    }
}
