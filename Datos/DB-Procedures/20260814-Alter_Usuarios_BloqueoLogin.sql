USE [CarniSys]
GO

-- Bloqueo de cuenta tras N intentos fallidos de contraseña (ver Security:AccountLockoutMaxAttempts
-- en Web.config, default 5). Distinto de LoginRateLimiter (Web/Helpers/LoginRateLimiter.cs), que es
-- un limite en memoria por IP y se auto-desbloquea por tiempo -- esto es persistente, por cuenta, y
-- requiere desbloqueo explicito (link por email o un admin, ver LoginController.UnlockAccount y
-- UsuariosController.DesbloquearUsuario).
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'intentosFallidosLogin')
BEGIN
    ALTER TABLE [dbo].[Usuarios]
    ADD [intentosFallidosLogin] INT NOT NULL CONSTRAINT DF_Usuarios_intentosFallidosLogin DEFAULT (0)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'bloqueado')
BEGIN
    ALTER TABLE [dbo].[Usuarios]
    ADD [bloqueado] BIT NOT NULL CONSTRAINT DF_Usuarios_bloqueado DEFAULT (0)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'fechaBloqueoUtc')
BEGIN
    ALTER TABLE [dbo].[Usuarios]
    ADD [fechaBloqueoUtc] DATETIME2 NULL
END
GO
