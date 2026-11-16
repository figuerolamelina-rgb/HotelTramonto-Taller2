# Verificación de correcciones 5 y 6 con un comando

La automatización ensaya la migración en una instancia LocalDB nueva y desechable.
NO instala en la instancia original ni conecta a ella. Usa exclusivamente un
respaldo FULL de HotelTramontoDB_Pruebas, leído sin modificar el archivo.
La confirmación de la usuaria indica que Servicios.categoria ya está instalada
y verificada en pruebas; el respaldo debe ser posterior a esa instalación.

## Requisitos de Windows

- Windows PowerShell 5.1, SDK .NET 10.
- SQL Server Express LocalDB instalado: SqlLocalDB.exe disponible en PATH,
  versión igual o posterior a la del respaldo y con capacidad para restaurarlo.
- Python 3.10 o posterior, pyodbc y Microsoft ODBC Driver 18 for SQL Server.
  La automatización comprueba dependencias, pero no las instala silenciosamente.
- Respaldo .bak con un único backup FULL de HotelTramontoDB_Pruebas, sin requisitos
  de TDE, FILESTREAM ni características no soportadas por LocalDB.
- Usuario Windows con permiso sobre el respaldo y espacio para restaurarlo.
- Repositorio completo de correccion-servicios-habitaciones, revisado en GitHub.

No se requieren credenciales SQL, cadenas de conexión ni ejecutar como administrador.
No guardar respaldos, configuración privada ni informes en el repositorio.

## Comando exacto

Desde la raíz del repositorio, reemplazando solo la ruta por la de tu respaldo:

    powershell.exe -NoProfile -File .\tools\Probar-ServiciosHabitaciones.ps1 -BackupPath "C:\Respaldos\HotelTramontoDB_Pruebas.bak"

Si Python no se llama python en tu computadora, agregar -Python con su ruta.
Si una política de ejecución impide abrir el archivo, seguir la política de tu
organización para scripts revisados; no se proporciona un bypass de seguridad.

## Qué hace

1. Ejecuta restore/build de .NET, las pruebas .NET sin SQL y cuatro suites de
   comprobaciones estáticas. Conserva el código de salida real de cada comando.
2. Crea una instancia LocalDB HT56_<GUID> nueva; no acepta un servidor de origen
   ni una cadena de conexión como parámetros.
3. Restaura el backup en esa instancia, con nombre HotelTramontoDB_Pruebas y
   archivos de datos nuevos. Valida nombre/origen/tipo del backup. No usa REPLACE.
4. Comprueba que el motor sea LocalDB y que no haya triggers activos, sinónimos,
   ensamblados de usuario, TRUSTWORTHY ni servidores vinculados que requieran
   revisar posibles accesos externos. No altera esos automatismos.
5. Ejecuta 00 y captura en memoria todas las filas preexistentes de once tablas,
   incluidos pagos, consumos, reservas, temporales, catálogos y usuarios.
6. Se detiene ANTES de instalar. Es obligatorio escribir exactamente INSTALAR.
   Otra respuesta cancela y limpia la copia. No existe opción de confirmación automática.
7. Instala 01 -> 02 -> 04 -> 05 solo en la copia y comprueba que las columnas
   originales de cada fila histórica sigan iguales.
8. Ejecuta exactamente 28 pruebas SQL, sin omitidas. Python exige LocalDB, nombre
   de base y una marca GUID que solo existe en la copia de esa ejecución.
   Una ejecución de cero pruebas, fallida o incompleta nunca se informa aprobada.
9. Vuelve a comparar las filas históricas y ejecuta 02/05 otra vez.
10. En finally, detiene y elimina únicamente la instancia creada y sus archivos
    desechables. Conserva los informes privados en
    %LOCALAPPDATA%\HotelTramonto\Verificaciones\<GUID>, fuera de Git.
    Si falla la limpieza, lo informa; nunca usa una instancia ajena como alternativa.

Los fixtures, pagos y cambios de catálogo de pruebas viven solo en esa copia:
no requieren borrar ni restaurar filas de la base de origen. Los registros
históricos de la copia se comparan por clave y valores originales, no solo por
totales. Si alguna prueba los modifica, la verificación falla y descarta la copia.

Las conexiones son named pipes locales autenticadas por Windows. No se permite
un host TCP/remoto ni se configura TrustServerCertificate. Encrypt=Optional se
limita a ese IPC de LocalDB; no cambia la configuración cifrada de la aplicación
ni la política de ninguna conexión remota.

## Por qué 00 -> 01 -> 02 -> 04 -> 05

| Script | Función y dependencia |
|---|---|
| 00_Diagnostico.sql | Comprueba categoria y contratos previos; obtiene las definiciones y valores iniciales antes de modificar el esquema. |
| 01_Instalar.sql | Agrega habitación/operación y la integridad compuesta de DetalleServicios; prepara consumos y cierre. Solo etapa inicial. |
| 02_Verificar.sql | Detecta asociaciones inválidas y restricciones ausentes antes de habilitar los pagos nuevos. |
| 04_ConcurrenciaPagos.sql | Agrega la identidad de pagos e instala el protocolo final de los seis escritores y la consulta de recuperación. Depende del modelo creado por 01. |
| 05_VerificarConcurrencia.sql | Comprueba idempotencia, anomalías históricas y las definiciones finales instaladas. Solo puede verificar la versión final después de 04. |

03_Recuperacion.sql es una recuperación de la etapa inicial, no un paso normal:
se bloquea después de instalar 04 para no volver a escritores inseguros.
06_SuspenderEscrituras.sql es la recuperación conservadora posterior: suspende
las seis escrituras preservando todos los datos. La automatización no ejecuta
03 ni 06; descarta su instancia propia ante fallos.

Si el respaldo YA contiene la migración de concurrencia (Pagos.id_operacion),
01 falla deliberadamente. Este comando ensaya una instalación inicial, no
desinstala cambios de un respaldo migrado. Usar un respaldo anterior a las
correcciones 5 y 6, pero posterior a categoria.

La automatización no concede permisos de la corrección 2 ni duplica su migración.
El ensayo se realiza como propietario de LocalDB; comprobar después los permisos
de la identidad operativa en el entorno de pruebas real sigue siendo necesario.

## Riesgo pendiente de check-in

No se modificó sp_Reserva_CheckIn. Su SELECT de estado y UPDATE posterior no son
atómicos: dos check-ins intercalados con un check-out podrían dejar un estado
reabierto o una marca de entrada incorrecta. Esto no se resuelve ejecutando la suite.

Los escritores nuevos exigen también fecha_checkout NULL para aceptar cargos
nuevos. Esa defensa evita consumos y pagos posteriores al cierre, pero no corrige
el estado. La solución propuesta para una tarea autorizada es bloquear la reserva,
revalidar estado/saldo en la misma transacción y actualizar condicionalmente
WHERE estado='CONFIRMADA' AND fecha_checkout IS NULL, comprobando @@ROWCOUNT.
Debe coordinarse con cancelación y probarse multisesión. No se implementó ni se
autoriza automáticamente con este comando.

## Alcance de la validación realizada en cloud

Se verifican compilación .NET, pruebas sin SQL, comprobaciones estáticas y sintaxis
PowerShell. No se ejecutaron LocalDB, RESTORE, los scripts de instalación ni las
28 pruebas SQL. El recorrido Windows completo queda pendiente de tu ejecución.
Los informes pueden contener datos de la copia y definiciones: mantenerlos privados.
La base original no queda migrada al terminar; el resultado es un ensayo aislado.
