-- Entrega 2 de Remitos (2026-09-26, ver docs/DECISIONS.md): numero de remito en el expendio
-- del sector REMITOS. Formato PPPP-NNNNNNNN (prefijo = id de la sucursal a 4 digitos,
-- correlativo de 8 digitos por sucursal), autogenerado en el POS y editable a mano.
-- Vacio ('') para todos los expendios que no son remitos (y para los ya existentes).
-- Aditiva e idempotente. Correr conectado a 'carnisys' con el rol carnisys_admin.

ALTER TABLE expendios ADD COLUMN IF NOT EXISTS nroremito text NOT NULL DEFAULT '';

-- Unico por empresa + sucursal + numero (solo cuando hay numero): evita que dos usuarios
-- carguen el mismo remito a la vez o que se repita por una edicion manual.
CREATE UNIQUE INDEX IF NOT EXISTS ux_expendios_nroremito
    ON expendios (idempresa, idsucursal, nroremito)
    WHERE nroremito <> '';
