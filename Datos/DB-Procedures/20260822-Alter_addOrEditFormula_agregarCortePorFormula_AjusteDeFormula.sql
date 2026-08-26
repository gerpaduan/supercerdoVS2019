-- Agrega los parametros nuevos de "Ajuste de Formula" (ver migracion de columnas en
-- 20260822-Alter_Formulas_CortePorFormula_AjusteDeFormula.sql) a los 2 SPs que ya persistian
-- Formulas/CortePorFormula. Cuerpo original obtenido con sp_helptext contra la base real antes
-- de tocarlo (no se inventa la firma). Ambos parametros nuevos con default 0, no rompe callers
-- viejos que no los pasen.

USE [carnisys]
GO

ALTER PROCEDURE [dbo].[addOrEditFormula]
    @idFormula int = null,
    @idEmbutido int,
    @receta nvarchar(max),
    @creadoPor int = null,
    @actualizadoPor int = null,
    @ajustarUnidad bit = 0
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

     -- Insert statements for procedure here
    IF @idFormula = 0
        BEGIN
            INSERT INTO [Formulas]
           ([idEmbutido]
           ,[receta]
           ,[creado]
           ,[creadoPor]
           ,[AjustarUnidad])
     VALUES
            (@idEmbutido
            ,@receta
           ,SYSDATETIME()
           ,@creadoPor
           ,@ajustarUnidad)

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
         WHERE idFormula = @idFormula

         --Se eliminan todo los Cortes en Formula Para volver a cargarlos
         delete from CortePorFormula where idFormula=@idFormula

        END

    select @idFormula

END
GO

ALTER PROCEDURE [dbo].[agregarCortePorFormula]
    -- Add the parameters for the stored procedure here
    @idFormula int,
    @idCorte int,
    @porcentaje float,
    @agregarAuto bit,
    @noSumaPeso bit = 0
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    -- Insert statements for procedure here
     INSERT INTO  [CortePorFormula]
            ([idFormula]
           ,[idCorte]
           ,[porcentaje]
           ,[agregarAuto]
           ,[NoSumaPeso])
     VALUES
           (@idFormula,
            @idCorte,
            @porcentaje,
            @agregarAuto,
            @noSumaPeso )
END
GO
