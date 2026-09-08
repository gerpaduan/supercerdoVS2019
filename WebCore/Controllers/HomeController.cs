using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
// no iTextSharp (no corre en .NET Core). Usuario/empresa reales via IUsuarioSesionService
// (login real, ver docs/DECISIONS.md "Login/Sesion real para WebCore" 2026-09-06) -- ya no
// hay stub hardcodeado.
public class HomeController : Controller
{
    private readonly WebCore.Services.IUsuarioSesionService _sesion;
    private readonly IEmpresaContext _empresa;
    private readonly IParametrosContext _param;
    private readonly Negocio.Sucursal _oSucursalN;
    private readonly Negocio.Venta _oVentaN;
    private readonly Negocio.CuentaCorriente _oCuentaCorrienteN;
    private readonly Negocio.Corte _oCorteN;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

    private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

    // Dependencias de Negocio para el dashboard post-login (Home/Index), agregadas 2026-09-07
    // (pedido explicito del usuario -- ver docs/DECISIONS.md), port de Web/Controllers/
    // HomeController.cs. Mismo patron que WebCore/Controllers/VentasController.cs: _param se
    // resuelve y recarga una sola vez por request, y de ahi cuelgan los Negocio.* que lo piden.
    public HomeController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, WebCore.Services.IUsuarioSesionService sesion)
    {
        _env = env;
        _sesion = sesion;
        _empresa = sesion.Empresa;

        _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
        _param.Reload();

        _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
        _oVentaN = WebCore.Infrastructure.NegocioFactory.CrearVenta(_empresa, _param);
        _oCuentaCorrienteN = WebCore.Infrastructure.NegocioFactory.CrearCuentaCorriente(_empresa, _param);
        _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
    }

    // Port de Web/Controllers/HomeController.cs Index() (2026-09-07, pedido explicito del usuario
    // -- ver docs/DECISIONS.md). El dashboard de DATOS (KPIs/graficos/tablas) es solo para
    // administradores -- Session["Usuario"].Admin del clasico pasa a _usuarioActual.Admin directo
    // (nunca null: [Authorize]/FallbackPolicy global de Program.cs ya lo garantiza). Los 8
    // endpoints JSON de abajo cargan los datos por AJAX progresivo desde wwwroot/Scripts/app/
    // home-dashboard.js, para no frenar el render inicial de esta vista.
    public IActionResult Index()
    {
        var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();

        var model = new HomeDashboardIndexVm
        {
            PuedeVerDashboardDatos = _usuarioActual.Admin,
            IdSucursalSeleccionada = 0,
            PeriodoDefault = "hoy",
            Sucursales = ConstruirSucursalesDashboard(sucursales)
        };

        return View(model);
    }

    [HttpGet]
    public JsonResult ObtenerResumenDashboard(string periodo = "hoy", int? idSucursal = null)
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var filtro = CrearFiltroDashboard(periodo, idSucursal);
            var ventas = ObtenerVentasDashboard(filtro);
            var saldos = ObtenerSaldosCuentaCorriente();

            var data = new DashboardResumenVm
            {
                VentasTotales = ventas.Sum(x => ToDecimal(x != null ? x.TotalImporte : 0f)),
                CantidadVentas = ventas.Count,
                CantidadClientes = CalcularClientesAtendidos(ventas),
                PromedioPorVenta = ventas.Count == 0 ? 0m : ventas.Average(x => ToDecimal(x != null ? x.TotalImporte : 0f)),
                SaldoACobrar = saldos.Where(x => x.Saldo < 0m).Sum(x => x.Saldo),
                SaldoAPagar = saldos.Where(x => x.Saldo > 0m).Sum(x => Math.Abs(x.Saldo)),
                PeriodoEtiqueta = filtro.PeriodoEtiqueta,
                SucursalEtiqueta = filtro.SucursalEtiqueta
            };

            return Json(new { ok = true, data });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerVentasPorHora(string periodo = "hoy", int? idSucursal = null)
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var filtro = CrearFiltroDashboard(periodo, idSucursal);
            var ventas = ObtenerVentasDashboard(filtro);
            var puntos = new List<DashboardSerieHoraVm>();

            for (int hora = 0; hora < 24; hora++)
            {
                int horaActual = hora;
                var ventasHora = ventas.Where(x => x != null && x.FechaVenta.Hour == horaActual).ToList();
                puntos.Add(new DashboardSerieHoraVm
                {
                    Hora = horaActual.ToString("00") + ":00",
                    Total = ventasHora.Sum(x => ToDecimal(x.TotalImporte)),
                    CantidadVentas = ventasHora.Count
                });
            }

            return Json(new
            {
                ok = true,
                data = new
                {
                    periodo = filtro.PeriodoEtiqueta,
                    sucursal = filtro.SucursalEtiqueta,
                    labels = puntos.Select(x => x.Hora).ToList(),
                    series = puntos
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerTopProductosVendidos(string periodo = "hoy", int? idSucursal = null)
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var filtro = CrearFiltroDashboard(periodo, idSucursal);
            int sucursalReporte = filtro.IdSucursalSeleccionada > 0 ? filtro.IdSucursalSeleccionada : 0;
            var dt = _oCorteN.TotalPorCortesVendidos("", sucursalReporte, filtro.FechaDesde, filtro.FechaHasta, "", 0, 0)
                ?? new DataTable();

            var items = new List<DashboardTopProductoVm>();
            foreach (DataRow row in dt.Rows)
            {
                long codigo = LeerLong(row, "Codigo", "codigo");
                string producto = LeerString(row, "Corte", "Producto", "corte");
                string productoNormalizado = (producto ?? "").Trim();

                items.Add(new DashboardTopProductoVm
                {
                    IdCorte = 0,
                    Codigo = codigo,
                    Producto = productoNormalizado,
                    Kg = LeerDecimalPrimeraCoincidencia(row, "Total Kgs", "Total Kg", "Kgs", "Kg", "cantKg", "CantKg"),
                    Importe = LeerDecimalPrimeraCoincidencia(row, "totalS", "Total $", "Total", "Importe", "Total Importe")
                });
            }

            items = items
                .Where(x => !string.IsNullOrWhiteSpace(x.Producto))
                .OrderByDescending(x => x.Kg)
                .ThenByDescending(x => x.Importe)
                .Take(10)
                .ToList();

            return Json(new { ok = true, data = items });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerTopDeudores()
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var items = ObtenerSaldosCuentaCorriente()
                .Where(x => x.Saldo < 0m)
                .OrderBy(x => x.Saldo)
                .Take(10)
                .ToList();

            return Json(new { ok = true, data = items, alcance = "global" });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerTopAcreedores()
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var items = ObtenerSaldosCuentaCorriente()
                .Where(x => x.Saldo > 0m)
                .Select(x => new DashboardSaldoPersonaVm
                {
                    IdPersona = x.IdPersona,
                    Persona = x.Persona,
                    Identificacion = x.Identificacion,
                    Saldo = Math.Abs(x.Saldo)
                })
                .OrderByDescending(x => x.Saldo)
                .Take(10)
                .ToList();

            return Json(new { ok = true, data = items, alcance = "global" });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerUltimasVentas(string periodo = "hoy", int? idSucursal = null)
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var filtro = CrearFiltroDashboard(periodo, idSucursal);
            var items = ObtenerVentasDashboard(filtro)
                .OrderByDescending(x => x.FechaVenta)
                .Take(10)
                .Select(x => new DashboardUltimaVentaVm
                {
                    IdVenta = x.IdVenta,
                    FechaHora = x.FechaVenta.ToString("dd/MM HH:mm"),
                    Cliente = ObtenerNombreCliente(x),
                    Total = ToDecimal(x.TotalImporte),
                    Usuario = x.Vendedor != null ? (x.Vendedor.Nombre ?? "") : "-",
                    Sucursal = x.Sucursal != null ? (x.Sucursal.SucursalNombre ?? "") : "-",
                    DetalleUrl = Url.Action("DetalleVenta", "Ventas", new { id = x.IdVenta })
                })
                .ToList();

            return Json(new { ok = true, data = items });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerUltimosElaborados(string periodo = "hoy", int? idSucursal = null)
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var filtro = CrearFiltroDashboard(periodo, idSucursal);
            var dt = _oCorteN.obtenerUltimosElaboradosDashboard(5, filtro.IdSucursalConsulta, filtro.FechaDesde, filtro.FechaHasta)
                ?? new DataTable();

            var resultado = new List<Tuple<DateTime, DashboardUltimoElaboradoVm>>();
            foreach (DataRow row in dt.Rows)
            {
                DateTime fecha = LeerDate(row, "Fecha", "fechaEmbutido", "fecha");
                resultado.Add(Tuple.Create(fecha, new DashboardUltimoElaboradoVm
                {
                    Id = LeerInt(row, "Id", "idEmbutido"),
                    Fecha = fecha == DateTime.MinValue ? "-" : fecha.ToString("dd/MM HH:mm"),
                    Producto = LeerString(row, "Embutido", "Elaborado", "corte"),
                    Cantidad = LeerDecimalPrimeraCoincidencia(row, "Kgs", "kgs"),
                    Sucursal = LeerString(row, "Sucursal", "sucursal"),
                    Usuario = LeerString(row, "Usuario", "usuario", "CreadoPor")
                }));
            }

            var items = resultado
                .Where(x => x.Item2 != null && !string.IsNullOrWhiteSpace(x.Item2.Producto))
                .OrderByDescending(x => x.Item1)
                .Select(x => x.Item2)
                .ToList();

            return Json(new { ok = true, data = items });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerFinanzasDashboard()
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var data = ConstruirDashboardFinanzas();
            return Json(new { ok = true, data });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }

    [HttpGet]
    public JsonResult ObtenerUltimosMovimientosDashboard()
    {
        try
        {
            var acceso = ValidarAccesoDashboardAdmin();
            if (acceso != null) return acceso;

            var data = ObtenerUltimosMovimientosDashboardData(5);
            return Json(new { ok = true, data });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
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

    // ---- Helpers privados del dashboard post-login (Home/Index), port literal de
    // Web/Controllers/HomeController.cs (2026-09-07, pedido explicito del usuario -- ver
    // docs/DECISIONS.md). Unico cambio de infraestructura: Session["Usuario"] pasa a
    // _usuarioActual (via IUsuarioSesionService, nunca null bajo [Authorize]/FallbackPolicy).

    private HomeDashboardFiltro CrearFiltroDashboard(string periodo, int? idSucursal)
    {
        DateTime hoy = DateTime.Today;
        DateTime desde = hoy;
        string periodoNormalizado = (periodo ?? "hoy").Trim().ToLowerInvariant();
        string periodoEtiqueta = "Hoy";

        switch (periodoNormalizado)
        {
            case "7dias":
                desde = hoy.AddDays(-6);
                periodoEtiqueta = "Ultimos 7 dias";
                break;
            case "mes":
                desde = new DateTime(hoy.Year, hoy.Month, 1);
                periodoEtiqueta = "Este mes";
                break;
            default:
                periodoNormalizado = "hoy";
                desde = hoy;
                periodoEtiqueta = "Hoy";
                break;
        }

        int sucursalSeleccionada = idSucursal ?? (_usuarioActual != null && _usuarioActual.IdSucursal > 0 ? _usuarioActual.IdSucursal : 0);
        string sucursalEtiqueta = "Todas las sucursales";
        if (sucursalSeleccionada > 0)
        {
            var sucursal = _oSucursalN.findById(sucursalSeleccionada);
            sucursalEtiqueta = sucursal != null && !string.IsNullOrWhiteSpace(sucursal.SucursalNombre)
                ? sucursal.SucursalNombre
                : "Sucursal " + sucursalSeleccionada.ToString(CultureInfo.InvariantCulture);
        }

        return new HomeDashboardFiltro
        {
            FechaDesde = desde.Date,
            FechaHasta = hoy.Date.AddDays(1).AddTicks(-1),
            IdSucursalSeleccionada = sucursalSeleccionada,
            IdSucursalConsulta = sucursalSeleccionada > 0 ? sucursalSeleccionada : -1,
            Periodo = periodoNormalizado,
            PeriodoEtiqueta = periodoEtiqueta,
            SucursalEtiqueta = sucursalEtiqueta
        };
    }

    private JsonResult ValidarAccesoDashboardAdmin()
    {
        if (_usuarioActual == null)
            return Json(new { ok = false, mensaje = "Sesion vencida." });

        if (!_usuarioActual.Admin)
            return Json(new { ok = false, mensaje = "El dashboard de datos esta disponible solo para administradores." });

        return null;
    }

    private List<SelectListItem> ConstruirSucursalesDashboard(List<Entidades.Sucursal> sucursales)
    {
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Todas las sucursales", Value = "0" }
        };

        foreach (var sucursal in (sucursales ?? new List<Entidades.Sucursal>())
            .Where(x => x != null)
            .OrderBy(x => x.SucursalNombre ?? ""))
        {
            items.Add(new SelectListItem
            {
                Text = sucursal.SucursalNombre ?? ("Sucursal " + sucursal.IdSucursal.ToString(CultureInfo.InvariantCulture)),
                Value = sucursal.IdSucursal.ToString(CultureInfo.InvariantCulture)
            });
        }

        return items;
    }

    private List<Entidades.Venta> ObtenerVentasDashboard(HomeDashboardFiltro filtro)
    {
        var ventas = _oVentaN.getAllVentas(
            filtro.FechaDesde,
            filtro.FechaHasta,
            "",
            -1,
            -1,
            filtro.IdSucursalConsulta,
            false,
            false) ?? new List<Entidades.Venta>();

        return ventas
            .Where(x => x != null)
            .Where(x => x.FechaVenta >= filtro.FechaDesde && x.FechaVenta <= filtro.FechaHasta)
            .Where(x => !string.Equals((x.Estado ?? "").Trim(), "ANULADO", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private List<DashboardSaldoPersonaVm> ObtenerSaldosCuentaCorriente()
    {
        var dt = _oCuentaCorrienteN.obtenerCtasCtes("", null, "DESC") ?? new DataTable();
        var items = new List<DashboardSaldoPersonaVm>();

        foreach (DataRow row in dt.Rows)
        {
            decimal saldo = LeerDecimalPrimeraCoincidencia(row, "Saldo", "saldo");
            if (saldo == 0m)
                continue;

            items.Add(new DashboardSaldoPersonaVm
            {
                IdPersona = LeerInt(row, "idPersona", "IdPersona", "ID"),
                Persona = LeerString(row, "Razon Social", "razonSocial", "RazonSocial", "Persona"),
                Identificacion = LeerString(row, "Identificacion", "identificacion", "CUIT", "Cuit"),
                Saldo = saldo
            });
        }

        return items;
    }

    private DashboardFinanzasViewModel ConstruirDashboardFinanzas()
    {
        var model = new DashboardFinanzasViewModel();

        var dtResumen = _oCuentaCorrienteN.obtenerResumenDashboard() ?? new DataTable();
        if (dtResumen.Rows.Count > 0)
        {
            var row = dtResumen.Rows[0];
            model.Resumen = new DashboardFinanzasResumenVm
            {
                CantidadConSaldo = LeerInt(row, "CantidadConSaldo"),
                CantidadDeudores = LeerInt(row, "CantidadDeudores"),
                CantidadAcreedores = LeerInt(row, "CantidadAcreedores"),
                TotalACobrar = LeerDecimalPrimeraCoincidencia(row, "TotalACobrar"),
                TotalAPagar = LeerDecimalPrimeraCoincidencia(row, "TotalAPagar")
            };
        }

        var dtMovimientos = _oCuentaCorrienteN.obtenerUltimosPagosDashboard(5) ?? new DataTable();
        foreach (DataRow row in dtMovimientos.Rows)
        {
            DateTime fecha = LeerDate(row, "fecha", "Fecha");
            string persona = LeerString(row, "razonSocial", "Razon Social", "Persona");
            string tipo = LeerString(row, "Operacion", "operacion");

            model.Movimientos.Add(new DashboardFinanzasMovimientoVm
            {
                Fecha = fecha == DateTime.MinValue ? "-" : fecha.ToString("dd/MM/yyyy HH:mm"),
                Persona = string.IsNullOrWhiteSpace(persona) ? "-" : persona,
                Tipo = string.IsNullOrWhiteSpace(tipo) ? "-" : tipo,
                Monto = LeerDecimalPrimeraCoincidencia(row, "importe", "Importe")
            });
        }

        DateTime hoy = DateTime.Today;
        var dtCheques = _oCuentaCorrienteN.obtenerChequesPendientesDashboard(10, hoy) ?? new DataTable();
        foreach (DataRow row in dtCheques.Rows)
        {
            DateTime fechaPago = LeerDate(row, "fechaPago", "FechaPago");
            string estado = LeerString(row, "estado", "Estado");
            if (!string.Equals((estado ?? "").Trim(), Entidades.Cheque.EstadoEnum.PENDIENTE.ToString(), StringComparison.OrdinalIgnoreCase))
                continue;

            if (fechaPago == DateTime.MinValue || fechaPago.AddDays(40) > hoy)
                continue;

            model.Cheques.Add(new DashboardFinanzasChequeVm
            {
                NroCheque = LeerString(row, "nroCheque", "NroCheque"),
                Banco = LeerString(row, "banco", "Banco"),
                Titular = LeerString(row, "titular", "Titular"),
                FechaPago = fechaPago == DateTime.MinValue ? "-" : fechaPago.ToString("dd/MM/yyyy"),
                Importe = LeerDecimalPrimeraCoincidencia(row, "importe", "Importe"),
                Estado = estado,
                ObservacionCalculada = CalcularObservacionChequeDashboard(fechaPago, hoy)
            });
        }

        return model;
    }

    private static string CalcularObservacionChequeDashboard(DateTime fechaPago, DateTime fechaActual)
    {
        if (fechaPago == DateTime.MinValue)
            return "";

        DateTime hoy = fechaActual.Date;
        DateTime fecha = fechaPago.Date;

        if (fecha.AddDays(40) <= hoy)
            return "Cheque vencido";

        if (fecha <= hoy)
            return "Listo para cobrar";

        return "";
    }

    private List<DashboardMovimientoResumenVm> ObtenerUltimosMovimientosDashboardData(int cantidad)
    {
        var items = new List<DashboardMovimientoResumenVm>();
        var dtMovimientos = _oCorteN.obtenerUltimosMovimientosDashboard(cantidad)
            ?? new DataTable();

        foreach (DataRow row in dtMovimientos.Rows)
        {
            DateTime fecha = LeerDate(row, "Fecha Movimiento", "fechaMovimiento");
            string origen = LeerString(row, "Origen", "sucursalOrigen", "origen");
            string destino = LeerString(row, "Destino", "sucursalDestino", "destino");

            items.Add(new DashboardMovimientoResumenVm
            {
                Fecha = fecha == DateTime.MinValue ? "-" : fecha.ToString("dd/MM/yyyy HH:mm"),
                Origen = string.IsNullOrWhiteSpace(origen) ? "-" : origen,
                Destino = string.IsNullOrWhiteSpace(destino) ? "-" : destino
            });
        }

        return items;
    }

    private static int CalcularClientesAtendidos(IEnumerable<Entidades.Venta> ventas)
    {
        var clientes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int anonimos = 0;

        foreach (var venta in ventas ?? Enumerable.Empty<Entidades.Venta>())
        {
            if (venta == null)
                continue;

            var persona = venta.Persona;
            if (persona != null && persona.IdPersona > 0 && !Entidades.Persona.esConsumidorFinal(persona))
            {
                clientes.Add("ID:" + persona.IdPersona.ToString(CultureInfo.InvariantCulture));
                continue;
            }

            if (persona != null && !string.IsNullOrWhiteSpace(persona.RazonSocial) &&
                !string.Equals(persona.RazonSocial.Trim(), "Consumidor Final", StringComparison.OrdinalIgnoreCase))
            {
                clientes.Add("TXT:" + persona.RazonSocial.Trim());
                continue;
            }

            anonimos++;
        }

        return clientes.Count + anonimos;
    }

    private static decimal ToDecimal(float value)
    {
        return Convert.ToDecimal(value);
    }

    private static string ObtenerNombreCliente(Entidades.Venta venta)
    {
        if (venta == null || venta.Persona == null)
            return "Mostrador";

        if (!string.IsNullOrWhiteSpace(venta.Persona.RazonSocial))
            return venta.Persona.RazonSocial;

        if (!string.IsNullOrWhiteSpace(venta.Persona.Identificacion))
            return venta.Persona.Identificacion;

        return "Mostrador";
    }

    private static string NormalizarClaveProducto(string valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? ""
            : valor.Trim().ToUpperInvariant();
    }

    private static string LeerString(DataRow row, params string[] columnas)
    {
        foreach (var columna in columnas)
        {
            if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                return Convert.ToString(row[columna]) ?? "";
        }

        return "";
    }

    private static int LeerInt(DataRow row, params string[] columnas)
    {
        foreach (var columna in columnas)
        {
            if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
            {
                if (int.TryParse(Convert.ToString(row[columna]), out int valor))
                    return valor;
            }
        }

        return 0;
    }

    private static long LeerLong(DataRow row, params string[] columnas)
    {
        foreach (var columna in columnas)
        {
            if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
            {
                if (long.TryParse(Convert.ToString(row[columna]), out long valor))
                    return valor;
            }
        }

        return 0;
    }

    private static decimal LeerDecimalPrimeraCoincidencia(DataRow row, params string[] columnas)
    {
        foreach (var columna in columnas)
        {
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                continue;

            if (decimal.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal valor))
                return valor;

            if (decimal.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                return valor;
        }

        return 0m;
    }

    private static DateTime LeerDate(DataRow row, params string[] columnas)
    {
        foreach (var columna in columnas)
        {
            if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
            {
                if (DateTime.TryParse(Convert.ToString(row[columna]), out DateTime valor))
                    return valor;
            }
        }

        return DateTime.MinValue;
    }

    private class HomeDashboardFiltro
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int IdSucursalSeleccionada { get; set; }
        public int IdSucursalConsulta { get; set; }
        public string Periodo { get; set; }
        public string PeriodoEtiqueta { get; set; }
        public string SucursalEtiqueta { get; set; }
    }
}
