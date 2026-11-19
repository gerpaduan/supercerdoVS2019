---
pantalla: Elaborados.EditarIngresoRapido
titulo: Ingreso rápido y desarme de elaborado
rol: usuario
modulo: Elaborados
orden: 3
permiso: Elaborado.IngresoEmbutidoRapido (editar) / Elaborado.VerEmbutidos (solo ver un registro existente)
revisada: 2026-11-19
---
## Para qué sirve
Registrar un elaborado (o desarmarlo) cargando **solo la cantidad final**: el sistema calcula los ingredientes con la fórmula del producto y los guarda. Se llega desde *Elaborados → Ingreso rápido* (o *Desarme de elaborado*) eligiendo un producto, o abriendo un registro existente desde el listado.

## Cursor al abrir (2026-11-13)
Al abrir un Ingreso rápido o un Desarme **nuevo**, el cursor queda en **Cantidad a elaborar** (o *a desarmar*): escribís (o llega el peso de la balanza) y **Enter guarda**. Si el registro ya existe o está anulado, no se mueve. El sistema insiste unos segundos porque otras partes de la pantalla le sacaban el foco al cargar; deja de insistir apenas tocás algo.

## Fórmula secreta
Si la fórmula del producto es [secreta](ayuda:concepto/formula-secreta), el panel **Fórmula calculada** no muestra ingredientes ni porcentajes y la **Receta** está oculta. Se ve el aviso *"La fórmula de este elaborado es secreta. Los ingredientes se calculan y se descuentan automáticamente al guardar"* y el botón **Ver fórmula**.

- **Cargar y guardar funciona igual**: ingresás la cantidad y guardás; el sistema descuenta los ingredientes de la fórmula aunque no los hayas visto.
- **Ver fórmula**: abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (solo aparecen quienes tienen permiso sobre fórmulas) y se escribe su contraseña (se pide siempre). Si valida, aparecen la receta, los ingredientes con sus kilos (se recalculan en vivo con la cantidad) y, solo para administradores, el costeo. Aparece el botón **Ocultar**.
- La fórmula vuelve a ocultarse sola a los 5 minutos, con **Ocultar** o al salir de la pantalla. Para verla de nuevo hay que volver a ingresar usuario y contraseña.
- Los borradores de *Elaborados sin guardar* no guardan la fórmula de un producto secreto.

## Teclado digital
Entre la **cantidad** y la **fórmula calculada** hay un teclado numérico en pantalla (7-9, 4-6, 1-3, 0 y punto; borrar y `*`), siempre visible y solo en pantallas grandes. Escribe en el campo de cantidad. **Enter** del teclado guarda el ingreso (hace lo mismo que Enter en el campo o el botón *Guardar*). No escribe si el campo es de solo lectura: con la balanza leyendo el peso, o cuando se abre un registro en modo vista hasta tocar *Modificar*.

**Punto y coma decimal (2026-11-14):** en la cantidad, el **punto** (del teclado físico o del teclado en pantalla) y la **coma** son siempre el separador decimal; el punto se convierte en coma al escribirlo, y nunca se interpreta como separador de miles. Escribir `5.325` o `5,325` carga **5,325 kg**. Solo admite un separador.

## En la computadora (pantalla de 1200 px o más)
Igual para **Ingreso rápido** y **Desarme**; pensado para ver todo sin bajar con la rueda, con una fórmula de hasta **10 ingredientes** (ventana de 690 px de alto o más):
- **Pestañas y botones en un renglón:** las pestañas a la izquierda, **Volver** y **Elaborados sin guardar** a la derecha. La etiqueta *Ingreso rápido* / *Desarme* ya no se muestra (repite la pestaña activa); *Anulado* sí.
- **Sucursal, Fecha y Usuario** en una fila, con la **fecha siempre completa, con hora y segundos**. Debajo, **Código**, **Elaborado** y **Receta**. Los valores de Sucursal, Fecha, Usuario y Código se ven en **negrita**; el Código y el Elaborado con letra más grande (el Elaborado, del mismo tamaño que la cantidad).
- Tres bloques: **Cantidad** (a elaborar o a desarmar), **Teclado** (teclas más bajas) y **Fórmula calculada**. La leyenda «La fórmula se calcula automáticamente en vivo» (o «se mostrará en negativo» en el desarme) ya no aparece en pantalla.
- La tabla de la fórmula muestra **10 ingredientes** de una línea cada uno; con más, tiene su propio scroll con el encabezado fijo. Al pasar el mouse por un ingrediente, la fila se pinta de azul tenue.
- Con el **costeo** (administradores) la pantalla es más alta y puede aparecer el scroll de la página. Con una ventana de menos de 690 px de alto la página scrollea un poco.

## Salir de un registro existente
Un registro existente se abre en **modo vista**. Mientras no se toque **Modificar**, se puede salir (Volver, Cancelar o cualquier enlace) sin ningún aviso, porque no hay nada que perder. Al tocar **Modificar**, el aviso de *salir sin guardar* solo aparece si después se cambió algo.

## Si ya tenés ingresos rápidos o desarmes abiertos en otras pestañas (2026-11-12)
Al abrir un **ingreso rápido o desarme nuevo**, si en este navegador ya tenés otros abiertos, aparece un aviso con la **lista**: nombre del elaborado, si es nuevo o edición, y la cantidad cargada. Cada fila muestra **qué es** (nuevo o edición), **dónde está** (pestaña del navegador, ventana independiente, app instalada o **dentro del POS**, menú Cargas, a la vista o minimizada), **desde qué hora** (y hace cuánto) y **qué tiene cargado** (los primeros productos con su cantidad; el resto, «y N más»).
- **Traer este** solo aparece en los que son del **mismo elaborado y del mismo modo** (ingreso o desarme) y ya tienen cantidad: el trabajo pasa a esta pestaña y la otra queda tapada con un botón **Ir al listado**.
- **Señalar:** esa pestaña o ventana hace titilar su título unos segundos y muestra una cinta amarilla, para que la encuentres en la barra de pestañas o de tareas (el navegador no permite traerla al frente desde acá).
- **Cerrar** (solo en un nuevo **vacío**): esa pestaña vuelve al listado y, si era la única, el aviso desaparece y seguís acá. Si en el medio le cargaron algo, no se cierra.
- Una carga abierta **dentro del POS** no se puede señalar ni cerrar desde acá: se vuelve a ella con el botón amarillo «… en curso» de la barra del POS.
- Si abrís la pantalla con **Abrir en ventana**, la ventana nueva no te avisa de la pestaña de origen (que vuelve al listado). Un F5 de la ventana vuelve a chequear como siempre.
- Si el aviso aparece y **parece que no hay nada abierto**, casi siempre es una carga minimizada del POS, una ventana o app instalada que no estás mirando, o una pestaña que el navegador restauró al abrirse; la fila te dice cuál.
- También podés **Cargar otro nuevo aparte** o **Volver al listado**. Al tocar **Modificar** en uno ya guardado, si otra pestaña lo está modificando, el aviso te deja traer esa modificación.
- Solo detecta pestañas del **mismo navegador** de esta computadora.

## Resto de la pantalla
PENDIENTE: documentar sucursal, fecha, balanza, desarme, modificar/anular y costeo (no verificado contra el código en esta revisión).

## Botones − / + de ancho (ventana independiente y app instalada, desde 2026-11-17)
- En la **ventana independiente** y en la **app instalada** (no en una pestaña normal) hay dos botones **−** y **+** justo a la izquierda de «… sin guardar», arriba a la derecha. Cada clic achica o agranda el **ancho de la ventana un 10 %** del ancho con que se abrió (mínimo 50 %, máximo hasta el borde izquierdo de la pantalla). **El borde derecho de la ventana queda fijo**: crece o se achica hacia la izquierda; el alto no cambia.
- Dejá el mouse sobre un botón para ver el nivel actual. Se recuerda el último nivel de cada sección.
- Si el navegador no deja cambiar el tamaño de la ventana, avisa «El navegador no permite cambiar el tamaño de esta ventana».
- Solo aparecen donde está el botón de registros sin guardar (si ese botón está apagado en el sistema, tampoco aparecen).
- PENDIENTE: no se probó en una app instalada real si el navegador permite cambiar el tamaño de la ventana.

## Desde el POS (desde 2026-11-18)
También se abre desde el botón **Cargas** del POS (un ingreso rápido nuevo, sin salir de la caja): minimizable, con aviso «en curso» y con la lista de lo cargado desde la apertura de la caja. Ver [Punto de venta](ayuda:pantalla/Ventas.POS), sección «Cargas desde el POS».

## Botones sin foco del teclado (2026-11-19)
- **Guardar ingreso rápido / desarme**, **Cancelar**, **Modificar** y **Anular** (atajo: Alt+Enter para Guardar o Modificar). Se mantiene el guardado con Enter en el campo **Cantidad** (y el Enter del teclado digital), que es parte del flujo de pesada. **no toman el foco**: Tab no se detiene en ellos y, al hacer clic, el cursor sigue donde estaba (no queda el recuadro de foco ni un Enter o Espacio posterior los vuelve a activar). Solo responden al **clic** o al **atajo** de la pantalla.
- **Enter dentro de un campo no guarda** la pantalla (antes el navegador lo trataba como un clic en Guardar). Enter sigue haciendo lo suyo en los campos que lo usan (Código, Cantidad, editor de línea).
- Detalle técnico: `sin-foco-botones.js` + atributo `data-sin-foco` en cada botón; ver `docs/DECISIONS.md` (2026-11-19).
