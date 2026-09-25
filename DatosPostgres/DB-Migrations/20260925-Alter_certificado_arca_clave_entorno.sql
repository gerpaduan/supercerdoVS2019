-- Certificados ARCA por entorno (2026-09-25, ver docs/DECISIONS.md "Certificados ARCA por entorno").
-- Correr como carnisys_admin (dueno de la tabla), ANTES de desplegar el codigo. Idempotente.
--
-- Cada empresa pasa a poder tener DOS certificados (produccion y homologacion), cada uno con su clave de
-- pfx. Antes la clave era una por empresa: guardar la de un cert de homologacion habria dejado ilegible el
-- pfx de produccion. Las filas existentes quedan como PROD (es lo unico que se cargaba hasta hoy).
-- Sin fila para un entorno = pfx sin clave (los certificados historicos).

ALTER TABLE certificado_arca_clave
    ADD COLUMN IF NOT EXISTS entorno varchar(4) NOT NULL DEFAULT 'PROD';

ALTER TABLE certificado_arca_clave DROP CONSTRAINT IF EXISTS certificado_arca_clave_entorno_chk;
ALTER TABLE certificado_arca_clave
    ADD CONSTRAINT certificado_arca_clave_entorno_chk CHECK (entorno IN ('PROD', 'HOMO'));

-- La clave primaria pasa de (idempresa) a (idempresa, entorno). Solo si todavia es la de una columna.
DO $$
BEGIN
    IF (SELECT count(*)
          FROM pg_constraint c, unnest(c.conkey) k
         WHERE c.conrelid = 'certificado_arca_clave'::regclass AND c.contype = 'p') = 1 THEN
        ALTER TABLE certificado_arca_clave DROP CONSTRAINT certificado_arca_clave_pkey;
        ALTER TABLE certificado_arca_clave ADD PRIMARY KEY (idempresa, entorno);
    END IF;
END $$;
