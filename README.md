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
