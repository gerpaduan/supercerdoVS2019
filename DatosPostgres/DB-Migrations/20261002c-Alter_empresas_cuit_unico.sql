-- CUIT unico por empresa (2026-10-02, ver docs/DECISIONS.md "Login por CUIT, clave rapida (PIN) y
-- politica de clave"). El login por CUIT (/Login/{cuit}) resuelve la empresa con
-- ISucursalRepository.findEmpresaByCuit, que devuelve la primera fila: con dos empresas con el mismo
-- CUIT el login seria ambiguo. Hasta ahora la unicidad solo se chequeaba en la aplicacion
-- (SystemAdministrationPg.ExisteCuit), sin indice. Correr como carnisys_admin.
--
-- Defensivo: si YA hay CUIT duplicados NO crea el indice (no rompe la migracion ni modifica datos)
-- y lo avisa con un NOTICE; hay que resolver los duplicados a mano y volver a correr el script.
-- Verificar los datos reales de cada base antes de correrlo:
--   SELECT cuit, count(*) FROM empresas WHERE cuit IS NOT NULL AND cuit <> 0 GROUP BY cuit HAVING count(*) > 1;
-- (en SQL Server no aplica: una sola empresa por base.)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM empresas WHERE cuit IS NOT NULL AND cuit <> 0 GROUP BY cuit HAVING count(*) > 1) THEN
        RAISE NOTICE 'Hay CUIT duplicados en empresas: NO se creo ux_empresas_cuit. Resolver los duplicados y volver a correr este script.';
    ELSE
        CREATE UNIQUE INDEX IF NOT EXISTS ux_empresas_cuit ON empresas (cuit) WHERE cuit IS NOT NULL AND cuit <> 0;
    END IF;
END $$;
