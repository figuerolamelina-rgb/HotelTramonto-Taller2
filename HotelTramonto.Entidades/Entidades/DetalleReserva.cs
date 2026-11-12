
namespace HotelTramonto.Entidades
{
    public class DetalleReserva
    {
        public int IdDetalleReserva { get; set; }

        public int IdReserva { get; set; }

        public int IdHabitacion { get; set; }

        public decimal PrecioNoche { get; set; }

        public int CantidadPersonas { get; set; }
    }
}
