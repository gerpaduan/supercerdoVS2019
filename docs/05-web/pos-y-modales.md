# POS y modales

## Regla actual

Toda vista que se abra dentro de un modal desde POS debe renderizarse como `PartialView` y sin `LayoutBase`.

## Secciones

- Vista o accion
- Se abre desde POS
- Renderizado esperado
- Dependencias visuales
- Observaciones

## `#modalTrabajoPOS` y `POSTrabajos` (2026-11-18)
Modal del POS para las cargas de Stock, Movimientos y Elaborados (botón **Cargas**). Fondo estático (`data-bs-backdrop="static"`, sin X), botones Minimizar y Descartar, franja `#trabajoPOSAviso` de posible duplicado y un `<iframe>` por carga dentro de `#contenedorTrabajoPOS`. Minimizar oculta el modal y el iframe sigue vivo; `#posCargasEnCurso` en la barra muestra un botón «<carga> en curso» por carga abierta. El teclado del iframe no llega al POS: `pos-trabajos.js` reenvía la actividad a `window.POSBloqueo.registrarActividad()` (bloqueo por inactividad). Detalle en [Ventas y POS](../03-modulos/ventas-y-pos.md).
