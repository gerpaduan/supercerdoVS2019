-- Variante del fix de 20260818-Alter_addOrEditPersona_SessionContextFallback.sql para
-- instalaciones SIN el esquema multi-tenant/RLS (SQL Server 2008 no tiene SESSION_CONTEXT(),
-- funcion recien disponible desde SQL Server 2016). Aplica a ServidorSM y San Lorenzo
-- (base "SuperCerdo"), donde solo existe una Empresa real (idEmpresa=1) -- no a la base
-- "CarniSys" de la VM, que usa el otro script (RLS real, multi-tenant).
--
-- Mismo bug de origen: @idEmpresa tenia default 0 y Datos/Persona.cs nunca lo pasa como
-- parametro, asi que toda alta de Persona quedaba con idEmpresa=0 en vez del idEmpresa=1
-- real de la unica empresa de esta instalacion (confirmado en San Lorenzo: 168 Personas con
-- idEmpresa=1, 1 sola fila -- idPersona=176, "RADIO IDEAL" -- con idEmpresa=0 por este bug).
--
-- Ajustar el "1" de mas abajo si el idEmpresa real de este servidor no es 1 (confirmar antes
-- de correr, ej. SELECT idEmpresa, nombreFantasia FROM dbo.Empresas).
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
    @idEmpresa int = 1
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
            bonificacion = @bonificacion,
            marca = ISNULL(@marca, 0),
            idPropietario = @idPropietario
        WHERE idPersona = @idPersona;
    END

    SELECT @idPersona;
END
GO
