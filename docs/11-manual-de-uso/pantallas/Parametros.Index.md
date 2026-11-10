---
pantalla: Parametros.Index
titulo: Parámetros
rol: admin
modulo: Mi empresa
orden: 2
alias: Parametros.Guardar
permiso: Admin de la empresa (para modificar)
revisada: 2026-11-10
---
## Para qué sirve
Ver y cambiar los **valores que ajustan cómo funciona el sistema para tu empresa**. Cada parámetro muestra un **título**, una **explicación** de qué cambia, su **unidad** (pesos, días) y, en letra chica, su **nombre técnico**. Están agrupados según la pantalla donde tienen efecto. Cualquier usuario que entre los ve en lectura; solo un administrador puede modificarlos.

## Cómo se usa
1. La pantalla abre en **modo lectura**. Tocá **Modificar** (solo administradores).
2. Cambiá lo que necesites. Cada fila cambiada se **resalta** y abajo aparece cuántos cambios llevás (*«2 cambios sin guardar»*).
3. Tocá **Guardar**. Aviso: *«Los parámetros de la empresa se guardaron correctamente.»* **Descartar** vuelve a la pantalla sin guardar nada.

## Qué parámetros hay
**Punto de venta (POS)**
- **Saldo mínimo a informar** (`saldoMinimoInformar`, en pesos): si el saldo de cuenta corriente del cliente es igual o menor a este monto (en deuda o a favor), el [POS](ayuda:pantalla/Ventas.POS) no lo muestra ni ofrece sumarlo al ticket. Con **0** se informa cualquier saldo distinto de cero. Vacío = **100**.
- **Formas de pago que abren la factura al cobrar** (`formasPagoFacturaAutomatica`, una casilla por forma de pago): al cobrar en el [POS](ayuda:pantalla/Ventas.POS) con una forma tildada se abre la factura electrónica de la venta; con las demás se pasa a «Venta completada». Opciones: Efectivo, Débito, Crédito, QR y Transferencia (Cuenta corriente sin cobro nunca abre la factura). **Sin cargar: Débito, Crédito, QR y Transferencia**, como siempre; tildar todas = facturar todo; ninguna = no se abre nunca sola (la factura se hace con el **5** de «Venta completada»). En un pago mixto se abre si alguna de las dos partes está tildada. Requiere certificado de AFIP. Solo en bases Postgres. *Verificado el 2026-11-10 en una copia local: el valor guardado llega al POS (Efectivo abre la factura, QR no, Cta. Cte. no, mixto sí). PENDIENTE: una venta real de punta a punta.*
- **Código del producto de precio libre** (`codProdGenerico`): código del producto genérico, el mismo que se compara con lo que se ingresa en el POS y en el punto de expendio para vender un importe suelto; el vendedor escribe el precio y se carga ese producto con ese precio. Con el sufijo `G` seguido de un número se usa el producto cuyo código es este más ese número. **Es un número entero mayor a cero de hasta 13 dígitos** (el campo solo acepta dígitos; con letras, decimales o más de 13 dígitos no se guarda). Vacío = no hay producto de precio libre. *PENDIENTE: confirmar con el usuario el formato exacto que tipea el vendedor.*

**Tickets**
- **Ocultar datos de la empresa en el ticket no fiscal** (`ocultarEncabezadoTicketNoFiscal`, Sí/No): con **Sí**, el ticket X del POS no imprime el nombre de la empresa ni los slogans. El ticket facturado, los PDF y los demás tickets no cambian. Sin cargar = **No**.

**Fechas y listados**
- **Días hacia atrás por defecto** (`diasLimitFechaDesde`, en días): desde qué fecha arrancan los listados de Stock, Elaborados y Movimientos (ej. 7 = la última semana), y hasta cuántos días atrás se puede llevar la fecha de una venta en el POS.

## Reglas
- Un usuario que no es administrador y llega a guardar recibe *«solo un administrador puede modificar los parámetros de la empresa»* y no se cambia nada.
- Un valor numérico inválido (texto que no es un número) se rechaza con un mensaje bajo el campo y no se guarda ninguno hasta corregirlo. Se aceptan **coma o punto** como decimal. **Dejarlo vacío es válido**: el sistema usa el valor por defecto de cada parámetro (se indica bajo el nombre).
- **No se muestran todos los parámetros de la base.** Algunos los usa solo la aplicación de escritorio (redondeo del POS, kilos máximos del ajuste por tarjeta, pedir usuario al cargar elaborados y movimientos, mayúscula en productos), otros no los lee ninguna pantalla (pedir usuario en stock, minutos para modificar la última venta) y otros son internos del sistema o se editan en otra pantalla (ej. el ajuste por forma de pago, en [Productos](ayuda:pantalla/Productos.Index)). Guardar esta pantalla **no cambia** los que no se muestran.
- Un parámetro nuevo que se agregue al sistema y todavía no tenga texto de ayuda aparece igual, en el grupo **Otros**, con su nombre técnico y la descripción original.

Ver también: [mapa de configuraciones](ayuda:referencia/configuraciones)
