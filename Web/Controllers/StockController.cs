using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class StockController : BaseController
    {
        private class ProductoNoCargadoCierreVm
        {
            public int IdCorte { get; set; }
            public long Codigo { get; set; }
            public string Producto { get; set; }
            public float StockActual { get; set; }
        }

        private class TablaModalStockVm
        {
            public List<ColumnaModalStockVm> columnas { get; set; }
            public List<List<string>> filas { get; set; }

            public TablaModalStockVm()
            {
                columnas = new List<ColumnaModalStockVm>();
                filas = new List<List<string>>();
            }
        }

        private class ColumnaModalStockVm
        {
            public string nombre { get; set; }
            public bool oculta { get; set; }
            public bool alineacionDerecha { get; set; }
            public bool formatoTresDecimales { get; set; }
        }

        private Negocio.Compra oCompraN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Corte oCorteN;
        private Negocio.Persona oPersonaN;

        private static readonly string[] TiposStock =
        {
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock)
        };

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oCompraN = new Negocio.Compra(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
            oUsuarioN = new Negocio.Usuario(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
            oPersonaN = new Negocio.Persona(empresa, param);
        }

        public ActionResult Index(int? idSucursal = null, string tipoCompra = "Ver Todos", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.VerStock, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            int sucursalSeleccionada = idSucursal.HasValue ? idSucursal.Value : (user.IdSucursal > 0 ? user.IdSucursal : 0);
            string tipoNormalizado = NormalizarTipoFiltro(tipoCompra);

            DataTable dt = oCompraN.obtenerCompras(sucursalSeleccionada, tipoNormalizado, "", desde.Date, hasta.Date, null) ?? new DataTable();
            dt = FiltrarSoloStock(dt);

            var model = new CompraIndexVm
            {
                Compras = dt,
                Detalles = ConstruirDetallesIndex(dt)
            };

            ViewBag.Title = "Stock";
            ViewBag.Seccion = "Stock";
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.IdSucursal = sucursalSeleccionada;
            ViewBag.TipoCompra = tipoNormalizado;
            ViewBag.FechaDesde = desde;
            ViewBag.FechaHasta = hasta;
            ViewBag.TotalKg = CalcularTotalKg(dt);

            return View("~/Views/Stock/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult ExistenciaPorSucursales()
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime fechaPermiso = DateTime.Today;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.VerStock, fechaPermiso, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            var model = new Entidades.ExistenciaPorSucursalesVm();

            try
            {
                model.Filtro = CrearFiltroExistencia(user);
                model.ConsultaRealizada = false;
                model.Mensaje = "Presioná Buscar para consultar stock.";
            }
            catch (Exception ex)
            {
                model.Filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();
                model.ConsultaRealizada = false;
                model.Mensaje = "No se pudieron cargar todos los filtros de la pantalla. " + ex.Message;
            }

            ViewBag.Title = "Existencia por sucursales";
            ViewBag.Seccion = "Stock";

            return View("~/Views/Stock/ExistenciaPorSucursales.cshtml", model);
        }

        [HttpGet]
        public PartialViewResult BuscarExistenciaPorSucursales(
            string texto = "",
            int idSucursal = 0,
            DateTime? fechaHasta = null,
            string tipo = "",
            int idProveedor = 0,
            int idMarca = 0,
            bool soloConStock = false)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            var model = new Entidades.ExistenciaPorSucursalesVm();

            if (user == null)
            {
                model.Filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();
                model.ConsultaRealizada = true;
                model.Mensaje = "Sesión inválida.";
                return PartialView("~/Views/Stock/_TablaExistenciaPorSucursales.cshtml", model);
            }

            DateTime fechaPermiso = fechaHasta ?? DateTime.Now;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.VerStock, fechaPermiso, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                model.Filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();
                model.ConsultaRealizada = true;
                model.Mensaje = "No tiene permisos para consultar stock.";
                return PartialView("~/Views/Stock/_TablaExistenciaPorSucursales.cshtml", model);
            }

            try
            {
                var filtro = CrearFiltroExistencia(user);
                filtro.Texto = (texto ?? "").Trim();
                filtro.IdSucursal = idSucursal > 0 ? idSucursal : 0;
                filtro.FechaHasta = fechaHasta;
                filtro.Tipo = (tipo ?? "").Trim();
                filtro.IdProveedor = idProveedor;
                filtro.IdMarca = idMarca;
                filtro.SoloConStock = soloConStock;

                model = oCorteN.ObtenerMatrizExistenciaPorSucursales(filtro);
                model.Filtro = filtro;
            }
            catch (Exception ex)
            {
                model.Filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();
                model.ConsultaRealizada = true;
                model.Mensaje = "Error al consultar la existencia por sucursales. " + ex.Message;
            }

            return PartialView("~/Views/Stock/_TablaExistenciaPorSucursales.cshtml", model);
        }

        public ActionResult Nuevo(string tipoCompra)
        {
            string tipoNormalizado = NormalizarTipoOperacion(tipoCompra);
            if (string.IsNullOrWhiteSpace(tipoNormalizado))
                return RedirectToAction("Index");

            return RedirectToAction("Editar", new { id = 0, tipoCompra = tipoNormalizado });
        }

        public ActionResult Editar(int id = 0, string tipoCompra = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            Entidades.Compra compra = null;
            if (id > 0)
            {
                compra = oCompraN.findById_convertToCompra(id);
                if (compra == null || compra.IdCompra == 0)
                    return HttpNotFound("No se encontró el movimiento de stock.");
            }

            string tipoOperacion = compra != null ? compra.TipoCompra : NormalizarTipoOperacion(tipoCompra);
            if (string.IsNullOrWhiteSpace(tipoOperacion) || !EsTipoStock(tipoOperacion))
                return RedirectToAction("Index");

            DateTime fechaPermiso = compra != null ? compra.FechaCompra : DateTime.Today;
            int idCreador = compra != null && compra.CreadoPor != null ? compra.CreadoPor.Id : user.Id;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.AddOrEditStock, fechaPermiso, idCreador))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (EsAjuste(tipoOperacion) && (user == null || !user.Admin))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para realizar Ajuste de Stock.";
                return RedirectToAction("Index");
            }

            var model = compra != null ? CrearViewModelEdicion(compra, user) : CrearViewModelNuevo(user, tipoOperacion);
            CargarViewBags(model);

            return View("~/Views/Stock/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(StockEditVm model)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (model == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Error";
                TempData["AlertMsg"] = "No se recibieron datos para guardar.";
                return RedirectToAction("Index");
            }

            string tipoOperacion = NormalizarTipoOperacion(model.TipoCompra);
            Entidades.Compra compraActual = null;
            if (model.IdCompra > 0)
            {
                compraActual = oCompraN.findById_convertToCompra(model.IdCompra);
                if (compraActual == null || compraActual.IdCompra == 0)
                {
                    TempData["AlertType"] = "error";
                    TempData["AlertTitle"] = "No encontrado";
                    TempData["AlertMsg"] = "No se encontró el movimiento de stock a modificar.";
                    return RedirectToAction("Index");
                }

            }

            if (string.IsNullOrWhiteSpace(tipoOperacion) && compraActual != null)
                tipoOperacion = compraActual.TipoCompra;

            model.TipoCompra = tipoOperacion;
            NormalizarDecimalesPosteados(model);
            string error = ValidarModelo(model, user);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError("", error);
                CargarViewBags(model);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }

            DateTime fechaPermiso = model.FechaCompra;
            int idCreador = compraActual != null && compraActual.CreadoPor != null ? compraActual.CreadoPor.Id : user.Id;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.AddOrEditStock, fechaPermiso, idCreador))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            Entidades.Sucursal sucursal = oSucursalN.findById(model.IdSucursal);
            if (sucursal == null || sucursal.IdSucursal <= 0)
            {
                ModelState.AddModelError("", "Seleccione una sucursal válida.");
                CargarViewBags(model);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }

            int idProveedor = EsPesaje(tipoOperacion) ? model.IdProveedor : param.GetInt(Entidades.ParamKeys.IdIndefinido, 0);
            Entidades.Persona proveedor = ResolverProveedor(idProveedor);
            if (proveedor == null || proveedor.IdPersona <= 0)
            {
                ModelState.AddModelError("", "No se pudo resolver la persona para este movimiento.");
                CargarViewBags(model);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }

            var compra = compraActual ?? new Entidades.Compra();
            compra.IdCompra = model.IdCompra;
            compra.TipoCompra = tipoOperacion;
            compra.NroRemito = compraActual != null ? compraActual.NroRemito ?? "" : "";
            compra.FechaCompra = model.FechaCompra;
            compra.Proveedor = proveedor;
            compra.CantMedias = model.CantMedias;
            compra.KgsMedias = model.KgsMedias;
            compra.Observaciones = (model.Observaciones ?? string.Empty).Trim();
            compra.Sucursal = sucursal;
            compra.EnCtaCte = false;
            compra.Estado = compraActual != null ? compraActual.Estado ?? "" : "";
            compra.CreadoPor = compraActual != null ? compraActual.CreadoPor : user;
            compra.ActualizadoPor = compraActual != null ? user : null;

            var lineas = new List<Entidades.CortePorCompra>();
            int index = 0;
            foreach (var linea in model.Lineas ?? new List<StockLineaVm>())
            {
                index++;
                var corte = oCorteN.findCorteById(linea.IdCorte ?? 0, false);
                if (corte == null || corte.IdCorte <= 0)
                {
                    ModelState.AddModelError("", "No se encontró el producto de la línea " + index + ".");
                    CargarViewBags(model);
                    RecalcularTotales(model);
                    return View("~/Views/Stock/Editar.cshtml", model);
                }

                float cantidad = linea.CantKgs;
                if (EsEgreso(tipoOperacion) && cantidad > 0)
                    cantidad = cantidad * -1;

                lineas.Add(new Entidades.CortePorCompra
                {
                    Compra = compra,
                    Corte = corte,
                    CantKgs = cantidad,
                    precioKg = 0,
                    PrecioVenta = 0,
                    Margen = 0,
                    Desc_recargo = 0,
                    Iva_compra = 0,
                    Balanza = linea.Balanza,
                    Sucursal = sucursal,
                    Creado = DateTime.Now,
                    CreadoPor = user
                });
            }

            try
            {
                oCompraN.AddOrEditCompra(compra, compra.TipoCompra, null, lineas, false, null);
                TempData["StockSuccessMessage"] = model.IdCompra > 0
                    ? "El movimiento de stock se guardó correctamente."
                    : "El movimiento de stock se registró correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar el movimiento de stock. " + ex.Message);
                CargarViewBags(model);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }
        }

        [HttpGet]
        public JsonResult BuscarCorte(string q = "")
        {
            try
            {
                var productos = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    string filtro = q.Trim();
                    productos = productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.CorteDesc) && p.CorteDesc.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        p.Codigo.ToString().IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                var resultado = productos.Take(200).Select(p => new
                {
                    id = p.IdCorte,
                    codigo = p.Codigo,
                    nombre = p.CorteDesc,
                    tipo = p.Tipo ?? "",
                    promedio = p.Promedio,
                    pesable = p.Pesable
                }).ToList();

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult BuscarCortePorCodigo(long? codigo)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
                return Json(new { ok = false, mensaje = "Código inválido." }, JsonRequestBehavior.AllowGet);

            var corte = oCorteN.findCorteByCodigo(codigo.Value, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el producto." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ok = true,
                id = corte.IdCorte,
                codigo = corte.Codigo,
                nombre = corte.CorteDesc,
                tipo = corte.Tipo ?? "",
                promedio = corte.Promedio,
                pesable = corte.Pesable
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProductosNoCargadosCierre(int idSucursal, DateTime fechaCompra, int idCompra = 0, long[] codigosCargados = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesión inválida." });

            DateTime fechaConsulta = fechaCompra == DateTime.MinValue ? DateTime.Today : fechaCompra;
            int idCreador = user.Id;
            if (idCompra > 0)
            {
                var compraActual = oCompraN.findById_convertToCompra(idCompra);
                if (compraActual != null && compraActual.IdCompra > 0 && compraActual.CreadoPor != null)
                    idCreador = compraActual.CreadoPor.Id;
            }

            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.AddOrEditStock, fechaConsulta, idCreador))
                return Json(new { ok = false, mensaje = "No tiene permisos para consultar productos pendientes." });

            if (idSucursal <= 0)
                return Json(new { ok = false, mensaje = "Seleccione una sucursal válida." });

            try
            {
                var items = ObtenerProductosNoCargadosCierre(idSucursal, fechaConsulta, idCompra, codigosCargados ?? new long[0]);
                return Json(new
                {
                    ok = true,
                    items = items.Select(x => new
                    {
                        idCorte = x.IdCorte,
                        codigo = x.Codigo,
                        producto = x.Producto,
                        stockActual = x.StockActual
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult VerPorcentajesPesaje(int idCompra)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesión inválida." });

            var pesaje = oCompraN.findById_convertToCompra(idCompra);
            if (pesaje == null || pesaje.IdCompra <= 0 || !EsPesaje(pesaje.TipoCompra))
                return Json(new { ok = false, mensaje = "No se encontró el pesaje seleccionado." });

            int idCreador = pesaje.CreadoPor != null ? pesaje.CreadoPor.Id : user.Id;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.AddOrEditStock, pesaje.FechaCompra, idCreador))
                return Json(new { ok = false, mensaje = "No tiene permisos para consultar este pesaje." });

            if (!pesaje.CantMedias.HasValue || pesaje.CantMedias.Value <= 0 || !pesaje.KgsMedias.HasValue || pesaje.KgsMedias.Value <= 0)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "El pesaje no tiene registrado KgsMedias y CantMedias. Ingrese KgsMedias y CantMedias, presione Guardar y vuelva a intentarlo."
                });
            }

            try
            {
                int idAjuste = oCompraN.getIdAjusteDelPesaje(idCompra);
                var estado = oCompraN.estadoAjusteStock(idCompra, idAjuste);
                DataTable dtPromMedias = oCompraN.getPromMedias(idCompra) ?? new DataTable();
                DataTable dtPorcCortes = oCompraN.getPorcCortesEnMedias(idCompra) ?? new DataTable();

                NormalizarTablaPorcCortes(dtPorcCortes);

                return Json(new
                {
                    ok = true,
                    estado = Entidades.Compra.estadoAjStockToString(estado),
                    puedeGenerarAjuste = estado != Entidades.Compra.estadoAjusteStock.Actualizado,
                    idAjuste = idAjuste,
                    promMedias = ConstruirTablaModal(dtPromMedias, false, -1),
                    porcCortes = ConstruirTablaModal(dtPorcCortes, true, 2)
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GenerarAjustePesaje(int idCompra)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesión inválida." });

            var pesaje = oCompraN.findById_convertToCompra(idCompra);
            if (pesaje == null || pesaje.IdCompra <= 0 || !EsPesaje(pesaje.TipoCompra))
                return Json(new { ok = false, mensaje = "No se encontró el pesaje seleccionado." });

            int idCreador = pesaje.CreadoPor != null ? pesaje.CreadoPor.Id : user.Id;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.AddOrEditStock, pesaje.FechaCompra, idCreador))
                return Json(new { ok = false, mensaje = "No tiene permisos para generar el ajuste de este pesaje." });

            if (!pesaje.CantMedias.HasValue || pesaje.CantMedias.Value <= 0 || !pesaje.KgsMedias.HasValue || pesaje.KgsMedias.Value <= 0)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "El pesaje no tiene registrado KgsMedias y CantMedias. Ingrese KgsMedias y CantMedias, presione Guardar y vuelva a intentarlo."
                });
            }

            try
            {
                int idAjuste = oCompraN.getIdAjusteDelPesaje(idCompra);
                var ajuste = idAjuste > 0 ? oCompraN.findById_convertToCompra(idAjuste) : new Entidades.Compra();

                ajuste.NroRemito = pesaje.IdCompra.ToString();
                ajuste.Proveedor = pesaje.Proveedor;
                ajuste.FechaCompra = pesaje.FechaCompra;
                ajuste.Estado = "";
                ajuste.Observaciones = "ID Pesaje: " + pesaje.IdCompra;
                ajuste.TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock);
                ajuste.CantMedias = pesaje.CantMedias;
                ajuste.KgsMedias = pesaje.KgsMedias;
                ajuste.Sucursal = pesaje.Sucursal;

                if (ajuste.IdCompra <= 0)
                {
                    ajuste.CreadoPor = user;
                    ajuste.IdCompra = oCompraN.agregarCompra(ajuste);
                }
                else
                {
                    ajuste.ActualizadoPor = user;
                    oCompraN.modificarCompra(ajuste);
                }

                DataTable dtPorcCortes = oCompraN.getPorcCortesEnMedias(idCompra) ?? new DataTable();
                NormalizarTablaPorcCortes(dtPorcCortes);

                foreach (DataRow row in dtPorcCortes.Rows)
                {
                    if (!dtPorcCortes.Columns.Contains("idCorte") || row["idCorte"] == DBNull.Value)
                        continue;

                    int idCorte;
                    if (!int.TryParse(Convert.ToString(row["idCorte"]), out idCorte) || idCorte <= 0)
                        continue;

                    float diferencia;
                    if (!TryParseFloatFlexible(Convert.ToString(row["Dif."]), out diferencia))
                        throw new Exception("No se pudo interpretar la diferencia de uno de los productos.");

                    var cortePorCompra = new Entidades.CortePorCompra
                    {
                        Corte = new Entidades.Corte { IdCorte = idCorte },
                        Compra = ajuste,
                        CantKgs = diferencia,
                        precioKg = 0f,
                        Creado = DateTime.Now,
                        CreadoPor = ajuste.CreadoPor ?? user,
                        Sucursal = ajuste.Sucursal
                    };

                    oCompraN.agregarCortePorCompra(cortePorCompra);
                }

                oCompraN.actualizarEstadoPesaje(pesaje.IdCompra, Entidades.Compra.estadoAjusteStock.Actualizado);

                return Json(new
                {
                    ok = true,
                    mensaje = "El Ajuste de Stock se realizó correctamente.",
                    estado = Entidades.Compra.estadoAjStockToString(Entidades.Compra.estadoAjusteStock.Actualizado),
                    idAjuste = ajuste.IdCompra
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        private static bool EsTipoStock(string tipoCompra)
        {
            return TiposStock.Contains(tipoCompra ?? "", StringComparer.OrdinalIgnoreCase);
        }

        private static bool EsPesaje(string tipoCompra)
        {
            return string.Equals(tipoCompra,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes),
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsAjuste(string tipoCompra)
        {
            return string.Equals(tipoCompra,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock),
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsEgreso(string tipoCompra)
        {
            return string.Equals(tipoCompra,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock),
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsCierre(string tipoCompra)
        {
            return string.Equals(tipoCompra,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizarTipoFiltro(string tipoCompra)
        {
            if (string.IsNullOrWhiteSpace(tipoCompra))
                return "Ver Todos";

            if (string.Equals(tipoCompra, "Todos", StringComparison.OrdinalIgnoreCase))
                return "Ver Todos";

            return tipoCompra.Trim();
        }

        private static string NormalizarTipoOperacion(string tipoCompra)
        {
            if (string.IsNullOrWhiteSpace(tipoCompra))
                return "";

            string tipo = tipoCompra.Trim();
            return EsTipoStock(tipo) ? tipo : "";
        }

        private DataTable FiltrarSoloStock(DataTable origen)
        {
            if (origen == null)
                return new DataTable();

            if (!origen.Columns.Contains("tipoCompra"))
                return origen.Copy();

            var filas = origen.AsEnumerable()
                .Where(row => EsTipoStock(row["tipoCompra"] != DBNull.Value ? row["tipoCompra"].ToString() : ""))
                .ToList();

            if (filas.Count == 0)
                return origen.Clone();

            return filas.CopyToDataTable();
        }

        private StockEditVm CrearViewModelNuevo(Entidades.Usuario user, string tipoCompra)
        {
            int idSucursal = user != null && user.IdSucursal > 0 ? user.IdSucursal : 0;
            Entidades.Sucursal sucursal = idSucursal > 0 ? oSucursalN.findById(idSucursal) : null;
            int idProveedor = param.GetInt(Entidades.ParamKeys.IdIndefinido, 0);

            var model = new StockEditVm
            {
                IdCompra = 0,
                EsEdicion = false,
                TipoCompra = tipoCompra,
                IdSucursal = idSucursal,
                SucursalNombre = sucursal != null ? sucursal.SucursalNombre : "",
                FechaCompra = DateTime.Now,
                DraftKey = BuildDraftKey(user, idSucursal, tipoCompra, 0),
                IdProveedor = idProveedor
            };

            if (EsPesaje(tipoCompra))
            {
                var proveedor = ResolverProveedor(idProveedor);
                model.ProveedorNombre = proveedor != null ? proveedor.RazonSocial : "";
                model.ProveedorCuit = proveedor != null ? proveedor.Cuit : "";
            }

            return model;
        }

        private Entidades.ExistenciaStockPorSucursalFiltroVm CrearFiltroExistencia(Entidades.Usuario user)
        {
            var filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();
            int idSucursalActual = user != null && user.IdSucursal > 0 ? user.IdSucursal : 0;
            Entidades.Sucursal sucursalActual = idSucursalActual > 0 ? oSucursalN.findById(idSucursalActual) : null;

            filtro.IdSucursal = idSucursalActual;
            filtro.FechaHasta = DateTime.Now;
            filtro.SucursalesDisponibles.Add(new Entidades.SucursalColumnaStockVm
            {
                IdSucursal = 0,
                Sucursal = "Todas"
            });

            if (sucursalActual != null && sucursalActual.IdSucursal > 0)
            {
                filtro.SucursalesDisponibles.Add(new Entidades.SucursalColumnaStockVm
                {
                    IdSucursal = sucursalActual.IdSucursal,
                    Sucursal = sucursalActual.SucursalNombre
                });
            }

            filtro.TiposDisponibles = ObtenerTiposExistencia();
            filtro.ProveedoresDisponibles = ObtenerProveedoresExistencia();
            filtro.MarcasDisponibles = ObtenerMarcasExistencia();
            return filtro;
        }

        private List<string> ObtenerTiposExistencia()
        {
            var tipos = new List<string>();
            DataTable dtTipos;
            try
            {
                dtTipos = oCorteN.obtenerTiposProductoGrilla("") ?? new DataTable();
            }
            catch
            {
                return tipos;
            }

            foreach (DataRow row in dtTipos.Rows)
            {
                string tipo = row["tipo"] == DBNull.Value ? "" : Convert.ToString(row["tipo"]);
                if (!string.IsNullOrWhiteSpace(tipo) && !tipos.Any(x => string.Equals(x, tipo, StringComparison.OrdinalIgnoreCase)))
                    tipos.Add(tipo);
            }

            return tipos.OrderBy(x => x).ToList();
        }

        private List<Entidades.Persona> ObtenerProveedoresExistencia()
        {
            var proveedores = new List<Entidades.Persona>();
            DataTable dt;
            try
            {
                dt = oPersonaN.buscarProveedor("") ?? new DataTable();
            }
            catch
            {
                return proveedores;
            }

            foreach (DataRow row in dt.Rows)
            {
                int id = row.Table.Columns.Contains("idPersona") && row["idPersona"] != DBNull.Value ? Convert.ToInt32(row["idPersona"]) : 0;
                string razonSocial = row.Table.Columns.Contains("razonSocial") && row["razonSocial"] != DBNull.Value
                    ? Convert.ToString(row["razonSocial"])
                    : (row.Table.Columns.Contains("Proveedor") && row["Proveedor"] != DBNull.Value ? Convert.ToString(row["Proveedor"]) : "");

                if (id <= 0 || string.IsNullOrWhiteSpace(razonSocial))
                    continue;

                if (proveedores.Any(x => x.IdPersona == id))
                    continue;

                proveedores.Add(new Entidades.Persona
                {
                    IdPersona = id,
                    RazonSocial = razonSocial
                });
            }

            return proveedores.OrderBy(x => x.RazonSocial).ToList();
        }

        private List<Entidades.Persona> ObtenerMarcasExistencia()
        {
            var marcas = new List<Entidades.Persona>();
            DataTable dt;
            try
            {
                dt = oPersonaN.buscarPersona("", true) ?? new DataTable();
            }
            catch
            {
                return marcas;
            }

            foreach (DataRow row in dt.Rows)
            {
                int id = row.Table.Columns.Contains("idPersona") && row["idPersona"] != DBNull.Value ? Convert.ToInt32(row["idPersona"]) : 0;
                string razonSocial = row.Table.Columns.Contains("Marca") && row["Marca"] != DBNull.Value
                    ? Convert.ToString(row["Marca"])
                    : (row.Table.Columns.Contains("razonSocial") && row["razonSocial"] != DBNull.Value ? Convert.ToString(row["razonSocial"]) : "");

                if (id <= 0 || string.IsNullOrWhiteSpace(razonSocial))
                    continue;

                if (marcas.Any(x => x.IdPersona == id))
                    continue;

                marcas.Add(new Entidades.Persona
                {
                    IdPersona = id,
                    RazonSocial = razonSocial
                });
            }

            return marcas.OrderBy(x => x.RazonSocial).ToList();
        }

        private StockEditVm CrearViewModelEdicion(Entidades.Compra compra, Entidades.Usuario user)
        {
            var model = new StockEditVm
            {
                IdCompra = compra.IdCompra,
                EsEdicion = true,
                TipoCompra = compra.TipoCompra,
                IdSucursal = compra.Sucursal != null ? compra.Sucursal.IdSucursal : (user != null ? user.IdSucursal : 0),
                SucursalNombre = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "",
                FechaCompra = compra.FechaCompra,
                Observaciones = compra.Observaciones,
                Estado = compra.Estado,
                IdProveedor = compra.Proveedor != null ? compra.Proveedor.IdPersona : param.GetInt(Entidades.ParamKeys.IdIndefinido, 0),
                ProveedorNombre = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "",
                ProveedorCuit = compra.Proveedor != null ? compra.Proveedor.Cuit : "",
                CantMedias = compra.CantMedias,
                KgsMedias = compra.KgsMedias,
                Creado = FormatearFechaHora(compra.Creado),
                CreadoPor = compra.CreadoPor != null ? compra.CreadoPor.Nombre : "-",
                Actualizado = FormatearFechaHora(compra.Actualizado),
                ActualizadoPor = compra.ActualizadoPor != null ? compra.ActualizadoPor.Nombre : "-",
                DraftKey = BuildDraftKey(user, compra.Sucursal != null ? compra.Sucursal.IdSucursal : (user != null ? user.IdSucursal : 0), compra.TipoCompra, compra.IdCompra)
            };

            var cortes = oCompraN.convertCortesPorCompraToList(compra.IdCompra);
            int index = 0;
            foreach (var corte in cortes)
            {
                index++;
                model.Lineas.Add(new StockLineaVm
                {
                    Index = index,
                    IdCorte = corte.Corte != null ? (int?)corte.Corte.IdCorte : null,
                    Codigo = corte.Corte != null ? (long?)corte.Corte.Codigo : null,
                    Producto = corte.Corte != null ? corte.Corte.CorteDesc : "",
                    CantKgs = corte.CantKgs,
                    Balanza = corte.Balanza,
                    CreadoTexto = FormatearFechaHora(corte.Creado),
                    Pesable = corte.Corte != null && corte.Corte.Pesable
                });
            }

            RecalcularTotales(model);
            return model;
        }

        private void CargarViewBags(StockEditVm model)
        {
            ViewBag.Title = model.EsEdicion ? "Modificar Stock" : "Nuevo Stock";
            ViewBag.Seccion = "Stock";
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.UrlBuscarPersonaModal = Url.Action("Buscar", "Personas");
            ViewBag.UrlPersonaListar = Url.Action("Listar", "Personas");
        }

        private Dictionary<int, CompraIndexDetalleVm> ConstruirDetallesIndex(DataTable dt)
        {
            var detalles = new Dictionary<int, CompraIndexDetalleVm>();
            if (dt == null)
                return detalles;

            foreach (DataRow row in dt.Rows)
            {
                int idCompra = Convert.ToInt32(row["idCompra"]);
                if (detalles.ContainsKey(idCompra))
                    continue;

                Entidades.Compra compra = oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra == 0)
                    continue;

                bool esPesaje = EsPesaje(compra.TipoCompra);
                bool esAjuste = EsAjuste(compra.TipoCompra);
                int? idPesajeRelacionado = null;
                int? idAjusteRelacionado = null;

                if (esPesaje)
                {
                    int ajusteRelacionado = oCompraN.getIdAjusteDelPesaje(compra.IdCompra);
                    if (ajusteRelacionado > 0)
                        idAjusteRelacionado = ajusteRelacionado;
                }
                else if (esAjuste)
                {
                    int idPesaje;
                    if (int.TryParse(compra.NroRemito ?? "", out idPesaje) && idPesaje > 0)
                        idPesajeRelacionado = idPesaje;
                }

                detalles[idCompra] = new CompraIndexDetalleVm
                {
                    IdCompra = compra.IdCompra,
                    FechaCompra = compra.FechaCompra,
                    TipoCompra = compra.TipoCompra ?? "",
                    Cantidad = row["cantKg"] == DBNull.Value ? 0f : Convert.ToSingle(row["cantKg"]),
                    Sucursal = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "",
                    Observaciones = compra.Observaciones ?? "",
                    Estado = compra.Estado ?? "",
                    IdPesajeRelacionado = idPesajeRelacionado,
                    IdAjusteRelacionado = idAjusteRelacionado,
                    EsPesaje = esPesaje,
                    EsAjuste = esAjuste,
                    UsuarioCreacion = compra.CreadoPor != null ? compra.CreadoPor.Nombre : "",
                    FechaCreacion = compra.Creado,
                    UsuarioActualizacion = compra.ActualizadoPor != null ? compra.ActualizadoPor.Nombre : "",
                    FechaActualizacion = compra.Actualizado
                };
            }

            return detalles;
        }

        private List<ProductoNoCargadoCierreVm> ObtenerProductosNoCargadosCierre(int idSucursal, DateTime fechaCompra, int idCompra, IEnumerable<long> codigosCargados)
        {
            var productos = new List<ProductoNoCargadoCierreVm>();
            DataTable dtCortes = oCorteN.obtenerCortes() ?? new DataTable();
            if (dtCortes.Rows.Count == 0)
                return productos;

            var codigosActuales = new HashSet<long>((codigosCargados ?? Enumerable.Empty<long>()).Where(x => x > 0));

            DateTime desde = DateTime.Today.Date.AddYears(-10);
            DataTable dtInicioStock = oCompraN.obtenerCompras(
                idSucursal,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock),
                "",
                desde,
                fechaCompra,
                null) ?? new DataTable();

            int rowIndex = idCompra > 0 ? 1 : 0;
            if (dtInicioStock.Rows.Count > rowIndex)
                desde = Convert.ToDateTime(dtInicioStock.Rows[rowIndex]["fechaCompra"]);

            DataTable dtStockActual = oCorteN.CierreStock(1, "", idSucursal, desde, fechaCompra, null, "", 0, 0) ?? new DataTable();
            var stockPorCodigo = new Dictionary<long, float>();
            if (dtStockActual.Columns.Contains("Codigo"))
            {
                foreach (DataRow row in dtStockActual.Rows)
                {
                    long codigo;
                    if (!long.TryParse(Convert.ToString(row["Codigo"]), out codigo))
                        continue;

                    float stock = 0f;
                    if (dtStockActual.Columns.Contains("DIF") && row["DIF"] != DBNull.Value)
                        stock = Convert.ToSingle(row["DIF"]);

                    stockPorCodigo[codigo] = stock;
                }
            }

            foreach (DataRow corte in dtCortes.Rows)
            {
                bool enCierreStock = corte.Table.Columns.Contains("enCierreStock") && corte["enCierreStock"] != DBNull.Value && Convert.ToBoolean(corte["enCierreStock"]);
                if (!enCierreStock)
                    continue;

                long codigo;
                if (!long.TryParse(Convert.ToString(corte["codigo"]), out codigo))
                    continue;

                if (codigosActuales.Contains(codigo))
                    continue;

                productos.Add(new ProductoNoCargadoCierreVm
                {
                    IdCorte = Convert.ToInt32(corte["idCorte"]),
                    Codigo = codigo,
                    Producto = Convert.ToString(corte["corte"]) ?? "",
                    StockActual = stockPorCodigo.ContainsKey(codigo) ? stockPorCodigo[codigo] : 0f
                });
            }

            return productos.OrderBy(x => x.Codigo).ToList();
        }

        private static float CalcularTotalKg(DataTable dt)
        {
            float total = 0f;
            if (dt == null)
                return total;

            foreach (DataRow row in dt.Rows)
            {
                total += row["cantKg"] == DBNull.Value ? 0f : Convert.ToSingle(row["cantKg"]);
            }

            return total;
        }

        private static void NormalizarTablaPorcCortes(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return;

            if (!dt.Columns.Contains("Gan."))
                return;

            decimal ganancia = 0m;
            int lastIndex = dt.Rows.Count - 1;

            for (int fila = 0; fila < dt.Rows.Count; fila++)
            {
                if (fila == lastIndex)
                {
                    dt.Rows[fila]["Gan."] = ganancia;
                    if (dt.Columns.Contains("Codigo"))
                        dt.Rows[fila]["Codigo"] = DBNull.Value;
                }
                else
                {
                    decimal valorGanancia;
                    if (TryConvertToDecimal(dt.Rows[fila]["Gan."], out valorGanancia))
                        ganancia += valorGanancia;
                }
            }
        }

        private static TablaModalStockVm ConstruirTablaModal(DataTable dt, bool ocultarIdCorte, int formatoTresDecimalesDesdeColumna)
        {
            var tabla = new TablaModalStockVm();
            if (dt == null)
                return tabla;

            for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
            {
                var column = dt.Columns[colIndex];
                tabla.columnas.Add(new ColumnaModalStockVm
                {
                    nombre = column.ColumnName,
                    oculta = ocultarIdCorte && string.Equals(column.ColumnName, "idCorte", StringComparison.OrdinalIgnoreCase),
                    alineacionDerecha = EsNumerica(column.DataType) || (formatoTresDecimalesDesdeColumna >= 0 && colIndex >= formatoTresDecimalesDesdeColumna),
                    formatoTresDecimales = formatoTresDecimalesDesdeColumna >= 0 && colIndex >= formatoTresDecimalesDesdeColumna
                });
            }

            foreach (DataRow row in dt.Rows)
            {
                var fila = new List<string>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    fila.Add(FormatearCeldaTabla(row[i], dt.Columns[i], tabla.columnas[i].formatoTresDecimales));
                }
                tabla.filas.Add(fila);
            }

            return tabla;
        }

        private static string FormatearCeldaTabla(object value, DataColumn column, bool formatoTresDecimales)
        {
            if (value == null || value == DBNull.Value)
                return "";

            var cultura = CultureInfo.GetCultureInfo("es-AR");

            if (EsNumerica(column.DataType))
            {
                decimal numero = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return numero.ToString(formatoTresDecimales ? "F3" : "0.###", cultura);
            }

            if (formatoTresDecimales)
            {
                float numeroFloat;
                if (TryParseFloatFlexible(Convert.ToString(value), out numeroFloat))
                    return numeroFloat.ToString("F3", cultura);
            }

            if (column.DataType == typeof(DateTime))
            {
                DateTime fecha;
                if (DateTime.TryParse(Convert.ToString(value), out fecha))
                    return fecha.ToString("dd/MM/yyyy HH:mm");
            }

            return Convert.ToString(value);
        }

        private static bool EsNumerica(Type type)
        {
            return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
                type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                type == typeof(byte);
        }

        private static bool TryConvertToDecimal(object value, out decimal numero)
        {
            numero = 0m;
            if (value == null || value == DBNull.Value)
                return false;

            if (value is decimal)
            {
                numero = (decimal)value;
                return true;
            }

            if (value is float || value is double || value is int || value is long || value is short || value is byte)
            {
                numero = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return true;
            }

            string raw = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim();

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
                return true;

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out numero))
                return true;

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out numero))
                return true;

            return false;
        }

        private void NormalizarDecimalesPosteados(StockEditVm model)
        {
            if (model == null || Request == null || Request.Form == null)
                return;

            float valorFloat;

            if (TryParseFloatFlexible(Request.Form["KgsMedias"], out valorFloat))
            {
                model.KgsMedias = valorFloat;
                ModelState.Remove("KgsMedias");
            }

            if (model.Lineas == null)
                return;

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea == null)
                    continue;

                string keyCantKgs = "Lineas[" + i + "].CantKgs";
                if (TryParseFloatFlexible(Request.Form[keyCantKgs], out valorFloat))
                {
                    linea.CantKgs = valorFloat;
                    ModelState.Remove(keyCantKgs);
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

        private string ValidarModelo(StockEditVm model, Entidades.Usuario user)
        {
            if (model == null)
                return "No se recibieron datos.";

            if (!EsTipoStock(model.TipoCompra))
                return "Seleccione una acción válida de stock.";

            if (model.IdSucursal <= 0)
                return "Seleccione una sucursal.";

            if (model.FechaCompra == DateTime.MinValue)
                return "Ingrese una fecha válida.";

            if (EsAjuste(model.TipoCompra) && (user == null || !user.Admin))
                return "No tiene permisos para realizar Ajuste de Stock.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe ingresar al menos una línea.";

            int index = 0;
            foreach (var linea in model.Lineas)
            {
                index++;
                if (!linea.IdCorte.HasValue || linea.IdCorte.Value <= 0)
                    return "La línea " + index + " no tiene un producto válido.";

                if (!EsCierre(model.TipoCompra) && linea.CantKgs == 0)
                    return "La línea " + index + " tiene una cantidad inválida.";
            }

            if (EsPesaje(model.TipoCompra))
            {
                if (model.IdProveedor <= 0)
                    return "Seleccione un proveedor para el pesaje.";

                if (!model.CantMedias.HasValue || model.CantMedias.Value <= 0)
                    return "Ingrese la cantidad de medias para el pesaje.";

                if (!model.KgsMedias.HasValue || model.KgsMedias.Value <= 0)
                    return "Ingrese los kilos de medias para el pesaje.";
            }

            return "";
        }

        private Entidades.Persona ResolverProveedor(int idProveedor)
        {
            int id = idProveedor > 0 ? idProveedor : param.GetInt(Entidades.ParamKeys.IdIndefinido, 0);
            return id > 0 ? oPersonaN.findById(id) : null;
        }

        private static string BuildDraftKey(Entidades.Usuario user, int idSucursal, string tipoCompra, int idCompra)
        {
            int idUsuario = user != null ? user.Id : 0;
            return "stock_draft_" + idUsuario + "_" + idSucursal + "_" + tipoCompra + "_" + idCompra;
        }

        private static string FormatearFechaHora(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        }

        private static void RecalcularTotales(StockEditVm model)
        {
            model.CantItems = model.Lineas != null ? model.Lineas.Count : 0;
            model.TotalKg = 0f;

            foreach (var linea in model.Lineas ?? new List<StockLineaVm>())
            {
                model.TotalKg += linea.CantKgs;
            }

            if (EsPesaje(model.TipoCompra) && (!model.KgsMedias.HasValue || model.KgsMedias.Value <= 0))
                model.KgsMedias = model.TotalKg;
        }
    }
}
