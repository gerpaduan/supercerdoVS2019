using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Entidades;
using System.Data;
using System.Globalization;
using Web.Helpers;

namespace Web.Controllers
{
    public class CajasController : Controller
    {
        private readonly Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        private readonly Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        private readonly Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        // GET: Cajas/CajasAbiertas
        public ActionResult CajasAbiertas(int? idSucursal, string buscar = "", bool ajax = false)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Caja.CierresDeCaja, null))
            {
                ViewBag.Seccion = "Cierres de Caja";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            // Modelo inicial vacío
            var dt = new DataTable();

            // --- Sucursales para el combo ---
            var sucursales = oSucursalN.findAll();
            ViewBag.Sucursales = sucursales;
            ViewBag.IdSucursal = idSucursal;   // No lo conviertas a 0
            ViewBag.Buscar = buscar;

            // Si no eligió sucursal → mostrar vista vacía
            if (!idSucursal.HasValue)
            {
                if (ajax)
                    return PartialView("_TablaCajasAbiertas", dt);

                return View("~/Views/Cajas/CajasAbiertas.cshtml", dt);
            }

            // --- Obtener sucursal actual ---
            var sucursalActual = oSucursalN.findById(idSucursal.Value);
            if (sucursalActual == null)
            {
                if (ajax)
                    return PartialView("_TablaCajasAbiertas", dt);

                return View("~/Views/Cajas/CajasAbiertas.cshtml", dt);
            }

            // --- Armar entidad filtro ---
            var filtro = new CierreCaja
            {
                Sucursal = sucursalActual
            };

            // --- Consultar cajas abiertas ---
            dt = oCierreN.findCierreCaja(
                filtro,
                CierreCaja.tipoBusqueda.FindOpen,
                buscar,
                null
            );

            // --- Si es AJAX, devolver solo la tabla ---
            if (ajax)
                return PartialView("_TablaCajasAbiertas", dt);

            // --- Vista completa ---
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
