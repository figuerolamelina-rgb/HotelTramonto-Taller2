
namespace HotelTramonto.Entidades
{
    public class Reserva
    {
        public int IdReserva { get; set; }

        public int IdHuesped { get; set; }

        public int? IdUsuario { get; set; }

        public DateTime FechaEntrada { get; set; }

        public DateTime FechaSalida { get; set; }

        public string Estado { get; set; } = "CONFIRMADA";

        public decimal Total { get; set; }

        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaCheckIn { get; set; }

        public DateTime? FechaCheckOut { get; set; }
    }
}
