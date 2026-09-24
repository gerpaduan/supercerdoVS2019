-- Certificado de la PLATAFORMA para el padron compartido (ws_sr_padron_a13) -- 2026-09-24, ver
-- docs/DECISIONS.md "Certificado global de padron". Correr como carnisys_admin (dueno de las tablas),
-- ANTES de desplegar el codigo.
-- Una sola fila (id = 1): el CUIT dueno del alias de la plataforma, el nombre del .pfx (en AFIP/_plataforma/)
-- y su clave CIFRADA con ASP.NET Data Protection (nunca en claro).
-- Seguridad: RLS activado SIN policy => cerrada para carnisys_user (una sesion de empresa no la ve ni con SQL
-- directo). Solo se accede con SET LOCAL ROLE carnisys_sysadmin_bypass (BYPASSRLS), que es lo que hace
-- DatosPostgres.PlataformaCertificadoArcaPg.
CREATE TABLE IF NOT EXISTS plataforma_certificado_arca (
    id               integer PRIMARY KEY DEFAULT 1,
    cuit             bigint    NOT NULL,
    nombre_archivo   text      NOT NULL,
    clave_protegida  text      NOT NULL,
    actualizado_en   timestamp NOT NULL DEFAULT now(),
    actualizado_por  integer,
    CONSTRAINT ck_plataforma_certificado_arca_unica CHECK (id = 1)
);

ALTER TABLE plataforma_certificado_arca ENABLE ROW LEVEL SECURITY;

GRANT SELECT, INSERT, UPDATE ON plataforma_certificado_arca TO carnisys_sysadmin_bypass;
