USE [CarniSys]
GO

-- Item 6 (2026-09-11, ver docs/DECISIONS.md "esquema Empresas: EmpresaPropia/EsCarniceria"):
-- 2 flags de clasificacion admin (no autoservicio, no aparecen en "Mi Empresa") -- default
-- false en todas las filas, true solo para la carniceria propia real (CUIT 20306210786,
-- confirmado con el usuario). Mismo patron guardado que 20260814-Alter_Empresas_HorarioLogin.sql.

IF COL_LENGTH('dbo.Empresas', 'EmpresaPropia') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD EmpresaPropia BIT NOT NULL CONSTRAINT DF_Empresas_EmpresaPropia DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Empresas', 'EsCarniceria') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD EsCarniceria BIT NOT NULL CONSTRAINT DF_Empresas_EsCarniceria DEFAULT (0);
END
GO

UPDATE dbo.Empresas
SET EmpresaPropia = 1, EsCarniceria = 1
WHERE Cuit = 20306210786;
GO
