using System.Text.RegularExpressions;

namespace HotelTramonto.VerificacionSql;

public sealed record IdentidadCopia(Guid Ejecucion)
{
    public const string Servidor = @"LAPTOP-5TSJJUP8\SQLEXPRESS";
    public const string Prefijo = "HotelTramontoDB_HT56_";
    public const string Marca = "HotelTramonto_TestRun";
    public string Nombre => Prefijo + Ejecucion.ToString("N");
    public bool EsPropia(string nombre, string? marca) =>
        Ejecucion != Guid.Empty && string.Equals(nombre, Nombre, StringComparison.Ordinal) &&
        string.Equals(marca, Ejecucion.ToString("D"), StringComparison.Ordinal);

    public static void ExigirNueva(Guid id)
    {
        if (id == Guid.Empty) throw new InvalidOperationException("Ejecución inválida.");
    }

    public const string GuardaOriginal =
        "IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'";

    // SQL original permanece protegido. La adaptación SOLO existe en memoria y exige
    // nombre derivado de GUID, parámetros y marca de esa misma ejecución.
    public string Adaptar(string sql)
    {
        ExigirNueva(Ejecucion);
        if (Regex.Matches(sql, Regex.Escape(GuardaOriginal)).Count != 1 ||
            Regex.IsMatch(sql, @"(?im)^\s*(GO|USE)\b"))
            throw new InvalidOperationException("Guarda o lotes inesperados; no se adapta el script.");
        string guarda = """
            IF @HT_Run IS NULL OR @HT_Run='00000000-0000-0000-0000-000000000000'
             OR @HT_Db COLLATE Latin1_General_100_BIN2 <>
                (N'HotelTramontoDB_HT56_'+LOWER(REPLACE(CONVERT(nvarchar(36),@HT_Run),N'-',N''))) COLLATE Latin1_General_100_BIN2
             OR DB_NAME() COLLATE Latin1_General_100_BIN2 <> @HT_Db COLLATE Latin1_General_100_BIN2
             OR DB_ID()<=4
             OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'HotelTramonto_TestRun'
                AND CONVERT(nvarchar(36),value) COLLATE Latin1_General_100_BIN2 =
                    LOWER(CONVERT(nvarchar(36),@HT_Run)) COLLATE Latin1_General_100_BIN2)
             THROW 51600, 'Destino temporal no validado; operacion cancelada.', 1;
            IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> @HT_Db COLLATE Latin1_General_100_BIN2
            """;
        return sql.Replace(GuardaOriginal, guarda.Trim(), StringComparison.Ordinal);
    }

    public static string Identificador(string valor) => "[" + valor.Replace("]", "]]") + "]";
}
