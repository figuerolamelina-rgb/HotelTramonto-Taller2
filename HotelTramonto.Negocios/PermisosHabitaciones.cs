namespace HotelTramonto.Negocios;

// Política de la aplicación. El texto del rol NO autentica al empleado ante SQL Server.
public static class PermisosHabitaciones
{
    public static bool PuedeAdministrar(string? rolUsuario) =>
        string.Equals(rolUsuario?.Trim(), "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase);

    public static void ExigirAdministrador(string? rolUsuario)
    {
        if (!PuedeAdministrar(rolUsuario))
            throw new UnauthorizedAccessException("Solo el administrador puede gestionar habitaciones y sus tipos.");
    }
}
