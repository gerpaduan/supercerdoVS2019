---
pantalla: Productos.Tipos
titulo: Tipos de producto
rol: usuario
modulo: Productos
orden: 3
permiso: Producto.VerTiposProducto para ver la pantalla; Producto.AddOrEditTipoProducto para crear, modificar o eliminar tipos
revisada: 2026-10-22
---

## Para qué sirve
Lista los **tipos de producto** (Carne, Embutido, etc.). Desde acá se crean, modifican y eliminan tipos. Los tipos reservados por el sistema no se pueden modificar ni eliminar.

## Eliminar un tipo
1. **Eliminar** (en la fila del tipo) pide confirmación. Si ningún producto usa el tipo, se elimina.
2. Si hay productos con ese tipo no se elimina: se abre una lista con **código, producto, tipo actual y un selector con el nuevo tipo**. **Pasar todos a** copia el mismo tipo a toda la lista.
3. **Guardar** cambia solo los productos con un tipo elegido (los demás quedan como estaban). Si después no queda ningún producto con el tipo, **se elimina solo**; si quedan, se avisa cuántos y el tipo se conserva.
4. Un producto que otra persona modificó mientras la lista estaba abierta no se cambia (se avisa cuántos).

PENDIENTE: las demás acciones de la pantalla (crear, modificar y catálogo global) todavía no están descriptas acá.
