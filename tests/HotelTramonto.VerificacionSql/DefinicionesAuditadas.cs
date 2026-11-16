using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HotelTramonto.VerificacionSql;

public static class DefinicionesAuditadas
{
    private static readonly Regex cabecera = new(
        @"\A[ \t\r\n]*(?:CREATE(?:[ \t\r\n]+OR[ \t\r\n]+ALTER)?|ALTER)[ \t\r\n]+PROCEDURE\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    public static string Canonizar(string definicion)
    {
        var match = cabecera.Match(definicion);
        if (!match.Success) throw new InvalidOperationException("Cabecera DDL no reconocida.");
        // ÚNICAMENTE el verbo DDL y su espacio inicial. Todo lo posterior a PROCEDURE
        // permanece idéntico: nombre, parámetros, cuerpo, literales y finales de línea.
        return "CREATE PROCEDURE" + definicion[match.Length..];
    }
    public static bool Coinciden(string esperado, string? observado) =>
        observado is not null && Canonizar(esperado).Equals(Canonizar(observado), StringComparison.Ordinal);

    public static Dictionary<string, string> Leer(string raiz)
    {
        string script = File.ReadAllText(Path.Combine(raiz, "base de datos HotelTramonto",
            "servicios-habitaciones", "04_ConcurrenciaPagos.sql"));
        var resultado = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(script, @"EXEC sys\.sp_executesql N'((?:''|[^'])*)';", RegexOptions.Singleline))
        {
            string texto = match.Groups[1].Value.Replace("''", "'");
            var nombre = Regex.Match(texto, @"\ACREATE OR ALTER PROCEDURE dbo\.(\w+)\b");
            if (!nombre.Success || !resultado.TryAdd(nombre.Groups[1].Value, texto))
                throw new InvalidOperationException("Manifiesto de definiciones inesperado.");
        }
        if (resultado.Count != 7) throw new InvalidOperationException("Se esperan siete definiciones auditadas.");
        return resultado;
    }
    public static bool ManifiestoCoincide(string raiz)
    {
        string sql = File.ReadAllText(Path.Combine(raiz, "base de datos HotelTramonto",
            "servicios-habitaciones", "05_VerificarConcurrencia.sql"));
        var lista = Regex.Matches(sql,
            @"INSERT @audit\(nombre,definicion\) VALUES\(N'(\w+)',CAST\(N'((?:''|[^'])*)' AS nvarchar\(max\)\)\);",
            RegexOptions.Singleline).Cast<Match>().ToDictionary(m => m.Groups[1].Value,
                m => m.Groups[2].Value.Replace("''", "'"), StringComparer.Ordinal);
        var esperados = Leer(raiz);
        return lista.Count == 7 && esperados.All(e => lista.TryGetValue(e.Key, out var valor) &&
            e.Value.Equals(valor, StringComparison.Ordinal));
    }

    public static string HashUtf16(string valor) =>
        Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(valor)));
}
