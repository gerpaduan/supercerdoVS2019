USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Pagos]    Script Date: 08/12/2016 18:47:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Pagos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nroRecibo] [nvarchar](50) NULL,
	[fecha] [datetime] NULL,
	[idPersona] [int] NULL,
	[aProveedor] [tinyint] NULL,
	[formaPago] [nvarchar](50) NULL,
	[banco] [nvarchar](50) NULL,
	[nroCheque] [nvarchar](50) NULL,
	[titularCheque] [nvarchar](50) NULL,
	[importe] [float] NULL,
	[observaciones] [nvarchar](max) NULL,
	[idSucursal] [int] NULL,
	[creado] [datetime] NULL,
	[creadoPor] [int] NULL,
	[actualizado] [datetime] NULL,
	[actualizadoPor] [int] NULL,
 CONSTRAINT [PK_Pagos] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


