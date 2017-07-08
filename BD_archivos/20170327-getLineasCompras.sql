USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[getLineasCompras]    Script Date: 03/27/2017 19:54:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[getLineasCompras] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@codigo nvarchar(50),
	@corte nvarchar(50),
	@fechaDesde datetime,
	@fechaHasta datetime,
	@tipoCompra nvarchar(50),
	@idSucursal int = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    IF @idSucursal > 0
		BEGIN				
			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, '-' AS codigo, 'Media Res' AS corte, dbo.MediaRes.kgMedia AS cantKg, dbo.MediaRes.precioMedia AS precioKg, 
                      dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia AS totalS, dbo.Compras.estado, dbo.Compras.idSucursal, 
                      dbo.Sucursal.sucursal, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor,
                       dbo.Compras.actualizado, ActualizadoPor.nombre AS actualizadoPor
FROM         dbo.Compras INNER JOIN
                      dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorCompra.cantKg AS cantKg, dbo.CortePorCompra.precioKg, 
                      dbo.CortePorCompra.precioKg * dbo.CortePorCompra.cantKg AS totalS, dbo.Compras.estado, dbo.Compras.idSucursal, 
                      dbo.Sucursal.sucursal, dbo.Compras.observaciones, dbo.Compras.creado, 
                      CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado, ActualizadoPor.nombre AS actualizadoPor
FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or (dbo.Compras.nroRemito like '%'+@texto+'%' ) )  and (cast(dbo.Corte.codigo as nvarchar(50)) like '%'+@codigo+'%') and (dbo.Corte.corte like '%'+@corte+'%'))
			order by dbo.Compras.fechaCompra desc
			
		END
	ELSE
		BEGIN
				
			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, '-' AS codigo, 'Media Res' AS corte, dbo.MediaRes.kgMedia AS cantKg, dbo.MediaRes.precioMedia AS precioKg, 
                      dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia AS totalS, dbo.Compras.estado, dbo.Compras.idSucursal, 
                      dbo.Sucursal.sucursal, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor,
                       dbo.Compras.actualizado, ActualizadoPor.nombre AS actualizadoPor
FROM         dbo.Compras INNER JOIN
                      dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos' ) and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorCompra.cantKg AS cantKg, dbo.CortePorCompra.precioKg, 
                      dbo.CortePorCompra.precioKg * dbo.CortePorCompra.cantKg AS totalS, dbo.Compras.estado, dbo.Compras.idSucursal, 
                      dbo.Sucursal.sucursal, dbo.Compras.observaciones, dbo.Compras.creado, 
                      CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado, ActualizadoPor.nombre AS actualizadoPor
FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos' ) and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or (dbo.Compras.nroRemito like '%'+@texto+'%' ) ) and (cast(dbo.Corte.codigo as nvarchar(50)) like '%'+@codigo+'%') and (dbo.Corte.corte like '%'+@corte+'%'))
			order by dbo.Compras.fechaCompra desc
			
		END

END


GO


