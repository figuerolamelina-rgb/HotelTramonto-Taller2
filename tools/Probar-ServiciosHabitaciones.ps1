#Requires -Version 5.1
<#
Solo Windows. Nunca conecta a una instancia existente ni crea backups del origen.
Restaura un .bak en LocalDB NUEVO, confirma antes del DDL y destruye la copia.
No existe una opción para omitir la confirmación.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string] $BackupPath,
    [ValidateNotNullOrEmpty()]
    [string] $Python = 'python'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Esta automatización requiere Windows.' }
Add-Type -AssemblyName System.Data
$repo = Split-Path -Parent $PSScriptRoot
$sqlDir = Join-Path $repo 'base de datos HotelTramonto\servicios-habitaciones'
$backup = (Resolve-Path -LiteralPath $BackupPath).ProviderPath
if ([IO.Path]::GetExtension($backup) -ne '.bak') { throw 'Se requiere un respaldo .bak de pruebas.' }
$localDb = (Get-Command SqlLocalDB.exe -ErrorAction Stop).Source
$pythonExe = (Get-Command $Python -ErrorAction Stop).Source
$null = Get-Command dotnet -ErrorAction Stop
$runId = [Guid]::NewGuid().ToString()
$instance = 'HT56_' + $runId.Replace('-', '')
$runDir = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('HotelTramonto\Verificaciones\' + $runId)
$dataDir = Join-Path $runDir 'DatosDesechables'
$null = New-Item -ItemType Directory -Path $dataDir -Force
$created = $false
$pipe = $null
$previousConnection = $env:HOTELTRAMONTO_SQL_TEST_CONNECTION
$previousRun = $env:HOTELTRAMONTO_TEST_RUN_ID
$previousResult = $env:HOTELTRAMONTO_TEST_RESULT_FILE
$success = $false
$cleanupSucceeded = $false

function Invoke-Checked {
    param([string] $Executable, [string[]] $Arguments, [string] $LogName)
    $log = Join-Path $runDir $LogName
    $previousPreference = $ErrorActionPreference
    try {
        # Windows PowerShell 5.1 no debe tratar stderr de unittest como fallo por sí solo.
        $ErrorActionPreference = 'Continue'
        & $Executable @Arguments *> $log
        $exitCode = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $previousPreference }
    if ($exitCode -ne 0) { throw "Falló $LogName. Revisar el informe privado: $log" }
}

function New-CopyConnection {
    param([string] $Database = 'HotelTramontoDB_Pruebas')
    if (-not $script:created -or $script:instance -notmatch '^HT56_[a-f0-9]{32}$' -or
        $script:pipe -notmatch '^np:\\\\\.\\pipe\\LOCALDB#[^\r\n]+\\tsql\\query$') {
        throw 'Destino fuera de la instancia desechable creada por esta ejecución.'
    }
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder.DataSource = $script:pipe
    $builder.InitialCatalog = $Database
    $builder.IntegratedSecurity = $true
    $builder.Pooling = $false
    $builder.ConnectTimeout = 30
    # Named pipe LOCAL con identidad Windows: no conexión TCP ni servidor remoto.
    return New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
}

function Invoke-CopyQuery {
    param([string] $Text, [string] $Database = 'HotelTramontoDB_Pruebas', [hashtable] $Parameters = @{})
    $connection = New-CopyConnection $Database
    try {
        $connection.Open()
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = $Text
        $cmd.CommandTimeout = 300
        foreach ($key in $Parameters.Keys) { $null = $cmd.Parameters.AddWithValue($key, $Parameters[$key]) }
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
        $set = New-Object System.Data.DataSet
        $null = $adapter.Fill($set)
        return ,$set
    }
    finally { $connection.Dispose() }
}

function Invoke-Migration {
    param([string] $Name)
    $path = Join-Path $sqlDir $Name
    $text = [IO.File]::ReadAllText($path)
    if ($text -match '(?im)^\s*(GO|USE)\b' -or
        -not $text.Contains("DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'")) {
        throw "Contrato de seguridad inválido: $Name"
    }
    $results = Invoke-CopyQuery $text
    $results.WriteXml((Join-Path $runDir ($Name + '.xml')), [System.Data.XmlWriteMode]::WriteSchema)
}

function Quote-SqlName {
    param([string] $Value)
    return '[' + $Value.Replace(']', ']]') + ']'
}

function Get-HistoryBaseline {
    $tables = @{
        Roles = 'id_rol'; Usuarios = 'id_usuario'; Huespedes = 'id_huesped'
        TiposHabitacion = 'id_tipoHabitacion'; Habitaciones = 'id_habitacion'
        Servicios = 'id_servicio'; Reservas = 'id_reserva'; DetalleReservas = 'id_detalle_reserva'
        ReservasTemporales = 'id_reserva_temporal'; DetalleServicios = 'id_detalle_servicio'; Pagos = 'id_pago'
    }
    $baseline = @{}
    foreach ($table in $tables.Keys) {
        $data = (Invoke-CopyQuery ('SELECT * FROM dbo.' + (Quote-SqlName $table))).Tables[0]
        $rows = @{}
        foreach ($row in $data.Rows) {
            $key = [string]$row[$tables[$table]]
            $rows[$key] = ConvertTo-Json -InputObject $row.ItemArray -Compress -Depth 10
        }
        $baseline[$table] = @{
            Key = $tables[$table]; Columns = @($data.Columns | ForEach-Object { $_.ColumnName }); Rows = $rows
        }
    }
    return $baseline
}

function Assert-HistoryUnchanged {
    param([hashtable] $Baseline)
    foreach ($table in $Baseline.Keys) {
        $before = $Baseline[$table]
        $columns = ($before.Columns | ForEach-Object { Quote-SqlName $_ }) -join ','
        $data = (Invoke-CopyQuery ('SELECT ' + $columns + ' FROM dbo.' + (Quote-SqlName $table))).Tables[0]
        $current = @{}
        foreach ($row in $data.Rows) {
            $current[[string]$row[$before.Key]] = ConvertTo-Json -InputObject $row.ItemArray -Compress -Depth 10
        }
        foreach ($key in $before.Rows.Keys) {
            if (-not $current.ContainsKey($key) -or $current[$key] -cne $before.Rows[$key]) {
                throw "Se modificó o eliminó un registro histórico de $table. La copia será descartada."
            }
        }
    }
}

Push-Location $repo
try {
    # Requisitos antes de crear la copia. No instala paquetes automáticamente.
    Invoke-Checked $pythonExe @('-c', "import sys,pyodbc; assert sys.version_info >= (3,10); assert 'ODBC Driver 18 for SQL Server' in pyodbc.drivers()") 'requisitos-python.log'
    Invoke-Checked 'dotnet' @('restore', 'HotelTramonto.slnx') 'restore.log'
    Invoke-Checked 'dotnet' @('build', 'HotelTramonto.slnx', '--no-restore') 'build.log'
    $filter = 'FullyQualifiedName!~ConexionBD_ConectaConHotelTramontoDB&FullyQualifiedName!~UsuarioNegocios_CambiarContrasena_GuardaHashValido&FullyQualifiedName!~UsuarioNegocios_AutenticarConEmailInexistente_RetornaNull'
    Invoke-Checked 'dotnet' @('test', 'HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj', '--no-build', '--no-restore', '--filter', $filter, '--logger', 'trx;LogFileName=tests-dotnet.trx', '--results-directory', $runDir) 'tests-dotnet.log'
    [xml]$trx = Get-Content -LiteralPath (Join-Path $runDir 'tests-dotnet.trx') -Raw
    $counters = $trx.TestRun.ResultSummary.Counters
    if ([int]$counters.executed -le 0 -or [int]$counters.failed -ne 0 -or
        [int]$counters.error -ne 0 -or [int]$counters.timeout -ne 0 -or [int]$counters.notExecuted -ne 0) {
        throw 'Las pruebas .NET no completaron una ejecución real sin fallos ni omitidas.'
    }
    foreach ($file in @('verificar_historial_sql.py', 'verificar_servicios_habitaciones.py', 'verificar_concurrencia_pagos.py', 'verificar_automatizacion_windows.py')) {
        Invoke-Checked $pythonExe @((Join-Path $PSScriptRoot $file)) ($file + '.log')
    }

    Invoke-Checked $localDb @('create', $instance) 'localdb-create.log'
    $created = $true
    Invoke-Checked $localDb @('start', $instance) 'localdb-start.log'
    $info = & $localDb info $instance 2>&1
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo identificar la instancia LocalDB nueva.' }
    $match = [regex]::Match(($info -join [Environment]::NewLine), 'np:\\\\\.\\pipe\\LOCALDB#[^\r\n]+?\\tsql\\query')
    if (-not $match.Success) { throw 'No se encontró el named pipe local de la instancia nueva.' }
    $pipe = $match.Value

    $metadata = Invoke-CopyQuery 'SELECT CONVERT(int,SERVERPROPERTY(''IsLocalDB'')) AS EsLocalDB;' 'master'
    if ($metadata.Tables[0].Rows[0].EsLocalDB -ne 1) { throw 'El motor no es LocalDB.' }
    $header = Invoke-CopyQuery 'RESTORE HEADERONLY FROM DISK=@backup;' 'master' @{ '@backup' = $backup }
    if ($header.Tables[0].Rows.Count -ne 1 -or
        $header.Tables[0].Rows[0].DatabaseName -cne 'HotelTramontoDB_Pruebas' -or
        $header.Tables[0].Rows[0].BackupType -ne 1) {
        throw 'Se requiere un archivo con un único respaldo FULL de HotelTramontoDB_Pruebas.'
    }
    $files = (Invoke-CopyQuery 'RESTORE FILELISTONLY FROM DISK=@backup;' 'master' @{ '@backup' = $backup }).Tables[0]
    $moves = @()
    $index = 0
    foreach ($file in $files.Rows) {
        if ($file.Type -notin @('D', 'L')) { throw 'El respaldo usa archivos no soportados por esta automatización.' }
        $extension = if ($file.Type -eq 'L') { '.ldf' } else { '.mdf' }
        $destination = Join-Path $dataDir ("archivo-$index" + $extension)
        $logical = ([string]$file.LogicalName).Replace("'", "''")
        $moves += "MOVE N'$logical' TO N'$($destination.Replace("'", "''"))'"
        $index++
    }
    if ($moves.Count -lt 2) { throw 'Respaldo sin los archivos esperados.' }
    # Sin REPLACE: nunca sobrescribe una base, incluso dentro de la instancia desechable.
    $restore = 'RESTORE DATABASE [HotelTramontoDB_Pruebas] FROM DISK=@backup WITH ' + ($moves -join ',') + ', RECOVERY;'
    $null = Invoke-CopyQuery $restore 'master' @{ '@backup' = $backup }
    $database = Invoke-CopyQuery 'SELECT DB_NAME() AS Nombre;'
    if ($database.Tables[0].Rows[0].Nombre -cne 'HotelTramontoDB_Pruebas') { throw 'Base restaurada inesperada.' }

    $null = Invoke-CopyQuery @'
IF EXISTS(SELECT 1 FROM sys.servers WHERE server_id<>0)
 OR EXISTS(SELECT 1 FROM sys.triggers WHERE is_ms_shipped=0 AND is_disabled=0)
 OR EXISTS(SELECT 1 FROM sys.synonyms)
 OR EXISTS(SELECT 1 FROM sys.assemblies WHERE is_user_defined=1)
 OR EXISTS(SELECT 1 FROM sys.databases WHERE database_id=DB_ID() AND is_trustworthy_on=1)
 THROW 51500, 'La copia contiene automatismos o accesos externos que requieren revision manual.', 1;
'@
    # Python exige LocalDB + nombre + GUID de esta ejecución.
    $null = Invoke-CopyQuery @'
IF EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'HotelTramonto_TestRun')
 EXEC sys.sp_dropextendedproperty @name=N'HotelTramonto_TestRun';
EXEC sys.sp_addextendedproperty @name=N'HotelTramonto_TestRun',@value=@run;
'@ 'HotelTramontoDB_Pruebas' @{ '@run' = $runId }
    Invoke-Migration '00_Diagnostico.sql'
    $baseline = Get-HistoryBaseline

    Write-Host 'Copia desechable lista. El origen y el archivo .bak no se modificarán.'
    Write-Host 'Se instalará 01 -> 02 -> 04 -> 05 y se ejecutarán exactamente 28 pruebas SQL.'
    Write-Host 'La copia y los fixtures se eliminarán al terminar. Los informes quedarán fuera de Git.'
    $answer = Read-Host 'Escribí INSTALAR para autorizar la instalación en esta copia'
    if ($answer -cne 'INSTALAR') { throw 'Instalación cancelada; no se ejecutó la migración.' }

    foreach ($script in @('01_Instalar.sql', '02_Verificar.sql', '04_ConcurrenciaPagos.sql', '05_VerificarConcurrencia.sql')) {
        Invoke-Migration $script
    }
    Assert-HistoryUnchanged $baseline

    $env:HOTELTRAMONTO_SQL_TEST_CONNECTION = 'DRIVER={ODBC Driver 18 for SQL Server};SERVER=' + $pipe + ';DATABASE=HotelTramontoDB_Pruebas;Trusted_Connection=Yes;Encrypt=Optional;'
    $env:HOTELTRAMONTO_TEST_RUN_ID = $runId
    $env:HOTELTRAMONTO_TEST_RESULT_FILE = Join-Path $runDir 'tests-sql.json'
    Invoke-Checked $pythonExe @('tests/servicios_habitaciones_sql.py', '--ejecutar-en-copia-desechable') 'tests-sql.log'
    $summary = Get-Content -LiteralPath $env:HOTELTRAMONTO_TEST_RESULT_FILE -Raw | ConvertFrom-Json
    if ($summary.expected -ne 28 -or $summary.executed -ne 28 -or $summary.failures -ne 0 -or
        $summary.errors -ne 0 -or $summary.skipped -ne 0) {
        throw 'La suite SQL no completó las 28 pruebas correctamente.'
    }
    Assert-HistoryUnchanged $baseline
    Invoke-Migration '02_Verificar.sql'
    Invoke-Migration '05_VerificarConcurrencia.sql'
    'Filas históricas originales intactas en las once tablas comparadas.' | Set-Content (Join-Path $runDir 'integridad-historica.txt')
    $success = $true
}
finally {
    $env:HOTELTRAMONTO_SQL_TEST_CONNECTION = $previousConnection
    $env:HOTELTRAMONTO_TEST_RUN_ID = $previousRun
    $env:HOTELTRAMONTO_TEST_RESULT_FILE = $previousResult
    Pop-Location
    [System.Data.SqlClient.SqlConnection]::ClearAllPools()
    if ($created -and $instance -match '^HT56_[a-f0-9]{32}$') {
        & $localDb stop $instance -k *> (Join-Path $runDir 'localdb-stop.log')
        $stopCode = $LASTEXITCODE
        if ($stopCode -eq 0) {
            & $localDb delete $instance *> (Join-Path $runDir 'localdb-delete.log')
            if ($LASTEXITCODE -eq 0) {
                Remove-Item -LiteralPath $dataDir -Recurse -Force
                $cleanupSucceeded = $true
            }
        }
    }
    Write-Host "Informes privados: $runDir"
    if ($created -and -not $cleanupSucceeded) {
        Write-Warning "No se completó la limpieza de la copia $instance; revisar los informes. El origen no fue conectado."
        if ($success) { throw 'Pruebas completadas, pero limpieza pendiente.' }
    }
}
if ($success) { Write-Host 'Verificación completada: 28 pruebas SQL aprobadas y copia desechable eliminada.' }
