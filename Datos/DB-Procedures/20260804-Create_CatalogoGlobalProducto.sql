SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Separa el catalogo global de productos (hoy mezclado dentro de dbo.Corte con
-- idEmpresa = 0) en una tabla propia. Motivo: en la base local dbo.Corte tiene
-- ~102.000 de ~102.008 filas en idEmpresa = 0 -- ese volumen es el que causo los
-- 3 incidentes de performance documentados en
-- docs/07-operacion-y-soporte/incidencias-frecuentes.md (linea 90 en adelante,
-- REGLA del 2026-08-01). Los indices agregados el 2026-08-03
-- (IX_Corte_Codigo_IdEmpresa, IX_Corte_IdCorteMaestro_Solo) mitigan pero no
-- resuelven de raiz: dbo.Corte sigue teniendo ~102.000 filas ajenas a cualquier
-- empresa real. Esta tabla nueva es el destino; el borrado de esas filas en
-- dbo.Corte va en un script aparte (20260804-Delete_Corte_IdEmpresa0.sql),
-- ejecutado solo tras confirmacion explicita.
--
-- Columnas/tipos verificados contra la base local (.\sqlexpress, 2026-08-04) via
-- sys.columns -- NO inferidos de Entidades/Corte.cs. dbo.Corte no tiene ningun
-- CREATE TABLE versionado en el repo (tabla legacy) y casi todas sus columnas
-- son NULLable sin DEFAULT a nivel de motor (el codigo C# aplica sus propios
-- defaults al leer, ver Datos/Corte.cs GetXSafe). Los "booleanos" son TINYINT,
-- no BIT. Se replican ambas cosas tal cual para no introducir constraints que
-- el origen no tiene.
--
-- idCorte se conserva igual al idCorte que la fila tenia en dbo.Corte (no se
-- generan IDs nuevos, por eso sin IDENTITY aca): dbo.CatalogoGlobalImportacionProductos.
-- IdProductoGlobal ya referencia esos valores y no debe reescribirse.
--
-- mayorista: columna presente en dbo.Corte que ningun codigo C# de este repo lee
-- ni escribe (no esta en Entidades.Corte ni en los SELECT/INSERT de Datos/Corte.cs).
-- Se incluye igual aca para no perder el dato al migrar, sin mapearla a la capa
-- de aplicacion -- si en el futuro se determina su uso real, se agrega.
--
-- Sin RLS: todo lo que vive en esta tabla es global por definicion, visible
-- para cualquier empresa sin necesidad de SESSION_CONTEXT.
IF OBJECT_ID('dbo.CatalogoGlobalProducto', 'U') IS NOT NULL
BEGIN
    PRINT 'La tabla dbo.CatalogoGlobalProducto ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.CatalogoGlobalProducto
    (
        idCorte                INT NOT NULL,
        codigo                  BIGINT NULL,
        corte                   NVARCHAR(50) NULL,
        tipo                    NVARCHAR(50) NULL,
        idCorteMaestro          INT NULL,
        porcentaje              FLOAT NULL,
        precioKg                FLOAT NULL,
        porcentajeHueso         FLOAT NULL,
        independiente           INT NULL,
        desvioEstandar          FLOAT NULL,
        creado                  DATETIME NULL,
        actualizado             DATETIME NULL,
        mayorista               TINYINT NULL,
        enCierreStock           TINYINT NULL,
        promedio                FLOAT NULL,
        habilitado              TINYINT NULL,
        idAlicuotaIva           INT NULL,
        alicuotaIva             FLOAT NULL,
        ingresoRapidoEmbutido   TINYINT NULL,
        nivel                   INT NULL,
        pesable                 TINYINT NULL,
        puntoStock              INT NULL,
        idMarca                 INT NULL,
        CONSTRAINT PK_CatalogoGlobalProducto PRIMARY KEY CLUSTERED (idCorte)
    );

    -- idCorteMaestro no lleva FK declarada: dbo.Corte tampoco la tiene hoy (la
    -- relacion se resuelve en la app via LEFT JOIN, tolera huerfanos). Se
    -- mantiene el mismo criterio para no bloquear la migracion con un
    -- constraint que no existia en el origen.
    CREATE NONCLUSTERED INDEX IX_CatalogoGlobalProducto_Codigo
        ON dbo.CatalogoGlobalProducto (codigo);

    CREATE NONCLUSTERED INDEX IX_CatalogoGlobalProducto_IdCorteMaestro
        ON dbo.CatalogoGlobalProducto (idCorteMaestro);

    PRINT 'Tabla dbo.CatalogoGlobalProducto creada correctamente.';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
