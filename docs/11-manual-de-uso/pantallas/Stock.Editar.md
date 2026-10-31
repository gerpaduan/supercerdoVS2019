---
pantalla: Stock.Editar
titulo: Cargar o modificar un movimiento de stock
rol: usuario
modulo: Stock
orden: 2
permiso: Stock.AddOrEditStock para guardar (PENDIENTE: en WebCore el chequeo de abrir la pantalla está omitido, ver TODO en StockController)
revisada: 2026-10-31
alias: Stock.Nuevo
---
## Para qué sirve
Cargar un movimiento de stock nuevo (ingreso, egreso, ajuste, cierre, **pesaje**, etc.) o modificar uno ya guardado. Esta página documenta por ahora solo la parte de **vincular y desvincular pesajes**; el resto de la pantalla está pendiente (ver al final).

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
Pensado para ver más líneas sin bajar con la rueda (la **fecha** de ancho completo, con la hora, ya desde 992 px):
- **Fecha siempre completa, con la hora**: Sucursal, Fecha y Acción van en una fila y lo que cede es la Sucursal. La leyenda «La acción se desbloquea sólo cuando presiona cambiar» ya no se muestra (el botón **Cambiar** tiene su ayuda al apoyar el mouse).
- **Agregar producto:** el título, **Balanza**, **Ver acumulados**, el teclado y los botones de la acción (**Ver Porc(%)**, **Productos no cargados**) van en **una sola línea**; el estado de la balanza y su barra, también. **Código, Producto, Cantidad y +** van en un renglón.
- **Líneas cargadas:** una por renglón (el **código** va al lado del producto, no debajo), letra un punto más chica y encabezado fijo. Con una ventana de **690 px de alto** se ven **11 líneas** (8 con 629 px, 18 con 829 px); con más, la lista tiene su propio scroll y la página no se mueve.
- **Fila resaltada, # y orden:** al pasar el mouse por una fila se pinta de azul tenue. La tabla tiene una columna **#** (número de carga, no cambia al ordenar) y se ordena tocando **#**, **Producto**, **Código**, **Cantidad** o **Creado**; arranca por **#** ascendente. **Ver acumulados** también tiene **#** (orden de primera carga del producto) y se ordena por **Código**, **Producto** o **Cantidad**; al abrirlo arranca por código. El orden en pantalla no cambia el orden en que se guarda.
- **Teclado digital abierto:** Código y Producto en un renglón, Cantidad y **+** en el siguiente, y el teclado con teclas más bajas. La lista cede lugar (unas **4 líneas** a 690 px de alto) y scrollea por dentro.
- **Pesaje:** la fila de **Proveedor, Cant.Medias, Kgs.Medias** y los botones de vincular suma una fila; con 690 px de alto entran unas 7 líneas. La caja **Compra vinculada** (al vincular) y, en un pesaje ya guardado, la caja de datos de la compra o del pesaje, van en pocos renglones. En un **pesaje ya guardado** la página scrollea unos 15 px a 690 px de alto (la columna derecha con Resumen, Observaciones y Datos del registro es más alta que la lista).
- **Resumen, Observaciones y Datos del registro** quedan a la derecha, más bajos; **Observaciones** es una línea y se agranda al tocarla.
- Con una ventana de menos de 690 px de alto la lista cede líneas (**8** a 629 px).
- **PENDIENTE (no medido en el navegador):** la altura exacta con **Compra vinculada** recién elegida, la caja de **Pesaje vinculado** de un **Ajuste** ya guardado, teclado abierto en modo edición y modo oscuro.

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
