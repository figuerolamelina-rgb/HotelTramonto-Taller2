
namespace HotelTramonto.Entidades
{
    public class DetalleServicio
    {
        public int IdDetalleServicio { get; set; }

        public int IdServicio { get; set; }

        public int IdReserva { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
