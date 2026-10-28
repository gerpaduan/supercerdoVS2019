---
pantalla: Finanzas.CtaCtePersona
titulo: Cuenta corriente de una persona
rol: usuario
modulo: Finanzas
orden: 2
permiso: PENDIENTE (no verificado contra el código en esta revisión)
revisada: 2026-10-27
---
## Para qué sirve
Ver los **movimientos** de la cuenta corriente de una persona (ventas, pagos y cobros, compras) con el **saldo** después de cada uno, y cargar un pago o cobro. Se llega desde [Cuentas corrientes](Finanzas.CtasCtes.md).

## Filtros y acciones
- **Desde** + **Filtrar**: muestra los movimientos a partir de esa fecha. **Mostrar anulados** suma los registros anulados.
- **Agregar Pago / Cobro** abre la carga de un pago (también con **Alt+Enter**).
- **PDF**, **Excel** y **Enviar por mail** solo aparecen si tu usuario puede exportar la cuenta corriente.
- Tocar un movimiento abre la **venta**, el **pago** o la **compra** de origen. **Detalle** (la flecha) despliega observaciones, quién lo creó y quién lo modificó.
- Etiquetas: **Modificado**, **Versión anterior**, **ELIMINADO** y **Anulado por eliminación** marcan las distintas versiones de un registro.

## En el celular
- Arriba, el nombre de la persona y, debajo, el **saldo** en grande (verde o rojo).
- **Agregar pago / cobro** ocupa todo el ancho; PDF, Excel y **Mail** van en una fila.
- Cada movimiento es un renglón de tres líneas: operación y tipo (Crédito / Débito) con el **importe** a la derecha; fecha y número de documento con el **saldo** a la derecha; y sucursal, detalle, el ícono de observaciones y **Detalle**.
- En la computadora sigue la tabla de siempre.

## Resto de la pantalla
PENDIENTE: documentar permisos, el recorte de fecha por permiso, el uso desde el POS y el envío por mail (no verificado contra el código en esta revisión).
