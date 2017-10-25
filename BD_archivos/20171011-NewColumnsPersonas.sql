use SuperCerdo

ALTER TABLE dbo.Personas ADD 
	idIva int NULL,
	cuit nvarchar(50) NULL,
	telefono nvarchar(50) NULL,
	domicilio nvarchar(50) NULL,
	ciudad nvarchar(50) NULL;