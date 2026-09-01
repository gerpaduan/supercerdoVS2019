// Port PARCIAL de Web/Controllers/ProductosController.cs (ver docs/DECISIONS.md, migracion
// ASP.NET Core, Modulo 3 -- Productos). El original tiene 2515 lineas y 24 acciones (Index,
// alta/edicion con "carga continua", Marcas, Tipos, Catalogo Global, PDF de etiquetas). Portado
// en varios turnos, confirmado con el usuario dado el tamano real del modulo (ver
// docs/10-migracion-aspnet-core/README.md): turno 1 = Index() (listado); turno 2 = AddOrEdit()/
// Guardar() (alta/edicion, con atajos de teclado y flujo de codigo de barras -- la pieza que mas
// le importaba al pedido original) + las 4 acciones AJAX que esa vista consume directamente
// (FindCorteByCodigo, BuscarProductoGlobalParaAlta, BuscarMarca, ListarProductos); turno 3 =
// Marcas/Tipos (CRUD completo) + Eliminar (borrar producto, boton ya presente en Index.cshtml).
// NO portado todavia: el modal "Ver catalogo global" completo (VerGlobales/BuscarGlobales/
// ImportarSeleccionados + equivalentes de Tipos), GenerarEtiquetasPdf (bloqueado por iTextSharp,
// blocker ya conocido del plan), EditPrecioCorte/GuardarPuntosStockSucursal/findCorteById
// (botones que ya existen en Index.cshtml portado pero cuyo backing action todavia no existe).
//
// Mismo criterio que Personas: IEmpresaContext + IParametrosContext reales (Negocio.Parametros +
// Reload(), evita el NullReferenceException ya encontrado en Modulo 2) en vez de Session["Usuario"]/
// Session["PARAM_CTX"]. Los flags de ViewBag/gates que en el original vienen de
// PermisosHelper.TienePermiso(Session, ...) se hardcodean a true (mismo criterio que el usuario
// admin=true de Personas) -- documentado, no un permiso real todavia.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data.SqlClient;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class ProductosController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private const int CatalogoGlobalTamanoPagina = 50;

        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.CortePuntoStockSucursal _oCortePuntoStockSucursalN;

        public ProductosController(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;

            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oCorteN = new Negocio.Corte(_empresa, _param);
            _oPersonaN = new Negocio.Persona(_empresa, _param);
            _oCortePuntoStockSucursalN = new Negocio.CortePuntoStockSucursal(_empresa, _param);
        }

        private Negocio.CatalogoGlobalProducto ObtenerGestorCatalogoGlobal()
        {
            return new Negocio.CatalogoGlobalProducto(new EmpresaContextNulo(), null);
        }

        public IActionResult Index(
            int SucursalId = 0,
            string tipo = "",
            int marcaId = 0,
            int proveedorId = 0,
            long? codigoDesde = null,
            long? codigoHasta = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
            var productos = (_oCorteN.ObtenerCortesListado(idEmpresaSesion, SucursalId) ?? new List<Entidades.Corte>())
                .Where(x => x != null && x.Codigo >= 0)
                .ToList();

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                string tipoFiltro = tipo.Trim();
                productos = productos
                    .Where(x => string.Equals(x.Tipo ?? "", tipoFiltro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (marcaId > 0)
            {
                productos = productos
                    .Where(x => x.Marca != null && x.Marca.IdPersona == marcaId)
                    .ToList();
            }

            if (codigoDesde.HasValue)
            {
                productos = productos
                    .Where(x => x.Codigo >= codigoDesde.Value)
                    .ToList();
            }

            if (codigoHasta.HasValue)
            {
                productos = productos
                    .Where(x => x.Codigo <= codigoHasta.Value)
                    .ToList();
            }

            if (proveedorId > 0)
            {
                var idsCortesProveedor = ObtenerIdsCortesPorProveedor(proveedorId);
                productos = productos
                    .Where(x => idsCortesProveedor.Contains(x.IdCorte))
                    .ToList();
            }

            if (fechaDesde.HasValue)
            {
                DateTime desde = fechaDesde.Value;
                productos = productos
                    .Where(x => ObtenerFechaFiltroProducto(x) >= desde)
                    .ToList();
            }

            if (fechaHasta.HasValue)
            {
                DateTime hasta = fechaHasta.Value;
                if (hasta.TimeOfDay == TimeSpan.Zero)
                    hasta = hasta.AddDays(1).AddSeconds(-1);
                productos = productos
                    .Where(x => ObtenerFechaFiltroProducto(x) <= hasta)
                    .ToList();
            }

            var sucursales = _oSucursalN.findAll();

            ViewBag.Sucursales = sucursales;
            ViewBag.SucursalId = SucursalId;
            ViewBag.Tipos = ObtenerListaTipos();
            ViewBag.Marcas = ObtenerListaMarcas();
            ViewBag.Proveedores = ObtenerListaProveedores();
            ViewBag.PuedeEditarProducto = true;
            ViewBag.PuedeModificarPreciosProducto = true;
            ViewBag.PuedeEliminarProducto = true;

            return View(productos);
        }

        private static DateTime ObtenerFechaFiltroProducto(Entidades.Corte producto)
        {
            if (producto == null)
                return DateTime.MinValue;

            return producto.Actualizado ?? producto.Creado;
        }

        private IEnumerable<SelectListItem> ObtenerListaMarcas()
        {
            DataTable dt = _oPersonaN.buscarPersona("", true);
            if (dt == null || dt.Rows.Count == 0)
                return new List<SelectListItem>();

            return dt.AsEnumerable()
                .Where(row => row["idPersona"] != DBNull.Value)
                .Select(row => new SelectListItem
                {
                    Value = row["idPersona"].ToString(),
                    Text = row["Marca"].ToString()
                })
                .OrderBy(x => x.Text)
                .ToList();
        }

        private IEnumerable<SelectListItem> ObtenerListaTipos()
        {
            DataTable dt = _oCorteN.obtenerTiposProducto(false);
            var lista = dt.AsEnumerable()
               .Select(row => new SelectListItem
               {
                   Value = row["tipo"].ToString(),
                   Text = row["tipo"].ToString()
               }).ToList();

            return lista;
        }

        private IEnumerable<SelectListItem> ObtenerListaProveedores()
        {
            DataTable dt = _oPersonaN.obtenerProveedoresConCompras();
            if (dt == null || dt.Rows.Count == 0)
                return new List<SelectListItem>();

            return dt.AsEnumerable()
                .Where(row => row["idPersona"] != DBNull.Value)
                .Select(row => new SelectListItem
                {
                    Value = row["idPersona"].ToString(),
                    Text = row["razonSocial"].ToString()
                })
                .OrderBy(x => x.Text)
                .ToList();
        }

        private HashSet<int> ObtenerIdsCortesPorProveedor(int proveedorId)
        {
            if (proveedorId <= 0)
                return new HashSet<int>();

            DataTable dtCortes = _oCorteN.obtenerCortesPorProveedor(proveedorId);
            return dtCortes
                .AsEnumerable()
                .Where(row => row["idCorte"] != DBNull.Value)
                .Select(row => Convert.ToInt32(row["idCorte"]))
                .ToHashSet();
        }

        public IActionResult Crear()
        {
            return AddOrEdit(id: 0);
        }

        public IActionResult Edit(int id)
        {
            return AddOrEdit(id: id);
        }

        private static string ToStr(object o)
            => o == null || o == DBNull.Value ? "" : o.ToString();

        private static int ToInt(object o)
        {
            int v;
            int.TryParse(ToStr(o), out v);
            return v;
        }

        private static float ParseAlicuotaDesdeTextoIva(string ivaTexto)
        {
            if (string.IsNullOrWhiteSpace(ivaTexto)) return 0;

            var m = Regex.Match(ivaTexto, @"(\d+(?:[.,]\d+)?)");
            if (!m.Success) return 0;

            var s = m.Groups[1].Value.Replace(",", ".");
            float v;
            float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
            return v;
        }

        private void LoadCombos(CorteUpsertVM vm)
        {
            DataTable dtTipos = _oCorteN.obtenerTiposProducto(false);
            var tiposList = new List<SelectListItem>();

            if (dtTipos != null)
            {
                foreach (DataRow r in dtTipos.Rows)
                {
                    string tipo = ToStr(r["tipo"]);

                    tiposList.Add(new SelectListItem
                    {
                        Value = tipo,
                        Text = tipo,
                        Selected = string.Equals(tipo, vm.Tipo ?? "", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            vm.Tipos = tiposList;

            DataTable dtIva = _oCorteN.obtenerAlicuotasIva(false);
            var ivaList = new List<SelectListItem>();

            if (dtIva != null)
            {
                foreach (DataRow r in dtIva.Rows)
                {
                    int idIva = ToInt(r["idIva"]);
                    string desc = ToStr(r["iva"]);

                    ivaList.Add(new SelectListItem
                    {
                        Value = idIva.ToString(),
                        Text = desc,
                        Selected = (idIva == vm.IdAlicuotaIva)
                    });
                }
            }

            vm.AlicuotasIva = ivaList;
        }

        private float ObtenerAlicuotaPorcentajeDesdeDT(int idIvaBuscado)
        {
            DataTable dt = _oCorteN.obtenerAlicuotasIva(false);
            if (dt == null) return 0;

            foreach (DataRow r in dt.Rows)
            {
                int id = ToInt(r["idIva"]);
                if (id == idIvaBuscado)
                {
                    string ivaTexto = ToStr(r["iva"]);
                    return ParseAlicuotaDesdeTextoIva(ivaTexto);
                }
            }

            return 0;
        }

        public IActionResult AddOrEdit(int id = 0, bool cargaContinua = false, bool productoGuardado = false, int? ultimoProductoContinuoId = null, int? retomarProductoId = null, string flujoBaseContinuo = null)
        {
            Entidades.Corte entity = (id == 0)
                ? new Entidades.Corte()
                : _oCorteN.findCorteById(id, true);

            if (id > 0 && entity == null) return NotFound();

            var vm = BuildVM(entity);
            vm.CargaContinua = cargaContinua;
            vm.UltimoProductoContinuoId = ultimoProductoContinuoId;
            vm.RetomarProductoId = retomarProductoId;
            vm.FlujoBaseContinuo = !string.IsNullOrWhiteSpace(flujoBaseContinuo)
                ? flujoBaseContinuo
                : (id > 0 ? "edicion" : "alta");

            LoadCombos(vm);
            ViewBag.ProductoGuardadoContinuo = productoGuardado;
            ViewBag.FlashSuccessContinuo = TempData["FlashSuccessContinuo"] as string;

            return View("AddOrEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(CorteUpsertVM vm)
        {
            NormalizarFloatsDesdeRequest(vm);

            bool esEdicionFormulario = string.Equals(Request.Form["EsEdicionFormulario"], "1", StringComparison.Ordinal);
            if (!esEdicionFormulario)
            {
                vm.IdCorte = 0;
            }

            if (vm.Codigo > 0)
            {
                int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
                var existente = idEmpresaSesion > 0
                    ? _oCorteN.findCorteByCodigoEmpresa(vm.Codigo, idEmpresaSesion, false)
                    : _oCorteN.findCorteByCodigo(vm.Codigo, false);

                if (existente != null && existente.IdCorte != vm.IdCorte)
                {
                    ModelState.AddModelError("Codigo", $"El código ya existe para el producto: {existente.CorteDesc}");
                }
            }

            ValidarModoCorte(vm);

            if (!ModelState.IsValid)
            {
                LoadCombos(vm);
                return View("AddOrEdit", vm);
            }

            int idEmpresaSesionActual = _empresa != null ? _empresa.IdEmpresa : 0;
            bool altaDesdeCatalogoGlobal = false;

            if (vm.IdCorte <= 0 && idEmpresaSesionActual > 0 && vm.Codigo > 0)
            {
                bool codigoExisteEnCatalogoGlobal = ObtenerGestorCatalogoGlobal().findCorteGlobalByCodigo(vm.Codigo, false) != null;
                if (codigoExisteEnCatalogoGlobal)
                {
                    altaDesdeCatalogoGlobal = true;
                }
            }

            var entity = (vm.IdCorte > 0)
                ? _oCorteN.findCorteById(vm.IdCorte, true)
                : new Entidades.Corte();

            if (vm.IdCorte > 0 && entity == null)
                return NotFound();

            bool enCierreStockAntesDeEditar = vm.IdCorte > 0 && entity != null && entity.EnCierreStock;

            if (vm.IdCorte > 0 && entity != null)
            {
                if (entity.IdEmpresa == 0)
                {
                    var productoGlobalBase = entity;
                    altaDesdeCatalogoGlobal = true;
                    vm.IdCorte = 0;
                    entity = ClonarProductoGlobal(productoGlobalBase, vm.Codigo, vm.PrecioKg);
                }
                else if (idEmpresaSesionActual > 0 && entity.IdEmpresa != idEmpresaSesionActual)
                {
                    TempData["FlashError"] = "Solo puede modificar productos de la empresa actual.";
                    return RedirectToAction("Index");
                }
            }

            vm.AlicuotaIva = ObtenerAlicuotaPorcentajeDesdeDT(vm.IdAlicuotaIva);

            MapToEntity(vm, entity);

            entity.IdEmpresa = idEmpresaSesionActual;

            bool esAltaNueva = vm.IdCorte <= 0;

            if (vm.IdCorte <= 0)
            {
                entity.IdCorte = 0;
            }

            if (altaDesdeCatalogoGlobal)
            {
                entity.IdCorte = _oCorteN.InsertarCorteEnEmpresa(entity);
            }
            else
            {
                _oCorteN.addOrEditCorte(entity);
            }

            int idProductoGuardado = entity.IdCorte;
            if (idProductoGuardado <= 0 && vm.Codigo > 0)
            {
                int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
                var productoGuardado = idEmpresaSesion > 0
                    ? _oCorteN.findCorteByCodigoEmpresa(vm.Codigo, idEmpresaSesion, false)
                    : _oCorteN.findCorteByCodigo(vm.Codigo, false);

                if (productoGuardado != null)
                {
                    idProductoGuardado = productoGuardado.IdCorte;
                }
            }

            if (esAltaNueva && idProductoGuardado > 0)
            {
                _oCortePuntoStockSucursalN.CrearParaTodasLasSucursales(idEmpresaSesionActual, idProductoGuardado, vm.PuntoStock);
            }
            else if (!esAltaNueva && idProductoGuardado > 0 && !enCierreStockAntesDeEditar && entity.EnCierreStock)
            {
                _oCortePuntoStockSucursalN.CrearParaTodasLasSucursales(idEmpresaSesionActual, idProductoGuardado, entity.PuntoStock);
            }

            string flujoBase = string.Equals(vm.FlujoBaseContinuo, "edicion", StringComparison.OrdinalIgnoreCase)
                ? "edicion"
                : "alta";

            if (vm.IdCorte <= 0 && vm.CargaContinua)
            {
                TempData["FlashSuccessContinuo"] = $"Se cargó exitosamente el producto \"{vm.CorteDesc}\".";
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Productos";
                TempData["AlertMsg"] = TempData["FlashSuccessContinuo"];
                return RedirectToAction("AddOrEdit", new
                {
                    cargaContinua = true,
                    productoGuardado = true,
                    ultimoProductoContinuoId = idProductoGuardado,
                    flujoBaseContinuo = "alta"
                });
            }

            if (vm.IdCorte > 0 && vm.CargaContinua)
            {
                TempData["FlashSuccessContinuo"] = $"Se guardó el producto \"{vm.CorteDesc}\".";
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Productos";
                TempData["AlertMsg"] = TempData["FlashSuccessContinuo"];

                if (flujoBase == "alta")
                {
                    return RedirectToAction("AddOrEdit", new
                    {
                        cargaContinua = true,
                        productoGuardado = true,
                        ultimoProductoContinuoId = idProductoGuardado,
                        flujoBaseContinuo = "alta"
                    });
                }

                int idRetorno = vm.RetomarProductoId.GetValueOrDefault() > 0
                    ? vm.RetomarProductoId.Value
                    : vm.SiguienteIdEdicion.GetValueOrDefault();

                if (idRetorno > 0)
                {
                    return RedirectToAction("AddOrEdit", new
                    {
                        id = idRetorno,
                        cargaContinua = true,
                        productoGuardado = true,
                        ultimoProductoContinuoId = idProductoGuardado,
                        flujoBaseContinuo = "edicion"
                    });
                }

                TempData["FlashSuccess"] = $"El producto \"{vm.CorteDesc}\" guardó correctamente.";
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Productos";
                TempData["AlertMsg"] = TempData["FlashSuccess"];
                return RedirectToAction("Index");
            }

            TempData["FlashSuccess"] = $"El producto \"{vm.CorteDesc}\" guardó correctamente.";
            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Productos";
            TempData["AlertMsg"] = TempData["FlashSuccess"];
            return RedirectToAction("Index");
        }

        private void ValidarModoCorte(CorteUpsertVM vm)
        {
            vm.ModoCorte = (vm.ModoCorte ?? "Ninguno").Trim();

            if (vm.ModoCorte != "Ninguno" && vm.ModoCorte != "CorteMaestro" && vm.ModoCorte != "Presentacion")
            {
                ModelState.AddModelError(nameof(vm.ModoCorte), "Modo inválido.");
                return;
            }

            if (vm.ModoCorte == "Ninguno")
                return;

            if (!vm.IdCorteMaestro.HasValue || vm.IdCorteMaestro.Value <= 0)
                ModelState.AddModelError("", "Seleccioná un corte maestro.");

            if (vm.ModoCorte == "CorteMaestro")
            {
                if (vm.Porcentaje <= 0) ModelState.AddModelError("", "El porcentaje debe ser mayor a 0.");
                if (vm.PorcentajeHueso < 0) ModelState.AddModelError("", "El desperdicio no puede ser negativo.");
            }

            if (vm.ModoCorte == "Presentacion")
            {
                if (vm.Porcentaje <= 0) ModelState.AddModelError("", "La presentación (unidades) debe ser un número mayor o igual a 1.");
            }
        }

        private void NormalizarFloatsDesdeRequest(CorteUpsertVM vm)
        {
            TrySetFloatFromRequest("PrecioKg", v => vm.PrecioKg = v, "Precio Kg");
            TrySetFloatFromRequest("Promedio", v => vm.Promedio = v, "Promedio");
            TrySetFloatFromRequest("Porcentaje", v => vm.Porcentaje = v, "Porcentaje");
            TrySetFloatFromRequest("PorcentajeHueso", v => vm.PorcentajeHueso = v, "Desperdicio");
        }

        private void TrySetFloatFromRequest(string key, Action<float> setter, string label)
        {
            var raw = (Request.Form[key].ToString() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(raw))
            {
                ModelState.Remove(key);
                return;
            }

            var normalized = raw.Replace(",", ".");

            float valor;
            bool ok = float.TryParse(
                normalized,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out valor);

            if (!ok)
            {
                ModelState.AddModelError(key, $"{label} tiene un formato numérico inválido.");
                return;
            }

            setter(valor);
            ModelState.Remove(key);
        }

        private CorteUpsertVM BuildVM(Entidades.Corte e)
        {
            var vm = new CorteUpsertVM();

            vm.IdCorte = e.IdCorte;
            vm.Codigo = e.Codigo;
            vm.CorteDesc = e.CorteDesc;
            vm.PrecioKg = e.PrecioKg;
            vm.Tipo = e.Tipo;

            vm.Pesable = e.Pesable;
            vm.Promedio = e.Promedio;

            vm.IdAlicuotaIva = e.IdAlicuotaIva;
            vm.AlicuotaIva = e.AlicuotaIva;

            vm.PuntoStock = e.PuntoStock;
            vm.EnCierreStock = e.EnCierreStock;
            vm.Habilitado = e.Habilitado;
            vm.IngresoRapidoEmbutido = e.IngresoRapidoEmbutido;

            vm.Nivel = e.Nivel;

            vm.MarcaNombre = e.MarcaNombre;
            vm.IdMarca = (e.Marca != null) ? (int?)GetIdPersonaReflection(e.Marca) : null;

            vm.Independiente = (e.Independiente == 1);

            if (e.CorteMaestro != null && e.CorteMaestro.IdCorte > 0)
            {
                vm.IdCorteMaestro = e.CorteMaestro.IdCorte;
                vm.CorteMaestroNombre = e.CorteMaestro.CorteDesc;

                vm.Porcentaje = e.Porcentaje;
                vm.PorcentajeHueso = e.PorcentajeHueso;

                if (e.Presentacion)
                {
                    vm.ModoCorte = "Presentacion";
                    vm.PresentacionUnidades = e.getCantPresentacion(e.PorcentajeHueso);
                }
                else
                {
                    vm.ModoCorte = "CorteMaestro";
                }
            }
            else
            {
                vm.ModoCorte = "Ninguno";
                vm.IdCorteMaestro = null;
                vm.CorteMaestroNombre = "";
                vm.Porcentaje = 0;
                vm.PorcentajeHueso = 0;
                vm.PresentacionUnidades = null;
            }

            if (vm.IdCorte == 0)
                vm.Independiente = true;

            return vm;
        }

        private void MapToEntity(CorteUpsertVM vm, Entidades.Corte e)
        {
            e.Codigo = vm.Codigo;
            e.CorteDesc = vm.CorteDesc;
            e.PrecioKg = vm.PrecioKg;
            e.Tipo = vm.Tipo;

            e.Pesable = vm.Pesable;
            e.Promedio = vm.Promedio;

            e.IdAlicuotaIva = vm.IdAlicuotaIva;
            e.AlicuotaIva = vm.AlicuotaIva;

            e.PuntoStock = vm.PuntoStock;
            e.EnCierreStock = vm.EnCierreStock;
            e.Habilitado = vm.Habilitado;
            e.IngresoRapidoEmbutido = vm.IngresoRapidoEmbutido;

            e.Independiente = vm.Independiente ? 1 : 0;

            if (vm.IdMarca.HasValue && vm.IdMarca.Value > 0)
            {
                var p = new Entidades.Persona();
                SetIdPersonaReflection(p, vm.IdMarca.Value);
                e.Marca = p;
            }
            else
            {
                e.Marca = null;
            }

            vm.ModoCorte = (vm.ModoCorte ?? "Ninguno").Trim();

            if (vm.ModoCorte == "Ninguno")
            {
                e.CorteMaestro = null;
                e.Presentacion = false;
                e.Porcentaje = 0;
                e.PorcentajeHueso = 0;
            }
            else
            {
                if (!vm.IdCorteMaestro.HasValue || vm.IdCorteMaestro.Value <= 0)
                    throw new Exception("Falta seleccionar corte maestro.");

                var cm = new Entidades.Corte();
                cm.IdCorte = vm.IdCorteMaestro.Value;
                e.CorteMaestro = cm;

                if (vm.ModoCorte == "CorteMaestro")
                {
                    e.Presentacion = false;
                    e.Porcentaje = vm.Porcentaje;
                    e.PorcentajeHueso = vm.PorcentajeHueso;
                }
                else
                {
                    e.Presentacion = true;
                    e.Porcentaje = vm.PresentacionUnidades ?? 0;
                    e.PorcentajeHueso = 0;
                }
            }

            if (vm.IdCorte == 0 && e.Creado == default(DateTime))
                e.Creado = DateTime.Now;

            e.Actualizado = DateTime.Now;
        }

        private int GetIdPersonaReflection(object persona)
        {
            if (persona == null) return 0;
            var t = persona.GetType();

            var p1 = t.GetProperty("IdPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p1 != null) return Convert.ToInt32(p1.GetValue(persona, null) ?? 0);

            var p2 = t.GetProperty("idPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p2 != null) return Convert.ToInt32(p2.GetValue(persona, null) ?? 0);

            var f1 = t.GetField("IdPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f1 != null) return Convert.ToInt32(f1.GetValue(persona) ?? 0);

            var f2 = t.GetField("idPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f2 != null) return Convert.ToInt32(f2.GetValue(persona) ?? 0);

            return 0;
        }

        private void SetIdPersonaReflection(object persona, int id)
        {
            if (persona == null) return;
            var t = persona.GetType();

            var p1 = t.GetProperty("IdPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p1 != null && p1.CanWrite) { p1.SetValue(persona, id, null); return; }

            var p2 = t.GetProperty("idPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p2 != null && p2.CanWrite) { p2.SetValue(persona, id, null); return; }

            var f1 = t.GetField("IdPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f1 != null) { f1.SetValue(persona, id); return; }

            var f2 = t.GetField("idPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f2 != null) { f2.SetValue(persona, id); return; }
        }

        private static Entidades.Corte ClonarProductoGlobal(Entidades.Corte global, long codigoDestino, float precioDestino)
        {
            var nuevo = new Entidades.Corte
            {
                IdCorte = 0,
                Codigo = codigoDestino,
                CorteDesc = global.CorteDesc,
                Tipo = global.Tipo,
                Pesable = global.Pesable,
                Promedio = global.Promedio,
                IdAlicuotaIva = global.IdAlicuotaIva,
                AlicuotaIva = global.AlicuotaIva,
                PuntoStock = global.PuntoStock,
                EnCierreStock = global.EnCierreStock,
                Habilitado = global.Habilitado,
                IngresoRapidoEmbutido = global.IngresoRapidoEmbutido,
                Independiente = global.Independiente,
                Porcentaje = global.Porcentaje,
                PorcentajeHueso = global.PorcentajeHueso,
                DesvioEstandar = global.DesvioEstandar,
                PrecioKg = precioDestino,
                PrecioKgReferencia = precioDestino,
                Presentacion = global.Presentacion,
                Nivel = global != null ? global.Nivel : 0
            };

            if (global.CorteMaestro != null && global.CorteMaestro.IdCorte > 0)
            {
                nuevo.CorteMaestro = new Entidades.Corte
                {
                    IdCorte = global.CorteMaestro.IdCorte,
                    CorteDesc = global.CorteMaestro.CorteDesc
                };
            }

            return nuevo;
        }

        [HttpGet]
        public JsonResult FindCorteByCodigo(long? codigo, int? idExcluir = null)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
            {
                return Json(new { existe = false });
            }

            int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
            var corte = idEmpresaSesion > 0
                ? _oCorteN.findCorteByCodigoEmpresa(codigo.Value, idEmpresaSesion, false)
                : _oCorteN.findCorteByCodigo(codigo.Value, false);

            if (corte == null)
            {
                return Json(new { existe = false });
            }

            int idExc = idExcluir.GetValueOrDefault();
            if (idExc > 0 && corte.IdCorte == idExc)
            {
                return Json(new
                {
                    existe = false,
                    mismoRegistro = true,
                    id = corte.IdCorte,
                    nombre = corte.CorteDesc
                });
            }

            return Json(new
            {
                existe = true,
                id = corte.IdCorte,
                nombre = corte.CorteDesc,
                descripcion = corte.CorteDesc,
                precio = corte.PrecioKg
            });
        }

        private static bool EsCodigoEanValido(string codigoBarra)
        {
            var limpio = Regex.Replace(codigoBarra ?? "", @"[^\d]", "");
            if (limpio.Length == 8) return EsCodigoEan8Valido(limpio);
            if (limpio.Length == 13) return EsCodigoEan13Valido(limpio);
            return false;
        }

        private static bool EsCodigoEan13Valido(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo) || !Regex.IsMatch(codigo, @"^\d{13}$"))
                return false;

            int suma = 0;
            for (int i = 0; i < 12; i++)
            {
                int digito = codigo[i] - '0';
                suma += (i % 2 == 0) ? digito : digito * 3;
            }

            int control = (10 - (suma % 10)) % 10;
            return control == (codigo[12] - '0');
        }

        private static bool EsCodigoEan8Valido(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo) || !Regex.IsMatch(codigo, @"^\d{8}$"))
                return false;

            int suma = 0;
            for (int i = 0; i < 7; i++)
            {
                int digito = codigo[i] - '0';
                suma += (i % 2 == 0) ? digito * 3 : digito;
            }

            int control = (10 - (suma % 10)) % 10;
            return control == (codigo[7] - '0');
        }

        private static long NormalizarCodigoBarra(string codigoBarra)
        {
            var limpio = Regex.Replace(codigoBarra ?? "", @"[^\d]", "");
            long codigo;
            return long.TryParse(limpio, out codigo) ? codigo : 0L;
        }

        [HttpGet]
        public JsonResult BuscarProductoGlobalParaAlta(string codigoBarra)
        {
            if (!EsCodigoEanValido(codigoBarra))
                return Json(new { ok = false, mensaje = "Solo se autocompleta con EAN-8 o EAN-13 válidos." });

            long codigo = NormalizarCodigoBarra(codigoBarra);
            if (codigo <= 0)
                return Json(new { ok = false, mensaje = "Ingrese un código de barra válido." });

            var global = ObtenerGestorCatalogoGlobal().findCorteGlobalByCodigo(codigo, true);
            if (global == null)
                return Json(new { ok = false, mensaje = "No existe un producto global para ese código." });

            string modoCorte = "Ninguno";
            if (global.Presentacion)
                modoCorte = "Presentacion";
            else if (global.CorteMaestro != null && global.CorteMaestro.IdCorte > 0)
                modoCorte = "CorteMaestro";

            return Json(new
            {
                ok = true,
                producto = new
                {
                    idCorte = 0,
                    idEmpresa = (int?)null,
                    codigo = global.Codigo,
                    descripcion = global.CorteDesc,
                    precioKg = global.PrecioKg,
                    tipo = global.Tipo,
                    idAlicuotaIva = global.IdAlicuotaIva,
                    idMarca = global.Marca != null ? global.Marca.IdPersona : 0,
                    marcaNombre = global.MarcaNombre ?? (global.Marca != null ? global.Marca.RazonSocial : ""),
                    pesable = global.Pesable,
                    habilitado = global.Habilitado,
                    ingresoRapidoEmbutido = global.IngresoRapidoEmbutido,
                    enCierreStock = global.EnCierreStock,
                    puntoStock = global.PuntoStock,
                    promedio = global.Promedio,
                    modoCorte = modoCorte,
                    idCorteMaestro = global.CorteMaestro != null ? global.CorteMaestro.IdCorte : 0,
                    corteMaestroNombre = global.CorteMaestro != null ? global.CorteMaestro.CorteDesc : "",
                    porcentaje = global.Porcentaje,
                    porcentajeHueso = global.PorcentajeHueso,
                    independiente = global.Independiente == 1,
                    presentacionUnidades = global.Presentacion ? global.getCantPresentacion(global.PorcentajeHueso) : (float?)null
                }
            });
        }

        [HttpGet]
        public JsonResult ListarProductos(string q = "")
        {
            try
            {
                int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
                var productos = idEmpresaSesion > 0
                    ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>())
                    : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>());

                productos = productos.Where(p => p != null && p.codigo >= 0).ToList();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.Trim();
                    productos = productos
                        .Where(p =>
                            (!string.IsNullOrEmpty(p.corte) && p.corte.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            p.codigo.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                var resultado = productos
                    .Select(p => new
                    {
                        id = p.IdCorte,
                        codigo = p.codigo.ToString(),
                        nombre = p.corte,
                        precio = p.precioKg
                    })
                    .Take(200)
                    .ToList();

                return Json(resultado);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public JsonResult BuscarMarca(string q = "")
        {
            q = (q ?? "").Trim();
            var dt = _oPersonaN.buscarPersona(q, true);

            var list = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    id = Convert.ToInt32(row["idPersona"]),
                    nombre = Convert.ToString(row["Marca"])
                });
            }

            return Json(list);
        }

        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: false);
                if (viewResult.View == null)
                    throw new InvalidOperationException("No se encontró la vista parcial '" + viewName + "'.");

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    new TempDataDictionary(HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        public IActionResult Tipos(string buscar = "")
        {
            ViewBag.BuscarTipoProducto = (buscar ?? "").Trim();
            ViewBag.PuedeEditarTiposProducto = true;

            DataTable dt = _oCorteN.obtenerTiposProductoGrillaEmpresa((buscar ?? "").Trim()) ?? new DataTable();
            return View(dt);
        }

        [HttpGet]
        public async Task<IActionResult> TipoProductoModal(string tipo = "")
        {
            try
            {
                bool esEdicion = !string.IsNullOrWhiteSpace(tipo);
                var model = new TipoProductoEditVm
                {
                    TipoOriginal = "",
                    Tipo = "",
                    Orden = 100,
                    Reservado = false
                };

                if (esEdicion)
                {
                    var row = BuscarTipoProductoRow(tipo);
                    if (row == null)
                        return Content("<div class='alert alert-danger mb-0'>No se encontró el tipo de producto seleccionado.</div>", "text/html");

                    model.TipoOriginal = Convert.ToString(row["tipo"]);
                    model.Tipo = Convert.ToString(row["tipo"]);
                    model.Orden = row["orden"] != DBNull.Value ? Convert.ToInt32(row["orden"]) : 100;
                    model.Reservado = row.Table.Columns.Contains("Reservado")
                        && row["Reservado"] != DBNull.Value
                        && Convert.ToBoolean(row["Reservado"]);
                }

                string html = await RenderPartialViewToStringAsync("_AddOrEditTipoProducto", model);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                string detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("<div class='alert alert-danger mb-0'>No se pudo abrir el formulario: " + System.Net.WebUtility.HtmlEncode(detalle) + "</div>", "text/html");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarTipoProducto(TipoProductoEditVm model)
        {
            string tipo = (model != null ? model.Tipo : null) ?? "";
            string tipoOriginal = (model != null ? model.TipoOriginal : null) ?? "";
            tipo = tipo.Trim();
            tipoOriginal = tipoOriginal.Trim();
            bool esInsert = string.IsNullOrWhiteSpace(tipoOriginal);

            if (string.IsNullOrWhiteSpace(tipo))
                return Json(new { success = false, message = "El campo Tipo no puede ser vacío." });

            if (model == null || model.Orden <= 0)
                return Json(new { success = false, message = "El campo Orden debe ser un número entero mayor a cero." });

            if (!esInsert)
            {
                var row = BuscarTipoProductoRow(tipoOriginal);
                if (row == null)
                    return Json(new { success = false, message = "No se encontró el tipo de producto seleccionado." });

                bool reservado = row.Table.Columns.Contains("Reservado")
                    && row["Reservado"] != DBNull.Value
                    && Convert.ToBoolean(row["Reservado"]);

                if (reservado)
                    return Json(new { success = false, message = "El tipo seleccionado es reservado por el sistema y no puede ser modificado." });
            }

            string mensaje = _oCorteN.addOrEditTipoProducto(tipo, model.Orden.ToString(CultureInfo.InvariantCulture), esInsert, tipoOriginal);
            if (!string.IsNullOrWhiteSpace(mensaje))
                return Json(new { success = false, message = mensaje });

            return Json(new
            {
                success = true,
                message = esInsert ? "El tipo de producto se registró correctamente." : "El tipo de producto se actualizó correctamente."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarTipoProducto(string tipo)
        {
            tipo = (tipo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tipo))
                return Json(new { success = false, message = "No se encontró el tipo de producto seleccionado." });

            var row = BuscarTipoProductoRow(tipo);
            if (row == null)
                return Json(new { success = false, message = "No se encontró el tipo de producto seleccionado." });

            bool reservado = row.Table.Columns.Contains("Reservado")
                && row["Reservado"] != DBNull.Value
                && Convert.ToBoolean(row["Reservado"]);

            if (reservado)
                return Json(new { success = false, message = "El tipo seleccionado es reservado por el sistema y no puede eliminarse." });

            string mensaje = _oCorteN.eliminarTipoProducto(tipo);
            if (!string.IsNullOrWhiteSpace(mensaje))
                return Json(new { success = false, message = mensaje });

            return Json(new { success = true, message = "El tipo de producto se eliminó correctamente." });
        }

        private DataRow BuscarTipoProductoRow(string tipo)
        {
            tipo = (tipo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tipo))
                return null;

            DataTable dt = _oCorteN.obtenerTiposProductoGrillaEmpresa("") ?? new DataTable();
            return dt.AsEnumerable().FirstOrDefault(row =>
                string.Equals(Convert.ToString(row["tipo"]) ?? "", tipo, StringComparison.OrdinalIgnoreCase));
        }

        public IActionResult Marcas(string buscar = "")
        {
            ViewBag.BuscarMarcaAdmin = (buscar ?? "").Trim();
            ViewBag.PuedeCrearMarca = true;
            ViewBag.UsuarioAdmin = true;

            DataTable dt = _oPersonaN.buscarPersona((buscar ?? "").Trim(), true) ?? new DataTable();
            return View(dt);
        }

        [HttpGet]
        public async Task<IActionResult> MarcaModal(int idPersona = 0)
        {
            try
            {
                bool esAdministrador = true;

                var model = new MarcaEditVm
                {
                    IdPersona = 0,
                    RazonSocial = "",
                    OtrosDatos = "",
                    IdPropietario = null,
                    PropietarioNombre = "",
                    EsAdministrador = esAdministrador,
                    SoloLecturaNombre = false
                };

                if (idPersona > 0)
                {
                    var marca = _oPersonaN.findById(idPersona);
                    if (marca == null || marca.IdPersona <= 0 || !marca.Marca)
                        return Content("<div class='alert alert-danger mb-0'>No se encontró la marca seleccionada.</div>", "text/html");

                    model.IdPersona = marca.IdPersona;
                    model.RazonSocial = marca.RazonSocial ?? "";
                    model.OtrosDatos = marca.OtrosDatos ?? "";
                    model.IdPropietario = marca.Propietario != null && marca.Propietario.IdPersona > 0
                        ? (int?)marca.Propietario.IdPersona
                        : (marca.IdPropietario.HasValue && marca.IdPropietario.Value > 0 ? marca.IdPropietario : null);
                    model.PropietarioNombre = marca.Propietario != null ? (marca.Propietario.RazonSocial ?? "") : "";
                    model.SoloLecturaNombre = !esAdministrador;
                }

                string html = await RenderPartialViewToStringAsync("_AddOrEditMarca", model);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                string detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("<div class='alert alert-danger mb-0'>No se pudo abrir el formulario: " + System.Net.WebUtility.HtmlEncode(detalle) + "</div>", "text/html");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarMarca(MarcaEditVm model)
        {
            bool esAdministrador = true;
            bool esInsert = model == null || model.IdPersona <= 0;

            if (model == null)
                return Json(new { success = false, message = "No se pudo procesar la marca." });

            model.RazonSocial = (model.RazonSocial ?? "").Trim();
            model.OtrosDatos = (model.OtrosDatos ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.RazonSocial))
                return Json(new { success = false, message = "El campo Nombre Marca no puede estar vacío." });

            Entidades.Persona marca = esInsert ? new Entidades.Persona() : _oPersonaN.findById(model.IdPersona);
            if (!esInsert && (marca == null || marca.IdPersona <= 0 || !marca.Marca))
                return Json(new { success = false, message = "No se encontró la marca seleccionada." });

            if (!esInsert && !esAdministrador)
            {
                string nombreActual = (marca.RazonSocial ?? "").Trim();
                if (!string.Equals(nombreActual, model.RazonSocial, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Solo los administradores pueden modificar el nombre de una marca existente."
                    });
                }
            }

            string advertencia = ConstruirAdvertenciaMarcasParecidas(model.RazonSocial, model.IdPersona);
            if (!string.IsNullOrWhiteSpace(advertencia) && !model.ConfirmarMarcasParecidas)
            {
                return Json(new
                {
                    success = false,
                    requiresConfirm = true,
                    message = advertencia
                });
            }

            marca.RazonSocial = model.RazonSocial;
            marca.Identificacion = model.RazonSocial;
            marca.Marca = true;
            marca.OtrosDatos = model.OtrosDatos;
            marca.Propietario = null;
            marca.IdPropietario = null;

            if (model.IdPropietario.HasValue && model.IdPropietario.Value > 0)
            {
                var propietario = _oPersonaN.findById(model.IdPropietario.Value);
                if (propietario == null || propietario.IdPersona <= 0)
                    return Json(new { success = false, message = "No se encontró la persona seleccionada como propietaria." });

                marca.Propietario = propietario;
                marca.IdPropietario = propietario.IdPersona;
            }

            _oPersonaN.addOrEditPersona(marca);

            return Json(new
            {
                success = true,
                message = esInsert ? "La marca se guardó correctamente." : "La marca se actualizó correctamente."
            });
        }

        private string ConstruirAdvertenciaMarcasParecidas(string razonSocial, int idMarcaActual)
        {
            string texto = (razonSocial ?? "").Trim();
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            var articulos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "el", "la", "los", "las", "un", "una", "unos", "unas",
                "de", "del", "en", "y", "por", "para", "con"
            };

            DataTable acumulado = null;
            string[] palabras = Regex.Split(texto, "\\s+");

            foreach (string palabraOriginal in palabras)
            {
                string palabra = (palabraOriginal ?? "").Trim();
                if (string.IsNullOrWhiteSpace(palabra) || articulos.Contains(palabra))
                    continue;

                DataTable dtTemp = _oPersonaN.existenMarcasParecidas(palabra, idMarcaActual);
                if (dtTemp == null || dtTemp.Rows.Count == 0)
                    continue;

                if (acumulado == null)
                    acumulado = dtTemp.Clone();

                foreach (DataRow row in dtTemp.Rows)
                {
                    string marca = Convert.ToString(row["Marca"]) ?? "";
                    string propietario = row.Table.Columns.Contains("Propietario") ? (Convert.ToString(row["Propietario"]) ?? "") : "";

                    bool existe = acumulado.AsEnumerable().Any(r =>
                        string.Equals(Convert.ToString(r["Marca"]) ?? "", marca, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(Convert.ToString(r["Propietario"]) ?? "", propietario, StringComparison.OrdinalIgnoreCase));

                    if (!existe)
                        acumulado.ImportRow(row);
                }
            }

            if (acumulado == null || acumulado.Rows.Count == 0)
                return "";

            var sb = new StringBuilder();
            sb.AppendLine("Ya existen marcas parecidas:");
            sb.AppendLine();

            foreach (DataRow row in acumulado.Rows)
            {
                string marca = Convert.ToString(row["Marca"]) ?? "";
                string propietario = row.Table.Columns.Contains("Propietario") ? (Convert.ToString(row["Propietario"]) ?? "") : "";
                sb.Append("• ").Append(marca);
                if (!string.IsNullOrWhiteSpace(propietario))
                    sb.Append(" | Propietario: ").Append(propietario);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append("¿Desea guardar la marca igualmente?");
            return sb.ToString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (id <= 0)
            {
                TempData["FlashError"] = "No se pudo eliminar: producto inválido.";
                return RedirectToAction("Index");
            }

            var entity = _oCorteN.findCorteById(id, true);
            if (entity == null)
            {
                TempData["FlashError"] = "No se pudo eliminar: el producto no existe o ya fue eliminado.";
                return RedirectToAction("Index");
            }

            var nombre = entity.CorteDesc ?? entity.corte ?? "(sin nombre)";

            try
            {
                _oCorteN.eliminarCorte(entity);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                TempData["FlashError"] = "No se puede eliminar el producto porque está asociado a otros registros del sistema, por ejemplo ventas, movimientos u otras relaciones.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["FlashError"] = "No se pudo eliminar el producto por un error inesperado.";
                return RedirectToAction("Index");
            }

            TempData["FlashSuccess"] = $"El producto \"{nombre}\" se eliminó correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult findCorteById(int id)
        {
            var corte = _oCorteN.findCorteById(id, true);

            if (corte == null)
                return NotFound();

            return Json(new
            {
                id = corte.IdCorte,
                descripcion = corte.CorteDesc,
                precio = corte.PrecioKg
            });
        }

        [HttpPost]
        public IActionResult EditPrecioCorte(int IdCorte, string PrecioKg)
        {
            if (string.IsNullOrWhiteSpace(PrecioKg))
                return Json(new { error = "Precio vacío" });

            string normalizado = PrecioKg
                .Replace(".", "")
                .Replace(",", ".");

            float precioDecimal;
            if (!float.TryParse(
                    normalizado,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out precioDecimal))
            {
                return Json(new { error = "Formato de precio inválido" });
            }

            Entidades.Corte model = new Entidades.Corte();
            model.idCorte = IdCorte;
            model.precioKg = precioDecimal;
            _oCorteN.editPrecioCorte(model);

            return Json(new
            {
                id = IdCorte,
                precio = precioDecimal,
                precioFormateado = "$ " + precioDecimal.ToString("N2", new CultureInfo("es-AR"))
            });
        }

        [HttpGet]
        public async Task<IActionResult> VerGlobales()
        {
            var model = ConstruirCatalogoGlobalVm("", "", 1, true);
            string html = await RenderPartialViewToStringAsync("_CatalogoGlobalModal", model);
            return Content(html, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> BuscarGlobales(string q = "", string tipo = "", int pagina = 1)
        {
            var model = ConstruirCatalogoGlobalVm(q, tipo, pagina, false);
            string html = await RenderPartialViewToStringAsync("_CatalogoGlobalRows", model.Productos);

            return Json(new
            {
                ok = true,
                html,
                cantidad = model.Productos.Count,
                pagina = model.Pagina,
                hayMas = model.HayMas
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ImportarSeleccionados(ImportarProductosGlobalesRequest request)
        {
            // TODO(claude): el original usa Session["Usuario"] como usuario que hizo la
            // importacion (GuardarImportacionCatalogoGlobal, ultimo parametro). WebCore todavia
            // no tiene sesion real -- se pasa null (columna nullable, ver Negocio/Corte.cs).
            int? idUsuario = null;

            _oCorteN.AsegurarTablaImportacionCatalogoGlobal();

            var seleccionados = (request?.Productos ?? new List<ProductoGlobalSeleccionVm>())
                .Where(x => x != null && x.IdProductoGlobal > 0)
                .GroupBy(x => x.IdProductoGlobal)
                .Select(x => x.First())
                .ToList();

            if (!seleccionados.Any())
                return Json(new { ok = false, mensaje = "Seleccione al menos un producto del catálogo global." });

            var catalogoGlobal = ObtenerGestorCatalogoGlobal();
            var productosGlobales = catalogoGlobal.ObtenerCatalogoGlobalPorIds(seleccionados.Select(x => x.IdProductoGlobal));

            if (productosGlobales.Count != seleccionados.Count)
                return Json(new { ok = false, mensaje = "No se pudieron resolver todos los productos seleccionados del catálogo global." });

            var importacionesExistentes = _oCorteN.ObtenerImportacionesCatalogoGlobal()
                .ToDictionary(x => x.IdProductoGlobal, x => x);

            var productosEmpresaActual = ObtenerProductosEmpresaSesionActual();
            var productosEmpresaPorId = productosEmpresaActual.ToDictionary(x => x.IdCorte, x => x);
            var codigosEmpresa = new HashSet<long>(productosEmpresaActual.Select(x => x.Codigo));

            foreach (var producto in productosGlobales)
            {
                Entidades.CatalogoGlobalImportacionProducto importacion;
                if (importacionesExistentes.TryGetValue(producto.IdCorte, out importacion))
                {
                    if (productosEmpresaPorId.ContainsKey(importacion.IdProductoEmpresa))
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = "El producto \"" + producto.CorteDesc + "\" ya fue importado previamente en esta empresa."
                        });
                    }
                }
            }

            var seleccionPorId = seleccionados.ToDictionary(x => x.IdProductoGlobal, x => x);
            var importacionesMaestros = _oCorteN.ObtenerImportacionesCatalogoGlobal();

            var importacionesMaestrosPorGlobal = importacionesMaestros
                .GroupBy(x => x.IdProductoGlobal)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var producto in productosGlobales)
            {
                if (producto.CorteMaestro == null || producto.CorteMaestro.IdCorte <= 0)
                    continue;

                if (seleccionPorId.ContainsKey(producto.CorteMaestro.IdCorte))
                    continue;

                Entidades.CatalogoGlobalImportacionProducto importacionMaestro;
                if (importacionesMaestrosPorGlobal.TryGetValue(producto.CorteMaestro.IdCorte, out importacionMaestro)
                    && productosEmpresaPorId.ContainsKey(importacionMaestro.IdProductoEmpresa))
                {
                    continue;
                }

                return Json(new
                {
                    ok = false,
                    mensaje = "Para importar " + producto.CorteDesc + " también debe importar " + producto.CorteMaestro.CorteDesc + "."
                });
            }

            var conflictos = new List<string>();
            var codigosSeleccionados = new HashSet<long>();

            foreach (var item in seleccionados)
            {
                if (item.CodigoDestino <= 0)
                {
                    var productoCodigo = productosGlobales.FirstOrDefault(x => x.IdCorte == item.IdProductoGlobal);
                    conflictos.Add("El código de destino para \"" + (productoCodigo != null ? productoCodigo.CorteDesc : ("producto global ID " + item.IdProductoGlobal)) + "\" debe ser mayor a 0.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Precio))
                {
                    var productoPrecio = productosGlobales.FirstOrDefault(x => x.IdCorte == item.IdProductoGlobal);
                    conflictos.Add("Debe ingresar el precio para \"" + (productoPrecio != null ? productoPrecio.CorteDesc : ("producto global ID " + item.IdProductoGlobal)) + "\".");
                    continue;
                }

                float precioDestino;
                if (!TryParseFloatFlexible(item.Precio, out precioDestino) || precioDestino < 0)
                {
                    var productoPrecio = productosGlobales.FirstOrDefault(x => x.IdCorte == item.IdProductoGlobal);
                    conflictos.Add("El precio ingresado para \"" + (productoPrecio != null ? productoPrecio.CorteDesc : ("producto global ID " + item.IdProductoGlobal)) + "\" es inválido.");
                    continue;
                }

                if (!codigosSeleccionados.Add(item.CodigoDestino))
                {
                    conflictos.Add("El código " + item.CodigoDestino + " está repetido dentro de la importación.");
                    continue;
                }

                if (codigosEmpresa.Contains(item.CodigoDestino))
                {
                    var sugerido = SugerirCodigoLibre(new HashSet<long>(codigosEmpresa.Concat(codigosSeleccionados)), item.CodigoDestino);
                    var producto = productosGlobales.FirstOrDefault(x => x.IdCorte == item.IdProductoGlobal);
                    conflictos.Add("El código " + item.CodigoDestino + " ya existe para la empresa actual en \"" + (producto != null ? producto.CorteDesc : "producto") + "\". Sugerido: " + sugerido + ".");
                }
            }

            if (conflictos.Any())
                return Json(new { ok = false, mensaje = string.Join("<br/>", conflictos) });

            var productosGlobalesPorId = productosGlobales.ToDictionary(x => x.IdCorte, x => x);
            var idsEmpresaPorGlobal = new Dictionary<int, int>();

            foreach (var importacion in importacionesMaestros)
            {
                if (productosEmpresaPorId.ContainsKey(importacion.IdProductoEmpresa))
                    idsEmpresaPorGlobal[importacion.IdProductoGlobal] = importacion.IdProductoEmpresa;
            }

            foreach (var producto in OrdenarProductosParaImportacion(productosGlobales))
            {
                var seleccion = seleccionPorId[producto.IdCorte];
                float precioDestino;
                if (!TryParseFloatFlexible(seleccion.Precio, out precioDestino) || precioDestino < 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "El precio ingresado para \"" + producto.CorteDesc + "\" es inválido."
                    });
                }

                var nuevoProducto = ClonarProductoGlobal(producto, seleccion.CodigoDestino, precioDestino);
                nuevoProducto.CorteMaestro = null;

                if (producto.CorteMaestro != null && producto.CorteMaestro.IdCorte > 0)
                {
                    int idMaestroEmpresa;
                    if (!idsEmpresaPorGlobal.TryGetValue(producto.CorteMaestro.IdCorte, out idMaestroEmpresa) || idMaestroEmpresa <= 0)
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = "No se pudo resolver el corte maestro para \"" + producto.CorteDesc + "\"."
                        });
                    }

                    nuevoProducto.CorteMaestro = new Entidades.Corte { IdCorte = idMaestroEmpresa };
                }

                int idInsertado = _oCorteN.InsertarCorteEnEmpresa(nuevoProducto);
                if (idInsertado <= 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se pudo guardar \"" + producto.CorteDesc + "\" en la empresa actual."
                    });
                }

                var insertado = ObtenerProductoEmpresaSesionPorCodigo(seleccion.CodigoDestino);
                if (insertado == null || insertado.IdCorte <= 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Se importó \"" + producto.CorteDesc + "\" pero no se pudo recuperar el identificador generado."
                    });
                }

                idsEmpresaPorGlobal[producto.IdCorte] = insertado.IdCorte;
                _oCorteN.GuardarImportacionCatalogoGlobal(producto.IdCorte, insertado.IdCorte, idUsuario);
                codigosEmpresa.Add(seleccion.CodigoDestino);
            }

            return Json(new
            {
                ok = true,
                mensaje = "Se importaron " + seleccionados.Count + " productos correctamente."
            });
        }

        private CatalogoGlobalProductosVm ConstruirCatalogoGlobalVm(string busqueda, string tipo, int pagina, bool incluirTipos)
        {
            _oCorteN.AsegurarTablaImportacionCatalogoGlobal();

            var catalogoGlobal = ObtenerGestorCatalogoGlobal();
            pagina = pagina < 1 ? 1 : pagina;
            var productosGlobales = catalogoGlobal.ObtenerCatalogoGlobalPagina(busqueda, tipo, pagina, CatalogoGlobalTamanoPagina, 1) ?? new List<Entidades.CatalogoGlobalProducto>();
            bool hayMas = productosGlobales.Count > CatalogoGlobalTamanoPagina;
            if (hayMas)
                productosGlobales.RemoveAt(productosGlobales.Count - 1);
            var productosEmpresaActual = ObtenerProductosEmpresaSesionActual();
            var codigosEmpresa = new HashSet<long>(productosEmpresaActual.Select(x => x.Codigo));
            var productosEmpresaPorId = productosEmpresaActual.ToDictionary(x => x.IdCorte, x => x);
            var importaciones = _oCorteN.ObtenerImportacionesCatalogoGlobal()
                .ToDictionary(x => x.IdProductoGlobal, x => x);

            var model = new CatalogoGlobalProductosVm
            {
                Busqueda = busqueda ?? "",
                Tipo = tipo ?? "",
                Pagina = pagina,
                HayMas = hayMas
            };

            if (incluirTipos)
                model.Tipos = catalogoGlobal.ObtenerTiposCatalogoGlobal() ?? new List<string>();

            foreach (var producto in productosGlobales)
            {
                Entidades.CatalogoGlobalImportacionProducto importacion;
                Entidades.Corte productoEmpresaImportado = null;
                bool yaImportado = false;

                if (importaciones.TryGetValue(producto.IdCorte, out importacion))
                {
                    productosEmpresaPorId.TryGetValue(importacion.IdProductoEmpresa, out productoEmpresaImportado);
                    yaImportado = productoEmpresaImportado != null;
                }

                bool codigoDuplicado = codigosEmpresa.Contains(producto.Codigo);
                long codigoSugerido = codigoDuplicado
                    ? SugerirCodigoLibre(new HashSet<long>(codigosEmpresa), producto.Codigo)
                    : producto.Codigo;

                string mensajeEstado = "";
                if (yaImportado)
                {
                    mensajeEstado = "Ya importado";
                }
                else if (codigoDuplicado)
                {
                    mensajeEstado = "Código ocupado. Sugerido: " + codigoSugerido;
                }
                else
                {
                    mensajeEstado = "Listo para importar";
                }

                model.Productos.Add(new ProductoGlobalImportItemVm
                {
                    IdProductoGlobal = producto.IdCorte,
                    CodigoOriginal = producto.Codigo,
                    CodigoDestino = producto.Codigo,
                    Descripcion = producto.CorteDesc,
                    Tipo = producto.Tipo ?? "",
                    IdProductoGlobalMaestro = producto.CorteMaestro != null && producto.CorteMaestro.IdCorte > 0
                        ? (int?)producto.CorteMaestro.IdCorte
                        : null,
                    ProductoGlobalMaestroNombre = producto.CorteMaestro != null ? producto.CorteMaestro.CorteDesc : "",
                    EsPresentacion = producto.Presentacion,
                    Porcentaje = producto.Porcentaje,
                    YaImportado = yaImportado,
                    IdProductoEmpresaImportado = productoEmpresaImportado != null ? (int?)productoEmpresaImportado.IdCorte : null,
                    CodigoEmpresaImportado = productoEmpresaImportado != null ? (long?)productoEmpresaImportado.Codigo : null,
                    CodigoDuplicadoEnEmpresa = codigoDuplicado,
                    CodigoSugerido = codigoSugerido,
                    MensajeEstado = mensajeEstado
                });
            }

            return model;
        }

        private static long SugerirCodigoLibre(HashSet<long> codigosUsados, long codigoBase)
        {
            long sugerido = codigoBase > 0 ? codigoBase : 1;
            while (codigosUsados.Contains(sugerido))
            {
                sugerido++;
            }

            return sugerido;
        }

        private List<Entidades.Corte> ObtenerProductosEmpresaSesionActual()
        {
            int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
            if (idEmpresaSesion <= 0)
                return new List<Entidades.Corte>();

            return _oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>();
        }

        private Entidades.Corte ObtenerProductoEmpresaSesionPorCodigo(long codigo)
        {
            int idEmpresaSesion = _empresa != null ? _empresa.IdEmpresa : 0;
            if (idEmpresaSesion <= 0)
                return null;

            return _oCorteN.findCorteByCodigoEmpresa(codigo, idEmpresaSesion, false);
        }

        private static List<Entidades.CatalogoGlobalProducto> OrdenarProductosParaImportacion(IEnumerable<Entidades.CatalogoGlobalProducto> productos)
        {
            var lista = (productos ?? new List<Entidades.CatalogoGlobalProducto>()).ToList();
            var dict = lista.ToDictionary(x => x.IdCorte, x => x);
            var resultado = new List<Entidades.CatalogoGlobalProducto>();
            var visitados = new HashSet<int>();

            Action<Entidades.CatalogoGlobalProducto> visitar = null;
            visitar = producto =>
            {
                if (producto == null || !visitados.Add(producto.IdCorte))
                    return;

                if (producto.CorteMaestro != null && producto.CorteMaestro.IdCorte > 0)
                {
                    Entidades.CatalogoGlobalProducto maestro;
                    if (dict.TryGetValue(producto.CorteMaestro.IdCorte, out maestro))
                        visitar(maestro);
                }

                resultado.Add(producto);
            };

            foreach (var producto in lista.OrderBy(x => x.Codigo))
            {
                visitar(producto);
            }

            return resultado;
        }

        private static Entidades.Corte ClonarProductoGlobal(Entidades.CatalogoGlobalProducto global, long codigoDestino, float precioDestino)
        {
            var nuevo = new Entidades.Corte
            {
                IdCorte = 0,
                Codigo = codigoDestino,
                CorteDesc = global.CorteDesc,
                Tipo = global.Tipo,
                Pesable = global.Pesable,
                Promedio = global.Promedio,
                IdAlicuotaIva = global.IdAlicuotaIva,
                AlicuotaIva = global.AlicuotaIva,
                PuntoStock = global.PuntoStock,
                EnCierreStock = global.EnCierreStock,
                Habilitado = global.Habilitado,
                IngresoRapidoEmbutido = global.IngresoRapidoEmbutido,
                Independiente = global.Independiente,
                Porcentaje = global.Porcentaje,
                PorcentajeHueso = global.PorcentajeHueso,
                DesvioEstandar = global.DesvioEstandar,
                PrecioKg = precioDestino,
                PrecioKgReferencia = precioDestino,
                Presentacion = global.Presentacion,
                Nivel = global != null ? global.Nivel : 0
            };

            if (global.CorteMaestro != null && global.CorteMaestro.IdCorte > 0)
            {
                nuevo.CorteMaestro = new Entidades.Corte
                {
                    IdCorte = global.CorteMaestro.IdCorte,
                    CorteDesc = global.CorteMaestro.CorteDesc
                };
            }

            return nuevo;
        }

        private static bool TryParseFloatFlexible(string texto, out float valor)
        {
            valor = 0f;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            string normalizado = texto.Trim().Replace(".", "").Replace(",", ".");
            return float.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
        }

        [HttpGet]
        public async Task<IActionResult> VerGlobalesTiposProducto()
        {
            var model = ConstruirCatalogoGlobalTiposProductoVm("");
            string html = await RenderPartialViewToStringAsync("_CatalogoGlobalTiposProductoModal", model);
            return Content(html, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> BuscarGlobalesTiposProducto(string q = "")
        {
            var model = ConstruirCatalogoGlobalTiposProductoVm(q);
            string html = await RenderPartialViewToStringAsync("_CatalogoGlobalTiposProductoRows", model.Tipos);

            return Json(new
            {
                ok = true,
                html,
                cantidad = model.Tipos.Count
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ImportarTiposProductoSeleccionados(ImportarTiposProductoGlobalesRequest request)
        {
            // TODO(claude): mismo criterio que ImportarSeleccionados -- sin sesion real todavia,
            // se pasa null como usuario que hizo la importacion.
            int? idUsuario = null;

            var seleccionados = (request?.Tipos ?? new List<TipoProductoGlobalSeleccionVm>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Tipo))
                .Select(x => x.Tipo.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!seleccionados.Any())
                return Json(new { ok = false, mensaje = "Seleccione al menos un tipo de producto del catálogo global." });

            var model = ConstruirCatalogoGlobalTiposProductoVm("");
            var disponibles = model.Tipos
                .Where(x => !x.YaExisteEnEmpresa)
                .ToDictionary(x => x.Tipo, x => x, StringComparer.OrdinalIgnoreCase);

            var tiposAImportar = seleccionados
                .Where(x => disponibles.ContainsKey(x))
                .ToList();

            if (!tiposAImportar.Any())
                return Json(new { ok = false, mensaje = "Los tipos seleccionados ya existen en el sistema o no están disponibles para importar." });

            string mensaje = _oCorteN.importarTiposProductoGlobales(tiposAImportar, idUsuario);
            if (!string.IsNullOrWhiteSpace(mensaje))
                return Json(new { ok = false, mensaje = mensaje });

            return Json(new
            {
                ok = true,
                mensaje = "Se agregaron correctamente los tipos de producto seleccionados."
            });
        }

        private CatalogoGlobalTiposProductoVm ConstruirCatalogoGlobalTiposProductoVm(string busqueda)
        {
            var tiposGlobales = _oCorteN.obtenerTiposProductoCatalogoGlobal(busqueda ?? "") ?? new DataTable();
            var tiposEmpresaActual = _oCorteN.obtenerTiposProductoGrillaEmpresa("") ?? new DataTable();
            var nombresEmpresa = new HashSet<string>(
                tiposEmpresaActual.AsEnumerable()
                    .Select(x => (Convert.ToString(x["tipo"]) ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)),
                StringComparer.OrdinalIgnoreCase);

            var model = new CatalogoGlobalTiposProductoVm
            {
                Busqueda = busqueda ?? ""
            };

            foreach (DataRow row in tiposGlobales.Rows)
            {
                string tipo = (Convert.ToString(row["tipo"]) ?? "").Trim();
                bool yaExiste = nombresEmpresa.Contains(tipo);

                model.Tipos.Add(new TipoProductoGlobalImportItemVm
                {
                    Tipo = tipo,
                    Orden = row["orden"] != DBNull.Value ? Convert.ToInt32(row["orden"]) : 0,
                    YaExisteEnEmpresa = yaExiste,
                    MensajeEstado = yaExiste ? "Ya existe en el sistema" : "Disponible"
                });
            }

            return model;
        }
    }
}
