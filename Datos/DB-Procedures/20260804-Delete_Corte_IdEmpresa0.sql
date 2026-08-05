SET NOCOUNT ON;
SET XACT_ABORT ON;

-- PASO DESTRUCTIVO -- correr SOLO despues de:
--   1) Ejecutar 20260804-Create_CatalogoGlobalProducto.sql y
--      20260804-Migrate_CatalogoGlobalProducto_desde_Corte.sql en este mismo
--      ambiente y confirmar que los conteos coinciden (ver verificacion mas
--      abajo).
--   2) Probar en la app (Web) el flujo de catalogo global (ver, buscar,
--      importar, alta rapida por codigo de barra) contra los datos ya
--      migrados.
--   3) Tomar un snapshot/backup de la base (guard de CLAUDE.md #7).
--   4) Confirmacion explicita del usuario para este ambiente puntual -- este
--      script borra ~102.000 filas en la base local de desarrollo (el numero
--      real depende del ambiente; en produccion/SM/San Lorenzo puede ser otro,
--      correr aparte y con su propia confirmacion, ver plan).
--
-- No se toca la RLS fn_rls_empresa_o_global_v2: su rama "OR idEmpresa=0" queda
-- inerte pero inofensiva (nunca va a matchear nada en dbo.Corte).
--
-- dbo.Corte tiene RLS (fn_rls_empresa_o_global_v2 para lectura,
-- fn_rls_block_empresa_o_global_v2 para bloquear el DELETE) que exige
-- SESSION_CONTEXT('IdEmpresa') seteado o el flag admin -- sin esto ni se ven ni
-- se pueden borrar las filas globales. Se usa el flag admin (no un IdEmpresa
-- puntual) porque el diagnostico de mas abajo necesita ver productos de
-- cualquier empresa cruzados contra el catalogo global.
EXEC sys.sp_set_session_context @key = N'EsAdminCarniSys', @value = 1;

IF OBJECT_ID('dbo.CatalogoGlobalProducto', 'U') IS NULL
BEGIN
    RAISERROR('dbo.CatalogoGlobalProducto no existe. Correr primero los scripts de creacion y migracion.', 16, 1);
    RETURN;
END;

DECLARE @FilasGlobalEnCorte INT = (SELECT COUNT(*) FROM dbo.Corte WHERE idEmpresa = 0);
DECLARE @FilasEnCatalogoGlobal INT = (SELECT COUNT(*) FROM dbo.CatalogoGlobalProducto);

IF @FilasGlobalEnCorte <> @FilasEnCatalogoGlobal
BEGIN
    RAISERROR('El conteo de filas idEmpresa=0 en dbo.Corte (%d) no coincide con dbo.CatalogoGlobalProducto (%d). No se borra nada hasta resolver la diferencia.', 16, 1, @FilasGlobalEnCorte, @FilasEnCatalogoGlobal);
    RETURN;
END;

-- Mismo guard que en el script de migracion: si algun producto propio de una
-- empresa real quedo referenciando como idCorteMaestro una fila de catalogo
-- global, no se borra nada hasta resolver esa inconsistencia (quedaria una FK
-- logica rota en dbo.Corte).
IF EXISTS (
    SELECT 1
    FROM dbo.Corte hijo
    INNER JOIN dbo.Corte maestro ON maestro.idCorte = hijo.idCorteMaestro
    WHERE hijo.idEmpresa > 0
      AND maestro.idEmpresa = 0
)
BEGIN
    RAISERROR('Hay productos propios de una empresa (idEmpresa > 0) cuyo idCorteMaestro apunta a una fila de catalogo global en dbo.Corte. Resolver antes de borrar (ver el diagnostico del script de migracion).', 16, 1);
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM dbo.Corte WHERE idEmpresa = 0;

    PRINT 'Borrado completado: ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' filas de catalogo global eliminadas de dbo.Corte.';

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
