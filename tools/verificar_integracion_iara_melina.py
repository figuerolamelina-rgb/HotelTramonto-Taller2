"""Comprueba contratos de la integración publicada. No conecta a SQL ni abre ventanas."""
import hashlib
import json
import re
import unittest
from pathlib import Path
from verificar_permisos_formularios import body

ROOT = Path(__file__).resolve().parents[1]
SQL = ROOT / 'base de datos HotelTramonto/servicios-habitaciones'


class IntegracionIaraMelina(unittest.TestCase):
    def test_preservacion_de_sql_concurrencia_historial_conexion_y_referencias(self):
        inventory = json.loads((ROOT / 'docs/inventario-integracion-iara-melina.json').read_text())
        self.assertEqual(9, len(inventory['commits_iara_incorporados']))
        self.assertEqual(10, len(inventory['referencias_sql_sha256']))
        for section in ('referencias_sql_sha256', 'contratos_conservados_sha256'):
            for name, digest in inventory[section].items():
                with self.subTest(file=name):
                    self.assertEqual(digest, hashlib.sha256((ROOT / name).read_bytes()).hexdigest())

    def test_pago_estado_no_pierde_el_contrato_coordinado(self):
        data = (ROOT / 'HotelTramonto.Datos/PagoDatos.cs').read_text()
        transition = body(data, 'ActualizarEstado')
        self.assertEqual({'@id_pago', '@estado'}, set(re.findall(r'"(@\w+)"', transition)))
        all_data = '\n'.join(p.read_text(encoding='utf-8-sig') for p in (ROOT / 'HotelTramonto.Datos').glob('*.cs'))
        self.assertNotIn('"sp_Reserva_CrearYConfirmar"', all_data)
        business = (ROOT / 'HotelTramonto.Negocios/ReservaNegocios.cs').read_text()
        create = body(business, 'CrearYConfirmar')
        self.assertLess(create.index('PermisosOperacion.Exigir'), create.index('FlujoReservaNegocios'))
        self.assertIn('Ejecutar(solicitud, contexto)', create)

    def test_seleccion_con_teclado_y_busqueda_fallida_no_cobran_reserva_anterior(self):
        source = (ROOT / 'HotelTramonto.Presentacion/FormPagos.cs').read_text()
        search = body(source, 'btnBuscarHuesped_Click')
        self.assertLess(search.index('LimpiarBusqueda()'), search.index('BuscarPorDNI'))
        self.assertIn('dgvReservas.SelectionChanged +=', source)
        self.assertIn('txtDNI.TextChanged +=', source)
        self.assertIn('seleccion.Seleccionar(id)', body(source, 'CargarSeleccionReserva'))
        payment = body(source, 'EjecutarCobro')
        self.assertIn('contexto.Id', payment)
        self.assertIn('Convert.ToInt32(pago["id_pago"])', payment)
        self.assertNotRegex(source, r'Sort\s*=\s*"fecha_pago DESC')

    def test_historial_y_selector_respetan_roles_y_habitaciones(self):
        source = (ROOT / 'HotelTramonto.Presentacion/FormHistorialHuespedes.cs').read_text()
        self.assertIn('PermisosOperacion.Exigir(rol)', source)
        self.assertIn('new HuespedNegocios(rol)', source)
        self.assertIn('Distinct().Count()', source)
        self.assertNotIn('ex.Message', source)
        source = (ROOT / 'HotelTramonto.Presentacion/FormRegistrarConsumos.cs').read_text()
        self.assertIn('ListarConReservaCheckIn(rolOperacion)', source)
        self.assertIn('Convert.ToInt32(fila["id_reserva"])', source)
        self.assertIn('EjecutarConsumo(consumoPendiente)', source)
        self.assertIn('contexto.Id', source)

    def test_incremento_sql_no_inventa_fechas_ni_reemplaza_escritores(self):
        source = (SQL / '09_ConsultasIntegracion.sql').read_text()
        definitions = [s.replace("''", "'") for s in re.findall(r"EXEC sys\.sp_executesql N'((?:''|[^'])*)';", source, re.S)]
        procedures = {re.search(r'PROCEDURE dbo\.(\w+)', s)[1]: s for s in definitions if 'PROCEDURE dbo.' in s}
        self.assertEqual({'sp_Pago_ObtenerComprobante', 'sp_Habitacion_ListarConReservaCheckIn', 'sp_Reporte_ServiciosMasUtilizados'}, set(procedures))
        self.assertIn("p.estado='APROBADO'", procedures['sp_Pago_ObtenerComprobante'])
        self.assertIn('r.fecha_checkout IS NULL', procedures['sp_Habitacion_ListarConReservaCheckIn'])
        self.assertIn('ds.fecha_consumo IS NULL', procedures['sp_Reporte_ServiciosMasUtilizados'])
        ddl = next(d for d in definitions if d.startswith('ALTER TABLE'))
        self.assertIn('DATETIME2(0) NULL', ddl)
        self.assertNotIn('WITH VALUES', ddl)
        for code in procedures.values():
            self.assertNotRegex(code, r'(?im)^\s*(UPDATE|DELETE|INSERT|MERGE)\b')
        self.assertIn('BEGIN TRANSACTION', source)
        self.assertIn('Definicion previa desconocida', source)

    def test_diagnostico_51904_compara_las_mismas_variantes_que_09(self):
        script = (SQL / '09_ConsultasIntegracion.sql').read_text()
        diag = (ROOT / 'tests/HotelTramonto.VerificacionSql/DiagnosticoIntegracion.cs').read_text()
        canon = re.search(r'(LTRIM\(RTRIM\(REPLACE\(.*? COLLATE Latin1_General_100_BIN2) AS canon', diag)[1]
        guards = re.findall(r"^IF OBJECT_ID\(N'(dbo\.\w+)'\) IS NOT NULL AND (.*)\n THROW 51904, '([^']+)'", script, re.M)
        self.assertEqual(3, len(guards))
        for i, (name, predicate, message) in enumerate(guards):
            with self.subTest(name=name):
                self.assertIn(f'("{name}", "nuevo{i}")', diag)
                self.assertIn(name, message)
                self.assertIn(f'@actual{i} IS NULL', predicate)
                versions = [f'nuevo{i}'] if i < 2 else ['nuevo2', 'anterior', 'iara']
                for var in versions:
                    self.assertIn(canon.replace('texto', f'@actual{i}') + ' <> ' + canon.replace('texto', '@' + var), predicate)
                    literal = re.search(rf"@{var} nvarchar\(max\)=N'((?:''|[^'])*)'", script)[1]
                    self.assertIn('PROCEDURE ' + name, literal.splitlines()[0])
                self.assertEqual(len(versions), predicate.count(' <> '))

    def test_diagnostico_51904_solo_lee_antes_de_migrar_en_la_copia_validada(self):
        runner = (ROOT / 'tests/HotelTramonto.VerificacionSql/Ejecutor.cs').read_text()
        migrar = runner.split('private void Migrar(string nombre)')[1].split('private void EliminarCopia')[0]
        self.assertLess(migrar.index('using var db = Copia.Abrir()'), migrar.index('DiagnosticoIntegracion.Guardar(db'))
        self.assertLess(migrar.index('DiagnosticoIntegracion.Guardar(db'), migrar.index('adapter.Fill'))
        diag = (ROOT / 'tests/HotelTramonto.VerificacionSql/DiagnosticoIntegracion.cs').read_text()
        query = diag.split('Consulta = """')[1].split('"""')[0]
        code = re.sub(r"'(?:''|[^'])*'", '', query)
        self.assertNotRegex(code, r'(?i)\b(EXEC|USE|INSERT|INTO|UPDATE|DELETE|MERGE|ALTER|DROP|CREATE)\b')
        self.assertIn('OBJECT_DEFINITION(o.id)', code)
        self.assertIn('CASE WHEN o.canon=r.canon', code)
        self.assertIn('diagnostico-integracion.json', diag)

    def test_ui_opcional_solo_despues_de_pruebas_y_antes_de_limpieza(self):
        source = (ROOT / 'tests/HotelTramonto.VerificacionSql/Ejecutor.cs').read_text()
        guard = source.split('if (pausaUi)', 1)[1].split('informe.Etapa = "Verificación final"', 1)[0]
        self.assertLess(guard.index('informe.Fallidas != 0'), guard.index('PrepararAceptacionUi()'))
        self.assertLess(guard.index('historicos.Verificar(Copia)'), guard.index('PrepararAceptacionUi()'))
        ui = body(source, 'PrepararAceptacionUi')
        self.assertLess(ui.index('Copia.Abrir()'), ui.index('File.WriteAllText'))
        self.assertIn('ConfiguracionUiTemporal.Crear(identidad, actual)', ui)
        self.assertIn('Console.ReadLine() != "FINALIZAR"', ui)
        self.assertIn('EliminarCopia()', source)
        self.assertIn('NO_WAIT', source)

    def test_runner_integrado_conserva_36_y_guarda_verificable(self):
        source = (ROOT / 'tests/HotelTramonto.VerificacionSql/Ejecutor.cs').read_text()
        self.assertIn('CasosSql.Todos.Concat(CasosSql.Ciclo)', source)
        self.assertIn('informe.Aprobadas == casos.Length', source)
        self.assertIn('if (cicloReserva)', source)
        self.assertIn('Migrar("09_ConsultasIntegracion.sql")', source)
        self.assertIn('Migrar("10_VerificarConsultasIntegracion.sql")', source)
        for name in ('09_ConsultasIntegracion.sql', '10_VerificarConsultasIntegracion.sql'):
            sql = (SQL / name).read_text()
            self.assertIn("DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'", sql)
            self.assertIn("SERVERPROPERTY('MachineName')", sql)


if __name__ == '__main__':
    unittest.main()
