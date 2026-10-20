-- Detalle del contador de billetes de la APERTURA de caja en cierrecaja (2026-10-17, ver docs/DECISIONS.md).
--  - conteobilletesapertura: lineas del contador de billetes con el que se cargo el monto inicial
--    ("N x $D = $ subtotal", "TOTAL EFECTIVO: ..."). NULL = el monto inicial se tipeo a mano / no se uso el contador.
-- El monto inicial sigue siendo cajainicio; esta columna solo guarda el detalle para el ticket de apertura.
-- Igual que el conteo de cierre del cajero (20261006-Alter_cierrecaja_add_conteo_cajero.sql), la escribe un metodo
-- propio (GuardarConteoApertura) y addOrEditCierreCaja NO la toca: si esta migracion todavia no corrio, abrir la caja
-- sigue funcionando (solo se pierde el detalle del contador en el ticket).
-- Aditiva, nullable e idempotente. Base unica multi-tenant (RLS por idempresa): una sola corrida cubre a todas las empresas.
-- ORDEN DE DEPLOY: correr ESTA migracion ANTES de publicar el codigo nuevo. Correr conectado a 'carnisys' con el rol carnisys_admin.
-- Solo Postgres: en SQL Server la funcion queda sin soporte (SoportaConteoApertura = false), como el conteo del cajero.

ALTER TABLE cierrecaja ADD COLUMN IF NOT EXISTS conteobilletesapertura text NULL;
