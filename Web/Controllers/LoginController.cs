using Entidades;
using System;
using System.Web.Mvc;
using Utilidades;
using wsAFIPvs2008;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            IEmpresaContext empresa = new EmpresaContextNulo();
            oUsuarioN = new Negocio.Usuario(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string usuario, string clave)
        {
            var user = oUsuarioN.validarUsuario(usuario, clave, false);

            if (user != null && user.Activo)
            {

                IEmpresaContext empresa = new EmpresaContextWin(user.IdEmpresa);

                oUsuarioN = new Negocio.Usuario(empresa);
                oSucursalN = new Negocio.Sucursal(empresa);

                user = oUsuarioN.getUsuarioById(user.Id); //user.IdSucursal == 0 ? null : oSucursalN.findById(user.IdSucursal);
                // Sucursal (igual que tenías)
                string sucNombre = user.Sucursal == null
                    ? "Seleccione Sucursal"
                    : user.Sucursal.SucursalNombre;

                user.SucursalNombre = sucNombre ?? "";

                // ✅ Guardar usuario en sesión
                Session["Usuario"] = user;

                // ✅ CLAVE: IdEmpresa en sesión (lo lee EmpresaContextWeb)
                Session["IdEmpresa"] = user.IdEmpresa;

                // ✅ Limpiar cache viejo por las dudas
                Session.Remove("PARAM_CTX");

                // ✅ Cargar parámetros 1 vez por sesión
                IEmpresaContext empresaContext = new EmpresaContextWeb(); // usa Session["IdEmpresa"]
                IParametrosContext paramCtx = new Negocio.Parametros(empresaContext);
                paramCtx.Reload(); // opcional (precarga)
                Session["PARAM_CTX"] = paramCtx;

                return RedirectToAction("Index", "Home");
            }

            string error = user == null ? "Usuario o clave incorrectos." : (!user.Activo ? "cuenta inactiva" : "");
            ViewBag.Error = error;
            return View();
        }

        public ActionResult Logout()
        {
            // ✅ limpiar lo que agregamos
            Session.Remove("PARAM_CTX");
            Session.Remove("IdEmpresa");

            // Limpia todos los objetos dentro de Session
            Session.Clear();

            // Elimina la cookie de sesión del navegador
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);
            }

            // Finaliza la sesión por completo
            Session.Abandon();

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public JsonResult CambiarSucursal(int idSucursal)
        {
            try
            {
                var usuario = Session["Usuario"] as Entidades.Usuario;

                if (usuario == null)
                    return Json(new { ok = false, msg = "Sesión expirada" });


                IEmpresaContext empresa = new EmpresaContextWin(usuario.IdEmpresa);

                oUsuarioN = new Negocio.Usuario(empresa);
                oSucursalN = new Negocio.Sucursal(empresa);

                var sucursal = oSucursalN.findById(idSucursal);
                if (sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida" });

                usuario.IdSucursal = sucursal.IdSucursal;
                usuario.Sucursal = sucursal;
                usuario.SucursalNombre = sucursal.SucursalNombre;

                oUsuarioN.setSucursalUsuario(usuario);
                Session["Usuario"] = usuario;

                return Json(new
                {
                    ok = true,
                    sucursalNombre = usuario.SucursalNombre,
                    idSucursal = usuario.IdSucursal
                });
            }
            catch
            {
                return Json(new { ok = false });
            }
        }
    }
}
