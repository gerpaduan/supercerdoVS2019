// POS sin conexion (2026-11-26): endpoints que usa el navegador para preparar la PC (catalogo y usuarios) y para subir lo
// que se hizo mientras no habia red (ventas y aperturas de caja). Ver docs/DECISIONS.md "POS sin conexion" y
// docs/03-modulos/ventas-y-pos.md.
//
// Puntos que conviene no perder de vista:
//  - Viene habilitado en todas las sucursales. Con el freno de emergencia (PosOffline:Habilitado=false) o sin la tabla
//    posclaveunica todo responde deshabilitado=true y el sistema sigue como antes.
//  - Cualquier usuario con sesion prepara la PC (no hace falta dispositivo autorizado): recibe las huellas de clave y PIN
//    de los usuarios de SU EMPRESA (todas las sucursales). La de un administrador solo viaja a las sesiones de ESE
//    administrador.
//  - Idempotencia real: cada venta/apertura trae un clientId de la PC; posclaveunica tiene indice unico y el INSERT va
//    en la misma transaccion que la venta (Negocio.Venta.agregarVentaConClave). Reintentar nunca duplica.
//  - La puede subir cualquier usuario de la sucursal. El vendedor es el que vendio; quien sube queda registrado.
//  - Nunca se pierde una venta cobrada: si el vendedor ya no esta habilitado se carga igual, marcada y con aviso al
//    admin; lo que no se puede grabar vuelve con rechazada=true y queda en la PC.
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
        // Largo maximo del texto libre que se guarda en posclaveunica.detalle.
        private const int MaxDetalleOffline = 4000;

        private JsonResult PosOfflineDeshabilitado() => Json(new { ok = false, deshabilitado = true });

        private bool PosOfflineRige(Entidades.Usuario? user) =>
            user != null && user.IdSucursal > 0 && PosOfflineDisponibilidad.Disponible(_empresa);

        // Estado para el navegador: si rige el modo offline, la caja del operador, la configuracion y un token antiforgery
        // fresco (la pagina offline es estatica y no trae el del render). Se llama antes de subir la cola.
        [HttpGet]
        public IActionResult EstadoPosOffline(string? posInstanceId)
        {
            var user = _usuarioActual;
            if (!PosOfflineRige(user))
                return PosOfflineDeshabilitado();

            var operador = ResolverOperadorPOS(posInstanceId, user);
            // La cuenta compartida de produccion sin operador autorizado no es un vendedor: no se informa caja.
            bool sinOperador = operador == null || (user.EsUsuarioProduccion && operador.Id == user.Id);
            var caja = sinOperador ? null : ObtenerCierreCajaActual(operador);
            var tokens = HttpContext.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(HttpContext);

            return Json(new
            {
                ok = true,
                deshabilitado = false,
                token = tokens.RequestToken,
                operador = sinOperador ? null : new { id = operador!.Id, nombre = operador.Nombre ?? "" },
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

        // Datos para vender sin conexion: productos de la empresa (misma lista que ProductosController.ListarProductos y
        // misma busqueda por codigo que BuscarProducto), formatos de etiqueta de balanza activos, codigo base generico,
        // factores por forma de pago y los usuarios que pueden entrar sin conexion. Con 'version' igual a la vigente
        // responde sinCambios (no reenvia todo).
        // Limite conocido: el servidor arma todo en cada pedido (para calcular la version) y lee un usuario por vez; con
        // pocos miles de productos, decenas de usuarios y un pedido cada MinutosRefrescoCatalogo por PC es aceptable.
        [HttpGet]
        public IActionResult CatalogoOffline(string? version)
        {
            var user = _usuarioActual;
            if (!PosOfflineRige(user))
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

                var (usuarios, adminsVigentes) = UsuariosParaOffline(user);
                var contenido = new
                {
                    idEmpresa,
                    codProdGenerico = _param.GetLong(Entidades.ParamKeys.CodProdGenerico, 0L),
                    productos,
                    formatos,
                    factores = ObtenerConfiguracionFormaPagoPOS(),
                    usuarios,
                    // Para que la PC descarte un admin guardado antes que ya no lo es o que cambio su clave.
                    adminsVigentes,
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
                    idConsumidorFinal = _param.GetInt(Entidades.ParamKeys.IdConsumidorFinal, 0),
                    diasVigencia = PosOfflineSettings.DiasVigencia,
                    horasAvisoPrecios = PosOfflineSettings.HorasAvisoPrecios
                };

                // Version = huella del contenido: igual contenido, igual version (la PC no vuelve a bajar lo mismo).
                string json = System.Text.Json.JsonSerializer.Serialize(contenido);
                string versionActual = HuellaCorta(json);
                if (string.Equals(version, versionActual, StringComparison.OrdinalIgnoreCase))
                    return Json(new { ok = true, sinCambios = true, version = versionActual });

                return Json(new
                {
                    ok = true,
                    sinCambios = false,
                    version = versionActual,
                    generado = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    contenido.idEmpresa,
                    contenido.codProdGenerico,
                    contenido.productos,
                    contenido.formatos,
                    contenido.factores,
                    contenido.usuarios,
                    contenido.adminsVigentes,
                    contenido.ticket,
                    contenido.sucursal,
                    contenido.idConsumidorFinal,
                    contenido.diasVigencia,
                    contenido.horasAvisoPrecios
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
        //  ok=true (ventaId, yaRegistrada, llegoTarde, sinCaja, pendienteFacturar, difPrecio, vendedorNoHabilitado).
        //  ok=false, rechazada=true (msg): no se puede grabar tal como esta; la PC la deja apartada para revisar.
        //  ok=false, otraSucursal: la sube alguien de esa sucursal; la PC la saltea.
        //  ok=false (sin rechazada) o error HTTP: problema transitorio; la PC reintenta.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SincronizarVentaOffline([FromBody] SincronizarVentaOfflineRequest request)
        {
            try
            {
                var user = _usuarioActual;
                if (!PosOfflineRige(user))
                    return PosOfflineDeshabilitado();
                if (request == null || request.ClientId == Guid.Empty)
                    return Json(new { ok = false, rechazada = true, msg = "La venta llegó sin código." });

                // Sucursal: la de la sesion que sube, como en FinalizarVenta.
                if (request.IdSucursalPOS != user.IdSucursal)
                    return Json(new { ok = false, otraSucursal = true, msg = "La venta es de otra sucursal: la sube un usuario trabajando en esa sucursal." });

                // Reintento de una venta ya grabada: se responde sin revalidar (la primera vez ya paso todo).
                var claveRepo = WebCore.Infrastructure.NegocioFactory.CrearPosClaveUnicaRepository(_empresa);
                var existente = claveRepo.ObtenerPorClientId(request.ClientId);
                if (existente != null)
                {
                    if (!string.Equals(existente.Tipo, Entidades.PosClaveUnica.TipoVenta, StringComparison.Ordinal))
                        return Json(new { ok = false, rechazada = true, msg = "El código de la venta ya se usó para otra operación." });
                    return Json(RespuestaVentaOffline(existente, yaRegistrada: true));
                }

                var vendedor = ResolverVendedorOffline(request.IdVendedor, user);
                if (vendedor == null)
                    return Json(new { ok = false, rechazada = true, msg = "El vendedor de la venta no existe en esta empresa." });

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
                var cajaAbierta = ObtenerCierreCajaActual(vendedor);
                var cajaCerrada = _oCierreN.BuscarCajaCerradaQueContiene(vendedor.Id, user.IdSucursal, request.FechaLocal);
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

                // Vendedor dado de baja, bloqueado o sin permiso: la venta entra igual (ya se cobro), marcada y con aviso.
                string? motivoVendedor = Negocio.VentaOffline.MotivoRevisarVendedor(
                    vendedor.Activo, vendedor.Bloqueado, TieneAlgunPermisoPOS(vendedor));

                // Venta: mismos campos que FinalizarVenta para una venta a Consumidor Final (ticket X, sin cta cte).
                string formaPago = Negocio.VentaOffline.FormasPagoPermitidas
                    .First(f => string.Equals(f, request.FormaPago, StringComparison.OrdinalIgnoreCase));
                var venta = new Entidades.Venta
                {
                    Persona = persona,
                    Sucursal = sucursal,
                    Vendedor = vendedor,
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

                var detalles = new List<string>();
                if (diferencias.Length > 0) detalles.Add("Precios: " + diferencias);
                if (motivoVendedor != null)
                    detalles.Add("Revisar: " + motivoVendedor + " (vendió " + vendedor.Nombre + ", la subió " + user.Nombre + ")");

                var clave = new Entidades.PosClaveUnica
                {
                    ClientId = request.ClientId,
                    Origen = Entidades.PosClaveUnica.OrigenOffline,
                    IdSucursal = user.IdSucursal,
                    IdUsuario = vendedor.Id,
                    IdUsuarioSync = user.Id,
                    FechaLocal = request.FechaLocal,
                    LlegoTarde = estadoCaja == Negocio.VentaOffline.EstadoCaja.LlegoTarde,
                    SinCaja = estadoCaja == Negocio.VentaOffline.EstadoCaja.SinCaja,
                    PendienteFacturar = pendienteFacturar,
                    DifPrecio = diferencias.Length > 0,
                    CausaCorte = Negocio.VentaOffline.NormalizarCausaCorte(request.CausaCorte),
                    VersionCatalogo = Recortar(request.VersionCatalogo, 40),
                    Detalle = detalles.Count > 0 ? Recortar(string.Join(" · ", detalles), MaxDetalleOffline) : null
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
                if (motivoVendedor != null)
                {
                    AvisarVendedorNoHabilitado(Entidades.Notificacion.TipoVentaSinConexionVendedor, user.IdSucursal, resultado.IdVenta,
                        "Venta sin conexión para revisar",
                        "Se cargó la venta #" + resultado.IdVenta + " hecha sin conexión el " + request.FechaLocal.ToString("dd/MM/yyyy HH:mm") +
                        " por " + vendedor.Nombre + ": " + motivoVendedor + ". La subió " + user.Nombre + ".");
                }
                return Json(RespuestaVentaOffline(clave, yaRegistrada: false, vendedorNoHabilitado: motivoVendedor != null));
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
                if (!PosOfflineRige(user))
                    return PosOfflineDeshabilitado();
                if (request == null || request.ClientId == Guid.Empty)
                    return Json(new { ok = false, rechazada = true, msg = "La apertura llegó sin código." });
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

                var vendedor = ResolverVendedorOffline(request.IdVendedor, user);
                if (vendedor == null)
                    return Json(new { ok = false, rechazada = true, msg = "El usuario de la apertura no existe en esta empresa." });
                string? motivoVendedor = Negocio.VentaOffline.MotivoRevisarVendedor(
                    vendedor.Activo, vendedor.Bloqueado, TieneAlgunPermisoPOS(vendedor));

                string? detalle = null;
                var cajaAbierta = ObtenerCierreCajaActual(vendedor);
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
                        new Entidades.CierreCaja { Sucursal = sucursal, UsuarioInicio = vendedor },
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
                        UsuarioInicio = vendedor,
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
                // Riesgo aceptado: dos subidas SIMULTANEAS de la misma apertura; la PC las sube de a una (navigator.locks).
                try
                {
                    claveRepo.Insertar(new Entidades.PosClaveUnica
                    {
                        ClientId = request.ClientId,
                        Tipo = Entidades.PosClaveUnica.TipoCaja,
                        IdRegistro = idCierre,
                        Origen = Entidades.PosClaveUnica.OrigenOffline,
                        IdSucursal = user.IdSucursal,
                        IdUsuario = vendedor.Id,
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

                if (motivoVendedor != null && !reutilizada)
                {
                    AvisarVendedorNoHabilitado(Entidades.Notificacion.TipoCajaSinConexionVendedor, user.IdSucursal, idCierre,
                        "Caja abierta sin conexión para revisar",
                        "Se creó la caja #" + idCierre + " abierta sin conexión el " + request.FechaLocal.ToString("dd/MM/yyyy HH:mm") +
                        " por " + vendedor.Nombre + ": " + motivoVendedor + ". La subió " + user.Nombre + ". Hay que cerrarla.");
                }

                return Json(new { ok = true, idCierre, reutilizada, detalle, vendedorNoHabilitado = motivoVendedor != null });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SincronizarAperturaCajaOffline " + request?.ClientId + ": " + ex);
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "Error al subir la apertura de caja", error = ex.Message });
            }
        }

        // Quien vendio (con sus permisos), esté activo o no; null si no es un usuario de la empresa de la sesion o es la
        // cuenta compartida de produccion (que no vende por si sola).
        private Entidades.Usuario? ResolverVendedorOffline(int idVendedor, Entidades.Usuario user)
        {
            if (idVendedor <= 0) return null;
            var vendedor = _oUsuarioN.getUsuarioConPermisos(idVendedor);
            if (vendedor == null || vendedor.IdEmpresa != user.IdEmpresa || vendedor.EsUsuarioProduccion) return null;
            return vendedor;
        }

        // Campana del admin (solo bases con notificaciones, Postgres). La venta o la caja ya quedaron guardadas: si el
        // aviso falla se registra en el log y se sigue (el dato esta igual en posclaveunica.detalle).
        private void AvisarVendedorNoHabilitado(string tipo, int idSucursal, int idRegistro, string titulo, string mensaje)
        {
            if (!PosBorradorSettings.SoportaNotificaciones) return;
            try
            {
                CrearVentaBorradorN().RegistrarNotificacion(new Entidades.Notificacion
                {
                    IdSucursal = idSucursal,
                    Tipo = tipo,
                    Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    RefId = idRegistro
                }, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("No se pudo avisar al admin (" + tipo + " " + idRegistro + "): " + ex.Message);
            }
        }

        // Usuarios que pueden entrar sin conexion desde una PC preparada por esta sesion (decision del usuario 2026-11-27):
        //  - los no administradores activos de la empresa (cualquier sucursal) con algun permiso de POS;
        //  - un administrador SOLO si es quien inicio sesion (su clave no viaja a las PC donde no entro).
        // Nunca la cuenta compartida de produccion. Nunca viaja la clave ni el PIN: solo su huella PBKDF2-SHA256 del
        // servidor (Utilidades.PasswordSecurity) con sal e iteraciones, que la PC verifica con WebCrypto. Un usuario que
        // solo tiene la clave vieja sin huella queda afuera hasta que inicie sesion con conexion.
        private (List<object> usuarios, List<object> adminsVigentes) UsuariosParaOffline(Entidades.Usuario sesion)
        {
            var usuarios = new List<object>();
            var adminsVigentes = new List<object>();
            foreach (var basico in _oUsuarioN.ListarUsuariosParaLogin(sesion.IdEmpresa) ?? new List<Entidades.Usuario>())
            {
                // getUsuarioConPermisos: getUsuarioById no trae permisos y dejaria afuera a todos los no administradores.
                var u = _oUsuarioN.getUsuarioConPermisos(basico.Id);
                if (u == null || !u.Activo || u.Bloqueado || u.EsUsuarioProduccion) continue;

                string version = HuellaCorta(string.Join("|", u.PasswordHash, u.PasswordSalt, u.PinHash, u.PinSalt));
                if (u.Admin)
                {
                    adminsVigentes.Add(new { id = u.Id, version });
                    if (u.Id != sesion.Id) continue;
                }

                if (!TieneAlgunPermisoPOS(u)) continue;
                bool tieneClave = !string.IsNullOrWhiteSpace(u.PasswordHash) && !string.IsNullOrWhiteSpace(u.PasswordSalt);
                if (!tieneClave && !u.TienePin) continue;

                usuarios.Add(new
                {
                    id = u.Id,
                    nombre = u.Nombre ?? "",
                    usuario = u.User ?? "",
                    admin = u.Admin,
                    version,
                    claveHash = tieneClave ? u.PasswordHash : null,
                    claveSalt = tieneClave ? u.PasswordSalt : null,
                    claveIteraciones = u.PasswordHashIterations > 0 ? u.PasswordHashIterations : Utilidades.PasswordSecurity.DefaultIterations,
                    pinHash = u.TienePin ? u.PinHash : null,
                    pinSalt = u.TienePin ? u.PinSalt : null,
                    pinIteraciones = u.PinHashIterations > 0 ? u.PinHashIterations : Utilidades.PasswordSecurity.DefaultIterations
                });
            }
            return (usuarios, adminsVigentes);
        }

        // 16 caracteres hexadecimales del SHA-256: para comparar si algo cambio.
        private static string HuellaCorta(string texto) => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(texto))).Substring(0, 16);

        private static object RespuestaVentaOffline(Entidades.PosClaveUnica clave, bool yaRegistrada, bool vendedorNoHabilitado = false)
        {
            return new
            {
                ok = true,
                ventaId = clave.IdRegistro,
                yaRegistrada,
                llegoTarde = clave.LlegoTarde,
                sinCaja = clave.SinCaja,
                pendienteFacturar = clave.PendienteFacturar,
                difPrecio = clave.DifPrecio,
                vendedorNoHabilitado
            };
        }

        private static string? Recortar(string? texto, int max)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            return texto.Length <= max ? texto : texto.Substring(0, max);
        }
    }
}
