using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;
using Utilidades;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class ComprasController : BaseController
    {
        private const long CuitHabilitaMediaRes = 20306210786;

        private Negocio.Compra oCompraN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Persona oPersonaN;
        private Negocio.Corte oCorteN;
        private Negocio.CierreCaja oCierreN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oCompraN = Web.Infrastructure.NegocioFactory.CrearCompra(empresa, param);
            oSucursalN = Web.Infrastructure.NegocioFactory.CrearSucursal(empresa, param);
            oUsuarioN = Web.Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            oPersonaN = Web.Infrastructure.NegocioFactory.CrearPersona(empresa, param);
            oCorteN = Web.Infrastructure.NegocioFactory.CrearCorte(empresa, param);
            oCierreN = Web.Infrastructure.NegocioFactory.CrearCierreCaja(empresa, param);
        }

        public ActionResult Index(int idSucursal = -1, string tipoCompra = "Todos", string texto = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Compra.VerCompras, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Compra.VerCompras, ref desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Compras", Permisos.Compra.VerCompras, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            }

            bool permiteMediaRes = PermiteMediaRes(user);
            string tipoFiltrado = string.IsNullOrWhiteSpace(tipoCompra) ? "Todos" : tipoCompra.Trim();
            if (!permiteMediaRes &&
                string.Equals(tipoFiltrado, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes), StringComparison.OrdinalIgnoreCase))
            {
                tipoFiltrado = "Todos";
            }

            DataTable dt = oCompraN.obtenerCompras(idSucursal, tipoFiltrado, texto ?? "", desde, hasta, null) ?? new DataTable();
            var model = new CompraIndexVm
            {
                Compras = dt
            };

            ViewBag.Title = "Compras";
            ViewBag.Seccion = "Compras";
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.IdSucursal = idSucursal;
            ViewBag.TipoCompra = tipoFiltrado;
            ViewBag.Texto = texto ?? "";
            ViewBag.FechaDesde = desde;
            ViewBag.FechaHasta = hasta;
            ConfigurarAdvertenciaFechaEnVivo("fechaDesde", Permisos.Compra.VerCompras, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            ViewBag.PermiteMediaRes = permiteMediaRes;
            ViewBag.TotalCantMedias = CalcularTotalCantMedias(dt);
            ViewBag.TotalKg = CalcularTotalKg(dt);
            ViewBag.TotalS = CalcularTotalImporte(dt);

            return View("~/Views/Compras/Index.cshtml", model);
        }

        public ActionResult Lineas(int idSucursal = -1, string tipoCompra = "Todos", string texto = "", string producto = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Compra.VerCompras, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Compra.VerCompras, ref desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Compras", Permisos.Compra.VerCompras, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            }

            bool permiteMediaRes = PermiteMediaRes(user);
            string tipoFiltrado = string.IsNullOrWhiteSpace(tipoCompra) ? "Todos" : tipoCompra.Trim();
            if (!permiteMediaRes &&
                string.Equals(tipoFiltrado, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes), StringComparison.OrdinalIgnoreCase))
            {
                tipoFiltrado = "Todos";
            }

            DataTable dt = oCompraN.obtenerCompras(idSucursal, tipoFiltrado, texto ?? "", desde.Date, hasta.Date, null) ?? new DataTable();
            var model = new CompraLineasIndexVm
            {
                IdSucursal = idSucursal,
                TipoCompra = tipoFiltrado,
                Texto = texto ?? "",
                Producto = producto ?? "",
                FechaDesde = desde,
                FechaHasta = hasta,
                PermiteMediaRes = permiteMediaRes
            };

            foreach (DataRow row in dt.Rows)
            {
                int idCompra = row["idCompra"] == DBNull.Value ? 0 : Convert.ToInt32(row["idCompra"]);
                if (idCompra <= 0 || model.Compras.Any(x => x.IdCompra == idCompra))
                    continue;

                Entidades.Compra compra = oCompraN.findById_convertToCompra(idCompra);
                if (compra == null || compra.IdCompra == 0)
                    continue;

                var lineas = ConstruirLineasCompra(compra);
                if (!string.IsNullOrWhiteSpace(producto))
                    lineas = lineas.Where(x => CoincideProducto(x.Codigo, x.Producto, producto)).ToList();

                if (lineas.Count == 0)
                    continue;

                float total = row.Table.Columns.Contains("totalS") && row["totalS"] != DBNull.Value
                    ? Convert.ToSingle(row["totalS"])
                    : compra.getImporteCompra(compra, compra.LineasMediasReses, compra.LineasCortes);

                var grupo = new CompraLineasGrupoVm
                {
                    IdCompra = compra.IdCompra,
                    CollapseId = "compraLineas_" + compra.IdCompra,
                    Titulo = "COMPRA ID: " + compra.IdCompra,
                    Subtitulo = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm"),
                    ResumenCompacto = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm"),
                    ResumenSecundario = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "-",
                    TotalTexto = total.ToString("N2"),
                    TotalImporte = Convert.ToDecimal(total),
                    TotalKg = lineas.Sum(x => x.CantidadKg),
                    EditUrl = Url.Action("Editar", "Compras", new { id = compra.IdCompra, origen = "layout" })
                };

                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Fecha", Valor = compra.FechaCompra.ToString("dd/MM/yyyy HH:mm") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Proveedor", Valor = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal", Valor = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Tipo de compra", Valor = string.IsNullOrWhiteSpace(compra.TipoCompra) ? "-" : compra.TipoCompra });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Nro/comprobante", Valor = string.IsNullOrWhiteSpace(compra.NroRemito) ? "-" : compra.NroRemito });

                grupo.Lineas.AddRange(lineas);
                model.Compras.Add(grupo);
            }

            ViewBag.Title = "Lineas de compra";
            ViewBag.Seccion = "Compras";
            ViewBag.Sucursales = oSucursalN.findAll();

            return View("~/Views/Compras/Lineas.cshtml", model);
        }

        public ActionResult Editar(int id = 0, string origen = "layout")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            string origenNormalizado = NormalizarOrigen(origen);
            bool desdePos = EsOrigenPos(origenNormalizado);

            string permiso = id > 0 ? Permisos.Compra.ModificarCompra : Permisos.Compra.NuevaCompra;
            int idCreador = user.Id;
            Entidades.Compra compra = null;

            if (id > 0)
            {
                compra = oCompraN.findById_convertToCompra(id);
                if (compra == null || compra.IdCompra == 0)
                    return HttpNotFound("No se encontró la compra solicitada.");

                idCreador = compra.CreadoPor != null ? compra.CreadoPor.Id : user.Id;
            }

            DateTime fechaPermiso = compra != null ? compra.FechaCompra : DateTime.Today;
            bool puedeEditar = PermisosHelper.TienePermiso(Session, permiso, fechaPermiso, idCreador);
            if (!puedeEditar)
            {
                if (desdePos)
                    return new HttpStatusCodeResult(403, "No tiene permisos para operar compras.");

                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(permiso, fechaPermiso, idCreador) ?? "No tiene permisos para operar compras.";
                return RedirectToAction("Index");
            }

            var model = compra != null
                ? CrearViewModelEdicion(compra, user, origenNormalizado)
                : CrearViewModelNuevo(user, origenNormalizado);

            model.SubmissionToken = Guid.NewGuid().ToString("N");
            CargarViewBags(model, user);
            ConfigurarAdvertenciaFechaEnVivo("FechaCompra", permiso, idCreador);

            if (desdePos)
                return PartialView("~/Views/Compras/Editar.cshtml", model);

            return View("~/Views/Compras/Editar.cshtml", model);
        }

        public ActionResult NuevaCompra(string origen = "layout")
        {
            return RedirectToAction("Editar", new { id = 0, origen = origen });
        }

        public ActionResult ModificarCompra(int id, string origen = "layout")
        {
            return RedirectToAction("Editar", new { id = id, origen = origen });
        }

        [HttpGet]
        public JsonResult BuscarCorte(string q = "")
        {
            try
            {
                int idEmpresaSesion = (Session["Usuario"] as Entidades.Usuario) != null
                    ? ((Session["Usuario"] as Entidades.Usuario).IdEmpresa)
                    : (empresa != null ? empresa.IdEmpresa : 0);
                var productos = idEmpresaSesion > 0
                    ? (oCorteN.ObtenerCortesPorEmpresa(idEmpresaSesion, false) ?? new List<Entidades.Corte>())
                    : (oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>());
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
                    precio = p.PrecioKg
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

            int idEmpresaSesion = (Session["Usuario"] as Entidades.Usuario) != null
                ? ((Session["Usuario"] as Entidades.Usuario).IdEmpresa)
                : (empresa != null ? empresa.IdEmpresa : 0);
            var corte = idEmpresaSesion > 0
                ? oCorteN.findCorteByCodigoEmpresa(codigo.Value, idEmpresaSesion, false)
                : oCorteN.findCorteByCodigo(codigo.Value, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el producto." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ok = true,
                id = corte.IdCorte,
                codigo = corte.Codigo,
                nombre = corte.CorteDesc,
                precio = corte.PrecioKg
            }, JsonRequestBehavior.AllowGet);
        }

        // Guarda contra doble-submit real: se vio en produccion (datos reales del usuario, no
        // solo el guard de boton) que la MISMA compra en cuenta corriente se registraba dos
        // veces con ~15s de diferencia -- demasiado consistente entre varios incidentes
        // separados para ser variacion humana de doble-click. No se pudo confirmar con certeza
        // el disparador exacto del lado cliente (compras.js no usa POSGuard para el boton
        // Guardar; MODAL_LOAD_STALE_MS=15000 en pos-guard.js es sospechoso por la coincidencia
        // de tiempo pero gobierna la apertura del modal, no el guardado) -- en vez de perseguir
        // mas la causa exacta del lado cliente, se cierra la puerta del lado servidor de forma
        // definitiva: SubmissionToken (CompraEditVm) se genera una sola vez por carga de pagina
        // (GET Editar) y viaja en el form. Cualquier reintento de ESTE mismo formulario -- sin
        // importar cuanto tiempo pase entre uno y otro, cubriendo tanto el doble-click rapido
        // como el caso real de ~15s -- comparte el mismo token y queda bloqueado. Un formulario
        // nuevo (F5, o "Nueva Compra" de nuevo) trae un token distinto, asi que nunca bloquea
        // una compra legitima distinta. MemoryCache.Add es atomico (a diferencia de Get+Set):
        // no hay ventana de carrera entre chequear y marcar el lock en si. TTL largo (30 min)
        // porque el token es de un solo uso -- no hace falta liberarlo pronto para no molestar
        // a otro guardado, ya que ese otro guardado siempre trae un token diferente.
        private static string ClaveLockGuardarCompra(string sessionId, string submissionToken)
        {
            return !string.IsNullOrWhiteSpace(submissionToken)
                ? "ComprasGuardarLock:token:" + submissionToken.Trim()
                : "ComprasGuardarLock:session:" + (sessionId ?? "");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Guardar(CompraEditVm model)
        {
            string claveLock = ClaveLockGuardarCompra(
                Session != null ? Session.SessionID : null,
                model != null ? model.SubmissionToken : null);
            if (!MemoryCache.Default.Add(claveLock, true, DateTimeOffset.UtcNow.AddMinutes(30)))
            {
                return Json(new { ok = false, mensaje = "Esta compra ya se guardó (o se está guardando). Si necesitás registrar otra, volvé a abrir el formulario." });
            }

            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida." });

                string origenNormalizado = NormalizarOrigen(model != null ? model.Origen : null);
                bool desdePos = EsOrigenPos(origenNormalizado);

                if (model == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos de la compra." });

                if (desdePos)
                {
                    model.IdSucursal = user.IdSucursal;
                }

                string errorValidacion = ValidarModelo(model, user, desdePos);
                if (!string.IsNullOrWhiteSpace(errorValidacion))
                    return Json(new { ok = false, mensaje = errorValidacion });

                Entidades.Compra compraActual = null;
                if (model.IdCompra > 0)
                {
                    compraActual = oCompraN.findById_convertToCompra(model.IdCompra);
                    if (compraActual == null || compraActual.IdCompra == 0)
                        return Json(new { ok = false, mensaje = "No se encontró la compra a modificar." });
                }

                string permiso = model.IdCompra > 0 ? Permisos.Compra.ModificarCompra : Permisos.Compra.NuevaCompra;
                DateTime fechaPermiso = model.FechaCompra;
                int idCreador = compraActual != null && compraActual.CreadoPor != null ? compraActual.CreadoPor.Id : user.Id;
                if (!PermisosHelper.TienePermiso(Session, permiso, fechaPermiso, idCreador))
                    return Json(new { ok = false, mensaje = "No tiene permisos para guardar esta compra." });

                Entidades.Persona proveedor = oPersonaN.findById(model.IdProveedor);
                if (proveedor == null || proveedor.IdPersona <= 0)
                    return Json(new { ok = false, mensaje = "Seleccione un proveedor válido." });

                Entidades.Sucursal sucursal = oSucursalN.findById(model.IdSucursal);
                if (sucursal == null || sucursal.IdSucursal <= 0)
                    return Json(new { ok = false, mensaje = "Seleccione una sucursal válida." });

                var compra = compraActual ?? new Entidades.Compra();
                compra.IdCompra = model.IdCompra;
                compra.NroRemito = (model.NroRemito ?? string.Empty).Trim();
                compra.FechaCompra = model.FechaCompra;
                compra.Proveedor = proveedor;
                compra.TipoCompra = model.TipoCompra;
                compra.CantMedias = model.CantMedias;
                compra.KgsMedias = model.KgsMedias;
                compra.Observaciones = (model.Observaciones ?? string.Empty).Trim();
                compra.Sucursal = sucursal;
                compra.EnCtaCte = model.EnCtaCte;
                compra.Estado = compraActual != null ? compraActual.Estado ?? "" : "";
                compra.CreadoPor = compraActual != null ? compraActual.CreadoPor : user;
                compra.ActualizadoPor = compraActual != null ? user : null;

                var lineasMediaRes = new List<Entidades.MediaRes>();
                var lineasCortes = new List<Entidades.CortePorCompra>();

                int index = 0;
                foreach (var linea in model.Lineas ?? new List<CompraLineaVm>())
                {
                    index++;

                    if (EsTipoMediaRes(model.TipoCompra))
                    {
                        lineasMediaRes.Add(new Entidades.MediaRes
                        {
                            compra = compra,
                            nroTropa = (linea.NroTropa ?? string.Empty).Trim(),
                            kgMedia = linea.KgMedia,
                            precioMedia = linea.PrecioMedia,
                            sucursal = sucursal
                        });
                    }
                    else
                    {
                        var corte = oCorteN.findCorteById(linea.IdCorte ?? 0, false);
                        int idEmpresaSesionLinea = (Session["Usuario"] as Entidades.Usuario) != null
                            ? ((Session["Usuario"] as Entidades.Usuario).IdEmpresa)
                            : (empresa != null ? empresa.IdEmpresa : 0);
                        if (corte == null || corte.IdCorte <= 0 || (idEmpresaSesionLinea > 0 && corte.IdEmpresa != idEmpresaSesionLinea))
                            return Json(new { ok = false, mensaje = "No se encontró el producto de la línea " + index + "." });

                        lineasCortes.Add(new Entidades.CortePorCompra
                        {
                            compra = compra,
                            corte = corte,
                            cantKgs = linea.CantKgs,
                            precioKg = linea.PrecioKg,
                            PrecioVenta = linea.PrecioVenta,
                            ActualizarPrecioVenta = linea.ActualizarPrecioVenta,
                            Margen = linea.Margen,
                            Desc_recargo = linea.DescRecargo,
                            Iva_compra = linea.IvaCompra,
                            Balanza = linea.Balanza,
                            sucursal = sucursal,
                            Creado = DateTime.Now,
                            CreadoPor = user
                        });
                    }
                }

                Entidades.EgresoCaja egresoCaja = null;
                if (desdePos)
                {
                    if (!oCierreN.validarCajaAbiertaVendedor(compra.FechaCompra, sucursal, user))
                        return Json(new { ok = false, mensaje = "La fecha y hora de la compra debe corresponder a una caja abierta del vendedor." });

                    if (model.IdCompra > 0)
                    {
                        egresoCaja = oCierreN.findEgresoCajaByTablaYId(Entidades.EgresoCaja.tablas.Compras.ToString(), model.IdCompra);
                        if (egresoCaja == null || egresoCaja.Id == 0)
                        {
                            var anterior = oCierreN.obtenerEgresosCaja(sucursal.IdSucursal, user.Id, Entidades.EgresoCaja.idCompraEgresoCaja, model.IdCompra.ToString(), compra.FechaCompra.Date, compra.FechaCompra.Date.AddDays(1));
                            if (anterior != null && anterior.Rows.Count > 0)
                            {
                                int idEgreso = Convert.ToInt32(anterior.Rows[0]["id"]);
                                egresoCaja = oCierreN.getEgresoCajaById(idEgreso);
                            }
                        }
                    }
                }

                int idCompra = oCompraN.AddOrEditCompra(
                    compra,
                    compra.TipoCompra,
                    lineasMediaRes,
                    lineasCortes,
                    desdePos,
                    egresoCaja);

                if (!desdePos)
                    TempData["ComprasSuccessMessage"] = model.IdCompra > 0
                        ? "La compra se guardó correctamente."
                        : "La compra se registró correctamente.";

                return Json(new
                {
                    ok = true,
                    idCompra = idCompra,
                    mensaje = model.IdCompra > 0 ? "La compra se guardó correctamente." : "La compra se registró correctamente.",
                    closeModal = desdePos,
                    redirectUrl = !desdePos
                        ? Url.Action("Index", "Compras")
                        : ""
                });
            }
            catch (Exception ex)
            {
                // Solo se libera el lock en error: el usuario tiene que poder reintentar de
                // inmediato si la compra no se guardo. En el camino de exito NO se libera antes
                // de tiempo -- si se liberara aca, un segundo click que llega despues de que el
                // primer POST ya termino (el caso real y mas comun de "guardar rapido dos veces",
                // no dos requests literalmente simultaneos) pasaria sin trabas. Se deja expirar
                // solo, cubriendo toda la ventana de 4s desde el primer submit.
                MemoryCache.Default.Remove(claveLock);
                return Json(new { ok = false, mensaje = "Error al guardar la compra. " + ex.Message });
            }
        }

        private CompraEditVm CrearViewModelNuevo(Entidades.Usuario user, string origen)
        {
            bool desdePos = EsOrigenPos(origen);
            bool permiteMediaRes = PermiteMediaRes(user);
            int idSucursal = user.IdSucursal > 0 ? user.IdSucursal : 0;
            Entidades.Sucursal sucursal = idSucursal > 0 ? oSucursalN.findById(idSucursal) : null;

            var model = new CompraEditVm
            {
                IdCompra = 0,
                Origen = origen,
                DesdePos = desdePos,
                EsEdicion = false,
                EmpresaCuit = ObtenerEmpresaCuit(user),
                PermiteMediaRes = permiteMediaRes,
                SucursalEditable = !desdePos,
                PuedeEditar = true,
                TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes),
                IdSucursal = idSucursal,
                SucursalNombre = sucursal != null ? sucursal.SucursalNombre : "",
                FechaCompra = DateTime.Now,
                EnCtaCte = false,
                DraftKey = BuildDraftKey(user, idSucursal, origen, 0)
            };

            model.TiposCompraDisponibles = ObtenerTiposCompraDisponibles(permiteMediaRes, desdePos);
            return model;
        }

        private CompraEditVm CrearViewModelEdicion(Entidades.Compra compra, Entidades.Usuario user, string origen)
        {
            bool desdePos = EsOrigenPos(origen);
            bool permiteMediaRes = PermiteMediaRes(user);
            var model = new CompraEditVm
            {
                IdCompra = compra.IdCompra,
                Origen = origen,
                DesdePos = desdePos,
                EsEdicion = true,
                EmpresaCuit = ObtenerEmpresaCuit(user),
                PermiteMediaRes = permiteMediaRes,
                SucursalEditable = !desdePos,
                PuedeEditar = true,
                TipoCompra = compra.TipoCompra,
                IdSucursal = compra.Sucursal != null ? compra.Sucursal.IdSucursal : user.IdSucursal,
                SucursalNombre = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "",
                FechaCompra = compra.FechaCompra,
                IdProveedor = compra.Proveedor != null ? compra.Proveedor.IdPersona : 0,
                ProveedorNombre = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "",
                ProveedorCuit = compra.Proveedor != null ? compra.Proveedor.Cuit : "",
                EnCtaCte = compra.EnCtaCte,
                NroRemito = compra.NroRemito,
                Observaciones = compra.Observaciones,
                CantMedias = compra.CantMedias,
                KgsMedias = compra.KgsMedias,
                Creado = FormatearFechaHora(compra.Creado),
                CreadoPor = compra.CreadoPor != null ? compra.CreadoPor.Nombre : "-",
                Actualizado = FormatearFechaHora(compra.Actualizado),
                ActualizadoPor = compra.ActualizadoPor != null ? compra.ActualizadoPor.Nombre : "-",
                DraftKey = BuildDraftKey(user, compra.Sucursal != null ? compra.Sucursal.IdSucursal : user.IdSucursal, origen, compra.IdCompra)
            };

            model.TiposCompraDisponibles = ObtenerTiposCompraDisponibles(permiteMediaRes, desdePos, compra.TipoCompra);

            if (EsTipoMediaRes(compra.TipoCompra))
            {
                var medias = oCompraN.obtenerMediasPorCompra(compra.IdCompra);
                int index = 0;
                foreach (System.Data.DataRow row in medias.Rows)
                {
                    index++;
                    float kg = row["kgMedia"] != DBNull.Value ? Convert.ToSingle(row["kgMedia"]) : 0f;
                    float precio = row["precioMedia"] != DBNull.Value ? Convert.ToSingle(row["precioMedia"]) : 0f;
                    model.Lineas.Add(new CompraLineaVm
                    {
                        Index = index,
                        TipoLinea = "MediaRes",
                        NroTropa = row["nroTropa"] != DBNull.Value ? row["nroTropa"].ToString() : "",
                        KgMedia = kg,
                        PrecioMedia = precio,
                        TotalLinea = kg * precio
                    });
                }
            }
            else
            {
                var cortes = oCompraN.convertCortesPorCompraToList(compra.IdCompra);
                int index = 0;
                foreach (var corte in cortes)
                {
                    index++;
                        model.Lineas.Add(new CompraLineaVm
                        {
                            Index = index,
                            TipoLinea = "Corte",
                            IdCorte = corte.Corte != null ? (int?)corte.Corte.IdCorte : null,
                            Codigo = corte.Corte != null ? (long?)corte.Corte.Codigo : null,
                            CorteNombre = corte.Corte != null ? corte.Corte.CorteDesc : "",
                            CantKgs = corte.CantKgs,
                            PrecioKg = corte.precioKg,
                            PrecioVenta = corte.PrecioVenta,
                            ActualizarPrecioVenta = false,
                            Margen = corte.Margen,
                            DescRecargo = corte.Desc_recargo,
                            IvaCompra = corte.Iva_compra,
                            Balanza = corte.Balanza,
                            TotalLinea = corte.CantKgs * corte.precioKg
                        });
                }
            }

            RecalcularTotales(model);
            return model;
        }

        private void CargarViewBags(CompraEditVm model, Entidades.Usuario user)
        {
            ViewBag.Title = model.EsEdicion ? "Modificar Compra" : "Nueva Compra";
            ViewBag.Seccion = "Compras";
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.EsPos = model.DesdePos;
            ViewBag.UrlGuardar = Url.Action("Guardar", "Compras");
            ViewBag.UrlBuscarPersonaModal = Url.Action("Buscar", "Personas");
            ViewBag.UrlPersonaListar = Url.Action("Listar", "Personas");
            ViewBag.UrlBuscarCorte = Url.Action("BuscarCorte", "Compras");
            ViewBag.UrlBuscarCortePorCodigo = Url.Action("BuscarCortePorCodigo", "Compras");
            ViewBag.UrlModalProducto = Url.Action("Buscar", "Productos");
            ViewBag.IdSucursalSesion = user != null ? user.IdSucursal : 0;
        }

        private static string NormalizarOrigen(string origen)
        {
            return string.Equals(origen, "pos", StringComparison.OrdinalIgnoreCase) ? "pos" : "layout";
        }

        private static bool EsOrigenPos(string origen)
        {
            return string.Equals(origen, "pos", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsTipoMediaRes(string tipoCompra)
        {
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes), StringComparison.OrdinalIgnoreCase);
        }

        private bool PermiteMediaRes(Entidades.Usuario user)
        {
            return ObtenerEmpresaCuit(user) == CuitHabilitaMediaRes;
        }

        private long ObtenerEmpresaCuit(Entidades.Usuario user)
        {
            return user != null && user.Empresa != null ? user.Empresa.Cuit : 0;
        }

        private List<string> ObtenerTiposCompraDisponibles(bool permiteMediaRes, bool desdePos, string tipoActual = "")
        {
            var tipos = new List<string>();
            if (permiteMediaRes)
                tipos.Add(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes));

            tipos.Add(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes));

            if (!string.IsNullOrWhiteSpace(tipoActual) && !tipos.Contains(tipoActual))
                tipos.Add(tipoActual);

            return tipos;
        }

        private static string BuildDraftKey(Entidades.Usuario user, int idSucursal, string origen, int idCompra)
        {
            int idUsuario = user != null ? user.Id : 0;
            return "compra_draft_" + idUsuario + "_" + idSucursal + "_" + origen + "_" + idCompra;
        }

        private static string FormatearFechaHora(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        }

        private static int CalcularTotalCantMedias(DataTable dt)
        {
            int total = 0;
            if (dt == null)
                return total;

            foreach (DataRow row in dt.Rows)
            {
                total += row["cantMedias"] == DBNull.Value ? 0 : Convert.ToInt32(row["cantMedias"]);
            }

            return total;
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

        private static float CalcularTotalImporte(DataTable dt)
        {
            float total = 0f;
            if (dt == null)
                return total;

            foreach (DataRow row in dt.Rows)
            {
                total += row["totalS"] == DBNull.Value ? 0f : Convert.ToSingle(row["totalS"]);
            }

            return total;
        }

        // Detalle de una compra (cabecera + lineas) para el expandible de Compras/Index.
        // Se carga por AJAX solo cuando el usuario abre la fila (mismo patron que
        // StockController.Detalle, ver docs/DECISIONS.md 2026-08-12): evita traer las lineas
        // de TODAS las compras visibles en la lista de una sola vez.
        public PartialViewResult Detalle(int idCompra)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
            {
                ViewBag.ComprasDetalleError = "La sesion expiró. Recargá la página para continuar.";
                return PartialView("~/Views/Compras/_ComprasDetalle.cshtml", null);
            }

            Entidades.Compra compra = oCompraN.findById_convertToCompra(idCompra);
            if (compra == null || compra.IdCompra <= 0)
            {
                ViewBag.ComprasDetalleError = "No se encontraron los detalles de la compra.";
                return PartialView("~/Views/Compras/_ComprasDetalle.cshtml", null);
            }

            if (!PermisosHelper.TienePermiso(Session, Permisos.Compra.VerCompras, compra.FechaCompra, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                ViewBag.ComprasDetalleError = ConstruirMensajePermisoFecha(Permisos.Compra.VerCompras, compra.FechaCompra, Utilidades.ValoresParametrosMetodos.IdCreadorNulo())
                    ?? "No tiene permisos para consultar esta compra.";
                return PartialView("~/Views/Compras/_ComprasDetalle.cshtml", null);
            }

            // compra.LineasCortes/LineasMediasReses (las del entity que arma findById_convertToCompra) NO
            // quedan pobladas de forma confiable -- se vio en vivo que dan Cantidad/Total en 0. Se recalcula
            // todo desde las MISMAS 2 fuentes que ya se usan en el resto del controller para este dato
            // (convertCortesPorCompraToList para cortes, obtenerMediasPorCompra para medias -- ver linea ~520).
            var cortes = oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>();
            var lineas = cortes
                .Select(corte => new StockLineaDetalleVm
                {
                    Codigo = corte.Corte != null ? corte.Corte.Codigo.ToString() : "-",
                    Producto = corte.Corte != null ? corte.Corte.CorteDesc : "-",
                    CantidadKgTexto = corte.CantKgs.ToString("N3"),
                    Balanza = corte.Balanza,
                    CreadoTexto = FormatearFechaHora(corte.Creado),
                    CantidadKg = Convert.ToDecimal(corte.CantKgs)
                })
                .ToList();

            float cantidadKg = cortes.Sum(c => c.CantKgs);
            float totalImporte = cortes.Sum(c => c.CantKgs * c.precioKgs);

            if (EsTipoMediaRes(compra.TipoCompra))
            {
                var medias = oCompraN.obtenerMediasPorCompra(compra.IdCompra);
                foreach (DataRow mediaRow in medias.Rows)
                {
                    float kg = mediaRow["kgMedia"] != DBNull.Value ? Convert.ToSingle(mediaRow["kgMedia"]) : 0f;
                    float precio = mediaRow["precioMedia"] != DBNull.Value ? Convert.ToSingle(mediaRow["precioMedia"]) : 0f;
                    cantidadKg += kg;
                    totalImporte += kg * precio;
                }
            }

            var detalle = new CompraIndexDetalleVm
            {
                IdCompra = compra.IdCompra,
                FechaCompra = compra.FechaCompra,
                NumeroDocumento = compra.NroRemito ?? "",
                Proveedor = compra.Proveedor != null ? compra.Proveedor.RazonSocial : "",
                TipoCompra = compra.TipoCompra ?? "",
                Cantidad = cantidadKg,
                CantidadMedias = compra.CantMedias ?? 0,
                Total = totalImporte,
                Sucursal = compra.Sucursal != null ? compra.Sucursal.SucursalNombre : "",
                Observaciones = compra.Observaciones ?? "",
                Estado = compra.Estado ?? "",
                EnCtaCte = compra.EnCtaCte,
                UsuarioCreacion = compra.CreadoPor != null ? compra.CreadoPor.Nombre : "",
                FechaCreacion = compra.Creado,
                UsuarioActualizacion = compra.ActualizadoPor != null ? compra.ActualizadoPor.Nombre : "",
                FechaActualizacion = compra.Actualizado,
                Lineas = lineas
            };

            return PartialView("~/Views/Compras/_ComprasDetalle.cshtml", detalle);
        }

        private List<CompraLineaDetalleVm> ConstruirLineasCompra(Entidades.Compra compra)
        {
            var lineas = new List<CompraLineaDetalleVm>();
            if (compra == null)
                return lineas;

            var cortes = oCompraN.convertCortesPorCompraToList(compra.IdCompra) ?? new List<Entidades.CortePorCompra>();
            foreach (var corte in cortes)
            {
                float totalLinea = corte.CantKgs * corte.precioKgs;
                lineas.Add(new CompraLineaDetalleVm
                {
                    Codigo = corte.Corte != null ? corte.Corte.Codigo.ToString() : "-",
                    Producto = corte.Corte != null ? corte.Corte.CorteDesc : "-",
                    CantidadKgTexto = corte.CantKgs.ToString("N3"),
                    PrecioTexto = corte.precioKgs.ToString("N2"),
                    TotalTexto = totalLinea.ToString("N2"),
                    CantidadKg = Convert.ToDecimal(corte.CantKgs),
                    Precio = Convert.ToDecimal(corte.precioKgs),
                    Total = Convert.ToDecimal(totalLinea)
                });
            }

            if (lineas.Count > 0)
                return lineas;

            DataTable dtMedias = oCompraN.obtenerMediasPorCompra(compra.IdCompra) ?? new DataTable();
            foreach (DataRow row in dtMedias.Rows)
            {
                float kg = row.Table.Columns.Contains("kgMedia") && row["kgMedia"] != DBNull.Value ? Convert.ToSingle(row["kgMedia"]) : 0f;
                float precio = row.Table.Columns.Contains("precioMedia") && row["precioMedia"] != DBNull.Value ? Convert.ToSingle(row["precioMedia"]) : 0f;
                string nroTropa = row.Table.Columns.Contains("nroTropa") && row["nroTropa"] != DBNull.Value ? Convert.ToString(row["nroTropa"]) : "";

                lineas.Add(new CompraLineaDetalleVm
                {
                    Codigo = string.IsNullOrWhiteSpace(nroTropa) ? "-" : nroTropa,
                    Producto = "Media Res",
                    CantidadKgTexto = kg.ToString("N3"),
                    PrecioTexto = precio.ToString("N2"),
                    TotalTexto = (kg * precio).ToString("N2"),
                    CantidadKg = Convert.ToDecimal(kg),
                    Precio = Convert.ToDecimal(precio),
                    Total = Convert.ToDecimal(kg * precio)
                });
            }

            return lineas;
        }

        private static bool CoincideProducto(string codigo, string descripcion, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return true;

            string texto = filtro.Trim();
            return (codigo ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || (descripcion ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string ValidarModelo(CompraEditVm model, Entidades.Usuario user, bool desdePos)
        {
            if (model == null)
                return "No se recibieron datos.";

            bool permiteMediaRes = PermiteMediaRes(user);
            string tipoCortes = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes);

            if (string.IsNullOrWhiteSpace(model.TipoCompra))
                return "Seleccione un tipo de compra.";

            if (!permiteMediaRes && !string.Equals(model.TipoCompra, tipoCortes, StringComparison.OrdinalIgnoreCase))
                return "El tipo de compra seleccionado no está habilitado para la empresa actual.";

            if (model.IdSucursal <= 0)
                return "Seleccione una sucursal.";

            if (model.FechaCompra == DateTime.MinValue)
                return "Ingrese una fecha válida.";

            if (model.IdProveedor <= 0)
                return "Seleccione un proveedor.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe ingresar al menos una línea.";

            if (EsTipoMediaRes(model.TipoCompra))
            {
                if (!model.CantMedias.HasValue || model.CantMedias.Value <= 0)
                    return "Ingrese la cantidad de medias.";

                int index = 0;
                foreach (var linea in model.Lineas)
                {
                    index++;
                    if (linea.KgMedia <= 0)
                        return "La línea " + index + " debe tener kilos mayores a cero.";

                    if (linea.PrecioMedia <= 0)
                        return "La línea " + index + " debe tener un precio mayor a cero.";
                }
            }
            else
            {
                int index = 0;
                foreach (var linea in model.Lineas)
                {
                    index++;
                    string detalleLinea = !string.IsNullOrWhiteSpace(linea.CorteNombre)
                        ? " (" + linea.CorteNombre.Trim() + ")"
                        : "";

                    if (!linea.IdCorte.HasValue || linea.IdCorte.Value <= 0)
                        return "La línea " + index + detalleLinea + " no tiene un producto válido.";

                    if (linea.CantKgs <= 0)
                        return "La línea " + index + detalleLinea + " debe tener una cantidad mayor a cero.";

                    if (linea.PrecioKg <= 0)
                        return "La línea " + index + detalleLinea + " debe tener un precio de compra mayor a cero.";

                    if (linea.Margen < 0)
                        return "La línea " + index + detalleLinea + " tiene un margen negativo.";
                }
            }

            return "";
        }

        private static void RecalcularTotales(CompraEditVm model)
        {
            model.CantItems = model.Lineas != null ? model.Lineas.Count : 0;
            model.TotalKg = 0f;
            model.TotalImporte = 0f;

            foreach (var linea in model.Lineas ?? new List<CompraLineaVm>())
            {
                if (linea.EsMediaRes)
                {
                    model.TotalKg += linea.KgMedia;
                    model.TotalImporte += linea.KgMedia * linea.PrecioMedia;
                }
                else
                {
                    model.TotalKg += linea.CantKgs;
                    model.TotalImporte += linea.CantKgs * linea.PrecioKg;
                }
            }

            if (EsTipoMediaRes(model.TipoCompra))
                model.KgsMedias = model.TotalKg;
        }
    }
}
