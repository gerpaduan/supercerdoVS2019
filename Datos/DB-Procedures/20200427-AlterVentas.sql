USE [SuperCerdo]

ALTER TABLE [dbo].[Ventas]
 ADD	
 formaPago nvarchar(50) NULL,
 tipoComprobante char NULL,
 cuit nvarchar(50) NULL,
 email nvarchar(50) NULL,
 acumRedondeoKgs float NULL,
 acumRedondeoImporte float NULL