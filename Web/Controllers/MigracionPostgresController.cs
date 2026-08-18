using System;
using System.Configuration;
using System.Web.Mvc;
using Web.Helpers;

namespace Web.Controllers
{
    // Herramienta de verificacion para el piloto de la migracion a Postgres (Etapa 2,
    // ver docs/DECISIONS.md 2026-08-18) -- NO es una feature de producto. Compara, para
    // el mismo idPersona y el mismo tenant de la sesion actual, el resultado de leerlo
    // desde SQL Server (capa de siempre) contra Postgres (capa nueva, DatosPostgres).
    // Aislada de todo el resto de la app: ninguna ruta ni controller existente cambia.
    public class MigracionPostgresController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null)
                return;

            if (!SystemAdministrationAccessHelper.PuedeAdministrarSistema(Session))
            {
                ViewBag.Title = "Migracion Postgres";
                ViewBag.Seccion = "Administracion del sistema";
                filterContext.Result = View("~/Views/Shared/AccesoDenegado.cshtml");
            }
        }

        [HttpGet]
        public ActionResult Comparar(int idPersona)
        {
            // SQL Server: constructor de siempre, sin cambios.
            var personaSqlServer = new Negocio.Persona(empresa, param);
            var resultadoSqlServer = personaSqlServer.findById(idPersona);

            // Postgres: constructor nuevo, inyectando DatosPostgres.PersonaPg.
            string connString = ConfigurationManager.ConnectionStrings["ConexionPostgresPiloto"].ConnectionString;
            var repoPostgres = new DatosPostgres.PersonaPg(connString, empresa.IdEmpresa);
            var personaPostgres = new Negocio.Persona(repoPostgres, empresa, param);

            Entidades.Persona resultadoPostgres;
            string errorPostgres = null;
            try
            {
                resultadoPostgres = personaPostgres.findById(idPersona);
            }
            catch (Exception ex)
            {
                resultadoPostgres = null;
                errorPostgres = ex.Message;
            }

            ViewBag.Title = "Migracion Postgres: comparar Persona #" + idPersona;
            ViewBag.Seccion = "Administracion del sistema";
            ViewBag.IdPersonaBuscada = idPersona;
            ViewBag.IdEmpresaSesion = empresa.IdEmpresa;
            ViewBag.ErrorPostgres = errorPostgres;

            return View("~/Views/MigracionPostgres/Comparar.cshtml", new Web.Models.ComparacionPersonaVm
            {
                SqlServer = resultadoSqlServer,
                Postgres = resultadoPostgres
            });
        }
    }
}
