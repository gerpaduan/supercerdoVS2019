---
pantalla: Pedidos.Index
titulo: Pedidos
rol: admin
modulo: Stock
orden: 5
permiso: Stock.VerStock (sin el permiso se muestra "Acceso denegado")
revisada: 2026-10-19
alias: Pedidos.Buscar
---
## Para qué sirve
Ver los **pedidos de mercadería guardados**, buscarlos y abrirlos. Los pedidos se arman en **Stock → Existencia por sucursales** con **Ingresar pedido**; acá quedan guardados para consultarlos, imprimirlos, mandarlos por mail o corregirlos.

Solo funciona con la base de datos nueva (Postgres). Con la base anterior (SQL Server) la pantalla avisa que no está disponible.

## Cómo se usa
1. Al entrar muestra los pedidos de los **últimos 30 días**, el más reciente primero.
2. Filtros (se aplican con **Buscar**):
   - **Desde / Hasta**: fecha del pedido (el día *Hasta* se incluye entero).
   - **Sucursal**: pedidos que tengan al menos un producto para esa sucursal.
   - **Usuario**: quién lo hizo (alcanza con parte del nombre).
   - **Producto, código o nota**: pedidos que incluyan ese producto (por nombre o código exacto) o cuya nota o usuario contenga el texto.
3. Cada fila muestra **N°**, fecha (con la marca *modificado* si se corrigió después), usuario, **sucursales** que cubre, cantidad de **productos** y de **líneas** (una línea es un producto para una sucursal), **total estimado** y la nota.
4. **Ver** (o el número) abre el detalle: [Pedido (detalle)](ayuda:pantalla/Pedidos.Detalle).

## Qué significa el total estimado
Es una **estimación**: cantidad pedida × el **precio de la última compra** de ese producto, **sin distinguir proveedor**. Los productos **sin precio de compra no suman** y se avisan debajo del total (*parcial · N sin precio*: el total está incompleto). No es el costo final ni una factura.

## Límites
- Muestra como máximo los **500 pedidos** más recientes que cumplan el filtro; para ver más viejos, acotá las fechas.
- Los pedidos **no se borran**: se modifican y queda el historial (quién, cuándo y qué cambió).

## Si algo no anda
- *No hay pedidos con esos filtros*: ampliá las fechas o sacá el filtro de usuario/sucursal.
- *Acceso denegado*: tu usuario no tiene el permiso de ver stock.
