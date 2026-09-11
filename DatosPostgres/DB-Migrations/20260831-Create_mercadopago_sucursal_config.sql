-- Fase 1 de la integracion con Mercado Pago Point (plan de sesion 2026-08-31, ver
-- docs/DECISIONS.md). Guarda el toggle "Conectado con Point Mercado Pago" por Sucursal: un
-- default que fija un admin una vez, y la ultima eleccion del cajero en el modal de forma de
-- pago (se sobreescribe en cada cambio, y es lo que se usa para precargar el toggle la proxima
-- vez). Es por Sucursal y no por "caja" porque no existe ese concepto persistente en el modelo
-- (ver 20260831-Create_terminales_mercadopago.sql).
CREATE TABLE mercadopago_sucursal_config (
    idsucursal integer PRIMARY KEY,
    idempresa integer NOT NULL,
    conectadopointdefault boolean NOT NULL DEFAULT false,
    conectadopointultimaeleccion boolean NOT NULL DEFAULT false,
    fechaactualizacionutc timestamp
);

CREATE INDEX ix_mercadopago_sucursal_config_idempresa ON mercadopago_sucursal_config(idempresa);

ALTER TABLE mercadopago_sucursal_config ENABLE ROW LEVEL SECURITY;

CREATE POLICY mercadopago_sucursal_config_rls ON mercadopago_sucursal_config FOR ALL
    USING (current_setting('app.id_empresa', true) IS NOT NULL AND current_setting('app.id_empresa', true) <> ''
           AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0))
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

GRANT SELECT, INSERT, UPDATE, DELETE ON mercadopago_sucursal_config TO carnisys_user, cs_admin_pg;
