USE [SuperCerdo]
GO

/****** Object:  StoredProcedure [dbo].[ActualizacionStockTotal]    Script Date: 05/16/2020 12:01:02 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActualizacionStockTotal]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ActualizacionStockTotal]
DROP PROCEDURE [dbo].[ActualizacionStockTotalTeorico]
DROP PROCEDURE [dbo].[actualizarStockCortesPrimarios]
DROP PROCEDURE [dbo].[actualizarStockEmbutido]
DROP PROCEDURE [dbo].[actualizarStockPorCorte]

DROP TABLE [dbo].[ActualizacionStock]
DROP TABLE [dbo].[ActualizacionStockPorCorte]
GO


