"""Contratos estáticos del verificador final: no ejecuta PowerShell ni SQL."""
from pathlib import Path
import hashlib
import json
import re
import unittest
ROOT = Path(__file__).resolve().parents[1]


def norm(p):
    return p.read_text(encoding='utf-8-sig').replace('\r\n','\n').replace('\r','\n')


class PruebasFinales(unittest.TestCase):
    def test_integridad_del_manifiesto_y_todos_los_archivos(self):
        p=ROOT/'tools/pruebas-finales.manifest.json'
        m=json.loads(norm(p))
        runner=norm(ROOT/'tools/Verificar-Final-Correccion2.ps1')
        self.assertIn(hashlib.sha256(norm(p).encode()).hexdigest().upper(),runner)
        for name,digest in m['archivos'].items():
            with self.subTest(file=name):
                self.assertEqual(digest,hashlib.sha256(norm(ROOT/name).encode()).hexdigest().upper())
        self.assertEqual(68,m['casos'])
        self.assertEqual(48,len(m['filtro'].split('|')))
        for name in ('ConexionBD_ConectaConHotelTramontoDB','UsuarioNegocios_CambiarContrasena_GuardaHashValido','UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull'):
            self.assertNotIn(name,m['filtro'])

    def test_sql_final_solo_metadatos_y_variable_local_sin_invocar_procedimientos(self):
        sql=norm(ROOT/'base de datos HotelTramonto/pruebas-finales/00_VerificarCorreccion2.sql')
        code=re.sub(r"--[^\n]*|'(?:''|[^'])*'",'',sql)
        self.assertNotRegex(code,r'(?i)\b(?:UPDATE|DELETE|TRUNCATE|ALTER|DROP|GRANT|DENY|REVOKE|EXEC|EXECUTE|USE)\b')
        self.assertTrue(all(x.startswith('@') for x in re.findall(r'(?i)INSERT INTO\s+(\S+)',code)))
        self.assertIn("DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'",sql)
        self.assertIn('sys.dm_exec_describe_first_result_set_for_object',sql)
        self.assertEqual(19,len(json.loads(norm(ROOT/'tools/pruebas-finales.manifest.json'))['definiciones_sql']))

    def test_runner_no_llama_migraciones_permisos_ni_runner_completo(self):
        code=norm(ROOT/'tools/Verificar-Final-Correccion2.ps1')
        self.assertNotIn('EjecutarArchivo',code)
        self.assertNotIn('02_PermisosMinimos',code)
        self.assertNotIn('01_AgregarCategoria',code)
        self.assertNotIn("& (Join-Path $PSScriptRoot 'Probar-HotelTramonto.ps1')",code)
        self.assertIn('configuracion-inexistente.json',code)
        self.assertIn("'HotelTramontoDB_Pruebas'",code)
        self.assertIn("'Canon'",code)


if __name__=='__main__':
    unittest.main(verbosity=2)
