// Port de Web/Controllers/VentasController.cs (3336 lineas en el original) para el Modulo 8
// (Ventas y POS) -- ver docs/DECISIONS.md. Este es el SLICE 1: solo las acciones de lectura, sin
// AFIP, sin POS transaccional, sin impresion/email/PDF. El controller original tiene 30 acciones;
// las 23 restantes NO se portan en este slice:
//
//  - AFIP (bloqueante conocido desde el plan original, requiere mini-spike aparte, ver
//    docs/10-migracion-aspnet-core/README.md): ProbarLoginAfip, GenerarFactura,
//    NuevaFacturaSinVenta, CrearVentaManualParaFactura, LimpiarLineasVentaManual,
//    CerrarVentaSinFacturar, GenerarNotaCredito.
//  - POS transaccional (venta real con balanza/codigo de barras/caja, arriesgado para portar sin
//    el juez de paridad del modulo completo, CLAUDE.md seccion 11): POS, AutorizarOperadorPOS,
//    CerrarOperadorPOS, AutorizarModuloVentas, AutorizarOperadorModuloVentas, BuscarExpendiosPOS,
//    ObtenerExpendioPOS, BuscarProducto, AgregarProducto, FinalizarVenta, ModificarVenta.
//  - Impresion/email/PDF (mismo bloqueante que FinanzasController, iTextSharp sin decidir +
//    envio real de email, CLAUDE.md secciones 1.2/4): ImprimirTicket, ImprimirTicketPayload,
//    ImprimirIngresoBilletesPayload, DescargarAgenteImpresion, ObtenerDatosEmailComprobante,
//    EnviarComprobanteEmail, Imprimir.
//
// Consecuencia visible en las vistas: los botones "Modificar venta"/"Cambiar Forma de Pago"
// (apuntan a POS, no portado), "Factura"/"Imprimir"/"Email" (AFIP e impresion/email, no
// portados) se EXCLUYEN de las vistas en vez de dejarlos wireados a una accion inexistente --
// mismo criterio que FinanzasController excluyo CtaCtePersona/AddOrEditPago en cascada.
//
// Bypass de permisos: igual que el resto de la migracion (Cajas/Finanzas/Reportes), el usuario
// stub tiene Admin=true, asi que TODOS los chequeos de PermisosHelper.TienePermiso*/
// VistaAccesoDenegado/ConfigurarAdvertenciaFechaEnVivo del original siempre resuelven "sin
// restriccion" -- se omiten directamente en vez de portarlos como no-ops. Por la misma razon se
// omiten los helpers que solo existen para calcular esos permisos (PuedeModificarUltimaVenta,
// PuedeCambiarFormaPago, TienePermisoAdministrativoSobreVenta y sus "Motivo") ya que solo los
// usaban los botones ya excluidos arriba.
//
// PerformanceInstrumentation.LogServerEvent (llamado en el DetalleVenta original) no se porta:
// no existe en WebCore/Utilidades.Core (ver docs/DECISIONS.md, spike de Utilidades.Core), y
// ningun otro controller de WebCore lo usa.
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class VentasController : Controller
    {
        // Tamano de pagina de la carga progresiva de Facturas -- mismo criterio que
        // ProductosController.CatalogoGlobalTamanoPagina.
        private const int FacturasTamanoPagina = 50;

        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;

        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.CierreCaja _oCierreN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public VentasController(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;

            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oVentaN = new Negocio.Venta(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oCierreN = new Negocio.CierreCaja(_empresa, _param);
        }

        private async System.Threading.Tasks.Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: false);
                if (viewResult.View == null)
                    throw new InvalidOperationException("No se encontró la vista parcial '" + viewName + "'.");

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    new TempDataDictionary(HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1)
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (desde == hasta && desde.Hour == 0)
                hasta = hasta.AddDays(1);

            var sucursales = _oSucursalN.findAll();

            ViewBag.Sucursales = sucursales;
            ViewBag.IdSucursalSeleccionada = idSucursal;

            List<Entidades.Venta> ventas = _oVentaN.getAllVentas(desde, hasta, "", -1, -1, idSucursal, false, false) ?? new List<Entidades.Venta>();
            ventas = ventas
                .Where(v => v != null && v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .ToList();

            ViewBag.TotalFiltrado = ventas.Sum(v => v.TotalImporte);
            return View(ventas);
        }

        public IActionResult Facturas(
            DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1,
            string cliente = "", string vendedor = "", string formasPago = "", string tiposComprobante = "")
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (desde == hasta && desde.Hour == 0)
                hasta = hasta.AddDays(1);

            var sucursales = _oSucursalN.findAll();
            ViewBag.Sucursales = sucursales;
            ViewBag.IdSucursalSeleccionada = idSucursal;

            var formasPagoSeleccionadas = SepararValoresCsv(formasPago);
            var codigosComprobante = TipoComprobanteFacturas.ObtenerCodigos(SepararValoresCsv(tiposComprobante));

            var model = new FacturasIndexVm
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                IdSucursal = idSucursal,
                Cliente = cliente ?? "",
                Vendedor = vendedor ?? "",
                FormasPagoCsv = formasPago ?? "",
                TiposComprobanteCsv = tiposComprobante ?? ""
            };

            CargarPaginaFacturas(model, formasPagoSeleccionadas, codigosComprobante, pagina: 1, incluirResumen: true);

            return View("~/Views/Ventas/Facturas.cshtml", model);
        }

        // Endpoint AJAX de la carga progresiva (scroll infinito de 50 en 50). Mismo patron que
        // ProductosController: JSON con HTML pre-renderizado (RenderPartialViewToStringAsync) +
        // "hayMas" calculado con peek-ahead (se pide FacturasTamanoPagina+1 filas).
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> BuscarFacturas(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, string formasPago, string tiposComprobante,
            int pagina = 1)
        {
            var formasPagoSeleccionadas = SepararValoresCsv(formasPago);
            var codigosComprobante = TipoComprobanteFacturas.ObtenerCodigos(SepararValoresCsv(tiposComprobante));

            var model = new FacturasIndexVm
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                IdSucursal = idSucursal,
                Cliente = cliente ?? "",
                Vendedor = vendedor ?? ""
            };

            CargarPaginaFacturas(model, formasPagoSeleccionadas, codigosComprobante, pagina, incluirResumen: pagina == 1);

            string html = await RenderPartialViewToStringAsync("_FacturasRows", model.Facturas);

            return Json(new
            {
                ok = true,
                html,
                pagina,
                hayMas = model.HayMas,
                cantidad = pagina == 1 ? (int?)model.Cantidad : null,
                totalFacturado = pagina == 1 ? (decimal?)model.TotalFacturado : null
            });
        }

        private void CargarPaginaFacturas(
            FacturasIndexVm model, List<string> formasPagoSeleccionadas, List<int> codigosComprobante,
            int pagina, bool incluirResumen)
        {
            var facturas = _oVentaN.BuscarFacturasPagina(
                model.FechaDesde, model.FechaHasta, model.IdSucursal,
                model.Cliente, model.Vendedor, formasPagoSeleccionadas, codigosComprobante,
                pagina, FacturasTamanoPagina, cantidadExtra: 1) ?? new List<Entidades.FacturaElectronica>();

            model.HayMas = facturas.Count > FacturasTamanoPagina;
            if (model.HayMas)
                facturas.RemoveAt(facturas.Count - 1);

            model.Facturas = new List<FacturaListadoItemVm>();
            foreach (var factura in facturas.Where(x => x != null && x.Venta != null))
            {
                model.Facturas.Add(new FacturaListadoItemVm
                {
                    Factura = factura,
                    Venta = factura.Venta,
                    FacturaAsociada = EsNotaCreditoAfip(factura.CodTipoCbteAfip) ? ObtenerFacturaAsociadaVenta(factura.IdVenta) : null,
                    NotaCreditoAsociada = EsNotaCreditoAfip(factura.CodTipoCbteAfip) ? null : ObtenerNotaCreditoAsociadaVenta(factura.IdVenta)
                });
            }

            if (incluirResumen)
            {
                var resumen = _oVentaN.ObtenerFacturasResumen(
                    model.FechaDesde, model.FechaHasta, model.IdSucursal,
                    model.Cliente, model.Vendedor, formasPagoSeleccionadas, codigosComprobante);
                model.Cantidad = resumen.Cantidad;
                model.TotalFacturado = resumen.Total;
            }
        }

        public IActionResult Lineas(DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1, string cliente = "", string vendedor = "", string formasPago = "", string producto = "")
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (hasta < desde)
                hasta = desde;

            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var formasPagoSeleccionadas = SepararValoresCsv(formasPago);

            List<Entidades.Venta> ventas = _oVentaN.getAllVentas(desde, hasta, "", -1, -1, idSucursal, false, true) ?? new List<Entidades.Venta>();
            ventas = ventas.Where(v => v != null && v.FechaVenta >= desde && v.FechaVenta <= hasta).ToList();
            ventas = ventas
                .Where(v => CoincideTexto(v != null && v.Persona != null ? v.Persona.RazonSocial : "", cliente)
                    || CoincideTexto(v != null && v.Persona != null ? v.Persona.Identificacion : "", cliente)
                    || string.IsNullOrWhiteSpace(cliente))
                .Where(v => CoincideTexto(v != null && v.Vendedor != null ? v.Vendedor.Nombre : "", vendedor) || string.IsNullOrWhiteSpace(vendedor))
                .Where(v => formasPagoSeleccionadas.Count == 0
                    || formasPagoSeleccionadas.Contains((v != null ? (v.FormaPago ?? "") : "").Trim(), StringComparer.OrdinalIgnoreCase))
                .ToList();

            var model = new VentaLineasIndexVm
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                IdSucursal = idSucursal,
                Cliente = cliente ?? "",
                Vendedor = vendedor ?? "",
                Producto = producto ?? "",
                FormasPagoCsv = formasPago ?? "",
                FormasPagoSeleccionadas = formasPagoSeleccionadas
            };

            foreach (var venta in ventas.OrderByDescending(x => x.FechaVenta))
            {
                var lineas = (venta.LineasVenta ?? new List<Entidades.LineaVenta>())
                    .Where(x => CoincideProductoVenta(x, producto))
                    .ToList();

                if (lineas.Count == 0)
                    continue;

                var grupo = new VentaLineasGrupoVm
                {
                    IdVenta = venta.IdVenta,
                    CollapseId = "ventaLineas_" + venta.IdVenta,
                    Titulo = "VENTA ID: " + venta.IdVenta,
                    Subtitulo = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    ResumenCompacto = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    ResumenSecundario = "Venta ID: " + venta.IdVenta,
                    TotalTexto = venta.TotalImporte.ToString("C"),
                    TotalImporte = Convert.ToDecimal(venta.TotalImporte),
                    TotalKg = lineas.Sum(x => Convert.ToDecimal(x.CantKg)),
                    EditUrl = Url.Action("DetalleVenta", "Ventas", new { id = venta.IdVenta })
                };

                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Fecha", Valor = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Nro/comprobante", Valor = string.IsNullOrWhiteSpace(venta.NroRemito) ? "-" : venta.NroRemito });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal", Valor = venta.Sucursal != null ? venta.Sucursal.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Cliente", Valor = venta.Persona != null ? venta.Persona.RazonSocial : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Vendedor", Valor = venta.Vendedor != null ? venta.Vendedor.Nombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Forma de pago", Valor = string.IsNullOrWhiteSpace(venta.FormaPago) ? "-" : venta.FormaPago });

                foreach (var linea in lineas)
                {
                    float totalLinea = linea.CantKg * linea.PrecioKg;
                    grupo.Lineas.Add(new VentaLineaDetalleVm
                    {
                        Codigo = linea.Corte != null ? linea.Corte.Codigo.ToString() : "-",
                        Producto = linea.Corte != null ? linea.Corte.CorteDesc : "-",
                        CantidadKgTexto = linea.CantKg.ToString("N3"),
                        PrecioTexto = linea.PrecioKg.ToString("N2"),
                        TotalTexto = totalLinea.ToString("N2"),
                        CantidadKg = Convert.ToDecimal(linea.CantKg),
                        Precio = Convert.ToDecimal(linea.PrecioKg),
                        Total = Convert.ToDecimal(totalLinea)
                    });
                }

                model.Ventas.Add(grupo);
            }

            ViewBag.Title = "Lineas de venta";
            ViewBag.Sucursales = sucursales;

            return View("~/Views/Ventas/Lineas.cshtml", model);
        }

        public IActionResult MisVentas(bool desdePos = false, int idCierre = 0)
        {
            var user = _usuarioActual;

            var cierre = ObtenerCierreMisVentas(user, desdePos, idCierre);
            if (cierre == null)
            {
                ViewBag.Mensaje = "No hay una caja abierta para consultar ventas.";
                ViewBag.DesdePOS = desdePos;
                ViewBag.IdCierreActividad = idCierre;
                return PartialView("~/Views/Ventas/_MisVentas.cshtml", new List<Entidades.Venta>());
            }

            var ventas = ConvertirVentasResumen(_oVentaN.getVentasVendedorCierreCaja(cierre, false));

            ViewBag.DesdePOS = desdePos;
            ViewBag.IdCierreActividad = cierre.Id;
            ViewBag.CierreCaja = cierre;
            ViewBag.TotalVisible = ventas.Sum(v => v.TotalImporte);
            ViewBag.MostrarTotalMisVentas = true;
            ViewBag.TituloVentas = "Mis ventas";
            ViewBag.SubtituloVentas = cierre.UsuarioInicio != null && cierre.Sucursal != null
                ? cierre.UsuarioInicio.Nombre + " | " + cierre.Sucursal.sucursal
                : "";

            return PartialView("~/Views/Ventas/_MisVentas.cshtml", ventas);
        }

        // GET: Ventas/DetalleVenta/5
        public IActionResult DetalleVenta(int id, bool modal = false, bool desdePos = false, int idCierre = 0, string returnUrl = "")
        {
            Entidades.Venta venta = _oVentaN.getVentaById(id);
            if (venta == null)
                return NotFound();

            ViewBag.ModoModal = modal;
            ViewBag.DesdePOS = desdePos;
            ViewBag.IdCierreActividad = idCierre;
            ViewBag.ReturnUrlDetalle = DecodeReturnUrlIfNeeded(returnUrl);
            ViewBag.TieneFacturaVenta = _oVentaN.existeFactuElectParaVenta(venta.IdVenta) > 0;
            ViewBag.IdNotaCreditoVenta = _oVentaN.existeNotaCreditoParaVenta(venta.IdVenta);
            ViewBag.TieneNotaCreditoVenta = (int)ViewBag.IdNotaCreditoVenta > 0;

            if (modal)
                return PartialView(venta);

            return View(venta);
        }

        public IActionResult DetalleFactura(int id, string returnUrl = "")
        {
            var factura = _oVentaN.getFactuElecById(id);
            if (factura == null || factura.Id <= 0)
                return NotFound();

            var venta = factura.Venta ?? _oVentaN.getVentaById(factura.IdVenta);
            if (venta == null)
                return NotFound();

            var model = new FacturaDetalleVm
            {
                Factura = factura,
                Venta = venta,
                ReturnUrl = DecodeReturnUrlIfNeeded(returnUrl),
                FacturaAsociada = EsNotaCreditoAfip(factura.CodTipoCbteAfip) ? ObtenerFacturaAsociadaVenta(venta.IdVenta) : null,
                NotaCreditoAsociada = EsNotaCreditoAfip(factura.CodTipoCbteAfip) ? null : ObtenerNotaCreditoAsociadaVenta(venta.IdVenta)
            };

            return View("~/Views/Ventas/DetalleFactura.cshtml", model);
        }

        private Entidades.CierreCaja ObtenerCierreMisVentas(Entidades.Usuario user, bool desdePos, int idCierre)
        {
            if (idCierre > 0)
            {
                var cierrePorId = _oCierreN.findByIdOrLast(
                    new Entidades.CierreCaja { Id = idCierre },
                    Entidades.CierreCaja.tipoBusqueda.FindById,
                    ""
                );

                bool abierta = cierrePorId != null && (cierrePorId.UsuarioCierre == null || cierrePorId.UsuarioCierre.Id == 0);
                return abierta ? cierrePorId : null;
            }

            if (!desdePos || user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null)
                user.Sucursal = _oSucursalN.findById(user.IdSucursal);

            var cierre = new Entidades.CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = _oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            bool cajaAbierta = cierre != null && (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
            return cajaAbierta ? cierre : null;
        }

        private static string DecodeReturnUrlIfNeeded(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return returnUrl;

            if (returnUrl.StartsWith("/") || returnUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return returnUrl;

            try
            {
                string decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(returnUrl));
                return string.IsNullOrWhiteSpace(decoded) ? returnUrl : decoded;
            }
            catch
            {
                return returnUrl;
            }
        }

        private List<Entidades.Venta> ConvertirVentasResumen(DataTable dt)
        {
            var ventas = new List<Entidades.Venta>();
            if (dt == null)
                return ventas;

            foreach (DataRow row in dt.Rows)
            {
                var venta = new Entidades.Venta
                {
                    IdVenta = ObtenerValor(row, "idVenta", 0),
                    FechaVenta = ObtenerValor(row, "fechaVenta", DateTime.MinValue),
                    FormaPago = ObtenerValor(row, "formaPago", ""),
                    TipoComprobante = ObtenerTipoComprobante(row),
                    Observaciones = ObtenerValor(row, "observaciones", ""),
                    TotalImporte = ObtenerValor(row, "totalS", 0f),
                    Persona = new Entidades.Persona
                    {
                        razonSocial = ObtenerValor(row, "razonSocial", ""),
                        Identificacion = ""
                    },
                    Vendedor = new Entidades.Usuario
                    {
                        Nombre = ObtenerValor(row, "nombre", "")
                    }
                };

                ventas.Add(venta);
            }

            return ventas;
        }

        private T ObtenerValor<T>(DataRow row, string columnName, T valorDefault)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return valorDefault;

            return (T)Convert.ChangeType(row[columnName], typeof(T));
        }

        private char ObtenerTipoComprobante(DataRow row)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains("tipoComprobante") || row["tipoComprobante"] == DBNull.Value)
                return 'X';

            var texto = Convert.ToString(row["tipoComprobante"]);
            return string.IsNullOrWhiteSpace(texto) ? 'X' : texto[0];
        }

        private static List<string> SepararValoresCsv(string csv)
        {
            return (csv ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool CoincideTexto(string valor, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return true;

            return (valor ?? "").IndexOf(filtro.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool CoincideProductoVenta(Entidades.LineaVenta linea, string producto)
        {
            if (string.IsNullOrWhiteSpace(producto))
                return true;

            string filtro = producto.Trim();
            string codigo = linea != null && linea.Corte != null ? linea.Corte.Codigo.ToString() : "";
            string descripcion = linea != null && linea.Corte != null ? linea.Corte.CorteDesc : "";

            return codigo.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0
                || descripcion.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool EsNotaCreditoAfip(int codTipoCbteAfip)
        {
            return codTipoCbteAfip == FacturaElectronica.codNotaCreditoA_Afip
                || codTipoCbteAfip == FacturaElectronica.codNotaCreditoB_Afip
                || codTipoCbteAfip == FacturaElectronica.codNotaCreditoC_Afip;
        }

        private Entidades.FacturaElectronica ObtenerFacturaAsociadaVenta(int idVenta)
        {
            int idFactura = _oVentaN.existeFactuElectParaVenta(idVenta);
            return idFactura > 0 ? _oVentaN.getFactuElecById(idFactura) : null;
        }

        private Entidades.FacturaElectronica ObtenerNotaCreditoAsociadaVenta(int idVenta)
        {
            int idNotaCredito = _oVentaN.existeNotaCreditoParaVenta(idVenta);
            return idNotaCredito > 0 ? _oVentaN.getFactuElecById(idNotaCredito) : null;
        }
    }
}
