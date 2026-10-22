-- Eliminacion de Compras, Stock y Movimientos con motivo y clave, para SQL Server. Espejo de
-- DatosPostgres/DB-Migrations/20261022-Create_auditoriaeliminaciones.sql; ver docs/DECISIONS.md
-- "Eliminar compras, stock y movimientos".
-- Idempotente y aditivo. Sin sintaxis posterior a SQL Server 2008 (corre en ServidorSM / San Lorenzo): sin FOR JSON,
-- sin RLS, sin IF NOT EXISTS en CREATE TABLE. Se ejecuta sobre la base activa (sqlcmd -d SuperCerdo).
-- Correr ANTES de desplegar el codigo. Un registro eliminado SE BORRA de su tabla; esta auditoria conserva
-- quien lo elimino, cuando, el motivo y la foto de lo que tenia (cabecera + lineas). Append-only.

IF OBJECT_ID('dbo.AuditoriaEliminaciones', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditoriaEliminaciones]
    (
        [id]                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]         INT NOT NULL,
        [modulo]            VARCHAR(12) NOT NULL,
        [idRegistro]        INT NOT NULL,
        [tipoRegistro]      NVARCHAR(100) NULL,
        [fechaRegistro]     DATETIME NULL,
        [idSucursal]        INT NULL,
        [idSucursalDestino] INT NULL,
        [idPersona]         INT NULL,
        [nroRemito]         NVARCHAR(100) NULL,
        [enCtaCte]          BIT NULL,
        [cantMedias]        INT NULL,
        [kgsMedias]         FLOAT NULL,
        [importe]           FLOAT NULL,
        [totalKg]           FLOAT NULL,
        [cantLineas]        INT NOT NULL CONSTRAINT DF_AuditoriaEliminaciones_cantLineas DEFAULT (0),
        [observaciones]     NVARCHAR(MAX) NULL,
        [idUsuarioCreador]  INT NULL,
        [creado]            DATETIME NULL,
        [idUsuarioElimino]  INT NOT NULL,
        [idUsuarioSesion]   INT NOT NULL,
        [fechaEliminacion]  DATETIME NOT NULL CONSTRAINT DF_AuditoriaEliminaciones_fecha DEFAULT (GETDATE()),
        [motivo]            NVARCHAR(MAX) NOT NULL,
        [ip]                VARCHAR(60) NULL,
        [efectos]           NVARCHAR(MAX) NULL,
        CONSTRAINT CK_AuditoriaEliminaciones_modulo CHECK ([modulo] IN ('COMPRA', 'STOCK', 'MOVIMIENTO'))
    );
    CREATE INDEX IX_AuditoriaEliminaciones_registro ON [dbo].[AuditoriaEliminaciones] ([idEmpresa], [modulo], [idRegistro]);
    CREATE INDEX IX_AuditoriaEliminaciones_fecha ON [dbo].[AuditoriaEliminaciones] ([idEmpresa], [fechaEliminacion]);
END
GO

IF OBJECT_ID('dbo.AuditoriaEliminacionLineas', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditoriaEliminacionLineas]
    (
        [id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]   INT NOT NULL,
        [idAuditoria] INT NOT NULL,
        [idCorte]     INT NULL,
        [codigo]      BIGINT NULL,
        [producto]    NVARCHAR(MAX) NULL,
        [idSucursal]  INT NULL,
        [cantKg]      FLOAT NULL,
        [cantUnidad]  INT NULL,
        [precioKg]    FLOAT NULL,
        [detalle]     NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_AuditoriaEliminacionLineas_auditoria ON [dbo].[AuditoriaEliminacionLineas] ([idEmpresa], [idAuditoria]);
END
GO
