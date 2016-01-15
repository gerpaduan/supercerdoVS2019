USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Ventas]    Script Date: 01/13/2016 21:32:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Ventas](
	[idVenta] [int] IDENTITY(1,1) NOT NULL,
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

