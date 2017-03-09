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
CREATE PROCEDURE addOrEditPago
--ALTER PROCEDURE addOrEditPago
	-- Add the parameters for the stored procedure here

		@id int = null,
		@nroRecibo nvarchar(50) = null,
		@fecha datetime = null,
		@idPersona int = null,
		@aProveedor tinyint = null,
		@formaPago nvarchar(50) = null,
		@banco nvarchar(50) = null,
		@nroCheque nvarchar(50) = null,
		@titularCheque nvarchar(50) = null,
		@importe float = null,
		@observaciones nvarchar(MAX) = null,
		@idSucursal int = null,
		@creado datetime = null,
		@creadoPor int = null,
		@actualizado datetime = null,
		@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
		IF @id = 0 
			BEGIN 
				INSERT INTO dbo.Pagos (nroRecibo, fecha, idPersona, aProveedor, formaPago, 
				banco, nroCheque, titularCheque, importe, observaciones, idSucursal, creado, 
				creadoPor) 
				 VALUES (@nroRecibo, @fecha, @idPersona, @aProveedor, @formaPago, @banco, @nroCheque, 
				 @titularCheque, @importe, @observaciones, @idSucursal, SYSDATETIME(), @creadoPor) 
			set @id = SCOPE_IDENTITY()
			END 
		ELSE 
		BEGIN 
			UPDATE dbo.Pagos set nroRecibo = @nroRecibo, fecha = @fecha, idPersona = @idPersona, 
			aProveedor = @aProveedor, formaPago = @formaPago, banco = @banco, nroCheque = @nroCheque, 		
			titularCheque = @titularCheque, importe = @importe, observaciones = @observaciones,
			idSucursal =  @idSucursal, actualizado =  SYSDATETIME(), actualizadoPor =  @actualizadoPor
			WHERE id = @id		
		END 
	select @id 
END
GO
