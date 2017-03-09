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
CREATE PROCEDURE addOrEditPersona
	-- Add the parameters for the stored procedure here
	
	@idPersona  int = null,
	@razonSocial nvarchar(50) = null,
	@otrosDatos nvarchar(200) = null,
	@tipo nvarchar(50) = null,
	@ctaCte tinyint  = null,
	@bonificacion float  = null
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF @idPersona = 0 
			BEGIN 
				Insert into Personas (razonSocial, otrosDatos, tipo, ctaCte, bonificacion) values (@razonSocial, @otrosDatos, @tipo, @ctaCte, @bonificacion) 
				set @idPersona = SCOPE_IDENTITY()
			END 
		ELSE 
		BEGIN 
			update Personas set razonSocial=@razonSocial, tipo=@tipo, otrosDatos= @otrosDatos,
				ctaCte = @ctaCte, bonificacion = @bonificacion 
			where idPersona =@idPersona 		
		END 
	select @idPersona 
END
GO
