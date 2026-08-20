using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Pantalla "Mis Sucursales": ver/editar las sucursales de la EMPRESA ACTUAL (no confundir
    // con SystemAdministrationController.Sucursales, que es cross-tenant para el super-admin de
    // la plataforma). Mismo patron de permiso que EmpresaController/ParametrosController:
    // cualquier usuario de la empresa puede ver, solo un Admin de esa misma empresa puede editar.
    // Es la pantalla que activa la geo-validacion de login ya existente en LoginController --
    // antes de esto, Latitud/Longitud/RadioLoginMetros/ValidarUbicacionLogin solo se podian
    // cargar por SQL directo.
    public class SucursalController : BaseController
    {
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oSucursalN = Web.Infrastructure.NegocioFactory.CrearSucursal(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mis Sucursales");
            }

            var sucursales = (oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
                .Where(s => s != null)
                .OrderBy(s => s.SucursalNombre ?? "")
                .ToList();

            var model = new SucursalIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(usuario),
                Items = sucursales.Select(s => new SucursalResumenVm
                {
                    IdSucursal = s.IdSucursal,
                    SucursalNombre = s.SucursalNombre,
                    Direccion = s.Direccion,
                    Localidad = s.Localidad,
                    ValidarUbicacionLogin = s.ValidarUbicacionLogin
                }).ToList()
            };

            ViewBag.Title = "Mis Sucursales";
            ViewBag.Seccion = "Mis Sucursales";
            return View("~/Views/Sucursal/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mis Sucursales");
            }

            var sucursal = oSucursalN.findById(id);
            if (sucursal == null || sucursal.IdEmpresa != empresa.IdEmpresa)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "No se encontró la sucursal solicitada.";
                return RedirectToAction("Index");
            }

            var model = CrearViewModel(sucursal, usuario, true);
            ViewBag.Title = "Editar sucursal";
            ViewBag.Seccion = "Mis Sucursales";
            return View("~/Views/Sucursal/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(SucursalEditVm model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mis Sucursales");
            }

            model = model ?? new SucursalEditVm();

            var sucursalActual = oSucursalN.findById(model.IdSucursal);
            if (sucursalActual == null || sucursalActual.IdEmpresa != empresa.IdEmpresa)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "No se encontró la sucursal solicitada.";
                return RedirectToAction("Index");
            }

            if (!PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "Puede consultar la sucursal, pero solo un administrador puede modificarla.";
                return RedirectToAction("Index");
            }

            model.PuedeAdministrar = true;
            model.SoloLecturaInicial = false;

            decimal? latitud = ParseNullableDecimal(model.Latitud);
            decimal? longitud = ParseNullableDecimal(model.Longitud);

            if (model.ValidarUbicacionLogin && (!latitud.HasValue || !longitud.HasValue))
            {
                ModelState.AddModelError("", "Para activar la validación de ubicación primero tenés que cargar Latitud y Longitud.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Editar sucursal";
                ViewBag.Seccion = "Mis Sucursales";
                return View("~/Views/Sucursal/Editar.cshtml", model);
            }

            try
            {
                oSucursalN.ActualizarDatosBasicos(new Entidades.Sucursal
                {
                    IdSucursal = model.IdSucursal,
                    IdEmpresa = empresa.IdEmpresa,
                    SucursalNombre = model.SucursalNombre ?? "",
                    Direccion = model.Direccion ?? "",
                    Localidad = model.Localidad ?? "",
                    Provincia = model.Provincia ?? "",
                    Pais = model.Pais ?? "",
                    Latitud = latitud,
                    Longitud = longitud,
                    RadioLoginMetros = model.RadioLoginMetros,
                    ValidarUbicacionLogin = model.ValidarUbicacionLogin
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "La sucursal se guardó correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = "Editar sucursal";
                ViewBag.Seccion = "Mis Sucursales";
                return View("~/Views/Sucursal/Editar.cshtml", model);
            }
        }

        private SucursalEditVm CrearViewModel(Entidades.Sucursal sucursal, Entidades.Usuario usuario, bool soloLecturaInicial)
        {
            bool puedeAdministrar = PuedeAdministrar(usuario);

            return new SucursalEditVm
            {
                PuedeAdministrar = puedeAdministrar,
                SoloLecturaInicial = soloLecturaInicial,
                MensajePermiso = puedeAdministrar
                    ? "Los datos se muestran inicialmente en modo lectura. Presione Modificar para editar y Guardar para aplicar los cambios."
                    : "Puede consultar los datos de la sucursal. Solo un usuario administrador puede modificarlos.",
                IdSucursal = sucursal.IdSucursal,
                SucursalNombre = sucursal.SucursalNombre,
                Direccion = sucursal.Direccion,
                Localidad = sucursal.Localidad,
                Provincia = sucursal.Provincia,
                Pais = sucursal.Pais,
                Latitud = sucursal.Latitud.HasValue ? sucursal.Latitud.Value.ToString(CultureInfo.InvariantCulture) : "",
                Longitud = sucursal.Longitud.HasValue ? sucursal.Longitud.Value.ToString(CultureInfo.InvariantCulture) : "",
                RadioLoginMetros = sucursal.RadioLoginMetros > 0 ? sucursal.RadioLoginMetros : 200,
                ValidarUbicacionLogin = sucursal.ValidarUbicacionLogin
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

        // Mismo parseo tolerante a coma/punto que LoginController.ParseNullableDecimal -- se
        // duplica acá (10 líneas) en vez de compartir un helper cruzado entre 2 controllers
        // por una lógica tan chica, mismo criterio ya usado en el resto de este codebase.
        private static decimal? ParseNullableDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            decimal parsed;
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            string normalizado = value.Replace(",", ".");
            if (decimal.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            return null;
        }
    }
}
