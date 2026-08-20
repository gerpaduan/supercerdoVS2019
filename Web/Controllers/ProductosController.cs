using Datos;
using Entidades;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utilidades;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class ProductosController : BaseController
    {
        private const int CatalogoGlobalTamanoPagina = 50;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Corte oCorteN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Persona oPersonaN;
        private Negocio.CortePuntoStockSucursal oCortePuntoStockSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oSucursalN = Web.Infrastructure.NegocioFactory.CrearSucursal(empresa, param);
            oCorteN = Web.Infrastructure.NegocioFactory.CrearCorte(empresa, param);
            oUsuarioN = Web.Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            oPersonaN = Web.Infrastructure.NegocioFactory.CrearPersona(empresa, param);
            oCortePuntoStockSucursalN = Web.Infrastructure.NegocioFactory.CrearCortePuntoStockSucursal(empresa, param);
        }

        public ActionResult Index(
            int SucursalId = 0,
            string tipo = "",
            int marcaId = 0,
            int proveedorId = 0,
            long? codigoDesde = null,
            long? codigoHasta = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
            var productos = (oCorteN.ObtenerCortesListado(idEmpresaSesion, SucursalId) ?? new List<Entidades.Corte>())
                .Where(x => x != null)
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

            var sucursales = oSucursalN.findAll(); // Obtiene List<Entidades.Sucursal>

            ViewBag.Sucursales = sucursales;
            ViewBag.SucursalId = SucursalId;
            ViewBag.Tipos = ObtenerListaTipos();
            ViewBag.Marcas = ObtenerListaMarcas();
            ViewBag.Proveedores = ObtenerListaProveedores();
            ViewBag.PuedeEditarProducto = PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null);
            ViewBag.PuedeModificarPreciosProducto = PermisosHelper.TienePermiso(Session, Permisos.Producto.ModificarPrecios, null);
            ViewBag.PuedeEliminarProducto = PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null);

            return View(productos);
        }

        private static DateTime ObtenerFechaFiltroProducto(Entidades.Corte producto)
        {
            if (producto == null)
                return DateTime.MinValue;

            return producto.Actualizado ?? producto.Creado;
        }

        [HttpGet]
        public ActionResult VerGlobales()
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return new HttpStatusCodeResult(403);

            var model = ConstruirCatalogoGlobalVm("", "", 1, true);
            return PartialView("~/Views/Productos/_CatalogoGlobalModal.cshtml", model);
        }

        [HttpGet]
        public JsonResult BuscarGlobales(string q = "", string tipo = "", int pagina = 1)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para importar productos." }, JsonRequestBehavior.AllowGet);

            var model = ConstruirCatalogoGlobalVm(q, tipo, pagina, false);
            string html = RenderPartialViewToString("~/Views/Productos/_CatalogoGlobalRows.cshtml", model.Productos);

            return Json(new
            {
                ok = true,
                html,
                cantidad = model.Productos.Count,
                pagina = model.Pagina,
                hayMas = model.HayMas
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ImportarSeleccionados(ImportarProductosGlobalesRequest request)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para importar productos." });

            var usuario = Session["Usuario"] as Entidades.Usuario;
            oCorteN.AsegurarTablaImportacionCatalogoGlobal();

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

            var importacionesExistentes = oCorteN.ObtenerImportacionesCatalogoGlobal()
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
            var importacionesMaestros = oCorteN.ObtenerImportacionesCatalogoGlobal();

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

                int idInsertado = oCorteN.InsertarCorteEnEmpresa(nuevoProducto);
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
                oCorteN.GuardarImportacionCatalogoGlobal(producto.IdCorte, insertado.IdCorte, usuario != null ? (int?)usuario.Id : null);
                codigosEmpresa.Add(seleccion.CodigoDestino);
            }

            return Json(new
            {
                ok = true,
                mensaje = "Se importaron " + seleccionados.Count + " productos correctamente."
            });
        }

        [HttpGet]
        public JsonResult BuscarProductoGlobalParaAlta(string codigoBarra)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para agregar productos." }, JsonRequestBehavior.AllowGet);

            if (!EsCodigoEanValido(codigoBarra))
                return Json(new { ok = false, mensaje = "Solo se autocompleta con EAN-8 o EAN-13 válidos." }, JsonRequestBehavior.AllowGet);

            long codigo = NormalizarCodigoBarra(codigoBarra);
            if (codigo <= 0)
                return Json(new { ok = false, mensaje = "Ingrese un código de barra válido." }, JsonRequestBehavior.AllowGet);

            var global = ObtenerGestorCatalogoGlobal().findCorteGlobalByCodigo(codigo, true);
            if (global == null)
                return Json(new { ok = false, mensaje = "No existe un producto global para ese código." }, JsonRequestBehavior.AllowGet);

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
            }, JsonRequestBehavior.AllowGet);
        }

        // Acción para búsqueda en vivo usada por el modal POS
        [HttpGet]
        public JsonResult ListarProductos(string q = "")
        {
            try
            {
                int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
                var productos = idEmpresaSesion > 0
                    ? (oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>())
                    : (oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>());

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.Trim();
                    productos = productos
                        .Where(p =>
                            (!string.IsNullOrEmpty(p.corte) && p.corte.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            p.codigo.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                // Mapeo ligero para el cliente
                var resultado = productos
                    .Select(p => new
                    {
                        id = p.IdCorte,
                        codigo = p.codigo.ToString(),
                        nombre = p.corte,
                        precio = p.precioKg
                    })
                    .Take(200) // tope razonable
                    .ToList();

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Opcional: loguear ex
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        private Negocio.CatalogoGlobalProducto ObtenerGestorCatalogoGlobal()
        {
            return Web.Infrastructure.NegocioFactory.CrearCatalogoGlobalProducto(new EmpresaContextNulo(), null);
        }

        private CatalogoGlobalProductosVm ConstruirCatalogoGlobalVm(string busqueda, string tipo, int pagina, bool incluirTipos)
        {
            oCorteN.AsegurarTablaImportacionCatalogoGlobal();

            var catalogoGlobal = ObtenerGestorCatalogoGlobal();
            pagina = pagina < 1 ? 1 : pagina;
            var productosGlobales = catalogoGlobal.ObtenerCatalogoGlobalPagina(busqueda, tipo, pagina, CatalogoGlobalTamanoPagina, 1) ?? new List<Entidades.CatalogoGlobalProducto>();
            bool hayMas = productosGlobales.Count > CatalogoGlobalTamanoPagina;
            if (hayMas)
                productosGlobales.RemoveAt(productosGlobales.Count - 1);
            var productosEmpresaActual = ObtenerProductosEmpresaSesionActual();
            var codigosEmpresa = new HashSet<long>(productosEmpresaActual.Select(x => x.Codigo));
            var productosEmpresaPorId = productosEmpresaActual.ToDictionary(x => x.IdCorte, x => x);
            var importaciones = oCorteN.ObtenerImportacionesCatalogoGlobal()
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
            int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
            if (idEmpresaSesion <= 0)
                return new List<Entidades.Corte>();

            return oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>();
        }

        private Entidades.Corte ObtenerProductoEmpresaSesionPorCodigo(long codigo)
        {
            int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
            if (idEmpresaSesion <= 0)
                return null;

            return oCorteN.findCorteByCodigoEmpresa(codigo, idEmpresaSesion, false);
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

        private static bool TryParseFloatFlexible(string texto, out float valor)
        {
            valor = 0f;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            string normalizado = texto.Trim().Replace(".", "").Replace(",", ".");
            return float.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
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

        // Caso defensivo de Guardar(): si vm.IdCorte terminó apuntando a una fila que resultó
        // ser global (entity.IdEmpresa == 0, ver mas abajo), esa fila todavia se lee de
        // dbo.Corte via findCorteById -- por eso este overload con Entidades.Corte se
        // mantiene. Deberia dejar de dispararse una vez que se borren las filas idEmpresa=0
        // de Corte (20260804-Delete_Corte_IdEmpresa0.sql), pero no se retira todavia por las
        // dudas (ver docs/09-cambios-y-pendientes/riesgos-conocidos.md, entrada de
        // sessionStorage del 2026-08-04, que ya mostro comportamiento raro de estado viejo
        // en esta misma pantalla).
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

        // Overload real usado por los flujos de catalogo global (dbo.CatalogoGlobalProducto):
        // VerGlobales/BuscarGlobales/ImportarSeleccionados, BuscarProductoGlobalParaAlta y
        // AgregarDesdeCodigoBarra.
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

        // ===============================
        // GET: CREAR
        // ===============================
        public ActionResult Crear()
        {
            var model = new Entidades.Corte();

            CargarCombos(model.IdCorte);
            return View("AddOrEdit", model);
        }

        // ===============================
        // GET: EDITAR
        // ===============================
        public ActionResult Edit(int id)
        {
            var model = oCorteN.findCorteById(id, true);
            if (model == null) return HttpNotFound();

            CargarCombos(id);
            return View("AddOrEdit", model);
        }

        #region ADDorEDIT

        // ==========================================
        // HELPERS conversion DataRow -> tipos seguros
        // ==========================================
        private static string ToStr(object o)
            => o == null || o == DBNull.Value ? "" : o.ToString();

        private static int ToInt(object o)
        {
            int v;
            int.TryParse(ToStr(o), out v);
            return v;
        }

        // Si tu campo "iva" viene como "21%" o "10,5%" esto lo convierte a float.
        // Si viene "Exento" o similar, devuelve 0.
        private static float ParseAlicuotaDesdeTextoIva(string ivaTexto)
        {
            if (string.IsNullOrWhiteSpace(ivaTexto)) return 0;

            // Busca el primer número (permite coma o punto)
            var m = Regex.Match(ivaTexto, @"(\d+(?:[.,]\d+)?)");
            if (!m.Success) return 0;

            var s = m.Groups[1].Value.Replace(",", ".");
            float v;
            float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
            return v;
        }

        // =========================
        // CARGAR COMBOS (DataTable)
        // =========================
        private void LoadCombos(CorteUpsertVM vm)
        {
            // -------- TIPOS (tabla tipos producto) --------
            DataTable dtTipos = oCorteN.obtenerTiposProducto(false);
            var tiposList = new List<SelectListItem>();

            if (dtTipos != null)
            {
                foreach (DataRow r in dtTipos.Rows)
                {
                    // Columna: tipo
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


            // -------- IVA (tabla alicuotas iva) --------
            // SELECT idIva, iva FROM AlicuotasIva
            DataTable dtIva = oCorteN.obtenerAlicuotasIva(false);
            var ivaList = new List<SelectListItem>();

            if (dtIva != null)
            {
                foreach (DataRow r in dtIva.Rows)
                {
                    int idIva = ToInt(r["idIva"]);      // Columna: idIva
                    string desc = ToStr(r["iva"]);      // Columna: iva

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

        // ==========================================
        // Si querés setear vm.AlicuotaIva SIN columna:
        // lo parseo desde el texto "iva"
        // ==========================================
        private float ObtenerAlicuotaPorcentajeDesdeDT(int idIvaBuscado)
        {
            DataTable dt = oCorteN.obtenerAlicuotasIva(false);
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

        // ===============================
        // EJEMPLO: GET AddOrEdit
        // ===============================
        public ActionResult AddOrEdit(int id = 0, bool cargaContinua = false, bool productoGuardado = false, int? ultimoProductoContinuoId = null, int? retomarProductoId = null, string flujoBaseContinuo = null)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                TempData["FlashError"] = "No tenés permisos para realizar la acción seleccionada.";
                return RedirectToAction("Index");
            }

            Entidades.Corte entity = (id == 0)
                ? new Entidades.Corte()
                : oCorteN.findCorteById(id, true);

            if (id > 0 && entity == null) return HttpNotFound();

            var vm = BuildVM(entity);   // Entity -> VM (tu método)
            vm.CargaContinua = cargaContinua;
            vm.UltimoProductoContinuoId = ultimoProductoContinuoId;
            vm.RetomarProductoId = retomarProductoId;
            vm.FlujoBaseContinuo = !string.IsNullOrWhiteSpace(flujoBaseContinuo)
                ? flujoBaseContinuo
                : (id > 0 ? "edicion" : "alta");

            LoadCombos(vm);
            ViewBag.ProductoGuardadoContinuo = productoGuardado;
            ViewBag.FlashSuccessContinuo = TempData["FlashSuccessContinuo"] as string;

            return View(vm);
        }

        // ===============================
        // POST Guardar
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(CorteUpsertVM vm)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                TempData["FlashError"] = "No tenés permisos para realizar la acción seleccionada.";
                return RedirectToAction("Index");
            }

            // 1) Normalizar floats desde Request.Form (acepta coma o punto)
            NormalizarFloatsDesdeRequest(vm);

            bool esEdicionFormulario = string.Equals(Request.Form["EsEdicionFormulario"], "1", StringComparison.Ordinal);
            if (!esEdicionFormulario)
            {
                vm.IdCorte = 0;
            }

            // 2) Validación server-side de código duplicado (seguridad extra)
            if (vm.Codigo > 0)
            {
                int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
                var existente = idEmpresaSesion > 0
                    ? oCorteN.findCorteByCodigoEmpresa(vm.Codigo, idEmpresaSesion, false)
                    : oCorteN.findCorteByCodigo(vm.Codigo, false);

                if (existente != null && existente.IdCorte != vm.IdCorte)
                {
                    ModelState.AddModelError("Codigo", $"El código ya existe para el producto: {existente.CorteDesc}");
                }
            }

            // 3) Validación de modo
            ValidarModoCorte(vm);

            if (!ModelState.IsValid)
            {
                LoadCombos(vm);
                return View("AddOrEdit", vm);
            }

            int idEmpresaSesionActual = empresa != null ? empresa.IdEmpresa : 0;
            bool altaDesdeCatalogoGlobal = false;
            bool codigoExisteEnCatalogoGlobal = false;

            if (vm.IdCorte <= 0 && idEmpresaSesionActual > 0 && vm.Codigo > 0)
            {
                codigoExisteEnCatalogoGlobal = ObtenerGestorCatalogoGlobal().findCorteGlobalByCodigo(vm.Codigo, false) != null;
                if (codigoExisteEnCatalogoGlobal)
                {
                    altaDesdeCatalogoGlobal = true;
                }
            }

            var entity = (vm.IdCorte > 0)
                ? oCorteN.findCorteById(vm.IdCorte, true)
                : new Entidades.Corte();

            if (vm.IdCorte > 0 && entity == null)
                return HttpNotFound();

            // Se guarda antes de mapear/clonar: si esto termina siendo una edicion real (no
            // alta, ver esAltaNueva mas abajo), sirve para detectar si EnCierreStock paso de
            // false a true en este guardado.
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

            // Si querés guardar el porcentaje (sin columna), lo saco del texto:
            vm.AlicuotaIva = ObtenerAlicuotaPorcentajeDesdeDT(vm.IdAlicuotaIva);

            MapToEntity(vm, entity); // VM -> Entity

            entity.IdEmpresa = idEmpresaSesionActual;

            // Se guarda antes de la mutacion de mas abajo: si vm.IdCorte ya es <= 0 aca, esto es
            // una insercion nueva en dbo.Corte (alta directa o clonado de un producto global),
            // sea cual sea el camino que se tome unas lineas mas abajo.
            bool esAltaNueva = vm.IdCorte <= 0;

            if (vm.IdCorte <= 0)
            {
                entity.IdCorte = 0;
            }

            if (altaDesdeCatalogoGlobal)
            {
                entity.IdCorte = oCorteN.InsertarCorteEnEmpresa(entity);
            }
            else
            {
                oCorteN.addOrEditCorte(entity);
            }

            int idProductoGuardado = entity.IdCorte;
            if (idProductoGuardado <= 0 && vm.Codigo > 0)
            {
                int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
                var productoGuardado = idEmpresaSesion > 0
                    ? oCorteN.findCorteByCodigoEmpresa(vm.Codigo, idEmpresaSesion, false)
                    : oCorteN.findCorteByCodigo(vm.Codigo, false);

                if (productoGuardado != null)
                {
                    idProductoGuardado = productoGuardado.IdCorte;
                }
            }

            // Alta nueva: el punto de stock cargado en el formulario se replica como valor
            // inicial en todas las sucursales existentes de la empresa (tabla intermedia
            // Producto x Sucursal, ver dbo.CortePuntoStockSucursal).
            if (esAltaNueva && idProductoGuardado > 0)
            {
                oCortePuntoStockSucursalN.CrearParaTodasLasSucursales(idEmpresaSesionActual, idProductoGuardado, vm.PuntoStock);
            }
            // Edicion de un producto existente que pasa de "no cierra stock" a "si cierra
            // stock": si por lo que sea no tiene fila en la tabla intermedia para alguna
            // sucursal (no deberia faltar, pero CrearParaTodasLasSucursales es idempotente
            // y sirve de red de seguridad), se crea ahi con el valor legacy del producto
            // como punto de stock inicial.
            else if (!esAltaNueva && idProductoGuardado > 0 && !enCierreStockAntesDeEditar && entity.EnCierreStock)
            {
                oCortePuntoStockSucursalN.CrearParaTodasLasSucursales(idEmpresaSesionActual, idProductoGuardado, entity.PuntoStock);
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
        // ============================================
        // 1) ValidarModoCorte
        // ============================================
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

            // Corte maestro obligatorio en CorteMaestro o Presentacion
            if (!vm.IdCorteMaestro.HasValue || vm.IdCorteMaestro.Value <= 0)
                ModelState.AddModelError("", "Seleccioná un corte maestro.");

            if (vm.ModoCorte == "CorteMaestro")
            {
                // Si querés exigir valores > 0, descomentá
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
            // Campos float de la vista
            TrySetFloatFromRequest("PrecioKg", v => vm.PrecioKg = v, "Precio Kg");
            TrySetFloatFromRequest("Promedio", v => vm.Promedio = v, "Promedio");
            TrySetFloatFromRequest("Porcentaje", v => vm.Porcentaje = v, "Porcentaje");
            TrySetFloatFromRequest("PorcentajeHueso", v => vm.PorcentajeHueso = v, "Desperdicio");
        }

        /// <summary>
        /// Lee Request.Form[key], acepta coma o punto decimal, y setea el float en VM.
        /// Si viene vacío, no agrega error (campo opcional).
        /// </summary>
        private void TrySetFloatFromRequest(string key, Action<float> setter, string label)
        {
            var raw = (Request.Form[key] ?? string.Empty).Trim();

            // Si está vacío, lo dejamos como vino del binder (normalmente 0 en float)
            // y removemos errores de parseo si existieran por tema de cultura.
            if (string.IsNullOrWhiteSpace(raw))
            {
                ModelState.Remove(key);
                return;
            }

            // Soporta coma o punto
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

            // Muy importante: sacamos el error del binder si falló por cultura
            ModelState.Remove(key);
        }

        // ============================================
        // 2) BuildVM (Entity -> VM)
        // ============================================
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

            // Marca (puede ser null)
            vm.MarcaNombre = e.MarcaNombre;
            vm.IdMarca = (e.Marca != null) ? (int?)GetIdPersonaReflection(e.Marca) : null;

            // Independiente (int -> bool)
            vm.Independiente = (e.Independiente == 1);

            // Corte Maestro / Presentación
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

            // Si es alta y querés independiente por defecto tildado:
            if (vm.IdCorte == 0)
                vm.Independiente = true;

            return vm;
        }


        // ============================================
        // 3) MapToEntity (VM -> Entity)
        // ============================================
        private void MapToEntity(CorteUpsertVM vm, Entidades.Corte e)
        {
            // Campos principales
            e.Codigo = vm.Codigo;
            e.CorteDesc = vm.CorteDesc;
            e.PrecioKg = vm.PrecioKg;
            e.Tipo = vm.Tipo;

            e.Pesable = vm.Pesable;
            e.Promedio = vm.Promedio;

            e.IdAlicuotaIva = vm.IdAlicuotaIva;
            e.AlicuotaIva = vm.AlicuotaIva; // si lo calculás desde el texto iva, queda acá

            e.PuntoStock = vm.PuntoStock;
            e.EnCierreStock = vm.EnCierreStock;
            e.Habilitado = vm.Habilitado;
            e.IngresoRapidoEmbutido = vm.IngresoRapidoEmbutido;

            e.Independiente = vm.Independiente ? 1 : 0;

            // Marca (sin depender de oPersonaN)
            if (vm.IdMarca.HasValue && vm.IdMarca.Value > 0)
            {
                var p = new Entidades.Persona();              // <-- tu namespace real
                SetIdPersonaReflection(p, vm.IdMarca.Value);  // setea IdPersona o idPersona
                e.Marca = p;
            }
            else
            {
                e.Marca = null;
            }

            // Modo Corte
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
                // Corte maestro obligatorio
                if (!vm.IdCorteMaestro.HasValue || vm.IdCorteMaestro.Value <= 0)
                    throw new Exception("Falta seleccionar corte maestro.");

                // Seteo corte maestro por Id (sin otro query)
                var cm = new Entidades.Corte();
                cm.IdCorte = vm.IdCorteMaestro.Value;
                e.CorteMaestro = cm;

                if (vm.ModoCorte == "CorteMaestro")
                {
                    e.Presentacion = false;
                    e.Porcentaje = vm.Porcentaje;
                    e.PorcentajeHueso = vm.PorcentajeHueso;
                }
                else // Presentacion
                {
                    e.Presentacion = true;

                    // Recalculo SIEMPRE del lado servidor
                    e.Porcentaje = vm.PresentacionUnidades ?? 0;
                    e.PorcentajeHueso = 0;
                }
            }

            // Fechas
            if (vm.IdCorte == 0 && e.Creado == default(DateTime))
                e.Creado = DateTime.Now;

            e.Actualizado = DateTime.Now;
        }


        // ============================================
        // Helpers Reflection Persona Id (IdPersona / idPersona)
        // ============================================
        private int GetIdPersonaReflection(object persona)
        {
            if (persona == null) return 0;
            var t = persona.GetType();

            // Propiedad
            var p1 = t.GetProperty("IdPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p1 != null) return Convert.ToInt32(p1.GetValue(persona, null) ?? 0);

            var p2 = t.GetProperty("idPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p2 != null) return Convert.ToInt32(p2.GetValue(persona, null) ?? 0);

            // Campo
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

            // Propiedad
            var p1 = t.GetProperty("IdPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p1 != null && p1.CanWrite) { p1.SetValue(persona, id, null); return; }

            var p2 = t.GetProperty("idPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p2 != null && p2.CanWrite) { p2.SetValue(persona, id, null); return; }

            // Campo
            var f1 = t.GetField("IdPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f1 != null) { f1.SetValue(persona, id); return; }

            var f2 = t.GetField("idPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f2 != null) { f2.SetValue(persona, id); return; }
        }

    #endregion


    [HttpGet]
        public ActionResult findCorteById(int id)
        {
            var corte = oCorteN.findCorteById(id, true);  // ← tu método para buscar

            if (corte == null)
                return HttpNotFound();


            return Json(new
            {
                id = corte.IdCorte,
                descripcion = corte.CorteDesc,
                precio = corte.PrecioKg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FindCorteByCodigo(long? codigo, int? idExcluir = null)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
            {
                return Json(new
                {
                    existe = false
                }, JsonRequestBehavior.AllowGet);
            }

            int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
            var corte = idEmpresaSesion > 0
                ? oCorteN.findCorteByCodigoEmpresa(codigo.Value, idEmpresaSesion, false)
                : oCorteN.findCorteByCodigo(codigo.Value, false);

            if (corte == null)
            {
                return Json(new
                {
                    existe = false
                }, JsonRequestBehavior.AllowGet);
            }

            // Si es edición y el código pertenece al mismo producto, no lo marcamos duplicado
            int idExc = idExcluir.GetValueOrDefault();
            if (idExc > 0 && corte.IdCorte == idExc)
            {
                return Json(new
                {
                    existe = false,
                    mismoRegistro = true,
                    id = corte.IdCorte,
                    nombre = corte.CorteDesc
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                existe = true,
                id = corte.IdCorte,
                nombre = corte.CorteDesc,
                descripcion = corte.CorteDesc,
                precio = corte.PrecioKg
            }, JsonRequestBehavior.AllowGet);
        }

        // ===============================
        // GET: modificar Precio
        // ===============================
        [HttpPost]
        public ActionResult EditPrecioCorte(int IdCorte, string PrecioKg)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.ModificarPrecios, null))
            {
                ViewBag.Seccion = "Productos - Modificar Precios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (string.IsNullOrWhiteSpace(PrecioKg))
                return Json(new { error = "Precio vacío" });

            // Normalizar: quitar miles y pasar coma a punto
            string normalizado = PrecioKg
                .Replace(".", "")
                .Replace(",", ".");

            if (!float.TryParse(
                    normalizado,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out float precioDecimal))
            {
                return Json(new { error = "Formato de precio inválido" });
            }
            Entidades.Corte model = new Entidades.Corte();
            model.idCorte = IdCorte;
            model.precioKg = precioDecimal;
            oCorteN.editPrecioCorte(model);

            return Json(new
            {
                id = IdCorte,
                precio = precioDecimal,
                precioFormateado = "$ "+precioDecimal.ToString("N2", new CultureInfo("es-AR"))
            });
        }

        // ===============================
        // Guardar punto de stock por sucursal (en lote, todas las sucursales de un
        // producto juntas — nunca sucursal por sucursal, ver modal "Ver stock por sucursales").
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarPuntosStockSucursal(int idCorte, List<PuntoStockSucursalItemVm> valores)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                return Json(new { error = "No tenés permisos para editar el punto de stock." });
            }

            if (idCorte <= 0)
                return Json(new { error = "Producto inválido." });

            if (valores == null || valores.Count == 0)
                return Json(new { error = "No hay valores para guardar." });

            if (valores.Any(v => v.PuntoStock < 0))
                return Json(new { error = "El punto de stock debe ser un número entero mayor o igual a 0." });

            int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;

            try
            {
                var lista = valores
                    .Select(v => (idSucursal: v.IdSucursal, puntoStock: v.PuntoStock))
                    .ToList();

                oCortePuntoStockSucursalN.GuardarPuntosStockLote(idEmpresaSesion, idCorte, lista);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("GuardarPuntosStockSucursal - idCorte={0}: {1}", idCorte, ex);
                return Json(new { error = "No se pudieron guardar los puntos de stock. Intentá de nuevo." });
            }

            return Json(new { ok = true });
        }

        // ===============================
        // CARGA DE COMBOS
        // ===============================
        private void CargarCombos(int idCorte)
        {
            ViewBag.Marcas = ObtenerListaMarcas();
            ViewBag.Tipos = ObtenerListaTipos();
        }

        private IEnumerable<SelectListItem> ObtenerListaMarcas()
        {
            DataTable dt = oPersonaN.buscarPersona("", true);
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
            DataTable dt = oCorteN.obtenerTiposProducto(false);
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
            DataTable dt = oPersonaN.obtenerProveedoresConCompras();
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

            DataTable dtCortes = oCorteN.obtenerCortesPorProveedor(proveedorId);
            return dtCortes
                .AsEnumerable()
                .Where(row => row["idCorte"] != DBNull.Value)
                .Select(row => Convert.ToInt32(row["idCorte"]))
                .ToHashSet();
        }

        public ActionResult Tipos(string buscar = "")
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.VerTiposProducto, null))
            {
                TempData["FlashError"] = "No tenés permisos para ver tipos de producto.";
                return RedirectToAction("Index");
            }

            ViewBag.BuscarTipoProducto = (buscar ?? "").Trim();
            ViewBag.PuedeEditarTiposProducto = PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null);

            DataTable dt = oCorteN.obtenerTiposProductoGrillaEmpresa((buscar ?? "").Trim()) ?? new DataTable();
            return View(dt);
        }

        [HttpGet]
        public ActionResult VerGlobalesTiposProducto()
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null))
                return new HttpStatusCodeResult(403);

            var model = ConstruirCatalogoGlobalTiposProductoVm("");
            return PartialView("~/Views/Productos/_CatalogoGlobalTiposProductoModal.cshtml", model);
        }

        [HttpGet]
        public JsonResult BuscarGlobalesTiposProducto(string q = "")
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para importar tipos de producto." }, JsonRequestBehavior.AllowGet);

            var model = ConstruirCatalogoGlobalTiposProductoVm(q);
            string html = RenderPartialViewToString("~/Views/Productos/_CatalogoGlobalTiposProductoRows.cshtml", model.Tipos);

            return Json(new
            {
                ok = true,
                html,
                cantidad = model.Tipos.Count
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ImportarTiposProductoSeleccionados(ImportarTiposProductoGlobalesRequest request)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para importar tipos de producto." });

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

            string mensaje = oCorteN.importarTiposProductoGlobales(tiposAImportar, PermisosHelper.ObtenerUsuario(Session)?.Id);
            if (!string.IsNullOrWhiteSpace(mensaje))
                return Json(new { ok = false, mensaje = mensaje });

            return Json(new
            {
                ok = true,
                mensaje = "Se agregaron correctamente los tipos de producto seleccionados."
            });
        }

        [HttpGet]
        public ActionResult TipoProductoModal(string tipo = "")
        {
            try
            {
                if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null))
                    return Content("<div class='alert alert-danger mb-0'>No tenés permisos para administrar tipos de producto.</div>");

                bool esEdicion = !string.IsNullOrWhiteSpace(tipo);
                var model = new Web.Models.TipoProductoEditVm
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
                        return Content("<div class='alert alert-danger mb-0'>No se encontró el tipo de producto seleccionado.</div>");

                    model.TipoOriginal = Convert.ToString(row["tipo"]);
                    model.Tipo = Convert.ToString(row["tipo"]);
                    model.Orden = row["orden"] != DBNull.Value ? Convert.ToInt32(row["orden"]) : 100;
                    model.Reservado = row.Table.Columns.Contains("Reservado")
                        && row["Reservado"] != DBNull.Value
                        && Convert.ToBoolean(row["Reservado"]);
                }

                string html = RenderPartialViewToString("_AddOrEditTipoProducto", model);
                return Content(html);
            }
            catch (Exception ex)
            {
                string detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("<div class='alert alert-danger mb-0'>No se pudo abrir el formulario: " + HttpUtility.HtmlEncode(detalle) + "</div>");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarTipoProducto(Web.Models.TipoProductoEditVm model)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null))
                return Json(new { success = false, message = "No tenés permisos para administrar tipos de producto." });

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

            string mensaje = oCorteN.addOrEditTipoProducto(tipo, model.Orden.ToString(CultureInfo.InvariantCulture), esInsert, tipoOriginal);
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
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.AddOrEditTipoProducto, null))
                return Json(new { success = false, message = "No tenés permisos para eliminar tipos de producto." });

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

            string mensaje = oCorteN.eliminarTipoProducto(tipo);
            if (!string.IsNullOrWhiteSpace(mensaje))
                return Json(new { success = false, message = mensaje });

            return Json(new { success = true, message = "El tipo de producto se eliminó correctamente." });
        }

        public ActionResult Marcas(string buscar = "")
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.VerCortes, null))
            {
                TempData["FlashError"] = "No tenés permisos para ver marcas.";
                return RedirectToAction("Index");
            }

            ViewBag.BuscarMarcaAdmin = (buscar ?? "").Trim();
            ViewBag.PuedeCrearMarca = PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null);
            ViewBag.UsuarioAdmin = PermisosHelper.ObtenerUsuario(Session)?.Admin ?? false;

            DataTable dt = oPersonaN.buscarPersona((buscar ?? "").Trim(), true) ?? new DataTable();
            return View(dt);
        }

        [HttpGet]
        public ActionResult MarcaModal(int idPersona = 0)
        {
            try
            {
                if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                    return Content("<div class='alert alert-danger mb-0'>No tenés permisos para administrar marcas.</div>");

                var usuario = PermisosHelper.ObtenerUsuario(Session);
                bool esAdministrador = usuario != null && usuario.Admin;

                var model = new Web.Models.MarcaEditVm
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
                    var marca = oPersonaN.findById(idPersona);
                    if (marca == null || marca.IdPersona <= 0 || !marca.Marca)
                        return Content("<div class='alert alert-danger mb-0'>No se encontró la marca seleccionada.</div>");

                    model.IdPersona = marca.IdPersona;
                    model.RazonSocial = marca.RazonSocial ?? "";
                    model.OtrosDatos = marca.OtrosDatos ?? "";
                    model.IdPropietario = marca.Propietario != null && marca.Propietario.IdPersona > 0
                        ? (int?)marca.Propietario.IdPersona
                        : (marca.IdPropietario.HasValue && marca.IdPropietario.Value > 0 ? marca.IdPropietario : null);
                    model.PropietarioNombre = marca.Propietario != null ? (marca.Propietario.RazonSocial ?? "") : "";
                    model.SoloLecturaNombre = !esAdministrador;
                }

                string html = RenderPartialViewToString("_AddOrEditMarca", model);
                return Content(html);
            }
            catch (Exception ex)
            {
                string detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content("<div class='alert alert-danger mb-0'>No se pudo abrir el formulario: " + HttpUtility.HtmlEncode(detalle) + "</div>");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarMarca(Web.Models.MarcaEditVm model)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { success = false, message = "No tenés permisos para administrar marcas." });

            var usuario = PermisosHelper.ObtenerUsuario(Session);
            bool esAdministrador = usuario != null && usuario.Admin;
            bool esInsert = model == null || model.IdPersona <= 0;

            if (model == null)
                return Json(new { success = false, message = "No se pudo procesar la marca." });

            model.RazonSocial = (model.RazonSocial ?? "").Trim();
            model.OtrosDatos = (model.OtrosDatos ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.RazonSocial))
                return Json(new { success = false, message = "El campo Nombre Marca no puede estar vacío." });

            Entidades.Persona marca = esInsert ? new Entidades.Persona() : oPersonaN.findById(model.IdPersona);
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
                var propietario = oPersonaN.findById(model.IdPropietario.Value);
                if (propietario == null || propietario.IdPersona <= 0)
                    return Json(new { success = false, message = "No se encontró la persona seleccionada como propietaria." });

                marca.Propietario = propietario;
                marca.IdPropietario = propietario.IdPersona;
            }

            oPersonaN.addOrEditPersona(marca);

            return Json(new
            {
                success = true,
                message = esInsert ? "La marca se guardó correctamente." : "La marca se actualizó correctamente."
            });
        }

        private DataRow BuscarTipoProductoRow(string tipo)
        {
            tipo = (tipo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tipo))
                return null;

            DataTable dt = oCorteN.obtenerTiposProductoGrillaEmpresa("") ?? new DataTable();
            return dt.AsEnumerable().FirstOrDefault(row =>
                string.Equals(Convert.ToString(row["tipo"]) ?? "", tipo, StringComparison.OrdinalIgnoreCase));
        }

        private CatalogoGlobalTiposProductoVm ConstruirCatalogoGlobalTiposProductoVm(string busqueda)
        {
            var tiposGlobales = oCorteN.obtenerTiposProductoCatalogoGlobal(busqueda ?? "") ?? new DataTable();
            var tiposEmpresaActual = oCorteN.obtenerTiposProductoGrillaEmpresa("") ?? new DataTable();
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

                DataTable dtTemp = oPersonaN.existenMarcasParecidas(palabra, idMarcaActual);
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


        // ===============================
        // EJEMPLO: POST Eliminar
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            // 1) Permisos -> cartel y volver al Index
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                TempData["FlashError"] = "No tenés permisos para realizar la acción seleccionada.";
                return RedirectToAction("Index");
            }

            // 2) Validación de Id
            if (id <= 0)
            {
                TempData["FlashError"] = "No se pudo eliminar: producto inválido.";
                return RedirectToAction("Index");
            }

            // 3) Buscar entidad real
            var entity = oCorteN.findCorteById(id, true);
            if (entity == null)
            {
                TempData["FlashError"] = "No se pudo eliminar: el producto no existe o ya fue eliminado.";
                return RedirectToAction("Index");
            }

            // 4) Guardar nombre ANTES de eliminar
            var nombre = entity.CorteDesc ?? entity.corte ?? "(sin nombre)";

            // 5) Eliminar
            try
            {
                oCorteN.eliminarCorte(entity);
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

            // 6) Mensaje OK
            TempData["FlashSuccess"] = $"El producto \"{nombre}\" se eliminó correctamente.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public JsonResult BuscarMarca(string q = "")
        {
            q = (q ?? "").Trim();

            // Si tu método con q="" ya devuelve todas, listo.
            // Si NO devuelve todas, ahí tenés que llamar a otro método "obtenerTodas".
            var dt = oPersonaN.buscarPersona(q, true); // DataTable

            var list = new List<object>();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    id = Convert.ToInt32(row["idPersona"]),          // <-- AJUSTÁ
                    nombre = Convert.ToString(row["Marca"])    // <-- AJUSTÁ
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // ============================================================
        // ETIQUETAS (PDF) -- generación de etiquetas de precio para imprimir.
        // Reemplaza al mecanismo equivalente de WinForms (Presentacion/Cortes/
        // formEtiquetas.cs, iTextSharp). Rediseñado dos veces a pedido del
        // usuario: primero agregando código de barras + 3 tamaños de hoja: mas
        // adelante, con membrete (logo CarniSys) y fecha de emisión con hora,
        // sobre una referencia visual que pasó el usuario. Layout dibujado con
        // posicionamiento absoluto (ColumnText/PdfContentByte), no con
        // PdfPTable -- ver docs/DECISIONS.md, 2026-08-08, por qué (PdfPCell con
        // FixedHeight puede no dibujar contenido sin avisar en esta versión de
        // iTextSharp).
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarEtiquetasPdf(string ids, string tamano)
        {
            var idsList = (ids ?? "")
                .Split(',')
                .Select(s => { int id; return int.TryParse(s, out id) ? id : 0; })
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (idsList.Count == 0)
                return HttpNotFound();

            var productos = idsList
                .Select(id => oCorteN.findCorteById(id, false))
                .Where(c => c != null && c.IdCorte > 0)
                .ToList();

            if (productos.Count == 0)
                return HttpNotFound();

            byte[] bytes = GenerarPdfEtiquetas(productos, ResolverTamanoEtiqueta(tamano));
            return File(bytes, "application/pdf", "Etiquetas.pdf");
        }

        // Presets de tamaño de etiqueta. "mediana" es el tamaño que ya usaba el
        // WinForms (60x35mm); "chica" y "grande" son nuevas, pedidas para poder
        // elegir según el uso (góndola vs. mostrador). En "chica" no entran
        // cómodamente el logo ni la fecha -- se saltean por falta de espacio.
        private class TamanoEtiqueta
        {
            public float AnchoMm;
            public float AltoMm;
            public bool MostrarLogo;
            public bool MostrarFecha;
            public float FuenteNombreGrande;
            public float FuenteNombreMedia;
            public float FuenteNombreChica;
            public float FuentePrecio;
            public float FuentePrecioLabel;
            public float FuenteFecha;
        }

        private static TamanoEtiqueta ResolverTamanoEtiqueta(string tamano)
        {
            switch ((tamano ?? "").Trim().ToLowerInvariant())
            {
                case "chica":
                    return new TamanoEtiqueta
                    {
                        AnchoMm = 40,
                        AltoMm = 30,
                        MostrarLogo = false,
                        MostrarFecha = true,
                        // Nombre reducido 30% (9/8/7 -> 6.3/5.6/4.9), "precio unitario"
                        // y fecha 2pt mas chicas -- pedido explicito del usuario.
                        FuenteNombreGrande = 6.3f,
                        FuenteNombreMedia = 5.6f,
                        FuenteNombreChica = 4.9f,
                        FuentePrecio = 20,
                        FuentePrecioLabel = 4,
                        FuenteFecha = 3
                    };
                case "grande":
                    return new TamanoEtiqueta
                    {
                        AnchoMm = 100,
                        AltoMm = 50,
                        MostrarLogo = true,
                        MostrarFecha = true,
                        FuenteNombreGrande = 14f,
                        FuenteNombreMedia = 11.9f,
                        FuenteNombreChica = 10.5f,
                        FuentePrecio = 46,
                        FuentePrecioLabel = 7,
                        FuenteFecha = 6
                    };
                case "mediana":
                default:
                    return new TamanoEtiqueta
                    {
                        AnchoMm = 60,
                        AltoMm = 35,
                        MostrarLogo = true,
                        MostrarFecha = true,
                        FuenteNombreGrande = 9.1f,
                        FuenteNombreMedia = 7.7f,
                        FuenteNombreChica = 7f,
                        FuentePrecio = 30,
                        FuentePrecioLabel = 5,
                        FuenteFecha = 4
                    };
            }
        }

        private const float MmAPuntos = 2.8346f;

        private byte[] GenerarPdfEtiquetas(List<Entidades.Corte> productos, TamanoEtiqueta tam)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                float etiquetaAncho = tam.AnchoMm * MmAPuntos;
                float etiquetaAlto = tam.AltoMm * MmAPuntos;
                float margenIzquierdo = 10f * MmAPuntos;
                float margenSuperior = 10f * MmAPuntos;
                float espacioHorizontal = 2f * MmAPuntos;
                float espacioVertical = 2f * MmAPuntos;

                float hojaAncho = PageSize.A4.Width;
                float hojaAlto = PageSize.A4.Height;

                int etiquetasPorFila = Math.Max(1, (int)((hojaAncho - 2 * margenIzquierdo + espacioHorizontal) / (etiquetaAncho + espacioHorizontal)));
                int etiquetasPorColumna = Math.Max(1, (int)((hojaAlto - 2 * margenSuperior + espacioVertical) / (etiquetaAlto + espacioVertical)));

                var fontNombreGrande = new Font(Font.FontFamily.HELVETICA, tam.FuenteNombreGrande, Font.NORMAL);
                var fontNombreMedia = new Font(Font.FontFamily.HELVETICA, tam.FuenteNombreMedia, Font.NORMAL);
                var fontNombreChica = new Font(Font.FontFamily.HELVETICA, tam.FuenteNombreChica, Font.NORMAL);
                var fontPrecio = new Font(Font.FontFamily.HELVETICA, tam.FuentePrecio, Font.BOLD);
                var fontPrecioSimbolo = new Font(Font.FontFamily.HELVETICA, tam.FuentePrecio * 0.45f, Font.BOLD);
                var fontPrecioLabel = new Font(Font.FontFamily.HELVETICA, tam.FuentePrecioLabel, Font.BOLD, BaseColor.DARK_GRAY);
                var fontFechaLabel = new Font(Font.FontFamily.HELVETICA, tam.FuenteFecha, Font.BOLD, BaseColor.DARK_GRAY);
                var fontFechaValor = new Font(Font.FontFamily.HELVETICA, tam.FuenteFecha, Font.NORMAL, BaseColor.DARK_GRAY);
                var bfBarcode = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                float fuenteBarcodeTexto = Math.Max(6f, tam.FuentePrecioLabel - 1f);

                // Logo cargado una sola vez y reusado en todas las etiquetas -- iTextSharp
                // deduplica el XObject de imagen cuando se reusa la misma instancia de
                // Image, así que no infla el tamaño del PDF por repetirlo. Si el archivo
                // falta o no se puede leer, se sigue sin logo en vez de romper todo el PDF.
                Image logo = null;
                if (tam.MostrarLogo)
                {
                    try
                    {
                        logo = Image.GetInstance(Server.MapPath("~/Content/img/CarniSys_Logo_sinSlogan.png"));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("GenerarPdfEtiquetas - no se pudo cargar el logo: {0}", ex);
                    }
                }

                string fechaImpresion = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                int productoIndex = 0;
                while (productoIndex < productos.Count)
                {
                    if (productoIndex > 0)
                        document.NewPage();

                    for (int fila = 0; fila < etiquetasPorColumna; fila++)
                    {
                        for (int columna = 0; columna < etiquetasPorFila; columna++)
                        {
                            if (productoIndex >= productos.Count)
                                break;

                            var producto = productos[productoIndex];
                            float x = margenIzquierdo + columna * (etiquetaAncho + espacioHorizontal);
                            float yTop = hojaAlto - margenSuperior - fila * (etiquetaAlto + espacioVertical);

                            DibujarEtiqueta(
                                producto, tam, writer.DirectContent, x, yTop, etiquetaAncho, etiquetaAlto, logo,
                                fontNombreGrande, fontNombreMedia, fontNombreChica,
                                fontPrecio, fontPrecioSimbolo, fontPrecioLabel,
                                fontFechaLabel, fontFechaValor, bfBarcode, fuenteBarcodeTexto,
                                fechaImpresion);

                            productoIndex++;
                        }
                    }
                }

                document.Close();
                return ms.ToArray();
            }
        }

        // Dibuja una etiqueta completa por posicionamiento absoluto dentro del
        // rectángulo (x, yTop) - (x+ancho, yTop-alto) -- yTop es el borde
        // SUPERIOR (coordenadas PDF, Y crece hacia arriba). Estructura, de
        // arriba hacia abajo: nombre + logo (misma fila) -> línea divisoria ->
        // precio grande + etiqueta "precio por kg/unitario" -> código de barras
        // + fecha de emisión (misma fila).
        private void DibujarEtiqueta(
            Entidades.Corte producto, TamanoEtiqueta tam, PdfContentByte cb,
            float x, float yTop, float ancho, float alto, Image logo,
            Font fontNombreGrande, Font fontNombreMedia, Font fontNombreChica,
            Font fontPrecio, Font fontPrecioSimbolo, Font fontPrecioLabel,
            Font fontFechaLabel, Font fontFechaValor, BaseFont bfBarcode, float fuenteBarcodeTexto,
            string fechaImpresion)
        {
            float pad = ancho * 0.05f;
            float contentLeft = x + pad;
            float contentRight = x + ancho - pad;
            float contentWidth = contentRight - contentLeft;

            // Borde fino -- no es parte del diseño de referencia (esa era una sola
            // etiqueta suelta, sin vecinas), pero en una hoja con varias etiquetas
            // por página sirve de guía de corte.
            cb.SetLineWidth(0.4f);
            cb.SetColorStroke(new BaseColor(200, 200, 200));
            cb.Rectangle(x, yTop - alto, ancho, alto);
            cb.Stroke();

            // --- Encabezado: nombre (izquierda) + logo (derecha) ---
            // 0.30 en vez de un valor más ajustado: con nombres largos que
            // wrappean a 2 líneas en el tamaño "chica" (fuente más chica, 7pt),
            // una zona más angosta hacía que la segunda línea se superpusiera
            // con la línea divisoria (visto en una captura real del PDF).
            float headerH = alto * 0.30f;
            float headerBottom = yTop - headerH;

            // Ancho del logo calculado ANTES que el nombre, para reservarle el
            // espacio -- el nombre se dibuja acotado a un ColumnText (con wrap
            // real) en vez de ShowTextAligned sin límite de ancho: un nombre
            // largo con ShowTextAligned se desborda encima de la etiqueta
            // vecina en la grilla en vez de cortar o pasar a una segunda línea
            // (encontrado y corregido al revisar una captura real del PDF).
            float logoW = 0f, logoH = 0f;
            if (tam.MostrarLogo && logo != null)
            {
                float logoMaxW = contentWidth * 0.30f;
                float logoMaxH = headerH * 0.85f;
                float escala = Math.Min(logoMaxW / logo.Width, logoMaxH / logo.Height);
                logoW = logo.Width * escala;
                logoH = logo.Height * escala;
                logo.SetAbsolutePosition(contentRight - logoW, headerBottom + (headerH - logoH) / 2f);
                logo.ScalePercent(escala * 100f);
            }

            string nombre = (producto.CorteDesc ?? "").ToUpperInvariant();
            Font fontNombre = nombre.Length > 40 ? fontNombreChica : (nombre.Length > 25 ? fontNombreMedia : fontNombreGrande);
            float nombreRight = contentRight - (logoW > 0 ? logoW + pad : 0f);
            // Margen de seguridad real (15% de la zona de encabezado) entre el
            // límite inferior de la columna de texto y la línea divisoria -- sin
            // esto, un nombre que wrappea a 2 líneas puede calcular su segunda
            // línea justo encima de donde se traza la línea, y el trazo termina
            // cruzando las letras (visto en una captura real, tamaño "grande").
            // Con este margen, si una segunda línea no entra ya ni se dibuja
            // (ColumnText la descarta en vez de dibujarla superpuesta).
            float nombreBottom = headerBottom + headerH * 0.15f;
            var ctNombre = new ColumnText(cb) { UseAscender = true };
            ctNombre.SetSimpleColumn(contentLeft, nombreBottom, nombreRight, yTop - pad * 0.5f);
            ctNombre.AddElement(new Paragraph(nombre, fontNombre) { Alignment = Element.ALIGN_LEFT, Leading = fontNombre.Size * 1.08f });
            ctNombre.Go();

            if (tam.MostrarLogo && logo != null)
                cb.AddImage(logo);

            // --- Línea divisoria ---
            cb.SetLineWidth(0.75f);
            cb.SetColorStroke(BaseColor.DARK_GRAY);
            cb.MoveTo(contentLeft, headerBottom);
            cb.LineTo(contentRight, headerBottom);
            cb.Stroke();

            // --- Precio grande + etiqueta "precio por kg" / "precio unitario" ---
            float precioH = alto * 0.40f;
            float precioBottom = headerBottom - precioH;

            var precioPhrase = new Phrase();
            precioPhrase.Add(new Chunk("$", fontPrecioSimbolo));
            precioPhrase.Add(new Chunk(string.Format(CultureInfo.InvariantCulture, "{0:#,0.00}", producto.PrecioKg), fontPrecio));
            float precioBaseline = precioBottom + precioH * 0.42f;
            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, precioPhrase, contentRight, precioBaseline, 0);

            string precioLabelTexto = producto.Pesable ? "PRECIO POR KG" : "PRECIO UNITARIO";
            float precioLabelBaseline = precioBottom + precioH * 0.12f;
            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(precioLabelTexto, fontPrecioLabel), contentRight, precioLabelBaseline, 0);

            // --- Pie: código de barras (izquierda) + fecha de emisión (derecha) ---
            float footerTop = precioBottom;
            float footerBottom = yTop - alto + pad;
            float footerH = footerTop - footerBottom;

            try
            {
                var barcode = GenerarImagenBarcode(producto.Codigo, cb, bfBarcode, fuenteBarcodeTexto);
                float bw = contentWidth * (tam.MostrarFecha ? 0.56f : 0.85f);
                float bh = footerH * 0.95f;
                float escala = Math.Min(bw / barcode.Width, bh / barcode.Height);
                barcode.SetAbsolutePosition(contentLeft, footerBottom);
                barcode.ScalePercent(escala * 100f);
                cb.AddImage(barcode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("GenerarPdfEtiquetas - barcode idCorte={0}, codigo={1}: {2}", producto.IdCorte, producto.Codigo, ex);
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("Cód: " + producto.Codigo, fontFechaValor), contentLeft, footerBottom + footerH * 0.4f, 0);
            }

            if (tam.MostrarFecha)
            {
                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase("FECHA DE EMISIÓN", fontFechaLabel), contentRight, footerBottom + footerH * 0.62f, 0);
                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(fechaImpresion, fontFechaValor), contentRight, footerBottom + footerH * 0.30f, 0);
            }
        }

        // Codifica item.Codigo como EAN-13/EAN-8 cuando el dígito verificador da
        // válido (mismo criterio que isValidEAN13/isValidEAN8 en el JS de esta
        // misma vista -- una sola definición de "EAN válido" en todo el proyecto,
        // con padding de ceros a la izquierda porque Codigo se guarda como long y
        // pierde el cero inicial de un EAN real). Si no valida como EAN, cae a
        // Code128 (cualquier largo numérico) -- sigue siendo escaneable, y el POS
        // ya busca por el valor numérico de Codigo, no por el tipo de símbolo.
        private static Image GenerarImagenBarcode(long codigo, PdfContentByte cb, BaseFont bfBarcode, float size)
        {
            string digitos = codigo.ToString(CultureInfo.InvariantCulture);
            string ean13 = digitos.PadLeft(13, '0');
            string ean8 = digitos.PadLeft(8, '0');

            if (digitos.Length <= 13 && EsEan13Valido(ean13))
            {
                var bc = new BarcodeEAN { CodeType = BarcodeEAN.EAN13, Code = ean13, Font = bfBarcode, Size = size };
                return bc.CreateImageWithBarcode(cb, null, null);
            }

            if (digitos.Length <= 8 && EsEan8Valido(ean8))
            {
                var bc = new BarcodeEAN { CodeType = BarcodeEAN.EAN8, Code = ean8, Font = bfBarcode, Size = size };
                return bc.CreateImageWithBarcode(cb, null, null);
            }

            var bc128 = new Barcode128 { Code = digitos, Font = bfBarcode, Size = size };
            return bc128.CreateImageWithBarcode(cb, null, null);
        }

        private static bool EsEan13Valido(string code13)
        {
            if (code13 == null || code13.Length != 13 || !code13.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 12; i++)
            {
                int digito = code13[i] - '0';
                suma += (i % 2 == 0) ? digito : digito * 3;
            }
            int check = (10 - (suma % 10)) % 10;
            return check == (code13[12] - '0');
        }

        private static bool EsEan8Valido(string code8)
        {
            if (code8 == null || code8.Length != 8 || !code8.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 7; i++)
            {
                int digito = code8[i] - '0';
                suma += (i % 2 == 0) ? digito * 3 : digito;
            }
            int check = (10 - (suma % 10)) % 10;
            return check == (code8[7] - '0');
        }

    }
}
