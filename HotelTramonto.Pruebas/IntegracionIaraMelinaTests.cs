using System.Data;
using HotelTramonto.Datos;
using HotelTramonto.Negocios;
using HotelTramonto.Presentacion.Servicios;

namespace HotelTramonto.Pruebas;

[TestClass]
public class IntegracionIaraMelinaTests
{
    [TestMethod]
    public void OtraBusquedaOError_NoConservaReservaDeCobroAnterior()
    {
        var seleccion = new SeleccionReservaPagos();
        var reservas = new DataTable(); reservas.Columns.Add("id_reserva", typeof(int));
        reservas.Rows.Add(12);
        seleccion.Cargar(reservas);
        Assert.AreEqual(0, seleccion.IdReserva); // Nunca cobrar la primera fila autoseleccionada.
        seleccion.Seleccionar(12);
        Assert.AreEqual(12, seleccion.IdReserva);
        seleccion.Limpiar(); // Se invoca antes de buscar, también si SQL o el DNI fallan.
        Assert.AreEqual(0, seleccion.IdReserva);
        Assert.ThrowsExactly<ArgumentException>(() => seleccion.Seleccionar(12));
        reservas.Rows.Clear(); reservas.Rows.Add(15);
        seleccion.Cargar(reservas);
        Assert.ThrowsExactly<ArgumentException>(() => seleccion.Seleccionar(12));
        seleccion.Seleccionar(15);
        seleccion.Deseleccionar();
        Assert.AreEqual(0, seleccion.IdReserva);
        seleccion.Recuperar(12); // Contexto persistido, separado de los controles de otra búsqueda.
        Assert.AreEqual(12, seleccion.IdReserva);
        Assert.ThrowsExactly<ArgumentException>(() => seleccion.Seleccionar(15));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("GERENTE")]
    [DataRow("")]
    public void ConsultasNuevas_ExigenRolAntesDeSql(string? rol)
    {
        var datos = new ComprobantesSimulados();
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new PagoNegocios(rol, datos).ObtenerComprobante(8));
        Assert.AreEqual(0, datos.Consultas);
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new HabitacionNegocios().ListarConReservaCheckIn(rol));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => new HuespedNegocios(rol).ObtenerHistorial(1));
    }

    [TestMethod]
    public void Comprobante_UsaIdExactoYNoElUltimoPagoDeLaLista()
    {
        var datos = new ComprobantesSimulados();
        var negocio = new PagoNegocios("RECEPCIONISTA", datos);
        var tabla = negocio.ObtenerComprobante(8);
        Assert.AreEqual(8, datos.IdSolicitado);
        Assert.AreEqual(8, tabla.Rows[0]["id_pago"]);
        datos.Tabla.Rows[0]["id_pago"] = 9;
        Assert.ThrowsExactly<InvalidOperationException>(() => negocio.ObtenerComprobante(8));
    }

    [TestMethod]
    [DataRow("PENDIENTE")]
    [DataRow("RECHAZADO")]
    [DataRow("REEMBOLSADO")]
    public void PagoNoAprobado_NoProduceComprobanteNiArchivo(string estado)
    {
        var datos = new ComprobantesSimulados(); datos.Tabla.Rows[0]["estado"] = estado;
        Assert.ThrowsExactly<InvalidOperationException>(() => new PagoNegocios("ADMINISTRADOR", datos).ObtenerComprobante(8));
        string archivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
        Assert.ThrowsExactly<InvalidOperationException>(() => ComprobantePagoPdf.Generar(datos.Tabla.Rows[0], archivo));
        Assert.IsFalse(File.Exists(archivo));
    }

    [TestMethod]
    public void ComprobanteVacioDuplicadoOIncompleto_SeRechaza()
    {
        var datos = new ComprobantesSimulados();
        datos.Tabla.ImportRow(datos.Tabla.Rows[0]);
        Assert.ThrowsExactly<InvalidOperationException>(() => new PagoNegocios("ADMINISTRADOR", datos).ObtenerComprobante(8));
        datos.Tabla.Rows.Clear();
        Assert.ThrowsExactly<Exception>(() => new PagoNegocios("ADMINISTRADOR", datos).ObtenerComprobante(8));
        var incompleta = new DataTable(); incompleta.Columns.Add("id_pago"); incompleta.Rows.Add("8");
        Assert.ThrowsExactly<InvalidOperationException>(() => ComprobantePagoNegocios.Validar(incompleta.Rows[0]));
    }

    [TestMethod]
    public void PdfReal_ConservaDisenoDeIaraYGeneraDocumento()
    {
        string archivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
        try
        {
            ComprobantePagoPdf.Generar(new ComprobantesSimulados().Tabla.Rows[0], archivo);
            byte[] bytes = File.ReadAllBytes(archivo);
            Assert.IsTrue(bytes.Length > 2000);
            Assert.AreEqual("%PDF-", System.Text.Encoding.ASCII.GetString(bytes, 0, 5));
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(bytes[^50..]).Contains("%%EOF"));
        }
        finally { if (File.Exists(archivo)) File.Delete(archivo); }
    }

    [TestMethod]
    public void FiltrosDeIara_ConservanInactivasYNoAlteranOrigen()
    {
        var origen = new DataTable();
        foreach (string campo in new[] { "tipo", "piso", "estado" }) origen.Columns.Add(campo);
        origen.Columns.Add("activo", typeof(bool));
        origen.Rows.Add("Doble", "1", "DISPONIBLE", true);
        origen.Rows.Add("Doble", "1", "DISPONIBLE", false);
        origen.Rows.Add("Suite", "2", "OCUPADA", true);
        origen.Rows.Add("Suite", DBNull.Value, "MANTENIMIENTO", true);
        Assert.HasCount(4, FiltrosHabitaciones.Aplicar(origen, "Todos", "Todos", "Todos", false).Rows);
        Assert.HasCount(3, FiltrosHabitaciones.Aplicar(origen, "Todos", "Todos", "Todos", true).Rows);
        Assert.HasCount(1, FiltrosHabitaciones.Aplicar(origen, "Disponibles", "Todos", "Todos", false).Rows);
        Assert.HasCount(1, FiltrosHabitaciones.Aplicar(origen, "Ocupadas", "suite", "2", true).Rows);
        Assert.HasCount(0, FiltrosHabitaciones.Aplicar(origen, "Mantenimiento", "Todos", "1", false).Rows);
        Assert.HasCount(4, origen.Rows);
    }

    [TestMethod]
    public void HistorialIntegrado_ConservaHabitacionesYObservacionesSinAlterarDatos()
    {
        var historial = new DataTable();
        historial.Columns.Add("id_reserva", typeof(int)); historial.Columns.Add("numero_habitacion_actual");
        historial.Columns.Add("tipo_habitacion_actual");
        historial.Rows.Add(12, "101", "Doble"); historial.Rows.Add(12, "102", "Doble");
        historial.Rows.Add(15, DBNull.Value, DBNull.Value);
        var reservas = new DataTable(); reservas.Columns.Add("id_reserva", typeof(int));
        reservas.Columns.Add("total", typeof(decimal)); reservas.Columns.Add("observaciones");
        reservas.Rows.Add(12, 1234.56m, "Observación de prueba"); reservas.Rows.Add(15, 99m, DBNull.Value);
        var completo = DetalleHistorialHuesped.Completar(historial, reservas);
        Assert.HasCount(3, completo.Rows);
        Assert.AreEqual("102", completo.Rows[1]["numero_habitacion_actual"]);
        Assert.AreEqual(1234.56m, completo.Rows[1]["total"]);
        Assert.AreEqual("Observación de prueba", completo.Rows[0]["observaciones"]);
        Assert.IsTrue(completo.Rows[2].IsNull("numero_habitacion_actual"));
        Assert.IsFalse(historial.Columns.Contains("total"));
    }

    [TestMethod]
    public void CrearYConfirmarIntegrado_NoPermiteSaltarElRolNiRecrearAltaIncierta()
    {
        string carpeta = Path.Combine(Path.GetTempPath(), "HotelTramonto-" + Guid.NewGuid());
        try
        {
            var diario = new DiarioOperaciones(carpeta);
            var solicitud = new SolicitudReserva(1, 2, DateTime.Today, DateTime.Today.AddDays(1), "PAGO_TOTAL", 1, 3, 100, "EFECTIVO");
            diario.Ejecutar("reserva-nueva:3", solicitud, c => { c.MarcarCreacion(); return true; });
            Assert.ThrowsExactly<UnauthorizedAccessException>(() => diario.Ejecutar("reserva-nueva:3", solicitud,
                c => new ReservaNegocios("GERENTE").CrearYConfirmar(solicitud, c)));
            Assert.ThrowsExactly<InvalidOperationException>(() => diario.Ejecutar("reserva-nueva:3", solicitud,
                c => new ReservaNegocios("RECEPCIONISTA").CrearYConfirmar(solicitud, c)));
            Assert.AreEqual(solicitud, diario.Leer<SolicitudReserva>("reserva-nueva:3"));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    private sealed class ComprobantesSimulados : IComprobantePagoDatos
    {
        public int Consultas, IdSolicitado;
        public DataTable Tabla = Crear();
        public DataTable ObtenerComprobante(int idPago) { Consultas++; IdSolicitado = idPago; return Tabla; }
        private static DataTable Crear()
        {
            var t = new DataTable();
            t.Columns.Add("id_pago", typeof(int)); t.Columns.Add("id_reserva", typeof(int));
            t.Columns.Add("monto", typeof(decimal)); t.Columns.Add("fecha_pago", typeof(DateTime));
            foreach (string campo in new[] { "estado", "metodo_pago", "tipo_pago", "nombre_huesped", "apellido_huesped", "dni", "nombre_usuario", "apellido_usuario" })
                t.Columns.Add(campo);
            t.Rows.Add(8, 12, 1234.56m, new DateTime(2026, 11, 16, 12, 30, 0), "APROBADO", "EFECTIVO", "SERVICIOS_ADICIONALES", "Huésped", "Prueba", "12345678", "Empleado", "Prueba");
            return t;
        }
    }
}
