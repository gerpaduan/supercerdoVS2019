# Clientes y proveedores

## Objetivo

Registrar mantenimiento, consultas, condiciones comerciales y relaciones operativas.

## Secciones

- Altas y ediciones
- Datos obligatorios
- Busquedas y filtros
- Impacto en otros modulos
- Cuenta corriente reservada (ver abajo)

## Cuenta corriente reservada

Una persona (cliente, proveedor o empleado) puede marcarse como "Cuenta corriente reservada" (`Persona.CtaCteReservada`, `personas.ctactereservada` / `Personas.ctaCteReservada`; switch en `Personas/Editar` y en el alta rapida, visible solo para admin o `formCtasCtes`). Decision y alcance completos en `docs/DECISIONS.md` (2026-10-01).

- **Autorizado** (admin o `formCtasCtes`): ve todo.
- **Restringido** (cualquier otro): puede venderle, cobrarle y pagarle desde el POS y la persona aparece en los buscadores, pero solo ve los registros que creo el mismo (Ventas `idvendedor`; Compras/Pagos/MovCtaCte `creadopor`) con `creado >=` hora de apertura de su caja abierta. Sin caja abierta no ve nada reservado. El extracto no muestra saldo (`***`).
- Avisos: el restringido ve una alerta en el extracto y en cobro/pago, la etiqueta "Reservada" en el POS y una nota en los listados; el autorizado ve la etiqueta "Reservada" junto al nombre (detalle en `docs/DECISIONS.md`, 2026-10-01).
- Los empleados nacen reservados (`EmpleadoPg.Agregar`); el admin lo puede destildar.
- Implementacion: `WebCore/Services/CtaCteReservadaService.cs` (quien esta restringido, operador y apertura de caja), `IPersonaRepository.idsPersonasReservadas`/`idsRegistrosOcultos` (una consulta por tabla, ambos motores), `[ProtegerRegistroReservado]` para acceso directo por id; Facturas filtra en SQL.
- `Persona.CtaCte` (flag viejo) queda en la base sin efecto funcional y sin switch.
