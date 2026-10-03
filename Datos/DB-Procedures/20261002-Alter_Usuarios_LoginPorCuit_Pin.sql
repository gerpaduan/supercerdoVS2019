-- Login por CUIT, clave rapida (PIN) y bloqueo por origen (2026-10-02, ver docs/DECISIONS.md
-- "Login por CUIT, clave rapida (PIN) y politica de clave"). Espeja
-- DatosPostgres/DB-Migrations/20261002-Alter_usuarios_login_por_cuit_pin.sql.
--
-- SIN linea USE: ejecutar sobre la base correcta (dev local: CarniSys; SM y San Lorenzo:
-- SuperCerdo, SQL Server 2008 RTM -- este script no usa sintaxis posterior a 2008, salvo
-- DATETIME2/SYSUTCDATETIME que ya usa Usuarios.fechaBloqueoUtc en esas bases). Verificar en vivo
-- que las columnas faltan antes de ejecutar y pedir aprobacion explicita por servidor.
--
-- Todo aditivo y con default neutro: aplicar este script NO cambia el comportamiento del login
-- hasta que el codigo nuevo lo use. WinForms/clasico no conocen estas columnas ni las tocan.
--
-- (a) Contadores/bloqueo por origen: los fallos desde dispositivo NO seguro llevan su propio
--     contador y bloqueo. intentosFallidosLogin/bloqueado existentes pasan a contar solo fallos
--     desde dispositivo seguro (bloqueo de cuenta completa).
-- (b) Clave rapida (PIN) de 4-6 digitos, valida solo desde dispositivo seguro: hash PBKDF2 aparte
--     de la clave (nunca texto plano, nunca un prefijo de la clave).

IF COL_LENGTH('dbo.Usuarios', 'intentosFallidosNoSeguro') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD intentosFallidosNoSeguro INT NOT NULL CONSTRAINT DF_Usuarios_intentosFallidosNoSeguro DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Usuarios', 'bloqueadoNoSeguro') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD bloqueadoNoSeguro BIT NOT NULL CONSTRAINT DF_Usuarios_bloqueadoNoSeguro DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Usuarios', 'fechaBloqueoNoSeguroUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD fechaBloqueoNoSeguroUtc DATETIME2 NULL;
END
GO

IF COL_LENGTH('dbo.Usuarios', 'pinHash') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD pinHash NVARCHAR(256) NOT NULL CONSTRAINT DF_Usuarios_pinHash DEFAULT ('');
END
GO

IF COL_LENGTH('dbo.Usuarios', 'pinSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD pinSalt NVARCHAR(256) NOT NULL CONSTRAINT DF_Usuarios_pinSalt DEFAULT ('');
END
GO

IF COL_LENGTH('dbo.Usuarios', 'pinHashIterations') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD pinHashIterations INT NOT NULL CONSTRAINT DF_Usuarios_pinHashIterations DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Usuarios', 'pinUpdatedAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD pinUpdatedAtUtc DATETIME2 NULL;
END
GO
