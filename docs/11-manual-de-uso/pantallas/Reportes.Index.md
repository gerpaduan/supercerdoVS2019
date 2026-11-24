---
pantalla: Reportes.Index
titulo: Reportes
rol: admin
modulo: Reportes
orden: 1
permiso: un permiso por tipo de reporte (Reporte.StockActual, CierreStock, StockRetroactivo, ProyeccionVentasVsStock, VentasPorProducto, BalanceEconomico; sin ninguno se muestra "Acceso denegado")
revisada: 2026-11-24
---
## Para qué sirve
Consultar **stock, ventas y balance** de una, varias o todas las sucursales. Se elige el **tipo de reporte**, las **fechas** y las **sucursales**, y se toca **Buscar**.

## Tipos de reporte
El selector muestra **solo los tipos que tu usuario tiene permitidos** (cada uno es un permiso aparte en *Usuarios → Permisos*, módulo Reportes; el permiso **Stock** ya no los habilita). *Días ver* de ese permiso limita hasta cuántos días atrás podés poner la fecha «desde». Si pedís un tipo que no tenés, se muestra el primero que sí; sin ninguno, «Acceso denegado».

- **Stock Actual**: el stock de cada producto desde el último cierre de stock hasta ahora.
- **Cierre Stock**: lo ocurrido entre dos cierres de stock y el stock al cierre.
- **Stock Retroactivo**: el stock desde el último cierre hasta la fecha que elijas.
- **Proyección Ventas vs Stock**: stock actual contra lo vendido en uno o más períodos, con una columna por sucursal.
- **Ventas por Producto**: kilos e importe vendidos por producto, comparando períodos, con gráfico por producto.
- **Balance Económico**: ventas, compras, gastos, cobros y pagos de uno o más períodos.

## Filtros y barra de botones
- En la **computadora** (pantalla de 1200 px o más) todos los filtros van en **una sola fila**: **Tipo de reporte**, **Fecha desde**, **Fecha hasta**, **Sucursales**, el interruptor **Filtros avanzados** y **Buscar**. Los filtros avanzados, la opción de ventas en cuenta corriente del Balance y los *Períodos comparativos* aparecen debajo, a todo el ancho. En pantallas más angostas se ven apilados.
- **Filtros plegados en pantalla angosta** (menos de 1200 px: ventana achicada, notebook con Windows al 125 %, celular): después de tocar **Buscar**, los filtros se **pliegan a una sola línea** para que la tabla tenga más lugar. La línea resume lo buscado (*Cierre Stock · 19/10 07:35 → 26/10 07:30 · San Martín*) y, si los hay, cuántos **filtros avanzados** y **períodos comparativos** están puestos. Tocá la línea o **Filtros** para abrirlos; **Buscar** repite la consulta. Con los filtros abiertos, **Ocultar filtros** los vuelve a plegar; si cambiaste algo sin buscar, la línea muestra *⚠ sin aplicar*. Al entrar a la pantalla (sin búsqueda) se ven abiertos. En la computadora con pantalla ancha no se pliegan: ya van en una fila.
- Debajo de los filtros hay **una barra**: a la izquierda el interruptor **Mostrar totales** y la cantidad de registros visibles; en el medio, el aviso *Cambiaste los filtros: tocá Buscar para actualizar.* (con **Buscar** en ámbar) cuando tocás un filtro y todavía no buscaste; a la derecha **Exportar a Excel** y **Abrir en ventana**.
- **Mostrar totales** despliega los totales como **etiquetas de una línea**, en un renglón debajo de la barra (en el celular siguen siendo tarjetas). Arrancan ocultos.

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
- **Columnas ocultas por defecto:** *St. inicial*, *Total Ingresos* y *Total Egresos* **no se muestran** al abrir el reporte, para dejar lugar a *Stock Cierre* y *Faltante*. El interruptor **Ver inicial, ingresos y egresos** (arriba de la tabla, junto a *Mostrar detalles*) las muestra; la elección se **recuerda mientras no cierres la pestaña** (al tocar **Buscar** sigue como la dejaste; en una pestaña nueva vuelve a empezar oculto). **Exportar a Excel** incluye siempre esas tres columnas, esté prendido o no el interruptor.
- **Stock Cierre** (columna con banda azul): el stock registrado en el cierre de stock de la fecha *hasta*.
- **Faltante** = (stock inicial + ingresos − egresos) − Stock Cierre. Se muestra siempre con signo y con color, icono y texto (el color nunca va solo):
  - **Positivo, rojo, ↑ "Faltó mercadería"**: había que tener más de lo que se registró al cierre.
  - **Negativo, ámbar, ↓ "Vendido de más"**: se registró más stock al cierre del que correspondía.
  - **0, verde tenue, ✓ "Coincide"**: el cierre coincide con lo calculado.
- La tabla va compacta (celdas con poco relleno, columnas angostas) y el valor del *Faltante* tiene el mismo tamaño de letra que el resto de las celdas, distinguido solo por la negrita. La **leyenda** con los tres casos (↑ faltó, ↓ vendido de más, ✓ coincide) está en el **encabezado de la columna Faltante**; al dejar el mouse sobre cada uno se lee su descripción completa. Al tocar el encabezado *Faltante* la tabla se ordena por ese valor: un toque de menor a mayor (primero los "vendido de más"), otro toque lo invierte (primero los que más faltaron).
- **Estado**: en *Cierre Stock* compara el **Stock Cierre** (el de la misma fila) con el *Punto stock*: *NEGATIVO* si es negativo; *BAJO* si hay punto de stock y el Stock Cierre es menor o igual; *SIN STOCK* si es 0; *OK* en el resto. En *Stock Actual* y *Retroactivo* compara el stock disponible, como siempre. El filtro en vivo *Estado stock* usa esa misma regla.
- **Faltante alineado a la izquierda (desde 2026-11-24):** el número, las unidades y la etiqueta arrancan desde el borde izquierdo de la celda.
- **Faltante cuando la columna es angosta (desde 2026-11-24):** por defecto el número, las unidades («-1.8 un.») y la etiqueta («↓ Vendido de más») van en **una sola línea**, como siempre. Si achicás la columna (ver [Ancho de las columnas](#ancho-de-las-columnas)) y no entran, **la etiqueta baja completa al renglón de abajo**, solo en esa fila; si la columna queda todavía más angosta, la etiqueta se parte en palabras y las unidades bajan debajo del número. Nada se oculta. El número queda siempre en el primer renglón, **a la misma altura** que Stock Cierre, Punto stock y Estado de la fila.
- **Totales** (switch *Mostrar totales*): en *Cierre Stock* hay dos tarjetas, **Faltó mercadería** (suma de los Faltantes positivos) y **Vendido de más** (suma de los negativos, en positivo). No se muestra una sola cifra porque lo que faltó y lo vendido de más se compensarían. Ambas se recalculan con las filas visibles.
- El **detalle de producto** (doble clic en la fila) usa el mismo código de colores en *Stock Cierre* y *Faltante*. En *Stock Actual* y *Stock Retroactivo* no hay colores: allí el mismo dato es el stock disponible, no un faltante.
- En ese detalle, *Solo ingresos* y *Egresos* quedan enfrentados renglón a renglón: **Compra | Ventas**, **Elaborado | Elaborado**, **Stock | Stock**, **Movimiento | Movimiento** y **Ajustes** solo del lado de ingresos (del otro hay un renglón vacío), así *Tot. ingresos* y *Tot. egresos* quedan a la misma altura. En el celular las tarjetas van una debajo de la otra y el renglón vacío no se muestra.

## Barra de la tabla y tamaño de letra (desde 2026-11-24)
- En los reportes de stock, **la lupa de buscar, *Mostrar detalles* y *Ver inicial, ingresos y egresos* están en una sola fila baja** (unos 33 px; antes eran dos filas de ~112 px), así entran 2 o 3 filas más de la tabla. Al tocar la lupa, el campo de búsqueda se abre al lado, sobre la misma fila, sin agrandarla. La barra sigue flotando arriba mientras bajás por la tabla, ahora **con la lupa incluida**.
- En Ventas por Producto y Proyección, donde no hay esos interruptores, queda solo la lupa, en una fila más baja que antes.
- **Punto stock** y **Estado** se ven con **1 punto menos de letra** que el resto de las celdas (los encabezados no cambian).

## Ancho de las columnas
Desde 2026-11-24, en las 4 tablas (Stock Actual / Cierre Stock / Stock Retroactivo, Ventas por Producto, Proyección Ventas vs Stock y Balance Económico):
- Pasá el mouse por un encabezado: en su **borde derecho** aparece una rayita. **Arrastrala** para ensanchar o angostar la columna. Tocar la rayita no ordena; tocar el título ordena como siempre.
- **Las columnas del mismo tipo cambian juntas:** todas las «Kg» y todas las «$» de los períodos (Ventas por Producto); «Stock actual», «Ventas» y «Dif.» de todas las sucursales y períodos (Proyección); todos los períodos (Balance); las 12 columnas de *Mostrar detalles* (Stock).
- **Nunca se tapa la columna de al lado ni se corta un número:** al angostar, la columna se frena en su número más largo. Los textos (la etiqueta de Faltante, las unidades) bajan de renglón. El nombre del producto se corta con «…» y se lee completo dejando el mouse encima. Si un número ya no entraba con el ancho de siempre, la columna no deja angostarla más (no se ensancha sola).
- **Doble clic en la rayita** vuelve esa columna a su ancho de siempre. El botón **Anchos** de la barra (junto a *Exportar a Excel*; solo aparece si cambiaste algún ancho) vuelve todas las columnas de la tabla que estás viendo.
- Los anchos **se recuerdan en este navegador** (no en otra PC), uno por tabla, aunque cambies de fechas o sucursales.
- **Balance** ocupa todo el ancho de la pantalla; apenas cambiás un ancho deja de estirarse, para que el cambio se note.
- En el **celular** no hay rayitas y los anchos son los de siempre.
- **Alineación:** en todas las tablas las celdas de una fila se alinean por su **primer renglón**. Si una celda tiene dos renglones (Producto con código debajo, la etiqueta de Faltante abajo), los números de las demás quedan a la altura del primero, no centrados.
- PENDIENTE: verificado con el CSS y el JS reales en una maqueta del navegador (ver `docs/DECISIONS.md` 2026-11-24), no con login en la pantalla real.

## Abrir en una ventana independiente
En la barra de botones (a la derecha de **Mostrar totales**) está el botón **Abrir en ventana**: abre Reportes en una ventana aparte (≈1100×800, se puede mover y redimensionar) **sin menú lateral ni barra superior**, para dejarlo en otro monitor o tener más lugar para las tablas.
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
En la barra de botones (a la derecha de **Mostrar totales**) aparece el botón verde **Exportar a Excel** cuando *Stock Actual*, *Cierre Stock* o *Stock Retroactivo* ya muestran resultados. Descarga un `.xlsx` (nombre `<Reporte>_<fecha>_<hora>.xlsx`) con **los filtros que hay en pantalla** (sucursales, fechas o cierres, producto, tipo, marca y estado), una fila por producto (y sucursal, con 2+ sucursales).
- Las columnas son las de la tabla: código, producto, (sucursal), St. inicial, Total ingresos, Total egresos, Stock Cierre y Faltante (Cierre Stock) o Stock actual, Punto stock y Estado. Con el switch **Mostrar detalles** activo se suman las de ingresos y egresos desglosados.
- Cantidades en **números** (se pueden sumar, ordenar y filtrar; la hoja trae filtro automático y encabezado fijo). El *Faltante* lleva signo y color (rojo: faltó mercadería, ámbar: vendido de más, verde: coincide).
- Las filas con *Stock Actual* se calculan **al momento de exportar** (igual que al buscar). Los otros reportes (Ventas por Producto, Proyección, Balance) no tienen exportación.
- Pide el permiso del reporte que se exporta (Stock Actual, Cierre Stock o Stock Retroactivo). Solo lectura. Código: `ReportesController.ExportarStockExcel`, `Services/ExcelReporteStock.cs`.
