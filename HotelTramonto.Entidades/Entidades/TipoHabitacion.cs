
namespace HotelTramonto.Entidades
{
    public class TipoHabitacion
    {
        public int IdTipoHabitacion { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal PrecioNoche { get; set; }

        public int Capacidad { get; set; }

        public bool Activo { get; set; }
    }
}
