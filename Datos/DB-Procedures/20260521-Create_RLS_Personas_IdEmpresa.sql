USE [SuperCerdo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.Personas', 'U') IS NULL
BEGIN
    RAISERROR('No existe la tabla dbo.Personas.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH('dbo.Personas', 'idEmpresa') IS NULL
BEGIN
    RAISERROR('La tabla dbo.Personas no tiene la columna idEmpresa.', 16, 1);
    RETURN;
END
GO

CREATE OR ALTER FUNCTION dbo.fn_RLS_Personas_IdEmpresa(@idEmpresa int)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
(
    SELECT 1 AS fn_result
    WHERE
        CONVERT(int, ISNULL(SESSION_CONTEXT(N'IdEmpresa'), 0)) = 0
        OR @idEmpresa = 0
        OR @idEmpresa = CONVERT(int, ISNULL(SESSION_CONTEXT(N'IdEmpresa'), 0))
);
GO

IF EXISTS
(
    SELECT 1
    FROM sys.security_policies
    WHERE name = 'RLS_Personas_IdEmpresa'
)
BEGIN
    ALTER SECURITY POLICY dbo.RLS_Personas_IdEmpresa
    WITH (STATE = OFF);

    DROP SECURITY POLICY dbo.RLS_Personas_IdEmpresa;
END
GO

CREATE SECURITY POLICY dbo.RLS_Personas_IdEmpresa
ADD FILTER PREDICATE dbo.fn_RLS_Personas_IdEmpresa(idEmpresa)
ON dbo.Personas
WITH (STATE = ON);
GO
