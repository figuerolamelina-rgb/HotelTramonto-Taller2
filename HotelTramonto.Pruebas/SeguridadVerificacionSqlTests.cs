using HotelTramonto.VerificacionSql;

namespace HotelTramonto.Pruebas;

[TestClass]
public class SeguridadVerificacionSqlTests
{
    [TestMethod]
    [DataRow("HotelTramontoDB")]
    [DataRow("HotelTramontoDB_Pruebas")]
    [DataRow("master")]
    [DataRow("HotelTramontoDB_HT56_00000000000000000000000000000000")]
    public void BasesProtegidasOAjena_NoSeAceptan(string nombre)
    {
        var identidad = new IdentidadCopia(Guid.NewGuid());
        Assert.IsFalse(identidad.EsPropia(nombre, identidad.Ejecucion.ToString("D")));
    }

    [TestMethod]
    public void MarcaAusenteODistinta_NoSeAcepta()
    {
        var identidad = new IdentidadCopia(Guid.NewGuid());
        Assert.IsFalse(identidad.EsPropia(identidad.Nombre, null));
        Assert.IsFalse(identidad.EsPropia(identidad.Nombre, Guid.NewGuid().ToString("D")));
        Assert.IsTrue(identidad.EsPropia(identidad.Nombre, identidad.Ejecucion.ToString("D")));
    }

    [TestMethod]
    public void AdaptacionSinGuardaOMultiplesLotes_SeRechaza()
    {
        var identidad = new IdentidadCopia(Guid.NewGuid());
        Assert.ThrowsExactly<InvalidOperationException>(() => identidad.Adaptar("SELECT 1;"));
        Assert.ThrowsExactly<InvalidOperationException>(() => identidad.Adaptar(IdentidadCopia.GuardaOriginal + "\nGO\n"));
        Assert.ThrowsExactly<InvalidOperationException>(() => identidad.Adaptar(IdentidadCopia.GuardaOriginal + "\nUSE HotelTramontoDB;"));
    }

    [TestMethod]
    public void ComparacionDeContratos_IgnoraFormatoPeroNoCambiaLiterales()
    {
        string uno = "CREATE OR ALTER PROCEDURE dbo.sp_Ejemplo AS SELECT 'HotelTramontoDB';";
        string dos = "-- comentario\nALTER PROCEDURE [dbo].[sp_Ejemplo] AS SELECT /* comentario */ 'HotelTramontoDB';";
        Assert.AreEqual(ContratosBase.Canonizar(uno, "sp_Ejemplo"), ContratosBase.Canonizar(dos, "sp_Ejemplo"));
        Assert.AreNotEqual(ContratosBase.Canonizar(uno, "sp_Ejemplo"),
            ContratosBase.Canonizar(dos.Replace("HotelTramontoDB", "otra"), "sp_Ejemplo"));
    }

    [TestMethod]
    public void PlanMantiene28CasosSql_YNoLosEjecuta()
    {
        Assert.AreEqual(28, CasosSql.Todos.Length);
        Assert.AreEqual(28, CasosSql.Todos.Select(c => c.Nombre).Distinct().Count());
    }

    [TestMethod]
    public void BarrerasYGuionesOriginales_PasanSinSql()
    {
        Assert.IsGreaterThan(20, OfflineChecks.Validar(OfflineChecks.Raiz()));
    }
}
