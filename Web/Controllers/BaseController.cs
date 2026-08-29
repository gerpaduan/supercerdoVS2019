using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Utilidades;
using Web.Helpers;
using System.Collections.Generic;
using Entidades;

namespace Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IEmpresaContext empresa;
        protected IParametrosContext param;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            bool sesionSoloLectura = EsSesionSoloLectura(filterContext.HttpContext);
            bool sesionInvalida = session == null
                || session["Usuario"] == null
                || session["IdEmpresa"] == null;

            if (sesionInvalida)
            {
                if (session != null)
                {
                    session.Remove("PARAM_CTX");
                    session.Remove("IdEmpresa");
                    session.Remove("Usuario");
                }

                if (!sesionSoloLectura)
                    TempData["Error"] = "La sesión venció o faltan datos de contexto. Iniciá sesión nuevamente.";

                string returnUrl = filterContext.HttpContext.Request.RawUrl ?? "";
                filterContext.Result = RedirectToAction("Index", "Login", new { returnUrl = returnUrl });
                return;
            }

            object ubicacionValidada = session["UbicacionLoginValidada"];
            if (ubicacionValidada is bool && !(bool)ubicacionValidada)
            {
                string returnUrl = filterContext.HttpContext.Request.RawUrl ?? "";
                filterContext.Result = RedirectToAction("ValidarUbicacion", "Login", new { returnUrl = returnUrl });
                return;
            }

            empresa = new EmpresaContextWeb();

            param = Session["PARAM_CTX"] as IParametrosContext;
            if (param == null)
            {
                param = Web.Infrastructure.NegocioFactory.CrearParametros(empresa);
                param.Reload();

                if (!sesionSoloLectura)
                    Session["PARAM_CTX"] = param;
            }

            var usuario = Session["Usuario"] as Usuario;
            if (usuario != null)
            {
                var sucursalN = Web.Infrastructure.NegocioFactory.CrearSucursal(empresa, param);
                var sucursales = sucursalN.findAll() ?? new List<Sucursal>();
                ViewBag.Sucursales = sucursales;

                if (usuario.IdSucursal > 0)
                {
                    var sucursalActual = sucursales.Find(s => s != null && s.IdSucursal == usuario.IdSucursal)
                        ?? sucursalN.findById(usuario.IdSucursal);

                    if (sucursalActual != null)
                    {
                        usuario.Sucursal = sucursalActual;
                        usuario.SucursalNombre = sucursalActual.SucursalNombre;

                        if (!sesionSoloLectura)
                            Session["Usuario"] = usuario;
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        protected ActionResult VistaAccesoDenegado(string seccion, string permiso = null, System.DateTime? fecha = null, int idCreador = -1)
        {
            ViewBag.Title = seccion;
            ViewBag.Seccion = seccion;
            ViewBag.MensajePermiso = ConstruirMensajePermisoFecha(permiso, fecha, idCreador);
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        // Overload explicito, para cuando "quien opera" no es Session["Usuario"] (ej. el
        // operador resuelto para el modulo Ventas de la cuenta de produccion, ver
        // BaseController.ResolverOperadorModulo). Para un usuario normal, pasar ese mismo
        // usuario acá es identico a la version basada en Session.
        protected ActionResult VistaAccesoDenegado(string seccion, string permiso, System.DateTime? fecha, Entidades.Usuario usuarioExplicito, int idCreador = -1)
        {
            ViewBag.Title = seccion;
            ViewBag.Seccion = seccion;
            ViewBag.MensajePermiso = ConstruirMensajePermisoFecha(usuarioExplicito, permiso, fecha, idCreador);
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        protected string ConstruirMensajePermisoFecha(string permiso, System.DateTime? fecha, int idCreador = -1)
        {
            if (string.IsNullOrWhiteSpace(permiso) || !fecha.HasValue)
                return null;

            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);
            if (!fechaMinima.HasValue || fecha.Value.Date >= fechaMinima.Value.Date)
                return null;

            return idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
        }

        protected string ConstruirMensajePermisoFecha(Entidades.Usuario usuarioExplicito, string permiso, System.DateTime? fecha, int idCreador = -1)
        {
            if (string.IsNullOrWhiteSpace(permiso) || !fecha.HasValue)
                return null;

            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(usuarioExplicito, permiso, idCreador);
            if (!fechaMinima.HasValue || fecha.Value.Date >= fechaMinima.Value.Date)
                return null;

            return idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
        }

        protected bool AjustarFechaSiNoTienePermiso(string permiso, ref System.DateTime fecha, int idCreador = -1)
        {
            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);
            if (!fechaMinima.HasValue || fecha.Date >= fechaMinima.Value.Date)
                return false;

            fecha = fechaMinima.Value.Date;
            if (!EsSesionSoloLectura(HttpContext))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = idCreador >= 0
                    ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                    : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
            }
            return true;
        }

        protected bool AjustarFechaSiNoTienePermiso(Entidades.Usuario usuarioExplicito, string permiso, ref System.DateTime fecha, int idCreador = -1)
        {
            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(usuarioExplicito, permiso, idCreador);
            if (!fechaMinima.HasValue || fecha.Date >= fechaMinima.Value.Date)
                return false;

            fecha = fechaMinima.Value.Date;
            if (!EsSesionSoloLectura(HttpContext))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = idCreador >= 0
                    ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                    : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
            }
            return true;
        }

        protected bool AjustarFechaIndiceSegunLimiteYPermiso(string permiso, ref System.DateTime fecha, System.DateTime fechaLimiteSinPermiso, int idCreador = -1, bool mostrarAviso = true)
        {
            var limite = fechaLimiteSinPermiso.Date;
            var fechaAdvertencia = ObtenerFechaAdvertenciaIndice(permiso, fechaLimiteSinPermiso, idCreador);
            if (fecha.Date >= limite)
                return false;

            if (PermisosHelper.TienePermiso(Session, permiso, fecha, idCreador))
                return false;

            fecha = fechaAdvertencia;
            if (mostrarAviso && !EsSesionSoloLectura(HttpContext))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = "No tiene permiso para ingresar una fecha desde menor a " + fechaAdvertencia.ToString("dd/MM/yyyy") + ".";
            }

            return true;
        }

        protected void ConfigurarAdvertenciaFechaEnVivo(string inputId, string permiso, int idCreador = -1)
        {
            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);
            if (!fechaMinima.HasValue)
                return;

            ViewBag.PermisoFechaInputId = inputId;
            ViewBag.PermisoFechaMinimaIso = fechaMinima.Value.ToString("yyyy-MM-dd");
            ViewBag.PermisoFechaMinimaIsoDateTime = fechaMinima.Value.ToString("yyyy-MM-ddT00:00:00");
            ViewBag.PermisoFechaMensaje = idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
        }

        protected void ConfigurarAdvertenciaFechaEnVivo(Entidades.Usuario usuarioExplicito, string inputId, string permiso, int idCreador = -1)
        {
            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(usuarioExplicito, permiso, idCreador);
            if (!fechaMinima.HasValue)
                return;

            ViewBag.PermisoFechaInputId = inputId;
            ViewBag.PermisoFechaMinimaIso = fechaMinima.Value.ToString("yyyy-MM-dd");
            ViewBag.PermisoFechaMinimaIsoDateTime = fechaMinima.Value.ToString("yyyy-MM-ddT00:00:00");
            ViewBag.PermisoFechaMensaje = idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
        }

        protected void ConfigurarAdvertenciaFechaIndiceConLimiteEnVivo(string inputId, string permiso, System.DateTime fechaLimiteSinPermiso, int idCreador = -1)
        {
            var limite = fechaLimiteSinPermiso.Date;
            var fechaAdvertencia = ObtenerFechaAdvertenciaIndice(permiso, fechaLimiteSinPermiso, idCreador);
            if (PermisosHelper.TienePermiso(Session, permiso, limite.AddDays(-1), idCreador))
                return;

            ViewBag.PermisoFechaInputId = inputId;
            ViewBag.PermisoFechaMinimaIso = fechaAdvertencia.ToString("yyyy-MM-dd");
            ViewBag.PermisoFechaMinimaIsoDateTime = fechaAdvertencia.ToString("yyyy-MM-ddT00:00:00");
            ViewBag.PermisoFechaMensaje = "No tiene permiso para ingresar una fecha desde menor a " + fechaAdvertencia.ToString("dd/MM/yyyy") + ".";
        }

        protected System.DateTime ObtenerFechaAdvertenciaIndice(string permiso, System.DateTime fechaLimiteSinPermiso, int idCreador = -1)
        {
            var limite = fechaLimiteSinPermiso.Date;
            var fechaPermiso = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);

            if (fechaPermiso.HasValue && fechaPermiso.Value.Date < limite)
                return fechaPermiso.Value.Date;

            return limite;
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (!string.IsNullOrWhiteSpace(viewName))
            {
                ViewData.Model = model;
            }

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                if (viewResult.View == null)
                    throw new InvalidOperationException("No se encontró la vista parcial '" + viewName + "'.");

                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        // Lista liviana (solo id + nombre, nada sensible) de usuarios activos de la empresa
        // actual, para el modal de seleccion de usuario (_ModalSeleccionUsuario.cshtml /
        // seleccion-usuario.js) -- usado tanto por el step-up de Cierre de Caja (con password)
        // como por el selector sin password de la sala de produccion.
        protected List<object> ObtenerUsuariosActivosEmpresaParaCombo()
        {
            var oUsuarioN = Web.Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            var dt = oUsuarioN.obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre"))
                return new List<object>();

            return dt.AsEnumerable()
                .Select(row => new { id = ValorInt(row, "id"), nombre = ValorString(row, "nombre") })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }

        // Resuelve quien queda como creador real de una operacion cuando el usuario logueado es
        // el usuario compartido de sala de produccion: si llego un idUsuarioCreador valido
        // (activo, misma empresa que la sesion -- nunca se confia ciegamente en lo que manda el
        // cliente), se usa ese usuario en vez del de sesion. Para cualquier usuario normal, o si
        // no llego el parametro, devuelve el usuario de sesion sin cambios -- cero impacto en el
        // comportamiento actual fuera de la sala de produccion.
        protected Entidades.Usuario ResolverUsuarioCreador(int idUsuarioCreador, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion || idUsuarioCreador <= 0)
                return usuarioSesion;

            var oUsuarioN = Web.Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            var candidato = oUsuarioN.getUsuarioById(idUsuarioCreador);
            if (candidato == null || !candidato.Activo || candidato.IdEmpresa != usuarioSesion.IdEmpresa)
                return usuarioSesion;

            return candidato;
        }

        // Resuelve quien opera realmente dentro de una instancia de POS (posInstanceId) cuando
        // el usuario logueado es la cuenta compartida de produccion: si ya se autoriz un
        // operador real para esta instancia (ver PermisosHelper.RegistrarOperadorPOS), se usa
        // ese en vez del de sesion -- gobierna tanto la atribucion (Vendedor/UsuarioInicio/
        // UsuarioCierre) como los permisos finos dentro de POS (Bonificar, anular, etc.), que
        // se chequean pasando el resultado de esto al overload de PermisosHelper.TienePermiso
        // que recibe el usuario explicito. Para cualquier usuario normal devuelve el mismo
        // usuario de sesion sin cambios -- cero impacto fuera de la cuenta de produccion.
        protected Entidades.Usuario ResolverOperadorPOS(string posInstanceId, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion)
                return usuarioSesion;

            return PermisosHelper.ObtenerOperadorPOS(Session, posInstanceId) ?? usuarioSesion;
        }

        // Step-up de credenciales para que la cuenta compartida de produccion pueda operar
        // POS Venta / POS Expendio: un usuario real tipea su propia contraseña. Mismo patron que
        // CajasController.AutorizarAccionCierre, pero guardado en Session (VentasController/
        // PuntosExpendioController no tienen [SessionState(ReadOnly)]) y sin expiracion por
        // tiempo -- dura hasta que se cierre la vista de POS (posInstanceId nuevo en cada
        // apertura fresca, ver PermisosHelper).
        // exigirPermisoVentas: false en POS Expendio (decision del usuario, ver DECISIONS.md --
        // cualquier usuario activo puede operar Expendio, no solo quien tiene "Ventas > Editar").
        // POS Venta sigue exigiendolo (true, default) -- ahi si implica una venta real.
        protected JsonResult ValidarOperadorPOS(int idUsuario, string clave, string posInstanceId, bool exigirPermisoVentas = true)
        {
            string sessionId = Session.SessionID;

            TimeSpan retryAfter;
            if (PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out retryAfter))
            {
                return Json(new
                {
                    ok = false,
                    bloqueado = true,
                    segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds)
                });
            }

            const string mensajeGenerico = "Usuario o contraseña incorrectos, o el usuario no tiene permiso de Ventas.";

            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(posInstanceId))
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var oUsuarioN = Web.Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            var candidato = oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var validado = oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            // BUG real encontrado: sin el 5to parametro (idCreador), Negocio.Usuario.tienePermiso
            // toma el default -1 y entra a la rama que chequea "Ver" (FormConsulta) en vez de
            // "Editar" (FormEdicion) -- para el formulario "Ventas", FormConsulta="formVentas" y
            // FormEdicion="formNuevaVenta" nunca son iguales, asi que esta llamada SIEMPRE daba
            // false sin importar el permiso que tuviera el usuario (salvo Admin, que tiene
            // bypass total). Pasando validado.Id (>= 0) se fuerza la rama de "Editar", que es la
            // que corresponde. Ver docs/DECISIONS.md.
            bool tienePermiso = !exigirPermisoVentas ||
                (validado != null && PermisosHelper.TienePermiso(validado, empresa, Permisos.Venta.NuevaVenta, DateTime.Now, validado.Id));
            if (validado == null || !validado.Activo || !tienePermiso)
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            PosOperadorStepUpRateLimiter.Reset(sessionId);
            PermisosHelper.RegistrarOperadorPOS(Session, posInstanceId, validado);

            return Json(new { ok = true, nombre = validado.Nombre });
        }

        // Igual que ResolverOperadorPOS pero para el operador de modulo (Ventas/Index,
        // DetalleVenta, Lineas) -- sin posInstanceId, una sola clave por sesion (ver
        // PermisosHelper.ObtenerOperadorModulo). Para cualquier usuario normal devuelve el
        // mismo usuario de sesion sin cambios -- cero impacto fuera de la cuenta de produccion.
        protected Entidades.Usuario ResolverOperadorModulo(string modulo, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion)
                return usuarioSesion;

            return PermisosHelper.ObtenerOperadorModulo(Session, modulo) ?? usuarioSesion;
        }

        // Step-up de credenciales para que la cuenta compartida de produccion pueda navegar un
        // modulo de consulta (hoy solo "Ventas": Index, DetalleVenta, Lineas). A diferencia de
        // ValidarOperadorPOS, esto SOLO valida identidad+contraseña -- no exige ningun permiso
        // puntual aca, porque los chequeos de permiso que ya existen en esas pantallas
        // (Permisos.Venta.VerVentas), una vez que apuntan al operador resuelto en vez de a
        // Session, son los que de verdad deciden que puede ver (mismo criterio que se uso para
        // relajar el paso de autorizar operador en POS Expendio, ver docs/DECISIONS.md).
        protected JsonResult ValidarOperadorModulo(int idUsuario, string clave, string modulo)
        {
            string sessionId = Session.SessionID;

            TimeSpan retryAfter;
            if (PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out retryAfter))
            {
                return Json(new
                {
                    ok = false,
                    bloqueado = true,
                    segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds)
                });
            }

            const string mensajeGenerico = "Usuario o contraseña incorrectos.";

            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(modulo))
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var oUsuarioN = Web.Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            var candidato = oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var validado = oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            if (validado == null || !validado.Activo)
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            PosOperadorStepUpRateLimiter.Reset(sessionId);
            PermisosHelper.RegistrarOperadorModulo(Session, modulo, validado);

            return Json(new { ok = true, nombre = validado.Nombre });
        }

        protected int ValorInt(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0;

            int valor;
            return int.TryParse(Convert.ToString(row[columna]), out valor) ? valor : 0;
        }

        protected string ValorString(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return "";

            return Convert.ToString(row[columna]);
        }

        private static bool EsSesionSoloLectura(HttpContextBase httpContext)
        {
            if (httpContext == null)
                return false;

            var handler = httpContext.CurrentHandler;
            return handler is IReadOnlySessionState && !(handler is IRequiresSessionState);
        }
    }
}
