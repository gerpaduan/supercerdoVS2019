// Port PARCIAL de Web/Controllers/StockController.cs (ver docs/DECISIONS.md, migracion ASP.NET
// Core, Modulo 4 -- Stock e inventario). El original tiene 2427 lineas y 13 acciones (listado,
// alta/edicion de movimientos de stock, flujo completo de "pesaje" con ajustes vinculados,
// existencia por sucursal, autocompletado de productos). Mismo criterio de escala que Modulo 3
// (Productos): se porta en slices. Portado hasta ahora: Index()/Detalle(), Nuevo()/Editar()/
// Guardar() (alta/edicion de movimientos de stock), Lineas() (listado por producto),
// BuscarCorte/BuscarCortePorCodigo (autocompletado), ExistenciaPorSucursales/
// BuscarExistenciaPorSucursales/StockPorSucursalesProducto/ObtenerFechaMinimaExistencia, y el
// sub-flujo completo de pesaje (UltimasComprasPesaje/DetalleCompraPesaje/
// ProductosNoCargadosCierre/VerPorcentajesPesaje/GenerarAjustePesaje). NO portado todavia:
// ningun endpoint queda pendiente en este controller -- ver docs/10-migracion-aspnet-core/README.md
// para el estado real (juez de paridad corrido o no por accion) y gaps.md para lo encontrado.
//
// Mismo criterio que Personas/Productos: IEmpresaContext + IParametrosContext reales, y un stub
// Entidades.Usuario (Admin=true, IdEmpresa=1, IdSucursal=2, Nombre="ger") que imita al usuario
// real de prueba usado en el juez de paridad. El sistema de "permiso con limite de fecha" del
// original (BaseController.AjustarFechaIndiceSegunLimiteYPermiso/
// ConfigurarAdvertenciaFechaIndiceConLimiteEnVivo, ver Web/Controllers/BaseController.cs:175-246)
// se omite por completo (no solo se hardcodea a true): con el stub admin de esta migracion el
// resultado de esas funciones es siempre "sin restriccion, sin aviso", asi que no llamarlas
// produce el mismo resultado observable que llamarlas con permiso total. Lo mismo se aplica ahora
// a PermisosHelper.TienePermiso(Session, Stock.AddOrEditStock, ...) en Editar/Guardar: se
// hardcodea a "siempre autorizado" (mismo criterio ya usado en ProductosController para
// esAdministrador). El calculo de la fecha default del filtro de Index ("fechaLimiteSinPermiso")
// SI se preserva -- es un valor de negocio real, no parte del gate de permiso.
//
// El gate de "usuario de sala de produccion" (Editar redirige a un controller SeleccionUsuario
// separado cuando Session["Usuario"].EsUsuarioProduccion==true y todavia no se eligio operador,
// ver Web/Controllers/StockController.cs:466 y docs/DECISIONS.md "Mover la seleccion de
// usuario...") NO se porta: el stub admin nunca es usuario de produccion (EsUsuarioProduccion
// queda en su default, false), asi que esa rama nunca se dispara -- mismo comportamiento
// observable que un usuario real no-produccion. ResolverUsuarioCreador() SI se porta (metodo
// trivial, no cuesta nada mantenerlo fiel) por si se agrega login real mas adelante.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class StockController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private static readonly string[] TiposStock =
        {
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes),
            Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock)
        };

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;
        private readonly Negocio.Compra _oCompraN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Corte _oCorteN;

        // Mismo criterio que PersonasController: stub que imita al usuario real de prueba (ger,
        // id=2, admin=true, empresa 1, sucursal San Lorenzo=2) para que el juez de paridad compare
        // contra el mismo comportamiento. EsUsuarioProduccion queda en su default (false) --
        // deliberado: el gate de "usuario de sala de produccion" (Editar redirige a
        // SeleccionUsuario, ver Web/Controllers/StockController.cs:466 y docs/DECISIONS.md,
        // "Mover la seleccion de usuario...") nunca aplica a un usuario admin real, asi que con
        // este stub esa rama no se dispara -- mismo comportamiento observable, no una omision.
        // Id=2 es OBLIGATORIO, no cosmetico: a diferencia de Index/Detalle (donde Id nunca se
        // persiste), Guardar/GenerarAjustePesaje escriben CreadoPor/ActualizadoPor a la base real
        // -- un stub sin Id (default 0) graba el usuario de sistema "CarniSys Admin" (id=0, un
        // usuario real pero equivocado) en vez de "ger". Bug real encontrado en la prueba en vivo
        // del 2026-09-01 (compra idCompra=9037 quedo con creadoPor=0 en vez de 2), ver docs/
        // DECISIONS.md. PersonasController/ProductosController no tienen este problema: ninguno
        // persiste CreadoPor/ActualizadoPor del usuario de sesion (revisado en la misma sesion).
        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public StockController()
        {
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oCompraN = new Negocio.Compra(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oPersonaN = new Negocio.Persona(_empresa, _param);
            _oCorteN = new Negocio.Corte(_empresa, _param);
        }

        public IActionResult Index(int? idSucursal = null, string tipoCompra = "Ver Todos", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            DateTime fechaLimiteSinPermiso = DateTime.Today.AddDays(-_param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime desde = fechaDesde ?? fechaLimiteSinPermiso;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            // TODO(claude): el original usa Session["Usuario"].IdSucursal como default cuando no
            // hay ?idSucursal en la URL. Sin sesion real, se hardcodea a 2 (San Lorenzo) -- la
            // sucursal del usuario de prueba (ger) usado en todo el juez de paridad de esta
            // migracion, para que la comparacion sea real. Si el usuario stub de WebCore cambia
            // en el futuro (ver otros controllers), actualizar este valor tambien.
            const int idSucursalUsuarioStub = 2;
            int sucursalSeleccionada = idSucursal.HasValue ? idSucursal.Value : idSucursalUsuarioStub;
            string tipoNormalizado = NormalizarTipoFiltro(tipoCompra);

            DataTable dt = _oCompraN.obtenerCompras(sucursalSeleccionada, tipoNormalizado, "", desde, hasta, null) ?? new DataTable();
            dt = FiltrarSoloStock(dt);

            var model = new CompraIndexVm
            {
                Compras = dt,
                Detalles = ConstruirDetallesIndex(dt)
            };

            ViewBag.Title = "Stock";
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.IdSucursal = sucursalSeleccionada;
            ViewBag.TipoCompra = tipoNormalizado;
            ViewBag.FechaDesde = desde;
            ViewBag.FechaHasta = hasta;
            ViewBag.TotalKg = CalcularTotalKg(dt);

            return View(model);
        }

        public IActionResult Lineas(int? idSucursal = null, string tipoCompra = "Ver Todos", string producto = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            Entidades.Usuario user = _usuarioActual;

            DateTime fechaLimiteSinPermiso = DateTime.Today.AddDays(-_param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime desde = fechaDesde ?? fechaLimiteSinPermiso;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            // TODO(claude): AjustarFechaIndiceSegunLimiteYPermiso omitido (mismo criterio que el
            // resto del sistema de "permiso con limite de fecha", ver header del archivo) -- el
            // stub admin siempre tiene permiso total, esa funcion nunca recortaria `desde`.

            int sucursalSeleccionada = idSucursal.HasValue ? idSucursal.Value : (user.IdSucursal > 0 ? user.IdSucursal : 0);
            string tipoNormalizado = NormalizarTipoFiltro(tipoCompra);

            DataTable dt = _oCompraN.obtenerCompras(sucursalSeleccionada, tipoNormalizado, "", desde.Date, hasta.Date, null) ?? new DataTable();
            dt = FiltrarSoloStock(dt);

            var model = new StockLineasIndexVm
            {
                IdSucursal = sucursalSeleccionada,
                TipoCompra = tipoNormalizado,
                Producto = producto ?? "",
                FechaDesde = desde,
                FechaHasta = hasta
            };

            foreach (DataRow row in dt.Rows)
            {
                int idCompra = row["idCompra"] == DBNull.Value ? 0 : Convert.ToInt32(row["idCompra"]);
                if (idCompra <= 0 || model.Registros.Any(x => x.IdCompra == idCompra))
                    continue;

                Entidades.Compra compra = _oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra == 0)
                    continue;

                var lineas = (_oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>())
                    .Select(corte => new StockLineaDetalleVm
                    {
                        Codigo = corte.Corte != null ? corte.Corte.Codigo.ToString() : "-",
                        Producto = corte.Corte != null ? corte.Corte.CorteDesc : "-",
                        CantidadKgTexto = corte.CantKgs.ToString("N3"),
                        Signo = ObtenerSignoStock(compra.TipoCompra),
                        Observacion = corte.Balanza ? "Peso balanza" : "-",
                        CantidadKg = Convert.ToDecimal(corte.CantKgs)
                    })
                    .Where(x => CoincideProductoStock(x.Codigo, x.Producto, producto))
                    .ToList();

                if (lineas.Count == 0)
                    continue;

                var grupo = new StockLineasGrupoVm
                {
                    IdCompra = compra.IdCompra,
                    CollapseId = "stockLineas_" + compra.IdCompra,
                    Titulo = "REGISTRO ID: " + compra.IdCompra,
                    Subtitulo = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm"),
                    ResumenCompacto = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm"),
                    ResumenSecundario = string.IsNullOrWhiteSpace(compra.TipoCompra) ? "-" : compra.TipoCompra,
                    EditUrl = Url.Action("Editar", "Stock", new { id = compra.IdCompra, tipoCompra = compra.TipoCompra }) ?? "",
                    TotalKg = lineas.Sum(x => x.CantidadKg)
                };

                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Fecha", Valor = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Tipo de operacion", Valor = string.IsNullOrWhiteSpace(compra.TipoCompra) ? "-" : compra.TipoCompra });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal", Valor = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Usuario", Valor = compra.CreadoPor != null ? compra.CreadoPor.Nombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Observacion", Valor = string.IsNullOrWhiteSpace(compra.Observaciones) ? "-" : compra.Observaciones });

                grupo.Lineas.AddRange(lineas);
                model.Registros.Add(grupo);
            }

            ViewBag.Title = "Lineas de stock";
            ViewBag.Seccion = "Stock";
            ViewBag.Sucursales = _oSucursalN.findAll();

            return View("~/Views/Stock/Lineas.cshtml", model);
        }

        [HttpGet]
        public IActionResult Detalle(int idCompra)
        {
            Entidades.Compra compra = _oCompraN.findById_convertToCompra(idCompra);
            if (compra == null || compra.IdCompra <= 0)
            {
                ViewBag.StockDetalleError = "No se encontraron los detalles del movimiento.";
                return PartialView("_StockDetalle", null);
            }

            return PartialView("_StockDetalle", ConstruirDetalleIndexCompleto(compra));
        }

        public IActionResult Nuevo(string tipoCompra, int idUsuarioCreador = 0)
        {
            string tipoNormalizado = NormalizarTipoOperacion(tipoCompra);
            if (string.IsNullOrWhiteSpace(tipoNormalizado))
                return RedirectToAction("Index");

            return RedirectToAction("Editar", new { id = 0, tipoCompra = tipoNormalizado, idUsuarioCreador });
        }

        public IActionResult Editar(int id = 0, string tipoCompra = "", int idUsuarioCreador = 0)
        {
            Entidades.Usuario user = _usuarioActual;

            Entidades.Compra compra = null;
            if (id > 0)
            {
                compra = _oCompraN.findById_convertToCompra(id);
                if (compra == null || compra.IdCompra == 0)
                    return NotFound();
            }

            string tipoOperacion = compra != null ? compra.TipoCompra : NormalizarTipoOperacion(tipoCompra);
            if (string.IsNullOrWhiteSpace(tipoOperacion) || !EsTipoStock(tipoOperacion))
                return RedirectToAction("Index");

            // TODO(claude): el original chequea PermisosHelper.TienePermiso(Session,
            // Stock.AddOrEditStock, fechaPermiso, idCreador) -- y si falla, exige ademas VerStock
            // para no redirigir directo a Index. Se omite igual que el resto del sistema de
            // "permiso con limite de fecha" (ver header del archivo): el stub admin de esta
            // migracion siempre esta autorizado, mismo resultado observable que portar el chequeo
            // real con Admin=true.
            bool puedeModificar = true;

            if (EsAjuste(tipoOperacion) && !user.Admin)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para realizar Ajuste de Stock.";
                return RedirectToAction("Index");
            }

            var model = compra != null ? CrearViewModelEdicion(compra, user) : CrearViewModelNuevo(user, tipoOperacion);
            model.SoloLecturaInicial = model.EsEdicion;
            model.PuedeHabilitarEdicion = !model.EsEdicion || puedeModificar;
            // Quien esta operando esta pantalla ahora (campo "Usuario" del formulario) -- con
            // produccion, el operador real ya seleccionado antes de entrar, no "User Produccion"
            // (ver docs/DECISIONS.md). Con el stub (EsUsuarioProduccion=false) siempre es la
            // segunda rama.
            model.UsuarioNombre = user.EsUsuarioProduccion
                ? ResolverUsuarioCreador(idUsuarioCreador, user).Nombre
                : (user.Nombre ?? "");
            CargarViewBags(model, idUsuarioCreador);

            return View("~/Views/Stock/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(StockEditVm model, int idUsuarioCreador = 0)
        {
            Entidades.Usuario user = _usuarioActual;

            if (model == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Error";
                TempData["AlertMsg"] = "No se recibieron datos para guardar.";
                return RedirectToAction("Index");
            }

            // Usuario de produccion: el creador/actualizador real es el elegido antes de entrar a
            // esta pantalla (idUsuarioCreador, ver Editar), no el usuario de sesion compartido.
            // Con el stub esto siempre devuelve user sin cambios (ver ResolverUsuarioCreador).
            var usuarioCreador = ResolverUsuarioCreador(idUsuarioCreador, user);

            string tipoOperacion = NormalizarTipoOperacion(model.TipoCompra);
            Entidades.Compra compraActual = null;
            if (model.IdCompra > 0)
            {
                compraActual = _oCompraN.findById_convertToCompra(model.IdCompra);
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
            AplicarGuardarSinPesaje(model);
            CompletarDatosProveedor(model);
            string error = ValidarModelo(model, user);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError("", error);
                if (user.EsUsuarioProduccion)
                    model.UsuarioNombre = usuarioCreador.Nombre;
                CargarViewBags(model, idUsuarioCreador);
                CargarDatosRelacionadosEnModelo(model, compraActual);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }

            // TODO(claude): permiso Stock.AddOrEditStock omitido aca tambien (ver Editar) -- stub
            // admin siempre autorizado.

            Entidades.Sucursal sucursal = _oSucursalN.findById(model.IdSucursal);
            if (sucursal == null || sucursal.IdSucursal <= 0)
            {
                ModelState.AddModelError("", "Seleccione una sucursal válida.");
                if (user.EsUsuarioProduccion)
                    model.UsuarioNombre = usuarioCreador.Nombre;
                CargarViewBags(model, idUsuarioCreador);
                CargarDatosRelacionadosEnModelo(model, compraActual);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }

            int idProveedor = model.IdProveedor > 0
                ? model.IdProveedor
                : (compraActual != null && compraActual.Proveedor != null && compraActual.Proveedor.IdPersona > 0
                    ? compraActual.Proveedor.IdPersona
                    : _param.GetInt(Entidades.ParamKeys.IdIndefinido, 0));
            Entidades.Persona proveedor = ResolverProveedor(idProveedor);
            if (proveedor == null || proveedor.IdPersona <= 0)
            {
                ModelState.AddModelError("", "No se pudo resolver la persona para este movimiento.");
                if (user.EsUsuarioProduccion)
                    model.UsuarioNombre = usuarioCreador.Nombre;
                CargarViewBags(model, idUsuarioCreador);
                CargarDatosRelacionadosEnModelo(model, compraActual);
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
            compra.IdPesajeAjustado = (EsPesaje(tipoOperacion) || EsAjuste(tipoOperacion))
                ? model.IdPesajeAjustado
                : (compraActual != null ? compraActual.IdPesajeAjustado : null);
            compra.CreadoPor = compraActual != null ? compraActual.CreadoPor : usuarioCreador;
            compra.ActualizadoPor = compraActual != null ? usuarioCreador : null;

            var lineas = new List<Entidades.CortePorCompra>();
            int index = 0;
            foreach (var linea in model.Lineas ?? new List<StockLineaVm>())
            {
                index++;
                var corte = _oCorteN.findCorteById(linea.IdCorte ?? 0, false);
                int idEmpresaSesion = _usuarioActual.IdEmpresa > 0 ? _usuarioActual.IdEmpresa : _empresa.IdEmpresa;
                if (corte == null || corte.IdCorte <= 0 || (idEmpresaSesion > 0 && corte.IdEmpresa != idEmpresaSesion))
                {
                    ModelState.AddModelError("", "No se encontró el producto de la línea " + index + ".");
                    if (user.EsUsuarioProduccion)
                        model.UsuarioNombre = usuarioCreador.Nombre;
                    CargarViewBags(model, idUsuarioCreador);
                    CargarDatosRelacionadosEnModelo(model, compraActual);
                    RecalcularTotales(model);
                    return View("~/Views/Stock/Editar.cshtml", model);
                }

                float cantidad = linea.CantKgs;
                if (EsEgreso(tipoOperacion) && cantidad > 0)
                    cantidad = cantidad * -1;

                DateTime creadoLinea = ParseFechaHoraStockLinea(linea.CreadoTexto) ?? DateTime.Now;

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
                    Creado = creadoLinea,
                    CreadoPor = usuarioCreador
                });
            }

            try
            {
                _oCompraN.AddOrEditCompra(compra, compra.TipoCompra, null, lineas, false, null);
                SincronizarPesajesVinculados(compra, model.PesajesVinculadosIds, usuarioCreador);
                TempData["StockDraftKeyToClear"] = model.DraftKey ?? "";
                TempData["StockSuccessMessage"] = model.IdCompra > 0
                    ? "El movimiento de stock se guardó correctamente."
                    : "El movimiento de stock se registró correctamente.";

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Stock";
                TempData["AlertMsg"] = TempData["StockSuccessMessage"];

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar el movimiento de stock. " + ex.Message);
                if (user.EsUsuarioProduccion)
                    model.UsuarioNombre = usuarioCreador.Nombre;
                CargarViewBags(model, idUsuarioCreador);
                CargarDatosRelacionadosEnModelo(model, compraActual);
                RecalcularTotales(model);
                return View("~/Views/Stock/Editar.cshtml", model);
            }
        }

        // Autocompletado de productos usado por el modal "Buscar producto" de Editar.cshtml
        // (boton F10) -- Core no necesita JsonRequestBehavior.AllowGet (MVC5 lo exige para GET
        // por seguridad contra JSON hijacking; Core no tiene esa restriccion por defecto).
        [HttpGet]
        public IActionResult BuscarCorte(string q = "")
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

                return Json(resultado);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public IActionResult BuscarCortePorCodigo(long? codigo)
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
        public IActionResult ExistenciaPorSucursales()
        {
            Entidades.Usuario user = _usuarioActual;

            // TODO(claude): permiso Stock.VerStock omitido, mismo criterio que el resto del
            // sistema de "permiso con limite de fecha" (ver header del archivo) -- el stub admin
            // siempre esta autorizado.
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
        public IActionResult BuscarExistenciaPorSucursales(
            string texto = "",
            int idSucursal = 0,
            DateTime? fechaHasta = null,
            string tipo = "",
            int idProveedor = 0,
            int idMarca = 0,
            int idCorte = 0,
            bool soloConStock = false)
        {
            Entidades.Usuario user = _usuarioActual;
            var model = new Entidades.ExistenciaPorSucursalesVm();

            try
            {
                var filtro = CrearFiltroExistencia(user);
                filtro.Texto = (texto ?? "").Trim();
                filtro.IdSucursal = idSucursal > 0 ? idSucursal : 0;
                filtro.FechaHasta = fechaHasta;
                filtro.Tipo = (tipo ?? "").Trim();
                filtro.IdProveedor = idProveedor;
                filtro.IdMarca = idMarca;
                filtro.IdCorte = idCorte > 0 ? idCorte : 0;
                filtro.SoloConStock = soloConStock;

                // El limite ya se recalculo para el idSucursal correcto dentro de
                // CrearFiltroExistencia -> AplicarUltimosCierres. El calculo de
                // a_ExistenciaStockPorSucursales usa el ultimo cierre como punto de partida sin
                // importar la fecha pedida, asi que pedir un FechaHasta anterior a ese cierre da
                // un resultado incorrecto -- se rechaza aca en vez de dejarlo pasar (ver
                // docs/DECISIONS.md).
                if (filtro.FechaHasta.HasValue && filtro.FechaMinimaConsulta.HasValue
                    && filtro.FechaHasta.Value < filtro.FechaMinimaConsulta.Value)
                {
                    model.Filtro = filtro;
                    model.ConsultaRealizada = true;
                    model.Mensaje = "No se puede consultar una fecha anterior al último cierre de stock (" +
                        filtro.FechaMinimaConsulta.Value.ToString("dd/MM/yyyy HH:mm") +
                        "). El cálculo siempre parte del último cierre registrado.";
                    return PartialView("~/Views/Stock/_TablaExistenciaPorSucursales.cshtml", model);
                }

                model = _oCorteN.ObtenerMatrizExistenciaPorSucursales(filtro);
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

        // Consumido tambien desde ProductosController.Index ("Ver stock por sucursales") --
        // Url.Action("StockPorSucursalesProducto", "Stock"), botón que quedó deshabilitado en el
        // Modulo 3 porque este endpoint todavia no existia (ver docs/DECISIONS.md/gaps.md de ese
        // momento). Con esto portado, ese boton ya deberia funcionar sin tocar ProductosController.
        [HttpGet]
        public IActionResult StockPorSucursalesProducto(int idCorte, DateTime? fechaHasta = null)
        {
            Entidades.Usuario user = _usuarioActual;
            var model = new Entidades.ExistenciaPorSucursalesVm();

            if (idCorte <= 0)
            {
                model.ConsultaRealizada = true;
                model.Mensaje = "Producto inválido.";
                return PartialView("~/Views/Productos/_StockPorSucursalesProductoModal.cshtml", model);
            }

            try
            {
                var corte = _oCorteN.findCorteById(idCorte, true);
                if (corte == null || corte.IdCorte <= 0)
                {
                    model.ConsultaRealizada = true;
                    model.Mensaje = "No se encontró el producto seleccionado.";
                    return PartialView("~/Views/Productos/_StockPorSucursalesProductoModal.cshtml", model);
                }

                var filtro = CrearFiltroExistencia(user);
                filtro.IdSucursal = 0;
                filtro.FechaHasta = fechaHasta;
                filtro.IdCorte = idCorte;
                filtro.SoloConStock = false;

                model = _oCorteN.ObtenerMatrizExistenciaPorSucursales(filtro);
                model.Filtro = filtro;

                ViewBag.ProductoNombre = corte.CorteDesc;
                ViewBag.ProductoCodigo = corte.Codigo;
                ViewBag.ProductoId = corte.IdCorte;
                ViewBag.ProductoSinStock = !model.Productos.Any() || !model.Productos.Any(x => x.TieneStockPositivo);
                // El SP de existencia excluye productos con EnCierreStock=false o
                // Independiente=false (ver a_ExistenciaStockPorSucursales). Se informa el motivo
                // real en vez de un generico "no tiene stock" cuando por eso no aparecen filas.
                ViewBag.ProductoEnCierreStock = corte.EnCierreStock;
                ViewBag.ProductoEsIndependiente = corte.Independiente != 0;
            }
            catch (Exception ex)
            {
                model = new Entidades.ExistenciaPorSucursalesVm
                {
                    ConsultaRealizada = true,
                    Mensaje = "Error al consultar la existencia por sucursales. " + ex.Message
                };
            }

            return PartialView("~/Views/Productos/_StockPorSucursalesProductoModal.cshtml", model);
        }

        [HttpGet]
        public IActionResult ObtenerFechaMinimaExistencia(int idSucursal = 0)
        {
            var filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm { IdSucursal = idSucursal };
            List<Entidades.Sucursal> sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            AplicarUltimosCierres(filtro, sucursales);

            return Json(new
            {
                ok = true,
                fechaMinima = filtro.FechaMinimaConsulta.HasValue
                    ? filtro.FechaMinimaConsulta.Value.ToString("yyyy-MM-ddTHH:mm")
                    : (string)null,
                detalle = filtro.UltimosCierresPorSucursal
                    .OrderBy(x => x.Sucursal)
                    .Select(x => new { x.Sucursal, fecha = x.FechaUltimoCierre.ToString("dd/MM/yyyy HH:mm") })
            });
        }

        // TODO(claude): permiso Stock.AddOrEditStock omitido en las 5 acciones de pesaje que
        // siguen (mismo criterio que el resto del archivo, ver header) -- el stub admin siempre
        // esta autorizado.
        [HttpGet]
        public IActionResult UltimasComprasPesaje(int idSucursal = 0, int idCompraActual = 0, bool soloPesajes = false, bool soloComprasPesaje = false, string proveedor = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            Entidades.Usuario user = _usuarioActual;

            try
            {
                int sucursal = idSucursal > 0 ? idSucursal : (user.IdSucursal > 0 ? user.IdSucursal : 0);
                if (sucursal <= 0)
                    return Json(new { ok = false, mensaje = "Seleccione una sucursal valida." });

                DateTime desde = fechaDesde.HasValue ? fechaDesde.Value.Date : DateTime.Today.AddDays(-7);
                DateTime hasta = fechaHasta.HasValue ? fechaHasta.Value.Date.AddDays(1) : DateTime.Today.AddDays(1);
                string proveedorFiltro = (proveedor ?? "").Trim();
                DataTable dt = soloComprasPesaje
                    ? UnirComprasParaPesajeSeleccion(sucursal, desde, hasta)
                    : _oCompraN.obtenerCompras(sucursal, NormalizarTipoFiltro("Todos"), "", desde, hasta, null) ?? new DataTable();
                var items = new List<CompraPesajeListadoVm>();

                var idsProcesados = new HashSet<int>();
                var rowsOrdenadas = dt.Rows.Cast<DataRow>()
                    .Where(r => r.Table.Columns.Contains("idCompra") && r["idCompra"] != DBNull.Value)
                    .OrderByDescending(r => r.Table.Columns.Contains("fechaCompra") && r["fechaCompra"] != DBNull.Value
                        ? Convert.ToDateTime(r["fechaCompra"])
                        : DateTime.MinValue);

                foreach (var row in rowsOrdenadas)
                {
                    int idCompra = Convert.ToInt32(row["idCompra"]);
                    if (idsProcesados.Contains(idCompra))
                        continue;

                    idsProcesados.Add(idCompra);

                    if (row.Table.Columns.Contains("tipoCompra") && row["tipoCompra"] != DBNull.Value)
                    {
                        string tipoCompra = Convert.ToString(row["tipoCompra"]);
                        if (soloPesajes)
                        {
                            if (!EsPesaje(tipoCompra))
                                continue;
                        }
                        else if (soloComprasPesaje && !EsCompraSeleccionableParaPesaje(tipoCompra))
                        {
                            continue;
                        }
                    }

                    DateTime fechaCompra = row.Table.Columns.Contains("fechaCompra") && row["fechaCompra"] != DBNull.Value
                        ? Convert.ToDateTime(row["fechaCompra"])
                        : DateTime.MinValue;
                    string proveedorNombre = row.Table.Columns.Contains("razonSocial") && row["razonSocial"] != DBNull.Value ? Convert.ToString(row["razonSocial"]) : "";
                    if (!string.IsNullOrWhiteSpace(proveedorFiltro)
                        && proveedorNombre.IndexOf(proveedorFiltro, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    int cantMedias = row.Table.Columns.Contains("cantMedias") && row["cantMedias"] != DBNull.Value
                        ? Convert.ToInt32(row["cantMedias"])
                        : 0;
                    float kgsMedias = row.Table.Columns.Contains("kgsMedias") && row["kgsMedias"] != DBNull.Value
                        ? Convert.ToSingle(row["kgsMedias"])
                        : 0f;
                    float totalKg = row.Table.Columns.Contains("cantKg") && row["cantKg"] != DBNull.Value
                        ? Convert.ToSingle(row["cantKg"])
                        : 0f;

                    items.Add(new CompraPesajeListadoVm
                    {
                        IdCompra = idCompra,
                        IdProveedor = row.Table.Columns.Contains("idProveedor") && row["idProveedor"] != DBNull.Value ? Convert.ToInt32(row["idProveedor"]) : 0,
                        FechaCompraTexto = fechaCompra != DateTime.MinValue ? fechaCompra.ToString("dd/MM/yyyy HH:mm") : "-",
                        Proveedor = proveedorNombre,
                        TipoCompra = row.Table.Columns.Contains("tipoCompra") && row["tipoCompra"] != DBNull.Value ? Convert.ToString(row["tipoCompra"]) : "",
                        CantMedias = cantMedias,
                        KgsMedias = kgsMedias,
                        TotalKg = totalKg,
                        Sucursal = row.Table.Columns.Contains("Sucursal") && row["Sucursal"] != DBNull.Value
                            ? Convert.ToString(row["Sucursal"])
                            : (row.Table.Columns.Contains("sucursal") && row["sucursal"] != DBNull.Value ? Convert.ToString(row["sucursal"]) : ""),
                        EsActual = idCompraActual > 0 && idCompra == idCompraActual
                    });
                }

                return Json(new
                {
                    ok = true,
                    items = items.Select(c => new
                    {
                        idCompra = c.IdCompra,
                        idProveedor = c.IdProveedor,
                        proveedor = c.Proveedor,
                        fechaCompra = c.FechaCompraTexto,
                        tipoCompra = c.TipoCompra,
                        cantMedias = c.CantMedias,
                        kgsMedias = c.KgsMedias,
                        totalKg = c.TotalKg,
                        sucursal = c.Sucursal,
                        esActual = c.EsActual
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DetalleCompraPesaje(int idCompra)
        {
            if (idCompra <= 0)
                return Json(new { ok = false, mensaje = "Seleccione una compra valida." });

            try
            {
                var compra = _oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra <= 0)
                    return Json(new { ok = false, mensaje = "No se encontro la compra seleccionada." });

                if (TiposStock.Any(x => string.Equals(x, compra.TipoCompra, StringComparison.OrdinalIgnoreCase))
                    && !EsPesaje(compra.TipoCompra))
                    return Json(new { ok = false, mensaje = "La compra seleccionada no aplica para pesaje." });

                var lineas = ConstruirLineasCompraParaSeleccion(compra);
                float totalKg = lineas.Sum(x => ParseFloatFlexibleLocal(x.KilosTexto));
                float kgsMedias = compra.KgsMedias.HasValue && compra.KgsMedias.Value > 0 ? compra.KgsMedias.Value : totalKg;

                return Json(new
                {
                    ok = true,
                    item = new
                    {
                        idCompra = compra.IdCompra,
                        idProveedor = compra.Proveedor != null ? compra.Proveedor.IdPersona : 0,
                        proveedor = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "",
                        proveedorCuit = compra.Proveedor != null ? compra.Proveedor.Cuit : "",
                        fechaCompra = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm"),
                        tipoCompra = compra.TipoCompra ?? "",
                        cantMedias = compra.CantMedias ?? 0,
                        kgsMedias = kgsMedias,
                        totalKg = totalKg,
                        lineas = lineas.Select(l => new
                        {
                            idCorte = l.IdCorte,
                            codigo = l.Codigo,
                            producto = l.Producto,
                            cantidad = l.CantidadTexto,
                            kilos = l.KilosTexto,
                            pesable = l.Pesable
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ProductosNoCargadosCierre(int idSucursal, DateTime fechaCompra, int idCompra = 0, long[] codigosCargados = null)
        {
            DateTime fechaConsulta = fechaCompra == DateTime.MinValue ? DateTime.Today : fechaCompra;

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
        public IActionResult VerPorcentajesPesaje(int idCompra)
        {
            var pesaje = _oCompraN.findById_convertToCompra(idCompra);
            if (pesaje == null || pesaje.IdCompra <= 0 || !EsPesaje(pesaje.TipoCompra))
                return Json(new { ok = false, mensaje = "No se encontró el pesaje seleccionado." });

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
                int idAjuste = _oCompraN.getIdAjusteDelPesaje(idCompra);
                var estado = _oCompraN.estadoAjusteStock(idCompra, idAjuste);
                DataTable dtPromMedias = _oCompraN.getPromMedias(idCompra) ?? new DataTable();
                DataTable dtPorcCortes = _oCompraN.getPorcCortesEnMedias(idCompra) ?? new DataTable();

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
        public IActionResult GenerarAjustePesaje(int idCompra)
        {
            Entidades.Usuario user = _usuarioActual;

            var pesaje = _oCompraN.findById_convertToCompra(idCompra);
            if (pesaje == null || pesaje.IdCompra <= 0 || !EsPesaje(pesaje.TipoCompra))
                return Json(new { ok = false, mensaje = "No se encontró el pesaje seleccionado." });

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
                int idAjuste = _oCompraN.getIdAjusteDelPesaje(idCompra);
                var ajuste = idAjuste > 0 ? _oCompraN.findById_convertToCompra(idAjuste) : new Entidades.Compra();

                ajuste.NroRemito = pesaje.IdCompra.ToString();
                ajuste.Proveedor = pesaje.Proveedor;
                ajuste.FechaCompra = pesaje.FechaCompra;
                ajuste.Estado = "";
                string observacionesActuales = (ajuste.Observaciones ?? string.Empty).Trim();
                if (string.Equals(observacionesActuales, "ID Pesaje: " + pesaje.IdCompra, StringComparison.OrdinalIgnoreCase))
                    ajuste.Observaciones = "";
                ajuste.TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock);
                ajuste.CantMedias = pesaje.CantMedias;
                ajuste.KgsMedias = pesaje.KgsMedias;
                ajuste.IdPesajeAjustado = pesaje.IdCompra;
                ajuste.Sucursal = pesaje.Sucursal;

                if (ajuste.IdCompra <= 0)
                {
                    ajuste.CreadoPor = user;
                    ajuste.IdCompra = _oCompraN.agregarCompra(ajuste);
                }
                else
                {
                    ajuste.ActualizadoPor = user;
                    _oCompraN.modificarCompra(ajuste);
                    _oCompraN.limpiarCortesPorCompra(ajuste.IdCompra);
                }

                DataTable dtPorcCortes = _oCompraN.getPorcCortesEnMedias(idCompra) ?? new DataTable();
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

                    _oCompraN.agregarCortePorCompra(cortePorCompra);
                }

                _oCompraN.actualizarEstadoPesaje(pesaje.IdCompra, Entidades.Compra.estadoAjusteStock.Actualizado);

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

        private static string NormalizarTipoFiltro(string tipoCompra)
        {
            if (string.IsNullOrWhiteSpace(tipoCompra))
                return "Ver Todos";

            if (string.Equals(tipoCompra, "Todos", StringComparison.OrdinalIgnoreCase))
                return "Ver Todos";

            return tipoCompra.Trim();
        }

        private static bool EsTipoStock(string tipoCompra)
        {
            return TiposStock.Contains(tipoCompra ?? "", StringComparer.OrdinalIgnoreCase);
        }

        private static bool EsPesaje(string tipoCompra)
        {
            string tipo = (tipoCompra ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tipo))
                return false;

            return string.Equals(tipo,
                    Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes),
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipo, "Pesaje", StringComparison.OrdinalIgnoreCase)
                || tipo.IndexOf("Pesaje", StringComparison.OrdinalIgnoreCase) >= 0;
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

        private static string NormalizarTipoOperacion(string tipoCompra)
        {
            if (string.IsNullOrWhiteSpace(tipoCompra))
                return "";

            string tipo = tipoCompra.Trim();
            return EsTipoStock(tipo) ? tipo : "";
        }

        private static string ObtenerSignoStock(string tipoCompra)
        {
            if (string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock), StringComparison.OrdinalIgnoreCase))
                return "+";
            if (string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock), StringComparison.OrdinalIgnoreCase))
                return "-";
            if (string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock), StringComparison.OrdinalIgnoreCase))
                return "+/-";
            if (string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), StringComparison.OrdinalIgnoreCase))
                return "=";
            if (string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes), StringComparison.OrdinalIgnoreCase))
                return "P";

            return "-";
        }

        private static bool CoincideProductoStock(string codigo, string descripcion, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return true;

            string texto = filtro.Trim();
            return (codigo ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || (descripcion ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
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

        private Dictionary<int, CompraIndexDetalleVm> ConstruirDetallesIndex(DataTable dt)
        {
            var detalles = new Dictionary<int, CompraIndexDetalleVm>();
            if (dt == null)
                return detalles;

            var filasPorIdCompra = dt.AsEnumerable()
                .Where(x => x.Table.Columns.Contains("idCompra") && x["idCompra"] != DBNull.Value)
                .GroupBy(x => Convert.ToInt32(x["idCompra"]))
                .ToDictionary(x => x.Key, x => x.First());
            var idsPesaje = filasPorIdCompra.Values
                .Where(x => EsPesaje(LeerString(x, "tipoCompra")))
                .Select(x => LeerInt(x, "idCompra"))
                .Where(x => x > 0)
                .ToList();
            var ajustesPorPesaje = _oCompraN.getIdsAjustePorPesajes(idsPesaje);
            var pesajesHijosPorDestino = _oCompraN.obtenerPesajesVinculadosPorDestinos(idsPesaje);

            foreach (DataRow row in dt.Rows)
            {
                int idCompra = Convert.ToInt32(row["idCompra"]);
                if (detalles.ContainsKey(idCompra))
                    continue;

                detalles[idCompra] = ConstruirDetalleIndexLiviano(row, filasPorIdCompra, ajustesPorPesaje, pesajesHijosPorDestino);
            }

            return detalles;
        }

        private CompraIndexDetalleVm ConstruirDetalleIndexLiviano(DataRow row, Dictionary<int, DataRow> filasPorIdCompra, Dictionary<int, int> ajustesPorPesaje, Dictionary<int, List<int>> pesajesHijosPorDestino)
        {
            int idCompra = LeerInt(row, "idCompra");
            string tipoCompra = LeerString(row, "tipoCompra");
            bool esPesaje = EsPesaje(tipoCompra);
            bool esAjuste = EsAjuste(tipoCompra);
            int? idPesajeRelacionado = LeerIntNullable(row, "idPesajeAjustado");
            int? idAjusteRelacionado = null;

            if (esPesaje)
            {
                int ajusteRelacionado = 0;
                if (ajustesPorPesaje != null)
                    ajustesPorPesaje.TryGetValue(idCompra, out ajusteRelacionado);
                if (ajusteRelacionado > 0)
                    idAjusteRelacionado = ajusteRelacionado;
            }

            DataRow filaRelacionada = null;
            if (idPesajeRelacionado.HasValue && filasPorIdCompra != null)
                filasPorIdCompra.TryGetValue(idPesajeRelacionado.Value, out filaRelacionada);

            bool compraVinculadaEsPesaje = esPesaje && filaRelacionada != null && EsPesaje(LeerString(filaRelacionada, "tipoCompra"));

            List<int> pesajesHijos = null;
            if (esPesaje && pesajesHijosPorDestino != null)
                pesajesHijosPorDestino.TryGetValue(idCompra, out pesajesHijos);

            return new CompraIndexDetalleVm
            {
                IdCompra = idCompra,
                FechaCompra = LeerDateTimeNullable(row, "fechaCompra"),
                TipoCompra = tipoCompra,
                Cantidad = row.Table.Columns.Contains("cantKg") && row["cantKg"] != DBNull.Value ? Convert.ToSingle(row["cantKg"]) : 0f,
                CantidadMedias = LeerInt(row, "cantMedias"),
                Sucursal = LeerString(row, "sucursal", "Sucursal", "sucursalNombre"),
                Observaciones = LeerString(row, "observaciones"),
                Estado = LeerString(row, "estado"),
                IdCompraVinculada = esPesaje ? idPesajeRelacionado : null,
                FechaCompraVinculada = esPesaje ? LeerDateTimeNullable(filaRelacionada, "fechaCompra") : null,
                ProveedorCompraVinculada = esPesaje ? LeerString(filaRelacionada, "razonSocial") : "",
                CantMediasCompraVinculada = esPesaje ? LeerIntNullable(filaRelacionada, "cantMedias") : null,
                KgsCompraVinculada = esPesaje ? LeerFloatNullable(filaRelacionada, "kgsMedias") : null,
                EstadoCompraVinculada = "",
                IdPesajeRelacionado = idPesajeRelacionado,
                IdAjusteRelacionado = idAjusteRelacionado,
                FechaPesajeRelacionado = esAjuste ? LeerDateTimeNullable(filaRelacionada, "fechaCompra") : null,
                ProveedorPesajeRelacionado = esAjuste ? LeerString(filaRelacionada, "razonSocial") : "",
                CantMediasPesajeRelacionado = esAjuste ? LeerIntNullable(filaRelacionada, "cantMedias") : null,
                KgsPesajeRelacionado = esAjuste ? LeerFloatNullable(filaRelacionada, "kgsMedias") : null,
                EstadoPesajeRelacionado = "",
                EsPesaje = esPesaje,
                EsAjuste = esAjuste,
                CompraVinculadaEsPesaje = compraVinculadaEsPesaje,
                PesajesHijosVinculadosIds = pesajesHijos ?? new List<int>(),
                UsuarioCreacion = LeerString(row, "CreadoPor", "creadoPorNombre", "usuarioCreacion"),
                FechaCreacion = LeerDateTimeNullable(row, "creado", "fechaCreacion"),
                UsuarioActualizacion = LeerString(row, "ActualizadoPor", "actualizadoPorNombre", "usuarioActualizacion"),
                FechaActualizacion = LeerDateTimeNullable(row, "actualizado", "fechaActualizacion")
            };
        }

        private CompraIndexDetalleVm ConstruirDetalleIndexCompleto(Entidades.Compra compra)
        {
            if (compra == null || compra.IdCompra <= 0)
                return null;

            bool esPesaje = EsPesaje(compra.TipoCompra);
            bool esAjuste = EsAjuste(compra.TipoCompra);
            int? idPesajeRelacionado = null;
            int? idAjusteRelacionado = null;
            Entidades.Compra pesajeRelacionado = null;
            string estadoPesajeRelacionado = "";
            var lineas = (_oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>())
                .Select(corte => new StockLineaDetalleVm
                {
                    Codigo = corte.Corte != null ? corte.Corte.Codigo.ToString() : "-",
                    Producto = corte.Corte != null ? corte.Corte.CorteDesc : "-",
                    CantidadKgTexto = corte.CantKgs.ToString("N3"),
                    Signo = ObtenerSignoStock(compra.TipoCompra),
                    Observacion = corte.Balanza ? "Peso balanza" : "-",
                    Balanza = corte.Balanza,
                    CreadoTexto = FormatearFechaHora(corte.Creado),
                    CantidadKg = Convert.ToDecimal(corte.CantKgs)
                })
                .ToList();

            bool compraVinculadaEsPesaje = false;
            List<int> pesajesHijos = new List<int>();

            if (esPesaje)
            {
                int ajusteRelacionado = _oCompraN.getIdAjusteDelPesaje(compra.IdCompra);
                if (ajusteRelacionado > 0)
                    idAjusteRelacionado = ajusteRelacionado;

                idPesajeRelacionado = compra.IdPesajeAjustado;
                if (idPesajeRelacionado.HasValue && idPesajeRelacionado.Value > 0)
                {
                    pesajeRelacionado = _oCompraN.findById_convertToCompra(idPesajeRelacionado.Value);
                    if (pesajeRelacionado == null || pesajeRelacionado.IdCompra <= 0)
                        estadoPesajeRelacionado = "No se encontro la compra vinculada.";
                    else
                        compraVinculadaEsPesaje = EsPesaje(pesajeRelacionado.TipoCompra);
                }

                pesajesHijos = _oCompraN.obtenerPesajesVinculadosPorDestino(compra.IdCompra) ?? new List<int>();
            }
            else if (esAjuste)
            {
                idPesajeRelacionado = compra.IdPesajeAjustado;
                if (!idPesajeRelacionado.HasValue || idPesajeRelacionado.Value <= 0)
                {
                    estadoPesajeRelacionado = "No tiene asignado un pesaje.";
                }
                else
                {
                    pesajeRelacionado = _oCompraN.findById_convertToCompra(idPesajeRelacionado.Value);
                    if (pesajeRelacionado == null || pesajeRelacionado.IdCompra <= 0)
                        estadoPesajeRelacionado = "No se encontro la referencia del pesaje asignado.";
                }
            }

            return new CompraIndexDetalleVm
            {
                IdCompra = compra.IdCompra,
                FechaCompra = compra.FechaCompra,
                TipoCompra = compra.TipoCompra ?? "",
                Cantidad = lineas.Sum(x => Convert.ToSingle(x.CantidadKg)),
                Sucursal = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "",
                Observaciones = compra.Observaciones ?? "",
                Estado = compra.Estado ?? "",
                IdCompraVinculada = esPesaje ? idPesajeRelacionado : null,
                FechaCompraVinculada = esPesaje && pesajeRelacionado != null ? (DateTime?)pesajeRelacionado.FechaCompra : null,
                ProveedorCompraVinculada = esPesaje && pesajeRelacionado != null && pesajeRelacionado.Proveedor != null ? pesajeRelacionado.Proveedor.RazonSocial : "",
                CantMediasCompraVinculada = esPesaje && pesajeRelacionado != null
                    ? (pesajeRelacionado.CantMedias.HasValue ? pesajeRelacionado.CantMedias : compra.CantMedias)
                    : null,
                KgsCompraVinculada = esPesaje && pesajeRelacionado != null
                    ? (pesajeRelacionado.KgsMedias.HasValue ? pesajeRelacionado.KgsMedias : compra.KgsMedias)
                    : null,
                EstadoCompraVinculada = esPesaje ? estadoPesajeRelacionado : "",
                IdPesajeRelacionado = idPesajeRelacionado,
                IdAjusteRelacionado = idAjusteRelacionado,
                FechaPesajeRelacionado = pesajeRelacionado != null ? (DateTime?)pesajeRelacionado.FechaCompra : null,
                ProveedorPesajeRelacionado = pesajeRelacionado != null && pesajeRelacionado.Proveedor != null ? pesajeRelacionado.Proveedor.RazonSocial : "",
                CantMediasPesajeRelacionado = pesajeRelacionado != null ? pesajeRelacionado.CantMedias : null,
                KgsPesajeRelacionado = pesajeRelacionado != null ? pesajeRelacionado.KgsMedias : null,
                EstadoPesajeRelacionado = estadoPesajeRelacionado,
                EsPesaje = esPesaje,
                EsAjuste = esAjuste,
                CompraVinculadaEsPesaje = compraVinculadaEsPesaje,
                PesajesHijosVinculadosIds = pesajesHijos,
                UsuarioCreacion = compra.CreadoPor != null ? compra.CreadoPor.Nombre : "",
                FechaCreacion = compra.Creado,
                UsuarioActualizacion = compra.ActualizadoPor != null ? compra.ActualizadoPor.Nombre : "",
                FechaActualizacion = compra.Actualizado,
                Lineas = lineas
            };
        }

        private static string LeerString(DataRow row, params string[] columnas)
        {
            if (row == null || row.Table == null || columnas == null)
                return string.Empty;

            foreach (var columna in columnas)
            {
                if (!string.IsNullOrWhiteSpace(columna)
                    && row.Table.Columns.Contains(columna)
                    && row[columna] != DBNull.Value)
                {
                    return Convert.ToString(row[columna]) ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static int LeerInt(DataRow row, params string[] columnas)
        {
            int? valor = LeerIntNullable(row, columnas);
            return valor ?? 0;
        }

        private static int? LeerIntNullable(DataRow row, params string[] columnas)
        {
            if (row == null || row.Table == null || columnas == null)
                return null;

            foreach (var columna in columnas)
            {
                if (!string.IsNullOrWhiteSpace(columna)
                    && row.Table.Columns.Contains(columna)
                    && row[columna] != DBNull.Value)
                {
                    int valor;
                    if (int.TryParse(Convert.ToString(row[columna]), out valor))
                        return valor;
                }
            }

            return null;
        }

        private static float? LeerFloatNullable(DataRow row, params string[] columnas)
        {
            if (row == null || row.Table == null || columnas == null)
                return null;

            foreach (var columna in columnas)
            {
                if (!string.IsNullOrWhiteSpace(columna)
                    && row.Table.Columns.Contains(columna)
                    && row[columna] != DBNull.Value)
                {
                    float valor;
                    if (float.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.CurrentCulture, out valor)
                        || float.TryParse(Convert.ToString(row[columna]), NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                    {
                        return valor;
                    }
                }
            }

            return null;
        }

        private static DateTime? LeerDateTimeNullable(DataRow row, params string[] columnas)
        {
            if (row == null || row.Table == null || columnas == null)
                return null;

            foreach (var columna in columnas)
            {
                if (!string.IsNullOrWhiteSpace(columna)
                    && row.Table.Columns.Contains(columna)
                    && row[columna] != DBNull.Value)
                {
                    DateTime valor;
                    if (DateTime.TryParse(Convert.ToString(row[columna]), out valor))
                        return valor;
                }
            }

            return null;
        }

        private static string FormatearFechaHora(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        }

        // Resuelve quien queda como creador real de una operacion cuando el usuario logueado es
        // el usuario compartido de sala de produccion (ver Web/Controllers/BaseController.cs:293).
        // Con el stub admin de esta migracion (EsUsuarioProduccion=false) siempre devuelve
        // usuarioSesion sin cambios -- se porta igual (no se omite) porque es codigo trivial y
        // deja la puerta abierta a un login real futuro sin volver a tocar este archivo.
        private Entidades.Usuario ResolverUsuarioCreador(int idUsuarioCreador, Entidades.Usuario usuarioSesion)
        {
            if (usuarioSesion == null || !usuarioSesion.EsUsuarioProduccion || idUsuarioCreador <= 0)
                return usuarioSesion;

            var oUsuarioN = new Negocio.Usuario(_empresa, _param);
            var candidato = oUsuarioN.getUsuarioById(idUsuarioCreador);
            if (candidato == null || !candidato.Activo || candidato.IdEmpresa != usuarioSesion.IdEmpresa)
                return usuarioSesion;

            return candidato;
        }

        private StockEditVm CrearViewModelNuevo(Entidades.Usuario user, string tipoCompra)
        {
            int idSucursal = user != null && user.IdSucursal > 0 ? user.IdSucursal : 0;
            Entidades.Sucursal sucursal = idSucursal > 0 ? _oSucursalN.findById(idSucursal) : null;
            int idProveedor = _param.GetInt(Entidades.ParamKeys.IdIndefinido, 0);

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
                IdProveedor = compra.Proveedor != null ? compra.Proveedor.IdPersona : _param.GetInt(Entidades.ParamKeys.IdIndefinido, 0),
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

            CargarCompraVinculadaEnPesajeEdicion(model, compra);
            CargarPesajeAjustadoEnEdicion(model, compra);

            // Precarga los pesajes ya vinculados a este (si es un Pesaje "destino") -- sin esto,
            // SincronizarPesajesVinculados() los desvincula en silencio al guardar cualquier cambio
            // no relacionado (idsActuales arranca vacio, y todo lo que no esta en idsActuales se
            // trata como "a desvincular"). Bug real encontrado en vivo el 2026-08-13 en el
            // original, ver docs/DECISIONS.md.
            if (EsPesaje(compra.TipoCompra))
            {
                model.PesajesVinculadosIds = _oCompraN.obtenerPesajesVinculadosPorDestino(compra.IdCompra) ?? new List<int>();
            }

            var cortes = (_oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>())
                .OrderBy(c => c.Creado ?? DateTime.MinValue)
                .ThenBy(c => c.IdCortePorCompra)
                .ToList();
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
                    Pesable = corte.Corte != null && corte.Corte.Pesable,
                    IdPesajeVinculado = null,
                    PesajeVinculadoTexto = ""
                });
            }

            RecalcularTotales(model);
            return model;
        }

        private void CargarPesajeAjustadoEnEdicion(StockEditVm model, Entidades.Compra compra)
        {
            if (model == null || compra == null || !EsAjuste(compra.TipoCompra))
                return;

            model.IdPesajeAjustado = compra.IdPesajeAjustado;
            model.FechaPesajeAjustado = null;
            model.ProveedorPesajeAjustado = "";
            model.CantMediasPesajeAjustado = null;
            model.KgsPesajeAjustado = null;
            if (!compra.IdPesajeAjustado.HasValue || compra.IdPesajeAjustado.Value <= 0)
            {
                model.EstadoPesajeAjustado = "No tiene asignado un pesaje.";
                return;
            }

            Entidades.Compra pesaje = _oCompraN.findById_convertToCompra(compra.IdPesajeAjustado.Value);
            if (pesaje == null || pesaje.IdCompra <= 0)
            {
                model.EstadoPesajeAjustado = "No se encontro la referencia del pesaje asignado.";
                return;
            }

            model.IdPesajeAjustado = pesaje.IdCompra;
            model.FechaPesajeAjustado = pesaje.FechaCompra;
            model.ProveedorPesajeAjustado = pesaje.Proveedor != null ? pesaje.Proveedor.RazonSocial : "";
            model.CantMediasPesajeAjustado = pesaje.CantMedias;
            model.KgsPesajeAjustado = pesaje.KgsMedias;
            model.EstadoPesajeAjustado = "";
        }

        private void CargarCompraVinculadaEnPesajeEdicion(StockEditVm model, Entidades.Compra compra)
        {
            if (model == null || compra == null || !EsPesaje(compra.TipoCompra))
                return;

            model.IdPesajeAjustado = compra.IdPesajeAjustado;
            model.FechaCompraVinculada = null;
            model.ProveedorCompraVinculada = "";
            model.CantMediasCompraVinculada = null;
            model.KgsCompraVinculada = null;
            model.CompraVinculadaEsPesaje = false;
            if (!compra.IdPesajeAjustado.HasValue || compra.IdPesajeAjustado.Value <= 0)
            {
                model.EstadoCompraVinculada = "";
                return;
            }

            Entidades.Compra compraVinculada = _oCompraN.findById_convertToCompra(compra.IdPesajeAjustado.Value);
            if (compraVinculada == null || compraVinculada.IdCompra <= 0)
            {
                model.EstadoCompraVinculada = "No se encontro la compra vinculada.";
                return;
            }

            model.IdPesajeAjustado = compraVinculada.IdCompra;
            model.FechaCompraVinculada = compraVinculada.FechaCompra;
            model.ProveedorCompraVinculada = compraVinculada.Proveedor != null ? compraVinculada.Proveedor.RazonSocial : "";
            model.CantMediasCompraVinculada = compraVinculada.CantMedias.HasValue ? compraVinculada.CantMedias : compra.CantMedias;
            model.KgsCompraVinculada = compraVinculada.KgsMedias.HasValue ? compraVinculada.KgsMedias : compra.KgsMedias;
            model.CompraVinculadaEsPesaje = EsPesaje(compraVinculada.TipoCompra);
            model.EstadoCompraVinculada = "";
        }

        private void CargarDatosRelacionadosEnModelo(StockEditVm model, Entidades.Compra compraActual)
        {
            if (model == null)
                return;

            string tipoCompra = !string.IsNullOrWhiteSpace(model.TipoCompra)
                ? model.TipoCompra
                : (compraActual != null ? compraActual.TipoCompra : "");
            int? idRelacionado = model.IdPesajeAjustado.HasValue
                ? model.IdPesajeAjustado
                : (compraActual != null ? compraActual.IdPesajeAjustado : null);

            if (EsPesaje(tipoCompra))
            {
                CargarCompraVinculadaEnPesajeEdicion(model, new Entidades.Compra
                {
                    TipoCompra = tipoCompra,
                    IdPesajeAjustado = idRelacionado
                });
                return;
            }

            if (EsAjuste(tipoCompra))
            {
                CargarPesajeAjustadoEnEdicion(model, new Entidades.Compra
                {
                    TipoCompra = tipoCompra,
                    IdPesajeAjustado = idRelacionado
                });
            }
        }

        private void CargarViewBags(StockEditVm model, int idUsuarioCreadorPreseleccionado = 0)
        {
            ViewBag.Title = model.EsEdicion ? "Modificar Stock" : "Nuevo Stock";
            ViewBag.Seccion = "Stock";
            ViewBag.Sucursales = _oSucursalN.findAll();
            ViewBag.UrlBuscarPersonaModal = Url.Action("Buscar", "Personas");
            ViewBag.UrlPersonaListar = Url.Action("Listar", "Personas");

            // TODO(claude): _usuarioActual.EsUsuarioProduccion es siempre false (stub admin, ver
            // constructor de la clase) -- ObtenerUsuariosActivosEmpresaParaCombo() (helper de
            // BaseController que arma el combo del selector de usuario de produccion) no se porta
            // porque su resultado nunca se usa mientras esto sea false; se deja una lista vacia,
            // mismo resultado observable que el original con este usuario.
            ViewBag.EsUsuarioProduccion = _usuarioActual.EsUsuarioProduccion;
            ViewBag.UsuariosActivosEmpresa = new List<object>();
            ViewBag.IdUsuarioCreadorPreseleccionado =
                _usuarioActual.EsUsuarioProduccion && idUsuarioCreadorPreseleccionado > 0
                    ? ResolverUsuarioCreador(idUsuarioCreadorPreseleccionado, _usuarioActual).Id
                    : 0;
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

                if (!model.GuardarSinPesaje && (!model.CantMedias.HasValue || model.CantMedias.Value <= 0))
                    return "Ingrese la cantidad de medias para el pesaje.";

                if (!model.GuardarSinPesaje && (!model.KgsMedias.HasValue || model.KgsMedias.Value <= 0))
                    return "Ingrese los kilos de medias para el pesaje.";
            }

            return "";
        }

        private Entidades.Persona ResolverProveedor(int idProveedor)
        {
            int id = idProveedor > 0 ? idProveedor : _param.GetInt(Entidades.ParamKeys.IdIndefinido, 0);
            return id > 0 ? _oPersonaN.findById(id) : null;
        }

        private void CompletarDatosProveedor(StockEditVm model)
        {
            if (model == null || !EsPesaje(model.TipoCompra))
                return;

            if (model.IdProveedor <= 0)
            {
                model.ProveedorNombre = "";
                model.ProveedorCuit = "";
                return;
            }

            var proveedor = ResolverProveedor(model.IdProveedor);
            model.ProveedorNombre = proveedor != null ? proveedor.RazonSocial : "";
            model.ProveedorCuit = proveedor != null ? proveedor.Cuit : "";
        }

        private void AplicarGuardarSinPesaje(StockEditVm model)
        {
            if (model == null || !EsPesaje(model.TipoCompra) || !model.GuardarSinPesaje)
                return;

            model.CantMedias = 0;
            model.KgsMedias = 0f;
        }

        private void SincronizarPesajesVinculados(Entidades.Compra compraDestino, IEnumerable<int> pesajesVinculadosIds, Entidades.Usuario user)
        {
            if (compraDestino == null || compraDestino.IdCompra <= 0 || !EsPesaje(compraDestino.TipoCompra))
                return;

            var idsActuales = (pesajesVinculadosIds ?? Enumerable.Empty<int>())
                .Where(id => id > 0 && id != compraDestino.IdCompra)
                .Distinct()
                .ToList();

            var idsPrevios = _oCompraN.obtenerPesajesVinculadosPorDestino(compraDestino.IdCompra) ?? new List<int>();

            foreach (int idPesaje in idsActuales)
            {
                _oCompraN.actualizarIdPesajeAjustado(idPesaje, compraDestino.IdCompra, user);
            }

            foreach (int idPesaje in idsPrevios.Except(idsActuales))
            {
                _oCompraN.actualizarIdPesajeAjustado(idPesaje, null, user);
            }
        }

        private static DateTime? ParseFechaHoraStockLinea(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            DateTime value;
            string[] formatos =
            {
                "dd/MM/yyyy HH:mm",
                "d/M/yyyy HH:mm",
                "dd/MM/yyyy H:mm",
                "d/M/yyyy H:mm"
            };

            if (DateTime.TryParseExact(text.Trim(), formatos, CultureInfo.CurrentCulture, DateTimeStyles.None, out value)
                || DateTime.TryParseExact(text.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
                || DateTime.TryParse(text.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out value)
                || DateTime.TryParse(text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
            {
                return value;
            }

            return null;
        }

        private static string BuildDraftKey(Entidades.Usuario user, int idSucursal, string tipoCompra, int idCompra)
        {
            int idUsuario = user != null ? user.Id : 0;
            return "stock_draft_" + idUsuario + "_" + idSucursal + "_" + tipoCompra + "_" + idCompra;
        }

        private static void RecalcularTotales(StockEditVm model)
        {
            model.CantItems = model.Lineas != null ? model.Lineas.Count : 0;
            model.TotalKg = 0f;

            foreach (var linea in model.Lineas ?? new List<StockLineaVm>())
            {
                model.TotalKg += linea.CantKgs;
            }
        }

        private Entidades.ExistenciaStockPorSucursalFiltroVm CrearFiltroExistencia(Entidades.Usuario user)
        {
            var filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();
            int idSucursalActual = user != null && user.IdSucursal > 0 ? user.IdSucursal : 0;
            List<Entidades.Sucursal> sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();

            filtro.IdSucursal = idSucursalActual;
            filtro.FechaHasta = DateTime.Now;
            filtro.SucursalesDisponibles.Add(new Entidades.SucursalColumnaStockVm
            {
                IdSucursal = 0,
                Sucursal = "Todas"
            });

            foreach (var sucursal in sucursales
                .Where(x => x != null && x.IdSucursal > 0)
                .GroupBy(x => x.IdSucursal)
                .Select(g => g.First())
                .OrderBy(x => x.SucursalNombre))
            {
                filtro.SucursalesDisponibles.Add(new Entidades.SucursalColumnaStockVm
                {
                    IdSucursal = sucursal.IdSucursal,
                    Sucursal = sucursal.SucursalNombre
                });
            }

            filtro.TiposDisponibles = ObtenerTiposExistencia();
            filtro.ProveedoresDisponibles = ObtenerProveedoresExistencia();
            filtro.MarcasDisponibles = ObtenerMarcasExistencia();
            AplicarUltimosCierres(filtro, sucursales);
            return filtro;
        }

        // El calculo de existencia (a_ExistenciaStockPorSucursales) usa el ultimo cierre de stock
        // registrado como punto de partida sin importar que FechaHasta se pida -- pedir una fecha
        // anterior a ese cierre da un resultado invalido (ver docs/DECISIONS.md). Esto calcula ese
        // limite para la/s sucursal/es relevantes del filtro actual, para mostrarlo en la pantalla
        // y para que BuscarExistenciaPorSucursales pueda rechazar una consulta invalida.
        private void AplicarUltimosCierres(Entidades.ExistenciaStockPorSucursalFiltroVm filtro, List<Entidades.Sucursal> sucursalesTodas)
        {
            var sucursalesRelevantes = (filtro.IdSucursal > 0
                ? sucursalesTodas.Where(s => s != null && s.IdSucursal == filtro.IdSucursal)
                : sucursalesTodas.Where(s => s != null && s.IdSucursal > 0))
                .GroupBy(s => s.IdSucursal)
                .Select(g => g.First())
                .ToList();

            filtro.UltimosCierresPorSucursal = _oCorteN.ObtenerUltimosCierresPorSucursal(sucursalesRelevantes);
            filtro.FechaMinimaConsulta = filtro.UltimosCierresPorSucursal.Any()
                ? filtro.UltimosCierresPorSucursal.Max(x => x.FechaUltimoCierre)
                : (DateTime?)null;
        }

        private List<string> ObtenerTiposExistencia()
        {
            var tipos = new List<string>();
            DataTable dtTipos;
            try
            {
                dtTipos = _oCorteN.obtenerTiposProductoGrilla("") ?? new DataTable();
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
                dt = _oPersonaN.buscarProveedor("") ?? new DataTable();
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
                dt = _oPersonaN.buscarPersona("", true) ?? new DataTable();
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

        private static bool EsCompraSeleccionableParaPesaje(string tipoCompra)
        {
            string tipo = (tipoCompra ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tipo))
                return false;

            return string.Equals(tipo,
                    Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes),
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipo,
                    Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes),
                    StringComparison.OrdinalIgnoreCase);
        }

        private DataTable UnirComprasParaPesajeSeleccion(int sucursal, DateTime desde, DateTime hasta)
        {
            var tipos = new[]
            {
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes),
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes)
            };

            DataTable resultado = null;
            foreach (string tipo in tipos)
            {
                DataTable dtTipo = _oCompraN.obtenerCompras(sucursal, tipo, "", desde, hasta, null) ?? new DataTable();
                if (resultado == null)
                {
                    resultado = dtTipo.Clone();
                }

                foreach (DataRow row in dtTipo.Rows)
                {
                    resultado.ImportRow(row);
                }
            }

            return resultado ?? new DataTable();
        }

        private List<CompraPesajeSeleccionLineaVm> ConstruirLineasCompraParaSeleccion(Entidades.Compra compra)
        {
            var lineas = new List<CompraPesajeSeleccionLineaVm>();
            if (compra == null || compra.IdCompra <= 0)
                return lineas;

            var cortes = _oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>();
            foreach (var corte in cortes)
            {
                float kilos = corte.CantKgs;
                lineas.Add(new CompraPesajeSeleccionLineaVm
                {
                    IdCorte = corte.Corte != null ? corte.Corte.IdCorte : 0,
                    Codigo = corte.Corte != null ? corte.Corte.Codigo : 0,
                    Producto = corte.Corte != null ? corte.Corte.CorteDesc : "-",
                    CantidadTexto = kilos.ToString("N3"),
                    KilosTexto = kilos.ToString("N3"),
                    Pesable = corte.Corte != null && corte.Corte.Pesable
                });
            }

            if (lineas.Count > 0)
                return lineas;

            DataTable dtMedias = _oCompraN.obtenerMediasPorCompra(compra.IdCompra) ?? new DataTable();
            foreach (DataRow row in dtMedias.Rows)
            {
                float kg = row.Table.Columns.Contains("kgMedia") && row["kgMedia"] != DBNull.Value ? Convert.ToSingle(row["kgMedia"]) : 0f;
                lineas.Add(new CompraPesajeSeleccionLineaVm
                {
                    Producto = "Media Res",
                    CantidadTexto = "1",
                    KilosTexto = kg.ToString("N3")
                });
            }

            return lineas;
        }

        private static float ParseFloatFlexibleLocal(string text)
        {
            float value;
            return float.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value)
                || float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                ? value
                : 0f;
        }

        private List<ProductoNoCargadoCierreVm> ObtenerProductosNoCargadosCierre(int idSucursal, DateTime fechaCompra, int idCompra, IEnumerable<long> codigosCargados)
        {
            var productos = new List<ProductoNoCargadoCierreVm>();
            DataTable dtCortes = _oCorteN.obtenerCortes() ?? new DataTable();
            if (dtCortes.Rows.Count == 0)
                return productos;

            var codigosActuales = new HashSet<long>((codigosCargados ?? Enumerable.Empty<long>()).Where(x => x > 0));

            DateTime desde = DateTime.Today.Date.AddYears(-10);
            DataTable dtInicioStock = _oCompraN.obtenerCompras(
                idSucursal,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock),
                "",
                desde,
                fechaCompra,
                null) ?? new DataTable();

            int rowIndex = idCompra > 0 ? 1 : 0;
            if (dtInicioStock.Rows.Count > rowIndex)
                desde = Convert.ToDateTime(dtInicioStock.Rows[rowIndex]["fechaCompra"]);

            // CierreStock (a_CierreStock) es el SP de WinForms -- este controller es Web, corresponde
            // CierreStockWeb (a_CierreStockWeb), que ademas ya esta migrado a Postgres (CortePg).
            // Bug preexistente encontrado en el original, ver docs/DECISIONS.md: usaba el SP
            // equivocado, cuya columna DIF da siempre .00 para este caso de uso.
            DataTable dtStockActual = _oCorteN.CierreStockWeb("", idSucursal, desde, fechaCompra, "", 0, 0) ?? new DataTable();
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

            return Convert.ToString(value) ?? "";
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
    }
}
