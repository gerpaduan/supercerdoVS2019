using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Entidades;
using System.Data;
using System.Globalization;
using Web.Helpers;
using Utilidades;

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
            if (!PermisosHelper.TienePermiso(Session, Permisos.Caja.CierresDeCaja, null))
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
        public ActionResult ObtenerDatosCierre(int id)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Caja.CerrarCaja, null))
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
            if (!PermisosHelper.TienePermiso(Session, Permisos.Caja.CerrarCaja, null))
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

    }
}
