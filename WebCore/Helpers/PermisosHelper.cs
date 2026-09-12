using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebCore.Helpers
{
    // Port de Web/Helpers/PermisosHelper.cs (2026-09-09, ver docs/DECISIONS.md "Batch B: permisos
    // reales + operador de produccion") -- SOLO la parte de "operador de modulo" para la cuenta
    // compartida de produccion (ResolverOperadorModulo/RegistrarOperadorModulo/etc). El chequeo de
    // permiso en si (TienePermiso) NO se replica aca: cada controller de WebCore ya construye su
    // propia instancia de Negocio.Usuario (_oUsuarioN, via NegocioFactory.CrearUsuario) en el
    // constructor, y esta accesible directo como "_oUsuarioN.tienePermiso(user, permiso, fecha,
    // idCreador)" -- exactamente lo mismo que hace PermisosHelper.TienePermiso(session, ...) del
    // clasico por debajo, sin necesitar un wrapper nuevo.
    //
    // ASP.NET Core ISession solo guarda string/byte[] (a diferencia de HttpSessionStateBase, que
    // guardaba el objeto Entidades.Usuario tal cual) -- se serializa a JSON, mismo criterio ya
    // usado en VentasController.cs (RegistrarOperadorPOS/ObtenerOperadorPOS) antes de que existiera
    // este helper compartido. VentasController/ComprasController siguen con su propia copia
    // privada (ya portada y verificada en el Batch 5) -- no se migran a este helper en este mismo
    // cambio para no mezclar un refactor de codigo que ya funciona con el fix de los controllers
    // que nunca tuvieron nada (CLAUDE.md §5, "no refactorices de paso").
    public static class PermisosHelper
    {
        // Mismo criterio que el clasico: que controllers usan el mecanismo de "operador de
        // modulo" (cuenta compartida de produccion que se identifica UNA vez por sesion+modulo,
        // sin contraseña -- distinto del step-up CON contraseña de Ventas/Compras/PuntosExpendio,
        // que vive aparte en cada controller). Clasico solo lo usa en Movimientos/Stock/Elaborados
        // (Web/Controllers/SeleccionUsuarioController.cs) -- confirmado con grep, no hay
        // "SeleccionUsuario"/"EsUsuarioProduccion" en Reportes/Finanzas/Personas/Productos/Empresa.
        private static readonly HashSet<string> ControllersConSeleccionUsuario =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Movimientos", "Stock", "Elaborados" };

        private static string ClaveSessionOperadorModulo(string modulo) => "OperadorModulo_" + (modulo ?? "");

        public static void RegistrarOperadorModulo(ISession session, string modulo, Entidades.Usuario operador)
        {
            if (session == null || string.IsNullOrWhiteSpace(modulo) || operador == null) return;
            session.SetString(ClaveSessionOperadorModulo(modulo), JsonSerializer.Serialize(operador));
        }

        public static Entidades.Usuario ObtenerOperadorModulo(ISession session, string modulo)
        {
            if (session == null || string.IsNullOrWhiteSpace(modulo)) return null;
            var json = session.GetString(ClaveSessionOperadorModulo(modulo));
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Entidades.Usuario>(json);
        }

        public static void LimpiarOperadorModulo(ISession session, string modulo)
        {
            if (session == null || string.IsNullOrWhiteSpace(modulo)) return;
            session.Remove(ClaveSessionOperadorModulo(modulo));
        }

        // Port literal de Web/Helpers/PermisosHelper.cs:199-213 -- se llama desde
        // WebCore/Views/Shared/_Layout.cshtml y _LayoutPOS.cshtml en cada render (mismo criterio
        // que el clasico en _LayoutBase.cshtml/_LayoutPOS.cshtml): si hay un operador de "Ventas"
        // o "Compras" activo y el controller actual no es ese modulo, lo limpia ahi mismo.
        // Movimientos/Stock/Elaborados NO entran aca -- no usan este mecanismo de sesion, usan
        // SeleccionUsuarioController (ver ControllersConSeleccionUsuario arriba), que no necesita
        // limpieza porque no persiste nada entre requests (el idUsuarioCreador viaja en la URL,
        // no en Session).
        //
        // BUG real (2026-09-10, reporte del usuario: "Compras pierde el permiso al clickear
        // afuera pero Ventas lo tiene en memoria todo el tiempo"): el port original de este batch
        // (2026-09-09) solo incluyo "Compras" en este array, a diferencia del clasico
        // (ControllersPorModulo en Web/Helpers/PermisosHelper.cs:143-148), que siempre tuvo los
        // dos. VentasController.ObtenerOperadorModulo/RegistrarOperadorModulo (privados, mismo
        // formato de clave "OperadorModulo_"+modulo) nunca se limpiaban, asi que el operador de
        // Ventas quedaba autorizado el resto de la sesion sin importar a que otro modulo navegara.
        private static readonly string[] ControllersConOperadorModuloSesion = { "Compras", "Ventas" };

        public static void LimpiarOperadorModuloSiSalioDelModulo(ISession session, string controllerActual)
        {
            if (session == null) return;

            foreach (var modulo in ControllersConOperadorModuloSesion)
            {
                if (string.IsNullOrEmpty(session.GetString(ClaveSessionOperadorModulo(modulo)))) continue;
                if (!string.Equals(controllerActual ?? "", modulo, StringComparison.OrdinalIgnoreCase))
                    session.Remove(ClaveSessionOperadorModulo(modulo));
            }
        }

        // Item 7 (2026-09-12, ver docs/DECISIONS.md "gate de caja abierta"): "el permiso para
        // registrar ventas sin apertura de caja termina al salir de la vista del pos venta" --
        // mismo criterio que LimpiarOperadorModuloSiSalioDelModulo de arriba (llamado en cada
        // render desde _Layout.cshtml/_LayoutPOS.cshtml), pero para el flag de bypass de caja
        // (VentasController.ClaveSessionBypassCaja/ContinuarSinCajaAbierta/TieneBypassCajaActivo
        // -- clave duplicada aca a proposito, mismo criterio de duplicacion ya establecido en este
        // archivo para OperadorModulo/OperadorPOS, cada controller mantiene su propia copia
        // privada para su uso interno). Simplificacion aceptada: usa el Id del usuario de sesion,
        // no el operador real resuelto por posInstanceId (cuentas de produccion) -- los layouts no
        // tienen ese dato facilmente disponible, y el caso de una cuenta de produccion usando
        // ESTE bypass puntual es marginal.
        private static string ClaveSessionBypassCajaPOS(int idOperador) => "BypassCajaPOS_" + idOperador;

        public static void LimpiarBypassCajaSiSalioDePOS(ISession session, string controllerActual, string actionActual, int idOperador)
        {
            if (session == null || idOperador <= 0) return;

            bool enPOS = string.Equals(controllerActual ?? "", "Ventas", StringComparison.OrdinalIgnoreCase)
                && string.Equals(actionActual ?? "", "POS", StringComparison.OrdinalIgnoreCase);
            if (enPOS) return;

            session.Remove(ClaveSessionBypassCajaPOS(idOperador));
        }

        // ===== Ventana de dias por permiso (Ver/Editar) =====
        // Port literal de Web/Helpers/PermisosHelper.cs:263-299 (item 2, 2026-09-12, ver
        // docs/DECISIONS.md) -- mismo mecanismo ya usado por Negocio.Usuario.tienePermiso para
        // decidir SI un usuario tiene un permiso valido en una fecha dada; este helper devuelve
        // la FECHA MINIMA que ese mismo calculo permite, para poder acotar listados/filtros (no
        // solo un true/false por registro individual). null = admin, o el usuario no tiene el
        // permiso asignado (Formulario.FormConsulta/FormEdicion* no matchea ninguna fila de
        // usuario.Permisos), o el permiso esta configurado sin limite (DiasPermitidosVer/Editar
        // < 0) -- en los 3 casos, "sin restriccion de fecha", el llamador no debe filtrar nada.
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

        // Port de Web/Controllers/MovimientosController.cs:174-181 (mismo gate en Stock/
        // Elaborados) -- si la sesion es la cuenta de produccion y todavia no se identifico quien
        // esta operando (idUsuarioCreador<=0), redirige a la pantalla compartida "SeleccionUsuario"
        // (sin contraseña, port de Web/Controllers/SeleccionUsuarioController.cs). Devuelve null si
        // no hace falta redirigir (usuario normal, o ya viene con idUsuarioCreador>0).
        public static RedirectToActionResult RequiereSeleccionUsuario(
            Controller controller,
            Entidades.Usuario user,
            int idUsuarioCreador,
            string returnUrl)
        {
            if (user == null || !user.EsUsuarioProduccion || idUsuarioCreador > 0) return null;

            return controller.RedirectToAction("Index", "SeleccionUsuario", new
            {
                returnUrl,
                cancelUrl = controller.Url.Action("Index", controller.ControllerContext.ActionDescriptor.ControllerName)
            });
        }

        // ===== Autorizacion temporal de Cierre de Caja (step-up de credenciales) =====
        // Port de Web/Helpers/PermisosHelper.cs:11-79 (tercera ronda de pedidos, 2026-09-10, ver
        // docs/DECISIONS.md). Clasico guarda la elevacion en MemoryCache (proceso, no Session)
        // porque su CajasController tiene [SessionState(SessionStateBehavior.ReadOnly)] --
        // restriccion de ASP.NET Framework para no serializar las varias llamadas AJAX
        // concurrentes de esa pantalla por el lock exclusivo de sesion; ASP.NET Core no tiene ese
        // lock exclusivo (ISession no serializa requests entre si) y WebCore/Controllers/
        // CajasController.cs no tiene ningun atributo de sesion especial -- se usa ISession+JSON,
        // mismo patron ya establecido arriba (RegistrarOperadorModulo/RegistrarOperadorPOS).
        // Desviacion deliberada de MECANISMO respecto al clasico (no de comportamiento
        // observable), documentada explicitamente en vez de asumida en silencio.
        private const string ClaveSessionElevacionCierre = "CierreCajaElevacion";

        private sealed class ElevacionCierre
        {
            public Entidades.Usuario Usuario { get; set; }
            public DateTime ExpiraUtc { get; set; }
        }

        /// <summary>
        /// Registra que, para esta sesion, un usuario CON permiso de cerrar caja autorizo las
        /// acciones de Cierre de Caja por un tiempo limitado (step-up de credenciales).
        /// </summary>
        public static void RegistrarElevacionCierre(ISession session, Entidades.Usuario usuarioAutorizado, TimeSpan duracion)
        {
            if (session == null || usuarioAutorizado == null) return;

            var elevacion = new ElevacionCierre { Usuario = usuarioAutorizado, ExpiraUtc = DateTime.UtcNow.Add(duracion) };
            session.SetString(ClaveSessionElevacionCierre, JsonSerializer.Serialize(elevacion));
        }

        /// <summary>
        /// Revoca la elevacion temporal de Cierre de Caja de esta sesion -- se llama al navegar
        /// afuera de la vista (antes de que expire el tope de tiempo).
        /// </summary>
        public static void RevocarElevacionCierre(ISession session)
        {
            if (session == null) return;
            session.Remove(ClaveSessionElevacionCierre);
        }

        /// <summary>
        /// Usuario habilitado a operar sobre Cierre de Caja en esta sesion: el logueado si
        /// tienePermisoDirecto ya es true (ya lo calculo el controller contra Permisos.Caja.
        /// CerrarCaja), o el que autorizo via step-up si hay una elevacion vigente y no vencida.
        /// Null si ninguna de las dos aplica -- es la unica fuente de verdad de "quien puede
        /// actuar" para las acciones de Cierre de Caja, tambien determina el UsuarioCierre que se
        /// graba al cerrar la caja.
        /// </summary>
        public static Entidades.Usuario ObtenerUsuarioAutorizadoCierre(ISession session, Entidades.Usuario usuarioLogueado, bool tienePermisoDirecto)
        {
            if (usuarioLogueado != null && tienePermisoDirecto) return usuarioLogueado;
            if (session == null) return null;

            var json = session.GetString(ClaveSessionElevacionCierre);
            if (string.IsNullOrEmpty(json)) return null;

            var elevacion = JsonSerializer.Deserialize<ElevacionCierre>(json);
            if (elevacion == null || elevacion.ExpiraUtc <= DateTime.UtcNow)
            {
                session.Remove(ClaveSessionElevacionCierre);
                return null;
            }

            return elevacion.Usuario;
        }
    }
}
