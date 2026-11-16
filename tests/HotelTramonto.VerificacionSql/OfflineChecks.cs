using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HotelTramonto.VerificacionSql;

public static class OfflineChecks
{
    public static string Raiz()
    {
        for (DirectoryInfo? d = new(AppContext.BaseDirectory); d != null; d = d.Parent)
            if (File.Exists(Path.Combine(d.FullName, "HotelTramonto.slnx"))) return d.FullName;
        throw new InvalidOperationException("Ejecutar desde un checkout completo de Hotel Tramonto.");
    }
    public static int Validar(string raiz)
    {
        int comprobaciones = 0;
        void Exigir(bool valor, string detalle)
        {
            if (!valor) throw new InvalidOperationException("Comprobación estática: " + detalle);
            comprobaciones++;
        }
        void Rechaza(Action accion)
        {
            try { accion(); }
            catch (InvalidOperationException) { comprobaciones++; return; }
            throw new InvalidOperationException("La barrera aceptó un caso inválido.");
        }
        var copia = new IdentidadCopia(Guid.NewGuid());
        Exigir(copia.EsPropia(copia.Nombre, copia.Ejecucion.ToString("D")), "Identidad válida.");
        foreach (string nombre in new[] { "HotelTramontoDB", "HotelTramontoDB_Pruebas", "master", copia.Nombre + "_otra" })
            Exigir(!copia.EsPropia(nombre, copia.Ejecucion.ToString("D")), "Rechazar base protegida/ajena.");
        Exigir(!copia.EsPropia(copia.Nombre, Guid.NewGuid().ToString("D")), "Marca ajena.");
        Exigir(!copia.EsPropia(copia.Nombre, null), "Falta marca.");
        Rechaza(() => IdentidadCopia.ExigirNueva(Guid.Empty));
        string directorio = Path.Combine(raiz, "base de datos HotelTramonto", "servicios-habitaciones");
        foreach (string archivo in Directory.GetFiles(directorio, "*.sql"))
        {
            string original = File.ReadAllText(archivo);
            string adaptado = copia.Adaptar(original);
            Exigir(original.Contains(IdentidadCopia.GuardaOriginal, StringComparison.Ordinal), "Guarda original " + archivo);
            Exigir(adaptado.Contains("@HT_Run", StringComparison.Ordinal) &&
                adaptado.Contains("HotelTramontoDB_HT56_", StringComparison.Ordinal) &&
                adaptado.Contains("sys.extended_properties", StringComparison.Ordinal), "Guarda temporal " + archivo);
        }
        Rechaza(() => copia.Adaptar("SELECT 1;"));
        string prueba = IdentidadCopia.GuardaOriginal + "\nTHROW 51400,'No',1;";
        Rechaza(() => copia.Adaptar(prueba + "\nGO\nSELECT 1"));
        Rechaza(() => copia.Adaptar(prueba + "\nUSE HotelTramontoDB;"));
        Rechaza(() => copia.Adaptar(prueba + "\n" + prueba));
        foreach (string nombre in ContratosBase.Procedimientos)
            Exigir(ContratosBase.Esperado(raiz, nombre).Contains("PROCEDURE", StringComparison.Ordinal),
                "Contrato de procedimiento base " + nombre);
        Exigir(CasosSql.Todos.Length == 28 && CasosSql.Todos.Select(c => c.Nombre).Distinct().Count() == 28,
            "Exactamente 28 pruebas distintas.");
        string proyecto = Path.Combine(raiz, "tests", "HotelTramonto.VerificacionSql", "HotelTramonto.VerificacionSql.csproj");
        var xml = XDocument.Load(proyecto);
        Exigir(xml.Descendants("TargetFramework").Single().Value == "net10.0", ".NET 10.");
        Exigir(!xml.Descendants("PackageReference").Any(), "Sin paquetes nuevos: SqlClient ya proviene de Datos.");
        Exigir(xml.Descendants("ProjectReference").Single().Attribute("Include")!.Value.EndsWith("HotelTramonto.Datos.csproj", StringComparison.Ordinal),
            "Reutilización de SqlClient.");
        return comprobaciones;
    }
}
