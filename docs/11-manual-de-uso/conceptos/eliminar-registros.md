---
titulo: Eliminar compras, stock y movimientos
rol: usuario
modulo: Acceso y seguridad
orden: 36
revisada: 2026-10-22
---
# Eliminar compras, stock y movimientos

## Dónde aplica
El botón **Eliminar** de la pantalla de edición de una [compra](ayuda:pantalla/Compras.Editar), de una operación de [Stock](ayuda:pantalla/Stock.Editar) (ingreso, egreso, cierre, ajuste, pesaje) y de un [movimiento entre sucursales](ayuda:pantalla/Movimientos.Editar).

## Reglas
- **Motivo obligatorio** (10 a 300 caracteres).
- **Usuario y contraseña en cada eliminación**, aunque ya estés logueado. No sirven el PIN ni la huella, y la validación no queda abierta para la próxima. Con tu usuario normal confirmás el **tuyo**; con la cuenta compartida de producción elegís tu nombre. Mismo límite que las demás autorizaciones: [5 intentos y bloqueo de 5 minutos](ayuda:concepto/intentos-de-clave-al-autorizar).
- **Permiso:** el mismo que para modificar ese tipo de registro, con su límite de fecha, y se mira el de **quien pone la clave**. **Ajuste de stock y Cierre de stock: solo administradores.**
- El registro **se borra** y, en la misma operación, se guarda la constancia completa (quién, cuándo, motivo y todas las líneas) en [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index).
- **Compras a cuenta corriente:** se genera el asiento contrario en la cuenta del proveedor (queda el original y el «ELIMINADO», el saldo vuelve al de antes). **Compras pagadas con la caja** (POS): egreso opuesto con la fecha de hoy.
- **No se puede eliminar** un registro del que depende otro: un pesaje con pesajes vinculados o con un ajuste ya generado, o una compra con pesajes vinculados. Primero se resuelve lo que depende de él.
- **Movimientos entre sucursales:** donde los movimientos se **replican entre dos bases** (hoy Servidor SM y San Lorenzo), **Eliminar está deshabilitado**, porque la copia en la otra base no se borra sola. Cuando se unifiquen las bases se habilita.
- **No se puede deshacer** desde el sistema.
- **Quién ve los eliminados:** los administradores y quien puede modificar ese módulo, desde el botón **Ver eliminados** del listado de Compras, Stock o Movimientos (ven todos los eliminados de su módulo). Ver [Registros eliminados](ayuda:pantalla/RegistrosEliminados.Index).
