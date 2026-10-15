-- Bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"): un
-- dispositivo seguro puede marcarse "No bloquear sesion" (ej. la PC propia de un administrador).
-- Por defecto false = la sesion se bloquea. Se evalua al iniciar sesion y viaja como claim.
--
-- Aditivo e idempotente (IF NOT EXISTS). Espeja Datos/DB-Procedures/20261015b-Alter_DispositivosSeguros_add_NoBloquearSesion.sql.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.
ALTER TABLE dispositivosseguros ADD COLUMN IF NOT EXISTS nobloquearsesion boolean NOT NULL DEFAULT false;
