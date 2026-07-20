using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Datos;
using System.Globalization;
using System.IO;
using System.Text;
using Web.Helpers;
using Web.Models;
using System.Web.SessionState;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class HomeController : BaseController
    {
        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Venta oVentaN;
        private Negocio.CuentaCorriente oCuentaCorrienteN;
        private Negocio.Corte oCorteN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oUsuarioN = new Negocio.Usuario(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
            oVentaN = new Negocio.Venta(empresa, param);
            oCuentaCorrienteN = new Negocio.CuentaCorriente(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
        }

        public ActionResult Index()
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            ViewBag.PuedeVerCierreCaja = oUsuarioN.tienePermiso(user, Permisos.Caja.CierresDeCaja, DateTime.Today, -1);

            var sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.Sucursales = sucursales;

            var model = new HomeDashboardIndexVm
            {
                PuedeVerDashboardDatos = user != null && user.Admin,
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

                return Json(new { ok = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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
                var dt = oCorteN.TotalPorCortesVendidos("", sucursalReporte, filtro.FechaDesde, filtro.FechaHasta, "", 0, 0)
                    ?? new System.Data.DataTable();

                var items = new List<DashboardTopProductoVm>();
                foreach (System.Data.DataRow row in dt.Rows)
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

                return Json(new { ok = true, data = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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

                return Json(new { ok = true, data = items, alcance = "global" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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

                return Json(new { ok = true, data = items, alcance = "global" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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

                return Json(new { ok = true, data = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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
                var dt = oCorteN.obtenerUltimosElaboradosDashboard(5, filtro.IdSucursalConsulta, filtro.FechaDesde, filtro.FechaHasta)
                    ?? new System.Data.DataTable();

                var resultado = new List<Tuple<DateTime, DashboardUltimoElaboradoVm>>();
                foreach (System.Data.DataRow row in dt.Rows)
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

                return Json(new { ok = true, data = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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
                return Json(new { ok = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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
                return Json(new { ok = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Utilidades()
        {
            var model = new UtilitiesIndexVm
            {
                Agentes = new List<UtilityItemVm>
                {
                    BuildUtilityItem(
                        "Agente de balanza",
                        "Agente local",
                        "Lee la balanza desde la PC local y expone la API en 127.0.0.1 para POS y otras pantallas web.",
                        "~/Content/downloads/Carnisys.Balanza.Agent.zip",
                        "Carnisys.Balanza.Agent.zip",
                        "Incluye ejecutable, configuración inicial y script de instalación local."),
                    BuildUtilityItem(
                        "Agente de impresión",
                        "Agente local",
                        "Permite imprimir tickets desde la terminal local sin depender del servidor web.",
                        "~/Content/downloads/CarniSys.PrintAgent.zip",
                        "CarniSys.PrintAgent.zip",
                        "Instalar en la terminal donde está conectada la impresora térmica.")
                },
                OtrasUtilidades = new List<UtilityItemVm>
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

            return View(model);
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ImprimirCalculadoraBilletesPayload(CalculadoraBilletesPrintVm request)
        {
            try
            {
                if (request == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos para imprimir." });

                int ticketMm = request.TicketMm == 58 ? 58 : 80;
                return Json(new
                {
                    ok = true,
                    ticketMm = ticketMm,
                    ticketLines = ConstruirLineasCalculadoraBilletes(request, ticketMm)
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DescargarCalculadoraBilletesPdf(CalculadoraBilletesPrintVm request)
        {
            try
            {
                if (request == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos para generar el PDF." });

                byte[] bytes = GenerarPdfCalculadoraBilletes(request);
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

        [HttpGet]
        public ActionResult DescargarAgenteImpresion()
        {
            string path = Server.MapPath("~/Content/downloads/CarniSys.PrintAgent.zip");
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            return File(path, "application/zip", "CarniSys.PrintAgent.zip");
        }

        private UtilityItemVm BuildUtilityItem(string nombre, string categoria, string descripcion, string virtualPath, string archivoNombre, string notaInstalacion)
        {
            string physicalPath = Server.MapPath(virtualPath);
            bool disponible = System.IO.File.Exists(physicalPath);
            var info = disponible ? new FileInfo(physicalPath) : null;

            return new UtilityItemVm
            {
                Nombre = nombre,
                Categoria = categoria,
                Descripcion = descripcion,
                Estado = disponible ? "Disponible" : "No disponible",
                Version = info != null ? info.LastWriteTime.ToString("dd/MM/yyyy HH:mm") : "-",
                ArchivoUrl = disponible ? Url.Content(virtualPath) : string.Empty,
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

        private HomeDashboardFiltro CrearFiltroDashboard(string periodo, int? idSucursal)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
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

            int sucursalSeleccionada = idSucursal ?? (user != null && user.IdSucursal > 0 ? user.IdSucursal : 0);
            string sucursalEtiqueta = "Todas las sucursales";
            if (sucursalSeleccionada > 0)
            {
                var sucursal = oSucursalN.findById(sucursalSeleccionada);
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
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesion vencida." }, JsonRequestBehavior.AllowGet);

            if (!user.Admin)
                return Json(new { ok = false, mensaje = "El dashboard de datos esta disponible solo para administradores." }, JsonRequestBehavior.AllowGet);

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
            var ventas = oVentaN.getAllVentas(
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
            var dt = oCuentaCorrienteN.obtenerCtasCtes("", null, "DESC") ?? new System.Data.DataTable();
            var items = new List<DashboardSaldoPersonaVm>();

            foreach (System.Data.DataRow row in dt.Rows)
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

            var dtResumen = oCuentaCorrienteN.obtenerResumenDashboard() ?? new System.Data.DataTable();
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

            var dtMovimientos = oCuentaCorrienteN.obtenerUltimosPagosDashboard(5) ?? new System.Data.DataTable();
            foreach (System.Data.DataRow row in dtMovimientos.Rows)
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
            var dtCheques = oCuentaCorrienteN.obtenerChequesPendientesDashboard(10, hoy) ?? new System.Data.DataTable();
            foreach (System.Data.DataRow row in dtCheques.Rows)
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
            var dtMovimientos = oCorteN.obtenerUltimosMovimientosDashboard(cantidad)
                ?? new System.Data.DataTable();

            foreach (System.Data.DataRow row in dtMovimientos.Rows)
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

        private static string LeerString(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas)
            {
                if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                    return Convert.ToString(row[columna]) ?? "";
            }

            return "";
        }

        private static int LeerInt(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas)
            {
                if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                {
                    int valor;
                    if (int.TryParse(Convert.ToString(row[columna]), out valor))
                        return valor;
                }
            }

            return 0;
        }

        private static long LeerLong(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas)
            {
                if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                {
                    long valor;
                    if (long.TryParse(Convert.ToString(row[columna]), out valor))
                        return valor;
                }
            }

            return 0;
        }

        private static decimal LeerDecimalPrimeraCoincidencia(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas)
            {
                if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                    continue;

                decimal valor;
                if (decimal.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.CurrentCulture, out valor))
                    return valor;

                if (decimal.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                    return valor;
            }

            return 0m;
        }

        private static DateTime LeerDate(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas)
            {
                if (row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                {
                    DateTime valor;
                    if (DateTime.TryParse(Convert.ToString(row[columna]), out valor))
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

        private List<string> ConstruirLineasCalculadoraBilletes(CalculadoraBilletesPrintVm request, int ticketMm)
        {
            int cantMaxChar = ticketMm == 58 ? 32 : 43;
            string titulo = string.IsNullOrWhiteSpace(request.Titulo) ? "Detalle billetes" : request.Titulo.Trim();

            var user = Session["Usuario"] as Entidades.Usuario;
            string empresaNombre = user != null && user.Empresa != null
                ? (user.Empresa.NombreFantasia ?? user.Empresa.RazonSocialAfip ?? "CarniSys")
                : "CarniSys";

            System.Func<string, int, string> truncar = (texto, maximo) =>
            {
                texto = texto ?? "";
                return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
            };

            System.Func<string, int, string> centrar = (texto, ancho) =>
            {
                texto = truncar(texto, ancho);
                int espaciosIzquierda = (ancho - texto.Length) / 2;
                if (espaciosIzquierda < 0) espaciosIzquierda = 0;
                return new string(' ', espaciosIzquierda) + texto;
            };

            var lineas = new List<string>();
            lineas.Add(centrar(titulo, cantMaxChar));
            lineas.Add(centrar(empresaNombre, cantMaxChar));
            lineas.Add(truncar("Fecha: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), cantMaxChar));
            lineas.Add(new string('-', cantMaxChar));
            lineas.Add("Total $: " + request.Total.ToString("N2"));
            lineas.Add("\u00A0");
            lineas.Add("\u00A0");
            lineas.Add("Detalles:");
            lineas.Add(NormalizarDetalleCalculadoraBilletes(request));
            lineas.Add("\u00A0");
            lineas.Add("\u00A0");
            lineas.Add("\u00A0");
            lineas.Add(".");
            lineas.Add("\u00A0");

            return lineas;
        }

        private byte[] GenerarPdfCalculadoraBilletes(CalculadoraBilletesPrintVm request)
        {
            using (var ms = new MemoryStream())
            {
                using (var document = new Document(PageSize.A4, 36f, 36f, 36f, 36f))
                {
                    PdfWriter.GetInstance(document, ms);
                    document.Open();

                    var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16f);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11f);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);

                    string titulo = string.IsNullOrWhiteSpace(request.Titulo) ? "Detalle de billetes" : request.Titulo.Trim();
                    document.Add(new Paragraph(titulo, tituloFont));
                    document.Add(new Paragraph(System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), normalFont));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("Total $: " + request.Total.ToString("N2"), boldFont));
                    document.Add(new Paragraph("Detalles:", boldFont));
                    document.Add(new Paragraph(NormalizarDetalleCalculadoraBilletes(request), normalFont));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));

                    document.Close();
                }

                return ms.ToArray();
            }
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
}
