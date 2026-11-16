# Automatización local de pruebas Windows

Archivo: `tools/Probar-HotelTramonto.ps1`, Windows PowerShell 5.1 y SDK .NET 10.
No requiere sqlcmd, el módulo SqlServer ni credenciales: utiliza autenticación
integrada de Windows mediante System.Data.SqlClient de .NET Framework.

## Único comando, desde la raíz del checkout actualizado de la rama

```powershell
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File .\tools\Probar-HotelTramonto.ps1 -ConfiarCertificadoLocal
```

RemoteSigned afecta únicamente este proceso, no la política permanente. Una política
corporativa puede impedir ejecutar scripts no firmados; no eludirla. El interruptor
ConfiarCertificadoLocal mantiene el cifrado SQL pero acepta el certificado local
sin validar su cadena/identidad criptográfica; está previsto exclusivamente para
la instancia local confirmada. Si el certificado es confiable, omitir ese
interruptor conserva la validación completa. No se guardan contraseñas.

Antes de empezar, tener una copia restaurada y respaldada como HotelTramontoDB_Pruebas,
cerrar las conexiones de SSMS/Visual Studio/aplicación que utilicen esa copia y
mantenerla sin otros escritores durante la prueba. El runner se detiene si detecta
otras sesiones; esa observación no bloquea que otro cliente se conecte después.
No modificar los fuentes o SQL de la rama antes de ejecutarlo: sus huellas están
fijadas deliberadamente. Se toleran los cambios de finales de línea LF/CRLF de Git.

## Secuencia y controles

1. Comprueba integridad de 60 archivos revisados: SQL autorizados, fuentes y contratos
   de las dependencias de pruebas. La lista de SQL ejecutables contiene exactamente
   siete archivos; no incluye instaladores de permisos, reversiones ni los SQL originales.
2. Abre master SOLO para SELECT de metadatos. Verifica ServerName exactamente contra
   LAPTOP-5TSJJUP8\SQLEXPRESS, existencia y IDs distintos de ambas bases, copia ONLINE,
   escribible y no snapshot, rutas físicas distintas de datos y log según sys.master_files.
   No abre una conexión a HotelTramontoDB ni ejecuta escrituras en ella. Las rutas
   distintas son evidencia de metadatos SQL, no una inspección física de enlaces NTFS.
3. Abre únicamente HotelTramontoDB_Pruebas para las siguientes operaciones y revalida
   instancia/base antes de cada lote. Los scripts también tienen sus propios guards.
4. Rechaza triggers habilitados de servidor o de la copia, metadatos sin visibilidad,
   esquemas incompatibles, procedimientos distintos de los revisados, firmas,
   EXECUTE AS inesperado y un catálogo diferente del único servicio confirmado.
   Guarda los resultados de ambos diagnósticos 00 en XML local con esquema.
5. Calcula una huella SHA256 y cantidad de filas de cada tabla dbo antes de migrar,
   ordenando por PK y excluyendo únicamente Servicios.categoria. Una tabla sin PK o
   no serializable por FOR JSON provoca detención antes de modificar. No guarda filas
   ni hashes de contraseñas/usuarios en el informe; las huellas son comparaciones en memoria.
6. Restaura paquetes, compila y ejecuta una lista positiva de 45 métodos revisados,
   que expande a 56 casos. No ejecuta las tres pruebas SQL existentes. Exige un TRX
   con 56 aprobados. Durante las pruebas, HOTELTRAMONTO_CONFIG apunta a un archivo
   inexistente y se restaura después; una conexión por defecto debe fallar antes
   de llegar a SQL. Las pruebas propias de configuración usan sus temporales revisados.
7. Pide escribir exactamente `MIGRAR HotelTramontoDB_Pruebas` y confirmar con ello que
   se dispone de respaldo. Cualquier otra respuesta cancela sin modificaciones SQL.
8. Ejecuta compatibilidad-servicios 01, 02, 03, 04 (su 00 ya se ejecutó), luego
   permisos-habitaciones/01_ProcedimientosFaltantes.sql y vuelve a comprobar con 04.
9. Compara huellas de todas las tablas, verifica categoría Otros, parámetros y cuerpos
   instalados. Ejecuta SOLO lecturas: servicios activos/inactivos, habitaciones
   activas/inactivas y disponibilidad para 2099-01-01/02. Vuelve a comprobar huellas.
   No invoca Reactivar, alta de usuarios, reservas, consumos, pagos, check-in o check-out.

La operación autorizada sobre filas es únicamente ID 1 -> categoria Otros, manteniendo
Desayuno Buffet, descripción, precio 9500.00 y activo = 0. Se agrega la columna y se
actualizan las tres definiciones de servicios; se crean los cinco procedimientos
faltantes si no existen. No hay cambios de cuentas, contraseñas, permisos, bases o
archivos de datos. Las comprobaciones de archivos no ejecutan restauraciones ni backups.

El instalador 02_PermisosMinimos.sql NO está en la lista de ejecución. El usuario
actual sysadmin/db_owner permite estas pruebas de compatibilidad, pero no demuestra
restricciones SQL por empleado. El rol compartido y sus limitaciones permanecen pendientes.

## Informe y errores

Se crea una carpeta única bajo `%LOCALAPPDATA%\HotelTramonto\Informes` con informe.json,
informe.txt, logs de restore/build/tests, TRX y resultados XML de diagnóstico/comprobación.
Queda fuera del repositorio. Puede contener metadatos privados de instancia y resultados
de revisión histórica: no subir la carpeta a Git. No contiene credenciales de conexión.
El comando termina con error ante cualquier fallo y deja los pasos aprobados y el error.

No se aplica una transacción global: los scripts rechazan transacciones externas.
Si falla después de una etapa confirmada, puede quedar migración parcial en LA COPIA.
No se ejecuta reversión automática ni se deshacen datos históricos. Revisar el informe
antes de reintentar; las etapas son idempotentes según sus contratos, pero un estado
inesperado se rechaza. Para reiniciar una prueba completa, restaurar otra vez la copia
manualmente; el runner nunca borra, recrea ni restaura bases. La reversión de definiciones
05 está disponible para revisión manual y sigue limitada a la base de pruebas.

## Pruebas que siguen siendo manuales

- Abrir Windows Forms con configuración privada apuntando a HotelTramontoDB_Pruebas.
- Verificar aspecto y controles por ADMINISTRADOR, RECEPCIONISTA y GERENTE.
- Probar las cuatro operaciones de habitaciones y las reactivaciones con datos de
  prueba apropiados, sin reutilizar cuentas o datos reales para pruebas destructivas.
- El servicio 1 seguirá inactivo: no debe ofrecerse como consumo adicional. Si se
  decide reactivarlo EN LA COPIA, Otros debe ofrecerse a 9500 y cobrarse por separado.
- Probar consumo, saldo, pago, reservas, check-in/check-out y mensajes visuales con
  datos aislados. Incluido continúa excluido del selector; la migración no convierte
  servicios en gratuitos ni modifica cargos históricos. Las llamadas SQL externas
  no están autorizadas por usuario de aplicación bajo la identidad compartida.

## Validación realizada por el agente

Parser oficial PowerShell: cero errores de sintaxis. Cuatro funciones puras probadas
con un motor PowerShell hospedado (sin cargar el cuerpo ni funciones SQL), cinco
comprobaciones estáticas Python y lista positiva de pruebas C# ejecutada en Linux:
56 casos aprobados. El runner completo NO se ejecutó en Windows ni en tu SQL Server.
La sintaxis y las comprobaciones estáticas no demuestran el resultado de las operaciones
SQL, la conectividad, el certificado o las funciones de Windows PowerShell en tu equipo.

## Corrección del constructor en Windows PowerShell 5.1

SqlConnectionStringBuilder implementa IDictionary y PowerShell puede interpretar
una asignación como `$b.DataSource = ...` como una clave SQL literal no admitida.
Crear-CadenaConexion usa ahora exclusivamente indexadores con Data Source,
Initial Catalog, Integrated Security, Encrypt, TrustServerCertificate,
Persist Security Info, Connect Timeout y Application Name. Abrir recibe la cadena
ya validada y sigue siendo el único punto que llama Open para iniciar conexión.

El runner ejecuta primero tools/Probar-CadenaConexion.ps1 y registra su resultado
en cadena-conexion.log. La prueba extrae SOLO Crear-CadenaConexion mediante AST,
sin cargar el cuerpo del runner. Comprueba cuatro combinaciones (master/pruebas y
certificado confiable/no confiable), las ocho opciones, ausencia de credenciales
y construcción de un objeto SqlConnection que permanece cerrado. No abre ninguna
conexión ni ejecuta migraciones o permisos. Es compatible con sintaxis 5.1; la
comprobación completa sobre Windows PowerShell 5.1 se realizará en tu computadora.
No se cambiaron controles de base original, confirmación, scripts SQL o informes.

La regresión de construcción también pasó las cuatro combinaciones en un motor
PowerShell hospedado con System.Data.SqlClient real en Linux, sin Open. En ese
host se sustituyeron únicamente la carga de ensamblado/activación de objetos y
la ruta del archivo, porque no es Windows PowerShell instalado. El archivo
Probar-CadenaConexion.ps1 está preparado para ejecutarse nativamente en 5.1.

## Referencia real de habitaciones activas

Se corrigió únicamente sp_Habitacion_Listar en Procedimientos_Almacenados.sql para
incluir WHERE H.activo = 1, según el cuerpo real confirmado por las integrantes.
No se aplica este archivo a la instancia: el runner lo usa solo como referencia.
ConsultarDisponibilidad se conserva sin cambios. El manifiesto incluye la nueva
huella y la prueba Probar-LecturasSql.ps1. Antes de conectarse, esa prueba extrae
solo Canon del runner, compara el cuerpo real proporcionado, exige rechazo del
filtro ausente/invertido y verifica la huella de la consulta de disponibilidad.
La comparación estricta contra los procedimientos instalados se mantiene intacta.
No se ejecuta SQL al realizar esta prueba; su log es lecturas-sql.log.
