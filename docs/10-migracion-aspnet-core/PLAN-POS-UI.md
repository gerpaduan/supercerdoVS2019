# Plan: UI de POS (`Ventas/POS.cshtml`) — CLAUDE.md §11.1

Continuación de `PLAN-POS.md` (backend del núcleo POS, los 7 batches ya portados y verificados con datos/AFIP reales). Este plan es solo para la VISTA en sí — el HTML/JS de `Web/Views/Ventas/POS.cshtml` (6226 líneas + ~1386 de partials + ~9500 de JS asociado), la pieza más grande de todo el programa de migración.

Investigación completa (estructura línea por línea, lista de JS, dependencias de servidor, acoplamiento a infraestructura local) delegada a un agente de exploración read-only el 2026-09-04 — resultado íntegro archivado en esta sesión, resumido abajo.

## 1. Alcance

Portar el flujo de **venta nueva en POS** (buscar producto → carrito → forma de pago → finalizar), reusando los 5 endpoints ya portados y verificados (`FinalizarVenta`, `ModificarVenta`, `BuscarProducto`, `BuscarExpendiosPOS`, `ObtenerExpendioPOS`) más `PuntosExpendioController.FinalizarPOS`. **Fuera de alcance de este plan** (quedan para después, ver sección 7): edición de la última venta (`idVentaEditar`/`soloFormaPago`, hoy depende de `Session["VentaActiva"]`), login de operador de producción (cuenta compartida, step-up de `Session`), multi-instancia ("Duplicar POS"), impresión con agente local, AFIP desde el modal de post-venta, calculadora de billetes, ayuda (F1-F8).

**Decisión de diseño heredada de `PLAN-POS.md`**: sin `Session`. La vista original inicializa `Session["VentaActiva"]` en el `GET POS` (línea 913 de `VentasController.cs`) y la lee de vuelta en cada acción — WebCore arma la venta en memoria en el propio `GET`, sin persistirla en ningún lado hasta que el usuario aprieta "Finalizar" (mismo criterio que ya se usó para `FinalizarVenta`/`ModificarVenta`).

## 2. Qué ya existe y qué falta (hallazgo real del agente de exploración)

**Reusable tal cual (sin `Session`, sin AJAX propia, o ya apuntando a un endpoint portado):**
- `pos-state.js` (284L) — estado del carrito en memoria, 100% portable.
- `pos-cart.js` (1093L) — tabla de líneas, modal "Línea de venta" (editar/bonificar/eliminar), portable casi entero (solo ~10 líneas sueltas de integración con expendios, que se agregan en el batch 5).
- `pos-keyboard.js` (192L), `pos-guard.js` (161L) — utilidades de UI puras.
- `pos-forma-pago-precios.js` (117L) — cálculo puro, sin servidor.
- `pos-product.js` (575L) — ya apunta a `BuscarProducto` (portado); ~30 líneas de parseo de código de expendio (`PE123F`) se posponen al batch 5.
- `ventas-expendios-pos.js` (642L) — ya apunta exclusivamente a `BuscarExpendiosPOS`/`ObtenerExpendioPOS` (portados).
- `forma-pago.js` (753L) — el `finalizarVenta()` real ya apunta a los endpoints portados; incluye pago mixto (ya soportado y verificado en el backend).

**Bloqueado o pospuesto a propósito:**
- Balanza física (`pos-balanza.js` + `Scripts/carnisys.balanza.js`, agente local `127.0.0.1:5100`) y scanner de cámara (`scanner.js`/zxing) — ambos degradan solos a modo manual sin el agente/permiso de cámara, no bloquean nada si se posponen.
- Impresión con agente local (`print-agent.js`, `127.0.0.1:18777`) y AFIP desde post-venta (`factura-electronica.js`) — el ticket tiene un fallback universal (`iframe` a `ImprimirTicket`, ya portable, + `window.print()` del navegador) que sí entra en un batch temprano.
- Login de operador de producción (`AutorizarOperadorPOS`/`CerrarOperadorPOS`, `Session`-heavy) — cuenta compartida, no la usa ningún juez de esta migración, se pospone completa.
- Edición de venta (`idVentaEditar`) — depende de permisos ya confirmados como código muerto bajo el stub Admin=true, pero el flujo de datos (cargar líneas existentes, `ModificarVenta` en vez de `FinalizarVenta`) es un batch aparte, no gratis.

## 3. Juez de paridad — limitación real, no una decisión cosmética

A diferencia del backend (verificable con HTTP directo + `sqlcmd`, sin necesitar interfaz), **una UI real necesita clickearse**. Esta sesión no tiene una herramienta de automatización de navegador (Playwright u otra) disponible. Mecanismo de verificación real para este plan:

1. Mecánico (lo que yo puedo hacer): build limpio, `dotnet run` + `curl`/lectura del HTML renderizado para confirmar que cada elemento/endpoint esperado está presente y que no hay errores de servidor.
2. **Visual/funcional (requiere al usuario)**: antes de dar un batch por cerrado, levantar `WebCore` y pedirle al usuario que lo pruebe en el navegador — clicks, atajos de teclado, el flujo completo de esa porción. Esto se declara explícitamente en cada entrega, no se asume éxito sin esa confirmación.

Se documenta como limitación real (CLAUDE.md §6.1: "no se reporta terminado sin verificación mecánica" + la regla de UI de CLAUDE.md: "si no podés testear la UI, decilo explícitamente").

## 4. Restore point

`git tag pre-pos-ui-YYYYMMDD` antes de arrancar el batch 1, y uno por batch grande (mismo criterio que el resto del programa).

## 5. Convención de UI (aplica desde el batch 1)

Sigue la regla ya establecida esta sesión (`docs/DECISIONS.md`, entrada 2026-09-04): densidad compacta (fuente `.82rem`-`.9rem`, controles `min-height` ~2.1rem-2.35rem) en vez del tamaño default de Bootstrap — el POS es la pantalla de uso más intensivo de todo el sistema, así que aplica con más razón que en ningún otro módulo.

## 6. Orden de batches propuesto

1. **Esqueleto**: acción `VentasController.POS()` GET (venta nueva en memoria, chequeo de caja abierta reusando `_oCierreN`, sin `idVentaEditar`/`soloFormaPago`/operador de producción todavía), vista base con topbar simplificado, tabla de carrito vacía, buscador de código + teclado numérico virtual, CSS base (sin todo el responsive fino de las 2609 líneas del `<style>` original, solo lo necesario para que no se vea roto).
2. **Carrito completo**: `pos-state.js`, `pos-product.js` (sin código de expendio), `pos-cart.js` completo (modal "Línea de venta": editar cantidad, bonificar, eliminar), `pos-keyboard.js`, `pos-guard.js`.
3. **Forma de pago + finalizar**: `_FormaPagoModal.cshtml`, `forma-pago.js` (incluye pago mixto, ya soportado en el backend), conectado a `FinalizarVenta`. **Primer punto verificable de punta a punta**: crear una venta real desde la UI.
4. **Balanza** (con degradación a manual si no hay agente): `pos-balanza.js` + `Scripts/carnisys.balanza.js`, modal "Producto Pesable".
5. **Expendios asociados**: `_ModalExpendiosPOS.cshtml`/`_ModalObservacionesExpendio.cshtml`, `ventas-expendios-pos.js`, integración en `pos-cart.js`/`pos-product.js` (conectado a `BuscarExpendiosPOS`/`ObtenerExpendioPOS`, ya portados).
6. **Post-venta básico**: `_ModalPostVenta.cshtml` recortado a imprimir ticket (fallback `iframe`+`window.print()`, sin agente de impresión) + email (reusa el patrón ya portado en Finanzas/Expendios).
7. **Cola final** (secundario, sin bloquear el POS operativo): edición de venta (`idVentaEditar`/`soloFormaPago`), AFIP desde post-venta (`factura-electronica.js`, ya probado el enganche en `PLAN-POS.md` batch 6), login de operador de producción, multi-instancia, ayuda (F1-F8), calculadora de billetes, comentario de venta, scanner de cámara, impresión con agente local.

Cada batch: `git tag` propio, build limpio, verificación HTTP donde aplique, y pedido explícito de prueba en navegador al usuario antes de cerrarlo (sección 3).

## 7. Gaps esperados

Se abre `docs/10-migracion-aspnet-core/GAPS-POS.md` si aparece algo que no se pueda portar 1:1 (por ahora, ninguno concreto más allá de lo ya documentado como pospuesto en la sección 2).

## 8. Qué necesito del usuario antes de arrancar

1. ~~Confirmar el orden de batches~~ -- **confirmado** (orden propuesto, 2026-09-04).
2. Confirmar la limitación del juez (sección 3): sin Playwright, la verificación visual/funcional la hace el usuario en el navegador, batch por batch.

## 9. Avance real (2026-09-04)

**Restore point**: `git tag pre-pos-ui-20260904` (antes de tocar código).

**Hallazgo y fix aparte, encontrado al armar el layout de POS**: ninguna vista de WebCore cargaba Font Awesome (`fas fa-*`) pese a que 68 vistas ya migradas usan esas clases -- los iconos no se veían en NINGUNA pantalla ya portada. Corregido agregando el CDN (misma versión 5.15.3 que vendoriza `Web/`, para paridad exacta de glifos) en `WebCore/Views/Shared/_Layout.cshtml`. Verificado que aparece tanto en `Stock/Index` (vista ya existente) como en la nueva `Ventas/POS`.

**Batch 1+2 (esqueleto + carrito completo, fusionados)**: portado y verificado mecánicamente.
- `VentasController.POS()` GET nuevo: venta siempre nueva (Consumidor Final, sin `Session`), chequeo de caja abierta reusando `_oCierreN.findByIdOrLast` (mismo criterio que el original). Si no hay caja abierta, muestra el modal "Apertura de Caja" (`Cajas/_AbrirCajaModal.cshtml`, portado) conectado al `Cajas.AbrirCaja` ya existente en WebCore -- el flujo completo caja-cerrada -> abrir caja -> POS operativo funciona sin código nuevo del lado de Cajas.
- `WebCore/Views/Shared/_LayoutPOS.cshtml` (nuevo, minimalista): topbar con marca + toggle de tema, sin dropdown de usuario/logout/KeepAlive (no hay Session real en WebCore).
- `WebCore/Views/Ventas/POS.cshtml` (nuevo): topbar simplificado (cliente=Consumidor Final fijo, botones "Buscar cliente"/"CF" visibles pero deshabilitados -- función pospuesta), carrito, buscador+teclado numérico+panel "info producto", modal "Línea de venta" completo (editar cantidad/bonificar/eliminar).
- JS portado **sin cambios de lógica**: `pos-state.js`, `pos-keyboard.js`, `pos-product.js`, `pos-cart.js`, `pos-guard.js` (copiados literales desde `Web/Scripts/app/`). Conectados a `Ventas/BuscarProducto`, ya portado y verificado en `PLAN-POS.md`.
- CSS: `WebCore/wwwroot/Content/css/pos.css`, port literal de las 2609 líneas del segundo `<style>` de `POS.cshtml` (con `@@media`/`@@keyframes` desescapados de Razor a CSS real). Varias reglas son inertes hasta que lleguen sus batches (balanza, expendios, post-venta).

**Verificación mecánica realizada** (sin Playwright, ver sección 3): build limpio (0 errores); `GET /Ventas/POS` devuelve 200 y renderiza la rama "caja abierta" (`#pos-app`) contra el estado real de la base local; los 5 JS + `pos.css` + el sonido de beep + bootstrap/jquery locales resuelven 200; `GET /Ventas/BuscarProducto?codigo=1` (ya verificado en `PLAN-POS.md`) sigue devolviendo el producto real (CARRE, id=33, $12000); sin excepciones en el log del servidor.

**Verificado en navegador real por el usuario (2026-09-04): batch 1+2 CERRADO.** Confirmó que funciona: búsqueda de código real → nombre/precio en el panel, cargar cantidad, "Agregar" → la línea aparece en la tabla del carrito, y editar/bonificar/eliminar la línea desde el modal. La caja ya estaba abierta en la base local (no hizo falta probar el modal de apertura en este pase).

**Excluido a propósito de este batch**: buscar/cambiar cliente, balanza y scanner (botón "*"/cámara deshabilitados, batch 4), expendios asociados (batch 5), impresión/email/AFIP de post-venta (batch 6), edición de venta existente, login de operador de producción, multi-instancia, ayuda, calculadora de billetes, comentario de venta (batch 7).

## 10. Batch 3 (forma de pago + Finalizar) -- 2026-09-04

Portado **sin cambios de lógica**: `Ventas/_FormaPagoModal.cshtml` y `Scripts/app/forma-pago.js` (753 líneas, incluye pago mixto) -- ambos ya apuntaban a `FinalizarVenta`/`ModificarVenta`, portados y verificados con datos reales en `PLAN-POS.md` (incluida una factura real de AFIP sobre una venta creada por este mismo mecanismo). Se conectó `window.api.venta.{finalizar,modificar,detalle}` a las acciones reales de `VentasController`.

**Diferencia deliberada**: `window.mostrarModalPostVenta()` se sobreescribe con un `Swal` simple ("Venta finalizada" + recarga) en vez de abrir el modal real de post-venta -- `modal-postventa.js` (1298 líneas: ticket, email, AFIP desde POS) no está portado todavía, llega en el batch 6. El botón "Finalizar" quedó habilitado.

**Verificado en navegador por el usuario (2026-09-04)**: venta real de punta a punta (buscar producto, agregar, Finalizar, elegir forma de pago) se guarda correctamente. Encontró "Pago Mixto" deshabilitado -- investigado y resuelto, ver `docs/DECISIONS.md` entrada "POS: 'Pago Mixto' ya no depende de preseleccionar forma de pago" (2026-09-04): no era un bug de la migración (con los parámetros reales de esta empresa, el original haría lo mismo), pero el usuario pidió el cambio deliberado de desacoplar pago mixto de la preselección de forma de pago -- implementado en `WebCore/wwwroot/Scripts/app/forma-pago.js` (única copia tocada, el `Web/` clásico no se toca). **Batch 3 CERRADO**: usuario confirmó que pago mixto ya funciona en el navegador con el nuevo comportamiento.

**Gap encontrado y corregido de paso** (2026-09-04): las ventas con pago mixto no mostraban esa condición en ningún listado de WebCore -- `Ventas/Index` (`_TablaVentas.cshtml`), `Ventas/DetalleVenta` (`_DetalleVentaCard.cshtml`), `Ventas/Facturas` (`_FacturasRows.cshtml`) y `Ventas/DetalleFactura.cshtml` mostraban solo `venta.FormaPago` crudo (ej. "Debito"), perdiendo el "+ Efectivo". Corregido aplicando el mismo formato que ya usa el original en `BuildFacturaDTO`/`_TicketHTML.cshtml` ("Debito | Efectivo") en los 4 lugares. Verificado contra la venta real de pago mixto (`idVenta=1740`): `Ventas/Index` muestra "B - Debito | Efectivo" y `Ventas/DetalleVenta` muestra "Forma de pago: Debito | Efectivo".

## 11. Batch 4 (balanza física) -- 2026-09-04

Portado **sin cambios de lógica**: `Scripts/carnisys.balanza.js` (cliente HTTP del agente local, `http://127.0.0.1:5100`) y `Scripts/app/pos-balanza.js` (orquesta modo manual/balanza del input cantidad). Se agregó el panel de estado (`#estadoBalanza`/`#barraBalanza`/`#msgBalanzaPOS`) y se habilitó la tecla "*" del teclado virtual (alterna manual/balanza). `#modalPesable` del original **no se porta**: confirmado código muerto, ningún JS lo referencia (ni en `pos-balanza.js` ni en ningún otro archivo del original).

**Verificado en navegador por el usuario (2026-09-04): batch 4 CERRADO.** Con una balanza Systel real conectada por COM3, el agente local (`Carnisys.Balanza.Agent`, ya compilado en el repo, instalado vía `Instalar-Local.cmd`) detecta la conexión y el panel de POS la muestra correctamente ("Conectada (COM3)").

**Nota operativa encontrada durante la verificación** (no es un bug de código, documentado para no repetir el diagnóstico): las vistas Razor (`.cshtml`) se compilan una sola vez al arrancar `dotnet run` -- a diferencia de los JS/CSS estáticos (que se sirven en caliente desde disco), un cambio en `POS.cshtml` no aparece con un simple refresh del navegador si el proceso de `dotnet run` viene corriendo desde antes del cambio; hace falta un restart real del proceso. Esto generó confusión en la verificación de este batch (el panel de balanza no existía en el DOM porque el usuario tenía un `dotnet run` viejo corriendo) -- se resolvió reiniciando el proceso.

## 12. Batch 5 (expendios asociados) -- 2026-09-04

Portado **sin cambios de lógica**: `_ModalExpendiosPOS.cshtml`, `_ModalObservacionesExpendio.cshtml` y `Scripts/app/ventas-expendios-pos.js` (642 líneas) -- ya apuntaban a `BuscarExpendiosPOS`/`ObtenerExpendioPOS`, portados y verificados con datos reales en `PLAN-POS.md`. El modal se abre con el atajo **AvPag (PageDown)** -- confirmado con grep que el original tampoco tiene un botón visible para esto, así que se porta el mismo atajo puntual (no el dispatcher completo de hotkeys F2-F8, que es batch 7) en vez de inventar un botón nuevo. `window.POSExpendiosCurrent` queda seteado, así que la integración ya existente en `pos-cart.js` (quitar línea de expendio) y `pos-product.js` (código de barra de expendio `PE123F`) empieza a funcionar sin tocar esos archivos.

**Verificación mecánica**: build limpio; JS nuevo pasa `node -c`; página `/Ventas/POS` renderiza con los 2 modales y las 2 URLs nuevas presentes; `BuscarExpendiosPOS` sigue devolviendo datos reales (21 expendios de la sucursal 2).

**Refinamiento pedido por el usuario tras probar el batch 5** (2026-09-04, ver `docs/DECISIONS.md` para el detalle completo de cada punto): filtros avanzados (fecha hasta + sucursal, con carga cross-sucursal habilitada a pedido explícito del usuario), columna Sucursal condicional, botón "Limpiar filtros" (también se dispara solo al abrir el modal y al finalizar una venta), checkbox "Cargar todos" con confirmación, y densidad compacta. Requirió un método nuevo aditivo `obtenerExpendiosAvanzado` en `Datos/Venta.cs` + `DatosPostgres/VentaPg.cs` + `Contratos.IVentaRepository`.

**Hallazgo grave encontrado de paso y corregido**: WebCore usa Bootstrap 5, pero las ~90 vistas ya portadas usan clases/atributos de Bootstrap 4 (`.badge-*`, `data-dismiss`, `.custom-switch`, etc.) que BS5 renombró o eliminó -- por eso el badge del modal no tenía color. Se agregó un shim de compatibilidad global (`bootstrap4-compat.css`/`.js`, cargado desde los 2 layouts) que corrige esto en **toda la app**, no solo en este modal. Ver `docs/DECISIONS.md` para el detalle técnico completo.

**Verificado mecánicamente** (build limpio en todas las capas tocadas -- WebCore, Datos, DatosPostgres, Negocio, net472 y net10.0 -- endpoints reales probados: `idSucursal=-1` trae todas las sucursales, `idSucursal=2` filtra San Lorenzo con el nombre real en la respuesta, dropdown de sucursales puebla con datos reales "San Martin"/"San Lorenzo").

**Verificado en navegador por el usuario (2026-09-04): batch 5 CERRADO**, incluido el refinamiento (filtros avanzados, cargar todos, cross-sucursal, shim de Bootstrap).

## 13. Batch 6 (post-venta básico: ticket + email, sin AFIP) -- 2026-09-04

**Hallazgo antes de portar nada**: `Imprimir`, `ObtenerDatosEmailComprobante` y `EnviarComprobanteEmail` ya estaban portados en `VentasController.cs` de un slice anterior (el header del archivo tenía una nota vieja diciendo lo contrario -- corregido de paso, ver `docs/DECISIONS.md` estilo de "comentario que miente es peor que ninguno", CLAUDE.md §2.6). Solo faltaba la UI de POS que los use.

**Nuevo**: `Ventas/_ModalPostVentaBasico.cshtml` -- versión reducida y deliberada de `_ModalPostVenta.cshtml` (198 líneas: ticket ESC/POS con agente local, factura AFIP automática, WhatsApp, configurar impresora). Cubre: "Imprimir comprobante" (abre el PDF ya portado en pestaña nueva, `documento=detalle`, el navegador hace de agente con su propio diálogo de impresión) y "Enviar por email" (modal con destino/asunto/mensaje precargados desde `ObtenerDatosEmailComprobante`, elige automáticamente `documento=factura` si la venta ya tiene factura AFIP asociada o `documento=detalle` si no). También agregado a pedido del usuario: pitido (`beep()`) al finalizar la venta.

**Verificación mecánica**: build limpio; página `/Ventas/POS` renderiza con los 2 modales nuevos y las 3 URLs; `ObtenerDatosEmailComprobante?id=1740` (la venta con factura AFIP real de antes) devuelve `tieneFactura:true` correctamente; `Imprimir?id=1740&documento=detalle` genera un PDF real de 1 página. **Deliberadamente NO probé `EnviarComprobanteEmail` en la verificación mecánica** -- ese endpoint manda un email real de verdad, no es algo para disparar solo de rutina; quedó para que el usuario lo probara él mismo sabiendo que es un envío real.

**`EnviarComprobanteEmail` probado por el usuario 2026-09-04, con hallazgo y fix intermedios**: primer intento falló con `535 5.7.0 Authentication Required` -- `WebCore/App.config` solo tenía placeholders de SMTP (nunca se habían cargado credenciales reales ahí). Al ir a cargarlas se encontró que `WebCore/App.config` estaba trackeado en git (a diferencia de `Web/Config/appSettings.secrets.config`, ya gitignorado) -- corregido antes de escribir ninguna credencial real, ver `docs/DECISIONS.md` (entrada 2026-09-04). Credenciales reales (mismas que ya usa `Web` en producción, VM CarniSys) cargadas en el `App.config` local gitignorado. **Confirmado por el usuario: el email funciona.**

**Excluido a propósito de este batch** (queda para el batch 7 o una iteración futura): factura AFIP automática disparada desde post-venta, WhatsApp, ticket ESC/POS con agente de impresión local, configurar impresora.

## 14. Batch 7 (cola final) -- EN PROGRESO, 2026-09-04

Arrancado con lo más chico y autocontenido: **comentario de venta** (`_ModalComentarioVenta.cshtml` + `Scripts/app/pos-comment.js`, portados sin cambios). Se agregó la fila `zona-botones` (Cancelar Venta / Comentario / Ayuda) que no existía en ningún batch anterior. Build limpio, JS verificado con `node -c`, modal renderiza sin errores de servidor.

**Iteración 2 (2026-09-04): Ayuda (F1) + Cancelar Venta**:
- **Ayuda**: `_ModalAyudaPOS.cshtml` + `Scripts/app/pos-help.js`, portados sin cambios. Los hooks F2-F8 (Cuentas corrientes/Calculadora de billetes/Nueva compra/Egresos caja/Historial de precios) degradan solos a un evento sin listener -- esos módulos no están portados. F9 (Buscar cliente) también degrada solo (botón disabled). F10 (Buscar producto) y AvPag (Expendios) sí funcionan. Se reemplazó el listener de PageDown ad-hoc del batch 5 por el hook real `window.posHotkeysHooks.AvPag`, ahora que el dispatcher que lo llama (`pos-help.js`) ya está portado -- mismo mecanismo que el original.
- **Cancelar Venta**: `guardarVentaAnuladaDirectamente` portado, con un recorte deliberado -- el original tiene 2 ramas más en el handler de click (`esEdicionVenta` → vuelve a `DetalleVenta` descartando cambios; `soloFormaPago` → vuelve atrás) que dependen de la edición de venta, todavía no portada. Ese caso queda para cuando se porte edición de venta. Se agregó `window.api.venta.pos` (URL de `POS`), necesaria para el fallback de `forma-pago.js` cuando `omitirPostVenta=true` -- sin esa URL el fallback navegaba a `"undefined"`.
- Build limpio (0 errores), `node -c` sobre `pos-help.js` OK.

**Iteración 3 (2026-09-04): Scanner de cámara + Calculadora de billetes (F3)**:
- **Scanner de cámara**: `scanner.js`/`zxing.min.js`/`barcode-code-input.js` ya estaban portados byte-a-byte (probados en `Stock/Editar.cshtml`) -- solo faltaba el wiring en POS. Se agregó el bloque HTML `#scannerContainer` (copia del original de POS, NO el partial `_ScannerCodigoBarra.cshtml` compartido con otros módulos, que trae un botón extra `#btnLimpiarScanner` que el POS original no usa) + el `BarcodeScanner` + los 2 `<script src>`. `manejarEnter` ya estaba expuesto global (`window.manejarEnter`, `pos-product.js:558`), no hizo falta adaptar nada.
- **Calculadora de billetes (F3)**: el JS (`calculadora-billetes.js`/`-targets.js`) ya estaba portado (usado por `Cajas/_AddOrEditEgresoCaja.cshtml`), pero **le faltaba el modal** -- el botón "Calcular efectivo" de Cajas estaba roto en producción (`window.CalculadoraBilletes.open()` corría pero `#modalCalculadoraBilletes` no existía). Se portó `_CalculadoraBilletesModal.cshtml` (nuevo en `WebCore/Views/Shared/`, incluido globalmente en `_Layout.cshtml` -- arregla Cajas de paso -- y puntualmente en `POS.cshtml`, que usa `_LayoutPOS.cshtml` y no pasa por `_Layout.cshtml`) + 2 endpoints nuevos en `HomeController.cs` (`ImprimirCalculadoraBilletesPayload`/`DescargarCalculadoraBilletesPdf`), con el PDF migrado de iTextSharp a QuestPDF (`GenerarDocsCore.GenerarPdfCalculadoraBilletes`, mismo patrón que el resto de los PDFs ya portados) -- iTextSharp no corre en .NET Core. `Session["Usuario"]` reemplazado por el mismo stub Id=2/Admin=true/IdEmpresa=1/IdSucursal=2 que el resto de los controllers.
  - **Bug real encontrado y corregido durante la verificación** (no solo build limpio -- se probó con datos reales via curl): los 2 endpoints nuevos no tenían `[FromBody]`. A diferencia de MVC5 clásico (bindea JSON crudo del POST automático), ASP.NET Core necesita `[FromBody]` explícito para bindear el `JSON.stringify` que manda `calculadora-billetes.js` -- sin eso, `Total` llegaba en 0 y `Denominaciones` vacío, silenciosamente (sin error). Mismo patrón que ya usan `FinalizarVenta`/`ModificarVenta` en `VentasController.cs:1421` (comentario ya explica el porqué). Verificado post-fix con curl real: total y detalle de denominaciones correctos.
  - "Descargar agente"/"Configurar impresora" (impresión local vía agente en `127.0.0.1:18777`) quedan con el link tal cual al original, inertes -- `print-agent.js` no está portado, ya excluido en todo el resto de la migración.
- Build limpio, `node -c` sobre los 3 JS OK.
- **Bug real encontrado el 2026-09-04, después de reportar esta iteración como "debería funcionar" sin haberlo probado en navegador real** (solo se había verificado con `curl`, que no ejecuta JS -- gap de metodología de verificación, corregido de acá en adelante: F3/botones de modal necesitan prueba en navegador, no alcanza con `curl`). El usuario confirmó que F3 no abría el modal, ni siquiera haciendo clic en el botón del modal de Ayuda (F1) -- eso descartó que el navegador reservara la tecla F3. Causa real, verificada contra el código fuente de `bootstrap.bundle.min.js` (v5.3.3): `CalculadoraBilletes.open()` en `calculadora-billetes.js` llamaba a `$('#modalCalculadoraBilletes').modal({backdrop:'static', keyboard:false})` -- en Bootstrap 4/3 pasar un objeto de opciones al plugin jQuery construía **y mostraba** el modal; en Bootstrap 5 el `jQueryInterface` cambió: si el argumento no es un string, solo corre `getOrCreateInstance` (crea/reconfigura la instancia) pero **nunca llama a `.show()`** -- la rama que ejecuta un método (`show`/`hide`/etc.) solo corre cuando el argumento es un string. Por eso no había ningún error en consola: la llamada era válida, simplemente no hacía nada visible. Afectaba también al botón "Calcular efectivo" de `Cajas/_AddOrEditEgresoCaja.cshtml` (mismo `open()` compartido) -- no confirmado si ese botón había sido probado en navegador real después del fix de la iteración 3 de arriba. **Fix**: agregar `$('#modalCalculadoraBilletes').modal('show');` como segunda línea después del `.modal({...})` (que sigue haciendo falta para fijar `backdrop`/`keyboard` la primera vez que se crea la instancia). Cache-busting (`?v=`) bumpeado en `_Layout.cshtml` y `POS.cshtml`. Verificado que el server sirve el archivo corregido real (`curl` al script, confirma la línea `.modal('show')` en el archivo servido) -- **pendiente que el usuario confirme en navegador real que el modal ahora aparece**, ya que la lección de este mismo hallazgo es no reportar "arreglado" sin esa prueba.

**Iteración 4 (2026-09-04): Multi-instancia ("Duplicar POS"), recortada**:
- `pos-multi-instance.js` (714 líneas) portado sin cambios de lógica. Recorte deliberado: sin login de operador de producción (`PosOperadorConfig`/`requiereOperadorPOS`, no existe en WebCore) -- `init()` se llama siempre, sin la condición que en el original pospone el arranque hasta autenticar operador.
- `VentasController.POS()` acepta `modoPos`/`posInstanceId`, los normaliza (mismo criterio que el original: `Guid.NewGuid()` si no viene) y los pasa por `ViewBag` (`PosModoInstancia`/`PosInstanceId`/`IdUsuarioPOS`, este último nuevo).
- `POS.cshtml`: badge `#posInstanceBadge` + botón `#btnDuplicarPOS` en `pos-top-actions`, `window.POSMultiInstanceConfig` + `window.POSModo.instancia`/`instanceId`, `window.POSMultiInstance.init()`.
- **Bonus de paso**: `window.POSModo` no existía en WebCore -- `forma-pago.js:594` mandaba `posInstanceId: ''` siempre (vestigio inerte de un port literal anterior). Con `window.POSModo.instanceId` ahora definido, ese campo viaja de verdad al finalizar una venta.
- `pvbContinuarNuevaVenta()` (post-venta básico, batch 6) llama a `window.POSMultiInstance?.closeAfterPostVenta?.()` antes del `reload()` -- mismo patrón que `modal-postventa.js` (el modal real, no portado) usa en `cerrarPostVentaYRecargar`, para que la pestaña "duplicado" se cierre sola y vuelva el foco al POS original.
- Verificado con curl real (no solo build limpio): `/Ventas/POS` sin parámetros genera `role:"original"` + un `instanceId` GUID nuevo y badge oculto; `/Ventas/POS?modoPos=duplicado&posInstanceId=dup-test123` devuelve `role:"duplicado"` y el badge en amarillo ("POS Duplicado"). Build limpio, `node -c` OK.
- **Comportamiento nuevo real, no un bug**: igual que el original, `checkConflicts()` bloquea activamente abrir una segunda pestaña normal de POS (sucursal/usuario) -- para atender dos clientes a la vez hay que usar el botón "Duplicar POS", no abrir otra pestaña a mano. Puede sorprender si se prueba con varias pestañas ya abiertas de antes.

**Iteración 5 (2026-09-04): Edición de venta existente (MVP, sin `soloFormaPago`)**:

Investigado a fondo antes de tocar código (CLAUDE.md §11.1) y confirmadas 3 decisiones con el usuario antes de implementar:
1. **Permisos**: bypass (siempre `true`), igual que el resto de WebCore. El usuario pidió explícitamente anotar que las reglas reales (`PuedeModificarUltimaVenta`/`PuedeCambiarFormaPago`) y "usuario producción" son importantes y se portan más adelante, junto con login/sesión real -- ver `docs/10-migracion-aspnet-core/gaps.md`.
2. **Bug heredado**: en el clásico, editar una venta y asociar un expendio desde el modal NO se persiste al guardar (sí funciona al crear) -- `Negocio/Venta.cs` `EjecutarModificarVenta` nunca llama `asignarVentaEnExpendio`, a diferencia de `EjecutarAgregarVenta`. Se replica tal cual (paridad exacta, decisión del usuario) -- no se corrige.
3. **Alcance**: solo edición completa. `soloFormaPago` ("Cambiar Forma de Pago", pantalla con bloqueo de UI) queda para una iteración siguiente.

**Cambios**:
- `VentasController.POS(...)` acepta `idVentaEditar`; si es >0, carga la venta (`_oVentaN.getVentaById`), valida que exista (si no, redirige con alerta) y setea `ViewBag.EsEdicionVenta`/`IdVentaEditar`.
- `POS.cshtml`: variables Razor + `lineasEdicionPos`/`listaExpendiosEdicionPos` (port literal de `Web/Views/Ventas/POS.cshtml:24-63`, incluida la lógica exacta de `anulado`/`indexAnulado` sin "corregir" nada que se haya notado ahí -- paridad de comportamiento), hidden `#idVentaEditar`, banner "Modificando venta #N" + aviso de precios históricos, textos condicionales de botones (Finalizar→Guardar, Cancelar Venta→Salir edición), precarga de `POSState`/`posCart`/`posComment`/`posExpendios` en `esEdicionVenta`, rama especial de "Salir edición" en el click de Cancelar Venta (Swal → `DetalleVenta?id=` sin guardar).
- `VentasController.DetalleVenta`: `ViewBag.PuedeModificarVenta = true` (bypass).
- `_DetalleVentaCard.cshtml`: botón "Modificar venta" → `POS?idVentaEditar=`.
- **Nada que tocar en `ModificarVenta`/`forma-pago.js`**: ya estaban completos de un slice/port anterior (`ModificarVenta` con lógica real, no un stub; `forma-pago.js` ya tenía la rama completa de post-guardado de edición con "Venta actualizada" + redirect a `DetalleVenta`) -- solo hacía falta que el DOM/JS los alimentara bien (`#idVentaEditar` con el id real, antes quedaba `undefined` → `idVenta` siempre `0`).
- Verificado con curl contra datos reales (venta #1740, la misma usada en batches anteriores): `esEdicionVenta=true`, banner con el número correcto, línea real precargada (`CARRE`, cód. 1, cant. 1, $15,00) en el formato exacto que espera `posCart`/`POSState`, botón "Guardar"/"Salir edición". Venta inexistente redirige sin romper. Venta nueva (sin el parámetro) sigue funcionando igual. Build limpio.

**Fix de seguimiento (mismo día, pedido del usuario tras probar)**: al abrir "Guardar cambios" en modo edición, el modal de forma de pago no indicaba cuál era la forma de pago actual de la venta ni precargaba los importes si era pago mixto. Causa: `window.POSFormaPagoActual` (que ya consume `window.preCargarFormaPagoActual` en `forma-pago.js`, portado sin cambios desde el batch 3) nunca se inicializaba desde el `Model` en `POS.cshtml` -- gap preexistente desde el batch 3, no algo nuevo de la edición. Port del bloque faltante (`Web/Views/Ventas/POS.cshtml:1716-1727`, literal). Verificado con curl contra la venta real #1740, que resultó ser un pago mixto real (`Debito` + `pagoMixtoEfectivo=5` sobre un total de `15`): `window.POSFormaPagoActual` sale con esos 3 valores exactos; confirmados en `_FormaPagoModal.cshtml` los ids/`data-tipo` que `preCargarFormaPagoActual` necesita (`data-tipo="Debito"`, `#chkPagoMixto`, `#montoEfectivo`, `#montoOtroPago`, `#labelOtroPago`) -- la lógica de marcado/precarga en sí no se tocó (ya estaba probada desde el batch 3), solo faltaba alimentarla. No se encontró ninguna venta con pago mixto real para un screenshot en navegador -- pendiente que el usuario lo confirme visualmente.

**Iteración 6 (2026-09-04): `soloFormaPago` ("Cambiar Forma de Pago")**:
- Reusa toda la mecánica de edición de venta de la iteración 5 (misma carga de `POSState`, mismo `ModificarVenta`) -- solo cambia el texto del banner ("Cambiando forma de pago") y bloquea la UI de productos.
- `VentasController.POS(...)` acepta `soloFormaPago`, pasado por `ViewBag.SoloFormaPago`.
- `POS.cshtml`: `aplicarModoSoloFormaPago()` (deshabilita inputCodigo/inputCantidad/btnAgregarManual/btnBuscarPersona/btnConsumidorFinal/btnScanner, bloquea la tabla, abre el modal de forma de pago automáticamente a los 250ms vía `POSGuard.requestModalOpen` -- ya cargado en `_LayoutPOS.cshtml` desde el batch 1) y `volverDesdeSoloFormaPago()` (vuelve a `DetalleVenta` sin guardar). El click de "Cancelar Venta" ahora chequea `soloFormaPago` primero (antes que `esEdicionVenta`), y el cierre del modal sin guardar (`hidden.bs.modal`) también dispara la vuelta -- salvo que ya se haya guardado (`window.POSSoloFormaPagoGuardado`, ya seteado en `forma-pago.js` desde el batch 3).
- `VentasController.DetalleVenta`: `ViewBag.PuedeCambiarFormaPago = true` (bypass, igual que "Modificar venta").
- `_DetalleVentaCard.cshtml`: botón "Cambiar Forma de Pago" → `POS?idVentaEditar=X&soloFormaPago=true`.
- Verificado con curl contra la venta real #1740: botón presente en `DetalleVenta`, banner "Cambiando forma de pago" en `POS`, `window.POSModo.soloFormaPago=true`, `aplicarModoSoloFormaPago()` invocado, `POSFormaPagoActual` con los valores reales (Debito/5/15, mismo fix de la iteración 5). Venta nueva y edición sin `soloFormaPago` siguen funcionando igual (sin regresión). Build limpio.

Con esto, **el batch 7 queda completo salvo lo bloqueado por falta de login/sesión real**:
- **Operador de producción (cuenta compartida)**: depende de una feature completa de Session (`PermisosHelper.RegistrarOperadorPOS/ObtenerOperadorPOS`, login step-up por pestaña) que no existe en absoluto en WebCore. Bloqueante real -- ver `gaps.md`.
- **Permisos reales de edición** (`PuedeModificarUltimaVenta`/`PuedeCambiarFormaPago`): mismo bloqueante, ver `gaps.md`.
