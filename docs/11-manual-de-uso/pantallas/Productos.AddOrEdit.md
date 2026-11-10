---
pantalla: Productos.AddOrEdit
titulo: Alta y edición de un producto
rol: usuario
modulo: Productos
orden: 3
permiso: Producto.NuevoCorte para crear y guardar cambios (el servidor lo valida al guardar; sin él se vuelve al listado con un aviso)
revisada: 2026-11-10
---

## Para qué sirve
Crear un producto nuevo (**Nuevo** en Productos) o ver y modificar uno existente (el ojo de la fila). El mismo formulario sirve para las dos cosas.

## Ver y modificar un producto existente
1. Al abrir un producto el formulario está en **solo lectura**: los datos se ven como ficha (texto oscuro, sin caja) y arriba aparece la etiqueta **Solo lectura**. Junto a ella se ven el **ID** y el **Nivel** del producto.
2. **Modificar** (Alt + M) habilita los campos. Cambiá lo que haga falta y **Guardar cambios** (Alt + G).
3. **Volver** (Alt + V) regresa al listado.
4. **Carga continua** (interruptor arriba a la derecha): al guardar, abre el siguiente producto visible de la grilla (en edición) o deja el formulario en blanco para cargar otro (en alta). En edición con Carga continua aparece **Siguiente** (Alt + flecha derecha) para pasar al siguiente producto de la grilla.

## Datos del producto
- **Código** (hasta 15 dígitos; se avisa si ya existe o si el dígito verificador del EAN es incorrecto), **Nombre**, **Tipo** e **IVA** son obligatorios. **Precio Kg** es el precio de lista.
- **Marca** es opcional: la lupa (F9) abre el buscador y la **X** la quita.

## Stock y estado
- Interruptores **Pesable**, **Habilitado** y **En cierre stock**. Para que el producto salga como renglón en el reporte de Cierre de Stock hace falta **En cierre stock** *e* **Independiente** (el formulario avisa si falta Independiente).
- **Promedio**: valor general del producto.
- **Punto Stock** (solo en el alta): se carga como punto de stock inicial en **todas** las sucursales.
- **Punto de stock por sucursal** (solo al editar): lista, en solo lectura, el valor real de cada sucursal. Se cambia desde **Productos**, con el botón del depósito de la fila.

## Producto maestro / Presentación
- **Modo**: *Ninguno*, *Producto maestro* o *Presentación*. En los dos últimos se elige el producto maestro (lupa o F10) y se carga **%** y **Desperdicio** (maestro) o las **unidades** (presentación).
- **Independiente**: el producto lleva su propio renglón de stock, separado del maestro. El ícono de información explica el detalle.

## Jerarquía por sucursal
Solo aparece al editar un producto que depende de un producto maestro. Permite marcar una **excepción** para una sucursal (Independiente / En cierre de stock) sin afectar a las demás; cada fila tiene su propio **Guardar** y **Quitar excepción**.

## Disposición de la pantalla
En pantallas anchas (desde 1200 px) el formulario se muestra en dos columnas: *Datos del producto* y *Producto maestro* a la izquierda; *Stock y estado* y *Jerarquía por sucursal* a la derecha. En pantallas más chicas todo queda en una columna.

Verificado en pantalla (2026-11-10, base local, producto Carré): disposición en dos columnas a 1536 px y en una a 1100 px, modo solo lectura y paso a edición con **Modificar**, y modo oscuro. PENDIENTE: no se verificó el uso dentro de Compras (formulario embebido), el guardado de cambios ni la carga continua.
