---
pantalla: Elaborados.Carga
titulo: Carga / ingreso de elaborado
rol: usuario
modulo: Elaborados
orden: 2
permiso: Elaborado.IngresoEmbutido (editar) / Elaborado.VerEmbutidos (solo ver un registro existente)
revisada: 2026-10-09
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

## Resto de la pantalla
PENDIENTE: documentar sucursal, fecha, búsqueda de elaborado e ingredientes, balanza, observaciones, modificar/anular y costeo (no verificado contra el código en esta revisión).
