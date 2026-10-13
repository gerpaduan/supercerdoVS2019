---
pantalla: Stock.ExistenciaPorSucursales
titulo: Existencia por sucursales
rol: admin
modulo: Stock
orden: 3
permiso: Stock.VerStock (PENDIENTE: en WebCore el chequeo está omitido, ver TODO en StockController.ExistenciaPorSucursales)
revisada: 2026-10-13
---
## Para qué sirve
Ver el **stock actual de cada producto, una sucursal por columna**, para comparar sucursales o ver una sola.

## Cómo se usa
1. En **Sucursales** abrí la lista y **tildá las que quieras ver**: una, dos, cuatro o todas. Al abrir la pantalla viene tildada solo **tu sucursal**.
   - **Todas** tilda o destilda el resto de un solo toque. Siempre tiene que quedar al menos una tildada (la última no se puede destildar).
   - La lista no se cierra al tildar. El botón muestra el resumen: *Todas*, el nombre de la sucursal, o *N sucursales*.
2. Tocá **Buscar**. Si cambiás la selección después de buscar, aparece el aviso de que hay cambios sin aplicar.
3. La tabla tiene una columna por sucursal tildada. Con **2 o más** sucursales aparece además la columna **Total**, que suma solo las sucursales mostradas.
4. **Filtros avanzados**: Tipo, Proveedor, Marca y Estado de stock.
5. Cada producto se puede expandir para ver el detalle por sucursal (stock inicial, ingresos, egresos, ventas).

## Abrir en una ventana independiente
Al lado de **Filtros avanzados** está el botón **Abrir en ventana**: abre esta misma pantalla en una ventana aparte (≈1100×800, se puede mover y redimensionar) **sin menú lateral ni barra superior**, para dejarla en otro monitor o tener más lugar para la tabla. Es la misma pantalla y la misma sesión (si la sesión vence, la ventana pide ingresar de nuevo). Si ya está abierta, el botón la reutiliza: la recarga con la pantalla inicial (se pierde lo que tuviera buscado) y la trae al frente, sin abrir otra. Si el navegador bloquea la ventana emergente, avisa y hay que permitirla para el sitio. En la ventana independiente no aparece el botón.

## Fecha de cálculo
El cálculo parte del **último cierre de stock** de cada sucursal. Con varias sucursales tildadas, la fecha mínima que se puede consultar es la del cierre **más reciente entre las tildadas**; la pantalla muestra *Cálculo desde dd/mm/aaaa* y, al pasar el mouse, la fecha de cada sucursal.

## Notas
- Con varias sucursales (pero no todas) la consulta calcula todas las sucursales y muestra solo las tildadas, así que tarda lo mismo que con *Todas* (≈1 s).
- La actualización automática y los switches de la pantalla (expandir, estado de stock, diferencia contra punto de stock) no cambiaron.
- PENDIENTE: documentar los switches y el detalle por sucursal con capturas.
