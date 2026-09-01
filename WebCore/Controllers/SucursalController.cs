// Port de Web/Controllers/SucursalController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Pantalla "Mis Sucursales": ver/editar las sucursales de
// la empresa actual (distinto de SystemAdministrationController.Sucursales, cross-tenant para el
// super-admin de plataforma, ya portado en Modulo 1). Mismo criterio de stub que EmpresaController
// (Id=2, Admin=true, IdEmpresa=1, IdSucursal=2, Nombre="ger") -- PuedeAdministrar siempre da true.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class SucursalController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly Negocio.Sucursal _oSucursalN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public SucursalController()
        {
            _oSucursalN = new Negocio.Sucursal(_empresa);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var sucursales = (_oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
                .Where(s => s != null)
                .OrderBy(s => s.SucursalNombre ?? "")
                .ToList();

            var model = new SucursalIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(_usuarioActual),
                Items = sucursales.Select(s => new SucursalResumenVm
                {
                    IdSucursal = s.IdSucursal,
                    SucursalNombre = s.SucursalNombre ?? "",
                    Direccion = s.Direccion ?? "",
                    Localidad = s.Localidad ?? "",
                    ValidarUbicacionLogin = s.ValidarUbicacionLogin
                }).ToList()
            };

            ViewBag.Title = "Mis Sucursales";
            ViewBag.Seccion = "Mis Sucursales";
            return View("~/Views/Sucursal/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var sucursal = _oSucursalN.findById(id);
            if (sucursal == null || sucursal.IdEmpresa != _empresa.IdEmpresa)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "No se encontró la sucursal solicitada.";
                return RedirectToAction("Index");
            }

            var model = CrearViewModel(sucursal, _usuarioActual, true);
            ViewBag.Title = "Editar sucursal";
            ViewBag.Seccion = "Mis Sucursales";
            return View("~/Views/Sucursal/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(SucursalEditVm model)
        {
            model = model ?? new SucursalEditVm();

            var sucursalActual = _oSucursalN.findById(model.IdSucursal);
            if (sucursalActual == null || sucursalActual.IdEmpresa != _empresa.IdEmpresa)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "No se encontró la sucursal solicitada.";
                return RedirectToAction("Index");
            }

            if (!PuedeAdministrar(_usuarioActual))
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
                _oSucursalN.ActualizarDatosBasicos(new Entidades.Sucursal
                {
                    IdSucursal = model.IdSucursal,
                    IdEmpresa = _empresa.IdEmpresa,
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
                SucursalNombre = sucursal.SucursalNombre ?? "",
                Direccion = sucursal.Direccion ?? "",
                Localidad = sucursal.Localidad ?? "",
                Provincia = sucursal.Provincia ?? "",
                Pais = sucursal.Pais ?? "",
                Latitud = sucursal.Latitud.HasValue ? sucursal.Latitud.Value.ToString(CultureInfo.InvariantCulture) : "",
                Longitud = sucursal.Longitud.HasValue ? sucursal.Longitud.Value.ToString(CultureInfo.InvariantCulture) : "",
                RadioLoginMetros = sucursal.RadioLoginMetros > 0 ? sucursal.RadioLoginMetros : 200,
                ValidarUbicacionLogin = sucursal.ValidarUbicacionLogin
            };
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }

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
