USE [CarniSys]
GO

-- ============================================================================
-- Permisos que no controlaban nada (2026-11-10, ver docs/DECISIONS.md "Permisos que
-- no controlaban nada"). Espejo SQL Server del script de Postgres
-- DatosPostgres/DB-Migrations/20261110b-Update_permisosusuarios_reportes_desde_stocks.sql y 20261110c-Update_formularios_descripcion_reportes_etiquetas_cierres.sql.
--
--  1) Reportes (idForm 25-30): hasta hoy los seis reportes de Reportes/Index se habilitaban
--     con el permiso "Stocks" (idForm 5). Desde este cambio cada tipo de reporte exige su
--     propia fila. Para que NADIE pierda acceso se copia el "ver" (dias) de Stocks a las
--     seis filas de quien hoy tiene Stocks con ver >= 0. Solo pisa filas que estaban en -1
--     (sin acceso) e inserta la fila si el usuario no la tenia. Correr UNA vez.
--  2) Textos de Formularios que mentian: Cierres de Caja (idForm 9) decia que "Puede ver"
--     respeta los dias atras y no los respeta; reportes y Etiquetas decian que no hace
--     falta Dias atras.
--
-- ANTES DE CORRER (servidor SM y San Lorenzo): verificar en solo lectura que existan las
-- filas de Formularios 21 y 25-30 y que PermisosUsuarios tenga las columnas usadas abajo:
--   SELECT idForm, nombreForm, formConsulta FROM Formularios WHERE idForm IN (5, 9, 21, 25, 26, 27, 28, 29, 30);
--   SELECT TOP 3 * FROM PermisosUsuarios;
-- (el nombre de la columna de empresa de PermisosUsuarios no se verifico: si no es idEmpresa,
-- ajustar el INSERT; si la tabla no la tiene, sacarla).
--
-- IMPORTANTE al correr via sqlcmd: usar -f i:65001,o:65001 (UTF-8 explicito).
-- Formularios y PermisosUsuarios tienen RLS: hace falta el session context de admin (ver
-- 20260807-Update_Formularios_DescripcionCierresDeCaja.sql). Idempotente salvo por la
-- aclaracion de (1).
-- Reversion de (1): UPDATE PermisosUsuarios SET diasPermitidosVer = -1 WHERE idForm IN (25,26,27,28,29,30);
--            de (2): restaurar los textos anteriores (ver docs/09-cambios-y-pendientes/bitacora-de-cambios.md, 2026-11-10).
-- ============================================================================

EXEC sp_set_session_context 'EsAdminCarniSys', 1;

-- (1) Reportes: copiar el "ver" de Stocks. Primero pisar las filas que ya existen sin acceso...
UPDATE r
SET r.diasPermitidosVer = s.diasPermitidosVer
FROM PermisosUsuarios r
INNER JOIN PermisosUsuarios s ON s.idUsuario = r.idUsuario AND s.idForm = 5
WHERE r.idForm IN (25, 26, 27, 28, 29, 30)
  AND r.diasPermitidosVer < 0
  AND s.diasPermitidosVer >= 0;

-- ...y despues insertar las que el usuario no tenia.
INSERT INTO PermisosUsuarios (idUsuario, idForm, diasPermitidosVer, diasPermitidosEditar, soloRegistrosPropios, idEmpresa)
SELECT s.idUsuario, f.idForm, s.diasPermitidosVer, -1, 1, s.idEmpresa
FROM PermisosUsuarios s
CROSS JOIN (VALUES (25), (26), (27), (28), (29), (30)) AS f(idForm)
WHERE s.idForm = 5 AND s.diasPermitidosVer >= 0
  AND NOT EXISTS (SELECT 1 FROM PermisosUsuarios x WHERE x.idUsuario = s.idUsuario AND x.idForm = f.idForm);

-- (2) Textos.
UPDATE Formularios
SET descripcion = N'Historial de todos los cierres de caja. "Puede ver" da acceso al historial y también a modificar un cierre histórico; los días atrás no restringen nada (el chequeo mira solo la fecha de hoy).'
WHERE idForm = 9;

UPDATE Formularios
SET descripcion = N'Permite ver este reporte en Reportes (los de stock también se exportan a Excel). Marcar solo una de las opciones (Puede ver). Días atrás = hasta cuántos días atrás puede poner la fecha "desde" del reporte. Antes los reportes se habilitaban con el permiso Stocks.'
WHERE idForm IN (25, 26, 27, 28, 29, 30);

UPDATE Formularios
SET descripcion = N'Permite generar etiquetas de productos en PDF (botón Etiquetas de Productos). Marcar solo una de las opciones (Puede ver). No es necesario ingresar Días Atrás.'
WHERE idForm = 21;
GO
