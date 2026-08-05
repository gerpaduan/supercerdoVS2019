SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Segundo hallazgo incidental durante el mismo testing (ver 20260803-CreateIndex_Corte_Codigo_IdEmpresa.sql):
-- el SP a_ExistenciaStockPorSucursales arma un CTE recursivo autouniendo dbo.Corte por
-- idCorteMaestro ("hijo.idCorteMaestro = padre.idCorte"). El unico indice que incluye esa
-- columna la tiene como quinta columna clave (IX_Corte_idCorteMaestro: idEmpresa,
-- independiente, codigo, corte, idCorteMaestro), inutil para ese join. Sin un indice propio,
-- ese self-join sobre ~102.000 filas fuerza un scan en cada nivel de recursion.
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Corte')
      AND name = 'IX_Corte_IdCorteMaestro_Solo'
)
BEGIN
    PRINT 'El indice IX_Corte_IdCorteMaestro_Solo ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    CREATE NONCLUSTERED INDEX IX_Corte_IdCorteMaestro_Solo
        ON dbo.Corte (idCorteMaestro, idCorte);

    PRINT 'Indice IX_Corte_IdCorteMaestro_Solo creado correctamente.';
END TRY
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
