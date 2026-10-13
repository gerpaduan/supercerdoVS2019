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

## Existencia por sucursales: ventana independiente (2026-10-13)

`Stock/ExistenciaPorSucursales?popup=true` renderiza la vista con `Views/Shared/_LayoutEmbed.cshtml` (sin sidebar/topbar; mismo patron que `Productos/AddOrEdit?embed=true`). El boton "Abrir en ventana" de la vista normal llama al helper compartido `wwwroot/Scripts/app/ventana-independiente.js` (`window.open` con nombre fijo `existenciaPorSucursales`: un segundo click recarga esa ventana). `_LayoutEmbed` carga `table-scroll-sync.js` para que la tabla conserve la barra de scroll horizontal flotante. Las acciones de busqueda (`BuscarExistenciaPorSucursales`, `ObtenerFechaMinimaExistencia`) no cambian. Ver `docs/DECISIONS.md` (2026-10-13).

## Cierre de Stock: productos "no contado"

En `Stock/Editar`, el modal "Productos no cargados" completa en bloque los productos con `Corte.EnCierreStock=true` que el operador nunca escaneó/tipeó durante el conteo (con el stock actual, o en 0). Esas líneas quedan marcadas `NoContado=true` (columna `noContado`/`nocontado` en `dbo.CortePorCompra`/`corteporcompra`, ver bitácora 2026-09-28) y se muestran con un badge "No contado" en la grilla, tanto al cargar como al reabrir el registro guardado. Las líneas cargadas a mano (escaneo/código+Enter, o vinculación de pesajes) siempre quedan en `false`.
