-- Datos del emisor guardados en cada comprobante (2026-10-09, ver docs/DECISIONS.md "Titulares fiscales por empresa y
-- por sucursal"): quien emitio la factura (titular) y sus datos fiscales al momento de emitir, para que la
-- reimpresion, la edicion y la nota de credito usen SIEMPRE los datos del titular que facturo.
-- Todo NULL = comprobante anterior a esta funcion: se imprime con los datos de la empresa como hasta hoy
-- (sin UPDATE masivo). Los totales del titular principal cuentan idtitular IS NULL OR idtitular = <principal>.
-- Aditiva e idempotente. ORDEN DE DEPLOY: despues de 20261009 y ANTES de publicar el codigo nuevo.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.

ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS idtitular integer;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS cuitemisor bigint;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS razonsocialemisor text;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS condicionivaemisor text;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS esrriiemisor boolean;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS iibbemisor bigint;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS inicioactividademisor date;
ALTER TABLE facturaelectronica ADD COLUMN IF NOT EXISTS domicilioemisor text;

CREATE INDEX IF NOT EXISTS ix_facturaelectronica_titular_fecha ON facturaelectronica (idtitular, fechaemisionafip);
