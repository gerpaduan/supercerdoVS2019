---
pantalla: Productos.Index
titulo: Productos
rol: usuario
modulo: Productos
orden: 1
permiso: Producto.ModificarPrecios para cambiar precios (el botón $ de cada fila y "Precios por selección" quedan deshabilitados sin él)
revisada: 2026-10-21
---

## Para qué sirve
Listado de los **productos** de la empresa, con su código, nombre, stock y precio. Desde acá se crean, editan y eliminan productos, se imprimen etiquetas y se cambian precios.

## Buscar y filtrar
- **Buscador**: filtra al escribir por código o por nombre.
- **Filtros avanzados** (interruptor de arriba a la derecha): tipo, marca y rango de código se aplican al instante; proveedor, fechas y *Familia de producto* necesitan **Aplicar** (recargan la pantalla).
- **Actualizado en período** (Filtros avanzados): *Desde* y *Hasta* tienen fecha y hora. Si elegís la fecha y no la hora, la hora queda en **00:00:00** (se completa sola). Un *Hasta* con hora 00:00:00 incluye todo ese día. Toma la fecha de última modificación del producto —que ahora también se actualiza al **cambiar el precio**— o, si nunca se modificó, la de creación.
- **Última modificación**: al desplegar **Detalles** de un producto, abajo se ve en chico «Últ. modificación: fecha y hora por usuario». Sin fecha («-») = nunca se modificó; sin usuario = lo modificó un proceso sin usuario (por ejemplo, el precio actualizado al cargar una compra) o fue anterior a este registro.
- **Familia de producto** (dentro de Filtros avanzados): sirve para ver un producto junto con los que dependen de él o de los que él depende. Escribí el código o el nombre y elegí el producto de la lista (tiene que quedar el texto «código - nombre»), y tildá:
  - **Hijos, nietos…**: suma todos los productos que salen de ése, de cualquier nivel (hijos, nietos, bisnietos…).
  - **Padres, abuelos…**: suma el producto del que ése sale y los de arriba (padre, abuelo, bisabuelo…).

  Se puede tildar uno, otro o los dos. Siempre se muestra también el producto elegido. Cuentan las presentaciones (cajas o packs) igual que los cortes. Se combina con los demás filtros: por ejemplo, Familia + Tipo muestra solo los de ese tipo dentro de la familia. **Limpiar** quita el filtro.

## Cambiar el precio de un producto
El botón amarillo **$** de cada fila abre *Modificar precio*: se escribe el nuevo precio y **Guardar**; el modal se cierra y aparece un aviso "El precio se guardó correctamente" con botón **OK** (se puede aceptar sin esperar a que el modal termine de cerrarse). Dentro de ese modal, **Modificar en lote** permite:
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
- Cada precio se guarda de a uno. Al guardarlo (por *Modificar precio* o por selección) el producto queda con la **fecha y el usuario de última modificación** (se ven en el detalle del producto). El sistema **no guarda cuál era el precio anterior**.

## Etiquetas
**Etiquetas (PDF)** (Alt + E) funciona igual: tildás los productos, elegís el tamaño (chica, mediana o grande) y **Generar etiquetas** descarga un PDF con una etiqueta por producto.

## Resto de la pantalla
PENDIENTE: documentar la columna de stock por sucursal, la vista completa, las acciones de cada fila (ver, stock por sucursales, eliminar), Tipos, Marcas, el catálogo global y el ajuste por forma de pago (no verificado contra el código en esta revisión).
