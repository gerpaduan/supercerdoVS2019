SET NOCOUNT ON;
SET XACT_ABORT ON;

-- dbo.Corte tiene RLS (RLS_Empresa, funcion fn_rls_empresa_o_global_v2) que exige
-- SESSION_CONTEXT('IdEmpresa') seteado (o el login cs_admin, o
-- SESSION_CONTEXT('EsAdminCarniSys')=1) para ver CUALQUIER fila -- sin esto la
-- sesion no ve nada, ni siquiera idEmpresa=0. Se usa el flag admin (en vez de
-- fijar IdEmpresa a un valor puntual) porque el diagnostico de mas abajo
-- necesita ver productos de CUALQUIER empresa cruzados contra el catalogo
-- global, no solo los de una empresa fija.
EXEC sys.sp_set_session_context @key = N'EsAdminCarniSys', @value = 1;

-- Migra a dbo.CatalogoGlobalProducto (creada en
-- 20260804-Create_CatalogoGlobalProducto.sql) las filas de catalogo global que
-- hoy viven en dbo.Corte con idEmpresa = 0. Conserva el idCorte original: la
-- tabla puente dbo.CatalogoGlobalImportacionProductos ya referencia esos IDs en
-- IdProductoGlobal y no debe reescribirse.
--
-- Este script NO borra nada de dbo.Corte -- el borrado va en
-- 20260804-Delete_Corte_IdEmpresa0.sql, como paso separado y confirmado
-- explicitamente, recien despues de verificar la app contra esta migracion.
IF OBJECT_ID('dbo.CatalogoGlobalProducto', 'U') IS NULL
BEGIN
    RAISERROR('dbo.CatalogoGlobalProducto no existe. Correr primero 20260804-Create_CatalogoGlobalProducto.sql.', 16, 1);
    RETURN;
END;

IF EXISTS (SELECT 1 FROM dbo.CatalogoGlobalProducto)
BEGIN
    PRINT 'dbo.CatalogoGlobalProducto ya tiene datos. No se realizo ninguna migracion (evita duplicar filas).';
    RETURN;
END;

-- Diagnostico informativo (no bloquea la migracion): productos propios de una
-- empresa real (idEmpresa > 0) cuyo idCorteMaestro apunta a una fila que se
-- esta por migrar. Si aparece algo aca, esa fila de Corte va a quedar con una
-- referencia a un idCorte que ya no existe en Corte una vez que se corra el
-- script de borrado -- revisar antes de confirmar ese paso.
IF EXISTS (
    SELECT 1
    FROM dbo.Corte hijo
    INNER JOIN dbo.Corte maestro ON maestro.idCorte = hijo.idCorteMaestro
    WHERE hijo.idEmpresa > 0
      AND maestro.idEmpresa = 0
)
BEGIN
    PRINT 'ATENCION: hay productos propios de una empresa (idEmpresa > 0) cuyo idCorteMaestro apunta a una fila de catalogo global. Revisar antes de correr el DELETE:';

    SELECT
        hijo.idEmpresa,
        hijo.idCorte AS idCorteEmpresa,
        hijo.codigo AS codigoEmpresa,
        hijo.corte AS corteEmpresa,
        maestro.idCorte AS idCorteMaestroGlobal,
        maestro.codigo AS codigoMaestroGlobal,
        maestro.corte AS corteMaestroGlobal
    FROM dbo.Corte hijo
    INNER JOIN dbo.Corte maestro ON maestro.idCorte = hijo.idCorteMaestro
    WHERE hijo.idEmpresa > 0
      AND maestro.idEmpresa = 0;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO dbo.CatalogoGlobalProducto
    (
        idCorte, codigo, corte, tipo, idMarca, puntoStock, promedio, independiente,
        precioKg, ingresoRapidoEmbutido, habilitado, enCierreStock, idCorteMaestro,
        nivel, porcentaje, porcentajeHueso, desvioEstandar, creado, actualizado,
        idAlicuotaIva, alicuotaIva, pesable, mayorista
    )
    SELECT
        idCorte, codigo, corte, tipo, idMarca, puntoStock, promedio, independiente,
        precioKg, ingresoRapidoEmbutido, habilitado, enCierreStock, idCorteMaestro,
        nivel, porcentaje, porcentajeHueso, desvioEstandar, creado, actualizado,
        idAlicuotaIva, alicuotaIva, pesable, mayorista
    FROM dbo.Corte
    WHERE idEmpresa = 0;

    PRINT 'Migracion completada: ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' filas copiadas de dbo.Corte (idEmpresa=0) a dbo.CatalogoGlobalProducto.';

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
