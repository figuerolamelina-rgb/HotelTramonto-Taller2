namespace HotelTramonto.Negocios;

// Barrera funcional del cliente. No sustituye la identidad/autorización de SQL Server.
public static class PermisosOperacion
{
    public static bool PuedeOperar(string? rol) =>
        rol?.Trim().ToUpperInvariant() is "ADMINISTRADOR" or "RECEPCIONISTA";

    public static void ExigirReportes(string? rol)
    {
        if (rol?.Trim().ToUpperInvariant() is not ("ADMINISTRADOR" or "GERENTE"))
            throw new UnauthorizedAccessException("No tenés permisos para consultar reportes.");
    }

    public static void Exigir(string? rol)
    {
        if (!PuedeOperar(rol))
            throw new UnauthorizedAccessException("No tenés permisos para gestionar reservas, consumos o pagos.");
    }
}
