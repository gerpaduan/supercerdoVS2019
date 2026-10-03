<#
.SYNOPSIS
  Control de cobertura y vigencia de la ayuda / manual de uso (docs/11-manual-de-uso).

.DESCRIPTION
  Pensado para correrse a mano antes de cada deploy (el repo no tiene CI, ver CLAUDE.md §7). Usa git y
  no tiene dependencias. Informa:
    (a) pantallas (vistas de WebCore/Views, sin parciales) que NO tienen .md de ayuda;
    (b) .md de pantalla cuya vista ya NO existe (y no figura como alias de otra);
    (c) .md posiblemente DESACTUALIZADOS: la vista (o su controller) cambio en git despues de la fecha
        "revisada" del .md;
    (d) regenera docs/11-manual-de-uso/README.md con la cobertura y el indice por rol.
  Limitacion conocida: la clave de la ayuda es controller.action de la RUTA, y una vista puede ser
  renderizada por otra accion (ej. Usuarios.Guardar muestra Usuarios/Editar). Para eso el .md declara
  "alias: Controller.Accion, ..." en su encabezado. El script solo conoce el par Carpeta.Vista.

.PARAMETER SinReadme
  No regenera el README.md del manual.

.PARAMETER Estricto
  Sale con codigo 1 si hay .md huerfanos o desactualizados (para usarlo en un hook propio).

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts\check-ayuda.ps1
#>
param(
    [switch]$SinReadme,
    [switch]$Estricto
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$vistas = Join-Path $raiz 'WebCore\Views'
$controllers = Join-Path $raiz 'WebCore\Controllers'
$manual = Join-Path $raiz 'docs\11-manual-de-uso'

# Carpetas de Views que no son pantallas con ayuda propia.
$carpetasExcluidas = @('Shared', 'Landing', 'Ayuda')
# Vistas sueltas que no son pantallas navegables.
$vistasExcluidas = @('AccesoDenegado', 'Error', 'Privacy')

function Leer-Encabezado([string]$ruta) {
    $meta = @{}
    $lineas = [System.IO.File]::ReadAllLines($ruta, [System.Text.Encoding]::UTF8)
    if ($lineas.Count -eq 0 -or $lineas[0].Trim() -ne '---') { return $meta }
    for ($i = 1; $i -lt $lineas.Count; $i++) {
        if ($lineas[$i].Trim() -eq '---') { break }
        $l = $lineas[$i]
        $p = $l.IndexOf(':')
        if ($p -le 0) { continue }
        $valor = $l.Substring($p + 1)
        $n = $valor.IndexOf(' #')
        if ($n -ge 0) { $valor = $valor.Substring(0, $n) }
        $meta[$l.Substring(0, $p).Trim().ToLowerInvariant()] = $valor.Trim()
    }
    return $meta
}

function Fecha-Git([string]$ruta) {
    # Fecha (yyyy-MM-dd) del ultimo commit que toco el archivo; $null si no esta en git.
    if (-not (Test-Path $ruta)) { return $null }
    Push-Location $raiz
    try {
        $f = git log -1 --format=%cs -- $ruta 2>$null
        if ([string]::IsNullOrWhiteSpace($f)) { return $null }
        return $f.Trim()
    } finally { Pop-Location }
}

# ---- Documentos de ayuda ----
$docs = @()
foreach ($tipo in @('pantallas', 'conceptos', 'referencia')) {
    $dir = Join-Path $manual $tipo
    if (-not (Test-Path $dir)) { continue }
    foreach ($f in Get-ChildItem $dir -Filter *.md -File) {
        $m = Leer-Encabezado $f.FullName
        $clave = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
        if ($tipo -eq 'pantallas' -and $m.ContainsKey('pantalla')) { $clave = $m['pantalla'] }
        $alias = @()
        if ($m.ContainsKey('alias') -and $m['alias']) { $alias = $m['alias'].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ } }
        $docs += [pscustomobject]@{
            Tipo = $tipo; Archivo = $f.Name; Ruta = $f.FullName; Clave = $clave; Alias = $alias
            Titulo = if ($m['titulo']) { $m['titulo'] } else { $clave }
            Rol = if ($m['rol']) { $m['rol'] } else { 'usuario' }
            Modulo = $m['modulo']; Revisada = $m['revisada']
        }
    }
}
$pantallasDoc = $docs | Where-Object { $_.Tipo -eq 'pantallas' }

# ---- Vistas de la app ----
$vistasApp = @()
foreach ($carpeta in Get-ChildItem $vistas -Directory) {
    if ($carpetasExcluidas -contains $carpeta.Name) { continue }
    foreach ($v in Get-ChildItem $carpeta.FullName -Filter *.cshtml -File) {
        $nombre = [System.IO.Path]::GetFileNameWithoutExtension($v.Name)
        if ($nombre.StartsWith('_') -or ($vistasExcluidas -contains $nombre)) { continue }
        $vistasApp += [pscustomobject]@{ Clave = "$($carpeta.Name).$nombre"; Carpeta = $carpeta.Name; Vista = $nombre; Ruta = $v.FullName }
    }
}

$clavesDoc = @{}
foreach ($d in $pantallasDoc) {
    $clavesDoc[$d.Clave.ToLowerInvariant()] = $d
    foreach ($a in $d.Alias) { $clavesDoc[$a.ToLowerInvariant()] = $d }
}
$clavesVista = @{}
foreach ($v in $vistasApp) { $clavesVista[$v.Clave.ToLowerInvariant()] = $v }

# (a) vistas sin ayuda
$sinAyuda = $vistasApp | Where-Object { -not $clavesDoc.ContainsKey($_.Clave.ToLowerInvariant()) } | Sort-Object Clave

# (b) .md de pantalla huerfanos: ni su clave ni ningun alias existe como vista
$huerfanos = @()
foreach ($d in $pantallasDoc) {
    $existe = $clavesVista.ContainsKey($d.Clave.ToLowerInvariant())
    if (-not $existe) { foreach ($a in $d.Alias) { if ($clavesVista.ContainsKey($a.ToLowerInvariant())) { $existe = $true } } }
    if (-not $existe) { $huerfanos += $d }
}

# (c) desactualizados
$desactualizados = @()
foreach ($d in $pantallasDoc) {
    if (-not $d.Revisada) { $desactualizados += [pscustomobject]@{ Clave = $d.Clave; Motivo = 'sin fecha "revisada" en el encabezado'; Nivel = 'REVISAR' }; continue }
    $v = $clavesVista[$d.Clave.ToLowerInvariant()]
    if (-not $v) { continue }
    $fv = Fecha-Git $v.Ruta
    if ($fv -and $fv -gt $d.Revisada) {
        $desactualizados += [pscustomobject]@{ Clave = $d.Clave; Motivo = "la vista cambio el $fv (ayuda revisada el $($d.Revisada))"; Nivel = 'REVISAR' }
    }
    $ctrl = Join-Path $controllers "$($v.Carpeta)Controller.cs"
    $fc = Fecha-Git $ctrl
    if ($fc -and $fc -gt $d.Revisada) {
        $desactualizados += [pscustomobject]@{ Clave = $d.Clave; Motivo = "el controller cambio el $fc (ayuda revisada el $($d.Revisada))"; Nivel = 'revisar (controller)' }
    }
}

# ---- Informe ----
$total = $vistasApp.Count
$cubiertas = $total - $sinAyuda.Count
Write-Host ''
Write-Host "Ayuda / manual de uso -- cobertura: $cubiertas de $total pantallas con ayuda ($([math]::Round(100.0 * $cubiertas / [math]::Max($total,1)))%)" -ForegroundColor Cyan
Write-Host "Documentos: $(($docs | Where-Object {$_.Tipo -eq 'pantallas'}).Count) pantallas, $(($docs | Where-Object {$_.Tipo -eq 'conceptos'}).Count) conceptos, $(($docs | Where-Object {$_.Tipo -eq 'referencia'}).Count) referencias"

if ($huerfanos.Count -gt 0) {
    Write-Host "`n[HUERFANOS] .md de pantalla cuya vista ya no existe (borrar o corregir 'pantalla:'/'alias:'):" -ForegroundColor Red
    $huerfanos | ForEach-Object { Write-Host "  - $($_.Archivo)  ($($_.Clave))" }
}
if ($desactualizados.Count -gt 0) {
    Write-Host "`n[DESACTUALIZADOS?] el codigo cambio despues de la ultima revision de la ayuda:" -ForegroundColor Yellow
    $desactualizados | ForEach-Object { Write-Host "  - $($_.Clave): $($_.Motivo) [$($_.Nivel)]" }
}
if ($sinAyuda.Count -gt 0) {
    Write-Host "`n[SIN AYUDA] pantallas sin .md ($($sinAyuda.Count)):" -ForegroundColor DarkYellow
    ($sinAyuda | Group-Object Carpeta | Sort-Object Name) | ForEach-Object { Write-Host ("  {0}: {1}" -f $_.Name, (($_.Group | ForEach-Object { $_.Vista }) -join ', ')) }
}
if ($huerfanos.Count -eq 0 -and $desactualizados.Count -eq 0) { Write-Host "`nSin huerfanos ni desactualizados." -ForegroundColor Green }

# ---- README del manual ----
if (-not $SinReadme) {
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('# Manual de uso y ayuda por pantalla')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('> Este archivo lo **regenera** `scripts/check-ayuda.ps1`: no editarlo a mano.')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('Fuente de verdad de la ayuda que muestra la app (boton **Ayuda** de la barra superior, panel lateral encima de la pantalla actual, y los manuales por rol en `/Ayuda`). Un `.md` por pantalla; el manual de cada rol se arma solo a partir de ellos. Se **embeben en el .dll** al compilar (`WebCore/WebCore.csproj`). Decision y reglas: `docs/DECISIONS.md` ("Manuales por rol y ayuda en cada pantalla").')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('## Estructura')
    [void]$sb.AppendLine('- `pantallas/<Controller>.<Action>.md` -- ayuda de UNA pantalla (clave = controller/action de la ruta; `alias:` para otras acciones que muestran la misma vista).')
    [void]$sb.AppendLine('- `conceptos/<slug>.md` -- explicaciones transversales (roles, dispositivo seguro, PIN...).')
    [void]$sb.AppendLine('- `referencia/<slug>.md` -- tablas de consulta (mapa de configuraciones).')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('Encabezado de cada archivo: `pantalla` (solo pantallas), `titulo`, `rol` (`usuario` | `admin` | `superadmin`), `modulo`, `orden`, `permiso` (informativo), `revisada` (yyyy-MM-dd), `alias` (opcional). Enlaces internos: `[texto](ayuda:pantalla/Usuarios.Index)`, `(ayuda:concepto/slug)`, `(ayuda:referencia/slug)`.')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('**Regla**: toda vista o comportamiento nuevo/modificado actualiza su `.md` y, si hay una configuracion nueva, `referencia/configuraciones.md`, en el mismo commit. Lo no verificado contra el codigo se marca `PENDIENTE`; si la doc contradice al codigo, manda el codigo. Control: `powershell -File scripts\check-ayuda.ps1`.')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine("## Cobertura ($cubiertas de $total pantallas)")
    [void]$sb.AppendLine('')
    foreach ($rol in @(@('usuario', 'Manual del usuario'), @('admin', 'Manual del administrador'), @('superadmin', 'Manual del super administrador'))) {
        $lista = $docs | Where-Object { $_.Rol -eq $rol[0] } | Sort-Object Tipo, Modulo, Titulo
        [void]$sb.AppendLine("## $($rol[1])")
        [void]$sb.AppendLine('')
        if (-not $lista) { [void]$sb.AppendLine('_Todavia sin contenido._'); [void]$sb.AppendLine(''); continue }
        [void]$sb.AppendLine('| Tipo | Titulo | Modulo | Archivo | Revisada |')
        [void]$sb.AppendLine('|---|---|---|---|---|')
        foreach ($d in $lista) { [void]$sb.AppendLine("| $($d.Tipo) | $($d.Titulo) | $($d.Modulo) | ``$($d.Tipo)/$($d.Archivo)`` | $($d.Revisada) |") }
        [void]$sb.AppendLine('')
    }
    [void]$sb.AppendLine('## Pantallas sin ayuda todavia')
    [void]$sb.AppendLine('')
    if ($sinAyuda.Count -eq 0) { [void]$sb.AppendLine('Ninguna.') }
    else { foreach ($g in ($sinAyuda | Group-Object Carpeta | Sort-Object Name)) { [void]$sb.AppendLine("- **$($g.Name)**: $((($g.Group | ForEach-Object { $_.Vista }) -join ', '))") } }
    $destino = Join-Path $manual 'README.md'
    [System.IO.File]::WriteAllText($destino, $sb.ToString(), (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "`nREADME regenerado: $destino" -ForegroundColor DarkGray
}

if ($Estricto -and ($huerfanos.Count -gt 0 -or $desactualizados.Count -gt 0)) { exit 1 }
exit 0
