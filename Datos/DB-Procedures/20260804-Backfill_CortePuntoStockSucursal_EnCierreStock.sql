-- ============================================================================
-- Backfill puntual: dbo.CortePuntoStockSucursal para productos existentes
-- con enCierreStock activado, en todas las sucursales de su propia empresa.
--
-- Contexto: dbo.CortePuntoStockSucursal se siembra hoy en dos casos: al crear
-- una sucursal nueva (Web/Helpers/SystemAdministrationRepository.cs,
-- SembrarPuntoStockSucursalNueva) o via el backfill generico embebido en
-- 20260803-Create_CortePuntoStockSucursal.sql (sembro TODOS los productos de
-- una sola vez, sin filtrar por enCierreStock, y queda inerte para siempre
-- una vez que la tabla existe). Este script cubre el caso puntual: productos
-- que activaron enCierreStock despues de ese backfill, o entornos donde ese
-- backfill todavia no corrio.
--
-- Reglas:
--   - Solo productos con dbo.Corte.enCierreStock = 1 (NULL se trata como no
--     activado).
--   - Nunca productos con idEmpresa = 0 (catalogo global compartido).
--   - Valor de referencia: dbo.Corte.puntoStock del producto, o 0 si es NULL.
--   - Una fila por Producto x Sucursal, siempre de la MISMA empresa del
--     producto (join por idEmpresa).
--
-- Idempotente: usa NOT EXISTS, se puede correr mas de una vez sin duplicar
-- (dbo.CortePuntoStockSucursal ademas tiene un indice UNIQUE en
-- (idEmpresa, idCorte, idSucursal) que actua como red de seguridad).
--
-- RLS: dbo.Corte tiene la politica RLS_Empresa (funcion
-- fn_rls_empresa_o_global_v2), que bloquea TODAS las filas salvo que la
-- sesion se identifique como admin (no hay excepcion implicita para sa /
-- sysadmin en RLS de SQL Server). Se setea SESSION_CONTEXT('EsAdminCarniSys')
-- = 1, mismo mecanismo que usa la app en Utilidades/Conexion.cs ->
-- SetAdminSession(), para poder leer todas las empresas en una sola pasada.
-- El guard de OBJECT_ID evita el error en SQL Server 2008 (ej. servidor San
-- Martin), que no tiene sys.sp_set_session_context ni RLS.
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.CortePuntoStockSucursal', 'U') IS NULL
BEGIN
    PRINT 'dbo.CortePuntoStockSucursal no existe en esta base. No se realizaron cambios.';
    RETURN;
END;

IF OBJECT_ID('sys.sp_set_session_context') IS NOT NULL
BEGIN
    EXEC sys.sp_set_session_context @key = N'EsAdminCarniSys', @value = 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @FilasInsertadas INT;

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
      AND s.idEmpresa > 0
      AND ISNULL(c.enCierreStock, 0) = 1
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.CortePuntoStockSucursal existente
          WHERE existente.idEmpresa = c.idEmpresa
            AND existente.idCorte = c.idCorte
            AND existente.idSucursal = s.idSucursal
      );

    SET @FilasInsertadas = @@ROWCOUNT;
    PRINT CONVERT(VARCHAR(20), @FilasInsertadas) + ' fila(s) insertadas en dbo.CortePuntoStockSucursal.';

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
