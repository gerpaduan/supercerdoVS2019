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
CREATE PROCEDURE ultimasVentasCliente
	-- Add the parameters for the stored procedure here	
	@idSucursal int = -1,
	@idPersona int = -1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    	SELECT     dbo.Usuarios.nombre as 'vendedor', dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Personas.razonSocial, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, 
						  dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalCorte, dbo.LineaVenta.bonificacion, dbo.LineaVenta.pesoBalanza, dbo.LineaVenta.idAnulado, dbo.Sucursal.sucursal
	FROM         dbo.Ventas INNER JOIN
                      dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
                      dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
                      dbo.Usuarios ON dbo.Ventas.idVendedor = dbo.Usuarios.id INNER JOIN
                      dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona
	WHERE dbo.Ventas.idPersona = @idPersona and dbo.Ventas.idSucursal = @idSucursal
			and dbo.Ventas.idVenta IN (
				SELECT  TOP 5  Vta.idVenta
						FROM         dbo.Ventas as Vta INNER JOIN dbo.Sucursal as Suc ON 
									Vta.idSucursal = Suc.idSucursal INNER JOIN
									 dbo.Personas as Pers ON Vta.idPersona = Pers.idPersona
						WHERE     (Vta.idPersona = @idPersona) AND (Vta.idSucursal = @idSucursal))
	order by dbo.Ventas.fechaVenta desc
END
GO
