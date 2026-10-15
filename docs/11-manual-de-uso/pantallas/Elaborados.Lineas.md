---
pantalla: Elaborados.Lineas
titulo: Líneas de elaborado
rol: usuario
modulo: Elaborados
orden: 6
permiso: PENDIENTE
revisada: 2026-10-14
---
## Para qué sirve
Listado de **cada ingrediente utilizado** en los elaborados registrados (una fila por ingrediente de cada registro), por sucursal y fechas.

## Tarjetas de totales
- **Total Kg lineas** y **Total lineas**: suma de cantidades y cantidad de filas de la búsqueda, **sin contar las anuladas** (las que tienen algo en *Estado*).
- Las anuladas siguen apareciendo en la tabla (fila amarilla). Si hay alguna en la búsqueda, debajo de las tarjetas, en letra chica, dice cuántas *"anuladas excluidas del total de Kg y de lineas"*.
- Las tarjetas y el aviso **siguen el filtro en vivo**: se recalculan al escribir en *Descripcion* y al marcar *Filtrar solo ingredientes*, con las líneas que quedan visibles.

## Fórmula secreta
De los registros cuyo producto tiene una fórmula [secreta](ayuda:concepto/formula-secreta), no se muestran los ingredientes: aparece **una sola fila por registro** con *"Fórmula secreta"* y el total de kilos (el total general de la pantalla no cambia). El botón de la llave (**Ver fórmula**) abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (solo aparecen quienes tienen permiso sobre fórmulas) y se escribe su contraseña; si valida, la pantalla se recarga con las líneas reales y las filas de ese producto muestran el botón **Ocultar fórmula**. Visible 5 minutos como máximo.

## Resto de la pantalla
PENDIENTE: documentar filtros, "Filtrar solo ingredientes" y el botón *Seleccionar* (no verificado contra el código en esta revisión).
