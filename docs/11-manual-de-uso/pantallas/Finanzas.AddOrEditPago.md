---
pantalla: Finanzas.AddOrEditPago
titulo: Pago / Cobro
rol: usuario
modulo: Finanzas
orden: 1
permiso: Sin permiso propio en el formulario; cambiar la persona requiere permiso de edición de pagos; eliminar requiere ese permiso o que el pago lo hayas cargado vos en tu caja abierta
revisada: 2026-10-22
---
## Para qué sirve
Registrar un **pago** (entregás dinero a una persona) o un **cobro** (recibís dinero de una persona) y dejar el movimiento en su cuenta corriente. Se abre desde la cuenta corriente de la persona (**Agregar Pago / Cobro**), desde el listado de Pagos o desde el POS.

## Cómo se usa
1. Elegí la operación: **Realizar un pago** (tecla **1**) o **Recibir un cobro** (tecla **2**). Hasta elegir, el resto del formulario está bloqueado.
2. Completá fecha, N° de recibo, forma de pago e importe. La sucursal viene por defecto la del usuario.
3. Tocá **Guardar** (Alt+Enter). Al guardar se abre una ventana para el comprobante (ver abajo).
4. Para **dejar la cuenta en cero**, en vez de los pasos 1 y 2 para la operación y el importe, activá el interruptor **Saldar cuenta** (ver abajo).
5. Un pago ya guardado se abre en **solo lectura** (fuera del POS): tocá **Modificar** para editarlo.

## Saldar cuenta
- Solo al cargar un pago o cobro **nuevo**. El interruptor está debajo de las tarjetas de operación y dice qué va a hacer: *Saldar cuenta — cobrar $ X* o *pagar $ X*.
- Al activarlo elige solo la operación según el saldo de la persona: **saldo negativo (la persona debe) = Cobro**; **saldo positivo (a favor de la persona) = Pago**. El **Importe** queda con el valor del saldo, y la operación y el importe quedan **bloqueados** (los atajos 1 y 2 y el botón de billetes tampoco los cambian). Fecha, recibo, forma de pago y observaciones se completan igual que siempre.
- Para pagar o cobrar **solo una parte**, dejá el interruptor apagado y cargá el importe a mano. Al apagarlo se desbloquea y el importe vuelve a 0.
- Aparece **deshabilitado** si la cuenta no tiene saldo (cero) y **no se puede usar con formas de pago con cheques** (el importe sale de los cheques); si lo tenías activo y cambiás a una forma con cheques, se apaga solo.
- No se ofrece en una cuenta corriente reservada que el usuario no puede ver (no se revela el saldo).
- Al guardar, el sistema **vuelve a calcular el saldo real**: si cambió desde que abriste la pantalla (por ejemplo otra caja registró una venta), el pago o cobro se guarda por el saldo actual y puede diferir de lo que viste. Si en el medio la cuenta quedó en cero, rechaza el guardado.

## Contador de billetes y conteo
- El botón de billetes junto al **Importe** (Alt+B) abre el contador. Al aceptar, el **total pasa al Importe** y el detalle del conteo (cuántos billetes de cada valor y las monedas) se guarda en el campo **Conteo de billetes**.
- **Conteo de billetes** es de **solo lectura**: no se escribe a mano, solo lo llena el contador, y **no se mezcla con Observaciones**. Cada vez que aceptás el contador el conteo se **reemplaza** por el nuevo (si el total es 0 queda vacío).
- **Limpiar contador** deja el conteo vacío (no toca el importe).
- El botón de billetes se oculta si la forma de pago incluye cheques.
- Cancelar o cerrar el contador sin aceptar no cambia nada.

## Al guardar: comprobante
Se muestra una ventana con cuatro opciones (teclas **1** a **4**):
1. **Cerrar sin imprimir.**
2. **Imprimir ticket térmico:** el ancho (**58 mm** u **80 mm**) se elige con los botones de al lado y se recuerda para la próxima vez.
3. **Generar PDF:** se abre el recibo en otra pestaña.
4. **Enviar por mail:** abre un formulario con el email de la persona (si lo tiene cargado), asunto y mensaje; el recibo se adjunta en PDF. **Cancelar** vuelve a las opciones.

El comprobante dice **Recibo de pago** o **Recibo de cobro** según la operación. En un **pago** la persona figura como quien **recibió** el dinero; en un **cobro**, como quien lo **entregó**. Las observaciones salen primero y, a continuación, el conteo de billetes si lo hay.

## Campos
- **Sucursal, Fecha, N° Recibo, Forma de pago, Importe, Observaciones.** Con *Efectivo + Cheques* se carga además el efectivo y los cheques.
- **Conteo de billetes** (solo lectura), descripto arriba; se ve al lado de Observaciones y ajusta su alto al contenido.
- **Sucursal:** dentro del POS no se elige: es siempre la de la caja del cajero y se muestra como texto bajo el nombre de la persona. Fuera del POS sí se puede elegir.

## Reglas y efectos
- El pago genera su asiento en la cuenta corriente y, desde el POS, el egreso de caja correspondiente. El conteo de billetes se copia también a ese egreso.
- **Al modificar un pago o cobro que tiene egreso de caja** (el que se hizo desde el POS) y cambiar el importe, la forma de pago, la fecha o la sucursal, la caja registra un movimiento **ANULACION** (el egreso original con signo contrario) y un egreso nuevo con el valor actual, los dos en la caja original. Si solo cambiás observaciones o el conteo, se actualiza el mismo egreso. Vale tanto si lo abrís desde la cuenta corriente en el POS como desde Finanzas. Los pagos cargados desde Finanzas (sin egreso) no mueven caja.
- Los pagos guardados **antes** de este cambio conservan el texto del conteo dentro de Observaciones; no se migra.
- El ticket se imprime directo si el agente de impresión está instalado; si no, se abre el diálogo de impresión del navegador.
- **Pago de una caja ya cerrada:** si el pago o cobro se hizo desde el POS y su egreso de caja está en una caja que **ya fue cerrada**, al modificarlo (importe, forma de pago, fecha, sucursal o persona) o **eliminarlo** aparece el aviso *"Esta caja ya fue cerrada"* con el detalle y hay que confirmar. Esa caja queda marcada con un aviso de que se modificaron datos (ver [Cajas abiertas](ayuda:pantalla/Cajas.CajasAbiertas)). Al eliminar, el egreso opuesto se registra con la fecha de hoy en la caja actual de quien elimina, no en la caja original. Los pagos cargados desde Finanzas (sin egreso de caja) no entran en ningún cierre y no avisan.
- **Eliminar un pago o cobro:** aparece el botón **Eliminar** si tenés el permiso de edición de pagos **o** si el pago/cobro lo cargaste vos desde que abriste tu caja actual (misma sucursal, caja todavía abierta). Pide un motivo, genera el asiento opuesto en la cuenta corriente y queda visible como ELIMINADO. Para pagos de otros cajeros, de cajas anteriores o sin que tengas caja abierta hace falta el permiso. **Cambiar la persona** sigue exigiendo el permiso.
- **PENDIENTE:** impresión física en la térmica y envío real de mail no verificados.

Ver también: [mapa de configuraciones](ayuda:referencia/configuraciones)
