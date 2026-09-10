// Port de Web/Controllers/VentasController.cs (3336 lineas en el original) para el Modulo 8
// (Ventas y POS) -- ver docs/DECISIONS.md. Header actualizado 2026-09-04: las entregas de este
// controller fueron incrementales (varios slices + el nucleo POS de PLAN-POS.md + la UI de POS
// de PLAN-POS-UI.md) y el comentario original quedo desactualizado varias veces -- este resumen
// refleja el estado real verificado, no lo que decia una nota vieja.
//
// PORTADO Y VERIFICADO CON DATOS REALES: Index/Facturas/Lineas/MisVentas/DetalleVenta/
// DetalleFactura (listados y detalle), POS (GET, venta siempre nueva, sin Session -- ver
// PLAN-POS-UI.md), FinalizarVenta/ModificarVenta/BuscarProducto (nucleo POS, PLAN-POS.md),
// BuscarExpendiosPOS/ObtenerExpendioPOS (expendios asociados, con filtros avanzados y carga
// cross-sucursal agregados a pedido del usuario, ver docs/DECISIONS.md), NuevaFacturaSinVenta/
// CrearVentaManualParaFactura/GenerarFactura/LimpiarLineasVentaManual (flujo AFIP "facturar sin
// venta", probado contra produccion real de AFIP), Imprimir/ObtenerDatosEmailComprobante/
// EnviarComprobanteEmail (PDF con QuestPDF + email real, ya portados de un slice anterior),
// ImprimirTicketHtml (ticket termico HTML 58/80mm, retomado 2026-09-06, ver docs/DECISIONS.md).
//
// SIN PORTAR (confirmado, no una lista vieja):
//  - AFIP: ProbarLoginAfip, CerrarVentaSinFacturar. GenerarNotaCredito SI se porto (2026-09-05,
//    ver docs/DECISIONS.md), sin la opcion "AnularVenta" del original (clona la venta entera como
//    anulada -- pieza separada, no necesaria para probar que la nota de credito real funciona).
//  - Operador de produccion (cuenta compartida, Session-heavy, pospuesto): AutorizarOperadorPOS,
//    CerrarOperadorPOS, AutorizarModuloVentas, AutorizarOperadorModuloVentas.
//  - AgregarProducto: confirmado codigo muerto en el original (PLAN-POS.md seccion 2), no se
//    porta nunca.
//  - Impresion con agente local ESC/POS (distinto del PDF y del ticket HTML, que si estan
//    portados -- Imprimir, ImprimirTicketHtml): ImprimirTicketPayload, ImprimirIngresoBilletesPayload,
//    DescargarAgenteImpresion. ImprimirTicket (el nombre clasico) esta ocupado en este controller
//    por el modal de Factura Electronica (ver su propio comentario) -- el ticket termico HTML real
//    (58/80mm, mismo diseño de _TicketHTML.cshtml, retomado 2026-09-06) vive en ImprimirTicketHtml,
//    ruta nueva para no pisar esa URL.
//
// Consecuencia visible en las vistas: los botones que dependen de lo no portado (AFIP automatica
// desde post-venta, ticket ESC/POS, operador de produccion) se EXCLUYEN de las vistas en vez de
// dejarlos wireados a una accion inexistente -- mismo criterio que FinanzasController excluyo
// CtaCtePersona/AddOrEditPago en cascada.
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06) -- ya no hay stub hardcodeado. Bypass de permisos: los chequeos de
// PermisosHelper.TienePermiso*/VistaAccesoDenegado/ConfigurarAdvertenciaFechaEnVivo del original
// se omiten directamente -- TODO(claude): revisar en Batch 4/5. PuedeModificarUltimaVenta,
// PuedeCambiarFormaPago, TienePermisoAdministrativoSobreVenta (permisos reales de Venta + usuario
// produccion) son justamente el Batch 5 del plan de login/permisos reales, ver
// docs/10-migracion-aspnet-core/gaps.md -- todavia no portados, es el trabajo mas grande que
// queda de ese plan.
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
// portados sin cambios de logica fiscal.
//
// AGREGADO 2026-09-05 (autorizado explicitamente por el usuario para probar con montos reales
// chicos, ver docs/DECISIONS.md): GenerarNotaCredito -- mismo AFIP.GenerarFacturaService, solo
// esNotaCredito=true. ProbarLoginAfip/CerrarVentaSinFacturar siguen sin portar, no los necesita
// ningun flujo actual.
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.CierreCaja _oCierreN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.BarcodeInterpreter _oBarcodeInterpreter;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public VentasController(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, WebCore.Services.IUsuarioSesionService sesion)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _env = env;
            _sesion = sesion;
            _empresa = sesion.Empresa;

            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oVentaN = WebCore.Infrastructure.NegocioFactory.CrearVenta(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oBarcodeInterpreter = WebCore.Infrastructure.NegocioFactory.CrearBarcodeInterpreter(_empresa, _param);
        }

        // ===== Permisos reales de Venta + "usuario de produccion" (Batch 5 del plan de login/
        // permisos reales, 2026-09-06, ver docs/DECISIONS.md) -- port literal de
        // Web/Controllers/VentasController.cs:2497-2686 y Web/Controllers/BaseController.cs:
        // 293-445, adaptado a IUsuarioSesionService/ISession en vez de Session["Usuario"]/
        // HttpSessionStateBase. =====

        // "La ultima venta del cierre del vendedor" resuelve por el CierreCaja mas reciente de esa
        // sucursal+usuario (findByIdOrLast/FindLast) que todavia no tiene UsuarioCierre asignado.
        private Entidades.CierreCaja ObtenerCierreCajaActual(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0) return null;
            if (user.Sucursal == null) user.Sucursal = _oSucursalN.findById(user.IdSucursal);
            if (user.Sucursal == null) return null;

            var cierre = new Entidades.CierreCaja { Sucursal = user.Sucursal, UsuarioInicio = user };
            cierre = _oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            bool cajaAbierta = cierre != null && (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
            return cajaAbierta ? cierre : null;
        }

        // Pedido explicito del usuario (2026-09-07, ver docs/DECISIONS.md "Editar fecha de venta en
        // curso + saltear caja abierta"): un usuario con permiso real de "modificar venta"
        // (mismo permiso Entidades.Permisos.Venta.UltimaVenta que ya gatea PuedeModificarUltimaVenta
        // para ventas YA GUARDADAS) puede, ademas, operar el POS sin caja abierta y editar la fecha
        // de la venta EN CURSO (todavia sin finalizar) -- se advierte de inconsistencias, queda a su
        // criterio, no se bloquea. Para una venta en curso (sin Id todavia) el "creador" relevante es
        // el propio usuario -- mismo criterio que TienePermisoAdministrativoSobreVenta usa con
        // venta.Vendedor.Id para una venta ya guardada.
        private bool PuedeOperarSinCajaYEditarFecha(Entidades.Usuario user)
        {
            if (user == null) return false;
            if (user.Admin) return true;

            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Venta.UltimaVenta, DateTime.Now, user.Id);
        }

        private bool TienePermisoAdministrativoSobreVenta(Entidades.Venta venta, Entidades.Usuario user = null)
        {
            if (venta == null) return false;
            user = user ?? _usuarioActual;
            if (user == null) return false;

            int idCreador = venta.Vendedor != null ? venta.Vendedor.Id : -1;
            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Venta.UltimaVenta, venta.FechaVenta, idCreador);
        }

        private bool PuedeModificarUltimaVenta(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null) return false;
            user = user ?? _usuarioActual;
            if (user == null) return false;

            if (TienePermisoAdministrativoSobreVenta(venta, user)) return true;

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.UsuarioInicio == null) return false;

            var ultimaVenta = _oVentaN.getUltimaVentaVendedor(cierre);
            if (ultimaVenta == null || ultimaVenta.IdVenta != venta.IdVenta) return false;

            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Venta.UltimaVenta, venta.FechaVenta, cierre.UsuarioInicio.Id);
        }

        private string ObtenerMotivoNoPuedeModificarUltimaVenta(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null) return "Venta inválida.";
            user = user ?? _usuarioActual;
            if (user == null) return "Sesión expirada.";

            if (TienePermisoAdministrativoSobreVenta(venta, user)) return "";

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.UsuarioInicio == null) return "No tenés una caja abierta.";

            var ultimaVenta = _oVentaN.getUltimaVentaVendedor(cierre);
            if (ultimaVenta == null || ultimaVenta.IdVenta != venta.IdVenta)
                return "Solo se puede modificar la última venta de tu cierre de caja actual.";

            return "No tenés permisos para modificar esta venta.";
        }

        private bool PuedeEditarFechaVenta(Entidades.Venta venta, DateTime? fechaSeleccionada = null, Entidades.Usuario user = null)
        {
            if (venta == null) return false;
            user = user ?? _usuarioActual;
            if (user == null) return false;

            int vendedorId = venta.Vendedor != null ? venta.Vendedor.Id : -1;
            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Venta.NuevaVenta, fechaSeleccionada ?? venta.FechaVenta, vendedorId);
        }

        private bool PuedeCambiarFormaPago(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null) return false;
            user = user ?? _usuarioActual;
            if (user == null || user.IdSucursal == 0) return false;

            if (TienePermisoAdministrativoSobreVenta(venta, user)) return true;

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.FechaHoraInicio == null || cierre.UsuarioInicio == null) return false;
            if (venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal) return false;
            if (venta.Vendedor == null || venta.Vendedor.Id != cierre.UsuarioInicio.Id) return false;

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            return venta.FechaVenta >= inicio && venta.FechaVenta <= fin;
        }

        // Historial de precios de cliente (F8, 2026-09-06 -- ver docs/DECISIONS.md). Mismo criterio
        // que FinanzasController.PuedeVerSaldosCuentaCorriente: admin pasa siempre, si no, requiere
        // el permiso de ver cuentas corrientes completas. Port literal de
        // Web/Controllers/VentasController.cs:967-976 (se replica el helper por controller, no se
        // centraliza -- mismo patron ya usado en este repo).
        private bool PuedeVerCtaCteCompleta(Entidades.Usuario usuario)
        {
            if (usuario == null) return false;
            if (usuario.Admin) return true;
            return _oUsuarioN.tienePermiso(usuario, Entidades.Permisos.Finanza.VerCtasCtes, DateTime.Today, -1);
        }

        // GET: Ventas/HistorialPreciosCliente
        // Historial de "ultimo precio por producto" de un cliente, sobre sus ultimas N ventas --
        // para el boton/atajo F8 del POS (2026-09-06, retomado -- ver docs/DECISIONS.md). Port
        // literal de Web/Controllers/VentasController.cs:978-1023. obtenerUltimosPreciosPorCliente
        // ya existe en Negocio/Venta.cs (compartido), no hubo que tocar la capa de negocio.
        [HttpGet]
        public PartialViewResult HistorialPreciosCliente(int idPersona, int topVentas = 10)
        {
            var user = _usuarioActual;
            if (user == null)
            {
                ViewBag.HistorialPreciosError = "La sesión expiró. Recargá la página para continuar.";
                return PartialView("~/Views/Ventas/_HistorialPreciosClientePOS.cshtml", (List<WebCore.Models.HistorialPrecioProductoVm>)null);
            }

            var persona = _oPersonaN.findById(idPersona);
            if (persona == null || persona.IdPersona <= 0)
            {
                ViewBag.HistorialPreciosError = "No se encontró el cliente.";
                return PartialView("~/Views/Ventas/_HistorialPreciosClientePOS.cshtml", (List<WebCore.Models.HistorialPrecioProductoVm>)null);
            }

            if (persona.CtaCte && !PuedeVerCtaCteCompleta(user))
            {
                ViewBag.HistorialPreciosError = "No tenés permiso para ver el historial de precios de este cliente.";
                return PartialView("~/Views/Ventas/_HistorialPreciosClientePOS.cshtml", (List<WebCore.Models.HistorialPrecioProductoVm>)null);
            }

            DataTable dt = _oVentaN.obtenerUltimosPreciosPorCliente(idPersona, topVentas);
            var model = new List<WebCore.Models.HistorialPrecioProductoVm>();
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    model.Add(new WebCore.Models.HistorialPrecioProductoVm
                    {
                        Codigo = row["codigo"] == DBNull.Value ? "" : Convert.ToString(row["codigo"]),
                        Producto = row["producto"] == DBNull.Value ? "" : Convert.ToString(row["producto"]),
                        PrecioKg = row["precioKg"] == DBNull.Value ? 0f : Convert.ToSingle(row["precioKg"]),
                        FechaVenta = row["fechaVenta"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["fechaVenta"])
                    });
                }
            }

            return PartialView("~/Views/Ventas/_HistorialPreciosClientePOS.cshtml", model);
        }

        private string ObtenerMotivoNoPuedeCambiarFormaPago(Entidades.Venta venta, Entidades.Usuario user = null, Entidades.CierreCaja cierre = null)
        {
            if (venta == null) return "Venta inválida.";
            user = user ?? _usuarioActual;
            if (user == null || user.IdSucursal == 0) return "Sesión expirada.";

            if (TienePermisoAdministrativoSobreVenta(venta, user)) return "";

            cierre = cierre ?? ObtenerCierreCajaActual(user);
            if (cierre == null || cierre.FechaHoraInicio == null || cierre.UsuarioInicio == null) return "No tenés una caja abierta.";
            if (venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal) return "La venta pertenece a otra sucursal.";
            if (venta.Vendedor == null || venta.Vendedor.Id != cierre.UsuarioInicio.Id) return "La venta no pertenece a tu cierre de caja actual.";

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            if (venta.FechaVenta < inicio || venta.FechaVenta > fin)
                return "La venta quedó fuera del rango de tu cierre de caja actual.";

            return "No tenés permisos para cambiar la forma de pago.";
        }

        // Resuelve quien queda como creador/operador real de una operacion cuando el usuario
        // logueado es la cuenta compartida de sala de produccion: si hay un operador ya
        // autorizado en Session para este posInstanceId/modulo, se usa ese; para cualquier
        // usuario normal (EsUsuarioProduccion=false) devuelve el usuario de sesion sin cambios --
        // cero impacto en el comportamiento fuera de la sala de produccion.
        private Entidades.Usuario ResolverOperadorPOS(string posInstanceId, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion) return usuarioSesion;
            return ObtenerOperadorPOS(posInstanceId) ?? usuarioSesion;
        }

        private Entidades.Usuario ResolverOperadorModulo(string modulo, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion) return usuarioSesion;
            return ObtenerOperadorModulo(modulo) ?? usuarioSesion;
        }

        // Claves de Session (ASP.NET Core ISession solo guarda string/byte[] -- se serializa el
        // Entidades.Usuario resuelto a JSON, a diferencia del clasico que guardaba el objeto tal
        // cual en HttpSessionStateBase). Mismo criterio de nombres de clave que
        // Web/Helpers/PermisosHelper.cs (ClaveSessionOperadorPOS/ClaveSessionOperadorModulo).
        private static string ClaveSessionOperadorPOS(string posInstanceId) => "OperadorPOS_" + (posInstanceId ?? "");
        private static string ClaveSessionOperadorModulo(string modulo) => "OperadorModulo_" + (modulo ?? "");

        // ASP.NET Core Session solo manda el Set-Cookie de sesion al browser la PRIMERA VEZ que
        // se escribe algo -- leer HttpContext.Session.Id antes de eso devuelve un id "fantasma"
        // que cambia en cada request (nunca persiste), lo que rompe por completo el rate-limit
        // por sesion (cada intento fallido cae en una key distinta, nunca acumula). Bug real
        // encontrado en la verificacion de este mismo batch: 4 intentos con clave incorrecta NO
        // bloqueaban el 5to. Fix: forzar un primer write (idempotente) antes de leer el Id.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        private void RegistrarOperadorPOS(string posInstanceId, Entidades.Usuario operador)
        {
            HttpContext.Session.SetString(ClaveSessionOperadorPOS(posInstanceId), System.Text.Json.JsonSerializer.Serialize(operador));
        }

        private Entidades.Usuario ObtenerOperadorPOS(string posInstanceId)
        {
            var json = HttpContext.Session.GetString(ClaveSessionOperadorPOS(posInstanceId));
            return string.IsNullOrEmpty(json) ? null : System.Text.Json.JsonSerializer.Deserialize<Entidades.Usuario>(json);
        }

        private void LimpiarOperadorPOS(string posInstanceId)
        {
            HttpContext.Session.Remove(ClaveSessionOperadorPOS(posInstanceId));
        }

        private void RegistrarOperadorModulo(string modulo, Entidades.Usuario operador)
        {
            HttpContext.Session.SetString(ClaveSessionOperadorModulo(modulo), System.Text.Json.JsonSerializer.Serialize(operador));
        }

        private Entidades.Usuario ObtenerOperadorModulo(string modulo)
        {
            var json = HttpContext.Session.GetString(ClaveSessionOperadorModulo(modulo));
            return string.IsNullOrEmpty(json) ? null : System.Text.Json.JsonSerializer.Deserialize<Entidades.Usuario>(json);
        }

        // Valida usuario+clave de un empleado real (nunca la cuenta compartida) y, si tiene
        // permiso de Ventas, lo deja registrado como operador de este posInstanceId. Rate-limit
        // por sesion (no por usuario/IP): los intentos son totales para esta pestaña de POS, sin
        // importar a que usuario del combo se le probo la contraseña.
        private JsonResult ValidarOperadorPOS(int idUsuario, string clave, string posInstanceId, bool exigirPermisoVentas = true)
        {
            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return Json(new { ok = false, bloqueado = true, segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds) });

            const string mensajeGenerico = "Usuario o contraseña incorrectos, o el usuario no tiene permiso de Ventas.";

            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(posInstanceId))
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var candidato = _oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var validado = _oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            bool tienePermiso = !exigirPermisoVentas ||
                (validado != null && _oUsuarioN.tienePermiso(validado, Entidades.Permisos.Venta.NuevaVenta, DateTime.Now, validado.Id));
            if (validado == null || !validado.Activo || !tienePermiso)
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            WebCore.Helpers.PosOperadorStepUpRateLimiter.Reset(sessionId);
            RegistrarOperadorPOS(posInstanceId, validado);
            return Json(new { ok = true, nombre = validado.Nombre });
        }

        private JsonResult ValidarOperadorModulo(int idUsuario, string clave, string modulo)
        {
            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return Json(new { ok = false, bloqueado = true, segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds) });

            const string mensajeGenerico = "Usuario o contraseña incorrectos.";
            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(modulo))
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var candidato = _oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var validado = _oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            if (validado == null || !validado.Activo)
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            WebCore.Helpers.PosOperadorStepUpRateLimiter.Reset(sessionId);
            RegistrarOperadorModulo(modulo, validado);
            return Json(new { ok = true, nombre = validado.Nombre });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AutorizarOperadorPOS(int idUsuario, string clave, string posInstanceId)
        {
            return ValidarOperadorPOS(idUsuario, clave, posInstanceId);
        }

        [HttpPost]
        public JsonResult CerrarOperadorPOS(string posInstanceId)
        {
            LimpiarOperadorPOS(posInstanceId);
            return Json(new { ok = true });
        }

        [HttpGet]
        public IActionResult AutorizarModuloVentas(string returnUrl)
        {
            if (_usuarioActual == null)
                return RedirectToAction("Index", "Login");

            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.ReturnUrlModuloVentas = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home") : returnUrl;
            ViewBag.Title = "Autorizar operador";
            ViewBag.Seccion = "Ventas";
            return View("~/Views/Ventas/AutorizarModulo.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AutorizarOperadorModuloVentas(int idUsuario, string clave)
        {
            return ValidarOperadorModulo(idUsuario, clave, "Ventas");
        }

        // Port de Web/Controllers/BaseController.cs:272-285 -- combo de usuarios activos de la
        // empresa para el modal de seleccion de operador (POS y modulo).
        private List<object> ObtenerUsuariosActivosEmpresaParaCombo()
        {
            var dt = _oUsuarioN.obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre"))
                return new List<object>();

            return dt.AsEnumerable()
                .Select(row => new { id = ValorInt(row, "id"), nombre = ValorString(row, "nombre") })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }

        private int ValorInt(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0;

            int valor;
            return int.TryParse(Convert.ToString(row[columna]), out valor) ? valor : 0;
        }

        private string ValorString(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return "";

            return Convert.ToString(row[columna]) ?? "";
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
            // Usuario de produccion: exige operador autorizado antes de entrar (Batch 5, ver
            // docs/DECISIONS.md "Login de operador para el modulo Ventas"). Port literal de
            // Web/Controllers/VentasController.cs:63-68.
            if (_usuarioActual.EsUsuarioProduccion && ObtenerOperadorModulo("Ventas") == null)
                return RedirectToAction("AutorizarModuloVentas", new { returnUrl = Request.Path + Request.QueryString });

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
            // Usuario de produccion: mismo gate que Index (Batch 5, ver docs/DECISIONS.md).
            if (_usuarioActual.EsUsuarioProduccion && ObtenerOperadorModulo("Ventas") == null)
                return RedirectToAction("AutorizarModuloVentas", new { returnUrl = Request.Path + Request.QueryString });

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
            // Usuario de produccion: exige operador autorizado SOLO en la pantalla completa -- no
            // en modo "modal" (embebido dentro de POS, que ya tiene su propio operador de POS
            // autenticado). Port literal de Web/Controllers/VentasController.cs:402-409.
            if (!modal && _usuarioActual.EsUsuarioProduccion && ObtenerOperadorModulo("Ventas") == null)
                return RedirectToAction("AutorizarModuloVentas", new { returnUrl = Request.Path + Request.QueryString });

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
            // Permisos reales (Batch 5, 2026-09-06, ver docs/DECISIONS.md) -- port literal de
            // Web/Controllers/VentasController.cs:454-457.
            ViewBag.PuedeModificarVenta = PuedeModificarUltimaVenta(venta);
            ViewBag.MotivoNoPuedeModificarVenta = (bool)ViewBag.PuedeModificarVenta ? "" : ObtenerMotivoNoPuedeModificarUltimaVenta(venta);
            ViewBag.PuedeCambiarFormaPago = PuedeCambiarFormaPago(venta);
            ViewBag.MotivoNoPuedeCambiarFormaPago = (bool)ViewBag.PuedeCambiarFormaPago ? "" : ObtenerMotivoNoPuedeCambiarFormaPago(venta);

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
        // facturacion manual (sin venta de productos real detras, ver docs/DECISIONS.md). Modal de
        // Factura Electronica (2026-09-06, retomado -- ver docs/DECISIONS.md): ahora SI devuelve la
        // vista rica (_FacturaElectronica.cshtml, port literal de Web/Views/Ventas/
        // _FacturaElectronica.cshtml) en vez de JSON -- gap ya cerrado, WebCore.VentasFacturaModal
        // la consume igual que el clasico (GET + inyeccion AJAX en #contenedorFacturaElectronica).
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
            ViewBag.AlicuotasIva = alicuotasDt.AsEnumerable()
                .Select(r => new WebCore.Models.AlicuotaIvaVm { IdIva = Convert.ToInt32(r["idIva"]), Iva = Convert.ToDouble(r["iva"]) })
                .ToList();

            ViewBag.SucursalNombreFactura = !string.IsNullOrWhiteSpace(sucursal.SucursalNombre) ? sucursal.SucursalNombre : sucursal.sucursal;
            ViewBag.EsSinVenta = true;

            return PartialView("~/Views/Ventas/_FacturaElectronica.cshtml", dto);
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

                // Editar una factura ya emitida (F.C. Electronica, boton "Factura" del post-venta,
                // 2026-09-06 -- ver docs/DECISIONS.md). Port literal de Web/Controllers/
                // VentasController.cs:1967-2012, gap real detectado al portar la vista rica: sin
                // esta rama, reenviar el form con dto.IdFactura>0 caia en el chequeo de idempotencia
                // de abajo y devolvia {ok:true, already:true} SIN GUARDAR los cambios de FormaPago/
                // Observaciones/DescItemUnitario que la UI si deja editar cuando ya-emitida=1 -- los
                // campos fiscales ya reportados a AFIP se restauran desde facturaExistente sin
                // importar lo que mande el cliente (defensa en profundidad, la UI ya los deja
                // readonly/disabled pero el servidor no confia en eso).
                if (dto.IdFactura > 0)
                {
                    if (factura.Venta == null)
                        return Json(new { ok = false, msg = "Venta no encontrada" });

                    var facturaExistente = _oVentaN.getFactuElecById(dto.IdFactura);
                    if (facturaExistente == null)
                        return Json(new { ok = false, msg = "Factura no encontrada" });

                    factura.PtoVtaAfip = facturaExistente.PtoVtaAfip;
                    factura.CodTipoCbteAfip = facturaExistente.CodTipoCbteAfip;
                    factura.DescTipoCbteAfip = facturaExistente.DescTipoCbteAfip;
                    factura.NroCbteAfip = facturaExistente.NroCbteAfip;
                    factura.FechaEmisionAfip = facturaExistente.FechaEmisionAfip;
                    factura.TipoDocAfip = facturaExistente.TipoDocAfip;
                    factura.NroDocAfip = facturaExistente.NroDocAfip;
                    factura.RazonSocialAFIP = facturaExistente.RazonSocialAFIP;
                    factura.CondicionIvaAFIP = facturaExistente.CondicionIvaAFIP;
                    factura.DomicilioAFIP = facturaExistente.DomicilioAFIP;
                    factura.CondicionVenta = facturaExistente.CondicionVenta;
                    factura.PorcentajeFacturacion = facturaExistente.PorcentajeFacturacion;
                    factura.ImporteNetoGravado = facturaExistente.ImporteNetoGravado;
                    factura.Iva = facturaExistente.Iva;
                    factura.ImporteTotal = facturaExistente.ImporteTotal;
                    factura.CAE1 = facturaExistente.CAE1;
                    factura.FecVtoCAE = facturaExistente.FecVtoCAE;

                    _oVentaN.addOrEditFactuElec(factura);

                    return Json(new
                    {
                        ok = true,
                        updated = true,
                        facturaId = dto.IdFactura,
                        ventaId = dto.IdVenta,
                        msg = "Factura actualizada correctamente"
                    });
                }

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

        // POST /Ventas/GenerarNotaCredito -- port 2026-09-05 (ver docs/DECISIONS.md), autorizado
        // explicitamente por el usuario para probar con montos reales chicos. Mismo mecanismo que
        // GenerarFactura (AFIP.GenerarFacturaService.GenerarNotaCredito, ya existente y probado en
        // produccion real por el mini-spike de facturacion) -- solo cambia esNotaCredito=true y que
        // se manda la factura ASOCIADA (origen) ademas de la nota nueva.
        // Recorte deliberado respecto al original: NO se porto la opcion "AnularVenta" (clona la
        // venta entera como venta anulada, Web/Controllers/VentasController.cs:3285
        // ClonarVentaParaNotaCredito) -- es una pieza separada y mas grande (afecta reportes/stock),
        // fuera del alcance de "probar que la nota de credito real contra AFIP funciona". Si se
        // necesita mas adelante, portar aparte.
        // Verificado contra AFIP produccion real el mismo dia: Nota de Credito B real nro
        // 00000004, CAE 86361677370904, revirtiendo la Factura B 00056387 (CAE 86361666170878)
        // emitida en la misma sesion -- ver docs/DECISIONS.md.
        [HttpPost]
        public IActionResult GenerarNotaCredito(int idFactura)
        {
            try
            {
                if (idFactura <= 0)
                    return Json(new { ok = false, msg = "Factura origen inválida" });

                var facturaOrigen = _oVentaN.getFactuElecById(idFactura);
                if (facturaOrigen == null || facturaOrigen.Id <= 0)
                    return Json(new { ok = false, msg = "No se encontró la factura origen" });

                if (EsNotaCreditoAfip(facturaOrigen.CodTipoCbteAfip))
                    return Json(new { ok = false, msg = "La nota de crédito debe generarse desde una factura emitida" });

                if (string.IsNullOrWhiteSpace(facturaOrigen.CAE1))
                    return Json(new { ok = false, msg = "La factura origen no tiene CAE válido" });

                var venta = facturaOrigen.Venta ?? _oVentaN.getVentaById(facturaOrigen.IdVenta);
                if (venta == null)
                    return Json(new { ok = false, msg = "No se encontró la venta asociada" });

                int idNotaExistente = _oVentaN.esVentaSinFacturar(venta.IdVenta, true);
                if (idNotaExistente > 0)
                {
                    var ncExistente = _oVentaN.getFactuElecById(idNotaExistente);
                    return Json(new
                    {
                        ok = true,
                        already = true,
                        facturaId = idNotaExistente,
                        nro = ncExistente?.NroCbteAfip,
                        cae = ncExistente?.CAE1,
                        mensaje = "Ya existe una nota de crédito asociada a esta venta",
                        detalleUrl = Url.Action("DetalleFactura", "Ventas", new { id = idNotaExistente })
                    });
                }

                facturaOrigen.Venta = venta;
                var notaCredito = CrearNotaCreditoDesdeFactura(facturaOrigen, venta);

                var afipSvc = new AFIP.GenerarFacturaService(venta, _env.ContentRootPath);
                var afipRes = afipSvc.GenerarNotaCredito(notaCredito, facturaOrigen);

                if (!afipRes.Ok)
                    return Json(new { ok = false, msg = "AFIP: " + afipRes.Mensaje });

                _oVentaN.addOrEditFactuElec(afipRes.Factura);

                int idNotaGenerada = _oVentaN.esVentaSinFacturar(venta.IdVenta, true);

                return Json(new
                {
                    ok = true,
                    facturaId = idNotaGenerada,
                    nro = afipRes.Factura?.NroCbteAfip,
                    cae = afipRes.Factura?.CAE1,
                    mensaje = "Nota de crédito generada correctamente",
                    detalleUrl = idNotaGenerada > 0 ? Url.Action("DetalleFactura", "Ventas", new { id = idNotaGenerada }) : null
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error generando nota de crédito", error = ex.Message });
            }
        }

        private static int MapearTipoNotaCreditoDesdeFactura(int codTipoCbteAfip)
        {
            if (codTipoCbteAfip == FacturaElectronica.codFacturaA_Afip) return FacturaElectronica.codNotaCreditoA_Afip;
            if (codTipoCbteAfip == FacturaElectronica.codFacturaB_Afip) return FacturaElectronica.codNotaCreditoB_Afip;
            if (codTipoCbteAfip == FacturaElectronica.codFacturaC_Afip) return FacturaElectronica.codNotaCreditoC_Afip;
            throw new InvalidOperationException("Tipo de factura no soportado para generar nota de crédito");
        }

        private static Entidades.FacturaElectronica CrearNotaCreditoDesdeFactura(Entidades.FacturaElectronica facturaOrigen, Entidades.Venta venta)
        {
            return new Entidades.FacturaElectronica
            {
                Venta = venta,
                IdVenta = venta.IdVenta,
                CodTipoCbteAfip = MapearTipoNotaCreditoDesdeFactura(facturaOrigen.CodTipoCbteAfip),
                FechaEmisionAfip = DateTime.Now,
                TipoDocAfip = facturaOrigen.TipoDocAfip,
                NroDocAfip = facturaOrigen.NroDocAfip,
                RazonSocialAFIP = facturaOrigen.RazonSocialAFIP,
                CondicionIvaAFIP = facturaOrigen.CondicionIvaAFIP,
                DomicilioAFIP = facturaOrigen.DomicilioAFIP,
                CondicionVenta = facturaOrigen.CondicionVenta,
                FormaPago = facturaOrigen.FormaPago,
                PorcentajeFacturacion = Convert.ToSingle(facturaOrigen.PorcentajeFacturacion),
                DescItemUnitario = facturaOrigen.DescItemUnitario ?? "",
                Observaciones = ""
            };
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

        // POST /Ventas/CerrarVentaSinFacturar -- boton "Cerrar venta sin facturar" del modal de
        // Factura Electronica (2026-09-06, retomado -- ver docs/DECISIONS.md). Port literal de
        // Web/Controllers/VentasController.cs:2261-2310: no borra nada, solo deja un registro de
        // FacturaElectronica marcado Error=true (mismo mecanismo que un fallo real de AFIP) para
        // que esVentaSinFacturar deje de considerar la venta pendiente de facturar.
        [HttpPost]
        public IActionResult CerrarVentaSinFacturar(int idVenta)
        {
            try
            {
                if (idVenta <= 0)
                    return Json(new { ok = false, msg = "Venta inválida" });

                var venta = _oVentaN.getVentaById(idVenta);
                if (venta == null)
                    return Json(new { ok = false, msg = "Venta no encontrada" });

                int idFacturaExistente = _oVentaN.esVentaSinFacturar(idVenta, false);
                if (idFacturaExistente > 0)
                    return Json(new { ok = false, msg = "La venta ya tiene una factura electrónica registrada." });

                var factErr = new Entidades.FacturaElectronica
                {
                    IdVenta = idVenta,
                    Venta = venta,
                    Error = true,
                    MensajeError = "se forzo el cierre de la ventana sin facturar.",
                    FechaError = DateTime.Now
                };

                _oVentaN.addOrEditFactuElec(factErr);

                return Json(new { ok = true, ventaId = idVenta, forcedClose = true });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error cerrando la venta sin facturar", error = ex.Message });
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

        // GET /Ventas/ImprimirTicket?id=X&mm=0 -- modal de Factura Electronica (2026-09-06,
        // retomado -- ver docs/DECISIONS.md). Port SOLO de la rama mm==0 de Web/Controllers/
        // VentasController.cs:1399-1422 ("ver factura ya emitida" / formulario para generarla),
        // que es html puro sin relacion con el resto del nombre "ImprimirTicket" -- se mantiene el
        // mismo nombre/ruta que el clasico porque VentasFacturaModal.abrir (modal-postventa.js,
        // window.AppUrls.ventasImprimir) llama a esta URL literal con mm=0 para ambos casos (venta
        // ya facturada o pendiente de facturar). mm!=0 (ticket ESC/POS via agente local) sigue
        // fuera de alcance -- devuelve el mismo JSON de error que ya usa este controller para lo no
        // portado, que el JS del modal ya sabe reconocer (VentasFacturaModal.abrir hace
        // dataType:'html' + un regex-sniff de {"ok":false...}).
        [HttpGet]
        public IActionResult ImprimirTicket(int id, int mm = 0)
        {
            var venta = _oVentaN.getVentaById(id);
            if (venta == null)
                return Json(new { ok = false, msg = "Venta no encontrada" });

            if (mm != 0)
                return Json(new { ok = false, msg = "Impresión de ticket ESC/POS no disponible en este sistema." });

            var factuElec = ObtenerFacturaAsociadaVenta(venta.IdVenta) ?? new Entidades.FacturaElectronica();
            var notaCreditoAsociada = ObtenerNotaCreditoAsociadaVenta(venta.IdVenta);
            ViewBag.NotaCreditoAsociadaId = notaCreditoAsociada != null ? notaCreditoAsociada.Id : 0;

            var dto = BuildFacturaDTO(venta, factuElec);
            ViewBag.SucursalNombreFactura = venta.Sucursal != null
                ? (!string.IsNullOrWhiteSpace(venta.Sucursal.SucursalNombre) ? venta.Sucursal.SucursalNombre : venta.Sucursal.sucursal)
                : "";

            return PartialView("~/Views/Ventas/_FacturaElectronica.cshtml", dto);
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

        // GET /Ventas/ImprimirTicketHtml?id=X&mm=58|80 -- ticket termico HTML, retomado 2026-09-06
        // (pedido explicito del usuario -- ver docs/DECISIONS.md). Port fiel de la rama mm!=0 de
        // Web/Controllers/VentasController.cs:ImprimirTicket (clasico) -- mismo diseño de
        // _TicketHTML.cshtml (WebCore/Views/Ventas/_TicketHTML.cshtml, portado sin cambios de
        // logica). El navegador hace de "agente" (dialogo nativo de impresion via window.print()
        // en la vista), igual que el resto de comprobantes de este controller -- el agente local
        // ESC/POS (ImprimirTicketPayload en el clasico) sigue explicitamente fuera de alcance, ver
        // header de este archivo. Ruta separada de ImprimirTicket (que ya esta ocupada en este
        // controller por el modal de Factura Electronica, ver su propio comentario) para no
        // pisar esa URL que ya consume factura-electronica.js.
        //
        // TODO(claude): Negocio/NegocioAgregado1-3 configurables via AppSettings (el clasico los
        // lee de ConfigurationManager.AppSettings antes de caer al fallback de Empresa) no estan
        // wireados todavia en WebCore/App.config -- se usa siempre el fallback de datos de Empresa
        // (mismo comportamiento que tendria el clasico si esas claves no estuvieran seteadas).
        [HttpGet]
        public IActionResult ImprimirTicketHtml(int id, int mm = 80)
        {
            var venta = _oVentaN.getVentaById(id);
            if (venta == null)
                return NotFound();

            int medida = mm == 58 ? 58 : 80;
            ViewBag.Medida = medida;

            var facturaTicket = ObtenerFacturaAsociadaVenta(venta.IdVenta);
            ViewBag.FacturaTicket = (facturaTicket != null && facturaTicket.Id > 0)
                ? BuildFacturaDTO(venta, facturaTicket)
                : null;

            var empresaTicket = ObtenerEmpresaVenta(venta);
            ViewBag.EmpresaSesion = empresaTicket;

            // QR oficial de AFIP (RG 4892/2020) para el ticket -- solo si la venta esta facturada.
            // GenerateQRCode devuelve null si falta algun dato obligatorio.
            if (facturaTicket != null && facturaTicket.Id > 0)
            {
                var qrBytes = WebCore.Services.GenerarDocsCore.GenerateQRCode(facturaTicket, venta);
                ViewBag.QrTicketBase64 = qrBytes != null ? Convert.ToBase64String(qrBytes) : null;
            }

            ViewBag.Negocio = ObtenerNombreEmpresaVenta(venta);
            ViewBag.NegocioAgregado1 = empresaTicket != null ? empresaTicket.Slogan1 ?? "" : "";
            ViewBag.NegocioAgregado2 = empresaTicket != null ? empresaTicket.Slogan2 ?? "" : "";
            ViewBag.NegocioAgregado3 = empresaTicket != null ? empresaTicket.Slogan3 ?? "" : "";

            return View("~/Views/Ventas/_TicketHTML.cshtml", venta);
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

        // ===== UI de POS (ver docs/10-migracion-aspnet-core/PLAN-POS-UI.md) =====
        //
        // Batch 1+2 (esqueleto + carrito completo, fusionados: pos-cart.js/pos-product.js ya
        // apuntaban a BuscarProducto -- portado -- y no ganaban nada quedando "a medias" en un
        // batch separado). Venta SIEMPRE nueva -- sin Session["VentaActiva"], sin idVentaEditar/
        // soloFormaPago (edicion de venta, pospuesta), sin login de operador de produccion
        // (EsUsuarioProduccion, cuenta compartida, pospuesto). El chequeo de caja abierta reusa
        // _oCierreN igual que el original (ultimo cierre sin UsuarioCierre).
        //
        // Batch 3 (agregado 2026-09-04): FormasPagoConfig/RequierePreseleccionFormaPago portados
        // desde ObtenerConfiguracionFormaPagoPOS/RequierePreseleccionFormaPagoPOS (mismos
        // parametros porcAjEfectivo/Debito/Credito/Qr/Tranf que ya usa el resto del sistema) --
        // CORRECCION sobre un hardcodeo inicial en "false" que rompia "Pago Mixto" en el
        // navegador (el checkbox del modal de forma de pago solo se habilita cuando
        // RequierePreseleccionFormaPago es real, no un valor fijo -- ver docs/DECISIONS.md).
        // Fuera de este batch: balanza (4), expendios asociados (5), post-venta real (6).
        [HttpGet]
        // modoPos/posInstanceId: soporte de "Duplicar POS" (batch 7, ver docs/10-migracion-
        // aspnet-core/PLAN-POS-UI.md). Sin el login de operador de producción (no portado, ver
        // header del archivo) -- solo la deteccion de conflicto entre pestañas via localStorage
        // (Scripts/app/pos-multi-instance.js, portado sin cambios).
        //
        // idVentaEditar: edicion de venta existente (batch 7, iteracion 5). SIN el chequeo de
        // permisos real del original (PuedeModificarUltimaVenta/PuedeCambiarFormaPago/caja del
        // cierre actual, Web/Controllers/VentasController.cs:2521-2643) -- WebCore corre con
        // usuario unico Admin=true, sin sesion, mismo bypass que el resto de los controllers ya
        // migrados. TODO(claude): portar esas reglas reales (y "usuario produccion") cuando se
        // diseñe login/sesion real -- pedido explicito del usuario 2026-09-04, anotado tambien en
        // docs/DECISIONS.md. Alcance de esta iteracion: solo edicion completa, sin soloFormaPago
        // (queda para una iteracion siguiente, ver PLAN-POS-UI.md).
        //
        // soloFormaPago (batch 7, iteracion 6): modo "Cambiar Forma de Pago", reusa toda la
        // mecanica de edicion de venta ya portada (misma carga de lineasEdicionPos, mismo
        // ModificarVenta) -- solo cambia el texto del banner y bloquea la UI de productos en la
        // vista (ver aplicarModoSoloFormaPago en POS.cshtml).
        public IActionResult POS(string modoPos = "original", string posInstanceId = "", int idVentaEditar = 0, bool soloFormaPago = false)
        {
            var user = _usuarioActual;
            string posInstanceIdNormalizado = string.IsNullOrWhiteSpace(posInstanceId)
                ? Guid.NewGuid().ToString("N")
                : posInstanceId.Trim();

            // Bug real reportado 2026-09-09 (ver docs/DECISIONS.md): al elegir una forma de pago
            // que no sea Efectivo/CtaCte, clasico abre el modal de Factura Electronica DIRECTO al
            // finalizar (Web/Views/Ventas/POS.cshtml:1670-1715, VentasFacturaModal.
            // requiereFacturaAutomatica) -- en WebCore, forma-pago.js YA llama a ese mismo metodo
            // (forma-pago.js:728-730), pero window.VentasFacturaModal (definido mas abajo en esta
            // vista) nunca definio requiereFacturaAutomatica/empresaPuedeFacturar, asi que la
            // condicion siempre daba false y caia siempre al modal basico. Se setea aca (una sola
            // vez, sirve para todas las ramas de return de esta accion) para que la vista arme
            // window.POSFacturaElectronicaConfig igual que el clasico.
            ViewBag.EmpresaTieneCertificadoAfip = EmpresaTieneCertificadoFacturaElectronica(user);

            // Usuario de produccion (Batch 5, 2026-09-06, ver docs/DECISIONS.md): el operador real
            // se resuelve por posInstanceId, no por modulo (cada pestaña de POS es independiente).
            // Port literal de Web/Controllers/VentasController.cs:789-808.
            var operador = ResolverOperadorPOS(posInstanceIdNormalizado, user);
            bool requiereOperadorPOS = user.EsUsuarioProduccion && ObtenerOperadorPOS(posInstanceIdNormalizado) == null;
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.RequiereOperadorPOS = requiereOperadorPOS;
            ViewBag.OperadorPOSNombre = (user.EsUsuarioProduccion && !requiereOperadorPOS) ? operador.Nombre : null;
            if (user.EsUsuarioProduccion)
                ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();

            if (requiereOperadorPOS)
            {
                // Sin operador resuelto todavia: no se toca caja/venta hasta que el modal de
                // seleccion de usuario (ver PosOperadorConfig en POS.cshtml) resuelva uno real.
                ViewBag.PosModoInstancia = string.Equals(modoPos, "duplicado", StringComparison.OrdinalIgnoreCase) ? "duplicado" : "original";
                ViewBag.PosInstanceId = posInstanceIdNormalizado;
                return View((Entidades.Venta)null);
            }

            if (idVentaEditar > 0)
            {
                var ventaEditar = _oVentaN.getVentaById(idVentaEditar);
                if (ventaEditar == null)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertTitle"] = "Punto de Venta";
                    TempData["AlertMsg"] = "No se encontró la venta a editar.";
                    return RedirectToAction("POS");
                }

                // Bug real (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md): la caja
                // se abre a nombre del OPERADOR real (CajasController.AbrirCaja), asi que buscarla
                // con "user" (la cuenta compartida de produccion) nunca matcheaba -- ObtenerCierreCajaActual
                // debe recibir el mismo operador que ya se resuelve mas arriba.
                var cierreEditar = ObtenerCierreCajaActual(operador);
                bool puedeEditarVenta = !soloFormaPago && PuedeModificarUltimaVenta(ventaEditar, operador, cierreEditar);
                bool puedeCambiarPago = soloFormaPago && PuedeCambiarFormaPago(ventaEditar, operador, cierreEditar);
                if (!puedeEditarVenta && !puedeCambiarPago)
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = soloFormaPago ? "Venta fuera de caja" : "Sin permisos";
                    TempData["AlertMsg"] = soloFormaPago
                        ? ObtenerMotivoNoPuedeCambiarFormaPago(ventaEditar, operador, cierreEditar)
                        : ObtenerMotivoNoPuedeModificarUltimaVenta(ventaEditar, operador, cierreEditar);
                    return RedirectToAction("POS");
                }

                ViewBag.CajaAbierta = true;
                ViewBag.SucursalNombre = user.Sucursal?.SucursalNombre ?? "";
                ViewBag.IdSucursalPOS = user.IdSucursal;
                ViewBag.IdUsuarioPOS = user.Id;
                ViewBag.PosModoInstancia = string.Equals(modoPos, "duplicado", StringComparison.OrdinalIgnoreCase) ? "duplicado" : "original";
                ViewBag.PosInstanceId = posInstanceIdNormalizado;

                var formasPagoConfigEditar = ObtenerConfiguracionFormaPagoPOS();
                ViewBag.FormasPagoConfig = formasPagoConfigEditar;
                ViewBag.RequierePreseleccionFormaPago = RequierePreseleccionFormaPagoPOS(formasPagoConfigEditar);
                ViewBag.Sucursales = _oSucursalN.findAll();
                ViewBag.IdConsumidorFinal = _oPersonaN.getConsumidorFinal().idPersona;
                ViewBag.PuedeVerCtaCteCompleta = PuedeVerCtaCteCompleta(operador);

                ViewBag.EsEdicionVenta = true;
                ViewBag.IdVentaEditar = idVentaEditar;
                ViewBag.SoloFormaPago = soloFormaPago;
                ViewBag.IdCierreActividadPOS = cierreEditar?.Id ?? 0;

                return View(ventaEditar);
            }

            string modoPosNormalizado = string.Equals(modoPos, "duplicado", StringComparison.OrdinalIgnoreCase)
                ? "duplicado"
                : "original";

            // Bug real (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md): "con el user
            // produccion, este mensaje aparece siempre aunque tenga la caja abierta". Causa: la
            // caja se abre a nombre del OPERADOR real (CajasController.AbrirCaja), pero esta
            // busqueda usaba "user" (la cuenta compartida) -- nunca encontraba la caja real. Para
            // un usuario NO-produccion, ResolverOperadorPOS ya devuelve la misma persona sin
            // cambios, asi que este fix no altera nada para el caso normal.
            var cierre = new Entidades.CierreCaja
            {
                Sucursal = operador.Sucursal,
                UsuarioInicio = operador
            };
            cierre = _oCierreN.findByIdOrLast(cierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            bool cajaAbierta = cierre != null && (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);

            // Item 5 (2026-09-07, ver docs/DECISIONS.md): usuario con permiso de "modificar venta"
            // puede operar el POS aunque no tenga caja abierta, y editar la fecha de la venta en
            // curso -- ver PuedeOperarSinCajaYEditarFecha. Chequeado contra el operador real, no
            // contra la cuenta compartida (mismo fix de arriba).
            bool puedeOperarSinCaja = PuedeOperarSinCajaYEditarFecha(operador);

            ViewBag.CajaAbierta = cajaAbierta || puedeOperarSinCaja;
            ViewBag.AdvertenciaCajaCerrada = !cajaAbierta && puedeOperarSinCaja;
            ViewBag.PuedeEditarFechaVenta = puedeOperarSinCaja;
            ViewBag.SucursalNombre = user.Sucursal?.SucursalNombre ?? "";
            ViewBag.IdSucursalPOS = user.IdSucursal;
            ViewBag.IdUsuarioPOS = user.Id;
            ViewBag.PosModoInstancia = modoPosNormalizado;
            ViewBag.PosInstanceId = posInstanceIdNormalizado;
            // F6/F7 (Mis actividades/Nuevo egreso, 2026-09-06 retomado -- ver docs/DECISIONS.md):
            // CajasController.ActividadesCaja/NuevoEgresoCaja necesitan el id de cierre explicito.
            ViewBag.IdCierreActividadPOS = cajaAbierta && cierre != null ? cierre.Id : 0;

            var formasPagoConfig = ObtenerConfiguracionFormaPagoPOS();
            ViewBag.FormasPagoConfig = formasPagoConfig;
            ViewBag.RequierePreseleccionFormaPago = RequierePreseleccionFormaPagoPOS(formasPagoConfig);

            // Para el filtro de sucursal del modal de expendios (batch 5, ver PLAN-POS-UI.md).
            ViewBag.Sucursales = _oSucursalN.findAll();

            if (!cajaAbierta && !puedeOperarSinCaja)
                return View((Entidades.Venta)null);

            var consumidorFinal = _oPersonaN.getConsumidorFinal();
            ViewBag.IdConsumidorFinal = consumidorFinal.idPersona;
            ViewBag.PuedeVerCtaCteCompleta = PuedeVerCtaCteCompleta(operador);

            var venta = new Entidades.Venta
            {
                LineasVenta = new List<Entidades.LineaVenta>(),
                Persona = consumidorFinal,
                IdPersona = consumidorFinal.idPersona,
                Vendedor = operador,
                FechaVenta = DateTime.Now
            };

            return View(venta);
        }

        // Mismo criterio que Web/Views/Ventas/POS.cshtml:1670 (empresaTieneCertificadoFacturaElectronica)
        // y PersonasController.ObtenerEmpresaAfipActual: getUsuarioById (DatosPostgres/UsuarioPg.cs:219)
        // ya carga user.Empresa via findEmpresaById, asi que el fallback por Sucursal casi nunca
        // deberia ejecutarse -- se deja igual por si algun camino de resolucion de usuario no la trae.
        private bool EmpresaTieneCertificadoFacturaElectronica(Entidades.Usuario user)
        {
            var empresaAfip = user?.Empresa;
            if (empresaAfip == null || string.IsNullOrWhiteSpace(empresaAfip.NombreCertificado_pfx))
            {
                int idEmpresa = user?.IdEmpresa ?? 0;
                if (idEmpresa > 0)
                    empresaAfip = _oSucursalN.findEmpresaById(idEmpresa);
            }

            return !string.IsNullOrWhiteSpace(empresaAfip?.NombreCertificado_pfx);
        }

        // Port de VentasController.ObtenerConfiguracionFormaPagoPOS/RequierePreseleccionFormaPagoPOS
        // (batch 3) -- sin cambios de logica. Si el negocio tiene un ajuste de precio distinto
        // segun la forma de pago (ej. recargo en credito), el cajero debe preseleccionar la forma
        // de pago ANTES de cargar productos (para que el precio ya salga ajustado); "Pago Mixto"
        // en el modal de Finalizar solo se habilita cuando hay una preseleccion real.
        private Dictionary<string, decimal> ObtenerConfiguracionFormaPagoPOS()
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { Entidades.Venta.formaPagoEnum.Efectivo.ToString(), _param.GetDecimal(Entidades.ParamKeys.PorcAjEfectivo, 1m) },
                { Entidades.Venta.formaPagoEnum.Debito.ToString(), _param.GetDecimal(Entidades.ParamKeys.PorcAjDebito, 1m) },
                { Entidades.Venta.formaPagoEnum.Credito.ToString(), _param.GetDecimal(Entidades.ParamKeys.PorcAjCredito, 1m) },
                { Entidades.Venta.formaPagoEnum.CtaCte.ToString(), 1m },
                { Entidades.Venta.formaPagoEnum.Qr.ToString(), _param.GetDecimal(Entidades.ParamKeys.PorcAjQr, 1m) },
                { Entidades.Venta.formaPagoEnum.Transferencia.ToString(), _param.GetDecimal(Entidades.ParamKeys.PorcAjTranf, 1m) }
            };
        }

        private bool RequierePreseleccionFormaPagoPOS(Dictionary<string, decimal> config)
        {
            if (config == null || config.Count == 0)
                return false;

            decimal referencia = config.Values.FirstOrDefault();
            return config.Values.Any(x => x != 1m) || config.Values.Any(x => x != referencia);
        }

        // ===== Nucleo POS transaccional (ver docs/10-migracion-aspnet-core/PLAN-POS.md) =====
        //
        // FinalizarVenta/ModificarVenta portados SIN Session["VentaActiva"] -- decision de diseño
        // confirmada en el plan: el original ya reconstruye LineasVenta siempre desde el request
        // del cliente (nunca desde Session), asi que el diseño sin estado de servidor no cambia
        // comportamiento observable. AgregarProducto NO se porta -- confirmado codigo muerto (
        // ningun .js del original le pega, ver PLAN-POS.md seccion 2).
        //
        // Vendedor real (Batch 5, 2026-09-06, ver docs/DECISIONS.md): resuelto via
        // ResolverOperadorPOS -- para un usuario de produccion es el operador ya autorizado con
        // password real para este posInstanceId; para cualquier usuario normal es el mismo `user`
        // de siempre, cero cambio de comportamiento.
        // [FromBody]: el cliente original (pos-cart.js) manda `data: JSON.stringify(payload)` --
        // en MVC5 el JsonValueProviderFactory bindea eso automaticamente sin atributo; en ASP.NET
        // Core hace falta declararlo explicito para un Controller comun (no [ApiController]).
        [HttpPost]
        public IActionResult FinalizarVenta([FromBody] FinalizarVentaRequest request)
        {
            try
            {
                var user = _usuarioActual;
                var operador = ResolverOperadorPOS(request?.PosInstanceId, user);

                if (request == null || request.LineasVenta == null || !request.LineasVenta.Any())
                    return Json(new { ok = false, msg = "No hay productos en la venta" });

                if (user.IdSucursal == 0)
                    return Json(new { ok = false, msg = "Seleccione una sucursal antes de finalizar la venta." });

                if (request.IdSucursalPOS != user.IdSucursal)
                {
                    var sucursalPos = _oSucursalN.findById(request.IdSucursalPOS);
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

                var persona = _oPersonaN.findById(request.IdPersona);
                if (persona == null)
                    return Json(new { ok = false, msg = "El Cliente no existe." });

                var sucursal = _oSucursalN.findById(user.IdSucursal);
                if (sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida." });

                // Item 5 (2026-09-07, ver docs/DECISIONS.md): con permiso real de "modificar
                // venta" se respeta la fecha editada en el POS (request.FechaVenta); sin permiso,
                // se ignora (igual que siempre) y se fuerza DateTime.Now -- mismo criterio que
                // ModificarVenta ya usa con PuedeEditarFechaVenta. Chequeado contra el operador
                // real (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md), mismo fix
                // que el resto de los sitios de este controller.
                bool puedeEditarFechaEnCurso = PuedeOperarSinCajaYEditarFecha(operador);
                DateTime fechaVentaFinal = puedeEditarFechaEnCurso && request.FechaVenta.HasValue
                    ? request.FechaVenta.Value
                    : DateTime.Now;

                var venta = new Entidades.Venta
                {
                    Persona = persona,
                    Sucursal = sucursal,
                    Vendedor = operador,
                    FechaVenta = fechaVentaFinal,
                    TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()),
                    Observaciones = request.Observaciones ?? "",
                    FormaPago = request.FormaPago,
                    EnCtaCte = request.FormaPago == Entidades.Venta.formaPagoEnum.CtaCte.ToString(),
                    PagoMixtoEfectivo = request.EsPagoMixto ? request.Efectivo : 0
                };

                if (venta.EnCtaCte && (!venta.FormaPago.Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString())
                    || persona.idPersona.Equals(_param.GetInt(ParamKeys.IdConsumidorFinal, 0))))
                {
                    return Json(new
                    {
                        ok = false,
                        msg = "Las ventas en Cuenta Corriente (CTA.CTE.) no pueden ser a Consumidor Final" +
                              "\n\nPor favor, revisa los datos ingresados y vuelva a intentarlo."
                    });
                }

                // Bug real encontrado leyendo el clasico (Web/Controllers/VentasController.cs:596):
                // ahi ya usaba "operador", no "user" -- WebCore se habia desviado (tercera ronda
                // de pedidos, 2026-09-10, ver docs/DECISIONS.md).
                bool cajaAbierta = _oCierreN.validarCajaAbiertaVendedor(DateTime.Now, venta.Sucursal, operador);
                if (!cajaAbierta && !puedeEditarFechaEnCurso)
                    return Json(new { ok = false, msg = "La caja ha sido cerrada." });

                List<Entidades.LineaVenta> lineasVenta = ConstruirLineasVentaDesdeRequest(request);
                CompletarAnulacionesVenta(lineasVenta);

                venta.LineasVenta = lineasVenta;
                venta.ListaExpendios = (request.ListaExpendios ?? new List<int>())
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                int idVenta = _oVentaN.agregarVenta(venta);

                return Json(new { ok = true, ventaId = idVenta });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error al finalizar la venta", error = ex.Message });
            }
        }

        [HttpPost]
        // Permisos reales portados 2026-09-06 (Batch 5, ver docs/DECISIONS.md) -- port literal de
        // Web/Controllers/VentasController.cs:636-733 (antes solo escritura, sin ningun chequeo,
        // bajo el usuario stub Admin=true que siempre resolvia "permitido").
        public IActionResult ModificarVenta([FromBody] FinalizarVentaRequest request)
        {
            try
            {
                var user = _usuarioActual;
                bool soloFormaPago = request != null && request.SoloFormaPago;

                if (request == null || request.IdVenta <= 0)
                    return Json(new { ok = false, msg = "Venta inválida" });

                var venta = _oVentaN.getVentaById(request.IdVenta);
                if (venta == null)
                    return Json(new { ok = false, msg = "La venta no existe." });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                // Mismo fix que POS() (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md):
                // la caja se abre a nombre del operador real, hay que buscarla con ese mismo operador.
                var cierreActual = ObtenerCierreCajaActual(operador);
                bool tienePermisoAdministrativoVenta = TienePermisoAdministrativoSobreVenta(venta, operador);

                if (soloFormaPago)
                {
                    if (!PuedeCambiarFormaPago(venta, operador, cierreActual))
                        return Json(new { ok = false, msg = ObtenerMotivoNoPuedeCambiarFormaPago(venta, operador, cierreActual) });
                }
                else
                {
                    if (!PuedeModificarUltimaVenta(venta, operador, cierreActual))
                        return Json(new { ok = false, msg = "No tiene permisos para modificar esta venta." });
                }

                if (request.LineasVenta == null || !request.LineasVenta.Any())
                    return Json(new { ok = false, msg = "No hay productos en la venta" });

                if (user.IdSucursal == 0 && !tienePermisoAdministrativoVenta)
                    return Json(new { ok = false, msg = "Seleccione una sucursal antes de guardar la venta." });

                if (!tienePermisoAdministrativoVenta &&
                    (request.IdSucursalPOS != user.IdSucursal || venta.Sucursal == null || venta.Sucursal.idSucursal != user.IdSucursal))
                    return Json(new { ok = false, msg = "La venta pertenece a otra sucursal. Cambie a la sucursal correcta antes de modificarla." });

                var persona = _oPersonaN.findById(request.IdPersona);
                if (persona == null)
                    return Json(new { ok = false, msg = "El Cliente no existe." });

                venta.Persona = persona;

                int idSucursalDestino = venta.Sucursal != null && venta.Sucursal.idSucursal > 0
                    ? venta.Sucursal.idSucursal
                    : user.IdSucursal;

                venta.Sucursal = _oSucursalN.findById(idSucursalDestino);
                if (venta.Sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida." });

                if (!tienePermisoAdministrativoVenta && (cierreActual == null || cierreActual.UsuarioInicio == null))
                    return Json(new { ok = false, msg = "La caja ha sido cerrada." });

                bool cajaAbierta = tienePermisoAdministrativoVenta ||
                    _oCierreN.validarCajaAbiertaVendedor(venta.FechaVenta, venta.Sucursal, cierreActual.UsuarioInicio);

                if (!cajaAbierta)
                    return Json(new { ok = false, msg = "La caja ha sido cerrada." });

                venta.Observaciones = request.Observaciones ?? venta.Observaciones ?? "";
                venta.FormaPago = request.FormaPago;
                venta.EnCtaCte = request.FormaPago == Entidades.Venta.formaPagoEnum.CtaCte.ToString();
                venta.PagoMixtoEfectivo = request.EsPagoMixto ? request.Efectivo : 0;

                if (!soloFormaPago && request.FechaVenta.HasValue && request.FechaVenta.Value != venta.FechaVenta)
                {
                    if (!PuedeEditarFechaVenta(venta, request.FechaVenta.Value, operador))
                        return Json(new { ok = false, msg = "No tiene permisos para modificar la venta con la fecha seleccionada." });

                    venta.FechaVenta = request.FechaVenta.Value;
                }

                if (venta.EnCtaCte && (!venta.FormaPago.Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString())
                    || persona.idPersona.Equals(_param.GetInt(ParamKeys.IdConsumidorFinal, 0))))
                {
                    return Json(new
                    {
                        ok = false,
                        msg = "Las ventas en Cuenta Corriente (CTA.CTE.) no pueden ser a Consumidor Final" +
                              "\n\nPor favor, revisa los datos ingresados y vuelva a intentarlo."
                    });
                }

                venta.LineasVenta = ConstruirLineasVentaDesdeRequest(request);
                venta.ListaExpendios = (request.ListaExpendios ?? new List<int>())
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();
                CompletarAnulacionesVenta(venta.LineasVenta);

                _oVentaN.modificarVenta(venta, venta.Sucursal.idSucursal, !soloFormaPago, null);

                return Json(new { ok = true, ventaId = venta.IdVenta });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error al modificar la venta", error = ex.Message });
            }
        }

        // Port de VentasController.BuscarProducto (2026-09-04): usa Negocio.BarcodeInterpreter,
        // la clase compartida (Ventas y Expendio) que centraliza los 2 mecanismos --codigo
        // generico (sufijo "G<n>"/precio manual) y codigos de barra internos de balanza
        // (EAN-13 prefijo 20-29, formato configurable por empresa)-- que hasta ahora vivian
        // duplicados en los 2 controllers. Portada por la sesion en paralelo que la desarrollo
        // (ver docs/DECISIONS.md), incorporada aca reemplazando la version simplificada del
        // 2026-09-03 (que usaba la logica vieja de PuntosExpendioController.BuscarProductoPOS,
        // sin el motor de codigos internos, porque en ese momento BarcodeInterpreter todavia no
        // compilaba). Misma logica que el original, sin cambios.
        [HttpGet]
        public IActionResult BuscarProducto(string codigo, bool ingresoCantidadX = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                    return Json(new { error = "Código vacío" });

                codigo = codigo.Replace(",", ".");

                int idEmpresaSesion = _usuarioActual.IdEmpresa;

                var generico = _oBarcodeInterpreter.InterpretarCodigoGenerico(
                    codigo, ingresoCantidadX, _param.GetLong(ParamKeys.CodProdGenerico, 0L));

                if (generico.FormatoInvalido)
                    return Json(new { error = "Formato de código inválido" });

                Entidades.Corte corte;
                decimal? cantidadSugerida = null;

                if (generico.EsGenerico)
                {
                    corte = idEmpresaSesion > 0
                        ? _oCorteN.findCorteByCodigoEmpresa(generico.CodigoProducto, idEmpresaSesion, false)
                        : _oCorteN.findCorteByCodigo(generico.CodigoProducto, false);

                    if (corte == null || (idEmpresaSesion > 0 && corte.IdEmpresa != idEmpresaSesion))
                        return Json(new { success = false, message = "No existe  el código genérico" });

                    corte.PrecioKg = generico.PrecioManual.Value;
                }
                else
                {
                    var interno = _oBarcodeInterpreter.Interpretar(codigo, idEmpresaSesion);

                    if (interno.EsCodigoInterno)
                    {
                        if (interno.Caso == Entidades.CasoInterpretacionBarcode.EstructuraInvalida)
                            return Json(new { success = false, message = interno.MensajeDiagnostico });

                        if (interno.Caso == Entidades.CasoInterpretacionBarcode.ProductoNoEncontrado)
                            return Json(new { success = false, message = "Código inexistente" });

                        corte = interno.Producto;
                        if (interno.TipoValor == Entidades.TipoValorCodigoBarras.Precio)
                            corte.PrecioKg = (float)interno.Valor.Value;
                        else
                            cantidadSugerida = interno.Valor;
                    }
                    else
                    {
                        long codigoProducto = Convert.ToInt64(codigo);
                        corte = idEmpresaSesion > 0
                            ? _oCorteN.findCorteByCodigoEmpresa(codigoProducto, idEmpresaSesion, false)
                            : _oCorteN.findCorteByCodigo(codigoProducto, false);

                        if (corte == null || (idEmpresaSesion > 0 && corte.IdEmpresa != idEmpresaSesion))
                            return Json(new { success = false, message = "Código inexistente" });
                    }
                }

                return Json(new
                {
                    id = corte.IdCorte,
                    nombre = corte.CorteDesc,
                    precioKg = Math.Round((double)corte.PrecioKg, 2),
                    precioOriginal = Math.Round((double)corte.PrecioKg, 2),
                    codigo = corte.codigo,
                    pesable = corte.Pesable,
                    cantidadSugerida
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Port de VentasController.BuscarExpendiosPOS/ObtenerExpendioPOS (2026-09-04, PLAN-POS.md
        // batch 5): expendios asociados al POS (ventas cargadas primero desde PuntosExpendio y
        // luego buscadas/asignadas a una venta de caja). Session["Usuario"] -> _usuarioActual
        // (stateless, ver diseno confirmado en PLAN-POS.md seccion 3) y JsonRequestBehavior.
        // AllowGet (no aplica en ASP.NET Core).
        //
        // AGREGADO 2026-09-04 (PLAN-POS-UI.md, refinamiento del batch 5, pedido explicito del
        // usuario -- ver docs/DECISIONS.md): filtros avanzados fechaHasta + sucursal. idSucursal
        // sigue la misma convencion que Ventas/Index ("-1" = todas las sucursales, no 0); por
        // defecto (sin filtro explicito) sigue siendo la sucursal del usuario actual, igual que
        // el comportamiento original. Cambio de query: obtenerUltimosExpendios (ultimosMinutos)
        // -> obtenerExpendiosAvanzado (fechaDesde/fechaHasta reales), metodo nuevo y aditivo en
        // Datos/DatosPostgres -- no se toco el original, que sigue usando la version del original.
        [HttpGet]
        public IActionResult BuscarExpendiosPOS(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int idSucursal = 0, string estado = "Pendientes", string texto = "", string idsActuales = "")
        {
            try
            {
                var user = _usuarioActual;
                if (user == null || user.IdSucursal == 0)
                    return Json(new { ok = false, msg = "Sesión inválida o sucursal no seleccionada." });

                DateTime fechaDesdeReal = fechaDesde ?? DateTime.Today;
                int idSucursalConsulta = idSucursal == -1 ? 0 : (idSucursal > 0 ? idSucursal : user.IdSucursal);

                DataTable dt = _oVentaN.obtenerExpendiosAvanzado(fechaDesdeReal, fechaHasta, idSucursalConsulta);
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
                            cargadoEnVentaActual = idsEnVentaActual.Contains(idExpendio),
                            observaciones = ToStr(row["observaciones"]),
                            idSucursal = ToInt(row["idSucursal"]),
                            sucursal = ToStr(row["sucursalNombre"])
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
                        idSucursal = idSucursalConsulta,
                        totalSql = dt.Rows.Count,
                        totalFiltrado = filas.Count,
                        estado = estadoNormalizado,
                        texto = textoNormalizado
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    msg = "Error al consultar expendios: " + ex.Message
                });
            }
        }

        // CAMBIO DELIBERADO 2026-09-04 (pedido explicito del usuario, ver docs/DECISIONS.md): el
        // original bloqueaba cargar un expendio de otra sucursal ("El expendio pertenece a otra
        // sucursal"). Se saca ese bloqueo -- la venta queda en la sucursal del cajero actual
        // (VentasController.FinalizarVenta ya usa Sucursal = _oSucursalN.findById(user.IdSucursal),
        // no la del expendio), pero ahora puede incluir lineas de expendios de cualquier sucursal.
        [HttpGet]
        public IActionResult ObtenerExpendioPOS(int idExpendio)
        {
            var user = _usuarioActual;
            if (user == null || user.IdSucursal == 0)
                return Json(new { ok = false, msg = "Sesión inválida o sucursal no seleccionada." });

            if (idExpendio <= 0)
                return Json(new { ok = false, msg = "Expendio inválido." });

            var expendio = _oVentaN.getExpedioById(idExpendio);
            if (expendio == null || expendio.IdExpendio <= 0)
                return Json(new { ok = false, msg = "El expendio no existe." });

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
                    asignado = expendio.IdVenta > 0 && expendio.IdVenta != expendio.IdExpendio,
                    observaciones = expendio.Observaciones ?? "",
                    idSucursal = expendio.Sucursal != null ? expendio.Sucursal.idSucursal : 0,
                    sucursal = expendio.Sucursal != null ? expendio.Sucursal.SucursalNombre : ""
                },
                // precioListaActual/cambioPrecio (2026-09-06, pedido explicito del usuario, ver
                // docs/DECISIONS.md): l.Corte viene de un JOIN en vivo contra la tabla corte (ver
                // VentaPg.GetLineasExpendio), asi que l.Corte.PrecioKg es el precio de lista DE
                // HOY, distinto de l.PrecioKg (el precio guardado en la linea al momento de crear
                // el expendio, en Puntos de Expendio -- ver docs/DECISIONS.md "no toma forma de
                // pago"). Tolerancia de 1 centavo para no marcar diferencias de redondeo float
                // como "cambio de precio" real.
                lineas = (expendio.LineasVenta ?? new List<Entidades.LineaVenta>()).Select(l =>
                {
                    float precioListaActual = l.Corte != null ? l.Corte.PrecioKg : l.PrecioKg;
                    bool cambioPrecio = l.Corte != null && Math.Abs(l.PrecioKg - precioListaActual) > 0.005f;

                    return new
                    {
                        idExpendio = expendio.IdExpendio,
                        codigo = l.Corte != null ? l.Corte.codigo : 0,
                        producto = l.Corte != null ? l.Corte.corte : "",
                        cantKg = l.CantKg,
                        precioKg = l.PrecioKg,
                        precioListaActual = precioListaActual,
                        cambioPrecio = cambioPrecio,
                        bonificacion = l.Bonificacion,
                        balanza = l.PesoBalanza
                    };
                }).ToList()
            });
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

        private List<Entidades.LineaVenta> ConstruirLineasVentaDesdeRequest(FinalizarVentaRequest request)
        {
            var lineasVenta = new List<Entidades.LineaVenta>();

            foreach (var l in request.LineasVenta)
            {
                var linea = new Entidades.LineaVenta
                {
                    Corte = _oCorteN.findCorteByCodigoEmpresa(l.Codigo, _usuarioActual.IdEmpresa, false),
                    KgsTotalCalculado = l.CantKg,
                    CantKg = l.CantKg,
                    PrecioKg = l.PrecioKg,
                    Bonificacion = l.Bonificacion,
                    Estado = l.Estado,
                    IndexAnulado = l.IndexAnulado,
                    PesoBalanza = l.Balanza,
                    IdExpendio = l.IdExpendio
                };

                lineasVenta.Add(linea);
            }

            return lineasVenta;
        }

        private void CompletarAnulacionesVenta(List<Entidades.LineaVenta> lineasVenta)
        {
            var lineasAnuladas = new List<Entidades.LineaVenta>();
            int cantLineaParam = lineasVenta.Count;

            for (int index = 0; index < lineasVenta.Count; index++)
            {
                if (Entidades.LineaVenta.esAnulado(lineasVenta[index].Estado) && lineasVenta[index].IndexAnulado == -1)
                {
                    lineasVenta[index].Estado = 0;

                    var oLineaVenta = new Entidades.LineaVenta
                    {
                        Corte = lineasVenta[index].Corte,
                        Venta = lineasVenta[index].Venta,
                        CantKg = lineasVenta[index].CantKg * -1,
                        KgsTotalCalculado = lineasVenta[index].KgsTotalCalculado * -1,
                        KgsAjusteTarj = lineasVenta[index].KgsAjusteTarj * -1,
                        PrecioKg = lineasVenta[index].PrecioKg,
                        Estado = 1,
                        Bonificacion = lineasVenta[index].Bonificacion,
                        IndexAnulado = index,
                        IdExpendio = lineasVenta[index].IdExpendio
                    };

                    lineasVenta[index].IndexAnulado = cantLineaParam++;
                    lineasAnuladas.Add(oLineaVenta);
                }
            }

            for (int index = 0; index < lineasAnuladas.Count; index++)
                lineasVenta.Add(lineasAnuladas[index]);
        }
    }
}
