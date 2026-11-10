-- Textos de formularios que mentian (2026-11-10, ver docs/DECISIONS.md "Permisos que no controlaban nada").
--
--  * Cierres de Caja (idform 9) decia que "Puede ver" respeta los dias atras y no los respeta (el chequeo pasa la fecha de hoy).
--  * Reportes (25-30) y Etiquetas (21) decian que no hace falta Dias atras; ahora los reportes si los usan (limitan la fecha "desde").
-- Productos (13) y Tipos Productos (14) no cambian de texto: ya decian "Editar = Alta, Baja y Modificacion".
--
-- Texto sin tildes a proposito (igual que el resto del catalogo). formularios es un catalogo global (sin idempresa, sin RLS).
-- Correr conectado a "carnisys" con el rol carnisys_admin. Idempotente.
-- Reversion: restaurar los textos anteriores (ver docs/09-cambios-y-pendientes/bitacora-de-cambios.md, 2026-11-10).

UPDATE formularios SET descripcion =
    'Historial de todos los cierres de caja. Puede ver da acceso al historial y tambien a modificar un cierre historico; '
    || 'los dias atras no restringen nada (el chequeo mira solo la fecha de hoy).'
WHERE idform = 9;

UPDATE formularios SET descripcion =
    'Permite ver este reporte en Reportes (los de stock tambien se exportan a Excel). Marcar solo una de las opciones (Puede ver). '
    || 'Dias atras = hasta cuantos dias atras puede poner la fecha "desde" del reporte. Antes los reportes se habilitaban con el permiso Stocks.'
WHERE idform IN (25, 26, 27, 28, 29, 30);

UPDATE formularios SET descripcion =
    'Permite generar etiquetas de productos en PDF (boton Etiquetas de Productos). Marcar solo una de las opciones (Puede ver). '
    || 'No es necesario ingresar Dias Atras.'
WHERE idform = 21;
