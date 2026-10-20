---
pantalla: Stock.ExistenciaPorSucursales
titulo: Existencia por sucursales
rol: admin
modulo: Stock
orden: 3
permiso: Stock.VerStock (PENDIENTE: en WebCore el chequeo está omitido, ver TODO en StockController.ExistenciaPorSucursales)
revisada: 2026-10-19
---
## Para qué sirve
Ver el **stock actual de cada producto, una sucursal por grupo de columnas**, para comparar sucursales o ver una sola, saber qué falta (contra el **punto de stock**) y exportarlo.

## Cómo se usa
1. Al abrir la pantalla **busca sola** y la **autoactualización queda encendida** (botón **Auto**). No hace falta tocar nada para ver el stock.
2. **Sucursales**: abrí la lista y tildá las que quieras ver (una, varias o todas). Al abrir viene tildada solo **tu sucursal**; siempre tiene que quedar al menos una. Si cambiás la selección después de buscar, aparece *Hay filtros sin aplicar*: tocá **Buscar**.
3. La tabla tiene **Cód. | Producto | [Stock | Estado] por cada sucursal | [Stock | Estado] Total**. El **Total** aparece solo con 2 o más sucursales y suma las mostradas. Entre una sucursal y la siguiente hay una **doble línea** y un tinte distinto, para no confundirlas.
4. **Ordenar**: tocá un encabezado (Cód., Producto, Stock o Estado, de cada sucursal o del Total); tocarlo de nuevo invierte el orden. Cód. y Producto ordenan de menor a mayor; Stock empieza por el mayor; **Estado empieza por los faltantes** (primero los negativos, después los que más les falta). El orden se mantiene cuando la tabla se actualiza sola.
5. **Producto o código**: el cuadro de búsqueda filtra al escribir lo que ya está en pantalla; con Enter o **Buscar** vuelve a consultar con ese texto.
6. **Estado** (lista de la barra): *Todos los estados*, *Bien (OK)*, *Faltantes* (negativo, falta o justo en el punto) o *Punto de stock 0* (productos sin punto cargado). Con varias sucursales, un producto entra si **alguna** de las mostradas cumple.
7. **Expandir** muestra el detalle por sucursal (stock inicial, ingresos, egresos, ventas); también se abre con doble click en la fila o con el ➕.

## La columna Estado
Compara el stock con el **punto de stock de esa sucursal**:
- **Falta 21,650** (rojo) y, chico, **pto 60**: falta esa cantidad para llegar al punto.
- **Negativo** (rojo): el stock es menor que cero (suele ser un ingreso sin cargar).
- **En el punto** (ámbar): el stock es igual al punto.
- **OK** (verde): por encima del punto.
- **Sin punto** / **Sin stock** (gris): no hay punto cargado.
- En el **Total**, el estado compara el stock total con la **suma de los puntos** de las sucursales mostradas.

El botón **Estado** de la barra muestra u oculta estas columnas.

**Editar el punto de stock sin salir**: si tenés permiso para editar productos, al lado de **pto** hay un lápiz ✎. Tocalo, escribí el número entero y apretá **Enter** (o *Guardar*); la pantalla actualiza el estado y los filtros sin volver a buscar. Es el mismo punto que se edita en Productos → *Ver stock por sucursales*.

## Filtros avanzados (cerrados hasta que los abras)
El botón **Filtros** despliega el panel; si hay filtros activos con el panel cerrado, el botón muestra la cantidad (por ejemplo *Filtros (2)*). Todos se aplican con **Buscar**.
- **Tipos**, **Proveedores** y **Marcas**: lista con buscador; podés tildar **uno o varios** de cada una. Dentro de una lista alcanza con que el producto coincida con **cualquiera** de los tildados; entre listas tiene que cumplir **todas** (tipo **y** proveedor **y** marca). Sin nada tildado = sin filtro ("Todos").
  - **Proveedores** muestra solo los proveedores a los que **se les compró** algo (compras de productos, no anuladas) y filtra los productos que se les compraron. No cuenta los movimientos de stock (cierres, ingresos, ajustes), aunque se guarden con un proveedor.
- **Período desde**: una fecha con atajos de 7 a 365 días (se recuerda el último atajo que usaste). Define desde cuándo se cuentan las ventas y el uso en elaborados.
- **Con movimiento** (solo con Postgres): *Solo con al menos [X]*; deja únicamente los productos que **tuvieron movimiento desde la fecha del período hasta ahora**. Medida a elegir: **Salidas (kg/un.)** = lo **vendido más lo que se usó en elaborados** (así un insumo que no se vende pero se gasta en elaborados cuenta como producto con movimiento) o **Días con venta** = en cuántos días distintos se vendió (solo ventas). Con varias sucursales cumple si **alguna** de las mostradas alcanza el mínimo. Sirve para sacar de la vista lo que hace tiempo no se mueve.

## Ingresar pedido
El botón grande **Ingresar pedido** (el primero de la barra) convierte la pantalla en una planilla para **armar un pedido de compra**. Solo funciona con la base de datos nueva (Postgres); con la anterior avisa que no está disponible.

1. Elegí las **sucursales** y, si querés, filtros (Estado *Faltantes* aísla lo que está bajo el punto). Encendé **Ingresar pedido**.
2. Cada sucursal gana una columna **A pedir**: escribí ahí la cantidad (coma o punto decimal, hasta 3 decimales). **Enter** baja al producto de abajo **en la misma sucursal** (Shift+Enter sube); al llegar al final pasa a la primera fila de la sucursal siguiente. Al entrar a una celda se selecciona todo lo que tenía: lo que escribas lo reemplaza. Junto a la cantidad aparece **≈ N un.** (solo productos por peso con promedio cargado).
3. Al final de la tabla: **Total pedido** (suma de todas las sucursales; se puede ordenar), **$ últ. compra** e **Importe est.** (total pedido × último precio de compra). Arriba, en la barra del pedido: **Total estimado** y cuántos productos quedaron **sin precio de compra** (esos **no suman**: nunca se cuentan como $0). Si falta el precio de alguno, el total se rotula **Total estimado (parcial)**: está incompleto.
4. **Guardar pedido** abre una **vista preliminar** con **solo los productos que tienen cantidad** (los que quedaron vacíos o en 0 no aparecen). *Volver a editar* no pierde nada; **Confirmar y guardar** lo guarda en el módulo **Pedidos** y ofrece imprimir el ticket, PDF, enviar por email o exportar a Excel. Las líneas marcadas con ⚠ ya tenían **stock suficiente según el consumo del período**: no bloquea, es solo un aviso.
5. Mientras **Ingresar pedido** está encendido la **autoactualización queda en pausa** (actualizar reemplazaría la tabla mientras escribís) y se reanuda al apagarlo o al guardar. Apagarlo **no borra** lo cargado: vuelve al encenderlo. Si intentás salir de la pantalla con cantidades sin guardar, pide confirmación. **Limpiar** borra todas las cantidades.

### Ingresar pedido: de dónde sale la sugerencia
Junto a cada cantidad se ve, en gris, una **sugerencia** (por ejemplo `sug. 52,500`). **Nunca se carga sola**: la aceptás vos.

**La fórmula.** La sugerencia de cada producto **en cada sucursal** es *lo que se consumió en el período, menos el stock que ya hay*. **Consumo = lo vendido + lo que se usó de ese producto en elaborados** (insumos como sal o pimienta no se venden, pero se gastan al elaborar), desde la fecha **Período desde** hasta ahora. Una leyenda fija en la barra del pedido lo recuerda con la fecha y los días (por ejemplo *desde 19/09/2026 (30 días)*) y el botón **Cambiar** lleva a ese campo; al pasar el mouse por la sugerencia se ve el desglose.

**Ejemplo con pedidos cada 30 días.** Ponés *Período desde* = hoy − 30 días (atajo **30 d** en Filtros). Un producto vendió **80,500** y se usaron **12,000** en elaborados: consumo **92,500**. En esa sucursal hay **40,000** de stock. Sugerencia = 92,500 − 40,000 = **52,500**. El tooltip dice: *Vendido 80,500 + usado en elaborados 12,000 desde 19/09 = 92,500 − stock 40,000 = sugerido 52,500*. Si el stock alcanza (por ejemplo 100,000), no se sugiere nada y la celda dice **✔ stock suficiente**.

**Qué muestra cada celda** (C = consumo del período, S = stock, P = punto de stock de esa sucursal):
- **S ≥ C y S ≥ P**: **✔ stock suficiente** (sin sugerencia).
- **S < C**: pista **sug. C − S** (ya descontado el stock que hay).
- **S < P**: además un chip **⚠ bajo pto · +N**, que ofrece completar hasta el punto (P − S).
- **Sin consumo en el período** (C = 0): no hay pista; el chip del punto aparece igual si el stock está bajo el punto.
- Si el stock es **negativo** se toma como **0** (un negativo es un error de carga: contarlo inflaría el pedido); el tooltip lo aclara.

**Cómo aceptarlas.** (a) **Aceptar sugerencias** (barra del pedido): todas las sucursales, solo los productos que se ven (respeta filtros y orden); (b) la **varita** del encabezado de cada sucursal: solo esa sucursal; (c) un click en la **pista** o en el **chip** de una celda. Si hay celdas con cantidad ya cargada, un cartel pregunta: *Solo vacías*, *Reemplazar también las cargadas* o *Cancelar*. **Sugerir según** cambia qué se sugiere: *Consumo − stock* (por defecto), *Hasta el punto de stock* o *El mayor de los dos*. **Completar bajo el punto (N)** carga *hasta el punto* solo en las N celdas que están bajo el punto. Un resumen indica cuántas celdas están bajo el punto (por sucursal) y cuántas tienen stock suficiente. Las sugerencias se redondean a 3 decimales (también en productos que no son por peso).

**Importes.** El precio es el de la **última compra real** de cada producto (las compras de productos registradas, no la tabla de proveedores habituales de *Qué pedir*: ahí puede diferir), por kg o por unidad, y el tooltip dice proveedor y fecha, y la vista previa los muestra debajo del precio. **No se distingue proveedor:** se toma el precio de la última compra de ese producto **sea cual sea el proveedor** (la barra del pedido lo aclara). Si esa compra tiene más de **90 días** (configurable, ver [configuraciones](ayuda:referencia/configuraciones-plataforma)), el precio aparece con ⚠ y la cantidad de días: puede estar desactualizado. **Los productos que nunca se compraron como compra de productos** (por ejemplo cortes que salen de la media res o elaborados propios) **no tienen precio** y quedan "sin precio": no suman al total. **Una compra de hace más de 365 días tampoco da precio** (configurable, `Pedidos:DiasPrecioMaximo`): un precio de hace años subestima mucho el total por la inflación, así que ese producto queda "sin precio" y el tooltip del guion dice de qué fecha es su última compra.

### Si se corta la luz o se cierra el navegador
El pedido en curso **se guarda solo en el servidor** (unos segundos después de cada cambio) y queda como **borrador**. Si se interrumpe, el botón **Borradores sin guardar** (siempre visible, con un número si hay alguno) lista los pedidos a medio cargar: **Cargar** vuelve a buscar con las mismas sucursales y filtros, enciende *Ingresar pedido* y vuelve a poner las cantidades (si un producto ya no aparece, avisa cuántas líneas no se encontraron y las conserva). **Descartar** pide un motivo y avisa al administrador. Solo podés cargar o descartar los tuyos. Lo que se guarda son las **cantidades**, no las sugerencias. Un pedido muy grande (más de 300 líneas, una por producto y sucursal) deja de resguardarse solo y la barra lo avisa.

## Diseño compacto
El botón **Compacto** pasa a un diseño de una sola línea de barra y **filas de un renglón** para ver muchos productos a la vez (en la ventana independiente caben más de 20 filas). La tabla scrollea por dentro con el encabezado y las columnas **Cód.** y **Producto** fijos. **Se recuerda en el navegador**: la pantalla normal arranca como la dejaste (la primera vez, en diseño normal) y la **ventana independiente arranca compacta** (guarda su propia elección).

## Abrir en una ventana independiente
**Ventana** abre esta misma pantalla aparte (≈1100×800, se puede mover y redimensionar) **sin menú lateral ni barra superior**, para dejarla en otro monitor. Es la misma pantalla y la misma sesión. Si ya está abierta, el botón la reutiliza (la recarga y la trae al frente). Si el navegador bloquea la ventana emergente, avisa y hay que permitirla para el sitio. En la ventana independiente no aparece el botón.

## Autoactualización
- Está **encendida al abrir**. Actualiza el stock cada **15 minutos, hasta 6 veces**; el botón **Auto** muestra el tiempo que falta. Podés apagarla con el mismo botón.
- Al completar las 6 se apaga y aparece un cartel: **¿Seguir actualizando el stock?** *Sí* actualiza ya y arranca otras 6; *No* la deja apagada.
- Si el **servidor está ocupado** (hay otros cálculos de stock en curso o el último tardó mucho), la actualización automática **se posterga**: la pantalla muestra *Servidor ocupado, reintento a las HH:MM*, **deja los datos que ya se ven**, no gasta una de las 6 vueltas y reintenta en un par de minutos. El botón **Buscar** nunca se bloquea: si lo apretás, consulta igual. Los límites son del servidor (ver `docs/11-manual-de-uso/referencia/configuraciones-plataforma.md`).

## Exportar a Excel o PDF
Los botones **Excel** y **PDF** exportan **lo que se ve**: los productos con los filtros y el **orden actuales** (y las columnas Estado si están visibles). El stock se recalcula en el momento de exportar, así que puede diferir levemente de lo que ya estaba en pantalla; el archivo dice la fecha y hora del stock y los filtros del servidor aplicados (los de la barra, como el de Estado, ya quedan reflejados en qué productos salen). Tope: 20.000 productos por archivo.

## Fecha de cálculo
El cálculo parte del **último cierre de stock** de cada sucursal. Con varias sucursales tildadas, la fecha mínima que se puede consultar es la del cierre **más reciente entre las tildadas**; la pantalla muestra *Cálculo desde dd/mm/aaaa* y, al pasar el mouse, la fecha de cada sucursal.

## Notas
- Con varias sucursales (pero no todas) la consulta calcula todas las sucursales y muestra solo las tildadas, así que tarda lo mismo que con *Todas* (≈1 s).
- El estado de cada celda usa el **punto de stock de la sucursal**. (Antes el estado del detalle salía de un punto global antiguo; ahora coincide con el que se ve en pantalla.)
- El detalle expandido de un producto **no** se actualiza al editar un punto desde la tabla; se corrige en la próxima actualización.
- Los pedidos guardados se ven en **Stock → Pedidos** (ver `Pedidos.Index`).
- PENDIENTE: con una **cuenta de producción** el pedido se guarda a nombre de la cuenta de la sesión (no pide elegir la persona como Stock o Movimientos).
- PENDIENTE: las cantidades sugeridas se redondean a 3 decimales también en productos que no son por peso; si hace falta que sean enteros hay que definirlo.
- PENDIENTE: capturas de la pantalla.
