USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[addOrEditEgresoCaja]    Script Date: 04/22/2016 10:56:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[addOrEditEgresoCaja]
	@id int = 0,
	@fecha datetime = null,
	@idTipoEgresoCaja int,
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
			insert into dbo.EgresosCaja (fechaHora,idTipoEgresoCaja,descripcion,detalle,monto,idSucursal,creado,creadoPor)		
			values (@fecha,@idTipoEgresoCaja,@descripcion,@detalle,@monto,@idSucursal, SYSDATETIME(),@creadoPor)
		END
	ELSE
		BEGIN
			update dbo.EgresosCaja 
			set  fechaHora = @fecha,idTipoEgresoCaja = @idTipoEgresoCaja,descripcion = @descripcion,
				detalle = @detalle,monto = @monto,idSucursal = @idSucursal,	actualizado = SYSDATETIME(),						actualizadoPor = @actualizadoPor
			where id = @id

		END
END

GO

