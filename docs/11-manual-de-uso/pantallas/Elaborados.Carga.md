---
pantalla: Elaborados.Carga
titulo: Carga / ingreso de elaborado
rol: usuario
modulo: Elaborados
orden: 2
permiso: Elaborado.IngresoEmbutido (editar) / Elaborado.VerEmbutidos (solo ver un registro existente)
revisada: 2026-11-04
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
Pensado para ver **20 ingredientes o más sin bajar con la rueda**: la tabla de ingredientes ocupa todo el alto de la ventana y crece a medida que cargás; recién cuando llena la pantalla tiene su propio scroll. En una ventana de 1920 × 860 se ven unos **23 ingredientes**. (La tabla **Fórmula** de la derecha mantiene su tope de 10 ingredientes con scroll propio.)
- **Pestañas y botones en un renglón:** las pestañas de Elaborados a la izquierda; **Volver** y **Embutidos sin guardar** a la derecha. Si la ventana es angosta (1200-1350 px) los botones bajan a un segundo renglón; nunca quedan tapados por las pestañas.
- **Sucursal, Fecha, Usuario y Creado siempre a la vista, sin etiquetas:** la fila de siempre (la **fecha se ve completa, con hora y segundos**) sin la etiqueta de arriba; el nombre de cada campo aparece al apoyar el mouse.
- **Código y Elaborado** van en **un solo renglón** (sin título), con la **última actualización** a la derecha.
- **Cantidad en cero o vacía:** al cerrar el aviso «Ingresa una cantidad de kilos mayor a cero» el cursor vuelve a **Cantidad**.
- **Ingredientes manuales en una fila:** **Código, Ingrediente (con F10), Cantidad y +**, y a continuación **Balanza**, **Ver acumulados** y el teclado solo con su **icono** (el nombre aparece al apoyar el mouse) y el estado de la balanza. Con el **teclado numérico desplegado** vuelve el cuadro de siempre y la lista cede lugar.
- **Tabla de ingredientes:** letra de 14 px, encabezado fijo y filas alternadas; **el ingrediente y los kilos van en negrita**, con lugar para unos **30 caracteres** y los kilos pegados al nombre (si es más largo, sigue en un segundo renglón).
- **Receta y Fórmula** a la derecha; **Observaciones** y los botones (**Modificar** o **Guardar**, **Anular**, **Cancelar**) quedan **al pie de la columna derecha**, siempre a la vista. **Observaciones** es una línea y se agranda al tocarla.
- Con el **costeo** (administradores) la columna derecha es más alta y puede aparecer un scroll dentro de ella.
- **Fila resaltada:** al pasar el mouse por una fila de las tablas (ingredientes, Fórmula y Ver acumulados) se pinta de azul tenue, para seguir el producto con su cantidad.
- **Columna # y orden:** la tabla de ingredientes tiene una columna **#** con el número de carga (1, 2, 3…, no cambia al ordenar). Tocá el encabezado **#**, **Codigo**, **Ingrediente** o **Kilos** para ordenar de menor a mayor; tocalo otra vez para invertir (la flecha ↑/↓ marca la columna). El orden **no cambia el orden en que se guardan** las líneas.
- **Ver acumulados:** el modal también tiene **#** (orden en que se cargó por primera vez cada producto) y se ordena tocando **Codigo**, **Producto** o **Cantidad**. Al abrirlo arranca ordenado por **código**.
- **Abrir en ventana:** El botón **Abrir en ventana** está **oculto por ahora** (pedido del 2026-11-04). La pantalla se sigue pudiendo usar sin menú si se instala como aplicación desde `https://carnisys.com` (*Instalar CarniSys …*; PENDIENTE: no se probó).
- **PENDIENTE (no medido):** modo oscuro y la tabla con el costeo de administrador abierto.

## En el celular
- **Ingredientes cargados:** se ven como una lista. Arriba el ingrediente y los kilos; abajo el código (`Cód. 3`) y el icono de **balanza** (azul = pesado en balanza, gris = no). El botón rojo de la derecha **quita la línea**. Con el **costeo** activado (solo administradores) debajo de cada línea aparece su costo.
- **Orden:** arranca con **lo último cargado arriba**. El selector de la derecha permite cambiarlo (primeros primero, código, ingrediente, kilos). Con la **lupa** se filtra por ingrediente o código.
- El selector de orden y la **lupa** **siguen activos en el modo ver** (cuando los campos y la tabla están bloqueados): solo cambian cómo se ven las líneas.
- El orden en que se ven las líneas **no cambia el orden en que se guardan**: siempre se guardan en el orden en que se cargaron.
- **Fórmula** y **Ver acumulados** también se muestran como lista (en la fórmula, debajo de cada ingrediente dice *Auto* si se agrega solo al guardar o *Manual*).
- En la computadora la tabla no tiene selector de orden ni lupa: se ordena tocando los encabezados (ver abajo).
- **Observaciones** sigue siendo el último bloque, de ancho completo, justo arriba de los botones.

## Resto de la pantalla
PENDIENTE: documentar sucursal, fecha, búsqueda de elaborado e ingredientes, balanza, observaciones, modificar/anular y costeo (no verificado contra el código en esta revisión).
