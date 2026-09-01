// Port de Web/Controllers/EmpresaController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Pantalla "Mi Empresa": ver/editar datos basicos y
// horario laboral de la empresa actual (distinto de SystemAdministrationController, que es
// cross-tenant para el super-admin de plataforma, ya portado en Modulo 1).
//
// Mismo criterio de siempre: stub Entidades.Usuario (Id=2, Admin=true, IdEmpresa=1, IdSucursal=2,
// Nombre="ger"). Con este stub, PuedeAdministrar(usuario) siempre da true (Admin=true e
// IdEmpresa coincide con el stub de empresa) -- el gate "solo lectura para no-admin" del original
// nunca se ejercita, pero el codigo real de PuedeAdministrar SI se porta (usa campos reales del
// usuario, no se omite como los sistemas de permiso con Session). El chequeo
// usuario.IdEmpresa != empresa.IdEmpresa (VistaAccesoDenegado) tampoco se porta -- con el stub
// nunca es true, y VistaAccesoDenegado depende de infraestructura de permisos con fecha que ya se
// omite en el resto de la migracion.
using System;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class EmpresaController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly Negocio.Empresa _oEmpresaN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public EmpresaController()
        {
            _oEmpresaN = new Negocio.Empresa(_empresa);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var datosEmpresa = _oEmpresaN.findById(_empresa.IdEmpresa);
            var model = CrearViewModel(datosEmpresa, _usuarioActual, true);

            ViewBag.Title = "Mi Empresa";
            ViewBag.Seccion = "Mi Empresa";
            return View("~/Views/Empresa/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(EmpresaIndexVm model)
        {
            if (!PuedeAdministrar(_usuarioActual))
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
                var datosEmpresaError = _oEmpresaN.findById(_empresa.IdEmpresa);
                model.RazonSocialAfip = datosEmpresaError != null ? datosEmpresaError.RazonSocialAfip : "";
                model.Cuit = datosEmpresaError != null ? datosEmpresaError.Cuit : 0;
                ViewBag.Title = "Mi Empresa";
                ViewBag.Seccion = "Mi Empresa";
                return View("~/Views/Empresa/Index.cshtml", model);
            }

            try
            {
                _oEmpresaN.ActualizarDatosBasicos(new Entidades.Empresa
                {
                    IdEmpresa = _empresa.IdEmpresa,
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
                RazonSocialAfip = datosEmpresa.RazonSocialAfip ?? "",
                Cuit = datosEmpresa.Cuit,
                NombreFantasia = datosEmpresa.NombreFantasia ?? "",
                Slogan1 = datosEmpresa.Slogan1 ?? "",
                Slogan2 = datosEmpresa.Slogan2 ?? "",
                Slogan3 = datosEmpresa.Slogan3 ?? "",
                Domicilio = datosEmpresa.Domicilio ?? "",
                Ciudad = datosEmpresa.Ciudad ?? "",
                Pais = datosEmpresa.Pais ?? "",
                Telefono = datosEmpresa.Telefono ?? "",
                Email = datosEmpresa.Email ?? "",
                HorarioDiurnoDesde = FormatearHora(datosEmpresa.HorarioDiurnoDesde),
                HorarioDiurnoHasta = FormatearHora(datosEmpresa.HorarioDiurnoHasta),
                HorarioTardeDesde = FormatearHora(datosEmpresa.HorarioTardeDesde),
                HorarioTardeHasta = FormatearHora(datosEmpresa.HorarioTardeHasta)
            };
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
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
