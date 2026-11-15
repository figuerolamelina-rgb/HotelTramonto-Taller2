using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class HistorialHuespedTests
{
    [TestMethod]
    [DataRow("ADMINISTRADOR")]
    [DataRow("RECEPCIONISTA")]
    [DataRow(" recepcionista ")]
    public void RolesOperativos_PuedenConsultar(string rol)
    {
        HuespedNegocios.ExigirPermisoHistorial(rol);
    }

    [TestMethod]
    [DataRow("GERENTE")]
    [DataRow("")]
    [DataRow("HUΈSPED")]
    public void OtrosRoles_NoPuedenConsultar(string rol)
    {
        Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
            new HuespedNegocios().HistorialReservas(1, rol));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    public void IdentificadorInvalido_NoConsultaSQL(int id)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new HuespedNegocios().HistorialReservas(id, "RECEPCIONISTA"));
    }
}
