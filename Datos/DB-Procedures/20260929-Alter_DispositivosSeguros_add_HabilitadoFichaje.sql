-- Fichaje de jornada en dispositivos habilitados (2026-09-29, ver docs/DECISIONS.md): mismo agregado
-- que 20260929h-Alter_dispositivosseguros_add_habilitadofichaje.sql del lado Postgres, para que
-- Datos/DispositivoSeguro.cs (rama SQL Server, todavia vigente para DataEngine=SqlServer) no rompa.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DispositivosSeguros') AND name = 'HabilitadoFichaje'
)
BEGIN
    ALTER TABLE dbo.DispositivosSeguros ADD HabilitadoFichaje BIT NOT NULL DEFAULT 0;
END
