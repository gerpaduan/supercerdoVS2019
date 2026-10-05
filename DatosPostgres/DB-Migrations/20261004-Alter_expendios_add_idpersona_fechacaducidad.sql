-- Cliente real y caducidad en el expendio (2026-10-04, ver docs/DECISIONS.md).
--  - idpersona: cliente real elegido en el POS de Expendio (cualquier sector). NULL = cliente
--    manual (texto libre en identificacionexpendio) o Consumidor Final. Sin FK, igual que
--    ventas.idpersona.
--  - fechacaducidad: solo sector PRESUPUESTO. Ultimo dia (inclusive) en que sus precios se pueden
--    copiar a una venta. NULL = se asume vigencia + 90 dias (lo resuelve Negocio.PresupuestosCliente).
-- Aditiva, nullable e idempotente: el codigo viejo sigue andando con las columnas ya creadas.
-- ORDEN DE DEPLOY: correr ESTA migracion ANTES de publicar el codigo nuevo (el codigo nuevo
-- escribe estas columnas en cada guardado de expendio). Ver docs/RUNBOOK.md.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.

ALTER TABLE expendios ADD COLUMN IF NOT EXISTS idpersona integer NULL;
ALTER TABLE expendios ADD COLUMN IF NOT EXISTS fechacaducidad date NULL;

-- Historial de presupuestos por cliente (solo filas con cliente).
CREATE INDEX IF NOT EXISTS ix_expendios_idpersona
    ON expendios (idpersona)
    WHERE idpersona IS NOT NULL;
