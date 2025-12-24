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
    public class HomeController : Controller
    {

        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
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