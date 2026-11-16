"""Comprobaciones estáticas de seguridad. No conecta ni ejecuta SQL Server."""
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
SQL = ROOT / 'base de datos HotelTramonto'
FOLDERS = ('compatibilidad-servicios', 'permisos-habitaciones')
FILES = sorted(p for folder in FOLDERS for p in (SQL / folder).glob('*.sql'))


def sin_comentarios(texto):
    return re.sub(r'--[^\n]*', '', texto).strip()


class SeguridadScripts(unittest.TestCase):
    def test_todos_los_lotes_exigen_base_de_pruebas_antes_de_operar(self):
        self.assertEqual(11, len(FILES))
        guard = re.compile(
            r"\AIF DB_NAME\(\) COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'"
            r"\s+BEGIN\s+THROW 51200, '[^']+', 1;\s+END;", re.S)
        for path in FILES:
            for batch in re.split(r'(?im)^GO\s*$', path.read_text(encoding='utf-8')):
                code = sin_comentarios(batch)
                if code:
                    with self.subTest(file=path.name, batch=code[:80]):
                        self.assertRegex(code, guard)

    def test_no_hay_cambio_de_contexto_ni_referencias_a_base_original(self):
        for path in FILES:
            code = sin_comentarios(path.read_text(encoding='utf-8'))
            with self.subTest(file=str(path)):
                self.assertNotRegex(code, r'(?im)^\s*USE\b')
                self.assertNotRegex(code, r'(?i)\bHotelTramontoDB\s*\.')

    def test_clasificacion_solo_id_uno_otros_y_actualiza_solo_categoria(self):
        code = sin_comentarios((SQL / FOLDERS[0] / '02_ClasificarServicios.sql').read_text())
        entries = re.findall(r'INSERT INTO @Clasificaciones[^;]+;', code)
        self.assertEqual(1, len(entries))
        self.assertRegex(entries[0], r"VALUES\s*\(1, 'Otros'\);")
        self.assertIn('(SELECT COUNT(*) FROM @Clasificaciones) <> 1', code)
        self.assertIn("id_servicio=1 AND categoria COLLATE Latin1_General_100_BIN2='Otros'", code)
        assignments = re.findall(r'UPDATE\s+s\s+SET\s+(.*?)\s+FROM', code, re.S)
        self.assertEqual(['categoria=c.categoria'], assignments)
        self.assertIn('WHERE s.categoria IS NULL AND s.id_servicio=1;', code)
        self.assertIn("id_servicio=1 AND nombre='Desayuno Buffet' AND precio=9500.00 AND activo=0", code)
        self.assertNotRegex(code, r'(?i)\b(?:DELETE|TRUNCATE|DROP)\b')

    def test_instalacion_permisos_bloqueada_antes_de_cambiar_permisos(self):
        code = sin_comentarios((SQL / FOLDERS[1] / '02_PermisosMinimos.sql').read_text())
        self.assertIn("THROW 51201, 'Instalación de permisos pendiente: no ejecutar en esta etapa.', 1;", code)
        self.assertLess(code.index('THROW 51201'), code.index('BEGIN TRANSACTION'))

    def test_listas_de_procedimientos_preservan_todas_las_rutas_de_datos(self):
        actual = set()
        for path in (ROOT / 'HotelTramonto.Datos').glob('*.cs'):
            actual.update(n.removeprefix('dbo.') for n in re.findall(
                r'"((?:dbo\.)?sp_\w+)"', path.read_text(encoding='utf-8-sig')))
        adicionales = {'sp_Huesped_HistorialReservas', 'sp_ReservaTemporal_ConsultarConfirmacion'}
        self.assertEqual(62, len(actual))
        self.assertTrue(adicionales <= actual)
        # La instalación/reversión originales de corrección 2 conservan su alcance 60.
        # Las dos rutas nuevas se conceden por un complemento separado, no se borran al revertir 2.
        for name in ('02_PermisosMinimos.sql', '04_RevertirPermisos.sql'):
            code = (SQL / FOLDERS[1] / name).read_text()
            self.assertEqual(actual - adicionales, set(re.findall(r"\(N'(sp_\w+)'\)", code)))

    def test_complemento_solo_agrega_dos_execute_y_respeta_las_bases(self):
        carpeta = SQL / 'integracion'
        instalador = (carpeta / '01_PermisosConsultasAdicionales_Pruebas.sql').read_text()
        codigo = sin_comentarios(instalador)
        self.assertEqual({'sp_Huesped_HistorialReservas', 'sp_ReservaTemporal_ConsultarConfirmacion'},
                         set(re.findall(r'GRANT EXECUTE ON OBJECT::dbo\.(sp_\w+) TO', codigo)))
        self.assertNotRegex(codigo, r'(?i)\b(?:REVOKE|ALTER ROLE|CREATE ROLE|DROP ROLE|USE)\b')
        self.assertNotRegex(codigo, r'(?i)GRANT\s+(?:SELECT|INSERT|UPDATE|DELETE|CONTROL)')
        for path in carpeta.glob('*.sql'):
            self.assertIn("DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'", path.read_text())
        verificador = (carpeta / '02_VerificarConsultasAdicionales_Pruebas.sql').read_text()
        self.assertNotRegex(sin_comentarios(verificador), r'(?i)\b(?:GRANT|DENY|REVOKE|EXEC|EXECUTE|ALTER|DROP)\s')


if __name__ == '__main__':
    unittest.main(verbosity=2)
