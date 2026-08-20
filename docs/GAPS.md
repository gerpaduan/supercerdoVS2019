# Gaps de la migración SQL Server → PostgreSQL

Inventario de comportamiento real de SQL Server que **todavía no está replicado** en el lado
Postgres, por depender de una tabla o clase que no se migró aún. Cada entrada espera una
decisión/implementación futura y se borra de acá cuando se resuelve (pasa a
`docs/DECISIONS.md` como decisión tomada, o se implementa directamente).

No confundir con deuda documentada como decisión deliberada (ej. `agregarStockVenta` como
no-op porque `StockCorteSucursal` está confirmada obsoleta) — eso vive en `docs/DECISIONS.md`,
no acá. Este archivo es solo para comportamiento real pendiente de portar.

---

## Abiertos (actualizado 2026-08-20, tras cablear los 10 módulos del modo dual)

- **`PersonaPg.buscarProveedor`** — el original de SQL Server es un stored procedure
  (`EXEC buscarProveedor`) que **no existe en la base local de dev** (confirmado con
  `sp_helptext` contra `sys.procedures`). Sin definición real para traducir. Tiene un caller
  real (`StockController.ObtenerProveedoresExistencia`) que ya envuelve la llamada en
  `try/catch` y degrada a lista vacía — mismo comportamiento en ambos motores hoy. Pendiente:
  confirmar si el SP existe en `ServidorSM`/`San Lorenzo` (producción real) antes de decidir
  si se traduce o se documenta como código muerto.

- **`PersonaPg.obtenerProveedores`** — sin caller en ningún controller cableado a
  `NegocioFactory`. Mismo patrón `ILIKE` que el resto de la clase si se implementa.

- **`SucursalPg.obtenerSucursalSanMartin` / `obtenerSucursalSanLorenzo`** — atados a la
  topología legacy de sincronización entre 3 servidores (tabla `Conexiones`, ver
  `docs/06-datos-e-integraciones/`). Sin caller en ningún controller cableado. Fuera de
  alcance mientras esa topología siga existiendo tal cual.

- **`CuentaCorrientePg.obtenerPagos`** — implementado, pero sus alias de columna **nunca se
  verificaron contra un caller real** (a diferencia de los otros 5 métodos de la clase, ya
  cerrados en la etapa de `CuentaCorriente`). Sin caller en ningún controller cableado hoy.
  Revisar alias antes de que algún controller lo use de verdad.

## Fuera de alcance, no son gaps de esta migración (documentado para no reabrir por error)

- **`CuentaCorrientePg.eliminarPago`** — el SP `eliminarPago` de SQL Server **no existe**
  (confirmado contra `sys.procedures`, bug real preexistente de la base original). Solo
  alcanzable desde `Presentacion/` (WinForms), que nunca toca Postgres por diseño — sin riesgo
  real para el modo dual.

---

Última auditoría completa: 2026-08-20 (revisión de production-readiness tras cablear
`LoginController`, el último de los 10 módulos del plan de modo dual). Antes de esa fecha este
archivo decía "sin gaps abiertos", lo cual ya no reflejaba el código — corregido acá.
