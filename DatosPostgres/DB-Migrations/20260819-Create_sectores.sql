-- Etapa 12a: bloque Sectores/Licencias de Venta.cs. Solo hace falta crear "sectores" --
-- expendios/licencias ya existen (slice minimo de la Etapa 10, cambiarSucursalCaja).
-- Verificado contra la base CarniSys real antes de escribir esto (columnas, sin PK/identity,
-- sin RLS explicita en el original -- se agrega igual, mismo criterio estandar del resto de
-- la migracion). Correr conectado a 'carnisys' con el rol carnisys_admin.

CREATE TABLE sectores (
    sector text,
    idempresa integer NOT NULL DEFAULT 0
);

CREATE INDEX ix_sectores_idempresa ON sectores(idempresa);

ALTER TABLE sectores ENABLE ROW LEVEL SECURITY;

CREATE POLICY sectores_rls ON sectores FOR ALL
    USING (current_setting('app.id_empresa', true) IS NOT NULL AND current_setting('app.id_empresa', true) <> ''
           AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0))
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

GRANT SELECT, INSERT, UPDATE, DELETE ON sectores TO carnisys_user, cs_admin_pg;
