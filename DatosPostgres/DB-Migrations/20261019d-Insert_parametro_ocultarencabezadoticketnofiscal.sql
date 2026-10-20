-- Ocultar los datos de la empresa en el ticket de venta no fiscal (2026-10-19, ver docs/DECISIONS.md
-- "Parametro para ocultar los datos de la empresa en el ticket de venta no fiscal"): da de alta en el
-- catalogo global "parametros" el parametro ocultarEncabezadoTicketNoFiscal. Tipo 2 = Si/No.
-- Con Si, el ticket termico de venta X (ESC/POS y HTML) no imprime el nombre ni los slogans de la
-- empresa. El ticket facturado (con CAE) no cambia.
--
-- El valor es POR EMPRESA (empresaparametros) y se edita en la pantalla Parametros. Sin valor cargado
-- el codigo usa No (se imprimen, como siempre), asi que esta migracion no inserta ningun valor: solo
-- hace visible el parametro para poder cambiarlo. El tilde significa "ocultar" a proposito: la
-- pantalla muestra destildado todo Si/No sin valor y al guardar graba 0.
-- idparametro no es serial: se toma el siguiente libre. Idempotente (se puede correr dos veces).
INSERT INTO parametros (idparametro, nombre, descripcion, tipo, id)
SELECT n.siguiente,
       'ocultarEncabezadoTicketNoFiscal',
       'Si = el ticket de venta X (no fiscal) no imprime nombre ni slogans de la empresa',
       2,
       n.siguiente
FROM (SELECT COALESCE(MAX(idparametro), 0) + 1 AS siguiente FROM parametros) n
WHERE NOT EXISTS (SELECT 1 FROM parametros WHERE nombre = 'ocultarEncabezadoTicketNoFiscal');
