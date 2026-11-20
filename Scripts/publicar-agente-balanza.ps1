<#
.SYNOPSIS
  Compila el agente de balanza, lo firma y lo deja listo para publicar en WebCore (Utilidades + auto-actualizacion).
.DESCRIPTION
  1. Compila Carnisys.Balanza.Agent en Release (MSBuild de Visual Studio).
  2. Copia el .exe a WebCore\wwwroot\Content\downloads\.
  3. Lo firma (RSA-2048, SHA-256) con la clave PRIVADA de ~/hosts/carnisys-balanza-firma.env (nunca se imprime ni va al repo).
  4. Escribe Carnisys.Balanza.Agent.json (version, sha256, firma, tamano, fecha) al lado del .exe.
  5. Comprueba que la firma se verifica con la clave PUBLICA embebida en Carnisys.Balanza.Agent\ActualizacionClave.cs
     (si no cierra, el agente instalado rechazaria la actualizacion: aborta).
  Despues hay que commitear ambos archivos (el .gitignore ignora *.exe: usar `git add -f` para el .exe) y desplegar WebCore.
  Ver docs/06-datos-e-integraciones/balanzas-e-impresoras.md.
.PARAMETER SinCompilar
  Usa el .exe ya compilado en Carnisys.Balanza.Agent\bin\Release (no recompila).
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts\publicar-agente-balanza.ps1
#>
param([switch]$SinCompilar)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$proyecto = Join-Path $repo 'Carnisys.Balanza.Agent\Carnisys.Balanza.Agent.csproj'
$salida = Join-Path $repo 'Carnisys.Balanza.Agent\bin\Release'
$exeCompilado = Join-Path $salida 'Carnisys.Balanza.Agent.exe'
$destinoDir = Join-Path $repo 'WebCore\wwwroot\Content\downloads'
$exeDestino = Join-Path $destinoDir 'Carnisys.Balanza.Agent.exe'
$manifiesto = Join-Path $destinoDir 'Carnisys.Balanza.Agent.json'
$archivoClave = Join-Path $env:USERPROFILE 'hosts\carnisys-balanza-firma.env'

# --- 1) Compilar ---
if (-not $SinCompilar) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = $null
    if (Test-Path $vswhere) { $msbuild = & $vswhere -latest -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1 }
    if (-not $msbuild) { throw 'No se encontro MSBuild (Visual Studio). El agente es .NET Framework y no compila con dotnet build.' }
    & $msbuild $proyecto -p:Configuration=Release -v:m -nologo
    if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }
}
if (-not (Test-Path $exeCompilado)) { throw "No existe $exeCompilado" }

# --- 2) Copiar ---
New-Item -ItemType Directory -Force $destinoDir | Out-Null
Copy-Item $exeCompilado $exeDestino -Force
$bytes = [IO.File]::ReadAllBytes($exeDestino)

# --- 3) Firmar con la clave privada (fuera del repo) ---
if (-not (Test-Path $archivoClave)) { throw "Falta la clave de firma: $archivoClave (ver docs/06-datos-e-integraciones/balanzas-e-impresoras.md)" }
$linea = Get-Content $archivoClave | Where-Object { $_ -like 'FIRMA_PRIVATE_KEY_XML=*' } | Select-Object -First 1
if (-not $linea) { throw 'El archivo de clave no tiene FIRMA_PRIVATE_KEY_XML.' }
$xmlPrivada = $linea.Substring('FIRMA_PRIVATE_KEY_XML='.Length)
$rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider
$rsa.FromXmlString($xmlPrivada)
$sha = [System.Security.Cryptography.SHA256]::Create()
$firma = $rsa.SignData($bytes, $sha)
$hash = ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace '-', '').ToLowerInvariant()

# --- 5) Comprobar contra la clave publica embebida en el agente ---
$fuenteClave = Get-Content (Join-Path $repo 'Carnisys.Balanza.Agent\ActualizacionClave.cs') -Raw
$m = [regex]::Match($fuenteClave, 'PublicaXml\s*=\s*"((?:[^"\\]|\\.)*)"')
if (-not $m.Success) { throw 'No se pudo leer la clave publica de ActualizacionClave.cs' }
$xmlPublica = $m.Groups[1].Value.Replace('\"', '"')
$rsaPublica = New-Object System.Security.Cryptography.RSACryptoServiceProvider
$rsaPublica.FromXmlString($xmlPublica)
if (-not $rsaPublica.VerifyData($bytes, $sha, $firma)) {
    throw 'La firma NO se verifica con la clave publica embebida en el agente: la clave privada de ~/hosts no corresponde. No se publica.'
}

# --- 4) Manifiesto ---
$version = [Diagnostics.FileVersionInfo]::GetVersionInfo($exeDestino).FileVersion
$partes = $version.Split('.')
$version3 = ($partes[0..2] -join '.')
$json = [ordered]@{
    version = $version3
    sha256  = $hash
    firma   = [Convert]::ToBase64String($firma)
    tamano  = $bytes.Length
    fecha   = (Get-Date).ToString('yyyy-MM-ddTHH:mm:ss')
} | ConvertTo-Json
[IO.File]::WriteAllText($manifiesto, $json, (New-Object System.Text.UTF8Encoding($false)))

"Agente $version3 firmado y listo."
"  exe        : $exeDestino ($($bytes.Length) bytes)"
"  sha256     : $hash"
"  manifiesto : $manifiesto"
"Siguiente: git add -f WebCore/wwwroot/Content/downloads/Carnisys.Balanza.Agent.exe WebCore/wwwroot/Content/downloads/Carnisys.Balanza.Agent.json, commit y deploy."
