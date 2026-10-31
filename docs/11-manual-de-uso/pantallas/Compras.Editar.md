---
pantalla: Compras.Editar
titulo: Cargar, modificar o eliminar una compra
rol: usuario
modulo: Compras
orden: 2
permiso: Compra.NuevaCompra (alta) y Compra.ModificarCompra (modificar y eliminar); a los usuarios de producción les pide elegir el operador de Compras antes de entrar
revisada: 2026-10-31
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
