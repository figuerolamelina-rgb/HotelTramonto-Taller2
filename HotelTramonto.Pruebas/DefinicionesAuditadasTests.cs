using System.Text.RegularExpressions;
using HotelTramonto.VerificacionSql;

namespace HotelTramonto.Pruebas;

[TestClass]
public class DefinicionesAuditadasTests
{
    [TestMethod]
    public void SieteProcedimientos_AceptanSoloLaRepresentacionDelVerboDDL()
    {
        var definiciones = DefinicionesAuditadas.Leer(OfflineChecks.Raiz());
        Assert.AreEqual(7, definiciones.Count);
        foreach (var (_, esperado) in definiciones)
            foreach (string cabecera in new[] { "CREATE PROCEDURE", "ALTER PROCEDURE",
                "CREATE   PROCEDURE", " \r\nCREATE\tOR  ALTER\tPROCEDURE", "alter procedure" })
            {
                string observado = Regex.Replace(esperado, @"\ACREATE OR ALTER PROCEDURE", cabecera);
                Assert.IsTrue(DefinicionesAuditadas.Coinciden(esperado, observado));
            }
    }

    [TestMethod]
    public void SieteProcedimientos_RechazanCambiosDeNombreParametrosYCuerpo()
    {
        foreach (var (nombre, esperado) in DefinicionesAuditadas.Leer(OfflineChecks.Raiz()))
        {
            Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado.Replace("dbo." + nombre, "dbo.otro")));
            Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado + "\n"));
            Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado.Replace("\n", "\r\n")));
            Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, null));
            string parametros = Regex.Replace(esperado, @"\bINT\b", "BIGINT", RegexOptions.None);
            Assert.AreNotEqual(esperado, parametros);
            Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, parametros));
        }
    }

    [TestMethod]
    public void RechazaCambiosFuncionalesDePagosBloqueosYTransacciones()
    {
        var esperado = DefinicionesAuditadas.Leer(OfflineChecks.Raiz())["sp_Pago_Insertar"];
        foreach (var (antes, despues) in new[]
        {
            ("DECIMAL(10,2)", "DECIMAL(12,2)"),
            ("@LockMode='Exclusive'", "@LockMode='Shared'"),
            ("@LockTimeout=10000", "@LockTimeout=20000"),
            ("BEGIN TRANSACTION;", "SELECT 1;"),
            ("COMMIT;", "ROLLBACK;"),
            ("@monto>@saldo", "@monto<@saldo"),
            ("'APROBADO'", "'PENDIENTE'"),
            ("'CHECKIN'", "'CONFIRMADA'"),
            ("solicitud_hash", "otra_columna"),
            ("WITH (UPDLOCK,HOLDLOCK)", "WITH (NOLOCK)")
        })
        {
            Assert.IsTrue(esperado.Contains(antes, StringComparison.Ordinal), antes);
            Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado.Replace(antes, despues)), antes);
        }
    }

    [TestMethod]
    public void NoEliminaComentariosNiNormalizaLiteralesDelCuerpo()
    {
        const string esperado = "CREATE OR ALTER PROCEDURE dbo.x AS SELECT 'a  b', N'X'; -- comentario\n";
        Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado.Replace("a  b", "a b")));
        Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado.Replace("N'X'", "N'x'")));
        Assert.IsFalse(DefinicionesAuditadas.Coinciden(esperado, esperado.Replace("-- comentario", "")));
        Assert.ThrowsExactly<InvalidOperationException>(() => DefinicionesAuditadas.Canonizar("EXEC dbo.x;"));
    }

    [TestMethod]
    public void ManifiestoSql05_CoincideLiteralmenteConLasSieteDefiniciones04()
    {
        string raiz = OfflineChecks.Raiz();
        string verificador = File.ReadAllText(Path.Combine(raiz, "base de datos HotelTramonto",
            "servicios-habitaciones", "05_VerificarConcurrencia.sql"));
        var insertados = Regex.Matches(verificador,
            @"INSERT @audit\(nombre,definicion\) VALUES\(N'(\w+)',CAST\(N'((?:''|[^'])*)' AS nvarchar\(max\)\)\);",
            RegexOptions.Singleline).Cast<Match>().ToDictionary(m => m.Groups[1].Value,
                m => m.Groups[2].Value.Replace("''", "'"));
        var esperados = DefinicionesAuditadas.Leer(raiz);
        Assert.AreEqual(7, insertados.Count);
        foreach (var (nombre, texto) in esperados) Assert.AreEqual(texto, insertados[nombre], nombre);
        Assert.IsTrue(verificador.Contains("CONVERT(varbinary(max),@canonE)", StringComparison.Ordinal));
        Assert.IsTrue(verificador.Contains("DATALENGTH(@canonE)<>DATALENGTH(@canonO)", StringComparison.Ordinal));
        Assert.IsTrue(verificador.Contains("THROW 51438", StringComparison.Ordinal));
    }
}
