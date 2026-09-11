-- Fase 1 de la integracion con Mercado Pago Point (plan de sesion 2026-08-31, ver
-- docs/DECISIONS.md). Una fila por empresa: credenciales OAuth de la cuenta de Mercado Pago
-- que esa empresa conecto -- una sola cuenta cubre todas sus sucursales (decision confirmada
-- con el usuario, no una cuenta por sucursal).
--
-- accesstokencifrado/refreshtokencifrado se guardan CIFRADOS a nivel de aplicacion
-- (Web/Helpers/MercadoPagoTokenCipher.cs) -- decision explicita del usuario, distinta del
-- precedente de configuracionwhatsapp (texto plano): son credenciales que mueven dinero real.
--
-- clientstate: token opaco generado al iniciar "Conectar con Mercado Pago", usado como
-- parametro OAuth "state" y validado en el callback antes de confiar en el idempresa que trae
-- la URL (evita que alguien arme una URL de callback apuntando a otra empresa).
CREATE TABLE mercadopago_config (
    idempresa integer PRIMARY KEY,
    clientstate text,
    accesstokencifrado text,
    refreshtokencifrado text,
    tokenexpirautc timestamp,
    conectado boolean NOT NULL DEFAULT false,
    idusuarioconexion integer,
    fechaconexionutc timestamp,
    fechaactualizacionutc timestamp
);

ALTER TABLE mercadopago_config ENABLE ROW LEVEL SECURITY;

CREATE POLICY mercadopago_config_rls ON mercadopago_config FOR ALL
    USING (current_setting('app.id_empresa', true) IS NOT NULL AND current_setting('app.id_empresa', true) <> ''
           AND (idempresa = current_setting('app.id_empresa', true)::int OR idempresa = 0))
    WITH CHECK (idempresa = current_setting('app.id_empresa', true)::int);

GRANT SELECT, INSERT, UPDATE, DELETE ON mercadopago_config TO carnisys_user, cs_admin_pg;
