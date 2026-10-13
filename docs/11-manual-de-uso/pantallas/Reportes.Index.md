---
pantalla: Reportes.Index
titulo: Reportes
rol: admin
modulo: Reportes
orden: 1
permiso: Stock.VerStock (sin el permiso se muestra "Acceso denegado")
revisada: 2026-10-13
---
## Para qué sirve
Consultar **stock, ventas y balance** de una, varias o todas las sucursales. Se elige el **tipo de reporte**, las **fechas** y las **sucursales**, y se toca **Buscar**.

## Tipos de reporte
- **Stock Actual**: el stock de cada producto desde el último cierre de stock hasta ahora.
- **Cierre Stock**: lo ocurrido entre dos cierres de stock y el stock al cierre.
- **Stock Retroactivo**: el stock desde el último cierre hasta la fecha que elijas.
- **Proyección Ventas vs Stock**: stock actual contra lo vendido en uno o más períodos, con una columna por sucursal.
- **Ventas por Producto**: kilos e importe vendidos por producto, comparando períodos, con gráfico por producto.
- **Balance Económico**: ventas, compras, gastos, cobros y pagos de uno o más períodos.

## Elegir sucursales
1. En **Sucursales** abrí la lista y **tildá las que quieras**: una, varias o todas. **Todas** tilda o destilda el resto; siempre tiene que quedar al menos una tildada. La lista no se cierra al tildar y el botón muestra el resumen (*Todas*, el nombre o *N sucursales*).
2. Al cambiar la selección aparece el aviso de que hay filtros sin aplicar: tocá **Buscar**.
3. Al abrir la pantalla viene tildada **tu sucursal** (en *Ventas por Producto*, todas).

### Qué pasa con varias sucursales
- **Ventas por Producto** y **Balance Económico**: se **suman** las sucursales tildadas. El gráfico de un producto también suma.
- **Proyección Ventas vs Stock**: una columna por sucursal tildada, más el total. El stock de cada sucursal parte de **su** último cierre.
- **Stock Actual, Cierre Stock y Stock Retroactivo**: cada sucursal se calcula con **sus propios cierres**, porque el stock inicial de cada una es el de su cierre:
  - *Cierre Stock*: del anteúltimo al último cierre de cada sucursal.
  - *Stock Actual*: del último cierre de cada una hasta ahora.
  - *Stock Retroactivo*: del último cierre de cada una hasta la fecha que elijas.
  - Las fechas **dejan de elegirse** (se lee *Cierre de cada sucursal*) y la tabla muestra una fila por producto y sucursal; bajo el código figura la sucursal y su rango de fechas. Los productos se agrupan con color alternado y una línea de corte al cambiar de producto, también al ordenar o filtrar.
  - Una sucursal **sin cierres de stock** no aparece y un aviso lo dice.
  - Con **una sola** sucursal tildada las fechas se eligen a mano, como siempre.

## Filtros en vivo
Producto, tipo, marca y estado de stock se aplican sobre lo ya cargado, sin volver a consultar. Los totales se recalculan con las filas visibles.

## Notas
- Con varias sucursales hay una consulta por cada una: puede tardar más que con una sola.
- Los enlaces anteriores con una sola sucursal (`?idSucursal=N`) siguen funcionando; `idSucursal=0` equivale a **Todas** (en los de stock ahora muestra todas, antes se reemplazaba por tu sucursal).
- PENDIENTE: documentar con capturas la tabla de cada tipo de reporte, el detalle de stock y los períodos comparativos.
