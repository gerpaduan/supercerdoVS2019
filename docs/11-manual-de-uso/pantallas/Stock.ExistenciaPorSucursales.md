---
pantalla: Stock.ExistenciaPorSucursales
titulo: Existencia por sucursales
rol: admin
modulo: Stock
orden: 3
permiso: Stock.VerStock (PENDIENTE: en WebCore el chequeo está omitido, ver TODO en StockController.ExistenciaPorSucursales)
revisada: 2026-10-30
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

**Editar el punto de stock sin salir**: si tenés permiso para editar productos, en la barra aparece el interruptor **Editar puntos**. Viene **apagado** cada vez que abrís la pantalla (no se recuerda) y los lápices ✎ están ocultos, para no modificar un punto sin querer. Al encenderlo aparece un lápiz ✎ al lado de **pto**; al apagarlo vuelve a ocultarse (y se cierra el editor si estaba abierto). Sin permiso no hay interruptor ni lápiz. Con el lápiz a la vista, tocalo, escribí el número entero y apretá **Enter** (o *Guardar*); la pantalla actualiza el estado y los filtros sin volver a buscar. Es el mismo punto que se edita en Productos → *Ver stock por sucursales*.

## Filtros avanzados (cerrados hasta que los abras)
El botón **Filtros** despliega el panel; si hay filtros activos con el panel cerrado, el botón muestra la cantidad (por ejemplo *Filtros (2)*). Todos se aplican con **Buscar**.
- **Tipos**, **Proveedores** y **Marcas**: lista con buscador; podés tildar **uno o varios** de cada una. Dentro de una lista alcanza con que el producto coincida con **cualquiera** de los tildados; entre listas tiene que cumplir **todas** (tipo **y** proveedor **y** marca). Sin nada tildado = sin filtro ("Todos").
  - **Proveedores** muestra solo los proveedores a los que **se les compró** algo (compras de productos, no anuladas) y filtra los productos que se les compraron. No cuenta los movimientos de stock (cierres, ingresos, ajustes), aunque se guarden con un proveedor.
- **Período desde**: una fecha con atajos de **1, 2, 3, 7, 15, 30, 60 y 365 días** (se recuerda el último atajo que usaste; si tenías guardado 90 o 180, que ya no existen, vuelve a 30). Define desde cuándo se cuentan las ventas y el uso en elaborados. **+ Fecha hasta** agrega una segunda fecha y el período queda **cerrado** (por ejemplo, el mes pasado): se cuenta de *desde* a *hasta*, ambos días **enteros**; la **×** la quita y se vuelve a contar hasta ahora. La fecha hasta **arranca oculta cada vez** que abrís la pantalla, no puede ser futura ni anterior al *desde*. Con fecha hasta, un atajo **N d** significa **los N días que terminan en esa fecha** (*desde* = *hasta* − N) y se mueve solo si cambiás el *hasta*. Vale para **Con movimiento** y para las sugerencias de **Ingresar pedido**; el **stock** que se ve sigue siendo el actual (no el de esa época).
- **Con movimiento** (solo con Postgres): *Solo con al menos [X]*; deja únicamente los productos que **tuvieron movimiento desde la fecha del período hasta ahora** (o hasta la fecha *hasta*, si la agregaste). Medida a elegir: **Salidas (kg/un.)** = lo **vendido más lo que se usó en elaborados** (así un insumo que no se vende pero se gasta en elaborados cuenta como producto con movimiento) o **Días con venta** = en cuántos días distintos se vendió (solo ventas). Con varias sucursales cumple si **alguna** de las mostradas alcanza el mínimo. Sirve para sacar de la vista lo que hace tiempo no se mueve.

### Modo «stock del destino» (dentro de un movimiento)
[Nuevo movimiento](ayuda:pantalla/Movimientos.Editar) abre esta misma pantalla como panel de stock de la sucursal **destino** (en un marco de la pantalla o en una ventana independiente). En ese modo: siempre compacta, **solo la sucursal destino**, sin *Ingresar pedido*, exportación, selector de sucursales ni el filtro *Con movimiento*; el **período** (con los atajos 1, 2, 3, 7 y 15 días y la fecha hasta) queda siempre a la vista y hay una columna **Sugerido** (ventas + uso en elaborados del período − stock). El **doble click** en una fila (y el click en el número sugerido) cargan el producto en el movimiento en vez de abrir el detalle, y los productos que el movimiento ya lleva se marcan. Detalle en la página del movimiento.

### Resumen de los filtros aplicados
Con el panel **Filtros** cerrado, si hay filtros avanzados aplicados aparece **una línea bajo la barra** con un chip por cada uno: `Tipo: Cerdo, Vacuno`, `Proveedor: … +1`, `Marca: …` y `Movimiento ≥ 5 salidas · 28/09 al 05/10` (con las listas largas muestra los dos primeros y «+N»; el tooltip trae todo). Así no perdés de vista **por qué faltan productos**. No repite Sucursales, texto ni Estado (siempre están a la vista en la barra) y no ocupa lugar si no hay filtros ni cuando el panel está abierto.
- Muestra lo de la **última búsqueda**. Si cambiaste filtros y todavía no tocaste *Buscar*, agrega **⚠ sin aplicar**.
- **Click en un chip**: abre el panel con el cursor en ese filtro. **×**: quita ese filtro y **vuelve a buscar**. **Limpiar**: quita todos los filtros avanzados y busca.
- Se ve igual con la barra del pedido abierta o colapsada.

## Ingresar pedido
El botón **Ingresar pedido** (el primero de la barra, del mismo tamaño que los demás botones, con contorno azul que se rellena al encenderlo) convierte la pantalla en una planilla para **armar un pedido de compra**. Solo funciona con la base de datos nueva (Postgres); con la anterior avisa que no está disponible.

1. Elegí las **sucursales** y, si querés, filtros (Estado *Faltantes* aísla lo que está bajo el punto). Encendé **Ingresar pedido**.
2. Cada sucursal gana una columna **A pedir**: escribí ahí la cantidad (coma o punto decimal, hasta 3 decimales). **Enter** baja al producto de abajo **en la misma sucursal** (Shift+Enter sube); al llegar al final pasa a la primera fila de la sucursal siguiente. Al entrar a una celda se selecciona todo lo que tenía: lo que escribas lo reemplaza. Junto a la cantidad aparece **≈ N un.** (solo productos por peso con promedio cargado).
3. Al final de la tabla: **Total pedido** (suma de todas las sucursales; se puede ordenar). Las columnas **$ últ. compra** e **Importe est.** (total pedido × último precio de compra) están **ocultas siempre**: se ven solo si encendés el interruptor **$ Ver precios** de la barra del pedido, y ese interruptor también agrega esas dos columnas a la **vista preliminar**. No se recuerda: la próxima vez que abras la pantalla vuelve a arrancar apagado. Si lo apagás con la tabla ordenada por *Importe*, vuelve al orden por código. Arriba, en la barra del pedido, siempre a la vista: **Total estimado** y cuántos productos quedaron **sin precio de compra** (esos **no suman**: nunca se cuentan como $0). Si falta el precio de alguno, el total se rotula **Total estimado (parcial)**: está incompleto.
4. **Guardar pedido** abre una **vista preliminar** con **solo los productos que tienen cantidad** (los que quedaron vacíos o en 0 no aparecen). *Volver a editar* no pierde nada; **Confirmar y guardar** lo guarda en el módulo **Pedidos** y ofrece imprimir el ticket (**uno por sucursal, sin importes**: ver [el pedido](ayuda:pantalla/Pedidos.Detalle)), PDF, enviar por email o exportar a Excel. Las líneas marcadas con ⚠ ya tenían **stock suficiente según el consumo del período**: no bloquea, es solo un aviso.
5. **Colapsar la barra del pedido.** El botón **▲** de la esquina izquierda de la barra (leyenda, *Sugerir según*, botones y resúmenes) la **reduce a una sola línea** —el resumen («4 productos · Total estimado…») y un **Guardar pedido**— para que no ocupe el alto de la pantalla; la tabla aprovecha ese lugar. **▼** la vuelve a abrir. Se recuerda en este navegador: la próxima vez que enciendas *Ingresar pedido* queda como la dejaste.
6. Mientras **Ingresar pedido** está encendido la **autoactualización queda en pausa** (actualizar reemplazaría la tabla mientras escribís) y se reanuda al apagarlo o al guardar. Apagarlo **no borra** lo cargado: vuelve al encenderlo. Si intentás salir de la pantalla con cantidades sin guardar, pide confirmación. **Limpiar** borra todas las cantidades.

### Ingresar pedido: de dónde sale la sugerencia
Junto a cada cantidad se ve, en gris, una **sugerencia** (por ejemplo `sug. 52,500`). **Nunca se carga sola**: la aceptás vos.

**La fórmula.** La sugerencia de cada producto **en cada sucursal** es *lo que se consumió en el período, menos el stock que ya hay*. **Consumo = lo vendido + lo que se usó de ese producto en elaborados** (insumos como sal o pimienta no se venden, pero se gastan al elaborar), desde la fecha **Período desde** hasta ahora (o hasta la fecha *hasta*, si la agregaste). Una leyenda fija en la barra del pedido lo recuerda con la fecha y los días (por ejemplo *desde 19/09/2026 (30 días)* o *desde 01/09/2026 hasta 30/09/2026 (29 días)*) y el botón **Cambiar** lleva a ese campo; al pasar el mouse por la sugerencia se ve el desglose.

**Ejemplo con pedidos cada 30 días.** Ponés *Período desde* = hoy − 30 días (atajo **30 d** en Filtros). Un producto vendió **80,500** y se usaron **12,000** en elaborados: consumo **92,500**. En esa sucursal hay **40,000** de stock. Sugerencia = 92,500 − 40,000 = **52,500**. El tooltip dice: *Vendido 80,500 + usado en elaborados 12,000 desde 19/09 = 92,500 − stock 40,000 = sugerido 52,500*. Si el stock alcanza (por ejemplo 100,000), no se sugiere nada y la celda dice **✔ stock suficiente**.

**Qué muestra cada celda** (C = consumo del período, S = stock, P = punto de stock de esa sucursal):
- **S ≥ C y S ≥ P**: **✔ stock suficiente** (sin sugerencia).
- **S < C**: pista **sug. C − S** (ya descontado el stock que hay).
- **S < P**: además un chip **⚠ bajo pto · +N**, que ofrece completar hasta el punto (P − S).
- En el **diseño compacto** esos textos van en su versión corta para ocupar menos: **✔** (stock suficiente) y **⚠ +N** (bajo el punto); el tooltip dice el detalle.

**Cómo se ve la columna A pedir.** La casilla queda **pegada al Estado** (alineada a la izquierda, sin ancho de más) y, en compacto, mide lo justo para un número como `159,256`; la sugerencia y el `≈ N un.` van a su lado. La fila en la que estás escribiendo se **resalta de punta a punta**, para seguir producto, stock y estado hasta la casilla. Con una sola sucursal y la pantalla de escritorio todo (Cód., Producto, Stock, Estado, A pedir y Total pedido) entra sin desplazar.
- **Sin consumo en el período** (C = 0): no hay pista; el chip del punto aparece igual si el stock está bajo el punto.
- Si el stock es **negativo** se toma como **0** (un negativo es un error de carga: contarlo inflaría el pedido); el tooltip lo aclara.

**Cómo aceptarlas.** (a) **Aceptar sugerencias** (barra del pedido): todas las sucursales, solo los productos que se ven (respeta filtros y orden); (b) la **varita** del encabezado de cada sucursal: solo esa sucursal; (c) un click en la **pista** o en el **chip** de una celda. Si hay celdas con cantidad ya cargada, un cartel pregunta: *Solo vacías*, *Reemplazar también las cargadas* o *Cancelar*. **Sugerir según** cambia qué se sugiere: *Consumo − stock* (por defecto), *Hasta el punto de stock* o *El mayor de los dos*. **Completar bajo el punto (N)** carga *hasta el punto* solo en las N celdas que están bajo el punto. Un resumen indica cuántas celdas están bajo el punto (por sucursal) y cuántas tienen stock suficiente. Las sugerencias se redondean a 3 decimales (también en productos que no son por peso).

**Importes.** El precio es el de la **última compra real** de cada producto (las compras de productos registradas, no la tabla de proveedores habituales de *Qué pedir*: ahí puede diferir), por kg o por unidad, y el tooltip dice proveedor y fecha, y la vista previa los muestra debajo del precio (las columnas de precio solo se ven con **$ Ver precios** encendido). **No se distingue proveedor:** se toma el precio de la última compra de ese producto **sea cual sea el proveedor** (la barra del pedido lo aclara). Si esa compra tiene más de **90 días** (configurable, ver [configuraciones](ayuda:referencia/configuraciones-plataforma)), el precio aparece con ⚠ y la cantidad de días: puede estar desactualizado. **Los productos que nunca se compraron como compra de productos** (por ejemplo cortes que salen de la media res o elaborados propios) **no tienen precio** y quedan "sin precio": no suman al total. **Una compra de hace más de 365 días tampoco da precio** (configurable, `Pedidos:DiasPrecioMaximo`): un precio de hace años subestima mucho el total por la inflación, así que ese producto queda "sin precio" y el tooltip del guion dice de qué fecha es su última compra.

### Si se corta la luz o se cierra el navegador
El pedido en curso **se guarda solo en el servidor** (unos segundos después de cada cambio) y queda como **borrador**. Si se interrumpe, el botón **Borradores sin guardar** (siempre visible, con un número si hay alguno) lista los pedidos a medio cargar: **Cargar** vuelve a buscar con las mismas sucursales y filtros, enciende *Ingresar pedido* y vuelve a poner las cantidades (si un producto ya no aparece, avisa cuántas líneas no se encontraron y las conserva). **Descartar** pide un motivo y avisa al administrador. Solo podés cargar o descartar los tuyos. Lo que se guarda son las **cantidades**, no las sugerencias. Un pedido muy grande (más de 300 líneas, una por producto y sucursal) deja de resguardarse solo y la barra lo avisa.

## Diseño compacto
La pantalla es **siempre compacta** (ya no hay botón **Compacto**): una sola línea de barra y **filas de un renglón** para ver muchos productos a la vez (en la ventana independiente caben más de 20 filas). La tabla scrollea por dentro con el encabezado y las columnas **Cód.** y **Producto** fijos. La elección que se había guardado en el navegador con el botón se ignora.

**Letra y Stock.** La letra de la tabla es la base de 12,5 px (desde 2026-10-30 es **1 punto menor** que antes) y **no cambia el alto de las filas** (el interlineado es fijo). La columna **Stock** va en **negrita** (el `≈ N un.` de al lado, en letra normal). El contenido de la columna **Estado** (el estado y el «pto N») va **1 punto por debajo de Stock** (unos 11,2 px). Con la letra menor y Producto más angosto, en una ventana independiente de **640 px** (un tercio de una pantalla de 1920 px) Cód., Producto, Stock y Estado deberían entrar sin desplazar (PENDIENTE: no medido con la letra nueva).

### Ancho de las columnas
Para tener todo a la vista sin mover los ojos, **Cód.** y **Producto** tienen **ancho fijo** y van pegados: Cód. de **15 caracteres** (entra un código de barras de 13 dígitos) y Producto de **25 caracteres**. Un nombre más largo se corta con «…» y se lee completo dejando el mouse encima. **Stock, Estado y A pedir** toman el ancho de su contenido: no se estiran para llenar la pantalla; el espacio que sobra queda vacío a la derecha de la tabla. Con muchas sucursales o con **Ingresar pedido** encendido la tabla no entra en una ventana angosta: se desplaza hacia los costados y **Cód. y Producto quedan fijos**.

## Abrir en una ventana independiente
**Ventana** abre esta misma pantalla aparte (**compacta**) **sin menú lateral ni barra superior**, para dejarla en otro monitor. En una pantalla de escritorio **abre con un tercio del ancho de la pantalla** (con un mínimo de 560 px: en una pantalla de 1920 px son 640 px; en una de 1366 px, 560) y el alto de siempre (800 px); en pantallas chicas abre de ≈1100 px. Se puede mover y redimensionar. **En esta ventana, al encender *Ingresar pedido* (cuando ya cargaron las sugerencias) o *Ver precios*, si hay columnas que quedan fuera de la ventana, aparece un cartel que propone ensancharla** (*Sí, ensanchar* / *No, gracias*): la ventana se ensancha lo que falta (hasta el ancho de la pantalla) y, si se saldría por la derecha, se corre a la izquierda. Si decís *No, gracias* no vuelve a preguntar hasta que cierres la ventana; si la pantalla no alcanza, avisa y el resto de las columnas se ve desplazando la tabla (Cód. y Producto quedan fijos). En una pestaña normal del navegador no se propone nada. En una ventana angosta de escritorio se mantiene el diseño de escritorio (barra compacta, columnas de ancho fijo); el diseño «de celular» (controles uno debajo del otro) queda para dispositivos táctiles. Es la misma pantalla y la misma sesión. Si ya está abierta, el botón la reutiliza (la recarga y la trae al frente). Si el navegador bloquea la ventana emergente, avisa y hay que permitirla para el sitio. En la ventana independiente no aparece el botón.

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
