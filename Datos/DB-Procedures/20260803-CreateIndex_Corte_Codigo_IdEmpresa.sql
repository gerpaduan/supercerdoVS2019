SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Hallazgo incidental durante el testing del cambio de punto de stock por sucursal (no
-- relacionado con ese feature): dbo.Corte no tenia ningun indice util para buscar por
-- codigo. Con ~102.000 de ~102.008 filas en idEmpresa=0 (catalogo global), cualquier
-- consulta "WHERE codigo = @codigo AND idEmpresa = 0" (ej. findCorteGlobalByCodigo,
-- usada en cada alta de producto) escaneaba practicamente toda la tabla.
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Corte')
      AND name = 'IX_Corte_Codigo_IdEmpresa'
)
BEGIN
    PRINT 'El indice IX_Corte_Codigo_IdEmpresa ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    CREATE NONCLUSTERED INDEX IX_Corte_Codigo_IdEmpresa
        ON dbo.Corte (codigo, idEmpresa);

    PRINT 'Indice IX_Corte_Codigo_IdEmpresa creado correctamente.';
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
