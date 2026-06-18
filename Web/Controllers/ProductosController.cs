using Datos;
using Entidades;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        private Negocio.Sucursal oSucursalN;
        private Negocio.Corte oCorteN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Persona oPersonaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oSucursalN = new Negocio.Sucursal(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
            oUsuarioN = new Negocio.Usuario(empresa, param);
            oPersonaN = new Negocio.Persona(empresa, param);
        }

        public ActionResult Index(
            int SucursalId = 0,
            string tipo = "",
            int marcaId = 0,
            int proveedorId = 0,
            long? codigoDesde = null,
            long? codigoHasta = null)
        {
            int idEmpresaSesion = empresa != null ? empresa.IdEmpresa : 0;
            var productos = (oCorteN.findAllCortes(true, SucursalId) ?? new List<Entidades.Corte>())
                .Where(x => x != null && x.IdEmpresa == idEmpresaSesion)
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

        [HttpGet]
        public ActionResult VerGlobales()
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return new HttpStatusCodeResult(403);

            var model = ConstruirCatalogoGlobalVm("");
            return PartialView("~/Views/Productos/_CatalogoGlobalModal.cshtml", model);
        }

        [HttpGet]
        public JsonResult BuscarGlobales(string q = "")
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para importar productos." }, JsonRequestBehavior.AllowGet);

            var model = ConstruirCatalogoGlobalVm(q);
            string html = RenderPartialViewToString("~/Views/Productos/_CatalogoGlobalRows.cshtml", model.Productos);

            return Json(new
            {
                ok = true,
                html,
                cantidad = model.Productos.Count
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
            var productosGlobales = catalogoGlobal.ObtenerCatalogoGlobal("")
                .Where(x => seleccionados.Any(s => s.IdProductoGlobal == x.IdCorte))
                .ToList();

            if (productosGlobales.Count != seleccionados.Count)
                return Json(new { ok = false, mensaje = "No se pudieron resolver todos los productos seleccionados del catálogo global." });

            var importacionesExistentes = oCorteN.ObtenerImportacionesCatalogoGlobal(productosGlobales.Select(x => x.IdCorte))
                .ToDictionary(x => x.IdProductoGlobal, x => x);

            var productosEmpresaActual = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();
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
            var importacionesMaestros = oCorteN.ObtenerImportacionesCatalogoGlobal(
                productosGlobales
                    .Where(x => x.CorteMaestro != null && x.CorteMaestro.IdCorte > 0)
                    .Select(x => x.CorteMaestro.IdCorte));

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
                    conflictos.Add("El código de destino para el producto global ID " + item.IdProductoGlobal + " debe ser mayor a 0.");
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
                var nuevoProducto = ClonarProductoGlobal(producto, seleccion.CodigoDestino, 0f);

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

                oCorteN.addOrEditCorte(nuevoProducto);

                var insertado = oCorteN.findCorteByCodigo(seleccion.CodigoDestino, false);
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
        public JsonResult BuscarPorCodigoBarraGlobal(string codigoBarra)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para agregar productos." }, JsonRequestBehavior.AllowGet);

            oCorteN.AsegurarTablaImportacionCatalogoGlobal();

            long codigo = NormalizarCodigoBarra(codigoBarra);
            if (codigo <= 0)
                return Json(new { ok = false, mensaje = "Ingrese un código de barra válido." }, JsonRequestBehavior.AllowGet);

            var existenteEmpresa = oCorteN.findCorteByCodigo(codigo, false);
            if (existenteEmpresa != null)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "El código ya existe en la empresa actual para \"" + existenteEmpresa.CorteDesc + "\"."
                }, JsonRequestBehavior.AllowGet);
            }

            var catalogoGlobal = ObtenerGestorCatalogoGlobal();
            var global = catalogoGlobal.findCorteByCodigo(codigo, true);
            if (global == null)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "No existe el producto en el catálogo global."
                }, JsonRequestBehavior.AllowGet);
            }

            var importacionExistente = oCorteN.ObtenerImportacionesCatalogoGlobal(new[] { global.IdCorte }).FirstOrDefault();
            if (importacionExistente != null)
            {
                var productoImportado = oCorteN.findCorteById(importacionExistente.IdProductoEmpresa, false);
                if (productoImportado != null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Ese producto global ya fue importado como \"" + productoImportado.CorteDesc + "\" (código " + productoImportado.Codigo + ")."
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            string mensajeBloqueo = null;
            if (global.CorteMaestro != null && global.CorteMaestro.IdCorte > 0)
            {
                var maestroImportado = oCorteN.ObtenerImportacionesCatalogoGlobal(new[] { global.CorteMaestro.IdCorte }).FirstOrDefault();
                bool maestroValido = maestroImportado != null && oCorteN.findCorteById(maestroImportado.IdProductoEmpresa, false) != null;
                if (!maestroValido)
                {
                    mensajeBloqueo = "Para agregar " + global.CorteDesc + " primero debe importar " + global.CorteMaestro.CorteDesc + " desde el catálogo global.";
                }
            }

            return Json(new
            {
                ok = true,
                producto = new
                {
                    id = global.IdCorte,
                    codigo = global.Codigo,
                    descripcion = global.CorteDesc,
                    mensajeBloqueo = mensajeBloqueo
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AgregarDesdeCodigoBarra(AgregarProductoDesdeCodigoBarraVm model)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
                return Json(new { ok = false, mensaje = "No tenés permisos para agregar productos." });

            oCorteN.AsegurarTablaImportacionCatalogoGlobal();

            long codigo = NormalizarCodigoBarra(model != null ? model.CodigoBarra : null);
            if (codigo <= 0)
                return Json(new { ok = false, mensaje = "Ingrese un código de barra válido." });

            if (oCorteN.findCorteByCodigo(codigo, false) != null)
                return Json(new { ok = false, mensaje = "El código ya existe en la empresa actual." });

            string descripcion = (model != null ? model.Descripcion : null) ?? "";
            descripcion = descripcion.Trim();
            if (string.IsNullOrWhiteSpace(descripcion))
                return Json(new { ok = false, mensaje = "La descripción no puede estar vacía." });

            float precio;
            if (!TryParseFloatFlexible(model != null ? model.Precio : null, out precio) || precio < 0)
                return Json(new { ok = false, mensaje = "El precio debe ser mayor o igual a 0." });

            var catalogoGlobal = ObtenerGestorCatalogoGlobal();
            var global = catalogoGlobal.findCorteByCodigo(codigo, true);
            if (global == null)
                return Json(new { ok = false, mensaje = "No existe el producto en el catálogo global." });

            var importacionExistente = oCorteN.ObtenerImportacionesCatalogoGlobal(new[] { global.IdCorte }).FirstOrDefault();
            if (importacionExistente != null)
            {
                var productoImportado = oCorteN.findCorteById(importacionExistente.IdProductoEmpresa, false);
                if (productoImportado != null)
                    return Json(new { ok = false, mensaje = "Ese producto global ya fue importado previamente en esta empresa." });
            }

            int? idMaestroEmpresa = null;
            if (global.CorteMaestro != null && global.CorteMaestro.IdCorte > 0)
            {
                var maestroImportado = oCorteN.ObtenerImportacionesCatalogoGlobal(new[] { global.CorteMaestro.IdCorte }).FirstOrDefault();
                if (maestroImportado == null)
                    return Json(new { ok = false, mensaje = "Para agregar " + global.CorteDesc + " primero debe importar " + global.CorteMaestro.CorteDesc + " desde el catálogo global." });

                var maestroEmpresa = oCorteN.findCorteById(maestroImportado.IdProductoEmpresa, false);
                if (maestroEmpresa == null)
                    return Json(new { ok = false, mensaje = "No se encontró el producto maestro ya importado para completar la relación." });

                idMaestroEmpresa = maestroEmpresa.IdCorte;
            }

            var usuario = Session["Usuario"] as Entidades.Usuario;
            var nuevoProducto = ClonarProductoGlobal(global, codigo, precio);
            nuevoProducto.CorteDesc = descripcion;
            if (idMaestroEmpresa.HasValue)
                nuevoProducto.CorteMaestro = new Entidades.Corte { IdCorte = idMaestroEmpresa.Value };

            oCorteN.addOrEditCorte(nuevoProducto);

            var insertado = oCorteN.findCorteByCodigo(codigo, false);
            if (insertado == null || insertado.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "El producto se guardó pero no se pudo recuperar el identificador generado." });

            oCorteN.GuardarImportacionCatalogoGlobal(global.IdCorte, insertado.IdCorte, usuario != null ? (int?)usuario.Id : null);

            return Json(new
            {
                ok = true,
                mensaje = "Producto guardado correctamente"
            });
        }

        // Acción para búsqueda en vivo usada por el modal POS
        [HttpGet]
        public JsonResult ListarProductos(string q = "")
        {
            try
            {
                var productos = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();

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

        private Negocio.Corte ObtenerGestorCatalogoGlobal()
        {
            return new Negocio.Corte(new EmpresaContextNulo(), null);
        }

        private CatalogoGlobalProductosVm ConstruirCatalogoGlobalVm(string busqueda)
        {
            oCorteN.AsegurarTablaImportacionCatalogoGlobal();

            var catalogoGlobal = ObtenerGestorCatalogoGlobal();
            var productosGlobales = catalogoGlobal.ObtenerCatalogoGlobal(busqueda) ?? new List<Entidades.Corte>();
            var productosEmpresaActual = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();
            var codigosEmpresa = new HashSet<long>(productosEmpresaActual.Select(x => x.Codigo));
            var productosEmpresaPorId = productosEmpresaActual.ToDictionary(x => x.IdCorte, x => x);
            var importaciones = oCorteN.ObtenerImportacionesCatalogoGlobal(productosGlobales.Select(x => x.IdCorte))
                .ToDictionary(x => x.IdProductoGlobal, x => x);

            var model = new CatalogoGlobalProductosVm
            {
                Busqueda = busqueda ?? ""
            };

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
                    mensajeEstado = "Código existente. Sugerido: " + codigoSugerido;
                }
                else
                {
                    mensajeEstado = "Listo para importar";
                }

                model.Productos.Add(new ProductoGlobalImportItemVm
                {
                    IdProductoGlobal = producto.IdCorte,
                    CodigoOriginal = producto.Codigo,
                    CodigoDestino = codigoSugerido,
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

        private static List<Entidades.Corte> OrdenarProductosParaImportacion(IEnumerable<Entidades.Corte> productos)
        {
            var lista = (productos ?? new List<Entidades.Corte>()).ToList();
            var dict = lista.ToDictionary(x => x.IdCorte, x => x);
            var resultado = new List<Entidades.Corte>();
            var visitados = new HashSet<int>();

            Action<Entidades.Corte> visitar = null;
            visitar = producto =>
            {
                if (producto == null || !visitados.Add(producto.IdCorte))
                    return;

                if (producto.CorteMaestro != null && producto.CorteMaestro.IdCorte > 0)
                {
                    Entidades.Corte maestro;
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
                Nivel = global.Nivel
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
        public ActionResult AddOrEdit(int id = 0)
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
            LoadCombos(vm);

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

            // 2) Validación server-side de código duplicado (seguridad extra)
            if (vm.Codigo > 0)
            {
                var existente = oCorteN.findCorteByCodigo(vm.Codigo, false);
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

            var entity = (vm.IdCorte > 0)
                ? oCorteN.findCorteById(vm.IdCorte, true)
                : new Entidades.Corte();

            if (vm.IdCorte > 0 && entity == null)
                return HttpNotFound();

            // Si querés guardar el porcentaje (sin columna), lo saco del texto:
            vm.AlicuotaIva = ObtenerAlicuotaPorcentajeDesdeDT(vm.IdAlicuotaIva);

            MapToEntity(vm, entity); // VM -> Entity

            oCorteN.addOrEditCorte(entity);

            TempData["FlashSuccess"] = $"El producto \"{vm.CorteDesc}\" guardó correctamente.";
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

            var corte = oCorteN.findCorteByCodigo(codigo.Value, false);

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

            DataTable dt = oCorteN.obtenerTiposProductoGrilla((buscar ?? "").Trim()) ?? new DataTable();
            return View(dt);
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

            DataTable dt = oCorteN.obtenerTiposProductoGrilla("") ?? new DataTable();
            return dt.AsEnumerable().FirstOrDefault(row =>
                string.Equals(Convert.ToString(row["tipo"]) ?? "", tipo, StringComparison.OrdinalIgnoreCase));
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
            oCorteN.eliminarCorte(entity);

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

    }
}
