-- Feriados persistentes + observacion libre al fichar (2026-09-30, ver docs/DECISIONS.md).
--
-- esferiado: marca administrativa (nunca la pone el propio empleado al fichar -- ver
-- Negocio/RegistroJornada.cs) para que ese dia pague la EmpleadoTarifa de DiaSemana='Feriado' si
-- existe, antes que la del dia real. Se marca al corregir o al cargar una marcacion manualmente.
--
-- observaciones: nota libre que el empleado puede dejar al confirmar su fichaje (ej. "llegue
-- tarde por trafico") -- no implica una correccion de hora, distinta de motivocorreccion.
--
-- Aditivo e idempotente, mismo estilo que 20260928-Alter_corteporcompra_add_nocontado.sql.
-- Correr conectado a 'carnisys' con el rol carnisys_admin.
ALTER TABLE registrojornada
    ADD COLUMN IF NOT EXISTS esferiado boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS observaciones text;
