USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Parametros]    Script Date: 05/19/2020 12:04:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Parametros](
	[id] [int] NOT NULL,
	[nombre] [nvarchar](50) NULL,
	[valor] [nvarchar](50) NULL,
	[descripcion] [nvarchar](250) NULL
) ON [PRIMARY]

GO


