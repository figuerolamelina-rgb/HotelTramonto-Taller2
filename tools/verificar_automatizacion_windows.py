"""Pruebas locales de las barreras de la automatización; no ejecutan PowerShell ni SQL."""
import ast
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "tools/Probar-ServiciosHabitaciones.ps1"
SUITE = ROOT / "tests/servicios_habitaciones_sql.py"


class AutomatizacionTests(unittest.TestCase):
    def setUp(self):
        self.ps = SCRIPT.read_text(encoding="utf-8-sig")
        self.py = SUITE.read_text()
        self.tree = ast.parse(self.py)

    def test_exactly_28_real_cases_and_checked_outcome(self):
        cases = [n for n in ast.walk(self.tree) if isinstance(n, ast.FunctionDef) and n.name.startswith("test_")]
        self.assertEqual(28, len(cases))
        self.assertIn("suite.countTestCases() != 28", self.py)
        self.assertIn("result.testsRun == 28 and not result.skipped", self.py)
        self.assertIn("$summary.executed -ne 28", self.ps)
        self.assertIn("$summary.errors -ne 0", self.ps)
        self.assertIn("[int]$counters.executed -le 0", self.ps)
        self.assertIn("tests-dotnet.trx", self.ps)

    def test_no_source_server_parameter_or_connection(self):
        parameters = self.ps.split("Set-StrictMode", 1)[0]
        self.assertNotIn("$Server", parameters)
        self.assertNotIn("$ConnectionString", parameters)
        self.assertIn("$builder.DataSource = $script:pipe", self.ps)
        self.assertIn("$builder.IntegratedSecurity = $true", self.ps)
        self.assertNotIn("TrustServerCertificate", self.ps)
        self.assertIn("$metadata.Tables[0].Rows[0].EsLocalDB -ne 1", self.ps)

    def test_named_pipe_guard_rejects_remote_and_existing_names(self):
        pattern = re.search(r"\$script:pipe -notmatch '([^']+)'", self.ps)[1]
        self.assertRegex(r"np:\\.\pipe\LOCALDB#12345678\tsql\query", pattern)
        for value in (r"np:\\remote\pipe\LOCALDB#abc\tsql\query", "localhost", r"(localdb)\MSSQLLocalDB"):
            self.assertIsNone(re.match(pattern, value))
        self.assertIn("$script:instance -notmatch '^HT56_[a-f0-9]{32}$'", self.ps)

    def test_explicit_confirmation_precedes_installation(self):
        prompt = self.ps.index("$answer = Read-Host")
        self.assertLess(prompt, self.ps.index("foreach ($script in @('01_Instalar.sql'"))
        self.assertIn("if ($answer -cne 'INSTALAR') { throw", self.ps)
        self.assertNotRegex(self.ps, r"(?i)\[switch\].*(?:force|yes|confirm)")
        self.assertIn("Invoke-Migration '00_Diagnostico.sql'", self.ps)

    def test_disposable_restore_and_cleanup_even_on_failure(self):
        self.assertIn("Invoke-Checked $localDb @('create', $instance)", self.ps)
        self.assertIn("$created = $true", self.ps)
        self.assertIn("RESTORE DATABASE [HotelTramontoDB_Pruebas]", self.ps)
        self.assertNotRegex(self.ps, r"(?i)\bWITH\s+REPLACE\b")
        self.assertNotRegex(self.ps, r"(?im)^\s*.*BACKUP DATABASE")
        self.assertIn("finally {", self.ps)
        self.assertIn("& $localDb stop $instance -k", self.ps)
        self.assertIn("& $localDb delete $instance", self.ps)
        self.assertIn("Remove-Item -LiteralPath $dataDir", self.ps)
        self.assertNotIn("Remove-Item -LiteralPath $backup", self.ps)

    def test_history_compared_before_and_after_and_environment_restored(self):
        self.assertEqual(2, self.ps.count("Assert-HistoryUnchanged $baseline"))
        self.assertIn("$current[$key] -cne $before.Rows[$key]", self.ps)
        self.assertIn("$env:HOTELTRAMONTO_SQL_TEST_CONNECTION = $previousConnection", self.ps)
        self.assertIn("'HotelTramonto\\Verificaciones\\'", self.ps)
        self.assertIn("SERVERPROPERTY('IsLocalDB')", self.py)
        self.assertIn('self.assertEqual(run_id, marker', self.py)

    def test_script_exit_status_and_install_order(self):
        self.assertIn("$exitCode = $LASTEXITCODE", self.ps)
        self.assertIn("if ($exitCode -ne 0)", self.ps)
        self.assertIn("@('01_Instalar.sql', '02_Verificar.sql', '04_ConcurrenciaPagos.sql', '05_VerificarConcurrencia.sql')", self.ps)
        self.assertNotRegex(self.ps, r"Invoke-Migration '0[36]_")


if __name__ == "__main__":
    unittest.main()
