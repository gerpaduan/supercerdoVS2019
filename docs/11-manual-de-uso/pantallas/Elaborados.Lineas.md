---
pantalla: Elaborados.Lineas
titulo: Líneas de elaborado
rol: usuario
modulo: Elaborados
orden: 6
permiso: PENDIENTE
revisada: 2026-11-02
---
## Para qué sirve
Listado de **cada ingrediente utilizado** en los elaborados registrados (una fila por ingrediente de cada registro), por sucursal y fechas.

## Filtros
- En la **computadora** (pantalla de 1200 px o más) los filtros van en **una sola fila**: **Fecha desde**, **Fecha hasta** (con hora y segundos), **Sucursal**, **Descripcion** y **Buscar**. Debajo, en una barra, van los totales, el aviso de anuladas y el interruptor **Filtrar solo ingredientes**. En pantallas más angostas se ven apilados, como antes.
- Al cambiar la **sucursal** o las **fechas** hay que tocar **Buscar**. Mientras tanto, en la computadora, el botón **Buscar** se pone **amarillo** y el texto de estado dice *Cambiaste sucursal o fechas: tocá Buscar para actualizar.*; en pantallas angostas aparece la alerta amarilla de siempre.
- **Descripcion** y **Filtrar solo ingredientes** filtran en vivo, sin tocar Buscar.

## Tarjetas de totales
- **Total Kg lineas** y **Total lineas**: suma de cantidades y cantidad de filas de la búsqueda, **sin contar las anuladas** (las que tienen algo en *Estado*).
- Las anuladas siguen apareciendo en la tabla (fila amarilla). Si hay alguna en la búsqueda, junto a las tarjetas (en la computadora) o debajo de ellas (en pantallas angostas), en letra chica, dice cuántas *"anuladas excluidas del total de Kg y de lineas"*.
- Las tarjetas y el aviso **siguen el filtro en vivo**: se recalculan al escribir en *Descripcion* y al marcar *Filtrar solo ingredientes*, con las líneas que quedan visibles.

## Fórmula secreta
De los registros cuyo producto tiene una fórmula [secreta](ayuda:concepto/formula-secreta), no se muestran los ingredientes: aparece **una sola fila por registro** con *"Fórmula secreta"* y el total de kilos (el total general de la pantalla no cambia). El botón de la llave (**Ver fórmula**) abre el selector de usuarios (el mismo de Cerrar Caja): se elige de la lista una persona (solo aparecen quienes tienen permiso sobre fórmulas) y se escribe su contraseña; si valida, la pantalla se recarga con las líneas reales y las filas de ese producto muestran el botón **Ocultar fórmula**. Visible 5 minutos como máximo.

## Resto de la pantalla
PENDIENTE: documentar qué hace exactamente "Filtrar solo ingredientes" y el botón *Seleccionar* (no verificado contra el código en esta revisión).
