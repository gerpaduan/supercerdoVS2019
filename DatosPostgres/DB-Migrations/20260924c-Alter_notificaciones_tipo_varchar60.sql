-- notificaciones.tipo: de varchar(30) a varchar(60) (2026-09-24, ver docs/DECISIONS.md "notificaciones.tipo demasiado corto").
-- Correr como carnisys_admin (dueno de las tablas), ANTES de desplegar. Aditiva e idempotente: agrandar un varchar
-- no reescribe la tabla y el indice unico ux_notificaciones_empresa_tipo_refid sigue valido.
-- Motivo: los tipos de borradores de Embutidos (BORRADOR_INTERRUMPIDO_EMBUTIDO_RAPIDO = 37 caracteres, etc.) no
-- entraban en varchar(30): GET /Notificaciones/Resumen devolvia 500 (22001) cuando habia borradores activos de Embutidos.
ALTER TABLE notificaciones ALTER COLUMN tipo TYPE varchar(60);
