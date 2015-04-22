USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Compras]    Script Date: 04/18/2015 10:59:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Compras](
	[idCompra] [int] IDENTITY(1,1) NOT NULL,
	[estado] [nvarchar](50) NULL,
	[fechaCompra] [datetime] NULL,
	[nroRemito] [nvarchar](50) NULL,
	[idProveedor] [int] NULL,
	[tipoCompra] [nvarchar](50) NULL,
	[observaciones] [nvarchar](200) NULL,
	[creado] [datetime] NULL,
	[creadoPor] [int] NULL,
	[actualizado] [datetime] NULL,
	[actualizadoPor] [int] NULL,
 CONSTRAINT [PK_Compras] PRIMARY KEY CLUSTERED 
(
	[idCompra] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

