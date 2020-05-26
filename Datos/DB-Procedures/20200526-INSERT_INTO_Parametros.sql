use SuperCerdo
--id	nombre	valor	descripcion
--6	comisionDebito	0,045	Comision que cobra MercadoPago en cobros con DEBITO
INSERT INTO [SuperCerdo].[dbo].[Parametros]
           ([id]
           ,[nombre]
           ,[valor]
           ,[descripcion])
     VALUES
           (6
           ,'comisionDebito'
           ,'0,045'
           ,'Comision que cobra MercadoPago en cobros con DEBITO')
--7	comisionCredito	0,075	Comision que cobra MercadoPago en cobros con CREDITO
INSERT INTO [SuperCerdo].[dbo].[Parametros]
           ([id]
           ,[nombre]
           ,[valor]
           ,[descripcion])
     VALUES
           (7
           ,'comisionCredito'
           ,'0,075'
           ,'Comision que cobra MercadoPago en cobros con CREDITO')
           
 
GO


