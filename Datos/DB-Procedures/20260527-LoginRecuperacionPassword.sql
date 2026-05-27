IF COL_LENGTH('dbo.Usuarios', 'passwordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD passwordHash NVARCHAR(256) NULL;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'passwordSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD passwordSalt NVARCHAR(256) NULL;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'passwordHashIterations') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD passwordHashIterations INT NULL;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'passwordUpdatedAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD passwordUpdatedAtUtc DATETIME2(0) NULL;
END
GO

IF OBJECT_ID('dbo.UsuarioPasswordResetTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UsuarioPasswordResetTokens
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idUsuario INT NOT NULL,
        idEmpresa INT NOT NULL,
        tokenHash NVARCHAR(128) NOT NULL,
        fechaCreacionUtc DATETIME2(0) NOT NULL,
        fechaExpiracionUtc DATETIME2(0) NOT NULL,
        usado BIT NOT NULL CONSTRAINT DF_UsuarioPasswordResetTokens_Usado DEFAULT(0),
        fechaUsoUtc DATETIME2(0) NULL,
        identificadorSolicitado NVARCHAR(120) NULL,
        emailDestino NVARCHAR(120) NULL
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_UsuarioPasswordResetTokens_TokenHash'
      AND object_id = OBJECT_ID('dbo.UsuarioPasswordResetTokens')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_UsuarioPasswordResetTokens_TokenHash
        ON dbo.UsuarioPasswordResetTokens(tokenHash);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_UsuarioPasswordResetTokens_UsuarioEstado'
      AND object_id = OBJECT_ID('dbo.UsuarioPasswordResetTokens')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_UsuarioPasswordResetTokens_UsuarioEstado
        ON dbo.UsuarioPasswordResetTokens(idUsuario, usado, fechaExpiracionUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_UsuarioPasswordResetTokens_Usuarios'
)
BEGIN
    ALTER TABLE dbo.UsuarioPasswordResetTokens
    ADD CONSTRAINT FK_UsuarioPasswordResetTokens_Usuarios
        FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(id);
END
GO
