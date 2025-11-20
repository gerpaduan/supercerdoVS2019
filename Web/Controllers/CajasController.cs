using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Entidades;
using System.Data;

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


        // POST: Cerrar Caja Individual (botón)
        [HttpPost]
        public ActionResult CerrarCaja(int id)
        {
            // Validar usuario que cierra
            var usuario = Session["UsuarioActual"] as Entidades.Usuario;
            if (usuario == null)
                return Json(new { ok = false, msg = "Debe iniciar sesión." });

            if (!oUsuarioN.tienePermiso(usuario, "formCerrarCaja",
                DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                return Json(new { ok = false, msg = "No tiene permisos." });
            }

            // Cierra la caja
            Entidades.CierreCaja cierre = new Entidades.CierreCaja();
            cierre.Id = id;

           // oCierreN.cerrarCaja(cierre, usuario);

            return Json(new { ok = true });
        }

        // POST: Cerrar múltiples cajas
        [HttpPost]
        public ActionResult CerrarMultiple(int idCajero, List<int> ids)
        {
            var usuario = Session["UsuarioActual"] as Entidades.Usuario;
            if (usuario == null)
                return Json(new { ok = false, msg = "Debe iniciar sesión." });

            if (!oUsuarioN.tienePermiso(usuario, "formCerrarCaja",
                DateTime.Today, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                return Json(new { ok = false, msg = "No tiene permisos." });
            }

            // Armar entidad del cajero seleccionado
            CierreCaja cierreCajero = new CierreCaja();
            cierreCajero.Id = idCajero;

            // Armamos la lista
            List<CierreCaja> lista = new List<CierreCaja>();
            foreach (var id in ids)
                lista.Add(new CierreCaja { Id = id });

            // Ejecutar negocio
            // oCierreN.cerrarCajasMultiples(cierreCajero, lista, usuario);

            return Json(new { ok = true });
        }
    }
}
