
namespace HotelTramonto.Entidades
{
    public class Pago
    {
        public int IdPago { get; set; }

        public int? IdReserva { get; set; }

        public int? IdReservaTemporal { get; set; }

        public int IdUsuario { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string Estado { get; set; } = "PENDIENTE";

        public string TipoPago { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }
    }
}
