using Entidades;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Utilidades;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class MovimientosController : BaseController
    {
        private const long CuitColumnasInternas = 20306210786;

        private Negocio.Corte oCorteN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Usuario oUsuarioN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oCorteN = new Negocio.Corte(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
            oUsuarioN = new Negocio.Usuario(empresa, param);
        }

        public ActionResult Index(int idSucursalOrigen = 0, int idSucursalDestino = 0, DateTime? fechaDesde = null, DateTime? fechaHasta = null, bool verDetalles = false)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Movimiento.VerMovimientos, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Movimiento.VerMovimientos, ref desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Movimientos", Permisos.Movimiento.VerMovimientos, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            }

            var sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            string sucOrigen = ObtenerNombreSucursalFiltro(sucursales, idSucursalOrigen);
            string sucDestino = ObtenerNombreSucursalFiltro(sucursales, idSucursalDestino);
            DataTable dt = oCorteN.obtenerMovimientos(sucOrigen, sucDestino, desde, hasta, "") ?? new DataTable();

            var model = new MovimientoIndexVm
            {
                IdSucursalOrigen = idSucursalOrigen,
                IdSucursalDestino = idSucursalDestino,
                FechaDesde = desde,
                FechaHasta = hasta,
                VerDetalles = verDetalles,
                MostrarColumnasInternas = ObtenerEmpresaCuit(user) == CuitColumnasInternas,
                Movimientos = ConstruirResumenes(dt)
            };

            ViewBag.Title = "Movimientos";
            ViewBag.Seccion = "Movimientos";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(sucursales);
            ConfigurarAdvertenciaFechaEnVivo("fechaDesde", Permisos.Movimiento.VerMovimientos, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());

            return View("~/Views/Movimientos/Index.cshtml", model);
        }

        public ActionResult Lineas(int idSucursalOrigen = 0, int idSucursalDestino = 0, DateTime? fechaDesde = null, DateTime? fechaHasta = null, string producto = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-param.GetInt(Entidades.ParamKeys.DiasLimitFechaDesde, 0));
            DateTime hasta = fechaHasta ?? DateTime.Today;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Movimiento.VerMovimientos, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Movimiento.VerMovimientos, ref desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Movimientos", Permisos.Movimiento.VerMovimientos, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            }

            var sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            string sucOrigen = ObtenerNombreSucursalFiltro(sucursales, idSucursalOrigen);
            string sucDestino = ObtenerNombreSucursalFiltro(sucursales, idSucursalDestino);
            DataTable dt = oCorteN.obtenerMovimientos(sucOrigen, sucDestino, desde.Date, hasta.Date, "") ?? new DataTable();

            var model = new MovimientoLineasIndexPageVm
            {
                IdSucursalOrigen = idSucursalOrigen,
                IdSucursalDestino = idSucursalDestino,
                Producto = producto ?? "",
                FechaDesde = desde,
                FechaHasta = hasta
            };

            foreach (DataRow row in dt.Rows)
            {
                int idMovimiento = ToInt(row, "Id Movimiento", "idMovimiento", "movimiento");
                if (idMovimiento <= 0 || model.Movimientos.Any(x => x.IdMovimiento == idMovimiento))
                    continue;

                Entidades.Movimiento movimiento = oCorteN.cargarMovimiento(idMovimiento, false);
                if (movimiento == null || movimiento.IdMovimiento <= 0)
                    continue;

                var lineas = (movimiento.ListaCortesPorMov ?? new List<Entidades.CortePorMovimiento>())
                    .Select(linea => new MovimientoLineaDetalleItemVm
                    {
                        Codigo = linea.Corte != null ? linea.Corte.Codigo.ToString() : "-",
                        Producto = linea.Corte != null ? linea.Corte.CorteDesc : "-",
                        CantidadKgTexto = linea.CantKg.ToString("N3"),
                        CantidadUnidadTexto = linea.CantUnidad.ToString("N0"),
                        Observacion = ConstruirObservacionLineaMovimiento(linea),
                        CantidadKg = Convert.ToDecimal(linea.CantKg),
                        CantidadUnidad = Convert.ToDecimal(linea.CantUnidad)
                    })
                    .Where(x => CoincideProductoMovimiento(x.Codigo, x.Producto, producto))
                    .ToList();

                if (lineas.Count == 0)
                    continue;

                var grupo = new MovimientoLineasGrupoVm
                {
                    IdMovimiento = movimiento.IdMovimiento,
                    CollapseId = "movLineas_" + movimiento.IdMovimiento,
                    Titulo = "MOVIMIENTO ID: " + movimiento.IdMovimiento,
                    Subtitulo = movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"),
                    ResumenCompacto = movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"),
                    ResumenSecundario = (movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-") + " -> " + (movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-"),
                    EditUrl = Url.Action("Editar", "Movimientos", new { id = movimiento.IdMovimiento }),
                    TotalKg = lineas.Sum(x => x.CantidadKg),
                    TotalUnidades = lineas.Sum(x => x.CantidadUnidad)
                };

                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Fecha", Valor = movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal origen", Valor = movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Sucursal destino", Valor = movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Estado", Valor = ToString(row, "Estado", "estado") });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Usuario", Valor = movimiento.CreadoPor != null ? movimiento.CreadoPor.Nombre : "-" });
                grupo.Campos.Add(new CabeceraDetalleCampoVm { Etiqueta = "Observaciones", Valor = string.IsNullOrWhiteSpace(movimiento.Observaciones) ? "-" : movimiento.Observaciones });

                grupo.Lineas.AddRange(lineas);
                model.Movimientos.Add(grupo);
            }

            ViewBag.Title = "Lineas de movimiento";
            ViewBag.Seccion = "Movimientos";
            ViewBag.Sucursales = ConstruirSucursalesConTodas(sucursales);

            return View("~/Views/Movimientos/Lineas.cshtml", model);
        }

        public ActionResult Nuevo()
        {
            return Editar(0);
        }

        public ActionResult Editar(int id = 0)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Movimiento.NuevoMovimiento, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Movimiento.NuevoMovimiento, DateTime.Today, user.Id) ?? "No tiene permisos para crear o modificar movimientos.";
                return RedirectToAction("Index");
            }

            MovimientoEditVm model = id > 0
                ? CrearModeloEdicion(id, user)
                : CrearModeloNuevo(user);

            if (model == null)
                return HttpNotFound();

            ViewBag.Title = model.EsEdicion ? "Modificar Movimiento" : "Nuevo Movimiento";
            ViewBag.Seccion = "Movimientos";
            ViewBag.Sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            ViewBag.UrlBuscarProducto = Url.Action("BuscarProducto", "Movimientos");
            ViewBag.UrlBuscarProductoPorCodigo = Url.Action("BuscarProductoPorCodigo", "Movimientos");
            ViewBag.UrlDetalle = Url.Action("Detalle", "Movimientos");
            ViewBag.UrlGuardar = Url.Action("Guardar", "Movimientos");
            ConfigurarAdvertenciaFechaEnVivo("FechaMovimiento", Permisos.Movimiento.NuevoMovimiento, user.Id);

            return View("~/Views/Movimientos/Editar.cshtml", model);
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

                var resultado = productos.Take(200).Select(p => new
                {
                    id = p.IdCorte,
                    codigo = p.codigo.ToString(),
                    nombre = !string.IsNullOrWhiteSpace(p.corte) ? p.corte : p.CorteDesc,
                    precio = p.precioKg
                }).ToList();

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

        [HttpGet]
        public PartialViewResult Detalle(int id)
        {
            var movimiento = oCorteN.cargarMovimiento(id, false);
            var lineas = oCorteN.cargarCortesPorMovimiento(id, false) ?? new List<Entidades.CortePorMovimiento>();

            var model = new MovimientoDetalleVm
            {
                IdMovimiento = id,
                Observaciones = movimiento != null ? movimiento.Observaciones ?? "" : "",
                Lineas = lineas.Select(MapLinea).ToList()
            };

            return PartialView("~/Views/Movimientos/_MovimientoDetalle.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Guardar(MovimientoEditVm model)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida." });

                if (!PermisosHelper.TienePermiso(Session, Permisos.Movimiento.NuevoMovimiento, model != null ? model.FechaMovimiento : DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para guardar movimientos." });

                if (model == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos del movimiento." });

                NormalizarDecimalesPosteados(model);

                string error = ValidarModelo(model);
                if (!string.IsNullOrWhiteSpace(error))
                    return Json(new { ok = false, mensaje = error });

                var entidad = model.IdMovimiento > 0 ? oCorteN.cargarMovimiento(model.IdMovimiento, false) : new Entidades.Movimiento();
                if (entidad == null)
                    entidad = new Entidades.Movimiento();

                entidad.FechaMovimiento = model.FechaMovimiento;
                entidad.SucursalOrigen = new Entidades.Sucursal { IdSucursal = model.IdSucursalOrigen };
                entidad.SucursalDestino = new Entidades.Sucursal { IdSucursal = model.IdSucursalDestino };
                entidad.Observaciones = model.Observaciones ?? "";

                if (model.IdMovimiento > 0)
                {
                    entidad.IdMovimiento = model.IdMovimiento;
                    entidad.ActualizadoPor = user;
                    if (entidad.CreadoPor == null)
                        entidad.CreadoPor = user;
                }
                else
                {
                    entidad.CreadoPor = user;
                }

                int idMovimiento = oCorteN.addOrEditMovimiento(entidad);
                entidad.IdMovimiento = idMovimiento;

                foreach (var linea in model.Lineas ?? new List<MovimientoLineaVm>())
                {
                    var item = new Entidades.CortePorMovimiento
                    {
                        Movimientos = entidad,
                        Corte = new Entidades.Corte { IdCorte = linea.IdCorte },
                        CantUnidad = linea.CantUnidad,
                        CantKg = linea.CantKg,
                        PesoBalanza = linea.PesoBalanza,
                        PermitirIngreso = linea.PermitirIngreso
                    };

                    oCorteN.agregarCortePorMovimiento(item);
                }

                return Json(new
                {
                    ok = true,
                    movimientoId = idMovimiento,
                    mensaje = model.IdMovimiento > 0 ? "El movimiento se guardó correctamente." : "El movimiento se registró correctamente.",
                    redirectUrl = Url.Action("Index", "Movimientos"),
                    imprimirUrl = Url.Action("ImprimirTicket", "Movimientos", new { id = idMovimiento }),
                    imprimirPayloadUrl = Url.Action("ImprimirTicketPayload", "Movimientos", new { id = idMovimiento }),
                    pdfUrl = Url.Action("ImprimirPdf", "Movimientos", new { id = idMovimiento }),
                    whatsappTexto = ConstruirMensajeWhatsapp(idMovimiento)
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ImprimirTicket(int id, int mm = 80)
        {
            var movimiento = oCorteN.cargarMovimiento(id, true);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return HttpNotFound();

            movimiento.ListaCortesPorMov = oCorteN.cargarCortesPorMovimiento(id, true) ?? new List<Entidades.CortePorMovimiento>();
            ViewBag.TicketMm = mm == 58 ? 58 : 80;
            return View("~/Views/Movimientos/_TicketMovimiento.cshtml", movimiento);
        }

        [HttpGet]
        public JsonResult ImprimirTicketPayload(int id, int mm = 80)
        {
            var movimiento = oCorteN.cargarMovimiento(id, true);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el movimiento." }, JsonRequestBehavior.AllowGet);

            movimiento.ListaCortesPorMov = oCorteN.cargarCortesPorMovimiento(id, true) ?? new List<Entidades.CortePorMovimiento>();
            int ticketMm = mm == 58 ? 58 : 80;
            return Json(new
            {
                ok = true,
                ticketMm = ticketMm,
                ticketLines = ConstruirLineasTicketMovimiento(movimiento, ticketMm)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DescargarAgenteImpresion()
        {
            string path = Server.MapPath("~/Content/downloads/CarniSys.PrintAgent.zip");
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            return File(path, "application/zip", "CarniSys.PrintAgent.zip");
        }

        [HttpGet]
        public ActionResult ImprimirPdf(int id)
        {
            var movimiento = oCorteN.cargarMovimiento(id, true);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return HttpNotFound();

            var lineas = oCorteN.cargarCortesPorMovimiento(id, true) ?? new List<Entidades.CortePorMovimiento>();
            byte[] bytes = GenerarPdfMovimiento(movimiento, lineas);
            return File(bytes, "application/pdf", "Movimiento_" + id + ".pdf");
        }

        private MovimientoEditVm CrearModeloNuevo(Entidades.Usuario user)
        {
            int idSucursalActual = user != null ? user.IdSucursal : 0;
            var sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var model = new MovimientoEditVm
            {
                IdMovimiento = 0,
                EsEdicion = false,
                EmpresaCuit = ObtenerEmpresaCuit(user),
                MostrarColumnasInternas = ObtenerEmpresaCuit(user) == CuitColumnasInternas,
                FechaMovimiento = DateTime.Now,
                UsuarioNombre = user != null ? user.Nombre : "",
                IdSucursalOrigen = idSucursalActual
            };

            if (sucursales.Count == 2 && idSucursalActual > 0)
            {
                var destino = sucursales.FirstOrDefault(s => s.IdSucursal != idSucursalActual);
                if (destino != null)
                    model.IdSucursalDestino = destino.IdSucursal;
            }

            return model;
        }

        private MovimientoEditVm CrearModeloEdicion(int id, Entidades.Usuario user)
        {
            var movimiento = oCorteN.cargarMovimiento(id, false);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return null;

            var lineas = oCorteN.cargarCortesPorMovimiento(id, false) ?? new List<Entidades.CortePorMovimiento>();

            return new MovimientoEditVm
            {
                IdMovimiento = movimiento.IdMovimiento,
                EsEdicion = true,
                EmpresaCuit = ObtenerEmpresaCuit(user),
                MostrarColumnasInternas = ObtenerEmpresaCuit(user) == CuitColumnasInternas,
                IdSucursalOrigen = movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.IdSucursal : 0,
                IdSucursalDestino = movimiento.SucursalDestino != null ? movimiento.SucursalDestino.IdSucursal : 0,
                FechaMovimiento = movimiento.FechaMovimiento,
                Observaciones = movimiento.Observaciones ?? "",
                UsuarioNombre = user != null ? user.Nombre : "",
                Creado = movimiento.Creado.HasValue ? FormatearFechaHora(movimiento.Creado.Value) : "-",
                CreadoPor = movimiento.CreadoPor != null ? movimiento.CreadoPor.Nombre : "-",
                Actualizado = movimiento.Actualizado.HasValue ? FormatearFechaHora(movimiento.Actualizado.Value) : "-",
                ActualizadoPor = movimiento.ActualizadoPor != null ? movimiento.ActualizadoPor.Nombre : "-",
                IdMovimientoOrigen = movimiento.IdMovOrigen.HasValue && movimiento.IdMovOrigen.Value > 0 ? movimiento.IdMovOrigen.Value.ToString() : movimiento.IdMovimiento.ToString(),
                IdMovimientoDestino = movimiento.IdMovOrigen.HasValue && movimiento.IdMovOrigen.Value > 0 ? movimiento.IdMovimiento.ToString() : "-",
                Lineas = lineas.Select(MapLinea).ToList()
            };
        }

        private List<MovimientoResumenVm> ConstruirResumenes(DataTable dt)
        {
            var lista = new List<MovimientoResumenVm>();
            if (dt == null)
                return lista;

            foreach (DataRow row in dt.Rows)
            {
                int idMovimiento = ToInt(row, "Id Movimiento", "idMovimiento", "movimiento");
                decimal totalUnidad = ObtenerDecimalFila(row, "totalUnidad", "cantUnidad", "Cant Prod.", "cantProd", "kgCorte");
                decimal totalKilos = ObtenerDecimalFila(row, "totalKilos", "cantKg", "cantKgs", "kg", "kgCorte");

                if (idMovimiento > 0 && (totalUnidad == 0m || totalKilos == 0m))
                {
                    var lineas = oCorteN.cargarCortesPorMovimiento(idMovimiento, false) ?? new List<Entidades.CortePorMovimiento>();
                    if (totalUnidad == 0m)
                        totalUnidad = lineas.Sum(x => Convert.ToDecimal(x.CantUnidad));
                    if (totalKilos == 0m)
                        totalKilos = lineas.Sum(x => Convert.ToDecimal(x.CantKg));
                }

                string observaciones = ToString(row, "observaciones", "Observaciones", "obs");

                lista.Add(new MovimientoResumenVm
                {
                    IdMovimiento = idMovimiento,
                    FechaMovimiento = ToDateTime(row, "Fecha Movimiento", "fechaMovimiento"),
                    Origen = ToString(row, "Origen", "sucursalOrigen", "origen"),
                    Destino = ToString(row, "Destino", "sucursalDestino", "destino"),
                    DeOrigen = ToString(row, "Id Origen", "idMovOrigen", "deOrigen"),
                    Estado = ToString(row, "Estado", "estado"),
                    TotalUnidad = totalUnidad,
                    TotalKilos = totalKilos,
                    Observaciones = observaciones,
                    TieneObservaciones = !string.IsNullOrWhiteSpace(observaciones)
                });
            }

            return lista.OrderByDescending(x => x.FechaMovimiento).ToList();
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

        private string ObtenerNombreSucursalFiltro(List<Entidades.Sucursal> sucursales, int idSucursal)
        {
            if (idSucursal <= 0 || sucursales == null)
                return "";

            var sucursal = sucursales.FirstOrDefault(s => s.IdSucursal == idSucursal);
            return sucursal != null ? sucursal.SucursalNombre : "";
        }

        private long ObtenerEmpresaCuit(Entidades.Usuario user)
        {
            return user != null && user.Empresa != null ? user.Empresa.Cuit : 0;
        }

        private string ValidarModelo(MovimientoEditVm model)
        {
            if (model.IdSucursalOrigen <= 0 || model.IdSucursalDestino <= 0)
                return "Debe seleccionar la sucursal origen y destino.";

            if (model.IdSucursalOrigen == model.IdSucursalDestino)
                return "La sucursal origen y destino deben ser diferentes.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe agregar al menos un producto al movimiento.";

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea.IdCorte <= 0)
                    return "La línea " + (i + 1) + " no tiene producto válido.";

                if (linea.CantUnidad < 0)
                    return "La línea " + (i + 1) + " debe tener una cantidad mayor o igual a cero.";

                if (linea.CantKg <= 0)
                    return "La línea " + (i + 1) + " debe tener kilos mayores a cero.";
            }

            return "";
        }

        private static bool CoincideProductoMovimiento(string codigo, string descripcion, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return true;

            string texto = filtro.Trim();
            return (codigo ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                || (descripcion ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ConstruirObservacionLineaMovimiento(Entidades.CortePorMovimiento linea)
        {
            var partes = new List<string>();
            if (linea != null && linea.PesoBalanza)
                partes.Add("Peso balanza");
            if (linea != null && linea.PermitirIngreso)
                partes.Add("Permite ingreso");

            return partes.Count == 0 ? "-" : string.Join(" | ", partes);
        }

        private MovimientoLineaVm MapLinea(Entidades.CortePorMovimiento linea)
        {
            return new MovimientoLineaVm
            {
                IdCorteMovimiento = linea.IdCorteMovimiento,
                IdCorte = linea.Corte != null ? linea.Corte.IdCorte : 0,
                Codigo = linea.Corte != null ? linea.Corte.Codigo : 0,
                Producto = linea.Corte != null ? linea.Corte.CorteDesc : "",
                TipoProducto = linea.Corte != null ? linea.Corte.Tipo : "",
                PromedioProducto = linea.Corte != null ? linea.Corte.Promedio : 0,
                CantUnidad = linea.CantUnidad,
                CantKg = linea.CantKg,
                PesoBalanza = linea.PesoBalanza,
                PermitirIngreso = linea.PermitirIngreso
            };
        }

        private void NormalizarDecimalesPosteados(MovimientoEditVm model)
        {
            if (model == null || model.Lineas == null || Request == null || Request.Form == null)
                return;

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                var linea = model.Lineas[i];
                if (linea == null)
                    continue;

                float valorFloat;
                string keyCantKg = "Lineas[" + i + "].CantKg";
                if (TryParseFloatFlexible(Request.Form[keyCantKg], out valorFloat))
                {
                    linea.CantKg = valorFloat;
                    ModelState.Remove(keyCantKg);
                }

                string keyPromedio = "Lineas[" + i + "].PromedioProducto";
                if (TryParseFloatFlexible(Request.Form[keyPromedio], out valorFloat))
                {
                    linea.PromedioProducto = valorFloat;
                    ModelState.Remove(keyPromedio);
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

        private string ConstruirMensajeWhatsapp(int idMovimiento)
        {
            var movimiento = oCorteN.cargarMovimiento(idMovimiento, true);
            if (movimiento == null || movimiento.IdMovimiento <= 0)
                return "Se registró un movimiento.";

            var lineas = oCorteN.cargarCortesPorMovimiento(idMovimiento, true) ?? new List<Entidades.CortePorMovimiento>();
            int totalUnidades = lineas.Sum(x => x.CantUnidad);
            float totalKilos = lineas.Sum(x => x.CantKg);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Movimiento {0}%0AOrigen: {1}%0ADestino: {2}%0AFecha: {3}%0ATotal unidades: {4}%0ATotal kilos: {5:F3}",
                movimiento.IdMovimiento,
                movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-",
                movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-",
                movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"),
                totalUnidades,
                totalKilos);
        }

        private List<string> ConstruirLineasTicketMovimiento(Entidades.Movimiento movimiento, int ticketMm)
        {
            var lineas = movimiento.ListaCortesPorMov ?? new List<Entidades.CortePorMovimiento>();
            int cantMaxChar = ticketMm == 58 ? 32 : 43;
            int totalUnidades = 0;
            decimal totalKilos = 0m;
            int anchoProducto = cantMaxChar == 32 ? 15 : 22;
            int anchoCant = cantMaxChar == 32 ? 5 : 6;
            int anchoKgs = cantMaxChar == 32 ? 7 : 8;

            Func<string, int, string> truncar = (texto, maximo) =>
            {
                texto = texto ?? "";
                return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
            };

            Func<string, int, string> centrar = (texto, ancho) =>
            {
                texto = truncar(texto, ancho);
                int espaciosIzquierda = (ancho - texto.Length) / 2;
                if (espaciosIzquierda < 0) espaciosIzquierda = 0;
                return new string(' ', espaciosIzquierda) + texto;
            };

            Func<string, int, bool, string> ajustarColumna = (texto, ancho, derecha) =>
            {
                texto = truncar(texto, ancho);
                return derecha ? texto.PadLeft(ancho) : texto.PadRight(ancho);
            };

            var sb = new StringBuilder();
            Action<string> linea = texto => sb.AppendLine(texto ?? "");

            linea(centrar("Movimiento", cantMaxChar));
            linea(truncar("ID: " + movimiento.IdMovimiento, cantMaxChar));
            linea(truncar("Origen: " + (movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-"), cantMaxChar));
            linea(truncar("Destino: " + (movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-"), cantMaxChar));
            linea(truncar("Fecha: " + movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"), cantMaxChar));
            linea(" ");
            linea(ajustarColumna("Producto", anchoProducto, false) + " " + ajustarColumna("Cant.", anchoCant, true) + "    " + ajustarColumna("Kgs.", anchoKgs, true));
            linea(new string('-', cantMaxChar));

            foreach (var item in lineas)
            {
                if (item == null)
                    continue;

                string nombreProducto = item.Corte != null ? item.Corte.CorteDesc : "";
                string codigoProducto = item.Corte != null ? item.Corte.Codigo.ToString() : "";
                string cantidadTexto = item.CantUnidad.ToString();
                string kilosTexto = Convert.ToDecimal(item.CantKg).ToString("N3");

                totalUnidades += item.CantUnidad;
                totalKilos += Convert.ToDecimal(item.CantKg);

                linea(ajustarColumna(nombreProducto, anchoProducto, false) + " " + ajustarColumna(cantidadTexto, anchoCant, true) + "    " + ajustarColumna(kilosTexto, anchoKgs, true));
                linea(ajustarColumna("Cod:" + codigoProducto, anchoProducto, false));
            }

            linea(new string('-', cantMaxChar));
            linea(ajustarColumna("Total", anchoProducto, false) + " " + ajustarColumna(totalUnidades.ToString(), anchoCant, true) + "    " + ajustarColumna(totalKilos.ToString("N3"), anchoKgs, true));

            if (!string.IsNullOrWhiteSpace(movimiento.Observaciones))
            {
                linea("");
                linea("Observaciones:");
                string observacion = movimiento.Observaciones ?? "";
                for (int i = 0; i < observacion.Length; i += cantMaxChar)
                    linea(observacion.Substring(i, Math.Min(cantMaxChar, observacion.Length - i)));
            }

            return sb.ToString()
                .Replace("\r\n", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.None)
                .ToList();
        }

        private byte[] GenerarPdfMovimiento(Entidades.Movimiento movimiento, List<Entidades.CortePorMovimiento> lineas)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                doc.Add(new Paragraph("Movimiento", titleFont));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("ID: " + movimiento.IdMovimiento, normalFont));
                doc.Add(new Paragraph("Origen: " + (movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-"), normalFont));
                doc.Add(new Paragraph("Destino: " + (movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-"), normalFont));
                doc.Add(new Paragraph("Fecha: " + movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"), normalFont));

                if (!string.IsNullOrWhiteSpace(movimiento.Observaciones))
                    doc.Add(new Paragraph("Observaciones: " + movimiento.Observaciones, normalFont));

                doc.Add(new Paragraph(" "));

                var table = new PdfPTable(4) { WidthPercentage = 100f };
                table.SetWidths(new float[] { 2.5f, 6f, 2f, 2f });
                table.AddCell(new Phrase("Código", boldFont));
                table.AddCell(new Phrase("Producto", boldFont));
                table.AddCell(new Phrase("Cant. Un.", boldFont));
                table.AddCell(new Phrase("Kgs.", boldFont));

                foreach (var linea in lineas)
                {
                    table.AddCell(new Phrase(linea.Corte != null ? linea.Corte.Codigo.ToString() : "", normalFont));
                    table.AddCell(new Phrase(linea.Corte != null ? linea.Corte.CorteDesc : "", normalFont));
                    table.AddCell(new Phrase(linea.CantUnidad.ToString(), normalFont));
                    table.AddCell(new Phrase(linea.CantKg.ToString("F3", CultureInfo.InvariantCulture), normalFont));
                }

                doc.Add(table);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Total unidades: " + lineas.Sum(x => x.CantUnidad), boldFont));
                doc.Add(new Paragraph("Total kilos: " + lineas.Sum(x => x.CantKg).ToString("F3", CultureInfo.InvariantCulture), boldFont));
                doc.Close();

                return ms.ToArray();
            }
        }

        private int ToInt(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (row.Table.Columns.Contains(name))
                {
                    int value;
                    if (int.TryParse(Convert.ToString(row[name]), out value))
                        return value;
                }
            }

            return 0;
        }

        private DateTime ToDateTime(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (row.Table.Columns.Contains(name))
                {
                    DateTime value;
                    if (DateTime.TryParse(Convert.ToString(row[name]), out value))
                        return value;
                }
            }

            return DateTime.MinValue;
        }

        private string ToString(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (row.Table.Columns.Contains(name))
                    return Convert.ToString(row[name]) ?? "";
            }

            return "";
        }

        private decimal ObtenerDecimalFila(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                if (!row.Table.Columns.Contains(name))
                    continue;

                decimal value;
                string raw = Convert.ToString(row[name]) ?? "";
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                    return value;

                if (decimal.TryParse(raw.Replace(".", ","), NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out value))
                    return value;

                if (decimal.TryParse(raw.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    return value;
            }

            return 0m;
        }

        private string FormatearFechaHora(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
