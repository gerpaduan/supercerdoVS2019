-- Permisos de cuentas corrientes (2026-11-10, ver docs/DECISIONS.md "Asistente de usuarios y permisos").
--
-- 1) idform 17 "Ctas.Ctes General" tenia formconsulta/formedicion NULL: ningun chequeo del codigo (que busca la clave
--    formCtasCtes) lo encontraba, asi que para un usuario no admin la pantalla Finanzas > Cuentas Corrientes siempre daba
--    "Acceso denegado". Se le pone la clave y un nombre/descripcion claros. Solo si todavia no tiene clave (idempotente).
-- 2) idform 9003 "Ver cuentas corrientes reservadas" (clave formCtaCteReservada): interruptor nuevo. Hasta ahora ver las
--    cuentas reservadas colgaba de formCtasCtes (o sea, solo admin). Al estrenarlo nadie lo tiene: solo admin las ve.
--
-- Texto sin tildes a proposito (igual que el resto del catalogo). `formularios` es un catalogo global (sin idempresa, sin RLS).
-- Correr conectado a 'carnisys' con el rol carnisys_admin. Idempotente.
-- Reversion: UPDATE formularios SET nombreform='Ctas.Ctes General', formconsulta=NULL, descripcion=<la anterior> WHERE idform=17;
--            DELETE FROM permisosusuarios WHERE idform=9003; DELETE FROM formularios WHERE idform=9003;

UPDATE formularios SET
    nombreform = 'Cuentas corrientes',
    descripcion = 'Entrar a Cuentas corrientes (menu Finanzas) y ver la cuenta de cualquier persona. Dias ver = cuantos dias hacia atras ve los movimientos. '
                  || 'No incluye las cuentas reservadas (ver "Ver cuentas corrientes reservadas").',
    formconsulta = 'formCtasCtes'
WHERE idform = 17 AND COALESCE(formconsulta, '') = '';

INSERT INTO formularios (idform, nombreform, descripcion, formconsulta, formedicion)
SELECT 9003, 'Ver cuentas corrientes reservadas',
       'Permite ver completas las cuentas corrientes marcadas como reservadas y marcar o desmarcar a una persona como reservada. '
       || 'Sin este permiso se ve la persona pero no su saldo, y solo lo que cargo uno mismo desde que abrio su caja. '
       || 'Marcar solo una de las opciones (Puede ver). No hace falta Dias atras ni Alcance.',
       'formCtaCteReservada', 'formCtaCteReservada'
WHERE NOT EXISTS (SELECT 1 FROM formularios WHERE idform = 9003);
