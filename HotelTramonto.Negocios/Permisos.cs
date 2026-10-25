
using System;

namespace HotelTramonto.Negocios
{
    public static class Permisos
    {
        public static void ExigirAdministrador(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol) ||
                !string.Equals(
                    rol.Trim(),
                    "ADMINISTRADOR",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "No tiene permisos para gestionar usuarios.");
            }
        }
    }
}
