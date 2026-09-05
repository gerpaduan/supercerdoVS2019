// Port de Web/Controllers/CodigosBarraController.cs (188 lineas, 4 acciones) -- formatos de
// codigo interno de balanza (EAN-13, prefijo 20-29) por empresa, usados por
// Negocio.BarcodeInterpreter al interpretar un codigo escaneado en el POS.
//
// Mismo criterio que el resto de la migracion: IEmpresaContext real y un stub Entidades.Usuario
// (Admin=true, IdEmpresa=1). El chequeo de permiso original (PuedeAdministrar: Admin de la misma
// empresa) se preserva tal cual -- con el stub admin siempre resuelve "autorizado", mismo
// resultado observable, no se omite el metodo en si (queda listo para un login real futuro).
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class CodigosBarraController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly Negocio.FormatoCodigoBarras _oFormatoN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public CodigosBarraController()
        {
            _oFormatoN = WebCore.Infrastructure.NegocioFactory.CrearFormatoCodigoBarras(_empresa);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var usuario = _usuarioActual;

            var model = new FormatosCodigoBarraIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(usuario),
                Items = _oFormatoN.Listar(_empresa.IdEmpresa)
            };

            ViewBag.Title = "Códigos de barra";
            return View(model);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            var usuario = _usuarioActual;
            if (!PuedeAdministrar(usuario))
                return Forbid();

            var model = new FormatoCodigoBarraEditVm
            {
                EsNuevo = true,
                LongitudTotal = 13,
                Activo = true
            };

            ViewBag.Title = "Nuevo formato de código de barra";
            return View("Editar", model);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var usuario = _usuarioActual;
            if (!PuedeAdministrar(usuario))
                return Forbid();

            var formato = _oFormatoN.ObtenerPorId(id, _empresa.IdEmpresa);
            if (formato == null)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Códigos de barra";
                TempData["AlertMsg"] = "No se encontró el formato solicitado.";
                return RedirectToAction("Index");
            }

            var model = new FormatoCodigoBarraEditVm
            {
                Id = formato.Id,
                EsNuevo = false,
                Nombre = formato.Nombre,
                Prefijo = formato.Prefijo,
                LongitudTotal = formato.LongitudTotal,
                PosicionCodigo = formato.PosicionCodigo,
                LongitudCodigo = formato.LongitudCodigo,
                PosicionValor = formato.PosicionValor,
                LongitudValor = formato.LongitudValor,
                TipoValor = formato.TipoValor,
                CantidadDecimales = formato.CantidadDecimales,
                Activo = formato.Activo
            };

            ViewBag.Title = "Editar formato de código de barra";
            return View("Editar", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(FormatoCodigoBarraEditVm model)
        {
            var usuario = _usuarioActual;
            if (!PuedeAdministrar(usuario))
                return Forbid();

            model = model ?? new FormatoCodigoBarraEditVm();

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.EsNuevo ? "Nuevo formato de código de barra" : "Editar formato de código de barra";
                return View("Editar", model);
            }

            try
            {
                var formato = new Entidades.FormatoCodigoBarras
                {
                    Id = model.Id,
                    IdEmpresa = _empresa.IdEmpresa,
                    Nombre = (model.Nombre ?? "").Trim(),
                    Prefijo = model.Prefijo,
                    LongitudTotal = model.LongitudTotal,
                    PosicionCodigo = model.PosicionCodigo,
                    LongitudCodigo = model.LongitudCodigo,
                    PosicionValor = model.PosicionValor,
                    LongitudValor = model.LongitudValor,
                    TipoValor = model.TipoValor,
                    CantidadDecimales = model.CantidadDecimales,
                    Activo = model.Activo
                };

                if (model.EsNuevo)
                {
                    formato.IdUsuarioCreador = usuario.Id;
                    _oFormatoN.Agregar(formato);
                }
                else
                {
                    var actual = _oFormatoN.ObtenerPorId(model.Id, _empresa.IdEmpresa);
                    if (actual == null)
                    {
                        TempData["AlertType"] = "danger";
                        TempData["AlertTitle"] = "Códigos de barra";
                        TempData["AlertMsg"] = "No se encontró el formato solicitado.";
                        return RedirectToAction("Index");
                    }

                    // El prefijo no se edita desde la UI (ver WebCore/Models/FormatoCodigoBarraVm.cs)
                    // -- se conserva el original aunque el form lo mande de otra forma.
                    formato.Prefijo = actual.Prefijo;
                    formato.IdUsuarioModificador = usuario.Id;
                    _oFormatoN.Actualizar(formato);
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Códigos de barra";
                TempData["AlertMsg"] = "El formato se guardó correctamente.";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = model.EsNuevo ? "Nuevo formato de código de barra" : "Editar formato de código de barra";
                return View("Editar", model);
            }
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }
    }
}
