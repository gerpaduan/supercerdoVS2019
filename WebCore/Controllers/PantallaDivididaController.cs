// Pantalla dividida (2026-11-20, docs/DECISIONS.md "Pantalla dividida"): una pagina con paneles <iframe> para ver varios modulos lado a lado
// (Stock | Movimientos | un Reporte...). Index solo dibuja la estructura; cada panel carga la pantalla real y por eso respeta sus permisos.
// LiberarPermisos: el navegador avisa que pantallas siguen abiertas en los paneles y se borra el operador autorizado de las que ya no
// (ver PermisosHelper.LimpiarOperadoresFueraDe). Detalle de la pantalla: docs/11-manual-de-uso/pantallas/PantallaDividida.Index.md.
using Microsoft.AspNetCore.Mvc;
using WebCore.Helpers;

namespace WebCore.Controllers
{
    public class PantallaDivididaController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Title = "Pantalla dividida";
            return View();
        }

        // Cuerpo JSON { "rutas": ["/Stock/Index", ...] }. Con rutas vacias (ningun panel abierto) se borran todos los operadores.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LiberarPermisos([FromBody] LiberarPermisosPedido? pedido)
        {
            var abiertas = PantallaDivididaRutas.Parsear(pedido?.Rutas, Request.PathBase.Value);
            PermisosHelper.LimpiarOperadoresFueraDe(HttpContext.Session, abiertas);
            return Json(new { ok = true, abiertas = abiertas.Count });
        }

        public class LiberarPermisosPedido
        {
            public List<string> Rutas { get; set; } = new List<string>();
        }
    }
}
