"""Comprueba contratos de la integración publicada. No conecta a SQL ni abre ventanas."""
import hashlib
import json
import re
import sqlite3
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
        guards = re.findall(r"^IF OBJECT_ID\(N'(dbo\.\w+)'\) IS NOT NULL AND (.*(?:\n AND @esHistoricaConCRLF = 0)?)\n THROW 51904, '([^']+)'", script, re.M)
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

    def test_reporte_51904_respaldo_real_y_mutaciones(self):
        # Evaluar las expresiones escalares de 09 en memoria con las cadenas del
        # procedimiento. La integración SQL Server completa queda a cargo del runner.
        script = (SQL / '09_ConsultasIntegracion.sql').read_text()
        diag = (ROOT / 'tests/HotelTramonto.VerificacionSql/DiagnosticoIntegracion.cs').read_text()
        refs = {k: re.search(rf"@{k} nvarchar\(max\)=N'((?:''|[^'])*)'", script)[1].replace("''", "'")
                for k in ('anterior', 'iara', 'nuevo2')}
        crlf = '\r\n'
        observada = crlf + refs['anterior'].replace('\n', crlf) + crlf
        self.assertEqual('776dfed78e6c0985455395d194ea3005e5bbb917a0d04bdee6370a4b139c5ef3',
                         hashlib.sha256(observada.encode('utf-8')).hexdigest())
        plantilla = re.search(r'DECLARE @historicaConCRLF nvarchar\(max\) = (.*);', script)[1]
        self.assertIn(plantilla, diag)
        bandera = re.search(r'DECLARE @esHistoricaConCRLF bit = (CASE WHEN.*?END);', script, re.S)[1]
        predicado = re.search(r"^IF OBJECT_ID\(N'dbo.sp_Reporte_ServiciosMasUtilizados'\) IS NOT NULL AND (.*?)\n THROW 51904", script, re.M | re.S)[1]
        self.assertIn('AND @esHistoricaConCRLF = 0', predicado)
        self.assertIn("WHEN r.variante='historica' AND DATALENGTH(o.texto) = DATALENGTH(@historicaConCRLF)", diag)
        self.assertIn('AND o.texto COLLATE Latin1_General_100_BIN2 = @historicaConCRLF', diag)
        db = sqlite3.connect(':memory:')
        self.addCleanup(db.close)
        db.create_function('NCHAR', 1, chr)
        db.create_function('DATALENGTH', 1, lambda s: None if s is None else len(s.encode('utf-16le')))

        def evaluar(expr, params):
            expr = expr.replace(' + ', ' || ').replace('Latin1_General_100_BIN2', 'BINARY')
            return db.execute('SELECT ' + expr.replace("N'", "'"), params).fetchone()[0]

        for eol in ('\n', crlf):
            params = {k: v.replace('\n', eol) for k, v in refs.items()}
            params['crlf'] = crlf
            params['historicaConCRLF'] = evaluar(plantilla, params)
            self.assertEqual(observada, params['historicaConCRLF'])
            for texto, rechazo in [
                (observada, False), (refs['anterior'], False), (refs['iara'], False), (refs['nuevo2'], False),
                (None, True), (observada.replace('AVG(', 'SUM('), True),
                (observada.replace("'CANCELADA'", "'CANCELADA '"), True),
                (observada.replace("'CANCELADA'", "'CAN\rCELADA'"), True),
                (observada + 'SELECT 1;', True)
            ]:
                with self.subTest(eol=eol, texto=texto):
                    params['actual2'] = texto
                    params['esHistoricaConCRLF'] = evaluar(bandera, params)
                    self.assertEqual(rechazo, bool(evaluar(predicado, params)))

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

    def test_51905_conserva_contratos_y_comparacion_de_las_tres_consultas(self):
        install = (SQL / '09_ConsultasIntegracion.sql').read_text()
        verify = (SQL / '10_VerificarConsultasIntegracion.sql').read_text()
        installed = re.findall(r"EXEC sys\.sp_executesql N'(CREATE OR ALTER PROCEDURE (?:''|[^'])*)';", install)
        self.assertEqual(3, len(installed))
        for i, literal in enumerate(installed):
            name = re.match(r'CREATE OR ALTER PROCEDURE (dbo\.\w+)', literal)[1]
            expected = re.search(rf"@nuevo{i} nvarchar\(max\)=N'((?:''|[^'])*)'", verify)[1]
            self.assertEqual(literal, expected)
            self.assertIn(f"SELECT N'{name}', @nuevo{i}", verify)
        self.assertIn("SELECT TOP (1) @error = nombre + N':", verify)
        self.assertIn('FROM comparaciones WHERE coincide = 0 ORDER BY nombre;', verify)
        self.assertIn('IF @error IS NOT NULL THROW 51905, @error, 1;', verify)

    def test_51905_definiciones_del_informe_y_mutaciones(self):
        verify = (SQL / '10_VerificarConsultasIntegracion.sql').read_text()
        query = verify.split('-- INICIO comparación del paso 10\n')[1].split('-- FIN comparación del paso 10')[0]
        self.assertNotRegex(query, r'(?im)^\s*(?:EXEC|USE|INSERT|UPDATE|DELETE|MERGE|ALTER|DROP|CREATE)\b')
        # Ejecutar la misma consulta de solo lectura del verificador y del diagnóstico.
        # SQLite aporta la CTE. Las funciones de catálogo leen este diccionario en memoria.
        query = query.replace("N'", "'").replace('RIGHT(', 'sql_right(') + 'SELECT nombre, coincide FROM comparaciones'
        db = sqlite3.connect(':memory:')
        self.addCleanup(db.close)
        catalog = {}
        db.create_function('OBJECT_ID', 2, lambda name, kind: name if kind == 'P' and name in catalog else None)
        db.create_function('OBJECT_DEFINITION', 1, catalog.get)
        db.create_function('CHARINDEX', 2, lambda sub, s: None if s is None else s.find(sub) + 1)
        db.create_function('LEN', 1, len)
        db.create_function('NCHAR', 1, chr)
        db.create_function('sql_right', 2, lambda s, n: None if s is None else s[-n:])
        db.create_function('DATALENGTH', 1, lambda s: None if s is None else len(s.encode('utf-16le')))

        def bin2(a, b):
            # La igualdad SQL rellena con espacios incluso con BIN2.
            a, b = a.rstrip(' '), b.rstrip(' ')
            return (a > b) - (a < b)

        db.create_collation('Latin1_General_100_BIN2', bin2)
        params = {}
        # SHA-256 de las tres observadas del diagnostico-paso10.json de la copia Pre56.
        digests = ['88b5771b62c9facd29b84cc3c36cc8977ca08a7e06b3c4d962b40d8c03993074',
                   'ea7f28bca350c1d3a5229869b3eb4403eab0e4c7a676874b0ffec06269f3bf2d',
                   '5d4945066fb34ff6fa6354b79cbaa9cccb4b542a3262d6b5978421e61f23c144']
        for i, digest in enumerate(digests):
            literal = re.search(rf"@nuevo{i} nvarchar\(max\)=N'((?:''|[^'])*)'", verify)[1]
            definition = literal.replace("''", "'").replace('\n', '\r\n')
            name = re.match(r'CREATE OR ALTER PROCEDURE (dbo\.\w+)', definition)[1]
            params[f'nuevo{i}'] = definition
            catalog[name] = definition.replace('CREATE OR ALTER', 'CREATE  ', 1)
            self.assertEqual(digest, hashlib.sha256(catalog[name].encode('utf-8')).hexdigest())
        approved = {name: 1 for name in catalog}
        self.assertEqual(approved, dict(db.execute(query, params)))

        for name, observed in catalog.copy().items():
            rest = observed.removeprefix('CREATE   PROCEDURE')
            for header in ('CREATE OR ALTER PROCEDURE', 'CREATE PROCEDURE', 'ALTER PROCEDURE', 'CREATE\t\r\n PROCEDURE'):
                with self.subTest(name=name, header=header):
                    catalog[name] = header + rest
                    self.assertEqual(approved, dict(db.execute(query, params)))
            literal = re.search(r"'[^']+'", rest)[0]
            for changed in (None, '', 'CREATEPROCEDURE' + rest, '-- CREATE PROCEDURE' + rest,
                            'SELECT 1;\r\n' + observed, observed.replace('dbo.', '', 1),
                            observed.replace('WHERE', 'WHERE 1=0 AND', 1), observed.replace('SELECT', 'SELECT 0,', 1),
                            observed.replace(literal, literal[:-1] + " '", 1),
                            observed.replace(literal, literal.swapcase(), 1),
                            observed.replace('\r\n', '\n', 1), observed + ' ', observed + '\r\nSELECT 1;'):
                with self.subTest(name=name, observed=changed):
                    catalog[name] = changed
                    self.assertEqual({**approved, name: 0}, dict(db.execute(query, params)))
            del catalog[name]
            self.assertEqual({**approved, name: 0}, dict(db.execute(query, params)))
            catalog[name] = observed

        # El antiguo REPLACE global también alcanzaba palabras DDL y CR dentro de literales.
        name = 'dbo.sp_Reporte_ServiciosMasUtilizados'
        frase = "N'CREATE PROCEDURE dbo.x\r\ndos  espacios'"
        params['nuevo2'] = params['nuevo2'].replace("N'Fecha de consumo'", frase)
        catalog[name] = catalog[name].replace("N'Fecha de consumo'", frase)
        self.assertEqual(approved, dict(db.execute(query, params)))
        revised = catalog[name]
        for mutation in (frase.replace('CREATE PROCEDURE dbo.', 'ALTER PROCEDURE '),
                         frase.replace('\r\n', '\n'), frase.replace('  ', ' ')):
            catalog[name] = revised.replace(frase, mutation, 1)
            self.assertEqual({**approved, name: 0}, dict(db.execute(query, params)))

    def test_51905_informe_completo_antes_de_verificar_en_la_copia(self):
        source = (ROOT / 'tests/HotelTramonto.VerificacionSql/Ejecutor.cs').read_text()
        migrar = source.split('private void Migrar(string nombre)')[1].split('private void EliminarCopia')[0]
        self.assertIn('if (nombre == "10_VerificarConsultasIntegracion.sql")', migrar)
        call = migrar.index('DiagnosticoIntegracion.GuardarPaso10(db, original, DirectorioInformes)')
        self.assertLess(migrar.index('Copia.Abrir()'), call)
        self.assertLess(call, migrar.index('adapter.Fill'))
        diag = (ROOT / 'tests/HotelTramonto.VerificacionSql/DiagnosticoIntegracion.cs').read_text()
        report = diag.split('internal static void GuardarPaso10')[1]
        self.assertIn('-- INICIO comparación del paso 10', report)
        self.assertIn('-- FIN comparación del paso 10', report)
        self.assertIn('bloque.Groups[1].Value', report)
        self.assertIn('SELECT * FROM comparaciones ORDER BY nombre;', report)
        self.assertIn('Sql.Tabla(copia, consulta, contratos)', report)
        for field in ('procedimiento = nombre', 'estado', 'coincideSegun10', 'diferenciaLiteral',
                      'diferenciaSegun10 = diferencia, esperada, observada', 'diagnostico-paso10.json'):
            self.assertIn(field, report)

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
