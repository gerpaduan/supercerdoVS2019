---
pantalla: Stock.Lineas
titulo: Líneas de stock
rol: usuario
modulo: Stock
orden: 4
permiso: PENDIENTE (no verificado contra el código en esta revisión)
revisada: 2026-11-02
---
## Para qué sirve
Ver los **movimientos de stock con sus líneas** (producto y cantidad de cada ingreso, egreso, cierre, pesaje o ajuste), por sucursal y fechas. Se llega desde [Stock](ayuda:pantalla/Stock.Index) con **Ver lineas**, que lleva los filtros ya aplicados.

## Filtros
- En la **computadora** (pantalla de 1200 px o más) los filtros van en **una sola fila**: **Fecha desde**, **Fecha hasta** (con hora y segundos), **Sucursal**, **Producto**, **Tipo de operación** y **Buscar**. Debajo, en una barra, van los **cuatro totales**, el texto de estado, **Ver detalles** y **Volver al índice**. En pantallas angostas se ven apilados, como antes.
- Ya no hay un título *Lineas de stock* en la pantalla (lo muestra la barra superior de la aplicación).
- **Fecha desde**, **Fecha hasta** y **Sucursal** consultan de nuevo: la sucursal recarga sola; las fechas esperan a **Buscar**. Mientras hay fechas sin aplicar, en la computadora el botón **Buscar** se pone **amarillo** y el texto de estado dice *Cambiaste las fechas: tocá Buscar para actualizar.*; en pantallas angostas aparece la alerta amarilla.
- **Producto** (código o descripción) y **Tipo de operación** (Ingreso, Egreso, Cierre, Pesaje, Ajuste) **filtran en vivo** lo que ya está cargado, sin consultar de nuevo.

## Totales y lista
- Los totales (**Total Kg**, **Total pesos**, **Cantidad de lineas**, **Cantidad de registros**) siguen el filtro en vivo.
- Cada movimiento es una tarjeta con sus líneas; **Ver detalles** muestra los datos extra de todos a la vez y **Ver registro** abre el movimiento.

## Resto de la pantalla
PENDIENTE: documentar permisos, de dónde sale cada total y qué significa *Tipo / signo* (no verificado contra el código en esta revisión).
