// Elevaciones de formula secreta en la sesion web (2026-10-04, ver docs/DECISIONS.md "Formula secreta
// con re-login"). Una "elevacion" dice: en esta sesion, la formula del producto elaborado X es visible
// porque un usuario con permiso se re-autentico (ver ElaboradosController.AutorizarVerFormula). Vence
// sola a los Security:FormulaElevacionMinutos y se revoca con "Ocultar" o al salir de la pantalla.
// La logica del estado vive en Negocio.FormulaSecretaElevaciones (con tests); aca solo se persiste en
// ISession. Se guarda unicamente idCorte + idUsuario + vencimiento, nunca el objeto Usuario (a diferencia
// de la elevacion de Cierre de Caja, que serializa el Usuario entero con hash y salt).
using System.Globalization;

namespace WebCore.Helpers
{
    public static class FormulaSecretaHelper
    {
        private const string ClaveSession = "FormulaSecretaElevaciones";
        private const string ClaveConfigMinutos = "Security:FormulaElevacionMinutos";
        private const int MinutosPorDefecto = 5;

        public static TimeSpan DuracionElevacion()
        {
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings[ClaveConfigMinutos],
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutos))
                minutos = MinutosPorDefecto;

            return TimeSpan.FromMinutes(Math.Min(Math.Max(minutos, 1), 60));
        }

        public static void Registrar(ISession session, int idCorte, int idUsuarioAutorizador)
        {
            if (session == null || idCorte <= 0) return;

            string estado = session.GetString(ClaveSession) ?? "";
            session.SetString(ClaveSession,
                Negocio.FormulaSecretaElevaciones.Registrar(estado, idCorte, idUsuarioAutorizador, DateTime.UtcNow, DuracionElevacion()));
        }

        public static bool TieneElevacion(ISession session, int idCorte)
        {
            if (session == null || idCorte <= 0) return false;
            return Negocio.FormulaSecretaElevaciones.Vigente(session.GetString(ClaveSession) ?? "", idCorte, DateTime.UtcNow);
        }

        // true si habia una elevacion vigente para ese producto (y se quito).
        public static bool Revocar(ISession session, int idCorte)
        {
            if (session == null || idCorte <= 0) return false;

            string estado = session.GetString(ClaveSession) ?? "";
            string nuevo = Negocio.FormulaSecretaElevaciones.Revocar(estado, idCorte, DateTime.UtcNow, out bool existia);
            if (string.IsNullOrEmpty(nuevo)) session.Remove(ClaveSession); else session.SetString(ClaveSession, nuevo);
            return existia;
        }

        // Quita todas; devuelve los productos que estaban vigentes (para auditar un OCULTAR por cada uno).
        public static IReadOnlyList<int> RevocarTodas(ISession session)
        {
            if (session == null) return Array.Empty<int>();

            string estado = session.GetString(ClaveSession) ?? "";
            var vigentes = Negocio.FormulaSecretaElevaciones.CortesVigentes(estado, DateTime.UtcNow);
            session.Remove(ClaveSession);
            return vigentes;
        }

        public static IReadOnlyList<int> CortesVigentes(ISession session)
        {
            if (session == null) return Array.Empty<int>();
            return Negocio.FormulaSecretaElevaciones.CortesVigentes(session.GetString(ClaveSession) ?? "", DateTime.UtcNow);
        }
    }
}
