USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[obtenerLineasVenta]    Script Date: 04/08/2015 18:55:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[obtenerLineasVenta] 
	-- Add the parameters for the stored procedure here
	@idVenta int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	
	
	SELECT     dbo.LineaVenta.idVenta, dbo.LineaVenta.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, 
                      dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalS, dbo.LineaVenta.pesoBalanza, 
                'estado' = 
				 CASE
					  WHEN dbo.LineaVenta.idAnulado=0 THEN ''
					  ELSE 'Anulado'
				 END
               
	FROM         dbo.LineaVenta INNER JOIN
                      dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte
    WHERE LineaVenta.idVenta=@idVenta
    order by dbo.Corte.codigo
	
 --   -- Insert statements for procedure here
	--SELECT     dbo.LineaVenta.idVenta, dbo.LineaVenta.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, 
 --                     dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalS, dbo.LineaVenta.idAnulado AS estado
	--FROM         dbo.LineaVenta INNER JOIN
 --                     dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte
 --   WHERE LineaVenta.idVenta=@idVenta
END

GO

