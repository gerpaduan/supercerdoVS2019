---
pantalla: Cajas.EgresosCaja
titulo: Egresos de caja
rol: usuario
modulo: Caja
orden: 1
permiso: EgresoCaja.VerEgresosCaja (ver el listado) / EgresoCaja.AddOrEditEgresoCaja (cargar). Desde el POS no pide el permiso de carga
revisada: 2026-10-31
---

## Para qué sirve
Registrar la **salida de dinero de la caja** (gastos, retiros, etc.) y consultarlas. El listado se filtra por sucursal, usuario, tipo, descripción y fechas. El formulario de carga también se abre desde el POS y desde las actividades de una caja.

## Cómo se usa
1. Tocá para crear un egreso nuevo (o **Modificar** en uno existente: se abre en solo lectura).
2. Completá **Sucursal, Fecha, Tipo, Descripción y Monto**. **Detalle** es opcional (se activa con el interruptor).
3. Guardá con **Guardar egreso** (Alt+Enter). Al guardar se abre una ventana para el comprobante (ver abajo).

## En la computadora (pantalla de 1200 px o más)
Modo compacto del listado de **Egresos de caja** (el que se abre desde el menú, no el del POS ni *Mis actividades*):
- **Filtros arriba, con las fechas primero** (Desde, Hasta, Sucursal, Usuario, Tipo, Descripción, Gasto y Buscar). En ventanas de menos de ~1550 px van en **dos filas**; desde ~1550 px, en **una sola**. La fecha se ve siempre completa, con hora y segundos.
- **Una sola barra debajo de los filtros** con **Items y Total** (se recalculan al buscar), el estado de la búsqueda, **Vista completa** y los botones **Nuevo Egreso**, **Calcular comisiones electrónicas** y **Tipos Egresos**. En ventanas de 1200 a 1400 px el texto «Resultados actualizados» no se muestra para dejar lugar a los botones.
- **Si cambiás las fechas y todavía no buscaste**, no aparece la alerta amarilla de siempre: **Buscar se pone amarillo** y la barra dice *«Cambió la fecha: presioná Buscar para actualizar»*. Al buscar vuelve a lo normal.
- **Tabla:** una línea por fila (la Descripción baja de renglón solo si es larga). El **Monto** se ve con la **misma letra que las demás columnas** (antes iba más grande y en negrita), en todos los anchos. En **Creado** la fecha y el usuario van en la misma línea (el nombre se corta si es largo; el nombre completo está en la fila desplegable).
- El **Detalle** de la tabla muestra la primera línea cortada a ~9 rem (el texto completo está en la fila desplegable).
- Con una ventana de **1200 px** la tabla puede tener scroll horizontal propio (no entran las 8 columnas con el menú lateral abierto).
- **Celular y tablet (menos de 1200 px):** se ve como antes (en el celular, el texto de estado queda debajo de los filtros).

## Columna Creado
- Antes de **Acciones** (en el listado, en **Mis actividades** y en **Actividades**) hay una columna **Creado**: fecha y hora en que se cargó el egreso y, debajo en gris, quién lo cargó (en el listado de escritorio compacto, en la misma línea).
- Si el egreso se **modificó**, aparece la etiqueta amarilla **Modificado** con la fecha y hora de la modificación y debajo el usuario que lo modificó. No se marca en los movimientos que el sistema actualiza solo (Pago | Cobro, Cta Cte, Pago electrónico) ni en las anulaciones.

## Mis actividades y Actividades (POS y Cerrar caja)
- La tabla **no muestra la columna Detalle**: si el registro tiene detalle (o conteo de billetes) aparece la etiqueta **Con detalle** bajo la descripción y el texto se ve **desplegando la fila** (flecha o doble clic). El listado completo de Egresos de caja sí conserva la columna.
- **Compras eliminadas:** al eliminar una compra hecha desde el POS aparece un egreso **ELIMINADO: …** (importe en negativo, o $ 0,00 si era en cuenta corriente). El egreso original queda con el aviso «Compra eliminada: ver el egreso ELIMINADO» y sin botón de modificar. En **Mis actividades** del POS las compras propias de la caja abierta tienen **Modificar** (ver [Ventas.POS](ayuda:pantalla/Ventas.POS)).
- **Actividades** (desde Cajas abiertas / Cerrar caja) usa el mismo formato compacto que **Mis actividades** del POS, para que Acciones y el resto de las columnas entren en pantalla.
- **Mi cierre** (solo en **Mis actividades** del POS, con tu caja abierta): abre una ventana para **contar tu caja** con el contador de billetes y dejar cargado el efectivo que tenés. Guardá con **Guardar mi cierre**. Al guardar aparece el aviso **"Se guardó correctamente el conteo de tu cierre de caja"** con las opciones de comprobante (cerrar, ticket térmico, PDF o mail) y después se cierra la ventana. El encargado lo ve al cerrar tu caja (ver [Cajas abiertas](ayuda:pantalla/Cajas.CajasAbiertas)). No es un egreso: no suma a los egresos ni cambia la diferencia. Solo disponible en empresas con base Postgres.
- **Modo actualizar:** si ya cargaste tu cierre, el botón pasa a **Actualizar mi cierre** (amarillo) y la ventana dice **Modo actualizar** con el importe y la hora de lo que cargaste antes. Lo que guardes lo reemplaza, y podés hacerlo hasta que el encargado cierre la caja.
- **Si cambiás el importe a mano** y ya hay un conteo de billetes que no coincide con ese importe, la ventana avisa en rojo que **el conteo de billetes se va a borrar**. Para guardar así hay que tocar de nuevo el botón (**Confirmar: guardar y borrar el conteo**). Si volvés a poner el importe del conteo, el aviso desaparece.

## Contador de billetes y conteo
- El botón de billetes junto al **Monto** (Alt+B) abre el contador. Al aceptar, el **total pasa al Monto** y el detalle del conteo se guarda en el campo **Conteo de billetes**.
- **Conteo de billetes** es de **solo lectura**, está siempre visible y **no se mezcla con el Detalle**. Cada aceptación del contador **reemplaza** el conteo anterior (si el total es 0 queda vacío).
- **Limpiar contador** deja el conteo vacío (no toca el monto).
- Cancelar o cerrar el contador sin aceptar no cambia nada.
- En el listado, el conteo se ve en la fila desplegable (flecha) de cada egreso. La letra del conteo es 2 pt más chica que la del resto (en el formulario y en el listado).

## Al guardar: comprobante
Se muestra una ventana con cuatro opciones (teclas **1** a **4**):
1. **Cerrar sin imprimir.**
2. **Imprimir ticket térmico:** **58 mm** u **80 mm** (se recuerda el último).
3. **Generar PDF:** se abre en otra pestaña.
4. **Enviar por mail:** el egreso no tiene persona asociada, así que el **email destino se escribe a mano**; asunto y mensaje vienen precargados y se adjunta el PDF.

**Reimprimir un egreso ya guardado:** con el egreso abierto en **modo vista** hay un botón **Imprimir** junto a **Modificar** (desaparece al activar la modificación, porque al guardar se ofrece de nuevo el comprobante). También en cada fila del listado (y en cada tarjeta en pantallas angostas) el botón de la impresora abre esta misma ventana (cerrar / ticket / PDF / mail).

El ticket respeta el que ya imprimía el sistema de escritorio: *Egreso Caja*, sucursal, vendedor, id, fecha, **tipo**, descripción, **monto**, detalle, el **conteo de efectivo** (nuevo, después del detalle), y fechas de creación / modificación. A diferencia del sistema de escritorio, el detalle sale completo (allá se cortaba en 5 renglones).

## Reglas y efectos
- Algunos egresos los genera el sistema (por un pago, una compra o una venta) y tienen reglas propias de modificación; **PENDIENTE** detallarlas en esta ayuda.
- Los egresos guardados **antes** de este cambio conservan el texto del conteo dentro del Detalle; no se migra.
- **Egreso de una caja ya cerrada:** si cargás un egreso con una fecha que cae dentro de una caja que **ya fue cerrada**, o modificás (monto, tipo, fecha o sucursal) uno que está en una caja cerrada, aparece el aviso *"Esta caja ya fue cerrada… el cierre va a quedar inconsistente"* y tenés que confirmar. Queda un aviso en el cierre de esa caja (ver [Cajas abiertas](ayuda:pantalla/Cajas.CajasAbiertas)). Desde el POS no pasa: ahí la fecha tiene que caer en la caja abierta.
- **PENDIENTE:** impresión física en la térmica y envío real de mail no verificados.

Ver también: [mapa de configuraciones](ayuda:referencia/configuraciones)
