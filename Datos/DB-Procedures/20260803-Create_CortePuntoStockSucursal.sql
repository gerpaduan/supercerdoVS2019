SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.CortePuntoStockSucursal', 'U') IS NOT NULL
BEGIN
    PRINT 'La tabla dbo.CortePuntoStockSucursal ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.CortePuntoStockSucursal
    (
        idCortePuntoStockSucursal INT IDENTITY(1,1) NOT NULL,
        idEmpresa   INT NOT NULL,
        idCorte     INT NOT NULL,
        idSucursal  INT NOT NULL,
        puntoStock  INT NOT NULL CONSTRAINT DF_CortePuntoStockSucursal_puntoStock DEFAULT (0),
        creado      DATETIME NOT NULL CONSTRAINT DF_CortePuntoStockSucursal_creado DEFAULT (GETDATE()),
        actualizado DATETIME NULL,
        CONSTRAINT PK_CortePuntoStockSucursal PRIMARY KEY CLUSTERED (idCortePuntoStockSucursal)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_CortePuntoStockSucursal_Empresa_Corte_Sucursal
        ON dbo.CortePuntoStockSucursal (idEmpresa, idCorte, idSucursal);

    -- Backfill: cada producto existente arranca con el mismo punto de stock (el que ya
    -- tenia en dbo.Corte.puntoStock) replicado a todas las sucursales de su empresa. A
    -- partir de aca cada combinacion Producto x Sucursal se administra de forma independiente.
    INSERT INTO dbo.CortePuntoStockSucursal
    (
        idEmpresa,
        idCorte,
        idSucursal,
        puntoStock
    )
    SELECT
        c.idEmpresa,
        c.idCorte,
        s.idSucursal,
        ISNULL(c.puntoStock, 0)
    FROM dbo.Corte c
    INNER JOIN dbo.Sucursal s
        ON s.idEmpresa = c.idEmpresa
    WHERE c.idEmpresa > 0
      AND s.idEmpresa > 0;

    PRINT 'Tabla dbo.CortePuntoStockSucursal creada e inicializada correctamente.';
    PRINT 'Se copio el punto de stock de dbo.Corte a cada sucursal de la empresa para cada producto propio.';

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
