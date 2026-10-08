-- Formula secreta (2026-10-04, ver docs/DECISIONS.md "Formula secreta con re-login").
-- Espejo SQL Server de DatosPostgres/DB-Migrations/20261004b-...sql.
--   dbo.Formulas.Secreta : si es 1, los ingredientes/porcentajes solo se muestran en WebCore tras un
--                          re-login (usuario + clave) de alguien con Permisos.Elaborado.VerFormulas.
--   dbo.AuditoriaFormulas: auditoria append-only de accesos y cambios sobre formulas secretas.
-- Default 0: ninguna formula existente cambia de comportamiento. El parametro nuevo de
-- addOrEditFormula tiene default NULL = "no tocar la marca": WinForms (que no manda @secreta) al
-- editar una formula CONSERVA el valor que tenia (si fuera 0 la borraria en silencio). Al insertar
-- sin el parametro queda en 0. Decision del usuario 2026-10-05: WinForm no toma el valor de secreta.
-- Cuerpo de addOrEditFormula tomado de 20260822-Alter_addOrEditFormula_agregarCortePorFormula_AjusteDeFormula.sql
-- (ultima version versionada). Verificar con sp_helptext contra la base real antes de aplicar, por si
-- el SP fue tocado a mano despues.

-- SIN "USE": el script corre sobre la base activa (sqlcmd -d SuperCerdo ..., o la base elegida en SSMS).
-- Antes tenia "USE [carnisys]": en SM y San Lorenzo (base SuperCerdo, WinForms en produccion) aplicaba
-- los cambios en OTRA base o fallaba. Guarda: solo sigue si la base activa tiene Formulas y el
-- addOrEditFormula conocido (version 20260822: @ajustarUnidad y CortePorFormula). Si no, avisa y
-- salta TODO lo que sigue (SET NOEXEC ON) sin modificar nada. Es re-ejecutable.
IF OBJECT_ID('dbo.Formulas', 'U') IS NULL
   OR OBJECT_ID('dbo.addOrEditFormula', 'P') IS NULL
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditFormula')), '') NOT LIKE '%@ajustarUnidad%'
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditFormula')), '') NOT LIKE '%CortePorFormula%'
BEGIN
    RAISERROR('Base equivocada o addOrEditFormula distinto del esperado: no se modifico nada. Correr con sqlcmd -d <base> y revisar sp_helptext addOrEditFormula.', 16, 1);
    SET NOEXEC ON;
END
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
    @secreta bit = NULL
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
           ,ISNULL(@secreta, 0))

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
              ,[Secreta] = ISNULL(@secreta, [Secreta])
         WHERE idFormula = @idFormula

         --Se eliminan todo los Cortes en Formula Para volver a cargarlos
         delete from CortePorFormula where idFormula=@idFormula

        END

    select @idFormula

END
GO

-- Deja la sesion usable si la guarda salteo el script (SSMS conserva NOEXEC entre ejecuciones).
SET NOEXEC OFF
GO
