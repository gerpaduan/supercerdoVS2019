---
pantalla: Ventas.POS
titulo: Punto de venta (POS) — saldo del cliente y ticket
rol: usuario
modulo: Ventas
orden: 0
permiso: PENDIENTE (no se verificó en esta revisión qué permiso exige la pantalla)
revisada: 2026-10-19
---
## Para qué sirve
Registrar ventas en el mostrador. **Esta ayuda cubre solo lo que se agregó el 2026-10-19: el saldo de cuenta corriente del cliente y su opción en el ticket, y la opción de no imprimir el nombre de la empresa en el ticket no fiscal.** El resto del POS (carrito, formas de pago, apertura de caja, borradores) está pendiente de documentar.

## Saldo del cliente
Al elegir un cliente (lupa o **F9**), debajo de su nombre aparece **Saldo:**
- En **rojo** y con **(debe)** si el cliente te debe. Ej.: `Saldo: $ 5.000,00 (debe)`.
- En **verde** y con **(a favor)** si tiene plata a favor en su cuenta corriente.

**No aparece** cuando:
- el cliente es **Consumidor Final** o **no tiene movimientos** en su cuenta corriente;
- el saldo es **chico**: hasta el monto de la configuración [Saldo mínimo a informar](ayuda:referencia/configuraciones) (viene en **$100**; vale para deudas y para saldos a favor). Por encima de ese monto se muestra el saldo exacto, sin redondear;
- el cliente tiene la **cuenta corriente reservada** y tu usuario no tiene permiso de Cuentas corrientes (los administradores y quienes tienen ese permiso sí lo ven).

El saldo se consulta al elegir el cliente. Si después cobrás su deuda desde el POS (**F2**) sin cambiar de cliente, el renglón no se refresca hasta que lo elijas de nuevo.

## Total con saldo en el carrito
Debajo del **Total** del carrito, en letra más chica, aparece un renglón que **depende de la forma de pago**: la venta se **suma al saldo solo si es a cuenta corriente** (ahí la deuda realmente crece).
- **Forma de pago elegida = Cuenta corriente:** **Total c/saldo** = total del carrito **más lo que el cliente debe** (si tiene saldo a favor, se resta; si supera la venta dice **A favor** con la diferencia).
- **Cualquier otra forma de pago, o todavía ninguna elegida** (en muchos negocios se elige al cobrar): **Saldo anterior (debe)** o **Saldo anterior (a favor)**, en rojo o verde, **sin sumar** la venta.

Se actualiza solo con cada cambio del carrito y al cambiar la forma de pago. Aparece **solo si hay productos en el carrito** y si el renglón **Saldo** corresponde: valen las mismas reglas de arriba (no se muestra con Consumidor Final, con saldo chico ni con una cuenta reservada si tu usuario no tiene el permiso).

Al **modificar una venta** a cuenta corriente del mismo cliente, el saldo que se usa es el de **antes** de esa venta (el renglón Saldo de arriba sigue mostrando el saldo actual), para no contarla dos veces.

## Sumar el saldo al ticket
Después de registrar la venta, al tocar **Ticket** (atajo **2**):
1. Si el cliente tiene un saldo que corresponde informar, el sistema pregunta si se incluye en el ticket y dice cuánto debe o tiene a favor: **«¿Sumar el saldo al ticket?»** si la venta fue a **cuenta corriente**, **«¿Mostrar el saldo en el ticket?»** si se pagó de otra forma.
2. **Sí:** el ticket imprime, debajo del total de la venta (y de Pago/Vuelto si los hay), el renglón `Saldo anterior (debe)` o `Saldo anterior (a favor)` (en tickets de 58 mm, `Saldo ant.`): lo que el cliente tenía **antes** de esta venta. **Solo si la venta fue a cuenta corriente** se agrega además `TOTAL c/saldo`: el total de la venta más lo que debía (si tenía a favor, se resta; si supera la venta se rotula `A favor`). Si la venta se pagó de otra forma, **no se suma**: el ticket muestra solo el saldo anterior.
3. **No** (o cerrar el aviso): el ticket sale como siempre, sin saldo.

En una venta a cuenta corriente `TOTAL c/saldo` coincide con lo que el cliente debe después de ella. Sin saldo para informar (Consumidor Final, saldo chico, cuenta reservada) no se pregunta nada y el ticket se imprime directo. Si falla la consulta del saldo, el ticket sale igual, sin saldo.

## Nombre de la empresa en el ticket
El ticket **no fiscal** (el que dice **X** / «No valido como Factura») imprime arriba el nombre de la empresa y sus slogans (los de [Mi Empresa](ayuda:pantalla/Empresa.Index)). Para que **no** salgan, un administrador tilda **ocultarEncabezadoTicketNoFiscal** en la pantalla **Parámetros** (`/Parametros`) y guarda. Destildado (o sin cargar) el ticket sale como siempre.
- Vale para todas las impresiones del ticket de venta: post-venta, reimpresión desde el historial y modal del comprobante, en 58 y 80 mm.
- El ticket **facturado** (con CAE) no cambia: siempre imprime el nombre, los slogans y los datos fiscales.
- No cambia los PDF ni los demás tickets (recibos de pago o cobro, apertura y cierre de caja).

## Notas
- Solo el ticket del post-venta ofrece el saldo. La **reimpresión desde el historial**, el **PDF** y el **email** no lo incluyen.
- El ticket facturado (con CAE) también lo admite: el saldo va después del total fiscal y antes de los datos fiscales.
- PENDIENTE: el resto de la pantalla (carrito, forma de pago, caja, borradores) no se documentó en esta revisión.
