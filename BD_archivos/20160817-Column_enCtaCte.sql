use SuperCerdo

ALTER TABLE dbo.Ventas ADD enCtaCte tinyint NULL;
ALTER TABLE dbo.Compras ADD enCtaCte tinyint NULL;


update dbo.Ventas set enCtaCte = 0;
update dbo.Compras set enCtaCte = 0;