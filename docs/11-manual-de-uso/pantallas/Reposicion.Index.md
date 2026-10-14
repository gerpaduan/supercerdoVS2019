---
pantalla: Reposicion.Index
titulo: Qué pedir
rol: admin
modulo: Stock
orden: 4
permiso: Stock.VerStock (sin el permiso se muestra "Acceso denegado")
revisada: 2026-10-13
---
## Para qué sirve
Ver **qué productos hay que pedir y cuánto**, sucursal por sucursal, para volver al **punto de stock**. Además avisa de los productos que están **en cero y se vienen moviendo**, sin llenar la lista con productos que hace meses no se venden. Es solo de consulta: no genera pedidos ni mueve stock.

## Cómo se usa
1. Elegí las **Sucursales** (una, varias o todas; al abrir viene la tuya).
2. Elegí en cuántos días mirar el movimiento (**"Se considera que se mueve si salió en los últimos…"**, por defecto 30 días).
3. **Buscar** (al abrir la pantalla ya busca solo). Con **Más filtros y opciones** podés acotar por producto, tipo, proveedor o marca.
4. Revisá la tabla, agrupada **por proveedor**. Con **Excel** la descargás (una fila por producto y sucursal) y con **Imprimir** sale solo el resultado, en hoja A4 apaisada (sin menús ni filtros; una fila no se corta entre hojas). **Abrir en ventana** la abre aparte, sin menús.

## Qué productos aparecen
Solo productos que llevan stock (los de *cierre de stock*), en las sucursales elegidas:
- **Bajo el punto** (etiqueta amarilla): tiene punto de stock cargado y el stock actual es **menor** al punto. **Pedir = punto de stock − stock actual**, redondeado hacia arriba (pesables a 0,1 kg; por unidad al entero). Si el stock es igual al punto no hay nada que pedir y no aparece. Un stock negativo cuenta como 0 para el cálculo.
- **En cero** (etiqueta roja): **no tiene punto de stock**, el stock es 0 o negativo **y salió algo en el período elegido**. Se avisa pero **no se inventa una cantidad**: la columna Pedir dice *sin punto*. Si querés una sugerencia igual, en *Más filtros* completá **"Sin punto de stock: sugerir cantidad para N días de venta"**: pide lo que se vende en N días.
- **En cero · sin movimiento** (gris): en cero pero **sin salidas en el período**. **No se avisa**: casi siempre es un producto que ya no se trabaja. Aparece solo como contador y, si tocás **Mostrar** (o el switch de *Más filtros*), en la tabla.

### Qué cuenta como "movimiento"
Ventas **y** uso en elaboración. Los insumos (sal, condimentos, madejas, colorantes) casi no se venden: se gastan al elaborar, y si solo se mirara la venta parecerían productos muertos. Los **movimientos entre sucursales no cuentan** (es el mismo producto cambiando de lugar, no consumo). Un producto vendido como corte (hijo de otro) cuenta para el producto que lleva el stock.

## Leer cada columna
- **Stock / Punto:** stock actual y punto de stock de esa sucursal (— = sin punto cargado).
- **Pedir:** la cantidad sugerida. Debajo, en pesables con peso promedio cargado, las unidades aproximadas (≈ N un.).
- **Motivo:** las etiquetas de arriba, más dos avisos para **revisar antes de pedir**:
  - **Negativo: revisar** — el stock calculado es negativo. Casi siempre es una compra o ingreso que no se cargó, o un cierre de stock desactualizado.
  - **Punto alto?** — el stock que queda alcanza para más de 90 días al ritmo reciente: el punto de stock cargado probablemente quedó alto.
- **Consumo/día:** salidas por día (ventas + elaboración) en el período. **Alcanza:** días que dura el stock actual a ese ritmo.
- **Última salida:** hace cuánto salió algo por última vez (se mira hasta un año atrás; "sin salidas" = nada en el último año).
- **Otras sucursales:** stock del mismo producto en las demás sucursales. Si alguna **cubre el pedido** lo dice (*Se cubre con un movimiento*): conviene mover mercadería en vez de comprar.
- **Último precio / Importe:** último precio de compra del producto (y su fecha) y su valor para la cantidad sugerida. El **importe estimado** de arriba y el de cada proveedor suman solo los productos con precio.
- **Proveedor:** el de la compra más reciente del producto (si se filtra por proveedor, ese). Los productos sin proveedor cargado van al final en *Sin proveedor cargado*.

## Avisos de la pantalla
- **Cierre de stock viejo:** el stock se calcula desde el **último cierre de stock de cada sucursal**. Si pasaron más de 30 días (o no hay ninguno) se avisa: lo que no se haya cargado desde entonces no está en el stock.
- **"N de M productos no tienen punto de stock"**: de esos solo se avisa si están en cero y se movieron. Cargá el punto de stock en *Productos → Ver stock por sucursales* para que el sistema calcule cuánto pedir de todos.

## Notas y límites
- El sistema **no registra pedidos a proveedor**: si ya pediste algo que todavía no llegó, la pantalla lo sigue sugiriendo. Mirá la fecha de la última compra (columna *Último precio*).
- **Pedir no considera múltiplos** (cajas, bultos, presentaciones): redondea solo a 0,1 kg o a la unidad.
- Los **días de movimiento** cambian mucho el resultado: con 7 días casi todo "en cero" parece sin movimiento; con 90 aparecen productos de temporada. Elegí según lo rápido que rota tu mercadería.
- Hoy solo funciona cuando el sistema usa Postgres; con otro motor la pantalla avisa y no busca.
- PENDIENTE: documentar con capturas la tabla y el Excel; confirmar con el uso real si 30 días es el valor por defecto adecuado.

## Exportar a Excel
**Excel** descarga lo que ves en pantalla (los filtros de la **última búsqueda**, no los que hayas tocado después sin volver a buscar): una hoja `Qué pedir` con encabezado de filtros y avisos, una fila por producto y sucursal ordenada por proveedor, columnas numéricas (se pueden sumar y ordenar), observaciones por fila y el total estimado al pie. Código: `ReposicionController.ExportarExcel`, `Services/ExcelReposicion.cs`.
