---
pantalla: Compras.Lineas
titulo: Líneas de compra
rol: usuario
modulo: Compras
orden: 2
permiso: Compra.VerCompras (sin el permiso se muestra "Acceso denegado")
revisada: 2026-10-13
---
## Para qué sirve
Ver **cada compra con sus líneas** (producto, cantidad, precio y total), con una tarjeta por compra, de un período.

## Filtros
- **Sucursal**: al cambiarla, la pantalla se recarga sola.
- **Fecha desde / Fecha hasta**: al cambiarlas aparece el aviso; tocá **Buscar** para recargar los datos.
- **Producto** (código o descripción) y **Tipo de compra**: se aplican en el momento sobre lo ya cargado, y los totales de arriba se recalculan.
- **Proveedor**: escribiendo buscás por letras; con la **lupa** (o **F9**) elegís de una lista, de a uno o varios, y filtra **exacto**. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Si venís de [Compras](ayuda:pantalla/Compras.Index), ya llega con el proveedor que tenías elegido.
- **Ver detalles**: muestra en cada compra la fecha, el proveedor, la sucursal, el tipo y el número de comprobante.

## Totales
Cuatro tarjetas: **Total Kg**, **Total pesos**, **Cantidad de líneas** y **Cantidad de registros** (compras), calculadas con lo que está visible.

## Notas
- Al tocar **Buscar** (o cambiar la sucursal) se conserva el proveedor elegido de la lista; el texto escrito en **Proveedor**, **Producto** y **Tipo de compra** se vuelve a empezar.
- El campo **Proveedor** arranca con lo que se escribió en **Descripción** de Compras (es un comportamiento anterior, no cambió).
