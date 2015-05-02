USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[agregarCompra]    Script Date: 04/20/2015 19:01:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[agregarCompra] 
	-- Add the parameters for the stored procedure here
	@nroRemito nvarchar(50),
	@fechaCompra datetime,
	@idProveedor int,
	@estado nvarchar(50),
	@observaciones nvarchar(200),
	@tipoCompra nvarchar(50),
	@idSucursal int,
	@creadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into Compras (nroRemito, fechaCompra, idProveedor, estado, observaciones,tipoCompra, idSucursal, creado, creadoPor)
	values (@nroRemito,@fechaCompra,@idProveedor,@estado,@observaciones,@tipoCompra, @idSucursal, SYSDATETIME(), @creadoPor)
	
	select top 1 Compras.idCompra from Compras order by Compras.idCompra desc
END

GO

