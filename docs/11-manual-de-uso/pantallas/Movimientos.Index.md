---
pantalla: Movimientos.Index
titulo: Movimientos
rol: usuario
modulo: Movimientos
orden: 1
permiso: PENDIENTE
revisada: 2026-11-13
---
## Para qué sirve
Ver los **movimientos de mercadería entre sucursales** de un período (origen, destino, totales) y abrir el detalle de cada uno.

## Filtros
- En la **computadora** todos los filtros van en **una sola fila**, con las fechas primero: **Fecha desde**, **Fecha hasta**, **Sucursal origen**, **Sucursal destino**, **Expandir todos** y **Buscar**. Así queda más lugar para la lista.
- **Fecha desde / Fecha hasta**: al cambiarlas aparece el aviso; tocá **Buscar** para recargar. En la computadora el aviso es el texto *Cambiaste las fechas: tocá Buscar para actualizar.* de la barra de abajo y el botón **Buscar** se pone **amarillo**; en el celular sigue siendo la alerta amarilla.
- **Expandir todos** (*Ver detalles*): al buscar, la lista viene con el detalle de todos los movimientos desplegado.

## Botones
- Debajo de los filtros, en una misma barra con el texto de estado (*Resultados actualizados.*): **Nuevo movimiento** (Alt+N), **Ver lineas** (Alt+V; lleva las sucursales y fechas ya aplicadas) y **Ver eliminados** (solo para quien puede verlos; lleva a [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index) de movimientos). Si la ventana es angosta, el texto de estado inactivo se oculta.
- En cada fila: **Ver** (abre el movimiento), **Imprimir** y **Detalle** (despliega las líneas debajo de la fila).
- El **detalle** lista cada línea con *Código*, *Producto*, *Cantidad*, *Kilos*, *Balanza*, *Permitir ingreso* y, desde el 2026-11-11, **Hora de carga**: la **fecha y hora** en que se cargó esa línea al movimiento (en el celular, solo la hora con un reloj; si se cargó otro día que el guardado, con la fecha y en ámbar). Las líneas de movimientos anteriores a ese cambio muestran «-» (o no muestran hora en el celular). Ver [Movimientos](ayuda:pantalla/Movimientos.Editar) y [Líneas de movimiento](ayuda:pantalla/Movimientos.Lineas).

## Lista
Columnas: *Fecha movimiento*, *Origen*, *De origen* y *Estado* (solo donde corresponde), *Destino*, *Total unidad*, *Total kilos*, *Obs.* (una marca si tiene observaciones), *Creado* (fecha, hora y quién; con *Modificado* si se cambió) y *Acciones*.

## Notas
- PENDIENTE: el permiso exacto para entrar y para ver eliminados, y el detalle de **Imprimir**, no se revisaron en esta versión.

## Lista y detalle en la computadora (desde el 2026-11-12)
- **Columnas compactas**: la tabla ya no se estira al ancho de la pantalla; las columnas quedan juntas (*Fecha*, *Origen*, *Destino* y los totales) para no mover la vista de un lado a otro. Si un texto no entra en su columna, baja al renglón siguiente.
- **Fecha**, **Origen** y **Destino** se ven en negrita. El **Origen** va en **rojo** con una flecha de salida (de ahí sale la mercadería) y el **Destino** en **verde** con una flecha de entrada (ahí ingresa). Los *Total kilos* también van en negrita.
- **Detalle** abre arriba una franja con el número, *Sale de* (rojo) → *Entra a* (verde), quién lo creó y, si se cambió, quién lo modificó. Las observaciones salen en un recuadro amarillo.
- Debajo, las líneas con *Cód.* (15 caracteres), *Producto* (25), *Unidades* (8), *Kilos* (8), los iconos de **balanza** y de **ingreso permitido** y la hora de carga; al pie, el **Total** de unidades y kilos.

## En una ventana angosta con mouse (desde el 2026-11-13)
- Si usás la computadora con el navegador o la ventana independiente **angostos** (menos de 1200 píxeles de ancho), los botones y filtros ocupan **poca altura** para dejar lugar a la lista: *Nuevo movimiento*, *Ver lineas* y *Ver eliminados* quedan en un solo renglón; *Sucursal origen* y *destino* en otro; y *Fecha desde*, *Fecha hasta*, *Expandir todos* y *Buscar* en el siguiente (si el ancho no alcanza, el último baja de renglón). La etiqueta de cada campo va a su izquierda.
- El aviso «Cambiaste las fechas: tocá Buscar para actualizar.» aparece solo cuando hay fechas sin aplicar (y *Buscar* se pone ámbar). Los atajos Alt+N y Alt+V siguen igual.
- En celular y tablet (pantalla táctil) no cambia nada: se ve como en la sección siguiente.

## En el celular
- En pantallas angostas (celular y tablet) la pantalla se ve como antes: botones arriba, filtros apilados y el estado (*Resultados actualizados.*) debajo del recuadro de filtros. Cada movimiento se ve como **tarjeta**: la fecha arriba (con el número de movimiento) y **Ver**, **Imprimir** y **Detalle** al pie.
