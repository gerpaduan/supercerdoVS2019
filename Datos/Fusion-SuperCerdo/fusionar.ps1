# Fusion SuperCerdo SM + SL en el SQL Server local.
# Arma la base -Dst como copia de la base de San Martin (-SmDb) y le carga San Lorenzo (-SlDb) con las reglas de
# 01-reglas.sql / 02-fusion.sql; despues corre 03-verificar.sql (el juez). Las bases de origen no se modifican.
# Uso y reglas: docs/07-operacion-y-soporte/despliegue-y-publicacion.md "Unificar SuperCerdo SM + SL" y
# docs/DECISIONS.md "Unificacion SuperCerdo SM + SL" (2026-10-27).
param(
    [Parameter(Mandatory)] [string] $SmDb,          # base de San Martin ya restaurada (ej. SuperCerdo_SM_20261026)
    [Parameter(Mandatory)] [string] $SlDb,          # base de San Lorenzo ya restaurada (ej. SuperCerdo_SL_20261026)
    [Parameter(Mandatory)] [string] $Dst,           # base a crear (se reemplaza si existe)
    [string] $MapDb = 'SuperCerdo_old_20260917',    # fusion del 17/9: de ahi salen las decisiones de _MergeMapPersonas/_MergeMapCorte
    [ValidateSet('corregido', 'replica')] [string] $Modo = 'corregido',
    [string] $Instancia = '.\SQLEXPRESS',
    [string] $LogDir = (Join-Path $PSScriptRoot 'logs'),
    [switch] $SinVerificar
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force $LogDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$log = Join-Path $LogDir "fusion-$Dst-$stamp.log"

# Ejecuta un .sql o una consulta con sqlcmd (autenticacion de Windows contra la instancia local) y corta si falla.
function Invoke-Sql([string] $Archivo, [string] $Consulta, [string] $Base = 'master') {
    $vars = @('-v', "DST=$Dst", "SM=$SmDb", "SL=$SlDb", "MAPDB=$MapDb", "MODO=$Modo")
    # -m 1: oculta los avisos informativos (ej. "Null value is eliminated by an aggregate")
    $sqlArgs = @('-S', $Instancia, '-E', '-C', '-b', '-m', '1', '-l', '30', '-t', '0', '-d', $Base, '-W', '-s', '|') + $vars
    if ($Archivo) { $sqlArgs += @('-i', (Join-Path $PSScriptRoot $Archivo)) } else { $sqlArgs += @('-Q', $Consulta) }
    $salida = & sqlcmd @sqlArgs 2>&1
    $salida | Tee-Object -FilePath $log -Append | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd fallo ($LASTEXITCODE) en $(if ($Archivo) { $Archivo } else { 'consulta' }). Ver $log" }
}

# Guardas: no pisar nunca un origen ni la fusion vieja.
if ($Dst -in @($SmDb, $SlDb, $MapDb)) { throw "-Dst no puede ser una base de origen" }

# Memoria: con poca RAM libre SQL Server Express ya dejo bases en RECOVERY_PENDING (error 701).
$libreMb = [int]((Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory / 1024)
if ($libreMb -lt 700) { Write-Warning "Solo $libreMb MB de RAM libre: cerrar programas antes de seguir puede evitar el error 701 de SQL Server." }

Write-Host "== 1/4 copia de $SmDb como $Dst" -ForegroundColor Cyan
$dataDir = (& sqlcmd -S $Instancia -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(400))").Trim()
$bak = Join-Path $dataDir "fusion_$SmDb.bak"
Invoke-Sql -Consulta "BACKUP DATABASE [$SmDb] TO DISK = N'$bak' WITH COPY_ONLY, INIT, FORMAT"
$archivos = & sqlcmd -S $Instancia -E -C -h -1 -W -s '|' -Q "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'$bak'"
$moves = ($archivos | Where-Object { $_ -match '\|' } | ForEach-Object {
    $p = $_ -split '\|'; $ext = if ($p[2] -eq 'L') { '_log.ldf' } else { '.mdf' }
    # archivos con fecha y hora: nunca chocan con los de otra base (ej. una renombrada que conserva SuperCerdo.mdf)
    "MOVE N'$($p[0])' TO N'$(Join-Path $dataDir "${Dst}_$stamp$ext")'" }) -join ', '
Invoke-Sql -Consulta ("IF DB_ID(N'$Dst') IS NOT NULL BEGIN ALTER DATABASE [$Dst] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Dst]; END; " +
    "RESTORE DATABASE [$Dst] FROM DISK = N'$bak' WITH $moves; ALTER DATABASE [$Dst] SET RECOVERY SIMPLE; ALTER DATABASE [$Dst] SET AUTO_CLOSE OFF;")
Remove-Item $bak -Force

Write-Host "== 2/4 reglas (modo $Modo)" -ForegroundColor Cyan
Invoke-Sql -Archivo '01-reglas.sql'
Write-Host "== 3/4 fusion" -ForegroundColor Cyan
Invoke-Sql -Archivo '02-fusion.sql'
Invoke-Sql -Base $Dst -Consulta "SET NOCOUNT ON; SELECT paso, tabla, filas, LEFT(detalle, 300) detalle FROM fusion.Log ORDER BY id"

if (-not $SinVerificar) {
    Write-Host "== 4/4 verificacion" -ForegroundColor Cyan
    Invoke-Sql -Archivo '03-verificar.sql' -Base $Dst
}
Write-Host "Listo. Log: $log" -ForegroundColor Green
