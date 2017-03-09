USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[MovCtaCte]    Script Date: 08/12/2016 19:30:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MovCtaCte](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idPersona] [int] NULL,
	[fecha] [datetime] NULL,
	[tabla] [nvarchar](50) NULL,
	[idTabla] [int] NULL,
	[detalle] [nvarchar](max) NULL,
	[tipo] [nvarchar](50) NULL,
	[importe] [float] NULL,
	[idSucursal] [int] NULL,
	[creado] [datetime] NULL,
	[creadoPor] [int] NULL,
	[actualizado] [datetime] NULL,
	[actualizadoPor] [int] NULL,
 CONSTRAINT [PK_MovCtaCte] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


