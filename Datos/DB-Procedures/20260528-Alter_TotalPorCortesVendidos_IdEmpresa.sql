USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[TotalPorCortesVendidos]
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
