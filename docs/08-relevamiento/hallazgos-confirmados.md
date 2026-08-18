# Hallazgos confirmados

## Objetivo

Registrar comportamiento observado y validado en código o en uso real.

## Inventario completo de `carnisys` (relevado 2026-08-18, ver `docs/DECISIONS.md` entrada del mismo día)

Snapshot crudo versionado en `docs/08-relevamiento/snapshot-2026-08-18/`:
- `schema-columnas.txt`: las 55 tablas, columna por columna (tipo, nullable, identity, default, si es PK). 596 filas.
- `schema-fks.txt`: 12 foreign keys reales declaradas a nivel de motor (pocas para 55 tablas — la mayoría de las relaciones se resuelven a mano en el código, no están declaradas como FK).
- `schema-indices.txt`: 120 filas de índices (incluye PKs).
- `stored-procedures.sql`: texto completo de los 117 stored procedures de usuario (`sys.procedures`, vía `OBJECT_DEFINITION()`), separados por `GO -- ===== <nombre> =====`.

Generado con `sqlcmd` contra catálogos de sistema (`sys.tables`, `sys.columns`, `sys.foreign_keys`, `sys.indexes`, `sys.procedures`) — **no** con "Generate Scripts" de SSMS (no hay forma de manejar esa UI sin mouse en esta sesión), ni con SMO/PowerShell (el módulo `SQLPS` 16.0 instalado devolvió colecciones vacías de forma inconsistente al enumerar tablas — descartado tras probarlo, ver intento fallido si se necesita reproducir). El contenido es equivalente en información a un DDL completo, aunque no es literalmente sentencias `CREATE TABLE` listas para ejecutar.

**Confirmado, sin `sp_set_session_context` embebido en ningún SP** — el RLS depende 100% de que la capa C# (`Utilidades/Conexion.cs`) lo setee en cada conexión, ningún stored procedure lo toca por su cuenta.

## Clasificación de los 117 stored procedures

Clasificación de primera pasada por patrón de nombre y grep de palabras clave (`AFIP`, `sp_set_session_context`, tablas de concurrencia) — **no** es una lectura línea por línea de los 117 cuerpos. Sirve para priorizar, se refina al portar cada uno.

### (c) Pendientes de decisión puntual — 9

No se asume ninguna clasificación, requieren mirar el cuerpo completo y decidir con el usuario antes de portar:

| SP | Por qué es pendiente |
|---|---|
| `addOrEditCierreCaja` | Toca `CierreCaja`, concurrencia de cierre de caja. |
| `addOrEditMovCtaCte` | Toca `MovCtaCte`, concurrencia de cuenta corriente. |
| `ventasVendedorCierreCaja` | Toca cierre de caja por vendedor. |
| `A1_CopiarBD_Diferente_Nombre` | Herramienta de provisioning/clonado de base, no lógica de request-path. |
| `A2_Crear_Claves_Foraneas_e_Indices` | DDL de mantenimiento, no lógica de negocio. |
| `A3_VaciarDatosTabla` | Operación destructiva de mantenimiento (vacía tablas) — nunca portar sin confirmación explícita. |
| `AA_AltaEmpresa` | Alta de un tenant nuevo — crítico para el multitenant, decisión de diseño aparte (cómo se provisiona un tenant en Postgres). |
| `achicaLog` | Mantenimiento/housekeeping, no lógica de negocio. |
| `sp_EmpresaParametros_SetDefaults` | Probablemente parte del alta de empresa — revisar junto con `AA_AltaEmpresa`. |

### (b) Candidatos a función en Postgres (agregación/reporting pesado) — 29

`Acum_Ventas`, `Balance`, `BalanceConMeses`, `BalanceConsFinal`, `BalanceConsFinal_FecDesde_Hasta`, `BalanceConsFinalVariosMeses`, `ControlLineasVtas`, `getPorcCortesEnMedias`, `getPromMedias`, `obtenerNivelCorte`, `obtenerEmbutidoTotal`, `obtenerTotalVentas`, `porcentajeCortesPorCompra`, `ResumenVentasMesPorCliente`, `StockCierre`, `StockCierre_2`, `StockIngresoEgreso`, `StockTeoricoReal`, `a_CierreStock`, `a_CierreStockWeb`, `a_ExistenciaStockPorSucursales`, `a_IngresoEgreso`, `TicketAnualdo`, `TotalKgsCortePorCompra`, `TotalMovimientosCorte`, `TotalMovimientosPorCorte`, `TotalPorCortesVendidos`, `TotalSegunCompras`, `TotalSegunComprasMonto`

### (a) Candidatos a portar como C# (CRUD/lógica simple) — 79 (el resto)

`addOrEditCompra`, `addOrEditCorte`, `addOrEditEgresoCaja`, `addOrEditFacturaElectronica`, `addOrEditFormula`, `addOrEditGasto`, `addOrEditMovimiento`, `addOrEditPago`, `addOrEditPersona`, `addOrEditUser`, `agregarActualizacionStock`, `agregarCompra`, `agregarCorte`, `agregarCortePorCompra`, `agregarCortePorEmbutido`, `agregarCortePorFormula`, `agregarCortePorMovimiento`, `agregarEmbutido`, `agregarExpendio`, `agregarLineaExpendio`, `agregarLineaVenta`, `agregarMediaRes`, `agregarStockVenta`, `agregarVenta`, `anularCompra`, `anularEmbutido`, `anularMovimiento`, `buscarCodigoCorte`, `buscarCorte`, `buscarCorteSinMaestro`, `buscarEmbutido`, `CargarBancos_BlocNotas`, `cargarCortesPorMovimiento`, `cargarMovimiento`, `cargarMovimientoOrigen`, `EliminarCorte`, `eliminarLineas`, `eliminarMovimiento`, `eliminarPersona`, `getAllLineasVenta`, `getCtaCteByIdPersona`, `getLineasCompras`, `getListaElegirEmbutido`, `getUsuariosActivos`, `IngresoMovIndependiente`, `modificarCompra`, `modificarCorte`, `modificarLineaVenta`, `modificarMediaPorCompra`, `modificarMovimiento`, `modificarPersona`, `modificarPrecioMedia`, `ModificarPrecioPorPorcentaje`, `modificarProveedor`, `modificarVenta`, `obtenerCompras`, `obtenerCortes`, `obtenerCortesPorCompra`, `obtenerCortesPorEmbutidos`, `ObtenerCortesPrimarios`, `obtenerEgresosCaja`, `obtenerEmbutidos`, `obtenerGastos`, `obtenerInfoCorte`, `obtenerLineasEmb`, `obtenerLineasMov`, `obtenerLineasVenta`, `obtenerMediasPorCompra`, `obtenerMovimientos`, `obtenerTemporalLineaVenta`, `obtenerVentas`, `quitarCortesPorMovimiento`, `quitarStockCorte`, `quitarStockMedia`, `quitarStockTeoricoMedia`, `ReiniciarCuarto`, `reiniciarStock`, `reiniciarStockTeorico`, `ultimasVentasCliente`

## Otros hallazgos de este relevamiento

- **AFIP/facturación electrónica no toca ningún SP** (`grep AFIP` sobre los 117 dio 0 resultados) — la lógica fiscal vive enteramente en C# (`AFIP/GenerarFacturaService.cs`), confirmando que no hace falta portar nada de AFIP a nivel de motor de base.
- **Solo 12 foreign keys reales** declaradas a nivel de motor para 55 tablas — la integridad referencial de la mayoría de las relaciones se resuelve en código C#, no en el esquema. Al migrar a Postgres, decidir si se declaran más FKs explícitas (más seguro) o se mantiene el mismo criterio permisivo (más fiel al comportamiento actual, menos trabajo).
