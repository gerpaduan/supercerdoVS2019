---
pantalla: Elaborados.Index
titulo: Elaborados
rol: usuario
modulo: Elaborados
orden: 1
permiso: PENDIENTE
revisada: 2026-10-27
---
## Para qué sirve
Listado de los **elaborados registrados** (por sucursal y fechas) con el total de kilos, la cantidad de elaborados mostrados y un detalle desplegable de cada uno.

## Tarjetas de totales
- **Total Kg**: suma de la cantidad de los elaborados visibles en la tabla, **sin contar los anulados** (las filas que tienen algo en *Estado*).
- **Elaborados mostrados**: cuántos elaborados visibles hay en la tabla, **sin contar los anulados**.
- Ambos se recalculan al escribir en *Nombre elaborado* (filtro en vivo). Los anulados siguen apareciendo en la tabla; si hay alguno visible, debajo de las tarjetas, en letra chica, dice cuántos *"anulados excluidos del total y de la cantidad"*.

## Fórmula secreta
En el detalle de un registro, si el producto tiene una fórmula [secreta](ayuda:concepto/formula-secreta), **se ocultan solo los ingredientes que salen de la fórmula** (los que el sistema agrega solo y el producto "Ajuste Fórmula"; en un ingreso rápido, todos los de la fórmula) y la receta. Los ingredientes que el operador cargó a mano siguen visibles. Debajo aparece una fila *"Ingredientes de la fórmula secreta ocultos."* (sin cantidades) con el botón **Ver fórmula**. En la fila del listado, la **Cantidad** no suma los ingredientes ocultos y lleva la etiqueta *"+ ingredientes"* para avisarlo; el total de arriba tampoco los incluye. Abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (la lista muestra solo a quienes tienen permiso sobre fórmulas) y se escribe su contraseña; si valida, el detalle se vuelve a cargar con todos los ingredientes y aparece **Ocultar fórmula**. La fórmula queda visible 5 minutos como máximo.

## Columnas Cantidad y Creado
- **Cantidad** (antes "Kgs"): puede ser unitaria, según el elaborado.
- **Creado**: fecha y hora de creación y, debajo en gris, quién lo creó. Si el registro se **modificó**, aparece además la etiqueta amarilla *Modificado* con la fecha y hora de la última modificación y, debajo, quién la hizo.

## Resto de la pantalla
PENDIENTE: documentar filtros, vista completa, totales y acciones (no verificado contra el código en esta revisión).

## En el celular
- En pantallas angostas (celular) cada elaborado se ve como **tarjeta**: arriba el **nombre y el código**, después fecha, sucursal, **cantidad**, quién lo creó, y el estado y las observaciones si los tiene. No hace falta desplazarse de costado.
- Al pie están **Ver** y **Detalles** (abre debajo de la tarjeta el detalle del elaborado). Los elaborados anulados se ven con la tarjeta en amarillo y los desarmes en celeste.
- Arriba a la derecha del nombre, en gris, va el **número del registro**.
- **Detalles** en el celular: no se muestra el recuadro de datos del registro. Arriba de los ingredientes aparecen las **observaciones** (si hay); la receta no, porque solo se usa en las cargas. Cada ingrediente muestra el producto y los kilos arriba, y abajo el código y el icono de **balanza** (azul = pesado en balanza, gris = no). Si la fórmula es secreta y está oculta, queda el renglón con el candado y **Ver fórmula**.
- En la computadora la pantalla se ve igual que antes.
