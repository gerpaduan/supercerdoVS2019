# Genera los iconos PNG de las mini-apps instalables "Punto de venta" (pos), "Punto de expendio" (expendio) y "Existencia por sucursal" (existencia).
# Dibuja la MISMA geometria que los SVG de WebCore/wwwroot/Content/img/ventanas/{pos,expendio,existencia}.svg (viewBox 64x64, baldosa #062d63 con rx=12),
# asi que si se cambia un SVG hay que reflejar el cambio en $apps y volver a correr:
#   powershell -NoProfile -File scripts/make-miniapp-icons.ps1
# Salida (en WebCore/wwwroot/Content/img/ventanas): <app>-192.png y <app>-512.png (esquinas redondeadas, fuera de la baldosa transparente) y
# <app>-512-maskable.png (cuadrado completo, dibujo al ~73 %: el sistema le pone su propia mascara). Sin dependencias: System.Drawing.
param(
    [string]$OutDir = (Join-Path $PSScriptRoot '..\WebCore\wwwroot\Content\img\ventanas')
)
Add-Type -AssemblyName System.Drawing

$azul = [System.Drawing.ColorTranslator]::FromHtml('#062d63')
$naranja = [System.Drawing.ColorTranslator]::FromHtml('#f59e0b')
$blanco = [System.Drawing.Color]::White

# Cada figura: x, y, ancho, alto, radio, color (coordenadas del viewBox 64x64 del SVG)
$apps = @{
    'existencia' = @(
        @(10, 26, 12, 24, 2, $blanco),
        @(26, 12, 12, 38, 2, $naranja),
        @(42, 34, 12, 16, 2, $blanco),
        @(8, 53, 48, 4, 2, $blanco)
    )
    'pos' = @(
        @(10, 12, 44, 14, 3, $naranja),
        @(10, 30, 44, 6, 2, $blanco),
        @(10, 40, 12, 12, 2, $blanco),
        @(26, 40, 12, 12, 2, $blanco),
        @(42, 40, 12, 12, 2, $blanco)
    )
    'expendio' = @(
        @(8, 10, 48, 14, 3, $naranja),
        @(12, 28, 40, 24, 2, $blanco),
        @(38, 34, 10, 18, 1, $azul),
        @(17, 34, 16, 12, 1, $azul)
    )
}

function New-RoundedPath([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 2 * $r
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

# Dibuja una baldosa de $tam px. $maskable = true: fondo cuadrado completo y dibujo reducido al centro.
function New-Icono([object[]]$figuras, [int]$tam, [bool]$maskable) {
    $bmp = New-Object System.Drawing.Bitmap $tam, $tam, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)
    $k = $tam / 64.0
    if ($maskable) {
        $g.FillRectangle((New-Object System.Drawing.SolidBrush $azul), 0, 0, $tam, $tam)
        # reduce el dibujo al 73 % alrededor del centro (zona segura de las mascaras)
        $g.TranslateTransform($tam / 2, $tam / 2); $g.ScaleTransform(0.73, 0.73); $g.TranslateTransform(-$tam / 2, -$tam / 2)
    } else {
        $baldosa = New-RoundedPath 0 0 $tam $tam (12 * $k)
        $g.FillPath((New-Object System.Drawing.SolidBrush $azul), $baldosa)
    }
    foreach ($f in $figuras) {
        $path = New-RoundedPath ($f[0] * $k) ($f[1] * $k) ($f[2] * $k) ($f[3] * $k) ($f[4] * $k)
        $g.FillPath((New-Object System.Drawing.SolidBrush $f[5]), $path)
        $path.Dispose()
    }
    $g.Dispose()
    return $bmp
}

New-Item -ItemType Directory -Force $OutDir | Out-Null
foreach ($nombre in $apps.Keys) {
    foreach ($par in @(@(192, $false, "$nombre-192.png"), @(512, $false, "$nombre-512.png"), @(512, $true, "$nombre-512-maskable.png"))) {
        $bmp = New-Icono $apps[$nombre] $par[0] $par[1]
        $bmp.Save((Join-Path $OutDir $par[2]), [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()
        "generado $($par[2])"
    }
}

