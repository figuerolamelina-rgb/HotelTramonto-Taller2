"""Regresión estática sin SQL Server. No prueba ejecución ni ausencia absoluta de deadlocks."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SQL = ROOT / "base de datos HotelTramonto/servicios-habitaciones"


def definitions(path):
    return {re.search(r"PROCEDURE (?:dbo\.)?(\w+)", text).group(1): text for text in
            (s.replace("''", "'") for s in re.findall(r"EXEC sys\.sp_executesql N'((?:''|[^'])*)';",
                                                     path.read_text(), re.S))}


class ConcurrenciaTests(unittest.TestCase):
    def setUp(self):
        self.script = (SQL / "04_ConcurrenciaPagos.sql").read_text()
        self.procs = definitions(SQL / "04_ConcurrenciaPagos.sql")
        self.insert = self.procs["sp_Pago_Insertar"]
        self.approve = self.procs["sp_Pago_ActualizarEstado"]

    def test_single_payment_writer_and_wrappers(self):
        for name in ("sp_Pago_RegistrarServicios", "sp_Pago_RegistrarSaldoAlojamiento"):
            self.assertIn("EXEC dbo.sp_Pago_Insertar", self.procs[name])
            self.assertNotIn("INSERT", self.procs[name])
            self.assertIn("@id_operacion=@id_operacion", self.procs[name])
        self.assertIn("@monto=NULL", self.procs["sp_Pago_RegistrarServicios"])

    def test_transaction_order(self):
        self.assertLess(self.insert.index("HotelTramonto:pago-operacion:"), self.insert.index("HotelTramonto:reserva:"))
        self.assertLess(self.approve.index("HotelTramonto:reserva:"),
                        self.approve.index("FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK) WHERE id_pago="))
        for name in ("sp_Pago_Insertar", "sp_Pago_ActualizarEstado", "sp_DetalleServicio_Insertar", "sp_Reserva_CheckOut"):
            text = self.procs[name]
            self.assertIn("BEGIN TRANSACTION", text)
            self.assertIn("@LockOwner='Transaction'", text)
            self.assertIn("ROLLBACK", text)

    def test_balance_revalidated_in_both_writers(self):
        for body in (self.insert, self.approve):
            self.assertIn("IF @monto>@saldo", body)
            self.assertIn("tipo_pago='SERVICIOS_ADICIONALES'", body)
            self.assertIn("tipo_pago IN ('PAGO_INICIAL','SALDO_ALOJAMIENTO')", body)
            self.assertIn("SUM(CAST(cantidad AS DECIMAL(19,2))*precio_unitario)", body)
            self.assertIn("@fecha_cierre IS NOT NULL", body)

    def test_stable_request_identity_survives_confirmation(self):
        self.assertIn("HASHBYTES('SHA2_256',@solicitud)", self.insert)
        self.assertLess(self.insert.index("IF @existente IS NOT NULL"),
                        self.insert.index("FROM dbo.Reservas WITH"))
        self.assertIn("@hash_previo<>@hash", self.insert)
        self.assertIn("@monto AS monto", self.insert)
        self.assertNotRegex(self.script, r"(?i)UPDATE\s+(?:dbo\.)?Pagos\s+SET\s+(?:monto|solicitud_hash)")

    def test_temporal_compatibility_and_approval_retry(self):
        for body in (self.insert, self.approve):
            self.assertIn("dbo.ReservasTemporales WITH (UPDLOCK,HOLDLOCK)", body)
            self.assertIn("@modalidad='SENA_50'", body)
            self.assertIn("ROUND(@total*0.50,2)", body)
            self.assertIn("La temporal ya tiene un pago inicial aprobado", body)
        self.assertIn("IF @actual=@estado BEGIN COMMIT; RETURN; END;", self.approve)
        self.assertNotIn("sp_ConfirmarReserva", self.procs)

    def test_safe_deployment_recovery_and_canonical_definitions(self):
        first = definitions(SQL / "01_Instalar.sql")
        for name in ("sp_DetalleServicio_Insertar", "sp_Reserva_CheckOut"):
            self.assertEqual(first[name], self.procs[name])
        self.assertIn("51434", (SQL / "01_Instalar.sql").read_text())
        self.assertIn("51434", (SQL / "03_Recuperacion.sql").read_text())
        freeze = (SQL / "06_SuspenderEscrituras.sql").read_text()
        self.assertEqual(6, len(definitions(SQL / "06_SuspenderEscrituras.sql")))
        self.assertNotRegex(freeze, r"(?i)\b(DELETE|DROP|TRUNCATE)\b")
        self.assertNotRegex(self.script, r"(?i)ALTER TABLE dbo\.Servicios")

    def test_inventory_original_payment_writers(self):
        source = (ROOT / "base de datos HotelTramonto/Procedimientos_Almacenados.sql").read_text()
        writers = []
        for match in re.finditer(r"(?:CREATE OR ALTER|CREATE) PROCEDURE (?:dbo\.)?(\w+)(.*?)(?=\nGO)", source, re.S):
            if re.search(r"(?i)\b(?:INSERT(?: INTO)?|UPDATE)\s+(?:dbo\.)?Pagos\b", match[2]):
                writers.append(match[1])
        self.assertEqual({"sp_Pago_Insertar", "sp_Pago_ActualizarEstado", "sp_Pago_RegistrarServicios",
                          "sp_Pago_RegistrarSaldoAlojamiento", "sp_ConfirmarReserva"}, set(writers))
        # Confirmation only relinks an existing approved initial payment; no new amount.
        confirmation = re.search(r"PROCEDURE sp_ConfirmarReserva(.*?)(?=\nGO)", source, re.S)[1]
        self.assertNotRegex(confirmation, r"(?i)INSERT(?: INTO)?\s+(?:dbo\.)?Pagos")
        self.assertIn("WHERE id_pago = @id_pago", confirmation)


if __name__ == "__main__":
    unittest.main()
