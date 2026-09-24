// Notificaciones/advertencias para el administrador (campana del topbar) y detalle de las ventas en
// curso y de los "productos pesados sin agregar". Solo Postgres. Solo admin (mismo gate que
// ActividadesController). Ver docs/DECISIONS.md "Ventas en curso: borrador en servidor y advertencias
// del POS".
//
// La deteccion de ventas interrumpidas es PEREZOSA: no hay proceso en segundo plano, se calcula cuando
// el admin consulta (Resumen). El rastro existe igual desde el primer latido y /Actividades lo muestra
// con su fecha real, aunque ningun admin haya abierto la app.
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebCore.Models;
using WebCore.Models.DTO;

namespace WebCore.Controllers
{
    public class NotificacionesController : Controller
    {
        // Cuantas notificaciones se listan en el desplegable de la campana.
        private const int MaximoEnCampana = 10;

        // Ventana de contexto de patron (ultimos N dias) en el detalle de advertencias.
        private const int DiasContexto7 = 7;
        private const int DiasContexto30 = 30;

        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly WebCore.Services.ICertificadoArcaEstado _certificadoArca;

        public NotificacionesController(WebCore.Services.IUsuarioSesionService sesion, WebCore.Services.ICertificadoArcaEstado certificadoArca)
        {
            _sesion = sesion;
            _certificadoArca = certificadoArca;
        }

        private bool EsAdmin => _sesion.UsuarioActual.Admin;

        private Negocio.VentaBorrador CrearNegocio() => WebCore.Infrastructure.NegocioFactory.CrearVentaBorrador(_sesion.Empresa);

        // Borradores de Compras/Stock/Movimientos/Embutidos (ver docs/DECISIONS.md "Borradores de
        // Compras/Stock/Movimientos/Embutidos"). Repositorio propio, pero misma tabla fisica
        // "notificaciones" que CrearNegocio(): Resumen/Atender siguen usando Negocio.VentaBorrador para
        // leer/atender (no se duplica ese CRUD en Negocio.BorradorGenerico).
        private Negocio.BorradorGenerico CrearNegocioGenerico() => WebCore.Infrastructure.NegocioFactory.CrearBorradorGenerico(_sesion.Empresa);

        // URL del detalle de una notificacion segun su tipo.
        private string DetalleUrl(Entidades.Notificacion n)
        {
            if (n.Tipo == Entidades.Notificacion.TipoProductoSinAgregar
                && Entidades.Notificacion.TryDecodificarRefIdOperadorDia(n.RefId, out int idOperador, out DateTime dia))
            {
                return Url.Action("DetalleProductoSinAgregar", "Notificaciones",
                    new { idOperador, dia = dia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }) ?? "";
            }

            if (n.Tipo == Entidades.Notificacion.TipoCertificadoArcaPorVencer)
                return Url.Action("Index", "CertificadoArca") ?? "";

            if (n.Tipo != null && n.Tipo.StartsWith(Entidades.Notificacion.PrefijoBorradorGenericoDescartado, StringComparison.Ordinal))
            {
                string modulo = n.Tipo.Substring(Entidades.Notificacion.PrefijoBorradorGenericoDescartado.Length);
                return Url.Action("DetalleBorradorGenerico", "Notificaciones", new { id = (int)n.RefId, modulo, idNotificacion = n.Id }) ?? "";
            }

            if (n.Tipo != null && n.Tipo.StartsWith(Entidades.Notificacion.PrefijoBorradorGenericoInterrumpido, StringComparison.Ordinal))
            {
                string modulo = n.Tipo.Substring(Entidades.Notificacion.PrefijoBorradorGenericoInterrumpido.Length);
                return Url.Action("DetalleBorradorGenerico", "Notificaciones", new { id = (int)n.RefId, modulo, idNotificacion = n.Id }) ?? "";
            }

            return Url.Action("DetalleBorrador", "Notificaciones", new { id = (int)n.RefId, idNotificacion = n.Id }) ?? "";
        }

        // Contador + ultimas notificaciones pendientes para la campana. Lo consulta el navegador del admin
        // cada ~60 s mientras la pestana esta visible.
        [HttpGet]
        public IActionResult Resumen()
        {
            if (!EsAdmin) return StatusCode(403, new { ok = false });
            // La campana muestra notificaciones de ventas en curso (POS) y de borradores de Compras/
            // Stock/Movimientos/Embutidos, dos features con flags independientes que escriben en la
            // misma tabla: se abre si cualquiera de las dos esta habilitada Y el motor tiene tabla de
            // notificaciones (Postgres; en SQL Server es etapa 2).
            if (!WebCore.Helpers.PosBorradorSettings.CampanaAdminHabilitada)
                return Json(new { ok = true, habilitado = false, pendientes = 0, items = Array.Empty<object>() });

            try
            {
                // Deteccion perezosa de borradores interrumpidos (Compras/Stock/Movimientos/Embutidos):
                // inserta las notificaciones nuevas ANTES de listar/contar, misma tabla fisica que usa
                // ResumenParaCampana de abajo.
                if (WebCore.Helpers.BorradorGenericoSettings.Habilitado)
                {
                    CrearNegocioGenerico().CrearNotificacionesInterrumpidas(WebCore.Helpers.BorradorGenericoSettings.MinutosSinLatidoInterrumpida);
                }

                // Aviso de certificado ARCA por vencer (evaluacion limitada a 1/hora, ver
                // CertificadoArcaEstadoService): mismo mecanismo perezoso que los borradores.
                _certificadoArca.EvaluarAviso(_certificadoArca.EmpresaActual());

                int pendientes = CrearNegocio().ResumenParaCampana(
                    WebCore.Helpers.PosBorradorSettings.MinutosSinLatidoInterrumpida, MaximoEnCampana, out var ultimas);

                var items = ultimas.Select(n => new NotificacionItemVm
                {
                    Id = n.Id,
                    Tipo = n.Tipo,
                    Severidad = n.Severidad,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    Fecha = (n.Actualizado ?? n.Creado).ToString("dd/MM/yyyy HH:mm:ss", CulturaAr),
                    DetalleUrl = DetalleUrl(n)
                }).ToList();

                return Json(new { ok = true, habilitado = true, pendientes, items });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Atender(int id)
        {
            if (!EsAdmin) return StatusCode(403, new { ok = false });
            if (!WebCore.Helpers.PosBorradorSettings.CampanaAdminHabilitada)
                return Json(new { ok = false });

            try
            {
                bool atendida = CrearNegocio().AtenderNotificacion(id, _sesion.UsuarioActual.Id);
                return Json(new { ok = atendida });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // Detalle de una venta en curso: items, cliente, total, linea de tiempo de eventos y estado final.
        [HttpGet]
        public IActionResult DetalleBorrador(int id, int idNotificacion = 0)
        {
            if (!EsAdmin) return StatusCode(403);
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado) return NotFound();

            var negocio = CrearNegocio();
            var borrador = negocio.ObtenerPorId(id);
            if (borrador == null) return NotFound();

            int minutos = WebCore.Helpers.PosBorradorSettings.MinutosSinLatidoInterrumpida;

            string estado;
            if (borrador.Estado == Entidades.VentaBorrador.EstadoFinalizada)
                estado = "Finalizada como venta #" + (borrador.IdVenta?.ToString(CultureInfo.InvariantCulture) ?? "?");
            else if (borrador.Estado == Entidades.VentaBorrador.EstadoDescartada)
                estado = "Descartada";
            else if (borrador.EstaInterrumpida(minutos))
                estado = "Interrumpida: sin señal desde el " + borrador.UltimoLatido.ToString("dd/MM/yyyy HH:mm:ss", CulturaAr) + " (posible corte de luz o de red)";
            else
                estado = "En curso (el cajero la está armando)";

            var modelo = new DetalleBorradorVm
            {
                Borrador = borrador,
                Items = WebCore.Services.BorradorPosPayload.ConstruirItems(borrador.Payload ?? ""),
                Eventos = negocio.ListarEventosPorBorrador(id),
                EstadoTexto = estado,
                Notificacion = idNotificacion > 0 ? negocio.ObtenerNotificacion(idNotificacion) : null,
                MinutosSinLatido = minutos
            };

            return PartialView("_DetalleBorradorVenta", modelo);
        }

        // Detalle de un borrador de Compras/Stock/Movimientos/Embutidos: resumen, linea de tiempo de
        // eventos y estado final. Sin desglose de items (ver DetalleBorradorGenericoVm): la vista muestra
        // el payload crudo, de solo lectura.
        [HttpGet]
        public IActionResult DetalleBorradorGenerico(int id, string modulo, int idNotificacion = 0)
        {
            if (!EsAdmin) return StatusCode(403);
            if (!WebCore.Helpers.BorradorGenericoSettings.Habilitado) return NotFound();

            var negocio = CrearNegocioGenerico();
            var borrador = negocio.ObtenerPorId(id);
            if (borrador == null || borrador.Modulo != modulo) return NotFound();

            int minutos = WebCore.Helpers.BorradorGenericoSettings.MinutosSinLatidoInterrumpida;

            string estado;
            if (borrador.Estado == Entidades.BorradorGenerico.EstadoFinalizada)
                estado = "Guardado como #" + (borrador.IdResultado?.ToString(CultureInfo.InvariantCulture) ?? "?");
            else if (borrador.Estado == Entidades.BorradorGenerico.EstadoDescartada)
                estado = "Descartado";
            else if (borrador.EstaInterrumpida(minutos))
                estado = "Interrumpido: sin señal desde el " + borrador.UltimoLatido.ToString("dd/MM/yyyy HH:mm:ss", CulturaAr) + " (posible corte de luz o de red)";
            else
                estado = "En curso (el operador lo está armando)";

            var modelo = new DetalleBorradorGenericoVm
            {
                Borrador = borrador,
                Eventos = negocio.ListarEventosPorBorrador(id),
                EstadoTexto = estado,
                // Misma tabla fisica que Negocio.VentaBorrador: se reusa CrearNegocio() solo para leer
                // esta notificacion puntual (no se duplica ObtenerNotificacion en Negocio.BorradorGenerico).
                Notificacion = idNotificacion > 0 ? CrearNegocio().ObtenerNotificacion(idNotificacion) : null,
                MinutosSinLatido = minutos
            };

            return PartialView("_DetalleBorradorGenerico", modelo);
        }

        // Advertencias "producto pesado sin agregar" de un operador en un dia, con contexto de patron.
        [HttpGet]
        public IActionResult DetalleProductoSinAgregar(int idOperador, string dia)
        {
            if (!EsAdmin) return StatusCode(403);
            // "Producto sin agregar" solo existe en Postgres (etapa 2 en SQL Server).
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado || !WebCore.Helpers.PosBorradorSettings.SoportaNotificaciones) return NotFound();

            if (!DateTime.TryParseExact(dia ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                fecha = DateTime.Today;
            fecha = fecha.Date;

            var negocio = CrearNegocio();
            var eventos = negocio.ListarProductoSinAgregarDelDia(idOperador, fecha);

            DateTime manana = fecha.AddDays(1);
            long refId = Entidades.Notificacion.RefIdOperadorDia(idOperador, fecha);
            var notificacion = negocio.ListarNotificaciones(false, 200)
                .FirstOrDefault(n => n.Tipo == Entidades.Notificacion.TipoProductoSinAgregar && n.RefId == refId);

            var modelo = new DetalleProductoSinAgregarVm
            {
                IdOperador = idOperador,
                NombreOperador = eventos.Count > 0 && !string.IsNullOrWhiteSpace(eventos[0].NombreOperador) ? eventos[0].NombreOperador : ("Usuario #" + idOperador),
                Dia = fecha,
                Eventos = eventos,
                Ultimos7Dias = negocio.ResumirProductoSinAgregar(idOperador, manana.AddDays(-DiasContexto7), manana),
                Ultimos30Dias = negocio.ResumirProductoSinAgregar(idOperador, manana.AddDays(-DiasContexto30), manana),
                Notificacion = notificacion,
                SegundosUmbral = WebCore.Helpers.PosBorradorSettings.SegundosProductoSinAgregar,
                SegundosCantidadCero = WebCore.Helpers.PosBorradorSettings.SegundosCantidadCeroConfirmacion
            };

            return PartialView("_DetalleProductoSinAgregar", modelo);
        }

        // Guarda la revision del admin sobre una advertencia (Justificada / Sospechosa + comentario).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RevisarProductoSinAgregar([FromBody] RevisionProductoSinAgregarRequest request)
        {
            if (!EsAdmin) return StatusCode(403, new { ok = false });
            if (!WebCore.Helpers.PosBorradorSettings.Habilitado || !WebCore.Helpers.PosBorradorSettings.SoportaNotificaciones) return Json(new { ok = false });

            try
            {
                if (request == null || request.Id <= 0) return Json(new { ok = false, msg = "Falta la advertencia." });

                bool ok = CrearNegocio().RevisarProductoSinAgregar(
                    request.Id, request.Revision, request.Comentario, _sesion.UsuarioActual.Id, out string? error);
                return ok ? Json(new { ok = true, revisor = _sesion.UsuarioActual.Nombre }) : Json(new { ok = false, msg = error });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo guardar la revisión.", error = ex.Message });
            }
        }
    }
}
