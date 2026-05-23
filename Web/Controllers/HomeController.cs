using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Datos;
using System.Globalization;
using System.IO;
using Web.Helpers;
using Web.Models;

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

        public ActionResult Utilidades()
        {
            var model = new UtilitiesIndexVm
            {
                Agentes = new List<UtilityItemVm>
                {
                    BuildUtilityItem(
                        "Agente de balanza",
                        "Agente local",
                        "Lee la balanza desde la PC local y expone la API en 127.0.0.1 para POS y otras pantallas web.",
                        "~/Content/downloads/Carnisys.Balanza.Agent.zip",
                        "Carnisys.Balanza.Agent.zip",
                        "Incluye ejecutable, configuración inicial y script de instalación local."),
                    BuildUtilityItem(
                        "Agente de impresión",
                        "Agente local",
                        "Permite imprimir tickets desde la terminal local sin depender del servidor web.",
                        "~/Content/downloads/CarniSys.PrintAgent.zip",
                        "CarniSys.PrintAgent.zip",
                        "Instalar en la terminal donde está conectada la impresora térmica.")
                },
                OtrasUtilidades = new List<UtilityItemVm>
                {
                    new UtilityItemVm
                    {
                        Nombre = "Próximas utilidades",
                        Categoria = "Catálogo",
                        Descripcion = "Este sector queda preparado para sumar nuevas herramientas locales o instaladores del sistema.",
                        Estado = "Próximamente",
                        Version = "-",
                        Disponible = false,
                        NotaInstalacion = "Aquí podremos ir agregando nuevas utilidades sin tocar el resto del menú."
                    }
                }
            };

            return View(model);
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }

        private UtilityItemVm BuildUtilityItem(string nombre, string categoria, string descripcion, string virtualPath, string archivoNombre, string notaInstalacion)
        {
            string physicalPath = Server.MapPath(virtualPath);
            bool disponible = System.IO.File.Exists(physicalPath);
            var info = disponible ? new FileInfo(physicalPath) : null;

            return new UtilityItemVm
            {
                Nombre = nombre,
                Categoria = categoria,
                Descripcion = descripcion,
                Estado = disponible ? "Disponible" : "No disponible",
                Version = info != null ? info.LastWriteTime.ToString("dd/MM/yyyy HH:mm") : "-",
                ArchivoUrl = disponible ? Url.Content(virtualPath) : string.Empty,
                ArchivoNombre = archivoNombre,
                ArchivoTamano = info != null ? FormatFileSize(info.Length) : "-",
                NotaInstalacion = notaInstalacion,
                Disponible = disponible
            };
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "0 KB";

            double kb = bytes / 1024d;
            if (kb < 1024d)
            {
                return kb.ToString("0.#", CultureInfo.InvariantCulture) + " KB";
            }

            double mb = kb / 1024d;
            return mb.ToString("0.##", CultureInfo.InvariantCulture) + " MB";
        }
    }
}
