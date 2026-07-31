# Riesgos conocidos

## Objetivo

Concentrar puntos sensibles que requieren especial cuidado antes de cualquier ajuste.

## Secciones

- Riesgo
- Area afectada
- Posible impacto
- Mitigacion

## 2026-07-29 - Web.config de produccion diverge del repo en `requireSSL`

- Riesgo: el `Web.config` que corre en la VM de produccion (`C:\inetpub\wwwroot\CarniSysWeb`) tiene a mano `requireSSL="false"` en `httpCookies`, `forms` y `Security:CookieRequireSsl`. El transform `Web.Release.config` del repo genera `true`. Si se deploya el Web.config "de fabrica" del repo sin este ajuste, se rompe el login.
- Area afectada: `Web/Web.Release.config`, autenticacion, deploy a la VM Windows (`carnisys.com`).
- Posible impacto: usuarios no pueden loguearse en produccion (la cookie de auth no se puede setear si ASP.NET ve la conexion como no-segura).
- Causa raiz: Caddy termina el TLS en el borde y le pasa el trafico a IIS por HTTP plano en `localhost:8069` (ver `C:\caddy\Caddyfile` en la VM). Caddy compensa agregando `; Secure` a cada `Set-Cookie` de salida, pero IIS/ASP.NET no sabe nativamente que la conexion "en verdad" es HTTPS (no hay middleware que confie en `X-Forwarded-Proto`).
- Mitigacion aplicada: en cada deploy manual, forzar `requireSSL="false"` en esos 3 lugares antes de copiar el `Web.config` a la VM (se hizo asi en el deploy del 2026-07-29). Pendiente evaluar como mejora futura: agregar un modulo/handler que lea `X-Forwarded-Proto` y fije `HttpContext.Current.Request` como seguro, para poder volver a `requireSSL="true"` de forma segura y que `Web.Release.config` deje de divergir de produccion.

## 2026-07-30 - `body.app-shell .btn-outline-*` pisa el hover de Bootstrap en toda la app

- Riesgo: `Content/css/ui-refresh.css` fuerza `background: transparent` en `.btn-outline-primary/secondary/dark` dentro de `body.app-shell`, con especificidad mayor al `:hover` propio de Bootstrap. El resultado: en **cualquier** boton outline de la app (no solo "Duplicar POS" en el POS, que ya se corrigio puntual, ver `incidencias-frecuentes.md` 2026-07-30), el hover deja el texto blanco (lo pone Bootstrap) sobre fondo transparente en vez de solido -> texto invisible en modo claro.
- Area afectada: `Content/css/ui-refresh.css` (regla global), cualquier vista con botones `.btn-outline-primary`, `.btn-outline-secondary` o `.btn-outline-dark`.
- Posible impacto: usabilidad (no funcional) — el boton sigue siendo clickeable, solo se pierde la legibilidad del texto en hover.
- Mitigacion sugerida (no aplicada, requiere confirmacion por ser un cambio de CSS global fuera del alcance del pedido puntual que lo encontro): agregar a esa misma regla los pares `:hover` con `background-color`/`color` explicitos de Bootstrap, o remover el `:hover` de su alcance (ej. `body.app-shell .btn-outline-primary:not(:hover)`).
