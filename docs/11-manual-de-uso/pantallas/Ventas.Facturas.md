---
pantalla: Ventas.Facturas
titulo: Facturas
rol: usuario
modulo: Ventas
orden: 2
permiso: Venta.VerVentas (sin el permiso se muestra "Acceso denegado")
revisada: 2026-10-28
---
## Para qué sirve
Ver las **facturas electrónicas emitidas** (con CAE) de un período: factura, nota de crédito, a quién y cuánto. Desde acá también se puede cargar una **nueva factura** sin venta.

## Filtros
- **Desde / Hasta** y **Sucursal**: tocá **Aplicar** para recargar; al cambiar la sucursal se recarga sola.
- **Vendedor** y **Cliente**: escribiendo buscás por letras (el cliente también por la razón social y el número de documento con que se facturó). Con la **lupa** (o **F9** vendedor, **F10** cliente) elegís de una lista, de a uno o varios, y filtra **exacto** por esas personas. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor).
- **Forma de pago** y **Tipo comprobante** (Factura A/B/C y Notas de crédito A/B/C): se tildan en el desplegable.
- **Entorno** (Producción / Pruebas / Todas): solo aparece si la empresa tiene alguna factura emitida en homologación; por defecto muestra solo Producción.
- Los filtros se aplican **en el servidor**: la lista se actualiza sola un instante después de escribir o tildar.

## Totales y lista
- **Cant.** y **Total** son los de **todo el filtro**, no solo de lo que ya se ve. Las notas de crédito **restan**.
- La lista carga de a 50: al bajar con el scroll se piden las siguientes.

## Ver el detalle de una factura sin salir de la lista
- Tocá una factura: se **despliega** con el **monto facturado**, el **porcentaje de facturación**, el **CAE** y su vencimiento, la factura o nota de crédito asociada, la observación del comprobante y las **líneas de la venta asociada** con su total. Podés dejar varias desplegadas.
- **Abrir detalle** abre el detalle completo de la factura en la misma pestaña; con **Volver** la lista queda con las mismas facturas desplegadas y en el mismo lugar (solo las que estaban entre las primeras cargadas; las que se habían cargado al bajar con el scroll vuelven cerradas).
- El panel no tiene acciones: para ver el comprobante completo y operar, abrí el detalle.

## Notas
- PENDIENTE: no se verificó en esta revisión qué permiso muestra la entrada de esta pantalla en el menú ni el detalle del botón **Nueva factura**.
