-- Formula secreta (2026-10-04, ver docs/DECISIONS.md "Formula secreta con re-login").
-- Espejo SQL Server de DatosPostgres/DB-Migrations/20261004b-...sql.
--   dbo.Formulas.Secreta : si es 1, los ingredientes/porcentajes solo se muestran en WebCore tras un
--                          re-login (usuario + clave) de alguien con Permisos.Elaborado.VerFormulas.
--   dbo.AuditoriaFormulas: auditoria append-only de accesos y cambios sobre formulas secretas.
-- Default 0: ninguna formula existente cambia de comportamiento. El parametro nuevo de
-- addOrEditFormula tambien tiene default 0 (no rompe callers viejos).
-- Cuerpo de addOrEditFormula tomado de 20260822-Alter_addOrEditFormula_agregarCortePorFormula_AjusteDeFormula.sql
-- (ultima version versionada). Verificar con sp_helptext contra la base real antes de aplicar, por si
-- el SP fue tocado a mano despues.

USE [carnisys]
GO

IF COL_LENGTH('dbo.Formulas', 'Secreta') IS NULL
BEGIN
    ALTER TABLE dbo.Formulas ADD Secreta BIT NOT NULL DEFAULT 0;
END
GO

IF OBJECT_ID('dbo.AuditoriaFormulas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditoriaFormulas (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Tipo            VARCHAR(20)  NOT NULL,   -- VER | OCULTAR | FALLO | MARCAR_SECRETA | QUITAR_SECRETA
        IdUsuario       INT          NULL,       -- quien se autentico en el step-up (quien ve la formula)
        IdUsuarioSesion INT          NULL,       -- usuario logueado que opera la pantalla
        IdCorte         INT          NULL,       -- producto elaborado (Formulas.idEmbutido guarda el idCorte)
        Ip              VARCHAR(64)  NULL,
        Fecha           DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
        Detalle         VARCHAR(300) NULL        -- nunca claves ni datos personales
    );
END
GO

ALTER PROCEDURE [dbo].[addOrEditFormula]
    @idFormula int = null,
    @idEmbutido int,
    @receta nvarchar(max),
    @creadoPor int = null,
    @actualizadoPor int = null,
    @ajustarUnidad bit = 0,
    @secreta bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @idFormula = 0
        BEGIN
            INSERT INTO [Formulas]
           ([idEmbutido]
           ,[receta]
           ,[creado]
           ,[creadoPor]
           ,[AjustarUnidad]
           ,[Secreta])
     VALUES
            (@idEmbutido
            ,@receta
           ,SYSDATETIME()
           ,@creadoPor
           ,@ajustarUnidad
           ,@secreta)

            set @idFormula = SCOPE_IDENTITY()
        END
    ELSE
        BEGIN
        UPDATE [Formulas]
           SET [idEmbutido] = @idEmbutido
              ,[receta] = @receta
              ,[actualizado] = SYSDATETIME()
              ,[actualizadoPor] = @actualizadoPor
              ,[AjustarUnidad] = @ajustarUnidad
              ,[Secreta] = @secreta
         WHERE idFormula = @idFormula

         --Se eliminan todo los Cortes en Formula Para volver a cargarlos
         delete from CortePorFormula where idFormula=@idFormula

        END

    select @idFormula

END
GO
