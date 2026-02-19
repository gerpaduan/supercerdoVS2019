using Datos;
using Entidades;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Web.Helpers;

namespace Web.Controllers
{
    public class ProductosController : BaseController
    {
        private Negocio.Sucursal oSucursalN;
        private Negocio.Corte oCorteN;
        private Negocio.Usuario oUsuarioN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            oSucursalN = new Negocio.Sucursal(empresa);
            oCorteN = new Negocio.Corte(empresa);
            oUsuarioN = new Negocio.Usuario(empresa);
        }

        public ActionResult Index(int SucursalId = 0)
        {
            var productos = oCorteN.findAllCortes(true, SucursalId);

            var sucursales = oSucursalN.findAll(); // Obtiene List<Entidades.Sucursal>

            ViewBag.Sucursales = sucursales;
            ViewBag.SucursalId = SucursalId;

            return View(productos);
        }

        // Acción para búsqueda en vivo usada por el modal POS
        [HttpGet]
        public JsonResult ListarParaPOS(string q = "")
        {
            try
            {
                var productos = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.Trim();
                    productos = productos
                        .Where(p =>
                            (!string.IsNullOrEmpty(p.corte) && p.corte.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            p.codigo.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                // Mapeo ligero para el cliente
                var resultado = productos
                    .Select(p => new
                    {
                        codigo = p.codigo.ToString(),
                        nombre = p.corte,
                        precio = p.precioKg
                    })
                    .Take(200) // tope razonable
                    .ToList();

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Opcional: loguear ex
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
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

        #region ADDorEDIT

        // ==========================================
        // HELPERS conversion DataRow -> tipos seguros
        // ==========================================
        private static string ToStr(object o)
            => o == null || o == DBNull.Value ? "" : o.ToString();

        private static int ToInt(object o)
        {
            int v;
            int.TryParse(ToStr(o), out v);
            return v;
        }

        // Si tu campo "iva" viene como "21%" o "10,5%" esto lo convierte a float.
        // Si viene "Exento" o similar, devuelve 0.
        private static float ParseAlicuotaDesdeTextoIva(string ivaTexto)
        {
            if (string.IsNullOrWhiteSpace(ivaTexto)) return 0;

            // Busca el primer número (permite coma o punto)
            var m = Regex.Match(ivaTexto, @"(\d+(?:[.,]\d+)?)");
            if (!m.Success) return 0;

            var s = m.Groups[1].Value.Replace(",", ".");
            float v;
            float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
            return v;
        }

        // =========================
        // CARGAR COMBOS (DataTable)
        // =========================
        private void LoadCombos(CorteUpsertVM vm)
        {
            // -------- TIPOS (tabla tipos producto) --------
            DataTable dtTipos = oCorteN.obtenerTiposProducto(false);
            var tiposList = new List<SelectListItem>();

            if (dtTipos != null)
            {
                foreach (DataRow r in dtTipos.Rows)
                {
                    // Columna: tipo
                    string tipo = ToStr(r["tipo"]);

                    tiposList.Add(new SelectListItem
                    {
                        Value = tipo,
                        Text = tipo,
                        Selected = string.Equals(tipo, vm.Tipo ?? "", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            vm.Tipos = tiposList;


            // -------- IVA (tabla alicuotas iva) --------
            // SELECT idIva, iva FROM AlicuotasIva
            DataTable dtIva = oCorteN.obtenerAlicuotasIva(false);
            var ivaList = new List<SelectListItem>();

            if (dtIva != null)
            {
                foreach (DataRow r in dtIva.Rows)
                {
                    int idIva = ToInt(r["idIva"]);      // Columna: idIva
                    string desc = ToStr(r["iva"]);      // Columna: iva

                    ivaList.Add(new SelectListItem
                    {
                        Value = idIva.ToString(),
                        Text = desc,
                        Selected = (idIva == vm.IdAlicuotaIva)
                    });
                }
            }

            vm.AlicuotasIva = ivaList;
        }

        // ==========================================
        // Si querés setear vm.AlicuotaIva SIN columna:
        // lo parseo desde el texto "iva"
        // ==========================================
        private float ObtenerAlicuotaPorcentajeDesdeDT(int idIvaBuscado)
        {
            DataTable dt = oCorteN.obtenerAlicuotasIva(false);
            if (dt == null) return 0;

            foreach (DataRow r in dt.Rows)
            {
                int id = ToInt(r["idIva"]);
                if (id == idIvaBuscado)
                {
                    string ivaTexto = ToStr(r["iva"]);
                    return ParseAlicuotaDesdeTextoIva(ivaTexto);
                }
            }

            return 0;
        }

        // ===============================
        // EJEMPLO: GET AddOrEdit
        // ===============================
        public ActionResult AddOrEdit(int id = 0)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                ViewBag.Seccion = "Agregar/Modificar Productos";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            Entidades.Corte entity = (id == 0)
                ? new Entidades.Corte()
                : oCorteN.findCorteById(id, true);

            if (id > 0 && entity == null) return HttpNotFound();

            var vm = BuildVM(entity);   // Entity -> VM (tu método)
            LoadCombos(vm);

            return View(vm);
        }

        // ===============================
        // EJEMPLO: POST Guardar
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(CorteUpsertVM vm)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                ViewBag.Seccion = "Agregar/Modificar Productos";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            ValidarModoCorte(vm);

            if (!ModelState.IsValid)
            {
                LoadCombos(vm);
                return View("AddOrEdit", vm);
            }

            var entity = (vm.IdCorte > 0)
                ? oCorteN.findCorteById(vm.IdCorte, true)
                : new Entidades.Corte();

            if (vm.IdCorte > 0 && entity == null) return HttpNotFound();

            // Si querés guardar el porcentaje (sin columna), lo saco del texto:
            vm.AlicuotaIva = ObtenerAlicuotaPorcentajeDesdeDT(vm.IdAlicuotaIva);

            MapToEntity(vm, entity); // VM -> Entity (que asigne IdAlicuotaIva y AlicuotaIva si la usás)

            oCorteN.addOrEditCorte(entity);

            return RedirectToAction("Index");
        }

    // ============================================
    // 1) ValidarModoCorte
    // ============================================
    private void ValidarModoCorte(CorteUpsertVM vm)
        {
            vm.ModoCorte = (vm.ModoCorte ?? "Ninguno").Trim();

            if (vm.ModoCorte != "Ninguno" && vm.ModoCorte != "CorteMaestro" && vm.ModoCorte != "Presentacion")
            {
                ModelState.AddModelError(nameof(vm.ModoCorte), "Modo inválido.");
                return;
            }

            if (vm.ModoCorte == "Ninguno")
                return;

            // Corte maestro obligatorio en CorteMaestro o Presentacion
            if (!vm.IdCorteMaestro.HasValue || vm.IdCorteMaestro.Value <= 0)
                ModelState.AddModelError("", "Seleccioná un corte maestro.");

            if (vm.ModoCorte == "CorteMaestro")
            {
                // Si querés exigir valores > 0, descomentá
                // if (vm.Porcentaje <= 0) ModelState.AddModelError("", "El porcentaje debe ser mayor a 0.");
                // if (vm.PorcentajeHueso < 0) ModelState.AddModelError("", "El desperdicio no puede ser negativo.");
            }

            if (vm.ModoCorte == "Presentacion")
            {
                if (!vm.PresentacionUnidades.HasValue || vm.PresentacionUnidades.Value < 1)
                    ModelState.AddModelError("", "La presentación (unidades) debe ser un número mayor o igual a 1.");
            }
        }


        // ============================================
        // 2) BuildVM (Entity -> VM)
        // ============================================
        private CorteUpsertVM BuildVM(Entidades.Corte e)
        {
            var vm = new CorteUpsertVM();

            vm.IdCorte = e.IdCorte;
            vm.Codigo = e.Codigo;
            vm.CorteDesc = e.CorteDesc;
            vm.PrecioKg = e.PrecioKg;
            vm.Tipo = e.Tipo;

            vm.Pesable = e.Pesable;
            vm.Promedio = e.Promedio;

            vm.IdAlicuotaIva = e.IdAlicuotaIva;
            vm.AlicuotaIva = e.AlicuotaIva;

            vm.PuntoStock = e.PuntoStock;
            vm.EnCierreStock = e.EnCierreStock;
            vm.Habilitado = e.Habilitado;
            vm.IngresoRapidoEmbutido = e.IngresoRapidoEmbutido;

            vm.Nivel = e.Nivel;

            // Marca (puede ser null)
            vm.MarcaNombre = e.MarcaNombre;
            vm.IdMarca = (e.Marca != null) ? (int?)GetIdPersonaReflection(e.Marca) : null;

            // Independiente (int -> bool)
            vm.Independiente = (e.Independiente == 1);

            // Corte Maestro / Presentación
            if (e.CorteMaestro != null && e.CorteMaestro.IdCorte > 0)
            {
                vm.IdCorteMaestro = e.CorteMaestro.IdCorte;
                vm.CorteMaestroNombre = e.CorteMaestro.CorteDesc;

                vm.Porcentaje = e.Porcentaje;
                vm.PorcentajeHueso = e.PorcentajeHueso;

                if (e.Presentacion)
                {
                    vm.ModoCorte = "Presentacion";
                    vm.PresentacionUnidades = e.getCantPresentacion(e.PorcentajeHueso);
                }
                else
                {
                    vm.ModoCorte = "CorteMaestro";
                }
            }
            else
            {
                vm.ModoCorte = "Ninguno";
                vm.IdCorteMaestro = null;
                vm.CorteMaestroNombre = "";
                vm.Porcentaje = 0;
                vm.PorcentajeHueso = 0;
                vm.PresentacionUnidades = null;
            }

            // Si es alta y querés independiente por defecto tildado:
            if (vm.IdCorte == 0)
                vm.Independiente = true;

            return vm;
        }


        // ============================================
        // 3) MapToEntity (VM -> Entity)
        // ============================================
        private void MapToEntity(CorteUpsertVM vm, Entidades.Corte e)
        {
            // Campos principales
            e.Codigo = vm.Codigo;
            e.CorteDesc = vm.CorteDesc;
            e.PrecioKg = vm.PrecioKg;
            e.Tipo = vm.Tipo;

            e.Pesable = vm.Pesable;
            e.Promedio = vm.Promedio;

            e.IdAlicuotaIva = vm.IdAlicuotaIva;
            e.AlicuotaIva = vm.AlicuotaIva; // si lo calculás desde el texto iva, queda acá

            e.PuntoStock = vm.PuntoStock;
            e.EnCierreStock = vm.EnCierreStock;
            e.Habilitado = vm.Habilitado;
            e.IngresoRapidoEmbutido = vm.IngresoRapidoEmbutido;

            e.Independiente = vm.Independiente ? 1 : 0;

            // Marca (sin depender de oPersonaN)
            if (vm.IdMarca.HasValue && vm.IdMarca.Value > 0)
            {
                var p = new Entidades.Persona();              // <-- tu namespace real
                SetIdPersonaReflection(p, vm.IdMarca.Value);  // setea IdPersona o idPersona
                e.Marca = p;
            }
            else
            {
                e.Marca = null;
            }

            // Modo Corte
            vm.ModoCorte = (vm.ModoCorte ?? "Ninguno").Trim();

            if (vm.ModoCorte == "Ninguno")
            {
                e.CorteMaestro = null;
                e.Presentacion = false;
                e.Porcentaje = 0;
                e.PorcentajeHueso = 0;
            }
            else
            {
                // Corte maestro obligatorio
                if (!vm.IdCorteMaestro.HasValue || vm.IdCorteMaestro.Value <= 0)
                    throw new Exception("Falta seleccionar corte maestro.");

                // Seteo corte maestro por Id (sin otro query)
                var cm = new Entidades.Corte();
                cm.IdCorte = vm.IdCorteMaestro.Value;
                e.CorteMaestro = cm;

                if (vm.ModoCorte == "CorteMaestro")
                {
                    e.Presentacion = false;
                    e.Porcentaje = vm.Porcentaje;
                    e.PorcentajeHueso = vm.PorcentajeHueso;
                }
                else // Presentacion
                {
                    e.Presentacion = true;

                    // Recalculo SIEMPRE del lado servidor
                    float unidades = vm.PresentacionUnidades ?? 0;
                    if (unidades <= 0)
                    {
                        // fallback si por alguna razón no vino PresentacionUnidades:
                        // unidades = (100 + desperdicio) / 100
                        unidades = (100f + vm.PorcentajeHueso) / 100f;
                    }
                    if (unidades < 1) unidades = 1;

                    e.Porcentaje = 100f;
                    e.PorcentajeHueso = 100f * (unidades - 1f);
                }
            }

            // Fechas
            if (vm.IdCorte == 0 && e.Creado == default(DateTime))
                e.Creado = DateTime.Now;

            e.Actualizado = DateTime.Now;
        }


        // ============================================
        // Helpers Reflection Persona Id (IdPersona / idPersona)
        // ============================================
        private int GetIdPersonaReflection(object persona)
        {
            if (persona == null) return 0;
            var t = persona.GetType();

            // Propiedad
            var p1 = t.GetProperty("IdPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p1 != null) return Convert.ToInt32(p1.GetValue(persona, null) ?? 0);

            var p2 = t.GetProperty("idPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p2 != null) return Convert.ToInt32(p2.GetValue(persona, null) ?? 0);

            // Campo
            var f1 = t.GetField("IdPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f1 != null) return Convert.ToInt32(f1.GetValue(persona) ?? 0);

            var f2 = t.GetField("idPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f2 != null) return Convert.ToInt32(f2.GetValue(persona) ?? 0);

            return 0;
        }

        private void SetIdPersonaReflection(object persona, int id)
        {
            if (persona == null) return;
            var t = persona.GetType();

            // Propiedad
            var p1 = t.GetProperty("IdPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p1 != null && p1.CanWrite) { p1.SetValue(persona, id, null); return; }

            var p2 = t.GetProperty("idPersona", BindingFlags.Public | BindingFlags.Instance);
            if (p2 != null && p2.CanWrite) { p2.SetValue(persona, id, null); return; }

            // Campo
            var f1 = t.GetField("IdPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f1 != null) { f1.SetValue(persona, id); return; }

            var f2 = t.GetField("idPersona", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f2 != null) { f2.SetValue(persona, id); return; }
        }

    #endregion


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

    }
}
