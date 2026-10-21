<#
.SYNOPSIS
  Recompila WebCore en limpio y lo levanta en localhost (entorno Development).

.DESCRIPTION
  Pasos:
    1. Detiene el WebCore anterior de ESTE checkout. Si sigue corriendo bloquea los .dll y la compilacion falla.
    2. dotnet clean + dotnet build (Debug). Las vistas Razor van precompiladas dentro de WebCore.dll: despues de
       tocar un .cshtml hay que compilar si o si, reiniciar el proceso no alcanza (ver docs/DECISIONS.md).
    3. Levanta el servidor con el perfil de WebCore\Properties\launchSettings.json (URL y variables de entorno
       salen de ahi, no se repiten en este script) y abre el navegador cuando el puerto ya responde.
  Ctrl+C (o cerrar la ventana) detiene el servidor. Pensado para correrse con doble clic en webcore-run.cmd.

.PARAMETER Incremental
  Salta el clean: compilacion incremental, bastante mas rapida. Usar el modo limpio si algo "no toma" el cambio.

.PARAMETER NoBrowser
  No abre el navegador al terminar de levantar.

.PARAMETER Perfil
  Perfil de launchSettings.json a usar. Por defecto 'http' (http://localhost:5270).

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Scripts\webcore-run.ps1
  powershell -ExecutionPolicy Bypass -File Scripts\webcore-run.ps1 -Incremental -NoBrowser
#>
param(
    [switch]$Incremental,
    [switch]$NoBrowser,
    [string]$Perfil = 'http'
)

$ErrorActionPreference = 'Stop'

# Segundos maximos de espera a que el servidor abra el puerto antes de darlo por colgado.
$segundosEsperaArranque = 60

$raiz = Split-Path -Parent $PSScriptRoot
$carpetaProyecto = Join-Path $raiz 'WebCore'
$proyecto = Join-Path $carpetaProyecto 'WebCore.csproj'
$archivoLaunchSettings = Join-Path $carpetaProyecto 'Properties\launchSettings.json'
# Binarios de WebCore de este checkout (con "\" final para no confundir con carpetas hermanas tipo bin2).
$carpetaBin = (Join-Path $carpetaProyecto 'bin') + '\'

function Write-Paso {
    param([string]$Mensaje)
    Write-Host ''
    Write-Host "==> $Mensaje" -ForegroundColor Cyan
}

# Devuelve el proceso que escucha en el puerto, o $null si esta libre.
function Get-EscuchaEnPuerto {
    param([int]$Puerto)
    $conexion = Get-NetTCPConnection -LocalPort $Puerto -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $conexion) { return $null }
    return Get-Process -Id $conexion.OwningProcess -ErrorAction SilentlyContinue
}

# Detiene los procesos que ejecutan binarios de WebCore\bin de este checkout (WebCore.exe o "dotnet WebCore.dll").
# No toca otros procesos dotnet (builds de otras sesiones, tests, etc.) ni un WebCore de otro checkout.
function Stop-WebCoreAnterior {
    $anteriores = Get-CimInstance Win32_Process | Where-Object {
        ($_.ExecutablePath -and $_.ExecutablePath.StartsWith($carpetaBin, [StringComparison]::OrdinalIgnoreCase)) -or
        ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -and $_.CommandLine.IndexOf($carpetaBin, [StringComparison]::OrdinalIgnoreCase) -ge 0)
    }
    foreach ($proceso in $anteriores) {
        Write-Host "Deteniendo WebCore anterior: $($proceso.Name) (PID $($proceso.ProcessId))"
        try {
            Stop-Process -Id $proceso.ProcessId -Force -ErrorAction Stop
        }
        catch {
            # Si el proceso ya termino solo entre la consulta y el kill no es un error; cualquier otra causa si.
            if (Get-Process -Id $proceso.ProcessId -ErrorAction SilentlyContinue) { throw }
        }
        Wait-Process -Id $proceso.ProcessId -Timeout 10 -ErrorAction SilentlyContinue
    }
}

# --- Perfil de lanzamiento: de aca salen URL(s) y variables de entorno ---------------------------------------
if (-not (Test-Path -LiteralPath $archivoLaunchSettings)) {
    throw "No se encontro $archivoLaunchSettings"
}
$perfiles = (Get-Content -LiteralPath $archivoLaunchSettings -Raw | ConvertFrom-Json).profiles
$perfilConfig = $perfiles.$Perfil
if (-not $perfilConfig) {
    throw "El perfil '$Perfil' no existe en launchSettings.json (disponibles: $($perfiles.PSObject.Properties.Name -join ', '))."
}
$urls = $perfilConfig.applicationUrl
$urlsLista = $urls -split ';'
$urlPrincipal = $urlsLista[0]
$puertos = $urlsLista | ForEach-Object { ([uri]$_).Port }

# --- 1. Frenar el WebCore anterior ---------------------------------------------------------------------------
Write-Paso 'Deteniendo WebCore anterior'
Stop-WebCoreAnterior
foreach ($puerto in $puertos) {
    $ocupante = Get-EscuchaEnPuerto -Puerto $puerto
    if ($ocupante) {
        throw "El puerto $puerto sigue ocupado por '$($ocupante.ProcessName)' (PID $($ocupante.Id)), que no es un WebCore de este checkout. Cerralo y reintenta."
    }
}

# --- 2. Compilar ---------------------------------------------------------------------------------------------
if (-not $Incremental) {
    Write-Paso 'dotnet clean'
    & dotnet clean $proyecto --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "dotnet clean fallo (codigo $LASTEXITCODE)." }
}
Write-Paso 'dotnet build'
# Solo errores + resumen: la solucion arrastra miles de warnings (nullable, APIs obsoletas) que taparian los errores reales.
& dotnet build $proyecto --nologo -v minimal -clp:"ErrorsOnly;Summary"
if ($LASTEXITCODE -ne 0) { throw "La compilacion de WebCore fallo (codigo $LASTEXITCODE). No se levanta el servidor." }

# --- 3. Levantar ---------------------------------------------------------------------------------------------
Write-Paso "Levantando WebCore en $urlPrincipal"

# Mismo efecto que --launch-profile pero sin que dotnet run decida por su cuenta abrir (o no) el navegador:
# se cargan las variables del perfil y las URLs a mano, y el navegador lo abre este script cuando el puerto responde.
# Se restauran al terminar por si el script se corre desde una consola que sigue viva.
$entornoOriginal = @{}
$variablesPerfil = @{ ASPNETCORE_URLS = $urls }
if ($perfilConfig.environmentVariables) {
    foreach ($variable in $perfilConfig.environmentVariables.PSObject.Properties) {
        $variablesPerfil[$variable.Name] = [string]$variable.Value
    }
}
foreach ($nombre in $variablesPerfil.Keys) {
    $entornoOriginal[$nombre] = [Environment]::GetEnvironmentVariable($nombre)
    [Environment]::SetEnvironmentVariable($nombre, $variablesPerfil[$nombre])
}

$servidor = $null
try {
    # Directorio de trabajo = carpeta del proyecto, igual que cuando se lanza desde VS o "dotnet run" dentro de WebCore.
    $servidor = Start-Process -FilePath 'dotnet' `
        -ArgumentList @('run', '--project', "`"$proyecto`"", '--no-build', '--no-launch-profile') `
        -WorkingDirectory $carpetaProyecto -NoNewWindow -PassThru
    # Con -NoNewWindow -PassThru, PowerShell 5.1 devuelve ExitCode vacio si no se toca Handle antes (bug conocido).
    $null = $servidor.Handle

    # Espera a que Kestrel abra el puerto; si el proceso muere antes (ej. no conecta a la base) se corta con el motivo.
    $limite = (Get-Date).AddSeconds($segundosEsperaArranque)
    while (-not (Get-EscuchaEnPuerto -Puerto $puertos[0])) {
        if ($servidor.HasExited) {
            throw "El servidor termino antes de abrir el puerto (codigo $($servidor.ExitCode)). El motivo esta en el log de arriba."
        }
        if ((Get-Date) -gt $limite) {
            throw "El servidor no abrio el puerto $($puertos[0]) en $segundosEsperaArranque s."
        }
        Start-Sleep -Milliseconds 500
    }

    Write-Host "Servidor arriba en $urlPrincipal. Ctrl+C para detenerlo." -ForegroundColor Green
    if (-not $NoBrowser) { Start-Process $urlPrincipal }

    Wait-Process -Id $servidor.Id
}
finally {
    # Red de seguridad: si algo corto el script con el servidor vivo, que no quede un WebCore huerfano ocupando el puerto.
    # Con Ctrl+C el servidor recibe la senal y cierra solo (se le da 10 s); si no cierra, se mata SOLO su arbol de
    # procesos (no Stop-WebCoreAnterior: otra corrida del script podria haber levantado un WebCore nuevo en el medio).
    if ($servidor -and -not $servidor.WaitForExit(10000)) {
        & taskkill /PID $servidor.Id /T /F | Out-Null
        if ($LASTEXITCODE -ne 0 -and -not $servidor.HasExited) {
            Write-Warning "No se pudo detener el servidor (PID $($servidor.Id)); cerralo a mano."
        }
    }
    foreach ($nombre in $entornoOriginal.Keys) {
        [Environment]::SetEnvironmentVariable($nombre, $entornoOriginal[$nombre])
    }
}
