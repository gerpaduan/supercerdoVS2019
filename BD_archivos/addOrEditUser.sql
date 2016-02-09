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
CREATE PROCEDURE addOrEditUser
	-- Add the parameters for the stored procedure here
	@id int = null,
	@nombre	varchar(50)	,
	@usuario varchar(50),	
	@clave varchar(50),	
	@admin	tinyint,	
	@colorForm	varchar(50)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF(@id = 0 or @id is null)
		BEGIN
			INSERT INTO Usuarios (nombre, usuario, clave, admin, colorForm) 
				values (@nombre, @usuario, @clave, @admin, @colorForm)
		END	
	ELSE
		BEGIN
			UPDATE Usuarios SET 
				nombre = @nombre, usuario = @usuario, clave = @clave, admin = @admin, colorForm = @colorForm
				WHERE id = @id
		END		
END
GO
