
namespace HotelTramonto.Entidades
{
    public class Rol
    {
        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}
