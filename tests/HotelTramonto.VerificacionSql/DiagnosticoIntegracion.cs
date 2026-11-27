using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

internal static class DiagnosticoIntegracion
{
    // Comparar con la misma expresión y collation del paso 09. La decisión de
    // instalar sigue dentro de la transacción de ese script.
    private const string Consulta = """
        DECLARE @crlf nchar(2) = NCHAR(13) + NCHAR(10);
        DECLARE @anterior nvarchar(max) = @p2;
        DECLARE @historicaConCRLF nvarchar(max) = @crlf + REPLACE(REPLACE(@anterior, @crlf, NCHAR(10)), NCHAR(10), @crlf) + @crlf;
        WITH textos AS (
            SELECT v.variante, v.texto, o.id
            FROM (SELECT OBJECT_ID(@p0) AS id) o
            CROSS APPLY (VALUES
                ('observada', OBJECT_DEFINITION(o.id)), ('revisada', @p1),
                ('historica', @p2), ('iara', @p3)) v(variante,texto)
        ), comparables AS (
            SELECT variante, texto, id,
                LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(texto,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 AS canon
            FROM textos
        )
        SELECT o.id, o.texto AS observada, o.canon AS canonObservado, r.variante,
               r.texto AS esperada, r.canon AS canonEsperado,
               CONVERT(bit,CASE WHEN o.canon=r.canon THEN 1
                   WHEN r.variante='historica' AND DATALENGTH(o.texto) = DATALENGTH(@historicaConCRLF)
                       AND o.texto COLLATE Latin1_General_100_BIN2 = @historicaConCRLF
                   THEN 1 ELSE 0 END) AS coincide
        FROM comparables o CROSS JOIN comparables r
        WHERE o.variante='observada' AND r.variante<>'observada' AND r.texto IS NOT NULL
        ORDER BY r.variante;
        """;

    private static readonly (string Nombre, string Variable)[] Procedimientos =
    [
        ("dbo.sp_Habitacion_ListarConReservaCheckIn", "nuevo0"),
        ("dbo.sp_Pago_ObtenerComprobante", "nuevo1"),
        ("dbo.sp_Reporte_ServiciosMasUtilizados", "nuevo2")
    ];

    private static string Literal(string script, string variable, string procedimiento)
    {
        var literal = Regex.Match(script, $@"@{variable} nvarchar\(max\)=N'((?:''|[^'])*)'");
        if (!literal.Success) throw new InvalidDataException($"Falta @{variable} para {procedimiento}.");
        return literal.Groups[1].Value.Replace("''", "'");
    }

    internal static void Guardar(SqlConnection copia, string script, string directorio)
    {
        var registros = new List<object>();
        foreach (var (nombre, variable) in Procedimientos)
        {
            bool reporte = variable == "nuevo2";
            var tabla = Sql.Tabla(copia, Consulta, nombre, Literal(script, variable, nombre),
                reporte ? Literal(script, "anterior", nombre) : null, reporte ? Literal(script, "iara", nombre) : null);
            DataRow objeto = tabla.Rows[0];
            string? observada = objeto["observada"] as string;
            if (observada != null) File.WriteAllText(Path.Combine(directorio, nombre + ".observada.sql"), observada);

            var comparaciones = new List<object>();
            bool coincide = false;
            foreach (DataRow fila in tabla.Rows)
            {
                string variante = (string)fila["variante"], esperada = (string)fila["esperada"];
                bool igual = (bool)fila["coincide"];
                coincide |= igual;
                File.WriteAllText(Path.Combine(directorio, nombre + "." + variante + ".sql"), esperada);
                comparaciones.Add(new {
                    variante, coincideSegun09 = igual,
                    diferenciaLiteral = DiagnosticoCiclo.Diferencia(esperada, observada),
                    diferenciaSegun09 = igual ? null : DiagnosticoCiclo.Diferencia((string)fila["canonEsperado"], fila["canonObservado"] as string)
                });
            }
            string estado = objeto.IsNull("id") ? "Objeto ausente o no visible" :
                observada == null ? "Definición no disponible" :
                coincide ? "Definición reconocida" : "Definición desconocida";
            registros.Add(new { procedimiento = nombre, estado, comparaciones });
            Console.WriteLine($"{nombre}: {estado}");
        }
        string archivo = Path.Combine(directorio, "diagnostico-integracion.json");
        File.WriteAllText(archivo, JsonSerializer.Serialize(registros, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Diagnóstico previo al paso 09: " + archivo);
    }

    internal static void GuardarPaso10(SqlConnection copia, string script, string directorio)
    {
        var bloque = Regex.Match(script, @"(?ms)^-- INICIO comparación del paso 10\r?\n(.*?)^-- FIN comparación del paso 10\r?$");
        if (!bloque.Success) throw new InvalidDataException("Falta la comparación de consultas del paso 10.");
        string consulta = "DECLARE @nuevo0 nvarchar(max)=@p0, @nuevo1 nvarchar(max)=@p1, @nuevo2 nvarchar(max)=@p2;\n"
            + bloque.Groups[1].Value + "\nSELECT * FROM comparaciones ORDER BY nombre;";
        var contratos = Procedimientos.Select(p => Literal(script, p.Variable, p.Nombre)).ToArray();
        var tabla = Sql.Tabla(copia, consulta, contratos);
        var registros = new List<object>();
        foreach (DataRow fila in tabla.Rows)
        {
            string nombre = (string)fila["nombre"], esperada = (string)fila["esperada"];
            string? observada = fila["observada"] as string;
            bool coincide = (int)fila["coincide"] == 1;
            string estado = fila.IsNull("id") ? "Objeto ausente o no visible" :
                observada == null ? "Definición no disponible" :
                coincide ? "Definición reconocida" : "Definición diferente";
            var diferencia = fila["cuerpoObservado"] is string cuerpo
                ? DiagnosticoCiclo.Diferencia((string)fila["cuerpoEsperado"], cuerpo)
                : DiagnosticoCiclo.Diferencia(esperada, observada);
            registros.Add(new {
                procedimiento = nombre, estado, coincideSegun10 = coincide,
                diferenciaLiteral = DiagnosticoCiclo.Diferencia(esperada, observada),
                diferenciaSegun10 = diferencia, esperada, observada
            });
            if (!coincide)
            {
                Console.WriteLine($"Paso 10: {nombre}: {estado}");
                if (diferencia != null) Console.WriteLine(JsonSerializer.Serialize(diferencia));
            }
        }
        string archivo = Path.Combine(directorio, "diagnostico-paso10.json");
        File.WriteAllText(archivo, JsonSerializer.Serialize(registros, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Diagnóstico previo al paso 10: " + archivo);
    }
}
