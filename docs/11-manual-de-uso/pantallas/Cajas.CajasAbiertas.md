---
pantalla: Cajas.CajasAbiertas
titulo: Cajas abiertas y cierre de caja
rol: admin
modulo: Caja
orden: 2
permiso: Caja.CerrarCaja (cerrar cajas y ver el historial; sin el permiso se pide usuario y clave de quien lo tiene, valen 5 minutos) / Caja.CierresDeCaja (modificar un cierre ya hecho)
revisada: 2026-10-07
---
## Para qué sirve
Ver las **cajas que están abiertas** (una por cajero), **cerrarlas** y consultar el **historial de cierres**. Quien cierra la caja es el encargado: confirma el efectivo que entrega el cajero y el sistema calcula la diferencia.

## Cómo se usa
1. Elegí sucursal y buscá al cajero si hace falta.
2. En la caja del cajero tocá **Cerrar**. Se abre **Cerrar Caja** con la caja inicial, las ventas y los egresos hasta ese momento.
3. Cargá **Caja Cierre** (el efectivo que hay en la caja) e **Importe Retirado**. Se calculan solos la **Diferencia** (verde hasta ±100, amarillo hasta ±300, rojo más allá) y **Queda en Caja**.
4. Con el botón de flechas junto a **Fecha Cierre** (**Actualizar importes**) se vuelven a traer ventas, egresos y la fecha hasta ahora.
5. **Cerrar Caja** confirma. Si el cajero tiene ventas sin finalizar, el sistema avisa y hay que confirmar.

## Importe que cargó el cajero (conteo de cierre)
- El cajero puede contar su caja desde el POS (**Mis actividades → Mi cierre**, con el contador de billetes). Ese importe **no cierra la caja**: es una referencia para el encargado.
- Al abrir **Cerrar Caja**, si el cajero ya lo cargó, aparece bajo **Caja Cierre** como **Declarado por el cajero** (con fecha y hora) y el importe **entra solo en Caja Cierre**. **Usar** lo vuelve a poner; **ver billetes** muestra el detalle del contador.
- Si el cajero **todavía no cargó** su cierre aparece la etiqueta **El cajero no cargó su cierre** y Caja Cierre queda vacío para que lo cargues vos.
- **Actualizar importes** también trae lo que el cajero haya cargado o corregido mientras tenías el modal abierto. Pisa **Caja Cierre** solo si **vos no lo escribiste a mano**; si ya lo escribiste, queda tu valor y se avisa que es distinto al del cajero (el del cajero se ofrece con **Usar**).
- **Aviso si el cajero lo cambia con el modal abierto:** mientras tenés **Cerrar Caja** abierto, el sistema revisa cada 10 segundos si el cajero cargó o modificó su cierre. Si lo hizo, aparece un aviso amarillo (*"El cajero modificó su cierre de caja mientras tenías este modal abierto"*, con el importe nuevo y la hora) y el botón de **Actualizar importes** se resalta. Tocalo para traer el importe nuevo. Si la caja la cerró otro usuario mientras tanto, el aviso lo dice.
- Esta función solo está en las empresas con base Postgres; en las demás no se ve nada.

## Historial de cierres
- Muestra cada cierre con sus importes y quién lo cerró. La columna **Origen del cierre** indica quién cargó el importe de **Caja cierre**:
  - **Ingresado por el cajero**: coincide con lo que contó el cajero.
  - **Modificado por el encargado**: el cajero declaró otro importe y el encargado cerró con uno distinto (al pasar el mouse se ve lo que declaró el cajero y el detalle de billetes).
  - **Cargado por el encargado**: el cajero no cargó su cierre.
- **Modificar** (solo con permiso de cierres de caja) abre el cierre ya hecho; ahí también se ve el origen del importe.
- Si el encargado escribió exactamente el mismo importe que declaró el cajero, figura como **Ingresado por el cajero**.

## Cambios en una caja después de cerrarla (aviso)
- Si después de cerrar una caja se **cargó o modificó** algo que cae dentro de ella (una venta, una compra del POS, un pago o cobro del POS, un egreso de caja), el sistema deja un **aviso en el cierre**: el cierre guardado (ventas, egresos y diferencia) puede quedar desactualizado.
- En el **Historial** la columna **Avisos** muestra **Modificada (N)** en rojo (al pasar el mouse se ve la última vez) y **Reabierta (N)** si la caja se reabrió.
- Al tocar **Modificar** en ese cierre aparece, arriba, el aviso con el resumen (por ejemplo "2 ventas, 1 pago") y **ver detalle**: quién lo hizo, cuándo y cómo cambió (monto o forma de pago antes → después).
- Quien hace el cambio ve antes una advertencia (*"Esta caja ya fue cerrada… el cierre va a quedar inconsistente"*) y tiene que confirmar. Si el cambio no toca montos (por ejemplo solo observaciones) se guarda igual y queda registrado.

## Reabrir la última caja de un usuario
- En el **Historial**, la última caja cerrada de cada usuario (en esa sucursal) tiene el botón **Reabrir** (solo con el permiso de modificar cierres de caja). **Solo se puede reabrir la última**: si el usuario ya abrió una caja nueva o tiene una abierta en esa sucursal, no se puede.
- Al tocarlo se muestra un resumen: hace cuánto se cerró, el **importe retirado** y lo que **queda en caja** del cierre que se deshace, y cuántas ventas, egresos y pagos de ese usuario hay **desde el cierre original** (al reabrir quedan dentro de la caja y se suman al próximo cierre). Hay que escribir el **motivo** (obligatorio).
- Al confirmar la caja vuelve a **Cajas abiertas** como cuando se abrió: se borran el cierre cargado (Caja Cierre, diferencia, importe retirado, queda en caja, totales) y **el conteo que había declarado el cajero**. Queda registrada la reapertura con todo lo que se deshizo.
- El vendedor puede **seguir vendiendo** al recargar el POS. Si el encargado ya retiró efectivo, al volver a cerrar la diferencia no lo tendrá en cuenta.
- Solo disponible en empresas con base Postgres.

## Reglas y efectos
- El importe del cajero **no es un egreso**: no cambia el total de EgresosCaja ni la diferencia.
- El cajero puede corregir su conteo solo **mientras su caja siga abierta**; una vez cerrada queda como lo declaró.
- **PENDIENTE:** documentar el cambio de sucursal de una caja y el botón Actividades de este modal.
- **PENDIENTE:** prueba manual en el navegador con sesión de este flujo (no verificado todavía).

Ver también: [Egresos de caja](ayuda:pantalla/Cajas.EgresosCaja), [mapa de configuraciones](ayuda:referencia/configuraciones)
