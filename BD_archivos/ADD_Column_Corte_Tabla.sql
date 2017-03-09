use SuperCerdo

ALTER TABLE dbo.Corte ADD mayorista tinyint NULL;
ALTER TABLE dbo.Corte ADD enCierreStock tinyint NULL;

ALTER TABLE dbo.ActualizacionCorte ADD mayorista tinyint NULL;
ALTER TABLE dbo.ActualizacionCorte ADD enCierreStock tinyint NULL;
