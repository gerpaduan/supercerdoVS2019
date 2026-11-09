---
pantalla: Ventas.POS
titulo: Punto de venta (POS) — saldo del cliente y ticket
rol: usuario
modulo: Ventas
orden: 0
permiso: PENDIENTE (no se verificó en esta revisión qué permiso exige la pantalla)
revisada: 2026-11-09
---
## Para qué sirve
Registrar ventas en el mostrador. **Esta ayuda cubre solo lo que se agregó el 2026-10-19: el saldo de cuenta corriente del cliente y su opción en el ticket, y la opción de no imprimir el nombre de la empresa en el ticket no fiscal; y, desde el 2026-10-20, qué muestra **Mis ventas** (F6); y, desde el 2026-10-21, cobrar el saldo del cliente junto con la venta («El cliente paga ahora»). y, desde el 2026-10-29, el filtro «Persona» del modal Expendios; y, desde el 2026-11-02, el tema gris del POS; y, desde el 2026-11-03, el aviso «Tu sesión se cerró» del POS bloqueado; y, desde el 2026-11-09, la ventana «Venta completada», los atajos y los videos tutoriales.** El resto del POS (carrito, formas de pago, apertura de caja, borradores) está pendiente de documentar.

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
   - Al tildarlo aparece un **aviso azul** que explica por qué: *la venta y el pago van a la cuenta corriente del cliente; la venta se suma a su deuda y lo que paga ahora se le descuenta*. Por eso Cta. Cte. no se puede elegir: el botón que toques solo indica con qué paga.
   - **Contar billetes**: el botón **$** a la izquierda del importe (o **Alt+B**) abre el contador de billetes. Al aceptar, el **total contado pasa al importe que paga** y debajo se resume lo contado (*«Billetes contados: 1 x $20.000 · 4 x $2.000 = $ 28.000,00»*). **Volver a contar** abre el contador en blanco; **Quitar** borra el detalle y deja el importe. Con «Paga todo» el botón $ queda inhabilitado.
   - El **detalle de billetes se guarda en el cobro** (y sale en su comprobante, como en un cobro de F2) **solo si paga en Efectivo, sin Pago Mixto, sin «Paga todo» y con el importe igual a lo contado**. Si cambiás el importe a mano después de contar, el resumen avisa *«El importe ya no coincide con lo contado»* y el detalle no se guarda; con otra forma de pago o con Pago Mixto el importe se mantiene pero el detalle no se guarda.
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

## Mis actividades (F6)
Muestra los movimientos de caja del vendedor (egresos, pagos electrónicos y cuenta corriente) de la caja abierta. Arriba dice quién es, la sucursal y cuántos movimientos hay.
- **Filtros:** **Todos**, **Egresos de caja**, **Pagos electrónicos** y **Cta Cte** (el elegido queda resaltado). A la derecha: **Mis ventas**, **Nuevo egreso** y, si corresponde, **Mi cierre**.
- **Total egresos de caja:** la tarjeta con el total **la ven solo los administradores**.
- **Tabla:** fecha y hora en dos renglones, el **Tipo** con un color según la categoría, **Ver detalle** para desplegar la fila, y los iconos de imprimir y modificar. La columna **Sucursal** no aparece si todos los movimientos son de la sucursal del encabezado (el desplegable de cada fila igual la muestra).
- En el celular el modal se ve como antes (listado de tarjetas).
- **PENDIENTE:** no se probó este diseño en el navegador con sesión real (aspecto, modo oscuro, administrador y no administrador).

## Mis ventas (F6)
**F6** abre **Mis actividades**; ahí el botón **Mis ventas** lista las ventas del vendedor **desde que abrió su caja** (la caja abierta en este momento), agrupadas por día, con buscador por cliente y filtro por forma de pago. Tocar una venta la **despliega** en el lugar (artículos, total y, si corresponde, **Modificar venta** y **Cambiar forma de pago**) sin salir de la lista; **Abrir detalle** abre el [detalle completo](ayuda:pantalla/Ventas.DetalleVenta) en el mismo modal, donde también está **Factura / Imprimir**. Pueden quedar varias ventas desplegadas a la vez.
- Con la cuenta compartida de **producción** son las ventas del **empleado que se autorizó** en esa pestaña («Autorizate para operar» o «Cambiar operario»), no las de la cuenta compartida. Si cambiás de operario, Mis ventas muestra las del nuevo. Al confirmar la clave en «Cambiar operario» aparece 1 segundo un cartel «Cambiando de operario — Pasando a <nombre>…» y la pantalla se recarga (también en el POS de Puntos de Expendio; no aparece en «Autorizate para operar»).
- En el panel desplegado y en el detalle de una venta de **tu caja abierta** (en esta sucursal) aparecen **Modificar venta** y **Cambiar Forma de Pago**, también con la cuenta de producción (los usa el empleado autorizado; no hace falta volver a autorizarse). **Modificar venta** además requiere el permiso de modificar ventas. Con la cuenta de producción, al guardar o con **Salir edición** volvés al POS con el mismo empleado autorizado.
## Permisos del POS (cuenta de producción y usuario propio)
Los permisos **POS - …** (Usuarios > Permisos) deciden quién entra al POS y qué puede hacer. Rige el operador autorizado en la pestaña (con la cuenta de producción) o el usuario logueado; el administrador no tiene límites.
- **Entrar:** alcanza con tener **activo al menos uno** de los cuatro: *POS - Ctas.Ctes Persona*, *POS - Bonificacion*, *POS - Ultimas Ventas* o *POS - Ultimas 5 Ventas Cliente*. Activo = **Días ver** o **Días editar** en 0 o más (−1 es sin permiso). Con ninguno no se entra: con la cuenta de producción el modal dice **«(Nombre) no tiene permisos de POS. Elegí a un usuario que los tenga.»** (la contraseña es correcta) y con un usuario propio aparece Acceso denegado. Con la contraseña mal el mensaje es **«Usuario o contraseña incorrectos.»**. Los dos cuentan como intento fallido para el bloqueo temporal.
- **Bonificación:** solo con *POS - Bonificacion*. Sin el permiso los botones de bonificar y el **%** de toda la venta avisan «No tiene permiso para bonificar (POS - Bonificación).» y el servidor rechaza una venta con bonificaciones o recargos. Al modificar una venta que ya tenía bonificación se respeta la que tenía.
- **Cuenta corriente (F2), cargar y modificar pagos y cobros:** con *POS - Ctas.Ctes Persona*. **Puede ver** abre las cuentas corrientes y sus movimientos hasta **Días ver** días atrás (más viejo se recorta y avisa). Sin el permiso aparece «No tenés permiso para ver cuentas corrientes (POS - Ctas.Ctes Persona).» Cargar un pago o cobro suelto, o modificar uno, pide **Puede editar**; un pago ya guardado se modifica solo si **lo cargaste vos desde que abriste tu caja actual**, y nunca más atrás del inicio de esa caja ni de **Días editar** (rige el que cae más cerca de hoy). El cobro «El cliente paga ahora» de la venta no depende de este permiso.
- **Últimas ventas (F6 > Mis ventas) y Modificar venta:** con *POS - Ultimas Ventas*. **Puede ver** lista tus ventas desde el inicio de tu caja; sin el permiso dice «No tenés permiso para ver tus últimas ventas (POS - Últimas Ventas).». **Puede editar** permite **Modificar venta** y cambiar su fecha, solo en ventas **de tu caja abierta**, desde su inicio y dentro de **Días editar**: con la caja abierta hace 3 días y 4 días de permiso, el límite son los 3 días de la caja. **Cambiar Forma de Pago** de una venta de tu caja abierta no pide el permiso. Sin caja abierta solo el administrador modifica ventas.
- Hay que dar el permiso de edición en Usuarios > Permisos; la columna **Detalle** de cada permiso explica qué opciones marcar.

### Autorizar con otro usuario (permiso provisorio)
Si al operador **le falta** el permiso de una acción, en el mismo aviso se abre **«Autorizar con otro usuario»**: otro usuario que **sí** tiene ese permiso elige su nombre y escribe **su** contraseña, y el operador puede hacer esa acción hasta que se cierre la ventana donde la usa. Si el operador ya tiene el permiso, no aparece nada de esto.
- **Dónde aparece:** *Bonificar* (el botón y el «%» de toda la venta), *Cuenta corriente* (F2) y *cargar o modificar un pago o cobro*, *Mis ventas* y *Modificar venta* (en el detalle de una venta aparece **«Modificar venta (requiere autorización)»**). Si se cancela, queda el aviso con el botón **Autorizar con otro usuario**.
- **Cuánto dura:** *Bonificar*, hasta **terminar la venta** (al finalizarla o cancelarla, o al cerrar el POS). *Cuenta corriente / pagos / Mis ventas*, hasta **cerrar esa ventana**; el pago, además, hasta guardarlo. *Modificar venta*, hasta **guardar** la venta o **Salir edición**. Cambiar de operario o cerrar el POS termina todas. Si el aviso de cierre no llega, vence sola a los 30 minutos (`Security:AutorizacionPosMinutos`).
- **Qué se evalúa:** el permiso y los **días de quien autoriza**, pero la venta o el pago tiene que seguir siendo de la **caja abierta del operador** y no anterior a su inicio. Si autoriza un **administrador** no hay límite de caja ni de días. La autorización vale para **esa venta** (o ese pago) y solo en esa pestaña del POS.
- **Mensajes:** «(Nombre) tampoco tiene permiso para esta acción. Elegí a un usuario que lo tenga.» si quien autoriza no tiene el permiso; «Tiene que autorizar otro usuario, no el mismo operador.»; y, para una venta o pago fuera de la caja del operador, que quien autoriza no puede autorizarla (salvo administrador). Los errores cuentan como intento fallido para el bloqueo temporal.
- **Registro:** cada autorización queda guardada (quién autorizó, a quién, qué acción, sobre qué venta o pago, cuándo empezó y cuándo terminó) y aparece en **Actividades** (administradores) como **Autorización POS**.
- **«La autorización del vendedor venció. Recargá el POS e ingresá de nuevo con tu usuario.»**: con la cuenta de producción, la autorización del empleado se perdió (por ejemplo, por mucho tiempo sin usar el POS). Recargá la página y volvé a autorizarte.
- **«No hay una caja abierta para consultar ventas.»**: el vendedor no tiene una caja abierta ahora (por ejemplo, entró con «venta rápida sin apertura de caja» o un encargado ya le cerró la caja). Es correcto: Mis ventas solo muestra la caja abierta. Las ventas anteriores se ven en el listado de **Ventas**.

## Compras propias desde Mis actividades (F6)
En **F6 → Mis actividades**, las compras que registraste **desde que abriste tu caja** (la abierta ahora, en tu sucursal) tienen el botón **Modificar**. Abre la compra en el mismo modal y ahí podés **modificarla** (botón **Modificar**, cambiás lo que haga falta y **Guardar compra**) o **eliminarla** (botón rojo **Eliminar**).
- **No hace falta el permiso «Modificar compra»** para tus propias compras de la caja abierta. Las de otra persona, las de una caja ya cerrada y las de otra sucursal siguen pidiéndolo (sin el permiso la compra no se abre y el sistema avisa «No tiene permisos para operar compras.»). Eliminar pide siempre motivo y usuario y contraseña, igual que en la pantalla completa (ver [Compras.Editar](ayuda:pantalla/Compras.Editar)).
- Al eliminar desde el POS se vuelve a **Mis actividades** (no se pierde la venta en curso). La compra queda con una fila nueva **ELIMINADO: …** (con el importe en negativo si salió plata de la caja; en $ 0,00 si era en cuenta corriente) y la fila original pasa a «Compra eliminada» sin botón de modificar.
- Con la cuenta compartida de producción el sistema compara con el operador que se autorizó para Compras.
- **PENDIENTE de probar con un usuario sin el permiso «Modificar compra»:** en la base de prueba el permiso de crear compras y el de modificarlas vienen juntos, y el login de usuarios que no son admin exige un dispositivo autorizado; lo probado fue con un administrador.

## Modal Expendios: filtro «Persona»
En el modal **Expendios** (AvPag), el campo **Persona (cliente del expendio)** busca por razón social, CUIT o identificación (desde 2 letras). Al elegir una persona de la lista se muestran **solo sus expendios**: los que se guardaron con ese cliente fijo y también los que tienen en **Identificación cliente** el mismo texto que la identificación de la persona (no importan mayúsculas, acentos ni espacios de los bordes).
- Al elegirla, **Fecha desde** pasa a hace 6 meses (si ya escribiste una fecha a mano se respeta). La **✕** (o Backspace) quita la persona y vuelve a la búsqueda de siempre.
- Si una persona no trae nada: la **identificación** de la persona debe ser igual a la «Identificación cliente» del expendio (no se compara la razón social, porque personas distintas pueden compartirla).
- PENDIENTE de probar en el navegador con login (lo verificado fue la consulta contra la base local).

## Tema de colores: claro, gris u oscuro
El botón de la barra de arriba (a la izquierda de tu nombre) cambia el tema del POS y de Punto de Expendio, en este orden: **claro → gris → oscuro → claro**. El icono muestra el **próximo** tema (medio círculo = pasar a gris, luna = pasar a oscuro, sol = volver a claro).
- El **gris** sirve cuando el fondo blanco de tu monitor no deja ver las líneas que separan los bloques: el fondo, los bloques, los botones de la barra, el buscador y las **teclas del teclado numérico** son grises, con un borde más marcado que separa cada uno. La única casilla que sigue blanca es la de **Cantidad**, donde se escribe. Las teclas se oscurecen un poco al apretarlas.
- Se recuerda **en cada computadora y navegador** (no por usuario): al volver a entrar queda el último que elegiste ahí.
- Los **cuadros emergentes** (modales) se ven como en el tema claro también en el gris.
- Fuera del POS, el tema gris se ve como el claro; si tocás el botón de tema de otra pantalla estando en gris, pasa a oscuro.
- PENDIENTE de revisar en el monitor donde se veía todo blanco; lo probado fue el aspecto y el cambio de tema en el navegador de desarrollo.

## POS bloqueado: «Tu sesión se cerró»
- Si el POS se bloqueó por inactividad y, mientras tanto, **se cerró la sesión del sistema** (venció, se cerró desde otra pestaña o se inició sesión con otro usuario en el mismo navegador), al tocar **Desbloquear** el POS **no valida la contraseña** y muestra **«Tu sesión se cerró»** en lugar del formulario. Antes mostraba «Contraseña incorrecta.» aunque la clave estuviera bien.
- El botón **Volver a iniciar sesión** recarga el POS: si la sesión no existe pide el login y vuelve al mismo POS; si hay otra sesión abierta carga el POS de ese usuario.
- Una contraseña realmente mal sigue mostrando «Contraseña incorrecta.»; sin red o con el servidor caído sigue el aviso «No se pudo validar la contraseña. Revisá la conexión…».
- PENDIENTE: no se verificó si la venta en curso se recupera después de volver a iniciar sesión.

## Después de cobrar: la ventana «Venta completada»
Al cobrar aparece **Venta completada** con cinco opciones. Cada una tiene su **tecla** (el número del botón) y funciona solo mientras el foco no esté en un campo de texto:

| Tecla | Botón | Qué hace |
|---|---|---|
| **1** | Nueva venta | Cierra la ventana y deja el POS listo para la próxima venta, sin comprobante. |
| **2** | Imprimir ticket | Imprime el ticket térmico (58 u 80 mm, con **cambiar** debajo del botón) y pasa solo a una venta nueva. |
| **3** | Imprimir PDF | Abre el comprobante en PDF (hoja común) y pasa solo a una venta nueva. |
| **4** | Enviar por email | Abre el envío: email destino, asunto y mensaje (los dos últimos ya vienen completos). Al enviar, pasa a una venta nueva; si cancelás, también. |
| **5** | Factura electrónica | Abre la factura electrónica de esa venta; con **Registrar factura** (o **Alt+R**) se emite y vuelve a esta ventana con el aviso «Factura registrada correctamente» y su número, para imprimirla o mandarla por email. |

En la factura electrónica, **Alt+R** registra la factura (Enter dentro de sus campos no la registra, a propósito, para no emitirla sin querer).

## Videos tutoriales del punto de venta
Atajos que muestran los videos (verificados en el código el 2026-11-09):
- **Código y Enter**: el cursor pasa a la cantidad. Con una **balanza** conectada, el peso entra solo; con el **lector de código de barras**, un producto con código EAN se agrega solo con cantidad 1. **Enter** sobre la cantidad agrega el producto.
- **Inicio**: el cursor vuelve al código. **F1**: ayuda con todos los atajos. **F2**: cuentas corrientes. **F4**: editar la última línea. **F6**: Mis actividades. **F7**: egreso de caja. **F8**: historial de precios del cliente. **F9**: buscar cliente. **F10**: buscar producto.
- **Fin**: igual que **Finalizar y cobrar** (no funciona con otra ventana abierta encima).
- En la forma de pago: **1** a **6** eligen la forma (efectivo, débito, crédito, Cta. Cte., QR, transferencia), **+** activa o desactiva el pago mixto, **/** abre el descuento o recargo al total, **C** tilda «El cliente paga ahora». Con pago mixto, **Fin** confirma.
- En pagos y cobros de cuenta corriente: **Alt+Enter** guarda. En el egreso de caja: **Alt+B** abre el contador de billetes.

Son de ejemplo: datos ficticios, la balanza, el lector y la impresora están simulados, el email no se envía y las facturas se emitieron en el modo prueba de AFIP (homologación). Debajo de la imagen va el subtítulo y la tecla que se aprieta.

### Vender: caja, productos, formas de pago y «Venta completada»
Apertura de caja (y la venta rápida sin abrir caja), carga manual, con balanza y con lector, formas de pago y pago mixto, y todas las opciones de **Venta completada**, incluida la factura electrónica con su ticket, PDF y email.

![Video: vender en el punto de venta, de la apertura de caja a la factura](media:Ventas.POS/tutorial-venta.mp4)

### Vender a un cliente con cuenta corriente
Buscar el cliente (F9) y volver a consumidor final (CF), su saldo, el historial de precios y los presupuestos (F8), cobrar parte de la deuda con la venta («El cliente paga ahora») y el ticket con el saldo.

![Video: venta a un cliente con cuenta corriente y cobro de su saldo](media:Ventas.POS/tutorial-cliente.mp4)

### Cuentas corrientes desde el POS (F2) y Mis actividades (F6)
Buscar el cliente, ver sus movimientos, registrar un cobro y un pago, y cómo quedan en la caja.

![Video: cuentas corrientes, pagos y cobros desde el punto de venta](media:Ventas.POS/tutorial-ctacte.mp4)

### Recorrido por la pantalla
Empresa, sucursal, fecha y hora, Duplicar POS, Salir, colores (claro, gris, oscuro), buscador de productos (F10), comentario de la venta, cancelar la venta, ayuda (F1), Mis actividades y Mis ventas.

![Video: recorrido por la pantalla del punto de venta](media:Ventas.POS/tutorial-recorrido.mp4)

### Egreso de caja (F7)
Tipo, descripción, contador de billetes y detalle.

![Video: registrar un egreso de caja con el contador de billetes](media:Ventas.POS/tutorial-egreso.mp4)

Voz: Piper, voz «daniela» (es_AR) de rhasspy/piper-voices, entrenada con el corpus OpenSLR 61 (licencia CC BY-SA 4.0).

## Notas
- Solo el ticket del post-venta ofrece el saldo. La **reimpresión desde el historial**, el **PDF** y el **email** no lo incluyen.
- El ticket facturado (con CAE) también lo admite: el saldo va después del total fiscal y antes de los datos fiscales.
- PENDIENTE: el resto de la pantalla (carrito, forma de pago, caja, borradores) no se documentó en esta revisión.
