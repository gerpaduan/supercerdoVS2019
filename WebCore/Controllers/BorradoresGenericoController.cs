// Borradores en servidor de Compras, Stock, Movimientos y Embutidos (alta/edicion) -- endpoints que
// llama el JS de cada pantalla (borrador-generico.js). Standalone (no partial de ComprasController/
// StockController/MovimientosController/ElaboradosController: son clases independientes sin contexto
// de instancia compartido que ganar), analogo a NotificacionesController. Ver docs/DECISIONS.md
// "Borradores de Compras/Stock/Movimientos/Embutidos" y VentasController.Borradores.cs (mismo
// mecanismo, generalizado por "modulo").
//
// Puntos que conviene no perder de vista (identicos a VentasController.Borradores.cs):
//  - Solo Postgres: con DataEngine=SqlServer (o BorradorGenerico:Habilitado=false) todo responde
//    deshabilitado=true y cada pantalla sigue con su borrador local (localStorage) y CapturaRespaldo.
//  - El borrador NUNCA toca las tablas reales (compra/movimiento/embutido): vive en su propia tabla.
//  - Los errores no rompen el formulario: el navegador ignora silenciosamente lo que falle aca.
//  - El operador que arma el formulario ya viene resuelto por el JS de cada modulo (mismo dato que hoy
//    viaja en el POST de Guardar de cada uno, ej. idUsuarioCreador): este controller solo confirma que
//    exista, este activo y sea de la empresa actual antes de confiar en el.
using Microsoft.AspNetCore.Mvc;
using WebCore.Models.DTO;

namespace WebCore.Controllers
{
    public class BorradoresGenericoController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly Negocio.Usuario _oUsuarioN;

        public BorradoresGenericoController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_sesion.Empresa, _sesion.Parametros);
        }

        private JsonResult BorradorDeshabilitado() => Json(new { ok = false, deshabilitado = true });

        private Negocio.BorradorGenerico CrearNegocio() => WebCore.Infrastructure.NegocioFactory.CrearBorradorGenerico(_sesion.Empresa);

        // Ver comentario identico en VentasController.ObtenerSessionIdEstable -- ASP.NET Core Session no
        // manda el Set-Cookie hasta el primer write, asi que sin esto el rate-limit por sesion nunca acumula.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // Resuelve y valida el operador que mando el navegador (ya elegido por el JS de cada modulo, con
        // su propio mecanismo -- ResolverOperadorModulo/ResolverUsuarioCreador -- que este controller no
        // reimplementa). Null si no existe, no esta activo o es de otra empresa.
        private Entidades.Usuario ResolverOperador(int idOperador)
        {
            if (idOperador <= 0) return null;
            var operador = _oUsuarioN.getUsuarioById(idOperador);
            if (operador == null || !operador.Activo) return null;
            if (_sesion.UsuarioActual.IdEmpresa > 0 && operador.IdEmpresa != _sesion.UsuarioActual.IdEmpresa) return null;
            return operador;
        }

        private static bool ModuloValido(string modulo)
        {
            foreach (string valido in Entidades.BorradorGenerico.ModulosValidos)
            {
                if (string.Equals(modulo, valido, StringComparison.Ordinal)) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------------------------
        // Guardado del formulario y latido
        // ------------------------------------------------------------------------------------

        // Guarda (o solo late) el borrador. Lo llama cada pantalla en cada cambio del formulario (con
        // debounce) y cada BorradorGenericoSettings.LatidoSegundos mientras haya lineas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarBorrador([FromBody] GuardarBorradorGenericoRequest request)
        {
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || !ModuloValido(request.Modulo) || request.ClientId == Guid.Empty)
                    return Json(new { ok = false, msg = "Falta el identificador del formulario." });
                if (request.IdSucursal <= 0)
                    return Json(new { ok = false, msg = "Falta la sucursal." });

                var operador = ResolverOperador(request.IdOperador);
                if (operador == null) return Json(new { ok = false, msg = "Operador inválido." });

                var negocio = CrearNegocio();

                if (request.SoloLatido)
                {
                    // Si no hay fila (todavia no se guardo, o ya se cerro) el navegador reenvia el borrador entero.
                    bool late = negocio.RegistrarLatido(request.ClientId, operador.Id, request.IdSucursal, request.Modulo);
                    return Json(new { ok = true, necesitaGuardar = !late });
                }

                int maxBytes = WebCore.Helpers.BorradorGenericoSettings.MaxPayloadKb * 1024;
                string crudo = request.Payload.ValueKind == System.Text.Json.JsonValueKind.Undefined ? "" : request.Payload.GetRawText();
                if (crudo.Length == 0)
                    return Json(new { ok = false, msg = "Falta el formulario." });
                if (crudo.Length > maxBytes)
                    return Json(new { ok = false, motivo = "payload_grande" });
                if (request.CantLineas > WebCore.Helpers.BorradorGenericoSettings.MaxLineas)
                    return Json(new { ok = false, motivo = "muchas_lineas" });

                var borrador = new Entidades.BorradorGenerico
                {
                    ClientId = request.ClientId,
                    IdSucursal = request.IdSucursal,
                    Modulo = request.Modulo,
                    IdRegistro = request.IdRegistro is int idRegistro && idRegistro > 0 ? idRegistro : (int?)null,
                    IdOperador = operador.Id,
                    IdUsuarioSesion = _sesion.UsuarioActual.Id,
                    Resumen = string.IsNullOrWhiteSpace(request.Resumen) ? null : request.Resumen.Trim(),
                    CantLineas = request.CantLineas,
                    Payload = crudo
                };

                var resultado = negocio.Guardar(borrador);
                switch (resultado)
                {
                    case Entidades.ResultadoGuardarBorradorGenerico.Guardado:
                        return Json(new { ok = true });
                    case Entidades.ResultadoGuardarBorradorGenerico.YaCerrado:
                        return Json(new { ok = true, cerrado = true });
                    default:
                        return Json(new { ok = false, ajeno = true });
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo resguardar el borrador.", error = ex.Message });
            }
        }

        // El navegador avisa (pagehide, via fetch keepalive) que se cierra la pestana con el formulario sin guardar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EventoBorrador([FromBody] EventoBorradorGenericoRequest request)
        {
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || !ModuloValido(request.Modulo) || request.ClientId == Guid.Empty) return Json(new { ok = false });
                if (request.IdSucursal <= 0) return Json(new { ok = false });

                var operador = ResolverOperador(request.IdOperador);
                if (operador == null) return Json(new { ok = false });

                bool registrado = CrearNegocio().RegistrarCierrePestana(request.ClientId, operador.Id, request.IdSucursal, request.Modulo, operador.Id);
                return Json(new { ok = registrado });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // El registro real ya se guardo con exito (Compras/Movimientos/Embutidos, flujo AJAX): marca el
        // borrador FINALIZADA y vacia su payload, para que deje de listarse y no quede ACTIVA para
        // siempre. Bug real corregido 2026-09-23: el JS llamaba a GuardarBorrador con un payload vacio
        // en vez de a este endpoint -- el borrador nunca pasaba a FINALIZADA (quedaba ACTIVA con
        // cantlineas=0, invisible en el listado pero sin limpiarse nunca, ver docs/DECISIONS.md).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarFinalizado([FromBody] MarcarFinalizadoBorradorGenericoRequest request)
        {
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || !ModuloValido(request.Modulo) || request.ClientId == Guid.Empty) return Json(new { ok = false });

                bool marcado = CrearNegocio().MarcarFinalizada(request.ClientId, request.Modulo, request.IdResultado);
                return Json(new { ok = marcado });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ------------------------------------------------------------------------------------
        // Listado, recuperacion y descarte ("Ver borradores sin cerrar")
        // ------------------------------------------------------------------------------------

        // Borradores sin cerrar de toda la sucursal para un modulo, con operador y resumen. No incluye
        // el borrador de esta misma pestana. Si el borrador local ya se habia convertido en el registro
        // real, informa su id (idempotencia del Guardar del modulo destino).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ListarBorradores([FromBody] ListarBorradoresGenericoRequest request)
        {
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || !ModuloValido(request.Modulo) || request.IdSucursal <= 0)
                    return Json(new { ok = true, pendientes = Array.Empty<object>(), interrumpidas = 0 });

                var operador = ResolverOperador(request.IdOperador);
                if (operador == null) return Json(new { ok = false, msg = "Operador inválido." });

                var negocio = CrearNegocio();
                int minutos = WebCore.Helpers.BorradorGenericoSettings.MinutosSinLatidoInterrumpida;

                int? resultadoLocalFinalizado = request.ClientIdLocal != null && request.ClientIdLocal != Guid.Empty
                    ? negocio.ObtenerIdResultadoSiYaFinalizada(request.ClientIdLocal.Value, request.Modulo)
                    : null;

                var lista = negocio.ListarSucursal(request.IdSucursal, request.Modulo, WebCore.Helpers.BorradorGenericoSettings.DiasRetencionFinalizadas);

                var pendientes = new List<object>();
                int interrumpidas = 0;
                foreach (var borrador in lista)
                {
                    // El formulario de esta misma pestana no se ofrece a si mismo; los vacios tampoco.
                    if (request.ClientIdLocal != null && borrador.ClientId == request.ClientIdLocal.Value) continue;
                    if (borrador.CantLineas <= 0) continue;

                    bool enUso = Negocio.BorradorGenerico.EstaEnUso(borrador, minutos);
                    if (!enUso) interrumpidas++;

                    pendientes.Add(new
                    {
                        id = borrador.Id,
                        idRegistro = borrador.IdRegistro,
                        operador = string.IsNullOrWhiteSpace(borrador.NombreOperador) ? ("Usuario #" + borrador.IdOperador) : borrador.NombreOperador,
                        esMio = borrador.IdOperador == operador.Id,
                        estado = enUso ? "En uso" : "Interrumpido",
                        puedeCargar = !enUso,
                        inicio = borrador.Creado.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("es-AR")),
                        sinLatido = TextoSinLatido(borrador.SegundosSinLatido),
                        resumen = string.IsNullOrWhiteSpace(borrador.Resumen) ? ("(" + borrador.CantLineas + " línea(s))") : borrador.Resumen,
                        cantLineas = borrador.CantLineas,
                        // Payload crudo (forma especifica de cada modulo, ver docs/DECISIONS.md):
                        // el modal lo usa solo para desplegar el detalle de lineas al pedido del
                        // usuario (icono/doble clic), interpretado del lado del cliente por el JS de
                        // cada modulo (cfg.renderizarLineas) -- el servidor no lo interpreta.
                        payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(borrador.Payload ?? "{}")
                    });
                }

                return Json(new { ok = true, pendientes, interrumpidas, resultadoLocalFinalizado });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudieron consultar los borradores sin cerrar.", error = ex.Message });
            }
        }

        private static string TextoSinLatido(int segundos)
        {
            if (segundos < 60) return "hace " + segundos + " s";
            if (segundos < 3600) return "hace " + (segundos / 60) + " min";
            if (segundos < 86400) return "hace " + (segundos / 3600) + " h";
            return "hace " + (segundos / 86400) + " d";
        }

        // Carga al formulario un borrador sin cerrar: lo pasa al operador actual y devuelve el payload
        // para que la pantalla lo aplique. Si es de otro usuario pide autorizacion de supervisor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecuperarBorrador([FromBody] AccionBorradorGenericoRequest request)
        {
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || !ModuloValido(request.Modulo) || request.Id <= 0)
                    return Json(new { ok = false, msg = "Falta el borrador a recuperar." });

                var operador = ResolverOperador(request.IdOperador);
                if (operador == null) return Json(new { ok = false, msg = "Operador inválido." });

                var negocio = CrearNegocio();
                var existente = negocio.ObtenerPorId(request.Id);
                if (existente == null || existente.Modulo != request.Modulo || existente.IdSucursal != request.IdSucursal
                    || existente.Estado != Entidades.BorradorGenerico.EstadoActiva)
                    return Json(new { ok = false, msg = "Ese borrador ya no está disponible." });

                var autorizacion = WebCore.Helpers.AutorizacionSupervisorHelper.Resolver(
                    existente.IdOperador, existente.NombreOperador, "un borrador", operador, _oUsuarioN, request.Supervisor, ObtenerSessionIdEstable());
                if (!autorizacion.Autorizado)
                    return Json(new { ok = false, requiereSupervisor = autorizacion.RequiereSupervisor, bloqueado = autorizacion.Bloqueado, msg = autorizacion.Mensaje });

                var borrador = negocio.Recuperar(request.Id, request.Modulo, operador.Id, _sesion.UsuarioActual.Id, operador.Id,
                    autorizacion.NombreSupervisor, WebCore.Helpers.BorradorGenericoSettings.MinutosSinLatidoInterrumpida, out string error);
                if (borrador == null) return Json(new { ok = false, msg = error });

                return Json(new
                {
                    ok = true,
                    clientId = borrador.ClientId,
                    idRegistro = borrador.IdRegistro,
                    payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(borrador.Payload ?? "{}")
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo recuperar el borrador.", error = ex.Message });
            }
        }

        // Descarta un borrador sin cerrar (motivo obligatorio). Siempre avisa al admin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DescartarBorrador([FromBody] AccionBorradorGenericoRequest request)
        {
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return BorradorDeshabilitado();

            try
            {
                if (request == null || !ModuloValido(request.Modulo) || request.Id <= 0)
                    return Json(new { ok = false, msg = "Falta el borrador a descartar." });

                var operador = ResolverOperador(request.IdOperador);
                if (operador == null) return Json(new { ok = false, msg = "Operador inválido." });

                var negocio = CrearNegocio();
                var existente = negocio.ObtenerPorId(request.Id);
                if (existente == null || existente.Modulo != request.Modulo || existente.IdSucursal != request.IdSucursal
                    || existente.Estado != Entidades.BorradorGenerico.EstadoActiva)
                    return Json(new { ok = false, msg = "Ese borrador ya no está disponible." });

                var autorizacion = WebCore.Helpers.AutorizacionSupervisorHelper.Resolver(
                    existente.IdOperador, existente.NombreOperador, "un borrador", operador, _oUsuarioN, request.Supervisor, ObtenerSessionIdEstable());
                if (!autorizacion.Autorizado)
                    return Json(new { ok = false, requiereSupervisor = autorizacion.RequiereSupervisor, bloqueado = autorizacion.Bloqueado, msg = autorizacion.Mensaje });

                bool descartado = negocio.Descartar(request.Id, request.Modulo, request.Motivo, operador.Id, operador.Id, operador.Nombre,
                    autorizacion.NombreSupervisor, WebCore.Helpers.BorradorGenericoSettings.MinutosSinLatidoInterrumpida, out string error);
                return descartado ? Json(new { ok = true }) : Json(new { ok = false, msg = error });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo descartar el borrador.", error = ex.Message });
            }
        }
    }
}
