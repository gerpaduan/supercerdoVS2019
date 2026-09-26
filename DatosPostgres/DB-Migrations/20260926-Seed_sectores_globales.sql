-- Sectores globales del Punto de Expendio: PRESUPUESTO y REMITOS (2026-09-26, ver docs/DECISIONS.md).
-- Se guardan UNA vez con idempresa = 0: la politica RLS de "sectores" deja leer las filas con
-- idempresa = 0 desde cualquier empresa (mismo mecanismo que Personas/Corte/Licencias), asi que
-- todas las empresas (actuales y futuras) los ven sin sembrar nada por empresa. No se pueden
-- modificar ni eliminar: lo impide el codigo (Negocio.SectorPuntoExpendio) y VentaPg filtra
-- "idempresa <> 0" en UPDATE/DELETE. Idempotente: se puede correr mas de una vez.
-- Correr conectado a 'carnisys' con el rol carnisys_admin (BYPASSRLS).

INSERT INTO sectores (sector, idempresa)
SELECT v.sector, 0
FROM (VALUES ('PRESUPUESTO'), ('REMITOS')) AS v(sector)
WHERE NOT EXISTS (
    SELECT 1 FROM sectores s WHERE s.idempresa = 0 AND upper(s.sector) = v.sector
);

-- OPCIONAL (no se ejecuta por defecto): las empresas que ya tenian su propio sector
-- "Presupuesto" quedan con esa fila duplicada. Es inofensiva (la lista de sectores no repite
-- nombres y el nombre esta reservado, no se puede editar ni borrar desde la app), y borrarla no
-- pierde historial porque expendios.sector guarda el texto. Si se quiere limpiar:
--   DELETE FROM sectores WHERE idempresa <> 0 AND upper(sector) IN ('PRESUPUESTO', 'REMITOS');
