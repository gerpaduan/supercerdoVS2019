-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE getAllLineasVenta
	-- Add the parameters for the stored procedure here
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50),
	@idSucursal int = -1,
	@idVendedor int = -1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT     dbo.Usuarios.nombre, dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Sucursal.sucursal, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, (dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) as totalCorte, dbo.LineaVenta.pesoBalanza, dbo.LineaVenta.idAnulado
	FROM         dbo.Ventas INNER JOIN
						  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
						  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
						  dbo.Usuarios ON dbo.Ventas.idVendedor = dbo.Usuarios.id INNER JOIN
						  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal
	WHERE dbo.Ventas.fechaVenta between @fechaDesde and @fechaHasta and ((@idSucursal < 0 and dbo.Ventas.idSucursal >= 0) 
			or (@idSucursal >= 0 and 
			dbo.Ventas.idSucursal = @idSucursal))
			and ((@idVendedor < 0 and dbo.Ventas.idVendedor >= 0) or (@idVendedor >= 0 and 
			dbo.Ventas.idVendedor = @idVendedor)) and (dbo.Corte.codigo like '%'+@texto+'%' or dbo.Corte.corte like '%'+@texto+'%')
	order by dbo.Ventas.fechaVenta		
		
END
GO
