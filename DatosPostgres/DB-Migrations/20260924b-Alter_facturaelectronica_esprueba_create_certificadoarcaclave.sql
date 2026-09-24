-- Homologacion AFIP + clave del certificado (2026-09-24, ver docs/DECISIONS.md "Certificado ARCA").
-- Correr como carnisys_admin (dueno de las tablas), ANTES de desplegar el codigo.

-- 1) Facturas emitidas en homologacion (modo prueba): quedan marcadas y se pueden filtrar/excluir del
--    total facturado. Las existentes quedan como produccion (default false): hasta hoy solo se emitia
--    en produccion.
ALTER TABLE facturaelectronica
    ADD COLUMN IF NOT EXISTS esprueba boolean NOT NULL DEFAULT false;

-- Indice parcial: solo las (pocas) facturas de prueba; sirve al EXISTS que decide si mostrar el filtro.
CREATE INDEX IF NOT EXISTS ix_facturaelectronica_esprueba
    ON facturaelectronica (idempresa) WHERE esprueba;

-- 2) Clave del .pfx de cada empresa, CIFRADA con ASP.NET Data Protection (nunca en claro). Sin fila =
--    pfx historico sin clave (comportamiento de siempre). Una fila por empresa.
CREATE TABLE IF NOT EXISTS certificado_arca_clave (
    idempresa        integer PRIMARY KEY,
    clave_protegida  text        NOT NULL,
    actualizado_en   timestamp   NOT NULL DEFAULT now(),
    actualizado_por  integer
);

ALTER TABLE certificado_arca_clave ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS certificado_arca_clave_rls ON certificado_arca_clave;
CREATE POLICY certificado_arca_clave_rls ON certificado_arca_clave FOR ALL
    USING (current_setting('app.id_empresa', true) IS NOT NULL AND current_setting('app.id_empresa', true) <> ''
           AND idempresa = current_setting('app.id_empresa', true)::int)
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

GRANT SELECT, INSERT, UPDATE ON certificado_arca_clave TO carnisys_user, cs_admin_pg;
