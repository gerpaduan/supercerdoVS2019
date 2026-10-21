-- Usuario que hizo la ultima modificacion de un producto (corte) (2026-10-21, ver docs/DECISIONS.md).
--  - actualizadopor: id de usuarios del ultimo que cambio el producto (edicion completa o cambio de precio, individual
--    o por seleccion). NULL = no se sabe (productos nunca modificados, o precio cambiado por un proceso sin usuario
--    como la carga de una compra). La fecha de esa modificacion ya vive en corte.actualizado.
-- Sin FK, igual que el resto de las columnas *por de la base. Aditiva, nullable e idempotente. Base unica multi-tenant
-- (RLS por idempresa): una sola corrida cubre a todas las empresas; los GRANT de la tabla corte se heredan.
-- ORDEN DE DEPLOY: correr ESTA migracion ANTES de publicar el codigo nuevo (CortePg.editPrecioCorte y addOrEditCorte ya
-- escriben la columna). Correr conectado a 'carnisys' con el rol carnisys_admin.
-- SQL Server (WinForms de produccion): equivalente en Datos/DB-Procedures/20261021-Alter_Corte_add_ActualizadoPor.sql (misma columna, nullable; verificar antes de correrlo en SuperCerdo).

ALTER TABLE corte ADD COLUMN IF NOT EXISTS actualizadopor integer NULL;
