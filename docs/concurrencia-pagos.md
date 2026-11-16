# Concurrencia y recuperación de pagos — correcciones 5 y 6

Trabajo en correccion-servicios-habitaciones. Publicación autorizada solo en esa rama; sin merges ni SQL ejecutado.
Se conservan la asociación habitación-reserva, los históricos y los archivos previos.
Este documento describe la versión FINAL de la implementación local.

## Escritores y protocolo

04_ConcurrenciaPagos.sql prepara las seis definiciones autorizadas:
sp_Pago_Insertar, sp_Pago_ActualizarEstado, sp_Pago_RegistrarServicios,
sp_Pago_RegistrarSaldoAlojamiento, sp_DetalleServicio_Insertar y sp_Reserva_CheckOut.
Incluye además sp_ReservaTemporal_ConsultarConfirmacion, de solo lectura,
para recuperar una confirmación ya realizada. No cambia sp_ConfirmarReserva,
check-in, cancelación, disponibilidad, reportes ni la migración de categoria.

Los cobros especializados delegan en sp_Pago_Insertar: no tienen INSERT propios.
Pagos nuevos: transacción -> bloqueo de operación -> bloqueo de reserva ->
fila de reserva -> saldos/pagos -> escritura -> COMMIT -> resultado.
Aprobación/rechazo: lectura inicial solo de enrutamiento -> transacción ->
bloqueo y fila de reserva/temporal -> pago -> revalidación -> escritura -> COMMIT.
Si el vínculo cambió durante una confirmación, se rechaza con error reintentable;
nunca se sigue con una reserva distinta de la que se bloqueó.

El recurso compartido es HotelTramonto:reserva:<ID>; para temporales se usa
HotelTramonto:temporal:<ID> y UPDLOCK,HOLDLOCK sobre ReservasTemporales.
ConfirmarReserva ya toma esa misma fila primero, por lo que coordina sin cambiar
su definición. En temporales, los escritores toman habitación antes del pago,
respetando el orden temporal -> habitación -> pago de la confirmación.
Operaciones locales simultáneas también se excluyen con un bloqueo de archivo.

Los locks son transaccionales, tienen timeout 10 segundos y se liberan en rollback.
No se admiten transacciones externas. Los índices por reserva/temporal reducen
bloqueos innecesarios. Un orden consistente elimina las inversiones identificadas;
NO puede probarse ausencia absoluta de deadlocks en un motor no ejecutado, con
otros escritores, triggers o escalado de locks. Ante error/timeout, conservar la
misma operación y reintentar; nunca inventar otra para el mismo cobro incierto.

## Reglas económicas

- Servicios: total de DetalleServicios.cantidad * precio_unitario histórico,
  menos pagos APROBADO de tipo SERVICIOS_ADICIONALES. Nuevos cargos requieren
  CHECKIN y fecha_checkout NULL.
- Alojamiento: Reservas.total menos APROBADO de PAGO_INICIAL y SALDO_ALOJAMIENTO.
  El saldo se cobra en CONFIRMADA y sin fecha_checkout.
- El alta general admite importes parciales positivos dentro del saldo.
  Los cobros especializados cobran el saldo completo calculado bajo el bloqueo.
- Los pendientes NO se descuentan como dinero pagado. Al aprobar cada uno se
  recalcula el saldo; si no cabe completo, se rechaza sin cambiar su monto.
- Repetir la aprobación o rechazo del mismo id_pago y estado devuelve éxito sin
  otra escritura. Cambiar un estado terminal a otro se rechaza.
- La temporal solo recibe PAGO_INICIAL: monto exactamente 50 % o 100 % según
  modalidad, temporal pendiente/vigente y sin otro inicial aprobado.
- El flujo temporal -> pago aprobado -> ConfirmarReserva conserva sus contratos.
  El pago se religa a la definitiva por el procedimiento existente.
- Se permiten nuevas solicitudes PENDIENTE, APROBADO o RECHAZADO. No se introduce
  un circuito de reembolsos; insertar un REEMBOLSADO nuevo por el alta genérica se
  rechaza. Los pagos REEMBOLSADO existentes no se modifican.
- No se corrigen sobrepagos históricos ni se recalculan precios antiguos.

## Identidad durable

Pagos.id_operacion y solicitud_hash son NULL para históricos, sin backfill.
Un índice único filtrado y un CHECK protegen la identidad de los registros nuevos.
El hash SHA-256 se calcula en SQL sobre JSON con campos explícitos de la solicitud
original, incluidos monto solicitado (NULL para cobrar saldo), tipo, estado inicial,
usuario, método y destino original temporal/definitivo.
No se compara el vínculo mutable del pago después de confirmar la temporal.

Misma operación y solicitud devuelve id_pago/importe/método/estado del registro
original, aunque se hayan agregado consumos o cerrado la reserva.
Datos diferentes con el mismo GUID se rechazan.
FormPagos usa ese resultado exacto; ya no muestra el último pago de la lista.

Aprobar/rechazar usa la identidad durable (id_pago, estado solicitado), sin crear
otro pago. Check-out usa id_reserva y la marca de cierre como identidad estable;
repetir un cierre ya FINALIZADA devuelve su resultado sin escribir de nuevo.

DiarioOperaciones guarda solicitud, GUID y referencia temporal en
%LOCALAPPDATA%/HotelTramonto/Operaciones/<hash-del-servidor-y-base>.
No guarda credenciales. Los archivos son locales al usuario Windows; deben
mantener sus permisos privados. Se escribe antes de llamar SQL, con archivo
temporal, Flush(true) y reemplazo; un archivo corrupto no se sustituye por un GUID nuevo.
Se conserva hasta que el usuario ve y confirma el resultado. Se archiva el recibo
local; confirmar un recibo viejo no completa una operación nueva.
Las solicitudes diferentes no pueden reemplazar una pendiente.

Al abrir Pagos, Nueva reserva o Consumos se ofrece recuperar la solicitud guardada
en ese contexto. No se reconstruye desde controles modificados.
Las validaciones SQL inequívocas sin escritura archivan solicitudes rechazadas,
salvo el flujo completo de reserva, que puede contener un pago ya realizado.

Si el alta temporal pierde su respuesta ANTES de guardar su ID, se conserva una
marca de creación incierta y se bloquea su recreación automática. Requiere
conciliación; no se implementó idempotencia en sp_ReservaTemporal_Insertar,
fuera del alcance autorizado. Si el ID ya se guardó, se reutiliza, se recupera
el pago y se consulta la confirmación. Una confirmación repetida por otra llamada
se recupera con la consulta de solo lectura, sin cambiar ConfirmarReserva.
Una temporal expirada o una confirmación rechazada después de cobrar requiere
resolución operativa: no se reembolsa ni se crea otra reserva automáticamente.

No borrar el diario, cambiar de computadora para repetir un cobro incierto ni
limpiar archivos locales manualmente: primero conciliar con la base.
La idempotencia identifica solicitudes, no transferencias bancarias externas.
Dos GUID distintos de operadores diferentes no prueban que se recibió dinero
distinto; las validaciones de saldo impiden sobrepagar el saldo registrado, pero
un nuevo saldo legítimo requiere confirmación humana de su cobro.

## Instalación protegida y recuperación

TODOS los scripts se preparan para HotelTramontoDB_Pruebas. No se ejecutó ninguno.
Usar una copia desechable respaldada, verificar instancia y detener clientes.

1. Aplicar previamente la compatibilidad de Servicios.categoria de corrección 2
   siguiendo su propia documentación; aquí solo se comprueba, no se duplica.
2. 00_Diagnostico.sql: guardar definiciones y pagos/importes previos de forma privada.
3. 01_Instalar.sql: asociación, consumos y cierre. SOLO instalación inicial.
4. 02_Verificar.sql: integridad de asociaciones.
5. 04_ConcurrenciaPagos.sql: escritor único, aprobación y recuperación de confirmación.
   También instala las versiones finales de consumo/cierre; admite reejecución
   y reactivación después de suspender escrituras.
6. 05_VerificarConcurrencia.sql: índice, operaciones, anomalías previas y comparación
   exacta de las definiciones auditadas. Una diferencia requiere investigación.
   Comparar también los registros previos guardados; no basta con totales.
7. Verificar permisos y probar la aplicación actualizada en Windows.
   Los clientes antiguos sin GUID no pueden registrar pagos nuevos; SQL falla
   explícitamente, sin generar identidades que permitirían duplicar reintentos.

Después de instalar 04, 01 y la recuperación antigua 03 se bloquean para no
restaurar escritores inseguros. No ejecutar el script monolítico de procedimientos
sobre esta instalación; sobrescribiría definiciones actuales.
Ante incidencia, 06_SuspenderEscrituras.sql bloquea los SEIS escritores sin borrar
filas, columnas ni índices. No es un downgrade transparente. Revisar respaldos
y resolver la causa antes de reactivar con 04. Las instalaciones/suspensiones
se detienen ante módulos firmados para coordinar con el DBA.

El rol de corrección 2 debe conservar EXECUTE de los procedimientos existentes y,
tras revisión independiente, necesitará:
GRANT EXECUTE ON OBJECT::dbo.sp_ReservaTemporal_ConsultarConfirmacion TO [ht_app_compartida];
Es documentación, no un permiso ejecutado. No cambiar scripts de la otra rama.
Verificar que los usuarios no tengan DML directo, db_owner/sysadmin, membresías,
triggers o escritores externos que eludan el protocolo. No se afirma haber
validado permisos reales sin conectarse al servidor.

## Pruebas y límites

Las pruebas .NET verifican persistencia, reintentos tras reinicio, payload distinto,
exclusión local, confirmación de recibos, referencia temporal y diario corrupto.
Usan archivos temporales y dobles de datos; no ejecutan SQL.
Las comprobaciones estáticas verifican contratos, orden de locks, escritor único,
compatibilidad con confirmación y preservación de definiciones.
NO son compilación ni ejecución de T-SQL.

tests/HotelTramonto.VerificacionSql convierte las 28 pruebas a C#/.NET 10.
Las ejecuta solo en una base temporal con GUID y marca verificados de SQL Express.
No requiere Python, LocalDB, pyodbc ni ODBC. Ver el comando y los controles en
[verificacion-windows-servicios.md](verificacion-windows-servicios.md).
Las pruebas SQL siguen pendientes: aquí solo se ejecutaron compilación y pruebas
sin SQL. Cada contendiente usa su propia conexión física.

Pendientes antes de declarar preparación operativa:
- Ejecutar y revisar esas pruebas T-SQL y la suite multisesión en SQL Server.
- Verificar UI, recuperación tras terminar el proceso y permisos Windows reales.
- Conciliar anomalías históricas informadas por 05 sin modificar sus importes.
- Verificar escritores, triggers y permisos del servidor real.
- Conservar/revisar el diario local ante pérdida de disco o referencia temporal incierta.

Limitación previa fuera de los seis procedimientos: sp_Reserva_CheckIn lee estado
y luego hace UPDATE sin condición de estado en el UPDATE. Un check-in antiguo
demorado podría reabrir el estado después de un cierre. No se modificó porque no
forma parte de los seis autorizados. Los escritores nuevos comprueban también
fecha_checkout y no permiten nuevos consumos/cargos después de esa marca, aunque
el estado sea reabierto por ese camino. Esto no corrige el estado de la reserva.
La cancelación tampoco comparte todo el protocolo; una garantía global del ciclo
de vida requiere una revisión específica. No se afirma que ese ciclo esté resuelto.

## Resultado de validación local

- Compilación .NET 10 con EnableWindowsTargeting: 0 errores, 121 advertencias
  de nulabilidad/delegados del código existente.
- 81 pruebas .NET aprobadas, 0 fallidas, 0 omitidas dentro del filtro seguro.
  Tres pruebas SQL de README se excluyeron expresamente.
- 19 comprobaciones estáticas del modelo y concurrencia aprobadas: 8 de servicios/habitación, 7 de
  concurrencia de pagos y 4 de regresión del historial.
- 28 pruebas de integración SQL preparadas, ninguna ejecutada.
- Sin cambios de Designer, dependencias ni lockfiles; publicación solo en la rama autorizada, sin merges.

El cálculo C# del 50 % ahora utiliza MidpointRounding.AwayFromZero, coincidente
con ROUND de SQL Server (por ejemplo, 100.01 -> 50.01). No cambia importes guardados.
Las pruebas de confirmación y de pago inicial preparado incluyen ambos modos;
el redondeo se comprueba sin SQL en la suite .NET.


## Automatización Express/.NET actual

La automatización LocalDB anterior quedó sustituida por el ejecutor .NET 10,
conservando las 28 comprobaciones de integración en CasosSql.cs.
Se usa la instancia autorizada LAPTOP-5TSJJUP8/SQLEXPRESS y una base temporal
HotelTramontoDB_HT56_<GUID>, jamás las dos bases originales.
Los scripts SQL protegidos no se editaron: se adaptan en memoria solo para la
copia identificada por su GUID, bajo las barreras documentadas.

La guía [verificacion-windows-servicios.md](verificacion-windows-servicios.md)
contiene el comando para la Terminal de Visual Studio, los requisitos de respaldo,
la pausa INSTALAR, comparación de once tablas históricas y eliminación sin forzar
sesiones. No hay nuevos paquetes NuGet: SqlClient se reutiliza desde Datos.

Los 28 casos SQL NO se ejecutaron. Continúan pendientes hasta que se cree el
respaldo posterior a categoria y anterior a correcciones 5/6.
No se modificó ningún procedimiento económico ni sp_Reserva_CheckIn en este ajuste.

Validación de la alternativa Express: 81 pruebas .NET sin SQL aprobadas y
34 comprobaciones internas del ejecutor aprobadas, además de las 26 estáticas
de scripts/historial/automatización. Los 28 casos SQL están compilados en C#,
pero no ejecutados. El recorrido requiere un respaldo aún pendiente de crear.
