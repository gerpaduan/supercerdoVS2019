-- Login por CUIT, clave rapida (PIN) y bloqueo por origen (2026-10-02, ver docs/DECISIONS.md
-- "Login por CUIT, clave rapida (PIN) y politica de clave"). Espeja
-- Datos/DB-Procedures/20261002-Alter_Usuarios_LoginPorCuit_Pin.sql (SQL Server).
-- Todo aditivo y con default neutro: aplicar este script NO cambia el comportamiento del login
-- hasta que el codigo nuevo lo use. Correr como carnisys_admin (dueno de las tablas).
--
-- (a) Contadores/bloqueo por origen: los fallos desde dispositivo NO seguro llevan su propio
--     contador y bloqueo (solo bloquea logins desde dispositivos no seguros de ese usuario). Los
--     intentosfallidoslogin/bloqueado existentes pasan a contar solo fallos desde dispositivo
--     seguro (bloqueo de cuenta completa).
-- (b) Clave rapida (PIN) de 4-6 digitos, valida solo desde dispositivo seguro: hash PBKDF2 aparte
--     de la clave (nunca texto plano, nunca un prefijo de la clave).
ALTER TABLE usuarios
    ADD COLUMN IF NOT EXISTS intentosfallidosnoseguro integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS bloqueadonoseguro boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS fechabloqueonosegurautc timestamp,
    ADD COLUMN IF NOT EXISTS pinhash text NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS pinsalt text NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS pinhashiterations integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS pinupdatedatutc timestamp;
