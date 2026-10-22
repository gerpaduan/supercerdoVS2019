# Stock e inventario

## Objetivo

Relevar altas, movimientos, ajustes, control de stock e inventarios.

## Secciones

- Pantallas involucradas
- Reglas de actualizacion
- Movimientos de stock
- Controles y cierres
- Riesgos

## Recuperación de borrador sin cerrar

Ver `docs/DECISIONS.md` "Borradores en servidor de Compras/Stock/Movimientos/Embutidos" (2026-09-22). Cubre tanto **Stock** (alta/edición de movimiento de stock, `Stock/Editar`) como **Movimientos** (traslado entre sucursales, `Movimientos/Editar`) — dos módulos, dos discriminadores (`STOCK`/`MOVIMIENTO`) de la misma tabla `borradorgenerico`.

Con `BorradorGenerico:Habilitado` (Postgres: prendido por defecto; SQL Server: opt-in desde 2026-09-23, tras correr `Datos/DB-Procedures/20260923-Create_Borradores.sql`); con el feature apagado, ambas pantallas siguen con el resguardo local de siempre (`localStorage` + captura de pantalla en Stock; solo `localStorage` en Movimientos), sin cambios.

- **Stock** guarda con un POST tradicional (`StockController.Guardar`, `RedirectToAction`, no AJAX): el `clientId` del borrador viaja en un campo hidden (`BorradorGenericoClientId`) y el servidor marca el borrador `FINALIZADA` (`MarcarBorradorGenericoFinalizado`, mejor esfuerzo) al guardar, porque no hay respuesta JSON donde el navegador pueda hacerlo. Botón "Stock sin cerrar" junto a "Volver".
- **Movimientos** guarda por AJAX (`MovimientosController.Guardar` devuelve JSON): el propio JS (`movimientos.js`) marca el borrador finalizado tras la respuesta exitosa. Botón "Movimientos sin cerrar" junto a "Imprimir".
- En ambos, cualquier operador de la sucursal ve los borradores sin cerrar del módulo y puede recuperar/descartar los propios; uno ajeno exige autorización de supervisor.
- Un borrador **propio** que figura "En uso" (corte de luz/cuelgue, todavía dentro de `BorradorGenerico:MinutosSinLatidoInterrumpida`) se puede cargar igual confirmando "Cargar igual" (desde 2026-10-04; rota el `clientId`, ver `docs/DECISIONS.md` 2026-10-04). Descartar sigue esperando el umbral.

## Columna "Creado" en los index (2026-10-09)

Los listados `Stock/Index` (`_StockTabla.cshtml`) y `Movimientos/Index` tienen una columna **Creado** antes de **Acciones**: fecha y hora de creación, debajo el usuario creador y, si el registro se modificó (`actualizado` con valor), la etiqueta **Modificado** con su fecha/hora y usuario. Celda compartida `Views/Shared/_CeldaCreadoModificado.cshtml`; datos de `CompraPg.obtenerCompras` y `CortePg.obtenerMovimientos` (Movimientos los vuelca en `MovimientoResumenVm` desde `ConstruirResumenes`). En Stock, `actualizado` también se toca al vincular un pesaje con su ajuste u observaciones (`actualizarIdPesajeAjustado`, `actualizarObservacionesCompra`). Ver `docs/DECISIONS.md` (2026-10-09).

## Código sin agregar bloquea el guardado (2026-10-09)

En `Stock/Editar` y `Movimientos/Editar` no se puede guardar mientras el campo **Código** del buscador tenga un valor que no se agregó a la tabla (se olvidó Enter / "+"): aviso "Validacion", beep y foco en Código; hay que agregarlo o borrarlo. La Cantidad no cuenta (la balanza la completa sola). Implementado en `WebCore/wwwroot/Scripts/app/producto-pendiente-guard.js`, llamado desde el `submit` de `stock.js` y `movimientos.js`. Los atajos de teclado no cambian. Ver `docs/DECISIONS.md` (2026-10-09).

## Movimientos: un solo guardado por movimiento y aviso post-guardado (2026-10-17)

`Movimientos/Editar` no admite un segundo guardado mientras hay uno en vuelo ni mientras el aviso **"Movimiento guardado"** (`_ModalPostMovimiento.cshtml`, `modal-postmovimiento.js`) sigue abierto. Tras guardar, el formulario queda enlazado al id devuelto por `MovimientosController.Guardar` (`#IdMovimiento`): si la persona elige "Imprimir" (que se queda en la pantalla) y vuelve a guardar, **actualiza** ese movimiento en vez de crear otro. "Imprimir" cierra primero el aviso y recién después manda a imprimir (`TicketPrint`). Implementado en `WebCore/wwwroot/Scripts/app/movimientos.js` (`state.enviando`, enlace del id) y `modal-postmovimiento.js` (`cerrarModal(alCerrar)`, `tomarAccion`, `PostMovimientoModal.estaAbierto()`). Ver `docs/DECISIONS.md` (2026-10-17).

## Existencia por sucursales: ventana independiente (2026-10-13)

`Stock/ExistenciaPorSucursales?popup=true` renderiza la vista con `Views/Shared/_LayoutEmbed.cshtml` (sin sidebar/topbar; mismo patron que `Productos/AddOrEdit?embed=true`). El boton "Abrir en ventana" de la vista normal llama al helper compartido `wwwroot/Scripts/app/ventana-independiente.js` (`window.open` con nombre fijo `existenciaPorSucursales`: un segundo click recarga esa ventana). `_LayoutEmbed` carga `table-scroll-sync.js` para que la tabla conserve la barra de scroll horizontal flotante. Las acciones de busqueda (`BuscarExistenciaPorSucursales`, `ObtenerFechaMinimaExistencia`) no cambian. Ver `docs/DECISIONS.md` (2026-10-13).

## Existencia por sucursales: compacto, orden, Estado, filtros y exportación (2026-10-19, Fase 1)

Pantalla `Stock/ExistenciaPorSucursales`. **Piezas**: `Views/Stock/ExistenciaPorSucursales.cshtml` (barra, filtros avanzados, estilos) + `_TablaExistenciaPorSucursales.cshtml` (tabla, solo HTML con atributos `data-` para ordenar/filtrar) + `wwwroot/Scripts/app/existencia-sucursales.js` (toda la lógica del navegador). En Negocio, puro y con tests: `ExistenciaEstado` (estado de una celda con el punto **real** de la sucursal; el SQL usa el punto global legacy), `ExistenciaFiltros` (Tipos/Marcas/Proveedores múltiples y "Con movimiento"), `LimitadorCalculo` (protección de carga). `Negocio.Corte.ObtenerExistenciaPlano` aplica la multiselección (1 valor → al motor, 2+ → en memoria; proveedores siempre en memoria con `ICorteRepository.obtenerProductosCompradosPorProveedor`: compras tipo `Cortes`). "Con movimiento" y las sugerencias futuras usan `IActividadStockRepository.ObtenerActividadStock(idSucursal, DateTime desde)` (solo Postgres). `StockController`: `BuscarExistenciaPorSucursales` (parámetro `auto` → 503 si el servidor está ocupado, `ExistenciaCargaGuard` + `ExistenciaCargaSettings`, claves `ExistenciaCarga:*` de `WebCore.dll.config`) y `ExportarExistencia` (Excel/PDF: `ExistenciaExportacion`, `ExcelExistenciaSucursales`, `PdfExistenciaSucursales`). Editar el punto de stock reusa `ProductosController.GuardarPuntosStockSucursal`. Decisiones, hallazgos y descartados: `docs/DECISIONS.md` (2026-10-19); uso: `docs/11-manual-de-uso/pantallas/Stock.ExistenciaPorSucursales.md`.

## Existencia por sucursales: Ingresar pedido (2026-10-19, Fase 2)

Modo **Ingresar pedido** de `Stock/ExistenciaPorSucursales` (solo Postgres): una columna **A pedir** por sucursal (más Total pedido, $ últ. compra e Importe est. al final), sugerencias que se ven pero no se cargan solas, autoguardado del pedido en curso y vista preliminar previa al guardado. **Piezas**: `wwwroot/Scripts/app/existencia-pedido.js` (toda la lógica del navegador; `ExistenciaPedidoCalculo` = reglas puras, espejo de `Negocio/PedidoSugerencia.cs`; engancha con `existencia-sucursales.js` por `window.ExistenciaPantalla` y eventos `existencia:*`), columnas/inputs en `_TablaExistenciaPorSucursales.cshtml` (ocultas por CSS hasta encender el modo), barra y vista preliminar en `ExistenciaPorSucursales.cshtml`. **Servidor** (`StockController`): `GET Stock/ConsumoPeriodo?desde=` (ventas + uso en elaborados por producto y sucursal, vía `ReposicionStock.ObtenerConsumoPeriodo` → `IActividadStockRepository.ObtenerActividadStock(0, desde)`, con la jerarquía madre/hija) y `GET Stock/UltimosPreciosCompra` (`corteproveedor.ultimoprecio` del proveedor habitual, la fuente de «Qué pedir»; `ReposicionStock.ObtenerUltimosPreciosCompra`). **Borrador**: módulo `PEDIDO_STOCK` de `BorradorGenerico` (`BorradoresGenericoController`, `borrador-generico.js`, modal `_ModalBorradoresGenerico`); **requiere la migración `20261019b-Alter_borradorgenerico_modulo_pedido_stock.sql`** (CHECK del módulo). **Guardado**: `POST Pedidos/Guardar` (módulo Pedidos, abajo) + `PedidoAcciones.abrirDialogo`; `ticket-print.js` se carga en la ventana independiente. Reglas, cobertura de precios medida y descartados: `docs/DECISIONS.md` (2026-10-19, Ingresar pedido); uso: manual de la pantalla (sección *Ingresar pedido*).

## Pedidos de mercadería (2026-10-19, Fase 3)

Módulo `Pedidos` (menú Stock → Pedidos), **solo Postgres**. **Piezas**: `Controllers/PedidosController.cs` (Index, Buscar, Detalle, Editar, Guardar, Modificar, Ticket, TicketPayload, Pdf, Excel, DatosEmail, EnviarEmail), vistas `Views/Pedidos/{Index,_Tabla,Detalle,Editar}.cshtml`, JS `wwwroot/Scripts/app/pedido-acciones.js` (diálogo posterior al guardado y acciones: Cerrar, ticket, PDF, mail, Excel) y `pedidos-editar.js`, servicios `Services/{PedidoPivot,PedidoTicket,ExcelPedido,PdfPedido}.cs` (el ticket térmico sale **uno por sucursal, sin importes**, con el formato del ticket de Movimientos y el comando de corte `EscPosFormato.Corte()` entre sucursales; tests en `WebCore.Tests/PedidoTicketTests.cs`), y las capas `Entidades/Pedido.cs`, `Contratos/IPedidoRepository`, `DatosPostgres/PedidoPg`, `Negocio/Pedido` (reglas puras en `Armar`; tests en `Negocio.Tests/PedidoTests.cs`).

**Datos**: `pedido` (cabecera), `pedidolinea` (una línea por producto y sucursal, con snapshot de precio de última compra, proveedor e importe) y `pedidoevento` (historial append-only con autorizador), todas con RLS por empresa (`20261019-Create_pedidos.sql`). Guardado **idempotente** por `clientid`. Importe = `round(cantidad × precio, 2)` solo con precio > 0; sin precio no suma y se avisa. Al modificar se **conservan los precios guardados**; las líneas nuevas quedan sin precio.

**Permisos**: `Stock.VerStock` ver y guardar; modificar pide `Stock.AddOrEditStock` o admin; una **cuenta de producción** necesita además usuario y clave de **una persona** autorizada (no puede ser otra cuenta de producción), que queda en el historial. **Control optimista** por versión (milisegundos de la última modificación; **no usar Ticks**: JavaScript no los representa exactos). `Pedidos/EnviarEmail` (mail real) valida anti-forgery.

**Precio de los importes** (2026-10-19): la última compra real de cada producto (`IActividadStockRepository.ObtenerUltimasComprasPorProducto`, líneas de compras tipo Cortes sin anular), sin distinguir proveedor; una compra de hace más de `Pedidos:DiasPrecioMaximo` (365) días no da precio y la de hace más de `Pedidos:DiasPrecioViejo` (90) se marca ⚠; el total se rotula «parcial» si falta algún precio. Textos en `Negocio/PedidoTextos.cs`. "Qué pedir" sigue con `corteproveedor`. Ver `docs/DECISIONS.md` (2026-10-19, «Pedidos: fuente del precio»).

**Contrato con la pantalla de armado** (Existencia por sucursales → *Ingresar pedido*): `POST Pedidos/Guardar` devuelve `acciones` (URLs) que se pasan a `PedidoAcciones.abrirDialogo`. Detalle y decisiones: `docs/DECISIONS.md` (2026-10-19, Pedidos). Manual: `docs/11-manual-de-uso/pantallas/Pedidos.{Index,Detalle,Editar}.md`.

## Cierre de Stock: productos "no contado"

En `Stock/Editar`, el modal "Productos no cargados" completa en bloque los productos con `Corte.EnCierreStock=true` que el operador nunca escaneó/tipeó durante el conteo (con el stock actual, o en 0). Esas líneas quedan marcadas `NoContado=true` (columna `noContado`/`nocontado` en `dbo.CortePorCompra`/`corteporcompra`, ver bitácora 2026-09-28) y se muestran con un badge "No contado" en la grilla, tanto al cargar como al reabrir el registro guardado. Las líneas cargadas a mano (escaneo/código+Enter, o vinculación de pesajes) siempre quedan en `false`.

## Vincular y desvincular pesajes (Stock/Editar de un Pesaje)

"Vincular pesajes" copia las líneas del pesaje elegido dentro del pesaje que se edita (con la etiqueta efímera `IdPesajeVinculado`) y, al Guardar, `StockController.SincronizarPesajesVinculados` le pone `idpesajeajustado = <este pesaje>` al pesaje elegido. **Desvincular** (2026-10-16, botón en la columna Acción del mismo modal, con confirmación): quita de este pesaje las líneas que vinieron de ese pesaje y lo saca de `PesajesVinculadosIds`, así que al Guardar el servidor le deja `idpesajeajustado = NULL`. Todo se aplica al Guardar (el estado vive en `stock.js`: `state.pesajesDesvinculados`, que también viaja en el borrador). Como el origen de cada línea no se persiste, las líneas de un pesaje ya guardado se reconocen **por contenido** (mismo producto y kilos que las líneas actuales del pesaje vinculado); si ese pesaje se editó después de vincularlo, esas líneas no coinciden y el diálogo avisa cuántas no se encontraron. Decisión y alternativas: `docs/DECISIONS.md` (2026-10-16). Manual: `docs/11-manual-de-uso/pantallas/Stock.Editar.md`.

## Qué pedir: reposición sugerida (2026-10-13)

`Stock → Qué pedir` (`ReposicionController`, ruta `/Reposicion`, solo Postgres, solo lectura, permiso `Stock.VerStock`). Para cada producto que lleva stock y cada sucursal: si el stock está **por debajo del punto de stock** sugiere pedir `punto − stock` (stock negativo cuenta como 0); si **no tiene punto** y está en cero, lo avisa **solo si salió algo (ventas + consumo por elaboración) en los últimos N días** (30 por defecto), y los en cero sin movimiento quedan como contador. Agrupa por proveedor habitual (`corteproveedor`, compra más reciente), muestra consumo/día, días que alcanza, última salida, stock en otras sucursales y último precio/importe, y exporta a Excel.

- **Piezas:** `Negocio/ReposicionStock.cs` (`ReposicionStock` orquesta; `ReposicionStockCalculador` tiene las reglas, puras, con tests en `Negocio.Tests/ReposicionStockCalculadorTests.cs`); stock y punto salen de `Negocio.Corte.ObtenerExistenciaPlano` (la misma base que Existencia por sucursales); actividad y proveedor de `CortePg.ObtenerActividadStock` / `ObtenerProveedoresHabituales` vía `Contratos.IActividadStockRepository`. La jerarquía madre/hija se comparte con la existencia (`CortePg.SqlPrepCorteEfectivoPorSucursal`, `SqlPrepMapaCorte`).
- **Dependencias de datos:** necesita que el stock tenga un cierre reciente por sucursal y que los productos tengan **punto de stock por sucursal** (`cortepuntostocksucursal`; se carga en Productos → Ver stock por sucursales) y **proveedor** (`corteproveedor`, se completa solo con cada compra). Sin punto de stock solo hay aviso de "en cero".
- **No hace:** no registra pedidos a proveedor ni "pedidos en camino"; no considera múltiplos de caja; no calcula puntos de pedido estadísticos (no hay lead time registrado).
- Manual de uso: `docs/11-manual-de-uso/pantallas/Reposicion.Index.md`. Decisión, hallazgos con datos y alternativas descartadas: `docs/DECISIONS.md` (2026-10-13, "Qué pedir").

### Sugerir puntos de stock (2026-10-13)

`Stock → Qué pedir → Sugerir puntos de stock` (`Reposicion/Puntos`, mismo controller, solo Postgres). Para cada producto y sucursal **sin punto de stock y con consumo** propone `consumo por día × días a cubrir` (por defecto 30 días de consumo y 7 de cobertura; entero, mínimo 1) y deja cargar los que el usuario tilde, con el número editable, confirmación y recalculo. Ver: `Stock.VerStock`; cargar: además `Producto.NuevoCorte` (el mismo de Productos).

- **Qué escribe:** `cortepuntostocksucursal` vía `CortePuntoStockSucursal.GuardarPuntosStockLote` (upsert), un producto por vez. **Nunca pisa un punto ya cargado**: lo revalida el servidor (`ReposicionStockCalculador.ValidarPuntosAGuardar`: producto que lleva stock, sin repetidos, entero 1–999.999, máx. 500 por pedido). Queda un log con usuario y cantidad; no hay "deshacer".
- **Piezas:** `ReposicionStock.ArmarPuntosSugeridos` / `GuardarPuntosSugeridos`, `ReposicionController.Puntos` / `BuscarPuntos` / `GuardarPuntos`, `Views/Reposicion/Puntos.cshtml` + `_TablaPuntos.cshtml`, `reposicion-puntos.js`. Compartidos con *Qué pedir*: `reposicion-comun.js`, `Content/css/reposicion.css`, `Helpers/ReposicionFormato.cs`.
- Manual: `docs/11-manual-de-uso/pantallas/Reposicion.Puntos.md`. Decisión: `docs/DECISIONS.md` (2026-10-13, "Sugerir puntos de stock").

## Eliminar stock y movimientos (2026-10-22)
- **Stock** (Ingreso, Egreso, Cierre, Ajuste, Pesaje; viven en la tabla `compras`): `Stock/PrevisualizarEliminar` y `Stock/Eliminar`, mismo `Negocio.Compra.eliminarCompra` que Compras (con `moduloEsperado = STOCK`: desde Compras no se borra stock y al revés). **Ajuste y Cierre: solo admin.** Un Pesaje con pesajes vinculados o con ajuste generado se bloquea; eliminar un Ajuste recalcula el estado del pesaje. El stock se calcula al consultar, así que deja de contar el registro apenas se borra.
- **Movimientos**: `Movimientos/PrevisualizarEliminar` y `Movimientos/Eliminar` → `Negocio.Corte.eliminarMovimientoConMotivo` (`ICorteRepository.eliminarMovimientoConAuditoria`). **Deshabilitado mientras `Movimientos:ReplicaEntreBases` sea `true`** (por defecto con SQL Server: Servidor SM y San Lorenzo replican sus movimientos y la copia en la otra base no se borra sola). El `eliminarMovimiento` anterior sigue existiendo para el WinForms (sin motivo).
- Todo registro eliminado queda en **Registros eliminados** (solo admin): quién lo eliminó, cuándo, el motivo y todas sus líneas.
- Detalle: `docs/DECISIONS.md` (2026-10-22). Manual: `Stock.Editar.md`, `Movimientos.Editar.md`, `RegistrosEliminados.Index.md`.
