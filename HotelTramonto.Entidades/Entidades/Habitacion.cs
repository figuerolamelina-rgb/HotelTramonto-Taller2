
namespace HotelTramonto.Entidades
{
    public class Habitacion
    {
        public int IdHabitacion { get; set; }

        public int TipoHabitacion { get; set; }

        public int? Piso { get; set; }

        public string Numero { get; set; } = string.Empty;

        public string Estado { get; set; } = "DISPONIBLE";

        public bool Activo { get; set; }
    }
}
