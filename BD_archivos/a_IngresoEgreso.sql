USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[a_IngresoEgreso]    Script Date: 05/01/2015 21:39:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[a_IngresoEgreso]
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
    select cast(IngresoStock.codigo as NCHAR(5)) as Codigo ,IngresoStock.corte as 'Corte', IngresoCompras.StockIngreso as 'Compras', IngresoEmbutido.StockIngreso as 'Ingr.Emb',IngresoStock.StockIngreso as 'Ingr.Stock', IngresoMovimiento.StockIngreso as 'Ingr. Mov.',(IngresoCompras.StockIngreso + IngresoEmbutido.StockIngreso + IngresoStock.StockIngreso + IngresoMovimiento.StockIngreso) as 'Tot.INGR' ,EgresoStock.StockIngreso as 'Egr.Stock', EgresoMovimiento.StockIngreso as 'Egr.Mov.', EgresoPorEmbutido.TotalEnEmbutidos as 'Egr.Emb', EgresoVentas.TotalVenta as 'Ventas', (EgresoStock.StockIngreso +  EgresoMovimiento.StockIngreso + EgresoPorEmbutido.TotalEnEmbutidos + EgresoVentas.TotalVenta) as 'Tot.EGR', ((IngresoCompras.StockIngreso + IngresoEmbutido.StockIngreso + IngresoStock.StockIngreso + IngresoMovimiento.StockIngreso) - (EgresoStock.StockIngreso +  EgresoMovimiento.StockIngreso + EgresoPorEmbutido.TotalEnEmbutidos + EgresoVentas.TotalVenta)) as 'DIF'
    	
   -- 	Ingreso.sucursal as 'Sucursal',
			--Ingreso.StockIngreso as 'Total Ingresado', Embutido.TotalEnEmbutidos as 'Kgs en Embutidos', 
			--Egreso.TotalVenta as 'Total Vendido',(Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) as 'Stock Teorico',
			-- CierreStock.StockCierre as 'Stock Real', 
			-- ((Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) - CierreStock.StockCierre ) as 'Faltante'
	from
		(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION

			--Stock Ingreso

			--++ Cortes ingresados
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteP ON dbo.CortePorCompra.idCorte = CorteP.idCorte
			WHERE     (CorteP.independiente = 1) AND (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta+1) AND (dbo.CortePorCompra.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)

			union
			--++ Suma de los cortes ingresados a su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal ON CorteP.idCorte = dbo.CortePorCompra.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteM.codigo > 0) AND (CorteM.independiente = 1)
			GROUP BY CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.Sucursal.sucursal, dbo.CortePorCompra.idSucursal)

			union
			--++ Suma de los cortes ingresados al CorteM de su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM((dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) 
								  + ((dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * 
								  dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) * CorteP.porcentajeHueso / CorteP.porcentaje)) 
								  AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte INNER JOIN
								  dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON dbo.Corte.idCorteMaestro = CorteP.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteM.codigo > 0) AND (CorteM.independiente = 1)
			GROUP BY CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.Sucursal.sucursal, dbo.CortePorCompra.idSucursal)

			union
			--++Suma a los Sub-Cortes independientes del corte ingresado
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100) 
								  AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal ON CorteM.idCorte = dbo.CortePorCompra.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteP.independiente = 1) AND (CorteM.codigo > 0)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)
			union
			--++Suma a los cortes independientes de los subcortes del corte ingresado
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal,
				  SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100 * dbo.Corte.porcentaje / 100) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteM ON dbo.CortePorCompra.idCorte = CorteM.idCorte INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS CorteP ON dbo.Corte.idCorte = CorteP.idCorteMaestro
			WHERE     (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteP.independiente = 1) AND (CorteM.codigo > 0)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)			
		) as IngresoStock
		group by idCorte,codigo,corte,idSucursal,sucursal) AS IngresoStock
	left outer JOIN

/*Ingreso por Movimiento*/
		(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION
				
			---Ingreso Movimiento
			--++SubCorte de Media
			((SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorMovimiento.cantKg) AS StockIngreso
			FROM         dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
								  dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal
			WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal) 

			UNION
			--++SubCorte 2 de Media
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
					SUM(dbo.CortePorMovimiento.cantKg+dbo.CortePorMovimiento.cantKg*SubCorte.porcentajeHueso/SubCorte.porcentaje) AS StockIngreso
			FROM         dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS SubCorte ON dbo.CortePorMovimiento.idCorte = SubCorte.idCorte INNER JOIN
								  dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte ON SubCorte.idCorteMaestro = dbo.Corte.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (CorteM.codigo < 1)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
			union
			--++SubCorte 3 de Media
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
							SUM(((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje)
								 +((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje))
								 *SubCorte.porcentajeHueso/SubCorte.porcentaje)))) AS StockIngreso
			FROM         dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Corte AS SubCorte ON dbo.Corte.idCorte = SubCorte.idCorteMaestro INNER JOIN
								  dbo.Corte AS SubCorte2 ON SubCorte.idCorte = SubCorte2.idCorteMaestro INNER JOIN
								  dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal ON SubCorte2.idCorte = dbo.CortePorMovimiento.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (CorteM.codigo < 1)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union
			--++Cortes Independiente derivados de un Corte que deriva de Media Res 
			(SELECT     TOP (100) PERCENT dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorMovimiento.cantKg) 
								  AS StockIngreso
			FROM         dbo.Corte AS CorteM INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte AND CorteM.idCorte <> MediaRes.idCorte INNER JOIN
								  dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (MediaRes.codigo < 1)
								   AND (dbo.Corte.independiente = 1)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union
			--++Cortes Independiente no derivados de un Corte que deriva de Media Res 
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorMovimiento.cantKg) AS StockIngreso
			FROM         dbo.Corte AS CorteM INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte INNER JOIN
								  dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (MediaRes.codigo > 0)
								   AND (dbo.Corte.independiente = 1)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
			union

			--++Sub Cortes de Cortes Independiente no derivados de Media Res direcamente
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
						SUM(dbo.CortePorMovimiento.cantKg + dbo.CortePorMovimiento.cantKg*  CorteP.porcentajeHueso/ CorteP.porcentaje) AS StockIngreso
			FROM         dbo.Corte INNER JOIN
								  dbo.Corte AS CorteP ON dbo.Corte.idCorte = CorteP.idCorteMaestro INNER JOIN
								  dbo.CortePorMovimiento ON dbo.CortePorMovimiento.idCorte = CorteP.idCorte INNER JOIN
								  dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte INNER JOIN
								  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (MediaRes.codigo > 0)
								   AND (dbo.Corte.independiente = 1)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union 

			--++ Suma al sub corte independiente del corte ingresado derivado de la media res
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.CortePorMovimiento.cantKg * dbo.Corte.porcentaje / 100) AS StockIngreso
			FROM         dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte INNER JOIN
								  dbo.Corte AS CorteMedia ON CorteM.idCorteMaestro = CorteMedia.idCorte ON dbo.CortePorMovimiento.idCorte = CorteM.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND 
								  (CorteMedia.codigo < 1)   AND (dbo.Corte.independiente = 1)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			)
		) as IngresoMovimiento
		group by idCorte,codigo,corte,idSucursal,sucursal
) as IngresoMovimiento ON IngresoMovimiento.idSucursal = IngresoStock.idSucursal AND IngresoMovimiento.idCorte = IngresoStock.idCorte 
/*Fin Ingreso movimiento*/
 
 left outer join 
 
 /*Ingreso por Embutido*/
(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION
			--Embutidos
			--++ Ingreso de embutidos
			((SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorEmbutido.kgUtilizados) AS StockIngreso
			FROM         dbo.Corte INNER JOIN
					  dbo.Embutidos ON dbo.Corte.idCorte = dbo.Embutidos.idCorte INNER JOIN
					  dbo.CortePorEmbutido ON dbo.Embutidos.idEmbutido = dbo.CortePorEmbutido.idEmbutido INNER JOIN
					  dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal
			WHERE     (dbo.Embutidos.estado = '') AND (dbo.Embutidos.fechaEmbutido BETWEEN @fechaDesde AND 
					  @fechaHasta+1) AND (dbo.Embutidos.idSucursal = @idSucursal)   
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal))
			union
			((SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorEmbutido.kgUtilizados-dbo.CortePorEmbutido.kgUtilizados) AS StockIngreso
			FROM         dbo.CortePorEmbutido INNER JOIN
					  dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
					  dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
					  dbo.Corte ON dbo.CortePorEmbutido.idCorte = dbo.Corte.idCorte
			WHERE     (dbo.Embutidos.estado = '') AND (dbo.Embutidos.fechaEmbutido BETWEEN @fechaDesde AND 
						@fechaHasta+1) AND (dbo.Embutidos.idSucursal = @idSucursal)          
			GROUP BY dbo.Corte.corte, dbo.Corte.codigo,dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte))
			
		) as Embutidos
		group by idCorte,codigo,corte,idSucursal,sucursal
) as IngresoEmbutido  
on IngresoEmbutido.idSucursal=IngresoStock.idSucursal and IngresoEmbutido.idCorte=IngresoStock.idCorte

/*Fin ingreso por Embutidos*/

 left outer join 
 
 /* Ingresos por Compras*/
 (select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION
			--Compra Medias

			--++Ingresos de cortes derivados de la Media Res
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100) AS StockIngreso
			FROM         dbo.Corte AS CorteMediaRes INNER JOIN
								  dbo.Corte AS CorteP ON CorteMediaRes.idCorte = CorteP.idCorteMaestro AND CorteMediaRes.idCorte <> CorteP.idCorte CROSS JOIN
								  dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
			WHERE     (CorteMediaRes.codigo = 0) AND (dbo.Compras.estado = '') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta+1) AND (dbo.MediaRes.idSucursal = @idSucursal)
			GROUP BY CorteP.corte, CorteP.codigo,CorteP.idCorte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal)

			UNION

			--++Ingresos de corte independientes derivados de los cortes de la Media Res
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) 
								  AS StockIngreso
			FROM         dbo.Corte AS CorteM INNER JOIN
								  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro INNER JOIN
								  dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte CROSS JOIN
								  dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
			WHERE     (dbo.Compras.estado = '') AND (CorteP.independiente = 1) AND (CorteMediaRes.codigo = 0) AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta + 1) AND (dbo.MediaRes.idSucursal = @idSucursal)
			GROUP BY CorteP.corte, CorteP.codigo,CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			--++Ingresos de corte independientes derivados de los Subcortes de los cortes de la Media Res
			union
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.MediaRes.kgMedia * CorteSubMedia.porcentaje / 100 * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) AS StockIngreso
			FROM         dbo.Corte AS CorteMediaRes INNER JOIN
								  dbo.Corte AS CorteSubMedia ON CorteMediaRes.idCorte = CorteSubMedia.idCorteMaestro INNER JOIN
								  dbo.Corte AS CorteM INNER JOIN
								  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro ON CorteSubMedia.idCorte = CorteM.idCorteMaestro CROSS JOIN
								  dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
			WHERE     (dbo.Compras.estado = '') AND (CorteP.independiente = 1) AND (CorteMediaRes.codigo = 0) AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta + 1) AND (dbo.MediaRes.idSucursal = @idSucursal) AND (CorteM.codigo > 0)
			GROUP BY CorteP.corte, CorteP.codigo,CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union

			--Compra por Cortes

			--++ Cortes ingresados
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteP ON dbo.CortePorCompra.idCorte = CorteP.idCorte
			WHERE     (CorteP.independiente = 1) AND (dbo.Compras.tipoCompra = 'Cortes') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta+1) AND (dbo.CortePorCompra.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)

			union
			--++ Suma de los cortes ingresados a su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal ON CorteP.idCorte = dbo.CortePorCompra.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Cortes') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteM.codigo > 0) AND (CorteM.independiente = 1)
			GROUP BY CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.Sucursal.sucursal, dbo.CortePorCompra.idSucursal)

			union
			--++ Suma de los cortes ingresados al CorteM de su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM((dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) 
								  + ((dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * 
								  dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) * CorteP.porcentajeHueso / CorteP.porcentaje)) 
								  AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte INNER JOIN
								  dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON dbo.Corte.idCorteMaestro = CorteP.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Cortes') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteM.codigo > 0) AND (CorteM.independiente = 1)
			GROUP BY CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.Sucursal.sucursal, dbo.CortePorCompra.idSucursal)

			union
			--++Suma a los Sub-Cortes independientes del corte ingresado
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100) 
								  AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal ON CorteM.idCorte = dbo.CortePorCompra.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Cortes') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteP.independiente = 1) AND (CorteM.codigo > 0)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)
			union
			--++Suma a los cortes independientes de los subcortes del corte ingresado
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal,
				  SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100 * dbo.Corte.porcentaje / 100) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteM ON dbo.CortePorCompra.idCorte = CorteM.idCorte INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS CorteP ON dbo.Corte.idCorte = CorteP.idCorteMaestro
			WHERE     (dbo.Compras.tipoCompra = 'Cortes') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteP.independiente = 1) AND (CorteM.codigo > 0)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)			
		) as IngresoCompras
		group by idCorte,codigo,corte,idSucursal,sucursal
 
 ) as IngresoCompras  
on IngresoCompras.idSucursal=IngresoStock.idSucursal and IngresoCompras.idCorte=IngresoStock.idCorte

/*Fin Ingresos por Compras*/

 left outer join 
 
 /* Egreso por Ventas*/
 (Select idCorte, codigo,corte,idSucursal,sucursal,SUM(TotalVenta) as TotalVenta
		from 
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS TotalVenta 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION
			
			--Suma de los cortes independientes ingresados 
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.LineaVenta.cantKg) AS TotalVenta
			FROM         dbo.Ventas INNER JOIN
								  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
								  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte
			WHERE     (dbo.Ventas.fechaVenta BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Ventas.idSucursal = @idSucursal) AND (dbo.Corte.independiente = 1)
			GROUP BY dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte)

			union

			--Suma sub-cortes al CorteM independiente
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS TotalVenta
			FROM         dbo.Ventas INNER JOIN
								  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
								  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteP ON dbo.LineaVenta.idCorte = CorteP.idCorte INNER JOIN
								  dbo.Corte ON CorteP.idCorteMaestro = dbo.Corte.idCorte
			WHERE     (dbo.Ventas.fechaVenta BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Ventas.idSucursal = @idSucursal) AND (dbo.Corte.codigo > 0) AND 
								  (dbo.Corte.independiente = 1)
			GROUP BY dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte,dbo.Corte.codigo, dbo.Corte.corte)

			union 
			--Suma sub de los sub-cortes
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
								  SUM((dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * Corte_1.porcentajeHueso / Corte_1.porcentaje) 
								  + ((dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * Corte_1.porcentajeHueso / Corte_1.porcentaje)
								   * CorteP.porcentajeHueso / CorteP.porcentaje)) AS TotalVenta
			FROM         dbo.Corte AS Corte_1 INNER JOIN
								  dbo.Ventas INNER JOIN
								  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
								  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal ON Corte_1.idCorte = dbo.LineaVenta.idCorte INNER JOIN
								  dbo.Corte INNER JOIN
								  dbo.Corte AS CorteP ON dbo.Corte.idCorte = CorteP.idCorteMaestro ON Corte_1.idCorteMaestro = CorteP.idCorte
			WHERE     (dbo.Ventas.fechaVenta BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Ventas.idSucursal = @idSucursal) AND (dbo.Corte.codigo > 0) AND 
								  (dbo.Corte.independiente = 1)
			GROUP BY dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte,dbo.Corte.codigo, dbo.Corte.corte)


			) as EgresoVentas
			group by idCorte,codigo,corte,idSucursal,sucursal
 ) as EgresoVentas  
on EgresoVentas.idSucursal=IngresoStock.idSucursal and EgresoVentas.idCorte=IngresoStock.idCorte

 /*Fin Egreso por ventas */

 left outer join 
 
 /*Egreso stock */
 (  select idCorte,codigo,corte,idSucursal,sucursal, (SUM(StockIngreso))*-1 as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION

			--Stock Egreso

			--++ Cortes ingresados
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteP ON dbo.CortePorCompra.idCorte = CorteP.idCorte
			WHERE     (CorteP.independiente = 1) AND (dbo.Compras.tipoCompra = 'Egreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta+1) AND (dbo.CortePorCompra.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)

			union
			--++ Suma de los cortes ingresados a su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal ON CorteP.idCorte = dbo.CortePorCompra.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Egreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteM.codigo > 0) AND (CorteM.independiente = 1)
			GROUP BY CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.Sucursal.sucursal, dbo.CortePorCompra.idSucursal)

			union
			--++ Suma de los cortes ingresados al CorteM de su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM((dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) 
								  + ((dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * 
								  dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) * CorteP.porcentajeHueso / CorteP.porcentaje)) 
								  AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte INNER JOIN
								  dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON dbo.Corte.idCorteMaestro = CorteP.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Egreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteM.codigo > 0) AND (CorteM.independiente = 1)
			GROUP BY CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.Sucursal.sucursal, dbo.CortePorCompra.idSucursal)

			union
			--++Suma a los Sub-Cortes independientes del corte ingresado
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100) 
								  AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal ON CorteM.idCorte = dbo.CortePorCompra.idCorte
			WHERE     (dbo.Compras.tipoCompra = 'Egreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteP.independiente = 1) AND (CorteM.codigo > 0)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)
			union
			--++Suma a los cortes independientes de los subcortes del corte ingresado
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal,
				  SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100 * dbo.Corte.porcentaje / 100) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteM ON dbo.CortePorCompra.idCorte = CorteM.idCorte INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS CorteP ON dbo.Corte.idCorte = CorteP.idCorteMaestro
			WHERE     (dbo.Compras.tipoCompra = 'Egreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.CortePorCompra.idSucursal = @idSucursal) AND
								   (CorteP.independiente = 1) AND (CorteM.codigo > 0)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)			
		) as EgresoStock
		group by idCorte,codigo,corte,idSucursal,sucursal
 ) as EgresoStock  
on EgresoStock.idSucursal=IngresoStock.idSucursal and EgresoStock.idCorte=IngresoStock.idCorte

/* Fin Egrso Stock */

 left outer join 
 
 /* Egreso por Embutidos*/
 (select idCorte,codigo,corte,idSucursal,sucursal, SUM(TotalEnEmbutidos) as TotalEnEmbutidos
		from
		(
		(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS TotalEnEmbutidos 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION
		--++Suma de Cortes Ingresados (independientes)
		(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorEmbutido.kgUtilizados) AS TotalEnEmbutidos
		FROM         dbo.CortePorEmbutido INNER JOIN
							  dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
							  dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
							  dbo.Corte ON dbo.CortePorEmbutido.idCorte = dbo.Corte.idCorte
		WHERE     (dbo.Embutidos.estado = '') AND (dbo.Embutidos.fechaEmbutido BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Embutidos.idSucursal = @idSucursal) AND 
							  (dbo.Corte.independiente = 1)
		GROUP BY dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte,dbo.Corte.codigo)
		union
		(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
							  SUM(dbo.CortePorEmbutido.kgUtilizados * Corte_1.porcentaje / 100) AS TotalEnEmbutidos
		FROM         dbo.CortePorEmbutido INNER JOIN
							  dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
							  dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
							  dbo.Corte AS Corte_1 ON dbo.CortePorEmbutido.idCorte = Corte_1.idCorte INNER JOIN
							  dbo.Corte ON Corte_1.idCorte = dbo.Corte.idCorteMaestro
		WHERE     (dbo.Embutidos.estado = '') AND (dbo.Embutidos.fechaEmbutido BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Embutidos.idSucursal = @idSucursal) AND 
							  (dbo.Corte.independiente = 1)
		GROUP BY dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte, dbo.Corte.codigo)
		union
		--++Suma de Cortes Ingresados a su Corte Maestro
		(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
							  SUM(dbo.CortePorEmbutido.kgUtilizados + dbo.CortePorEmbutido.kgUtilizados * SubCorte.porcentajeHueso / SubCorte.porcentaje) AS TotalEnEmbutidos
		FROM         dbo.CortePorEmbutido INNER JOIN
							  dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
							  dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
							  dbo.Corte AS SubCorte ON dbo.CortePorEmbutido.idCorte = SubCorte.idCorte INNER JOIN
							  dbo.Corte ON SubCorte.idCorteMaestro = dbo.Corte.idCorte
		WHERE     (dbo.Embutidos.estado = '') AND (dbo.Embutidos.fechaEmbutido BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Embutidos.idSucursal = @idSucursal) AND 
							  (dbo.Corte.independiente = 1) AND (dbo.Corte.codigo > 0)
		GROUP BY dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte,dbo.Corte.codigo)
		union

		--++Suma de Cortes Ingresados al Corte M de su Corte Maestro
		(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
							  SUM((dbo.CortePorEmbutido.kgUtilizados + dbo.CortePorEmbutido.kgUtilizados * SubCorte.porcentajeHueso / SubCorte.porcentaje) 
							  + ((dbo.CortePorEmbutido.kgUtilizados + dbo.CortePorEmbutido.kgUtilizados * SubCorte.porcentajeHueso / SubCorte.porcentaje) 
							  * Corte_1.porcentajeHueso / Corte_1.porcentaje)) AS TotalEnEmbutidos
		FROM         dbo.CortePorEmbutido INNER JOIN
							  dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
							  dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
							  dbo.Corte AS SubCorte ON dbo.CortePorEmbutido.idCorte = SubCorte.idCorte INNER JOIN
							  dbo.Corte AS Corte_1 ON SubCorte.idCorteMaestro = Corte_1.idCorte INNER JOIN
							  dbo.Corte ON Corte_1.idCorteMaestro = dbo.Corte.idCorte
		WHERE     (dbo.Embutidos.estado = '') AND (dbo.Embutidos.fechaEmbutido BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Embutidos.idSucursal = @idSucursal) AND 
							  (dbo.Corte.independiente = 1) AND (dbo.Corte.codigo > 0)
		GROUP BY dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte,dbo.Corte.codigo)

		) as EgresoPorEmbutido
		group by idCorte, codigo,corte,idSucursal,sucursal
 ) as EgresoPorEmbutido  
on EgresoPorEmbutido.idSucursal=IngresoStock.idSucursal and EgresoPorEmbutido.idCorte=IngresoStock.idCorte

/* Fin Egreso por Embutidos */
 left outer join 
 
 /*Egreso por Movimientos */
 (select idCorte,codigo,corte,idSucursal,sucursal,(SUM(StockIngreso) * -1) as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION
			---EgresoMovimiento
			--++SubCorte de Media
			((SELECT     dbo.Corte.idCorte,dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(-dbo.CortePorMovimiento.cantKg) AS StockIngreso
			FROM         dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
								  dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalOrigen = dbo.Sucursal.idSucursal
			WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal) 
			union
			--++SubCorte 2 de Media
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal,
						 SUM(- (dbo.CortePorMovimiento.cantKg+dbo.CortePorMovimiento.cantKg*SubCorte.porcentajeHueso/SubCorte.porcentaje)) AS StockIngreso
			FROM         dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalOrigen = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS SubCorte ON dbo.CortePorMovimiento.idCorte = SubCorte.idCorte INNER JOIN
								  dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte ON SubCorte.idCorteMaestro = dbo.Corte.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (CorteM.codigo < 1) AND (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
			union
			--++SubCorte 3 de Media
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal,
						 SUM(- ((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje)
								 +((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje))
								 *SubCorte.porcentajeHueso/SubCorte.porcentaje)))) AS StockIngreso
			FROM         dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalOrigen = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS SubCorte2 ON dbo.CortePorMovimiento.idCorte = SubCorte2.idCorte INNER JOIN
								  dbo.Corte INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Corte AS SubCorte ON dbo.Corte.idCorte = SubCorte.idCorteMaestro ON SubCorte2.idCorteMaestro = SubCorte.idCorte
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (CorteM.codigo < 1) AND (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union
			--++Cortes Independiente derivados de un Corte que deriva de Media Res 
			(SELECT     TOP (100) PERCENT dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(-dbo.CortePorMovimiento.cantKg) 
								  AS StockIngreso
			FROM         dbo.Sucursal INNER JOIN
								  dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Corte AS CorteM INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte AND CorteM.idCorte <> MediaRes.idCorte ON 
								  dbo.CortePorMovimiento.idCorte = dbo.Corte.idCorte ON dbo.Sucursal.idSucursal = dbo.Movimiento.sucursalOrigen
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (MediaRes.codigo < 1) AND (dbo.Corte.independiente = 1) AND 
								  (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union
			--++Cortes Independiente no derivados de Media Res direcamente
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(-dbo.CortePorMovimiento.cantKg) AS StockIngreso
			FROM         dbo.Sucursal INNER JOIN
								  dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
								  dbo.Corte AS CorteM INNER JOIN
								  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro INNER JOIN
								  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte ON dbo.CortePorMovimiento.idCorte = dbo.Corte.idCorte ON 
								  dbo.Sucursal.idSucursal = dbo.Movimiento.sucursalOrigen
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (MediaRes.codigo > 0) AND (dbo.Corte.independiente = 1) AND 
								  (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
			union
			--++Sub Cortes de Cortes Independiente no derivados de Media Res direcamente
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal,
						 SUM(-(dbo.CortePorMovimiento.cantKg + dbo.CortePorMovimiento.cantKg*  CorteP.porcentajeHueso/ CorteP.porcentaje)) AS StockIngreso
			FROM         dbo.Corte INNER JOIN
								  dbo.Corte AS CorteP ON dbo.Corte.idCorte = CorteP.idCorteMaestro INNER JOIN
								  dbo.CortePorMovimiento ON dbo.CortePorMovimiento.idCorte = CorteP.idCorte INNER JOIN
								  dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
								  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte INNER JOIN
								  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte INNER JOIN
								  dbo.Sucursal ON dbo.Movimiento.sucursalOrigen = dbo.Sucursal.idSucursal
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (MediaRes.codigo > 0) AND (dbo.Corte.independiente = 1) AND 
								  (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			union 

			--++ Suma al sub corte independiente del corte ingresado derivado de la media res
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(- (dbo.CortePorMovimiento.cantKg * dbo.Corte.porcentaje / 100)) AS StockIngreso
			FROM         dbo.Movimiento INNER JOIN
					  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
					  dbo.Corte INNER JOIN
					  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte INNER JOIN
					  dbo.Corte AS CorteMedia ON CorteM.idCorteMaestro = CorteMedia.idCorte ON dbo.CortePorMovimiento.idCorte = CorteM.idCorte INNER JOIN
					  dbo.Sucursal ON dbo.Movimiento.sucursalOrigen = dbo.Sucursal.idSucursal
			WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (CorteMedia.codigo < 1) AND (dbo.Corte.independiente = 1) AND 
					  (dbo.Movimiento.sucursalOrigen = @idSucursal)
			GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
			)
		) as EgresoMovimiento
		group by idCorte,codigo,corte,idSucursal,sucursal
 ) as EgresoMovimiento  
on EgresoMovimiento.idSucursal=IngresoStock.idSucursal and EgresoMovimiento.idCorte=IngresoStock.idCorte

/* Fin Egreso por Movimientos */

where (@texto like '' 
	and (IngresoCompras.StockIngreso > 0 or IngresoStock.StockIngreso > 0 or IngresoEmbutido.StockIngreso > 0 or IngresoMovimiento.StockIngreso> 0 or EgresoStock.StockIngreso > 0 or EgresoMovimiento.StockIngreso > 0 or EgresoPorEmbutido.TotalEnEmbutidos > 0 or EgresoVentas.TotalVenta > 0 or IngresoStock.StockIngreso > 0)) 
	or (@texto not like '' and (IngresoStock.corte like '%'+@texto+'%' or  CAST( IngresoStock.codigo as NCHAR) = @texto))
order by IngresoStock.codigo

END

GO

