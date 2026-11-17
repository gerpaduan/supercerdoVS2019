# Genera el icono redondeado de CarniSys (logo sobre baldosa blanca con esquinas redondeadas y transparencia fuera de la baldosa).
param(
    [Parameter(Mandatory)][string]$LogoPath,
    [Parameter(Mandatory)][string]$OutDir
)
Add-Type -AssemblyName System.Drawing

$tamano = 512
$radio = 112          # ~22 % del lado: igual a los iconos de las mini-apps (stock/movimientos/elaborados)
$escalaLogo = 0.76    # el logo original ocupa todo el cuadro: se achica para dejar aire dentro de la baldosa

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

$logo = [System.Drawing.Image]::FromFile($LogoPath)
$bmp = New-Object System.Drawing.Bitmap $tamano, $tamano, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.InterpolationMode = 'HighQualityBicubic'
$g.PixelOffsetMode = 'HighQuality'
$g.Clear([System.Drawing.Color]::Transparent)

# Baldosa blanca redondeada + borde fino gris azulado
$path = New-RoundedPath 2 2 ($tamano - 4) ($tamano - 4) $radio
$g.FillPath([System.Drawing.Brushes]::White, $path)
$lapiz = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 205, 213, 226)), 4
$g.DrawPath($lapiz, $path)

# Logo centrado
$lado = [int]($tamano * $escalaLogo)
$pos = [int](($tamano - $lado) / 2)
$g.DrawImage($logo, $pos, $pos, $lado, $lado)
$g.Dispose()

$bmp.Save((Join-Path $OutDir 'pwa-rounded-512.png'), [System.Drawing.Imaging.ImageFormat]::Png)

# 192 px a partir del 512
$chico = New-Object System.Drawing.Bitmap 192, 192, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g2 = [System.Drawing.Graphics]::FromImage($chico)
$g2.SmoothingMode = 'AntiAlias'; $g2.InterpolationMode = 'HighQualityBicubic'; $g2.PixelOffsetMode = 'HighQuality'
$g2.Clear([System.Drawing.Color]::Transparent)
$g2.DrawImage($bmp, 0, 0, 192, 192)
$g2.Dispose()
$chico.Save((Join-Path $OutDir 'pwa-rounded-192.png'), [System.Drawing.Imaging.ImageFormat]::Png)

$logo.Dispose(); $bmp.Dispose(); $chico.Dispose(); $lapiz.Dispose(); $path.Dispose()
"ok"
