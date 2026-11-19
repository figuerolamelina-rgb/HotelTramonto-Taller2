using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

internal static class DiagnosticoCiclo
{
    internal static void Validar(SqlConnection copia, string carpetaSql, string informes)
    {
        string script = File.ReadAllText(Path.Combine(carpetaSql, "07_CicloReserva.sql"));
        // Usar el mismo cálculo escalar que 07 y 08, también para los casos sintéticos.
        var bloque = Regex.Match(script, @"(?ms)^    -- INICIO canon de ciclo[^\n]*\n(.*?)^    -- FIN canon de ciclo");
        if (!bloque.Success) throw new InvalidDataException("Falta el comparador de CheckIn/Cancelar en 07.");
        using var cmd = new SqlCommand("DECLARE @canon nvarchar(max);\n" + bloque.Groups[1].Value + "\nSELECT @canon;", copia);
        var parametro = cmd.Parameters.Add("@texto", SqlDbType.NVarChar, -1);
        string? Canon(string? valor)
        {
            parametro.Value = (object?)valor ?? DBNull.Value;
            using var fila = cmd.ExecuteReader(CommandBehavior.SingleRow);
            return fila.Read() && !fila.IsDBNull(0) ? fila.GetString(0) : null;
        }

        var registros = new List<object>();
        var rechazados = new List<string>();
        string[] nombres = ["sp_Reserva_CheckIn", "sp_Reserva_Cancelar"];
        for (int i = 0; i < nombres.Length; i++)
        {
            string nombre = nombres[i];
            var literal = Regex.Match(script, $@"@anterior{i} nvarchar\(max\)=N'((?:''|[^'])*)'");
            if (!literal.Success) throw new InvalidDataException("Falta contrato histórico para dbo." + nombre);
            string esperado = literal.Groups[1].Value.Replace("''", "'");
            string? actual = Sql.Tabla(copia, "SELECT OBJECT_DEFINITION(OBJECT_ID(@p0,'P'))", "dbo." + nombre).Rows[0][0] as string;
            File.WriteAllText(Path.Combine(informes, nombre + ".historico.sql"), esperado);
            if (actual != null) File.WriteAllText(Path.Combine(informes, nombre + ".restaurado.sql"), actual);

            string? canonEsperado = Canon(esperado), canonActual = Canon(actual);
            bool coincide = canonEsperado != null && canonEsperado == canonActual;
            registros.Add(new {
                procedimiento = "dbo." + nombre, disponible = actual != null, coincide,
                diferenciaLiteral = Diferencia(esperado, actual),
                diferenciaCanonica = Diferencia(canonEsperado, canonActual)
            });
            if (!coincide) rechazados.Add("dbo." + nombre);
        }
        string archivo = Path.Combine(informes, "diagnostico-ciclo.json");
        File.WriteAllText(archivo, JsonSerializer.Serialize(registros, new JsonSerializerOptions { WriteIndented = true }));
        if (rechazados.Count > 0)
            throw new InvalidOperationException("Contrato histórico desconocido: " + string.Join(", ", rechazados) + ". Ver " + archivo);

        // Ejecutar el comparador real sobre parámetros de texto en la misma copia.
        foreach (string nombre in nombres)
        {
            string cuerpo = nombre + " AS\nSELECT N'a''  B\r\nCREATE PROCEDURE dbo.x';";
            string? canon = Canon("CREATE OR ALTER PROCEDURE dbo." + cuerpo);
            if (canon == null || canon != Canon("---check-in\r\n-- CREATE PROCEDURE falso\nCREATE   \r\nPROCEDURE\t" +
                cuerpo.Replace("AS\n", "AS\r\n") + "\r\n"))
                throw new InvalidOperationException("Prueba positiva del comparador: dbo." + nombre);
            foreach (string? variante in new string?[] {
                null, "SELECT 1;\nCREATE PROCEDURE " + cuerpo,
                "CREATE PROCEDURE " + cuerpo.Replace("SELECT", "PRINT"),
                "ALTER PROCEDURE " + cuerpo.Replace("  B", " B"),
                "ALTER PROCEDURE " + cuerpo.Replace("  B", "  b"),
                "ALTER PROCEDURE " + cuerpo.Replace("B\r\n", "B\n"),
                "ALTER PROCEDURE " + cuerpo.Replace("CREATE PROCEDURE dbo.x", "ALTER PROCEDURE x") })
                if (canon == Canon(variante))
                    throw new InvalidOperationException("Prueba negativa del comparador: dbo." + nombre);
        }
    }

    // Índice en unidades UTF-16 desde cero. Los archivos contienen ambos textos completos.
    internal static object? Diferencia(string? esperado, string? actual)
    {
        if (esperado == null || actual == null || esperado == actual) return null;
        int indice = 0;
        while (indice < Math.Min(esperado.Length, actual.Length) && esperado[indice] == actual[indice]) indice++;
        int inicio = Math.Max(0, indice - 24);
        string Contexto(string s) => s.Substring(inicio, Math.Min(96, s.Length - inicio));
        return new { indice, inicio, esperado = Contexto(esperado), observado = Contexto(actual),
            longitudEsperada = esperado.Length, longitudObservada = actual.Length };
    }
}
