USE [SuperCerdo]

--ALTER TABLE [dbo].[LineaVenta]
-- ADD	
-- ajustePrecio float DEFAULT 0
 
 
 
 ---ACTUALIZA REGISTROS PARA EVITAR NULOS---
 UPDATE LineaVenta SET
 ajustePrecio = 0
 