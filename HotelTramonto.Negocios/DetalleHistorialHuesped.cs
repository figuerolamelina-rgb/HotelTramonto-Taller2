using System.Data;

namespace HotelTramonto.Negocios;

public static class DetalleHistorialHuesped
{
    // Conserva una fila por reserva/habitación y añade el total/observaciones de Iara,
    // sin agrupar ni multiplicar importes ni modificar la tabla recibida.
    public static DataTable Completar(DataTable historial, DataTable reservas)
    {
        var resultado = historial.Copy();
        resultado.Columns.Add("total", typeof(decimal));
        resultado.Columns.Add("observaciones", typeof(string));
        var porId = reservas.AsEnumerable().ToDictionary(f => Convert.ToInt32(f["id_reserva"]));
        foreach (DataRow fila in resultado.Rows)
        {
            if (!porId.TryGetValue(Convert.ToInt32(fila["id_reserva"]), out var reserva)) continue;
            fila["total"] = reserva["total"];
            fila["observaciones"] = reserva["observaciones"];
        }
        return resultado;
    }
}
