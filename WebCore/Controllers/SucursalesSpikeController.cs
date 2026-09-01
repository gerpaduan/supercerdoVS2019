// Controller de PRUEBA para el spike de migracion a ASP.NET Core (ver docs/DECISIONS.md).
// No es codigo de produccion: usa un IEmpresaContext hardcodeado (sin login/sesion real) solo
// para probar, de punta a punta, que Negocio/Datos/Utilidades.Core funcionan igual desde un
// proyecto ASP.NET Core corriendo bajo Kestrel (y, mas adelante, bajo Linux real).
using Microsoft.AspNetCore.Mvc;
using Utilidades;

namespace WebCore.Controllers
{
    public class SucursalesSpikeController : Controller
    {
        private sealed class SpikeEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        public IActionResult Index()
        {
            var empresa = new SpikeEmpresaContext();
            var oSucursalN = new Negocio.Sucursal(empresa);
            var sucursales = oSucursalN.findAll();

            return View(sucursales);
        }
    }
}
