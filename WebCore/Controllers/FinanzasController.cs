// Port PARCIAL de Web/Controllers/FinanzasController.cs (ver docs/DECISIONS.md, migracion ASP.NET
// Core, Modulo 7 -- Caja y tesoreria, ultimo slice). El original tiene 1941 lineas. Portado en
// este slice: CtasCtes (listado), Cheques (pantalla + CRUD completo: GetCheques/GetCheque/
// GuardarCheque/BuscarChequePorNro/ValidarChequeParaPago).
//
// NO portado en este slice, deliberadamente, por 2 bloqueantes reales (no por falta de tiempo):
//
// 1. Generacion de PDF (iTextSharp): ExportarPdfPersona, ImprimirPdfPago, GenerarPdfPago,
//    GenerarPdfCuentaCorrienteBytes. iTextSharp ya esta marcado como bloqueante en el plan
//    original de esta migracion (no es netstandard, requiere decision de licencia AGPL/comercial
//    de iText7 antes de tocarlo -- CLAUDE.md §1.2, "esperar confirmacion explicita antes de
//    instalar"). No se tomo esa decision, asi que no se porta.
// 2. Envio real de emails (SmtpMailHelper.SendMail): ObtenerDatosEmailCuentaCorriente/
//    EnviarCuentaCorrienteEmail, ObtenerDatosEmailPago/EnviarComprobantePagoEmail. Ademas de
//    depender del PDF (bloqueante #1), enviar un email de prueba mandaria contenido real a la
//    casilla de un cliente/proveedor real -- una accion con efecto visible a terceros que esta
//    migracion nunca ejecuta sin autorizacion explicita puntual, mucho mas alla del permiso
//    generico de escritura en la base local.
//
// Por la misma razon, CtaCtePersona (detalle de cuenta corriente de una persona, con botones de
// exportar/enviar) y AddOrEditPago/AddOrEditPagoPost (alta de pagos/cobros, con impresion de
// ticket/PDF y acoplamiento a POS para el flujo "desdePos") tampoco se portan en este slice --
// dependen en cascada de los 2 bloqueantes de arriba para su flujo completo. Quedan documentados
// como Modulo 7 slice B en docs/10-migracion-aspnet-core/README.md, pendientes de la decision de
// licencia de iText7 antes de continuar.
//
// Mismo stub que el resto de la migracion. PermisosHelper.TienePermiso(Session, ...) se omite
// (bypass de Admin=true) salvo donde el original ya usa un chequeo real independiente de Session
// (ej. "usuario.Admin" en PuedeVerSaldosCuentaCorriente, que se preserva).
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;

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
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oCtaCteN = new Negocio.CuentaCorriente(_empresa, _param);
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
    }
}
