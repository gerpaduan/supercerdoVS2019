USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[agregarVenta]    Script Date: 03/19/2015 17:27:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[agregarVenta] 
	-- Add the parameters for the stored procedure here
	@idVenta int = 0,
	@idVentaUltima int = null,
	@fechaVenta datetime,
	@turno nvarchar(50),
	@tipoVenta nvarchar(50),
	@vendedor nvarchar(50),
	@diaFestivo nvarchar(50),
	@observaciones nvarchar(200),
	@idPersona int,
	@idSucursal int,
	@nroRemito nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- Insert statements for procedure here
      IF(@idVenta = 0) --CONDITION
            BEGIN 
				set @idVentaUltima  = (select top 1 Ventas.idVenta from Ventas where Ventas.idSucursal = @idSucursal order by Ventas.idVenta desc) 
				IF(@idVentaUltima is null or @idVentaUltima < (10000000 * @idSucursal)) --CONDITION
					 BEGIN
						set @idVentaUltima = (10000000 * @idSucursal)
					 END
                 set @idVenta = @idVentaUltima + 1
            END
            
	insert into Ventas (idVenta, fechaVenta,idSucursal,turno,diaFestivo,observaciones,idPersona,nroRemito,estado, creado)
	values (@idVenta, @fechaVenta,@idSucursal,@turno,@diaFestivo,@observaciones,@idPersona,@nroRemito,'', SYSDATETIME())

	select top 1 Ventas.idVenta from Ventas where Ventas.idSucursal = @idSucursal order by Ventas.idVenta desc
	
END

GO


