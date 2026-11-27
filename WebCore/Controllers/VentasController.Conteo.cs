// Conteo previo de Ventas / Lineas de venta: dice cuanto traeria un filtro ANTES de cargarlo, para que la pantalla
// advierta si es una busqueda pesada (aviso-busqueda-pesada.js). Ver docs/DECISIONS.md 2026-11-26.
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebCore.Helpers;

namespace WebCore.Controllers
{
    public partial class VentasController
    {
        // pantalla = "lineas" cuenta lineas; cualquier otro valor cuenta ventas (Ventas/Index). Devuelve { ok, pesada,
        // cantidad, unidad }. ok=false (sin permiso, SQL Server o error) hace que la pantalla busque sin preguntar: el
        // aviso nunca bloquea una consulta. La cantidad solo viaja cuando la busqueda es pesada.
        [HttpGet]
        public IActionResult ContarBusqueda(string pantalla, DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1)
        {
            bool esLineas = string.Equals(pantalla, "lineas", StringComparison.OrdinalIgnoreCase);

            // Mismos gates que la pantalla que se va a cargar: operador de produccion en las dos; Venta.VerVentas en Index
            // (Lineas no valida un permiso propio).
            if (_usuarioActual.EsUsuarioProduccion && ObtenerOperadorModulo("Ventas") == null)
                return Json(new { ok = false });
            if (!esLineas && !_oUsuarioN.tienePermiso(ResolverOperadorModulo("Ventas", _usuarioActual), Entidades.Permisos.Venta.VerVentas, DateTime.Today, -1))
                return Json(new { ok = false });

            int limite = esLineas ? BusquedaPesadaSettings.LimiteLineas : BusquedaPesadaSettings.LimiteVentas;
            var repositorio = WebCore.Infrastructure.NegocioFactory.CrearConteoVentasRepository(_empresa);
            if (limite == 0 || repositorio == null)
                return Json(new { ok = true, pesada = false });

            // Mismo periodo que arma cada pantalla: sin fechas es hoy; en Index, un solo dia a las 00:00 abarca ese dia.
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;
            if (!esLineas && desde == hasta && desde.Hour == 0)
                hasta = hasta.AddDays(1);
            if (hasta < desde)
                hasta = desde;
            // "Sin sucursal" llega como 0 o -1 segun la pantalla.
            int sucursal = idSucursal > 0 ? idSucursal : -1;

            try
            {
                int[] ocultas = _ctaCteReservada.IdsOcultos(Entidades.RestriccionCtaCteReservada.TablaVentas).ToArray();
                long cantidad = esLineas
                    ? repositorio.ContarLineas(desde, hasta, sucursal, ocultas)
                    : repositorio.ContarVentas(desde, hasta, sucursal, ocultas);

                if (!BusquedaPesadaSettings.EsPesada(cantidad, limite))
                    return Json(new { ok = true, pesada = false });

                return Json(new { ok = true, pesada = true, cantidad, unidad = esLineas ? "líneas de venta" : "ventas" });
            }
            catch (Exception ex)
            {
                // Se transforma en respuesta controlada: si no se puede contar, la busqueda sigue como siempre.
                _logger.LogWarning(ex, "No se pudo contar la busqueda de {Pantalla} entre {Desde} y {Hasta}", esLineas ? "lineas" : "ventas", desde, hasta);
                return Json(new { ok = false });
            }
        }
    }
}
