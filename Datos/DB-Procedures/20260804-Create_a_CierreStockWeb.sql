USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
--      varias sucursales en una sola llamada.
--   4. Tot.INGR/Tot.EGR/DIF/Faltante/Stock.Un/Falta se calculan aca en SQL
--      (formulas identicas a las que hoy hace en C# el post-procesamiento de
--      Negocio.Corte.CierreStock, ver docs/DECISIONS.md) en vez de devolverse
--      en 0.00/'-'/'' y que el caller los complete -- este SP es nuevo, no
--      hace falta preservar ese paso intermedio.
-- ============================================================================
CREATE PROCEDURE [dbo].[a_CierreStockWeb]
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
          AND (@idEmpresa IS NULL OR c.idEmpresa = @idEmpresa)
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
        WHERE (@idEmpresa IS NULL OR padre.idEmpresa = @idEmpresa)

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
        WHERE (@idEmpresa IS NULL OR hijo.idEmpresa = @idEmpresa)

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

        SELECT d.IdCorteOrigen, d.IdCorteStock, d.Factor
        FROM Descendientes d
        INNER JOIN dbo.Corte cStock
            ON cStock.idCorte = d.IdCorteStock
           AND cStock.independiente = 1

        UNION ALL

        SELECT a.IdCorteOrigen, a.IdCorteStock, a.Factor
        FROM Ascendientes a
        INNER JOIN dbo.Corte cStock
            ON cStock.idCorte = a.IdCorteStock
           AND cStock.independiente = 1
    )
    INSERT INTO #MapaCorte (IdCorteOrigen, IdCorteStock, Factor)
    SELECT IdCorteOrigen, IdCorteStock, SUM(Factor) AS Factor
    FROM Mapa
    WHERE Factor <> 0
    GROUP BY IdCorteOrigen, IdCorteStock
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
    LEFT JOIN dbo.CorteProveedor cp
        ON cp.idCorte = c.idCorte
    WHERE
        c.independiente = 1
        -- enCierreStock aplicado siempre, con o sin @texto (a_CierreStock lo
        -- salteaba por completo en la busqueda por texto -- fix intencional).
        AND ISNULL(c.enCierreStock, 0) = 1
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

    -- Stock Inicial: ultimo "Cierre Stock" cargado para esa sucursal, ANTES de ahora.
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
    WHERE
        c.tipoCompra = 'Cierre Stock'
        AND ISNULL(c.estado, '') = ''
        AND c.fechaCompra = (
            SELECT MAX(c2.fechaCompra)
            FROM dbo.Compras c2
            INNER JOIN dbo.CortePorCompra cpc2 ON cpc2.idCompra = c2.idCompra
            WHERE c2.tipoCompra = 'Cierre Stock' AND ISNULL(c2.estado, '') = ''
              AND cpc2.idSucursal = cpc.idSucursal
        )
    GROUP BY cpc.idSucursal, mc.IdCorteStock;

    -- Stock Cierre: "Cierre Stock" cargado EXACTO en @fechaHasta (mismo patron
    -- que a_CierreStock, que usaba "fechaCompra LIKE @fechaHasta" -- se
    -- preserva ese comportamiento tal cual para no romper la paridad).
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
    WHERE
        c.tipoCompra = 'Cierre Stock'
        AND ISNULL(c.estado, '') = ''
        AND c.fechaCompra LIKE @fechaHasta
    GROUP BY cpc.idSucursal, mc.IdCorteStock;

    -- Compras derivadas de Media Res (mismo truco que a_ExistenciaStockPorSucursales:
    -- corteMedia.codigo = 0 es el "corte" especial que representa la media res entera).
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
    WHERE
        ISNULL(c.estado, '') = ''
        AND c.fechaCompra >= @fechaDesde
        AND c.fechaCompra <= @fechaHasta
    GROUP BY mr.idSucursal, mc.IdCorteStock;

    -- Compras / Ingreso Stock / Ajuste Stock / Egreso Stock: mismo origen
    -- (Compras/CortePorCompra), separados por tipoCompra.
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
        SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(38, 6)) * mc.Factor)
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

    -- Ventas
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
    WHERE
        v.fechaVenta >= @fechaDesde
        AND v.fechaVenta <= @fechaHasta
    GROUP BY v.idSucursal, mc.IdCorteStock;

    -- Movimientos entre sucursales: ingreso en destino, egreso en origen.
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
    WHERE
        m.fechaMovimiento >= @fechaDesde
        AND m.fechaMovimiento <= @fechaHasta
    GROUP BY m.sucursalOrigen, mc.IdCorteStock;

    -- Elaborados (embutidos): consumo de cortes ingresados / egresados.
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
        -- Tot.INGR: identico a StockActual de a_ExistenciaStockPorSucursales menos
        -- StockIni (ese se suma aparte en DIF, mismo criterio que el post-procesamiento
        -- de Negocio.Corte.CierreStock).
        CAST(f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock AS decimal(18, 3)) AS [Tot.INGR],
        f.EgresoStock AS [Egr.Stock],
        f.EgresoMovimiento AS [Egr.Mov],
        f.EgresoElaborado AS [Egr.Elab],
        f.Ventas,
        CAST(f.EgresoStock + f.EgresoMovimiento + f.EgresoElaborado + f.Ventas AS decimal(18, 3)) AS [Tot.EGR],
        -- DIF: stock calculado (igual formula que StockActual en a_ExistenciaStockPorSucursales).
        CAST(
            f.StockIni + f.Compras + f.IngresoElaborado + f.IngresoStock + f.IngresoMovimiento + f.AjusteStock
            - f.EgresoStock - f.EgresoMovimiento - f.EgresoElaborado - f.Ventas
            AS decimal(18, 3)
        ) AS DIF,
        f.StockCierre AS [Stock.Cierre],
        -- Faltante: DIF menos el stock cargado en un cierre fisico a @fechaHasta.
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
            -- Caso 1: sin texto -> solo filas con algun movimiento (mismo
            -- criterio que a_CierreStock, no se muestran todos los productos
            -- en cero).
            @textoLimpio = ''
            AND (
                f.StockIni <> 0 OR f.Compras <> 0 OR f.IngresoElaborado <> 0 OR f.IngresoStock <> 0
                OR f.IngresoMovimiento <> 0 OR f.AjusteStock <> 0 OR f.EgresoStock <> 0
                OR f.EgresoMovimiento <> 0 OR f.EgresoElaborado <> 0 OR f.Ventas <> 0 OR f.StockCierre <> 0
            )
        )
        OR
        (
            -- Caso 2: con texto -> buscar por nombre o codigo, sin filtro de movimiento
            -- (ahora enCierreStock ya se aplico arriba en #AllCortes para los dos casos,
            -- a diferencia de a_CierreStock que lo salteaba en este caso).
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
