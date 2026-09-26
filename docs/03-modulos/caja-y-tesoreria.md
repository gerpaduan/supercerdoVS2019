# Caja y tesoreria

## Objetivo

Documentar aperturas, cierres, movimientos de caja y controles asociados.

## Secciones

- Flujo de apertura y cierre
- Medios de pago
- Validaciones
- Reportes
- Riesgos operativos

## Pagos y cobros (Finanzas > Pagos) — 2026-09-24

- **Listado**: `GET /Finanzas/Pagos` (`FinanzasController.Pagos`, permiso `Permisos.Finanza.VerPagos`). Filtros: persona/N° recibo, rango de fechas (por defecto el último mes), Pago/Cobro y "Ver eliminados". Cada fila abre `AddOrEditPago?idPago=`.
- **Pantalla de un pago** (`Views/Finanzas/AddOrEditPago.cshtml`): fuera de POS un pago existente se abre en solo lectura; "Modificar" (Alt+Enter) habilita los campos y el botón pasa a "Guardar" (Alt+Enter); "Cancelar" (Alt+C) descarta. Con permiso de edición de pagos se puede **cambiar la persona** (selector sobre `Personas/Listar`) y **eliminar**.
- **Calculadora de billetes en el Importe** (2026-09-26): botón junto a `#txtImporte` (`js-calculadora-billetes-launch`, mismo mecanismo que Egresos de caja). Al aceptar carga el total en el importe y agrega el detalle del conteo en Observaciones (el área se despliega sola). Se oculta con Cheque / Efectivo + Cheques. **PENDIENTE**: prueba manual en navegador.
- **Cambio de persona**: el `MovCtaCte` se muta en el lugar (mismo `id`, nueva persona); queda registro en `auditoriapagos` (`CAMBIO_PERSONA`) y se actualiza el nombre en la descripción del egreso de caja del pago, si tiene.
- **Eliminar**: eliminación lógica (`pagos.eliminado`), asiento opuesto `ELIMINADO - ...` en la cta cte, egreso de caja opuesto (fechado hoy) si lo había, cheques liberados, auditoría `ELIMINACION` y notificación `PAGO_ELIMINADO` al admin. Un pago eliminado es solo lectura. Un cobro cuyos cheques ya se entregaron en otro pago no se puede eliminar.
- **Advertencia fecha ≠ carga**: banner en pantalla y notificación `PAGO_FECHA_DISTINTA` (campana del admin, solo Postgres).
- **Cuenta corriente de la persona** (`CtaCtePersona`): columna Obs. (ícono con "1" si el registro origen tiene observaciones), fila desplegable con todo el registro y export PDF/CSV con Nro.Doc.
- Detalle de decisiones, riesgos y pendientes: `docs/DECISIONS.md` (2026-09-24 "Pagos: listado, cambio de persona y eliminación").
