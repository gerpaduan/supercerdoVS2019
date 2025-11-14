using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using Entidades;

namespace Web.Controllers
{
    public class ProductosController : Controller
    {
        // GET: Productos
        // Acción que carga la vista Index

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Corte oCorteN = new Negocio.Corte();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public ActionResult Index()
        {
            int idSucursal = 2;
            List<Entidades.Corte> productos = oCorteN.findAllCortes(false, idSucursal, true);
            // Pasamos el DataTable directamente a la vista
            return View(productos);
        }

        // Opcional: métodos para Crear, Editar, Eliminar...
        public ActionResult Crear() => View();
        public ActionResult Editar(int id) => View();
        public ActionResult Eliminar(int id) => View();

    }
}