-- Item 6 (2026-09-11, ver docs/DECISIONS.md "esquema Empresas: EmpresaPropia/EsCarniceria"):
-- 2 flags de clasificacion admin (no autoservicio, no aparecen en "Mi Empresa") -- default
-- false en todas las filas, true solo para la carniceria propia real (CUIT 20306210786,
-- confirmado con el usuario). Mismo patron guardado que
-- 20260825-Alter_empresas_sucursal_add_observaciones_creado.sql.

ALTER TABLE empresas
    ADD COLUMN IF NOT EXISTS empresa_propia boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS es_carniceria boolean NOT NULL DEFAULT false;

UPDATE empresas
SET empresa_propia = true, es_carniceria = true
WHERE cuit = 20306210786;

-- Sin GRANT nuevo: las columnas se agregan a una tabla que ya tiene
-- "GRANT SELECT, INSERT, UPDATE, DELETE ... TO carnisys_user, cs_admin_pg" a nivel tabla.
