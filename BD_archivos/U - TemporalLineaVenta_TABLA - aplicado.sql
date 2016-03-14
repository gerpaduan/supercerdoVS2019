USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[TemporalLineaVenta]    Script Date: 03/14/2016 00:21:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TemporalLineaVenta](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idVendedor] [int] NULL,
	[fechaInicioPesada] [datetime] NULL,
	[idCorte] [int] NULL,
	[cantKg] [decimal](18, 3) NULL,
	[precioKg] [decimal](18, 2) NULL,
	[totalCorte] [decimal](18, 2) NULL,
	[ventaEnCurso] [tinyint] NULL,
	[idSucursal] [int] NULL,
	[creado] [datetime] NULL,
 CONSTRAINT [PK_TemporalLineaVenta] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

