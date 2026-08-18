# Despliegue y publicacion

## Objetivo

Documentar el proceso actual de publicacion de escritorio, web y componentes auxiliares.

## Secciones

- Componente
- Metodo de publicacion
- Artefactos generados
- Validaciones posteriores

## Sistema web (`Web/`) -> VM Windows de produccion

Acceso SSH de la VM: `~/hosts/carnisys-vm-windows.env` (fuera del repo). Sitio real: IIS "CarniSys" en `C:\inetpub\wwwroot\CarniSysWeb`, detras de Caddy (`C:\caddy\Caddyfile` en la VM) que termina TLS y reenvia a `localhost:8069`. **No hay pipeline git en el servidor**: es un publish precompilado copiado a mano, no un `git pull`.

### Pasos (probado 2026-07-29)

1. Publish Release precompilado a una carpeta local (no usar el `PublishUrl` de `Web/Properties/PublishProfiles/FolderProfile.pubxml`, apunta a una ruta que solo existe dentro de la VM):
   ```
   msbuild Web/Web.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:publishUrl=<carpeta_local> /p:WebPublishMethod=FileSystem /p:DeployDefaultTarget=WebPublish
   ```
2. **Antes de nada**, comparar el `Web.config` publicado contra el que corre en produccion (traerlo por SSH). Ver `riesgos-conocidos.md`: produccion necesita `requireSSL="false"` en `httpCookies`, `forms` y `Security:CookieRequireSsl` (el transform de Release trae `true`). Ajustar a mano antes de copiar.
3. **Sacar del paquete** `Config\connectionStrings.config` y `Config\appSettings.secrets.config` (son secretos locales del dev, nunca se suben).
4. Subir por SFTP (Posh-SSH) a `C:\inetpub\wwwroot\web\_deploy\<algo>.zip` y extraer ahi mismo. El SFTP de esta VM exige paths estilo POSIX: `/C:/inetpub/...`, no `C:\inetpub\...`.
5. Backup completo de lo que esta corriendo: `robocopy C:\inetpub\wwwroot\CarniSysWeb C:\inetpub\wwwroot\web\backups\CarniSysWeb_<yyyyMMdd_HHmmss> /MIR`.
6. Swap: `Stop-WebAppPool -Name CarniSys` -> `robocopy /MIR` de `bin`, `Content`, `Scripts`, `Views`, `fonts` desde lo extraido hacia `CarniSysWeb`, copiar sueltos (`Web.config`, `favicon.ico`, `libman.json`, `manifest.json`, `sw.js`, `PrecompiledApp.config`) -> `Start-WebAppPool -Name CarniSys`.
7. **Nunca tocar** `Config\` (secrets reales), `AFIP\` (tickets WSAA vivos) ni `App_Data\` (logs vivos) de la VM: no son parte del build, son estado de produccion.

### Validaciones posteriores

- `curl https://carnisys.com/` debe dar `200` (landing de marketing publica, no redirige mas a `/Login/Index` desde que se agrego esa home -- desactualizado respecto a la version original de este runbook, 2026-07-29, que esperaba `302`).
- `curl https://carnisys.com/Login/Index` debe dar `200`, sin `Stack Trace` / `Server Error` en el body.
- Confirmado 2026-08-14 (deploy del commit `c9f60625`, feature de bloqueo/desbloqueo de cuenta y dispositivos seguros): ambas URLs devuelven `200`, headers de seguridad completos, y el `Content-Security-Policy` incluye `connect-src ... http://127.0.0.1:18777` (confirma que el build con el fix de CSP para el PrintAgent quedo efectivamente publicado).

### Rollback

Restaurar el backup de `web\backups\CarniSysWeb_<timestamp>` con `robocopy /MIR` hacia `CarniSysWeb` (respetando el mismo cuidado de no tocar `Config\`/`AFIP\`/`App_Data\` si ya cambiaron) y reiniciar el App Pool `CarniSys`.

### Nota sobre el sitio "web" y `_deploy`/`backups`

`C:\inetpub\wwwroot\web` es un segundo sitio IIS que **no** recibe trafico publico (Caddy solo enruta a "CarniSys"). Ahi vive el historial de `_deploy\` (paquetes extraidos) y `backups\` (snapshots). Al 2026-07-29 habia un build de staging del 28/7 parcheado a mano el 29/7 (`Web.config_before_dbfix_*`) que nunca se promovio a produccion y se dejo sin tocar.

## Segundo destino: "Servidor SM" (`192.168.0.151`) -> distinto de la VM de produccion "Carnisys"

**No confundir con la VM de arriba.** Hay dos servidores de deploy distintos para este proyecto:

- **Servidor "Carnisys"**: la VM Windows de produccion documentada arriba, acceso SSH (`~/hosts/carnisys-vm-windows.env`), publica detras de Caddy en `carnisys.com`.
- **Servidor "SM"** (`PCSERVIDORSM`, IP `192.168.0.151`): otro servidor en la LAN. Acceso por **SMB** (`\\servidorsm\carnisysweb` o `\\192.168.0.151\carnisysweb`) y por **SSH** (puerto 22, cuenta admin), credenciales en `~/hosts/servidorsm.env`. Aloja IIS con un sitio "web" en el puerto **8069** (HTTP) y **443** (HTTPS) que contiene, como aplicaciones separadas: `CarniSysWeb` (produccion de este servidor), `CarniSysWeb - copia`, y otro sitio independiente `SuperCerdoWeb` (otro proyecto, fuera de alcance). URL real: `https://192.168.0.151/CarniSysWeb/` (el `:8069` sobre HTTP redirige ahi con 301). PENDIENTE: confirmar si hay un hostname/dominio real para este server (hoy solo se probo por IP).

### Setup unico del servidor (hecho 2026-07-31, no hace falta repetir en cada deploy)

Al momento del primer deploy (2026-07-30) el servidor no tenia SSH, la cuenta de deploy no era admin, y el sitio "web" no tenia ningun binding HTTPS ni certificado (por eso el redirect a HTTPS de la app, `Security:EnforceHttps=true`, daba timeout). Se resolvio asi:

1. **SSH**: `Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0` -> `Start-Service sshd` -> `Set-Service -Name sshd -StartupType Automatic`. La regla de firewall `OpenSSH-Server-In-TCP` se crea sola.
2. **Privilegios admin para la cuenta de deploy** (`carnisys-deploy`, la misma que SMB): `Add-LocalGroupMember -SID "S-1-5-32-544" -Member "carnisys-deploy"` mas la clave de registro `HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\LocalAccountTokenFilterPolicy = 1` (DWORD). Sin esa clave, las sesiones remotas (SSH/SMB) de una cuenta admin que no es la `Administrador` incorporada reciben igual un token recortado por UAC y los comandos de administracion fallan con "el proceso debe tener un estado elevado" (KB951016).
3. **Certificado y binding HTTPS**: no habia ningun certificado en `Cert:\LocalMachine\My` ni binding HTTPS en ningun sitio del servidor. Se genero uno autofirmado (es una IP privada, no aplica un cert publico tipo Let's Encrypt) y se bindeo al sitio "web":
   ```powershell
   $cert = New-SelfSignedCertificate -DnsName "192.168.0.151" -CertStoreLocation "Cert:\LocalMachine\My" -FriendlyName "CarniSysWeb-ServidorSM-selfsigned" -NotAfter (Get-Date).AddYears(5)
   New-WebBinding -Name "web" -Protocol https -Port 443 -IPAddress 192.168.0.151
   New-Item -Path "IIS:\SslBindings\192.168.0.151!443" -Value $cert
   ```
   **Nota**: al ser autofirmado, cualquier navegador muestra advertencia de "certificado no confiable" la primera vez, hasta que se instale como confiable en cada PC que lo use. Es el mismo tradeoff que la VM evita usando Caddy con TLS real hacia afuera; este servidor no tiene ese reverse proxy.

### Pasos de deploy (probado 2026-07-30 y 2026-07-31)

1. Publish Release precompilado a una carpeta local, igual que el paso 1 de la VM (mismo `msbuild ... /p:PublishProfile=FolderProfile /p:publishUrl=<carpeta_local> ...`). A diferencia de la VM, **no hace falta tocar `requireSSL`**: el transform de `Web.Release.config` (`true`) ya es el valor correcto para este servidor.
2. Backup del sitio actual: mapear el share con `net use Z: \\192.168.0.151\carnisysweb /user:ServidorSM\carnisys-deploy <password>` y `robocopy Z:\ <carpeta_local_backup> /MIR`.
3. Copiar el build nuevo con `robocopy <publish>\<carpeta> Z:\<carpeta> /MIR` para `bin`, `Content`, `Scripts`, `Views`, `fonts`, y los sueltos (`Web.config`, `favicon.ico`, `libman.json`, `manifest.json`, `sw.js`, `PrecompiledApp.config`) con `Copy-Item -Force`.
4. **Nunca tocar** `Config\`, `AFIP\`, `App_Data\` del share (mismo motivo que en la VM: estado vivo, no build).
5. `net use Z: /delete` al terminar.
6. No hay pipeline ni acceso remoto para reciclar el App Pool a mano; IIS/ASP.NET recicla el AppDomain solo al detectar cambios en `bin\` o `Web.config`, asi que no hace falta paso manual.

### Validaciones posteriores

- `curl http://192.168.0.151:8069/CarniSysWeb/` debe dar `301` a `https://192.168.0.151/CarniSysWeb/` con los headers de seguridad (`Content-Security-Policy`, `X-Frame-Options`) del `Web.config` publicado.
- `curl -k https://192.168.0.151/CarniSysWeb/Login/Index` debe dar `200`, titulo `CarniSysWeb - Login`, sin `Stack Trace`/`Server Error` en el body, y `Set-Cookie` con `secure`/`HttpOnly` (confirma que `requireSSL="true"` esta funcionando con el binding). El `-k` es porque el certificado es autofirmado. **Verificado 2026-07-31.**

### Rollback

Restaurar el backup local (paso 2 de arriba) con `robocopy <backup> Z:\ /MIR`, respetando no tocar `Config\`/`AFIP\`/`App_Data\`. No hay snapshot historico en el propio servidor (a diferencia de la VM, que tiene `web\backups\`) — el backup queda en la maquina donde se corrio el deploy, PENDIENTE definir si conviene subirlo tambien a un `backups\` dentro del server.

## Tercer destino: "San Lorenzo" (`200.107.108.44`) -> IP publica, sin SMB, sin carpeta `Config\`

**No confundir con los dos de arriba.** Servidor Windows nuevo (alta 2026-08-01), acceso solo por **SSH** (puerto `2222`, no `22` — bloqueado por el ISP en la WAN, ver `~/hosts/sanlorenzo.env`) y RDP (`3389`). **No tiene SMB compartido** (a diferencia de Servidor SM), asi que la transferencia de archivos es por **SFTP** (Posh-SSH), no por `net use`. Aloja IIS con un sitio "web" (puertos `8069` HTTP / `443` HTTPS) con `CarniSysWeb` y `SuperCerdoWeb` como aplicaciones hermanas, mas un sitio standalone separado `SuperCerdo` (fuera de alcance). URL real: `https://200.107.108.44/CarniSysWeb/` (funciona desde afuera via DMZ del router; el certificado es autofirmado, con `-k`/`-k` en `curl`).

**Diferencia critica con los otros dos destinos**: este servidor **no tiene carpeta `Config\`** — el `connectionStrings` y todo `appSettings` (incluidas credenciales SMTP reales) viven **directo dentro de `Web.config`**. Esto significa que un publish normal (que trae su propio `Web.config` transformado, con connection strings distintas) **pisaria los secretos reales del servidor** si se copia sin cuidado. Por eso el paso de swap de este destino **excluye explicitamente `Web.config`** — se deja el que ya esta en el servidor, intacto.

### Primer deploy de codigo (hecho 2026-08-03, commit `257ca0ab` de `codex_ia`)

El IIS/cert/binding de este servidor ya estaban configurados de antes (alta del servidor, 2026-08-01/02) — este fue el primer deploy de la **aplicacion** en si.

1. Publish Release local, igual que los otros dos destinos (mismo `msbuild ... /p:PublishProfile=FolderProfile /p:publishUrl=<carpeta_local> ...`). Si ya se publico el mismo commit para otro destino en la misma sesion, se puede reusar la misma carpeta de publish.
2. Backup remoto (en el propio servidor, via SSH — no hay forma de traerlo a la maquina local sin SMB): 
   ```
   robocopy C:\inetpub\wwwroot\web\CarniSysWeb C:\inetpub\wwwroot\web\backups\CarniSysWeb_<timestamp> /MIR
   ```
3. Subida por **SFTP** (Posh-SSH, `New-SFTPSession` + `Set-SFTPItem`) de `bin`, `Content`, `Scripts`, `Views`, `fonts` (carpetas completas) y los sueltos `favicon.ico`, `libman.json`, `manifest.json`, `sw.js`, `PrecompiledApp.config` — **nunca `Web.config`** — a una carpeta de staging: `C:\inetpub\wwwroot\web\_deploy\<algo>\`. El path de destino en `Set-SFTPItem` va en formato POSIX (`/C:/inetpub/...`), igual que el SFTP de la VM.
   - **Gotcha visto el 2026-08-07**: `Set-SFTPItem` recursivo sobre una carpeta completa (`bin`, ~2300 archivos entre todas) puede cortar la conexion a mitad de camino ("conexion cerrada por el host remoto"), causa no confirmada. Mitigacion que funciono: subir archivo por archivo (`Get-ChildItem -Recurse` + loop), creando los directorios remotos nivel por nivel con `New-SFTPItem -ItemType Directory` (no crea intermedios solo, hay que iterar los segmentos del path) y reabriendo la sesion SFTP si una transferencia individual falla. Mas lento pero resistente a cortes parciales.
4. Swap por SSH (`Invoke-SSHCommand`, shell remoto es **`cmd.exe`**, no PowerShell — usar `dir`/`copy`/`robocopy`, no cmdlets): `robocopy <staging>\<carpeta> C:\inetpub\wwwroot\web\CarniSysWeb\<carpeta> /MIR` para cada una de las 5 carpetas, y `copy /Y <staging>\<archivo> C:\inetpub\wwwroot\web\CarniSysWeb\<archivo>` para cada suelto.
5. **Nunca tocar** `Web.config` (secretos reales embebidos, ver arriba), `AFIP\` ni `App_Data\`.
6. No hace falta reciclar el App Pool a mano — IIS/ASP.NET lo hace solo al detectar cambios en `bin\`.

### Validaciones posteriores

- `curl http://200.107.108.44:8069/CarniSysWeb/` da `200` sirviendo la home publica de marketing directo (no `302` a Login como documentaba esta seccion hasta el 2026-08-03: la ruta raiz cambio de comportamiento con el feature "home publica" -- commit `72abd98e` -- que la desacoplo del login; no forzar upgrade a HTTPS sigue siendo el comportamiento esperado de este servidor, eso no cambio). **Corregido 2026-08-14** tras notar la discrepancia doc-vs-codigo tras un deploy real (CLAUDE.md SS8.3: manda el codigo).
- `curl -k https://200.107.108.44/CarniSysWeb/Login/Index` debe dar `200`, titulo `Ingresar a CARNISYS` (el texto del titulo tambien cambio desde `CarniSysWeb - Login`, mismo motivo), sin `Stack Trace`/`Server Error`. El `Set-Cookie` **no** trae `secure` (a diferencia de SM/VM) — tampoco es un bug: este `Web.config` no tiene `requireSSL`/`CookieRequireSsl` configurado, se deja como esta (no se toca `Web.config`). **Verificado 2026-08-03, re-verificado 2026-08-14.**
- `Invoke-WebRequest` de PowerShell 5.1 **falla** contra el binding HTTPS de este servidor (error de renegociacion TLS) aunque `curl.exe` funciona bien — usar siempre `curl.exe`/`curl -k` para health checks aca, nunca `Invoke-WebRequest`.

### Rollback

Restaurar el backup remoto (paso 2 de arriba) con `robocopy C:\inetpub\wwwroot\web\backups\CarniSysWeb_<timestamp> C:\inetpub\wwwroot\web\CarniSysWeb /MIR` (via SSH), sin tocar `Web.config`/`AFIP\`/`App_Data\`. El backup queda en el propio servidor (a diferencia de Servidor SM, que no tiene backup remoto — aca si, porque no hay SMB para bajarlo a la maquina local).
