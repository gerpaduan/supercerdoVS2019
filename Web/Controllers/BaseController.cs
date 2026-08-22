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

        protected bool AjustarFechaIndiceSegunLimiteYPermiso(string permiso, ref System.DateTime fecha, System.DateTime fechaLimiteSinPermiso, int idCreador = -1)
        {
            var limite = fechaLimiteSinPermiso.Date;
            var fechaAdvertencia = ObtenerFechaAdvertenciaIndice(permiso, fechaLimiteSinPermiso, idCreador);
            if (fecha.Date >= limite)
                return false;

            if (PermisosHelper.TienePermiso(Session, permiso, fecha, idCreador))
                return false;

            fecha = fechaAdvertencia;
            if (!EsSesionSoloLectura(HttpContext))
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
