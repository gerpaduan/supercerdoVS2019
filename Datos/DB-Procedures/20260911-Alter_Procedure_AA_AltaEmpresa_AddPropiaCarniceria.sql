USE [CarniSys]
GO

-- Item 6 (2026-09-11, ver docs/DECISIONS.md "esquema Empresas: EmpresaPropia/EsCarniceria"):
-- agrega los 2 parametros nuevos a AA_AltaEmpresa (usado por SystemAdministrationRepository.
-- CrearEmpresa/CrearEmpresaInterna). Este procedure NO esta versionado en git -- se releyo la
-- definicion REAL y actual de esta base local via OBJECT_DEFINITION('dbo.AA_AltaEmpresa') antes
-- de escribir este ALTER (2026-09-11), NO se confio en la foto vieja de
-- docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql. Cuerpo identico al leido en
-- vivo, con 2 agregados unicamente: los parametros @empresaPropia/@esCarniceria (default 0,
-- mismo criterio que @esRRII) y sus columnas correspondientes en el INSERT INTO dbo.Empresas.

CREATE OR ALTER PROCEDURE [dbo].[AA_AltaEmpresa]
(
    @razonSocialAfip        NVARCHAR(100) = NULL,
    @cuit                   BIGINT        = NULL,
    @nombreFantasia         NVARCHAR(100) = NULL,
    @slogan1                NVARCHAR(MAX) = NULL,
    @slogan2                NVARCHAR(MAX) = NULL,
    @slogan3                NVARCHAR(MAX) = NULL,
    @iibb                   BIGINT        = NULL,
    @condicionIVA           NVARCHAR(100) = NULL,
    @inicioActividad        DATE          = NULL,
    @tenantSlug             NVARCHAR(100) = NULL,
    @domicilio              NVARCHAR(100) = NULL,
    @ciudad                 NVARCHAR(100) = NULL,
    @pais                   NVARCHAR(100) = NULL,
    @telefono               NVARCHAR(100) = NULL,
    @email                  NVARCHAR(100) = NULL,
    @basePath               NVARCHAR(100) = NULL,
    @esRRII                 TINYINT       = 0,
    @nombreCertificado_pfx  NVARCHAR(100) = NULL,
    @entorno_HOMO_PROD      NVARCHAR(100) = NULL,
    @baseDatosNombre        NVARCHAR(100) = NULL,
    @activa                 TINYINT       = 1,
    @creado					DATE          = sysdatetime,
    @observaciones          NVARCHAR(MAX) = NULL,
    @empresaPropia          BIT           = 0,
    @esCarniceria           BIT           = 0,
    @idEmpresa              INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Evita colisiones por concurrencia (mismo CUIT / mismo id libre)
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRAN;

    -------------------------------------------------------------------
    -- 0) Validar que no exista otra empresa con el mismo CUIT (si viene)
    -------------------------------------------------------------------
    IF @cuit IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM dbo.Empresas WITH (UPDLOCK, HOLDLOCK)
            WHERE cuit = @cuit
        )
        BEGIN
            THROW 50010, 'Ya existe una empresa con ese mismo CUIT.', 1;
        END
    END

    -------------------------------------------------------------------
    -- 1) Obtener el primer idEmpresa positivo disponible (mas bajo)
    --    Si no hay huecos: usa MAX+1
    -------------------------------------------------------------------
    DECLARE @nuevoId INT;

    IF NOT EXISTS (SELECT 1 FROM dbo.Empresas WITH (UPDLOCK, HOLDLOCK) WHERE idEmpresa = 1)
    BEGIN
        SET @nuevoId = 1;
    END
    ELSE
    BEGIN
        SELECT TOP (1) @nuevoId = e.idEmpresa + 1
        FROM dbo.Empresas e WITH (UPDLOCK, HOLDLOCK)
        WHERE e.idEmpresa >= 1
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.Empresas e2 WITH (UPDLOCK, HOLDLOCK)
              WHERE e2.idEmpresa = e.idEmpresa + 1
          )
        ORDER BY e.idEmpresa;

        -- Si no encontro hueco, usa MAX+1
        IF @nuevoId IS NULL
            SELECT @nuevoId = MAX(idEmpresa) + 1
            FROM dbo.Empresas WITH (UPDLOCK, HOLDLOCK)
            WHERE idEmpresa >= 1;
    END

    SET @idEmpresa = @nuevoId;

    -------------------------------------------------------------------
    -- 2) Insertar Empresa
    -------------------------------------------------------------------
    INSERT INTO dbo.Empresas
    (
        idEmpresa, razonSocialAfip, cuit, nombreFantasia,
        slogan1, slogan2, slogan3,
        iibb, condicionIVA, inicioActividad,
        tenantSlug, domicilio, ciudad, pais,
        telefono, email, basePath,
        esRRII, nombreCertificado_pfx, entorno_HOMO_PROD,
        baseDatosNombre, activa, creado, observaciones,
        EmpresaPropia, EsCarniceria
    )
    VALUES
    (
        @idEmpresa, @razonSocialAfip, @cuit, @nombreFantasia,
        @slogan1, @slogan2, @slogan3,
        @iibb, @condicionIVA, @inicioActividad,
        @tenantSlug, @domicilio, @ciudad, @pais,
        @telefono, @email, @basePath,
        @esRRII, @nombreCertificado_pfx, @entorno_HOMO_PROD,
        @baseDatosNombre, @activa, @creado, @observaciones,
        @empresaPropia, @esCarniceria
    );

    -------------------------------------------------------------------
    -- 3) Copiar parametros default (-1) a la empresa recien creada
    -------------------------------------------------------------------
    INSERT INTO dbo.EmpresaParametros (idEmpresa, idParametro, valor)
    SELECT
        @idEmpresa,
        ep.idParametro,
        ep.valor
    FROM dbo.EmpresaParametros ep
    WHERE ep.idEmpresa = -1
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.EmpresaParametros ep2
          WHERE ep2.idEmpresa = @idEmpresa
            AND ep2.idParametro = ep.idParametro
      );

    -------------------------------------------------------------------
    -- 4) Insertar Sucursal por defecto (datos vacios)
    --    sucursal = 'Suc.' + razonSocial (recortado a 50)
    -------------------------------------------------------------------
    INSERT INTO dbo.Sucursal
    (
        sucursal,
        idEmpresa,
        direccion,
        localidad,
        provincia,
        pais,
        codPuntoVentaAfip,
		creado, observaciones
    )
    VALUES
    (
        LEFT(CONCAT(N'Suc.', COALESCE(@razonSocialAfip, N'')), 50),
        @idEmpresa,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
		@creado, @observaciones
    );

    COMMIT;
END
GO
