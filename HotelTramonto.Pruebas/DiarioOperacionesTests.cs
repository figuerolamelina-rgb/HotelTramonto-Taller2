using HotelTramonto.Negocios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class DiarioOperacionesTests
{
    private string carpeta = null!;
    [TestInitialize]
    public void Preparar() => carpeta = Path.Combine(Path.GetTempPath(), "tramonto-diario-" + Guid.NewGuid());
    [TestCleanup]
    public void Limpiar() { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, recursive: true); }

    [TestMethod]
    public void RespuestaPerdidaYReinicio_ConservanIdentificadorYSolicitud()
    {
        var solicitud = new SolicitudCobro(10, 2, "EFECTIVO", true);
        Guid primero = Guid.Empty;
        Assert.ThrowsExactly<IOException>(() =>
            new DiarioOperaciones(carpeta).Ejecutar<SolicitudCobro, bool>("servicios:2", solicitud, contexto =>
            {
                primero = contexto.Id;
                throw new IOException("Respuesta perdida después del commit simulado.");
            }));
        var reiniciado = new DiarioOperaciones(carpeta);
        Assert.AreEqual(solicitud, reiniciado.Leer<SolicitudCobro>("servicios:2"));
        var resultado = reiniciado.Ejecutar("servicios:2", solicitud, contexto => contexto.Id);
        Assert.AreEqual(primero, resultado.Id);
        Assert.AreEqual(primero, resultado.Valor);
    }

    [TestMethod]
    public void SolicitudDiferente_NoSustituyePendiente()
    {
        var diario = new DiarioOperaciones(carpeta);
        var original = new SolicitudCobro(1, 2, "EFECTIVO", true);
        diario.Ejecutar("pago", original, contexto => true);
        bool enviado = false;
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            diario.Ejecutar("pago", original with { Metodo = "OTRO" }, contexto => enviado = true));
        Assert.IsFalse(enviado);
        Assert.AreEqual(original, diario.Leer<SolicitudCobro>("pago"));
    }

    [TestMethod]
    public void TemporalPersistida_ReinicioNoCreaOtraTemporal()
    {
        var solicitud = new SolicitudReserva(1, 2, DateTime.Today, DateTime.Today.AddDays(1),
            "SENA_50", 1, 3, 50m, "EFECTIVO");
        var diario = new DiarioOperaciones(carpeta);
        Assert.ThrowsExactly<IOException>(() => diario.Ejecutar<SolicitudReserva, bool>(
            "reserva-nueva:3", solicitud, contexto =>
            {
                contexto.GuardarReferencia(42);
                throw new IOException("Cierre después del pago.");
            }));
        new DiarioOperaciones(carpeta).Ejecutar("reserva-nueva:3", solicitud, contexto =>
        {
            Assert.AreEqual(42, contexto.Referencia);
            return true;
        });
    }

    [TestMethod]
    public void TemporalSinRespuesta_NoSeRecreaAutomaticamente()
    {
        var diario = new DiarioOperaciones(carpeta);
        var solicitud = new SolicitudReserva(1, 2, DateTime.Today, DateTime.Today.AddDays(1),
            "PAGO_TOTAL", 1, 3, 100m, "EFECTIVO");
        Assert.ThrowsExactly<IOException>(() => diario.Ejecutar<SolicitudReserva, bool>(
            "reserva-nueva:3", solicitud, contexto =>
            {
                contexto.MarcarCreacion();
                throw new IOException("Sin respuesta del alta temporal.");
            }));
        new DiarioOperaciones(carpeta).Ejecutar("reserva-nueva:3", solicitud, contexto =>
        {
            Assert.IsTrue(contexto.CreacionIniciada);
            Assert.IsNull(contexto.Referencia);
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                new FlujoReservaNegocios().Ejecutar(solicitud, contexto));
            return true;
        });
    }

    [TestMethod]
    public void ComprobanteConfirmado_NoPuedeCompletarOtraOperacion()
    {
        var diario = new DiarioOperaciones(carpeta);
        var solicitud = new SolicitudCobro(1, 2, "EFECTIVO", true);
        var uno = diario.Ejecutar("pago", solicitud, contexto => true);
        diario.Confirmar("pago", uno.Id);
        Assert.IsNull(diario.Leer<SolicitudCobro>("pago"));
        var dos = diario.Ejecutar("pago", solicitud, contexto => true);
        Assert.AreNotEqual(uno.Id, dos.Id);
        diario.Confirmar("pago", uno.Id);
        Assert.IsNotNull(diario.Leer<SolicitudCobro>("pago"));
        Assert.IsTrue(Directory.GetFiles(carpeta, "*.confirmada").Length == 1);
    }

    [TestMethod]
    public void EscritoresLocalesSimultaneos_NoSustituyenSolicitud()
    {
        var diario = new DiarioOperaciones(carpeta);
        var solicitud = new SolicitudCobro(1, 2, "EFECTIVO", true);
        diario.Ejecutar("pago", solicitud, contexto =>
        {
            Assert.ThrowsExactly<IOException>(() =>
                new DiarioOperaciones(carpeta).Ejecutar("pago", solicitud, otro => true));
            return true;
        });
    }

    [TestMethod]
    public void DiarioCorrupto_NoGeneraNuevaOperacion()
    {
        var diario = new DiarioOperaciones(carpeta);
        var solicitud = new SolicitudCobro(1, 2, "EFECTIVO", true);
        diario.Ejecutar("pago", solicitud, contexto => true);
        File.WriteAllText(Directory.GetFiles(carpeta, "*.json").Single(), "{");
        bool enviado = false;
        Assert.ThrowsExactly<System.Text.Json.JsonException>(() =>
            diario.Ejecutar("pago", solicitud, contexto => enviado = true));
        Assert.IsFalse(enviado);
    }

    [TestMethod]
    public void PagoInicial_RedondeoCoincideConReglaSQL()
    {
        Assert.AreEqual(50.01m, FlujoReservaNegocios.CalcularPagoInicial(100.01m, "SENA_50"));
        Assert.AreEqual(100.01m, FlujoReservaNegocios.CalcularPagoInicial(100.01m, "PAGO_TOTAL"));
        Assert.ThrowsExactly<ArgumentException>(() => FlujoReservaNegocios.CalcularPagoInicial(100m, "OTRA"));
    }

    [TestMethod]
    public void AprobacionesYAltasSinOperacion_NoLleganASQL()
    {
        var pagos = new PagoNegocios();
        Assert.ThrowsExactly<ArgumentException>(() => pagos.RegistrarServicios(1, 1, "EFECTIVO", Guid.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => pagos.RegistrarSaldoAlojamiento(1, 1, "EFECTIVO", Guid.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => pagos.Insertar(null, 1, 1, 10m, "EFECTIVO", "APROBADO", "PAGO_INICIAL", Guid.Empty));
    }
}
