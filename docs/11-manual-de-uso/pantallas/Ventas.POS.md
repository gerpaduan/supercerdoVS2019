---
pantalla: Ventas.POS
titulo: Punto de venta (POS) — saldo del cliente y ticket
rol: usuario
modulo: Ventas
orden: 0
permiso: PENDIENTE (no se verificó en esta revisión qué permiso exige la pantalla)
revisada: 2026-11-12
---
## Para qué sirve
Registrar ventas en el mostrador. **Esta ayuda cubre solo lo que se agregó el 2026-10-19: el saldo de cuenta corriente del cliente y su opción en el ticket, y la opción de no imprimir el nombre de la empresa en el ticket no fiscal; y, desde el 2026-10-20, qué muestra **Mis ventas** (F6); y, desde el 2026-10-21, cobrar el saldo del cliente junto con la venta («El cliente paga ahora»). y, desde el 2026-10-29, el filtro «Persona» del modal Expendios; y, desde el 2026-11-02, el tema gris del POS; y, desde el 2026-11-03, el aviso «Tu sesión se cerró» del POS bloqueado; y, desde el 2026-11-09, la ventana «Venta completada», los atajos, la ventana «Línea de venta» (cantidad, eliminar, bonificar) y los videos tutoriales; y, desde el 2026-11-10, el aviso «Peso inestable» debajo de Cantidad; y, desde el 2026-11-11, qué pasa si te cierran la caja con el POS abierto, el tema gris más oscuro y el botón de brillo de pantalla.** El resto del POS (formas de pago, apertura de caja, borradores) está pendiente de documentar.

## Saldo del cliente
Al elegir un cliente (lupa o **F9**), debajo de su nombre aparece **Saldo:**
- En **rojo** y con **(debe)** si el cliente te debe. Ej.: `Saldo: $ 5.000,00 (debe)`.
- En **verde** y con **(a favor)** si tiene plata a favor en su cuenta corriente.

**No aparece** cuando:
- el cliente es **Consumidor Final** o **no tiene movimientos** en su cuenta corriente;
- el saldo es **chico**: hasta el monto de la configuración [Saldo mínimo a informar](ayuda:referencia/configuraciones) (viene en **$100**; vale para deudas y para saldos a favor). Por encima de ese monto se muestra el saldo exacto, sin redondear;
- el cliente tiene la **cuenta corriente reservada** y tu usuario no tiene el permiso **Ver cuentas corrientes reservadas** (los administradores y quienes lo tienen sí lo ven).

El saldo se consulta al elegir el cliente. Si después cobrás su deuda desde el POS (**F2**) sin cambiar de cliente, el renglón no se refresca hasta que lo elijas de nuevo.

## Renglones del carrito
Cada producto agregado muestra, debajo de su nombre, **cantidad x precio** y, a la derecha, el **subtotal**, en formato argentino: punto de miles y coma decimal. Ej.: `2,000 x $ 9.900,00` y `$ 19.800,00` (desde 2026-10-27; antes se veía `2.000 x $ 9900.00`). La cantidad lleva siempre 3 decimales.
- Es solo cómo se muestra: los campos de la ventana de la línea (precio, subtotal) no cambiaron.
- Desde el 2026-11-12, el **código** del producto va en una etiqueta chica pegada al nombre: `#1 Cabeza [cód 2]` (antes `#1 Cabeza (cod: 2)`). El `#1` es el número de línea y se ve apagado. Si el nombre es muy largo se corta con «…», pero la etiqueta del código siempre se ve. Lo mismo vale en Punto de Expendio. También desde el 2026-11-12, «cantidad x precio» y el total de cada línea se ven en un negro más fuerte (en el tema oscuro, en un blanco más fuerte) y la negrita del nombre es un poco más chica. Además, el nombre del producto se ve en **semi-negrita** (menos pesado que la negrita) en el carrito, y en el panel de la derecha, cuando buscás por código, es **más grande** para que sea lo primero que se lee.

## Campos Código y Cantidad, foco y Subtotal
Desde el 2026-11-12, en el POS y en Punto de Expendio:
- **Código** tiene la misma letra que **Cantidad** (mismo tamaño y negrita), en cualquier tamaño de pantalla. En Punto de Expendio la caja de Cantidad es más alta porque llena su tarjeta; la letra es igual.
- El campo que tiene el cursor (**Código** o **Cantidad**) se marca con un **contorno verde grueso** y el campo se aclara un poco, así se ve de lejos dónde se va a escribir. En el tema gris el verde es más luminoso (con un resplandor suave) y el campo se aclara a un gris verdoso; ahí, **Cantidad** en reposo tiene el mismo gris que **Código**, no un casi blanco, con un **borde oscuro más grueso** para que no se pierda. La etiqueta «Cantidad» también se pinta de verde mientras el cursor está ahí. El verde se usa solo para esto.
- Cuando la cantidad **viene de la balanza** (el campo queda bloqueado mostrando el peso), aparece el **dibujo de una balanza** dentro de Cantidad, a la izquierda. Al pasar a cantidad manual (tecla `*`) desaparece. PENDIENTE: se probó simulando el estado, no con una balanza real conectada.
- **Subtotal** se ve más grande, en negrita y en una fila con borde oscuro (no verde). En el celular se agranda solo el valor.
- Debajo del **Total**, los textos «N ítems activos» y «X kgs | Y unidad» se ven más oscuros y en semi-negrita (en el tema oscuro, más claros) y con la letra un punto más grande, para leerse mejor.

### Ventas sin guardar (corte de luz, navegador cerrado)
Verificado en el código el 2026-11-10 (`pos-borrador.js`, `VentasController.Borradores.cs`):
- Mientras hay productos en el carrito, el POS guarda la venta en el navegador y en el sistema; abajo dice **«Venta resguardada»** con la hora.
- **Misma computadora:** al volver a entrar al POS, la venta aparece sola en el carrito.
- **Otra computadora, o el navegador no la tiene:** la **campana** de arriba muestra tus ventas sin guardar (con **Cargar al carrito**), y **F1 → Ver ventas sin guardar** lista las de **toda la sucursal**: usuario, inicio, última señal, estado, cliente y total, con **Ver ítems**, **Cargar al carrito** y **Descartar**.
- **Estado:** «En uso» si tuvo señal en los últimos **5 minutos**; si es tuya se puede cargar igual confirmando «Figura en uso» → **Cargar igual** (típico de un corte de luz). «Interrumpida» se carga directo; si la pestaña se cerró normalmente, queda interrumpida enseguida.
- Al cargarla avisa «Revisá los precios antes de finalizar: son los de cuando se armó la venta». Para cargar otra, el carrito tiene que estar vacío.
- **Descartar** pide un motivo, queda registrado y se avisa al administrador. Las ventas de **otro usuario** (cargar o descartar) piden la autorización de un supervisor.
- Si se abre la caja con ventas sin guardar propias, la apertura avisa que se recuperan desde F1.

### Corregir o bonificar un renglón (ventana «Línea de venta»)
Se abre tocando el renglón, o con **F4** para el último. Al pie muestra los atajos (verificados en el código el 2026-11-09):
- **C**: cantidad. Se escribe (o con los botones **−** y **+**) y **Enter** la aplica.
- **E**: eliminar. Pregunta «¿Eliminar el producto?»: **Enter** = Sí, **N** = No. La línea queda **tachada** (anulada), no suma al total y ya no se puede modificar.
- **B** o **/**: bonificar. Se escribe el **precio bonificado** y el % se calcula solo. Una segunda **/** tilda **Bonificar por porcentaje** (se escribe el %; negativo = **recargo**) y una tercera **/** tilda **Aplicar a todos los productos**: el mismo % para todo el carrito, que **reemplaza** las bonificaciones que ya tenían. **Enter** aplica. El renglón muestra `Bonif:12%` (o `Recargo:`).
- Si la línea ya tiene bonificación, la ventana se abre directo en el porcentaje (una sola **/** pasa a «Aplicar a todos»).
- **Descuento a toda la venta al cobrar**: en la forma de pago, **/** abre el «% descuento a todos los productos», **Enter** lo confirma y se ve el descuento aplicado y el total a cobrar. **Solo** si ningún producto tiene bonificación propia; si no, avisa «Bonificación conjunta no disponible». Cerrar la ventana de la forma de pago lo descarta.
- Sin el permiso *POS - Bonificacion*, bonificar pide la autorización de otro usuario (ver «Permisos del POS» más abajo).
- **Defecto conocido (2026-11-09):** si se aplica el descuento a toda la venta y se cierra la forma de pago sin cobrar, al volver a abrirla el total queda bien (sin el descuento), pero debajo sigue diciendo «(con descuento)».

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
- **Factura**: si el cobro es con una forma de pago que la empresa eligió para facturar (por defecto Débito, Crédito, QR y Transferencia; ver más abajo) se abre la factura, igual que en una venta con ese medio.
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
- **Tabla:** fecha y hora en dos renglones, el **Tipo** con un color según la categoría, **Ver detalle** para desplegar la fila, y los iconos de imprimir y modificar. La columna **Sucursal** no aparece si todos los movimientos son de la sucursal del encabezado.
- **Modificar un egreso (desde 2026-11-10):** cualquier vendedor, **sea o no administrador**, ve el lápiz **Modificar** en los egresos de caja **que cargó él**, de **su caja abierta** y de la **sucursal activa**. Con la cuenta de producción vale para el operador autorizado. No aparece en **Pagos electrónicos** ni en **Cta Cte** (se cambian desde su módulo); las compras propias siguen como se explica en *Compras propias desde Mis actividades*. Si alguien intenta abrir el egreso de otro vendedor, el sistema avisa «Solo podés modificar egresos de caja que cargaste vos.». **Cada modificación queda registrada** (antes → después) y el encargado la ve al cerrar la caja (ver [Cajas.CajasAbiertas](ayuda:pantalla/Cajas.CajasAbiertas)).
- **Fila desplegada (desde 2026-11-10):** un panel chico pegado a la fila (la fila queda resaltada y la flecha se da vuelta) con el **Detalle** (o «Sin detalle cargado»), el **Conteo de billetes** si se usó el contador, **Modificado por … · fecha** solo si se modificó, y el **N° de egreso**. No repite lo que ya muestra la fila (Creado por, Creado) ni la sucursal del encabezado, y no muestra campos vacíos.
- En el celular el modal se ve como antes (listado de tarjetas).
- Probado en el navegador con sesión real el 2026-11-10 (base local, usuario administrador): Modificar en el egreso propio, la fila desplegada en claro y oscuro, y el rechazo al abrir el egreso de otro vendedor. **PENDIENTE:** probarlo con un usuario no administrador (en el POS pasa por la misma regla que el administrador) y con la cuenta de producción.

### Mi cierre de caja (conteo del cajero)
Verificado en el código y grabado el 2026-11-10 (`Cajas/_MiCierreCaja.cshtml`):
- **Mi cierre** (en Mis actividades) abre «Mi cierre de caja»: **Efectivo contado en caja**, a mano o con el **contador de billetes** (botón del billete), que deja el detalle en **Conteo de billetes**; después **Guardar mi cierre**.
- Al guardar: «Se guardó correctamente el conteo de tu cierre de caja.» y el comprobante: **1** cerrar sin imprimir, **2** ticket (58 u 80 mm), **3** PDF, **4** mail.
- **No cierra la caja ni es un egreso**: no saca plata de la caja. El encargado lo ve al cerrar la caja (Cajas → Cajas abiertas).
- Mientras la caja siga abierta se puede corregir: el botón pasa a **Actualizar mi cierre** y la ventana avisa «Modo actualizar» con el importe y la fecha anteriores. Si se cambia el importe a mano y no coincide con el conteo de billetes, avisa que el conteo se borra y pide un segundo clic.

## Mis ventas (F6)
**F6** abre **Mis actividades**; ahí el botón **Mis ventas** lista las ventas del vendedor **desde que abrió su caja** (la caja abierta en este momento), agrupadas por día, con buscador por cliente y filtro por forma de pago. A la izquierda de los filtros se ve **cuántas ventas** y el **total** de lo que muestra la lista (cambian al filtrar). Cada venta ocupa dos renglones: comprobante, forma de pago y cliente; y caja e idVenta. Tocar una venta la **despliega** en el lugar (un renglón por artículo y, en el pie, **Modificar venta** y **Cambiar forma de pago** si corresponden, **Abrir detalle** y el **total** con la cantidad de artículos) sin salir de la lista; **Abrir detalle** abre el [detalle completo](ayuda:pantalla/Ventas.DetalleVenta) en el mismo modal, donde también está **Factura / Imprimir**. Pueden quedar varias ventas desplegadas a la vez.
- Con la cuenta compartida de **producción** son las ventas del **empleado que se autorizó** en esa pestaña («Autorizate para operar» o «Cambiar operario»), no las de la cuenta compartida. Si cambiás de operario, Mis ventas muestra las del nuevo. Al confirmar la clave en «Cambiar operario» aparece 1 segundo un cartel «Cambiando de operario — Pasando a <nombre>…» y la pantalla se recarga (también en el POS de Puntos de Expendio; no aparece en «Autorizate para operar»).
- En el panel desplegado y en el detalle de una venta de **tu caja abierta** (en esta sucursal) aparecen **Modificar venta** y **Cambiar Forma de Pago**, también con la cuenta de producción (los usa el empleado autorizado; no hace falta volver a autorizarse). **Modificar venta** además requiere el permiso de modificar ventas. Con la cuenta de producción, al guardar o con **Salir edición** volvés al POS con el mismo empleado autorizado.
- **Abrir detalle** (en el panel desplegado) muestra la venta completa y el botón **Factura / Imprimir**: ticket (58 u 80 mm), PDF, email o factura electrónica de esa venta. Desde el panel no se imprime.
- **Modificar venta** abre el POS en modo edición: arriba del total dice **«Modificando venta #…»**, los productos que ya estaban mantienen su precio original, el botón verde dice **Guardar cambios** y **Cancelar Venta** pasa a **Salir edición** (vuelve sin cambiar nada). Al cobrar, la forma de pago muestra **«Forma de pago actual»** y **Enter** la mantiene. Al guardar aparece **«Venta actualizada»** y se vuelve al POS (verificado 2026-11-09).
- **Cambiar forma de pago** abre el POS con **«Cambiando forma de pago #…»** y los productos bloqueados: **Fin** y se elige la nueva forma (por ejemplo **4**, cuenta corriente, si la venta tiene cliente).
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
- Probado en el navegador con login el 2026-11-10 (video de Expendios): al elegir la persona, **Fecha desde** pasó a 6 meses atrás y apareció su presupuesto de 2 días antes.

### Cobrar expendios (AvPag)
Verificado en el código y grabado el 2026-11-10 (`ventas-expendios-pos.js`): **AvPag** abre **Expendios** con los de hoy en estado **Pendientes** y el cursor en **Búsqueda** (número de expendio, identificación del cliente o número de remito; filtra mientras escribís). **Enter** carga el primero que coincide; **Cargar** en la fila hace lo mismo. Lo cargado pasa al carrito con **Exp.Nro** en cada producto y deja de figurar en «Pendientes». **Cargar todos los expendios mostrados** carga los visibles juntos (pide confirmar) y **Quitar expendios cargados** saca los de esta venta. Al cobrar, el expendio queda asignado a la venta.

## Tema de colores: claro, gris u oscuro
El botón de la barra de arriba (a la izquierda de tu nombre) cambia el tema del POS y de Punto de Expendio, en este orden: **claro → gris → oscuro → claro**. El icono muestra el **próximo** tema (medio círculo = pasar a gris, luna = pasar a oscuro, sol = volver a claro).
- El **gris** sirve cuando el fondo claro cansa la vista o no deja ver las líneas que separan los bloques, por ejemplo en un monitor que queda más abajo que tus ojos. Desde el 2026-11-11 es un **gris medio**, bastante más oscuro que antes. El fondo, los bloques, los botones de la barra, el buscador y las **teclas del teclado numérico** son grises, con bordes oscuros que separan cada uno. Los botones **CF** y **Agregar** también son grises (Agregar en verde apagado cuando se puede usar). La única casilla clara es la de **Cantidad**, donde se escribe. Las teclas se oscurecen al apretarlas.
- Se recuerda **en cada computadora y navegador** (no por usuario): al volver a entrar queda el último que elegiste ahí.
- Desde el 2026-11-11, los **cuadros emergentes** (Forma de pago, Buscar producto, Expendios, Comentario, avisos y confirmaciones, Nuevo egreso, etc.) y el **panel de ayuda** también se ven en gris. Las casillas donde se escribe son de un gris apenas más claro, con borde oscuro (en la pantalla principal, desde el 2026-11-12, Cantidad tiene el mismo gris que el campo Código), y los botones y avisos de color (verde de cobrar, rojo, amarillo) no cambian.
- Fuera del POS, el tema gris se ve como el claro; si tocás el botón de tema de otra pantalla estando en gris, pasa a oscuro.
- PENDIENTE de revisar en el monitor real del local; lo probado fue el aspecto en el navegador de desarrollo.

## Brillo de pantalla
El botón con la **lamparita**, a la izquierda del botón de tema, oscurece toda la pantalla del POS y de Punto de Expendio. Cada vez que lo tocás baja un escalón: **100 % → 90 % → 80 % → 70 % → 100 %**. Cuando el brillo está por debajo de 100, la lamparita muestra el porcentaje en un recuadro amarillo.
- Sirve con los tres temas (claro, gris u oscuro) y oscurece también los cuadros emergentes. Los botones se siguen tocando igual.
- Se recuerda **en cada computadora y navegador**, igual que el tema.
- El ticket y cualquier impresión salen siempre con el brillo normal.
- Si el monitor también tiene botón de brillo, conviene probar primero bajándolo ahí. Si queda por debajo de la vista, inclinarlo hacia arriba, hacia el cajero, también ayuda.

## POS bloqueado: «Tu sesión se cerró»
- Si el POS se bloqueó por inactividad y, mientras tanto, **se cerró la sesión del sistema** (venció, se cerró desde otra pestaña o se inició sesión con otro usuario en el mismo navegador), al tocar **Desbloquear** el POS **no valida la contraseña** y muestra **«Tu sesión se cerró»** en lugar del formulario. Antes mostraba «Contraseña incorrecta.» aunque la clave estuviera bien.
- El botón **Volver a iniciar sesión** recarga el POS: si la sesión no existe pide el login y vuelve al mismo POS; si hay otra sesión abierta carga el POS de ese usuario.
- Una contraseña realmente mal sigue mostrando «Contraseña incorrecta.»; sin red o con el servidor caído sigue el aviso «No se pudo validar la contraseña. Revisá la conexión…».
- PENDIENTE: no se verificó si la venta en curso se recupera después de volver a iniciar sesión.

## Si te cierran la caja con el POS abierto
Si el encargado cierra tu caja desde otra computadora, el POS lo detecta solo (revisa cada **15 segundos**, y también al volver a la ventana) y **corta la venta**:
- Aparece el cartel **«La caja fue cerrada»**, que no se puede cerrar con Esc ni tocando afuera, y el teclado y el lector de códigos quedan frenados. Si estabas cobrando y justo se cerró, el cobro **no se registra** y aparece el mismo cartel.
- La venta en curso **queda resguardada**. Con **Entendido** el POS se recarga y te pide **abrir la caja de nuevo**.
- Al abrir la caja nueva, la venta **vuelve sola al carrito** en esa misma computadora. Si no aparece, está en **F1 → Ver ventas sin guardar**.
- **Cambio de turno:** si otro cajero abre su caja, en **F1 → Ver ventas sin guardar** ve tu venta con la marca **«caja cerrada»** y la puede **cargar al carrito sin pedir supervisor** (queda registrado quién la tomó). Descartarla sí pide supervisor. Si la tomó otro, tu POS limpia su copia para no registrarla dos veces.
- **Nadie puede registrar ventas con la caja cerrada**, tampoco un administrador: solo se puede vender sin caja si en el modal de apertura se tildó a propósito «continuar sin caja abierta» (usuarios con permiso de modificar ventas).
- PENDIENTE: no se verificó en pantalla con un usuario real; el tiempo de revisión se cambia en la configuración del servidor `PosCajaVigilante:Segundos` (ver [configuraciones](ayuda:referencia/configuraciones)).

## Factura que se abre sola al cobrar (débito, crédito, QR, transferencia)
Verificado en el código y grabado el 2026-11-10 (`POS.cshtml` `facturaElectronicaRequeridaPorFormaPagoPOS`, `factura-electronica.js`): si la empresa tiene certificado de AFIP, las formas de pago que el administrador tildó en el parámetro **Formas de pago que abren la factura al cobrar** (pantalla [Parámetros](ayuda:pantalla/Parametros.Index)) abren la **factura electrónica** de la venta apenas se cobra. **Sin cargar, rige lo de siempre: Débito, Crédito, QR y Transferencia** (no Efectivo). Se puede tildar también **Efectivo** (para facturar todo) o dejar **ninguna** (nada se abre solo). Cuenta corriente sin cobro nunca abre la factura; en un **pago mixto** se abre si alguna de las dos partes está tildada (la parte que no es efectivo, o el efectivo si está tildado). *Rediseño 2026-11-10, no probado en el POS real: PENDIENTE verificar con una venta de cada forma de pago.* **Alt+R** (o **Registrar factura**) la emite y vuelve a «Venta completada» con «Factura registrada correctamente» y su número. **Cerrar venta sin facturar** (o **Supr**, con el foco fuera de un campo) pide confirmar, deja la venta registrada sin factura y pasa a una venta nueva; se puede facturar después desde Mis ventas → Abrir detalle → Factura / Imprimir. Con las formas de pago no tildadas (por defecto efectivo y cuenta corriente) la factura se hace con el **5** de «Venta completada».

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

## Peso inestable de la balanza
Con un producto pesable y la balanza conectada, si se aprieta **Enter** (o **Agregar**) mientras el peso todavía se mueve, el producto **no se agrega** y aparece debajo de **Cantidad**, en amarillo, «Peso inestable, espere y presione Enter»; el recuadro de Cantidad se marca en amarillo y **el cursor queda en Cantidad**. No se abre ninguna ventana. Cuando la balanza se queda quieta el aviso se borra solo: se vuelve a apretar **Enter** y el producto se agrega. Desde el 2026-11-10 (antes salía una ventana que había que cerrar). Igual en el POS de los puntos de expendio.

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
Tipo, descripción, contador de billetes y detalle. Al guardar aparece el comprobante (cerrar sin imprimir, ticket, PDF o mail) y, al terminar, **el POS queda listo para la próxima venta**: con F7 no se abre Mis actividades (sí se abre si el egreso se carga con el botón **Nuevo egreso** desde Mis actividades).

![Video: registrar un egreso de caja con el contador de billetes](media:Ventas.POS/tutorial-egreso.mp4)

### Corregir y bonificar los productos del carrito
Cambiar la cantidad (C), eliminar una línea (F4 y E), el descuento a toda la venta al cobrar (/), bonificar por precio (B) y por porcentaje (/ /), y aplicar el mismo porcentaje a todos los productos.

![Video: cantidad, eliminar y bonificaciones de los renglones del carrito](media:Ventas.POS/tutorial-lineas.mp4)

### Mis ventas: revisar, reimprimir y corregir una venta
Mis ventas (F6), el panel desplegado, el detalle con **Factura / Imprimir** (reimprimir el ticket), **Modificar venta** (cambiar una cantidad y guardar manteniendo la forma de pago con Enter) y **Cambiar forma de pago** (de efectivo a cuenta corriente), con las reglas de quién puede modificar.

![Video: Mis ventas, reimprimir, modificar una venta y cambiar su forma de pago](media:Ventas.POS/tutorial-misventas.mp4)

### Recuperar una venta sin guardar
Qué pasa si se corta la luz o se cierra el navegador antes de cobrar: la venta vuelve sola en la misma computadora; si no, se recupera desde la campana o desde F1 → **Ver ventas sin guardar**.

![Video: recuperar una venta que quedó sin guardar](media:Ventas.POS/tutorial-borradores.mp4)

### Mi cierre de caja (F6)
El conteo de cierre del cajero: F6 → Mi cierre, el contador de billetes, guardar e imprimir el ticket del conteo, y cómo corregirlo («Actualizar mi cierre»).

![Video: cargar el cierre de caja con el contador de billetes](media:Ventas.POS/tutorial-cierre.mp4)

### Cobrar los expendios de Punto de Expendio (AvPag)
AvPag, buscar por número y Enter, Cargar en la fila, el filtro Persona para un presupuesto de días atrás, Cargar todos y Quitar expendios, y el cobro.

![Video: cobrar en la caja los expendios de Punto de Expendio](media:Ventas.POS/tutorial-expendios.mp4)

### Factura con débito, crédito, QR o transferencia
La factura electrónica que se abre sola al cobrar con débito: registrarla con Alt+R, la opción de cerrar sin facturar y el ticket de la factura.

![Video: la factura electrónica que se abre al cobrar con débito](media:Ventas.POS/tutorial-factura.mp4)

Voz: Piper, voz «daniela» (es_AR) de rhasspy/piper-voices, entrenada con el corpus OpenSLR 61 (licencia CC BY-SA 4.0).

## Notas
- Solo el ticket del post-venta ofrece el saldo. La **reimpresión desde el historial**, el **PDF** y el **email** no lo incluyen.
- El ticket facturado (con CAE) también lo admite: el saldo va después del total fiscal y antes de los datos fiscales.
- PENDIENTE: el resto de la pantalla (carrito, forma de pago, caja, borradores) no se documentó en esta revisión.
