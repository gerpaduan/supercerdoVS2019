-- Cierre de Stock: persiste el flag "no contado" (docs/09-cambios-y-pendientes/bitacora-de-cambios.md,
-- 2026-09-28). El badge
-- "No contado" ya se calculaba en el cliente (WebCore/wwwroot/Scripts/app/stock.js) para las
-- lineas agregadas en bloque desde el modal "Productos no cargados", pero nunca viajaba al
-- servidor ni se guardaba. Aditiva e idempotente. Correr conectado a 'carnisys' con el rol
-- carnisys_admin.

ALTER TABLE corteporcompra ADD COLUMN IF NOT EXISTS nocontado boolean NOT NULL DEFAULT false;
