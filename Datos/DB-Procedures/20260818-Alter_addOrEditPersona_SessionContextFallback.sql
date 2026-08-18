USE [carnisys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Bug: @idEmpresa tenia default literal 0 y Datos/Persona.cs nunca lo pasaba como parametro,
-- asi que toda alta de Persona via /Personas (flujo normal, cualquier tenant) quedaba con
-- idEmpresa=0 -- visible para TODAS las empresas (fila "global"), sin que nadie lo pidiera.
-- Fix: mismo patron que ya usa addOrEditUser -- default NULL + fallback a SESSION_CONTEXT,
-- con guard que tira error si no hay sesion de empresa valida. Las filas idEmpresa=0 ya
-- existentes (ej. "Consumidor Final", cargadas a mano a proposito) no se tocan: esta rama
-- solo corre en el INSERT (alta nueva), nunca en el UPDATE.
ALTER PROCEDURE [dbo].[addOrEditPersona]
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
    @idEmpresa int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @idPersona = 0
    BEGIN
        -- Leer desde SESSION_CONTEXT si no te lo pasan (mismo criterio que addOrEditUser)
        IF @idEmpresa IS NULL
            SET @idEmpresa = TRY_CAST(SESSION_CONTEXT(N'IdEmpresa') AS int);

        IF @idEmpresa IS NULL
            THROW 50030, 'No esta seteado IdEmpresa en SESSION_CONTEXT.', 1;

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
            @idEmpresa
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
