"""Regresión estática de la alternativa Express/.NET; Python NO se necesita para ejecutarla."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CODE = ROOT / "tests/HotelTramonto.VerificacionSql"


class AutomatizacionTests(unittest.TestCase):
    def setUp(self):
        self.ps = (ROOT / "tools/Probar-ServiciosHabitaciones.ps1").read_text(encoding="utf-8-sig")
        self.seguridad = (CODE / "SeguridadCopia.cs").read_text()
        self.runner = (CODE / "Ejecutor.cs").read_text()
        self.cases = (CODE / "CasosSql.cs").read_text()
        self.sql = (CODE / "Sql.cs").read_text()

    def test_28_cases_and_report(self):
        self.assertEqual(28, len(re.findall(r'\("\d\d_[^"]+", c =>', self.cases)))
        self.assertIn("informe.Aprobadas == casos.Length", self.runner)
        self.assertIn("CasosSql.Todos.Concat(CasosSql.Ciclo)", self.runner)
        self.assertIn('Estado { get; set; } = "Pendiente"', self.runner)
        self.assertIn('informe.Pruebas[i].Estado = "Fallida"', self.runner)

    def test_no_extra_programs(self):
        self.assertNotIn("SqlLocalDB.exe", self.ps)
        self.assertNotIn("pyodbc", self.ps)
        self.assertNotIn("ODBC Driver", self.ps)
        self.assertNotIn("& python", self.ps)
        self.assertIn("HotelTramonto.VerificacionSql", self.ps)
        self.assertNotIn("PackageReference", (CODE / "HotelTramonto.VerificacionSql.csproj").read_text())

    def test_only_generated_copy_and_matching_marker(self):
        self.assertIn('Prefijo = "HotelTramontoDB_HT56_"', self.seguridad)
        self.assertIn('Ejecucion.ToString("N")', self.seguridad)
        self.assertIn("sys.extended_properties", self.seguridad)
        self.assertIn("LOWER(REPLACE(CONVERT(nvarchar(36),@HT_Run)", self.seguridad)
        self.assertIn("Solo se admite", self.runner)
        self.assertIn("lpc:", self.sql)

    def test_confirmation_and_order(self):
        self.assertLess(self.runner.index('Console.ReadLine() != "INSTALAR"'),
                        self.runner.index('foreach (string nombre in new[]'))
        self.assertIn('"01_Instalar.sql", "02_Verificar.sql", "04_ConcurrenciaPagos.sql", "05_VerificarConcurrencia.sql"', self.runner)
        self.assertNotIn("03_Recuperacion.sql", self.runner)
        self.assertNotIn("06_SuspenderEscrituras.sql", self.runner)

    def test_restore_never_replaces_files_or_databases(self):
        # Comments can describe the forbidden option; the SQL issued never contains it.
        sql_lines = "\n".join(line for line in self.runner.splitlines() if not line.lstrip().startswith("//"))
        self.assertNotRegex(sql_lines, r"(?i)\bWITH\s+REPLACE\b")
        self.assertIn("xp_fileexist", self.runner)
        self.assertIn("sys.master_files", self.runner)
        self.assertIn("La base ya existe; no se restaura", self.runner)
        self.assertIn("InstanceDefaultDataPath", self.runner)

    def test_cleanup_requires_full_identity_without_killing_sessions(self):
        self.assertIn("if (!marcada || idCreado is null || fechaCreada is null || propietario is null)", self.runner)
        self.assertIn("create_date=@p2 AND owner_sid=@p3", self.runner)
        self.assertIn("sys.dm_exec_sessions", self.runner)
        self.assertIn("SET SINGLE_USER WITH NO_WAIT", self.runner)
        self.assertNotIn("ROLLBACK IMMEDIATE", self.runner)
        self.assertIn("ExigirArchivos(master);", self.runner)

    def test_independent_connections_and_history(self):
        self.assertIn("copia.Abrir()", self.cases)
        self.assertIn("new Barrier", self.cases)
        self.assertIn("Task.WhenAll", self.cases)
        self.assertIn("Pooling = false", self.sql)
        history = (CODE / "Historicos.cs").read_text()
        self.assertIn("SHA256.HashData", history)
        self.assertIn("original.Filas", history)
        self.assertEqual(3, self.runner.count("historicos.Verificar(Copia)"))  # Inicio, antes de UI y final.


if __name__ == "__main__":
    unittest.main()
