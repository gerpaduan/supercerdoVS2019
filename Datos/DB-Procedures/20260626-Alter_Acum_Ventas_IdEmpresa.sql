USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[Acum_Ventas]
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
            AND (@idEmpresaFiltro = 0 OR ISNULL(CorteP.idEmpresa, 0) IN (0, @idEmpresaFiltro))
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
                AND (@idEmpresaFiltro = 0 OR ISNULL(c.idEmpresa, 0) IN (0, @idEmpresaFiltro))
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
                ON CorteP.idCorteMaestro = c.idCorte
            WHERE
                v.fechaVenta BETWEEN @fechaDesde AND @fechaHasta
                AND
                (
                    (@idSucursalFiltro > 0 AND v.idSucursal = @idSucursalFiltro)
                    OR
                    (@idSucursalFiltro = 0 AND @idEmpresaFiltro > 0 AND s.idEmpresa = @idEmpresaFiltro)
                )
                AND (@idEmpresaFiltro = 0 OR s.idEmpresa = @idEmpresaFiltro)
                AND c.codigo > 0
                AND c.independiente = 1
                AND (@idEmpresaFiltro = 0 OR ISNULL(c.idEmpresa, 0) IN (0, @idEmpresaFiltro))
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
                SUM(
                    ((lv.cantKg - lv.kgsAjusteTarj) + (lv.cantKg - lv.kgsAjusteTarj) * Corte_1.porcentajeHueso / Corte_1.porcentaje)
                    + (((lv.cantKg - lv.kgsAjusteTarj) + (lv.cantKg - lv.kgsAjusteTarj) * Corte_1.porcentajeHueso / Corte_1.porcentaje)
                    * CorteP.porcentajeHueso / CorteP.porcentaje)
                ) AS TotalVenta
            FROM dbo.Ventas v
            INNER JOIN dbo.LineaVenta lv
                ON v.idVenta = lv.idVenta
            INNER JOIN dbo.Sucursal s
                ON v.idSucursal = s.idSucursal
            INNER JOIN dbo.Corte AS Corte_1
                ON Corte_1.idCorte = lv.idCorte
            INNER JOIN dbo.Corte AS CorteP
                ON Corte_1.idCorteMaestro = CorteP.idCorte
            INNER JOIN dbo.Corte c
                ON c.idCorte = CorteP.idCorteMaestro
            WHERE
                v.fechaVenta BETWEEN @fechaDesde AND @fechaHasta
                AND
                (
                    (@idSucursalFiltro > 0 AND v.idSucursal = @idSucursalFiltro)
                    OR
                    (@idSucursalFiltro = 0 AND @idEmpresaFiltro > 0 AND s.idEmpresa = @idEmpresaFiltro)
                )
                AND (@idEmpresaFiltro = 0 OR s.idEmpresa = @idEmpresaFiltro)
                AND c.codigo > 0
                AND c.independiente = 1
                AND (@idEmpresaFiltro = 0 OR ISNULL(c.idEmpresa, 0) IN (0, @idEmpresaFiltro))
            GROUP BY
                s.idSucursal,
                s.sucursal,
                c.idCorte,
                c.codigo,
                c.corte
        ) AS EgresoVentas
        GROUP BY
            idCorte,
            codigo,
            corte,
            idSucursal,
            sucursal
    ) AS EgresoVentas
        ON EgresoVentas.idSucursal = AllCortes.idSucursal
       AND EgresoVentas.idCorte = AllCortes.idCorte
    WHERE
        (@texto LIKE '')
        OR
        (
            @texto NOT LIKE ''
            AND
            (
                (@esNumero = 0 AND AllCortes.corte LIKE '%' + @texto + '%')
                OR
                (@esNumero = 1 AND CAST(AllCortes.codigo AS NCHAR) = @texto)
            )
        )
    ORDER BY
        AllCortes.codigo;
END
GO
