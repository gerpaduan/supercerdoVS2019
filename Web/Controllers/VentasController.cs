using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class VentasController : Controller
    {
        Negocio.Venta oVentaN = new Negocio.Venta();
        // GET: Ventas
        public ActionResult Index()
        {
            List<Entidades.Venta> ventas = oVentaN.getAllVentas(DateTime.Now.AddYears(-1), DateTime.Now, "", -1, -1, -1, false, false); //new List<Entidades.Venta>();
            //ventas.Add(oVentaE);
            return View(ventas);
        }
    }
}