USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[addOrEditGasto]    Script Date: 02/09/2016 12:06:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditGasto]
	@id int = 0,
	@fecha datetime = null,
	@idTipoGasto int,
	@descripcion nvarchar(50),
	@detalle nvarchar(MAX),
	@monto float,
	@idSucursal int,
	@creadoPor int = null,
	@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF @id = 0
		BEGIN
			insert into dbo.Gastos (fechaHora,idTipoGasto,descripcion,detalle,monto,idSucursal,creado,creadoPor)		
			values (@fecha,@idTipoGasto,@descripcion,@detalle,@monto,@idSucursal, SYSDATETIME(),@creadoPor)
		END
	ELSE
		BEGIN
			update dbo.Gastos 
			set  fechaHora = @fecha,idTipoGasto = @idTipoGasto,descripcion = @descripcion,
				detalle = @detalle,monto = @monto,idSucursal = @idSucursal,	actualizado = SYSDATETIME(),						actualizadoPor = @actualizadoPor
			where id = @id

		END
END

GO

