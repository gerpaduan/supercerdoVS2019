---
pantalla: Ventas.DetalleVenta
titulo: Detalle de venta
rol: usuario
modulo: Ventas
orden: 4
permiso: a los usuarios de producción les pide elegir el operador de Ventas antes de entrar (no en el modal de Mis ventas); las acciones Modificar venta y Cambiar forma de pago aparecen solo si corresponden (ver abajo). PENDIENTE: no se verificó el permiso de menú de esta pantalla.
revisada: 2026-10-28
---
## Para qué sirve
Ver **una venta completa**: a quién, cuándo, con qué forma de pago, cada artículo y el total, y desde ahí **facturar o imprimir**, **modificarla** o **cambiarle la forma de pago**. También se ve dentro de otras pantallas: **Mis ventas** (modal del POS y de Cajas Abiertas) y, debajo de la factura, en el [detalle de la factura](ayuda:pantalla/Ventas.Facturas).

## Cómo se llega
- Desde [Ventas](ayuda:pantalla/Ventas.Index): tocá la venta para **desplegarla** y después **Abrir detalle**. **Volver** (la flecha de arriba) te devuelve al listado con las mismas ventas desplegadas y en el mismo lugar.
- Desde **Mis ventas**: tocá la venta y **Abrir detalle**; se abre en el mismo modal.

## Qué muestra
- **Encabezado**: flecha de volver, **Venta #número** y la fecha y hora.
- **Cliente**, **Comprobante** y **Forma de pago** (si se cobró combinando efectivo con otro medio aparece, por ejemplo, «Debito | Efectivo»).
- **Factura asociada: sí** y el link a la **nota de crédito**, si la venta las tiene.
- **Comentario** de la venta, si lo tiene.
- **Artículos**: cada uno con número, nombre y código, **cantidad × precio** y el **subtotal** a la derecha. Si tiene bonificación o recargo aparece como una marca (**Bonif. 27,07%** / **Recargo x%**) junto a la cuenta. Abajo, el **Total**.

## Acciones (abajo, dentro de la tarjeta)
- **Factura / Imprimir**: abre el comprobante para imprimir el ticket, mandarlo por email o facturar.
- **Modificar venta**: solo aparece si corresponde. Abre la venta en el POS para editarla.
- **Cambiar forma de pago**: solo aparece si corresponde. Abre el POS en modo «solo forma de pago».
- Con un usuario común, **Modificar venta** y **Cambiar forma de pago** se ofrecen para ventas de **su caja abierta actual** (misma sucursal y dentro del horario de esa caja); Modificar además pide el permiso de última venta. Quien tiene permiso administrativo sobre la venta las ve siempre. PENDIENTE: no se verificó en esta revisión cuál es el permiso administrativo exacto.
- Con la cuenta compartida de **producción**, desde Mis ventas del POS, se evalúan con el empleado autorizado en esa pestaña.

## En el celular y en pantallas chicas
Los datos del cliente, el comprobante y la forma de pago se ordenan en una cuadrícula, cada artículo ocupa dos renglones con el subtotal siempre a la derecha, y los botones de acción quedan **uno debajo del otro a todo el ancho** (en pantallas grandes, en fila).

## Notas
- Esta pantalla no se despliega en el listado: en las tarjetas de [Ventas](ayuda:pantalla/Ventas.Index) solo se ve una versión de lectura. **Factura / Imprimir** se hace únicamente desde acá.
