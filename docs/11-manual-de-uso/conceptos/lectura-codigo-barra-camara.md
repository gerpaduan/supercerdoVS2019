---
titulo: Leer códigos de barra con la cámara del celular
rol: usuario
modulo: Cargas y ventas
orden: 30
revisada: 2026-11-20
---
# Leer códigos de barra con la cámara del celular

## Para qué sirve
En el POS, el POS de expendio, Movimientos, Stock, Compras y el alta de producto, el botón del código de barra abre la cámara del celular (o de la PC) y completa el código del producto. Esta página explica cómo apuntar, qué códigos lee y por qué a veces pide «confirmar».

## Cómo apuntar para que lea rápido
1. **Poné el código en el centro del recuadro de la cámara.** El recuadro muestra la franja central de la imagen (desde 2026-11-20 es más alto: 120 px) y es lo que el sistema analiza; la línea roja marca el medio.
2. **Mantenelo quieto un instante** frente a la cámara. Cuando aparece *«Código detectado. Mantené el código quieto frente a la cámara para confirmar»*, no lo muevas: la confirmación llega sola en medio segundo.
3. **Distancia:** que el código ocupe buena parte del ancho del recuadro, sin tocar los bordes. Si queda borroso, alejá un poco el celular: casi todos no enfocan a menos de 10–15 cm. En los celulares que lo permiten, el sistema aplica solo un zoom moderado (1,5x) para que puedas leer desde más lejos.
4. **Luz:** evitá reflejos sobre el envoltorio. Si hay poca luz, usá el botón **Flash**.
5. **Envases curvos o arrugados** (sachets, latas): estiralo o girá el producto hasta que las barras queden derechas.

## Qué códigos lee
EAN-13, EAN-8, UPC-A (12 dígitos) y Code 128. Cada lectura se controla con el **dígito verificador** del código; si no cierra, no se acepta.

Un EAN-13 que empieza con 0 se guarda y se lee con **12 dígitos** (UPC-A). Desde 2026-11-20 el alta de producto, Compras, Movimientos y Stock lo aceptan; antes decían «Código inválido».

## Por qué pide «confirmar» y las pausas
- **Alta de producto, catálogo global, Movimientos, Stock y Compras** exigen **dos lecturas iguales seguidas** antes de aceptar, para no cargar un producto equivocado por una lectura errada. En el alta de producto y en el catálogo global, la segunda lectura tiene que llegar **al menos 0,5 s después** de la primera (antes tardaba 2 s).
- Tras aceptar un código hay una **pausa de 2 s** en la que no se acepta otra lectura, para no cargar el mismo producto dos veces. En el alta de producto la cámara se cierra sola; en Movimientos, Stock y Compras queda abierta para el siguiente producto.
- **POS y POS de expendio:** el mismo código no se vuelve a cargar antes de 2 s. Si el producto no existe, el alta rápida solo se ofrece con la segunda lectura igual.

## Si no lee
- *«Código inválido (solo EAN-8, EAN-13 o UPC-A)»*: el código leído no cierra con su dígito verificador (lectura errada o etiqueta dañada). Volvé a enfocar; si es un código propio de la casa, cargalo a mano.
- *«No se pudo iniciar la cámara»*: el navegador no tiene permiso para usarla; habilitalo desde el candado de la barra de direcciones.
- Con un código que nunca lee, probá con otro producto igual: si ese sí lee, la etiqueta está dañada o impresa muy chica.

## PENDIENTE
Los ajustes del 2026-11-20 (recuadro más alto, lectura de la franja central, zoom automático) se verificaron con una cámara simulada y la librería real, **no con celulares físicos**. Falta confirmar en un Android y en un iPhone con productos reales.
