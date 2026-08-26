-- Etapa: System Administration (ultimo modulo 100% SQL Server, ver docs/DECISIONS.md 2026-08-25).
-- Cierra 2 gaps de columnas encontrados al comparar dbo.Empresas/dbo.Sucursal de SQL Server
-- (confirmado en vivo con sys.columns, servicio reanudado) contra el schema de Postgres creado
-- en la Etapa 3 (20260818-Create_sucursal_empresas.sql):
--   - empresas: le falta "observaciones" (nvarchar(max) en SQL Server, usada por
--     AA_AltaEmpresa y por SystemAdministrationRepository.ObtenerEmpresa/ActualizarEmpresa).
--   - sucursal: le faltan "creado" (date) y "observaciones" (nvarchar(max)), ambas escritas
--     por AA_AltaEmpresa (paso 4, alta de la sucursal default) y por
--     SystemAdministrationRepository.CrearSucursal/ActualizarSucursal.
--
-- "telefono" y "activa" de Sucursal NO se agregan: confirmado en vivo (sys.columns contra
-- SQL Server real) que tampoco existen hoy en ese motor -- el codigo defensivo que las detecta
-- dinamicamente (SystemAdministrationRepository.TablaSucursalTieneTelefono/TieneActiva) es
-- para una migracion de schema que nunca paso en ningun motor. El port de este modulo a
-- Postgres asume ambas columnas ausentes siempre, sin introspeccion dinamica.

ALTER TABLE empresas
    ADD COLUMN IF NOT EXISTS observaciones text;

ALTER TABLE sucursal
    ADD COLUMN IF NOT EXISTS creado date,
    ADD COLUMN IF NOT EXISTS observaciones text;

-- Sin GRANT nuevo: las columnas se agregan a tablas que ya tienen
-- "GRANT SELECT, INSERT, UPDATE, DELETE ... TO carnisys_user, cs_admin_pg" a nivel tabla
-- (Postgres no gradua permisos por columna salvo que se pida explicito).
