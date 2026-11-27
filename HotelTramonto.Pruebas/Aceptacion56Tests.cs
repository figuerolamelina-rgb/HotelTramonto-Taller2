using HotelTramonto.Datos;
using HotelTramonto.Entidades;
using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class Aceptacion56Tests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("GERENTE")]
    [DataRow("ADMINISTRADORX")]
    public void RolesNoOperativos_RechazanLlamadasDirectasAntesDeSql(string? rol)
    {
        var pagos = new PagoNegocios(rol);
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => pagos.RegistrarServicios(1, 1, "EFECTIVO", Guid.NewGuid()));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => pagos.RegistrarSaldoAlojamiento(1, 1, "EFECTIVO", Guid.NewGuid()));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => pagos.Insertar(1, null, 1, 10, "EFECTIVO", "APROBADO", "SERVICIOS_ADICIONALES", Guid.NewGuid()));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => pagos.ActualizarEstado(1, "APROBADO"));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => pagos.ListarPorReserva(1));
        var reservas = new ReservaNegocios(rol);
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => reservas.CheckIn(1));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => reservas.CheckOut(1));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => reservas.Cancelar(1));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => reservas.ConfirmarReserva(1, 1));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => reservas.Listar());
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new ReservaTemporalNegocios(rol).ExpirarVencidas());
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new ReservaTemporalNegocios(rol).Insertar(1, 1, DateTime.Today, DateTime.Today.AddDays(1), "PAGO_TOTAL"));
        var consumos = new DetalleServicioNegocios(rol);
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => consumos.Insertar(1, 1, 1, 10, 1, Guid.NewGuid()));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => consumos.ListarPorReserva(1));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => consumos.HabitacionesDeReserva(1));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new HuespedNegocios(rol).Desactivar(1));
    }

    [TestMethod]
    [DataRow("ADMINISTRADOR")]
    [DataRow(" recepcionista ")]
    public void RolesOperativos_ConservanValidacionDeDatos(string rol)
    {
        Assert.IsTrue(PermisosOperacion.PuedeOperar(rol));
        Assert.ThrowsExactly<Exception>(() => new PagoNegocios(rol).RegistrarServicios(0, 1, "EFECTIVO", Guid.NewGuid()));
        Assert.ThrowsExactly<ArgumentException>(() => new ReservaNegocios(rol).CheckIn(0));
        Assert.ThrowsExactly<ArgumentException>(() => new DetalleServicioNegocios(rol).Insertar(0, 1, 1, 10, 1, Guid.NewGuid()));
    }

    [TestMethod]
    [DataRow("RECEPCIONISTA")]
    [DataRow(null)]
    public void Reportes_RechazanPerfilNoAutorizado(string? rol)
    {
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new ReporteNegocios(rol).ObtenerIngresos(DateTime.Today, DateTime.Today));
    }

    [TestMethod]
    [DataRow(51413, EstadoResultadoOperacion.RechazoDefinitivo)]
    [DataRow(51414, EstadoResultadoOperacion.RechazoDefinitivo)]
    [DataRow(51317, EstadoResultadoOperacion.RechazoDefinitivo)]
    [DataRow(51421, EstadoResultadoOperacion.ResultadoIncierto)]
    [DataRow(-2, EstadoResultadoOperacion.ResultadoIncierto)]
    [DataRow(1205, EstadoResultadoOperacion.ResultadoIncierto)]
    [DataRow(229, EstadoResultadoOperacion.ResultadoIncierto)]
    public void RechazoYRespuestaIncierta_NoSeConfunden(int numero, EstadoResultadoOperacion esperado)
    {
        Assert.AreEqual(esperado, ErroresOperacionSql.Clasificar(numero));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ErroresOperacionSql.Mensaje(numero)));
    }

    [TestMethod]
    public void RecuperacionConsumo_ConservaSolicitudYGuidAunqueCambienLosControles()
    {
        string carpeta = Path.Combine(Path.GetTempPath(), "HotelTramonto-" + Guid.NewGuid());
        try
        {
            var original = new SolicitudConsumo(2, 3, 4, 25.50m, 5);
            var diario = new DiarioOperaciones(carpeta);
            Guid id = Guid.Empty;
            Assert.ThrowsExactly<IOException>(() => diario.Ejecutar<SolicitudConsumo, bool>("consumo:3", original, contexto =>
            {
                id = contexto.Id;
                throw new IOException("Respuesta perdida");
            }));
            var reiniciado = new DiarioOperaciones(carpeta);
            var recuperada = reiniciado.Leer<SolicitudConsumo>("consumo:3");
            Assert.AreEqual(original, recuperada);
            bool enviada = false;
            Assert.ThrowsExactly<InvalidOperationException>(() => reiniciado.Ejecutar("consumo:3",
                original with { Cantidad = 99 }, _ => enviada = true));
            Assert.IsFalse(enviada);
            var resultado = reiniciado.Ejecutar("consumo:3", recuperada!, contexto => contexto.Id);
            Assert.AreEqual(id, resultado.Valor);
            reiniciado.Confirmar("consumo:3", resultado.Id);
            Assert.IsNull(reiniciado.Leer<SolicitudConsumo>("consumo:3"));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }
}
