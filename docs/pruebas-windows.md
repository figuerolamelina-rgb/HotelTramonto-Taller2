# Pruebas de corrección 2 y migración de servicios en Windows

Usar la rama `correccion-permisos-habitaciones`. Abrir `HotelTramonto.slnx` en
Visual Studio con .NET 10 y Presentación como proyecto de inicio. No fusionar la
rama durante las pruebas. No ejecutar los scripts SQL originales para instalar
esta corrección: conservan su contexto original y no tienen los nuevos guards.

## Copia de SQL y orden exacto

1. Restaurar una copia como **HotelTramontoDB_Pruebas**. Confirmar servidor y base
   en SSMS. Seleccionar esa base en el desplegable. No usar HotelTramontoDB.
2. Guardar un respaldo de la copia y cerrar la aplicación mientras se migra.
3. Ejecutar `compatibilidad-servicios/00_DiagnosticoYRespaldo.sql`. Guardar
   definiciones y resultados; confirmar que coinciden con lo revisado.
4. Ejecutar `compatibilidad-servicios/01_AgregarCategoria.sql`.
5. Ejecutar `compatibilidad-servicios/02_ClasificarServicios.sql`. Su único mapa
   autorizado es ID 1 -> Otros. Antes de escribir exige Desayuno Buffet, precio
   9500.00 y activo = 0. Solo cambia categoria si era NULL. Si encuentra otra
   categoría o un escenario distinto, se detiene sin sobrescribir.
6. Verificar ID 1: categoria Otros, precio 9500.00, activo 0, nombre/descripción
   sin cambios. No debe quedar ningún servicio sin categoría; si aparecieron
   otros servicios, detenerse y revisar la clasificación por separado.
7. Ejecutar `compatibilidad-servicios/03_ActualizarProcedimientos.sql` y luego
   `compatibilidad-servicios/04_Comprobaciones.sql`.
8. Ejecutar `permisos-habitaciones/00_Diagnostico.sql` y después
   `permisos-habitaciones/01_ProcedimientosFaltantes.sql`. Instala los cinco
   procedimientos faltantes, sin reemplazar los ya existentes.
9. Repetir `compatibilidad-servicios/04_Comprobaciones.sql` para revisar también
   los procedimientos de inactivos y reactivación.

NO ejecutar `permisos-habitaciones/02_PermisosMinimos.sql`: permanece pendiente y
bloqueado por un THROW incondicional. Tampoco corresponde ejecutar su reversión
04 ni esperar que 03_VerificarPermisos demuestre restricciones no instaladas.
La identidad sysadmin/db_owner actual no demuestra separación de permisos SQL.

Todos los lotes nuevos verifican DB_NAME y se detienen si no coincide exactamente
con HotelTramontoDB_Pruebas. Ninguno cambia de base con USE. Ejecutar archivos
completos; una selección manual que omita los guards no tiene esa protección.
El nombre no identifica el servidor: comprobar también la instancia. Estas
comprobaciones no sustituyen permisos SQL ni protegen contra editar el script.

## Conexión privada del cliente

La aplicación también debe apuntar a la copia: los guards SQL no restringen las
conexiones que abra Windows Forms. Desde la raíz del checkout, PowerShell:

```powershell
$carpeta = Join-Path $env:LOCALAPPDATA 'HotelTramonto\Pruebas'
New-Item -ItemType Directory -Force $carpeta | Out-Null
$archivo = Join-Path $carpeta 'conexion.local.json'
if (-not (Test-Path $archivo)) {
    Copy-Item '.\configuracion\conexion.pruebas.ejemplo.json' $archivo
}
notepad $archivo
```

Completar Servidor con la instancia real de pruebas y verificar BaseDatos =
HotelTramontoDB_Pruebas. No incluir credenciales. Mantener la verificación de
certificado; la excepción local de desarrollo está explicada en README.

Para iniciar desde esa misma consola:

```powershell
$env:HOTELTRAMONTO_CONFIG = $archivo
dotnet run --project HotelTramonto.Presentacion
```

Para depurar desde Visual Studio, configurar HOTELTRAMONTO_CONFIG con esa ruta
en el entorno de su proceso. Una opción local es establecerla para el usuario y
cerrar/reabrir Visual Studio para que la herede:

```powershell
[Environment]::SetEnvironmentVariable('HOTELTRAMONTO_CONFIG', $archivo, 'User')
```

Registrar primero el valor anterior si existía y restaurarlo al finalizar las
pruebas. No subir este archivo ni exportar datos de usuarios a Git. La plantilla
versionada es genérica; el archivo privado queda fuera del checkout.

## Recorrido funcional

- ADMINISTRADOR: crear/modificar/desactivar/reactivar habitaciones de prueba.
- RECEPCIONISTA y GERENTE: sin controles de administración; consultas disponibles.
- Servicios: ID 1 sigue inactivo y no aparece para nuevos consumos. Si se prueba
  su reactivación administrativa en la copia, Otros debe ofrecerse con precio
  9500.00. Registrar cantidad/consumo y revisar el saldo de servicios separado del
  alojamiento y el pago correspondiente. No alterar los consumos históricos.
- Huéspedes, reservas, saldos, check-in y check-out: probar recorridos existentes
  con cuentas/datos de prueba privados y conservar las reglas de pagos.
- Incluido sigue siendo una categoría del código heredado y se excluye del selector;
  no elegirla para adicionales bajo la regla confirmada. No se garantiza protección
  contra llamadas SQL directas o clientes alterados con la identidad compartida.

## Reversión

Detener la aplicación y revisar `compatibilidad-servicios/05_RevertirProcedimientos.sql`.
También está restringido a HotelTramontoDB_Pruebas. Restaura los tres cuerpos
anteriores confirmados y mantiene categoria/clasificaciones. No deshace filas de
pruebas funcionales ni elimina los cinco procedimientos faltantes. El contrato
anterior era incompatible con C#; no reabrir el cliente hasta resolver/reaplicar
la migración. Una copia de respaldo permite reiniciar la prueba completa sin tocar
la base original. No ejecutar reversión de permisos porque no se instalaron.

## Verificación automatizada sin SQL

```text
python tools/verificar_scripts_pruebas.py
```

El script comprueba cada lote, ausencia de USE, mapa exclusivo de clasificación y
bloqueo del instalador de permisos; no conecta a SQL Server. Las pruebas .NET sin
SQL usan el filtro de README que excluye tres pruebas existentes con acceso a base,
incluida una que cambia contraseñas. No ejecutar toda la suite indiscriminadamente.
