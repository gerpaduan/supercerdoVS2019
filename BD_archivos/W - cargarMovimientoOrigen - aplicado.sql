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
CREATE PROCEDURE cargarMovimientoOrigen
	@idMovimiento int = 0,
	@fechaMovimiento datetime,
	@sucursalOrigen int,
	@sucursalDestino int,
	@idMovOrigen int = 0,
	@observaciones nvarchar(200) = '',
	@isAdd tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @isAdd = 1
		begin
			insert into Movimiento 
					(fechaMovimiento,sucursalOrigen,sucursalDestino, idMovOrigen,observaciones, creado)
			values (@fechaMovimiento,@sucursalOrigen,@sucursalDestino, @idMovOrigen,@observaciones, SYSDATETIME())
			
			select top 1 idMovimiento from Movimiento order by idMovimiento desc
		end
	else
		begin
			update Movimiento set fechaMovimiento=@fechaMovimiento,sucursalOrigen=@sucursalOrigen,
				sucursalDestino=@sucursalDestino, observaciones=@observaciones, actualizado = SYSDATETIME()
			where idMovOrigen=@idMovOrigen	
			
			select top 1 idMovimiento from Movimiento order by idMovimiento desc
		end
END
GO
