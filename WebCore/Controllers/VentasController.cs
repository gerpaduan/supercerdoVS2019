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
//
// AGREGADO (mini-spike AFIP, ver docs/DECISIONS.md): NuevaFacturaSinVenta/CrearVentaManualParaFactura/
// GenerarFactura/LimpiarLineasVentaManual -- el flujo de "facturar sin venta" del original, elegido
// a proposito porque es la unica via de facturacion que NO depende de POS (no portado). Usa
// AFIP.GenerarFacturaService tal cual (mismo codigo fuente que Web clasico, ver AFIP.csproj
// multi-target net472;net10.0) contra PRODUCCION real de AFIP -- BuildFacturaDTO/MapDtoToFactura
// portados sin cambios de logica fiscal. El resto de las acciones AFIP (GenerarNotaCredito,
// ProbarLoginAfip, CerrarVentaSinFacturar) siguen sin portar, no las necesita este flujo.
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
using WebCore.Models.DTO;

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
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;

        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.CierreCaja _oCierreN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Corte _oCorteN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public VentasController(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _env = env;

            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oVentaN = new Negocio.Venta(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oCierreN = new Negocio.CierreCaja(_empresa, _param);
            _oPersonaN = new Negocio.Persona(_empresa, _param);
            _oCorteN = new Negocio.Corte(_empresa, _param);
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

        // GET /Ventas/NuevaFacturaSinVenta -- arma el DTO "en blanco" para el formulario de
        // facturacion manual (sin venta de productos real detras, ver docs/DECISIONS.md). Devuelve
        // JSON, no una vista: la vista rica original (_FacturaElectronica.cshtml, 828 lineas) NO se
        // porta en este slice (fuera de alcance del mini-spike AFIP, ver docs/DECISIONS.md) -- este
        // endpoint sirve para verificar el armado del DTO y como base de una UI futura.
        [HttpGet]
        public IActionResult NuevaFacturaSinVenta()
        {
            var user = _usuarioActual;
            var sucursal = _oSucursalN.findById(user.IdSucursal);
            if (sucursal == null)
                return Json(new { ok = false, msg = "Sucursal inválida" });

            var ventaVacia = new Entidades.Venta
            {
                IdVenta = 0,
                Sucursal = sucursal,
                Persona = new Entidades.Persona(),
                LineasVenta = new List<Entidades.LineaVenta>(),
                FormaPago = Entidades.Venta.formaPagoEnum.Efectivo.ToString(),
                Observaciones = "",
                FechaVenta = DateTime.Now
            };

            var dto = BuildFacturaDTO(ventaVacia, new Entidades.FacturaElectronica());
            dto.IdVenta = 0;
            dto.NroDocAfip = "";
            dto.RazonSocialAFIP = "";
            dto.CondicionIvaAFIP = "Consumidor Final";
            dto.DomicilioAFIP = "";
            dto.AgruparItemUnitario = true;

            var alicuotasDt = _oCorteN.obtenerAlicuotasIva(false);
            var alicuotas = alicuotasDt.AsEnumerable()
                .Select(r => new { idIva = Convert.ToInt32(r["idIva"]), iva = Convert.ToDouble(r["iva"]) })
                .ToList();

            return Json(new
            {
                ok = true,
                dto,
                sucursalNombre = !string.IsNullOrWhiteSpace(sucursal.SucursalNombre) ? sucursal.SucursalNombre : sucursal.sucursal,
                alicuotas
            });
        }

        // POST /Ventas/CrearVentaManualParaFactura -- crea una venta real minima (Efectivo, sin
        // cta.cte, 1 linea con el total ingresado a mano) para poder facturarla con el circuito
        // normal de GenerarFactura sin tocarlo. La linea se borra despues, una vez que la factura
        // ya tiene CAE (ver LimpiarLineasVentaManual).
        [HttpPost]
        public IActionResult CrearVentaManualParaFactura(int idPersona, decimal montoTotal, int idAlicuotaIva, float alicuotaIva)
        {
            try
            {
                if (idPersona <= 0)
                    return Json(new { ok = false, msg = "Seleccioná un cliente." });

                if (montoTotal <= 0)
                    return Json(new { ok = false, msg = "El monto total debe ser mayor a cero." });

                var user = _usuarioActual;
                var persona = _oPersonaN.findById(idPersona);
                if (persona == null)
                    return Json(new { ok = false, msg = "Cliente inválido" });

                var sucursal = _oSucursalN.findById(user.IdSucursal);
                if (sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida" });

                var productoPlaceholder = _oCorteN.ObtenerCortesPorEmpresa(user.IdEmpresa, false).FirstOrDefault();
                if (productoPlaceholder == null)
                    return Json(new { ok = false, msg = "No hay ningún producto cargado para esta empresa." });

                var venta = new Entidades.Venta
                {
                    IdVenta = 0,
                    Persona = persona,
                    Sucursal = sucursal,
                    TipoVenta = "Caja",
                    FechaVenta = DateTime.Now,
                    Turno = "",
                    DiaFestivo = "",
                    Observaciones = "Factura manual sin venta asociada",
                    NroRemito = "",
                    FormaPago = Entidades.Venta.formaPagoEnum.Efectivo.ToString(),
                    EnCtaCte = false,
                    TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()),
                    Vendedor = user,
                    LineasVenta = new List<Entidades.LineaVenta>
                    {
                        new Entidades.LineaVenta
                        {
                            Corte = productoPlaceholder,
                            KgsTotalCalculado = 1,
                            CantKg = 1,
                            PrecioKg = (float)montoTotal,
                            Bonificacion = 0,
                            Estado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado),
                            IndexAnulado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado),
                            PesoBalanza = false,
                            IdExpendio = 0
                        }
                    }
                };

                int idVenta = _oVentaN.agregarVenta(venta);

                _oVentaN.actualizarAlicuotaLineaVenta(venta.LineasVenta[0].IdLineaVenta, idAlicuotaIva, alicuotaIva);

                return Json(new { ok = true, idVenta });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error creando la venta manual: " + ex.Message });
            }
        }

        // POST /Ventas/GenerarFactura -- llama a AFIP.GenerarFacturaService (produccion real, ver
        // docs/DECISIONS.md) y persiste el resultado. Logica identica a Web clasico: MapDtoToFactura
        // + AFIP.GenerarFacturaService.GenerarFactura, sin cambios.
        // El original postea esto como form-urlencoded (Web/Scripts/app/factura-electronica.js,
        // $form.serialize()), no JSON -- FacturaElectronicaDto se bindea por defecto desde los
        // campos del form (mismo binder implicito que MVC5), sin [FromBody].
        [HttpPost]
        public IActionResult GenerarFactura(FacturaElectronicaDto dto)
        {
            try
            {
                if (dto == null)
                    return Json(new { ok = false, msg = "No se recibieron datos de la factura." });

                var factura = MapDtoToFactura(dto);

                if (factura.Venta == null)
                    return Json(new { ok = false, msg = "Venta no encontrada" });

                int idFactExistente = _oVentaN.esVentaSinFacturar(factura.Venta.IdVenta, false);
                if (idFactExistente > 0)
                {
                    var fExist = _oVentaN.getFactuElecById(idFactExistente);
                    return Json(new
                    {
                        ok = true,
                        already = true,
                        facturaId = idFactExistente,
                        nro = fExist?.NroCbteAfip,
                        cae = fExist?.CAE1,
                        mensaje = "Ya existe una factura asociada a esta venta"
                    });
                }

                var afipSvc = new AFIP.GenerarFacturaService(factura.Venta, _env.ContentRootPath);
                var afipRes = afipSvc.GenerarFactura(factura, false);

                if (!afipRes.Ok)
                {
                    try
                    {
                        var factErr = new Entidades.FacturaElectronica
                        {
                            IdVenta = factura.Venta.IdVenta,
                            Error = true,
                            MensajeError = afipRes.Mensaje,
                            FechaError = DateTime.Now
                        };
                        _oVentaN.addOrEditFactuElec(factErr);
                    }
                    catch
                    {
                        // no bloquear la respuesta por fallo al guardar el error
                    }

                    return Json(new { ok = false, msg = "AFIP: " + afipRes.Mensaje });
                }

                try
                {
                    _oVentaN.addOrEditFactuElec(afipRes.Factura);
                }
                catch (Exception saveEx)
                {
                    return Json(new { ok = false, msg = "Error guardando factura en BD: " + saveEx.Message });
                }

                int idGuardado = _oVentaN.esVentaSinFacturar(factura.Venta.IdVenta, false);
                var facturaGuardada = idGuardado > 0 ? _oVentaN.getFactuElecById(idGuardado) : factura;

                return Json(new
                {
                    ok = true,
                    facturaId = idGuardado,
                    nro = facturaGuardada?.NroCbteAfip,
                    cae = facturaGuardada?.CAE1,
                    mensaje = afipRes.Mensaje ?? "Factura generada correctamente"
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error generando factura", error = ex.Message });
            }
        }

        // POST /Ventas/LimpiarLineasVentaManual -- borra la linea temporal de una venta manual,
        // una vez que la factura ya fue emitida (tiene CAE).
        [HttpPost]
        public IActionResult LimpiarLineasVentaManual(int idVenta)
        {
            try
            {
                if (idVenta <= 0)
                    return Json(new { ok = false, msg = "Venta inválida" });

                var venta = _oVentaN.getVentaById(idVenta);
                if (venta == null)
                    return Json(new { ok = false, msg = "Venta no encontrada" });

                if (venta.EnCtaCte || venta.FormaPago != Entidades.Venta.formaPagoEnum.Efectivo.ToString())
                    return Json(new { ok = false, msg = "Esta venta no corresponde a una factura manual." });

                _oVentaN.eliminarLineasVenta(idVenta);

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error limpiando la venta manual: " + ex.Message });
            }
        }

        // GET /Ventas/PreviewFacturaDto?idVenta=X -- helper de verificacion para probar el flujo de
        // facturacion manual de punta a punta sin portar la vista rica _FacturaElectronica.cshtml
        // (828 lineas, fuera de alcance de este slice, ver docs/DECISIONS.md). Devuelve el mismo
        // BuildFacturaDTO que arma la UI real, ya computado contra la Persona real de la venta --
        // asi el caller (curl/Postman) puede tomar estos valores tal cual y postearlos a
        // GenerarFactura, en vez de adivinar CodTipoCbteAfip/TipoDocAfip/etc a mano.
        [HttpGet]
        public IActionResult PreviewFacturaDto(int idVenta)
        {
            var venta = _oVentaN.getVentaById(idVenta);
            if (venta == null)
                return Json(new { ok = false, msg = "Venta no encontrada" });

            var dto = BuildFacturaDTO(venta, new Entidades.FacturaElectronica());
            return Json(new { ok = true, dto });
        }

        private FacturaElectronicaDto BuildFacturaDTO(Entidades.Venta venta, Entidades.FacturaElectronica factuElec)
        {
            var dto = new FacturaElectronicaDto();
            bool facturaYaGenerada = factuElec != null && factuElec.Id > 0;

            dto.IdVenta = venta.IdVenta;
            dto.IdFactura = factuElec.Id;

            dto.CodTipoCbteAfip = factuElec.CodTipoCbteAfip == 0 ?
                factuElec.getCodTipoCbteAFIP(venta.Sucursal.Empresa.EsRRII, venta.Persona.EsRRII(venta.Persona.IdIva), false) :
                factuElec.CodTipoCbteAfip;
            dto.DescTipoCbteAfip = factuElec.DescTipoCbteAfip;
            dto.LetraCbte = factuElec.getLetraId_TipoCbte(dto.CodTipoCbteAfip).ToString();
            dto.NroCbteAfip = factuElec.NroCbteAfip;
            dto.FechaEmisionAfip = factuElec.Id > 0
                ? factuElec.FechaEmisionAfip
                : venta.FechaVenta;

            dto.PtoVtaAfip = venta.Sucursal.CodPuntoVentaAfip.ToString();
            dto.EmisorRazonSocial = venta.Sucursal.Empresa.RazonSocialAfip;
            dto.EmisorCUIT = venta.Sucursal.Empresa.Cuit.ToString();
            dto.EmisorCondicionIVA = venta.Sucursal.Empresa.CondicionIVA;
            dto.EmisorDomicilio = venta.Sucursal.Direccion;
            dto.EmisorIngresosBrutos = venta.Sucursal.Empresa.Iibb.ToString();
            dto.EmisorInicioActividad = venta.Sucursal.Empresa.InicioActividad.ToString("dd/MM/yyyy");

            dto.TipoDocAfip = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.TipoDocAfip)
                ? factuElec.TipoDocAfip
                : (venta.Persona.IdIva == Entidades.FacturaElectronica.codCF_IvaAfip ?
                    Entidades.FacturaElectronica.codTipoDoc_SinIdentif : Entidades.FacturaElectronica.codTipoDoc_CUIT).ToString();
            dto.NroDocAfip = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.NroDocAfip)
                ? factuElec.NroDocAfip
                : venta.Persona.Cuit?.Replace("-", "");
            dto.RazonSocialAFIP = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.RazonSocialAFIP)
                ? factuElec.RazonSocialAFIP
                : venta.Persona.razonSocial;
            dto.CondicionIvaAFIP = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.CondicionIvaAFIP)
                ? factuElec.CondicionIvaAFIP
                : venta.Persona.Iva;
            dto.DomicilioAFIP = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.DomicilioAFIP)
                ? factuElec.DomicilioAFIP
                : $"{venta.Persona.Domicilio} - {venta.Persona.Ciudad}";
            dto.Whatsapp = venta.Persona.Telefono;

            dto.CondicionVenta = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.CondicionVenta)
                ? factuElec.CondicionVenta
                : dto.CondicionVenta;
            dto.FormaPago = facturaYaGenerada && !string.IsNullOrWhiteSpace(factuElec.FormaPago)
                ? factuElec.FormaPago
                : venta.FormaPago + (venta.PagoMixtoEfectivo > 0 ? " | Efectivo" : "");
            dto.PorcentajeFacturacion = facturaYaGenerada
                ? Convert.ToDecimal(factuElec.PorcentajeFacturacion)
                : 100m;
            dto.DescItemUnitario = facturaYaGenerada ? (factuElec.DescItemUnitario ?? "") : "";
            dto.AgruparItemUnitario = !string.IsNullOrWhiteSpace(dto.DescItemUnitario);
            dto.Observaciones = facturaYaGenerada
                ? (factuElec.Observaciones ?? "")
                : (venta.Observaciones ?? "");

            foreach (var l in venta.LineasVenta)
            {
                dto.Detalle.Add(new LineaVentaDto
                {
                    IdLineaVenta = l.IdLineaVenta,
                    IdCorte = l.Corte.idCorte,
                    Codigo = l.Corte.codigo,
                    Descripcion = l.Corte.corte,
                    CantKg = l.CantKg,
                    PrecioKg = l.PrecioKg,
                    Importe = (float)Math.Round((l.CantKg * l.PrecioKg), 2),
                    IdAlicuotaIva = l.IdAlicuotaIva,
                    AlicuotaIva = l.AlicuotaIva,
                    Bonificacion = l.Bonificacion,
                    Estado = l.Estado,
                    Balanza = l.PesoBalanza,
                    IndexAnulado = l.IndexAnulado,
                });
            }

            dto.ImporteTotal = facturaYaGenerada
                ? Convert.ToDecimal(factuElec.ImporteTotal)
                : (decimal)venta.LineasVenta.Sum(l => l.ImporteConIva());
            dto.ImporteNetoGravado = facturaYaGenerada
                ? Convert.ToDecimal(factuElec.ImporteNetoGravado)
                : (decimal)venta.LineasVenta.Sum(l => l.ImporteNeto());
            dto.Iva = facturaYaGenerada
                ? Convert.ToDecimal(factuElec.Iva)
                : (decimal)venta.LineasVenta.Sum(l => l.ImporteIva());

            dto.CAE = factuElec.CAE1;
            dto.FecVtoCAE = factuElec.FecVtoCAE;

            return dto;
        }

        private FacturaElectronica MapDtoToFactura(FacturaElectronicaDto dto)
        {
            return new FacturaElectronica
            {
                Id = dto.IdFactura,
                IdVenta = dto.IdVenta,
                Venta = _oVentaN.getVentaById(dto.IdVenta),
                PtoVtaAfip = dto.PtoVtaAfip,
                CodTipoCbteAfip = dto.CodTipoCbteAfip,
                DescTipoCbteAfip = dto.DescTipoCbteAfip,
                NroCbteAfip = dto.NroCbteAfip,
                FechaEmisionAfip = dto.FechaEmisionAfip,

                TipoDocAfip = dto.TipoDocAfip,
                NroDocAfip = dto.NroDocAfip,
                RazonSocialAFIP = dto.RazonSocialAFIP,
                CondicionIvaAFIP = dto.CondicionIvaAFIP,
                DomicilioAFIP = dto.DomicilioAFIP,

                CondicionVenta = dto.CondicionVenta,
                FormaPago = dto.FormaPago,
                DescItemUnitario = dto.AgruparItemUnitario ? (dto.DescItemUnitario ?? "") : "",
                Observaciones = dto.Observaciones ?? "",

                PorcentajeFacturacion = (float)dto.PorcentajeFacturacion,
                ImporteNetoGravado = (float)dto.ImporteNetoGravado,
                Iva = (float)dto.Iva,
                ImporteTotal = (float)dto.ImporteTotal,

                CAE1 = dto.CAE,
                FecVtoCAE = dto.FecVtoCAE,

                Creado = DateTime.Now,
                Error = false
            };
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

        // ===== PDF (QuestPDF, ver docs/DECISIONS.md) y email real =====

        [HttpGet]
        public IActionResult Imprimir(int id, string documento = "")
        {
            Entidades.Venta venta = _oVentaN.getVentaById(id);
            if (venta == null)
                return NotFound();

            string documentoSolicitado = (documento ?? "").Trim().ToLowerInvariant();
            byte[] pdfBytes;
            string nombreArchivo;

            switch (documentoSolicitado)
            {
                case "detalle":
                    pdfBytes = GenerarPdfDetalleVentaBytes(venta);
                    nombreArchivo = "Detalle_" + id + ".pdf";
                    break;
                case "nc":
                    var notaCredito = ObtenerNotaCreditoAsociadaVenta(venta.IdVenta);
                    pdfBytes = GenerarPdfNotaCreditoBytes(venta);
                    nombreArchivo = ConstruirNombreArchivoComprobante(venta, notaCredito, "NotaCredito_" + id + ".pdf");
                    break;
                case "factura":
                default:
                    var factura = ObtenerFacturaAsociadaVenta(venta.IdVenta);
                    pdfBytes = GenerarPdfVentaBytes(venta);
                    nombreArchivo = ConstruirNombreArchivoComprobante(venta, factura, "Factura_" + id + ".pdf");
                    break;
            }

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }

        [HttpGet]
        public IActionResult ObtenerDatosEmailComprobante(int id)
        {
            try
            {
                var venta = _oVentaN.getVentaById(id);
                if (venta == null || venta.IdVenta <= 0)
                    return Json(new { ok = false, msg = "Venta no encontrada." });

                var empresaVenta = ObtenerEmpresaVenta(venta);
                var factuElec = ObtenerFacturaAsociadaVenta(venta.IdVenta);
                var notaCredito = ObtenerNotaCreditoAsociadaVenta(venta.IdVenta);
                string nombreEmpresa = ObtenerNombreEmpresaVenta(venta);
                string emailDestino = venta.Persona != null ? (venta.Persona.Email ?? "").Trim() : "";
                bool adjuntarDetalleDisponible = factuElec != null
                    && factuElec.Id > 0
                    && (Math.Abs(factuElec.PorcentajeFacturacion - 100f) > 0.0001f
                        || !string.IsNullOrWhiteSpace(factuElec.DescItemUnitario));
                string asunto = "Comprobante de " + nombreEmpresa;
                string cuerpo =
                    "Estimado/a cliente:\n\n" +
                    "Adjuntamos la factura correspondiente.\n\n" +
                    "Este correo fue enviado automáticamente. Por favor, no responda a este mensaje.\n\n" +
                    "Atentamente,\n" +
                    nombreEmpresa;

                return Json(new
                {
                    ok = true,
                    email = emailDestino,
                    asunto,
                    mensaje = cuerpo,
                    adjuntarDetalleDisponible,
                    tieneFactura = factuElec != null && factuElec.Id > 0,
                    tieneNotaCredito = notaCredito != null && notaCredito.Id > 0,
                    facturaAgrupaItems = factuElec != null && !string.IsNullOrWhiteSpace(factuElec.DescItemUnitario),
                    empresa = nombreEmpresa,
                    replyTo = empresaVenta != null ? (empresaVenta.Email ?? "") : ""
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EnviarComprobanteEmail(int idVenta, string emailDestino, string asunto, string mensaje, bool adjuntarDetalle = false, string documento = "")
        {
            try
            {
                var venta = _oVentaN.getVentaById(idVenta);
                if (venta == null || venta.IdVenta <= 0)
                    return Json(new { ok = false, msg = "Venta no encontrada." });

                emailDestino = (emailDestino ?? "").Trim();
                asunto = (asunto ?? "").Trim();
                mensaje = (mensaje ?? "").Trim();

                if (string.IsNullOrWhiteSpace(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email destino." });

                if (!SmtpMailHelper.IsValidEmail(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email válido." });

                if (string.IsNullOrWhiteSpace(asunto))
                    return Json(new { ok = false, msg = "Ingrese un asunto." });

                var empresaVenta = ObtenerEmpresaVenta(venta);
                string nombreEmpresa = ObtenerNombreEmpresaVenta(venta);
                var factura = ObtenerFacturaAsociadaVenta(venta.IdVenta);
                var notaCredito = ObtenerNotaCreditoAsociadaVenta(venta.IdVenta);
                byte[] pdfBytes = null;
                byte[] pdfDetalleBytes = null;
                byte[] pdfNotaCreditoBytes = null;
                string bodyHtml = ConvertirTextoAHtml(mensaje);
                string nombreAdjunto = ConstruirNombreArchivoComprobante(venta, factura, "Factura_" + venta.IdVenta + ".pdf");
                string nombreAdjuntoDetalle = "Detalle_" + venta.IdVenta + ".pdf";
                string nombreAdjuntoNotaCredito = ConstruirNombreArchivoComprobante(venta, notaCredito, "NotaCredito_" + venta.IdVenta + ".pdf");
                string fromName = "CarniSys - " + nombreEmpresa;
                string replyToEmail = empresaVenta != null ? (empresaVenta.Email ?? "").Trim() : "";
                string documentoSolicitado = (documento ?? "").Trim().ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(documentoSolicitado))
                    documentoSolicitado = adjuntarDetalle ? "todos" : "factura";

                bool incluirDetalle = documentoSolicitado == "todos" || documentoSolicitado == "detalle";
                bool incluirFactura = documentoSolicitado == "todos" || documentoSolicitado == "factura";
                bool incluirNc = documentoSolicitado == "todos" || documentoSolicitado == "nc";

                if (incluirDetalle)
                    pdfDetalleBytes = GenerarPdfDetalleVentaBytes(venta);

                if (incluirFactura && factura != null && factura.Id > 0)
                    pdfBytes = GenerarPdfComprobanteBytes(venta, factura, false);

                if (incluirNc && notaCredito != null && notaCredito.Id > 0)
                    pdfNotaCreditoBytes = GenerarPdfComprobanteBytes(venta, notaCredito, true);

                if (!incluirFactura && !incluirNc && !incluirDetalle)
                    return Json(new { ok = false, msg = "Seleccione al menos un comprobante para enviar." });

                if (incluirFactura && pdfBytes == null)
                    return Json(new { ok = false, msg = "La venta no tiene factura asociada." });

                if (incluirNc && pdfNotaCreditoBytes == null)
                    return Json(new { ok = false, msg = "La venta no tiene nota de crédito asociada." });

                if (adjuntarDetalle && documentoSolicitado == "factura")
                {
                    bool puedeAdjuntarDetalle = factura != null
                        && factura.Id > 0
                        && (Math.Abs(factura.PorcentajeFacturacion - 100f) > 0.0001f
                            || !string.IsNullOrWhiteSpace(factura.DescItemUnitario));

                    if (puedeAdjuntarDetalle)
                        pdfDetalleBytes = GenerarPdfDetalleVentaBytes(venta);
                }

                SmtpMailHelper.SendMail(
                    toEmail: emailDestino,
                    toName: venta.Persona != null ? venta.Persona.RazonSocial : "",
                    subject: asunto,
                    bodyHtml: bodyHtml,
                    attachmentFileName: nombreAdjunto,
                    attachmentBytes: pdfBytes,
                    attachmentContentType: "application/pdf",
                    attachmentFileName2: pdfDetalleBytes != null ? nombreAdjuntoDetalle : null,
                    attachmentBytes2: pdfDetalleBytes,
                    attachmentContentType2: "application/pdf",
                    attachmentFileName3: pdfNotaCreditoBytes != null ? nombreAdjuntoNotaCredito : null,
                    attachmentBytes3: pdfNotaCreditoBytes,
                    attachmentContentType3: "application/pdf",
                    fromNameOverride: fromName,
                    replyToEmail: SmtpMailHelper.IsValidEmail(replyToEmail) ? replyToEmail : null,
                    replyToName: nombreEmpresa
                );

                return Json(new { ok = true, msg = "El comprobante se envió correctamente." });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo enviar el email. " + ex.Message });
            }
        }

        private static string ConstruirNombreArchivoComprobante(
            Entidades.Venta venta,
            Entidades.FacturaElectronica comprobante,
            string nombreFallback)
        {
            if (comprobante == null || comprobante.Id <= 0)
                return nombreFallback;

            string letraFactura = string.Empty;
            if (comprobante.CodTipoCbteAfip > 0)
            {
                char letraPorCodigo = comprobante.getLetraId_TipoCbte(comprobante.CodTipoCbteAfip);
                letraFactura = letraPorCodigo == '\0' ? string.Empty : letraPorCodigo.ToString().ToUpper();
            }

            if (string.IsNullOrWhiteSpace(letraFactura) && !string.IsNullOrWhiteSpace(comprobante.DescTipoCbteAfip))
            {
                string descTipoCbteAfip = comprobante.DescTipoCbteAfip.Trim();
                letraFactura = descTipoCbteAfip.Substring(descTipoCbteAfip.Length - 1).ToUpper();
            }
            string nombreClienteArchivo = (comprobante.RazonSocialAFIP ?? venta?.Persona?.razonSocial ?? string.Empty).Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                letraFactura = letraFactura.Replace(invalidChar.ToString(), string.Empty);
                nombreClienteArchivo = nombreClienteArchivo.Replace(invalidChar.ToString(), string.Empty);
            }

            if (string.IsNullOrWhiteSpace(nombreClienteArchivo))
                nombreClienteArchivo = "CLIENTE";

            nombreClienteArchivo = nombreClienteArchivo.Length > 15
                ? nombreClienteArchivo.Substring(0, 15).Trim()
                : nombreClienteArchivo;

            string fechaArchivo = (comprobante.FechaEmisionAfip ?? venta?.FechaVenta ?? DateTime.Now).ToString("yyyyMMdd");

            return fechaArchivo + "_Factura" +
                letraFactura + "_" +
                (comprobante.PtoVtaAfip ?? string.Empty) + "-" +
                (comprobante.NroCbteAfip ?? string.Empty) + "_" +
                nombreClienteArchivo + ".pdf";
        }

        private byte[] GenerarPdfVentaBytes(Entidades.Venta venta)
        {
            if (venta == null || venta.IdVenta <= 0)
                throw new InvalidOperationException("Venta no encontrada.");

            Entidades.FacturaElectronica factuElec = ObtenerFacturaAsociadaVenta(venta.IdVenta);
            if (factuElec == null || factuElec.Id <= 0)
                return GenerarPdfDetalleVentaBytes(venta);

            return GenerarPdfComprobanteBytes(venta, factuElec, false);
        }

        private byte[] GenerarPdfNotaCreditoBytes(Entidades.Venta venta)
        {
            if (venta == null || venta.IdVenta <= 0)
                throw new InvalidOperationException("Venta no encontrada.");

            var notaCredito = ObtenerNotaCreditoAsociadaVenta(venta.IdVenta);
            if (notaCredito == null || notaCredito.Id <= 0)
                throw new InvalidOperationException("La venta no tiene nota de crédito asociada.");

            return GenerarPdfComprobanteBytes(venta, notaCredito, true);
        }

        private byte[] GenerarPdfComprobanteBytes(Entidades.Venta venta, Entidades.FacturaElectronica comprobante, bool esNotaCredito)
        {
            if (venta == null || venta.IdVenta <= 0)
                throw new InvalidOperationException("Venta no encontrada.");

            if (comprobante == null || comprobante.Id <= 0)
                throw new InvalidOperationException("Comprobante no encontrado.");

            if (esNotaCredito)
            {
                var facturaAsociada = ObtenerFacturaAsociadaVenta(venta.IdVenta);
                if (facturaAsociada != null && facturaAsociada.Id > 0)
                {
                    string numeroComprobanteAsociado =
                        string.Format(
                            "{0}-{1}",
                            facturaAsociada.PtoVtaAfip ?? "",
                            facturaAsociada.NroCbteAfip ?? ""
                        ).Trim().Trim('-');

                    string nombreComprobanteAsociado = (facturaAsociada.DescTipoCbteAfip ?? "").Trim();

                    if (!string.IsNullOrWhiteSpace(numeroComprobanteAsociado) &&
                        !string.IsNullOrWhiteSpace(nombreComprobanteAsociado))
                    {
                        comprobante.ComprobanteAsociadoInfo =
                            numeroComprobanteAsociado + " " + nombreComprobanteAsociado;
                    }
                    else if (!string.IsNullOrWhiteSpace(numeroComprobanteAsociado))
                    {
                        comprobante.ComprobanteAsociadoInfo = numeroComprobanteAsociado;
                    }
                    else
                    {
                        comprobante.ComprobanteAsociadoInfo = nombreComprobanteAsociado;
                    }
                }
                else
                {
                    comprobante.ComprobanteAsociadoInfo = "";
                }
            }
            else
            {
                comprobante.ComprobanteAsociadoInfo = "";
            }

            char letraComprobante = comprobante.getLetraId_TipoCbte(comprobante.CodTipoCbteAfip);
            return WebCore.Services.GenerarDocsCore.GenerarFacturaPDF(CrearVentaDocumento(venta, letraComprobante), comprobante);
        }

        private byte[] GenerarPdfDetalleVentaBytes(Entidades.Venta venta)
        {
            if (venta == null || venta.IdVenta <= 0)
                throw new InvalidOperationException("Venta no encontrada.");

            var ventaDetalle = CrearVentaDetalleTipoX(venta);
            return WebCore.Services.GenerarDocsCore.GenerarFacturaPDF(ventaDetalle, null);
        }

        private Entidades.Venta CrearVentaDetalleTipoX(Entidades.Venta venta)
        {
            return CrearVentaDocumento(venta, 'X');
        }

        private Entidades.Venta CrearVentaDocumento(Entidades.Venta venta, char tipoComprobante)
        {
            return new Entidades.Venta
            {
                IdVenta = venta.IdVenta,
                FechaVenta = venta.FechaVenta,
                Observaciones = venta.Observaciones,
                Sucursal = venta.Sucursal,
                Persona = venta.Persona,
                NroRemito = venta.NroRemito,
                FormaPago = venta.FormaPago,
                TipoComprobante = tipoComprobante,
                Vendedor = venta.Vendedor,
                LineasVenta = venta.LineasVenta,
                TotalImporte = venta.LineasVenta != null ? venta.LineasVenta.Sum(l => l != null ? l.ImporteConIva() : 0f) : 0f,
                TotalImporteOriginal = venta.TotalImporteOriginal
            };
        }

        private Entidades.Empresa ObtenerEmpresaVenta(Entidades.Venta venta)
        {
            return (venta != null && venta.Sucursal != null ? venta.Sucursal.Empresa : null)
                ?? _usuarioActual.Empresa;
        }

        private string ObtenerNombreEmpresaVenta(Entidades.Venta venta)
        {
            var empresaVenta = ObtenerEmpresaVenta(venta);
            string nombre = empresaVenta != null
                ? (!string.IsNullOrWhiteSpace(empresaVenta.NombreFantasia) ? empresaVenta.NombreFantasia : empresaVenta.RazonSocialAfip)
                : "";
            return !string.IsNullOrWhiteSpace(nombre)
                ? nombre.Trim()
                : "CarniSys";
        }

        private string ConvertirTextoAHtml(string texto)
        {
            string safe = System.Net.WebUtility.HtmlEncode(texto ?? "");
            safe = safe.Replace("\r\n", "\n").Replace("\r", "\n");
            string cuerpoHtml = "<p>" + safe.Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
            string pieHtml =
                "<div style=\"margin-top:24px; padding-top:12px; border-top:1px solid #ddd; font-size:11px; color:#777; line-height:1.4;\">" +
                "<p>CarniSys es un software de gestión comercial para pequeños y medianos comercios, diseñado para administrar ventas, stock y facturación, con integración a balanzas para agilizar la atención en productos pesables.</p>" +
                "</div>";

            return cuerpoHtml + pieHtml;
        }
    }
}
