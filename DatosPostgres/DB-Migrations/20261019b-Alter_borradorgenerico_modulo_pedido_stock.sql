-- Borradores en servidor: nuevo modulo PEDIDO_STOCK (2026-10-19, "Ingresar pedido" de Existencia por sucursales, Fase 2).
-- Amplia el CHECK ck_borradorgenerico_modulo (20260922a-Create_borradorgenerico.sql) para aceptar el modulo nuevo.
-- Sin esto el INSERT de un borrador de pedido falla con 23514 y el navegador solo muestra "sin conexion" (no se resguarda nada).
-- notificaciones.tipo ya es varchar(60) (20260924c): 'BORRADOR_INTERRUMPIDO_PEDIDO_STOCK' (34) entra, no hace falta tocarlo.
-- Correr como carnisys_admin (dueno de la tabla), ANTES de desplegar el codigo. Idempotente: se puede correr mas de una vez.
-- Solo Postgres. En SQL Server no se agrega: el modo "Ingresar pedido" depende de la actividad de "Que pedir" (solo Postgres),
-- asi que ese motor nunca llega a guardar un borrador PEDIDO_STOCK.
ALTER TABLE borradorgenerico DROP CONSTRAINT IF EXISTS ck_borradorgenerico_modulo;
ALTER TABLE borradorgenerico ADD CONSTRAINT ck_borradorgenerico_modulo
    CHECK (modulo IN ('COMPRA', 'STOCK', 'MOVIMIENTO', 'EMBUTIDO_CARGA', 'EMBUTIDO_RAPIDO', 'PEDIDO_STOCK'));
