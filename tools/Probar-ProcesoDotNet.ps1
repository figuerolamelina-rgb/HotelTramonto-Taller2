#requires -Version 5.1
# Prueba de procesos locales simulados. No usa dotnet real ni SQL Server.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$tokens=$null; $errores=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Probar-HotelTramonto.ps1'),[ref]$tokens,[ref]$errores)
if ($errores.Count -ne 0) { throw 'Sintaxis inválida del runner.' }
foreach ($nombre in @('Convertir-ArgumentoWindows','Invoke-HotelDotNet')) {
    $funciones=@($ast.FindAll({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $nombre},$false))
    if ($funciones.Count -ne 1) { throw ('Función no encontrada: '+$nombre) }
    . ([scriptblock]::Create($funciones[0].Extent.Text))
}
$temporal=Join-Path ([IO.Path]::GetTempPath()) ('tramonto-proceso-'+[Guid]::NewGuid().ToString('N'))
$Raiz=Join-Path $temporal 'repositorio con espacios'
$Salida=Join-Path $temporal 'informes con espacios'
$otra=Join-Path $temporal 'directorio llamador'
[void][IO.Directory]::CreateDirectory($Raiz)
[void][IO.Directory]::CreateDirectory($Salida)
[void][IO.Directory]::CreateDirectory($otra)
$solucion=Join-Path $Raiz 'HotelTramonto.slnx'
[IO.File]::WriteAllText($solucion,'SOLUCION SENTINELA')
$clase='ProcesoPrueba'+[Guid]::NewGuid().ToString('N')
$exe=Join-Path $temporal 'dotnet simulado.exe'
$codigo=@'
using System;
using System.Text;
public class CLASE {
    public static int Main(string[] args) {
        Console.WriteLine("CWD="+Convert.ToBase64String(Encoding.UTF8.GetBytes(Environment.CurrentDirectory)));
        foreach (string arg in args) Console.WriteLine("ARG="+Convert.ToBase64String(Encoding.UTF8.GetBytes(arg)));
        Console.Error.WriteLine("Aviso simulado por stderr con salida cero");
        if (Array.IndexOf(args,"--volumen")>=0) {
            Console.WriteLine(new string('X',131072)); Console.Error.WriteLine(new string('Y',131072));
        }
        if (Array.IndexOf(args,"--fallar")>=0) {
            Console.Error.WriteLine("ERROR REAL SIMULADO: fallo de prueba"); return 7;
        }
        return 0;
    }
}
'@
try {
    Add-Type -TypeDefinition ($codigo.Replace('CLASE',$clase)) -OutputAssembly $exe -OutputType ConsoleApplication
    # Un nombre de función heredado no debe capturar el ejecutable.
    function DotNet { throw 'Se invocó la función heredada en vez del proceso.' }
    Push-Location $otra
    try {
        $casos=0
        $comandos=@(
            [pscustomobject]@{Args=@('restore','HotelTramonto.slnx');Log='restore.log'},
            [pscustomobject]@{Args=@('build','HotelTramonto.slnx','--no-restore','--volumen');Log='build.log'},
            [pscustomobject]@{Args=@('test','HotelTramonto.Pruebas/HotelTramonto.Pruebas.csproj','--filter','A|B','--logger','trx;LogFileName=seguras.trx','--results-directory',$Salida,'a"b','C:\ruta con espacios\','');Log='tests.log'}
        )
        foreach ($comando in $comandos) {
            Invoke-HotelDotNet -Argumentos $comando.Args -Log $comando.Log -Ejecutable $exe
            $contenido=[IO.File]::ReadAllText((Join-Path $Salida $comando.Log))
            $cwd=[regex]::Match($contenido,'(?m)^CWD=([^\r\n]*)').Groups[1].Value
            if ([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($cwd)) -cne $Raiz) { throw 'Directorio de trabajo incorrecto.' }
            $recibidos=@([regex]::Matches($contenido,'(?m)^ARG=([^\r\n]*)') | ForEach-Object { [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($_.Groups[1].Value)) })
            if ($recibidos.Count -ne $comando.Args.Count) { throw 'Cantidad de argumentos incorrecta.' }
            for ($i=0;$i -lt $recibidos.Count;$i++) { if ($recibidos[$i] -cne $comando.Args[$i]) { throw ('Argumento alterado: '+$i) } }
            if ($contenido -notmatch 'STDERR:' -or $contenido -notmatch 'ExitCode: 0') { throw 'Log incompleto.' }
            if ((Get-Location).Path -cne $otra) { throw 'Se modificó el directorio del llamador.' }
            $casos++
        }
        $falloEsperado=$false
        try { Invoke-HotelDotNet -Argumentos @('build','--fallar') -Log 'build.log' -Ejecutable $exe }
        catch {
            if ($_.Exception.Message -notmatch 'código 7' -or $_.Exception.Message -notmatch 'ERROR REAL SIMULADO') { throw }
            $falloEsperado=$true
        }
        if (-not $falloEsperado) { throw 'No se propagó el fallo del proceso.' }
        $casos++
        $falloEsperado=$false
        try { Invoke-HotelDotNet -Argumentos @('restore') -Log 'restore.log' -Ejecutable (Join-Path $temporal 'no-existe.exe') }
        catch { $falloEsperado=$true }
        if (-not $falloEsperado -or ([IO.File]::ReadAllText((Join-Path $Salida 'restore.log'))) -notmatch 'ERROR:') { throw 'No se registró el fallo de inicio.' }
        $casos++
        $falloEsperado=$false
        try { Invoke-HotelDotNet -Argumentos @('restore') -Log 'HotelTramonto.slnx' -Ejecutable $exe }
        catch { $falloEsperado=$true }
        if (-not $falloEsperado) { throw 'Se admitió un nombre de log peligroso.' }
        $casos++
        if ([IO.File]::Exists((Join-Path $Salida 'HotelTramonto.slnx'))) { throw 'Se creó una solución en Informes.' }
        if ([IO.File]::ReadAllText($solucion) -cne 'SOLUCION SENTINELA') { throw 'Se alteró la solución.' }
        Write-Output ('APROBADOS: '+$casos+' casos de procesos, argumentos, directorios, logs y errores. Sin SQL.')
    } finally { Pop-Location }
} finally {
    if ([IO.Directory]::Exists($temporal)) { Remove-Item -LiteralPath $temporal -Recurse -Force }
}
