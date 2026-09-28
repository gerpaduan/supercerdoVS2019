USE [CarniSys]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Cierre de Stock: persiste el flag "no contado" (docs/09-cambios-y-pendientes/bitacora-de-cambios.md,
-- 2026-09-28). El badge
-- "No contado" ya se calculaba en el cliente (WebCore/wwwroot/Scripts/app/stock.js) para las
-- lineas agregadas en bloque desde el modal "Productos no cargados", pero nunca viajaba al
-- servidor ni se guardaba. Columna aditiva con default 0: todo el historico queda como
-- "contado" (no se sabe si esas lineas viejas fueron o no completadas por el modal).
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CortePorCompra') AND name = 'noContado'
)
BEGIN
    ALTER TABLE dbo.CortePorCompra ADD noContado bit NOT NULL DEFAULT 0;
END
GO

-- Agrega @noContado (parametro nuevo, default 0 para no romper callers existentes) al INSERT.
-- Resto del cuerpo identico al original (extraido con sp_helptext antes de este cambio, ver
-- docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql).
ALTER PROCEDURE [dbo].[agregarCortePorCompra]
	-- Add the parameters for the stored procedure here
	@idCompra int,
	@idCorte int,
	@idSucursal int,
	@precioKg float,
	@cantKg float,
	@balanza tinyint,
	@creado datetime = SYSDATETIME,
	@creadoPor int = 0,
	@noContado bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into CortePorCompra(idCompra,idCorte,idSucursal,precioKg,cantKg, balanza, creado, creadoPor, noContado)
	values (@idCompra,@idCorte,@idSucursal,@precioKg,@cantKg, @balanza, @creado, @creadoPor, @noContado)


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

-- Agrega CortePorCompra.noContado al SELECT. Resto del cuerpo identico al original (ver
-- docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql).
ALTER PROCEDURE [dbo].[obtenerCortesPorCompra]
	-- Add the parameters for the stored procedure here
	@idCompra int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT     dbo.CortePorCompra.idCortePorCompra, dbo.CortePorCompra.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorCompra.cantKg, dbo.CortePorCompra.precioKg,
                      dbo.CortePorCompra.cantKg * dbo.CortePorCompra.precioKg AS totalS, dbo.CortePorCompra.balanza,  dbo.CortePorCompra.idSucursal, dbo.Sucursal.sucursal, dbo.CortePorCompra.creado, dbo.CortePorCompra.creadoPor, dbo.CortePorCompra.noContado
	FROM         dbo.CortePorCompra INNER JOIN
                      dbo.Corte ON dbo.CortePorCompra.idCorte = dbo.Corte.idCorte INNER JOIN
                      dbo.Sucursal ON dbo.CortePorCompra.idSucursal = dbo.Sucursal.idSucursal
	 where CortePorCompra.idCompra=@idCompra
	 order by dbo.Corte.codigo
END
GO
