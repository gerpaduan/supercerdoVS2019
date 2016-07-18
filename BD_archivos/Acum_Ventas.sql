USE [SuperCerdo]
GO
/****** Object:  StoredProcedure [dbo].[Acum_Ventas]    Script Date: 07/18/2016 18:11:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[Acum_Ventas]
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

select cast(AllCortes.codigo as NCHAR(5)) as Codigo ,AllCortes.corte as 'Corte', 0.00 as 'StockActual', EgresoVentas.TotalVenta as 'Ventas', 0.00 as 'DIF'
	from
		--Seleccion de todos los cortes
		(SELECT     CorteP.idCorte as idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0.00 AS StockIngreso 
		FROM         dbo.Corte AS CorteP CROSS JOIN
							  dbo.Sucursal
		WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = 
		@idSucursal)
		GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
		as AllCortes
		
		left outer JOIN
    /* Egreso por Ventas*/
 (Select idCorte, codigo,corte,idSucursal,sucursal,SUM(TotalVenta) as TotalVenta
		from 
		(
			
			--Suma de los cortes independientes ingresados 
			(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.LineaVenta.cantKg) AS TotalVenta
			FROM         dbo.Ventas INNER JOIN
								  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
								  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte
			WHERE     (dbo.Ventas.fechaVenta BETWEEN @fechaDesde AND @fechaHasta) AND (dbo.Ventas.idSucursal = @idSucursal) AND (dbo.Corte.independiente = 1)
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
			WHERE     (dbo.Ventas.fechaVenta BETWEEN @fechaDesde AND @fechaHasta) AND (dbo.Ventas.idSucursal = @idSucursal) AND (dbo.Corte.codigo > 0) AND 
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
			WHERE     (dbo.Ventas.fechaVenta BETWEEN @fechaDesde AND @fechaHasta) AND (dbo.Ventas.idSucursal = @idSucursal) AND (dbo.Corte.codigo > 0) AND 
								  (dbo.Corte.independiente = 1)
			GROUP BY dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, dbo.Corte.idCorte,dbo.Corte.codigo, dbo.Corte.corte)


			) as EgresoVentas
			group by idCorte,codigo,corte,idSucursal,sucursal
 ) as EgresoVentas  
on EgresoVentas.idSucursal=AllCortes.idSucursal and EgresoVentas.idCorte=AllCortes.idCorte

 /*Fin Egreso por ventas */
 
where (@texto like '')
	or (@texto not like '' and (AllCortes.corte like '%'+@texto+'%' or  CAST( AllCortes.codigo as NCHAR) = @texto))
order by AllCortes.codigo

END
