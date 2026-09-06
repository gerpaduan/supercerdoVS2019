// Port parcial de Web/Controllers/PuntosExpendioController.cs (1286 lineas, 20 acciones) para el
// Modulo 8 (Ventas y POS) -- ver docs/DECISIONS.md y docs/10-migracion-aspnet-core/README.md. Este
// slice porta SOLO: ExpendiosGenerados/ExpendiosGeneradosData (listado de solo lectura de
// expendios ya generados) y Sectores/GuardarSector/EliminarSector (catalogo simple de sectores,
// mismo perfil de riesgo que TiposEgresoCaja de Modulo 7 -- CRUD chico, sin dinero involucrado).
//
// AGREGADO (2026-09-03, ver docs/DECISIONS.md): ImprimirPdf (PDF con QuestPDF, reemplazo de
// iTextSharp elegido por el usuario) y ObtenerDatosEmailExpendio/EnviarComprobanteEmailExpendio
// (envio real de email, autorizado explicitamente) -- portados, ver mas abajo.
//
// AGREGADO (2026-09-04, PLAN-POS.md batch 7): FinalizarPOS -- el endpoint transaccional que crea
// el expendio real (Negocio.Venta.agregarExpendio/agregarLineaExprendio), mismo patron y mismo
// nivel de verificacion que VentasController.FinalizarVenta (HTTP directo + sqlcmd, sin UI propia
// todavia). ResolverOperadorPOS/AutorizarOperadorPOS/CerrarOperadorPOS portados 2026-09-06
// (retomado del Batch 5 del plan de login/permisos reales, ver docs/DECISIONS.md) --
// exigirPermisoVentas=false a diferencia de Ventas: cualquier usuario activo puede operar
// Expendio, no solo quien tiene Ventas > Editar.
//
// Las acciones restantes del original siguen sin portar en este slice:
//  - Abrir, Guardar, BuscarProducto, BuscarProductoPorCodigo, MisExpendiosPOS.
//  - Impresion de tickets ESC/POS (agente de impresion local, sin relacion con el bloqueante de
//    PDF): ImprimirTicket, ImprimirTicketPayload, DescargarAgenteImpresion.
//
// Consecuencia visible en la vista: el boton "Imprimir" (ticket) de cada card de
// ExpendiosGenerados sigue excluido; se agregan botones nuevos "PDF" y "Email" en su lugar.
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06) -- ya no hay stub hardcodeado. PermisosHelper.TienePermiso(Session,
// Permisos.Venta.NuevaVenta, ...) del original (gate de ExpendiosGenerados/Guardar) se sigue
// omitiendo -- son permisos de "ver"/"nueva venta" generales del modulo, no del mecanismo de
// usuario de produccion (ya portado arriba); TODO(claude): revisar si hace falta portarlos aparte.
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Utilidades;
using WebCore.Models;
using WebCore.Models.DTO;

namespace WebCore.Controllers
{
    public class PuntosExpendioController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.BarcodeInterpreter _oBarcodeInterpreter;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public PuntosExpendioController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oVentaN = WebCore.Infrastructure.NegocioFactory.CrearVenta(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oBarcodeInterpreter = WebCore.Infrastructure.NegocioFactory.CrearBarcodeInterpreter(_empresa, _param);
        }

        // ===== "Usuario de produccion" para el POS de Expendio (2026-09-06, retomado del Batch 5
        // del plan de login/permisos reales, ver docs/DECISIONS.md) -- mismo patron que
        // VentasController, sin centralizar (ningun base controller comun entre ambos en WebCore).
        // Diferencia real respecto a Ventas: exigirPermisoVentas=false (cualquier usuario activo
        // puede operar Expendio, no solo quien tiene Ventas > Editar, ver
        // Web/Controllers/BaseController.cs:328-330) y PuedeBonificarPuntoExpendio se recalcula
        // contra el operador resuelto (antes hardcodeado a true bajo el stub). =====
        private Entidades.Usuario ResolverOperadorPOS(string posInstanceId, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion) return usuarioSesion;
            return ObtenerOperadorPOS(posInstanceId) ?? usuarioSesion;
        }

        private static string ClaveSessionOperadorPOS(string posInstanceId) => "OperadorPOS_" + (posInstanceId ?? "");

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

        // Ver comentario identico en VentasController.ObtenerSessionIdEstable -- ASP.NET Core
        // Session no manda el Set-Cookie hasta el primer write, asi que sin esto el rate-limit
        // por sesion nunca acumula.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // exigirPermisoVentas=false: a diferencia de Ventas, cualquier usuario activo puede operar
        // Expendio (ver Web/Controllers/BaseController.cs:328-330, PuntosExpendioController.cs:127).
        private JsonResult ValidarOperadorPOS(int idUsuario, string clave, string posInstanceId, bool exigirPermisoVentas = false)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AutorizarOperadorPOS(int idUsuario, string clave, string posInstanceId)
        {
            return ValidarOperadorPOS(idUsuario, clave, posInstanceId, exigirPermisoVentas: false);
        }

        [HttpPost]
        public JsonResult CerrarOperadorPOS(string posInstanceId)
        {
            LimpiarOperadorPOS(posInstanceId);
            return Json(new { ok = true });
        }

        // Port de Web/Controllers/BaseController.cs:272-285.
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

        // Port de PuntosExpendioController.POS (batch UI de Expendios, ver
        // docs/10-migracion-aspnet-core/PLAN-POS-EXPENDIOS-UI.md). Login de operador de produccion
        // portado 2026-09-06 (retomado del Batch 5 del plan de login/permisos reales, ver
        // docs/DECISIONS.md) -- exigirPermisoVentas=false, a diferencia de Ventas/POS: cualquier
        // usuario activo puede operar Expendio (Web/Controllers/BaseController.cs:328-330). Sin
        // AsegurarSucursalUsuario (no aplica: la sucursal del usuario logueado ya es fija).
        [HttpGet]
        public IActionResult POS(string sector = "", string modoPos = "original", string posInstanceId = "")
        {
            var user = _usuarioActual;

            string modoPosNormalizado = string.Equals(modoPos, "duplicado", StringComparison.OrdinalIgnoreCase)
                ? "duplicado"
                : "original";
            string posInstanceIdNormalizado = string.IsNullOrWhiteSpace(posInstanceId)
                ? Guid.NewGuid().ToString("N")
                : posInstanceId.Trim();

            // Port literal de Web/Controllers/PuntosExpendioController.cs:90-100.
            var operador = user;
            bool requiereOperadorPOS = false;
            if (user.EsUsuarioProduccion)
            {
                operador = ObtenerOperadorPOS(posInstanceIdNormalizado);
                requiereOperadorPOS = operador == null;
                ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            }
            operador = operador ?? user;
            ViewBag.OperadorPOSNombre = (user.EsUsuarioProduccion && !requiereOperadorPOS) ? operador.Nombre : null;

            string sectorNormalizado = (sector ?? "").Trim();
            var model = new PuntoExpendioEditVm
            {
                Sector = sectorNormalizado,
                FechaExpendio = DateTime.Now,
                IdentificacionCliente = "",
                Observaciones = "",
                EsGuardado = false,
                SectoresDisponibles = ObtenerSectores(),
                PermiteEditarPrecio = string.Equals(sectorNormalizado, "PRESUPUESTO", StringComparison.OrdinalIgnoreCase),
                VendedorNombre = user.Nombre ?? "",
                SucursalNombre = user.Sucursal?.SucursalNombre ?? ""
            };

            ViewBag.IdConsumidorFinal = _oPersonaN.getConsumidorFinal()?.idPersona ?? 0;
            // -1 = Utilidades.ValoresParametrosMetodos.IdCreadorNulo() (chequeo de "ver", no de
            // edicion) -- esa clase vive en Utilidades.csproj (WinForms), no en Utilidades.Core,
            // que es lo unico que WebCore referencia; se usa el literal directo.
            ViewBag.PuedeBonificarPuntoExpendio = !requiereOperadorPOS &&
                _oUsuarioN.tienePermiso(operador, Entidades.Permisos.Venta.Bonificar, DateTime.Today, -1);
            ViewBag.IdSucursalPOS = user.IdSucursal;
            ViewBag.IdUsuarioPOS = user.Id;
            ViewBag.PosModoInstancia = modoPosNormalizado;
            ViewBag.PosInstanceId = posInstanceIdNormalizado;
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.RequiereOperadorPOS = requiereOperadorPOS;

            return View("~/Views/PuntosExpendio/POS.cshtml", model);
        }

        // Port de PuntosExpendioController.BuscarProductoPOS -- mismo motor que
        // VentasController.BuscarProducto (Negocio.BarcodeInterpreter, codigos genericos G/G1 y
        // codigos internos de balanza 20-29), sin cambios de logica.
        [HttpGet]
        public IActionResult BuscarProductoPOS(string codigo, bool ingresoCantidadX = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                    return Json(new { success = false, message = "Código inválido." });

                codigo = codigo.Replace(",", ".");
                int idEmpresaSesion = _usuarioActual.IdEmpresa;

                var generico = _oBarcodeInterpreter.InterpretarCodigoGenerico(
                    codigo, ingresoCantidadX, _param.GetLong(ParamKeys.CodProdGenerico, 0L));

                if (generico.FormatoInvalido)
                    return Json(new { success = false, message = "Formato de código inválido." });

                Entidades.Corte corte;
                decimal? cantidadSugerida = null;

                if (generico.EsGenerico)
                {
                    corte = idEmpresaSesion > 0
                        ? _oCorteN.findCorteByCodigoEmpresa(generico.CodigoProducto, idEmpresaSesion, false)
                        : _oCorteN.findCorteByCodigo(generico.CodigoProducto, false);

                    if (corte == null || corte.IdCorte <= 0)
                        return Json(new { success = false, message = "No existe el código genérico." });

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
                            return Json(new { success = false, message = "No se encontró el producto." });

                        corte = interno.Producto;
                        if (interno.TipoValor == Entidades.TipoValorCodigoBarras.Precio)
                            corte.PrecioKg = (float)interno.Valor.Value;
                        else
                            cantidadSugerida = interno.Valor;
                    }
                    else
                    {
                        if (!long.TryParse(codigo, out long codigoBuscado) || codigoBuscado <= 0)
                            return Json(new { success = false, message = "Código inválido." });

                        corte = idEmpresaSesion > 0
                            ? _oCorteN.findCorteByCodigoEmpresa(codigoBuscado, idEmpresaSesion, false)
                            : _oCorteN.findCorteByCodigo(codigoBuscado, false);

                        if (corte == null || corte.IdCorte <= 0)
                            return Json(new { success = false, message = "No se encontró el producto." });
                    }
                }

                return Json(new
                {
                    id = corte.IdCorte,
                    codigo = corte.codigo.ToString(),
                    nombre = !string.IsNullOrWhiteSpace(corte.corte) ? corte.corte : corte.CorteDesc,
                    precioKg = corte.PrecioKg,
                    precioOriginal = corte.PrecioKg,
                    pesable = corte.Pesable,
                    balanza = corte.Pesable,
                    cantidadSugerida
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult ExpendiosGenerados()
        {
            var sectoresDt = _oVentaN.obtenerSectores();
            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            _oUsuarioN.obtenerUsuarios(true);
            var usuarios = (_oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(x => x != null && x.Activo)
                .OrderBy(x => x.Nombre ?? "")
                .ToList();

            ViewBag.Title = "Expendios generados";
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewBag.SectoresExpendio = (sectoresDt != null
                ? sectoresDt.AsEnumerable()
                    .Select(r => Convert.ToString(r["sector"] ?? ""))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .Select(s => new SelectListItem { Value = s, Text = s })
                    .ToList()
                : new List<SelectListItem>());
            ViewBag.SucursalesExpendio = sucursales
                .Where(s => s != null && s.idSucursal > 0)
                .OrderBy(s => s.sucursal ?? "")
                .Select(s => new SelectListItem { Value = s.sucursal ?? "", Text = s.sucursal ?? "" })
                .ToList();
            ViewBag.UsuariosExpendio = usuarios
                .Where(u => u.Id > 0)
                .Select(u => new SelectListItem { Value = u.Nombre ?? "", Text = u.Nombre ?? "" })
                .ToList();

            return View("~/Views/PuntosExpendio/ExpendiosGenerados.cshtml");
        }

        [HttpGet]
        public IActionResult ExpendiosGeneradosData(string fechaDesde = null, string fechaHasta = null, int top = 300)
        {
            var user = _usuarioActual;
            if (user.IdSucursal == 0)
                return Json(new { ok = false, mensaje = "Sesión inválida o sucursal no seleccionada." });

            try
            {
                DateTime fechaDesdeValue;
                DateTime fechaHastaValue;
                DateTime? fechaDesdeFiltro = DateTime.TryParse(fechaDesde, out fechaDesdeValue) ? (DateTime?)fechaDesdeValue : null;
                DateTime? fechaHastaFiltro = null;
                if (DateTime.TryParse(fechaHasta, out fechaHastaValue))
                {
                    fechaHastaFiltro = fechaHastaValue.TimeOfDay == TimeSpan.Zero
                        ? fechaHastaValue.AddDays(1).AddSeconds(-1)
                        : fechaHastaValue;
                }
                DataTable dt = _oVentaN.obtenerExpendiosEmpresa(top <= 0 ? 300 : top, fechaDesdeFiltro, fechaHastaFiltro);

                var items = dt.AsEnumerable()
                    .Select(row =>
                    {
                        DateTime fechaExpendio = row["fechaExpendio"] != DBNull.Value
                            ? Convert.ToDateTime(row["fechaExpendio"])
                            : DateTime.MinValue;

                        int idExpendio = row["idExpendio"] != DBNull.Value ? Convert.ToInt32(row["idExpendio"]) : 0;
                        int idVenta = row["idVenta"] != DBNull.Value ? Convert.ToInt32(row["idVenta"]) : 0;
                        var expendio = idExpendio > 0 ? _oVentaN.getExpedioById(idExpendio) : null;
                        var lineas = (expendio != null ? expendio.LineasVenta : null) ?? new List<Entidades.LineaVenta>();

                        return new
                        {
                            fechaExpendio = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("yyyy-MM-ddTHH:mm:ss") : "",
                            fecha = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("dd/MM/yyyy") : "",
                            hora = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("HH:mm") : "",
                            idExpendio = idExpendio,
                            identificacionExpendio = Convert.ToString(row["identificacionExpendio"] ?? ""),
                            sucursal = Convert.ToString(row["sucursal"] ?? ""),
                            sector = Convert.ToString(row["sector"] ?? ""),
                            usuario = Convert.ToString(row["vendedor"] ?? ""),
                            cantItems = Convert.ToString(row["cantItems"] ?? "0"),
                            totalKg = row["totalKg"] != DBNull.Value ? Convert.ToDecimal(row["totalKg"]) : 0m,
                            totalImporte = row["importe"] != DBNull.Value ? Convert.ToDecimal(row["importe"]) : 0m,
                            idVenta = idVenta,
                            estado = idVenta > 0 && idVenta != idExpendio ? "Asignado" : "Pendiente",
                            pdfUrl = idExpendio > 0 ? Url.Action("ImprimirPdf", "PuntosExpendio", new { id = idExpendio }) : "",
                            emailUrl = idExpendio > 0 ? Url.Action("ObtenerDatosEmailExpendio", "PuntosExpendio", new { idExpendio }) : "",
                            lineas = lineas.Select(l => new
                            {
                                codigo = l.Corte != null ? l.Corte.Codigo : 0,
                                producto = l.Corte != null
                                    ? (!string.IsNullOrWhiteSpace(l.Corte.corte) ? l.Corte.corte : l.Corte.CorteDesc)
                                    : "",
                                cantKg = l.CantKg,
                                precioKg = l.PrecioKg,
                                total = l.CantKg * l.PrecioKg
                            }).ToList()
                        };
                    })
                    .ToList();

                return Json(new { ok = true, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "No se pudieron consultar los expendios: " + ex.Message });
            }
        }

        public IActionResult Sectores(string editar = "")
        {
            var model = new SectorAbmVm
            {
                SectorOriginal = editar ?? "",
                Nombre = editar ?? ""
            };

            CargarSectores(model);
            ViewBag.Title = "Sectores";
            return View("~/Views/PuntosExpendio/Sectores.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarSector(SectorAbmVm model)
        {
            string nombre = (model != null ? model.Nombre : "") ?? "";
            string nombreNormalizado = nombre.Trim();
            string sectorOriginal = (model != null ? model.SectorOriginal : "") ?? "";

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Debe ingresar un nombre de sector.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (_oVentaN.existeSector(nombreNormalizado, sectorOriginal))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Ya existe otro sector con ese nombre en esta empresa.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (string.IsNullOrWhiteSpace(sectorOriginal))
            {
                _oVentaN.agregarSector(nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se creó correctamente.";
            }
            else
            {
                _oVentaN.modificarSector(sectorOriginal, nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se actualizó correctamente.";
            }

            return RedirectToAction("Sectores");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarSector(string sector)
        {
            string nombre = (sector ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se recibió un sector válido para eliminar.";
                return RedirectToAction("Sectores");
            }

            if (_oVentaN.sectorEstaEnUso(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se puede eliminar el sector porque está en uso en puntos de expendio.";
                return RedirectToAction("Sectores");
            }

            _oVentaN.eliminarSector(nombre);
            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Sectores";
            TempData["AlertMsg"] = "El sector se eliminó correctamente.";
            return RedirectToAction("Sectores");
        }

        // Port de PuntosExpendioController.FinalizarPOS (2026-09-04, PLAN-POS.md batch 7) -- crea
        // el expendio real (Venta con TipoVenta="Caja", FormaPago=Nulo, TipoComprobante=X) y sus
        // lineas. Sin cambios de logica respecto al original mas alla de: Session["Usuario"] ->
        // _usuarioActual; ResolverOperadorPOS omitido (ver header del archivo); [FromBody] porque
        // el body es JSON (mismo motivo que VentasController.FinalizarVenta).
        [HttpPost]
        public IActionResult FinalizarPOS([FromBody] FinalizarPuntoExpendioRequest request)
        {
            var user = _usuarioActual;
            DateTime fecha = request != null && request.FechaExpendio.HasValue ? request.FechaExpendio.Value : DateTime.Today;
            var operador = ResolverOperadorPOS(request?.PosInstanceId, user);

            var model = new PuntoExpendioEditVm
            {
                FechaExpendio = request != null && request.FechaExpendio.HasValue ? request.FechaExpendio.Value : DateTime.Now,
                Sector = request != null ? request.Sector : "",
                IdentificacionCliente = request != null ? request.IdentificacionCliente : "",
                Observaciones = request != null ? request.Observaciones : "",
                Lineas = new List<PuntoExpendioLineaVm>()
            };

            foreach (var linea in (request != null ? request.LineasVenta : null) ?? new List<LineaVentaDto>())
            {
                if (linea == null || Entidades.LineaVenta.esAnulado(linea.Estado))
                    continue;

                model.Lineas.Add(new PuntoExpendioLineaVm
                {
                    IdCorte = linea.IdCorte,
                    Codigo = linea.Codigo,
                    Producto = linea.Descripcion ?? "",
                    CantKg = linea.CantKg,
                    PrecioKg = linea.PrecioKg,
                    PesoBalanza = linea.Balanza,
                    Total = linea.Importe
                });
            }

            NormalizarLineas(model);

            string error = ValidarModelo(model);
            if (!string.IsNullOrWhiteSpace(error))
                return Json(new { ok = false, mensaje = error });

            var consumidorFinal = _oPersonaN.getConsumidorFinal() ?? new Entidades.Persona();
            var sucursal = user.Sucursal ?? _oSucursalN.findById(user.IdSucursal);
            if (sucursal == null)
                return Json(new { ok = false, mensaje = "No se encontró la sucursal activa del usuario." });

            var expendio = new Entidades.Venta
            {
                IdVenta = 0,
                Persona = consumidorFinal,
                Sucursal = sucursal,
                TipoVenta = "Caja",
                FechaVenta = model.FechaExpendio,
                Turno = "",
                DiaFestivo = "",
                TotalImporte = model.Lineas.Sum(l => l.Total),
                AcumRedondeoImporte = 0,
                AcumRedondeoKgs = 0,
                LineasVenta = new List<Entidades.LineaVenta>(),
                FormaPago = Entidades.Venta.formaPagoEnum.Nulo.ToString(),
                TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()),
                IdentificacionExpendio = model.IdentificacionCliente ?? "",
                Sector = (model.Sector ?? "").Trim(),
                CantItems = model.Lineas.Count.ToString(CultureInfo.InvariantCulture),
                Observaciones = model.Observaciones ?? "",
                NroRemito = "",
                SerialCPU = "",
                Vendedor = operador
            };

            try
            {
                expendio.IdVenta = expendio.IdExpendio = _oVentaN.agregarExpendio(expendio);

                foreach (var linea in model.Lineas)
                {
                    int idEmpresaSesionLinea = user.IdEmpresa;
                    var corte = linea.IdCorte > 0
                        ? _oCorteN.findCorteById(linea.IdCorte, false)
                        : (linea.Codigo > 0
                            ? (idEmpresaSesionLinea > 0
                                ? _oCorteN.findCorteByCodigoEmpresa(linea.Codigo, idEmpresaSesionLinea, false)
                                : _oCorteN.findCorteByCodigo(linea.Codigo, false))
                            : null);

                    if (corte == null || corte.IdCorte <= 0 || (idEmpresaSesionLinea > 0 && corte.IdEmpresa != idEmpresaSesionLinea))
                        return Json(new { ok = false, mensaje = "No se encontró uno de los productos cargados." });

                    var item = new Entidades.LineaVenta
                    {
                        Venta = expendio,
                        Corte = new Entidades.Corte { IdCorte = corte.IdCorte },
                        CantKg = linea.CantKg,
                        KgsTotalCalculado = linea.CantKg,
                        PrecioKg = linea.PrecioKg,
                        PesoBalanza = linea.PesoBalanza,
                        Estado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado),
                        IndexAnulado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado)
                    };

                    _oVentaN.agregarLineaExprendio(item);
                }

                return Json(new
                {
                    ok = true,
                    idExpendio = expendio.IdExpendio,
                    redirectUrl = Url.Action("POS", "PuntosExpendio", new { sector = expendio.Sector, posInstanceId = request != null ? request.PosInstanceId : null }),
                    pdfUrl = Url.Action("ImprimirPdf", "PuntosExpendio", new { id = expendio.IdExpendio })
                    // imprimirUrl/imprimirPayloadUrl/whatsappTexto excluidos -- ImprimirTicket
                    // (agente de impresion local) sigue sin portar, ver header del archivo.
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        private string ValidarModelo(PuntoExpendioEditVm model)
        {
            if (model == null)
                return "No se recibieron datos del punto de expendio.";

            model.Sector = (model.Sector ?? "").Trim();
            if (string.IsNullOrWhiteSpace(model.Sector))
                return "Debe seleccionar un sector para el punto de expendio.";

            if (!ObtenerSectores().Any(s => string.Equals(s, model.Sector, StringComparison.OrdinalIgnoreCase)))
                return "El sector seleccionado no existe o ya no está disponible.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe cargar al menos un producto en el punto de expendio.";

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                PuntoExpendioLineaVm linea = model.Lineas[i];
                if (linea == null || linea.IdCorte <= 0)
                    return "Hay una línea sin producto válido.";

                if (linea.CantKg <= 0)
                    return "La cantidad en kilos debe ser mayor a cero en todas las líneas.";

                if (linea.PrecioKg <= 0)
                    return "El precio por kilo debe ser mayor a cero en todas las líneas.";
            }

            return null;
        }

        private void NormalizarLineas(PuntoExpendioEditVm model)
        {
            if (model == null || model.Lineas == null)
                return;

            foreach (var linea in model.Lineas)
            {
                if (linea == null)
                    continue;

                linea.Producto = (linea.Producto ?? "").Trim();
                linea.Total = linea.CantKg * linea.PrecioKg;
            }
        }

        [HttpGet]
        public IActionResult ImprimirPdf(int id)
        {
            var expendio = _oVentaN.getExpedioById(id);
            if (expendio == null || expendio.IdExpendio <= 0)
                return NotFound();

            byte[] bytes = GenerarPdfPuntoExpendio(expendio);
            return File(bytes, "application/pdf", "PuntoExpendio_" + id + ".pdf");
        }

        // Reemplazo de "Enviar por WhatsApp" en el modal post-expendio (ver docs/DECISIONS.md).
        // El punto de expendio no tiene factura electronica ni nota de credito, y el cliente es
        // texto libre (sin Persona.Email confiable) -- por eso el email destino arranca vacio, a
        // cargar a mano.
        [HttpGet]
        public IActionResult ObtenerDatosEmailExpendio(int idExpendio)
        {
            try
            {
                var expendio = _oVentaN.getExpedioById(idExpendio);
                if (expendio == null || expendio.IdExpendio <= 0)
                    return Json(new { ok = false, msg = "Punto de expendio no encontrado." });

                string nombreEmpresa = ObtenerNombreEmpresaExpendio(expendio);
                string asunto = "Punto de expendio " + expendio.IdExpendio + " - " + nombreEmpresa;
                string cuerpo =
                    "Hola:\n\n" +
                    "Adjuntamos el comprobante del punto de expendio Nro " + expendio.IdExpendio + ".\n\n" +
                    "Este correo fue enviado automáticamente. Por favor, no responda a este mensaje.\n\n" +
                    "Atentamente,\n" +
                    nombreEmpresa;

                return Json(new { ok = true, email = "", asunto, mensaje = cuerpo });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EnviarComprobanteEmailExpendio(int idExpendio, string emailDestino, string asunto, string mensaje)
        {
            try
            {
                var expendio = _oVentaN.getExpedioById(idExpendio);
                if (expendio == null || expendio.IdExpendio <= 0)
                    return Json(new { ok = false, msg = "Punto de expendio no encontrado." });

                emailDestino = (emailDestino ?? "").Trim();
                asunto = (asunto ?? "").Trim();
                mensaje = (mensaje ?? "").Trim();

                if (string.IsNullOrWhiteSpace(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email destino." });

                if (!SmtpMailHelper.IsValidEmail(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email válido." });

                if (string.IsNullOrWhiteSpace(asunto))
                    return Json(new { ok = false, msg = "Ingrese un asunto." });

                string nombreEmpresa = ObtenerNombreEmpresaExpendio(expendio);
                var empresaExpendio = ObtenerEmpresaExpendio(expendio);
                byte[] pdfBytes = GenerarPdfPuntoExpendio(expendio);
                string nombreAdjunto = "PuntoExpendio_" + expendio.IdExpendio + ".pdf";
                string fromName = "CarniSys - " + nombreEmpresa;
                string replyToEmail = empresaExpendio != null ? (empresaExpendio.Email ?? "").Trim() : "";

                SmtpMailHelper.SendMail(
                    toEmail: emailDestino,
                    toName: "",
                    subject: asunto,
                    bodyHtml: ConvertirTextoAHtmlExpendio(mensaje),
                    attachmentFileName: nombreAdjunto,
                    attachmentBytes: pdfBytes,
                    attachmentContentType: "application/pdf",
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

        private Entidades.Empresa ObtenerEmpresaExpendio(Entidades.Venta expendio)
        {
            return (expendio != null && expendio.Sucursal != null ? expendio.Sucursal.Empresa : null)
                ?? _usuarioActual.Empresa;
        }

        private string ObtenerNombreEmpresaExpendio(Entidades.Venta expendio)
        {
            var empresaExpendio = ObtenerEmpresaExpendio(expendio);
            string nombre = empresaExpendio != null
                ? (!string.IsNullOrWhiteSpace(empresaExpendio.NombreFantasia) ? empresaExpendio.NombreFantasia : empresaExpendio.RazonSocialAfip)
                : "";
            return !string.IsNullOrWhiteSpace(nombre)
                ? nombre.Trim()
                : "CarniSys";
        }

        private string ConvertirTextoAHtmlExpendio(string texto)
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

        // Port de GenerarPdfPuntoExpendio (Web/Controllers/PuntosExpendioController.cs) de
        // iTextSharp a QuestPDF -- mismo contenido/orden de campos, sintaxis nueva.
        private byte[] GenerarPdfPuntoExpendio(Entidades.Venta expendio)
        {
            var lineas = expendio.LineasVenta ?? new List<Entidades.LineaVenta>();
            var culturaAr = new CultureInfo("es-AR");

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(4);
                        col.Item().Text("Punto de Expendio").FontSize(16).Bold();
                        col.Item().PaddingTop(6);
                        col.Item().Text("Nro: " + expendio.IdExpendio);
                        col.Item().Text("Sector: " + (expendio.Sector ?? "-"));
                        col.Item().Text("Fecha: " + expendio.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                        col.Item().Text("Cliente: " + (!string.IsNullOrWhiteSpace(expendio.IdentificacionExpendio) ? expendio.IdentificacionExpendio : "-"));
                        col.Item().Text("Sucursal: " + (expendio.Sucursal != null ? expendio.Sucursal.SucursalNombre : "-"));
                        col.Item().Text("Vendedor: " + (expendio.Vendedor != null ? expendio.Vendedor.Nombre : "-"));
                        col.Item().PaddingTop(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(6f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Código").Bold();
                                header.Cell().Text("Producto").Bold();
                                header.Cell().Text("Kgs.").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            foreach (var linea in lineas)
                            {
                                string nombreProducto = linea.Corte != null
                                    ? (!string.IsNullOrWhiteSpace(linea.Corte.corte) ? linea.Corte.corte : linea.Corte.CorteDesc)
                                    : "";

                                table.Cell().Text(linea.Corte != null ? linea.Corte.Codigo.ToString() : "");
                                table.Cell().Text(nombreProducto);
                                table.Cell().Text(linea.CantKg.ToString("F3", CultureInfo.InvariantCulture));
                                table.Cell().Text((linea.CantKg * linea.PrecioKg).ToString("$ #,##0.00", culturaAr));
                            }
                        });

                        col.Item().PaddingTop(10);
                        col.Item().Text("Total items: " + lineas.Count).Bold();
                        col.Item().Text("Total kilos: " + lineas.Sum(x => x.CantKg).ToString("F3", CultureInfo.InvariantCulture)).Bold();
                        col.Item().Text("Total importe: " + expendio.TotalImporte.ToString("$ #,##0.00", culturaAr)).Bold();
                    });
                });
            });

            return documento.GeneratePdf();
        }

        private List<string> ObtenerSectores()
        {
            DataTable dt = _oVentaN.obtenerSectores() ?? new DataTable();
            return dt.Rows
                .Cast<DataRow>()
                .Select(r => Convert.ToString(r["sector"] ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
        }

        private void CargarSectores(SectorAbmVm model)
        {
            if (model == null)
                return;

            model.Sectores = ObtenerSectores()
                .Select(s => new SectorResumenVm
                {
                    Nombre = s,
                    EnUso = _oVentaN.sectorEstaEnUso(s)
                })
                .ToList();
        }
    }
}
