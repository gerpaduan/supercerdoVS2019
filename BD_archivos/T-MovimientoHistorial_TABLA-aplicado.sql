USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[MovimientoHistorial]]   Script Date: 04/01/2016 11:30:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MovimientoHistorial](
	[idMovimiento] [int] NULL,
	[FechaMovimiento] [datetime] NULL,
	[idSucOrigen] [int] NULL,
	[idSucDestino] [int] NULL,
	[idCorte] [int] NULL,
	[cantKg] [float] NULL,
	[cantUnidad] [int] NULL,
	[pesoBalanza] [bit] NULL,
	[actualizadoPor] [int] NULL,
	[actualizado] [datetime] NULL,
	[observaciones] [nvarchar] (max) NULL,
) ON [PRIMARY]

GO


