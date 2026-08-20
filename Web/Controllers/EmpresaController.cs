using System;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Pantalla "Mi Empresa": ver/editar los datos basicos y el horario laboral de la EMPRESA
    // ACTUAL (no confundir con SystemAdministrationController, que es cross-tenant para el
    // super-admin de la plataforma). Mismo patron de permiso que ParametrosController: cualquier
    // usuario de la empresa puede ver, solo un Admin de esa misma empresa puede editar.
    public class EmpresaController : BaseController
    {
        private Negocio.Empresa oEmpresaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oEmpresaN = Web.Infrastructure.NegocioFactory.CrearEmpresa(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mi Empresa");
            }

            var datosEmpresa = oEmpresaN.findById(empresa.IdEmpresa);
            var model = CrearViewModel(datosEmpresa, usuario, true);

            ViewBag.Title = "Mi Empresa";
            ViewBag.Seccion = "Mi Empresa";
            return View("~/Views/Empresa/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(EmpresaIndexVm model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mi Empresa");
            }

            if (!PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Mi Empresa";
                TempData["AlertMsg"] = "Puede consultar los datos de la empresa, pero solo un administrador puede modificarlos.";
                return RedirectToAction("Index");
            }

            model = model ?? new EmpresaIndexVm();
            model.PuedeAdministrar = true;
            model.SoloLecturaInicial = false;

            TimeSpan diurnoDesde = TimeSpan.Zero, diurnoHasta = TimeSpan.Zero, tardeDesde = TimeSpan.Zero, tardeHasta = TimeSpan.Zero;
            bool horariosValidos =
                TryParseHora(model.HorarioDiurnoDesde, out diurnoDesde) &&
                TryParseHora(model.HorarioDiurnoHasta, out diurnoHasta) &&
                TryParseHora(model.HorarioTardeDesde, out tardeDesde) &&
                TryParseHora(model.HorarioTardeHasta, out tardeHasta);

            if (!horariosValidos)
            {
                ModelState.AddModelError("", "Los horarios ingresados no son válidos.");
            }

            if (!ModelState.IsValid)
            {
                var datosEmpresaError = oEmpresaN.findById(empresa.IdEmpresa);
                model.RazonSocialAfip = datosEmpresaError != null ? datosEmpresaError.RazonSocialAfip : "";
                model.Cuit = datosEmpresaError != null ? datosEmpresaError.Cuit : 0;
                ViewBag.Title = "Mi Empresa";
                ViewBag.Seccion = "Mi Empresa";
                return View("~/Views/Empresa/Index.cshtml", model);
            }

            try
            {
                oEmpresaN.ActualizarDatosBasicos(new Entidades.Empresa
                {
                    IdEmpresa = empresa.IdEmpresa,
                    NombreFantasia = model.NombreFantasia ?? "",
                    Slogan1 = model.Slogan1 ?? "",
                    Slogan2 = model.Slogan2 ?? "",
                    Slogan3 = model.Slogan3 ?? "",
                    Domicilio = model.Domicilio ?? "",
                    Ciudad = model.Ciudad ?? "",
                    Pais = model.Pais ?? "",
                    Telefono = model.Telefono ?? "",
                    Email = model.Email ?? "",
                    HorarioDiurnoDesde = diurnoDesde,
                    HorarioDiurnoHasta = diurnoHasta,
                    HorarioTardeDesde = tardeDesde,
                    HorarioTardeHasta = tardeHasta
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mi Empresa";
                TempData["AlertMsg"] = "Los datos de la empresa se guardaron correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = "Mi Empresa";
                ViewBag.Seccion = "Mi Empresa";
                return View("~/Views/Empresa/Index.cshtml", model);
            }
        }

        private EmpresaIndexVm CrearViewModel(Entidades.Empresa datosEmpresa, Entidades.Usuario usuario, bool soloLecturaInicial)
        {
            bool puedeAdministrar = PuedeAdministrar(usuario);
            datosEmpresa = datosEmpresa ?? new Entidades.Empresa();

            return new EmpresaIndexVm
            {
                PuedeAdministrar = puedeAdministrar,
                SoloLecturaInicial = soloLecturaInicial,
                MensajePermiso = puedeAdministrar
                    ? "Los datos se muestran inicialmente en modo lectura. Presione Modificar para editar y Guardar para aplicar los cambios."
                    : "Puede consultar los datos de la empresa. Solo un usuario administrador puede modificarlos.",
                RazonSocialAfip = datosEmpresa.RazonSocialAfip,
                Cuit = datosEmpresa.Cuit,
                NombreFantasia = datosEmpresa.NombreFantasia,
                Slogan1 = datosEmpresa.Slogan1,
                Slogan2 = datosEmpresa.Slogan2,
                Slogan3 = datosEmpresa.Slogan3,
                Domicilio = datosEmpresa.Domicilio,
                Ciudad = datosEmpresa.Ciudad,
                Pais = datosEmpresa.Pais,
                Telefono = datosEmpresa.Telefono,
                Email = datosEmpresa.Email,
                HorarioDiurnoDesde = FormatearHora(datosEmpresa.HorarioDiurnoDesde),
                HorarioDiurnoHasta = FormatearHora(datosEmpresa.HorarioDiurnoHasta),
                HorarioTardeDesde = FormatearHora(datosEmpresa.HorarioTardeDesde),
                HorarioTardeHasta = FormatearHora(datosEmpresa.HorarioTardeHasta)
            };
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == empresa.IdEmpresa && usuario.Admin;
        }

        private Entidades.Usuario ObtenerUsuarioActual()
        {
            return Session["Usuario"] as Entidades.Usuario;
        }

        private static string FormatearHora(TimeSpan valor)
        {
            return string.Format("{0:D2}:{1:D2}", (int)valor.Hours, (int)valor.Minutes);
        }

        private static bool TryParseHora(string valor, out TimeSpan resultado)
        {
            resultado = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            TimeSpan parsed;
            if (!TimeSpan.TryParse(valor, out parsed))
                return false;

            resultado = new TimeSpan(0, parsed.Hours, parsed.Minutes, 0);
            return true;
        }
    }
}
