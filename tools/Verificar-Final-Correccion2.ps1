#requires -Version 5.1
[CmdletBinding()]
param([switch]$ConfiarCertificadoLocal,[switch]$SinSql)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$Raiz=Split-Path $PSScriptRoot -Parent
$Salida=Join-Path $env:LOCALAPPDATA ('HotelTramonto\InformesFinales\'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
[void][IO.Directory]::CreateDirectory($Salida)
$pasos=New-Object 'System.Collections.Generic.List[object]'
$c=$null; $errorFinal=$null; $sqlEstado='PENDIENTE'
function Aprobar([string]$Nombre) { $pasos.Add([pscustomobject]@{prueba=$Nombre;resultado='APROBADA'}); Write-Host ('OK: '+$Nombre) }
function Revisar([bool]$Valor,[string]$Mensaje) { if (-not $Valor) { throw $Mensaje } }
function HashTexto([string]$Ruta) {
    $texto=[IO.File]::ReadAllText($Ruta).Replace("`r`n","`n").Replace("`r","`n")
    $sha=[Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($texto)))).Replace('-','') }
    finally { $sha.Dispose() }
}
try {
    Revisar ($env:OS -eq 'Windows_NT') 'Usar Windows PowerShell 5.1.'
    $archivo=Join-Path $PSScriptRoot 'pruebas-finales.manifest.json'
    Revisar ((HashTexto $archivo) -ceq 'F79CCD491B1CCE00A1A8ACB428122F82CA31FE178A9B687FACC0CAD5F797631E') 'El manifiesto final fue modificado.'
    $m=[IO.File]::ReadAllText($archivo) | ConvertFrom-Json
    foreach ($p in $m.archivos.PSObject.Properties) {
        Revisar ((HashTexto (Join-Path $Raiz $p.Name)) -ceq $p.Value) ('Archivo distinto del revisado: '+$p.Name)
    }
    Aprobar 'Integridad de fuentes, pruebas y contratos'
    # Cargar solo funciones puras/procesos del runner; NUNCA ejecutar su cuerpo/migración.
    $tokens=$null; $errores=$null
    $ast=[System.Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Probar-HotelTramonto.ps1'),[ref]$tokens,[ref]$errores)
    Revisar ($errores.Count -eq 0) 'Sintaxis inválida del helper.'
    foreach ($nombre in @('Convertir-ArgumentoWindows','Invoke-HotelDotNet','Crear-CadenaConexion','Canon')) {
        $f=@($ast.FindAll({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $nombre},$false))
        Revisar ($f.Count -eq 1) ('Falta helper '+$nombre)
        . ([scriptblock]::Create($f[0].Extent.Text))
    }
    Invoke-HotelDotNet -Argumentos @('restore','HotelTramonto.slnx') -Log 'restore.log'
    Invoke-HotelDotNet -Argumentos @('build','HotelTramonto.slnx','--no-restore') -Log 'build.log'
    $previa=$env:HOTELTRAMONTO_CONFIG
    try {
        $env:HOTELTRAMONTO_CONFIG=Join-Path $Salida 'configuracion-inexistente.json'
        Revisar (-not [IO.File]::Exists($env:HOTELTRAMONTO_CONFIG)) 'Configuración de bloqueo inesperada.'
        Invoke-HotelDotNet -Argumentos @('test','HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj','--no-build','--no-restore','--filter',[string]$m.filtro,'--logger','trx;LogFileName=finales.trx','--results-directory',$Salida) -Log 'tests.log'
    } finally { $env:HOTELTRAMONTO_CONFIG=$previa }
    [xml]$trx=[IO.File]::ReadAllText((Join-Path $Salida 'finales.trx'))
    $ct=$trx.SelectSingleNode("//*[local-name()='Counters']")
    Revisar (($null -ne $ct) -and ([int]$ct.total -eq [int]$m.casos) -and ([int]$ct.passed -eq [int]$m.casos) -and ([int]$ct.failed -eq 0)) 'Suite final incompleta o con fallos.'
    Aprobar ('Compilación y '+$m.casos+' casos sin SQL')
    if (-not $SinSql) {
        $sqlEstado='EN_CURSO'
        Add-Type -AssemblyName System.Data
        $cadena=Crear-CadenaConexion 'LAPTOP-5TSJJUP8\SQLEXPRESS' 'HotelTramontoDB_Pruebas' ([bool]$ConfiarCertificadoLocal)
        $c=New-Object -TypeName System.Data.SqlClient.SqlConnection -ArgumentList $cadena
        $c.Open()
        $cmd=$c.CreateCommand()
        try {
            $cmd.CommandText="SELECT DB_NAME() AS db,CONVERT(nvarchar(128),SERVERPROPERTY('ServerName')) AS servidor;"
            $r=$cmd.ExecuteReader()
            try { Revisar ($r.Read() -and ($r['db'] -ceq 'HotelTramontoDB_Pruebas') -and ($r['servidor'] -ieq 'LAPTOP-5TSJJUP8\SQLEXPRESS')) 'Contexto SQL incorrecto.' }
            finally { $r.Dispose() }
            $cmd.CommandText=[IO.File]::ReadAllText((Join-Path $Raiz 'base de datos HotelTramonto/pruebas-finales/00_VerificarCorreccion2.sql'))
            $cmd.CommandTimeout=120
            $ds=New-Object System.Data.DataSet
            $ad=New-Object -TypeName System.Data.SqlClient.SqlDataAdapter -ArgumentList $cmd
            try { [void]$ad.Fill($ds) } finally { $ad.Dispose() }
            $ds.WriteXml((Join-Path $Salida 'contratos-sql.xml'),[System.Data.XmlWriteMode]::WriteSchema)
            $definiciones=0; $comprobaciones=0
            foreach ($tabla in $ds.Tables) {
                foreach ($fila in $tabla.Rows) {
                    if ($tabla.Columns.Contains('resultado')) {
                        $comprobaciones++
                        Revisar ($fila.resultado -ceq 'APROBADA') ('Contrato SQL incompatible: '+$fila.procedimiento)
                    }
                    if ($tabla.Columns.Contains('definicion')) {
                        $propiedad=$m.definiciones_sql.PSObject.Properties[[string]$fila.procedimiento]
                        Revisar (($null -ne $propiedad) -and ($fila.definicion -isnot [DBNull])) 'Definición no visible/no prevista.'
                        Revisar ((Canon ([string]$fila.definicion)) -ceq [string]$propiedad.Value) ('Definición SQL diferente: '+$fila.procedimiento)
                        $definiciones++
                    }
                    if ($tabla.Columns.Contains('es_sysadmin')) {
                        $pasos.Add([pscustomobject]@{prueba='Seguridad SQL compartida';resultado='LIMITACION';sysadmin=[int]$fila.es_sysadmin;db_owner=[int]$fila.es_db_owner;detalle='No se instaló seguridad por empleado en SQL. Roles textuales en C# no autentican conexiones externas.'})
                    }
                }
            }
            Revisar (($definiciones -eq 19) -and ($comprobaciones -gt 50)) 'Diagnóstico SQL incompleto.'
            $sqlEstado='APROBADA'
            Aprobar ('Metadatos y cuerpos de '+$definiciones+' procedimientos; sin ejecutarlos')
        } finally { $cmd.Dispose() }
    }
} catch {
    if ($sqlEstado -eq 'EN_CURSO') { $sqlEstado='ERROR' }
    $errorFinal=$_.Exception.Message
    $pasos.Add([pscustomobject]@{prueba='Verificación final';resultado='ERROR';detalle=$errorFinal})
    Write-Warning $errorFinal
} finally {
    if ($null -ne $c) { $c.Dispose() }
    [pscustomobject]@{fecha=(Get-Date).ToString('o');exito=($null -eq $errorFinal);sql=$sqlEstado;pasos=$pasos.ToArray();pendiente='Validación visual de Windows Forms. Escrituras SQL no se prueban. No se instala migración ni permisos.'} |
        ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Salida 'informe-final.json') -Encoding UTF8
    Write-Host ('Enviar informe-final.json de: '+$Salida)
}
if ($null -ne $errorFinal) { throw $errorFinal }
