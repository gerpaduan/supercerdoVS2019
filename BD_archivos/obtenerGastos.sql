USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[obtenerGastos]    Script Date: 02/09/2016 12:07:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obtenerGastos]
	-- Add the parameters for the stored procedure here
	@id int = 0,
	@idTipoGasto int = 0,
	@idSucursal int = 0,
	@fechaDesde datetime = null,
	@fechaHasta datetime = null,
	--@fechaActual datetime = CONVERT(date, SYSDATETIME()),
	@idVendedor int = 0,
	@texto nvarchar(50) = '',
	@montoGasto tinyint = 0,
	@verGasto tinyint = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Se obtiene todos los gastos asociados al Cajero entre la Apertura y Cierre de caja
	IF @montoGasto = 1	
		BEGIN
			SELECT ROUND(sum(dbo.Gastos.monto), 2) as monto
			FROM      dbo.Gastos 
			WHERE dbo.Gastos.fechaHora between @fechaDesde and SYSDATETIME() and Gastos.idSucursal = @idSucursal and					dbo.Gastos.creadoPor = @idVendedor
		END
	ELSE
		BEGIN
			IF @id > 0
				BEGIN
					SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora, dbo.Gastos.idTipoGasto, dbo.TipoGasto.tipoGasto, dbo.Gastos.descripcion, 
							substring(dbo.Gastos.detalle, 1, 30)+'...' as detalle,  
							ROUND(dbo.Gastos.monto, 2) as monto, dbo.Gastos.idSucursal, dbo.Gastos.creado, 
							  dbo.Gastos.creadoPor, dbo.Gastos.actualizado, dbo.Gastos.actualizadoPor
					FROM      dbo.Gastos INNER JOIN dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id
					WHERE dbo.Gastos.id = @id
				END
			ELSE
				BEGIN
					SET @fechaDesde = CONVERT(date, @fechaDesde)			
					SET @fechaHasta = CONVERT(date, @fechaHasta)
					IF @idTipoGasto > 0
						BEGIN
							SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora as 'Fecha', dbo.TipoGasto.tipoGasto as 'Tipo Gasto', 
							dbo.Gastos.descripcion as 'Descripción', substring(dbo.Gastos.detalle, 1, 30) as 'Detalle', 
							ROUND(dbo.Gastos.monto, 2) as Monto, dbo.Gastos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
							dbo.Gastos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
							FROM      dbo.Gastos INNER JOIN
									  dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id LEFT OUTER JOIN
									  dbo.Usuarios AS CreadoPor ON dbo.Gastos.creadoPor = CreadoPor.id LEFT OUTER JOIN
									  dbo.Usuarios AS ActualizadoPor ON dbo.Gastos.actualizadoPor = ActualizadoPor.id	
							WHERE  CONVERT(date, dbo.Gastos.fechaHora) between @fechaDesde and 
									@fechaHasta and dbo.Gastos.idTipoGasto = @idTipoGasto and 
									Gastos.idSucursal = @idSucursal and ( dbo.Gastos.descripcion like '%'+@texto+'%' 
										or	ActualizadoPor.nombre like '%'+@texto+'%')
							ORDER BY dbo.Gastos.fechaHora DESC
						END
					ELSE
						BEGIN
							IF @verGasto = 1
								SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora as 'Fecha', dbo.TipoGasto.tipoGasto as 'Tipo Gasto', 
									dbo.Gastos.descripcion as 'Descripción', substring(dbo.Gastos.detalle, 1, 30) as 'Detalle', 
									ROUND(dbo.Gastos.monto, 2) as Monto, dbo.Gastos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.Gastos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
								FROM      dbo.Gastos INNER JOIN
										  dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id LEFT OUTER JOIN
										  dbo.Usuarios AS CreadoPor ON dbo.Gastos.creadoPor = CreadoPor.id LEFT OUTER JOIN
										  dbo.Usuarios AS ActualizadoPor ON dbo.Gastos.actualizadoPor = ActualizadoPor.id	
								WHERE dbo.Gastos.fechaHora between @fechaDesde and SYSDATETIME() and Gastos.idSucursal = @idSucursal and 
										dbo.Gastos.creadoPor = @idVendedor
								ORDER BY dbo.Gastos.fechaHora DESC
							ELSE
								SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora as 'Fecha', dbo.TipoGasto.tipoGasto as 'Tipo Gasto', 
									dbo.Gastos.descripcion as 'Descripción', substring(dbo.Gastos.detalle, 1, 30) as 'Detalle', 
									ROUND(dbo.Gastos.monto, 2) as Monto, dbo.Gastos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.Gastos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
								FROM      dbo.Gastos INNER JOIN
										  dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id LEFT OUTER JOIN
										  dbo.Usuarios AS CreadoPor ON dbo.Gastos.creadoPor = CreadoPor.id LEFT OUTER JOIN
										  dbo.Usuarios AS ActualizadoPor ON dbo.Gastos.actualizadoPor = ActualizadoPor.id	
								WHERE CONVERT(date, dbo.Gastos.fechaHora) between @fechaDesde and @fechaHasta and 
										Gastos.idSucursal = @idSucursal and	( dbo.Gastos.descripcion like '%'+@texto+'%' 
										or	ActualizadoPor.nombre like '%'+@texto+'%')
								ORDER BY dbo.Gastos.fechaHora DESC
						END
				END
		END
    
END

GO

