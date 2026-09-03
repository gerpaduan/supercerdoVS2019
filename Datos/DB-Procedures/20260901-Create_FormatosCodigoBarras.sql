USE [CarniSys]
GO

-- Motor configurable de interpretacion de codigos de barra internos (EAN-13, prefijo interno
-- de balanza 20-29) por empresa. Un solo formato activo por (IdEmpresa, Prefijo) -- UNIQUE
-- real, replicado tambien en Postgres (a diferencia del gap conocido en DispositivosSeguros,
-- donde el UNIQUE de SQL Server no se replico como UNIQUE real alla). Ver Negocio/
-- BarcodeInterpreter.cs y Negocio/FormatoCodigoBarras.cs.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FormatosCodigoBarras')
BEGIN
    CREATE TABLE [dbo].[FormatosCodigoBarras]
    (
        [Id]                    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdEmpresa]             INT NOT NULL,
        [Nombre]                NVARCHAR(100) NOT NULL,
        [Prefijo]               SMALLINT NOT NULL,
        [LongitudTotal]         SMALLINT NOT NULL,
        [PosicionCodigo]        SMALLINT NOT NULL,
        [LongitudCodigo]        SMALLINT NOT NULL,
        [PosicionValor]         SMALLINT NOT NULL,
        [LongitudValor]         SMALLINT NOT NULL,
        [TipoValor]             NVARCHAR(20) NOT NULL,
        [CantidadDecimales]     SMALLINT NOT NULL CONSTRAINT DF_FormatosCodigoBarras_CantidadDecimales DEFAULT (0),
        [Activo]                BIT NOT NULL CONSTRAINT DF_FormatosCodigoBarras_Activo DEFAULT (1),
        [Prioridad]             INT NOT NULL CONSTRAINT DF_FormatosCodigoBarras_Prioridad DEFAULT (0),
        [CreadoUtc]             DATETIME2 NOT NULL,
        [IdUsuarioCreador]      INT NULL,
        [ModificadoUtc]         DATETIME2 NULL,
        [IdUsuarioModificador]  INT NULL,
        CONSTRAINT [UQ_FormatosCodigoBarras_Empresa_Prefijo] UNIQUE ([IdEmpresa], [Prefijo]),
        CONSTRAINT [CK_FormatosCodigoBarras_Prefijo] CHECK ([Prefijo] BETWEEN 20 AND 29),
        CONSTRAINT [CK_FormatosCodigoBarras_TipoValor] CHECK ([TipoValor] IN (N'Precio', N'Cantidad'))
    )
END
GO
