using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Datos;
using System.Globalization;
using Web.Helpers;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oUsuarioN = new Negocio.Usuario(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
        }

        public ActionResult Index()
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            ViewBag.PuedeVerCierreCaja = oUsuarioN.tienePermiso(user, Permisos.Caja.CierresDeCaja, DateTime.Today, -1);

            var sucursales = oSucursalN.findAll();
            ViewBag.Sucursales = sucursales;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult AccesoDenegado()
        {
            return View();
        }
    }
}