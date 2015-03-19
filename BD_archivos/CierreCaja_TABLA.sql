USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[CierreCaja]    Script Date: 03/19/2015 17:29:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CierreCaja](
	[id] [int] NOT NULL,
	[idSucursal] [int] NULL,
	[fechaHoraInicio] [datetime] NULL,
	[fechaHoraCierre] [datetime] NULL,
	[cajaInicio] [float] NULL,
	[ventas] [float] NULL,
	[gastos] [float] NULL,
	[saldoCaja] [float] NULL,
	[cajaCierre] [float] NULL,
	[diferencia] [float] NULL,
	[cajaInicioSiguiente] [float] NULL,
	[importeRetirado] [float] NULL,
	[usuarioInicio] [varchar](50) NULL,
	[usuarioCierre] [varchar](50) NULL,
	[creado] [datetime] NULL,
	[actualizado] [datetime] NULL,
 CONSTRAINT [PK_CierreCaja] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


