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
CREATE PROCEDURE ventasVendedorCierreCaja
	-- Add the parameters for the stored procedure here
	@idVendedor int,
	@fechaDesde datetime,
	@texto nvarchar(50) = '',
	@idSucursal int,
	@soloAnulados tinyint = 0
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
	WHERE fechaVenta >= @fechaDesde and dbo.Ventas.idSucursal = @idSucursal and dbo.Ventas.idVendedor = @idVendedor
	and(nroRemito like '%'+@texto+'%' or  Personas.razonSocial like '%'+@texto+'%') and
	(@soloAnulados=0 or (@soloAnulados=1 and dbo.LineaVenta.cantKg < 0 ))

	GROUP BY dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Ventas.idVendedor, dbo.Usuarios.nombre, dbo.Ventas.nroRemito, dbo.Ventas.idPersona, dbo.Personas.razonSocial, dbo.Ventas.idSucursal, dbo.Sucursal.sucursal, 
				  dbo.Ventas.turno, dbo.Ventas.diaFestivo, dbo.Ventas.observaciones, dbo.Ventas.creado, dbo.Ventas.actualizado, dbo.Ventas.estado
	order by fechaVenta desc
	
END
GO
