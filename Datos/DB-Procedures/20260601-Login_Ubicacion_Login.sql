IF COL_LENGTH('dbo.Sucursal', 'Latitud') IS NULL
BEGIN
    ALTER TABLE dbo.Sucursal
    ADD Latitud DECIMAL(10, 7) NULL;
END
GO

IF COL_LENGTH('dbo.Sucursal', 'Longitud') IS NULL
BEGIN
    ALTER TABLE dbo.Sucursal
    ADD Longitud DECIMAL(10, 7) NULL;
END
GO

IF COL_LENGTH('dbo.Sucursal', 'RadioLoginMetros') IS NULL
BEGIN
    ALTER TABLE dbo.Sucursal
    ADD RadioLoginMetros INT NOT NULL
        CONSTRAINT DF_Sucursal_RadioLoginMetros DEFAULT (200);
END
GO

IF COL_LENGTH('dbo.Sucursal', 'ValidarUbicacionLogin') IS NULL
BEGIN
    ALTER TABLE dbo.Sucursal
    ADD ValidarUbicacionLogin BIT NOT NULL
        CONSTRAINT DF_Sucursal_ValidarUbicacionLogin DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Usuarios', 'PermitirLoginFueraSucursal') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD PermitirLoginFueraSucursal BIT NOT NULL
        CONSTRAINT DF_Usuarios_PermitirLoginFueraSucursal DEFAULT (0);
END
GO

IF OBJECT_ID('dbo.LoginUbicacionLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginUbicacionLog
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdUsuario INT NOT NULL,
        IdSucursal INT NOT NULL,
        FechaHora DATETIME NOT NULL,
        Latitud DECIMAL(10, 7) NULL,
        Longitud DECIMAL(10, 7) NULL,
        PrecisionMetros DECIMAL(10, 2) NULL,
        DistanciaMetros DECIMAL(10, 2) NULL,
        Permitido BIT NOT NULL,
        Motivo NVARCHAR(300) NOT NULL,
        Ip NVARCHAR(100) NOT NULL
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.LoginUbicacionLog')
      AND name = 'IX_LoginUbicacionLog_IdUsuario_FechaHora')
BEGIN
    CREATE INDEX IX_LoginUbicacionLog_IdUsuario_FechaHora
        ON dbo.LoginUbicacionLog (IdUsuario, FechaHora DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.LoginUbicacionLog')
      AND name = 'IX_LoginUbicacionLog_IdSucursal_FechaHora')
BEGIN
    CREATE INDEX IX_LoginUbicacionLog_IdSucursal_FechaHora
        ON dbo.LoginUbicacionLog (IdSucursal, FechaHora DESC);
END
GO
