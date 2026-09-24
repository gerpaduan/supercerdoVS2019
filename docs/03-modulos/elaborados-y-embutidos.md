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
