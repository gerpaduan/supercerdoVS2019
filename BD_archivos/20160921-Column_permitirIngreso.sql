use SuperCerdo

ALTER TABLE dbo.CortePorMovimiento ADD permitirIngreso tinyint NULL;

UPDATE dbo.CortePorMovimiento set permitirIngreso = 0 