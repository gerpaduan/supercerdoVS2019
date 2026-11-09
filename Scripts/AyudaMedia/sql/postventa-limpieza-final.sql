-- Limpieza final despues de grabar (base de DESARROLLO, rol admin): primero postventa-limpieza-grabacion.sql; esto
-- borra los registros de login del usuario de prueba 9101, el usuario y los datos de ejemplo de postventa-datos-ejemplo.sql.
BEGIN;
SELECT set_config('app.id_empresa', '1', true);
DELETE FROM loginubicacionlog WHERE idusuario = 9101;
DELETE FROM usuarios WHERE id = 9101;
DELETE FROM lineaexpendio WHERE idexpendio = 9000001;
DELETE FROM expendios WHERE idexpendio = 9000001;
DELETE FROM personas WHERE idpersona = 9000001;
DELETE FROM corte WHERE idcorte IN (9000001, 9000002, 9000003);
COMMIT;
