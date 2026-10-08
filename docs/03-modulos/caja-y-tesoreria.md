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
- **Calculadora de billetes en el Importe** (2026-09-26, reformada 2026-10-05): botón junto a `#txtImporte` (`js-calculadora-billetes-launch`, mismo mecanismo que Egresos de caja). Al aceptar carga el total en el importe y el conteo en el campo propio `#ConteoBilletes` (solo lectura, `pagos.conteobilletes`; ya **no** se agrega a Observaciones) y **no abre el modal de impresión** de la calculadora (`data-cb-conteo` + `data-cb-sin-post`). "Limpiar contador" vacía el campo. Se oculta con Cheque / Efectivo + Cheques. **PENDIENTE**: prueba manual en navegador.
- **Comprobante al guardar** (2026-10-05): `AddOrEditPagoPost` devuelve las URLs y la vista abre `PostComprobanteModal` (`Views/Shared/_ModalPostComprobante.cshtml` + `wwwroot/Scripts/app/post-comprobante.js`): 1 cerrar, 2 ticket 58/80 mm (`FinanzasController.ImprimirTicketPago` / `ImprimirTicketPagoPayload`, texto de `Services/ComprobanteTickets.ConstruirLineasPago`), 3 PDF (`ImprimirPdfPago` → `GenerarDocsCore.GenerarPdfPago`), 4 mail con PDF adjunto (`ObtenerDatosEmailPago` / `EnviarComprobantePagoEmail`). Título "Recibo de pago/cobro"; la persona figura como "Recibió" (pago) o "Entregó" (cobro). El conteo se imprime después de las observaciones. Lo que antes hacía el aviso de 2 s (cerrar el modal POS / navegar) corre al terminar el flujo del modal (`onClose`).
- **Egreso de caja del pago**: `Negocio.CuentaCorriente.CargarEgresoCajaPorPago` copia también `ConteoBilletes` al campo propio del egreso.

## Cambios en cajas ya cerradas y reapertura — 2026-10-06

- **Idea:** un registro (venta, compra, pago, egreso) pertenece a una caja por **dueño + sucursal + fecha ∈ [apertura, cierre]** (no hay FK). Si algo que cae en una caja **ya cerrada** se carga o modifica, el cierre guardado (`ventas`, `gastos`, `diferencia`) queda desactualizado. El servidor lo detecta (`Negocio.CajaCerradaServicio.Evaluar` → `CierreCaja.BuscarCajaCerradaQueContiene` → `findCierreCerradoQueContiene`), responde `requiereConfirmacion` (`CajaCerradaRespuesta.Confirmacion`) y el cliente (`caja-cerrada-confirm.js`) muestra el aviso y reenvía con `confirmarCajaCerrada=true`. Si se guarda, `Registrar` deja una fila en `auditoriacierrecaja` por cada caja afectada (también cuando no se pidió confirmar: p. ej. solo observaciones).
- **Dónde aplica:** ventas (`VentasController.ModificarVenta`, banner en el POS en modo edición), egresos de caja (`GuardarEgresoCajaCore`, fuera del POS), pagos/cobros con egreso de caja (`AddOrEditPagoPost` y `EliminarPago`), compras con egreso de caja (`ComprasController.Guardar`). Reglas de cuándo se pide confirmar: `docs/DECISIONS.md` (2026-10-06). Solo Postgres.
- **Dónde se ve:** historial de cierres (`_TablaCierresDeCaja.cshtml`): columna **Avisos** con "Modificada (N)" / "Reabierta (N)"; modal "Cerrar caja" en modo modificar: bloque `#avisoCambiosCaja` con el detalle (quién, cuándo, monto/forma de pago antes → después), que entrega `ObtenerDatosCierre` (`cambiosPosteriores`, solo a quien puede cerrar caja).
- **Reabrir la última caja** (historial, botón **Reabrir**, permiso `Caja.CierresDeCaja`): `GET PreviewReaperturaCaja` calcula si se puede (es la última del usuario/sucursal, sin otra abierta) y lo que queda adentro; `POST ReabrirCaja(idCierre, motivo)` ejecuta `CierreCajaPg.reabrirCierreCaja` en una transacción (UPDATE condicionado + auditoría `REAPERTURA` con snapshot). Deja `usuariocierre='0'` y `fechahoracierre=NULL`, borra los importes del cierre y el conteo del cajero. El vendedor sigue vendiendo al recargar el POS. `AbrirCaja` ahora bloquea una segunda caja abierta (antes el guard nunca se cumplía).
- **Límites:** solo Postgres; el registro se escribe después de guardar el origen (fuera de su transacción); sin tope de antigüedad para reabrir; ids migrados namespaced (se exige mayor `id` y mayor `fechahorainicio`).

## Egresos de caja (Cajas > Egresos) — 2026-10-05

- **Conteo de billetes**: campo `#egresoConteoBilletes` (`egresoscaja.conteobilletes`, solo lectura) en `Views/Cajas/_AddOrEditEgresoCaja.cshtml`; lo llena el contador (botón junto al Monto, `data-cb-conteo` + `data-cb-sin-post`), ya no se vuelca al Detalle. Se muestra en la fila desplegable de `_EgresosCajaTabla.cshtml`. El botón abría la calculadora dos veces (handler inline + delegado de `calculadora-billetes-targets.js`): se quitó el inline.
- **Comprobante al guardar**: `CajasController.GuardarEgresoCaja` devuelve las URLs y `egresos-caja.js` abre el mismo `PostComprobanteModal`. Acciones nuevas: `ImprimirTicketEgreso` / `ImprimirTicketEgresoPayload` (layout igual al de WinForms `formAddOrEditEgresoCaja.imprimirTicket` + conteo; `ComprobanteTickets.ConstruirLineasEgreso`), `ImprimirPdfEgreso` (`GenerarDocsCore.GenerarPdfEgreso`), `ObtenerDatosEmailEgreso` / `EnviarEgresoEmail` (el egreso no tiene persona: el destino se tipea).
- Permiso para ver el comprobante: quien cargó el egreso, o con `EgresoCaja.VerEgresosCaja` / `AddOrEditEgresoCaja`.
- SQL Server (WinForms / clásico): columna y parámetros de los SP con default NULL en `Datos/DB-Procedures/20261005-Alter_Pagos_EgresosCaja_add_ConteoBilletes.sql`. El script corre sobre la base activa (`sqlcmd -d SuperCerdo`, sin `USE`) y tiene guarda; verificado en una copia de la `SuperCerdo` local (2026-10-08). SP reales de SM y SL verificados el 2026-10-08 (idénticos a los esperados); script **aplicado el 2026-10-08 en SM y San Lorenzo** (ver `despliegue-y-publicacion.md`);, y el SP de lectura de egresos no devuelve `ConteoBilletes` (no versionado).
- **Cambio de persona**: el `MovCtaCte` se muta en el lugar (mismo `id`, nueva persona); queda registro en `auditoriapagos` (`CAMBIO_PERSONA`) y se actualiza el nombre en la descripción del egreso de caja del pago, si tiene.
- **Eliminar**: eliminación lógica (`pagos.eliminado`), asiento opuesto `ELIMINADO - ...` en la cta cte, egreso de caja opuesto (fechado hoy) si lo había, cheques liberados, auditoría `ELIMINACION` y notificación `PAGO_ELIMINADO` al admin. Un pago eliminado es solo lectura. Un cobro cuyos cheques ya se entregaron en otro pago no se puede eliminar.
- **Advertencia fecha ≠ carga**: banner en pantalla y notificación `PAGO_FECHA_DISTINTA` (campana del admin, solo Postgres).
- **Cuenta corriente de la persona** (`CtaCtePersona`): columna Obs. (ícono con "1" si el registro origen tiene observaciones), fila desplegable con todo el registro y export PDF/CSV con Nro.Doc.
- Detalle de decisiones, riesgos y pendientes: `docs/DECISIONS.md` (2026-09-24 "Pagos: listado, cambio de persona y eliminación").
