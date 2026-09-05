# Plan: AddOrEditPago (Finanzas) — alta/edición de pagos y cobros en cuenta corriente

CLAUDE.md §11.1 — plan escrito antes de tocar código. Scoping vía agente de exploración (solo lectura), resumido acá.

## Contexto

Ya portado y verificado con datos reales: `FinanzasController.CtaCtePersona` (extracto, PDF/Excel/email reales), `Cheques`/CRUD. `AddOrEditPago`/`AddOrEditPagoPost` quedaron pendientes por ser escritura de dinero real, acoplada a POS/Cajas.

## Decisiones confirmadas con el usuario antes de implementar

1. **Gap de permisos real encontrado**: `Permisos.Finanza.AddOrEditPago` ("formAddOrEditPago") existe en el catálogo pero el código clásico NUNCA lo valida en `AddOrEditPago`/`AddOrEditPagoPost` -- cualquier usuario que pueda ver una cuenta corriente puede dar de alta/editar pagos. Bajo WebCore (usuario único, todo bypaseado) esto no cambia nada hoy. **Decisión**: replicar tal cual (paridad), documentado en `gaps.md` junto con el resto de permisos reales pendientes (ver `gaps.md` "Permisos reales de venta y usuario producción").
2. **Aclaración del usuario sobre la regla real de negocio** (importante, no perder): la restricción real para editar/cargar un pago desde el POS no es un permiso formal -- es que **la fecha/hora del pago debe caer dentro de la ventana de la caja actualmente abierta del cajero** (confirmado también en el código clásico, `FinanzasController.cs:1003-1012`). Es decir, un cajero puede modificar pagos que él mismo cargó, mientras la fecha del pago sea posterior al inicio de su caja actual. Esta regla **solo aplica en modo POS** (no en el acceso directo desde Cuenta Corriente) -- queda documentada para cuando se aborde esa iteración.
3. **Alcance de esta iteración**: **sin modo POS** (sin `desdePos`, sin gate de caja abierta, sin espejo de `EgresoCaja`). Solo alta/edición directa desde `CtaCtePersona` → botón "Agregar Pago/Cobro".

## Tamaño real (confirmado, no estimado a ojo)

- Controller: ~600 líneas nuevas (`AddOrEditPago`/`AddOrEditPagoPost`/`ImprimirPdfPago`/`ObtenerDatosEmailPago`/`EnviarComprobantePagoEmail` + helpers `ConstruirReciboPagoVm`/`GenerarPdfPago`).
- Vista: `AddOrEditPago.cshtml` (1541L) + `_AddOrEditPagoScripts.cshtml` (698L) + `_ModalPostPago.cshtml` (143L) = **2382 líneas** -- el componente más grande del slice.
- `GenerarPdfPago` usa iTextSharp directo (177L) -- a diferencia de `CtaCtePersona`, **no hay port previo a QuestPDF**, hay que escribirlo de cero.

## Ya reutilizable sin tocar (confirmado en WebCore, no asumido)

- `Negocio.CuentaCorriente` completo (`getPagoById`/`addOrEditPago`/`ValidarPago`/`ValidarChequeParaPago`/`crearMovCtaCtePago`) -- mismo `Negocio.csproj` que usa Web, cero cambios.
- `BuscarChequePorNro`, `ValidarChequeParaPago` (endpoint), `ParseFloat`, `ConvertirTextoAHtmlPago`, `SmtpMailHelper`, `_ChequeBusquedaTabla.cshtml`/`_ModalAltaCheque.cshtml` -- todos ya portados en `WebCore/Controllers/FinanzasController.cs`.
- Infraestructura de agente de impresión local -- ya portada (Ventas/Expendios), reusable si se decide incluir ticket más adelante.

## Scope de esta iteración (MVP)

**Incluye:**
1. `AddOrEditPago` (GET) + `AddOrEditPagoPost` (POST) -- sin `desdePos`, sin gate de caja.
2. `GenerarPdfPago` nuevo en `GenerarDocsCore.cs` (QuestPDF, reescrito desde cero -- no hay port previo).
3. `ImprimirPdfPago` + `ObtenerDatosEmailPago`/`EnviarComprobantePagoEmail` (calco directo del patrón ya portado en `CtaCtePersona`).
4. Vista `AddOrEditPago.cshtml` + JS propio, recortada (sin ramas de POS/atajos 1-2 si complican).
5. `_ModalPostPago.cshtml` básico (PDF + email, mismo patrón `_ModalPostVentaBasico.cshtml`/`_ModalPostPuntoExpendioBasico.cshtml` ya usado 2 veces).
6. Botón "Agregar Pago/Cobro" en `CtaCtePersona.cshtml` de WebCore (hoy excluido a propósito).

**Explícitamente afuera** (segunda iteración, no ahora):
- Modo POS completo (`desdePos`, gate de caja abierta, `EgresoCaja` espejo) -- documentado como pendiente en `gaps.md`, junto con la regla real de la ventana temporal (decisión 2 arriba).
- Ticket térmico ESC/POS (`ImprimirTicketPago`/`_TicketPago.cshtml`) y `DescargarAgenteImpresion` -- mismo criterio ya usado en todo el programa (agente de impresión local no es el foco).
- `eliminarPago` -- código muerto en el original (sin caller), no se porta.

## Verificación

Mismo criterio que todo el programa: build limpio, curl + sqlcmd contra datos reales (alta de un pago real de prueba, verificado en `MovCtaCte`, y su reversión/limpieza si corresponde), servidor propio en `localhost:5270`.

## Progreso

**MVP portado y verificado con datos reales (2026-09-04)**:

- Recorte adicional decidido durante la implementación (no estaba en el scoping original): **sin "Cheque"/"EftvoCheque" como forma de pago** en esta iteración -- el merge de `ChequesJson` + 2 modales de búsqueda/alta es una pieza grande aparte; los cheques ya tienen su propio CRUD completo en WebCore, solo no se puede pagar CON un cheque desde este formulario todavía.
- `WebCore/Models/ReciboPagoVm.cs` (nuevo), `WebCore/Services/GenerarDocsCore.cs` ganó `GenerarPdfPago` (QuestPDF desde cero, sin port previo -- el original usaba iTextSharp directo). Simplificación deliberada: sin la tabla de detalle línea-por-línea de cheques (no se ejercita, cheques excluidos de esta iteración).
- `WebCore/Controllers/FinanzasController.cs`: agregado `_oSucursalN` (faltaba), `AddOrEditPago` (GET), `AddOrEditPagoPost` (POST), `ImprimirPdfPago`, `ObtenerDatosEmailPago`, `EnviarComprobantePagoEmail`, `ConstruirReciboPagoVm`, `verMovimientoCtaCte` (rutas Ventas→`DetalleVenta` y Pagos→`AddOrEditPago`; Compras no portado, cae a `NotFound`).
- `WebCore/Views/Finanzas/AddOrEditPago.cshtml` (nuevo, form simplificado -- ver header del archivo para el detalle completo de recortes). `WebCore/Views/Finanzas/CtaCtePersona.cshtml`: botón "Agregar Pago / Cobro" + filas de tipo `Pagos` (no históricas) ahora clickeables para editar.
- **Bug real encontrado y corregido durante la verificación** (no solo build limpio): el input `NroRecibo` no tenía su valor incluido en el payload de guardado -- el primer pago de prueba (#62) se guardó con `nroRecibo` vacío en la base. Corregido agregándolo al payload JS; reverificado con un segundo pago (#63) con el número correcto (`002-00000063`).
- **Hallazgo aparte, no relacionado al código**: `sqlcmd` con el login `sa` (usado sin problema toda la sesión para `Ventas`/`Sectores`/etc.) devolvió **0 filas silenciosamente** para la tabla `Pagos` por RLS propia de esta base -- confirmado y corregido usando el login de bypass documentado en memoria (`cs_admin`, `~/hosts/carnisys-web-local.env`). No es un problema de WebCore; documentado para no repetir la falsa alarma.
- **Verificado con curl + sqlcmd (login de bypass) contra datos reales, ciclo completo**: alta real de un pago (persona #23, $750.50, Efectivo) → pago **#63** persistido con el número de recibo correcto; edición real del mismo pago (Debito, $900.00, observaciones actualizadas) → verificado en la base tras el update; `ImprimirPdfPago` generó un PDF válido real (1 página) en ambos casos; `ObtenerDatosEmailPago` devolvió asunto/cuerpo/empresa reales (`SuperCerdo`) con `replyTo` real; `verMovimientoCtaCte?tabla=Pagos` redirige correctamente al formulario de edición; el botón "Agregar Pago/Cobro" y las filas clickeables aparecen correctamente en `CtaCtePersona` real. Build limpio (0 errores).
- **Pendiente**: prueba visual del usuario en navegador (no se puede automatizar sin herramienta de browser en esta sesión). Envío real de email no se volvió a probar en esta iteración (el pipeline SMTP ya está confirmado funcionando desde antes en esta misma sesión, ver `PLAN-POS-UI.md`).
