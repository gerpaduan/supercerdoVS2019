---
pantalla: Movimientos.Lineas
titulo: Líneas de movimiento
rol: usuario
modulo: Movimientos
orden: 3
permiso: PENDIENTE (no se revisó el permiso exacto para entrar)
revisada: 2026-11-11
---
## Para qué sirve
Ver **cada movimiento entre sucursales con sus líneas** (producto, cantidad, unidades y si se pesó en balanza), con una tarjeta por movimiento, de un período. Se llega desde [Movimientos](ayuda:pantalla/Movimientos.Index) con **Ver lineas**, que lleva las sucursales y fechas ya aplicadas.

## Arriba de la pantalla
- El título **Líneas de movimiento** figura solo en la barra superior (ya no se repite como subtítulo).
- **Volver** (botón chico, a la derecha) regresa a Movimientos con las mismas sucursales y fechas.

## Filtros
- **Sucursal origen / destino**: al cambiarlas la pantalla se recarga sola. **Fecha desde / hasta**: al cambiarlas aparece el aviso; tocá **Buscar** para recargar.
- **Producto** (código o descripción) y **Estado**: se aplican en el momento sobre lo ya cargado y los totales de arriba se recalculan.
- **Ver detalles**: muestra en cada movimiento la fecha, las sucursales, el estado, el usuario y las observaciones.

## Líneas
Cada tarjeta lista las líneas con: **Código producto**, **Producto**, **Cantidad / kg**, **Total unidades**, **Balanza** (*Sí* si la línea se cargó con el peso de la balanza, *No* si no) y **Observación línea** (*Permite ingreso* si el operador forzó la línea aunque los kilos no coincidían con las unidades; *-* si no). El peso por balanza ya no figura dentro de la observación: tiene su propia columna.

## Ver solo líneas (2026-11-11)
El botón **Ver solo líneas** (debajo de los filtros, junto a *Ver detalles*) cambia la pantalla de una tarjeta por movimiento a **una sola tabla con una fila por línea**; al tocarlo de nuevo (*Ver agrupado por movimiento*) se vuelve a las tarjetas. Es el mismo botón que en [Líneas de compra](ayuda:pantalla/Compras.Lineas).
- Cada fila trae: **Fecha** del movimiento, **Sucursal origen**, **Sucursal destino**, **Estado**, **Usuario**, código, producto, cantidad/kg, total de unidades, **Balanza**, observación de la línea y un ojo para **abrir el movimiento**. Las observaciones del movimiento no se muestran en esta tabla (se ven en el movimiento).
- **Ordenar**: tocá el título de una columna; un segundo toque invierte el orden. Al abrir la vista viene ordenada por **Fecha**, de la más reciente a la más vieja.
- Los **filtros en vivo y los totales** funcionan igual en las dos vistas. *Ver detalles* se oculta en la vista plana.
- El navegador **recuerda la vista elegida** al tocar *Buscar* o cambiar una sucursal.

## Totales
Cuatro tarjetas: **Total Kg**, **Total unidades**, **Cantidad de líneas** y **Cantidad de registros** (movimientos), calculadas con lo que está visible.

## Verificado y pendiente
- Verificado el 2026-11-11 con un usuario de prueba en la base local: sin subtítulo, botón *Volver* chico, columna *Balanza*, cambio entre las dos vistas, orden por cantidad, filtro en vivo por producto y totales.
- PENDIENTE: no se probó con una línea cargada en balanza (los datos de prueba no tenían ninguna), en modo oscuro ni en celular.
