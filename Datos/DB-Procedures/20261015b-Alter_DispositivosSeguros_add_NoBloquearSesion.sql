-- Bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"): mismo
-- agregado que DatosPostgres/DB-Migrations/20261015b-Alter_dispositivosseguros_add_nobloquearsesion.sql.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.DispositivosSeguros') AND name = 'NoBloquearSesion'
)
BEGIN
    ALTER TABLE dbo.DispositivosSeguros ADD NoBloquearSesion BIT NOT NULL DEFAULT 0;
END
