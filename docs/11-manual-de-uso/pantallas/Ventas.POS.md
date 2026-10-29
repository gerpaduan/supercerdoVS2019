---
pantalla: Ventas.POS
titulo: Punto de venta (POS) — saldo del cliente y ticket
rol: usuario
modulo: Ventas
orden: 0
permiso: PENDIENTE (no se verificó en esta revisión qué permiso exige la pantalla)
revisada: 2026-10-29
---
## Para qué sirve
Registrar ventas en el mostrador. **Esta ayuda cubre solo lo que se agregó el 2026-10-19: el saldo de cuenta corriente del cliente y su opción en el ticket, y la opción de no imprimir el nombre de la empresa en el ticket no fiscal; y, desde el 2026-10-20, qué muestra **Mis ventas** (F6); y, desde el 2026-10-21, cobrar el saldo del cliente junto con la venta («El cliente paga ahora»).** El resto del POS (carrito, formas de pago, apertura de caja, borradores) está pendiente de documentar.

## Saldo del cliente
Al elegir un cliente (lupa o **F9**), debajo de su nombre aparece **Saldo:**
- En **rojo** y con **(debe)** si el cliente te debe. Ej.: `Saldo: $ 5.000,00 (debe)`.
- En **verde** y con **(a favor)** si tiene plata a favor en su cuenta corriente.

**No aparece** cuando:
- el cliente es **Consumidor Final** o **no tiene movimientos** en su cuenta corriente;
- el saldo es **chico**: hasta el monto de la configuración [Saldo mínimo a informar](ayuda:referencia/configuraciones) (viene en **$100**; vale para deudas y para saldos a favor). Por encima de ese monto se muestra el saldo exacto, sin redondear;
- el cliente tiene la **cuenta corriente reservada** y tu usuario no tiene permiso de Cuentas corrientes (los administradores y quienes tienen ese permiso sí lo ven).

El saldo se consulta al elegir el cliente. Si después cobrás su deuda desde el POS (**F2**) sin cambiar de cliente, el renglón no se refresca hasta que lo elijas de nuevo.

## Renglones del carrito
Cada producto agregado muestra, debajo de su nombre, **cantidad x precio** y, a la derecha, el **subtotal**, en formato argentino: punto de miles y coma decimal. Ej.: `2,000 x $ 9.900,00` y `$ 19.800,00` (desde 2026-10-27; antes se veía `2.000 x $ 9900.00`). La cantidad lleva siempre 3 decimales.
- Es solo cómo se muestra: los campos de la ventana de la línea (precio, subtotal) no cambiaron.

## Saldo bajo el total del carrito
Debajo del **Total** del carrito, en letra más chica, aparece el saldo del cliente. **Rojo** si el cliente **debe**, **azul** si tiene **saldo a favor**. Qué dice depende de la forma de pago, porque la venta solo toca la cuenta del cliente si es a **cuenta corriente**:
- **Sin forma de pago elegida, o cualquier otra que no sea Cuenta corriente:** **Saldo actual (debe)** o **Saldo actual (a favor)**. No suma la venta: pagar en efectivo, tarjeta o QR no cambia su cuenta.
- **Forma de pago elegida = Cuenta corriente:** **Quedaría (debe)** o **Quedaría (a favor)**: el saldo que le queda al cliente con esta venta (ver el cuadro de abajo).

Se actualiza solo con cada cambio del carrito y al cambiar la forma de pago. Aparece **solo si hay productos en el carrito** y si el renglón **Saldo** corresponde: valen las mismas reglas de arriba (no se muestra con Consumidor Final, con saldo chico ni con una cuenta reservada si tu usuario no tiene el permiso).

Al **modificar una venta** a cuenta corriente del mismo cliente, el saldo que se usa es el de **antes** de esa venta (el renglón Saldo de arriba sigue mostrando el saldo actual), para no contarla dos veces.

## Al finalizar: el saldo que quedaría si elegís Cta. Cte.
En la ventana **Seleccione la Forma de Pago**, **un clic en un botón registra la venta**. Por eso, si el cliente tiene saldo para informar, **antes de tocar nada** aparece, justo debajo de los botones, un cuadro:

**Si elegís Cta. Cte. (4), el saldo queda:** el importe grande, con **DEBE** (rojo) o **A FAVOR** (azul), y abajo la cuenta: *saldo actual ± esta venta*.

| Situación | Cuadro |
|---|---|
| El cliente **debe** $ 146.293,73 y la venta es de $ 15.891,90 | **$ 162.185,63 DEBE** (rojo) — saldo actual $ 146.293,73 (debe) **+** esta venta $ 15.891,90 |
| Tiene **a favor** $ 57.300,00 y la venta es de $ 15.891,90 | **$ 41.408,10 A FAVOR** (azul) — saldo actual $ 57.300,00 (a favor) **−** esta venta $ 15.891,90 |
| Tiene **a favor** $ 300,00 y la venta es de $ 1.000,00 | **$ 700,00 DEBE** (rojo) — el saldo a favor no alcanza y queda debiendo la diferencia |

- Si aplicás un **descuento o recargo** con **/**, el cuadro se recalcula con el total nuevo.
- **No aparece** con Pago Mixto (Cta. Cte. no admite pago mixto), cuando el botón Cuenta corriente no se puede usar, con Consumidor Final, con saldo chico (por debajo del [Saldo mínimo a informar](ayuda:referencia/configuraciones)) ni con una cuenta reservada si tu usuario no tiene el permiso.
- Es solo un aviso: con cualquier otra forma de pago la cuenta del cliente no cambia.

## Cobrar el saldo al finalizar («El cliente paga ahora»)
Si con esta venta el cliente **quedaría debiendo** (el cuadro rojo de arriba), debajo del cuadro aparece el interruptor **El cliente paga ahora** (atajo **C**). Sirve para cobrar en el mismo momento, sin ir después a Cuentas corrientes (F2):
1. **Tildalo** (clic o tecla **C**). La venta va **sola a Cta. Cte.** y el botón **Cta. Cte.** queda **inhabilitado** (el cliente está pagando, no es un medio de cobro). El cuadro pasa a decir **«Con esta venta el cliente debe en total»** y el importe es lo que hay que cobrar para dejar la cuenta en cero.
2. Escribí el **importe que paga** (el cursor ya queda en el campo), o tildá **«Paga todo»** (viene destildado) para cobrar toda la deuda. Debajo se lee qué le queda a la cuenta: **«Le queda debiendo $ X»**, **«La cuenta queda en $ 0,00»** o **«Paga de más: le queda a favor $ X»**.
3. **Tocá la forma de pago con la que paga** (Efectivo, Débito, Crédito, QR o Transferencia, clic o teclas 1 a 6). **Un clic registra la venta y el cobro**: el cobro queda con esa forma de pago (Débito → cobro en Débito), con su recibo automático, y suma a la caja solo si es Efectivo.
4. **Pago Mixto**: activalo antes o después del tilde. Se reparte el **importe que paga** (no el total de la venta) entre Efectivo y el otro medio, y se registran **dos cobros** (uno en Efectivo y otro en el otro medio).
5. **El modal se achica solo** (igual que con Pago Mixto o con el descuento): botones de forma de pago en una línea y, en escritorio, el cuadro del saldo y el del cobro lado a lado, para que «Total de la Venta» quede a la vista sin scrollear. Al destildar vuelve al tamaño normal.

**Cuándo no aparece el interruptor:** cliente sin deuda tras la venta (sin saldo, saldo chico, saldo a favor que alcanza), Consumidor Final, cuenta reservada sin permiso, **sin caja abierta** (el cobro se asienta en la caja), al **modificar una venta** o cambiar solo la forma de pago. Si estando tildado cambia el total (por ejemplo, un descuento con **/**) y ya no queda deuda, se destilda solo.
- **Factura**: si el cobro es con un medio que exige factura electrónica (Débito, Crédito, QR, Transferencia) se abre la factura, igual que en una venta con ese medio.
- **«La venta se registró, pero el cobro no»**: la venta quedó en Cta. Cte. y no se pudo guardar el cobro (error del sistema). Cobralo desde **F2 → Cuenta corriente → Agregar Pago/Cobro**.
- Si el importe no es válido (vacío, mixto que no suma, forma no permitida) el sistema avisa **antes de guardar** y no registra nada.

## Después de la venta: saldo del cliente
En el aviso **Venta completada**, debajo del total, aparece **Saldo del cliente (debe)** o **Saldo del cliente (a favor)** (rojo o azul): cómo **quedó** su cuenta ahora. Si la venta fue a cuenta corriente ya la incluye («Incluye esta venta, que quedó en cuenta corriente»); si se pagó de otra forma es el mismo saldo de antes («Esta venta no es a Cta. Cte.: no cambia su saldo»). Mismas reglas de arriba para cuándo no aparece.

Si hubo **cobro junto con la venta**, la nota dice «Incluye esta venta … y el cobro de $ X» y, si la cuenta quedó en cero, el renglón dice **«Saldo del cliente (saldado)»**.

El **ticket** sigue su propia regla: suma la venta al saldo **solo si es a cuenta corriente** (ver abajo).

## Sumar el saldo al ticket
Después de registrar la venta, al tocar **Ticket** (atajo **2**):
1. Si el cliente tiene un saldo que corresponde informar, el sistema pregunta si se incluye en el ticket y dice cuánto debe o tiene a favor: **«¿Sumar el saldo al ticket?»** si la venta fue a **cuenta corriente**, **«¿Mostrar el saldo en el ticket?»** si se pagó de otra forma.
2. **Sí:** el ticket imprime, debajo del total de la venta (y de Pago/Vuelto si los hay), el renglón `Saldo anterior (debe)` o `Saldo anterior (a favor)` (en tickets de 58 mm, `Saldo ant.`): lo que el cliente tenía **antes** de esta venta. **Solo si la venta fue a cuenta corriente** se agrega además `TOTAL c/saldo`: el total de la venta más lo que debía (si tenía a favor, se resta; si supera la venta se rotula `A favor`). Si la venta se pagó de otra forma, **no se suma**: el ticket muestra solo el saldo anterior.
3. **No** (o cerrar el aviso): el ticket sale como siempre, sin saldo.

**Con cobro junto con la venta** («El cliente paga ahora») el ticket se imprime **sin preguntar** y sigue después de `TOTAL c/saldo` con un renglón `Su pago <forma>` por cada cobro (dos en pago mixto), un separador y, en negrita, **`Saldo actual (debe|a favor)`**: lo que queda en la cuenta del cliente (`Saldo actual 0,00` si quedó saldada). El post-venta no sobrevive a recargar la pantalla: si se recarga antes de imprimir, el ticket de esa venta ya no incluye el pago.

En una venta a cuenta corriente `TOTAL c/saldo` coincide con lo que el cliente debe después de ella. Sin saldo para informar (Consumidor Final, saldo chico, cuenta reservada) no se pregunta nada y el ticket se imprime directo. Si falla la consulta del saldo, el ticket sale igual, sin saldo.

## Nombre de la empresa en el ticket
El ticket **no fiscal** (el que dice **X** / «No valido como Factura») imprime arriba el nombre de la empresa y sus slogans (los de [Mi Empresa](ayuda:pantalla/Empresa.Index)). Para que **no** salgan, un administrador tilda **ocultarEncabezadoTicketNoFiscal** en la pantalla **Parámetros** (`/Parametros`) y guarda. Destildado (o sin cargar) el ticket sale como siempre.
- Vale para todas las impresiones del ticket de venta: post-venta, reimpresión desde el historial y modal del comprobante, en 58 y 80 mm.
- El ticket **facturado** (con CAE) no cambia: siempre imprime el nombre, los slogans y los datos fiscales.
- No cambia los PDF ni los demás tickets (recibos de pago o cobro, apertura y cierre de caja).

## Mis ventas (F6)
**F6** abre **Mis actividades**; ahí el botón **Mis ventas** lista las ventas del vendedor **desde que abrió su caja** (la caja abierta en este momento), agrupadas por día, con buscador por cliente y filtro por forma de pago. Tocar una venta abre su detalle.
- Con la cuenta compartida de **producción** son las ventas del **empleado que se autorizó** en esa pestaña («Autorizate para operar» o «Cambiar operario»), no las de la cuenta compartida. Si cambiás de operario, Mis ventas muestra las del nuevo.
- En el detalle de una venta de **tu caja abierta** (en esta sucursal) aparecen **Modificar venta** y **Cambiar Forma de Pago**, también con la cuenta de producción (los usa el empleado autorizado; no hace falta volver a autorizarse). **Modificar venta** además requiere el permiso de modificar ventas. Con la cuenta de producción, al guardar o con **Salir edición** volvés al POS con el mismo empleado autorizado.
- **«La autorización del vendedor venció. Recargá el POS e ingresá de nuevo con tu usuario.»**: con la cuenta de producción, la autorización del empleado se perdió (por ejemplo, por mucho tiempo sin usar el POS). Recargá la página y volvé a autorizarte.
- **«No hay una caja abierta para consultar ventas.»**: el vendedor no tiene una caja abierta ahora (por ejemplo, entró con «venta rápida sin apertura de caja» o un encargado ya le cerró la caja). Es correcto: Mis ventas solo muestra la caja abierta. Las ventas anteriores se ven en el listado de **Ventas**.

## Compras propias desde Mis actividades (F6)
En **F6 → Mis actividades**, las compras que registraste **desde que abriste tu caja** (la abierta ahora, en tu sucursal) tienen el botón **Modificar**. Abre la compra en el mismo modal y ahí podés **modificarla** (botón **Modificar**, cambiás lo que haga falta y **Guardar compra**) o **eliminarla** (botón rojo **Eliminar**).
- **No hace falta el permiso «Modificar compra»** para tus propias compras de la caja abierta. Las de otra persona, las de una caja ya cerrada y las de otra sucursal siguen pidiéndolo (sin el permiso la compra no se abre y el sistema avisa «No tiene permisos para operar compras.»). Eliminar pide siempre motivo y usuario y contraseña, igual que en la pantalla completa (ver [Compras.Editar](ayuda:pantalla/Compras.Editar)).
- Al eliminar desde el POS se vuelve a **Mis actividades** (no se pierde la venta en curso). La compra queda con una fila nueva **ELIMINADO: …** (con el importe en negativo si salió plata de la caja; en $ 0,00 si era en cuenta corriente) y la fila original pasa a «Compra eliminada» sin botón de modificar.
- Con la cuenta compartida de producción el sistema compara con el operador que se autorizó para Compras.
- **PENDIENTE de probar con un usuario sin el permiso «Modificar compra»:** en la base de prueba el permiso de crear compras y el de modificarlas vienen juntos, y el login de usuarios que no son admin exige un dispositivo autorizado; lo probado fue con un administrador.

## Notas
- Solo el ticket del post-venta ofrece el saldo. La **reimpresión desde el historial**, el **PDF** y el **email** no lo incluyen.
- El ticket facturado (con CAE) también lo admite: el saldo va después del total fiscal y antes de los datos fiscales.
- PENDIENTE: el resto de la pantalla (carrito, forma de pago, caja, borradores) no se documentó en esta revisión.
