---
pantalla: Compras.Lineas
titulo: Líneas de compra
rol: usuario
modulo: Compras
orden: 2
permiso: Compra.VerCompras (sin el permiso se muestra "Acceso denegado")
revisada: 2026-10-14
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

## Ver solo líneas
El botón **Ver solo líneas** (debajo de los filtros) cambia la pantalla de una tarjeta por compra a **una sola tabla con una fila por línea**; al tocarlo de nuevo (*Ver agrupado por compra*) volvés a las tarjetas.
- Cada fila trae: **Fecha**, **Creado** (cuándo se cargó el registro, con segundos), proveedor, sucursal, tipo de compra, nro/comprobante, código, producto, cantidad/kg, precio y total de la línea, y un ojo para **abrir la compra**.
- **Ordenar**: tocá el título de una columna (Fecha, Creado, Código, Producto, Proveedor, etc.); un segundo toque invierte el orden. La flecha ▲▼ marca la columna activa. Al abrir la vista viene ordenada por **Fecha**, de la más reciente a la más vieja. Las líneas sin código (*-*, p. ej. media res sin tropa) quedan siempre al final.
- Los **filtros en vivo y los totales** de arriba funcionan igual en las dos vistas. *Ver detalles* se oculta en la vista plana (ya se ven todos los datos).
- El navegador **recuerda la vista elegida** (para esta pantalla) al tocar *Buscar* o cambiar la sucursal.
- Con muchas columnas, en pantallas angostas la tabla se desplaza hacia los costados dentro de su recuadro.

## Notas
- Al tocar **Buscar** (o cambiar la sucursal) se conserva el proveedor elegido de la lista; el texto escrito en **Proveedor**, **Producto** y **Tipo de compra** se vuelve a empezar.
- El campo **Proveedor** arranca con lo que se escribió en **Descripción** de Compras (es un comportamiento anterior, no cambió).
