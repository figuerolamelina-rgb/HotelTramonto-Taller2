"""Contratos del incremento final; no ejecuta SQL ni Windows Forms."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SQL = ROOT / 'base de datos HotelTramonto/servicios-habitaciones'


class Aceptacion56(unittest.TestCase):
    def test_transiciones_comparten_protocolo_y_rechazan_cierre(self):
        script = (SQL / '07_CicloReserva.sql').read_text()
        definitions = re.findall(r"EXEC sys\.sp_executesql N'((?:''|[^'])*)';", script, re.S)
        self.assertEqual(2, len(definitions))
        for body in (s.replace("''", "'") for s in definitions):
            self.assertLess(body.index('BEGIN TRANSACTION'), body.index('sys.sp_getapplock'))
            self.assertLess(body.index('sys.sp_getapplock'), body.index('WITH (UPDLOCK,HOLDLOCK)'))
            self.assertIn("HotelTramonto:reserva:", body)
            self.assertIn("@LockOwner='Transaction'", body)
            self.assertIn("AND estado='CONFIRMADA' AND fecha_checkout IS NULL", body)
            self.assertIn('IF @@ROWCOUNT<>1', body)
            self.assertIn('IF XACT_STATE()<>0 ROLLBACK', body)

    def test_instalacion_definiciones_conocidas_y_verificacion_exacta(self):
        install = (SQL / '07_CicloReserva.sql').read_text()
        verify = (SQL / '08_VerificarCicloReserva.sql').read_text()
        for source in (install, verify):
            self.assertIn("DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'", source)
            self.assertIn("SERVERPROPERTY('MachineName')", source)
            self.assertIn('Latin1_General_100_BIN2', source)
        self.assertIn('sys.crypt_properties', install)
        self.assertIn('Definicion previa desconocida', install)
        self.assertIn('BEGIN TRANSACTION', install)
        self.assertNotRegex(re.sub(r"'(?:''|[^'])*'", '', verify), r'(?im)^\s*(EXEC|UPDATE|DELETE|INSERT|ALTER|CREATE)\s')

    def test_contratos_del_ciclo_coinciden_con_los_lotes_originales_y_ddl_instalado(self):
        original = (SQL.parent / 'Procedimientos_Almacenados.sql').read_text()
        install = (SQL / '07_CicloReserva.sql').read_text()
        verify = (SQL / '08_VerificarCicloReserva.sql').read_text()
        batches = re.split(r'(?im)^GO[ \t]*$', original)
        ddl = re.findall(r"EXEC sys\.sp_executesql N'((?:''|[^'])*)';", install)
        for index, name in enumerate(('sp_Reserva_CheckIn', 'sp_Reserva_Cancelar')):
            with self.subTest(name=name):
                header = rf'(?m)^CREATE OR ALTER PROCEDURE (?:dbo\.)?{name}\b'
                batch, = [b for b in batches if re.search(header, b)]
                body = batch[re.search(header, batch).start():].rstrip()
                previous = re.search(rf"@anterior{index} nvarchar\(max\)=N'((?:''|[^'])*)'", install)[1]
                self.assertEqual(body, previous.replace("''", "'"))
                for script in (install, verify):
                    reviewed = re.search(rf"@nuevo{index} nvarchar\(max\)=N'((?:''|[^'])*)'", script)[1]
                    self.assertEqual(ddl[index], reviewed)
                    self.assertIn(f"(N'dbo.{name}', 3, @actual{index})", script)
                if index == 0:
                    self.assertIn('---check-in', batch[:batch.index(body)])

    def test_diagnostico_y_guardas_comparten_el_comparador_escalar(self):
        marker = r'(?ms)^    -- INICIO canon de ciclo[^\n]*\n(.*?)^    -- FIN canon de ciclo'
        blocks = [re.findall(marker, (SQL / f).read_text()) for f in
                  ('07_CicloReserva.sql', '08_VerificarCicloReserva.sql')]
        self.assertEqual(1, len(blocks[0]))
        self.assertEqual(*blocks)
        code = re.sub(r"--[^\n]*|'(?:''|[^'])*'", '', blocks[0][0])
        self.assertNotRegex(code, r'(?i)\b(EXEC|CREATE|ALTER|DROP|INSERT|UPDATE|DELETE|MERGE|FROM|INTO)\b')
        self.assertIn('IF @enLiteral=0 AND UNICODE(@car)=13', code)
        runner = (ROOT / 'tests/HotelTramonto.VerificacionSql/Ejecutor.cs').read_text()
        self.assertLess(runner.index('DiagnosticoCiclo.Validar'), runner.index('foreach (string nombre in ContratosBase.Procedimientos)'))

    def test_recuperacion_no_reconstruye_consumo_desde_controles(self):
        source = (ROOT / 'HotelTramonto.Presentacion/FormRegistrarConsumos.cs').read_text()
        click = source.split('private void btnAgregar_Click')[1].split('private string ClaveConsumo')[0]
        self.assertLess(click.index('EjecutarConsumo(consumoPendiente)'), click.index('cmbProducto.SelectedItem'))
        self.assertIn('consumoPendiente = solicitud;', source.split('private void ReanudarConsumo')[1])
        self.assertIn('diario.Confirmar(ClaveConsumo, resultado.Id)', source)

    def test_minimos_son_objetos_explicitos_y_verificacion_real(self):
        path = ROOT / 'base de datos HotelTramonto/acceso-operativo'
        install = (path / '01_Instalar.sql').read_text()
        verify = (path / '02_Verificar.sql').read_text()
        self.assertNotIn('GRANT EXECUTE ON SCHEMA', install)
        self.assertNotIn('GRANT CONTROL', install)
        self.assertIn('DENY SELECT, INSERT, UPDATE, DELETE ON OBJECT', install)
        self.assertIn('sp_Huesped_HistorialReservas', install)
        self.assertIn('sp_ReservaTemporal_ConsultarConfirmacion', install)
        self.assertIn('COLUMN', verify)
        self.assertNotRegex(verify, r'(?im)^\s*EXECUTE AS')
        self.assertIn("COALESCE(IS_SRVROLEMEMBER('sysadmin'),1)<>0", verify)

    def test_nuevos_casos_conservan_barreras_de_copia(self):
        source = (ROOT / 'tests/HotelTramonto.VerificacionSql/CasosCiclo.cs').read_text()
        self.assertEqual(8, len(re.findall(r'\("\d\d_', source)))
        self.assertIn('using var espera = copia.Abrir()', source)
        self.assertIn('sys.dm_exec_requests', source)
        self.assertIn('Exigir(bloqueada', source)

    def test_consulta_aceptacion_sin_escrituras(self):
        source = (ROOT / 'base de datos HotelTramonto/pruebas/ConsultarReservas_Pruebas.sql').read_text()
        code = re.sub(r"/\*.*?\*/|--[^\n]*|'(?:''|[^'])*'", '', source, flags=re.S)
        self.assertNotRegex(code, r'(?i)\b(UPDATE|DELETE|INSERT|MERGE|ALTER|DROP|CREATE|EXEC|USE)\b')
        self.assertIn("DB_NAME() <> N'HotelTramontoDB_Pruebas'", source)
        self.assertIn('Histórico inconsistente documentado', source)


if __name__ == '__main__':
    unittest.main()
