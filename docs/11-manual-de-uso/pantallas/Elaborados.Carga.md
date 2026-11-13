---
pantalla: Elaborados.Carga
titulo: Nuevo Elaborado (carga manual)
rol: usuario
modulo: Elaborados
orden: 2
permiso: Elaborado.IngresoEmbutido (editar) / Elaborado.VerEmbutidos (solo ver un registro existente)
revisada: 2026-11-13
---
## Para qué sirve
Registrar un elaborado cargando **a mano los ingredientes** (con su peso). Los ingredientes de la fórmula marcados como *Auto* se agregan solos al guardar, calculados como porcentaje de lo cargado a mano.

## Fórmula secreta
Si el elaborado elegido tiene una fórmula [secreta](ayuda:concepto/formula-secreta), la **Receta** y la tabla **Fórmula** no se muestran: aparece el aviso *"La fórmula de este elaborado es secreta. Los ingredientes automáticos se calculan y se agregan solos al guardar"* y el botón **Ver fórmula**. También se ocultan el **Total a producir** y el costeo (dependen de la fórmula).

- **Guardar funciona igual**: cargás tus ingredientes manuales y el sistema agrega los automáticos al guardar.
- **Ver fórmula** abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (solo aparecen quienes tienen permiso sobre fórmulas) y se escribe su contraseña (se pide siempre). Si valida se ven receta, ingredientes y totales, con un botón **Ocultar**. Se vuelve a ocultar sola a los 5 minutos, con **Ocultar** o al salir de la pantalla.
- Los borradores de esta pantalla no guardan la fórmula de un producto secreto; al recuperarlos se vuelve a pedir.

## Código de ingrediente sin agregar
Si escribiste (o escaneaste) un código de ingrediente y no lo agregaste a la tabla (Enter o botón +), **no se puede guardar**: aparece el aviso *"El campo Código tiene un valor sin agregar..."*. Agregá el ingrediente o borrá el código y volvé a guardar.

## Agregar un ingrediente
**Código**, **Ingrediente**, **Cantidad** (kilos) y el botón **+**. En el celular **Cantidad** y **+** van en el mismo renglón, con el **+** pegado a la cantidad (con el teclado numérico en pantalla abierto se apilan como antes).

## En la computadora (pantalla de 1200 px o más)
Pensado para ver **muchos ingredientes sin bajar con la rueda**: la tabla de ingredientes ocupa **todo el alto libre de la columna izquierda** (con su borde verde, aunque tenga pocas filas) y tiene su propio scroll cuando se llena. (La tabla **Fórmula** de la derecha **nunca scrollea**: se ven todos los ingredientes y las tres columnas; si son muchos, **crece la página** y hay que bajar para llegar a **Observaciones** y **Guardar elaborado**. La columna derecha es más ancha que antes y el nombre de un ingrediente largo baja a un segundo renglón. Excepción: con el **costeo** abierto la tabla vuelve a tener scroll horizontal por la columna Costo.)
- **Dos sectores identificados (2026-11-11):** una franja de color sobre cada cuadro dice qué se carga ahí. **① ELABORADO** (azul): *lo que vas a fabricar (producto final)*. **② INGREDIENTES** (verde): *con lo que se fabrica*, con la cantidad de ingredientes y los kilos cargados a la derecha. En **modo oscuro** el azul del sector ① es un celeste oscuro (franja azul petróleo con letra clara) en lugar del azul claro de antes. Cada campo tiene su **etiqueta siempre visible** (Código, Nombre del elaborado, Código, Ingrediente, Cantidad (kg)); antes solo había un texto gris que desaparecía al escribir.
- **Pestañas y botones en un renglón:** las pestañas de Elaborados a la izquierda; **Volver** y **Elaborados sin guardar** (antes *Embutidos sin guardar*; los dos botones son más chicos desde el 2026-11-11) a la derecha. Si la ventana es angosta (1200-1350 px) los botones bajan a un segundo renglón; nunca quedan tapados por las pestañas.
- **Sucursal, Fecha, Usuario y Creado siempre a la vista, sin etiquetas:** la fila de siempre (la **fecha se ve completa, con hora y segundos**; **Sucursal y Fecha van en negrita**) sin la etiqueta de arriba; el nombre de cada campo aparece al apoyar el mouse.
- **Pantalla completa (F11):** a la derecha de **Elaborados sin guardar** hay un botón con el icono de pantalla completa. Al tocarlo la pantalla pasa a pantalla completa y, al tocarlo de nuevo, sale; **la tecla F11** hace lo mismo. Al apoyar el mouse sobre el icono aparece *F11 · Pantalla completa* (o *F11 · Salir de pantalla completa* cuando ya está activa) y el icono cambia entre expandir y contraer. Si el navegador no soporta la pantalla completa, el botón no se muestra. Al cambiar de pantalla el navegador sale de pantalla completa solo. PENDIENTE: si se entró con el F11 nativo del navegador (sin pasar por la página), el botón no puede salir de ese modo; hay que volver a tocar F11.
- **Código y Nombre del elaborado** van en **un solo renglón**, con la **última actualización** a la derecha.
- **Cantidad en cero o vacía:** al cerrar el aviso «Ingresa una cantidad de kilos mayor a cero» el cursor vuelve a **Cantidad**.
- **Ingredientes manuales en una fila:** **Código, Ingrediente (con F10), Cantidad y +**, y a continuación, **en la misma línea**, **Balanza**, **Ver acumulados** y el teclado solo con su **icono** (el nombre aparece al apoyar el mouse); el estado de la balanza queda debajo de los campos. Desde ~1366 px de ancho entra todo en una línea; a 1280 px el teclado baja a la línea de abajo y a 1200 px bajan también Balanza y Ver acumulados. **Código** muestra **15 caracteres** y **Cantidad** **8** (medido 2026-11-11); el nombre del ingrediente se ve en ~100 px a 1366 px y en ~200 px a 1524 px (los nombres largos se cortan al final del campo). Con el **teclado numérico desplegado** vuelve el cuadro de siempre y la lista cede lugar.
- **Tabla de ingredientes:** letra de 14 px, encabezado fijo y filas alternadas; **el ingrediente y los kilos van en negrita**, con lugar para unos **30 caracteres** y los kilos pegados al nombre (si es más largo, sigue en un segundo renglón).
- **Tabla Fórmula:** misma letra que la tabla de ingredientes (14 px, cifras tabulares, filas de la misma altura) y **filas alternadas** con el mismo tono; el **ingrediente y los kilos van en negrita**, con un punto verde por fila (el color del sector Ingredientes) y una etiqueta **Auto** (verde: se agrega sola al guardar) o **Manual** (borde azul: hay que cargarla a mano) en lugar de Sí / No.
- **Receta y Fórmula** a la derecha; **Observaciones** y los botones (**Modificar** o **Guardar**, **Anular**, **Cancelar**) quedan **al pie de la columna derecha**, siempre a la vista. **Observaciones** es una línea y se agranda al tocarla.
- Con el **costeo** (administradores) la columna derecha es más alta y puede aparecer un scroll dentro de ella.
- **Fila resaltada:** al pasar el mouse por una fila de las tablas (ingredientes, Fórmula y Ver acumulados) se pinta de azul tenue, para seguir el producto con su cantidad.
- **Balanza como icono:** el interruptor pasó a un **icono de balanza con «Balanza» en chiquito**, **verde** si está activada y **gris** si no (se toca, o con la tecla **`*`**, que la activa o desactiva y **deja el cursor en Cantidad**; tocar el icono hace lo mismo). Con la balanza activa, **Cantidad** queda de solo lectura pero bien visible (fondo verde con el peso); **Código y Cantidad** van en negrita y **todo campo con el cursor se marca con borde verde**. **Enter**: Código ↵ pasa a Cantidad y Cantidad ↵ **registra la línea** y vuelve a Código. En el celular el icono sigue oculto, como antes.
- **Cursor al abrir (2026-11-13):** al abrir un registro **nuevo** el cursor queda en el **Código del cuadro «1 Elaborado»** (el producto final; con Enter pasa a los ingredientes). Si el registro ya existe (o la pantalla está en modo ver) no se mueve. El sistema insiste unos segundos porque otras partes de la pantalla (borradores, avisos) le sacaban el foco al cargar; deja de insistir apenas tocás algo. El Código del elaborado tiene el mismo estilo que el de los ingredientes (negrita, foco verde).
- **Columna # y orden:** la tabla de ingredientes tiene una columna **#** con el número de carga (1, 2, 3…, no cambia al ordenar), **Código** (reserva **14 caracteres**), **Ingrediente** (pegado a Cantidad), **Cantidad** (antes *Kilos*), **Balanza** y **Acción**. Tocá cualquier encabezado (menos Acción) para ordenar de menor a mayor: **#**, **Código**, **Ingrediente**, **Cantidad** o **Balanza**; tocalo otra vez para invertir (la flecha ↑/↓ marca la columna). El orden **no cambia el orden en que se guardan** las líneas.
- **Ver acumulados:** el modal también tiene **#** (orden en que se cargó por primera vez cada producto) y se ordena tocando **Codigo**, **Producto**, **Cantidad** o **Veces**. Al abrirlo arranca ordenado por **código**. En el modal, la columna **Veces** (la última) dice en cuántos ítems de la tabla está cargado ese producto: **×1** en gris es lo normal; **×2 o más** va en ámbar con ⚠ (posible carga duplicada). Tocando la fila (o con Enter) se despliegan las cargas una por una, con su **Ítem #** de la tabla principal, y al pie el total, que coincide con la fila. Arriba hay un resumen («N productos distintos en M ítems · ⚠ X repetidos») y el tilde **Solo repetidos** deja únicamente los repetidos. Al abrir el modal todo arranca cerrado y sin filtro. En celular, "Veces" va junto al código de cada tarjeta.
- **Abrir en ventana (2026-11-13):** el botón con el icono ↗ (arriba a la derecha, antes de *Pantalla completa*; solo con 1200 px o más) abre el elaborado en **otra ventana del navegador** sin menú lateral ni barra superior (antes estaba oculto desde el 2026-11-04). Mide **un tercio del ancho de la pantalla y todo el alto** (piso de 480 px). Mismo diseño compacto que Movimientos: sin las pestañas, Sucursal / Fecha en un renglón con la etiqueta chica arriba (el Usuario no se repite: ya está en la barra), los cuadros **1 Elaborado** y **2 Ingredientes** con los campos en una fila, la tabla con columnas justas y debajo Receta, la Fórmula (con la tabla acotada a ~8 rem de alto y su scroll; sin «Mostrar costeo»), Observaciones y los botones. «Elaborados sin guardar» queda como icono con el número. **Quién opera:** la barra muestra el **usuario con sesión** y, si es la cuenta de producción, **«Operador: <nombre>»**.
- **PENDIENTE (no medido):** modo oscuro y la tabla con el costeo de administrador abierto.

## En el celular
- **Ingredientes cargados:** se ven como una lista. Arriba el ingrediente y los kilos; abajo el código (`Cód. 3`) y el icono de **balanza** (azul = pesado en balanza, gris = no). El botón rojo de la derecha **quita la línea**. Con el **costeo** activado (solo administradores) debajo de cada línea aparece su costo.
- **Orden:** arranca con **lo último cargado arriba**. El selector de la derecha permite cambiarlo (primeros primero, código, ingrediente, kilos). Con la **lupa** se filtra por ingrediente o código.
- El selector de orden y la **lupa** **siguen activos en el modo ver** (cuando los campos y la tabla están bloqueados): solo cambian cómo se ven las líneas.
- El orden en que se ven las líneas **no cambia el orden en que se guardan**: siempre se guardan en el orden en que se cargaron.
- **Fórmula** y **Ver acumulados** también se muestran como lista (en la fórmula, debajo de cada ingrediente dice *Auto* si se agrega solo al guardar o *Manual*).
- En la computadora la tabla no tiene selector de orden ni lupa: se ordena tocando los encabezados (ver abajo).
- **Observaciones** sigue siendo el último bloque, de ancho completo, justo arriba de los botones.

## Si ya tenés elaborados abiertos en otras pestañas (2026-11-12)
Al abrir una **carga nueva**, si en este navegador ya tenés otras abiertas, aparece un aviso con la **lista** de lo que tenés abierto: nombre del elaborado, si es **nuevo** o **edición #número**, y cuántas líneas tiene.
- Cada carga **nueva con algo cargado** trae su botón **Traer este**: el trabajo pasa a esta pestaña y la otra queda tapada con un botón **Ir al listado**. Las ediciones y las cargas vacías se listan pero no se pueden traer.
- Además podés **Cargar otro nuevo aparte** (esta carga es independiente) o **Volver al listado**.
- **Modificar uno ya guardado:** el aviso aparece al tocar **Modificar** si otra pestaña ya lo está modificando (**Traer la modificación a esta pestaña**, **Cargar uno nuevo aparte** o **Volver al listado**).
- Solo detecta pestañas del **mismo navegador** de esta computadora.

## Resto de la pantalla
PENDIENTE: documentar sucursal, fecha, búsqueda de elaborado e ingredientes, balanza, observaciones, modificar/anular y costeo (no verificado contra el código en esta revisión).
