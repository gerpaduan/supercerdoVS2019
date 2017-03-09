USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[Conexiones]    Script Date: 09/29/2016 18:23:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Conexiones](
	[name] [varchar](50) NOT NULL,
	[connectionString] [varchar](max) NULL,
	[nombre] [varchar](50) NULL,
	[idSucursal] [int] NULL,
	[mostrarEnPrincipal] [tinyint] NULL,
	[mostrarEnStockActual] [tinyint] NULL,
 CONSTRAINT [PK_Conexiones] PRIMARY KEY CLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


