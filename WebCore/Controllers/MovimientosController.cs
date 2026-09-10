// Port de Web/Controllers/MovimientosController.cs (932 lineas, 10 acciones) -- traslados de
// stock entre sucursales. Portado completo: Index/Lineas (listados), Nuevo/Editar/Guardar
// (alta/edicion), Detalle (AJAX), BuscarProducto/BuscarProductoPorCodigo (autocompletado),
// ImprimirPdf (QuestPDF, ver GenerarDocsCore.GenerarPdfMovimiento). NO portado: ImprimirTicket/
// ImprimirTicketPayload/DescargarAgenteImpresion -- dependen del agente de impresion local
// (print-agent.js), mismo bloqueante ya documentado en el resto de la migracion (Ventas/
// PuntosExpendio); el boton "Imprimir ticket" del modal post-guardado se excluye, "Generar PDF"
// y "Enviar a WhatsApp" si se portan (no dependen del agente).
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06). Permisos reales y gate de "usuario de sala de produccion" portados 2026-09-09
// (Batch B, ver docs/DECISIONS.md "Batch B: permisos reales + operador de produccion"):
// Permisos.Movimiento.NuevoMovimiento/VerMovimientos via _oUsuarioN.tienePermiso (equivalente
// exacto de PermisosHelper.TienePermiso del clasico, que por debajo llama al mismo metodo),
// SeleccionUsuario para la cuenta compartida de produccion (port de
// Web/Controllers/SeleccionUsuarioController.cs, WebCore/Helpers/PermisosHelper.cs). NO
// portado (deliberado, UX-only, no afecta el control de acceso en si -- Negocio.Usuario.
// tienePermiso ya aplica el limite de fecha internamente): ConstruirMensajePermisoFecha/
// AjustarFechaSiNoTienePermiso (arman un mensaje mas detallado con la fecha minima permitida y
// recortan el filtro de fecha en silencio) -- se usa un mensaje generico en su lugar.
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class MovimientosController : Controller
    {
        private const long CuitColumnasInternas = 20306210786;

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Usuario _oUsuarioN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public MovimientosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
        }

        public IActionResult Index(int idSucursalOrigen = 0, int idSucursalDestino = 0, DateTime? fechaDesde = null, DateTime? fechaHasta = null, bool verDetalles = false)
        {
            var user = _usuarioActual;
            DateTime fechaLimiteSinPermiso = DateTime.Today.AddDays(-_param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime desde = fechaDesde ?? fechaLimiteSinPermiso;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            string sucOrigen = ObtenerNombreSucursalFiltro(sucursales, idSucursalOrigen);
            string sucDestino = ObtenerNombreSucursalFiltro(sucursales, idSucursalDestino);
            DataTable dt = _oCorteN.obtenerMovimientos(sucOrigen, sucDestino, desde, hasta, "") ?? new DataTable();

            var model = new MovimientoIndexVm
            {
                IdSucursalOrigen = idSucursalOrigen,
                IdSucursalDestino = idSucursalDestino,
                FechaDesde = desde,
                FechaHasta = hasta,
                VerDetalles = verDetalles,
                MostrarColumnasInternas = ObtenerEmpresaCuit(user) == CuitColumnasInternas,
                Movimientos = ConstruirResumenes(dt)
            };

            ViewBag.Title = "Movimientos";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(sucursales);

            return View(model);
        }

        public IActionResult Lineas(int idSucursalOrigen = 0, int idSucursalDestino = 0, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string producto = "")
        {
            DateTime fechaLimiteSinPermiso = DateTime.Today.AddDays(-_param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime desde = fechaDesde ?? fechaLimiteSinPermiso;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            string sucOrigen = ObtenerNombreSucursalFiltro(sucursales, idSucursalOrigen);
            string sucDestino = ObtenerNombreSucursalFiltro(sucursales, idSucursalDestino);
            DataTable dt = _oCorteN.obtenerMovimientos(sucOrigen, sucDestino, desde.Date, hasta.Date, "") ?? new DataTable();

            var model = new MovimientoLineasIndexPageVm
            {
                IdSucursalOrigen = idSucursalOrigen,
                IdSucursalDestino = idSucursalDestino,
                Producto = producto ?? "",
                FechaDesde = desde,
                FechaHasta = hasta
            };

            foreach (DataRow row in dt.Rows)
            {
                int idMovimiento = ToInt(row, "Id Movimiento", "idMovimiento", "movimiento");
                if (idMovimiento <= 0 || model.Movimientos.Any(x => x.IdMovimiento == idMovimiento))
                    continue;

                Entidades.Movimiento movimiento = _oCorteN.cargarMovimiento(idMovimiento, false);
                if (movimiento == null || movimiento.IdMovimiento <= 0)
                    continue;

                var lineas = (movimiento.ListaCortesPorMov ?? new List<Entidades.CortePorMovimiento>())
                    .Select(linea => new MovimientoLineaDetalleItemVm
                    {
                        Codigo = linea.Corte != null ? linea.Corte.Codigo.ToString() : "-",
                        Producto = linea.Corte != null ? linea.Corte.CorteDesc : "-",
                        CantidadKgTexto = linea.CantKg.ToString("N3"),
                        CantidadUnidadTexto = linea.CantUnidad.ToString("N0"),
                        Observacion = ConstruirObservacionLineaMovimiento(linea),
                        CantidadKg = Convert.ToDecimal(linea.CantKg),
                        CantidadUnidad = Convert.ToDecimal(linea.CantUnidad)
                    })
                    .Where(x => CoincideProductoMovimiento(x.Codigo, x.Producto, producto))
                    .ToList();

                if (lineas.Count == 0)
                    continue;

                var grupo = new MovimientoLineasGrupoVm
                {
                    IdMovimiento = movimiento.IdMovimiento,
                    CollapseId = "movLineas_" + movimiento.IdMovimiento,
                    Titulo = "MOVIMIENTO ID: " + movimiento.IdMovimiento,
                    Subtitulo = movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"),
                    ResumenCompacto = movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"),
                    ResumenSecundario = (movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-") + " -> " + (movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-"),
                    EditUrl = Url.Action("Editar", "Movimientos", new { id = movimiento.IdMovimiento }),
                    TotalKg = lineas.Sum(x => x.CantidadKg),
                    TotalUnidades = lineas.Sum(x => x.CantidadUnidad)
                };

                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Fecha", Valor = movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal origen", Valor = movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal destino", Valor = movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Estado", Valor = ToString(row, "Estado", "estado") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Usuario", Valor = movimiento.CreadoPor != null ? movimiento.CreadoPor.Nombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Observaciones", Valor = string.IsNullOrWhiteSpace(movimiento.Observaciones) ? "-" : movimiento.Observaciones });

                grupo.Lineas.AddRange(lineas);
                model.Movimientos.Add(grupo);
            }

            ViewBag.Title = "Lineas de movimiento";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(sucursales);

            return View(model);
        }

        public IActionResult Nuevo(int idUsuarioCreador = 0)
        {
            return Editar(0, idUsuarioCreador);
        }

        public IActionResult Editar(int id = 0, int idUsuarioCreador = 0)
        {
            var user = _usuarioActual;

            // Port de Web/Controllers/MovimientosController.cs:174-181 -- usuario de la cuenta
            // compartida de produccion sin identificar todavia (idUsuarioCreador<=0) redirige a
            // la pantalla comun de seleccion (sin contraseña, ver SeleccionUsuarioController).
            var redirectSeleccion = WebCore.Helpers.PermisosHelper.RequiereSeleccionUsuario(
                this, user, idUsuarioCreador, Request.Path + Request.QueryString);
            if (redirectSeleccion != null) return redirectSeleccion;

            MovimientoEditVm model;
            bool puedeModificar;

            if (id > 0)
            {
                model = CrearModeloEdicion(id, user);
                if (model == null)
                    return NotFound();

                // Port de Web/Controllers/MovimientosController.cs:192-199.
                puedeModificar = _oUsuarioN.tienePermiso(user, Entidades.Permisos.Movimiento.NuevoMovimiento, model.FechaMovimiento, user.Id);
                if (!puedeModificar && !_oUsuarioN.tienePermiso(user, Entidades.Permisos.Movimiento.VerMovimientos, model.FechaMovimiento, -1 /* Utilidades.ValoresParametrosMetodos.IdCreadorNulo() -- esa clase vive en Utilidades.csproj (WinForms), no en Utilidades.Core que es lo unico que WebCore referencia, se usa el literal directo (mismo criterio ya usado en PuntosExpendioController.cs) */))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "No tenés permisos para consultar movimientos.";
                    return RedirectToAction("Index");
                }

                model.SoloLecturaInicial = true;
                model.PuedeHabilitarEdicion = puedeModificar;
            }
            else
            {
                // Port de Web/Controllers/MovimientosController.cs:206-212.
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Movimiento.NuevoMovimiento, DateTime.Today, user.Id))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "No tenés permisos para crear o modificar movimientos.";
                    return RedirectToAction("Index");
                }

                model = CrearModeloNuevo(user);
                model.SoloLecturaInicial = false;
                model.PuedeHabilitarEdicion = true;
            }

            ViewBag.Title = model.EsEdicion ? "Modificar Movimiento" : "Nuevo Movimiento";
            ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.IdUsuarioCreadorPreseleccionado = idUsuarioCreador;

            // Bug real (2026-09-08, ver docs/DECISIONS.md): "return View(model)" sin nombre
            // explicito resuelve la vista segun la RUTA ENTRANTE, no segun el metodo C# que
            // efectivamente corrio -- al entrar por /Movimientos/Nuevo (que llama a Editar(0,...)
            // por invocacion directa de metodo, no RedirectToAction), ASP.NET Core buscaba
            // "Views/Movimientos/Nuevo.cshtml" (no existe) y tiraba InvalidOperationException.
            // Clasico usa ruta explicita ("~/Views/Movimientos/Editar.cshtml") por el mismo motivo.
            return View("Editar", model);
        }

        [HttpGet]
        public JsonResult BuscarProducto(string q = "")
        {
            try
            {
                int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
                var productos = idEmpresaSesion > 0
                    ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>())
                    : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>());
                if (!string.IsNullOrWhiteSpace(q))
                {
                    string filtro = q.Trim();
                    productos = productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.corte) && p.corte.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrWhiteSpace(p.CorteDesc) && p.CorteDesc.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        p.codigo.ToString().IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                var resultado = productos.Take(200).Select(p => new
                {
                    id = p.IdCorte,
                    codigo = p.codigo.ToString(),
                    nombre = !string.IsNullOrWhiteSpace(p.corte) ? p.corte : p.CorteDesc,
                    precio = p.precioKg
                }).ToList();

                return Json(resultado);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public JsonResult BuscarProductoPorCodigo(long? codigo)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
                return Json(new { ok = false, mensaje = "Código inválido." });

            int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
            var corte = idEmpresaSesion > 0
                ? _oCorteN.findCorteByCodigoEmpresa(codigo.Value, idEmpresaSesion, false)
                : _oCorteN.findCorteByCodigo(codigo.Value, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el producto." });

            return Json(new
            {
                ok = true,
                id = corte.IdCorte,
                codigo = corte.Codigo,
                nombre = corte.CorteDesc,
                tipo = corte.Tipo ?? "",
                promedio = corte.Promedio,
                pesable = corte.Pesable
            });
        }

        [HttpGet]
        public PartialViewResult Detalle(int id)
        {
            var movimiento = _oCorteN.cargarMovimiento(id, false);
            var lineas = _oCorteN.cargarCortesPorMovimiento(id, false) ?? new List<Entidades.CortePorMovimiento>();

            var model = new MovimientoDetalleVm
            {
                IdMovimiento = id,
                Observaciones = movimiento != null ? movimiento.Observaciones ?? "" : "",
                Creado = movimiento != null ? movimiento.Creado : null,
                CreadoPor = movimiento != null && movimiento.CreadoPor != null ? movimiento.CreadoPor.Nombre ?? "" : "",
                Actualizado = movimiento != null ? movimiento.Actualizado : null,
                ActualizadoPor = movimiento != null && movimiento.ActualizadoPor != null ? movimiento.ActualizadoPor.Nombre ?? "" : "",
                Lineas = lineas.Select(MapLinea).ToList()
            };

            return PartialView("_MovimientoDetalle", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Guardar(MovimientoEditVm model, int idUsuarioCreador = 0)
        {
            try
            {
                var user = _usuarioActual;

                // Port de Web/Controllers/MovimientosController.cs:341-347: el chequeo de
                // permiso usa SIEMPRE el usuario de sesion real (asi funciona SoloRegistrosPropios
                // sin cambios); el creador que efectivamente queda grabado se resuelve DESPUES,
                // con el id elegido en la pantalla de SeleccionUsuario (cuenta de produccion).
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Movimiento.NuevoMovimiento, model?.FechaMovimiento ?? DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para guardar movimientos." });

                user = ResolverUsuarioCreador(idUsuarioCreador, user);

                NormalizarDecimalesPosteados(model);

                string error = ValidarModelo(model);
                if (!string.IsNullOrWhiteSpace(error))
                    return Json(new { ok = false, mensaje = error });

                var entidad = model.IdMovimiento > 0 ? _oCorteN.cargarMovimiento(model.IdMovimiento, false) : new Entidades.Movimiento();
                if (entidad == null)
                    entidad = new Entidades.Movimiento();

                entidad.FechaMovimiento = model.FechaMovimiento;
                entidad.SucursalOrigen = new Entidades.Sucursal { IdSucursal = model.IdSucursalOrigen };
                entidad.SucursalDestino = new Entidades.Sucursal { IdSucursal = model.IdSucursalDestino };
                entidad.Observaciones = model.Observaciones ?? "";

                if (model.IdMovimiento > 0)
                {
                    entidad.IdMovimiento = model.IdMovimiento;
                    entidad.ActualizadoPor = user;
                    if (entidad.CreadoPor == null)
                        entidad.CreadoPor = user;
                }
                else
                {
                    entidad.CreadoPor = user;
                }

                int idMovimiento = _oCorteN.addOrEditMovimiento(entidad);
                entidad.IdMovimiento = idMovimiento;

                foreach (var linea in model.Lineas ?? new List<MovimientoLineaVm>())
                {
                    var item = new Entidades.CortePorMovimiento
                    {
                        Movimientos = entidad,
                        Corte = new Entidades.Corte { IdCorte = linea.IdCorte },
                        CantUnidad = linea.CantUnidad,
                        CantKg = linea.CantKg,
                        PesoBalanza = linea.PesoBalanza,
                        PermitirIngreso = linea.PermitirIngreso
                    };

                    _oCorteN.agregarCortePorMovimiento(item);
                }

                return Json(new
                {
                    ok = true,
                    movimientoId = idMovimiento,
                    mensaje = model.IdMovimiento > 0 ? "El movimiento se guardó correctamente." : "El movimiento se registró correctamente.",
                    redirectUrl = Url.Action("Index", "Movimientos"),
                    pdfUrl = Url.Action("ImprimirPdf", "Movimientos", new { id = idMovimiento }),
                    whatsappTexto = ConstruirMensajeWhatsapp(idMovimiento)
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ImprimirPdf(int id)
        {
            var movimiento = _oCorteN.cargarMovimiento(id, true);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return NotFound();

            var lineas = _oCorteN.cargarCortesPorMovimiento(id, true) ?? new List<Entidades.CortePorMovimiento>();
            byte[] bytes = WebCore.Services.GenerarDocsCore.GenerarPdfMovimiento(movimiento, lineas);
            return File(bytes, "application/pdf", "Movimiento_" + id + ".pdf");
        }

        private MovimientoEditVm CrearModeloNuevo(Entidades.Usuario user)
        {
            int idSucursalActual = user != null ? user.IdSucursal : 0;
            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var model = new MovimientoEditVm
            {
                IdMovimiento = 0,
                EsEdicion = false,
                EmpresaCuit = ObtenerEmpresaCuit(user),
                MostrarColumnasInternas = ObtenerEmpresaCuit(user) == CuitColumnasInternas,
                FechaMovimiento = DateTime.Now,
                UsuarioNombre = user != null ? user.Nombre : "",
                IdSucursalOrigen = idSucursalActual
            };

            if (sucursales.Count == 2 && idSucursalActual > 0)
            {
                var destino = sucursales.FirstOrDefault(s => s.IdSucursal != idSucursalActual);
                if (destino != null)
                    model.IdSucursalDestino = destino.IdSucursal;
            }

            return model;
        }

        private MovimientoEditVm CrearModeloEdicion(int id, Entidades.Usuario user)
        {
            var movimiento = _oCorteN.cargarMovimiento(id, false);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return null;

            var lineas = _oCorteN.cargarCortesPorMovimiento(id, false) ?? new List<Entidades.CortePorMovimiento>();

            return new MovimientoEditVm
            {
                IdMovimiento = movimiento.IdMovimiento,
                EsEdicion = true,
                EmpresaCuit = ObtenerEmpresaCuit(user),
                MostrarColumnasInternas = ObtenerEmpresaCuit(user) == CuitColumnasInternas,
                IdSucursalOrigen = movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.IdSucursal : 0,
                IdSucursalDestino = movimiento.SucursalDestino != null ? movimiento.SucursalDestino.IdSucursal : 0,
                FechaMovimiento = movimiento.FechaMovimiento,
                Observaciones = movimiento.Observaciones ?? "",
                UsuarioNombre = user != null ? user.Nombre : "",
                Creado = movimiento.Creado.HasValue ? FormatearFechaHora(movimiento.Creado.Value) : "-",
                CreadoPor = movimiento.CreadoPor != null ? movimiento.CreadoPor.Nombre : "-",
                Actualizado = movimiento.Actualizado.HasValue ? FormatearFechaHora(movimiento.Actualizado.Value) : "-",
                ActualizadoPor = movimiento.ActualizadoPor != null ? movimiento.ActualizadoPor.Nombre : "-",
                IdMovimientoOrigen = movimiento.IdMovOrigen.HasValue && movimiento.IdMovOrigen.Value > 0 ? movimiento.IdMovOrigen.Value.ToString() : movimiento.IdMovimiento.ToString(),
                IdMovimientoDestino = movimiento.IdMovOrigen.HasValue && movimiento.IdMovOrigen.Value > 0 ? movimiento.IdMovimiento.ToString() : "-",
                Lineas = lineas.Select(MapLinea).ToList()
            };
        }

        private List<MovimientoResumenVm> ConstruirResumenes(DataTable dt)
        {
            var lista = new List<MovimientoResumenVm>();
            if (dt == null)
                return lista;

            var filas = dt.Rows.Cast<DataRow>().ToList();
            var idsConTotalesFaltantes = filas
                .Select(row => new
                {
                    IdMovimiento = ToInt(row, "Id Movimiento", "idMovimiento", "movimiento"),
                    TotalUnidad = ObtenerDecimalFila(row, "totalUnidad", "cantUnidad", "Cant Prod.", "cantProd", "kgCorte"),
                    TotalKilos = ObtenerDecimalFila(row, "totalKilos", "cantKg", "cantKgs", "kg", "kgCorte")
                })
                .Where(x => x.IdMovimiento > 0 && (x.TotalUnidad == 0m || x.TotalKilos == 0m))
                .Select(x => x.IdMovimiento)
                .Distinct()
                .ToList();

            var totalesPorMovimiento = _oCorteN.ObtenerTotalesPorMovimiento(idsConTotalesFaltantes);

            foreach (DataRow row in filas)
            {
                int idMovimiento = ToInt(row, "Id Movimiento", "idMovimiento", "movimiento");
                decimal totalUnidad = ObtenerDecimalFila(row, "totalUnidad", "cantUnidad", "Cant Prod.", "cantProd", "kgCorte");
                decimal totalKilos = ObtenerDecimalFila(row, "totalKilos", "cantKg", "cantKgs", "kg", "kgCorte");

                if (idMovimiento > 0 && (totalUnidad == 0m || totalKilos == 0m) && totalesPorMovimiento.ContainsKey(idMovimiento))
                {
                    var totales = totalesPorMovimiento[idMovimiento];
                    if (totalUnidad == 0m)
                        totalUnidad = totales.Item1;
                    if (totalKilos == 0m)
                        totalKilos = totales.Item2;
                }

                string observaciones = ToString(row, "observaciones", "Observaciones", "obs");

                lista.Add(new MovimientoResumenVm
                {
                    IdMovimiento = idMovimiento,
                    FechaMovimiento = ToDateTime(row, "Fecha Movimiento", "fechaMovimiento"),
                    Origen = ToString(row, "Origen", "sucursalOrigen", "origen"),
                    Destino = ToString(row, "Destino", "sucursalDestino", "destino"),
                    DeOrigen = ToString(row, "Id Origen", "idMovOrigen", "deOrigen"),
                    Estado = ToString(row, "Estado", "estado"),
                    TotalUnidad = totalUnidad,
                    TotalKilos = totalKilos,
                    Observaciones = observaciones,
                    TieneObservaciones = !string.IsNullOrWhiteSpace(observaciones)
                });
            }

            return lista.OrderByDescending(x => x.FechaMovimiento).ToList();
        }

        private List<Entidades.Sucursal> ConstruirSucursalesConTodas(List<Entidades.Sucursal> sucursales)
        {
            var lista = new List<Entidades.Sucursal>
            {
                new Entidades.Sucursal { IdSucursal = 0, SucursalNombre = "Todas" }
            };

            if (sucursales != null)
                lista.AddRange(sucursales);

            return lista;
        }

        private string ObtenerNombreSucursalFiltro(List<Entidades.Sucursal> sucursales, int idSucursal)
        {
            if (idSucursal <= 0 || sucursales == null)
                return "";

            var sucursal = sucursales.FirstOrDefault(s => s.IdSucursal == idSucursal);
            return sucursal != null ? sucursal.SucursalNombre : "";
        }

        private long ObtenerEmpresaCuit(Entidades.Usuario user)
        {
            return user != null && user.Empresa != null ? user.Empresa.Cuit : 0;
        }

        private string ValidarModelo(MovimientoEditVm model)
        {
            if (model.IdSucursalOrigen <= 0 || model.IdSucursalDestino <= 0)
                return "Debe seleccionar la sucursal origen y destino.";

            if (model.IdSucursalOrigen == model.IdSucursalDestino)
                return "La sucursal origen y destino deben ser diferentes.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe agregar al menos un producto al movimiento.";

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea.IdCorte <= 0)
                    return "La línea " + (i + 1) + " no tiene producto válido.";

                if (linea.CantUnidad < 0)
                    return "La línea " + (i + 1) + " debe tener una cantidad mayor o igual a cero.";

                if (linea.CantKg <= 0)
                    return "La línea " + (i + 1) + " debe tener kilos mayores a cero.";
            }

            return "";
        }

        private static bool CoincideProductoMovimiento(string codigo, string descripcion, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return true;

            string texto = filtro.Trim();
            return (codigo ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || (descripcion ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ConstruirObservacionLineaMovimiento(Entidades.CortePorMovimiento linea)
        {
            var partes = new List<string>();
            if (linea != null && linea.PesoBalanza)
                partes.Add("Peso balanza");
            if (linea != null && linea.PermitirIngreso)
                partes.Add("Permite ingreso");

            return partes.Count == 0 ? "-" : string.Join(" | ", partes);
        }

        private MovimientoLineaVm MapLinea(Entidades.CortePorMovimiento linea)
        {
            return new MovimientoLineaVm
            {
                IdCorteMovimiento = linea.IdCorteMovimiento,
                IdCorte = linea.Corte != null ? linea.Corte.IdCorte : 0,
                Codigo = linea.Corte != null ? linea.Corte.Codigo : 0,
                Producto = linea.Corte != null ? linea.Corte.CorteDesc : "",
                TipoProducto = linea.Corte != null ? linea.Corte.Tipo : "",
                Pesable = linea.Corte != null && linea.Corte.Pesable,
                PromedioProducto = linea.Corte != null ? linea.Corte.Promedio : 0,
                CantUnidad = linea.CantUnidad,
                CantKg = linea.CantKg,
                PesoBalanza = linea.PesoBalanza,
                PermitirIngreso = linea.PermitirIngreso
            };
        }

        private void NormalizarDecimalesPosteados(MovimientoEditVm model)
        {
            if (model == null || model.Lineas == null || Request == null || Request.Form == null)
                return;

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea == null)
                    continue;

                float valorFloat;
                string keyCantKg = "Lineas[" + i + "].CantKg";
                if (TryParseFloatFlexible(Request.Form[keyCantKg], out valorFloat))
                {
                    linea.CantKg = valorFloat;
                    ModelState.Remove(keyCantKg);
                }

                string keyPromedio = "Lineas[" + i + "].PromedioProducto";
                if (TryParseFloatFlexible(Request.Form[keyPromedio], out valorFloat))
                {
                    linea.PromedioProducto = valorFloat;
                    ModelState.Remove(keyPromedio);
                }
            }
        }

        private static bool TryParseFloatFlexible(string raw, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim();

            if (float.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;

            if (float.TryParse(raw.Replace(".", ","), NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out value))
                return true;

            if (float.TryParse(raw.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;

            return false;
        }

        private string ConstruirMensajeWhatsapp(int idMovimiento)
        {
            var movimiento = _oCorteN.cargarMovimiento(idMovimiento, true);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return "Se registró un movimiento.";

            var lineas = _oCorteN.cargarCortesPorMovimiento(idMovimiento, true) ?? new List<Entidades.CortePorMovimiento>();
            int totalUnidades = lineas.Sum(x => x.CantUnidad);
            float totalKilos = lineas.Sum(x => x.CantKg);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Movimiento {0}%0AOrigen: {1}%0ADestino: {2}%0AFecha: {3}%0ATotal unidades: {4}%0ATotal kilos: {5:F3}",
                movimiento.IdMovimiento,
                movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-",
                movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-",
                movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"),
                totalUnidades,
                totalKilos);
        }

        // Trivial, mismo criterio que StockController -- deja la puerta abierta a un login real
        // futuro sin volver a tocar este archivo (con el stub, EsUsuarioProduccion=false siempre
        // devuelve usuarioSesion sin cambios).
        private Entidades.Usuario ResolverUsuarioCreador(int idUsuarioCreador, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion || idUsuarioCreador <= 0)
                return usuarioSesion;

            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            var candidato = oUsuarioN.getUsuarioById(idUsuarioCreador);
            if (candidato == null || !candidato.Activo || candidato.IdEmpresa != usuarioSesion.IdEmpresa)
                return usuarioSesion;

            return candidato;
        }

        private List<object> ObtenerUsuariosActivosEmpresaParaCombo()
        {
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            var dt = oUsuarioN.obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre"))
                return new List<object>();

            return dt.AsEnumerable()
                .Select(row => new { id = ValorInt(row, "id"), nombre = ValorString(row, "nombre") })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }

        private static int ValorInt(DataRow row, string columna)
        {
            return row.Table.Columns.Contains(columna) && int.TryParse(Convert.ToString(row[columna]), out int value) ? value : 0;
        }

        private static string ValorString(DataRow row, string columna)
        {
            return row.Table.Columns.Contains(columna) ? Convert.ToString(row[columna]) ?? "" : "";
        }

        private int ToInt(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (row.Table.Columns.Contains(name))
                {
                    int value;
                    if (int.TryParse(Convert.ToString(row[name]), out value))
                        return value;
                }
            }

            return 0;
        }

        private DateTime ToDateTime(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (row.Table.Columns.Contains(name))
                {
                    DateTime value;
                    if (DateTime.TryParse(Convert.ToString(row[name]), out value))
                        return value;
                }
            }

            return DateTime.MinValue;
        }

        private string ToString(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (row.Table.Columns.Contains(name))
                    return Convert.ToString(row[name]) ?? "";
            }

            return "";
        }

        private decimal ObtenerDecimalFila(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (!row.Table.Columns.Contains(name))
                    continue;

                decimal value;
                string raw = Convert.ToString(row[name]) ?? "";
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                    return value;

                if (decimal.TryParse(raw.Replace(".", ","), NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out value))
                    return value;

                if (decimal.TryParse(raw.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    return value;
            }

            return 0m;
        }

        private string FormatearFechaHora(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
