"""Verifica los contratos de despliegue del historial sin conectarse a SQL."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "base de datos HotelTramonto"


class HistorialSqlTests(unittest.TestCase):
    def setUp(self):
        self.canonical = (ROOT / "Actualizacion_HistorialHuespedes.sql").read_text()
        self.installer = (ROOT / "pruebas/Instalar_HistorialHuespedes_Pruebas.sql").read_text()
        self.checks = (ROOT / "pruebas/HistorialHuespedes_Consulta.sql").read_text()
        self.body = self.canonical.split("CREATE OR ALTER PROCEDURE", 1)[1].rsplit("\nGO", 1)[0]
        self.body = "CREATE OR ALTER PROCEDURE" + self.body

    def test_installer_contains_same_definition(self):
        embedded = self.installer.split("EXEC sys.sp_executesql N'\n", 1)[1].rsplit("';", 1)[0]
        self.assertEqual(self.body.strip(), embedded.replace("''", "'").strip())

    def test_test_scripts_guard_database_before_execution(self):
        guard = "IF DB_NAME() <> N'HotelTramontoDB_Pruebas'"
        for script, execution in [(self.installer, "EXEC sys.sp_executesql"),
                                  (self.checks, "CREATE TABLE #Historial")]:
            self.assertLess(script.index(guard), script.index(execution))
            self.assertIn("THROW 51000", script)
            self.assertNotRegex(script, r"(?im)^\s*USE\s")
        # DDL is inside ELSE in the same batch as the guard.
        before_ddl = self.installer.split("EXEC sys.sp_executesql", 1)[0]
        self.assertRegex(before_ddl, r";\s*ELSE\s*$")
        self.assertNotRegex(before_ddl, r"(?im)^\s*GO\s*$")

    def test_reusable_procedure_has_no_database_restriction_or_writes(self):
        self.assertNotRegex(self.body, r"(?i)DB_NAME|HotelTramontoDB")
        self.assertNotRegex(self.body, r"(?i)\b(INSERT|UPDATE|DELETE|MERGE|DROP|TRUNCATE|GRANT|USE)\b")
        self.assertIn("WHERE r.id_huesped = @id_huesped", self.body)
        self.assertIn("@id_huesped IS NULL OR @id_huesped <= 0", self.body)

    def test_columns_exist_in_versioned_schema(self):
        schema = (ROOT / "Creacion_Base_HotelTramonto.sql").read_text()
        for alias, table in {"r": "Reservas", "h": "Huespedes", "d": "DetalleReservas",
                             "hab": "Habitaciones", "t": "TiposHabitacion"}.items():
            table_body = re.search(r"CREATE TABLE " + table + r"\s*\((.*?)\n\);", schema, re.S).group(1)
            columns = set(re.findall(r"^\s*(\w+)\s+(?:INT|VARCHAR|DATE|DATETIME|BIT|DECIMAL)", table_body, re.M))
            used = set(re.findall(r"\b" + alias + r"\.(\w+)", self.body))
            self.assertFalse(used - columns, (table, used - columns))


if __name__ == "__main__":
    unittest.main()
