> Informe histórico del incremento `5d747c9`. Para la versión que incorpora el trabajo posterior de Iara, consultar [integración final](integracion-final-iara-melina.md). Sus resultados anteriores no sustituyen la nueva validación.

# Entrega final de correcciones 5 y 6 — 16/11/2026

## Alcance y estado

Incremento sobre `6dbfb7997d459c0df937a88fbed78598805ea784`, exclusivamente en `correccion-servicios-habitaciones`. Al iniciar y antes de publicar, la rama remota y la referencia del PR #13 seguían en ese commit. El checkout de nube estaba limpio. No se modificaron `main`, `develop` ni `integracion-melina-iara`; no se hizo merge. El cambio diagnóstico de FormHuespedes de la computadora Windows no estaba en este checkout y no fue sobrescrito: antes de actualizar esa computadora, conservar `git diff` y los cambios locales.

El usuario informó 120 .NET, 47 estáticas, 35 guardas, 28/28 SQL dos veces y comprobación visual de historial antes de este incremento. Esa evidencia se conserva como antecedente, sin atribuirla al código nuevo. Las notas anteriores del repositorio que dicen «SQL pendiente» corresponden a otra etapa.

## Problemas y soluciones por capa

| Capa | Cambio y propósito |
| --- | --- |
| Entidades | `EstadoResultadoOperacion` distingue rechazo definitivo de resultado incierto. |
| Datos | `ErroresOperacionSql` centraliza códigos de rechazo sin escritura y mensajes seguros; se conservan los parámetros y resultados de los procedimientos de pagos/consumos. |
| Negocios | Rol operativo explícito en Pagos, DetalleServicio, Reservas, Temporales, FlujoReserva y Huéspedes; ausencia de rol o GERENTE rechaza esas operaciones antes de SQL. Reportes exige ADMINISTRADOR/GERENTE. Gestión de habitaciones/tipos y servicios conserva la restricción administrativa. El diario usa la misma clasificación que Presentación. |
| Presentación | La recuperación de consumo conserva la solicitud original: «Reintentar consumo» ya no reconstruye desde controles diferentes o sin habitación. Pagos diferencia rechazos definitivos de errores inciertos; al cambiar el ID limpia tablas/resumen y la confirmación muestra reserva, concepto y método. Propagación del rol desde menú a formularios y Negocios. |

No se alteraron firmas C#/SQL de escrituras existentes ni la identidad durable GUID. Los constructores de Negocios ahora necesitan un rol operativo para ejecutar sus operaciones protegidas; las llamadas internas se actualizaron y las llamadas sin rol fallan de forma cerrada. El importe de un cobro de saldo continúa calculándose bajo el bloqueo de SQL, no a partir del resumen visual.

Se conservaron los procedimientos ya instalados 01–06 sin cambios: validación de habitación perteneciente a la reserva, CHECKIN sin fecha de cierre, servicio activo no incluido, precio coincidente con el catálogo SQL, cantidades/importes, recuperación por GUID y saldos. SQL valida el precio solicitado contra el autorizado y conserva el precio histórico. No se aflojaron esas condiciones ni se recalcularon históricos.

### Formularios y errores

Huéspedes e historial, Habitaciones, Reservas, Servicios, Consumos, Pagos y Reportes utilizan un formateador común: encabezados españoles, fechas legibles, decimales con formato argentino, anchos y scroll. No cambia DataPropertyName, nombres SQL ni visibilidad de columnas que el formulario ya oculta. No hay un formulario independiente de tipos en este checkout; los selectores existentes de tipos y sus reglas se conservan. Consumos tiene una superficie desplazable para acceder a controles en ventanas más pequeñas. No se editaron archivos Designer, dependencias, lockfiles ni configuración privada.

Los errores técnicos de consulta de los formularios revisados se sustituyeron por mensajes seguros. El historial muestra una referencia local, sin ex.Message. El registro en `%LOCALAPPDATA%/HotelTramonto/Diagnosticos` guarda únicamente fecha UTC, referencia y código SQL o CLIENTE: no guarda mensajes, stack, nombres de huéspedes, rutas ni cadenas de conexión. La causa del fallo inicial de historial sigue sin demostrarse. Los rechazos de deuda/estado de check-in y check-out mantienen mensajes funcionales explícitos.

La distribución visual, el escalado y los eventos de Windows Forms no se ejecutaron aquí: los cambios de presentación están compilados, no aceptados visualmente.

## Nuevo protocolo SQL, preparado sin ejecutar

Archivos nuevos en `base de datos HotelTramonto/servicios-habitaciones`:

1. `07_CicloReserva.sql`: actualiza únicamente `sp_Reserva_CheckIn` y `sp_Reserva_Cancelar`.
2. `08_VerificarCicloReserva.sql`: compara las definiciones exactas esperadas; no ejecuta operaciones de negocio.

Ambas transiciones adquieren `HotelTramonto:reserva:<ID>` exclusivo y transaccional, igual que pagos/consumos/check-out, antes de leer Reservas con UPDLOCK/HOLDLOCK. Check-in exige CONFIRMADA, sin marca de cierre y alojamiento aprobado suficiente. Compara total y pagado directamente para no perder centavos por reducción de escala de una resta decimal(38,2); el caso 36 incluye deuda de 0,01. La consulta de aceptación conserva dos decimales también en sus diferencias. Cancelación conserva su regla: solo CONFIRMADA, sin reembolso automático. Las dos actualizan con predicado de estado y cierre y comprueban una única fila. Ante error hacen rollback. Check-in repetido continúa rechazándose como en el contrato anterior; check-out repetido conserva la idempotencia de 04.

07 exige la instancia autorizada, base de pruebas, ausencia de transacción externa, protocolo previo de cierre/pagos, definiciones anteriores conocidas y módulos sin firmas. Valida ambos procedimientos antes de aplicar DDL; instalación atómica, sin DML sobre datos históricos. Si existen cambios locales desconocidos, permisos insuficientes o módulos firmados, se detiene para revisión DBA; no reemplazar la definición esperada para evitar el rechazo. 08 admite variaciones de cabecera CREATE/ALTER y saltos CRLF, conservando comparación binaria del cuerpo. Ninguno de estos scripts se ejecutó en esta intervención.

Se agregaron ocho casos SQL: check-in/cancelación, check-in/cierre, doble check-in, check-in demorado tras cierre, cancelación demorada tras check-in, consumo/check-in, cierre sobre cancelada y marca de cierre incompatible. Cada contendiente utiliza conexión física distinta. Los dos casos demorados acreditan espera real con `sys.dm_exec_requests`, no solo tareas simultáneas. Conservan las barreras GUID/marca y la comprobación de once tablas históricas del ejecutor existente.

El protocolo protege a los escritores que lo respetan. No acredita ausencia absoluta de deadlocks ni protege frente a DML arbitrario de un sysadmin, triggers o escritores externos no auditados. No cambia disponibilidad, pagos iniciales 50/100 ni las reglas de confirmación existentes.

## Privilegios mínimos preparados, sin instalar

Carpeta nueva `base de datos HotelTramonto/acceso-operativo`:

1. `00_Diagnostico.sql`: token efectivo, membresías y permisos; solo lectura.
2. `01_Instalar.sql`: propuesta para un usuario Windows dedicado YA EXISTENTE, con placeholder que el DBA debe seleccionar; crea el rol técnico solo si está ausente. No crea login ni usuario, no quita privilegios anteriores y rechaza identidades elevadas o con permisos/membresías incompatibles. Concede EXECUTE sobre la lista explícita de rutas de Datos, incluidos historial y consulta de confirmación; sin EXECUTE de esquema ni CONTROL. Niega acceso directo SELECT/INSERT/UPDATE/DELETE sobre las once tablas funcionales, conservando cadena de propiedad dbo.
3. `02_Verificar.sql`: solo lectura, bajo la identidad Windows operativa REAL. Rechaza sysadmin/db_owner o identidad indeterminada; comprueba rutas EXECUTE, acceso directo y permisos de columna. No ejecuta los procedimientos ni prueba por sí solo sus cuerpos.

Estos scripts no se ejecutaron. No usar `melina` con sysadmin/db_owner para declarar separación efectiva. Inspeccionar grupos Windows, permisos heredados/public, propietarios, suplantación y privilegios de servidor con DBA; las guardas no sustituyen esa auditoría ni un ensayo funcional del token real. No ejecutar automáticamente los antiguos scripts de permisos que permanecen bloqueados por diseño.

**Límite de arquitectura:** los roles que la UI pasa a Negocios son una barrera funcional del cliente; no son identidad criptográfica ni impiden a un cliente modificado suministrar otro rol. Una misma identidad Windows técnica no permite a SQL distinguir ADMINISTRADOR, RECEPCIONISTA y GERENTE. El rol técnico necesita las rutas de todos los perfiles, incluida administración. Para separación fuerte se requiere decidir entre identidades Windows individuales con grupos y permisos SQL por perfil, o un servicio/API que autentique y autorice cada sesión y sea el único escritor SQL. No se cambió esa arquitectura ni se inventaron credenciales.

## Resultados ejecutados en nube

| Verificación | Aprobadas | Fallidas | Omitidas/excluidas |
| --- | ---: | ---: | --- |
| Compilación de toda la solución, .NET 10 y EnableWindowsTargeting | Correcta, 0 errores | 0 | 121 advertencias de nulabilidad/delegados |
| Suite .NET segura, incluido incremento de 16 casos | 136 | 0 | 0 omitidas dentro del filtro; 3 pruebas con SQL local excluidas expresamente |
| Comprobaciones Python existentes y nuevas | 53 | 0 | 0 |
| Guardas del ejecutor con --self-test | 39 | 0 | No abre SQL |
| Integración SQL con incremento | 0 | 0 | 36 no ejecutadas: 28 anteriores + 8 nuevas |
| Windows Forms y token SQL operativo | Sin ejecución | Sin resultado | Pendientes en Windows |

La reconstrucción final con dos procesos encontró MSB4166 (terminación prematura de un nodo); el directorio diagnóstico informado no estaba disponible. Se repitió la reconstrucción completa con `-m:1 -nodeReuse:false`: 0 errores, 121 advertencias; después pasaron nuevamente las 136 pruebas y las 39 guardas. No se estableció la causa de la terminación del nodo.

Los primeros intentos de validación detectaron dos expectativas nuevas de tipo de excepción incompatibles con el contrato existente y manifiestos de archivos de la versión anterior. Se ajustaron las expectativas, el control de cantidad del ejecutor opcional y los manifiestos a los cambios revisados; no se quitaron filtros de seguridad, controles de integridad ni aserciones de negocio. Los manifiestos también incluyen los archivos nuevos. Se preservó BOM de PowerShell 5.1. PowerShell no está disponible aquí: los scripts Windows no se ejecutaron.

Comandos reproducibles desde la raíz:

```powershell
dotnet build HotelTramonto.slnx
dotnet test HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj --no-build --filter "FullyQualifiedName!~ConexionBD_ConectaConHotelTramontoDB&FullyQualifiedName!~UsuarioNegocios_CambiarContrasena_GuardaHashValido&FullyQualifiedName!~UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull"
python -m unittest discover -s tools -p "verificar_*.py"
dotnet run --project tests/HotelTramonto.VerificacionSql -- --self-test
```

Linux requiere `-p:EnableWindowsTargeting=true` en build y activar el SDK preparado. Los resultados TRX y logs de esta ejecución quedaron fuera del checkout en `/workspace/.onboarding`.

## Instalación y aceptación local mínima

No se instaló nada en `HotelTramontoDB_Pruebas`. No se abrió ni modificó `HotelTramontoDB`. No se ejecutaron reversiones. La reserva histórica 1 se conserva inconsistente (FINALIZADA, alojamiento 165.000, inicial REEMBOLSADO 100.000, alojamiento aprobado 0, consumos 19.000, servicios aprobados 0). No usarla para pruebas ni regularizarla.

Antes de cualquier instalación local: conservar cambios de Windows, cerrar clientes antiguos, confirmar instancia/base, registrar conexiones/transacciones y guardar definiciones/firmas/permisos actuales. El responsable DBA debe crear un FULL COPY_ONLY en archivo NUEVO de la base de pruebas, ejecutar RESTORE VERIFYONLY y comprobar recuperación en una copia desechable; VERIFYONLY solo no acredita una restauración completa. No sobrescribir el respaldo anterior ni tocar la base original. Guardar la salida de definiciones previas de 07 en forma privada.

Para la base de pruebas YA migrada, el único incremento de ciclo es **07 → 08**. No ejecutar otra vez 01, 04, Servicios.categoria ni el monolítico. Probar primero el incremento en una copia temporal controlada. La propuesta de permisos es un recorrido separado **00 → revisión DBA/elección de identidad → 01 → 02 con identidad real**; no se necesita para compilar y no está instalada.

Para automatizar las 36 pruebas en Windows sin escribir en bases permanentes:

```powershell
dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup-ciclo "C:\Respaldos\RESPALDO_PREVIO_A_5Y6.bak"
```

Usar el respaldo FULL aprobado posterior a categoria y anterior a 5/6 que requiere el ejecutor existente; no sustituirlo silenciosamente por uno posterior. El runner valida Express local y respaldo, restaura una copia `HotelTramontoDB_HT56_<GUID>`, pide `INSTALAR` para ESA copia, aplica 01/02/04/05/07/08 exclusivamente allí, ejecuta 36 casos, verifica once tablas y elimina solo su copia acreditada sin expulsar sesiones ajenas. Las barreras y archivos originales permanecen. Conservar informe privado y confirmar eliminación. Si ese respaldo ya no existe, el runner no debe forzarse; preparar otro recorrido revisado para un respaldo postmigración queda como requisito local.

Tras la aceptación automatizada, el mínimo visual es:

- ADMINISTRADOR y RECEPCIONISTA, en copia temporal controlada: consumo con habitación vacía rechazado; consumo correcto asociado a su habitación y filtro; servicio inactivo/precio desactualizado rechazado. No escribir pruebas nuevas en las bases permanentes.
- En esa copia: alojamiento antes de check-in, deuda de servicios bloquea check-out, pago de saldo correcto, cierre y repetición sin efectos; nuevo consumo tras cierre rechazado. FormPagos cobra saldos completos: pagos parciales/pendientes/aprobación se cubren con integración SQL, no con controles inexistentes.
- Terminar únicamente el proceso de la aplicación mientras muestra un resultado exitoso aún no aceptado. Reabrir el mismo contexto, con mismo usuario Windows y configuración; recuperar consumo/pago y verificar mismo GUID, mismo ID de pago y una sola fila. No borrar el diario. Esto prueba confirmación local pendiente; una pérdida real de respuesta de red requiere ensayo controlado separado y conciliación.
- GERENTE: reportes y consulta de habitaciones según política; sin acceso a huéspedes/reservas/pagos/consumos ni administración. RECEPCIONISTA sin administración de habitaciones/tipos/servicios. Validar token Windows real sin privilegios, no solo rol en el login de la aplicación.
- Historial de huésped sin reservas y con RECEPCIONISTA; encabezados, fechas, anchos, scroll y escalado de los formularios afectados. El fallo inicial de historial no puede declararse resuelto por la desaparición del síntoma.

Ante fallo de instalación 07: rollback transaccional de ambas definiciones. Ante incidencia posterior: detener clientes/escritores, conservar diario e informes y conciliar; no bajar automáticamente a escritores antiguos ni usar 03 como reversión. Una restauración o cambio de definiciones posterior requiere revisión DBA del respaldo y del estado de datos. Nunca borrar operaciones pendientes para «desbloquear» un cobro.

## Publicación y decisión sobre PR #13

El commit de esta entrega y su SHA se informan al publicar en la rama autorizada. Un push a esa rama actualiza el contenido del PR existente; no autoriza merge, conversión a listo ni despliegue. La API GitHub devolvió Forbidden durante esta sesión: no se pudo verificar ni editar la descripción/metadatos del PR por API. El usuario lo informa como borrador y no se emitió ninguna acción para cambiarlo. Se verifican por Git la rama remota y la referencia del PR después del push.

**Recomendación: mantener en borrador, sin fusionar todavía.** Bloqueos concretos de aceptación: los ocho casos SQL nuevos y regresión combinada aún no ejecutados, revisión/ensayo de privilegios efectivos y aceptación visual/recuperación en Windows. Las limitaciones de autenticación compartida requieren decisión de arquitectura si se exige aislamiento fuerte entre empleados. Compilación y pruebas de nube aprobadas no equivalen a despliegue validado.
