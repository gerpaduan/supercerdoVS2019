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
        public static Entidades.Usuario ObtenerUsuarioAutorizadoCierre(HttpSessionStateBase session, string posInstanceId = null)
        {
            var usuarioLogueado = ObtenerUsuario(session);
            if (usuarioLogueado != null && TienePermisoVer(session, PermisosPantallasWeb.Cajas.CerrarCaja))
                return usuarioLogueado;

            if (session == null)
                return null;

            var elevado = MemoryCache.Default.Get(ClaveCacheElevacionCierre(session.SessionID)) as Entidades.Usuario;
            if (elevado != null)
                return elevado;

            // Si la accion se dispara desde un POS donde ya hay un operador de produccion
            // autorizado (ver RegistrarOperadorPOS abajo), ese operador tambien puede cerrar
            // caja sin pedir contraseña de nuevo -- ya se autentico al entrar al POS. Parametro
            // opcional: los llamados existentes de Cierre de Caja (fuera de POS) no lo pasan y
            // se comportan exactamente igual que antes.
            if (!string.IsNullOrWhiteSpace(posInstanceId))
                return ObtenerOperadorPOS(session, posInstanceId);

            return null;
        }

        // ===== Operador de POS para la cuenta compartida "usuario de produccion" =====
        //
        // VentasController/PuntosExpendioController NO tienen [SessionState(ReadOnly)] (a
        // diferencia de CajasController arriba), asi que Session[...] persiste normalmente
        // entre requests -- no hace falta MemoryCache aca. Clave por posInstanceId (no solo
        // por sesion): cada pestaña de POS duplicada tiene su propio operador, no se pisan
        // entre si. posInstanceId viaja en la URL de POS y sobrevive a un F5 (ver
        // pos-multi-instance.js) pero es nuevo (GUID) cada vez que se entra de cero a la
        // vista -- por eso esto "dura hasta cerrar la vista" sin necesitar expiracion por
        // tiempo: una vista nueva nunca matchea una clave vieja.
        private static string ClaveSessionOperadorPOS(string posInstanceId)
        {
            return "OperadorPOS_" + (posInstanceId ?? "");
        }

        /// <summary>
        /// Registra que, para esta instancia de POS (pestaña), un usuario real autorizo con su
        /// propia contraseña operar en nombre de la cuenta compartida de produccion.
        /// </summary>
        public static void RegistrarOperadorPOS(HttpSessionStateBase session, string posInstanceId, Entidades.Usuario operador)
        {
            if (session == null || string.IsNullOrWhiteSpace(posInstanceId) || operador == null)
                return;

            session[ClaveSessionOperadorPOS(posInstanceId)] = operador;
        }

        /// <summary>
        /// Operador real autorizado para esta instancia de POS, o null si todavia no se
        /// autorizo ninguno (la vista debe pedir usuario+contraseña).
        /// </summary>
        public static Entidades.Usuario ObtenerOperadorPOS(HttpSessionStateBase session, string posInstanceId)
        {
            if (session == null || string.IsNullOrWhiteSpace(posInstanceId))
                return null;

            return session[ClaveSessionOperadorPOS(posInstanceId)] as Entidades.Usuario;
        }

        /// <summary>
        /// Limpia el operador de esta instancia de POS -- se llama al cerrar la vista (beacon
        /// de beforeunload/pagehide) para no dejar la identidad del operador colgada en
        /// Session. Es higiene, no la garantia real de "vuelve a pedir credenciales" (esa la
        /// da que cada apertura nueva de POS usa un posInstanceId distinto).
        /// </summary>
        public static void LimpiarOperadorPOS(HttpSessionStateBase session, string posInstanceId)
        {
            if (session == null || string.IsNullOrWhiteSpace(posInstanceId))
                return;

            session.Remove(ClaveSessionOperadorPOS(posInstanceId));
        }

        // ===== Operador de modulo para la cuenta compartida "usuario de produccion" =====
        //
        // Mismo concepto que "Operador de POS" arriba, pero para pantallas de consulta
        // (Ventas/Index, DetalleVenta, Lineas) en vez de POS -- por eso NO hay posInstanceId:
        // no existe "pestaña duplicada" para un listado/reporte, una sola clave por sesion
        // alcanza. La duracion no depende de una vista con id nuevo cada vez (como en POS),
        // asi que la limpieza corre desde los layouts compartidos (_LayoutBase.cshtml /
        // _LayoutPOS.cshtml, via LimpiarOperadorModuloSiSalioDelModulo) apenas el usuario
        // navega a un controller fuera del modulo autorizado -- ver docs/DECISIONS.md.
        private static readonly System.Collections.Generic.Dictionary<string, string> ControllersPorModulo =
            new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Ventas", "Ventas" },
                { "Compras", "Compras" }
            };

        private static string ClaveSessionOperadorModulo(string modulo)
        {
            return "OperadorModulo_" + (modulo ?? "");
        }

        /// <summary>
        /// Registra que, para esta sesion, un usuario real autorizo con su propia contraseña
        /// operar en nombre de la cuenta compartida de produccion dentro de un modulo (ej.
        /// "Ventas").
        /// </summary>
        public static void RegistrarOperadorModulo(HttpSessionStateBase session, string modulo, Entidades.Usuario operador)
        {
            if (session == null || string.IsNullOrWhiteSpace(modulo) || operador == null)
                return;

            session[ClaveSessionOperadorModulo(modulo)] = operador;
        }

        /// <summary>
        /// Operador real autorizado para este modulo en esta sesion, o null si todavia no se
        /// autorizo ninguno (la vista debe pedir usuario+contraseña).
        /// </summary>
        public static Entidades.Usuario ObtenerOperadorModulo(HttpSessionStateBase session, string modulo)
        {
            if (session == null || string.IsNullOrWhiteSpace(modulo))
                return null;

            return session[ClaveSessionOperadorModulo(modulo)] as Entidades.Usuario;
        }

        /// <summary>
        /// Limpia el operador de este modulo.
        /// </summary>
        public static void LimpiarOperadorModulo(HttpSessionStateBase session, string modulo)
        {
            if (session == null || string.IsNullOrWhiteSpace(modulo))
                return;

            session.Remove(ClaveSessionOperadorModulo(modulo));
        }

        /// <summary>
        /// Se llama desde _LayoutBase.cshtml y _LayoutPOS.cshtml en CADA render (de cualquier
        /// pagina de la app) -- si hay un operador de modulo activo y el controller que se esta
        /// ejecutando no pertenece a ese modulo, lo limpia ahi mismo. Es la forma en que el
        /// operador "pierde el permiso al hacer clic afuera" sin depender de que el navegador
        /// dispare nada (nada de beacon/pagehide aca) ni de tocar el filtro global de todos los
        /// controllers -- el propio layout compartido ya lee Session para otras cosas.
        /// </summary>
        public static void LimpiarOperadorModuloSiSalioDelModulo(HttpSessionStateBase session, string controllerActual)
        {
            if (session == null)
                return;

            foreach (var modulo in ControllersPorModulo.Keys)
            {
                if (session[ClaveSessionOperadorModulo(modulo)] == null)
                    continue;

                string controllerDelModulo = ControllersPorModulo[modulo];
                if (!string.Equals(controllerActual ?? "", controllerDelModulo, StringComparison.OrdinalIgnoreCase))
                    session.Remove(ClaveSessionOperadorModulo(modulo));
            }
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
