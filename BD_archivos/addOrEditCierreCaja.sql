USE [SuperCerdo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE addOrEditCierreCaja 
	-- Add the parameters for the stored procedure here
	@id	int = 0,
	@idCierreAnterior int = 0,
	@idSucursal	int,
	@fechaHoraInicio datetime,
	@fechaHoraCierre datetime,
	@cajaInicio	float,
	@ventas	float,
	@gastos	float,
	@saldoCaja	float,
	@cajaCierre	float,
	@diferencia	float,
	@cajaInicioSiguiente float,
	@importeRetirado float,
	@usuarioInicio varchar(50),
	@usuarioCierre varchar(50),
	@creado	datetime,
	@actualizado datetime,
	@idEncontrado int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- int,erfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
      IF(@id = 0 or @id is null)
            BEGIN 
				set @idCierreAnterior  = (select top 1 CierreCaja.id from CierreCaja where CierreCaja.idSucursal = @idSucursal order by CierreCaja.id desc) 
				IF(@idCierreAnterior is null or @idCierreAnterior < (10000000 * @idSucursal)) --CONDITION
					 BEGIN
						set @idCierreAnterior = (10000000 * @idSucursal)
					 END
                 set @id = @idCierreAnterior + 1
                 
                insert into CierreCaja (id, idSucursal, fechaHoraInicio, fechaHoraCierre, cajaInicio, ventas, gastos, saldoCaja, cajaCierre, diferencia, cajaInicioSiguiente, importeRetirado, usuarioInicio, usuarioCierre, creado, actualizado)
						values (@id, @idSucursal, @fechaHoraInicio, @fechaHoraCierre, @cajaInicio, @ventas, @gastos, @saldoCaja, @cajaCierre, @diferencia, @cajaInicioSiguiente, @importeRetirado, @usuarioInicio, @usuarioCierre, SYSDATETIME(),'')
            END
       ELSE
		BEGIN
			set @idEncontrado = (select CierreCaja.id from CierreCaja where CierreCaja.id = @id and CierreCaja.idSucursal = @idSucursal)		
			IF(@idEncontrado > 0)
				BEGIN 
					UPDATE CierreCaja
					 SET idSucursal = @idSucursal,fechaHoraInicio = @fechaHoraInicio,fechaHoraCierre = @fechaHoraCierre,
						cajaInicio = @cajaInicio, ventas = @ventas,gastos = @gastos,saldoCaja = @saldoCaja,cajaCierre = @cajaCierre,diferencia = @diferencia,
						cajaInicioSiguiente = @cajaInicioSiguiente,importeRetirado = @importeRetirado,usuarioInicio = @usuarioInicio,
						usuarioCierre = @usuarioCierre,actualizado = SYSDATETIME()
					 WHERE id = @id
				END	
			ELSE
				BEGIN 
					insert into CierreCaja (id, idSucursal, fechaHoraInicio, fechaHoraCierre, cajaInicio, ventas, gastos, saldoCaja, cajaCierre, diferencia, cajaInicioSiguiente, importeRetirado, usuarioInicio, usuarioCierre, creado, actualizado)
							values (@id, @idSucursal, @fechaHoraInicio, @fechaHoraCierre, @cajaInicio, @ventas, @gastos, @saldoCaja, @cajaCierre, @diferencia, @cajaInicioSiguiente, @importeRetirado, @usuarioInicio, @usuarioCierre, SYSDATETIME(),'')
				END			
		END           

	select top 1 CierreCaja.id from CierreCaja where CierreCaja.idSucursal = @idSucursal order by CierreCaja.id desc
	
END
GO
