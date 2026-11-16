"""Auditoría estática de rutas UI; no abre Windows Forms ni SQL."""
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
UI = ROOT / 'HotelTramonto.Presentacion'


def body(text, method):
    # Ignorar comentarios y literales para contar bloques sin depender de su contenido.
    pattern = r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|/\*[\s\S]*?\*/|//[^\n]*'
    clean = re.sub(pattern, lambda m: ' ' * len(m[0]), text)
    start = re.search(r'\b' + re.escape(method) + r'\s*\([^)]*\)\s*\{', clean)
    assert start, method
    pos = start.end(); depth = 1
    for end in range(pos, len(clean)):
        depth += (clean[end] == '{') - (clean[end] == '}')
        if depth == 0:
            return text[pos:end]
    raise AssertionError('Método sin cierre: ' + method)


class FormulariosHabitaciones(unittest.TestCase):
    def test_todos_los_handlers_administrativos_tienen_guard_antes_de_actuar(self):
        cases = {
            'FormHabitaciones': [('btnNuevaHabitacion_Click','TienePermisoDeGestion','new FormNuevaHabitacion'),
                ('btnModificarHabitacion_Click','TienePermisoDeGestion','new FormModificarHabitacion'),
                ('btnDesactivarHabitacion_Click','TienePermisoDeGestion','negocio.Desactivar'),
                ('btnReactivarHabitacion_Click','TienePermisoDeGestion','new FormReactivarHabitacion')],
            'FormNuevaHabitacion': [('btnGuardar_Click','EsAdministrador','negocio.Insertar')],
            'FormModificarHabitacion': [('btnGuardarCambios_Click','EsAdministrador','negocio.Modificar')],
            'FormReactivarHabitacion': [('btnReactivar_Click','EsAdministrador','negocio.Reactivar')],
        }
        for form, handlers in cases.items():
            source = (UI / (form + '.cs')).read_text(encoding='utf-8-sig')
            self.assertIn('PermisosHabitaciones.PuedeAdministrar(rolUsuario)', source)
            for method, guard, action in handlers:
                with self.subTest(form=form, method=method):
                    code = body(source, method)
                    self.assertRegex(code, r'if\s*\(\s*!' + guard)
                    self.assertLess(code.index(guard), code.index(action))
                    self.assertIn('return;', code[:code.index(action)])
                    self.assertIn('rolUsuario', code[code.index(action):])

    def test_ui_no_accede_directamente_a_datos_de_habitaciones(self):
        for file in UI.glob('*.cs'):
            self.assertNotRegex(file.read_text(encoding='utf-8-sig'), r'new\s+(?:HabitacionDatos|TipoHabitacionDatos)\s*\(')

    def test_controles_y_grillas_conservan_restricciones(self):
        source = (UI / 'FormHabitaciones.cs').read_text(encoding='utf-8-sig')
        for button in ('Nueva','Modificar','Desactivar','Reactivar'):
            self.assertIn(f'btn{button}Habitacion.Visible = EsAdministrador;', source)
        self.assertIn('dgvHabitaciones.ReadOnly = true;', source)
        for form, button in [('FormNuevaHabitacion','btnGuardar'),('FormModificarHabitacion','btnGuardarCambios'),('FormReactivarHabitacion','btnReactivar')]:
            code = (UI / (form + '.cs')).read_text(encoding='utf-8-sig')
            self.assertIn(button + '.Enabled = EsAdministrador;', code)

    def test_reactivar_admin_no_depende_de_seleccionar_una_habitacion_activa(self):
        source = (UI / 'FormHabitaciones.cs').read_text(encoding='utf-8-sig')
        click = body(source, 'btnReactivarHabitacion_Click')
        self.assertIn('new FormReactivarHabitacion(rolUsuario)', click)
        self.assertNotIn('CurrentRow', click)
        buttons = body(source, 'ActualizarBotones')
        self.assertLess(buttons.index('btnReactivarHabitacion.Enabled = true;'), buttons.index('dgvHabitaciones.CurrentRow'))
        self.assertIn('btnReactivarHabitacion.Enabled = false;', buttons)


if __name__ == '__main__':
    unittest.main(verbosity=2)
