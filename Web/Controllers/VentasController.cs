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
using Web.Models.DTO;
using AFIP;
using static Entidades.Venta;
using System.IO;

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
                ViewBag.Seccion = "Ventas";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }


            //si ambas fechas son iguales se suma 24 horas a fechaHasta
            if (desde == hasta && desde.Hour == 0)
            {
                hasta = hasta.AddDays(1);
            }


            var sucursales = oSucursalN.findAll(); // Obtiene List<Entidades.Sucursal>

            ViewBag.Sucursales = sucursales;
            ViewBag.IdSucursalSeleccionada = idSucursal;

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

        // GET: Ventas/DetalleVenta/5
        public ActionResult DetalleVenta(int id)
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

            // Pasar la venta a la vista
            return View(venta);
        }

        public ActionResult Imprimir(int id)
        {
            Entidades.Venta venta = oVentaN.getVentaById(id);

            var generador = new Utilidades.GenerarDocs();
            byte[] pdfBytes = generador.GenerarFacturaX(venta);
            return File(pdfBytes, "application/pdf", $"Factura_{id}.pdf");
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

                if (venta == null)
                    return Json(new { ok = false, msg = "No hay venta activa" });

                if (request.LineasVenta == null || !request.LineasVenta.Any())
                    return Json(new { ok = false, msg = "No hay productos en la venta" });



                venta.Persona = oPersonaN.findById(request.IdPersona);
                venta.Sucursal = oSucursalN.findById(user.IdSucursal);

                venta.TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString());
                venta.Observaciones = venta.Observaciones ?? "";

                // ===============================
                // FORMAS DE PAGO
                // ===============================
                venta.FormaPago = request.FormaPago;
                venta.EnCtaCte = request.FormaPago == formaPagoEnum.CtaCte.ToString();
                venta.PagoMixtoEfectivo = request.EsPagoMixto ? request.Efectivo : 0;

                //VALIDAR VENTA EN CTACTE sea solo en Cta Cte Y NO A 
                if (venta.EnCtaCte && (!venta.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString()) ||
                    venta.Persona.idPersona.Equals(Entidades.Persona.idConsumidorFinal)))
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

                List<Entidades.LineaVenta> lineasVenta = new List<LineaVenta>();
                Entidades.LineaVenta linea;

                foreach (var l in request.LineasVenta)
                {
                    linea = new Entidades.LineaVenta();


                    linea.Corte = oCorteN.findCorteByCodigo(l.Codigo, false);

                    // En Negocio hace la asignacion inversa 'no recuerdo xq hice esto, posiblente por redondeo'
                    linea.KgsTotalCalculado = l.CantKg = l.CantKg;
                    linea.PrecioKg = l.PrecioKg;
                    linea.Bonificacion = l.Bonificacion;
                    linea.Estado = l.Estado;

                    lineasVenta.Add(linea);
                }

                List<Entidades.LineaVenta> lineasAnuladas = new List<LineaVenta>();
                Entidades.LineaVenta oLineaVenta;
                int cantLineaParam = lineasVenta.Count; //cantidad de lineas q vienen por param

                for (int index = 0; index < lineasVenta.Count; index++)
                {
                    ///crear Lineas de anulacion
                    if (Entidades.LineaVenta.esAnulado(lineasVenta[index].Estado))
                    {
                        lineasVenta[index].Estado = 0;//se lo setea a No anulado xq se esta creando el registro opuesto

                        oLineaVenta = new Entidades.LineaVenta();
                        oLineaVenta.Corte = lineasVenta[index].Corte;
                        oLineaVenta.Venta = lineasVenta[index].Venta;
                        oLineaVenta.CantKg = lineasVenta[index].CantKg * -1;
                        oLineaVenta.KgsTotalCalculado = lineasVenta[index].KgsTotalCalculado * -1;
                        oLineaVenta.KgsAjusteTarj = lineasVenta[index].KgsAjusteTarj * -1;
                        oLineaVenta.PrecioKg = lineasVenta[index].PrecioKg;
                        oLineaVenta.Estado = 1;//anulado
                        oLineaVenta.Bonificacion = lineasVenta[index].Bonificacion;
                        oLineaVenta.IndexAnulado = (index + 1);
                        oLineaVenta.IdExpendio = lineasVenta[index].IdExpendio;

                        //se agrega el index del anulado al corte seleccionado para anular
                        //--el index equivale a la cantidad en listaLineaVenta antes de cargarLista--
                        lineasVenta[index].IndexAnulado = cantLineaParam++;

                        lineasAnuladas.Add(oLineaVenta);
                    }
                }

                //cargo las lineas anuladas
                for (int index = 0; index < lineasAnuladas.Count; index++)
                {
                    lineasVenta.Add(lineasAnuladas[index]);
                }

                // ===============================
                // LINEAS DE VENTA (CLAVE)
                // ===============================
                venta.LineasVenta = lineasVenta;

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


        #region POS
        public ActionResult POS()
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

            user.Sucursal = user.Sucursal == null ? oSucursalN.findById(user.IdSucursal) : user.Sucursal;
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

            // 🚨 Si NO hay caja abierta, NO inicializo venta
            if (!cajaAbierta)
            {
                return View((Venta)null);  // La vista muestra modal y POS bloqueado
            }

            // ==========================
            // POS habilitado
            // ==========================
            var venta = Session["VentaActiva"] as Venta;

            if (venta == null)
            {
                venta = new Venta
                {
                    LineasVenta = new List<LineaVenta>()
                };
            }

            var oCliente = oPersonaN.getConsumidorFinal();

            venta.Persona = oCliente;
            venta.IdPersona = oCliente.idPersona;
            venta.Vendedor = user;
            venta.FechaVenta = DateTime.Now;

            Session["VentaActiva"] = venta;

            return View(venta);
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
                    //foreach (var l in venta.LineasVenta)
                    //{
                    //    dto.Detalle.Add(new LineaVentaDto
                    //    {
                    //        Codigo = l.Codigo,
                    //        CantKg = l.CantKg,
                    //        PrecioKg = l.PrecioKg,
                    //        Bonificacion = l.Bonificacion,
                    //        Estado = l.Estado,
                    //        Balanza = l.PesoBalanza
                    //    });
                    //}
                    return PartialView("~/Views/Ventas/_FacturaElectronica.cshtml", dto);
                }

                imprimirCbte = Entidades.Venta.imprimirCbteEnum.Ticket;
                // Genera el ticket (ESC/POS, PDF, etc)

                ViewBag.Medida = mm == 58 ? 58 : 80;

                return View("~/Views/Ventas/_TicketHTML.cshtml", venta);

            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
        public JsonResult GenerarFactura(int idVenta, bool esNotaCredito = false)
        {
            try
            {
                // Validar existencia de la venta
                var venta = oVentaN.getVentaById(idVenta);
                if (venta == null)
                    return Json(new { ok = false, msg = "Venta no encontrada" });

                // Idempotencia: si ya existe factura (o nota) para esta venta devolvemos la info
                int idFactExistente = oVentaN.esVentaSinFacturar(idVenta, esNotaCredito);
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
                var afipSvc = new AFIP.GenerarFacturaService(venta);
                var afipRes = afipSvc.GenerarFactura(venta, esNotaCredito);

                // Si AFIP devolvió error, persistir registro de fallo y devolver error al cliente
                if (!afipRes.Ok)
                {
                    try
                    {
                        var factErr = new Entidades.FacturaElectronica
                        {
                            IdVenta = idVenta,
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

                // AFIP ok → persistir la factura en BD y asociarla a la venta
                var factura = afipRes.Factura ?? new Entidades.FacturaElectronica();
                factura.IdVenta = idVenta;
                try
                {
                    oVentaN.addOrEditFactuElec(factura);
                }
                catch (Exception saveEx)
                {
                    // Si falla guardar, informar pero mantener la info AFIP en el mensaje
                    return Json(new { ok = false, msg = "Error guardando factura en BD: " + saveEx.Message });
                }

                // Recuperar id guardado (método seguro que ya usás)
                int idGuardado = oVentaN.esVentaSinFacturar(idVenta, esNotaCredito);

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


        public FacturaElectronicaDTO BuildFacturaDTO(
                     Entidades.Venta venta,
                     Entidades.FacturaElectronica factuElec
                    )
        {
            var dto = new FacturaElectronicaDTO();

            dto.IdVenta = venta.IdVenta;
            dto.IdFactura = factuElec.Id;
            dto.CodTipoCbteAfip = factuElec.CodTipoCbteAfip;
            dto.DescTipoCbteAfip = factuElec.DescTipoCbteAfip;
            dto.LetraCbte = factuElec.getLetraId_TipoCbte(factuElec.CodTipoCbteAfip).ToString();
            dto.NroCbteAfip = factuElec.NroCbteAfip;
            dto.FechaEmisionAfip = factuElec.Id > 0
                ? factuElec.FechaEmisionAfip
                : venta.FechaVenta;

            // ===== EMISOR (TU EMPRESA) =====
            dto.PtoVtaAfip = venta.Sucursal.CodPuntoVentaAfip.ToString(); 
            dto.EmisorRazonSocial = venta.Sucursal.Empresa.RazonSocialAfip;
            dto.EmisorCUIT = venta.Sucursal.Empresa.Cuit.ToString();
            //dto.EmisorRazonSocial = user.Empresa.RazonSocial // venta.Sucursal.RazonSocial;
            //dto.EmisorCUIT = user.Empresa.CUIT;
            //dto.EmisorCondicionIVA = user.Empresa.CondicionIVA;
            //dto.EmisorDomicilio = user.Sucursal.Domicilio;
            //dto.EmisorIngresosBrutos = user.EmpresaIngresosBrutos;
            //dto.EmisorInicioActividad = vuser.Empresa.InicioActividad?.ToString("dd/MM/yyyy");

            // ===== Cliente =====
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

            //iniciarTipoIva
            //Obtiene las Alícuotas y establece el 10.5%
            //for (int index = 0; index < TipoIVACmb.Items.Count; index++)
            //{
            //    IvaTipo item = (IvaTipo)TipoIVACmb.Items[index];

            //    //cargo las alicuotas de iva para luego aplicar el importe
            //    Entidades.AlicuotaIva oAli = new Entidades.AlicuotaIva();
            //    oAli.IdIva = Convert.ToInt32(item.Id);
            //    oAli.Iva = (float)(item.Desc.Replace("%", ""), true);
            //    listaAlicuotasFactura.Add(oAli);

            //}

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

                //recorro las lineas de venta para obtener las alicuotas utilizadas
                //calculo de las Base Imponible de Alicuotas
                for (int i = 0; i < listaAlicuotasFactura.Count; i++)
                {
                    if (listaAlicuotasFactura[i].IdIva == l.IdAlicuotaIva)
                    {
                        float totalProd = (float)Math.Round((l.CantKg * l.PrecioKg), 2);
                        float divisorIva = 1 + (listaAlicuotasFactura[i].Iva / 100);
                        float baseImponibleLinea = totalProd / divisorIva;
                        importeTotal += totalProd;
                        importeNeto += baseImponibleLinea;
                        importeIva += totalProd - baseImponibleLinea;
                        listaAlicuotasFactura[i].BaseImponible += (float)Math.Round(baseImponibleLinea, 2);
                        listaAlicuotasFactura[i].Importe += (float)Math.Round((totalProd - baseImponibleLinea), 2);
                    }
                }
            }

            // ===== Importes =====            


            dto.ImporteTotal =(decimal) importeTotal;// Math.Round((decimal)venta.TotalImporte, 2);
            dto.ImporteNetoGravado = (decimal)importeNeto;// dto.ImporteTotal;
            dto.Iva = (decimal)importeIva;

            // ===== CAE =====
            dto.CAE = factuElec.CAE1;
            dto.FecVtoCAE = factuElec.FecVtoCAE;

            return dto;
        }


        #endregion
    }

}