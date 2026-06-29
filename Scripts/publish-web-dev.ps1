param(
    [string]$ProjectRoot = "C:\Sistemas - Codigo\supercerdoVS2019",
    [string]$WebProjectPath = "C:\Sistemas - Codigo\supercerdoVS2019\Web\Web.csproj",
    [string]$SourceWebPath = "C:\Sistemas - Codigo\supercerdoVS2019\Web",
    [string]$TargetWebPath = "C:\inetpub\wwwroot\CarniSysWeb",
    [string]$MsBuildPath = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe",
    [string]$IisSiteName = "CarniSysWeb",
    [string]$AppPoolName = "",
    [switch]$SkipBuild,
    [switch]$SkipBackup,
    [switch]$RecycleAppPool,
    [switch]$RestartSite
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-PathExists {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Label no encontrado: $Path"
    }
}

function Remove-TargetContents {
    param([string]$Path)

    Get-ChildItem -LiteralPath $Path -Force | ForEach-Object {
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
    }
}

function Import-IisModule {
    if (-not (Get-Module -Name WebAdministration -ListAvailable)) {
        throw "No se encontro el modulo WebAdministration para administrar IIS."
    }

    Import-Module WebAdministration -ErrorAction Stop
}

function Recycle-IisAppPool {
    param([string]$PoolName)

    if ([string]::IsNullOrWhiteSpace($PoolName)) {
        throw "Debe indicar -AppPoolName para reciclar el Application Pool."
    }

    Import-IisModule

    if (-not (Test-Path "IIS:\AppPools\$PoolName")) {
        throw "No existe el Application Pool '$PoolName'."
    }

    Restart-WebAppPool -Name $PoolName
}

function Restart-IisSiteSafe {
    param([string]$SiteName)

    if ([string]::IsNullOrWhiteSpace($SiteName)) {
        throw "Debe indicar -IisSiteName para reiniciar el sitio."
    }

    Import-IisModule

    if (-not (Test-Path "IIS:\Sites\$SiteName")) {
        throw "No existe el sitio IIS '$SiteName'."
    }

    Stop-Website -Name $SiteName
    Start-Sleep -Seconds 1
    Start-Website -Name $SiteName
}

Assert-PathExists -Path $ProjectRoot -Label "ProjectRoot"
Assert-PathExists -Path $WebProjectPath -Label "Web.csproj"
Assert-PathExists -Path $SourceWebPath -Label "Carpeta Web origen"
Assert-PathExists -Path $MsBuildPath -Label "MSBuild"

if (-not (Test-Path -LiteralPath $TargetWebPath)) {
    New-Item -ItemType Directory -Path $TargetWebPath | Out-Null
}

if (-not $SkipBuild) {
    Write-Step "Compilando Web.csproj"
    & $MsBuildPath $WebProjectPath /t:Build /p:Configuration=Debug /nologo
    if ($LASTEXITCODE -ne 0) {
        throw "La compilacion de Web.csproj fallo."
    }
}

if (-not $SkipBackup -and (Get-ChildItem -LiteralPath $TargetWebPath -Force | Select-Object -First 1)) {
    $backupPath = "{0}_backup_{1}" -f $TargetWebPath, (Get-Date -Format "yyyyMMdd_HHmmss")
    Write-Step "Generando backup en $backupPath"
    Copy-Item -LiteralPath $TargetWebPath -Destination $backupPath -Recurse
}

Write-Step "Limpiando destino $TargetWebPath"
Remove-TargetContents -Path $TargetWebPath

Write-Step "Copiando sitio completo"
Copy-Item -Path (Join-Path $SourceWebPath "*") -Destination $TargetWebPath -Recurse -Force

Write-Step "Verificando archivos clave"
$checkFiles = @(
    (Join-Path $TargetWebPath "bin\Web.dll"),
    (Join-Path $TargetWebPath "Views\Ventas\_TablaVentas.cshtml"),
    (Join-Path $TargetWebPath "Views\Ventas\_ModalComentarioVenta.cshtml")
)

$checkFiles | ForEach-Object {
    Assert-PathExists -Path $_ -Label "Archivo publicado"
}

Get-Item -LiteralPath $checkFiles | Select-Object FullName, LastWriteTime, Length | Format-Table -AutoSize

if ($RecycleAppPool) {
    Write-Step "Reciclando Application Pool $AppPoolName"
    Recycle-IisAppPool -PoolName $AppPoolName
}

if ($RestartSite) {
    Write-Step "Reiniciando sitio IIS $IisSiteName"
    Restart-IisSiteSafe -SiteName $IisSiteName
}

Write-Step "Publicacion finalizada"
Write-Host "Recomendado: reciclar el sitio/app pool y luego probar con Ctrl+F5." -ForegroundColor Yellow
