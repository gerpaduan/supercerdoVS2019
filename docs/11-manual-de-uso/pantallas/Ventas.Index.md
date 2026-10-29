---
pantalla: Ventas.Index
titulo: Ventas
rol: usuario
modulo: Ventas
orden: 1
permiso: Venta.VerVentas (sin el permiso se muestra "Acceso denegado"; a los usuarios de producción les pide elegir el operador de Ventas antes de entrar)
revisada: 2026-10-28
---
## Para qué sirve
Ver las **ventas de un período**, agrupadas por día, con la cantidad y el total de lo que se está mostrando. Tocando una venta se **despliega** su detalle sin salir de la lista.

## Ver el detalle de una venta sin salir de la lista
- Tocá una venta (o, con el teclado, Enter o Espacio sobre ella): se despliega debajo con los **artículos** (cantidad × precio, subtotal, bonificación o recargo), el **total**, el comentario completo y las marcas de **Facturada** y **nota de crédito**. Volvé a tocarla para cerrarla. Podés tener **varias desplegadas a la vez** para compararlas.
- Dentro del panel hay **Modificar venta** y **Cambiar forma de pago** (solo si te corresponden, con las mismas reglas que en el [detalle completo](ayuda:pantalla/Ventas.DetalleVenta)) y **Abrir detalle**, que abre la venta completa en la misma pestaña (con Ctrl+clic se abre en otra). **Facturar, imprimir o mandar por email** se hace solo desde el detalle completo; el panel lo recuerda con un aviso.
- Al volver del detalle con la flecha **Volver**, la lista queda como estaba: las mismas ventas desplegadas y en la misma posición. Esto se recuerda en el navegador por pestaña y por período; si el navegador no lo permite, la lista vuelve simplemente al principio. Los filtros de Vendedor, Cliente, Forma de pago y Tipo comprobante ya tienen su propio recuerdo (ver arriba).
- Si no se puede cargar el detalle (venta inexistente, sesión vencida, cuenta corriente reservada) el panel muestra el motivo; al cerrarlo y abrirlo de nuevo se reintenta.

## Filtros
- **Desde / Hasta**: al cambiarlos aparece el aviso de que hay filtros sin aplicar; tocá **Buscar** para recargar.
- **Sucursal**: al cambiarla, la pantalla se recarga sola.
- **Vendedor** y **Cliente**: escribiendo buscás por letras; con la **lupa** (o **F9** vendedor, **F10** cliente) elegís de una lista, de a uno o varios, y filtra **exacto** por esas personas. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Cliente busca por razón social o identificación.
- **Forma de pago** y **Tipo comprobante**: se tildan en el desplegable.
- Vendedor, Cliente, Forma de pago y Tipo comprobante se aplican **en el momento** sobre lo ya cargado y recalculan **Cant.** y **Total**. Los filtros quedan recordados en la sesión del navegador (al volver a la pantalla siguen puestos).

## Botones de arriba
- **Facturas**: abre el listado de [facturas](ayuda:pantalla/Ventas.Facturas) del mismo período y sucursal (no lleva Vendedor ni Cliente).
- **Ver líneas**: abre las [líneas de venta](ayuda:pantalla/Ventas.Lineas) con los mismos filtros (fechas, sucursal, vendedor, cliente y formas de pago). Si elegiste personas de la lista viajan **esas** personas; si escribiste, viaja el texto.

## Notas
- PENDIENTE: no se verificó en esta revisión qué permiso muestra la entrada de esta pantalla en el menú.
