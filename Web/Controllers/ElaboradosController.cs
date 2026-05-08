using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Utilidades;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class ElaboradosController : BaseController
    {
        private Negocio.Corte oCorteN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oCorteN = new Negocio.Corte(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
        }

        public ActionResult Index(int? idSucursal = null, string elaborado = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = Session["Usuario"] as Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = NormalizarFechaDesde(fechaDesde ?? DateTime.Today.AddDays(-param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0)));
            DateTime hasta = NormalizarFechaHasta(fechaHasta ?? DateTime.Today);

            if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.VerEmbutidos, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            int sucursalSeleccionada = idSucursal ?? (user.IdSucursal > 0 ? user.IdSucursal : 0);
            DataTable dt = oCorteN.buscarEmbutido(sucursalSeleccionada > 0 ? sucursalSeleccionada : -1, (elaborado ?? "").Trim(), desde, hasta) ?? new DataTable();

            var model = new ElaboradoIndexVm
            {
                IdSucursal = sucursalSeleccionada,
                Elaborado = (elaborado ?? "").Trim(),
                FechaDesde = desde,
                FechaHasta = hasta,
                Items = MapElaborados(dt),
                Detalles = ConstruirDetallesElaborados(dt),
                Tabs = BuildTabs("Index")
            };

            model.TotalKg = model.Items
                .Where(x => string.IsNullOrWhiteSpace(x.Estado))
                .Sum(x => x.Kgs);

            ViewBag.Title = "Elaborados";
            ViewBag.Seccion = "Elaborados";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(oSucursalN.findAll() ?? new List<Sucursal>());

            return View("~/Views/Elaborados/Index.cshtml", model);
        }

        public ActionResult Lineas(int? idSucursal = null, string descripcion = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = Session["Usuario"] as Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = NormalizarFechaDesde(fechaDesde ?? DateTime.Today.AddDays(-param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0)));
            DateTime hasta = NormalizarFechaHasta(fechaHasta ?? DateTime.Today);

            if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.VerEmbutidos, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            int sucursalSeleccionada = idSucursal ?? (user.IdSucursal > 0 ? user.IdSucursal : 0);
            DataTable dt = oCorteN.obtenerLineasEmb(sucursalSeleccionada > 0 ? sucursalSeleccionada : -1, (descripcion ?? "").Trim(), desde, hasta) ?? new DataTable();

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
            ViewBag.Seccion = "Elaborados";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(oSucursalN.findAll() ?? new List<Sucursal>());

            return View("~/Views/Elaborados/Lineas.cshtml", model);
        }

        public ActionResult Formulas(string descripcion = "")
        {
            var user = Session["Usuario"] as Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.VerFormulas, DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            DataTable dt = oCorteN.buscarFormula((descripcion ?? "").Trim()) ?? new DataTable();
            bool puedeEditar = PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoFormula, DateTime.Today, user.Id);
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
            ViewBag.Seccion = "Elaborados";

            return View("~/Views/Elaborados/Formulas.cshtml", model);
        }

        public ActionResult EditarFormula(int id = 0)
        {
            var user = Session["Usuario"] as Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoFormula, DateTime.Today, user.Id))
            {
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            ElaboradoFormulaEditVm model;
            if (id > 0)
            {
                var formula = oCorteN.findFormulaByID(id, 0);
                if (formula == null || formula.IdFormula <= 0)
                    return HttpNotFound("No se encontró la fórmula.");

                int idCreador = formula.CreadoPor != null ? formula.CreadoPor.Id : user.Id;
                if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoFormula, DateTime.Today, idCreador))
                {
                    ViewBag.Seccion = "Elaborados";
                    return View("~/Views/Shared/AccesoDenegado.cshtml");
                }

                model = CrearViewModelFormulaEdicion(formula, user);
            }
            else
            {
                model = new ElaboradoFormulaEditVm
                {
                    UsuarioNombre = user.Nombre ?? "",
                    Tabs = BuildTabs("Formulas")
                };
            }

            ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
            ViewBag.Seccion = "Elaborados";
            return View("~/Views/Elaborados/EditarFormula.cshtml", model);
        }

        public ActionResult IngresoRapido()
        {
            return View("~/Views/Elaborados/Placeholder.cshtml", CrearPlaceholder(
                "Ingreso rapido",
                "La base del modulo ya quedo preparada para esta seccion.",
                "En la proxima fase se implementara como modal, tomando automaticamente el usuario logueado y reutilizando la logica de busqueda y balanza de Web."
            ));
        }

        public ActionResult Desarme()
        {
            return View("~/Views/Elaborados/Placeholder.cshtml", CrearPlaceholder(
                "Desarme de elaborado",
                "Esta seccion quedo reservada dentro del modulo web para respetar la navegacion del WinForms.",
                "La implementacion siguiente debe reutilizar el flujo de ingreso rapido con cantidades negativas y observacion de desarme, sin tocar la logica existente de stock."
            ));
        }

        public ActionResult Carga(int id = 0)
        {
            var user = Session["Usuario"] as Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            ElaboradoCargaVm model;
            if (id > 0)
            {
                var embutido = oCorteN.findEmbutidoById(id);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return HttpNotFound("No se encontró el elaborado.");

                int idCreador = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoEmbutido, embutido.FechaEmbutido, idCreador))
                {
                    ViewBag.Seccion = "Elaborados";
                    return View("~/Views/Shared/AccesoDenegado.cshtml");
                }

                model = CrearViewModelEdicion(embutido, user);
            }
            else
            {
                if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoEmbutido, DateTime.Today, user.Id))
                {
                    ViewBag.Seccion = "Elaborados";
                    return View("~/Views/Shared/AccesoDenegado.cshtml");
                }

                model = new ElaboradoCargaVm
                {
                    IdSucursal = user.IdSucursal > 0 ? user.IdSucursal : 0,
                    FechaEmbutido = DateTime.Now,
                    UsuarioNombre = user.Nombre ?? "",
                    Tabs = BuildTabs("Carga"),
                    PermiteGuardarEdicion = true
                };
            }

            ViewBag.Title = model.EsEdicion ? "Modificar elaborado" : "Carga / ingreso de elaborado";
            ViewBag.Seccion = "Elaborados";
            ViewBag.Sucursales = oSucursalN.findAll() ?? new List<Sucursal>();
            ViewBag.UrlBuscarProducto = Url.Action("BuscarProducto", "Elaborados");
            ViewBag.UrlBuscarProductoPorCodigo = Url.Action("BuscarProductoPorCodigo", "Elaborados");
            ViewBag.UrlObtenerFormula = Url.Action("ObtenerFormula", "Elaborados");
            ViewBag.UrlGuardar = Url.Action("GuardarCarga", "Elaborados");

            return View("~/Views/Elaborados/Carga.cshtml", model);
        }

        [HttpGet]
        public JsonResult BuscarProducto(string q = "")
        {
            try
            {
                var productos = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();
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
                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult BuscarProductoPorCodigo(long? codigo)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
                return Json(new { ok = false, mensaje = "Codigo invalido." }, JsonRequestBehavior.AllowGet);

            var corte = oCorteN.findCorteByCodigo(codigo.Value, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "No se encontro el producto." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ok = true,
                id = corte.IdCorte,
                codigo = corte.Codigo,
                nombre = !string.IsNullOrWhiteSpace(corte.CorteDesc) ? corte.CorteDesc : corte.corte,
                tipo = corte.Tipo ?? "",
                promedio = corte.Promedio,
                ingresoRapido = corte.IngresoRapidoEmbutido
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ObtenerFormula(int idCorte)
        {
            try
            {
                var formula = oCorteN.findFormulaByID(0, idCorte);
                var dtFormula = oCorteN.getFormulaEmbutido(idCorte) ?? new DataTable();

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
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarFormula(ElaboradoFormulaEditVm model)
        {
            var user = Session["Usuario"] as Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            int idCreador = user.Id;
            Entidades.Formula formulaActual = null;
            if (model != null && model.IdFormula > 0)
            {
                formulaActual = oCorteN.findFormulaByID(model.IdFormula, 0);
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

            if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoFormula, DateTime.Today, idCreador))
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
                RecalcularTotalesFormula(model);
                ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Elaborados/EditarFormula.cshtml", model);
            }

            try
            {
                var embutido = oCorteN.findCorteById(model.IdElaborado, false);
                if (embutido == null || embutido.IdCorte <= 0)
                {
                    ModelState.AddModelError("", "No se encontró el elaborado seleccionado.");
                    model.Tabs = BuildTabs("Formulas");
                    model.UsuarioNombre = user.Nombre ?? "";
                    RecalcularTotalesFormula(model);
                    ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                    ViewBag.Seccion = "Elaborados";
                    return View("~/Views/Elaborados/EditarFormula.cshtml", model);
                }

                var existenteMismoElaborado = oCorteN.findFormulaByID(0, model.IdElaborado);
                if (existenteMismoElaborado != null && existenteMismoElaborado.IdFormula > 0 && existenteMismoElaborado.IdFormula != model.IdFormula)
                {
                    ModelState.AddModelError("", "El elaborado ya posee una fórmula. Modifique la existente.");
                    model.Tabs = BuildTabs("Formulas");
                    model.UsuarioNombre = user.Nombre ?? "";
                    RecalcularTotalesFormula(model);
                    ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                    ViewBag.Seccion = "Elaborados";
                    return View("~/Views/Elaborados/EditarFormula.cshtml", model);
                }

                var formula = formulaActual ?? new Entidades.Formula();
                formula.IdFormula = model.IdFormula;
                formula.Embutido = embutido;
                formula.Receta = (model.Receta ?? "").Trim();
                formula.CreadoPor = formulaActual != null ? formulaActual.CreadoPor : user;
                formula.ActualizadoPor = formulaActual != null ? user : null;

                var lineas = new List<Entidades.CortePorFormula>();
                foreach (var linea in model.Lineas ?? new List<ElaboradoFormulaEditLineaVm>())
                {
                    var corte = oCorteN.findCorteById(linea.IdCorte, false);
                    if (corte == null || corte.IdCorte <= 0)
                    {
                        ModelState.AddModelError("", "No se encontró el ingrediente " + (linea.Producto ?? "") + ".");
                        model.Tabs = BuildTabs("Formulas");
                        model.UsuarioNombre = user.Nombre ?? "";
                        RecalcularTotalesFormula(model);
                        ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                        ViewBag.Seccion = "Elaborados";
                        return View("~/Views/Elaborados/EditarFormula.cshtml", model);
                    }

                    lineas.Add(new Entidades.CortePorFormula
                    {
                        Formula = formula,
                        CorteEnFormula = corte,
                        Porcentaje = linea.Porcentaje,
                        AgregarAuto = linea.AgregarAuto
                    });
                }

                formula.IdFormula = oCorteN.addOrEditFormula(formula, lineas);
                TempData["ElaboradosSuccessMessage"] = model.IdFormula > 0
                    ? "La fórmula se guardó correctamente."
                    : "La fórmula se registró correctamente.";
                return RedirectToAction("Formulas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No se pudo guardar la fórmula. " + ex.Message);
                model.Tabs = BuildTabs("Formulas");
                model.UsuarioNombre = user.Nombre ?? "";
                RecalcularTotalesFormula(model);
                ViewBag.Title = model.EsEdicion ? "Modificar fórmula" : "Nueva fórmula";
                ViewBag.Seccion = "Elaborados";
                return View("~/Views/Elaborados/EditarFormula.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarFormula(int idFormula)
        {
            try
            {
                var user = Session["Usuario"] as Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesion invalida." });

                var formula = oCorteN.findFormulaByID(idFormula, 0);
                if (formula == null || formula.IdFormula <= 0)
                    return Json(new { ok = false, mensaje = "No se encontró la fórmula." });

                int idCreador = formula.CreadoPor != null ? formula.CreadoPor.Id : user.Id;
                if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoFormula, DateTime.Today, idCreador))
                    return Json(new { ok = false, mensaje = "No tiene permisos para eliminar esta fórmula." });

                oCorteN.eliminarFormula(idFormula);
                TempData["ElaboradosSuccessMessage"] = "La fórmula se eliminó correctamente.";
                return Json(new { ok = true, redirectUrl = Url.Action("Formulas", "Elaborados") });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarCarga(ElaboradoCargaVm model)
        {
            try
            {
                var user = Session["Usuario"] as Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesion invalida." });

                if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoEmbutido, model != null ? model.FechaEmbutido : DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para guardar elaborados." });

                if (model == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos del elaborado." });

                if (model.IdEmbutido > 0)
                    return Json(new { ok = false, mensaje = "La modificación del elaborado existente todavía no está habilitada en Web sin cambios adicionales de capa." });

                string error = ValidarCarga(model);
                if (!string.IsNullOrWhiteSpace(error))
                    return Json(new { ok = false, mensaje = error });

                var corteElaborado = oCorteN.findCorteById(model.IdElaborado, false);
                if (corteElaborado == null || corteElaborado.IdCorte <= 0)
                    return Json(new { ok = false, mensaje = "El elaborado seleccionado no es valido." });

                var embutido = new Entidades.Embutido
                {
                    FechaEmbutido = model.FechaEmbutido,
                    Corte = corteElaborado,
                    Sucursal = new Entidades.Sucursal { IdSucursal = model.IdSucursal },
                    Observaciones = model.Observaciones ?? "",
                    CreadoPor = user
                };

                int idEmbutido = oCorteN.agregarEmbutido(embutido);
                if (idEmbutido <= 0)
                    return Json(new { ok = false, mensaje = "No se pudo registrar el elaborado." });

                embutido.IdEmbutido = idEmbutido;

                foreach (var item in BuildIngredientesAutomaticos(model, embutido))
                {
                    oCorteN.agregarCortePorEmbutido(item);
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

                    oCorteN.agregarCortePorEmbutido(item);
                }

                float cantidadRegistrada = CalcularCantidadRegistrada(model, corteElaborado);
                string unidadTexto = corteElaborado.Pesable ? "kgs" : "unidades";
                string nombreElaborado = !string.IsNullOrWhiteSpace(corteElaborado.CorteDesc) ? corteElaborado.CorteDesc : corteElaborado.corte;
                TempData["ElaboradosSuccessMessage"] = "Se guardó correctamente " +
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
                var user = Session["Usuario"] as Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesion invalida." });

                var embutido = oCorteN.findEmbutidoById(idEmbutido);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    return Json(new { ok = false, mensaje = "No se encontró el elaborado." });

                int idCreador = embutido.CreadoPor != null ? embutido.CreadoPor.Id : user.Id;
                if (!PermisosHelper.TienePermiso(Session, Permisos.Elaborado.IngresoEmbutido, embutido.FechaEmbutido, idCreador))
                    return Json(new { ok = false, mensaje = "No tiene permisos para anular este elaborado." });

                if (string.Equals(embutido.Estado ?? "", "Anulado", StringComparison.OrdinalIgnoreCase))
                    return Json(new { ok = false, mensaje = "El elaborado ya se encuentra anulado." });

                embutido.ActualizadoPor = user;
                oCorteN.anularEmbutido(embutido);
                TempData["ElaboradosSuccessMessage"] = "El elaborado se anuló correctamente.";

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

        private ElaboradoPlaceholderVm CrearPlaceholder(string titulo, string descripcion, string nota)
        {
            ViewBag.Title = titulo;
            ViewBag.Seccion = "Elaborados";

            return new ElaboradoPlaceholderVm
            {
                Titulo = titulo,
                Descripcion = descripcion,
                Nota = nota,
                Tabs = BuildTabs(ControllerContext.RouteData.Values["action"].ToString())
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
                    Codigo = ToLong(row, "Cod.Emb", "CodEmb", "codigoEmbutido", "CodigoEmbutido", "Codigo", "codigo"),
                    Elaborado = ToString(row, "Embutido", "Elaborado", "corte"),
                    Kgs = kgs,
                    Observaciones = observaciones,
                    Estado = ToString(row, "Estado", "estado"),
                    Creado = ToDateString(row, "Creado", "creado"),
                    Actualizado = ToDateString(row, "Actualizado", "actualizado"),
                    EsDesarme = !string.IsNullOrWhiteSpace(observaciones)
                        && observaciones.ToLowerInvariant().Contains("desarme")
                        && kgs < 0
                });
            }

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
                    Observaciones = ToString(row, "Observaciones", "observaciones")
                });
            }

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

        private Dictionary<int, ElaboradoDetalleVm> ConstruirDetallesElaborados(DataTable dt)
        {
            var detalles = new Dictionary<int, ElaboradoDetalleVm>();
            if (dt == null)
                return detalles;

            foreach (DataRow row in dt.Rows)
            {
                int idEmbutido = ToInt(row, "Id", "idEmbutido");
                if (idEmbutido <= 0 || detalles.ContainsKey(idEmbutido))
                    continue;

                var embutido = oCorteN.findEmbutidoById(idEmbutido);
                if (embutido == null || embutido.IdEmbutido <= 0)
                    continue;

                var formula = oCorteN.findFormulaByID(0, embutido.Corte != null ? embutido.Corte.IdCorte : 0);
                detalles[idEmbutido] = new ElaboradoDetalleVm
                {
                    Id = embutido.IdEmbutido,
                    Fecha = embutido.FechaEmbutido,
                    Sucursal = embutido.Sucursal != null ? embutido.Sucursal.SucursalNombre : "",
                    Codigo = embutido.Corte != null ? embutido.Corte.Codigo : 0,
                    Elaborado = embutido.Corte != null ? embutido.Corte.CorteDesc : "",
                    Kgs = ToFloat(row, "Kgs", "kgs"),
                    Observaciones = embutido.Observaciones ?? "",
                    Estado = embutido.Estado ?? "",
                    Receta = formula != null ? (formula.Receta ?? "") : "",
                    UsuarioCreacion = embutido.CreadoPor != null ? embutido.CreadoPor.Nombre : "",
                    FechaCreacion = embutido.Creado,
                    UsuarioActualizacion = embutido.ActualizadoPor != null ? embutido.ActualizadoPor.Nombre : "",
                    FechaActualizacion = embutido.Actualizado
                };
            }

            return detalles;
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

                var formula = oCorteN.findFormulaByID(idFormula, 0);
                if (formula == null || formula.IdFormula <= 0)
                    continue;

                var detalle = new ElaboradoFormulaDetalleVm
                {
                    IdFormula = formula.IdFormula,
                    Codigo = formula.Embutido != null ? formula.Embutido.Codigo : 0,
                    Elaborado = formula.Embutido != null ? formula.Embutido.CorteDesc : "",
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
                        Porcentaje = item.Porcentaje,
                        AgregarAuto = item.AgregarAuto
                    });
                }

                detalles[idFormula] = detalle;
            }

            return detalles;
        }

        private List<ElaboradoTabVm> BuildTabs(string activeAction)
        {
            return new List<ElaboradoTabVm>
            {
                new ElaboradoTabVm { Titulo = "Elaborados", Action = "Index", Activo = string.Equals(activeAction, "Index", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Lineas de elaborado", Action = "Lineas", Activo = string.Equals(activeAction, "Lineas", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Ingreso rapido", Action = "IngresoRapido", Activo = string.Equals(activeAction, "IngresoRapido", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Formulas", Action = "Formulas", Activo = string.Equals(activeAction, "Formulas", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Desarme de elaborado", Action = "Desarme", Activo = string.Equals(activeAction, "Desarme", StringComparison.OrdinalIgnoreCase) },
                new ElaboradoTabVm { Titulo = "Carga / ingreso", Action = "Carga", Activo = string.Equals(activeAction, "Carga", StringComparison.OrdinalIgnoreCase) }
            };
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
                ingresoRapido = p.IngresoRapidoEmbutido
            };
        }

        private ElaboradoFormulaEditVm CrearViewModelFormulaEdicion(Entidades.Formula formula, Usuario user)
        {
            var model = new ElaboradoFormulaEditVm
            {
                IdFormula = formula.IdFormula,
                EsEdicion = true,
                SoloLecturaInicial = true,
                IdElaborado = formula.Embutido != null ? formula.Embutido.IdCorte : 0,
                CodigoElaborado = formula.Embutido != null ? formula.Embutido.Codigo : 0,
                Elaborado = formula.Embutido != null ? formula.Embutido.CorteDesc : "",
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
                    Porcentaje = item.Porcentaje,
                    AgregarAuto = item.AgregarAuto
                });
            }

            RecalcularTotalesFormula(model);
            return model;
        }

        private ElaboradoCargaVm CrearViewModelEdicion(Entidades.Embutido embutido, Usuario user)
        {
            var model = new ElaboradoCargaVm
            {
                IdEmbutido = embutido.IdEmbutido,
                EsEdicion = true,
                PermiteGuardarEdicion = false,
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

            var formula = oCorteN.findFormulaByID(0, model.IdElaborado);
            model.Receta = formula != null ? (formula.Receta ?? "") : "";

            var formulaItems = MapFormula(oCorteN.getFormulaEmbutido(model.IdElaborado) ?? new DataTable());
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

        private List<Entidades.CortePorEmbutido> BuildIngredientesAutomaticos(ElaboradoCargaVm model, Entidades.Embutido embutido)
        {
            var lista = new List<Entidades.CortePorEmbutido>();
            var dtFormula = oCorteN.getFormulaEmbutido(model.IdElaborado) ?? new DataTable();
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

        private List<Sucursal> ConstruirSucursalesConTodas(List<Sucursal> sucursales)
        {
            var lista = new List<Sucursal>
            {
                new Sucursal { IdSucursal = 0, SucursalNombre = "Todas" }
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
                    return Convert.ToString(value);
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
                string text = Convert.ToString(raw);
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
            return fecha.Date;
        }

        private static DateTime NormalizarFechaHasta(DateTime fecha)
        {
            return fecha.Date.AddDays(1).AddSeconds(-1);
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
                model.TotalPorcentaje += linea.Porcentaje;
            }

            model.TotalUnidades = model.TotalPorcentaje / 100f;
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
    }
}
