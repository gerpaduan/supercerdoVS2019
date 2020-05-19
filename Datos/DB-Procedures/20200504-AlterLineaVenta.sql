USE [SuperCerdo]

ALTER TABLE [dbo].[LineaVenta]
 ADD	
 kgsAjusteTarj float DEFAULT 0
 porcKgsAjusteTarj float DEFAULT 0
 
 
 
 ---ACTUALIZA REGISTROS PARA EVITAR NULOS---
 UPDATE LineaVenta SER
 kgsAjusteTarj = 0,
 porcKgsAjusteTarj = 0
 