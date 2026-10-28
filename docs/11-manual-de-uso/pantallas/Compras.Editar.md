---
pantalla: Compras.Editar
titulo: Cargar, modificar o eliminar una compra
rol: usuario
modulo: Compras
orden: 2
permiso: Compra.NuevaCompra (alta) y Compra.ModificarCompra (modificar y eliminar); a los usuarios de producción les pide elegir el operador de Compras antes de entrar
revisada: 2026-10-27
alias: Compras.NuevaCompra, Compras.ModificarCompra
---
## Para qué sirve
Cargar una compra nueva o abrir una ya guardada para verla, modificarla o **eliminarla**. Esta página documenta por ahora **Eliminar** y cómo se ve **en el celular**; el resto de la pantalla (proveedor, productos, medias reses, cuenta corriente, guardar) está pendiente (**PENDIENTE**: no se revisó en esta versión).

## En el celular
- **Ajustes:** Desc./Recargo, IVA y Margen / precio de venta están guardados detrás del botón **Ajustes**, al lado de **Agregar producto**. Tocalo para abrirlos y otra vez para cerrarlos. Con el panel cerrado, el botón muestra los ajustes que están encendidos (por ejemplo «IVA 21%»), así sabés que se van a aplicar a la próxima línea. El panel no se cierra solo al agregar un producto.
- **Cantidad, Precio y Subtotal** van en una misma fila.
- **Productos cargados:** se ven como una lista. Arriba, el número de línea, el producto y el total; abajo, el código con cantidad × precio (en las medias, la tropa con kilos × precio). Si la línea tiene descuento/recargo, IVA, precio de venta o margen, se muestran en un renglón más. El tacho de la derecha quita la línea.
- **Orden:** arranca con **lo último cargado arriba**; con el selector de la derecha se cambia a «Primeros primero». En la computadora se ordena tocando «#», como siempre.

## Eliminar una compra
El botón rojo **Eliminar** aparece al abrir una compra **ya guardada** (en la pantalla completa; no dentro del modal del POS ni de la cuenta corriente del proveedor) para quien tenga permiso de modificar compras.

1. Tocá **Eliminar**. Se abre una ventana con el **resumen** de la compra (tipo, fecha, sucursal, proveedor, remito, importe, líneas y kilos), **todas sus líneas** y **qué va a pasar**.
2. Escribí el **motivo** (de 10 a 300 caracteres; es obligatorio y queda registrado).
3. Confirmá tu **usuario y contraseña**. Aunque ya estés logueado, **se pide siempre**, en cada eliminación. Con tu propio usuario no se puede confirmar otro usuario. Si trabajás con la cuenta de producción, elegí tu nombre de la lista y escribí tu contraseña. Tenés 5 intentos; al 5.º error se bloquea 5 minutos (ver [intentos de clave al autorizar](ayuda:concepto/intentos-de-clave-al-autorizar)).
4. **Eliminar definitivamente**. La compra se borra y volvés al listado.

### Qué pasa con la compra
- **Cuenta corriente del proveedor.** Si la compra estaba en cuenta corriente, **no se borra su renglón**: se agrega un **asiento contrario** («ELIMINADO - …») con la fecha original de la compra. En el extracto del proveedor quedan los dos renglones y el saldo vuelve a ser el de antes de la compra. La ventana muestra antes el saldo actual y cómo queda.
- **Caja.** Si la compra se cargó desde el POS con plata de la caja, se registra un **egreso opuesto con la fecha de hoy**. Si esa caja ya está cerrada, el sistema avisa y pide confirmar (el cierre de esa caja queda sin ese reverso).
- **Precios.** Si la compra había actualizado el precio de venta de algún producto, ese precio **no se revierte**. El «último precio de compra» de cada proveedor se recalcula con la compra anterior.
- **Constancia.** Quedan guardados en [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index) quién la eliminó, cuándo, el motivo y todo lo que tenía. Los administradores reciben un aviso en la campana.
- **No se puede deshacer** desde el sistema.

### Cuándo no se puede eliminar
- Si la compra tiene **pesajes vinculados**, la ventana lo dice y no deja confirmar: desvinculalos o eliminalos antes.
- Si tu usuario no tiene permiso para esa fecha, el sistema lo avisa al confirmar.
- Si otra persona ya la eliminó, lo avisa y no hace nada más.

Si abrís una compra que ya fue eliminada (por ejemplo desde un link guardado), el sistema te muestra quién la eliminó, cuándo y por qué (los administradores ven el detalle completo).
