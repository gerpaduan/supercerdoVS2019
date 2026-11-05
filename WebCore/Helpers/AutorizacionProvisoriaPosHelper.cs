// Autorizaciones provisorias del POS en la sesion web (2026-11-03, ver docs/DECISIONS.md "POS: autorizacion provisoria
// de otro usuario"). Cuando el operador no tiene un permiso, otro usuario que SI lo tiene lo autoriza con su usuario y
// clave (VentasController.AutorizarAccionPOS) y la accion queda habilitada hasta que se cierra la ventana que la usa.
// La logica del estado vive en Negocio.AutorizacionesProvisoriasPos (con tests); aca se persiste en ISession y se deja
// constancia en la auditoria (Contratos.IAutorizacionPosRepository). Se guarda solo ids y vencimiento, nunca al usuario.
// Ventas y Finanzas usan este mismo helper para que las dos pantallas decidan igual.
using System.Globalization;
using Negocio;

namespace WebCore.Helpers
{
    public static class AutorizacionProvisoriaPosHelper
    {
        private const string ClaveSession = "AutorizacionesProvisoriasPos";
        private const string ClaveConfigMinutos = "Security:AutorizacionPosMinutos";
        private const int MinutosPorDefecto = 30;

        // Vencimiento de respaldo: la autorizacion se cierra al cerrar la ventana de la accion, pero si ese aviso no
        // llega al servidor (navegador cerrado, corte) igual deja de valer sola.
        public static TimeSpan Duracion()
        {
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings[ClaveConfigMinutos],
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutos))
                minutos = MinutosPorDefecto;

            return TimeSpan.FromMinutes(Math.Min(Math.Max(minutos, 1), 240));
        }

        // El permiso que cada accion exige (mismo que ya frena al operador). La fecha de la venta o del pago y el
        // limite de caja los suma el llamador, porque dependen del registro.
        public static bool TienePermiso(Negocio.Usuario usuarioN, Entidades.Usuario usuario, string accion)
        {
            if (usuario == null || !usuario.Activo) return false;
            if (usuario.Admin) return true;

            switch (accion)
            {
                case AutorizacionesProvisoriasPos.AccionModificarVenta:
                    return usuarioN.tienePermiso(usuario, Entidades.Permisos.Venta.UltimaVenta, DateTime.Now, usuario.Id);
                case AutorizacionesProvisoriasPos.AccionBonificar:
                    return usuarioN.tienePermiso(usuario, Entidades.Permisos.Venta.Bonificar, DateTime.Today, -1);
                case AutorizacionesProvisoriasPos.AccionCtaCte:
                    return usuarioN.tienePermiso(usuario, Entidades.Permisos.Finanza.VerCtaCtePersona, DateTime.Today, -1);
                case AutorizacionesProvisoriasPos.AccionPago:
                    return usuarioN.tienePermiso(usuario, Entidades.Permisos.Finanza.AddOrEditPago, DateTime.Now, usuario.Id);
                case AutorizacionesProvisoriasPos.AccionMisVentas:
                    return usuarioN.tienePermiso(usuario, Entidades.Permisos.Venta.VerVentasVendedor, DateTime.Today, -1);
                default:
                    return false;
            }
        }

        // Quien autorizo esa accion en esa pestana, si la autorizacion sigue vigente Y esa persona sigue activa y con el
        // permiso (se vuelve a leer de la base: si se lo sacaron en el medio, deja de valer). null = no hay autorizacion.
        public static Entidades.Usuario Autorizante(ISession session, Negocio.Usuario usuarioN, string posInstanceId, string accion, int idRegistro)
        {
            if (session == null || string.IsNullOrEmpty(posInstanceId)) return null;

            var vigente = AutorizacionesProvisoriasPos.Vigente(
                session.GetString(ClaveSession) ?? "", posInstanceId, accion, idRegistro, DateTime.UtcNow);
            if (vigente == null) return null;

            var autorizante = usuarioN.getUsuarioConPermisos(vigente.IdAutorizador);
            return TienePermiso(usuarioN, autorizante, accion) ? autorizante : null;
        }

        // Deja constancia en la auditoria y habilita la accion. Si no se puede auditar lanza: falla cerrado, no se
        // autoriza nada que no quede registrado (mismo criterio que la formula secreta).
        public static void Registrar(ISession session, Contratos.IAutorizacionPosRepository repo,
            string posInstanceId, string accion, int idRegistro, int idOperador, int idAutorizador, int idUsuarioSesion)
        {
            if (repo == null)
                throw new InvalidOperationException("La auditoria de autorizaciones no esta disponible: no se puede autorizar.");

            string estado = session.GetString(ClaveSession) ?? "";
            estado = AutorizacionesProvisoriasPos.ExtraerVencidas(estado, DateTime.UtcNow, out var vencidas);
            CerrarAuditoria(repo, vencidas, "vencido");

            int idAuditoria = repo.Registrar(idOperador, idAutorizador, idUsuarioSesion, posInstanceId, accion, idRegistro);
            session.SetString(ClaveSession, AutorizacionesProvisoriasPos.Registrar(
                estado, posInstanceId, accion, idRegistro, idAutorizador, idAuditoria, DateTime.UtcNow, Duracion()));
        }

        // Cierra autorizaciones de una pestana. accion null = todas (al cerrar el POS); idRegistro null = cualquier
        // registro de esa accion. motivo: 'guardado' o 'cerrado'. No lanza: cerrar la ventana nunca debe fallar por la auditoria.
        public static void Revocar(ISession session, Contratos.IAutorizacionPosRepository repo,
            string posInstanceId, string accion, int? idRegistro, string motivo)
        {
            if (session == null || string.IsNullOrEmpty(posInstanceId)) return;

            string estado = session.GetString(ClaveSession) ?? "";
            if (string.IsNullOrEmpty(estado)) return;

            string nuevo = AutorizacionesProvisoriasPos.Revocar(estado, posInstanceId, accion, idRegistro, DateTime.UtcNow, out var ids);
            if (string.IsNullOrEmpty(nuevo)) session.Remove(ClaveSession); else session.SetString(ClaveSession, nuevo);

            CerrarAuditoria(repo, ids, motivo);
        }

        private static void CerrarAuditoria(Contratos.IAutorizacionPosRepository repo, IEnumerable<int> ids, string motivo)
        {
            if (repo == null) return;

            foreach (int id in ids)
            {
                try
                {
                    repo.Cerrar(id, motivo);
                }
                catch (Exception ex)
                {
                    // Cerrar la ventana no puede fallar por esto: se deja el aviso y la fila queda sin fecha de fin.
                    System.Diagnostics.Trace.TraceError("No se pudo cerrar la auditoria de autorizacion POS " + id + ": " + ex);
                }
            }
        }
    }
}
