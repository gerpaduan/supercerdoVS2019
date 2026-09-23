// Ventas en curso del POS (borrador en servidor) y advertencia de "producto pesado sin agregar" --
// endpoints que llama el navegador del cajero. Ver docs/DECISIONS.md "Ventas en curso: borrador en
// servidor y advertencias del POS". Vive en un archivo aparte (VentasController es partial) para no
// engordar mas el controller principal.
//
// Puntos que conviene no perder de vista:
//  - Solo Postgres: con DataEngine=SqlServer (o PosBorrador:Habilitado=false) todo responde
//    deshabilitado=true y el POS sigue con el POSDraft local de siempre.
//  - El borrador NUNCA toca ventas/lineaventa: vive en su propia tabla (ver la migracion).
//  - Nada de lo que manda el navegador se toma como verdad para horas ni precios: las horas las fija la
//    base (now()) y nombre/precio del producto se re-resuelven por codigo en el servidor.
//  - Los errores no rompen la venta: el navegador ignora silenciosamente lo que falle aca.
using Microsoft.AspNetCore.Mvc;
using WebCore.Models.DTO;

namespace WebCore.Controllers
{
    public partial class VentasController
    {
        private static readonly System.Globalization.CultureInfo CulturaAr = new System.Globalization.CultureInfo("es-AR");

        // Respuesta comun cuando la funcion esta apagada (SqlServer o config).
        private JsonResult BorradorDeshabilitado() => Json(new { ok = false, deshabilitado = true });

        private Negocio.VentaBorrador CrearVentaBorradorN() => WebCore.Infrastructure.NegocioFactory.CrearVentaBorrador(_empresa);

        // Resuelve quien autoriza a operar sobre una venta sin cerrar de OTRO usuario. Delega en
        // WebCore.Helpers.AutorizacionSupervisorHelper (extraido de aca, reusado tambien por
        // BorradoresGenericoController -- ver docs/DECISIONS.md "Borradores de
        // Compras/Stock/Movimientos/Embutidos"). Devuelve true si se puede seguir; nombreSupervisor
        // queda con el nombre de quien autorizo (null si no hizo falta). Si no se puede, "respuesta"
        // trae el JSON para el navegador (pide supervisor).
        private bool ResolverAutorizacionSupervisor(Entidades.VentaBorrador borrador, Entidades.Usuario operador,
            SupervisorAutorizacionDto? supervisor, out string? nombreSupervisor, out JsonResult? respuesta)
        {
            var resultado = WebCore.Helpers.AutorizacionSupervisorHelper.Resolver(
                borrador.IdOperador, borrador.NombreOperador, "una venta", operador, _oUsuarioN, supervisor, ObtenerSessionIdEstable());

            nombreSupervisor = resultado.NombreSupervisor;
            if (resultado.Autorizado)
            {
                respuesta = null;
                return true;
            }

            respuesta = Json(new
            {
                ok = false,
                requiereSupervisor = resultado.RequiereSupervisor,
                bloqueado = resultado.Bloqueado,
                msg = resultado.Mensaje
            });
            return false;
        }

        // ------------------------------------------------------------------------------------
        // Guardado del carrito y latido
        // ------------------------------------------------------------------------------------

        // Guarda (o solo late) la venta en curso. Lo llama el POS en cada cambio del carrito (con debounce)
        // y cada PosBorradorSettings.LatidoSegundos mientras haya lineas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarBorradorPOS([FromBody] GuardarBorradorRequest request)
        {
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || request.ClientId == Guid.Empty)
                    return Json(new { ok = false, msg = "Falta el identificador del carrito." });

                var user = _usuarioActual;
                if (user.IdSucursal == 0 || request.IdSucursalPOS != user.IdSucursal)
                    return Json(new { ok = false, motivo = "sucursal" });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                var negocio = CrearVentaBorradorN();

                if (request.SoloLatido)
                {
                    // Si no hay fila (todavia no se guardo, o ya se cerro) el navegador reenvia el carrito entero.
                    bool late = negocio.RegistrarLatido(request.ClientId, operador.Id, user.IdSucursal);
                    return Json(new { ok = true, necesitaGuardar = !late });
                }

                // Limites de sanidad antes de parsear nada.
                int maxBytes = WebCore.Helpers.PosBorradorSettings.MaxPayloadKb * 1024;
                string crudo = request.Payload.ValueKind == System.Text.Json.JsonValueKind.Undefined ? "" : request.Payload.GetRawText();
                if (crudo.Length == 0)
                    return Json(new { ok = false, msg = "Falta el carrito." });
                if (crudo.Length > maxBytes)
                    return Json(new { ok = false, motivo = "payload_grande" });

                if (!WebCore.Services.BorradorPosPayload.TryResumir(request.Payload, out var resumen))
                    return Json(new { ok = false, msg = "El carrito no tiene el formato esperado." });
                if (resumen.LineasTotales > WebCore.Helpers.PosBorradorSettings.MaxLineas)
                    return Json(new { ok = false, motivo = "muchas_lineas" });

                var borrador = new Entidades.VentaBorrador
                {
                    ClientId = request.ClientId,
                    IdSucursal = user.IdSucursal,
                    IdOperador = operador.Id,
                    IdUsuarioSesion = user.Id,
                    PosInstanceId = string.IsNullOrWhiteSpace(request.PosInstanceId) ? null : request.PosInstanceId,
                    IdPersona = resumen.IdPersona,
                    RazonSocial = resumen.RazonSocial,
                    CantLineas = resumen.LineasActivas,
                    Total = resumen.Total,
                    Payload = crudo
                    // IdCierreCaja queda null: reservado, no se completa hoy (evita una consulta de caja por cada guardado).
                };

                var resultado = negocio.Guardar(borrador);
                switch (resultado)
                {
                    case Entidades.ResultadoGuardarBorrador.Guardado:
                        return Json(new { ok = true });
                    case Entidades.ResultadoGuardarBorrador.YaCerrado:
                        // Ya se finalizo o se descarto: el navegador debe abandonar ese carrito.
                        return Json(new { ok = true, cerrado = true });
                    default:
                        return Json(new { ok = false, ajeno = true });
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo resguardar la venta en curso.", error = ex.Message });
            }
        }

        // El navegador avisa (pagehide, via fetch keepalive) que se cierra la pestana con la venta sin finalizar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EventoBorradorPOS([FromBody] EventoBorradorRequest request)
        {
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || request.ClientId == Guid.Empty) return Json(new { ok = false });

                var user = _usuarioActual;
                if (user.IdSucursal == 0 || request.IdSucursalPOS != user.IdSucursal) return Json(new { ok = false });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                bool registrado = CrearVentaBorradorN().RegistrarCierrePestana(request.ClientId, operador.Id, user.IdSucursal, operador.Id);
                return Json(new { ok = registrado });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ------------------------------------------------------------------------------------
        // Producto pesado sin agregar (advertencia silenciosa para el cajero)
        // ------------------------------------------------------------------------------------

        // El POS detecto un producto con cantidad estable en pantalla que salio sin agregarse. Se registra
        // como advertencia para el admin. Nunca devuelve un error visible: el cajero no debe enterarse.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarProductoSinAgregarPOS([FromBody] ProductoSinAgregarRequest request)
        {
            if (!WebCore.Helpers.PosBorradorSettings.AdvertenciaProductoSinAgregarHabilitada) return BorradorDeshabilitado();

            try
            {
                if (request == null) return Json(new { ok = false });

                var user = _usuarioActual;
                if (user.IdSucursal == 0 || request.IdSucursalPOS != user.IdSucursal) return Json(new { ok = false });

                // Se re-resuelve el producto en el servidor (no se confia en nombre/precio del navegador).
                if (!long.TryParse((request.Codigo ?? "").Trim(), System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out long codigoProducto))
                    return Json(new { ok = false });

                int idEmpresaSesion = user.IdEmpresa;
                var corte = idEmpresaSesion > 0
                    ? _oCorteN.findCorteByCodigoEmpresa(codigoProducto, idEmpresaSesion, false)
                    : _oCorteN.findCorteByCodigo(codigoProducto, false);
                if (corte == null || (idEmpresaSesion > 0 && corte.IdEmpresa != idEmpresaSesion))
                    return Json(new { ok = false });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);

                var producto = new Entidades.ProductoSinAgregar
                {
                    IdSucursal = user.IdSucursal,
                    IdOperador = operador.Id,
                    IdUsuarioSesion = user.Id,
                    ClientId = request.ClientId,
                    Codigo = corte.codigo.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Producto = corte.CorteDesc ?? "",
                    PrecioKg = Math.Round((decimal)corte.PrecioKg, 2, MidpointRounding.AwayFromZero),
                    CantidadKg = Math.Round(request.CantidadKg, 3, MidpointRounding.AwayFromZero),
                    SegundosEnPantalla = request.SegundosEnPantalla,
                    SegundosEstables = request.SegundosEstables,
                    SegundosDesdeSalida = request.SegundosDesdeSalida,
                    Origen = (request.Origen ?? "").Trim().ToUpperInvariant(),
                    Motivo = (request.Motivo ?? "").Trim().ToUpperInvariant()
                };

                int id = CrearVentaBorradorN().RegistrarProductoSinAgregar(
                    producto, WebCore.Helpers.PosBorradorSettings.SegundosProductoSinAgregar, operador.Nombre);
                return Json(new { ok = id > 0 });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ------------------------------------------------------------------------------------
        // Listado, recuperacion y descarte ("Ver ventas sin cerrar", panel de Ayuda F1)
        // ------------------------------------------------------------------------------------

        // Ventas sin cerrar de toda la sucursal, con usuario e items. No incluye el carrito de esta misma
        // pestana. Devuelve ademas cuantas estan interrumpidas (para el badge) y, si el carrito local ya se
        // habia convertido en venta real, el id de esa venta.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ListarBorradoresPOS([FromBody] ListarBorradoresRequest request)
        {
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                var user = _usuarioActual;
                if (user.IdSucursal == 0) return Json(new { ok = true, pendientes = Array.Empty<object>(), interrumpidas = 0 });

                var operador = ResolverOperadorPOS(request?.PosInstanceId, user);
                var negocio = CrearVentaBorradorN();
                int minutos = WebCore.Helpers.PosBorradorSettings.MinutosSinLatidoInterrumpida;

                int? ventaLocalFinalizada = request?.ClientIdLocal != null && request.ClientIdLocal != Guid.Empty
                    ? negocio.ObtenerIdVentaSiYaFinalizada(request.ClientIdLocal.Value)
                    : null;

                var lista = negocio.ListarSucursal(user.IdSucursal, WebCore.Helpers.PosBorradorSettings.DiasRetencionFinalizadas);

                var pendientes = new List<object>();
                int interrumpidas = 0;
                foreach (var borrador in lista)
                {
                    // El carrito de esta misma pestana no se ofrece a si mismo; los vacios tampoco.
                    if (request?.ClientIdLocal != null && borrador.ClientId == request.ClientIdLocal.Value) continue;
                    if (borrador.CantLineas <= 0) continue;

                    bool enUso = Negocio.VentaBorrador.EstaEnUso(borrador, minutos);
                    if (!enUso) interrumpidas++;

                    pendientes.Add(new
                    {
                        id = borrador.Id,
                        operador = string.IsNullOrWhiteSpace(borrador.NombreOperador) ? ("Usuario #" + borrador.IdOperador) : borrador.NombreOperador,
                        esMia = borrador.IdOperador == operador.Id,
                        estado = enUso ? "En uso" : "Interrumpida",
                        puedeCargar = !enUso,
                        inicio = borrador.Creado.ToString("dd/MM/yyyy HH:mm:ss", CulturaAr),
                        ultimoLatido = borrador.UltimoLatido.ToString("dd/MM/yyyy HH:mm:ss", CulturaAr),
                        sinLatido = TextoSinLatido(borrador.SegundosSinLatido),
                        cliente = string.IsNullOrWhiteSpace(borrador.RazonSocial) ? "Consumidor Final" : borrador.RazonSocial,
                        total = "$ " + borrador.Total.ToString("N2", CulturaAr),
                        cantLineas = borrador.CantLineas,
                        items = WebCore.Services.BorradorPosPayload.ConstruirItems(borrador.Payload ?? "")
                    });
                }

                return Json(new
                {
                    ok = true,
                    pendientes,
                    interrumpidas,
                    ventaLocalFinalizada
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo consultar las ventas sin cerrar.", error = ex.Message });
            }
        }

        private static string TextoSinLatido(int segundos)
        {
            if (segundos < 60) return "hace " + segundos + " s";
            if (segundos < 3600) return "hace " + (segundos / 60) + " min";
            if (segundos < 86400) return "hace " + (segundos / 3600) + " h";
            return "hace " + (segundos / 86400) + " d";
        }

        // Carga al carrito una venta sin cerrar: la pasa al operador actual y devuelve el payload para que
        // el POS lo aplique. Si es de otro usuario pide autorizacion de supervisor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecuperarBorradorPOS([FromBody] AccionBorradorRequest request)
        {
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || request.Id <= 0) return Json(new { ok = false, msg = "Falta la venta a recuperar." });

                var user = _usuarioActual;
                if (user.IdSucursal == 0 || request.IdSucursalPOS != user.IdSucursal)
                    return Json(new { ok = false, msg = "La venta pertenece a otra sucursal o no hay sucursal seleccionada." });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                var negocio = CrearVentaBorradorN();

                var existente = negocio.ObtenerPorId(request.Id);
                if (existente == null || existente.IdSucursal != user.IdSucursal || existente.Estado != Entidades.VentaBorrador.EstadoActiva)
                    return Json(new { ok = false, msg = "Esa venta ya no está disponible." });

                if (!ResolverAutorizacionSupervisor(existente, operador, request.Supervisor, out string? nombreSupervisor, out JsonResult? respuesta))
                    return respuesta!;

                var borrador = negocio.Recuperar(request.Id, operador.Id, user.Id, operador.Id, nombreSupervisor,
                    WebCore.Helpers.PosBorradorSettings.MinutosSinLatidoInterrumpida, out string? error);
                if (borrador == null) return Json(new { ok = false, msg = error });

                return Json(new
                {
                    ok = true,
                    clientId = borrador.ClientId,
                    payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(borrador.Payload ?? "{}")
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo recuperar la venta.", error = ex.Message });
            }
        }

        // Descarta una venta sin cerrar (motivo obligatorio). Siempre avisa al admin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DescartarBorradorPOS([FromBody] AccionBorradorRequest request)
        {
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || request.Id <= 0) return Json(new { ok = false, msg = "Falta la venta a descartar." });

                var user = _usuarioActual;
                if (user.IdSucursal == 0 || request.IdSucursalPOS != user.IdSucursal)
                    return Json(new { ok = false, msg = "La venta pertenece a otra sucursal o no hay sucursal seleccionada." });

                var operador = ResolverOperadorPOS(request.PosInstanceId, user);
                var negocio = CrearVentaBorradorN();

                var existente = negocio.ObtenerPorId(request.Id);
                if (existente == null || existente.IdSucursal != user.IdSucursal || existente.Estado != Entidades.VentaBorrador.EstadoActiva)
                    return Json(new { ok = false, msg = "Esa venta ya no está disponible." });

                if (!ResolverAutorizacionSupervisor(existente, operador, request.Supervisor, out string? nombreSupervisor, out JsonResult? respuesta))
                    return respuesta!;

                bool descartada = negocio.Descartar(request.Id, request.Motivo, operador.Id, operador.Id, operador.Nombre, nombreSupervisor,
                    WebCore.Helpers.PosBorradorSettings.MinutosSinLatidoInterrumpida, out string? error);
                return descartada ? Json(new { ok = true }) : Json(new { ok = false, msg = error });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo descartar la venta.", error = ex.Message });
            }
        }
    }
}
