---
pantalla: RegistrosEliminados.Index
titulo: Registros eliminados
rol: usuario
modulo: Configuración
orden: 40
permiso: administradores (los tres tipos) y quien puede modificar compras, stock o movimientos (solo los eliminados de ese módulo)
revisada: 2026-10-23
alias: RegistrosEliminados.Detalle
---
## Para qué sirve
Ver las **compras, movimientos de stock y movimientos entre sucursales que se eliminaron**: quién los eliminó, cuándo, por qué y **qué tenían**. Es la constancia que queda cuando se usa el botón **Eliminar** de [Compras](ayuda:pantalla/Compras.Editar), [Stock](ayuda:pantalla/Stock.Editar) o [Movimientos](ayuda:pantalla/Movimientos.Editar). Los administradores entran por **Configuración → Registros eliminados**; quien tiene permiso de **modificar** compras, stock o movimientos entra por el botón **Ver eliminados** que está junto a *Ver lineas* en el listado de **Compras**, **Stock** o **Movimientos**.

## Quién ve qué
- **Administradores:** los tres tipos.
- **Quien puede modificar** compras, stock o movimientos: **todos** los eliminados de ese módulo (no solo los que eliminó él). Si puede modificar más de un módulo, ve esos y el filtro *Tipo* solo ofrece los suyos.
- **La cuenta compartida de producción** no ve los eliminados (no hay una persona identificada); sí los ven los administradores.

## Listado
- Filtros: **Tipo** (compras, stock, movimientos), **fecha de eliminación** (por defecto los últimos 30 días) y un buscador de texto (motivo, remito, número del registro, usuario que eliminó o proveedor). Se muestran hasta 1000 por vez.
- Cada fila dice cuándo se eliminó, de qué tipo era, su número, su fecha original, la sucursal, el proveedor o persona, el importe, cuántas líneas tenía, quién lo eliminó y el motivo. Si la sesión estaba abierta con la cuenta compartida de producción, abajo del nombre figura «sesión: …».
- **Ver** abre el detalle.

## Detalle
- **Cómo estaba el registro:** tipo, fecha, sucursal (o origen → destino), proveedor, remito, importe, kilos, observaciones y quién lo había creado.
- **Qué pasó al eliminarlo:** quién, cuándo, desde qué dirección y los **efectos** (por ejemplo el asiento contrario en la cuenta corriente del proveedor o el egreso de caja opuesto).
- **Líneas que tenía:** código, producto (como se llamaba en ese momento), detalle, kilos, unidades y precio.

## Recuperar un registro eliminado
En el detalle, el botón verde **Recuperar** abre la pantalla de **carga** del módulo (nueva compra, nuevo movimiento de stock o nuevo movimiento) **ya completa** con los datos y las líneas del registro eliminado, y un cartel que lo explica. **No se guarda solo**: revisá, corregí lo que haga falta y tocá **Guardar** como en cualquier alta.
- Se guarda como un registro **nuevo, con otro número**; el eliminado sigue figurando en esta pantalla. Una compra a cuenta corriente vuelve a generar su asiento y una compra pagada con la caja, su egreso, como cualquier compra nueva.
- Los productos se cargan con su nombre y código **de hoy**; si un producto ya no existe, esa línea se omite y el cartel lo avisa. La fecha es la del registro original (si es muy anterior y tu permiso no llega, cambiala antes de guardar).
- **No se recupera** un **ajuste de stock** (se vuelve a generar desde el pesaje con *Generar ajuste*), ni los **vínculos** de un pesaje con otros pesajes o compras.
- Aparece solo si podés ver los eliminados de ese módulo **y** cargar ese tipo de registro.

## Notas
- Esta información **no se puede modificar ni borrar** desde el sistema.
