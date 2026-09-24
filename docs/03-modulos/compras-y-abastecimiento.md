# Compras y abastecimiento

## Objetivo

Registrar el proceso de compras, recepcion, ordenes y abastecimiento interno.

## Secciones

- Pantallas involucradas
- Flujo principal
- Validaciones
- Impacto en stock
- Observaciones

## Recuperación de borrador sin cerrar

Ver `docs/DECISIONS.md` "Borradores en servidor de Compras/Stock/Movimientos/Embutidos" (2026-09-22).

Alta/edición de una compra (`Compras/Editar`, `ComprasController.Guardar`) autoguarda el formulario en el servidor (tabla `borradorgenerico`, módulo `COMPRA`) mientras se arma, con `BorradorGenerico:Habilitado` (Postgres: prendido por defecto; SQL Server: opt-in desde 2026-09-23, ver `docs/DECISIONS.md` "Borradores en SQL Server"). Cualquier operador de la sucursal puede ver los borradores sin cerrar ("Compras sin cerrar", junto al botón Volver) y recuperarlos o descartarlos; uno ajeno exige autorización de supervisor. Al guardar la compra con éxito, el borrador se marca `FINALIZADA` (JS: `compras.js`, `marcarFinalizado`).

Con el feature apagado por config (default en SQL Server hasta activarlo), sigue el resguardo local de siempre: borrador en `localStorage` (banner "Recuperar/Limpiar borrador") y respaldo por captura de pantalla (`captura-respaldo.js`, botón "Activar respaldo automático", solo Chrome/Edge). Ambos mecanismos son mutuamente excluyentes con el borrador en servidor, nunca conviven.
