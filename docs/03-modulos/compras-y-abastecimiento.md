# Compras y abastecimiento

## Objetivo

Registrar el proceso de compras, recepcion, ordenes y abastecimiento interno.

## Secciones

- Pantallas involucradas
- Flujo principal
- Validaciones
- Impacto en stock
- Observaciones

## Modificar una compra cuyo egreso está en una caja ya cerrada — 2026-10-06

Solo las compras hechas desde el POS generan egreso de caja (tipo 19, `egresoscaja.idcompra`) y por lo tanto mueven una caja. `ComprasController.Guardar` busca ese egreso (`findEgresoCajaPorCompra`); si está en una caja **ya cerrada** responde `requiereConfirmacion` (libera antes el lock anti doble-submit) y `compras.js` reenvía el formulario con `ConfirmarCajaCerrada=true`. Desde el POS el egreso se actualiza con los datos nuevos; desde otra pantalla **no se toca** y compra y egreso pueden quedar distintos (el aviso lo dice). Queda un aviso en el cierre (`auditoriacierrecaja`, origen `COMPRA`). Solo Postgres. Ver `docs/03-modulos/caja-y-tesoreria.md`.

## Recuperación de borrador sin cerrar

Ver `docs/DECISIONS.md` "Borradores en servidor de Compras/Stock/Movimientos/Embutidos" (2026-09-22).

Alta/edición de una compra (`Compras/Editar`, `ComprasController.Guardar`) autoguarda el formulario en el servidor (tabla `borradorgenerico`, módulo `COMPRA`) mientras se arma, con `BorradorGenerico:Habilitado` (Postgres: prendido por defecto; SQL Server: opt-in desde 2026-09-23, ver `docs/DECISIONS.md` "Borradores en SQL Server"). Cualquier operador de la sucursal puede ver los borradores sin cerrar ("Compras sin cerrar", junto al botón Volver) y recuperarlos o descartarlos; uno ajeno exige autorización de supervisor. Al guardar la compra con éxito, el borrador se marca `FINALIZADA` (JS: `compras.js`, `marcarFinalizado`).

Un borrador **propio** que figura "En uso" (corte de luz/cuelgue, todavía dentro de `BorradorGenerico:MinutosSinLatidoInterrumpida`) se puede cargar igual confirmando "Cargar igual" (desde 2026-10-04; rota el `clientId`, ver `docs/DECISIONS.md` 2026-10-04). Descartar sigue esperando el umbral.

Con el feature apagado por config (default en SQL Server hasta activarlo), sigue el resguardo local de siempre: borrador en `localStorage` (banner "Recuperar/Limpiar borrador") y respaldo por captura de pantalla (`captura-respaldo.js`, botón "Activar respaldo automático", solo Chrome/Edge). Ambos mecanismos son mutuamente excluyentes con el borrador en servidor, nunca conviven.
