---
pantalla: Cajas.EgresosCaja
titulo: Egresos de caja
rol: usuario
modulo: Caja
orden: 1
permiso: EgresoCaja.VerEgresosCaja (ver el listado) / EgresoCaja.AddOrEditEgresoCaja (cargar). Desde el POS no pide el permiso de carga
revisada: 2026-11-16
---

## Para qué sirve
Registrar la **salida de dinero de la caja** (gastos, retiros, etc.) y consultarlas. El listado se filtra por sucursal, usuario, tipo, descripción y fechas. El formulario de carga también se abre desde el POS y desde las actividades de una caja.

## Cómo se usa
1. Tocá para crear un egreso nuevo (o **Modificar** en uno existente: se abre en solo lectura).
2. Completá **Sucursal, Fecha, Tipo, Descripción y Monto**. **Detalle** es opcional (se activa con el interruptor). Al abrir un egreso nuevo el cursor arranca en **Sucursal** (si el formulario la muestra; en el POS y en el egreso de una caja puntual no hay selector y arranca en Fecha o Tipo). **Enter** pasa al campo siguiente en ese orden (Sucursal, Fecha, Tipo, Descripción, Monto, Detalle); en **Tipo** también, después de elegir la opción.
3. Guardá con **Guardar egreso** (Alt+Enter). Al guardar se abre una ventana para el comprobante (ver abajo). Mientras se guarda, el botón queda deshabilitado: un doble clic o un Enter repetido no carga el egreso dos veces (lo mismo en **Calcular comisiones electrónicas**).
4. **Cargar en Lote** (interruptor a la izquierda de **Guardar egreso**, solo en un egreso nuevo): con el interruptor prendido, al guardar **no** se abre el comprobante ni se cierra la ventana; aparece un aviso corto («Egreso guardado») y vuelve el formulario vacío para el próximo, con la **fecha y hora del último egreso cargado** y el cursor en **Sucursal** (si el formulario la muestra) o, si no, en **Fecha**. Para terminar, cerrá la ventana. Los egresos cargados en lote se pueden imprimir después desde el listado. El interruptor vuelve a quedar apagado cada vez que abrís **Nuevo Egreso**. **PENDIENTE:** el recorrido completo no se probó todavía en el navegador.
5. El formulario de nuevo egreso y el de **Calcular comisiones electrónicas** usan letra y campos compactos (mismo tamaño que el resto de las pantallas).
6. **Calcular comisiones electrónicas** tiene dos filas de campos: **Sucursal, Desde, Hasta y Fecha del egreso**; y debajo **Tipo de egreso, % para todas, Aplicar y Recalcular**. Si tu usuario tiene sucursal fija, el campo Sucursal muestra el nombre sin desplegable. **PENDIENTE:** el aspecto en pantalla no se verificó todavía en el navegador.

## Con la cuenta de producción (solo ver)
- Si entrás con la **cuenta compartida de producción**, al abrir **Egresos de caja** se pide **autorizarte**: elegís tu usuario y escribís tu contraseña. Tu usuario tiene que tener el permiso **Ver egresos de caja**; si no, aparece *«Ese usuario no tiene permiso para ver egresos de caja»*. Si te equivocás de contraseña varias veces, se bloquea por un rato.
- Una vez adentro es **solo consulta**: se ve el listado y se puede filtrar, desplegar el detalle e **imprimir el comprobante**, pero **no** están **Nuevo Egreso**, **Calcular comisiones electrónicas**, **Tipos Egresos** ni **Modificar**. Arriba aparece *«Consulta como (tu nombre)»*. Los límites de días de tu permiso se aplican a las fechas que puedas ver.
- **La autorización termina apenas cambiás de pantalla** (cualquier otra, incluso *Cajas abiertas*); para volver hay que autorizarse de nuevo. Si la sesión se vence con la pantalla abierta, al buscar aparece un aviso y la página se recarga para pedir la autorización otra vez.
- Un usuario común (no la cuenta de producción) entra directo, como siempre.
- **PENDIENTE:** el recorrido completo (login, tabla y caducidad al navegar) no se probó todavía en el navegador.

## En la computadora (pantalla de 1200 px o más)
Modo compacto del listado de **Egresos de caja** (el que se abre desde el menú, no el del POS ni *Mis actividades*):
- **Filtros arriba, con las fechas primero** (Desde, Hasta, Sucursal, Usuario, Tipo, Descripción, Gasto y Buscar). En ventanas de menos de ~1550 px van en **dos filas**; desde ~1550 px, en **una sola**. La fecha se ve siempre completa, con hora y segundos.
- **Una sola barra debajo de los filtros** con **Items y Total** (se recalculan al buscar), el estado de la búsqueda, **Vista completa** y los botones **Nuevo Egreso**, **Calcular comisiones electrónicas** y **Tipos Egresos**. En ventanas de 1200 a 1400 px el texto «Resultados actualizados» no se muestra para dejar lugar a los botones.
- **Si cambiás las fechas y todavía no buscaste**, no aparece la alerta amarilla de siempre: **Buscar se pone amarillo** y la barra dice *«Cambió la fecha: presioná Buscar para actualizar»*. Al buscar vuelve a lo normal.
- **Tabla:** una línea por fila (la Descripción baja de renglón solo si es larga). El **Monto** se ve con la **misma letra que las demás columnas** (antes iba más grande y en negrita), en todos los anchos. En **Creado** la fecha y el usuario van en la misma línea (el nombre se corta si es largo; el nombre completo está en la fila desplegable).
- El **Detalle** de la tabla muestra la primera línea cortada a ~9 rem (el texto completo está en la fila desplegable).
- Con una ventana de **1200 px** la tabla puede tener scroll horizontal propio (no entran las 8 columnas con el menú lateral abierto).
- **Celular y tablet (menos de 1200 px):** se ve como antes (en el celular, el texto de estado queda debajo de los filtros). La tabla sigue igual entre 768 y 1199 px; por debajo de 768 px se ve como tarjetas (ver abajo).

## En ventana angosta (menos de 1200 px)
Desde el 2026-11-16, igual que **Movimientos**, **Stock** y **Elaborados**:
- **Computadora con mouse y ventana de menos de 1200 px:** los botones **Nuevo Egreso**, **Calcular comisiones electrónicas** y **Tipos Egresos** van en **un solo renglón**, y los filtros en **tres renglones compactos** con la etiqueta a la izquierda de cada campo: *Desde / Hasta*, *Sucursal / Usuario / Tipo* y *Descripción / Gasto / Buscar*. Items, Total y **Vista completa** quedan en una línea. Si cambiás las fechas y todavía no buscaste, **Buscar se pone amarillo** y aparece el aviso de siempre en su propio renglón (no la alerta grande). En un celular o una tablet los filtros siguen como antes.
- **Menos de 768 px (celular o ventana muy angosta):** cada egreso del listado es una **tarjeta** con el mismo estilo que Movimientos: arriba la **fecha** y, a la derecha en gris, el **número de egreso**; abajo **Tipo y Monto**, la **Descripción**, el **Detalle** (si tiene), **Sucursal y Creado** (con la etiqueta **Modificado** si corresponde) y una fila de botones **Detalle**, **Imprimir** y **Modificar** (este último solo si se puede). **Detalle** despliega los datos completos debajo de la tarjeta. **Vista completa** los despliega todos. *Mis actividades* y *Actividades* del POS conservan sus tarjetas de siempre.
- Verificado el 2026-11-16 en el navegador con sesión real: filtros y tarjetas a 1000, 600 y 1500 px, **Detalle** y el filtro **Gasto** en tarjeta. **PENDIENTE:** modo oscuro y celular real (el panel emula táctil por debajo de 768 px, así que los renglones compactos de los filtros se vieron solo a 1000 px).

## Ver los pagos electrónicos (control)
- «Todos» **no** lista los pagos electrónicos ni la cuenta corriente: son movimientos que genera el sistema con cada venta y ensuciarían el listado.
- Para verlos, elegí **Pago Electronico** en **Tipo** y buscá. Muestra un renglón por venta cobrada con débito, crédito, QR o transferencia (la parte en efectivo de un pago mixto no entra).
- **Rango máximo: 31 días.** Si elegís más, aparece un aviso amarillo y no se muestra nada hasta que achiques las fechas. Para ver solo la cantidad: **Items** y **Total** de la barra.
- No se modifican desde acá (se cambian desde Ventas). Los que no tienen venta asociada (cargados a mano en el pasado) también se ven y tampoco se pueden editar.

## Columna Creado
- Antes de **Acciones** (en el listado, en **Mis actividades** y en **Actividades**) hay una columna **Creado**: fecha y hora en que se cargó el egreso y, debajo en gris, quién lo cargó (en el listado de escritorio compacto, en la misma línea).
- Si el egreso se **modificó**, aparece la etiqueta amarilla **Modificado** con la fecha y hora de la modificación y debajo el usuario que lo modificó. No se marca en los movimientos que el sistema actualiza solo (Pago | Cobro, Cta Cte, Pago electrónico) ni en las anulaciones.

## Mis actividades y Actividades (POS y Cerrar caja)
- La tabla **no muestra la columna Detalle**: si el registro tiene detalle (o conteo de billetes) aparece la etiqueta **Con detalle** bajo la descripción y el texto se ve **desplegando la fila** (flecha o doble clic). El listado completo de Egresos de caja sí conserva la columna.
- **Venta vinculada** (desde el 2026-11-14): las filas **Pago Electronico** y **Cta Cte** de una venta muestran en Descripción solo el nombre de la venta (**Venta Mixta**, **Venta Debito**, **Venta Credito**…; en cuenta corriente, **Venta Cta Cte · nombre del cliente**), sin importes ni número. El botón **Ver detalle** (una píldora con una flecha que gira) pasa a **Ocultar detalle** al abrir. Al **desplegar la fila** aparece el recuadro **Venta vinculada #N** (en cuenta corriente, con el **Cliente**: la razón social y, en gris y más chico, el nombre corto si es distinto) con los productos, la **forma de pago** (si fue mixta, cuánto en efectivo y cuánto en el otro medio), el total y los botones **Modificar venta**, **Cambiar forma de pago** (solo si corresponden) y **Abrir detalle**. La venta se carga recién al desplegar. En esas filas **no se muestra** el texto automático «Detalle: | Kgs | Precio | TOT» (lo reemplaza el recuadro de la venta, que ya trae los productos y el comentario). Si la venta no se puede leer, el recuadro muestra el motivo. No aparece en egresos de otro tipo ni con el motor SQL Server. (Verificado contra el código 2026-11-14; PENDIENTE probarlo en pantalla.)
- **Compras eliminadas:** al eliminar una compra hecha desde el POS aparece un egreso **ELIMINADO: …** (importe en negativo, o $ 0,00 si era en cuenta corriente). El egreso original queda con el aviso «Compra eliminada: ver el egreso ELIMINADO» y sin botón de modificar. En **Mis actividades** del POS las compras propias de la caja abierta tienen **Modificar** (ver [Ventas.POS](ayuda:pantalla/Ventas.POS)).
- **Actividades** (desde Cajas abiertas / Cerrar caja) usa el mismo formato compacto que **Mis actividades** del POS, para que Acciones y el resto de las columnas entren en pantalla.
- **Mi cierre** (solo en **Mis actividades** del POS, con tu caja abierta): abre una ventana para **contar tu caja** con el contador de billetes y dejar cargado el efectivo que tenés. Guardá con **Guardar mi cierre**. Al guardar aparece el aviso **"Se guardó correctamente el conteo de tu cierre de caja"** con las opciones de comprobante (cerrar, ticket térmico, PDF o mail) y después se cierra la ventana. El encargado lo ve al cerrar tu caja (ver [Cajas abiertas](ayuda:pantalla/Cajas.CajasAbiertas)). No es un egreso: no suma a los egresos ni cambia la diferencia. Solo disponible en empresas con base Postgres.
- **Modo actualizar:** si ya cargaste tu cierre, el botón pasa a **Actualizar mi cierre** (amarillo) y la ventana dice **Modo actualizar** con el importe y la hora de lo que cargaste antes. Lo que guardes lo reemplaza, y podés hacerlo hasta que el encargado cierre la caja.
- **Si cambiás el importe a mano** y ya hay un conteo de billetes que no coincide con ese importe, la ventana avisa en rojo que **el conteo de billetes se va a borrar**. Para guardar así hay que tocar de nuevo el botón (**Confirmar: guardar y borrar el conteo**). Si volvés a poner el importe del conteo, el aviso desaparece.

## Contador de billetes y conteo
- El botón de billetes junto al **Monto** (Alt+B) abre el contador. Al aceptar, el **total pasa al Monto** y el detalle del conteo se guarda en el campo **Conteo de billetes**.
- **Conteo de billetes** es de **solo lectura** y **no se mezcla con el Detalle**. Mientras está **vacío** aparece **plegado** (una línea con la etiqueta *vacío*); si tiene datos —al aceptar el contador, o en un egreso que ya lo trae— se **despliega solo**. Se puede abrir o cerrar a mano tocando el encabezado; **Limpiar contador** (visible con el conteo desplegado) lo vacía y lo vuelve a plegar. Cada aceptación del contador **reemplaza** el conteo anterior (si el total es 0 queda vacío).
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
