USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[LineaVenta]    Script Date: 04/07/2015 19:19:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LineaVenta](
	[idLineaVenta] [int] IDENTITY(1,1) NOT NULL,
	[idVenta] [int] NULL,
	[idCorte] [int] NULL,
	[idAnulado] [int] NULL,
	[cantKg] [float] NULL,
	[precioKg] [float] NULL,
	[pesoBalanza] [tinyint] NULL,
 CONSTRAINT [PK_LineaVenta_1] PRIMARY KEY CLUSTERED 
(
	[idLineaVenta] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

