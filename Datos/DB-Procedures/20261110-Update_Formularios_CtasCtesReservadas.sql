USE [CarniSys]
GO

-- ============================================================================
-- Permisos de cuentas corrientes (2026-11-10, ver docs/DECISIONS.md "Asistente de
-- usuarios y permisos"). Espejo SQL Server del script de Postgres
-- DatosPostgres/DB-Migrations/20261110-Update_formularios_ctasctes_y_reservadas.sql.
--
--  1) idForm 17 "Ctas.Ctes General": si todavia no tiene formConsulta se le pone
--     formCtasCtes (el codigo busca esa clave) y un nombre/descripcion claros.
--  2) idForm 9003 "Ver cuentas corrientes reservadas" (formCtaCteReservada): nuevo.
--
-- ANTES DE CORRER (servidor SM y San Lorenzo): verificar en solo lectura
--   SELECT idForm, nombreForm, formConsulta, formEdicion FROM Formularios WHERE idForm IN (17, 9001, 9002, 9003);
--   SELECT MAX(idForm) FROM Formularios;
-- Si la fila 17 YA tiene formConsulta, este script no la toca, pero hay que revisar a quien
-- se le dio ese permiso: hasta hoy "ver reservadas" colgaba de el, y con este cambio pasa
-- a depender de la fila 9003 (habria que otorgarsela a mano).
-- Si idForm es IDENTITY en esa base, el INSERT falla: avisar antes de seguir.
--
-- IMPORTANTE al correr via sqlcmd: usar -f i:65001,o:65001 (UTF-8 explicito).
-- dbo.Formularios tiene RLS: hace falta el session context de admin (ver
-- 20260807-Update_Formularios_DescripcionCierresDeCaja.sql). Idempotente.
-- Reversion: UPDATE Formularios SET nombreForm = N'Ctas.Ctes General', formConsulta = NULL WHERE idForm = 17;
--            DELETE FROM PermisosUsuarios WHERE idForm = 9003; DELETE FROM Formularios WHERE idForm = 9003;
-- ============================================================================

EXEC sp_set_session_context 'EsAdminCarniSys', 1;

UPDATE Formularios
SET nombreForm = N'Cuentas corrientes',
    descripcion = N'Entrar a Cuentas corrientes (menú Finanzas) y ver la cuenta de cualquier persona. Días ver = cuántos días hacia atrás ve los movimientos. No incluye las cuentas reservadas (ver "Ver cuentas corrientes reservadas").',
    formConsulta = N'formCtasCtes'
WHERE idForm = 17 AND COALESCE(formConsulta, N'') = N'';

IF NOT EXISTS (SELECT 1 FROM Formularios WHERE idForm = 9003)
    INSERT INTO Formularios (idForm, nombreForm, descripcion, formConsulta, formEdicion)
    VALUES (9003, N'Ver cuentas corrientes reservadas',
            N'Permite ver completas las cuentas corrientes marcadas como reservadas y marcar o desmarcar a una persona como reservada. Sin este permiso se ve la persona pero no su saldo, y solo lo que cargó uno mismo desde que abrió su caja. Marcar solo una de las opciones (Puede ver). No hace falta Días atrás ni Alcance.',
            N'formCtaCteReservada', N'formCtaCteReservada');
GO
