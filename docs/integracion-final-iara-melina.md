# Integración final de Iara y Melina — 16/11/2026

## Fuentes y alcance

Se realizó fetch y comparación de las cinco ramas y de las referencias publicadas de los PR. El checkout de nube estaba limpio. Se integró `develop` sobre `correccion-servicios-habitaciones` mediante un merge real, conservando los commits originales y las correcciones de `5d747c9`; no se usaron cherry-picks repetidos, force-push ni worktrees. No se modificaron directamente las ramas protegidas ni se fusionó el PR #13.

| Fuente al iniciar y al volver a comprobar antes de publicar | Commit | Situación |
| --- | --- | --- |
| `feature/iara` | `11440cf6e38b5c7dbc3ddd65ddc751a28683ea56` | Nueve commits funcionales pendientes; incorporados conservando su historial. |
| `feature/melina` | `d5467c7d7683a3017f1b75bb0213017a534d4545` | Ya era antecesor de `5d747c9`; no se reaplicaron sus cambios. |
| `develop` | `b607d885f6f50217734afee9c50a16b908411cd0` | Contenía el merge del PR #11 de Iara y el merge anterior del PR #8. Se incorporó como segundo padre de la integración. |
| `integracion-melina-iara` | `f6cae70eca93f3df646220b975ad4953ee501c35` | Base del PR, conservada. |
| `correccion-servicios-habitaciones` | `5d747c946ccfcfa3a99d464564bb79902762574c` | Primer padre y base de las correcciones preservadas. |

La API GitHub respondió `Forbidden`. Se utilizó como alternativa de solo lectura la página del PR y su JSON embebido: se verificó **PR 13, estado DRAFT, base `integracion-melina-iara` en `f6cae70`, cabecera `correccion-servicios-habitaciones`**, con el SHA publicado coincidente con Git. No se editaron descripción, revisiones ni estado. Por Git se verificaron además las ramas y los commits de merge; el PR #11 está en la historia de develop. El PR #13 no se convirtió a listo ni se fusionó. Su SHA final se informa en la entrega y se comprueba contra `refs/pull/13/head`.

El merge funcional y código validado quedó publicado como [`a362b161e76c8b2f24127d3a80e991bfe8f1cc0a`](https://github.com/figuerolamelina-rgb/HotelTramonto-Taller2/commit/a362b161e76c8b2f24127d3a80e991bfe8f1cc0a), con padres `5d747c9` y `b607d88`. La confirmación posterior de metadatos se agrega como documentación, sin cambiar el código probado.

### Último trabajo de Iara incorporado

Los nueve commits posteriores a la base común `bc267d2` son:

| Commit | Función incorporada |
| --- | --- |
| `fa9a31b` | Actualización de reservas, pagos, servicios y reportes. |
| `8ff1d02` | Restricción de modificaciones de habitaciones para recepción. |
| `d59b668` | Encabezados y visualización de huéspedes. |
| `b917714` | Generación de comprobantes PDF. |
| `eb0a341` | Mejoras de diseño, pagos y comprobantes. |
| `23ee77d` | Búsqueda por DNI y distribución adaptable de Pagos. |
| `a49ca2b` | Filtros y encabezados de habitaciones y reservas. |
| `73a6b71` | Habitaciones, reservas y formulario propio de historial. |
| `11440cf` | Selección de habitación con check-in desde Servicios para registrar consumos. |

Se conservaron los colores, distribución, formularios y reportes con pestañas de Iara. QuestPDF `2026.9.2` es la dependencia que ya incorporaba para PDF; se usa la misma versión en la prueba del generador, sin cargar Windows Forms en Linux. No se modificaron archivos Designer. La generación PDF se ejecutó de verdad; la aceptación visual de ventanas sigue pendiente.

### Trabajo de Melina y correcciones conservadas

Se preservaron esquema y procedimientos base, Datos, Entidades, autenticación y contraseñas, usuarios/roles/tipos, conexión privada por configuración local, Negocios, historial y las correcciones coordinadas de consumos/pagos/reservas. La conexión, el monolítico, el procedimiento de historial y los scripts 01–08 se verifican byte a byte contra `5d747c9` en las pruebas nuevas.

La comparación de archivos encontró dos ausencias respecto de la antigua rama Melina: `Entidades/Class1.cs` y `Negocios/Class1.cs`, clases vacías eliminadas previamente por `4dd66ba` al introducir entidades y validaciones reales. No son funcionalidades pendientes ni se restauraron como código duplicado. No se detectaron archivos funcionales faltantes de las fuentes publicadas; esto no acredita archivos que solo existan en computadoras personales.

Los diez SQL originales nuevos de Iara se conservaron **byte a byte**, incluido el archivo UTF-16, en `base de datos HotelTramonto/referencias-iara`. No son instaladores de la versión final. El inventario y sus SHA-256 están en [inventario de integración](inventario-integracion-iara-melina.json). No se descartó código SQL ni se ejecutó ese directorio.

## Conflictos y soluciones

Hubo conflictos de Git en siete archivos: `HabitacionNegocios`, `Form1`, `FormHabitaciones`, `FormHuespedes`, `FormNuevaReserva`, `FormPagos` y `FormRegistrarConsumos`. Además se revisaron las fusiones automáticas: compilar no bastaba para detectar contratos incompatibles.

| Conflicto o defecto concreto | Resolución integrada |
| --- | --- |
| Diseño y filtros de habitaciones frente a la autorización C2 | Se conserva la interfaz de Iara y la política ADMINISTRADOR de Negocios/UI. Se combinan las consultas de activas e inactivas para que el filtro no prometa registros que nunca carga. Reactivar abre la lista protegida de inactivas aun sin selección en la grilla principal. La suite estática detectó y permitió corregir esa regresión. |
| Pagos por DNI frente a cobros recuperables | Diseño adaptable y búsqueda DNI de Iara, más consulta explícita por ID. Una nueva búsqueda, DNI editado, error o pérdida de selección invalida el destino anterior. La selección por teclado y ratón usa el mismo contexto validado. |
| Comprobante del «último pago» frente al resultado de la operación original | El comprobante automático usa exactamente el `id_pago` devuelto por la operación; la impresión histórica usa el pago seleccionado y comprueba identidad/estado/importe. Un pago rechazado, pendiente o reembolsado no genera PDF como aprobado. Fallar al imprimir no crea otro cobro. |
| `sp_Pago_ActualizarEstado` con cuatro parámetros en Iara frente a dos en 04 | Se conserva la firma coordinada de dos parámetros, sus validaciones y reintentos. La variante de actor/motivo no tenía contrato SQL publicado ni controles que la invocaran; no se inventó auditoría ni se ignoraron parámetros silenciosamente. Una auditoría persistente adicional requiere su propio contrato revisado. |
| Alta «Crear y confirmar» alternativa sin GUID | Se mantiene la experiencia de crear/confirmar y su fachada de Negocios sobre `FlujoReservaNegocios` con solicitud y contexto durables. No se conecta el cliente al escritor alternativo sin operación. Se conservan temporal, 50/100, disponibilidad, pago, confirmación y recuperación, incluida conciliación cuando se pierde el ID temporal. |
| Historial nuevo frente al procedimiento C3/C4 ya validado | Se usa el formulario de Iara con autorización y mensajes seguros; consume el historial validado con número/tipo. Se enriquece con importe/observaciones mediante la consulta existente de reservas, sin sumar dos veces importes ni modificar las tablas originales. Cuenta reservas distintas, no filas por habitación. |
| Consumo desde una reserva frente a selección global de habitación | Ambos recorridos funcionan en código: Pagos fija la reserva; Servicios permite elegir un par habitación/reserva con CHECKIN. Se conserva la habitación obligatoria, filtro y solicitud/GUID original en los reintentos. SQL sigue decidiendo asociación, estado y precio. |
| Llamadas nuevas sin procedimiento SQL publicado | Se prepararon consultas de habitaciones con check-in y comprobante aprobado, con sus contratos y permisos explícitos. No se asumió que ya existen en la computadora Windows. |
| Fecha histórica de consumo que el SQL original rellenaría con la fecha de instalación | 09 agrega una fecha nullable con default solo para nuevas filas, **sin WITH VALUES**. No inventa fechas históricas. El reporte usa media ponderada y distingue fecha de consumo de entrada prevista cuando la fecha histórica es desconocida. Los cargos valorizados no se presentan como dinero efectivamente cobrado. |

### Las seis correcciones

1. Mínimo de ocho caracteres y hashing conservados; pruebas existentes repetidas.
2. Administración de habitaciones/tipos solo ADMINISTRADOR, con rechazo directo en Negocios.
3. Historial autorizado para ADMINISTRADOR/RECEPCIONISTA; GERENTE no obtiene esa ruta.
4. Número y tipo actuales, fechas previstas y marcas de check-in/out de la reserva; no se afirma ocupación real por habitación.
5. Consumo vinculado al par reserva/habitación, sin permitir habitaciones ajenas.
6. Escrituras de consumo mediante procedimiento almacenado, validación SQL de precio/estado/servicio e identidad durable.

También se conservan los mensajes de rechazo frente a resultado incierto, el diario recuperable, la propuesta transaccional de check-in/cancelación y el registro local de errores sin mensajes técnicos sensibles. Los encabezados no cambian nombres físicos de columnas ni DataPropertyName.

## Archivos principales y validación

Datos: `HabitacionDatos`, `IHabitacionDatos`, `PagoDatos`, `IComprobantePagoDatos`; la vía alternativa sin GUID de `ReservaDatos` no se usa. Negocios: políticas existentes y las adaptaciones de habitaciones, huéspedes, pagos/reservas; nuevos `SeleccionReservaPagos`, `FiltrosHabitaciones`, `DetalleHistorialHuesped` y `ComprobantePagoNegocios`.

Presentación: menú, Habitaciones, Huéspedes, `FormHistorialHuespedes`, Nueva reserva, Pagos, Consumos, Servicios, Reservas, Reportes, formateador común y generador PDF. Se conservan las pestañas de ingresos (cuatro resultados) y estadísticas (tres resultados). Los filtros de estado de habitación reflejan el catálogo; el selector de consumo se basa en la reserva con check-in, sin confundir ambos estados.

Pruebas/operación: `IntegracionIaraMelinaTests`, `ConfiguracionUiTemporalTests`, adaptación de los dobles de habitaciones, `ConfiguracionUiTemporal` y el modo opcional del ejecutor. Se actualizaron los manifiestos de integridad para los archivos revisados, manteniendo los filtros de pruebas seguras, los 60 procedimientos de C2 y los 64 contratos actuales en la propuesta separada de permisos.

El listado completo de cambios está disponible con `git diff --name-status 5d747c9..HEAD`. La lista de fuentes y sus commits permite contrastar las adaptaciones sin volver a aplicar trabajo ya integrado.

| Verificación sobre la integración | Resultado |
| --- | --- |
| Reconstrucción completa .NET 10, WindowsTargeting, un proceso MSBuild | 0 errores; 209 advertencias de nulabilidad/delegados del código de formularios integrado. No se suprimieron advertencias. |
| Pruebas .NET seguras | **154 aprobadas, 0 fallidas, 0 omitidas dentro del filtro**. Incluyen las 136 anteriores y 18 casos de integración/configuración, incluida generación real de PDF. |
| Pruebas .NET con SQL local excluidas expresamente | 3 no ejecutadas; no se cuentan como aprobadas ni omitidas por el runner. |
| Comprobaciones estáticas Python | **60 aprobadas, 0 fallidas**. |
| Guardas locales `--self-test` | **44 aprobadas**, sin abrir SQL. |
| Sintaxis con Microsoft ScriptDom 180.154.0, gramática TSQL140 | 7 scripts (07–10 y propuesta de permisos), 20 cuerpos SQL completos embebidos, 0 errores de sintaxis. Herramienta instalada fuera del repositorio; no ejecuta SQL ni valida comportamiento transaccional. |
| Integración SQL actualizada | **36 no ejecutadas**, 0 resultados nuevos atribuibles a SQL. Las 28 aprobadas anteriormente son antecedente, no aprobación de este commit. |
| Windows Forms / identidad Windows efectiva / pausa UI sobre SQL real | No ejecutados en nube. El generador de configuración temporal y sus rechazos sí se probaron sin conexión. |

El servidor `LAPTOP-5TSJJUP8` no se resuelve desde nube y no hay configuración ni identidad SQL inyectada. No se abrió ni alteró ninguna de las bases permanentes, no se probaron pagos sobre históricos y no se instalaron permisos. La reserva histórica 1 conserva su anomalía documentada; no es un caso de prueba.

## SQL preparado, no instalado

En la base de pruebas ya migrada, lo pendiente para revisión DBA es:

1. `servicios-habitaciones/07_CicloReserva.sql`: check-in y cancelación coordinados con el mismo bloqueo de reserva que pagos/consumos/cierre; predicado de estado y marca de cierre, saldo exacto y rollback.
2. `08_VerificarCicloReserva.sql`: definiciones exactas del incremento anterior.
3. **Nuevo `09_ConsultasIntegracion.sql`**: `sp_Habitacion_ListarConReservaCheckIn`, `sp_Pago_ObtenerComprobante`, reporte `sp_Reporte_ServiciosMasUtilizados` y `fecha_consumo` nullable para filas nuevas. Instalación atómica; rechaza módulos firmados, definiciones anteriores desconocidas, instancia/base incorrectas y columnas/defaults incompatibles. Si la fecha ya existía, no cambia sus datos ni acredita que sus valores históricos sean auténticos.
4. **Nuevo `10_VerificarConsultasIntegracion.sql`**: comprueba cuerpos, tipo y default de la fecha, sin ejecutar escrituras de negocio.

**No reinstalar 01/04, categoria ni el monolítico en la base permanente ya migrada.** La tarea solo preparó estos archivos. Antes de cualquier despliegue futuro: revisión DBA, clientes detenidos, transacciones/conexiones comprobadas, FULL COPY_ONLY en archivo nuevo, RESTORE VERIFYONLY y ensayo de restauración, más copia privada de definiciones/firmas/permisos. No sobrescribir respaldos anteriores. Los scripts guardan las definiciones previas en su salida para revisión y hacen rollback atómico ante error; no hay downgrade automático a escritores inseguros.

La propuesta `acceso-operativo/00_Diagnostico → 01_Instalar → 02_Verificar` ahora contempla las 64 rutas, incluidas las dos consultas nuevas; continúa sin instalarse. No se alteraron los scripts antiguos bloqueados de permisos C2. El usuario Windows sysadmin/db_owner **no** valida privilegios mínimos. Con una identidad técnica compartida SQL no distingue empleados funcionales; los controles de rol del cliente no son un límite frente a un cliente modificado. La separación fuerte requiere identidades individuales/grupos o un servicio autenticado que autorice cada sesión. Es una decisión de arquitectura pendiente, no una solución supuesta por esta integración.

## Mínimo local antes de aceptar el PR

### 1. Actualizar preservando el trabajo local

Revisar `git status` y conservar el diff diagnóstico de `FormHuespedes.cs` u otros cambios de Windows antes de actualizar `correccion-servicios-habitaciones`. No usar reset/force para resolverlos. El código/SQL que solo exista en computadoras de Iara o Melina no pudo inspeccionarse ni se afirma integrado.

### 2. Ejecutar las 36 pruebas y la aceptación visual en la misma copia temporal

Para investigar **51705**, usá el modo `--backup-ciclo` desde la raíz del worktree en PowerShell:

```powershell
dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup-ciclo (Read-Host 'Ruta completa del respaldo FULL anterior a 5/6 (.bak)')
```

Antes del pedido de `INSTALAR`, el ejecutor lee las definiciones de CheckIn y Cancelar
desde la copia restaurada y marcada. En la carpeta privada de la ejecución guarda
`diagnostico-ciclo.json` y, para cada procedimiento, los textos completos
`.historico.sql` y `.restaurado.sql` (si el módulo es visible). El JSON indica la
primera diferencia literal y canónica, con índice UTF-16 desde cero y contexto.
Una definición ausente o un cuerpo desconocido detienen el recorrido antes de 01.
Los originales versionados se encuentran en `Procedimientos_Almacenados.sql`.

El comparador de 07/08 reconoce los comentarios de línea iniciales, la cabecera
CREATE/ALTER/CREATE OR ALTER PROCEDURE con espacios, tabuladores o saltos, y el
prefijo opcional `dbo.`. Desde el nombre compara el texto ordinal, admitiendo
finales de línea equivalentes fuera de cadenas y espacio blanco al final del lote.
Se conservan los espacios, mayúsculas, saltos y comillas escapadas dentro de
literales. En el respaldo anterior a 5/6 se exige el contrato histórico. Al
reaplicar 07 se admite también la versión revisada, y 08 exige esa última versión.

Revisá ambas entradas del diagnóstico antes de escribir `INSTALAR`. Al finalizar,
`informe.txt` e `informe.json` registran los conteos de los 36 casos, la verificación
de históricos y la limpieza de la copia GUID.

Si 09 devuelve **51904**, el mensaje identifica el procedimiento. En la carpeta
privada del informe, `diagnostico-integracion.json` registra el estado de las tres
consultas inmediatamente antes de 09. Para cada versión admitida indica
`coincideSegun09` y la primera diferencia literal y tras la comparación de 09
(índice UTF-16 desde cero y contexto). Los archivos `.observada.sql`,
`.revisada.sql` y, para el reporte, `.historica.sql` e `.iara.sql` permiten
comparar los textos completos. El informe distingue un objeto ausente o invisible
de uno existente con definición ilegible. Los archivos quedan disponibles después
de eliminar la copia temporal.

Si aparece **51905** después de la instalación, buscá `diagnostico-paso10.json`
en la carpeta de esa ejecución. El verificador lee los tres módulos de la copia
justo antes de cada paso 10. El JSON de la última comprobación guarda el nombre,
el estado de visibilidad, los textos completos esperado y observado y la primera
diferencia literal y según la comparación de 10 (índice UTF-16 desde cero y
contexto). Ese único archivo alcanza para revisar el rechazo después de la limpieza.
El paso 10 reconoce la cabecera `CREATE   PROCEDURE` que SQL Express guardó en la copia.
Después de `PROCEDURE` exige la misma longitud y el mismo texto, incluidos el esquema,
los literales y los saltos de línea. Si ambas cabeceras son reconocidas, el índice de
`diferenciaSegun10` cuenta desde el carácter siguiente a `PROCEDURE`.

Para el reporte histórico del respaldo Pre56, 09 también reconoce el contrato
original con finales de línea de Windows y un CRLF adicional antes de `CREATE`
y después de `END;`. Construye esa variante desde `@anterior` y exige igual
longitud y contenido binario. El diagnóstico registra coincidencia histórica
incluso si hay una diferencia literal de formato.

Requisitos: Windows del equipo autorizado, Express local, .NET 10, respaldo FULL aprobado **posterior a categoria y anterior a 5/6**, y el archivo privado de conexión de pruebas ya existente. El ejecutor valida el motor/respaldo y se detiene ante esquemas o definiciones no reconocidos; no relajar sus controles. Si solo se dispone de un respaldo postmigración, no usarlo como si fuese el anterior: requiere un recorrido de restauración revisado.

En PowerShell, desde la raíz actualizada:

```powershell
$env:HOTELTRAMONTO_CONFIG = Join-Path $env:LOCALAPPDATA 'HotelTramonto\Pruebas\conexion.local.json'
dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup-ciclo-ui (Read-Host 'Ruta completa del respaldo FULL aprobado anterior a 5/6 (.bak)')
```

El comando pide la ruta real, sin asumir un nombre de archivo privado, y luego `INSTALAR` **solo para la copia GUID que acaba de validar**. Aplica 01/02/04/05/07/08/09/10 únicamente en esa copia; no instala nada en HotelTramontoDB ni HotelTramontoDB_Pruebas. Los 36 casos usan conexiones separadas cuando corresponde. El modo `--backup-ciclo` hace lo mismo sin pausa visual y elimina la copia al terminar; el modo legado `--backup` conserva su recorrido anterior de 28 casos.

Con `--backup-ciclo-ui`, solo después de aprobar los 36 casos y verificar los once históricos se ofrece el destino temporal. Se genera un JSON **nuevo**, sin credenciales, en el directorio privado del informe, derivado únicamente del GUID propio y conservando la confianza de certificado de la configuración existente. Se imprimen los comandos exactos para otra terminal. No se modifica el JSON original ni se conecta a su base para crear esa configuración.

En esa otra terminal, ejecutar los comandos impresos para abrir Windows Forms. Usar huéspedes/reservas de prueba nuevos y completar únicamente:

- ADMINISTRADOR/RECEPCIONISTA: buscar DNI y seleccionar con ratón/teclado; una búsqueda inválida no debe mantener la reserva anterior. Registrar consumo en su habitación, comprobar filtro, deuda que bloquea cierre, pago y comprobante del ID correcto.
- Servicios (administrador): abrir el selector global de habitaciones con check-in. Inactivos/incluidos y precio desactualizado deben rechazarse; sin habitación no debe escribirse un consumo.
- Reiniciar solamente la aplicación mientras muestra un resultado aún no aceptado; reabrir con el **mismo JSON temporal** y recuperar consumo/pago sin duplicados. Para recuperar una reserva ya cerrada, usar la consulta por ID en Pagos. No borrar el diario.
- RECEPCIONISTA sin administración de habitaciones/tipos/servicios; GERENTE sin rutas operativas y con reportes autorizados. Revisar también huésped sin reservas, historial con varias habitaciones, PDF y pestañas de reportes/scroll/escalado.

Cerrar Windows Forms antes de escribir `FINALIZAR` en la primera terminal. El ejecutor vuelve a verificar los históricos y elimina únicamente la copia acreditada, sin expulsar sesiones. EOF/cancelación también intenta la limpieza protegida. Si hay sesiones que impiden eliminarla, informa **limpieza no confirmada**, falla y deja la copia para revisión; no fuerza rollback ajeno. No reutilizar el JSON temporal después de su eliminación. El informe JSON distingue «usuario indicó finalizar» de una prueba visual automatizada: esa confirmación no convierte por sí sola los casos manuales en aprobados.

### 3. Permisos y decisión de merge

Antes de afirmar preparación operativa, revisar/ensayar la propuesta con una identidad Windows operativa real sin privilegios elevados, en una copia controlada y bajo revisión DBA. No se crean roles ni se conceden permisos con el comando anterior. Mantener explícito el límite de autenticación compartida.

**Recomendación:** conservar el PR #13 para aceptación final, sin fusionarlo todavía. El trabajo publicado de ambas integrantes y las seis correcciones está integrado y las verificaciones de nube pasan. Los bloqueos imprescindibles son la ejecución real de los 36 casos sobre el incremento, la aceptación visual/recuperación local y la revisión de permisos efectivos para el despliegue previsto. No atribuir a este commit las pruebas SQL anteriores ni confundir publicación del código con despliegue validado.
