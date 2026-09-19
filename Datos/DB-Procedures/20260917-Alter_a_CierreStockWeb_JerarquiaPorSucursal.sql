USE [supercerdo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- Mismo cambio que 20260917-Alter_a_ExistenciaStockPorSucursales_JerarquiaPorSucursal.sql,
-- ya probado y verificado ahi contra datos reales (ver docs/DECISIONS.md, 2026-09-17) --
-- replicado tal cual a este segundo SP. No repito el detalle completo del porque acá,
-- ver el otro archivo para el razonamiento completo (incluido el bug real encontrado y
-- corregido: la exclusion de Descendientes/Ascendientes tiene que basarse en
-- "tieneExcepcionIndependiente", NUNCA en "independiente" a secas -- un corte puede ser
-- independiente globalmente Y AL MISMO TIEMPO depender de otro corte independiente mas
-- arriba en la cadena, ej. "Carre" depende de "Media Res" -- ambos independientes).
--
-- Diferencias mecanicas de este SP respecto al otro (no relacionadas a la feature nueva):
-- no tiene FechaUltimoCierre (usa @fechaDesde/@fechaHasta fijos), y tiene un INSERT mas
-- en #Operaciones (StockCierre) que tambien gana "AND mc.idSucursal = X.idSucursal".
-- ============================================================================

ALTER PROCEDURE [dbo].[a_CierreStockWeb]
    @texto nvarchar(50) = '',
    @idEmpresa int = NULL,
    @idSucursal int = 0,
    @fechaDesde datetime,
    @fechaHasta datetime,
    @tipo nvarchar(50) = '',
    @idProveedor int = 0,
    @idMarca int = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @textoLimpio nvarchar(50) = LTRIM(RTRIM(ISNULL(@texto, '')));

    CREATE TABLE #Sucursales
    (
        idSucursal int NOT NULL PRIMARY KEY,
        sucursal nvarchar(200) NOT NULL
    );

    INSERT INTO #Sucursales (idSucursal, sucursal)
    SELECT s.idSucursal, s.sucursal
    FROM dbo.Sucursal s
    WHERE
        (@idSucursal IS NULL OR @idSucursal = 0 OR s.idSucursal = @idSucursal)
        AND (@idEmpresa IS NULL OR s.idEmpresa = @idEmpresa);

    -- Ver 20260917-Alter_a_ExistenciaStockPorSucursales_JerarquiaPorSucursal.sql para el
    -- razonamiento completo de esta tabla puente y de tieneExcepcionIndependiente.
    CREATE TABLE #CorteEfectivoPorSucursal
    (
        idCorte int NOT NULL,
        idSucursal int NOT NULL,
        independiente bit NOT NULL,
        enCierreStock bit NOT NULL,
        tieneExcepcionIndependiente bit NOT NULL,
        PRIMARY KEY (idCorte, idSucursal)
    );

    INSERT INTO #CorteEfectivoPorSucursal (idCorte, idSucursal, independiente, enCierreStock, tieneExcepcionIndependiente)
    SELECT
        c.idCorte,
        s.idSucursal,
        COALESCE(ov.independiente, ISNULL(c.independiente, 0)),
        COALESCE(ov.enCierreStock, ISNULL(c.enCierreStock, 0)),
        CASE WHEN ov.idCorteJerarquiaSucursal IS NOT NULL AND ov.independiente = 1 THEN 1 ELSE 0 END
    FROM dbo.Corte c
    CROSS JOIN #Sucursales s
    LEFT JOIN dbo.CorteJerarquiaSucursal ov
        ON ov.idEmpresa = c.idEmpresa
       AND ov.idCorte = c.idCorte
       AND ov.idSucursal = s.idSucursal
    WHERE (@idEmpresa IS NULL OR c.idEmpresa = @idEmpresa);

    CREATE TABLE #MapaCorte
    (
        idSucursal int NOT NULL,
        IdCorteOrigen int NOT NULL,
        IdCorteStock int NOT NULL,
        Factor decimal(38, 10) NOT NULL,
        PRIMARY KEY (idSucursal, IdCorteOrigen, IdCorteStock)
    );

    ;WITH SelfMap AS
    (
        SELECT
            ce.idSucursal,
            c.idCorte AS IdCorteOrigen,
            c.idCorte AS IdCorteStock,
            CAST(1 AS decimal(38, 10)) AS Factor
        FROM dbo.Corte c
        INNER JOIN #CorteEfectivoPorSucursal ce
            ON ce.idCorte = c.idCorte
           AND ce.independiente = 1
        WHERE (@idEmpresa IS NULL OR c.idEmpresa = @idEmpresa)
    ),
    Descendientes AS
    (
        SELECT
            ceHijo.idSucursal,
            padre.idCorte AS IdCorteOrigen,
            hijo.idCorte AS IdCorteStock,
            CAST(ISNULL(hijo.porcentaje, 0) / 100.0 AS decimal(38, 10)) AS Factor,
            1 AS Nivel
        FROM dbo.Corte padre
        INNER JOIN dbo.Corte hijo
            ON hijo.idCorteMaestro = padre.idCorte
           AND hijo.idCorte <> padre.idCorte
        INNER JOIN #CorteEfectivoPorSucursal ceHijo
            ON ceHijo.idCorte = hijo.idCorte
           AND ceHijo.tieneExcepcionIndependiente = 0
        WHERE (@idEmpresa IS NULL OR padre.idEmpresa = @idEmpresa)

        UNION ALL

        SELECT
            d.idSucursal,
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
        INNER JOIN #CorteEfectivoPorSucursal ceHijo
            ON ceHijo.idCorte = hijo.idCorte
           AND ceHijo.idSucursal = d.idSucursal
           AND ceHijo.tieneExcepcionIndependiente = 0
        WHERE d.Nivel < 10
    ),
    Ascendientes AS
    (
        SELECT
            ceHijo.idSucursal,
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
        INNER JOIN #CorteEfectivoPorSucursal ceHijo
            ON ceHijo.idCorte = hijo.idCorte
           AND ceHijo.tieneExcepcionIndependiente = 0
        WHERE (@idEmpresa IS NULL OR hijo.idEmpresa = @idEmpresa)

        UNION ALL

        SELECT
            a.idSucursal,
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
        INNER JOIN #CorteEfectivoPorSucursal ceActual
            ON ceActual.idCorte = actual.idCorte
           AND ceActual.idSucursal = a.idSucursal
           AND ceActual.tieneExcepcionIndependiente = 0
        WHERE a.Nivel < 10
    ),
    Mapa AS
    (
        SELECT idSucursal, IdCorteOrigen, IdCorteStock, Factor
        FROM SelfMap

        UNION ALL

        SELECT
            d.idSucursal,
            d.IdCorteOrigen,
            d.IdCorteStock,
            d.Factor
        FROM Descendientes d
        INNER JOIN #CorteEfectivoPorSucursal ceStock
            ON ceStock.idCorte = d.IdCorteStock
           AND ceStock.idSucursal = d.idSucursal
           AND ceStock.independiente = 1

        UNION ALL

        SELECT
            a.idSucursal,
            a.IdCorteOrigen,
            a.IdCorteStock,
            a.Factor
        FROM Ascendientes a
        INNER JOIN #CorteEfectivoPorSucursal ceStock
            ON ceStock.idCorte = a.IdCorteStock
           AND ceStock.idSucursal = a.idSucursal
           AND ceStock.independiente = 1
    )
    INSERT INTO #MapaCorte (idSucursal, IdCorteOrigen, IdCorteStock, Factor)
    SELECT idSucursal, IdCorteOrigen, IdCorteStock, SUM(Factor) AS Factor
    FROM Mapa
    WHERE Factor <> 0
    GROUP BY idSucursal, IdCorteOrigen, IdCorteStock
    OPTION (MAXRECURSION 20);

    CREATE TABLE #AllCortes
    (
        idCorte int NOT NULL,
        Codigo nvarchar(50) NULL,
        Corte nvarchar(200) NULL,
        idSucursal int NOT NULL,
        Sucursal nvarchar(200) NOT NULL,
        promedio decimal(18, 3) NULL,
        PuntoStock decimal(18, 3) NULL,
        pesable bit NULL
    );

    INSERT INTO #AllCortes (idCorte, Codigo, Corte, idSucursal, Sucursal, promedio, PuntoStock, pesable)
    SELECT DISTINCT
        c.idCorte,
        CAST(c.codigo AS nvarchar(50)) AS Codigo,
        c.corte AS Corte,
        s.idSucursal,
        s.sucursal AS Sucursal,
        CAST(ISNULL(c.promedio, 0) AS decimal(18, 3)) AS promedio,
        CAST(ISNULL(c.puntoStock, 0) AS decimal(18, 3)) AS PuntoStock,
        CAST(ISNULL(c.pesable, 0) AS bit) AS pesable
    FROM dbo.Corte c
    CROSS JOIN #Sucursales s
    INNER JOIN #CorteEfectivoPorSucursal ce
        ON ce.idCorte = c.idCorte
       AND ce.idSucursal = s.idSucursal
    LEFT JOIN dbo.CorteProveedor cp
        ON cp.idCorte = c.idCorte
    WHERE
        ce.independiente = 1
        AND ce.enCierreStock = 1
        AND (@idEmpresa IS NULL OR c.idEmpresa = @idEmpresa)
        AND (@tipo IS NULL OR @tipo = '' OR c.tipo = @tipo)
        AND (@idProveedor IS NULL OR @idProveedor = 0 OR cp.idProveedor = @idProveedor)
        AND (@idMarca IS NULL OR @idMarca = 0 OR c.idMarca = @idMarca);

    CREATE TABLE #Operaciones
    (
        TipoOperacion nvarchar(40) NOT NULL,
        idSucursal int NOT NULL,
        idCorte int NOT NULL,
        Kg decimal(38, 6) NOT NULL
    );

    CREATE INDEX IX_Operaciones_Sucursal_Corte
        ON #Operaciones (idSucursal, idCorte);

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'StockInicial',
        cpc.idSucursal,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Compras c
    INNER JOIN dbo.CortePorCompra cpc
        ON cpc.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = cpc.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpc.idCorte
       AND mc.idSucursal = cpc.idSucursal
    WHERE
        c.tipoCompra = 'Cierre Stock'
        AND ISNULL(c.estado, '') = ''
        AND c.fechaCompra LIKE @fechaDesde
    GROUP BY cpc.idSucursal, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'StockCierre',
        cpc.idSucursal,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Compras c
    INNER JOIN dbo.CortePorCompra cpc
        ON cpc.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = cpc.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpc.idCorte
       AND mc.idSucursal = cpc.idSucursal
    WHERE
        c.tipoCompra = 'Cierre Stock'
        AND ISNULL(c.estado, '') = ''
        AND c.fechaCompra LIKE @fechaHasta
    GROUP BY cpc.idSucursal, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'Compras',
        mr.idSucursal,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(mr.kgMedia, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Compras c
    INNER JOIN dbo.MediaRes mr
        ON mr.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = mr.idSucursal
    INNER JOIN dbo.Corte corteMedia
        ON corteMedia.codigo = 0
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = corteMedia.idCorte
       AND mc.idSucursal = mr.idSucursal
    WHERE
        ISNULL(c.estado, '') = ''
        AND c.fechaCompra >= @fechaDesde
        AND c.fechaCompra <= @fechaHasta
    GROUP BY mr.idSucursal, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        CASE c.tipoCompra
            WHEN 'Cortes' THEN 'Compras'
            WHEN 'Ingreso Stock' THEN 'IngresoStock'
            WHEN 'Ajuste Stock' THEN 'AjusteStock'
            WHEN 'Egreso Stock' THEN 'EgresoStock'
        END,
        cpc.idSucursal,
        mc.IdCorteStock,
        SUM(
            CAST(ISNULL(cpc.cantKg, 0) AS decimal(38, 6))
            * mc.Factor
            * CASE WHEN c.tipoCompra = 'Egreso Stock' THEN -1 ELSE 1 END
        )
    FROM dbo.Compras c
    INNER JOIN dbo.CortePorCompra cpc
        ON cpc.idCompra = c.idCompra
    INNER JOIN #Sucursales s
        ON s.idSucursal = cpc.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpc.idCorte
       AND mc.idSucursal = cpc.idSucursal
    WHERE
        c.tipoCompra IN ('Cortes', 'Ingreso Stock', 'Ajuste Stock', 'Egreso Stock')
        AND ISNULL(c.estado, '') = ''
        AND c.fechaCompra >= @fechaDesde
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

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'Ventas',
        v.idSucursal,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(lv.cantKg, 0) - ISNULL(lv.kgsAjusteTarj, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Ventas v
    INNER JOIN dbo.LineaVenta lv
        ON lv.idVenta = v.idVenta
    INNER JOIN #Sucursales s
        ON s.idSucursal = v.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = lv.idCorte
       AND mc.idSucursal = v.idSucursal
    WHERE
        v.fechaVenta >= @fechaDesde
        AND v.fechaVenta <= @fechaHasta
    GROUP BY v.idSucursal, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'IngresoMovimiento',
        m.sucursalDestino,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(cpm.cantKg, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Movimiento m
    INNER JOIN dbo.CortePorMovimiento cpm
        ON cpm.idMovimientos = m.idMovimiento
    INNER JOIN #Sucursales s
        ON s.idSucursal = m.sucursalDestino
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpm.idCorte
       AND mc.idSucursal = m.sucursalDestino
    WHERE
        m.fechaMovimiento >= @fechaDesde
        AND m.fechaMovimiento <= @fechaHasta
    GROUP BY m.sucursalDestino, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'EgresoMovimiento',
        m.sucursalOrigen,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(cpm.cantKg, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Movimiento m
    INNER JOIN dbo.CortePorMovimiento cpm
        ON cpm.idMovimientos = m.idMovimiento
    INNER JOIN #Sucursales s
        ON s.idSucursal = m.sucursalOrigen
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpm.idCorte
       AND mc.idSucursal = m.sucursalOrigen
    WHERE
        m.fechaMovimiento >= @fechaDesde
        AND m.fechaMovimiento <= @fechaHasta
    GROUP BY m.sucursalOrigen, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'IngresoElaborado',
        e.idSucursal,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(cpe.kgUtilizados, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Embutidos e
    INNER JOIN dbo.CortePorEmbutido cpe
        ON cpe.idEmbutido = e.idEmbutido
    INNER JOIN #Sucursales s
        ON s.idSucursal = e.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = e.idCorte
       AND mc.idSucursal = e.idSucursal
    WHERE
        ISNULL(e.estado, '') = ''
        AND e.fechaEmbutido >= @fechaDesde
        AND e.fechaEmbutido <= @fechaHasta
    GROUP BY e.idSucursal, mc.IdCorteStock;

    INSERT INTO #Operaciones (TipoOperacion, idSucursal, idCorte, Kg)
    SELECT
        'EgresoElaborado',
        e.idSucursal,
        mc.IdCorteStock,
        SUM(CAST(ISNULL(cpe.kgUtilizados, 0) AS decimal(38, 6)) * mc.Factor)
    FROM dbo.Embutidos e
    INNER JOIN dbo.CortePorEmbutido cpe
        ON cpe.idEmbutido = e.idEmbutido
    INNER JOIN #Sucursales s
        ON s.idSucursal = e.idSucursal
    INNER JOIN #MapaCorte mc
        ON mc.IdCorteOrigen = cpe.idCorte
       AND mc.idSucursal = e.idSucursal
    WHERE
        ISNULL(e.estado, '') = ''
        AND e.fechaEmbutido >= @fechaDesde
        AND e.fechaEmbutido <= @fechaHasta
    GROUP BY e.idSucursal, mc.IdCorteStock;

    ;WITH Resumen AS
    (
        SELECT
            o.idSucursal,
            o.idCorte,
            SUM(CASE WHEN o.TipoOperacion = 'StockInicial' THEN o.Kg ELSE 0 END) AS StockInicial,
            SUM(CASE WHEN o.TipoOperacion = 'StockCierre' THEN o.Kg ELSE 0 END) AS StockCierre,
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
        GROUP BY o.idSucursal, o.idCorte
    ),
    Final AS
    (
        SELECT
            ac.idCorte,
            ac.Codigo,
            ac.Corte,
            ac.idSucursal,
            ac.Sucursal,
            CAST(ISNULL(r.StockInicial, 0) AS decimal(18, 3)) AS StockIni,
            CAST(ISNULL(r.StockCierre, 0) AS decimal(18, 3)) AS StockCierre,
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
            ac.PuntoStock,
            ac.pesable
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
        f.StockIni AS [Stock.Ini],
        f.Compras,
        f.IngresoElaborado AS [Ingr.Elab],
        f.IngresoStock AS [Ingr.Stock],
        f.IngresoMovimiento AS [Ingr. Mov],
        f.AjusteStock AS [Ajus.Stock],
        CAST(f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock AS decimal(18, 3)) AS [Tot.INGR],
        f.EgresoStock AS [Egr.Stock],
        f.EgresoMovimiento AS [Egr.Mov],
        f.EgresoElaborado AS [Egr.Elab],
        f.Ventas,
        CAST(f.EgresoStock + f.EgresoMovimiento + f.EgresoElaborado + f.Ventas AS decimal(18, 3)) AS [Tot.EGR],
        CAST(
            f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
            - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
            AS decimal(18, 3)
        ) AS DIF,
        f.StockCierre AS [Stock.Cierre],
        CAST(
            (
                f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
                - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
            ) - f.StockCierre
            AS decimal(18, 3)
        ) AS Faltante,
        f.promedio,
        CASE
            WHEN f.promedio = 0 THEN
                CAST(
                    (
                        f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
                        - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
                    ) - f.StockCierre
                    AS decimal(18, 2)
                )
            ELSE
                ROUND(
                    (
                        (
                            f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
                            - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
                        ) - f.StockCierre
                    ) / f.promedio,
                    0
                )
        END AS [Stock.Un],
        CASE
            WHEN f.PuntoStock > 0
             AND (
                (
                    (
                        f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
                        - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
                    ) < 0
                )
                OR f.PuntoStock > (
                    f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
                    - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
                )
             )
            THEN 'X'
            ELSE ''
        END AS Falta,
        f.PuntoStock AS [Pto.Stock],
        f.pesable AS Pesable
    FROM Final f
    WHERE
        (
            @textoLimpio = ''
            AND (
                f.StockIni <> 0 OR f.Compras <> 0 OR f.IngresoElaborado <> 0 OR f.IngresoStock <> 0
                OR f.IngresoMovimiento <> 0 OR f.AjusteStock <> 0 OR f.EgresoStock <> 0
                OR f.EgresoMovimiento <> 0 OR f.EgresoElaborado <> 0 OR f.Ventas <> 0 OR f.StockCierre <> 0
            )
        )
        OR
        (
            @textoLimpio <> ''
            AND (
                f.Corte LIKE '%' + @textoLimpio + '%'
                OR CAST(f.Codigo AS nvarchar(50)) LIKE '%' + @textoLimpio + '%'
            )
        )
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
