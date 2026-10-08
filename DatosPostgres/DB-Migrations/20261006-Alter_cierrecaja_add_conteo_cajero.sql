-- Conteo de cierre del cajero (pre-cierre) en cierrecaja (2026-10-06, ver docs/DECISIONS.md).
--  - cajacierrecajero: importe de efectivo que el propio cajero contó al terminar su turno. NULL = no cargó.
--  - conteobilletescajero: detalle del contador de billetes (lineas "N x $D = $ subtotal", "TOTAL EFECTIVO: ...").
--  - fechaconteocajero: cuando lo cargo / actualizo por ultima vez.
-- El cierre OFICIAL sigue siendo cajacierre (lo confirma el encargado); estas 3 columnas solo registran lo que
-- declaro el cajero, y el historial las compara contra cajacierre para mostrar el origen del importe.
-- Las escribe un metodo propio (GuardarConteoCajero) y addOrEditCierreCaja NO las toca, asi el cierre del encargado
-- nunca las pisa.
-- Aditiva, nullable e idempotente. Base unica multi-tenant (RLS por idempresa): una sola corrida cubre a todas las empresas.
-- ORDEN DE DEPLOY: correr ESTA migracion ANTES de publicar el codigo nuevo (el codigo nuevo lee estas columnas en
-- findCierreCaja). Correr conectado a 'carnisys' con el rol carnisys_admin.

ALTER TABLE cierrecaja ADD COLUMN IF NOT EXISTS cajacierrecajero double precision NULL;
ALTER TABLE cierrecaja ADD COLUMN IF NOT EXISTS conteobilletescajero text NULL;
ALTER TABLE cierrecaja ADD COLUMN IF NOT EXISTS fechaconteocajero timestamp NULL;
