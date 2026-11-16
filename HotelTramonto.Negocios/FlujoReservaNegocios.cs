using System.Data;
using HotelTramonto.Datos;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Negocios;

public sealed class FlujoReservaNegocios
{
    public static decimal CalcularPagoInicial(decimal total, string modalidad)
    {
        if (total <= 0 || total > 99999999.99m || decimal.Round(total, 2) != total)
            throw new ArgumentException("El total de alojamiento no es válido.");
        return modalidad switch
        {
            "PAGO_TOTAL" => total,
            "SENA_50" => decimal.Round(total * 0.50m, 2, MidpointRounding.AwayFromZero),
            _ => throw new ArgumentException("Modalidad inválida.")
        };
    }

    public DataTable Ejecutar(SolicitudReserva solicitud, DiarioOperaciones.Contexto contexto)
    {
        if (contexto.Referencia == null)
        {
            if (contexto.CreacionIniciada)
                throw new InvalidOperationException("El resultado del alta temporal es incierto. Requiere conciliación antes de continuar.");
            contexto.MarcarCreacion();
            int temporal = new ReservaTemporalNegocios().Insertar(solicitud.IdHuesped, solicitud.IdHabitacion,
                solicitud.Entrada, solicitud.Salida, solicitud.Modalidad);
            contexto.GuardarReferencia(temporal); // Antes del pago, incluso frente a un cierre posterior.
        }
        int idTemporal = contexto.Referencia!.Value;
        var datos = new ReservaTemporalDatos();
        var confirmada = datos.ConsultarConfirmacion(idTemporal);
        if (confirmada.Rows.Count > 0) return confirmada;

        new PagoNegocios().Insertar(null, idTemporal, solicitud.IdUsuario, solicitud.PagoInicial,
            solicitud.Metodo, "APROBADO", "PAGO_INICIAL", contexto.Id);
        try { return new ReservaNegocios().ConfirmarReserva(idTemporal, solicitud.Personas); }
        catch (SqlException ex) when (ex.Number == 50002)
        {
            // Otro reintento pudo confirmar después de la consulta anterior.
            confirmada = datos.ConsultarConfirmacion(idTemporal);
            if (confirmada.Rows.Count == 0) throw;
            return confirmada;
        }
    }
}
