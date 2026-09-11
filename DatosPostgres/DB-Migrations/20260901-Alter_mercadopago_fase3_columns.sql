-- Fase 3 de la integracion con Mercado Pago Point: alta de Sucursal/Caja/Terminal contra la
-- API real de Mercado Pago (ver docs/DECISIONS.md 2026-09-01). Agrega columnas que la Fase 1
-- no habia anticipado -- se encontraron al leer el detalle real de las APIs de Store/POS/
-- Terminal, no estaban en la investigacion original.
--
-- mpuserid: el "user_id" que devuelve Mercado Pago en el intercambio OAuth (Fase 2) -- hace
-- falta para crear una Store via POST /users/{user_id}/stores. No se guardaba hasta ahora.
ALTER TABLE mercadopago_config ADD COLUMN mpuserid text;

-- mpstoreid: el id de "Store" que devuelve Mercado Pago al crearla -- una Store por Sucursal.
ALTER TABLE mercadopago_sucursal_config ADD COLUMN mpstoreid text;

-- posid: el id numerico de la Caja/POS que devuelve Mercado Pago al crearla -- hace falta
-- despues para consultar que terminal fisica quedo vinculada
-- (GET /terminals/v1/list?store_id&pos_id). terminalidmp pasa a admitir NULL: recien se
-- completa (con el id de HARDWARE de la terminal, formato "MARCA_MODELO__SERIAL", NO el id de
-- la Caja) despues de que el admin empareja la terminal fisica con la app de Mercado Pago
-- (paso manual, sin API) y CarniSys lo verifica.
ALTER TABLE terminales_mercadopago ADD COLUMN posid text;
ALTER TABLE terminales_mercadopago ALTER COLUMN terminalidmp DROP NOT NULL;
