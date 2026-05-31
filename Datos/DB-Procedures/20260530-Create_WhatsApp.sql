IF OBJECT_ID('dbo.ConfiguracionWhatsApp', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConfiguracionWhatsApp
    (
        IdConfiguracionWhatsApp INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdEmpresa INT NOT NULL,
        Activo BIT NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_Activo DEFAULT (0),
        MetaApiVersion NVARCHAR(20) NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_MetaApiVersion DEFAULT ('v22.0'),
        PhoneNumberId NVARCHAR(100) NULL,
        BusinessAccountId NVARCHAR(100) NULL,
        AccessToken NVARCHAR(500) NULL,
        FechaAlta DATETIME NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_FechaAlta DEFAULT (GETDATE()),
        FechaModificacion DATETIME NOT NULL CONSTRAINT DF_ConfiguracionWhatsApp_FechaModificacion DEFAULT (GETDATE()),
        IdUsuarioModificacion INT NULL
    );

    CREATE UNIQUE INDEX UX_ConfiguracionWhatsApp_IdEmpresa
        ON dbo.ConfiguracionWhatsApp(IdEmpresa);
END;
GO

IF OBJECT_ID('dbo.WhatsAppEnvios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WhatsAppEnvios
    (
        IdWhatsAppEnvio INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdEmpresa INT NOT NULL,
        IdVenta INT NULL,
        IdPersona INT NULL,
        TelefonoOriginal NVARCHAR(50) NULL,
        TelefonoFormateado NVARCHAR(50) NULL,
        NombreArchivo NVARCHAR(255) NULL,
        MediaId NVARCHAR(100) NULL,
        Estado NVARCHAR(50) NULL,
        Exito BIT NOT NULL CONSTRAINT DF_WhatsAppEnvios_Exito DEFAULT (0),
        MensajeError NVARCHAR(1000) NULL,
        RespuestaApi NVARCHAR(MAX) NULL,
        FechaAlta DATETIME NOT NULL CONSTRAINT DF_WhatsAppEnvios_FechaAlta DEFAULT (GETDATE()),
        IdUsuarioAlta INT NULL
    );

    CREATE INDEX IX_WhatsAppEnvios_IdEmpresa_FechaAlta
        ON dbo.WhatsAppEnvios(IdEmpresa, FechaAlta DESC);
END;
GO
