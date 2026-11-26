// POS sin conexion (2026-11-26): endpoints que usa el POS para preparar el modo offline y para subir lo que hizo
// mientras no habia red (ventas y aperturas de caja). Ver docs/DECISIONS.md "POS sin conexion" y
// docs/03-modulos/ventas-y-pos.md.
//
// Puntos que conviene no perder de vista:
//  - Con PosOffline:Habilitado apagado (o la sucursal fuera de PosOffline:SoloSucursales) todo responde
//    deshabilitado=true y nada toca la base: el POS sigue como antes.
//  - Idempotencia real: cada venta/apertura trae un clientId de la PC; posclaveunica tiene indice unico y el INSERT va
//    en la misma transaccion que la venta (Negocio.Venta.agregarVentaConClave). Reintentar nunca duplica.
//  - Solo sube el propio vendedor: el vendedor que vendio con PIN tiene que ser el operador de la sesion que sube.
//  - Nunca se pierde una venta cobrada: lo que no se puede grabar vuelve con rechazada=true y queda en la PC.
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using WebCore.Helpers;
using WebCore.Models.DTO;

namespace WebCore.Controllers
{
    public partial class VentasController
    {
        private JsonResult PosOfflineDeshabilitado() => Json(new { ok = false, deshabilitado = true });

        // Estado para el POS: si rige el modo offline, quien es el operador, su caja, la configuracion y un token
        // antiforgery fresco (la pagina offline es estatica y no trae el del render). El POS lo llama al abrir y al
        // volver la conexion, antes de subir la cola.
        [HttpGet]
        public IActionResult EstadoPosOffline(string? posInstanceId)
        {
            var user = _usuarioActual;
            if (user == null || !PosOfflineSettings.HabilitadoPara(user.IdSucursal))
                return PosOfflineDeshabilitado();

            var operador = ResolverOperadorPOS(posInstanceId, user);
            bool requiereOperador = user.EsUsuarioProduccion && operador != null && operador.Id == user.Id;
            var caja = requiereOperador ? null : ObtenerCierreCajaActual(operador);
            var tokens = HttpContext.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(HttpContext);

            return Json(new
            {
                ok = true,
                deshabilitado = false,
                token = tokens.RequestToken,
                requiereOperador,
                operador = requiereOperador || operador == null ? null : new { id = operador.Id, nombre = operador.Nombre ?? "" },
                sucursal = new { id = user.IdSucursal, nombre = user.Sucursal?.SucursalNombre ?? "" },
                caja = caja == null ? null : new
                {
                    id = caja.Id,
                    fechaInicio = caja.FechaHoraInicio?.ToString("yyyy-MM-ddTHH:mm:ss")
                },
                idConsumidorFinal = _param.GetInt(Entidades.ParamKeys.IdConsumidorFinal, 0),
                config = new
                {
                    minutosRefrescoCatalogo = PosOfflineSettings.MinutosRefrescoCatalogo,
                    fallasSeguidasParaCorte = PosOfflineSettings.FallasSeguidasParaCorte,
                    segundosTimeoutPing = PosOfflineSettings.SegundosTimeoutPing,
                    urlChequeoInternet = PosOfflineSettings.UrlChequeoInternet,
                    maxLineas = PosOfflineSettings.MaxLineas
                }
            });
        }

        // Catalogo para vender sin conexion: productos de la empresa (misma lista que ProductosController.ListarProductos y
        // misma busqueda por codigo que BuscarProducto), formatos de etiqueta de balanza activos, codigo base generico y
        // factores de precio por forma de pago. Con 'version' igual a la vigente responde sinCambios (no reenvia todo).
        // Limite conocido: el servidor arma el catalogo completo en cada pedido (para calcular la version); con pocos
        // miles de productos y un pedido cada PosOffline:MinutosRefrescoCatalogo por caja es aceptable.
        [HttpGet]
        public IActionResult CatalogoOffline(string? version)
        {
            var user = _usuarioActual;
            if (user == null || !PosOfflineSettings.HabilitadoPara(user.IdSucursal))
                return PosOfflineDeshabilitado();

            try
            {
                int idEmpresa = user.IdEmpresa;
                var productos = (_oCorteN.ObtenerCortesPorEmpresa(idEmpresa, false) ?? new List<Entidades.Corte>())
                    .Where(c => c != null && c.codigo >= 0)
                    .Select(c => new
                    {
                        id = c.IdCorte,
                        codigo = c.codigo,
                        nombre = c.CorteDesc ?? "",
                        // Mismo redondeo que BuscarProducto (precioKg del JSON).
                        precioKg = Math.Round((double)c.PrecioKg, 2),
                        pesable = c.Pesable
                    })
                    .ToList();

                var formatos = WebCore.Infrastructure.NegocioFactory.CrearFormatoCodigoBarras(_empresa).Listar(idEmpresa)
                    .Where(f => f != null && f.Activo)
                    .Select(f => new
                    {
                        prefijo = f.Prefijo,
                        nombre = f.Nombre ?? "",
                        longitudTotal = f.LongitudTotal,
                        posicionCodigo = f.PosicionCodigo,
                        longitudCodigo = f.LongitudCodigo,
                        posicionValor = f.PosicionValor,
                        longitudValor = f.LongitudValor,
                        tipoValor = f.TipoValor.ToString(),
                        cantidadDecimales = f.CantidadDecimales
                    })
                    .ToList();

                bool dispositivoAutorizado = SesionDesdeDispositivoAutorizado();
                var contenido = new
                {
                    codProdGenerico = _param.GetLong(Entidades.ParamKeys.CodProdGenerico, 0L),
                    productos,
                    formatos,
                    factores = ObtenerConfiguracionFormaPagoPOS(),
                    dispositivoAutorizado,
                    usuarios = dispositivoAutorizado ? UsuariosConPinParaOffline(user.IdSucursal) : new List<object>(),
                    // Encabezado del ticket X (mismo criterio que ConstruirLineasTicketVenta).
                    ticket = new
                    {
                        negocio = ObtenerNombreEmpresaVenta(null),
                        slogan1 = ObtenerEmpresaVenta(null)?.Slogan1 ?? "",
                        slogan2 = ObtenerEmpresaVenta(null)?.Slogan2 ?? "",
                        slogan3 = ObtenerEmpresaVenta(null)?.Slogan3 ?? "",
                        ocultarEncabezado = OcultarEncabezadoTicketNoFiscal()
                    },
                    sucursal = new { id = user.IdSucursal, nombre = user.Sucursal?.SucursalNombre ?? "" },
                    idConsumidorFinal = _param.GetInt(Entidades.ParamKeys.IdConsumidorFinal, 0)
                };

                // Version = huella del contenido: igual contenido, igual version (la PC no vuelve a bajar lo mismo).
                string json = System.Text.Json.JsonSerializer.Serialize(contenido);
                string versionActual = Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json))).Substring(0, 16);
                if (string.Equals(version, versionActual, StringComparison.OrdinalIgnoreCase))
                    return Json(new { ok = true, sinCambios = true, version = versionActual });

                return Json(new
                {
                    ok = true,
                    sinCambios = false,
                    version = versionActual,
                    generado = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    contenido.codProdGenerico,
                    contenido.productos,
                    contenido.formatos,
                    contenido.factores,
                    contenido.dispositivoAutorizado,
                    contenido.usuarios,
                    contenido.ticket,
                    contenido.sucursal,
                    contenido.idConsumidorFinal
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CatalogoOffline: " + ex);
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo armar el catálogo sin conexión", error = ex.Message });
            }
        }

        // Sube UNA venta hecha sin conexion. Respuestas:
        //  ok=true (ventaId, yaRegistrada, llegoTarde, sinCaja, pendienteFacturar, difPrecio): grabada (o ya estaba).
        //  ok=false, rechazada=true (msg): no se puede grabar tal como esta; la PC la deja apartada para revisar.
        //  ok=false, requiereOperador / otroVendedor: falta que entre el vendedor correcto; la PC reintenta despues.
        //  ok=false (sin rechazada) o error HTTP: problema transitorio; la PC reintenta.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SincronizarVentaOffline([FromBody] SincronizarVentaOfflineRequest request)
        {
            try
            {
                var user = _usuarioActual;
                if (user == null || !PosOfflineSettings.HabilitadoPara(user.IdSucursal))
                    return PosOfflineDeshabilitado();
                if (request == null || request.ClientId == Guid.Empty)
                    return Json(new { ok = false, rechazada = true, msg = "La venta llegó sin código." });

                // Vendedor: el que vendio con PIN tiene que ser el operador de esta sesion (mismo criterio que
                // FinalizarVenta para la cuenta compartida de produccion).
                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                if (user.EsUsuarioProduccion && operador != null && operador.Id == user.Id)
                    return Json(new { ok = false, requiereOperador = true, msg = "Ingresá con tu usuario para subir las ventas sin conexión." });
                if (operador == null || operador.Id != request.IdVendedor)
                    return Json(new { ok = false, otroVendedor = true, msg = "Estas ventas son de otro vendedor: se suben cuando él entre al POS." });

                // Sucursal: la de la sesion, como en FinalizarVenta.
                if (request.IdSucursalPOS != user.IdSucursal)
                    return Json(new { ok = false, otraSucursal = true, msg = "La venta es de otra sucursal: cambiá a esa sucursal para subirla." });

                // Reintento de una venta ya grabada: se responde sin revalidar (la primera vez ya paso todo).
                var claveRepo = WebCore.Infrastructure.NegocioFactory.CrearPosClaveUnicaRepository(_empresa);
                var existente = claveRepo.ObtenerPorClientId(request.ClientId);
                if (existente != null)
                {
                    if (!string.Equals(existente.Tipo, Entidades.PosClaveUnica.TipoVenta, StringComparison.Ordinal))
                        return Json(new { ok = false, rechazada = true, msg = "El código de la venta ya se usó para otra operación." });
                    return Json(RespuestaVentaOffline(existente, yaRegistrada: true));
                }

                // Reglas de la venta offline (Consumidor Final, sin descuentos, formas de pago, fecha).
                var pedido = new Negocio.VentaOffline.Pedido
                {
                    IdPersona = request.IdPersona,
                    FormaPago = request.FormaPago,
                    EsPagoMixto = request.EsPagoMixto,
                    EfectivoMixto = request.Efectivo,
                    FechaLocal = request.FechaLocal,
                    Lineas = (request.Lineas ?? new List<LineaVentaOfflineDto>())
                        .Select(l => l == null ? null : new Negocio.VentaOffline.LineaPedido
                        {
                            Codigo = l.Codigo,
                            CantKg = l.CantKg,
                            PrecioKg = l.PrecioKg,
                            Bonificacion = l.Bonificacion
                        })
                        .ToList()
                };
                var limites = new Negocio.VentaOffline.Limites
                {
                    IdConsumidorFinal = _param.GetInt(Entidades.ParamKeys.IdConsumidorFinal, 0),
                    MaxLineas = PosOfflineSettings.MaxLineas,
                    MinutosToleranciaFuturo = PosOfflineSettings.MinutosToleranciaFuturo,
                    DiasMaximosAntiguedad = PosOfflineSettings.DiasMaximosAntiguedad
                };
                string motivo = Negocio.VentaOffline.Validar(pedido, limites, DateTime.Now);
                if (motivo != null)
                    return Json(new { ok = false, rechazada = true, msg = motivo });

                var persona = _oPersonaN.findById(request.IdPersona);
                var sucursal = _oSucursalN.findById(user.IdSucursal);
                if (persona == null || sucursal == null)
                    return Json(new { ok = false, rechazada = true, msg = "No se encontró Consumidor Final o la sucursal." });

                // Productos: se re-resuelven por codigo. Uno que ya no existe no se puede grabar (queda para revisar).
                var lineasDto = request.Lineas!;
                var cortes = new Dictionary<long, Entidades.Corte>();
                foreach (long codigo in lineasDto.Select(l => l.Codigo).Distinct())
                {
                    var corte = _oCorteN.findCorteByCodigoEmpresa(codigo, user.IdEmpresa, false);
                    if (corte == null)
                        return Json(new { ok = false, rechazada = true, msg = "El producto " + codigo + " ya no existe en el sistema." });
                    cortes[codigo] = corte;
                }

                // Caja por FECHA DE LA VENTA (no por la hora de llegada): abierta, ya cerrada (llego tarde) o ninguna.
                var cajaAbierta = ObtenerCierreCajaActual(operador);
                var cajaCerrada = _oCierreN.BuscarCajaCerradaQueContiene(operador.Id, user.IdSucursal, request.FechaLocal);
                var estadoCaja = Negocio.VentaOffline.ClasificarCaja(request.FechaLocal, cajaAbierta, cajaCerrada);

                // Precios: se acepta lo cobrado; si difiere de la lista vigente (con cualquier factor de forma de pago)
                // queda marcado. Las lineas con precio de etiqueta o generico no se comparan.
                var factores = ObtenerConfiguracionFormaPagoPOS().Values.Distinct().ToList();
                var preciosAceptables = cortes.ToDictionary(
                    kv => kv.Key,
                    kv => (IReadOnlyCollection<decimal>)factores
                        .Select(f => Math.Round((decimal)kv.Value.PrecioKg * f, 2))
                        .ToList());
                var lineasCatalogo = lineasDto
                    .Where(l => string.IsNullOrEmpty(l.OrigenPrecio) || string.Equals(l.OrigenPrecio, "catalogo", StringComparison.OrdinalIgnoreCase))
                    .Select(l => new Negocio.VentaOffline.LineaPedido { Codigo = l.Codigo, PrecioKg = l.PrecioKg });
                string diferencias = Negocio.VentaOffline.DetalleDiferenciasPrecio(lineasCatalogo, preciosAceptables);

                bool pendienteFacturar = Negocio.VentaOffline.QuedaPendienteDeFacturar(
                    request.FormaPago,
                    FormasPagoFacturaAutomatica.Parse(_param.GetString(Entidades.ParamKeys.FormasPagoFacturaAutomatica, "")),
                    EmpresaTieneCertificadoFacturaElectronica(user));

                // Venta: mismos campos que FinalizarVenta para una venta a Consumidor Final (ticket X, sin cta cte).
                string formaPago = Negocio.VentaOffline.FormasPagoPermitidas
                    .First(f => string.Equals(f, request.FormaPago, StringComparison.OrdinalIgnoreCase));
                var venta = new Entidades.Venta
                {
                    Persona = persona,
                    Sucursal = sucursal,
                    Vendedor = operador,
                    FechaVenta = request.FechaLocal,
                    TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()),
                    Observaciones = request.Observaciones ?? "",
                    FormaPago = formaPago,
                    EnCtaCte = false,
                    PagoMixtoEfectivo = request.EsPagoMixto ? (float)request.Efectivo : 0f,
                    ListaExpendios = new List<int>()
                };

                var lineas = lineasDto.Select(l => new Entidades.LineaVenta
                {
                    Corte = cortes[l.Codigo],
                    KgsTotalCalculado = (float)l.CantKg,
                    CantKg = (float)l.CantKg,
                    PrecioKg = (float)l.PrecioKg,
                    Bonificacion = 0f,
                    Estado = l.Estado,
                    IndexAnulado = l.IndexAnulado,
                    PesoBalanza = l.Balanza
                }).ToList();
                CompletarAnulacionesVenta(lineas);
                venta.LineasVenta = lineas;

                var clave = new Entidades.PosClaveUnica
                {
                    ClientId = request.ClientId,
                    Origen = Entidades.PosClaveUnica.OrigenOffline,
                    IdSucursal = user.IdSucursal,
                    IdUsuario = operador.Id,
                    IdUsuarioSync = user.Id,
                    FechaLocal = request.FechaLocal,
                    LlegoTarde = estadoCaja == Negocio.VentaOffline.EstadoCaja.LlegoTarde,
                    SinCaja = estadoCaja == Negocio.VentaOffline.EstadoCaja.SinCaja,
                    PendienteFacturar = pendienteFacturar,
                    DifPrecio = diferencias.Length > 0,
                    CausaCorte = Negocio.VentaOffline.NormalizarCausaCorte(request.CausaCorte),
                    VersionCatalogo = Recortar(request.VersionCatalogo, 40),
                    Detalle = diferencias.Length > 0 ? Recortar("Precios: " + diferencias, 4000) : null
                };

                var resultado = _oVentaN.agregarVentaConClave(venta, clave);
                if (resultado.YaRegistrada)
                {
                    // Otro envio simultaneo la grabo primero: se devuelve la suya.
                    var ganadora = claveRepo.ObtenerPorClientId(request.ClientId);
                    if (ganadora != null)
                        return Json(RespuestaVentaOffline(ganadora, yaRegistrada: true));
                    return Json(new { ok = true, ventaId = resultado.IdVenta, yaRegistrada = true });
                }

                clave.IdRegistro = resultado.IdVenta;
                return Json(RespuestaVentaOffline(clave, yaRegistrada: false));
            }
            catch (Exception ex)
            {
                // Transitorio desde el punto de vista de la PC: la venta sigue en la cola y se reintenta.
                System.Diagnostics.Trace.TraceError("SincronizarVentaOffline " + request?.ClientId + ": " + ex);
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error al subir la venta sin conexión", error = ex.Message });
            }
        }

        // Crea en el servidor una apertura de caja hecha sin conexion, con la hora real en que se abrio. Si el vendedor ya
        // tiene una caja abierta en la sucursal (la PC se reinicio a mitad de turno, o se abrio desde otra PC) se usa esa
        // y no se crea otra. La PC sube la apertura ANTES que las ventas para que caigan dentro de esa caja por fecha.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SincronizarAperturaCajaOffline([FromBody] SincronizarAperturaCajaOfflineRequest request)
        {
            try
            {
                var user = _usuarioActual;
                if (user == null || !PosOfflineSettings.HabilitadoPara(user.IdSucursal))
                    return PosOfflineDeshabilitado();
                if (request == null || request.ClientId == Guid.Empty)
                    return Json(new { ok = false, rechazada = true, msg = "La apertura llegó sin código." });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                if (user.EsUsuarioProduccion && operador != null && operador.Id == user.Id)
                    return Json(new { ok = false, requiereOperador = true, msg = "Ingresá con tu usuario para subir la apertura de caja." });
                if (operador == null || operador.Id != request.IdVendedor)
                    return Json(new { ok = false, otroVendedor = true, msg = "La apertura es de otro vendedor." });
                if (request.IdSucursalPOS != user.IdSucursal)
                    return Json(new { ok = false, otraSucursal = true, msg = "La apertura es de otra sucursal." });
                if (request.CajaInicio <= 0)
                    return Json(new { ok = false, rechazada = true, msg = "El monto inicial de la caja tiene que ser mayor a 0." });
                if (request.FechaLocal == default(DateTime)
                    || request.FechaLocal > DateTime.Now.AddMinutes(PosOfflineSettings.MinutosToleranciaFuturo)
                    || request.FechaLocal < DateTime.Now.AddDays(-PosOfflineSettings.DiasMaximosAntiguedad))
                    return Json(new { ok = false, rechazada = true, msg = "La hora de la apertura no es válida: revisá la hora de la PC." });

                var claveRepo = WebCore.Infrastructure.NegocioFactory.CrearPosClaveUnicaRepository(_empresa);
                var existente = claveRepo.ObtenerPorClientId(request.ClientId);
                if (existente != null)
                {
                    if (!string.Equals(existente.Tipo, Entidades.PosClaveUnica.TipoCaja, StringComparison.Ordinal))
                        return Json(new { ok = false, rechazada = true, msg = "El código de la apertura ya se usó para otra operación." });
                    return Json(new { ok = true, idCierre = existente.IdRegistro, yaRegistrada = true });
                }

                string detalle = null;
                var cajaAbierta = ObtenerCierreCajaActual(operador);
                int idCierre;
                bool reutilizada = cajaAbierta != null;
                if (reutilizada)
                {
                    // Ya hay caja abierta del vendedor: no se crea otra (una sola caja abierta por vendedor y sucursal).
                    idCierre = cajaAbierta!.Id;
                    detalle = "Apertura sin conexión de las " + request.FechaLocal.ToString("dd/MM/yyyy HH:mm") +
                              " no creada: el vendedor ya tenía la caja " + idCierre + " abierta.";
                }
                else
                {
                    var sucursal = _oSucursalN.findById(user.IdSucursal);
                    DateTime fechaApertura = request.FechaLocal;

                    // No se puede abrir antes de que cerrara la ultima caja del vendedor: se corre al segundo siguiente
                    // (las ventas anteriores a esa hora quedan en la caja cerrada, como "llego tarde").
                    var ultima = _oCierreN.findByIdOrLast(
                        new Entidades.CierreCaja { Sucursal = sucursal, UsuarioInicio = operador },
                        Entidades.CierreCaja.tipoBusqueda.FindLast, "");
                    if (ultima?.FechaHoraCierre != null && ultima.FechaHoraCierre.Value >= fechaApertura)
                    {
                        fechaApertura = ultima.FechaHoraCierre.Value.AddSeconds(1);
                        detalle = "Hora de apertura corrida de " + request.FechaLocal.ToString("dd/MM/yyyy HH:mm:ss") +
                                  " a " + fechaApertura.ToString("dd/MM/yyyy HH:mm:ss") + " (cierre anterior posterior).";
                    }

                    var nuevoCierre = new Entidades.CierreCaja
                    {
                        Sucursal = sucursal,
                        UsuarioInicio = operador,
                        FechaHoraInicio = fechaApertura,
                        CajaInicio = (float)request.CajaInicio
                    };
                    _oCierreN.addOrEditCierreCaja(nuevoCierre);

                    // Postgres deja el id en la entidad; SQL Server no (mismo criterio que CajasController.AbrirCaja).
                    idCierre = nuevoCierre.Id;
                    if (idCierre <= 0)
                        idCierre = _oCierreN.findByIdOrLast(nuevoCierre, Entidades.CierreCaja.tipoBusqueda.FindLast, "")?.Id ?? 0;
                    if (idCierre <= 0)
                        throw new InvalidOperationException("La caja se creó pero no se pudo leer su número.");
                }

                // La clave va despues de crear la caja (addOrEditCierreCaja no participa de una transaccion compartida).
                // Riesgo aceptado: dos subidas SIMULTANEAS de la misma apertura; la PC las sube de a una, en orden.
                try
                {
                    claveRepo.Insertar(new Entidades.PosClaveUnica
                    {
                        ClientId = request.ClientId,
                        Tipo = Entidades.PosClaveUnica.TipoCaja,
                        IdRegistro = idCierre,
                        Origen = Entidades.PosClaveUnica.OrigenOffline,
                        IdSucursal = user.IdSucursal,
                        IdUsuario = operador.Id,
                        IdUsuarioSync = user.Id,
                        FechaLocal = request.FechaLocal,
                        CausaCorte = Negocio.VentaOffline.NormalizarCausaCorte(request.CausaCorte),
                        Detalle = detalle
                    });
                }
                catch (Contratos.ClaveDuplicadaException)
                {
                    var ganadora = claveRepo.ObtenerPorClientId(request.ClientId);
                    return Json(new { ok = true, idCierre = ganadora?.IdRegistro ?? idCierre, yaRegistrada = true });
                }

                return Json(new { ok = true, idCierre, reutilizada, detalle });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SincronizarAperturaCajaOffline " + request?.ClientId + ": " + ex);
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error al subir la apertura de caja", error = ex.Message });
            }
        }

        // true si esta sesion se abrio desde un dispositivo seguro que sigue autorizado (no bloqueado despues del login).
        private bool SesionDesdeDispositivoAutorizado()
        {
            string? claim = User?.FindFirst(WebCore.Infrastructure.SesionClaims.IdDispositivoSeguro)?.Value;
            if (!int.TryParse(claim, out int idDispositivo) || idDispositivo <= 0)
                return false;

            var dispositivo = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(_empresa)
                .Listar(_usuarioActual.IdEmpresa)
                ?.FirstOrDefault(d => d != null && d.Id == idDispositivo);
            return dispositivo != null && !dispositivo.Bloqueado;
        }

        // Huellas de PIN para entrar al POS sin conexion (decision del usuario 2026-11-26: todos los PIN de la sucursal,
        // incluidos los admins, cuyo PIN solo sirve sin conexion). Solo usuarios activos, con PIN, con algun permiso de
        // POS (admin siempre) y de esta sucursal (los admin, de cualquiera). Nunca viaja el PIN: solo el hash PBKDF2-SHA256
        // del servidor (Utilidades.PasswordSecurity) con su sal e iteraciones, que la PC verifica con WebCrypto.
        // Limite conocido: una lectura por usuario (getUsuarioById trae el hash y los permisos); para pocos usuarios.
        private List<object> UsuariosConPinParaOffline(int idSucursal)
        {
            var resultado = new List<object>();
            foreach (var basico in _oUsuarioN.ListarUsuariosParaLogin(_usuarioActual.IdEmpresa) ?? new List<Entidades.Usuario>())
            {
                var u = _oUsuarioN.getUsuarioById(basico.Id);
                if (u == null || !u.Activo || !u.TienePin) continue;
                if (!u.Admin && u.IdSucursal != idSucursal) continue;
                if (!TieneAlgunPermisoPOS(u)) continue;

                resultado.Add(new
                {
                    id = u.Id,
                    nombre = u.Nombre ?? "",
                    usuario = u.User ?? "",
                    admin = u.Admin,
                    pinHash = u.PinHash,
                    pinSalt = u.PinSalt,
                    pinIteraciones = u.PinHashIterations > 0 ? u.PinHashIterations : Utilidades.PasswordSecurity.DefaultIterations
                });
            }
            return resultado;
        }

        private static object RespuestaVentaOffline(Entidades.PosClaveUnica clave, bool yaRegistrada)
        {
            return new
            {
                ok = true,
                ventaId = clave.IdRegistro,
                yaRegistrada,
                llegoTarde = clave.LlegoTarde,
                sinCaja = clave.SinCaja,
                pendienteFacturar = clave.PendienteFacturar,
                difPrecio = clave.DifPrecio
            };
        }

        private static string? Recortar(string? texto, int max)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            return texto.Length <= max ? texto : texto.Substring(0, max);
        }
    }
}
