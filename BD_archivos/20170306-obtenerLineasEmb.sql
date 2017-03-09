USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[obtenerLineasEmb]    Script Date: 03/06/2017 19:09:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasEmb] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idSucursal int = 0,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT     dbo.Embutidos.idEmbutido as 'Id', dbo.Embutidos.fechaEmbutido as 'Fecha',CorteEmbutido.codigo as 'Cod.Emb', CorteEmbutido.corte as 'Embutido', dbo.Corte.codigo AS 'Codigo', dbo.Corte.corte AS 'Corte', 
                      dbo.CortePorEmbutido.kgUtilizados as 'Kgs', dbo.CortePorEmbutido.pesoBalanza as 'Balanza', dbo.Sucursal.sucursal as 'Sucursal', dbo.Embutidos.estado  as 'Estado', 'Observaciones' = case  
                      when LEN(dbo.Embutidos.observaciones) <= 20 then dbo.Embutidos.observaciones
                      else (SUBSTRING(dbo.Embutidos.observaciones, 1, 20) + '...') end ,                       
                      dbo.Embutidos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
                      dbo.Embutidos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
FROM          dbo.Corte INNER JOIN
                      dbo.CortePorEmbutido ON dbo.Corte.idCorte = dbo.CortePorEmbutido.idCorte INNER JOIN
                      dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
                      dbo.Corte AS CorteEmbutido ON dbo.Embutidos.idCorte = CorteEmbutido.idCorte INNER JOIN
                      dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Embutidos.creadoPor = CreadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Embutidos.actualizadoPor = ActualizadoPor.id
WHERE fechaEmbutido between @fechaDesde and @fechaHasta 
and 
(
(@idSucursal > 0 and dbo.Sucursal.idSucursal = @idSucursal) or 
dbo.Embutidos.idSucursal > 0
) 
and ((CAST(dbo.Embutidos.idEmbutido as nvarchar(50)) = @texto )or (CAST( CorteEmbutido.codigo as nvarchar(50)) = @texto )or(CorteEmbutido.corte like '%'+@texto+'%' ) or (CAST( dbo.Corte.codigo as nvarchar(50)) = @texto )or(dbo.Corte.corte like '%'+@texto+'%' ) or(CreadoPor.nombre like '%'+@texto+'%' ) or (ActualizadoPor.nombre like '%'+@texto+'%' ))                 
ORDER BY fechaEmbutido DESC   

END

GO


