// Port de Web/Controllers/ElaboradosController.cs (1859 lineas, 17 acciones) -- catalogo de
// elaborados/embutidos (carga manual, ingreso rapido con formula, formulas/recetas, desarme).
// Portado completo: Index/Lineas/Detalle (listados), Formulas/EditarFormula/GuardarFormula/
// EliminarFormula (CRUD de formulas), IngresoRapido/Desarme/EditarIngresoRapido/
// GuardarIngresoRapido (ingreso con formula automatica), Carga/GuardarCarga (carga manual),
// BuscarProducto/BuscarProductoPorCodigo/ObtenerFormula (autocompletado), Anular.
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06) -- ya no hay stub hardcodeado. PermisosHelper.TienePermiso se omite por completo --
// TODO(claude): revisar en Batch 4/5. El gate de "usuario de sala de produccion"
// (SeleccionUsuario) todavia no se porta (Batch 5) -- ResolverUsuarioCreador() SI se porta
// (trivial, ya fiel al original).
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class ElaboradosController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Sucursal _oSucursalN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public ElaboradosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
        }

        public IActionResult Index(int? idSucursal = null, string elaborado = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = _usuarioActual;
            DateTime fechaLimiteSinPermiso = DateTime.Today.AddDays(-_param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime desde = NormalizarFechaDesde(fechaDesde ?? fechaLimiteSinPermiso);
            DateTime hasta = NormalizarFechaHasta(fechaHasta ?? DateTime.Today);

            int sucursalSeleccionada = idSucursal ?? (user.IdSucursal > 0 ? user.IdSucursal : 0);
            DataTable dt = _oCorteN.buscarEmbutido(sucursalSeleccionada > 0 ? sucursalSeleccionada : -1, (elaborado ?? "").Trim(), desde, hasta) ?? new DataTable();

            var model = new ElaboradoIndexVm
            {
                IdSucursal = sucursalSeleccionada,
                Elaborado = (elaborado ?? "").Trim(),
                FechaDesde = desde,
                FechaHasta = hasta,
                Items = MapElaborados(dt),
                Tabs = BuildTabs("Index")
            };

            model.TotalKg = model.Items
                .Where(x => string.IsNullOrWhiteSpace(x.Estado))
                .Sum(x => x.Kgs);

            ViewBag.Title = "Elaborados";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(_oSucursalN.findAll() ?? new List<Entidades.Sucursal>());

            return View(model);
        }

        [HttpGet]
        public IActionResult Detalle(int id = 0)
        {
            if (id <= 0)
                return NotFound("No se encontró el elaborado.");

            var detalle = ConstruirDetalleElaborado(id);
            if (detalle == null)
                return NotFound("No se encontraron los detalles del elaborado.");

            return PartialView("_DetalleElaborado", detalle);
        }

        public IActionResult Lineas(int? idSucursal = null, string descripcion = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = _usuarioActual;
            DateTime fechaLimiteSinPermiso = DateTime.Today.AddDays(-_param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime desde = NormalizarFechaDesde(fechaDesde ?? fechaLimiteSinPermiso);
            DateTime hasta = NormalizarFechaHasta(fechaHasta ?? DateTime.Today);

            int sucursalSeleccionada = idSucursal ?? (user.IdSucursal > 0 ? user.IdSucursal : 0);
            DataTable dt = _oCorteN.obtenerLineasEmb(sucursalSeleccionada > 0 ? sucursalSeleccionada : -1, (descripcion ?? "").Trim(), desde, hasta) ?? new DataTable();

            var model = new ElaboradoLineasIndexVm
            {
                IdSucursal = sucursalSeleccionada,
                Descripcion = (descripcion ?? "").Trim(),
                FechaDesde = desde,
                FechaHasta = hasta,
                Items = MapLineas(dt),
                Tabs = BuildTabs("Lineas")
            };

            model.TotalKg = model.Items
                .Where(x => string.IsNullOrWhiteSpace(x.Estado))
                .Sum(x => x.Kgs);

            ViewBag.Title = "Lineas de elaborado";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(_oSucursalN.findAll() ?? new List<Entidades.Sucursal>());

            return View(model);
        }

        public IActionResult Formulas(string descripcion = "")
        {
            var user = _usuarioActual;
            DataTable dt = _oCorteN.buscarFormula((descripcion ?? "").Trim()) ?? new DataTable();
            bool puedeEditar = true;
            var model = new ElaboradoFormulasIndexVm
            {
                Descripcion = (descripcion ?? "").Trim(),
                Items = MapFormulas(dt),
                Detalles = ConstruirDetallesFormulas(dt),
                PuedeCrear = puedeEditar,
                PuedeEditar = puedeEditar,
                PuedeEliminar = puedeEditar,
                Tabs = BuildTabs("Formulas")
            };

            ViewBag.Title = "Formulas";

            return View(model);
        }

        public IActionResult EditarFormula(int id = 0)
        {
            var user = _usuarioActual;

            ElaboradoFormulaEditVm model;
            if (id > 0)
            {
                var formula = _oCorteN.findFormulaByID(id, 0);
                if (formula == null || formula.IdFormula <= 0)
                    return NotFound("No se encontró la fórmula.");

                model = CrearViewModelFormulaEdicion(formula, user);
            }
            else
            {
                var productoGenerico = _oCorteN.ObtenerProductoAjusteFormula();
                model = new ElaboradoFormulaEditVm
                {
                    UsuarioNombre = user.Nombre ?? "",
                    EtiquetaValorFormula = "Porcentaje",
                    EscalaUnidad = false,
                    CodigoProductoGenerico = productoGenerico != null ? productoGenerico.Codigo : 0,
                    NombreProductoGenerico = productoGenerico != null ? (!string.IsNullOrWhiteSpace(productoGenerico.CorteDesc) ? productoGenerico.CorteDesc : productoGenerico.corte) : "",
                    Tabs = BuildTabs("Formulas")
                };
            }

            ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
            return View(model);
        }

        public IActionResult IngresoRapido()
        {
            return VistaIngresoRapido(false);
        }

        public IActionResult Desarme()
        {
            return VistaIngresoRapido(true);
        }

        public IActionResult EditarIngresoRapido(int idElaborado = 0, int id = 0, bool esDesarme = false, int idUsuarioCreador = 0)
        {
            var user = _usuarioActual;
            int idCreadorPermiso = user.Id;

            if (id > 0)
            {
                var embutido = _oCorteN.findEmbutidoById(id);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return NotFound("No se encontró el elaborado.");

                bool puedeModificar = true;
                idCreadorPermiso = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                var modelEdicion = CrearViewModelIngresoRapidoEdicion(embutido, user, esDesarme);
                if (modelEdicion == null)
                    return NotFound("No se encontró el elaborado.");

                modelEdicion.SoloLecturaInicial = !string.Equals(modelEdicion.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase);
                modelEdicion.PuedeHabilitarEdicion = puedeModificar;

                ViewBag.Title = modelEdicion.EsDesarme ? "Desarme de elaborado" : "Ingreso rápido";
                ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
                ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
                ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
                ViewBag.IdUsuarioCreadorPreseleccionado = 0;
                return View(modelEdicion);
            }

            var corte = _oCorteN.findCorteById(idElaborado, false);
            if (corte == null || corte.IdCorte <= 0)
                return NotFound("No se encontró el elaborado.");

            var formula = _oCorteN.findFormulaByID(0, corte.IdCorte);
            var formulaItems = MapFormula(_oCorteN.getFormulaEmbutido(corte.IdCorte) ?? new DataTable());
            if (formulaItems.Count == 0)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Elaborados";
                TempData["AlertMsg"] = "El producto seleccionado no tiene fórmula cargada para ingreso rápido.";
                return RedirectToAction(esDesarme ? "Desarme" : "IngresoRapido");
            }

            var model = new ElaboradoRapidoEditVm
            {
                EsDesarme = esDesarme,
                IdSucursal = user.IdSucursal > 0 ? user.IdSucursal : 0,
                FechaEmbutido = DateTime.Now,
                UsuarioNombre = user.Nombre ?? "",
                IdElaborado = corte.IdCorte,
                CodigoElaborado = corte.Codigo,
                Elaborado = !string.IsNullOrWhiteSpace(corte.CorteDesc) ? corte.CorteDesc : corte.corte,
                Receta = formula != null ? (formula.Receta ?? "") : "",
                EsPesableElaborado = corte.Pesable,
                Formula = formulaItems,
                Tabs = BuildTabs(esDesarme ? "Desarme" : "IngresoRapido"),
                PuedeHabilitarEdicion = true
            };

            RecalcularFormulaRapida(model);

            ViewBag.Title = esDesarme ? "Desarme de elaborado" : "Ingreso rápido";
            ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.IdUsuarioCreadorPreseleccionado = 0;
            return View(model);
        }

        public IActionResult Carga(int id = 0, int idUsuarioCreador = 0)
        {
            var user = _usuarioActual;
            int idCreadorPermiso = user.Id;

            ElaboradoCargaVm model;
            if (id > 0)
            {
                var embutido = _oCorteN.findEmbutidoById(id);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return NotFound("No se encontró el elaborado.");

                bool puedeModificar = true;
                idCreadorPermiso = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                model = CrearViewModelEdicion(embutido, user);
                model.SoloLecturaInicial = !string.Equals(model.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase);
                model.PuedeHabilitarEdicion = puedeModificar;
                model.PermiteGuardarEdicion = puedeModificar;
            }
            else
            {
                model = new ElaboradoCargaVm
                {
                    IdSucursal = user.IdSucursal > 0 ? user.IdSucursal : 0,
                    FechaEmbutido = DateTime.Now,
                    UsuarioNombre = user.Nombre ?? "",
                    Tabs = BuildTabs("Carga"),
                    PermiteGuardarEdicion = true,
                    PuedeHabilitarEdicion = true
                };
            }

            ViewBag.Title = model.EsEdicion ? "Modificar elaborado" : "Carga / ingreso de elaborado";
            ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.IdUsuarioCreadorPreseleccionado = 0;

            return View(model);
        }

        [HttpGet]
        public JsonResult BuscarProducto(string q = "")
        {
            try
            {
                int idEmpresaSesion = ObtenerIdEmpresaSesionActual();
                if (idEmpresaSesion <= 0)
                    return Json(new List<object>());

                var productos = _oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    string filtro = q.Trim();
                    productos = productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.corte) && p.corte.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrWhiteSpace(p.CorteDesc) && p.CorteDesc.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        p.codigo.ToString().IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                var resultado = productos.Take(200).Select(MapProductoBusqueda).ToList();
                return Json(resultado);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public JsonResult BuscarProductoPorCodigo(long? codigo, bool permitirCodigoNegativo = false, int idFormulaActual = 0)
        {
            bool codigoInvalido = !codigo.HasValue || codigo.Value == 0 || (codigo.Value < 0 && !permitirCodigoNegativo);
            if (codigoInvalido)
                return Json(new { ok = false, mensaje = "Codigo invalido." });

            int idEmpresaSesion = ObtenerIdEmpresaSesionActual();
            if (idEmpresaSesion <= 0)
                return Json(new { ok = false, mensaje = "No se encontro el producto." });

            var corte = _oCorteN.findCorteByCodigoEmpresa(codigo!.Value, idEmpresaSesion, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "No se encontro el producto." });

            var formulaExistente = _oCorteN.findFormulaByID(0, corte.IdCorte);
            bool tieneOtraFormula = formulaExistente != null && formulaExistente.IdFormula > 0 && formulaExistente.IdFormula != idFormulaActual;

            return Json(new
            {
                ok = true,
                id = corte.IdCorte,
                codigo = corte.Codigo,
                nombre = !string.IsNullOrWhiteSpace(corte.CorteDesc) ? corte.CorteDesc : corte.corte,
                tipo = corte.Tipo ?? "",
                promedio = corte.Promedio,
                ingresoRapido = corte.IngresoRapidoEmbutido,
                pesable = corte.Pesable,
                tieneFormula = tieneOtraFormula
            });
        }

        [HttpGet]
        public JsonResult ObtenerFormula(int idCorte)
        {
            try
            {
                var formula = _oCorteN.findFormulaByID(0, idCorte);
                var dtFormula = _oCorteN.getFormulaEmbutido(idCorte) ?? new DataTable();

                var items = new List<ElaboradoFormulaLineaVm>();
                foreach (DataRow row in dtFormula.Rows)
                {
                    items.Add(new ElaboradoFormulaLineaVm
                    {
                        IdCorte = ToInt(row, "idCorte", "IdCorte"),
                        Codigo = ToLong(row, "codigo", "Codigo"),
                        Producto = ToString(row, "corte", "Corte"),
                        Porcentaje = ToFloat(row, "porcentaje", "Porcentaje"),
                        AgregarAuto = ToBool(row, "agregarAuto", "AgregarAuto"),
                        Kgs = 0f
                    });
                }

                return Json(new
                {
                    ok = true,
                    receta = formula != null ? (formula.Receta ?? "") : "",
                    tieneFormula = items.Count > 0,
                    formula = items
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarFormula(ElaboradoFormulaEditVm model)
        {
            var user = _usuarioActual;
            int idCreador = user.Id;
            Entidades.Formula formulaActual = null;
            if (model != null && model.IdFormula > 0)
            {
                formulaActual = _oCorteN.findFormulaByID(model.IdFormula, 0);
                if (formulaActual == null || formulaActual.IdFormula <= 0)
                {
                    TempData["AlertType"] = "error";
                    TempData["AlertTitle"] = "No encontrada";
                    TempData["AlertMsg"] = "No se encontró la fórmula a modificar.";
                    return RedirectToAction("Formulas");
                }

                if (formulaActual.CreadoPor != null)
                    idCreador = formulaActual.CreadoPor.Id;
            }

            string error = ValidarFormula(model);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError("", error);
                if (model == null) model = new ElaboradoFormulaEditVm();
                model.Tabs = BuildTabs("Formulas");
                model.UsuarioNombre = user.Nombre ?? "";
                PrepararMetadataFormulaModel(model);
                RecalcularTotalesFormula(model);
                ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                return View("EditarFormula", model);
            }

            try
            {
                var embutido = _oCorteN.findCorteById(model.IdElaborado, false);
                int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
                if (embutido == null || embutido.IdCorte <= 0 || (idEmpresaSesion > 0 && embutido.IdEmpresa != idEmpresaSesion))
                {
                    ModelState.AddModelError("", "No se encontró el elaborado seleccionado.");
                    model.Tabs = BuildTabs("Formulas");
                    model.UsuarioNombre = user.Nombre ?? "";
                    PrepararMetadataFormulaModel(model);
                    RecalcularTotalesFormula(model);
                    ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                    return View("EditarFormula", model);
                }

                var existenteMismoElaborado = _oCorteN.findFormulaByID(0, model.IdElaborado);
                if (existenteMismoElaborado != null && existenteMismoElaborado.IdFormula > 0 && existenteMismoElaborado.IdFormula != model.IdFormula)
                {
                    ModelState.AddModelError("", "El elaborado ya posee una fórmula. Modifique la existente.");
                    model.Tabs = BuildTabs("Formulas");
                    model.UsuarioNombre = user.Nombre ?? "";
                    PrepararMetadataFormulaModel(model);
                    RecalcularTotalesFormula(model);
                    ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                    return View("EditarFormula", model);
                }

                if (embutido.IngresoRapidoEmbutido != model.EsIngresoRapidoElaborado)
                {
                    embutido.IngresoRapidoEmbutido = model.EsIngresoRapidoElaborado;
                    _oCorteN.addOrEditCorte(embutido);
                }

                var formula = formulaActual ?? new Entidades.Formula();
                formula.IdFormula = model.IdFormula;
                formula.Embutido = embutido;
                formula.Receta = (model.Receta ?? "").Trim();
                formula.CreadoPor = formulaActual != null ? formulaActual.CreadoPor : user;
                formula.ActualizadoPor = formulaActual != null ? user : null;
                formula.AjustarUnidad = model.AjustarUnidad;

                var lineasVisuales = new List<Entidades.CortePorFormula>();
                foreach (var linea in model.Lineas ?? new List<ElaboradoFormulaEditLineaVm>())
                {
                    var corte = _oCorteN.findCorteById(linea.IdCorte, false);
                    if (corte == null || corte.IdCorte <= 0 || (idEmpresaSesion > 0 && corte.IdEmpresa != idEmpresaSesion))
                    {
                        ModelState.AddModelError("", "No se encontró el ingrediente " + (linea.Producto ?? "") + ".");
                        model.Tabs = BuildTabs("Formulas");
                        model.UsuarioNombre = user.Nombre ?? "";
                        PrepararMetadataFormulaModel(model);
                        RecalcularTotalesFormula(model);
                        ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                        return View("EditarFormula", model);
                    }

                    lineasVisuales.Add(new Entidades.CortePorFormula
                    {
                        Formula = formula,
                        CorteEnFormula = corte,
                        Porcentaje = linea.Porcentaje,
                        AgregarAuto = linea.AgregarAuto,
                        NoSumaPeso = linea.NoSumaPeso
                    });
                }

                var lineas = _oCorteN.NormalizarFormulaElaborado(embutido, formula, lineasVisuales, model.EscalaUnidad);
                formula.IdFormula = _oCorteN.addOrEditFormula(formula, lineas);
                var mensaje = model.IdFormula > 0 ? "La fórmula se guardó correctamente." : "La fórmula se registró correctamente.";
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Elaborados";
                TempData["AlertMsg"] = mensaje;
                return RedirectToAction("Formulas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No se pudo guardar la fórmula. " + ex.Message);
                model.Tabs = BuildTabs("Formulas");
                model.UsuarioNombre = user.Nombre ?? "";
                PrepararMetadataFormulaModel(model);
                RecalcularTotalesFormula(model);
                ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                return View("EditarFormula", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarFormula(int idFormula)
        {
            try
            {
                var formula = _oCorteN.findFormulaByID(idFormula, 0);
                if (formula == null || formula.IdFormula <= 0)
                    return Json(new { ok = false, mensaje = "No se encontró la fórmula." });

                _oCorteN.eliminarFormula(idFormula);
                return Json(new { ok = true, redirectUrl = Url.Action("Formulas", "Elaborados") });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarIngresoRapido(ElaboradoRapidoEditVm model, int idUsuarioCreador = 0)
        {
            try
            {
                var user = _usuarioActual;
                var usuarioCreador = ResolverUsuarioCreador(idUsuarioCreador, user);

                string error = ValidarIngresoRapido(model);
                if (!string.IsNullOrWhiteSpace(error))
                    return Json(new { ok = false, mensaje = error });

                Entidades.Embutido embutidoOriginal = null;
                if (model.IdEmbutido > 0)
                {
                    embutidoOriginal = _oCorteN.findEmbutidoById(model.IdEmbutido);
                    if (embutidoOriginal == null || embutidoOriginal.IdEmbutido <= 0)
                        return Json(new { ok = false, mensaje = "No se encontro el elaborado original a modificar." });
                }

                var elaborado = _oCorteN.findCorteById(model.IdElaborado, false);
                int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
                if (elaborado == null || elaborado.IdCorte <= 0 || (idEmpresaSesion > 0 && elaborado.IdEmpresa != idEmpresaSesion))
                    return Json(new { ok = false, mensaje = "No se encontró el elaborado seleccionado." });

                if (!elaborado.IngresoRapidoEmbutido)
                    return Json(new { ok = false, mensaje = "El producto seleccionado no esta configurado para ingreso rapido." });

                var formula = MapFormula(_oCorteN.getFormulaEmbutido(model.IdElaborado) ?? new DataTable());
                if (formula.Count == 0)
                    return Json(new { ok = false, mensaje = "El elaborado no tiene fórmula cargada." });

                model.Formula = formula;
                RecalcularFormulaRapida(model);

                if (embutidoOriginal != null)
                {
                    if (string.Equals(embutidoOriginal.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase))
                        return Json(new { ok = false, mensaje = "El elaborado original ya se encuentra anulado." });

                    embutidoOriginal.ActualizadoPor = usuarioCreador;
                    _oCorteN.anularEmbutido(embutidoOriginal);
                }

                var embutido = new Entidades.Embutido
                {
                    fechaEmbutido = model.FechaEmbutido,
                    corte = elaborado,
                    sucursal = new Entidades.Sucursal { IdSucursal = model.IdSucursal },
                    observaciones = model.EsDesarme ? "Desarme" : "",
                    CreadoPor = usuarioCreador
                };

                embutido.idEmbutido = _oCorteN.agregarEmbutido(embutido);

                foreach (var item in model.Formula)
                {
                    var kg = item.Kgs;
                    if (kg == 0f)
                        continue;

                    _oCorteN.agregarCortePorEmbutido(new Entidades.CortePorEmbutido
                    {
                        Embutido = embutido,
                        Corte = new Entidades.Corte { IdCorte = item.IdCorte },
                        KgUtilizado = kg,
                        PesoBalanza = false
                    });
                }

                float cantidad = model.EsDesarme ? Math.Abs(model.Cantidad) : model.Cantidad;
                string unidad = elaborado.Pesable ? "kgs" : "unidades";
                string mensajeExito = model.EsDesarme
                    ? "Se registró correctamente el desarme de " + FormatearCantidadMensaje(cantidad, elaborado.Pesable) + " " + unidad + " de " + (!string.IsNullOrWhiteSpace(elaborado.CorteDesc) ? elaborado.CorteDesc : elaborado.corte) + "."
                    : "Se guardó correctamente " + FormatearCantidadMensaje(cantidad, elaborado.Pesable) + " " + unidad + " de " + (!string.IsNullOrWhiteSpace(elaborado.CorteDesc) ? elaborado.CorteDesc : elaborado.corte) + ".";
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Elaborados";
                TempData["AlertMsg"] = mensajeExito;

                return Json(new { ok = true, redirectUrl = Url.Action("Index", "Elaborados") });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarCarga(ElaboradoCargaVm model, int idUsuarioCreador = 0)
        {
            try
            {
                var user = _usuarioActual;
                var usuarioCreador = ResolverUsuarioCreador(idUsuarioCreador, user);

                if (model == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos del elaborado." });

                string error = ValidarCarga(model);
                if (!string.IsNullOrWhiteSpace(error))
                    return Json(new { ok = false, mensaje = error });

                Entidades.Embutido embutidoOriginal = null;
                if (model.IdEmbutido > 0)
                {
                    embutidoOriginal = _oCorteN.findEmbutidoById(model.IdEmbutido);
                    if (embutidoOriginal == null || embutidoOriginal.IdEmbutido <= 0)
                        return Json(new { ok = false, mensaje = "No se encontro el elaborado original a modificar." });
                }

                var corteElaborado = _oCorteN.findCorteById(model.IdElaborado, false);
                int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
                if (corteElaborado == null || corteElaborado.IdCorte <= 0 || (idEmpresaSesion > 0 && corteElaborado.IdEmpresa != idEmpresaSesion))
                    return Json(new { ok = false, mensaje = "El elaborado seleccionado no es valido." });

                if (embutidoOriginal != null)
                {
                    if (string.Equals(embutidoOriginal.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase))
                        return Json(new { ok = false, mensaje = "El elaborado original ya se encuentra anulado." });

                    embutidoOriginal.ActualizadoPor = usuarioCreador;
                    _oCorteN.anularEmbutido(embutidoOriginal);
                }

                var embutido = new Entidades.Embutido
                {
                    FechaEmbutido = model.FechaEmbutido,
                    Corte = corteElaborado,
                    Sucursal = new Entidades.Sucursal { IdSucursal = model.IdSucursal },
                    Observaciones = model.Observaciones ?? "",
                    CreadoPor = usuarioCreador
                };

                int idEmbutido = _oCorteN.agregarEmbutido(embutido);
                if (idEmbutido <= 0)
                    return Json(new { ok = false, mensaje = "No se pudo registrar el elaborado." });

                embutido.IdEmbutido = idEmbutido;

                foreach (var item in BuildIngredientesAutomaticos(model, embutido))
                {
                    _oCorteN.agregarCortePorEmbutido(item);
                }

                foreach (var linea in model.Lineas ?? new List<ElaboradoCargaLineaVm>())
                {
                    var item = new Entidades.CortePorEmbutido
                    {
                        Embutido = embutido,
                        Corte = new Entidades.Corte { IdCorte = linea.IdCorte },
                        KgUtilizado = linea.CantKg,
                        PesoBalanza = linea.PesoBalanza
                    };

                    _oCorteN.agregarCortePorEmbutido(item);
                }

                float cantidadRegistrada = CalcularCantidadRegistrada(model, corteElaborado);
                string unidadTexto = corteElaborado.Pesable ? "kgs" : "unidades";
                string nombreElaborado = !string.IsNullOrWhiteSpace(corteElaborado.CorteDesc) ? corteElaborado.CorteDesc : corteElaborado.corte;
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Elaborados";
                TempData["AlertMsg"] = "Se guardó correctamente " +
                    FormatearCantidadMensaje(cantidadRegistrada, corteElaborado.Pesable) + " " +
                    unidadTexto + " de " + nombreElaborado + ".";

                return Json(new
                {
                    ok = true,
                    mensaje = "El elaborado se registro correctamente.",
                    redirectUrl = Url.Action("Index", "Elaborados")
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Anular(int idEmbutido)
        {
            try
            {
                var embutido = _oCorteN.findEmbutidoById(idEmbutido);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return Json(new { ok = false, mensaje = "No se encontró el elaborado." });

                if (string.Equals(embutido.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase))
                    return Json(new { ok = false, mensaje = "El elaborado ya se encuentra anulado." });

                embutido.ActualizadoPor = _usuarioActual;
                _oCorteN.anularEmbutido(embutido);

                return Json(new
                {
                    ok = true,
                    mensaje = "El elaborado se anuló correctamente.",
                    redirectUrl = Url.Action("Index", "Elaborados")
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        private IActionResult VistaIngresoRapido(bool esDesarme)
        {
            var model = new ElaboradoRapidoIndexVm
            {
                EsDesarme = esDesarme,
                Titulo = esDesarme ? "Desarme de elaborado" : "Ingreso rápido",
                Descripcion = esDesarme
                    ? "Seleccioná un elaborado con fórmula para registrar su desarme. La cantidad se descontará automáticamente junto con sus ingredientes."
                    : "Seleccioná un elaborado con fórmula para ingresar solo la cantidad final y calcular automáticamente todos sus ingredientes.",
                MostrarTodos = false,
                Items = ObtenerItemsIngresoRapido(),
                Tabs = BuildTabs(esDesarme ? "Desarme" : "IngresoRapido")
            };

            // El original siempre renderiza el mismo archivo de vista (IngresoRapido.cshtml) tanto
            // para IngresoRapido() como para Desarme() -- la vista se adapta sola segun
            // Model.EsDesarme, no hay una vista "Desarme.cshtml" separada.
            ViewBag.Title = model.Titulo;
            return View("IngresoRapido", model);
        }

        private List<ElaboradoRapidoItemVm> ObtenerItemsIngresoRapido()
        {
            var items = new List<ElaboradoRapidoItemVm>();
            var ids = new HashSet<int>();
            var dt = _oCorteN.getListaElegirEmbutido() ?? new DataTable();

            foreach (DataRow row in dt.Rows)
            {
                int idCorte = ToInt(row, "idCorteEmbutido", "IdCorteEmbutido", "idCorte", "IdCorte");
                if (idCorte <= 0 || ids.Contains(idCorte))
                    continue;

                var corte = _oCorteN.findCorteById(idCorte, false);
                int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
                if (corte == null || corte.IdCorte <= 0 || (idEmpresaSesion > 0 && corte.IdEmpresa != idEmpresaSesion))
                    continue;
                var item = CrearItemIngresoRapido(corte);
                if (item == null)
                    continue;

                ids.Add(idCorte);
                items.Add(item);
            }

            int idEmpresaSesionListado = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
            foreach (var corte in (idEmpresaSesionListado > 0
                ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesionListado, false) ?? new List<Entidades.Corte>())
                : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>())))
            {
                if (corte == null || corte.IdCorte <= 0 || ids.Contains(corte.IdCorte))
                    continue;

                var item = CrearItemIngresoRapido(corte);
                if (item == null)
                    continue;

                ids.Add(corte.IdCorte);
                items.Add(item);
            }

            return items
                .OrderByDescending(x => x.IngresoRapido)
                .ThenBy(x => x.Codigo)
                .ThenBy(x => x.Producto)
                .ToList();
        }

        private ElaboradoRapidoItemVm CrearItemIngresoRapido(Entidades.Corte corte)
        {
            if (corte == null || corte.IdCorte <= 0)
                return null;

            if (!corte.IngresoRapidoEmbutido)
                return null;

            var formula = _oCorteN.findFormulaByID(0, corte.IdCorte);
            var tieneFormula = formula != null && formula.IdFormula > 0;
            if (!tieneFormula)
                return null;

            return new ElaboradoRapidoItemVm
            {
                IdCorte = corte.IdCorte,
                Codigo = corte.Codigo,
                Producto = !string.IsNullOrWhiteSpace(corte.CorteDesc) ? corte.CorteDesc : corte.corte,
                IngresoRapido = corte.IngresoRapidoEmbutido,
                TieneFormula = true,
                Receta = formula.Receta ?? ""
            };
        }

        private List<ElaboradoResumenVm> MapElaborados(DataTable dt)
        {
            var lista = new List<ElaboradoResumenVm>();
            if (dt == null) return lista;

            foreach (DataRow row in dt.Rows)
            {
                string observaciones = ToString(row, "Observaciones", "observaciones");
                float kgs = ToFloat(row, "Kgs", "kgs");

                lista.Add(new ElaboradoResumenVm
                {
                    Id = ToInt(row, "Id", "idEmbutido"),
                    Fecha = ToDate(row, "Fecha", "fechaEmbutido", "fecha"),
                    Sucursal = ToString(row, "Sucursal", "sucursal"),
                    Codigo = ToLong(row, "Cod.Emb", "CodEmb", "codigoEmbutido", "CodigoEmbutido", "Código", "Codigo", "codigo"),
                    Elaborado = ToString(row, "Embutido", "Elaborado", "corte"),
                    Kgs = kgs,
                    Observaciones = observaciones,
                    Estado = ToString(row, "Estado", "estado"),
                    Creado = ToDateString(row, "Creado", "creado"),
                    Actualizado = ToDateString(row, "Actualizado", "actualizado"),
                    EsIngresoRapido = false,
                    EsDesarme = !string.IsNullOrWhiteSpace(observaciones)
                        && observaciones.ToLowerInvariant().Contains("desarme")
                        && kgs < 0
                });
            }

            MarcarTiposElaborado(lista);
            return lista.OrderByDescending(x => x.Fecha).ToList();
        }

        private List<ElaboradoLineaResumenVm> MapLineas(DataTable dt)
        {
            var lista = new List<ElaboradoLineaResumenVm>();
            if (dt == null) return lista;

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ElaboradoLineaResumenVm
                {
                    Id = ToInt(row, "Id", "idEmbutido"),
                    Fecha = ToDate(row, "Fecha", "fechaEmbutido", "fecha"),
                    Sucursal = ToString(row, "Sucursal", "sucursal"),
                    CodigoElaborado = ToLong(row, "Cod.Emb", "CodEmb", "codigoEmbutido", "codigo"),
                    Elaborado = ToString(row, "Embutido", "Elaborado", "corteEmbutido"),
                    CodigoIngrediente = ToLong(row, "Codigo", "Cod.Corte", "codigoIngrediente"),
                    Ingrediente = ToString(row, "Corte", "Ingrediente", "corte"),
                    Kgs = ToFloat(row, "Kgs", "kgs"),
                    Estado = ToString(row, "Estado", "estado"),
                    Observaciones = ToString(row, "Observaciones", "observaciones"),
                    EsDesarme = false,
                    EsIngresoRapido = false
                });
            }

            MarcarTiposLineas(lista);
            return lista.OrderByDescending(x => x.Fecha).ToList();
        }

        private List<ElaboradoFormulaResumenVm> MapFormulas(DataTable dt)
        {
            var lista = new List<ElaboradoFormulaResumenVm>();
            if (dt == null) return lista;

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ElaboradoFormulaResumenVm
                {
                    IdFormula = ToInt(row, "idFormula", "IdFormula"),
                    Codigo = ToLong(row, "codigo", "Codigo"),
                    Elaborado = ToString(row, "corte", "Corte", "Elaborado"),
                    Creado = ToDateString(row, "creado", "Creado"),
                    Actualizado = ToDateString(row, "actualizado", "Actualizado")
                });
            }

            return lista.OrderBy(x => x.Codigo).ToList();
        }

        private ElaboradoDetalleVm ConstruirDetalleElaborado(int idEmbutido)
        {
            var embutido = _oCorteN.findEmbutidoById(idEmbutido);
            if (embutido == null || embutido.IdEmbutido <= 0)
                return null;

            var formula = _oCorteN.findFormulaByID(0, embutido.Corte != null ? embutido.Corte.IdCorte : 0);
            var detalle = new ElaboradoDetalleVm
            {
                Id = embutido.IdEmbutido,
                Fecha = embutido.FechaEmbutido,
                Sucursal = embutido.Sucursal != null ? embutido.Sucursal.SucursalNombre : "",
                Codigo = embutido.Corte != null ? embutido.Corte.Codigo : 0,
                Elaborado = embutido.Corte != null ? embutido.Corte.CorteDesc : "",
                Kgs = embutido.CortesEnEmbutido != null ? embutido.CortesEnEmbutido.Sum(x => x != null ? x.KgUtilizado : 0f) : 0f,
                Observaciones = embutido.Observaciones ?? "",
                Estado = embutido.Estado ?? "",
                Receta = formula != null ? (formula.Receta ?? "") : "",
                UsuarioCreacion = embutido.CreadoPor != null ? embutido.CreadoPor.Nombre : "",
                FechaCreacion = embutido.Creado,
                UsuarioActualizacion = embutido.ActualizadoPor != null ? embutido.ActualizadoPor.Nombre : "",
                FechaActualizacion = embutido.Actualizado,
                EsIngresoRapido = embutido.Corte != null && embutido.Corte.IngresoRapidoEmbutido
            };

            foreach (var linea in embutido.CortesEnEmbutido ?? new List<Entidades.CortePorEmbutido>())
            {
                if (linea == null || linea.Corte == null)
                    continue;

                var corteLinea = linea.Corte;
                string nombreLinea = !string.IsNullOrWhiteSpace(corteLinea.CorteDesc) ? corteLinea.CorteDesc : corteLinea.corte;
                if (corteLinea.Codigo <= 0 && corteLinea.IdCorte > 0)
                {
                    var corteRecargado = _oCorteN.findCorteById(corteLinea.IdCorte, false);
                    if (corteRecargado != null && corteRecargado.IdCorte > 0)
                    {
                        corteLinea = corteRecargado;
                        nombreLinea = !string.IsNullOrWhiteSpace(corteLinea.CorteDesc) ? corteLinea.CorteDesc : corteLinea.corte;
                    }
                }

                long codigoLinea = corteLinea.Codigo;
                if (codigoLinea <= 0
                    && embutido.Corte != null
                    && !string.IsNullOrWhiteSpace(nombreLinea)
                    && nombreLinea.IndexOf("Ajuste Formula", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    codigoLinea = embutido.Corte.Codigo;
                }

                detalle.IngredientesUtilizados.Add(new ElaboradoDetalleLineaVm
                {
                    Codigo = codigoLinea,
                    Producto = nombreLinea,
                    Kgs = linea.KgUtilizado,
                    PesoBalanza = linea.PesoBalanza
                });
            }

            return detalle;
        }

        private Dictionary<int, ElaboradoFormulaDetalleVm> ConstruirDetallesFormulas(DataTable dt)
        {
            var detalles = new Dictionary<int, ElaboradoFormulaDetalleVm>();
            if (dt == null)
                return detalles;

            foreach (DataRow row in dt.Rows)
            {
                int idFormula = ToInt(row, "idFormula", "IdFormula");
                if (idFormula <= 0 || detalles.ContainsKey(idFormula))
                    continue;

                var formula = _oCorteN.findFormulaByID(idFormula, 0);
                if (formula == null || formula.IdFormula <= 0)
                    continue;

                var detalle = new ElaboradoFormulaDetalleVm
                {
                    IdFormula = formula.IdFormula,
                    Codigo = formula.Embutido != null ? formula.Embutido.Codigo : 0,
                    Elaborado = formula.Embutido != null ? formula.Embutido.CorteDesc : "",
                    EsPesableElaborado = formula.Embutido != null && formula.Embutido.Pesable,
                    EsIngresoRapidoElaborado = formula.Embutido != null && formula.Embutido.IngresoRapidoEmbutido,
                    EtiquetaValorFormula = formula.Embutido != null && !formula.Embutido.Pesable ? "Unidad" : "Porcentaje",
                    Receta = formula.Receta ?? "",
                    Creado = FormatearFechaHora(formula.Creado),
                    CreadoPor = formula.CreadoPor != null ? formula.CreadoPor.Nombre : "-",
                    Actualizado = FormatearFechaHora(formula.Actualizado),
                    ActualizadoPor = formula.ActualizadoPor != null ? formula.ActualizadoPor.Nombre : "-"
                };

                foreach (var item in formula.ListaCortesEnFormula ?? new List<Entidades.CortePorFormula>())
                {
                    if (item == null || item.CorteEnFormula == null)
                        continue;

                    detalle.Lineas.Add(new ElaboradoFormulaEditLineaVm
                    {
                        IdCorte = item.CorteEnFormula.IdCorte,
                        Codigo = item.CorteEnFormula.Codigo,
                        Producto = !string.IsNullOrWhiteSpace(item.CorteEnFormula.CorteDesc) ? item.CorteEnFormula.CorteDesc : item.CorteEnFormula.corte,
                        Porcentaje = _oCorteN.ConvertirFormulaParaVisualizacion(_oCorteN.FormulaUsaUnidades(formula.Embutido), item.Porcentaje),
                        AgregarAuto = item.AgregarAuto,
                        EsAjusteFormula = EsProductoGenerico(item.CorteEnFormula)
                    });
                }

                detalle.Lineas = detalle.Lineas
                    .OrderByDescending(x => x.EsAjusteFormula)
                    .ThenBy(x => x.Codigo)
                    .ToList();
                detalles[idFormula] = detalle;
            }

            return detalles;
        }

        private List<ElaboradoTabVm> BuildTabs(string activeAction)
        {
            var tabs = new List<ElaboradoTabVm>
            {
                new ElaboradoTabVm { Titulo = "Elaborados", Action = "Index", Activo = string.Equals(activeAction, "Index", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Carga / ingreso", Action = "Carga", Activo = string.Equals(activeAction, "Carga", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Ingreso rapido", Action = "IngresoRapido", Activo = string.Equals(activeAction, "IngresoRapido", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Formulas", Action = "Formulas", Activo = string.Equals(activeAction, "Formulas", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Desarme de elaborado", Action = "Desarme", Activo = string.Equals(activeAction, "Desarme", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Lineas de elaborado", Action = "Lineas", Activo = string.Equals(activeAction, "Lineas", StringComparison.OrdinalIgnoreCase) },
            };

            return tabs;
        }

        private object MapProductoBusqueda(Entidades.Corte p)
        {
            return new
            {
                id = p.IdCorte,
                codigo = p.codigo.ToString(),
                nombre = !string.IsNullOrWhiteSpace(p.corte) ? p.corte : p.CorteDesc,
                precio = p.precioKg,
                tipo = p.Tipo ?? "",
                promedio = p.Promedio,
                ingresoRapido = p.IngresoRapidoEmbutido,
                pesable = p.Pesable
            };
        }

        private ElaboradoFormulaEditVm CrearViewModelFormulaEdicion(Entidades.Formula formula, Entidades.Usuario user)
        {
            bool escalaUnidad = _oCorteN.FormulaUsaUnidades(formula.Embutido);

            var model = new ElaboradoFormulaEditVm
            {
                IdFormula = formula.IdFormula,
                EsEdicion = true,
                SoloLecturaInicial = true,
                IdElaborado = formula.Embutido != null ? formula.Embutido.IdCorte : 0,
                CodigoElaborado = formula.Embutido != null ? formula.Embutido.Codigo : 0,
                Elaborado = formula.Embutido != null ? formula.Embutido.CorteDesc : "",
                EsPesableElaborado = formula.Embutido != null && formula.Embutido.Pesable,
                EsIngresoRapidoElaborado = formula.Embutido != null && formula.Embutido.IngresoRapidoEmbutido,
                EtiquetaValorFormula = escalaUnidad ? "Unidad" : "Porcentaje",
                EscalaUnidad = escalaUnidad,
                AjustarUnidad = formula.AjustarUnidad,
                Receta = formula.Receta ?? "",
                UsuarioNombre = user != null ? (user.Nombre ?? "") : "",
                Creado = FormatearFechaHora(formula.Creado),
                CreadoPor = formula.CreadoPor != null ? formula.CreadoPor.Nombre : "-",
                Actualizado = FormatearFechaHora(formula.Actualizado),
                ActualizadoPor = formula.ActualizadoPor != null ? formula.ActualizadoPor.Nombre : "-",
                Tabs = BuildTabs("Formulas")
            };

            foreach (var item in formula.ListaCortesEnFormula ?? new List<Entidades.CortePorFormula>())
            {
                if (item == null || item.CorteEnFormula == null)
                    continue;

                model.Lineas.Add(new ElaboradoFormulaEditLineaVm
                {
                    IdCorte = item.CorteEnFormula.IdCorte,
                    Codigo = item.CorteEnFormula.Codigo,
                    Producto = !string.IsNullOrWhiteSpace(item.CorteEnFormula.CorteDesc) ? item.CorteEnFormula.CorteDesc : item.CorteEnFormula.corte,
                    Porcentaje = _oCorteN.ConvertirFormulaParaVisualizacion(escalaUnidad, item.Porcentaje),
                    AgregarAuto = item.AgregarAuto,
                    EsAjusteFormula = EsProductoGenerico(item.CorteEnFormula),
                    NoSumaPeso = item.NoSumaPeso
                });
            }

            AplicarProductoGenericoFormula(model);
            model.Lineas = model.Lineas
                .OrderByDescending(x => x.EsAjusteFormula)
                .ThenBy(x => x.Codigo)
                .ToList();
            RecalcularTotalesFormula(model);
            return model;
        }

        private ElaboradoCargaVm CrearViewModelEdicion(Entidades.Embutido embutido, Entidades.Usuario user)
        {
            var model = new ElaboradoCargaVm
            {
                IdEmbutido = embutido.IdEmbutido,
                EsEdicion = true,
                PermiteGuardarEdicion = true,
                PuedeAnular = !string.Equals(embutido.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase),
                EsPesableElaborado = embutido.Corte != null && embutido.Corte.Pesable,
                IdSucursal = embutido.Sucursal != null ? embutido.Sucursal.IdSucursal : (user != null ? user.IdSucursal : 0),
                FechaEmbutido = embutido.FechaEmbutido,
                Observaciones = embutido.Observaciones ?? "",
                UsuarioNombre = user != null ? (user.Nombre ?? "") : "",
                Estado = embutido.Estado ?? "",
                IdElaborado = embutido.Corte != null ? embutido.Corte.IdCorte : 0,
                CodigoElaborado = embutido.Corte != null ? embutido.Corte.Codigo : 0,
                Elaborado = embutido.Corte != null ? embutido.Corte.CorteDesc : "",
                Receta = "",
                IngresoRapidoSugerido = embutido.Corte != null && embutido.Corte.IngresoRapidoEmbutido,
                Creado = FormatearFechaHora(embutido.Creado),
                CreadoPor = embutido.CreadoPor != null ? embutido.CreadoPor.Nombre : "-",
                Actualizado = FormatearFechaHora(embutido.Actualizado),
                ActualizadoPor = embutido.ActualizadoPor != null ? embutido.ActualizadoPor.Nombre : "-",
                Tabs = BuildTabs("Carga")
            };

            var formula = _oCorteN.findFormulaByID(0, model.IdElaborado);
            model.Receta = formula != null ? (formula.Receta ?? "") : "";

            var formulaItems = MapFormula(_oCorteN.getFormulaEmbutido(model.IdElaborado) ?? new DataTable());
            model.Formula = formulaItems;
            var codigosAutomaticos = new HashSet<long>(formulaItems.Where(x => x.AgregarAuto).Select(x => x.Codigo));

            foreach (var item in embutido.CortesEnEmbutido ?? new List<Entidades.CortePorEmbutido>())
            {
                if (item == null || item.Corte == null)
                    continue;

                if (codigosAutomaticos.Contains(item.Corte.Codigo))
                    continue;

                model.Lineas.Add(new ElaboradoCargaLineaVm
                {
                    IdCorte = item.Corte.IdCorte,
                    Codigo = item.Corte.Codigo,
                    Producto = !string.IsNullOrWhiteSpace(item.Corte.CorteDesc) ? item.Corte.CorteDesc : item.Corte.corte,
                    TipoProducto = item.Corte.Tipo ?? "",
                    CantKg = item.KgUtilizado,
                    PesoBalanza = item.PesoBalanza
                });
            }

            return model;
        }

        private ElaboradoRapidoEditVm CrearViewModelIngresoRapidoEdicion(Entidades.Embutido embutido, Entidades.Usuario user, bool esDesarmeSolicitado)
        {
            if (embutido == null || embutido.IdEmbutido <= 0 || embutido.Corte == null || !embutido.Corte.IngresoRapidoEmbutido)
                return null;

            bool esDesarme = esDesarmeSolicitado || (!string.IsNullOrWhiteSpace(embutido.Observaciones) && embutido.Observaciones.IndexOf("desarme", StringComparison.OrdinalIgnoreCase) >= 0);
            var formula = _oCorteN.findFormulaByID(0, embutido.Corte.IdCorte);
            var formulaItems = MapFormula(_oCorteN.getFormulaEmbutido(embutido.Corte.IdCorte) ?? new DataTable());
            if (formulaItems.Count == 0)
                return null;

            var model = new ElaboradoRapidoEditVm
            {
                IdEmbutido = embutido.IdEmbutido,
                EsEdicion = true,
                EsDesarme = esDesarme,
                PuedeAnular = !string.Equals(embutido.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase),
                IdSucursal = embutido.Sucursal != null ? embutido.Sucursal.IdSucursal : (user != null ? user.IdSucursal : 0),
                FechaEmbutido = embutido.FechaEmbutido,
                UsuarioNombre = user != null ? (user.Nombre ?? "") : "",
                Estado = embutido.Estado ?? "",
                IdElaborado = embutido.Corte.IdCorte,
                CodigoElaborado = embutido.Corte.Codigo,
                Elaborado = !string.IsNullOrWhiteSpace(embutido.Corte.CorteDesc) ? embutido.Corte.CorteDesc : embutido.Corte.corte,
                Receta = formula != null ? (formula.Receta ?? "") : "",
                EsPesableElaborado = embutido.Corte.Pesable,
                Formula = formulaItems,
                Tabs = BuildTabs(esDesarme ? "Desarme" : "IngresoRapido")
            };

            model.Cantidad = CalcularCantidadIngresoRapido(model.Formula, embutido.CortesEnEmbutido);
            RecalcularFormulaRapida(model);
            return model;
        }

        private void AplicarProductoGenericoFormula(ElaboradoFormulaEditVm model)
        {
            var productoGenerico = _oCorteN.ObtenerProductoAjusteFormula();
            model.CodigoProductoGenerico = productoGenerico != null ? productoGenerico.Codigo : 0;
            model.NombreProductoGenerico = productoGenerico != null
                ? (!string.IsNullOrWhiteSpace(productoGenerico.CorteDesc) ? productoGenerico.CorteDesc : productoGenerico.corte)
                : "";
        }

        private bool EsProductoGenerico(Entidades.Corte corte)
        {
            if (corte == null || corte.IdCorte <= 0)
                return false;

            var productoGenerico = _oCorteN.ObtenerProductoAjusteFormula();
            return productoGenerico != null && productoGenerico.IdCorte == corte.IdCorte;
        }

        private void PrepararMetadataFormulaModel(ElaboradoFormulaEditVm model)
        {
            if (model == null)
                return;

            AplicarProductoGenericoFormula(model);

            if (model.IdElaborado <= 0)
            {
                model.EtiquetaValorFormula = string.IsNullOrWhiteSpace(model.EtiquetaValorFormula) ? "Porcentaje" : model.EtiquetaValorFormula;
                return;
            }

            var elaborado = _oCorteN.findCorteById(model.IdElaborado, false);
            int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
            if (elaborado == null || elaborado.IdCorte <= 0 || (idEmpresaSesion > 0 && elaborado.IdEmpresa != idEmpresaSesion))
                return;

            model.CodigoElaborado = elaborado.Codigo;
            model.Elaborado = !string.IsNullOrWhiteSpace(elaborado.CorteDesc) ? elaborado.CorteDesc : elaborado.corte;
            model.EsPesableElaborado = elaborado.Pesable;
            model.EsIngresoRapidoElaborado = elaborado.IngresoRapidoEmbutido;
            model.EtiquetaValorFormula = model.EscalaUnidad ? "Unidad" : "Porcentaje";
        }

        private List<ElaboradoFormulaLineaVm> MapFormula(DataTable dtFormula)
        {
            var items = new List<ElaboradoFormulaLineaVm>();
            if (dtFormula == null)
                return items;

            foreach (DataRow row in dtFormula.Rows)
            {
                items.Add(new ElaboradoFormulaLineaVm
                {
                    IdCorte = ToInt(row, "idCorte", "IdCorte"),
                    Codigo = ToLong(row, "codigo", "Codigo"),
                    Producto = ToString(row, "corte", "Corte"),
                    Porcentaje = ToFloat(row, "porcentaje", "Porcentaje"),
                    AgregarAuto = ToBool(row, "agregarAuto", "AgregarAuto"),
                    Kgs = ToFloat(row, "kgs", "Kgs")
                });
            }

            return items;
        }

        private string ValidarCarga(ElaboradoCargaVm model)
        {
            if (model.IdSucursal <= 0)
                return "Debe seleccionar una sucursal.";

            if (model.IdElaborado <= 0)
                return "Debe seleccionar el elaborado.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe agregar al menos un ingrediente manual.";

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea.IdCorte <= 0)
                    return "La linea " + (i + 1) + " no tiene producto valido.";

                if (linea.CantKg <= 0)
                    return "La linea " + (i + 1) + " debe tener kilos mayores a cero.";
            }

            return "";
        }

        private string ValidarIngresoRapido(ElaboradoRapidoEditVm model)
        {
            if (model == null)
                return "No se recibieron datos del ingreso rápido.";

            if (model.IdSucursal <= 0)
                return "Debe seleccionar una sucursal.";

            if (model.IdElaborado <= 0)
                return "Debe seleccionar el elaborado.";

            if (Math.Abs(model.Cantidad) <= 0f)
                return "Debe ingresar una cantidad mayor a cero.";

            return "";
        }

        private void MarcarTiposElaborado(List<ElaboradoResumenVm> items)
        {
            var idsIngresoRapido = _oCorteN.ObtenerIdsEmbutidosIngresoRapido((items ?? new List<ElaboradoResumenVm>()).Select(x => x.Id));
            foreach (var item in items ?? new List<ElaboradoResumenVm>())
            {
                item.EsIngresoRapido = idsIngresoRapido.Contains(item.Id);
            }
        }

        private void MarcarTiposLineas(List<ElaboradoLineaResumenVm> items)
        {
            var idsIngresoRapido = _oCorteN.ObtenerIdsEmbutidosIngresoRapido((items ?? new List<ElaboradoLineaResumenVm>()).Select(x => x.Id));
            foreach (var item in items ?? new List<ElaboradoLineaResumenVm>())
            {
                item.EsIngresoRapido = idsIngresoRapido.Contains(item.Id);
                item.EsDesarme = !string.IsNullOrWhiteSpace(item.Observaciones)
                    && item.Observaciones.IndexOf("desarme", StringComparison.OrdinalIgnoreCase) >= 0
                    && item.Kgs < 0;
            }
        }

        private float CalcularCantidadIngresoRapido(List<ElaboradoFormulaLineaVm> formula, List<Entidades.CortePorEmbutido> lineas)
        {
            foreach (var itemFormula in formula ?? new List<ElaboradoFormulaLineaVm>())
            {
                if (itemFormula == null || itemFormula.IdCorte <= 0 || itemFormula.Porcentaje == 0f)
                    continue;

                var linea = (lineas ?? new List<Entidades.CortePorEmbutido>())
                    .FirstOrDefault(x => x != null && x.Corte != null && x.Corte.IdCorte == itemFormula.IdCorte && x.KgUtilizado != 0f);

                if (linea == null)
                    continue;

                return (float)Math.Round(Math.Abs((linea.KgUtilizado * 100f) / itemFormula.Porcentaje), 3);
            }

            return 0f;
        }

        private string ValidarFormula(ElaboradoFormulaEditVm model)
        {
            if (model == null)
                return "No se recibieron datos de la fórmula.";

            if (model.IdElaborado <= 0)
                return "Debe seleccionar el elaborado.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe agregar al menos un ingrediente a la fórmula.";

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea.IdCorte <= 0)
                    return "La línea " + (i + 1) + " no tiene un ingrediente válido.";

                if (linea.Porcentaje < 0)
                    return "La línea " + (i + 1) + " tiene un porcentaje inválido.";
            }

            return "";
        }

        private static void RecalcularFormulaRapida(ElaboradoRapidoEditVm model)
        {
            if (model == null)
                return;

            float cantidad = Math.Abs(model.Cantidad);
            if (model.EsDesarme)
                cantidad *= -1f;

            foreach (var item in model.Formula ?? new List<ElaboradoFormulaLineaVm>())
            {
                item.Kgs = (float)Math.Round(0.01f * cantidad * item.Porcentaje, 3);
            }
        }

        private List<Entidades.CortePorEmbutido> BuildIngredientesAutomaticos(ElaboradoCargaVm model, Entidades.Embutido embutido)
        {
            var lista = new List<Entidades.CortePorEmbutido>();
            var dtFormula = _oCorteN.getFormulaEmbutido(model.IdElaborado) ?? new DataTable();
            if (dtFormula.Rows.Count == 0)
                return lista;

            float totalKgSinCond = CalcularBaseFormula(model, dtFormula);

            foreach (DataRow row in dtFormula.Rows)
            {
                if (!ToBool(row, "agregarAuto", "AgregarAuto"))
                    continue;

                float porcentaje = ToFloat(row, "porcentaje", "Porcentaje");
                float kgs = (float)Math.Round(0.01f * totalKgSinCond * porcentaje, 3);
                if (kgs == 0f)
                    continue;

                lista.Add(new Entidades.CortePorEmbutido
                {
                    Embutido = embutido,
                    Corte = new Entidades.Corte { IdCorte = ToInt(row, "idCorte", "IdCorte") },
                    KgUtilizado = kgs,
                    PesoBalanza = false
                });
            }

            return lista;
        }

        private float CalcularBaseFormula(ElaboradoCargaVm model, DataTable dtFormula)
        {
            float totalKg = 0f;
            var autoCodes = new HashSet<long>();
            foreach (DataRow row in dtFormula.Rows)
            {
                if (ToBool(row, "agregarAuto", "AgregarAuto"))
                    autoCodes.Add(ToLong(row, "codigo", "Codigo"));
            }

            foreach (var linea in model.Lineas ?? new List<ElaboradoCargaLineaVm>())
            {
                if (autoCodes.Contains(linea.Codigo))
                    continue;

                totalKg += linea.CantKg;
            }

            return totalKg;
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

        private static bool HasColumn(DataRow row, string columnName)
        {
            return row != null && row.Table != null && row.Table.Columns.Contains(columnName);
        }

        private static string ToString(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (!HasColumn(row, columnName)) continue;
                object value = row[columnName];
                if (value != DBNull.Value)
                    return Convert.ToString(value) ?? "";
            }

            return "";
        }

        private static int ToInt(DataRow row, params string[] columnNames)
        {
            int value;
            return int.TryParse(ToString(row, columnNames), out value) ? value : 0;
        }

        private static long ToLong(DataRow row, params string[] columnNames)
        {
            long value;
            return long.TryParse(ToString(row, columnNames), out value) ? value : 0;
        }

        private static float ToFloat(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (!HasColumn(row, columnName))
                    continue;

                object raw = row[columnName];
                if (raw == null || raw == DBNull.Value)
                    continue;

                if (raw is float || raw is double || raw is decimal || raw is int || raw is long || raw is short || raw is byte)
                    return Convert.ToSingle(raw, CultureInfo.InvariantCulture);

                float value;
                string text = Convert.ToString(raw) ?? "";
                if (float.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                    return value;

                if (float.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out value))
                    return value;

                if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    return value;

                string replaced = text.Replace(".", ",");
                if (float.TryParse(replaced, NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out value))
                    return value;
            }

            return 0f;
        }

        private static DateTime ToDate(DataRow row, params string[] columnNames)
        {
            DateTime value;
            return DateTime.TryParse(ToString(row, columnNames), out value) ? value : DateTime.MinValue;
        }

        private static string ToDateString(DataRow row, params string[] columnNames)
        {
            DateTime value;
            return DateTime.TryParse(ToString(row, columnNames), out value)
                ? value.ToString("dd/MM/yyyy HH:mm:ss")
                : "";
        }

        private static bool ToBool(DataRow row, params string[] columnNames)
        {
            bool value;
            return bool.TryParse(ToString(row, columnNames), out value) && value;
        }

        private static DateTime NormalizarFechaDesde(DateTime fecha)
        {
            return fecha;
        }

        private int ObtenerIdEmpresaSesionActual()
        {
            return _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : (_empresa != null ? _empresa.IdEmpresa : 0);
        }

        private static DateTime NormalizarFechaHasta(DateTime fecha)
        {
            return fecha.TimeOfDay == TimeSpan.Zero ? fecha.Date.AddDays(1).AddSeconds(-1) : fecha;
        }

        private static string FormatearFechaHora(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-";
        }

        private static void RecalcularTotalesFormula(ElaboradoFormulaEditVm model)
        {
            if (model == null)
                return;

            model.TotalPorcentaje = 0f;
            foreach (var linea in model.Lineas ?? new List<ElaboradoFormulaEditLineaVm>())
            {
                if (linea.EsAjusteFormula)
                    continue;

                model.TotalPorcentaje += linea.Porcentaje;
            }

            model.TotalUnidades = model.EscalaUnidad
                ? model.TotalPorcentaje
                : (model.TotalPorcentaje / 100f);
        }

        private float CalcularCantidadRegistrada(ElaboradoCargaVm model, Entidades.Corte corteElaborado)
        {
            if (model == null)
                return 0f;

            float cantidad = 0f;
            foreach (var linea in model.Lineas ?? new List<ElaboradoCargaLineaVm>())
            {
                cantidad += linea.CantKg;
            }

            if (!corteElaborado.Pesable)
                cantidad = (float)Math.Round(cantidad, 0);

            return cantidad;
        }

        private static string FormatearCantidadMensaje(float cantidad, bool esPesable)
        {
            return cantidad.ToString(esPesable ? "N3" : "N0", CultureInfo.GetCultureInfo("es-AR"));
        }

        // Trivial, mismo criterio que Movimientos/Stock -- deja la puerta abierta a un login real
        // futuro (con el stub, EsUsuarioProduccion=false siempre devuelve usuarioSesion sin cambios).
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
    }
}
