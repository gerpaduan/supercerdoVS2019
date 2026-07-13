SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.ProductStockPointByBranchNG', 'U') IS NOT NULL
BEGIN
    PRINT 'La tabla dbo.ProductStockPointByBranchNG ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.ProductStockPointByBranchNG
    (
        IdProductStockPointByBranchNG INT IDENTITY(1,1) NOT NULL,
        IdEmpresa INT NOT NULL,
        IdCorte INT NOT NULL,
        IdSucursal INT NOT NULL,
        PuntoStock INT NOT NULL CONSTRAINT DF_ProductStockPointByBranchNG_PuntoStock DEFAULT (0),
        Creado DATETIME NOT NULL CONSTRAINT DF_ProductStockPointByBranchNG_Creado DEFAULT (GETDATE()),
        Actualizado DATETIME NULL,
        CONSTRAINT PK_ProductStockPointByBranchNG PRIMARY KEY CLUSTERED (IdProductStockPointByBranchNG)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_ProductStockPointByBranchNG_Empresa_Corte_Sucursal
        ON dbo.ProductStockPointByBranchNG (IdEmpresa, IdCorte, IdSucursal);

    INSERT INTO dbo.ProductStockPointByBranchNG
    (
        IdEmpresa,
        IdCorte,
        IdSucursal,
        PuntoStock
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

    PRINT 'Tabla dbo.ProductStockPointByBranchNG creada e inicializada correctamente.';
    PRINT 'Se copiaron los valores de dbo.Corte.puntoStock a cada sucursal de la empresa para cada producto propio.';

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
