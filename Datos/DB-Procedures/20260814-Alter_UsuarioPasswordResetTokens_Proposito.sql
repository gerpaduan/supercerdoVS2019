USE [CarniSys]
GO

-- Generaliza la tabla de tokens de un solo uso (creada en 20260527-LoginRecuperacionPassword.sql
-- para recuperacion de contraseña) para que sirva tambien para el link de desbloqueo de cuenta
-- (LoginController.UnlockAccount): mismo mecanismo de token hasheado + expiracion + un solo uso,
-- discriminado por proposito para que un token de un flujo no sirva para el otro. El default
-- 'reset' cubre los tokens ya emitidos sin tocarlos.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UsuarioPasswordResetTokens') AND name = 'proposito')
BEGIN
    ALTER TABLE [dbo].[UsuarioPasswordResetTokens]
    ADD [proposito] NVARCHAR(20) NOT NULL CONSTRAINT DF_UsuarioPasswordResetTokens_proposito DEFAULT ('reset')
END
GO
