---
pantalla: Stock.Editar
titulo: Cargar o modificar un movimiento de stock
rol: usuario
modulo: Stock
orden: 2
permiso: Stock.AddOrEditStock para guardar (PENDIENTE: en WebCore el chequeo de abrir la pantalla está omitido, ver TODO en StockController)
revisada: 2026-11-13
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

## Si el movimiento ya está abierto en otra pestaña (2026-11-12)
Si en esta computadora y en este navegador ya tenés un movimiento de stock abierto en otra pestaña o ventana, la pantalla te avisa antes de dejarte pisar lo que cargaste.
- **Nuevo movimiento:** solo avisa si el otro nuevo es de **la misma acción** (por ejemplo, dos «Egreso Stock»). Un «Egreso Stock» y un «Cierre Stock» abiertos a la vez no se avisan entre sí. Si **cambiás la acción** dentro de la pantalla, se vuelve a chequear con la acción nueva. Con **uno** abierto (y con algo cargado) elegís: **Traer a esta pestaña**, **Cargar otro nuevo aparte** o **Volver al listado**. Con **dos o más** no se puede traer ninguno: buscalos entre tus pestañas.
- **Modificar uno ya guardado:** mirarlo en dos pestañas no avisa; el aviso aparece al tocar **Modificar** si otra pestaña ya lo está modificando (**Traer la modificación a esta pestaña**, **Cargar uno nuevo aparte** o **Volver al listado**). Si cerrás el aviso, queda en solo lectura.
- La pestaña que entrega el trabajo **no se cierra sola**: queda tapada con un botón **Ir al listado**. Solo detecta pestañas del **mismo navegador** de esta computadora.

## Resto de la pantalla
PENDIENTE: documentar los datos del encabezado, la carga de productos (código, balanza, teclado numérico), Productos no cargados, Acumulados, Ver porcentaje de cortes y Generar ajuste, Vincular compra, borradores y las reglas por tipo de movimiento (no verificado contra el código en esta revisión).

## Datos del registro
- En la pantalla de un registro ya guardado, **Creado** y **Actualizado** (con el usuario debajo de cada fecha) se muestran en un bloque compacto de dos columnas, con letra chica, para ocupar poca altura.

## Modo solo lectura: cómo se ve que no se puede editar (2026-11-13)
Al abrir un movimiento **ya guardado** la pantalla queda en **modo ver** y lo deja claro:
- Arriba aparece la etiqueta gris **Solo lectura** y, a su lado, un botón azul **Modificar** (el mismo que el de abajo a la derecha; los dos hacen lo mismo).
- El cuadro **Agregar producto** (Código, Producto, Cantidad, balanza, teclado) **no se muestra**: no hay dónde escribir. En su lugar queda una franja que dice *«Estás viendo este registro. Para cambiarlo, presioná Modificar»* y los botones que siguen funcionando en modo ver (**Ver acumulados**, **Ver Porc(%)**).
- **Cambiar** la acción y su leyenda tampoco se muestran en modo ver.
- Si igual intentás escribir (letras o números, por ejemplo con el lector de código de barras), sale un aviso *«Estás viendo este registro…»* y el botón **Modificar** late dos veces. El aviso no se repite antes de 3 segundos.
- Al tocar **Modificar** vuelve el cuadro de carga y la etiqueta desaparece. Un movimiento **nuevo** no pasa por modo ver.
- Si no tenés permiso para modificar, la etiqueta se ve igual, el botón **Modificar** queda apagado y el aviso dice solo *«Estás viendo este registro en modo lectura.»*
- Lo hace `pantalla-trabajo.js` (sección 7) y `pantalla-trabajo.css` (sección 10), igual en Movimientos y Elaborados. **Verificado** solo en una maqueta con el CSS y el JS reales (Stock); PENDIENTE probarlo en las pantallas reales (también Movimientos con Stock destino y Elaborados).

## En la computadora (pantalla de 1200 px o más)
Pensado para ver **20 líneas o más sin bajar con la rueda**: la tabla ocupa todo el alto de la ventana y crece a medida que cargás productos (en un alta nueva arranca chica); recién cuando llena la pantalla tiene su propio scroll. La página no se mueve. La tabla **termina donde empieza el pie** («© 2026 - CarniSys») y, cuando no entran todas las líneas, scrollea **solo por dentro**: la página no se mueve, así no hay dos barras de scroll. En una ventana de 1920 × 860 se ven unas **25 líneas** en un alta y unas **16** en una de 1524 × 686 (Full HD con Windows al 125 %). En un pesaje ya guardado con compra y pesajes vinculados la cabecera ocupa más y entran menos líneas a la vista.
- **Datos del registro siempre a la vista, sin etiquetas:** Sucursal, Fecha, Acción, Proveedor, Cant.Medias y Kgs.Medias se ven siempre, en su lugar de siempre, sin la etiqueta de arriba (para ganar altura). El nombre de cada campo aparece al apoyar el mouse y, en Cant.Medias y Kgs.Medias, como texto dentro del campo vacío. «Solo productos», los botones de vincular y las cajas de compra o pesaje vinculado no cambian.
- **Pesaje (computadora): más productos a la vista.** Con la acción **Pesaje** la cabecera se acomoda en menos renglones: «Solo productos» va al lado de **Cambiar**; **Proveedor** (con el CUIT al lado), **Cant.Medias** y **Kgs.Medias** (con su nombre chico adentro del campo) y los botones **Vincular compra** y **Vincular pesajes** (solo el ícono; el nombre aparece al apoyar el mouse) van en un renglón, y la **compra vinculada** se resume en una línea (al final de ese renglón o, si no entra, debajo). El estado de la balanza («Conexión balanza…») pasa al renglón de la lupa. En una ventana de 1524 × 737 se ven unos 15 productos. Si activás «Solo productos» o cambiás la acción, todo vuelve a su lugar.
- **Botones de arriba y pantalla completa (2026-11-11):** **Volver** queda a la izquierda; a la derecha, juntos, el estado del **Ajuste** (en un Pesaje), **Stock sin guardar** (antes iba pegado a Volver) y un botón con el icono de **pantalla completa**. Tocarlo (o **F11**) entra y sale de pantalla completa; al apoyar el mouse aparece *F11 · Pantalla completa*. Igual que en Elaborados y Movimientos.
- **Cuadro «Agregar producto» identificado (2026-11-11):** una franja azul con el título **AGREGAR PRODUCTO** (*uno por uno: código, producto y cantidad*) y el cuadro con fondo y borde azules, para distinguir dónde se ingresa cada producto. En modo claro es el azul de Elaborados; en modo oscuro, un celeste oscuro. En el celular no cambia.
- **Agregar producto en una fila:** **Código, Producto (con F10), Cantidad y +**, y a continuación **Balanza**, **Ver acumulados**, el teclado y los botones de la acción (**Ver Porc(%)**, **Productos no cargados**) solo con su **icono** (el nombre aparece al apoyar el mouse) y el estado de la balanza. Las etiquetas (Código, Producto, Cantidad) se ven como texto dentro del campo vacío. Si la ventana es angosta, los iconos bajan a un segundo renglón. Con el **teclado digital abierto** vuelve el cuadro de siempre (Código y Producto en un renglón, Cantidad y **+** en el siguiente) y la lista cede lugar.
- **Líneas cargadas:** una por renglón, letra de 14 px, encabezado fijo y filas alternadas. El **nombre del producto y la cantidad van en negrita** (la misma negrita que en Movimientos y Elaborados); el **número de código** va en gris al lado del nombre (ya no dice «Código:»). El producto tiene lugar para unos **30 caracteres** y la cantidad queda pegada a él; si el nombre es más largo, sigue en un segundo renglón de la misma fila.
- **Botones al pie de la columna derecha:** **Modificar** (o **Guardar stock**), **Eliminar** y **Cancelar** están debajo de Resumen, Observaciones y Datos del registro, siempre a la vista.
- **Balanza como icono:** el interruptor pasó a un **icono de balanza con «Balanza» en chiquito**, **verde** si está activada y **gris** si no (se toca, o con la tecla **`*`**, que la activa o desactiva y **deja el cursor en Cantidad**; tocar el icono hace lo mismo). Con la balanza activa, **Cantidad** queda de solo lectura pero bien visible (fondo verde con el peso); **Código y Cantidad** van en negrita y **todo campo con el cursor se marca con borde verde**. **Enter**: Código ↵ pasa a Cantidad y Cantidad ↵ **registra la línea** y vuelve a Código. En el celular el icono sigue oculto, como antes.
- **Cursor al abrir (2026-11-13):** al abrir un registro **nuevo** el cursor queda en el campo **Código** del producto. Si el registro ya existe (o la pantalla está en modo ver) no se mueve. El sistema insiste unos segundos porque otras partes de la pantalla (borradores, avisos) le sacaban el foco al cargar; deja de insistir apenas tocás algo.
- **Sucursal y Fecha en negrita (2026-11-13):** igual que en Elaborados y Movimientos.
- **Fila resaltada, # y orden:** al pasar el mouse por una fila se pinta de azul tenue. La tabla tiene una columna **#** (número de carga, no cambia al ordenar), **Código** (columna propia que **reserva 14 caracteres**; si el código es más largo sigue en el renglón de abajo), **Producto** (pegado a Cantidad), **Cantidad**, **Bal**, **Creado** y **Acción**. **Todos los encabezados ordenan con un clic** (menos Acción; otro clic invierte el sentido y la flecha ↑/↓ marca la columna): **#**, **Código**, **Producto**, **Cantidad**, **Bal** y **Creado**; arranca por **#** ascendente. (Antes el código iba debajo del nombre y se ordenaba desde un link dentro del encabezado «Producto · Código».) **Ver acumulados** también tiene **#** (orden de primera carga del producto) y se ordena por **Código**, **Producto**, **Cantidad** o **Veces**. En el modal, la columna **Veces** (la última) dice en cuántos ítems de la tabla está cargado ese producto: **×1** en gris es lo normal; **×2 o más** va en ámbar con ⚠ (posible carga duplicada). Tocando la fila (o con Enter) se despliegan las cargas una por una, con su **Ítem #** de la tabla principal, y al pie el total, que coincide con la fila. Arriba hay un resumen («N productos distintos en M ítems · ⚠ X repetidos») y el tilde **Solo repetidos** deja únicamente los repetidos. Al abrir el modal todo arranca cerrado y sin filtro.
- **Resumen, Observaciones y Datos del registro** quedan a la derecha; **Observaciones** es una línea y se agranda al tocarla.
- **PENDIENTE (no medido en el navegador):** la altura con la caja de **Pesaje vinculado** de un **Ajuste** ya guardado y el modo oscuro.

## Abrir en una ventana independiente (computadora)
El botón con el icono ↗ (arriba a la derecha, antes de *Pantalla completa*; solo con 1200 px o más) abre el stock en **otra ventana del navegador** sin menú lateral, sin barra superior y sin pie (el título empieza con **«Stock ·»**). Vuelve a estar visible desde el 2026-11-13 (estaba oculto desde el 2026-11-04) y es **el mismo comportamiento que Movimientos**:
- **Tamaño:** **un tercio del ancho de la pantalla y todo el alto** (piso de 480 px); se abre centrada. Mismo diseño compacto que Movimientos: Sucursal / Fecha / Acción en un renglón con la etiqueta chica arriba, el cuadro azul **Agregar producto** en dos renglones, la tabla sin la columna *Creado* (la hora queda en el tooltip) y debajo Resumen (ítems y cantidad), Observaciones y los botones. «Stock sin guardar» queda como icono con el número.
- **Se cierra sola al guardar (2026-11-13):** en la ventana independiente, después de guardar y cerrar el aviso «El movimiento de stock se registró correctamente» (sale en el listado y se cierra solo a los 2 s o con OK), la ventana **se cierra automáticamente** (antes volvía al listado dentro de la misma ventana). Si el navegador no deja cerrarla (por ejemplo, una pestaña común abierta con `?ventana=1`), se va al listado como antes.
- **Quién opera:** como no hay barra superior, la barra de la pantalla muestra al lado de *Volver* el **usuario con sesión** y, si es la **cuenta de producción**, una etiqueta amarilla **«Operador: <nombre>»** con el operador elegido antes de entrar.
- Si ya hay una ventana abierta de esta pantalla, se la trae al frente. Con cambios sin guardar el botón avisa y no abre (Movimientos sí puede pasar lo cargado).

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
