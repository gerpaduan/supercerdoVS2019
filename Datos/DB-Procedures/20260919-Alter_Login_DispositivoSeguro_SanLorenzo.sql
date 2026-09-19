USE [SuperCerdo]
GO

-- Login solo desde dispositivos seguros (2026-09-19, ver docs/DECISIONS.md "Dispositivo seguro
-- como condicion de login"). Version San Lorenzo (base SuperCerdo, SQL Server 2008: sin sintaxis nueva). Verificar en vivo antes de ejecutar; aprobacion explicita del usuario.
-- Todo aditivo y con default apagado: aplicar este script NO cambia el comportamiento del login
-- hasta que un admin active el switch de empresa o el tilde por usuario.

IF COL_LENGTH('dbo.Usuarios', 'RequiereDispositivoSeguro') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD RequiereDispositivoSeguro BIT NOT NULL CONSTRAINT DF_Usuarios_RequiereDispositivoSeguro DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Empresas', 'ExigirDispositivoSeguro') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas
    ADD ExigirDispositivoSeguro BIT NOT NULL CONSTRAINT DF_Empresas_ExigirDispositivoSeguro DEFAULT (0);
END
GO

IF OBJECT_ID('dbo.DispositivosSeguros', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.DispositivosSeguros', 'Origen') IS NULL
        ALTER TABLE dbo.DispositivosSeguros
        ADD Origen NVARCHAR(20) NOT NULL CONSTRAINT DF_DispositivosSeguros_Origen DEFAULT ('Manual');

    IF COL_LENGTH('dbo.DispositivosSeguros', 'EmailAlta') IS NULL
        ALTER TABLE dbo.DispositivosSeguros
        ADD EmailAlta NVARCHAR(200) NULL;

    IF COL_LENGTH('dbo.DispositivosSeguros', 'Bloqueado') IS NULL
        ALTER TABLE dbo.DispositivosSeguros
        ADD Bloqueado BIT NOT NULL CONSTRAINT DF_DispositivosSeguros_Bloqueado DEFAULT (0);
END
GO

IF OBJECT_ID('dbo.LoginUbicacionLog', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.LoginUbicacionLog', 'IdDispositivoSeguro') IS NULL
        ALTER TABLE dbo.LoginUbicacionLog
        ADD IdDispositivoSeguro INT NULL;

    IF COL_LENGTH('dbo.LoginUbicacionLog', 'Dispositivo') IS NULL
        ALTER TABLE dbo.LoginUbicacionLog
        ADD Dispositivo NVARCHAR(200) NULL;
END
GO
