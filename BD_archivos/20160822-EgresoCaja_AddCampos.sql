use SuperCerdo

ALTER TABLE dbo.EgresosCaja ADD tabla nvarchar(50) NULL;
ALTER TABLE dbo.EgresosCaja ADD idTabla int NULL;

update dbo.EgresosCaja set idTabla = 0;