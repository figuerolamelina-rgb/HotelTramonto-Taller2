# Preparación del PR: correcciones 5 y 6 sobre integración 2, 3 y 4

## Alcance y referencias revisadas

- Rama de trabajo y única rama a publicar: correccion-servicios-habitaciones.
- Base de integración inspeccionada: f6cae70eca93f3df646220b975ad4953ee501c35
  (Merge PR #12: corrección 2; ya incluye el historial de PR #10).
- Rama 5/6 antes de combinar: b0ccc4851a21f6960abb5720dadf1464f44eea17.
- Se incorpora la integración EN la rama de trabajo para compilar el resultado
  real y que el futuro PR solo presente sus cambios pendientes.
  No se cambia ni publica integracion-melina-iara, main ni develop.
- No se ejecutó SQL, migración, permiso, reserva, pago ni formulario en esta tarea.

## Compatibilidad y ajustes

| Área | Resultado |
|---|---|
| Permisos de habitaciones y tipos | Se mantienen las interfaces, guards en Negocios y handlers/UI de corrección 2. Solo ADMINISTRADOR administra; recepción conserva consultas. |
| Formularios de habitaciones | Se mantienen los de integración/Iara; no se cambian Designer. |
| Historial de huéspedes | Se conservan procedimiento, consulta parametrizada, validación ADMINISTRADOR/RECEPCIONISTA, botón y formulario de 3/4. |
| Servicios.categoria | Se incorpora UNA sola migración, la existente en integración. No se duplica, reejecuta ni reclasifica el servicio 1. |
| Consumos por habitación | Entidad, Datos, Negocios, selector/filtro y FK compuesta quedan intactos respecto de b0ccc48. |
| Pagos/reintentos/cierre | Los seis procedimientos, diario persistente y recuperación de confirmación siguen intactos. |
| Reservas y reportes | Sin cambios adicionales en contratos o cálculo; se conserva el filtro H.activo=1 de integración en sp_Habitacion_Listar. |
| Runner SQL 5/6 | Se conservan los 28 casos, fixtures propios, marcas GUID, once tablas históricas, diagnóstico de siete definiciones y limpieza segura. |
| Credenciales/dependencias | Sin credenciales ni configuración privada nuevas; sin paquetes NuGet, lockfiles o cambios de conexión. |

El único conflicto de archivos fue add/add en tools/verificar_automatizacion_windows.py.
Se mantiene ese nombre para los ocho controles originales del runner C2; los siete
de Express/.NET quedan en tools/verificar_automatizacion_servicios.py.
No se descarta ninguno. Ambas suites forman parte del descubrimiento completo.

Los manifiestos de Probar-HotelTramonto.ps1 y del verificador final conservaban
huellas de fuentes previos a 3/4 y 5/6. Se actualizaron tras revisar el código
combinado y se amplió cobertura a los fuentes nuevos. Se mantiene SHA-256,
normalización de fin de línea, todos los archivos previamente cubiertos y las
listas cerradas de ejecución. No se deshabilitan verificaciones ni se autorizan
scripts nuevos por el hecho de tener su hash en el manifiesto.
Los filtros positivos originales y sus conteos 56/68 NO cambian.

## Permisos SQL complementarios: pendientes de revisión DBA, no ejecutados

El catálogo de Datos combinado contiene 62 rutas. La instalación/reversión C2
permanece con su alcance original de 60, incluido su bloqueo de instalación.
Las dos adicionales son:
- dbo.sp_Huesped_HistorialReservas.
- dbo.sp_ReservaTemporal_ConsultarConfirmacion.

Se preparan en base de datos HotelTramonto/integracion:
- 01_PermisosConsultasAdicionales_Pruebas.sql: solo dos GRANT EXECUTE al rol
  técnico existente ht_app_compartida; sin roles/membresías nuevos, sin REVOKE,
  sin DML/SELECT general ni cambios a los DENY de habitaciones.
- 02_VerificarConsultasAdicionales_Pruebas.sql: lectura de permisos bajo la
  identidad Windows REAL, comprobando ambos EXECUTE y ausencia de DML directo.

Ambos exigen exactamente HotelTramontoDB_Pruebas. Ningún runner los ejecuta.
Exigen rol y procedimientos existentes y propiedad dbo; no habilitan el script
bloqueado de permisos C2. DENY heredado/grupos deben revisarse con el token real.
El rol técnico compartido no distingue empleados: las restricciones de roles
funcionales se mantienen en la aplicación, como en la corrección 2.

La reversión C2 detectará permisos adicionales y se detendrá: no extender su
alcance ni retirar los nuevos GRANT automáticamente. Coordinar cualquier
reversión integral con el DBA y respaldar previamente el estado efectivo.

## Instalación y límites de despliegue

Este PR combina código, no migra bases. Antes de ejecutar la aplicación nueva,
la base de pruebas debe contar con:
1. Esquema y contratos de categoria ya revisados de C2.
2. Historial 3/4 instalado.
3. Migración 5/6 según 00 -> 01 -> 02 -> 04 -> 05 y revisión de recuperación.
4. Permisos efectivos de las 62 rutas para la identidad técnica aprobada,
   incluidos los dos del complemento.

No ejecutar todo el directorio SQL indiscriminadamente. En especial, no reinstalar
Procedimientos_Almacenados.sql después de las migraciones: contiene contratos base
y sobrescribiría pagos/consumos coordinados y los procedimientos de categoria.
Los scripts se conservan separados para revisión y ejecución en orden.

El runner C2 tiene un recorrido propio de migración/clasificación de categoria;
no hace falta repetirlo sobre una copia donde ya fue instalada. Preferir su
verificador final de solo lectura y la suite .NET combinada.
El runner Express 5/6 ensaya una instalación inicial: requiere un respaldo
posterior a categoria y anterior a 5/6, crea una copia temporal y la elimina.
No hay un despliegue autorizado/preparado automáticamente para HotelTramontoDB.

La ventana de pagos y alta de reservas requiere la aplicación y las nuevas
firmas SQL juntas: clientes anteriores sin identificador de operación no pueden
escribir pagos nuevos. Planificar despliegue coordinado y cerrar clientes viejos.

## Evidencia disponible

- Compilación del código combinado .NET 10: sin errores.
- 120 pruebas .NET seguras aprobadas, 0 fallidas, 0 omitidas dentro del filtro.
  Las tres pruebas antiguas que acceden a SQL se excluyen expresamente.
- Subconjuntos originales C2: 56/56 y 68/68 aprobados, preservando sus filtros.
- 47 comprobaciones estáticas aprobadas, incluyendo ambos verificadores Windows.
- 35 comprobaciones internas de Express sin SQL aprobadas.
- Siete scripts PowerShell: sintaxis validada, sin ejecutarlos.
- Usuario informó previamente 28/28 SQL, once tablas históricas intactas y
  eliminación segura. Los archivos de esos procedimientos y del ejecutor se
  conservaron byte a byte desde b0ccc48; no se reejecutaron en SQL después de combinar.
- Se verificó igualdad de las carpetas originales de categoria/permisos y las
  clases centrales C2 con el extremo de integración revisado.
- No se modificaron archivos Designer al preparar la integración.

Comando completo de pruebas sin SQL desde Windows:

    dotnet build HotelTramonto.slnx
    dotnet test HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj --no-build --filter "FullyQualifiedName!~ConexionBD_ConectaConHotelTramontoDB&FullyQualifiedName!~UsuarioNegocios_CambiarContrasena_GuardaHashValido&FullyQualifiedName!~UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull"

En Linux, agregar -p:EnableWindowsTargeting=true a restore/build.
No confundir las 120 pruebas .NET con las 28 SQL opt-in.

## Riesgos pendientes

- sp_Reserva_CheckIn sigue leyendo estado y luego actualizando sin condición
  atómica. Un check-in demorado puede reabrir un estado tras un cierre.
  Los cargos nuevos también exigen fecha_checkout NULL; no corrigen el estado.
  Resolverlo requiere una tarea autorizada que coordine check-in y cancelación.
- El ensayo SQL como DBA no prueba el token Windows operativo ni separación
  de empleados en un rol SQL compartido.
- La recuperación de una creación temporal cuyo ID se perdió sigue requiriendo
  conciliación, sin recreación ni cobro automático.
- Reversión, datos históricos anómalos, firma de módulos y permisos heredados
  necesitan revisión de despliegue, sin tocar importes existentes.
- Esta tarea no ejecutó Windows Forms, SQL ni migraciones. Las pruebas SQL
  anteriores no sustituyen la aceptación visual y de permisos del conjunto.

## Aceptación final en Windows Forms antes del merge

Usar exclusivamente una base de pruebas autorizada y verificar el archivo privado
de conexión antes de abrir la aplicación. No hacerlo contra producción.

1. ADMINISTRADOR: alta/modificación/desactivación/reactivación de habitaciones y
   tipos; consultas activas/inactivas y disponibilidad, conservando diseño y eventos.
2. RECEPCIONISTA: puede listar/consultar, no administrar habitaciones/tipos.
   Confirmar ausencia de botones y rechazo de llamadas directas desde Negocios.
3. Historial: ambos roles operativos consultan huésped activo/inactivo, sin reservas,
   varias reservas/habitaciones, CANCELADA y FINALIZADA. GERENTE no obtiene acceso.
4. Consumos: seleccionar habitación de la reserva, filtrar/listar, históricos sin
   habitación, servicio inactivo, cambio de precio y reserva fuera de CHECKIN.
5. Pagos: inicial 50/100, confirmación, saldo de alojamiento, check-in, servicio
   separado, pago parcial/pendiente/aprobación y rechazo de sobrepago.
6. Reinicio: recuperar consumo, cobro y confirmación pendientes con la misma
   operación persistida; no crear un cargo nuevo tras respuesta incierta.
7. Cierre: deuda bloquea; cierre/reintento no duplica; no registrar nuevos cargos
   después de fecha_checkout. Revisar reportes de ingresos/servicios y saldos.
8. Diseño Iara: distintos tamaños/escalado, tercera fila de huéspedes, selector de
   habitación, scroll, confirmaciones, cierre de ventanas y botones anteriores.
9. Con la identidad operativa REAL comprobar ambos EXECUTE complementarios,
   cadena dbo y DML directo denegado en habitaciones/tipos. No probarlo solo como DBA.
10. Como cierre de aceptación, repetir el runner temporal de 28 casos desde el
    commit combinado con el respaldo correcto y revisar su informe privado.

Conclusión de revisión: código preparado para CREAR un PR hacia
integracion-melina-iara. Crear/revisar el PR no autoriza su merge ni el despliegue;
la aceptación Windows y la revisión de permisos/migraciones siguen pendientes.
