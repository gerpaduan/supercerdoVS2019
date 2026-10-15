---
pantalla: Ventas.Lineas
titulo: Líneas de venta
rol: usuario
modulo: Ventas
orden: 3
permiso: PENDIENTE (la acción no valida un permiso propio; a los usuarios de producción les pide elegir el operador de Ventas antes de entrar)
revisada: 2026-10-14
---
## Para qué sirve
Ver **cada venta con sus líneas** (producto, cantidad en kilos, precio y total de cada línea) de un período, con una tarjeta por venta. Sirve para revisar qué se vendió producto por producto y para **exportarlo a PDF o Excel**.

## Filtros
- **Sucursal**: al cambiarla, la pantalla se recarga sola.
- **Fecha desde / Fecha hasta**: al cambiarlas aparece el aviso de que hay filtros sin aplicar; tocá **Buscar** para recargar los datos.
- **Producto** (código o descripción), **Cliente**, **Vendedor** y **Forma de pago** (Efectivo, Debito, Credito, CtaCte, Qr, Transferencia): se aplican en el momento sobre lo ya cargado, sin recargar, y los totales de arriba se recalculan.
- **Cliente** y **Vendedor** se pueden escribir (busca por letras) o **elegir de una lista** con la lupa (o **F10** cliente, **F9** vendedor), de a uno o varios: así filtra **exacto** por esas personas. Ver [cómo funciona el selector](ayuda:concepto/selector-cliente-vendedor-proveedor). Si venís de [Ventas](ayuda:pantalla/Ventas.Index) con personas elegidas, llegan ya cargadas, y las que elegís se conservan al tocar **Buscar**.
- **Ver detalles**: muestra en cada venta la fecha, el nro/comprobante, la sucursal, el cliente, el vendedor y la forma de pago.

## Totales
Cuatro tarjetas: **Total Kg**, **Total pesos** (suma de las líneas que se ven), **Cantidad de líneas** y **Cantidad de registros** (ventas).

## Ver solo líneas
El botón **Ver solo líneas** (debajo de los filtros) cambia la pantalla de una tarjeta por venta a **una sola tabla con una fila por línea**; al tocarlo de nuevo (*Ver agrupado por venta*) volvés a las tarjetas.
- Cada fila trae: **Fecha**, **Creado** (cuándo se cargó el registro, con segundos), nro/comprobante, sucursal, cliente, vendedor, forma de pago, código, producto, cantidad/kg, precio y total de la línea, y un ojo para **abrir la venta**.
- **Ordenar**: tocá el título de una columna (Fecha, Creado, Código, Producto, Cliente, etc.); un segundo toque invierte el orden. La flecha ▲▼ marca la columna activa. Al abrir la vista viene ordenada por **Fecha**, de la más reciente a la más vieja. Las líneas sin código (*-*) quedan siempre al final.
- Los **filtros en vivo y los totales** de arriba funcionan igual en las dos vistas. *Ver detalles* se oculta en la vista plana (ya se ven todos los datos).
- **PDF y Excel** no cambian: exportan lo mismo en cualquiera de las dos vistas.
- El navegador **recuerda la vista elegida** (para esta pantalla) al tocar *Buscar* o cambiar la sucursal.
- Con muchas columnas, en pantallas angostas la tabla se desplaza hacia los costados dentro de su recuadro.

## Exportar a PDF o Excel
1. Dejá la pantalla como la querés: sucursal, fechas (ya aplicadas con **Buscar**) y filtros.
2. Tocá **PDF** o **Excel**, arriba a la derecha, junto a *Volver al indice*.
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
