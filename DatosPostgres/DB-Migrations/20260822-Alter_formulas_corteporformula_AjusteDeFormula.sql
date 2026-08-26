-- Espejo Postgres de Datos/DB-Procedures/20260822-Alter_Formulas_CortePorFormula_AjusteDeFormula.sql
-- (SQL Server) -- mismo criterio: ver ese archivo y docs/DECISIONS.md (2026-08-22) para el porque.

ALTER TABLE formulas ADD COLUMN IF NOT EXISTS ajustarunidad boolean NOT NULL DEFAULT false;
ALTER TABLE corteporformula ADD COLUMN IF NOT EXISTS nosumapeso boolean NOT NULL DEFAULT false;
