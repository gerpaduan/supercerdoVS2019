-- Etapa: Parametros.cs (primer modulo de la ronda de 6 modulos chicos 0%-migrados, tras
-- cerrar Corte.cs/Venta.cs/Usuario.cs). 5 metodos: ObtenerGrid, GuardarGrid,
-- ObtenerDiccionario, ObtenerValor, SetValor.
--
-- "parametros": catalogo global de definiciones de parametros (nombre, descripcion, tipo).
-- Sin idEmpresa, sin RLS -- mismo criterio que formularios/alicuotasiva (catalogos
-- compartidos entre todas las empresas). Se incluye la columna "id" pese a no tener ningun
-- caller conocido (duplica idparametro 1:1 en el original) -- replica fiel, no se audito el
-- 100% del codigo para confirmar que nada mas la usa.
CREATE TABLE parametros (
    idparametro integer PRIMARY KEY,
    nombre text NOT NULL,
    descripcion text,
    tipo smallint NOT NULL DEFAULT 0,
    id integer
);

CREATE UNIQUE INDEX ux_parametros_nombre ON parametros(nombre);

GRANT SELECT ON parametros TO carnisys_user, cs_admin_pg;

-- "empresaparametros": valor de cada parametro por empresa. DECISION: en SQL Server NO tiene
-- RLS (verificado, 0 filas en sys.security_policies) -- a diferencia de las ~39 tablas RLS ya
-- trianguladas en la Etapa 4 de esta migracion. No es un caso como "usuarios" (no hay problema
-- de tenant-todavia-no-conocido); simplemente nunca se le agrego RLS en el original. El
-- aislamiento hoy es solo por aplicacion (WHERE idEmpresa=@idEmpresa explicito en cada query).
-- Confirmado con el usuario: se agrega RLS estandar en Postgres como mejora deliberada (mismo
-- backstop que el resto de la migracion), no como replica 1:1 del original.
CREATE TABLE empresaparametros (
    idempresa integer NOT NULL,
    idparametro integer NOT NULL,
    valor text,
    PRIMARY KEY (idempresa, idparametro)
);

CREATE INDEX ix_empresaparametros_idempresa ON empresaparametros(idempresa);

ALTER TABLE empresaparametros ENABLE ROW LEVEL SECURITY;

CREATE POLICY empresaparametros_rls ON empresaparametros FOR ALL
    USING (current_setting('app.id_empresa', true) IS NOT NULL AND current_setting('app.id_empresa', true) <> ''
           AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0))
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

GRANT SELECT, INSERT, UPDATE, DELETE ON empresaparametros TO carnisys_user, cs_admin_pg;
