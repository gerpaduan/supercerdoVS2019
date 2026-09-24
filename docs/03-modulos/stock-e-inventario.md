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
