using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Helpers;
using static Entidades.Venta;

namespace Web.Controllers
{
    public class VentasController : Controller
    {
        Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Negocio.Persona oPersonaN = new Negocio.Persona();
        Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        Entidades.CierreCaja  oCierreE = new Entidades.CierreCaja();
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

            // 1️⃣ Enum → lista (sin Nulo)
            var formasPago = Enum.GetValues(typeof(formaPagoEnum))
                                 .Cast<formaPagoEnum>()
                                 .Where(f => f != formaPagoEnum.Nulo)
                                 .ToList();

            // 2️⃣ Todas seleccionadas por defecto
            var seleccionadas = formasPago;

            // 3️⃣ Armar MultiSelectList
            ViewBag.FormasPago = new MultiSelectList(
                formasPago.Select(f => new
                {
                    Value = f.ToString().ToLower(), // para usar en data-forma-pago
                    Text = f.ToString()
                }),
                "Value",
                "Text",
                seleccionadas.Select(f => f.ToString().ToLower())
            );

            List<Entidades.Venta> ventas = oVentaN.getAllVentas(desde, hasta, "", -1, -1, idSucursal, false, false); //new List<Entidades.Venta>();

            ViewBag.TotalFiltrado = ventas.Sum(v => v.TotalImporte);
            //ventas.Add(oVentaE);
            return View(ventas);
        }

        // GET: Ventas/DetalleVenta/5
        public ActionResult DetalleVenta(int id)
        {
            // Buscar la venta por ID
            var venta = oVentaN.getVentaById(id);
            //_context.Ventas
            //    .Include("Persona")
            //    .Include("Vendedor")
            //    .Include("Lineas.Producto")
            //    .FirstOrDefault(v => v.IdVenta == id);

            if (venta == null)
            {
                return HttpNotFound();
            }

            // Pasar la venta a la vista
            return View(venta);
        }
        [HttpPost]
        public JsonResult FinalizarVenta(
            string formaPago,
            bool esPagoMixto,
            float efectivo,
            List<LineaVenta> lineasVenta
)
        {
            try
            {
                var venta = Session["VentaActiva"] as Venta;

                if (venta == null)
                    return Json(new { ok = false, msg = "No hay venta activa" });

                if (lineasVenta == null || !lineasVenta.Any())
                    return Json(new { ok = false, msg = "No hay productos en la venta" });

                Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
                var user = Session["Usuario"] as Entidades.Usuario;
                venta.Sucursal = oSucursalN.findById(user.IdSucursal);

                venta.Observaciones = venta.Observaciones ?? "";

                // ===============================
                // FORMAS DE PAGO
                // ===============================
                venta.FormaPago = formaPago;
                venta.EnCtaCte = formaPago == formaPagoEnum.CtaCte.ToString();
                venta.PagoMixtoEfectivo = esPagoMixto ? efectivo : 0;

                Negocio.Corte oCorteN = new Negocio.Corte(); 
                for (int index = 0; index < lineasVenta.Count; index++)
                {
                    lineasVenta[index].Corte = oCorteN.findCorteByCodigo(lineasVenta[index].Codigo, false);

                    // En Negocio hace la asignacion inversa 'no recuerdo xq hice esto, posiblente por redondeo'
                    lineasVenta[index].KgsTotalCalculado = lineasVenta[index].CantKg;
                }

                // ===============================
                // LINEAS DE VENTA (CLAVE)
                // ===============================
                venta.LineasVenta = lineasVenta;

                oVentaN.agregarVenta(venta);

                Session.Remove("VentaActiva");

                return Json(new { ok = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;

                return Json(new
                {
                    ok = false,
                    msg = "Error al finalizar la venta",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        #region POS
        public ActionResult POS()
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Inicializo cierre
            var cierre = new Entidades.CierreCaja
            {
                Sucursal = oSucursalN.findById(user.IdSucursal),
                UsuarioInicio = user
            };

            // Busco último cierre
            cierre = oCierreN.findByIdOrLast(
                cierre,
                Entidades.CierreCaja.tipoBusqueda.FindLast,
                ""
            );

            // ¿Hay caja abierta?
            bool cajaAbierta = cierre != null &&
                               (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);

            // Paso info a la vista
            ViewBag.CajaAbierta = cajaAbierta;
            ViewBag.SucursalNombre = user.SucursalNombre;

            // 🚨 Si NO hay caja abierta, NO inicializo venta
            if (!cajaAbierta)
            {
                return View((Venta)null);  // La vista muestra modal y POS bloqueado
            }

            // ==========================
            // POS habilitado
            // ==========================
            var venta = Session["VentaActiva"] as Venta;

            if (venta == null)
            {
                venta = new Venta
                {
                    LineasVenta = new List<LineaVenta>()
                };
            }

            var oCliente = oPersonaN.getConsumidorFinal();

            venta.Persona = oCliente;
            venta.Vendedor = user;
            venta.FechaVenta = DateTime.Now;

            Session["VentaActiva"] = venta;

            return View(venta);
        }



        // ======================================================
        // GET /Ventas/BuscarProducto?codigo=123
        // ======================================================
        public JsonResult BuscarProducto(Int64 codigo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo.ToString()))
                    return Json(new { error = "Código vacío" }, JsonRequestBehavior.AllowGet);

                // Buscar producto (Corte)
                // Reemplazar por tu método real para obtener productos
                var gestorCortes = new Negocio.Corte();
                var corte = gestorCortes.findCorteByCodigo(codigo, false);

                if (corte == null)
                    return Json(new { error = "Producto no encontrado" }, JsonRequestBehavior.AllowGet);

                // Respuesta JSON
                return Json(new
                {
                    id = corte.IdCorte,
                    nombre = corte.CorteDesc,
                    precioKg = corte.PrecioKg,
                    codigo = corte.codigo,
                    pesable = corte.Pesable
                },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        // ======================================================
        // POST /Ventas/AgregarProducto    (AJAX)
        // ======================================================
        [HttpPost]
        public JsonResult AgregarProducto(int idCorte, float cantidadKg)
        {
            try
            {
                // Recuperar o crear venta activa desde la Session
                var venta = Session["VentaActiva"] as Venta;

                if (venta == null)
                {
                    venta = new Venta
                    {
                        //Fecha = DateTime.Now,
                        LineasVenta = new System.Collections.Generic.List<LineaVenta>()
                    };

                    Session["VentaActiva"] = venta;
                }

                // Obtener el producto
                var gestorCortes = new  Negocio.Corte();
                var corte = gestorCortes.findCorteById(idCorte, false);

                if (corte == null)
                    return Json(new { error = "Producto no encontrado por ID" });

                // Crear la línea
                var linea = new LineaVenta
                {
                    Corte = corte,
                    PrecioKg = corte.PrecioKg,
                    CantKg = cantidadKg
                };

                venta.LineasVenta.Add(linea);

                // Respuesta con la venta actualizada
                return Json(new
                {
                    ok = true,
                    total = venta.LineasVenta.Sum(x => x.CantKg * x.PrecioKg),
                    lineas = venta.LineasVenta.Select((x, i) => new
                    {
                        index = i + 1,
                        producto = x.Corte.CorteDesc,
                        codigo = x.Corte.codigo,
                        cant = x.CantKg.ToString("0.###"),
                        precio = x.PrecioKg.ToString("C"),
                        subtotal = (x.CantKg * x.PrecioKg).ToString("C")
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        #endregion
    }

}