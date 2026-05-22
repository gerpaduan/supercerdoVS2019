USE [SuperCerdo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH('dbo.Personas', 'idEmpresa') IS NULL
BEGIN
    ALTER TABLE dbo.Personas
    ADD idEmpresa int NULL;
END
GO

UPDATE dbo.Personas
SET idEmpresa = 0
WHERE idEmpresa IS NULL;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Personas')
      AND name = 'idEmpresa'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Personas
    ALTER COLUMN idEmpresa int NOT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.Personas')
      AND c.name = 'idEmpresa'
)
BEGIN
    ALTER TABLE dbo.Personas
    ADD CONSTRAINT DF_Personas_idEmpresa DEFAULT (0) FOR idEmpresa;
END
GO
