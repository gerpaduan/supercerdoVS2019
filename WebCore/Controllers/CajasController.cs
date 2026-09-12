// Port de Web/Controllers/CajasController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 7 -- Caja y tesoreria). Incluye la pantalla "Cajas Abiertas" (listado + historial +
// egresos de caja + cierre de caja + cambio de sucursal) y la pantalla administrativa separada
// "Egresos de Caja" (EgresosCaja/TiposEgresoCaja/GuardarTipoEgresoCaja/EliminarTipoEgresoCaja/
// CalcularComisionesElectronicas/GuardarComisionesElectronicas/TiposEgresoCajaOpciones) -- este
// comentario decia "NO portados en este slice" para el segundo grupo, pero las acciones ya
// estaban escritas mas abajo sin permisos reales; el gate de permisos de Egresos de Caja se
// completo en la tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md).
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06) -- ya no hay stub hardcodeado.
//
// Step-up de Cierre de Caja (AutorizarAccionCierre/RevocarAutorizacionCierre, con
// CierreCajaStepUpRateLimiter + PermisosHelper.RegistrarElevacionCierre/ObtenerUsuarioAutorizadoCierre)
// portado en la tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md) -- mecanismo para que
// un usuario SIN el permiso directo de cerrar caja pueda autorizar temporalmente (5 min) tipeando
// la clave de otro usuario que si lo tiene. De paso se cerraron 3 gates de servidor que hasta
// entonces aceptaban la peticion de CUALQUIER usuario logueado sin validar nada:
// CerrarCaja/ActividadesCaja/PuedeCambiarSucursalCaja (esta ultima protege tambien
// PreviewCambioSucursalCaja/CambiarSucursalCaja, que ya la llamaban). Desviacion deliberada de
// MECANISMO respecto al clasico: la elevacion se guarda en ISession (no en MemoryCache -- ver
// comentario completo en PermisosHelper.cs), porque WebCore/CajasController.cs no tiene la
// restriccion [SessionState(ReadOnly)] que forzaba esa eleccion en clasico.
//
// El boton "Ventas" de cada fila abre Ventas/MisVentas (Modulo 8, POS, no portado) -- queda
// wireado igual que el original pero da 404 al clickear, gap ya aceptado en este mismo patron
// para toda dependencia de POS (ver Compras.desdePos).
//
// AbrirCaja: todas las validaciones del original portadas 1:1 (importe>0, formato de fecha exacto
// dd/MM/yyyy HH:mm, caja ya abierta por sucursal) -- confirmado 2026-09-06 comparando linea por
// linea contra Web/Controllers/CajasController.cs:866-932. ResolverOperadorPOS (cuenta compartida
// de produccion) portado el mismo dia -- antes faltaba, la caja quedaba siempre a nombre de la
// cuenta compartida en vez del operador real resuelto por VentasController/PuntosExpendioController
// (mismo posInstanceId, misma clave de Session).
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class CajasController : Controller
    {
        private sealed class GuardarEgresoCajaResultado
        {
            public bool Ok { get; set; }
            public int Id { get; set; }
            public string Mensaje { get; set; } = "";
        }

        public sealed class CambioSucursalCajaPostVm
        {
            public int IdCierre { get; set; }
            public int IdSucursalNueva { get; set; }
        }

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.CierreCaja _oCierreN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Venta _oVentaN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public CajasController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oVentaN = WebCore.Infrastructure.NegocioFactory.CrearVenta(_empresa, _param);
        }

        // Resuelve el operador real de la cuenta compartida de produccion para AbrirCaja
        // (2026-09-06, ver docs/DECISIONS.md) -- port literal de Web/Controllers/BaseController.cs:
        // 314-320 y Web/Controllers/CajasController.cs:878. Mismo patron que VentasController/
        // PuntosExpendioController (duplicado, sin base controller comun en WebCore); lee la MISMA
        // clave de Session que esos 2 controllers escriben (OperadorPOS_<posInstanceId>), ya que
        // comparten el mismo posInstanceId dentro de la misma pestaña de POS.
        private Entidades.Usuario ResolverOperadorPOS(string posInstanceId, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion) return usuarioSesion;

            var json = HttpContext.Session.GetString("OperadorPOS_" + (posInstanceId ?? ""));
            if (string.IsNullOrEmpty(json)) return usuarioSesion;

            var operador = System.Text.Json.JsonSerializer.Deserialize<Entidades.Usuario>(json);
            return operador ?? usuarioSesion;
        }

        // ===== Step-up de Cierre de Caja (tercera ronda de pedidos, 2026-09-10, ver
        // docs/DECISIONS.md) -- port de Web/Controllers/CajasController.cs:805-864. =====

        // Mismo criterio que VentasController.ObtenerSessionIdEstable/ComprasController (duplicado
        // por controller, sin base controller comun en WebCore): ASP.NET Core Session no manda el
        // Set-Cookie hasta el primer write, asi que sin esto el rate-limit por sesion nunca acumula.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // Wrapper fino sobre PermisosHelper.ObtenerUsuarioAutorizadoCierre -- el chequeo de
        // permiso directo (Permisos.Caja.CerrarCaja) se resuelve aca con _oUsuarioN (el helper
        // compartido no tiene acceso a Negocio.Usuario), igual que ya hace CajasAbiertas() con
        // tienePermisoCerrarCaja.
        private Entidades.Usuario ObtenerUsuarioAutorizadoCierre()
        {
            var user = _usuarioActual;
            bool tienePermisoDirecto = user != null && _oUsuarioN.tienePermiso(user, Entidades.Permisos.Caja.CerrarCaja, DateTime.Today, -1);
            return WebCore.Helpers.PermisosHelper.ObtenerUsuarioAutorizadoCierre(HttpContext.Session, user, tienePermisoDirecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutorizarAccionCierre(int idUsuario, string clave)
        {
            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.CierreCajaStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return Json(new { ok = false, bloqueado = true, segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds) });

            const string mensajeGenerico = "Usuario o contraseña incorrectos.";
            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave))
            {
                WebCore.Helpers.CierreCajaStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var candidato = _oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
            {
                WebCore.Helpers.CierreCajaStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            var validado = _oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            bool tienePermiso = validado != null && _oUsuarioN.tienePermiso(validado, Entidades.Permisos.Caja.CerrarCaja, DateTime.Today, -1);
            if (validado == null || !validado.Activo || !tienePermiso)
            {
                WebCore.Helpers.CierreCajaStepUpRateLimiter.RegisterFailure(sessionId);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            WebCore.Helpers.CierreCajaStepUpRateLimiter.Reset(sessionId);
            WebCore.Helpers.PermisosHelper.RegistrarElevacionCierre(HttpContext.Session, validado, TimeSpan.FromMinutes(5));
            return Json(new { ok = true, nombre = validado.Nombre });
        }

        // SIN [ValidateAntiForgeryToken] a proposito -- se llama via navigator.sendBeacon() al
        // navegar afuera de Cierre de Caja (beforeunload), que no puede adjuntar el token. Mismo
        // criterio que el clasico (Web/Controllers/CajasController.cs:859-864).
        [HttpPost]
        public IActionResult RevocarAutorizacionCierre()
        {
            WebCore.Helpers.PermisosHelper.RevocarElevacionCierre(HttpContext.Session);
            return Json(new { ok = true });
        }

        [HttpGet]
        public IActionResult CajasAbiertas(int? idSucursal, string buscar = "", DateTime? fechaDesde = null, bool ajax = false)
        {
            var user = _usuarioActual;
            // Port de Web/Controllers/CajasController.cs:57-60 -- la pantalla en si es
            // accesible para cualquier usuario logueado (nunca se bloqueo con AccesoDenegado);
            // el historial de cierres y "PuedeModificarCierres" se gatean aparte con permisos
            // reales. PermisosPantallasWeb.Cajas.CerrarCaja/CajasAbiertasConsulta son alias de
            // Permisos.Caja.CerrarCaja/CierresDeCaja (Web/Helpers/PermisosPantallasWeb.cs:7-11).
            bool tienePermisoCerrarCaja = _oUsuarioN.tienePermiso(user, Entidades.Permisos.Caja.CerrarCaja, DateTime.Today, -1);

            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            int? idSucursalSeleccionada = ResolverSucursalSeleccionada(sucursales, idSucursal);
            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);

            ViewBag.Sucursales = sucursales;
            ViewBag.HayVariasSucursales = sucursales.Count > 1;
            ViewBag.IdSucursal = idSucursalSeleccionada;
            ViewBag.Buscar = buscar ?? "";
            ViewBag.FechaDesde = desde;
            // Port de Web/Controllers/CajasController.cs:81.
            ViewBag.PuedeModificarCierres = user != null && _oUsuarioN.tienePermiso(user, Entidades.Permisos.Caja.CierresDeCaja, DateTime.Today, -1);
            ViewBag.TienePermisoCerrarCaja = tienePermisoCerrarCaja;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.HistorialCierres = tienePermisoCerrarCaja
                ? ObtenerHistorialCierresCaja(idSucursalSeleccionada, buscar ?? "", desde, sucursales)
                : new DataTable();

            var dt = ObtenerCajasAbiertas(idSucursalSeleccionada, buscar ?? "");

            if (ajax)
                return PartialView("~/Views/Cajas/_TablaCajasAbiertas.cshtml", dt);

            ViewBag.Title = "Cajas Abiertas";
            return View("~/Views/Cajas/CajasAbiertas.cshtml", dt);
        }

        [HttpGet]
        public IActionResult HistorialCierresCaja(int? idSucursal, string buscar = "", DateTime? fechaDesde = null)
        {
            // Port de Web/Controllers/CajasController.cs:658-665 -- el historial NUNCA se
            // desbloquea con la autorizacion temporal (step-up), solo con el permiso de cerrar
            // caja en forma directa.
            if (!_oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Caja.CerrarCaja, DateTime.Today, -1))
                return StatusCode(403, "No tiene permisos para ver cierres de caja.");

            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            int? idSucursalSeleccionada = ResolverSucursalSeleccionada(sucursales, idSucursal);
            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);

            ViewBag.PuedeModificarCierres = _oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Caja.CierresDeCaja, DateTime.Today, -1);
            var dt = ObtenerHistorialCierresCaja(idSucursalSeleccionada, buscar ?? "", desde, sucursales);
            return PartialView("~/Views/Cajas/_TablaCierresDeCaja.cshtml", dt);
        }

        [HttpGet]
        public IActionResult ObtenerDatosCierre(int id, bool modoModificacion = false)
        {
            var usuarioAutorizado = _usuarioActual;

            Entidades.CierreCaja oCierreE = new CierreCaja();
            oCierreE.Id = id;
            var caja = oCierreE = _oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindById, "");

            if (caja == null || caja.Id == 0)
                return NotFound("No se encontró la caja seleccionada.");

            DateTime fechaHastaVentas = modoModificacion && caja.FechaHoraCierre.HasValue
                ? caja.FechaHoraCierre.Value
                : DateTime.Now;

            caja.EgresosCaja = _oCierreN.getMontoEgresosCajaVendedor(oCierreE);

            return Json(new
            {
                id = caja.Id,
                suc = caja.Sucursal.SucursalNombre,
                vendedor = caja.UsuarioInicio.Nombre,
                cajaInicial = caja.CajaInicio,
                fechaApertura = caja.FechaHoraInicio.Value.ToString("dd/MM/yyyy HH:mm"),
                fechaCierre = caja.FechaHoraCierre.HasValue ? caja.FechaHoraCierre.Value.ToString("dd/MM/yyyy HH:mm") : "",
                usuario = (caja.UsuarioCierre != null && caja.UsuarioCierre.Id > 0)
                    ? caja.UsuarioCierre.Nombre
                    : usuarioAutorizado.Nombre,
                ventas = _oCierreN.obtenerTotalVentas(oCierreE.UsuarioInicio.Id, oCierreE.Sucursal.idSucursal,
                        oCierreE.FechaHoraInicio, fechaHastaVentas).ToString(),
                egresosCaja = caja.EgresosCaja,
                cajaCierre = caja.CajaCierre,
                diferencia = caja.Diferencia,
                importeRetirado = caja.ImporteRetirado,
                cajaInicioSiguiente = caja.CajaInicioSiguiente,
                modoModificacion = modoModificacion
            });
        }

        public IActionResult CerrarCaja(
                int Id,
                string CajaCierre,
                string Diferencia,
                string ImporteRetirado,
                string CajaInicioSiguiente,
                bool modoModificacion = false
            )
        {
            // Permiso real (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md) -- port
            // literal de Web/Controllers/CajasController.cs:751-765. Antes: "var usuarioAutorizado
            // = _usuarioActual;" sin ningun chequeo -- cualquier usuario logueado podia cerrar
            // cualquier caja llamando esta accion directo, gate de servidor real ausente.
            var usuarioAutorizado = ObtenerUsuarioAutorizadoCierre();
            if (usuarioAutorizado == null)
                return Json(new { ok = false, error = "No tiene permisos para cerrar caja. Volvé a autorizar e intentá de nuevo." });

            if (modoModificacion && !_oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Caja.CierresDeCaja, DateTime.Today, -1))
                return Json(new { ok = false, error = "No tiene permisos para modificar un cierre de caja histórico." });

            Entidades.CierreCaja model = _oCierreN.findByIdOrLast(
                new CierreCaja { Id = Id },
                Entidades.CierreCaja.tipoBusqueda.FindById,
                ""
            );

            if (model == null || model.Id == 0)
                return Json(new { ok = false, error = "No se encontró la caja seleccionada." });

            if (modoModificacion && !model.FechaHoraCierre.HasValue)
                return Json(new { ok = false, error = "La caja seleccionada todavía no tiene cierre para modificar." });

            model.CajaCierre = ParseFloat(CajaCierre);
            model.Diferencia = ParseFloat(Diferencia);
            model.ImporteRetirado = ParseFloat(ImporteRetirado);
            model.CajaInicioSiguiente = ParseFloat(CajaInicioSiguiente);
            model.UsuarioCierre = usuarioAutorizado;
            model.FechaHoraCierre = modoModificacion
                ? model.FechaHoraCierre
                : (model.FechaHoraCierre != null ? model.FechaHoraCierre : DateTime.Now);
            model.Ventas = _oCierreN.obtenerTotalVentas(model.UsuarioInicio.Id, model.Sucursal.idSucursal,
                    model.FechaHoraInicio, modoModificacion ? model.FechaHoraCierre : DateTime.Now);
            model.EgresosCaja = _oCierreN.getMontoEgresosCajaVendedor(model);

            var result = _oCierreN.addOrEditCierreCaja_Result(model);

            if (!result.Ok)
                return Json(new { ok = false, error = result.Mensaje });

            return Json(new
            {
                ok = true,
                mensaje = modoModificacion ? "El cierre de caja se actualizó correctamente." : result.Mensaje
            });
        }

        [HttpGet]
        public IActionResult PreviewCambioSucursalCaja(int idCierre, int idSucursalNueva)
        {
            var sucursales = _oSucursalN.findAll();
            if (!PuedeCambiarSucursalCaja(sucursales))
                return Json(new { ok = false, mensaje = "No tiene permisos para cambiar la sucursal de una caja." });

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return Json(new { ok = false, mensaje = "No se encontro la caja seleccionada." });
            if (!CajaSigueAbierta(cierre))
                return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." });

            var preview = _oCierreN.obtenerPreviewCambioSucursalCaja(cierre, idSucursalNueva);
            return Json(new
            {
                ok = true,
                puedeEjecutar = preview.PuedeEjecutar,
                mensaje = preview.Mensaje,
                tieneCajaAbiertaEnDestino = preview.TieneCajaAbiertaEnDestino,
                hayMovimientosEnDestino = preview.HayMovimientosEnDestino,
                advertenciaMovimientosEnDestino = preview.AdvertenciaMovimientosEnDestino,
                idCierreCaja = preview.IdCierreCaja,
                sucursalActual = preview.SucursalActual,
                sucursalNueva = preview.SucursalNueva,
                usuarioCaja = preview.UsuarioCaja,
                fechaDesde = preview.FechaDesde.ToString("dd/MM/yyyy HH:mm"),
                fechaHasta = preview.FechaHasta.ToString("dd/MM/yyyy HH:mm"),
                tablas = preview.Tablas.Select(t => new { tabla = t.Tabla, cantidad = t.Cantidad }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarSucursalCaja(CambioSucursalCajaPostVm model)
        {
            var user = _usuarioActual;
            var sucursales = _oSucursalN.findAll();
            if (!PuedeCambiarSucursalCaja(sucursales))
                return Json(new { ok = false, mensaje = "No tiene permisos para cambiar la sucursal de una caja." });

            if (model == null || model.IdCierre <= 0 || model.IdSucursalNueva <= 0)
                return Json(new { ok = false, mensaje = "Datos invalidos para cambiar la sucursal." });

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = model.IdCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return Json(new { ok = false, mensaje = "No se encontro la caja seleccionada." });
            if (!CajaSigueAbierta(cierre))
                return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." });

            var resultado = _oCierreN.cambiarSucursalCaja(cierre, model.IdSucursalNueva, user.Id, user.Nombre);
            return Json(new
            {
                ok = resultado.Ok,
                mensaje = resultado.Mensaje,
                tablas = resultado.Tablas.Select(t => new { tabla = t.Tabla, cantidad = t.Cantidad }).ToList()
            });
        }

        [HttpGet]
        public IActionResult ActividadesCaja(int idCierre, string filtroActividad = "todos")
        {
            var user = _usuarioActual;

            // Permiso real (tercera ronda, ver docs/DECISIONS.md) -- port literal de
            // Web/Controllers/CajasController.cs:166-168. Antes sin ningun chequeo.
            if (ObtenerUsuarioAutorizadoCierre() == null)
                return StatusCode(403, "No tiene permisos para ver actividades de caja.");

            if (idCierre <= 0)
                return BadRequest("Caja inválida.");

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return NotFound("No se encontró la caja seleccionada.");

            DataTable dt = _oCierreN.getEgresosCajaVendedor(cierre);
            dt = FiltrarActividadesCaja(dt, filtroActividad);

            string nombreVendedor = cierre.UsuarioInicio != null ? cierre.UsuarioInicio.Nombre : "cajero";
            string nombreSucursal = cierre.Sucursal != null ? cierre.Sucursal.SucursalNombre : "";

            ViewBag.DesdePOS = false;
            ViewBag.SoloEgresos = false;
            ViewBag.FiltroActividad = filtroActividad ?? "todos";
            ViewBag.CierreCaja = cierre;
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.TotalVisible = CalcularTotalGastosCaja(dt);
            // Port literal de Web/Controllers/CajasController.cs:189,192 (antes stub "= true" /
            // sin el chequeo de permiso -- mismo batch de Egresos de Caja, 2026-09-10).
            ViewBag.MostrarResumenMisActividades = _oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.VerEgresosCaja, DateTime.Today, -1);
            ViewBag.ModoActividades = true;
            ViewBag.PermitirNuevo = CajaSigueAbierta(cierre) &&
                user != null &&
                _oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditEgresoCaja, DateTime.Today, user.Id);
            ViewBag.IdCierreActividad = idCierre;
            ViewBag.SucursalActividad = nombreSucursal;
            ViewBag.TituloActividades = "Actividades";
            // Antes: un solo string concatenado "ger | San Martin" (ViewBag.SubtituloActividades)
            // -- pedido explicito del usuario (2026-09-07, ver docs/DECISIONS.md): se ve feo.
            // Se pasan por separado para que la vista los pueda mostrar cada uno con su propio
            // estilo (icono + nombre, badge de sucursal) en vez de texto plano con un pipe.
            ViewBag.VendedorActividad = nombreVendedor;
            CargarPermisosEdicionEgresos(dt, false, cierre);

            return PartialView("~/Views/Cajas/_MisEgresosCaja.cshtml", dt);
        }

        // Bug real reportado 2026-09-11 (ver docs/DECISIONS.md): F6 en POS ("Mis actividades")
        // llamaba a ActividadesCaja, que exige Permisos.Caja.CerrarCaja (el permiso de CERRAR
        // caja, no de ver la propia actividad) -- un vendedor sin ese permiso no podia ver ni sus
        // propios movimientos. El clasico (Web/Controllers/CajasController.cs:130-162) resuelve
        // esto con una accion separada, MisEgresosCaja, sin ese gate -- se porta acá como
        // MisActividadesCaja, sin tocar ActividadesCaja (que sigue siendo la pantalla admin de
        // Cajas Abiertas, correctamente gateada). A diferencia del clasico (que resuelve la caja
        // del vendedor de sesion internamente y por eso nunca puede ver la caja de otro), aca se
        // recibe idCierre ya resuelto server-side por VentasController.POS (correcto tambien para
        // cuentas de produccion, que ObtenerCajaAbiertaUsuario(_usuarioActual) no resolveria bien
        // -- ver Batch 4/docs/DECISIONS.md) -- por eso se valida que la caja consultada sea
        // realmente la propia antes de mostrarla, en vez de confiar ciegamente en el idCierre que
        // manda el cliente.
        [HttpGet]
        public IActionResult MisActividadesCaja(int idCierre, string filtroActividad = "todos")
        {
            var user = _usuarioActual;
            if (idCierre <= 0)
                return BadRequest("Caja inválida.");

            var cierre = _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return NotFound("No se encontró la caja seleccionada.");

            // Solo la propia actividad: sin esto, cualquier usuario logueado podria ver la caja de
            // otro cambiando el idCierre a mano. Las cuentas de produccion (usuario compartido) no
            // se pueden validar por Id sin el posInstanceId -- quedan confiadas al mismo criterio
            // que el resto del flujo de POS para ese tipo de cuenta.
            bool esPropiaCaja = cierre.UsuarioInicio != null && cierre.UsuarioInicio.Id == user.Id;
            if (!esPropiaCaja && !user.EsUsuarioProduccion)
                return StatusCode(403, "No tiene permisos para ver esta actividad de caja.");

            DataTable dt = _oCierreN.getEgresosCajaVendedor(cierre);
            dt = FiltrarActividadesCaja(dt, filtroActividad);

            string nombreVendedor = cierre.UsuarioInicio != null ? cierre.UsuarioInicio.Nombre : "cajero";
            string nombreSucursal = cierre.Sucursal != null ? cierre.Sucursal.SucursalNombre : "";

            ViewBag.DesdePOS = false;
            ViewBag.SoloEgresos = false;
            ViewBag.FiltroActividad = filtroActividad ?? "todos";
            ViewBag.CierreCaja = cierre;
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.TotalVisible = CalcularTotalGastosCaja(dt);
            ViewBag.MostrarResumenMisActividades = _oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.VerEgresosCaja, DateTime.Today, -1);
            ViewBag.ModoActividades = true;
            ViewBag.PermitirNuevo = CajaSigueAbierta(cierre) &&
                user != null &&
                _oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditEgresoCaja, DateTime.Today, user.Id);
            ViewBag.IdCierreActividad = idCierre;
            ViewBag.SucursalActividad = nombreSucursal;
            ViewBag.TituloActividades = "Actividades";
            ViewBag.VendedorActividad = nombreVendedor;
            CargarPermisosEdicionEgresos(dt, false, cierre);

            return PartialView("~/Views/Cajas/_MisEgresosCaja.cshtml", dt);
        }

        [HttpGet]
        public IActionResult NuevoEgresoCaja(int id = 0, bool desdePos = false, int idCierre = 0)
        {
            var user = _usuarioActual;

            // Permiso real (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md) -- port
            // literal de Web/Controllers/CajasController.cs:210-215. Igual que clasico, el gate se
            // saltea cuando desdePos=true: el egreso se registra desde el propio POS del vendedor,
            // ya protegido por el acceso al POS en si (misma decision que clasico, no una omision).
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditEgresoCaja, DateTime.Today, user.Id) && !desdePos)
                return StatusCode(403, "No tiene permisos para registrar egresos de caja.");

            CierreCaja? cierreContexto = idCierre > 0
                ? _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return NotFound("No se encontró la caja seleccionada.");

                if (!CajaSigueAbierta(cierreContexto))
                    return BadRequest("La caja seleccionada ya no se encuentra abierta.");
            }

            EgresoCaja egreso;
            int idSucursal = cierreContexto != null && cierreContexto.Sucursal != null
                ? cierreContexto.Sucursal.idSucursal
                : user.IdSucursal;

            if (id > 0)
            {
                egreso = _oCierreN.getEgresoCajaById(id);
                if (egreso == null || egreso.Id == 0)
                    return NotFound("Egreso de caja no encontrado.");

                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egreso, desdePos, _empresa, _oCierreN.validarCajaAbiertaVendedor);
                if (!validacion.PuedeModificar)
                    return BadRequest(validacion.MensajeBloqueo);

                if (cierreContexto != null && !FechaDentroDeCaja(egreso.Fecha, cierreContexto))
                    return BadRequest("El egreso seleccionado no corresponde al rango horario de la caja abierta.");

                idSucursal = egreso.Sucursal != null && egreso.Sucursal.idSucursal > 0 ? egreso.Sucursal.idSucursal : idSucursal;
            }
            else
            {
                egreso = new EgresoCaja
                {
                    Fecha = DateTime.Now,
                    Sucursal = _oSucursalN.findById(idSucursal),
                    CreadoPor = cierreContexto != null && cierreContexto.UsuarioInicio != null ? cierreContexto.UsuarioInicio.Id : user.Id,
                    CreadoPorUser = cierreContexto != null ? cierreContexto.UsuarioInicio : user
                };
            }

            CargarViewBagsFormularioEgreso(desdePos, idSucursal, cierreContexto);
            ViewBag.EsEdicion = id > 0;
            ViewBag.IdCierreActividad = idCierre;

            if (egreso.Sucursal == null || egreso.Sucursal.idSucursal == 0)
                egreso.Sucursal = _oSucursalN.findById(idSucursal);

            return PartialView("~/Views/Cajas/_AddOrEditEgresoCaja.cshtml", egreso);
        }

        [HttpPost]
        public IActionResult GuardarEgresoCaja(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var resultado = GuardarEgresoCajaCore(id, fecha, idTipoEgresoCaja, descripcion, monto, detalle, idSucursal, desdePos, idCierre);
                return Json(new { ok = resultado.Ok, id = resultado.Id, mensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar egreso de caja. " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AbrirCaja(string cajaInicio, string fechaHora, string posInstanceId = null)
        {
            try
            {
                var user = _usuarioActual;
                var operador = ResolverOperadorPOS(posInstanceId, user);

                float cajaInicio_ = ParseFloat(cajaInicio);
                if (cajaInicio_ <= 0)
                    return Json(new { ok = false, mensaje = "Importe inválido" });

                if (!DateTime.TryParseExact(
                        fechaHora,
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fechaApertura))
                {
                    return Json(new { ok = false, mensaje = "Fecha inválida" });
                }

                var nuevoCierre = new Entidades.CierreCaja
                {
                    Sucursal = _oSucursalN.findById(user.IdSucursal),
                    UsuarioInicio = operador,
                    FechaHoraInicio = fechaApertura,
                    CajaInicio = cajaInicio_
                };

                Entidades.CierreCaja cierre = _oCierreN.findByIdOrLast(
                    nuevoCierre,
                    Entidades.CierreCaja.tipoBusqueda.FindLast,
                    ""
                );

                if (cierre != null && cierre.UsuarioCierre == null)
                    return Json(new { ok = false, mensaje = "Ya existe una caja abierta" });

                _oCierreN.addOrEditCierreCaja(nuevoCierre);

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        // ---- Pantalla administrativa "Egresos de Caja" (EgresosCaja/TiposEgresoCaja) ----
        // Gate de permisos real (Entidades.Permisos.EgresoCaja.*) agregado en la tercera ronda de
        // pedidos (2026-09-10, ver docs/DECISIONS.md) -- antes estas acciones no chequeaban nada.

        [HttpGet]
        public IActionResult EgresosCaja(int? idSucursal, int idUsuario = -1, int idTipoEgresoCaja = 0, string descripcion = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null, string filtroGasto = "todos", bool ajax = false)
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today.AddDays(1).AddSeconds(-1);
            if (hasta.TimeOfDay == TimeSpan.Zero)
                hasta = hasta.AddDays(1).AddSeconds(-1);

            int sucursalSeleccionada = idSucursal ?? 0;

            // Permiso real (tercera ronda, ver docs/DECISIONS.md) -- port literal de
            // Web/Controllers/CajasController.cs:104-112.
            if (!_oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.EgresoCaja.VerEgresosCaja, desde, -1))
            {
                if (ajax)
                    return StatusCode(403, "No tiene permisos para ver egresos de caja.");

                CargarViewBagsEgresos(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion, desde, hasta, filtroGasto, false);
                ViewBag.SinPermiso = true;
                ViewBag.MensajePermiso = "No tiene permisos para ver egresos de caja.";
                ViewBag.Title = "Egresos de Caja";
                return View("~/Views/Cajas/EgresosCaja.cshtml", new DataTable());
            }

            CargarViewBagsEgresos(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion, desde, hasta, filtroGasto, false);

            DataTable dt = _oCierreN.obtenerEgresosCaja(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion ?? "", desde, hasta);
            dt = ExcluirTiposReservadosEgresosCaja(dt);
            CargarPermisosEdicionEgresos(dt, false);

            if (ajax)
                return PartialView("~/Views/Cajas/_EgresosCajaTabla.cshtml", dt);

            ViewBag.Title = "Egresos de Caja";
            return View("~/Views/Cajas/EgresosCaja.cshtml", dt);
        }

        [HttpGet]
        public IActionResult TiposEgresoCaja(string buscar = "")
        {
            var user = _usuarioActual;

            // Permisos reales (tercera ronda, ver docs/DECISIONS.md) -- port literal de
            // Web/Controllers/CajasController.cs:367-378.
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.VerTiposEgresos, DateTime.Today, -1))
                return StatusCode(403, "No tiene permisos para ver tipos de egreso.");

            ViewBag.BuscarTipoEgreso = buscar ?? "";
            ViewBag.PuedeEditarTipos = _oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditTipoEgreso, DateTime.Today, user.Id);
            ViewBag.UsuarioAdmin = user.Admin;

            DataTable dt = _oCierreN.obtenerTiposEgresoCaja(buscar ?? "", 0);
            return PartialView("~/Views/Cajas/_TiposEgresoCajaModal.cshtml", dt);
        }

        [HttpGet]
        public IActionResult AddOrEditTipoEgresoCaja(int id = 0)
        {
            var userTipo = _usuarioActual;

            // Permiso real (tercera ronda, ver docs/DECISIONS.md) -- port literal de
            // Web/Controllers/CajasController.cs:384-387.
            if (!_oUsuarioN.tienePermiso(userTipo, Entidades.Permisos.EgresoCaja.AddOrEditTipoEgreso, DateTime.Today, userTipo.Id))
                return StatusCode(403, "No tiene permisos para administrar tipos de egreso.");

            var model = new TipoEgresoCajaEditVm();
            if (id > 0)
            {
                DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", id);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound("No se encontró el tipo de egreso seleccionado.");

                DataRow row = dt.Rows[0];
                bool reservado = row.Table.Columns.Contains("Reservado") && row["Reservado"] != DBNull.Value && Convert.ToBoolean(row["Reservado"]);
                if (reservado)
                    return BadRequest("El tipo de egreso seleccionado es reservado por el sistema y no puede modificarse.");

                model.Id = id;
                model.TipoEgresoCaja = Convert.ToString(row["tipoEgresoCaja"]) ?? "";
                model.EsGasto = row.Table.Columns.Contains("Es_Gasto") && row["Es_Gasto"] != DBNull.Value && Convert.ToBoolean(row["Es_Gasto"]);
                model.Reservado = reservado;
            }

            return PartialView("~/Views/Cajas/_AddOrEditTipoEgresoCaja.cshtml", model);
        }

        [HttpGet]
        public IActionResult TiposEgresoCajaOpciones()
        {
            DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", 0);
            var items = dt.AsEnumerable()
                .Where(r => Convert.ToInt32(r["id"]) > 0)
                .Select(r => new
                {
                    id = Convert.ToInt32(r["id"]),
                    nombre = Convert.ToString(r["tipoEgresoCaja"])
                })
                .ToList();

            return Json(new { ok = true, items = items });
        }

        [HttpGet]
        public IActionResult CalcularComisionesElectronicas(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int idSucursal = 0, bool desdePos = false, int idCierre = 0)
        {
            var user = _usuarioActual;

            CierreCaja? cierreContexto = idCierre > 0
                ? _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return NotFound("No se encontró la caja seleccionada.");

                if (!CajaSigueAbierta(cierreContexto))
                    return BadRequest("La caja seleccionada ya no se encuentra abierta.");
            }

            DateTime desde = (fechaDesde ?? DateTime.Today).Date;
            DateTime hasta = (fechaHasta ?? DateTime.Today).Date;
            if (desde > hasta)
                return BadRequest("La fecha desde no puede ser mayor que la fecha hasta.");

            bool sucursalFija = desdePos || (cierreContexto != null && cierreContexto.Sucursal != null);
            int sucursalSeleccionada = desdePos
                ? user.IdSucursal
                : (cierreContexto != null && cierreContexto.Sucursal != null
                    ? cierreContexto.Sucursal.idSucursal
                    : idSucursal);

            var sucursal = sucursalSeleccionada > 0 ? _oSucursalN.findById(sucursalSeleccionada) : null;
            if (sucursalSeleccionada > 0 && sucursal == null)
                return BadRequest("Sucursal inválida.");

            if (desdePos && !_oCierreN.validarCajaAbiertaVendedor(DateTime.Now, sucursal, user))
                return BadRequest("Debe tener una caja abierta en la sucursal activa para registrar egresos desde POS.");

            var model = CrearModelComisionesElectronicas(desde, hasta, DateTime.Now, sucursalSeleccionada, desdePos, idCierre);
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.MostrarSelectorSucursal = !sucursalFija;
            return PartialView("~/Views/Cajas/_CalcularComisionesElectronicas.cshtml", model);
        }

        [HttpGet]
        public IActionResult ObtenerResumenComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal)
        {
            try
            {
                if (fechaDesde.Date > fechaHasta.Date)
                    return Json(new { ok = false, mensaje = "La fecha desde no puede ser mayor que la fecha hasta." });

                int? sucursalConsulta = idSucursal > 0 ? (int?)idSucursal : null;
                if (sucursalConsulta.HasValue && _oSucursalN.findById(sucursalConsulta.Value) == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." });

                var formas = ObtenerFormasPagoElectronicas(fechaDesde.Date, fechaHasta.Date, sucursalConsulta, null);
                return Json(new
                {
                    ok = true,
                    items = formas.Select(f => new
                    {
                        codigo = f.Codigo,
                        nombre = f.Nombre,
                        totalCobrado = f.TotalCobrado
                    }).ToList(),
                    total = formas.Sum(f => f.TotalCobrado)
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "No se pudieron calcular las comisiones. " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarTipoEgresoCaja(TipoEgresoCajaEditVm model)
        {
            try
            {
                var user = _usuarioActual;

                // Permiso real (tercera ronda, ver docs/DECISIONS.md) -- port literal de
                // Web/Controllers/CajasController.cs:535-538.
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditTipoEgreso, DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para administrar tipos de egreso." });

                string nombre = model != null ? (model.TipoEgresoCaja ?? "").Trim() : "";
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { ok = false, mensaje = "El campo Tipo no puede ser vacío." });

                int id = model != null ? model.Id : 0;
                if (id > 0)
                {
                    DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", id);
                    if (dt == null || dt.Rows.Count == 0)
                        return Json(new { ok = false, mensaje = "No se encontró el tipo de egreso seleccionado." });

                    bool reservado = dt.Rows[0].Table.Columns.Contains("Reservado") &&
                                     dt.Rows[0]["Reservado"] != DBNull.Value &&
                                     Convert.ToBoolean(dt.Rows[0]["Reservado"]);
                    if (reservado)
                        return Json(new { ok = false, mensaje = "El tipo de egreso seleccionado es reservado por el sistema y no puede modificarse." });
                }

                _oCierreN.addOrEditTipoEgreso(id > 0 ? id : -1, nombre, model != null && model.EsGasto);
                return Json(new { ok = true, mensaje = "El Tipo Egreso se registró correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarTipoEgresoCaja(int id)
        {
            try
            {
                var user = _usuarioActual;

                // Permiso real (tercera ronda, ver docs/DECISIONS.md) -- port literal de
                // Web/Controllers/CajasController.cs:575-581 (el gate de permiso va ANTES del
                // chequeo de Admin, mismo orden que clasico).
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditTipoEgreso, DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para administrar tipos de egreso." });

                if (!user.Admin)
                    return Json(new { ok = false, mensaje = "Debe tener permiso de Administrador para eliminar un Tipo Egreso." });

                DataTable dt = _oCierreN.obtenerTiposEgresoCaja("", id);
                if (dt == null || dt.Rows.Count == 0)
                    return Json(new { ok = false, mensaje = "No se encontró el tipo de egreso seleccionado." });

                bool reservado = dt.Rows[0].Table.Columns.Contains("Reservado") &&
                                 dt.Rows[0]["Reservado"] != DBNull.Value &&
                                 Convert.ToBoolean(dt.Rows[0]["Reservado"]);
                if (reservado)
                    return Json(new { ok = false, mensaje = "El Tipo Egreso seleccionado es reservado por el sistema y no puede eliminarse." });

                _oCierreN.eliminarTipoEgreso(id);
                return Json(new { ok = true, mensaje = "El Tipo Egreso se eliminó correctamente." });
            }
            catch (Exception ex)
            {
                string msg = ex.Message != null && ex.Message.IndexOf("FK", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "No se puede eliminar porque existen egresos de caja con el Tipo Egreso seleccionado."
                    : ex.Message;
                return Json(new { ok = false, mensaje = msg });
            }
        }

        [HttpPost]
        public IActionResult GuardarComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, DateTime fechaEgreso, int idTipoEgresoCaja, int idSucursal, string porcentajeDebito, string porcentajeCredito, string porcentajeQr, string porcentajeTransferencia, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var user = _usuarioActual;

                DateTime desde = fechaDesde.Date;
                DateTime hasta = fechaHasta.Date;
                if (desde > hasta)
                    return Json(new { ok = false, mensaje = "La fecha desde no puede ser mayor que la fecha hasta." });

                var porcentajes = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Debito", ParseDecimalFlexible(porcentajeDebito) },
                    { "Credito", ParseDecimalFlexible(porcentajeCredito) },
                    { "Qr", ParseDecimalFlexible(porcentajeQr) },
                    { "Transferencia", ParseDecimalFlexible(porcentajeTransferencia) }
                };

                int? sucursalFiltro = idSucursal > 0 ? (int?)idSucursal : null;
                if (sucursalFiltro.HasValue && _oSucursalN.findById(sucursalFiltro.Value) == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." });

                var formas = ObtenerFormasPagoElectronicas(desde, hasta, sucursalFiltro, porcentajes);
                decimal totalComisiones = formas.Sum(f => f.ImporteComision);
                if (totalComisiones <= 0)
                    return Json(new { ok = false, mensaje = "Debe existir al menos una comisión mayor a cero para guardar el egreso." });

                string sucursalDescripcion = ObtenerDescripcionSucursal(idSucursal);
                string descripcion = string.Format(
                    "Periodo {0} - {1} | Sucursal: {2}",
                    desde.ToString("dd/MM/yyyy"),
                    hasta.ToString("dd/MM/yyyy"),
                    sucursalDescripcion);

                StringBuilder detalleBuilder = new StringBuilder();
                detalleBuilder.AppendFormat(
                    "Cálculo automático de comisiones por pagos electrónicos entre {0} y {1} | Sucursal: {2}.",
                    desde.ToString("dd/MM/yyyy"),
                    hasta.ToString("dd/MM/yyyy"),
                    sucursalDescripcion);

                foreach (var forma in formas)
                {
                    detalleBuilder.AppendLine();
                    detalleBuilder.AppendFormat(
                        "{0}: total cobrado ${1} - comisión {2}% = ${3}",
                        forma.Nombre,
                        FormatearImporte(forma.TotalCobrado),
                        FormatearPorcentaje(forma.Porcentaje),
                        FormatearImporte(forma.ImporteComision));
                }

                int idSucursalGuardar = idSucursal > 0 ? idSucursal : user.IdSucursal;
                var resultado = GuardarEgresoCajaCore(
                    0,
                    fechaEgreso,
                    idTipoEgresoCaja,
                    descripcion,
                    totalComisiones.ToString(CultureInfo.InvariantCulture),
                    detalleBuilder.ToString(),
                    idSucursalGuardar,
                    desdePos,
                    idCierre);

                return Json(new { ok = resultado.Ok, id = resultado.Id, mensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar comisiones electrónicas. " + ex.Message });
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

        private void CargarViewBagsFormularioEgreso(bool desdePos, int idSucursal, CierreCaja? cierreContexto = null)
        {
            var user = _usuarioActual;

            ViewBag.DesdePOS = desdePos;
            ViewBag.UsuarioActual = cierreContexto != null && cierreContexto.UsuarioInicio != null
                ? cierreContexto.UsuarioInicio
                : user;
            ViewBag.UsuarioOperando = user;
            ViewBag.UsuarioAdmin = user.Admin;
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdCierreActividad = cierreContexto != null ? cierreContexto.Id : 0;
        }

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

        private int? ResolverSucursalSeleccionada(List<Entidades.Sucursal> sucursales, int? idSucursal)
        {
            if (idSucursal.HasValue)
                return idSucursal.Value > 0 ? idSucursal : (int?)null;

            if (sucursales != null && sucursales.Count == 1)
                return sucursales[0].IdSucursal;

            return null;
        }

        private DataTable ObtenerCajasAbiertas(int? idSucursal, string buscar)
        {
            CierreCaja filtro = new CierreCaja();

            if (idSucursal.HasValue)
            {
                var sucursalActual = _oSucursalN.findById(idSucursal.Value);
                if (sucursalActual == null)
                    return new DataTable();

                filtro.Sucursal = sucursalActual;
            }

            return _oCierreN.findCierreCaja(filtro, CierreCaja.tipoBusqueda.FindOpen, buscar, null);
        }

        private DataTable ObtenerHistorialCierresCaja(int? idSucursal, string buscar, DateTime fechaDesde, List<Entidades.Sucursal> sucursales)
        {
            if (idSucursal.HasValue)
            {
                var sucursal = _oSucursalN.findById(idSucursal.Value);
                return sucursal == null ? new DataTable() : ObtenerHistorialCierresCajaSucursal(sucursal, buscar, fechaDesde);
            }

            var listaSucursales = sucursales ?? new List<Entidades.Sucursal>();
            DataTable? acumulado = null;

            foreach (var sucursal in listaSucursales)
            {
                if (sucursal == null || sucursal.IdSucursal <= 0)
                    continue;

                var dtSucursal = ObtenerHistorialCierresCajaSucursal(sucursal, buscar, fechaDesde);
                if (dtSucursal == null)
                    continue;

                if (acumulado == null)
                    acumulado = dtSucursal.Clone();

                foreach (DataRow row in dtSucursal.Rows)
                    acumulado.ImportRow(row);
            }

            if (acumulado == null)
                return new DataTable();

            var vista = acumulado.DefaultView;
            vista.Sort = "id DESC";
            return vista.ToTable();
        }

        private DataTable ObtenerHistorialCierresCajaSucursal(Entidades.Sucursal sucursal, string buscar, DateTime fechaDesde)
        {
            if (sucursal == null)
                return new DataTable();

            var filtro = new CierreCaja
            {
                Sucursal = sucursal
            };

            return _oCierreN.findCierreCaja(filtro, CierreCaja.tipoBusqueda.FindAll, buscar ?? "", fechaDesde);
        }

        private CierreCaja? ObtenerCajaAbiertaUsuario(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null)
                user.Sucursal = _oSucursalN.findById(user.IdSucursal);

            var cierre = new CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = _oCierreN.findByIdOrLast(cierre, CierreCaja.tipoBusqueda.FindLast, "");

            bool abierta = cierre != null && cierre.UsuarioCierre != null && cierre.UsuarioCierre.Id == 0;
            return abierta ? cierre : null;
        }

        private DataTable FiltrarActividadesCaja(DataTable dt, string filtroActividad)
        {
            if (dt == null)
                return dt;

            string filtro = string.IsNullOrWhiteSpace(filtroActividad) ? "todos" : filtroActividad;
            if (string.Equals(filtro, "todos", StringComparison.OrdinalIgnoreCase))
                return dt;

            DataTable filtrado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                bool incluir = false;

                if (string.Equals(filtro, "gastos", StringComparison.OrdinalIgnoreCase))
                    incluir = EsGastoCaja(row) || (EsPagoCobro(row) && TieneMovimientoCaja(row));
                else if (string.Equals(filtro, "pagoElectronico", StringComparison.OrdinalIgnoreCase))
                    incluir = EsPagoElectronico(row);
                else if (string.Equals(filtro, "ctaCte", StringComparison.OrdinalIgnoreCase))
                    incluir = EsCtaCte(row) || EsPagoCobro(row);

                if (incluir)
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        private bool EsPagoElectronico(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            return string.Equals(tipo, "Pago Electronico", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsCtaCte(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            return string.Equals(tipo, "Cta Cte", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsPagoCobro(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            if (string.IsNullOrWhiteSpace(tipo))
                return false;

            string tipoNormalizado = tipo.Replace(" ", "");
            return tipoNormalizado.IndexOf("Pago", StringComparison.OrdinalIgnoreCase) >= 0 &&
                   tipoNormalizado.IndexOf("Cobro", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool EsGastoCaja(DataRow row)
        {
            if (row == null || row.Table == null)
                return false;

            if (EsPagoCobro(row))
                return false;

            return !EsPagoElectronico(row) && !EsCtaCte(row);
        }

        private bool TieneMovimientoCaja(DataRow row)
        {
            decimal monto;
            string valor = ValorString(row, "Monto");
            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
            {
                return monto != 0;
            }

            return false;
        }

        private decimal CalcularTotalGastosCaja(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("Monto"))
                return 0;

            decimal total = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (!EsGastoCaja(row) && !(EsPagoCobro(row) && TieneMovimientoCaja(row)))
                    continue;

                decimal monto;
                if (decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                    decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
                {
                    total += monto;
                }
            }

            return total;
        }

        // Permiso real (tercera ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md) -- port
        // literal de Web/Controllers/CajasController.cs:1284-1295. Antes: solo chequeaba
        // "cantidadSucursales > 1" -- CUALQUIER usuario logueado podia cambiar la sucursal de
        // cualquier caja, sin ningun chequeo de permiso.
        private bool PuedeCambiarSucursalCaja(List<Entidades.Sucursal> sucursales = null)
        {
            var user = _usuarioActual;
            if (user == null) return false;

            bool tienePermiso = user.Admin || ObtenerUsuarioAutorizadoCierre() != null;
            int cantidadSucursales = sucursales != null ? sucursales.Count : _oSucursalN.findAll().Count;
            return tienePermiso && cantidadSucursales > 1;
        }

        private void CargarPermisosEdicionEgresos(DataTable dt, bool desdePos, CierreCaja? cierreContexto = null)
        {
            var user = _usuarioActual;
            var idsModificables = new HashSet<int>();
            var pagosModificables = new Dictionary<int, int>();
            var comprasModificables = new Dictionary<int, int>();

            if (dt == null || !dt.Columns.Contains("id"))
            {
                ViewBag.IdsEgresosModificables = idsModificables;
                ViewBag.PagosModificables = pagosModificables;
                ViewBag.ComprasModificables = comprasModificables;
                return;
            }

            var ids = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id > 0)
                    ids.Add(id);
            }

            var egresosPorId = _oCierreN.getEgresosCajaByIds(ids)
                .Where(x => x != null && x.Id > 0)
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First());

            bool puedeEditarEnPos = false;
            if (desdePos)
            {
                var cierrePos = cierreContexto ?? ObtenerCajaAbiertaUsuario(user);
                puedeEditarEnPos = cierrePos != null &&
                                   cierrePos.Id > 0 &&
                                   cierrePos.Sucursal != null &&
                                   cierrePos.Sucursal.idSucursal > 0 &&
                                   CajaSigueAbierta(cierrePos) &&
                                   user.IdSucursal > 0 &&
                                   user.IdSucursal == cierrePos.Sucursal.idSucursal;
            }

            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id <= 0)
                    continue;

                Entidades.EgresoCaja egreso;
                if (!egresosPorId.TryGetValue(id, out egreso))
                    continue;

                bool esPagoCobro = egreso != null &&
                                   egreso.Id > 0 &&
                                   string.Equals(egreso.Tabla, Entidades.EgresoCaja.tablas.Pagos.ToString(), StringComparison.OrdinalIgnoreCase) &&
                                   egreso.IdTabla.HasValue &&
                                   egreso.IdTabla.Value > 0;

                if (esPagoCobro)
                    pagosModificables[id] = egreso.IdTabla.Value;

                if (desdePos)
                {
                    if (!puedeEditarEnPos)
                        continue;

                    if (EgresosCajaPolicy.EsCompra(egreso))
                    {
                        int idCompraRelacionado = egreso.IdCompra.HasValue && egreso.IdCompra.Value > 0
                            ? egreso.IdCompra.Value
                            : (egreso.IdTabla.HasValue ? egreso.IdTabla.Value : 0);

                        if (idCompraRelacionado > 0 &&
                            (cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)) &&
                            egreso.Sucursal != null &&
                            egreso.Sucursal.idSucursal == user.IdSucursal)
                        {
                            comprasModificables[id] = idCompraRelacionado;
                        }

                        continue;
                    }

                    if (EgresosCajaPolicy.EsPagoElectronico(egreso) ||
                        EgresosCajaPolicy.EsCuentaCorriente(egreso))
                    {
                        continue;
                    }

                    if ((cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)) &&
                        egreso.Sucursal != null &&
                        egreso.Sucursal.idSucursal == user.IdSucursal)
                    {
                        idsModificables.Add(id);
                    }

                    continue;
                }

                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egreso, false, _empresa, _oCierreN.validarCajaAbiertaVendedor);
                if (validacion.PuedeModificar && (cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)))
                    idsModificables.Add(id);
            }

            ViewBag.IdsEgresosModificables = idsModificables;
            ViewBag.PagosModificables = pagosModificables;
            ViewBag.ComprasModificables = comprasModificables;
        }

        private bool CajaSigueAbierta(CierreCaja cierre)
        {
            return cierre != null &&
                   cierre.Id > 0 &&
                   (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
        }

        private bool FechaDentroDeCaja(DateTime fecha, CierreCaja cierre)
        {
            if (cierre == null || cierre.FechaHoraInicio == null)
                return false;

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            return fecha >= inicio && fecha <= fin;
        }

        private GuardarEgresoCajaResultado GuardarEgresoCajaCore(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos, int idCierre)
        {
            var user = _usuarioActual;

            if (idTipoEgresoCaja <= 0)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Seleccione un tipo de egreso." };

            if (string.IsNullOrWhiteSpace(descripcion))
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Ingrese una descripción." };

            float importe = ParseFloat(monto);
            if (importe <= 0)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Ingrese un monto válido." };

            CierreCaja? cierreContexto = idCierre > 0
                ? _oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No se encontró la caja seleccionada." };

                if (!CajaSigueAbierta(cierreContexto))
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La caja seleccionada ya no se encuentra abierta." };
            }

            if (desdePos)
                idSucursal = user.IdSucursal;
            else if (cierreContexto != null && cierreContexto.Sucursal != null)
                idSucursal = cierreContexto.Sucursal.idSucursal;

            EgresoCaja egresoAnterior = id > 0 ? _oCierreN.getEgresoCajaById(id) : null;
            if (id > 0 && (egresoAnterior == null || egresoAnterior.Id == 0))
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No se encontró el egreso de caja a modificar." };

            var sucursal = _oSucursalN.findById(idSucursal);
            if (sucursal == null)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Sucursal inválida." };

            if (desdePos)
            {
                bool cajaAbierta = _oCierreN.validarCajaAbiertaVendedor(fecha, sucursal, user);
                if (!cajaAbierta)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La fecha y hora del egreso debe corresponder a una caja abierta del vendedor." };
            }
            else if (cierreContexto != null && !FechaDentroDeCaja(fecha, cierreContexto))
            {
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La fecha y hora del egreso debe corresponder a la caja abierta seleccionada." };
            }
            // Permiso real (tercera ronda, ver docs/DECISIONS.md) -- port literal de
            // Web/Controllers/CajasController.cs:1496-1499: solo aplica a un egreso NUEVO
            // (id==0) y fuera de POS -- modificar uno existente ya lo cubre EgresosCajaPolicy
            // mas abajo, y desde POS el gate se saltea igual que en NuevoEgresoCaja.
            else if (id == 0 && !_oUsuarioN.tienePermiso(user, Entidades.Permisos.EgresoCaja.AddOrEditEgresoCaja, fecha, user.Id))
            {
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No tiene permisos para registrar egresos de caja." };
            }

            if (egresoAnterior != null)
            {
                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egresoAnterior, desdePos, _empresa, _oCierreN.validarCajaAbiertaVendedor);
                if (!validacion.PuedeModificar)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = validacion.MensajeBloqueo };
            }

            var egreso = new EgresoCaja
            {
                Id = id,
                Fecha = fecha,
                IdTipoEgresoCaja = idTipoEgresoCaja,
                Descripcion = descripcion ?? "",
                Detalle = detalle ?? "",
                Monto = importe,
                Sucursal = sucursal,
                IdCompra = egresoAnterior != null ? egresoAnterior.IdCompra : null,
                Tabla = egresoAnterior != null ? egresoAnterior.Tabla : null,
                IdTabla = egresoAnterior != null ? egresoAnterior.IdTabla : null,
                CreadoPor = egresoAnterior != null
                    ? egresoAnterior.CreadoPor
                    : (cierreContexto != null && cierreContexto.UsuarioInicio != null ? cierreContexto.UsuarioInicio.Id : user.Id),
                ActualizadoPor = id > 0 ? user.Id : 0
            };

            egreso = _oCierreN.addOrEditEgresoCaja(egreso);

            return new GuardarEgresoCajaResultado
            {
                Ok = true,
                Id = egreso.Id,
                Mensaje = id > 0 ? "El egreso de caja se modificó correctamente." : "El egreso de caja se guardó correctamente."
            };
        }

        private void CargarViewBagsEgresos(int idSucursal, int idUsuario, int idTipoEgresoCaja, string descripcion, DateTime fechaDesde, DateTime fechaHasta, string filtroGasto, bool desdePos)
        {
            var user = _usuarioActual;
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.Usuarios = ObtenerUsuariosFiltroEgresos();
            ViewBag.TiposEgresoCaja = _oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdUsuario = idUsuario;
            ViewBag.IdTipoEgresoCaja = idTipoEgresoCaja;
            ViewBag.Descripcion = descripcion ?? "";
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.FiltroGasto = filtroGasto;
            ViewBag.DesdePOS = desdePos;
            ViewBag.UsuarioAdmin = user.Admin;
            ViewBag.PuedeVerTiposEgreso = true;
            ViewBag.PuedeEditarTiposEgreso = true;
        }

        private DataTable ObtenerUsuariosFiltroEgresos()
        {
            var dtUsuarios = _oUsuarioN.obtenerUsuarios(true);

            if (dtUsuarios != null && dtUsuarios.Columns.Contains("id") && dtUsuarios.Columns.Contains("nombre"))
            {
                DataRow drTodos = dtUsuarios.NewRow();
                drTodos["id"] = -1;
                drTodos["nombre"] = "Todos";
                dtUsuarios.Rows.Add(drTodos);
                dtUsuarios.DefaultView.Sort = "id";
            }

            return dtUsuarios;
        }

        // Tipos reservados que el sistema inserta solo (reflejo de una venta con forma de pago no
        // efectivo o de un movimiento de cuenta corriente) -- no son egresos de caja reales, nunca
        // se muestran en este listado.
        private static readonly string[] TiposExcluidosDeEgresosCaja = { "Cta Cte", "Pago Electronico" };

        private DataTable ExcluirTiposReservadosEgresosCaja(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("TipoEgresoCaja"))
                return dt;

            DataTable resultado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                string tipo = Convert.ToString(row["TipoEgresoCaja"]);
                bool esReservadoExcluido = TiposExcluidosDeEgresosCaja.Any(t => string.Equals(t, tipo, StringComparison.OrdinalIgnoreCase));
                if (!esReservadoExcluido)
                    resultado.ImportRow(row);
            }

            return resultado;
        }

        private CalcularComisionesElectronicasVm CrearModelComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, DateTime fechaEgreso, int idSucursal, bool desdePos, int idCierre)
        {
            var sucursal = idSucursal > 0 ? _oSucursalN.findById(idSucursal) : null;
            var porcentajesDefault = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "Debito", Convert.ToDecimal(_param.GetFloat(ParamKeys.ComisionDebito, 0f)) },
                { "Credito", Convert.ToDecimal(_param.GetFloat(ParamKeys.ComisionCredito, 0f)) },
                { "Qr", 0m },
                { "Transferencia", 0m }
            };

            var formas = ObtenerFormasPagoElectronicas(fechaDesde, fechaHasta, idSucursal, porcentajesDefault);
            return new CalcularComisionesElectronicasVm
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                FechaEgreso = fechaEgreso,
                IdSucursal = idSucursal,
                SucursalNombre = sucursal != null ? sucursal.SucursalNombre : "Todas",
                IdTipoEgresoCaja = 0,
                DesdePos = desdePos,
                IdCierre = idCierre,
                FormasPago = formas,
                TotalEgreso = formas.Sum(f => f.ImporteComision)
            };
        }

        private List<ComisionElectronicaFormaVm> ObtenerFormasPagoElectronicas(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, IDictionary<string, decimal> porcentajes)
        {
            var items = new List<ComisionElectronicaFormaVm>
            {
                new ComisionElectronicaFormaVm { Codigo = "Debito", Nombre = "Débito", Porcentaje = ObtenerPorcentaje(porcentajes, "Debito") },
                new ComisionElectronicaFormaVm { Codigo = "Credito", Nombre = "Crédito", Porcentaje = ObtenerPorcentaje(porcentajes, "Credito") },
                new ComisionElectronicaFormaVm { Codigo = "Qr", Nombre = "QR", Porcentaje = ObtenerPorcentaje(porcentajes, "Qr") },
                new ComisionElectronicaFormaVm { Codigo = "Transferencia", Nombre = "Transferencia", Porcentaje = ObtenerPorcentaje(porcentajes, "Transferencia") }
            };

            var ventas = _oVentaN.getAllVentas(fechaDesde.Date, fechaHasta.Date, "", -1, -1, idSucursal, false, false) ?? new List<Entidades.Venta>();
            foreach (var venta in ventas)
            {
                if (venta == null || string.Equals(venta.Estado ?? "", "ANULADO", StringComparison.OrdinalIgnoreCase))
                    continue;

                var item = items.FirstOrDefault(i => string.Equals(i.Codigo, venta.FormaPago ?? "", StringComparison.OrdinalIgnoreCase));
                if (item == null)
                    continue;

                decimal totalVenta = Convert.ToDecimal(venta.TotalImporte);
                decimal pagoMixtoEfectivo = Convert.ToDecimal(venta.PagoMixtoEfectivo);
                decimal totalElectronico = pagoMixtoEfectivo > 0m ? totalVenta - pagoMixtoEfectivo : totalVenta;
                if (totalElectronico <= 0m)
                    continue;

                item.TotalCobrado += totalElectronico;
            }

            foreach (var item in items)
            {
                item.ImporteComision = decimal.Round(item.TotalCobrado * item.Porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
            }

            return items;
        }

        private decimal ObtenerPorcentaje(IDictionary<string, decimal> porcentajes, string codigo)
        {
            if (porcentajes == null || string.IsNullOrWhiteSpace(codigo))
                return 0m;

            decimal valor;
            return porcentajes.TryGetValue(codigo, out valor) ? valor : 0m;
        }

        private string FormatearImporte(decimal valor)
        {
            return valor.ToString("N2", new CultureInfo("es-AR"));
        }

        private string FormatearPorcentaje(decimal valor)
        {
            return valor.ToString("0.##", new CultureInfo("es-AR"));
        }

        private string ObtenerDescripcionSucursal(int idSucursal)
        {
            if (idSucursal <= 0)
                return "Todas";

            var sucursal = _oSucursalN.findById(idSucursal);
            return sucursal != null && !string.IsNullOrWhiteSpace(sucursal.SucursalNombre)
                ? sucursal.SucursalNombre
                : "Todas";
        }

        private decimal ParseDecimalFlexible(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            string limpio = value.Trim()
                .Replace("$", "")
                .Replace(" ", "");

            decimal numero;
            if (decimal.TryParse(limpio, NumberStyles.Any, new CultureInfo("es-AR"), out numero))
                return numero;

            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
                return numero;

            int ultimaComa = limpio.LastIndexOf(',');
            int ultimoPunto = limpio.LastIndexOf('.');
            if (ultimaComa >= 0 && ultimoPunto >= 0)
            {
                char separadorDecimal = ultimaComa > ultimoPunto ? ',' : '.';
                limpio = separadorDecimal == ','
                    ? limpio.Replace(".", "").Replace(',', '.')
                    : limpio.Replace(",", "");
            }
            else
            {
                limpio = limpio.Replace(',', '.');
            }

            return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out numero)
                ? numero
                : 0m;
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
    }

    // Port de Web/Helpers/EgresosCajaPolicy.cs. La unica diferencia real respecto al original: la
    // rama "no desde POS" llamaba a PermisosHelper.TienePermiso(usuario, empresa,
    // EgresosCaja.AltaEdicion, fecha, creadoPor) (chequeo de permiso vs Session) -- se reemplaza
    // por el mismo bypass de Admin ya usado en toda esta migracion (usuario.Admin), ya que
    // PermisosHelper.TienePermiso hace exactamente eso mismo internamente para un usuario Admin.
    // El resto de las reglas de negocio reales (que registro es una compra/pago electronico/
    // cuenta corriente, que sucursal/caja abierta aplica desde POS) se portan tal cual.
    public sealed class EgresoCajaPermisoResultado
    {
        public bool PuedeModificar { get; set; }
        public string MensajeBloqueo { get; set; } = "";
    }

    public static class EgresosCajaPolicy
    {
        public static EgresoCajaPermisoResultado EvaluarModificacion(
            Entidades.Usuario usuario,
            EgresoCaja egreso,
            bool desdePos,
            IEmpresaContext empresa,
            Func<DateTime, Sucursal, Entidades.Usuario, bool> validarCajaAbierta)
        {
            if (usuario == null)
                return Bloqueado("Sesion invalida.");

            if (egreso == null || egreso.Id == 0)
                return Bloqueado("No se encontro el egreso de caja.");

            if (EsCompra(egreso))
                return Bloqueado("Este registro corresponde a una compra y debe modificarse desde Compras.");

            if (EsPagoElectronico(egreso))
                return Bloqueado("Los pagos electronicos se modifican desde Ventas.");

            if (EsCuentaCorriente(egreso))
                return Bloqueado("Los movimientos de cuenta corriente se modifican desde su modulo original.");

            if (!desdePos)
            {
                return usuario.Admin
                    ? Permitido()
                    : Bloqueado("No tiene permisos para modificar este egreso de caja.");
            }

            if (egreso.Sucursal == null || egreso.Sucursal.idSucursal <= 0)
                return Bloqueado("No se pudo determinar la sucursal del egreso.");

            if (usuario.IdSucursal <= 0 || usuario.IdSucursal != egreso.Sucursal.idSucursal)
                return Bloqueado("Solo puede modificar egresos de la sucursal activa en la sesion.");

            if (validarCajaAbierta == null || !validarCajaAbierta(egreso.Fecha, egreso.Sucursal, usuario))
                return Bloqueado("La fecha y hora del egreso debe corresponder a una caja abierta del vendedor.");

            return Permitido();
        }

        public static bool EsPagoElectronico(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return egreso.IdTipoEgresoCaja == EgresoCaja.idPagoTarjeta ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Ventas.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCuentaCorriente(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return egreso.esEgresoCtaCte(egreso.IdTipoEgresoCaja) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Pagos.ToString(), StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.MovCtaCte.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsCompra(EgresoCaja egreso)
        {
            if (egreso == null)
                return false;

            return (egreso.IdCompra.HasValue && egreso.IdCompra.Value > 0) ||
                   string.Equals(egreso.Tabla, EgresoCaja.tablas.Compras.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static EgresoCajaPermisoResultado Permitido()
        {
            return new EgresoCajaPermisoResultado
            {
                PuedeModificar = true,
                MensajeBloqueo = string.Empty
            };
        }

        private static EgresoCajaPermisoResultado Bloqueado(string mensaje)
        {
            return new EgresoCajaPermisoResultado
            {
                PuedeModificar = false,
                MensajeBloqueo = mensaje ?? "No se puede modificar este egreso."
            };
        }
    }
}
