USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Gastos]    Script Date: 02/09/2016 12:06:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Gastos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[fechaHora] [datetime] NULL,
	[idTipoGasto] [int] NULL,
	[descripcion] [nvarchar](50) NULL,
	[detalle] [nvarchar](max) NULL,
	[monto] [float] NULL,
	[idSucursal] [int] NULL,
	[creado] [datetime] NULL,
	[creadoPor] [int] NULL,
	[actualizado] [datetime] NULL,
	[actualizadoPor] [int] NULL,
 CONSTRAINT [PK_Gastos] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

