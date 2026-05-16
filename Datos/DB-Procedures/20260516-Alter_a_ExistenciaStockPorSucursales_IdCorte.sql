USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[a_ExistenciaStockPorSucursales]
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

    INSERT INTO #Sucursales
    (
        idSucursal,
        sucursal,
        FechaUltimoCierre
    )
    SELECT
        s.idSucursal,
        s.sucursal,
        ISNULL(MAX(c.fechaCompra), CONVERT(datetime, '19000101', 112)) AS FechaUltimoCierre
    FROM dbo.Sucursal s
    LEFT JOIN dbo.CortePorCompra cpc
        ON cpc.idSucursal = s.idSucursal
    LEFT JOIN dbo.Compras c
        ON c.idCompra = cpc.idCompra
       AND c.tipoCompra = 'Cierre Stock'
       AND ISNULL(c.estado, '') = ''
    WHERE
        (@idSucursal IS NULL OR @idSucursal = 0 OR s.idSucursal = @idSucursal)
        AND (@idEmpresa IS NULL OR s.idEmpresa = @idEmpresa)
    GROUP BY
        s.idSucursal,
        s.sucursal;

    CREATE TABLE #MapaCorte
    (
        IdCorteOrigen int NOT NULL,
        IdCorteStock int NOT NULL,
        Factor decimal(38, 10) NOT NULL,
        PRIMARY KEY (IdCorteOrigen, IdCorteStock)
    );

    ;WITH SelfMap AS
    (
        SELECT
            c.idCorte AS IdCorteOrigen,
            c.idCorte AS IdCorteStock,
            CAST(1 AS decimal(38, 10)) AS Factor
        FROM dbo.Corte c
        WHERE c.independiente = 1
    ),
    Descendientes AS
    (
        SELECT
            padre.idCorte AS IdCorteOrigen,
            hijo.idCorte AS IdCorteStock,
            CAST(ISNULL(hijo.porcentaje, 0) / 100.0 AS decimal(38, 10)) AS Factor,
            1 AS Nivel
        FROM dbo.Corte padre
        INNER JOIN dbo.Corte hijo
            ON hijo.idCorteMaestro = padre.idCorte
           AND hijo.idCorte <> padre.idCorte

        UNION ALL

        SELECT
            d.IdCorteOrigen,
            hijo.idCorte AS IdCorteStock,
            CAST(d.Factor * (ISNULL(hijo.porcentaje, 0) / 100.0) AS decimal(38, 10)) AS Factor,
            d.Nivel + 1
        FROM Descendientes d
        INNER JOIN dbo.Corte actual
            ON actual.idCorte = d.IdCorteStock
        INNER JOIN dbo.Corte hijo
            ON hijo.idCorteMaestro = actual.idCorte
           AND hijo.idCorte <> actual.idCorte
        WHERE d.Nivel < 10
    ),
    Ascendientes AS
    (
        SELECT
            hijo.idCorte AS IdCorteOrigen,
            padre.idCorte AS IdCorteStock,
            CAST(
                1 + ISNULL(hijo.porcentajeHueso / NULLIF(hijo.porcentaje, 0), 0)
                AS decimal(38, 10)
            ) AS Factor,
            1 AS Nivel
        FROM dbo.Corte hijo
        INNER JOIN dbo.Corte padre
            ON hijo.idCorteMaestro = padre.idCorte
           AND hijo.idCorte <> padre.idCorte

        UNION ALL

        SELECT
            a.IdCorteOrigen,
            padre.idCorte AS IdCorteStock,
            CAST(
                a.Factor *
                (
                    1 + ISNULL(actual.porcentajeHueso / NULLIF(actual.porcentaje, 0), 0)
                )
                AS decimal(38, 10)
            ) AS Factor,
            a.Nivel + 1
        FROM Ascendientes a
        INNER JOIN dbo.Corte actual
            ON actual.idCorte = a.IdCorteStock
        INNER JOIN dbo.Corte padre
            ON actual.idCorteMaestro = padre.idCorte
           AND actual.idCorte <> padre.idCorte
        WHERE a.Nivel < 10
    ),
    Mapa AS
    (
        SELECT IdCorteOrigen, IdCorteStock, Factor
        FROM SelfMap

        UNION ALL

        SELECT
            d.IdCorteOrigen,
            d.IdCorteStock,
            d.Factor
        FROM Descendientes d
        INNER JOIN dbo.Corte cStock
            ON cStock.idCorte = d.IdCorteStock
           AND cStock.independiente = 1

        UNION ALL

        SELECT
            a.IdCorteOrigen,
            a.IdCorteStock,
            a.Factor
        FROM Ascendientes a
        INNER JOIN dbo.Corte cStock
            ON cStock.idCorte = a.IdCorteStock
           AND cStock.independiente = 1
    )
    INSERT INTO #MapaCorte
    (
        IdCorteOrigen,
        IdCorteStock,
        Factor
    )
    SELECT
        IdCorteOrigen,
        IdCorteStock,
        SUM(Factor) AS Factor
    FROM Mapa
    WHERE
        Factor <> 0
        AND (@idCorte IS NULL OR @idCorte = 0 OR IdCorteStock = @idCorte)
    GROUP BY
        IdCorteOrigen,
        IdCorteStock
    OPTION (MAXRECURSION 20);

    CREATE TABLE #AllCortes
    (
        idCorte int NOT NULL,
        Codigo nvarchar(50) NULL,
        Corte nvarchar(200) NULL,
        idSucursal int NOT NULL,
        Sucursal nvarchar(200) NOT NULL,
        FechaUltimoCierre datetime NOT NULL,
        promedio decimal(18, 3) NULL,
        PuntoStock decimal(18, 3) NULL
    );

    INSERT INTO #AllCortes
    (
        idCorte,
        Codigo,
        Corte,
        idSucursal,
        Sucursal,
        FechaUltimoCierre,
        promedio,
        PuntoStock
    )
    SELECT DISTINCT
        c.idCorte,
        CAST(c.codigo AS nvarchar(50)) AS Codigo,
        c.corte AS Corte,
        s.idSucursal,
        s.sucursal AS Sucursal,
        s.FechaUltimoCierre,
        CAST(ISNULL(c.promedio, 0) AS decimal(18, 3)) AS promedio,
        CAST(ISNULL(c.puntoStock, 0) AS decimal(18, 3)) AS PuntoStock
    FROM dbo.Corte c
    CROSS JOIN #Sucursales s
    LEFT JOIN dbo.CorteProveedor cp
        ON cp.idCorte = c.idCorte
    WHERE
        c.independiente = 1
        AND ISNULL(c.enCierreStock, 0) = 1
        AND (@tipo IS NULL OR @tipo = '' OR c.tipo = @tipo)
        AND (@idProveedor IS NULL OR @idProveedor = 0 OR cp.idProveedor = @idProveedor)
        AND (@idMarca IS NULL OR @idMarca = 0 OR c.idMarca = @idMarca)
        AND (@idCorte IS NULL OR @idCorte = 0 OR c.idCorte = @idCorte)
        AND
        (
            @textoLimpio = ''
            OR c.corte LIKE '%' + @textoLimpio + '%'
            OR CAST(c.codigo AS nvarchar(50)) LIKE '%' + @textoLimpio + '%'
        );

    CREATE TABLE #Operaciones
    (
        TipoOperacion nvarchar(40) NOT NULL,
        idSucursal int NOT NULL,
        idCorte int NOT NULL,
        Kg decimal(38, 6) NOT NULL
    );

    CREATE INDEX IX_Operaciones_Sucursal_Corte
        ON #Operaciones (idSucursal, idCorte);

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'StockInicial' AS TipoOperacion,
        cpc.idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Compras c
    INNER JOIN dbo.CortePorCompra cpc
        ON cpc.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = cpc.idSucursal
       AND c.fechaCompra = s.FechaUltimoCierre
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpc.idCorte
    WHERE
        c.tipoCompra = 'Cierre Stock'
        AND ISNULL(c.estado, '') = ''
    GROUP BY
        cpc.idSucursal,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'Compras' AS TipoOperacion,
        mr.idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(mr.kgMedia, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Compras c
    INNER JOIN dbo.MediaRes mr
        ON mr.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = mr.idSucursal
    INNER JOIN dbo.Corte corteMedia
        ON corteMedia.codigo = 0
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = corteMedia.idCorte
    WHERE
        ISNULL(c.estado, '') = ''
        AND c.fechaCompra >= s.FechaUltimoCierre
        AND c.fechaCompra <= @fechaHasta
    GROUP BY
        mr.idSucursal,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        CASE c.tipoCompra
            WHEN 'Cortes' THEN 'Compras'
            WHEN 'Ingreso Stock' THEN 'IngresoStock'
            WHEN 'Ajuste Stock' THEN 'AjusteStock'
            WHEN 'Egreso Stock' THEN 'EgresoStock'
        END AS TipoOperacion,
        cpc.idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Compras c
    INNER JOIN dbo.CortePorCompra cpc
        ON cpc.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = cpc.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpc.idCorte
    WHERE
        c.tipoCompra IN ('Cortes', 'Ingreso Stock', 'Ajuste Stock', 'Egreso Stock')
        AND ISNULL(c.estado, '') = ''
        AND c.fechaCompra >= s.FechaUltimoCierre
        AND c.fechaCompra <= @fechaHasta
    GROUP BY
        CASE c.tipoCompra
            WHEN 'Cortes' THEN 'Compras'
            WHEN 'Ingreso Stock' THEN 'IngresoStock'
            WHEN 'Ajuste Stock' THEN 'AjusteStock'
            WHEN 'Egreso Stock' THEN 'EgresoStock'
        END,
        cpc.idSucursal,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'Ventas' AS TipoOperacion,
        v.idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(
            CAST(
                ISNULL(lv.cantKg, 0) - ISNULL(lv.kgsAjusteTarj, 0)
                AS decimal(38, 6)
            ) * mc.Factor
        ) AS Kg
    FROM dbo.Ventas v
    INNER JOIN dbo.LineaVenta lv
        ON lv.idVenta = v.idVenta
    INNER JOIN #Sucursales s
        ON s.idSucursal = v.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = lv.idCorte
    WHERE
        v.fechaVenta >= s.FechaUltimoCierre
        AND v.fechaVenta <= @fechaHasta
    GROUP BY
        v.idSucursal,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'IngresoMovimiento' AS TipoOperacion,
        m.sucursalDestino AS idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(cpm.cantKg, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Movimiento m
    INNER JOIN dbo.CortePorMovimiento cpm
        ON cpm.idMovimientos = m.idMovimiento
    INNER JOIN #Sucursales s
        ON s.idSucursal = m.sucursalDestino
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpm.idCorte
    WHERE
        m.fechaMovimiento >= s.FechaUltimoCierre
        AND m.fechaMovimiento <= @fechaHasta
    GROUP BY
        m.sucursalDestino,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'EgresoMovimiento' AS TipoOperacion,
        m.sucursalOrigen AS idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(cpm.cantKg, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Movimiento m
    INNER JOIN dbo.CortePorMovimiento cpm
        ON cpm.idMovimientos = m.idMovimiento
    INNER JOIN #Sucursales s
        ON s.idSucursal = m.sucursalOrigen
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpm.idCorte
    WHERE
        m.fechaMovimiento >= s.FechaUltimoCierre
        AND m.fechaMovimiento <= @fechaHasta
    GROUP BY
        m.sucursalOrigen,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'IngresoElaborado' AS TipoOperacion,
        e.idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(cpe.kgUtilizados, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Embutidos e
    INNER JOIN dbo.CortePorEmbutido cpe
        ON cpe.idEmbutido = e.idEmbutido
    INNER JOIN #Sucursales s
        ON s.idSucursal = e.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = e.idCorte
    WHERE
        ISNULL(e.estado, '') = ''
        AND e.fechaEmbutido >= s.FechaUltimoCierre
        AND e.fechaEmbutido <= @fechaHasta
    GROUP BY
        e.idSucursal,
        mc.IdCorteStock;

    INSERT INTO #Operaciones
    (
        TipoOperacion,
        idSucursal,
        idCorte,
        Kg
    )
    SELECT
        'EgresoElaborado' AS TipoOperacion,
        e.idSucursal,
        mc.IdCorteStock AS idCorte,
        SUM(CAST(ISNULL(cpe.kgUtilizados, 0) AS decimal(38, 6)) * mc.Factor) AS Kg
    FROM dbo.Embutidos e
    INNER JOIN dbo.CortePorEmbutido cpe
        ON cpe.idEmbutido = e.idEmbutido
    INNER JOIN #Sucursales s
        ON s.idSucursal = e.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpe.idCorte
    WHERE
        ISNULL(e.estado, '') = ''
        AND e.fechaEmbutido >= s.FechaUltimoCierre
        AND e.fechaEmbutido <= @fechaHasta
    GROUP BY
        e.idSucursal,
        mc.IdCorteStock;

    ;WITH Resumen AS
    (
        SELECT
            o.idSucursal,
            o.idCorte,
            SUM(CASE WHEN o.TipoOperacion = 'StockInicial' THEN o.Kg ELSE 0 END) AS StockInicial,
            SUM(CASE WHEN o.TipoOperacion = 'Compras' THEN o.Kg ELSE 0 END) AS Compras,
            SUM(CASE WHEN o.TipoOperacion = 'IngresoElaborado' THEN o.Kg ELSE 0 END) AS IngresoElaborado,
            SUM(CASE WHEN o.TipoOperacion = 'IngresoStock' THEN o.Kg ELSE 0 END) AS IngresoStock,
            SUM(CASE WHEN o.TipoOperacion = 'IngresoMovimiento' THEN o.Kg ELSE 0 END) AS IngresoMovimiento,
            SUM(CASE WHEN o.TipoOperacion = 'AjusteStock' THEN o.Kg ELSE 0 END) AS AjusteStock,
            SUM(CASE WHEN o.TipoOperacion = 'EgresoStock' THEN o.Kg ELSE 0 END) AS EgresoStock,
            SUM(CASE WHEN o.TipoOperacion = 'EgresoMovimiento' THEN o.Kg ELSE 0 END) AS EgresoMovimiento,
            SUM(CASE WHEN o.TipoOperacion = 'EgresoElaborado' THEN o.Kg ELSE 0 END) AS EgresoElaborado,
            SUM(CASE WHEN o.TipoOperacion = 'Ventas' THEN o.Kg ELSE 0 END) AS Ventas
        FROM #Operaciones o
        GROUP BY
            o.idSucursal,
            o.idCorte
    ),
    Final AS
    (
        SELECT
            ac.idCorte,
            ac.Codigo,
            ac.Corte,
            ac.idSucursal,
            ac.Sucursal,
            ac.FechaUltimoCierre,
            CAST(ISNULL(r.StockInicial, 0) AS decimal(18, 3)) AS StockInicial,
            CAST(ISNULL(r.Compras, 0) AS decimal(18, 3)) AS Compras,
            CAST(ISNULL(r.IngresoElaborado, 0) AS decimal(18, 3)) AS IngresoElaborado,
            CAST(ISNULL(r.IngresoStock, 0) AS decimal(18, 3)) AS IngresoStock,
            CAST(ISNULL(r.IngresoMovimiento, 0) AS decimal(18, 3)) AS IngresoMovimiento,
            CAST(ISNULL(r.AjusteStock, 0) AS decimal(18, 3)) AS AjusteStock,
            CAST(ISNULL(r.EgresoStock, 0) AS decimal(18, 3)) AS EgresoStock,
            CAST(ISNULL(r.EgresoMovimiento, 0) AS decimal(18, 3)) AS EgresoMovimiento,
            CAST(ISNULL(r.EgresoElaborado, 0) AS decimal(18, 3)) AS EgresoElaborado,
            CAST(ISNULL(r.Ventas, 0) AS decimal(18, 3)) AS Ventas,
            ac.promedio,
            ac.PuntoStock
        FROM #AllCortes ac
        LEFT JOIN Resumen r
            ON r.idSucursal = ac.idSucursal
           AND r.idCorte = ac.idCorte
    )
    SELECT
        f.idCorte,
        CAST(f.Codigo AS nchar(20)) AS Codigo,
        f.Corte,
        f.idSucursal,
        f.Sucursal,
        f.FechaUltimoCierre,
        f.StockInicial,
        f.Compras,
        f.IngresoElaborado,
        f.IngresoStock,
        f.IngresoMovimiento,
        f.AjusteStock,
        calc.TotalIngresos,
        f.EgresoStock,
        f.EgresoMovimiento,
        f.EgresoElaborado,
        f.Ventas,
        calc.TotalEgresos,
        calc.StockActual,
        f.promedio,
        f.PuntoStock,
        CASE
            WHEN calc.StockActual < 0 THEN 'NEGATIVO'
            WHEN f.PuntoStock > 0 AND calc.StockActual <= f.PuntoStock THEN 'BAJO'
            WHEN calc.StockActual = 0 THEN 'SIN STOCK'
            ELSE 'OK'
        END AS EstadoStock
    FROM Final f
    CROSS APPLY
    (
        SELECT
            CAST(
                f.Compras
                + f.IngresoElaborado
                + f.IngresoStock
                + f.IngresoMovimiento
                + f.AjusteStock
                AS decimal(18, 3)
            ) AS TotalIngresos,
            CAST(
                f.EgresoStock
                + f.EgresoMovimiento
                + f.EgresoElaborado
                + f.Ventas
                AS decimal(18, 3)
            ) AS TotalEgresos,
            CAST(
                f.StockInicial
                + f.Compras
                + f.IngresoElaborado
                + f.IngresoStock
                + f.IngresoMovimiento
                + f.AjusteStock
                - f.EgresoStock
                - f.EgresoMovimiento
                - f.EgresoElaborado
                - f.Ventas
                AS decimal(18, 3)
            ) AS StockActual
    ) calc
    WHERE
        @soloConStock = 0
        OR ABS(calc.StockActual) > 0.000
    ORDER BY
        CASE
            WHEN LTRIM(RTRIM(ISNULL(f.Codigo, ''))) <> ''
             AND LTRIM(RTRIM(ISNULL(f.Codigo, ''))) NOT LIKE '%[^0-9]%'
            THEN CONVERT(decimal(18, 0), LTRIM(RTRIM(f.Codigo)))
            ELSE 999999999999999999
        END ASC,
        LTRIM(RTRIM(ISNULL(f.Codigo, ''))) ASC,
        f.Sucursal ASC;
END
GO
