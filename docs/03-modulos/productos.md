# Productos

## Objetivo

Relevar el modelo de datos y el flujo de alta/edicion de productos (entidad `Corte` en el codigo, aunque en la Web y en el negocio se llama "Producto"), incluido el catalogo global compartido entre empresas.

## Entidades

- **`dbo.Corte`** (`Entidades.Corte`, `Datos.Corte`, `Negocio.Corte`): productos propios de cada empresa. Multi-tenant por columna `idEmpresa`, con RLS (`RLS_Empresa`, funcion `fn_rls_empresa_o_global_v2`/`fn_rls_block_empresa_o_global_v2` para INSERT/UPDATE/DELETE) que exige `SESSION_CONTEXT('IdEmpresa')` seteado (o el login `cs_admin`, o `SESSION_CONTEXT('EsAdminCarniSys')=1`) para ver o modificar **cualquier** fila -- sin eso la sesion no ve nada. Ninguna de las dos funciones esta versionada en el repo (traidas con `OBJECT_DEFINITION` el 2026-08-05, ver `docs/DECISIONS.md`).
- **`dbo.CatalogoGlobalProducto`** (`Entidades.CatalogoGlobalProducto`, `Datos.CatalogoGlobalProducto`, `Negocio.CatalogoGlobalProducto`): catalogo global de productos, compartido entre todas las empresas. Separado de `dbo.Corte` el 2026-08-05 (antes vivia mezclado ahi con `idEmpresa = 0`, ver `docs/DECISIONS.md`). Sin RLS -- toda fila es visible para cualquier empresa por definicion. Mismas columnas de negocio que `Corte` menos `idEmpresa`. Script de creacion: `Datos/DB-Procedures/20260804-Create_CatalogoGlobalProducto.sql`.
- **`dbo.CatalogoGlobalImportacionProductos`** (`Entidades.CatalogoGlobalImportacionProducto`): tabla puente que registra que producto global (`IdProductoGlobal`, ahora apunta a `dbo.CatalogoGlobalProducto.idCorte`) fue importado como que producto de empresa (`IdProductoEmpresa`, apunta a `dbo.Corte.idCorte`), para evitar reimportar el mismo global dos veces en la misma empresa. Creada perezosamente desde codigo (`Datos.Corte.AsegurarTablaImportacionCatalogoGlobal`, `IF OBJECT_ID(...) IS NULL CREATE TABLE`), no tiene script de creacion propio versionado.

## Flujo de alta

Dos caminos, conviven en `Web/Controllers/ProductosController.cs`:

1. **Alta manual**: `Crear()` -> `AddOrEdit.cshtml` -> `POST Guardar(CorteUpsertVM vm)`, persiste via `Negocio.Corte.addOrEditCorte` (SP `addOrEditCorte`, legacy, sin script versionado).
2. **Alta desde catalogo global** (el mecanismo relevante de este documento):
   - `VerGlobales`/`BuscarGlobales` abren y pueblan el modal (`_CatalogoGlobalModal.cshtml`/`_CatalogoGlobalRows.cshtml`), leyendo `Negocio.CatalogoGlobalProducto.ObtenerCatalogoGlobalPagina` (paginado) contra `dbo.CatalogoGlobalProducto`.
   - `ImportarSeleccionados` clona los productos elegidos (`ClonarProductoGlobal(Entidades.CatalogoGlobalProducto, codigoDestino, precio)`) e inserta cada uno en la empresa actual con `Negocio.Corte.InsertarCorteEnEmpresa` (INSERT crudo parametrizado a `dbo.Corte` con el `idEmpresa` de la sesion), y registra la trazabilidad en `CatalogoGlobalImportacionProductos`.
   - `BuscarProductoGlobalParaAlta`: autocompleta el formulario de alta manual (`AddOrEdit.cshtml`) al escanear un codigo de barra que existe en el catalogo global, mismo mecanismo de clonado. (El boton "Agregar por codigo de barra" de `Productos/Index` -- alta rapida sin pasar por el formulario -- se saco el 2026-08-08 por obsoleto; sus acciones `BuscarPorCodigoBarraGlobal`/`AgregarDesdeCodigoBarra` ya no existen.)
   - `Guardar`: si el codigo tipeado a mano coincide con uno del catalogo global (`Negocio.CatalogoGlobalProducto.findCorteGlobalByCodigo`), se marca `altaDesdeCatalogoGlobal=true` y se inserta via `InsertarCorteEnEmpresa` en vez de `addOrEditCorte` -- este chequeo es solo un existence-check (booleano), no clona campos; el usuario ya tipeo el formulario a mano.
   - `ClonarProductoGlobal` tiene un segundo overload que toma `Entidades.Corte` en vez de `Entidades.CatalogoGlobalProducto`: cubre el caso defensivo de editar (`vm.IdCorte > 0`) un producto que resulta tener `IdEmpresa == 0` -- residual de cuando el catalogo global vivia dentro de `Corte`. Deberia dejar de dispararse una vez que se corra el borrado de esas filas (`Datos/DB-Procedures/20260804-Delete_Corte_IdEmpresa0.sql`), pero se dejo sin retirar por las dudas.

## Ajuste de precio por forma de pago (WebCore, desde 2026-09-25)

Recargo (+) o descuento (-) sobre el **precio de lista** del producto segun como pague el cliente. Es **un solo porcentaje por forma de pago para toda la empresa** (no por producto). Se edita desde `Productos/Index` -> boton "Ajuste por forma de pago" (modal `#modalAjusteFormaPago`), solo con el permiso `Permisos.Producto.ModificarPrecios` (`formModificarPrecios`).

- **Formas**: Efectivo, Debito, Credito, Qr, Transferencia (`Negocio.AjusteFormaPago.Formas`). CtaCte siempre sin ajuste; Billetera no existe como forma de pago.
- **Interruptor**: apagado = sin recargo ni descuento (se guarda factor `1`). No se recuerda el % previo.
- **Unidad**: el usuario ve/carga porcentaje con signo (`+10`, `-5,5`; mayor a -100, hasta 500, max 2 decimales). Se **guarda como factor** (`1,10`) en `empresaparametros` bajo las claves `porcAjEfectivo/Debito/Credito/Qr/Tranf` -- es lo que ya lee el POS, que no cambio.
- **Endpoints** (`WebCore/Controllers/ProductosController.cs`): `GET Productos/AjustesFormaPago` (JSON con `clave/activo/porcentaje`) y `POST Productos/GuardarAjustesFormaPago` (form-urlencoded, antiforgery; campos `<Clave>_activo` y `<Clave>_porcentaje`). Valida en servidor; guarda las 5 claves en una sola transaccion via `Negocio.Parametros.GuardarValores`.
- **Conversion y validacion**: `Negocio/AjusteFormaPago.cs` (tests: `Negocio.Tests/AjusteFormaPagoTests.cs`).
- **Parametros**: los `porcAj*` (y `porcAjBilletera`) quedaron ocultos en la pantalla Parametros (`ParametrosController.EsParametroSiempreOculto`); ya no se editan ahi.
- **Como se aplica en el POS**: en el navegador, `precioLista x factor` (`wwwroot/Scripts/app/pos-forma-pago-precios.js`); el servidor guarda el precio final de cada linea y no recalcula. Si algun factor es distinto de 1 (`RequierePreseleccionFormaPagoPOS`), el POS abre el modal de forma de pago **al cargar la pagina**: si se elige una, los precios se calculan con ella; si se cierra con Escape, se carga a **precio de lista** y la forma se elige al finalizar la venta, donde el carrito se recalcula y se avisa el cambio de total (2026-09-26). Las ventas ya guardadas no cambian.
- **Limite conocido**: no hay historial de quien cambio el porcentaje (igual que cuando eran parametros).

## Dependencias

- `Web/Models/CorteUpsertVM.cs`, `CatalogoGlobalProductosVm.cs`, `ProductoGlobalSeleccionVm.cs`, `ImportarProductosGlobalesRequest.cs`.
- Mismo patron replicado para "Tipos de producto" (`dbo.TiposProducto`, catalogo global mezclado por `idEmpresa=0` dentro de la misma tabla) -- **no separado todavia**, queda pendiente si en el futuro se pide lo mismo para Tipos.
- `Web/Controllers/StockController.cs`, `VentasController.cs`, `ReportesController.cs`, `MovimientosController.cs`, `ElaboradosController.cs`, `ComprasController.cs` leen `Negocio.Corte`/`dbo.Corte` (nunca `CatalogoGlobalProducto` directamente).

## Riesgos conocidos

- Stored procedures legacy sobre `dbo.Corte` (`buscarCodigoCorte`, `buscarCorteSinMaestro`, `addOrEditCorte`) no tienen su SQL versionado en el repo. Antes de alterarlos, traer el texto real con `sp_helptext`/`OBJECT_DEFINITION` (regla ya establecida, ver `docs/09-cambios-y-pendientes/riesgos-conocidos.md`).
- Cualquier query nueva sobre `dbo.Corte` que necesite ver "todos los productos" debe filtrar explicitamente por `idEmpresa` -- la RLS no alcanza como filtro funcional (ver `docs/07-operacion-y-soporte/incidencias-frecuentes.md`, REGLA del 2026-08-01).
