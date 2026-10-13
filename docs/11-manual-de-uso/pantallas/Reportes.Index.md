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

## Leer Stock Cierre y Faltante (reporte Cierre Stock)
- **Stock Cierre** (columna con banda azul): el stock registrado en el cierre de stock de la fecha *hasta*.
- **Faltante** = (stock inicial + ingresos − egresos) − Stock Cierre. Se muestra siempre con signo y con color, icono y texto (el color nunca va solo):
  - **Positivo, rojo, ↑ "Faltó mercadería"**: había que tener más de lo que se registró al cierre.
  - **Negativo, ámbar, ↓ "Vendido de más"**: se registró más stock al cierre del que correspondía.
  - **0, verde tenue, ✓ "Coincide"**: el cierre coincide con lo calculado.
- La tabla va compacta (celdas con poco relleno, columnas angostas) y el valor del *Faltante* tiene el mismo tamaño de letra que el resto de las celdas, distinguido solo por la negrita. Arriba de la tabla hay una **leyenda** con los tres casos. Al tocar el encabezado *Faltante* la tabla se ordena por ese valor: un toque de menor a mayor (primero los "vendido de más"), otro toque lo invierte (primero los que más faltaron).
- **Estado**: en *Cierre Stock* compara el **Stock Cierre** (el de la misma fila) con el *Punto stock*: *NEGATIVO* si es negativo; *BAJO* si hay punto de stock y el Stock Cierre es menor o igual; *SIN STOCK* si es 0; *OK* en el resto. En *Stock Actual* y *Retroactivo* compara el stock disponible, como siempre. El filtro en vivo *Estado stock* usa esa misma regla.
- **Totales** (switch *Mostrar totales*): en *Cierre Stock* hay dos tarjetas, **Faltó mercadería** (suma de los Faltantes positivos) y **Vendido de más** (suma de los negativos, en positivo). No se muestra una sola cifra porque lo que faltó y lo vendido de más se compensarían. Ambas se recalculan con las filas visibles.
- El **detalle de producto** (doble clic en la fila) usa el mismo código de colores en *Stock Cierre* y *Faltante*. En *Stock Actual* y *Stock Retroactivo* no hay colores: allí el mismo dato es el stock disponible, no un faltante.
- En ese detalle, *Solo ingresos* y *Egresos* quedan enfrentados renglón a renglón: **Compra | Ventas**, **Elaborado | Elaborado**, **Stock | Stock**, **Movimiento | Movimiento** y **Ajustes** solo del lado de ingresos (del otro hay un renglón vacío), así *Tot. ingresos* y *Tot. egresos* quedan a la misma altura. En el celular las tarjetas van una debajo de la otra y el renglón vacío no se muestra.

## Abrir en una ventana independiente
En la fila de **Mostrar totales**, a la derecha, está el botón **Abrir en ventana**: abre Reportes en una ventana aparte (≈1100×800, se puede mover y redimensionar) **sin menú lateral ni barra superior**, para dejarlo en otro monitor o tener más lugar para las tablas.
- La ventana se abre con **los mismos filtros** que tenés en pantalla (tipo de reporte, fechas o cierres, sucursales, filtros avanzados y períodos comparativos). Si ya habías tocado **Buscar**, el reporte se carga solo; si no, queda listo para buscar.
- Funciona igual que la pantalla normal (Buscar, ordenar, filtros en vivo, detalle de producto, gráficos) y **sigue siendo ventana independiente al buscar**.
- Es la misma sesión (si vence, la ventana pide ingresar de nuevo). Si volvés a tocar el botón con la ventana abierta, **la recarga con los filtros de ese momento** y la trae al frente, sin abrir otra. Si el navegador bloquea la ventana emergente, avisa y hay que permitirla para el sitio. En la ventana no aparece el botón.

## Filtros en vivo
Producto, tipo, marca y estado de stock se aplican sobre lo ya cargado, sin volver a consultar. Los totales se recalculan con las filas visibles.

## Notas
- Con varias sucursales hay una consulta por cada una: puede tardar más que con una sola.
- Los enlaces anteriores con una sola sucursal (`?idSucursal=N`) siguen funcionando; `idSucursal=0` equivale a **Todas** (en los de stock ahora muestra todas, antes se reemplazaba por tu sucursal).
- PENDIENTE: documentar con capturas la tabla de cada tipo de reporte, el detalle de stock y los períodos comparativos.

## Exportar a Excel (reportes de stock)
En la fila de **Mostrar totales**, a la derecha, aparece el botón verde **Exportar a Excel** cuando *Stock Actual*, *Cierre Stock* o *Stock Retroactivo* ya muestran resultados. Descarga un `.xlsx` (nombre `<Reporte>_<fecha>_<hora>.xlsx`) con **los filtros que hay en pantalla** (sucursales, fechas o cierres, producto, tipo, marca y estado), una fila por producto (y sucursal, con 2+ sucursales).
- Las columnas son las de la tabla: código, producto, (sucursal), St. inicial, Total ingresos, Total egresos, Stock Cierre y Faltante (Cierre Stock) o Stock actual, Punto stock y Estado. Con el switch **Mostrar detalles** activo se suman las de ingresos y egresos desglosados.
- Cantidades en **números** (se pueden sumar, ordenar y filtrar; la hoja trae filtro automático y encabezado fijo). El *Faltante* lleva signo y color (rojo: faltó mercadería, ámbar: vendido de más, verde: coincide).
- Las filas con *Stock Actual* se calculan **al momento de exportar** (igual que al buscar). Los otros reportes (Ventas por Producto, Proyección, Balance) no tienen exportación.
- Pide el mismo permiso que ver el reporte (Stock > Ver stock). Solo lectura. Código: `ReportesController.ExportarStockExcel`, `Services/ExcelReporteStock.cs`.
