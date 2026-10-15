-- Bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"): mismo
-- agregado que DatosPostgres/DB-Migrations/20261015-Alter_usuarios_add_minutosinactividad.sql, para
-- que Datos/Usuario.cs (rama SQL Server) no rompa. 0 = nunca; DEFAULT 60 para los existentes.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'MinutosInactividad'
)
BEGIN
    ALTER TABLE dbo.Usuarios ADD MinutosInactividad INT NOT NULL
        CONSTRAINT DF_Usuarios_MinutosInactividad DEFAULT (60);
    ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_MinutosInactividad CHECK (MinutosInactividad >= 0);
END
