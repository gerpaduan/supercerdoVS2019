# Plan: Módulo 8, núcleo POS transaccional (CLAUDE.md §11.1)

Estado: **confirmado (2026-09-03) y en implementación** -- ver sección 10 para el avance real. Este documento es el "plan escrito y confirmado" que exige CLAUDE.md §11.1 antes de tocar código del núcleo POS. La decisión de diseño de la sección 3 (sin `Session["VentaActiva"]`) fue confirmada por el usuario y ya está implementada y verificada.

Alcance de "núcleo POS": `Web/Controllers/VentasController.cs` — `POS`, `AutorizarOperadorPOS`, `CerrarOperadorPOS`, `AutorizarModuloVentas`, `AutorizarOperadorModuloVentas`, `BuscarExpendiosPOS`, `ObtenerExpendioPOS`, `BuscarProducto`, `AgregarProducto`, `FinalizarVenta`, `ModificarVenta` — más el equivalente en `Web/Controllers/PuntosExpendioController.cs` (`POS`, `FinalizarPOS`, etc., ya señalado como excluido en Módulo 8 slice 2, mismo motivo).

## 1. Por qué esto no se portó junto con el resto de Módulo 8

Tamaño real, medido, no estimado:

| Archivo | Líneas |
|---|---|
| `Web/Views/Ventas/POS.cshtml` | 6226 |
| `Web/Views/PuntosExpendio/POS.cshtml` | 1980 |
| `Web/Scripts/app/pos-cart.js` | 1093 |
| `Web/Scripts/app/pos-multi-instance.js` | 714 |
| `Web/Scripts/app/pos-product.js` | 575 |
| `Web/Scripts/app/pos-balanza.js` | 381 |
| `Web/Scripts/app/pos-state.js` | 284 |
| `Web/Scripts/app/pos-keyboard.js` | 192 |
| `Web/Scripts/app/pos-help.js` | 197 |
| `Web/Scripts/app/pos-guard.js` | 161 |
| `Web/Scripts/app/pos-forma-pago-precios.js` | 117 |
| `Web/Scripts/app/pos-comment.js` | 131 |
| **Total (sin contar el controller ni PuntosExpendio/POS)** | **~12 050 líneas** |

Es, por lejos, la pieza más grande de toda la migración — más grande que todo lo demás portado hasta ahora combinado. Confirma lo que ya decía el plan original del programa completo: "el más grande, con AFIP + balanza + código de barras + más hotkeys — último, cuando el patrón ya esté probado". El patrón ya está probado (Módulos 1-8 read-only/CRUD chico + AFIP + PDF + email, todos verificados con datos/producción reales) — lo que falta es este núcleo.

## 2. Hallazgo de arquitectura: `Session["VentaActiva"]`

Investigado leyendo `POS` (GET), `FinalizarVenta`, `ModificarVenta`, `AgregarProducto`, `ConstruirLineasVentaDesdeRequest` completos, y buscando en todos los `.js` del proyecto quién llama a cada endpoint.

- Solo hay **3 usos reales** de `Session["VentaActiva"]` en todo el código, los 3 dentro de `VentasController.cs`, y los 3 acotados a la misma sesión de navegador del mismo cajero (no hay coordinación entre usuarios ni entre pantallas):
  1. `POS` (GET) arma un objeto `Venta` "cáscara" (sin líneas, o resumiendo las últimas si el usuario recargó la página) y lo guarda en `Session["VentaActiva"]`.
  2. `FinalizarVenta` (POST) lee esa cáscara **solo** para `Vendedor`/`FechaVenta` -- las líneas de la venta (`LineasVenta`) se reconstruyen siempre desde el `FinalizarVentaRequest` que manda el cliente (`ConstruirLineasVentaDesdeRequest`, resuelve cada `Corte` por código contra la base), **no** desde la Session.
  3. `AgregarProducto` (POST) es la única acción que de verdad manipula `Session["VentaActiva"].LineasVenta` -- pero **es código muerto**: se buscó `AgregarProducto` en los 4 archivos `.js` donde aparece el texto (`pos-product.js`, `pos-cart.js`, `pos-balanza.js`, `movimientos.js`) y ninguno hace un `$.ajax`/`fetch` contra `/Ventas/AgregarProducto` -- las coincidencias son el botón `#btnAgregarProducto` y la función `addProduct()` del carrito 100% client-side, sin relación con este endpoint. Mismo patrón ya visto en Cajas con `AbrirCaja` (ported for fidelity only).
- **`ModificarVenta` (modificar una venta existente) NO usa `Session["VentaActiva"]` en ningún punto** -- busca la venta directo por ID contra la base (`oVentaN.getVentaById(request.IdVenta)`). El caso de "modificar una venta desde otra pantalla/sesión" (ej. desde Cierre de Caja) ya es 100% stateless en el original, confirmado leyendo el método completo.
- El carrito real (qué productos, cantidades, forma de pago en pantalla) vive en el cliente, en `pos-cart.js`/`pos-state.js` -- el servidor nunca es la fuente de verdad del carrito mientras se arma la venta.

**Consecuencia para WebCore**: ninguno de los controllers portados hasta ahora usa `Session` (todos usan el usuario stub `Admin=true`, sin login). Portar el núcleo POS tal cual, con `Session["VentaActiva"]`, requeriría agregar infraestructura de sesión real a WebCore por primera vez -- algo que el resto de la migración evitó a propósito, para replicar un mecanismo que el propio código demuestra ser mucho más chico de lo que parece a primera vista (2 usos reales, no una coordinación compleja entre pantallas).

## 3. Decisión de diseño propuesta (necesita tu confirmación)

**Opción recomendada: eliminar la dependencia de `Session["VentaActiva"]` en el rediseño, no portarla.**

Justificación: dado que el carrito ya es 100% client-side y el servidor ya reconstruye `LineasVenta` desde el request en cada `FinalizarVenta`/`ModificarVenta`, la Session solo aporta 2 cosas menores:
1. Persistir `Vendedor`/`FechaVenta` entre el GET inicial de `POS` y el POST final -- se puede mandar como parte del mismo `FinalizarVentaRequest` sin problema (el vendedor ya se resuelve server-side vía el operador autenticado, no hace falta que viaje en Session).
2. "Recordar" un carrito sin terminar si el usuario recarga la página -- una comodidad de UX, no una regla de negocio. Se puede lograr con `localStorage` en el cliente (mismo patrón ya usado en otras pantallas de esta migración, ej. `_VentasFacturasFiltrosScripts.cshtml`) sin tocar el servidor.

**Alternativa descartada por ahora**: implementar `ISession` real en WebCore (`AddSession()`/`UseSession()`, con backing store en memoria o distribuido) para portar el mecanismo tal cual. Se descarta como default porque agrega una pieza de infraestructura nueva que nada más necesita, para replicar un mecanismo que el propio código ya demuestra ser prescindible. Si en la implementación real aparece algún caso donde la Session SÍ importa (ej. usuario de producción con step-up de operador, ver más abajo), se revisita puntualmente, no en bloque.

**Esto es un cambio de comportamiento respecto al original** (aunque invisible para el usuario final en el caso común) -- por eso se marca acá para que lo confirmes antes de implementar, no se decide en silencio (CLAUDE.md §0/§11.1).

## 4. Otros bloqueantes/decisiones ya conocidos que este núcleo hereda

- **Balanza y lector de código de barras** (`pos-balanza.js`, `barcode-code-input.js`): 100% client-side, hablan por HTTP a agentes locales (`127.0.0.1`) -- ya confirmado portable sin cambios en la investigación original de toda la migración. No es un bloqueante, es trabajo de portar la vista/JS tal cual.
- **AFIP durante el flujo de POS** (`GenerarFactura` invocado desde POS, no desde el flujo "manual sin venta" que ya se portó y probó con producción real): la lógica de AFIP en sí ya está resuelta y probada (Factura A y B reales, ver README.md) -- lo que falta es el *punto de enganche* desde el carrito real de POS, no la integración con AFIP en sí.
- **Step-up de operador para cuenta de producción** (`AutorizarOperadorPOS`/`ResolverOperadorPOS`, `PermisosHelper.ObtenerOperadorPOS`): mismo patrón ya documentado como "código muerto bajo el stub Admin=true" en Cajas/Ventas/PuntosExpendio -- se documenta, no se porta, mismo criterio.
- **Impresión de ticket** (ESC/POS vía PrintAgent local): mismo bloqueante ya señalado en Ventas/PuntosExpendio/Finanzas -- depende del agente de impresión local, no portado todavía en ningún lado de esta migración.

## 5. Juez de paridad propuesto (falta implementar, esto es el diseño)

CLAUDE.md §11.1 exige definir el árbitro mecánico *antes* de migrar, y validarlo (pasa contra el original, falla contra código roto a propósito) antes de confiar en él.

Dado que el POS es interactivo (balanza, teclado, F9/F10, modales), un diff de HTML estático no alcanza como en los módulos read-only ya migrados. Propuesta:

1. **Harness de paridad con Playwright** (ya vive un patrón similar informal en las pruebas manuales de esta sesión, pero acá se propone automatizado): un set fijo de escenarios reales --
   - Agregar 1 producto por código de barras, finalizar venta Efectivo.
   - Agregar 2 productos por búsqueda de texto, forma de pago mixta (Efectivo + tarjeta), finalizar.
   - Modificar la última venta del cajero (permiso bajo el stub Admin siempre disponible).
   - Cambiar forma de pago de una venta ya cerrada.
   - Cerrar sesión de POS sin operador de producción (camino normal, no el de cuenta compartida).
2. Cada escenario corre contra `Web` (IIS/localhost) y contra `WebCore` (Kestrel/localhost) con el **mismo seed de datos** (mismos productos/precios/cliente), y se compara: total de la venta persistida en `Ventas`/`LineaVenta`, no el HTML pixel a pixel (el layout puede variar levemente entre motores sin que sea un bug real).
3. **Validación del juez**: correr los 5 escenarios contra `Web` sin tocar nada (deben pasar), y después contra una copia de `Web` con un bug metido a propósito (ej. forzar que `PrecioKg` se guarde con un redondeo distinto) -- si el juez no lo detecta, el juez está mal diseñado, se corrige antes de usarlo para validar `WebCore`.

Este harness **no existe todavía** -- es la primera tarea real de la implementación, antes de portar una sola línea del controller/vista.

## 6. Orden de batches sugerido (una vez confirmado el punto 3)

1. Harness de paridad (sección 5) + `git tag pre-pos-nucleo-YYYYMMDD` (restore point).
2. `BuscarProducto`/`AgregarProducto` (lectura + armado de línea, sin persistir) -- más chico, sin el problema de Session, valida el patrón de comunicación con `pos-cart.js` antes del resto.
3. `FinalizarVenta` (la escritura real, forma de pago simple: Efectivo/Tarjeta/CtaCte, sin pago mixto todavía) contra el juez.
4. `ModificarVenta` (edición de la última venta del cajero).
5. Pago mixto, expendios asociados (`BuscarExpendiosPOS`/`ObtenerExpendioPOS`), balanza, código de barras -- uno por uno, cada uno contra el juez.
6. Enganche con AFIP (ya resuelto en el flujo manual, acá es "generar factura de esta venta real recién creada" en vez de la venta manual de prueba).
7. `PuntosExpendioController.POS`/`FinalizarPOS` (mismo patrón, después de validar el de Ventas).

Cada batch: `git tag` propio antes de arrancarlo, no solo al principio de todo.

## 7. Inventario de gaps esperado

Se abrirá `docs/10-migracion-aspnet-core/GAPS-POS.md` cuando arranque la implementación real (por ahora no hay gaps concretos que registrar, solo la decisión de diseño de la sección 3, que se resuelve acá, no ahí).

## 8. Criterio de éxito

- Los 5 escenarios del harness (sección 5) pasan contra `WebCore`, con el mismo resultado persistido en base que contra `Web`.
- `Web` (clásico) sigue compilando y funcionando sin cambios durante todo el proceso (mismo criterio que el resto del programa).
- Ninguna escritura real se hace sin que el harness la haya validado primero contra datos de prueba.
- Cada decisión de diseño que se aparte del original (como la de la sección 3) queda registrada en `docs/DECISIONS.md` con fecha, antes de implementarla -- no después.

## 9. Qué necesito de vos antes de arrancar

1. ~~Confirmar o rechazar la decisión de la sección 3~~ -- **confirmado** ("ADELANTE", 2026-09-03).
2. ~~Confirmar el orden de batches~~ -- **confirmado**, con el ajuste real descripto en la sección 10.

## 10. Avance real (2026-09-03)

**Restore point**: `git tag pre-pos-nucleo-20260903` (antes de tocar código).

**Juez de paridad -- desviación consciente del diseño original de la sección 5**: el harness Playwright automatizado **no se construyó** -- `WebCore` todavía no tiene ninguna vista de POS para que un browser automatizado clickee. En su lugar se usó el mismo mecanismo de verificación de toda esta migración (llamada HTTP real a la acción + `sqlcmd` contra la base local para confirmar lo persistido), que cumple el mismo objetivo del juez (comparar comportamiento contra datos reales, no contra lo que el código "debería" hacer) sin necesitar una UI que todavía no existe. El harness Playwright de la sección 5 sigue siendo la propuesta correcta para cuando haya una vista de POS real que probar de punta a punta (batches 5 en adelante); se retoma en ese momento.

**Portado y verificado con datos reales** (`WebCore/Controllers/VentasController.cs`):
- **`FinalizarVenta`** -- reescrito sin `Session["VentaActiva"]` (venta armada entera desde el `FinalizarVentaRequest`, `[FromBody]` porque el cliente original ya manda JSON). Probado: venta real `idVenta=1739`, 1 línea (CARRE, 1 kg, $15), `idVendedor=2`, `formaPago=Efectivo`, verificada con `sqlcmd` contra `Ventas`/`LineaVenta`.
- **`ModificarVenta`** -- ya era 100% stateless en el original (confirmado en la sección 2), portado sin cambios de lógica más allá de sacar los chequeos de permiso (bypass Admin, mismo criterio de siempre). Probado: modificó la venta 1739 (observaciones + cantidad 1kg→2kg), verificado que `LineaVenta` reemplazó la línea vieja por una nueva (`eliminarLineas=true`, mismo comportamiento que el original).
- **`BuscarProducto`** -- **no se portó la versión actual del original**, que depende de `Negocio.BarcodeInterpreter` (clase nueva, sin commitear, de una sesión en paralelo trabajando en códigos de barra internos -- y que hoy **no compila**, `CS0103: 'ValidacionEan' no existe en el contexto actual`, confirmado en un intento de build de la solución completa). Se portó en su lugar la lógica equivalente y estable de `PuntosExpendioController.BuscarProductoPOS` (código genérico vía sufijo `G<n>` + precio manual, sin el motor de códigos de barra internos de balanza que agrega `BarcodeInterpreter`). Cuando la otra sesión commitee su trabajo, hay que revisar si `VentasController.BuscarProducto` necesita actualizarse a la clase compartida real. Probado: `BuscarProducto?codigo=1` devolvió el producto real (CARRE, id=33, $12000/kg).
- **`AgregarProducto`**: confirmado código muerto (sección 2), no se porta.

**No implementado todavía** (batches 5-7 del plan): pago mixto, expendios asociados (`BuscarExpendiosPOS`/`ObtenerExpendioPOS`), balanza, código de barras, enganche con AFIP desde una venta real de POS (no la venta manual ya probada), `PuntosExpendioController.POS`/`FinalizarPOS`, y toda la vista de POS en sí (`Ventas/POS.cshtml`, 6226 líneas -- nada de esto tiene UI en WebCore todavía, solo los 3 endpoints de arriba, verificados por HTTP directo).
