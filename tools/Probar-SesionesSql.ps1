#requires -Version 5.1
# Regresión PURA: no carga el cuerpo del runner, abre SQL ni ejecuta consultas.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$tokens=$null; $errores=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Probar-HotelTramonto.ps1'),[ref]$tokens,[ref]$errores)
if ($errores.Count -ne 0) { throw 'Sintaxis del runner inválida.' }
foreach ($nombre in @('Clasificar-Sesiones','Actualizar-ObservacionesLimpias')) {
    $funciones=@($ast.FindAll({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $nombre},$false))
    if ($funciones.Count -ne 1) { throw ('Función no encontrada: '+$nombre) }
    . ([scriptblock]::Create($funciones[0].Extent.Text))
}
function Sesion([int]$Id,[string]$Guid,[string]$Programa='runner-A',[int]$Proceso=123,[int]$Afecta=1) {
    return [pscustomobject]@{session_id=$Id;connection_id=$Guid;program_name=$Programa;host_process_id=$Proceso;host_name='equipo';status='sleeping';afecta_copia=$Afecta}
}
$registradas=@((Sesion 51 'conn-master'),(Sesion 52 'conn-copia'))
$casos=0
function Revisar([object[]]$Filas,[int]$Propias,[int]$Ajenas) {
    $r=Clasificar-Sesiones -Sesiones $Filas -Registradas $registradas
    if ($r.Propias.Count -ne $Propias -or $r.Ajenas.Count -ne $Ajenas) { throw ('Clasificación incorrecta en caso '+$script:casos) }
    $script:casos++
}
Revisar @() 0 0
Revisar @((Sesion 52 'conn-copia')) 1 0
Revisar $registradas 2 0
Revisar @((Sesion 52 'conn-copia'),(Sesion 80 'externa' 'SSMS' 999)) 1 1
# Ni otro runner ni una conexión del mismo proceso se ignoran sin registro exacto.
Revisar @((Sesion 80 'otra' 'runner-B' 999)) 0 1
Revisar @((Sesion 80 'otra' 'runner-A' 123)) 0 1
# Un SPID reutilizado con nueva conexión y metadatos incompletos son ajenos.
Revisar @((Sesion 52 'conexion-nueva')) 0 1
Revisar @((Sesion 52 '')) 0 1
Revisar @((Sesion 52 'conn-copia' 'runner-A' 999)) 0 1
Revisar @((Sesion 90 'otra-base' 'SSMS' 999 0)) 0 0
# Incluye una petición contra la copia aunque la sesión tenga otro contexto inicial.
Revisar @((Sesion 91 'peticion-en-copia' 'SSMS' 999 1)) 0 1
foreach ($secuencia in @(
    [pscustomobject]@{valores=@(0,0);permitir=$true},
    [pscustomobject]@{valores=@(1,0,0);permitir=$true},
    [pscustomobject]@{valores=@(0,1,0);permitir=$false},
    [pscustomobject]@{valores=@(1,1,1);permitir=$false})) {
    $limpias=0
    foreach ($cantidad in $secuencia.valores) { $limpias=Actualizar-ObservacionesLimpias $limpias $cantidad }
    if (($limpias -ge 2) -ne $secuencia.permitir) { throw 'Política de observaciones incorrecta.' }
    $casos++
}
Write-Output ('APROBADOS: '+$casos+' casos de sesiones propias/ajenas y transitorias. Sin conexión SQL.')
