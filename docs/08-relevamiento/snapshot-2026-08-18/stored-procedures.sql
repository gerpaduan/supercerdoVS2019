-- ===== a_CierreStock =====

CREATE PROCEDURE [dbo].[a_CierreStock]
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@tipo nvarchar(50),
	@idProveedor int,
	@idMarca int,
	@idEmpresa int = NULL
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--se verifica si texto es un numero entero
	
	Declare @esNumero int = (SELECT ISNUMERIC(@texto));
    
	select CONVERT(INT, AllCortes.idCorte) AS idCorte, cast(AllCortes.codigo as NCHAR(20)) as Codigo ,
	AllCortes.corte as 'Corte', StockInicial.StockCierre as 'Stock.Ini', 
	IngresoCompras.StockIngreso as 'Compras', IngresoEmbutido.StockIngreso as 'Ingr.Elab',IngresoStock.StockIngreso as 'Ingr.Stock', 
	IngresoMovimiento.StockIngreso as 'Ingr. Mov' , AjusteStock.StockIngreso as 'Ajus.Stock', 0.00 as 'Tot.INGR' , 
	EgresoStock.StockIngreso as 'Egr.Stock', EgresoMovimiento.StockIngreso as 'Egr.Mov', EgresoPorEmbutido.TotalEnEmbutidos as 'Egr.Elab', 
	EgresoVentas.TotalVenta as 'Ventas', 0.00 as 'Tot.EGR', 0.00 as 'DIF', StockCierre.StockCierre as 'Stock.Cierre', 0.00 as 'Faltante',
	 AllCortes.promedio, '-' as 'Stock.Un', '' as 'Falta', AllCortes.puntoStock as 'Pto.Stock', AllCortes.pesable as 'Pesable'
	from
		--Seleccion de todos los cortes
		(
		--SELECT     CorteP.idCorte as idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0.00 AS StockIngreso, CorteP.promedio, CorteP.enCierreStock, CorteP.puntoStock
		--FROM     dbo.Corte AS CorteP LEFT OUTER JOIN
  --                dbo.CorteProveedor ON CorteP.idCorte = dbo.CorteProveedor.idCorte CROSS JOIN
  --                dbo.Sucursal
		--WHERE  (CorteP.independiente = 1) AND (dbo.Sucursal.idSucursal = @idSucursal)
		--and (@tipo = '' OR @tipo IS NULL OR tipo = @tipo )
		--and (@idProveedor = 0 OR @idProveedor IS NULL OR idProveedor = @idProveedor) 
		--and (@idMarca = 0 OR @idMarca IS NULL OR idMarca = @idMarca )
		--GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, CorteP.promedio, CorteP.enCierreStock, CorteP.puntoStock
		
		SELECT DISTINCT
			CorteP.idCorte AS idCorte,
			CorteP.codigo,
			CorteP.corte,
			s.idSucursal,
			s.sucursal,
			0.00 AS StockIngreso,
			CorteP.promedio,
			CorteP.enCierreStock,
			CorteP.puntoStock,
			CorteP.pesable
		FROM dbo.Corte AS CorteP
		CROSS APPLY (SELECT TOP 1 * FROM dbo.Sucursal s WHERE s.idSucursal = @idSucursal) s
		LEFT JOIN dbo.CorteProveedor cp ON CorteP.idCorte = cp.idCorte
		WHERE CorteP.independiente = 1
		  AND (@idEmpresa IS NULL OR CorteP.idEmpresa = @idEmpresa)
		  AND (@tipo IS NULL OR @tipo = '' OR CorteP.tipo = @tipo)
		  AND (@idProveedor IS NULL OR @idProveedor = 0 OR cp.idProveedor = @idProveedor)
		  AND (@idMarca IS NULL OR @idMarca = 0 OR idMarca = @idMarca)
		)
		as AllCortes
		
		left outer JOIN
			
		(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
		from
		(
			--Stock Ingreso

			--++ Cortes ingresados
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteP ON dbo.CortePorCompra.idCorte = CorteP.idCorte
			WHERE     (CorteP.independiente = 1) AND (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta) AND (dbo.CortePorCompra.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)

			union
			--++ Suma de los cortes ingresados a su Corte Maestro
GO
-- ===== a_CierreStockWeb =====

-- ============================================================================
-- Fix sobre 20260804-Create_a_CierreStockWeb.sql: 2 bugs de calculo detectados
-- al auditar por que "Egreso Stock" aparecia negativo en la grilla de Reportes.
--
--   1. Signo de EgresoStock: Web/Controllers/StockController.cs (linea ~618)
--      guarda los movimientos "Egreso Stock" con cantKg NEGATIVO
--      (cantidad = cantidad * -1 al guardar). Este SP sumaba ese cantKg tal
--      cual, sin corregir signo, a diferencia de EgresoMovimiento/
--      EgresoElaborado/Ventas que si quedan positivos. Resultado: Egr.Stock
--      salia negativo, Tot.EGR quedaba subestimado, y Faltante/DIF (que
--      restan Egr.Stock) terminaban SUMANDOLO en vez de restarlo -- un
--      egreso de stock inflaba el stock calculado en vez de reducirlo.
--      Fix: multiplicar por -1 solo la fila 'Egreso Stock' al acumularla en
--      #Operaciones, para que EgresoStock quede como magnitud positiva
--      (igual que las otras 3 columnas de egreso).
--
--   2. StockInicial ignoraba @fechaDesde: tomaba el "Cierre Stock" mas
--      reciente sin condicion (MAX(fechaCompra) incondicional). Da igual en
--      modo "Stock Actual"/"Stock Retroactivo" (ahi @fechaDesde YA es el
--      cierre mas reciente), pero esta mal en modo "Cierre Stock", donde
--      Web/Controllers/ReportesController.cs (AplicarConfiguracionFechasSegunReporte)
--      pasa a proposito el ANTE-ultimo cierre como @fechaDesde (para
--      calcular el Faltante entre el ante-ultimo y el ultimo cierre). El SP
--      seguia tomando el ultimo cierre como StockInicial, comparandolo
--      consigo mismo en vez de auditar el periodo real.
--      Fix: igual que StockCierre ya hace con @fechaHasta
--      ("c.fechaCompra LIKE @fechaHasta"), StockInicial ahora usa
--      "c.fechaCompra LIKE @fechaDesde" -- mismo patron, sin subquery
--      correlacionada.
--
-- Ver docs/DECISIONS.md (2026-08-05, entrada "Egreso Stock negativo...")
-- para el detalle completo y la verificacion contra datos de prueba.
-- ============================================================================

-- ============================================================================
-- a_CierreStockWeb: reemplazo de a_CierreStock EXCLUSIVO PARA WEB.
--
-- a_CierreStock (nroCierre=1) esta genuinamente compartido con WinForms (4
-- llamadas reales: Presentacion/Cortes/formReporteStock.cs x2,
-- Presentacion/Stock/formStockActual.cs, Presentacion/Stock/formAddOrEditStock.cs).
-- Nunca se toca WinForms y no hay forma de probarlo tras un cambio, asi que
-- en vez de alterar el SP compartido se escribe este SP nuevo, solo para Web.
-- a_CierreStock sigue existiendo intacto para WinForms, para siempre.
--
-- Mismo patron ya probado en a_ExistenciaStockPorSucursales (de 6.8s a 1.1s):
-- la jerarquia de cortes (madre/hija) se calcula UNA SOLA VEZ en #MapaCorte,
-- en vez de repetir el self-join 5 veces por cada una de las ~10 categorias
-- de movimiento como hace a_CierreStock.
--
-- Diferencias de comportamiento respecto a a_CierreStock (intencionales,
-- confirmadas, no bugs de esta migracion):
--   1. @idSucursal = 0 ahora significa "todas las sucursales de la empresa"
--      (CROSS JOIN, igual que a_ExistenciaStockPorSucursales). En a_CierreStock,
--      @idSucursal=0 hace que el CROSS APPLY sobre Sucursal no encuentre fila
--      y el reporte entero devuelve 0 filas sin error -- ese es justo el modo
--      en que ReportesController llama al SP cuando el usuario no filtra por
--      una sucursal puntual (model.SucursalId > 0 ? model.SucursalId : 0).
--   2. enCierreStock=1 se filtra en #AllCortes (antes de calcular cualquier
--      movimiento), consistente para busqueda con y sin texto. En
--      a_CierreStock ese filtro solo corria al final y SOLO cuando @texto=''
--      -- buscar por texto se salteaba el filtro por completo.
--   3. Columnas nuevas idSucursal/Sucursal en la salida, para poder devolver
--      varias sucurs
GO
-- ===== a_ExistenciaStockPorSucursales =====

-- ============================================================================
-- Fix sobre 20260804-Alter_a_ExistenciaStockPorSucursales_FiltroEmpresaEnMapaCorte.sql:
-- mismos 2 bugs de calculo detectados y corregidos en a_CierreStockWeb
-- (ver 20260805-Alter_a_CierreStockWeb_SignoEgresoStockYFechaInicial.sql).
--
--   1. Signo de EgresoStock: cantKg viene NEGATIVO desde
--      Web/Controllers/StockController.cs para los movimientos "Egreso
--      Stock" (guarda cantidad*-1). Este SP sumaba ese cantKg tal cual, sin
--      corregir signo. Fix: multiplicar por -1 solo la fila 'Egreso Stock'
--      al acumularla en #Operaciones, igual que en a_CierreStockWeb.
--
--   2. FechaUltimoCierre ignoraba @fechaHasta: tomaba el "Cierre Stock" mas
--      reciente sin condicion (MAX(c.fechaCompra) incondicional en el LEFT
--      JOIN de #Sucursales). Ya documentado en docs/DECISIONS.md (2026-08-05,
--      "Existencia por Sucursales: bloquear FechaHasta anterior al ultimo
--      cierre") y mitigado ahi solo con un guard de UI que bloquea pedir una
--      @fechaHasta anterior al ultimo cierre. Fix real: acotar el propio
--      MAX() por @fechaHasta en la condicion del JOIN (no en el WHERE, para
--      no romper el LEFT JOIN cuando no hay ningun cierre antes de esa
--      fecha). Todo lo que ya usa s.FechaUltimoCierre rio abajo (StockInicial,
--      Compras, Ventas, Movimientos, Embutidos) queda corregido
--      automaticamente sin tocar nada mas.
--      El guard de UI (Web/Views/Stock/ExistenciaPorSucursales.cshtml +
--      StockController.cs: AplicarUltimosCierres/ObtenerFechaMinimaExistencia)
--      se deja intacto -- queda como proteccion redundante-pero-inofensiva,
--      no se saca en este cambio (decision de alcance, ver docs/DECISIONS.md).
--
-- Ver docs/DECISIONS.md (2026-08-05, entrada "Egreso Stock negativo...")
-- para el detalle completo y la verificacion contra datos de prueba.
-- ============================================================================

-- Cambio puntual sobre 20260803-Alter_a_ExistenciaStockPorSucursales_PuntoStockPorSucursal.sql:
-- la CTE recursiva #MapaCorte (jerarquia madre/hija de cortes) no filtraba por idEmpresa,
-- a diferencia de #AllCortes que si lo hace. Recorria dbo.Corte completo (catalogo global
-- incluido, ~102K filas) en cada corrida, sin importar que empresa pidio el reporte.
-- Confirmado con el usuario (2026-08-04): un producto y todos sus descendientes siempre
-- pertenecen a una sola empresa, y el catalogo global (idEmpresa=0) solo se usa para copiar
-- valores al dar de alta un producto, sin relacion viva despues. Con esa regla, basta con
-- filtrar el ancla de la recursion (SelfMap, y el nivel 1 de Descendientes/Ascendientes):
-- como un hijo siempre es de la misma empresa que su padre, toda la cadena recursiva que
-- sale de un ancla ya filtrada queda automaticamente dentro de la misma empresa.
CREATE PROCEDURE [dbo].[a_ExistenciaStockPorSucursales]
    @texto nvarchar(50) = '',
    @idEmpresa int = NULL,
    @idSucursal int = 0,
    @fechaHasta datetime = NULL,
    @tipo nvarchar(50) = '',
    @idProveedor int = 0,
    @idMarca int = 0,
    @idCorte int = 0,
    @soloConStock bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @fechaHasta IS NULL
        SET @fechaHasta = GETDATE();

    DECLARE @textoLimpio nvarchar(50) = LTRIM(RTRIM(ISNULL(@texto, '')));

    CREATE TABLE #Sucursales
    (
        idSucursal int NOT NULL PRIMARY KEY,
        sucursal nvarchar(200) NOT NULL,
        FechaUltimoCierre datetime NOT NULL
    );

    -- FechaUltimoCierre ahora acota el MAX() por @fechaHasta (antes: MAX()
    -- incondicional, ignoraba @fechaHasta -- fix Hallazgo 3). La condicion
    -- va en el JOIN, no en el WHERE, para no perder sucursales sin ningun
    -- cierre anterior a @fechaHasta (quedan con FechaUltimoCierre='19000101'
    -- via el ISNULL, mismo comportamiento que hoy cuando no hay cierres).
    INSERT INTO #Sucursales
    (
        idSucursal,
        sucurs
GO
-- ===== a_IngresoEgreso =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[a_IngresoEgreso]
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
    select cast(IngresoStock.codigo as NCHAR(5)) as Codigo ,IngresoStock.corte as 'Corte', IngresoCompras.StockIngreso as 'Compras', IngresoEmbutido.StockIngreso as 'Ingr.Emb',IngresoStock.StockIngreso as 'Ingr.Stock', IngresoMovimiento.StockIngreso as 'Ingr. Mov.',(IngresoCompras.StockIngreso + IngresoEmbutido.StockIngreso + IngresoStock.StockIngreso + IngresoMovimiento.StockIngreso) as 'Tot.INGR' ,EgresoStock.StockIngreso as 'Egr.Stock', EgresoMovimiento.StockIngreso as 'Egr.Mov.', EgresoPorEmbutido.TotalEnEmbutidos as 'Egr.Emb', EgresoVentas.TotalVenta as 'Ventas', (EgresoStock.StockIngreso +  EgresoMovimiento.StockIngreso + EgresoPorEmbutido.TotalEnEmbutidos + EgresoVentas.TotalVenta) as 'Tot.EGR', ((IngresoCompras.StockIngreso + IngresoEmbutido.StockIngreso + IngresoStock.StockIngreso + IngresoMovimiento.StockIngreso) - (EgresoStock.StockIngreso +  EgresoMovimiento.StockIngreso + EgresoPorEmbutido.TotalEnEmbutidos + EgresoVentas.TotalVenta)) as 'DIF'
    	
   -- 	Ingreso.sucursal as 'Sucursal',
			--Ingreso.StockIngreso as 'Total Ingresado', Embutido.TotalEnEmbutidos as 'Kgs en Embutidos', 
			--Egreso.TotalVenta as 'Total Vendido',(Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) as 'Stock Teorico',
			-- CierreStock.StockCierre as 'Stock Real', 
			-- ((Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) - CierreStock.StockCierre ) as 'Faltante'
	from
		(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
		from
		(
			--Seleccion de todos los cortes
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
			FROM         dbo.Corte AS CorteP CROSS JOIN
								  dbo.Sucursal
			WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

			UNION

			--Stock Ingreso

			--++ Cortes ingresados
			(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg) AS StockIngreso
			FROM         dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
								  dbo.Corte AS CorteP ON dbo.CortePorCompra.idCorte = CorteP.idCorte
			WHERE     (CorteP.independiente = 1) AND (dbo.Compras.tipoCompra = 'Ingreso Stock') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND 
								  @fechaHasta+1) AND (dbo.CortePorCompra.idSucursal = @idSucursal)
			GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)

			union
			--++ Suma de los cortes ingresados a su Corte Maestro
			(SELECT     CorteM.idCorte, CorteM.codigo,CorteM.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, 
								  SUM(dbo.CortePorCompra.cantKg + dbo.CortePorCompra.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS StockIngreso
			FROM         dbo.Corte AS CorteP INNER JOIN
								  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
								  dbo.Compras INNER JOIN
								  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
								  dbo.Sucursal ON dbo.CortePorCom
GO
-- ===== A1_CopiarBD_Diferente_Nombre =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[A1_CopiarBD_Diferente_Nombre]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--use master
	--RESTORE DATABASE DB
	--FROM DISK = 'C:\Dropbox\BackUp\20240914SuperCerdoSM.bak'
	--WITH 
 --   MOVE 'SuperCerdo' TO 'c:\Program Files (x86)\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\DB.mdf', -- Cambiar ruta del archivo .mdf
 --   MOVE 'SuperCerdo_log' TO 'c:\Program Files (x86)\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\DB_log.ldf'; -- Cambiar ruta del archivo de log .ldf
 --   --REPLACE; -- Opcional si quieres sobrescribir una base de datos existente con el mismo nombre
 
 ----CIERRA Y ABRE CONEXIONES
 alter database CarniSys_Default set single_user with rollback immediate;
alter database CarniSys_Default set multi_user;
 
 -----MUESTRA NOMBRES LOGICOS
 --RESTORE FILELISTONLY
 --FROM DISK = 'RUTADELARCHIVO.BAK'
 
END


GO
-- ===== A2_Crear_Claves_Foraneas_e_Indices =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[A2_Crear_Claves_Foraneas_e_Indices]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
------/////////////////////////////////////////
--use SuperCerdo
--ALTER TABLE LineaVenta
--ADD CONSTRAINT FK_LineaVenta_Corte FOREIGN KEY (IdCorte)
--REFERENCES Corte (IdCorte);

--select l.idCorte from LineaVenta l where l.idCorte not in (select c.idCorte from Corte c)

--delete from LineaVenta where LineaVenta.idCorte not in (select c.idCorte from Corte c)

------/////////////////////////////////////////////

--ALTER TABLE CortePorEmbutido
--ADD CONSTRAINT FK_CortePorEmbutido_Corte FOREIGN KEY (IdCorte)
--REFERENCES Corte (IdCorte);

--select l.idCorte from CortePorEmbutido l where l.idCorte not in (select c.idCorte from Corte c)

--delete from CortePorEmbutido where CortePorEmbutido.idCorte not in (select c.idCorte from Corte c)

------/////////////////////////////////////////////

--ALTER TABLE CortePorCompra
--ADD CONSTRAINT FK_CortePorCompra_Corte FOREIGN KEY (IdCorte)
--REFERENCES Corte (IdCorte);

--select l.idCorte from CortePorCompra l where l.idCorte not in (select c.idCorte from Corte c)

--delete from CortePorCompra where CortePorCompra.idCorte not in (select c.idCorte from Corte c)

------/////////////////////////////////////////////

--ALTER TABLE CortePorMovimiento
--ADD CONSTRAINT FK_CortePorMovimiento_Corte FOREIGN KEY (IdCorte)
--REFERENCES Corte (IdCorte);

--select l.idCorte from CortePorMovimiento l where l.idCorte not in (select c.idCorte from Corte c)

--delete from CortePorMovimiento where CortePorMovimiento.idCorte not in (select c.idCorte from Corte c)


------/////////////////////////////////////////////

--ALTER TABLE EgresosCaja
--ADD CONSTRAINT FK_EgresosCaja_TiposEgresoCaja FOREIGN KEY (idTipoEgresoCaja)
--REFERENCES TiposEgresoCaja (id);

--select l.idTipoEgresoCaja from EgresosCaja l where l.idTipoEgresoCaja not in (select c.id from TiposEgresoCaja c)

--delete from EgresosCaja where EgresosCaja.idTipoEgresoCaja not in (select c.id from TiposEgresoCaja c)


END


GO
-- ===== A3_VaciarDatosTabla =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[A3_VaciarDatosTabla]
	-- Add the parameters for the stored procedure here
	@ActualizacionCorte tinyint = 0,
	@CierreCaja tinyint = 0,
	@Claves tinyint = 0,
	@Compras tinyint = 0,
	@Corte tinyint = 0,
	@CortePorCompra tinyint = 0,
	@CortePorEmbutido tinyint = 0,
	@CortePorMovimiento tinyint = 0,
	@Embutidos tinyint = 0,
	@EgresosCaja tinyint = 0,
	@FacturaElectronica tinyint = 0,
	@Feriados tinyint = 0,
	@Formulas tinyint = 0,
	@Licencias tinyint = 0,
	@LineaVenta tinyint = 0,
	@MediaRes tinyint = 0,
	@Movimiento tinyint = 0,
	@MovimientoHistorial tinyint = 0,
	@MovCtaCte tinyint = 0,
	@Pagos tinyint = 0,
	@Personas tinyint = 0,
	@Proveedores tinyint = 0,
	@StockCorteSucursal tinyint = 0,
	@Sucursal tinyint = 0,
	@TemporalLineaVenta tinyint = 0,
	@Usuarios tinyint = 0,
	@VencimientosLicencia tinyint = 0,
	@Ventas tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @ActualizacionCorte = 1
		truncate table ActualizacionCorte		
	
		
	if @CierreCaja = 1
		truncate table CierreCaja 
			
	if @Claves = 1
		truncate table Claves		
	
	if @Compras = 1
		truncate table Compras
		
		
	if @CortePorCompra = 1
		truncate table CortePorCompra
		
	if @CortePorEmbutido = 1
		truncate table CortePorEmbutido
		
	if @CortePorMovimiento = 1
		truncate table CortePorMovimiento
		
	if @Embutidos = 1
		truncate table Embutidos
		
	if @EgresosCaja = 1
		truncate table EgresosCaja 

	
	if @FacturaElectronica = 1
		truncate table FacturaElectronica 

	if @Feriados = 1
		truncate table Feriados
		
	if @Formulas = 1
		truncate table Formulas 
		
	if @Licencias = 1
		truncate table Licencias 
		
	if @LineaVenta = 1
		truncate table LineaVenta
		
	if @MediaRes = 1
		truncate table MediaRes
		
	if @Movimiento = 1
		truncate table Movimiento
		
	if @MovimientoHistorial = 1
		truncate table MovimientoHistorial 

	if @MovCtaCte = 1
		truncate table MovCtaCte 

	if @Pagos = 1
		truncate table Pagos
		
		
	if @Proveedores = 1
		truncate table Proveedores
		
	if @StockCorteSucursal = 1
		truncate table StockCorteSucursal
		
	if @Sucursal = 1
		truncate table Sucursal
		
		
	if @TemporalLineaVenta = 1
		truncate table TemporalLineaVenta 
		
	if @VencimientosLicencia = 1
		truncate table VencimientosLicencia 
		
		
	if @Ventas = 1
		truncate table Ventas
		
	if @Corte = 1
		truncate table Corte
		
	if @Personas = 1
		truncate table Personas
		
	if @Usuarios = 1
		truncate table Usuarios
		
END


GO
-- ===== AA_AltaEmpresa =====

CREATE   PROCEDURE [dbo].[AA_AltaEmpresa]
(
    @razonSocialAfip        NVARCHAR(100) = NULL,
    @cuit                   BIGINT        = NULL,
    @nombreFantasia         NVARCHAR(100) = NULL,
    @slogan1                NVARCHAR(MAX) = NULL,
    @slogan2                NVARCHAR(MAX) = NULL,
    @slogan3                NVARCHAR(MAX) = NULL,
    @iibb                   BIGINT        = NULL,
    @condicionIVA           NVARCHAR(100) = NULL,
    @inicioActividad        DATE          = NULL,
    @tenantSlug             NVARCHAR(100) = NULL,
    @domicilio              NVARCHAR(100) = NULL,
    @ciudad                 NVARCHAR(100) = NULL,
    @pais                   NVARCHAR(100) = NULL,
    @telefono               NVARCHAR(100) = NULL,
    @email                  NVARCHAR(100) = NULL,
    @basePath               NVARCHAR(100) = NULL,
    @esRRII                 TINYINT       = 0,
    @nombreCertificado_pfx  NVARCHAR(100) = NULL,
    @entorno_HOMO_PROD      NVARCHAR(100) = NULL,
    @baseDatosNombre        NVARCHAR(100) = NULL,
    @activa                 TINYINT       = 1,
    @creado					DATE          = sysdatetime,
    @observaciones          NVARCHAR(MAX) = NULL,
    @idEmpresa              INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Evita colisiones por concurrencia (mismo CUIT / mismo id libre)
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRAN;

    -------------------------------------------------------------------
    -- 0) Validar que no exista otra empresa con el mismo CUIT (si viene)
    -------------------------------------------------------------------
    IF @cuit IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM dbo.Empresas WITH (UPDLOCK, HOLDLOCK)
            WHERE cuit = @cuit
        )
        BEGIN
            THROW 50010, 'Ya existe una empresa con ese mismo CUIT.', 1;
        END
    END

    -------------------------------------------------------------------
    -- 1) Obtener el primer idEmpresa positivo disponible (más bajo)
    --    Si no hay huecos: usa MAX+1
    -------------------------------------------------------------------
    DECLARE @nuevoId INT;

    IF NOT EXISTS (SELECT 1 FROM dbo.Empresas WITH (UPDLOCK, HOLDLOCK) WHERE idEmpresa = 1)
    BEGIN
        SET @nuevoId = 1;
    END
    ELSE
    BEGIN
        SELECT TOP (1) @nuevoId = e.idEmpresa + 1
        FROM dbo.Empresas e WITH (UPDLOCK, HOLDLOCK)
        WHERE e.idEmpresa >= 1
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.Empresas e2 WITH (UPDLOCK, HOLDLOCK)
              WHERE e2.idEmpresa = e.idEmpresa + 1
          )
        ORDER BY e.idEmpresa;

        -- Si no encontró hueco, usa MAX+1
        IF @nuevoId IS NULL
            SELECT @nuevoId = MAX(idEmpresa) + 1
            FROM dbo.Empresas WITH (UPDLOCK, HOLDLOCK)
            WHERE idEmpresa >= 1;
    END

    SET @idEmpresa = @nuevoId;

    -------------------------------------------------------------------
    -- 2) Insertar Empresa
    -------------------------------------------------------------------
    INSERT INTO dbo.Empresas
    (
        idEmpresa, razonSocialAfip, cuit, nombreFantasia,
        slogan1, slogan2, slogan3,
        iibb, condicionIVA, inicioActividad,
        tenantSlug, domicilio, ciudad, pais,
        telefono, email, basePath,
        esRRII, nombreCertificado_pfx, entorno_HOMO_PROD,
        baseDatosNombre, activa, creado, observaciones
    )
    VALUES
    (
        @idEmpresa, @razonSocialAfip, @cuit, @nombreFantasia,
        @slogan1, @slogan2, @slogan3,
        @iibb, @condicionIVA, @inicioActividad,
        @tenantSlug, @domicilio, @ciudad, @pais,
        @telefono, @email, @basePath,
        @esRRII, @nombreCertificado_pfx, @entorno_HOMO_PROD,
        @baseDatosNombre, @activa, @creado, @observaciones
    );

    -----------------------------
GO
-- ===== achicaLog =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[achicaLog]
AS
    -- Insert statements for procedure here
     /*
	go
	alter database SuperCerdo
	set recovery simple
	go
	dbcc shrinkfile (supercerdo_Log)
	go 
	alter database SuperCerdo
	set recovery full
	go
	*/

GO
-- ===== Acum_Ventas =====

CREATE PROCEDURE [dbo].[Acum_Ventas]
    @texto nvarchar(50),
    @idSucursal int = 0,
    @fechaDesde datetime,
    @fechaHasta datetime,
    @tipo nvarchar(50),
    @idProveedor int,
    @idMarca int,
    @idEmpresa int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @esNumero int = ISNUMERIC(@texto);
    DECLARE @idSucursalFiltro int = ISNULL(@idSucursal, 0);
    DECLARE @idEmpresaFiltro int = ISNULL(@idEmpresa, 0);

    SELECT
        CAST(AllCortes.codigo AS NCHAR(20)) AS Codigo,
        AllCortes.corte AS Corte,
        0.00 AS StockActual,
        EgresoVentas.TotalVenta AS Ventas,
        0.00 AS DIF
    FROM
    (
        SELECT
            CorteP.idCorte AS idCorte,
            CorteP.codigo,
            CorteP.corte,
            s.idSucursal,
            s.sucursal,
            0.00 AS StockIngreso
        FROM dbo.Corte AS CorteP
        LEFT OUTER JOIN dbo.CorteProveedor cp
            ON CorteP.idCorte = cp.idCorte
        CROSS JOIN dbo.Sucursal s
        WHERE
            CorteP.independiente = 1
            AND
            (
                (@idSucursalFiltro > 0 AND s.idSucursal = @idSucursalFiltro)
                OR
                (@idSucursalFiltro = 0 AND @idEmpresaFiltro > 0 AND s.idEmpresa = @idEmpresaFiltro)
            )
            AND (@idEmpresaFiltro = 0 OR s.idEmpresa = @idEmpresaFiltro)
            AND (@idEmpresaFiltro = 0 OR CorteP.idEmpresa = @idEmpresaFiltro)
            AND (@tipo = '' OR @tipo IS NULL OR CorteP.tipo = @tipo)
            AND (@idProveedor = 0 OR @idProveedor IS NULL OR cp.idProveedor = @idProveedor)
            AND (@idMarca = 0 OR @idMarca IS NULL OR CorteP.idMarca = @idMarca)
        GROUP BY
            CorteP.idCorte,
            CorteP.codigo,
            CorteP.corte,
            s.idSucursal,
            s.sucursal
    ) AS AllCortes
    LEFT OUTER JOIN
    (
        SELECT
            idCorte,
            codigo,
            corte,
            idSucursal,
            sucursal,
            SUM(TotalVenta) AS TotalVenta
        FROM
        (
            SELECT
                c.idCorte,
                c.codigo,
                c.corte,
                s.idSucursal,
                s.sucursal,
                SUM(lv.cantKg - lv.kgsAjusteTarj) AS TotalVenta
            FROM dbo.Ventas v
            INNER JOIN dbo.LineaVenta lv
                ON v.idVenta = lv.idVenta
            INNER JOIN dbo.Sucursal s
                ON v.idSucursal = s.idSucursal
            INNER JOIN dbo.Corte c
                ON lv.idCorte = c.idCorte
            WHERE
                v.fechaVenta BETWEEN @fechaDesde AND @fechaHasta
                AND
                (
                    (@idSucursalFiltro > 0 AND v.idSucursal = @idSucursalFiltro)
                    OR
                    (@idSucursalFiltro = 0 AND @idEmpresaFiltro > 0 AND s.idEmpresa = @idEmpresaFiltro)
                )
                AND (@idEmpresaFiltro = 0 OR s.idEmpresa = @idEmpresaFiltro)
                AND c.independiente = 1
                AND (@idEmpresaFiltro = 0 OR c.idEmpresa = @idEmpresaFiltro)
            GROUP BY
                s.idSucursal,
                s.sucursal,
                c.idCorte,
                c.codigo,
                c.corte

            UNION

            SELECT
                c.idCorte,
                c.codigo,
                c.corte,
                s.idSucursal,
                s.sucursal,
                SUM((lv.cantKg - lv.kgsAjusteTarj) + (lv.cantKg - lv.kgsAjusteTarj) * CorteP.porcentajeHueso / CorteP.porcentaje) AS TotalVenta
            FROM dbo.Ventas v
            INNER JOIN dbo.LineaVenta lv
                ON v.idVenta = lv.idVenta
            INNER JOIN dbo.Sucursal s
                ON v.idSucursal = s.idSucursal
            INNER JOIN dbo.Corte AS CorteP
                ON lv.idCorte = CorteP.idCorte
            INNER JOIN dbo.Corte c
          
GO
-- ===== addOrEditCierreCaja =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditCierreCaja] 
	-- Add the parameters for the stored procedure here
	@id	int = 0,
	@idCierreAnterior int = 0,
	@idSucursal	int,
	@fechaHoraInicio datetime,
	@fechaHoraCierre datetime = null,
	@cajaInicio	float,
	@ventas	float = null,
	@gastos	float = null,
	@cajaCierre	float = null,
	@diferencia	float = null,
	@cajaInicioSiguiente float = null,
	@importeRetirado float = null,
	@usuarioInicio int = null,
	@usuarioCierre int = null,
	@creado	datetime = null,
	@actualizado datetime = null,
	@idEncontrado int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- int,erfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
      IF(@id = 0 or @id is null)
            BEGIN 
				set @idCierreAnterior  = (select top 1 CierreCaja.id from CierreCaja where CierreCaja.idSucursal = @idSucursal order by CierreCaja.id desc) 
				IF(@idCierreAnterior is null or @idCierreAnterior < (10000000 * @idSucursal)) --CONDITION
					 BEGIN
						set @idCierreAnterior = (10000000 * @idSucursal)
					 END
                 set @id = @idCierreAnterior + 1
                 
                insert into CierreCaja (id, idSucursal, fechaHoraInicio, fechaHoraCierre, cajaInicio, ventas, gastos, cajaCierre, diferencia, cajaInicioSiguiente, importeRetirado, usuarioInicio, usuarioCierre, creado, actualizado)
						values (@id, @idSucursal, @fechaHoraInicio, @fechaHoraCierre, @cajaInicio, @ventas, @gastos, @cajaCierre, @diferencia, @cajaInicioSiguiente, @importeRetirado, @usuarioInicio, @usuarioCierre, SYSDATETIME(),'')
            END
       ELSE
		BEGIN
			set @idEncontrado = (select CierreCaja.id from CierreCaja where CierreCaja.id = @id and CierreCaja.idSucursal = @idSucursal)		
			IF(@idEncontrado > 0)
				BEGIN 
					UPDATE CierreCaja
					 SET idSucursal = @idSucursal,fechaHoraInicio = @fechaHoraInicio,fechaHoraCierre = @fechaHoraCierre,
						cajaInicio = @cajaInicio, ventas = @ventas,gastos = @gastos, cajaCierre = @cajaCierre,diferencia = @diferencia,
						cajaInicioSiguiente = @cajaInicioSiguiente,importeRetirado = @importeRetirado,usuarioInicio = @usuarioInicio,
						usuarioCierre = @usuarioCierre,actualizado = SYSDATETIME()
					 WHERE id = @id
				END	
			ELSE
				BEGIN 
					insert into CierreCaja (id, idSucursal, fechaHoraInicio, fechaHoraCierre, cajaInicio, ventas, gastos, cajaCierre, diferencia, cajaInicioSiguiente, importeRetirado, usuarioInicio, usuarioCierre, creado, actualizado)
							values (@id, @idSucursal, @fechaHoraInicio, @fechaHoraCierre, @cajaInicio, @ventas, @gastos, @cajaCierre, @diferencia, @cajaInicioSiguiente, @importeRetirado, @usuarioInicio, @usuarioCierre, SYSDATETIME(),'')
				END			
		END           

	select top 1 CierreCaja.id from CierreCaja where CierreCaja.idSucursal = @idSucursal order by CierreCaja.id desc
	
END

GO
-- ===== addOrEditCompra =====

CREATE PROCEDURE [dbo].[addOrEditCompra]
    @idCompra int = 0,
    @nroRemito nvarchar(50) = '',
    @fechaCompra datetime,
    @idProveedor int,
    @cantMedias int = null,
    @kgsMedias int = null,
    @estado nvarchar(50),
    @observaciones nvarchar(max),
    @tipoCompra nvarchar(50),
    @idSucursal int,
    @creadoPor int = null,
    @actualizadoPor int = null,
    @enCtaCte tinyint = 0,
    @idPesajeAjustado int = null
AS
BEGIN
    SET NOCOUNT ON;

    IF @idCompra = 0
    BEGIN
        INSERT INTO Compras
        (
            nroRemito,
            fechaCompra,
            idProveedor,
            estado,
            observaciones,
            tipoCompra,
            cantMedias,
            kgsMedias,
            enCtaCte,
            idSucursal,
            creado,
            creadoPor,
            idPesajeAjustado
        )
        VALUES
        (
            @nroRemito,
            @fechaCompra,
            @idProveedor,
            @estado,
            @observaciones,
            @tipoCompra,
            @cantMedias,
            @kgsMedias,
            @enCtaCte,
            @idSucursal,
            SYSDATETIME(),
            @creadoPor,
            @idPesajeAjustado
        );

        SET @idCompra = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE Compras
        SET nroRemito = @nroRemito,
            fechaCompra = @fechaCompra,
            idProveedor = @idProveedor,
            cantMedias = @cantMedias,
            kgsMedias = @kgsMedias,
            estado = @estado,
            observaciones = @observaciones,
            tipoCompra = @tipoCompra,
            enCtaCte = @enCtaCte,
            idSucursal = @idSucursal,
            actualizado = SYSDATETIME(),
            actualizadoPor = @actualizadoPor,
            idPesajeAjustado = @idPesajeAjustado
        WHERE idCompra = @idCompra;

        DELETE FROM CortePorCompra WHERE CortePorCompra.idCompra = @idCompra;
        DELETE FROM MediaRes WHERE MediaRes.idCompra = @idCompra;
    END

    SELECT @idCompra;
END

GO
-- ===== addOrEditCorte =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditCorte]
	-- Add the parameters for the stored procedure here
	@idCorte int = null,
	@codigo bigint,
	@corte nvarchar(50),
	@precioKg float,
	@ingresoRapidoEmbutido tinyint,
	@habilitado tinyint,
	@enCierreStock tinyint,
	@tipo nvarchar(50),
	@independiente int,
	@idCorteMaestro int,
	@porcentaje float,
	@porcentajeHueso float,
	@desvioEstandar float,
	@promedio float,
	@idAlicuotaIva int,
	@alicuotaIva float,
	@pesable tinyint,
	@nivel int = 0,
	@puntoStock int = 0,
	@idMarca int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE  @idMaestro int;
	DECLARE @id_deCodigo_encontrado int = 0;

	
	---Si existe idCorte es 0 y Codigo existe es porque se está llamando desde una Importacion	
	IF @idCorte = 0
    BEGIN
		set @id_deCodigo_encontrado = (SELECT TOP (1) idCorte FROM  dbo.Corte where codigo = @codigo)
    END 
	IF @id_deCodigo_encontrado > 0
    BEGIN
		set @idCorte = @id_deCodigo_encontrado
	END
	--FIN de Validacion por Importacion csv


	---Se calcula el Nivel del corte segun su Corte Maestro	
	IF (@idCorteMaestro > 0)
		begin
			set @idMaestro = (SELECT     TOP (1) idCorte
				FROM         dbo.Corte
				WHERE     (@idCorteMaestro IN
						  (SELECT     idCorte
							FROM          dbo.Corte AS Corte_n2
							WHERE      (idCorteMaestro IN
									   (SELECT     idCorte
										 FROM          dbo.Corte AS Corte_n1
										 WHERE      (idCorteMaestro IN
									(SELECT     idCorte
									  FROM          dbo.Corte AS Corte_n0
									  WHERE      (idCorteMaestro IN
										(SELECT     idCorte
										  FROM          dbo.Corte AS Corte_n)))))))))
			IF (@idMaestro > 0)
				begin
					set @nivel = 4;
				end
			
			else
			 begin							  
				set @idMaestro = (SELECT TOP 1 [idCorte]
				  FROM Corte
				  where @idCorteMaestro in 
				  (select Corte_n2.idCorte from Corte as Corte_n2 
					where Corte_n2.idCorteMaestro in 
					(select Corte_n1.idCorte from Corte as Corte_n1
					where Corte_n1.idCorteMaestro in 
						(select Corte_n0.idCorte from Corte as Corte_n0))))
						
				--Si encontró maestro se setea Nivel = 3
				IF (@idMaestro > 0)
					begin
						set @nivel = 3;
					end
				
				else
					begin
						set @idMaestro = (SELECT TOP 1 [idCorte]
						  FROM Corte
						  where @idCorteMaestro in 
						  (select Corte_n1.idCorte from Corte as Corte_n1 
							where Corte_n1.idCorteMaestro in (select Corte_n0.idCorte from Corte as Corte_n0)))
								
						--Si encontró maestro se setea Nivel = 2
						IF (@idMaestro > 0)
						begin
							set @nivel = 2;
						end
						
						else
							begin
								set @idMaestro = (SELECT TOP 1 [idCorte]
								  FROM Corte
								  where @idCorteMaestro in (select Corte_n0.idCorte from Corte as Corte_n0))
										
								--Si encontró maestro se setea Nivel = 3
								IF (@idMaestro > 0)
								begin
									set @nivel = 1;
								end
							end
					end
				end
		end
	----FIN calculo de Nivel en Corte
	

    -- Edit
	IF (@idCorte > 0)
		begin
			update Corte set codigo=@codigo,corte=@corte, precioKg=@precioKg, tipo=@tipo,
			ingresoRapidoEmbutido = @ingresoRapidoEmbutido, habilitado = @habilitado, enCierreStock = @enCierreStock, independiente=@independiente,idCorteMaestro=@idCorteMaestro, porcentaje=@porcentaje,
			porcentajeHueso=@porcentajeHueso,desvioEstandar=@desvioEstandar, promedio = @promedio, puntoStock = @puntoStock, idAlicuotaIva = @idAlicuotaIva, alicuotaIva = @alicuotaIva, pesable = @pesable, nivel = @nivel, idMarca = @idMarca, actualizado = SYSDATETIME() 
			where idCorte=@idCorte

			-----Se crea el registro del h
GO
-- ===== addOrEditEgresoCaja =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditEgresoCaja]
	@id int = 0,
	@fecha datetime = null,
	@idTipoEgresoCaja int,
	@descripcion nvarchar(MAX),
	@detalle nvarchar(MAX),
	@monto float,
	@idCompra int = null,
	@tabla nvarchar(50) = null,
	@idTabla int = null,
	@esGasto tinyint = null,
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
			set @esGasto = (select esGasto from TiposEgresoCaja where TiposEgresoCaja.id = @idTipoEgresoCaja)
			
			insert into dbo.EgresosCaja (fechaHora,idTipoEgresoCaja,descripcion,detalle,monto, idCompra, tabla, idTabla, esGasto, idSucursal,creado,creadoPor)		
			values (@fecha,@idTipoEgresoCaja,@descripcion,@detalle,@monto,@idCompra, @tabla, @idTabla, @esGasto, @idSucursal, SYSDATETIME(),@creadoPor)
			set @id = SCOPE_IDENTITY() 		
		END
	ELSE
		BEGIN
			--Si @esGasto es null
			IF (@id <> 0 and @id <> 1)
				BEGIN
					set @esGasto = (select TiposEgresoCaja.esGasto from TiposEgresoCaja where TiposEgresoCaja.id = @idTipoEgresoCaja)
				END
			
			update dbo.EgresosCaja 
			set  fechaHora = @fecha,idTipoEgresoCaja = @idTipoEgresoCaja,descripcion = @descripcion,
				detalle = @detalle,monto = @monto, idCompra = @idCompra, tabla = @tabla, idTabla = @idTabla, esGasto = @esGasto, idSucursal = @idSucursal,	actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor
			where id = @id
		END
	
	select @id
END

GO
-- ===== addOrEditFacturaElectronica =====

CREATE PROCEDURE [dbo].[addOrEditFacturaElectronica]
    @id int = null,
    @ptoVtaAfip nvarchar(50) = null,
    @fechaEmisionAfip nvarchar(50) = null,
    @descTipoCbteAfip nvarchar(50) = null,
    @codTipoCbteAfip int = null,
    @nroCbteAfip nvarchar(50) = null,
    @tipoDocAfip nvarchar(50) = null,
    @nroDocAfip nvarchar(50) = null,
    @razonSocialAFIP nvarchar(50) = null,
    @condicionIvaAFIP nvarchar(50) = null,
    @domicilioAFIP nvarchar(50) = null,
    @condicionVenta nvarchar(50) = null,
    @formaPago nvarchar(50) = null,
    @CAE nvarchar(50) = null,
    @fecVtoCAE nvarchar(50) = null,
    @importeNetoGravado float = null,
    @iva float = null,
    @importeTotal float = null,
    @PorcentajeFacturacion float = 100,
    @descItemUnitario nvarchar(200) = null,
    @observaciones nvarchar(500) = null,
    @idVenta int = null,
    @error tinyint = null,
    @mensajeError nvarchar(MAX) = null,
    @fechaError nvarchar(50) = null,
    @cantErrores int = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @id = 0
    BEGIN
        INSERT INTO dbo.FacturaElectronica
        (
            ptoVtaAfip,
            fechaEmisionAfip,
            descTipoCbteAfip,
            codTipoCbteAfip,
            nroCbteAfip,
            tipoDocAfip,
            nroDocAfip,
            razonSocialAFIP,
            condicionIvaAFIP,
            domicilioAFIP,
            condicionVenta,
            formaPago,
            CAE,
            fecVtoCAE,
            importeNetoGravado,
            iva,
            importeTotal,
            porcentajeFacturacion,
            descItemUnitario,
            observaciones,
            idVenta,
            creado,
            error,
            mensajeError,
            fechaError,
            cantErrores
        )
        VALUES
        (
            @ptoVtaAfip,
            @fechaEmisionAfip,
            @descTipoCbteAfip,
            @codTipoCbteAfip,
            @nroCbteAfip,
            @tipoDocAfip,
            @nroDocAfip,
            @razonSocialAFIP,
            @condicionIvaAFIP,
            @domicilioAFIP,
            @condicionVenta,
            @formaPago,
            @CAE,
            @fecVtoCAE,
            @importeNetoGravado,
            @iva,
            @importeTotal,
            @PorcentajeFacturacion,
            @descItemUnitario,
            ISNULL(@observaciones, ''),
            @idVenta,
            SYSDATETIME(),
            @error,
            @mensajeError,
            @fechaError,
            @cantErrores
        );

        SET @id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SET @cantErrores = 0;

        IF @CAE IS NULL OR @CAE LIKE ''
        BEGIN
            SET @cantErrores = (
                SELECT FacturaElectronica.cantErrores + 1
                FROM FacturaElectronica
                WHERE id = @id
            );
        END

        UPDATE dbo.FacturaElectronica
        SET
            ptoVtaAfip = @ptoVtaAfip,
            fechaEmisionAfip = fechaEmisionAfip,
            descTipoCbteAfip = @descTipoCbteAfip,
            codTipoCbteAfip = @codTipoCbteAfip,
            nroCbteAfip = @nroCbteAfip,
            tipoDocAfip = @tipoDocAfip,
            nroDocAfip = @nroDocAfip,
            razonSocialAFIP = @razonSocialAFIP,
            condicionIvaAFIP = @condicionIvaAFIP,
            domicilioAFIP = @domicilioAFIP,
            condicionVenta = @condicionVenta,
            formaPago = @formaPago,
            CAE = @CAE,
            fecVtoCAE = @fecVtoCAE,
            importeNetoGravado = @importeNetoGravado,
            iva = @iva,
            importeTotal = @importeTotal,
            porcentajeFacturacion = @PorcentajeFacturacion,
            descItemUnitario = @descItemUnitario,
            observaciones = ISNULL(@observaciones, ''),
            idVenta = @idVenta,
            error = @error,
            mensajeError = @mensaje
GO
-- ===== addOrEditFormula =====

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditFormula] 
	
	@idFormula int = null,
	@idEmbutido int,
	@receta nvarchar(max),
	@creadoPor int = null,
	@actualizadoPor int = null
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
     
	 -- Insert statements for procedure here
	IF @idFormula = 0
		BEGIN
			INSERT INTO [Formulas]
           ([idEmbutido]
		   ,[receta]
           ,[creado]
           ,[creadoPor])
     VALUES
            (@idEmbutido
			,@receta
           ,SYSDATETIME()
           ,@creadoPor)
           
			set @idFormula = SCOPE_IDENTITY() 		
		END
	ELSE
		BEGIN
		UPDATE [Formulas]
		   SET [idEmbutido] = @idEmbutido
			  ,[receta] = @receta
			  ,[actualizado] = SYSDATETIME()
			  ,[actualizadoPor] = @actualizadoPor
		 WHERE idFormula = @idFormula
		 
		 --Se eliminan todo los Cortes en Formula Para volver a cargarlos
		 delete from CortePorFormula where idFormula=@idFormula
		  
		END
	
	select @idFormula	
	
END

GO
-- ===== addOrEditGasto =====

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
-- ===== addOrEditMovCtaCte =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditMovCtaCte] 
	-- Add the parameters for the stored procedure here
	@id int = 0,
	@idPersona int = null,
	@fecha datetime = null,
	@tabla nvarchar(50) = null,
	@idTabla int = null,
	@nroDoc nvarchar(50) = null,
	@detalle nvarchar(MAX) = null,
	@tipo nvarchar(50) = null,
	@importe float = null,
	@quitadoCtaCte tinyint = null,
	@idSucursal int = null,
	@creado datetime = null,
	@creadoPor int = null,
	@actualizado datetime = null,
	@actualizadoPor int = null

AS
BEGIN
	SET NOCOUNT ON;
	IF @id = 0 
		BEGIN 
			INSERT INTO dbo.MovCtaCte (idPersona, fecha, tabla, idTabla, nroDoc, detalle, tipo, importe, quitadoCtaCte, idSucursal, creado, creadoPor) 
 VALUES (@idPersona, @fecha, @tabla, @idTabla, @nroDoc, @detalle, @tipo, @importe, @quitadoCtaCte, @idSucursal, SYSDATETIME(), @creadoPor) 
			set @id = SCOPE_IDENTITY()
		END 
	ELSE 
		BEGIN 
			UPDATE dbo.MovCtaCte set idPersona =  @idPersona, fecha =  @fecha, tabla =  @tabla, idTabla =  @idTabla, nroDoc = @nroDoc ,detalle =  @detalle, tipo =  @tipo, importe =  @importe, quitadoCtaCte = @quitadoCtaCte, idSucursal =  @idSucursal, actualizado =  SYSDATETIME(), actualizadoPor =  @actualizadoPor
			WHERE id = @id		
		END 
	select @id 

END

GO
-- ===== addOrEditMovimiento =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditMovimiento]
	-- Add the parameters for the stored procedure here	
	@idMovimiento int = 0,
	@fechaMovimiento datetime,
	@sucursalOrigen int,
	@sucursalDestino int,
	@observaciones nvarchar(max),
	@creadoPor int = null,
	@actualizadoPor int = null,
	@EstadoPendiente_valor int = 2
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @idMovimiento = 0
		begin
			-- Agrego el movimiento
			insert into Movimiento (fechaMovimiento,sucursalOrigen,sucursalDestino, actualizacionCompleta,observaciones, creado, creadoPor)
			values (@fechaMovimiento,@sucursalOrigen,@sucursalDestino, @EstadoPendiente_valor,@observaciones, SYSDATETIME(), @creadoPor)
			
			select top 1 idMovimiento from Movimiento order by idMovimiento desc
		end
	else
		begin
		
			--Se crea registro de historial
			insert into MovimientoHistorial (idMovimiento, FechaMovimiento, idSucOrigen, idSucDestino, idCorte, cantKg, cantUnidad, pesoBalanza, actualizadoPor, actualizado, observaciones)

SELECT     dbo.Movimiento.idMovimiento, dbo.Movimiento.fechaMovimiento, dbo.Movimiento.sucursalOrigen, dbo.Movimiento.sucursalDestino, 
					  dbo.CortePorMovimiento.idCorte, dbo.CortePorMovimiento.cantKg, dbo.CortePorMovimiento.cantUnidad, dbo.CortePorMovimiento.pesoBalanza, 
					  @actualizadoPor, SYSDATETIME(), dbo.Movimiento.observaciones
FROM         dbo.Movimiento INNER JOIN
					  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
where Movimiento.idMovimiento = @idMovimiento				
			
			--se actualiza los datos del movimiento
			update Movimiento set fechaMovimiento=@fechaMovimiento,sucursalOrigen=@sucursalOrigen,sucursalDestino=@sucursalDestino,observaciones=@observaciones, actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor
	where idMovimiento=@idMovimiento
	
			--se establece el valor para ActualizacionCompleta
			update Movimiento set actualizacionCompleta=@EstadoPendiente_valor
			where idMovimiento=@idMovimiento and idMovOrigen IS NULL
	
			--se eliminan todos los cortes en el movimiento
			delete from CortePorMovimiento where idMovimientos=@idMovimiento
	
		end
END

GO
-- ===== addOrEditPago =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[addOrEditPago]
--ALTER PROCEDURE addOrEditPago
	-- Add the parameters for the stored procedure here

		@id int = null,
		@nroRecibo nvarchar(50) = null,
		@fecha datetime = null,
		@idPersona int = null,
		@aProveedor tinyint = null,
		@formaPago nvarchar(50) = null,
		@banco nvarchar(50) = null,
		@nroCheque nvarchar(50) = null,
		@titularCheque nvarchar(50) = null,
		@importe float = null,
		@efectivo float = null,
		@observaciones nvarchar(MAX) = null,
		@idSucursal int = null,
		@creado datetime = null,
		@creadoPor int = null,
		@actualizado datetime = null,
		@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
		IF @id = 0 
			BEGIN 
				INSERT INTO dbo.Pagos (nroRecibo, fecha, idPersona, aProveedor, formaPago, 
				banco, nroCheque, titularCheque, importe, efectivo, observaciones, idSucursal, creado, 
				creadoPor) 
				 VALUES (@nroRecibo, @fecha, @idPersona, @aProveedor, @formaPago, @banco, @nroCheque, 
				 @titularCheque, @importe, @efectivo, @observaciones, @idSucursal, SYSDATETIME(), @creadoPor) 
			set @id = SCOPE_IDENTITY()
			END 
		ELSE 
		BEGIN 
			UPDATE dbo.Pagos set nroRecibo = @nroRecibo, fecha = @fecha, idPersona = @idPersona, 
			aProveedor = @aProveedor, formaPago = @formaPago, banco = @banco, nroCheque = @nroCheque, 		
			titularCheque = @titularCheque, importe = @importe, efectivo = @efectivo, observaciones = @observaciones,
			idSucursal =  @idSucursal, actualizado =  SYSDATETIME(), actualizadoPor =  @actualizadoPor
			WHERE id = @id		
		END 
	select @id 
END

GO
-- ===== addOrEditPersona =====

CREATE PROCEDURE [dbo].[addOrEditPersona]
    @idPersona int = NULL,
    @identificacion nvarchar(50) = NULL,
    @razonSocial nvarchar(50) = NULL,
    @idIva int = NULL,
    @cuit nvarchar(50) = NULL,
    @telefono nvarchar(50) = NULL,
    @email nvarchar(200) = NULL,
    @domicilio nvarchar(50) = NULL,
    @ciudad nvarchar(50) = NULL,
    @otrosDatos nvarchar(200) = NULL,
    @tipo nvarchar(50) = NULL,
    @ctaCte tinyint = NULL,
    @bonificacion float = NULL,
    @marca bit = 0,
    @idPropietario int = NULL,
    @idEmpresa int = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @idPersona = 0
    BEGIN
        INSERT INTO dbo.Personas
        (
            identificacion,
            razonSocial,
            idIva,
            cuit,
            telefono,
            email,
            domicilio,
            ciudad,
            otrosDatos,
            tipo,
            ctaCte,
            bonificacion,
            marca,
            idPropietario,
            idEmpresa
        )
        VALUES
        (
            @identificacion,
            @razonSocial,
            @idIva,
            @cuit,
            @telefono,
            @email,
            @domicilio,
            @ciudad,
            @otrosDatos,
            @tipo,
            @ctaCte,
            @bonificacion,
            ISNULL(@marca, 0),
            @idPropietario,
            ISNULL(@idEmpresa, 0)
        );

        SET @idPersona = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Personas
        SET
            identificacion = @identificacion,
            razonSocial = @razonSocial,
            idIva = @idIva,
            cuit = @cuit,
            telefono = @telefono,
            email = @email,
            domicilio = @domicilio,
            ciudad = @ciudad,
            tipo = @tipo,
            otrosDatos = @otrosDatos,
            ctaCte = @ctaCte,
            bonificacion = @bonificacion,
            marca = ISNULL(@marca, 0),
            idPropietario = @idPropietario
        WHERE idPersona = @idPersona;
    END

    SELECT @idPersona;
END

GO
-- ===== addOrEditUser =====

CREATE PROCEDURE [dbo].[addOrEditUser]
    @id int = null,
    @nombre varchar(50),
    @usuario varchar(50),
    @clave varchar(50),
    @email varchar(100) = '',
    @admin tinyint,
    @activo tinyint,
    @colorForm varchar(50),
    @IdEmpresa int = null,
    @idSucursal int = 0
AS
BEGIN
    SET NOCOUNT ON;
	
    -- Leer desde SESSION_CONTEXT si no te lo pasan
    IF @idEmpresa IS NULL
        SET @idEmpresa = TRY_CAST(SESSION_CONTEXT(N'IdEmpresa') AS int);

    IF @idEmpresa IS NULL
        THROW 50030, 'No está seteado IdEmpresa en SESSION_CONTEXT.', 1;

    SET XACT_ABORT ON; -- si algo falla, aborta la transacción

    BEGIN TRY
        BEGIN TRAN;

        IF (@id IS NULL OR @id = 0)
        BEGIN
            -- OJO: MAX+1 puede colisionar con concurrencia; ideal sería IDENTITY o SEQUENCE.
            SELECT @id = ISNULL(MAX(id), 0) + 1
            FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK);

            -- Si no viene sucursal, tomo la primera de la empresa
            IF (@idSucursal IS NULL OR @idSucursal = 0)
            BEGIN
                SELECT TOP (1) @idSucursal = idSucursal
                FROM dbo.Sucursal
                WHERE idEmpresa = @IdEmpresa
                ORDER BY idSucursal; -- para que sea determinístico
            END

            -- Validaciones mínimas para evitar FK / datos inválidos
            IF (@IdEmpresa IS NULL OR @IdEmpresa = 0)
                THROW 50020, 'IdEmpresa inválido.', 1;

            IF (@idSucursal IS NULL OR @idSucursal = 0)
                THROW 50021, 'No existe sucursal para la empresa indicada.', 1;

            INSERT INTO dbo.Usuarios
                (id, nombre, usuario, clave, email, admin, activo, colorForm, idSucursalUser, idEmpresa)
            VALUES
                (@id, @nombre, @usuario, @clave, @email, @admin, @activo, @colorForm, @idSucursal, @IdEmpresa);

            -- Permisos por defecto
            INSERT INTO dbo.PermisosUsuarios
                (idUsuario, idForm, diasPermitidosVer, diasPermitidosEditar, soloRegistrosPropios, idEmpresa)
            SELECT
                @id,
                f.idForm,
                ISNULL(v.diasPermitidosVer, -1) AS diasPermitidosVer,
                ISNULL(v.diasPermitidosEditar, -1) AS diasPermitidosEditar,
                ISNULL(v.propios, 1) AS soloRegistrosPropios,
                @IdEmpresa
            FROM dbo.Formularios f
            LEFT JOIN (VALUES
                (3,  -1,  0, 1),
                (10, -1,  0, 1),
                (19,  0,  0, 0),
                (21,  0, -1, 0),
                (1,   0,  0, 0),
                (2,  -1,  0, 0),
                (32,  0, -1, 0),
                (18, 14,  0, 1),
                (33,  0,  0, 1),
                (34,  0,  0, 1),
                (5,   0,  0, 0),
                (12, -1,  0, 0)
            ) AS v(idForm, diasPermitidosVer, diasPermitidosEditar, propios)
                ON f.idForm = v.idForm;
        END
        ELSE
        BEGIN
            UPDATE dbo.Usuarios
            SET
                nombre = @nombre,
                usuario = @usuario,
                clave = @clave,
                email = @email,
                admin = @admin,
                activo = @activo,
                colorForm = @colorForm
            WHERE id = @id;
        END

        COMMIT;

        -- opcional: devolver el id para que el cliente lo use
        SELECT @id AS id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;

        -- Re-lanza el error original con detalle
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END

GO
-- ===== agregarActualizacionStock =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarActualizacionStock] 
	-- Add the parameters for the stored procedure here
	@fechaActualizacion datetime,
	@observaciones nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into ActualizacionStock (fechaActualizacion, observaciones)
	values(@fechaActualizacion,@observaciones)
	
	select top 1 idActualizacion from ActualizacionStock order by idActualizacion desc
	
END

GO
-- ===== agregarCompra =====

CREATE PROCEDURE [dbo].[agregarCompra]
    @nroRemito nvarchar(50) = '',
    @fechaCompra datetime,
    @idProveedor int,
    @cantMedias int = null,
    @kgsMedias int = null,
    @estado nvarchar(50),
    @observaciones nvarchar(max),
    @tipoCompra nvarchar(50),
    @idSucursal int,
    @creadoPor int = null,
    @enCtaCte tinyint = 0,
    @idPesajeAjustado int = null
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Compras
    (
        nroRemito,
        fechaCompra,
        idProveedor,
        cantMedias,
        kgsMedias,
        estado,
        observaciones,
        tipoCompra,
        idSucursal,
        creado,
        creadoPor,
        enCtaCte,
        idPesajeAjustado
    )
    VALUES
    (
        @nroRemito,
        @fechaCompra,
        @idProveedor,
        @cantMedias,
        @kgsMedias,
        @estado,
        @observaciones,
        @tipoCompra,
        @idSucursal,
        SYSDATETIME(),
        @creadoPor,
        @enCtaCte,
        @idPesajeAjustado
    );

    SELECT CAST(SCOPE_IDENTITY() AS int) AS idCompra;
END

GO
-- ===== agregarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCorte] 
	-- Add the parameters for the stored procedure here
	
	@codigo int,
	@corte nvarchar(50),
	@precioKg float,
	@tipo nvarchar(50),
	@independiente int,
	@idCorteMaestro int,
	@porcentaje float,
	@porcentajeHueso float,
	@desvioEstandar float
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    
    if ( @idCorteMaestro = -1)
		begin
			insert into Corte (codigo, corte, precioKg, tipo, independiente, idCorteMaestro,porcentaje,porcentajeHueso, desvioEstandar, creado)
			 values (@codigo,@corte,@precioKg,@tipo, @independiente,@idCorteMaestro,@porcentaje,@porcentajeHueso, @desvioEstandar, SYSDATETIME())
			
			update Corte set Corte.idCorteMaestro =(select top 1 Corte.idCorte from Corte order by Corte.idCorte desc)
			from Corte, Corte as CorteActual
			where Corte.idCorteMaestro=-1 and Corte.idCorte=CorteActual.idCorte
		end
		
	else
		begin
			insert into Corte (codigo, corte, precioKg, tipo, independiente, idCorteMaestro, porcentaje,porcentajeHueso,desvioEstandar, creado)
			 values (@codigo,@corte,@precioKg,@tipo,@independiente,@idCorteMaestro,@porcentaje,@porcentajeHueso,@desvioEstandar, SYSDATETIME())
		end
	
	--se inician los stock de los sucursales a cero
	insert into StockCorteSucursal(idCorte,idSucursal,stock,stockTeorico)
	values ((select top 1 Corte.idCorte from Corte order by Corte.idCorte desc), 1, 0,0)
	
	insert into StockCorteSucursal(idCorte,idSucursal,stock,stockTeorico)
	values ((select top 1 Corte.idCorte from Corte order by Corte.idCorte desc), 2, 0,0)
END

GO
-- ===== agregarCortePorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@idCorte int,
	@idSucursal int,
	@precioKg float,
	@cantKg float,
	@balanza tinyint,
	@creado datetime = SYSDATETIME,
	@creadoPor int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into CortePorCompra(idCompra,idCorte,idSucursal,precioKg,cantKg, balanza, creado, creadoPor)
	values (@idCompra,@idCorte,@idSucursal,@precioKg,@cantKg, @balanza, @creado, @creadoPor)
	

	--Si el registro es de compras, se actualiza la tabla cortes proveedor
	IF EXISTS (SELECT 1 
				   FROM Compras 
				   WHERE idCompra = @idCompra and tipoCompra = 'Cortes')
	BEGIN
	---Actualizar CorteProveedor
		-- Declarar variables para almacenar datos
		DECLARE @idProveedor INT;
		DECLARE @fechaCompra DATETIME;

		-- Obtener los valores de idProveedor, fechaCompra y ultimoPrecio desde la tabla Compras
		SELECT 
			@idProveedor = idProveedor, 
			@fechaCompra = fechaCompra
		FROM Compras
		WHERE idCompra = @idCompra;

		-- Verificar si existe el registro en CorteProveedor
		IF EXISTS (SELECT 1 
				   FROM CorteProveedor 
				   WHERE idProveedor = @idProveedor AND idCorte = @idCorte)
		BEGIN
			-- Actualizar los campos si fechaUltimaCompra es menor que fechaCompra
			UPDATE CorteProveedor
			SET 
				ultimoPrecio = @precioKg,
				fechaUltimaCompra = @fechaCompra
			WHERE idProveedor = @idProveedor
			  AND idCorte = @idCorte
			  AND fechaUltimaCompra < @fechaCompra;
		END
		ELSE
		BEGIN
			-- Insertar un nuevo registro en CorteProveedor
			INSERT INTO CorteProveedor (idProveedor, idCorte, ultimoPrecio, fechaUltimaCompra)
			VALUES (@idProveedor, @idCorte, @precioKg, @fechaCompra);
		END
	END
END

GO
-- ===== agregarCortePorEmbutido =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorEmbutido] 
	-- Add the parameters for the stored procedure here
	@idEmbutido int,
	@idCorte int,
	@kgUtilizados float,
	@idSucursal int,
	@pesoBalanza bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Se agrega el corte utilizado en el embutido
	insert into CortePorEmbutido (idEmbutido,idCorte,kgUtilizados,pesoBalanza)
	values (@idEmbutido,@idCorte,@kgUtilizados,@pesoBalanza)
	
	-- Se suma los kg utilizados en el corte al embutido
	update StockCorteSucursal  set stock =(stock + @kgUtilizados)
	from StockCorteSucursal, Embutidos, Corte
	where StockCorteSucursal.idSucursal=@idSucursal and StockCorteSucursal.idCorte=Embutidos.idCorte
		and Embutidos.idEmbutido=@idEmbutido
	
	-- Se descuenta los kg utilizados en el stock de la sucursal
	
	---- Se actualiza stock del corte ingresado
	update StockCorteSucursal set stock=(stock-@kgUtilizados)
	FROM  StockCorteSucursal, 
		Corte as CorteP
	WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=@idSucursal
		and CorteP.idCorte=@idCorte
		
		
	-- Se actualizan todos los sub-cortes del corte ingresado
	update StockCorteSucursal 
		set stock=(stock - (@kgUtilizados * (CorteP.porcentaje / 100) ))
	FROM  StockCorteSucursal as StockCorteSucursal, CortePorCompra,
		Corte as CorteP, Corte as CorteM		 
		WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=@idSucursal
		and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorte=@idCorte
				
	-- Se actualizan todos los cortes nivel 3
	update StockCorteSucursal 
		set stock=(stock - (@kgUtilizados *((CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM  StockCorteSucursal as StockCorteSucursal, CortePorCompra,
		Corte as CorteP, Corte as CorteM, Corte as CorteMedia		 
	WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=@idSucursal
		and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorteMaestro=CorteMedia.idCorte and CorteMedia.idCorte=@idCorte
				
				
	-- Se Actualiza Corte M del CorteM del corte ingresado
	update StockCorteSucursal 
		set stock=(stock - ((@kgUtilizados + (@kgUtilizados * CorteP.porcentajeHueso / CorteP.porcentaje))+
						 ((@kgUtilizados + (@kgUtilizados * CorteP.porcentajeHueso / CorteP.porcentaje))
						  * CorteM.porcentajeHueso/CorteM.porcentaje)))
	FROM         dbo.Corte AS CorteM INNER JOIN
						  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro AND CorteM.idCorte <> CorteP.idCorte INNER JOIN
						  dbo.Corte AS CorteSubMedia ON CorteM.idCorteMaestro = CorteSubMedia.idCorte INNER JOIN
						  dbo.StockCorteSucursal AS StockCorteSucursal ON CorteSubMedia.idCorte = StockCorteSucursal.idCorte
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (CorteP.idCorte = @idCorte) AND (CorteSubMedia.codigo > 0)

	-- Se Actualiza CorteM del corte ingresado
	update StockCorteSucursal 
		set stock=(stock - (@kgUtilizados + (@kgUtilizados * CorteP.porcentajeHueso / CorteP.porcentaje)))
	FROM         dbo.Corte AS CorteM INNER JOIN
						  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro AND CorteM.idCorte <> CorteP.idCorte INNER JOIN
						  dbo.StockCorteSucursal AS StockCorteSucursal ON CorteM.idCorte = StockCorteSucursal.idCorte
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (CorteP.idCorte = @idCorte) AND (CorteM.codigo > 0)

	
	--Agregar Cortes x Embutido
	---ACTUALIZACION DE CORTES

		
	--SubCortes del CorteMaestro del Corte M del Corte Ingresado
	update StockCorteSucursal 
	set stock=(StockCorteSucursalMaestro.stock * CorteSubCorte.porcentaje/100)	
	FROM         dbo.Corte AS CorteP INNER JOIN
			  dbo.Corte AS SubCorteM ON CorteP
GO
-- ===== agregarCortePorFormula =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorFormula]
	-- Add the parameters for the stored procedure here
	@idFormula int,
	@idCorte int,
	@porcentaje float,
	@agregarAuto bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure hereINSERT INTO [SuperCerdo].[dbo].[CortePorFormula]
     INSERT INTO  [CortePorFormula]
			([idFormula]
           ,[idCorte]
           ,[porcentaje]
           ,[agregarAuto])
     VALUES
           (@idFormula,   
            @idCorte, 
            @porcentaje,  
            @agregarAuto )
END


GO
-- ===== agregarCortePorMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarCortePorMovimiento] 
	-- Add the parameters for the stored procedure here
	@idMovimiento int,
	@idCorte int,
	@cantKg float,
	@cantUnidad int = 0,
	@pesoBalanza bit=0,
	@permitirIngreso tinyint = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    insert into CortePorMovimiento(idMovimientos,idCorte,cantKg, cantUnidad, pesoBalanza, permitirIngreso) 
		values (@idMovimiento,@idCorte,@cantKg, @cantUnidad, @pesoBalanza, @permitirIngreso)
		
	declare @idCorteMovimiento int;
	
	select top 1 @idCorteMovimiento=idCorteMovimiento  from CortePorMovimiento order by idCorteMovimiento desc
	
	
	----*******Sucursal Origen
	----Actualizo el Stock de los cortes
	
	----Descuento los Kgs de la sucursal de origen
	--update StockCorteSucursal set stock=(stock - dbo.CortePorMovimiento.cantKg )
	--FROM         dbo.StockCorteSucursal INNER JOIN
	--		  dbo.Movimiento ON dbo.StockCorteSucursal.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
	--		  dbo.CortePorMovimiento ON dbo.StockCorteSucursal.idCorte = dbo.CortePorMovimiento.idCorte AND 
	--		  dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
	--WHERE     (dbo.CortePorMovimiento.idCorteMovimiento = @idCorteMovimiento)
		
	
	---- Se actualizan todos los sub-cortes del corte ingresado
	--update StockCorteSucursal 
	--	set stock=(stock - (dbo.CortePorMovimiento.cantKg  * (CorteP.porcentaje / 100) ))
	--FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--		  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
	--		  dbo.Movimiento ON StockCorteSucursal.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
	--		  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
	--		  dbo.CortePorMovimiento ON CorteM.idCorte = dbo.CortePorMovimiento.idCorte AND dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
	--WHERE     (dbo.CortePorMovimiento.idCorteMovimiento = @idCorteMovimiento)
	
				
	---- Se actualizan todos los sub-cortes de los sub-cortes del corte ingresado
	--update StockCorteSucursal 
	--	set stock=(stock - (dbo.CortePorMovimiento.cantKg  *((CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	--FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
	--	  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
	--	  dbo.Movimiento ON StockCorteSucursal.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
	--	  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
	--	  dbo.Corte AS CorteMedia ON CorteM.idCorteMaestro = CorteMedia.idCorte INNER JOIN
	--	  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos AND 
	--	  CorteMedia.idCorte = dbo.CortePorMovimiento.idCorte
	--WHERE     (dbo.CortePorMovimiento.idCorteMovimiento = @idCorteMovimiento)	
	
				
	---- Se Actualiza Corte M del CorteM del corte ingresado
	--update StockCorteSucursal 
	--	set stock=(stock - (dbo.CortePorMovimiento.cantKg + (dbo.CortePorMovimiento.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje)
	--					+(( dbo.CortePorMovimiento.cantKg + (dbo.CortePorMovimiento.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje))*
	--						CorteM.porcentajeHueso / CorteM.porcentaje)))
	--FROM         dbo.Corte AS CorteM INNER JOIN
	--	  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro AND CorteM.idCorte <> CorteP.idCorte INNER JOIN
	--	  dbo.Corte AS CorteSubMedia ON CorteM.idCorteMaestro = CorteSubMedia.idCorte INNER JOIN
	--	  dbo.StockCorteSucursal AS StockCorteSucursal ON CorteSub
GO
-- ===== agregarEmbutido =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarEmbutido] 
	-- Add the parameters for the stored procedure here
	@fechaEmbutido datetime,
	@idCorte int,
	@idSucursal int,
	@creadoPor int = null,
	@observaciones nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into Embutidos (fechaEmbutido,idCorte,idSucursal,estado, observaciones, creado, creadoPor) 
	values (@fechaEmbutido,@idCorte,@idSucursal,'',@observaciones, SYSDATETIME(), @creadoPor)
	
	--selecciono el idEmbutido agregado
	select top 1 Embutidos.idEmbutido from Embutidos order by idEmbutido desc
END

GO
-- ===== agregarExpendio =====

CREATE PROCEDURE [dbo].[agregarExpendio]
	@idExpendio int=0,
	@idVendedor int=0,
	@fechaExpendio datetime=null,
	@idSucursal int=null,
	@identificacionExpendio nvarchar(50)=null,
	@sector nvarchar(MAX)=null,
	@cantItems int=0,
	@importe float=0,
	@serialCPU nvarchar(MAX)=null,
	@observaciones nvarchar(MAX)=null
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Licencias SET sector = @sector WHERE nroLicencia = @serialCPU

	insert into Expendios(idVendedor, fechaExpendio, idSucursal, identificacionExpendio, sector, cantItems, importe, creado, observaciones)
	values (@idVendedor, @fechaExpendio, @idSucursal, @identificacionExpendio, @sector, @cantItems, @importe, SYSDATETIME(), @observaciones)

	select top 1 Expendios.idExpendio from Expendios where Expendios.idSucursal = @idSucursal order by Expendios.idExpendio desc
END

GO
-- ===== agregarLineaExpendio =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[agregarLineaExpendio]

@idLineaExpendio int=0,
@idExpendio int=0,
@idCorte int=null,
@cantKg float=null,
@precioKg float=null,
@pesoBalanza tinyint=null

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into LineaExpendio(idExpendio, idCorte, cantKg, precioKg, pesoBalanza)
	values (@idExpendio,@idCorte, @cantKg, @precioKg, @pesoBalanza)
	
	set @idLineaExpendio = SCOPE_IDENTITY() 	
	
	select @idLineaExpendio	
END

GO
-- ===== agregarLineaVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarLineaVenta] 
	
	@idLineaVenta int = null,
	@idVenta int,
	@idCorte int,
	@pesoBalanza int = null,
	@idAnulado int, --0 Activo --1 Anulado
	@idLineaVentaAnulado int = null, -- idLineaVenta por la que fue anulado
	@cantKg float,
	@kgsAjusteTarj float,
	@porcKgsAjusteTarj float,
	@precioKg float,
	@bonificacion float = 0,
	@ajustePrecio float = 0,
	@idAlicuotaIva int = 0,
    @alicuotaIva float = 0
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
     
	insert into LineaVenta (idVenta,idCorte,  idAnulado, idLineaVentaAnulado,cantKg, kgsAjusteTarj, porcKgsAjusteTarj, idAlicuotaIva, alicuotaIva, precioKg, ajustePrecio, pesoBalanza, bonificacion)
	values (@idVenta,@idCorte, @idAnulado, @idLineaVentaAnulado,@cantKg, @kgsAjusteTarj, @porcKgsAjusteTarj, @idAlicuotaIva, @alicuotaIva,@precioKg, @ajustePrecio, @pesoBalanza, @bonificacion)
	
	set @idLineaVenta = SCOPE_IDENTITY() 	
	
	select @idLineaVenta	
	
END

GO
-- ===== agregarMediaRes =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarMediaRes] 
	-- Add the parameters for the stored procedure here
	@nroTropa nvarchar(50),
	@precioMedia float,
	@kgMedia float,
	@idCompra int,
	@idSucursal int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Se agrega la media res
	insert into MediaRes (idCompra,nroTropa, precioMedia,kgMedia,idSucursal)
	values (@idCompra,@nroTropa,@precioMedia,@kgMedia,@idSucursal)
	
	
	-- Se ingresa el stock por corte que se desprende de la Media Res
    
    
    ---STOCK REAL
    -- Actualiza los cortes primarios (Nivel 1)
	update StockCorteSucursal set stock=(stock+ @kgMedia * CorteP.porcentaje / 100 )
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
						  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
						  dbo.Corte AS CorteMediaRes ON CorteP.idCorteMaestro = CorteMediaRes.idCorte AND CorteP.idCorteMaestro = CorteMediaRes.idCorte
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (CorteMediaRes.codigo = 0)  AND 
						  CorteP.idCorte <> CorteMediaRes.idCorte

		
	-- Se actualizan todos los sub-cortes (Nivel 2)
	update StockCorteSucursal 
		set stock=(stock+ (((@kgMedia * CorteM.porcentaje / 100) * (CorteP.porcentaje / 100) )))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
						  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
						  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
						  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte AND CorteM.idCorte <> MediaRes.idCorte
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (MediaRes.codigo = 0)
		
	-- Se actualizan todos los cortes nivel 3 (Nivel 3)
	update StockCorteSucursal 
		set stock=(stock+ (((@kgMedia * ( CorteMedia.porcentaje/100 ))*(CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM         dbo.Corte AS CorteMedia INNER JOIN
						  dbo.Corte AS MediaRes ON CorteMedia.idCorteMaestro = MediaRes.idCorte AND CorteMedia.idCorte <> MediaRes.idCorte INNER JOIN
						  dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
						  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
						  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON CorteMedia.idCorte = CorteM.idCorteMaestro
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (MediaRes.codigo = 0)
	
			
	---	STOCK TEORICO
	    -- Actualiza los cortes primarios (Nivel 1)
	update StockCorteSucursal set stockTeorico=(stockTeorico+ @kgMedia * CorteP.porcentaje / 100 )
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
						  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
						  dbo.Corte AS CorteMediaRes ON CorteP.idCorteMaestro = CorteMediaRes.idCorte AND CorteP.idCorteMaestro = CorteMediaRes.idCorte
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (CorteMediaRes.codigo = 0)  AND 
						  CorteP.idCorte <> CorteMediaRes.idCorte

		
	-- Se actualizan todos los sub-cortes (Nivel 2)
	update StockCorteSucursal 
		set stockTeorico=(stockTeorico+ (((@kgMedia * CorteM.porcentaje / 100) * (CorteP.porcentaje / 100) )))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
						  dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
						  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
						  dbo.Corte AS MediaRes ON CorteM.idCorteMaestro = MediaRes.idCorte AND CorteM.idCorte <> MediaRes.idCorte
	WHERE     (StockCorteSucursal.idSucursal = @idSucursal) AND (MediaRes.codigo = 0)
		
	-- Se actualizan todos los cortes nivel 3 (Nivel 3)
	update Stock
GO
-- ===== agregarStockVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[agregarStockVenta] 
		-- Add the parameters for the stored procedure here
	@idVenta int, 
	@estado nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	
	----Cortes No Anulados
			
	---- Se actualiza stock del corte ingresado
	update StockCorteSucursal set stock=( 	StockCorteSucursal_1.stock +
						  (SELECT     SUM(dbo.LineaVenta.cantKg) AS Expr1
							FROM          dbo.LineaVenta INNER JOIN
												   dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
												   dbo.StockCorteSucursal ON dbo.Ventas.idSucursal = dbo.StockCorteSucursal.idSucursal INNER JOIN
												   dbo.Corte ON dbo.StockCorteSucursal.idCorte = dbo.Corte.idCorte AND dbo.LineaVenta.idCorte = dbo.Corte.idCorte
							WHERE      (dbo.Ventas.idVenta = Ventas_1.idVenta) AND (dbo.Corte.idCorte = CorteP.idCorte)
							GROUP BY dbo.Corte.idCorte))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal_1 INNER JOIN
						  dbo.Corte AS CorteP ON StockCorteSucursal_1.idCorte = CorteP.idCorte INNER JOIN
						  dbo.Ventas AS Ventas_1 ON StockCorteSucursal_1.idSucursal = Ventas_1.idSucursal INNER JOIN
						  dbo.LineaVenta AS LineaVenta_1 ON CorteP.idCorte = LineaVenta_1.idCorte AND Ventas_1.idVenta = LineaVenta_1.idVenta
	WHERE     (Ventas_1.idVenta = @idVenta) AND (LineaVenta_1.idAnulado < 1 OR
						  LineaVenta_1.idAnulado = 1)
		



	-----PUCHERO
	------Se ingresa la cantidad de Kg en hueso que representa el corte  *****visto
	
	--update StockCorteSucursal 
	--	set stock=(StockCorteSucursal.stock -
 --                         (SELECT     SUM(dbo.LineaVenta.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS Expr1
 --                           FROM          dbo.LineaVenta INNER JOIN
 --                                                  dbo.Corte AS CorteP ON dbo.LineaVenta.idCorte = CorteP.idCorte INNER JOIN
 --                                                  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta
 --                           WHERE      (dbo.Ventas.idVenta = @idVenta) 
 --                           GROUP BY dbo.Ventas.idVenta))
	--FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
 --                     dbo.Ventas AS Ventas_1 ON StockCorteSucursal.idSucursal = Ventas_1.idSucursal INNER JOIN
 --                     dbo.Corte AS CortePuchero ON StockCorteSucursal.idCorte = CortePuchero.idCorte
	--WHERE     (CortePuchero.corte = 'Puchero') AND (Ventas_1.idVenta = @idventa)



	----Actulizar los cortes del cual deriva el corte ingresado


			-- Se actualiza el corte maestro del corte superior
			
	update StockCorteSucursal 
		set stock=( StockCorteSucursal_1.stock +
						  (SELECT     SUM(dbo.LineaVenta.cantKg + dbo.LineaVenta.cantKg * CorteP.porcentajeHueso / CorteP.porcentaje) AS Expr1
							FROM          dbo.StockCorteSucursal INNER JOIN
												   dbo.Ventas ON dbo.StockCorteSucursal.idSucursal = dbo.Ventas.idSucursal INNER JOIN
												   dbo.LineaVenta INNER JOIN
												   dbo.Corte AS CorteP ON dbo.LineaVenta.idCorte = CorteP.idCorte ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
												   dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
												   dbo.Corte AS CorteMedia ON CorteM.idCorteMaestro = CorteMedia.idCorte AND dbo.StockCorteSucursal.idCorte = CorteMedia.idCorte AND 
												   CorteM.idCorte <> CorteMedia.idCorte
							WHERE      (dbo.Ventas.idVenta = Ventas_1.idVenta) AND (CorteMedia_1.idCorte = CorteMedia.idCorte)
							GROUP BY CorteMedia.idCorte) )
	FROM         dbo.Corte AS CorteM_1 INNER JOIN
						  dbo.StockCorteSucursal AS StockCorteSucu
GO
-- ===== agregarVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[agregarVenta] 
	-- Add the parameters for the stored procedure here
	@idVenta int = 0,
	@idVendedor int = null,
	@idVentaUltima int = null,
	@fechaVenta datetime,
	@turno nvarchar(50),
	@tipoVenta nvarchar(50) = null,
	@diaFestivo nvarchar(50) = null,
	@observaciones nvarchar(200),
	@idPersona int,
	@idSucursal int,
	@nroRemito nvarchar(50),
	@enCtaCte tinyint = 0,
	@formaPago nvarchar(50),
	@tipoComprobante char = null,
	@cuit nvarchar(50) = null,
	@email nvarchar(50) = null,
	@acumRedondeoKgs float = 0,
	@acumRedondeoImporte float = 0,
	@comisionTarjeta float = 0,
	@pagoMixtoEfectivo float = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	set @diaFestivo = (select Feriados.feriado from Feriados where @fechaVenta between Feriados.desde and Feriados.hasta)
            
	insert into Ventas (idVendedor, fechaVenta,idSucursal,turno,diaFestivo,observaciones,idPersona,nroRemito,estado, enCtaCte, formaPago, tipoComprobante, cuit, email, acumRedondeoKgs, acumRedondeoImporte, comisionTarjeta, pagoMixtoEfectivo, creado)
	values (@idVendedor, @fechaVenta,@idSucursal,@turno,@diaFestivo,@observaciones,@idPersona,@nroRemito,'', @enCtaCte, @formaPago, @tipoComprobante, @cuit, @email, @acumRedondeoKgs, @acumRedondeoImporte, @comisionTarjeta, @pagoMixtoEfectivo, SYSDATETIME())

	select top 1 Ventas.idVenta from Ventas where Ventas.idSucursal = @idSucursal order by Ventas.idVenta desc
	
END

GO
-- ===== anularCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[anularCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update Compras set estado='Anulado' where idCompra=@idCompra
	
	---Media Res	

	-- Actualiza los cortes primarios
	update StockCorteSucursal set stock=(stock-(MediaRes.kgMedia*(CorteP.porcentaje)/100))
	FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, 
		SuperCerdo.dbo.Corte as CorteP, MediaRes
	where StockCorteSucursal.idCorte=CorteP.idCorte and CorteP.codigo=0
		and MediaRes.idCompra=@idCompra and StockCorteSucursal.idSucursal=MediaRes.idSucursal
		
	-- Se actualizan todos los sub-cortes
	update StockCorteSucursal 
		set stock=(stock- (((MediaRes.kgMedia*(CorteM.porcentaje)/100)) * (CorteP.porcentaje / 100) ))
	FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, 
		SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, MediaRes
	where StockCorteSucursal.idCorte=CorteP.idCorte and CorteP.idCorteMaestro=CorteM.idCorte and
		 CorteM.codigo=0
		and MediaRes.idCompra=@idCompra and StockCorteSucursal.idSucursal=MediaRes.idSucursal
		
	-- Se actualizan todos los cortes nivel 3
	update StockCorteSucursal 
		set stock=(stock- (((MediaRes.kgMedia * ( CorteMedia.porcentaje/100 ))*(CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, 
		SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, SuperCerdo.dbo.Corte as CorteMedia,MediaRes
	where StockCorteSucursal.idCorte=CorteP.idCorte and CorteP.idCorteMaestro=CorteM.idCorte and
		CorteM.idCorteMaestro=CorteMedia.idCorte  and CorteMedia.codigo=0
		and MediaRes.idCompra=@idCompra and StockCorteSucursal.idSucursal=MediaRes.idSucursal
		
	-- Se Actualizan todos los cortes nivel 4
	update StockCorteSucursal 
		set stock=(stock- (((MediaRes.kgMedia * ( CorteMedia.porcentaje/100 ))*(CorteSubMedia.porcentaje/100)*(CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, 
		SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, SuperCerdo.dbo.Corte as CorteSubMedia,
		SuperCerdo.dbo.Corte as CorteMedia,MediaRes
	where StockCorteSucursal.idCorte=CorteP.idCorte and CorteP.idCorteMaestro=CorteM.idCorte and
		 CorteM.idCorteMaestro=CorteSubMedia.idCorte
		  and CorteSubMedia.idCorteMaestro=CorteMedia.idCorte and CorteMedia.codigo=0
		and MediaRes.idCompra=@idCompra and StockCorteSucursal.idSucursal=MediaRes.idSucursal	
	
	
	--Cortes
		
	-- Actualiza los cortes primarios
	update StockCorteSucursal 
		set stock=(stock-(CortePorCompra.cantKg))
	FROM  SuperCerdo.dbo.StockCorteSucursal, 
		SuperCerdo.dbo.Corte as CorteP, CortePorCompra
	WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=CortePorCompra.idSucursal
		and CortePorCompra.idCompra=@idCompra and CorteP.idCorte=CortePorCompra.idCorte 
		
		
	-- Se actualizan todos los sub-cortes
	update StockCorteSucursal 
		set stock=(stock- ((CortePorCompra.cantKg) * (CorteP.porcentaje / 100) ))
	FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, CortePorCompra,
		SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM		 
	WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=CortePorCompra.idSucursal
		and CortePorCompra.idCompra=@idCompra
		and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorte=CortePorCompra.idCorte and  CorteP.idCorte<>CorteM.idCorte
				
	-- Se actualizan todos los cortes nivel 3
	update StockCorteSucursal 
		set stock=(stock- ((CortePorCompra.cantKg *(CorteM.porcentaje)/100) * (CorteP.por
GO
-- ===== anularEmbutido =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[anularEmbutido] 
	-- Add the parameters for the stored procedure here
	@idEmbutido int,
	@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update Embutidos set estado='Anulado', actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor where idEmbutido=@idEmbutido
	
	
END

GO
-- ===== anularMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[anularMovimiento] 
	-- Add the parameters for the stored procedure here
	@idMovimiento int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update Movimientos set estado='Anulado' where idMovimiento=@idMovimiento
	
	--Se reestablece el stock de las sucursales
	
	 
	 --*******Sucursal Origen
	--Actualizo el Stock de los cortes
	update StockCorteSucursal set stock=stock + Movimientos.kgCorte
	from StockCorteSucursal,Movimientos
	where Movimientos.idMovimiento=@idMovimiento and
	 StockCorteSucursal.idCorte=Movimientos.idCorte and
	 StockCorteSucursal.idSucursal=Movimientos.sucursalOrigen
	
	-- Se actualizan todos los sub-cortes del corte ingresado
	update StockCorteSucursal 
		set stock=(stock + (Movimientos.kgCorte * (CorteP.porcentaje / 100) ))
	FROM  StockCorteSucursal,Movimientos,
		SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM		 
		WHERE Movimientos.idMovimiento=@idMovimiento and
	 StockCorteSucursal.idCorte=CorteP.idCorte and
	 StockCorteSucursal.idSucursal=Movimientos.sucursalOrigen 
		and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorte=Movimientos.idCorte and CorteP.idCorte<>CorteM.idCorte
				
	-- Se actualizan todos los cortes nivel 3
	update StockCorteSucursal 
		set stock=(stock + (Movimientos.kgCorte *((CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM  StockCorteSucursal,Movimientos,
		SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, SuperCerdo.dbo.Corte as CorteMedia		 
	WHERE Movimientos.idMovimiento=@idMovimiento and StockCorteSucursal.idCorte=CorteP.idCorte 
		and  StockCorteSucursal.idSucursal=Movimientos.sucursalOrigen
		and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorteMaestro=CorteMedia.idCorte and CorteMedia.idCorte=Movimientos.idCorte and CorteP.idCorte<>CorteM.idCorte
				
	-- Se Actualiza Corte M del CorteM del corte ingresado
	update StockCorteSucursal 
		set stock=(stock +  dbo.Movimientos.kgCorte + (dbo.Movimientos.kgCorte * CorteP.porcentajeHueso / CorteP.porcentaje ))
	FROM         dbo.Corte AS CorteP INNER JOIN
                      dbo.StockCorteSucursal INNER JOIN
                      dbo.Movimientos ON dbo.StockCorteSucursal.idSucursal = dbo.Movimientos.sucursalOrigen INNER JOIN
                      dbo.Corte AS CorteMedia INNER JOIN
                      dbo.Corte AS CorteM ON CorteMedia.idCorte = CorteM.idCorteMaestro ON dbo.StockCorteSucursal.idCorte = CorteMedia.idCorte ON 
                      CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte = dbo.Movimientos.idCorte
	WHERE     (dbo.Movimientos.idMovimiento = @idMovimiento) AND (CorteMedia.codigo <> 0)
	

	-- Se Actualiza CorteM del corte ingresado
	update StockCorteSucursal 
		set stock=(stock +  dbo.Movimientos.kgCorte + (dbo.Movimientos.kgCorte * CorteP.porcentajeHueso / CorteP.porcentaje ))
FROM         dbo.StockCorteSucursal INNER JOIN
                      dbo.Movimientos ON dbo.StockCorteSucursal.idSucursal = dbo.Movimientos.sucursalOrigen INNER JOIN
                      dbo.Corte AS CorteP INNER JOIN
                      dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte ON dbo.StockCorteSucursal.idCorte = CorteM.idCorte AND 
                      dbo.Movimientos.idCorte = CorteP.idCorte
WHERE     (dbo.Movimientos.idMovimiento = @idMovimiento) AND (CorteM.codigo <> 0)

	 
	 --***Sucursal Destino
	 
	update StockCorteSucursal set stock=stock - Movimientos.kgCorte
	from StockCorteSucursal,Movimientos
	where Movimientos.idMovimiento=@idMovimiento and
	 StockCorteSucursal.idCorte=Movimientos.idCorte and
	 StockCorteSucursal.idSucursal=Movimientos.sucursalDestino	 
	
	-- Se actualizan todo
GO
-- ===== Balance =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Balance]

		@año_param int,
		@mes_param int,
		@dia_desde int, 
		@dia_hasta int
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 
AS
BEGIN
		--Declare 
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 

		/********* BALANCE *************/
		  
		  Select 
			  [mes]
			  ,[año]
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
		  from
		  (
		  /***** Compra Medias ******/
		SELECT TOP 1000
			  [mes]
			  ,[año]
			  ,sum(-[kilos]) as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		  UNION
		  
		/***** Compra Cortes ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,0 as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		  UNION
		  
		/***** Gastos ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum(-[monto]) as Monto
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		  UNION
		  
		/***** Ventas ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  ) as Balance
		  group by mes, año


		/************ DETALLES ***************/
		/***** Compra Medias ******/
		SELECT TOP 1000
			  [mes]
			  ,[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) as CompraMedias
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		/***** Compra Cortes ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) as CompraCortes
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		/***** Gastos ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum([monto]) as Gastos
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias]
		  where mes = @mes_param and año = @año_param and dia between @dia_desde and @dia_hasta
		  group by mes, año
		  
		  /**********/

END

GO
-- ===== BalanceConMeses =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[BalanceConMeses]

		@año_param int,
		@mes1_param int,
		@dia1_desde int, 
		@dia1_hasta int,
		@mes2_param int,
		@dia2_desde int, 
		@dia2_hasta int
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 
AS
BEGIN
		--Declare 
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 

		/********* BALANCE *************/
		  
		  Select 
			  [mes]
			  ,[año]
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
		  from
		  (
		  /***** Compra Medias ******/
		SELECT TOP 1000
			  [mes]
			  ,[año]
			  ,sum(-[kilos]) as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		  UNION
		  
		/***** Compra Cortes ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,0 as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		  UNION
		  
		/***** Gastos ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum(-[monto]) as Monto
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		  UNION
		  
		/***** Ventas ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  ) as Balance
		  group by mes, año


		/************ DETALLES ***************/
		/***** Compra Medias ******/
		SELECT TOP 1000
			  [mes]
			  ,[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) as CompraMedias
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		/***** Compra Cortes ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) as CompraCortes
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		/***** Gastos ******/
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum([monto]) as Gastos
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		SELECT TOP 1000 
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param an
GO
-- ===== BalanceConsFinal =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[BalanceConsFinal]

		@año_param int,
		@mes1_param int,
		@dia1_desde int, 
		@dia1_hasta int,
		@mes2_param int,
		@dia2_desde int, 
		@dia2_hasta int
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 
AS
BEGIN
		--Declare 
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 

		  
		  		  
/********* BALANCE *************/
		  
		  Select 
		  'BALANCE' as Descripcion,
				[año]
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
			  ,sum(Tickets)as Tickets
		  from
		  (
		  /***** Compra Medias ******/
		SELECT 
			  [mes]
			  ,[año]
			  ,sum(-[kilos]) as Kgs
			  ,sum(-[Total]) as Monto
			  ,0 as Tickets
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		  UNION
		  
		/***** Compra Cortes ******/
		SELECT  
			  [mes]
			  ,[año]
			  ,0 as Kgs
			  ,sum(-[Total]) as Monto
			  ,0 as Tickets
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		  UNION
		  
		/***** Gastos ******/
		SELECT  
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum(-[monto]) as Monto
			  ,0 as Tickets
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  
		  UNION
		  
		/***** Ventas ******/
		SELECT  
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
			  ,COUNT(*) as Tickets
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias_ConsumidorFinal]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by mes, año
		  ) as Balance
		  group by año
		  
		  
		/************ DETALLES ***************/
		UNION
		
		Select 
			'DETALLE BALANCE' as Descripcion
			  ,'' as [año]
			  ,'' as Kgs
			  ,'' as Monto
			  ,'' as Tickets		  
			  

		UNION 
		
		
		/***** VENTAS A CONS.FINAL ******/
		SELECT  
			'VENTAS A CONS.FINAL' as Descripcion
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
			  ,COUNT(*) as Tickets
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias_ConsumidorFinal]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by año
		  
		  
		  UNION
		/***** Compra Medias ******/
		SELECT 
			'COMPRAS MEDIAS' as Descripcion,
			[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) * -1 as Monto
			  ,'' as Tickets		
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
		  group by año
		  
		  UNION 
		/***** Compra Cortes ******/
		SELECT  
			'COMPRAS' as Descripcion,
			[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) * -1 as Monto
			  ,'' as Tickets		
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  where (mes = @mes1_param and año = @año_par
GO
-- ===== BalanceConsFinal_FecDesde_Hasta =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[BalanceConsFinal_FecDesde_Hasta]


		@idSucursal int,
		@FechaDesde datetime,
		@FechaHasta datetime
AS
BEGIN		  
		  		  
/********* BALANCE *************/
		  
		  Select 
		  1 as orden,
		  'BALANCE' as Descripcion
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
			  ,sum(Tickets)as Tickets
		  from
		  (
		  /***** Compra Medias ******/
		  SELECT     dbo.Compras.idSucursal as idSucursal, 0 AS Kgs, 
                      SUM(-dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia) AS Monto, 0 as Tickets
			FROM         dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra
			WHERE  dbo.Compras.idSucursal = @idSucursal AND dbo.Compras.fechaCompra between @FechaDesde and @FechaHasta 
			and ((dbo.Compras.tipoCompra = N'Media Res') OR
							  (dbo.Compras.tipoCompra = N'Cortes'))
		GROUP BY dbo.Compras.idSucursal
		  
		  UNION
		  
		/***** Compra Cortes ******/
		SELECT   dbo.Compras.idSucursal as idSucursal, 0 AS Kgs, 
                      SUM(-dbo.CortePorCompra.cantKg * dbo.CortePorCompra.precioKg) as Monto
			  ,0 as Tickets
		FROM         dbo.Compras INNER JOIN
							  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra
		WHERE  dbo.Compras.idSucursal = @idSucursal AND dbo.Compras.fechaCompra between @FechaDesde and @FechaHasta and ((dbo.Compras.tipoCompra = N'Media Res') OR
							  (dbo.Compras.tipoCompra = N'Cortes'))
		GROUP BY dbo.Compras.idSucursal
		  
		  UNION
		  
		/***** Gastos ******/
		 SELECT    dbo.EgresosCaja.idSucursal as idSucursal,  0 as Kgs, SUM(-dbo.EgresosCaja.monto) as Monto ,0 as Tickets 
		FROM         dbo.EgresosCaja INNER JOIN
							  dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id
		WHERE     dbo.EgresosCaja.idSucursal = @idSucursal AND dbo.EgresosCaja.fechaHora between @FechaDesde and @FechaHasta AND (dbo.TiposEgresoCaja.esGasto = 1) AND (dbo.EgresosCaja.idCompra IS NULL)
		GROUP BY dbo.EgresosCaja.idSucursal
		  
		  UNION
		  
		/***** Ventas ******/
		SELECT 
			idSucursal,
			sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
			  ,COUNT(*) as Tickets
		  FROM (SELECT     dbo.Ventas.idVenta, dbo.Ventas.idSucursal as idSucursal, SUM(dbo.LineaVenta.cantKg) AS Kgs, SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) AS Monto
				FROM         dbo.Ventas INNER JOIN
									  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
									  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
									  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal
				WHERE     dbo.Ventas.idSucursal = @idSucursal AND dbo.Ventas.fechaVenta between @FechaDesde and @FechaHasta AND (dbo.Ventas.enCtaCte = 0)
				GROUP BY  dbo.Ventas.idVenta, dbo.Ventas.idSucursal) AS Ventas
		GROUP BY idSucursal
		  ) as Balance
		  group by idSucursal		  
		  
		/************ DETALLES ***************/
		UNION
		
		Select 
		
		  2 as orden,
			'DETALLE BALANCE' as Descripcion
			  ,'' as Kgs
			  ,'' as Monto
			  ,'' as Tickets	 
			  
		UNION 		
		
		/***** VENTAS A CONS.FINAL ******/
		SELECT  		
		  3 as orden,
			'VENTAS A CONS.FINAL' as Descripcion
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
			  ,COUNT(*) as Tickets
		  FROM (SELECT     dbo.Ventas.idVenta, dbo.Ventas.idSucursal as idSucursal, SUM(dbo.LineaVenta.cantKg) AS Kgs, SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) AS Monto
				FROM         dbo.Ventas INNER JOIN
									  dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
									  dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
									  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal
				WHERE     dbo.
GO
-- ===== BalanceConsFinalVariosMeses =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[BalanceConsFinalVariosMeses]

		@año_desde int,
		@año_hasta int,
		@mes_desde int,
		@mes_hasta int,
		@dia_desde int, 
		@dia_hasta int
AS
BEGIN
		--Declare 
		--@año_param int = 2016,
		--@mes_param int = 12,
		--@dia_desde int = 26, 
		--@dia_hasta int = 31 

/********* BALANCE *************/
		  
		  Select 
		  [año]
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
		  from
		  (
		  /***** Compra Medias ******/
		SELECT 
			  [mes]
			  ,[año]
			  ,sum(-[kilos]) as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  
		  group by mes, año
		  
		  UNION
		  
		/***** Compra Cortes ******/
		SELECT  
			  [mes]
			  ,[año]
			  ,0 as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  group by mes, año
		  
		  UNION
		  
		/***** Gastos ******/
		SELECT  
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum(-[monto]) as Monto
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  group by mes, año
		  
		  UNION
		  
		/***** Ventas ******/
		SELECT 
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias_ConsumidorFinal]
		  group by mes, año
		  ) as Balance
		  group by año
		  
		/********* BALANCE *************/
		  
		  Select 
			  [mes]
			  ,[año]
			  ,sum(Kgs) as Kgs
			  ,sum(Monto) as Monto
		  from
		  (
		  /***** Compra Medias ******/
		SELECT 
			  [mes]
			  ,[año]
			  ,sum(-[kilos]) as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  group by mes, año
		  
		  UNION
		  
		/***** Compra Cortes ******/
		SELECT  
			  [mes]
			  ,[año]
			  ,0 as Kgs
			  ,sum(-[Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  group by mes, año
		  
		  UNION
		  
		/***** Gastos ******/
		SELECT  
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum(-[monto]) as Monto
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  group by mes, año
		  
		  UNION
		  
		/***** Ventas ******/
		SELECT  
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias_ConsumidorFinal]
		  group by mes, año
		  ) as Balance
		  group by mes, año


		/************ DETALLES ***************/
		/***** Compra Medias ******/
		SELECT 
			  [mes]
			  ,[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) as CompraMedias
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_MediaRes]
		  group by mes, año
		  
		/***** Compra Cortes ******/
		SELECT  
			  [mes]
			  ,[año]
			  ,sum([kilos]) as Kgs
			  ,sum([Total]) as CompraCortes
		  FROM [SuperCerdo].[dbo].[Compras_Diarias_Cortes]
		  group by mes, año
		  
		/***** Gastos ******/
		SELECT  
			  [mes]
			  ,[año]
			  , 0 as Kgs
			  ,sum([monto]) as Gastos
		  FROM [SuperCerdo].[dbo].[EgresosGastos_DiaMesAno]
		  group by mes, año
		  
		SELECT  
			  [mes]
			  ,[año]
			  ,sum([Kilos]) as Kgs
			  ,sum([Total]) as Monto
		  FROM [SuperCerdo].[dbo].[Ventas_Diarias_ConsumidorFinal]
		  group by mes, año
		  
		  /**********/

END

GO
-- ===== buscarCodigoCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[buscarCodigoCorte] 
	-- Add the parameters for the stored procedure here
	@codigo bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Corte.*
FROM         dbo.Corte 
where Corte.codigo=@codigo
END


GO
-- ===== buscarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[buscarCorte] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT      CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.precioKg as 'efectivo', CorteP.precioKg as 'debito', CorteP.precioKg as 'credito', CorteP.precioKg as 'Billetera', CorteP.precioKg as 'Qr', CorteP.precioKg as 'Transf', dbo.Personas.identificacion as 'Marca', CorteP.puntoStock, CorteP.habilitado, CorteP.mayorista, CorteP.enCierreStock, CorteP.alicuotaIva, CorteP.tipo, CorteP.pesable, CorteP.nivel,CorteP.idCorteMaestro,
	CASE 
    WHEN CorteP.porcentajeHueso > 1000 THEN 'PRES.:' + CorteM.corte 
    ELSE CorteM.corte 
END AS corteMaestro,

                      CAST(CorteP.porcentaje AS numeric(10, 2)) AS porcentaje, CASE 
    WHEN CorteP.porcentajeHueso > 1000 THEN 0
    ELSE CorteP.porcentajeHueso
END AS porcentajeHueso, 
CorteP.desvioEstandar, CorteP.independiente, CorteP.promedio, CorteP.ingresoRapidoEmbutido
FROM         dbo.Personas RIGHT OUTER JOIN
                  dbo.Corte AS CorteP ON dbo.Personas.idPersona = CorteP.idMarca LEFT OUTER JOIN
                  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte
WHERE    (CorteP.corte like '%'+@texto+'%'  OR cast(CorteP.codigo as nvarchar(50))= @texto or CorteM.corte like '%'+@texto+'%')

order by CorteP.codigo

END

GO
-- ===== buscarCorteSinMaestro =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[buscarCorteSinMaestro] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT     CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg
	FROM      
			  dbo.Corte AS CorteP 
	WHERE    (CorteP.corte like '%'+@texto+'%'  OR convert(nvarchar(50), CorteP.codigo)like @texto+'%' )

	order by CorteP.codigo
END

GO
-- ===== buscarEmbutido =====
CREATE PROCEDURE [dbo].[buscarEmbutido]
	@texto nvarchar(50),
	@idSucursal int = 0,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	SET NOCOUNT ON;

	SELECT     dbo.Embutidos.idEmbutido as 'Id', dbo.Embutidos.fechaEmbutido as 'Fecha',CorteEmbutido.codigo as 'Codigo', CorteEmbutido.corte as 'Corte',
                      SUM(dbo.CortePorEmbutido.kgUtilizados) AS 'Kgs', dbo.Sucursal.sucursal as 'Sucursal', dbo.Embutidos.estado  as 'Estado', 'Observaciones' = case
                      when LEN(dbo.Embutidos.observaciones) <= 20 then dbo.Embutidos.observaciones
                      else (SUBSTRING(dbo.Embutidos.observaciones, 1, 20) + '...') end ,
                      dbo.Embutidos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por',
                      dbo.Embutidos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
FROM          dbo.Corte INNER JOIN
                      dbo.CortePorEmbutido ON dbo.Corte.idCorte = dbo.CortePorEmbutido.idCorte INNER JOIN
                      dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
                      dbo.Corte AS CorteEmbutido ON dbo.Embutidos.idCorte = CorteEmbutido.idCorte INNER JOIN
                      dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Embutidos.creadoPor = CreadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Embutidos.actualizadoPor = ActualizadoPor.id
WHERE fechaEmbutido between @fechaDesde and @fechaHasta
and
(
(@idSucursal > 0 and dbo.Sucursal.idSucursal = @idSucursal) or
(@idSucursal <= 0 and dbo.Embutidos.idSucursal > 0)
)
and ((CAST( CorteEmbutido.codigo as nvarchar(50)) = @texto )or(CorteEmbutido.corte like '%'+@texto+'%' ) or (CreadoPor.nombre like '%'+@texto+'%' ) or (ActualizadoPor.nombre like '%'+@texto+'%' ))
GROUP BY dbo.Embutidos.idEmbutido, dbo.Embutidos.fechaEmbutido, CorteEmbutido.codigo, CorteEmbutido.corte, dbo.Sucursal.sucursal, dbo.Embutidos.creado, CreadoPor.nombre, dbo.Embutidos.actualizado, ActualizadoPor.nombre, dbo.Embutidos.estado, dbo.Embutidos.observaciones
ORDER BY fechaEmbutido, dbo.Embutidos.creado DESC

END
GO
-- ===== CargarBancos_BlocNotas =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[CargarBancos_BlocNotas]
AS
BEGIN

	BULK INSERT Bancos
	FROM 'C:\Temp\bancos.txt'
	WITH (
		FIELDTERMINATOR = '\n',  -- Cada línea es un banco
		ROWTERMINATOR = '\n',
		FIRSTROW = 1,            -- Empieza desde la primera línea
		CODEPAGE = 'ACP'         -- Ajusta según codificación
	);
END

GO
-- ===== cargarCortesPorMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[cargarCortesPorMovimiento] 
	-- Add the parameters for the stored procedure here
	@idMovimiento int,
	@acumulado tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    IF @acumulado = 0
		begin
		
			SELECT     dbo.CortePorMovimiento.idCorteMovimiento, dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorMovimiento.cantKg, dbo.CortePorMovimiento.cantUnidad, dbo.CortePorMovimiento.pesoBalanza, dbo.CortePorMovimiento.permitirIngreso
			FROM         dbo.CortePorMovimiento INNER JOIN
					  dbo.Corte ON dbo.CortePorMovimiento.idCorte = dbo.Corte.idCorte
			where dbo.CortePorMovimiento.idMovimientos=@idMovimiento
		
		end
    else
		begin
			SELECT     dbo.Corte.codigo, dbo.Corte.corte, SUM(dbo.CortePorMovimiento.cantUnidad) AS cantUnidad, SUM(dbo.CortePorMovimiento.cantKg) AS cantKg, 
					  0 AS idCorteMovimiento, 0 AS pesoBalanza, dbo.CortePorMovimiento.idCorte, dbo.CortePorMovimiento.idMovimientos
			FROM         dbo.CortePorMovimiento INNER JOIN
								  dbo.Corte ON dbo.CortePorMovimiento.idCorte = dbo.Corte.idCorte
			WHERE     (dbo.CortePorMovimiento.idMovimientos = @idMovimiento)
			GROUP BY dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorMovimiento.idCorte, dbo.CortePorMovimiento.idMovimientos
			ORDER BY dbo.Corte.codigo
		end
	
END

GO
-- ===== cargarMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[cargarMovimiento] 
	-- Add the parameters for the stored procedure here
	@idMovimiento int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Movimiento.idMovimiento, dbo.Movimiento.fechaMovimiento, dbo.Movimiento.sucursalOrigen as idOrigen, SucursalOrigen.sucursal as origen, dbo.Movimiento.idMovOrigen, dbo.Movimiento.sucursalDestino as idDestino, SucursalDestino.sucursal AS destino, dbo.Movimiento.observaciones, dbo.Movimiento.creado, dbo.Movimiento.actualizado, dbo.Movimiento.creadoPor, dbo.Movimiento.actualizadoPor
	FROM         dbo.Sucursal AS SucursalOrigen INNER JOIN
			  dbo.Movimiento ON SucursalOrigen.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
			  dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal
	where dbo.Movimiento.idMovimiento=@idMovimiento
END

GO
-- ===== cargarMovimientoOrigen =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[cargarMovimientoOrigen]
	@idMovimiento int = 0,
	@fechaMovimiento datetime,
	@sucursalOrigen int,
	@sucursalDestino int,
	@idMovOrigen int = 0,
	@observaciones nvarchar(200) = '',
	@creadoPor	int	= null,
	@actualizadoPor	int	= null,
	@isAdd tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @isAdd = 1
		begin
			insert into Movimiento 
					(fechaMovimiento,sucursalOrigen,sucursalDestino, idMovOrigen,observaciones, creado,creadoPor)
			values (@fechaMovimiento,@sucursalOrigen,@sucursalDestino, @idMovOrigen,@observaciones, SYSDATETIME(),@creadoPor)
			
			select top 1 idMovimiento from Movimiento order by idMovimiento desc
		end
	else
		begin
			--SE OBTIENE EL ID LOCAL QUE CORRESPONDO CON EL IdMovOrigen
			set @idMovimiento = (SELECT TOP 1 idMovimiento From Movimiento WHERE idMovOrigen=@idMovOrigen)
			
			--Se crea registro de historial
			insert into MovimientoHistorial (idMovimiento, FechaMovimiento, idSucOrigen, idSucDestino, idCorte, cantKg, cantUnidad, pesoBalanza, actualizadoPor, actualizado, observaciones)

			SELECT     dbo.Movimiento.idMovimiento, dbo.Movimiento.fechaMovimiento, dbo.Movimiento.sucursalOrigen, dbo.Movimiento.sucursalDestino, 
								  dbo.CortePorMovimiento.idCorte, dbo.CortePorMovimiento.cantKg, dbo.CortePorMovimiento.cantUnidad, dbo.CortePorMovimiento.pesoBalanza, 
								  @actualizadoPor, SYSDATETIME(), dbo.Movimiento.observaciones
			FROM         dbo.Movimiento INNER JOIN
								  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
			where Movimiento.idMovimiento = @idMovimiento

			--SE ACTUALIZA			
			update Movimiento set fechaMovimiento=@fechaMovimiento,sucursalOrigen=@sucursalOrigen,
				sucursalDestino=@sucursalDestino, observaciones = @observaciones, actualizacionCompleta = 0, actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor
			where idMovimiento = @idMovimiento	
						
			--se eliminan todos los cortes en el movimiento
			delete from CortePorMovimiento where idMovimientos=@idMovimiento
			
			--SE Devuelvo el id local
			--select @idMovimiento
			select top 1 idMovimiento from Movimiento where idMovimiento = @idMovimiento
		end
END

GO
-- ===== ControlLineasVtas =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ControlLineasVtas]
	-- Add the parameters for the stored procedure here
	@año_param int,
	@mes1_param int,
	@dia1_desde int, 
	@dia1_hasta int,
	@mes2_param int,
	@dia2_desde int, 
	@dia2_hasta int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     Fecha, Hora, nombre, codigo, corte, cantKg
	FROM         dbo.LineasVenta_ControlCam
	where (mes = @mes1_param and año = @año_param and dia between @dia1_desde and @dia1_hasta) OR 
				(mes = @mes2_param and año = @año_param and dia between @dia2_desde and @dia2_hasta)
	order by Fecha, Hora
	
	
END

GO
-- ===== EliminarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[EliminarCorte] 
	-- Add the parameters for the stored procedure here
	@idCorte int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	delete from Corte where idCorte=@idCorte
	
	delete from StockCorteSucursal where idCorte=@idCorte
	
END

GO
-- ===== eliminarLineas =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[eliminarLineas] 
	-- Add the parameters for the stored procedure here
	@p1 int = 0, 
	@p2 int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	delete from LineaVenta where idVenta=197
END

GO
-- ===== eliminarMovimiento =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[eliminarMovimiento]
	-- Add the parameters for the stored procedure here
	@idMovimiento int = 0,
	@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    --Se crea registro de historial
	insert into MovimientoHistorial (idMovimiento, FechaMovimiento, idSucOrigen, idSucDestino, idCorte, cantKg, cantUnidad, pesoBalanza, actualizadoPor, actualizado, observaciones)

	SELECT     dbo.Movimiento.idMovimiento, dbo.Movimiento.fechaMovimiento, dbo.Movimiento.sucursalOrigen, dbo.Movimiento.sucursalDestino, 
						  dbo.CortePorMovimiento.idCorte, dbo.CortePorMovimiento.cantKg, dbo.CortePorMovimiento.cantUnidad, dbo.CortePorMovimiento.pesoBalanza, 
						  @actualizadoPor, SYSDATETIME(), dbo.Movimiento.observaciones
	FROM         dbo.Movimiento INNER JOIN
						  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
	where Movimiento.idMovimiento = @idMovimiento
			
    
	delete from CortePorMovimiento where idMovimientos = @idMovimiento
	
	delete from Movimiento where idMovimiento = @idMovimiento
END

GO
-- ===== eliminarPersona =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[eliminarPersona] 
	-- Add the parameters for the stored procedure here
	@idPersona int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	delete from Personas where idPersona=@idPersona
	
END

GO
-- ===== getAllLineasVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getAllLineasVenta]
	-- Add the parameters for the stored procedure here
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50),
	@idSucursal int = -1,
	@idVendedor int = -1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT     dbo.Usuarios.nombre, dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Personas.razonSocial, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, 
                      dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalCorte, dbo.LineaVenta.bonificacion, dbo.LineaVenta.pesoBalanza, dbo.LineaVenta.idAnulado, dbo.Sucursal.sucursal
FROM         dbo.Ventas INNER JOIN
                      dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
                      dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
                      dbo.Usuarios ON dbo.Ventas.idVendedor = dbo.Usuarios.id INNER JOIN
                      dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona
	WHERE dbo.Ventas.fechaVenta between @fechaDesde and @fechaHasta and ((@idSucursal < 0 and dbo.Ventas.idSucursal >= 0) 
			or (@idSucursal >= 0 and 
			dbo.Ventas.idSucursal = @idSucursal))
			and ((@idVendedor < 0 and dbo.Ventas.idVendedor >= 0) or (@idVendedor >= 0 and 
			dbo.Ventas.idVendedor = @idVendedor)) and (dbo.Corte.codigo like '%'+@texto+'%' or dbo.Corte.corte like '%'+@texto+'%' or dbo.Personas.razonSocial like '%'+@texto+'%')
	order by dbo.Ventas.fechaVenta desc
		
END

GO
-- ===== getCtaCteByIdPersona =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getCtaCteByIdPersona]
	-- Add the parameters for the stored procedure here
	@idPersona int,
	@fechaDesde datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SELECT * FROM
    (
    SELECT     dbo.Personas.idPersona, dbo.Personas.razonSocial, '-' AS id, @fechaDesde AS fecha, '-' AS tabla, '-' AS idTabla, '-' AS nroDoc, 'Saldo Anterior' AS detalle, '-' AS tipo, SaldoAnteriorTabla.SaldoAnterior AS importe, 0 AS Saldo, '-' AS sucursal, @fechaDesde AS creado, '-' AS  CreadoPor, @fechaDesde AS actualizado, '-' AS ActualizadoPor
	FROM 
    (SELECT     dbo.Personas.idPersona, SUM(dbo.MovCtaCte.importe) AS SaldoAnterior
	FROM         dbo.MovCtaCte INNER JOIN
						  dbo.Personas ON dbo.MovCtaCte.idPersona = dbo.Personas.idPersona 
	Where dbo.Personas.idPersona = @idPersona and dbo.MovCtaCte.fecha < @fechaDesde
	GROUP BY dbo.Personas.idPersona) as SaldoAnteriorTabla INNER JOIN
                      dbo.Personas ON SaldoAnteriorTabla.idPersona = dbo.Personas.idPersona 	
    union
    
	SELECT     dbo.Personas.idPersona, dbo.Personas.razonSocial, dbo.MovCtaCte.id, dbo.MovCtaCte.fecha, dbo.MovCtaCte.tabla, dbo.MovCtaCte.idTabla, dbo.MovCtaCte.nroDoc,dbo.MovCtaCte.detalle, dbo.MovCtaCte.tipo, 
					  dbo.MovCtaCte.importe, 0.00 AS Saldo, dbo.Sucursal.sucursal, dbo.MovCtaCte.creado, CreadoPor.nombre AS CreadoPor, dbo.MovCtaCte.actualizado, ActualizadoPor.nombre AS ActualizadoPor
	FROM         dbo.MovCtaCte INNER JOIN
                      dbo.Personas ON dbo.MovCtaCte.idPersona = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Sucursal ON dbo.MovCtaCte.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.MovCtaCte.creadoPor = CreadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.MovCtaCte.actualizadoPor = ActualizadoPor.id
	Where dbo.Personas.idPersona = @idPersona and dbo.MovCtaCte.fecha > @fechaDesde
	) as MovCtaCte
	Order by fecha --, creado, id
END

GO
-- ===== getLineasCompras =====


-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[getLineasCompras] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@codigo nvarchar(50),
	@corte nvarchar(50),
	@fechaDesde datetime,
	@fechaHasta datetime,
	@tipoCompra nvarchar(50),
	@idSucursal int = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    IF @idSucursal > 0
		BEGIN				
			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, '-' AS codigo, 'Media Res' AS corte, dbo.MediaRes.kgMedia AS cantKg, dbo.MediaRes.precioMedia AS precioKg, 
                      dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia AS totalS, dbo.Compras.estado, dbo.Compras.idSucursal, 
                      dbo.Sucursal.sucursal, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor,
                       dbo.Compras.actualizado, ActualizadoPor.nombre AS actualizadoPor
FROM         dbo.Compras INNER JOIN
                      dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorCompra.cantKg AS cantKg, dbo.CortePorCompra.precioKg, 
                      dbo.CortePorCompra.precioKg * dbo.CortePorCompra.cantKg AS totalS, dbo.Compras.estado, dbo.Compras.idSucursal, 
                      dbo.Sucursal.sucursal, dbo.Compras.observaciones, dbo.Compras.creado, 
                      CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado, ActualizadoPor.nombre AS actualizadoPor
FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN
                      dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or (dbo.Compras.nroRemito like '%'+@texto+'%' ) )  and (cast(dbo.Corte.codigo as nvarchar(50)) like '%'+@codigo+'%') and (dbo.Corte.corte like '%'+@corte+'%'))
			order by dbo.Compras.fechaCompra desc
			
		END
	ELSE
		BEGIN
				
			(SELECT     dbo.Compras.idCompra, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo
GO
-- ===== getListaElegirEmbutido =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getListaElegirEmbutido]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;	
		
   SELECT     CorteEmbutido.idCorte AS idCorteEmbutido, CorteEmbutido.codigo as codigoEmbutido, 
		CorteEmbutido.corte as corteEmbutido
		FROM         dbo.Corte as  CorteEmbutido
		Where (CorteEmbutido.ingresoRapidoEmbutido = 1)
			
		Order by CorteEmbutido.codigo;
END

GO
-- ===== getPorcCortesEnMedias =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getPorcCortesEnMedias]
	-- Add the parameters for the stored procedure here
	@id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SELECT    TablaUnion.idCorte, TablaUnion.codigo as 'Codigo', TablaUnion.corte as Corte, TablaUnion.CantKg, TablaUnion.PromPorMedia, TablaUnion.PorcReal, TablaUnion.PorcTeo, TablaUnion.Dif as 'Dif.', TablaUnion.Espacio as '-', TablaUnion.precioKg, TablaUnion.Gan as 'Gan.'
    FROM
	((SELECT    dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte as Corte, sum(dbo.CortePorCompra.cantKg) as CantKg, (sum(dbo.CortePorCompra.cantKg) / dbo.Compras.cantMedias) as PromPorMedia, (sum(dbo.CortePorCompra.cantKg) / dbo.Compras.kgsMedias) as PorcReal, (dbo.Corte.porcentaje / 100) as PorcTeo, (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100)) as CantKgTeo, sum(dbo.CortePorCompra.cantKg) - (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100)) as 'Dif', '' as 'Espacio', dbo.Corte.precioKg as 'PrecioKg', (sum(dbo.CortePorCompra.cantKg) - (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100))) * dbo.Corte.precioKg as 'Gan'
	FROM         dbo.Corte INNER JOIN
						  dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
						  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra
	WHERE     (dbo.CortePorCompra.idCompra = @id)
	Group by dbo.Compras.idCompra, dbo.Compras.cantMedias, dbo.Compras.kgsMedias, dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Corte.porcentaje, dbo.Corte.precioKg
	)
	
	UNION
	--se pone el codigo '99999' para que quede ultima la fila de los totales order by  TablaUnion.codigo**
	(SELECT    null as 'idCorte', '99999' as codigo, '' as corte, null as CantKg, null as PromPorMedia,null as PorcReal, null as PorcTeo, null as CantKgTeo, null as 'Dif', null as 'Espacio', null as 'PrecioKg', 0 as 'Gan'
	FROM         dbo.Corte INNER JOIN
						  dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
						  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra
	WHERE     (dbo.CortePorCompra.idCompra = @id)
	)) as TablaUnion
	order by  TablaUnion.codigo

END

GO
-- ===== getPromMedias =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getPromMedias]
	-- Add the parameters for the stored procedure here
	@id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  TOP 1   dbo.Compras.cantMedias as CantMedias, dbo.Compras.kgsMedias as Kgs, (dbo.Compras.kgsMedias / dbo.Compras.cantMedias) as PromMedias, dbo.Personas.razonSocial as Proveedor, 
                      CONVERT(VARCHAR(10),dbo.Compras.fechaCompra, 103) as Fecha
FROM         dbo.Corte INNER JOIN
                      dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
                      dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona
	WHERE     (dbo.CortePorCompra.idCompra = @id)

END

GO
-- ===== getUsuariosActivos =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[getUsuariosActivos]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT nombre,usuario,clave from Usuarios 
		where id = 1 or id = 2 or id = 4 or 
		id = 8 or id = 12 or id = 18 or id = 20 or id = 22 or id = 23
END

GO
-- ===== IngresoMovIndependiente =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[IngresoMovIndependiente] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     CorteI.idCorte, CorteI.codigo, CorteI.corte, SucursalCorte.idSucursal, SucursalCorte.sucursal,
							  (SELECT     SUM(dbo.CortePorMovimiento.cantKg) AS Expr1
								FROM          dbo.Corte INNER JOIN
													   dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
													   dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
													   dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
													   dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal
								WHERE      (CorteI.idCorteMaestro = CorteM.idCorte) AND (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND 
													   @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal)
								GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal) * CorteI.porcentaje / 100 AS StockIngreso
	FROM         dbo.Corte AS CorteI CROSS JOIN
						  dbo.Sucursal AS SucursalCorte
	WHERE     (SucursalCorte.idSucursal = @idSucursal)

END

GO
-- ===== modificarCompra =====

CREATE PROCEDURE [dbo].[modificarCompra]
    @idCompra int,
    @nroRemito nvarchar(50) = '',
    @fechaCompra datetime,
    @idProveedor int,
    @cantMedias int = null,
    @kgsMedias int = null,
    @estado nvarchar(50),
    @observaciones nvarchar(max),
    @tipoCompra nvarchar(50),
    @idSucursal int,
    @actualizadoPor int = null,
    @enCtaCte tinyint = 0,
    @idPesajeAjustado int = null
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Compras
    SET nroRemito = @nroRemito,
        fechaCompra = @fechaCompra,
        idProveedor = @idProveedor,
        cantMedias = @cantMedias,
        kgsMedias = @kgsMedias,
        estado = @estado,
        observaciones = @observaciones,
        tipoCompra = @tipoCompra,
        idSucursal = @idSucursal,
        actualizado = SYSDATETIME(),
        actualizadoPor = @actualizadoPor,
        enCtaCte = @enCtaCte,
        idPesajeAjustado = @idPesajeAjustado
    WHERE idCompra = @idCompra;
END

GO
-- ===== modificarCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarCorte] 
	-- Add the parameters for the stored procedure here
	@idCorte int,
	@codigo int,
	@corte nvarchar(50),
	@tipo nvarchar(50),
	@independiente int,
	@precioKg float,
	@idCorteMaestro int,
	@porcentaje float,
	@porcentajeHueso float,
	@desvioEstandar float
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
   	update Corte set codigo=@codigo,corte=@corte, precioKg=@precioKg, tipo=@tipo,independiente=@independiente,idCorteMaestro=@idCorteMaestro, porcentaje=@porcentaje, porcentajeHueso=@porcentajeHueso,desvioEstandar=@desvioEstandar, actualizado = SYSDATETIME() where idCorte=@idCorte

	-----Se crea el registro del historial
	insert into ActualizacionCorte (idCorte, codigo, corte, precioKg, tipo, independiente, idCorteMaestro,porcentaje,porcentajeHueso, desvioEstandar, creado, actualizado)
			 values (@idCorte, @codigo,@corte,@precioKg,@tipo, @independiente,@idCorteMaestro,@porcentaje,@porcentajeHueso, @desvioEstandar, null, SYSDATETIME())
			

 --   if ( @idCorteMaestro = -1)
	--	begin
	--		update Corte set codigo=@codigo,corte=@corte, precioKg=@precioKg, tipo=@tipo,independiente=@independiente,idCorteMaestro=@idCorte, porcentaje=@porcentaje, porcentajeHueso=@porcentajeHueso where idCorte=@idCorte
	--	end
	--else
	--	begin
	--		update Corte set codigo=@codigo,corte=@corte, precioKg=@precioKg, tipo=@tipo,independiente=@independiente,idCorteMaestro=@idCorteMaestro, porcentaje=@porcentaje, porcentajeHueso=@porcentajeHueso where idCorte=@idCorte
	--	end
			
			
END

GO
-- ===== modificarLineaVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarLineaVenta] 
	@idVenta int,
	@idCorte int,
	@idAnulado int, --0 Activo --1 Anulado
	@cantKg float,
	@precioKg float
	
	
AS
BEGIN --Se re
	
	SET NOCOUNT ON;

	
		
		
	---SE VUELVEN A AGREGAR LAS LINEA Y ACTUALIZAR EL STOCK
	
	insert into LineaVenta (idVenta,idCorte,idAnulado,cantKg,precioKg)
	values (@idVenta,@idCorte,@idAnulado,@cantKg,@precioKg)
	
	
	------ Se actualiza stock del corte ingresado
	--update StockCorteSucursal set stock=(stock-@cantKg)
	--FROM  SuperCerdo.dbo.StockCorteSucursal, Ventas,
	--	SuperCerdo.dbo.Corte as CorteP
	--WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta 
		 
	--	and CorteP.idCorte=@idCorte
		
		
	---- Se actualizan todos los sub-cortes del corte ingresado
	--update StockCorteSucursal 
	--	set stock=(stock- (@cantKg* (CorteP.porcentaje / 100) ))
	--FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, Ventas,
	--	SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM		 
	--	WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta
		
	--	and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorte=@idCorte and CorteP.idCorte<>CorteM.idCorte
				
	---- Se actualizan todos los cortes nivel 3
	--update StockCorteSucursal 
	--	set stock=(stock- (@cantKg *((CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	--FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, Ventas,
	--	SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, SuperCerdo.dbo.Corte as CorteMedia		 
	--WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta		
	--	and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorteMaestro=CorteMedia.idCorte 
	--	and CorteMedia.idCorte=@idCorte and CorteP.idCorte<>CorteM.idCorte
				
	---- Se Actualizan todos los cortes nivel 4
	--update StockCorteSucursal 
	--	set stock=(stock- (@cantKg * (CorteSubMedia.porcentaje/100)*(CorteM.porcentaje/100) * (CorteP.porcentaje / 100)))
	--FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, Ventas,
	--	SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, SuperCerdo.dbo.Corte as CorteSubMedia,
	--	SuperCerdo.dbo.Corte as CorteMedia
	--WHERE StockCorteSucursal.idCorte=CorteP.idCorte and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta
	--	and CorteP.idCorteMaestro=CorteM.idCorte and CorteM.idCorteMaestro=CorteSubMedia.idCorte
	--	and CorteSubMedia.idCorteMaestro=CorteMedia.idCorte and CorteMedia.idCorte=@idCorte and  CorteP.idCorte<>CorteM.idCorte

	------Actulizar los cortes del cual deriva el corte ingresado

		
	---- Se actualiza el corte superior del corte ingresado
	--update StockCorteSucursal 
	--	set stock=(stock- @cantKg )
	--FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, Ventas,
	--	SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM		 
	--	WHERE StockCorteSucursal.idCorte=CorteM.idCorte and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta
		
	--	and CorteM.idCorte=CorteP.idCorteMaestro and CorteP.idCorte=@idCorte
				
	---- Se actualiza el corte maestro del corte superior
	--update StockCorteSucursal 
	--	set stock=(stock - @cantKg)
	--FROM  SuperCerdo.dbo.StockCorteSucursal as StockCorteSucursal, Ventas,
	--	SuperCerdo.dbo.Corte as CorteP, SuperCerdo.dbo.Corte as CorteM, SuperCerdo.dbo.Corte as CorteMedia		 
	--WHERE StockCorteSucursal.idCorte=CorteMedia.idCorte and StockCorteSucursal.idSucursal=Ventas.idSucursal and Ventas.idVenta=@idVenta		
	--	and CorteMedia.idCorte=CorteM.idCorteMaestro and CorteM.idCorte=CorteP.idCorteMaestro and 
GO
-- ===== modificarMediaPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarMediaPorCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@idMedia int,
	@idSucursal int,
	@nroTropa nvarchar(50),
	@precioMedia float,
	@kgMedia float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	--if exists (select MediaRes.idCompra from MediaRes where MediaRes.idCompra=@idCompra)
	--update MediaRes set nroTropa=@nroTropa,idSucursal=@idSucursal,kgMedia=@kgMedia,precioMedia=@precioMedia
	--else
	insert into MediaRes (nroTropa, idCompra, idSucursal, kgMedia, precioMedia)
	values (@nroTropa,@idCompra,@idSucursal,@kgMedia,@precioMedia)
	
	
	
END

GO
-- ===== modificarMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarMovimiento] 
	-- Add the parameters for the stored procedure here
	@idMovimiento int,
	@fechaMovimiento datetime,
	@sucursalOrigen int,
	@sucursalDestino int,
	@observaciones nvarchar(max),
	@actualizadoPor int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update Movimiento set fechaMovimiento=@fechaMovimiento,sucursalOrigen=@sucursalOrigen,sucursalDestino=@sucursalDestino,observaciones=@observaciones, actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor
	where idMovimiento=@idMovimiento

END

GO
-- ===== modificarPersona =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarPersona] 
	
	@otrosDatos nvarchar(200), 
	@idPersona  int,
	@tipo nvarchar(50),
	@razonSocial nvarchar(50)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update Personas set razonSocial=@razonSocial, tipo=@tipo, otrosDatos= @otrosDatos 
	where idPersona =@idPersona 
END

GO
-- ===== modificarPrecioMedia =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarPrecioMedia] 
	-- Add the parameters for the stored procedure here
	@idCompra int,	
	@precioKg float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update dbo.MediaRes set precioMedia=@precioKg FROM     dbo.MediaRes INNER JOIN
                      dbo.Compras ON dbo.MediaRes.idCompra = dbo.Compras.idCompra
	where dbo.Compras.idCompra =@idCompra
END

GO
-- ===== ModificarPrecioPorPorcentaje =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ModificarPrecioPorPorcentaje]
	-- Add the parameters for the stored procedure here
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE Corte set precioKg = round(precioKg*1.05,2)	
	  where tipo<>'Unidad' 
	  and corte not like '%chori%'
	  and corte not like '%cuero%'
END

GO
-- ===== modificarProveedor =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[modificarProveedor] 
	-- Add the parameters for the stored procedure here
	@otrosDatos varchar(200), 
	@idProveedor int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update proveedores set otrosDatos= @otrosDatos 
	where idProveedor=@idProveedor
END

GO
-- ===== modificarVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[modificarVenta] 
	-- Add the parameters for the stored procedure here
	@idVenta int, 
	@fechaVenta datetime,
	@idSucursal int,
	@idSucNueva int,
	@tipoVenta nvarchar(50) = null,
	@idVendedor int = null,
	@turno nvarchar(50),
	@diaFestivo nvarchar(50) = null,
	@observaciones nvarchar(200),
	@idPersona int,
	@nroRemito nvarchar(50),
	@estado nvarchar(50),
	@eliminarLineas tinyint = 1,
	@enCtaCte tinyint = 0,
	@idEgresoCaja int = 0,	
	@montoEgresoCaja float = 0,
	@formaPago nvarchar(50),
	@tipoComprobante char = null,
	@cuit nvarchar(50) = null,
	@email nvarchar(50) = null,
	@acumRedondeoKgs float = 0,
	@acumRedondeoImporte float = 0,
	@comisionTarjeta float = 0,
	@pagoMixtoEfectivo float = 0
		
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
    -- Insert statements for procedure here
	-- SE ELIMINAN LAS LINEAS DE VENTAS
	IF @eliminarLineas = 1
		BEGIN
			delete from LineaVenta where idVenta=@idVenta 	
		END
	
	-- SE MODIFICA DATOS DE LA VENTA
	update Ventas set fechaVenta=@fechaVenta, turno=@turno,idSucursal=@idSucNueva, idVendedor = @idVendedor,			diaFestivo=@diaFestivo,
		observaciones=@observaciones,idPersona=@idPersona,nroRemito=@nroRemito,estado=@estado, 
		enCtaCte=@enCtaCte, formaPago=@formaPago, tipoComprobante=@tipoComprobante, cuit=@cuit,
		email=@email, acumRedondeoKgs=@acumRedondeoKgs, acumRedondeoImporte=@acumRedondeoImporte,					comisionTarjeta = @comisionTarjeta, pagoMixtoEfectivo = @pagoMixtoEfectivo, actualizado = SYSDATETIME() from Ventas
		where idVenta=@idVenta
		
		
	--Si tiene egreso de caja por venta cta cte se genera un registro opuesto	

	set @montoEgresoCaja = (SELECT  top 1 monto
							FROM EgresosCaja
							WHERE     (tabla = 'Ventas') AND (idTabla = @idVenta)
							ORDER BY EgresosCaja.id desc)
	IF @montoEgresoCaja > 0
		BEGIN
			INSERT INTO EgresosCaja
			(fechaHora, idTipoEgresoCaja, descripcion, detalle, monto, idSucursal, creado, creadoPor,		 actualizado, actualizadoPor, idCompra, esGasto, tabla, idTabla)
			SELECT  top 1 fechaHora, idTipoEgresoCaja, descripcion, detalle, monto, idSucursal, creado, creadoPor,		 actualizado, actualizadoPor, idCompra, esGasto, tabla, idTabla
			FROM EgresosCaja
			WHERE     (tabla = 'Ventas') AND (idTabla = @idVenta)
			ORDER BY EgresosCaja.id desc;
			
			set @idEgresoCaja = SCOPE_IDENTITY() 
			update dbo.EgresosCaja 
				set descripcion = ('Anulado:'+ descripcion)  ,monto = (-1 * monto), creado = SYSDATETIME(), actualizado = null, actualizadoPor = null
			where id = @idEgresoCaja
		END
END

GO
-- ===== obtenerCompras =====

-- Agrega dbo.Compras.idPesajeAjustado a cada uno de los 14 SELECT/GROUP BY (7 tipoCompra x 2 ramas
-- @idSucursal>0 / ELSE) de obtenerCompras. Sin esta columna, la grilla de /Stock no puede mostrar a
-- que Pesaje/Compra/Ajuste esta vinculado un registro (Web/Controllers/StockController.cs,
-- ConstruirDetalleIndexLiviano, lee row["idPesajeAjustado"] con guard defensivo Columns.Contains --
-- fallaba en silencio porque el SP nunca proyectaba la columna). El resto del cuerpo queda igual al
-- original (extraido con sp_helptext antes de este cambio), solo se agrega la columna nueva.
CREATE PROCEDURE [dbo].[obtenerCompras]
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@fechaDesde datetime,
	@fechaHasta datetime,
	@tipoCompra nvarchar(50),
	@idSucursal int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    IF @idSucursal > 0
		BEGIN
			(SELECT     dbo.Compras.idCompra, dbo.Compras.idPesajeAjustado, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
						  SUM(dbo.MediaRes.kgMedia) AS cantKg, SUM(dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia) AS totalS, Compras.cantMedias, dbo.Compras.estado,
						  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor
			FROM         dbo.Compras INNER JOIN
								  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE (dbo.Compras.tipoCompra like '%'+@tipoCompra+'%' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  and dbo.Compras.fechaCompra between @fechaDesde and @fechaHasta+1 and ((dbo.Personas.razonSocial like '%'+@texto+'%')or(dbo.Compras.nroRemito like '%'+@texto+'%' ) )
			GROUP BY  dbo.Compras.idCompra, dbo.Compras.idPesajeAjustado, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,Compras.cantMedias, dbo.Compras.estado,
								  dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre, ActualizadoPor.nombre, dbo.Compras.actualizado
			UNION
			SELECT     dbo.Compras.idCompra, dbo.Compras.idPesajeAjustado, dbo.Compras.nroRemito, dbo.Compras.fechaCompra, dbo.Compras.idProveedor, dbo.Personas.razonSocial, dbo.Compras.tipoCompra, dbo.Compras.idSucursal, dbo.Sucursal.sucursal,
								  SUM(dbo.CortePorCompra.cantKg) AS cantKg, SUM(dbo.CortePorCompra.precioKg* dbo.CortePorCompra.cantKg) AS totalS, Compras.cantMedias, dbo.Compras.estado, dbo.Compras.observaciones, dbo.Compras.creado, CreadoPor.nombre AS creadoPor, dbo.Compras.actualizado,
                      ActualizadoPor.nombre as actualizadoPor
			FROM         dbo.CortePorCompra INNER JOIN
								  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra INNER JOIN
								  dbo.Personas ON dbo.Compras.idProveedor = dbo.Personas.idPersona INNER JOIN dbo.Sucursal ON dbo.Compras.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Compras.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Compras.creadoPor = CreadoPor.id
			WHERE dbo.Compras.tipoCompra like 'Cortes' and (@tipoCompra like '' or @tipoCompra like 'Cortes' or @tipoCompra like 'Todos' ) and dbo.Compras.idSucursal = @idSucursal  an
GO
-- ===== obtenerCortes =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerCortes] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--SELECT corteP.idCorte, corteP.codigo, corteP.corte, corteP.tipo, corteP.idCorteMaestro, corteM.corte, corteP.porcentaje from Corte corteP, Corte corteM
	--where corteP.idCorte=corteM.idCorte
	SELECT     CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.mayorista, CorteP.enCierreStock, CorteP.tipo, CorteP.idCorteMaestro, CorteM.corte AS corteMaestro, CorteP.porcentaje, 
                      CorteP.porcentajeHueso, CorteP.desvioEstandar, CorteP.independiente, CorteP.promedio
FROM         dbo.Corte AS CorteM RIGHT OUTER JOIN
                      dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro

order by CorteP.codigo

END

GO
-- ===== obtenerCortesPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerCortesPorCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.CortePorCompra.idCortePorCompra, dbo.CortePorCompra.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorCompra.cantKg, dbo.CortePorCompra.precioKg, 
                      dbo.CortePorCompra.cantKg * dbo.CortePorCompra.precioKg AS totalS, dbo.CortePorCompra.balanza,  dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, dbo.CortePorCompra.creado, dbo.CortePorCompra.creadoPor
	FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte INNER JOIN
                      dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal
	 where CortePorCompra.idCompra=@idCompra
	 order by dbo.Corte.codigo            
END

GO
-- ===== obtenerCortesPorEmbutidos =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerCortesPorEmbutidos] 
	-- Add the parameters for the stored procedure here
	@idEmbutido int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT   dbo.CortePorEmbutido.idEmbutido, dbo.CortePorEmbutido.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorEmbutido.kgUtilizados, dbo.CortePorEmbutido.pesoBalanza
	FROM         dbo.Corte INNER JOIN
              dbo.CortePorEmbutido ON dbo.Corte.idCorte = dbo.CortePorEmbutido.idCorte INNER JOIN
              dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido
	where Embutidos.idEmbutido=@idEmbutido
	
END

GO
-- ===== ObtenerCortesPrimarios =====
-- =============================================
-- Author:		
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[ObtenerCortesPrimarios] 
	-- Add the parameters for the stored procedure here
	@p1 int = 0, 
	@p2 int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT @p1, @p2
END

GO
-- ===== obtenerEgresosCaja =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obtenerEgresosCaja]
	-- Add the parameters for the stored procedure here
	@id int = 0,
	@idTipoEgresoCaja int = 0,
	@idSucursal int = 0,
	@fechaDesde datetime = null,
	@fechaHasta datetime = null,
	--@fechaActual datetime = CONVERT(date, SYSDATETIME()),
	@idVendedor int = -1,
	@texto nvarchar(50) = '',
	@montoEgresoCaja tinyint = 0,
	@verEgresoCaja tinyint = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Se obtiene todos los EgresosCaja asociados al Cajero entre la Apertura y Cierre de caja
	IF @montoEgresoCaja = 1	
		BEGIN
			SELECT ROUND(sum(dbo.EgresosCaja.monto), 2) as monto
			FROM      dbo.EgresosCaja 
			WHERE dbo.EgresosCaja.fechaHora between @fechaDesde and @fechaHasta and EgresosCaja.idSucursal = @idSucursal and					dbo.EgresosCaja.creadoPor = @idVendedor
		END
	ELSE
		BEGIN
			IF @id > 0
				BEGIN
					SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora, dbo.EgresosCaja.idTipoEgresoCaja, dbo.TiposEgresoCaja.tipoEgresoCaja, dbo.EgresosCaja.descripcion, 
							dbo.EgresosCaja.detalle as detalle,  
							ROUND(dbo.EgresosCaja.monto, 2) as monto, dbo.EgresosCaja.idCompra, dbo.EgresosCaja.idSucursal, dbo.EgresosCaja.creado, 
							  dbo.EgresosCaja.creadoPor, dbo.EgresosCaja.actualizado, dbo.EgresosCaja.actualizadoPor
					FROM      dbo.EgresosCaja INNER JOIN dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id
					WHERE dbo.EgresosCaja.id = @id
				END
			ELSE
				BEGIN
					IF @idTipoEgresoCaja > 0
						BEGIN
							SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora as 'Fecha', dbo.TiposEgresoCaja.tipoEgresoCaja as 'TipoEgresoCaja', 
							dbo.EgresosCaja.descripcion as 'Descripción', dbo.EgresosCaja.detalle as 'Detalle', 
							ROUND(dbo.EgresosCaja.monto, 2) as Monto, dbo.TiposEgresoCaja.esGasto as 'Gasto', dbo.EgresosCaja.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
							dbo.EgresosCaja.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
							FROM      dbo.EgresosCaja INNER JOIN
									  dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id LEFT OUTER JOIN
									  dbo.Usuarios AS CreadoPor ON dbo.EgresosCaja.creadoPor = CreadoPor.id LEFT OUTER JOIN
									  dbo.Usuarios AS ActualizadoPor ON dbo.EgresosCaja.actualizadoPor = ActualizadoPor.id	
							WHERE  dbo.EgresosCaja.fechaHora between @fechaDesde and 
									@fechaHasta and dbo.EgresosCaja.idTipoEgresoCaja = @idTipoEgresoCaja and 
									(@idSucursal = 0 OR EgresosCaja.idSucursal = @idSucursal)
									--EgresosCaja.idSucursal = @idSucursal 
										 and dbo.EgresosCaja.descripcion like '%'+@texto+'%' 									
									  AND (
											@idVendedor <= 0 
											OR dbo.EgresosCaja.creadoPor = @idVendedor
										  )
							ORDER BY dbo.EgresosCaja.fechaHora desc, dbo.EgresosCaja.id DESC
						END
					ELSE
						BEGIN
							IF @verEgresoCaja = 1
								SELECT  dbo.EgresosCaja.id, dbo.EgresosCaja.fechaHora as 'Fecha', dbo.TiposEgresoCaja.tipoEgresoCaja as 'TipoEgresoCaja', 
									dbo.EgresosCaja.descripcion as 'Descripción', dbo.EgresosCaja.detalle as 'Detalle', 
									ROUND(dbo.EgresosCaja.monto, 2) as Monto, dbo.TiposEgresoCaja.esGasto as 'Gasto', dbo.EgresosCaja.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.EgresosCaja.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
								FROM      dbo.EgresosCaja INNER JOIN
										  dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id LEFT OUTER JOIN
										  dbo.Usuarios AS CreadoPor ON dbo.EgresosCaja.creadoPor = CreadoPor.id LEFT OUTER JOIN
	
GO
-- ===== obtenerEmbutidos =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerEmbutidos] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50)
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.tipo, CorteP.idCorteMaestro, CorteM.corte AS corteMaestro, CorteP.porcentaje, StockSL.idSucursal AS idSucursalSL,
                       SucursalSL.sucursal AS sucursalSL, StockSL.stock AS stockSL, StockSM.idSucursal AS idSucursalSM, SucursalSM.sucursal AS sucursalSM, StockSM.stock AS stockSM
FROM         dbo.Corte AS CorteM INNER JOIN
                      dbo.Sucursal AS SucursalSM INNER JOIN
                      dbo.Corte AS CorteP INNER JOIN
                      dbo.StockCorteSucursal AS StockSM ON CorteP.idCorte = StockSM.idCorte ON SucursalSM.idSucursal = StockSM.idSucursal INNER JOIN
                      dbo.StockCorteSucursal AS StockSL ON CorteP.idCorte = StockSL.idCorte INNER JOIN
                      dbo.Sucursal AS SucursalSL ON StockSL.idSucursal = SucursalSL.idSucursal ON CorteM.idCorte = CorteP.idCorteMaestro
WHERE     (StockSM.idSucursal = 2) AND (StockSL.idSucursal = 1) and CorteP.tipo = 'Embutido' and CorteP.corte like '%'+@texto+'%'
END

GO
-- ===== obtenerEmbutidoTotal =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerEmbutidoTotal] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Embutidos.idEmbutido, dbo.Embutidos.fechaEmbutido, dbo.Embutidos.idCorte, CorteEmbutido.codigo, CorteEmbutido.corte, 
                      SUM(dbo.CortePorEmbutido.kgUtilizados) AS totalKg
FROM         dbo.Corte INNER JOIN
                      dbo.CortePorEmbutido ON dbo.Corte.idCorte = dbo.CortePorEmbutido.idCorte INNER JOIN
                      dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
                      dbo.Corte AS CorteEmbutido ON dbo.Embutidos.idCorte = CorteEmbutido.idCorte
	WHERE fechaEmbutido between @fechaDesde and @fechaHasta+1 and ((CAST( CorteEmbutido.codigo as nvarchar(50)) = @texto )or(CorteEmbutido.corte like '%'+@texto+'%' ))
GROUP BY dbo.Embutidos.idEmbutido, dbo.Embutidos.fechaEmbutido, dbo.Embutidos.idCorte, CorteEmbutido.codigo, CorteEmbutido.corte
order by dbo.Embutidos.fechaEmbutido desc

END

GO
-- ===== obtenerGastos =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obtenerGastos]
	-- Add the parameters for the stored procedure here
	@id int = 0,
	@idTipoGasto int = 0,
	@idSucursal int = 0,
	@fechaDesde datetime = null,
	@fechaHasta datetime = null,
	--@fechaActual datetime = CONVERT(date, SYSDATETIME()),
	@idVendedor int = 0,
	@texto nvarchar(50) = '',
	@montoGasto tinyint = 0,
	@verGasto tinyint = 0
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Se obtiene todos los gastos asociados al Cajero entre la Apertura y Cierre de caja
	IF @montoGasto = 1	
		BEGIN
			SELECT ROUND(sum(dbo.Gastos.monto), 2) as monto
			FROM      dbo.Gastos 
			WHERE dbo.Gastos.fechaHora between @fechaDesde and SYSDATETIME() and Gastos.idSucursal = @idSucursal and					dbo.Gastos.creadoPor = @idVendedor
		END
	ELSE
		BEGIN
			IF @id > 0
				BEGIN
					SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora, dbo.Gastos.idTipoGasto, dbo.TipoGasto.tipoGasto, dbo.Gastos.descripcion, 
							dbo.Gastos.detalle as detalle,  
							ROUND(dbo.Gastos.monto, 2) as monto, dbo.Gastos.idSucursal, dbo.Gastos.creado, 
							  dbo.Gastos.creadoPor, dbo.Gastos.actualizado, dbo.Gastos.actualizadoPor
					FROM      dbo.Gastos INNER JOIN dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id
					WHERE dbo.Gastos.id = @id
				END
			ELSE
				BEGIN
					IF @idTipoGasto > 0
						BEGIN
							SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora as 'Fecha', dbo.TipoGasto.tipoGasto as 'Tipo Gasto', 
							dbo.Gastos.descripcion as 'Descripción', dbo.Gastos.detalle as 'Detalle', 
							ROUND(dbo.Gastos.monto, 2) as Monto, dbo.Gastos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
							dbo.Gastos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
							FROM      dbo.Gastos INNER JOIN
									  dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id LEFT OUTER JOIN
									  dbo.Usuarios AS CreadoPor ON dbo.Gastos.creadoPor = CreadoPor.id LEFT OUTER JOIN
									  dbo.Usuarios AS ActualizadoPor ON dbo.Gastos.actualizadoPor = ActualizadoPor.id	
							WHERE  dbo.Gastos.fechaHora between @fechaDesde and 
									@fechaHasta and dbo.Gastos.idTipoGasto = @idTipoGasto and 
									Gastos.idSucursal = @idSucursal and ( dbo.Gastos.descripcion like '%'+@texto+'%' 
										or	ActualizadoPor.nombre like '%'+@texto+'%')
							ORDER BY dbo.Gastos.fechaHora DESC
						END
					ELSE
						BEGIN
							IF @verGasto = 1
								SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora as 'Fecha', dbo.TipoGasto.tipoGasto as 'Tipo Gasto', 
									dbo.Gastos.descripcion as 'Descripción', dbo.Gastos.detalle as 'Detalle', 
									ROUND(dbo.Gastos.monto, 2) as Monto, dbo.Gastos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.Gastos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
								FROM      dbo.Gastos INNER JOIN
										  dbo.TipoGasto ON dbo.Gastos.idTipoGasto = dbo.TipoGasto.id LEFT OUTER JOIN
										  dbo.Usuarios AS CreadoPor ON dbo.Gastos.creadoPor = CreadoPor.id LEFT OUTER JOIN
										  dbo.Usuarios AS ActualizadoPor ON dbo.Gastos.actualizadoPor = ActualizadoPor.id	
								WHERE dbo.Gastos.fechaHora between @fechaDesde and SYSDATETIME() and Gastos.idSucursal = @idSucursal and 
										dbo.Gastos.creadoPor = @idVendedor
								ORDER BY dbo.Gastos.fechaHora DESC
							ELSE
								SELECT  dbo.Gastos.id, dbo.Gastos.fechaHora as 'Fecha', dbo.TipoGasto.tipoGasto as 'Tipo Gasto', 
									dbo.Gastos.descripcion as 'Descripción', dbo.Gastos.detalle as 'Detalle', 
									ROUND(dbo.Gastos.monto, 2) as Monto, dbo.Gastos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
									dbo.Gastos.actualizado as 'A
GO
-- ===== obtenerInfoCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	Obtiene la información completa del corte seleccionado
-- =============================================
CREATE PROCEDURE [dbo].[obtenerInfoCorte] 
	-- Add the parameters for the stored procedure here
	@idCorte int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		SELECT CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.nivel, CorteP.independiente, CorteP.ingresoRapidoEmbutido, CorteP.habilitado, CorteP.enCierreStock, CorteP.tipo, CorteP.idCorteMaestro, 
                  CorteM.corte AS corteMaestro, CorteP.porcentaje, CorteP.porcentajeHueso, CorteP.desvioEstandar, CorteP.promedio, CorteP.alicuotaIva, CorteP.idMarca, dbo.Personas.identificacion as marca
FROM     dbo.Corte AS CorteP LEFT OUTER JOIN
                  dbo.Personas ON CorteP.idMarca = dbo.Personas.idPersona LEFT OUTER JOIN
                  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte
WHERE     (CorteP.idCorte=@idCorte)
END

GO
-- ===== obtenerLineasEmb =====

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasEmb] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idSucursal int = 0,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT     dbo.Embutidos.idEmbutido as 'Id', dbo.Embutidos.fechaEmbutido as 'Fecha',CorteEmbutido.codigo as 'Cod.Emb', CorteEmbutido.corte as 'Embutido', dbo.Corte.codigo AS 'Codigo', dbo.Corte.corte AS 'Corte', 
                      dbo.CortePorEmbutido.kgUtilizados as 'Kgs', dbo.CortePorEmbutido.pesoBalanza as 'Balanza', dbo.Sucursal.sucursal as 'Sucursal', dbo.Embutidos.estado  as 'Estado', 'Observaciones' = case  
                      when LEN(dbo.Embutidos.observaciones) <= 20 then dbo.Embutidos.observaciones
                      else (SUBSTRING(dbo.Embutidos.observaciones, 1, 20) + '...') end ,                       
                      dbo.Embutidos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por', 
                      dbo.Embutidos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
FROM          dbo.Corte INNER JOIN
                      dbo.CortePorEmbutido ON dbo.Corte.idCorte = dbo.CortePorEmbutido.idCorte INNER JOIN
                      dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
                      dbo.Corte AS CorteEmbutido ON dbo.Embutidos.idCorte = CorteEmbutido.idCorte INNER JOIN
                      dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Embutidos.creadoPor = CreadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Embutidos.actualizadoPor = ActualizadoPor.id
WHERE fechaEmbutido between @fechaDesde and @fechaHasta 
and 
(
(@idSucursal > 0 and dbo.Sucursal.idSucursal = @idSucursal) or 
dbo.Embutidos.idSucursal > 0
) 
and ((CAST(dbo.Embutidos.idEmbutido as nvarchar(50)) = @texto )or (CAST( CorteEmbutido.codigo as nvarchar(50)) = @texto )or(CorteEmbutido.corte like '%'+@texto+'%' ) or (CAST( dbo.Corte.codigo as nvarchar(50)) = @texto )or(dbo.Corte.corte like '%'+@texto+'%' ) or(CreadoPor.nombre like '%'+@texto+'%' ) or (ActualizadoPor.nombre like '%'+@texto+'%' ))                 
ORDER BY fechaEmbutido DESC   

END


GO
-- ===== obtenerLineasMov =====

-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasMov] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(200),
	@sucOrigen nvarchar(50),
	@sucDestino nvarchar(50),
	@fechaDesde datetime,
	@fechaHasta datetime	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;	

		SELECT     dbo.Movimiento.idMovimiento AS 'Id Movimiento', dbo.Movimiento.fechaMovimiento AS 'Fecha Movimiento', 
		dbo.Corte.codigo as 'Codigo', dbo.Corte.corte as 'Corte', (dbo.CortePorMovimiento.cantUnidad) AS 'Total Un.', 
		CAST((dbo.CortePorMovimiento.cantKg) AS decimal(10, 3)) AS 'Total Kg', CAST(dbo.CortePorMovimiento.permitirIngreso AS bit) AS 'Permitir Ingr.', dbo.CortePorMovimiento.pesoBalanza as 'Balanza',  
		CASE WHEN (actualizacionCompleta = 2) THEN 'PENDIENTE' WHEN (dbo.Movimiento.idMovOrigen > 0 AND actualizacionCompleta = 1) OR
		(dbo.Movimiento.idMovOrigen IS NULL) THEN 'OK' ELSE 'ERROR' END AS 'Estado', SucursalOrigen.sucursal AS Origen, dbo.Movimiento.idMovOrigen AS 'Id Origen', SucursalDestino.sucursal AS Destino, dbo.Movimiento.observaciones, dbo.Movimiento.creado, CreadoPor.nombre AS 'creado por', 
		dbo.Movimiento.actualizado, ActualizadoPor.nombre AS 'actualizado por'
		FROM         dbo.CortePorMovimiento INNER JOIN
		dbo.Sucursal AS SucursalOrigen INNER JOIN
		dbo.Movimiento ON SucursalOrigen.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
		dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
		dbo.Corte ON dbo.CortePorMovimiento.idCorte = dbo.Corte.idCorte LEFT OUTER JOIN
		dbo.Usuarios AS ActualizadoPor ON dbo.Movimiento.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
		dbo.Usuarios AS CreadoPor ON dbo.Movimiento.creadoPor = CreadoPor.id
		WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (SucursalOrigen.sucursal LIKE '%' + @sucOrigen + '%') AND 
		(SucursalDestino.sucursal LIKE '%' + @sucDestino + '%') AND 
		((CAST(dbo.Movimiento.idMovimiento AS nvarchar(50)) = @texto) OR
		(CAST(dbo.Corte.codigo AS nvarchar(50)) = @texto) OR
		(CAST(dbo.Corte.corte AS nvarchar(50)) LIKE '%' + @texto + '%'))	
	order by fechaMovimiento desc
	
END


GO
-- ===== obtenerLineasVenta =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerLineasVenta] 
	-- Add the parameters for the stored procedure here
	@idVenta int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	
	
	SELECT     dbo.LineaVenta.idLineaVenta, dbo.LineaVenta.idVenta, dbo.LineaVenta.idCorte, dbo.Corte.codigo, 
	dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.kgsAjusteTarj, dbo.LineaVenta.porcKgsAjusteTarj, 
	dbo.LineaVenta.idAlicuotaIva, dbo.LineaVenta.alicuotaIva,dbo.LineaVenta.precioKg, dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalS, dbo.LineaVenta.bonificacion, dbo.LineaVenta.pesoBalanza, 
                'estado' = 
				 CASE
					  WHEN dbo.LineaVenta.idAnulado=0 THEN ''
					  ELSE 'Anulado'
				 END,
				 dbo.LineaVenta.idLineaVentaAnulado
               
	FROM         dbo.LineaVenta INNER JOIN
                      dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte
    WHERE LineaVenta.idVenta=@idVenta
    order by dbo.Corte.codigo
	
 --   -- Insert statements for procedure here
	--SELECT     dbo.LineaVenta.idVenta, dbo.LineaVenta.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, 
 --                     dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalS, dbo.LineaVenta.idAnulado AS estado
	--FROM         dbo.LineaVenta INNER JOIN
 --                     dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte
 --   WHERE LineaVenta.idVenta=@idVenta
END

GO
-- ===== obtenerMediasPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerMediasPorCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT    dbo.MediaRes.idMedia, dbo.MediaRes.nroTropa, dbo.MediaRes.kgMedia, dbo.MediaRes.precioMedia, dbo.MediaRes.kgMedia * dbo.MediaRes.precioMedia AS totalS,
                       dbo.MediaRes.idSucursal, dbo.Sucursal.sucursal
	FROM         dbo.Compras INNER JOIN
                      dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
                      dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
     where MediaRes.idCompra=@idCompra   
END

GO
-- ===== obtenerMovimientos =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[obtenerMovimientos] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(200),
	@sucOrigen nvarchar(50),
	@sucDestino nvarchar(50),
	@fechaDesde datetime,
	@fechaHasta datetime
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	((SELECT    dbo.Movimiento.idMovimiento as 'Id Movimiento',  dbo.Movimiento.fechaMovimiento as 'Fecha Movimiento', SucursalOrigen.sucursal as Origen, dbo.Movimiento.idMovOrigen as 'Id Origen', CASE 
            WHEN (actualizacionCompleta = 2)
               THEN 'PENDIENTE' 
            WHEN (dbo.Movimiento.idMovOrigen > 0 and actualizacionCompleta = 1) or (dbo.Movimiento.idMovOrigen IS NULL)
               THEN 'OK'
               ELSE 'ERROR' 
       END as 'Estado', SucursalDestino.sucursal AS Destino, SUM(dbo.CortePorMovimiento.cantUnidad) AS 'Total Un.',CAST(SUM(dbo.CortePorMovimiento.cantKg)as decimal(10,3)) AS 'Total Kg', 
			  dbo.Movimiento.observaciones, dbo.Movimiento.creado, CreadoPor.nombre AS 'creado por', dbo.Movimiento.actualizado, ActualizadoPor.nombre as 'actualizado por'
	FROM         dbo.CortePorMovimiento INNER JOIN
		  dbo.Sucursal AS SucursalOrigen INNER JOIN
		  dbo.Movimiento ON SucursalOrigen.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
		  dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal ON 
		  dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Movimiento.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Movimiento.creadoPor = CreadoPor.id
	WHERE	fechaMovimiento between @fechaDesde and @fechaHasta+1  and SucursalOrigen.sucursal like '%'+@sucOrigen+'%'  and sucursalDestino.sucursal like '%'+@sucDestino+'%'  or (CAST(dbo.Movimiento.idMovimiento as nvarchar(50)) like @texto)      
	GROUP BY dbo.Movimiento.idMovimiento,dbo.Movimiento.fechaMovimiento, SucursalOrigen.sucursal, dbo.Movimiento.idMovOrigen, dbo.Movimiento.actualizacionCompleta, SucursalDestino.sucursal, dbo.Movimiento.observaciones, dbo.Movimiento.creado, CreadoPor.nombre, dbo.Movimiento.actualizado, ActualizadoPor.nombre)

	UNION
	
	(SELECT    dbo.Movimiento.idMovimiento as 'Id Movimiento',  dbo.Movimiento.fechaMovimiento as 'Fecha Movimiento', SucursalOrigen.sucursal as Origen, dbo.Movimiento.idMovOrigen as 'Id Origen', CASE 
            WHEN dbo.Movimiento.idMovOrigen > 0 and dbo.Movimiento.actualizacionCompleta = 0 
               THEN 'Error' 
               ELSE 'OK' 
       END as 'Estado', SucursalDestino.sucursal AS Destino,0 AS 'Total Un.', 0 AS 'Total Kg', 
			  dbo.Movimiento.observaciones, dbo.Movimiento.creado, CreadoPor.nombre AS 'creado por', dbo.Movimiento.actualizado, ActualizadoPor.nombre as 'actualizado por'
	FROM  dbo.Movimiento INNER JOIN
		  dbo.Sucursal AS SucursalOrigen ON SucursalOrigen.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
		  dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Movimiento.actualizadoPor = ActualizadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Movimiento.creadoPor = CreadoPor.id
	WHERE (SELECT COUNT(*) AS cant FROM dbo.CortePorMovimiento 
		WHERE (dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento)) = 0 and	fechaMovimiento between @fechaDesde and @fechaHasta+1  and SucursalOrigen.sucursal like '%'+@sucOrigen+'%'  and sucursalDestino.sucursal like '%'+@sucDestino+'%'  or (CAST(dbo.Movimiento.idMovimiento as nvarchar(50)) like @texto)))
	
	order by fechaMovimiento desc
	
END
GO
-- ===== obtenerNivelCorte =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obtenerNivelCorte] 
	-- Add the parameters for the stored procedure here	
	@idCorteMaestro int,
	@nivel int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE  @idMaestro int;
	
	IF (@idCorteMaestro > 0)
			begin
				set @idMaestro = (SELECT     TOP (1) idCorte
					FROM         dbo.Corte
					WHERE     (@idCorteMaestro IN
							  (SELECT     idCorte
								FROM          dbo.Corte AS Corte_n2
								WHERE      (idCorteMaestro IN
										   (SELECT     idCorte
											 FROM          dbo.Corte AS Corte_n1
											 WHERE      (idCorteMaestro IN
										(SELECT     idCorte
										  FROM          dbo.Corte AS Corte_n0
										  WHERE      (idCorteMaestro IN
											(SELECT     idCorte
											  FROM          dbo.Corte AS Corte_n)))))))))
				IF (@idMaestro > 0)
					begin
						set @nivel = 4;
					end
				
				else
				 begin							  
					set @idMaestro = (SELECT TOP 1 [idCorte]
					  FROM [Corte]
					  where @idCorteMaestro in 
					  (select Corte_n2.idCorte from Corte as Corte_n2 
						where Corte_n2.idCorteMaestro in 
						(select Corte_n1.idCorte from Corte as Corte_n1
						where Corte_n1.idCorteMaestro in 
							(select Corte_n0.idCorte from Corte as Corte_n0))))
							
					--Si encontró maestro se setea Nivel = 3
					IF (@idMaestro > 0)
						begin
							set @nivel = 3;
						end
					
					else
						begin
							set @idMaestro = (SELECT TOP 1 [idCorte]
							  FROM [Corte]
							  where @idCorteMaestro in 
							  (select Corte_n1.idCorte from Corte as Corte_n1 
								where Corte_n1.idCorteMaestro in (select Corte_n0.idCorte from Corte as Corte_n0)))
									
							--Si encontró maestro se setea Nivel = 2
							IF (@idMaestro > 0)
							begin
								set @nivel = 2;
							end
							
							else
								begin
									set @idMaestro = (SELECT TOP 1 [idCorte]
									  FROM [Corte]
									  where @idCorteMaestro in (select Corte_n0.idCorte from Corte as Corte_n0))
											
									--Si encontró maestro se setea Nivel = 3
									IF (@idMaestro > 0)
									begin
										set @nivel = 1;
									end
								end
						end
					end
			end
	END
	
	select @nivel

GO
-- ===== obtenerTemporalLineaVenta =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
--create PROCEDURE obtenerTemporalLineaVenta
CREATE PROCEDURE [dbo].[obtenerTemporalLineaVenta]
	-- Add the parameters for the stored procedure here	
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50),
	@idSucursal int = -1,
	@idVendedor int = -1,
	@conVentas tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	(SELECT  dbo.Usuarios.nombre, dbo.TemporalLineaVenta.fechaInicioPesada, dbo.Corte.codigo, dbo.Corte.corte, dbo.TemporalLineaVenta.cantKg, 
						  dbo.TemporalLineaVenta.precioKg, dbo.TemporalLineaVenta.totalCorte, dbo.Ventas.idVenta, 
						  dbo.TemporalLineaVenta.ventaEnCurso
	FROM         dbo.TemporalLineaVenta INNER JOIN
						  dbo.Corte ON dbo.TemporalLineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
						  dbo.Usuarios ON dbo.TemporalLineaVenta.idVendedor = dbo.Usuarios.id CROSS JOIN
						  dbo.Ventas
	WHERE @conVentas = 1 and dbo.TemporalLineaVenta.ventaEnCurso = 1 and dbo.TemporalLineaVenta.fechaInicioPesada between @fechaDesde and @fechaHasta and (dbo.TemporalLineaVenta.fechaInicioPesada BETWEEN dbo.Ventas.fechaVenta AND dbo.Ventas.creado) and ((@idSucursal < 0 and dbo.TemporalLineaVenta.idSucursal >= 0) 
		or (@idSucursal >= 0 and 
		dbo.TemporalLineaVenta.idSucursal = @idSucursal))
		and ((@idVendedor < 0 and dbo.TemporalLineaVenta.idVendedor >= 0) or (@idVendedor >= 0 and 
		dbo.TemporalLineaVenta.idVendedor = @idVendedor)) and (dbo.Corte.codigo like '%'+@texto+'%' or dbo.Corte.corte like '%'+@texto+'%'))
	union
	(SELECT  dbo.Usuarios.nombre, dbo.TemporalLineaVenta.fechaInicioPesada, dbo.Corte.codigo, dbo.Corte.corte, dbo.TemporalLineaVenta.cantKg, 
						  dbo.TemporalLineaVenta.precioKg, dbo.TemporalLineaVenta.totalCorte, null as idVenta, dbo.TemporalLineaVenta.ventaEnCurso
	FROM         dbo.TemporalLineaVenta INNER JOIN
						  dbo.Corte ON dbo.TemporalLineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
						  dbo.Usuarios ON dbo.TemporalLineaVenta.idVendedor = dbo.Usuarios.id
	WHERE     dbo.TemporalLineaVenta.ventaEnCurso = 0 and dbo.TemporalLineaVenta.fechaInicioPesada between @fechaDesde and @fechaHasta and ((@idSucursal < 0 and dbo.TemporalLineaVenta.idSucursal >= 0) 
		or (@idSucursal >= 0 and 
		dbo.TemporalLineaVenta.idSucursal = @idSucursal))
		and ((@idVendedor < 0 and dbo.TemporalLineaVenta.idVendedor >= 0) or (@idVendedor >= 0 and 
		dbo.TemporalLineaVenta.idVendedor = @idVendedor)) and (dbo.Corte.codigo like '%'+@texto+'%' or dbo.Corte.corte like '%'+@texto+'%'))

	ORDER BY dbo.TemporalLineaVenta.fechaInicioPesada DESC     
     
END

GO
-- ===== obtenerTotalVentas =====

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[obtenerTotalVentas]
	-- Add the parameters for the stored procedure here
	@idVendedor int,
	@idSucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT     SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) AS totalS
	FROM         dbo.LineaVenta INNER JOIN
                      dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
                      dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal 
	WHERE fechaVenta between @fechaDesde and @fechaHasta and Ventas.idVendedor = @idVendedor and Ventas.idSucursal = @idSucursal
END

GO
-- ===== obtenerVentas =====
CREATE PROCEDURE [dbo].[obtenerVentas] 
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50) = '',
	@idSucursal int = -1,
	@idVendedor int = -1,
	@idCliente int = -1,
	@soloAnulados tinyint = 0
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @sql nvarchar(max);
	DECLARE @params nvarchar(max) = '
		@fechaDesde datetime,
		@fechaHasta datetime,
		@texto nvarchar(50),
		@idSucursal int,
		@idVendedor int,
		@idCliente int,
		@soloAnulados tinyint';

	-- Base de la consulta
	SET @sql = N'
	SELECT v.idVenta, v.fechaVenta, v.idVendedor, u.nombre, v.nroRemito, 
		   v.idPersona, p.razonSocial, v.idSucursal, s.sucursal, 
		   v.tipoComprobante, v.enCtaCte, v.formaPago, v.pagoMixtoEfectivo, v.comisionTarjeta,
		   v.turno, v.diaFestivo, v.observaciones, v.creado, v.actualizado,
		   v.estado,
		   SUM(l.kgsAjusteTarj) as totalKgAj, 
		   SUM(l.kgsAjusteTarj * l.precioKg) AS totalImpAj,
		   SUM(l.cantKg) as totalKg, 
		   SUM(l.cantKg * l.precioKg) AS totalS, 
		   (v.comisionTarjeta * SUM(l.cantKg * l.precioKg)) AS totComTarj, 
		   SUM(l.ajustePrecio * l.cantKg) AS totAjuste
	FROM dbo.LineaVenta l
	INNER JOIN dbo.Ventas v ON l.idVenta = v.idVenta
	INNER JOIN dbo.Sucursal s ON v.idSucursal = s.idSucursal
	INNER JOIN dbo.Personas p ON v.idPersona = p.idPersona
	INNER JOIN dbo.Usuarios u ON v.idVendedor = u.id
	WHERE v.fechaVenta BETWEEN @fechaDesde AND @fechaHasta ';

	-- Filtros opcionales
	IF (@idSucursal >= 0)
		SET @sql += ' AND v.idSucursal = @idSucursal';

	IF (@idVendedor >= 0)
		SET @sql += ' AND v.idVendedor = @idVendedor';

	IF (@idCliente >= 0)
		SET @sql += ' AND v.idPersona = @idCliente';

	IF (LTRIM(RTRIM(@texto)) <> '')
		SET @sql += '
		AND (
			CAST(v.idVenta AS nvarchar(50)) LIKE ''%'' + @texto + ''%'' OR
			v.nroRemito LIKE ''%'' + @texto + ''%'' OR
			p.razonSocial LIKE ''%'' + @texto + ''%'' OR
			v.diaFestivo LIKE ''%'' + @texto + ''%'' OR
			DATENAME(WEEKDAY, v.fechaVenta) LIKE ''%'' + @texto + ''%''
		)';

	IF (@soloAnulados = 1)
		SET @sql += ' AND l.cantKg < 0';

	-- Group by y orden
	SET @sql += '
	GROUP BY v.idVenta, v.fechaVenta, v.idVendedor, u.nombre, v.nroRemito, 
			 v.idPersona, p.razonSocial, v.idSucursal, s.sucursal, 
			 v.tipoComprobante, v.enCtaCte, v.formaPago, v.pagoMixtoEfectivo, v.comisionTarjeta,
			 v.turno, v.diaFestivo, v.observaciones, v.creado, v.actualizado, 
			 v.estado, v.acumRedondeoKgs, v.acumRedondeoImporte
	ORDER BY v.fechaVenta DESC;';

	-- Ejecutar
	EXEC sp_executesql @sql, @params, 
		@fechaDesde=@fechaDesde, 
		@fechaHasta=@fechaHasta,
		@texto=@texto, 
		@idSucursal=@idSucursal,
		@idVendedor=@idVendedor,
		@idCliente=@idCliente,
		@soloAnulados=@soloAnulados;
END

GO
-- ===== porcentajeCortesPorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[porcentajeCortesPorCompra] 
	-- Add the parameters for the stored procedure here
	@idCompra int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     PorcentajesPorCorte.corte as 'Corte', PorcentajesPorCorte.sucursal as 'Sucursal', 
			PorcentajesPorCorte.StockTeorico as 'Cantidad Kgs',PorcentajesPorCorte.StockMin as 'Stock Min', PorcentajesPorCorte.StockMax as 'Stock Max'
	FROM
		((SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100) AS StockTeorico, 
                      SUM(dbo.MediaRes.kgMedia * (CorteP.porcentaje - CorteP.desvioEstandar) / 100) AS StockMin, 
                      SUM(dbo.MediaRes.kgMedia * (CorteP.porcentaje + CorteP.desvioEstandar) / 100) AS StockMax
		FROM         dbo.Corte AS CorteMediaRes INNER JOIN
							  dbo.Corte AS CorteP ON CorteMediaRes.idCorte = CorteP.idCorteMaestro AND CorteMediaRes.idCorte <> CorteP.idCorte CROSS JOIN
							  dbo.MediaRes INNER JOIN
							  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0) AND (dbo.MediaRes.idCompra = @idCompra)
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal)
		UNION
		(SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) 
                      AS StockTeorico, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje - CorteP.desvioEstandar) / 100) AS StockMin, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje + CorteP.desvioEstandar) / 100) AS StockMax
		FROM         dbo.Corte AS CorteM INNER JOIN
							  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro INNER JOIN
							  dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte CROSS JOIN
							  dbo.MediaRes INNER JOIN
							  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0) AND (CorteP.independiente = 1) AND (dbo.MediaRes.idCompra = @idCompra)
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
		) as PorcentajesPorCorte
	
END

GO
-- ===== quitarCortesPorMovimiento =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarCortesPorMovimiento] 
	-- Add the parameters for the stored procedure here
	@idMovimiento int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		--Se reestablece el stock de las sucursales
	
	 
	 --*******Sucursal Origen
	--Actualizo el Stock de los cortes
	update StockCorteSucursal set stock= dbo.StockCorteSucursal.stock +
                          (SELECT     SUM(CortePorMovimiento_1.cantKg) AS Expr1
                            FROM          dbo.Movimiento AS Movimiento_1 INNER JOIN
                                                   dbo.CortePorMovimiento AS CortePorMovimiento_1 ON Movimiento_1.idMovimiento = CortePorMovimiento_1.idMovimientos
                            WHERE      (Movimiento_1.idMovimiento = dbo.Movimiento.idMovimiento) AND (CortePorMovimiento_1.idCorte = dbo.CortePorMovimiento.idCorte)
                            GROUP BY CortePorMovimiento_1.idCorte) 
	FROM         dbo.CortePorMovimiento INNER JOIN
			  dbo.StockCorteSucursal ON dbo.CortePorMovimiento.idCorte = dbo.StockCorteSucursal.idCorte INNER JOIN
			  dbo.Movimiento ON dbo.StockCorteSucursal.idSucursal = dbo.Movimiento.sucursalOrigen AND 
			  dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento
	WHERE     (dbo.Movimiento.idMovimiento = @idMovimiento)
	

		
		
	-- Se actualizan todos los sub-cortes del corte ingresado
	update StockCorteSucursal 
		set stock=(stock + (SELECT     SUM(CortePorMovimiento_1.cantKg * (CorteP_1.porcentaje / 100)) AS Expr1
							FROM         dbo.CortePorMovimiento AS CortePorMovimiento_1 INNER JOIN
									  dbo.Corte AS CorteM_1 ON CortePorMovimiento_1.idCorte = CorteM_1.idCorte INNER JOIN
									  dbo.Corte AS CorteP_1 ON CorteM_1.idCorte = CorteP_1.idCorteMaestro AND CorteM_1.idCorte <> CorteP_1.idCorte
							WHERE CorteP_1.idCorte=CorteP.idCorte and CortePorMovimiento_1.idMovimientos=dbo.Movimiento.idMovimiento
							GROUP BY CorteP_1.idCorte )) 
	FROM         dbo.StockCorteSucursal INNER JOIN
		  dbo.Corte AS CorteP ON dbo.StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
		  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> CorteM.idCorte INNER JOIN
		  dbo.Movimiento ON dbo.StockCorteSucursal.idSucursal = dbo.Movimiento.sucursalOrigen INNER JOIN
		  dbo.CortePorMovimiento ON CorteM.idCorte = dbo.CortePorMovimiento.idCorte AND dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos
	WHERE     (dbo.Movimiento.idMovimiento = @idMovimiento)
	
				
				
	-- Se actualizan todos los subcortes del subcorte del corte ingresado
	update StockCorteSucursal 
		set stock=(stock + (SELECT     SUM((CortePorMovimiento_1.cantKg * (CorteM_1.porcentaje / 100)) * (CorteP_1.porcentaje / 100)) AS Expr1
							FROM         dbo.Corte AS CorteP_1 INNER JOIN
									  dbo.Corte AS CorteMedia_1 INNER JOIN
									  dbo.CortePorMovimiento AS CortePorMovimiento_1 ON CorteMedia_1.idCorte = CortePorMovimiento_1.idCorte INNER JOIN
									  dbo.Corte AS CorteM_1 ON CorteMedia_1.idCorte = CorteM_1.idCorteMaestro ON CorteP_1.idCorteMaestro = CorteM_1.idCorte AND 
									  CorteP_1.idCorte <> CorteM_1.idCorte
							WHERE     (CortePorMovimiento_1.idMovimientos = dbo.Movimiento.idMovimiento) and CorteP_1.idCorte=CorteP.idCorte
							GROUP BY CorteP_1.idCorte)) 
	FROM         dbo.Movimiento INNER JOIN
		  dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
		  dbo.StockCorteSucursal INNER JOIN
		  dbo.Corte AS CorteP ON dbo.StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
		  dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte AND CorteP.idCorte <> Cor
GO
-- ===== quitarStockCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarStockCorte] 
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@idCorte int,
	@idSucursal int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		 
	delete from CortePorCompra where CortePorCompra.idCompra=@idCompra and
	CortePorCompra.idCorte=@idCorte and CortePorCompra.idSucursal=@idSucursal	
	
END

GO
-- ===== quitarStockMedia =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarStockMedia] 
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@idMedia int,
	@idSucursal int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--STOCK REAL
 		-- Actualiza los cortes primarios
	update StockCorteSucursal set stock=(stock-(MediaRes.kgMedia*(CorteP.porcentaje)/100))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
                      dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
                      dbo.MediaRes ON StockCorteSucursal.idSucursal = dbo.MediaRes.idSucursal INNER JOIN
                      dbo.Corte AS CorteMediaRes ON CorteP.idCorteMaestro = CorteMediaRes.idCorte AND CorteP.idCorte <> CorteMediaRes.idCorte
	WHERE     (dbo.MediaRes.idMedia = @idMedia) AND (CorteMediaRes.codigo = 0)
		
	-- Se actualizan todos los sub-cortes
	update StockCorteSucursal 
		set stock=(stock- (((MediaRes.kgMedia*(CorteM.porcentaje)/100)) * (CorteP.porcentaje / 100) ))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
                      dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
                      dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
                      dbo.MediaRes ON StockCorteSucursal.idSucursal = dbo.MediaRes.idSucursal INNER JOIN
                      dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte
	WHERE     (dbo.MediaRes.idMedia = @idMedia) AND (CorteMediaRes.codigo = 0)
	
		
	-- Se actualizan todos los cortes nivel 3
	update StockCorteSucursal 
		set stock=(stock- (((MediaRes.kgMedia * ( CorteMedia.porcentaje/100 ))*(CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
                      dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
                      dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
                      dbo.MediaRes ON StockCorteSucursal.idSucursal = dbo.MediaRes.idSucursal INNER JOIN
                      dbo.Corte AS CorteMedia ON CorteM.idCorteMaestro = CorteMedia.idCorte INNER JOIN
                      dbo.Corte AS CorteMediaRes ON CorteMedia.idCorteMaestro = CorteMediaRes.idCorte AND CorteMedia.idCorte <> CorteMediaRes.idCorte
	WHERE     (dbo.MediaRes.idMedia = @idMedia) AND (CorteMediaRes.codigo = 0)
		
	

			
END

GO
-- ===== quitarStockTeoricoMedia =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[quitarStockTeoricoMedia] 
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@idMedia int,
	@idSucursal int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		---STOCK TEORICO
			-- Actualiza los cortes primarios
	update StockCorteSucursal set stockTeorico=(stockTeorico-(MediaRes.kgMedia*(CorteP.porcentaje)/100))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
                      dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
                      dbo.MediaRes ON StockCorteSucursal.idSucursal = dbo.MediaRes.idSucursal INNER JOIN
                      dbo.Corte AS CorteMediaRes ON CorteP.idCorteMaestro = CorteMediaRes.idCorte AND CorteP.idCorte <> CorteMediaRes.idCorte
	WHERE     (dbo.MediaRes.idMedia = @idMedia) AND (CorteMediaRes.codigo = 0)
		
	-- Se actualizan todos los sub-cortes
	update StockCorteSucursal 
		set stockTeorico=(stockTeorico- (((MediaRes.kgMedia*(CorteM.porcentaje)/100)) * (CorteP.porcentaje / 100) ))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
                      dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
                      dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
                      dbo.MediaRes ON StockCorteSucursal.idSucursal = dbo.MediaRes.idSucursal INNER JOIN
                      dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte
	WHERE     (dbo.MediaRes.idMedia = @idMedia) AND (CorteMediaRes.codigo = 0)
	
		
	-- Se actualizan todos los cortes nivel 3
	update StockCorteSucursal 
		set stockTeorico=(stockTeorico- (((MediaRes.kgMedia * ( CorteMedia.porcentaje/100 ))*(CorteM.porcentaje)/100) * (CorteP.porcentaje / 100)))
	FROM         dbo.StockCorteSucursal AS StockCorteSucursal INNER JOIN
                      dbo.Corte AS CorteP ON StockCorteSucursal.idCorte = CorteP.idCorte INNER JOIN
                      dbo.Corte AS CorteM ON CorteP.idCorteMaestro = CorteM.idCorte INNER JOIN
                      dbo.MediaRes ON StockCorteSucursal.idSucursal = dbo.MediaRes.idSucursal INNER JOIN
                      dbo.Corte AS CorteMedia ON CorteM.idCorteMaestro = CorteMedia.idCorte INNER JOIN
                      dbo.Corte AS CorteMediaRes ON CorteMedia.idCorteMaestro = CorteMediaRes.idCorte AND CorteMedia.idCorte <> CorteMediaRes.idCorte
	WHERE     (dbo.MediaRes.idMedia = @idMedia) AND (CorteMediaRes.codigo = 0)
	
	
	delete from MediaRes where idMedia=@idMedia
			
END

GO
-- ===== ReiniciarCuarto =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[ReiniciarCuarto] 
	-- Add the parameters for the stored procedure here
	@p1 int = 0, 
	@p2 int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update StockCorteSucursal set stock=0
	FROM         dbo.Corte INNER JOIN
                      dbo.StockCorteSucursal ON dbo.Corte.idCorte = dbo.StockCorteSucursal.idCorte
	WHERE     (dbo.Corte.codigo = 49)
	
	update StockCorteSucursal set stock=0
	FROM         dbo.StockCorteSucursal INNER JOIN
                      dbo.Corte AS Corte_1 ON dbo.StockCorteSucursal.idCorte = Corte_1.idCorte INNER JOIN
                      dbo.Corte ON Corte_1.idCorteMaestro = dbo.Corte.idCorte
	WHERE     (dbo.Corte.codigo = 49)

	update StockCorteSucursal set stock=0
FROM         dbo.Corte INNER JOIN
                      dbo.Corte AS Corte_1 ON dbo.Corte.idCorte = Corte_1.idCorteMaestro INNER JOIN
                      dbo.Corte AS Corte_2 ON Corte_1.idCorte = Corte_2.idCorteMaestro INNER JOIN
                      dbo.StockCorteSucursal ON Corte_2.idCorte = dbo.StockCorteSucursal.idCorte
WHERE     (dbo.Corte.codigo = 49)
	
		update StockCorteSucursal set stock=0
FROM         dbo.StockCorteSucursal INNER JOIN
                      dbo.Corte AS Corte_3 ON dbo.StockCorteSucursal.idCorte = Corte_3.idCorte INNER JOIN
                      dbo.Corte INNER JOIN
                      dbo.Corte AS Corte_1 ON dbo.Corte.idCorte = Corte_1.idCorteMaestro INNER JOIN
                      dbo.Corte AS Corte_2 ON Corte_1.idCorte = Corte_2.idCorteMaestro ON Corte_3.idCorteMaestro = Corte_2.idCorte
WHERE     (dbo.Corte.codigo = 49)
	
END

GO
-- ===== reiniciarStock =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[reiniciarStock] 
	-- Add the parameters for the stored procedure here
	@idSucursal int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if(@idSucursal=0)
    begin
		update StockCorteSucursal set stock=0 
	end
	
	else
    begin
		update StockCorteSucursal set stock=0 where idSucursal=@idSucursal
	end
	
	
END

GO
-- ===== reiniciarStockTeorico =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[reiniciarStockTeorico] 
	-- Add the parameters for the stored procedure here
		@idSucursal int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    if(@idSucursal=0)
    begin
		update StockCorteSucursal set stockTeorico=0 
	end
	
	else
    begin
		update StockCorteSucursal set stockTeorico=0 where idSucursal=@idSucursal
	end
	
END

GO
-- ===== ResumenVentasMesPorCliente =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ResumenVentasMesPorCliente]
	-- Add the parameters for the stored procedure here
	@idCli_Avda19_Reco20 int = null,
	@año int = null,
	@mes int = null
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Ventas.idPersona, dbo.Personas.razonSocial, dbo.Ventas.fechaVenta, dbo.Ventas.nroRemito, dbo.Corte.corte, ROUND(dbo.LineaVenta.cantKg, 3) as cantKgs,  ROUND(dbo.LineaVenta.precioKg, 2) as precioKg, 
						   ROUND(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg, 2) AS total
	FROM         dbo.Corte INNER JOIN
						  dbo.LineaVenta ON dbo.Corte.idCorte = dbo.LineaVenta.idCorte INNER JOIN
						  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
						  dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona
	WHERE   YEAR(dbo.Ventas.fechaVenta) = @año AND MONTH(dbo.Ventas.fechaVenta) = @mes AND 
		dbo.Ventas.idPersona = @idCli_Avda19_Reco20	
	ORDER BY dbo.Ventas.fechaVenta asc
	-- ID Coc. Avda = 19 / ID Coc. Reco = 20
	
	SELECT     dbo.Ventas.idPersona, dbo.Personas.razonSocial,ROUND(SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg),2) AS total
	FROM         dbo.Corte INNER JOIN
						  dbo.LineaVenta ON dbo.Corte.idCorte = dbo.LineaVenta.idCorte INNER JOIN
						  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
						  dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona
	WHERE   YEAR(dbo.Ventas.fechaVenta) = @año AND MONTH(dbo.Ventas.fechaVenta) = @mes AND 
		dbo.Ventas.idPersona = @idCli_Avda19_Reco20	
	GROUP BY dbo.Ventas.idPersona, dbo.Personas.razonSocial

END

GO
-- ===== sp_EmpresaParametros_SetDefaults =====

CREATE   PROCEDURE dbo.sp_EmpresaParametros_SetDefaults
    @idEmpresa INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Validaciones básicas
    IF @idEmpresa IS NULL OR @idEmpresa = 0
        THROW 50000, 'idEmpresa inválido.', 1;

    -- Si tenés FK a Empresas, conviene validar que exista
    IF NOT EXISTS (SELECT 1 FROM dbo.Empresas WHERE idEmpresa = @idEmpresa)
        THROW 50001, 'La empresa indicada no existe en dbo.Empresas.', 1;

    -- Copia parámetros desde la empresa default (-1) hacia la empresa nueva (@idEmpresa)
    INSERT INTO dbo.EmpresaParametros (idEmpresa, idParametro, valor)
    SELECT
        @idEmpresa,
        ep.idParametro,
        ep.valor
    FROM dbo.EmpresaParametros ep
    WHERE ep.idEmpresa = -1
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.EmpresaParametros ep2
          WHERE ep2.idEmpresa = @idEmpresa
            AND ep2.idParametro = ep.idParametro
      );
END

GO
-- ===== StockCierre =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockCierre] 
	-- Add the parameters for the stored procedure here
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select cast(Ingreso.codigo as NCHAR(5)) as Codigo ,Ingreso.corte as 'Corte',Ingreso.sucursal as 'Sucursal',
			Ingreso.StockIngreso as 'Total Ingresado', Embutido.TotalEnEmbutidos as 'Kgs en Embutidos', 
			Egreso.TotalVenta as 'Total Vendido',(Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) as 'Stock Teorico',
			 CierreStock.StockCierre as 'Stock Real', 
			 ((Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) - CierreStock.StockCierre ) as 'Faltante'
from

(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
from
(
--Seleccion de todos los cortes
(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
FROM         dbo.Corte AS CorteP CROSS JOIN
                      dbo.Sucursal
WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

union
---Ingreso Movimiento

--++SubCorte de Media
((SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorMovimiento.cantKg) AS StockIngreso
FROM         dbo.Corte INNER JOIN
                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
                      dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal
WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal) 

union

--++SubCorte 2 de Media
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
		SUM(dbo.CortePorMovimiento.cantKg+dbo.CortePorMovimiento.cantKg*SubCorte.porcentajeHueso/SubCorte.porcentaje) AS StockIngreso
FROM         dbo.Movimiento INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
                      dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Corte AS SubCorte ON dbo.CortePorMovimiento.idCorte = SubCorte.idCorte INNER JOIN
                      dbo.Corte INNER JOIN
                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte ON SubCorte.idCorteMaestro = dbo.Corte.idCorte
WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (CorteM.codigo < 1)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
union
--++SubCorte 3 de Media
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
				SUM(((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHu
GO
-- ===== StockCierre_2 =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockCierre_2] 
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select cast(Ingreso.codigo as NCHAR(5)) as Codigo ,Ingreso.corte as 'Corte',Ingreso.sucursal as 'Sucursal',
			Ingreso.StockIngreso as 'Total Ingresado', Embutido.TotalEnEmbutidos as 'Kgs en Embutidos', 
			Egreso.TotalVenta as 'Total Vendido',(Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) as 'Stock Teorico',
			 CierreStock.StockCierre as 'Stock Real', 
			 ((Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) - CierreStock.StockCierre ) as 'Faltante'
from

(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
from
(
--Seleccion de todos los cortes
(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
FROM         dbo.Corte AS CorteP CROSS JOIN
                      dbo.Sucursal
WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

union
---Ingreso Movimiento

--++SubCorte de Media
((SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorMovimiento.cantKg) AS StockIngreso
FROM         dbo.Corte INNER JOIN
                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
                      dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal
WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal) 

union

--++SubCorte 2 de Media
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
		SUM(dbo.CortePorMovimiento.cantKg+dbo.CortePorMovimiento.cantKg*SubCorte.porcentajeHueso/SubCorte.porcentaje) AS StockIngreso
FROM         dbo.Movimiento INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
                      dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Corte AS SubCorte ON dbo.CortePorMovimiento.idCorte = SubCorte.idCorte INNER JOIN
                      dbo.Corte INNER JOIN
                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte ON SubCorte.idCorteMaestro = dbo.Corte.idCorte
WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (CorteM.codigo < 1)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
union
--++SubCorte 3 de Media
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
				SUM(((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje)
					 +((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte
GO
-- ===== StockIngresoEgreso =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockIngresoEgreso] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select cast(Ingreso.codigo as NCHAR) as Codigo ,Ingreso.corte as 'Corte',Ingreso.sucursal as 'Sucursal',Ingreso.StockIngreso as 'Total Ingresado', Embutido.TotalEnEmbutidos as 'Kgs en Embutidos', Egreso.TotalVenta as 'Total Vendido',(Ingreso.StockIngreso-Embutido.TotalEnEmbutidos-Egreso.TotalVenta) as 'Diferencia Stock'
from

(select idCorte,codigo,corte,idSucursal,sucursal,SUM(StockIngreso) as StockIngreso
from
(
--Seleccion de todos los cortes
(SELECT     CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 0 AS StockIngreso 
FROM         dbo.Corte AS CorteP CROSS JOIN
                      dbo.Sucursal
WHERE     (CorteP.independiente = 1) AND (CorteP.codigo > 0) AND (dbo.Sucursal.idSucursal = @idSucursal)
GROUP BY CorteP.idCorte, CorteP.codigo,CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)

union
---Ingreso Movimiento

--++SubCorte de Media
((SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorMovimiento.cantKg) AS StockIngreso
FROM         dbo.Corte INNER JOIN
                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
                      dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal
WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal) 

union

--++SubCorte 2 de Media
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
		SUM(dbo.CortePorMovimiento.cantKg+dbo.CortePorMovimiento.cantKg*SubCorte.porcentajeHueso/SubCorte.porcentaje) AS StockIngreso
FROM         dbo.Movimiento INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Movimiento.idMovimiento = dbo.CortePorMovimiento.idMovimientos INNER JOIN
                      dbo.Sucursal ON dbo.Movimiento.sucursalDestino = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Corte AS SubCorte ON dbo.CortePorMovimiento.idCorte = SubCorte.idCorte INNER JOIN
                      dbo.Corte INNER JOIN
                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte ON SubCorte.idCorteMaestro = dbo.Corte.idCorte
WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND (CorteM.codigo < 1)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
union
--++SubCorte 3 de Media
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, 
				SUM(((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje)
					 +((dbo.CortePorMovimiento.cantKg+(dbo.CortePorMovimiento.cantKg*SubCorte2.porcentajeHueso/ SubCorte2.porcentaje))
					 *SubCorte.porcentajeHueso/SubCorte.porcentaje)))) AS StockIngreso
GO
-- ===== StockTeoricoReal =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[StockTeoricoReal]
	-- Add the parameters for the stored procedure here
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
SELECT     StockTeorico.corte as 'Corte', StockTeorico.sucursal as 'Sucursal', 
		StockTeorico.StockTeorico as 'Stock Teórico', StockReal.StockReal as 'Stock Real', (StockTeorico.StockTeorico - StockReal.StockReal) as 'Diferencia'
FROM
		((SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100) AS StockTeorico
		FROM         dbo.Corte AS CorteMediaRes INNER JOIN
					  dbo.Corte AS CorteP ON CorteMediaRes.idCorte = CorteP.idCorteMaestro AND CorteMediaRes.idCorte <> CorteP.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo < 1) AND (dbo.Compras.estado = 'Stock Borrado') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal) and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal)
		UNION
		(SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) 
					  AS StockTeorico
		FROM         dbo.Corte AS CorteM INNER JOIN
					  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro INNER JOIN
					  dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo < 1) AND (CorteP.independiente = 1) AND (dbo.Compras.estado = 'Stock Borrado') AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal)  and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
		) as StockTeorico  Left OUTER JOIN ((SELECT     CorteP.idCorte, CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg) AS StockReal
		FROM         dbo.Compras INNER JOIN
					  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
					  dbo.Corte AS CorteP ON dbo.CortePorCompra.idCorte = CorteP.idCorte
		WHERE     (CorteP.independiente = 1) AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND (dbo.CortePorCompra.idSucursal = @idSucursal)
					 and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.idCorte, CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal)
		UNION
		(SELECT     CorteP.idCorte, CorteP.corte, dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.CortePorCompra.cantKg * CorteP.porcentaje / 100) AS StockReal
		FROM         dbo.Compras INNER JOIN
				  dbo.CortePorCompra ON dbo.Compras.idCompra = dbo.CortePorCompra.idCompra INNER JOIN
				  dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
				  dbo.Corte AS CorteM ON dbo.CortePorCompra.idCorte = CorteM.idCorte INNER JOIN
				  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro AN
GO
-- ===== TicketAnualdo =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TicketAnualdo] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Ventas.fechaVenta as Fecha, dbo.Corte.corte AS Corte, dbo.Sucursal.sucursal AS Sucursal, dbo.LineaVenta.cantKg AS 'Total Kgs', (dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) as 'Importe'

	FROM         dbo.Corte INNER JOIN
				  dbo.LineaVenta ON dbo.Corte.idCorte = dbo.LineaVenta.idCorte INNER JOIN
				  dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
				  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal
	WHERE     dbo.LineaVenta.idAnulado=1 and (dbo.Ventas.fechaVenta  BETWEEN @fechaDesde AND 
                      @fechaHasta+1)and (dbo.Ventas.idSucursal=@idSucursal) and dbo.Corte.corte like '%'+@texto+'%'
	order by dbo.Corte.corte, dbo.Ventas.fechaVenta
END

GO
-- ===== TotalKgsCortePorCompra =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalKgsCortePorCompra] 
	-- Add the parameters for the stored procedure here

    -- Insert statements for procedure here
	@idSucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50)	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     StockTeorico.corte as 'Corte', StockTeorico.sucursal as 'Sucursal', 
			StockTeorico.StockTeorico as 'Cantidad Kgs',StockTeorico.StockMin as 'Stock Min', StockTeorico.StockMax as 'Stock Max'
	FROM
		((SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100) AS StockTeorico, 
                      SUM((dbo.MediaRes.kgMedia * (CorteP.porcentaje / 100)) - CorteP.desvioEstandar)  AS StockMin, 
                      SUM((dbo.MediaRes.kgMedia * (CorteP.porcentaje / 100)) + CorteP.desvioEstandar) AS StockMax
		FROM         dbo.Corte AS CorteMediaRes INNER JOIN
					  dbo.Corte AS CorteP ON CorteMediaRes.idCorte = CorteP.idCorteMaestro AND CorteMediaRes.idCorte <> CorteP.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0)  AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal) and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal)
		UNION
		(SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) 
					  AS StockTeorico, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje - CorteP.desvioEstandar) / 100) AS StockMin, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje + CorteP.desvioEstandar) / 100) AS StockMax
		FROM         dbo.Corte AS CorteM INNER JOIN
					  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro INNER JOIN
					  dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0) AND (CorteP.independiente = 1)  AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal)  and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
		) as StockTeorico
		
END

GO
-- ===== TotalMovimientosCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalMovimientosCorte] 

	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
SELECT     Movimientos.corte as 'Corte', Movimientos.origen AS 'Sucursal Origen', Movimientos.destino AS 'Sucursal Destino', SUM(Movimientos.TotalEgreso) AS 'Total Kgs'
FROM       
(
--Seleccion de todos los cortes
(SELECT     dbo.Corte.idCorte, dbo.Corte.corte, SucursalOrigen.idSucursal AS idOrigen, SucursalOrigen.sucursal AS origen, SucursalDestino.idSucursal AS idDestino, 
                      SucursalDestino.sucursal AS destino, 0 AS TotalEgreso
FROM         dbo.Sucursal AS SucursalOrigen INNER JOIN
                      dbo.Sucursal AS SucursalDestino ON SucursalOrigen.idSucursal <> SucursalDestino.idSucursal CROSS JOIN
                      dbo.Corte
WHERE     (dbo.Corte.corte LIKE '%' + @texto + '%') AND (dbo.Corte.codigo > 0) AND (SucursalDestino.idSucursal = @idSucursal) AND (dbo.Corte.independiente = 1))

union
--Movimientos Cortes independientes
(SELECT     dbo.Corte.idCorte, dbo.Corte.corte, SucursalOrigen.idSucursal AS idOrigen, SucursalOrigen.sucursal AS origen, SucursalDestino.idSucursal AS idDestino, 
                      SucursalDestino.sucursal AS destino, SUM(dbo.CortePorMovimiento.cantKg) AS TotalEgreso
FROM         dbo.Corte INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
                      dbo.Sucursal AS SucursalOrigen ON dbo.Movimiento.sucursalOrigen = SucursalOrigen.idSucursal INNER JOIN
                      dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal
WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND 
                      (dbo.Corte.corte LIKE '%' + @texto + '%') AND (dbo.Corte.codigo > 0) OR
                      (CAST(dbo.Movimiento.idMovimiento AS nvarchar(8)) LIKE @texto)
GROUP BY dbo.Corte.idCorte, dbo.Corte.corte, SucursalOrigen.idSucursal, SucursalOrigen.sucursal, SucursalDestino.idSucursal, SucursalDestino.sucursal
)
--Suma de sub-cortes
--(SELECT     dbo.Corte.idCorte, dbo.Corte.corte, SucursalOrigen.idSucursal as idOrigen, SucursalOrigen.sucursal as origen, SucursalDestino.idSucursal AS idDestino, SucursalDestino.sucursal AS destino, 
--                      SUM(dbo.CortePorMovimiento.cantKg) AS TotalEgreso
--FROM         dbo.Corte INNER JOIN
--                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
--                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
--                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
--                      dbo.Sucursal AS SucursalOrigen ON dbo.Movimiento.sucursalOrigen = SucursalOrigen.idSucursal INNER JOIN
--                      dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal
--WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta+1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND 
--                    ((dbo.Corte.corte LIKE '%' + @texto + '%') or (cast( dbo.Movimiento.idMovimiento as nvarchar(8)) like @texto ))
--GROUP BY dbo.Corte.idCorte, dbo.Corte.corte, SucursalOrigen.idSucursal, SucursalOrigen.sucursal, SucursalDestino.idSucursal, 
GO
-- ===== TotalMovimientosPorCorte =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalMovimientosPorCorte] 
	-- Add the parameters for the stored procedure here
	@texto nvarchar(50),
	@idsucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
SELECT   cast(Movimientos.codigo  as NCHAR(5)) as Codigo, Movimientos.corte as 'Corte', SUM(Movimientos.TotalUnidad) as 'Total Unidades', SUM(Movimientos.TotalEgreso) AS 'Total Kgs', Movimientos.origen AS 'Sucursal Origen', Movimientos.destino AS 'Sucursal Destino'
FROM       
(
--Seleccion de todos los cortes
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, SucursalOrigen.idSucursal AS idOrigen, SucursalOrigen.sucursal AS origen, SucursalDestino.idSucursal AS idDestino, 
                      SucursalDestino.sucursal AS destino, 0 AS TotalUnidad, 0 AS TotalEgreso
FROM         dbo.Sucursal AS SucursalOrigen INNER JOIN
                      dbo.Sucursal AS SucursalDestino ON SucursalOrigen.idSucursal <> SucursalDestino.idSucursal CROSS JOIN
                      dbo.Corte
WHERE     (dbo.Corte.corte LIKE '%' + @texto + '%' or dbo.Corte.codigo LIKE '%' + @texto + '%') AND (dbo.Corte.codigo > 0) AND (SucursalDestino.idSucursal = @idSucursal) AND (dbo.Corte.independiente = 1))

union
--Movimientos Cortes independientes
(SELECT     dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, SucursalOrigen.idSucursal AS idOrigen, SucursalOrigen.sucursal AS origen, SucursalDestino.idSucursal AS idDestino, 
                      SucursalDestino.sucursal AS destino, SUM(dbo.CortePorMovimiento.cantUnidad) AS TotalUnidad, SUM(dbo.CortePorMovimiento.cantKg) AS TotalEgreso
FROM         dbo.Corte INNER JOIN
                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
                      dbo.Sucursal AS SucursalOrigen ON dbo.Movimiento.sucursalOrigen = SucursalOrigen.idSucursal INNER JOIN
                      dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal
WHERE     (dbo.Movimiento.fechaMovimiento BETWEEN @fechaDesde AND @fechaHasta + 1) AND (dbo.Movimiento.sucursalDestino = @idSucursal) AND 
                      (dbo.Corte.corte LIKE '%' + @texto + '%' or dbo.Corte.codigo LIKE '%' + @texto + '%') AND (dbo.Corte.codigo > 0) OR
                      (CAST(dbo.Movimiento.idMovimiento AS nvarchar(8)) LIKE @texto)
GROUP BY dbo.Corte.idCorte, dbo.Corte.codigo,dbo.Corte.corte, SucursalOrigen.idSucursal, SucursalOrigen.sucursal, SucursalDestino.idSucursal, SucursalDestino.sucursal
)
--Suma de sub-cortes
--(SELECT     dbo.Corte.idCorte, dbo.Corte.corte, SucursalOrigen.idSucursal as idOrigen, SucursalOrigen.sucursal as origen, SucursalDestino.idSucursal AS idDestino, SucursalDestino.sucursal AS destino, 
--                      SUM(dbo.CortePorMovimiento.cantKg) AS TotalEgreso
--FROM         dbo.Corte INNER JOIN
--                      dbo.Corte AS CorteM ON dbo.Corte.idCorteMaestro = CorteM.idCorte AND dbo.Corte.idCorte <> CorteM.idCorte INNER JOIN
--                      dbo.CortePorMovimiento ON dbo.Corte.idCorte = dbo.CortePorMovimiento.idCorte INNER JOIN
--                      dbo.Movimiento ON dbo.CortePorMovimiento.idMovimientos = dbo.Movimiento.idMovimiento INNER JOIN
--                      dbo.Sucursal AS SucursalOrigen ON dbo.Movimiento.sucursalOrigen = SucursalOrigen.idSucursal INNER JOIN
--                      dbo.Sucursal AS SucursalDestino ON dbo.Movimiento.sucursalDestino = SucursalDestino.idSucursal
--WHERE     (CorteM.codigo < 1) AND (dbo.Movimiento.fechaMov
GO
-- ===== TotalPorCortesVendidos =====

CREATE   PROCEDURE [dbo].[TotalPorCortesVendidos]
    @texto nvarchar(50),
    @idEmpresa int = NULL,
    @idSucursal int,
    @fechaDesde datetime,
    @fechaHasta datetime,
    @tipo nvarchar(50),
    @idProveedor int,
    @idMarca int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @esNumero int = ISNUMERIC(@texto);

    SELECT
        CONVERT(VARCHAR, c.codigo) AS Codigo,
        c.corte AS Corte,
        CASE
            WHEN ISNULL(@idSucursal, 0) = 0 THEN 'Todas'
            ELSE MAX(s.sucursal)
        END AS Sucursal,
        SUM(lv.cantKg) AS [Total Kgs],
        SUM(lv.cantKg * lv.precioKg) AS [Total $]
    FROM dbo.Corte c
    INNER JOIN dbo.LineaVenta lv ON c.idCorte = lv.idCorte
    INNER JOIN dbo.Ventas v ON lv.idVenta = v.idVenta
    INNER JOIN dbo.Sucursal s ON v.idSucursal = s.idSucursal
    LEFT OUTER JOIN dbo.CorteProveedor cp ON c.idCorte = cp.idCorte
    WHERE
        v.fechaVenta BETWEEN @fechaDesde AND @fechaHasta
        AND (ISNULL(@idEmpresa, 0) = 0 OR s.idEmpresa = @idEmpresa)
        AND (ISNULL(@idSucursal, 0) = 0 OR v.idSucursal = @idSucursal)
        AND (@tipo = '' OR @tipo IS NULL OR c.tipo = @tipo)
        AND (
            (@esNumero = 0 AND c.corte LIKE '%' + @texto + '%')
            OR (@esNumero = 1 AND CAST(c.codigo AS NCHAR) = @texto)
        )
        AND (@idProveedor = 0 OR @idProveedor IS NULL OR cp.idProveedor = @idProveedor)
        AND (@idMarca = 0 OR @idMarca IS NULL OR c.idMarca = @idMarca)
    GROUP BY
        c.codigo,
        c.corte
    ORDER BY
        c.corte;
END

GO
-- ===== TotalSegunCompras =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalSegunCompras] 

	-- Add the parameters for the stored procedure here
	@idSucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50)	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select CorteTabla.idCorte,CorteTabla.corte, CorteTabla.precioKg, StockCompras.StockTeorico, CorteTabla.precioKg*StockCompras.StockTeorico as Total
    from
    
	(SELECT    StockTeorico.idCorte, StockTeorico.corte, StockTeorico.sucursal , 
			StockTeorico.StockTeorico 
	FROM
		((SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100) AS StockTeorico, 
                      SUM((dbo.MediaRes.kgMedia * (CorteP.porcentaje / 100)) - CorteP.desvioEstandar)  AS StockMin, 
                      SUM((dbo.MediaRes.kgMedia * (CorteP.porcentaje / 100)) + CorteP.desvioEstandar) AS StockMax
		FROM         dbo.Corte AS CorteMediaRes INNER JOIN
					  dbo.Corte AS CorteP ON CorteMediaRes.idCorte = CorteP.idCorteMaestro AND CorteMediaRes.idCorte <> CorteP.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0)  AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal) and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal)
		UNION
		(SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) 
					  AS StockTeorico, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje - CorteP.desvioEstandar) / 100) AS StockMin, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje + CorteP.desvioEstandar) / 100) AS StockMax
		FROM         dbo.Corte AS CorteM INNER JOIN
					  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro INNER JOIN
					  dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0) AND (CorteP.independiente = 1)  AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal)  and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
		) as StockTeorico) as StockCompras INNER JOIN
                      dbo.Corte as CorteTabla ON CorteTabla.idCorte = StockCompras.idCorte
      
      where CorteTabla.idCorte <> 75 and CorteTabla.idCorte <> 53 and CorteTabla.idCorte <> 57 and CorteTabla.idCorte <> 63 and CorteTabla.idCorte <> 66 and CorteTabla.idCorte <> 72 and 
			CorteTabla.idCorte <> 81  and CorteTabla.idCorte <> 84 and CorteTabla.idCorte <> 121  and CorteTabla.idCorte <> 147      
		
END

GO
-- ===== TotalSegunComprasMonto =====
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[TotalSegunComprasMonto] 
	-- Add the parameters for the stored procedure here

	@idSucursal int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50)	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select SUM(StockCompras.StockTeorico) as TotalKg,Sum(CorteTabla.precioKg*StockCompras.StockTeorico) as Total
    from
    
	(SELECT    StockTeorico.idCorte, StockTeorico.corte, StockTeorico.sucursal , 
			StockTeorico.StockTeorico 
	FROM
		((SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100) AS StockTeorico, 
                      SUM((dbo.MediaRes.kgMedia * (CorteP.porcentaje / 100)) - CorteP.desvioEstandar)  AS StockMin, 
                      SUM((dbo.MediaRes.kgMedia * (CorteP.porcentaje / 100)) + CorteP.desvioEstandar) AS StockMax
		FROM         dbo.Corte AS CorteMediaRes INNER JOIN
					  dbo.Corte AS CorteP ON CorteMediaRes.idCorte = CorteP.idCorteMaestro AND CorteMediaRes.idCorte <> CorteP.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0)  AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal) and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.sucursal, dbo.Sucursal.idSucursal)
		UNION
		(SELECT     CorteP.idCorte, CorteP.corte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal, SUM(dbo.MediaRes.kgMedia * CorteP.porcentaje / 100 * CorteM.porcentaje / 100) 
					  AS StockTeorico, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje - CorteP.desvioEstandar) / 100) AS StockMin, 
                      SUM(dbo.MediaRes.kgMedia * CorteM.porcentaje / 100 * (CorteP.porcentaje + CorteP.desvioEstandar) / 100) AS StockMax
		FROM         dbo.Corte AS CorteM INNER JOIN
					  dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro INNER JOIN
					  dbo.Corte AS CorteMediaRes ON CorteM.idCorteMaestro = CorteMediaRes.idCorte AND CorteM.idCorte <> CorteMediaRes.idCorte CROSS JOIN
					  dbo.Compras INNER JOIN
					  dbo.MediaRes ON dbo.Compras.idCompra = dbo.MediaRes.idCompra INNER JOIN
					  dbo.Sucursal ON dbo.MediaRes.idSucursal = dbo.Sucursal.idSucursal
		WHERE     (CorteMediaRes.codigo = 0) AND (CorteP.independiente = 1)  AND (dbo.Compras.fechaCompra BETWEEN @fechaDesde AND @fechaHasta+1) AND 
					  (dbo.MediaRes.idSucursal = @idSucursal)  and (dbo.Compras.nroRemito like '%'+@texto+'%')
		GROUP BY CorteP.corte, CorteP.idCorte, dbo.Sucursal.idSucursal, dbo.Sucursal.sucursal)
		) as StockTeorico) as StockCompras INNER JOIN
                      dbo.Corte as CorteTabla ON CorteTabla.idCorte = StockCompras.idCorte
      
      where CorteTabla.idCorte <> 75 and CorteTabla.idCorte <> 53 and CorteTabla.idCorte <> 57 and CorteTabla.idCorte <> 63 and CorteTabla.idCorte <> 66 and CorteTabla.idCorte <> 72 and 
			CorteTabla.idCorte <> 81  and CorteTabla.idCorte <> 84 and CorteTabla.idCorte <> 121  and CorteTabla.idCorte <> 147      
		
	 
END

GO
-- ===== ultimasVentasCliente =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ultimasVentasCliente]
	-- Add the parameters for the stored procedure here	
	@idSucursal int = -1,
	@idPersona int = -1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    	SELECT     dbo.Usuarios.nombre as 'vendedor', dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Personas.razonSocial, dbo.Corte.codigo, dbo.Corte.corte, dbo.LineaVenta.cantKg, dbo.LineaVenta.precioKg, 
						  dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg AS totalCorte, dbo.LineaVenta.bonificacion, dbo.LineaVenta.pesoBalanza, dbo.LineaVenta.idAnulado, dbo.Sucursal.sucursal
	FROM         dbo.Ventas INNER JOIN
                      dbo.LineaVenta ON dbo.Ventas.idVenta = dbo.LineaVenta.idVenta INNER JOIN
                      dbo.Corte ON dbo.LineaVenta.idCorte = dbo.Corte.idCorte INNER JOIN
                      dbo.Usuarios ON dbo.Ventas.idVendedor = dbo.Usuarios.id INNER JOIN
                      dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
                      dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona
	WHERE dbo.Ventas.idPersona = @idPersona and dbo.Ventas.idSucursal = @idSucursal
			and dbo.Ventas.idVenta IN (
				SELECT  TOP 5  Vta.idVenta
						FROM         dbo.Ventas as Vta INNER JOIN dbo.Sucursal as Suc ON 
									Vta.idSucursal = Suc.idSucursal INNER JOIN
									 dbo.Personas as Pers ON Vta.idPersona = Pers.idPersona
						WHERE     (Vta.idPersona = @idPersona) AND (Vta.idSucursal = @idSucursal)
						ORDER BY Vta.fechaVenta desc)
	order by dbo.Ventas.fechaVenta desc
END

GO
-- ===== ventasVendedorCierreCaja =====
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ventasVendedorCierreCaja]
	-- Add the parameters for the stored procedure here
	@idVendedor int,
	@fechaDesde datetime,
	@fechaHasta datetime,
	@texto nvarchar(50) = '',
	@idSucursal int,
	@soloAnulados tinyint = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Ventas.idVendedor, dbo.Usuarios.nombre, dbo.Ventas.nroRemito, dbo.Ventas.idPersona, dbo.Personas.razonSocial, dbo.Ventas.idSucursal, dbo.Sucursal.sucursal, 
                      dbo.Ventas.formaPago, dbo.Ventas.tipoComprobante, dbo.Ventas.observaciones, dbo.Ventas.creado, dbo.Ventas.actualizado,
                      dbo.Ventas.estado,SUM(dbo.LineaVenta.cantKg) as totalKg, SUM(dbo.LineaVenta.cantKg * dbo.LineaVenta.precioKg) AS totalS
	FROM         dbo.LineaVenta INNER JOIN
                      dbo.Ventas ON dbo.LineaVenta.idVenta = dbo.Ventas.idVenta INNER JOIN
			  dbo.Sucursal ON dbo.Ventas.idSucursal = dbo.Sucursal.idSucursal INNER JOIN
			  dbo.Personas ON dbo.Ventas.idPersona = dbo.Personas.idPersona INNER JOIN
			  dbo.Usuarios ON dbo.Ventas.idVendedor = dbo.Usuarios.id
	WHERE fechaVenta between @fechaDesde and @fechaHasta and dbo.Ventas.idSucursal = @idSucursal and dbo.Ventas.idVendedor = @idVendedor
	and(nroRemito like '%'+@texto+'%' or  Personas.razonSocial like '%'+@texto+'%') and
	(@soloAnulados=0 or (@soloAnulados=1 and dbo.LineaVenta.cantKg < 0 ))
	--case(@soloAnulados? true, false) --and dbo.LineaVenta.cantKg < 0 )

	GROUP BY dbo.Ventas.idVenta, dbo.Ventas.fechaVenta, dbo.Ventas.idVendedor, dbo.Usuarios.nombre, dbo.Ventas.nroRemito, dbo.Ventas.idPersona, dbo.Personas.razonSocial, dbo.Ventas.idSucursal, dbo.Sucursal.sucursal, 
				  dbo.Ventas.formaPago, dbo.Ventas.tipoComprobante, dbo.Ventas.observaciones, dbo.Ventas.creado, dbo.Ventas.actualizado, dbo.Ventas.estado
	order by fechaVenta desc
	
END

GO
