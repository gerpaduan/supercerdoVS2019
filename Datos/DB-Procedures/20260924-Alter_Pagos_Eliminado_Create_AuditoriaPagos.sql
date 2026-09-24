-- Pagos/cobros: eliminacion logica (con contra-asiento en cta cte) y auditoria de cambios de persona
-- para SQL Server. Espejo de DatosPostgres/DB-Migrations/20260924-Alter_pagos_eliminado_create_auditoriapagos.sql;
-- ver docs/DECISIONS.md "Pagos: listado, cambio de persona y eliminacion".
-- Idempotente y aditivo. Sin sintaxis posterior a SQL Server 2008 (corre en ServidorSM / San Lorenzo).
-- Se ejecuta sobre la base activa (sqlcmd -d SuperCerdo). Correr ANTES de desplegar el codigo.
-- Un pago eliminado NUNCA se borra: queda con eliminado = 1 y su asiento opuesto en MovCtaCte.
-- Las notificaciones al admin (campana) no existen todavia en SQL Server: ahi la advertencia es solo en pantalla.

IF COL_LENGTH('dbo.Pagos', 'eliminado') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD [eliminado] BIT NOT NULL CONSTRAINT DF_Pagos_eliminado DEFAULT (0);
GO
IF COL_LENGTH('dbo.Pagos', 'eliminadoPor') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD [eliminadoPor] INT NULL;
GO
IF COL_LENGTH('dbo.Pagos', 'fechaEliminacion') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD [fechaEliminacion] DATETIME NULL;
GO
IF COL_LENGTH('dbo.Pagos', 'motivoEliminacion') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD [motivoEliminacion] NVARCHAR(MAX) NULL;
GO

-- Auditoria append-only de un pago: CAMBIO_PERSONA y ELIMINACION.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditoriaPagos')
BEGIN
    CREATE TABLE [dbo].[AuditoriaPagos]
    (
        [id]                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]         INT NOT NULL,
        [idPago]            INT NOT NULL,
        [tipo]              VARCHAR(20) NOT NULL,
        [idPersonaAnterior] INT NULL,
        [idPersonaNueva]    INT NULL,
        [idUsuario]         INT NOT NULL,
        [fecha]             DATETIME NOT NULL CONSTRAINT DF_AuditoriaPagos_fecha DEFAULT (GETDATE()),
        [detalle]           NVARCHAR(MAX) NULL,
        CONSTRAINT CK_AuditoriaPagos_tipo CHECK ([tipo] IN ('CAMBIO_PERSONA', 'ELIMINACION'))
    );
    CREATE INDEX IX_AuditoriaPagos_pago ON [dbo].[AuditoriaPagos] ([idEmpresa], [idPago], [fecha]);
END
GO
