-- Agrega el soporte de "Ajuste de Formula" (modos A/B) para formulas que no son de Ingreso
-- Rapido. Ver docs/DECISIONS.md (2026-08-22) y Negocio/Corte.cs (NormalizarFormulaElaborado).
--
-- AjustarUnidad (dbo.Formulas): interruptor 2 del EditarFormula.cshtml -- si esta activo (o si
-- el elaborado es de Ingreso Rapido, comportamiento preexistente sin cambios), la fila de ajuste
-- (producto codigo -1) se calcula para que la formula sume exactamente 100% (Modo A).
--
-- NoSumaPeso (dbo.CortePorFormula): marca por-ingrediente (Modo B, solo aplica si AjustarUnidad
-- esta desactivado) -- ingredientes tildados se restan de la fila de ajuste, sin que la formula
-- tenga que sumar 100% (caso de uso: tripa en una formula de chorizo, que se carga por cantidad,
-- no aporta al peso del producto elaborado).
--
-- Ambas columnas con DEFAULT 0 para no romper callers/filas existentes.

USE [carnisys]
GO

IF COL_LENGTH('dbo.Formulas', 'AjustarUnidad') IS NULL
BEGIN
    ALTER TABLE dbo.Formulas ADD AjustarUnidad BIT NOT NULL DEFAULT 0;
END
GO

IF COL_LENGTH('dbo.CortePorFormula', 'NoSumaPeso') IS NULL
BEGIN
    ALTER TABLE dbo.CortePorFormula ADD NoSumaPeso BIT NOT NULL DEFAULT 0;
END
GO
