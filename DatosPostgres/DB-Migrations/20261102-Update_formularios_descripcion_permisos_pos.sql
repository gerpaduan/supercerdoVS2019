-- Descripciones de los permisos "POS - ..." (2026-11-02, ver docs/DECISIONS.md "POS: permisos por funcion").
-- Solo cambia el texto que se muestra en Usuarios > Permisos (columna Detalle); no toca permisos ni usuarios.
-- Correr como carnisys_admin (dueno de la tabla). El catalogo `formularios` es global (sin idempresa, sin RLS).
-- Texto sin tildes a proposito, igual que el resto del catalogo (el cliente psql de Windows no siempre es UTF-8).
-- Reversion: volver a poner los textos anteriores (estan al pie).

UPDATE formularios SET descripcion =
    'Da acceso al POS. Puede ver: ve la cuenta corriente de los clientes (Dias ver = cuantos dias hacia atras). '
    || 'Puede editar: carga pagos y cobros y modifica los que cargo el mismo (Dias editar nunca pasa del inicio de su caja abierta).'
WHERE idform = 18 AND nombreform = 'POS - Ctas.Ctes Persona';

UPDATE formularios SET descripcion =
    'Da acceso al POS. Permite bonificar precios de productos en la venta. '
    || 'Marcar solo una de las opciones (Puede ver o Puede editar). No hace falta Dias atras ni Alcance.'
WHERE idform = 32 AND nombreform = 'POS - Bonificacion';

UPDATE formularios SET descripcion =
    'Da acceso al POS. Puede ver: lista sus ventas desde el inicio de su caja. '
    || 'Puede editar: modifica esas ventas (Dias editar nunca pasa del inicio de su caja abierta).'
WHERE idform = 33 AND nombreform = 'POS - Ultimas Ventas';

UPDATE formularios SET descripcion =
    'Da acceso al POS. Permite ver las 5 ultimas ventas de un cliente. '
    || 'No hace falta Dias atras, ni Puede editar, tampoco el Alcance.'
WHERE idform = 34 AND nombreform = 'POS - Ultimas 5 Ventas Cliente';

UPDATE formularios SET descripcion =
    'Historial de Ventas. No da acceso al POS: para eso hace falta tener activo al menos un permiso "POS - ...".'
WHERE idform = 7 AND nombreform = 'Ventas';

-- Textos anteriores (para revertir):
--   18: 'Ver Cta Cte de Cliente y Agregar Pagos/Cobros'
--   32: 'Permiso para bonificar precio de productos en Punto de Venta. Para obtener el permiso seleccione solo una de las opciones. No es necesario ingresar Dias Atras.'
--   33: 'Permiso para que el vendedor acceda a sus ultimas ventas desde la apertura de caja y pueda modificarlas.'
--   34: 'Permiso para que el vendedor acceda a las 5 ultimas ventas de un cliente. No es necesario ingresar Dias Atras, ni Editar, tampoco el Alcance.'
--    7: 'Historial de Ventas'
