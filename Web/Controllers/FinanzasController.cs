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
using Datos;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;

namespace Web.Controllers
{
    public class FinanzasController : BaseController
    {
        private Negocio.CuentaCorriente oCtaCteN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Persona oPersonasN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            oCtaCteN = new Negocio.CuentaCorriente(empresa, param);
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


        // ============================================================
        // GET: /Finanzas/CtasCtes
        // ============================================================

        public ActionResult CtasCtes(string buscar = "", string ordenSaldo = "DESC", bool desdePos = false)
        {
            try
            {
                bool modoPos = desdePos || DesdePOS;

                if (!modoPos)
                {
                    var user = Session["Usuario"] as Entidades.Usuario;
                    if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCtasCtes, null))
                    {
                        ViewBag.Seccion = "Cuenta Corriente";
                        return View("~/Views/Shared/AccesoDenegado.cshtml");
                    }
                }

                ViewBag.Buscar = buscar;
                ViewBag.DesdePOS = desdePos;
                ViewBag.OrdenSaldo = ordenSaldo;

                DataTable dt = oCtaCteN.obtenerCtasCtes(buscar, null);

                DataView dv = dt.DefaultView;
                dv.Sort = $"Saldo {ordenSaldo}";
                dt = dv.ToTable();

                if (desdePos)
                    return PartialView("CtasCtes", dt);

                return View("CtasCtes", dt);
            }
            catch (Exception ex)
            {
                if (desdePos)
                    return Content("<div class='alert alert-danger m-3'>Error: " + HttpUtility.HtmlEncode(ex.Message) + "</div>");

                return Content("Error: " + ex.Message);
            }
        }
        // GET: Finanzas/CtaCtePersona
        public ActionResult CtaCtePersona(int idPersona, DateTime? fechaDesde, bool mostrarAnulados = false, bool desdePos = false)
        {
            try
            {
                bool modoPos = desdePos || DesdePOS;

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
                ViewBag.FechaDesde = fechaDesde.Value.ToString("yyyy-MM-dd");
                ViewBag.MostrarAnulados = mostrarAnulados;
                ViewBag.DesdePOS = desdePos;

                if (desdePos)
                    return PartialView("CtaCtePersona", dtMov);

                return View(dtMov);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar cuenta corriente: " + ex.Message;
                ViewBag.DesdePOS = desdePos;
                ViewBag.MostrarAnulados = mostrarAnulados;

                if (desdePos)
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

            // Trabajamos sobre una copia para no tocar la original inesperadamente
            DataTable dt = dtMov.Copy();

            int[] aBorrar = new int[dt.Rows.Count];
            for (int i = 0; i < aBorrar.Length; i++)
            {
                aBorrar[i] = -1;
            }

            for (int filaPrimer = 0; filaPrimer < dt.Rows.Count; filaPrimer++)
            {
                if (dt.Rows[filaPrimer].RowState == DataRowState.Deleted)
                    continue;

                for (int fila = 0; fila < dt.Rows.Count; fila++)
                {
                    if (dt.Rows[fila].RowState == DataRowState.Deleted)
                        continue;

                    if (aBorrar[filaPrimer] == 1)
                        break;

                    string tablaPrimer = dt.Rows[filaPrimer]["tabla"].ToString();
                    string idtablaPrimer = dt.Rows[filaPrimer]["idTabla"].ToString();
                    string sucursalPrimer = dt.Rows[filaPrimer]["sucursal"].ToString();
                    int idPrimer = Convert.ToInt32(dt.Rows[filaPrimer]["id"].ToString());

                    string tabla = dt.Rows[fila]["tabla"].ToString();
                    string idtabla = dt.Rows[fila]["idTabla"].ToString();
                    string sucursal = dt.Rows[fila]["sucursal"].ToString();
                    int id = Convert.ToInt32(dt.Rows[fila]["id"].ToString());

                    if (tabla.Equals(tablaPrimer) &&
                        idtabla.Equals(idtablaPrimer) &&
                        sucursal.Equals(sucursalPrimer) &&
                        id < idPrimer)
                    {
                        aBorrar[fila] = 1;
                    }
                }
            }

            for (int i = 0; i < aBorrar.Length; i++)
            {
                if (aBorrar[i] == 1)
                    dt.Rows[i].Delete();
            }

            dt.AcceptChanges();
            return dt;
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
        public ActionResult ExportarExcelPersona(int idPersona, string fechaDesde)
        {
            DateTime fecha = DateTime.ParseExact(fechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            DataTable dt = oCtaCteN.getCtaCteByIdPersona(idPersona, fecha);

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

            return File(buffer, "text/csv", "CuentaCorriente.csv");
        }


        // ============================================================
        //  EXPORTAR A PDF
        // ============================================================
        public ActionResult ExportarPdfPersona(int idPersona, string fechaDesde)
        {
            DateTime fechaD;
            if (!DateTime.TryParse(fechaDesde, out fechaD))
                fechaD = DateTime.Today.AddMonths(-1);

            DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaD);

            // Obtener persona y saldo igual que en la vista
            string persona = "";

            if (dtMov.Rows.Count > 0)
            {
                persona = dtMov.Rows[0]["razonSocial"].ToString();
            }

            byte[] pdfBytes = Utilidades.GenerarDocs.GenerarPdfCtaCtePersona(dtMov, fechaD); // GenerarPdfPersona(dtMov, persona, saldo, fechaD);

            return File(pdfBytes, "application/pdf", $"{persona}_CuentaCorriente.pdf");
        }


        // ============================================================
        //  DETALLE DE MOVIMIENTO (placeholder)
        // ============================================================
        public ActionResult verMovimientoCtaCte(int idPersona, string returnUrl, string tabla, int idTabla = 0)
        {
            Entidades.MovCtaCte oMovCtaCteE = new Entidades.MovCtaCte();
            Entidades.MovCtaCte.tablas tablaEnum = oMovCtaCteE.getTablaEnum(tabla);
            string decodedReturnUrl = "";

            if (!string.IsNullOrEmpty(returnUrl))
            {
                decodedReturnUrl = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(returnUrl)
                );
            }

            switch (tablaEnum)
            {
                case Entidades.MovCtaCte.tablas.Ventas:
                    return RedirectToAction("InfoVenta", "Finanzas", new { id = idTabla });

                case Entidades.MovCtaCte.tablas.Compras:
                    return RedirectToAction("ModificarCompra", "Compras", new { id = idTabla });

                case Entidades.MovCtaCte.tablas.Pagos:
                    return RedirectToAction("AddOrEditPago", "Finanzas",
                        new { idPersona = idPersona, returnUrl = decodedReturnUrl, idPago = idTabla });

                default:
                    return HttpNotFound();
            }
        }

        // ============================================================
        //  AGREGAR COBRO / PAGO (placeholder)
        // ============================================================

        public ActionResult AddOrEditPago(int idPersona, string returnUrl, int idPago = 0, bool desdePos = false)
        {
            var sucursales = oSucursalN.findAll();

            ViewBag.Sucursales = sucursales;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.DesdePOS = desdePos;

            if (!string.IsNullOrEmpty(returnUrl) && idPago == 0)
            {
                ViewBag.ReturnUrl = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(returnUrl)
                );
            }

            DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, DateTime.Today);

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
            ViewBag.Bancos = oCtaCteN.getBancos();

            Pago model;

            if (idPago == 0)
            {
                model = new Pago();
                model.Fecha = DateTime.Now;
                model.AProveedor = true;

                var user = Session["Usuario"] as Entidades.Usuario;
                model.Sucursal = user.Sucursal;
            }
            else
            {
                model = oCtaCteN.getPagoById(idPago);
                if (model == null)
                    return HttpNotFound();
            }

            if (desdePos)
                return PartialView("AddOrEditPago", model);

            return View(model);
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

            oPagoE.CreadoPor = oPagoE.Id > 0
                ? oCtaCteN.getPagoById(oPagoE.Id).CreadoPor
                : Session["Usuario"] as Entidades.Usuario;

            oPagoE.ActualizadoPor = oPagoE.Id > 0
                ? Session["Usuario"] as Entidades.Usuario
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
            _ = oCtaCteN.addOrEditPago(oPagoE, null, null);

            if (desdePos)
            {
                string urlRetornoPos = !string.IsNullOrEmpty(returnUrl)
                    ? returnUrl
                    : Url.Action("CtaCtePersona", "Finanzas", new
                    {
                        idPersona = idPersona,
                        fechaDesde = DateTime.Today.ToString("yyyy-MM-dd"),
                        desdePos = true
                    });

                return Json(new
                {
                    ok = true,
                    redirectUrl = urlRetornoPos,
                    cerrarModalPago = true
                });
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Json(new { ok = true, redirectUrl = returnUrl });

            if (Request.IsAjaxRequest())
                return Json(new { ok = true, redirectUrl = Url.Action("CtasCtes") });

            return RedirectToAction("CtasCtes");
        }


        [HttpGet]
        public JsonResult BuscarChequePorNro(string numero, int pagoId = 0, bool esAProveedor = true)
        {
            try
            {
                var pagoActual = oCtaCteN.getPagoById(pagoId);

                if (pagoActual == null)
                    pagoActual = new Pago { Cheques = new List<Cheque>() };

                if (pagoActual.Cheques == null)
                    pagoActual.Cheques = new List<Cheque>();

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
                listaCheques.Add(new
                {
                    Id = row["id"]?.ToString(),
                    Propio = row["propio"]?.ToString(),
                    Origen = row["Origen"]?.ToString(),
                    NroCheque = row["nroCheque"]?.ToString(),
                    Banco = row["banco"]?.ToString(),
                    Importe = Convert.ToDouble(row["importe"] ?? 0),
                    FechaPago = row["fechaPago"] == DBNull.Value ? "" :
                                Convert.ToDateTime(row["fechaPago"]).ToString("yyyy-MM-dd"),
                    Estado = row["estado"]?.ToString()
                });
            }

            return Json(listaCheques, JsonRequestBehavior.AllowGet);
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
        public JsonResult GuardarCheque(Cheque cheque, string importe)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                //busco el creador de cheque si id > 0
                int idUserCreador = cheque.Id > 0 ? (oCtaCteN.getChequePorIDorNro(cheque.Id, "").CreadoPor.Id) : user.Id;

                if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCheques, null,idUserCreador))
                {
                    ViewBag.Seccion = "Cheques";
                    //return View("~/Views/Shared/AccesoDenegado.cshtml");
                    return Json(new { success = false, message = "No tienes permisos para esta acción." });
                }

                cheque.Importe = ParseFloat(importe);

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

                return Json(new { ok = true });
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

