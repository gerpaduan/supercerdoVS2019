USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[TiposEgresoCaja]    Script Date: 04/22/2016 10:57:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TiposEgresoCaja](
	[id] [int] NOT NULL,
	[tipoEgresoCaja] [nvarchar](50) NULL,
 CONSTRAINT [PK_TiposEgresoCaja] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

