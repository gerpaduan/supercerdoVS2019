// Endpoints JSON del modal multi-seleccion de los filtros Cliente / Vendedor / Proveedor
// (wwwroot/Scripts/app/filtro-picker.js). Solo lectura: lista personas y vendedores para tildar
// y resuelve IDs a nombres para hidratar un input que llega con IDs por URL.
// La logica vive en Services/FiltroPickerService.cs. Detalle: docs/DECISIONS.md (2026-10-13).
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCore.Services;

namespace WebCore.Controllers
{
    public class FiltroPickerController : Controller
    {
        private readonly FiltroPickerService _servicio;

        public FiltroPickerController(IUsuarioSesionService sesion)
        {
            // Mismo armado que el resto de los controllers de WebCore: contexto de empresa de la sesion
            // y objetos de negocio por request (ver PersonasController).
            var empresa = sesion.Empresa;
            var parametros = WebCore.Infrastructure.NegocioFactory.CrearParametros(empresa);
            parametros.Reload();

            _servicio = new FiltroPickerService(
                WebCore.Infrastructure.NegocioFactory.CrearPersona(empresa, parametros),
                WebCore.Infrastructure.NegocioFactory.CrearUsuario(empresa, parametros));
        }

        // Clientes y proveedores (misma tabla de personas). hayMas = hay mas filas que el tope del modal.
        [HttpGet]
        public JsonResult Personas(string filtro = "")
        {
            var (items, hayMas) = _servicio.BuscarPersonas(filtro);
            return Json(new { items, hayMas });
        }

        // Vendedores = usuarios de la empresa (incluye inactivos, marcados como tales).
        [HttpGet]
        public JsonResult Vendedores(string filtro = "")
        {
            var (items, hayMas) = _servicio.BuscarVendedores(filtro);
            return Json(new { items, hayMas });
        }

        // Resuelve IDs (CSV) a sus datos. tipo: "vendedor" -> usuarios; cualquier otro valor
        // ("cliente", "proveedor") -> personas. IDs invalidos o inexistentes se ignoran.
        [HttpGet]
        public JsonResult Resolver(string tipo, string ids)
        {
            List<int> idsValidos = FiltroPickerService.ParsearIds(ids);

            var items = string.Equals(tipo, "vendedor", System.StringComparison.OrdinalIgnoreCase)
                ? _servicio.ResolverVendedores(idsValidos)
                : _servicio.ResolverPersonas(idsValidos);

            return Json(new { items = items.ToList() });
        }
    }
}
