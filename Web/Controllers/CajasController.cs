using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Entidades;
using System.Data;
using System.Globalization;
using System.Text;
using Web.Helpers;
using Utilidades;
using System.Linq;
using System.Web.SessionState;
using Web.Models;

namespace Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class CajasController : BaseController
    {
        Negocio.CierreCaja oCierreN;
        Negocio.Sucursal oSucursalN;
        Negocio.Usuario oUsuarioN;
        Negocio.Venta oVentaN;

        private sealed class GuardarEgresoCajaResultado
        {
            public bool Ok { get; set; }
            public int Id { get; set; }
            public string Mensaje { get; set; }
        }

        public sealed class CambioSucursalCajaPostVm
        {
            public int IdCierre { get; set; }
            public int IdSucursalNueva { get; set; }
        }

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oCierreN = new Negocio.CierreCaja(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);
            oUsuarioN = new Negocio.Usuario(empresa);
            oVentaN = new Negocio.Venta(empresa, param);
        }

        // GET: Cajas/CajasAbiertas
        public ActionResult CajasAbiertas(int? idSucursal, string buscar = "", bool ajax = false)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.Cajas.CajasAbiertasConsulta))
            {
                ViewBag.Seccion = "Cierres de Caja";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            // --- Sucursales para el combo ---
            var sucursales = oSucursalN.findAll();
            ViewBag.Sucursales = sucursales;
            ViewBag.PuedeCambiarSucursalCaja = PuedeCambiarSucursalCaja(user, sucursales);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.Buscar = buscar;

            // --- Armar filtro ---
            CierreCaja filtro = new CierreCaja();

            if (idSucursal.HasValue)
            {
                var sucursalActual = oSucursalN.findById(idSucursal.Value);

                if (sucursalActual == null)
                {
                    var dtVacio = new DataTable();

                    if (ajax)
                        return PartialView("_TablaCajasAbiertas", dtVacio);

                    return View("~/Views/Cajas/CajasAbiertas.cshtml", dtVacio);
                }

                filtro.Sucursal = sucursalActual;
            }

            // Si idSucursal viene null, filtro queda sin sucursal
            // y debería traer todas las sucursales
            var dt = oCierreN.findCierreCaja(
                filtro,
                CierreCaja.tipoBusqueda.FindOpen,
                buscar,
                null
            );

            if (ajax)
                return PartialView("_TablaCajasAbiertas", dt);

            return View("~/Views/Cajas/CajasAbiertas.cshtml", dt);
        }

        public ActionResult EgresosCaja(int? idSucursal, int idUsuario = -1, int idTipoEgresoCaja = 0, string descripcion = "", DateTime? fechaDesde = null, DateTime? fechaHasta = null, bool soloGastos = false, bool ajax = false)
        {
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today.AddDays(1).AddSeconds(-1);
            if (hasta.TimeOfDay == TimeSpan.Zero)
                hasta = hasta.AddDays(1).AddSeconds(-1);

            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.EgresosCaja.Consulta, desde))
            {
                if (ajax)
                    return new HttpStatusCodeResult(403, "No tiene permisos para ver egresos de caja.");

                CargarViewBagsEgresos(idSucursal ?? 0, idUsuario, idTipoEgresoCaja, descripcion, desde, hasta, soloGastos, false);
                ViewBag.SinPermiso = true;
                ViewBag.MensajePermiso = ConstruirMensajePermisoFecha(PermisosPantallasWeb.EgresosCaja.Consulta, desde)
                    ?? "No tiene permisos para ver egresos de caja.";
                return View("~/Views/Cajas/EgresosCaja.cshtml", new DataTable());
            }

            int sucursalSeleccionada = idSucursal ?? 0;

            CargarViewBagsEgresos(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion, desde, hasta, soloGastos, false);

            DataTable dt = oCierreN.obtenerEgresosCaja(sucursalSeleccionada, idUsuario, idTipoEgresoCaja, descripcion ?? "", desde, hasta);
            dt = FiltrarSoloGastos(dt, soloGastos, oCierreN.obtenerTiposEgresoCaja("", 0));
            CargarPermisosEdicionEgresos(dt, false);

            if (ajax)
                return PartialView("~/Views/Cajas/_EgresosCajaTabla.cshtml", dt);

            return View("~/Views/Cajas/EgresosCaja.cshtml", dt);
        }

        public ActionResult MisEgresosCaja(bool desdePos = false, bool soloEgresos = false, string filtroActividad = "todos")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new HttpStatusCodeResult(401, "Sesión inválida");

            CierreCaja cierre = ObtenerCajaAbiertaUsuario(user);
            if (cierre == null)
            {
                ViewBag.Mensaje = "No hay una caja abierta para el vendedor actual.";
                ViewBag.DesdePOS = desdePos;
                return PartialView("~/Views/Cajas/_MisEgresosCaja.cshtml", new DataTable());
            }

            DataTable dt = oCierreN.getEgresosCajaVendedor(cierre);
            if (soloEgresos)
                dt = FiltrarEgresosRealesVendedor(dt, soloEgresos);
            else
                dt = FiltrarActividadesCaja(dt, filtroActividad);

            ViewBag.DesdePOS = desdePos;
            ViewBag.SoloEgresos = soloEgresos;
            ViewBag.FiltroActividad = filtroActividad ?? "todos";
            ViewBag.CierreCaja = cierre;
            ViewBag.SucursalActividad = cierre != null && cierre.Sucursal != null ? cierre.Sucursal.sucursal : "";
            ViewBag.TotalVisible = CalcularTotalGastosCaja(dt);
            ViewBag.MostrarResumenMisActividades = PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.EgresosCaja.Consulta);
            ViewBag.ModoActividades = desdePos;
            CargarPermisosEdicionEgresos(dt, desdePos);

            return PartialView("~/Views/Cajas/_MisEgresosCaja.cshtml", dt);
        }

        public ActionResult ActividadesCaja(int idCierre, string filtroActividad = "todos")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.Cajas.CajasAbiertasConsulta))
                return new HttpStatusCodeResult(403, "No tiene permisos para ver actividades de caja.");

            if (idCierre <= 0)
                return new HttpStatusCodeResult(400, "Caja inválida.");

            var cierre = oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return HttpNotFound("No se encontró la caja seleccionada.");

            DataTable dt = oCierreN.getEgresosCajaVendedor(cierre);
            dt = FiltrarActividadesCaja(dt, filtroActividad);

            string nombreVendedor = cierre.UsuarioInicio != null ? cierre.UsuarioInicio.Nombre : "cajero";
            string nombreSucursal = cierre.Sucursal != null ? cierre.Sucursal.sucursal : "";

            ViewBag.DesdePOS = false;
            ViewBag.SoloEgresos = false;
            ViewBag.FiltroActividad = filtroActividad ?? "todos";
            ViewBag.CierreCaja = cierre;
            ViewBag.TotalVisible = CalcularTotalGastosCaja(dt);
            ViewBag.MostrarResumenMisActividades = PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.EgresosCaja.Consulta);
            ViewBag.ModoActividades = true;
            ViewBag.PermitirNuevo = CajaSigueAbierta(cierre) &&
                                    user != null &&
                                    PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.AltaEdicion, DateTime.Today, user.Id);
            ViewBag.IdCierreActividad = idCierre;
            ViewBag.SucursalActividad = nombreSucursal;
            ViewBag.TituloActividades = "Actividades";
            ViewBag.SubtituloActividades = string.IsNullOrWhiteSpace(nombreSucursal)
                ? nombreVendedor
                : nombreVendedor + " | " + nombreSucursal;
            CargarPermisosEdicionEgresos(dt, false, cierre);

            return PartialView("~/Views/Cajas/_MisEgresosCaja.cshtml", dt);
        }

        public ActionResult NuevoEgresoCaja(int id = 0, bool desdePos = false, int idCierre = 0)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new HttpStatusCodeResult(401, "Sesión inválida");

            bool tienePermiso = PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.AltaEdicion, DateTime.Today, user.Id);
            if (!tienePermiso && !desdePos)
            {
                return new HttpStatusCodeResult(403, "No tiene permisos para registrar egresos de caja.");
            }

            CierreCaja cierreContexto = idCierre > 0
                ? oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return HttpNotFound("No se encontró la caja seleccionada.");

                if (!CajaSigueAbierta(cierreContexto))
                    return new HttpStatusCodeResult(403, "La caja seleccionada ya no se encuentra abierta.");
            }

            EgresoCaja egreso;
            int idSucursal = cierreContexto != null && cierreContexto.Sucursal != null
                ? cierreContexto.Sucursal.idSucursal
                : user.IdSucursal;

            if (id > 0)
            {
                egreso = oCierreN.getEgresoCajaById(id);
                if (egreso == null || egreso.Id == 0)
                    return HttpNotFound("Egreso de caja no encontrado.");

                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egreso, desdePos, empresa, oCierreN.validarCajaAbiertaVendedor);
                if (!validacion.PuedeModificar)
                    return new HttpStatusCodeResult(403, validacion.MensajeBloqueo);

                if (cierreContexto != null && !FechaDentroDeCaja(egreso.Fecha, cierreContexto))
                    return new HttpStatusCodeResult(403, "El egreso seleccionado no corresponde al rango horario de la caja abierta.");

                idSucursal = egreso.Sucursal != null && egreso.Sucursal.idSucursal > 0 ? egreso.Sucursal.idSucursal : idSucursal;
            }
            else
            {
                if (desdePos)
                {
                    var sucursalSesion = oSucursalN.findById(user.IdSucursal);
                    if (sucursalSesion == null || !oCierreN.validarCajaAbiertaVendedor(DateTime.Now, sucursalSesion, user))
                        return new HttpStatusCodeResult(403, "Debe tener una caja abierta en la sucursal activa para registrar egresos desde POS.");
                }

                egreso = new EgresoCaja
                {
                    Fecha = DateTime.Now,
                    Sucursal = oSucursalN.findById(idSucursal),
                    CreadoPor = cierreContexto != null && cierreContexto.UsuarioInicio != null ? cierreContexto.UsuarioInicio.Id : user.Id,
                    CreadoPorUser = cierreContexto != null ? cierreContexto.UsuarioInicio : user
                };
            }

            CargarViewBagsFormularioEgreso(desdePos, idSucursal, cierreContexto);
            ViewBag.EsEdicion = id > 0;
            ViewBag.IdCierreActividad = idCierre;

            if (egreso.Sucursal == null || egreso.Sucursal.idSucursal == 0)
                egreso.Sucursal = oSucursalN.findById(idSucursal);

            return PartialView("~/Views/Cajas/_AddOrEditEgresoCaja.cshtml", egreso);
        }

        public ActionResult CalcularComisionesElectronicas(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int idSucursal = 0, bool desdePos = false, int idCierre = 0)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new HttpStatusCodeResult(401, "Sesión inválida");

            bool tienePermiso = PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.AltaEdicion, DateTime.Today, user.Id);
            if (!tienePermiso && !desdePos)
                return new HttpStatusCodeResult(403, "No tiene permisos para registrar egresos de caja.");

            CierreCaja cierreContexto = idCierre > 0
                ? oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return HttpNotFound("No se encontró la caja seleccionada.");

                if (!CajaSigueAbierta(cierreContexto))
                    return new HttpStatusCodeResult(403, "La caja seleccionada ya no se encuentra abierta.");
            }

            DateTime desde = (fechaDesde ?? DateTime.Today).Date;
            DateTime hasta = (fechaHasta ?? DateTime.Today).Date;
            if (desde > hasta)
                return new HttpStatusCodeResult(400, "La fecha desde no puede ser mayor que la fecha hasta.");

            bool sucursalFija = desdePos || (cierreContexto != null && cierreContexto.Sucursal != null);
            int sucursalSeleccionada = desdePos
                ? user.IdSucursal
                : (cierreContexto != null && cierreContexto.Sucursal != null
                    ? cierreContexto.Sucursal.idSucursal
                    : idSucursal);

            var sucursal = sucursalSeleccionada > 0 ? oSucursalN.findById(sucursalSeleccionada) : null;
            if (sucursalSeleccionada > 0 && sucursal == null)
                return new HttpStatusCodeResult(400, "Sucursal inválida.");

            if (desdePos && !oCierreN.validarCajaAbiertaVendedor(DateTime.Now, sucursal, user))
                return new HttpStatusCodeResult(403, "Debe tener una caja abierta en la sucursal activa para registrar egresos desde POS.");

            var model = CrearModelComisionesElectronicas(desde, hasta, DateTime.Now, sucursalSeleccionada, desdePos, idCierre);
            ViewBag.TiposEgresoCaja = oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.MostrarSelectorSucursal = !sucursalFija;
            return PartialView("~/Views/Cajas/_CalcularComisionesElectronicas.cshtml", model);
        }

        public JsonResult ObtenerResumenComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida." }, JsonRequestBehavior.AllowGet);

                if (fechaDesde.Date > fechaHasta.Date)
                    return Json(new { ok = false, mensaje = "La fecha desde no puede ser mayor que la fecha hasta." }, JsonRequestBehavior.AllowGet);

                int? sucursalConsulta = idSucursal > 0 ? (int?)idSucursal : null;
                if (sucursalConsulta.HasValue && oSucursalN.findById(sucursalConsulta.Value) == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." }, JsonRequestBehavior.AllowGet);

                var formas = ObtenerFormasPagoElectronicas(fechaDesde.Date, fechaHasta.Date, sucursalConsulta, null);
                return Json(new
                {
                    ok = true,
                    items = formas.Select(f => new
                    {
                        codigo = f.Codigo,
                        nombre = f.Nombre,
                        totalCobrado = f.TotalCobrado
                    }).ToList(),
                    total = formas.Sum(f => f.TotalCobrado)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "No se pudieron calcular las comisiones. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TiposEgresoCaja(string buscar = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new HttpStatusCodeResult(401, "Sesión inválida");

            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.EgresosCaja.TiposConsulta))
                return new HttpStatusCodeResult(403, "No tiene permisos para ver tipos de egreso.");

            ViewBag.BuscarTipoEgreso = buscar ?? "";
            ViewBag.PuedeEditarTipos = PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.TiposAltaEdicion, DateTime.Today, user.Id);
            ViewBag.UsuarioAdmin = user.Admin;

            DataTable dt = oCierreN.obtenerTiposEgresoCaja(buscar ?? "", 0);
            return PartialView("~/Views/Cajas/_TiposEgresoCajaModal.cshtml", dt);
        }

        public ActionResult AddOrEditTipoEgresoCaja(int id = 0)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new HttpStatusCodeResult(401, "Sesión inválida");

            if (!PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.TiposAltaEdicion, DateTime.Today, user.Id))
                return new HttpStatusCodeResult(403, "No tiene permisos para administrar tipos de egreso.");

            var model = new TipoEgresoCajaEditVm();
            if (id > 0)
            {
                DataTable dt = oCierreN.obtenerTiposEgresoCaja("", id);
                if (dt == null || dt.Rows.Count == 0)
                    return HttpNotFound("No se encontró el tipo de egreso seleccionado.");

                DataRow row = dt.Rows[0];
                bool reservado = row.Table.Columns.Contains("Reservado") && row["Reservado"] != DBNull.Value && Convert.ToBoolean(row["Reservado"]);
                if (reservado)
                    return new HttpStatusCodeResult(403, "El tipo de egreso seleccionado es reservado por el sistema y no puede modificarse.");

                model.Id = id;
                model.TipoEgresoCaja = Convert.ToString(row["tipoEgresoCaja"]);
                model.EsGasto = row.Table.Columns.Contains("Es_Gasto") && row["Es_Gasto"] != DBNull.Value && Convert.ToBoolean(row["Es_Gasto"]);
                model.Reservado = reservado;
            }

            return PartialView("~/Views/Cajas/_AddOrEditTipoEgresoCaja.cshtml", model);
        }

        public JsonResult TiposEgresoCajaOpciones()
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesión inválida." }, JsonRequestBehavior.AllowGet);

            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.EgresosCaja.Consulta, DateTime.Today))
                return Json(new { ok = false, mensaje = "No tiene permisos para ver tipos de egreso." }, JsonRequestBehavior.AllowGet);

            DataTable dt = oCierreN.obtenerTiposEgresoCaja("", 0);
            var items = dt.AsEnumerable()
                .Where(r => Convert.ToInt32(r["id"]) > 0)
                .Select(r => new
                {
                    id = Convert.ToInt32(r["id"]),
                    nombre = Convert.ToString(r["tipoEgresoCaja"])
                })
                .ToList();

            return Json(new { ok = true, items = items }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditarPagoActividad(int id, string returnUrl = "", bool desdePos = false)
        {
            return new HttpStatusCodeResult(403, "La modificación de pagos y cobros no está disponible desde Mis actividades.");
        }

        [HttpPost]
        public JsonResult GuardarEgresoCaja(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var resultado = GuardarEgresoCajaCore(id, fecha, idTipoEgresoCaja, descripcion, monto, detalle, idSucursal, desdePos, idCierre);
                return Json(new { ok = resultado.Ok, id = resultado.Id, mensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar egreso de caja. " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GuardarComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, DateTime fechaEgreso, int idTipoEgresoCaja, int idSucursal, string porcentajeDebito, string porcentajeCredito, string porcentajeQr, string porcentajeTransferencia, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida." });

                DateTime desde = fechaDesde.Date;
                DateTime hasta = fechaHasta.Date;
                if (desde > hasta)
                    return Json(new { ok = false, mensaje = "La fecha desde no puede ser mayor que la fecha hasta." });

                var porcentajes = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Debito", ParseDecimalFlexible(porcentajeDebito) },
                    { "Credito", ParseDecimalFlexible(porcentajeCredito) },
                    { "Qr", ParseDecimalFlexible(porcentajeQr) },
                    { "Transferencia", ParseDecimalFlexible(porcentajeTransferencia) }
                };

                int? sucursalFiltro = idSucursal > 0 ? (int?)idSucursal : null;
                if (sucursalFiltro.HasValue && oSucursalN.findById(sucursalFiltro.Value) == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." });

                var formas = ObtenerFormasPagoElectronicas(desde, hasta, sucursalFiltro, porcentajes);
                decimal totalComisiones = formas.Sum(f => f.ImporteComision);
                if (totalComisiones <= 0)
                    return Json(new { ok = false, mensaje = "Debe existir al menos una comisión mayor a cero para guardar el egreso." });

                string sucursalDescripcion = ObtenerDescripcionSucursal(idSucursal);
                string descripcion = string.Format(
                    "Periodo {0} - {1} | Sucursal: {2}",
                    desde.ToString("dd/MM/yyyy"),
                    hasta.ToString("dd/MM/yyyy"),
                    sucursalDescripcion);

                StringBuilder detalleBuilder = new StringBuilder();
                detalleBuilder.AppendFormat(
                    "Cálculo automático de comisiones por pagos electrónicos entre {0} y {1} | Sucursal: {2}.",
                    desde.ToString("dd/MM/yyyy"),
                    hasta.ToString("dd/MM/yyyy"),
                    sucursalDescripcion);

                foreach (var forma in formas)
                {
                    detalleBuilder.AppendLine();
                    detalleBuilder.AppendFormat(
                        "{0}: total cobrado ${1} - comisión {2}% = ${3}",
                        forma.Nombre,
                        FormatearImporte(forma.TotalCobrado),
                        FormatearPorcentaje(forma.Porcentaje),
                        FormatearImporte(forma.ImporteComision));
                }

                int idSucursalGuardar = idSucursal > 0 ? idSucursal : user.IdSucursal;
                var resultado = GuardarEgresoCajaCore(
                    0,
                    fechaEgreso,
                    idTipoEgresoCaja,
                    descripcion,
                    totalComisiones.ToString(CultureInfo.InvariantCulture),
                    detalleBuilder.ToString(),
                    idSucursalGuardar,
                    desdePos,
                    idCierre);

                return Json(new { ok = resultado.Ok, id = resultado.Id, mensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar comisiones electrónicas. " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarTipoEgresoCaja(TipoEgresoCajaEditVm model)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida." });

                if (!PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.TiposAltaEdicion, DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para administrar tipos de egreso." });

                string nombre = model != null ? (model.TipoEgresoCaja ?? "").Trim() : "";
                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { ok = false, mensaje = "El campo Tipo no puede ser vacío." });

                int id = model != null ? model.Id : 0;
                if (id > 0)
                {
                    DataTable dt = oCierreN.obtenerTiposEgresoCaja("", id);
                    if (dt == null || dt.Rows.Count == 0)
                        return Json(new { ok = false, mensaje = "No se encontró el tipo de egreso seleccionado." });

                    bool reservado = dt.Rows[0].Table.Columns.Contains("Reservado") &&
                                     dt.Rows[0]["Reservado"] != DBNull.Value &&
                                     Convert.ToBoolean(dt.Rows[0]["Reservado"]);
                    if (reservado)
                        return Json(new { ok = false, mensaje = "El tipo de egreso seleccionado es reservado por el sistema y no puede modificarse." });
                }

                oCierreN.addOrEditTipoEgreso(id > 0 ? id : -1, nombre, model != null && model.EsGasto);
                return Json(new { ok = true, mensaje = "El Tipo Egreso se registró correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarTipoEgresoCaja(int id)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida." });

                if (!PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.TiposAltaEdicion, DateTime.Today, user.Id))
                    return Json(new { ok = false, mensaje = "No tiene permisos para administrar tipos de egreso." });

                if (!user.Admin)
                    return Json(new { ok = false, mensaje = "Debe tener permiso de Administrador para eliminar un Tipo Egreso." });

                DataTable dt = oCierreN.obtenerTiposEgresoCaja("", id);
                if (dt == null || dt.Rows.Count == 0)
                    return Json(new { ok = false, mensaje = "No se encontró el tipo de egreso seleccionado." });

                bool reservado = dt.Rows[0].Table.Columns.Contains("Reservado") &&
                                 dt.Rows[0]["Reservado"] != DBNull.Value &&
                                 Convert.ToBoolean(dt.Rows[0]["Reservado"]);
                if (reservado)
                    return Json(new { ok = false, mensaje = "El Tipo Egreso seleccionado es reservado por el sistema y no puede eliminarse." });

                oCierreN.eliminarTipoEgreso(id);
                return Json(new { ok = true, mensaje = "El Tipo Egreso se eliminó correctamente." });
            }
            catch (Exception ex)
            {
                string msg = ex.Message != null && ex.Message.IndexOf("FK", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "No se puede eliminar porque existen egresos de caja con el Tipo Egreso seleccionado."
                    : ex.Message;
                return Json(new { ok = false, mensaje = msg });
            }
        }

        public ActionResult ObtenerDatosCierre(int id)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.Cajas.CerrarCaja))
            {
                ViewBag.Seccion = "Cierrar Caja";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            Entidades.CierreCaja oCierreE = new CierreCaja();
            oCierreE.Id = id;
            var caja = oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindById, "");// oCajasN.obtenerDatosCierre(id);

            bool esModificarCaja = false;

            return Json(new
            {
                id = caja.Id,
                suc = caja.Sucursal.sucursal,
                vendedor = caja.UsuarioInicio.Nombre,
                cajaInicial = caja.CajaInicio,
                fechaApertura = caja.FechaHoraInicio.Value.ToString("dd/MM/yyyy HH:mm"),
                usuario = caja.UsuarioCierre.Nombre,
                ventas = oCierreN.obtenerTotalVentas(oCierreE.UsuarioInicio.Id, oCierreE.Sucursal.idSucursal,
                        oCierreE.FechaHoraInicio, esModificarCaja ? oCierreE.FechaHoraCierre : DateTime.Now).ToString(),
                egresosCaja = oCierreN.getMontoEgresosCajaVendedor(oCierreE)
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PreviewCambioSucursalCaja(int idCierre, int idSucursalNueva)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesion invalida." }, JsonRequestBehavior.AllowGet);

            var sucursales = oSucursalN.findAll();
            if (!PuedeCambiarSucursalCaja(user, sucursales))
                return Json(new { ok = false, mensaje = "No tiene permisos para cambiar la sucursal de una caja." }, JsonRequestBehavior.AllowGet);

            var cierre = oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return Json(new { ok = false, mensaje = "No se encontro la caja seleccionada." }, JsonRequestBehavior.AllowGet);
            if (!CajaSigueAbierta(cierre))
                return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." }, JsonRequestBehavior.AllowGet);

            var preview = oCierreN.obtenerPreviewCambioSucursalCaja(cierre, idSucursalNueva);
            return Json(new
            {
                ok = true,
                puedeEjecutar = preview.PuedeEjecutar,
                mensaje = preview.Mensaje,
                tieneCajaAbiertaEnDestino = preview.TieneCajaAbiertaEnDestino,
                idCierreCaja = preview.IdCierreCaja,
                sucursalActual = preview.SucursalActual,
                sucursalNueva = preview.SucursalNueva,
                usuarioCaja = preview.UsuarioCaja,
                fechaDesde = preview.FechaDesde.ToString("dd/MM/yyyy HH:mm"),
                fechaHasta = preview.FechaHasta.ToString("dd/MM/yyyy HH:mm"),
                tablas = preview.Tablas.Select(t => new { tabla = t.Tabla, cantidad = t.Cantidad }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CambiarSucursalCaja(CambioSucursalCajaPostVm model)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesion invalida." });

            var sucursales = oSucursalN.findAll();
            if (!PuedeCambiarSucursalCaja(user, sucursales))
                return Json(new { ok = false, mensaje = "No tiene permisos para cambiar la sucursal de una caja." });

            if (model == null || model.IdCierre <= 0 || model.IdSucursalNueva <= 0)
                return Json(new { ok = false, mensaje = "Datos invalidos para cambiar la sucursal." });

            var cierre = oCierreN.findByIdOrLast(new CierreCaja { Id = model.IdCierre }, CierreCaja.tipoBusqueda.FindById, "");
            if (cierre == null || cierre.Id == 0)
                return Json(new { ok = false, mensaje = "No se encontro la caja seleccionada." });
            if (!CajaSigueAbierta(cierre))
                return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." });

            var resultado = oCierreN.cambiarSucursalCaja(cierre, model.IdSucursalNueva, user.Id, user.Nombre);
            return Json(new
            {
                ok = resultado.Ok,
                mensaje = resultado.Mensaje,
                tablas = resultado.Tablas.Select(t => new { tabla = t.Tabla, cantidad = t.Cantidad }).ToList()
            });
        }

        public ActionResult CerrarCaja(
                int Id,
                string CajaCierre,
                string Diferencia,
                string ImporteRetirado,
                string CajaInicioSiguiente
            )
        {

            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.Cajas.CerrarCaja))
            {
                ViewBag.Seccion = "Cierrar Caja";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            Entidades.CierreCaja model = oCierreN.findByIdOrLast(
                new CierreCaja { Id = Id },
                Entidades.CierreCaja.tipoBusqueda.FindById,
                ""
            );

            bool esModificarCaja = false;

            model.CajaCierre = ParseFloat(CajaCierre);
            model.Diferencia = ParseFloat(Diferencia);
            model.ImporteRetirado = ParseFloat(ImporteRetirado);
            model.CajaInicioSiguiente = ParseFloat(CajaInicioSiguiente);
            model.UsuarioCierre = (Entidades.Usuario)Session["Usuario"];
            model.FechaHoraCierre = model.FechaHoraCierre != null ? model.FechaHoraCierre : DateTime.Now;
            model.Ventas = oCierreN.obtenerTotalVentas(model.UsuarioInicio.Id, model.Sucursal.idSucursal,
                    model.FechaHoraInicio, esModificarCaja ? model.FechaHoraCierre : DateTime.Now);
            model.EgresosCaja = oCierreN.getMontoEgresosCajaVendedor(model);

            var result = oCierreN.addOrEditCierreCaja_Result(model);

            if (!result.Ok)
                return Json(new { ok = false, error = result.Mensaje });

            return Json(new { ok = true, result.Mensaje });
        }

        [HttpPost]
        public JsonResult AbrirCaja(string cajaInicio, string fechaHora)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida" });

                float cajaInicio_ = ParseFloat(cajaInicio);
                if (cajaInicio_ <= 0)
                    return Json(new { ok = false, mensaje = "Importe inválido" });

                // Parsear fecha dd/MM/yyyy HH:mm
                if (!DateTime.TryParseExact(
                        fechaHora,
                        "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fechaApertura))
                {
                    return Json(new { ok = false, mensaje = "Fecha inválida" });
                }


                // Crear entidad
                var nuevoCierre = new Entidades.CierreCaja
                {
                    Sucursal = oSucursalN.findById(user.IdSucursal),
                    UsuarioInicio = user,
                    FechaHoraInicio = fechaApertura,
                    CajaInicio = cajaInicio_
                };


                // 🔒 Revalidar caja abierta

                // Busco último cierre
                Entidades.CierreCaja cierre = oCierreN.findByIdOrLast(
                    nuevoCierre,
                    Entidades.CierreCaja.tipoBusqueda.FindLast,
                    ""
                );

                // ¿Hay caja abierta?
                bool cajaAbierta = cierre != null &&
                                   (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);

                if (cierre != null && cierre.UsuarioCierre == null)
                    return Json(new { ok = false, mensaje = "Ya existe una caja abierta" });


                oCierreN.addOrEditCierreCaja(nuevoCierre);

                return Json(new { ok = true });

            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message});
            }
        }




        private float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(",", "."); // unifica formato

            float result;
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }

        private decimal ParseDecimalFlexible(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            string limpio = value.Trim()
                .Replace("$", "")
                .Replace(" ", "");

            decimal numero;
            if (decimal.TryParse(limpio, NumberStyles.Any, new CultureInfo("es-AR"), out numero))
                return numero;

            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
                return numero;

            int ultimaComa = limpio.LastIndexOf(',');
            int ultimoPunto = limpio.LastIndexOf('.');
            if (ultimaComa >= 0 && ultimoPunto >= 0)
            {
                char separadorDecimal = ultimaComa > ultimoPunto ? ',' : '.';
                limpio = separadorDecimal == ','
                    ? limpio.Replace(".", "").Replace(',', '.')
                    : limpio.Replace(",", "");
            }
            else
            {
                limpio = limpio.Replace(',', '.');
            }

            return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out numero)
                ? numero
                : 0m;
        }

        private void CargarViewBagsEgresos(int idSucursal, int idUsuario, int idTipoEgresoCaja, string descripcion, DateTime fechaDesde, DateTime fechaHasta, bool soloGastos, bool desdePos)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.Usuarios = ObtenerUsuariosFiltroEgresos();
            ViewBag.TiposEgresoCaja = oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdUsuario = idUsuario;
            ViewBag.IdTipoEgresoCaja = idTipoEgresoCaja;
            ViewBag.Descripcion = descripcion ?? "";
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.SoloGastos = soloGastos;
            ViewBag.DesdePOS = desdePos;
            ViewBag.UsuarioAdmin = user != null && user.Admin;
            ViewBag.PuedeVerTiposEgreso = PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.EgresosCaja.TiposConsulta);
            ViewBag.PuedeEditarTiposEgreso = user != null && PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.TiposAltaEdicion, DateTime.Today, user.Id);
        }

        private void CargarViewBagsFormularioEgreso(bool desdePos, int idSucursal, CierreCaja cierreContexto = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            bool usuarioAdmin = user != null && user.Admin;

            ViewBag.DesdePOS = desdePos;
            ViewBag.UsuarioActual = cierreContexto != null && cierreContexto.UsuarioInicio != null
                ? cierreContexto.UsuarioInicio
                : user;
            ViewBag.UsuarioOperando = user;
            ViewBag.UsuarioAdmin = usuarioAdmin;
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.TiposEgresoCaja = oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdCierreActividad = cierreContexto != null ? cierreContexto.Id : 0;
        }

        private CierreCaja ObtenerCajaAbiertaUsuario(Entidades.Usuario user)
        {
            if (user == null || user.IdSucursal == 0)
                return null;

            if (user.Sucursal == null)
                user.Sucursal = oSucursalN.findById(user.IdSucursal);

            var cierre = new CierreCaja
            {
                Sucursal = user.Sucursal,
                UsuarioInicio = user
            };

            cierre = oCierreN.findByIdOrLast(cierre, CierreCaja.tipoBusqueda.FindLast, "");

            bool abierta = cierre != null && cierre.UsuarioCierre != null && cierre.UsuarioCierre.Id == 0;
            return abierta ? cierre : null;
        }

        private DataTable FiltrarSoloGastos(DataTable dt, bool soloGastos, DataTable tiposEgresoCaja)
        {
            if (!soloGastos || dt == null)
                return dt;

            if (dt.Columns.Contains("Gasto"))
            {
                DataRow[] rows = dt.Select("Gasto = true");
                return rows.Length > 0 ? rows.CopyToDataTable() : dt.Clone();
            }

            if (!dt.Columns.Contains("TipoEgresoCaja") || tiposEgresoCaja == null || !tiposEgresoCaja.Columns.Contains("Es_Gasto"))
                return dt;

            var tiposGasto = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow tipo in tiposEgresoCaja.Rows)
            {
                string valor = Convert.ToString(tipo["Es_Gasto"]);
                bool esGasto = string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase) || valor == "1";
                if (esGasto)
                    tiposGasto.Add(Convert.ToString(tipo["tipoEgresoCaja"]));
            }

            DataTable filtrado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                if (tiposGasto.Contains(Convert.ToString(row["TipoEgresoCaja"])))
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        private DataTable FiltrarEgresosRealesVendedor(DataTable dt, bool soloEgresos)
        {
            if (!soloEgresos || dt == null || !dt.Columns.Contains("TipoEgresoCaja"))
                return dt;

            DataTable filtrado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                string tipo = Convert.ToString(row["TipoEgresoCaja"]);
                bool esPagoElectronico = string.Equals(tipo, "Pago Electronico", StringComparison.OrdinalIgnoreCase);
                bool esCtaCte = string.Equals(tipo, "Cta Cte", StringComparison.OrdinalIgnoreCase);

                if (!esPagoElectronico && !esCtaCte)
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        private DataTable FiltrarActividadesCaja(DataTable dt, string filtroActividad)
        {
            if (dt == null)
                return dt;

            string filtro = string.IsNullOrWhiteSpace(filtroActividad) ? "todos" : filtroActividad;
            if (string.Equals(filtro, "todos", StringComparison.OrdinalIgnoreCase))
                return dt;

            DataTable filtrado = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                bool incluir = false;

                if (string.Equals(filtro, "gastos", StringComparison.OrdinalIgnoreCase))
                    incluir = EsGastoCaja(row) || (EsPagoCobro(row) && TieneMovimientoCaja(row));
                else if (string.Equals(filtro, "pagoElectronico", StringComparison.OrdinalIgnoreCase))
                    incluir = EsPagoElectronico(row);
                else if (string.Equals(filtro, "ctaCte", StringComparison.OrdinalIgnoreCase))
                    incluir = EsCtaCte(row) || EsPagoCobro(row);

                if (incluir)
                    filtrado.ImportRow(row);
            }

            return filtrado;
        }

        private bool EsPagoElectronico(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            return string.Equals(tipo, "Pago Electronico", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsCtaCte(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            return string.Equals(tipo, "Cta Cte", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsPagoCobro(DataRow row)
        {
            string tipo = ValorString(row, "TipoEgresoCaja");
            if (string.IsNullOrWhiteSpace(tipo))
                return false;

            string tipoNormalizado = tipo.Replace(" ", "");
            return tipoNormalizado.IndexOf("Pago", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                   tipoNormalizado.IndexOf("Cobro", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool EsGastoCaja(DataRow row)
        {
            if (row == null || row.Table == null)
                return false;

            if (EsPagoCobro(row))
                return false;

            if (row.Table.Columns.Contains("Gasto"))
            {
                string valor = Convert.ToString(row["Gasto"]);
                return string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase) || valor == "1";
            }

            return !EsPagoElectronico(row) && !EsCtaCte(row);
        }

        private bool TieneMovimientoCaja(DataRow row)
        {
            decimal monto;
            string valor = ValorString(row, "Monto");
            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
            {
                return monto != 0;
            }

            return false;
        }

        private decimal CalcularTotal(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("Monto"))
                return 0;

            decimal total = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal monto;
                if (decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                    decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
                {
                    total += monto;
                }
            }

            return total;
        }

        private decimal CalcularTotalGastosCaja(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("Monto"))
                return 0;

            decimal total = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (!EsGastoCaja(row) && !(EsPagoCobro(row) && TieneMovimientoCaja(row)))
                    continue;

                decimal monto;
                if (decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.CurrentCulture, out monto) ||
                    decimal.TryParse(Convert.ToString(row["Monto"]), NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
                {
                    total += monto;
                }
            }

            return total;
        }

        private bool PuedeCambiarSucursalCaja(Entidades.Usuario user, List<Entidades.Sucursal> sucursales = null)
        {
            if (user == null)
                return false;

            bool tienePermiso = user.Admin || PermisosHelper.TienePermisoVer(Session, PermisosPantallasWeb.Cajas.CerrarCaja);
            int cantidadSucursales = sucursales != null ? sucursales.Count : oSucursalN.findAll().Count;
            return tienePermiso && cantidadSucursales > 1;
        }

        private int ValorInt(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0;

            int valor;
            return int.TryParse(Convert.ToString(row[columna]), out valor) ? valor : 0;
        }

        private string ValorString(DataRow row, string columna)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return "";

            return Convert.ToString(row[columna]);
        }

        private void CargarPermisosEdicionEgresos(DataTable dt, bool desdePos, CierreCaja cierreContexto = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            var idsModificables = new HashSet<int>();
            var pagosModificables = new Dictionary<int, int>();

            if (user == null || dt == null || !dt.Columns.Contains("id"))
            {
                ViewBag.IdsEgresosModificables = idsModificables;
                ViewBag.PagosModificables = pagosModificables;
                return;
            }

            var ids = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id > 0)
                    ids.Add(id);
            }

            var egresosPorId = oCierreN.getEgresosCajaByIds(ids)
                .Where(x => x != null && x.Id > 0)
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id <= 0)
                    continue;

                Entidades.EgresoCaja egreso;
                if (!egresosPorId.TryGetValue(id, out egreso))
                    continue;

                bool esPagoCobro = egreso != null &&
                                   egreso.Id > 0 &&
                                   string.Equals(egreso.Tabla, Entidades.EgresoCaja.tablas.Pagos.ToString(), StringComparison.OrdinalIgnoreCase) &&
                                   egreso.IdTabla.HasValue &&
                                   egreso.IdTabla.Value > 0;

                if (esPagoCobro)
                    pagosModificables[id] = egreso.IdTabla.Value;

                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egreso, desdePos, empresa, oCierreN.validarCajaAbiertaVendedor);
                if (validacion.PuedeModificar && (cierreContexto == null || FechaDentroDeCaja(egreso.Fecha, cierreContexto)))
                    idsModificables.Add(id);
            }

            ViewBag.IdsEgresosModificables = idsModificables;
            ViewBag.PagosModificables = pagosModificables;
        }

        private DataTable ObtenerUsuariosFiltroEgresos()
        {
            var oUsuarioD = new Datos.Usuario(empresa);
            var dtUsuarios = oUsuarioD.obtenerUsuarios(true);

            if (dtUsuarios != null && dtUsuarios.Columns.Contains("id") && dtUsuarios.Columns.Contains("nombre"))
            {
                DataRow drTodos = dtUsuarios.NewRow();
                drTodos["id"] = -1;
                drTodos["nombre"] = "Todos";
                dtUsuarios.Rows.Add(drTodos);
                dtUsuarios.DefaultView.Sort = "id";
            }

            return dtUsuarios;
        }

        private bool CajaSigueAbierta(CierreCaja cierre)
        {
            return cierre != null &&
                   cierre.Id > 0 &&
                   (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
        }

        private bool FechaDentroDeCaja(DateTime fecha, CierreCaja cierre)
        {
            if (cierre == null || cierre.FechaHoraInicio == null)
                return false;

            DateTime inicio = cierre.FechaHoraInicio.Value;
            DateTime fin = cierre.FechaHoraCierre ?? DateTime.Now;
            return fecha >= inicio && fecha <= fin;
        }

        private GuardarEgresoCajaResultado GuardarEgresoCajaCore(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos, int idCierre)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Sesión inválida" };

            if (idTipoEgresoCaja <= 0)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Seleccione un tipo de egreso." };

            if (string.IsNullOrWhiteSpace(descripcion))
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Ingrese una descripción." };

            float importe = ParseFloat(monto);
            if (importe <= 0)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Ingrese un monto válido." };

            CierreCaja cierreContexto = idCierre > 0
                ? oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                : null;

            if (idCierre > 0)
            {
                if (cierreContexto == null || cierreContexto.Id == 0)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No se encontró la caja seleccionada." };

                if (!CajaSigueAbierta(cierreContexto))
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La caja seleccionada ya no se encuentra abierta." };
            }

            if (desdePos)
                idSucursal = user.IdSucursal;
            else if (cierreContexto != null && cierreContexto.Sucursal != null)
                idSucursal = cierreContexto.Sucursal.idSucursal;

            EgresoCaja egresoAnterior = id > 0 ? oCierreN.getEgresoCajaById(id) : null;
            if (id > 0 && (egresoAnterior == null || egresoAnterior.Id == 0))
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No se encontró el egreso de caja a modificar." };

            var sucursal = oSucursalN.findById(idSucursal);
            if (sucursal == null)
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "Sucursal inválida." };

            if (desdePos)
            {
                bool cajaAbierta = oCierreN.validarCajaAbiertaVendedor(fecha, sucursal, user);
                if (!cajaAbierta)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La fecha y hora del egreso debe corresponder a una caja abierta del vendedor." };
            }
            else if (cierreContexto != null && !FechaDentroDeCaja(fecha, cierreContexto))
            {
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "La fecha y hora del egreso debe corresponder a la caja abierta seleccionada." };
            }
            else if (id == 0 && !PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.AltaEdicion, fecha, user.Id))
            {
                return new GuardarEgresoCajaResultado { Ok = false, Mensaje = "No tiene permisos para registrar egresos de caja." };
            }

            if (egresoAnterior != null)
            {
                var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egresoAnterior, desdePos, empresa, oCierreN.validarCajaAbiertaVendedor);
                if (!validacion.PuedeModificar)
                    return new GuardarEgresoCajaResultado { Ok = false, Mensaje = validacion.MensajeBloqueo };
            }

            var egreso = new EgresoCaja
            {
                Id = id,
                Fecha = fecha,
                IdTipoEgresoCaja = idTipoEgresoCaja,
                Descripcion = descripcion ?? "",
                Detalle = detalle ?? "",
                Monto = importe,
                Sucursal = sucursal,
                IdCompra = egresoAnterior != null ? egresoAnterior.IdCompra : null,
                Tabla = egresoAnterior != null ? egresoAnterior.Tabla : null,
                IdTabla = egresoAnterior != null ? egresoAnterior.IdTabla : null,
                CreadoPor = egresoAnterior != null
                    ? egresoAnterior.CreadoPor
                    : (cierreContexto != null && cierreContexto.UsuarioInicio != null ? cierreContexto.UsuarioInicio.Id : user.Id),
                ActualizadoPor = id > 0 ? user.Id : 0
            };

            egreso = oCierreN.addOrEditEgresoCaja(egreso);

            return new GuardarEgresoCajaResultado
            {
                Ok = true,
                Id = egreso.Id,
                Mensaje = id > 0 ? "El egreso de caja se modificó correctamente." : "El egreso de caja se guardó correctamente."
            };
        }

        private CalcularComisionesElectronicasVm CrearModelComisionesElectronicas(DateTime fechaDesde, DateTime fechaHasta, DateTime fechaEgreso, int idSucursal, bool desdePos, int idCierre)
        {
            var sucursal = idSucursal > 0 ? oSucursalN.findById(idSucursal) : null;
            var porcentajesDefault = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "Debito", Convert.ToDecimal(param.GetFloat(ParamKeys.ComisionDebito, 0f)) },
                { "Credito", Convert.ToDecimal(param.GetFloat(ParamKeys.ComisionCredito, 0f)) },
                { "Qr", 0m },
                { "Transferencia", 0m }
            };

            var formas = ObtenerFormasPagoElectronicas(fechaDesde, fechaHasta, idSucursal, porcentajesDefault);
            return new CalcularComisionesElectronicasVm
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                FechaEgreso = fechaEgreso,
                IdSucursal = idSucursal,
                SucursalNombre = sucursal != null ? sucursal.SucursalNombre : "Todas",
                IdTipoEgresoCaja = 0,
                DesdePos = desdePos,
                IdCierre = idCierre,
                FormasPago = formas,
                TotalEgreso = formas.Sum(f => f.ImporteComision)
            };
        }

        private List<ComisionElectronicaFormaVm> ObtenerFormasPagoElectronicas(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, IDictionary<string, decimal> porcentajes)
        {
            var items = new List<ComisionElectronicaFormaVm>
            {
                new ComisionElectronicaFormaVm { Codigo = "Debito", Nombre = "Débito", Porcentaje = ObtenerPorcentaje(porcentajes, "Debito") },
                new ComisionElectronicaFormaVm { Codigo = "Credito", Nombre = "Crédito", Porcentaje = ObtenerPorcentaje(porcentajes, "Credito") },
                new ComisionElectronicaFormaVm { Codigo = "Qr", Nombre = "QR", Porcentaje = ObtenerPorcentaje(porcentajes, "Qr") },
                new ComisionElectronicaFormaVm { Codigo = "Transferencia", Nombre = "Transferencia", Porcentaje = ObtenerPorcentaje(porcentajes, "Transferencia") }
            };

            var ventas = oVentaN.getAllVentas(fechaDesde.Date, fechaHasta.Date, "", -1, -1, idSucursal, false, false) ?? new List<Entidades.Venta>();
            foreach (var venta in ventas)
            {
                if (venta == null || string.Equals(venta.Estado ?? "", "ANULADO", StringComparison.OrdinalIgnoreCase))
                    continue;

                var item = items.FirstOrDefault(i => string.Equals(i.Codigo, venta.FormaPago ?? "", StringComparison.OrdinalIgnoreCase));
                if (item == null)
                    continue;

                decimal totalVenta = Convert.ToDecimal(venta.TotalImporte);
                decimal pagoMixtoEfectivo = Convert.ToDecimal(venta.PagoMixtoEfectivo);
                decimal totalElectronico = pagoMixtoEfectivo > 0m ? totalVenta - pagoMixtoEfectivo : totalVenta;
                if (totalElectronico <= 0m)
                    continue;

                item.TotalCobrado += totalElectronico;
            }

            foreach (var item in items)
            {
                item.ImporteComision = decimal.Round(item.TotalCobrado * item.Porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
            }

            return items;
        }

        private decimal ObtenerPorcentaje(IDictionary<string, decimal> porcentajes, string codigo)
        {
            if (porcentajes == null || string.IsNullOrWhiteSpace(codigo))
                return 0m;

            decimal valor;
            return porcentajes.TryGetValue(codigo, out valor) ? valor : 0m;
        }

        private string FormatearImporte(decimal valor)
        {
            return valor.ToString("N2", new CultureInfo("es-AR"));
        }

        private string FormatearPorcentaje(decimal valor)
        {
            return valor.ToString("0.##", new CultureInfo("es-AR"));
        }

        private string ObtenerDescripcionSucursal(int idSucursal)
        {
            if (idSucursal <= 0)
                return "Todas";

            var sucursal = oSucursalN.findById(idSucursal);
            return sucursal != null && !string.IsNullOrWhiteSpace(sucursal.SucursalNombre)
                ? sucursal.SucursalNombre
                : "Todas";
        }

    }
}
