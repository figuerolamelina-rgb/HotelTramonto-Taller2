# Pruebas SQL con SQL Server Express y .NET 10

Esta versión reemplaza el recorrido LocalDB/Python por un ejecutor C#/.NET.
No requiere instalar LocalDB, Python, pyodbc, ODBC ni sqlcmd.
Utiliza Microsoft.Data.SqlClient que el proyecto Datos ya referencia; no se
agregan paquetes NuGet ni herramientas de sistema nuevos.

## Comando desde Visual Studio

Abrir Ver -> Terminal (PowerShell) en la raíz del checkout de
correccion-servicios-habitaciones, y ejecutar:

    dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup "C:\Respaldos\HotelTramontoDB_Pruebas.bak"

Cambiar únicamente la ruta por la del respaldo real.
Alternativa equivalente con el wrapper PowerShell:

    powershell.exe -NoProfile -File .\tools\Probar-ServiciosHabitaciones.ps1 -BackupPath "C:\Respaldos\HotelTramontoDB_Pruebas.bak"

El respaldo todavía debe crearse. Debe ser FULL, de HotelTramontoDB_Pruebas,
posterior a Servicios.categoria y anterior a correcciones 5/6.
La automatización NO crea ese respaldo ni abre las bases originales.
En SSMS, el responsable puede crear un FULL COPY_ONLY nuevo de la base de pruebas
ya verificada, en un archivo nuevo y accesible para la cuenta del servicio SQL.
Revisar instancia y nombre antes de hacerlo; no reutilizar un archivo de producción.

Sin respaldo, puede comprobarse compilación y seguridad sin tocar SQL:

    dotnet run --project tests/HotelTramonto.VerificacionSql -- --self-test

## Requisitos existentes y configuración

- Windows en LAPTOP-5TSJJUP8, SQL Server Express SQLEXPRESS 2017 o posterior.
  Se valida motor Express, nombre de equipo e instancia.
- .NET 10; Visual Studio es opcional para el ejecutor.
- Un respaldo compatible con esa versión de Express y sus límites de capacidad,
  sin TDE, FILESTREAM ni características no soportadas.
- Autenticación Windows con permisos para RESTORE/crear y eliminar la copia,
  consultar metadatos y comprobar archivos del servidor. Se exige VIEW ANY
  DEFINITION y visibilidad de sesiones (VIEW SERVER STATE, o VIEW SERVER PERFORMANCE
  STATE desde SQL Server 2022). Una identidad sysadmin ya los posee.
  No se conceden ni elevan permisos automáticamente; sin visibilidad se detiene.
- El servicio SQL debe poder leer el .bak. El usuario de Windows y el servicio
  SQL son identidades distintas: que el archivo abra en el Explorador no basta.
- Espacio para una copia completa y sus logs. Se utilizan los directorios
  predeterminados de datos/log del servidor, que ya utiliza la instancia.
- Acceso compartido en memoria (shared memory) habilitado en SQL Express.

No se aceptan nombres de servidor o bases arbitrarios como parámetros.
El destino de conexión es lpc:LAPTOP-5TSJJUP8\SQLEXPRESS, local mediante memoria
compartida y autenticación Windows. No hay fallback TCP ni contraseña SQL.
Encrypt=Optional se limita a ese IPC local; TrustServerCertificate permanece false.
No se modifica la configuración de cifrado de la aplicación. Si el servidor
exige cifrado/certificados incompatibles, falla: no se saltea su validación.

dotnet restore puede descargar los paquetes ya utilizados por la solución si
no están en caché; no instala programas adicionales ni agrega dependencias.

## Aislamiento y controles

1. El plan contiene exactamente 28 casos C#, equivalentes a los Python anteriores.
   Se valida el plan y las guardas sin SQL antes de conectar.
2. Se conecta únicamente a master para inspeccionar motor y respaldo.
   Nunca abre HotelTramontoDB ni HotelTramontoDB_Pruebas.
3. Genera HotelTramontoDB_HT56_<GUID de 32 caracteres>, sin opción de elegir el nombre.
4. Inspecciona RESTORE HEADERONLY y FILELISTONLY. Exige un solo FULL del nombre
   correcto y versión compatible; rechaza archivos no ordinarios.
5. Comprueba que no exista la base y que cada archivo nuevo no exista ni esté
   en uso. Restaura con MOVE a nombres físicos únicos; nunca usa WITH REPLACE.
6. Registra una marca GUID en la copia, su database_id, create_date, propietario
   y todos los archivos físicos. Si falta identidad completa, no se elimina.
7. Rechaza triggers activos, sinónimos, CLR de usuario, TRUSTWORTHY o tablas que
   sean vistas. Comprueba los cuatro procedimientos heredados que ejecutarán las
   pruebas contra sus definiciones revisadas. Diferencias de lógica requieren
   revisión; no se sobrescriben para hacer pasar el ensayo.
8. Cada nueva conexión a la copia verifica nombre y marca de ejecución.
9. Pide escribir exactamente INSTALAR antes de aplicar cualquier migración.
   No hay -Force, -Yes ni opción de confirmación automática.
10. Ejecuta 00 -> 01 -> 02 -> 04 -> 05 y las 28 pruebas con conexiones físicas
    independientes para los contendientes de los casos concurrentes.
11. Compara cada fila/columna histórica de once tablas antes de la migración y
    después de las pruebas mediante hashes, usando solo las columnas originales.
    Los fixtures nuevos quedan exclusivamente en la copia.
12. Al finalizar valida otra vez marca, ID, creación, propietario y archivos.
    Si hay otras sesiones, cambios de identidad o falta de permisos, NO elimina.
    No usa ROLLBACK IMMEDIATE ni expulsa conexiones ajenas.
13. Solo con esas pruebas de identidad elimina la base temporal. SQL elimina sus
    archivos propios; no se borran carpetas ni archivos originales desde el cliente.
14. Escribe informe.json e informe.txt con casos aprobados, fallidos y pendientes,
    integridad histórica y resultado de limpieza en:
    %LOCALAPPDATA%\HotelTramonto\VerificacionesExpress\<GUID>.

Si falla antes de ejecutar los casos, los 28 figuran pendientes, no aprobados.
Un fallo de una prueba no se cuenta como éxito aunque SQL rechace otra operación.
Un ensayo solo termina con exit code 0 si pasan las 28, se conserva la historia,
se completan las verificaciones y se elimina con seguridad la copia.
El informe puede contener metadatos, definiciones y errores: mantenerlo privado.

## Scripts protegidos y orden

Los siete .sql originales permanecen SIN CAMBIOS y solo aceptan
HotelTramontoDB_Pruebas. El ejecutor no los reescribe en disco.
Para la copia transforma una única guarda reconocida EN MEMORIA y agrega
controles de nombre derivado del GUID, GUID no vacío, marca en la base y DB_ID>4.
No acepta sustituir la guarda por un nombre arbitrario. Si la guarda cambia,
hay varios lotes o aparece USE, se detiene.

| Paso | Función |
|---|---|
| 00_Diagnostico.sql | Verifica categoria y contratos previos; obtiene estado inicial antes de migrar. |
| 01_Instalar.sql | Agrega asociación habitación/reserva e identidad de consumos. |
| 02_Verificar.sql | Verifica las asociaciones y restricciones antes de habilitar pagos nuevos. |
| 04_ConcurrenciaPagos.sql | Instala la identidad de pagos y el protocolo transaccional final; depende de 01. |
| 05_VerificarConcurrencia.sql | Comprueba operaciones, anomalías y definiciones finales; necesita 04. |

00 se ejecuta antes de pedir INSTALAR porque solo diagnostica. Los siguientes
cuatro pasos requieren la confirmación. 02 y 05 se repiten después de los casos.
03 y 06 son recuperación/suspensión, no parte del ensayo normal.
Si el respaldo ya contiene 5/6, se rechaza. No se desinstalan para poder ensayarlas.

La migración Servicios.categoria NO se duplica ni se instala aquí.
El rol de corrección 2 y los permisos reales de aplicación requieren su
verificación independiente: el usuario que restaura no representa al recepcionista.

## Riesgos y operaciones pendientes

- No se ejecutó SQL en cloud ni en tu computadora. Compilación y pruebas sin SQL
  verifican compatibilidad de código, no prueban RESTORE ni concurrencia real.
  Las 28 pruebas siguen pendientes hasta disponer del respaldo y ejecutar el comando.
- Si se interrumpe el proceso o se pierde conexión, puede quedar una copia.
  El informe, si alcanzó a escribirse, identifica la base. Revisar manualmente
  su GUID, marca, archivos y sesiones. Nunca eliminar por prefijo ni por nombre solo.
- Si no se acredita la identidad, la limpieza falla cerrada: conserva la copia.
  Puede quedar SINGLE_USER si un error ocurre entre restringir acceso y DROP.
- Dos instancias comparten recursos del motor: el ensayo no escribe las bases
  originales, pero RESTORE y pruebas consumen disco, memoria, CPU y conexiones.
  Ejecutarlo fuera de la operación del hotel.
- Las pruebas no reparan importes históricos ni eluden anomalías del respaldo.
- No se modificó sp_Reserva_CheckIn. Su carrera previa de SELECT/UPDATE puede
  reabrir el estado tras otro cierre. La defensa fecha_checkout de los cargos
  nuevos no corrige ese estado. Corregir check-in requiere autorización y pruebas
  multisesión específicas; no está resuelto por esta automatización.
- Un respaldo con procedimientos personalizados puede ser rechazado: se prefiere
  revisión antes de ejecutar código que pudiera acceder a otras bases.
