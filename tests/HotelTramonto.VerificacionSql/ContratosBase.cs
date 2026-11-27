using System.Text.RegularExpressions;

namespace HotelTramonto.VerificacionSql;

public static class ContratosBase
{
    public static readonly string[] Procedimientos =
        ["sp_ReservaTemporal_Insertar", "sp_ConfirmarReserva", "sp_Reserva_CheckIn", "sp_Reserva_ConsultarSaldo"];
    public static readonly string[] Tablas =
        ["Roles", "Usuarios", "Huespedes", "TiposHabitacion", "Habitaciones", "Servicios",
         "Reservas", "DetalleReservas", "ReservasTemporales", "DetalleServicios", "Pagos"];

    // Ignora formato/comentarios, pero conserva literales y límites de tokens.
    // No se acepta una definición local con lógica diferente a la revisada.
    public static string Canonizar(string texto, string nombre)
    {
        string cabecera = @"\b(?:CREATE\s+OR\s+ALTER|CREATE|ALTER)\s+PROCEDURE\s+(?:\[?dbo\]?\s*\.\s*)?\[?" +
            Regex.Escape(nombre) + @"\]?(?=\s|$)";
        var match = Regex.Match(texto, cabecera, RegexOptions.IgnoreCase);
        if (!match.Success) throw new InvalidOperationException("Contrato SQL no encontrado: " + nombre);
        texto = "CREATE PROCEDURE dbo." + nombre + texto[(match.Index + match.Length)..];
        texto = Regex.Split(texto, @"(?im)^\s*GO\s*;?\s*$")[0];
        var tokens = Regex.Matches(texto,
            @"N?'(?:''|[^'])*'|--[^\r\n]*|/\*.*?\*/|\[(?:\]\]|[^\]])*\]|[\p{L}\p{N}_@#]+|[^\s]",
            RegexOptions.Singleline);
        return string.Join("\u001f", tokens.Cast<Match>().Select(m => m.Value)
            .Where(v => !v.StartsWith("--", StringComparison.Ordinal) && !v.StartsWith("/*", StringComparison.Ordinal))
            .Select(v => v.StartsWith("'", StringComparison.Ordinal) || v.StartsWith("N'", StringComparison.Ordinal)
                ? v : v.ToUpperInvariant()));
    }

    public static string Esperado(string raiz, string nombre) =>
        Canonizar(File.ReadAllText(Path.Combine(raiz, "base de datos HotelTramonto", "Procedimientos_Almacenados.sql")), nombre);
}
