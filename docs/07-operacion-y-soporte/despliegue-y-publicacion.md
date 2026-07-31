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

- `curl https://carnisys.com/` debe dar `302` a `/Login/Index`.
- `curl https://carnisys.com/Login/Index` debe dar `200`, sin `Stack Trace` / `Server Error` en el body.

### Rollback

Restaurar el backup de `web\backups\CarniSysWeb_<timestamp>` con `robocopy /MIR` hacia `CarniSysWeb` (respetando el mismo cuidado de no tocar `Config\`/`AFIP\`/`App_Data\` si ya cambiaron) y reiniciar el App Pool `CarniSys`.

### Nota sobre el sitio "web" y `_deploy`/`backups`

`C:\inetpub\wwwroot\web` es un segundo sitio IIS que **no** recibe trafico publico (Caddy solo enruta a "CarniSys"). Ahi vive el historial de `_deploy\` (paquetes extraidos) y `backups\` (snapshots). Al 2026-07-29 habia un build de staging del 28/7 parcheado a mano el 29/7 (`Web.config_before_dbfix_*`) que nunca se promovio a produccion y se dejo sin tocar.

## Segundo destino: "Servidor SM" (`192.168.0.151`) -> distinto de la VM de produccion "Carnisys"

**No confundir con la VM de arriba.** Hay dos servidores de deploy distintos para este proyecto:

- **Servidor "Carnisys"**: la VM Windows de produccion documentada arriba, acceso SSH (`~/hosts/carnisys-vm-windows.env`), publica detras de Caddy en `carnisys.com`.
- **Servidor "SM"** (`PCSERVIDORSM`, IP `192.168.0.151`): otro servidor en la LAN, sin acceso SSH conocido. Acceso por **SMB** (`\\servidorsm\carnisysweb` o `\\192.168.0.151\carnisysweb`), credenciales en `~/hosts/servidorsm.env`. Aloja IIS con un sitio "web" en el puerto **8069** que contiene, como aplicaciones separadas: `CarniSysWeb` (produccion de este servidor), `CarniSysWeb - copia`, y otro sitio independiente `SuperCerdoWeb` (otro proyecto, fuera de alcance). URL real: `https://192.168.0.151/CarniSysWeb/` (el `:8069` sobre HTTP redirige ahi con 301). PENDIENTE: confirmar si hay un hostname/dominio real para este server (hoy solo se probo por IP) y si el puerto 443 esta pensado para accederse solo desde la LAN/VPN (desde una sesion externa a esa red dio timeout).

### Pasos (probado 2026-07-30)

1. Publish Release precompilado a una carpeta local, igual que el paso 1 de la VM (mismo `msbuild ... /p:PublishProfile=FolderProfile /p:publishUrl=<carpeta_local> ...`). A diferencia de la VM, **no hace falta tocar `requireSSL`**: el transform de `Web.Release.config` (`true`) ya es el valor correcto para este servidor.
2. Backup del sitio actual: mapear el share con `net use Z: \\192.168.0.151\carnisysweb /user:ServidorSM\carnisys-deploy <password>` y `robocopy Z:\ <carpeta_local_backup> /MIR`.
3. Copiar el build nuevo con `robocopy <publish>\<carpeta> Z:\<carpeta> /MIR` para `bin`, `Content`, `Scripts`, `Views`, `fonts`, y los sueltos (`Web.config`, `favicon.ico`, `libman.json`, `manifest.json`, `sw.js`, `PrecompiledApp.config`) con `Copy-Item -Force`.
4. **Nunca tocar** `Config\`, `AFIP\`, `App_Data\` del share (mismo motivo que en la VM: estado vivo, no build).
5. `net use Z: /delete` al terminar.
6. No hay pipeline ni acceso remoto para reciclar el App Pool a mano; IIS/ASP.NET recicla el AppDomain solo al detectar cambios en `bin\` o `Web.config`, asi que no hace falta paso manual.

### Validaciones posteriores

- `curl http://192.168.0.151:8069/CarniSysWeb/` debe dar `301` a `https://192.168.0.151/CarniSysWeb/` con los headers de seguridad (`Content-Security-Policy`, `X-Frame-Options`) del `Web.config` publicado.
- El `200` final de `/CarniSysWeb/Login/Index` sobre HTTPS **no se pudo verificar en la sesion del 2026-07-30** (puerto 443 no alcanzable desde esa sesion) — validar desde un navegador en la LAN.

### Rollback

Restaurar el backup local (paso 2 de arriba) con `robocopy <backup> Z:\ /MIR`, respetando no tocar `Config\`/`AFIP\`/`App_Data\`. No hay snapshot historico en el propio servidor (a diferencia de la VM, que tiene `web\backups\`) — el backup queda en la maquina donde se corrio el deploy, PENDIENTE definir si conviene subirlo tambien a un `backups\` dentro del server.
