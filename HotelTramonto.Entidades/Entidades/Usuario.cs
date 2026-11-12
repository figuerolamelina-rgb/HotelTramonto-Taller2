
namespace HotelTramonto.Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string DNI { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Contrasena { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
