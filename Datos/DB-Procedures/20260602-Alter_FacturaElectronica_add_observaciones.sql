USE [SuperCerdo]
GO

IF COL_LENGTH('dbo.FacturaElectronica', 'observaciones') IS NULL
BEGIN
    ALTER TABLE dbo.FacturaElectronica
        ADD observaciones NVARCHAR(500) NOT NULL
            CONSTRAINT DF_FacturaElectronica_observaciones DEFAULT ('');
END
GO
