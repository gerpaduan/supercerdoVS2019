using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Negocio;
using Entidades;

namespace Web.Controllers
{
    public class ProductosController : Controller
    {
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Corte oCorteN = new Negocio.Corte();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        // LISTADO
        public ActionResult Index()
        {
            int idSucursal = 2;
            var productos = oCorteN.findAllCortes(false, idSucursal, true);
            return View(productos);
        }

        // ===============================
        // GET: CREAR
        // ===============================
        public ActionResult Crear()
        {
            var model = new Entidades.Corte();

            CargarCombos(model.IdCorte);
            return View("AddOrEdit", model);
        }

        // ===============================
        // GET: EDITAR
        // ===============================
        public ActionResult Edit(int id)
        {
            var model = oCorteN.findCorteById(id, true);
            if (model == null) return HttpNotFound();

            CargarCombos(id);
            return View("AddOrEdit", model);
        }

        // ===============================
        // GET: AddOrEdit
        // ===============================
        public ActionResult AddOrEdit(int id = 0)
        {
            Entidades.Corte model;

            if (id == 0)
            {
                // Nuevo producto
                model = new Entidades.Corte();
            }
            else
            {
                // Editar producto
                model = oCorteN.findCorteById(id, true);
                if (model == null)
                    return HttpNotFound();
            }

            CargarCombos(id);
            return View(model);  // Busca Views/Productos/AddOrEdit.cshtml
        }

        // ===============================
        // POST: CREATE / EDIT
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOrEdit(Entidades.Corte model)
        {
            //TODO: validar los datos ingresados
            //if (!ModelState.IsValid)
            //{
            //    CargarCombos(model.IdCorte);
            //    return View("AddOrEdit", model);
            //}

            try
            {
                oCorteN.addOrEditCorte(model);

                TempData["Success"] = "Corte guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                CargarCombos(model.IdCorte);
                return View("AddOrEdit", model);
            }
        }

        // ===============================
        // CARGA DE COMBOS
        // ===============================
        private void CargarCombos(int idCorte)
        {
            ViewBag.Marcas = ObtenerListaMarcas();
            ViewBag.Tipos = ObtenerListaTipos();
            ViewBag.Proveedores = ObtenerProveedores(idCorte);
        }

        private IEnumerable<SelectListItem> ObtenerListaMarcas()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Marca A" },
                new SelectListItem { Value = "2", Text = "Marca B" }
            };
        }

        private IEnumerable<SelectListItem> ObtenerListaTipos()
        {
            DataTable dt = oCorteN.obtenerTiposProducto(false);
            var lista = dt.AsEnumerable()
               .Select(row => new SelectListItem
               {
                   Value = row["Tipo"].ToString(),
                   Text = row["Tipo"].ToString()
               }).ToList();

            lista.Insert(0, new SelectListItem { Value = "", Text = "-- Seleccione --" });

            return lista;
        }

        private IEnumerable<object> ObtenerProveedores(int idCorte)
        {
            return new List<dynamic>
                {
                    new {
                        RazonSocial = "Prov A",
                        UltimoPrecio = "5000",
                        FecCompra = "2024-11-01"
                    }
                };
        }
    }
}
