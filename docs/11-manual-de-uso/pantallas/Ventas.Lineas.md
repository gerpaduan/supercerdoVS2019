---
pantalla: Ventas.Lineas
titulo: Líneas de venta
rol: usuario
modulo: Ventas
orden: 1
permiso: PENDIENTE (la acción no valida un permiso propio; a los usuarios de producción les pide elegir el operador de Ventas antes de entrar)
revisada: 2026-10-13
---
## Para qué sirve
Ver **cada venta con sus líneas** (producto, cantidad en kilos, precio y total de cada línea) de un período, con una tarjeta por venta. Sirve para revisar qué se vendió producto por producto y para **exportarlo a PDF o Excel**.

## Filtros
- **Sucursal**: al cambiarla, la pantalla se recarga sola.
- **Fecha desde / Fecha hasta**: al cambiarlas aparece el aviso de que hay filtros sin aplicar; tocá **Buscar** para recargar los datos.
- **Producto** (código o descripción), **Cliente**, **Vendedor** y **Forma de pago** (Efectivo, Debito, Credito, CtaCte, Qr, Transferencia): se aplican en el momento sobre lo ya cargado, sin recargar, y los totales de arriba se recalculan.
- **Ver detalles**: muestra en cada venta la fecha, el nro/comprobante, la sucursal, el cliente, el vendedor y la forma de pago.

## Totales
Cuatro tarjetas: **Total Kg**, **Total pesos** (suma de las líneas que se ven), **Cantidad de líneas** y **Cantidad de registros** (ventas).

## Exportar a PDF o Excel
1. Dejá la pantalla como la querés: sucursal, fechas (ya aplicadas con **Buscar**) y filtros.
2. Tocá **PDF** o **Excel**, arriba a la derecha, junto a *Volver al indice*.
3. Se descarga el archivo `Lineas_de_venta_AAAAMMDD_HHMM.pdf` o `.xlsx`. La pantalla no se recarga.

### Qué trae el archivo
- Un **corte por venta**: una banda con la fecha, el *Venta ID* y el **total de la venta**; una línea gris con **cliente, vendedor, forma de pago, sucursal y nro/comprobante**; y debajo sus líneas (código, producto, cantidad/kg, precio y total de la línea).
- Arriba, el **período**, la sucursal y los **filtros activos**. Al final, los **mismos totales** que las tarjetas de la pantalla (suma de las líneas incluidas).
- **PDF**: si una venta sigue en otra hoja, se repite su encabezado en la hoja siguiente.
- **Excel**: las cantidades y los importes son **números** (se pueden sumar y ordenar), no texto.

### Reglas
- El archivo respeta **lo que ves**: usa las fechas **ya aplicadas** (no las que cambiaste sin tocar *Buscar*) y los filtros de Producto, Cliente, Vendedor y Forma de pago.
- Con un filtro de producto, el total de cada venta sigue siendo el de **toda la venta** aunque se listen solo algunas líneas; el total general suma solo las líneas listadas.
- Si no hay ninguna venta visible, aparece el aviso *No hay ventas para exportar* y no se descarga nada.
- Las ventas **reservadas** que están ocultas para tu usuario tampoco salen en el archivo.
- No hay tope de filas: un período muy largo puede tardar en generarse.
- El filtro de **Cliente** de la pantalla busca por nombre; el archivo busca por nombre **o identificación**, así que un cliente escrito por CUIT/DNI puede traer ventas que la pantalla oculta.

## Notas
- PENDIENTE: qué permiso o rol muestra la entrada de esta pantalla en el menú (no verificado contra el código en esta revisión).
