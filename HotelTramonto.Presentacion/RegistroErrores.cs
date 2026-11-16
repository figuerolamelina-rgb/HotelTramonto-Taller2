namespace HotelTramonto.Presentacion;

internal static class RegistroErrores
{
    public static string Registrar(Exception error)
    {
        string referencia = Guid.NewGuid().ToString("N");
        try
        {
            string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HotelTramonto", "Diagnosticos");
            Directory.CreateDirectory(carpeta);
            // Lista permitida: sin mensaje, stack, nombres, rutas ni cadenas de conexión.
            string codigo = error is Microsoft.Data.SqlClient.SqlException sql ? $"SQL:{sql.Number}" : "CLIENTE";
            File.AppendAllText(Path.Combine(carpeta, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log"),
                $"{DateTime.UtcNow:O} {referencia} {codigo}{Environment.NewLine}");
        }
        catch { /* Un fallo del diagnóstico no altera el resultado de la operación. */ }
        return referencia;
    }

    public static string Mensaje(Exception error)
    {
        if (error is Microsoft.Data.SqlClient.SqlException sql)
        {
            string? mensaje = sql.Number switch
            {
                50027 => "La reserva debe estar confirmada y sin cierre para registrar el check-in.",
                50028 or 50031 => "Existe saldo de alojamiento pendiente. Registrá el pago antes de continuar.",
                50030 => "La reserva debe tener check-in para registrar el check-out.",
                50032 => "Existen servicios pendientes de pago. No se realizó el check-out.",
                50041 => "La reserva ya está cancelada.",
                50042 => "Solo se pueden cancelar reservas confirmadas y sin cierre.",
                _ => null
            };
            if (mensaje != null) return mensaje;
        }
        return "No se pudo completar la consulta u operación. Revisá la conexión y los permisos. Referencia: " + Registrar(error);
    }
}
