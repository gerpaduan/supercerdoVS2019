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
