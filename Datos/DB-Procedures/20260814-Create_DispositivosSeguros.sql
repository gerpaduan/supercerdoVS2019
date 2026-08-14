USE [CarniSys]
GO

-- Dispositivos (PCs) marcados como seguros por empresa: loguearse desde uno de ellos salta el
-- bloqueo por IP del login (Web/Helpers/LoginRateLimiter.cs), pero NO el bloqueo persistente por
-- cuenta (Usuario.Bloqueado, 5 errores de contraseña) -- ese sigue aplicando igual. El numero de
-- serie es el CPU ID via WMI (Win32_Processor.ProcessorId), mismo mecanismo ya usado en WinForms
-- (Utilidades/Util_Form.cs, GetCPUId()). Riesgo aceptado explicitamente por el usuario: el valor
-- viaja como campo de formulario comun en el POST de login, sin atadura criptografica al
-- dispositivo real en ese momento -- ver docs/DECISIONS.md.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DispositivosSeguros')
BEGIN
    CREATE TABLE [dbo].[DispositivosSeguros]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdEmpresa] INT NOT NULL,
        [NumeroSerie] NVARCHAR(200) NOT NULL,
        [Descripcion] NVARCHAR(200) NULL,
        [CreadoUtc] DATETIME2 NOT NULL,
        [IdUsuarioCreador] INT NULL,
        CONSTRAINT [UQ_DispositivosSeguros_Empresa_Serie] UNIQUE ([IdEmpresa], [NumeroSerie])
    )
END
GO
