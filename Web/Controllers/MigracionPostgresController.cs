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

        [HttpGet]
        public ActionResult CompararSucursal(int idSucursal)
        {
            // SQL Server: constructor de siempre, sin cambios.
            var sucursalSqlServer = new Negocio.Sucursal(empresa, param);
            var resultadoSqlServer = sucursalSqlServer.findById(idSucursal);

            // Postgres: constructor nuevo, inyectando DatosPostgres.SucursalPg.
            string connString = ConfigurationManager.ConnectionStrings["ConexionPostgresPiloto"].ConnectionString;
            var repoPostgres = new DatosPostgres.SucursalPg(connString, empresa.IdEmpresa);
            var sucursalPostgres = new Negocio.Sucursal(repoPostgres, empresa, param);

            Entidades.Sucursal resultadoPostgres;
            string errorPostgres = null;
            try
            {
                resultadoPostgres = sucursalPostgres.findById(idSucursal);
            }
            catch (Exception ex)
            {
                resultadoPostgres = null;
                errorPostgres = ex.Message;
            }

            ViewBag.Title = "Migracion Postgres: comparar Sucursal #" + idSucursal;
            ViewBag.Seccion = "Administracion del sistema";
            ViewBag.IdSucursalBuscada = idSucursal;
            ViewBag.IdEmpresaSesion = empresa.IdEmpresa;
            ViewBag.ErrorPostgres = errorPostgres;

            return View("~/Views/MigracionPostgres/CompararSucursal.cshtml", new Web.Models.ComparacionSucursalVm
            {
                SqlServer = resultadoSqlServer,
                Postgres = resultadoPostgres
            });
        }

        [HttpGet]
        public ActionResult CompararPago(int idPago)
        {
            // SQL Server: constructor de siempre, sin cambios.
            var pagoSqlServer = new Negocio.CuentaCorriente(empresa, param);
            var resultadoSqlServer = pagoSqlServer.getPagoById(idPago);

            // Postgres: constructor nuevo, inyectando DatosPostgres.CuentaCorrientePg
            // (que a su vez recibe PersonaPg para resolver la Persona relacionada).
            string connString = ConfigurationManager.ConnectionStrings["ConexionPostgresPiloto"].ConnectionString;
            var personaRepoPostgres = new DatosPostgres.PersonaPg(connString, empresa.IdEmpresa);
            var repoPostgres = new DatosPostgres.CuentaCorrientePg(connString, empresa.IdEmpresa, personaRepoPostgres);
            var pagoPostgres = new Negocio.CuentaCorriente(repoPostgres, empresa, param);

            Entidades.Pago resultadoPostgres;
            string errorPostgres = null;
            try
            {
                resultadoPostgres = pagoPostgres.getPagoById(idPago);
            }
            catch (Exception ex)
            {
                resultadoPostgres = null;
                errorPostgres = ex.Message;
            }

            ViewBag.Title = "Migracion Postgres: comparar Pago #" + idPago;
            ViewBag.Seccion = "Administracion del sistema";
            ViewBag.IdPagoBuscado = idPago;
            ViewBag.IdEmpresaSesion = empresa.IdEmpresa;
            ViewBag.ErrorPostgres = errorPostgres;

            return View("~/Views/MigracionPostgres/CompararPago.cshtml", new Web.Models.ComparacionPagoVm
            {
                SqlServer = resultadoSqlServer,
                Postgres = resultadoPostgres
            });
        }

        [HttpGet]
        public ActionResult CompararCorte(int idCorte)
        {
            // SQL Server: constructor de siempre, sin cambios.
            var corteSqlServer = new Negocio.Corte(empresa, param);
            var resultadoSqlServer = corteSqlServer.findCorteById(idCorte, true);

            // Postgres: constructor nuevo, inyectando DatosPostgres.CortePg (que a su vez
            // recibe PersonaPg para resolver la Marca relacionada). Cubre solo el bloque
            // CRUD/referencia migrado -- el resto de Negocio.Corte sigue yendo por SQL Server.
            string connString = ConfigurationManager.ConnectionStrings["ConexionPostgresPiloto"].ConnectionString;
            var personaRepoPostgres = new DatosPostgres.PersonaPg(connString, empresa.IdEmpresa);
            var repoPostgres = new DatosPostgres.CortePg(connString, empresa.IdEmpresa, personaRepoPostgres);
            var cortePostgres = new Negocio.Corte(repoPostgres, empresa, param);

            Entidades.Corte resultadoPostgres;
            string errorPostgres = null;
            try
            {
                resultadoPostgres = cortePostgres.findCorteById(idCorte, true);
            }
            catch (Exception ex)
            {
                resultadoPostgres = null;
                errorPostgres = ex.Message;
            }

            ViewBag.Title = "Migracion Postgres: comparar Corte #" + idCorte;
            ViewBag.Seccion = "Administracion del sistema";
            ViewBag.IdCorteBuscado = idCorte;
            ViewBag.IdEmpresaSesion = empresa.IdEmpresa;
            ViewBag.ErrorPostgres = errorPostgres;

            return View("~/Views/MigracionPostgres/CompararCorte.cshtml", new Web.Models.ComparacionCorteVm
            {
                SqlServer = resultadoSqlServer,
                Postgres = resultadoPostgres
            });
        }
    }
}
