USE [SuperCerdo]
GO

-- Item 6 (2026-09-11/12, ver docs/DECISIONS.md "esquema Empresas: EmpresaPropia/EsCarniceria"):
-- version para el servidor remoto "ServidorSM" (ver ~/hosts/servidorsm.env) -- base real
-- "SuperCerdo", NO "CarniSys" (nombre distinto al local/SanLorenzo). Confirmado en vivo antes de
-- ejecutar: columnas ausentes, una sola fila en dbo.Empresas (CUIT 20306210786, coincide con el
-- CUIT objetivo), y dbo.AA_AltaEmpresa NO EXISTE en este servidor (OBJECT_ID devuelve NULL) --
-- por eso este script no incluye ningun ALTER PROCEDURE, a diferencia de la version local.

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
