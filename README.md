> **Versión integrada de Iara y Melina:** ver [informe y aceptación final](docs/integracion-final-iara-melina.md).
> Las bases permanentes no se usan para pruebas de escritura. Los scripts de `referencias-iara` son referencias originales, no un instalador. El modo `--backup-ciclo-ui` prepara una copia temporal para validar los 36 casos y Windows Forms, y luego intenta su limpieza protegida.

# Hotel Tramonto: ejecución local

Aplicación Windows Forms en .NET 10 con SQL Server y cuatro capas:
Presentación, Negocios, Datos y Entidades. Melina e Iara pueden usar instancias
distintas de SQL Server sin editar el código de conexión.

## Requisitos en cada computadora

- Windows y Visual Studio con soporte para .NET 10 y desarrollo de escritorio .NET,
  o SDK .NET 10 y un editor. En cloud se verificó con SDK 10.0.404.
- SQL Server accesible y la base `HotelTramontoDB` con el esquema y los procedimientos
  del proyecto. El usuario de Windows que ejecuta la aplicación debe tener acceso.
- Una cuenta de aplicación existente, activa y con hash compatible con
  `PasswordService`. No se incluyen cuentas, contraseñas ni hashes de prueba nuevos.

## Configurar la conexión por computadora

Desde la raíz del checkout, ejecutar en PowerShell:

```powershell
$carpeta = Join-Path $env:LOCALAPPDATA 'HotelTramonto'
New-Item -ItemType Directory -Force $carpeta | Out-Null
$archivo = Join-Path $carpeta 'conexion.local.json'
if (-not (Test-Path $archivo)) {
    Copy-Item '.\configuracion\conexion.ejemplo.json' $archivo
}
notepad $archivo
```

Completar `Servidor` con la instancia de esa computadora (por ejemplo
`.\SQLEXPRESS`) y `BaseDatos` con la base instalada. Las barras invertidas dentro
del JSON se escriben dobles, como en la plantilla. Ambas integrantes realizan este
paso por separado. No se usa el antiguo servidor `FUN` como valor predeterminado.

El archivo se guarda en `%LOCALAPPDATA%\HotelTramonto\conexion.local.json`, fuera
del checkout y del directorio de compilación. También está ignorado por Git si se
copia accidentalmente al repositorio. Solo se comparte `conexion.ejemplo.json`.
No agregar datos privados a esa plantilla ni forzar la inclusión de archivos ignorados.

Opcionalmente, `HOTELTRAMONTO_CONFIG` permite seleccionar otra ruta de archivo;
es una ruta, no una cadena de conexión ni una contraseña. Si se utiliza, debe estar
definida en el proceso que inicia la aplicación (reiniciar Visual Studio si corresponde).

La autenticación es integrada de Windows: no hay campos para usuario SQL ni
contraseña. Se rechazan propiedades desconocidas para detectar errores de configuración.
El cifrado de transporte es obligatorio y el certificado se verifica por defecto.
En una instancia exclusivamente de desarrollo con certificado autofirmado puede
habilitarse explícitamente `ConfiarCertificadoServidor: true` en el archivo privado.
Esto conserva el cifrado, pero omite comprobar la identidad del servidor; para
servidores compartidos se recomienda un certificado válido y mantener `false`.

Si falta el archivo o su contenido no es válido, no se intenta conectar a otra
instancia automáticamente. La configuración se vuelve a leer al solicitar una conexión.

## Preparar la base y ejecutar

Los scripts están en `base de datos HotelTramonto`. `Creacion_Base_HotelTramonto.sql`
crea tablas y datos de catálogo sobre una base ya creada; no es una migración
repetible. No ejecutarlo sobre una base que ya tiene tablas. Los scripts de
actualización, índices y procedimientos deben revisarse según el estado de cada
base. No ejecutar todo indiscriminadamente ni usar una base de producción para pruebas.

Para login se necesitan `Usuarios.email`, `Usuarios.contrasena`, `Usuarios.activo`,
la relación con `Roles` y `dbo.sp_Usuario_BuscarPorEmail`. Los procedimientos de
alta y cambio de contraseña requieren `@longitud_contrasena` (mínimo 8). El script
actual ya contiene esos contratos; ver [la revisión](docs/revision-preparacion.md).
Si falta la cuenta inicial, debe provisionarse de forma privada y controlada con
el responsable de la base, usando el hash generado por `PasswordService`;
no guardar credenciales ni hashes en scripts compartidos.

En Windows, desde la raíz:

```powershell
dotnet restore HotelTramonto.slnx
dotnet build HotelTramonto.slnx --no-restore
dotnet run --project HotelTramonto.Presentacion --no-build
```

También puede abrirse `HotelTramonto.slnx` en Visual Studio y elegir Presentación
como proyecto de inicio. Configurar la conexión antes de iniciar sesión.

## Pruebas seguras sin SQL y verificación manual

La suite contiene una prueba que **cambia la contraseña de un usuario**. Para las
pruebas sin SQL, usar este filtro desde la raíz (PowerShell o Bash):

```text
dotnet test HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj --filter "FullyQualifiedName!~ConexionBD_ConectaConHotelTramontoDB&FullyQualifiedName!~UsuarioNegocios_CambiarContrasena_GuardaHashValido&FullyQualifiedName!~UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull"
```

En Windows comprobar manualmente: campos vacíos, credenciales incorrectas, usuario
inactivo, archivo local ausente o inválido, SQL inaccesible y login correcto con
un usuario existente. Los errores de infraestructura deben mostrar un aviso
genérico, sin servidor, rutas, consultas, stack traces ni mensajes de excepciones.
No ejecutar la prueba que cambia contraseñas sobre cuentas reales.

En Linux puede compilarse la solución agregando `-p:EnableWindowsTargeting=true`
a restore/build. Las pruebas sin SQL corren en .NET normal. La interfaz Windows
Forms requiere `Microsoft.WindowsDesktop.App` en Windows: no se validaron aquí
pantallas, autenticación integrada contra SQL ni procedimientos ejecutados en SQL Server.

## Corrección 2: autorización de habitaciones

Ver [implementación, límites y pruebas](docs/correccion-2-habitaciones.md).
Los scripts nuevos de diagnóstico y permisos están en
`base de datos HotelTramonto/permisos-habitaciones`; deben revisarse y probarse
antes de ejecutarlos. No separan empleados ante SQL cuando comparten identidad Windows.

Para probar esta rama sin tocar la base original, seguir
[Pruebas en Windows](docs/pruebas-windows.md). Los once scripts nuevos requieren
seleccionar `HotelTramontoDB_Pruebas` y no cambian de base automáticamente.
La plantilla `configuracion/conexion.pruebas.ejemplo.json` apunta a esa copia.
La instalación de permisos SQL sigue bloqueada y pendiente.

Para ejecutar el recorrido controlado desde un único comando en Windows, ver
[Automatización de pruebas](docs/automatizacion-windows.md).

Para la aceptación final sin repetir migración, ver
[Resultados y verificador final](docs/resultados-finales-correccion2.md).
El nuevo comando ejecuta 68 casos .NET y verifica contratos SQL mediante metadatos,
sin instalar migraciones ni permisos.

## Integración de correcciones 2 a 6

Ver [preparación del PR y aceptación Windows](docs/preparacion-pr-correcciones56.md).
Se conservan las herramientas de corrección 2 y el runner independiente de 28
pruebas SQL en copia temporal de correcciones 5/6. Los dos recorridos tienen
objetivos y requisitos distintos; no ejecutar migraciones indiscriminadamente.

La suite .NET combinada se ejecuta con el filtro seguro indicado arriba.
Para Express/.NET sin Python ni LocalDB, ver
[verificación de servicios y habitaciones](docs/verificacion-windows-servicios.md).
La revisión de permisos incluye dos consultas adicionales (historial y
recuperación de confirmación); sus scripts complementarios no se aplican automáticamente.
