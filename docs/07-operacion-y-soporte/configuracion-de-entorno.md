# Configuracion de entorno

## Objetivo

Registrar requisitos, configuraciones locales y puntos necesarios para ejecutar o mantener el sistema.

## Secciones

- Herramienta o componente
- Configuracion
- Ubicacion
- Observaciones

## Levantar WebCore en local (recompilacion limpia + localhost)

Script: `Scripts\webcore-run.cmd` (doble clic; envuelve a `Scripts\webcore-run.ps1`). Alta: 2026-10-21.

```
Scripts\webcore-run.cmd                       # clean + build + levanta + abre el navegador
Scripts\webcore-run.cmd -Incremental          # sin clean (~5 s en vez de ~20-40 s)
Scripts\webcore-run.cmd -NoBrowser            # no abre el navegador
Scripts\webcore-run.cmd -Perfil https         # otro perfil de launchSettings.json (por defecto 'http')
Scripts\webcore-rapido.cmd                    # atajo de doble clic: igual que "webcore-run.cmd -Incremental" (alta 2026-10-28)
```

- Orden: (1) detiene el WebCore anterior **de este checkout** (procesos con binarios en `WebCore\bin\`; no toca otros `dotnet`, ni un WebCore de otro checkout: si otro proceso ocupa el puerto, aborta con el nombre y el PID); (2) `dotnet clean` + `dotnet build` de `WebCore\WebCore.csproj` (Debug); (3) `dotnet run --no-build --no-launch-profile` con la URL y las variables de `WebCore\Properties\launchSettings.json` (hoy `http://localhost:5270`, `ASPNETCORE_ENVIRONMENT=Development`); abre el navegador cuando el puerto responde.
- Ctrl+C o cerrar la ventana detiene el servidor. Si la compilacion falla no se levanta nada.
- La consola muestra solo errores + resumen de la compilacion (la solucion arrastra ~1.200 warnings previos, nullable y APIs obsoletas, que tapaban los errores reales).
- Por que compila siempre: las vistas Razor van precompiladas dentro de `WebCore.dll`; reiniciar el proceso no toma un `.cshtml` editado (ver `docs/DECISIONS.md`, gotcha de `dotnet run --no-build`). Los archivos de `wwwroot` (JS/CSS) se sirven desde disco, no requieren compilar.
- Arranque de WebCore con otro puerto o dos instancias a la vez: ver `incidencias-frecuentes.md` ("Dos WebCore de prueba en `localhost`..."), las cookies de `localhost` no distinguen puertos.
- Alternativa no incluida: `dotnet watch run --project WebCore/WebCore.csproj` (recarga en caliente); PENDIENTE de evaluar si sirve con las vistas precompiladas de este proyecto.
