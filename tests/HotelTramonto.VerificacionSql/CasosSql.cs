using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public sealed partial class CasosSql : IDisposable
{
    private readonly CopiaSql copia;
    private readonly SqlConnection db;
    private readonly int usuario, huesped, servicio, reserva;
    private readonly int[] habitaciones;
    private int siguienteTemporal;
    public static readonly (string Nombre, Action<CasosSql> Prueba)[] Todos =
    [
        ("01_UnaYVariasHabitaciones", c => c.UnaYVarias()),
        ("02_HabitacionAjenaYClaveForanea", c => c.Ajena()),
        ("03_ServicioInactivoYPrecio", c => c.Inactivo()),
        ("04_ReservaSinCheckin", c => c.SinCheckin()),
        ("05_ReintentoYPrecioHistorico", c => c.ReintentoConsumo()),
        ("06_HistoricosSinHabitacion", c => c.Historico()),
        ("07_DosCobrosSimultaneos", c => c.DosCobros()),
        ("08_ReintentosConsumoSimultaneos", c => c.DosReintentos()),
        ("09_ConsumoContraCheckout", c => c.ConsumoCierre()),
        ("10_PagoContraCheckout", c => c.PagoCierre()),
        ("11_ConsumoContraPago", c => c.ConsumoPago()),
        ("12_AltaGeneralLimitaSaldo", c => c.LimiteGeneral()),
        ("13_DosPendientesUnSaldo", c => c.Pendientes()),
        ("14_AprobacionContraCobro", c => c.AprobacionCobro()),
        ("15_GeneralContraEspecializado", c => c.GeneralCobro()),
        ("16_ReintentoNoCobraSaldoNuevo", c => c.ReintentoPago()),
        ("17_ReintentosPagoSimultaneos", c => c.ReintentosPago()),
        ("18_AprobacionIdempotente", c => c.AprobacionRepetida()),
        ("19_CierreRepetidoYNuevosCargos", c => c.CierreRepetido()),
        ("20_PendienteDespuesDelCierre", c => c.PendienteCerrado()),
        ("21_Inicial50Y100ConfirmacionReintento", c => c.Inicial()),
        ("22_DosInicialesSimultaneos", c => c.DosIniciales()),
        ("23_AprobarInicialYConfirmar", c => c.AprobarInicial()),
        ("24_DosConfirmacionesSimultaneas", c => c.DosConfirmaciones()),
        ("25_AprobacionContraConfirmacion", c => c.AprobarConfirmar()),
        ("26_AlojamientoCheckinCheckoutSaldo", c => c.Alojamiento()),
        ("27_DosSaldosAlojamientoSimultaneos", c => c.DosAlojamientos()),
        ("28_IntegridadHistoricaYSeparacion", c => c.Separacion())
    ];

    public CasosSql(CopiaSql copia)
    {
        this.copia = copia;
        db = copia.Abrir();
        try
        {
            usuario = Convert.ToInt32(Sql.Escalar(db, "SELECT TOP(1) id_usuario FROM dbo.Usuarios WHERE activo=1 ORDER BY id_usuario")
                ?? throw new InvalidOperationException("Falta usuario activo."));
            huesped = Convert.ToInt32(Sql.Escalar(db, "SELECT TOP(1) id_huesped FROM dbo.Huespedes ORDER BY id_huesped")
                ?? throw new InvalidOperationException("Falta huésped."));
            // Ejecutor captura los históricos antes de construir cualquier caso.
            // Cada caso recibe su propio tipo y tres habitaciones NUEVAS, nunca el catálogo anterior.
            habitaciones = HabitacionesFixture.Crear(copia).Habitaciones;
            servicio = Convert.ToInt32(Sql.Escalar(db, """
                INSERT dbo.Servicios(nombre,descripcion,precio,activo,categoria)
                OUTPUT inserted.id_servicio VALUES(@p0,'Fixture correcciones 5 y 6',25.50,1,'Otros');
                """, "TEST-" + Guid.NewGuid()));
            reserva = Reserva("CHECKIN", 2);
        }
        catch { db.Dispose(); throw; }
    }
    public void Dispose() => db.Dispose();
    private static void Exigir(bool ok, string mensaje = "Resultado inesperado.") { if (!ok) throw new InvalidOperationException(mensaje); }
    private static void Error(int numero, Action accion)
    {
        try { accion(); }
        catch (SqlException ex) when (ex.Number == numero) { return; }
        throw new InvalidOperationException("Se esperaba rechazo SQL " + numero);
    }
    private int Reserva(string estado, int numero)
    {
        int id = Convert.ToInt32(Sql.Escalar(db, """
            INSERT dbo.Reservas(id_huesped,id_usuario,fecha_entrada,fecha_salida,estado,total,observaciones)
            OUTPUT inserted.id_reserva VALUES(@p0,@p1,'2099-01-01','2099-01-02',@p2,0,'Fixture correcciones 5 y 6');
            """, huesped, usuario, estado));
        foreach (int h in habitaciones.Take(numero))
            Sql.Ejecutar(db, "INSERT dbo.DetalleReservas(id_reserva,id_habitacion,precio_noche,cantidad_personas) VALUES(@p0,@p1,1,1)", id, h);
        return id;
    }
    private void Consumir(SqlConnection? conexion = null, int? r = null, int? h = null, int cantidad = 1,
        decimal precio = 25.50m, Guid? operacion = null) =>
        Sql.Ejecutar(conexion ?? db, "EXEC dbo.sp_DetalleServicio_Insertar @p0,@p1,@p2,@p3,@p4,@p5",
            servicio, r ?? reserva, cantidad, precio, h ?? habitaciones[0], operacion ?? Guid.NewGuid());
    private DataTable Pagar(SqlConnection? conexion = null, Guid? operacion = null) =>
        Sql.Tabla(conexion ?? db, "EXEC dbo.sp_Pago_RegistrarServicios @p0,@p1,@p2,@p3",
            reserva, usuario, "EFECTIVO", operacion ?? Guid.NewGuid());
    private void Cerrar(SqlConnection? conexion = null) =>
        Sql.Ejecutar(conexion ?? db, "EXEC dbo.sp_Reserva_CheckOut @p0", reserva);
    private (decimal Consumido, decimal Pagado) Totales() =>
        (Convert.ToDecimal(Sql.Escalar(db, "SELECT COALESCE(SUM(cantidad*precio_unitario),0) FROM dbo.DetalleServicios WHERE id_reserva=@p0", reserva)),
        Convert.ToDecimal(Sql.Escalar(db, "SELECT COALESCE(SUM(monto),0) FROM dbo.Pagos WHERE id_reserva=@p0 AND estado='APROBADO' AND tipo_pago='SERVICIOS_ADICIONALES'", reserva)));
    private int General(SqlConnection? conexion = null, decimal monto = 25.50m, string estado = "APROBADO",
        string tipo = "SERVICIOS_ADICIONALES", Guid? operacion = null, int? r = null, int? temporal = null) =>
        Convert.ToInt32(Sql.Tabla(conexion ?? db, "EXEC dbo.sp_Pago_Insertar @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7",
            temporal.HasValue ? null : r ?? reserva, temporal, usuario, monto, "EFECTIVO", estado, tipo,
            operacion ?? Guid.NewGuid()).Rows[0]["id_pago"]);
    private void Aprobar(int pago, SqlConnection? conexion = null, string estado = "APROBADO") =>
        Sql.Ejecutar(conexion ?? db, "EXEC dbo.sp_Pago_ActualizarEstado @p0,@p1", pago, estado);
    private int[] Paralelo(params Action<SqlConnection>[] acciones)
    {
        using var barrera = new Barrier(acciones.Length);
        var tareas = acciones.Select(accion => Task.Run(() =>
        {
            using var otraConexion = copia.Abrir(); // Una conexión física independiente por contendiente.
            if (!barrera.SignalAndWait(TimeSpan.FromSeconds(20))) throw new TimeoutException("Barrera agotada.");
            try { accion(otraConexion); return 0; }
            catch (SqlException ex) { return ex.Number; } // Solo códigos esperados cuentan como éxito de una prueba.
        })).ToArray();
        return Task.WhenAll(tareas).GetAwaiter().GetResult();
    }
    private static void Resultados(int[] actual, params int[] esperado) =>
        Exigir(actual.Order().SequenceEqual(esperado.Order()), "Concurrencia inesperada: " + string.Join(",", actual));

    private (int Id, decimal Monto) Temporal(string modo)
    {
        var entrada = HabitacionesFixture.EntradaTemporal(siguienteTemporal++);
        int id = Convert.ToInt32(Sql.Escalar(db, "EXEC dbo.sp_ReservaTemporal_Insertar @p0,@p1,@p2,@p3,@p4",
            huesped, habitaciones[2], entrada, entrada.AddDays(2), modo));
        decimal precio = Convert.ToDecimal(Sql.Escalar(db, """
            SELECT t.precio_noche FROM dbo.Habitaciones h JOIN dbo.TiposHabitacion t
            ON t.id_tipoHabitacion=h.tipo_habitacion WHERE h.id_habitacion=@p0
            """, habitaciones[2]));
        return (id, modo == "PAGO_TOTAL" ? precio * 2 : precio);
    }
    private int Confirmar(int temporal, SqlConnection? conexion = null) =>
        Convert.ToInt32(Sql.Tabla(conexion ?? db, "EXEC dbo.sp_ConfirmarReserva @p0,1", temporal).Rows[0]["id_reserva"]);

    private void UnaYVarias()
    {
        int una = Reserva("CHECKIN", 1); Consumir(r: una); Consumir(); Consumir(h: habitaciones[1]);
        var filas = Sql.Tabla(db, "EXEC dbo.sp_DetalleServicio_ListarPorReserva @p0", reserva);
        Exigir(filas.Rows.Count == 2 && filas.Rows.Cast<DataRow>().Select(r => (int)r["id_habitacion"]).Order().SequenceEqual(habitaciones.Take(2).Order()));
        Exigir(Sql.Tabla(db, "EXEC dbo.sp_DetalleServicio_ListarPorReserva @p0,@p1", reserva, habitaciones[1]).Rows.Count == 1);
    }
    private void Ajena()
    {
        Error(51316, () => Consumir(h: habitaciones[2]));
        Error(547, () => Sql.Ejecutar(db, """
            INSERT dbo.DetalleServicios(id_servicios,id_reserva,id_habitacion,cantidad,precio_unitario,id_operacion)
            VALUES(@p0,@p1,@p2,1,25.50,NEWID());
            """, servicio, reserva, habitaciones[2]));
    }
    private void Inactivo()
    {
        Error(51317, () => Consumir(precio: 20m));
        Sql.Ejecutar(db, "UPDATE dbo.Servicios SET activo=0 WHERE id_servicio=@p0", servicio);
        Error(51317, () => Consumir());
    }
    private void SinCheckin()
    {
        foreach (string estado in new[] { "CONFIRMADA", "CANCELADA", "FINALIZADA" })
        { int r = Reserva(estado, 1); Error(51315, () => Consumir(r: r)); }
    }
    private void ReintentoConsumo()
    {
        var op = Guid.NewGuid(); Consumir(operacion: op);
        Sql.Ejecutar(db, "UPDATE dbo.Servicios SET precio=30 WHERE id_servicio=@p0", servicio);
        Consumir(operacion: op); Error(51314, () => Consumir(cantidad: 2, operacion: op));
        Exigir(Totales() == (25.50m, 0m));
        Exigir(Convert.ToInt32(Sql.Escalar(db, "SELECT COUNT(*) FROM dbo.DetalleServicios WHERE id_operacion=@p0", op)) == 1);
    }
    private void Historico()
    {
        Sql.Ejecutar(db, "INSERT dbo.DetalleServicios(id_servicios,id_reserva,cantidad,precio_unitario) VALUES(@p0,@p1,2,10)", servicio, reserva);
        Consumir();
        var filas = Sql.Tabla(db, "EXEC dbo.sp_DetalleServicio_ListarPorReserva @p0", reserva);
        Exigir(filas.Rows.Count == 2 && filas.Rows.Cast<DataRow>().Count(r => r.IsNull("id_habitacion")) == 1 && Totales().Consumido == 45.50m);
    }
    private void DosCobros() { Consumir(); Resultados(Paralelo(c => Pagar(c), c => Pagar(c)), 0, 51413); Exigir(Totales().Consumido == Totales().Pagado); }
    private void DosReintentos() { var op = Guid.NewGuid(); Resultados(Paralelo(c => Consumir(c, operacion: op), c => Consumir(c, operacion: op)), 0, 0); Exigir(Totales().Consumido == 25.50m); }
    private void ConsumoCierre()
    {
        var resultado = Paralelo(c => Consumir(c), c => Cerrar(c));
        string estado = (string)Sql.Escalar(db, "SELECT estado FROM dbo.Reservas WHERE id_reserva=@p0", reserva)!;
        if (estado == "FINALIZADA") { Resultados(resultado, 51315, 0); Exigir(Totales().Consumido == 0); }
        else { Exigir(estado == "CHECKIN"); Resultados(resultado, 0, 50032); }
    }
    private void PagoCierre() { Consumir(); var r = Paralelo(c => Pagar(c), c => Cerrar(c)); Exigir(r[0] == 0 && r[1] is 0 or 50032 && Totales().Consumido == Totales().Pagado); }
    private void ConsumoPago() { var r = Paralelo(c => Consumir(c), c => Pagar(c)); var t = Totales(); Exigir(r[0] == 0 && r[1] is 0 or 51413 && t.Consumido == 25.50m && (t.Pagado == 0 || t.Pagado == t.Consumido)); }
    private void LimiteGeneral() { Consumir(); Error(51414, () => General(monto: 26)); General(monto: 20); Error(51414, () => General(monto: 10)); Exigir(Totales() == (25.50m, 20m)); }
    private void Pendientes() { Consumir(); int a = General(estado: "PENDIENTE"), b = General(estado: "PENDIENTE"); Resultados(Paralelo(c => Aprobar(a, c), c => Aprobar(b, c)), 0, 51413); Exigir(Totales().Consumido == Totales().Pagado); }
    private void AprobacionCobro() { Consumir(); int p = General(estado: "PENDIENTE"); Resultados(Paralelo(c => Aprobar(p, c), c => Pagar(c)), 0, 51413); Exigir(Totales().Consumido == Totales().Pagado); }
    private void GeneralCobro() { Consumir(); Resultados(Paralelo(c => General(c), c => Pagar(c)), 0, 51413); Exigir(Totales().Consumido == Totales().Pagado); }
    private void ReintentoPago()
    {
        Consumir(); var op = Guid.NewGuid(); var primero = Pagar(operacion: op).Rows[0]["id_pago"];
        Consumir(); Exigir(Equals(primero, Pagar(operacion: op).Rows[0]["id_pago"]) && Totales() == (51m, 25.50m));
        Error(51403, () => Sql.Ejecutar(db, "EXEC dbo.sp_Pago_RegistrarServicios @p0,@p1,'OTRO',@p2", reserva, usuario, op));
    }
    private void ReintentosPago()
    {
        Consumir(); var op = Guid.NewGuid(); Resultados(Paralelo(c => Pagar(c, op), c => Pagar(c, op)), 0, 0);
        Exigir(Totales().Consumido == Totales().Pagado && Convert.ToInt32(Sql.Escalar(db, "SELECT COUNT(*) FROM dbo.Pagos WHERE id_operacion=@p0", op)) == 1);
    }
    private void AprobacionRepetida() { Consumir(); int p = General(estado: "PENDIENTE"); Aprobar(p); Aprobar(p); Error(50122, () => Aprobar(p, estado: "RECHAZADO")); Exigir(Totales().Consumido == Totales().Pagado); }
    private void CierreRepetido() { Consumir(); Pagar(); Cerrar(); Cerrar(); Error(51315, () => Consumir()); Error(51410, () => General()); }
    private void PendienteCerrado() { Consumir(); int p = General(estado: "PENDIENTE"); Pagar(); Cerrar(); Error(51410, () => Aprobar(p)); Exigir((string)Sql.Escalar(db, "SELECT estado FROM dbo.Pagos WHERE id_pago=@p0", p)! == "PENDIENTE"); }
    private void Inicial()
    {
        foreach (string modo in new[] { "SENA_50", "PAGO_TOTAL" })
        {
            var t = Temporal(modo); var op = Guid.NewGuid();
            int pago = General(monto: t.Monto, tipo: "PAGO_INICIAL", operacion: op, temporal: t.Id);
            int confirmada = Confirmar(t.Id);
            Exigir(pago == General(monto: t.Monto, tipo: "PAGO_INICIAL", operacion: op, temporal: t.Id));
            Exigir((int)Sql.Tabla(db, "EXEC dbo.sp_ReservaTemporal_ConsultarConfirmacion @p0", t.Id).Rows[0]["id_reserva"] == confirmada);
            Error(50002, () => Confirmar(t.Id));
            Error(51416, () => General(monto: t.Monto, tipo: "PAGO_INICIAL", temporal: t.Id));
        }
    }
    private void DosIniciales()
    {
        var t = Temporal("SENA_50");
        Resultados(Paralelo(c => General(c, monto: t.Monto, tipo: "PAGO_INICIAL", temporal: t.Id), c => General(c, monto: t.Monto, tipo: "PAGO_INICIAL", temporal: t.Id)), 0, 51420);
    }
    private void AprobarInicial() { var t = Temporal("PAGO_TOTAL"); int p = General(monto: t.Monto, estado: "PENDIENTE", tipo: "PAGO_INICIAL", temporal: t.Id); Aprobar(p); Exigir(Confirmar(t.Id) > 0); Aprobar(p); }
    private void DosConfirmaciones()
    {
        var t = Temporal("PAGO_TOTAL"); General(monto: t.Monto, tipo: "PAGO_INICIAL", temporal: t.Id);
        Resultados(Paralelo(c => Confirmar(t.Id, c), c => Confirmar(t.Id, c)), 0, 50002);
        Exigir(Sql.Tabla(db, "EXEC dbo.sp_ReservaTemporal_ConsultarConfirmacion @p0", t.Id).Rows.Count == 1);
    }
    private void AprobarConfirmar()
    {
        var t = Temporal("PAGO_TOTAL"); int p = General(monto: t.Monto, estado: "PENDIENTE", tipo: "PAGO_INICIAL", temporal: t.Id);
        var r = Paralelo(c => Aprobar(p, c), c => Confirmar(t.Id, c));
        Exigir(r[0] == 0 && r[1] is 0 or 50008);
        if (r[1] == 50008) Confirmar(t.Id);
        Exigir(Sql.Tabla(db, "EXEC dbo.sp_ReservaTemporal_ConsultarConfirmacion @p0", t.Id).Rows.Count == 1);
    }
    private void Alojamiento()
    {
        var t = Temporal("SENA_50"); General(monto: t.Monto, tipo: "PAGO_INICIAL", temporal: t.Id); int r = Confirmar(t.Id);
        Error(50028, () => Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckIn @p0", r));
        Sql.Ejecutar(db, "EXEC dbo.sp_Pago_RegistrarSaldoAlojamiento @p0,@p1,'EFECTIVO',@p2", r, usuario, Guid.NewGuid());
        Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckIn @p0", r);
        Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckOut @p0", r);
        Exigir((decimal)Sql.Tabla(db, "EXEC dbo.sp_Reserva_ConsultarSaldo @p0", r).Rows[0]["total_pendiente"] == 0);
    }
    private void DosAlojamientos()
    {
        var t = Temporal("SENA_50"); General(monto: t.Monto, tipo: "PAGO_INICIAL", temporal: t.Id); int r = Confirmar(t.Id);
        void Cobrar(SqlConnection c) => Sql.Ejecutar(c, "EXEC dbo.sp_Pago_RegistrarSaldoAlojamiento @p0,@p1,'EFECTIVO',@p2", r, usuario, Guid.NewGuid());
        Resultados(Paralelo(Cobrar, Cobrar), 0, 51413);
    }
    private void Separacion()
    {
        Consumir();
        string antes = System.Text.Json.JsonSerializer.Serialize(Sql.Tabla(db, "SELECT id_detalle_servicio,cantidad,precio_unitario FROM dbo.DetalleServicios WHERE id_reserva=@p0 ORDER BY id_detalle_servicio", reserva).Rows.Cast<DataRow>().Select(r => r.ItemArray));
        Pagar();
        string despues = System.Text.Json.JsonSerializer.Serialize(Sql.Tabla(db, "SELECT id_detalle_servicio,cantidad,precio_unitario FROM dbo.DetalleServicios WHERE id_reserva=@p0 ORDER BY id_detalle_servicio", reserva).Rows.Cast<DataRow>().Select(r => r.ItemArray));
        var pago = Sql.Tabla(db, "SELECT monto,tipo_pago FROM dbo.Pagos WHERE id_reserva=@p0", reserva).Rows[0];
        Exigir(antes == despues && (decimal)pago["monto"] == 25.50m && (string)pago["tipo_pago"] == "SERVICIOS_ADICIONALES");
    }
}
