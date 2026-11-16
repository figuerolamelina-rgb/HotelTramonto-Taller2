using HotelTramonto.VerificacionSql;

try
{
    string raiz = OfflineChecks.Raiz();
    int verificadas = OfflineChecks.Validar(raiz);
    Console.WriteLine($"Comprobaciones sin SQL: {verificadas} aprobadas.");
    if (args.SequenceEqual(new[] { "--self-test" })) return 0;
    if (args.Length != 2 || args[0] != "--backup" && args[0] != "--backup-ciclo")
    {
        Console.Error.WriteLine("Uso: dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup \"C:\\Respaldos\\HotelTramontoDB_Pruebas.bak\"");
        Console.Error.WriteLine("No se ejecutó SQL. --self-test solo realiza comprobaciones locales.");
        return 2;
    }
    return new Ejecutor(raiz, Path.GetFullPath(args[1]), args[0] == "--backup-ciclo").Ejecutar();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
    return 1;
}
