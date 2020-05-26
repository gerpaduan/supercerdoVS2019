USE [SuperCerdo]

ALTER TABLE [dbo].[Ventas]
 ADD	
 comisionTarjeta float NULL
 
  
 ---ACTUALIZA REGISTROS PARA EVITAR NULOS---
 UPDATE Ventas SET
 comisionTarjeta = 0