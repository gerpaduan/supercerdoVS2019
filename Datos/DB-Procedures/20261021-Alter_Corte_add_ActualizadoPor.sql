-- Usuario que hizo la ultima modificacion de un producto (2026-10-21, ver docs/DECISIONS.md "Productos: cambiar el
-- precio cuenta como modificar el producto"): mismo agregado que
-- DatosPostgres/DB-Migrations/20261021-Alter_corte_add_actualizadopor.sql, para que las dos bases tengan el mismo esquema.
-- Columna NULLABLE al FINAL de la tabla, sin DEFAULT ni FK (igual que Compras/Cheques/EgresosCaja.actualizadoPor): para el
-- WinForms de produccion es invisible, porque Datos/Corte.cs lee por NOMBRE de columna, los INSERT/UPDATE del codigo
-- nombran sus columnas, y no hay DataSet tipado ni ordinales sobre Corte. SELECT * / Corte.* (findAllCortes,
-- buscarCodigoCorte, cargarDtCortes) devuelven una columna mas que nadie lee. Nadie en SQL Server escribe la columna:
-- queda NULL (el WinForms no la actualiza; si algun dia se quiere, hay que tocar Datos/Corte.cs y addOrEditCorte).
-- Idempotente. Es solo metadata (columna nullable): toma un Sch-M lock de instantes sobre dbo.Corte, correr con el WinForms
-- sin ventas en curso. Compatible con SQL Server 2008 (San Lorenzo).
-- Decision del usuario (2026-10-21): esta columna es SOLO para WebCore; el WinForms no se toca ni la escribe.
-- Deshacer (si algo raro aparece): ALTER TABLE dbo.Corte DROP COLUMN actualizadoPor;  (nadie mas la usa en SQL Server).
-- ANTES de correrlo en una base real: backup, y verificar que no haya triggers/vistas con SELECT * o INSERT sin lista de
-- columnas sobre dbo.Corte (ver consulta de abajo).
--
--   SELECT OBJECT_NAME(parent_id) AS tabla, name FROM sys.triggers WHERE parent_id = OBJECT_ID('dbo.Corte');
--   SELECT o.name, o.type_desc FROM sys.sql_modules m JOIN sys.objects o ON o.object_id = m.object_id
--   WHERE m.definition LIKE '%Corte%' AND o.type IN ('V','TR');
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Corte') AND name = 'actualizadoPor'
)
BEGIN
    ALTER TABLE dbo.Corte ADD actualizadoPor INT NULL;
END
