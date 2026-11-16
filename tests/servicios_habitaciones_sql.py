"""Suite OPT-IN para LocalDB desechable creado por Probar-ServiciosHabitaciones.ps1.
No instala scripts. Crea datos de prueba propios y los conserva; no usar la base original.
Requiere pyodbc, ODBC Driver SQL Server y HOTELTRAMONTO_SQL_TEST_CONNECTION privado.
Ejecutar únicamente después de revisar docs/servicios-habitaciones.md.
"""
from datetime import date, timedelta
import json
import os
import re
import sys
import threading
import unittest
import uuid
from concurrent.futures import ThreadPoolExecutor
from decimal import Decimal


class IntegridadSQL(unittest.TestCase):
    def connect(self):
        run_id = os.environ.get("HOTELTRAMONTO_TEST_RUN_ID")
        if not run_id:
            raise RuntimeError("Usar tools/Probar-ServiciosHabitaciones.ps1; falta copia aislada.")
        connection = pyodbc.connect(os.environ["HOTELTRAMONTO_SQL_TEST_CONNECTION"], autocommit=True)
        try:
            metadata = connection.cursor().execute(
                "SELECT DB_NAME(), CONVERT(int,SERVERPROPERTY('IsLocalDB'))").fetchone()
            self.assertEqual(("HotelTramontoDB_Pruebas", 1), tuple(metadata))
            marker = connection.cursor().execute(
                "SELECT CONVERT(nvarchar(36),value) FROM sys.extended_properties "
                "WHERE class=0 AND name='HotelTramonto_TestRun'").fetchval()
            self.assertEqual(run_id, marker, "La copia no pertenece a esta ejecución.")
            return connection
        except BaseException:
            connection.close()
            raise

    def setUp(self):
        self.db = self.connect()
        self.addCleanup(self.db.close)
        cur = self.db.cursor()
        self.user = cur.execute("SELECT TOP(1) id_usuario FROM dbo.Usuarios WHERE activo=1 ORDER BY id_usuario").fetchval()
        guest = cur.execute("SELECT TOP(1) id_huesped FROM dbo.Huespedes ORDER BY id_huesped").fetchval()
        self.rooms = [row[0] for row in cur.execute("SELECT TOP(3) h.id_habitacion FROM dbo.Habitaciones h JOIN dbo.TiposHabitacion t ON t.id_tipoHabitacion=h.tipo_habitacion WHERE h.activo=1 AND t.activo=1 AND h.estado<>'MANTENIMIENTO' ORDER BY h.id_habitacion")]
        self.assertTrue(self.user and guest and len(self.rooms) == 3,
                        "Se requieren un usuario, un huesped y tres habitaciones en la copia de pruebas.")
        self.guest = guest
        self.service = cur.execute("""
INSERT dbo.Servicios(nombre,descripcion,precio,activo,categoria)
OUTPUT inserted.id_servicio VALUES(?, 'Fixture correcciones 5 y 6', 25.50, 1, 'Otros')""",
                                  "TEST-" + str(uuid.uuid4())).fetchval()
        self.reservation = self.reserva("CHECKIN", 2)

    def reserva(self, state, count):
        cur = self.db.cursor()
        result = cur.execute("""
INSERT dbo.Reservas(id_huesped,id_usuario,fecha_entrada,fecha_salida,estado,total,observaciones)
OUTPUT inserted.id_reserva VALUES(?,?, '2099-01-01','2099-01-02',?,0,'Fixture correcciones 5 y 6')""",
                             self.guest, self.user, state).fetchval()
        for room in self.rooms[:count]:
            cur.execute("INSERT dbo.DetalleReservas(id_reserva,id_habitacion,precio_noche,cantidad_personas) VALUES(?,?,1,1)",
                        result, room)
        return result

    def consumir(self, db=None, reservation=None, room=None, amount=1, price=Decimal("25.50"), operation=None):
        db = db or self.db
        db.cursor().execute("EXEC dbo.sp_DetalleServicio_Insertar ?,?,?,?,?,?",
                            self.service, reservation or self.reservation, amount, price,
                            room or self.rooms[0], str(operation or uuid.uuid4()))

    def fail_sql(self, number, function):
        with self.assertRaises(pyodbc.Error) as raised:
            function()
        self.assertRegex(str(raised.exception), r"\b" + str(number) + r"\b")

    def totals(self):
        cur = self.db.cursor()
        consumed = cur.execute("SELECT COALESCE(SUM(cantidad*precio_unitario),0) FROM dbo.DetalleServicios WHERE id_reserva=?",
                               self.reservation).fetchval()
        paid = cur.execute("""SELECT COALESCE(SUM(monto),0) FROM dbo.Pagos
WHERE id_reserva=? AND estado='APROBADO' AND tipo_pago='SERVICIOS_ADICIONALES'""", self.reservation).fetchval()
        return consumed, paid

    def test_una_y_varias_habitaciones_y_sin_duplicacion_de_joins(self):
        single = self.reserva("CHECKIN", 1)
        self.consumir(reservation=single)
        self.consumir()
        self.consumir(room=self.rooms[1])
        rows = self.db.cursor().execute("EXEC dbo.sp_DetalleServicio_ListarPorReserva ?", self.reservation).fetchall()
        self.assertEqual(2, len(rows))
        self.assertEqual(set(self.rooms[:2]), {row.id_habitacion for row in rows})
        filtered = self.db.cursor().execute("EXEC dbo.sp_DetalleServicio_ListarPorReserva ?,?",
                                           self.reservation, self.rooms[1]).fetchall()
        self.assertEqual(1, len(filtered))

    def test_habitacion_ajena_y_fk_directa(self):
        self.fail_sql(51316, lambda: self.consumir(room=self.rooms[2]))
        self.fail_sql(547, lambda: self.db.cursor().execute("""
INSERT dbo.DetalleServicios(id_servicios,id_reserva,id_habitacion,cantidad,precio_unitario,id_operacion)
VALUES(?,?,?,1,25.50,NEWID())""", self.service, self.reservation, self.rooms[2]))

    def test_servicio_inactivo_y_precio_desactualizado(self):
        self.fail_sql(51317, lambda: self.consumir(price=Decimal("20")))
        self.db.cursor().execute("UPDATE dbo.Servicios SET activo=0 WHERE id_servicio=?", self.service)
        self.fail_sql(51317, self.consumir)

    def test_reserva_sin_checkin(self):
        for state in ("CONFIRMADA", "CANCELADA", "FINALIZADA"):
            reservation = self.reserva(state, 1)
            self.fail_sql(51315, lambda: self.consumir(reservation=reservation))

    def test_reintento_y_payload_distinto_y_precio_historico(self):
        operation = uuid.uuid4()
        self.consumir(operation=operation)
        self.db.cursor().execute("UPDATE dbo.Servicios SET precio=30 WHERE id_servicio=?", self.service)
        self.consumir(operation=operation)
        self.fail_sql(51314, lambda: self.consumir(operation=operation, amount=2))
        self.assertEqual((Decimal("25.50"), Decimal("0")), self.totals())
        self.assertEqual(1, self.db.cursor().execute(
            "SELECT COUNT(*) FROM dbo.DetalleServicios WHERE id_operacion=?", str(operation)).fetchval())

    def test_historicos_sin_habitacion_se_cobran_una_vez(self):
        # Fixture NUEVO que representa el formato antiguo; no se reasocian registros existentes.
        self.db.cursor().execute("""
INSERT dbo.DetalleServicios(id_servicios,id_reserva,cantidad,precio_unitario) VALUES(?,?,2,10)""",
                                 self.service, self.reservation)
        self.consumir()
        rows = self.db.cursor().execute("EXEC dbo.sp_DetalleServicio_ListarPorReserva ?", self.reservation).fetchall()
        self.assertEqual(2, len(rows))
        self.assertEqual(1, sum(row.id_habitacion is None for row in rows))
        self.assertEqual(Decimal("45.50"), self.totals()[0])

    def parallel(self, functions):
        barrier = threading.Barrier(len(functions))
        def run(function):
            db = self.connect()
            try:
                barrier.wait(timeout=10)
                function(db)
                return "ok"
            except pyodbc.Error as error:
                # No publicar detalles de conexión en resultados.
                for code in (50002, 50008, 50024, 50025, 50030, 50032, 51315, 51410, 51411, 51413, 51414, 51420):
                    if re.search(r"\b" + str(code) + r"\b", str(error)):
                        return code
                raise AssertionError("Fallo SQL inesperado; revisar localmente sin compartir credenciales.") from None
            finally:
                db.close()
        with ThreadPoolExecutor(max_workers=len(functions)) as executor:
            return list(executor.map(run, functions))

    def pay(self, db, operation=None):
        return db.cursor().execute("EXEC dbo.sp_Pago_RegistrarServicios ?,?,?,?", self.reservation, self.user,
                                   "EFECTIVO", str(operation or uuid.uuid4())).fetchall()

    def checkout(self, db):
        db.cursor().execute("EXEC dbo.sp_Reserva_CheckOut ?", self.reservation).fetchall()

    def test_dos_cobros_concurrentes_no_duplican(self):
        self.consumir()
        self.assertCountEqual(["ok", 51413], self.parallel([self.pay, self.pay]))
        self.assertEqual(self.totals()[0], self.totals()[1])

    def test_reintentos_concurrentes_un_solo_consumo(self):
        operation = uuid.uuid4()
        self.assertEqual(["ok", "ok"], self.parallel([
            lambda db: self.consumir(db=db, operation=operation),
            lambda db: self.consumir(db=db, operation=operation)]))
        self.assertEqual(Decimal("25.50"), self.totals()[0])

    def test_consumo_concurrente_con_checkout(self):
        result = self.parallel([lambda db: self.consumir(db=db), self.checkout])
        state = self.db.cursor().execute("SELECT estado FROM dbo.Reservas WHERE id_reserva=?", self.reservation).fetchval()
        if state == "FINALIZADA":
            self.assertCountEqual([51315, "ok"], result)
            self.assertEqual(Decimal("0"), self.totals()[0])
        else:
            self.assertEqual("CHECKIN", state)
            self.assertCountEqual(["ok", 50032], result)

    def test_pago_concurrente_con_checkout(self):
        self.consumir()
        result = self.parallel([self.pay, self.checkout])
        self.assertEqual("ok", result[0])
        self.assertIn(result[1], ("ok", 50032))
        self.assertEqual(self.totals()[0], self.totals()[1])

    def test_consumo_concurrente_con_pago(self):
        result = self.parallel([lambda db: self.consumir(db=db), self.pay])
        self.assertEqual("ok", result[0])
        self.assertIn(result[1], ("ok", 51413))
        consumed, paid = self.totals()
        self.assertEqual(Decimal("25.50"), consumed)
        self.assertIn(paid, (Decimal("0"), consumed))


    def generic(self, db=None, amount=Decimal("25.50"), state="APROBADO", payment_type="SERVICIOS_ADICIONALES",
                operation=None, reservation=None, temporal=None):
        db = db or self.db
        # id_reserva must be NULL for a temporal.
        target = None if temporal is not None else (reservation or self.reservation)
        row = db.cursor().execute("EXEC dbo.sp_Pago_Insertar ?,?,?,?,?,?,?,?",
            target, temporal, self.user, amount, "EFECTIVO", state, payment_type, str(operation or uuid.uuid4())).fetchone()
        return row.id_pago

    def approve(self, payment, db=None, state="APROBADO"):
        (db or self.db).cursor().execute("EXEC dbo.sp_Pago_ActualizarEstado ?,?", payment, state)

    def test_alta_general_no_supera_saldo(self):
        self.consumir()
        self.fail_sql(51414, lambda: self.generic(amount=Decimal("26")))
        self.generic(amount=Decimal("20"))
        self.fail_sql(51414, lambda: self.generic(amount=Decimal("10")))
        self.assertEqual((Decimal("25.50"), Decimal("20")), self.totals())

    def test_dos_pendientes_solo_una_aprobacion(self):
        self.consumir()
        first = self.generic(state="PENDIENTE")
        second = self.generic(state="PENDIENTE")
        result = self.parallel([lambda db: self.approve(first, db), lambda db: self.approve(second, db)])
        self.assertCountEqual(["ok", 51413], result)
        self.assertEqual(self.totals()[0], self.totals()[1])

    def test_aprobacion_compite_con_cobro_especializado(self):
        self.consumir()
        pending = self.generic(state="PENDIENTE")
        result = self.parallel([lambda db: self.approve(pending, db), self.pay])
        self.assertCountEqual(["ok", 51413], result)
        self.assertEqual(self.totals()[0], self.totals()[1])

    def test_alta_general_compite_con_cobro_especializado(self):
        self.consumir()
        result = self.parallel([lambda db: self.generic(db=db), self.pay])
        self.assertCountEqual(["ok", 51413], result)
        self.assertEqual(self.totals()[0], self.totals()[1])

    def test_reintento_pago_no_cobra_consumos_posteriores(self):
        self.consumir()
        operation = uuid.uuid4()
        first = self.pay(self.db, operation)[0]
        self.consumir()
        replay = self.pay(self.db, operation)[0]
        self.assertEqual(first.id_pago, replay.id_pago)
        self.assertEqual((Decimal("51.00"), Decimal("25.50")), self.totals())
        self.fail_sql(51403, lambda: self.db.cursor().execute(
            "EXEC dbo.sp_Pago_RegistrarServicios ?,?,?,?", self.reservation, self.user, "OTRO", str(operation)))

    def test_reintentos_concurrentes_del_mismo_pago(self):
        self.consumir()
        operation = uuid.uuid4()
        self.assertEqual(["ok", "ok"], self.parallel([
            lambda db: self.pay(db, operation), lambda db: self.pay(db, operation)]))
        self.assertEqual(self.totals()[0], self.totals()[1])
        self.assertEqual(1, self.db.cursor().execute(
            "SELECT COUNT(*) FROM dbo.Pagos WHERE id_operacion=?", str(operation)).fetchval())

    def test_repetir_aprobacion_es_idempotente(self):
        self.consumir()
        payment = self.generic(state="PENDIENTE")
        self.approve(payment)
        self.approve(payment)
        self.fail_sql(50122, lambda: self.approve(payment, state="RECHAZADO"))
        self.assertEqual(self.totals()[0], self.totals()[1])

    def test_cierre_reintento_y_rechazo_de_nuevos_cargos(self):
        self.consumir()
        self.pay(self.db)
        self.checkout(self.db)
        self.checkout(self.db)  # stable reserve ID, no second state mutation
        self.fail_sql(51315, self.consumir)
        self.fail_sql(51410, self.generic)

    def test_pendiente_antiguo_no_se_aprueba_despues_del_cierre(self):
        self.consumir()
        payment = self.generic(state="PENDIENTE")
        self.pay(self.db)
        self.checkout(self.db)
        self.fail_sql(51410, lambda: self.approve(payment))
        self.assertEqual("PENDIENTE", self.db.cursor().execute(
            "SELECT estado FROM dbo.Pagos WHERE id_pago=?", payment).fetchval())

    def temporal(self, mode):
        entrada = date(2300, 1, 1) + timedelta(days=uuid.uuid4().int % 1000000)
        temporal = self.db.cursor().execute("EXEC dbo.sp_ReservaTemporal_Insertar ?,?,?,?,?",
            self.guest, self.rooms[2], entrada, entrada + timedelta(days=2), mode).fetchval()
        price = self.db.cursor().execute("""SELECT t.precio_noche FROM dbo.Habitaciones h
JOIN dbo.TiposHabitacion t ON t.id_tipoHabitacion=h.tipo_habitacion WHERE h.id_habitacion=?""", self.rooms[2]).fetchval()
        amount = price * 2 if mode == "PAGO_TOTAL" else price
        return temporal, amount

    def test_pago_inicial_50_y_100_confirmacion_y_reintento(self):
        for mode in ("SENA_50", "PAGO_TOTAL"):
            temporal, amount = self.temporal(mode)
            operation = uuid.uuid4()
            payment = self.generic(amount=amount, payment_type="PAGO_INICIAL", operation=operation, temporal=temporal)
            confirmed = self.db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal).fetchone()
            replay = self.generic(amount=amount, payment_type="PAGO_INICIAL", operation=operation, temporal=temporal)
            self.assertEqual(payment, replay)
            recovered = self.db.cursor().execute("EXEC dbo.sp_ReservaTemporal_ConsultarConfirmacion ?", temporal).fetchone()
            self.assertEqual(confirmed.id_reserva, recovered.id_reserva)
            self.fail_sql(50002, lambda: self.db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal))
            self.fail_sql(51416, lambda: self.generic(amount=amount, payment_type="PAGO_INICIAL", temporal=temporal))

    def test_dos_iniciales_simultaneos_no_duplican(self):
        temporal, amount = self.temporal("SENA_50")
        result = self.parallel([
            lambda db: self.generic(db=db, amount=amount, payment_type="PAGO_INICIAL", temporal=temporal),
            lambda db: self.generic(db=db, amount=amount, payment_type="PAGO_INICIAL", temporal=temporal)])
        self.assertCountEqual(["ok", 51420], result)

    def test_aprobar_inicial_pendiente_y_confirmar(self):
        temporal, amount = self.temporal("PAGO_TOTAL")
        payment = self.generic(amount=amount, state="PENDIENTE", payment_type="PAGO_INICIAL", temporal=temporal)
        self.approve(payment)
        confirmed = self.db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal).fetchone()
        self.approve(payment)  # routing changed, same durable payment ID
        self.assertIsNotNone(confirmed.id_reserva)

    def test_dos_confirmaciones_simultaneas(self):
        temporal, amount = self.temporal("PAGO_TOTAL")
        self.generic(amount=amount, payment_type="PAGO_INICIAL", temporal=temporal)
        def confirm(db):
            db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal).fetchall()
        self.assertCountEqual(["ok", 50002], self.parallel([confirm, confirm]))
        recovered = self.db.cursor().execute("EXEC dbo.sp_ReservaTemporal_ConsultarConfirmacion ?", temporal).fetchall()
        self.assertEqual(1, len(recovered))

    def test_aprobacion_inicial_concurrente_con_confirmacion(self):
        temporal, amount = self.temporal("PAGO_TOTAL")
        pending = self.generic(amount=amount, state="PENDIENTE", payment_type="PAGO_INICIAL", temporal=temporal)
        def confirm(db):
            db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal).fetchall()
        result = self.parallel([lambda db: self.approve(pending, db), confirm])
        self.assertEqual("ok", result[0])
        self.assertIn(result[1], ("ok", 50008))
        if result[1] == 50008:
            confirm(self.db)
        self.assertEqual(1, len(self.db.cursor().execute(
            "EXEC dbo.sp_ReservaTemporal_ConsultarConfirmacion ?", temporal).fetchall()))

    def test_alojamiento_saldos_checkin_y_checkout(self):
        temporal, amount = self.temporal("SENA_50")
        self.generic(amount=amount, payment_type="PAGO_INICIAL", temporal=temporal)
        confirmed = self.db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal).fetchone()
        self.fail_sql(50028, lambda: self.db.cursor().execute("EXEC dbo.sp_Reserva_CheckIn ?", confirmed.id_reserva))
        operation = str(uuid.uuid4())
        self.db.cursor().execute("EXEC dbo.sp_Pago_RegistrarSaldoAlojamiento ?,?,?,?",
                                 confirmed.id_reserva, self.user, "EFECTIVO", operation).fetchall()
        self.db.cursor().execute("EXEC dbo.sp_Reserva_CheckIn ?", confirmed.id_reserva).fetchall()
        self.db.cursor().execute("EXEC dbo.sp_Reserva_CheckOut ?", confirmed.id_reserva).fetchall()
        balance = self.db.cursor().execute("EXEC dbo.sp_Reserva_ConsultarSaldo ?", confirmed.id_reserva).fetchone()
        self.assertEqual(Decimal("0"), balance.total_pendiente)

    def test_dos_cobros_alojamiento_simultaneos(self):
        temporal, amount = self.temporal("SENA_50")
        self.generic(amount=amount, payment_type="PAGO_INICIAL", temporal=temporal)
        confirmed = self.db.cursor().execute("EXEC dbo.sp_ConfirmarReserva ?,1", temporal).fetchone()
        def collect(db):
            db.cursor().execute("EXEC dbo.sp_Pago_RegistrarSaldoAlojamiento ?,?,?,?",
                confirmed.id_reserva, self.user, "EFECTIVO", str(uuid.uuid4())).fetchall()
        self.assertCountEqual(["ok", 51413], self.parallel([collect, collect]))

    def test_historia_y_separacion_de_saldos(self):
        self.consumir()
        before = [tuple(row) for row in self.db.cursor().execute(
            "SELECT id_detalle_servicio,cantidad,precio_unitario FROM dbo.DetalleServicios WHERE id_reserva=?", self.reservation)]
        self.pay(self.db)
        self.assertEqual(before, [tuple(row) for row in self.db.cursor().execute(
            "SELECT id_detalle_servicio,cantidad,precio_unitario FROM dbo.DetalleServicios WHERE id_reserva=?", self.reservation)])
        payment = tuple(self.db.cursor().execute(
            "SELECT monto,tipo_pago FROM dbo.Pagos WHERE id_reserva=?", self.reservation).fetchone())
        self.assertEqual((Decimal("25.50"), "SERVICIOS_ADICIONALES"), payment)


if __name__ == "__main__":
    if "--ejecutar-en-copia-desechable" not in sys.argv:
        raise SystemExit("No ejecutado. Requiere --ejecutar-en-copia-desechable y configuración privada.")
    sys.argv.remove("--ejecutar-en-copia-desechable")
    import pyodbc
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(IntegridadSQL)
    if suite.countTestCases() != 28:
        raise SystemExit("La suite esperada debe contener exactamente 28 pruebas.")
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    summary = {"expected": 28, "executed": result.testsRun, "failures": len(result.failures),
               "errors": len(result.errors), "skipped": len(result.skipped)}
    if os.environ.get("HOTELTRAMONTO_TEST_RESULT_FILE"):
        with open(os.environ["HOTELTRAMONTO_TEST_RESULT_FILE"], "w", encoding="utf-8") as output:
            json.dump(summary, output)
    raise SystemExit(0 if result.wasSuccessful() and result.testsRun == 28 and not result.skipped else 1)
