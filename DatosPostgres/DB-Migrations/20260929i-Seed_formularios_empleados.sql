-- Modulo Empleados y Liquidacion de Sueldos (2026-09-29, ver docs/DECISIONS.md). Da de alta los 2
-- Formulario nuevos para Permisos.Empleado (Entidades/Permisos.cs): ABM de empleados (incluida la
-- pestaña Vacaciones) y Liquidaciones (Ver=consultar listado/historico, Editar=generar/eliminar
-- liquidaciones, mismo esquema Ver/Editar que el resto de PermisosUsuarios).
--
-- idform: la tabla "formularios" es un catalogo global sin IDENTITY (PK manual, ver
-- 20260819-Alter_usuarios_create_formularios_permisosusuarios.sql) y este repo no tiene ningun
-- INSERT INTO formularios registrado en migraciones -- las filas existentes vinieron de una copia
-- de datos desde SQL Server, no de un script. No se pudo verificar contra la base real que 9001/9002
-- esten libres.
-- TODO(claude): antes de correr esta migracion, ejecutar `SELECT MAX(idform) FROM formularios;`
-- contra la base real y ajustar estos valores si hiciera falta (se eligieron altos a proposito para
-- minimizar el riesgo de choque con IDs legados bajos).
--
-- Correr conectado a 'carnisys' con el rol carnisys_admin.
INSERT INTO formularios (idform, nombreform, descripcion, formconsulta, formedicion)
SELECT 9001, 'Empleados', 'Alta, edición y baja de empleados (incluye vacaciones)', 'formEmpleados', 'formNuevoEmpleado'
WHERE NOT EXISTS (SELECT 1 FROM formularios WHERE idform = 9001);

INSERT INTO formularios (idform, nombreform, descripcion, formconsulta, formedicion)
SELECT 9002, 'Liquidación de sueldos', 'Generar, consultar y eliminar liquidaciones de sueldo', 'formLiquidaciones', 'formNuevaLiquidacion'
WHERE NOT EXISTS (SELECT 1 FROM formularios WHERE idform = 9002);
