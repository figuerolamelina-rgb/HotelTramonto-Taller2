"""Comprobaciones estáticas de scripts; no sustituyen pruebas en SQL Server."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SQL = ROOT / "base de datos HotelTramonto/servicios-habitaciones"


def definitions(script):
    return [s.replace("''", "'") for s in re.findall(
        r"EXEC sys\.sp_executesql N'((?:''|[^'])*)';", script, re.S)]


class ScriptsTests(unittest.TestCase):
    def setUp(self):
        self.install = (SQL / "01_Instalar.sql").read_text()
        self.procs = {re.search(r"PROCEDURE (?:dbo\.)?(\w+)", s).group(1): s
                      for s in definitions(self.install)}
        self.insert = self.procs["sp_DetalleServicio_Insertar"]

    def test_protected_atomic_and_no_historical_backfill(self):
        for file in SQL.glob("*.sql"):
            text = file.read_text()
            self.assertIn("DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'", text)
            self.assertNotRegex(text, r"(?im)^\s*(GO|USE)\b")
        self.assertIn("BEGIN TRANSACTION", self.install)
        self.assertIn("ROLLBACK TRANSACTION", self.install)
        ddl = self.install.split("EXEC sys.sp_executesql", 1)[0]
        self.assertNotRegex(ddl, r"(?i)\b(DELETE|TRUNCATE)\b")
        self.assertNotRegex(ddl, r"(?i)UPDATE\s+dbo\.DetalleServicios")

    def test_composite_fk_and_operation_constraint(self):
        self.assertIn("FOREIGN KEY(id_reserva,id_habitacion) REFERENCES dbo.DetalleReservas(id_reserva,id_habitacion)", self.install)
        self.assertIn("ADD id_habitacion INT NULL", self.install)
        self.assertIn("ADD id_operacion UNIQUEIDENTIFIER NULL", self.install)
        self.assertIn("CREATE UNIQUE INDEX UX_DetalleServicios_Operacion", self.install)
        self.assertIn("WHERE id_operacion IS NOT NULL", self.install)
        self.assertIn("CK_DetalleServicios_AsociacionOperacion", self.install)

    def test_category_is_prerequisite_not_reimplemented(self):
        self.assertIn("sp_Servicio_Listar", self.install)
        self.assertIn("Faltan procedimientos compatibles con categoria", self.install)
        self.assertNotRegex(self.install, r"(?i)ALTER TABLE dbo\.Servicios")
        self.assertNotIn("sp_Servicio_Insertar", self.procs)

    def test_retry_checks_payload_before_current_catalog(self):
        for predicate in ("id_reserva=@id_reserva", "id_habitacion=@id_habitacion",
                          "id_servicios=@id_servicios", "cantidad=@cantidad",
                          "precio_unitario=@precio_unitario"):
            self.assertIn(predicate, self.insert)
        self.assertLess(self.insert.index("IF @existente IS NOT NULL"),
                        self.insert.index("SELECT @precio=precio"))
        self.assertIn("51314", self.insert)

    def test_server_checks_and_stores_catalog_price(self):
        for predicate in ("estado='CHECKIN'", "activo=1", "categoria<>'Incluido'",
                          "@precio<>@precio_unitario", "dbo.DetalleReservas WITH (UPDLOCK,HOLDLOCK)"):
            self.assertIn(predicate, self.insert)
        self.assertIn("@cantidad,@precio,@id_operacion", self.insert)
        self.assertNotRegex(self.insert, r"(?i)\bUPDATE\b")

    def test_shared_transaction_lock_and_only_authorized_procedures(self):
        self.assertEqual({"sp_DetalleServicio_Insertar", "sp_DetalleServicio_ListarPorReserva",
                          "sp_Pago_RegistrarServicios", "sp_Reserva_CheckOut"}, set(self.procs))
        for name in ("sp_DetalleServicio_Insertar", "sp_Pago_RegistrarServicios", "sp_Reserva_CheckOut"):
            body = self.procs[name]
            self.assertLess(body.index("BEGIN TRANSACTION"), body.index("sp_getapplock"))
            self.assertIn("HotelTramonto:reserva:", body)
            self.assertIn("@LockOwner='Transaction'", body)
            self.assertIn("ROLLBACK", body)
            self.assertIn("COMMIT", body)

    def test_billing_uses_historical_prices_without_room_join(self):
        for name in ("sp_Pago_RegistrarServicios", "sp_Reserva_CheckOut"):
            body = self.procs[name]
            self.assertIn("SUM(cantidad * precio_unitario)", body)
            self.assertIn("tipo_pago = 'SERVICIOS_ADICIONALES'", body)
            self.assertNotIn("JOIN", body)
        listing = self.procs["sp_DetalleServicio_ListarPorReserva"]
        self.assertIn("LEFT JOIN dbo.Habitaciones H ON H.id_habitacion=DS.id_habitacion", listing)
        self.assertIn("DS.cantidad*DS.precio_unitario", listing)

    def test_recovery_keeps_data_and_blocks_new_consumption(self):
        recovery = (SQL / "03_Recuperacion.sql").read_text()
        self.assertIn("Registro de consumos suspendido", recovery)
        self.assertNotRegex(recovery, r"(?i)\b(DROP|TRUNCATE|DELETE)\b")
        self.assertNotRegex(recovery, r"(?i)ALTER TABLE")


if __name__ == "__main__":
    unittest.main()
