SET NOCOUNT ON;
SET XACT_ABORT ON;

-- FindPorSucursal (Datos/CortePuntoStockSucursal.cs) hace WHERE idSucursal = @idSucursal,
-- pero el unico indice existente (UX_CortePuntoStockSucursal_Empresa_Corte_Sucursal) tiene
-- idSucursal como ultima columna de la clave -> no sirve para ese filtro, obliga a un scan
-- completo de la tabla. Se llama en cada carga de Existencia por Sucursales (una vez por
-- sucursal en el resultado) y de Stock Actual/Cierre Stock. Invisible hoy (pocas filas),
-- crece como productos x sucursales x empresas.
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.CortePuntoStockSucursal')
      AND name = 'IX_CortePuntoStockSucursal_Sucursal'
)
BEGIN
    PRINT 'El indice IX_CortePuntoStockSucursal_Sucursal ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE NONCLUSTERED INDEX IX_CortePuntoStockSucursal_Sucursal
        ON dbo.CortePuntoStockSucursal (idSucursal)
        INCLUDE (idCorte, puntoStock);

    PRINT 'Indice IX_CortePuntoStockSucursal_Sucursal creado correctamente.';

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
