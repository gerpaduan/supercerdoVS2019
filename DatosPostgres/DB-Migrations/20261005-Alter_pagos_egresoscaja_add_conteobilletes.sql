-- Conteo de billetes como campo propio en pagos/cobros y egresos de caja (2026-10-05, ver docs/DECISIONS.md).
--  - conteobilletes: texto del conteo de efectivo hecho con el contador de billetes (lineas
--    "N x $D = $ subtotal", "Monedas: ...", "TOTAL EFECTIVO: ..."). Antes ese texto se mezclaba dentro de
--    observaciones (pagos) o detalle (egresoscaja). NULL / vacio = sin conteo.
-- Aditiva, nullable e idempotente: el codigo viejo sigue andando con las columnas ya creadas.
-- ORDEN DE DEPLOY: correr ESTA migracion ANTES de publicar el codigo nuevo (el codigo nuevo lee y
-- escribe estas columnas en cada guardado de pago y de egreso). Ver docs/RUNBOOK.md.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.

ALTER TABLE pagos ADD COLUMN IF NOT EXISTS conteobilletes text NULL;
ALTER TABLE egresoscaja ADD COLUMN IF NOT EXISTS conteobilletes text NULL;
