USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[VaciarDatosTabla]    Script Date: 02/09/2016 17:14:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[VaciarDatosTabla]
	-- Add the parameters for the stored procedure here
	@ActualizacionCorte tinyint = 0,
	@ActualizacionStock tinyint = 0,
	@ActualizacionStockPorCorte tinyint = 0,
	@CierreCaja tinyint = 0,
	@Claves tinyint = 0,
	@Compras tinyint = 0,
	@Corte tinyint = 0,
	@CortePorCompra tinyint = 0,
	@CortePorEmbutido tinyint = 0,
	@CortePorMovimiento tinyint = 0,
	@Embutidos tinyint = 0,
	@Feriados tinyint = 0,
	@Gastos tinyint = 0,
	@LineaVenta tinyint = 0,
	@MediaRes tinyint = 0,
	@Movimiento tinyint = 0,
	@Pagos tinyint = 0,
	@Personas tinyint = 0,
	@Proveedores tinyint = 0,
	@StockCorteSucursal tinyint = 0,
	@Sucursal tinyint = 0,
	@TipoGasto tinyint = 0,
	@Usuarios tinyint = 0,
	@Ventas tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF	@ActualizacionCorte = 1
		truncate table ActualizacionCorte		
	
	if @ActualizacionStock = 1
		truncate table ActualizacionStock
		
	if @ActualizacionStockPorCorte = 1
		truncate table ActualizacionStockPorCorte
	
	if @CierreCaja = 1
		truncate table CierreCaja
		
	if @Claves = 1
		truncate table Claves		
	
	if @Compras = 1
		truncate table Compras
		
	if @Corte = 1
		truncate table Corte
		
	if @CortePorCompra = 1
		truncate table CortePorCompra
		
	if @CortePorEmbutido = 1
		truncate table CortePorEmbutido
		
	if @CortePorMovimiento = 1
		truncate table CortePorMovimiento
		
	if @Embutidos = 1
		truncate table Embutidos
		
	if @Feriados = 1
		truncate table Feriados
		
	if @Gastos = 1
		truncate table Gastos
		
	if @LineaVenta = 1
		truncate table LineaVenta
		
	if @MediaRes = 1
		truncate table MediaRes
		
	if @Movimiento = 1
		truncate table Movimiento
		
	if @Pagos = 1
		truncate table Pagos
		
	if @Personas = 1
		truncate table Personas
		
	if @Proveedores = 1
		truncate table Proveedores
		
	if @StockCorteSucursal = 1
		truncate table StockCorteSucursal
		
	if @Sucursal = 1
		truncate table Sucursal
		
	if @TipoGasto = 1
		truncate table TipoGasto
		
	if @Usuarios = 1
		truncate table Usuarios
		
	if @Usuarios = 1
		truncate table Usuarios
		
	if @Ventas = 1
		truncate table Ventas
	
END

GO

