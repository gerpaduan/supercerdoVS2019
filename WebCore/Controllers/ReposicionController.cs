// "Qué pedir" (reposición sugerida): qué productos hay que pedir por sucursal para volver al punto de stock, y cuáles
// están en cero con movimiento reciente. Solo lectura: no crea pedidos ni modifica stock.
// Las reglas viven en Negocio.ReposicionStock / ReposicionStockCalculador; acá solo se validan los filtros que llegan de
// la pantalla, se controla el permiso (Stock.VerStock, igual que Reportes) y se arma la vista, la tabla parcial y el Excel.
// Solo Postgres (NegocioFactory.CrearReposicionStock). Detalle: docs/11-manual-de-uso/pantallas/Reposicion.Index.md.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utilidades;

namespace WebCore.Controllers
{
    public class ReposicionController : Controller
    {
        private const string VistaTabla = "~/Views/Reposicion/_Tabla.cshtml";
        private const string MensajeSoloPostgres = "Esta pantalla solo está disponible cuando el sistema usa Postgres.";

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Usuario _oUsuarioN;
        // null si el motor de datos no es Postgres: la pantalla se muestra con un aviso y sin poder buscar.
        private readonly Negocio.ReposicionStock? _oReposicionN;
        private readonly ILogger<ReposicionController> _log;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public ReposicionController(WebCore.Services.IUsuarioSesionService sesion, ILogger<ReposicionController> log)
        {
            _sesion = sesion;
            _log = log;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oReposicionN = WebCore.Infrastructure.NegocioFactory.UsarPostgres
                ? WebCore.Infrastructure.NegocioFactory.CrearReposicionStock(_empresa, _param)
                : null;
        }

        // Pantalla. popup=true: ventana independiente sin menús (mismo patrón que Existencia por sucursales).
        [HttpGet]
        public IActionResult Index(bool popup = false)
        {
            var user = _usuarioActual;
            if (!TienePermiso(user))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            var model = new Entidades.ReposicionVm();
            CargarListasDeFiltros(model);
            // Por defecto se tilda solo la sucursal del usuario (si no tiene, quedan todas).
            if (user != null && user.IdSucursal > 0)
                model.Filtro.IdsSucursales = new List<int> { user.IdSucursal };
            model.Mensaje = _oReposicionN == null ? MensajeSoloPostgres : "";

            ViewBag.Title = "Qué pedir";
            ViewBag.Seccion = "Stock";
            ViewBag.Popup = popup;
            ViewBag.Disponible = _oReposicionN != null;
            return View("~/Views/Reposicion/Index.cshtml", model);
        }

        // Tabla de resultados (HTML parcial) para el Buscar por AJAX.
        [HttpGet]
        public IActionResult Buscar(
            int[] idsSucursales = null, string texto = "", string tipo = "", int idProveedor = 0, int idMarca = 0,
            int diasVentana = 30, int diasCobertura = 0, bool mostrarSinMovimiento = false)
        {
            if (!TienePermiso(_usuarioActual))
                return StatusCode(403, "Sin permiso para ver el stock.");

            var model = Consultar(idsSucursales, texto, tipo, idProveedor, idMarca, diasVentana, diasCobertura, mostrarSinMovimiento);
            return PartialView(VistaTabla, model);
        }

        // Mismo resultado que la pantalla, en .xlsx (los filtros llegan iguales).
        [HttpGet]
        public IActionResult ExportarExcel(
            int[] idsSucursales = null, string texto = "", string tipo = "", int idProveedor = 0, int idMarca = 0,
            int diasVentana = 30, int diasCobertura = 0, bool mostrarSinMovimiento = false)
        {
            if (!TienePermiso(_usuarioActual))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }
            if (_oReposicionN == null)
                return BadRequest(MensajeSoloPostgres);

            var model = Consultar(idsSucursales, texto, tipo, idProveedor, idMarca, diasVentana, diasCobertura, mostrarSinMovimiento);
            if (!string.IsNullOrEmpty(model.Mensaje) && !model.ConsultaRealizada)
                return BadRequest(model.Mensaje);

            string sucursalesTexto = string.Join(", ", model.SucursalesDisponibles
                .Where(s => model.Filtro.IdsSucursales.Count == 0 || model.Filtro.IdsSucursales.Contains(s.IdSucursal))
                .Select(s => s.Sucursal));

            byte[] excel = WebCore.Services.ExcelReposicion.Generar(model, sucursalesTexto);
            string nombreArchivo = "Que_pedir_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }

        // ---- Sugerir puntos de stock ----

        // Pantalla "Sugerir puntos de stock": para los productos sin punto cargado, propone uno según el consumo reciente y deja
        // cargar los que el usuario tilde. Ver es Stock.VerStock; guardar pide además el permiso de editar productos.
        [HttpGet]
        public IActionResult Puntos(bool popup = false)
        {
            var user = _usuarioActual;
            if (!TienePermiso(user))
            {
                ViewBag.Seccion = "Stock";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            var model = new Entidades.PuntosSugeridosVm();
            var listas = new Entidades.ReposicionVm();
            CargarListasDeFiltros(listas);
            CopiarListas(listas, model);
            if (user != null && user.IdSucursal > 0)
                model.Filtro.IdsSucursales = new List<int> { user.IdSucursal };
            model.PuedeGuardar = TienePermisoEditarPuntos(user);
            model.Mensaje = _oReposicionN == null ? MensajeSoloPostgres : "";

            ViewBag.Title = "Sugerir puntos de stock";
            ViewBag.Seccion = "Stock";
            ViewBag.Popup = popup;
            ViewBag.Disponible = _oReposicionN != null;
            return View("~/Views/Reposicion/Puntos.cshtml", model);
        }

        // Tabla de sugerencias (HTML parcial) para el Buscar por AJAX.
        [HttpGet]
        public IActionResult BuscarPuntos(
            int[] idsSucursales = null, string texto = "", string tipo = "", int idProveedor = 0, int idMarca = 0,
            int diasVentana = 30, int diasCoberturaPunto = 7, bool incluirConPunto = false, bool mostrarSinMovimiento = false)
        {
            var user = _usuarioActual;
            if (!TienePermiso(user))
                return StatusCode(403, "Sin permiso para ver el stock.");

            var model = new Entidades.PuntosSugeridosVm();
            var listas = new Entidades.ReposicionVm();
            CargarListasDeFiltros(listas);
            CopiarListas(listas, model);
            model.PuedeGuardar = TienePermisoEditarPuntos(user);

            if (_oReposicionN == null)
            {
                model.Mensaje = MensajeSoloPostgres;
                return PartialView("~/Views/Reposicion/_TablaPuntos.cshtml", model);
            }

            try
            {
                var filtro = new Entidades.ReposicionFiltroVm
                {
                    IdsSucursales = IdsSucursalesValidos(idsSucursales, model.SucursalesDisponibles),
                    Texto = texto,
                    Tipo = tipo,
                    IdProveedor = idProveedor,
                    IdMarca = idMarca,
                    DiasVentana = diasVentana,
                    DiasCoberturaPunto = diasCoberturaPunto,
                    IncluirConPunto = incluirConPunto,
                    MostrarSinMovimiento = mostrarSinMovimiento
                };

                var resultado = _oReposicionN.ArmarPuntosSugeridos(filtro);
                CopiarListas(listas, resultado);
                resultado.PuedeGuardar = model.PuedeGuardar;
                return PartialView("~/Views/Reposicion/_TablaPuntos.cshtml", resultado);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sugerir puntos de stock: falló la consulta (empresa {IdEmpresa}).", _empresa.IdEmpresa);
                model.Mensaje = "No se pudieron calcular las sugerencias. Probá de nuevo; si sigue fallando, avisá a soporte.";
                return PartialView("~/Views/Reposicion/_TablaPuntos.cshtml", model);
            }
        }

        // Guarda los puntos que el usuario tildó. Nunca pisa un punto ya cargado (lo revalida Negocio contra el stock real).
        // Devuelve { ok, guardados, rechazados[] } o { ok:false, error }.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPuntos([FromBody] WebCore.Models.GuardarPuntosSugeridosRequest request)
        {
            var user = _usuarioActual;
            // Mismo permiso que el guardado de puntos de Productos (ProductosController.GuardarPuntosStockSucursal).
            if (!TienePermisoEditarPuntos(user))
                return Json(new { ok = false, error = "No tenés permisos para editar el punto de stock." });
            if (_oReposicionN == null)
                return Json(new { ok = false, error = MensajeSoloPostgres });

            var valores = request == null ? null : request.Valores;
            if (valores == null || valores.Count == 0)
                return Json(new { ok = false, error = "No hay puntos de stock para guardar." });

            try
            {
                var resultado = _oReposicionN.GuardarPuntosSugeridos(valores, _empresa.IdEmpresa);
                if (resultado.Error != null)
                {
                    _log.LogError(resultado.Error, "Sugerir puntos de stock: falló el guardado (empresa {IdEmpresa}, usuario {IdUsuario}); guardados antes del error: {Guardados}.",
                        _empresa.IdEmpresa, user == null ? 0 : user.Id, resultado.Guardados);
                    return Json(new
                    {
                        ok = false,
                        error = "No se pudieron guardar todos los puntos de stock. Lo que ya se guardó queda guardado: volvé a buscar para ver cómo quedó.",
                        guardados = resultado.Guardados,
                        rechazados = resultado.Rechazados
                    });
                }

                // Queda constancia de quién cargó puntos (no hay otra auditoría de este dato).
                _log.LogInformation("Sugerir puntos de stock: usuario {IdUsuario} cargó {Guardados} puntos (empresa {IdEmpresa}); rechazados: {Rechazados}.",
                    user == null ? 0 : user.Id, resultado.Guardados, _empresa.IdEmpresa, resultado.Rechazados.Count);
                return Json(new { ok = resultado.Guardados > 0, guardados = resultado.Guardados, rechazados = resultado.Rechazados,
                                  error = resultado.Guardados > 0 ? "" : "No se guardó ningún punto de stock." });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sugerir puntos de stock: error inesperado al guardar (empresa {IdEmpresa}).", _empresa.IdEmpresa);
                return Json(new { ok = false, error = "No se pudieron guardar los puntos de stock. Intentá de nuevo." });
            }
        }

        // ---- internos ----

        // Mismo permiso que Reportes (Stock.VerStock); la pantalla solo lee.
        private bool TienePermiso(Entidades.Usuario? user)
        {
            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Stock.VerStock, DateTime.Today, -1);
        }

        // Cargar puntos de stock: el mismo permiso que ya exige Productos para editarlos.
        private bool TienePermisoEditarPuntos(Entidades.Usuario? user)
        {
            return _oUsuarioN.tienePermiso(user, Entidades.Permisos.Producto.NuevoCorte, DateTime.Today, -1);
        }

        // Solo ids de sucursales de esta empresa. Todas tildadas o ninguna = "todas" (lista vacía).
        private static List<int> IdsSucursalesValidos(int[] idsSucursales, List<Entidades.SucursalColumnaStockVm> disponibles)
        {
            var idsValidos = (idsSucursales ?? new int[0])
                .Where(id => disponibles.Any(s => s.IdSucursal == id))
                .Distinct()
                .ToList();
            if (idsValidos.Count >= disponibles.Count) idsValidos.Clear();
            return idsValidos;
        }

        private static void CopiarListas(Entidades.ReposicionVm origen, Entidades.PuntosSugeridosVm destino)
        {
            destino.SucursalesDisponibles = origen.SucursalesDisponibles;
            destino.TiposDisponibles = origen.TiposDisponibles;
            destino.ProveedoresDisponibles = origen.ProveedoresDisponibles;
            destino.MarcasDisponibles = origen.MarcasDisponibles;
        }

        // Valida el input de la pantalla (ids de sucursal que no sean de esta empresa, textos) y llama a Negocio.
        // Nunca lanza: ante un error de datos devuelve un modelo con el mensaje (y lo registra en el log).
        private Entidades.ReposicionVm Consultar(
            int[] idsSucursales, string texto, string tipo, int idProveedor, int idMarca,
            int diasVentana, int diasCobertura, bool mostrarSinMovimiento)
        {
            var model = new Entidades.ReposicionVm();
            CargarListasDeFiltros(model);

            if (_oReposicionN == null)
            {
                model.Mensaje = MensajeSoloPostgres;
                return model;
            }

            try
            {
                var filtro = new Entidades.ReposicionFiltroVm
                {
                    IdsSucursales = IdsSucursalesValidos(idsSucursales, model.SucursalesDisponibles),
                    Texto = texto,
                    Tipo = tipo,
                    IdProveedor = idProveedor,
                    IdMarca = idMarca,
                    DiasVentana = diasVentana,
                    DiasCoberturaSinPunto = diasCobertura,
                    MostrarSinMovimiento = mostrarSinMovimiento
                };

                var resultado = _oReposicionN.Armar(filtro);
                resultado.SucursalesDisponibles = model.SucursalesDisponibles;
                resultado.TiposDisponibles = model.TiposDisponibles;
                resultado.ProveedoresDisponibles = model.ProveedoresDisponibles;
                resultado.MarcasDisponibles = model.MarcasDisponibles;
                return resultado;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Qué pedir: falló la consulta (empresa {IdEmpresa}).", _empresa.IdEmpresa);
                model.Mensaje = "No se pudo calcular qué pedir. Probá de nuevo; si sigue fallando, avisá a soporte.";
                return model;
            }
        }

        // Listas de los filtros (sucursales, tipos, proveedores, marcas). Mismo criterio que
        // StockController.Obtener*Existencia: si una lista falla, el filtro queda vacío en vez de romper la pantalla.
        private void CargarListasDeFiltros(Entidades.ReposicionVm model)
        {
            model.SucursalesDisponibles = (_oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
                .Where(s => s != null && s.IdSucursal > 0)
                .GroupBy(s => s.IdSucursal)
                .Select(g => g.First())
                .OrderBy(s => s.SucursalNombre)
                .Select(s => new Entidades.SucursalColumnaStockVm { IdSucursal = s.IdSucursal, Sucursal = s.SucursalNombre })
                .ToList();

            model.TiposDisponibles = LeerLista(() =>
            {
                var tipos = new List<string>();
                foreach (DataRow row in (_oCorteN.obtenerTiposProductoGrilla("") ?? new DataTable()).Rows)
                {
                    string t = row["tipo"] == DBNull.Value ? "" : Convert.ToString(row["tipo"]);
                    if (!string.IsNullOrWhiteSpace(t) && !tipos.Any(x => string.Equals(x, t, StringComparison.OrdinalIgnoreCase)))
                        tipos.Add(t);
                }
                return tipos.OrderBy(x => x).ToList();
            });

            model.ProveedoresDisponibles = LeerLista(() =>
                PersonasDeTabla(_oPersonaN.buscarProveedor("") ?? new DataTable(), "razonSocial", "Proveedor"));

            model.MarcasDisponibles = LeerLista(() =>
                PersonasDeTabla(_oPersonaN.buscarPersona("", true) ?? new DataTable(), "Marca", "razonSocial"));
        }

        private List<T> LeerLista<T>(Func<List<T>> leer)
        {
            try
            {
                return leer();
            }
            catch (Exception ex)
            {
                // Un filtro sin opciones no debe impedir usar la pantalla; se deja constancia en el log.
                _log.LogWarning(ex, "Qué pedir: no se pudo cargar una lista de filtros.");
                return new List<T>();
            }
        }

        // idPersona + nombre (primera columna con valor entre las candidatas) -> Persona, sin repetidos.
        private static List<Entidades.Persona> PersonasDeTabla(DataTable dt, params string[] columnasNombre)
        {
            var personas = new List<Entidades.Persona>();
            foreach (DataRow row in dt.Rows)
            {
                int id = dt.Columns.Contains("idPersona") && row["idPersona"] != DBNull.Value ? Convert.ToInt32(row["idPersona"]) : 0;
                string nombre = "";
                foreach (var col in columnasNombre)
                {
                    if (dt.Columns.Contains(col) && row[col] != DBNull.Value)
                    {
                        nombre = Convert.ToString(row[col]);
                        break;
                    }
                }
                if (id <= 0 || string.IsNullOrWhiteSpace(nombre) || personas.Any(x => x.IdPersona == id)) continue;
                personas.Add(new Entidades.Persona { IdPersona = id, RazonSocial = nombre });
            }
            return personas.OrderBy(x => x.RazonSocial).ToList();
        }
    }
}
