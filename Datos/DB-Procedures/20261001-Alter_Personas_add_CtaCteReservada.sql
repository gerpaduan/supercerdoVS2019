USE [CarniSys]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01). Columna aditiva con default 0:
-- ninguna persona queda reservada de entrada (el modulo Empleados es solo Postgres, asi que
-- aca no hay empleados que marcar). Personas.ctaCte queda sin efecto funcional.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Personas') AND name = 'ctaCteReservada'
)
BEGIN
    ALTER TABLE dbo.Personas ADD ctaCteReservada bit NOT NULL DEFAULT 0;
END
GO

-- addOrEditPersona: agrega @ctaCteReservada (default NULL: un caller viejo que no lo mande no
-- pisa el valor existente en el UPDATE; en el INSERT queda 0). Resto del cuerpo identico a
-- 20260818-Alter_addOrEditPersona_SessionContextFallback.sql.
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
    @idEmpresa int = NULL,
    @ctaCteReservada bit = NULL
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
            ctaCteReservada,
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
            ISNULL(@ctaCteReservada, 0),
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
            ctaCteReservada = ISNULL(@ctaCteReservada, ctaCteReservada),
            bonificacion = @bonificacion,
            marca = ISNULL(@marca, 0),
            idPropietario = @idPropietario
        WHERE idPersona = @idPersona;
    END

    SELECT @idPersona;
END
GO
