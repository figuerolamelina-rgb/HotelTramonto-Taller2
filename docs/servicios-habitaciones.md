# Correcciones 5 y 6: consumos por habitación y reserva

Implementación local en correccion-servicios-habitaciones, basada en
3bed9df4c0567bee16d885a7d2de27a9b129f333 (integración con PR #10).
Publicación autorizada únicamente en esta rama, sin merges ni ejecución de SQL Server.

## Modelo y reglas

Se extiende DetalleServicios con id_habitacion INT NULL y
id_operacion UNIQUEIDENTIFIER NULL. La FK compuesta (id_reserva,id_habitacion)
referencia la clave única existente de DetalleReservas. No existe una relación
independiente que permita consumir en habitaciones ajenas a la reserva.
La FK no tiene cascadas; impide cambiar o quitar una asociación con consumos.
Los históricos conservan ambos campos NULL, sus cantidades y sus precios.
No hay backfill, borrado ni reasignación automática.

Un CHECK exige ambos campos nulos o ambos presentes; un índice único filtrado
impide repetir un id_operacion no nulo. La escritura operativa debe usar el
procedimiento: el esquema no puede distinguir por sí solo una inserción manual
con ambos campos NULL de un registro histórico. No conceder INSERT directo a la
identidad operativa. No se modifica ningún permiso en esta rama.

El procedimiento exige CHECKIN, habitación perteneciente a la reserva, servicio
activo y no Incluido, cantidad positiva, precio positivo con subtotal acotado.
Obtiene el precio del catálogo SQL y verifica que coincide con el mostrado al
operador. Si cambió, rechaza la solicitud para que se vuelva a confirmar.
Guarda el precio SQL; nunca recalcula consumos anteriores con el catálogo actual.
No cambia el total de alojamiento.

Los reintentos buscan primero id_operacion: misma reserva, habitación, servicio,
cantidad y precio devuelven éxito sin insertar de nuevo, incluso si después
cambió el catálogo o terminó la reserva. Datos diferentes generan error 51314.
Otro consumo legítimo del mismo servicio utiliza otro identificador.

## Formularios y capas

DetalleServicio incorpora habitación y operación opcionales para históricos.
DetalleServicioDatos envía los parámetros y admite filtro opcional por habitación.
IDetalleServicioDatos permite probar Negocios sin conectarse a SQL.
DetalleServicioNegocios valida entrada y conserva el identificador de reintento.

FormRegistrarConsumos agrega un selector de habitaciones de la reserva y un
filtro opcional. Sin filtro muestra también los históricos sin habitación.
Bloquea los datos mientras una solicitud de resultado incierto está pendiente,
conservando su GUID para reintentar en esa misma ventana. Un rechazo SQL de
validación permite corregir y volver a cargar catálogo. Ante un resultado incierto,
no cerrar y volver a registrar el mismo consumo como una nueva operación.
El GUID y la solicitud se persisten en el diario local para recuperar tras cierre;
ver concurrencia-pagos.md para el protocolo final y los casos de conciliación.
No existe garantía de deduplicar una solicitud nueva con otro GUID.

FormPagos muestra la habitación sin filtrar el cobro global de la reserva.
Se mantienen los cálculos y tipos de pago existentes. No se editaron Designer.
La revisión visual, escalado y navegación requieren Windows.

## Orden de instalación, exclusivamente en pruebas

No se ejecutó ningún script. Todos los scripts de este directorio rechazan bases
distintas de HotelTramontoDB_Pruebas mediante comparación binaria, sin USE ni GO.
El nombre de base no prueba que la instancia sea segura: comprobar también el
servidor y usar una COPIA DESECHABLE, respaldada y sin clientes activos.

1. Integrar posteriormente, con aprobación independiente, la compatibilidad
   de Servicios.categoria preparada en correccion-permisos-habitaciones.
   No se copió ni duplicó aquí. Referencia revisada:
   2ca61cf79dd170e8349e917ed5d0bf4ca7aeeb7e.
   Seguir su documentación: diagnóstico/respaldo, columna, clasificación explícita,
   actualización de procedimientos y comprobaciones. No habilitar scripts de
   permisos bloqueados ni fusionar ramas automáticamente.
2. Ejecutar/revisar servicios-habitaciones/00_Diagnostico.sql. Guardar fuera del
   repositorio las definiciones previas, resultados y respaldo completo.
   Deben existir categoria VARCHAR(30) NULL, clasificaciones resueltas y contratos
   compatibles de sp_Servicio_Insertar/Modificar/Listar. La instalación comprueba
   estos requisitos, incluidos los metadatos de la columna devuelta por Listar.
3. Revisar 01_Instalar.sql: transaccional, aditivo e idempotente. Verifica las
   columnas, índice y FK existentes en lugar de reemplazarlos a ciegas.
   Si hay firmas en los procedimientos, se detiene para coordinar con el DBA.
   No modifica datos existentes ni instala migraciones de categorías.
4. Ejecutar 02_Verificar.sql y comparar consumos/importes con el diagnóstico.
5. Instalar 04_ConcurrenciaPagos.sql y verificar con 05_VerificarConcurrencia.sql
   según concurrencia-pagos.md antes de desplegar.
6. Desplegar conjuntamente aplicación y procedimientos: los clientes anteriores
   sin habitación/operación ya no pueden registrar nuevos consumos.
7. Probar con la identidad operativa, con EXECUTE sobre los procedimientos usados.
   Las consultas de habitaciones reutilizan sp_DetalleReserva_ListarPorReserva.
   No se otorgan SELECT/DML generales ni se sobrescriben permisos de habitaciones.

## Procedimientos cambiados

- sp_DetalleServicio_Insertar: habitación, operación y validaciones transaccionales.
- sp_DetalleServicio_ListarPorReserva: habitación, filtro opcional, históricos conservados.
- sp_Pago_RegistrarServicios: mismo contrato, cálculo y tipo económico; transacción.
- sp_Reserva_CheckOut: mismo contrato y condiciones de saldos; transacción.

Los tres procedimientos de escritura coordinan con sp_getapplock Exclusive
por HotelTramonto:reserva:<ID>, propietario Transaction y tiempo máximo 10 s,
más bloqueos de reserva y lecturas protegidas de saldos. No admiten una transacción
externa. No se cambia check-in, confirmación, disponibilidad ni procedimientos
generales de pagos. El archivo original Procedimientos_Almacenados.sql permanece
sin cambios; se instalan las cuatro definiciones desde la migración separada.

## Concurrencia final

Se implementaron los ajustes adicionales autorizados. El protocolo final, los seis
procedimientos, el diario persistente, los scripts 04/05/06 y sus límites se
describen en [concurrencia-pagos.md](concurrencia-pagos.md).
Los caminos generales de pagos ahora validan bajo el mismo protocolo; no se
modificó sp_ConfirmarReserva. No hay garantía verificada contra el motor hasta
ejecutar las pruebas opt-in en una copia desechable.

## Recuperación sin borrar datos

Detener todos los clientes y guardar un respaldo. Tras instalar 04, usar
06_SuspenderEscrituras.sql; 03 queda bloqueado para no restaurar pagos inseguros.
El siguiente recorrido 03 solo describe la etapa inicial antes de 04.
Suspende nuevos consumos (THROW explícito) y restaura las definiciones de consulta,
cobro y check-out de la base de esta rama. Conserva columnas, FK, índice, consumos
nuevos e históricos, precios y pagos. No deshace cobros ni asociaciones.
NO restaura automáticamente definiciones locales distintas de esa base:
comparar con las definiciones guardadas por 00_Diagnostico y adaptar con el DBA.
Tampoco es un downgrade transparente: deja el registro de consumos suspendido
y pierde la coordinación nueva de pago/cierre. No reabrir operación sin resolver.
Reinstalar 01 tras corregir la causa permite reactivar, conservando los datos.
No eliminar columnas para volver atrás ni restaurar una copia sobre datos posteriores.

## Pruebas

Sin SQL:
- dotnet build HotelTramonto.slnx --no-restore -p:EnableWindowsTargeting=true
- dotnet test HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj --no-build --no-restore
  con el filtro seguro de README que excluye las tres pruebas SQL.
- python3 tools/verificar_servicios_habitaciones.py
- python3 tools/verificar_historial_sql.py

Las pruebas .NET nuevas verifican validaciones, parámetros, reintentos y lectura
histórica mediante un doble de Datos. No simulan una FK ni prueban concurrencia SQL.
Las comprobaciones estáticas verifican estructura y contratos de scripts;
no son compilación ni ejecución de T-SQL.

Pendiente contra SQL Server: tests/servicios_habitaciones_sql.py prepara 28 pruebas
opt-in de integración con pyodbc y un driver ODBC SQL Server ya disponible.
Usa HOTELTRAMONTO_SQL_TEST_CONNECTION como cadena privada, nunca en el repositorio
ni como argumento visible. Verifica DB_NAME antes de insertar.
Usar el runner aislado de docs/verificacion-windows-servicios.md.
No ejecutar directamente el archivo Python: exige la marca de la copia LocalDB.

La copia necesita un usuario y huésped existentes y tres habitaciones.
La suite crea servicios y reservas de prueba propios y LOS CONSERVA; no borra
datos ni modifica consumos anteriores. No ejecutarla en la base original.
Comprueba una/varias habitaciones, FK, servicio inactivo, estados sin CHECKIN,
precio cambiado, reintentos y payload distinto, históricos sin habitación,
dos cobros simultáneos, reintentos simultáneos, consumo/pago/cierre concurrentes.
Las pruebas de concurrencia usan conexiones independientes y una barrera.
Un timeout/deadlock inesperado falla la prueba; no cuenta como aprobación.
La suite ampliada cubre también altas generales, aprobaciones, alojamiento y confirmación; permanece sin ejecutar.
Repetir corridas de concurrencia en la copia y revisar resultados antes de integrar.

También queda pendiente probar UI en Windows con ambos roles operativos,
selección vacía, catálogo actualizado, reserva cerrada, fallo de conexión/reintento,
filtro por habitación e históricos, y verificar los reportes/saldos previos
con importes de referencia. El reporte de servicios sigue sumando precio histórico
desde DetalleServicios y no necesita JOIN con todas las habitaciones.

## Archivos de esta entrega

Modificados:
- HotelTramonto.Entidades/Entidades/DetalleServicio.cs
- HotelTramonto.Datos/DetalleServicioDatos.cs
- HotelTramonto.Negocios/DetalleServicioNegocios.cs
- HotelTramonto.Presentacion/FormRegistrarConsumos.cs
- HotelTramonto.Presentacion/FormPagos.cs

Nuevos:
- HotelTramonto.Datos/IDetalleServicioDatos.cs
- HotelTramonto.Pruebas/ServiciosHabitacionTests.cs
- base de datos HotelTramonto/servicios-habitaciones/00_Diagnostico.sql
- base de datos HotelTramonto/servicios-habitaciones/01_Instalar.sql
- base de datos HotelTramonto/servicios-habitaciones/02_Verificar.sql
- base de datos HotelTramonto/servicios-habitaciones/03_Recuperacion.sql
- tools/verificar_servicios_habitaciones.py
- tests/servicios_habitaciones_sql.py
- docs/servicios-habitaciones.md

Comprobaciones locales de la versión completa: 72 pruebas .NET aprobadas,
19 comprobaciones estáticas aprobadas entre servicios, concurrencia e historial.
Tres pruebas .NET que acceden a SQL quedaron explícitamente excluidas.
Las 28 pruebas de integración SQL están preparadas y NO ejecutadas.
No se cambió ninguna dependencia, proyecto, Designer ni script de otra corrección.

La lista completa de ajustes adicionales y resultados actualizados está en concurrencia-pagos.md.

La automatización final y el orden de instalación están en [verificacion-windows-servicios.md](verificacion-windows-servicios.md).
