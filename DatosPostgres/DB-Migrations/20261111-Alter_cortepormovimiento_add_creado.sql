-- Hora de carga de cada linea de un movimiento (2026-11-11, ver docs/DECISIONS.md "Movimientos: tiempo de envio,
-- llegada y hora de carga por linea"): agrega cortepormovimiento.creado, el momento en que el operador agrego la
-- linea al movimiento. El inicio del movimiento es la hora de su primera linea (MIN(creado)); no hay columna propia.
-- Las lineas anteriores a este cambio quedan en NULL (la pantalla muestra "-"): no se inventa una hora.
-- Rol: carnisys_admin. Idempotente (se puede correr dos veces). Los GRANT de la tabla ya cubren la columna nueva.
ALTER TABLE cortepormovimiento ADD COLUMN IF NOT EXISTS creado timestamp NULL;
