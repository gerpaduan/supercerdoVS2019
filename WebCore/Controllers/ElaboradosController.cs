// Port de Web/Controllers/ElaboradosController.cs (1859 lineas, 17 acciones) -- catalogo de
// elaborados/embutidos (carga manual, ingreso rapido con formula, formulas/recetas, desarme).
// Portado completo: Index/Lineas/Detalle (listados), Formulas/EditarFormula/GuardarFormula/
// EliminarFormula (CRUD de formulas), IngresoRapido/Desarme/EditarIngresoRapido/
// GuardarIngresoRapido (ingreso con formula automatica), Carga/GuardarCarga (carga manual),
// BuscarProducto/BuscarProductoPorCodigo/ObtenerFormula (autocompletado), Anular.
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06). Permisos reales y gate de "usuario de sala de produccion" portados 2026-09-09
// (Batch B, ver docs/DECISIONS.md "Batch B: permisos reales + operador de produccion"):
// Permisos.Elaborado.* via _oUsuarioN.tienePermiso (equivalente exacto de
// PermisosHelper.TienePermiso del clasico), SeleccionUsuario para la cuenta compartida de
// produccion (port de Web/Controllers/SeleccionUsuarioController.cs,
// WebCore/Helpers/PermisosHelper.cs). NO portado (deliberado, UX-only, ver mismo criterio en
// MovimientosController.cs): ConstruirMensajePermisoFecha/ConfigurarAdvertenciaFechaEnVivo.
using System.Data;
using System.Globalization;
using System.Text.Json;
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
        private readonly Negocio.Usuario _oUsuarioN;

        // El formateador JSON global de ASP.NET Core cameliza los nombres de propiedad
        // (IdCorte -> idCorte). elaborados-carga.js/elaborados-rapido.js (renderFormula/
        // recalcularFormula) leen los campos en PascalCase tal cual estan en
        // ElaboradoFormulaLineaVm -- mismo criterio que Model.Formula embebido en la vista via
        // System.Text.Json.JsonSerializer.Serialize (que no camel-iza por defecto). Sin esto,
        // ObtenerFormula devuelve producto/kgs "camelizados" y el JS los lee como undefined/0.
        private static readonly JsonSerializerOptions PascalCaseJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public ElaboradosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
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

            AplicarSecretoCantidadElaborados(model.Items);

            model.TotalKg = model.Items
                .Where(x => string.IsNullOrWhiteSpace(x.Estado))
                .Sum(x => x.Kgs);

            ViewBag.Title = "Elaborados";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(_oSucursalN.findAll() ?? new List<Entidades.Sucursal>());
            // Lista para el selector de usuario de "Ver fórmula" (formula-secreta.js).
            ViewBag.UsuariosConPermisoFormula = ObtenerUsuariosConPermisoFormulaParaCombo();

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
            // Lista para el selector de usuario de "Ver fórmula" (formula-secreta.js).
            ViewBag.UsuariosConPermisoFormula = ObtenerUsuariosConPermisoFormulaParaCombo();

            return View(model);
        }

        public IActionResult Formulas(string descripcion = "")
        {
            var user = _usuarioActual;

            // Port de Web/Controllers/ElaboradosController.cs:127-131.
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.VerFormulas, DateTime.Today, -1))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            DataTable dt = _oCorteN.buscarFormula((descripcion ?? "").Trim()) ?? new DataTable();
            // Port de Web/Controllers/ElaboradosController.cs:134.
            bool puedeEditar = _oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoFormula, DateTime.Today, user.Id);
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

            // Formula secreta: el listado marca con candado las secretas. Esta pantalla ya exige
            // VerFormulas, por eso el detalle se muestra sin re-login (el re-login aplica a las
            // pantallas operativas: Ingreso Rapido, Carga, Detalle y Lineas).
            foreach (var item in model.Items)
            {
                if (model.Detalles.TryGetValue(item.IdFormula, out var detalleFormula))
                    item.Secreta = detalleFormula.Secreta;
            }

            ViewBag.Title = "Formulas";

            return View(model);
        }

        public IActionResult EditarFormula(int id = 0)
        {
            var user = _usuarioActual;

            // Port de Web/Controllers/ElaboradosController.cs:158-164.
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoFormula, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = "No tiene permisos para administrar fórmulas.";
                return RedirectToAction("Formulas");
            }

            ElaboradoFormulaEditVm model;
            if (id > 0)
            {
                var formula = _oCorteN.findFormulaByID(id, 0);
                if (formula == null || formula.IdFormula <= 0)
                    return NotFound("No se encontró la fórmula.");

                // Port de Web/Controllers/ElaboradosController.cs:173-180.
                int idCreadorFormula = formula.CreadoPor != null ? formula.CreadoPor.Id : user.Id;
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoFormula, DateTime.Today, idCreadorFormula))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "No tiene permisos para administrar fórmulas.";
                    return RedirectToAction("Formulas");
                }

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

            // Costeo (solo Usuario.Admin, ver docs/DECISIONS.md pedido 2026-09-21): visibilidad de
            // toda la columna/panel de costo en EditarFormula.cshtml.
            model.EsAdmin = user.Admin;

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

            // Port de Web/Controllers/ElaboradosController.cs:224-231.
            var redirectSeleccion = WebCore.Helpers.PermisosHelper.RequiereSeleccionUsuario(
                this, user, idUsuarioCreador, Request.Path + Request.QueryString);
            if (redirectSeleccion != null) return redirectSeleccion;

            // Port de Web/Controllers/ElaboradosController.cs:236.
            var operadorResuelto = user.EsUsuarioProduccion ? ResolverUsuarioCreador(idUsuarioCreador, user) : null;
            int idUsuarioCreadorResuelto = operadorResuelto?.Id ?? 0;

            // Port de Web/Controllers/ElaboradosController.cs:240-246.
            if (id <= 0 && !_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoEmbutidoRapido, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = "No tiene permisos para ingreso rápido.";
                return RedirectToAction(esDesarme ? "Desarme" : "IngresoRapido");
            }

            if (id > 0)
            {
                var embutido = _oCorteN.findEmbutidoById(id);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return NotFound("No se encontró el elaborado.");

                // Port de Web/Controllers/ElaboradosController.cs:254-262.
                int idCreador = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                bool puedeModificar = _oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoEmbutidoRapido, embutido.FechaEmbutido, idCreador);
                if (!puedeModificar && !_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.VerEmbutidos, embutido.FechaEmbutido, -1))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "No tiene permisos para consultar elaborados.";
                    return RedirectToAction(esDesarme ? "Desarme" : "IngresoRapido");
                }

                idCreadorPermiso = idCreador;
                var modelEdicion = CrearViewModelIngresoRapidoEdicion(embutido, user, esDesarme);
                if (modelEdicion == null)
                    return NotFound("No se encontró el elaborado.");

                // Formula secreta: la Cantidad ya se calculo con la formula; recien ahora se oculta.
                AplicarSecretoFormulaRapida(modelEdicion);

                modelEdicion.SoloLecturaInicial = !string.Equals(modelEdicion.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase);
                modelEdicion.PuedeHabilitarEdicion = puedeModificar;
                // Costeo (solo Usuario.Admin), ver docs/DECISIONS.md pedido 2026-09-21.
                modelEdicion.EsAdmin = user.Admin;
                modelEdicion.PrecioActualElaborado = embutido.Corte != null ? embutido.Corte.PrecioKg : 0f;
                if (!puedeModificar)
                    modelEdicion.PuedeAnular = false;
                if (operadorResuelto != null)
                    modelEdicion.UsuarioNombre = operadorResuelto.Nombre;

                ViewBag.Title = modelEdicion.EsDesarme ? "Desarme de elaborado" : "Ingreso rápido";
                ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
                ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
                ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
                ViewBag.UsuariosConPermisoFormula = ObtenerUsuariosConPermisoFormulaParaCombo();
                ViewBag.IdUsuarioCreadorPreseleccionado = idUsuarioCreadorResuelto;
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
                UsuarioNombre = operadorResuelto != null ? operadorResuelto.Nombre : (user.Nombre ?? ""),
                IdElaborado = corte.IdCorte,
                CodigoElaborado = corte.Codigo,
                Elaborado = !string.IsNullOrWhiteSpace(corte.CorteDesc) ? corte.CorteDesc : corte.corte,
                Receta = formula != null ? (formula.Receta ?? "") : "",
                EsPesableElaborado = corte.Pesable,
                Formula = formulaItems,
                Tabs = BuildTabs(esDesarme ? "Desarme" : "IngresoRapido"),
                PuedeHabilitarEdicion = true,
                // Costeo (solo Usuario.Admin), ver docs/DECISIONS.md pedido 2026-09-21.
                EsAdmin = user.Admin,
                PrecioActualElaborado = corte.PrecioKg
            };

            RecalcularFormulaRapida(model);
            AplicarSecretoFormulaRapida(model);

            ViewBag.Title = esDesarme ? "Desarme de elaborado" : "Ingreso rápido";
            ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.UsuariosConPermisoFormula = ObtenerUsuariosConPermisoFormulaParaCombo();
            ViewBag.IdUsuarioCreadorPreseleccionado = idUsuarioCreadorResuelto;
            return View(model);
        }

        public IActionResult Carga(int id = 0, int idUsuarioCreador = 0)
        {
            var user = _usuarioActual;
            int idCreadorPermiso = user.Id;

            // Port de Web/Controllers/ElaboradosController.cs:337-344.
            var redirectSeleccion = WebCore.Helpers.PermisosHelper.RequiereSeleccionUsuario(
                this, user, idUsuarioCreador, Request.Path + Request.QueryString);
            if (redirectSeleccion != null) return redirectSeleccion;

            // Port de Web/Controllers/ElaboradosController.cs:349.
            var operadorResuelto = user.EsUsuarioProduccion ? ResolverUsuarioCreador(idUsuarioCreador, user) : null;
            int idUsuarioCreadorResuelto = operadorResuelto?.Id ?? 0;

            ElaboradoCargaVm model;
            if (id > 0)
            {
                var embutido = _oCorteN.findEmbutidoById(id);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return NotFound("No se encontró el elaborado.");

                // Port de Web/Controllers/ElaboradosController.cs:360-368.
                int idCreador = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                bool puedeModificar = _oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoEmbutido, embutido.FechaEmbutido, idCreador);
                if (!puedeModificar && !_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.VerEmbutidos, embutido.FechaEmbutido, -1))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "No tiene permisos para consultar elaborados.";
                    return RedirectToAction("Index");
                }

                idCreadorPermiso = idCreador;
                model = CrearViewModelEdicion(embutido, user);
                // Formula secreta: CrearViewModelEdicion ya excluyo de Lineas los ingredientes automaticos
                // usando la formula; recien ahora se vacian Formula y Receta si esta oculta.
                AplicarSecretoFormulaCarga(model);
                model.SoloLecturaInicial = !string.Equals(model.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase);
                model.PuedeHabilitarEdicion = puedeModificar;
                model.PermiteGuardarEdicion = puedeModificar;
                if (!puedeModificar)
                    model.PuedeAnular = false;
                if (operadorResuelto != null)
                    model.UsuarioNombre = operadorResuelto.Nombre;
                // Costeo (solo Usuario.Admin), ver docs/DECISIONS.md pedido 2026-09-21.
                model.PrecioActualElaborado = embutido.Corte != null ? embutido.Corte.PrecioKg : 0f;
            }
            else
            {
                // Port de Web/Controllers/ElaboradosController.cs:382-388.
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoEmbutido, DateTime.Today, user.Id))
                {
                    TempData["AlertType"] = "warning";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "No tiene permisos para crear o modificar elaborados.";
                    return RedirectToAction("Index");
                }

                model = new ElaboradoCargaVm
                {
                    IdSucursal = user.IdSucursal > 0 ? user.IdSucursal : 0,
                    FechaEmbutido = DateTime.Now,
                    UsuarioNombre = operadorResuelto != null ? operadorResuelto.Nombre : (user.Nombre ?? ""),
                    Tabs = BuildTabs("Carga"),
                    PermiteGuardarEdicion = true,
                    PuedeHabilitarEdicion = true
                };
            }

            // Costeo (solo Usuario.Admin), ver docs/DECISIONS.md pedido 2026-09-21.
            model.EsAdmin = user.Admin;

            ViewBag.Title = model.EsEdicion ? "Modificar elaborado" : "Carga / ingreso de elaborado";
            ViewBag.Sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.EsUsuarioProduccion = user.EsUsuarioProduccion;
            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo();
            ViewBag.UsuariosConPermisoFormula = ObtenerUsuariosConPermisoFormulaParaCombo();
            ViewBag.IdUsuarioCreadorPreseleccionado = idUsuarioCreadorResuelto;

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
                tieneFormula = tieneOtraFormula,
                // Costeo (solo Usuario.Admin): precio de venta actual del producto, reutilizado
                // tanto para el toggle Compra/Venta del ingrediente como para "precio actual" del
                // elaborado en las vistas de Formula/Carga/Ingreso Rapido.
                precioVenta = corte.PrecioKg
            });
        }

        [HttpGet]
        public JsonResult ObtenerFormula(int idCorte)
        {
            try
            {
                // Formula secreta: sin una elevacion vigente de esta sesion NO viajan ingredientes, %
                // ni receta. Solo se informa que existe (secreta=true) para que la pantalla muestre
                // "Ver formula". Con elevacion (o si no es secreta) devuelve todo como siempre.
                if (FormulaOculta(idCorte))
                {
                    return Json(new
                    {
                        ok = true,
                        secreta = true,
                        receta = "",
                        tieneFormula = true,
                        formula = new List<ElaboradoFormulaLineaVm>()
                    }, PascalCaseJsonOptions);
                }

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
                    secreta = formula != null && formula.Secreta,
                    receta = formula != null ? (formula.Receta ?? "") : "",
                    tieneFormula = items.Count > 0,
                    formula = items
                }, PascalCaseJsonOptions);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        // ===== Formula secreta (2026-10-04, ver docs/DECISIONS.md "Formula secreta con re-login") =====

        // true si la formula del producto elaborado es secreta y esta sesion NO tiene una elevacion
        // vigente: en ese caso ninguna pantalla debe mostrar ingredientes, % ni receta.
        private bool FormulaOculta(int idCorte)
        {
            return idCorte > 0
                && _oCorteN.esFormulaSecreta(idCorte)
                && !WebCore.Helpers.FormulaSecretaHelper.TieneElevacion(HttpContext.Session, idCorte);
        }

        // Aplica el ocultamiento a un modelo de Ingreso Rapido ya armado: marca FormulaSecreta/FormulaOculta
        // y, si esta oculta, vacia Formula y Receta (la vista no debe renderizarlas ni como hidden).
        // Se llama DESPUES de calcular lo que dependa de la formula (ej. Cantidad en la edicion).
        private void AplicarSecretoFormulaRapida(ElaboradoRapidoEditVm model)
        {
            if (model == null) return;

            model.FormulaSecreta = _oCorteN.esFormulaSecreta(model.IdElaborado);
            model.FormulaOculta = model.FormulaSecreta
                && !WebCore.Helpers.FormulaSecretaHelper.TieneElevacion(HttpContext.Session, model.IdElaborado);

            if (model.FormulaOculta)
            {
                model.Formula = new List<ElaboradoFormulaLineaVm>();
                model.Receta = "";
            }
        }

        // Idem AplicarSecretoFormulaRapida para la pantalla de Carga.
        private void AplicarSecretoFormulaCarga(ElaboradoCargaVm model)
        {
            if (model == null) return;

            model.FormulaSecreta = model.IdElaborado > 0 && _oCorteN.esFormulaSecreta(model.IdElaborado);
            model.FormulaOculta = model.FormulaSecreta
                && !WebCore.Helpers.FormulaSecretaHelper.TieneElevacion(HttpContext.Session, model.IdElaborado);

            if (model.FormulaOculta)
            {
                model.Formula = new List<ElaboradoFormulaLineaVm>();
                model.Receta = "";
            }
        }

        // Mismo criterio que CajasController.ObtenerSessionIdEstable: ASP.NET Core Session no manda el
        // Set-Cookie hasta el primer write, asi que sin esto el rate limit por sesion nunca acumula.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // Escribe un evento en auditoriaformulas. Puede lanzar: quien lo llama decide si falla cerrado
        // (VER: sin auditoria no se muestra la formula) o solo lo loguea (AuditarFormulaSinFallar).
        private void AuditarFormula(string tipo, int? idUsuario, int? idCorte, string detalle)
        {
            _oCorteN.registrarAuditoriaFormula(new Entidades.AuditoriaFormula
            {
                Tipo = tipo,
                IdUsuario = idUsuario,
                IdUsuarioSesion = _usuarioActual != null ? _usuarioActual.Id : (int?)null,
                IdCorte = idCorte,
                Ip = WebCore.Helpers.ClientIp.Obtener(HttpContext),
                Detalle = detalle
            });
        }

        private void AuditarFormulaSinFallar(string tipo, int? idUsuario, int? idCorte, string detalle)
        {
            try
            {
                AuditarFormula(tipo, idUsuario, idCorte, detalle);
            }
            catch (Exception ex)
            {
                // El evento ya ocurrio (fallo o "ocultar"): no se corta el flujo, pero queda en el log.
                HttpContext.RequestServices.GetRequiredService<ILogger<ElaboradosController>>()
                    .LogWarning(ex, "No se pudo registrar el evento {Tipo} de auditoria de formula (corte {IdCorte}).", tipo, idCorte);
            }
        }

        // Re-login para "Ver formula": usuario (elegido de la lista, igual que en Cierre de Caja) + clave de
        // alguien con permiso de ver/editar formulas.
        // Se pide SIEMPRE, aunque el usuario logueado ya tenga el permiso (decision 2026-10-04: la formula
        // secreta no se muestra por defecto en las pantallas operativas). idCorte = producto elaborado;
        // en el historial se manda idEmbutido (el registro producido) y el servidor resuelve el idCorte.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutorizarVerFormula(int idUsuario, string clave, int idCorte = 0, int idEmbutido = 0)
        {
            if (idCorte <= 0 && idEmbutido > 0)
            {
                var embutido = _oCorteN.findEmbutidoById(idEmbutido);
                idCorte = embutido != null && embutido.Corte != null ? embutido.Corte.IdCorte : 0;
            }

            if (idCorte <= 0)
                return Json(new { ok = false, msg = "Producto inválido." });

            // Si la formula no es secreta no hace falta re-login: la pantalla simplemente la pide.
            if (!_oCorteN.esFormulaSecreta(idCorte))
                return Json(new { ok = true, secreta = false });

            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.FormulaStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return Json(new { ok = false, bloqueado = true, segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds) });

            // Mensaje unico para cualquier falla: no revela si el usuario existe ni si tiene permiso.
            const string mensajeGenerico = "Usuario o contraseña incorrectos, o sin permiso para ver fórmulas.";

            // El selector manda el id; se resuelve a su identificador de login (getUsuarioById ya limita a la
            // empresa de la sesion). Si no existe, el identificador vacio cae en "Datos incompletos" (falla igual).
            var candidatoElegido = idUsuario > 0 ? _oUsuarioN.getUsuarioById(idUsuario) : null;
            var resultado = _oUsuarioN.AutorizarVerFormulaSecreta(candidatoElegido?.User ?? "", clave);
            if (!resultado.Autorizado)
            {
                WebCore.Helpers.FormulaStepUpRateLimiter.RegisterFailure(sessionId);
                AuditarFormulaSinFallar(Entidades.AuditoriaFormula.TipoFallo, resultado.IdUsuarioCandidato, idCorte, resultado.Motivo);
                return Json(new { ok = false, msg = mensajeGenerico });
            }

            // Falla cerrado: si no se puede dejar constancia de quien vio la formula, no se muestra.
            try
            {
                AuditarFormula(Entidades.AuditoriaFormula.TipoVer, resultado.Autorizador.Id, idCorte, null);
            }
            catch (Exception ex)
            {
                HttpContext.RequestServices.GetRequiredService<ILogger<ElaboradosController>>()
                    .LogError(ex, "No se pudo auditar el acceso a la formula secreta (corte {IdCorte}); se deniega.", idCorte);
                return Json(new { ok = false, msg = "No se pudo registrar el acceso. Intentá de nuevo o avisá al administrador." });
            }

            WebCore.Helpers.FormulaStepUpRateLimiter.Reset(sessionId);
            WebCore.Helpers.FormulaSecretaHelper.Registrar(HttpContext.Session, idCorte, resultado.Autorizador.Id);
            return Json(new
            {
                ok = true,
                secreta = true,
                idCorte,
                nombre = resultado.Autorizador.Nombre,
                segundos = (int)WebCore.Helpers.FormulaSecretaHelper.DuracionElevacion().TotalSeconds
            });
        }

        // "Ocultar": quita la elevacion de un producto (idCorte > 0) o de todos (idCorte = 0, al salir de
        // la pantalla). SIN [ValidateAntiForgeryToken] a proposito: se llama con navigator.sendBeacon() al
        // salir de la pagina, que no puede adjuntar el token (mismo criterio que RevocarAutorizacionCierre).
        // Es inocuo ante CSRF: solo oculta, nunca muestra.
        [HttpPost]
        public IActionResult OcultarFormula(int idCorte = 0)
        {
            var ocultadas = idCorte > 0
                ? (WebCore.Helpers.FormulaSecretaHelper.Revocar(HttpContext.Session, idCorte) ? new[] { idCorte } : Array.Empty<int>())
                : WebCore.Helpers.FormulaSecretaHelper.RevocarTodas(HttpContext.Session).ToArray();

            foreach (int id in ocultadas)
                AuditarFormulaSinFallar(Entidades.AuditoriaFormula.TipoOcultar, null, id, null);

            return Json(new { ok = true });
        }

        // Costeo de formula/carga/ingreso rapido (solo Usuario.Admin, ver docs del pedido
        // 2026-09-21): precio de compra mas reciente de un producto entre todos los proveedores.
        // obtenerCorteProveedor ya viene ordenado por fechaultimacompra DESC (CortePg.cs), asi que
        // la primera fila es la compra mas reciente.
        [HttpGet]
        public JsonResult ObtenerPrecioCompra(int idCorte)
        {
            if (idCorte <= 0)
                return Json(new { ok = false, mensaje = "Producto invalido." });

            try
            {
                // El precio de venta viaja siempre (aunque no haya referencia de compra): alimenta
                // el toggle Compra/Venta del cliente sin necesidad de otro request.
                var corte = _oCorteN.findCorteById(idCorte, false);
                float precioVenta = corte != null ? corte.PrecioKg : 0f;

                var dt = _oCorteN.obtenerCorteProveedor(idCorte) ?? new DataTable();
                if (dt.Rows.Count == 0)
                {
                    return Json(new
                    {
                        ok = true,
                        idCorte,
                        encontrado = false,
                        precioVenta,
                        mensaje = "No se encontro referencia de compra para este producto."
                    });
                }

                var row = dt.Rows[0];
                return Json(new
                {
                    ok = true,
                    idCorte,
                    encontrado = true,
                    precioCompra = ToFloat(row, "ultimoprecio", "UltimoPrecio"),
                    precioVenta,
                    proveedor = ToString(row, "razonsocial", "RazonSocial"),
                    fecha = ToDateString(row, "fechaultimacompra", "FechaUltimaCompra")
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

            // Port de Web/Controllers/ElaboradosController.cs:547-551.
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoFormula, DateTime.Today, idCreador))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
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
                // Formula secreta: capturar el valor previo ANTES de pisarlo (formula puede ser la misma
                // instancia que formulaActual) para auditar solo cuando cambia.
                bool eraSecreta = formulaActual != null && formulaActual.Secreta;
                formula.Secreta = model.Secreta;

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

                if (formula.Secreta != eraSecreta)
                {
                    AuditarFormulaSinFallar(
                        formula.Secreta ? Entidades.AuditoriaFormula.TipoMarcarSecreta : Entidades.AuditoriaFormula.TipoQuitarSecreta,
                        user.Id, embutido.IdCorte, null);
                }

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
                int idCreadorPermiso = user.Id;
                if (model.IdEmbutido > 0)
                {
                    embutidoOriginal = _oCorteN.findEmbutidoById(model.IdEmbutido);
                    if (embutidoOriginal == null || embutidoOriginal.IdEmbutido <= 0)
                        return Json(new { ok = false, mensaje = "No se encontro el elaborado original a modificar." });

                    if (embutidoOriginal.CreadoPor != null)
                        idCreadorPermiso = embutidoOriginal.CreadoPor.Id;
                }

                // Port de Web/Controllers/ElaboradosController.cs:724 (GuardarIngresoRapido).
                DateTime fechaPermisoRapido = embutidoOriginal != null ? embutidoOriginal.FechaEmbutido : (model != null ? model.FechaEmbutido : DateTime.Today);
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoEmbutidoRapido, fechaPermisoRapido, idCreadorPermiso))
                    return Json(new { ok = false, mensaje = "No tiene permisos para guardar elaborados." });

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

                return Json(new { ok = true, idEmbutido = embutido.idEmbutido, redirectUrl = Url.Action("Index", "Elaborados") });
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
                int idCreadorPermisoCarga = user.Id;
                if (model.IdEmbutido > 0)
                {
                    embutidoOriginal = _oCorteN.findEmbutidoById(model.IdEmbutido);
                    if (embutidoOriginal == null || embutidoOriginal.IdEmbutido <= 0)
                        return Json(new { ok = false, mensaje = "No se encontro el elaborado original a modificar." });

                    if (embutidoOriginal.CreadoPor != null)
                        idCreadorPermisoCarga = embutidoOriginal.CreadoPor.Id;
                }

                // Port de Web/Controllers/ElaboradosController.cs:830-832.
                DateTime fechaPermisoCarga = embutidoOriginal != null ? embutidoOriginal.FechaEmbutido : (model != null ? model.FechaEmbutido : DateTime.Today);
                if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Elaborado.IngresoEmbutido, fechaPermisoCarga, idCreadorPermisoCarga))
                    return Json(new { ok = false, mensaje = "No tiene permisos para guardar elaborados." });

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
                    idEmbutido = idEmbutido,
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
                var user = _usuarioActual;
                var embutido = _oCorteN.findEmbutidoById(idEmbutido);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return Json(new { ok = false, mensaje = "No se encontró el elaborado." });

                // Port de Web/Controllers/ElaboradosController.cs:919-924 -- el permiso depende
                // de si el elaborado se cargo por ingreso rapido (con formula) o carga manual.
                int idCreadorAnulacion = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                string permisoAnulacion = embutido.Corte != null && embutido.Corte.IngresoRapidoEmbutido
                    ? Entidades.Permisos.Elaborado.IngresoEmbutidoRapido
                    : Entidades.Permisos.Elaborado.IngresoEmbutido;
                if (!_oUsuarioN.tienePermiso(user, permisoAnulacion, embutido.FechaEmbutido, idCreadorAnulacion))
                    return Json(new { ok = false, mensaje = "No tiene permisos para anular este elaborado." });

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
            // Port de Web/Controllers/ElaboradosController.cs:952-956.
            if (!_oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Elaborado.IngresoEmbutidoRapido, DateTime.Today, _usuarioActual.Id))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

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
                // Formula secreta: el listado nunca muestra la receta (no hay re-login en esta pantalla).
                Receta = formula.Secreta ? "" : (formula.Receta ?? "")
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
                    CreadoPor = ToString(row, "Creado Por", "CreadoPor", "creadoPor"),
                    Actualizado = ToDateString(row, "Actualizado", "actualizado"),
                    ActualizadoPor = ToString(row, "Actualizado Por", "ActualizadoPor", "actualizadoPor"),
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
            lista = ColapsarLineasConFormulaOculta(lista);
            return lista.OrderByDescending(x => x.Fecha).ToList();
        }

        // Formula secreta: de cada registro producido (embutido) cuyo elaborado tiene formula secreta y
        // sin elevacion vigente, las lineas por ingrediente se reemplazan por UNA sola fila sin
        // ingrediente con el total de kg (asi no se reconstruye la formula y el total del listado no
        // cambia). Con elevacion vigente las lineas se muestran normal.
        private List<ElaboradoLineaResumenVm> ColapsarLineasConFormulaOculta(List<ElaboradoLineaResumenVm> lista)
        {
            if (lista == null || lista.Count == 0) return lista;

            var secretos = _oCorteN.ObtenerCortesDeEmbutidosConFormulaSecreta(lista.Select(x => x.Id));
            if (secretos.Count == 0) return lista;

            var resultado = new List<ElaboradoLineaResumenVm>();
            var yaColapsados = new HashSet<int>();

            foreach (var linea in lista)
            {
                if (!secretos.TryGetValue(linea.Id, out int idCorteElaborado))
                {
                    resultado.Add(linea);
                    continue;
                }

                linea.IdCorteElaborado = idCorteElaborado;
                if (WebCore.Helpers.FormulaSecretaHelper.TieneElevacion(HttpContext.Session, idCorteElaborado))
                {
                    resultado.Add(linea);
                    continue;
                }

                if (!yaColapsados.Add(linea.Id))
                    continue;

                linea.FormulaOculta = true;
                linea.CodigoIngrediente = 0;
                linea.Ingrediente = "";
                linea.Kgs = lista.Where(x => x.Id == linea.Id).Sum(x => x.Kgs);
                resultado.Add(linea);
            }

            return resultado;
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

        // Formula secreta: ids de producto (corte) de los ingredientes de un registro que NO se pueden mostrar
        // sin elevacion: los que el sistema agrega solo (AgregarAuto), todos los de la formula si es un
        // ingreso rapido, y el producto generico "Ajuste Formula". Los cargados a mano no entran.
        // Lo usan el detalle y el listado de Index, para que ambos oculten exactamente lo mismo.
        private HashSet<int> ObtenerIdsCorteIngredientesOcultos(Entidades.Embutido embutido)
        {
            int idCorteElaborado = embutido != null && embutido.Corte != null ? embutido.Corte.IdCorte : 0;
            bool ocultarTodaLaFormula = embutido != null && embutido.Corte != null && embutido.Corte.IngresoRapidoEmbutido;

            var formulaItems = MapFormula(_oCorteN.getFormulaEmbutido(idCorteElaborado) ?? new DataTable());
            var ids = new HashSet<int>(formulaItems
                .Where(x => ocultarTodaLaFormula || x.AgregarAuto)
                .Select(x => x.IdCorte));

            var productoAjuste = _oCorteN.ObtenerProductoAjusteFormula();
            if (productoAjuste != null && productoAjuste.IdCorte > 0)
                ids.Add(productoAjuste.IdCorte);

            return ids;
        }

        // Formula secreta en el listado de Index: de los registros cuyo elaborado tiene formula secreta y sin
        // elevacion vigente, la Cantidad pasa a ser solo la de los ingredientes visibles (marca
        // IngredientesOcultos para rotular "+ ingredientes"). Si no, el total filtrado a un solo registro
        // delataria cuanto pesan los ingredientes ocultos. Va ANTES de sumar TotalKg.
        private void AplicarSecretoCantidadElaborados(List<ElaboradoResumenVm> items)
        {
            if (items == null || items.Count == 0) return;

            var secretos = _oCorteN.ObtenerCortesDeEmbutidosConFormulaSecreta(items.Select(x => x.Id));
            if (secretos.Count == 0) return;

            foreach (var item in items)
            {
                if (!secretos.TryGetValue(item.Id, out int idCorteElaborado))
                    continue;

                if (WebCore.Helpers.FormulaSecretaHelper.TieneElevacion(HttpContext.Session, idCorteElaborado))
                    continue;

                var embutido = _oCorteN.findEmbutidoById(item.Id);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    continue;

                var idsOcultos = ObtenerIdsCorteIngredientesOcultos(embutido);
                item.Kgs = (embutido.CortesEnEmbutido ?? new List<Entidades.CortePorEmbutido>())
                    .Where(x => x != null && x.Corte != null && !idsOcultos.Contains(x.Corte.IdCorte))
                    .Sum(x => x.KgUtilizado);
                item.IngredientesOcultos = true;
            }
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
                EsIngresoRapido = embutido.Corte != null && embutido.Corte.IngresoRapidoEmbutido,
                IdCorte = embutido.Corte != null ? embutido.Corte.IdCorte : 0
            };

            // Formula secreta: sin elevacion vigente no se devuelve la receta y se ocultan SOLO los
            // ingredientes que salen de la formula (los que el sistema agrega solo; en un ingreso rapido,
            // todos los de la formula; mas el producto "Ajuste Formula"). Los ingredientes que el operador
            // cargo a mano siguen visibles, igual que en Carga. Una fila (sin cantidades, para no dar
            // pistas de la formula) avisa que hay ingredientes ocultos y ofrece el boton "Ver formula".
            detalle.FormulaSecreta = detalle.IdCorte > 0 && _oCorteN.esFormulaSecreta(detalle.IdCorte);
            HashSet<int> idsCorteOcultos = null;
            if (detalle.FormulaSecreta && !WebCore.Helpers.FormulaSecretaHelper.TieneElevacion(HttpContext.Session, detalle.IdCorte))
            {
                detalle.FormulaOculta = true;
                detalle.Receta = "";

                idsCorteOcultos = ObtenerIdsCorteIngredientesOcultos(embutido);
            }

            foreach (var linea in embutido.CortesEnEmbutido ?? new List<Entidades.CortePorEmbutido>())
            {
                if (linea == null || linea.Corte == null)
                    continue;

                if (idsCorteOcultos != null && idsCorteOcultos.Contains(linea.Corte.IdCorte))
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
                    Secreta = formula.Secreta,
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
                PrecioActualElaborado = formula.Embutido != null ? formula.Embutido.PrecioKg : 0f,
                EtiquetaValorFormula = escalaUnidad ? "Unidad" : "Porcentaje",
                EscalaUnidad = escalaUnidad,
                AjustarUnidad = formula.AjustarUnidad,
                Secreta = formula.Secreta,
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

            // Costeo (solo Usuario.Admin): este metodo se llama tambien en todos los redisplays
            // de GuardarFormula tras un error de validacion, asi que es el unico punto que
            // garantiza el flag correcto en esos casos.
            model.EsAdmin = _usuarioActual.Admin;

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
            model.PrecioActualElaborado = elaborado.PrecioKg;
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

                // Porcentaje negativo permitido a propósito: las fórmulas llevan líneas de ajuste con el
                // producto en negativo (resta del total). No se bloquea el guardado por eso.
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

        // Lista del selector de "Ver fórmula": solo usuarios activos con permiso sobre fórmulas (mismo
        // criterio que AutorizarVerFormulaSecreta). Distinta de ObtenerUsuariosActivosEmpresaParaCombo,
        // que alimenta tambien el selector del operador de produccion y debe traer a todos.
        private List<object> ObtenerUsuariosConPermisoFormulaParaCombo()
        {
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            return oUsuarioN.ObtenerUsuariosConPermisoVerFormula()
                .Where(u => u.Id > 0 && !string.IsNullOrWhiteSpace(u.Nombre))
                .Select(u => (object)new { id = u.Id, nombre = u.Nombre })
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
