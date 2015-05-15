USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[CortePorMovimiento]    Script Date: 05/03/2015 16:53:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DECLARE @idMovimiento int, @lastId int, @creado datetime
set @idMovimiento = 1
SET @lastId = (select top 1 Movimiento.idMovimiento from Movimiento order by Movimiento.idMovimiento desc)

WHILE @idMovimiento <= @lastId
BEGIN
	set @creado = null
	set @creado = (select Movimiento.fechaMovimiento from Movimiento where Movimiento.idMovimiento = @idMovimiento)
	
	update Movimiento set creado = @creado where Movimiento.idMovimiento = @idMovimiento
	
	set @idMovimiento = @idMovimiento + 1 
END

GO


