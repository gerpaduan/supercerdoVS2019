---
pantalla: Stock.Editar
titulo: Cargar o modificar un movimiento de stock
rol: usuario
modulo: Stock
orden: 2
permiso: Stock.AddOrEditStock para guardar (PENDIENTE: en WebCore el chequeo de abrir la pantalla está omitido, ver TODO en StockController)
revisada: 2026-11-04
alias: Stock.Nuevo
---
## Para qué sirve
Cargar un movimiento de stock nuevo (ingreso, egreso, ajuste, cierre, **pesaje**, etc.) o modificar uno ya guardado. Esta página documenta por ahora solo la parte de **vincular y desvincular pesajes**; el resto de la pantalla está pendiente (ver al final).

## Pesaje «Solo productos» (sin medias de res)
Un **Pesaje** normal pide **Proveedor, Cant.Medias y Kgs.Medias** (sirven para comparar los cortes contra la media res en **Ver Porc(%)** y **Generar ajuste**). Si lo que pesás **no es de cortes de media res**, activá el interruptor **Solo productos**, que está en el renglón del título **Acción** (solo se ve con la acción Pesaje; al pasar el mouse dice para qué sirve).
- **Al activarlo** se ocultan **Proveedor, Cant.Medias, Kgs.Medias, Vincular compra, Vincular pesajes y Ver Porc(%)**, y el pesaje se guarda sin medias ni kilos. Para guardar solo hace falta cargar los productos.
- **Proveedor:** un pesaje nuevo queda con el proveedor «Indefinido». Si ya estaba guardado, conserva el proveedor que tenía.
- **Si intentás guardar sin medias o kilos** y el interruptor está apagado, el mensaje dice *«Si este pesaje no es de cortes de media res, active "Solo productos"»* y el interruptor se resalta en amarillo.
- **No se puede activar** si el pesaje tiene una **compra o pesajes vinculados** (primero se desvinculan) ni si ya tiene un **ajuste generado** (primero se elimina el ajuste).
- **Al abrir** un pesaje guardado sin medias ni kilos y sin vínculos, el interruptor aparece **activado**. Si tiene vínculos queda apagado y hay que cargar las medias o desvincular.
- **En la lista de Stock** estos pesajes muestran *Solo productos* en vez del proveedor y los ceros.
- Al cambiar la acción a otra distinta de Pesaje, el interruptor se apaga. El borrador también lo recuerda.
- PENDIENTE: el detalle desplegado de la lista de Stock sigue mostrando el botón **Ver Porc(%)** en estos pesajes; al tocarlo avisa que el pesaje no tiene medias.

## Vincular y desvincular pesajes
Aplica cuando el tipo del movimiento es **Pesaje**. El botón **Vincular pesajes** abre una lista de pesajes de la misma sucursal (por defecto, de los últimos 7 días; se puede cambiar con *Fecha desde / Fecha hasta* y el buscador de proveedor).

### Vincular
1. En la fila del pesaje que querés sumar, tocá **Detalle** para ver sus líneas (opcional) y **Vincular**.
2. Las líneas de ese pesaje se **agregan a este pesaje** (con la etiqueta *Pesaje vinculado #N*) y arriba aparece el aviso *Pesajes vinculados en esta edición*.
3. El vínculo queda guardado recién cuando tocás **Guardar**.

### Desvincular
1. Abrí **Vincular pesajes**. Los pesajes que ya están vinculados a este muestran el botón rojo **Desvincular** (antes decían "Ya vinculado"). Si el pesaje es muy viejo y no aparece, ampliá *Fecha desde*.
2. Tocá **Desvincular**. El sistema pide confirmación y dice **cuántas líneas (y cuántos kg) se van a quitar** de este pesaje.
3. Al confirmar, las líneas de ese pesaje **desaparecen de la tabla** y el pesaje deja de figurar como vinculado. **Nada queda definitivo hasta que tocás Guardar**: si salís sin guardar, todo vuelve a como estaba.
4. Si te arrepentís antes de guardar, podés **volver a vincular** el mismo pesaje desde la misma lista.

Cosas a tener en cuenta:
- Las líneas se reconocen por **producto y kilos**. Si el pesaje vinculado se **modificó después de haberlo vinculado**, o si ya borraste a mano algunas de sus líneas, el aviso dice cuántas **no se encontraron**: esas no se tocan y, si hace falta, se quitan a mano con el tacho de cada fila.
- Si este pesaje tiene dos líneas idénticas (mismo producto y kilos) y el pesaje vinculado aportó una, se quita una de las dos (son equivalentes).
- Al guardar, el pesaje desvinculado queda marcado como *modificado* (cambia su fecha y usuario de última modificación).
- La confirmación se puede cancelar sin efecto.

## Eliminar un movimiento de stock
El botón rojo **Eliminar** aparece al abrir un movimiento **ya guardado** (ingreso, egreso, cierre, ajuste o pesaje) para quien puede modificarlo. Funciona igual que en [Compras](ayuda:pantalla/Compras.Editar): resumen y líneas, **motivo obligatorio** (10 a 300 caracteres) y **usuario y contraseña en cada eliminación** (ver [eliminar registros](ayuda:concepto/eliminar-registros)).
- El registro **se borra** y el stock de la sucursal se recalcula sin él. Queda la constancia (quién, cuándo, motivo y todas las líneas) en [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index).
- **Ajuste de stock y Cierre de stock:** solo administradores. Un **cierre** es el punto de partida del cálculo: al eliminarlo cambia el stock de la sucursal desde el cierre anterior (la ventana lo avisa).
- Un **Pesaje** con pesajes vinculados o con un **ajuste generado** no se puede eliminar: primero se desvinculan o se elimina el ajuste. Al eliminar un **ajuste**, el pesaje que ajustaba vuelve a «No realizado».
- No se puede deshacer desde el sistema.

## Resto de la pantalla
PENDIENTE: documentar los datos del encabezado, la carga de productos (código, balanza, teclado numérico), Productos no cargados, Acumulados, Ver porcentaje de cortes y Generar ajuste, Vincular compra, borradores y las reglas por tipo de movimiento (no verificado contra el código en esta revisión).

## Datos del registro
- En la pantalla de un registro ya guardado, **Creado** y **Actualizado** (con el usuario debajo de cada fecha) se muestran en un bloque compacto de dos columnas, con letra chica, para ocupar poca altura.

## En la computadora (pantalla de 1200 px o más)
Pensado para ver **20 líneas o más sin bajar con la rueda**: la tabla ocupa todo el alto de la ventana y crece a medida que cargás productos (en un alta nueva arranca chica); recién cuando llena la pantalla tiene su propio scroll. La página no se mueve. La tabla **termina donde empieza el pie** («© 2026 - CarniSys») y, cuando no entran todas las líneas, scrollea **solo por dentro**: la página no se mueve, así no hay dos barras de scroll. En una ventana de 1920 × 860 se ven unas **25 líneas** en un alta y unas **16** en una de 1524 × 686 (Full HD con Windows al 125 %). En un pesaje ya guardado con compra y pesajes vinculados la cabecera ocupa más y entran menos líneas a la vista.
- **Datos del registro siempre a la vista, sin etiquetas:** Sucursal, Fecha, Acción, Proveedor, Cant.Medias y Kgs.Medias se ven siempre, en su lugar de siempre, sin la etiqueta de arriba (para ganar altura). El nombre de cada campo aparece al apoyar el mouse y, en Cant.Medias y Kgs.Medias, como texto dentro del campo vacío. «Solo productos», los botones de vincular y las cajas de compra o pesaje vinculado no cambian.
- **Pesaje (computadora): más productos a la vista.** Con la acción **Pesaje** la cabecera se acomoda en menos renglones: «Solo productos» va al lado de **Cambiar**; **Proveedor** (con el CUIT al lado), **Cant.Medias** y **Kgs.Medias** (con su nombre chico adentro del campo) y los botones **Vincular compra** y **Vincular pesajes** (solo el ícono; el nombre aparece al apoyar el mouse) van en un renglón, y la **compra vinculada** se resume en una línea (al final de ese renglón o, si no entra, debajo). El estado de la balanza («Conexión balanza…») pasa al renglón de la lupa. En una ventana de 1524 × 737 se ven unos 15 productos. Si activás «Solo productos» o cambiás la acción, todo vuelve a su lugar.
- **Agregar producto en una fila:** **Código, Producto (con F10), Cantidad y +**, y a continuación **Balanza**, **Ver acumulados**, el teclado y los botones de la acción (**Ver Porc(%)**, **Productos no cargados**) solo con su **icono** (el nombre aparece al apoyar el mouse) y el estado de la balanza. Las etiquetas (Código, Producto, Cantidad) se ven como texto dentro del campo vacío. Si la ventana es angosta, los iconos bajan a un segundo renglón. Con el **teclado digital abierto** vuelve el cuadro de siempre (Código y Producto en un renglón, Cantidad y **+** en el siguiente) y la lista cede lugar.
- **Líneas cargadas:** una por renglón, letra de 14 px, encabezado fijo y filas alternadas. El **nombre del producto y la cantidad van en negrita** (la misma negrita que en Movimientos y Elaborados); el **número de código** va en gris al lado del nombre (ya no dice «Código:»). El producto tiene lugar para unos **30 caracteres** y la cantidad queda pegada a él; si el nombre es más largo, sigue en un segundo renglón de la misma fila.
- **Botones al pie de la columna derecha:** **Modificar** (o **Guardar stock**), **Eliminar** y **Cancelar** están debajo de Resumen, Observaciones y Datos del registro, siempre a la vista.
- **Fila resaltada, # y orden:** al pasar el mouse por una fila se pinta de azul tenue. La tabla tiene una columna **#** (número de carga, no cambia al ordenar) y se ordena tocando **#**, **Producto**, **Código**, **Cantidad** o **Creado**; arranca por **#** ascendente. **Ver acumulados** también tiene **#** (orden de primera carga del producto) y se ordena por **Código**, **Producto** o **Cantidad**.
- **Resumen, Observaciones y Datos del registro** quedan a la derecha; **Observaciones** es una línea y se agranda al tocarla.
- **PENDIENTE (no medido en el navegador):** la altura con la caja de **Pesaje vinculado** de un **Ajuste** ya guardado y el modo oscuro.

## Abrir en una ventana independiente (computadora)
El botón **Abrir en ventana** está **oculto por ahora** (pedido del 2026-11-04). La pantalla se sigue pudiendo usar sin menú si se instala como aplicación desde `https://carnisys.com` (*Instalar CarniSys …*; PENDIENTE: no se probó).

## En el celular
- **Líneas cargadas:** se ven como una lista. Arriba el producto y la cantidad; abajo el código (`Cód. 132`), la **hora de carga** y el icono de **balanza** (azul = pesado en balanza, gris = no). Si la línea se cargó **otro día** que el registro, en vez de la hora aparece fecha y hora en ámbar. Las etiquetas **No contado** y **Pesaje #N** quedan junto al nombre del producto. El botón rojo de la derecha **quita la línea**. En la computadora es una tabla (ver arriba).
- **Orden:** en el celular arranca con **lo último cargado arriba**. Con el selector de la derecha se puede cambiar (primeros primero, código, producto, cantidad). En la computadora se ordena tocando los encabezados, como siempre.
- El orden en que se ven las líneas **no cambia el orden en que se guardan**: siempre se guardan en el orden en que se cargaron.
- El selector de orden y la **lupa de búsqueda** **siguen activos en el modo ver** (cuando los campos y la tabla están bloqueados): solo cambian cómo se ven las líneas.
- **Botones:** en un **Pesaje**, **Vincular compra** y **Vincular pesajes** van uno al lado del otro. En **Agregar producto**, **Ver acumulados**, **Ver Porc(%)** y **Productos no cargados** (los que corresponden a la acción elegida) se reparten el ancho debajo del título.

## Signo de las cantidades según la acción
Al **cambiar la acción** (botón **Cambiar** junto a la acción) las cantidades **ya cargadas** se ajustan solas, y el sistema avisa con un mensaje verde sobre la tabla cuántas líneas cambiaron:

| Pasás a | Qué pasa con las líneas ya cargadas | Las que cargues después |
|---|---|---|
| **Egreso** | todas pasan a **negativas** | se cargan en negativo (tipeás el número en positivo y se convierte solo) |
| **Ingreso, Cierre o Pesaje** | todas pasan a **positivas** (también si venías de Ajuste con negativas) | deben ser mayores a cero |
| **Ajuste** | **no cambian** (si venías de Egreso quedan negativas) | positivas o negativas, como las tipees |

- El cambio es solo en pantalla: **queda definitivo al tocar Guardar**. Si cambiás de acción y volvés a la anterior, las cantidades que pasaron a positivas o negativas **no vuelven a su signo original** (por ejemplo, Ajuste → Ingreso → Ajuste deja todo positivo).
- El texto de ayuda debajo de **Cantidad** dice la regla de la acción elegida.
