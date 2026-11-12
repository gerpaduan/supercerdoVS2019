-- Tiempo de envio de un movimiento entre sucursales (2026-11-11, ver docs/DECISIONS.md "Movimientos: tiempo de
-- envio, llegada y hora de carga por linea"): da de alta en el catalogo global "parametros" el parametro
-- minutosEnvioMovimiento. Tipo 3 = entero (minutos). Al guardar un movimiento NUEVO, la fecha del movimiento queda en
-- la hora actual + estos minutos (llegada estimada); en la pantalla se puede ajustar en cada movimiento.
--
-- El valor es POR EMPRESA (empresaparametros) y se edita en la pantalla Parametros. Sin valor cargado el codigo usa 0
-- (la fecha queda en la hora actual, el comportamiento de antes), asi que esta migracion no inserta ningun valor: solo
-- hace visible el parametro. idparametro no es serial: se toma el siguiente libre. Idempotente.
INSERT INTO parametros (idparametro, nombre, descripcion, tipo, id)
SELECT n.siguiente,
       'minutosEnvioMovimiento',
       'Minutos que tarda un movimiento entre sucursales: al guardar un movimiento nuevo, la fecha queda en la hora actual mas estos minutos. Vacio = 0',
       3,
       n.siguiente
FROM (SELECT COALESCE(MAX(idparametro), 0) + 1 AS siguiente FROM parametros) n
WHERE NOT EXISTS (SELECT 1 FROM parametros WHERE nombre = 'minutosEnvioMovimiento');
