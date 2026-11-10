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
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06). Permisos reales portados 2026-09-09 (Batch B, ver docs/DECISIONS.md "Batch B:
// permisos reales + operador de produccion") en CtasCtes/Cheques/GuardarCheque/CtaCtePersona via
// _oUsuarioN.tienePermiso. AddOrEditPago/AddOrEditPagoPost quedan DELIBERADAMENTE sin gate: el
// propio clasico (Web/Controllers/FinanzasController.cs:822,910) tampoco chequea ningun permiso
// ahi -- es un gap real del sistema legacy, no se inventa un chequeo que el original no tiene.
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
        private readonly IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.CuentaCorriente _oCtaCteN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.CierreCaja _oCierreN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly ICtaCteReservadaService _ctaCteReservada;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public FinanzasController(IUsuarioSesionService sesion, ICtaCteReservadaService ctaCteReservada)
        {
            _sesion = sesion;
            _ctaCteReservada = ctaCteReservada;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oCtaCteN = WebCore.Infrastructure.NegocioFactory.CrearCuentaCorriente(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
        }

        // Port de FinanzasController.ObtenerCajaAbiertaUsuario (Web/Controllers/FinanzasController.cs:1215)
        // -- misma logica exacta, adaptada al usuario real de sesion en vez de Session["Usuario"]
        // clasico. Mismo patron ya usado y verificado en VentasController/CajasController para
        // resolver "la caja abierta del vendedor actual".
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

        // Bug real reportado 2026-09-11 (ver docs/DECISIONS.md): AddOrEditPago buscaba la caja
        // abierta con _usuarioActual directo -- para una cuenta de produccion (usuario compartido)
        // eso nunca coincide con la caja real, abierta a nombre del OPERADOR resuelto por
        // posInstanceId (mismo patron ya usado en VentasController/CajasController/
        // ComprasController, duplicado por controller a proposito -- no hay base controller comun
        // en WebCore). Sin efecto para un usuario normal: ResolverOperadorPOS devuelve el mismo
        // usuario sin cambios cuando no es cuenta de produccion.
        private Entidades.Usuario ResolverOperadorPOS(string posInstanceId, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion) return usuarioSesion;

            var json = HttpContext.Session.GetString("OperadorPOS_" + (posInstanceId ?? ""));
            if (string.IsNullOrEmpty(json)) return usuarioSesion;

            var operador = System.Text.Json.JsonSerializer.Deserialize<Entidades.Usuario>(json);
            return operador ?? usuarioSesion;
        }

        // Mismo texto que MisVentas/FinalizarVenta (VentasController) cuando el operador de produccion ya no esta en Session.
        private const string MensajeOperadorPosVencido = "La autorización del vendedor venció. Recargá el POS e ingresá de nuevo con tu usuario.";

        // True si la cuenta compartida de produccion ya no tiene al operador de esta pestaña de POS en la Session del
        // servidor (reinicio del servidor, 20 min sin pedidos, o recarga del POS que borro al operador con el
        // sendBeacon de pagehide, ver Ventas/POS.cshtml). La pantalla del POS puede seguir mostrando al operador, pero
        // ResolverOperadorPOS ya cae a la cuenta compartida, que nunca tiene caja.
        private bool OperadorPosVencido(string posInstanceId)
        {
            return _usuarioActual != null
                && _usuarioActual.EsUsuarioProduccion
                && string.IsNullOrEmpty(HttpContext.Session.GetString("OperadorPOS_" + (posInstanceId ?? "")));
        }

        // Cuenta corriente / pagos desde el POS: el que consulta es el OPERADOR de la pestana (no la cuenta compartida
        // de produccion) y necesita "POS - Ctas.Ctes Persona" (docs/DECISIONS.md, 2026-11-02 "POS: permisos por
        // funcion"). Devuelve null si puede, o el motivo para responder 403. Los dias ver del permiso los aplica cada
        // pantalla con PermisosHelper.ObtenerFechaMinimaPermitida.
        private string ValidarCtaCteDesdePos(string posInstanceId, out Entidades.Usuario operador)
        {
            operador = null;
            if (OperadorPosVencido(posInstanceId)) return MensajeOperadorPosVencido;

            operador = ResolverOperadorPOS(posInstanceId, _usuarioActual);
            if (!_oUsuarioN.tienePermiso(operador, Entidades.Permisos.Finanza.VerCtaCtePersona, DateTime.Today, -1))
            {
                // Autorizacion provisoria de otro usuario (2026-11-03): mientras dure, la consulta se hace con los dias
                // ver de quien autorizo. Sin ella, el mismo 403 de siempre y un encabezado que le avisa al cliente que
                // puede pedirla (el cuerpo no cambia).
                var autorizante = WebCore.Helpers.AutorizacionProvisoriaPosHelper.Autorizante(
                    HttpContext.Session, _oUsuarioN, posInstanceId, Negocio.AutorizacionesProvisoriasPos.AccionCtaCte, 0);
                if (autorizante == null)
                {
                    MarcarPuedePedirAutorizacion(Negocio.AutorizacionesProvisoriasPos.AccionCtaCte, 0);
                    return "No tenés permiso para ver cuentas corrientes (POS - Ctas.Ctes Persona).";
                }

                operador = autorizante;
            }

            return null;
        }

        // Encabezados con los que el 403 le dice al cliente del POS que la accion se puede autorizar con otro usuario.
        private void MarcarPuedePedirAutorizacion(string accion, int idRegistro)
        {
            Response.Headers["X-Pos-Autorizacion"] = accion;
            Response.Headers["X-Pos-Autorizacion-Id"] = idRegistro.ToString(CultureInfo.InvariantCulture);
        }

        // Cargar o modificar un pago/cobro desde el POS (2026-11-02): exige la parte de edicion de "POS - Ctas.Ctes
        // Persona". Un pago ya guardado se modifica solo si es del propio operador, de su caja abierta
        // (EsPagoDeMiCajaAbierta) y dentro del limite de fecha: el mas reciente entre el inicio de la caja y los dias
        // editar del permiso (Negocio.LimiteEdicionCaja). El administrador no tiene limites. null = puede.
        //
        // Autorizacion provisoria de otro usuario (2026-11-03): si el operador NO tiene el permiso y otro usuario que si lo
        // tiene lo autorizo, rigen los dias editar de quien autorizo; el pago sigue teniendo que ser de la caja del
        // operador y de su inicio en adelante, salvo que autorice un administrador. puedeAutorizar = el motivo es la
        // falta del permiso (y no hay autorizacion vigente): el cliente puede pedirla.
        private string ValidarPermisoPagoDesdePos(Entidades.Pago pagoExistente, DateTime fechaNueva, Entidades.Usuario operador, Entidades.CierreCaja caja, string posInstanceId, out bool puedeAutorizar)
        {
            puedeAutorizar = false;
            if (operador == null) return MensajeOperadorPosVencido;
            if (operador.Admin) return null;

            Entidades.Usuario autorizante = null;
            if (!_oUsuarioN.tienePermiso(operador, Entidades.Permisos.Finanza.AddOrEditPago, DateTime.Now, operador.Id))
            {
                autorizante = WebCore.Helpers.AutorizacionProvisoriaPosHelper.Autorizante(
                    HttpContext.Session, _oUsuarioN, posInstanceId, Negocio.AutorizacionesProvisoriasPos.AccionPago, pagoExistente?.Id ?? 0);
                if (autorizante == null)
                {
                    puedeAutorizar = true;
                    return "No tenés permiso para cargar ni modificar pagos o cobros (POS - Ctas.Ctes Persona).";
                }

                // Alta autorizada: la fecha ya se acota a la caja abierta en AddOrEditPagoPost. Un administrador no tiene limites.
                if (pagoExistente == null || autorizante.Admin) return null;
            }

            // Alta: la fecha ya se acota a la caja abierta en AddOrEditPagoPost.
            if (pagoExistente == null) return null;

            if (!EsPagoDeMiCajaAbierta(pagoExistente, posInstanceId))
                return "Solo podés modificar los pagos y cobros que cargaste desde que abriste tu caja actual.";

            // Los dias editar son los de quien tiene el permiso: el operador, o quien lo autorizo.
            var conPermiso = autorizante ?? operador;
            DateTime? minimaPorDias = WebCore.Helpers.PermisosHelper.ObtenerFechaMinimaPermitida(
                conPermiso, Entidades.Permisos.Finanza.AddOrEditPago, conPermiso.Id);
            DateTime? inicioCaja = caja != null ? caja.FechaHoraInicio : null;
            if (!Negocio.LimiteEdicionCaja.PuedeEditar(pagoExistente.Fecha, inicioCaja, minimaPorDias)
                || !Negocio.LimiteEdicionCaja.PuedeEditar(fechaNueva, inicioCaja, minimaPorDias))
                return "Solo podés modificar pagos y cobros desde el inicio de tu caja y dentro de los días que permite tu permiso.";

            return null;
        }

        // Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01). Para un usuario restringido y una
        // persona reservada: saca del extracto los movimientos que no puede ver, saca la fila sintetica
        // "Saldo Anterior" (id 0, suma TODO el historial previo, incluido lo oculto) y anula el saldo
        // acumulado (se calculo sobre el libro completo). Devuelve true si se aplico la reserva: el
        // llamador debe ocultar el saldo en pantalla/exportes. Modifica el DataTable recibido.
        private bool AplicarReservaAlExtracto(DataTable dtMov, int idPersona)
        {
            if (dtMov == null || !_ctaCteReservada.SaldoOculto(idPersona))
                return false;

            var ocultos = _ctaCteReservada.IdsOcultos(Entidades.RestriccionCtaCteReservada.TablaMovCtaCte);
            bool tieneId = dtMov.Columns.Contains("id");
            bool tieneSaldo = dtMov.Columns.Contains("Saldo");

            for (int i = dtMov.Rows.Count - 1; i >= 0; i--)
            {
                // Sin columna id no se puede saber que es propio: falla cerrado (no se muestra nada).
                int id = tieneId && dtMov.Rows[i]["id"] != DBNull.Value ? Convert.ToInt32(dtMov.Rows[i]["id"]) : 0;
                if (id == 0 || ocultos.Contains(id))
                    dtMov.Rows.RemoveAt(i);
                else if (tieneSaldo)
                    dtMov.Rows[i]["Saldo"] = 0m;
            }
            return true;
        }

        // Saca de un listado (DataTable con columna de id) las filas de registros ocultos para el
        // usuario restringido. Modifica y devuelve el mismo DataTable.
        private DataTable QuitarFilasOcultas(DataTable dt, string tabla, string columnaId)
        {
            if (dt == null || !dt.Columns.Contains(columnaId)) return dt;

            var ocultos = _ctaCteReservada.IdsOcultos(tabla);
            if (ocultos.Count == 0) return dt;

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                object valor = dt.Rows[i][columnaId];
                if (valor != DBNull.Value && ocultos.Contains(Convert.ToInt32(valor)))
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }

        // Respuesta de "registro reservado" para los chequeos manuales de este controller (las
        // acciones con id directo usan [ProtegerRegistroReservado]).
        private IActionResult DenegarRegistroReservado()
        {
            return WebCore.Helpers.ProtegerRegistroReservadoFilter.Denegar(ControllerContext);
        }

        private bool EsPeticionAjax()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public IActionResult CtasCtes(string buscar = "", string ordenSaldo = "DESC", bool desdePos = false, string posInstanceId = "")
        {
            bool renderParcial = desdePos || EsPeticionAjax();

            // Desde el POS (F2) la lista de cuentas corrientes exige "POS - Ctas.Ctes Persona" del operador (2026-11-02).
            if (desdePos)
            {
                string motivoSinAcceso = ValidarCtaCteDesdePos(posInstanceId, out _);
                if (motivoSinAcceso != null)
                    return StatusCode(403, motivoSinAcceso);
            }

            // Port de Web/Controllers/FinanzasController.cs:99-134. Bug real (cuarta ronda de
            // pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 4: modal Cuenta Corriente desde
            // POS (F2)"): faltaba el parametro desdePos -- el gate de permiso corria siempre, asi
            // que F2 desde POS quedaba bloqueado para cualquier usuario sin el permiso general,
            // en vez de entrar igual (consulta puntual) con el saldo oculto.
            bool puedeVerCtasCtes = _oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Finanza.VerCtasCtes, DateTime.Today, -1);
            if (!desdePos && !puedeVerCtasCtes)
            {
                ViewBag.Seccion = "Cuenta Corriente";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            ordenSaldo = string.Equals(ordenSaldo, "ASC", StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : "DESC";

            ViewBag.Buscar = buscar;
            ViewBag.DesdePOS = desdePos;
            ViewBag.PosInstanceId = posInstanceId;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.OrdenSaldo = ordenSaldo;
            ViewBag.OcultarSaldo = desdePos && !puedeVerCtasCtes;

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

            // Port de Web/Controllers/FinanzasController.cs:179-186 (rama "no modo POS" -- esta
            // accion siempre es esa rama).
            if (!_oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Finanza.VerCheques, DateTime.Today, -1))
            {
                ViewBag.Seccion = "Cheques";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

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

                // Port de Web/Controllers/FinanzasController.cs:1819-1826 -- el idCreador para el
                // chequeo de permiso es el creador ORIGINAL del cheque (si es edicion) o el
                // usuario actual (si es alta).
                int idUserCreador = cheque.Id > 0 ? (_oCtaCteN.getChequePorIDorNro(cheque.Id, "")?.CreadoPor?.Id ?? user.Id) : user.Id;
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Finanza.VerCheques, DateTime.Today, idUserCreador))
                    return Json(new { ok = false, message = "No tienes permisos para esta acción." });

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
        public IActionResult CtaCtePersona(int idPersona, DateTime? fechaDesde, bool mostrarAnulados = false, string returnUrl = "", bool desdePos = false, string posInstanceId = "", int idTitular = 0)
        {
            bool renderParcial = desdePos || EsPeticionAjax();
            // Se reenvia a la vista para armar los links de "nuevo pago"/"editar pago" -- ver
            // ResolverOperadorPOS mas arriba.
            ViewBag.PosInstanceId = posInstanceId;

            // Port de Web/Controllers/FinanzasController.cs:217-235. Bug real (cuarta ronda de
            // pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 4: modal Cuenta Corriente desde
            // POS (F2)"): faltaba el parametro desdePos -- Ventas/POS.cshtml ya lo mandaba en la
            // URL (abrirCtaCtePersona) pero la firma de esta accion no lo aceptaba, asi que se
            // ignoraba en silencio. Simplificacion deliberada frente al clasico (Web/Controllers/
            // FinanzasController.cs:277-294, que ademas ajusta MostrarSoloMovimientosCajaActual/
            // PuedeExportarCuentaCorriente/OcultarFiltroFechaDesde segun el modo POS): acá solo se
            // replica el bypass de permiso + ocultar saldo, que es lo unico pedido y verificado --
            // si en el futuro se necesita paridad completa de esos otros campos, es un batch aparte.
            // Cuenta de produccion (usuario compartido): exige un operador autenticado con contrasena antes de
            // mostrar la cuenta corriente, mismo criterio que Ventas/Compras (docs/DECISIONS.md, 2026-11-02). Fuera
            // del POS (desdePos tiene su propio operador) y con ese operador se evalua el permiso y la ventana
            // de dias; el operador caduca al salir de Finanzas (PermisosHelper.LimpiarOperadorModuloSiSalioDelModulo).
            Entidades.Usuario usuarioConsulta = _usuarioActual;
            if (desdePos)
            {
                // Desde el POS la consulta es del operador de la pestana y exige "POS - Ctas.Ctes Persona" (2026-11-02).
                string motivoSinAcceso = ValidarCtaCteDesdePos(posInstanceId, out var operadorPos);
                if (motivoSinAcceso != null)
                    return StatusCode(403, motivoSinAcceso);
                usuarioConsulta = operadorPos;
            }

            if (!desdePos && _usuarioActual.EsUsuarioProduccion)
            {
                var operadorFinanzas = ObtenerOperadorFinanzas();
                if (operadorFinanzas == null)
                {
                    // Un pedido AJAX no puede mostrar la pantalla de autorizacion: se corta con 403.
                    if (renderParcial)
                        return StatusCode(403, "Autorizate con tu usuario para ver la cuenta corriente.");

                    return RedirectToAction("AutorizarModuloCtaCte", new
                    {
                        returnUrl = Request.PathBase + Request.Path + Request.QueryString,
                        cancelUrl = DecodeReturnUrlIfNeeded(returnUrl)
                    });
                }

                usuarioConsulta = operadorFinanzas;
            }

            bool puedeVerCtaCtePersona = _oUsuarioN.tienePermiso(usuarioConsulta, Entidades.Permisos.Finanza.VerCtaCtePersona, DateTime.Today, -1);
            if (!desdePos && !puedeVerCtaCtePersona)
            {
                ViewBag.Seccion = "Cuenta Corriente Persona";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (!fechaDesde.HasValue)
                fechaDesde = DateTime.Now.Date;

            // Item 2a (2026-09-12, ver docs/DECISIONS.md): "Días ver" ya es un mecanismo real
            // configurable por usuario (Entidades.PermisosUsuarios.DiasPermitidosVer, ver
            // PermisosHelper.ObtenerFechaMinimaPermitida) -- antes solo se usaba para el gate
            // todo-o-nada de arriba (tienePermiso con DateTime.Today fijo), nunca para acotar
            // hasta donde puede pedir el filtro de fecha. Si el usuario pide explicitamente algo
            // mas viejo que la ventana permitida, se acota (no se le muestra igual) y se informa
            // -- "informar, no fallar en silencio", mismo criterio que el resto del proyecto.
            // null = admin o sin este permiso asignado con limite -- sin restriccion.
            DateTime? fechaMinimaPermitidaCtaCte = WebCore.Helpers.PermisosHelper.ObtenerFechaMinimaPermitida(
                usuarioConsulta, Entidades.Permisos.Finanza.VerCtaCtePersona);
            bool fechaRecortadaPorPermiso = fechaMinimaPermitidaCtaCte.HasValue && fechaDesde.Value < fechaMinimaPermitidaCtaCte.Value;
            if (fechaRecortadaPorPermiso)
                fechaDesde = fechaMinimaPermitidaCtaCte.Value;

            var persona = _oPersonaN.findById(idPersona);
            // Titular propio (2026-11-09, solo Postgres y solo con 2 o mas titulares): cada movimiento lleva su titular y se puede filtrar
            // por uno. Dentro del modal del POS la cuenta queda como siempre.
            bool conTitulares = false;
            DataTable dtMov = desdePos
                ? _oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesde.Value)
                : ObtenerExtractoPersona(idPersona, fechaDesde.Value, idTitular, out conTitulares);
            bool extractoReservado = AplicarReservaAlExtracto(dtMov, idPersona);

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

            // Observaciones del registro origen (Pagos/Ventas/Compras) para la columna "Obs." de la grilla.
            // (Para una persona reservada y un usuario restringido no se mandan: son de todos los registros.)
            ViewBag.ObservacionesCtaCte = extractoReservado
                ? new Dictionary<string, string>()
                : _oCtaCteN.obtenerObservacionesCtaCte(idPersona);

            // Items de cada venta/compra ("3 articulos · 13,35 kg") para la linea gris de la fila. Misma regla que las
            // observaciones: una persona reservada vista por un usuario restringido no recibe datos de otros registros.
            ViewBag.ResumenItemsCtaCte = extractoReservado
                ? new Dictionary<string, Negocio.ResumenItemsMovimientoCtaCte>()
                : _oCtaCteN.obtenerResumenItemsCtaCte(idPersona, fechaDesde.Value);

            ViewBag.IdPersona = idPersona;
            ViewBag.Persona = persona;
            ViewBag.SaldoPersona = saldo;
            ViewBag.ReturnUrlCtaCte = DecodeReturnUrlIfNeeded(returnUrl);
            ViewBag.FechaDesde = fechaDesde.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaMinimaPermitidaCtaCte = fechaMinimaPermitidaCtaCte?.ToString("yyyy-MM-dd");
            ViewBag.MensajeFechaRecortadaPorPermiso = fechaRecortadaPorPermiso
                ? "No podés ver movimientos anteriores al " + fechaMinimaPermitidaCtaCte.Value.ToString("dd/MM/yyyy") + "."
                : "";
            ViewBag.MostrarAnulados = mostrarAnulados;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.DesdePOS = desdePos;
            ViewBag.OcultarSaldo = (desdePos && !puedeVerCtaCtePersona) || extractoReservado;
            // Resumen por titular y control fiscal. Una cuenta reservada vista por un usuario restringido no lo recibe.
            ViewBag.TitularesCtaCte = conTitulares
                ? ArmarTitularesCtaCte(idPersona, fechaDesde.Value, idTitular, usuarioConsulta, extractoReservado, true)
                : null;
            ViewBag.PuedeExportarCuentaCorriente = true;
            ViewBag.Title = "Cuenta Corriente";

            if (renderParcial)
                return PartialView("~/Views/Finanzas/CtaCtePersona.cshtml", dtMov);

            return View("~/Views/Finanzas/CtaCtePersona.cshtml", dtMov);
        }

        // Detalle (items) de un movimiento de la cuenta corriente de una persona, para el panel que despliega cada fila
        // de CtaCtePersona. Solo lectura, pedido AJAX: los fallos devuelven un codigo HTTP con un mensaje corto que el
        // panel muestra tal cual. Aplica las mismas reglas de acceso que CtaCtePersona (operador de produccion, permiso
        // VerCtaCtePersona y su ventana de dias) y ademas exige que el movimiento sea de ESA persona: el id viene del
        // navegador y no se confia en el. Las compras muestran precios solo con el permiso VerCompras.
        [HttpGet]
        public IActionResult DetalleMovimiento(int idPersona, string tabla, int idTabla, bool desdePos = false, string posInstanceId = "")
        {
            // 1) Quien consulta: igual que CtaCtePersona (cuenta de produccion = operador autenticado de Finanzas).
            Entidades.Usuario usuarioConsulta = _usuarioActual;
            if (desdePos)
            {
                string motivoSinAcceso = ValidarCtaCteDesdePos(posInstanceId, out var operadorPos);
                if (motivoSinAcceso != null)
                    return StatusCode(403, motivoSinAcceso);
                usuarioConsulta = operadorPos;
            }

            if (!desdePos && _usuarioActual.EsUsuarioProduccion)
            {
                usuarioConsulta = ObtenerOperadorFinanzas();
                if (usuarioConsulta == null)
                    return StatusCode(403, "Autorizate con tu usuario para ver la cuenta corriente.");
            }

            if (!desdePos && !_oUsuarioN.tienePermiso(usuarioConsulta, Entidades.Permisos.Finanza.VerCtaCtePersona, DateTime.Today, -1))
                return StatusCode(403, "No tenés permiso para ver la cuenta corriente.");

            // 2) Solo los tipos que tienen items para mostrar.
            Entidades.MovCtaCte.tablas tablaMovimiento;
            if (string.Equals(tabla, Entidades.RestriccionCtaCteReservada.TablaVentas, StringComparison.OrdinalIgnoreCase))
                tablaMovimiento = Entidades.MovCtaCte.tablas.Ventas;
            else if (string.Equals(tabla, Entidades.RestriccionCtaCteReservada.TablaCompras, StringComparison.OrdinalIgnoreCase))
                tablaMovimiento = Entidades.MovCtaCte.tablas.Compras;
            else if (string.Equals(tabla, Entidades.RestriccionCtaCteReservada.TablaPagos, StringComparison.OrdinalIgnoreCase))
                tablaMovimiento = Entidades.MovCtaCte.tablas.Pagos;
            else
                return BadRequest("Este tipo de movimiento no tiene detalle.");

            if (idPersona <= 0 || idTabla <= 0)
                return BadRequest("Movimiento inválido.");

            // 3) El movimiento tiene que ser de la persona pedida, y no anterior a la ventana de dias que su permiso deja ver.
            var movimiento = _oCtaCteN.getMovCtaCteDeRegistro(tablaMovimiento, idTabla);
            if (movimiento == null || movimiento.Id <= 0 || movimiento.Persona == null || movimiento.Persona.IdPersona != idPersona)
                return NotFound("El movimiento no pertenece a esta cuenta corriente.");

            DateTime? fechaMinima = WebCore.Helpers.PermisosHelper.ObtenerFechaMinimaPermitida(usuarioConsulta, Entidades.Permisos.Finanza.VerCtaCtePersona);
            if (fechaMinima.HasValue && movimiento.Fecha < fechaMinima.Value)
                return StatusCode(403, "No podés ver movimientos anteriores al " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".");

            // 4) Cuenta reservada: el usuario restringido no ve el registro (misma regla que las pantallas de cada registro).
            if (_ctaCteReservada.RegistroOculto(tabla, idTabla))
                return DenegarRegistroReservado();

            var modelo = new CtaCteDetalleMovimientoVm { Tabla = tablaMovimiento.ToString() };
            var cultura = CultureInfo.CurrentCulture;

            switch (tablaMovimiento)
            {
                case Entidades.MovCtaCte.tablas.Ventas:
                {
                    var oVentaN = WebCore.Infrastructure.NegocioFactory.CrearVenta(_empresa, _param);
                    var venta = oVentaN.getVentaById(idTabla);
                    if (venta == null || venta.IdVenta <= 0)
                        return NotFound("No se encontró la venta.");

                    modelo.Venta = venta;
                    if (!string.IsNullOrWhiteSpace(venta.FormaPago))
                        modelo.Notas.Add("Forma de pago: " + venta.FormaPago);
                    if (oVentaN.existeFactuElectParaVenta(venta.IdVenta) > 0)
                        modelo.Notas.Add("Facturada");
                    break;
                }

                case Entidades.MovCtaCte.tablas.Compras:
                {
                    var oCompraN = WebCore.Infrastructure.NegocioFactory.CrearCompra(_empresa, _param);
                    var compra = oCompraN.findById_convertToCompra(idTabla);
                    if (compra == null || compra.IdCompra <= 0)
                        return NotFound("No se encontró la compra.");

                    // Precios de costo: solo con el permiso de ver compras, evaluado sobre el mismo usuario que la cuenta corriente.
                    bool verPrecios = _oUsuarioN.tienePermiso(usuarioConsulta, Entidades.Permisos.Compra.VerCompras, compra.FechaCompra, -1);

                    var cortes = oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>();
                    foreach (var corte in cortes)
                    {
                        modelo.Lineas.Add(new CtaCteLineaDetalleVm
                        {
                            Nombre = corte.Corte != null ? corte.Corte.CorteDesc : "-",
                            Codigo = corte.Corte != null ? "cod " + corte.Corte.Codigo : "",
                            Calculo = TextoCalculoLineaCtaCte((decimal)corte.CantKgs, verPrecios ? (decimal?)corte.precioKgs : null, cultura),
                            SubtotalTexto = verPrecios ? ((decimal)corte.CantKgs * (decimal)corte.precioKgs).ToString("C", cultura) : ""
                        });
                    }

                    // Medias reses (compra de media res): una linea por media, con su tropa si la tiene.
                    var medias = oCompraN.obtenerMediasPorCompra(compra.IdCompra);
                    int numeroMedia = 1;
                    foreach (DataRow media in medias.Rows)
                    {
                        decimal kg = media["kgMedia"] != DBNull.Value ? Convert.ToDecimal(media["kgMedia"]) : 0m;
                        decimal precio = media["precioMedia"] != DBNull.Value ? Convert.ToDecimal(media["precioMedia"]) : 0m;
                        string tropa = media.Table.Columns.Contains("nroTropa") && media["nroTropa"] != DBNull.Value
                            ? Convert.ToString(media["nroTropa"]).Trim()
                            : "";
                        modelo.Lineas.Add(new CtaCteLineaDetalleVm
                        {
                            Nombre = "Media res " + numeroMedia,
                            Codigo = tropa.Length > 0 ? "tropa " + tropa : "",
                            Calculo = TextoCalculoLineaCtaCte(kg, verPrecios ? (decimal?)precio : null, cultura),
                            SubtotalTexto = verPrecios ? (kg * precio).ToString("C", cultura) : ""
                        });
                        numeroMedia++;
                    }

                    // Titulo del panel: "3 productos", "5 medias" o "2 productos y 1 media" (mismo criterio que la linea gris de la fila).
                    int cantidadMedias = numeroMedia - 1;
                    var partesTitulo = new List<string>();
                    if (cortes.Count > 0)
                        partesTitulo.Add(cortes.Count + (cortes.Count == 1 ? " producto" : " productos"));
                    if (cantidadMedias > 0)
                        partesTitulo.Add(cantidadMedias + (cantidadMedias == 1 ? " media" : " medias"));
                    modelo.TituloLineas = string.Join(" y ", partesTitulo);
                    if (verPrecios)
                    {
                        decimal total = cortes.Sum(c => (decimal)c.CantKgs * (decimal)c.precioKgs);
                        foreach (DataRow media in medias.Rows)
                        {
                            decimal kg = media["kgMedia"] != DBNull.Value ? Convert.ToDecimal(media["kgMedia"]) : 0m;
                            decimal precio = media["precioMedia"] != DBNull.Value ? Convert.ToDecimal(media["precioMedia"]) : 0m;
                            total += kg * precio;
                        }
                        modelo.TotalTexto = total.ToString("C", cultura);
                    }

                    if (!string.IsNullOrWhiteSpace(compra.TipoCompra))
                        modelo.Notas.Add("Tipo de compra: " + compra.TipoCompra);
                    if (!string.IsNullOrWhiteSpace(compra.NroRemito))
                        modelo.Notas.Add("Remito/Factura: " + compra.NroRemito);

                    // Titular propio y comprobantes del proveedor (2026-11-09, solo Postgres). Los importes de los comprobantes se ven
                    // solo con el permiso de ver compras, igual que los precios.
                    var comprobanteN = WebCore.Infrastructure.NegocioFactory.CrearComprobanteCompra(_empresa);
                    if (comprobanteN != null && Negocio.ValidacionComprobanteCompra.TipoCompraAdmiteComprobantes(compra.TipoCompra))
                    {
                        var detalleComprobantes = comprobanteN.ObtenerDetalle(compra.IdCompra);
                        var titularesDetalle = ListarTitularesPago();
                        if (titularesDetalle.Count > 0)
                        {
                            int idTitularDetalle = detalleComprobantes.IdTitular ?? IdTitularPrincipalPago();
                            var titularDetalle = titularesDetalle.FirstOrDefault(t => t.IdTitular == idTitularDetalle);
                            if (titularDetalle != null)
                                modelo.Notas.Add("Titular: " + titularDetalle.Nombre);
                        }

                        string estadoDetalle = detalleComprobantes.EstadoFactura == Entidades.EstadoFacturaCompra.Facturada ? "facturada"
                            : detalleComprobantes.EstadoFactura == Entidades.EstadoFacturaCompra.Pendiente ? "pendiente de factura"
                            : detalleComprobantes.EstadoFactura == Entidades.EstadoFacturaCompra.NoLleva ? "no lleva factura"
                            : "";
                        if (estadoDetalle.Length > 0)
                            modelo.Notas.Add("Factura: " + estadoDetalle);
                        if (verPrecios)
                        {
                            foreach (var comprobante in detalleComprobantes.Comprobantes)
                                modelo.Notas.Add(comprobante.TipoComprobante + " " + comprobante.NumeroCompleto + ": " + comprobante.TotalConSigno.ToString("C", cultura)
                                    + " (neto " + (comprobante.NetoGravado * Entidades.TiposComprobanteCompra.Signo(comprobante.TipoComprobante)).ToString("C", cultura) + ")");
                        }
                    }
                    break;
                }

                default:
                {
                    var pago = _oCtaCteN.getPagoById(idTabla);
                    if (pago == null || pago.Id <= 0)
                        return NotFound("No se encontró el pago o cobro.");

                    modelo.TituloLineas = "Cómo se pagó";
                    if (pago.EfectivoDec > 0m)
                        modelo.Lineas.Add(new CtaCteLineaDetalleVm { Nombre = "Efectivo", SubtotalTexto = pago.EfectivoDec.ToString("C", cultura) });

                    // Cada cheque con su banco y numero; si el pago no trae la lista pero tiene datos de cheque, se muestra uno.
                    if (pago.Cheques != null && pago.Cheques.Count > 0)
                    {
                        foreach (var cheque in pago.Cheques)
                        {
                            modelo.Lineas.Add(new CtaCteLineaDetalleVm
                            {
                                Nombre = "Cheque " + (cheque.Banco ?? "").Trim(),
                                Codigo = string.IsNullOrWhiteSpace(cheque.NroCheque) ? "" : "Nº " + cheque.NroCheque.Trim(),
                                Calculo = cheque.FechaPago > DateTime.MinValue ? "Cobro " + cheque.FechaPago.ToString("dd/MM/yyyy") : "",
                                SubtotalTexto = ((decimal)cheque.Importe).ToString("C", cultura)
                            });
                        }
                    }
                    else if (pago.ImporteDec > pago.EfectivoDec)
                    {
                        modelo.Lineas.Add(new CtaCteLineaDetalleVm
                        {
                            Nombre = string.IsNullOrWhiteSpace(pago.FormaPago) ? "Otro medio" : pago.FormaPago,
                            Codigo = string.IsNullOrWhiteSpace(pago.NroCheque) ? "" : "Nº " + pago.NroCheque.Trim(),
                            SubtotalTexto = (pago.ImporteDec - pago.EfectivoDec).ToString("C", cultura)
                        });
                    }

                    modelo.TotalTexto = pago.ImporteDec.ToString("C", cultura);
                    // La forma de pago ya va en los renglones (Efectivo / Cheque / Transferencia): no se repite como nota.
                    if (!string.IsNullOrWhiteSpace(pago.NroRecibo))
                        modelo.Notas.Add("Recibo: " + pago.NroRecibo);
                    break;
                }
            }

            return PartialView("~/Views/Finanzas/_CtaCteDetalleMovimiento.cshtml", modelo);
        }

        // "91 kg × $ 4.250,00" (o "91 kg" sin precio) para un renglon del panel. Los espacios de cada pieza son duros (NBSP):
        // en celular la linea solo puede partirse en el "×", nunca entre el "$" y el numero ni entre el numero y "kg".
        private static string TextoCalculoLineaCtaCte(decimal kg, decimal? precioKg, CultureInfo cultura)
        {
            string kilos = kg.ToString("0.###", cultura) + " kg";
            if (!precioKg.HasValue)
                return kilos;

            return kilos + " × " + precioKg.Value.ToString("C", cultura).Replace(' ', ' ');
        }

        // ===== Operador de produccion para la cuenta corriente de una persona (docs/DECISIONS.md, 2026-11-02) =====
        // Mismo patron que ComprasController/VentasController (AutorizarModulo*), duplicado por controller a
        // proposito: no hay base controller comun en WebCore. El operador vive en Session como
        // "OperadorModulo_Finanzas" hasta que se renderiza una pantalla de otro modulo.
        private const string ModuloOperadorFinanzas = "Finanzas";

        private Entidades.Usuario ObtenerOperadorFinanzas()
        {
            return WebCore.Helpers.PermisosHelper.ObtenerOperadorModulo(HttpContext.Session, ModuloOperadorFinanzas);
        }

        // Ver comentario identico en ComprasController.ObtenerSessionIdEstable: ASP.NET Core no manda la cookie
        // de sesion hasta el primer write, y sin eso el rate-limit por sesion nunca acumula intentos.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        [HttpGet]
        public IActionResult AutorizarModuloCtaCte(string returnUrl, string cancelUrl)
        {
            var dt = _oUsuarioN.obtenerUsuarios(true);
            var usuarios = new List<object>();
            if (dt != null && dt.Columns.Contains("id") && dt.Columns.Contains("nombre"))
            {
                usuarios = dt.AsEnumerable()
                    .Select(row => new
                    {
                        id = row["id"] == DBNull.Value ? 0 : Convert.ToInt32(row["id"]),
                        nombre = row["nombre"] == DBNull.Value ? "" : Convert.ToString(row["nombre"])
                    })
                    .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                    .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                    .Cast<object>()
                    .ToList();
            }

            // Solo rutas locales: el destino viene de la URL y no puede mandar a otro sitio.
            ViewBag.UsuariosActivosEmpresa = usuarios;
            ViewBag.ReturnUrlModuloCtaCte = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("CtasCtes", "Finanzas");
            ViewBag.CancelUrlModuloCtaCte = !string.IsNullOrWhiteSpace(cancelUrl) && Url.IsLocalUrl(cancelUrl)
                ? cancelUrl
                : Url.Action("Index", "Home");
            ViewBag.Title = "Autorizar operador";
            ViewBag.Seccion = "Cuenta Corriente";
            return View("~/Views/Finanzas/AutorizarModuloCtaCte.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AutorizarOperadorModuloCtaCte(int idUsuario, string clave)
        {
            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return Json(new { ok = false, bloqueado = true, segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds) });

            const string mensajeGenerico = "Usuario o contraseña incorrectos.";
            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave))
                return Json(WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId).ComoRespuestaJson(mensajeGenerico));

            var candidato = _oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
                return Json(WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId).ComoRespuestaJson(mensajeGenerico));

            var validado = _oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            if (validado == null || !validado.Activo)
                return Json(WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId).ComoRespuestaJson(mensajeGenerico));

            WebCore.Helpers.PosOperadorStepUpRateLimiter.Reset(sessionId);

            // La clave era correcta, pero este operador tiene que poder ver la cuenta corriente: si no, no se
            // registra y el modal se lo dice (no es un intento fallido de clave).
            if (!_oUsuarioN.tienePermiso(validado, Entidades.Permisos.Finanza.VerCtaCtePersona, DateTime.Today, -1))
                return Json(new { ok = false, msg = "Ese usuario no tiene permiso para ver cuentas corrientes." });

            WebCore.Helpers.PermisosHelper.RegistrarOperadorModulo(HttpContext.Session, ModuloOperadorFinanzas, validado);
            return Json(new { ok = true, nombre = validado.Nombre });
        }

        [HttpGet]
        public IActionResult ExportarPdfPersona(int idPersona, string fechaDesde, bool mostrarAnulados = false, int idTitular = 0)
        {
            DateTime fechaD;
            if (!DateTime.TryParse(fechaDesde, out fechaD))
                fechaD = DateTime.Today.AddMonths(-1);

            string fileName;
            // Descarga directa: incluye el control fiscal solo si el usuario tiene el permiso (se decide adentro).
            byte[] pdfBytes = GenerarPdfCuentaCorrienteBytes(idPersona, fechaD, mostrarAnulados, idTitular, true, out fileName);
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet]
        public IActionResult ExportarExcelPersona(int idPersona, string fechaDesde, bool mostrarAnulados = false, int idTitular = 0)
        {
            DateTime fecha = ParsearFechaCuentaCorriente(fechaDesde);
            string fileName;
            byte[] buffer = GenerarExcelCuentaCorrienteBytes(idPersona, fecha, mostrarAnulados, idTitular, true, out fileName);
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
        public IActionResult EnviarCuentaCorrienteEmail(int idPersona, string fechaDesde, bool mostrarAnulados, string emailDestino, string asunto, string mensaje, string formato, int idTitular = 0)
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
                    // El mail sale a un tercero (la persona): NUNCA lleva el control fiscal, aunque quien lo envia tenga el permiso.
                    attachmentBytes = GenerarExcelCuentaCorrienteBytes(idPersona, fecha, mostrarAnulados, idTitular, false, out fileName);
                    contentType = "text/csv";
                }
                else
                {
                    attachmentBytes = GenerarPdfCuentaCorrienteBytes(idPersona, fecha, mostrarAnulados, idTitular, false, out fileName);
                    contentType = "application/pdf";
                }

                string fromName = SmtpMailHelper.BuildTenantFromName(nombreEmpresa);
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
        // ===== Pagos: listado de todos los pagos y cobros (2026-09-24, ver docs/DECISIONS.md "Pagos:
        // listado, cambio de persona y eliminacion"). Cada fila abre AddOrEditPago del pago. Los eliminados
        // se muestran con la etiqueta ELIMINADO (nunca se borran). =====
        [HttpGet]
        public IActionResult Pagos(string texto = "", string desde = "", string hasta = "", string operacion = "", bool verEliminados = true, int idTitular = 0)
        {
            bool renderParcial = EsPeticionAjax();

            if (!_oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Finanza.VerPagos, DateTime.Today, -1))
            {
                ViewBag.Seccion = "Pagos";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            // por defecto el ultimo mes; fechas invalidas caen al default en vez de romper la pantalla
            DateTime fechaDesde = DateTime.Today.AddMonths(-1);
            DateTime fechaHasta = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(desde) && DateTime.TryParse(desde, out var desdeParseado)) fechaDesde = desdeParseado.Date;
            if (!string.IsNullOrWhiteSpace(hasta) && DateTime.TryParse(hasta, out var hastaParseado)) fechaHasta = hastaParseado.Date;

            DataTable dt = QuitarFilasOcultas(_oCtaCteN.obtenerPagos(texto ?? "", fechaDesde, fechaHasta), Entidades.RestriccionCtaCteReservada.TablaPagos, "id");

            // Titular propio (2026-11-09, solo Postgres y solo con 2 o mas titulares): columna y filtro. Un titular NULL es el principal
            // y los cobros no tienen titular (idTitularEfec = 0).
            var titularesPago = ListarTitularesPago();
            bool mostrarTitularPago = titularesPago.Count > 0 && dt.Columns.Contains("idTitular");
            if (mostrarTitularPago)
            {
                int principal = IdTitularPrincipalPago();
                var nombresTitular = titularesPago.ToDictionary(t => t.IdTitular, t => t.Nombre);
                dt.Columns.Add("idTitularEfec", typeof(int));
                dt.Columns.Add("titularNombre", typeof(string));
                foreach (DataRow fila in dt.Rows)
                {
                    bool esPago = string.Equals(Convert.ToString(fila["Operacion"]), "Pago", StringComparison.OrdinalIgnoreCase);
                    int efectivo = !esPago ? 0 : (fila["idTitular"] == DBNull.Value ? principal : Convert.ToInt32(fila["idTitular"]));
                    fila["idTitularEfec"] = efectivo;
                    fila["titularNombre"] = efectivo > 0 && nombresTitular.ContainsKey(efectivo) ? nombresTitular[efectivo] : "";
                }
            }

            // filtros de operacion y eliminados sobre lo ya traido (el repositorio no los conoce)
            var filtros = new List<string>();
            if (mostrarTitularPago && idTitular > 0) filtros.Add("idTitularEfec = " + idTitular.ToString(CultureInfo.InvariantCulture));
            if (string.Equals(operacion, "Cobro", StringComparison.OrdinalIgnoreCase)) filtros.Add("Operacion = 'Cobro'");
            else if (string.Equals(operacion, "Pago", StringComparison.OrdinalIgnoreCase)) filtros.Add("Operacion = 'Pago'");
            if (!verEliminados) filtros.Add("eliminado = false");
            DataView vista = dt.DefaultView;
            vista.RowFilter = string.Join(" AND ", filtros);

            ViewBag.Texto = texto ?? "";
            ViewBag.Desde = fechaDesde.ToString("yyyy-MM-dd");
            ViewBag.Hasta = fechaHasta.ToString("yyyy-MM-dd");
            ViewBag.Operacion = operacion ?? "";
            ViewBag.VerEliminados = verEliminados;
            ViewBag.TitularesPago = titularesPago;
            ViewBag.MostrarTitularPago = mostrarTitularPago;
            ViewBag.IdTitularFiltro = idTitular;
            ViewBag.RenderSinLayout = renderParcial;
            ViewBag.Title = "Pagos y cobros";

            if (renderParcial)
                return PartialView("~/Views/Finanzas/Pagos.cshtml", vista.ToTable());

            return View("~/Views/Finanzas/Pagos.cshtml", vista.ToTable());
        }

        // Cambiar la persona de un pago o eliminarlo son operaciones sensibles (mueven saldos de cta cte):
        // requieren el permiso de edicion de pagos. Al resto de la edicion del pago no se le agrega gate
        // (paridad con el clasico, ver comentario de cabecera del controller). Admin siempre puede.
        private bool PuedeReasignarOEliminarPago(Entidades.Pago pago)
        {
            if (pago == null) return false;
            int idCreador = pago.CreadoPor != null ? pago.CreadoPor.Id : 0;
            return _oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Finanza.AddOrEditPago, pago.Fecha, idCreador);
        }

        // Eliminar un pago o cobro: ademas del permiso de edicion de pagos, quien tiene una caja abierta puede
        // eliminar los pagos/cobros que cargo el mismo en ESA caja (2026-10-13, ver docs/DECISIONS.md "Eliminar pagos
        // y cobros de la propia caja abierta"). Cambiar la persona sigue exigiendo solo el permiso.
        private bool PuedeEliminarPago(Entidades.Pago pago, string posInstanceId)
        {
            if (pago == null) return false;
            return PuedeReasignarOEliminarPago(pago) || EsPagoDeMiCajaAbierta(pago, posInstanceId);
        }

        // Pago/cobro cargado por el operador desde que abrio su caja actual (regla pura en Negocio.PagoEnCajaAbierta).
        // El operador sale de posInstanceId (cuenta de produccion compartida) o es el usuario de sesion; el dueno del
        // egreso de caja del pago (si lo tiene) es el que dice quien lo cargo realmente.
        private bool EsPagoDeMiCajaAbierta(Entidades.Pago pago, string posInstanceId)
        {
            if (pago == null || pago.Id <= 0 || pago.Eliminado) return false;

            var operador = ResolverOperadorPOS(posInstanceId, _usuarioActual);
            var caja = ObtenerCajaAbiertaUsuario(operador);
            if (operador == null || caja == null || caja.Sucursal == null) return false;

            var egreso = _oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Pagos.ToString(), pago.Id);
            int? idCreadorEgreso = egreso != null && egreso.Id > 0 ? (int?)egreso.CreadoPor : null;
            int idSucursalPago = pago.Sucursal != null && pago.Sucursal.idSucursal > 0 ? pago.Sucursal.idSucursal : pago.IdSucursal;

            return Negocio.PagoEnCajaAbierta.Evaluar(
                pago.Creado, pago.IdCreadoPor, idSucursalPago,
                idCreadorEgreso, operador.Id,
                caja.FechaHoraInicio, caja.FechaHoraCierre, caja.Sucursal.IdSucursal);
        }

        // Donde buscar la caja de un pago ya guardado. Solo cuenta lo que mueve una caja: el EGRESO de caja del pago
        // (los pagos hechos desde el POS, tipo 300). Un pago cargado desde Finanzas no genera egreso, no entra en ningun
        // cierre y por eso no se avisa (si no, cada cobro con fecha pasada pediria confirmar sin que el cierre cambie).
        // El egreso queda a nombre del OPERADOR (puede no ser el creador del pago en una cuenta de produccion).
        // Cambios en cajas cerradas (2026-10-06, ver docs/DECISIONS.md).
        private List<Negocio.CajaCerradaConsulta> ConsultasCajaDelPago(Entidades.Pago pago)
        {
            var consultas = new List<Negocio.CajaCerradaConsulta>();
            if (pago == null || pago.Id <= 0) return consultas;

            var egreso = _oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Pagos.ToString(), pago.Id);
            if (egreso != null && egreso.Id > 0)
            {
                int idSucursalEgreso = egreso.Sucursal != null ? egreso.Sucursal.idSucursal : (pago.Sucursal != null ? pago.Sucursal.idSucursal : 0);
                consultas.Add(new Negocio.CajaCerradaConsulta(egreso.CreadoPor, idSucursalEgreso, egreso.Fecha));
            }

            return consultas;
        }

        // Eliminacion logica del pago/cobro (con asiento opuesto en la cta cte). Motivo obligatorio.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "id")]
        public IActionResult EliminarPago(int id, string motivo, bool confirmarCajaCerrada = false, string posInstanceId = "")
        {
            Entidades.Pago pago = id > 0 ? _oCtaCteN.getPagoById(id) : null;
            if (pago == null)
                return Json(new { ok = false, mensaje = "No se encontró el pago o cobro." });

            if (!PuedeEliminarPago(pago, posInstanceId))
                return Json(new { ok = false, mensaje = "No tiene permiso para eliminar pagos o cobros. Sin ese permiso solo puede eliminar los que cargó usted desde que abrió su caja actual." });

            // Cajas YA CERRADAS a las que pertenecia el pago (2026-10-06, ver docs/DECISIONS.md "Cambios en cajas
            // cerradas y reapertura"): eliminarlo deja el egreso opuesto fechado HOY (cae en la caja actual de quien
            // elimina) y la caja original queda sin ese reverso -> inconsistente. Se avisa y se pide confirmar.
            var cajaCerradaN = new Negocio.CajaCerradaServicio(_oCierreN);
            var cajasCerradasPago = cajaCerradaN.Evaluar(ConsultasCajaDelPago(pago).ToArray());
            if (cajasCerradasPago.Count > 0 && !confirmarCajaCerrada)
            {
                return Json(WebCore.Helpers.CajaCerradaRespuesta.Confirmacion(
                    cajasCerradasPago,
                    new List<string> { WebCore.Helpers.CajaCerradaRespuesta.LineaImporte("Se elimina el " + (pago.AProveedor ? "pago" : "cobro"), pago.Importe, 0) },
                    "eliminar",
                    "El egreso opuesto se registra con la fecha de hoy, en la caja actual de quien elimina: la caja cerrada queda sin ese reverso."));
            }

            var (ok, mensaje) = _oCtaCteN.eliminarPagoConContraasiento(id, motivo, _usuarioActual);

            if (ok)
            {
                cajaCerradaN.Registrar(
                    Entidades.AuditoriaCierreCaja.OrigenPago, id, Entidades.AuditoriaCierreCaja.AccionEliminacion,
                    cajasCerradasPago, _usuarioActual.Id, _usuarioActual.Nombre,
                    pago.Importe, 0, pago.FormaPago, null,
                    "Se eliminó el " + (pago.AProveedor ? "pago" : "cobro") + ". Motivo: " + (motivo ?? ""));
            }

            return Json(new
            {
                ok,
                mensaje,
                redirectUrl = Url.Action("Pagos", "Finanzas")
            });
        }

        // Opciones del selector de titular de un pago. Vacia = no se muestra (SQL Server o un solo titular).
        private List<WebCore.Models.TitularCompraOpcionVm> ListarTitularesPago()
        {
            var titularN = WebCore.Infrastructure.NegocioFactory.CrearTitularFiscal(_empresa);
            if (titularN == null || !titularN.TieneVariosTitulares(_empresa.IdEmpresa))
                return new List<WebCore.Models.TitularCompraOpcionVm>();

            return titularN.Listar(_empresa.IdEmpresa, false)
                .Select(t => new WebCore.Models.TitularCompraOpcionVm
                {
                    IdTitular = t.IdTitular,
                    Nombre = t.NombreCorto,
                    EsPrincipal = t.EsPredeterminado,
                    Seleccionable = t.Habilitado && !t.Revocado
                })
                .ToList();
        }

        // Id del titular principal (el que representa un titular NULL en la base), o 0 si no hay.
        private int IdTitularPrincipalPago()
        {
            var titularN = WebCore.Infrastructure.NegocioFactory.CrearTitularFiscal(_empresa);
            var principal = titularN != null ? titularN.ObtenerPrincipal(_empresa.IdEmpresa) : null;
            return principal != null ? principal.IdTitular : 0;
        }

        [HttpGet]
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "idPago")]
        public IActionResult AddOrEditPago(int idPersona, string returnUrl, int idPago = 0, bool desdePos = false, string posInstanceId = "")
        {
            bool renderParcial = desdePos || EsPeticionAjax();
            var sucursales = _oSucursalN.findAll();
            string returnUrlDecodificada = DecodeReturnUrlIfNeeded(returnUrl);
            Entidades.Pago pagoExistente = null;

            // _ModalAltaCheque.cshtml (incluido en esta vista desde 2026-09-05, pago con cheque)
            // necesita ViewBag.Bancos -- mismo dato que ya carga Cheques() para el mismo modal.
            ViewBag.Bancos = _oCtaCteN.getBancos();
            ViewBag.PosInstanceId = posInstanceId;

            if (desdePos)
            {
                // Sin esto la cuenta compartida (sin caja propia) caia al cartel "Debe tener una caja abierta",
                // que parecia un problema de permisos del operador (2026-11-02, ver docs/DECISIONS.md).
                if (OperadorPosVencido(posInstanceId))
                    return StatusCode(403, MensajeOperadorPosVencido);

                var operadorPago = ResolverOperadorPOS(posInstanceId, _usuarioActual);
                var cierreCajaActual = ObtenerCajaAbiertaUsuario(operadorPago);

                // Un pago NUEVO exige la parte de edicion de "POS - Ctas.Ctes Persona"; abrir uno ya guardado es solo
                // consulta (se valida al guardar, ver AddOrEditPagoPost). Va antes del aviso de caja: sin permiso el
                // motivo real es el permiso, no la caja. 2026-11-02.
                if (idPago <= 0)
                {
                    string motivoSinPermiso = ValidarPermisoPagoDesdePos(null, DateTime.Now, operadorPago, cierreCajaActual, posInstanceId, out bool puedeAutorizarPago);
                    if (motivoSinPermiso != null)
                    {
                        // El cuerpo es el de siempre; el encabezado le dice al POS que puede pedir la autorizacion de otro usuario.
                        if (puedeAutorizarPago) MarcarPuedePedirAutorizacion(Negocio.AutorizacionesProvisoriasPos.AccionPago, 0);
                        return StatusCode(403, motivoSinPermiso);
                    }
                }

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
            ViewBag.OcultarSaldo = AplicarReservaAlExtracto(dtMov, idPersona);
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
            // Saldo exacto (negativo = debe, positivo = a favor) que usa el interruptor "Saldar cuenta" del alta.
            // Es el mismo que el POST recalcula, no el del extracto del encabezado.
            ViewBag.SaldoSaldar = _oCtaCteN.obtenerSaldoPersona(idPersona) ?? 0m;
            ViewBag.ImprimirPagoPdfUrl = idPago > 0 ? Url.Action("ImprimirPdfPago", "Finanzas", new { id = idPago }) : "";

            // Pago existente fuera de POS: solo lectura hasta "Modificar"; cambio de persona y eliminacion
            // solo con permiso de edicion de pagos. Un pago eliminado queda solo lectura para siempre.
            bool pagoEliminado = pagoExistente != null && pagoExistente.Eliminado;
            ViewBag.PagoEliminado = pagoEliminado;
            ViewBag.PuedeReasignarOEliminar = pagoExistente != null && !desdePos && !pagoEliminado && PuedeReasignarOEliminarPago(pagoExistente);
            ViewBag.PuedeEliminar = pagoExistente != null && !desdePos && !pagoEliminado && PuedeEliminarPago(pagoExistente, posInstanceId);
            ViewBag.UrlPagosListado = Url.Action("Pagos", "Finanzas");
            ViewBag.UrlEliminarPago = Url.Action("EliminarPago", "Finanzas");
            ViewBag.UrlListarPersonas = Url.Action("Listar", "Personas");

            // Titular propio del pago a proveedor (2026-11-09, solo Postgres y solo con 2 o mas titulares): ver docs/DECISIONS.md.
            ViewBag.TitularesPago = ListarTitularesPago();
            ViewBag.IdTitularPrincipalPago = IdTitularPrincipalPago();

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
            bool desdePos = false,
            string posInstanceId = "",
            bool confirmarCajaCerrada = false,
            bool saldarCuenta = false)
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

            // El binder convierte "" en null; WebCore siempre manda el campo, asi que null = "sin conteo" (vacio).
            // Con '' el SP de SQL Server pisa el conteo anterior (null lo conservaria, pensado para WinForms).
            oPagoE.ConteoBilletes ??= string.Empty;

            oPagoE.Importe = ParseFloat(importe);
            oPagoE.Efectivo = string.IsNullOrEmpty(Efectivo) ? 0 : ParseFloat(Efectivo);

            // "Saldar cuenta" (2026-10-22, ver docs/DECISIONS.md): solo en alta. El servidor recalcula el saldo real y
            // pisa operacion e importe, porque el saldo que vio la pantalla pudo cambiar (otra caja, otra venta).
            // No aplica con cheques: ahi el importe sale de los cheques cargados.
            if (saldarCuenta)
            {
                if (oPagoE.Id > 0)
                    return Json(new { ok = false, mensaje = "Saldar cuenta solo se puede usar al cargar un pago o cobro nuevo." });

                if (oPagoE.FormaPago_.ToString().Contains("Cheque", StringComparison.OrdinalIgnoreCase))
                    return Json(new { ok = false, mensaje = "Saldar cuenta no se puede combinar con una forma de pago con cheques." });

                // Cuenta reservada vista por un usuario restringido: no se le revela el saldo ni siquiera por el importe.
                if (_ctaCteReservada.SaldoOculto(idPersona))
                    return Json(new { ok = false, mensaje = WebCore.Helpers.ProtegerRegistroReservadoFilter.MensajeDenegado });

                var saldar = Negocio.SaldarCuenta.Resolver(_oCtaCteN.obtenerSaldoPersona(idPersona) ?? 0m);
                if (!saldar.PuedeSaldar)
                    return Json(new { ok = false, mensaje = "La cuenta ya está saldada: no hay saldo para cobrar ni pagar." });

                oPagoE.AProveedor = saldar.AProveedor;
                oPagoE.Importe = (float)saldar.Importe;
            }

            // Titular propio (2026-11-09): solo los pagos a proveedor lo llevan; un cobro no tiene titular. El titular tiene que estar
            // habilitado y no cancelado (el principal = NULL se guarda como NULL, igual que en Compras).
            if (!oPagoE.AProveedor)
            {
                oPagoE.IdTitular = null;
            }
            else
            {
                if (oPagoE.IdTitular.HasValue && oPagoE.IdTitular.Value <= 0) oPagoE.IdTitular = null;
                var comprobanteN = WebCore.Infrastructure.NegocioFactory.CrearComprobanteCompra(_empresa);
                string errorTitular = comprobanteN != null ? comprobanteN.ErrorSiTitularNoDisponible(oPagoE.IdTitular) : null;
                if (errorTitular != null)
                    return Json(new { ok = false, mensaje = errorTitular });
            }

            oPagoE.Banco = "";
            oPagoE.NroCheque = "";
            oPagoE.TitularCheque = "";

            Entidades.Pago pagoAnterior = oPagoE.Id > 0 ? _oCtaCteN.getPagoById(oPagoE.Id) : null;
            if (oPagoE.Id > 0 && pagoAnterior == null)
                return Json(new { ok = false, mensaje = "No se encontró el pago o cobro a modificar." });

            if (pagoAnterior != null)
            {
                // Cuenta corriente reservada: no se edita un pago/cobro que el operador no puede ver.
                if (_ctaCteReservada.RegistroOculto(Entidades.RestriccionCtaCteReservada.TablaPagos, pagoAnterior.Id, ResolverOperadorPOS(posInstanceId, _usuarioActual)))
                    return Json(new { ok = false, mensaje = WebCore.Helpers.ProtegerRegistroReservadoFilter.MensajeDenegado });

                if (pagoAnterior.Eliminado)
                    return Json(new { ok = false, mensaje = "El pago o cobro fue eliminado y no puede modificarse." });

                if (idPersona != pagoAnterior.IdPersona && (desdePos || !PuedeReasignarOEliminarPago(pagoAnterior)))
                    return Json(new { ok = false, mensaje = "No tiene permiso para cambiar la persona del pago o cobro." });
            }

            oPagoE.CreadoPor = oPagoE.Id > 0 ? pagoAnterior.CreadoPor : _usuarioActual;
            oPagoE.ActualizadoPor = oPagoE.Id > 0 ? _usuarioActual : oPagoE.ActualizadoPor;
            oPagoE.FormaPago = oPagoE.FormaPago_.ToString();

            // Desde el POS: permiso de edicion de "POS - Ctas.Ctes Persona" y limite de fecha (2026-11-02, ver
            // docs/DECISIONS.md "POS: permisos por funcion"). Va antes de validar el resto: si no puede, el motivo es ese.
            if (desdePos && !OperadorPosVencido(posInstanceId))
            {
                var operadorPermisoPago = ResolverOperadorPOS(posInstanceId, _usuarioActual);
                string motivoSinPermisoPago = ValidarPermisoPagoDesdePos(
                    pagoAnterior, oPagoE.Fecha, operadorPermisoPago, ObtenerCajaAbiertaUsuario(operadorPermisoPago), posInstanceId, out bool puedeAutorizarPago);
                if (motivoSinPermisoPago != null)
                    return Json(puedeAutorizarPago
                        ? (object)new { ok = false, mensaje = motivoSinPermisoPago, requiereAutorizacion = true, accion = Negocio.AutorizacionesProvisoriasPos.AccionPago, idRegistro = pagoAnterior?.Id ?? 0 }
                        : new { ok = false, mensaje = motivoSinPermisoPago });
            }

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
                // Mismo criterio que AddOrEditPago (GET): operador perdido de la Session != "sin caja abierta".
                if (OperadorPosVencido(posInstanceId))
                    return Json(new { ok = false, mensaje = MensajeOperadorPosVencido });

                cierreCajaActual = ObtenerCajaAbiertaUsuario(ResolverOperadorPOS(posInstanceId, _usuarioActual));
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

            // ---- Cajas YA CERRADAS afectadas (2026-10-06, ver docs/DECISIONS.md "Cambios en cajas cerradas y reapertura") ----
            // Un pago pertenece a la caja de (creadopor, sucursal, fecha); si es del POS tambien por su egreso. Se evalua
            // la caja ORIGINAL (si se edita) y la de la fecha/sucursal NUEVA. Alta en una caja cerrada, o cambio de
            // importe/forma de pago/fecha/sucursal/persona sobre un pago de caja cerrada, pide confirmacion; el resto
            // (observaciones...) solo se registra.
            var cajaCerradaN = new Negocio.CajaCerradaServicio(_oCierreN);
            // Solo importa lo que mueve una caja: pagos del POS (con egreso de caja). Un pago de Finanzas sin egreso no
            // entra en ningun cierre, asi que no genera aviso (ver ConsultasCajaDelPago).
            var consultasCajaPago = ConsultasCajaDelPago(pagoAnterior);
            if (consultasCajaPago.Count > 0 || desdePos)
            {
                consultasCajaPago.Add(new Negocio.CajaCerradaConsulta(
                    consultasCajaPago.Count > 0 ? consultasCajaPago[0].IdDueno : ResolverOperadorPOS(posInstanceId, _usuarioActual).Id,
                    oPagoE.Sucursal != null ? oPagoE.Sucursal.idSucursal : 0,
                    oPagoE.Fecha));
            }
            var cajasCerradasPago = cajaCerradaN.Evaluar(consultasCajaPago.ToArray());

            if (cajasCerradasPago.Count > 0 && !confirmarCajaCerrada)
            {
                int sucursalPagoOriginal = pagoAnterior != null && pagoAnterior.Sucursal != null ? pagoAnterior.Sucursal.idSucursal : 0;
                int sucursalPagoNueva = oPagoE.Sucursal != null ? oPagoE.Sucursal.idSucursal : 0;
                bool esAltaPago = pagoAnterior == null;
                bool cambioPersona = pagoAnterior != null && idPersona != pagoAnterior.IdPersona;
                bool afectaPago = esAltaPago || cambioPersona || Negocio.CambioCajaCerrada.HuboCambioQueAfectaCierre(
                    pagoAnterior.Importe, oPagoE.Importe, pagoAnterior.FormaPago, oPagoE.FormaPago,
                    pagoAnterior.Fecha, oPagoE.Fecha, sucursalPagoOriginal, sucursalPagoNueva);

                if (afectaPago)
                {
                    string tipoPago = oPagoE.AProveedor ? "pago" : "cobro";
                    var cambioPago = new List<string>();
                    if (esAltaPago)
                    {
                        cambioPago.Add("Nuevo " + tipoPago + " de $ " + oPagoE.Importe.ToString("N2", new System.Globalization.CultureInfo("es-AR")) + " fechado el " + oPagoE.Fecha.ToString("dd/MM/yyyy HH:mm"));
                    }
                    else
                    {
                        if (Math.Abs(pagoAnterior.Importe - oPagoE.Importe) >= Negocio.CambioCajaCerrada.ToleranciaImporte)
                            cambioPago.Add(WebCore.Helpers.CajaCerradaRespuesta.LineaImporte("Importe del " + tipoPago, pagoAnterior.Importe, oPagoE.Importe));
                        if (!string.Equals((pagoAnterior.FormaPago ?? "").Trim(), (oPagoE.FormaPago ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
                            cambioPago.Add(WebCore.Helpers.CajaCerradaRespuesta.LineaTexto("Forma de pago", pagoAnterior.FormaPago, oPagoE.FormaPago));
                        if (Negocio.CambioCajaCerrada.CambioFecha(pagoAnterior.Fecha, oPagoE.Fecha))
                            cambioPago.Add(WebCore.Helpers.CajaCerradaRespuesta.LineaFecha("Fecha", pagoAnterior.Fecha, oPagoE.Fecha));
                        if (sucursalPagoOriginal != sucursalPagoNueva)
                            cambioPago.Add("Sucursal: " + sucursalPagoOriginal + " → " + sucursalPagoNueva);
                        if (cambioPersona)
                            cambioPago.Add("Se cambia la persona del " + tipoPago);
                    }

                    return Json(WebCore.Helpers.CajaCerradaRespuesta.Confirmacion(
                        cajasCerradasPago, cambioPago, esAltaPago ? "cargar" : "modificar",
                        !esAltaPago && desdePos ? "Si cambia el importe o la forma de pago, el reverso del egreso se registra en la caja original (ya cerrada)." : null));
                }
            }

            // Un fallo inesperado del guardado (base, egreso de caja, etc.) se responde como JSON controlado con el
            // motivo, en vez de un 500 sin cuerpo que la pantalla mostraba como "No se pudo guardar el pago." a secas.
            try
            {
                _ = _oCtaCteN.addOrEditPago(oPagoE, cierreCajaActual, pagoAnterior);

                // Aviso en el cierre de cada caja afectada (tambien si no pidio confirmacion), fuera de la transaccion del pago.
                cajaCerradaN.Registrar(
                    Entidades.AuditoriaCierreCaja.OrigenPago, oPagoE.Id,
                    pagoAnterior == null ? Entidades.AuditoriaCierreCaja.AccionAlta : Entidades.AuditoriaCierreCaja.AccionModificacion,
                    cajasCerradasPago, _usuarioActual.Id, _usuarioActual.Nombre,
                    pagoAnterior != null ? (double?)pagoAnterior.Importe : null, oPagoE.Importe,
                    pagoAnterior != null ? pagoAnterior.FormaPago : null, oPagoE.FormaPago,
                    (oPagoE.AProveedor ? "Pago" : "Cobro") + (pagoAnterior == null ? " nuevo." : " modificado."));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AddOrEditPagoPost: fallo al guardar el pago " + oPagoE.Id + " (desdePos=" + desdePos + "): " + ex);
                return Json(new
                {
                    ok = false,
                    mensaje = "No se pudo guardar el pago: " + ex.GetBaseException().Message
                });
            }

            string urlRetornoDefault = !string.IsNullOrWhiteSpace(returnUrl)
                ? returnUrl
                : Url.Action("CtaCtePersona", "Finanzas", new { idPersona, fechaDesde = DateTime.Today.ToString("yyyy-MM-dd") });

            if (desdePos)
            {
                // Guardado el pago, termina la autorizacion provisoria de otro usuario para cargar/modificar pagos (2026-11-03).
                WebCore.Helpers.AutorizacionProvisoriaPosHelper.Revocar(
                    HttpContext.Session, WebCore.Infrastructure.NegocioFactory.CrearAutorizacionPosRepository(_empresa),
                    posInstanceId, Negocio.AutorizacionesProvisoriasPos.AccionPago, null, "guardado");

                return Json(new
                {
                    ok = true,
                    redirectUrl = urlRetornoDefault,
                    cerrarModalPago = true,
                    pagoId = oPagoE.Id,
                    pdfUrl = Url.Action("ImprimirPdfPago", "Finanzas", new { id = oPagoE.Id }),
                    ticketUrl = Url.Action("ImprimirTicketPago", "Finanzas", new { id = oPagoE.Id }),
                    ticketPayloadUrl = Url.Action("ImprimirTicketPagoPayload", "Finanzas", new { id = oPagoE.Id }),
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
                ticketUrl = Url.Action("ImprimirTicketPago", "Finanzas", new { id = oPagoE.Id }),
                ticketPayloadUrl = Url.Action("ImprimirTicketPagoPayload", "Finanzas", new { id = oPagoE.Id }),
                emailConfigUrl = Url.Action("ObtenerDatosEmailPago", "Finanzas"),
                emailSendUrl = Url.Action("EnviarComprobantePagoEmail", "Finanzas")
            });
        }

        [HttpGet]
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "id")]
        public IActionResult ImprimirPdfPago(int id)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return NotFound();

            byte[] bytes = GenerarDocsCore.GenerarPdfPago(model);
            string nroRecibo = string.IsNullOrWhiteSpace(model.Pago.NroRecibo) ? ("Pago_" + model.Pago.Id) : model.Pago.NroRecibo.Replace("/", "-");
            return File(bytes, "application/pdf", "Recibo_" + nroRecibo + ".pdf");
        }

        // Ticket termico (58/80 mm) del recibo de pago/cobro. HTML = fallback por navegador (iframe oculto de
        // ticket-print.js); el payload es para el agente ESC/POS. Las dos salen de ComprobanteTickets (misma
        // fuente de texto, ver docs/DECISIONS.md 2026-10-05).
        [HttpGet]
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "id")]
        public IActionResult ImprimirTicketPago(int id, int mm = 80)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return NotFound();

            int ticketMm = ComprobanteTickets.NormalizarMm(mm);
            ViewBag.TicketMm = ticketMm;
            ViewBag.TicketTitulo = "Recibo " + (model.Pago.NroRecibo ?? model.Pago.Id.ToString());
            // Sin formato ESC/POS: este HTML lo imprime el navegador y mostraria los caracteres de control.
            ViewBag.TicketLineas = string.Join("\n", ComprobanteTickets.ConstruirLineasPago(model, ticketMm, false));
            return View("~/Views/Shared/_TicketTexto.cshtml");
        }

        [HttpGet]
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "id")]
        public IActionResult ImprimirTicketPagoPayload(int id, int mm = 80)
        {
            var model = ConstruirReciboPagoVm(id);
            if (model == null || model.Pago == null || model.Pago.Id <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el pago o cobro." });

            int ticketMm = ComprobanteTickets.NormalizarMm(mm);
            return Json(new
            {
                ok = true,
                ticketMm,
                ticketLines = ComprobanteTickets.ConstruirLineasPago(model, ticketMm, true)
            });
        }

        [HttpGet]
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "id")]
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
        [WebCore.Helpers.ProtegerRegistroReservado(Entidades.RestriccionCtaCteReservada.TablaPagos, "idPago")]
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

                string fromName = SmtpMailHelper.BuildTenantFromName(nombreEmpresa);
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
                    if (_ctaCteReservada.RegistroOculto(Entidades.RestriccionCtaCteReservada.TablaPagos, idTabla))
                        return DenegarRegistroReservado();
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

            // Persona reservada + usuario restringido: el recibo no muestra el saldo de la cuenta.
            bool saldoReservado = _ctaCteReservada.SaldoOculto(pago.Persona.IdPersona);

            if (!saldoReservado && dtCtaCte != null && dtCtaCte.Rows.Count > 0)
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
                // El asiento opuesto de un pago eliminado ("ELIMINADO - ...") no compite por ser "el ultimo":
                // se muestra siempre junto al asiento vigente del pago, para que se vea la pareja
                // original / eliminado en vez de solo un movimiento con el signo invertido.
                if (EsAsientoDePagoEliminado(row)) continue;

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
                if (EsAsientoDePagoEliminado(row) || (maxIdPorClave.TryGetValue(clave, out idMaximo) && id == idMaximo))
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        // Contra-asiento que genera Negocio.CuentaCorriente.eliminarPagoConContraasiento (detalle "ELIMINADO - ...").
        private static bool EsAsientoDePagoEliminado(DataRow row)
        {
            return string.Equals(Convert.ToString(row["tabla"]), "Pagos", StringComparison.OrdinalIgnoreCase)
                && Convert.ToString(row["detalle"]).StartsWith("ELIMINADO - ", StringComparison.Ordinal);
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

        // Extracto de la persona. Con 2 o mas titulares propios (solo Postgres) cada movimiento trae su titular (columna "idTitular") y
        // se puede filtrar por uno; el saldo anterior y el acumulado salen solo de lo filtrado. Si no, es el extracto de siempre.
        private DataTable ObtenerExtractoPersona(int idPersona, DateTime desde, int idTitular, out bool conTitulares)
        {
            var repo = WebCore.Infrastructure.NegocioFactory.CrearResumenProveedor(_empresa);
            conTitulares = repo != null && ListarTitularesPago().Count > 0;
            if (!conTitulares)
                return _oCtaCteN.getCtaCteByIdPersona(idPersona, desde);

            DataTable extracto = repo.ObtenerExtractoPorTitular(idPersona, desde, idTitular > 0 ? idTitular : 0);
            Negocio.ResumenCtaCtePorTitular.AcumularSaldo(extracto);
            return extracto;
        }

        // Usuario cuyos permisos valen para consultar la cuenta corriente: en la cuenta de produccion (compartida) es el operador
        // autenticado de Finanzas; sin operador queda la propia cuenta, que no suma permisos de mas.
        private Entidades.Usuario UsuarioConsultaCtaCte()
        {
            if (_usuarioActual.EsUsuarioProduccion)
                return ObtenerOperadorFinanzas() ?? _usuarioActual;
            return _usuarioActual;
        }

        // Resumen por titular de la cuenta corriente de un proveedor. "Control fiscal" (facturado, pagado bancarizado y en efectivo,
        // diferencia y sin factura) solo si permitirControlFiscal Y el usuario es admin o puede EDITAR el formulario Pagos del menu
        // (formPagos): el permiso del POS no cuenta. Sin permiso esos numeros no se leen ni viajan en el modelo.
        private WebCore.Models.CtaCteTitularesVm ArmarTitularesCtaCte(int idPersona, DateTime desde, int idTitular, Entidades.Usuario usuarioConsulta, bool ocultar, bool permitirControlFiscal)
        {
            var vm = new WebCore.Models.CtaCteTitularesVm
            {
                Mostrar = true,
                IdTitularFiltro = idTitular,
                Titulares = ListarTitularesPago()
            };
            vm.Nombres = vm.Titulares.ToDictionary(t => t.IdTitular, t => t.Nombre);
            // Cuenta reservada vista por un usuario restringido: sin resumen (revelaria saldos).
            if (ocultar) return vm;

            var repo = WebCore.Infrastructure.NegocioFactory.CrearResumenProveedor(_empresa);
            if (repo == null) return vm;

            bool puedeControl = permitirControlFiscal
                && _oUsuarioN.PuedeEditarFormularioPorConsulta(usuarioConsulta, Entidades.Permisos.Finanza.VerPagos);
            vm.PuedeVerControlFiscal = puedeControl;

            var movimientos = repo.ObtenerMovimientos(idPersona);
            var compras = movimientos.Where(m => m.Tabla == Negocio.ResumenCtaCtePorTitular.TablaCompras).ToList();
            // Sin compras la persona no es proveedor: no hay resumen que mostrar.
            if (compras.Count == 0) return vm;

            vm.PrimeraCompra = compras.Min(m => m.Fecha);
            var pagos = puedeControl ? repo.ObtenerPagos(idPersona, desde) : new List<Entidades.PagoResumen>();
            var facturado = puedeControl ? repo.ObtenerFacturado(idPersona, desde) : new List<Entidades.FacturadoResumen>();
            var resumen = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, pagos, facturado, desde, vm.Nombres, idTitular);

            // Sin permiso se borra todo lo que sale del control fiscal (SinFactura sale de Comprado, que si se ve).
            if (!puedeControl)
            {
                foreach (var fila in resumen.Filas.Concat(new[] { resumen.Total }))
                {
                    fila.FacturadoNeto = 0m;
                    fila.PagadoBancarizado = 0m;
                    fila.PagadoEfectivo = 0m;
                    fila.DiferenciaFacturadoBancarizado = 0m;
                    fila.SinFactura = 0m;
                    fila.PagadoBancarizadoExcedeFacturado = false;
                }
            }
            vm.Resumen = resumen;
            return vm;
        }

        private DataTable ObtenerMovimientosCuentaCorrienteParaExportacion(int idPersona, DateTime fecha, bool mostrarAnulados, int idTitular, out bool conTitulares)
        {
            DataTable dt = ObtenerExtractoPersona(idPersona, fecha, idTitular, out conTitulares);
            AplicarReservaAlExtracto(dt, idPersona);
            if (!mostrarAnulados)
                dt = FiltrarRegistrosRepetidos(dt);

            dt = OrdenarMovimientosCuentaCorrientePorFecha(dt, true);
            return dt;
        }

        private byte[] GenerarExcelCuentaCorrienteBytes(int idPersona, DateTime fecha, bool mostrarAnulados, int idTitular, bool incluirControlFiscal, out string fileName)
        {
            bool conTitulares;
            DataTable dt = ObtenerMovimientosCuentaCorrienteParaExportacion(idPersona, fecha, mostrarAnulados, idTitular, out conTitulares);
            bool ocultarSaldo = _ctaCteReservada.SaldoOculto(idPersona);
            var titulares = conTitulares
                ? ArmarTitularesCtaCte(idPersona, fecha, idTitular, UsuarioConsultaCtaCte(), ocultarSaldo, incluirControlFiscal)
                : null;
            string csv = "Fecha;Tabla;Nro.Doc;Detalle;Importe;Saldo;Sucursal" + (conTitulares ? ";Titular" : "") + "\n";

            foreach (DataRow r in dt.Rows)
            {
                // Solo compras y pagos llevan titular propio; el resto de los movimientos (ventas, etc.) no.
                string tablaFila = Convert.ToString(r["Tabla"]);
                string titularFila = "";
                if (conTitulares && (string.Equals(tablaFila, "Compras", StringComparison.OrdinalIgnoreCase) || string.Equals(tablaFila, "Pagos", StringComparison.OrdinalIgnoreCase)))
                {
                    int idTitularFila = r["idTitular"] == DBNull.Value ? 0 : Convert.ToInt32(r["idTitular"]);
                    titulares.Nombres.TryGetValue(idTitularFila, out titularFila);
                }

                csv += Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy") + ";" +
                       r["Tabla"] + ";" +
                       r["NroDoc"] + ";" +
                       r["Detalle"] + ";" +
                       Convert.ToDecimal(r["Importe"]).ToString("N2") + ";" +
                       (ocultarSaldo ? "***" : Convert.ToDecimal(r["Saldo"]).ToString("N2")) + ";" +
                       r["Sucursal"] + (conTitulares ? ";" + titularFila : "") + "\n";
            }

            // Resumen por titular al pie (con el control fiscal solo si corresponde).
            if (titulares != null && titulares.Resumen != null)
            {
                csv += "\nResumen por titular\n";
                csv += "Titular;Saldo anterior;Comprado;Pagado;Saldo" + (titulares.PuedeVerControlFiscal ? ";Facturado neto;Pagado bancarizado;Pagado efectivo;Facturado menos bancarizado;Sin factura" : "") + "\n";
                foreach (var fila in titulares.Resumen.Filas.Concat(new[] { titulares.Resumen.Total }))
                {
                    csv += fila.NombreTitular + ";" + fila.SaldoAnterior.ToString("N2") + ";" + fila.Comprado.ToString("N2") + ";" + fila.Pagado.ToString("N2") + ";" + fila.Saldo.ToString("N2");
                    if (titulares.PuedeVerControlFiscal)
                        csv += ";" + fila.FacturadoNeto.ToString("N2") + ";" + fila.PagadoBancarizado.ToString("N2") + ";" + fila.PagadoEfectivo.ToString("N2") + ";" + fila.DiferenciaFacturadoBancarizado.ToString("N2") + ";" + fila.SinFactura.ToString("N2");
                    csv += "\n";
                }
            }

            string persona = ObtenerNombreArchivoPersona(dt, idPersona);
            fileName = "CuentaCorriente_" + persona + "_Desde_" + fecha.ToString("yyyy-MM-dd") + ".csv";
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }

        private byte[] GenerarPdfCuentaCorrienteBytes(int idPersona, DateTime fecha, bool mostrarAnulados, int idTitular, bool incluirControlFiscal, out string fileName)
        {
            bool conTitulares;
            DataTable dtMov = ObtenerMovimientosCuentaCorrienteParaExportacion(idPersona, fecha, mostrarAnulados, idTitular, out conTitulares);
            var titulares = conTitulares
                ? ArmarTitularesCtaCte(idPersona, fecha, idTitular, UsuarioConsultaCtaCte(), _ctaCteReservada.SaldoOculto(idPersona), incluirControlFiscal)
                : null;
            string persona = dtMov.Rows.Count > 0 ? Convert.ToString(dtMov.Rows[0]["razonSocial"]) : "";

            persona = SanitizarNombreArchivo(persona);
            fileName = "CuentaCorriente_" + persona + "_Desde_" + fecha.ToString("yyyy-MM-dd") + ".pdf";
            return WebCore.Services.GenerarDocsCore.GenerarPdfCtaCtePersona(dtMov, fecha, _ctaCteReservada.SaldoOculto(idPersona), titulares);
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
