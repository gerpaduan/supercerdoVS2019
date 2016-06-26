USE [SuperCerdo]
GO

EXEC sp_rename 'CortePorCompra', 'CortePorCompra_1';  

/****** Object:  Table [dbo].[CortePorCompra]    Script Date: 06/24/2016 20:09:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CortePorCompra](
	[idCortePorCompra] [int] IDENTITY(1,1) NOT NULL,
	[idCompra] [int] NULL,
	[idCorte] [int] NULL,
	[idSucursal] [int] NULL,
	[precioKg] [float] NULL,
	[cantKg] [float] NULL,	
	[creado] [datetime] NULL,
	[creadoPor] [int] NULL,
 CONSTRAINT [PK_CortePorCompra] PRIMARY KEY CLUSTERED 
(
	[idCortePorCompra] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


INSERT INTO CortePorCompra  (idCompra, idCorte, idSucursal, precioKg, cantKg)
       SELECT idCompra, idCorte, idSucursal, precioKg, cantKg
       FROM CortePorCompra_1;
