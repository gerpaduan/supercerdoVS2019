USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[modificarCompra]    Script Date: 04/20/2015 19:02:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[modificarCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@nroRemito nvarchar(50),
	@fechaCompra datetime,
	@idProveedor int,
	@estado nvarchar(50),
	@observaciones nvarchar(200),
	@tipoCompra nvarchar(50),
	@idSucursal int,
	@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update Compras set nroRemito=@nroRemito,fechaCompra=@fechaCompra,
		idProveedor=@idProveedor, estado=@estado, observaciones=@observaciones, tipoCompra=@tipoCompra,
		idSucursal=@idSucursal ,actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor
	where idCompra=@idCompra
	
	delete from CortePorCompra where CortePorCompra.idCompra=@idCompra 
END

GO

