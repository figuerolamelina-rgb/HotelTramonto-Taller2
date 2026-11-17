using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class IntegracionCorreccionesTests
{
    [TestMethod]
    [DataRow("ADMINISTRADOR", true)]
    [DataRow(" administrador ", true)]
    [DataRow("RECEPCIONISTA", false)]
    [DataRow(" recepcionista ", false)]
    public void HistorialPermitido_NoAmpliaPermisosAdministrativosDeHabitaciones(string rol, bool administra)
    {
        HuespedNegocios.ExigirPermisoHistorial(rol);
        Assert.AreEqual(administra, PermisosHabitaciones.PuedeAdministrar(rol));
        // Las validaciones se ejecutan antes de cualquier consulta real.
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new HuespedNegocios().HistorialReservas(0, rol));
        if (administra)
            Assert.ThrowsExactly<ArgumentException>(() => new HabitacionNegocios().Desactivar(0, rol));
        else
            Assert.ThrowsExactly<UnauthorizedAccessException>(() => new HabitacionNegocios().Desactivar(0, rol));
    }

    [TestMethod]
    public void GerenteNoObtieneAdministracionNiHistorialPorLaIntegracion()
    {
        Assert.IsFalse(PermisosHabitaciones.PuedeAdministrar("GERENTE"));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => HuespedNegocios.ExigirPermisoHistorial("GERENTE"));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new HabitacionNegocios().Desactivar(1, "GERENTE"));
    }
}
