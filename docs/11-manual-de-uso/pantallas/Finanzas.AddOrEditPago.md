---
pantalla: Finanzas.AddOrEditPago
titulo: Pago / Cobro
rol: usuario
modulo: Finanzas
orden: 1
permiso: Sin permiso propio en el formulario; cambiar la persona o eliminar requiere permiso de edición de pagos
revisada: 2026-10-07
---
## Para qué sirve
Registrar un **pago** (entregás dinero a una persona) o un **cobro** (recibís dinero de una persona) y dejar el movimiento en su cuenta corriente. Se abre desde la cuenta corriente de la persona (**Agregar Pago / Cobro**), desde el listado de Pagos o desde el POS.

## Cómo se usa
1. Elegí la operación: **Realizar un pago** (tecla **1**) o **Recibir un cobro** (tecla **2**). Hasta elegir, el resto del formulario está bloqueado.
2. Completá fecha, N° de recibo, forma de pago e importe. La sucursal viene por defecto la del usuario.
3. Tocá **Guardar** (Alt+Enter). Al guardar se abre una ventana para el comprobante (ver abajo).
4. Un pago ya guardado se abre en **solo lectura** (fuera del POS): tocá **Modificar** para editarlo.

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
- Los pagos guardados **antes** de este cambio conservan el texto del conteo dentro de Observaciones; no se migra.
- El ticket se imprime directo si el agente de impresión está instalado; si no, se abre el diálogo de impresión del navegador.
- **Pago de una caja ya cerrada:** si el pago o cobro se hizo desde el POS y su egreso de caja está en una caja que **ya fue cerrada**, al modificarlo (importe, forma de pago, fecha, sucursal o persona) o **eliminarlo** aparece el aviso *"Esta caja ya fue cerrada"* con el detalle y hay que confirmar. Esa caja queda marcada con un aviso de que se modificaron datos (ver [Cajas abiertas](ayuda:pantalla/Cajas.CajasAbiertas)). Al eliminar, el egreso opuesto se registra con la fecha de hoy en la caja actual de quien elimina, no en la caja original. Los pagos cargados desde Finanzas (sin egreso de caja) no entran en ningún cierre y no avisan.
- **PENDIENTE:** impresión física en la térmica y envío real de mail no verificados.

Ver también: [mapa de configuraciones](ayuda:referencia/configuraciones)
