---
pantalla: Elaborados.Index
titulo: Elaborados
rol: usuario
modulo: Elaborados
orden: 1
permiso: PENDIENTE
revisada: 2026-10-05
---
## Para qué sirve
Listado de los **elaborados registrados** (por sucursal y fechas) con el total de kilos y un detalle desplegable de cada uno.

## Fórmula secreta
En el detalle de un registro, si el producto tiene una fórmula [secreta](ayuda:concepto/formula-secreta), **se ocultan solo los ingredientes que salen de la fórmula** (los que el sistema agrega solo y el producto "Ajuste Fórmula"; en un ingreso rápido, todos los de la fórmula) y la receta. Los ingredientes que el operador cargó a mano siguen visibles. Debajo aparece una fila *"Ingredientes de la fórmula secreta ocultos."* (sin cantidades) con el botón **Ver fórmula**. En la fila del listado, la **Cantidad** no suma los ingredientes ocultos y lleva la etiqueta *"+ ingredientes"* para avisarlo; el total de arriba tampoco los incluye. Abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (la lista muestra solo a quienes tienen permiso sobre fórmulas) y se escribe su contraseña; si valida, el detalle se vuelve a cargar con todos los ingredientes y aparece **Ocultar fórmula**. La fórmula queda visible 5 minutos como máximo.

## Columnas Cantidad y Creado
- **Cantidad** (antes "Kgs"): puede ser unitaria, según el elaborado.
- **Creado**: fecha y hora de creación y, debajo en gris, quién lo creó. Si el registro se **modificó**, aparece además la etiqueta amarilla *Modificado* con la fecha y hora de la última modificación y, debajo, quién la hizo.

## Resto de la pantalla
PENDIENTE: documentar filtros, vista completa, totales y acciones (no verificado contra el código en esta revisión).
