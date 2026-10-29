---
pantalla: Finanzas.CtaCtePersona
titulo: Cuenta corriente de una persona
rol: usuario
modulo: Finanzas
orden: 2
permiso: PENDIENTE (no verificado contra el código en esta revisión)
revisada: 2026-10-28
---

## Para qué sirve
Ver los **movimientos** de la cuenta corriente de una persona (ventas, pagos y cobros, compras) con el **saldo** después de cada uno, y cargar un pago o cobro. Se llega desde [Cuentas corrientes](Finanzas.CtasCtes.md).

## Filtros y acciones
- **Desde** + **Filtrar**: muestra los movimientos a partir de esa fecha. **Mostrar anulados** suma los registros anulados.
- **Agregar Pago / Cobro** abre la carga de un pago (también con **Alt+Enter**).
- **PDF**, **Excel** y **Enviar por mail** solo aparecen si tu usuario puede exportar la cuenta corriente.
- Tocar un movimiento abre la **venta**, el **pago** o la **compra** de origen. **Detalle** (la flecha) despliega observaciones, quién lo creó y quién lo modificó.
- Etiquetas: **Modificado**, **Versión anterior**, **ELIMINADO** y **Anulado por eliminación** marcan las distintas versiones de un registro.

## En la computadora
- Arriba de la tabla hay un **resumen del período**: saldo anterior a la fecha del filtro, **Débitos** y **Créditos** (con la cantidad de movimientos) y **Saldo actual**. Si tu usuario no puede ver saldos (o la cuenta es reservada), las tarjetas de saldo no se muestran y el saldo de cada fila es `***`.
- Columnas: **Fecha**, **Operación** (etiqueta: Pagos, Compras, Ventas…), **Comprobante**, **Detalle**, **Débito** (en rojo, lo que la persona paga o se le descuenta), **Crédito** (en verde, lo que se le suma) y **Saldo** (en rojo si es negativo). El importe se muestra sin signo en la columna que corresponde.
- Debajo del detalle, en gris, van el número de registro y la **sucursal** (`#13154 · San Martín`). Si el registro tiene observaciones se ve un ícono de comentario junto al detalle (al pasar el mouse se leen). La primera fila, **Saldo Anterior**, va sombreada y no suma a los totales.
- La fila final **Totales del período** suma los Débitos y los Créditos de las filas mostradas (con **Mostrar anulados** incluye las versiones anteriores y los contraasientos, que es lo que hace cuadrar el saldo): saldo anterior − débitos + créditos = saldo actual.
- La flecha de cada fila despliega el detalle completo (observaciones, quién lo creó y modificó). El estado (**Versión anterior**, **ELIMINADO**…) aparece debajo de la etiqueta de la operación.
- Se ve igual abierta desde el POS (F2).

## En el celular
- Arriba, el nombre de la persona y, debajo, el **saldo** en grande (verde o rojo).
- **Agregar pago / cobro** ocupa todo el ancho; PDF, Excel y **Mail** van en una fila.
- Cada movimiento es un renglón de tres líneas: operación y tipo (Crédito / Débito) con el **importe** a la derecha; fecha y número de documento con el **saldo** a la derecha; y sucursal, detalle, el ícono de observaciones y **Detalle**.
- El resumen y los totales de la computadora no se muestran en el celular.

## Desde el POS
La misma lista se ve en el celular al abrir las cuentas corrientes desde el POS (F2), en modo claro y oscuro. En la computadora se ve la misma tabla con Débito / Crédito que en la página.

## Resto de la pantalla
PENDIENTE: documentar permisos, el recorte de fecha por permiso, el uso desde el POS y el envío por mail (no verificado contra el código en esta revisión).
