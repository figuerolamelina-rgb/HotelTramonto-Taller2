# Error 51438: diagnóstico de las siete definiciones

La ejecución informada restauró y limpió correctamente la copia. Las 28 pruebas
quedaron pendientes porque 05 rechazó la representación de sp_Pago_Insertar.
No tenemos el OBJECT_DEFINITION de aquella copia, ya eliminada: no se puede
afirmar cuál era su diferencia exacta solo comparando los dos archivos Git.

## Qué puede cambiar

OBJECT_DEFINITION devuelve el texto que SQL Server conserva en sys.sql_modules,
no un árbol sintáctico ni una representación canónica. La cabecera conservada
al procesar CREATE OR ALTER puede representarse como CREATE/ALTER y variar en
los espacios del verbo DDL. Un hash literal considera esa diferencia significativa.

HASHBYTES no introduce diferencias aleatorias: mismos bytes y algoritmo producen
el mismo hash. Sí pueden diferir varchar frente a nvarchar, longitud o truncación,
CR/LF y caracteres invisibles. OBJECT_DEFINITION también puede devolver NULL por
objeto inexistente, cifrado o visibilidad insuficiente; eso sigue rechazándose.

La corrección no presupone la causa concreta de la ejecución anterior.
El próximo ensayo captura la evidencia de los siete procedimientos ANTES de 05,
incluso si luego falla la verificación y se elimina la copia con seguridad.

## Comparación corregida

05 conserva sus guardas de base, integridad y error 51438. Los siete textos
esperados siguen siendo literalmente los mismos que instala 04; hay pruebas
.NET y una comprobación antes de SQL para detectar cualquier divergencia.

ÚNICA normalización permitida: el verbo inicial CREATE, ALTER o CREATE OR ALTER,
su uso de mayúsculas/minúsculas y el espacio ASCII, tabulación, CR o LF anterior
a la palabra PROCEDURE y entre las palabras de esa cabecera DDL.
Se reemplaza solo ese prefijo por CREATE PROCEDURE.

Desde el carácter inmediatamente posterior a PROCEDURE se conserva TODO:
espacio que precede al nombre, esquema y nombre, parámetros y sus valores
predeterminados, tipos, transacciones, locks, timeout, controles, consultas,
literales, comentarios, mayúsculas y saltos de línea. No se quitan comentarios
ni se colapsa formato del cuerpo. Incluso un cambio LF -> CRLF en el cuerpo
produce rechazo: no se normaliza porque puede alterar una cadena SQL.

SQL compara longitud en bytes y SHA-256 de varbinary(max) obtenido del texto
nvarchar(max) canónico. Los siete se procesan con la misma regla. NULL, cabeceras
no reconocidas o cuerpos diferentes siguen deteniendo toda la instalación.
El comparador .NET nuevo sigue la misma regla estricta; no utiliza el comparador
de formato más amplio de los cuatro procedimientos heredados.

No se cambia 04 ni la lógica de ningún procedimiento. Tampoco la identidad,
protección de originales, confirmación INSTALAR o limpieza de la copia.

## Evidencia del próximo ensayo

En la carpeta del informe NUEVO:
%LOCALAPPDATA%\HotelTramonto\VerificacionesExpress\<GUID-nuevo>

- diagnostico-definiciones.json: siete entradas, versión y collation del servidor,
  disponibilidad de definición, longitudes, DATALENGTH, hashes de SQL y de .NET,
  igualdad literal y con cabecera normalizada, primera diferencia y contexto.
- <procedimiento>.esperado.sql: texto decodificado de 04.
- <procedimiento>.observado.sql: texto recibido de OBJECT_DEFINITION, si existe.

Se guardan CR y LF sin reescribirlos. El JSON muestra el contexto escapado y cada
unidad UTF-16 en hexadecimal, permitiendo distinguir 0020, 0009, 000D y 000A.
Los hashes .NET usan UTF-16LE; el parámetro esperado de SQL está tipado explícitamente
nvarchar(max), y se registran sus hashes sobre varbinary(max).
También se informan uses_ansi_nulls y uses_quoted_identifier para investigar
configuración del módulo. El diagnóstico es SELECT de metadatos, sin cambios SQL.

Interpretación:
- Literal diferente + canónica igual: la diferencia está solo en el prefijo DDL.
- Canónica diferente: sigue siendo error; revisar ambos textos y la primera diferencia.
  No se permite continuar ni reemplazar automáticamente el procedimiento observado.
- Hash SQL esperado diferente del hash .NET esperado: investigar representación
  o codificación antes de atribuir el problema al cuerpo.
- Definición no disponible: revisar existencia/visibilidad; no aceptar como coincidencia.

Los archivos quedan fuera de Git y sobreviven a la limpieza de la copia.
El informe del GUID anterior no se modifica ni se utiliza para instalar.

## Pruebas sin SQL

Se cubren los siete manifiestos y cabeceras CREATE/ALTER/CREATE OR ALTER.
Se rechazan cambios de nombre, tipos/parámetros, estados, importes, idempotencia,
locks, timeout, COMMIT/ROLLBACK, eliminación de transacción, literales, comentarios
y finales de línea del cuerpo. No se ejecutó SQL en esta tarea.

La causa exacta del ensayo del usuario queda pendiente de confirmar con ese
diagnóstico nuevo; no se declara ejecutada ninguna de las 28 pruebas SQL.

## Actualizar prueba-correccion56 sin merge ni reset

Primero comprobar que no hay cambios locales pendientes y conservarlos si existen.
Desde la raíz del checkout:

    git status --short
    git fetch origin
    git switch prueba-correccion56
    git cherry-pick <commit-de-esta-correccion>

No se necesita traer de nuevo todos los cambios previos si esa rama ya ejecutó
el runner Express. El commit de corrección se informa en la entrega y queda en
la historia de origin/correccion-servicios-habitaciones.
Si hay conflictos, detenerse; git cherry-pick --abort conserva el estado anterior.
No usar reset --hard ni sobreescribir archivos locales para resolverlos.

Volver a ejecutar con el mismo respaldo válido (el ensayo anterior no lo modificó):

    dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup "C:\Respaldos\HotelTramontoDB_Pruebas.bak"

Autorizar INSTALAR solo en la nueva copia temporal. Si vuelve a fallar 51438,
las siete diferencias estarán disponibles en la carpeta del informe.

Validación local de la corrección: compilación sin errores, 86 pruebas .NET sin
SQL aprobadas, 35 comprobaciones internas del ejecutor y 26 comprobaciones
estáticas aprobadas. Las 28 pruebas SQL no se ejecutaron en esta tarea.
