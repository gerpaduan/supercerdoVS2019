# Elaborados / Embutidos

## Objetivo

Relevar el modulo de produccion de elaborados (entidad `Embutido` en el codigo; el modulo web se llama "Elaborados"): carga de un lote consumiendo ingredientes de stock segun una formula/receta.

## Secciones

- Pantallas involucradas: PENDIENTE (documentar entidades `Embutido`/`CortePorEmbutido`/`Formula`, flujo de `Carga` vs. `Ingreso Rápido`/`Desarme`, `ElaboradosController`)
- Flujo principal
- Validaciones
- Observaciones

## Recuperación de borrador sin cerrar

Ver `docs/DECISIONS.md` "Borradores en servidor de Compras/Stock/Movimientos/Embutidos" (2026-09-22). Cubre **dos** pantallas, con discriminadores distintos de la misma tabla `borradorgenerico`:

- **Carga** (`Elaborados/Carga`, alta/edición completa con ingredientes manuales + fórmula, `ElaboradosController.GuardarCarga`): módulo `EMBUTIDO_CARGA`. Ya tenía un borrador local (`localStorage`) antes de esto; con `BorradorGenerico:Habilitado` (Postgres) ese local y el respaldo por captura de pantalla se desactivan, reemplazados por el servidor. Particularidad: editar un elaborado existente **nunca es in-place** — el guardado real anula el original y crea uno nuevo (`anularEmbutido` + `agregarEmbutido`), así que el borrador siempre se representa como alta nueva; el `IdEmbutido` original que se está reemplazando viaja como `idregistro` solo para que el listado de "Embutidos sin cerrar" muestre "editando #N".
- **Ingreso Rápido / Desarme** (`Elaborados/EditarIngresoRapido`, formula fija, solo se pide cantidad total, `ElaboradosController.GuardarIngresoRapido`): módulo `EMBUTIDO_RAPIDO`. **No tenía ningún resguardo antes** (ni local ni servidor) — se cubre solo con el borrador en servidor; no se le agregó `localStorage`/captura de pantalla nuevos (decisión explícita: el pedido era reemplazar lo local existente en las otras pantallas, no expandirlo donde nunca hubo). Con SQL Server o el feature apagado, esta pantalla sigue sin ningún resguardo, igual que siempre.

En ambas, cualquier operador de la sucursal ve los borradores sin cerrar del módulo ("Embutidos sin cerrar", junto a "Volver") y puede recuperar/descartar los propios; uno ajeno exige autorización de supervisor. Motores: Postgres (prendido por defecto) y SQL Server (opt-in desde 2026-09-23, ver `docs/DECISIONS.md` "Borradores en SQL Server"; sin notificaciones al admin todavía).

Un borrador **propio** que figura "En uso" (corte de luz/cuelgue, todavía dentro de `BorradorGenerico:MinutosSinLatidoInterrumpida`) se puede cargar igual confirmando "Cargar igual" (desde 2026-10-04; rota el `clientId`, ver `docs/DECISIONS.md` 2026-10-04). Descartar sigue esperando el umbral.

## Fórmula secreta (2026-10-04)

Una fórmula se puede marcar como **secreta** (casilla en `Elaborados/EditarFormula`, columna `formulas.secreta`; requiere `Permisos.Elaborado.IngresoFormula`). Mientras la sesión no tenga una *elevación* vigente para ese producto elaborado, ninguna pantalla operativa muestra sus ingredientes, porcentajes ni receta. Detalle de diseño y alternativas: `docs/DECISIONS.md` "Fórmula secreta con re-login". Guía de uso: `docs/11-manual-de-uso/conceptos/formula-secreta.md`.

- **Pantallas con ocultamiento**: `EditarIngresoRapido` (también Desarme), `Carga` (alta y edición), `Index` → detalle (`ElaboradosController.Detalle`), `Lineas`. `Formulas`/`EditarFormula` no piden re-login: ya exigen `VerFormulas`/`IngresoFormula` (el listado marca las secretas con candado).
- **Qué hace el servidor** (`ElaboradosController`): `FormulaOculta(idCorte)` = fórmula secreta **y** sin elevación. Si está oculta: `ObtenerFormula` devuelve `secreta=true` sin líneas; `EditarIngresoRapido`/`Carga` dejan `Formula` y `Receta` vacías (`AplicarSecretoFormulaRapida`/`AplicarSecretoFormulaCarga`, **después** de calcular la cantidad/los automáticos); `ConstruirDetalleElaborado` no devuelve receta ni ingredientes; `Lineas` colapsa las líneas de cada registro en **una** fila con el total (`ColapsarLineasConFormulaOculta`). `GuardarIngresoRapido`/`GuardarCarga` **no cambian**: ya releían la fórmula de la base, así que el stock se descuenta igual aunque el operador no la haya visto.
- **Re-login ("Ver fórmula")**: `POST Elaborados/AutorizarVerFormula(usuario, clave, idCorte | idEmbutido)` → `Negocio.Usuario.AutorizarVerFormulaSecreta` (activo, sin bloqueo, y `VerFormulas` **o** `IngresoFormula`; Admin siempre) → `FormulaSecretaHelper.Registrar` guarda `idCorte,idUsuario,vencimiento` en `ISession` (lógica pura y probada en `Negocio.FormulaSecretaElevaciones`). Vence a los `Security:FormulaElevacionMinutos` (5). `POST Elaborados/OcultarFormula(idCorte)` ("Ocultar", y `sendBeacon` en `pagehide` de Ingreso Rápido/Carga; sin antiforgery a propósito, solo oculta).
- **Rate limit**: `FormulaStepUpRateLimiter` (por sesión, 3 intentos / 5 min / bloqueo 5 min). No suma intentos fallidos a la cuenta ni la bloquea (un operador no debe poder bloquear a quien tiene el permiso). Mensaje de error único para no revelar qué usuarios existen ni cuáles tienen permiso.
- **Auditoría** (`auditoriaformulas`, append-only; CLAUDE.md §9.3): `VER` (quién se autenticó = `idusuario`, quién operaba = `idusuariosesion`, producto, IP), `OCULTAR`, `FALLO` (con motivo interno), `MARCAR_SECRETA`/`QUITAR_SECRETA`. **Falla cerrado**: si no se puede escribir el `VER`, no se muestra la fórmula. Todavía **no hay pantalla** para consultar esta tabla (PENDIENTE).
- **Borradores** (`BorradoresGenericoController.QuitarFormulaSecretaDelPayload`): para `EMBUTIDO_RAPIDO`/`EMBUTIDO_CARGA` con fórmula secreta se vacían `formula` y `elaborado.receta` del payload al guardar y al devolver el borrador (cubre borradores viejos). Requiere que el payload traiga `idElaborado` (Rápido) o `elaborado.id` (Carga).
- **Cierre de caja**: no interviene (no usa fórmulas, elaborados ni costos).
