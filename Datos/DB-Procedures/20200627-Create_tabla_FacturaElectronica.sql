USE [SuperCerdo]
GO

/****** Object:  Table [dbo].[FacturaElectronica]    Script Date: 06/27/2020 10:26:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FacturaElectronica](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ptoVtaAfip] [nvarchar](50) NULL,
	[fechaEmisionAfip] [datetime] NULL,
	[descTipoCbteAfip] [nvarchar](50) NULL,
	[codTipoCbteAfip] [int] NULL,
	[nroCbteAfip] [nvarchar](50) NULL,
	[tipoDocAfip] [nvarchar](50) NULL,
	[nroDocAfip] [nvarchar](50) NULL,
	[razonSocialAFIP] [nvarchar](50) NULL,
	[condicionIvaAFIP] [nvarchar](50) NULL,
	[domicilioAFIP] [nvarchar](50) NULL,
	[condicionVenta] [nvarchar](50) NULL,
	[formaPago] [nvarchar](50) NULL,
	[CAE] [nvarchar](50) NULL,
	[fecVtoCAE] [nvarchar](50) NULL,
	[importeNetoGravado] [float] NULL,
	[iva] [float] NULL,
	[importeTotal] [float] NULL,
	[idVenta] [int] NULL,
	[creado] [datetime] NULL,
	[error] [tinyint] NULL,
	[mensajeError] [nvarchar](max) NULL,
	[fechaError] [datetime] NULL,
	[cantErrores] [int] NOT NULL,
 CONSTRAINT [PK_FacturaElectronica] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FacturaElectronica] ADD  CONSTRAINT [DF_FacturaElectronica_fechaEmisionAfip]  DEFAULT (NULL) FOR [fechaEmisionAfip]
GO

ALTER TABLE [dbo].[FacturaElectronica] ADD  CONSTRAINT [DF_FacturaElectronica_fechaError]  DEFAULT (NULL) FOR [fechaError]
GO

ALTER TABLE [dbo].[FacturaElectronica] ADD  CONSTRAINT [DF_FacturaElectronica_cantErrores]  DEFAULT ((0)) FOR [cantErrores]
GO

