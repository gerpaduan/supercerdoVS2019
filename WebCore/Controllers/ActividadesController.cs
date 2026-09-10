// Modulo "Actividades" (solo admin), 2026-09-07 -- pedido explicito del usuario, ver
// docs/DECISIONS.md. Junta 6 fuentes heterogeneas (cambios de precio de Corte, ventas con lineas
// anuladas, ventas con bonificacion/recargo aplicado manualmente por el cajero, egresos de caja
// que son gastos, movimientos de stock entre sucursales, compras/registros de stock, y altas o
// ediciones de formulas de elaborados) en una sola linea de tiempo ordenada por fecha, con
// filtro de rango (max 7 dias, default hoy) y paginacion de 30 items. Mismo patron de gate de
// permiso que AuditoriaLoginController (solo admin -- este modulo es exclusivamente de negocio/
// dueño, no hay permiso granular como en Auditoria de accesos).
//
// La agregacion de las 6 fuentes vive en WebCore/Services/ActividadesFeedService.cs (extraido
// 2026-09-10, item 4 de la segunda ronda de pedidos -- ver docs/DECISIONS.md "Batch 8:
// Actividades en el dashboard") para reusarla desde el bloque "Actividades" del dashboard
// (HomeController.ObtenerUltimasActividadesDashboard) sin duplicar la logica de agregacion.
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace WebCore.Controllers
{
    public class ActividadesController : Controller
    {
        private const int MaxDiasRango = 7;
        private const int ItemsPorPagina = 30;

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly WebCore.Services.ActividadesFeedService _feed;

        public ActividadesController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _feed = new WebCore.Services.ActividadesFeedService(sesion.Empresa, sesion.Parametros);
        }

        [HttpGet]
        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int pagina = 1)
        {
            var usuario = _sesion.UsuarioActual;
            if (!usuario.Admin)
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            DateTime hasta = (fechaHasta ?? DateTime.Today).Date;
            DateTime desde = (fechaDesde ?? hasta).Date;

            // Intervalo maximo 7 dias (pedido explicito del usuario) -- si el request pide mas,
            // se recorta silenciosamente a los ultimos 7 dias del rango pedido en vez de tirar
            // error (mismo criterio "tolerante" que el resto de los filtros de fecha del sistema).
            if ((hasta - desde).TotalDays > MaxDiasRango)
                desde = hasta.AddDays(-MaxDiasRango);

            DateTime desdeConHora = desde.Date;
            DateTime hastaConHora = hasta.Date.AddDays(1).AddTicks(-1);

            var items = _feed.ObtenerActividades(desdeConHora, hastaConHora);

            int totalItems = items.Count;
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItems / (double)ItemsPorPagina));
            pagina = Math.Max(1, Math.Min(pagina, totalPaginas));
            var pagina_items = items.Skip((pagina - 1) * ItemsPorPagina).Take(ItemsPorPagina).ToList();

            var model = new Models.ActividadesIndexVm
            {
                FechaDesde = desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                FechaHasta = hasta.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Pagina = pagina,
                TotalPaginas = totalPaginas,
                TotalItems = totalItems,
                Items = pagina_items
            };

            return View(model);
        }
    }
}
