using System.Data;

namespace HotelTramonto.Negocios;

public static class ComprobantePagoNegocios
{
    public static void Validar(DataRow pago, int? idEsperado = null)
    {
        ArgumentNullException.ThrowIfNull(pago);
        foreach (string columna in new[] { "id_pago", "id_reserva", "monto", "estado", "fecha_pago",
                     "metodo_pago", "tipo_pago", "nombre_huesped", "apellido_huesped", "dni",
                     "nombre_usuario", "apellido_usuario" })
            if (!pago.Table.Columns.Contains(columna) || pago.IsNull(columna))
                throw new InvalidOperationException("El comprobante no tiene todos los datos requeridos.");
        int id = Convert.ToInt32(pago["id_pago"]);
        decimal monto = Convert.ToDecimal(pago["monto"]);
        if (id <= 0 || (idEsperado.HasValue && id != idEsperado.Value) ||
            Convert.ToInt32(pago["id_reserva"]) <= 0 ||
            !string.Equals(Convert.ToString(pago["estado"]), "APROBADO", StringComparison.Ordinal) ||
            monto <= 0 || monto > 99999999.99m || decimal.Round(monto, 2) != monto)
            throw new InvalidOperationException("El comprobante debe corresponder al pago aprobado solicitado.");
    }
}
