using Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;
using Web.Models.DTO;
using AFIP;
using static Entidades.Venta;
using System.IO;
using System.Data;

namespace Web.Controllers
{
    public class VentasController : BaseController
    {
        private Negocio.Venta oVentaN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Persona oPersonaN;
        private Negocio.CierreCaja oCierreN;
        private Negocio.Corte oCorteN;

        private Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
        private Entidades.Venta.imprimirCbteEnum imprimirCbte =
            Entidades.Venta.imprimirCbteEnum.Nulo;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oVentaN = new Negocio.Venta(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
            oUsuarioN = new Negocio.Usuario(empresa, param);
            oPersonaN = new Negocio.Persona(empresa, param);
            oCierreN = new Negocio.CierreCaja(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
        }

        public ActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1)
        {
            // Si no envían fechas, por defecto usar hoy
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.VerVentas, desde))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Venta.VerVentas, ref desde) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Ventas", Permisos.Venta.VerVentas, desde);
            }


            //si ambas fechas son iguales se suma 24 horas a fechaHasta
            if (desde == hasta && desde.Hour == 0)
            {
                hasta = hasta.AddDays(1);
            }


            var sucursales = oSucursalN.findAll(); // Obtiene List<Entidades.Sucursal>

            ViewBag.Sucursales = sucursales;
            ViewBag.IdSucursalSeleccionada = idSucursal;
            ConfigurarAdvertenciaFechaEnVivo("fechaDesde", Permisos.Venta.VerVentas);

            // 1️⃣ Enum → lista (sin Nulo)
            var formasPago = Enum.GetValues(typeof(formaPagoEnum))
                                 .Cast<formaPagoEnum>()
                                 .Where(f => f != formaPagoEnum.Nulo)
                                 .ToList();

            // 2️⃣ Todas seleccionadas por defecto
            var seleccionadas = formasPago;

            // 3️⃣ Armar MultiSelectList
            ViewBag.FormasPago = new MultiSelectList(
                formasPago.Select(f => new
                {
                    Value = f.ToString().ToLower(), // para usar en data-forma-pago
                    Text = f.ToString()
                }),
                "Value",
                "Text",
                seleccionadas.Select(f => f.ToString().ToLower())
            );

            List<Entidades.Venta> ventas = oVentaN.getAllVentas(desde, hasta, "", -1, -1, idSucursal, false, false); //new List<Entidades.Venta>();

            ViewBag.TotalFiltrado = ventas.Sum(v => v.TotalImporte);
            //ventas.Add(oVentaE);
            return View(ventas);
        }

        public ActionResult Lineas(DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1, string cliente = "", string vendedor = "", string formasPago = "", string producto = "")
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.VerVentas, desde))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Venta.VerVentas, ref desde) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Ventas", Permisos.Venta.VerVentas, desde);
            }

            if (hasta < desde)
                hasta = desde;

            var sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var formasPagoSeleccionadas = SepararValoresCsv(formasPago);

            List<Entidades.Venta> ventas = oVentaN.getAllVentas(desde, hasta, "", -1, -1, idSucursal, false, true) ?? new List<Entidades.Venta>();
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

                if (!string.IsNullOrWhiteSpace(producto) && lineas.Count == 0)
                    continue;

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
            ViewBag.Seccion = "Ventas";
            ViewBag.Sucursales = sucursales;

            return View("~/Views/Ventas/Lineas.cshtml", model);
        }

        public ActionResult MisVentas(bool desdePos = false, int idCierre = 0)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new HttpStatusCodeResult(401, "Sesión inválida");

            var cierre = ObtenerCierreMisVentas(user, desdePos, idCierre);
            if (cierre == null)
            {
                ViewBag.Mensaje = "No hay una caja abierta para consultar ventas.";
                ViewBag.DesdePOS = desdePos;
                ViewBag.IdCierreActividad = idCierre;
                return PartialView("~/Views/Ventas/_MisVentas.cshtml", new List<Entidades.Venta>());
            }

            var ventas = ConvertirVentasResumen(oVentaN.getVentasVendedorCierreCaja(cierre, false));

            ViewBag.DesdePOS = desdePos;
            ViewBag.IdCierreActividad = cierre.Id;
            ViewBag.CierreCaja = cierre;
            ViewBag.TotalVisible = ventas.Sum(v => v.TotalImporte);
            ViewBag.TituloVentas = "Mis ventas";
            ViewBag.SubtituloVentas = cierre.UsuarioInicio != null && cierre.Sucursal != null
                ? cierre.UsuarioInicio.Nombre + " | " + cierre.Sucursal.sucursal
                : "";

            return PartialView("~/Views/Ventas/_MisVentas.cshtml", ventas);
        }

        // GET: Ventas/DetalleVenta/5
        public ActionResult DetalleVenta(int id, bool modal = false, bool desdePos = false, int idCierre = 0, string returnUrl = "")
        {
            // Buscar la venta por ID
            var venta = oVentaN.getVentaById(id);
            //_context.Ventas
            //    .Include("Persona")
            //    .Include("Vendedor")
            //    .Include("Lineas.Producto")
            //    .FirstOrDefault(v => v.IdVenta == id);

            if (venta == null)
            {
                return HttpNotFound();
            }

            ViewBag.ModoModal = modal;
            ViewBag.DesdePOS = desdePos;
            ViewBag.IdCierreActividad = idCierre;
            ViewBag.ReturnUrlDetalle = DecodeReturnUrlIfNeeded(returnUrl);
            ViewBag.PuedeModificarVenta = PuedeModificarUltimaVenta(venta);
            ViewBag.MotivoNoPuedeModificarVenta = ViewBag.PuedeModificarVenta ? "" : ObtenerMotivoNoPuedeModificarUltimaVenta(venta);
            ViewBag.PuedeCambiarFormaPago = PuedeCambiarFormaPago(venta);
            ViewBag.MotivoNoPuedeCambiarFormaPago = ViewBag.PuedeCambiarFormaPago ? "" : ObtenerMotivoNoPuedeCambiarFormaPago(venta);
            ViewBag.TieneFacturaVenta = oVentaN.existeFactuElectParaVenta(venta.IdVenta) > 0;

            // Pasar la venta a la vista
            if (modal)
                return PartialView(venta);

            return View(venta);
        }

        [HttpPost]

        public JsonResult FinalizarVenta(FinalizarVentaRequest request)
        {
            try
            {
                //probarloginafip();
                //return Json(new { ok = true, msg = "Login AFIP exitoso" }, JsonRequestBehavior.AllowGet);

                var venta = Session["VentaActiva"] as Venta;
                var user = Session["Usuario"] as Entidades.Usuario;

                if (user == null)
                    return Json(new { ok = false, msg = "Sesión expirada" });

                if (venta == null)
                    return Json(new { ok = false, msg = "No hay venta activa" });

                if (request.LineasVenta == null || !request.LineasVenta.Any())
                    return Json(new { ok = false, msg = "No hay productos en la venta" });

                if (user.IdSucursal == 0)
                    return Json(new { ok = false, msg = "Seleccione una sucursal antes de finalizar la venta." });

                if (request.IdSucursalPOS != user.IdSucursal)
                {
                    var sucursalPos = oSucursalN.findById(request.IdSucursalPOS);
                    string nombreSucursalPos = sucursalPos != null && !string.IsNullOrWhiteSpace(sucursalPos.SucursalNombre)
                        ? sucursalPos.SucursalNombre
                        : "original del POS";

                    return Json(new
                    {
                        ok = false,
                        msg = "La venta fue iniciada en la sucursal " + nombreSucursalPos +
                              ". Vuelva a la pantalla principal, cambie a esa sucursal y luego finalice la venta."
                    });
                }

                venta.Persona = oPersonaN.findById(request.IdPersona);
                if (venta.Persona == null)
                    return Json(new { ok = false, msg = "El Cliente no existe." });

                venta.Sucursal = oSucursalN.findById(user.IdSucursal);
                if (venta.Sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida." });

                venta.TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString());

                venta.Observaciones = request.Observaciones ?? venta.Observaciones ?? "";

                // ===============================
                // FORMAS DE PAGO
                // ===============================
                venta.FormaPago = request.FormaPago;
                venta.EnCtaCte = request.FormaPago == formaPagoEnum.CtaCte.ToString();
                venta.PagoMixtoEfectivo = request.EsPagoMixto ? request.Efectivo : 0;

                //VALIDAR VENTA EN CTACTE sea solo en Cta Cte Y NO A 
                if (venta.EnCtaCte && (!venta.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString()) ||
                    venta.Persona.idPersona.Equals(param.GetInt(ParamKeys.IdConsumidorFinal, 0))))
                {
                    string msg_ = "Las ventas en Cuenta Corriente (CTA.CTE.) no pueden ser a Consumidor Final" +
                        "\n\nPor favor, revisa los datos ingresados y vuelva a intentarlo.";
                    return Json(new { ok = false, msg = msg_ });
                }

                //VALIDAR CAJA ABIERTA
                bool cajaAbierta = oCierreN.validarCajaAbiertaVendedor(DateTime.Now, venta.Sucursal, user);
                if (!cajaAbierta)
                {
                    string msg_ = "La caja ha sido cerrada.";

                    return Json(new { ok = false, msg = msg_ });
                }

                List<Entidades.LineaVenta> lineasVenta = ConstruirLineasVentaDesdeRequest(request);
                CompletarAnulacionesVenta(lineasVenta);

                // ===============================
                // LINEAS DE VENTA (CLAVE)
                // ===============================
                venta.LineasVenta = lineasVenta;
                venta.ListaExpendios = (request.ListaExpendios ?? new List<int>())
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                int idVenta = oVentaN.agregarVenta(venta);

                Session.Remove("VentaActiva");

                return Json(new { ok = true, ventaId = idVenta }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;

                return Json(new
                {
                    ok = false,
                    msg = "Error al finalizar la venta",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ModificarVenta(FinalizarVentaRequest request)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                bool soloFormaPago = request != null && request.SoloFormaPago;

                if (user == null)
                    return Json(new { ok = false, msg = "Sesión expirada" });

                if (request == null || request.IdVenta <= 0)
                    return Json(new { ok = false, msg = "Venta inválida" });

                var venta = oVentaN.getVentaById(request.IdVenta);
                if (venta == null)
                    return Json(new { ok = false, msg = "La venta no existe." });

                var cierreActual = ObtenerCierreCajaActual(user);
                bool tienePermisoAdministrativoVenta = TienePermisoAdministrativoSobreVenta(venta, user);

                if (soloFormaPago)
                {
                    if (!PuedeCambiarFormaPago(venta, user, cierreActual))
                        return Json(new { ok = false, msg = ObtenerMotivoNoPuedeCambiarFormaPago(venta, user, cierreActual) });
                }
                else
                {
                    if (!PuedeModificarUltimaVenta(venta, user, cierreActual))
                        return Json(new { ok = false, msg = "No tiene permisos para modificar esta venta." });
                }

                if (request.LineasVenta == null || !request.LineasVenta.Any())
                    return Json(new { ok = false, msg = "No hay productos en la venta" });

                if (user.IdSucursal == 0 && !tienePermisoAdministrativoVenta)
                    return Json(new { ok = false, msg = "Seleccione una sucursal antes de guardar la venta." });

                if (!tienePermisoAdministrativoVenta &&
                    (request.IdSucursalPOS != user.IdSucursal || venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal))
                    return Json(new { ok = false, msg = "La venta pertenece a otra sucursal. Cambie a la sucursal correcta antes de modificarla." });

                venta.Persona = oPersonaN.findById(request.IdPersona);
                if (venta.Persona == null)
                    return Json(new { ok = false, msg = "El Cliente no existe." });

                int idSucursalDestino = venta.Sucursal != null && venta.Sucursal.idSucursal > 0
                    ? venta.Sucursal.idSucursal
                    : user.IdSucursal;

                venta.Sucursal = oSucursalN.findById(idSucursalDestino);
                if (venta.Sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida." });

                if (!tienePermisoAdministrativoVenta && (cierreActual == null || cierreActual.UsuarioInicio == null))
                    return Json(new { ok = false, msg = "La caja ha sido cerrada." });

                bool cajaAbierta = tienePermisoAdministrativoVenta ||
                    oCierreN.validarCajaAbiertaVendedor(venta.FechaVenta, venta.Sucursal, cierreActual.UsuarioInicio);

                if (!cajaAbierta)
                    return Json(new { ok = false, msg = "La caja ha sido cerrada." });

                venta.Observaciones = request.Observaciones ?? venta.Observaciones ?? "";
                venta.FormaPago = request.FormaPago;
                venta.EnCtaCte = request.FormaPago == formaPagoEnum.CtaCte.ToString();
                venta.PagoMixtoEfectivo = request.EsPagoMixto ? request.Efectivo : 0;

                if (!soloFormaPago && request.FechaVenta.HasValue && request.FechaVenta.Value != venta.FechaVenta)
                {
                    if (!PuedeEditarFechaVenta(venta, request.FechaVenta.Value))
                        return Json(new { ok = false, msg = "No tiene permisos para modificar la venta con la fecha seleccionada." });

                    venta.FechaVenta = request.FechaVenta.Value;
                }

                if (venta.EnCtaCte && (!venta.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString()) ||
                    venta.Persona.idPersona.Equals(param.GetInt(ParamKeys.IdConsumidorFinal, 0))))
                {
                    string msg_ = "Las ventas en Cuenta Corriente (CTA.CTE.) no pueden ser a Consumidor Final" +
                        "\n\nPor favor, revisa los datos ingresados y vuelva a intentarlo.";
                    return Json(new { ok = false, msg = msg_ });
                }

                venta.LineasVenta = ConstruirLineasVentaDesdeRequest(request);
                venta.ListaExpendios = (request.ListaExpendios ?? new List<int>())
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();
                CompletarAnulacionesVenta(venta.LineasVenta);

                oVentaN.modificarVenta(venta, venta.Sucursal.idSucursal, !soloFormaPago, null);

                Session.Remove("VentaActiva");

                return Json(new { ok = true, ventaId = venta.IdVenta }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;

                return Json(new
                {
                    ok = false,
                    msg = "Error al modificar la venta",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #region POS
        public ActionResult POS(int idVentaEditar = 0, bool soloFormaPago = false, string returnUrl = "", int abrirDetalleVentaId = 0, string returnUrlDetalle = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (user.IdSucursal == 0)
            {
                TempData["AlertType"] = "info"; // success | info | warning | error
                TempData["AlertTitle"] = "Sucursal no seleccionada";
                TempData["AlertMsg"] = "Seleccione una sucursal desde el icono de usuario (arriba a la derecha) y vuelve a entrar al Punto de Venta.";

                return RedirectToAction("Index", "Home");
            }

            user.Sucursal = oSucursalN.findById(user.IdSucursal);
            if (user.Sucursal == null)
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Sucursal inválida";
                TempData["AlertMsg"] = "Seleccione una sucursal válida desde el icono de usuario (arriba a la derecha) y vuelve a entrar al Punto de Venta.";

                return RedirectToAction("Index", "Home");
            }

            user.SucursalNombre = user.Sucursal.SucursalNombre;
            Session["Usuario"] = user;

            // Inicializo cierre
            var cierre = new Entidades.CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            // Busco último cierre
            cierre = oCierreN.findByIdOrLast(
                cierre,
                Entidades.CierreCaja.tipoBusqueda.FindLast,
                ""
            );

            // ¿Hay caja abierta?
            bool cajaAbierta = cierre != null &&
                               (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);

            // Paso info a la vista
            ViewBag.CajaAbierta = cajaAbierta;
            ViewBag.SucursalNombre = user.SucursalNombre;
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.IdSucursalPOS = user.IdSucursal;
            ViewBag.SoloFormaPago = soloFormaPago;
            ViewBag.ReturnUrlPOS = DecodeReturnUrlIfNeeded(returnUrl);
            ViewBag.AbrirDetalleVentaId = abrirDetalleVentaId;
            ViewBag.ReturnUrlDetalle = DecodeReturnUrlIfNeeded(returnUrlDetalle);

            // 🚨 Si NO hay caja abierta, NO inicializo venta
            if (!cajaAbierta)
            {
                return View((Venta)null);  // La vista muestra modal y POS bloqueado
            }

            // ==========================
            // POS habilitado
            // ==========================
            bool esEdicionVenta = idVentaEditar > 0;
            var venta = esEdicionVenta ? oVentaN.getVentaById(idVentaEditar) : Session["VentaActiva"] as Venta;

            if (esEdicionVenta)
            {
                if (venta == null)
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Venta inexistente";
                    TempData["AlertMsg"] = "No se encontró la venta seleccionada para modificar.";
                    return RedirectToAction("POS");
                }

                bool puedeEditarVenta = !soloFormaPago && PuedeModificarUltimaVenta(venta, user, cierre);
                bool puedeCambiarPago = soloFormaPago && PuedeCambiarFormaPago(venta, user, cierre);

                if (!puedeEditarVenta && !puedeCambiarPago)
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = soloFormaPago ? "Venta fuera de caja" : "Sin permisos";
                    TempData["AlertMsg"] = soloFormaPago
                        ? ObtenerMotivoNoPuedeCambiarFormaPago(venta, user, cierre)
                        : "No tiene permisos para modificar esta venta.";
                    return RedirectToAction("POS");
                }

                if (venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal)
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Sucursal incorrecta";
                    TempData["AlertMsg"] = "Cambie a la sucursal de la venta antes de modificarla.";
                    return RedirectToAction("POS");
                }
            }
            else if (venta == null)
            {
                venta = new Venta
                {
                    LineasVenta = new List<LineaVenta>()
                };
            }

            var oCliente = oPersonaN.getConsumidorFinal();
            ViewBag.IdConsumidorFinal = oCliente.idPersona;

            if (venta.Persona == null)
            {
                venta.Persona = oCliente;
                venta.IdPersona = oCliente.idPersona;
            }

            venta.Vendedor = user;
            if (venta.FechaVenta == DateTime.MinValue)
                venta.FechaVenta = DateTime.Now;

            ViewBag.EsEdicionVenta = esEdicionVenta;
            ViewBag.IdVentaEditar = idVentaEditar;
            ViewBag.PuedeEditarFechaVenta = esEdicionVenta && PuedeModificarUltimaVenta(venta, user, cierre) && PuedeEditarFechaVenta(venta);

            Session["VentaActiva"] = venta;

            return View(venta);
        }

        [HttpGet]
        public JsonResult BuscarExpendiosPOS(int ultimosMinutos = 300, string estado = "Pendientes", string texto = "", string idsActuales = "")
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null || user.IdSucursal == 0)
                    return Json(new { ok = false, msg = "Sesión inválida o sucursal no seleccionada." }, JsonRequestBehavior.AllowGet);

                DataTable dt = oVentaN.obtenerUltimosExpendios(ultimosMinutos, user.IdSucursal);
                List<int> idsEnVentaActual = ParseIdsExpendio(idsActuales);
                string estadoNormalizado = (estado ?? "Pendientes").Trim().ToUpperInvariant();
                string textoNormalizado = (texto ?? "").Trim();
                int nroExpendio;
                bool buscarPorNumero = int.TryParse(textoNormalizado, out nroExpendio);

                var filas = dt.AsEnumerable()
                    .Where(row =>
                    {
                        int idExpendio = ToInt(row["idExpendio"]);
                        int idVenta = ToInt(row["idVenta"]);
                        bool estaAsignadoDb = idVenta > 0 && idVenta != idExpendio;
                        bool estaEnVentaActual = idsEnVentaActual.Contains(idExpendio);

                        switch (estadoNormalizado)
                        {
                            case "ASIGNADOS":
                                if (!estaAsignadoDb && !estaEnVentaActual) return false;
                                break;
                            case "TODOS":
                                break;
                            default:
                                if (estaAsignadoDb || estaEnVentaActual) return false;
                                break;
                        }

                        if (string.IsNullOrWhiteSpace(textoNormalizado))
                            return true;

                        string identificacion = ToStr(row["identificacionExpendio"]);
                        if (buscarPorNumero && idExpendio == nroExpendio)
                            return true;

                        return identificacion.IndexOf(textoNormalizado, StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .OrderBy(row => ToDate(row["fechaExpendio"]))
                    .ThenBy(row => ToInt(row["idExpendio"]))
                    .Select(row =>
                    {
                        int idExpendio = ToInt(row["idExpendio"]);
                        int idVenta = ToInt(row["idVenta"]);
                        DateTime fechaExpendio = ToDate(row["fechaExpendio"]);

                        return new
                        {
                            fechaExpendio = fechaExpendio.ToString("yyyy-MM-ddTHH:mm:ss"),
                            hora = fechaExpendio.ToString("HH:mm"),
                            idExpendio = idExpendio,
                            identificacionExpendio = ToStr(row["identificacionExpendio"]),
                            sector = ToStr(row["sector"]),
                            codigo = ToInt(row["codigo"]),
                            producto = ToStr(row["corte"]),
                            cantKg = ToDecimal(row["cantKg"]),
                            precioKg = ToDecimal(row["precioKg"]),
                            total = ToDecimal(row["total"]),
                            vendedor = ToStr(row["vendedor"]),
                            idVenta = idVenta,
                            asignado = idVenta > 0 && idVenta != idExpendio,
                            cargadoEnVentaActual = idsEnVentaActual.Contains(idExpendio)
                        };
                    })
                    .ToList();

                return Json(new
                {
                    ok = true,
                    items = filas,
                    vacio = filas.Count == 0,
                    debug = new
                    {
                        idSucursal = user.IdSucursal,
                        totalSql = dt.Rows.Count,
                        totalFiltrado = filas.Count,
                        estado = estadoNormalizado,
                        texto = textoNormalizado
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    msg = "Error al consultar expendios: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerExpendioPOS(int idExpendio)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null || user.IdSucursal == 0)
                return Json(new { ok = false, msg = "Sesión inválida o sucursal no seleccionada." }, JsonRequestBehavior.AllowGet);

            if (idExpendio <= 0)
                return Json(new { ok = false, msg = "Expendio inválido." }, JsonRequestBehavior.AllowGet);

            var expendio = oVentaN.getExpedioById(idExpendio);
            if (expendio == null || expendio.IdExpendio <= 0)
                return Json(new { ok = false, msg = "El expendio no existe." }, JsonRequestBehavior.AllowGet);

            if (expendio.Sucursal == null || expendio.Sucursal.idSucursal != user.IdSucursal)
                return Json(new { ok = false, msg = "El expendio pertenece a otra sucursal." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ok = true,
                expendio = new
                {
                    idExpendio = expendio.IdExpendio,
                    fechaExpendio = expendio.FechaVenta.ToString("yyyy-MM-ddTHH:mm:ss"),
                    identificacionExpendio = expendio.IdentificacionExpendio ?? "",
                    sector = expendio.Sector ?? "",
                    vendedor = expendio.Vendedor != null ? expendio.Vendedor.Nombre : "",
                    idVenta = expendio.IdVenta,
                    asignado = expendio.IdVenta > 0 && expendio.IdVenta != expendio.IdExpendio
                },
                lineas = (expendio.LineasVenta ?? new List<Entidades.LineaVenta>()).Select(l => new
                {
                    idExpendio = expendio.IdExpendio,
                    codigo = l.Corte != null ? l.Corte.codigo : 0,
                    producto = l.Corte != null ? l.Corte.corte : "",
                    cantKg = l.CantKg,
                    precioKg = l.PrecioKg,
                    bonificacion = l.Bonificacion,
                    balanza = l.PesoBalanza
                }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }



        // ======================================================
        // GET /Ventas/BuscarProducto?codigo=123
        // ======================================================
        public JsonResult BuscarProducto(string codigo, bool ingresoCantidadX)
        {
            try
            {
               if (string.IsNullOrWhiteSpace(codigo))
                    return Json(new { error = "Código vacío" }, JsonRequestBehavior.AllowGet);

                codigo = codigo.Replace(",", ".");

                int cantidadPuntos = codigo.Split('.').Length - 1;

                if (cantidadPuntos > 1)
                {
                    return Json(new { error = "Formato de código inválido" }, JsonRequestBehavior.AllowGet);
                }
                //validar la cantidad de G
                var match = Regex.Match(codigo, @"^[^G]*G(\d+)[^G]*$");
                long numeroSumaGen = match.Success ? int.Parse(match.Groups[1].Value) :0;

                //Para validar que sea precio siempre q contenga '.' y se suponga q no es un codigo de barras
                int cantMinDig_EAN8 = 8;
                bool esGenerico = ingresoCantidadX && (codigo.Contains(".") || codigo.Contains("G") || codigo.Length < cantMinDig_EAN8);

                long codigoProducto = esGenerico
                    ? param.GetLong(ParamKeys.CodProdGenerico, 0L) + numeroSumaGen
                    : Convert.ToInt64(codigo);

                var gestorCortes = oCorteN;
                var corte = gestorCortes.findCorteByCodigo(codigoProducto, false);

                if (corte == null)
                {
                    string mensaje = esGenerico ? ("No existe  el código genérico") : "Código inexistente"; 
                    return Json(new { success = false, message = mensaje }, JsonRequestBehavior.AllowGet);
                }

                if (esGenerico)
                {
                    int indexG = codigo.IndexOf('G');

                    if (indexG != -1)
                        codigo = codigo.Substring(0, indexG);

                    corte.PrecioKg = float.Parse(codigo, CultureInfo.InvariantCulture);
                }

                return Json(new
                {
                    id = corte.IdCorte,
                    nombre = corte.CorteDesc,
                    precioKg = Math.Round((double)corte.PrecioKg, 2),
                    precioOriginal = Math.Round((double)corte.PrecioKg, 2),
                    codigo = corte.codigo,
                    pesable = corte.Pesable
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        // ======================================================
        // POST /Ventas/AgregarProducto    (AJAX)
        // ======================================================
        [HttpPost]
        public JsonResult AgregarProducto(int idCorte, float cantidadKg)
        {
            try
            {
                // Recuperar o crear venta activa desde la Session
                var venta = Session["VentaActiva"] as Venta;

                if (venta == null)
                {
                    venta = new Venta
                    {
                        //Fecha = DateTime.Now,
                        LineasVenta = new System.Collections.Generic.List<LineaVenta>()
                    };

                    Session["VentaActiva"] = venta;
                }

                // Obtener el producto
                var gestorCortes = oCorteN;
                var corte = gestorCortes.findCorteById(idCorte, false);

                if (corte == null)
                    return Json(new { error = "Producto no encontrado por ID" });

                // Crear la línea
                var linea = new LineaVenta
                {
                    Corte = corte,
                    PrecioKg = corte.PrecioKg,
                    CantKg = cantidadKg
                };

                venta.LineasVenta.Add(linea);

                // Respuesta con la venta actualizada
                return Json(new
                {
                    ok = true,
                    total = venta.LineasVenta.Sum(x => x.CantKg * x.PrecioKg),
                    lineas = venta.LineasVenta.Select((x, i) => new
                    {
                        index = i + 1,
                        producto = x.Corte.CorteDesc,
                        codigo = x.Corte.codigo,
                        cant = x.CantKg.ToString("0.###"),
                        precio = x.PrecioKg.ToString("C"),
                        subtotal = (x.CantKg * x.PrecioKg).ToString("C")
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        #endregion

        #region IMPRIMIR
        [HttpGet]
        public ActionResult ImprimirTicket(int id, int mm)
        {
            try
            {
                var venta = oVentaN.getVentaById(id);// _ventaService.ObtenerVentaCompleta(id);
                if (venta == null)
                    return Json(new { ok = false, msg = "Venta no encontrada" }, JsonRequestBehavior.AllowGet);

                
                if (mm == 0)
                {
                    int idFactuElec = oVentaN.esVentaSinFacturar(venta.IdVenta, false);
                    var factuElec = idFactuElec > 0 ? oVentaN.getFactuElecById(idFactuElec) : new Entidades.FacturaElectronica();

                    var dto = BuildFacturaDTO(
                                        venta,
                                        factuElec
                                    );
                    return PartialView("~/Views/Ventas/_FacturaElectronica.cshtml", dto);
                }

                imprimirCbte = Entidades.Venta.imprimirCbteEnum.Ticket;
                // Genera el ticket (ESC/POS, PDF, etc)

                ViewBag.Medida = mm == 58 ? 58 : 80;
                int idFacturaElectronica = oVentaN.existeFactuElectParaVenta(venta.IdVenta);
                var facturaTicket = idFacturaElectronica > 0
                    ? oVentaN.getFactuElecById(idFacturaElectronica)
                    : null;
                var user = Session["Usuario"] as Entidades.Usuario;
                ViewBag.FacturaTicket = (facturaTicket != null && facturaTicket.Id > 0)
                    ? BuildFacturaDTO(venta, facturaTicket)
                    : null;
                ViewBag.EmpresaSesion = (user != null ? user.Empresa : null) ?? (venta.Sucursal != null ? venta.Sucursal.Empresa : null);
                var empresaTicket = (venta.Sucursal != null ? venta.Sucursal.Empresa : null) ?? (user != null ? user.Empresa : null);

                string negocio = ConfigurationManager.AppSettings["Negocio"];
                string negocioAgregado1 = ConfigurationManager.AppSettings["NegocioAgregado1"];
                string negocioAgregado2 = ConfigurationManager.AppSettings["NegocioAgregado2"];
                string negocioAgregado3 = ConfigurationManager.AppSettings["NegocioAgregado3"];

                ViewBag.Negocio = !string.IsNullOrWhiteSpace(negocio)
                    ? negocio
                    : (empresaTicket != null ? (empresaTicket.NombreFantasia ?? empresaTicket.RazonSocialAfip ?? "CarniSys") : "CarniSys");
                ViewBag.NegocioAgregado1 = !string.IsNullOrWhiteSpace(negocioAgregado1)
                    ? negocioAgregado1
                    : (empresaTicket != null ? empresaTicket.Slogan1 ?? "" : "");
                ViewBag.NegocioAgregado2 = !string.IsNullOrWhiteSpace(negocioAgregado2)
                    ? negocioAgregado2
                    : (empresaTicket != null ? empresaTicket.Slogan2 ?? "" : "");
                ViewBag.NegocioAgregado3 = !string.IsNullOrWhiteSpace(negocioAgregado3) && negocioAgregado3 != "-"
                    ? negocioAgregado3
                    : (empresaTicket != null ? empresaTicket.Slogan3 ?? "" : "");

                return View("~/Views/Ventas/_TicketHTML.cshtml", venta);

            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ImprimirTicketPayload(int id, int mm = 80)
        {
            try
            {
                var venta = oVentaN.getVentaById(id);
                if (venta == null)
                    return Json(new { ok = false, mensaje = "Venta no encontrada." }, JsonRequestBehavior.AllowGet);

                int ticketMm = mm == 58 ? 58 : 80;
                int idFacturaElectronica = oVentaN.existeFactuElectParaVenta(venta.IdVenta);
                var facturaTicket = idFacturaElectronica > 0
                    ? oVentaN.getFactuElecById(idFacturaElectronica)
                    : null;
                var user = Session["Usuario"] as Entidades.Usuario;
                var empresaSesion = (user != null ? user.Empresa : null) ?? (venta.Sucursal != null ? venta.Sucursal.Empresa : null);
                var facturaDto = (facturaTicket != null && facturaTicket.Id > 0)
                    ? BuildFacturaDTO(venta, facturaTicket)
                    : null;

                return Json(new
                {
                    ok = true,
                    ticketMm = ticketMm,
                    ticketLines = ConstruirLineasTicketVenta(venta, ticketMm, facturaDto, empresaSesion)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ImprimirIngresoBilletesPayload(IngresoBilletesPrintVm request)
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
                    ticketLines = ConstruirLineasIngresoBilletes(request, ticketMm)
                });
            }
            catch (Exception ex)
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


        public ActionResult Imprimir(int id)
        {
            Entidades.Venta venta = oVentaN.getVentaById(id);
            Entidades.FacturaElectronica factuElec = oVentaN.getFactuElecById(oVentaN.existeFactuElectParaVenta(id));

            var generador = new Utilidades.GenerarDocs();
            byte[] pdfBytes = generador.GenerarFacturaPDF(venta, factuElec);
            return File(pdfBytes, "application/pdf", $"Factura_{id}.pdf");
        }

        #endregion

        #region AFIP

        public ActionResult ProbarLoginAfip()
        {
            ///OBTENER LOS DATOS DE LA EMPRESA LOGUEADA
            ///SE DEBERIAN RECUPERAR DE LA SESSION

            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "AFIP",
                "20306210786"//empresa.Cuit
            );

            string rutaCertificado = Path.Combine(
                basePath,
                "certif-prod.pfx"// empresa.AfipCertFileName //-- ej: AFIP\20123456789\certificado.pfx
            );

            string rutaTA = Path.Combine(
                basePath,
                "TicketAcceso.txt"
            );


            var login = new LoginClass(
                "wsfe",
                "https://wsaa.afip.gov.ar/ws/services/LoginCms",
                Path.Combine(rutaCertificado),
                "",//ConfigurationManager.AppSettings["ClaveCertificadoAFIP"], //la clave la mando vacia en winform
                Path.Combine(rutaTA),
                basePath
            );

            login.HacerLogin();

            return Content(
                $"OK<br/>Token: {login.Token}<br/>Vence: {login.ExpirationTime}"
            );
        }

        [HttpPost]
        public JsonResult GenerarFactura(Web.Models.DTO.FacturaElectronicaDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errs = ModelState
                        .Where(kv => kv.Value.Errors.Any())
                        .Select(kv => kv.Key + ": " + string.Join(" | ", kv.Value.Errors.Select(e => e.ErrorMessage)))
                        .ToList();

                    return Json(new { ok = false, msg = "ModelState inválido", errs });
                }

                var factura = MapDtoToFactura(dto);
                
                bool esNotaCredito = dto.CodTipoCbteAfip == 3 || dto.CodTipoCbteAfip == 8 || dto.CodTipoCbteAfip == 13; // CodTipoCbteAfip 3=NC B, 8=NC A

                // Validar existencia de la venta
                if (factura.Venta == null)
                    return Json(new { ok = false, msg = "Venta no encontrada" });


                // Idempotencia: si ya existe factura (o nota) para esta venta devolvemos la info
                int idFactExistente = oVentaN.esVentaSinFacturar(factura.Venta.IdVenta, esNotaCredito);
                if (idFactExistente > 0)
                {
                    var fExist = oVentaN.getFactuElecById(idFactExistente);
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

                // Llamar al servicio AFIP (encapsulado en AFIP.GenerarFacturaService)
                var afipSvc = new AFIP.GenerarFacturaService(factura.Venta);
                var afipRes = afipSvc.GenerarFactura(factura, esNotaCredito);

                // Si AFIP devolvió error, persistir registro de fallo y devolver error al cliente
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
                        oVentaN.addOrEditFactuElec(factErr);
                    }
                    catch
                    {
                        // no bloquear la respuesta por fallo al guardar el error, sólo loguear si es necesario
                    }

                    return Json(new { ok = false, msg = "AFIP: " + afipRes.Mensaje });
                }

                //// AFIP ok → persistir la factura en BD y asociarla a la venta
                //var factura = afipRes.Factura ?? new Entidades.FacturaElectronica();

                //factura.IdVenta = idVenta;
                try
                {
                    oVentaN.addOrEditFactuElec(afipRes.Factura);
                }
                catch (Exception saveEx)
                {
                    // Si falla guardar, informar pero mantener la info AFIP en el mensaje
                    return Json(new { ok = false, msg = "Error guardando factura en BD: " + saveEx.Message });
                }

                // Recuperar id guardado (método seguro que ya usás)
                int idGuardado = oVentaN.esVentaSinFacturar(factura.Venta.IdVenta, esNotaCredito);

                var facturaGuardada = idGuardado > 0 ? oVentaN.getFactuElecById(idGuardado) : factura;

                // Retornar resultado al cliente
                return Json(new
                {
                    ok = true,
                    facturaId = idGuardado,
                    nro = facturaGuardada?.NroCbteAfip,
                    cae = facturaGuardada?.CAE1,
                    mensaje = afipRes.Mensaje ?? "Factura generada correctamente"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new
                {
                    ok = false,
                    msg = "Error generando factura",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region MAPEAR

        private Entidades.CierreCaja ObtenerCierreMisVentas(Entidades.Usuario user, bool desdePos, int idCierre)
        {
            if (idCierre > 0)
            {
                var cierrePorId = oCierreN.findByIdOrLast(
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
                user.Sucursal = oSucursalN.findById(user.IdSucursal);

            var cierre = new Entidades.CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
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

        private Entidades.CierreCaja ObtenerCierreCajaActual(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null)
                user.Sucursal = oSucursalN.findById(user.IdSucursal);

            if (user.Sucursal == null)
                return null;

            var cierre = new Entidades.CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            bool cajaAbierta = cierre != null && (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
            return cajaAbierta ? cierre : null;
        }

        private bool PuedeModificarUltimaVenta(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null)
                return false;

            user = user ?? (Session["Usuario"] as Entidades.Usuario);
            if (user == null)
                return false;

            if (TienePermisoAdministrativoSobreVenta(venta, user))
                return true;

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.UsuarioInicio == null)
                return false;

            var ultimaVenta = oVentaN.getUltimaVentaVendedor(cierre);
            if (ultimaVenta == null || ultimaVenta.IdVenta != venta.IdVenta)
                return false;

            return PermisosHelper.TienePermisoEditar(Session, Permisos.Venta.UltimaVenta, venta.FechaVenta, cierre.UsuarioInicio.Id);
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

        private string ObtenerMotivoNoPuedeModificarUltimaVenta(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null)
                return "Venta inexistente.";

            user = user ?? (Session["Usuario"] as Entidades.Usuario);
            if (user == null)
                return "Sesión inválida.";

            if (TienePermisoAdministrativoSobreVenta(venta, user))
                return "";

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.UsuarioInicio == null)
                return "No hay una caja abierta para el usuario actual.";

            var ultimaVenta = oVentaN.getUltimaVentaVendedor(cierre);
            if (ultimaVenta == null)
                return "No se encontró una última venta para este cajero.";

            if (ultimaVenta.IdVenta != venta.IdVenta)
                return "La venta visible no es la última venta del cajero.";

            bool tienePermiso = PermisosHelper.TienePermisoEditar(Session, Permisos.Venta.UltimaVenta, venta.FechaVenta, cierre.UsuarioInicio.Id);
            if (!tienePermiso)
                return "El usuario no tiene permiso vigente para formUltimaVenta.";

            return "";
        }

        private bool PuedeEditarFechaVenta(Entidades.Venta venta, DateTime? fechaSeleccionada = null)
        {
            if (venta == null)
                return false;

            int vendedorId = venta.Vendedor != null ? venta.Vendedor.Id : -1;
            return PermisosHelper.TienePermisoEditar(Session, Permisos.Venta.NuevaVenta, fechaSeleccionada ?? venta.FechaVenta, vendedorId);
        }

        private bool PuedeCambiarFormaPago(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null)
                return false;

            user = user ?? (Session["Usuario"] as Entidades.Usuario);
            if (user == null || user.IdSucursal == 0)
                return false;

            if (TienePermisoAdministrativoSobreVenta(venta, user))
                return true;

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.FechaHoraInicio == null || cierre.UsuarioInicio == null)
                return false;

            if (venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal)
                return false;

            if (venta.Vendedor == null || venta.Vendedor.Id != cierre.UsuarioInicio.Id)
                return false;

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            return venta.FechaVenta >= inicio && venta.FechaVenta <= fin;
        }

        private string ObtenerMotivoNoPuedeCambiarFormaPago(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null)
                return "Venta inexistente.";

            user = user ?? (Session["Usuario"] as Entidades.Usuario);
            if (user == null || user.IdSucursal == 0)
                return "Sesión inválida o sucursal no seleccionada.";

            if (TienePermisoAdministrativoSobreVenta(venta, user))
                return "";

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.FechaHoraInicio == null || cierre.UsuarioInicio == null)
                return "No hay una caja abierta para el usuario actual.";

            if (venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal)
                return "La venta pertenece a otra sucursal.";

            if (venta.Vendedor == null || venta.Vendedor.Id != cierre.UsuarioInicio.Id)
                return "La venta no pertenece a la caja actual del cajero.";

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            if (venta.FechaVenta < inicio || venta.FechaVenta > fin)
                return "La venta está fuera del rango de la caja actual.";

            return "";
        }

        private bool TienePermisoAdministrativoSobreVenta(Entidades.Venta venta, Entidades.Usuario user = null)
        {
            if (venta == null)
                return false;

            user = user ?? (Session["Usuario"] as Entidades.Usuario);
            if (user == null)
                return false;

            int idCreador = venta.Vendedor != null ? venta.Vendedor.Id : -1;
            return PermisosHelper.TienePermisoEditar(Session, Permisos.Venta.UltimaVenta, venta.FechaVenta, idCreador);
        }

        private List<Entidades.LineaVenta> ConstruirLineasVentaDesdeRequest(FinalizarVentaRequest request)
        {
            List<Entidades.LineaVenta> lineasVenta = new List<LineaVenta>();

            foreach (var l in request.LineasVenta)
            {
                var linea = new Entidades.LineaVenta();
                linea.Corte = oCorteN.findCorteByCodigo(l.Codigo, false);
                linea.KgsTotalCalculado = l.CantKg;
                linea.CantKg = l.CantKg;
                linea.PrecioKg = l.PrecioKg;
                linea.Bonificacion = l.Bonificacion;
                linea.Estado = l.Estado;
                linea.IndexAnulado = l.IndexAnulado;
                linea.PesoBalanza = l.Balanza;
                linea.IdExpendio = l.IdExpendio;

                lineasVenta.Add(linea);
            }

            return lineasVenta;
        }

        private List<int> ParseIdsExpendio(string idsActuales)
        {
            return (idsActuales ?? "")
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    int id;
                    return int.TryParse(x.Trim(), out id) ? id : 0;
                })
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }

        private int ToInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            int result;
            return int.TryParse(value.ToString(), out result) ? result : 0;
        }

        private decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal result;
            return decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result)
                || decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out result)
                ? result
                : 0m;
        }

        private DateTime ToDate(object value)
        {
            if (value == null || value == DBNull.Value) return DateTime.MinValue;
            DateTime result;
            return DateTime.TryParse(value.ToString(), out result) ? result : DateTime.MinValue;
        }

        private string ToStr(object value)
        {
            return value == null || value == DBNull.Value ? "" : value.ToString();
        }

        private void CompletarAnulacionesVenta(List<Entidades.LineaVenta> lineasVenta)
        {
            List<Entidades.LineaVenta> lineasAnuladas = new List<LineaVenta>();
            int cantLineaParam = lineasVenta.Count;

            for (int index = 0; index < lineasVenta.Count; index++)
            {
                if (Entidades.LineaVenta.esAnulado(lineasVenta[index].Estado) && lineasVenta[index].IndexAnulado == -1)
                {
                    lineasVenta[index].Estado = 0;

                    Entidades.LineaVenta oLineaVenta = new Entidades.LineaVenta();
                    oLineaVenta.Corte = lineasVenta[index].Corte;
                    oLineaVenta.Venta = lineasVenta[index].Venta;
                    oLineaVenta.CantKg = lineasVenta[index].CantKg * -1;
                    oLineaVenta.KgsTotalCalculado = lineasVenta[index].KgsTotalCalculado * -1;
                    oLineaVenta.KgsAjusteTarj = lineasVenta[index].KgsAjusteTarj * -1;
                    oLineaVenta.PrecioKg = lineasVenta[index].PrecioKg;
                    oLineaVenta.Estado = 1;
                    oLineaVenta.Bonificacion = lineasVenta[index].Bonificacion;
                    oLineaVenta.IndexAnulado = index;
                    oLineaVenta.IdExpendio = lineasVenta[index].IdExpendio;

                    lineasVenta[index].IndexAnulado = cantLineaParam++;
                    lineasAnuladas.Add(oLineaVenta);
                }
            }

            for (int index = 0; index < lineasAnuladas.Count; index++)
            {
                lineasVenta.Add(lineasAnuladas[index]);
            }
        }

        public FacturaElectronicaDTO BuildFacturaDTO(
                     Entidades.Venta venta,
                     Entidades.FacturaElectronica factuElec
                    )
        {
            var dto = new FacturaElectronicaDTO();

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

            // ===== EMISOR (TU EMPRESA) =====
            dto.PtoVtaAfip = venta.Sucursal.CodPuntoVentaAfip.ToString(); 
            dto.EmisorRazonSocial = venta.Sucursal.Empresa.RazonSocialAfip;
            dto.EmisorCUIT = venta.Sucursal.Empresa.Cuit.ToString();
            dto.EmisorCondicionIVA = venta.Sucursal.Empresa.CondicionIVA;
            dto.EmisorDomicilio = venta.Sucursal.Direccion;
            dto.EmisorIngresosBrutos = venta.Sucursal.Empresa.Iibb.ToString();
            dto.EmisorInicioActividad = venta.Sucursal.Empresa.InicioActividad.ToString("dd/MM/yyyy");

            // ===== Cliente =====
            dto.TipoDocAfip = (venta.Persona.IdIva == Entidades.FacturaElectronica.codCF_IvaAfip ?
                Entidades.FacturaElectronica.codTipoDoc_SinIdentif : Entidades.FacturaElectronica.codTipoDoc_CUIT).ToString();// factuElec.MapearCondicionIVAReceptorIdAfip(venta.Persona.IdIva).ToString();
            dto.NroDocAfip = venta.Persona.Cuit?.Replace("-", "");
            dto.RazonSocialAFIP = venta.Persona.razonSocial;
            dto.CondicionIvaAFIP = venta.Persona.Iva;
            dto.DomicilioAFIP = $"{venta.Persona.Domicilio} - {venta.Persona.Ciudad}";
            dto.Whatsapp = venta.Persona.Telefono;

            // ===== Venta =====
            dto.FormaPago = venta.FormaPago + (venta.PagoMixtoEfectivo > 0 ? " | Efectivo" : "");


            List<Entidades.AlicuotaIva> listaAlicuotasFactura = new List<Entidades.AlicuotaIva>();
            List<int> listaIdAlicuotaConIva = new List<int>();
            float importeTotal = 0, importeNeto = 0, importeIva = 0;

            //Inicializo valores de las Base Imponible de Alicuotas
            for (int i = 0; i < listaAlicuotasFactura.Count; i++)
            {
                listaAlicuotasFactura[i].BaseImponible = 0;
                listaAlicuotasFactura[i].Importe = 0;
            }

            // ===== Líneas =====
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
                    AlicuotaIva = l.AlicuotaIva,//(decimal)l.AlicuotaIva,
                    Bonificacion = l.Bonificacion,
                    Estado = l.Estado,
                    Balanza = l.PesoBalanza,
                    IndexAnulado = l.IndexAnulado,
                });

                ////recorro las lineas de venta para obtener las alicuotas utilizadas
                ////calculo de las Base Imponible de Alicuotas
                //for (int i = 0; i < listaAlicuotasFactura.Count; i++)
                //{
                //    if (listaAlicuotasFactura[i].IdIva == l.IdAlicuotaIva)
                //    {
                //        float totalProd = (float)Math.Round((l.CantKg * l.PrecioKg), 2);
                //        float divisorIva = 1 + (listaAlicuotasFactura[i].Iva / 100);
                //        float baseImponibleLinea = totalProd / divisorIva;
                //        importeTotal += totalProd;
                //        importeNeto += baseImponibleLinea;
                //        importeIva += totalProd - baseImponibleLinea;
                //        listaAlicuotasFactura[i].BaseImponible += (float)Math.Round(baseImponibleLinea, 2);
                //        listaAlicuotasFactura[i].Importe += (float)Math.Round((totalProd - baseImponibleLinea), 2);
                //    }
                //}
            }

            // ===== Importes =====
            // 
            dto.ImporteTotal = (decimal)venta.LineasVenta.Sum(l => l.ImporteConIva());
            dto.ImporteNetoGravado = (decimal)venta.LineasVenta.Sum(l => l.ImporteNeto());
            dto.Iva = (decimal)venta.LineasVenta.Sum(l => l.ImporteIva());

            // ===== CAE =====
            dto.CAE = factuElec.CAE1;
            dto.FecVtoCAE = factuElec.FecVtoCAE;

            return dto;
        }

        private List<string> ConstruirLineasTicketVenta(Entidades.Venta venta, int ticketMm, FacturaElectronicaDTO factura, Entidades.Empresa empresaSesion)
        {
            int cantMaxChar = ticketMm == 58 ? 32 : 43;
            bool esFacturada = factura != null && factura.IdFactura > 0 && !string.IsNullOrWhiteSpace(factura.CAE);
            bool esFacturaA = esFacturada && factura.CodTipoCbteAfip == Entidades.FacturaElectronica.codFacturaA_Afip;

            Func<string, int, string> truncar = (texto, maximo) =>
            {
                texto = texto ?? "";
                return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
            };

            Func<string, int, string> centrar = (texto, ancho) =>
            {
                texto = truncar(texto, ancho);
                int espaciosIzquierda = (ancho - texto.Length) / 2;
                if (espaciosIzquierda < 0) espaciosIzquierda = 0;
                return new string(' ', espaciosIzquierda) + texto;
            };

            Func<string, string, int, string> alinearExtremos = (izquierda, derecha, ancho) =>
            {
                izquierda = truncar(izquierda, 18);
                derecha = truncar(derecha, 18);
                int espacios = ancho - (izquierda.Length + derecha.Length);
                if (espacios < 1) espacios = 1;
                return izquierda + new string(' ', espacios) + derecha;
            };

            Func<string, decimal, int, string> formatearTotal = (etiqueta, importe, ancho) =>
            {
                string derecha = importe.ToString("N2");
                int espacios = ancho - (etiqueta.Length + derecha.Length);
                if (espacios < 1) espacios = 1;
                return etiqueta + new string(' ', espacios) + derecha;
            };

            Func<string, decimal, int, string> formatearArticulo = (producto, total, ancho) =>
            {
                string descripcion = truncar(producto, 22);
                string importe = total.ToString("N2");
                int espacios = ancho - (descripcion.Length + importe.Length);
                if (espacios < 1) espacios = 1;
                return descripcion + new string(' ', espacios) + importe;
            };

            Func<int, string> obtenerTituloComprobante = cod =>
            {
                switch (cod)
                {
                    case 1: return "Factura A";
                    case 6: return "Factura B";
                    case 11: return "Factura C";
                    default: return "Factura";
                }
            };

            var lineas = new List<string>();
            string negocio = ConfigurationManager.AppSettings["Negocio"] ?? "";
            string negocioAgregado1 = ConfigurationManager.AppSettings["NegocioAgregado1"] ?? "";
            string negocioAgregado2 = ConfigurationManager.AppSettings["NegocioAgregado2"] ?? "";
            string negocioAgregado3 = ConfigurationManager.AppSettings["NegocioAgregado3"] ?? "";
            string formaPagoImprimir = venta.PagoMixtoEfectivo > 0
                ? (venta.FormaPago ?? "") + "|Efvo"
                : (venta.FormaPago ?? "");

            lineas.Add(centrar(esFacturada ? obtenerTituloComprobante(factura.CodTipoCbteAfip) : "X", cantMaxChar));
            lineas.Add(centrar(esFacturada ? ("COD." + factura.CodTipoCbteAfip.ToString("00")) : "-No valido como Factura-", cantMaxChar));
            if (!string.IsNullOrWhiteSpace(negocio)) lineas.Add(centrar(negocio, cantMaxChar));
            if (!string.IsNullOrWhiteSpace(negocioAgregado1)) lineas.Add(centrar(negocioAgregado1, cantMaxChar));
            if (!string.IsNullOrWhiteSpace(negocioAgregado2)) lineas.Add(centrar(negocioAgregado2, cantMaxChar));
            if (!string.IsNullOrWhiteSpace(negocioAgregado3) && negocioAgregado3 != "-") lineas.Add(centrar(negocioAgregado3, cantMaxChar));

            if (esFacturada && empresaSesion != null)
            {
                lineas.Add(truncar("CUIT: " + empresaSesion.Cuit, cantMaxChar));
                lineas.Add(truncar("IIBB: " + empresaSesion.Iibb, cantMaxChar));
                lineas.Add(truncar(empresaSesion.RazonSocialAfip ?? "", cantMaxChar));
                lineas.Add(truncar((empresaSesion.Domicilio ?? "") + (string.IsNullOrWhiteSpace(empresaSesion.Ciudad) ? "" : " - " + empresaSesion.Ciudad), cantMaxChar));
                lineas.Add(truncar("Inicio Activ.: " + empresaSesion.InicioActividad.ToString("dd/MM/yyyy"), cantMaxChar));
                lineas.Add(truncar(empresaSesion.CondicionIVA ?? "", cantMaxChar));
            }

            lineas.Add("");
            if (!esFacturada)
            {
                if (venta.EnCtaCte && venta.FormaPago == "Efectivo")
                    lineas.Add(centrar("A Cta. Cte.", cantMaxChar));

                lineas.Add(truncar("A " + (venta.Persona != null ? venta.Persona.razonSocial : ""), cantMaxChar));
                lineas.Add(truncar("Forma Pago: " + formaPagoImprimir, cantMaxChar));
                lineas.Add(truncar("Nro. T. " + venta.IdVenta, cantMaxChar));
                lineas.Add(alinearExtremos("Fecha: " + venta.FechaVenta.ToString("dd/MM/yyyy"), "Hora: " + venta.FechaVenta.ToString("HH:mm"), cantMaxChar));
                lineas.Add(new string('-', cantMaxChar));
            }
            else
            {
                lineas.Add(new string('-', cantMaxChar));
                lineas.Add(truncar("Nro. " + (factura.PtoVtaAfip ?? "") + "-" + (factura.NroCbteAfip ?? ""), cantMaxChar));
                lineas.Add(truncar("Fecha: " + (factura.FechaEmisionAfip.HasValue ? factura.FechaEmisionAfip.Value.ToString("dd/MM/yyyy") : venta.FechaVenta.ToString("dd/MM/yyyy")), cantMaxChar));
                lineas.Add(truncar("Pago: " + (factura.FormaPago ?? formaPagoImprimir), cantMaxChar));
                lineas.Add(new string('-', cantMaxChar));
                lineas.Add(truncar(factura.RazonSocialAFIP ?? (venta.Persona != null ? venta.Persona.razonSocial : ""), cantMaxChar));
                if (!string.IsNullOrWhiteSpace(factura.NroDocAfip)) lineas.Add(truncar("CUIT: " + factura.NroDocAfip, cantMaxChar));
                if (!string.IsNullOrWhiteSpace(factura.CondicionIvaAFIP)) lineas.Add(truncar(factura.CondicionIvaAFIP, cantMaxChar));
                if (!string.IsNullOrWhiteSpace(factura.DomicilioAFIP)) lineas.Add(truncar(factura.DomicilioAFIP, cantMaxChar));
                lineas.Add(new string('-', cantMaxChar));
            }

            foreach (var item in venta.LineasVenta ?? new List<Entidades.LineaVenta>())
            {
                if (item == null) continue;

                decimal cantidad = Convert.ToDecimal(item.CantKg);
                decimal precio = Convert.ToDecimal(item.PrecioKg);
                decimal totalLinea = cantidad * precio;
                string producto = ((item.Corte != null ? item.Corte.corte : "") ?? "").Trim();

                if (esFacturaA)
                {
                    decimal divisorIva = 1m + (Convert.ToDecimal(item.AlicuotaIva) / 100m);
                    decimal precioNeto = divisorIva != 0 ? (precio / divisorIva) : precio;
                    decimal importeNeto = cantidad * precioNeto;
                    lineas.Add(cantidad.ToString("F3") + " x " + precioNeto.ToString("N2"));
                    lineas.Add(formatearArticulo(producto, importeNeto, cantMaxChar));
                }
                else
                {
                    lineas.Add(cantidad.ToString("F3") + " x " + precio.ToString("N2"));
                    lineas.Add(formatearArticulo(producto, totalLinea, cantMaxChar));
                }
            }

            if (!esFacturaA)
            {
                lineas.Add("-------".PadLeft(cantMaxChar));
                lineas.Add(formatearTotal(esFacturada ? "TOTAL" : "Total", esFacturada ? factura.ImporteTotal : Convert.ToDecimal(venta.TotalImporte), cantMaxChar));
            }

            if (esFacturaA)
            {
                lineas.Add("-------".PadLeft(cantMaxChar));
                lineas.Add(formatearTotal("Neto s/iva", factura.ImporteNetoGravado, cantMaxChar));

                var alicuotas = (venta.LineasVenta ?? new List<Entidades.LineaVenta>())
                    .GroupBy(x => x.AlicuotaIva)
                    .Select(g => new
                    {
                        Alicuota = g.Key,
                        Importe = g.Sum(x => Convert.ToDecimal(x.ImporteIva()))
                    })
                    .Where(x => x.Importe != 0m)
                    .OrderBy(x => x.Alicuota)
                    .ToList();

                foreach (var item in alicuotas)
                    lineas.Add(formatearTotal("IVA " + Convert.ToDecimal(item.Alicuota).ToString("N2") + "%", Convert.ToDecimal(item.Importe), cantMaxChar));

                lineas.Add(formatearTotal("TOTAL", factura.ImporteTotal, cantMaxChar));
            }

            if (!esFacturada && venta.Abona > 0)
            {
                lineas.Add(formatearTotal("Pago", Convert.ToDecimal(venta.Abona), cantMaxChar));
                lineas.Add(formatearTotal("Vuelto", Convert.ToDecimal(venta.Cambio), cantMaxChar));
            }

            if (esFacturada)
            {
                lineas.Add("");
                lineas.Add(truncar("Regimen de Transparencia Fiscal", cantMaxChar));
                lineas.Add(truncar("Al Consumidor (Ley 27.743)", cantMaxChar));
                lineas.Add(truncar("IVA Contenido: " + factura.Iva.ToString("N2"), cantMaxChar));
                lineas.Add(truncar("CAE: " + (factura.CAE ?? ""), cantMaxChar));
                lineas.Add(truncar("Vto: " + (factura.FecVtoCAE ?? ""), cantMaxChar));
            }

            if (!string.IsNullOrWhiteSpace(venta.Observaciones))
            {
                lineas.Add("");
                lineas.Add("Comentario:");
                string observacion = venta.Observaciones ?? "";
                for (int i = 0; i < observacion.Length; i += cantMaxChar)
                    lineas.Add(observacion.Substring(i, Math.Min(cantMaxChar, observacion.Length - i)));
            }

            lineas.Add("");
            lineas.Add("Gracias por su visita");
            return lineas;
        }

        private List<string> ConstruirLineasIngresoBilletes(IngresoBilletesPrintVm request, int ticketMm)
        {
            int cantMaxChar = ticketMm == 58 ? 32 : 43;
            var user = Session["Usuario"] as Entidades.Usuario;
            string empresaNombre = user != null && user.Empresa != null
                ? (user.Empresa.NombreFantasia ?? user.Empresa.RazonSocialAfip ?? "CarniSys")
                : "CarniSys";

            Func<string, int, string> truncar = (texto, maximo) =>
            {
                texto = texto ?? "";
                return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
            };

            Func<string, int, string> centrar = (texto, ancho) =>
            {
                texto = truncar(texto, ancho);
                int espaciosIzquierda = (ancho - texto.Length) / 2;
                if (espaciosIzquierda < 0) espaciosIzquierda = 0;
                return new string(' ', espaciosIzquierda) + texto;
            };

            Func<string, string, int, string> alinearExtremos = (izquierda, derecha, ancho) =>
            {
                izquierda = truncar(izquierda, ancho - 8);
                derecha = truncar(derecha, ancho - 8);
                int espacios = ancho - (izquierda.Length + derecha.Length);
                if (espacios < 1) espacios = 1;
                return izquierda + new string(' ', espacios) + derecha;
            };

            var lineas = new List<string>();
            lineas.Add(centrar("Detalle billetes", cantMaxChar));
            lineas.Add(centrar(empresaNombre, cantMaxChar));
            lineas.Add(truncar("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), cantMaxChar));
            lineas.Add(new string('-', cantMaxChar));

            foreach (var item in (request.Denominaciones ?? new List<IngresoBilletesDenominacionVm>())
                .Where(x => x != null && x.Denominacion > 0 && x.Cantidad > 0)
                .OrderByDescending(x => x.Denominacion))
            {
                decimal subtotal = item.Denominacion * item.Cantidad;
                lineas.Add(alinearExtremos(
                    "$ " + item.Denominacion.ToString("N0") + " x " + item.Cantidad,
                    subtotal.ToString("N2"),
                    cantMaxChar));
            }

            if (request.Monedas > 0)
            {
                lineas.Add(alinearExtremos("Monedas", request.Monedas.ToString("N2"), cantMaxChar));
            }

            lineas.Add(new string('-', cantMaxChar));
            lineas.Add(alinearExtremos("TOTAL", request.Total.ToString("N2"), cantMaxChar));
            lineas.Add("");
            lineas.Add("Gracias por su visita");

            return lineas;
        }

        public FacturaElectronica MapDtoToFactura(FacturaElectronicaDTO dto)
        {
            return new FacturaElectronica
            {
                IdVenta = dto.IdVenta,
                Venta = oVentaN.getVentaById(dto.IdVenta),
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

                PorcentajeFacturacion = (float)dto.PorcentajeFacturacion,
                ImporteNetoGravado = (float)dto.ImporteNetoGravado,
                Iva = (float)dto.Iva,
                ImporteTotal = (float)dto.ImporteTotal,

                CAE1 = dto.CAE,
                FecVtoCAE = dto.FecVtoCAE,

                // Otros campos que podés asignar si son necesarios:
                Creado = DateTime.Now,
                Error = false
            };
        }

        #endregion
    }

}
