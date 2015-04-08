USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[obtenerVentas]    Script Date: 04/08/2015 18:56:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[obtenerVentas] 
	-- Add the parameters for the stored procedure here
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50),
	@sucursal nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Ventas.idVendedor, dbo.Usuarios.nombre, dbo.Ventas.nroRemito, dbo.Ventas.idPersona, dbo.Personas.razonSocial, dbo.Ventas.idSucursal, dbo.Sucursal.sucursal, 
                      dbo.Ventas.turno, dbo.Ventas.diaFestivo, dbo.Ventas.observaciones, dbo.Ventas.creado, dbo.Ventas.actualizado,
                      dbo.Ventas.estado,SUM(dbo.LineaVenta.cantKg) as totalKg, SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) AS totalS
	FROM         dbo.LineaVenta INNER JOIN
                      dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
                      dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona INNER JOIN
                      dbo.Usuarios ON dbo.Ventas.idVendedor = dbo.Usuarios.id
	WHERE fechaVenta between @fechaDesde and @fechaHasta+1 and sucursal like '%'+@sucursal+'%' 
		and( Usuarios.nombre like '%'+@texto+'%' or  nroRemito like '%'+@texto+'%' or  Personas.razonSocial like '%'+@texto+'%'
			 or Ventas.diaFestivo like '%'+@texto+'%' or (DATENAME(WEEKDAY, dbo.Ventas.fechaVenta) LIKE '%' + @texto + '%'))
	
	GROUP BY dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Ventas.idVendedor, dbo.Usuarios.nombre, dbo.Ventas.nroRemito, dbo.Ventas.idPersona, dbo.Personas.razonSocial, dbo.Ventas.idSucursal, dbo.Sucursal.sucursal, 
                      dbo.Ventas.turno, dbo.Ventas.diaFestivo, dbo.Ventas.observaciones, dbo.Ventas.creado, dbo.Ventas.actualizado, dbo.Ventas.estado
	order by fechaVenta desc
	
END

GO

