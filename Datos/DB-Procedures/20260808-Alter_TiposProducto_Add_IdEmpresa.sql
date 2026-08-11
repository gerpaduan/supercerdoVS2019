USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- Agrega la columna idEmpresa a dbo.TiposProducto -- faltaba el script versionado
-- de este cambio (se hizo directo en Datos/Corte.cs, commit 8e25a6ff, 2026-06-19,
-- sin ALTER acompanante). Local y la VM de produccion ya tenian la columna por
-- distintos caminos; San Lorenzo (SQL Server 2008, single-empresa) nunca la
-- recibio y quedo con /Productos roto (columna 'idEmpresa' invalida) hasta
-- aplicarse a mano el 2026-08-08. Ver docs/09-cambios-y-pendientes/bitacora-de-cambios.md
-- y docs/DECISIONS.md, 2026-08-08.
--
-- Backfill automatico solo para el caso single-empresa (legacy, un unico registro
-- en dbo.Empresas): las filas no reservadas (reservadoSistema=0) quedan con el
-- idEmpresa real de esa unica empresa, para no perder visibilidad de los tipos
-- ya cargados bajo el filtro "WHERE (reservadoSistema=1 OR idEmpresa=@idEmpresa)"
-- que ya usa Datos/Corte.cs. Si el servidor es multi-empresa (mas de una fila en
-- Empresas, ej. la base local de desarrollo), el backfill se saltea a proposito
-- -- ahi corresponde una migracion de datos manual, no un default automatico.
-- ============================================================================

IF COL_LENGTH('dbo.TiposProducto', 'idEmpresa') IS NULL
BEGIN
    ALTER TABLE dbo.TiposProducto
        ADD idEmpresa int NOT NULL DEFAULT 0;

    IF (SELECT COUNT(*) FROM dbo.Empresas) = 1
    BEGIN
        DECLARE @idEmpresaUnica int = (SELECT TOP 1 idEmpresa FROM dbo.Empresas);

        UPDATE dbo.TiposProducto
            SET idEmpresa = @idEmpresaUnica
            WHERE reservadoSistema = 0;
    END
END
GO
