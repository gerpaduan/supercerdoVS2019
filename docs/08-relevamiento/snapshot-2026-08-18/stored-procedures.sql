 
-
GO -- ===== a_CierreStock =====

CREATE PROCEDURE [dbo].[a_CierreStock]
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@tipo nvarchar(50),
	@idProveedor i
GO -- ===== a_CierreStockWeb =====

-- ============================================================================
-- Fix sobre 20260804-Create_a_CierreStockWeb.sql: 2 bugs de calculo detectados
-- al auditar por que "Egreso Stock" aparecia negativo en l
GO -- ===== a_ExistenciaStockPorSucursales =====

-- ============================================================================
-- Fix sobre 20260804-Alter_a_ExistenciaStockPorSucursales_FiltroEmpresaEnMapaCorte.sql:
-- mismos 2 bugs de calculo detectad
GO -- ===== a_IngresoEgreso =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[a
GO -- ===== A1_CopiarBD_Diferente_Nombre =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCE
GO -- ===== A2_Crear_Claves_Foraneas_e_Indices =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE
GO -- ===== A3_VaciarDatosTabla =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo
GO -- ===== AA_AltaEmpresa =====

CREATE   PROCEDURE [dbo].[AA_AltaEmpresa]
(
    @razonSocialAfip        NVARCHAR(100) = NULL,
    @cuit                   BIGINT        = NULL,
    @nombreFantasia         NVARCHAR(100) = NULL,
    @slogan1         
GO -- ===== achicaLog =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[achicaLog
GO -- ===== Acum_Ventas =====

CREATE PROCEDURE [dbo].[Acum_Ventas]
    @texto nvarchar(50),
    @idSucursal int = 0,
    @fechaDesde datetime,
    @fechaHasta datetime,
    @tipo nvarchar(50),
    @idProveedor int,
    @idMarca int,
    @idEmpre
GO -- ===== addOrEditCierreCaja =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].
GO -- ===== addOrEditCompra =====

CREATE PROCEDURE [dbo].[addOrEditCompra]
    @idCompra int = 0,
    @nroRemito nvarchar(50) = '',
    @fechaCompra datetime,
    @idProveedor int,
    @cantMedias int = null,
    @kgsMedias int = null,
    @estad
GO -- ===== addOrEditCorte =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addO
GO -- ===== addOrEditEgresoCaja =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].
GO -- ===== addOrEditFacturaElectronica =====

CREATE PROCEDURE [dbo].[addOrEditFacturaElectronica]
    @id int = null,
    @ptoVtaAfip nvarchar(50) = null,
    @fechaEmisionAfip nvarchar(50) = null,
    @descTipoCbteAfip nvarchar(50) = null,
    @c
GO -- ===== addOrEditFormula =====

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditFormula] 
	
	@idFormula int =
GO -- ===== addOrEditGasto =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ad
GO -- ===== addOrEditMovCtaCte =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[
GO -- ===== addOrEditMovimiento =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].
GO -- ===== addOrEditPago =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[add
GO -- ===== addOrEditPersona =====

CREATE PROCEDURE [dbo].[addOrEditPersona]
    @idPersona int = NULL,
    @identificacion nvarchar(50) = NULL,
    @razonSocial nvarchar(50) = NULL,
    @idIva int = NULL,
    @cuit nvarchar(50) = NULL,
    @telef
GO -- ===== addOrEditUser =====

CREATE PROCEDURE [dbo].[addOrEditUser]
    @id int = null,
    @nombre varchar(50),
    @usuario varchar(50),
    @clave varchar(50),
    @email varchar(100) = '',
    @admin tinyint,
    @activo tinyint,
    @col
GO -- ===== agregarActualizacionStock =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarActualizacionStock] 
	-- 
GO -- ===== agregarCompra =====

CREATE PROCEDURE [dbo].[agregarCompra]
    @nroRemito nvarchar(50) = '',
    @fechaCompra datetime,
    @idProveedor int,
    @cantMedias int = null,
    @kgsMedias int = null,
    @estado nvarchar(50),
    @observ
GO -- ===== agregarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCorte] 
	-- Add the parameters for the
GO -- ===== agregarCortePorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorCompra] 
	-- Add the 
GO -- ===== agregarCortePorEmbutido =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorEmbutido] 
	-- Add 
GO -- ===== agregarCortePorFormula =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [
GO -- ===== agregarCortePorMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorMovimiento] 
	-- 
GO -- ===== agregarEmbutido =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarEmbutido] 
	-- Add the parameters f
GO -- ===== agregarExpendio =====

CREATE PROCEDURE [dbo].[agregarExpendio]
	@idExpendio int=0,
	@idVendedor int=0,
	@fechaExpendio datetime=null,
	@idSucursal int=null,
	@identificacionExpendio nvarchar(50)=null,
	@sector nvarchar(MAX)=null,
	@cantItems 
GO -- ===== agregarLineaExpendio =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo]
GO -- ===== agregarLineaVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarLineaVenta] 
	
	@idLineaVenta in
GO -- ===== agregarMediaRes =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarMediaRes] 
	-- Add the parameters f
GO -- ===== agregarStockVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarStockVenta] 
		-- Add the paramet
GO -- ===== agregarVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[agrega
GO -- ===== anularCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[anularCompra] 
	-- Add the parameters for the
GO -- ===== anularEmbutido =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[anularEmbutido] 
	-- Add the parameters for
GO -- ===== anularMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[anularMovimiento] 
	-- Add the parameters
GO -- ===== Balance =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Balance]

GO -- ===== BalanceConMeses =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Bal
GO -- ===== BalanceConsFinal =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[
GO -- ===== BalanceConsFinal_FecDesde_Hasta =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PR
GO -- ===== BalanceConsFinalVariosMeses =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDUR
GO -- ===== buscarCodigoCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[buscarCodigoCorte] 
	-- Add the paramete
GO -- ===== buscarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[buscarCorte] 
	-- Add the parameters for the s
GO -- ===== buscarCorteSinMaestro =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[buscarCorteSinMaestro] 
	-- Add the 
GO -- ===== buscarEmbutido =====
CREATE PROCEDURE [dbo].[buscarEmbutido]
	@texto nvarchar(50),
	@idSucursal int = 0,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	SET NOCOUNT ON;

	SELECT     dbo.Embutidos.idEmbutido as 'Id', dbo.Embutidos.fechaE
GO -- ===== CargarBancos_BlocNotas =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [db
GO -- ===== cargarCortesPorMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[cargarCortesPorMovimiento] 
	-- 
GO -- ===== cargarMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[cargarMovimiento] 
	-- Add the parameters
GO -- ===== cargarMovimientoOrigen =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [db
GO -- ===== ControlLineasVtas =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[C
GO -- ===== EliminarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[EliminarCorte] 
	-- Add the parameters for t
GO -- ===== eliminarLineas =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[eliminarLineas] 
	-- Add the parameters for
GO -- ===== eliminarMovimiento =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[
GO -- ===== eliminarPersona =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[eliminarPersona] 
	-- Add the parameters f
GO -- ===== getAllLineasVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[g
GO -- ===== getCtaCteByIdPersona =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo]
GO -- ===== getLineasCompras =====


-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[getLineasCompras] 
	-- Add the parame
GO -- ===== getListaElegirEmbutido =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [db
GO -- ===== getPorcCortesEnMedias =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo
GO -- ===== getPromMedias =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getPr
GO -- ===== getUsuariosActivos =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[
GO -- ===== IngresoMovIndependiente =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[IngresoMovIndependiente] 
	-- Add 
GO -- ===== modificarCompra =====

CREATE PROCEDURE [dbo].[modificarCompra]
    @idCompra int,
    @nroRemito nvarchar(50) = '',
    @fechaCompra datetime,
    @idProveedor int,
    @cantMedias int = null,
    @kgsMedias int = null,
    @estado nv
GO -- ===== modificarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarCorte] 
	-- Add the parameters for
GO -- ===== modificarLineaVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarLineaVenta] 
	@idVenta int,

GO -- ===== modificarMediaPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarMediaPorCompra] 
	-- Add 
GO -- ===== modificarMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarMovimiento] 
	-- Add the para
GO -- ===== modificarPersona =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarPersona] 
	
	@otrosDatos nvarch
GO -- ===== modificarPrecioMedia =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarPrecioMedia] 
	-- Add the pa
GO -- ===== ModificarPrecioPorPorcentaje =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDU
GO -- ===== modificarProveedor =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarProveedor] 
	-- Add the parame
GO -- ===== modificarVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[modi
GO -- ===== obtenerCompras =====

-- Agrega dbo.Compras.idPesajeAjustado a cada uno de los 14 SELECT/GROUP BY (7 tipoCompra x 2 ramas
-- @idSucursal>0 / ELSE) de obtenerCompras. Sin esta columna, la grilla de /Stock no puede mostrar a
-- que Pesaje/Compra
GO -- ===== obtenerCortes =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerCortes] 
	-- Add the parameters for t
GO -- ===== obtenerCortesPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerCortesPorCompra] 
	-- Add th
GO -- ===== obtenerCortesPorEmbutidos =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerCortesPorEmbutidos] 
	-- 
GO -- ===== ObtenerCortesPrimarios =====
-- =============================================
-- Author:		
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[ObtenerCortesPrimarios] 
	-- Add the pa
GO -- ===== obtenerEgresosCaja =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo]
GO -- ===== obtenerEmbutidos =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerEmbutidos] 
	-- Add the parameters
GO -- ===== obtenerEmbutidoTotal =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerEmbutidoTotal] 
	-- Add the pa
GO -- ===== obtenerGastos =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obt
GO -- ===== obtenerInfoCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	Obtiene la información completa del corte seleccionado
-- =============================================
CREATE PROCE
GO -- ===== obtenerLineasEmb =====

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasEmb] 
	-- Add the paramete
GO -- ===== obtenerLineasMov =====

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasMov] 
	-- Add the paramete
GO -- ===== obtenerLineasVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasVenta] 
	-- Add the parame
GO -- ===== obtenerMediasPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerMediasPorCompra] 
	-- Add th
GO -- ===== obtenerMovimientos =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerMovimientos] 
	-- Add the parame
GO -- ===== obtenerNivelCorte =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[o
GO -- ===== obtenerTemporalLineaVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
--create PROCEDUR
GO -- ===== obtenerTotalVentas =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo]
GO -- ===== obtenerVentas =====
CREATE PROCEDURE [dbo].[obtenerVentas] 
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50) = '',
	@idSucursal int = -1,
	@idVendedor int = -1,
	@idCliente int = -1,
	@soloAnulados tinyint = 0
AS
BEG
GO -- ===== porcentajeCortesPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[porcentajeCortesPorCompra] 
	-- 
GO -- ===== quitarCortesPorMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarCortesPorMovimiento] 
	-- 
GO -- ===== quitarStockCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarStockCorte] 
	-- Add the parameters
GO -- ===== quitarStockMedia =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarStockMedia] 
	-- Add the parameters
GO -- ===== quitarStockTeoricoMedia =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarStockTeoricoMedia] 
	-- Add 
GO -- ===== ReiniciarCuarto =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[ReiniciarCuarto] 
	-- Add the parameters f
GO -- ===== reiniciarStock =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[reiniciarStock] 
	-- Add the parameters for
GO -- ===== reiniciarStockTeorico =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[reiniciarStockTeorico] 
	-- Add the 
GO -- ===== ResumenVentasMesPorCliente =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE
GO -- ===== sp_EmpresaParametros_SetDefaults =====

CREATE   PROCEDURE dbo.sp_EmpresaParametros_SetDefaults
    @idEmpresa INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Validaciones básicas
    IF @idEmpresa IS NULL OR @idEmpres
GO -- ===== StockCierre =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockCierre] 
	-- Add the parameters for the s
GO -- ===== StockCierre_2 =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockCierre_2] 
	@texto nvarchar(50),
	@ids
GO -- ===== StockIngresoEgreso =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockIngresoEgreso] 
	-- Add the parame
GO -- ===== StockTeoricoReal =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockTeoricoReal]
	-- Add the parameters 
GO -- ===== TicketAnualdo =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TicketAnualdo] 
	-- Add the parameters for t
GO -- ===== TotalKgsCortePorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalKgsCortePorCompra] 
	-- Add th
GO -- ===== TotalMovimientosCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalMovimientosCorte] 

	@texto nv
GO -- ===== TotalMovimientosPorCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalMovimientosPorCorte] 
	-- Ad
GO -- ===== TotalPorCortesVendidos =====

CREATE   PROCEDURE [dbo].[TotalPorCortesVendidos]
    @texto nvarchar(50),
    @idEmpresa int = NULL,
    @idSucursal int,
    @fechaDesde datetime,
    @fechaHasta datetime,
    @tipo nvarchar(50),
    @i
GO -- ===== TotalSegunCompras =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalSegunCompras] 

	-- Add the parame
GO -- ===== TotalSegunComprasMonto =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalSegunComprasMonto] 
	-- Add th
GO -- ===== ultimasVentasCliente =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo]
GO -- ===== ventasVendedorCierreCaja =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [
