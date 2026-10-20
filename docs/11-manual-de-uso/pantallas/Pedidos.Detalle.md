---
pantalla: Pedidos.Detalle
titulo: Pedido (detalle)
rol: admin
modulo: Stock
orden: 6
permiso: Stock.VerStock para ver, imprimir y mandar; Modificar necesita Stock.AddOrEditStock (o ser administrador)
revisada: 2026-10-19
alias: Pedidos.Ticket, Pedidos.TicketPayload, Pedidos.Pdf, Pedidos.Excel, Pedidos.DatosEmail, Pedidos.EnviarEmail
---
## Para qué sirve
Ver **un pedido guardado completo**: qué se pidió, a qué sucursal, cuánto cuesta aproximadamente y qué cambios tuvo. Desde acá se **imprime, se manda por mail o se exporta**.

## Qué muestra
- **Cabecera**: número, fecha, quién lo hizo, nota, **total estimado** y cuántas líneas quedaron sin precio de compra.
- **Tabla**: una fila por producto con **la cantidad de cada sucursal** (vacío = no se pidió a esa sucursal), el **Total** del producto (en productos pesables, junto al total aparece `≈ N un.`, calculado con el peso promedio), el **precio de la última compra** (con el proveedor de esa compra al dejar el mouse encima) y el **importe estimado**. *parcial* junto a un importe significa que alguna sucursal de ese producto no tenía precio y no suma.
- **Historial**: alta y cada modificación, con fecha, quién la hizo, **quién la autorizó** (si hizo falta) y un resumen de lo que cambió (*Cambió Pernil (San Martín): 4 → 5*, *Se agregó…*, *Se quitó…*, y el total antes y después).

## Botones
| Botón | Qué hace |
|---|---|
| **Pedidos** | Vuelve al listado. |
| **Ticket** | Imprime el pedido en la impresora térmica (58 o 80 mm; se recuerda la última medida usada). Si no hay agente de impresión, abre la impresión del navegador. |
| **PDF** | Descarga el pedido en PDF. |
| **Enviar por email** | Abre el formulario con asunto y mensaje ya escritos (se pueden cambiar); pide el **email destino** y manda el PDF adjunto. |
| **Excel** | Descarga una planilla con las mismas columnas. |
| **Modificar** | Solo aparece si tu usuario puede modificar pedidos. Ver [Modificar pedido](ayuda:pantalla/Pedidos.Editar). |

Ticket, PDF, mail y Excel dicen **"estimado"** en los importes, aclaran de dónde sale el precio y avisan cuántas líneas no suman por no tener precio. El Excel suma dos columnas al final: **Proveedor de la compra** y **Fecha del precio**.

## De dónde sale el precio
Del **precio de la última compra** que tiene registrado cada producto en el sistema, **por kilo**, aplicado a la cantidad pedida. **No se distingue proveedor**: se toma el precio de la última compra de ese producto **sea cual sea el proveedor** (si le comprás a varios, el precio real puede ser otro). Debajo de cada precio se ve **de qué proveedor y de qué fecha** es esa compra. Esta aclaración figura al pie de la tabla y en el ticket, el PDF, el Excel y el mail.

- El precio se **guarda en el pedido** al momento de armarlo: si después cambia, el pedido no cambia.
- **⚠ Precio viejo**: si la compra que dio el precio es de hace **más de 90 días** respecto de la fecha del pedido (el límite se cambia en `Pedidos:DiasPrecioViejo`, ver [configuraciones](ayuda:referencia/configuraciones-plataforma)), el precio se marca con ⚠ y los días: puede estar desactualizado. En el ticket se marca con `(!)` y en el Excel la fecha va en naranja.
- **Total estimado (parcial)**: cuando hay productos **sin precio de compra** (no suman), el total se rotula *parcial*: es un mínimo, no el costo completo.
- **Una compra de hace más de 365 días no da precio** (`Pedidos:DiasPrecioMaximo`, ver [configuraciones](ayuda:referencia/configuraciones-plataforma)): el producto queda *sin precio* y no suma, porque un precio de hace años subestima el total por la inflación. Los productos que no se compran como tales (cortes que salen de la media res, elaborados) tampoco tienen precio; por eso muchos salen *sin precio*.

## Si algo no anda
- *Enviar por email* dice *Ingrese un email válido*: revisá la dirección. Si dice *No se pudo enviar*, el correo del sistema no está configurado o falló el envío; avisá a soporte (queda registrado el error).
- El ticket no sale: probá con la otra medida (58/80 mm) o con **PDF**.
