---
pantalla: Reposicion.Puntos
titulo: Sugerir puntos de stock
rol: admin
modulo: Stock
orden: 5
permiso: Stock.VerStock para ver; para cargar los puntos además el de editar productos (Producto.NuevoCorte), el mismo que ya pide Productos
revisada: 2026-10-13
---
## Para qué sirve
**Qué pedir** calcula cuánto pedir contra el **punto de stock** de cada producto, pero la mayoría de los productos todavía no lo tiene cargado. Esta pantalla **propone un punto de stock** para los que no lo tienen, según lo que realmente se consume, y te deja **cargar los que vos elijas** de una vez, en lugar de entrar producto por producto en *Productos → Ver stock por sucursales*.

## Cómo se usa
1. Elegí las **Sucursales** (al abrir viene la tuya) y dos números:
   - **Mirar el consumo de los últimos N días** (30 por defecto): período con el que se calcula cuánto se consume por día.
   - **El punto debe cubrir N días de consumo** (7 por defecto): cuántos días de mercadería querés tener al pedir "hasta el punto".
2. **Buscar** (al abrir ya busca solo). Aparece una fila por producto y sucursal **sin punto cargado y con consumo**, de lo que más se mueve a lo que menos.
3. Revisá la columna **Punto sugerido**: es un número editable. Si lo tocás, la fila se tilda sola. Corregilo si hace falta (productos de temporada, una promoción que distorsionó la venta, un quiebre que hizo caer el consumo).
4. Tildá los que quieras cargar (la casilla de arriba tilda todos) y tocá **Cargar puntos de stock seleccionados**. Pide confirmación y, al terminar, avisa cuántos se cargaron y recalcula: los cargados salen de la lista.

## Cómo se calcula el punto sugerido
**Punto sugerido = consumo por día × días a cubrir**, redondeado hacia arriba (mínimo 1). El *consumo por día* es lo que salió en el período (**ventas + uso en elaboración**, igual que en *Qué pedir*) dividido por los días del período. El punto de stock es siempre un **número entero** (kilos en productos pesables, unidades en el resto).

## Qué muestra cada columna
- **Stock / Punto actual:** el stock de hoy y el punto cargado (— = sin punto).
- **Consumo/día** y **Alcanza** (días que dura el stock actual a ese ritmo).
- **Días con venta** ("10 de 30"): en cuántos días del período hubo al menos una venta. Un producto que se vendió 2 días de 30 sale **a golpes**: el promedio engaña, mirá con más cuidado el número. (El uso en elaboración no cuenta acá, por eso un insumo puede mostrar "—".)
- **Última salida** y **Proveedor** (el de la compra más reciente).

## Qué NO hace
- **Nunca pisa un punto ya cargado.** Con *Mostrar también los que ya tienen punto* en **Más filtros y opciones** se listan solo para comparar, sin casilla ni número editable; el servidor además lo rechaza si alguien intenta enviarlo. Para cambiar un punto cargado usá *Productos → Ver stock por sucursales*.
- **No toca el stock ni crea pedidos.** Cargar un punto solo define hasta dónde sugiere pedir *Qué pedir*.
- **No sugiere para productos sin consumo** en el período (no hay base): aparecen en el contador *Sin consumo*; *Más filtros → Mostrar también los que no tuvieron consumo* los lista sin sugerencia.
- **No hay "deshacer":** después de cargar, el punto se cambia a mano desde Productos. Queda registrado en el log del servidor quién cargó cuántos.

## Permisos y límites
- Para **ver** hace falta *Stock → Ver stock*; para **cargar**, el permiso de editar productos. Sin ese permiso la pantalla queda **solo lectura** (sin casillas) y lo avisa abajo.
- Por pedido se pueden cargar **hasta 500** puntos. Valores permitidos: enteros entre 1 y 999.999.
- Si algo no se puede cargar (producto que ya tiene punto, valor inválido), el aviso final lista cada caso con su motivo; el resto sí se carga.
- Si la base falla a mitad de camino, lo ya guardado queda guardado (cada producto se escribe completo o nada): volvé a buscar para ver cómo quedó.
- Hoy solo funciona cuando el sistema usa Postgres.
- PENDIENTE: documentar con capturas; evaluar si 7 días de cobertura es un valor por defecto adecuado para todos los rubros (los insumos de elaboración probablemente piden más días que la carne fresca).
