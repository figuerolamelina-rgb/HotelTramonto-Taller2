using System.Data;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class PermisosHabitacionesTests
{
    [TestMethod]
    [DataRow("RECEPCIONISTA")]
    [DataRow("GERENTE")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("OTRO")]
    [DataRow(null)]
    public void NoAdministradores_NoLleganADatosEnNingunaOperacion(string? rol)
    {
        var datos = new HabitacionesSimuladas();
        var tipos = new TiposSimulados();
        var negocio = new HabitacionNegocios(datos);
        var negocioTipos = new TipoHabitacionNegocios(tipos);
        Assert.IsFalse(PermisosHabitaciones.PuedeAdministrar(rol));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Insertar(1, 0, "101", "DISPONIBLE", rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Modificar(1, 1, 0, "101", "MANTENIMIENTO", rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Desactivar(1, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Reactivar(1, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocioTipos.Insertar("Doble", null, 100, 2, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocioTipos.Modificar(1, "Doble", null, 100, 2, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocioTipos.Desactivar(1, rol!));
        Assert.AreEqual(0, datos.Llamadas.Count);
        Assert.AreEqual(0, tipos.Llamadas.Count);
    }

    [TestMethod]
    [DataRow("ADMINISTRADOR")]
    [DataRow(" administrador ")]
    public void Administrador_ConservaLasCuatroOperacionesYLosParametros(string rol)
    {
        var datos = new HabitacionesSimuladas();
        var negocio = new HabitacionNegocios(datos);
        negocio.Insertar(2, null, " 101 ", " DISPONIBLE ", rol);
        negocio.Modificar(7, 2, 1, " 102 ", " MANTENIMIENTO ", rol);
        negocio.Desactivar(7, rol);
        negocio.Reactivar(7, rol);
        CollectionAssert.AreEqual(new[] { "insertar:2::101:DISPONIBLE", "modificar:7:2:1:102:MANTENIMIENTO",
            "desactivar:7", "reactivar:7" }, datos.Llamadas);
    }

    [TestMethod]
    public void Administrador_PuedeAdministrarTiposSinOmitirValidaciones()
    {
        var datos = new TiposSimulados();
        var negocio = new TipoHabitacionNegocios(datos);
        negocio.Insertar(" Doble ", null, 100, 2, "ADMINISTRADOR");
        negocio.Modificar(3, " Doble ", "Detalle", 200, 2, "ADMINISTRADOR");
        negocio.Desactivar(3, "ADMINISTRADOR");
        CollectionAssert.AreEqual(new[] { "insertar:Doble:100:2", "modificar:3:Doble:200:2", "desactivar:3" }, datos.Llamadas);
        Assert.Throws<ArgumentException>(() => negocio.Insertar("", null, 100, 2, "ADMINISTRADOR"));
        Assert.Throws<ArgumentException>(() => negocio.Modificar(0, "Doble", null, 100, 2, "ADMINISTRADOR"));
        Assert.Throws<ArgumentException>(() => negocio.Desactivar(0, "ADMINISTRADOR"));
        Assert.AreEqual(3, datos.Llamadas.Count);
    }

    [TestMethod]
    public void Administrador_HabitacionesConservaValidacionDeDatos()
    {
        var datos = new HabitacionesSimuladas();
        var negocio = new HabitacionNegocios(datos);
        Assert.Throws<ArgumentException>(() => negocio.Insertar(0, 0, "101", "DISPONIBLE", "ADMINISTRADOR"));
        Assert.Throws<ArgumentException>(() => negocio.Modificar(0, 1, 0, "101", "DISPONIBLE", "ADMINISTRADOR"));
        Assert.Throws<ArgumentException>(() => negocio.Desactivar(0, "ADMINISTRADOR"));
        Assert.Throws<ArgumentException>(() => negocio.Reactivar(0, "ADMINISTRADOR"));
        Assert.AreEqual(0, datos.Llamadas.Count);
    }

    [TestMethod]
    public void Consultas_NoRequierenPermisoAdministrativoYConservanResultados()
    {
        var datos = new HabitacionesSimuladas();
        var negocio = new HabitacionNegocios(datos);
        var tipos = new TiposSimulados();
        Assert.AreSame(datos.Resultado, negocio.Listar());
        Assert.AreSame(datos.Resultado, negocio.BuscarPorId(1));
        Assert.AreSame(datos.Resultado, negocio.ListarInactivas());
        Assert.AreSame(tipos.Resultado, new TipoHabitacionNegocios(tipos).Listar());
        var entrada = new DateTime(2026, 12, 1, 15, 0, 0);
        var salida = entrada.AddDays(2);
        Assert.AreSame(datos.Resultado, negocio.ConsultarDisponibilidad(entrada, salida));
        Assert.AreEqual((entrada.Date, salida.Date), datos.Fechas);
        Assert.AreEqual(0, datos.Llamadas.Count);
        Assert.Throws<ArgumentException>(() => negocio.ConsultarDisponibilidad(salida, entrada));
    }

    private sealed class HabitacionesSimuladas : IHabitacionDatos
    {
        public List<string> Llamadas { get; } = new();
        public DataTable Resultado { get; } = CrearResultado();
        public (DateTime, DateTime) Fechas { get; private set; }
        private static DataTable CrearResultado()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("id_habitacion", typeof(int));
            tabla.Rows.Add(1);
            return tabla;
        }
        public void Insertar(int tipoHabitacion, int? piso, string numero, string estado) =>
            Llamadas.Add($"insertar:{tipoHabitacion}:{piso}:{numero}:{estado}");
        public void Modificar(int idHabitacion, int tipoHabitacion, int? piso, string numero, string estado) =>
            Llamadas.Add($"modificar:{idHabitacion}:{tipoHabitacion}:{piso}:{numero}:{estado}");
        public void Desactivar(int idHabitacion) => Llamadas.Add($"desactivar:{idHabitacion}");
        public void Reactivar(int idHabitacion) => Llamadas.Add($"reactivar:{idHabitacion}");
        public DataTable ListarConReservaCheckIn() => Resultado;
        public DataTable Listar() => Resultado;
        public DataTable BuscarPorId(int idHabitacion) => Resultado;
        public DataTable ListarInactivas() => Resultado;
        public DataTable ConsultarDisponibilidad(DateTime entrada, DateTime salida)
        {
            Fechas = (entrada, salida);
            return Resultado;
        }
    }

    private sealed class TiposSimulados : ITipoHabitacionDatos
    {
        public List<string> Llamadas { get; } = new();
        public DataTable Resultado { get; } = new();
        public DataTable ListarConReservaCheckIn() => Resultado;
        public DataTable Listar() => Resultado;
        public void Insertar(string nombre, string? descripcion, decimal precioNoche, int capacidad) =>
            Llamadas.Add($"insertar:{nombre}:{precioNoche}:{capacidad}");
        public void Modificar(int idTipoHabitacion, string nombre, string? descripcion, decimal precioNoche, int capacidad) =>
            Llamadas.Add($"modificar:{idTipoHabitacion}:{nombre}:{precioNoche}:{capacidad}");
        public void Desactivar(int idTipoHabitacion) => Llamadas.Add($"desactivar:{idTipoHabitacion}");
    }
}
