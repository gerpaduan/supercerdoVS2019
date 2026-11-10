-- Borra lo que deja CADA clip del escenario "postventa" en la base de DESARROLLO (rol admin): ventas y lineas del
-- usuario de prueba 9101, su factura de prueba (homologacion), borradores, la apertura de caja y sus avisos. Correrlo
-- entre un clip y otro: si la caja queda abierta, el clip siguiente no muestra la apertura. NO borra el usuario ni los
-- datos de ejemplo (eso es postventa-limpieza-final.sql).
BEGIN;
SELECT set_config('app.id_empresa','1',true);
-- Factura electronica de prueba (homologacion) de esas ventas.
DELETE FROM alicuotaivaporfactura WHERE idfacturaelectronica IN (SELECT id FROM facturaelectronica WHERE idventa IN (SELECT idventa FROM ventas WHERE idvendedor = 9101));
DELETE FROM facturaelectronica WHERE idventa IN (SELECT idventa FROM ventas WHERE idvendedor = 9101);
-- Cuenta corriente y caja de esas ventas (venta a Cta. Cte.: movimiento de la clienta y egreso de caja de la venta).
DELETE FROM egresoscaja WHERE tabla = 'Ventas' AND idtabla IN (SELECT idventa FROM ventas WHERE idvendedor = 9101);
-- Pagos y cobros de la clienta (desde F2 o junto con la venta) y su registro en la caja.
DELETE FROM egresoscaja WHERE tabla = 'Pagos' AND idtabla IN (SELECT id FROM pagos WHERE idpersona = 9000001);
DELETE FROM pagos WHERE idpersona = 9000001;
-- Egresos de caja cargados a mano por el usuario de prueba (video del egreso, F7).
DELETE FROM egresoscaja WHERE creadopor = 9101;
DELETE FROM movctacte WHERE idpersona = 9000001 OR (tabla = 'Ventas' AND idtabla IN (SELECT idventa FROM ventas WHERE idvendedor = 9101));
-- Expendios de ejemplo cobrados en el video "expendios": vuelven a quedar pendientes (sin venta asignada).
UPDATE expendios SET idventa = NULL WHERE idexpendio IN (9000001, 9000002, 9000003);
DELETE FROM lineaventa WHERE idventa IN (SELECT idventa FROM ventas WHERE idvendedor = 9101);
DELETE FROM ventaborradorevento WHERE idborrador IN (SELECT id FROM ventaborrador WHERE idoperador = 9101 OR idusuariosesion = 9101);
DELETE FROM ventaborrador WHERE idoperador = 9101 OR idusuariosesion = 9101;
DELETE FROM ventas WHERE idvendedor = 9101;
DELETE FROM cierrecaja WHERE usuarioinicio = '9101';
DELETE FROM notificaciones WHERE mensaje LIKE 'Martín Gómez %';
COMMIT;
