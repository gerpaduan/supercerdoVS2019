// Port de Web/Controllers/SucursalController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Pantalla "Mis Sucursales": ver/editar las sucursales de
// la empresa actual (distinto de SystemAdministrationController.Sucursales, cross-tenant para el
// super-admin de plataforma, ya portado en Modulo 1). Usuario/empresa reales via
// IUsuarioSesionService (login real, ver docs/DECISIONS.md 2026-09-06) -- ya no hay stub
// hardcodeado.
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
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly Negocio.Sucursal _oSucursalN;
        // Titulares fiscales (null en SQL Server: la pantalla no muestra el bloque de titular).
        private readonly Negocio.TitularFiscal? _titulares;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public SucursalController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa);
            _titulares = WebCore.Infrastructure.NegocioFactory.CrearTitularFiscal(_empresa);
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
            model.Titular = ArmarBloqueTitular(sucursal);
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
                model.Titular = ArmarBloqueTitular(sucursalActual);
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
                model.Titular = ArmarBloqueTitular(sucursalActual);
                return View("~/Views/Sucursal/Editar.cshtml", model);
            }
        }

        // Asigna a la sucursal el titular de facturación (y su punto de venta si no es el principal). Solo administradores y
        // solo sobre una sucursal de la propia empresa. No pasa por ActualizarDatosBasicos: es otro dato y otra regla
        // (titulares fiscales, 2026-10-09). Un cambio vale para las facturas FUTURAS de la sucursal.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarTitular(int idSucursal, int idTitular, int? codPuntoVenta)
        {
            var sucursal = _oSucursalN.findById(idSucursal);
            if (sucursal == null || sucursal.IdEmpresa != _empresa.IdEmpresa)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "No se encontró la sucursal solicitada.";
                return RedirectToAction("Index");
            }

            if (!PuedeAdministrar(_usuarioActual) || _titulares == null)
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Mis Sucursales";
                TempData["AlertMsg"] = "Solo un administrador puede cambiar el titular de facturación.";
                return RedirectToAction("Editar", new { id = idSucursal });
            }

            string? error = _titulares.AsignarTitularSucursal(sucursal, _empresa.IdEmpresa, idTitular, codPuntoVenta);
            TempData["AlertType"] = error == null ? "success" : "danger";
            TempData["AlertTitle"] = "Titular de facturación";
            TempData["AlertMsg"] = error ?? "El titular de facturación de la sucursal se guardó correctamente.";
            return RedirectToAction("Editar", new { id = idSucursal });
        }

        // Bloque "Titular de facturación": titulares habilitados de la empresa con sus puntos de venta. null en SQL Server.
        private SucursalTitularBloqueVm? ArmarBloqueTitular(Entidades.Sucursal sucursal)
        {
            if (_titulares == null) return null;

            var titulares = _titulares.Listar(_empresa.IdEmpresa, false);
            var principal = titulares.FirstOrDefault(t => t.EsPredeterminado);
            int actual = _titulares.ObtenerIdTitularDeSucursal(sucursal.IdSucursal) ?? principal?.IdTitular ?? 0;
            var titularActual = titulares.FirstOrDefault(t => t.IdTitular == actual);

            var bloque = new SucursalTitularBloqueVm { IdTitularActual = actual };
            if (titularActual != null)
                bloque.PuntoVentaActual = _titulares.ResolverPuntoVenta(titularActual, sucursal, out _);

            // Solo se ofrecen los habilitados; el actual siempre figura aunque se haya deshabilitado despues.
            foreach (var t in titulares.Where(t => t.Habilitado || t.IdTitular == actual))
            {
                bloque.Opciones.Add(new SucursalTitularOpcionVm
                {
                    IdTitular = t.IdTitular,
                    Nombre = t.RazonSocial ?? "",
                    Cuit = t.Cuit,
                    EsPrincipal = t.EsPredeterminado,
                    Habilitado = t.Habilitado,
                    TieneCertificado = t.TieneCertificado,
                    PuntosVenta = t.EsPredeterminado
                        ? new List<int>()
                        : t.PuntosVenta.Where(p => p.Habilitado).Select(p => p.CodPuntoVentaAfip).OrderBy(p => p).ToList()
                });
            }
            return bloque;
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
