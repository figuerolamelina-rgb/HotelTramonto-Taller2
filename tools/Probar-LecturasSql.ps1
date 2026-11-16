#requires -Version 5.1
# Regresión sin SQL: carga solo Canon mediante AST, no ejecuta el runner.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$raiz=Split-Path $PSScriptRoot -Parent
$tokens=$null; $errores=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Probar-HotelTramonto.ps1'),[ref]$tokens,[ref]$errores)
if ($errores.Count -ne 0) { throw 'Sintaxis del runner inválida.' }
$funciones=@($ast.FindAll({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Canon'},$false))
if ($funciones.Count -ne 1) { throw 'No se encontró Canon.' }
. ([scriptblock]::Create($funciones[0].Extent.Text))
$fuente=[IO.File]::ReadAllText((Join-Path $raiz 'base de datos HotelTramonto/Procedimientos_Almacenados.sql'))
$real=@'
CREATE PROCEDURE sp_Habitacion_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT H.id_habitacion, H.tipo_habitacion,
           T.nombre AS tipo, H.piso, H.numero,
           H.estado, H.activo
    FROM Habitaciones H
    INNER JOIN TiposHabitacion T
        ON H.tipo_habitacion = T.id_tipoHabitacion
    WHERE H.activo = 1
    ORDER BY H.numero;
END;
'@
$referencia=[regex]::Match($fuente,'(?ims)^CREATE OR ALTER PROCEDURE sp_Habitacion_Listar\b.*?(?=^GO\s*$)')
if (-not $referencia.Success -or (Canon $real) -cne (Canon $referencia.Value)) {
    throw 'La definición versionada no coincide con la lectura real revisada.'
}
$sinFiltro=[regex]::Replace($real,'(?im)^\s*WHERE H\.activo = 1\s*$','')
if ((Canon $sinFiltro) -ceq (Canon $referencia.Value)) { throw 'La comparación dejó de detectar un filtro ausente.' }
$inactivas=$real.Replace('WHERE H.activo = 1','WHERE H.activo = 0')
if ((Canon $inactivas) -ceq (Canon $referencia.Value)) { throw 'La comparación dejó de detectar otro filtro.' }
$disponibilidad=[regex]::Match($fuente,'(?ims)^CREATE OR ALTER PROCEDURE sp_Habitacion_ConsultarDisponibilidad\b.*?(?=^GO\s*$)')
if (-not $disponibilidad.Success) { throw 'Falta la consulta de disponibilidad.' }
$sha=[Security.Cryptography.SHA256]::Create()
try { $huella=([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes((Canon $disponibilidad.Value))))).Replace('-','') }
finally { $sha.Dispose() }
if ($huella -cne 'BE780CC38DFA6369B2120B47EE5EA5A22CCD1EBD36017184935B2FD4356A9FA8') { throw 'La lógica revisada de disponibilidad cambió.' }
Write-Output 'APROBADAS: definición real activa, rechazo de filtro ausente/invertido y disponibilidad sin cambios. Sin ejecutar SQL.'
