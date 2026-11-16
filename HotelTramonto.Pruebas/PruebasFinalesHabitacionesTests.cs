using System.Data;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class PruebasFinalesHabitacionesTests
{
    [TestMethod]
    [DataRow("RECEPCIONISTA")]
    [DataRow("GERENTE")]
    [DataRow(" recepcionista ")]
    [DataRow(" gerente ")]
    [DataRow("ADMINISTRADORX")]
    [DataRow("")]
    [DataRow(null)]
    public void SinInterfaz_RolNoAdministradorSeRechazaAntesDeValidarOAccederADatos(string? rol)
    {
        var datos = new DatosEspia();
        var negocio = new HabitacionNegocios(datos);
        var tipos = new TipoHabitacionNegocios(datos);
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Insertar(0, -1, "", "", rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Modificar(0, 0, -1, "", "", rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Desactivar(0, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => negocio.Reactivar(0, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => tipos.Insertar("", null, 0, 0, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => tipos.Modificar(0, "", null, 0, 0, rol!));
        Assert.Throws<UnauthorizedAccessException>(() => tipos.Desactivar(0, rol!));
        Assert.AreEqual(0, datos.Escrituras);
    }

    [TestMethod]
    [DataRow("RECEPCIONISTA")]
    [DataRow("GERENTE")]
    [DataRow("ADMINISTRADOR")]
    public void RolConservaConsultasAunqueNoPuedaAdministrar(string rol)
    {
        var datos = new DatosEspia();
        var negocio = new HabitacionNegocios(datos);
        if (rol != "ADMINISTRADOR")
            Assert.Throws<UnauthorizedAccessException>(() => negocio.Desactivar(1, rol));
        Assert.AreSame(datos.Resultado, negocio.Listar());
        Assert.AreSame(datos.Resultado, negocio.BuscarPorId(1));
        Assert.AreSame(datos.Resultado, negocio.ListarInactivas());
        Assert.AreSame(datos.Resultado, negocio.ConsultarDisponibilidad(new DateTime(2099, 1, 1), new DateTime(2099, 1, 2)));
        Assert.AreSame(datos.Resultado, new TipoHabitacionNegocios(datos).Listar());
        Assert.AreEqual(5, datos.Lecturas);
        Assert.AreEqual(0, datos.Escrituras);
    }

    [TestMethod]
    [DataRow("ADMINISTRADOR")]
    [DataRow(" administrador ")]
    public void AdministradorLlegaADatosYNoSeOcultanSusErrores(string rol)
    {
        var datos = new DatosEspia();
        var negocio = new HabitacionNegocios(datos);
        var tipos = new TipoHabitacionNegocios(datos);
        Assert.Throws<InvalidOperationException>(() => negocio.Insertar(1, 0, "101", "DISPONIBLE", rol));
        Assert.Throws<InvalidOperationException>(() => negocio.Modificar(1, 1, 0, "101", "MANTENIMIENTO", rol));
        Assert.Throws<InvalidOperationException>(() => negocio.Desactivar(1, rol));
        Assert.Throws<InvalidOperationException>(() => negocio.Reactivar(1, rol));
        Assert.Throws<InvalidOperationException>(() => tipos.Insertar("Doble", null, 100, 2, rol));
        Assert.Throws<InvalidOperationException>(() => tipos.Modificar(1, "Doble", null, 100, 2, rol));
        Assert.Throws<InvalidOperationException>(() => tipos.Desactivar(1, rol));
        Assert.AreEqual(7, datos.Escrituras);
    }

    private sealed class DatosEspia : IHabitacionDatos, ITipoHabitacionDatos
    {
        public int Escrituras { get; private set; }
        public int Lecturas { get; private set; }
        public DataTable Resultado { get; } = CrearTabla();
        private static DataTable CrearTabla()
        {
            var t = new DataTable(); t.Columns.Add("id_habitacion", typeof(int)); t.Rows.Add(1); return t;
        }
        private void Escribir() { Escrituras++; throw new InvalidOperationException("Fallo simulado de Datos, sin SQL."); }
        private DataTable Leer() { Lecturas++; return Resultado; }
        public void Insertar(int tipoHabitacion, int? piso, string numero, string estado) => Escribir();
        public void Modificar(int idHabitacion, int tipoHabitacion, int? piso, string numero, string estado) => Escribir();
        public void Desactivar(int idHabitacion) => Escribir();
        public void Reactivar(int idHabitacion) => Escribir();
        public DataTable Listar() => Leer();
        public DataTable BuscarPorId(int idHabitacion) => Leer();
        public DataTable ListarInactivas() => Leer();
        public DataTable ConsultarDisponibilidad(DateTime fechaEntrada, DateTime fechaSalida) => Leer();
        public void Insertar(string nombre, string? descripcion, decimal precioNoche, int capacidad) => Escribir();
        public void Modificar(int idTipoHabitacion, string nombre, string? descripcion, decimal precioNoche, int capacidad) => Escribir();
    }
}
