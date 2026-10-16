---
pantalla: Productos.Index
titulo: Productos
rol: usuario
modulo: Productos
orden: 1
permiso: Producto.ModificarPrecios para cambiar precios (el botón $ de cada fila y "Precios por selección" quedan deshabilitados sin él)
revisada: 2026-10-16
---
## Para qué sirve
Listado de los **productos** de la empresa, con su código, nombre, stock y precio. Desde acá se crean, editan y eliminan productos, se imprimen etiquetas y se cambian precios.

## Buscar y filtrar
- **Buscador**: filtra al escribir por código o por nombre.
- **Filtros avanzados** (interruptor de arriba a la derecha): tipo, marca y rango de código se aplican al instante; proveedor y fechas necesitan **Aplicar** (recargan la pantalla).

## Cambiar el precio de un producto
El botón amarillo **$** de cada fila abre *Modificar precio*: se escribe el nuevo precio y **Guardar**. Dentro de ese modal, **Modificar en lote** permite:
- **Aplicar porcentaje a todos**: suma o resta un porcentaje (`+10`, `-5`) a **todos los productos que se están viendo** en la grilla en ese momento (los que dejan el buscador y los filtros). Antes de guardar muestra una lista para revisar.
- Sin porcentaje: recorre **uno por uno** los productos visibles.

## Precios por selección (elegir qué productos cambiar)
Sirve para cambiar el precio **solo de los productos que vos elijas**, sin tener que filtrar hasta que queden justo esos.

1. Apretá **Precios por selección** (barra de arriba, atajo **Alt + P**). Si el botón está apagado, tu usuario no tiene permiso para modificar precios.
2. La grilla muestra una columna **✓**. Tildá los productos con el casillero o con **doble clic** sobre la fila. El buscador y los filtros siguen funcionando para achicar la lista; **Tildar todos** tilda solo los que se están viendo y **Destildar todos** los suelta. El contador dice cuántos tildados hay.
3. Elegí cómo cambiar el precio de los tildados:
   - **Aplicar % a los tildados**: escribís el porcentaje (`+10` o `-5`) y **Verificar cambios**. Se abre la lista con **precio anterior** y **precio nuevo** de cada producto tildado: podés corregir un precio a mano o quitar una fila con el tacho. Si está bien, **Guardar**.
   - **Uno por uno**: se abre *Modificar precio* en el primer tildado; al **Guardar** pasa al siguiente tildado, y al terminar avisa.
4. Los productos que se guardan **quedan destildados**, así no se les aplica el mismo porcentaje dos veces. Si algo falla a mitad de camino, los ya guardados quedan destildados y los pendientes siguen tildados: podés volver a intentar solo con ellos.
5. **Volver** regresa a la grilla normal.

Cosas a tener en cuenta:
- Si sin querer recargás la página (por ejemplo al aplicar un filtro de proveedor o fechas, o al cambiar de sucursal), la selección se pierde y hay que tildar de nuevo.
- Un producto tildado que el buscador o un filtro **oculta** no se cuenta ni se modifica hasta que vuelva a verse.
- Cada precio se guarda de a uno, y por ahora el sistema **no guarda quién lo cambió ni cuál era el precio anterior**.

## Etiquetas
**Etiquetas (PDF)** (Alt + E) funciona igual: tildás los productos, elegís el tamaño (chica, mediana o grande) y **Generar etiquetas** descarga un PDF con una etiqueta por producto.

## Resto de la pantalla
PENDIENTE: documentar la columna de stock por sucursal, la vista completa, las acciones de cada fila (ver, stock por sucursales, eliminar), Tipos, Marcas, el catálogo global y el ajuste por forma de pago (no verificado contra el código en esta revisión).
