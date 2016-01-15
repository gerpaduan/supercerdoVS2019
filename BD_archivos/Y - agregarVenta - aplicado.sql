USE [SuperCerdo]
GO
/****** Object:  StoredProcedure [dbo].[agregarVenta]    Script Date: 01/13/2016 21:21:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[agregarVenta] 
	-- Add the parameters for the stored procedure here
	@idVendedor int = null,
	@idVentaUltima int = null,
	@fechaVenta datetime,
	@turno nvarchar(50),
	@tipoVenta nvarchar(50) = null,
	@diaFestivo nvarchar(50) = null,
	@observaciones nvarchar(200),
	@idPersona int,
	@idSucursal int,
	@nroRemito nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	set @diaFestivo = (select Feriados.feriado from Feriados where @fechaVenta between Feriados.desde and Feriados.hasta)
            
	insert into Ventas (idVendedor, fechaVenta,idSucursal,turno,diaFestivo,observaciones,idPersona,nroRemito,estado, creado)
	values (@idVendedor, @fechaVenta,@idSucursal,@turno,@diaFestivo,@observaciones,@idPersona,@nroRemito,'', SYSDATETIME())

	select top 1 Ventas.idVenta from Ventas where Ventas.idSucursal = @idSucursal order by Ventas.idVenta desc
	
END
