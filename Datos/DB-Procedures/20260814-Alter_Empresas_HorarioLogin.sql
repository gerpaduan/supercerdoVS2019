USE [CarniSys]
GO

-- Horario laboral por empresa: 2 jornadas diarias (diurno/tarde) que acotan cuando los
-- empleados no-admin pueden loguearse (ver LoginController.EstaDentroDelHorarioPermitido).
-- Default 00:00:00-23:59:59 en ambas jornadas = sin restriccion real hasta que el admin
-- de la empresa las acote desde la pantalla "Mi Empresa" -- asi el deploy no bloquea a
-- nadie por accidente.

IF COL_LENGTH('dbo.Empresas', 'HorarioDiurnoDesde') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD HorarioDiurnoDesde TIME(0) NOT NULL CONSTRAINT DF_Empresas_HorarioDiurnoDesde DEFAULT ('00:00:00');
END
GO

IF COL_LENGTH('dbo.Empresas', 'HorarioDiurnoHasta') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD HorarioDiurnoHasta TIME(0) NOT NULL CONSTRAINT DF_Empresas_HorarioDiurnoHasta DEFAULT ('23:59:59');
END
GO

IF COL_LENGTH('dbo.Empresas', 'HorarioTardeDesde') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD HorarioTardeDesde TIME(0) NOT NULL CONSTRAINT DF_Empresas_HorarioTardeDesde DEFAULT ('00:00:00');
END
GO

IF COL_LENGTH('dbo.Empresas', 'HorarioTardeHasta') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD HorarioTardeHasta TIME(0) NOT NULL CONSTRAINT DF_Empresas_HorarioTardeHasta DEFAULT ('23:59:59');
END
GO
