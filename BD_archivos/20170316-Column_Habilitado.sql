use SuperCerdo

ALTER TABLE dbo.Corte ADD habilitado tinyint NULL;

update dbo.Corte set habilitado = 1;