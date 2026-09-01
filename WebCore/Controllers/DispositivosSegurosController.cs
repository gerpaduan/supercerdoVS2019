// Port de Web/Controllers/DispositivosSegurosController.cs (ver docs/DECISIONS.md, migracion
// ASP.NET Core, Modulo 6 -- Reportes y administracion). Registro de PCs conocidas (numero de
// serie via agente local) que saltan el bloqueo por IP del login. Mismo criterio de stub que
// Empresa/SucursalController.
using System;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class DispositivosSegurosController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly Negocio.DispositivoSeguro _oDispositivoN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public DispositivosSegurosController()
        {
            _oDispositivoN = new Negocio.DispositivoSeguro(_empresa);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new DispositivosSegurosIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(_usuarioActual),
                Items = _oDispositivoN.Listar(_empresa.IdEmpresa)
            };

            ViewBag.Title = "Dispositivos seguros";
            ViewBag.Seccion = "Dispositivos seguros";
            return View("~/Views/DispositivosSeguros/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(string numeroSerie, string descripcion)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para agregar dispositivos seguros.";
                return RedirectToAction("Index");
            }

            numeroSerie = (numeroSerie ?? "").Trim();
            if (string.IsNullOrWhiteSpace(numeroSerie))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "El número de serie es obligatorio.";
                return RedirectToAction("Index");
            }

            try
            {
                _oDispositivoN.Agregar(new Entidades.DispositivoSeguro
                {
                    IdEmpresa = _empresa.IdEmpresa,
                    NumeroSerie = numeroSerie,
                    Descripcion = (descripcion ?? "").Trim(),
                    IdUsuarioCreador = _usuarioActual.Id
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "El dispositivo se agregó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "No se pudo agregar el dispositivo: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para eliminar dispositivos seguros.";
                return RedirectToAction("Index");
            }

            _oDispositivoN.Eliminar(id, _empresa.IdEmpresa);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = "El dispositivo se eliminó correctamente.";
            return RedirectToAction("Index");
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }
    }
}
