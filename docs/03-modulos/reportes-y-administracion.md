# Reportes y administracion

## Objetivo

Centralizar reportes, configuraciones y procesos administrativos.

## Secciones

- Reportes disponibles
  - `/Reportes` (`WebCore/Controllers/ReportesController.cs`): Stock Actual, Cierre Stock, Stock Retroactivo, Proyección Ventas vs Stock, Ventas por Producto y Balance Económico.
  - **Sucursales (2026-10-13)**: se pueden tildar 1, varias o todas (`idsSucursales`; el `idSucursal` viejo sigue valiendo). Ventas por Producto, Proyección y Balance suman las tildadas (`IdsParaConsulta`: `[id]`, `[0]` = todas en una sola consulta, o la lista). Los 3 de stock, con 2+ sucursales, calculan cada una con sus propios cierres (`UsaCierresPorSucursal`). Detalle y decisiones: `docs/DECISIONS.md` (2026-10-13, Reportes) y `docs/11-manual-de-uso/pantallas/Reportes.Index.md`.
- Parametros y filtros
- Configuraciones sensibles
- Permisos
