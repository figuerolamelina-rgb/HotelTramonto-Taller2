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
function Convertir-ArgumentoWindows([AllowEmptyString()][string]$Valor) {
    # Reglas de argumentos de Windows: duplicar barras ante comillas y al cerrar.
    $texto=New-Object Text.StringBuilder
    [void]$texto.Append([char]34)
    $barras=0
    foreach ($caracter in $Valor.ToCharArray()) {
        if ($caracter -eq [char]92) { $barras++; continue }
        if ($caracter -eq [char]34) {
            [void]$texto.Append(('\' * (2*$barras+1)))
            [void]$texto.Append([char]34)
        } else {
            [void]$texto.Append(('\' * $barras))
            [void]$texto.Append($caracter)
        }
        $barras=0
    }
    [void]$texto.Append(('\' * (2*$barras)))
    [void]$texto.Append([char]34)
    return $texto.ToString()
}
function Invoke-HotelDotNet {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)][AllowEmptyString()][string[]]$Argumentos,
        [Parameter(Mandatory=$true)][ValidateSet('restore.log','build.log','tests.log')][string]$Log,
        [string]$Ejecutable
    )
    $directorio=[IO.Path]::GetFullPath($Raiz)
    if (-not [IO.Directory]::Exists($directorio)) { throw ('No existe la raíz: '+$directorio) }
    $carpeta=[IO.Path]::GetFullPath($Salida)
    [void][IO.Directory]::CreateDirectory($carpeta)
    $rutaLog=[IO.Path]::Combine($carpeta,$Log)
    $argsNativos=(@($Argumentos | ForEach-Object { Convertir-ArgumentoWindows $_ })) -join ' '
    $cabecera='Directorio: '+$directorio+"`r`nArgumentos: "+$argsNativos+"`r`n"
    [IO.File]::WriteAllText($rutaLog,$cabecera,[Text.Encoding]::UTF8)
    $proceso=New-Object Diagnostics.Process
    try {
        if ([string]::IsNullOrWhiteSpace($Ejecutable)) {
            # No usar & dotnet: PowerShell ignora mayúsculas y resolvía la antigua
            # función DotNet recursivamente en lugar del ejecutable.
            $Ejecutable=(Get-Command -Name dotnet.exe -CommandType Application -ErrorAction Stop).Source
        }
        if (-not [IO.Path]::IsPathRooted($Ejecutable)) { throw 'Se requiere una ruta absoluta al ejecutable.' }
        $proceso.StartInfo.FileName=$Ejecutable
        $proceso.StartInfo.WorkingDirectory=$directorio
        $proceso.StartInfo.Arguments=$argsNativos
        $proceso.StartInfo.UseShellExecute=$false
        $proceso.StartInfo.RedirectStandardOutput=$true
        $proceso.StartInfo.RedirectStandardError=$true
        $proceso.StartInfo.CreateNoWindow=$true
        if (-not $proceso.Start()) { throw 'No se pudo iniciar el proceso.' }
        # Leer ambos canales simultáneamente para no bloquear por buffers llenos.
        $stdout=$proceso.StandardOutput.ReadToEndAsync()
        $stderr=$proceso.StandardError.ReadToEndAsync()
        $proceso.WaitForExit()
        $salidaNormal=$stdout.GetAwaiter().GetResult()
        $salidaError=$stderr.GetAwaiter().GetResult()
        $codigo=$proceso.ExitCode
        [IO.File]::AppendAllText($rutaLog,"`r`nSTDOUT:`r`n"+$salidaNormal+"`r`nSTDERR:`r`n"+$salidaError+"`r`nExitCode: "+$codigo+"`r`n",[Text.Encoding]::UTF8)
        if ($codigo -ne 0) {
            $detalle=(@(($salidaNormal+"`n"+$salidaError) -split '\r?\n' | Select-Object -Last 40)) -join "`n"
            throw ('dotnet '+$Argumentos[0]+' falló (código '+$codigo+'). Log: '+$rutaLog+"`n"+$detalle)
        }
    } catch {
        [IO.File]::AppendAllText($rutaLog,"`r`nERROR: "+$_.Exception.Message+"`r`n",[Text.Encoding]::UTF8)
        throw
    } finally { $proceso.Dispose() }
}
$NumeroArchivo=0
try {
    Exigir ($env:OS -eq 'Windows_NT') 'Ejecutar en Windows con Windows PowerShell 5.1.'
    Add-Type -AssemblyName System.Data
    $null=Get-Command dotnet.exe -CommandType Application -ErrorAction Stop
    # Fuentes combinados revisados; filtros y allowlist SQL no cambian.
    $Manifest=@{
        'HotelTramonto.Datos/ConexionBD.cs'='AFEC5A33A1757C3A34081983CC2A2F622C4E10F129A2BB72E1D987F879E26D29'
        'HotelTramonto.Datos/DetalleReservaDatos.cs'='2FE2BBA8F285392190EEE4F9E2C3C028F5BD4BCE815B9D37F63F57F67E13CF4E'
        'HotelTramonto.Datos/DetalleServicioDatos.cs'='F65DE1CC130A6402ECAFF5A61CB165D377DD744929223A8E64665117F7FD6230'
        'HotelTramonto.Datos/HabitacionDatos.cs'='7246FC16507414ACFEAF0FA9440E37399D69FBBA512DAF7B7351FE7D8CE26199'
        'HotelTramonto.Datos/HotelTramonto.Datos.csproj'='1CC3B1B2C6F273E5D164D57EE882EC45F61253023E3499CE9FC964EE5E4B872E'
        'HotelTramonto.Datos/HuespedDatos.cs'='05314277A1E35E61A16E090E8AD4758C91A185439DF9F4E74D20B8E5A7095977'
        'HotelTramonto.Datos/IDetalleServicioDatos.cs'='DBAF97BF317C485F933DD6BF24D21F4C3EBE64CC19F9F2DFFDFFC64F376F848C'
        'HotelTramonto.Datos/IHabitacionDatos.cs'='D9AB5849723750E0A702CBBAB3CEAF0EE7146B99DB7B33CA247F5FC89CA87D6A'
        'HotelTramonto.Datos/ITipoHabitacionDatos.cs'='AB86774F0A578DBDFCAE310F2D9E58FD29FA2277E25A2019D86D9381754734A7'
        'HotelTramonto.Datos/PagoDatos.cs'='7676A600A88BAB2E702A49E9F1D9606E9A8F8173D7B1B1885BF3BCA90FD51B70'
        'HotelTramonto.Datos/ReporteDatos.cs'='9F97362B8D2812FCCBD3C1B7EDC9F80C116086F0FF12FFD60CAE45AA540D2F6F'
        'HotelTramonto.Datos/ReservaDatos.cs'='33303FE0F8F54A8885690177799FAB7B307CD47D494CCFBCEB8B99F501AAF45A'
        'HotelTramonto.Datos/ReservaTemporalDatos.cs'='3581E26825A0BC09820F2A97A1919840BD03B7358B4C76DCA2B83DA944B43C27'
        'HotelTramonto.Datos/RolDatos.cs'='F5E5226644B5E0312D6820EBDCD5C63F3488C5FE46FD419535EAA278A8067F78'
        'HotelTramonto.Datos/ServicioDatos.cs'='D17348A5F2EC293E559FB1BA8B9CBD68AA292F4D484A1962E8C2616135EBB2AE'
        'HotelTramonto.Datos/TipoHabitacionDatos.cs'='EE4F9B69B4D6839836A30702AE304831406DC156248F529BEAA619F12F676136'
        'HotelTramonto.Datos/UsuarioDatos.cs'='1856802409682B860D33428EF492CDC5862EF147A4D9F3DF4156EA9D9E431441'
        'HotelTramonto.Entidades/Entidades/DetalleReserva.cs'='37B2FDEAD7C6BFA105EFFE01D1479B541CDA1B5BA2514B55538A19BB2780AFFF'
        'HotelTramonto.Entidades/Entidades/DetalleServicio.cs'='2F4FBCFA1801A99EA52D02B46D2108B9F987681E5B7C272CCFF9D1EB331DC7B3'
        'HotelTramonto.Entidades/Entidades/Habitacion.cs'='0ABEF8B1B7C10FD756229C1E48BDC5CE61086FE051BECA92DAE2CAB3A5065E60'
        'HotelTramonto.Entidades/Entidades/Huesped.cs'='C72310E964FA078C5BF44AD8F080C429085C91FABDA30ED0966A46738B9EF720'
        'HotelTramonto.Entidades/Entidades/Pago.cs'='482126F367AEE035CFDAC26CAD245EB0A06B009A185981979FAA5E472A6275C7'
        'HotelTramonto.Entidades/Entidades/Reserva.cs'='B683FE37215304F0091BB4DE143C88E28C7E584C48B8DDDE2170C745C15E7F21'
        'HotelTramonto.Entidades/Entidades/ReservaTemporal.cs'='A0E8B2C96950195CF02EE2EEB112D312B36D8D460EFDD2B328E4084D43C9D4EF'
        'HotelTramonto.Entidades/Entidades/Rol.cs'='50DA547AC2D629E71647C62C718FC3F5633F6D1A38AD210328EC8195BFD1672B'
        'HotelTramonto.Entidades/Entidades/Servicio.cs'='9519AD5E9F0006D2902EC6C6C0CB14D7D572AA11A5548C7338BF2E5EAC862746'
        'HotelTramonto.Entidades/Entidades/TipoHabitacion.cs'='BA8F6628C976D2EAADD2BF3CC9E48F7031833805A9BC3DD11D3B3297390474A9'
        'HotelTramonto.Entidades/Entidades/Usuario.cs'='891101CD79D1F3CBA8FBF501DFC06A662F962C29E3DFD4CF630BAD1EC46CCA3B'
        'HotelTramonto.Entidades/HotelTramonto.Entidades.csproj'='2C82DAE7A6492E5DC0D99B6B5A5D1C89A4702B892F71757981E46302949D6115'
        'HotelTramonto.Negocios/DetalleServicioNegocios.cs'='BD2C7F5B9A5B3A231BBA16301AEF1CBE4FCFAC66458DE2C41B3BAB27030E36F0'
        'HotelTramonto.Negocios/DiarioOperaciones.cs'='32E415DAA5FA235C5E2C6A2EA6FC902C2E819A3191CFE74B4326194F522151E7'
        'HotelTramonto.Negocios/FlujoReservaNegocios.cs'='9067AF23FF8CF1B3E9E1BABD61988566580DFFD02098B1FE2D22DA39CCFFDBF4'
        'HotelTramonto.Negocios/HabitacionNegocios.cs'='209C3ABEB76570718E5AA9447BCF66514A519973A031C28F0695A5404B3B8B58'
        'HotelTramonto.Negocios/HotelTramonto.Negocios.csproj'='907A90BAEA0C27DE59035EDA2A37924F52AF24BD2E3635A7AB39D203A194033D'
        'HotelTramonto.Negocios/HuespedNegocios.cs'='A6FE230841EA6FAAD92817913A4218A14C6FD499EA75288E23412A8AF7447ADD'
        'HotelTramonto.Negocios/PagoNegocios.cs'='91EFE92EB8EEBEAA0E33A5CB346DCA1119ACFFBB540A30EA81CCDC233A48BE10'
        'HotelTramonto.Negocios/PasswordService.cs'='83C310101CFB7A6ECA4462EAE07586EF9892276352D197A1E894FE0D9FFCBC93'
        'HotelTramonto.Negocios/Permisos.cs'='C0AF02401728F12CEFD331A466E9946EE2E590E25CDE6C98F0CCC5E61F4E16D7'
        'HotelTramonto.Negocios/PermisosHabitaciones.cs'='04385D8A0AF3A1E00E723A2047A643F58CCC94677624970864D71C12689413C5'
        'HotelTramonto.Negocios/ReporteNegocios.cs'='5C543809794EA0F18246AE9B819B67FA75AF39020F5338C90C651D0B1935DCB5'
        'HotelTramonto.Negocios/ReservaNegocios.cs'='B73E851B0C2F1F0186BA01E2311F53F140990A9A0300D1547540419451983E7D'
        'HotelTramonto.Negocios/ReservaTemporalNegocios.cs'='23AA501FCB8984057523010356CB6026723105B7289A1EE68260545587064D82'
        'HotelTramonto.Negocios/ServicioNegocios.cs'='236523C30753577C4442826959723193DD2282F53773327ABC7425588AFF19FB'
        'HotelTramonto.Negocios/TipoHabitacionNegocios.cs'='09429E276833B42E817EBBC25BB48EE331FE6799BE99E366BF189548BA51CF74'
        'HotelTramonto.Negocios/UsuarioNegocios.cs'='C72644003C4216A763F4AE172C2B52B448302279D09560D24FBDF73462B8CBC9'
        'HotelTramonto.Negocios/Validaciones.cs'='B8AEBFF1195EC1E26F87A0FC48AB2659E0D97AA032831569BC6EA7C4A8C3C0C4'
        'HotelTramonto.Presentacion/Form1.Designer.cs'='CDD7AF20C7E87A03570888C16DBCFDD1FA6A535301341ACBF6E67342DCC063F2'
        'HotelTramonto.Presentacion/Form1.cs'='32FB9EA25ABA7171CC6FD8C71D95A9EC4374510DB1259E92F5E5E0CF9C105749'
        'HotelTramonto.Presentacion/FormHabitaciones.Designer.cs'='587E98D9451399CAE4AB214B0EF1B58EB76ABF3DBC58C4FD015302427D1AAEF7'
        'HotelTramonto.Presentacion/FormHabitaciones.cs'='DC1BDB2A89444BA077F9B4CD2DBC9FF69848F526E9FD655B20069CB8A0D8F8FE'
        'HotelTramonto.Presentacion/FormHuespedes.Designer.cs'='F45F35D207CA7655261D7ECADEDDF485AB1CA292F3ABF9780256DD715237D5DC'
        'HotelTramonto.Presentacion/FormHuespedes.cs'='76B07ED084F44887805FAC16C893CF0F657E7B52BB69CB1E96648AFB7021E5C7'
        'HotelTramonto.Presentacion/FormLogin.Designer.cs'='DC633D7FD562DAF5484872BB7275005D9C6A12B748E85E959CA3465A9889E234'
        'HotelTramonto.Presentacion/FormLogin.cs'='978A21B43105CBA423656341D01F7E3CADE6964B2D91DD86AB3DACEDF8E7F708'
        'HotelTramonto.Presentacion/FormModificarHabitacion.Designer.cs'='8F276E5DACC67500ED12CD87A16EFBB1BC51E726D385440CE330BED0BB1678C0'
        'HotelTramonto.Presentacion/FormModificarHabitacion.cs'='8988C36CE76F6508CBE71B264D6EA26990A29A51DAA7D316EA6F84478F3B89DC'
        'HotelTramonto.Presentacion/FormModificarHuesped.Designer.cs'='848B1309E39480BED90C05C633A63A6237726F5E0604032735949EECD412295E'
        'HotelTramonto.Presentacion/FormModificarHuesped.cs'='6B26FA71CA2212062A1A6EC4C88CD563A2A660DC20147BF00A8B948EB6EE9624'
        'HotelTramonto.Presentacion/FormModificarUsuario.Designer.cs'='083A0005EC050094550284BA0C3894BFABA9FDD98A2E78360C0E2312BD4CEB18'
        'HotelTramonto.Presentacion/FormModificarUsuario.cs'='2B708D858D254518C1694F69EBFA40863A30A551480625205AD2055F21A1CA45'
        'HotelTramonto.Presentacion/FormNuevaHabitacion.Designer.cs'='858613EBE6E19598D6A004B678D7EAEF5BAA22D7672EF16E96563DFF36EEA1C7'
        'HotelTramonto.Presentacion/FormNuevaHabitacion.cs'='0F72CE8215B1C92E65304D021B2976AC00FC7CAD3E831DCA095355A69F183269'
        'HotelTramonto.Presentacion/FormNuevaReserva.Designer.cs'='504CC366504807F9C266150E21C23954BD1C93B209EEFEBBC39C9B00613956F7'
        'HotelTramonto.Presentacion/FormNuevaReserva.cs'='AFC9F8ED454EFDF09C98184582C9A8136B5615D84C3ECA36DACD9B448D6C16C0'
        'HotelTramonto.Presentacion/FormNuevoHuesped.Designer.cs'='01291040704642BEA2A7632CA79A3A8413D33580853697418CEDC0A3B3A99C13'
        'HotelTramonto.Presentacion/FormNuevoHuesped.cs'='D361A3CD7C708D340D49C6A1599EAAC90250CCBD8BC5522983F32D8A0B57F3A1'
        'HotelTramonto.Presentacion/FormPagos.Designer.cs'='9B156FBC11D2C56C291C1FC0E3E5F87DD08A5156B9B82127EE12960AB62A7E42'
        'HotelTramonto.Presentacion/FormPagos.cs'='E0F62AC82C5D7C5F49BF250ECA31C43E8F6693D3D67CE06E00AFD41F9F699237'
        'HotelTramonto.Presentacion/FormReactivarHabitacion.Designer.cs'='EF5B4E6A6E4B574CF8B0D287E241871002D69A8D85DFD022F797A714804A8357'
        'HotelTramonto.Presentacion/FormReactivarHabitacion.cs'='985580196E8CDCCDC1CE9A25C4CAD4A9DB63E6EE0DA0C1C59F7B914B79E7D5D2'
        'HotelTramonto.Presentacion/FormRegistrarConsumos.Designer.cs'='E8E91939B74987442C603DE9627202A12FEBA6E89EDE0E50219FEDBCF179D4C4'
        'HotelTramonto.Presentacion/FormRegistrarConsumos.cs'='59D8A2A23948809670BBF73122C7EAE2BBA32CC37F97E45CF51B7BB45183C30E'
        'HotelTramonto.Presentacion/FormReportes.Designer.cs'='6DD9E386095607F1098485ABC91B90F5130B12C8CE9F65643F59BFB9FFF466A1'
        'HotelTramonto.Presentacion/FormReportes.cs'='7FAEE25DA2824233584C480D4B4979D0AA03728E29B22651988E3E19ED53A1BD'
        'HotelTramonto.Presentacion/FormReservas.Designer.cs'='836CFC5084BB6AA514F37D20A18180D7ABD2642C4F67BE1A57A583FA1D1009EB'
        'HotelTramonto.Presentacion/FormReservas.cs'='D8C2CD12E4CF664B1E484F4B716136A289BB74926863489A637F4BCC72009B52'
        'HotelTramonto.Presentacion/FormServicios.Designer.cs'='10F3E318D68E7EBF3DFA003AE84291CFB1BC7795B3FD41AAC637944C314C8202'
        'HotelTramonto.Presentacion/FormServicios.cs'='7D9150CCB4C16D6DFF6652C865940F65FFBD7673A01F6CCEEDA1316541BCAE94'
        'HotelTramonto.Presentacion/FormUsuario.Designer.cs'='C344F66B1BCD7912841315A1CDAF3582871E6D4986C1C08BDB8E8F58CABBEAA5'
        'HotelTramonto.Presentacion/FormUsuario.cs'='AC4F02BF5362A545C9F3490A83B21CD52451D78C3D14D73B32F2458309EBE63B'
        'HotelTramonto.Presentacion/HotelTramonto.Presentacion.csproj'='D88A98B58DBEE6729F8737709A0DBE309CA95A1F0280C3E4FF64C24E5BD37BAB'
        'HotelTramonto.Presentacion/Program.cs'='7AE4B17C1ED6F7BB6042945250427795C3C9C0BD5CD5B2ADE61B9EE7AB439E81'
        'HotelTramonto.Presentacion/Resources.Designer.cs'='0E79DD4EE09F1B1A35AA6B339CE437E03CFF430F853B8017F2F110F315B6AF09'
        'HotelTramonto.Pruebas/ConfiguracionLocalTests.cs'='F662B42D0381AAD095CB2025B77BEA2B733ED8148415E6C2E6EAFCE5D36A4D97'
        'HotelTramonto.Pruebas/DefinicionesAuditadasTests.cs'='B65813CC70E430803A553398E44CD604A7DECB23764C67BBD7FFB86407824D34'
        'HotelTramonto.Pruebas/DiarioOperacionesTests.cs'='24FEB7C483DD7D06631C1900E44B9EBAA822D2244A9616CA0F2B427E9D935743'
        'HotelTramonto.Pruebas/HabitacionesFixtureTests.cs'='2C298310E1CB55094CE7523B02A30511621D56CE59D365017F933DDBF9922FDB'
        'HotelTramonto.Pruebas/HistorialHuespedTests.cs'='8604CBCAE88F59B4B05629D7E3B96BD418913C8DE4E571071EBD010ED8C93085'
        'HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj'='2A5285A58A256AFD611FF972376ECEDCF679A9447AC9E75CF1AE5E9109A7DDB2'
        'HotelTramonto.Pruebas/IntegracionCorreccionesTests.cs'='21D15FD52DF0033A613920AA6E25DC3CC76D446D8E9FCB82C6EA4BAA4A709C7B'
        'HotelTramonto.Pruebas/MSTestSettings.cs'='72F34577D30D2F5C78B3C50D2A2D898B52ACCA841CA165B68E352462F8FE525D'
        'HotelTramonto.Pruebas/PermisosHabitacionesTests.cs'='21D3A80AB6058EAF70BF2E91A1B6039F4C862B713EB7217736D52E035818D74D'
        'HotelTramonto.Pruebas/PruebasFinalesHabitacionesTests.cs'='A5C4E52890F5D4AC188B40D4DEB4B48D66F529282CBE7050F826DE390A3DB69B'
        'HotelTramonto.Pruebas/SeguridadVerificacionSqlTests.cs'='4C962CE551F79CD2423D72EBC4D540F8E86DFEFDA75B81B4155D5818B364E9E8'
        'HotelTramonto.Pruebas/ServiciosHabitacionTests.cs'='A7192CA087DA8408B1EC7CFC5D9EE259D3D7562897DEE6D0101705EB7776B498'
        'HotelTramonto.Pruebas/Test1.cs'='4552B893873D4957606737134B326772E10405442E651E2DCFE40BB797984BF5'
        'HotelTramonto.slnx'='38DF1D0077777EA6791B0426B1A31CF069A9F6CDA5FF03703C15FCC144A97836'
        'base de datos HotelTramonto/Actualizacion_HistorialHuespedes.sql'='D5834CDE2D8C0FA8FB4454DF26441DC75D1CF6B7A139C9DFCDCE593DC6349823'
        'base de datos HotelTramonto/Actualizacion_ReservasTemporales.sql'='519CACD9C96AEEE76844CD59E8D9F87A205EED817339EEC4E1B8B7CE582990C6'
        'base de datos HotelTramonto/Actualizacion_Reservas_IdUsuario.sql'='DD671B44C222E62D7C50264337E3C5CC37636DCB28F426620CA030296BD42C0F'
        'base de datos HotelTramonto/Actualizacion_Usuarios_Email.sql'='B828CA1C69C26A0B6BD33546F67F0B6B49E76AFEA48BB7ECF7C7B3120F0570F3'
        'base de datos HotelTramonto/Creacion_Base_HotelTramonto.sql'='9D543FD3513B93967014620CF741EF83B49AD0B8470CDF3A198E300282C07521'
        'base de datos HotelTramonto/Indices_HotelTramonto.sql'='E945FE926D18FE8B414934CAB02FC3E22C1AF70A09D03F759AD4C173D7FF27B5'
        'base de datos HotelTramonto/Procedimientos_Almacenados.sql'='6AC5C5C431DBEFFF7622EB44598D6DC49F0109DB47CE908C85B05FE1CB42120F'
        'base de datos HotelTramonto/compatibilidad-servicios/00_DiagnosticoYRespaldo.sql'='F77089D67BE7DC8CD62747ABC76BCC5B637C2EA23B9270D368FDC950E94A9ADA'
        'base de datos HotelTramonto/compatibilidad-servicios/01_AgregarCategoria.sql'='00C5558A8740532C03FC7DE88D993DAAB21DA580D7E9EE69E42F91C413A047CB'
        'base de datos HotelTramonto/compatibilidad-servicios/02_ClasificarServicios.sql'='C7C9471A6FE33C28DE1ED61CA3C3CDCB626DE344F47FB7A33BD88BA0706A5BE6'
        'base de datos HotelTramonto/compatibilidad-servicios/03_ActualizarProcedimientos.sql'='2D6D9B2C2D8CE691157BAF1EAA40AED14D503E96711BD82A2EE15145B3AA7DFC'
        'base de datos HotelTramonto/compatibilidad-servicios/04_Comprobaciones.sql'='7AE4E34EBB8C5AD315D1BF2C4F88D36F579EECAAC89B4ADAA9CE9D1AD46EE58A'
        'base de datos HotelTramonto/compatibilidad-servicios/05_RevertirProcedimientos.sql'='30608753FA0EC9AC4195953C2D208A4B93991E4B54F30BBE0B533A8A553CE3C9'
        'base de datos HotelTramonto/integracion/01_PermisosConsultasAdicionales_Pruebas.sql'='38B9AD5636A9CC02E614EB2A9372C4FC9C4A4FE7FCB9A0E67459A36B87D34817'
        'base de datos HotelTramonto/integracion/02_VerificarConsultasAdicionales_Pruebas.sql'='22BA91523785772E34220F0E8EBB5FBA6EDCAE32E456454AFF0B611CCEA365AC'
        'base de datos HotelTramonto/permisos-habitaciones/00_Diagnostico.sql'='1D93EC24176BC0F68867771EA0AC4CBC896C2E524F60BC7DEEE6808EDCBBF2BB'
        'base de datos HotelTramonto/permisos-habitaciones/01_ProcedimientosFaltantes.sql'='A067BB173009352B21C6AF9BE3C9C9EC822464AA9775962A48424915B8E7B5E3'
        'base de datos HotelTramonto/permisos-habitaciones/02_PermisosMinimos.sql'='7ECD728251C8AE750AD625D6532FA4F8498E1173B599833312C94254E922657A'
        'base de datos HotelTramonto/permisos-habitaciones/03_VerificarPermisos.sql'='2D76BDF9788A11187F5BE959CE1A62AEE7385A6584F20B4A8040AE21FCD99559'
        'base de datos HotelTramonto/permisos-habitaciones/04_RevertirPermisos.sql'='1652403A0CAD32C76622E67B0DEE9A02B24C385D408C64AC58F6E5074793C279'
        'base de datos HotelTramonto/pruebas-finales/00_VerificarCorreccion2.sql'='88F12B9743518B0BC4F0D157B98B49CDC2A008AE2702C70DDC34322DB20C45BB'
        'base de datos HotelTramonto/pruebas/HistorialHuespedes_Consulta.sql'='FD5F801FBCED168E95FD0D2EF1609A6F5BAFCA3C98F07D3C2C2EC5D7876E49C3'
        'base de datos HotelTramonto/pruebas/Instalar_HistorialHuespedes_Pruebas.sql'='7F55F0E5C8E45DC6E6B03647F45D3E100C6DA6A0D70A7E8766B2E7FCC021141A'
        'base de datos HotelTramonto/servicios-habitaciones/00_Diagnostico.sql'='A0E4696B855DF178C256E3A4C9F1C88E94C7323E746F2EF7220D36648AF4E772'
        'base de datos HotelTramonto/servicios-habitaciones/01_Instalar.sql'='8989A79BA7FDDAE1511C4D69F9DC119493B658A15F8ED71F5F92A6614DB89D94'
        'base de datos HotelTramonto/servicios-habitaciones/02_Verificar.sql'='44BC0AF0ED7AAB1A2DFEADC6067AC1573E02467B1E38E87517EA54D5B24429AC'
        'base de datos HotelTramonto/servicios-habitaciones/03_Recuperacion.sql'='DA06E2BC4325172D02B6A6E225C530EB43EEF44124160E943C8A4430E95B6EE8'
        'base de datos HotelTramonto/servicios-habitaciones/04_ConcurrenciaPagos.sql'='A7097D597A34BB1982D35286782C44CF436CA7DE5EE3947567DC36D4DD093929'
        'base de datos HotelTramonto/servicios-habitaciones/05_VerificarConcurrencia.sql'='215B55442797C765ECAE9DFF9E131DBCB4EC74C867D28B33F56A559546DE7C85'
        'base de datos HotelTramonto/servicios-habitaciones/06_SuspenderEscrituras.sql'='C438B6F177EBDE22925CFF036556AA53EB21A7CF02BB5C3E64CF97BFECEA3A4A'
        'tests/HotelTramonto.VerificacionSql/CasosSql.cs'='1E6CABDA0C2311D0666005E61789CC4521C259BB7ACB153FCD858C8A712356C3'
        'tests/HotelTramonto.VerificacionSql/ContratosBase.cs'='2AB522B10163C96027B32E8812988A7F4B27B8816C326991B71436B3C0BC52DD'
        'tests/HotelTramonto.VerificacionSql/DefinicionesAuditadas.cs'='03E2BFE58493F41F02A9890E036132609249BB31D4D741CCE3379AFB312089AB'
        'tests/HotelTramonto.VerificacionSql/DiagnosticoDefiniciones.cs'='4B86D82E6E8F20F231B5EF7D7C0C85A348B10D207D0DE04481FA436BA769D138'
        'tests/HotelTramonto.VerificacionSql/Ejecutor.cs'='C2BF41DBD92BCAC5DAEA571ADFE0FD6C6DCA09B84B277853704C4BB82D02ACC5'
        'tests/HotelTramonto.VerificacionSql/DiagnosticoCiclo.cs'='0E1BA79CFDF64F8AFD241F91AA074F3B1F12F4FF5EBE2005C5F3AC2EDD4C8E80'
        'tests/HotelTramonto.VerificacionSql/DiagnosticoIntegracion.cs'='16D5D12BD50750F8DBD3FA7B962F6F0A60E2C6A5EFB39C86ACC1147BA9DAE060'
        'tests/HotelTramonto.VerificacionSql/HabitacionesFixture.cs'='5EEACE65C3F1A2D824E238228AE6BFF48F942A5741A42A3C1C6A3FE6510AC6AF'
        'tests/HotelTramonto.VerificacionSql/Historicos.cs'='7D67E92E896987CDC748B4257862CA28058DF0F363A85AFFB7572F58D9A59E54'
        'tests/HotelTramonto.VerificacionSql/HotelTramonto.VerificacionSql.csproj'='80061FAA76ABECF87DF5B663F9D5CD00BDED96240DB1E3219272B47C0E21E988'
        'tests/HotelTramonto.VerificacionSql/OfflineChecks.cs'='23FAEC960A62E55A9A329CF0C0AC239335063884F5595907D2DA9BF8C28F2267'
        'tests/HotelTramonto.VerificacionSql/Program.cs'='3898C78DEDE2D59C6D3EBC6F2B5FC04F5ACD66A076E3C3B00B97E754E7786908'
        'tests/HotelTramonto.VerificacionSql/SeguridadCopia.cs'='463685A5BD54DCDE0AE2E0C906B53D4638D834C846E84470BCF209406673EB1D'
        'tests/HotelTramonto.VerificacionSql/Sql.cs'='DCEA115E345C0996BEB1B39ECD544031E4061D4C31A60C85E53F7BC0895DD7F2'
        'tools/Probar-CadenaConexion.ps1'='C5C2E5C64C637F2FC4F8E03F4BDB1C501E3E0166A9F0A343C55F1E79D2153C0C'
        'tools/Probar-LecturasSql.ps1'='33FD5474D7EC4B99E34DD4A030546F803300EE62151F620D9A8D56E54AF713B6'
        'tools/Probar-ProcesoDotNet.ps1'='29B09582C3C8267F7E11B1193894B2D4B4EC0BE9333F38941729A70D2617BD65'
        'tools/Probar-SesionesSql.ps1'='6D4FF42FFDD13BBBFE4A43AD42905EA7C6A05295A915C6F426817904F5A2440A'
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
    & (Join-Path $PSScriptRoot 'Probar-ProcesoDotNet.ps1') | Out-File -LiteralPath (Join-Path $Salida 'proceso-dotnet.log') -Encoding utf8
    Registrar 'Ejecución de procesos dotnet' 'Seis casos simulados de argumentos, directorios y logs aprobados sin SQL.'
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
    Invoke-HotelDotNet -Argumentos @('restore','HotelTramonto.slnx') -Log 'restore.log'
    Invoke-HotelDotNet -Argumentos @('build','HotelTramonto.slnx','--no-restore') -Log 'build.log'
    $configAnterior=$env:HOTELTRAMONTO_CONFIG
    $env:HOTELTRAMONTO_CONFIG=Join-Path $Salida 'conexion-bloqueada-inexistente.json'
    Exigir (-not (Test-Path -LiteralPath $env:HOTELTRAMONTO_CONFIG)) 'La configuración de bloqueo no debe existir.'
    try {
        Invoke-HotelDotNet -Argumentos @('test','HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj','--no-build','--no-restore','--filter',$FiltroSeguro,'--logger','trx;LogFileName=seguras.trx','--results-directory',$Salida) -Log 'tests.log'
    } finally { $env:HOTELTRAMONTO_CONFIG=$configAnterior }
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
