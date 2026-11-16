using HotelTramonto.VerificacionSql;

namespace HotelTramonto.Pruebas;

[TestClass]
public class HabitacionesFixtureTests
{
    private sealed record Tipo(int Id, string Nombre, decimal Precio, int Capacidad, bool Activo);
    private sealed record Habitacion(int Id, int Tipo, string Numero, string Estado, bool Activa);
    private sealed class Datos : ICatalogoHabitacionesFixture
    {
        public readonly List<Tipo> Tipos = [];
        public readonly List<Habitacion> Habitaciones = [];
        public int TiposEnColision, HabitacionesEnColision;
        public bool FallarValidacion, FallarInsercion;
        public int Validaciones;
        public int? InsertarTipo(string nombre, decimal precio, int capacidad)
        {
            if (TiposEnColision-- > 0 || Tipos.Any(t => t.Nombre == nombre)) return null;
            int id = Tipos.Select(t => t.Id).DefaultIfEmpty().Max() + 1;
            Tipos.Add(new(id, nombre, precio, capacidad, true));
            return id;
        }
        public int? InsertarHabitacion(int tipo, string numero)
        {
            if (FallarInsercion) throw new InvalidOperationException("Fallo de integridad simulado.");
            if (HabitacionesEnColision-- > 0 || Habitaciones.Any(h => h.Numero == numero)) return null;
            int id = Habitaciones.Select(h => h.Id).DefaultIfEmpty().Max() + 1;
            Habitaciones.Add(new(id, tipo, numero, "DISPONIBLE", true));
            return id;
        }
        public void Validar(CatalogoHabitacionesFixture creado)
        {
            Validaciones++;
            if (FallarValidacion) throw new InvalidOperationException("Fixture inválido.");
            var tipo = Tipos.Single(t => t.Id == creado.Tipo);
            Assert.IsTrue(tipo.Activo);
            Assert.AreEqual(100m, tipo.Precio);
            Assert.AreEqual(4, tipo.Capacidad);
            foreach (int id in creado.Habitaciones)
            {
                var h = Habitaciones.Single(h => h.Id == id);
                Assert.AreEqual(creado.Tipo, h.Tipo);
                Assert.AreEqual("DISPONIBLE", h.Estado);
                Assert.IsTrue(h.Activa);
                Assert.IsLessThanOrEqualTo(10, h.Numero.Length);
            }
        }
    }

    [TestMethod]
    public void RespaldoConSolo101_SeConservaYSeCreanTresHabitacionesPropias()
    {
        var datos = new Datos();
        var tipoAnterior = new Tipo(7, "Anterior", 300m, 2, true);
        var anterior = new Habitacion(101, 7, "101", "DISPONIBLE", true);
        datos.Tipos.Add(tipoAnterior);
        datos.Habitaciones.Add(anterior);

        var nuevo = HabitacionesFixture.Preparar(datos);

        Assert.AreEqual(3, nuevo.Habitaciones.Length);
        Assert.AreEqual(3, nuevo.Habitaciones.Distinct().Count());
        Assert.IsFalse(nuevo.Habitaciones.Contains(101));
        Assert.AreEqual(4, datos.Habitaciones.Count);
        Assert.AreEqual(anterior, datos.Habitaciones.Single(h => h.Id == 101));
        Assert.AreEqual(tipoAnterior, datos.Tipos.Single(t => t.Id == 7));
        Assert.AreEqual(1, datos.Validaciones);
    }

    [TestMethod]
    public void CatalogoVacioOTiposInactivos_NoSonDependenciasDelFixture()
    {
        foreach (bool incluirHistoricoInactivo in new[] { false, true })
        {
            var datos = new Datos();
            if (incluirHistoricoInactivo)
            {
                datos.Tipos.Add(new(1, "Inactivo", 1m, 1, false));
                datos.Habitaciones.Add(new(1, 1, "101", "MANTENIMIENTO", false));
            }
            var creado = HabitacionesFixture.Preparar(datos);
            Assert.AreEqual(3, creado.Habitaciones.Length);
            if (incluirHistoricoInactivo)
                Assert.AreEqual(new Habitacion(1, 1, "101", "MANTENIMIENTO", false), datos.Habitaciones.Single(h => h.Id == 1));
        }
    }

    [TestMethod]
    public void ColisionesSeReintentanSinReutilizarHabitacionesHistoricas()
    {
        var datos = new Datos { TiposEnColision = 1, HabitacionesEnColision = 2 };
        datos.Tipos.Add(new(5, "Anterior", 10m, 1, true));
        datos.Habitaciones.Add(new(99, 5, "F000000000", "OCUPADA", true));
        var creado = HabitacionesFixture.Preparar(datos);
        Assert.AreEqual(3, creado.Habitaciones.Length);
        Assert.IsFalse(creado.Habitaciones.Contains(99));
        Assert.AreEqual("OCUPADA", datos.Habitaciones.Single(h => h.Id == 99).Estado);
        Assert.AreEqual(1, datos.Validaciones);
    }

    [TestMethod]
    public void CadaCasoDisponeDeTiposHabitacionesYPreciosIndependientes()
    {
        var datos = new Datos();
        var a = HabitacionesFixture.Preparar(datos);
        var b = HabitacionesFixture.Preparar(datos);
        Assert.AreNotEqual(a.Tipo, b.Tipo);
        Assert.IsFalse(a.Habitaciones.Intersect(b.Habitaciones).Any());
        Assert.AreEqual(6, datos.Habitaciones.Select(h => h.Numero).Distinct().Count());
        Assert.IsTrue(datos.Tipos.All(t => t.Nombre.Length <= 50));
        Assert.IsTrue(HabitacionesFixture.EntradaTemporal(0).AddDays(2) <= HabitacionesFixture.EntradaTemporal(1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => HabitacionesFixture.EntradaTemporal(-1));
    }

    [TestMethod]
    public void ColisionesPersistentesOErroresDeIntegridad_NoSeOcultan()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            HabitacionesFixture.Preparar(new Datos { TiposEnColision = 100 }));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            HabitacionesFixture.Preparar(new Datos { HabitacionesEnColision = 100 }));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            HabitacionesFixture.Preparar(new Datos { FallarInsercion = true }));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            HabitacionesFixture.Preparar(new Datos { FallarValidacion = true }));
    }

    [TestMethod]
    public void PreparacionSoloInsertaTrasCapturarHistoricos_ConservaLos28Casos()
    {
        string raiz = OfflineChecks.Raiz();
        string ejecutor = File.ReadAllText(Path.Combine(raiz, "tests/HotelTramonto.VerificacionSql/Ejecutor.cs"));
        Assert.IsTrue(ejecutor.IndexOf("Historicos.Capturar(Copia)", StringComparison.Ordinal) <
            ejecutor.IndexOf("new CasosSql(Copia)", StringComparison.Ordinal));
        string fixture = File.ReadAllText(Path.Combine(raiz, "tests/HotelTramonto.VerificacionSql/HabitacionesFixture.cs"));
        Assert.IsFalse(fixture.Contains("UPDATE dbo.", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(fixture.Contains("DELETE ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(fixture.Contains("copia.Abrir()", StringComparison.Ordinal));
        Assert.IsTrue(fixture.Contains("OUTPUT inserted.id_habitacion", StringComparison.Ordinal));
        Assert.AreEqual(28, CasosSql.Todos.Length);
    }
}
