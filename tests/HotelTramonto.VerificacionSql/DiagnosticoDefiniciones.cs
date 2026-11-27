using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public static class DiagnosticoDefiniciones
{
    public static void Guardar(CopiaSql copia, string raiz, string directorio)
    {
        using var db = copia.Abrir(); // La misma validación de identidad, sin conexiones a originales.
        var registros = new List<object>();
        var servidor = Sql.Tabla(db, """
            SELECT CONVERT(nvarchar(128),SERVERPROPERTY('ProductVersion')) AS version,
             CONVERT(nvarchar(128),DATABASEPROPERTYEX(DB_NAME(),'Collation')) AS collation;
            """).Rows[0];
        foreach (var (nombre, esperado) in DefinicionesAuditadas.Leer(raiz))
        {
            using var cmd = new SqlCommand("""
                SELECT OBJECT_DEFINITION(OBJECT_ID(@nombre,'P')) AS definicion,
                 DATALENGTH(OBJECT_DEFINITION(OBJECT_ID(@nombre,'P'))) AS bytes_observados,
                 DATALENGTH(@esperado) AS bytes_esperados,
                 CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),OBJECT_DEFINITION(OBJECT_ID(@nombre,'P')))),2) AS hash_observado_sql,
                 CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),@esperado)),2) AS hash_esperado_sql,
                 m.uses_ansi_nulls,m.uses_quoted_identifier
                FROM (VALUES(1)) v(n)
                LEFT JOIN sys.sql_modules m ON m.object_id=OBJECT_ID(@nombre,'P');
                """, db);
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 128).Value = "dbo." + nombre;
            cmd.Parameters.Add("@esperado", SqlDbType.NVarChar, -1).Value = esperado;
            using var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            var row = table.Rows[0];
            string? actual = row["definicion"] as string;
            File.WriteAllText(Path.Combine(directorio, nombre + ".esperado.sql"), esperado, new UTF8Encoding(false));
            if (actual is not null)
                File.WriteAllText(Path.Combine(directorio, nombre + ".observado.sql"), actual, new UTF8Encoding(false));
            int indice = 0;
            if (actual is not null)
                while (indice < Math.Min(esperado.Length, actual.Length) && esperado[indice] == actual[indice]) indice++;
            bool literal = actual is not null && esperado.Equals(actual, StringComparison.Ordinal);
            bool soloCabecera = false;
            string? errorCabecera = null;
            try { soloCabecera = DefinicionesAuditadas.Coinciden(esperado, actual); }
            catch (InvalidOperationException ex) { errorCabecera = ex.Message; }
            object? Valor(string columna) => row.IsNull(columna) ? null : row[columna];
            registros.Add(new
            {
                procedimiento = nombre, versionServidor = servidor["version"], collation = servidor["collation"],
                definicionDisponible = actual is not null, igualdadLiteral = literal,
                igualdadConCabeceraNormalizada = soloCabecera, errorCabecera,
                longitudEsperadaUtf16 = esperado.Length, longitudObservadaUtf16 = actual?.Length,
                bytesEsperadosSql = Valor("bytes_esperados"), bytesObservadosSql = Valor("bytes_observados"),
                hashEsperadoUtf16 = DefinicionesAuditadas.HashUtf16(esperado),
                hashObservadoUtf16 = actual is null ? null : DefinicionesAuditadas.HashUtf16(actual),
                hashEsperadoSql = Valor("hash_esperado_sql"), hashObservadoSql = Valor("hash_observado_sql"),
                ansiNulls = Valor("uses_ansi_nulls"), quotedIdentifier = Valor("uses_quoted_identifier"),
                primeraDiferenciaUtf16 = literal || actual is null ? (int?)null : indice,
                contextoEsperado = Contexto(esperado, indice), contextoObservado = actual is null ? null : Contexto(actual, indice)
            });
        }
        File.WriteAllText(Path.Combine(directorio, "diagnostico-definiciones.json"),
            JsonSerializer.Serialize(registros, new JsonSerializerOptions { WriteIndented = true }));
    }
    private static object Contexto(string texto, int indice)
    {
        int inicio = Math.Max(0, Math.Min(indice, texto.Length) - 30);
        string fragmento = texto.Substring(inicio, Math.Min(120, texto.Length - inicio));
        return new { inicioUtf16 = inicio, texto = fragmento, unidadesUtf16 = fragmento.Select(c => ((int)c).ToString("X4")).ToArray() };
    }
}
