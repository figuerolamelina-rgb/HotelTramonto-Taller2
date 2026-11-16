using System.Data;
using System.Security.Cryptography;
using System.Text.Json;

namespace HotelTramonto.VerificacionSql;

public sealed class Historicos
{
    private sealed record TablaInicial(string Nombre, string Clave, string[] Columnas, Dictionary<string, string> Filas);
    private readonly List<TablaInicial> tablas = [];
    private static readonly Dictionary<string, string> claves = new()
    {
        ["Roles"]="id_rol", ["Usuarios"]="id_usuario", ["Huespedes"]="id_huesped",
        ["TiposHabitacion"]="id_tipoHabitacion", ["Habitaciones"]="id_habitacion",
        ["Servicios"]="id_servicio", ["Reservas"]="id_reserva", ["DetalleReservas"]="id_detalle_reserva",
        ["ReservasTemporales"]="id_reserva_temporal", ["DetalleServicios"]="id_detalle_servicio", ["Pagos"]="id_pago"
    };
    private static string Huella(DataRow row)
    {
        object?[] values = row.ItemArray.Select(v => v is DBNull ? null : v).ToArray();
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(values)));
    }
    public static Historicos Capturar(CopiaSql copia)
    {
        var snapshot = new Historicos();
        using var db = copia.Abrir();
        foreach (var (nombre, clave) in claves)
        {
            var tabla = Sql.Tabla(db, "SELECT * FROM dbo." + IdentidadCopia.Identificador(nombre));
            snapshot.tablas.Add(new(nombre, clave, tabla.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray(),
                tabla.Rows.Cast<DataRow>().ToDictionary(r => r[clave].ToString()!, Huella)));
        }
        return snapshot;
    }
    public void Verificar(CopiaSql copia)
    {
        using var db = copia.Abrir();
        foreach (var original in tablas)
        {
            var actual = Sql.Tabla(db, "SELECT " + string.Join(",", original.Columnas.Select(IdentidadCopia.Identificador)) +
                " FROM dbo." + IdentidadCopia.Identificador(original.Nombre));
            var rows = actual.Rows.Cast<DataRow>().ToDictionary(r => r[original.Clave].ToString()!, Huella);
            foreach (var (id, hash) in original.Filas)
                if (!rows.TryGetValue(id, out var ahora) || hash != ahora)
                    throw new InvalidOperationException("Histórico modificado o eliminado en " + original.Nombre);
        }
    }
}
