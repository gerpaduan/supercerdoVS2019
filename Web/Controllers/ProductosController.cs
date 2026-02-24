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
        private Negocio.Persona oPersonaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            oSucursalN = new Negocio.Sucursal(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
            oUsuarioN = new Negocio.Usuario(empresa, param);
            oPersonaN = new Negocio.Persona(empresa, param);
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
        public JsonResult ListarProductos(string q = "")
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
                        id = p.IdCorte,
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
                TempData["FlashError"] = "No tenés permisos para realizar la acción seleccionada.";
                return RedirectToAction("Index");
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
        // POST Guardar
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(CorteUpsertVM vm)
        {
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                TempData["FlashError"] = "No tenés permisos para realizar la acción seleccionada.";
                return RedirectToAction("Index");
            }

            // 1) Normalizar floats desde Request.Form (acepta coma o punto)
            NormalizarFloatsDesdeRequest(vm);

            // 2) Validación server-side de código duplicado (seguridad extra)
            if (vm.Codigo > 0)
            {
                var existente = oCorteN.findCorteByCodigo(vm.Codigo, false);
                if (existente != null && existente.IdCorte != vm.IdCorte)
                {
                    ModelState.AddModelError("Codigo", $"El código ya existe para el producto: {existente.CorteDesc}");
                }
            }

            // 3) Validación de modo
            ValidarModoCorte(vm);

            if (!ModelState.IsValid)
            {
                LoadCombos(vm);
                return View("AddOrEdit", vm);
            }

            var entity = (vm.IdCorte > 0)
                ? oCorteN.findCorteById(vm.IdCorte, true)
                : new Entidades.Corte();

            if (vm.IdCorte > 0 && entity == null)
                return HttpNotFound();

            // Si querés guardar el porcentaje (sin columna), lo saco del texto:
            vm.AlicuotaIva = ObtenerAlicuotaPorcentajeDesdeDT(vm.IdAlicuotaIva);

            MapToEntity(vm, entity); // VM -> Entity

            oCorteN.addOrEditCorte(entity);

            TempData["FlashSuccess"] = $"El producto \"{vm.CorteDesc}\" guardó correctamente.";
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
                if (vm.Porcentaje <= 0) ModelState.AddModelError("", "El porcentaje debe ser mayor a 0.");
                if (vm.PorcentajeHueso < 0) ModelState.AddModelError("", "El desperdicio no puede ser negativo.");
            }

            if (vm.ModoCorte == "Presentacion")
            {
                if (vm.Porcentaje <= 0) ModelState.AddModelError("", "La presentación (unidades) debe ser un número mayor o igual a 1.");
                
            }
        }

        private void NormalizarFloatsDesdeRequest(CorteUpsertVM vm)
        {
            // Campos float de la vista
            TrySetFloatFromRequest("PrecioKg", v => vm.PrecioKg = v, "Precio Kg");
            TrySetFloatFromRequest("Promedio", v => vm.Promedio = v, "Promedio");
            TrySetFloatFromRequest("Porcentaje", v => vm.Porcentaje = v, "Porcentaje");
            TrySetFloatFromRequest("PorcentajeHueso", v => vm.PorcentajeHueso = v, "Desperdicio");
        }

        /// <summary>
        /// Lee Request.Form[key], acepta coma o punto decimal, y setea el float en VM.
        /// Si viene vacío, no agrega error (campo opcional).
        /// </summary>
        private void TrySetFloatFromRequest(string key, Action<float> setter, string label)
        {
            var raw = (Request.Form[key] ?? string.Empty).Trim();

            // Si está vacío, lo dejamos como vino del binder (normalmente 0 en float)
            // y removemos errores de parseo si existieran por tema de cultura.
            if (string.IsNullOrWhiteSpace(raw))
            {
                ModelState.Remove(key);
                return;
            }

            // Soporta coma o punto
            var normalized = raw.Replace(",", ".");

            float valor;
            bool ok = float.TryParse(
                normalized,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out valor);

            if (!ok)
            {
                ModelState.AddModelError(key, $"{label} tiene un formato numérico inválido.");
                return;
            }

            setter(valor);

            // Muy importante: sacamos el error del binder si falló por cultura
            ModelState.Remove(key);
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
                    e.Porcentaje = vm.PresentacionUnidades ?? 0;
                    e.PorcentajeHueso = 0;
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

        [HttpGet]
        public JsonResult FindCorteByCodigo(long? codigo, int? idExcluir = null)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
            {
                return Json(new
                {
                    existe = false
                }, JsonRequestBehavior.AllowGet);
            }

            var corte = oCorteN.findCorteByCodigo(codigo.Value, false);

            if (corte == null)
            {
                return Json(new
                {
                    existe = false
                }, JsonRequestBehavior.AllowGet);
            }

            // Si es edición y el código pertenece al mismo producto, no lo marcamos duplicado
            int idExc = idExcluir.GetValueOrDefault();
            if (idExc > 0 && corte.IdCorte == idExc)
            {
                return Json(new
                {
                    existe = false,
                    mismoRegistro = true,
                    id = corte.IdCorte,
                    nombre = corte.CorteDesc
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                existe = true,
                id = corte.IdCorte,
                nombre = corte.CorteDesc,
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


        // ===============================
        // EJEMPLO: POST Eliminar
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            // 1) Permisos -> cartel y volver al Index
            if (!PermisosHelper.TienePermiso(Session, Permisos.Producto.NuevoCorte, null))
            {
                TempData["FlashError"] = "No tenés permisos para realizar la acción seleccionada.";
                return RedirectToAction("Index");
            }

            // 2) Validación de Id
            if (id <= 0)
            {
                TempData["FlashError"] = "No se pudo eliminar: producto inválido.";
                return RedirectToAction("Index");
            }

            // 3) Buscar entidad real
            var entity = oCorteN.findCorteById(id, true);
            if (entity == null)
            {
                TempData["FlashError"] = "No se pudo eliminar: el producto no existe o ya fue eliminado.";
                return RedirectToAction("Index");
            }

            // 4) Guardar nombre ANTES de eliminar
            var nombre = entity.CorteDesc ?? entity.corte ?? "(sin nombre)";

            // 5) Eliminar
            oCorteN.eliminarCorte(entity);

            // 6) Mensaje OK
            TempData["FlashSuccess"] = $"El producto \"{nombre}\" se eliminó correctamente.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public JsonResult BuscarMarca(string q = "")
        {
            q = (q ?? "").Trim();

            // Si tu método con q="" ya devuelve todas, listo.
            // Si NO devuelve todas, ahí tenés que llamar a otro método "obtenerTodas".
            var dt = oPersonaN.buscarPersona(q, true); // DataTable

            var list = new List<object>();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    id = Convert.ToInt32(row["idPersona"]),          // <-- AJUSTÁ
                    nombre = Convert.ToString(row["Marca"])    // <-- AJUSTÁ
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}
