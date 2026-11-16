# Corrección 2: permisos de habitaciones

**Estado de pruebas:** seguir `docs/pruebas-windows.md`. Los once scripts nuevos
solo admiten HotelTramontoDB_Pruebas y no cambian de base. La instalación de
permisos 02 sigue pendiente y tiene un THROW incondicional; las explicaciones de
instalación/reversión siguientes describen una etapa futura, no pasos autorizados
para este recorrido. No ejecutar los originales contra HotelTramontoDB.

Rama: `correccion-permisos-habitaciones`, basada en `c08eb6c` de
`integracion-melina-iara` (incluye PR #9). No se cambia la autenticación por
correo/contraseña, las cuatro capas ni los diseños de Iara.

## Alcance real de la protección

La política `PermisosHabitaciones` permite administrar exclusivamente a
ADMINISTRADOR; rechaza RECEPCIONISTA, GERENTE, roles desconocidos, vacíos o nulos.
Se aplica antes de acceder a Datos en las cuatro escrituras de habitaciones y
las tres escrituras de tipos. Las consultas no exigen un rol administrativo.
Los formularios conservan sus comprobaciones antes de las acciones y su aspecto;
usan ahora la misma política para habilitar/ocultar los controles administrativos.
Las operaciones de tipos se exponen por Negocios con autorización; no se agregan
pantallas nuevas. Desactivar un tipo afecta indirectamente la disponibilidad.

**Esta política protege el flujo de la aplicación sin modificar. No autentica
al llamador de una biblioteca ni protege frente a un cliente alterado.** Un rol
textual falsificado puede superar el control; una llamada directa a Datos no
pasa por Negocios. No se presenta esa situación como solucionada.

SQL Server sigue recibiendo una identidad Windows compartida. El rol técnico
`ht_app_compartida` NO es el rol de un empleado: incluye las operaciones necesarias
para todos los roles del programa, incluidas las administrativas. No se puede
DENEGAR su ejecución para recepcionistas sin denegarla también al administrador.
La restricción por empleado queda en la aplicación. Proteger también llamadas
SQL externas por empleado requeriría otra autenticación o un servicio servidor,
expresamente excluidos de esta implementación.

El refuerzo SQL limita el acceso directo a tablas y concede EXECUTE únicamente
sobre una lista explícita de procedimientos usados por Datos. Las denegaciones se limitan a INSERT/UPDATE/DELETE directos en Habitaciones y
TiposHabitacion; no se deniega SELECT ni se recorren todas las tablas. Los procedimientos
con la misma cadena de propiedad acceden a tablas sin conceder DML al cliente.
No se usa un id/rol del cliente ni SESSION_CONTEXT como prueba de autenticidad.
Tampoco se introduce EXECUTE AS OWNER, TRUSTWORTHY, credenciales ni hashes.
Los permisos de servidor, grupos Windows, public, suplantación, propietarios y
permisos de columna pueden cambiar el resultado: deben auditarse en la instancia.
En particular **sysadmin, dbo y db_owner no son identidades operativas válidas**:
pueden eludir o alterar la protección. No usar tampoco identidades con capacidad
de elevar permisos o modificar procedimientos. Las comprobaciones del script
son preventivas; no reemplazan revisar el token efectivo de la conexión real.

## Cambios C#

- `PermisosHabitaciones.cs`: única política para habitaciones y sus tipos.
- `HabitacionNegocios.cs`: aplica esa política a Insertar, Modificar, Desactivar
  y Reactivar; conserva validaciones y consultas existentes.
- `TipoHabitacionNegocios.cs`: incorpora Insertar, Modificar y Desactivar con
  autorización y validaciones; conserva Listar sin restricción administrativa.
- `IHabitacionDatos` e `ITipoHabitacionDatos`, implementadas por las clases de
  Datos existentes: permiten probar Negocios con dobles que detectan cualquier
  escritura indebida, sin abrir conexiones. Los constructores originales siguen
  creando las implementaciones reales; no se modifica ninguna operación SQL C#.
- Los cuatro formularios de habitaciones reutilizan la política. No se modifican
  Designer, controles, tamaños, colores ni la disposición visual.

## Scripts separados para revisar en SSMS

Ubicación: `base de datos HotelTramonto/permisos-habitaciones`.
**Ninguno se ejecutó contra SQL Server durante el desarrollo.**

1. `00_Diagnostico.sql`: solo lectura. Ejecutar como la identidad real usada por
   la aplicación. Revisa identidad, permisos, membresías, propietarios,
   procedimientos y triggers. Para metadatos completos, un DBA debe complementar
   la inspección: una definición NULL puede ser falta de visibilidad.
2. `01_ProcedimientosFaltantes.sql`: revisar e instalar como DBA primero en una
   base de pruebas. Agrega los cinco procedimientos confirmados como faltantes:

   | Procedimiento | Parámetros | Contrato C# |
   | --- | --- | --- |
   | dbo.sp_Habitacion_ListarInactivas | Ninguno | id_habitacion, tipo_habitacion, tipo, piso, numero, estado, activo |
   | dbo.sp_Habitacion_Reactivar | @id_habitacion INT | ExecuteNonQuery; solo activo = 1 |
   | dbo.sp_Servicio_ListarInactivos | Ninguno | id_servicio, nombre, descripcion, categoria, precio, activo |
   | dbo.sp_Servicio_Reactivar | @id_servicio INT | ExecuteNonQuery; solo activo = 1 |
   | dbo.sp_Usuario_Reactivar | @id_usuario INT | ExecuteNonQuery; solo activo = 1; sin cambiar rol, contraseña o hash |

   Se crean solo si no existen; no se sobrescriben definiciones. La instalación
   es transaccional y no invoca los procedimientos ni cambia filas. Los listados
   filtran activo = 0. Las reactivaciones no cambian reservas, pagos, estadías,
   precios, estados de habitación ni permisos. Los triggers reales de la base,
   si existen, deben revisarse antes de invocar cualquier reactivación.

   **Requisito de esquema pendiente:** Servicios.categoria es leída por C#, pero
   falta en Creacion_Base_HotelTramonto.sql y en los procedimientos originales de
   servicios. El diagnóstico ampliado muestra las columnas reales. Si falta,
   01 se detiene ANTES de instalar cualquiera de los cinco, sin inventar valores
   ni alterar la tabla. Si existe, el listado devuelve la columna real. También
   deben compararse los procedimientos de alta/modificación/listado de servicios
   de la instancia: C# envía @categoria y los originales versionados no lo aceptan.
   No se cambiaron esos originales; su eventual actualización es un trabajo
   separado que requiere conocer el esquema real y los valores existentes.
3. `02_PermisosMinimos.sql`: NO ejecutar con la identidad operativa actual:
   el diagnóstico del usuario dio sysadmin = 1 y db_owner = 1. Usar como destino
   un usuario Windows de base existente, operativo y sin privilegios elevados;
   puede ser una identidad compartida, no se requiere una cuenta por empleado.
   El DBA instala, pero la aplicación debe conectarse con la identidad limitada.
   No se eliminan roles o privilegios de tu cuenta actual ni se crean logins.

   Reemplazar localmente REEMPLAZAR_USUARIO_BASE. Exige que ht_app_compartida
   NO exista: no sobrescribe permisos/membresías previos. Crea ese rol técnico,
   concede los 60 EXECUTE explícitos y deniega INSERT/UPDATE/DELETE directos solo
   sobre Habitaciones y TiposHabitacion. La justificación es proteger cambios
   manuales de estado, activo, características y tipos que alteran disponibilidad.
   No se deniega SELECT ni se aplican restricciones a huéspedes, reservas, pagos,
   servicios o usuarios. Tampoco se conceden permisos directos a esas tablas:
   el funcionamiento utiliza procedimientos. Public/grupos/otros privilegios
   deben revisarse. Una futura concesión amplia puede cambiar el resultado.
4. `03_VerificarPermisos.sql`: solo lectura como la identidad REAL de la app.
   Las escrituras directas indicadas deben dar 0 y los 60 EXECUTE, 1.
   Un NULL o un resultado distinto requiere diagnóstico. No ejecuta funciones
   de negocio ni demuestra por sí mismo que sus cuerpos funcionen.
5. `04_RevertirPermisos.sql`: reversión MANUAL del rol nuevo de 02, con el mismo
   usuario destino. Retira su membresía y elimina el rol junto con sus GRANT/DENY,
   en una transacción; no borra usuarios, tablas, procedimientos ni datos. Se
   detiene si detecta otros miembros, roles heredados, objetos propios o permisos
   diferentes de los previstos. No debe utilizarse sobre un rol ajeno o una
   instalación que haya sido modificada: revisar el registro previo con el DBA.

Guardar los resultados de 00 ANTES de instalar, registrar nombre del usuario
operativo y confirmar que el rol no existe. Probar 01 y 02 en una copia de pruebas;
verificar 03, el recorrido funcional y la reversión 04 allí antes de usarlo en una
base compartida. No ejecutar dentro de transacciones externas: 01, 02 y 04 lo
rechazan para evitar afectar una transacción del operador. Si 01 o 02 fallan,
su instalación se revierte. Si las pruebas fallan después del COMMIT de 02,
se utiliza 04; esto no revierte datos creados en pruebas funcionales.

La reversión restaura la ausencia original del rol técnico y su membresía, no
concede accesos alternativos: un usuario que antes no tenía acceso volverá a no
tenerlo. 01 es independiente: 04 conserva sus cinco procedimientos. Retirarlos
no es necesario para revertir permisos; cualquier eliminación debe revisarse por
separado según las dependencias. No ejecutar DROP PROCEDURE indiscriminadamente.

Los originales `Procedimientos_Almacenados.sql` y los demás SQL se conservan
intactos. No ejecutar indiscriminadamente todos los scripts ni reinstalar tablas.
La lista de permisos corresponde a las llamadas actuales de Datos: revisar y
actualizar al agregar procedimientos o tablas. No se puede asegurar un resultado
idéntico en una instancia cuyo esquema, triggers o permisos no se inspeccionaron.
Guardar copia del diagnóstico de permisos antes de aplicar y planificar con el
DBA la reversión de esos cambios; no eliminar usuarios o roles automáticamente.

## Disponibilidad y operaciones legítimas

No se alteraron `ReservaNegocios`, `ReservaDatos`, `ReservaTemporalDatos`,
`PagoDatos`, los módulos de huéspedes ni los procedimientos de reservas/pagos.
La consulta existente de disponibilidad considera habitaciones/tipos activos,
mantenimiento y solapamiento con reservas CONFIRMADA/CHECKIN. Check-in y check-out
actualizan Reservas, no Habitaciones.estado. Las concesiones explícitas incluyen
consultas y las rutas de huéspedes, reservas, pagos, confirmación, check-in y
check-out para conservar esa cadena de trabajo.

La preservación del código y de permisos previstos no sustituye una prueba real
contra SQL. No se afirmó haber probado transacciones o disponibilidad real.

## Validación automática realizada

- Restore/build con SDK 10.0.404 y EnableWindowsTargeting: cero errores y las
  mismas 122 advertencias de nulabilidad preexistentes.
- 56 casos pasaron: 45 previos sin SQL y 11 nuevos. Cero fallos.
- Los nuevos casos verifican las siete escrituras rechazadas antes de Datos para
  RECEPCIONISTA, GERENTE, roles desconocidos/vacíos/nulos; administrador con rol
  normalizado llega a Datos con los parámetros esperados; se conservan validaciones
  de datos; listado, búsqueda y disponibilidad devuelven los resultados de Datos
  sin requerir permiso administrativo y mantienen las fechas normalizadas.
- Los cinco scripts y las cinco definiciones dinámicas de procedimientos pasan
  el análisis sintáctico de Microsoft ScriptDom (TSql160Parser). El verificador
  se instaló fuera del repositorio; no se agregaron paquetes al proyecto. Esto
  valida sintaxis, no ejecución ni permisos efectivos en una instancia.
- Se contrastaron automáticamente los parámetros de los cinco procedimientos
  con AddWithValue en Datos y las columnas requeridas por los listados. Las
  reactivaciones solo contienen UPDATE de activo = 1 sobre su tabla correspondiente.
- Las listas de instalación y reversión coinciden con las 60 llamadas de Datos. Los SQL
  originales y todos los Designer se verificaron idénticos byte por byte.
- Se usa el filtro documentado en README: no se ejecutan las tres pruebas existentes
  con acceso SQL, incluida la que cambia una contraseña de usuario.

## Comprobaciones pendientes en Windows y SQL Server

En una **base de pruebas separada**, con datos preparados privadamente:

1. Ejecutar diagnóstico, revisar procedimientos existentes, aplicar los scripts
   aprobados y verificar permisos como la conexión real. Ningún sysadmin/db_owner
   debe usarse para demostrar la restricción.
2. Entrar como ADMINISTRADOR: crear/modificar una habitación de prueba, desactivarla,
   verla inactiva y reactivarla. Confirmar que la reactivación conserva su estado.
3. Entrar como RECEPCIONISTA y GERENTE: controles administrativos ocultos y sin
   acceso de guardado; consultas de habitaciones/disponibilidad funcionan.
4. Como RECEPCIONISTA, registrar huésped, consultar disponibilidad, reservar,
   registrar los pagos requeridos y realizar check-in/check-out. Comprobar las
   reglas de saldos y solapamientos existentes antes y después; usar los formularios
   con datos de prueba y no la prueba automática que cambia contraseñas reales.
5. Con la conexión operativa, comprobar en base de pruebas que DML directo se
   rechaza. No probar comandos de escritura en producción. La llamada directa a
   un procedimiento administrativo compartido seguirá permitida: es la limitación
   conocida, no una prueba aprobada de separación de empleados en SQL.

Windows Forms no se puede ejecutar en Linux. Permanecen pendientes la prueba
visual, autenticación Windows real, permisos efectivos, instalación y ejecución
de los procedimientos y el recorrido completo de reservas/estadías/pagos.

## Actualización: esquema de servicios confirmado

El usuario confirmó que categoria no existe y que Insertar/Modificar no reciben
ese parámetro. Antes de 01 de esta carpeta, seguir
[migración de categoría](migracion-categoria-servicios.md) y revisar los scripts
`compatibilidad-servicios`. Los cuerpos reales ya fueron recibidos y el ALTER de los tres procedimientos
está finalizado; la migración no inventa
categorías para los servicios históricos. No ejecutar aún los permisos SQL.

## Corrección justificada de la referencia versionada

Se agregó WHERE H.activo = 1 únicamente a sp_Habitacion_Listar en el archivo
Procedimientos_Almacenados.sql, para reflejar la definición instalada confirmada.
Esta es una excepción explícitamente autorizada a la preservación anterior del
archivo; no cambia reservas, pagos ni disponibilidad. El runner lee ese archivo
como referencia y nunca lo instala. No se modificó el procedimiento en SQL Server.
