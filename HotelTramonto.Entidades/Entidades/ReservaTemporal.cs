
namespace HotelTramonto.Entidades
{
    public class ReservaTemporal
    {
        public int IdReservaTemporal { get; set; }

        public int IdHuesped { get; set; }

        public int IdHabitacion { get; set; }

        public int? IdReserva { get; set; }

        public string Estado { get; set; } = "PENDIENTE";

        public string? ModalidadPago { get; set; }

        public DateTime FechaEntrada { get; set; }

        public DateTime FechaSalida { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }
    }
}
