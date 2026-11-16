#requires -Version 5.1
# Prueba local sin conexión: carga SOLAMENTE la función pura mediante el AST.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$archivo=Join-Path $PSScriptRoot 'Probar-HotelTramonto.ps1'
$tokens=$null; $errores=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile($archivo,[ref]$tokens,[ref]$errores)
if ($errores.Count -ne 0) { throw 'El runner contiene errores de sintaxis.' }
$funciones=@($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Crear-CadenaConexion' },$false))
if ($funciones.Count -ne 1) { throw 'No se encontró exactamente una función de construcción.' }
. ([scriptblock]::Create($funciones[0].Extent.Text))
$casos=0
foreach ($base in @('master','HotelTramontoDB_Pruebas')) {
    foreach ($confiar in @($false,$true)) {
        $cadena=Crear-CadenaConexion 'LAPTOP-5TSJJUP8\SQLEXPRESS' $base $confiar
        $b=New-Object -TypeName System.Data.SqlClient.SqlConnectionStringBuilder -ArgumentList $cadena
        $esperado=@{
            'Data Source'='LAPTOP-5TSJJUP8\SQLEXPRESS'; 'Initial Catalog'=$base
            'Integrated Security'=$true; 'Encrypt'=$true; 'TrustServerCertificate'=$confiar
            'Persist Security Info'=$false; 'Connect Timeout'=15
            'Application Name'='HotelTramonto-Pruebas-Controladas'; 'Pooling'=$false
        }
        foreach ($clave in $esperado.Keys) {
            if ($b[$clave] -cne $esperado[$clave]) { throw ('Valor incorrecto: '+$clave) }
        }
        if ($b['User ID'] -ne '' -or $b['Password'] -ne '') { throw 'La cadena contiene credenciales.' }
        # Crear un objeto cerrado comprueba también el constructor; NUNCA se llama Open.
        $c=New-Object -TypeName System.Data.SqlClient.SqlConnection -ArgumentList $cadena
        try {
            if ($c.State -ne [System.Data.ConnectionState]::Closed) { throw 'Se esperaba una conexión cerrada.' }
        } finally { $c.Dispose() }
        $casos++
    }
}
Write-Output ('APROBADAS: '+$casos+' combinaciones, nueve claves y conexión cerrada. Sin abrir conexiones SQL.')
