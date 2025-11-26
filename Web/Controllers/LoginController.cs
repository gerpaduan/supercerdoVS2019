using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Web.Helpers;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        private readonly Negocio.Usuario _usuarioNegocio = new Negocio.Usuario();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string usuario, string clave)
        {
            var user = _usuarioNegocio.validarUsuario(usuario, clave, false);

            if (user != null && user.Activo)
            {
                Session["Usuario"] = user;
                return RedirectToAction("Index", "Home");
            }

            string error = user == null ? "Usuario o clave incorrectos." :(!user.Activo ? "cuenta inactiva" : "");
            ViewBag.Error = error;
            return View();
        }
        public ActionResult Logout()
        {
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

    }
}