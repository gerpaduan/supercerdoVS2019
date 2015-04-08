USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Ventas]    Script Date: 04/05/2015 11:19:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Ventas](
	[idVenta] [int] NOT NULL,
	[idVendedor] [int] NULL,
	[fechaVenta] [datetime] NULL,
	[turno] [nvarchar](50) NULL,
	[idSucursal] [int] NULL,
	[diaFestivo] [nvarchar](50) NULL,
	[observaciones] [nvarchar](max) NULL,
	[idPersona] [int] NULL,
	[nroRemito] [nvarchar](50) NULL,
	[estado] [nvarchar](50) NULL,
	[creado] [datetime] NULL,
	[actualizado] [datetime] NULL,
 CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED 
(
	[idVenta] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

