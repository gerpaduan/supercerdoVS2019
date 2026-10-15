-- Bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"): minutos sin
-- actividad tras los cuales la web avisa "Extender sesion" y luego cierra la sesion (en el POS
-- bloquea con clave). 0 = nunca. Los usuarios de produccion quedan exentos por codigo, sin importar
-- este valor. DEFAULT 60: todos los usuarios existentes pasan a 60 al aplicar la migracion.
--
-- Aditivo e idempotente (IF NOT EXISTS). Espeja Datos/DB-Procedures/20261015-Alter_Usuarios_add_MinutosInactividad.sql.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS minutosinactividad integer NOT NULL DEFAULT 60;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'usuarios_minutosinactividad_ck'
    ) THEN
        ALTER TABLE usuarios
            ADD CONSTRAINT usuarios_minutosinactividad_ck CHECK (minutosinactividad >= 0);
    END IF;
END $$;
