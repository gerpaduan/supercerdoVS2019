-- Datos de ejemplo para grabar los clips del escenario "postventa" (scripts/AyudaMedia, ver
-- docs/11-manual-de-uso/README.md). SOLO base de DESARROLLO (Postgres local), con el rol admin. Ficticios:
-- clienta "Laura Fernandez", dos productos pesables (codigos 9901 y 9902) y uno por unidad con codigo de barras EAN-13
-- valido (7790000000010, para mostrar que el lector lo agrega solo con cantidad 1), con ids altos que no chocan con datos reales.
-- El usuario de prueba de la grabacion (id 9101) NO se crea aca: lleva clave y se da de alta a mano con OK del
-- usuario (receta en la memoria del proyecto). Se borra todo con postventa-limpieza-final.sql.
BEGIN;
SELECT set_config('app.id_empresa', '1', true);
DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM personas WHERE idpersona = 9000001) THEN RAISE EXCEPTION 'persona 9000001 ya existe'; END IF;
  IF EXISTS (SELECT 1 FROM expendios WHERE idexpendio = 9000001) THEN RAISE EXCEPTION 'presupuesto 9000001 ya existe'; END IF;
  IF EXISTS (SELECT 1 FROM corte WHERE idcorte IN (9000001, 9000002, 9000003) OR (idempresa = 1 AND codigo IN (9901, 9902, 7790000000010))) THEN
    RAISE EXCEPTION 'producto de ejemplo ya existe';
  END IF;
END $$;
INSERT INTO personas (idpersona, identificacion, razonsocial, idiva, cuit, telefono, email, domicilio, ciudad, tipo, ctacte,
                      bonificacion, marca, idempresa, creado, ctactereservada)
VALUES (9000001, 'Laura Fernández', 'Laura Fernández', 1, '', '341 555-0142', '', 'Av. Belgrano 1450', 'Rosario', 'Cliente',
        false, 0, false, 1, now(), false);
-- Elaborados independientes, pesables y sin control de stock (encierrestock = false): no mueven stock al venderlos.
INSERT INTO corte (idcorte, codigo, corte, tipo, idcortemaestro, porcentaje, preciokg, independiente, creado, actualizado,
                   mayorista, encierrestock, habilitado, idalicuotaiva, alicuotaiva, nivel, pesable, puntostock, idmarca, idempresa)
VALUES (9000001, 9901, 'Vacío especial', 'Elaborado', 0, 100, 12500, 1, now(), now(), false, false, true, 4, 10.5, 0, true, 0, 0, 1),
       (9000002, 9902, 'Chorizo casero', 'Elaborado', 0, 100, 8900, 1, now(), now(), false, false, true, 4, 10.5, 0, true, 0, 0, 1),
       (9000003, 7790000000010, 'Gaseosa cola 1,5 L', 'Elaborado', 0, 100, 2500, 1, now(), now(), false, false, true, 4, 10.5, 0, false, 0, 0, 1);
-- Presupuesto vigente de la clienta (expendio del sector PRESUPUESTO, como los que arma Punto de Expendio), con precios
-- menores a los de lista: el historial de precios del POS (F8) los ofrece para copiar al carrito.
INSERT INTO expendios (idexpendio, idvendedor, fechaexpendio, identificacionexpendio, sector, idsucursal, creado, cantitems,
                       importe, idventa, idempresa, observaciones, nroremito, idpersona, fechacaducidad)
VALUES (9000001, 9101, now() - interval '2 days', 'Laura Fernández', 'PRESUPUESTO', 2, now(), 2, 32000, NULL, 1, '', '',
        9000001, current_date + 30);
INSERT INTO lineaexpendio (idlineaexpendio, idexpendio, idcorte, cantkg, preciokg, pesobalanza, idempresa)
VALUES (9000001, 9000001, 9000001, 2, 11800, false, 1),
       (9000002, 9000001, 9000002, 1, 8400, false, 1);
COMMIT;
