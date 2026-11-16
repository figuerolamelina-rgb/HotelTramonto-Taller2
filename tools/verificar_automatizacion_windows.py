"""Valida contratos estáticos del runner Windows; no ejecuta PowerShell ni SQL."""
from pathlib import Path
import hashlib
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / 'tools/Probar-HotelTramonto.ps1'
TEXT = SCRIPT.read_text(encoding='utf-8-sig')


class AutomatizacionWindows(unittest.TestCase):
    def test_manifiesto_coincide_con_archivos_revisados(self):
        entries = re.findall(r"^\s*'([^']+)'='([A-F0-9]{64})'", TEXT, re.M)
        self.assertGreaterEqual(len(entries), 40)
        for name, digest in entries:
            with self.subTest(file=name):
                self.assertEqual(digest, hashlib.sha256((ROOT / name).read_text(encoding='utf-8-sig').replace('\r\n', '\n').replace('\r', '\n').encode()).hexdigest().upper())

    def test_lista_sql_cerrada_excluye_original_permisos_y_reversiones(self):
        block = TEXT.split('$ScriptsAutorizados=@(')[1].split(')')[0]
        names = re.findall(r"'([^']+)'", block)
        self.assertEqual(7, len(names))
        self.assertTrue(all('compatibilidad-servicios/' in n or 'permisos-habitaciones/' in n for n in names))
        self.assertFalse(any('PermisosMinimos' in n or 'Revertir' in n or 'Procedimientos_Almacenados' in n for n in names))
        self.assertIn('$ScriptsAutorizados -ccontains $Relativo', TEXT)

    def test_lista_positiva_pruebas_no_contiene_sql_real(self):
        match = re.search(r"\$FiltroSeguro='([^']+)'", TEXT)
        self.assertIsNotNone(match)
        entries = match[1].split('|')
        self.assertEqual(45, len(entries))
        self.assertTrue(all(n.startswith('FullyQualifiedName=HotelTramonto.Pruebas.') for n in entries))
        for excluded in ('ConexionBD_ConectaConHotelTramontoDB',
                         'UsuarioNegocios_CambiarContrasena_GuardaHashValido',
                         'UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull'):
            self.assertNotIn(excluded, match[1])
        self.assertIn('[int]$cuenta.passed -eq 56', TEXT)
        self.assertIn("$env:HOTELTRAMONTO_CONFIG=Join-Path $Salida 'conexion-bloqueada-inexistente.json'", TEXT)
        self.assertIn('$env:HOTELTRAMONTO_CONFIG=$configAnterior', TEXT)

    def test_constructor_usa_indexadores_y_prueba_no_abre_conexion(self):
        body = TEXT.split('function Crear-CadenaConexion(')[1].split('function Abrir(')[0]
        for key in ('Data Source', 'Initial Catalog', 'Integrated Security', 'Encrypt',
                    'TrustServerCertificate', 'Persist Security Info', 'Connect Timeout', 'Application Name'):
            self.assertIn("$b['" + key + "']=", body)
        self.assertNotRegex(body, r'\$b\.\w+\s*=')
        self.assertNotIn('.Open(', body)
        test = (ROOT / 'tools/Probar-CadenaConexion.ps1').read_text(encoding='utf-8-sig')
        self.assertNotIn('.Open(', test)
        self.assertIn('#requires -Version 5.1', test)
        self.assertIn("$base in @('master','HotelTramontoDB_Pruebas')", test)
        self.assertLess(TEXT.index("& (Join-Path $PSScriptRoot 'Probar-CadenaConexion.ps1')"),
                        TEXT.index("$Maestra=Abrir 'master'"))

    def test_referencia_de_habitaciones_exige_activas_y_test_sin_sql(self):
        sql = (ROOT / 'base de datos HotelTramonto/Procedimientos_Almacenados.sql').read_text(encoding='utf-8-sig')
        block = re.search(r'(?ims)^CREATE OR ALTER PROCEDURE sp_Habitacion_Listar\b.*?(?=^GO\s*$)', sql)[0]
        self.assertEqual(1, len(re.findall(r'WHERE H\.activo = 1', block)))
        self.assertIn('ORDER BY H.numero;', block)
        test = (ROOT / 'tools/Probar-LecturasSql.ps1').read_text(encoding='utf-8-sig')
        self.assertNotIn('.Open(', test)
        self.assertNotIn('SqlConnection', test)
        self.assertIn('$sinFiltro', test)
        self.assertIn('$inactivas', test)
        self.assertRegex(test, r"\$huella -cne '[A-F0-9]{64}'")
        self.assertLess(TEXT.index("& (Join-Path $PSScriptRoot 'Probar-LecturasSql.ps1')"), TEXT.index("$Maestra=Abrir 'master'"))
        self.assertIn("'Lectura SQL diferente de la revisada: '+$nombre", TEXT)

    def test_sesiones_identidad_exacta_transitorios_y_diagnostico(self):
        self.assertIn("$b['Pooling']=$false", TEXT)
        self.assertIn('$AplicacionSql', TEXT)
        logic = TEXT.split('function Clasificar-Sesiones(')[1].split('function EjecutarArchivo(')[0]
        for field in ('connection_id', 'session_id', 'program_name', 'host_process_id', 'host_name'):
            self.assertIn(field, logic)
        self.assertIn('$limpias -ge 2', logic)
        self.assertIn('$intento -le 3', logic)
        self.assertIn('sys.dm_exec_requests', logic)
        self.assertIn('ConvertTo-Json', logic)
        self.assertNotRegex(logic, r'(?i)\bKILL\b|Stop-Process')
        test = (ROOT / 'tools/Probar-SesionesSql.ps1').read_text(encoding='utf-8-sig')
        self.assertNotIn('.Open(', test)
        self.assertIn("Sesion 80 'otra' 'runner-A' 123", test)
        self.assertIn("Sesion 52 'conexion-nueva'", test)

    def test_confirmacion_antecede_instalacion_y_origen_solo_metadatos(self):
        self.assertLess(TEXT.index("$respuesta=Read-Host"), TEXT.index("foreach ($n in @('01_AgregarCategoria.sql'"))
        self.assertIn("$respuesta -ceq 'MIGRAR HotelTramontoDB_Pruebas'", TEXT)
        self.assertIn("$Maestra=Abrir 'master'", TEXT)
        self.assertIn('$Conexion=Abrir $Pruebas', TEXT)
        self.assertNotIn('Abrir $Original', TEXT)
        self.assertNotRegex(TEXT, r'(?i)\b(?:DELETE|UPDATE|INSERT|ALTER|DROP)\s+(?:DATABASE\s+)?HotelTramontoDB\b')
        self.assertTrue(SCRIPT.read_bytes().startswith(b'\xef\xbb\xbf'))


if __name__ == '__main__':
    unittest.main(verbosity=2)
