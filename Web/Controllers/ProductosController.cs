using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Datos;
using System.Globalization;
using Web.Helpers;

namespace Web.Controllers
{
    public class ProductosController : Controller
    {
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Corte oCorteN = new Negocio.Corte();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public ActionResult Index(int SucursalId = 0)
        {
            var productos = oCorteN.findAllCortes(false, SucursalId);

            var sucursales = oSucursalN.findAll(); // Obtiene List<Entidades.Sucursal>

            ViewBag.Sucursales = sucursales;
            ViewBag.SucursalId = SucursalId;

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
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                ViewBag.Seccion = "Agregar/Modificar Productos";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

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


        [HttpGet]
        public ActionResult findCorteById(int id)
        {
            var corte = oCorteN.findCorteById(id, true);  // ← tu método para buscar

            if (corte == null)
                return HttpNotFound();


            return Json(new
            {
                id = corte.IdCorte,
                descripcion = corte.CorteDesc,
                precio = corte.PrecioKg
            }, JsonRequestBehavior.AllowGet);
        }

        // ===============================
        // GET: modificar Precio
        // ===============================
        [HttpPost]
        public ActionResult EditPrecioCorte(int IdCorte, string PrecioKg)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.ModificarPrecios, null))
            {
                ViewBag.Seccion = "Productos - Modificar Precios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (string.IsNullOrWhiteSpace(PrecioKg))
                return Json(new { error = "Precio vacío" });

            // Normalizar: quitar miles y pasar coma a punto
            string normalizado = PrecioKg
                .Replace(".", "")
                .Replace(",", ".");

            if (!float.TryParse(
                    normalizado,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out float precioDecimal))
            {
                return Json(new { error = "Formato de precio inválido" });
            }
            Entidades.Corte model = new Entidades.Corte();
            model.idCorte = IdCorte;
            model.precioKg = precioDecimal;
            oCorteN.editPrecioCorte(model);

            return Json(new
            {
                id = IdCorte,
                precio = precioDecimal,
                precioFormateado = "$ "+precioDecimal.ToString("N2", new CultureInfo("es-AR"))
            });
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
