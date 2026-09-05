// Port de Web/Controllers/FinanzasController.cs (ver docs/DECISIONS.md, migracion ASP.NET
// Core, Modulo 7 -- Caja y tesoreria). El original tiene 1941 lineas. Portado: CtasCtes (listado),
// Cheques (pantalla + CRUD completo), CtaCtePersona (extracto de cuenta corriente de una persona)
// con exportacion a PDF/Excel/email real (2026-09-03), y AddOrEditPago/AddOrEditPagoPost (alta y
// edicion de pagos/cobros en cuenta corriente) + ImprimirPdfPago/ObtenerDatosEmailPago/
// EnviarComprobantePagoEmail (2026-09-04, ver docs/10-migracion-aspnet-core/README.md) --
// verificado de punta a punta con datos reales (pago real creado y editado, PDF generado, email
// preparado). verMovimientoCtaCte solo cubre la rama "Ventas" (redirige a Ventas/DetalleVenta) y
// ahora tambien "Pagos" (redirige a AddOrEditPago); la rama "Compras" sigue sin portar
// (ModificarCompra de Compras, fuera de alcance de este controller).
//
// AddOrEditPago: modo POS (2026-09-05) y pago con Cheque/EftvoCheque (2026-09-05) ya portados,
// ver docs/DECISIONS.md. Sin piezas deferred pendientes en este controller.
//
// Mismo stub que el resto de la migracion. PermisosHelper.TienePermiso(Session, ...) se omite
// (bypass de Admin=true) salvo donde el original ya usa un chequeo real independiente de Session
// (ej. "usuario.Admin" en PuedeVerSaldosCuentaCorriente, que se preserva).
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;
using WebCore.Services;

namespace WebCore.Controllers
{
    public class FinanzasController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;
        private readonly Negocio.CuentaCorriente _oCtaCteN;
        private readonly Negocio.Persona _oPersonaN;
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

        public FinanzasController()
        {
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oCtaCteN = WebCore.Infrastructure.NegocioFactory.CrearCuentaCorriente(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _param);
        }

        // Port de FinanzasController.ObtenerCajaAbiertaUsuario (Web/Controllers/FinanzasController.cs:1215)
        // -- misma logica exacta, adaptada al stub _usuarioActual en vez de Session["Usuario"]
        // (WebCore no tiene sesion real, ver docs/DECISIONS.md). Mismo patron ya usado y verificado
        // en VentasController/CajasController para resolver "la caja abierta del vendedor actual".
        private Entidades.CierreCaja ObtenerCajaAbiertaUsuario(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null || user.Sucursal.IdSucursal != user.IdSucursal)
                user.Sucursal = _oSucursalN.findById(user.IdSucursal);

            if (user.Sucursal == null || user.Sucursal.IdSucursal == 0)
                return null;

            var cierre = new Entidades.CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = _oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");

            bool abierta = cierre != null && !cierre.FechaHoraCierre.HasValue;
            return abierta ? cierre : null;
        }

        private bool EsPeticionAjax()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public IActionResult CtasCtes(string buscar = "", string ordenSaldo = "DESC")
        {
            bool renderParcial = EsPeticionAjax();

            ordenSaldo = string.Equals(ordenSaldo, "ASC", StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : "DESC";

            ViewBag.Buscar = buscar;
            ViewBag.DesdePOS = false;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.OrdenSaldo = ordenSaldo;
            ViewBag.OcultarSaldo = false;

            DataTable dt = _oCtaCteN.obtenerCtasCtes(buscar, null, ordenSaldo);

            ViewBag.Title = "Cuentas Corrientes";
            if (renderParcial)
                return PartialView("~/Views/Finanzas/CtasCtes.cshtml", dt);

            return View("~/Views/Finanzas/CtasCtes.cshtml", dt);
        }

        [HttpGet]
        public IActionResult Cheques(string estado = "", string nroCheque = "", string desde = "")
        {
            bool renderParcial = EsPeticionAjax();

            DateTime fechaDesde = DateTime.Today.AddMonths(-1);
            if (!string.IsNullOrWhiteSpace(desde))
            {
                DateTime.TryParse(desde, out fechaDesde);
                if (fechaDesde == DateTime.MinValue)
                    fechaDesde = DateTime.Today.AddMonths(-1);
            }

            ViewBag.DesdePOS = false;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.FiltroEstado = estado ?? "";
            ViewBag.FiltroNroCheque = nroCheque ?? "";
            ViewBag.FiltroDesde = fechaDesde.ToString("yyyy-MM-dd");
            ViewBag.Bancos = _oCtaCteN.getBancos();

            ViewBag.Title = "Cheques";
            if (renderParcial)
                return PartialView("~/Views/Finanzas/Cheques.cshtml");

            return View("~/Views/Finanzas/Cheques.cshtml");
        }

        [HttpGet]
        public IActionResult GetCheques(string estado, string nroCheque, string desde)
        {
            DateTime fechaDesde = DateTime.Today.AddMonths(-1);
            if (!string.IsNullOrEmpty(desde))
                DateTime.TryParse(desde, out fechaDesde);

            DateTime fechaHasta = DateTime.Today.AddYears(1);
            string descripcion = nroCheque ?? "";
            bool propio = false;

            DataTable dt = _oCtaCteN.obtenerCheques(descripcion, fechaDesde, fechaHasta, propio, estado);

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

            return Json(listaCheques);
        }

        [HttpGet]
        public IActionResult GetCheque(int id)
        {
            try
            {
                var cheque = _oCtaCteN.getChequePorIDorNro(id, "");

                if (cheque == null || cheque.Id <= 0)
                {
                    Response.StatusCode = 404;
                    return Json(new { ok = false, mensaje = "Cheque no encontrado." });
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
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GuardarCheque(Cheque cheque, string importe, bool? esAProveedor = null)
        {
            try
            {
                var user = _usuarioActual;

                if (cheque == null)
                    return Json(new { ok = false, message = "No se recibieron los datos del cheque." });

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

                _oCtaCteN.AddOrEditCheque(cheque);

                var chequeGuardado = cheque.Id > 0
                    ? _oCtaCteN.getChequePorIDorNro(cheque.Id, "")
                    : _oCtaCteN.getChequePorIDorNro(0, cheque.NroCheque);

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

        [HttpGet]
        public IActionResult BuscarChequePorNro(string numero, int pagoId = 0, bool esAProveedor = true, string chequesJson = "")
        {
            try
            {
                var pagoActual = pagoId > 0 ? _oCtaCteN.getPagoById(pagoId) : null;

                if (pagoActual == null)
                    pagoActual = new Pago { Cheques = new List<Cheque>() };

                if (pagoActual.Cheques == null)
                    pagoActual.Cheques = new List<Cheque>();

                if (!string.IsNullOrWhiteSpace(chequesJson))
                {
                    var chequesActuales = System.Text.Json.JsonSerializer.Deserialize<List<Cheque>>(chequesJson) ?? new List<Cheque>();

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

                var (ok, mensaje, cheque) = _oCtaCteN.ValidarChequeParaPago(numero, pagoActual, esAProveedor);

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
                        cheque.Importe
                    } : null
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ValidarChequeParaPago(string numero, int pagoId = 0, bool esAProveedor = true, bool validarCheque = false)
        {
            try
            {
                var pagoActual = pagoId > 0 ? _oCtaCteN.getPagoById(pagoId) : null;

                if (pagoActual == null)
                    pagoActual = new Pago { Cheques = new List<Cheque>() };

                if (pagoActual.Cheques == null)
                    pagoActual.Cheques = new List<Cheque>();

                var (ok, mensaje, cheque) = validarCheque
                    ? _oCtaCteN.ValidarChequeParaPago(numero, pagoActual, esAProveedor)
                    : (true, "", null);

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
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        private float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(",", ".");

            float result;
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }

        // ===== CtaCtePersona: extracto de una persona, con exportacion PDF/Excel/email real =====

        [HttpGet]
        public IActionResult CtaCtePersona(int idPersona, DateTime? fechaDesde, bool mostrarAnulados = false, string returnUrl = "")
        {
            bool renderParcial = EsPeticionAjax();

            if (!fechaDesde.HasValue)
                fechaDesde = DateTime.Now.Date;

            var persona = _oPersonaN.findById(idPersona);
            DataTable dtMov = _oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesde.Value);

            if (!mostrarAnulados)
                dtMov = FiltrarRegistrosRepetidos(dtMov);

            dtMov = OrdenarMovimientosCuentaCorrientePorFecha(dtMov, true);

            decimal saldo = 0;
            if (dtMov != null && dtMov.Rows.Count > 0)
            {
                DataRow ultimaFila = dtMov.Rows[dtMov.Rows.Count - 1];
                if (ultimaFila["Saldo"] != DBNull.Value)
                    saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
            }

            ViewBag.IdPersona = idPersona;
            ViewBag.Persona = persona;
            ViewBag.SaldoPersona = saldo;
            ViewBag.ReturnUrlCtaCte = DecodeReturnUrlIfNeeded(returnUrl);
            ViewBag.FechaDesde = fechaDesde.Value.ToString("yyyy-MM-dd");
            ViewBag.MostrarAnulados = mostrarAnulados;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.OcultarSaldo = false;
            ViewBag.PuedeExportarCuentaCorriente = true;
            ViewBag.Title = "Cuenta Corriente";

            if (renderParcial)
                return PartialView("~/Views/Finanzas/CtaCtePersona.cshtml", dtMov);

            return View("~/Views/Finanzas/CtaCtePersona.cshtml", dtMov);
        }

        [HttpGet]
        public IActionResult ExportarPdfPersona(int idPersona, string fechaDesde, bool mostrarAnulados = false)
        {
            DateTime fechaD;
            if (!DateTime.TryParse(fechaDesde, out fechaD))
                fechaD = DateTime.Today.AddMonths(-1);

            string fileName;
            byte[] pdfBytes = GenerarPdfCuentaCorrienteBytes(idPersona, fechaD, mostrarAnulados, out fileName);
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet]
        public IActionResult ExportarExcelPersona(int idPersona, string fechaDesde, bool mostrarAnulados = false)
        {
            DateTime fecha = ParsearFechaCuentaCorriente(fechaDesde);
            string fileName;
            byte[] buffer = GenerarExcelCuentaCorrienteBytes(idPersona, fecha, mostrarAnulados, out fileName);
            return File(buffer, "text/csv", fileName);
        }

        [HttpGet]
        public IActionResult ObtenerDatosEmailCuentaCorriente(int idPersona, string fechaDesde, bool mostrarAnulados = false)
        {
            try
            {
                DateTime fecha = ParsearFechaCuentaCorriente(fechaDesde);
                var persona = _oPersonaN.findById(idPersona);
                var empresaActual = _usuarioActual.Empresa;
                string nombreEmpresa = empresaActual != null
                    ? (!string.IsNullOrWhiteSpace(empresaActual.NombreFantasia) ? empresaActual.NombreFantasia : empresaActual.RazonSocialAfip)
                    : "";

                if (string.IsNullOrWhiteSpace(nombreEmpresa))
                    nombreEmpresa = "CarniSys";

                string razonSocial = persona != null ? (persona.RazonSocial ?? "").Trim() : "";
                string asunto = "Cuenta corriente de " + (!string.IsNullOrWhiteSpace(razonSocial) ? razonSocial : "cliente") + " - " + nombreEmpresa;
                string cuerpo =
                    "Hola" + (!string.IsNullOrWhiteSpace(razonSocial) ? " " + razonSocial : "") + ",\n\n" +
                    "Te enviamos adjunta la cuenta corriente solicitada.\n\n" +
                    "Fecha desde: " + fecha.ToString("dd/MM/yyyy") + "\n" +
                    "Incluye anulados: " + (mostrarAnulados ? "Sí" : "No") + "\n\n" +
                    "Saludos,\n" +
                    nombreEmpresa;

                return Json(new
                {
                    ok = true,
                    email = persona != null ? (persona.Email ?? "").Trim() : "",
                    asunto,
                    mensaje = cuerpo,
                    replyTo = empresaActual != null ? (empresaActual.Email ?? "") : ""
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EnviarCuentaCorrienteEmail(int idPersona, string fechaDesde, bool mostrarAnulados, string emailDestino, string asunto, string mensaje, string formato)
        {
            try
            {
                emailDestino = (emailDestino ?? "").Trim();
                asunto = (asunto ?? "").Trim();
                mensaje = (mensaje ?? "").Trim();
                string formatoNormalizado = (formato ?? "").Trim().ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email destino." });

                if (!SmtpMailHelper.IsValidEmail(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email válido." });

                if (string.IsNullOrWhiteSpace(asunto))
                    return Json(new { ok = false, msg = "Ingrese un asunto." });

                if (formatoNormalizado != "pdf" && formatoNormalizado != "excel")
                    return Json(new { ok = false, msg = "Seleccione un formato válido para adjuntar." });

                DateTime fecha = ParsearFechaCuentaCorriente(fechaDesde);
                var persona = _oPersonaN.findById(idPersona);
                var empresaActual = _usuarioActual.Empresa;
                string nombreEmpresa = empresaActual != null
                    ? (!string.IsNullOrWhiteSpace(empresaActual.NombreFantasia) ? empresaActual.NombreFantasia : empresaActual.RazonSocialAfip)
                    : "";

                if (string.IsNullOrWhiteSpace(nombreEmpresa))
                    nombreEmpresa = "CarniSys";

                string fileName;
                byte[] attachmentBytes;
                string contentType;

                if (formatoNormalizado == "excel")
                {
                    attachmentBytes = GenerarExcelCuentaCorrienteBytes(idPersona, fecha, mostrarAnulados, out fileName);
                    contentType = "text/csv";
                }
                else
                {
                    attachmentBytes = GenerarPdfCuentaCorrienteBytes(idPersona, fecha, mostrarAnulados, out fileName);
                    contentType = "application/pdf";
                }

                string fromName = "CarniSys - " + nombreEmpresa;
                string replyToEmail = empresaActual != null ? (empresaActual.Email ?? "").Trim() : "";

                SmtpMailHelper.SendMail(
                    toEmail: emailDestino,
                    toName: persona != null ? persona.RazonSocial : "",
                    subject: asunto,
                    bodyHtml: ConvertirTextoAHtmlPago(mensaje),
                    attachmentFileName: fileName,
                    attachmentBytes: attachmentBytes,
                    attachmentContentType: contentType,
                    fromNameOverride: fromName,
                    replyToEmail: SmtpMailHelper.IsValidEmail(replyToEmail) ? replyToEmail : null,
                    replyToName: nombreEmpresa
                );

                return Json(new { ok = true, msg = "La cuenta corriente se envió correctamente." });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo enviar el email. " + ex.Message });
            }
        }

        // ===== AddOrEditPago (alta/edicion de pago o cobro en cuenta corriente) =====
        // Ver docs/10-migracion-aspnet-core/PLAN-ADDOREDITPAGO.md. Port de Web/Controllers/
        // FinanzasController.cs:822-1282. Recortes deliberados confirmados con el usuario:
        //   1. Modo POS (desdePos, gate de caja abierta) portado 2026-09-05 -- ver
        //      ObtenerCajaAbiertaUsuario arriba. Se dejo afuera a proposito el TempData["DesdePOS"]
        //      del original (persistia el flag entre requests via redirect): WebCore es 100%
        //      stateless por querystring en todos sus controllers, y hoy no hay ningun caller real
        //      que dependa de ese carry-over (ningun boton de POS todavia linkea a AddOrEditPago).
        //      Si aparece un caso real, se agrega ahi, no antes.
        //   2. SIN "Cheque"/"EftvoCheque" como forma de pago -- el merge de cheques (ChequesJson,
        //      2 modales de busqueda/alta) es una pieza grande aparte; los cheques YA tienen su
        //      propio CRUD completo en WebCore (Finanzas/Cheques.cshtml), solo no se puede pagar
        //      CON un cheque desde este formulario todavia. Ver gaps.md.
        // Gap de permisos real encontrado en el original (Permisos.Finanza.AddOrEditPago nunca se
        // valida) -- documentado en gaps.md, no se corrige (bypass, mismo criterio de siempre).
        [HttpGet]
        public IActionResult AddOrEditPago(int idPersona, string returnUrl, int idPago = 0, bool desdePos = false)
        {
            bool renderParcial = desdePos || EsPeticionAjax();
            var sucursales = _oSucursalN.findAll();
            string returnUrlDecodificada = DecodeReturnUrlIfNeeded(returnUrl);
            Entidades.Pago pagoExistente = null;

            // _ModalAltaCheque.cshtml (incluido en esta vista desde 2026-09-05, pago con cheque)
            // necesita ViewBag.Bancos -- mismo dato que ya carga Cheques() para el mismo modal.
            ViewBag.Bancos = _oCtaCteN.getBancos();

            if (desdePos)
            {
                var cierreCajaActual = ObtenerCajaAbiertaUsuario(_usuarioActual);
                if (cierreCajaActual == null)
                    return StatusCode(403, "Debe tener una caja abierta en la sucursal activa para registrar pagos o cobros desde POS.");
            }

            if (idPago > 0)
            {
                pagoExistente = _oCtaCteN.getPagoById(idPago);
                if (pagoExistente == null)
                    return NotFound();

                if (idPersona <= 0 && pagoExistente.Persona != null)
                    idPersona = pagoExistente.Persona.IdPersona;
            }

            ViewBag.Sucursales = sucursales;
            ViewBag.ReturnUrl = returnUrlDecodificada;
            ViewBag.DesdePOS = desdePos;
            ViewBag.RenderSinLayout = renderParcial;

            DataTable dtMov = _oCtaCteN.getCtaCteByIdPersona(idPersona, DateTime.Today);
            decimal saldo = 0;
            if (dtMov != null && dtMov.Rows.Count > 0)
            {
                DataRow ultimaFila = dtMov.Rows[dtMov.Rows.Count - 1];
                if (ultimaFila["Saldo"] != DBNull.Value)
                    saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
            }

            var persona = _oPersonaN.findById(idPersona);
            if (persona == null)
                return NotFound();

            ViewBag.IdPersona = idPersona;
            ViewBag.Persona = persona;
            ViewBag.SaldoPersona = saldo;
            ViewBag.ImprimirPagoPdfUrl = idPago > 0 ? Url.Action("ImprimirPdfPago", "Finanzas", new { id = idPago }) : "";

            Entidades.Pago model;
            if (idPago == 0)
            {
                model = new Entidades.Pago
                {
                    Fecha = DateTime.Now,
                    Sucursal = _usuarioActual.Sucursal ?? _oSucursalN.findById(_usuarioActual.IdSucursal)
                };
                model.NroRecibo = model.Sucursal != null && model.Sucursal.idSucursal > 0
                    ? _oCtaCteN.getNroReciboAutomatico(model.Sucursal.idSucursal)
                    : "";
            }
            else
            {
                model = pagoExistente;
            }

            if (renderParcial)
                return PartialView("~/Views/Finanzas/AddOrEditPago.cshtml", model);

            return View("~/Views/Finanzas/AddOrEditPago.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrEditPagoPost(
            Entidades.Pago oPagoE,
            string returnUrl,
            int SucursalId = 0,
            int idPersona = 0,
            string importe = "",
            string Efectivo = "",
            string ChequesJson = "",
            bool desdePos = false)
        {
            returnUrl = DecodeReturnUrlIfNeeded(returnUrl);

            if (desdePos && !_usuarioActual.Admin)
                SucursalId = _usuarioActual.IdSucursal;

            oPagoE.Sucursal = _oSucursalN.findById(SucursalId);
            oPagoE.Persona = _oPersonaN.findById(idPersona);

            // Pago con Cheque/EftvoCheque (2026-09-05, ver docs/DECISIONS.md): el cliente manda
            // solo Id/NroCheque de cada fila ya agregada (ver pago-cheques.js:obtenerChequesActualesJson,
            // reusado tal cual del original) -- se vuelve a pedir cada cheque real por Id para no
            // confiar en datos que el navegador pudo alterar (mismo criterio que el original).
            oPagoE.Cheques = new List<Entidades.Cheque>();
            if (!string.IsNullOrWhiteSpace(ChequesJson))
            {
                var chequesCliente = System.Text.Json.JsonSerializer.Deserialize<List<Entidades.Cheque>>(ChequesJson) ?? new List<Entidades.Cheque>();
                foreach (var chequeCliente in chequesCliente)
                {
                    if (chequeCliente == null || chequeCliente.Id <= 0) continue;
                    var chequeReal = _oCtaCteN.getChequePorIDorNro(chequeCliente.Id, "");
                    if (chequeReal != null) oPagoE.Cheques.Add(chequeReal);
                }
            }

            oPagoE.Importe = ParseFloat(importe);
            oPagoE.Efectivo = string.IsNullOrEmpty(Efectivo) ? 0 : ParseFloat(Efectivo);
            oPagoE.Banco = "";
            oPagoE.NroCheque = "";
            oPagoE.TitularCheque = "";

            Entidades.Pago pagoAnterior = oPagoE.Id > 0 ? _oCtaCteN.getPagoById(oPagoE.Id) : null;
            if (oPagoE.Id > 0 && pagoAnterior == null)
                return Json(new { ok = false, mensaje = "No se encontró el pago o cobro a modificar." });

            oPagoE.CreadoPor = oPagoE.Id > 0 ? pagoAnterior.CreadoPor : _usuarioActual;
            oPagoE.ActualizadoPor = oPagoE.Id > 0 ? _usuarioActual : oPagoE.ActualizadoPor;
            oPagoE.FormaPago = oPagoE.FormaPago_.ToString();

            var (ok, mensaje) = _oCtaCteN.ValidarPago(oPagoE);
            if (!ok)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = string.IsNullOrWhiteSpace(mensaje) ? "No se pudo validar el pago." : mensaje
                });
            }

            // cierreCajaActual queda null fuera de modo POS -- mismo comportamiento que el
            // original (crearMovCtaCtePago no genera egreso de caja si no hay cierre asociado).
            Entidades.CierreCaja cierreCajaActual = null;
            if (desdePos)
            {
                cierreCajaActual = ObtenerCajaAbiertaUsuario(_usuarioActual);
                if (cierreCajaActual == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe tener una caja abierta en la sucursal activa para registrar pagos o cobros desde POS."
                    });
                }

                if (oPagoE.Sucursal == null || oPagoE.Sucursal.idSucursal != _usuarioActual.IdSucursal)
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

            _ = _oCtaCteN.addOrEditPago(oPagoE, cierreCajaActual, pagoAnterior);

            string urlRetornoDefault = !string.IsNullOrWhiteSpace(returnUrl)
                ? returnUrl
                : Url.Action("CtaCtePersona", "Finanzas", new { idPersona, fechaDesde = DateTime.Today.ToString("yyyy-MM-dd") });

            if (desdePos)
            {
                return Json(new
                {
                    ok = true,
                    redirectUrl = urlRetornoDefault,
                    cerrarModalPago = true,
                    pagoId = oPagoE.Id,
                    pdfUrl = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id }),
                    emailConfigUrl = Url.Action("ObtenerDatosEmailPago", "Finanzas"),
                    emailSendUrl = Url.Action("EnviarComprobantePagoEmail", "Finanzas")
                });
            }

            return Json(new
            {
                ok = true,
                redirectUrl = urlRetornoDefault,
                pagoId = oPagoE.Id,
                pdfUrl = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id }),
                emailConfigUrl = Url.Action("ObtenerDatosEmailPago", "Finanzas"),
                emailSendUrl = Url.Action("EnviarComprobantePagoEmail", "Finanzas")
            });
        }

        [HttpGet]
        public IActionResult ImprimirPdfPago(int id)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return NotFound();

            byte[] bytes = GenerarDocsCore.GenerarPdfPago(model);
            string nroRecibo = string.IsNullOrWhiteSpace(model.Pago.NroRecibo) ? ("Pago_" + model.Pago.Id) : model.Pago.NroRecibo.Replace("/", "-");
            return File(bytes, "application/pdf", "Recibo_" + nroRecibo + ".pdf");
        }

        [HttpGet]
        public IActionResult ObtenerDatosEmailPago(int id)
        {
            try
            {
                var model = ConstruirReciboPagoVm(id);
                if (model == null || model.Pago == null || model.Pago.Id <= 0)
                    return Json(new { ok = false, msg = "Pago no encontrado." });

                string nombreEmpresa = model.Empresa != null
                    ? (!string.IsNullOrWhiteSpace(model.Empresa.NombreFantasia) ? model.Empresa.NombreFantasia : model.Empresa.RazonSocialAfip)
                    : "";
                if (string.IsNullOrWhiteSpace(nombreEmpresa)) nombreEmpresa = "CarniSys";

                string tipoOperacion = model.TipoOperacion ?? "Recibo";
                string nroRecibo = model.Pago.NroRecibo ?? ("#" + model.Pago.Id);
                string emailDestino = model.Pago.Persona != null ? (model.Pago.Persona.Email ?? "").Trim() : "";
                string asunto = tipoOperacion + " " + nroRecibo + " - " + nombreEmpresa;
                string cuerpo =
                    "Hola" + (model.Pago.Persona != null && !string.IsNullOrWhiteSpace(model.Pago.Persona.RazonSocial) ? " " + model.Pago.Persona.RazonSocial : "") + ",\n\n" +
                    "Te enviamos adjunto el recibo " + nroRecibo + ".\n\n" +
                    "Fecha: " + model.Pago.Fecha.ToString("dd/MM/yyyy HH:mm") + "\n" +
                    "Importe: $" + Convert.ToDecimal(model.Pago.Importe).ToString("N2") + "\n\n" +
                    "Saludos,\n" + nombreEmpresa;

                return Json(new
                {
                    ok = true,
                    email = emailDestino,
                    asunto,
                    mensaje = cuerpo,
                    empresa = nombreEmpresa,
                    replyTo = model.Empresa != null ? (model.Empresa.Email ?? "") : ""
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EnviarComprobantePagoEmail(int idPago, string emailDestino, string asunto, string mensaje)
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

                byte[] pdfBytes = GenerarDocsCore.GenerarPdfPago(model);
                string nroRecibo = string.IsNullOrWhiteSpace(model.Pago.NroRecibo) ? ("Pago_" + model.Pago.Id) : model.Pago.NroRecibo.Replace("/", "-");
                string nombreAdjunto = "Recibo_" + nroRecibo + ".pdf";
                string nombreEmpresa = model.Empresa != null
                    ? (!string.IsNullOrWhiteSpace(model.Empresa.NombreFantasia) ? model.Empresa.NombreFantasia : model.Empresa.RazonSocialAfip)
                    : "";
                if (string.IsNullOrWhiteSpace(nombreEmpresa)) nombreEmpresa = "CarniSys";

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

        // Puente para clickear una fila "Pagos" del extracto de CtaCtePersona.cshtml (ya existe
        // la variable esPago ahi, solo faltaba esta action) -- Ventas/Compras tambien pasan por
        // aca pero solo Ventas ya esta portado (Compras.ModificarCompra no existe todavia).
        [HttpGet]
        public IActionResult verMovimientoCtaCte(int idPersona, string returnUrl, string tabla, int idTabla = 0)
        {
            var movTabla = new Entidades.MovCtaCte();
            var tablaEnum = movTabla.getTablaEnum(tabla);
            string decodedReturnUrl = DecodeReturnUrlIfNeeded(returnUrl);

            switch (tablaEnum)
            {
                case Entidades.MovCtaCte.tablas.Ventas:
                    return RedirectToAction("DetalleVenta", "Ventas", new { id = idTabla });

                case Entidades.MovCtaCte.tablas.Pagos:
                    return AddOrEditPago(idPersona, decodedReturnUrl, idTabla);

                default:
                    return NotFound();
            }
        }

        private ReciboPagoVm ConstruirReciboPagoVm(int idPago)
        {
            var pago = _oCtaCteN.getPagoById(idPago);
            if (pago == null || pago.Id <= 0)
                return null;

            var empresaActual = _usuarioActual.Empresa;
            if (empresaActual == null && pago.Sucursal != null && pago.Sucursal.IdEmpresa > 0)
                empresaActual = _oSucursalN.findEmpresaById(pago.Sucursal.IdEmpresa);

            DataTable dtCtaCte = _oCtaCteN.getCtaCteByIdPersona(pago.Persona.IdPersona, new DateTime(2000, 1, 1));
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
            string detalleOperacion = pago.AProveedor ? "Se entregó dinero a la persona." : "Se recibió dinero de la persona.";

            return new ReciboPagoVm
            {
                Pago = pago,
                Empresa = empresaActual,
                Saldo = saldo,
                TieneSaldo = tieneSaldo,
                TipoOperacion = tipoOperacion,
                PersonaEtiqueta = personaEtiqueta,
                DetalleOperacion = detalleOperacion,
                UrlPdfAbsoluta = Url.Action("ImprimirPdfPago", "Finanzas", new { id = pago.Id }, Request.Scheme),
                ComprobantesRelacionados = new List<string>()
            };
        }

        private DataTable FiltrarRegistrosRepetidos(DataTable dtMov)
        {
            if (dtMov == null)
                return new DataTable();

            if (dtMov.Rows.Count == 0)
                return dtMov;

            var maxIdPorClave = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in dtMov.Rows)
            {
                string clave = string.Join("|",
                    Convert.ToString(row["tabla"]),
                    Convert.ToString(row["idTabla"]),
                    Convert.ToString(row["sucursal"]));

                int id = Convert.ToInt32(row["id"]);

                int idActual;
                if (!maxIdPorClave.TryGetValue(clave, out idActual) || id > idActual)
                    maxIdPorClave[clave] = id;
            }

            DataTable filtrado = dtMov.Clone();

            foreach (DataRow row in dtMov.Rows)
            {
                string clave = string.Join("|",
                    Convert.ToString(row["tabla"]),
                    Convert.ToString(row["idTabla"]),
                    Convert.ToString(row["sucursal"]));

                int id = Convert.ToInt32(row["id"]);

                int idMaximo;
                if (maxIdPorClave.TryGetValue(clave, out idMaximo) && id == idMaximo)
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        private DataTable TomarUltimosRegistros(DataTable dtMov, int cantidad)
        {
            if (dtMov == null)
                return new DataTable();

            if (cantidad <= 0 || dtMov.Rows.Count <= cantidad)
                return dtMov;

            DataTable ultimos = dtMov.Clone();
            int indiceInicial = Math.Max(0, dtMov.Rows.Count - cantidad);

            for (int i = indiceInicial; i < dtMov.Rows.Count; i++)
                ultimos.ImportRow(dtMov.Rows[i]);

            return ultimos;
        }

        private DataTable OrdenarMovimientosCuentaCorrientePorFecha(DataTable dtMov, bool ascendente)
        {
            if (dtMov == null)
                return new DataTable();

            if (dtMov.Rows.Count <= 1)
                return dtMov;

            string columnaFecha = ObtenerNombreColumna(dtMov, "Fecha", "fecha");
            string columnaId = ObtenerNombreColumna(dtMov, "ID", "Id", "id");

            if (string.IsNullOrWhiteSpace(columnaFecha) && string.IsNullOrWhiteSpace(columnaId))
                return dtMov;

            var filasOrdenadas = dtMov.AsEnumerable()
                .OrderBy(row =>
                {
                    int id = string.IsNullOrWhiteSpace(columnaId) ? 0 : LeerIntRow(row, columnaId);
                    return ascendente ? (id == 0 ? 0 : 1) : (id == 0 ? 1 : 0);
                });

            if (!string.IsNullOrWhiteSpace(columnaFecha))
            {
                filasOrdenadas = ascendente
                    ? filasOrdenadas.ThenBy(row => LeerDateTimeRow(row, columnaFecha))
                    : filasOrdenadas.ThenByDescending(row => LeerDateTimeRow(row, columnaFecha));
            }

            if (!string.IsNullOrWhiteSpace(columnaId))
            {
                filasOrdenadas = ascendente
                    ? filasOrdenadas.ThenBy(row => LeerIntRow(row, columnaId))
                    : filasOrdenadas.ThenByDescending(row => LeerIntRow(row, columnaId));
            }

            DataTable ordenado = dtMov.Clone();
            foreach (DataRow row in filasOrdenadas)
                ordenado.ImportRow(row);

            return ordenado;
        }

        private static string ObtenerNombreColumna(DataTable dt, params string[] candidatos)
        {
            if (dt == null || dt.Columns == null || candidatos == null)
                return null;

            foreach (string candidato in candidatos)
            {
                if (string.IsNullOrWhiteSpace(candidato))
                    continue;

                foreach (DataColumn col in dt.Columns)
                {
                    if (string.Equals(col.ColumnName, candidato, StringComparison.OrdinalIgnoreCase))
                        return col.ColumnName;
                }
            }

            return null;
        }

        private static int LeerIntRow(DataRow row, string columna)
        {
            if (row == null || string.IsNullOrWhiteSpace(columna) || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0;

            int valor;
            return int.TryParse(Convert.ToString(row[columna]), out valor) ? valor : 0;
        }

        private static DateTime LeerDateTimeRow(DataRow row, string columna)
        {
            if (row == null || string.IsNullOrWhiteSpace(columna) || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return DateTime.MinValue;

            DateTime valor;
            return DateTime.TryParse(Convert.ToString(row[columna]), out valor) ? valor : DateTime.MinValue;
        }

        private DateTime ParsearFechaCuentaCorriente(string fechaDesde)
        {
            DateTime fecha;
            if (!DateTime.TryParseExact(fechaDesde ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                fecha = DateTime.Today;

            return fecha;
        }

        private DataTable ObtenerMovimientosCuentaCorrienteParaExportacion(int idPersona, DateTime fecha, bool mostrarAnulados)
        {
            DataTable dt = _oCtaCteN.getCtaCteByIdPersona(idPersona, fecha);
            if (!mostrarAnulados)
                dt = FiltrarRegistrosRepetidos(dt);

            dt = OrdenarMovimientosCuentaCorrientePorFecha(dt, true);
            return dt;
        }

        private byte[] GenerarExcelCuentaCorrienteBytes(int idPersona, DateTime fecha, bool mostrarAnulados, out string fileName)
        {
            DataTable dt = ObtenerMovimientosCuentaCorrienteParaExportacion(idPersona, fecha, mostrarAnulados);
            string csv = "Fecha;Tabla;Detalle;Importe;Saldo;Sucursal\n";

            foreach (DataRow r in dt.Rows)
            {
                csv += Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy") + ";" +
                       r["Tabla"] + ";" +
                       r["Detalle"] + ";" +
                       Convert.ToDecimal(r["Importe"]).ToString("N2") + ";" +
                       Convert.ToDecimal(r["Saldo"]).ToString("N2") + ";" +
                       r["Sucursal"] + "\n";
            }

            string persona = ObtenerNombreArchivoPersona(dt, idPersona);
            fileName = "CuentaCorriente_" + persona + "_Desde_" + fecha.ToString("yyyy-MM-dd") + ".csv";
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }

        private byte[] GenerarPdfCuentaCorrienteBytes(int idPersona, DateTime fecha, bool mostrarAnulados, out string fileName)
        {
            DataTable dtMov = ObtenerMovimientosCuentaCorrienteParaExportacion(idPersona, fecha, mostrarAnulados);
            string persona = dtMov.Rows.Count > 0 ? Convert.ToString(dtMov.Rows[0]["razonSocial"]) : "";

            persona = SanitizarNombreArchivo(persona);
            fileName = "CuentaCorriente_" + persona + "_Desde_" + fecha.ToString("yyyy-MM-dd") + ".pdf";
            return WebCore.Services.GenerarDocsCore.GenerarPdfCtaCtePersona(dtMov, fecha);
        }

        private string ObtenerNombreArchivoPersona(DataTable dt, int idPersona)
        {
            string persona = "";

            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("razonSocial"))
                persona = Convert.ToString(dt.Rows[0]["razonSocial"]);

            if (string.IsNullOrWhiteSpace(persona))
            {
                var personaObj = _oPersonaN.findById(idPersona);
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
                limpio = limpio.Replace(c, '_');

            limpio = Regex.Replace(limpio, "\\s+", "_");
            return string.IsNullOrWhiteSpace(limpio) ? "Persona" : limpio;
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

        private string ConvertirTextoAHtmlPago(string texto)
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
