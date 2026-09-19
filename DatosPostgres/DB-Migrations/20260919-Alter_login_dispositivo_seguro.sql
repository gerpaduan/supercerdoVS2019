-- Login solo desde dispositivos seguros (2026-09-19, ver docs/DECISIONS.md "Dispositivo seguro
-- como condicion de login"). Espeja Datos/DB-Procedures/20260919-Alter_Login_DispositivoSeguro.sql.
-- Aditivo y con default apagado: no cambia el comportamiento del login hasta que un admin active
-- el switch de empresa o el tilde por usuario. Correr como carnisys_admin (dueno de las tablas;
-- carnisys_user solo tiene SELECT/INSERT en loginubicacionlog).
ALTER TABLE usuarios
    ADD COLUMN requieredispositivoseguro boolean NOT NULL DEFAULT false;

ALTER TABLE empresas
    ADD COLUMN exigirdispositivoseguro boolean NOT NULL DEFAULT false;

ALTER TABLE dispositivosseguros
    ADD COLUMN origen text NOT NULL DEFAULT 'Manual',
    ADD COLUMN emailalta text,
    ADD COLUMN bloqueado boolean NOT NULL DEFAULT false;

ALTER TABLE loginubicacionlog
    ADD COLUMN iddispositivoseguro integer,
    ADD COLUMN dispositivo text;
