# Plan: UI de POS de Puntos de Expendio (`PuntosExpendioController.POS`)

CLAUDE.md §11.1 — plan escrito antes de tocar código. Scoping completo (investigación vía agente de exploración, solo lectura) resumido acá; no se duplica el detalle técnico línea por línea del clásico, solo lo que importa para decidir y verificar.

## Contexto

Ya portada y verificada en navegador la UI completa del POS de Ventas normales (`WebCore/Views/Ventas/POS.cshtml`, ver `PLAN-POS-UI.md`, 7 batches). El backend transaccional de Expendios (`FinalizarPOS`, `BuscarProductoPOS` pendiente) y el catálogo de Sectores ya están portados y verificados con datos reales (`docs/10-migracion-aspnet-core/README.md`, Módulo 8 slice 2). Falta la vista `POS.cshtml` en sí.

## Tamaño y reutilización (confirmado, no estimado a ojo)

- Clásico: `Web/Views/PuntosExpendio/POS.cshtml` (1980 líneas) + `punto-expendio-pos.js` (974) + 4 partials propios (434) = **~3900 líneas**, contra ~15600 del POS de Ventas ya migrado -- **~25% del tamaño**.
- **Los 9 JS compartidos que necesita (`pos-state.js`, `pos-cart.js`, `pos-keyboard.js`, `pos-guard.js`, `pos-product.js`, `pos-balanza.js`, `pos-help.js`, `pos-comment.js`, `pos-multi-instance.js`) YA están portados en WebCore** (se usan en Ventas/POS) -- se reutilizan tal cual, cero cambios.
- `pos.css` ya portado (literal), pero le faltan las clases propias de Expendio (`.pos-expendio-*`, ~400-500 líneas del `<style>` original) -- hay que agregarlas, no asumir cobertura total.

## Diferencias de negocio vs. el POS de Ventas ya portado

- **Sin forma de pago**: el expendio se crea `FormaPago=Nulo`/`TipoComprobante='X'`, queda pendiente de cobro -- el cobro real pasa después por `Ventas/POS` (modal de expendios asociados, ya portado). No se porta `_FormaPagoModal.cshtml`/`forma-pago.js` para este flujo.
- **Sector obligatorio**: concepto exclusivo de Expendio. Modal de selección de sector bloquea "Finalizar" hasta elegir uno. Catálogo ya portado.
- **Cliente = texto libre** (no `Persona` real, salvo "Consumidor Final").
- **Con balanza** (igual mecanismo que Ventas, ya portado y wireado).
- **Sin scanner de cámara wireado**: confirmado que el propio original no lo conecta (HTML del contenedor existe pero sin `<script src="scanner.js">`) -- no se inventa wiring que el original no tiene.
- **Con "Duplicar POS"**: mismo `pos-multi-instance.js`, namespaced con `productKey: 'expendio'` (el propio módulo ya soporta esto, ver comentario en `pos-multi-instance.js:22-30`, agregado cuando se portó para Ventas).

## Decisión de permisos: mismo bypass ya vigente, no una decisión nueva

`requiereOperadorPOS` (que en el clásico puede ocultar la vista ENTERA si el usuario de producción no autorizó operador) se hardcodea a `false` -- **mismo gap ya registrado** en `gaps.md` ("Permisos reales de venta y 'usuario producción'"), no es una decisión nueva de este plan. `MisExpendiosPOS`/F6 (depende de `ResolverOperadorPOS`) queda fuera del MVP por el mismo motivo, sin equivalente tampoco en el Ventas/POS ya portado.

## Scope de esta iteración (MVP)

**Incluye:**
1. `PuntosExpendioController.POS()` GET (nueva) -- sin Session, `requiereOperadorPOS=false` fijo.
2. `BuscarProductoPOS` (nueva) -- port directo, usa `BarcodeInterpreter` ya existente.
3. Vista `POS.cshtml` completa: topbar, selector de sector (modal, bloquea Finalizar sin sector), carrito, buscador+teclado+balanza, modal "Línea del expendio" (reusa `pos-cart.js`).
4. Comentario (adaptar `_ModalComentarioVenta.cshtml`, mismos ids) + Ayuda F1 (contenido propio) + Duplicar POS.
5. Post-guardado básico nuevo (`_ModalPostPuntoExpendioBasico.cshtml`, mismo patrón que `_ModalPostVentaBasico.cshtml` de Ventas batch 6): solo PDF (`ImprimirPdf`, ya portado) y Email (`ObtenerDatosEmailExpendio`/`EnviarComprobanteEmailExpendio`, ya portados). Sin ticket ESC/POS, sin agente de impresión, sin WhatsApp, sin "configurar impresora".

**Explícitamente afuera** (mismo criterio que toda la migración, no se implementa ahora):
- Login de operador de producción (`AutorizarOperadorPOS`/`CerrarOperadorPOS`) -- gap ya abierto.
- `MisExpendiosPOS` + su modal (F6).
- Ticket ESC/POS / agente de impresión local / WhatsApp.
- Scanner de cámara (el original tampoco lo tiene wireado acá).
- `Abrir`/`Guardar` (form clásico sin POS) -- flujo paralelo, no pedido.

## Verificación

Mismo criterio que todo el programa: build limpio, curl contra datos reales (sector real, producto real, expendio real generado y verificado con `sqlcmd`), y prueba visual del usuario en navegador antes de cerrar. Reusar el servidor que ya corro yo mismo en `localhost:5270` (autorizado por el usuario) para las verificaciones HTTP.

## Progreso

**MVP portado y verificado con datos reales (2026-09-04)**:

- `PuntosExpendioController.cs`: agregado `_oBarcodeInterpreter` (faltaba), `POS()` GET (carga sectores reales, sin login de operador) y `BuscarProductoPOS` (port directo de `VentasController.BuscarProducto`, mismo motor `BarcodeInterpreter`).
- `WebCore/Views/PuntosExpendio/POS.cshtml` (nueva), `_ModalComentarioPuntoExpendio.cshtml` (copia casi literal de `_ModalComentarioVenta.cshtml`), `_ModalAyudaPuntoExpendio.cshtml` (contenido propio, F6/F9 quedan visibles pero degradan solos -- mismo criterio que Ventas), `_ModalPostPuntoExpendioBasico.cshtml` (nueva, solo PDF + email).
- `WebCore/wwwroot/Scripts/app/punto-expendio-pos.js` (nuevo, recorte de 974→~500 líneas): sin Mis Expendios/F6, sin login de operador, sin buscar cliente real. **Hallazgo corrigiendo al propio agente de exploración**: el reporte inicial decía que el scanner de cámara no estaba wireado en el original -- falso, solo miraba el `@section Scripts` de la vista y no el include global de `scanner.js`/`zxing.min.js` en `_LayoutPOS.cshtml` (confirmado leyendo el archivo directamente). Se porta el scanner igual que en Ventas.
- `WebCore/wwwroot/Content/css/pos-expendio.css` (nuevo, ~700 líneas, port literal del `<style>` propio de la vista original) + link agregado en `_LayoutPOS.cshtml` (compartido, no pisa nada de Ventas).
- **Excluido deliberadamente del MVP, sin portar**: "compactado adaptativo" por altura de viewport (cosmético, el CSS queda portado pero inerte sin el JS que lo dispara), dark mode (WebCore no tiene ese toggle todavía, mismo caso).
- **Verificado con curl + sqlcmd contra datos reales**: `/PuntosExpendio/POS` sin sector devuelve los 3 sectores reales de la empresa (Carniceria/Presupuesto/Ramos Generales) y `Finalizar` deshabilitado; con `?sector=Carniceria` lo habilita y setea "Sector activo". `BuscarProductoPOS?codigo=1` devuelve el producto real (CARRE, $12000/kg). `FinalizarPOS` con un payload real generó el expendio **#96** de verdad (`ok:true`), y `ImprimirPdf/96` devolvió un PDF válido real (1 página) generado con esos datos. Build limpio (0 errores), `node -c` OK.
- **Pendiente**: prueba visual del usuario en navegador (no se puede automatizar sin herramienta de browser en esta sesión, mismo criterio que el resto del programa).
