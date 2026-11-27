---
pantalla: Ventas.Lineas
titulo: Líneas de venta
rol: usuario
modulo: Ventas
orden: 3
permiso: PENDIENTE (la acción no valida un permiso propio; a los usuarios de producción les pide elegir el operador de Ventas antes de entrar)
revisada: 2026-11-27
---
## Para qué sirve
Ver **cada venta con sus líneas** (producto, cantidad en kilos, precio y total de cada línea) de un período, con una tarjeta por venta. Sirve para revisar qué se vendió producto por producto y para **exportarlo a PDF o Excel**.

## Filtros
- **Sucursal**: al cambiarla, la pantalla se recarga sola.
- **Producto** está aparte, **justo arriba de la tabla**; en computadora el cursor queda puesto ahí al entrar. La **x** lo borra y Enter no recarga la pantalla.
- **Limpiar filtros** (al lado de *Buscar*): vuelve a hoy de 00:00 a 23:59, todas las sucursales y sin cliente, vendedor, formas de pago ni producto.
- **Forma de pago** son botones: tocá uno para marcarlo y otra vez para sacarlo.
- **Fecha desde / Fecha hasta**: al cambiarlas aparece el aviso de que hay filtros sin aplicar; tocá **Buscar** para recargar los datos.
- **Producto** (código o descripción), **Cliente**, **Vendedor** y **Forma de pago** (Efectivo, Debito, Credito, CtaCte, Qr, Transferencia): se aplican en el momento sobre lo ya cargado, sin recargar, y los totales de arriba se recalculan.
- **Cliente** y **Vendedor** se pueden escribir (busca por letras) o **elegir de una lista** con la lupa (o **F10** cliente, **F9** vendedor), de a uno o varios: así filtra **exacto** por esas personas. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Si venís de [Ventas](ayuda:pantalla/Ventas.Index) con personas elegidas, llegan ya cargadas, y las que elegís se conservan al tocar **Buscar**.
- **Ver detalles**: muestra en cada venta la fecha, el nro/comprobante, la sucursal, el cliente, el vendedor y la forma de pago.

## Descuento / recargo y anulaciones
Se aplican en el momento, junto con los demás filtros, y también en **PDF y Excel**.

**Descuento / recargo**
- **Con descuento** / **Con recargo**: líneas a las que se les aplicó un porcentaje.
- **Precio distinto al de lista**: sin porcentaje, pero cobradas a un precio distinto del de lista al grabarse la venta. Una venta modificada después de un cambio de precios puede aparecer acá sin que nadie haya tocado el precio.
- La columna **Desc./Rec.** muestra el valor (por ejemplo *Desc. 10 %* o *Lista - $ 489,40*).

**Anulaciones.** Un ítem anulado queda en dos líneas: la original (**Anulada**, tachada) y otra en negativo (**Anulación**). Si en una venta se anuló todo, lleva la marca **Venta cancelada**.
- **Todas**: todo, con las marcas.
- **Ventas con anulaciones**: la venta completa, para ver qué se sacó y qué se cobró. Los totales muestran lo realmente cobrado.
- **Solo lo anulado**: una fila por ítem anulado; los totales pasan a ser **Kg anulados**, **Pesos anulados**, **Ítems anulados** y **Ventas**.

Los filtros se combinan: si elegís *Con recargo* y esas líneas pertenecen a una venta anulada, el total puede dar cero porque cada línea se compensa con su anulación.

## Aviso de búsqueda pesada
Antes de cargar un período grande (desde **1.500 líneas**, configurable) aparece *Esta búsqueda puede demorar* con la cantidad: **Cambiar el período** no carga nada; **Buscar igual** carga, y la pantalla puede dejar de responder (esperá, sin cerrar ni recargar). Aparece al tocar *Buscar*, al cambiar la sucursal y al tocar *Volver* (que cuenta ventas). La cantidad no descuenta el filtro de Producto, Cliente ni Vendedor. Si no se puede contar, busca sin preguntar. Solo con base Postgres.

## Cómo se ven los datos
La fecha y la hora (en gris), la sucursal (un color por sucursal), el producto, la cantidad y el total van resaltados, con filas de color intercalado; las líneas anuladas tienen fondo rojo suave.

## Totales
Cuatro tarjetas: **Total Kg**, **Total pesos** (suma de las líneas que se ven), **Cantidad de líneas** y **Cantidad de registros** (ventas).

## Ver solo líneas
El botón **Ver solo líneas** (arriba a la derecha) cambia la pantalla de una tarjeta por venta a **una sola tabla con una fila por línea**; al tocarlo de nuevo (*Ver agrupado por venta*) volvés a las tarjetas.
- Cada fila trae: **Fecha y hora**, nro/comprobante, sucursal, cliente, vendedor, forma de pago, código, producto, cantidad/kg, precio, desc./rec., total de la línea, **Creado** (cuándo se cargó el registro, con segundos) y un ojo para **abrir la venta**.
- **Ordenar**: tocá el título de una columna (Fecha, Creado, Código, Producto, Cliente, etc.); un segundo toque invierte el orden. La flecha ▲▼ marca la columna activa. Al abrir la vista viene ordenada por **Fecha**, de la más reciente a la más vieja. Las líneas sin código (*-*) quedan siempre al final.
- Los **filtros en vivo y los totales** de arriba funcionan igual en las dos vistas. *Ver detalles* se oculta en la vista plana (ya se ven todos los datos).
- **PDF y Excel** no cambian: exportan lo mismo en cualquiera de las dos vistas.
- El navegador **recuerda la vista elegida** (para esta pantalla) al tocar *Buscar* o cambiar la sucursal.
- Con muchas columnas, en pantallas angostas la tabla se desplaza hacia los costados dentro de su recuadro.

## Elegir columnas
En **Ver solo líneas** aparece el botón **Columnas** (arriba a la derecha).
- **Destildá** una columna para ocultarla y volvé a tildarla para verla. **Producto** se ve siempre.
- **Control** deja de un toque *Fecha y hora*, *Código*, *Producto* y *Cantidad / kg*, juntas, más el ojo para abrir la venta. **Todas** vuelve a mostrar todo.
- El botón se pone azul y dice cuántas columnas hay ocultas.
- Se recuerda **para tu usuario en ese navegador**; en otra computadora se elige de nuevo.
- Ocultar no filtra: los filtros y los totales siguen igual (podés filtrar por vendedor con esa columna oculta) y **PDF y Excel** exportan todo.

## Exportar a PDF o Excel
1. Dejá la pantalla como la querés: sucursal, fechas (ya aplicadas con **Buscar**) y filtros.
2. Tocá **PDF** o **Excel**, arriba a la derecha, en la fila de botones (*Volver* queda a la izquierda).
3. Se descarga el archivo `Lineas_de_venta_AAAAMMDD_HHMM.pdf` o `.xlsx`. La pantalla no se recarga.

### Qué trae el archivo
- Un **corte por venta**: una banda con la fecha, el *Venta ID* y el **total de la venta**; una línea gris con **cliente, vendedor, forma de pago, sucursal y nro/comprobante**; y debajo sus líneas (código, producto, cantidad/kg, precio y total de la línea).
- Arriba, el **período**, la sucursal y los **filtros activos**. Al final, los **mismos totales** que las tarjetas de la pantalla (suma de las líneas incluidas).
- **PDF**: si una venta sigue en otra hoja, se repite su encabezado en la hoja siguiente.
- **Excel**: las cantidades y los importes son **números** (se pueden sumar y ordenar), no texto.

### Reglas
- El archivo respeta **lo que ves**: usa las fechas **ya aplicadas** (no las que cambiaste sin tocar *Buscar*) y los filtros de Producto, Cliente, Vendedor y Forma de pago.
- Con un filtro de producto, el total de cada venta sigue siendo el de **toda la venta** aunque se listen solo algunas líneas; el total general suma solo las líneas listadas.
- Si no hay ninguna venta visible, aparece el aviso *No hay ventas para exportar* y no se descarga nada.
- Las ventas **reservadas** que están ocultas para tu usuario tampoco salen en el archivo.
- No hay tope de filas: un período muy largo puede tardar en generarse.
- Si **escribís** un cliente, la pantalla busca por nombre y el archivo busca por nombre **o identificación**, así que un cliente escrito por CUIT/DNI puede traer ventas que la pantalla oculta. Si lo **elegís de la lista** (filtro exacto) pantalla y archivo coinciden, y el encabezado del archivo lista los nombres elegidos.

## Excel compacto para imprimir
Es otra versión del Excel pensada para **imprimir**: **sin colores ni fondos** (ahorra tinta), letra chica (8) y las líneas en **dos columnas por hoja**, así entran más del doble de líneas en cada hoja A4 vertical.

1. Dejá la pantalla como la querés (sucursal, fechas aplicadas y filtros) y tocá **Excel compacto**.
2. Se abre una ventana para elegir qué datos de la venta querés ver:
   - **En el renglón de la fecha**: *Vendedor* y *Sucursal*.
   - **En un renglón debajo (opcional)**: *Cliente*, *Forma de pago*, *Venta ID* y *Nro/comprobante*. Si no tildás ninguno de estos, ese renglón no se genera y entran más líneas por hoja.
3. Tocá **Descargar**. La próxima vez la ventana recuerda tu elección en ese navegador (de entrada vienen tildados *Vendedor* y *Cliente*).

Cómo queda cada venta, de arriba hacia abajo:
- **Renglón de la venta**: fecha y hora (y vendedor / sucursal si los elegiste); a la derecha, el **total de la venta**.
- **Renglón de datos extra** (si elegiste alguno).
- **Las líneas**: código, descripción, cantidad, precio y total de la línea.

Reglas:
- Las ventas van **de la más vieja a la más nueva** (la pantalla las muestra al revés).
- Una venta corta no se parte entre dos columnas; una muy larga sí, y en la columna siguiente se repite su renglón con *(continúa)*.
- Los títulos de columna y el período, la sucursal y los filtros se repiten en cada hoja, con *Página X de Y*. Al final va el total general.
- Un nombre de producto muy largo se recorta en el ancho de su columna.
- El total del renglón de la venta es el de **toda la venta**, aunque un filtro de producto muestre solo algunas líneas.
- No es para filtrar ni ordenar en Excel (dos bloques lado a lado no son una tabla): para eso usá el **Excel** con formato.
- PENDIENTE: cuántas líneas entran por hoja según tu impresora (se calculó para A4 vertical; no se probó imprimiendo).

## Notas
- PENDIENTE: qué permiso o rol muestra la entrada de esta pantalla en el menú (no verificado contra el código en esta revisión).

## En el celular
- **Filtros plegados (desde 2026-11-25):** arriba hay una barra **Filtros** con un resumen de lo elegido (sucursal, fechas, etc.). Tocala para abrir los filtros; van de a dos por renglón y **Buscar** a lo ancho. Los totales van de a dos.
- Cada línea se ve como una fila corta (código, producto y cantidad; debajo el resto), sin desplazarse de costado (desde 2026-11-25).
