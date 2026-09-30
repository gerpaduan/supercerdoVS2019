-- Empleados: se saca el campo "legajo" (2026-09-30, ver docs/DECISIONS.md). Era un codigo interno
-- redundante con personas.identificacion/cuit -- la busqueda de empleados pasa a filtrar por
-- razon social, identificacion o cuit directamente (ver DatosPostgres/EmpleadoPg.cs Listar).
DROP INDEX IF EXISTS ux_empleado_empresa_legajo;
ALTER TABLE empleado DROP COLUMN IF EXISTS legajo;
