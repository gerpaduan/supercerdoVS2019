-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
alter PROCEDURE addOrEditMovimiento
	-- Add the parameters for the stored procedure here	
	@idMovimiento int = 0,
	@fechaMovimiento datetime,
	@sucursalOrigen int,
	@sucursalDestino int,
	@observaciones nvarchar(max),
	@creadoPor int = null,
	@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @idMovimiento = 0
		begin
			-- Agrego el movimiento
			insert into Movimiento (fechaMovimiento,sucursalOrigen,sucursalDestino,observaciones, creado, creadoPor)
			values (@fechaMovimiento,@sucursalOrigen,@sucursalDestino,@observaciones, SYSDATETIME(), @creadoPor)
			
			select top 1 idMovimiento from Movimiento order by idMovimiento desc
		end
	else
		begin
		
			--Se crea registro de historial
			insert into MovimientoHistorial (idMovimiento, FechaMovimiento, idSucOrigen, idSucDestino, idCorte, cantKg, cantUnidad, pesoBalanza, actualizadoPor, actualizado, observaciones)

SELECT     dbo.Movimiento.idMovimiento, dbo.Movimiento.fechaMovimiento, dbo.Movimiento.sucursalOrigen, dbo.Movimiento.sucursalDestino, 
					  dbo.CortePorMovimiento.idCorte, dbo.CortePorMovimiento.cantKg, dbo.CortePorMovimiento.cantUnidad, dbo.CortePorMovimiento.pesoBalanza, 
					  @actualizadoPor, SYSDATETIME(), dbo.Movimiento.observaciones
FROM         dbo.Movimiento INNER JOIN
					  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
where Movimiento.idMovimiento = @idMovimiento				
			
			--se actualiza los datos del movimiento
			update Movimiento set fechaMovimiento=@fechaMovimiento,sucursalOrigen=@sucursalOrigen,sucursalDestino=@sucursalDestino,observaciones=@observaciones, actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor
	where idMovimiento=@idMovimiento
	
			--se eliminan todos los cortes en el movimiento
			delete from CortePorMovimiento where idMovimientos=@idMovimiento
	
		end
END
GO
