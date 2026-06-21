USE [CarniSys]
GO

IF COL_LENGTH('dbo.Compras', 'idPesajeAjustado') IS NULL
BEGIN
    ALTER TABLE dbo.Compras
    ADD idPesajeAjustado INT NULL;
END
GO

UPDATE aj
SET aj.idPesajeAjustado = pes.idCompra
FROM dbo.Compras aj
INNER JOIN dbo.Compras pes
    ON pes.idCompra = CONVERT(int, aj.nroRemito)
WHERE aj.tipoCompra = 'Ajuste Stock'
  AND pes.tipoCompra = 'Pesaje Cortes'
  AND (aj.idPesajeAjustado IS NULL OR aj.idPesajeAjustado = 0)
  AND aj.nroRemito IS NOT NULL
  AND aj.nroRemito <> ''
  AND aj.nroRemito NOT LIKE '%[^0-9]%';
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[addOrEditCompra]
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

ALTER PROCEDURE [dbo].[agregarCompra]
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

ALTER PROCEDURE [dbo].[modificarCompra]
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
