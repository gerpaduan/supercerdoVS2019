-- Reportes con permiso propio (2026-11-10, ver docs/DECISIONS.md "Permisos que no controlaban nada").
--
-- Hasta hoy los seis reportes de Reportes/Index (permisos idform 25-30) se habilitaban con el permiso "Stocks" (idform 5)
-- y esas filas no las miraba nadie. Desde este cambio cada tipo de reporte exige su propia fila. Para que NADIE pierda
-- acceso, se copia el "ver" (dias) de Stocks a las seis filas de quien hoy tiene Stocks con ver >= 0. Solo pisa filas
-- que estaban en -1 (sin acceso): lo que ya tuviera una fila de reporte no se toca. Inserta la fila si el usuario no la
-- tenia. Correr UNA vez: si despues se le quita un reporte a alguien y se vuelve a correr, se lo devuelve (mientras
-- tenga Stocks).
--
-- permisosusuarios tiene RLS por idempresa: correr conectado a "carnisys" con el rol cs_admin_pg (BYPASSRLS), que cubre
-- todas las empresas (carnisys_admin NO lo bypasea y cs_admin_pg no puede tocar formularios: los textos van en el script
-- 20261110c). Antes de correr en produccion, mirar en solo lectura: SELECT count(*) FROM permisosusuarios WHERE idform = 5 AND diaspermitidosver >= 0;
-- Reversion: UPDATE permisosusuarios SET diaspermitidosver = -1 WHERE idform IN (25,26,27,28,29,30);

-- (1) Reportes: copiar el "ver" de Stocks.
INSERT INTO permisosusuarios (idusuario, idform, diaspermitidosver, diaspermitidoseditar, soloregistrospropios, idempresa)
SELECT s.idusuario, r.idform, s.diaspermitidosver, -1, true, s.idempresa
FROM permisosusuarios s
CROSS JOIN (VALUES (25), (26), (27), (28), (29), (30)) AS r(idform)
WHERE s.idform = 5 AND s.diaspermitidosver >= 0
ON CONFLICT (idusuario, idform) DO UPDATE
    SET diaspermitidosver = EXCLUDED.diaspermitidosver
    WHERE permisosusuarios.diaspermitidosver < 0;
