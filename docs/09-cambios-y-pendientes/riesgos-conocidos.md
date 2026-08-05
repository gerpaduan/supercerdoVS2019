# Riesgos conocidos

## Objetivo

Concentrar puntos sensibles que requieren especial cuidado antes de cualquier ajuste.

## 2026-08-04 - `a_CierreStock` (SP compartido con WinForms) tiene 2 bugs reales de calculo

- Riesgo: `dbo.a_CierreStock` (usado por Stock Actual/Cierre Stock, ver `docs/DECISIONS.md` 2026-08-05 para el detalle completo) tiene 2 bugs de comportamiento encontrados al escribir su reemplazo `a_CierreStockWeb`: (1) arma la sucursal via `CROSS APPLY (SELECT TOP 1 * FROM Sucursal WHERE idSucursal=@idSucursal) s`, que sin coincidencias se comporta como `INNER JOIN` -- con `@idSucursal=0` (el default cuando el usuario ve el reporte sin filtrar por una sucursal puntual) el SP devuelve **0 filas sin error**, no un error visible; (2) el filtro `enCierreStock=1` no se aplica en el mismo punto segun si hay `@texto` o no -- buscar un producto por texto se saltea ese filtro, mostrando productos que no deberian entrar en el cierre de stock.
- Area afectada: `dbo.a_CierreStock`, `Negocio.Corte.CierreStock`, cualquier pantalla de WinForms que use "Stock Actual"/"Cierre Stock" sin elegir una sucursal puntual o buscando por texto (`Presentacion/Cortes/formReporteStock.cs`, `Presentacion/Stock/formStockActual.cs`, `Presentacion/Stock/formAddOrEditStock.cs`).
- Posible impacto: en WinForms, un usuario que entra a Stock Actual/Cierre Stock sin fijar sucursal puede ver una grilla vacia sin ningun mensaje de error (bug 1); una busqueda por texto puede traer productos fuera del cierre de stock (bug 2).
- Mitigacion: **no aplicada en WinForms** -- por regla del proyecto ese SP y ese cliente no se tocan nunca sin poder probarlo. En Web, `a_CierreStockWeb` (SP nuevo, exclusivo de Web) corrige ambos bugs de raiz. Si en el futuro se decide corregir tambien el lado WinForms, coordinar una sesion de testing manual de esa app antes de tocar el SP compartido -- no asumir que el fix de Web es trasladable sin verificarlo ahi.

## 2026-08-04 - DESCARTADO: sospecha de que Tipo/IVA/Independiente se reseteaban al presionar "Modificar" en `Productos/AddOrEdit`

- Lo que se penso originalmente: durante testing en vivo se vio, en un producto real (CABEZA), que despues de presionar "Modificar" (`#btnHabilitarEdicionProducto`) los `<select>` de Tipo e IVA quedaban en blanco. Se documento como riesgo serio y sin causa raiz identificada.
- **Retestado a fondo y DESCARTADO** (mismo dia, un rato despues): con `sessionStorage` limpio y una carga fresca de la pagina, el mismo producto (CABEZA) y otro distinto (CARRE) NO reproducen el problema — Tipo/IVA/Independiente conservan su valor correctamente despues de "Modificar", verificado ademas instrumentando el setter de `.value` del `<select>` (cero escrituras interceptadas). La causa real de lo que se vio antes: esa sesion de testing habia hecho **muchisimas** navegaciones y submits fallidos seguidos sobre el mismo `idCorte=34` en la misma pestaña de Chrome, dejando el `sessionStorage` de la pagina (`guardarModoEdicionActivo`, flags de "carga continua", historial de navegacion entre productos) en un estado que ningun usuario real generaria en un uso normal.
- **No hay bug para corregir** en `edit-readonly.js` ni en `AddOrEdit.cshtml` por esto — se deja esta entrada como registro de que se investigo y se descarto, para no volver a sospechar de lo mismo si alguna vez se repite un sintoma parecido (en ese caso, sospechar primero del estado de `sessionStorage`/pestaña del navegador antes que del codigo).

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

## 2026-08-03 - El script versionado de `a_CierreStock` esta desactualizado respecto al SP real

- Riesgo: `Datos/DB-Procedures/20200516-Alter_a_CierreStock.sql` (el unico script versionado de este SP) define la firma `(@texto, @idsucursal, @fechaDesde, @fechaHasta)`, pero `Datos/Corte.cs:CierreStock` (el codigo C# que lo invoca hoy) envia ademas `@tipo`, `@idProveedor`, `@idMarca` y, cuando no es el modo "clasico" (`StockCierre_2`), `@idEmpresa` — parametros que ese script ni siquiera declara. El SP real en la base tuvo que haber sido alterado directamente en algun momento sin subir un script nuevo a `Datos/DB-Procedures/`.
- Area afectada: `dbo.a_CierreStock`, `Negocio.Corte.CierreStock`, `ReportesController.CalcularEstadoStock` (consume el mismo DataTable).
- Posible impacto: nadie puede confiar en el script del repo como fuente de verdad de este SP puntual — si se necesita modificar su logica SQL, hay que traer primero el texto real desde la base (`sp_helptext a_CierreStock`) antes de tocarlo, o se corre el riesgo de sobreescribir parametros/columnas que el C# ya asume que existen.
- Mitigacion aplicada (2026-08-03, ver bitacora de cambios): para el cambio de punto de stock por sucursal, se evito tocar este SP — en cambio, `Negocio.Corte.CierreStock` sobreescribe en C# la columna `Pto.Stock` del `DataTable` ya devuelto, leyendo el valor real desde `dbo.CortePuntoStockSucursal`. Verificado con Chrome real contra `/Reportes` (Stock Actual): pisando a mano un valor en `CortePuntoStockSucursal` distinto del legacy `Corte.puntoStock`, el reporte mostro el valor nuevo — confirma que el override esta activo. Pendiente (no bloqueante): si en el futuro hace falta editar la logica SQL de `a_CierreStock` en si, traer el texto real de la base y subir un script nuevo versionado antes de modificarlo a ciegas.

## REGLA - Antes de un `ALTER PROCEDURE` basado en un script viejo del repo, comparar contra el texto real de la base (2 veces confirmado, 2026-08-03)

- **Ocurrencias**: (1) `dbo.a_CierreStock`/`Acum_Ventas` (2026-08-01, ver `incidencias-frecuentes.md` y la entrada de arriba) — el script versionado no tenia el filtro `idEmpresa` que ya estaba parcheado en vivo. (2) `dbo.a_ExistenciaStockPorSucursales` (2026-08-03, esta sesion) — **el mismo caso, dos veces sobre el mismo SP**: el script versionado mas reciente (`20260516-Alter_a_ExistenciaStockPorSucursales_IdCorte.sql`) le faltaban DOS parches ya aplicados en vivo el 2026-08-01 y nunca versionados: el filtro `@idEmpresa` en `#AllCortes` (la correccion de performance documentada ese dia) y una columna `pesable` que `Datos/Corte.cs:ObtenerExistenciaPorSucursalesPlano` ya lee del resultado (`dr["pesable"]`). Al correr un `ALTER PROCEDURE` basado en ese script viejo (sin saberlo) se **revirtieron ambos parches**, causando una regresion real: la pantalla volvio a tardar minutos (mismo sintoma que el incidente original) y el modal "Ver stock por sucursales" tiro error ("Error al consultar... pesable") hasta reconstruir la columna faltante a mano.
- **Causa raiz**: los SPs de este repo se parchean directo en la base durante sesiones de debugging/fix, y no siempre se sube un script nuevo a `Datos/DB-Procedures/` con ese parche — el archivo versionado mas reciente puede estar desactualizado sin que nada lo marque como tal.
- **REGLA**: antes de escribir o correr un `ALTER PROCEDURE` para cualquier SP de este repo (no solo los ya mencionados), traer primero el texto real desde la base con `sp_helptext '<nombre_sp>'` (o `OBJECT_DEFINITION(OBJECT_ID('<nombre_sp>'))`) y diffearlo contra el script versionado mas reciente en `Datos/DB-Procedures/`. Si difieren, el texto de la base manda — versionar ese diff en un script nuevo antes de construir el cambio propio encima. Aplica en cualquier maquina/servidor: la base local de este dev, la VM de produccion y los servidores SM/San Lorenzo pueden divergir entre si ademas de divergir del repo.
- **Mitigacion para el caso puntual de `a_ExistenciaStockPorSucursales`**: reconstruido en `Datos/DB-Procedures/20260803-Alter_a_ExistenciaStockPorSucursales_PuntoStockPorSucursal.sql` (queda versionado con el filtro `idEmpresa` y la columna `pesable` incluidos, ademas del cambio de punto de stock por sucursal). Aplicado y verificado en la base local (`.\sqlexpress`). **Pendiente**: aplicar el mismo `ALTER PROCEDURE` en los servidores reales (VM `carnisys.com`, SM, San Lorenzo) cuando el usuario lo confirme — ahi el SP puede tener el mismo drift (el parche del 2026-08-01 tampoco se aplico en esos servidores, ver la entrada de `incidencias-frecuentes.md`).

## 2026-07-30 - `body.app-shell .btn-outline-*` pisa el hover de Bootstrap en toda la app

- Riesgo: `Content/css/ui-refresh.css` fuerza `background: transparent` en `.btn-outline-primary/secondary/dark` dentro de `body.app-shell`, con especificidad mayor al `:hover` propio de Bootstrap. El resultado: en **cualquier** boton outline de la app (no solo "Duplicar POS" en el POS, que ya se corrigio puntual, ver `incidencias-frecuentes.md` 2026-07-30), el hover deja el texto blanco (lo pone Bootstrap) sobre fondo transparente en vez de solido -> texto invisible en modo claro.
- Area afectada: `Content/css/ui-refresh.css` (regla global), cualquier vista con botones `.btn-outline-primary`, `.btn-outline-secondary` o `.btn-outline-dark`.
- Posible impacto: usabilidad (no funcional) — el boton sigue siendo clickeable, solo se pierde la legibilidad del texto en hover.
- Mitigacion sugerida (no aplicada, requiere confirmacion por ser un cambio de CSS global fuera del alcance del pedido puntual que lo encontro): agregar a esa misma regla los pares `:hover` con `background-color`/`color` explicitos de Bootstrap, o remover el `:hover` de su alcance (ej. `body.app-shell .btn-outline-primary:not(:hover)`).
