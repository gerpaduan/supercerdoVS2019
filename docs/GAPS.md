# Gaps de la migración SQL Server → PostgreSQL

Inventario de comportamiento real de SQL Server que **todavía no está replicado** en el lado
Postgres, por depender de una tabla o clase que no se migró aún. Cada entrada espera una
decisión/implementación futura y se borra de acá cuando se resuelve (pasa a
`docs/DECISIONS.md` como decisión tomada, o se implementa directamente).

No confundir con deuda documentada como decisión deliberada (ej. `agregarStockVenta` como
no-op porque `StockCorteSucursal` está confirmada obsoleta) — eso vive en `docs/DECISIONS.md`,
no acá. Este archivo es solo para comportamiento real pendiente de portar.

---

Sin gaps abiertos por ahora. El último (reverso de `EgresosCaja` en `VentaPg.modificarVenta`,
abierto en la Etapa 7) se resolvió en la Etapa 8 al migrar `EgresosCaja`/`TiposEgresoCaja` —
ver `docs/DECISIONS.md`.
