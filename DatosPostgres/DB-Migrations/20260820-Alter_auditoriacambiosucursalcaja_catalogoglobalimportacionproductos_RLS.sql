-- Cierra 2 gaps de RLS encontrados en la auditoria de produccion-readiness (2026-08-20):
-- auditoriacambiosucursalcaja y catalogoglobalimportacionproductos tienen idempresa pero
-- no tenian politica RLS -- el codigo ya filtra bien a mano (defensa en profundidad, no una
-- fuga activa), pero rompia el principio de "aislamiento desde el dia 1" del stack.
-- Mismo patron ya usado en el resto de las tablas (ver corte_rls). Ver docs/DECISIONS.md.

ALTER TABLE auditoriacambiosucursalcaja ENABLE ROW LEVEL SECURITY;

CREATE POLICY auditoriacambiosucursalcaja_rls ON auditoriacambiosucursalcaja
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND (idempresa = current_setting('app.id_empresa', true)::integer OR idempresa = 0)
    )
    WITH CHECK (
        idempresa = current_setting('app.id_empresa', true)::integer
    );

ALTER TABLE catalogoglobalimportacionproductos ENABLE ROW LEVEL SECURITY;

CREATE POLICY catalogoglobalimportacionproductos_rls ON catalogoglobalimportacionproductos
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND (idempresa = current_setting('app.id_empresa', true)::integer OR idempresa = 0)
    )
    WITH CHECK (
        idempresa = current_setting('app.id_empresa', true)::integer
    );
