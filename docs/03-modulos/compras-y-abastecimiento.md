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

## Eliminar una compra (2026-10-22)
- `Negocio.Compra.eliminarCompra` / `obtenerVistaPreviaEliminacion`; endpoints `Compras/PrevisualizarEliminar` (GET) y `Compras/Eliminar` (POST). Botón **Eliminar** en `Compras/Editar` (pantalla completa; no dentro del modal del POS ni de la cuenta corriente).
- Borra la compra y sus líneas (`corteporcompra` / `mediares`), guarda la foto completa en `auditoriaeliminaciones` + `auditoriaeliminacionlineas`, genera el **asiento contrario** `ELIMINADO - ...` en la cuenta corriente del proveedor si tenía uno vigente, un **egreso de caja opuesto** (fecha de hoy) si el egreso de la compra tenía monto, y recalcula `corteproveedor`. Todo en una transacción. Aviso al admin (campana).
- Pide motivo (10 a 300 caracteres) y **usuario y contraseña en cada eliminación**. Permiso = `Compra.ModificarCompra` de quien puso la clave. Se bloquea si hay pesajes vinculados a la compra.
- Detalle y alternativas: `docs/DECISIONS.md` (2026-10-22, «Eliminar compras, stock y movimientos»). Manual: `docs/11-manual-de-uso/pantallas/Compras.Editar.md`, `RegistrosEliminados.Index.md` y el concepto `eliminar-registros`.

## Titular propio y comprobantes del proveedor (2026-11-09)

Solo Postgres. Decisiones y alternativas descartadas: `docs/DECISIONS.md` (2026-11-09, «Compras: titular propio y comprobantes del proveedor»).

- **Datos:** `compras.idtitular` (NULL = principal), `compras.estadofactura` (`PENDIENTE` / `FACTURADA` / `NO_LLEVA`; NULL = anterior a la migración) y la tabla `comprobantescompra` (factura, notas de crédito y de débito con baja lógica y motivo). Una factura activa por compra; las notas exigen la factura. Índice único `(idempresa, idproveedor, tipocomprobante, ptovta, numero)` entre los no eliminados.
- **La deuda es el importe de la compra:** los comprobantes nunca tocan `movctacte`.
- **Código:** `Negocio/ComprobanteCompra.cs` (servicio), `Negocio/ValidacionComprobanteCompra.cs` (reglas puras), `Negocio/ResumenCtaCtePorTitular.cs` (resumen por titular), `Negocio/ClasificacionMedioPago.cs` (bancarizado vs efectivo), `DatosPostgres/ComprobanteCompraPg.cs`, `WebCore/Controllers/ComprasController.Comprobantes.cs` (acciones `Comprobantes`, `GuardarComprobante`, `EliminarComprobante`, `MarcarSinFactura`, `CambiarTitularCompra`), vistas `Compras/_ComprobantesCompra*.cshtml` y `wwwroot/Scripts/app/comprobantes-compra.js`.
- **Pantallas:** Compras/Index (filtros Factura y Titular, columnas, botón *Comprobantes* = bandeja de facturas pendientes), Compras/Editar (titular y «La factura vino con la mercadería» en el alta; bloque de solo lectura y botón *Comprobantes* al editar), Compras/Lineas (Titular).
- **Al eliminar una compra** sus comprobantes se dan de baja (`CompraPg.EjecutarEliminarCompra`, paso 5).
- **Configuración:** `Compras:AlicuotaIvaDefault`, `Compras:AlicuotasIva`.
