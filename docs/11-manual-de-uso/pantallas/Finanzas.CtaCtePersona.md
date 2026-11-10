---
pantalla: Finanzas.CtaCtePersona
titulo: Cuenta corriente de una persona
rol: usuario
modulo: Finanzas
orden: 2
permiso: PENDIENTE (no verificado contra el código en esta revisión)
alias: Finanzas.AutorizarModuloCtaCte
revisada: 2026-11-09
---

## Para qué sirve
Ver los **movimientos** de la cuenta corriente de una persona (ventas, pagos y cobros, compras) con el **saldo** después de cada uno, y cargar un pago o cobro. Se llega desde [Cuentas corrientes](Finanzas.CtasCtes.md).

## Cuenta de producción: identificarse para entrar
Si la sesión es la **cuenta compartida de producción**, al abrir una cuenta corriente (por ejemplo desde el icono en [Personas](Personas.Index.md)) aparece **Autorizate para ver la cuenta corriente**: elegí tu usuario y escribí tu contraseña.
- El usuario tiene que tener el permiso **Cuentas corrientes** (o **POS - Ctas.Ctes Persona**). Si no lo tiene aparece «Ese usuario no tiene permiso para ver cuentas corrientes» y no se entra. Con la contraseña mal se cuentan intentos y, pasados varios, se bloquea un rato.
- Los permisos y los días que podés ver se toman de **tu usuario**, no de la cuenta compartida.
- La autorización **vale mientras estés en Finanzas**. Al ir a cualquier otra pantalla (Personas, Ventas…) caduca y hay que identificarse de nuevo.
- **Cuentas reservadas:** si el usuario con el que te identificaste tiene el permiso **Ver cuentas corrientes reservadas** (o es administrador), ves la cuenta reservada **completa**: el saldo, todos los movimientos y sus ítems, sin el aviso amarillo. Si solo tiene *Cuentas corrientes* o *POS - Ctas.Ctes Persona*, la ves recortada, como cualquier usuario restringido: solo lo que cargó desde la apertura de su caja y sin saldo. Este permiso rige **únicamente dentro de Finanzas**: al abrir una venta o una compra de una cuenta reservada desde acá se vuelve a evaluar con la cuenta compartida y se deniega.
- **Cancelar** vuelve a la pantalla de donde venías. Abierta desde el POS (F2) no pide esto: el POS ya tiene su propio operador, y el permiso que cuenta es **POS - Ctas.Ctes Persona** de ese operador (sin él, «No tenés permiso para ver cuentas corrientes (POS - Ctas.Ctes Persona).»; los días atrás que ves son sus **Días ver**). Si falta el permiso, el POS ofrece **Autorizar con otro usuario** (vale hasta cerrar la cuenta corriente). Ver [POS](Ventas.POS.md).

## Filtros y acciones
- **Desde** + **Filtrar**: muestra los movimientos a partir de esa fecha. **Mostrar anulados** suma los registros anulados.
- **Agregar Pago / Cobro** abre la carga de un pago (también con **Alt+Enter**).
- **PDF**, **Excel** y **Enviar por mail** solo aparecen si tu usuario puede exportar la cuenta corriente.
- **Tocar un movimiento lo despliega** (también con la flecha de la derecha): muestra los **ítems** de la venta o de la compra, o cómo se pagó un cobro o pago, y a un costado la sucursal, quién lo creó y modificó y las observaciones. Pueden quedar varios abiertos. Seleccionar texto de una fila (para copiarlo) no la despliega.
- Para ir al registro de origen está el botón del panel: **Abrir venta** (pantalla de la venta), **Abrir compra** (se abre en una ventana) o **Abrir pago / cobro** (el formulario de edición).
- Etiquetas: **Modificado**, **Versión anterior**, **ELIMINADO** y **Anulado por eliminación** marcan las distintas versiones de un registro.

## En la computadora
- El encabezado, el buscador **Desde / Filtrar**, el interruptor y los botones (**Agregar Pago / Cobro**, **PDF**, **Excel**, **Enviar por mail**) tienen el tamaño compacto del resto de las pantallas; el panel desplegado usa la misma letra chica. En el celular se mantienen los tamaños grandes para tocar.
- Arriba de la tabla hay un **resumen del período**: saldo anterior a la fecha del filtro, **Débitos** y **Créditos** (con la cantidad de movimientos) y **Saldo actual**. Si tu usuario no puede ver saldos (o la cuenta es reservada), las tarjetas de saldo no se muestran y el saldo de cada fila es `***`.
- Columnas: **Fecha**, **Operación** (etiqueta con ícono: **Venta**, **Compra**, **Cobro** o **Pago**; un pago que suma a la cuenta es un Cobro y uno que resta es un Pago), **Comprobante**, **Detalle**, **Débito** (en rojo, lo que la persona paga o se le descuenta), **Crédito** (en verde, lo que se le suma) y **Saldo** (en rojo si es negativo). El importe se muestra sin signo en la columna que corresponde.
- Debajo del detalle, en gris, van el número de registro, **cuántos ítems y kilos** tiene una venta o compra, y la **sucursal** (`#192973 · 2 artículos · 101,3 kg · San Martín`; una compra de media res dice `5 medias · 444 kg`). Si el registro tiene observaciones se ve un ícono de comentario junto al detalle (al pasar el mouse se leen). La primera fila, **Saldo Anterior**, va sombreada y no suma a los totales.
- La fila final **Totales del período** suma los Débitos y los Créditos de las filas mostradas (con **Mostrar anulados** incluye las versiones anteriores y los contraasientos, que es lo que hace cuadrar el saldo): saldo anterior − débitos + créditos = saldo actual.
- El panel desplegado tiene dos cajas. A la izquierda, los **ítems**: en una **venta**, cada artículo con cantidad × precio, bonificación o recargo y el total; en una **compra**, cada producto (o cada media res) con kilos × precio y el total; en un **cobro o pago**, cuánto fue en efectivo y cada cheque (banco, número y fecha de cobro). Encima va el tipo de compra y el remito, o el recibo. A la derecha, **Sucursal**, **Creado**, **Actualizado**, **Observaciones** y el botón **Abrir …**.
- Los **precios de una compra** (precio por kilo y subtotales) solo los ve quien tiene permiso para **ver compras**; sin ese permiso se ven los productos y los kilos. En una cuenta de producción se evalúa el permiso del usuario con el que te identificaste.
- Una fila marcada **Versión anterior** o **ELIMINADO** no trae ítems: avisa que están en la versión vigente del registro. Los movimientos que no son ventas, compras ni pagos (por ejemplo una liquidación) se despliegan sin ítems.
- Si el panel no puede cargar, muestra el motivo (sesión vencida, sin permiso, cuenta reservada) y se vuelve a intentar al cerrar y abrir la fila.
- El estado (**Versión anterior**, **ELIMINADO**…) aparece debajo de la etiqueta de la operación.
- Se ve igual abierta desde el POS (F2).
- Si la ventana es angosta y la tabla se desplaza hacia el costado, el panel queda siempre dentro de la parte visible.

## En el celular
- Arriba, el nombre de la persona y, debajo, el **saldo** en grande (verde o rojo).
- **Agregar pago / cobro** ocupa todo el ancho; PDF, Excel y **Mail** van en una fila.
- Cada movimiento es un renglón de tres líneas: operación y tipo (Crédito / Débito) con el **importe** a la derecha; fecha y número de documento con el **saldo** a la derecha; y sucursal, detalle (con la cantidad de ítems y kilos entre paréntesis), el ícono de observaciones y **Detalle**. Tocar el renglón (o **Detalle**) lo despliega; los ítems y los datos del movimiento quedan uno debajo del otro.
- El resumen y los totales de la computadora no se muestran en el celular.

## Desde el POS
La misma lista se ve en el celular al abrir las cuentas corrientes desde el POS (F2), en modo claro y oscuro. En la computadora se ve la misma tabla con Débito / Crédito que en la página.

## Resto de la pantalla
PENDIENTE: documentar permisos, el recorte de fecha por permiso, el uso desde el POS y el envío por mail (no verificado contra el código en esta revisión).

## Titulares y control fiscal del proveedor (desde 2026-11-09; solo bases Postgres, solo con 2 o más titulares)
- **Titular** (filtro): deja solo los movimientos de ese titular; el **saldo anterior y el saldo acumulado** se calculan solo con lo filtrado. Cada compra y cada pago muestran su titular en la línea gris. Pensado para proveedores: una venta cuenta como del titular principal.
- **Desde el inicio:** pone la fecha de la primera compra de la persona (el control fiscal usa el mismo período que *Desde*; si la factura llega en otro mes que el pago, conviene mirar más).
- **Deuda por titular** (personas con compras): saldo anterior, comprado, pagado y saldo de cada titular, con el **total**. Los saldos por titular pueden no cerrar en cero si se pagó con un titular lo comprado con otro; **el total es la deuda real** y coincide con el saldo de arriba.
- **Control fiscal:** solo lo ven el **administrador** y quien tiene **Puede editar** en *Pagos* del menú (el permiso «POS - Ctas.Ctes Persona» no alcanza). Por titular: *Facturado neto* (facturas + notas de débito − notas de crédito), *Pagado bancarizado* (transferencia, débito, crédito, QR y cheques), *Facturado − bancarizado* (**en rojo con ⚠** si lo pagado bancarizado supera lo facturado; solo avisa, no bloquea nada), *Pagado en efectivo* (efectivo y «Otro») y *Sin factura* (comprado − facturado).
- **PDF y Excel (CSV):** respetan el filtro de titular, agregan el titular y el resumen, y el control fiscal solo si tenés el permiso. El que se **envía por mail** nunca lleva el control fiscal.
- PENDIENTE: no se probó en el celular; en el modal del POS esta pantalla queda como siempre (sin titulares).
