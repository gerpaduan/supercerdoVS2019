USE [CarniSys]
GO

-- Marca un usuario como "de produccion": compartido en la sala de produccion (Movimientos,
-- Stock, Elaborados), sin acceso a Ventas ni Finanzas. Ver Web/Controllers/UsuariosController.cs
-- (validacion Admin/Produccion mutuamente excluyentes) y BaseController.ResolverUsuarioCreador
-- (quien queda como creador real de cada operacion no es este usuario, sino el que se elige del
-- modal de seleccion sin password).
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'esUsuarioProduccion'
)
BEGIN
    ALTER TABLE [dbo].[Usuarios]
    ADD [esUsuarioProduccion] BIT NOT NULL CONSTRAINT DF_Usuarios_esUsuarioProduccion DEFAULT (0)
END
GO
