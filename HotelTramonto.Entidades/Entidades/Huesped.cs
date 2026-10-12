
namespace HotelTramonto.Entidades
{
    public class Huesped
    {
        public int IdHuesped { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string DNI { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string Nacionalidad { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
