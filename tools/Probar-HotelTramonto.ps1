#requires -Version 5.1
[CmdletBinding()]
param([switch]$ConfiarCertificadoLocal)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Servidor = 'LAPTOP-5TSJJUP8\SQLEXPRESS'
$Original = 'HotelTramontoDB'
$Pruebas = 'HotelTramontoDB_Pruebas'
$Raiz = Split-Path $PSScriptRoot -Parent
$Salida = Join-Path $env:LOCALAPPDATA ('HotelTramonto\Informes\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
$null = New-Item -ItemType Directory -Path $Salida -Force
$Pasos = New-Object 'System.Collections.Generic.List[object]'
$Conexion = $null
$Maestra = $null
$Confirmado = $false
$Fallo = $null
$AplicacionSql='HotelTramonto-Pruebas-Controladas/' + [Guid]::NewGuid().ToString('N')
$ConexionesPropias=New-Object 'System.Collections.Generic.List[object]'
$ObservacionSesion=0
function Registrar([string]$Nombre, [string]$Detalle) {
    $Pasos.Add([pscustomobject]@{ Prueba=$Nombre; Estado='APROBADA'; Detalle=$Detalle })
    Write-Host ('OK: ' + $Nombre)
}
function Exigir([bool]$Condicion, [string]$Mensaje) { if (-not $Condicion) { throw $Mensaje } }
function Crear-CadenaConexion([string]$Instancia, [string]$Base, [bool]$ConfiarCertificado=$false, [string]$Aplicacion='HotelTramonto-Pruebas-Controladas') {
    $b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    # En PowerShell el adaptador IDictionary puede tratar las propiedades como claves SQL.
    # Usar siempre el indexador con las palabras clave admitidas por System.Data.SqlClient.
    $b['Data Source']=$Instancia
    $b['Initial Catalog']=$Base
    $b['Integrated Security']=$true
    $b['Encrypt']=$true
    $b['TrustServerCertificate']=$ConfiarCertificado
    $b['Persist Security Info']=$false
    $b['Connect Timeout']=15
    $b['Application Name']=$Aplicacion
    $b['Pooling']=$false
    return $b.get_ConnectionString()
}
function Abrir([string]$Base) {
    $cadena=Crear-CadenaConexion $Servidor $Base ([bool]$ConfiarCertificadoLocal) $AplicacionSql
    $c = New-Object -TypeName System.Data.SqlClient.SqlConnection -ArgumentList $cadena
    try {
        $c.Open()
        $t=Tabla $c @'
SELECT s.session_id,CONVERT(varchar(36),c.connection_id) AS connection_id,
       s.program_name,s.host_process_id
FROM sys.dm_exec_sessions s JOIN sys.dm_exec_connections c ON c.session_id=s.session_id
WHERE s.session_id=@@SPID AND c.parent_connection_id IS NULL;
'@
        Exigir ($t.Rows.Count -eq 1) 'No se pudo identificar inequívocamente la conexión propia.'
        $r=$t.Rows[0]
        Exigir (([string]$r.program_name -ceq $AplicacionSql) -and ([int]$r.host_process_id -eq $PID)) 'Identidad de conexión propia inesperada.'
        $ConexionesPropias.Add([pscustomobject]@{session_id=[int]$r.session_id;connection_id=[string]$r.connection_id;program_name=[string]$r.program_name;host_process_id=[int]$r.host_process_id})
        return $c
    } catch { $c.Dispose(); throw }
}
function Consulta([System.Data.SqlClient.SqlConnection]$C, [string]$Sql) {
    $cmd=$C.CreateCommand(); $cmd.CommandText=$Sql; $cmd.CommandTimeout=120
    $adapter=New-Object -TypeName System.Data.SqlClient.SqlDataAdapter -ArgumentList $cmd
    $ds=New-Object System.Data.DataSet
    try { $null=$adapter.Fill($ds); return ,$ds } finally { $adapter.Dispose(); $cmd.Dispose() }
}
function Tabla([System.Data.SqlClient.SqlConnection]$C, [string]$Sql) {
    $ds=Consulta $C $Sql
    Exigir ($ds.Tables.Count -gt 0) 'La consulta no devolvió resultados.'
    return ,$ds.Tables[0]
}
function HuellaArchivo([string]$Archivo) {
    # Git puede convertir LF a CRLF en Windows; validar el contenido, no el fin de línea.
    $texto=[IO.File]::ReadAllText($Archivo).Replace("`r`n","`n").Replace("`r","`n")
    $sha=[Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($texto)))).Replace('-','') }
    finally { $sha.Dispose() }
}
function Contexto {
    $t=Tabla $Conexion "SELECT DB_NAME() AS db, CONVERT(nvarchar(128),SERVERPROPERTY('ServerName')) AS servidor;"
    Exigir (($t.Rows[0].db -ceq $Pruebas) -and ($t.Rows[0].servidor -ieq $Servidor)) 'Contexto SQL inesperado. Detenido.'
}
function Clasificar-Sesiones([object[]]$Sesiones=@(), [object[]]$Registradas=@()) {
    $propias=@(); $ajenas=@()
    foreach ($sesion in $Sesiones) {
        if ($null -eq $sesion) { continue }
        if (($null -ne $sesion.PSObject.Properties['afecta_copia']) -and ($sesion.afecta_copia -eq 0)) { continue }
        $esPropia=$false
        foreach ($registro in $Registradas) {
            # Nunca ignorar por nombre de programa, host o PID solamente.
            if (([string]$sesion.connection_id -ne '') -and
                ([int]$sesion.session_id -eq [int]$registro.session_id) -and
                ([string]$sesion.connection_id -ieq [string]$registro.connection_id) -and
                ([string]$sesion.program_name -ceq [string]$registro.program_name) -and
                ([int]$sesion.host_process_id -eq [int]$registro.host_process_id)) { $esPropia=$true; break }
        }
        if ($esPropia) { $propias+= $sesion } else { $ajenas+= $sesion }
    }
    return [pscustomobject]@{Propias=@($propias);Ajenas=@($ajenas)}
}
function Actualizar-ObservacionesLimpias([int]$Anteriores, [int]$Ajenas) {
    if ($Ajenas -gt 0) { return 0 }
    return ($Anteriores+1)
}
function SinOtrosClientes {
    # Dos observaciones limpias consecutivas; un error SQL no se reintenta ni se ignora.
    $limpias=0
    for ($intento=1; $intento -le 3; $intento++) {
        Contexto
        $t=Tabla $Conexion @'
SELECT s.session_id,s.program_name,s.host_name,ISNULL(s.host_process_id,-1) AS host_process_id,s.status,
       CONVERT(varchar(36),c.connection_id) AS connection_id,
       s.database_id AS session_database_id,r.database_id AS request_database_id,
       1 AS afecta_copia
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_connections c ON c.session_id=s.session_id
LEFT JOIN sys.dm_exec_requests r ON r.session_id=s.session_id
WHERE s.is_user_process=1 AND
      (s.database_id=DB_ID() OR EXISTS
       (SELECT 1 FROM sys.dm_exec_requests q WHERE q.session_id=s.session_id AND q.database_id=DB_ID()))
ORDER BY s.session_id;
'@
        $sesiones=@(foreach ($r in $t.Rows) {
            [pscustomobject]@{session_id=[int]$r.session_id;program_name=[string]$r.program_name;
                host_name=[string]$r.host_name;host_process_id=[int]$r.host_process_id;status=[string]$r.status;
                connection_id=[string]$r.connection_id;session_database_id=$r.session_database_id;
                request_database_id=$r.request_database_id;afecta_copia=1}
        })
        $clasificacion=Clasificar-Sesiones -Sesiones $sesiones -Registradas @($ConexionesPropias.ToArray())
        $script:ObservacionSesion++
        $archivo=Join-Path $Salida ('sesiones-{0:D3}.json' -f $script:ObservacionSesion)
        [pscustomobject]@{Fecha=(Get-Date).ToString('o');Intento=$intento;Propias=$clasificacion.Propias;Ajenas=$clasificacion.Ajenas} |
            ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $archivo -Encoding UTF8
        $limpias=Actualizar-ObservacionesLimpias $limpias $clasificacion.Ajenas.Count
        if ($clasificacion.Ajenas.Count -gt 0) {
            Write-Host ('Sesiones ajenas detectadas. Detalle: '+$archivo)
            $clasificacion.Ajenas | Format-Table session_id,program_name,host_name,host_process_id,status -AutoSize | Out-Host
        }
        if ($limpias -ge 2) { return }
        if ($intento -lt 3) { Start-Sleep -Milliseconds 400 }
    }
    if ($clasificacion.Ajenas.Count -gt 0) {
        $detalle=($clasificacion.Ajenas | ForEach-Object { 'session_id={0}; program_name={1}; host_name={2}; host_process_id={3}' -f $_.session_id,$_.program_name,$_.host_name,$_.host_process_id }) -join ' | '
        throw ('Hay sesiones ajenas en la copia. No se migra. '+$detalle+'. Revisar '+$archivo)
    }
    throw ('Sesiones transitorias/inestables: no hubo dos observaciones limpias consecutivas. No se migra. Revisar '+$archivo)
}
function EjecutarArchivo([string]$Relativo) {
    Exigir ($ScriptsAutorizados -ccontains $Relativo) 'Archivo SQL fuera de la lista autorizada.'
    Contexto
    SinOtrosClientes
    $path=Join-Path $Raiz $Relativo
    $sql=[IO.File]::ReadAllText($path)
    $numero=0
    foreach ($lote in [regex]::Split($sql,'(?im)^GO\s*$')) {
        if ([string]::IsNullOrWhiteSpace($lote)) { continue }
        Contexto
        SinOtrosClientes
        $numero++
        $ds=Consulta $Conexion $lote
        $nombre=([IO.Path]::GetFileName($path)) + '-' + $numero + '-' + $script:NumeroArchivo + '.xml'
        $ds.WriteXml((Join-Path $Salida $nombre),[System.Data.XmlWriteMode]::WriteSchema)
    }
    $script:NumeroArchivo++
    Registrar $Relativo ('Lotes completados: ' + $numero)
}
function Huellas {
    Contexto
    $tablas=Tabla $Conexion "SELECT t.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE s.name='dbo' AND t.is_ms_shipped=0 ORDER BY t.name;"
    $resultado=@{}
    foreach ($fila in $tablas.Rows) {
        $nombre=[string]$fila.name
        $obj='dbo.['+$nombre.Replace(']',']]')+']'
        $literal=$obj.Replace("'","''")
        $columnas=Tabla $Conexion ("SELECT name FROM sys.columns WHERE object_id=OBJECT_ID(N'" + $literal + "') AND NOT (name=N'categoria' AND OBJECT_NAME(object_id)=N'Servicios') ORDER BY column_id;")
        $pk=Tabla $Conexion ("SELECT c.name FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id WHERE i.object_id=OBJECT_ID(N'" + $literal + "') AND i.is_primary_key=1 ORDER BY ic.key_ordinal;")
        Exigir ($pk.Rows.Count -gt 0) ('Falta clave primaria para comparar datos: '+$nombre)
        $cols=(@($columnas.Rows | ForEach-Object { '['+([string]$_.name).Replace(']',']]')+']' })) -join ','
        $orden=(@($pk.Rows | ForEach-Object { '['+([string]$_.name).Replace(']',']]')+']' })) -join ','
        $sql="SELECT COUNT_BIG(*) AS cantidad, CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),(SELECT $cols FROM $obj ORDER BY $orden FOR JSON PATH, INCLUDE_NULL_VALUES))),2) AS huella FROM $obj;"
        $t=Tabla $Conexion $sql
        $resultado[$nombre]=([string]$t.Rows[0].cantidad)+':'+([string]$t.Rows[0].huella)
    }
    return $resultado
}
function Canon([string]$Texto) {
    $Texto=[regex]::Replace($Texto,'(?s)/\*.*?\*/|--[^\r\n]*','')
    $Texto=$Texto.Replace('[','').Replace(']','')
    $Texto=[regex]::Replace($Texto,'(?i)\bdbo\.','')
    $Texto=[regex]::Replace($Texto,'(?i)\b(?:CREATE\s+OR\s+ALTER|CREATE|ALTER)\s+PROCEDURE','PROCEDURE')
    return ([regex]::Replace($Texto,'\s+|;','')).ToUpperInvariant()
}
function Versiones([string]$Archivo) {
    $map=@{}
    $texto=[IO.File]::ReadAllText((Join-Path $Raiz $Archivo))
    foreach ($m in [regex]::Matches($texto,"(?s)EXEC\(N'((?:''|[^'])*)'\)")) {
        $def=$m.Groups[1].Value.Replace("''", "'")
        $n=[regex]::Match($def,'(?i)(?:CREATE(?:\s+OR\s+ALTER)?|ALTER) PROCEDURE dbo\.(\w+)')
        if ($n.Success) { $map[$n.Groups[1].Value]=Canon $def }
    }
    return $map
}
function DotNet([string[]]$Argumentos, [string]$Log) {
    & dotnet @Argumentos 2>&1 | Out-File (Join-Path $Salida $Log) -Encoding utf8
    if ($LASTEXITCODE -ne 0) { throw ('dotnet falló; revisar '+$Log) }
}
$NumeroArchivo=0
try {
    Exigir ($env:OS -eq 'Windows_NT') 'Ejecutar en Windows con Windows PowerShell 5.1.'
    Add-Type -AssemblyName System.Data
    $null=Get-Command dotnet -ErrorAction Stop
    $Manifest=@{
        'tools/Probar-SesionesSql.ps1'='6D4FF42FFDD13BBBFE4A43AD42905EA7C6A05295A915C6F426817904F5A2440A'
        'tools/Probar-LecturasSql.ps1'='33FD5474D7EC4B99E34DD4A030546F803300EE62151F620D9A8D56E54AF713B6'
        'tools/Probar-CadenaConexion.ps1'='C5C2E5C64C637F2FC4F8E03F4BDB1C501E3E0166A9F0A343C55F1E79D2153C0C'
        'HotelTramonto.Datos/ConexionBD.cs'='AFEC5A33A1757C3A34081983CC2A2F622C4E10F129A2BB72E1D987F879E26D29'
        'HotelTramonto.Datos/DetalleReservaDatos.cs'='2FE2BBA8F285392190EEE4F9E2C3C028F5BD4BCE815B9D37F63F57F67E13CF4E'
        'HotelTramonto.Datos/DetalleServicioDatos.cs'='40393E0CFE800E5192D3CAD6866CF04D7A391B6FCA0E07A079C747AE84C2C406'
        'HotelTramonto.Datos/HabitacionDatos.cs'='76B34C24FA834D526FFE9705E469AB06740614F4F9768B2EBC4DDDC597A2C3F2'
        'HotelTramonto.Datos/HotelTramonto.Datos.csproj'='1CC3B1B2C6F273E5D164D57EE882EC45F61253023E3499CE9FC964EE5E4B872E'
        'HotelTramonto.Datos/HuespedDatos.cs'='E7621F710B3CD5189B58FDD1103F3CBD8ABB59A6DB52708B66A2B48851517736'
        'HotelTramonto.Datos/IHabitacionDatos.cs'='9E3E7C53294293A324AC63C3226F1370FC51B7B9AA16C3B9493E93882A1F762A'
        'HotelTramonto.Datos/ITipoHabitacionDatos.cs'='AB86774F0A578DBDFCAE310F2D9E58FD29FA2277E25A2019D86D9381754734A7'
        'HotelTramonto.Datos/PagoDatos.cs'='006A43C270C42FA028B84E1E1631E7EA2B9F0CCC84ADA22B31DD84491072DF8F'
        'HotelTramonto.Datos/ReporteDatos.cs'='9F97362B8D2812FCCBD3C1B7EDC9F80C116086F0FF12FFD60CAE45AA540D2F6F'
        'HotelTramonto.Datos/ReservaDatos.cs'='A63EF859ED9EEA025705E9DC8A697D4B46A89BAB8430146B5A1D7C015357BFE0'
        'HotelTramonto.Datos/ReservaTemporalDatos.cs'='E8949DAA50BBDD2A2FB261AB340DBA3818A66712D3B921920406A7F0C87B1025'
        'HotelTramonto.Datos/RolDatos.cs'='F5E5226644B5E0312D6820EBDCD5C63F3488C5FE46FD419535EAA278A8067F78'
        'HotelTramonto.Datos/ServicioDatos.cs'='D17348A5F2EC293E559FB1BA8B9CBD68AA292F4D484A1962E8C2616135EBB2AE'
        'HotelTramonto.Datos/TipoHabitacionDatos.cs'='EE4F9B69B4D6839836A30702AE304831406DC156248F529BEAA619F12F676136'
        'HotelTramonto.Datos/UsuarioDatos.cs'='1856802409682B860D33428EF492CDC5862EF147A4D9F3DF4156EA9D9E431441'
        'HotelTramonto.Entidades/Entidades/DetalleReserva.cs'='37B2FDEAD7C6BFA105EFFE01D1479B541CDA1B5BA2514B55538A19BB2780AFFF'
        'HotelTramonto.Entidades/Entidades/DetalleServicio.cs'='72A0F83C940E49451994BD9B9B75035FC3A594AAA812F9F4CEED8C689798A4A1'
        'HotelTramonto.Entidades/Entidades/Habitacion.cs'='0ABEF8B1B7C10FD756229C1E48BDC5CE61086FE051BECA92DAE2CAB3A5065E60'
        'HotelTramonto.Entidades/Entidades/Huesped.cs'='C72310E964FA078C5BF44AD8F080C429085C91FABDA30ED0966A46738B9EF720'
        'HotelTramonto.Entidades/Entidades/Pago.cs'='235560D9629799C7FBBB1FBD84050A6FA67D3F36E6C670A02C76D1FCE095325B'
        'HotelTramonto.Entidades/Entidades/Reserva.cs'='B683FE37215304F0091BB4DE143C88E28C7E584C48B8DDDE2170C745C15E7F21'
        'HotelTramonto.Entidades/Entidades/ReservaTemporal.cs'='A0E8B2C96950195CF02EE2EEB112D312B36D8D460EFDD2B328E4084D43C9D4EF'
        'HotelTramonto.Entidades/Entidades/Rol.cs'='50DA547AC2D629E71647C62C718FC3F5633F6D1A38AD210328EC8195BFD1672B'
        'HotelTramonto.Entidades/Entidades/Servicio.cs'='9519AD5E9F0006D2902EC6C6C0CB14D7D572AA11A5548C7338BF2E5EAC862746'
        'HotelTramonto.Entidades/Entidades/TipoHabitacion.cs'='BA8F6628C976D2EAADD2BF3CC9E48F7031833805A9BC3DD11D3B3297390474A9'
        'HotelTramonto.Entidades/Entidades/Usuario.cs'='891101CD79D1F3CBA8FBF501DFC06A662F962C29E3DFD4CF630BAD1EC46CCA3B'
        'HotelTramonto.Entidades/HotelTramonto.Entidades.csproj'='2C82DAE7A6492E5DC0D99B6B5A5D1C89A4702B892F71757981E46302949D6115'
        'HotelTramonto.Negocios/DetalleServicioNegocios.cs'='44CF0CCCAB43B87439F1340F51E776BE2818CE4C4C0514D990D78236E617C747'
        'HotelTramonto.Negocios/HabitacionNegocios.cs'='287EE8FA58B454FF69A54AABF64C0DA6E53B988FDFE881FF44C4D1E36864DDF7'
        'HotelTramonto.Negocios/HotelTramonto.Negocios.csproj'='907A90BAEA0C27DE59035EDA2A37924F52AF24BD2E3635A7AB39D203A194033D'
        'HotelTramonto.Negocios/HuespedNegocios.cs'='968B3C8BC6A47E7B64588EA1AF6BA782CB916089D8656E0C9E559CA411C52558'
        'HotelTramonto.Negocios/PagoNegocios.cs'='AE6864FF81DEA57DAA136A652BCE41AADEEBA9C5B6245A87737EE1A39F43D6DB'
        'HotelTramonto.Negocios/PasswordService.cs'='83C310101CFB7A6ECA4462EAE07586EF9892276352D197A1E894FE0D9FFCBC93'
        'HotelTramonto.Negocios/Permisos.cs'='C0AF02401728F12CEFD331A466E9946EE2E590E25CDE6C98F0CCC5E61F4E16D7'
        'HotelTramonto.Negocios/PermisosHabitaciones.cs'='04385D8A0AF3A1E00E723A2047A643F58CCC94677624970864D71C12689413C5'
        'HotelTramonto.Negocios/ReporteNegocios.cs'='8BB00226B24FF73A696C799F1A7C74064D58A5880F4DA0F1B0547D47C0D3A30E'
        'HotelTramonto.Negocios/ReservaNegocios.cs'='EA6315BE589A3170FF6B336A49201F77642192C01C00FDA740D4A058AF048D5D'
        'HotelTramonto.Negocios/ReservaTemporalNegocios.cs'='289C89862FDC1CEA4CD987227E4A48DE8F7A77597F476638C619B21C28CBFF31'
        'HotelTramonto.Negocios/ServicioNegocios.cs'='5931B64C106F1B7C162289DD5FBF1BE4AEEF90EF1F9F67D6DD06C4E1E9938FD2'
        'HotelTramonto.Negocios/TipoHabitacionNegocios.cs'='09429E276833B42E817EBBC25BB48EE331FE6799BE99E366BF189548BA51CF74'
        'HotelTramonto.Negocios/UsuarioNegocios.cs'='C72644003C4216A763F4AE172C2B52B448302279D09560D24FBDF73462B8CBC9'
        'HotelTramonto.Negocios/Validaciones.cs'='B8AEBFF1195EC1E26F87A0FC48AB2659E0D97AA032831569BC6EA7C4A8C3C0C4'
        'HotelTramonto.Presentacion/HotelTramonto.Presentacion.csproj'='ECFCC9BA165CABF5F00D02C25B9FF7C9081A851ECC5416DEEAA0ED8DA4AF77A0'
        'HotelTramonto.Pruebas/ConfiguracionLocalTests.cs'='F662B42D0381AAD095CB2025B77BEA2B733ED8148415E6C2E6EAFCE5D36A4D97'
        'HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj'='90F58262497E9FE4D0B2A5186578CA8D926D1A19E7376232F0F80DCC1D9F7D28'
        'HotelTramonto.Pruebas/MSTestSettings.cs'='72F34577D30D2F5C78B3C50D2A2D898B52ACCA841CA165B68E352462F8FE525D'
        'HotelTramonto.Pruebas/PermisosHabitacionesTests.cs'='2094C66401D9EED7DBA43BD203C361AA7631FD92EA47EFD20CB56C7EAC72D153'
        'HotelTramonto.Pruebas/Test1.cs'='4552B893873D4957606737134B326772E10405442E651E2DCFE40BB797984BF5'
        'HotelTramonto.slnx'='38DF1D0077777EA6791B0426B1A31CF069A9F6CDA5FF03703C15FCC144A97836'
        'base de datos HotelTramonto/Procedimientos_Almacenados.sql'='6AC5C5C431DBEFFF7622EB44598D6DC49F0109DB47CE908C85B05FE1CB42120F'
        'base de datos HotelTramonto/compatibilidad-servicios/00_DiagnosticoYRespaldo.sql'='F77089D67BE7DC8CD62747ABC76BCC5B637C2EA23B9270D368FDC950E94A9ADA'
        'base de datos HotelTramonto/compatibilidad-servicios/01_AgregarCategoria.sql'='00C5558A8740532C03FC7DE88D993DAAB21DA580D7E9EE69E42F91C413A047CB'
        'base de datos HotelTramonto/compatibilidad-servicios/02_ClasificarServicios.sql'='C7C9471A6FE33C28DE1ED61CA3C3CDCB626DE344F47FB7A33BD88BA0706A5BE6'
        'base de datos HotelTramonto/compatibilidad-servicios/03_ActualizarProcedimientos.sql'='2D6D9B2C2D8CE691157BAF1EAA40AED14D503E96711BD82A2EE15145B3AA7DFC'
        'base de datos HotelTramonto/compatibilidad-servicios/04_Comprobaciones.sql'='7AE4E34EBB8C5AD315D1BF2C4F88D36F579EECAAC89B4ADAA9CE9D1AD46EE58A'
        'base de datos HotelTramonto/permisos-habitaciones/00_Diagnostico.sql'='1D93EC24176BC0F68867771EA0AC4CBC896C2E524F60BC7DEEE6808EDCBBF2BB'
        'base de datos HotelTramonto/permisos-habitaciones/01_ProcedimientosFaltantes.sql'='A067BB173009352B21C6AF9BE3C9C9EC822464AA9775962A48424915B8E7B5E3'
    }
    $ScriptsAutorizados=@(
        'base de datos HotelTramonto/compatibilidad-servicios/00_DiagnosticoYRespaldo.sql',
        'base de datos HotelTramonto/compatibilidad-servicios/01_AgregarCategoria.sql',
        'base de datos HotelTramonto/compatibilidad-servicios/02_ClasificarServicios.sql',
        'base de datos HotelTramonto/compatibilidad-servicios/03_ActualizarProcedimientos.sql',
        'base de datos HotelTramonto/compatibilidad-servicios/04_Comprobaciones.sql',
        'base de datos HotelTramonto/permisos-habitaciones/00_Diagnostico.sql',
        'base de datos HotelTramonto/permisos-habitaciones/01_ProcedimientosFaltantes.sql'
    )
    $FiltroSeguro='FullyQualifiedName=HotelTramonto.Pruebas.Test1.TextoObligatorio_ConTextoValido_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.TextoObligatorio_ConTextoVacio_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.FechasReservaValidas_SalidaPosterior_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.FechasReservaValidas_SalidaAnterior_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.DNIValido_ConSoloNumeros_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.DNIValido_ConLetras_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.EmailValido_ConFormatoCorrecto_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.EmailValido_SinArroba_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.MontoValido_ConMontoPositivo_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.MontoValido_ConCero_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.DNIValido_ConMenosDeSieteDigitos_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.DNIValido_ConMasDeQuinceDigitos_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.EmailValido_ConTextoVacio_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.EmailValido_SinDominioValido_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.MontoValido_ConMontoNegativo_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.TextoObligatorio_ConTextoNulo_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.HuespedValido_ConDatosCorrectos_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.HuespedValido_SinNombre_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.HuespedValido_ConDNIInvalido_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.HuespedValido_SinEmail_RetornaTrue|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_BuscarPorDNIInvalido_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_InsertarConEmailInvalido_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_ModificarConIdInvalido_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_DesactivarConIdInvalido_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_CambiarContrasenaVacia_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_InsertarConDNIInvalido_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_InsertarSinNombre_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.UsuarioNegocios_InsertarSinApellido_LanzaExcepcion|FullyQualifiedName=HotelTramonto.Pruebas.Test1.PasswordService_Hash_GeneraHashDiferente|FullyQualifiedName=HotelTramonto.Pruebas.Test1.PasswordService_Verificar_ValidaContrasena|FullyQualifiedName=HotelTramonto.Pruebas.Test1.PasswordService_VerificarContrasenaIncorrecta_RetornaFalse|FullyQualifiedName=HotelTramonto.Pruebas.Test1.Permisos_Recepcionista_NoPuedeGestionarUsuarios|FullyQualifiedName=HotelTramonto.Pruebas.Test1.Permisos_Gerente_NoPuedeGestionarUsuarios|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.ConfiguracionLocal_UsaInstanciaYAutenticacionWindowsSinAbrirConexion|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.ConfiguracionLocal_PermiteCertificadoLocalSoloExplicitamente|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.ConfiguracionLocal_NoReinterpretaServidorComoCadenaDeConexion|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.ConfiguracionLocal_ArchivoAusenteNoRevelaRuta|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.ConfiguracionLocal_RechazaArchivosInvalidos|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.UsuarioNegocios_RechazaSieteCaracteresAntesDeAccederASql|FullyQualifiedName=HotelTramonto.Pruebas.ConfiguracionLocalTests.UsuarioDatos_RechazaLongitudInsuficienteAntesDeAccederASql|FullyQualifiedName=HotelTramonto.Pruebas.PermisosHabitacionesTests.NoAdministradores_NoLleganADatosEnNingunaOperacion|FullyQualifiedName=HotelTramonto.Pruebas.PermisosHabitacionesTests.Administrador_ConservaLasCuatroOperacionesYLosParametros|FullyQualifiedName=HotelTramonto.Pruebas.PermisosHabitacionesTests.Administrador_PuedeAdministrarTiposSinOmitirValidaciones|FullyQualifiedName=HotelTramonto.Pruebas.PermisosHabitacionesTests.Administrador_HabitacionesConservaValidacionDeDatos|FullyQualifiedName=HotelTramonto.Pruebas.PermisosHabitacionesTests.Consultas_NoRequierenPermisoAdministrativoYConservanResultados'
    foreach ($item in $Manifest.GetEnumerator()) {
        $archivo=Join-Path $Raiz $item.Key
        Exigir (Test-Path -LiteralPath $archivo -PathType Leaf) ('Falta archivo: '+$item.Key)
        Exigir ((HuellaArchivo $archivo) -eq $item.Value) ('Archivo modificado/no revisado: '+$item.Key)
    }
    Registrar 'Integridad de scripts y pruebas' 'Lista cerrada; no se permite ejecutar archivos arbitrarios.'
    & (Join-Path $PSScriptRoot 'Probar-CadenaConexion.ps1') | Out-File -LiteralPath (Join-Path $Salida 'cadena-conexion.log') -Encoding utf8
    Registrar 'Construcción de cadena SQL' 'Cuatro combinaciones y nueve claves verificadas sin abrir conexiones.'
    & (Join-Path $PSScriptRoot 'Probar-LecturasSql.ps1') | Out-File -LiteralPath (Join-Path $Salida 'lecturas-sql.log') -Encoding utf8
    Registrar 'Referencias de lecturas SQL' 'Filtro de habitaciones activas y disponibilidad verificados sin SQL.'
    & (Join-Path $PSScriptRoot 'Probar-SesionesSql.ps1') | Out-File -LiteralPath (Join-Path $Salida 'deteccion-sesiones.log') -Encoding utf8
    Registrar 'Lógica de detección de sesiones' 'Quince casos puros aprobados antes de abrir SQL.'
    $Maestra=Abrir 'master' # solo SELECT de metadatos, nunca comandos de escritura
    $t=Tabla $Maestra "SELECT CONVERT(nvarchar(128),SERVERPROPERTY('ServerName')) AS servidor, CONVERT(int,SERVERPROPERTY('ProductMajorVersion')) AS version;"
    Exigir ($t.Rows[0].servidor -ieq $Servidor) ('Instancia inesperada: '+$t.Rows[0].servidor)
    Exigir ([int]$t.Rows[0].version -ge 13) 'Se requiere SQL Server 2016 o posterior para las huellas JSON/SHA256.'
    $bases=Tabla $Maestra "SELECT database_id,name,state_desc,is_read_only,source_database_id FROM sys.databases WHERE name IN (N'HotelTramontoDB',N'HotelTramontoDB_Pruebas');"
    Exigir ($bases.Rows.Count -eq 2) 'Deben existir ambas bases y ser visibles para el usuario.'
    $o=@($bases.Rows | Where-Object { $_.name -ceq $Original })
    $p=@($bases.Rows | Where-Object { $_.name -ceq $Pruebas })
    Exigir (($o.Count -eq 1) -and ($p.Count -eq 1)) 'Nombres de bases inesperados.'
    Exigir ([int]$o[0].database_id -ne [int]$p[0].database_id) 'Las bases no son diferentes.'
    Exigir (($p[0].state_desc -eq 'ONLINE') -and (-not [bool]$p[0].is_read_only) -and ($p[0].source_database_id -is [DBNull])) 'La copia debe ser una base independiente ONLINE y escribible, no un snapshot.'
    $files=Tabla $Maestra "SELECT database_id,type_desc,physical_name FROM sys.master_files WHERE database_id IN (DB_ID(N'HotelTramontoDB'),DB_ID(N'HotelTramontoDB_Pruebas'));"
    $fo=@($files.Rows | Where-Object { $_.database_id -eq $o[0].database_id })
    $fp=@($files.Rows | Where-Object { $_.database_id -eq $p[0].database_id })
    Exigir (($fo.Count -ge 2) -and ($fp.Count -ge 2)) 'Metadatos de archivos insuficientes.'
    Exigir ((@($fp | Where-Object { $_.type_desc -eq 'ROWS' }).Count -gt 0) -and (@($fp | Where-Object { $_.type_desc -eq 'LOG' }).Count -gt 0)) 'La copia requiere archivos de datos y log.'
    foreach ($f in $fp) { Exigir (-not (@($fo.physical_name) -icontains $f.physical_name)) 'La copia comparte una ruta de archivo con la original.' }
    Registrar 'Instancia, bases y archivos independientes' 'Solo metadatos leídos en master; ninguna conexión de escritura a la original.'
    $permisoDmv=if ([int]$t.Rows[0].version -ge 16) { 'VIEW SERVER PERFORMANCE STATE' } else { 'VIEW SERVER STATE' }
    $visibilidad=Tabla $Maestra ("SELECT HAS_PERMS_BY_NAME(NULL,NULL,'"+$permisoDmv+"') AS permitido;")
    Exigir ($visibilidad.Rows[0].permitido -eq 1) 'Sin visibilidad completa de sesiones; no se puede verificar exclusividad.'
    $Conexion=Abrir $Pruebas
    Contexto
    SinOtrosClientes
    $tr=Tabla $Maestra 'SELECT COUNT(*) AS cantidad FROM sys.server_triggers WHERE is_disabled=0;'
    Exigir ([int]$tr.Rows[0].cantidad -eq 0) 'Hay triggers de servidor habilitados: requieren revisión antes de DDL.'
    $tr=Tabla $Conexion 'SELECT COUNT(*) AS cantidad FROM sys.triggers WHERE is_disabled=0;'
    Exigir ([int]$tr.Rows[0].cantidad -eq 0) 'Hay triggers habilitados en la copia: requieren revisión antes de migrar.'
    $per=Tabla $Maestra "SELECT HAS_PERMS_BY_NAME(NULL,NULL,'VIEW ANY DEFINITION') AS puede_ver;"
    Exigir ($per.Rows[0].puede_ver -eq 1) 'Falta visibilidad suficiente para verificar triggers/metadatos de servidor.'
    $per=Tabla $Conexion "SELECT HAS_PERMS_BY_NAME(DB_NAME(),'DATABASE','VIEW DEFINITION') AS puede_ver;"
    Exigir ($per.Rows[0].puede_ver -eq 1) 'Falta VIEW DEFINITION en la copia; no se puede diagnosticar con seguridad.'
    EjecutarArchivo 'base de datos HotelTramonto/compatibilidad-servicios/00_DiagnosticoYRespaldo.sql'
    EjecutarArchivo 'base de datos HotelTramonto/permisos-habitaciones/00_Diagnostico.sql'
    $serv=Tabla $Conexion 'SELECT id_servicio,nombre,descripcion,precio,activo FROM dbo.Servicios;'
    Exigir ($serv.Rows.Count -eq 1) 'Se esperaba únicamente el servicio ID 1. Revisar la clasificación.'
    $s=$serv.Rows[0]
    Exigir (($s.id_servicio -eq 1) -and ($s.nombre -ceq 'Desayuno Buffet') -and ($s.descripcion -ceq 'Servicio de desayuno buffet') -and ([decimal]$s.precio -eq 9500) -and (-not [bool]$s.activo)) 'El servicio no coincide con el escenario confirmado.'
    $col=Tabla $Conexion "SELECT name,system_type_id,max_length,is_nullable FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Servicios') AND name='categoria';"
    if ($col.Rows.Count -gt 0) {
        Exigir (($col.Rows[0].system_type_id -eq 167) -and ($col.Rows[0].max_length -eq 30) -and [bool]$col.Rows[0].is_nullable) 'categoria existente no es VARCHAR(30) NULL.'
        $cat=Tabla $Conexion 'SELECT categoria FROM dbo.Servicios WHERE id_servicio=1;'
        Exigir (($cat.Rows[0].categoria -is [DBNull]) -or ($cat.Rows[0].categoria -ceq 'Otros')) 'La categoría existente difiere de Otros.'
    }
    $old=Versiones 'base de datos HotelTramonto/compatibilidad-servicios/05_RevertirProcedimientos.sql'
    $new=Versiones 'base de datos HotelTramonto/compatibilidad-servicios/03_ActualizarProcedimientos.sql'
    Exigir (($old.Count -eq 3) -and ($new.Count -eq 3)) 'Referencias de procedimientos incompletas.'
    foreach ($nombre in $old.Keys) {
        $defs=Tabla $Conexion ("SELECT m.definition,m.execute_as_principal_id FROM sys.sql_modules m WHERE object_id=OBJECT_ID(N'dbo."+$nombre+"');")
        Exigir (($defs.Rows.Count -eq 1) -and ($defs.Rows[0].definition -isnot [DBNull])) ('Definición no visible: '+$nombre)
        $actual=Canon ([string]$defs.Rows[0].definition)
        Exigir (($actual -ceq $old[$nombre]) -or ($actual -ceq $new[$nombre])) ('Procedimiento distinto del revisado: '+$nombre)
        Exigir ($defs.Rows[0].execute_as_principal_id -is [DBNull]) ('EXECUTE AS inesperado: '+$nombre)
    }
    $sign=Tabla $Conexion "SELECT COUNT(*) AS cantidad FROM sys.crypt_properties WHERE class=1 AND major_id IN (OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar'),OBJECT_ID(N'dbo.sp_Servicio_Listar'));"
    Exigir ($sign.Rows[0].cantidad -eq 0) 'Procedimientos firmados: revisar con el DBA.'
    # Requisitos de esquema de los cinco procedimientos, antes de cualquier modificación.
    $req=Tabla $Conexion @'
SELECT t.tabla,t.columna FROM (VALUES
('Habitaciones','id_habitacion'),('Habitaciones','tipo_habitacion'),('Habitaciones','piso'),
('Habitaciones','numero'),('Habitaciones','estado'),('Habitaciones','activo'),
('TiposHabitacion','id_tipoHabitacion'),('TiposHabitacion','nombre'),
('Servicios','id_servicio'),('Servicios','nombre'),('Servicios','descripcion'),('Servicios','precio'),('Servicios','activo'),
('Usuarios','id_usuario'),('Usuarios','activo')) t(tabla,columna)
WHERE OBJECT_ID(N'dbo.'+t.tabla,N'U') IS NULL OR COL_LENGTH(N'dbo.'+t.tabla,t.columna) IS NULL;
'@
    Exigir ($req.Rows.Count -eq 0) 'Faltan tablas/columnas requeridas; revisar diagnóstico.'
    $faltantesEsperados=Versiones 'base de datos HotelTramonto/permisos-habitaciones/01_ProcedimientosFaltantes.sql'
    Exigir ($faltantesEsperados.Count -eq 5) 'Referencias de procedimientos faltantes incompletas.'
    foreach ($nombre in $faltantesEsperados.Keys) {
        $t=Tabla $Conexion ("SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo."+$nombre+"',N'P')) AS definicion;")
        if ($t.Rows[0].definicion -isnot [DBNull]) {
            Exigir ((Canon ([string]$t.Rows[0].definicion)) -ceq $faltantesEsperados[$nombre]) ('Procedimiento existente distinto: '+$nombre)
        } else {
            $obj=Tabla $Conexion ("SELECT OBJECT_ID(N'dbo."+$nombre+"') AS id;")
            Exigir ($obj.Rows[0].id -is [DBNull]) ('Objeto incompatible/no visible: '+$nombre)
        }
    }
    # Solo se ejecutarán lecturas cuyas definiciones coincidan con las revisadas.
    $fuente=[IO.File]::ReadAllText((Join-Path $Raiz 'base de datos HotelTramonto/Procedimientos_Almacenados.sql'))
    foreach ($nombre in @('sp_Habitacion_Listar','sp_Habitacion_ConsultarDisponibilidad')) {
        $patron='(?ims)^CREATE OR ALTER PROCEDURE '+[regex]::Escape($nombre)+'\b.*?(?=^GO\s*$)'
        $esperado=[regex]::Match($fuente,$patron)
        Exigir $esperado.Success ('Definición de referencia no encontrada: '+$nombre)
        $t=Tabla $Conexion ("SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo."+$nombre+"',N'P')) AS definicion;")
        Exigir (($t.Rows[0].definicion -isnot [DBNull]) -and ((Canon ([string]$t.Rows[0].definicion)) -ceq (Canon $esperado.Value))) ('Lectura SQL diferente de la revisada: '+$nombre)
    }
    $tipos=Tabla $Conexion @'
SELECT r.tabla,r.columna FROM (VALUES
('Servicios','id_servicio','int',4,0),('Servicios','nombre','varchar',50,0),
('Servicios','descripcion','varchar',150,1),('Servicios','precio','decimal',9,0),('Servicios','activo','bit',1,0),
('Habitaciones','id_habitacion','int',4,0),('Habitaciones','tipo_habitacion','int',4,0),
('Habitaciones','activo','bit',1,0),('Usuarios','id_usuario','int',4,0),('Usuarios','activo','bit',1,0))
r(tabla,columna,tipo,longitud,nullable)
LEFT JOIN sys.columns c ON c.object_id=OBJECT_ID(N'dbo.'+r.tabla) AND c.name=r.columna
WHERE c.column_id IS NULL OR TYPE_NAME(c.user_type_id)<>r.tipo OR c.max_length<>r.longitud
 OR c.is_nullable<>r.nullable OR c.is_computed=1 OR (r.columna='precio' AND (c.precision<>10 OR c.scale<>2));
'@
    Exigir ($tipos.Rows.Count -eq 0) 'Tipos, nulabilidad o columnas calculadas diferentes de los contratos revisados.'
    $previas=Huellas
    Registrar 'Diagnóstico previo y huellas de datos' 'Todas las tablas dbo comparables; no se guardan filas ni hashes de usuarios.'
    Push-Location $Raiz
    try {
        DotNet -Argumentos @('restore','HotelTramonto.slnx') -Log 'restore.log'
        DotNet -Argumentos @('build','HotelTramonto.slnx','--no-restore') -Log 'build.log'
        $configAnterior=$env:HOTELTRAMONTO_CONFIG
        $env:HOTELTRAMONTO_CONFIG=Join-Path $Salida 'conexion-bloqueada-inexistente.json'
        Exigir (-not (Test-Path -LiteralPath $env:HOTELTRAMONTO_CONFIG)) 'La configuración de bloqueo no debe existir.'
        try {
        DotNet -Argumentos @('test','HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj','--no-build','--no-restore','--filter',$FiltroSeguro,'--logger','trx;LogFileName=seguras.trx','--results-directory',$Salida) -Log 'tests.log'
        } finally { $env:HOTELTRAMONTO_CONFIG=$configAnterior }
    } finally { Pop-Location }
    [xml]$trx=Get-Content -LiteralPath (Join-Path $Salida 'seguras.trx') -Raw
    $cuenta=$trx.SelectSingleNode("//*[local-name()='Counters']")
    Exigir (($null -ne $cuenta) -and ([int]$cuenta.total -eq 56) -and ([int]$cuenta.passed -eq 56) -and ([int]$cuenta.failed -eq 0)) 'Las 56 pruebas seguras no completaron satisfactoriamente.'
    Registrar 'Compilación y pruebas sin SQL' '56 casos aprobados. Lista positiva de métodos revisados.'
    Write-Host "Se modificará SOLO $Pruebas en $Servidor. No se instalarán permisos ni se escribirán usuarios/reservas/pagos."
    Write-Host 'Confirmá antes que tenés un respaldo de la copia y que no habrá otras conexiones durante la prueba.'
    $respuesta=Read-Host 'Escribí exactamente MIGRAR HotelTramontoDB_Pruebas para continuar'
    Exigir ($respuesta -ceq 'MIGRAR HotelTramontoDB_Pruebas') 'Cancelado: no se iniciaron modificaciones SQL.'
    $Confirmado=$true
    Contexto
    SinOtrosClientes
    foreach ($n in @('01_AgregarCategoria.sql','02_ClasificarServicios.sql','03_ActualizarProcedimientos.sql','04_Comprobaciones.sql')) {
        EjecutarArchivo ('base de datos HotelTramonto/compatibilidad-servicios/'+$n)
    }
    EjecutarArchivo 'base de datos HotelTramonto/permisos-habitaciones/01_ProcedimientosFaltantes.sql'
    EjecutarArchivo 'base de datos HotelTramonto/compatibilidad-servicios/04_Comprobaciones.sql'
    $despues=Huellas
    Exigir ($previas.Count -eq $despues.Count) 'Cambió el conjunto de tablas.'
    foreach ($n in $previas.Keys) { Exigir ($despues.ContainsKey($n) -and ($previas[$n] -ceq $despues[$n])) ('Cambios no previstos en datos: '+$n) }
    $t=Tabla $Conexion "SELECT id_servicio,nombre,descripcion,precio,activo,categoria FROM dbo.Servicios;"
    Exigir (($t.Rows.Count -eq 1) -and ($t.Rows[0].categoria -ceq 'Otros')) 'Clasificación final inesperada.'
    Registrar 'Preservación de datos y clasificación' 'SHA256 antes/después de todas las tablas dbo coincide, excluyendo únicamente Servicios.categoria; ID 1 = Otros.'
    foreach ($nombre in $new.Keys) {
        $t=Tabla $Conexion ("SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo."+$nombre+"')) AS definicion;")
        Exigir ((Canon ([string]$t.Rows[0].definicion)) -ceq $new[$nombre]) ('Contrato instalado inesperado: '+$nombre)
    }
    foreach ($nombre in $faltantesEsperados.Keys) {
        $t=Tabla $Conexion ("SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo."+$nombre+"',N'P')) AS definicion;")
        Exigir ((Canon ([string]$t.Rows[0].definicion)) -ceq $faltantesEsperados[$nombre]) ('Procedimiento faltante instalado distinto: '+$nombre)
        $t=Tabla $Conexion ("SELECT name,TYPE_NAME(user_type_id) AS tipo FROM sys.parameters WHERE object_id=OBJECT_ID(N'dbo."+$nombre+"');")
        $esperado=@{sp_Habitacion_Reactivar='@id_habitacion';sp_Servicio_Reactivar='@id_servicio';sp_Usuario_Reactivar='@id_usuario'}
        if ($esperado.ContainsKey($nombre)) {
            Exigir (($t.Rows.Count -eq 1) -and ($t.Rows[0].name -ceq $esperado[$nombre]) -and ($t.Rows[0].tipo -eq 'int')) ('Parámetros incompatibles: '+$nombre)
        } else { Exigir ($t.Rows.Count -eq 0) ('Parámetros inesperados: '+$nombre) }
    }
    $param=Tabla $Conexion "SELECT OBJECT_NAME(object_id) AS procedimiento,name,TYPE_NAME(user_type_id) AS tipo,max_length FROM sys.parameters WHERE object_id IN (OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar')) AND name='@categoria';"
    Exigir (($param.Rows.Count -eq 2) -and (@($param.Rows | Where-Object { $_.tipo -ne 'varchar' -or $_.max_length -ne 30 }).Count -eq 0)) 'Parámetro categoria inesperado.'
    # SOLO lecturas: nunca invocar Reactivar, Insertar, Modificar, pagos o estadías.
    $activos=Tabla $Conexion 'EXEC dbo.sp_Servicio_Listar;'
    Exigir (($activos.Rows.Count -eq 0) -and $activos.Columns.Contains('categoria')) 'El servicio inactivo apareció en el catálogo activo o falta categoria.'
    $inactivos=Tabla $Conexion 'EXEC dbo.sp_Servicio_ListarInactivos;'
    Exigir (($inactivos.Rows.Count -eq 1) -and ($inactivos.Rows[0].id_servicio -eq 1) -and ($inactivos.Rows[0].categoria -ceq 'Otros')) 'Listado inactivo incompatible.'
    $null=Tabla $Conexion 'EXEC dbo.sp_Habitacion_Listar;'
    $null=Tabla $Conexion 'EXEC dbo.sp_Habitacion_ListarInactivas;'
    $null=Tabla $Conexion "EXEC dbo.sp_Habitacion_ConsultarDisponibilidad @fecha_entrada='20990101', @fecha_salida='20990102';"
    Registrar 'Contratos y lecturas SQL' 'Listados activos/inactivos y disponibilidad completados; procedimientos de escritura no invocados.'
    # Repetir huellas tras las lecturas para detectar efectos no previstos.
    $final=Huellas
    foreach ($n in $previas.Keys) { Exigir ($final.ContainsKey($n) -and ($previas[$n] -ceq $final[$n])) ('Una lectura alteró datos: '+$n) }
    Registrar 'Verificación final de datos' 'Sin cambios históricos detectados. Pruebas funcionales de escritura quedan manuales.'
} catch {
    $Fallo=$_.Exception.Message
    $Pasos.Add([pscustomobject]@{Prueba='Ejecución';Estado='ERROR';Detalle=$Fallo})
    Write-Warning $Fallo
} finally {
    if ($null -ne $Conexion) { $Conexion.Dispose() }
    if ($null -ne $Maestra) { $Maestra.Dispose() }
    $informe=[pscustomobject]@{ Fecha=(Get-Date).ToString('o'); Servidor=$Servidor; Base=$Pruebas; AplicacionSql=$AplicacionSql; ConexionesPropias=$ConexionesPropias.ToArray(); ObservacionesSesiones=$ObservacionSesion; ModificacionesConfirmadas=$Confirmado; Exito=($null -eq $Fallo); Pasos=$Pasos.ToArray(); Limitacion='Sin validación visual. No se instalaron permisos. Una falla posterior a un script confirmado puede dejar una migración parcial; no se ejecuta reversión automática.' }
    $informe | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $Salida 'informe.json') -Encoding UTF8
    $Pasos | Format-Table -AutoSize -Wrap | Out-String -Width 180 | Set-Content (Join-Path $Salida 'informe.txt') -Encoding UTF8
    Write-Host ('Informe: '+$Salida)
}
if ($null -ne $Fallo) { throw ('Automatización detenida. Revisar informe. '+$Fallo) }
