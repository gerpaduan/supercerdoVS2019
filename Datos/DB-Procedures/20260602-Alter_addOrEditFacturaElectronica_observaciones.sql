USE [SuperCerdo]
GO

ALTER PROCEDURE [dbo].[addOrEditFacturaElectronica]
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
            mensajeError = @mensajeError,
            fechaError = @fechaError,
            cantErrores = @cantErrores
        WHERE id = @id;
    END

    SELECT @id;
END
GO
