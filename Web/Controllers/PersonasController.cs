using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class PersonasController : BaseController
    {
        private Negocio.Persona oPersonaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            oPersonaN = new Negocio.Persona(empresa);
        }

        // GET: Personas
        public ActionResult Buscar()
        {
            return PartialView("_BuscarPersona");
        }

        public JsonResult Listar(string filtro)
        {
            filtro = filtro ?? "";
            DataTable dt = oPersonaN.buscarPersona(filtro, false);

            var personas = dt.AsEnumerable().Select(row => new
            {
                idPersona = row.Field<int>("IdPersona"),
                razonSocial = row.Field<string>("RazonSocial"),
                cuit = row.Field<string>("Cuit"),
                identificacion = row.Field<string>("nombreidentif")
            }).ToList();

            return Json(personas, JsonRequestBehavior.AllowGet);
        }


    }
}