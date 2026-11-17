using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebCore.Models;

namespace WebCore.Controllers;

// Pagina anfitriona para instalar una mini-app (Stock, Movimientos, Elaborados, Punto de venta, Punto de expendio) desde Utilidades.
// El navegador solo ofrece instalar la PWA cuyo manifest enlaza la pagina actual, y Utilidades usa el manifest general. Esta pagina enlaza el
// manifest de la app elegida y vive DENTRO de su scope (/Stock/InstalarApp, /Ventas/InstalarApp ...), por eso la ruta es por atributo y cuelga de
// la carpeta de cada app. Controller aparte (sin dependencias de base de datos, a diferencia de HomeController). Requiere sesion (FallbackPolicy
// global de Program.cs). Detalle: docs/DECISIONS.md (2026-11-17 c).
public class InstalarAppController : Controller
{
    // Carpeta (primer segmento de la URL) -> datos de la app. Debe coincidir con manifest-*.json y con la lista de Views/Home/Utilidades.cshtml.
    private static readonly Dictionary<string, AppInstalableVm> Apps = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Stock"] = new AppInstalableVm { Nombre = "Stock", ArchivoManifest = "manifest-stock.json", ArchivoIcono = "stock-192.png" },
        ["Movimientos"] = new AppInstalableVm { Nombre = "Movimientos", ArchivoManifest = "manifest-movimientos.json", ArchivoIcono = "movimientos-192.png" },
        ["Elaborados"] = new AppInstalableVm { Nombre = "Elaborados", ArchivoManifest = "manifest-elaborados.json", ArchivoIcono = "elaborados-192.png" },
        ["Ventas"] = new AppInstalableVm { Nombre = "Punto de venta", ArchivoManifest = "manifest-pos.json", ArchivoIcono = "pos-192.png" },
        ["PuntosExpendio"] = new AppInstalableVm { Nombre = "Punto de expendio", ArchivoManifest = "manifest-expendio.json", ArchivoIcono = "expendio-192.png" }
    };

    // Existencia por sucursal: su scope es /Existencia (StockController.ExistenciaApp, modo ventana independiente, layout sin menu; FUERA de /Stock/ para no
    // anidarse con la app Stock), asi que la pagina anfitriona cuelga de ahi. Ruta literal.
    private static readonly AppInstalableVm AppExistencia =
        new AppInstalableVm { Nombre = "Existencia por sucursal", ArchivoManifest = "manifest-existencia.json", ArchivoIcono = "existencia-192.png" };

    [HttpGet("{app:regex(^(Stock|Movimientos|Elaborados|Ventas|PuntosExpendio)$)}/InstalarApp")]
    public IActionResult Index(string app)
    {
        if (!Apps.TryGetValue(app, out var modelo)) return NotFound();
        return View(modelo);
    }

    [HttpGet("Existencia/InstalarApp")]
    public IActionResult Existencia() => View("Index", AppExistencia);
}
