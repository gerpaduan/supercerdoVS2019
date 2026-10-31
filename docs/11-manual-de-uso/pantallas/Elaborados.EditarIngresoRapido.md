---
pantalla: Elaborados.EditarIngresoRapido
titulo: Ingreso rápido y desarme de elaborado
rol: usuario
modulo: Elaborados
orden: 3
permiso: Elaborado.IngresoEmbutidoRapido (editar) / Elaborado.VerEmbutidos (solo ver un registro existente)
revisada: 2026-10-31
---
## Para qué sirve
Registrar un elaborado (o desarmarlo) cargando **solo la cantidad final**: el sistema calcula los ingredientes con la fórmula del producto y los guarda. Se llega desde *Elaborados → Ingreso rápido* (o *Desarme de elaborado*) eligiendo un producto, o abriendo un registro existente desde el listado.

## Fórmula secreta
Si la fórmula del producto es [secreta](ayuda:concepto/formula-secreta), el panel **Fórmula calculada** no muestra ingredientes ni porcentajes y la **Receta** está oculta. Se ve el aviso *"La fórmula de este elaborado es secreta. Los ingredientes se calculan y se descuentan automáticamente al guardar"* y el botón **Ver fórmula**.

- **Cargar y guardar funciona igual**: ingresás la cantidad y guardás; el sistema descuenta los ingredientes de la fórmula aunque no los hayas visto.
- **Ver fórmula**: abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (solo aparecen quienes tienen permiso sobre fórmulas) y se escribe su contraseña (se pide siempre). Si valida, aparecen la receta, los ingredientes con sus kilos (se recalculan en vivo con la cantidad) y, solo para administradores, el costeo. Aparece el botón **Ocultar**.
- La fórmula vuelve a ocultarse sola a los 5 minutos, con **Ocultar** o al salir de la pantalla. Para verla de nuevo hay que volver a ingresar usuario y contraseña.
- Los borradores de *Embutidos sin guardar* no guardan la fórmula de un producto secreto.

## Teclado digital
Entre la **cantidad** y la **fórmula calculada** hay un teclado numérico en pantalla (7-9, 4-6, 1-3, 0 y punto; borrar y `*`), siempre visible y solo en pantallas grandes. Escribe en el campo de cantidad. **Enter** del teclado guarda el ingreso (hace lo mismo que Enter en el campo o el botón *Guardar*). No escribe si el campo es de solo lectura: con la balanza leyendo el peso, o cuando se abre un registro en modo vista hasta tocar *Modificar*.

## En la computadora (pantalla de 1200 px o más)
Igual para **Ingreso rápido** y **Desarme**; pensado para ver todo sin bajar con la rueda, con una fórmula de hasta **10 ingredientes** (ventana de 690 px de alto o más):
- **Pestañas y botones en un renglón:** las pestañas a la izquierda, **Volver** y **Embutidos sin guardar** a la derecha. La etiqueta *Ingreso rápido* / *Desarme* ya no se muestra (repite la pestaña activa); *Anulado* sí.
- **Sucursal, Fecha y Usuario** en una fila, con la **fecha siempre completa, con hora y segundos**. Debajo, **Código**, **Elaborado** y **Receta**.
- Tres bloques: **Cantidad** (a elaborar o a desarmar), **Teclado** (teclas más bajas) y **Fórmula calculada**. La leyenda «La fórmula se calcula automáticamente en vivo» (o «se mostrará en negativo» en el desarme) ya no aparece en pantalla.
- La tabla de la fórmula muestra **10 ingredientes** de una línea cada uno; con más, tiene su propio scroll con el encabezado fijo. Al pasar el mouse por un ingrediente, la fila se pinta de azul tenue.
- Con el **costeo** (administradores) la pantalla es más alta y puede aparecer el scroll de la página. Con una ventana de menos de 690 px de alto la página scrollea un poco.

## Salir de un registro existente
Un registro existente se abre en **modo vista**. Mientras no se toque **Modificar**, se puede salir (Volver, Cancelar o cualquier enlace) sin ningún aviso, porque no hay nada que perder. Al tocar **Modificar**, el aviso de *salir sin guardar* solo aparece si después se cambió algo.

## Resto de la pantalla
PENDIENTE: documentar sucursal, fecha, balanza, desarme, modificar/anular y costeo (no verificado contra el código en esta revisión).
