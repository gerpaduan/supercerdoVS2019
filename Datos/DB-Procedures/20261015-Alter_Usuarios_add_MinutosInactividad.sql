-- Bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"): mismo
-- agregado que DatosPostgres/DB-Migrations/20261015-Alter_usuarios_add_minutosinactividad.sql, para
-- que Datos/Usuario.cs (rama SQL Server) no rompa. 0 = nunca; DEFAULT 60 para los existentes.
-- El CHECK va en la misma definicion de la columna: un ALTER TABLE ADD CONSTRAINT que nombra una
-- columna agregada en el mismo lote falla al compilar el lote ("Invalid column name").
-- Compatible con SQL Server 2008 (San Lorenzo).
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'MinutosInactividad'
)
BEGIN
    ALTER TABLE dbo.Usuarios ADD MinutosInactividad INT NOT NULL
        CONSTRAINT DF_Usuarios_MinutosInactividad DEFAULT (60)
        CONSTRAINT CK_Usuarios_MinutosInactividad CHECK (MinutosInactividad >= 0);
END
