using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;
using WebCore.Services;

namespace WebCore.Controllers;

// ImprimirCalculadoraBilletesPayload/DescargarCalculadoraBilletesPdf: port de
// Web/Controllers/HomeController.cs (batch 7 POS, ver docs/10-migracion-aspnet-core/
// PLAN-POS-UI.md). Usados por Scripts/app/calculadora-billetes.js (ya portado byte-a-byte,
// ver WebCore/Views/Shared/_Layout.cshtml) desde el atajo F3 del POS y el boton "Calcular
// efectivo" de Cajas/_AddOrEditEgresoCaja.cshtml (este ultimo estaba roto -- faltaba el modal,
// ver _CalculadoraBilletesModal.cshtml, ya portado). PDF via QuestPDF (GenerarDocsCore.cs),
// no iTextSharp (no corre en .NET Core). Sin Session["Usuario"]: usa el mismo stub Id=2/
// Admin=true/IdEmpresa=1/IdSucursal=2/Nombre="ger" que el resto de los controllers portados.
public class HomeController : Controller
{
    private sealed class StubEmpresaContext : IEmpresaContext
    {
        public int IdEmpresa => 1;
    }

    private readonly IEmpresaContext _empresa = new StubEmpresaContext();
    private readonly Negocio.Sucursal _oSucursalN;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

    private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
    {
        Id = 2,
        Admin = true,
        IdEmpresa = 1,
        IdSucursal = 2,
        Nombre = "ger"
    };

    public HomeController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        _env = env;
        _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa);
        _usuarioActual.Sucursal = _oSucursalN.findById(_usuarioActual.IdSucursal);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Port de Web/Controllers/HomeController.cs Utilidades()/DescargarAgenteImpresion() --
    // pagina de descarga de agentes locales (balanza/impresion), 100% estatica (sin acceso a
    // base de datos). Server.MapPath (MVC5) -> IWebHostEnvironment.WebRootPath (Core), mismo
    // criterio ya usado en VentasController (AFIP) y ProductosController (logo de etiquetas).
    public IActionResult Utilidades()
    {
        var model = new UtilitiesIndexVm
        {
            Agentes = new System.Collections.Generic.List<UtilityItemVm>
            {
                BuildUtilityItem(
                    "Agente de balanza",
                    "Agente local",
                    "Lee la balanza desde la PC local y expone la API en 127.0.0.1 para POS y otras pantallas web.",
                    "Content/downloads/Carnisys.Balanza.Agent.zip",
                    "Carnisys.Balanza.Agent.zip",
                    "Incluye ejecutable, configuración inicial y script de instalación local."),
                BuildUtilityItem(
                    "Agente de impresión",
                    "Agente local",
                    "Permite imprimir tickets desde la terminal local sin depender del servidor web.",
                    "Content/downloads/CarniSys.PrintAgent.zip",
                    "CarniSys.PrintAgent.zip",
                    "Instalar en la terminal donde está conectada la impresora térmica.")
            },
            OtrasUtilidades = new System.Collections.Generic.List<UtilityItemVm>
            {
                new UtilityItemVm
                {
                    Nombre = "Próximas utilidades",
                    Categoria = "Catálogo",
                    Descripcion = "Este sector queda preparado para sumar nuevas herramientas locales o instaladores del sistema.",
                    Estado = "Próximamente",
                    Version = "-",
                    Disponible = false,
                    NotaInstalacion = "Aquí podremos ir agregando nuevas utilidades sin tocar el resto del menú."
                }
            }
        };

        ViewBag.Title = "CarniSys | Utilidades";
        return View(model);
    }

    public IActionResult DescargarAgenteImpresion()
    {
        string path = System.IO.Path.Combine(_env.WebRootPath, "Content", "downloads", "CarniSys.PrintAgent.zip");
        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "application/zip", "CarniSys.PrintAgent.zip");
    }

    private UtilityItemVm BuildUtilityItem(string nombre, string categoria, string descripcion, string relativePath, string archivoNombre, string notaInstalacion)
    {
        string physicalPath = System.IO.Path.Combine(_env.WebRootPath, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        bool disponible = System.IO.File.Exists(physicalPath);
        var info = disponible ? new System.IO.FileInfo(physicalPath) : null;

        return new UtilityItemVm
        {
            Nombre = nombre,
            Categoria = categoria,
            Descripcion = descripcion,
            Estado = disponible ? "Disponible" : "No disponible",
            Version = info != null ? info.LastWriteTime.ToString("dd/MM/yyyy HH:mm") : "-",
            ArchivoUrl = disponible ? Url.Content("~/" + relativePath) : string.Empty,
            ArchivoNombre = archivoNombre,
            ArchivoTamano = info != null ? FormatFileSize(info.Length) : "-",
            NotaInstalacion = notaInstalacion,
            Disponible = disponible
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes <= 0) return "0 KB";

        double kb = bytes / 1024d;
        if (kb < 1024d)
        {
            return kb.ToString("0.#", CultureInfo.InvariantCulture) + " KB";
        }

        double mb = kb / 1024d;
        return mb.ToString("0.##", CultureInfo.InvariantCulture) + " MB";
    }

    [HttpPost]
    public JsonResult ImprimirCalculadoraBilletesPayload([FromBody] CalculadoraBilletesPrintVm request)
    {
        try
        {
            if (request == null)
                return Json(new { ok = false, mensaje = "No se recibieron datos para imprimir." });

            int ticketMm = request.TicketMm == 58 ? 58 : 80;
            return Json(new
            {
                ok = true,
                ticketMm,
                ticketLines = ConstruirLineasCalculadoraBilletes(request, ticketMm)
            });
        }
        catch (System.Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpPost]
    public JsonResult DescargarCalculadoraBilletesPdf([FromBody] CalculadoraBilletesPrintVm request)
    {
        try
        {
            if (request == null)
                return Json(new { ok = false, mensaje = "No se recibieron datos para generar el PDF." });

            string titulo = string.IsNullOrWhiteSpace(request.Titulo) ? "Detalle de billetes" : request.Titulo.Trim();
            byte[] bytes = GenerarDocsCore.GenerarPdfCalculadoraBilletes(titulo, request.Total, NormalizarDetalleCalculadoraBilletes(request));

            return Json(new
            {
                ok = true,
                fileName = "DetalleBilletes.pdf",
                base64 = System.Convert.ToBase64String(bytes)
            });
        }
        catch (System.Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    private System.Collections.Generic.List<string> ConstruirLineasCalculadoraBilletes(CalculadoraBilletesPrintVm request, int ticketMm)
    {
        int cantMaxChar = ticketMm == 58 ? 32 : 43;
        string titulo = string.IsNullOrWhiteSpace(request.Titulo) ? "Detalle billetes" : request.Titulo.Trim();

        var empresa = _usuarioActual.Sucursal?.Empresa;
        string empresaNombre = empresa != null
            ? (!string.IsNullOrWhiteSpace(empresa.NombreFantasia) ? empresa.NombreFantasia : empresa.RazonSocialAfip) ?? "CarniSys"
            : "CarniSys";

        string Truncar(string texto, int maximo)
        {
            texto ??= "";
            return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
        }

        string Centrar(string texto, int ancho)
        {
            texto = Truncar(texto, ancho);
            int espaciosIzquierda = (ancho - texto.Length) / 2;
            if (espaciosIzquierda < 0) espaciosIzquierda = 0;
            return new string(' ', espaciosIzquierda) + texto;
        }

        var lineas = new System.Collections.Generic.List<string>
        {
            Centrar(titulo, cantMaxChar),
            Centrar(empresaNombre, cantMaxChar),
            Truncar("Fecha: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), cantMaxChar),
            new string('-', cantMaxChar),
            "Total $: " + request.Total.ToString("N2"),
            " ",
            " ",
            "Detalles:",
            NormalizarDetalleCalculadoraBilletes(request),
            " ",
            " ",
            " ",
            ".",
            " "
        };

        return lineas;
    }

    private string NormalizarDetalleCalculadoraBilletes(CalculadoraBilletesPrintVm request)
    {
        if (request == null)
            return "";

        if (request.Denominaciones != null && request.Denominaciones.Count > 0)
        {
            var partes = request.Denominaciones
                .Where(x => x != null && x.Denominacion > 0)
                .Select(x => x.Cantidad.ToString() + " x " + x.Denominacion.ToString("N0"))
                .ToList();

            if (request.Monedas > 0)
                partes.Add("Monedas " + request.Monedas.ToString("N2"));

            return string.Join(" + ", partes);
        }

        return request.DetalleTexto ?? "";
    }
}
