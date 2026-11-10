-- Factura que se abre sola al cobrar en el POS (2026-11-10, ver docs/DECISIONS.md "Parametro: que formas de pago
-- abren la factura al cobrar"): da de alta en el catalogo global "parametros" el parametro
-- formasPagoFacturaAutomatica. Tipo 0 = texto: lista separada por comas de Efectivo, Debito, Credito, Qr y
-- Transferencia, o "Ninguna".
--
-- El valor es POR EMPRESA (empresaparametros) y se edita en la pantalla Parametros. Sin valor cargado el codigo
-- usa Debito,Credito,Qr,Transferencia (el comportamiento de antes), asi que esta migracion no inserta ningun
-- valor: solo hace visible el parametro. idparametro no es serial: se toma el siguiente libre. Idempotente.
INSERT INTO parametros (idparametro, nombre, descripcion, tipo, id)
SELECT n.siguiente,
       'formasPagoFacturaAutomatica',
       'Formas de pago que abren la factura electronica al cobrar en el POS (lista: Efectivo, Debito, Credito, Qr, Transferencia; o Ninguna). Vacio = todas menos Efectivo',
       0,
       n.siguiente
FROM (SELECT COALESCE(MAX(idparametro), 0) + 1 AS siguiente FROM parametros) n
WHERE NOT EXISTS (SELECT 1 FROM parametros WHERE nombre = 'formasPagoFacturaAutomatica');
