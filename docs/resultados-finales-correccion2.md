# Pruebas finales de la corrección 2

Rama de trabajo exclusiva: correccion-permisos-habitaciones. No se modifica la rama
correccion-servicios-habitaciones ni se realizan merges. Se toma como evidencia
**reportada por la usuaria** la ejecución exitosa de la migración, compilación,
lecturas y conservación de datos en HotelTramontoDB_Pruebas. No fue una ejecución
SQL realizada por el agente. No se necesita repetir la migración.

## Resultado local del agente

| Comprobación | Resultado y alcance |
| --- | --- |
| Restore/build .NET 10.0.404 en Linux, Windows targeting | Cero errores; 122 advertencias de nulabilidad preexistentes. |
| Pruebas .NET seguras | 68 casos aprobados, cero fallos: 56 existentes y 12 nuevos. |
| Pruebas estáticas Python | 20 aprobadas: seguridad scripts (5), runner (8), formularios (4), verificador final (3). |
| PowerShell hospedado, sin SQL | Constructor (4 combinaciones), sesiones (15 casos), referencias de lecturas y proceso simulado (6 casos) aprobados. |
| Verificador final nuevo | Parser oficial PowerShell: cero errores. T-SQL de metadatos: sintaxis válida con ScriptDom. |
| Windows Forms visual | No ejecutado: el entorno es Linux. Auditoría de fuente y compilación sí realizadas. |
| SQL real por el agente | No ejecutado. No se instalaron restricciones ni se cambiaron datos/permisos. |

Las tres pruebas existentes con SQL real se excluyen por nombre mediante lista
positiva: ConexionBD_ConectaConHotelTramontoDB,
UsuarioNegocios_CambiarContrasena_GuardaHashValido y
UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull. La segunda altera una
contraseña real; no se ejecutó. La configuración por defecto de las pruebas se
bloquea con una ruta inexistente. No se inventan credenciales para hacerlas pasar.

## Qué demuestran las pruebas de roles

- Invocación directa de las siete escrituras de habitaciones/tipos en Negocios
  con RECEPCIONISTA, GERENTE, variaciones normalizadas, rol desconocido, vacío y
  nulo: se rechaza antes de validar datos o llegar a Datos. No dependen de botones.
- ADMINISTRADOR conserva las operaciones y los parámetros enviados, admite
  normalización y conserva los errores de Datos en vez de simular un éxito.
  Los dobles implementan los contratos reales de Datos, sin SQL.
- Consultas, búsqueda y disponibilidad siguen llegando a Datos y devolviendo sus
  resultados aun después de rechazar una operación administrativa. Las fechas
  normalizadas y validaciones previas se conservan.
- GERENTE no recibe permisos administrativos por defecto.

Esto prueba el código de Negocios con dobles; no afirma que se hayan ejecutado
escrituras SQL reales por ADMINISTRADOR ni reservas/pagos/estadías de punta a punta.
Esos módulos y sus procedimientos permanecen intactos en este ajuste.

## Auditoría de formularios y corrección encontrada

Se revisaron siete handlers administrativos de los cuatro formularios de habitaciones.
Conservan la comprobación de permisos antes de abrir formularios o llamar a Negocios,
transmiten rolUsuario y usan PermisosHabitaciones. Las grillas son de solo lectura.
No se encontró acceso directo de Presentación a HabitacionDatos/TipoHabitacionDatos.
Los constructores sin rol no conceden autorización. No se modificaron Designer,
colores, tamaños ni disposición de controles.

Se detectó una regresión funcional: FormHabitaciones lista solo activas, pero el
botón Reactivar esperaba una fila inactiva y quedaba deshabilitado. Ahora ADMINISTRADOR
puede abrir FormReactivarHabitacion, ya existente, sin seleccionar una fila activa;
se recarga la lista principal al cerrar. Los demás roles conservan controles
ocultos/deshabilitados y el handler sigue exigiendo permiso. Una prueba estática
cubre esta ruta. La prueba visual final de apertura/cierre sigue pendiente.

Un cliente alterado puede construir un formulario con un texto ADMINISTRADOR o
llamar directamente a Datos. Los guards de UI/Negocios no se presentan como una
barrera contra ese atacante. Dentro del flujo normal, el rol procede del login
existente, que verifica usuario activo y contraseña; no se cambió la autenticación.

## Comprobación restante en Windows: un comando, sin migrar

Actualizar la rama completa. Desde su raíz, ejecutar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File .\tools\Verificar-Final-Correccion2.ps1 -ConfiarCertificadoLocal
```

El interruptor de certificado tiene el mismo alcance local documentado para el
runner anterior: mantiene cifrado, pero omite la validación del certificado local.
Puede omitirse si se dispone de certificado confiable. Este verificador NO ejecuta
el cuerpo del runner de migración. Importa solo sus cuatro helpers revisados para
procesos, construcción de cadena y normalización de definiciones.

- Verifica las huellas del manifiesto final, fuentes, pruebas y consultas.
- Compila y ejecuta la lista positiva de 68 casos con conexión por defecto bloqueada.
- Se conecta únicamente a LAPTOP-5TSJJUP8\SQLEXPRESS / HotelTramontoDB_Pruebas,
  vuelve a comprobar contexto y ejecuta pruebas-finales/00_VerificarCorreccion2.sql.
- Ese archivo contiene SELECT de metadatos y variables locales de contratos; no
  invoca procedimientos de escritura NI de lectura de la aplicación. No migra,
  crea tablas, clasifica datos, reactiva, escribe historias o concede permisos.
- Comprueba existencia/cuerpo de 19 procedimientos relevantes, parámetros y sus
  tipos/longitudes, ausencia de parámetros inesperados y columnas de seis lecturas
  mediante descripción de resultados sin ejecutar esos procedimientos.
- Informa sysadmin/db_owner y permisos efectivos de forma informativa; no intenta
  cambiarlos ni considera que una cuenta privilegiada demuestre denegación SQL.

Enviar únicamente `informe-final.json`, ubicado en la carpeta que muestra el comando
bajo %LOCALAPPDATA%\HotelTramonto\InformesFinales. Si falla una compilación, adjuntar
el log que indique el informe. contratos-sql.xml conserva definiciones/identidad
localmente y no hace falta enviarlo si las comprobaciones pasan. No publicar informes
privados en Git. No hace falta cerrar todas las sesiones para esta fase: no realiza
escrituras de base y no requiere repetir la instalación/conservación ya comprobada.
Con -SinSql se ejecuta solo la parte .NET, dejando SQL expresamente PENDIENTE.

## Garantías SQL y límite de arquitectura

La conexión Windows compartida representa una única identidad frente a SQL Server.
No distingue el correo/rol de cada empleado. Roles/idUsuario/SESSION_CONTEXT enviados
por un cliente modificable no autentican al empleado ante SQL. Las validaciones
C# garantizan rechazo en el flujo normal, no frente a llamadas SQL externas.

02_PermisosMinimos.sql sigue bloqueado y pendiente: no hay restricciones nuevas
instaladas por esta tarea. Un sysadmin/db_owner puede escribir o alterar objetos;
no corresponde afirmar que SQL impide a esa identidad modificar habitaciones.
Incluso con un principal técnico de mínimos permisos, otorgar ejecución de los
procedimientos administrativos compartidos no separa administrador y recepcionista.
Un servicio servidor de confianza que autentique al empleado y autorice cada acción,
o identidades SQL realmente diferenciadas, permitiría esa separación. No se
implementa ese cambio de arquitectura en esta corrección académica.

## Decisión de integración

La parte automatizable local del alcance académico pasó. La compatibilidad instalada
ya tiene evidencia positiva reportada de la etapa anterior, pero el control ampliado
de 19 contratos todavía debe ejecutarse en la copia. Recomiendo recibir ese informe
y hacer únicamente una comprobación visual breve antes de integrar definitivamente:

Confirmar primero que la configuración privada de Windows Forms apunta a
HotelTramontoDB_Pruebas; el verificador no modifica esa configuración.

1. ADMINISTRADOR abre Habitaciones -> Reactivar y ve el formulario de inactivas,
   incluso sin fila seleccionada; cerrar sin reactivar nada.
2. RECEPCIONISTA ve consultas y no controles administrativos. GERENTE conserva
   la misma restricción administrativa (ya cubierto en código/pruebas).

No repetir la migración ni alterar datos históricos para estas dos comprobaciones.
Las escrituras reales de administración y los recorridos completos de reservas,
pagos y estadías no fueron ejercitados por el agente: si la aceptación exige
end-to-end, realizarlos solo con datos aislados en una copia controlada, por separado.
La corrección no debe etiquetarse como autorización segura por empleado en SQL.
