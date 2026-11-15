# Correcciones 3 y 4: historial de huéspedes

Rama: correccion-historial-huespedes, creada desde el último FETCH_HEAD remoto
de integracion-melina-iara. No se incorporaron otras ramas ni se realizaron merges.

## Instalación en SSMS

1. Usar una instancia de pruebas, nunca la base real. El nombre debe ser
   HotelTramontoDB_Pruebas. El nombre por sí solo no permite distinguir producción:
   comprobar también la instancia y el entorno antes de ejecutar.
2. La base debe contener el esquema versionado del proyecto, incluidas
   Reservas.fecha_checkin y fecha_checkout. No ejecutar el script de creación
   de tablas sobre una base existente.
3. Seleccionar esa base en SSMS y revisar/ejecutar únicamente
   base de datos HotelTramonto/Actualizacion_HistorialHuespedes.sql.
   Crea o actualiza dbo.sp_Huesped_HistorialReservas y puede repetirse.
   No cambia tablas ni datos; la consulta solo lee. Requiere permisos de DDL
   del instalador y EXECUTE para el usuario de conexión según la política local.
4. En la base de pruebas, ejecutar
   base de datos HotelTramonto/pruebas/HistorialHuespedes_Consulta.sql.
   Solo consulta datos existentes y usa una tabla temporal; no crea reservas.
   No se ejecutó ninguno de estos scripts desde cloud.

## Uso y alcance

En Gestión de huéspedes, seleccionar un huésped y pulsar Historial de reservas.
ADMINISTRADOR y RECEPCIONISTA pueden consultar; Negocios también exige el rol.
GERENTE, roles desconocidos y la apertura sin rol no habilitan este historial.
No se cambian otros permisos del módulo.

La ventana de solo lectura conserva la paleta marrón, crema y dorado.
Muestra una fila por reserva y habitación; si no tiene detalles, conserva la
reserva con habitación nula. Incluye huésped (ID, DNI, nombre, apellido),
fecha de creación, entrada/salida previstas, estado y check-in/out de la reserva.
Un huésped inexistente o sin reservas definitivas devuelve cero filas.
No se filtran huéspedes inactivos ni reservas CANCELADA o FINALIZADA.
No se incluyen reservas temporales pendientes: son otro ciclo de vida.

Los nombres reales son Reservas, DetalleReservas, Habitaciones y TiposHabitacion.
La restricción UNIQUE(id_reserva,id_habitacion) garantiza una asociación por par.
Los JOIN siguen claves primarias y no unen pagos ni servicios, evitando multiplicar
habitaciones. No se usa DISTINCT para ocultar errores de integridad.

Las habitaciones están **asociadas/reservadas**, no certificadas como utilizadas.
fecha_checkin y fecha_checkout son de la reserva, no de cada habitación.
No hay registro independiente de ocupación real por habitación. El número y tipo
mostrados son del catálogo actual; tampoco existe una copia histórica de esos
atributos. No se deduce ocupación de Habitaciones.estado (es el estado actual).

## Validación

SDK .NET 10.0.404 en Linux. Solución compilada con EnableWindowsTargeting=true:
0 errores, 122 advertencias del código existente. Suite segura: 54 aprobadas,
0 fallidas, 0 omitidas. Se excluyeron explícitamente las tres pruebas SQL
documentadas en README, incluida la que cambia una contraseña.
Las 9 ejecuciones nuevas comprueban roles e identificadores sin abrir conexión.

Pendiente en Windows/SQL Server:
- Ejecutar el script de pruebas con datos de prueba ya existentes. Comprueba
  cada huésped, cantidad de filas, asociaciones, estados, check-in/out e IDs
  cero, negativos y nulos. Sus listados finales identifican la cobertura real:
  se necesita huésped sin reservas, con una y varias reservas, reserva con
  varias habitaciones y estados CANCELADA y FINALIZADA. Una categoría ausente
  queda pendiente; no significa prueba aprobada.
- Comparar visualmente DNI y fechas con los registros fuente.
- Probar el botón con administrador y recepcionista, selección vacía, huésped
  inactivo, sin reservas, SQL inaccesible y procedimiento no instalado.
- Confirmar que todas las habitaciones se ven al desplazar horizontalmente la
  tabla y que los controles existentes siguen funcionando.
- Verificar permisos EXECUTE con la identidad Windows de cada computadora.

Linux no ejecuta Windows Forms ni valida autenticación integrada Windows.
No se conectó a SQL Server ni se modificaron datos históricos.

## Archivos y coordinación

Modificados: HuespedDatos.cs, HuespedNegocios.cs, FormHuespedes.cs y Form1.cs.
Nuevos: script de instalación, script SQL de pruebas, HistorialHuespedTests.cs
y este documento. Se mantiene DataTable como patrón existente; no se requieren
nuevas entidades ni proyectos. No se tocaron archivos Designer.

Posible conflicto con Iara: FormHuespedes.cs agrega un botón y una tercera fila,
desplazando la tabla; coordinar cualquier cambio simultáneo del mismo formulario.
Con corrección 2, el punto compartido potencial es Form1.cs: aquí solo se pasa
rolUsuario a FormHuespedes. No se modifican FormHabitaciones, Permisos.cs,
procedimientos de reservas/pagos/check-in/out ni reglas de disponibilidad.
Se autoriza commit y push exclusivamente en correccion-historial-huespedes; no merges ni PR.

## Revisión final para publicación

Se comparó la rama remota correccion-permisos-habitaciones con la base de esta
rama mediante fetch y diff, sin incorporar sus cambios.
Extremo remoto revisado: 1cc34b1888bf85df0c4814d2108674fdf587716e. No hay archivos
modificados en común en los extremos revisados: aquella rama modifica
HabitacionDatos, TipoHabitacionDatos, sus interfaces y negocios, formularios de
habitaciones, pruebas, README y scripts/documentación propios. Form1.cs y
FormHuespedes.cs no presentan cambios en esa comparación. No se conocen
conflictos de archivos con esa revisión; cambios futuros requieren otra revisión.

La instalación y el script de pruebas rechazan una base distinta de
HotelTramontoDB_Pruebas. El procedimiento instalado también verifica ese nombre
al ejecutarse. No hay USE que cambie silenciosamente la base. El procedimiento
solo hace SELECT; el script de pruebas escribe exclusivamente en #Historial.
La protección del nombre no sustituye comprobar la instancia de pruebas.

Los estados se conservan tal como están registrados: CONFIRMADA, CANCELADA,
CHECKIN y FINALIZADA. No se calcula un estado vigente a partir de fechas.
Una reserva con varias habitaciones tiene una fila por asociación; repetir
su ID en esas filas es intencional y no significa reservas adicionales.
La restricción única del esquema y los JOIN por claves impiden duplicar el par.

Se revisó el recorrido Form1 -> FormHuespedes(rolUsuario) -> Negocios.
Ambos roles operativos llegan al botón; Negocios vuelve a comprobar el permiso.
La revisión visual efectiva requiere Windows. En el código se mantienen los
seis botones existentes, sus eventos y las dos primeras filas en su posición;
se añade una tercera fila y se ajusta la altura de la tabla sin editar Designer.

Para probar en Windows:
1. Obtener esta rama y usar .NET 10 / Visual Studio con Windows Forms.
2. Configurar el archivo privado indicado en README con BaseDatos:
   HotelTramontoDB_Pruebas y la instancia de pruebas correcta. No compartirlo.
3. Revisar e instalar únicamente el script de historial en esa base existente
   con el esquema compatible; luego ejecutar el script de pruebas de consulta.
4. Ejecutar dotnet restore HotelTramonto.slnx y dotnet build HotelTramonto.slnx.
5. Ejecutar dotnet run --project HotelTramonto.Presentacion --no-build.
6. Iniciar sesión con cada rol autorizado, abrir Gestión de huéspedes, seleccionar
   huésped y consultar Historial de reservas. Revisar estados y habitaciones.
7. Comprobar tamaños de ventana, desplazamiento de columnas y los botones previos.

Revalidación final: compilación completa con 0 errores y 122 advertencias
preexistentes; 54 pruebas seguras aprobadas, 0 fallidas y 0 omitidas dentro
del filtro. Las tres pruebas SQL permanecen excluidas. La comprobación estática
contrastó cada columna usada por el procedimiento con el CREATE TABLE versionado,
verificó las guardas de base y la ausencia de escrituras en su cuerpo.
Estas comprobaciones no sustituyen la ejecución SQL ni la prueba visual pendiente.
