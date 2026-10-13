-- Topes de un titular compartido entre empresas (2026-10-12, ver docs/DECISIONS.md "Titular fiscal compartido entre empresas").
-- El tope real de una persona es sobre TODO lo que factura su CUIT, en todas las empresas donde es titular. Para informarlo
-- (solo lectura, solo un total por empresa) la consulta corre con el rol de super-admin (SET LOCAL ROLE carnisys_sysadmin_bypass:
-- BYPASSRLS no exime de los GRANT de objeto, asi que hace falta este GRANT explicito).
-- Idempotente. Correr conectado a 'carnisys' con el rol carnisys_admin (dueno de las tablas). Va ANTES que el codigo nuevo.
GRANT SELECT ON facturaelectronica TO carnisys_sysadmin_bypass;
