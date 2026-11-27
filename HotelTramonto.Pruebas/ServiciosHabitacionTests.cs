using System.Data;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class ServiciosHabitacionTests
{
    private sealed class Datos : IDetalleServicioDatos
    {
        public readonly List<(int servicio, int reserva, int cantidad, decimal precio, int habitacion, Guid operacion)> Solicitudes = [];
        public int? Filtro;
        public DataTable Tabla = new();
        public void Insertar(int servicio, int reserva, int cantidad, decimal precio, int habitacion, Guid operacion)
            => Solicitudes.Add((servicio, reserva, cantidad, precio, habitacion, operacion));
        public DataTable ListarPorReserva(int reserva, int? habitacion = null)
        {
            Filtro = habitacion;
            return Tabla;
        }
    }

    [TestMethod]
    [DataRow(0, 1, 1, 1)]
    [DataRow(1, 0, 1, 1)]
    [DataRow(1, 1, 0, 1)]
    [DataRow(1, 1, 1, 0)]
    [DataRow(1, 1, -1, 1)]
    public void IdentificadoresYCantidadInvalidos_NoLleganADatos(int servicio, int reserva, int cantidad, int habitacion)
    {
        var datos = new Datos();
        Assert.ThrowsExactly<ArgumentException>(() =>
            new DetalleServicioNegocios(datos, "RECEPCIONISTA").Insertar(servicio, reserva, cantidad, 20m, habitacion, Guid.NewGuid()));
        Assert.AreEqual(0, datos.Solicitudes.Count);
    }

    [TestMethod]
    public void ImportesInvalidos_NoLleganADatos()
    {
        foreach (decimal precio in new[] { 0m, -1m, 1.001m, 100000000m })
        {
            var datos = new Datos();
            Assert.ThrowsExactly<ArgumentException>(() =>
                new DetalleServicioNegocios(datos, "RECEPCIONISTA").Insertar(1, 1, 1, precio, 1, Guid.NewGuid()));
            Assert.AreEqual(0, datos.Solicitudes.Count);
        }
        Assert.ThrowsExactly<ArgumentException>(() =>
            new DetalleServicioNegocios(new Datos(), "RECEPCIONISTA").Insertar(1, 1, 2, 99999999.99m, 1, Guid.NewGuid()));
    }

    [TestMethod]
    public void OperacionVacia_SeRechaza()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            new DetalleServicioNegocios(new Datos(), "RECEPCIONISTA").Insertar(1, 1, 1, 20m, 1, Guid.Empty));
    }

    [TestMethod]
    public void Reintento_ConservaOperacionYDatosParaValidacionSQL()
    {
        var datos = new Datos();
        var negocio = new DetalleServicioNegocios(datos, "RECEPCIONISTA");
        Guid operacion = Guid.NewGuid();
        negocio.Insertar(2, 3, 4, 25.50m, 5, operacion);
        negocio.Insertar(2, 3, 4, 25.50m, 5, operacion);
        Assert.AreEqual(datos.Solicitudes[0], datos.Solicitudes[1]);
        Assert.AreEqual((2, 3, 4, 25.50m, 5, operacion), datos.Solicitudes[0]);
        // La deduplicación es responsabilidad SQL, no se simula aquí.
    }

    [TestMethod]
    public void ConsultaHistorica_ConservaNullYPrecioSinRecalcular()
    {
        var datos = new Datos();
        datos.Tabla.Columns.Add("id_habitacion", typeof(int));
        datos.Tabla.Columns.Add("precio_unitario", typeof(decimal));
        datos.Tabla.Rows.Add(DBNull.Value, 12.50m);
        var negocio = new DetalleServicioNegocios(datos, "RECEPCIONISTA");
        var tabla = negocio.ListarPorReserva(1);
        Assert.IsNull(datos.Filtro);
        Assert.IsTrue(tabla.Rows[0].IsNull("id_habitacion"));
        Assert.AreEqual(12.50m, tabla.Rows[0]["precio_unitario"]);
        negocio.ListarPorReserva(1, 7);
        Assert.AreEqual(7, datos.Filtro);
        Assert.ThrowsExactly<ArgumentException>(() => negocio.ListarPorReserva(1, 0));
    }
}
