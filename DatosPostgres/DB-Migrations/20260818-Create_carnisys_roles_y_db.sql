-- Roles y base para el piloto de la migracion (Etapa 2). Correr conectado a la base
-- 'postgres' con el superusuario. Ver docs/06-datos-e-integraciones/rls-postgres.md.

CREATE ROLE carnisys_admin LOGIN PASSWORD 'ADMIN_PWD_PLACEHOLDER';
CREATE ROLE carnisys_user  LOGIN PASSWORD 'USER_PWD_PLACEHOLDER';
CREATE ROLE cs_admin_pg    LOGIN PASSWORD 'ADMIN_PWD_PLACEHOLDER' BYPASSRLS;

CREATE DATABASE carnisys OWNER carnisys_admin;
