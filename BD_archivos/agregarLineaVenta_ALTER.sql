USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[agregarLineaVenta]    Script Date: 04/15/2015 16:55:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[agregarLineaVenta] 
	
	@idVenta int,
	@idCorte int,
	@pesoBalanza int,
	@idAnulado int, --0 Activo --1 Anulado
	@cantKg float,
	@precioKg float
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    
     
	insert into LineaVenta (idVenta,idCorte,  idAnulado,cantKg,precioKg, pesoBalanza)
	values (@idVenta,@idCorte, @idAnulado,@cantKg,@precioKg, @pesoBalanza)
	
	--if(select estado from Ventas where idVenta=@idVenta)=''
	--begin
	
	--	update StockCorteSucursal set stock=(stock - LineaVenta.cantKg)
		
	--	FROM         dbo.Corte INNER JOIN
	--					  dbo.StockCorteSucursal ON dbo.Corte.idCorte = dbo.StockCorteSucursal.idCorte INNER JOIN
	--					  dbo.LineaVenta INNER JOIN
	--					  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta ON dbo.StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal AND 
	--					  dbo.Corte.idCorte = dbo.LineaVenta.idCorte
	--	WHERE     (dbo.Ventas.idVenta = @idVenta) AND (dbo.Corte.idCorte = @idcorte) AND (dbo.LineaVenta.idAnulado = @idanulado)
			
		
	--	------Se ingresa la cantidad de Kg en hueso que representa el corte
		
	--	--update StockCorteSucursal 
	--	--	set stock=(stock + (@cantKg * CorteP.porcentajeHueso /CorteP.porcentaje))
	--	--FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, Ventas,
	--	--	SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CortePuchero		 
	--	--	WHERE StockCorteSucursal.idCorte=CortePuchero.idCorte and CortePuchero.corte like 'Puchero'
	--	--	and CorteP.idCorte=@idCorte
	--	--	 and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta
			
		

	--	----Actulizar los cortes del cual deriva el corte ingresado

	--		-- Se actualiza el corte maestro del corte superior
	--	update StockCorteSucursal 
	--		set stock=( stock -
	--						  (SELECT     (dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) 
	--									 + ((dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje) 
	--									 * CorteM.porcentajeHueso / CorteM.porcentaje)
										
	--								FROM         dbo.LineaVenta INNER JOIN
	--													  dbo.Corte AS CorteMedia INNER JOIN
	--													  dbo.StockCorteSucursal INNER JOIN
	--													  dbo.Ventas ON dbo.StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal ON CorteMedia.idCorte = dbo.StockCorteSucursal.idCorte ON 
	--													  dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
	--													  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
	--													  dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND CorteMedia.idCorte = CorteM.idCorteMaestro
	--								WHERE     (dbo.Ventas.idVenta = Ventas_1.idVenta) AND (LineaVenta_1.idAnulado = dbo.LineaVenta.idAnulado) AND (dbo.Corte.idCorte = CorteP.idCorte) ))
	--	FROM         dbo.Corte AS CorteM_1 INNER JOIN
	--						  dbo.StockCorteSucursal AS StockCorteSucursal_1 INNER JOIN
	--						  dbo.Ventas AS Ventas_1 ON StockCorteSucursal_1.idSucursal = Ventas_1.idSucursal INNER JOIN
	--						  dbo.Corte AS CorteMedia_1 ON StockCorteSucursal_1.idCorte = CorteMedia_1.idCorte INNER JOIN
	--						  dbo.LineaVenta AS LineaVenta_1 INNER JOIN
	--						  dbo.Corte AS CorteP ON LineaVenta_1.idCorte = CorteP.idCorte ON Ventas_1.idVenta = LineaVenta_1.idVenta ON CorteM_1.idCorteMaestro = CorteMedia_1.idCorte AND
	--						   CorteM_1.idCorte = CorteP.idCorteMaestro
	--	WHERE     (Ventas_1.idVenta = @idventa) AND (CorteMedia_1.codigo > 0) and CorteP.idCorte=@idCorte and LineaVenta_1.idAnulado=@idAnulado 
	--	--			and CorteP.tipo='Corte'
					

			
	--	-- Se actualiza el corte superior del corte ingresado
	--	update StockCorteSucursal 
	--		set stock=(stock - 
	--						  (SELECT     dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * dbo.Corte.porcentajeHueso / dbo.Corte.porcentaje AS Expr1
	--FROM         dbo.StockCorteSucursal INNER JOIN
	--					  dbo.Ventas ON dbo.StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
	--					  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
	--					  dbo.Corte AS CorteM ON dbo.StockCorteSucursal.idCorte = CorteM.idCorte INNER JOIN
	--					  dbo.Corte ON CorteM.idCorte = dbo.Corte.idCorteMaestro AND dbo.LineaVenta.idCorte = dbo.Corte.idCorte
	--WHERE     (dbo.Ventas.idVenta = Ventas_1.idVenta) AND (LineaVenta_1.idAnulado = dbo.LineaVenta.idAnulado) AND (dbo.Corte.idCorte = CorteP.idCorte)))
	--	FROM         dbo.StockCorteSucursal AS StockCorteSucursal_1 INNER JOIN
	--						  dbo.Ventas AS Ventas_1 ON StockCorteSucursal_1.idSucursal = Ventas_1.idSucursal INNER JOIN
	--						  dbo.LineaVenta AS LineaVenta_1 INNER JOIN
	--						  dbo.Corte AS CorteP ON LineaVenta_1.idCorte = CorteP.idCorte ON Ventas_1.idVenta = LineaVenta_1.idVenta INNER JOIN
	--						  dbo.Corte AS CorteM_1 ON CorteP.idCorteMaestro = CorteM_1.idCorte AND CorteP.idCorte <> CorteM_1.idCorte AND 
	--						  StockCorteSucursal_1.idCorte = CorteM_1.idCorte
	--	WHERE     (Ventas_1.idVenta = @idVenta) AND (CorteM_1.codigo > 0) and CorteP.idCorte=@idCorte and LineaVenta_1.idAnulado=@idAnulado
	--	--		and CorteP.tipo='Corte'
			
		
	--	--SubCortes del corte vendido
	--	update StockCorteSucursal 
	--	set stock=dbo.StockCorteSucursal.stock - dbo.LineaVenta.cantKg * SubCorte.porcentaje / 100 
	--	FROM         dbo.LineaVenta INNER JOIN
	--		  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
	--		  dbo.StockCorteSucursal ON dbo.Ventas.idSucursal = dbo.StockCorteSucursal.idSucursal INNER JOIN
	--		  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
	--		  dbo.Corte AS SubCorte ON dbo.Corte.idCorte = SubCorte.idCorteMaestro AND dbo.StockCorteSucursal.idCorte = SubCorte.idCorte
	--   WHERE     (dbo.Ventas.idVenta = @idVenta) AND (dbo.Corte.idCorte = @idcorte) AND (dbo.LineaVenta.idAnulado = @idanulado)
		
		
	--	--(StockCorteSucursalMaestro.stock * CorteSubCorte.porcentaje/100)
	--	--FROM         dbo.Corte AS CorteP INNER JOIN
 -- --                dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
 -- --                dbo.Corte AS CorteSubCorte ON StockCorteSucursal.idCorte = CorteSubCorte.idCorte INNER JOIN
 -- --                dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
 -- --                dbo.StockCorteSucursal AS StockCorteSucursalMaestro ON dbo.Ventas.idSucursal = StockCorteSucursalMaestro.idSucursal ON 
 -- --                CorteP.idCorte = StockCorteSucursalMaestro.idCorte AND CorteP.idCorte = CorteSubCorte.idCorteMaestro INNER JOIN
 -- --                dbo.LineaVenta ON CorteP.idCorte = dbo.LineaVenta.idCorte AND dbo.Ventas.idVenta = dbo.LineaVenta.idVenta
	--	--WHERE     (dbo.Ventas.idVenta = @idVenta) 
		
	--	--Sub Cortes de los Sub-Cortes del corte vendido
	--	update StockCorteSucursal 
	--	set stock=  dbo.StockCorteSucursal.stock - dbo.LineaVenta.cantKg * SubCorte.porcentaje / 100 * SubCorte2.porcentaje / 100 
	--	FROM         dbo.LineaVenta INNER JOIN
	--			  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
	--			  dbo.StockCorteSucursal ON dbo.Ventas.idSucursal = dbo.StockCorteSucursal.idSucursal INNER JOIN
	--			  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
	--			  dbo.Corte AS SubCorte ON dbo.Corte.idCorte = SubCorte.idCorteMaestro INNER JOIN
	--			  dbo.Corte AS SubCorte2 ON dbo.StockCorteSucursal.idCorte = SubCorte2.idCorte AND SubCorte.idCorte = SubCorte2.idCorteMaestro
	--	WHERE     (dbo.Ventas.idVenta = @idVenta) AND (dbo.Corte.idCorte = @idcorte) AND (dbo.LineaVenta.idAnulado = @idanulado)
		
	--	--(StockCorteSucursalMaestro.stock * CorteSubCorte2.porcentaje / 100 )
	--	--FROM         dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
	--	--		  dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--	--		  dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal ON StockCorteSucursalMaestro.idSucursal = dbo.Ventas.idSucursal INNER JOIN
	--	--		  dbo.LineaVenta INNER JOIN
	--	--		  dbo.Corte AS CorteP ON dbo.LineaVenta.idCorte = CorteP.idCorte ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
	--	--		  dbo.Corte AS CorteSubCorte ON StockCorteSucursalMaestro.idCorte = CorteSubCorte.idCorte AND CorteP.idCorte = CorteSubCorte.idCorteMaestro INNER JOIN
	--	--		  dbo.Corte AS CorteSubCorte2 ON StockCorteSucursal.idCorte = CorteSubCorte2.idCorte AND CorteSubCorte.idCorte = CorteSubCorte2.idCorteMaestro
	--	--WHERE     (dbo.Ventas.idVenta = @idVenta)
		
		
		
		
	--	--SE ACTUALIZA EL STOCK DE LOS CORTE QUE DERIVAN DEL CORTE MAESTRO DEL CORTE EN CUESTION
		
	--	--SubCortes del corte Maestro del corte maestro del Corte ingresado
		
	--	update StockCorteSucursal 
	--	set stock=(StockCorteSucursalMaestro.stock * CorteSubCorte1.porcentaje/100)
	--	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--		  dbo.Corte AS CorteSubCorte1 ON StockCorteSucursal.idCorte = CorteSubCorte1.idCorte INNER JOIN
	--		  dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
	--		  dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
	--		  dbo.Corte AS CorteSubCorte ON StockCorteSucursalMaestro.idCorte = CorteSubCorte.idCorte ON dbo.Ventas.idSucursal = StockCorteSucursalMaestro.idSucursal AND 
	--		  CorteSubCorte1.idCorteMaestro = CorteSubCorte.idCorte INNER JOIN
	--		  dbo.Corte AS CorteP INNER JOIN
	--		  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON CorteSubCorte.idCorte = CorteM.idCorteMaestro AND 
	--		  CorteSubCorte1.idCorte <> CorteM.idCorte INNER JOIN
	--		  dbo.LineaVenta ON CorteP.idCorte = dbo.LineaVenta.idCorte AND dbo.Ventas.idVenta = dbo.LineaVenta.idVenta
	--	WHERE     (dbo.Ventas.idVenta = @idVenta)  AND (CorteSubCorte.codigo > 0) AND (CorteSubCorte1.independiente = 0)

	--	--SubCortes de los SubCortes del Corte Maestro del Corte Maestro del Corte ingresado
	--	update StockCorteSucursal 
	--	set stock=(StockCorteSucursalMaestro.stock * CorteSubCorte2.porcentaje/100)
	--	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
 --                     dbo.Corte AS CorteSubCorte2 ON StockCorteSucursal.idCorte = CorteSubCorte2.idCorte INNER JOIN
 --                     dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
 --                     dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
 --                     dbo.Corte AS CorteSubCorte1 ON StockCorteSucursalMaestro.idCorte = CorteSubCorte1.idCorte ON dbo.Ventas.idSucursal = StockCorteSucursalMaestro.idSucursal AND
 --                      CorteSubCorte2.idCorteMaestro = CorteSubCorte1.idCorte INNER JOIN
 --                     dbo.Corte AS CorteSubCorte ON CorteSubCorte1.idCorteMaestro = CorteSubCorte.idCorte INNER JOIN
 --                     dbo.Corte AS CorteP INNER JOIN
 --                     dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON CorteSubCorte.idCorte = CorteM.idCorteMaestro AND 
 --                     CorteSubCorte1.idCorte <> CorteM.idCorte INNER JOIN
 --                     dbo.LineaVenta ON CorteP.idCorte = dbo.LineaVenta.idCorte AND dbo.Ventas.idVenta = dbo.LineaVenta.idVenta
	--	WHERE     (dbo.Ventas.idVenta = @idVenta) AND (CorteSubCorte.codigo > 0) and (CorteSubCorte1.independiente=0)
	--				 and (CorteSubCorte2.independiente=0)
	
	--	--FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--	--				  dbo.Corte AS CorteSubCorte2 ON StockCorteSucursal.idCorte = CorteSubCorte2.idCorte INNER JOIN
	--	--				  dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
	--	--				  dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
	--	--				  dbo.Corte AS CorteSubCorte1 ON StockCorteSucursalMaestro.idCorte = CorteSubCorte1.idCorte ON dbo.Ventas.idSucursal = StockCorteSucursalMaestro.idSucursal AND
	--	--				   CorteSubCorte2.idCorteMaestro = CorteSubCorte1.idCorte INNER JOIN
	--	--				  dbo.Corte AS CorteSubCorte ON CorteSubCorte1.idCorteMaestro = CorteSubCorte.idCorte INNER JOIN
	--	--				  dbo.Corte AS CorteP INNER JOIN
	--	--				  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON CorteSubCorte.idCorte = CorteM.idCorteMaestro AND 
	--	--				  CorteSubCorte.idCorte <> CorteM.idCorte INNER JOIN
	--	--				  dbo.LineaVenta ON CorteP.idCorte = dbo.LineaVenta.idCorte
	--	--WHERE     (dbo.Ventas.idVenta = @idVenta) AND (CorteSubCorte.codigo > 0) --AND (CorteSubCorte2.independiente = 0)

	--	--SubCortes del CorteMaestro
	--	update StockCorteSucursal 
	--	set stock=(StockCorteSucursalMaestro.stock * CorteSubCorte.porcentaje/100)
	--	FROM         dbo.Corte AS CorteP INNER JOIN
 --                     dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
 --                     dbo.Corte AS CorteSubCorte ON StockCorteSucursal.idCorte = CorteSubCorte.idCorte INNER JOIN
 --                     dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
 --                     dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
 --                     dbo.Corte AS CorteM ON StockCorteSucursalMaestro.idCorte = CorteM.idCorte ON dbo.Ventas.idSucursal = StockCorteSucursalMaestro.idSucursal AND 
 --                     CorteSubCorte.idCorteMaestro = CorteM.idCorte ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteSubCorte.idCorte INNER JOIN
 --                     dbo.LineaVenta ON CorteP.idCorte = dbo.LineaVenta.idCorte AND dbo.Ventas.idVenta = dbo.LineaVenta.idVenta
	--	WHERE     (dbo.Ventas.idVenta = @idVenta) AND (CorteM.codigo > 0) AND (CorteSubCorte.independiente = 0)
		
	--	--FROM         dbo.Corte AS CorteP INNER JOIN
	--	--				  dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--	--				  dbo.Corte AS CorteSubCorte ON StockCorteSucursal.idCorte = CorteSubCorte.idCorte INNER JOIN
	--	--				  dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
	--	--				  dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
	--	--				  dbo.Corte AS CorteM ON StockCorteSucursalMaestro.idCorte = CorteM.idCorte ON dbo.Ventas.idSucursal = StockCorteSucursalMaestro.idSucursal AND 
	--	--				  CorteSubCorte.idCorteMaestro = CorteM.idCorte ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
	--	--				  dbo.LineaVenta ON CorteP.idCorte = dbo.LineaVenta.idCorte AND dbo.Ventas.idVenta = dbo.LineaVenta.idVenta
	--	--WHERE     (dbo.Ventas.idVenta = @idVenta) AND (CorteM.codigo > 0) AND (CorteSubCorte.independiente = 0)
		
		
	--	--Subcortes de los Subcortes del Corte Maestro
	--	update StockCorteSucursal 
	--	set stock=(StockCorteSucursalMaestro.stock * CorteSubCorte2.porcentaje / 100 )
	--	 FROM         dbo.LineaVenta INNER JOIN
 --                     dbo.Corte AS CorteM INNER JOIN
 --                     dbo.Corte AS CorteSubCorte ON CorteM.idCorte = CorteSubCorte.idCorteMaestro INNER JOIN
 --                     dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro AND CorteSubCorte.idCorte <> CorteP.idCorte ON dbo.LineaVenta.idCorte = CorteP.idCorte INNER JOIN
 --                     dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
 --                     dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
 --                     dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal ON StockCorteSucursalMaestro.idSucursal = dbo.Ventas.idSucursal ON 
 --                     dbo.LineaVenta.idVenta = dbo.Ventas.idVenta AND CorteSubCorte.idCorte = StockCorteSucursalMaestro.idCorte INNER JOIN
 --                     dbo.Corte AS CorteSubCorte2 ON StockCorteSucursal.idCorte = CorteSubCorte2.idCorte AND CorteSubCorte.idCorte = CorteSubCorte2.idCorteMaestro
	--	WHERE     (dbo.Ventas.idVenta = @idVenta) AND (CorteM.codigo > 0) and (CorteSubCorte.independiente=0)
	--			 and (CorteSubCorte2.independiente=0)
	--	--FROM         dbo.LineaVenta INNER JOIN
	--	--					  dbo.Corte AS CorteM INNER JOIN
	--	--					  dbo.Corte AS CorteSubCorte ON CorteM.idCorte = CorteSubCorte.idCorteMaestro INNER JOIN
	--	--					  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro ON dbo.LineaVenta.idCorte = CorteP.idCorte INNER JOIN
	--	--					  dbo.StockCorteSucursal AS StockCorteSucursalMaestro INNER JOIN
	--	--					  dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--	--					  dbo.Ventas ON StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal ON StockCorteSucursalMaestro.idSucursal = dbo.Ventas.idSucursal ON 
	--	--					  dbo.LineaVenta.idVenta = dbo.Ventas.idVenta AND CorteSubCorte.idCorte = StockCorteSucursalMaestro.idCorte INNER JOIN
	--	--					  dbo.Corte AS CorteSubCorte2 ON StockCorteSucursal.idCorte = CorteSubCorte2.idCorte AND CorteSubCorte.idCorte = CorteSubCorte2.idCorteMaestro
	--	--WHERE     (dbo.Ventas.idVenta = @idVenta) AND (CorteM.codigo > 0) -- AND (CorteSubCorte2.independiente = 0)
		
		
	--end
	


END

GO

