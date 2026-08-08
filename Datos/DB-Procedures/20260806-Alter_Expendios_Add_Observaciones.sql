USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- Agrega la columna de observaciones a dbo.Expendios y el parametro nuevo a
-- agregarExpendio para que la observacion cargada en Punto de Expendio (boton
-- "Comentario") deje de perderse -- antes, PuntosExpendioController.FinalizarPOS
-- pisaba el valor con "" antes de insertar, y el SP no tenia donde guardarlo.
-- Ver docs/09-cambios-y-pendientes/bitacora-de-cambios.md, 2026-08-06 (ronda 8).
-- ============================================================================

IF COL_LENGTH('dbo.Expendios', 'observaciones') IS NULL
BEGIN
    ALTER TABLE dbo.Expendios
        ADD observaciones NVARCHAR(MAX) NULL;
END
GO

ALTER PROCEDURE [dbo].[agregarExpendio]
	@idExpendio int=0,
	@idVendedor int=0,
	@fechaExpendio datetime=null,
	@idSucursal int=null,
	@identificacionExpendio nvarchar(50)=null,
	@sector nvarchar(MAX)=null,
	@cantItems int=0,
	@importe float=0,
	@serialCPU nvarchar(MAX)=null,
	@observaciones nvarchar(MAX)=null
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Licencias SET sector = @sector WHERE nroLicencia = @serialCPU

	insert into Expendios(idVendedor, fechaExpendio, idSucursal, identificacionExpendio, sector, cantItems, importe, creado, observaciones)
	values (@idVendedor, @fechaExpendio, @idSucursal, @identificacionExpendio, @sector, @cantItems, @importe, SYSDATETIME(), @observaciones)

	select top 1 Expendios.idExpendio from Expendios where Expendios.idSucursal = @idSucursal order by Expendios.idExpendio desc
END
GO
