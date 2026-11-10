-- Datos de ejemplo para el video "expendios" (escenario tutorial, 2026-11-10), base de DESARROLLO, rol admin. Correr
-- DESPUES de postventa-datos-ejemplo.sql (usa el usuario 9101 y los productos 9901/9902). Dos expendios de HOY del
-- sector CARNICERIA en la sucursal 2, como los que arma el carnicero en Punto de Expendio, pendientes de cobrar.
-- Se borran con postventa-limpieza-final.sql; entre toma y toma postventa-limpieza-grabacion.sql los deja pendientes.
BEGIN;
SELECT set_config('app.id_empresa', '1', true);
DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM expendios WHERE idexpendio IN (9000002, 9000003)) THEN RAISE EXCEPTION 'expendio de ejemplo ya existe'; END IF;
  IF EXISTS (SELECT 1 FROM lineaexpendio WHERE idlineaexpendio IN (9000003, 9000004, 9000005)) THEN RAISE EXCEPTION 'linea de ejemplo ya existe'; END IF;
END $$;
INSERT INTO expendios (idexpendio, idvendedor, fechaexpendio, identificacionexpendio, sector, idsucursal, creado, cantitems,
                       importe, idventa, idempresa, observaciones, nroremito, idpersona, fechacaducidad)
VALUES (9000002, 9101, now() - interval '25 minutes', 'Mostrador 12', 'CARNICERIA', 2, now(), 2, 23995, NULL, 1, '', '', NULL, NULL),
       (9000003, 9101, now() - interval '10 minutes', 'Mostrador 15', 'CARNICERIA', 2, now(), 1, 10680, NULL, 1, '', '', NULL, NULL);
-- 9000002: vacio 1,350 kg + chorizo 0,800 kg; 9000003: chorizo 1,200 kg (precios de lista de los productos de ejemplo).
INSERT INTO lineaexpendio (idlineaexpendio, idexpendio, idcorte, cantkg, preciokg, pesobalanza, idempresa)
VALUES (9000003, 9000002, 9000001, 1.35, 12500, true, 1),
       (9000004, 9000002, 9000002, 0.8, 8900, true, 1),
       (9000005, 9000003, 9000002, 1.2, 8900, true, 1);
COMMIT;
