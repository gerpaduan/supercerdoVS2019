// Pedidos de compra armados desde Stock > Existencia por sucursales ("Ingresar pedido"): guardar, listar, ver el detalle, modificar (con la
// clave de otra persona si quien modifica es un usuario de produccion) y sacar el ticket, el PDF, el Excel o mandarlo por mail.
// Solo Postgres (NegocioFactory.CrearPedido). Las reglas viven en Negocio.Pedido (puras, con tests); aca se validan permisos y sesion, se
// arma el catalogo de productos y sucursales de la empresa y se responde. Detalle y porques: docs/DECISIONS.md (2026-10-19, Pedidos) y
// docs/11-manual-de-uso/pantallas/Pedidos.*.md.
//
// Permisos: ver, listar y guardar = Stock.VerStock (igual que "Que pedir"); modificar = Stock.AddOrEditStock. Si quien modifica es un usuario de
// PRODUCCION (cuenta compartida), ademas hace falta usuario y clave de otra persona autorizada (admin o con Stock.AddOrEditStock), con freno de
// intentos por sesion (PosOperadorStepUpRateLimiter); queda como "autorizador" en el historial del pedido.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utilidades;
using WebCore.Models.DTO;

namespace WebCore.Controllers
{
    public class PedidosController : Controller
    {
        private const string VistaTabla = "~/Views/Pedidos/_Tabla.cshtml";
        private const string MensajeSoloPostgres = "Esta pantalla solo está disponible cuando el sistema usa Postgres.";

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Usuario _oUsuarioN;
        // null si el motor de datos no es Postgres: las pantallas muestran un aviso y no se puede guardar ni consultar.
        private readonly Negocio.Pedido? _oPedidoN;
        private readonly ILogger<PedidosController> _log;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public PedidosController(WebCore.Services.IUsuarioSesionService sesion, ILogger<PedidosController> log)
        {
            _sesion = sesion;
            _log = log;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oPedidoN = WebCore.Infrastructure.NegocioFactory.UsarPostgres
                ? WebCore.Infrastructure.NegocioFactory.CrearPedido(_empresa)
                : null;
        }

        // ---------- Permisos ----------

        private bool TienePermisoVer(Entidades.Usuario? user)
        {
            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Stock.VerStock, DateTime.Today, -1);
        }

        private bool PuedeModificar(Entidades.Usuario? user)
        {
            if (user == null || !user.Activo) return false;
            return user.Admin || _oUsuarioN.tienePermiso(user, Entidades.Permisos.Stock.AddOrEditStock, DateTime.Today, -1);
        }

        // Mismo criterio que Stock/Movimientos/Elaborados: con una cuenta de produccion y una persona elegida, el pedido queda a nombre de esa persona.
        private Entidades.Usuario ResolverUsuarioCreador(int idUsuarioCreador, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion || idUsuarioCreador <= 0)
                return usuarioSesion;

            var candidato = _oUsuarioN.getUsuarioById(idUsuarioCreador);
            if (candidato == null || !candidato.Activo || candidato.IdEmpresa != usuarioSesion.IdEmpresa)
                return usuarioSesion;

            return candidato;
        }

        // ASP.NET Core Session no manda el Set-Cookie hasta el primer write: sin esto el freno de intentos por sesion nunca acumula.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // ---------- Catalogo (para validar y rotular lo que manda el navegador) ----------

        private List<Entidades.Corte> CatalogoProductos()
        {
            int idEmpresa = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
            return idEmpresa > 0
                ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresa, false) ?? new List<Entidades.Corte>())
                : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>());
        }

        private List<Entidades.Sucursal> SucursalesEmpresa()
        {
            return (_oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
                .Where(s => s != null && s.IdSucursal > 0)
                .GroupBy(s => s.IdSucursal).Select(g => g.First())
                .OrderBy(s => s.SucursalNombre)
                .ToList();
        }

        private IActionResult SinPermiso()
        {
            ViewBag.Seccion = "Stock";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        // ---------- Listado ----------

        [HttpGet]
        public IActionResult Index()
        {
            if (!TienePermisoVer(_usuarioActual)) return SinPermiso();

            ViewBag.Title = "Pedidos";
            ViewBag.Seccion = "Stock";
            ViewBag.Disponible = _oPedidoN != null;
            ViewBag.Mensaje = _oPedidoN == null ? MensajeSoloPostgres : "";
            ViewBag.Sucursales = SucursalesEmpresa();
            // Por defecto los ultimos 30 dias.
            ViewBag.Desde = DateTime.Today.AddDays(-30);
            ViewBag.Hasta = DateTime.Today;

            var filtro = new Entidades.PedidoFiltro { Desde = DateTime.Today.AddDays(-30), Hasta = DateTime.Today };
            var lista = _oPedidoN != null ? ConsultarLista(filtro) : new List<Entidades.PedidoResumen>();
            ViewBag.Lista = lista;
            return View("~/Views/Pedidos/Index.cshtml");
        }

        [HttpGet]
        public IActionResult Buscar(DateTime? desde = null, DateTime? hasta = null, int idSucursal = 0, string usuario = "", string texto = "")
        {
            if (!TienePermisoVer(_usuarioActual)) return StatusCode(403, "Sin permiso para ver pedidos.");
            if (_oPedidoN == null) return BadRequest(MensajeSoloPostgres);

            var filtro = new Entidades.PedidoFiltro
            {
                Desde = desde,
                Hasta = hasta,
                IdSucursal = idSucursal > 0 && SucursalesEmpresa().Any(s => s.IdSucursal == idSucursal) ? idSucursal : 0,
                Usuario = usuario,
                Texto = texto
            };
            ViewBag.Lista = ConsultarLista(filtro);
            return PartialView(VistaTabla);
        }

        private List<Entidades.PedidoResumen> ConsultarLista(Entidades.PedidoFiltro filtro)
        {
            try
            {
                return _oPedidoN!.Listar(filtro);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "No se pudo listar los pedidos.");
                ViewBag.ErrorLista = "No se pudo consultar los pedidos. Intentá de nuevo.";
                return new List<Entidades.PedidoResumen>();
            }
        }

        // ---------- Detalle y edicion ----------

        [HttpGet]
        public IActionResult Detalle(int id)
        {
            if (!TienePermisoVer(_usuarioActual)) return SinPermiso();
            if (_oPedidoN == null) return BadRequest(MensajeSoloPostgres);

            var pedido = _oPedidoN.ObtenerPorId(id);
            if (pedido == null) return NotFound();

            ViewBag.Title = "Pedido N° " + pedido.Id;
            ViewBag.Seccion = "Stock";
            ViewBag.Pivot = WebCore.Services.PedidoPivot.Construir(pedido);
            ViewBag.PuedeModificar = PuedeModificar(_usuarioActual) || _usuarioActual.EsUsuarioProduccion;
            return View("~/Views/Pedidos/Detalle.cshtml", pedido);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!TienePermisoVer(_usuarioActual)) return SinPermiso();
            if (_oPedidoN == null) return BadRequest(MensajeSoloPostgres);

            // Un usuario de produccion entra: la clave de la persona autorizada se pide al guardar. Los demas necesitan el permiso de editar stock.
            var user = _usuarioActual;
            if (!user.EsUsuarioProduccion && !PuedeModificar(user)) return SinPermiso();

            var pedido = _oPedidoN.ObtenerPorId(id);
            if (pedido == null) return NotFound();

            ViewBag.Title = "Modificar pedido N° " + pedido.Id;
            ViewBag.Seccion = "Stock";
            ViewBag.Sucursales = SucursalesEmpresa();
            ViewBag.Version = Negocio.Pedido.Version(pedido);
            ViewBag.RequiereAutorizacion = user.EsUsuarioProduccion;
            return View("~/Views/Pedidos/Editar.cshtml", pedido);
        }

        // ---------- Guardar (alta) ----------

        // Cuerpo JSON (PedidoGuardarDto). Respuesta: { ok, id, yaExistia, totalEstimado, lineasSinPrecio, urls... } o { ok:false, mensaje }.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar([FromBody] PedidoGuardarDto dto)
        {
            if (!TienePermisoVer(_usuarioActual)) return StatusCode(403, new { ok = false, mensaje = "Sin permiso para guardar pedidos." });
            if (_oPedidoN == null) return StatusCode(409, new { ok = false, mensaje = MensajeSoloPostgres });
            if (dto == null) return BadRequest(new { ok = false, mensaje = "Pedido inválido." });

            try
            {
                var usuario = ResolverUsuarioCreador(dto.IdUsuarioCreador, _usuarioActual);
                var resultado = _oPedidoN.Guardar(AlSolicitud(dto), CatalogoProductos(), SucursalesEmpresa(), usuario, DateTime.Now);
                if (!resultado.Ok) return Json(new { ok = false, mensaje = resultado.Error });

                var pedido = resultado.Pedido ?? _oPedidoN.ObtenerPorId(resultado.IdPedido);
                return Json(new
                {
                    ok = true,
                    id = resultado.IdPedido,
                    yaExistia = resultado.YaExistia,
                    totalEstimado = pedido != null ? pedido.TotalEstimado : 0m,
                    lineasSinPrecio = pedido != null ? pedido.LineasSinPrecio : 0,
                    acciones = AccionesPedido(resultado.IdPedido)
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "No se pudo guardar el pedido.");
                return StatusCode(500, new { ok = false, mensaje = "No se pudo guardar el pedido. Intentá de nuevo." });
            }
        }

        // ---------- Modificar ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(int id, [FromBody] PedidoGuardarDto dto)
        {
            var user = _usuarioActual;
            if (!TienePermisoVer(user)) return StatusCode(403, new { ok = false, mensaje = "Sin permiso para modificar pedidos." });
            if (_oPedidoN == null) return StatusCode(409, new { ok = false, mensaje = MensajeSoloPostgres });
            if (dto == null) return BadRequest(new { ok = false, mensaje = "Pedido inválido." });

            try
            {
                Entidades.Usuario? autorizador = null;

                if (user.EsUsuarioProduccion)
                {
                    // Cuenta de produccion: hace falta usuario y clave de otra persona autorizada.
                    var autorizacion = ValidarAutorizador(dto.Supervisor);
                    if (!autorizacion.ok)
                        return Json(new { ok = false, requiereAutorizacion = true, bloqueado = autorizacion.bloqueado, mensaje = autorizacion.mensaje });
                    autorizador = autorizacion.autorizador;
                }
                else if (!PuedeModificar(user))
                {
                    return StatusCode(403, new { ok = false, mensaje = "No tenés permiso para modificar pedidos." });
                }

                // Con cuenta de produccion el cambio se asienta a nombre de la persona elegida (si hay) y el autorizador queda aparte.
                var usuario = ResolverUsuarioCreador(dto.IdUsuarioCreador, user);
                var resultado = _oPedidoN.Modificar(id, AlSolicitud(dto), CatalogoProductos(), SucursalesEmpresa(), usuario, autorizador, DateTime.Now);
                if (!resultado.Ok) return Json(new { ok = false, mensaje = resultado.Error });

                return Json(new
                {
                    ok = true,
                    id = resultado.IdPedido,
                    totalEstimado = resultado.Pedido != null ? resultado.Pedido.TotalEstimado : 0m,
                    lineasSinPrecio = resultado.Pedido != null ? resultado.Pedido.LineasSinPrecio : 0,
                    acciones = AccionesPedido(resultado.IdPedido)
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "No se pudo modificar el pedido {IdPedido}.", id);
                return StatusCode(500, new { ok = false, mensaje = "No se pudo modificar el pedido. Intentá de nuevo." });
            }
        }

        // Valida usuario y clave de la persona que autoriza. Mensaje unico para cualquier falla (no revela si el usuario existe ni si tiene permiso).
        private (bool ok, bool bloqueado, string mensaje, Entidades.Usuario? autorizador) ValidarAutorizador(SupervisorAutorizacionDto? supervisor)
        {
            const string requerida = "Para modificar un pedido hace falta el usuario y la contraseña de una persona autorizada.";
            if (supervisor == null || string.IsNullOrWhiteSpace(supervisor.Usuario) || string.IsNullOrWhiteSpace(supervisor.Clave))
                return (false, false, requerida, null);

            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return (false, true, "Demasiados intentos. Esperá " + (int)Math.Ceiling(retryAfter.TotalSeconds) + " segundos.", null);

            var validado = _oUsuarioN.ValidarUsuarioWeb(supervisor.Usuario, supervisor.Clave);
            // Una cuenta de produccion es compartida: no identifica a nadie, asi que no puede autorizar (ni la propia ni otra). Mismo criterio que
            // VentasController, donde la cuenta de produccion tampoco puede ser su propio operador. Mismo mensaje que cualquier otra falla.
            if (validado == null || !validado.Activo || validado.EsUsuarioProduccion || !PuedeModificar(validado))
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                // Auditoria del intento fallido (nunca la clave): quien pidio autorizacion y el usuario que se ingreso como autorizador.
                _log.LogWarning("Autorización rechazada para modificar un pedido: solicitante {IdUsuario}, autorizador ingresado {Autorizador}.",
                    _usuarioActual?.Id, TextoSeguroParaLog(supervisor.Usuario));
                return (false, false, "Usuario o contraseña incorrectos, o sin permiso para modificar pedidos.", null);
            }

            WebCore.Helpers.PosOperadorStepUpRateLimiter.Reset(sessionId);
            return (true, false, "", validado);
        }

        // Texto que escribio el usuario, apto para un log: sin saltos de linea ni caracteres de control (evita inyectar lineas falsas) y acotado.
        private static string TextoSeguroParaLog(string? texto)
        {
            const int maxLargo = 60;
            var limpio = new string((texto ?? "").Where(c => !char.IsControl(c)).ToArray()).Trim();
            return limpio.Length > maxLargo ? limpio.Substring(0, maxLargo) : limpio;
        }

        private static Negocio.Pedido.Solicitud AlSolicitud(PedidoGuardarDto dto)
        {
            return new Negocio.Pedido.Solicitud
            {
                ClientId = dto.ClientId,
                Nota = dto.Nota,
                Version = dto.Version,
                Lineas = (dto.Lineas ?? new List<PedidoLineaDto>()).Where(l => l != null).Select(l => new Negocio.Pedido.LineaSolicitada
                {
                    IdCorte = l.IdCorte,
                    IdSucursal = l.IdSucursal,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    Proveedor = l.Proveedor,
                    FechaPrecio = l.FechaPrecio,
                    StockActual = l.StockActual,
                    PuntoStock = l.PuntoStock
                }).ToList()
            };
        }

        // URLs de las acciones sobre un pedido (las usa el dialogo "¿Qué querés hacer ahora?" de pedido-acciones.js).
        private object AccionesPedido(int id)
        {
            return new
            {
                detalleUrl = Url.Action("Detalle", "Pedidos", new { id }),
                ticketUrl = Url.Action("Ticket", "Pedidos", new { id }),
                ticketPayloadUrl = Url.Action("TicketPayload", "Pedidos", new { id }),
                pdfUrl = Url.Action("Pdf", "Pedidos", new { id }),
                excelUrl = Url.Action("Excel", "Pedidos", new { id }),
                emailDatosUrl = Url.Action("DatosEmail", "Pedidos", new { id }),
                emailEnviarUrl = Url.Action("EnviarEmail", "Pedidos")
            };
        }

        // ---------- Ticket, PDF, Excel y mail ----------

        private Entidades.Pedido? PedidoParaSalida(int id)
        {
            if (_oPedidoN == null || !TienePermisoVer(_usuarioActual)) return null;
            return _oPedidoN.ObtenerPorId(id);
        }

        // HTML del ticket: fallback por navegador (iframe oculto de ticket-print.js). Mismo texto que el payload del agente ESC/POS.
        [HttpGet]
        public IActionResult Ticket(int id, int mm = 80)
        {
            var pedido = PedidoParaSalida(id);
            if (pedido == null) return NotFound();

            int ticketMm = WebCore.Services.ComprobanteTickets.NormalizarMm(mm);
            ViewBag.TicketMm = ticketMm;
            ViewBag.TicketTitulo = "Pedido " + pedido.Id;
            // Sin formato ESC/POS: este HTML lo imprime el navegador y mostraria los caracteres de control.
            ViewBag.TicketLineas = string.Join("\n", WebCore.Services.PedidoTicket.ConstruirLineas(pedido, ticketMm, false));
            return View("~/Views/Shared/_TicketTexto.cshtml");
        }

        // Payload JSON para el agente de impresion ESC/POS local.
        [HttpGet]
        public IActionResult TicketPayload(int id, int mm = 80)
        {
            var pedido = PedidoParaSalida(id);
            if (pedido == null) return Json(new { ok = false, mensaje = "No se encontró el pedido." });

            int ticketMm = WebCore.Services.ComprobanteTickets.NormalizarMm(mm);
            return Json(new
            {
                ok = true,
                ticketMm,
                ticketLines = WebCore.Services.PedidoTicket.ConstruirLineas(pedido, ticketMm, true)
            });
        }

        [HttpGet]
        public IActionResult Pdf(int id)
        {
            var pedido = PedidoParaSalida(id);
            if (pedido == null) return NotFound();
            return File(WebCore.Services.PdfPedido.Generar(pedido), "application/pdf", "Pedido_" + pedido.Id + ".pdf");
        }

        [HttpGet]
        public IActionResult Excel(int id)
        {
            var pedido = PedidoParaSalida(id);
            if (pedido == null) return NotFound();
            return File(WebCore.Services.ExcelPedido.Generar(pedido),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pedido_" + pedido.Id + ".xlsx");
        }

        private string NombreEmpresa(out string replyTo)
        {
            replyTo = "";
            try
            {
                var empresa = WebCore.Infrastructure.NegocioFactory.CrearEmpresa(_empresa).findById(_empresa.IdEmpresa);
                if (empresa != null)
                {
                    replyTo = (empresa.Email ?? "").Trim();
                    string nombre = !string.IsNullOrWhiteSpace(empresa.NombreFantasia) ? empresa.NombreFantasia : empresa.RazonSocialAfip;
                    if (!string.IsNullOrWhiteSpace(nombre)) return nombre;
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "No se pudo leer la empresa para armar el mail del pedido.");
            }
            return "CarniSys";
        }

        // Datos para el formulario de mail (asunto y mensaje sugeridos; el destinatario lo escribe el usuario).
        [HttpGet]
        public IActionResult DatosEmail(int id)
        {
            var pedido = PedidoParaSalida(id);
            if (pedido == null) return Json(new { ok = false, msg = "Pedido no encontrado." });

            var pivot = WebCore.Services.PedidoPivot.Construir(pedido);
            var culturaAr = new CultureInfo("es-AR");
            string empresa = NombreEmpresa(out _);
            string cuerpo =
                "Hola,\n\n" +
                "Te enviamos adjunto el pedido N° " + pedido.Id + " del " + pedido.Fecha.ToString("dd/MM/yyyy", culturaAr) + ".\n\n" +
                "Productos: " + pivot.Filas.Count + "\n" +
                global::Negocio.PedidoTextos.EtiquetaTotal(pedido.LineasSinPrecio) + ": $" + pedido.TotalEstimado.ToString("N2", culturaAr) + "\n\n" +
                // Mismo texto que la pantalla, el ticket, el PDF y el Excel (aclaracion del precio + cuantas lineas no suman).
                global::Negocio.PedidoTextos.Aclaracion(pedido.LineasSinPrecio) + "\n\n" +
                "Saludos,\n" + empresa;

            return Json(new { ok = true, email = "", asunto = "Pedido N° " + pedido.Id + " - " + empresa, mensaje = cuerpo });
        }

        // Envia mail real con el PDF adjunto: valida anti-forgery (el JS manda el token en el header) para que una pagina ajena no pueda disparar envios.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarEmail(int id, string emailDestino, string asunto, string mensaje)
        {
            try
            {
                var pedido = PedidoParaSalida(id);
                if (pedido == null) return Json(new { ok = false, msg = "Pedido no encontrado." });

                emailDestino = (emailDestino ?? "").Trim();
                asunto = (asunto ?? "").Trim();
                mensaje = (mensaje ?? "").Trim();

                if (string.IsNullOrWhiteSpace(emailDestino)) return Json(new { ok = false, msg = "Ingrese un email destino." });
                if (!SmtpMailHelper.IsValidEmail(emailDestino)) return Json(new { ok = false, msg = "Ingrese un email válido." });
                if (string.IsNullOrWhiteSpace(asunto)) return Json(new { ok = false, msg = "Ingrese un asunto." });

                string empresa = NombreEmpresa(out string replyToEmail);

                SmtpMailHelper.SendMail(
                    toEmail: emailDestino,
                    toName: "",
                    subject: asunto,
                    bodyHtml: WebCore.Services.MailCuerpoHtml.DesdeTexto(mensaje),
                    attachmentFileName: "Pedido_" + pedido.Id + ".pdf",
                    attachmentBytes: WebCore.Services.PdfPedido.Generar(pedido),
                    attachmentContentType: "application/pdf",
                    fromNameOverride: SmtpMailHelper.BuildTenantFromName(empresa),
                    replyToEmail: SmtpMailHelper.IsValidEmail(replyToEmail) ? replyToEmail : null,
                    replyToName: empresa
                );

                return Json(new { ok = true, msg = "El pedido se envió correctamente." });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "No se pudo enviar por mail el pedido {IdPedido}.", id);
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo enviar el email. Intentá de nuevo." });
            }
        }
    }
}
