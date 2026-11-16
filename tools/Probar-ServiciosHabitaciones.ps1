#Requires -Version 5.1
<#
SQL Express local. Sin Python/LocalDB/ODBC. El ejecutor .NET conserva todas las barreras.
No tiene parámetros de nombre de base, servidor alternativo ni confirmación automática.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string] $BackupPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Ejecutar desde Windows en LAPTOP-5TSJJUP8.' }
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$repo = Split-Path -Parent $PSScriptRoot
$backup = (Resolve-Path -LiteralPath $BackupPath).ProviderPath
Push-Location $repo
try {
    # La confirmación INSTALAR la solicita el ejecutor ANTES de migrar la copia.
    & $dotnet run --project tests/HotelTramonto.VerificacionSql -- --backup $backup
    $code = $LASTEXITCODE
}
finally { Pop-Location }
exit $code
