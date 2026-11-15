# Revisión de preparación local

Rama de trabajo: `preparacion-entorno-local`, creada desde
`integracion-melina-iara` en `f5d58b67ab73b94925004d4e1c6cb0c5ad549b77`.
No se realizaron commits, pushes, merges ni operaciones sobre SQL Server.

## Advertencias de compilación

La compilación inicial de esta rama y la compilación después de los cambios
reportan 122 advertencias y cero errores. Las 122 están en Presentación;
ninguna corresponde a `FormLogin.cs`. Se cuentan una vez: MSBuild repite los
mensajes en el resumen final.

| Código | Cantidad | Prioridad y significado |
| --- | ---: | --- |
| CS8602 | 53 | Alta revisión: posible desreferencia nula; puede causar cierres o fallos en acciones de pantalla. |
| CS8604 | 4 | Alta revisión: argumento posiblemente nulo al agregar un ítem o pasar estado/categoría a Negocios. |
| CS8600 | 10 | Media: conversión de resultado posiblemente nulo; revisar los usos posteriores. |
| CS8618 | 38 | Media: campos de controles sin inicialización que el compilador pueda probar. Revisar todas las rutas del constructor antes de descartar la advertencia. |
| CS8622 | 17 | Baja: firma `object sender` frente al contrato nullable de EventHandler; no demuestra por sí sola un fallo de ejecución. |

Ejemplos revisados: `FormServicios.cs:383` y `:423` usan
`cmbCategoria.SelectedItem.ToString()` para pasar una categoría a Negocios.
`FormServicios.cs:232` accede a una columna tras comprobar `Contains`, caso que
requiere evaluar el contexto antes de declararlo un defecto. En `FormPagos.cs`
los controles se asignan a través de `ConfigurarFormulario()` después de
`InitializeComponent()`, patrón que explica parte de CS8618, aunque no prueba
que todas las rutas sean seguras. No usar `!`, desactivar nullable o suprimir
advertencias indiscriminadamente.

| Archivo (Presentación) | Advertencias |
| --- | ---: |
| `FormPagos.cs` | 29 |
| `FormRegistrarConsumos.cs` | 29 |
| `FormServicios.cs` | 13 |
| `Form1.cs` | 13 |
| `FormReservas.cs` | 11 |
| `FormReportes.cs` | 10 |
| `FormNuevaReserva.cs` | 6 |
| `FormHuespedes.cs` | 5 |
| `FormModificarHabitacion.cs` | 2 |
| `FormModificarUsuario.cs` | 2 |
| `FormNuevaHabitacion.cs` | 2 |

No se corrigieron estas advertencias en esta tarea: incluyen reservas, pagos y
huéspedes, excluidos del alcance. La lista no equivale a 122 errores confirmados;
conviene revisar primero CS8602/CS8604 con selecciones vacías, columnas ausentes y
resultados nulos, y después inicialización de controles y firmas de eventos.
Los diseños de formularios permanecen intactos.

## Dependencias y seguridad

El aviso anterior de `System.Security.Cryptography.Pkcs` 9.0.18 correspondía al
checkout inicial. Era importante: NU1902 y NU1903 indicaban vulnerabilidades
moderadas y altas (GHSA-2fmv-rv2c-59x7 y GHSA-846f-f764-76mx).
La rama de integración **ya** tiene una referencia explícita a 9.0.21 en Datos.
Los assets restaurados resuelven esa versión; no se modificaron csproj, paquetes
ni lockfiles para esta preparación.

La consulta actual de NuGet con `--vulnerable --include-transitive` sobre los
cinco proyectos, habilitando Windows targeting, no reportó paquetes vulnerables.
Esto refleja la fuente de avisos consultada al ejecutar la revisión, no una
garantía frente a vulnerabilidades futuras. No hay motivo en este resultado
para actualizar más dependencias. El primer intento de auditoría en Linux no
pudo restaurar el target Windows; se corrigió habilitando ese target y se
completó la consulta.

## Contratos de usuarios y SQL

`Procedimientos_Almacenados.sql` ya incluye:

- `dbo.sp_Usuario_BuscarPorEmail(@email VARCHAR(100))`, que devuelve
  `id_usuario`, `nombre`, `apellido`, `nombre_rol`, `activo` y `contrasena`, entre
  otros campos. Coincide con el parámetro que envía `UsuarioDatos.BuscarPorEmail`
  y con los campos usados por `UsuarioNegocios.Autenticar` y el login.
- `sp_Usuario_Insertar` con `@contrasena VARCHAR(255)` y
  `@longitud_contrasena INT`: rechaza NULL o longitudes menores que 8 antes del INSERT.
- `sp_Usuario_CambiarContrasena` con el mismo contrato de hash y longitud:
  rechaza NULL o menos de 8 antes del UPDATE.

Negocios valida el texto original, calcula su longitud y luego genera el hash;
Datos transmite ambos parámetros. SQL valida la longitud declarada: no puede
recuperar ni verificar la longitud original a partir del hash. Es una defensa
complementaria al negocio, no una comprobación independiente de la contraseña.
No se cambió el algoritmo ni la autenticación de usuarios existentes, y no se
agregaron contraseñas, hashes o cuentas iniciales al repositorio.

El script ya satisface lo solicitado y se conserva sin cambios. La revisión fue
estática; no demuestra que estos procedimientos estén instalados en la base de
cada integrante ni que esa base tenga el esquema actualizado.

## Login y conexión

`ConexionBD` construye conexiones cerradas desde un archivo local por computadora,
con autenticación Windows y un constructor de cadenas que evita reinterpretar
valores como opciones adicionales. No existe fallback a un servidor privado.
La plantilla compartida no contiene credenciales. El archivo privado queda fuera
del checkout y se agregó una regla de ignore adicional.

`FormLogin` valida campos vacíos con un mensaje fijo y conserva el aviso conjunto
para credenciales incorrectas/usuario inactivo. Los errores restantes, incluidos
ArgumentException del proveedor o de la configuración, tienen un aviso genérico;
se eliminó la ruta que mostraba `ex.Message`. No se modificaron tamaños, controles,
estilos ni Designer. La prueba visual debe realizarse en Windows.

## Validación realizada y límites

- Restore y build de los cinco proyectos con SDK 10.0.404 y
  `-p:EnableWindowsTargeting=true`: cero errores, 122 advertencias.
- 45 casos de prueba pasaron, cero fallaron y cero omitidos por el runner:
  33 existentes sin SQL y 12 nuevos de configuración/validación de contraseña.
  Las contraseñas temporales nuevas se generan en memoria; no se guardan hashes.
- Se excluyeron del descubrimiento mediante filtro tres pruebas existentes:
  `ConexionBD_ConectaConHotelTramontoDB`,
  `UsuarioNegocios_CambiarContrasena_GuardaHashValido` y
  `UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull`.
  La segunda cambia una contraseña real; no se ejecutó ninguna de ellas.
- Las pruebas nuevas verifican lectura de instancia SQL Express, autenticación
  integrada, cifrado, certificado seguro por defecto y opt-in local, conexión
  cerrada, separación de opciones de conexión, archivo ausente, JSON inválido,
  campos vacíos, propiedades desconocidas y rechazo de contraseña menor que 8
  antes de acceder a SQL.
- Auditoría NuGet transitiva completada sin vulnerabilidades reportadas.
- No hay SQL Server de las integrantes disponible ni runtime Windows Desktop en
  Linux: no se ejecutaron scripts SQL, no se abrió ninguna conexión en las
  pruebas seleccionadas y no se verificaron pantallas ni login real.

Para completar la comprobación en cada equipo, seguir `README.md`: configurar
el archivo privado, verificar permisos del usuario Windows y el esquema instalado,
y probar los escenarios de login con cuentas existentes. No usar la prueba de
cambio de contraseña sobre usuarios reales.
