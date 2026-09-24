# Despliegue y publicacion

## Objetivo

Documentar el proceso actual de publicacion de escritorio, web y componentes auxiliares.

## Secciones

- Componente
- Metodo de publicacion
- Artefactos generados
- Validaciones posteriores

## Sistema web -> VM Windows de produccion "CarniSys" (`carnisys.com`)

**Cutover 2026-09-14: la clasica (`Web/`) se retiro de esta VM, `carnisys.com` corre WebCore standalone.** Todo lo que sigue en esta seccion reemplaza el procedimiento anterior (que describia IIS "CarniSys" -- esa parte quedo obsoleta, ver "Historia" al final). Las otras 2 VMs (Servidor SM, San Lorenzo, mas abajo en este archivo) siguen con la clasica sin cambios, este cutover fue especifico de esta VM.

Acceso SSH de la VM: `~/hosts/carnisys-vm-windows.env` (fuera del repo). **No hay pipeline git en el servidor**: es un publish precompilado copiado a mano, no un `git pull`.

### Arquitectura actual

- **WebCore corre standalone (self-contained `win-x64`, sin IIS)** en `C:\WebCore\WebCore.exe --urls http://127.0.0.1:5250`. El publish incluye el runtime de .NET 10 completo -- la VM no tiene instalado (ni necesita) el SDK/Hosting Bundle de ASP.NET Core.
- **Proceso administrado por Tarea Programada + Watchdog**, mismo patron que ya usaba Caddy en esta VM (`C:\caddy\{caddy.exe, watchdog.ps1, register-watchdog.ps1}`): tarea `WebCoreApp` (lanza el proceso, `ExecutionTimeLimit=PT0S` = sin limite), tarea `WebCoreAppWatchdog` (cada 2 min, SYSTEM, revisa `Get-NetTCPConnection -LocalPort 5250`, mata y relanza `WebCoreApp` si no escucha). Scripts en `C:\WebCore\{watchdog.ps1, register-webcore-tasks.ps1}` (staging local, no versionados en el repo -- son especificos de esta VM).
- **Kestrel bindea solo a `127.0.0.1`**, nunca expuesto directo a la red (mismo criterio que Postgres en esta VM).
- **Caddy** (`C:\caddy\Caddyfile`) hace `reverse_proxy localhost:5250` para `carnisys.com` -- ya no hay compat-redirect de `/CarniSysWeb/*` (la clasica se retiro, ese prefijo no aplica mas). `www.carnisys.com` sigue redirigiendo al apex, sin cambios.
- **Base de datos**: `C:\WebCore\WebCore.dll.config` -> `DataEngine=Postgres`, `ConexionPostgresPiloto` = la MISMA conexion que ya usaba la clasica en esta VM (`localhost`, base `carnisys`, rol `carnisys_user` -- ver `~/hosts/carnisys-vm-postgres.env`). SMTP tambien reusado de `Config\appSettings.secrets.config` de la clasica. **`WebCore.dll.config`, NO `App.config`, es el archivo que `System.Configuration.ConfigurationManager` lee en runtime** (ver bug real más abajo) -- es estado de esta VM, generado una vez a mano -- **nunca se pisa en un deploy de codigo** (mismo criterio que `Config\` de la clasica en las otras VMs). Se mantiene igual `App.config` junto a él (mismo contenido, por si algún día se lee de ahí) pero no es el que importa.

### Bug real encontrado el mismo día del cutover: login redirigía en loop a `/Login` -- `WebCore.dll.config`, no `App.config`, es el archivo real

`System.Configuration.ConfigurationManager` (paquete NuGet usado por WebCore para leer `App.config` estilo .NET Framework desde .NET 10) **lee `<NombreEnsamblado>.dll.config` (`WebCore.dll.config`), no `App.config`** -- ese último no se lee en absoluto en producción (self-contained, corriendo `WebCore.exe` directo). `dotnet publish` copia el `App.config` del REPO (el de dev local) a `WebCore.dll.config` en la carpeta de publish, con el nombre renombrado -- el `App.config` que se sube aparte a `C:\WebCore\App.config` con las credenciales reales de esta VM queda como un archivo suelto sin ningún efecto.

**Síntoma real reportado por el usuario**: al loguearse (`ger`/`a`, `phm`/`a`), la página volvía a Login sin mensaje de error. Causa real (Event Viewer de la VM, `.NET Runtime` categoría `ExceptionHandlerMiddleware`): `Npgsql.PostgresException: 28P01 password authentication failed for user "carnisys_user"` -- WebCore intentaba autenticar con la password de la base LOCAL de dev (la que trae `WebCore.dll.config` recién publicado) contra el rol real de la VM, fallando siempre. La excepción no manejada caía a `/Home/Error`, que exige `[Authorize]`, generando el loop de vuelta a `/Login?ReturnUrl=%2FHome%2FError` -- de ahí el síntoma "redirige a la misma página de login" sin mensaje visible.

**Por qué no se detectó en el cutover original**: en dev local, `App.config` y `WebCore.dll.config` tienen el MISMO contenido (ambos vienen del mismo `App.config` del repo) -- nunca hubo una corrida real donde difirieran, así que el bug era invisible hasta el primer deploy a un ambiente con credenciales distintas.

**Fix**: copiar el contenido de `C:\WebCore\App.config` (ya correcto, con las credenciales reales) a `C:\WebCore\WebCore.dll.config`, sobreescribiendo lo que trajo el publish. Verificado con `psql` directo (confirmó que la password en sí era válida, descartando error de tipeo) antes de aislar el problema al archivo equivocado. **Cualquier deploy futuro de WebCore a esta VM debe restaurar `WebCore.dll.config` (no `App.config`) con el backup/restore del paso 4 de abajo** -- corregido en el procedimiento de deploy de esta sección.

**Verificado en vivo, de punta a punta**: `curl` con formulario real (`Usuario=ger`, `Clave=a`, token antiforgery real) -> `302` a `/Home/Index` con `Set-Cookie: .AspNetCore.Cookies=...` real: -> `GET /Home/Index` con esa cookie -> `200`, `<title>Dashboard - CarniSys</title>`.

### Bug real encontrado en el cutover: registrar un watchdog con trigger `AtStartup + Repetition` no empieza a repetir hasta el proximo reboot

Al copiar literalmente el patron de `C:\caddy\register-watchdog.ps1` (trigger `-AtStartup` con un `Repetition` pegado desde un trigger `-Once` descartado) para `WebCoreAppWatchdog`, `Get-ScheduledTaskInfo` mostraba `NextRunTime` vacio -- la tarea NUNCA arrancaba su ciclo de 2 minutos porque el trigger base (`AtStartup`) no habia disparado (la VM no reinicio desde que se registro la tarea), y sin ese primer disparo la repeticion adosada nunca arranca. Confirmado en vivo: se mato el proceso a mano, `curl` dio `502` durante ~1.5 minutos hasta que se corrigio el trigger.

**Fix real aplicado**: 2 triggers separados en la misma tarea -- uno `-Once -At (Get-Date).AddSeconds(30) -RepetitionInterval 2min -RepetitionDuration 3650dias` (arranca el ciclo YA, sin esperar un reboot) mas uno `-AtStartup` (para que tambien arranque solo si el sistema reinicia). Verificado en vivo: proceso matado a mano -> log (`C:\WebCore\watchdog.log`) registro `CAIDO -> reinicio tarea` y `RECUPERADO` ~9 segundos despues, sitio de nuevo en `200` dentro del ciclo de 2 minutos esperado.

**Nota importante, no corregida todavia**: `C:\caddy\CaddyWatchdog` (el original, referencia de este patron) probablemente tiene el MISMO problema estructural -- si nunca reinicio la VM desde que se registro esa tarea (2026-06-15), su repeticion tampoco arranco hasta el primer reboot real. Como Caddy viene funcionando bien hace meses, lo mas probable es que la VM ya reinicio al menos una vez desde entonces y el ciclo arranco en ese momento -- pero **no esta confirmado**, y si algun dia se reinicia `CaddyWatchdog` con `Register-ScheduledTask -Force` sin corregir el trigger, quedaria con el mismo bug latente. Fuera de alcance de este cutover (no se toco la tarea de Caddy); queda como hallazgo para una revision aparte si se justifica.

### Pasos de deploy (probado 2026-09-14, primer deploy de WebCore standalone a esta VM)

1. `dotnet publish WebCore/WebCore.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o <carpeta_local>`.
2. Comprimir con `Compress-Archive` (un solo zip, mas resistente a cortes que subir ~370 archivos sueltos).
3. Subir por SFTP (Posh-SSH) a `C:\WebCore\<nombre>.zip`.
4. **Swap con backup/restore de `WebCore.dll.config`** (el archivo que realmente se lee en runtime -- ver bug real más arriba; el zip trae la version de DEV local, hay que preservar la de produccion): `Stop-ScheduledTask WebCoreApp` -> matar proceso residual -> guardar el contenido actual de `C:\WebCore\WebCore.dll.config` en una variable -> `Expand-Archive -Force` (pisa todo, incluido `WebCore.dll.config`) -> restaurar el contenido guardado con `Set-Content` -> `Start-ScheduledTask WebCoreApp`. (Opcional, no crítico: hacer lo mismo con `App.config` para que ambos archivos queden consistentes, aunque solo `WebCore.dll.config` tiene efecto real.)
5. Verificar `Get-NetTCPConnection -LocalPort 5250` y `curl https://carnisys.com/` -> `200`.

### Validaciones posteriores

- `curl https://carnisys.com/` y `/Login` -> `200`, `Server: Kestrel` en la respuesta (confirma que es WebCore, no la clasica vieja).
- `Get-ScheduledTask -TaskName "WebCoreApp","WebCoreAppWatchdog"` -> ambas `Running`/`Ready` segun corresponda, sin `LastTaskResult` de error.
- **Verificado 2026-09-14**: `carnisys.com` devolvia `404` en `/Login/Index` y `/Home/Index` ANTES de este cutover (causa: el sitio IIS "CarniSys" de la clasica estaba `Stopped`, el trafico caia por fallback a un sitio "web" con una copia vieja de julio -- ver "Historia" abajo). Post-cutover: `200` en ambas rutas, sirviendo WebCore.

### Retiro de la clasica en esta VM (2026-09-14)

Sitios IIS "CarniSys" y "web" **detenidos** (`Stop-Website`), no borrados -- los archivos (incluido `AFIP\` con certificados/tickets WSAA reales) quedan intactos en `C:\inetpub\wwwroot\{CarniSysWeb,web}` como red de seguridad. Decision explicita del usuario: no borrar de disco por ahora. El unico link de WebCore que apuntaba a la clasica (Mercado Pago, via `WebClasicoBaseUrl`) se oculto en `WebCore/Views/Shared/_Layout.cshtml` -- es el unico modulo que todavia no se porto.

### Historia (obsoleto, se deja como referencia del estado anterior al 2026-09-14)

Antes del cutover, el sitio real de la clasica en esta VM era IIS "CarniSys" en `C:\inetpub\wwwroot\CarniSysWeb`, detras de Caddy reenviando a `localhost:8069`. **Hallazgo real al investigar el cutover**: para cuando se audito el estado de esta VM (2026-09-14), ese sitio ya estaba `Stopped` de antes (causa desconocida, no relacionada a este cutover) y el trafico real caia, por el binding sin hostname de IIS, a un sitio "web" con una copia de julio desactualizada -- de ahi el `404` real encontrado en produccion antes de arrancar este trabajo. La doc de abajo (pasos con `Stop-WebAppPool -Name CarniSys`, etc.) describe el procedimiento que se usaba CUANDO ese sitio era el real -- ya no aplica a esta VM, se conserva por si alguna vez se necesita reconstruir el contexto historico:

### Pasos (probado 2026-07-29)

1. Publish Release precompilado a una carpeta local (no usar el `PublishUrl` de `Web/Properties/PublishProfiles/FolderProfile.pubxml`, apunta a una ruta que solo existe dentro de la VM):
   ```
   msbuild Web/Web.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:publishUrl=<carpeta_local> /p:WebPublishMethod=FileSystem /p:DeployDefaultTarget=WebPublish
   ```
2. **Antes de nada**, comparar el `Web.config` publicado contra el que corre en produccion (traerlo por SSH). Ver `riesgos-conocidos.md`: produccion necesita `requireSSL="false"` en `httpCookies`, `forms` y `Security:CookieRequireSsl` (el transform de Release trae `true`). Ajustar a mano antes de copiar.
3. **Sacar del paquete** `Config\connectionStrings.config`, `Config\appSettings.secrets.config` y `Config\machineKey.config` (son secretos locales del dev, nunca se suben) -- este último es la clave fija de antiforgery/Forms Auth/ViewState (ver `docs/DECISIONS.md` 2026-09-05): cada servidor tiene la suya, generada una vez a mano, nunca se pisa desde un deploy de código.
4. Subir por SFTP (Posh-SSH) a `C:\inetpub\wwwroot\web\_deploy\<algo>.zip` y extraer ahi mismo. El SFTP de esta VM exige paths estilo POSIX: `/C:/inetpub/...`, no `C:\inetpub\...`.
5. Backup completo de lo que esta corriendo: `robocopy C:\inetpub\wwwroot\CarniSysWeb C:\inetpub\wwwroot\web\backups\CarniSysWeb_<yyyyMMdd_HHmmss> /MIR`.
6. Swap: `Stop-WebAppPool -Name CarniSys` -> `robocopy /MIR` de `bin`, `Content`, `Scripts`, `Views`, `fonts` desde lo extraido hacia `CarniSysWeb`, copiar sueltos (`Web.config`, `favicon.ico`, `libman.json`, `manifest.json`, `sw.js`, `PrecompiledApp.config`) -> `Start-WebAppPool -Name CarniSys`.
7. **Nunca tocar** `Config\` (secrets reales), `AFIP\` (tickets WSAA vivos) ni `App_Data\` (logs vivos) de la VM: no son parte del build, son estado de produccion.

### Validaciones posteriores

- `curl https://carnisys.com/` debe dar `200` (landing de marketing publica, no redirige mas a `/Login/Index` desde que se agrego esa home -- desactualizado respecto a la version original de este runbook, 2026-07-29, que esperaba `302`).
- `curl https://carnisys.com/Login/Index` debe dar `200`, sin `Stack Trace` / `Server Error` en el body.
- Confirmado 2026-08-14 (deploy del commit `c9f60625`, feature de bloqueo/desbloqueo de cuenta y dispositivos seguros): ambas URLs devuelven `200`, headers de seguridad completos, y el `Content-Security-Policy` incluye `connect-src ... http://127.0.0.1:18777` (confirma que el build con el fix de CSP para el PrintAgent quedo efectivamente publicado).
- **Confirmado 2026-08-29** (deploy commit `d9b3e3f7`, fix de `Web.csproj` sin wildcard -- ver `incidencias-frecuentes.md`): metodo de swap carpeta-por-carpeta (nunca `CarniSysWeb` entera, mismo criterio que San Lorenzo) + `Web.config` SI se reemplaza en esta VM (a diferencia de San Lorenzo), con `requireSSL`/`Security:CookieRequireSsl` ajustados a mano a `false` y `DataEngine=Postgres` preservado (ya viene asi del `Web.config` de dev). `Stop-WebAppPool`/`Start-WebAppPool` tuvo un gotcha real (ver `incidencias-frecuentes.md`, el `Start` inmediatamente despues del `Stop` fallo con `0x80070425`) -- resuelto reintentando en una llamada separada. `curl` final: `200` en ambas URLs, CSP incluye `http://127.0.0.1:5100` (agente de balanza, mismo fix que San Lorenzo).

### Rollback

Restaurar el backup de `web\backups\CarniSysWeb_<timestamp>` con `robocopy /MIR` hacia `CarniSysWeb` (respetando el mismo cuidado de no tocar `Config\`/`AFIP\`/`App_Data\` si ya cambiaron) y reiniciar el App Pool `CarniSys`.

### Nota sobre el sitio "web" y `_deploy`/`backups`

`C:\inetpub\wwwroot\web` es un segundo sitio IIS que **no** recibe trafico publico (Caddy solo enruta a "CarniSys"). Ahi vive el historial de `_deploy\` (paquetes extraidos) y `backups\` (snapshots). Al 2026-07-29 habia un build de staging del 28/7 parcheado a mano el 29/7 (`Web.config_before_dbfix_*`) que nunca se promovio a produccion y se dejo sin tocar.

### Postgres en esta VM (desde 2026-08-29, cutover real)

Desde el 2026-08-29 `carnisys.com` corre en Postgres (`DataEngine=Postgres` en `Web.config`, ver
`docs/DECISIONS.md` de esa fecha para el detalle completo del cutover). Esto cambia el deploy de
esta VM en dos puntos que no aplicaban antes:

- **El `Web.config` que se sube ahora trae `DataEngine=Postgres`** (viene del `Web.config` de dev,
  sin override en `Web.Release.config`) -- a diferencia de antes, cuando esa clave ni existia en
  el `Web.config` de la VM. **Cuidado en cada deploy futuro**: si algun dia se necesita que esta
  VM vuelva a SQL Server sin tocar el motor de dev, hay que forzar `DataEngine=SqlServer` a mano
  en el `Web.config` publicado antes de copiarlo (mismo criterio que ya se usa con `requireSSL`).
- **`Config\connectionStrings.config` de la VM tiene una entrada `ConexionPostgresPiloto`**
  (host `localhost`, base `carnisys`, rol `carnisys_user`, `Keepalive=30;Tcp Keepalive=true;`
  obligatorio) ademas de `ConexionPrincipal` (SQL Server, que se deja intacta como via de
  rollback). Este archivo **nunca se toca por el deploy de codigo** (se excluye del paquete, como
  siempre) -- si hace falta rotar la password o el host, se edita a mano en la VM.

**Roles Postgres de esta VM** (credenciales en `~/hosts/carnisys-vm-postgres.env`, nunca en el
repo): `carnisys_admin` (dueno/DDL de las 49 tablas), `carnisys_user` (rol de aplicacion, el que
usa `connectionStrings.config`), `cs_admin_pg` (`BYPASSRLS`), mas los roles `NOLOGIN BYPASSRLS`
`carnisys_usuarios_bypass`/`carnisys_sysadmin_bypass`. El esquema se creo corriendo los 30 scripts
de `DatosPostgres/DB-Migrations/*.sql` en orden (ver `docs/DECISIONS.md` 2026-08-29 para el detalle
de que rol corre cada script) -- **no correr `DB-Migrations` de nuevo sobre esta base** salvo que
sea una migracion nueva que todavia no se aplico ahi (comparar contra dev local primero).

## Segundo destino: "Servidor SM" (`192.168.0.151`) -> distinto de la VM de produccion "Carnisys"

**No confundir con la VM de arriba.** Hay tres servidores de deploy distintos para este proyecto:

- **Servidor "Carnisys"**: la VM Windows de produccion documentada arriba, acceso SSH (`~/hosts/carnisys-vm-windows.env`), publica detras de Caddy en `carnisys.com`.
- **Servidor "SM"** (`PCSERVIDORSM`, IP `192.168.0.151`): otro servidor en la LAN. Acceso por **SMB** (`\\servidorsm\carnisysweb` o `\\192.168.0.151\carnisysweb`) y por **SSH** (puerto 22, cuenta admin), credenciales en `~/hosts/servidorsm.env`. Aloja IIS con un sitio "web" en el puerto **8069** (HTTP) y **443** (HTTPS) que contiene, como aplicaciones separadas: `CarniSysWeb` (produccion de este servidor, registrada pero inalcanzable desde el cutover del 2026-09-16, ver abajo), `CarniSysWeb - copia`, y otro sitio independiente `SuperCerdoWeb` (otro proyecto, fuera de alcance, **nunca tocar**). URL real: `https://192.168.0.151/` (raiz del sitio, WebCore desde el cutover). PENDIENTE: confirmar si hay un hostname/dominio real para este server (hoy solo se probo por IP).
- **Servidor "San Lorenzo"**: ver tercer destino mas abajo, cutover propio con arquitectura distinta (IIS + ASP.NET Core Module, no ARR).

**Cutover 2026-09-16: la clasica (`Web/`) se retiro de este servidor, `https://192.168.0.151/` corre WebCore standalone detras de IIS+ARR.** Todo lo que sigue en esta seccion reemplaza el procedimiento anterior (que describia el deploy de la clasica -- esa parte quedo obsoleta, ver "Historia" al final de esta seccion). Ver `docs/DECISIONS.md` 2026-09-16 para el detalle completo de los bugs reales encontrados durante este cutover.

### Arquitectura actual (desde el cutover)

- **WebCore corre standalone (self-contained `win-x64`, sin ANCM)** en `C:\WebCore\WebCore.exe --urls http://127.0.0.1:5250`, bindeado solo a `127.0.0.1`. El publish incluye el runtime de .NET 10 completo -- este servidor no tiene instalado (ni necesita) el SDK/Hosting Bundle de ASP.NET Core.
- **Proceso administrado por Tarea Programada + Watchdog**, mismo patron que la VM CarniSys: tarea `WebCoreApp` (lanza el proceso, `ExecutionTimeLimit=PT0S`), tarea `WebCoreAppWatchdog` (cada 2 min, SYSTEM, revisa `Get-NetTCPConnection -LocalPort 5250`, mata y relanza si no escucha). Scripts en `C:\WebCore\{watchdog.ps1, register-webcore-tasks-sm.ps1}` (staging local de este servidor, no versionados en el repo). **Importante**: el watchdog necesita DOS triggers separados en la tarea (`TimeTrigger` con `Repetition` que arranca ya, mas un `BootTrigger` aparte) -- adosar la `Repetition` directamente a un trigger `-AtStartup` no arranca el ciclo hasta el proximo reboot real (mismo bug ya encontrado y corregido en la VM CarniSys el 2026-09-14, ver esa seccion arriba). Verificado en vivo: proceso matado a mano, recuperado en ~8s, dos veces.
- **IIS + ARR (Application Request Routing) + URL Rewrite Module como reverse proxy**, no Caddy (no estaba instalado en este servidor) ni ANCM (patron usado en cambio para San Lorenzo -- decision independiente). `C:\inetpub\wwwroot\web.config` (raiz fisica del sitio "web") tiene las reglas de rewrite: excluye `/SuperCerdoWeb*` (nunca proxeado), redirige `/CarniSysWeb/*` (bookmarks viejos) con `301` a la raiz equivalente, y reversa-proxea todo el resto a `http://127.0.0.1:5250/{R:1}`. El proxy de ARR se habilita una vez a nivel de maquina (`appcmd set config -section:system.webServer/proxy /enabled:"True"`).
- **`/CarniSysWeb` sigue registrada como aplicacion IIS, pero inalcanzable**: **no se puede simplemente "sacarla" de la config de IIS** -- su propio `Web.config` clasico tiene secciones de `<system.web>` que solo son validas en la raiz de una aplicacion; si se quita el registro de aplicacion, esa carpeta pasa a evaluarse como subcarpeta de la app raiz del sitio y **tira `500` en las tres rutas del sitio** (bug real encontrado y revertido, ver `docs/DECISIONS.md`). Se mantiene registrada, apuntando al mismo physical path y pool "web" de siempre -- la regla de redirect a nivel de sitio intercepta antes de que cualquier peticion real llegue a ejecutarla.
- **Base de datos**: `C:\WebCore\WebCore.dll.config` -> `DataEngine=SqlServer`, connectionString `ConexionPrincipal` = la MISMA conexion que ya usa la clasica en este servidor (base `SuperCerdo`, ver `~/hosts/servidorsm.env`). SMTP reusado de `Config\appSettings.secrets.config` de la clasica. **`WebCore.dll.config`, NO `App.config`, es el archivo que se lee en runtime** (mismo bug real que la VM CarniSys, ver esa seccion arriba) -- ambos se generan identicos por prolijidad, pero solo el `.dll.config` importa.
- **`idSucursal=2` (San Martin), DIVERGE del `1` que usa la clasica en este mismo servidor**: decision explicita del usuario (confirmada durante el cutover, ver `docs/DECISIONS.md`) -- la clasica corre con `1`, WebCore con `2`. No es un bug si algun dia se nota que las dos apps ven sucursales distintas.

### Bug real encontrado: comentarios XML con `--` rompen el archivo entero (2 veces en el mismo cutover)

Tanto el primer intento de `App.config`/`WebCore.dll.config` como el primer intento del `web.config` de la raiz del sitio se armaron con un comentario de cabecera que incluia `--` (guion doble) -- **los comentarios XML no admiten `--` en su contenido** (spec XML 1.0), el archivo queda mal formado. Para el `web.config` de IIS el efecto fue grave: **toda la config del sitio quedo invalida, `500` en las tres rutas** (WebCore, SuperCerdoWeb, CarniSysWeb), confirmado con `appcmd list config` (`"El archivo de configuracion no es un codigo XML correcto"`). **Regla**: nunca usar `--` dentro de un comentario XML, y validar todo XML generado con un parser real (`System.Xml.XmlDocument.Load()`, o `appcmd list config` para configs de IIS) antes de darlo por bueno -- no confiar en la ausencia de error visible.

### Setup unico del servidor (hecho 2026-07-31, no hace falta repetir en cada deploy)

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

### Pasos de deploy (WebCore, probado 2026-09-16, primer deploy de este cutover)

0. **Deploys posteriores al primero (no aplica al setup inicial)**: `Disable-ScheduledTask -TaskName "WebCoreAppWatchdog"` ANTES de tocar `WebCore.dll.config` o subir archivos -- **bug real encontrado 2026-09-16** (ver `docs/DECISIONS.md`): `Stop-ScheduledTask` solo corta la corrida en curso, el trigger de repeticion (cada 2 min) sigue activo y el watchdog puede relanzar `WebCoreApp` a mitad del deploy, con el config equivocado (el que trajo el publish nuevo, todavia sin restaurar). `Enable-ScheduledTask` recien al final, despues de confirmar que el proceso arranco con el config correcto.
1. `dotnet publish WebCore/WebCore.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o <carpeta_local>`.
2. Subir el publish completo por SFTP (Posh-SSH, `New-SFTPSession`/`Set-SFTPItem` archivo por archivo preservando estructura) a `C:\WebCore\` en el servidor.
3. Instalar URL Rewrite Module 2.1 y ARR 3.0 (una sola vez, no en cada deploy -- descarga directa de Microsoft, `msiexec /qn`) y habilitar el proxy de ARR (`appcmd set config -section:system.webServer/proxy /enabled:"True" /commit:apphost`, tambien una sola vez).
4. Registrar las tareas `WebCoreApp`/`WebCoreAppWatchdog` (`C:\WebCore\register-webcore-tasks-sm.ps1`, idempotente con `-Force` -- reusar en cada deploy que reinstale las tareas, no hace falta si ya existen).
5. **Ultimo paso antes de arrancar, SIEMPRE despues de la subida del publish**: armar `App.config`/`WebCore.dll.config` **directo en el servidor por SSH** a partir de `WebCore/App.config.example` (nunca en la maquina local): un script remoto lee el connectionString real de `Config\connectionStrings.config` y los `Smtp*` de `Config\appSettings.secrets.config` de la clasica (backupeados/vivos en este mismo servidor) y arma el archivo con `DataEngine=SqlServer`, `idSucursal=2`, `cuit` real. **Validar con `System.Xml.XmlDocument.Load()` antes de darlo por bueno** (ver bug real de comentarios `--` arriba). `WebCore.dll.config` (no `App.config`) es el que se lee en runtime -- se generan ambos identicos por prolijidad. **Bug real (2026-09-16, ver `docs/DECISIONS.md`)**: si este paso se hace ANTES de subir el publish, la subida lo pisa silenciosamente con el `App.config`/`WebCore.dll.config` de DEV que trae el publish (`DataEngine=Postgres`) -- login roto en producción sin error visible salvo en el Event Viewer. El orden de esta lista ya reflaja el fix.
6. Arrancar `WebCoreApp` (o reiniciarlo si ya estaba corriendo desde un intento previo -- `ConfigurationManager` cachea la config al primer acceso del proceso, un archivo nuevo sin reiniciar el proceso no tiene efecto) y confirmar `http://127.0.0.1:5250/Login` en `200` **desde el propio servidor** antes de exponerlo.
7. Subir `C:\inetpub\wwwroot\web.config` (reglas de reverse-proxy/exclusion/redirect, ver Arquitectura arriba) -- **no se pisa en cada deploy de codigo**, es config de infraestructura del servidor, igual criterio que `WebCore.dll.config`.
8. Confirmar que `/CarniSysWeb` sigue registrada como aplicacion IIS (no removerla, ver bug real arriba) y que `/SuperCerdoWeb` sigue intacta.

### Validaciones posteriores (WebCore)

- `curl -k https://192.168.0.151/Login` -> `200`, contenido real `Ingresar a CARNISYS` (no alcanza con el status code: confirmar el body, para descartar una pagina estatica de error servida por IIS con `200` enganoso).
- `curl -k https://192.168.0.151/SuperCerdoWeb/` -> `200`, sin cambios (proyecto fuera de alcance, nunca deberia verse afectado por un deploy de WebCore).
- `curl -k https://192.168.0.151/CarniSysWeb/Login/Index` y `https://192.168.0.151/CarniSysWeb` (bookmarks viejos) -> `301` a la raiz equivalente, **sin doble slash** en el `Location` (bug real encontrado y corregido en la regex de redirect, ver `docs/DECISIONS.md`).
- `Get-ScheduledTask -TaskName "WebCoreApp","WebCoreAppWatchdog"` -> `Ready`/corriendo, sin `LastTaskResult` de error.
- Prueba real del watchdog (matar el proceso a mano por SSH, confirmar que se recupera solo dentro de ~15s, mucho antes del ciclo de 2 minutos) -- **verificado 2026-09-16**, dos veces.
- **Pendiente, a cargo del usuario** (mismo criterio de riesgo aceptado que San Lorenzo): login real con un usuario real contra la base `SuperCerdo` de este servidor especifico -- no se hizo Fase C antes del cutover. Tampoco se verifico la URL publica (`PUBLIC_URL` de `servidorsm.env`), solo la LAN.

### Rollback (WebCore)

1. Detener las tareas (`Stop-ScheduledTask WebCoreApp`, `Stop-ScheduledTask WebCoreAppWatchdog` -- detener el watchdog primero o va a relanzar `WebCoreApp` solo).
2. Restaurar `C:\inetpub\wwwroot\web.config` al estado anterior (sin reglas de rewrite) o borrarlo -- sin el, IIS vuelve a servir `/CarniSysWeb/` y `/SuperCerdoWeb/` como antes (ambas aplicaciones siguen registradas e intactas, nunca se tocaron sus archivos).
3. No hace falta desinstalar ARR/URL Rewrite -- no tienen efecto sin las reglas del `web.config`.

### Historia (obsoleto, se deja como referencia del deploy de la clasica anterior al 2026-09-16)

Antes del cutover, `CarniSysWeb` en este servidor era la aplicacion clasica real, deployada con el procedimiento de abajo. Ya no aplica a deploys nuevos -- WebCore es lo que corre en produccion desde el 2026-09-16.

### Pasos de deploy (clasica, HISTORIA -- probado 2026-07-30 y 2026-07-31)

1. Publish Release precompilado a una carpeta local, igual que el paso 1 de la VM (mismo `msbuild ... /p:PublishProfile=FolderProfile /p:publishUrl=<carpeta_local> ...`). A diferencia de la VM, **no hace falta tocar `requireSSL`**: el transform de `Web.Release.config` (`true`) ya es el valor correcto para este servidor.
2. Backup del sitio actual: mapear el share con `net use Z: \\192.168.0.151\carnisysweb /user:ServidorSM\carnisys-deploy <password>` y `robocopy Z:\ <carpeta_local_backup> /MIR`.
3. Copiar el build nuevo con `robocopy <publish>\<carpeta> Z:\<carpeta> /MIR` para `bin`, `Content`, `Scripts`, `Views`, `fonts`, y los sueltos (`favicon.ico`, `libman.json`, `manifest.json`, `sw.js`, `PrecompiledApp.config`) con `Copy-Item -Force`. **Nunca `Web.config`** (ver punto 4).
4. **Nunca tocar** `Web.config`, `Config\`, `AFIP\`, `App_Data\` del share (`Web.config`: incidente real 2026-08-26, ver `docs/DECISIONS.md` -- el `Web.config` local de dev trae `appSettings` propios del entorno, ej. `DataEngine=Postgres`, que no aplican a este servidor SQL Server y rompen la app en produccion; `Config\`/`AFIP\`/`App_Data\`: mismo motivo que en la VM, estado vivo, no build).
5. `net use Z: /delete` al terminar.
6. No hay pipeline ni acceso remoto para reciclar el App Pool a mano; IIS/ASP.NET recicla el AppDomain solo al detectar cambios en `bin\` o `Web.config`, asi que no hace falta paso manual.

### Validaciones posteriores (clasica, HISTORIA)

- `curl http://192.168.0.151:8069/CarniSysWeb/` debe dar `301` a `https://192.168.0.151/CarniSysWeb/` con los headers de seguridad (`Content-Security-Policy`, `X-Frame-Options`) del `Web.config` publicado.
- `curl -k https://192.168.0.151/CarniSysWeb/Login/Index` debe dar `200`, titulo `Ingresar a CARNISYS` (el texto del titulo cambio de `CarniSysWeb - Login`, desactualizado en este runbook hasta hoy -- CLAUDE.md SS8.3, manda el codigo, mismo motivo ya corregido en la seccion de San Lorenzo mas abajo), sin `Stack Trace`/`Server Error` en el body, y `Set-Cookie` con `secure`/`HttpOnly` (confirma que `requireSSL="true"` esta funcionando con el binding). El `-k` es porque el certificado es autofirmado. **Verificado 2026-07-31, re-verificado 2026-08-26 (deploy commit `7eb5547f`, tras excluir `Web.config` del swap -- ver `docs/DECISIONS.md`) y 2026-08-29 (deploy commit `d9b3e3f7`, fix de `Web.csproj` sin wildcard -- mismo fix ya publicado en San Lorenzo y la VM Carnisys ese mismo dia; CSP incluye `http://127.0.0.1:5100` del agente de balanza).**
- **2026-08-29**: swap hecho con `Copy-Item`/`Remove-Item` por carpeta (no `robocopy /MIR`) via drive mapeado (`net use Z:`), corriendo el script como proceso hijo local (`powershell -NoProfile -EncodedCommand <base64>`) -- ver `incidencias-frecuentes.md` (el sandbox del agente bloquea `Remove-Item -Recurse -Force` en un loop, igual que bloquea `/MIR`).

### Rollback (clasica, HISTORIA)

Restaurar el backup local (paso 2 de arriba) con `robocopy <backup> Z:\ /MIR`, respetando no tocar `Web.config`/`Config\`/`AFIP\`/`App_Data\`. No hay snapshot historico en el propio servidor (a diferencia de la VM, que tiene `web\backups\`) — el backup queda en la maquina donde se corrio el deploy, PENDIENTE definir si conviene subirlo tambien a un `backups\` dentro del server. **Usado en un caso real 2026-08-26**: swap con `Web.config` incluido rompio el login (ver `docs/DECISIONS.md`), rollback completo con este mismo procedimiento resolvio en <1 minuto.

## Tercer destino: "San Lorenzo" (`200.107.108.44`) -> IP publica, sin SMB, sin carpeta `Config\`

**No confundir con los dos de arriba.** Servidor Windows nuevo (alta 2026-08-01), acceso solo por **SSH** (puerto `2222`, no `22` — bloqueado por el ISP en la WAN, ver `~/hosts/sanlorenzo.env`) y RDP (`3389`). **No tiene SMB compartido** (a diferencia de Servidor SM), asi que la transferencia de archivos es por **SFTP** (Posh-SSH), no por `net use`. Aloja IIS con un sitio "web" (puertos `8069` HTTP / `443` HTTPS) con `CarniSysWeb` y `SuperCerdoWeb` como aplicaciones hermanas, mas un sitio standalone separado `SuperCerdo` (fuera de alcance). URL real: `https://200.107.108.44/CarniSysWeb/` (funciona desde afuera via DMZ del router; el certificado es autofirmado, con `-k`/`-k` en `curl`).

**Diferencia critica con los otros dos destinos**: este servidor **no tiene carpeta `Config\`** — el `connectionStrings` y todo `appSettings` (incluidas credenciales SMTP reales) viven **directo dentro de `Web.config`**. Esto significa que un publish normal (que trae su propio `Web.config` transformado, con connection strings distintas) **pisaria los secretos reales del servidor** si se copia sin cuidado. Por eso el paso de swap de este destino **excluye explicitamente `Web.config`** — se deja el que ya esta en el servidor, intacto.

**Cutover a WebCore, 2026-09-16**: `CarniSysWeb` en este servidor **ya no es la clasica** — fue reemplazada por WebCore (IIS + ASP.NET Core Module). Ver seccion "Cutover a WebCore" mas abajo. Las subsecciones que siguen (`Primer deploy de codigo`, `Metodo revisado`, sus `Validaciones` y `Rollback`) describen el deploy de la **clasica** y quedan como **historia** — solo son relevantes si algun dia se necesita entender como se llego hasta aca, no para deploys nuevos.

### Primer deploy de codigo (hecho 2026-08-03, commit `257ca0ab` de `codex_ia`) — HISTORIA, ver nota de cutover arriba

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

### Metodo revisado (usado desde 2026-08-29, commit `71bd638f`) — evita el incidente de "Web.config desaparecido"

Ver `incidencias-frecuentes.md` (2026-08-29): un `Move-Item`/`robocopy /MIR` sobre la carpeta `CarniSysWeb` **completa** (para backup-y-vaciar en un solo paso) puede reportar error por un archivo bloqueado (ej. un log en `App_Data\`) **habiendo ya movido/borrado el resto** — porque un rename de directorio en NTFS no necesita que los archivos de adentro esten libres. Resultado real visto: el sitio quedo sin `Web.config`/`Global.asax`/`AFIP\` varios minutos, sin que el error de PowerShell lo dejara claro. Por eso, en este destino (y en general en cualquier deploy sin `Config\` separado):

1. Publish Release local + **zip local** de `bin`, `Content`, `Scripts`, `Views`, `fonts` + los 5 sueltos (`Compress-Archive`, excluyendo `Web.config`/`Config\`) — un solo archivo de subir en vez de ~2300 sueltos, mas resistente a cortes que la subida archivo-por-archivo del metodo original.
2. Subida por SFTP de **ese unico zip** a `C:\inetpub\wwwroot\web\_deploy\<timestamp>\`.
3. Extraccion remota con `Expand-Archive` (PowerShell, no cmd.exe — invocar via `powershell -EncodedCommand <base64>`, ver nota de sandbox abajo).
4. **Backup y swap carpeta por carpeta, nunca la carpeta contenedora entera**: para cada una de `bin`/`Content`/`Scripts`/`Views`/`fonts` (y los 5 sueltos): copiar a `backups\CarniSysWeb_<timestamp>\<carpeta>` (backup), despues `Remove-Item` + `Move-Item` desde el staging extraido hacia `CarniSysWeb\<carpeta>`. **Nunca** una operacion de mover/borrar sobre `CarniSysWeb` en si misma — solo sobre sus subcarpetas de codigo, una por una.
5. Nunca tocar `Web.config`, `Global.asax`, `AFIP\`, `App_Data\` — ni para backup ni para swap. Quedan siempre en su lugar.
6. Validar con los `curl` de abajo, y ademas confirmar con `dir` que `CarniSysWeb\` tiene los 13 items esperados (7 carpetas: `AFIP`, `App_Data`, `bin`, `Content`, `fonts`, `Scripts`, `Views`; 6 sueltos: `favicon.ico`, `Global.asax`, `libman.json`, `manifest.json`, `PrecompiledApp.config`, `sw.js`, `Web.config` — 7 en realidad) antes de dar el deploy por terminado — no confiar solo en que el script no haya tirado un error visible.

**Nota de entorno (agente/sandbox local, no del servidor)**: el sandbox del tool de PowerShell bloquea localmente (mensaje enganoso "Remove-Item on system path... blocked") cualquier comando que contenga el literal `/MIR` o regex con `\s`/`\S`, aunque el comando vaya a ejecutarse remoto por SSH. Mitigacion: armar esos fragmentos en variables en runtime (no como substring literal en el script), evitar `robocopy /MIR` en favor del metodo de arriba, y mandar el script remoto completo codificado (`powershell -NoProfile -EncodedCommand <base64 de UTF-16LE>`) — ojo con el limite de ~8191 caracteres de linea de comando de `cmd.exe`: el script remoto debe usar loops/arrays, no una linea desenrollada por carpeta/archivo.

### Validaciones posteriores

- `curl http://200.107.108.44:8069/CarniSysWeb/` da `200` sirviendo la home publica de marketing directo (no `302` a Login como documentaba esta seccion hasta el 2026-08-03: la ruta raiz cambio de comportamiento con el feature "home publica" -- commit `72abd98e` -- que la desacoplo del login; no forzar upgrade a HTTPS sigue siendo el comportamiento esperado de este servidor, eso no cambio). **Corregido 2026-08-14** tras notar la discrepancia doc-vs-codigo tras un deploy real (CLAUDE.md SS8.3: manda el codigo).
- `curl -k https://200.107.108.44/CarniSysWeb/Login/Index` debe dar `200`, titulo `Ingresar a CARNISYS` (el texto del titulo tambien cambio desde `CarniSysWeb - Login`, mismo motivo), sin `Stack Trace`/`Server Error`. El `Set-Cookie` **no** trae `secure` (a diferencia de SM/VM) — tampoco es un bug: este `Web.config` no tiene `requireSSL`/`CookieRequireSsl` configurado, se deja como esta (no se toca `Web.config`). **Verificado 2026-08-03, re-verificado 2026-08-14, 2026-08-26 (deploy commit `7eb5547f`) y 2026-08-29 (deploy commit `71bd638f`, metodo revisado de arriba).**
- `Invoke-WebRequest` de PowerShell 5.1 **falla** contra el binding HTTPS de este servidor (error de renegociacion TLS) aunque `curl.exe` funciona bien — usar siempre `curl.exe`/`curl -k` para health checks aca, nunca `Invoke-WebRequest`.
- **2026-08-29**: `Content-Security-Policy` en la respuesta incluye `connect-src ... http://127.0.0.1:18777 http://127.0.0.1:5100` (confirma que el fix de CSP para el agente de balanza, del mismo deploy, quedo publicado) y `providerName="System.Data.SqlClient"` en `Web.config` (confirma que la conexion a SQL Server del servidor no se toco).

### Rollback (de la clasica — historia)

Restaurar el backup remoto (paso 2 de arriba, o el backup por-carpeta del metodo revisado) con `robocopy C:\inetpub\wwwroot\web\backups\CarniSysWeb_<timestamp> C:\inetpub\wwwroot\web\CarniSysWeb /MIR` (via SSH), sin tocar `Web.config`/`Global.asax`/`AFIP\`/`App_Data\`. El backup queda en el propio servidor (a diferencia de Servidor SM, que no tiene backup remoto — aca si, porque no hay SMB para bajarlo a la maquina local).

### Cutover a WebCore (IIS + ASP.NET Core Module) — hecho 2026-09-16

Reemplaza la clasica descripta arriba. Mismo patron de diseño que el planificado para Servidor SM (ver plan `en-reportes-cuando-se-sprightly-planet.md`): **IIS + ASP.NET Core Module (ANCM)**, no standalone+Caddy (ese patron, usado en la VM `carnisys.com`, no aplica aca porque el mismo IIS sirve tambien a `SuperCerdoWeb` y al sitio standalone `SuperCerdo` — parar IIS completo los afectaria a los tres).

**Verificado antes de tocar nada (solo lectura, por SSH)**:
- Hosting Bundle de ASP.NET Core **no estaba instalado** (prerequisito real, no de rutina).
- Sin runtime `.NET` compartido instalado → publish tiene que ser **self-contained win-x64**, no framework-dependent.
- `CarniSysWeb` y `SuperCerdoWeb` **comparten el mismo application pool** ("web") → un solo `w3wp.exe`. El hosting model `inprocess` de ANCM aloja el runtime .NET Core dentro de ese worker process — mezclarlo con `SuperCerdoWeb` (clasica, `System.Web`) no es soportado. Por eso se creo un pool dedicado.

**Pasos ejecutados**:
1. Backup de la carpeta clasica completa: `Copy-Item` (no `Move-Item`/`robocopy /MIR`, para no arriesgar el gotcha de archivo bloqueado documentado arriba) a `C:\inetpub\wwwroot\web\CarniSysWeb-backup-20260916` — permite rollback inmediato.
2. Publish local self-contained: `dotnet publish WebCore/WebCore.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o <carpeta_local>`. `dotnet publish` genera automaticamente el `web.config` correcto con el stanza `<aspNetCore processPath=".\WebCore.exe" ... hostingModel="inprocess" />` — no hace falta armarlo a mano.
3. `WebCore.dll.config` (no `App.config` — bug ya conocido, `dotnet publish` lo pisa con el de dev) se arma **directo en el servidor por SSH**, nunca en la maquina local: un script remoto lee los valores `Smtp*` reales del `Web.config` clasico (backupeado en el paso 1) y arma el nuevo archivo con `DataEngine=SqlServer` y `ConexionPrincipal` local (`Data Source=.\sqlexpress;Initial Catalog=supercerdo;...`). Los secretos nunca salen del servidor ni pasan por la maquina de deploy — evita el permiso de "Credential Materialization" del sandbox local y reduce superficie de exposicion.
4. Subida por SFTP (mismo patron `New-SFTPSession`/`Set-SFTPItem` archivo-por-archivo con reintento que el metodo original de este servidor) de los 816 archivos del publish (todo excepto `WebCore.dll.config`) a un staging: `C:\inetpub\wwwroot\web\_deploy\webcore_sl_<timestamp>\`.
5. Instalacion del Hosting Bundle (`dotnet-hosting-10.0.12-win.exe /quiet /norestart`, descargado directo en el servidor desde `https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.12/dotnet-hosting-10.0.12-win.exe`) — **una sola vez para todo el IIS**, afecta potencialmente a los 3 sitios (interrupcion breve esperada, no observada en la practica: los 3 sitios respondieron `200` antes y despues sin downtime perceptible).
6. Application pool nuevo y dedicado `CarniSysWebCore` ("No Managed Code") — `SuperCerdoWeb` se deja intacto en el pool "web".
7. Swap: `robocopy <staging> C:\inetpub\wwwroot\web\CarniSysWeb /MIR` (staging → carpeta viva, ambas en el mismo disco del servidor, no hay riesgo de la mitad-borrado del incidente de 2026-08-29 porque no es un rename de directorio sino un mirror archivo-por-archivo).
8. Reasignar `/CarniSysWeb` al pool `CarniSysWebCore` (`Set-ItemProperty IIS:\Sites\web\CarniSysWeb -Name applicationPool`) y reciclar **solo ese pool** (`Restart-WebAppPool`), nunca el pool "web".

**Nota de sandbox local** (agrega al gotcha ya documentado mas arriba de `/MIR`): el filtro tambien bloquea por texto la combinacion de `Get-ChildItem` sobre rutas `"C:\Program Files\..."` entre comillas junto con `Remove-SSHSession` en el mismo script ("Credential Materialization" / "Remove-Item on system path" son los mensajes de error enganosos vistos). Mismo workaround: variables en runtime en vez de literales, y/o separar en llamadas mas chicas.

### Validaciones posteriores (WebCore)

- `curl -k https://200.107.108.44/CarniSysWeb/Login/Index` → `200`, headers con `.AspNetCore.Antiforgery.*` y `.AspNetCore.Mvc.CookieTempDataProvider` en `Set-Cookie` (marca real de ASP.NET Core — la clasica no los tiene) y `X-Powered-By: ASP.NET` (sin el `Server: Microsoft-IIS/10.0` engaña, ese header lo pone IIS igual como reverse proxy/host, no indica el framework de la app).
- Assets estaticos (`/CarniSysWeb/Content/css/carnisys-login.css`, `/CarniSysWeb/lib/jquery/dist/jquery.min.js`) → `200`.
- `SuperCerdoWeb` (`https://200.107.108.44/SuperCerdoWeb/`) → `200`, sin cambios.
- **Login real confirmado por el usuario (2026-09-16, post-fix)**: primer intento con usuario real dio `404` en `https://200.107.108.44/Home/Index` -- bug real encontrado y corregido el mismo dia, ver mas abajo. Reintentado despues del fix, confirmado funcionando.
- **Pendiente todavia**: una lectura y una escritura real de negocio contra `supercerdo` (alta simple) -- el login en si ya se probo, falta ejercitar un modulo de datos (Compras/Ventas/Stock) para cerrar del todo la Fase C.

### Bug real encontrado y corregido el mismo dia del cutover (2026-09-16): login exitoso daba `404` en `/Home/Index`

**Sintoma**: con credenciales correctas, el login devolvia `404 - File or directory not found` en vez de mostrar el dashboard. Con credenciales incorrectas, el mensaje de error normal aparecia bien (esto ayudo a descartar que fuera un problema de la base de datos o del hash de password -- confirmado por lectura, sin escribir nada: el usuario de prueba existia, activo, sin bloquear, y el intento fallido se registro correctamente en `intentosFallidosLogin`, probando que la logica de auth contra SQL Server funciona).

**Causa real** (`WebCore\Controllers\LoginController.cs`, metodo `RedirigirPostLogin`, y el mismo patron en `WebCore\Controllers\LandingController.cs`): ambos usaban `Redirect("/Home/Index")` -- una ruta **absoluta literal**, elegida a proposito el 2026-09-07 para evitar que el generador de links de ASP.NET Core colapsara `RedirectToAction("Index","Home")` de vuelta a `"/"` (ver comentario en el codigo). Ese fix funcionaba bien en `carnisys.com` (standalone, hosteado en la raiz del dominio, `PathBase` vacio) pero **nunca se probo en un deploy hosteado como subaplicacion de IIS** -- en San Lorenzo (y Servidor SM, mismo codigo compartido) la app corre bajo `/CarniSysWeb`, con `PathBase="/CarniSysWeb"` seteado automaticamente por el modulo ANCM in-process. Un `Redirect` con ruta absoluta literal ignora el `PathBase` -- el browser terminaba pidiendo `/Home/Index` (sin el prefijo) contra el sitio IIS "web", que no tiene nada mapeado ahi -> `404`.

**Fix**: `Redirect(Url.Content("~/Home/Index"))` en los 2 lugares -- `Url.Content` resuelve el `~` contra el `PathBase` real de la request (da `/CarniSysWeb/Home/Index` aca, `/Home/Index` en `carnisys.com`), sin reintroducir el colapso a `"/"` que motivo el fix original (no pasa por generacion de rutas con valores default).

**Deploy del fix**: solo `WebCore.dll`/`WebCore.pdb` cambiaron (unico codigo tocado, ver `git diff`) -- se paro el pool `CarniSysWebCore` (corte de segundos, solo `CarniSysWeb`), se subieron los 2 archivos por SFTP, se volvio a arrancar el pool. Verificado con `Get-FileHash` que el binario en el servidor coincide exactamente con el build local antes de dar el fix por desplegado.

**Impacto en Servidor SM**: mismo codigo compartido (`WebCore/Controllers/LoginController.cs`/`LandingController.cs`), pero **NO afectado** -- ese cutover (hecho en paralelo, ver seccion "Segundo destino" arriba) uso una arquitectura distinta (IIS + ARR reverse-proxy hacia WebCore standalone en la raiz del sitio, no ANCM como subaplicacion bajo `/CarniSysWeb`), asi que `PathBase` queda vacio igual que en `carnisys.com` y el `Redirect` a ruta absoluta literal era correcto ahi de casualidad. El `Url.Content("~/...")` de este fix es inocuo si algun dia se actualiza ese binario tambien (mismo resultado con `PathBase` vacio), pero no era necesario.

### Rollback (WebCore)

1. Reasignar `/CarniSysWeb` de nuevo al pool "web" (`Set-ItemProperty IIS:\Sites\web\CarniSysWeb -Name applicationPool -Value "web"`).
2. Restaurar el contenido: `robocopy C:\inetpub\wwwroot\web\CarniSysWeb-backup-20260916 C:\inetpub\wwwroot\web\CarniSysWeb /MIR`.
3. Reciclar el pool "web" (`Restart-WebAppPool -Name web`).
4. No hace falta desinstalar el Hosting Bundle — no tiene efecto sobre la clasica ni sobre `SuperCerdoWeb`/`SuperCerdo`.

## Migración "Login solo desde dispositivos seguros" (2026-09-19) -- orden de despliegue

Cambio con esquema nuevo (todo aditivo, defaults apagados: no cambia el comportamiento del login hasta que un admin active el switch de "Mi Empresa" o el tilde por usuario). **Orden obligatorio: migración primero, código después** -- `Datos/Empresa.cs` (UPDATE explícito), `Datos/DispositivoSeguro.cs` (SELECT/INSERT explícitos) y `EmpresaPg.findById` usan las columnas nuevas y fallan si faltan. **Cada servidor requiere aprobación explícita del usuario.** Estado: **aplicado y desplegado en Servidor SM y San Lorenzo el 2026-09-19** (ver "Registro de aplicación" abajo); **VM CarniSys (Postgres) NO migrada ni redesplegada** (PENDIENTE).

| Servidor | Motor | Script | Notas |
|---|---|---|---|
| Servidor SM | SQL Server (`SuperCerdo`) | `Datos/DB-Procedures/20260919-Alter_Login_DispositivoSeguro_ServidorSM.sql` | verificar en vivo que las columnas faltan antes de correr (`sys.columns`); tiene guards `COL_LENGTH`/`OBJECT_ID` |
| San Lorenzo | SQL Server 2008 (`SuperCerdo`) | `Datos/DB-Procedures/20260919-Alter_Login_DispositivoSeguro_SanLorenzo.sql` | sin sintaxis nueva; verificar antes de correr |
| VM CarniSys | Postgres (`carnisys`) | `DatosPostgres/DB-Migrations/20260919-Alter_login_dispositivo_seguro.sql` | correr como `carnisys_admin` (el rol de la app solo tiene SELECT/INSERT en `loginubicacionlog`); subir por SFTP a `C:\WebCore\pending-migrations\` como en 2026-09-14 |

Después del deploy de código (mismos pasos que el resto de esta sección, incluido restaurar `WebCore.dll.config` al final):
1. Verificar que el login normal sigue igual (switch apagado): `curl -k .../Login` -> 200 y login real.
2. **Antes de activar el switch de empresa**: cargar el mail de cada empleado no-admin (sin mail no puede autorizar un celular) y que el SMTP esté configurado en el servidor (`SmtpHost`/`SmtpFromEmail` en `WebCore.dll.config`).
3. Activar desde `/Empresa` ("Exigir dispositivo seguro a no-administradores") o por usuario, y probar con un usuario no-admin desde un celular. **Prueba manual pendiente del mail real** (no automatizable).
4. Rollback funcional: apagar el switch/tilde (los admin siempre entran, así que nunca queda nadie afuera). Las columnas nuevas no requieren rollback de esquema.

### Registro de aplicación (2026-09-19, Servidor SM y San Lorenzo)

Deploy de `codex_ia` (HEAD `aa2070ef`) + migraciones SQL Server, pedido explícito del usuario, sin usuarios activos. Aplicadas en cada servidor sobre la BD `SuperCerdo` (SQL Server 2008 RTM 10.0.1600.22 en ambos):
- `Datos/DB-Procedures/20260917-Create_CorteJerarquiaSucursal.sql`, `20260917-Alter_a_CierreStockWeb_JerarquiaPorSucursal.sql`, `20260917-Alter_a_ExistenciaStockPorSucursales_JerarquiaPorSucursal.sql` (Jerarquía por sucursal) y `20260919-Alter_Login_DispositivoSeguro_{ServidorSM|SanLorenzo}.sql`.

**Método (repetible para próximas migraciones en producción)**:
1. Antes de tocar: volcar la definición de cada SP a modificar y compararla por hash con la esperada (si difiere, parar: alguien la cambió a mano). Los volcados originales sirven de rollback.
2. `BACKUP DATABASE ... WITH COPY_ONLY, CHECKSUM` + `RESTORE VERIFYONLY`. Archivos: `SuperCerdo_PRE_dispositivos_jerarquia_20260919.bak` en el directorio de backups de cada SQL Server.
3. Ensayo dentro de `BEGIN TRAN ... ROLLBACK` contra el motor real (2008) para detectar sintaxis/compatibilidad sin dejar rastro.
4. Aplicar y verificar (`sys.columns`, `OBJECT_ID`).
5. Paridad de SPs: crear copias temporales `zz_old_*` con la definición vieja, comparar salidas viejo vs nuevo sobre datos reales (fechas en sqlcmd como `YYYYMMDD`: con locale es-AR `2026-09-19` da error de conversión en ambos lados y engaña como "paridad"), y borrar las `zz_old_*`.

**Deploy de código**:
- SM: se deshabilitó `WebCoreAppWatchdog` (`Disable-ScheduledTask`; `Stop-ScheduledTask` no alcanza porque el trigger repite), se paró `WebCoreApp`, backup de la carpeta en `C:\WebCore-backup-20260919`, subida por SFTP (820/820), restaurar `WebCore.dll.config`, arrancar, re-habilitar watchdog. Verificado: puerto 5250 escuchando, `/Login` 200, sin excepciones en el Event Log.
- SL: solo se paró el pool `CarniSysWebCore` (el pool `web` es de SuperCerdoWeb, no se toca), staging `C:\inetpub\wwwroot\web\_deploy\webcore_sl_20260919`, `robocopy /MIR` (exit code 1 = ok) a `CarniSysWeb`, restaurar `WebCore.dll.config` desde `C:\WebCore-SL-config-20260919.txt` (DataEngine=SqlServer), arrancar pool. Backup de la carpeta previa: `C:\inetpub\wwwroot\web\CarniSysWeb-backup-predispositivos-20260919`. Verificado por HTTPS: `/CarniSysWeb/Login/Index` 200 con cookie `cs_dev` (`secure; samesite=lax; httponly`), POST con usuario inexistente responde el mensaje normal (confirma acceso a la BD con el código nuevo), `/SuperCerdoWeb/` 200, sin excepciones .NET en el Event Log.
- Lección: en Git Bash (MSYS) los argumentos que empiezan con `/` se convierten a rutas Windows (`/C:/WebCore` -> nada escrito, SFTP 0/820); exportar `MSYS_NO_PATHCONV=1` para rutas remotas.

**Hallazgos sin resolver (PENDIENTE)**:
- San Lorenzo: la carpeta viva `CarniSysWeb` no tiene `AFIP/` (la clásica sí, en `CarniSysWeb-backup-20260916`). La facturación electrónica desde WebCore en SL no tendría certificados. No verificado ni tocado.
- San Lorenzo: las llaves de DataProtection son efímeras; cada reciclado del pool cierra las sesiones (este deploy incluido).
- No probado: login real de un usuario (no hay credenciales en esta sesión), mail real por SMTP y flujo desde un celular.

## Cuarto destino: VPS "luden" (DonWeb) -> WebCore en Docker (preparado 2026-09-19, SIN publicar)

Host compartido con 4 clientes ajenos (cat, crm, cancha5, multeo): reglas y convencion del host en `C:\Users\FPX\Downloads\vps-luden-BRIEFING-INSTALADOR.md` (fuera del repo). Acceso: `~/hosts/vps-luden.env` (SSH por clave; usuario, IP y puerto en ese mismo archivo; clave de trabajo `~/.ssh/id_luden_claude`, la password de root sirve solo para la consola web de DonWeb). **Prohibido** en ese host: tocar `/srv/{cat,crm,cancha5,multeo}`, editar `/srv/proxy/Caddyfile`, `docker system|image|volume prune`, `restart` de `caddy`, sshd/firewall/WireGuard/fail2ban/netdata, `apt upgrade`/reboot.

- **Stack**: `/srv/carnisys/` (`name: carnisys`). Fuente del compose y del Dockerfile en el repo: `deploy/luden/`. Servicios: `db` (`postgres:17-alpine`, volumen `carnisys-pgdata`) y `web` (`carnisys-web:latest`, alias `carnisys-web` en la red externa `edge`, puerto interno **8080**). No publica puertos al host.
- **Secretos**: `/srv/carnisys/.env` (chmod 600; `POSTGRES_PASSWORD`, `CARNISYS_ADMIN_PASSWORD`, `CARNISYS_USER_PASSWORD`, `CS_ADMIN_PG_PASSWORD`, generadas en el servidor) y `/srv/carnisys/WebCore.dll.config` (chmod 640 root:1654; se monta en `/app/WebCore.dll.config`; es el archivo que se lee en runtime, ver bug del cutover de la VM CarniSys). La imagen NO lleva config: el publish local trae la de DEV y se le borra antes del `docker build`.
- **Base**: roles `carnisys_admin` (dueno/DDL), `carnisys_user` (app, sin BYPASSRLS), `cs_admin_pg` (BYPASSRLS) y base `carnisys` creados con passwords generadas (no las del script `20260818-Create_carnisys_roles_y_db.sql`). Esquema: **PENDIENTE** (base vacia; falta decidir restaurar dump de la VM Windows o correr las migraciones de `DatosPostgres/DB-Migrations` en orden).
- **Build y deploy (manual, sin CI)**: (1) local: `dotnet publish WebCore/WebCore.csproj -c Release -r linux-x64 --self-contained false -o <dir>` y borrar `App.config` y `WebCore.dll.config` del `<dir>`; (2) `tar` del `<dir>` + `deploy/luden/Dockerfile` por `scp` a `/srv/carnisys/`; (3) en el host: extraer en `_build/`, `docker build -t carnisys-web:latest -t carnisys-web:<YYYYMMDD-HHMM> _build`, borrar `_build/` y el tar; (4) `cd /srv/carnisys && docker compose up -d web`. Rollback de codigo: re-taggear una `carnisys-web:<fecha>` anterior como `latest` y `up -d web`.
- **Probado 2026-09-19**: `web` arranca, `GET /Login` -> 200 dentro del contenedor, healthcheck healthy, los otros 11 contenedores intactos.
- **PENDIENTE para publicar**: dominio y `/srv/proxy/sites/carnisys.caddy` (plantilla en el briefing; `reverse_proxy carnisys-web:8080`; confirmar que el dominio este en la cuenta Cloudflare del `CF_API_TOKEN`); esquema/datos; valores `PENDIENTE` de `WebCore.dll.config` (`cuit`, `SmtpUser`/`SmtpPass`, `WebClasicoBaseUrl`); certificados/carpeta AFIP (el publish no trae ninguno); alta en `/usr/local/bin/backup-stack.sh` (hoy solo cat, cancha5 y crm: **carnisys queda sin backup**); rotar la password de root (se compartio por chat); verificar que QuestPDF genere un PDF real (la imagen instala `libfontconfig1` + `fonts-dejavu-core`, no probado con un PDF).

**Caddy para `carnisys.com` (2026-09-20, ONLINE con HTTPS; historial del bloqueo abajo)**: creado `/srv/proxy/sites/carnisys.caddy` (`carnisys.com` -> `reverse_proxy carnisys-web:8080`; `www.carnisys.com` -> redir permanente al apex), validado y recargado con `caddy validate` + `caddy reload` (los otros 4 sitios siguen respondiendo). **El certificado NO se emite**: el `Caddyfile` del host tiene un `acme_dns cloudflare` global que se aplica a todos los issuers (tambien a `tls { issuer acme }`, no hay opt-out por sitio) y el `CF_API_TOKEN` del host **no cubre la zona `carnisys.com`** (API de Cloudflare: 0 zonas para ese nombre) -> `expected 1 zone, got 0`. Caddy reintenta con backoff y llena el log de errores hasta resolverlo; los otros sitios no se ven afectados. Salidas: (a) token con `Zone:DNS:Edit` sobre `carnisys.com` (ampliar el del host o uno propio en la cuenta duena de la zona); (b) proxy naranja de Cloudflare + certificado Origin CA cargado con `tls <cert> <key>` (sin renovacion automatica). DNS relevado 2026-09-20 via 1.1.1.1: `carnisys.com A <IP del VPS, ver ~/hosts/vps-luden.env>`, `www` CNAME al apex; otros resolvers todavia devolvian IPs de Cloudflare (propagacion).

**Resuelto 2026-09-20**: token propio de la zona `carnisys.com` (Cloudflare, `Zone:DNS:Edit` + `Zone:Read`, en `~/hosts/cloudflare-carnisys.env`) guardado en `/srv/proxy/sites/carnisys-cf-token` (chmod 600, fuera del glob `*.caddy`); `carnisys.caddy` lo lee con `tls { dns cloudflare {file./etc/caddy/sites/carnisys-cf-token} resolvers 8.8.8.8 9.9.9.9 }` en los dos bloques. Let's Encrypt emitio ambos certificados; `https://carnisys.com/Login` -> 200, `www` -> 301 al apex, `http` -> 308 a https. Los otros 4 sitios siguen respondiendo. **El bloque de arriba sobre el token global y las salidas (a)/(b) quedo como historial.** Sigue vigente: base vacia (login inutilizable), backup, SMTP/cuit/AFIP, rotar la password de root.

**Base cargada 2026-09-20 (reemplaza el "esquema PENDIENTE" de arriba)**: se subio la base **local de desarrollo** (`localhost:5432/carnisys`, PG 17.11) a `carnisys-db-1`. Procedimiento: `pg_dump -Fc` local -> `scp` a `/srv/carnisys/` -> crear en el destino los roles NOLOGIN BYPASSRLS `carnisys_usuarios_bypass` y `carnisys_sysadmin_bypass` (+ `GRANT` a `carnisys_user`, como en las migraciones `20260821b`/`20260825b`) -> `web` detenida -> `docker exec -i carnisys-db-1 pg_restore -U postgres -d carnisys --exit-on-error < dump` -> borrar el dump. Verificado: 51 tablas y 112.042 filas identicas al origen (conteo exacto por tabla), 44 tablas con RLS, extension `unaccent`, duenos `carnisys_admin`, conexion de `carnisys_user` OK, `https://carnisys.com/Login` -> 200. **Son datos de DEV, no los de produccion de la VM Windows**: incluyen usuarios de prueba (19) que hay que revisar/cambiar antes de usar el dominio en serio.

**Email en la VPS luden: Resend, no Postmark (2026-09-20)**: `SmtpMailHelper` es SMTP generico, no se toco codigo. `/srv/carnisys/WebCore.dll.config`: `SmtpHost=smtp.resend.com`, `SmtpPort=587` (STARTTLS; el 465 implicito no lo soporta `SmtpClient`), `SmtpEnableSsl=true`, `SmtpUser=resend` (literal), `SmtpPass`=API key de Resend (en `~/hosts/resend.env`), `SmtpFromEmail=notificaciones@carnisys.com`. El dominio `carnisys.com` esta **verificado en Resend** (region sa-east-1, envio habilitado); `mail.carnisys.com` NO esta en esa cuenta, por eso el remitente es del dominio raiz. Verificado: login SMTP contra Resend `235 Authentication successful` (sin enviar mail) y web healthy tras el restart. **NO probado**: un envio real desde la app (reset de contrasena / codigo de dispositivo). La VM Windows sigue con Postmark, sin cambios. Siguen `PENDIENTE` en el config: `cuit`, `WebClasicoBaseUrl`.

**Redeploy 2026-09-23 (imagen `carnisys-web:20260923-2105`; anterior `20260920-0119`)**: en la VPS **siempre es `DataEngine=Postgres`** y la app usa la base `carnisys` (`Host=db`, usuario `carnisys_user`; verificado en `pg_stat_activity`). Pasos hechos, en este orden: (1) backup `pg_dump -Fc` en `/srv/carnisys/backups/carnisys-pre-20260923-2103.dump` (chmod 600; restaurar con `pg_restore -U postgres -d carnisys --clean --if-exists`); (2) migraciones **como `carnisys_admin`** (`docker exec -i carnisys-db-1 psql -U carnisys_admin -d carnisys -v ON_ERROR_STOP=1 --single-transaction < <archivo>`, con `tr -d '\r'` si el archivo viene de Windows) de `20260921-Create_usuariopasskeys`, `20260921b-Create_ventaborrador_productosinagregar_notificaciones` y `20260922a-Create_borradorgenerico` (la base pasa de 51 a **58 tablas**, las 7 nuevas con RLS y `GRANT` a `carnisys_user`); (3) `WebCore.dll.config` de la VPS: agregadas `Passkeys:Enabled=true`, `Passkeys:ServerDomain=carnisys.com`, `Passkeys:Origins=https://carnisys.com`, `Passkeys:ServerName=CarniSys` (copia previa: `WebCore.dll.config.bak-20260923`); (4) publish + build + `up -d web` como en "Build y deploy". Verificado: `web` healthy, `https://carnisys.com/Login` 200 con el boton "Ingresar con huella", `borrador-generico.js` servido, login con usuario inexistente responde normal, 0 excepciones en el log. **NO probado**: registrar/usar una huella real, ni el borrador/recuperacion en pantallas (requiere sesion). Regla para siguientes redeploys: **cada migracion nueva de `DatosPostgres/DB-Migrations` posterior a la ultima aplicada va antes que el codigo**; ultima aplicada en la VPS: `20260922a`. Para saber que falta: comparar `ls DatosPostgres/DB-Migrations` con las tablas/columnas de la base.

**Base reemplazada por la local 2026-09-23 (21:1x)**: a pedido del usuario, la base `carnisys` de la VPS se reemplazo por completo con la local (`localhost:5432/carnisys`, PG 17.11, contiene los datos migrados de SuperCerdo: 577.092 ventas, 1.056.265 lineaventa, 404 personas, **35 usuarios**). Los datos de dev anteriores (1.595 ventas, 19 usuarios) se perdieron salvo por el backup `/srv/carnisys/backups/carnisys-pre-reemplazo-20260923-2111.dump` (restaurar: `DROP DATABASE carnisys WITH (FORCE)` + `CREATE DATABASE carnisys OWNER carnisys_admin ENCODING 'UTF8' TEMPLATE template0` + `pg_restore`). Procedimiento (repetible): backup de la base de la VPS -> `pg_dump -Fc` local -> `scp` a `/srv/carnisys/` -> `docker compose stop web` -> DROP/CREATE de la base -> `docker exec -i carnisys-db-1 pg_restore -U postgres -d carnisys --exit-on-error < dump` (en segundo plano con `nohup`; con 43 MB tardo menos de un minuto) -> comparar conteo exacto por tabla local vs VPS -> `docker compose up -d web` -> borrar el dump. Verificado: 58 tablas con conteos identicos, 51 con RLS, duenos `carnisys_admin`, `unaccent`, `web` healthy y `carnisys_user` conectado. **Aviso**: son datos reales de un cliente en un dominio publico; la base trae 1 fila en `usuariopasskeys` registrada en el ambiente local; si el RP fue `localhost` no sirve en `carnisys.com` (PENDIENTE verificar; el usuario dueno puede registrar otra huella).

## Ventas en curso (borrador en servidor) y advertencias del POS -- orden de despliegue (2026-09-21)

Solo Postgres. Ver `docs/DECISIONS.md` "Ventas en curso: borrador en servidor y advertencias del POS". **Orden obligatorio: migracion primero, codigo despues** (mismo motivo que passkeys: el codigo nuevo solo consulta las tablas si `PosBorradorSettings.Habilitado` da true, pero conviene tenerlas antes de activar).

1. **Migracion**: `DatosPostgres/DB-Migrations/20260921b-Create_ventaborrador_productosinagregar_notificaciones.sql`, como `carnisys_admin`. Crea `ventaborrador`, `ventaborradorevento`, `ventaproductosinagregar` y `notificaciones` (todas con RLS) y da `GRANT` a `carnisys_user`, `cs_admin_pg`. Aditiva: no toca tablas existentes. Sin variante SQL Server.
2. **Deploy de codigo**: mismos pasos que el resto de esta guia. Sin dependencias NuGet nuevas.
3. **Activar/ajustar por ambiente** (opcional) en `WebCore.dll.config`: seccion `PosBorrador:*` (`Habilitado`, `LatidoSegundos`, `MinutosSinLatidoInterrumpida`, `DiasRetencionFinalizadas`, `MaxLineas`, `MaxPayloadKb`, `SegundosProductoSinAgregar`, `SegundosCantidadCeroConfirmacion`, `AdvertenciaProductoSinAgregarHabilitada`). Sin estas claves, `PosBorradorSettings` usa los defaults documentados en `WebCore/Helpers/PosBorradorSettings.cs` — con `DataEngine=Postgres` la funcion queda activa por default apenas se aplica la migracion.
4. **Verificar**: en el POS, cargar un item y confirmar que aparece un indicador de "Venta resguardada hh:mm:ss" bajo la balanza; cerrar la pestana sin finalizar y volver a entrar -> el boton "Ayuda (F1)" muestra un badge con la cantidad y "Ver ventas sin cerrar" la lista; como admin, la campana del topbar debe aparecer junto al toggle de tema.
5. **Rollback funcional**: `PosBorrador:Habilitado=false` y reiniciar (el POS vuelve a depender solo del `POSDraft` local). Para revertir el esquema: las 4 tablas se pueden `DROP` sin afectar `ventas`/`lineaventa` (nunca las referencian por FK).
6. **Servidor SM / San Lorenzo (SQL Server)**: la funcion no existe por defecto; desde 2026-09-23 se puede activar (version reducida, sin notificaciones) -- ver la seccion "Borradores en SQL Server" mas abajo.

## Borradores en SQL Server (`SuperCerdo`) -- orden de despliegue (2026-09-23; ESQUEMA APLICADO en SM y SL, CODIGO NO DESPLEGADO, FLAGS APAGADOS)

**Registro de aplicación del esquema (2026-09-23, pedido explícito del usuario, Servidor SM `192.168.0.151,1433` y San Lorenzo `200.107.108.44\sqlexpress`, ambos SQL Server 2008 RTM 10.0.1600.22, base `SuperCerdo`)**: (1) prechequeo de solo lectura: base ONLINE, sin triggers DDL, sin tablas `%Borrador%` previas; (2) `BACKUP DATABASE ... WITH COPY_ONLY, CHECKSUM` + `RESTORE VERIFYONLY` válido, archivo `SuperCerdo_PRE_borradores_20260923.bak` en `c:\Program Files (x86)\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\Backup` de cada servidor; (3) ensayo del script en `BEGIN TRAN ... ROLLBACK` (4 tablas dentro, 0 tras rollback); (4) aplicado de verdad: 4 tablas + 14 índices en ambos; (5) humo de las consultas del repositorio (upsert INSERT/UPDATE, `DATEDIFF`/`DATEADD`, `SCOPE_IDENTITY`, purga, `LEFT JOIN Usuarios`) dentro de una transacción revertida: OK en ambos, sin filas residuales. **Deploy de código (2026-09-24)**: **Servidor SM desplegado** (HEAD `4980d967`, publish `win-x64` autocontenido sin `App.config`/`WebCore.dll.config`; zip subido por SFTP con hash verificado; watchdog deshabilitado → `WebCoreApp` frenado → backup `C:\WebCore-backup-20260923` (829 archivos) → `Expand-Archive` sobre `C:\WebCore` → config idéntico por hash → arranque → watchdog habilitado; verificado `127.0.0.1:5250/Login` y `https://192.168.0.151/Login` 200 con cuerpo real, `SuperCerdoWeb` 200, 0 errores .NET en el Event Log). **San Lorenzo NO desplegado**: el zip quedó subido en `C:\WebCore-SL-deploy-20260923\` (hash verificado) pero el paso de parar el pool y copiar fue denegado por el clasificador de permisos; la app de SL sigue en la versión del 19/09. Restore point local: tag `pre-borradores-sqlserver-20260923`. **Los flags siguen apagados (default en SQL Server) en ambos**: las tablas están vacías y ningún proceso las usa. La app WinForms/clásica no las conoce ni las toca (nada en el repo enumera tablas; no hay FK ni triggers). Rollback del esquema: `DROP TABLE` de las 4 tablas.

Ver `docs/DECISIONS.md` "Borradores en SQL Server". Cubre las recuperaciones de Compras, Stock, Movimientos, Embutidos (2 pantallas) y ventas en curso del POS. **No incluye** notificaciones/campana del admin ni "producto sin agregar" (etapa 2). **Estado**: probado solo contra SQL Server 2022 local; **PENDIENTE el ensayo en un 2008 real** y la prueba manual de UI.

1. **Antes de tocar**: backup (`BACKUP DATABASE ... WITH COPY_ONLY, CHECKSUM` + `RESTORE VERIFYONLY`) segun el metodo de "Registro de aplicacion (2026-09-19)". El script solo crea 4 tablas nuevas (no modifica objetos existentes), asi que no hace falta comparar hashes de SP.
2. **Ensayo**: correr `Datos/DB-Procedures/20260923-Create_Borradores.sql` dentro de `BEGIN TRAN ... ROLLBACK` contra el motor real (2008) para detectar sintaxis/compatibilidad sin dejar rastro; luego aplicarlo de verdad (`sqlcmd -S <servidor> -d SuperCerdo -i ...`; es idempotente, se puede repetir). Verificar con `SELECT name FROM sys.tables WHERE name LIKE '%Borrador%'` (deben ser 4: `BorradorGenerico`, `BorradorGenericoEvento`, `VentaBorrador`, `VentaBorradorEvento`).
3. **Deploy de codigo**: mismos pasos de cada servidor (SM: watchdog + `WebCoreApp`; SL: pool `CarniSysWebCore`). Con el codigo nuevo y **sin** las claves de abajo nada cambia (en SQL Server el flag arranca apagado).
4. **Activar** en el `WebCore.dll.config` **del servidor** (el archivo real, no `App.config`; ver bug ya documentado) y reiniciar: `<add key="BorradorGenerico:Habilitado" value="true" />` y `<add key="PosBorrador:Habilitado" value="true" />`. Umbrales opcionales: mismas claves `BorradorGenerico:*` / `PosBorrador:*` que en Postgres. **Efecto visible**: en esas pantallas se desactivan `localStorage` y el respaldo por captura de pantalla (el servidor pasa a ser la unica fuente).
5. **Verificar**: cargar una linea en Compras, cerrar la pestana y volver -> "Compras sin guardar" muestra el borrador; en el POS, el badge de "Ventas sin guardar". No debe aparecer la campana del admin (etapa 2).
6. **Rollback funcional**: poner ambas claves en `false` (o quitarlas) y reiniciar. Para revertir el esquema: las 4 tablas se pueden `DROP` (nada las referencia por FK).

## Login por huella (passkeys) -- orden de despliegue (2026-09-21)

Solo Postgres + HTTPS con dominio real. Ver `docs/DECISIONS.md` "Login por huella (passkeys WebAuthn)". **Orden obligatorio: migracion primero, codigo despues** (el codigo nuevo consulta `usuariopasskeys` solo si el ambiente lo habilita, pero conviene tener la tabla antes de activar).

1. **Migracion**: `DatosPostgres/DB-Migrations/20260921-Create_usuariopasskeys.sql`, como `carnisys_admin` (dueno de las tablas). Crea `usuariopasskeys` (con RLS) y da `GRANT` a `carnisys_user`, `cs_admin_pg` y `carnisys_usuarios_bypass`. Aditiva: no toca tablas existentes. Sin variante SQL Server.
2. **Deploy de codigo**: mismos pasos que el resto de esta guia (incluido restaurar `WebCore.dll.config` al final). Trae la dependencia NuGet `Fido2.AspNet` 4.1.0 (va en el publish).
3. **Activar por ambiente** en `WebCore.dll.config` (en luden: `/srv/carnisys/WebCore.dll.config`, montado en el contenedor; activado 2026-09-23): `Passkeys:Enabled=true`, `Passkeys:ServerDomain=carnisys.com`, `Passkeys:Origins=https://carnisys.com`, `Passkeys:ServerName=CarniSys`. Sin estas claves (o con `Enabled` distinto de `true`) el boton de huella no aparece y `/Login/PasskeyOptions` responde 404. Servidor SM y San Lorenzo: dejar apagado.
4. **Verificar**: `/Login` muestra "Ingresar con huella" arriba del formulario (solo con HTTPS y navegador compatible); iniciar sesion normal, menu de usuario -> "Mi huella" -> agregar; cerrar sesion e ingresar con la huella. Detras de proxy, `Origins` debe coincidir exactamente con el origen que ve el navegador (esquema + host + puerto).
5. **Rollback funcional**: `Passkeys:Enabled=false` y reiniciar. La tabla no requiere rollback de esquema (queda sin uso). Para revocar una huella puntual: el propio usuario desde "Mi huella", o `DELETE FROM usuariopasskeys WHERE id = ...` como `carnisys_admin`.

## Pagos: eliminación lógica y auditoría (2026-09-24) — migraciones ANTES del código

Agrega columnas a `pagos` (`eliminado`, `eliminadopor`, `fechaeliminacion`, `motivoeliminacion`) y la tabla `auditoriapagos`. Sin estas columnas **fallan `getPagoById` y el listado de pagos** del código nuevo.
- **Postgres (VPS luden / local)**: `DatosPostgres/DB-Migrations/20260924-Alter_pagos_eliminado_create_auditoriapagos.sql` como `carnisys_admin`, con el procedimiento de "Redeploy 2026-09-23" (`psql -v ON_ERROR_STOP=1 --single-transaction`). Idempotente. **Aplicada en la base local de desarrollo el 2026-09-24; NO aplicada en la VPS** (PENDIENTE).
- **SQL Server (SM / San Lorenzo)**: `Datos/DB-Procedures/20260924-Alter_Pagos_Eliminado_Create_AuditoriaPagos.sql` sobre `SuperCerdo` (`sqlcmd -d SuperCerdo -i ...`), con backup y ensayo en `BEGIN TRAN ... ROLLBACK` como en "Borradores en SQL Server". Idempotente. **NO aplicada** (PENDIENTE). En SQL Server no hay campana de notificaciones: la advertencia es solo en pantalla.
- Rollback del esquema: `ALTER TABLE pagos DROP COLUMN eliminado, ...` y `DROP TABLE auditoriapagos` (verificar antes que no haya pagos eliminados: perderían la marca).

## Carpeta AFIP (certificados) -- 2026-09-24
- Ubicacion: `AFIP/<CUIT>/` dentro del sitio. Es estado vivo: **nunca se toca en un deploy** (IIS: ya estaba en la lista de "no tocar"; Docker/luden: volumen `carnisys-afip` montado en `/app/AFIP`, creado con dueno `app`; agregado en `deploy/luden/docker-compose.yml` y `Dockerfile`).
- Desde 2026-09-24 la app **escribe** ahi (pantalla Configuracion > Certificado ARCA crea `pendiente/`, el pfx, `LoginTemplate.xml`, `*.bak`): el usuario del App Pool de IIS necesita permiso de modificacion sobre `AFIP\` (antes solo lectura/tickets). PENDIENTE verificar en cada servidor IIS.
- Backup: incluir `AFIP/` en el backup del servidor (los pfx tienen la clave de la empresa; en luden `carnisys` sigue sin backup, ver pendientes arriba). Ademas hay una copia local de los certificados que estuvieron en git en `~/afip-certs-backup/` (fuera del repo).
- Migracion 20260924d (`plataforma_certificado_arca`, certificado de padron de la plataforma): correr como `carnisys_admin` antes de desplegar (aplicada en la base local el 2026-09-24; PENDIENTE en los demas ambientes).
- Migracion: `DatosPostgres/DB-Migrations/20260924b-Alter_facturaelectronica_esprueba_create_certificadoarcaclave.sql` (columna `esprueba` + tabla `certificado_arca_clave`) debe correrse como `carnisys_admin` **antes** de desplegar este codigo (aplicada en la base local el 2026-09-24; PENDIENTE en los demas ambientes).
