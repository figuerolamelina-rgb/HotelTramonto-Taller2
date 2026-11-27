using System.Data;

namespace HotelTramonto.Negocios;

public static class FiltrosHabitaciones
{
    public static DataTable Aplicar(DataTable origen, string estado, string tipo, string piso, bool soloActivas)
    {
        var resultado = origen.Clone();
        foreach (DataRow fila in origen.Rows)
        {
            bool activa = Convert.ToBoolean(fila["activo"]);
            string estadoFila = Convert.ToString(fila["estado"])?.Trim() ?? "";
            string tipoFila = Convert.ToString(fila["tipo"])?.Trim() ?? "";
            string pisoFila = Convert.ToString(fila["piso"])?.Trim() ?? "";
            if (soloActivas && !activa) continue;
            if (estado == "Disponibles" && (!activa || estadoFila != "DISPONIBLE")) continue;
            if (estado == "Ocupadas" && estadoFila != "OCUPADA") continue;
            if (estado == "Mantenimiento" && estadoFila != "MANTENIMIENTO") continue;
            if (tipo != "Todos" && !tipoFila.Equals(tipo, StringComparison.OrdinalIgnoreCase)) continue;
            if (piso != "Todos" && pisoFila != piso) continue;
            resultado.ImportRow(fila);
        }
        return resultado;
    }
}
