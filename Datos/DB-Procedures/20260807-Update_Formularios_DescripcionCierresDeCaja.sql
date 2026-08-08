USE [CarniSys]
GO

-- ============================================================================
-- Actualiza la descripcion de Formularios.idForm=9 (formCierresDeCaja) para
-- explicar la regla nueva: "Puede ver" el historial de cierres arrastra
-- automaticamente "Puede editar" (modificar un cierre historico), con los
-- mismos dias atras. Ver docs/09-cambios-y-pendientes/bitacora-de-cambios.md,
-- 2026-08-07.
--
-- IMPORTANTE al correr este script via sqlcmd: usar -f i:65001,o:65001 (UTF-8
-- explicito) -- sin eso, sqlcmd corrompe las tildes de N'...' (doble-encoding
-- UTF-8/Windows-1252, ya visto y documentado en la ronda del 2026-08-07).
-- ============================================================================

-- dbo.Formularios tiene RLS: sin el session context de admin, sa no ve/actualiza
-- la fila (confirmado en local -- el UPDATE corre sin error pero "0 rows affected").
EXEC sp_set_session_context 'EsAdminCarniSys', 1;

UPDATE Formularios
SET descripcion = N'Historial de todos los cierres de caja. "Puede ver" respeta los días atrás ingresados. Al otorgar "Puede ver" se habilita también la edición (modificar un cierre histórico), con los mismos días atrás.'
WHERE idForm = 9;
GO
