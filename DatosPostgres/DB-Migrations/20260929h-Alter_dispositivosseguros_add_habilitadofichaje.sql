-- Fichaje de jornada en dispositivos habilitados (2026-09-29, ver docs/DECISIONS.md): reutiliza el
-- modulo ya existente de Dispositivos Seguros (20260819-Create_dispositivosseguros.sql) en vez de
-- crear un concepto de dispositivo nuevo. HabilitadoFichaje es independiente de Bloqueado: un
-- dispositivo puede saltar el rate-limit de login sin estar habilitado para fichar, y viceversa.
--
-- Aditivo e idempotente (IF NOT EXISTS), mismo estilo que 20260928-Alter_corteporcompra_add_nocontado.sql.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.
ALTER TABLE dispositivosseguros ADD COLUMN IF NOT EXISTS habilitadofichaje boolean NOT NULL DEFAULT false;
