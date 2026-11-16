# Compatibilidad de Servicios.categoria

Rama: correccion-permisos-habitaciones. No se ejecutó SQL Server, ni se cambiaron
formularios, entidades, Negocios o Datos en este ajuste. Los SQL originales permanecen intactos.

## Evidencia y contrato

El diagnóstico real confirmó que no existe dbo.Servicios.categoria. Las integrantes
aportaron los cuerpos reales completos de Insertar, Modificar y Listar: coinciden
en lógica con los originales versionados, sin validaciones adicionales. Los ALTER
finalizados preservan esa lógica y agregan únicamente el contrato de categoría y
su validación. El diagnóstico 00 sigue siendo necesario para registrar el estado
previo de la copia, tipos/constraints, opciones de módulo y permisos.

ServicioDatos envía @categoria como string mediante AddWithValue en Insertar y
Modificar. ServicioNegocios exige exactamente Almuerzo, Merienda, Cena, Tragos,
Pesca guiada, Incluido u Otros. El formulario exige selección y usa esa columna
para cargar/filtrar servicios. ListarParaConsumo usa `categoria <> 'Incluido'`:
con NULL el servicio también queda fuera del filtro. FormRegistrarConsumos usa
las categorías retornadas para agrupar productos. Por eso no es seguro asignar
Otros a todos ni habilitar el nuevo listado con categorías pendientes.

La entidad Servicio no tiene Categoria. En estos recorridos los formularios usan
DataTable, no esa entidad: no es necesario modificarla para reparar el contrato.
Se propone VARCHAR(30): suficiente para los siete valores actuales, sin caracteres
que requieran Unicode. C# no impone una longitud libre; limita a esos siete textos.

## Archivos preparados y orden

En `base de datos HotelTramonto/compatibilidad-servicios`:

1. **00_DiagnosticoYRespaldo.sql**: SOLO LECTURA. Ejecutar primero. Obtener columnas,
   tipos, nullability, defaults, checks, índices, triggers, dependencias y cuerpos
   completos de Insertar, Modificar y Listar con sus parámetros, opciones de módulo,
   permisos y presencia de firmas. Guardar resultados fuera de Git. En SSMS guardar
   además cada procedimiento como `Script as > ALTER To > File`, con sus SET y
   contexto. Guardar el respaldo desde la COPIA; no ejecutar respaldos que apunten
   a HotelTramontoDB. Para esta prueba usar 05, que contiene el guard de base.
   Si una definición es NULL o está cifrada/no visible, resolver la visibilidad antes
   de continuar. Las definiciones ya fueron recibidas y revisadas; si cambiaron desde entonces,
   detenerse y compararlas antes de instalar.
2. **01_AgregarCategoria.sql**: ejecutar en una copia de pruebas solo después de
   comprobar el esquema. Agrega categoria VARCHAR(30) NULL sin DEFAULT únicamente
   si falta. No clasifica registros, no borra ni recrea tablas. Al repetirlo acepta
   ese mismo tipo; se detiene ante otro contrato en vez de alterarlo automáticamente.
   La columna nullable permite conservar los servicios históricos sin inventar datos.
   Registrar cantidad/IDs de servicios y guardar un respaldo de la base antes de migrar.
3. **02_ClasificarServicios.sql**: utiliza exclusivamente la decisión confirmada ID 1 = Otros. No agregar
   filas adicionales; el script rechaza otro mapa de clasificaciones.
   La plantilla contiene la decisión real confirmada: ID 1 = Otros. No es un
   default para otros servicios ni incluye credenciales. No clasificar por
   precio o nombre automáticamente; Incluido afecta los consumos adicionales.
   Rechaza IDs inexistentes, categorías fuera de la lista y sobrescrituras distintas.
   Solo rellena NULL de los IDs seleccionados. Admite repetir una
   clasificación idéntica; requiere revisión manual para corregir una decisión previa.
   Cada lote es transaccional. Guardar privadamente la lista aplicada y pendientes.
4. **03_ActualizarProcedimientos.sql**: finalizado según los cuerpos reales
   confirmados. Ya no requiere desbloquear un borrador. Mantener la aplicación
   cerrada. Exige todos los servicios clasificados y categoria VARCHAR(30) NULL.
   Si hay pendientes o categorías inválidas, falla sin aplicar ningún ALTER.
   Instala de forma transaccional Insertar/Modificar con @categoria VARCHAR(30)
   obligatorio y Listar con categoria, conservando las columnas anteriores,
   filtro activo = 1, orden por nombre y las demás asignaciones originales.
   No cambia el precio a cero para Incluido: eso alteraría información existente.
   ALTER conserva permisos del objeto; el script se detiene si detecta firmas
   que requieren atención del DBA. No modifica otros procedimientos.
5. **04_Comprobaciones.sql**: SOLO LECTURA. Revisar que no haya pendientes/valores
   inválidos, parámetros y metadatos de resultados. No ejecuta procedimientos de
   escritura. Los servicios inactivos también deben estar clasificados.
   La comprobación adicional de consumos históricos Incluido es una revisión,
   no una autorización para borrar cargos o devolver pagos automáticamente.
6. **05_RevertirProcedimientos.sql**: NO es parte de la instalación normal.
   Restaura transaccionalmente los tres cuerpos anteriores confirmados,
   conservando la columna y las clasificaciones. Revisar antes de usarlo.
7. Luego revisar/instalar **permisos-habitaciones/01_ProcedimientosFaltantes.sql**,
   que crea los cinco procedimientos si faltan. Sus parámetros y columnas siguen
   coincidiendo con C#. Servicios_ListarInactivos devuelve la columna categoria real;
   los tres Reactivar solo cambian activo de su tabla. No cambian contraseñas/roles,
   categoría, precio, estado de habitación, reservas, pagos o estadías.
8. Probar funciones en Windows y en una base de pruebas. **NO ejecutar todavía
   permisos-habitaciones/02_PermisosMinimos.sql**: la identidad actual es sysadmin y
   db_owner. Sus pruebas requieren una identidad operativa limitada, en otra etapa.

Coordinar una ventana sin escrituras durante la clasificación/activación: los
procedimientos antiguos todavía pueden crear servicios sin categoría. No permitir
uso normal con la migración a medias. Los clientes antiguos que no envían categoria
no son compatibles con el nuevo parámetro obligatorio. No hay fallback silencioso.

## Preservación de datos y procesos

No hay DELETE, TRUNCATE, DROP TABLE ni cambio de IDs, nombres, precios o activo.
La única actualización de filas en la migración es la clasificación explícita de
la nueva columna. Las definiciones de procedimientos no escriben datos al instalar.
La columna queda nullable para conservar historia y facilitar una reversión sin
pérdida; los procedimientos nuevos y Negocios exigen categoría válida para las
escrituras normales. No es una garantía frente a escrituras SQL externas.

No se alteran Reservas, Pagos ni DetalleServicios, sus procedimientos o sus datos.
Los consumos existentes conservan referencia, cantidad y precio_unitario registrado.
La clasificación afecta qué servicios se ofrecen para NUEVOS consumos (excluye
Incluido); el hotel debe decidirla, no el script. Debe comprobarse el recorrido
completo de consumo, saldo, pago y estadía en Windows/SQL antes de habilitarlo.

## Reversión segura

- Un fallo dentro de cada script transaccional revierte ese script, no etapas
  anteriores ya confirmadas. Se rechazan transacciones externas.
- Si falla la activación de 03, sus tres ALTER se revierten juntos. La columna y
  clasificaciones anteriores se conservan para diagnóstico/reintento.
- Si el fallo aparece después del COMMIT, detener escrituras y restaurar los TRES
  procedimientos mediante 05_RevertirProcedimientos.sql, basado en los cuerpos
  reales recibidos. Comparar con los ALTER de respaldo: si cambiaron después de
  esa revisión, usar los respaldos correspondientes bajo control del DBA.
  Reponer las opciones de módulo de los respaldos si difieren de la sesión actual;
  no se modifican automáticamente firmas, roles o permisos.
- Mantener categoria y sus valores: quitar la columna perdería clasificaciones.
  No se entrega un DROP COLUMN ni un backfill inverso general. Las demás columnas
  originales no se modificaron. Corregir una clasificación exige una decisión
  explícita revisada, sin recalcular precios de consumos históricos.
- Revertir los procedimientos devuelve el contrato anterior, que ya era incompatible
  con este C#: no reabrir la aplicación hasta resolver/reaplicar la migración. No se
  promete que restaurar ese contrato haga funcionar categoria en la aplicación.
- El script de reversión de permisos 04_RevertirPermisos.sql es independiente y no
  se necesita aquí, porque no se aplicaron esos permisos.

## Verificación realizada y pendientes

Se revisaron Datos, Negocios, entidad y formularios sin modificarlos. Se contrastaron
parámetros enviados por C#, categorías permitidas y columnas utilizadas; se validó
sintaxis con Microsoft ScriptDom fuera del repositorio, incluidos ALTER dinámicos.
Los siete valores caben en VARCHAR(30): longitud máxima 12 bytes (Pesca guiada).
Una comprobación .NET con la expresión real de Negocios confirmó seis categorías
adicionales visibles y exclusión de Incluido y NULL. No sustituye la prueba del
recorrido de consumo/pago. Los originales SQL y el código de reservas/pagos/estadías
permanecen intactos.
No se conectó a SQL Server ni se ejecutaron procedimientos. La validación sintáctica
no prueba resultados, conservación de las validaciones reales ni permisos efectivos.

Pendientes de ejecución: respaldo y comprobación de metadatos en la copia,
clasificación de IDs reales y pruebas SQL/Windows. No falta recibir otra definición
para preparar los cambios. Sí falta la decisión del hotel sobre la categoría de
cada servicio histórico y la comprobación operativa de opciones/permisos del entorno.
La prueba de permisos con identidad limitada sigue separada y pendiente.

## Pasos exactos en la copia de pruebas

1. Restaurar la copia con el nombre exacto **HotelTramontoDB_Pruebas**. Verificar
   la instancia y seleccionar esa base en el desplegable de SSMS ANTES de ejecutar.
   Los once scripts nuevos no contienen USE y cada lote verifica DB_NAME antes de
   actuar; una selección distinta provoca THROW. No cambiar el nombre esperado.
2. Cerrar la aplicación y detener escrituras. Ejecutar 00 y guardar resultados,
   los tres ALTER originales y el respaldo de la copia fuera del repositorio.
3. Ejecutar 01. En una ventana nueva, ejecutar:

   ```sql
   SELECT id_servicio, nombre, descripcion, precio, activo, categoria
   FROM dbo.Servicios
   ORDER BY id_servicio;
   ```

4. Preparar privadamente una tabla de decisiones ID/categoría, también para los
   inactivos. Completar el INSERT comentado en 02 con esos valores reales y
   ejecutarlo; no incluir esta copia con datos reales en Git. El mapa autorizado contiene solo ID 1; no se aceptan otros servicios.
   Los IDs no se modifican. La versión final rechaza otras filas además del ID 1. No inferir Incluido por precio ni usar Otros para
   salir del paso: el hotel debe decidir si será un consumo adicional.
5. Ejecutar esta consulta. Debe devolver CERO filas antes de 03:

   ```sql
   SELECT id_servicio, nombre, activo
   FROM dbo.Servicios
   WHERE categoria IS NULL;
   ```

   03 verifica además que todos los valores estén permitidos. Si la plantilla
   rechaza una clasificación ya registrada, revisar el error; no sobrescribir
   automáticamente una decisión anterior.
6. Ejecutar 03 y después 04. Verificar @categoria VARCHAR(30) en Insertar/Modificar,
   resultado Listar con categoria y ningún servicio pendiente/incorrecto. Comparar
   los IDs, nombres, precios, activo y consumos previos con el respaldo; permanecen
   iguales salvo la columna nueva expresamente clasificada.
7. Revisar e instalar permisos-habitaciones/01_ProcedimientosFaltantes.sql si
   continúan faltando los cinco procedimientos. Repetir las comprobaciones de 04.
8. Configurar la aplicación para la copia y probar altas/edición con las siete
   categorías, listado de inactivos, reactivación y registro/pago de un consumo.
   Incluido no debe aparecer en el selector de consumos adicionales; las otras
   seis categorías sí, si tienen servicios activos. Categorías inválidas o NULL
   deben rechazarse por las escrituras nuevas. No modificar precios históricos.
9. Si falla, cerrar la aplicación, registrar el error, revisar y ejecutar 05.
   Conservar la columna, clasificaciones y copia de pruebas para investigar.
   No reabrir el cliente con el contrato anterior: ya era incompatible con C#.

## Alcance de Incluido

El filtro existente de Negocios excluye Incluido del catálogo de NUEVOS consumos.
La migración no convierte servicios, no pone precios a cero, no inserta consumos
ni recalcula pagos. Al exigir clasificación completa se evita que un servicio
sin clasificar desaparezca silenciosamente del filtro o reciba Otros por defecto.
No se deben reclasificar consumos históricos a partir de la categoría actual.

Limitación existente: DetalleServicioNegocios y sp_DetalleServicio_Insertar no
verifican por sí mismos la categoría. Una llamada directa puede intentar registrar
un servicio Incluido. No se presenta el filtro de UI como garantía ante llamadas
externas ni se cambia ese procedimiento fuera de este alcance. También debe
revisarse en prueba el caso de una categoría cambiada mientras un formulario de
consumos permanecía abierto; cerrar/reabrir ventanas evita usar el catálogo viejo
al migrar, pero no equivale a autorización transaccional del consumo.

## Clasificación confirmada para la copia actual

El hotel confirmó un único servicio, ID 1 (Desayuno Buffet), con precio 9500.00 y
activo = 0. El script 02 contiene exclusivamente `(1, 'Otros')` y comprueba además nombre,
precio 9500.00 y activo = 0 antes de modificar. Si el escenario cambió, se detiene. No modifica
nombre, descripción, precio o activo, no reactiva el servicio y no toca consumos
ni pagos históricos. Al repetirlo, una categoría idéntica no cambia la fila;
una categoría distinta ya registrada provoca rechazo para revisión.

Regla confirmada: todos los servicios adicionales se cobran aparte del alojamiento.
Otros no recibe tratamiento gratuito. El catálogo de consumos lista solo servicios
activos: el ID 1 seguirá sin ofrecerse hasta una reactivación administrativa
explícita. Cuando se reactive, Otros pasa el filtro de consumo y se utiliza su
precio; los consumos previos conservan su precio_unitario.

La compatibilidad existente conserva Incluido entre los siete valores admitidos,
pero NO debe elegirse para servicios adicionales bajo la regla actual. El filtro
existente excluye esa categoría. La migración no elimina esa opción ni convierte
la regla en una nueva restricción global: impedir que alguien seleccione Incluido
requeriría un cambio coherente de validación/catálogos fuera de esta clasificación.

Para esta copia, después de 02 verificar ID 1: categoria = Otros, precio = 9500.00,
activo = 0 y ningún servicio pendiente. Si aparecen servicios nuevos desde el
último diagnóstico, clasificarlos explícitamente antes de 03. No ejecutar aún los
permisos SQL ni usar la base original para las pruebas.

## Seguridad de ejecución final

Todos los scripts de ambas carpetas están restringidos a HotelTramontoDB_Pruebas.
Cada lote separado por GO comienza con la comprobación del nombre; no se utiliza
USE para redirigir una ventana equivocada. Ejecutar el archivo completo, no una
selección que omita el guard. La comprobación de nombre no identifica el servidor:
confirmar también la instancia de pruebas. Los SQL originales no se modificaron y
siguen conteniendo su contexto original: NO ejecutarlos para este recorrido.

02_PermisosMinimos.sql de permisos-habitaciones tiene además un THROW incondicional
que impide instalar permisos en esta etapa. No desbloquearlo ni ejecutarlo ahora.
No confundirlo con 02_ClasificarServicios.sql, que sí pertenece a la migración.

Para comenzar en Windows seguir docs/pruebas-windows.md. La configuración privada
del cliente debe indicar BaseDatos = HotelTramontoDB_Pruebas. Los guards protegen
los scripts preparados, no una aplicación configurada por error contra otra base.
