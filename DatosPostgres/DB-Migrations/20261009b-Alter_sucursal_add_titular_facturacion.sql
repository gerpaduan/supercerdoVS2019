-- Titular de facturacion de cada sucursal (2026-10-09, ver docs/DECISIONS.md "Titulares fiscales por empresa y por sucursal").
-- sucursal.idtitularfacturacion = titulares_fiscales.idtitular que factura en esa sucursal. Sin FK real (igual que el
-- resto del esquema): lo valida Negocio.TitularFiscal. NULL se interpreta SIEMPRE como "el titular principal de la
-- empresa", asi cualquier camino que cree sucursales sin pasar por SystemAdministrationPg sigue funcionando.
-- Aditiva e idempotente. ORDEN DE DEPLOY: despues de 20261009 y ANTES de publicar el codigo nuevo.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.

ALTER TABLE sucursal ADD COLUMN IF NOT EXISTS idtitularfacturacion integer;

-- Backfill: toda sucursal existente factura con el titular principal de su empresa (comportamiento de hoy).
UPDATE sucursal s
SET idtitularfacturacion = t.idtitular
FROM titulares_fiscales t
WHERE t.idempresa = s.idempresa
  AND t.espredeterminado
  AND s.idtitularfacturacion IS NULL
  AND s.idempresa > 0;
