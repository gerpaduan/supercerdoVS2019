---
pantalla: Productos.Marcas
titulo: Marcas de productos
rol: usuario
modulo: Productos
orden: 2
permiso: Producto.VerCortes para ver la pantalla; Producto.NuevoCorte para crear, modificar o eliminar marcas
revisada: 2026-10-22
---

## Para qué sirve
Lista las **marcas** que usan los productos. Desde acá se crean marcas nuevas, se modifican las existentes (propietario y otros datos; el nombre solo lo cambia un administrador) y se eliminan las que no tienen productos.

## Propietario de una marca
1. En **Nueva marca** o **Modificar**, el campo *Propietario* tiene una lupa. Al apretarla se abre el mismo buscador de personas que usa el POS (razón social, CUIT o identificación).
2. Elegí la persona con **doble clic**, **Enter** (la fila marcada) o **Alt + número** (la fila de la columna #). Las flechas mueven la selección.
3. Si la persona no existe, apretá **Nueva**: se abre el alta de persona. Con el **CUIT** (11 dígitos) y el botón de la lupa se completan los datos desde AFIP/ARCA; si el CUIT ya estaba escrito en el buscador, se consulta solo. Hay que elegir la condición frente al IVA. Al guardar, la persona queda como propietaria.
4. Cerrar el buscador o el alta sin elegir vuelve al formulario de la marca sin perder lo escrito.
5. La **X** del campo quita el propietario.

PENDIENTE: no se verificó en pantalla con datos reales el aspecto de la consulta a AFIP desde este flujo (es el mismo formulario de alta de personas del POS).

## Eliminar una marca
1. **Eliminar** (en la fila de la marca) pide confirmación. Si ningún producto usa la marca, se elimina.
2. Si hay productos con esa marca no se elimina: se abre una lista con **código, producto, marca actual y un selector con la nueva marca** (incluye **(Sin marca)**). **Pasar todos a** copia la misma marca a toda la lista.
3. **Guardar** cambia solo los productos con una marca elegida (los demás quedan como estaban). Si después no queda ningún producto con la marca, **se elimina sola**; si quedan, se avisa cuántos y la marca se conserva.
4. Un producto que otra persona modificó mientras la lista estaba abierta no se cambia (se avisa cuántos).
