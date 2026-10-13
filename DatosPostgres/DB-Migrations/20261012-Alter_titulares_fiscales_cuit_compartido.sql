-- Titular compartido entre empresas (2026-10-12, ver docs/DECISIONS.md "Titular fiscal compartido entre empresas").
-- Un mismo CUIT puede ser titular en varias empresas y todas usan EL MISMO certificado de ARCA (AFIP/<cuit>/{prod|homo}/):
--  1. El CUIT deja de ser unico en toda la base: pasa a ser unico DENTRO de cada empresa (idempresa, cuit). El CUIT de
--     la empresa (login /Login/<cuit>, empresas.cuit) sigue siendo unico: un CUIT es principal de UNA sola empresa.
--  2. certificados_arca_cuit: registro por CUIT y entorno del certificado instalado y de la clave (cifrada) de su .pfx.
--     NO tiene RLS por empresa a proposito: es el unico lugar donde todas las empresas que comparten el CUIT leen la
--     misma clave. La clave esta cifrada con Data Protection (nunca en claro) y el codigo la lee solo por CUIT.
--     El titular principal sigue leyendo su clave de certificado_arca_clave (sin cambios); al instalar un certificado
--     se escribe en ambas tablas, para poder volver a un build anterior sin dejar claves desfasadas.
--  3. titulares_fiscales: revocado / revocado_en / revocado_por (cancelacion de la titularidad por el super-admin).
-- Aditiva e idempotente. Correr conectado a 'carnisys' con el rol carnisys_admin (dueno de las tablas).
-- NO hay que regenerar certificados: el .pfx no se toca y la clave se copia tal cual (mismo cifrado que el principal).
-- ORDEN DE DEPLOY: backup -> esta migracion -> codigo nuevo.

-- 1. Unicidad del CUIT por empresa (y no global).
DROP INDEX IF EXISTS ux_titulares_fiscales_cuit;
CREATE UNIQUE INDEX IF NOT EXISTS ux_titulares_fiscales_empresa_cuit ON titulares_fiscales (idempresa, cuit) WHERE cuit IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_titulares_fiscales_cuit ON titulares_fiscales (cuit) WHERE cuit IS NOT NULL;

-- 2. Cancelacion de la titularidad (super-admin). Un titular revocado no se puede habilitar ni usar para emitir.
ALTER TABLE titulares_fiscales ADD COLUMN IF NOT EXISTS revocado boolean NOT NULL DEFAULT false;
ALTER TABLE titulares_fiscales ADD COLUMN IF NOT EXISTS revocado_en timestamp;
ALTER TABLE titulares_fiscales ADD COLUMN IF NOT EXISTS revocado_por integer;

-- 3. Certificado por CUIT (compartido entre las empresas que tienen a ese CUIT como titular).
CREATE TABLE IF NOT EXISTS certificados_arca_cuit (
    cuit                    bigint NOT NULL,
    entorno                 varchar(4) NOT NULL,
    nombrecertificado_pfx   text NOT NULL DEFAULT 'certificado.pfx',   -- nombre historico (PROD) o el fijo
    clave_protegida         text,                                      -- NULL = pfx historico sin clave guardada
    actualizado             timestamp NOT NULL DEFAULT now(),
    PRIMARY KEY (cuit, entorno),
    CONSTRAINT ck_certificados_arca_cuit_entorno CHECK (entorno IN ('PROD', 'HOMO'))
);

GRANT SELECT, INSERT, UPDATE ON certificados_arca_cuit TO carnisys_user, cs_admin_pg;

-- Siembra: la clave que hoy guarda cada empresa para su CUIT (misma clave cifrada, sin tocar el .pfx).
INSERT INTO certificados_arca_cuit (cuit, entorno, nombrecertificado_pfx, clave_protegida)
SELECT e.cuit, c.entorno,
       CASE WHEN c.entorno = 'PROD' THEN COALESCE(NULLIF(e.nombrecertificado_pfx, ''), 'certificado.pfx') ELSE 'certificado.pfx' END,
       c.clave_protegida
FROM certificado_arca_clave c
JOIN empresas e ON e.idempresa = c.idempresa
WHERE e.idempresa > 0 AND e.cuit IS NOT NULL AND e.cuit > 0
ON CONFLICT (cuit, entorno) DO NOTHING;

-- Siembra: certificados historicos de produccion (pfx en la raiz del CUIT, sin clave guardada).
INSERT INTO certificados_arca_cuit (cuit, entorno, nombrecertificado_pfx, clave_protegida)
SELECT e.cuit, 'PROD', e.nombrecertificado_pfx, NULL
FROM empresas e
WHERE e.idempresa > 0 AND e.cuit IS NOT NULL AND e.cuit > 0 AND COALESCE(e.nombrecertificado_pfx, '') <> ''
ON CONFLICT (cuit, entorno) DO NOTHING;

-- Consultas entre empresas (el CUIT ya es titular en otra empresa, administradora del certificado, punto de venta
-- repetido): corren con el rol de super-admin (SET LOCAL ROLE, BYPASSRLS no exime de los GRANT).
GRANT SELECT ON titulares_fiscales_ptovta TO carnisys_sysadmin_bypass;
