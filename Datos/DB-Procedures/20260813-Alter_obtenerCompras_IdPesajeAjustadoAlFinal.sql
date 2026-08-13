USE [CarniSys]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Fix: mueve dbo.Compras.idPesajeAjustado al FINAL de cada uno de los 14 SELECT/GROUP BY de
-- obtenerCompras (en vez de justo despues de idCompra, como quedo en
-- 20260813-Alter_obtenerCompras_IdPesajeAjustado.sql). Esa version insertaba la columna nueva en
-- la posicion 1, lo que corrio un lugar el ordinal de todas las columnas siguientes.
-- Presentacion/Stock/formStock.cs:116 (WinForms) lee esa misma tabla por indice ordinal
-- (fila[6], antes = idSucursal) en vez de por nombre, y con el corrimiento paso a leer
-- tipoCompra (texto) donde esperaba un numero -> float.Parse tira "La cadena de entrada no
-- tiene el formato correcto" al abrir /Stock desde WinForms. Web lee esta misma tabla por
-- NOMBRE de columna (Web/Controllers/StockController.cs, row["idPesajeAjustado"]), asi que
-- el orden no le afecta y este fix no le cambia nada. Resto del cuerpo sin cambios.
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
						  SUM(dbo.MediaRes.kgMedia) AS cantKg, SUM(dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia) AS totalS, Compras.cantMedias, dbo.Compras.estado,
						  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Ingreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Ingreso Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1

			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Egreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Egreso Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1

			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cierre Stock' and (@tipoCompra like '' or @tipoCompra like 'Cierre Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, (SUBSTRING(dbo.Personas.razonSocial,0,5) + ' | ' + (CONVERT(VARCHAR(10),dbo.Compras.kgsMedias) + ' Kgs') + ' | ' + (CONVERT(VARCHAR(10),dbo.Compras.cantMedias) + ' Medias') + CHAR(13) + CHAR(10) + dbo.Compras.observaciones) as observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Pesaje Cortes' and (@tipoCompra like '' or @tipoCompra like 'Pesaje Cortes' or @tipoCompra like 'Ver Todos') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1

			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.kgsMedias, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado,
					  (CASE WHEN LEN(dbo.Compras.nroRemito) > 0
								THEN 'ID Pesaje:' + dbo.Compras.nroRemito + CHAR(13) + CHAR(10) + dbo.Compras.observaciones
							ELSE dbo.Compras.observaciones end	) AS observaciones,
						 dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM      dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE
			dbo.Compras.tipoCompra like 'Ajuste Stock' and (@tipoCompra like '' or @tipoCompra like 'Ajuste Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
						GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado)
			order by dbo.Compras.fechaCompra desc, dbo.Compras.idCompra desc, dbo.Compras.creado desc

		END
	ELSE
		BEGIN

			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
						  SUM(dbo.MediaRes.kgMedia) AS cantKg, SUM(dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia) AS totalS, Compras.cantMedias, dbo.Compras.estado,
						  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos' ) and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos' ) and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Ingreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Ingreso Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Egreso Stock' and (@tipoCompra like '' or @tipoCompra like 'Egreso Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cierre Stock' and (@tipoCompra like '' or @tipoCompra like 'Cierre Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, (SUBSTRING(dbo.Personas.razonSocial,0,5) + ' | ' + (CONVERT(VARCHAR(10),dbo.Compras.kgsMedias) + ' Kgs') + ' | ' + (CONVERT(VARCHAR(10),dbo.Compras.cantMedias) + ' Medias') + CHAR(13) + CHAR(10) + dbo.Compras.observaciones) as observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Pesaje Cortes' and (@tipoCompra like '' or @tipoCompra like 'Pesaje Cortes' or @tipoCompra like 'Ver Todos') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.kgsMedias, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado,
					  (CASE WHEN LEN(dbo.Compras.nroRemito) > 0
								THEN 'ID Pesaje' + dbo.Compras.nroRemito + CHAR(13) + CHAR(10) + dbo.Compras.observaciones
							ELSE dbo.Compras.observaciones end	) AS observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor, dbo.Compras.idPesajeAjustado
			FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE
			dbo.Compras.tipoCompra like 'Ajuste Stock' and (@tipoCompra like '' or @tipoCompra like 'Ajuste Stock' or @tipoCompra like 'Ver Todos') and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1
					GROUP BY  dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado, dbo.Compras.idPesajeAjustado)
			order by dbo.Compras.fechaCompra desc, dbo.Compras.idCompra desc, dbo.Compras.creado desc

		END

END
GO
