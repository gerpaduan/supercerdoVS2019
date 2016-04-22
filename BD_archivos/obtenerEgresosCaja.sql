USE [SuperCerdo]
GO
/****** Object:  StoredProcedure [dbo].[obtenerEgresosCaja]    Script Date: 04/22/2016 10:58:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[obtenerEgresosCaja]
	-- Add the parameters for the stored procedure here
	@id int = 0,
	@idTipoEgresoCaja int = 0,
	@idSucursal int = 0,
	@fechaDesde datetime = null,
	@fechaHasta datetime = null,
	--@fechaActual datetime = CONVERT(date, SYSDATETIME()),
	@idVendedor int = 0,
	@texto nvarchar(50) = '',
	@montoEgresoCaja tinyint = 0,
	@verEgresoCaja tinyint = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Se obtiene todos los EgresosCaja asociados al Cajero entre la Apertura y Cierre de caja
	IF @montoEgresoCaja = 1	
		BEGIN
			SELECT ROUND(sum(dbo.EgresosCaja.monto), 2) as monto
			FROM      dbo.EgresosCaja 
			WHERE dbo.EgresosCaja.fechaHora between @fechaDesde and SYSDATETIME() and EgresosCaja.idSucursal = @idSucursal and					dbo.EgresosCaja.creadoPor = @idVendedor
		END
	ELSE
		BEGIN
			IF @id > 0
				BEGIN
					SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora, dbo.EgresosCaja.idTipoEgresoCaja, dbo.TiposEgresoCaja.tipoEgresoCaja, dbo.EgresosCaja.descripcion, 
							dbo.EgresosCaja.detalle as detalle,  
							ROUND(dbo.EgresosCaja.monto, 2) as monto, dbo.EgresosCaja.idSucursal, dbo.EgresosCaja.creado, 
							  dbo.EgresosCaja.creadoPor, dbo.EgresosCaja.actualizado, dbo.EgresosCaja.actualizadoPor
					FROM      dbo.EgresosCaja INNER JOIN dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id
					WHERE dbo.EgresosCaja.id = @id
				END
			ELSE
				BEGIN
					IF @idTipoEgresoCaja > 0
						BEGIN
							SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora as 'Fecha', dbo.TiposEgresoCaja.tipoEgresoCaja as 'Tipo EgresoCaja', 
							dbo.EgresosCaja.descripcion as 'Descripción', dbo.EgresosCaja.detalle as 'Detalle', 
							ROUND(dbo.EgresosCaja.monto, 2) as Monto, dbo.EgresosCaja.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
							dbo.EgresosCaja.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
							FROM      dbo.EgresosCaja INNER JOIN
									  dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id LEFT OUTER JOIN
									  dbo.Usuarios AS CreadoPor ON dbo.EgresosCaja.creadoPor = CreadoPor.id LEFT OUTER JOIN
									  dbo.Usuarios AS ActualizadoPor ON dbo.EgresosCaja.actualizadoPor = ActualizadoPor.id	
							WHERE  dbo.EgresosCaja.fechaHora between @fechaDesde and 
									@fechaHasta and dbo.EgresosCaja.idTipoEgresoCaja = @idTipoEgresoCaja and 
									EgresosCaja.idSucursal = @idSucursal and ( dbo.EgresosCaja.descripcion like '%'+@texto+'%' 
										or	ActualizadoPor.nombre like '%'+@texto+'%')
							ORDER BY dbo.EgresosCaja.fechaHora DESC
						END
					ELSE
						BEGIN
							IF @verEgresoCaja = 1
								SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora as 'Fecha', dbo.TiposEgresoCaja.tipoEgresoCaja as 'Tipo EgresoCaja', 
									dbo.EgresosCaja.descripcion as 'Descripción', dbo.EgresosCaja.detalle as 'Detalle', 
									ROUND(dbo.EgresosCaja.monto, 2) as Monto, dbo.EgresosCaja.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.EgresosCaja.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
								FROM      dbo.EgresosCaja INNER JOIN
										  dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id LEFT OUTER JOIN
										  dbo.Usuarios AS CreadoPor ON dbo.EgresosCaja.creadoPor = CreadoPor.id LEFT OUTER JOIN
										  dbo.Usuarios AS ActualizadoPor ON dbo.EgresosCaja.actualizadoPor = ActualizadoPor.id	
								WHERE dbo.EgresosCaja.fechaHora between @fechaDesde and SYSDATETIME() and EgresosCaja.idSucursal = @idSucursal and 
										dbo.EgresosCaja.creadoPor = @idVendedor
								ORDER BY dbo.EgresosCaja.fechaHora DESC
							ELSE
								SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora as 'Fecha', dbo.TiposEgresoCaja.tipoEgresoCaja as 'Tipo EgresoCaja', 
									dbo.EgresosCaja.descripcion as 'Descripción', dbo.EgresosCaja.detalle as 'Detalle', 
									ROUND(dbo.EgresosCaja.monto, 2) as Monto, dbo.EgresosCaja.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.EgresosCaja.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
								FROM      dbo.EgresosCaja INNER JOIN
										  dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id LEFT OUTER JOIN
										  dbo.Usuarios AS CreadoPor ON dbo.EgresosCaja.creadoPor = CreadoPor.id LEFT OUTER JOIN
										  dbo.Usuarios AS ActualizadoPor ON dbo.EgresosCaja.actualizadoPor = ActualizadoPor.id	
								WHERE dbo.EgresosCaja.fechaHora between @fechaDesde and @fechaHasta and 
										EgresosCaja.idSucursal = @idSucursal and	( dbo.EgresosCaja.descripcion like '%'+@texto+'%' 
										or	ActualizadoPor.nombre like '%'+@texto+'%')
								ORDER BY dbo.EgresosCaja.fechaHora DESC
						END
				END
		END
    
END

