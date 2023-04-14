USE [SuperCerdo]

ALTER TABLE [dbo].[Personas]
 ADD	
 identificacion nvarchar(50) NULL
 
 
 -----ACTUALIZA REGISTROS PARA EVITAR NULOS---
 --UPDATE Personas SET
 --identificacion = razonSocial