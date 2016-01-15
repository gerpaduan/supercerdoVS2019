USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[obtenerTotalVentas]    Script Date: 01/13/2016 22:46:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obtenerTotalVentas]
	-- Add the parameters for the stored procedure here
	@idVendedor int,
	@idSucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT     SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) AS totalS
	FROM         dbo.LineaVenta INNER JOIN
                      dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
                      dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal 
	WHERE fechaVenta between @fechaDesde and @fechaHasta and Ventas.idVendedor = @idVendedor and Ventas.idSucursal = @idSucursal
END
GO

