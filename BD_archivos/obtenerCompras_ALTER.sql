USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[obtenerCompras]    Script Date: 04/21/2015 17:23:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[obtenerCompras] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
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
			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
							  SUM(dbo.MediaRes.kgMedia) AS cantKg, SUM(dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia) AS totalS, dbo.Compras.estado, 
							  dbo.Compras.observaciones
			FROM         dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos'  or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos'  or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Ingreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Ingreso Stock' or @tipoCompra like 'Ingreso, Egreso y Cierre' or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Egreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Egreso Stock' or @tipoCompra like 'Ingreso, Egreso y Cierre' or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Cierre Stock' and (@tipoCompra like '' or @tipoCompra like 'Cierre Stock' or @tipoCompra like 'Ingreso, Egreso y Cierre' or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, dbo.Compras.estado, 
								  dbo.Compras.observaciones)
			order by dbo.Compras.fechaCompra desc
			
		END
	ELSE
		BEGIN
				
			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, 
							  SUM(dbo.MediaRes.kgMedia) AS cantKg, SUM(dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia) AS totalS, dbo.Compras.estado, 
							  dbo.Compras.observaciones
			FROM         dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos'  or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos'  or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Ingreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Ingreso Stock' or @tipoCompra like 'Ingreso, Egreso y Cierre' or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Egreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Egreso Stock' or @tipoCompra like 'Ingreso, Egreso y Cierre' or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,dbo.Compras.estado, 
								  dbo.Compras.observaciones
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, dbo.Compras.estado, dbo.Compras.observaciones
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal
			WHERE dbo.Compras.tipoCompra like 'Cierre Stock' and (@tipoCompra like '' or @tipoCompra like 'Cierre Stock' or @tipoCompra like 'Ingreso, Egreso y Cierre' or @tipoCompra like 'Todas c/Ingreso Stock') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, dbo.Compras.estado, 
								  dbo.Compras.observaciones)
			order by dbo.Compras.fechaCompra desc
			
		END

	
END

GO

