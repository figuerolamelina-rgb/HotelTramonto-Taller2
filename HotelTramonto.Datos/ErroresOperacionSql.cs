using HotelTramonto.Entidades;

namespace HotelTramonto.Datos;

public static class ErroresOperacionSql
{
    // Solo errores que los procedimientos garantizan rechazar sin escritura.
    public static EstadoResultadoOperacion Clasificar(int numero) =>
        numero is >= 51313 and <= 51318 or 51402 or 51404 or >= 51410 and <= 51420
            ? EstadoResultadoOperacion.RechazoDefinitivo : EstadoResultadoOperacion.ResultadoIncierto;

    public static string Mensaje(int numero) => numero switch
    {
        51315 or 51410 => "La reserva debe tener check-in y no estar cerrada para registrar servicios.",
        51316 => "La habitación no pertenece a esta reserva.",
        51317 => "El servicio está inactivo o cambió de precio. Actualizá el catálogo.",
        51318 => "El importe del consumo supera el máximo permitido.",
        51404 => "El usuario responsable no está habilitado.",
        51411 => "El saldo de alojamiento se paga con la reserva confirmada y antes del check-in.",
        51413 => "La reserva no tiene saldo pendiente para este pago.",
        51414 => "El importe supera el saldo actualizado. Consultá nuevamente la reserva.",
        51416 => "La reserva temporal ya no está pendiente o está vencida.",
        51419 => "El pago inicial debe corresponder al 50 % o al 100 % del alojamiento.",
        51420 => "La reserva temporal ya tiene un pago inicial aprobado.",
        _ => "La solicitud fue rechazada sin registrar cambios. Revisá sus datos."
    };
}
