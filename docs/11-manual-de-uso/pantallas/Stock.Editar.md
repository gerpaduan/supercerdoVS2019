---
pantalla: Stock.Editar
titulo: Cargar o modificar un movimiento de stock
rol: usuario
modulo: Stock
orden: 2
permiso: Stock.AddOrEditStock para guardar (PENDIENTE: en WebCore el chequeo de abrir la pantalla está omitido, ver TODO en StockController)
revisada: 2026-10-27
alias: Stock.Nuevo
---
## Para qué sirve
Cargar un movimiento de stock nuevo (ingreso, egreso, ajuste, cierre, **pesaje**, etc.) o modificar uno ya guardado. Esta página documenta por ahora solo la parte de **vincular y desvincular pesajes**; el resto de la pantalla está pendiente (ver al final).

## Vincular y desvincular pesajes
Aplica cuando el tipo del movimiento es **Pesaje**. El botón **Vincular pesajes** abre una lista de pesajes de la misma sucursal (por defecto, de los últimos 7 días; se puede cambiar con *Fecha desde / Fecha hasta* y el buscador de proveedor).

### Vincular
1. En la fila del pesaje que querés sumar, tocá **Detalle** para ver sus líneas (opcional) y **Vincular**.
2. Las líneas de ese pesaje se **agregan a este pesaje** (con la etiqueta *Pesaje vinculado #N*) y arriba aparece el aviso *Pesajes vinculados en esta edición*.
3. El vínculo queda guardado recién cuando tocás **Guardar**.

### Desvincular
1. Abrí **Vincular pesajes**. Los pesajes que ya están vinculados a este muestran el botón rojo **Desvincular** (antes decían "Ya vinculado"). Si el pesaje es muy viejo y no aparece, ampliá *Fecha desde*.
2. Tocá **Desvincular**. El sistema pide confirmación y dice **cuántas líneas (y cuántos kg) se van a quitar** de este pesaje.
3. Al confirmar, las líneas de ese pesaje **desaparecen de la tabla** y el pesaje deja de figurar como vinculado. **Nada queda definitivo hasta que tocás Guardar**: si salís sin guardar, todo vuelve a como estaba.
4. Si te arrepentís antes de guardar, podés **volver a vincular** el mismo pesaje desde la misma lista.

Cosas a tener en cuenta:
- Las líneas se reconocen por **producto y kilos**. Si el pesaje vinculado se **modificó después de haberlo vinculado**, o si ya borraste a mano algunas de sus líneas, el aviso dice cuántas **no se encontraron**: esas no se tocan y, si hace falta, se quitan a mano con el tacho de cada fila.
- Si este pesaje tiene dos líneas idénticas (mismo producto y kilos) y el pesaje vinculado aportó una, se quita una de las dos (son equivalentes).
- Al guardar, el pesaje desvinculado queda marcado como *modificado* (cambia su fecha y usuario de última modificación).
- La confirmación se puede cancelar sin efecto.

## Eliminar un movimiento de stock
El botón rojo **Eliminar** aparece al abrir un movimiento **ya guardado** (ingreso, egreso, cierre, ajuste o pesaje) para quien puede modificarlo. Funciona igual que en [Compras](ayuda:pantalla/Compras.Editar): resumen y líneas, **motivo obligatorio** (10 a 300 caracteres) y **usuario y contraseña en cada eliminación** (ver [eliminar registros](ayuda:concepto/eliminar-registros)).
- El registro **se borra** y el stock de la sucursal se recalcula sin él. Queda la constancia (quién, cuándo, motivo y todas las líneas) en [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index).
- **Ajuste de stock y Cierre de stock:** solo administradores. Un **cierre** es el punto de partida del cálculo: al eliminarlo cambia el stock de la sucursal desde el cierre anterior (la ventana lo avisa).
- Un **Pesaje** con pesajes vinculados o con un **ajuste generado** no se puede eliminar: primero se desvinculan o se elimina el ajuste. Al eliminar un **ajuste**, el pesaje que ajustaba vuelve a «No realizado».
- No se puede deshacer desde el sistema.

## Resto de la pantalla
PENDIENTE: documentar los datos del encabezado, la carga de productos (código, balanza, teclado numérico), Productos no cargados, Acumulados, Ver porcentaje de cortes y Generar ajuste, Vincular compra, borradores y las reglas por tipo de movimiento (no verificado contra el código en esta revisión).

## Datos del registro
- En la pantalla de un registro ya guardado, **Creado** y **Actualizado** (con el usuario debajo de cada fecha) se muestran en un bloque compacto de dos columnas, con letra chica, para ocupar poca altura.
