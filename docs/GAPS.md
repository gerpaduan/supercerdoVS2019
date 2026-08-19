# Gaps de la migración SQL Server → PostgreSQL

Inventario de comportamiento real de SQL Server que **todavía no está replicado** en el lado
Postgres, por depender de una tabla o clase que no se migró aún. Cada entrada espera una
decisión/implementación futura y se borra de acá cuando se resuelve (pasa a
`docs/DECISIONS.md` como decisión tomada, o se implementa directamente).

No confundir con deuda documentada como decisión deliberada (ej. `agregarStockVenta` como
no-op porque `StockCorteSucursal` está confirmada obsoleta) — eso vive en `docs/DECISIONS.md`,
no acá. Este archivo es solo para comportamiento real pendiente de portar.

---

## `VentaPg.modificarVenta` — reverso de `EgresosCaja` no implementado

**Origen**: `docs/DECISIONS.md`, Etapa 7 (2026-08-19).

**Comportamiento real en SQL Server** (SP `dbo.modificarVenta`, verificado con `sp_helptext`
contra la base viva): cuando se edita una venta con `eliminarLineas=true` y existe un
`EgresosCaja` previo ligado a esa venta (`tabla='Ventas' AND idTabla=@idVenta`, típico de una
venta en cuenta corriente que generó un egreso), el SP inserta un registro inverso en
`EgresosCaja` (mismo monto en negativo, descripción prefijada `"Anulado:"`) para revertir el
efecto contable del egreso original.

**Por qué no está en Postgres**: `EgresosCaja`/`TiposEgresoCaja` son dominio de `CierreCaja.cs`,
todavía sin migrar. `VentaPg.modificarVenta` implementa el resto del método (borrado de líneas +
`UPDATE Ventas`) pero omite este paso — marcado con `TODO(claude)` en el código.

**Impacto real si no se resuelve**: al comparar SQL Server vs Postgres para una venta cta-cte
editada con líneas eliminadas y egreso previo, `EgresosCaja` queda desincronizada entre motores
(SQL Server tiene el reverso, Postgres no). No afecta ningún otro flujo.

**Confirmado con el usuario (2026-08-19)**: este gap es importante y **debe resolverse
obligatoriamente** cuando se aborde `CierreCaja.cs` — no es opcional, no se puede dejar así
indefinidamente.

**Para resolverlo**: migrar `EgresosCaja`/`TiposEgresoCaja` (schema + RLS + datos), agregar el
paso equivalente en `VentaPg.modificarVenta` dentro de la misma transacción que ya usa
(`ConexionPg.AbrirConTenant`), y extender el harness de comparación (`CompararVenta` o uno
dedicado) para ejercitar el caso: venta cta-cte con egreso previo → editar con `eliminarLineas`
→ confirmar el reverso en ambos motores.
