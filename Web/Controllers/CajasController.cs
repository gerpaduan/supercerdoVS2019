using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Entidades;
using System.Data;
using System.Globalization;
using Web.Helpers;
using Utilidades;
using System.Linq;

namespace Web.Controllers
{
    public class CajasController : BaseController
    {
        Negocio.CierreCaja oCierreN;
        Negocio.Sucursal oSucursalN;
        Negocio.Usuario oUsuarioN;

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            oCierreN = new Negocio.CierreCaja(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);
            oUsuarioN = new Negocio.Usuario(empresa);
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
                ViewBag.MensajePermiso = "No tiene permisos para ver egresos de caja.";
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

        public ActionResult EditarPagoActividad(int id, string returnUrl = "", bool desdePos = false)
        {
            return new HttpStatusCodeResult(403, "La modificación de pagos y cobros no está disponible desde Mis actividades.");
        }

        [HttpPost]
        public JsonResult GuardarEgresoCaja(int id, DateTime fecha, int idTipoEgresoCaja, string descripcion, string monto, string detalle, int idSucursal, bool desdePos = false, int idCierre = 0)
        {
            try
            {
                var user = Session["Usuario"] as Entidades.Usuario;
                if (user == null)
                    return Json(new { ok = false, mensaje = "Sesión inválida" });

                if (idTipoEgresoCaja <= 0)
                    return Json(new { ok = false, mensaje = "Seleccione un tipo de egreso." });

                if (string.IsNullOrWhiteSpace(descripcion))
                    return Json(new { ok = false, mensaje = "Ingrese una descripción." });

                float importe = ParseFloat(monto);
                if (importe <= 0)
                    return Json(new { ok = false, mensaje = "Ingrese un monto válido." });

                CierreCaja cierreContexto = idCierre > 0
                    ? oCierreN.findByIdOrLast(new CierreCaja { Id = idCierre }, CierreCaja.tipoBusqueda.FindById, "")
                    : null;

                if (idCierre > 0)
                {
                    if (cierreContexto == null || cierreContexto.Id == 0)
                        return Json(new { ok = false, mensaje = "No se encontró la caja seleccionada." });

                    if (!CajaSigueAbierta(cierreContexto))
                        return Json(new { ok = false, mensaje = "La caja seleccionada ya no se encuentra abierta." });
                }

                if (desdePos)
                    idSucursal = user.IdSucursal;
                else if (cierreContexto != null && cierreContexto.Sucursal != null)
                    idSucursal = cierreContexto.Sucursal.idSucursal;

                EgresoCaja egresoAnterior = id > 0 ? oCierreN.getEgresoCajaById(id) : null;
                if (id > 0 && (egresoAnterior == null || egresoAnterior.Id == 0))
                    return Json(new { ok = false, mensaje = "No se encontró el egreso de caja a modificar." });

                var sucursal = oSucursalN.findById(idSucursal);
                if (sucursal == null)
                    return Json(new { ok = false, mensaje = "Sucursal inválida." });

                if (desdePos)
                {
                    bool cajaAbierta = oCierreN.validarCajaAbiertaVendedor(fecha, sucursal, user);
                    if (!cajaAbierta)
                        return Json(new { ok = false, mensaje = "La fecha y hora del egreso debe corresponder a una caja abierta del vendedor." });
                }
                else if (cierreContexto != null && !FechaDentroDeCaja(fecha, cierreContexto))
                {
                    return Json(new { ok = false, mensaje = "La fecha y hora del egreso debe corresponder a la caja abierta seleccionada." });
                }
                else if (id == 0 && !PermisosHelper.TienePermisoEditar(Session, PermisosPantallasWeb.EgresosCaja.AltaEdicion, fecha, user.Id))
                {
                    return Json(new { ok = false, mensaje = "No tiene permisos para registrar egresos de caja." });
                }

                if (egresoAnterior != null)
                {
                    var validacion = EgresosCajaPolicy.EvaluarModificacion(user, egresoAnterior, desdePos, empresa, oCierreN.validarCajaAbiertaVendedor);
                    if (!validacion.PuedeModificar)
                        return Json(new { ok = false, mensaje = validacion.MensajeBloqueo });
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

                return Json(new { ok = true, id = egreso.Id, mensaje = id > 0 ? "El egreso de caja se modificó correctamente." : "El egreso de caja se guardó correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "Error al guardar egreso de caja. " + ex.Message });
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

        private void CargarViewBagsEgresos(int idSucursal, int idUsuario, int idTipoEgresoCaja, string descripcion, DateTime fechaDesde, DateTime fechaHasta, bool soloGastos, bool desdePos)
        {
            ViewBag.Sucursales = oSucursalN.findAll();
            ViewBag.Usuarios = oUsuarioN.obtenerUsuariosConTodos(true);
            ViewBag.TiposEgresoCaja = oCierreN.obtenerTiposEgresoCaja("", 0);
            ViewBag.IdSucursal = idSucursal;
            ViewBag.IdUsuario = idUsuario;
            ViewBag.IdTipoEgresoCaja = idTipoEgresoCaja;
            ViewBag.Descripcion = descripcion ?? "";
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.SoloGastos = soloGastos;
            ViewBag.DesdePOS = desdePos;
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

            foreach (DataRow row in dt.Rows)
            {
                int id = ValorInt(row, "id");
                if (id <= 0)
                    continue;

                var egreso = oCierreN.getEgresoCajaById(id);
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

    }
}
