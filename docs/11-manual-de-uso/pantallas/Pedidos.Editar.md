---
pantalla: Pedidos.Editar
titulo: Modificar pedido
rol: admin
modulo: Stock
orden: 7
permiso: Stock.AddOrEditStock o administrador. Un usuario de producción puede abrirla, pero al guardar necesita la autorización de otra persona
revisada: 2026-10-20
alias: Pedidos.Modificar
---
## Para qué sirve
**Corregir un pedido ya guardado**: cambiar cantidades por sucursal, sacar productos o agregar otros. El pedido conserva su número y queda un **historial** de cada modificación.

## Cómo se usa
1. Desde el detalle del pedido, tocá **Modificar**.
2. **Nota**: texto libre (hasta 500 caracteres).
3. **Cantidades**: la tabla tiene un casillero por sucursal. Escribí la cantidad (con coma decimal si hace falta); vacío o 0 = no se pide a esa sucursal. El **Total**, el **importe estimado** y el **total estimado** de arriba se recalculan al escribir.
4. **Quitar un producto**: el tacho de la última columna lo saca del pedido (queda oculto y se descarta al guardar).
5. **Agregar un producto**: escribí nombre o código en *Agregar un producto* y elegilo de la lista; aparece una fila nueva para cargar las cantidades.
6. **Guardar cambios**. Si no cambiaste nada, avisa *No hay cambios para guardar*. **Cancelar** vuelve al detalle sin tocar nada.

## Qué pasa con los precios
- Los productos que **ya estaban** en el pedido **conservan el precio de última compra con el que se guardaron**; solo cambia la cantidad.
- Los productos que **agregás** quedan **sin precio** (no suman al total estimado y se avisa con *N productos sin precio de compra*). Mientras haya alguno sin precio, el total se rotula **Total estimado (parcial)**.
- Debajo de cada precio se ve de qué proveedor y fecha es (⚠ si es de hace más de 90 días respecto del pedido). El precio es el de la última compra **sin distinguir proveedor**; la aclaración está al pie de la tabla.

## Usuario de producción: hace falta una persona autorizada
Si entraste con una **cuenta de producción** (la cuenta compartida del local), al guardar se abre un cartel pidiendo **usuario y contraseña de una persona autorizada** (administrador o con permiso para modificar stock).
- Tiene que ser una **persona**: una cuenta de producción no puede autorizar, ni la propia ni otra.
- Si el usuario o la contraseña no sirven, o la persona no tiene permiso, el mensaje es siempre el mismo (*Usuario o contraseña incorrectos, o sin permiso para modificar pedidos*). Tenés **5 intentos**; desde el 3.º el mensaje dice cuántos te quedan y, al 5.º, hay que **esperar 5 minutos** (ver [intentos de clave al autorizar](ayuda:concepto/intentos-de-clave-al-autorizar)).
- La persona que autorizó **queda registrada en el historial** del pedido. Los intentos rechazados quedan en el registro del sistema (sin la contraseña).
- Con un usuario común no se pide nada.

## Si dos personas lo modifican a la vez
Si otra persona guardó cambios mientras vos editabas, al guardar aparece *El pedido fue modificado por otra persona mientras lo editabas*. No se pisa nada: volvé a abrir el pedido, mirá los cambios y repetí los tuyos.

## Reglas
- El pedido tiene que quedar con **al menos un producto con cantidad**.
- Las cantidades tienen que ser mayores que 0; no se repite el mismo producto en la misma sucursal.
