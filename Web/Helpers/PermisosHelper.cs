using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using Utilidades;

namespace Web.Helpers
{
    public static class PermisosHelper
    {
        // ===== Autorizacion temporal de Cierre de Caja (step-up de credenciales) =====
        //
        // CajasController tiene [SessionState(SessionStateBehavior.ReadOnly)] (deliberado,
        // evita que las varias llamadas AJAX concurrentes de esa pantalla se serialicen por
        // el lock exclusivo de sesion de ASP.NET) -- escribir en Session[...] desde ahi no
        // persiste de forma confiable entre requests. Por eso la elevacion temporal se guarda
        // en MemoryCache (proceso, no session), con clave por Session.SessionID.
        private const string PrefijoCacheElevacionCierre = "CierreCajaElevacion:";

        private static string ClaveCacheElevacionCierre(string sessionId)
        {
            return PrefijoCacheElevacionCierre + (sessionId ?? "");
        }

        /// <summary>
        /// Registra que, para esta sesion, un usuario CON permiso de cerrar caja autorizo
        /// las acciones de Cierre de Caja por un tiempo limitado (step-up de credenciales).
        /// </summary>
        public static void RegistrarElevacionCierre(HttpSessionStateBase session, Entidades.Usuario usuarioAutorizado, TimeSpan duracion)
        {
            if (session == null || usuarioAutorizado == null)
                return;

            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(duracion) };
            MemoryCache.Default.Set(ClaveCacheElevacionCierre(session.SessionID), usuarioAutorizado, policy);
        }

        /// <summary>
        /// Revoca la elevacion temporal de Cierre de Caja de esta sesion -- se llama al
        /// navegar afuera de la vista (antes de que expire el tope de tiempo).
        /// </summary>
        public static void RevocarElevacionCierre(HttpSessionStateBase session)
        {
            if (session == null)
                return;

            MemoryCache.Default.Remove(ClaveCacheElevacionCierre(session.SessionID));
        }

        /// <summary>
        /// Usuario habilitado a operar sobre Cierre de Caja en esta sesion: el logueado si
        /// ya tiene el permiso directo, o el que autorizo via step-up si hay una elevacion
        /// vigente. Null si ninguna de las dos aplica. Es la unica fuente de verdad de "quien
        /// puede actuar" para las acciones de Cierre de Caja -- tambien determina el
        /// UsuarioCierre que se graba al cerrar la caja.
        /// </summary>
        public static Entidades.Usuario ObtenerUsuarioAutorizadoCierre(HttpSessionStateBase session)
        {
            var usuarioLogueado = ObtenerUsuario(session);
            if (usuarioLogueado != null && TienePermisoVer(session, PermisosPantallasWeb.Cajas.CerrarCaja))
                return usuarioLogueado;

            if (session == null)
                return null;

            return MemoryCache.Default.Get(ClaveCacheElevacionCierre(session.SessionID)) as Entidades.Usuario;
        }

        /// <summary>
        /// De aquí se llama a Negocio.Usuario
        /// Se valida si el oUsuario tiene permiso en el formulario, por defecto pasar Fecha Actual,
        /// idCreador, pasar -1 si no se quiere verificar la Edicion
        /// </summary>
        /// <param name="oUser"></param>
        /// <param name="nombreForm"></param>
        /// <param name="fechaDesde"></param>
        /// <returns></returns>
        public static bool TienePermiso(HttpSessionStateBase session, string permiso, DateTime? fechaDesde, int idCreador = -1)
        {
            var fechaDesde_ = fechaDesde ?? DateTime.Today;
            var user = ObtenerUsuario(session);

            if (user == null)
                return false;

            // EmpresaContextWeb lee Session["IdEmpresa"]
            IEmpresaContext empresa = new EmpresaContextWeb();

            return TienePermiso(user, empresa, permiso, fechaDesde_, idCreador);
        }

        public static bool TienePermisoVer(HttpSessionStateBase session, string permiso, DateTime? fechaDesde = null)
        {
            return TienePermiso(session, permiso, fechaDesde, -1);
        }

        public static bool TienePermisoEditar(HttpSessionStateBase session, string permiso, DateTime fechaDesde, int idCreador)
        {
            return TienePermiso(session, permiso, fechaDesde, idCreador);
        }

        public static bool TienePermiso(Entidades.Usuario user, IEmpresaContext empresa, string permiso, DateTime fechaDesde, int idCreador = -1)
        {
            if (user == null || empresa == null)
                return false;

            var oUsuarioN = new Negocio.Usuario(empresa);

            return oUsuarioN.tienePermiso(
                user,
                permiso,
                fechaDesde,
                idCreador
            );
        }

        public static DateTime? ObtenerFechaMinimaPermitida(HttpSessionStateBase session, string permiso, int idCreador = -1)
        {
            return ObtenerFechaMinimaPermitida(ObtenerUsuario(session), permiso, idCreador);
        }

        public static DateTime? ObtenerFechaMinimaPermitida(Entidades.Usuario user, string permiso, int idCreador = -1)
        {
            if (user == null || user.Admin || user.Permisos == null || user.Permisos.Count == 0)
                return null;

            string permisoNormalizado = (permiso ?? string.Empty).Trim().ToUpperInvariant();
            bool esEdicion = idCreador >= 0;

            var permisoUsuario = user.Permisos.FirstOrDefault(p =>
            {
                if (p == null || p.Formulario == null)
                    return false;

                if (esEdicion)
                {
                    return string.Equals(p.Formulario.FormEdicion ?? string.Empty, permisoNormalizado, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(p.Formulario.FormEdicionExtra1 ?? string.Empty, permisoNormalizado, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(p.Formulario.FormEdicionExtra2 ?? string.Empty, permisoNormalizado, StringComparison.OrdinalIgnoreCase);
                }

                return string.Equals(p.Formulario.FormConsulta ?? string.Empty, permisoNormalizado, StringComparison.OrdinalIgnoreCase);
            });

            if (permisoUsuario == null)
                return null;

            int dias = esEdicion ? permisoUsuario.DiasPermitidosEditar : permisoUsuario.DiasPermitidosVer;
            if (dias < 0)
                return null;

            return DateTime.Today.AddDays(-dias).Date;
        }

        public static Entidades.Usuario ObtenerUsuario(HttpSessionStateBase session)
        {
            return session == null ? null : session["Usuario"] as Entidades.Usuario;
        }
    }
}
