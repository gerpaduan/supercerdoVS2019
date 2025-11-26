using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Helpers;

namespace Web.Controllers
{
    public class VentasController : Controller
    {
        Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        // GET: Ventas
        public ActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int idSucursal = -1)
        {
            // Si no envían fechas, por defecto usar hoy
            DateTime desde = fechaDesde ?? DateTime.Today;
            DateTime hasta = fechaHasta ?? DateTime.Today;

            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.VerVentas, desde))
            {
                ViewBag.Seccion = "Ventas";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }


            //si ambas fechas son iguales se suma 24 horas a fechaHasta
            if (desde == hasta && desde.Hour == 0)
            {
                hasta = hasta.AddDays(1);
            }


            var sucursales = oSucursalN.findAll(); // Obtiene List<Entidades.Sucursal>

            ViewBag.Sucursales = sucursales;
            ViewBag.IdSucursalSeleccionada = idSucursal;

            List<Entidades.Venta> ventas = oVentaN.getAllVentas(desde, hasta, "", -1, -1, idSucursal, false, false); //new List<Entidades.Venta>();

            ViewBag.TotalFiltrado = ventas.Sum(v => v.TotalImporte);
            //ventas.Add(oVentaE);
            return View(ventas);
        }
    }
}