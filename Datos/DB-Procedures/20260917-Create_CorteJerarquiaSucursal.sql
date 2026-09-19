-- Tabla override para poder marcar un corte como "independiente" (y decidir si aparece en
-- el cierre de stock) en una sucursal puntual, sin cambiar su configuracion global. Caso real
-- (ver docs/DECISIONS.md): el corte "Malvina" depende de "Carre" en San Martin, pero es
-- independiente en San Lorenzo -- misma empresa (SuperCerdo), dos sucursales, Corte.independiente/
-- Corte.enCierreStock son columnas por (idEmpresa, idCorte) sin dimension de sucursal, asi que
-- hoy es imposible representar esto con una sola fila.
--
-- Deliberadamente NO tiene idCorteMaestro: el override solo puede marcar "independiente"/"en
-- cierre de stock", nunca redirigir a un corte maestro distinto por sucursal (no hay ese caso de
-- uso hoy). Si no hay fila para (idEmpresa, idCorte, idSucursal), se usa el valor GLOBAL de
-- Corte tal cual. Ver Datos/DB-Procedures/20260803-Create_CortePuntoStockSucursal.sql como
-- precedente del mismo patron de tabla (grano idEmpresa+idCorte+idSucursal) para otro atributo.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.CorteJerarquiaSucursal', 'U') IS NOT NULL
BEGIN
    PRINT 'La tabla dbo.CorteJerarquiaSucursal ya existe. No se realizaron cambios.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.CorteJerarquiaSucursal
    (
        idCorteJerarquiaSucursal INT IDENTITY(1,1) NOT NULL,
        idEmpresa       INT NOT NULL,
        idCorte         INT NOT NULL,
        idSucursal      INT NOT NULL,
        independiente   BIT NOT NULL,
        enCierreStock   BIT NOT NULL,
        creado          DATETIME NOT NULL CONSTRAINT DF_CorteJerarquiaSucursal_creado DEFAULT (GETDATE()),
        actualizado     DATETIME NULL,
        CONSTRAINT PK_CorteJerarquiaSucursal PRIMARY KEY CLUSTERED (idCorteJerarquiaSucursal)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_CorteJerarquiaSucursal_Empresa_Corte_Sucursal
        ON dbo.CorteJerarquiaSucursal (idEmpresa, idCorte, idSucursal);

    -- Indice de apoyo: la CTE recursiva de stock busca por (idCorte, idSucursal) en cada
    -- nivel de la recursion.
    CREATE NONCLUSTERED INDEX IX_CorteJerarquiaSucursal_Corte_Sucursal
        ON dbo.CorteJerarquiaSucursal (idCorte, idSucursal) INCLUDE (independiente, enCierreStock);

    PRINT 'Tabla dbo.CorteJerarquiaSucursal creada. Sin backfill: la ausencia de fila ya significa "usar el valor global de dbo.Corte".';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
