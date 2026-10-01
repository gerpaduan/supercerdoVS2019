USE [SuperCerdo]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01) -- variante SIN RLS / SIN SESSION_CONTEXT
-- para ServidorSM, San Lorenzo y la base local "SuperCerdo" (SQL Server 2008: no existe
-- SESSION_CONTEXT(), THROW ni TRY_CAST). Es el equivalente de 20261001-Alter_Personas_add_CtaCteReservada.sql,
-- que es SOLO para la base "CarniSys" (RLS). NO correr aquel script en estas bases: reemplazaria
-- addOrEditPersona por la variante con SESSION_CONTEXT y las altas de Personas dejarian de funcionar.
-- Antes de reemplazar el SP se verifica que sea la variante esperada (20260818-Alter_addOrEditPersona_
-- DefaultIdEmpresa1_SinRLS.sql); si no lo es, aborta sin tocarlo. Ajustar el "1" si el idEmpresa real
-- del servidor no es 1.

-- Columna aditiva con default 0: ninguna persona queda reservada de entrada (el modulo Empleados es
-- solo Postgres, aca no hay empleados que marcar). Personas.ctaCte queda sin efecto funcional.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Personas') AND name = 'ctaCteReservada'
)
BEGIN
    ALTER TABLE dbo.Personas ADD ctaCteReservada bit NOT NULL DEFAULT 0;
END
GO

-- Guarda: el SP actual debe ser la variante SinRLS (default @idEmpresa = 1, sin SESSION_CONTEXT). Se busca la
-- llamada real SESSION_CONTEXT(N'...' y no el texto, porque el comentario del SP SinRLS nombra la funcion.
IF OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditPersona')) LIKE '%SESSION_CONTEXT(N''%'
BEGIN
    RAISERROR('addOrEditPersona ya es la variante con SESSION_CONTEXT (base CarniSys con RLS): usar 20261001-Alter_Personas_add_CtaCteReservada.sql. No se modifico el procedimiento.', 16, 1);
    SET NOEXEC ON;
END
GO

-- addOrEditPersona: agrega @ctaCteReservada (default NULL: un caller viejo que no lo mande no pisa el
-- valor existente en el UPDATE; en el INSERT queda 0). Resto del cuerpo identico a la variante SinRLS.
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
    @idEmpresa int = 1,
    @ctaCteReservada bit = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @idPersona = 0
    BEGIN
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
            ISNULL(@idEmpresa, 1)
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

SET NOEXEC OFF
GO
