-- Complementa 20260825b-Create_carnisys_sysadmin_bypass_role.sql: ese rol solo tenia GRANT
-- sobre empresas/sucursal/usuarios/corte/empresaparametros/cortepuntostocksucursal (RW) y
-- alicuotasiva (solo lectura). Falta "iva" -- catalogo de "Condicion frente al IVA" que
-- SystemAdministrationPg.ObtenerCondicionesIva() ahora lee (precarga el datalist de
-- AltaRapidaEmpresa.cshtml, ver docs/DECISIONS.md 2026-08-25). BYPASSRLS no exime del sistema
-- de grants de objeto -- sin este GRANT explicito, la query falla con "permission denied for
-- table iva" en cuanto se asume el rol via SET LOCAL ROLE.

GRANT SELECT ON iva TO carnisys_sysadmin_bypass;
