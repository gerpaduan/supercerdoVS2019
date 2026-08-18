-- Tercer batch del piloto (Etapa 4): Licencias, VencimientosLicencia (Datos/OtrasClases.cs)
-- y Conexiones (Datos/Sucursal.cs, ya declarada en ISucursalRepository desde la Etapa 3).
-- Correr conectado a 'carnisys' con el rol carnisys_admin.
-- Sin PK explicita en licencias/vencimientoslicencia/conexiones (salvo conexiones.name):
-- SQL Server tampoco las tiene (confirmado en schema-columnas.txt), fidelidad al esquema real.

CREATE TABLE licencias (
    nrolicencia text,
    sector text,
    identificacion text,
    creado timestamp,
    idempresa integer NOT NULL DEFAULT 0
);

CREATE TABLE vencimientoslicencia (
    fechavencimiento date,
    pagado boolean NOT NULL DEFAULT false,
    fechapago date,
    idempresa integer NOT NULL DEFAULT 0
);

CREATE TABLE conexiones (
    name text PRIMARY KEY,
    connectionstring text,
    nombre text,
    idsucursal integer,
    mostrarenprincipal boolean NOT NULL DEFAULT false,
    mostrarenstockactual boolean NOT NULL DEFAULT false,
    idempresa integer NOT NULL DEFAULT 0
);

CREATE INDEX ix_licencias_idempresa ON licencias(idempresa);
CREATE INDEX ix_vencimientoslicencia_idempresa ON vencimientoslicencia(idempresa);
CREATE INDEX ix_conexiones_idempresa ON conexiones(idempresa);

ALTER TABLE licencias ENABLE ROW LEVEL SECURITY;
ALTER TABLE vencimientoslicencia ENABLE ROW LEVEL SECURITY;
ALTER TABLE conexiones ENABLE ROW LEVEL SECURITY;

CREATE POLICY licencias_rls ON licencias
    FOR ALL
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0)
    )
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

CREATE POLICY vencimientoslicencia_rls ON vencimientoslicencia
    FOR ALL
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0)
    )
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

CREATE POLICY conexiones_rls ON conexiones
    FOR ALL
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0)
    )
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

GRANT SELECT, INSERT, UPDATE, DELETE ON licencias, vencimientoslicencia, conexiones TO carnisys_user, cs_admin_pg;
