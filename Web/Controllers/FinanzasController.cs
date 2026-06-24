using System;
using System.Data;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Usuario = Negocio.Usuario;
using Web.Helpers;
using System.Globalization;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using Datos;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using Web.Models;
using System.Diagnostics;
using Utilidades;

namespace Web.Controllers
{
    public class FinanzasController : BaseController
    {
        private Negocio.CuentaCorriente oCtaCteN;
        private Negocio.CierreCaja oCierreN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Persona oPersonasN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oCtaCteN = new Negocio.CuentaCorriente(empresa, param);
            oCierreN = new Negocio.CierreCaja(empresa);
            oSucursalN = new Negocio.Sucursal(empresa, param);
            oUsuarioN = new Negocio.Usuario(empresa, param);
            oPersonasN = new Negocio.Persona(empresa, param);
        }

        // ***********************************************************
        //  Estos valores se pasan como QueryString o TempData
        // ***********************************************************
        public bool DesdePOS
        {
            get { return (bool)(TempData["DesdePOS"] ?? false); }
            set { TempData["DesdePOS"] = value; }
        }

        public Entidades.CierreCaja OCierreCajaE
        {
            get { return TempData["OCierreCajaE"] as Entidades.CierreCaja; }
            set { TempData["OCierreCajaE"] = value; }
        }

        private static string AppendCacheBuster(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            string separator = url.Contains("?") ? "&" : "?";
            return url + separator + "_ts=" + DateTime.UtcNow.Ticks;
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

                if (decoded.StartsWith("/") || decoded.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    return decoded;
            }
            catch
            {
                // Si no venia en base64 valido, lo dejamos tal cual.
            }

            return returnUrl;
        }


        // ============================================================
        // GET: /Finanzas/CtasCtes
        // ============================================================

        public ActionResult CtasCtes(string buscar = "", string ordenSaldo = "DESC", bool desdePos = false)
        {
            bool modoPos = desdePos || DesdePOS;
            bool renderParcial = modoPos || Request.IsAjaxRequest();
            var swTotal = Stopwatch.StartNew();
            long msPermiso = 0;
            long msPreparacion = 0;
            long msDatos = 0;

            try
            {
                var swEtapa = Stopwatch.StartNew();
                if (!modoPos)
                {
                    var user = Session["Usuario"] as Entidades.Usuario;
                    if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCtasCtes, null))
                    {
                        ViewBag.Seccion = "Cuenta Corriente";
                        return View("~/Views/Shared/AccesoDenegado.cshtml");
                    }
                }
                swEtapa.Stop();
                msPermiso = swEtapa.ElapsedMilliseconds;

                swEtapa.Restart();
                ordenSaldo = string.Equals(ordenSaldo, "ASC", StringComparison.OrdinalIgnoreCase)
                    ? "ASC"
                    : "DESC";

                ViewBag.Buscar = buscar;
                ViewBag.DesdePOS = modoPos;
                ViewBag.RenderSinLayout = renderParcial;
                ViewBag.OrdenSaldo = ordenSaldo;
                swEtapa.Stop();
                msPreparacion = swEtapa.ElapsedMilliseconds;

                // F2 puede traer muchas cuentas corrientes. Ordenar en SQL evita
                // un paso extra caro en memoria sobre toda la tabla ya cargada.
                swEtapa.Restart();
                DataTable dt = oCtaCteN.obtenerCtasCtes(buscar, null, ordenSaldo);
                swEtapa.Stop();
                msDatos = swEtapa.ElapsedMilliseconds;

                swTotal.Stop();
                PerformanceInstrumentation.LogServerEvent(
                    "Finanzas",
                    "CtasCtes",
                    swTotal.ElapsedMilliseconds,
                    "permiso=" + msPermiso.ToString() + " ms"
                        + " | preparar=" + msPreparacion.ToString() + " ms"
                        + " | datos=" + msDatos.ToString() + " ms"
                        + " | parcial=" + (renderParcial ? "true" : "false")
                        + " | rows=" + (dt != null ? dt.Rows.Count.ToString() : "0"),
                    null,
                    Request != null ? Request.RawUrl : null);

                if (renderParcial)
                    return PartialView("CtasCtes", dt);

                return View("CtasCtes", dt);
            }
            catch (Exception ex)
            {
                if (renderParcial)
                    return Content("<div class='alert alert-danger m-3'>Error: " + HttpUtility.HtmlEncode(ex.Message) + "</div>");

                return Content("Error: " + ex.Message);
            }
        }

        public ActionResult Cheques(string estado = "", string nroCheque = "", string desde = "", bool desdePos = false)
        {
            bool modoPos = desdePos || DesdePOS;
            bool renderParcial = modoPos || Request.IsAjaxRequest();

            try
            {
                if (!modoPos)
                {
                    if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCheques, null))
                    {
                        ViewBag.Seccion = "Cheques";
                        return View("~/Views/Shared/AccesoDenegado.cshtml");
                    }
                }

                DateTime fechaDesde = DateTime.Today.AddMonths(-1);
                if (!string.IsNullOrWhiteSpace(desde))
                {
                    DateTime.TryParse(desde, out fechaDesde);
                    if (fechaDesde == DateTime.MinValue)
                        fechaDesde = DateTime.Today.AddMonths(-1);
                }

                ViewBag.DesdePOS = modoPos;
                ViewBag.RenderSinLayout = renderParcial;
                ViewBag.FiltroEstado = estado ?? "";
                ViewBag.FiltroNroCheque = nroCheque ?? "";
                ViewBag.FiltroDesde = fechaDesde.ToString("yyyy-MM-dd");
                ViewBag.Bancos = oCtaCteN.getBancos();

                if (renderParcial)
                    return PartialView("Cheques");

                return View("Cheques");
            }
            catch (Exception ex)
            {
                if (renderParcial)
                    return Content("<div class='alert alert-danger m-3'>Error: " + HttpUtility.HtmlEncode(ex.Message) + "</div>");

                return Content("Error: " + ex.Message);
            }
        }

        // GET: Finanzas/CtaCtePersona
        public ActionResult CtaCtePersona(int idPersona, DateTime? fechaDesde, bool mostrarAnulados = false, bool desdePos = false, string returnUrl = "")
        {
            bool modoPos = desdePos || DesdePOS;
            bool renderParcial = modoPos || Request.IsAjaxRequest();

            try
            {
                if (!modoPos)
                {
                    var user = Session["Usuario"] as Entidades.Usuario;
                    if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCtaCtePersona, null))
                    {
                        ViewBag.Seccion = "Cuenta Corriente Persona";
                        return View("~/Views/Shared/AccesoDenegado.cshtml");
                    }
                }

                if (!fechaDesde.HasValue)
                    fechaDesde = DateTime.Now.Date;

                DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesde.Value);

                // MISMA LÓGICA QUE EN WINFORMS:
                // si NO mostrarAnulados => oculta repetidos y deja el de mayor ID
                if (!mostrarAnulados)
                {
                    dtMov = FiltrarRegistrosRepetidos(dtMov);
                }

                decimal saldo = 0;
                if (dtMov != null && dtMov.Rows.Count > 0)
                {
                    DataRow ultimaFila = dtMov.Rows[dtMov.Rows.Count - 1];

                    if (ultimaFila["Saldo"] != DBNull.Value)
                        saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
                }

                ViewBag.IdPersona = idPersona;
                ViewBag.Persona = oPersonasN.findById(idPersona);
                ViewBag.SaldoPersona = saldo;
                ViewBag.ReturnUrlCtaCte = DecodeReturnUrlIfNeeded(returnUrl);
                ViewBag.FechaDesde = fechaDesde.Value.ToString("yyyy-MM-dd");
                ViewBag.MostrarAnulados = mostrarAnulados;
                ViewBag.DesdePOS = modoPos;
                ViewBag.RenderSinLayout = renderParcial;

                if (renderParcial)
                    return PartialView("CtaCtePersona", dtMov);

                return View(dtMov);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar cuenta corriente: " + ex.Message;
                ViewBag.DesdePOS = modoPos;
                ViewBag.RenderSinLayout = renderParcial;
                ViewBag.MostrarAnulados = mostrarAnulados;

                if (renderParcial)
                    return PartialView("CtaCtePersona", new DataTable());

                return View(new DataTable());
            }
        }

        private DataTable FiltrarRegistrosRepetidos(DataTable dtMov)
        {
            if (dtMov == null)
                return new DataTable();

            if (dtMov.Rows.Count == 0)
                return dtMov;

            // Version O(n): para cada combinacion (tabla, idTabla, sucursal)
            // nos quedamos con el registro de mayor ID, igual que la logica anterior
            // pero sin comparar cada fila contra todas las demas.
            var maxIdPorClave = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in dtMov.Rows)
            {
                string clave = string.Join("|",
                    Convert.ToString(row["tabla"]),
                    Convert.ToString(row["idTabla"]),
                    Convert.ToString(row["sucursal"]));

                int id = Convert.ToInt32(row["id"]);

                if (!maxIdPorClave.TryGetValue(clave, out int idActual) || id > idActual)
                {
                    maxIdPorClave[clave] = id;
                }
            }

            DataTable filtrado = dtMov.Clone();

            foreach (DataRow row in dtMov.Rows)
            {
                string clave = string.Join("|",
                    Convert.ToString(row["tabla"]),
                    Convert.ToString(row["idTabla"]),
                    Convert.ToString(row["sucursal"]));

                int id = Convert.ToInt32(row["id"]);

                if (maxIdPorClave.TryGetValue(clave, out int idMaximo) && id == idMaximo)
                {
                    filtrado.ImportRow(row);
                }
            }

            return filtrado;
        }

        // POST: Finanzas/CtaCtePersona
        [HttpPost]
        public ActionResult CtaCtePersonaPost(int idPersona, DateTime fechaDesde)
        {
            // Redirecciono al GET con los parámetros
            return RedirectToAction("CtaCtePersona", new { idPersona = idPersona, fechaDesde = fechaDesde });
        }

        // ============================================================
        //  EXPORTAR A EXCEL
        // ============================================================
        public ActionResult ExportarExcelPersona(int idPersona, string fechaDesde, bool mostrarAnulados = false)
        {
            DateTime fecha = DateTime.ParseExact(fechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            DataTable dt = oCtaCteN.getCtaCteByIdPersona(idPersona, fecha);
            if (!mostrarAnulados)
            {
                dt = FiltrarRegistrosRepetidos(dt);
            }

            // Calcular saldos
            decimal saldo = 0;
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow ultimaFila = dt.Rows[dt.Rows.Count - 1];

                if (ultimaFila["Saldo"] != DBNull.Value)
                    saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
            }

            // EXPORTACIÓN A EXCEL SIN LIBRERÍAS (CSV)
            string csv = "Fecha;Tabla;Detalle;Importe;Saldo;Sucursal\n";

            foreach (DataRow r in dt.Rows)
            {
                csv += $"{Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy")};" +
                       $"{r["Tabla"]};" +
                       $"{r["Detalle"]};" +
                       $"{Convert.ToDecimal(r["Importe"]).ToString("N2")};" +
                       $"{Convert.ToDecimal(r["Saldo"]).ToString("N2")};" +
                       $"{r["Sucursal"]}\n";
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csv);

            string persona = ObtenerNombreArchivoPersona(dt, idPersona);
            string fileName = $"CuentaCorriente_{persona}_Desde_{fecha:yyyy-MM-dd}.csv";
            return File(buffer, "text/csv", fileName);
        }


        // ============================================================
        //  EXPORTAR A PDF
        // ============================================================
        public ActionResult ExportarPdfPersona(int idPersona, string fechaDesde, bool mostrarAnulados = false)
        {
            DateTime fechaD;
            if (!DateTime.TryParse(fechaDesde, out fechaD))
                fechaD = DateTime.Today.AddMonths(-1);

            DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaD);
            if (!mostrarAnulados)
            {
                dtMov = FiltrarRegistrosRepetidos(dtMov);
            }

            // Obtener persona y saldo igual que en la vista
            string persona = "";

            if (dtMov.Rows.Count > 0)
            {
                persona = dtMov.Rows[0]["razonSocial"].ToString();
            }

            persona = SanitizarNombreArchivo(persona);

            byte[] pdfBytes = Utilidades.GenerarDocs.GenerarPdfCtaCtePersona(dtMov, fechaD); // GenerarPdfPersona(dtMov, persona, saldo, fechaD);

            string fileName = $"CuentaCorriente_{persona}_Desde_{fechaD:yyyy-MM-dd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private string ObtenerNombreArchivoPersona(DataTable dt, int idPersona)
        {
            string persona = "";

            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("razonSocial"))
            {
                persona = Convert.ToString(dt.Rows[0]["razonSocial"]);
            }

            if (string.IsNullOrWhiteSpace(persona))
            {
                var personaObj = oPersonasN.findById(idPersona);
                persona = personaObj != null ? personaObj.razonSocial : "Persona";
            }

            return SanitizarNombreArchivo(persona);
        }

        private static string SanitizarNombreArchivo(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "Persona";

            string limpio = valor.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                limpio = limpio.Replace(c, '_');
            }

            limpio = Regex.Replace(limpio, "\\s+", "_");
            return string.IsNullOrWhiteSpace(limpio) ? "Persona" : limpio;
        }


        // ============================================================
        //  DETALLE DE MOVIMIENTO (placeholder)
        // ============================================================
        public ActionResult verMovimientoCtaCte(int idPersona, string returnUrl, string tabla, int idTabla = 0, bool desdePos = false)
        {
            Entidades.MovCtaCte oMovCtaCteE = new Entidades.MovCtaCte();
            Entidades.MovCtaCte.tablas tablaEnum = oMovCtaCteE.getTablaEnum(tabla);
            string decodedReturnUrl = DecodeReturnUrlIfNeeded(returnUrl);

            switch (tablaEnum)
            {
                case Entidades.MovCtaCte.tablas.Ventas:
                    return RedirectToAction("InfoVenta", "Finanzas", new { id = idTabla });

                case Entidades.MovCtaCte.tablas.Compras:
                    return RedirectToAction("ModificarCompra", "Compras", new { id = idTabla });

                case Entidades.MovCtaCte.tablas.Pagos:
                    return AddOrEditPago(idPersona, decodedReturnUrl, idTabla, desdePos);

                default:
                    return HttpNotFound();
            }
        }

        // ============================================================
        //  AGREGAR COBRO / PAGO (placeholder)
        // ============================================================

        public ActionResult AddOrEditPago(int idPersona, string returnUrl, int idPago = 0, bool desdePos = false)
        {
            bool modoPos = desdePos || DesdePOS;
            bool renderParcial = modoPos || Request.IsAjaxRequest();
            var user = Session["Usuario"] as Entidades.Usuario;
            var sucursales = oSucursalN.findAll();
            string returnUrlDecodificada = DecodeReturnUrlIfNeeded(returnUrl);
            Pago pagoExistente = null;

            if (user != null && user.IdSucursal > 0 &&
                (user.Sucursal == null || user.Sucursal.IdSucursal != user.IdSucursal))
            {
                user.Sucursal = oSucursalN.findById(user.IdSucursal);
                Session["Usuario"] = user;
            }

            if (modoPos)
            {
                var cierreCajaActual = ObtenerCajaAbiertaUsuario(user);
                if (cierreCajaActual == null)
                    return new HttpStatusCodeResult(403, "Debe tener una caja abierta en la sucursal activa para registrar pagos o cobros desde POS.");
            }

            if (idPago > 0)
            {
                pagoExistente = oCtaCteN.getPagoById(idPago);
                if (pagoExistente == null)
                    return HttpNotFound();

                if (idPersona <= 0 && pagoExistente.Persona != null)
                    idPersona = pagoExistente.Persona.IdPersona;
            }

            ViewBag.Sucursales = sucursales;
            ViewBag.ReturnUrl = returnUrlDecodificada;
            ViewBag.DesdePOS = modoPos;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.UsuarioAdmin = user != null && user.Admin;

            DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, DateTime.Today);

            decimal saldo = 0;
            if (dtMov != null && dtMov.Rows.Count > 0)
            {
                DataRow ultimaFila = dtMov.Rows[dtMov.Rows.Count - 1];

                if (ultimaFila["Saldo"] != DBNull.Value)
                    saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
            }

            var persona = oPersonasN.findById(idPersona);
            if (persona == null)
                return HttpNotFound();

            ViewBag.IdPersona = idPersona;
            ViewBag.Persona = persona;
            ViewBag.SaldoPersona = saldo;
            ViewBag.Bancos = oCtaCteN.getBancos();
            ViewBag.ImprimirPagoUrl = idPago > 0 ? Url.Action("ImprimirTicketPago", "Finanzas", new { id = idPago }) : "";
            ViewBag.ImprimirPagoPdfUrl = idPago > 0 ? Url.Action("ImprimirPdfPago", "Finanzas", new { id = idPago }) : "";
            ViewBag.ImprimirPagoPayloadUrl = idPago > 0 ? Url.Action("ImprimirTicketPagoPayload", "Finanzas", new { id = idPago }) : "";

            Pago model;

            if (idPago == 0)
            {
                model = new Pago();
                model.Fecha = DateTime.Now;
                model.Sucursal = user != null ? user.Sucursal : null;
                if (model.Sucursal == null)
                    model.Sucursal = oSucursalN.findById(user != null ? user.IdSucursal : 0);

                model.NroRecibo = model.Sucursal != null && model.Sucursal.idSucursal > 0
                    ? oCtaCteN.getNroReciboAutomatico(model.Sucursal.idSucursal)
                    : "";
            }
            else
            {
                model = pagoExistente;
            }

            if (renderParcial)
                return PartialView("AddOrEditPago", model);

            return View("AddOrEditPago", model);
        }

        [HttpPost]
        public ActionResult AddOrEditPagoPost(
            Pago oPagoE,
            string returnUrl,
            int SucursalId = 0,
            int idPersona = 0,
            string importe = "",
            string Efectivo = "",
            string ChequesJson = "",
            bool desdePos = false)
        {
            returnUrl = DecodeReturnUrlIfNeeded(returnUrl);
            var usuarioActual = Session["Usuario"] as Entidades.Usuario;
            bool modoPos = desdePos || DesdePOS;

            if (usuarioActual != null && usuarioActual.IdSucursal > 0 &&
                (usuarioActual.Sucursal == null || usuarioActual.Sucursal.IdSucursal != usuarioActual.IdSucursal))
            {
                usuarioActual.Sucursal = oSucursalN.findById(usuarioActual.IdSucursal);
                Session["Usuario"] = usuarioActual;
            }

            if (modoPos && usuarioActual != null && !usuarioActual.Admin)
                SucursalId = usuarioActual.IdSucursal;

            oPagoE.Sucursal = oSucursalN.findById(SucursalId);
            oPagoE.Persona = oPersonasN.findById(idPersona);

            if (!string.IsNullOrEmpty(ChequesJson))
            {
                oPagoE.Cheques = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Cheque>>(ChequesJson);
                for (int index = 0; index < oPagoE.Cheques.Count; index++)
                    oPagoE.Cheques[index] = oCtaCteN.getChequePorIDorNro(oPagoE.Cheques[index].Id, "");
            }
            else
            {
                oPagoE.Cheques = new List<Cheque>();
            }

            oPagoE.Importe = ParseFloat(importe);
            oPagoE.Efectivo = Efectivo == null ? 0 : ParseFloat(Efectivo);
            oPagoE.Banco = "";
            oPagoE.NroCheque = "";
            oPagoE.TitularCheque = "";

            Pago pagoAnterior = oPagoE.Id > 0 ? oCtaCteN.getPagoById(oPagoE.Id) : null;
            if (oPagoE.Id > 0 && pagoAnterior == null)
                return Json(new { ok = false, mensaje = "No se encontró el pago o cobro a modificar." });

            oPagoE.CreadoPor = oPagoE.Id > 0
                ? pagoAnterior.CreadoPor
                : usuarioActual;

            oPagoE.ActualizadoPor = oPagoE.Id > 0
                ? usuarioActual
                : oPagoE.ActualizadoPor;

            oPagoE.FormaPago = oPagoE.FormaPago_.ToString();

            var (ok, mensaje) = oCtaCteN.ValidarPago(oPagoE);

            if (!ok)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = string.IsNullOrWhiteSpace(mensaje)
                        ? "No se pudo validar el pago."
                        : mensaje
                });
            }

            Entidades.CierreCaja cierreCajaActual = null;
            if (modoPos)
            {
                cierreCajaActual = ObtenerCajaAbiertaUsuario(usuarioActual);
                if (cierreCajaActual == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe tener una caja abierta en la sucursal activa para registrar pagos o cobros desde POS."
                    });
                }

                if (oPagoE.Sucursal == null || oPagoE.Sucursal.IdSucursal != usuarioActual.IdSucursal)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "El pago o cobro debe registrarse en la sucursal activa del POS."
                    });
                }

                if (!cierreCajaActual.FechaHoraInicio.HasValue ||
                    oPagoE.Fecha < cierreCajaActual.FechaHoraInicio.Value ||
                    oPagoE.Fecha > DateTime.Now)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La fecha y hora del pago o cobro debe corresponder a una caja abierta del vendedor."
                    });
                }
            }

            _ = oCtaCteN.addOrEditPago(oPagoE, cierreCajaActual, pagoAnterior);

            string urlRetornoDefault = !string.IsNullOrWhiteSpace(returnUrl)
                ? returnUrl
                : Url.Action("CtaCtePersona", "Finanzas", new
                {
                    idPersona = idPersona,
                    fechaDesde = DateTime.Today.ToString("yyyy-MM-dd")
                });

            string urlRetornoConCache = AppendCacheBuster(urlRetornoDefault);

            if (modoPos)
            {
                string pdfUrl = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id });
                string pdfUrlAbsoluta = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id }, Request != null && Request.Url != null ? Request.Url.Scheme : "http");

                return Json(new
                {
                    ok = true,
                    redirectUrl = urlRetornoConCache,
                    cerrarModalPago = true,
                    pagoId = oPagoE.Id,
                    imprimirUrl = Url.Action("ImprimirTicketPago", "Finanzas", new { id = oPagoE.Id }),
                    imprimirPayloadUrl = Url.Action("ImprimirTicketPagoPayload", "Finanzas", new { id = oPagoE.Id }),
                    pdfUrl,
                    emailConfigUrl = Url.Action("ObtenerDatosEmailPago", "Finanzas"),
                    emailSendUrl = Url.Action("EnviarComprobantePagoEmail", "Finanzas")
                });
            }

            if (Request.IsAjaxRequest())
            {
                string pdfUrl = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id });
                string pdfUrlAbsoluta = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id }, Request != null && Request.Url != null ? Request.Url.Scheme : "http");

                return Json(new
                {
                    ok = true,
                    redirectUrl = urlRetornoConCache,
                    pagoId = oPagoE.Id,
                    imprimirUrl = Url.Action("ImprimirTicketPago", "Finanzas", new { id = oPagoE.Id }),
                    imprimirPayloadUrl = Url.Action("ImprimirTicketPagoPayload", "Finanzas", new { id = oPagoE.Id }),
                    pdfUrl,
                    emailConfigUrl = Url.Action("ObtenerDatosEmailPago", "Finanzas"),
                    emailSendUrl = Url.Action("EnviarComprobantePagoEmail", "Finanzas")
                });
            }

            return Redirect(urlRetornoConCache);
        }

        [HttpGet]
        public ActionResult ImprimirTicketPago(int id, int mm = 80)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return HttpNotFound();

            ViewBag.TicketMm = mm == 58 ? 58 : 80;
            return View("~/Views/Finanzas/_TicketPago.cshtml", model);
        }

        [HttpGet]
        public JsonResult ImprimirTicketPagoPayload(int id, int mm = 80)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el pago." }, JsonRequestBehavior.AllowGet);

            int ticketMm = mm == 58 ? 58 : 80;
            return Json(new
            {
                ok = true,
                ticketMm = ticketMm,
                ticketLines = ConstruirLineasTicketPago(model, ticketMm)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DescargarAgenteImpresion()
        {
            string path = Server.MapPath("~/Content/downloads/CarniSys.PrintAgent.zip");
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            return File(path, "application/zip", "CarniSys.PrintAgent.zip");
        }

        [HttpGet]
        public ActionResult ImprimirPdfPago(int id)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return HttpNotFound();

            byte[] bytes = GenerarPdfPago(model);
            string nroRecibo = string.IsNullOrWhiteSpace(model.Pago.NroRecibo) ? ("Pago_" + model.Pago.Id) : model.Pago.NroRecibo.Replace("/", "-");
            string fileName = "Recibo_" + nroRecibo + ".pdf";
            return File(bytes, "application/pdf", fileName);
        }

        [HttpGet]
        public JsonResult ObtenerDatosEmailPago(int id)
        {
            try
            {
                var model = ConstruirReciboPagoVm(id);
                if (model == null || model.Pago == null || model.Pago.Id <= 0)
                    return Json(new { ok = false, msg = "Pago no encontrado." }, JsonRequestBehavior.AllowGet);

                var empresaPago = model.Empresa;
                string nombreEmpresa = empresaPago != null
                    ? (!string.IsNullOrWhiteSpace(empresaPago.NombreFantasia) ? empresaPago.NombreFantasia : empresaPago.RazonSocialAfip)
                    : "";
                if (string.IsNullOrWhiteSpace(nombreEmpresa))
                    nombreEmpresa = "CarniSys";

                string tipoOperacion = model.TipoOperacion ?? "Recibo";
                string nroRecibo = model.Pago.NroRecibo ?? ("#" + model.Pago.Id);
                string emailDestino = model.Pago.Persona != null ? (model.Pago.Persona.Email ?? "").Trim() : "";
                string asunto = tipoOperacion + " " + nroRecibo + " - " + nombreEmpresa;
                string cuerpo =
                    "Hola" + (model.Pago.Persona != null && !string.IsNullOrWhiteSpace(model.Pago.Persona.RazonSocial) ? " " + model.Pago.Persona.RazonSocial : "") + ",\n\n" +
                    "Te enviamos adjunto el recibo " + nroRecibo + ".\n\n" +
                    "Fecha: " + model.Pago.Fecha.ToString("dd/MM/yyyy HH:mm") + "\n" +
                    "Importe: $" + Convert.ToDecimal(model.Pago.Importe).ToString("N2") + "\n\n" +
                    "Saludos,\n" +
                    nombreEmpresa;

                return Json(new
                {
                    ok = true,
                    email = emailDestino,
                    asunto = asunto,
                    mensaje = cuerpo,
                    empresa = nombreEmpresa,
                    replyTo = empresaPago != null ? (empresaPago.Email ?? "") : ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EnviarComprobantePagoEmail(int idPago, string emailDestino, string asunto, string mensaje)
        {
            try
            {
                var model = ConstruirReciboPagoVm(idPago);
                if (model == null || model.Pago == null || model.Pago.Id <= 0)
                    return Json(new { ok = false, msg = "Pago no encontrado." });

                emailDestino = (emailDestino ?? "").Trim();
                asunto = (asunto ?? "").Trim();
                mensaje = (mensaje ?? "").Trim();

                if (string.IsNullOrWhiteSpace(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email destino." });

                if (!SmtpMailHelper.IsValidEmail(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email válido." });

                if (string.IsNullOrWhiteSpace(asunto))
                    return Json(new { ok = false, msg = "Ingrese un asunto." });

                byte[] pdfBytes = GenerarPdfPago(model);
                string nroRecibo = string.IsNullOrWhiteSpace(model.Pago.NroRecibo) ? ("Pago_" + model.Pago.Id) : model.Pago.NroRecibo.Replace("/", "-");
                string nombreAdjunto = "Recibo_" + nroRecibo + ".pdf";
                string nombreEmpresa = model.Empresa != null
                    ? (!string.IsNullOrWhiteSpace(model.Empresa.NombreFantasia) ? model.Empresa.NombreFantasia : model.Empresa.RazonSocialAfip)
                    : "";
                if (string.IsNullOrWhiteSpace(nombreEmpresa))
                    nombreEmpresa = "CarniSys";

                string fromName = "CarniSys - " + nombreEmpresa;
                string replyToEmail = model.Empresa != null ? (model.Empresa.Email ?? "").Trim() : "";

                SmtpMailHelper.SendMail(
                    toEmail: emailDestino,
                    toName: model.Pago.Persona != null ? model.Pago.Persona.RazonSocial : "",
                    subject: asunto,
                    bodyHtml: ConvertirTextoAHtmlPago(mensaje),
                    attachmentFileName: nombreAdjunto,
                    attachmentBytes: pdfBytes,
                    attachmentContentType: "application/pdf",
                    fromNameOverride: fromName,
                    replyToEmail: SmtpMailHelper.IsValidEmail(replyToEmail) ? replyToEmail : null,
                    replyToName: nombreEmpresa
                );

                return Json(new { ok = true, msg = "El recibo se envió correctamente." });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo enviar el email. " + ex.Message });
            }
        }

        private Entidades.CierreCaja ObtenerCajaAbiertaUsuario(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null || user.Sucursal.IdSucursal != user.IdSucursal)
                user.Sucursal = oSucursalN.findById(user.IdSucursal);

            if (user.Sucursal == null || user.Sucursal.IdSucursal == 0)
                return null;

            var cierre = new Entidades.CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");

            bool abierta = cierre != null && cierre.UsuarioCierre != null && cierre.UsuarioCierre.Id == 0;
            return abierta ? cierre : null;
        }

        private ReciboPagoVm ConstruirReciboPagoVm(int idPago)
        {
            var pago = oCtaCteN.getPagoById(idPago);
            if (pago == null || pago.Id <= 0)
                return null;

            var user = Session["Usuario"] as Entidades.Usuario;
            var empresaActual = user != null ? user.Empresa : null;

            if (empresaActual == null && pago.Sucursal != null && pago.Sucursal.IdEmpresa > 0)
                empresaActual = oSucursalN.findEmpresaById(pago.Sucursal.IdEmpresa);

            DataTable dtCtaCte = oCtaCteN.getCtaCteByIdPersona(pago.Persona.IdPersona, new DateTime(2000, 1, 1));
            decimal saldo = 0m;
            bool tieneSaldo = false;

            if (dtCtaCte != null && dtCtaCte.Rows.Count > 0)
            {
                DataRow ultimaFila = dtCtaCte.Rows[dtCtaCte.Rows.Count - 1];
                if (ultimaFila["Saldo"] != DBNull.Value)
                {
                    saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
                    tieneSaldo = true;
                }
            }

            string tipoOperacion = pago.AProveedor ? "Pago" : "Cobro";
            string personaEtiqueta = pago.AProveedor ? "Proveedor" : "Cliente";
            string detalleOperacion = pago.AProveedor
                ? "Se entrego dinero a la persona."
                : "Se recibio dinero de la persona.";

            return new ReciboPagoVm
            {
                Pago = pago,
                Empresa = empresaActual,
                Saldo = saldo,
                TieneSaldo = tieneSaldo,
                TipoOperacion = tipoOperacion,
                PersonaEtiqueta = personaEtiqueta,
                DetalleOperacion = detalleOperacion,
                UrlPdfAbsoluta = Url.Action("ImprimirPdfPago", "Finanzas", new { id = pago.Id }, Request != null && Request.Url != null ? Request.Url.Scheme : "http"),
                ComprobantesRelacionados = new List<string>()
            };
        }

        private byte[] GenerarPdfPago(ReciboPagoVm model)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 30, 30, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var colorRojo = new BaseColor(174, 0, 0);
                var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 25, colorRojo);
                var fuenteRazonSocial = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fuenteX = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35);
                var fuenteFooter = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                var fuenteMono = FontFactory.GetFont(FontFactory.COURIER, 9);

                string negocio = model.Empresa != null
                    ? (model.Empresa.NombreFantasia ?? model.Empresa.RazonSocialAfip ?? "CarniSys")
                    : "CarniSys";

                PdfPTable cabecera = new PdfPTable(3);
                cabecera.WidthPercentage = 100;
                cabecera.SetWidths(new float[] { 33f, 34f, 33f });

                PdfPCell izquierda = new PdfPCell { Border = Rectangle.NO_BORDER };
                izquierda.AddElement(new Paragraph(negocio + "\n", fuenteTitulo));
                izquierda.AddElement(new Paragraph(" ", fuenteRazonSocial));
                izquierda.AddElement(new Paragraph("Razón Social: " + (model.Empresa != null ? model.Empresa.RazonSocialAfip : "") + "\n", fuenteRazonSocial));
                izquierda.AddElement(new Paragraph(((model.Empresa != null ? model.Empresa.Domicilio : "") ?? "") + " - " + ((model.Empresa != null ? model.Empresa.Ciudad : "") ?? "") + "\n", fuenteRazonSocial));
                izquierda.AddElement(new Paragraph("Cond.IVA: " + (model.Empresa != null ? model.Empresa.CondicionIVA : "") + "\n", fuenteRazonSocial));
                cabecera.AddCell(izquierda);

                PdfPCell centro = new PdfPCell { Border = Rectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_MIDDLE };
                Paragraph parrafoCentro = new Paragraph { Alignment = Element.ALIGN_CENTER };
                parrafoCentro.Add(new Chunk("X\n", fuenteX));
                parrafoCentro.Add(new Chunk("- Documento no válido como factura -", fuenteFooter));
                centro.AddElement(parrafoCentro);
                cabecera.AddCell(centro);

                PdfPCell derecha = new PdfPCell { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
                derecha.AddElement(new Paragraph("N°Recibo: " + (model.Pago.NroRecibo ?? "") + "\n", fuenteNegrita));
                derecha.AddElement(new Paragraph("Fecha: " + model.Pago.Fecha.ToString("dd/MM/yyyy") + "\n\n", fuenteNormal));
                derecha.AddElement(new Paragraph((model.Empresa != null ? model.Empresa.Iibb.ToString() : "") + "\n", fuenteNormal));
                derecha.AddElement(new Paragraph("CUIT: " + (model.Empresa != null ? model.Empresa.Cuit.ToString() : "") + "\n", fuenteNormal));
                derecha.AddElement(new Paragraph("Inicio Act.: " + (model.Empresa != null ? model.Empresa.InicioActividad.ToString("dd/MM/yyyy") : "") + "\n", fuenteNormal));
                cabecera.AddCell(derecha);

                doc.Add(cabecera);
                LineSeparator linea = new LineSeparator(1.5f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -6);
                doc.Add(new Chunk(linea));
                doc.Add(new Paragraph(" "));

                PdfPTable cliente = new PdfPTable(4);
                cliente.WidthPercentage = 100;
                cliente.SetWidths(new float[] { 15, 45, 10, 20 });
                cliente.AddCell(CeldaSimple(model.PersonaEtiqueta + ":", fuenteNegrita));
                cliente.AddCell(CeldaSimple((model.Pago.Persona != null ? model.Pago.Persona.RazonSocial : "").ToUpperInvariant(), fuenteNormal));
                cliente.AddCell(CeldaSimple("Cond. IVA:", fuenteNegrita));
                cliente.AddCell(CeldaSimple(model.Pago.Persona != null ? model.Pago.Persona.Iva : "", fuenteNormal));
                cliente.AddCell(CeldaSimple("Domicilio:", fuenteNegrita));
                cliente.AddCell(CeldaSimple((model.Pago.Persona != null ? model.Pago.Persona.Domicilio : "").ToUpperInvariant(), fuenteNormal));
                cliente.AddCell(CeldaSimple("CUIT:", fuenteNegrita));
                cliente.AddCell(CeldaSimple(model.Pago.Persona != null ? model.Pago.Persona.Cuit : "", fuenteNormal));
                doc.Add(cliente);

                doc.Add(new Chunk(linea));
                doc.Add(new Paragraph(" "));

                string detallePago = model.Pago.Observaciones ?? "";
                string importesDetalle = model.Pago.Importe.ToString("F2", CultureInfo.InvariantCulture);

                if (!string.IsNullOrWhiteSpace(model.Pago.FormaPago) &&
                    model.Pago.FormaPago.IndexOf(Entidades.Pago.formasPago.Cheque.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int espaciosBlanco = 15;
                    double importesCheque = 0d;
                    var detalleBuilder = new System.Text.StringBuilder();
                    var importesBuilder = new System.Text.StringBuilder();

                    if (model.Pago.FormaPago.IndexOf("Eftvo", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        detalleBuilder.Append(AjustarString("Efectivo", espaciosBlanco, true));
                        detalleBuilder.Append(AjustarString(" ", espaciosBlanco, true));
                        detalleBuilder.Append(AjustarString(" ", espaciosBlanco, true));
                        detalleBuilder.Append(AjustarString(" ", espaciosBlanco, true));
                        detalleBuilder.Append("\n\n");
                        importesBuilder.AppendLine(model.Pago.Efectivo.ToString("F2", CultureInfo.InvariantCulture));
                    }

                    detalleBuilder.Append(AjustarString("Nro Cheque", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("Banco", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("Fecha Pago", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("Importe", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("-------------", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("-------------", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("-------------", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("-------------", espaciosBlanco, true));
                    importesBuilder.Append("\n\n\n");

                    foreach (var cheque in model.Pago.Cheques ?? new List<Cheque>())
                    {
                        detalleBuilder.Append("\n");
                        detalleBuilder.Append(AjustarString(cheque.NroCheque, espaciosBlanco, true));
                        detalleBuilder.Append(AjustarString(cheque.Banco, espaciosBlanco, true));
                        detalleBuilder.Append(AjustarString(cheque.FechaPago.ToShortDateString(), espaciosBlanco, true));
                        detalleBuilder.Append(AjustarString(cheque.Importe.ToString("F2", CultureInfo.InvariantCulture), 12, false));
                        importesBuilder.Append("\n");
                        importesCheque += cheque.Importe;
                    }

                    detalleBuilder.Append(AjustarString("             ", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("             ", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("             ", espaciosBlanco, true));
                    detalleBuilder.Append(AjustarString("Total Cheques", espaciosBlanco, true));
                    importesBuilder.Append(importesCheque.ToString("F2", CultureInfo.InvariantCulture));

                    if (!string.IsNullOrWhiteSpace(model.Pago.Observaciones))
                        detalleBuilder.Append("\n\n___________________\nObs.: " + model.Pago.Observaciones);

                    detallePago = detalleBuilder.ToString();
                    importesDetalle = importesBuilder.ToString();
                }

                PdfPTable tablaValores = new PdfPTable(3);
                tablaValores.WidthPercentage = 100;
                tablaValores.SetWidths(new float[] { 20, 60, 20 });
                foreach (var h in new[] { "Forma Pago", "Detalle", "Importe" })
                {
                    var celda = new PdfPCell(new Phrase(h, fuenteNegrita));
                    celda.BackgroundColor = new BaseColor(255, 200, 200);
                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    tablaValores.AddCell(celda);
                }

                tablaValores.AddCell(CeldaCentrada(model.Pago.FormaPago ?? "", fuenteNormal));
                tablaValores.AddCell(new PdfPCell(new Phrase(detallePago, (model.Pago.FormaPago ?? "").IndexOf("Cheque", StringComparison.OrdinalIgnoreCase) >= 0 ? fuenteMono : fuenteNormal)) { HorizontalAlignment = Element.ALIGN_LEFT });
                tablaValores.AddCell(CeldaDerecha(importesDetalle, fuenteNormal));
                doc.Add(tablaValores);

                PdfPTable tablaTotal = new PdfPTable(6);
                tablaTotal.WidthPercentage = 100;
                tablaTotal.SetWidths(new float[] { 12, 16, 10, 10, 26, 26 });
                for (int i = 0; i < 5; i++)
                    tablaTotal.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
                PdfPCell celdaTotal = new PdfPCell(new Phrase("Total: $ " + model.Pago.Importe.ToString("#,##0.00", new CultureInfo("es-AR")), fuenteNegrita));
                celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaTotal.Border = Rectangle.TOP_BORDER;
                tablaTotal.AddCell(celdaTotal);
                doc.Add(tablaTotal);

                if (model.TieneSaldo)
                {
                    PdfPTable tablaSaldo = new PdfPTable(6);
                    tablaSaldo.WidthPercentage = 100;
                    tablaSaldo.SetWidths(new float[] { 12, 16, 10, 10, 26, 26 });
                    for (int i = 0; i < 5; i++)
                        tablaSaldo.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
                    PdfPCell celdaSaldo = new PdfPCell(new Phrase("[ Saldo: $ " + model.Saldo.ToString("N2") + " ]", fuenteNormal));
                    celdaSaldo.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaSaldo.Border = Rectangle.NO_BORDER;
                    tablaSaldo.AddCell(celdaSaldo);
                    doc.Add(tablaSaldo);
                }

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(model.TipoOperacion.ToUpperInvariant() + " - " + model.DetalleOperacion, fuenteNegrita));

                if (model.Pago.Sucursal != null)
                    doc.Add(new Paragraph("Sucursal: " + (model.Pago.Sucursal.SucursalNombre ?? ""), fuenteNormal));
                if (model.Pago.CreadoPor != null)
                    doc.Add(new Paragraph("Usuario: " + (model.Pago.CreadoPor.Nombre ?? ""), fuenteNormal));

                doc.Close();
                return ms.ToArray();
            }
        }

        private static string AjustarString(string texto, int espacios, bool izquierda)
        {
            texto = texto ?? "";
            if (texto.Length > espacios)
                texto = texto.Substring(0, espacios);
            return izquierda ? texto.PadRight(espacios) : texto.PadLeft(espacios);
        }

        private List<string> ConstruirLineasTicketPago(ReciboPagoVm model, int ticketMm)
        {
            int cantMaxChar = ticketMm == 58 ? 32 : 48;
            string negocio = model.Empresa != null
                ? (model.Empresa.NombreFantasia ?? model.Empresa.RazonSocialAfip ?? "CarniSys")
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
                izquierda = truncar(izquierda, ancho);
                derecha = truncar(derecha, ancho);
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

            var sb = new StringBuilder();
            Action<string> linea = texto => sb.AppendLine(texto ?? "");

            linea(centrar("RECIBO " + (model.TipoOperacion ?? "").ToUpperInvariant(), cantMaxChar));
            linea(centrar("X", cantMaxChar));
            linea(centrar(negocio, cantMaxChar));
            linea(truncar(model.Empresa != null ? model.Empresa.RazonSocialAfip : "", cantMaxChar));
            linea(truncar("CUIT: " + (model.Empresa != null ? model.Empresa.Cuit.ToString() : ""), cantMaxChar));
            linea(truncar("IIBB: " + (model.Empresa != null ? model.Empresa.Iibb.ToString() : ""), cantMaxChar));
            linea(truncar((model.Empresa != null ? model.Empresa.Domicilio : "") + " " + (model.Empresa != null ? model.Empresa.Ciudad : ""), cantMaxChar));
            linea(new string('-', cantMaxChar));
            linea(truncar("Nro. Recibo: " + (model.Pago.NroRecibo ?? ""), cantMaxChar));
            linea(alinearExtremos("Fecha: " + model.Pago.Fecha.ToString("dd/MM/yyyy"), "Hora: " + model.Pago.Fecha.ToString("HH:mm"), cantMaxChar));
            linea(truncar((model.PersonaEtiqueta ?? "Persona") + ": " + (model.Pago.Persona != null ? model.Pago.Persona.RazonSocial : ""), cantMaxChar));

            if (model.Pago.Persona != null && !string.IsNullOrWhiteSpace(model.Pago.Persona.Cuit))
                linea(truncar("CUIT: " + model.Pago.Persona.Cuit, cantMaxChar));

            if (model.Pago.Persona != null && !string.IsNullOrWhiteSpace(model.Pago.Persona.Iva))
                linea(truncar("Cond. IVA: " + model.Pago.Persona.Iva, cantMaxChar));

            if (model.Pago.Persona != null && !string.IsNullOrWhiteSpace(model.Pago.Persona.Domicilio))
                linea(truncar("Domicilio: " + model.Pago.Persona.Domicilio, cantMaxChar));

            linea(new string('-', cantMaxChar));
            linea(truncar("Forma Pago: " + (model.Pago.FormaPago ?? ""), cantMaxChar));
            linea(truncar(model.DetalleOperacion ?? "", cantMaxChar));
            linea(formatearTotal("Importe", Convert.ToDecimal(model.Pago.Importe), cantMaxChar));

            if (model.Pago.Cheques != null && model.Pago.Cheques.Count > 0)
            {
                linea(" ");
                linea(truncar("Cheques:", cantMaxChar));
                foreach (var cheque in model.Pago.Cheques)
                {
                    linea(truncar((cheque.NroCheque ?? "") + " " + (cheque.Banco ?? ""), cantMaxChar));
                    linea(alinearExtremos("F.Pago: " + cheque.FechaPago.ToString("dd/MM/yy"), cheque.Importe.ToString("N2"), cantMaxChar));
                }

                if (Convert.ToDecimal(model.Pago.Efectivo) > 0)
                {
                    linea("-");
                    linea(formatearTotal("Efectivo:", Convert.ToDecimal(model.Pago.Efectivo), cantMaxChar));
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Pago.Observaciones))
            {
                linea(" ");
                linea(truncar("Observaciones:", cantMaxChar));
                string observacion = model.Pago.Observaciones ?? "";
                for (int i = 0; i < observacion.Length; i += cantMaxChar)
                    linea(observacion.Substring(i, Math.Min(cantMaxChar, observacion.Length - i)));
            }

            if (model.TieneSaldo)
            {
                linea(" ");
                linea(formatearTotal("Saldo", model.Saldo, cantMaxChar));
            }

            linea(" ");
            if (model.Pago.Sucursal != null)
                linea(truncar("Sucursal: " + (model.Pago.Sucursal.SucursalNombre ?? ""), cantMaxChar));
            if (model.Pago.CreadoPor != null)
                linea(truncar("Usuario: " + (model.Pago.CreadoPor.Nombre ?? ""), cantMaxChar));
            linea(" ");
            linea("Firma:");

            return sb.ToString()
                .Replace("\r\n", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.None)
                .ToList();
        }

        private PdfPCell CeldaSimple(string texto, iTextSharp.text.Font fuente, int alineacion = Element.ALIGN_LEFT)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto ?? "", fuente));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 4f;
            return celda;
        }

        private PdfPCell CeldaCentrada(string texto, iTextSharp.text.Font fuente)
        {
            return new PdfPCell(new Phrase(texto ?? "", fuente))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
        }

        private PdfPCell CeldaDerecha(string texto, iTextSharp.text.Font fuente)
        {
            return new PdfPCell(new Phrase(texto ?? "", fuente))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 5
            };
        }

        private string ConvertirTextoAHtmlPago(string texto)
        {
            string safe = HttpUtility.HtmlEncode(texto ?? "");
            safe = safe.Replace("\r\n", "\n").Replace("\r", "\n");
            string cuerpoHtml = "<p>" + safe.Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
            string pieHtml =
                "<div style=\"margin-top:24px; padding-top:12px; border-top:1px solid #ddd; font-size:11px; color:#777; line-height:1.4;\">" +
                "<p>CarniSys es un software de gestión comercial para pequeños y medianos comercios, diseñado para administrar ventas, stock y facturación, con integración a balanzas para agilizar la atención en productos pesables.</p>" +
                "</div>";

            return cuerpoHtml + pieHtml;
        }


        [HttpGet]
        public JsonResult BuscarChequePorNro(string numero, int pagoId = 0, bool esAProveedor = true, string chequesJson = "")
        {
            try
            {
                var pagoActual = oCtaCteN.getPagoById(pagoId);

                if (pagoActual == null)
                    pagoActual = new Pago { Cheques = new List<Cheque>() };

                if (pagoActual.Cheques == null)
                    pagoActual.Cheques = new List<Cheque>();

                if (!string.IsNullOrWhiteSpace(chequesJson))
                {
                    var chequesActuales = JsonConvert.DeserializeObject<List<Cheque>>(chequesJson) ?? new List<Cheque>();

                    foreach (var chequeActual in chequesActuales)
                    {
                        if (chequeActual == null)
                            continue;

                        bool yaExiste = pagoActual.Cheques.Any(c =>
                            c != null &&
                            (
                                (chequeActual.Id > 0 && c.Id == chequeActual.Id) ||
                                (!string.IsNullOrWhiteSpace(chequeActual.NroCheque) &&
                                 !string.IsNullOrWhiteSpace(c.NroCheque) &&
                                 c.NroCheque.Equals(chequeActual.NroCheque, StringComparison.OrdinalIgnoreCase))
                            ));

                        if (!yaExiste)
                            pagoActual.Cheques.Add(chequeActual);
                    }
                }

                var (ok, mensaje, cheque) = oCtaCteN.ValidarChequeParaPago(numero, pagoActual, esAProveedor);

                return Json(new
                {
                    ok,
                    mensaje,
                    cheque = ok ? new
                    {
                        cheque.Id,
                        cheque.NroCheque,
                        cheque.Banco,
                        FechaPago = cheque.FechaPago.ToString("yyyy-MM-dd"),
                        //cheque.FechaPago,
                        cheque.Importe
                    } : null
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult GetCheques(string estado, string nroCheque, string desde)
        {
            // fecha desde: si viene vacía
            DateTime fechaDesde = DateTime.Today.AddMonths(-1);

            if (!string.IsNullOrEmpty(desde))
                fechaDesde = DateTime.Parse(desde);

            // fecha hasta: incluimos todos los cheques hacia adelante
            DateTime fechaHasta = DateTime.Today.AddYears(1);

            // tu filtro por descripción (número cheque)
            string descripcion = nroCheque ?? "";

            // propio = false porque no lo pediste desde el modal (lo podés agregar si querés)
            bool propio = false;

            // 👉 ESTE método devuelve un DataTable
            DataTable dt = oCtaCteN.obtenerCheques(descripcion, fechaDesde, fechaHasta, propio, estado);


            //List<Entidades.Cheque> listaCheques = new List<Entidades.Cheque>();
            List<object> listaCheques = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                int recibidoDe = row["recibidoDe"] == DBNull.Value ? 0 : Convert.ToInt32(row["recibidoDe"]);
                int entregadoA = row["entregadoA"] == DBNull.Value ? 0 : Convert.ToInt32(row["entregadoA"]);
                string estadoDb = row["estado"]?.ToString() ?? "";
                string estadoVista = estadoDb;

                if (string.Equals(estadoDb, Entidades.Cheque.EstadoEnum.PENDIENTE.ToString(), StringComparison.OrdinalIgnoreCase)
                    && entregadoA > 0)
                {
                    estadoVista = Entidades.Cheque.EstadoEnum.ENTREGADO.ToString();
                }

                listaCheques.Add(new
                {
                    Id = row["id"]?.ToString(),
                    Propio = row["propio"]?.ToString(),
                    Origen = row["Origen"]?.ToString(),
                    NroCheque = row["nroCheque"]?.ToString(),
                    Banco = row["banco"]?.ToString(),
                    Importe = Convert.ToDouble(row["importe"] ?? 0),
                    FechaEmision = row["fechaEmision"]?.ToString() ?? "",
                    FechaPago = row["fechaPago"] == DBNull.Value ? "" :
                                Convert.ToDateTime(row["fechaPago"]).ToString("yyyy-MM-dd"),
                    Estado = estadoVista,
                    EstadoDb = estadoDb,
                    Titular = row["titular"]?.ToString(),
                    RecibidoDe = recibidoDe,
                    RecibidoDeNombre = row["Recibido_De"]?.ToString(),
                    EntregadoA = entregadoA,
                    EntregadoANombre = row["Entregado_A"]?.ToString(),
                    Observaciones = row["observaciones"]?.ToString(),
                    ObservacionesCorta = row["obs."]?.ToString(),
                    Creado = row["creado"] == DBNull.Value ? "" :
                                Convert.ToDateTime(row["creado"]).ToString("yyyy-MM-dd HH:mm"),
                    CreadoPor = row["CreadoPor"]?.ToString(),
                    Actualizado = row["actualizado"] == DBNull.Value ? "" :
                                Convert.ToDateTime(row["actualizado"]).ToString("yyyy-MM-dd HH:mm"),
                    ActualizadoPor = row["ActualizadoPor"]?.ToString()
                });
            }

            return Json(listaCheques, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCheque(int id)
        {
            try
            {
                var cheque = oCtaCteN.getChequePorIDorNro(id, "");

                if (cheque == null || cheque.Id <= 0)
                {
                    Response.StatusCode = 404;
                    return Json(new { ok = false, mensaje = "Cheque no encontrado." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    ok = true,
                    cheque = new
                    {
                        cheque.Id,
                        cheque.NroCheque,
                        cheque.Banco,
                        cheque.Propio,
                        cheque.FechaEmision,
                        FechaPago = cheque.FechaPago.ToString("yyyy-MM-dd"),
                        cheque.Importe,
                        cheque.Estado,
                        cheque.Titular,
                        Observaciones = cheque.Observaciones ?? ""
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(",", "."); // unifica formato

            float result;
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }
        [HttpPost]
        public JsonResult GuardarCheque(Cheque cheque, string importe, bool? esAProveedor = null)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, message = "No se encontró un usuario activo." });

                if (cheque == null)
                    return Json(new { ok = false, message = "No se recibieron los datos del cheque." });

                //busco el creador de cheque si id > 0
                int idUserCreador = cheque.Id > 0 ? (oCtaCteN.getChequePorIDorNro(cheque.Id, "").CreadoPor.Id) : user.Id;

                if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCheques, null,idUserCreador))
                {
                    ViewBag.Seccion = "Cheques";
                    //return View("~/Views/Shared/AccesoDenegado.cshtml");
                    return Json(new { success = false, message = "No tienes permisos para esta acción." });
                }

                if (string.IsNullOrWhiteSpace(cheque.Banco))
                    return Json(new { ok = false, message = "El Banco ingresado no es válido." });

                if (string.IsNullOrWhiteSpace(cheque.NroCheque))
                    return Json(new { ok = false, message = "Debe ingresar el número de cheque." });

                float importeCheque = !string.IsNullOrWhiteSpace(importe)
                    ? ParseFloat(importe)
                    : (float)cheque.Importe;

                if (importeCheque <= 0)
                    return Json(new { ok = false, message = "El importe ingresado no es válido." });

                if (cheque.FechaPago == DateTime.MinValue)
                    return Json(new { ok = false, message = "Debe ingresar la fecha de pago." });

                if (esAProveedor.HasValue && !esAProveedor.Value && cheque.Propio)
                    return Json(new { ok = false, message = "No se puede registrar un Cobro con un cheque propio." });

                cheque.NroCheque = cheque.NroCheque.Trim();
                cheque.Banco = cheque.Banco.Trim();
                cheque.Titular = cheque.Titular == null ? "" : cheque.Titular.Trim();
                cheque.Observaciones = cheque.Observaciones == null ? "" : cheque.Observaciones.Trim();
                cheque.Importe = importeCheque;

                if (cheque.Id == 0)
                {
                    cheque.Creado = DateTime.Now;
                    cheque.CreadoPor = user;
                }
                else
                {
                    cheque.Actualizado = DateTime.Now;
                    cheque.ActualizadoPor = user;
                }

                oCtaCteN.AddOrEditCheque(cheque);

                var chequeGuardado = cheque.Id > 0
                    ? oCtaCteN.getChequePorIDorNro(cheque.Id, "")
                    : oCtaCteN.getChequePorIDorNro(0, cheque.NroCheque);

                return Json(new
                {
                    ok = true,
                    cheque = chequeGuardado == null ? null : new
                    {
                        chequeGuardado.Id,
                        chequeGuardado.NroCheque,
                        chequeGuardado.Banco,
                        chequeGuardado.Propio,
                        chequeGuardado.FechaEmision,
                        FechaPago = chequeGuardado.FechaPago.ToString("yyyy-MM-dd"),
                        chequeGuardado.Importe,
                        chequeGuardado.Estado,
                        chequeGuardado.Titular,
                        Observaciones = chequeGuardado.Observaciones ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        /// <summary>
        /// validarCheque si se quiere validar para la carga. Se pasa false al cargar la pagina xq valida a si mismo
        /// y tira error
        /// </summary>
        /// <param name="numero"></param>
        /// <param name="pagoId"></param>
        /// <param name="esAProveedor"></param>
        /// <param name="validarCheque"></param>
        /// <returns></returns>
        public JsonResult ValidarChequeParaPago(string numero, int pagoId = 0, bool esAProveedor = true, bool validarCheque = false)
        {
            try
            {
                var pagoActual = oCtaCteN.getPagoById(pagoId);

                if (pagoActual == null)
                    pagoActual = new Pago { Cheques = new List<Cheque>() };

                if (pagoActual.Cheques == null)
                    pagoActual.Cheques = new List<Cheque>();

                var (ok, mensaje, cheque) = validarCheque ? 
                        oCtaCteN.ValidarChequeParaPago(numero, pagoActual, esAProveedor) :
                        (true, "", null);

                return Json(new
                {
                    ok,
                    mensaje,
                    cheque = ok && cheque != null ? new
                    {
                        cheque.Id,
                        cheque.NroCheque,
                        cheque.Banco,
                        FechaPago = cheque.FechaPago.ToString("yyyy-MM-dd"),
                        cheque.Importe
                    } : null

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
    
}

