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
CREATE PROCEDURE addOrEditCorte
	-- Add the parameters for the stored procedure here
	@idCorte int = null,
	@codigo int,
	@corte nvarchar(50),
	@precioKg float,
	@tipo nvarchar(50),
	@independiente int,
	@idCorteMaestro int,
	@porcentaje float,
	@porcentajeHueso float,
	@desvioEstandar float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

     -- Edit
	IF (@idCorte > 0)
		begin
			update Corte set codigo=@codigo,corte=@corte, precioKg=@precioKg, tipo=@tipo,
			independiente=@independiente,idCorteMaestro=@idCorteMaestro, porcentaje=@porcentaje,
			porcentajeHueso=@porcentajeHueso,desvioEstandar=@desvioEstandar, actualizado = SYSDATETIME() 
			where idCorte=@idCorte

			-----Se crea el registro del historial
			insert into ActualizacionCorte (idCorte, codigo, corte, precioKg, tipo, independiente,
			 idCorteMaestro,porcentaje,porcentajeHueso, desvioEstandar, creado, actualizado)
			 values (@idCorte, @codigo,@corte,@precioKg,@tipo, @independiente,@idCorteMaestro,@porcentaje,
			 @porcentajeHueso, @desvioEstandar, null, SYSDATETIME())		
		end
	-- Add
	ELSE 
		begin
			if ( @idCorteMaestro = 0)
				begin
					insert into Corte (codigo, corte, precioKg, tipo, independiente, idCorteMaestro,
					porcentaje,porcentajeHueso, desvioEstandar, creado)
					 values (@codigo,@corte,@precioKg,@tipo, @independiente,@idCorteMaestro,@porcentaje,
					 @porcentajeHueso, @desvioEstandar, SYSDATETIME())					
					
					SELECT @idCorte = SCOPE_IDENTITY()
					update Corte set Corte.idCorteMaestro = @idCorte, porcentaje = 100, porcentajeHueso = 0,
					desvioEstandar = 0
					where Corte.idCorte = @idCorte
				end
				
			else
				begin
					insert into Corte (codigo, corte, precioKg, tipo, independiente, idCorteMaestro,
					 porcentaje,porcentajeHueso,desvioEstandar, creado)
					 values (@codigo,@corte,@precioKg,@tipo,@independiente,@idCorteMaestro,@porcentaje,
					 @porcentajeHueso,@desvioEstandar, SYSDATETIME())
				end
	
			--se inician los stock de los sucursales a cero
			insert into StockCorteSucursal(idCorte,idSucursal,stock,stockTeorico)
			values ((select top 1 Corte.idCorte from Corte order by Corte.idCorte desc), 1, 0,0)
			
			insert into StockCorteSucursal(idCorte,idSucursal,stock,stockTeorico)
			values ((select top 1 Corte.idCorte from Corte order by Corte.idCorte desc), 2, 0,0)
		end
END
GO
