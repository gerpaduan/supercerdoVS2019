-- Saldo del cliente en el POS y en el ticket (2026-10-19, ver docs/DECISIONS.md "Saldo del cliente en
-- el POS y en el ticket"): da de alta en el catalogo global "parametros" el parametro
-- saldoMinimoInformar (pesos). Hasta ese monto el POS no muestra el saldo de cuenta corriente del
-- cliente ni ofrece sumarlo al ticket. Tipo 1 = decimal (igual que los demas importes del catalogo).
--
-- El valor es POR EMPRESA (empresaparametros) y se edita en la pantalla Parametros. Sin valor cargado
-- el codigo usa Negocio.CalculoSaldoCliente.SaldoMinimoInformarPorDefecto (100), asi que esta
-- migracion no inserta ningun valor: solo hace visible el parametro para poder cambiarlo.
-- idparametro no es serial: se toma el siguiente libre. Idempotente (se puede correr dos veces).
INSERT INTO parametros (idparametro, nombre, descripcion, tipo, id)
SELECT n.siguiente,
       'saldoMinimoInformar',
       'Saldo de cuenta corriente (en pesos) hasta el cual el POS NO lo muestra ni ofrece sumarlo al ticket. Vacio = 100',
       1,
       n.siguiente
FROM (SELECT COALESCE(MAX(idparametro), 0) + 1 AS siguiente FROM parametros) n
WHERE NOT EXISTS (SELECT 1 FROM parametros WHERE nombre = 'saldoMinimoInformar');
