---
pantalla: Compras.Editar
titulo: Cargar, modificar o eliminar una compra
rol: usuario
modulo: Compras
orden: 2
permiso: Compra.NuevaCompra (alta) y Compra.ModificarCompra (modificar y eliminar); a los usuarios de producción les pide elegir el operador de Compras antes de entrar
revisada: 2026-11-20
alias: Compras.NuevaCompra, Compras.ModificarCompra
---
## Para qué sirve
Cargar una compra nueva o abrir una ya guardada para verla, modificarla o **eliminarla**. Esta página documenta por ahora **Eliminar** y cómo se ve **en el celular**; el resto de la pantalla (proveedor, productos, medias reses, cuenta corriente, guardar) está pendiente (**PENDIENTE**: no se revisó en esta versión).

## En la computadora (pantalla de 992 px o más)
Pensado para ver más campos y más líneas sin bajar con la rueda. Es igual en la pantalla completa y en los dos modales (**Nueva compra** del POS con F5 y la compra que se abre desde la cuenta corriente de un proveedor).
- **Datos principales en una fila:** Tipo compra (solo si la empresa lo usa), Sucursal, Fecha, Proveedor, Nro. remito/factura y el interruptor **Cuenta corriente**, que siempre queda a la vista (antes se salía de la pantalla en ventanas de 1366 px o menos). Con ventanas de **1270 px o más** son una sola fila; en ventanas más angostas (hasta 992 px) pasan a **dos renglones**: arriba Tipo, Sucursal, Fecha y Cuenta corriente, y abajo Proveedor y Nro. remito/factura. El CUIT del proveedor se lee al lado de la palabra «Proveedor».
- **Barra «Agregar producto» (o «Agregar media»), en dos renglones.** Arriba: el título, el interruptor «Cargar varias veces» (pasá el mouse para ver qué hace), los interruptores **Desc./Recargo**, **IVA** y **Margen / precio de venta**, y **Agregar producto** a la derecha. Abajo, la fila de carga: Código, Producto, Cantidad, Precio, y Subtotal; el nombre del producto toma todo el ancho que sobra. Anchos mínimos pensados para que se lean **15 caracteres** del código, unos **37** del nombre del producto con descuento e IVA encendidos (39 sin ellos), **7** de la cantidad y **9** del precio (estimado: PENDIENTE medirlo en cada pantalla).
- **Ajustes:** al encender un interruptor, su ficha se marca con borde azul y el campo aparece donde corresponde:
  - **Desc./Recargo** (porcentaje; negativo descuenta, positivo recarga): en la fila de carga, **después de Precio**.
  - **IVA** (porcentaje): en la fila de carga, **después de Precio** y de Desc./Recargo si está encendido. El Enter va en ese orden: Precio → Desc./Recargo → IVA → Margen → Agregar.
  - **Margen / precio de venta:** una franja propia **debajo de la fila de carga**, con el precio actual del producto, el modo (Margen % o Nuevo precio de venta) y los dos campos. No es parte de la compra: es el precio al que se va a vender, por eso queda aparte del Subtotal.
  Con Desc./Recargo e IVA encendidos y una ventana de menos de unos 1300 px, el Subtotal baja a un renglón propio.
- **Lista de líneas:** una sola línea por producto (el código va al lado del nombre; si el nombre no entra se corta con «…»), con letra de 12,9 px (el código en gris, un punto menos). Entran **13 productos a la vista** con una ventana de 685 px de alto o más (745 px o más si la cabecera ocupa dos renglones; en una de 630 px, 10); con más productos la lista tiene su propio scroll con el encabezado fijo, así que el resto de la pantalla no se mueve y los botones de abajo no se esconden. Columnas: #, Detalle, Cantidad, Precio, **Total**, **P. venta** y **Margen**. Estas dos últimas **no forman parte de la compra** (actualizan el precio de venta del producto): van después del Total, con fondo violeta tenue y una línea punteada que las separa.
- **Números en formato argentino:** punto para los miles y coma para los decimales (1.234,50) en el total de la compra, el total de cantidad, el subtotal, el precio actual del producto y todas las columnas de la lista. Los campos que se escriben no cambian: el Precio ya pone los puntos mientras escribís; Cantidad, Desc./Recargo, IVA y Margen se escriben como siempre. Lo que se guarda es lo mismo de antes.
- **Fila resaltada y orden:** al pasar el mouse por una fila se pinta de azul tenue. Se ordena tocando los encabezados **#**, **Detalle**, **Código** (los dos enlaces del encabezado Detalle) y **Cantidad**; otro toque invierte el orden y una flecha ↑/↓ marca la columna. Arranca en **# descendente** (lo último cargado arriba). Las medias reses se ordenan como «Media», sin código. El número **#** es el de carga y no cambia al ordenar; quitar una línea quita la correcta y el orden en que se guarda es siempre el de carga.
- **A la derecha** de la lista: total de la compra (grande), items y cantidad, cantidad de medias reses y observaciones.
- **Botones de abajo** (Guardar / Modificar / Eliminar / Cancelar) quedan siempre a la vista.
- En el modal del POS la lista de líneas se ve como en la pantalla completa (antes mostraba, repetidos, los textos pensados para el celular).

## En el celular
- **Ajustes:** Desc./Recargo, IVA y Margen / precio de venta están guardados detrás del botón **Ajustes**, al lado de **Agregar producto**. Tocalo para abrirlos y otra vez para cerrarlos. Con el panel cerrado, el botón muestra los ajustes que están encendidos (por ejemplo «IVA 21%»), así sabés que se van a aplicar a la próxima línea. El panel no se cierra solo al agregar un producto. Desde el 2026-10-30 los **campos** de Desc./Recargo e IVA aparecen al final de la fila de carga (arriba de los interruptores) y el de Margen debajo de ellos, siempre con los Ajustes abiertos.
- Los importes también salen con puntos de miles y coma decimal (1.234,50).
- **Cantidad, Precio y Subtotal** van en una misma fila.
- **Productos cargados:** se ven como una lista. Arriba, el número de línea, el producto y el total; abajo, el código con cantidad × precio (en las medias, la tropa con kilos × precio). Si la línea tiene descuento/recargo, IVA, precio de venta o margen, se muestran en un renglón más. El tacho de la derecha quita la línea.
- **Orden:** arranca con **lo último cargado arriba**; con el selector de la derecha se cambia a «Primeros primero». En la computadora se ordena tocando los encabezados (ver arriba).

## Modo ver: cómo se nota que no se puede editar (desde 2026-11-14)
Al abrir una compra **ya guardada** queda en **modo ver**. El bloque **Agregar producto** (o **Agregar media**) **no se muestra**: no hay dónde escribir. En su lugar aparece una franja gris con *«Estás viendo este registro. Para cambiarlo, presioná Modificar»*. Al tocar **Modificar** (abajo a la derecha) la franja desaparece y vuelve el bloque. Una compra **nueva** no pasa por modo ver. Es el mismo criterio que en Stock, Movimientos y Elaborados.
- Diferencia con esas pantallas: en Compras no hay etiqueta «Solo lectura» arriba ni botón **Modificar** arriba, ni aviso al intentar escribir; solo la franja.
- PENDIENTE: probarlo en pantalla real (se verificó la compilación, no el aspecto).

## Corregir el precio o la cantidad de una línea (desde 2026-11-14)
Si cargaste un producto (o una media) con un precio o una cantidad equivocados, **no hace falta quitarlo y cargarlo de nuevo**.
1. Tocá el **lápiz** azul de la línea (al lado del tacho) o tocá la fila. La línea se marca en azul y se abre un panel debajo con **Cantidad** (en las medias, **Kg media**) y **Precio**, y el total de la línea en vivo.
2. Cambiá lo que haga falta y apretá **Aplicar** (o Enter). **Cancelar** o Esc cierran el panel sin cambiar nada.
3. Si cambiaste el **precio** y hay **otras líneas iguales**, el panel las lista con una casilla y pregunta si querés cambiarles el precio también:
   - Producto: «iguales» son las líneas del **mismo producto**. Vienen tildadas las que tenían **el mismo precio** que la que corregís; las de otro precio quedan sin tildar (pueden ser legítimas).
   - Media res: «iguales» son las otras medias con **el mismo precio**.
   - **Solo esta** cambia únicamente la línea elegida; **Aplicar a las marcadas** cambia también las tildadas. La **cantidad** se cambia siempre solo en la línea elegida.
4. La fila cambiada destella en verde y el total de la compra se recalcula.

Qué hay que saber:
- **No se guarda nada todavía:** el cambio queda en la compra recién al tocar **Guardar compra**, igual que al quitar o agregar una línea. En una compra ya guardada, el panel te recuerda que al guardar se recalculan el total y la cuenta corriente del proveedor, y que el egreso de caja solo se actualiza si la compra se abre desde el POS (si esa caja ya está cerrada se pide confirmar).
- **Descuento/Recargo e IVA:** el precio que escribís es el **final** (el de la columna Precio). Si la línea tenía descuento, recargo o IVA, esos ajustes se **quitan** (el panel lo avisa).
- **P. venta y Margen:** si la línea tenía un precio de venta cargado, **se mantiene** y el margen se recalcula con el precio nuevo. Si el precio nuevo es mayor que el precio de venta, el margen quedaría negativo y **no se aplica**: quitá la línea y cargala de nuevo con el precio de venta que corresponda. Si la línea actualiza el precio de venta del producto, el panel lo avisa.
- En una compra **ya guardada**, el lápiz no se ve hasta tocar **Modificar**.
- Solo se corrigen precio y cantidad; para cambiar el **producto** de una línea hay que quitarla y cargarla de nuevo.
- PENDIENTE: no se verificó en el código qué pasa con el **stock** cuando se modifica una compra guardada; el panel no dice nada al respecto.

## Eliminar una compra
El botón rojo **Eliminar** aparece al abrir una compra **ya guardada** para quien tenga permiso de modificar compras: en la pantalla completa y, desde **F6 → Mis actividades** del POS, en las compras propias de la caja abierta (ver [Ventas.POS](ayuda:pantalla/Ventas.POS)); no dentro de la cuenta corriente del proveedor.

1. Tocá **Eliminar**. Se abre una ventana con el **resumen** de la compra (tipo, fecha, sucursal, proveedor, remito, importe, líneas y kilos), **todas sus líneas** y **qué va a pasar**.
2. Escribí el **motivo** (de 10 a 300 caracteres; es obligatorio y queda registrado).
3. Confirmá tu **usuario y contraseña**. Aunque ya estés logueado, **se pide siempre**, en cada eliminación. Con tu propio usuario no se puede confirmar otro usuario. Si trabajás con la cuenta de producción, elegí tu nombre de la lista y escribí tu contraseña. Tenés 5 intentos; al 5.º error se bloquea 5 minutos (ver [intentos de clave al autorizar](ayuda:concepto/intentos-de-clave-al-autorizar)).
4. **Eliminar definitivamente**. La compra se borra y volvés al listado (desde el POS, a **Mis actividades**).

### Qué pasa con la compra
- **Cuenta corriente del proveedor.** Si la compra estaba en cuenta corriente, **no se borra su renglón**: se agrega un **asiento contrario** («ELIMINADO - …») con la fecha original de la compra. En el extracto del proveedor quedan los dos renglones y el saldo vuelve a ser el de antes de la compra. La ventana muestra antes el saldo actual y cómo queda.
- **Caja.** Si la compra se cargó desde el POS, se registra un **egreso opuesto «ELIMINADO: …» con la fecha de hoy**: con el importe en negativo si salió plata de la caja, o en $ 0,00 si la compra era en cuenta corriente (así queda a la vista en Mis actividades). Si esa caja ya está cerrada, el sistema avisa y pide confirmar (el cierre de esa caja queda sin ese reverso).
- **Precios.** Si la compra había actualizado el precio de venta de algún producto, ese precio **no se revierte**. El «último precio de compra» de cada proveedor se recalcula con la compra anterior.
- **Constancia.** Quedan guardados en [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index) quién la eliminó, cuándo, el motivo y todo lo que tenía. Los administradores reciben un aviso en la campana.
- **No se puede deshacer** desde el sistema.

### Cuándo no se puede eliminar
- Si la compra tiene **pesajes vinculados**, la ventana lo dice y no deja confirmar: desvinculalos o eliminalos antes.
- Si tu usuario no tiene permiso para esa fecha, el sistema lo avisa al confirmar.
- Si otra persona ya la eliminó, lo avisa y no hace nada más.

Si abrís una compra que ya fue eliminada (por ejemplo desde un link guardado), el sistema te muestra quién la eliminó, cuándo y por qué (los administradores ven el detalle completo).

## Titular y factura del proveedor (desde 2026-11-09; solo bases Postgres)
- **Cuándo aparece:** en la pantalla completa (no dentro del POS ni del modal de cuenta corriente), en el recuadro *Facturación del proveedor*.
- **En una compra nueva:** si la empresa tiene 2 o más titulares, elegís **Comprado a nombre de** (por defecto el principal). Tildando **La factura vino con la mercadería** se despliegan los datos de la factura (tipo, punto de venta, número, fecha, total, IVA, neto, otros impuestos) y la compra se guarda ya *facturada*. Si no la tildás, la compra queda **pendiente de factura** y la cargás después. Con un porcentaje de IVA elegido, el neto y el IVA se calculan desde el total; **Igual al total de la compra** copia el total de la compra. Los decimales van con punto o coma, sin separador de miles. Si la factura tiene un dato mal, **la compra se guarda igual** y te avisa que la cargues desde *Comprobantes*.
- **Al modificar una compra:** el recuadro es de solo lectura (estado de factura, comprobantes y totales: *Total compra*, *Facturado neto*, *Sin factura* o *Facturado de más*). El botón **Comprobantes** abre el modal para cargar, editar o eliminar comprobantes (con motivo), cambiar el titular (pide confirmar si ya hay comprobantes) o marcar **no lleva factura**. **Cargar o cambiar comprobantes no modifica la compra, el stock ni la cuenta corriente** y se puede hacer con la caja cerrada. Necesita el permiso de modificar compras.
- **Reglas:** una sola factura por compra (si el proveedor manda otra, es otra compra); las notas de crédito y débito necesitan la factura; el mismo número no se carga dos veces para el mismo proveedor; neto + IVA + otros tiene que dar el total. La deuda con el proveedor sigue siendo el **importe de la compra**, no el de la factura.
- PENDIENTE: el borrador local de esta pantalla no guarda los datos de la factura del alta; la pantalla no se probó en el celular.

## Compra en curso en el POS (desde 2026-11-17)
La **Nueva compra** del POS (**F5**) y **Modificar compra** se abren en su propia ventana, que **no se cierra** al tocar afuera ni con **Esc**:
- **Minimizar** (arriba a la derecha) vuelve al POS y deja la compra «en curso». Podés seguir vendiendo; arriba aparece el botón amarillo **Compra en curso**: tocalo y la compra vuelve tal como la dejaste.
- **Descartar** tira lo cargado sin guardar (pide confirmar).
- **Una sola compra a la vez:** con una compra en curso, **F5** o **Modificar compra** no abren otra: te devuelven a la que está y avisan «Ya hay una compra en curso». Guardala o descartala primero.
- Al **Guardar compra** la ventana se cierra y el aviso desaparece. Si recargás o cerrás la pestaña del POS con una compra en curso, el navegador avisa.
- Si abrís **Modificar** desde **F6 → Mis actividades**, esa pantalla queda abajo y la ves al guardar o minimizar.
- PENDIENTE: no se verificó en el navegador; descartar no cierra el borrador del servidor (queda como «interrumpido» sin latido).

## Botones sin foco del teclado (2026-11-19)
- **Guardar compra**, **Cancelar** y **Modificar** (atajos: Alt+G o Alt+Enter para Guardar o Modificar, Alt+C para Cancelar). **no toman el foco**: Tab no se detiene en ellos y, al hacer clic, el cursor sigue donde estaba (no queda el recuadro de foco ni un Enter o Espacio posterior los vuelve a activar). Solo responden al **clic** o al **atajo** de la pantalla.
- **Enter dentro de un campo no guarda** la pantalla (antes el navegador lo trataba como un clic en Guardar). Enter sigue haciendo lo suyo en los campos que lo usan (Código, Cantidad, editor de línea).
- Detalle técnico: `sin-foco-botones.js` + atributo `data-sin-foco` en cada botón; ver `docs/DECISIONS.md` (2026-11-19).

## Leer el código con la cámara (2026-11-20)
El botón de la cámara exige **dos lecturas iguales seguidas** y ahora acepta también **UPC-A** (12 dígitos: un EAN-13 que empieza con 0; antes decía «Código inválido»). La cámara queda abierta para el siguiente producto y tras cada lectura hay una pausa de 2 s. El recuadro es más alto (120 px) y muestra el **centro** de la imagen, que es lo que se lee. Detalle y consejos: concepto «Leer códigos de barra con la cámara del celular» (`conceptos/lectura-codigo-barra-camara.md`).

- **Aviso «Salir sin guardar» (desde 2026-11-20):** el foco arranca en «No, quedarme» con borde marcado; Enter solo activa el botón con foco y las flechas pasan a «Sí, salir». Ver [aviso «Salir sin guardar»](ayuda:concepto/aviso-salir-sin-guardar).
