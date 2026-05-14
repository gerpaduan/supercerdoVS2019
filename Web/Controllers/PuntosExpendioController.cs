using Entidades;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;
using Web.Models.DTO;

namespace Web.Controllers
{
    public class PuntosExpendioController : BaseController
    {
        private Negocio.Venta oVentaN;
        private Negocio.Corte oCorteN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Persona oPersonaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oVentaN = new Negocio.Venta(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
            oPersonaN = new Negocio.Persona(empresa, param);
        }

        public ActionResult Abrir(int id = 0, string sector = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, DateTime.Today, user.Id) ?? "No tiene permisos para abrir puntos de expendio.";
                return RedirectToAction("Index", "Home");
            }

            AsegurarSucursalUsuario(user);

            var model = id > 0
                ? CrearModeloExistente(id)
                : CrearModeloNuevo(user, sector);

            if (model == null)
                return HttpNotFound();

            ViewBag.Title = model.EsGuardado ? "Punto de expendio" : "Abrir punto de expendio";
            ViewBag.Seccion = "Punto de expendio";

            return View("~/Views/PuntosExpendio/Abrir.cshtml", model);
        }

        public ActionResult POS(string sector = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, DateTime.Today, user.Id) ?? "No tiene permisos para abrir puntos de expendio.";
                return RedirectToAction("Index", "Home");
            }

            AsegurarSucursalUsuario(user);

            var model = CrearModeloNuevo(user, sector);
            ViewBag.IdConsumidorFinal = oPersonaN.getConsumidorFinal() != null ? oPersonaN.getConsumidorFinal().IdPersona : 0;
            ViewBag.PuedeBonificarPuntoExpendio = PermisosHelper.TienePermiso(
                Session,
                Permisos.Venta.Bonificar,
                DateTime.Today,
                Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            ViewBag.Title = "CarniSys | Punto de Expendio";
            ViewBag.Seccion = "Punto de expendio";

            return View("~/Views/PuntosExpendio/POS.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(PuntoExpendioEditVm model)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, model != null ? model.FechaExpendio : DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, model != null ? model.FechaExpendio : DateTime.Today, user.Id) ?? "No tiene permisos para guardar puntos de expendio.";
                return RedirectToAction("Abrir");
            }

            AsegurarSucursalUsuario(user);

            if (model == null)
                model = new PuntoExpendioEditVm();

            NormalizarLineas(model);

            string error = ValidarModelo(model);
            if (!string.IsNullOrWhiteSpace(error))
            {
                CompletarModeloNuevo(model, user);
                ModelState.AddModelError("", error);
                ViewBag.Title = "Abrir punto de expendio";
                ViewBag.Seccion = "Punto de expendio";
                return View("~/Views/PuntosExpendio/Abrir.cshtml", model);
            }

            var consumidorFinal = oPersonaN.getConsumidorFinal() ?? new Entidades.Persona();
            var sucursal = user.Sucursal ?? oSucursalN.findById(user.IdSucursal);
            if (sucursal == null)
            {
                CompletarModeloNuevo(model, user);
                ModelState.AddModelError("", "No se encontró la sucursal activa del usuario.");
                ViewBag.Title = "Abrir punto de expendio";
                ViewBag.Seccion = "Punto de expendio";
                return View("~/Views/PuntosExpendio/Abrir.cshtml", model);
            }

            var expendio = new Entidades.Venta
            {
                IdVenta = 0,
                Persona = consumidorFinal,
                Sucursal = sucursal,
                TipoVenta = "Caja",
                FechaVenta = model.FechaExpendio,
                Turno = "",
                DiaFestivo = "",
                TotalImporte = model.Lineas.Sum(l => l.Total),
                AcumRedondeoImporte = 0,
                AcumRedondeoKgs = 0,
                LineasVenta = new List<Entidades.LineaVenta>(),
                FormaPago = Entidades.Venta.formaPagoEnum.Nulo.ToString(),
                TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()),
                IdentificacionExpendio = model.IdentificacionCliente ?? "",
                Sector = (model.Sector ?? "").Trim(),
                CantItems = model.Lineas.Count.ToString(CultureInfo.InvariantCulture),
                Observaciones = "",
                NroRemito = "",
                SerialCPU = "",
                Vendedor = user
            };

            try
            {
                expendio.IdVenta = expendio.IdExpendio = oVentaN.agregarExpendio(expendio);

                foreach (var linea in model.Lineas)
                {
                    var item = new Entidades.LineaVenta
                    {
                        Venta = expendio,
                        Corte = new Entidades.Corte { IdCorte = linea.IdCorte },
                        CantKg = linea.CantKg,
                        KgsTotalCalculado = linea.CantKg,
                        PrecioKg = linea.PrecioKg,
                        PesoBalanza = linea.PesoBalanza,
                        Estado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado),
                        IndexAnulado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado)
                    };

                    oVentaN.agregarLineaExprendio(item);
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Punto de expendio";
                TempData["AlertMsg"] = "El punto de expendio se guardó correctamente.";
                return RedirectToAction("Abrir", new { id = expendio.IdExpendio });
            }
            catch (Exception ex)
            {
                CompletarModeloNuevo(model, user);
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = "Abrir punto de expendio";
                ViewBag.Seccion = "Punto de expendio";
                return View("~/Views/PuntosExpendio/Abrir.cshtml", model);
            }
        }

        [HttpGet]
        public JsonResult BuscarProducto(string q = "")
        {
            try
            {
                var productos = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    string filtro = q.Trim();
                    productos = productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.corte) && p.corte.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrWhiteSpace(p.CorteDesc) && p.CorteDesc.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        p.codigo.ToString().IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                var resultado = productos.Take(200).Select(p => new
                {
                    id = p.IdCorte,
                    codigo = p.codigo.ToString(),
                    nombre = !string.IsNullOrWhiteSpace(p.corte) ? p.corte : p.CorteDesc,
                    precio = p.precioKg
                }).ToList();

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult BuscarProductoPorCodigo(long? codigo)
        {
            if (!codigo.HasValue || codigo.Value <= 0)
                return Json(new { ok = false, mensaje = "Código inválido." }, JsonRequestBehavior.AllowGet);

            var corte = oCorteN.findCorteByCodigo(codigo.Value, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el producto." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ok = true,
                id = corte.IdCorte,
                codigo = corte.Codigo,
                nombre = !string.IsNullOrWhiteSpace(corte.corte) ? corte.corte : corte.CorteDesc,
                precio = corte.precioKg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BuscarProductoPOS(string codigo, bool ingresoCantidadX = false)
        {
            long codigoBuscado;
            if (string.IsNullOrWhiteSpace(codigo) || !long.TryParse(codigo, out codigoBuscado) || codigoBuscado <= 0)
                return Json(new { success = false, message = "Código inválido." }, JsonRequestBehavior.AllowGet);

            var corte = oCorteN.findCorteByCodigo(codigoBuscado, false);
            if (corte == null || corte.IdCorte <= 0)
                return Json(new { success = false, message = "No se encontró el producto." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                id = corte.IdCorte,
                codigo = corte.Codigo.ToString(),
                nombre = !string.IsNullOrWhiteSpace(corte.corte) ? corte.corte : corte.CorteDesc,
                precioKg = corte.PrecioKg,
                precioOriginal = corte.PrecioKg,
                balanza = corte.Pesable
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FinalizarPOS(FinalizarPuntoExpendioRequest request)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "La sesión expiró. Vuelva a ingresar." });

            DateTime fecha = request != null && request.FechaExpendio.HasValue ? request.FechaExpendio.Value : DateTime.Today;
            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, fecha, user.Id))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, fecha, user.Id) ?? "No tiene permisos para guardar puntos de expendio."
                });
            }

            AsegurarSucursalUsuario(user);

            var model = new PuntoExpendioEditVm
            {
                FechaExpendio = request != null && request.FechaExpendio.HasValue ? request.FechaExpendio.Value : DateTime.Now,
                Sector = request != null ? request.Sector : "",
                IdentificacionCliente = request != null ? request.IdentificacionCliente : "",
                Observaciones = request != null ? request.Observaciones : "",
                Lineas = new List<PuntoExpendioLineaVm>()
            };

            foreach (var linea in (request != null ? request.LineasVenta : null) ?? new List<LineaVentaDto>())
            {
                if (linea == null || Entidades.LineaVenta.esAnulado(linea.Estado))
                    continue;

                model.Lineas.Add(new PuntoExpendioLineaVm
                {
                    IdCorte = linea.IdCorte,
                    Codigo = linea.Codigo,
                    Producto = linea.Descripcion ?? "",
                    CantKg = linea.CantKg,
                    PrecioKg = linea.PrecioKg,
                    PesoBalanza = linea.Balanza,
                    Total = linea.Importe
                });
            }

            NormalizarLineas(model);

            string error = ValidarModelo(model);
            if (!string.IsNullOrWhiteSpace(error))
                return Json(new { ok = false, mensaje = error });

            var consumidorFinal = oPersonaN.getConsumidorFinal() ?? new Entidades.Persona();
            var sucursal = user.Sucursal ?? oSucursalN.findById(user.IdSucursal);
            if (sucursal == null)
                return Json(new { ok = false, mensaje = "No se encontró la sucursal activa del usuario." });

            var expendio = new Entidades.Venta
            {
                IdVenta = 0,
                Persona = consumidorFinal,
                Sucursal = sucursal,
                TipoVenta = "Caja",
                FechaVenta = model.FechaExpendio,
                Turno = "",
                DiaFestivo = "",
                TotalImporte = model.Lineas.Sum(l => l.Total),
                AcumRedondeoImporte = 0,
                AcumRedondeoKgs = 0,
                LineasVenta = new List<Entidades.LineaVenta>(),
                FormaPago = Entidades.Venta.formaPagoEnum.Nulo.ToString(),
                TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()),
                IdentificacionExpendio = model.IdentificacionCliente ?? "",
                Sector = (model.Sector ?? "").Trim(),
                CantItems = model.Lineas.Count.ToString(CultureInfo.InvariantCulture),
                Observaciones = "",
                NroRemito = "",
                SerialCPU = "",
                Vendedor = user
            };

            try
            {
                expendio.IdVenta = expendio.IdExpendio = oVentaN.agregarExpendio(expendio);

                foreach (var linea in model.Lineas)
                {
                    var corte = linea.IdCorte > 0
                        ? oCorteN.findCorteById(linea.IdCorte, false)
                        : (linea.Codigo > 0 ? oCorteN.findCorteByCodigo(linea.Codigo, false) : null);

                    if (corte == null || corte.IdCorte <= 0)
                        return Json(new { ok = false, mensaje = "No se encontró uno de los productos cargados." });

                    var item = new Entidades.LineaVenta
                    {
                        Venta = expendio,
                        Corte = new Entidades.Corte { IdCorte = corte.IdCorte },
                        CantKg = linea.CantKg,
                        KgsTotalCalculado = linea.CantKg,
                        PrecioKg = linea.PrecioKg,
                        PesoBalanza = linea.PesoBalanza,
                        Estado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado),
                        IndexAnulado = Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado)
                    };

                    oVentaN.agregarLineaExprendio(item);
                }

                string pdfUrl = Url.Action("ImprimirPdf", "PuntosExpendio", new { id = expendio.IdExpendio });
                string pdfUrlAbsoluta = Url.Action("ImprimirPdf", "PuntosExpendio", new { id = expendio.IdExpendio }, Request != null && Request.Url != null ? Request.Url.Scheme : "http");

                return Json(new
                {
                    ok = true,
                    idExpendio = expendio.IdExpendio,
                    redirectUrl = Url.Action("POS", "PuntosExpendio", new { sector = expendio.Sector }),
                    imprimirUrl = Url.Action("ImprimirTicket", "PuntosExpendio", new { id = expendio.IdExpendio }),
                    pdfUrl = pdfUrl,
                    whatsappTexto = "Punto de expendio " + expendio.IdExpendio + " - " + pdfUrlAbsoluta
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        public ActionResult ImprimirTicket(int id, int mm = 58)
        {
            var expendio = oVentaN.getExpedioById(id);
            if (expendio == null)
                return HttpNotFound();

            ViewBag.TicketMm = mm == 80 ? 80 : 58;
            return View("~/Views/PuntosExpendio/_TicketPuntoExpendio.cshtml", expendio);
        }

        [HttpGet]
        public ActionResult ImprimirPdf(int id)
        {
            var expendio = oVentaN.getExpedioById(id);
            if (expendio == null || expendio.IdExpendio <= 0)
                return HttpNotFound();

            byte[] bytes = GenerarPdfPuntoExpendio(expendio);
            return File(bytes, "application/pdf", "PuntoExpendio_" + id + ".pdf");
        }

        public ActionResult Sectores(string editar = "")
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, DateTime.Today, user.Id) ?? "No tiene permisos para administrar sectores.";
                return RedirectToAction("Index", "Home");
            }

            var model = new SectorAbmVm
            {
                SectorOriginal = editar ?? "",
                Nombre = editar ?? ""
            };

            CargarSectores(model);
            ViewBag.Title = "Sectores";
            ViewBag.Seccion = "Punto de expendio";
            return View("~/Views/PuntosExpendio/Sectores.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarSector(SectorAbmVm model)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            string nombre = (model != null ? model.Nombre : "") ?? "";
            string nombreNormalizado = nombre.Trim();
            string sectorOriginal = (model != null ? model.SectorOriginal : "") ?? "";

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, DateTime.Today, user.Id) ?? "No tiene permisos para administrar sectores.";
                return RedirectToAction("Sectores");
            }

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Debe ingresar un nombre de sector.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (oVentaN.existeSector(nombreNormalizado, sectorOriginal))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Ya existe otro sector con ese nombre en esta empresa.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (string.IsNullOrWhiteSpace(sectorOriginal))
            {
                oVentaN.agregarSector(nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se creó correctamente.";
            }
            else
            {
                oVentaN.modificarSector(sectorOriginal, nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se actualizó correctamente.";
            }

            return RedirectToAction("Sectores");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarSector(string sector)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, DateTime.Today, user.Id))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = ConstruirMensajePermisoFecha(Permisos.Venta.NuevaVenta, DateTime.Today, user.Id) ?? "No tiene permisos para administrar sectores.";
                return RedirectToAction("Sectores");
            }

            string nombre = (sector ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se recibió un sector válido para eliminar.";
                return RedirectToAction("Sectores");
            }

            if (oVentaN.sectorEstaEnUso(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se puede eliminar el sector porque está en uso en puntos de expendio.";
                return RedirectToAction("Sectores");
            }

            oVentaN.eliminarSector(nombre);
            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Sectores";
            TempData["AlertMsg"] = "El sector se eliminó correctamente.";
            return RedirectToAction("Sectores");
        }

        private PuntoExpendioEditVm CrearModeloNuevo(Entidades.Usuario user, string sector)
        {
            var model = new PuntoExpendioEditVm
            {
                Sector = (sector ?? "").Trim(),
                FechaExpendio = DateTime.Now,
                IdentificacionCliente = "",
                Observaciones = "",
                EsGuardado = false
            };

            CompletarModeloNuevo(model, user);
            return model;
        }

        private void CompletarModeloNuevo(PuntoExpendioEditVm model, Entidades.Usuario user)
        {
            if (model == null)
                return;

            model.SectoresDisponibles = ObtenerSectores();
            model.PermiteEditarPrecio = string.Equals((model.Sector ?? "").Trim(), "PRESUPUESTO", StringComparison.OrdinalIgnoreCase);
            model.CantItems = (model.Lineas ?? new List<PuntoExpendioLineaVm>()).Count;
            model.TotalKilos = (model.Lineas ?? new List<PuntoExpendioLineaVm>()).Sum(l => l.CantKg);
            model.TotalImporte = (model.Lineas ?? new List<PuntoExpendioLineaVm>()).Sum(l => l.Total);
            model.VendedorNombre = user != null ? user.Nombre : "";
            model.SucursalNombre = user != null && user.Sucursal != null ? user.Sucursal.SucursalNombre : "";
        }

        private PuntoExpendioEditVm CrearModeloExistente(int id)
        {
            var expendio = oVentaN.getExpedioById(id);
            if (expendio == null)
                return null;

            var model = new PuntoExpendioEditVm
            {
                IdExpendio = expendio.IdExpendio,
                EsGuardado = true,
                Sector = expendio.Sector ?? "",
                FechaExpendio = expendio.FechaVenta,
                IdentificacionCliente = expendio.IdentificacionExpendio ?? "",
                Observaciones = expendio.Observaciones ?? "",
                SucursalNombre = expendio.Sucursal != null ? expendio.Sucursal.SucursalNombre : "",
                VendedorNombre = expendio.Vendedor != null ? expendio.Vendedor.Nombre : "",
                PermiteEditarPrecio = false,
                CantItems = expendio.LineasVenta != null ? expendio.LineasVenta.Count : 0,
                TotalKilos = expendio.LineasVenta != null ? expendio.LineasVenta.Sum(l => l.CantKg) : 0f,
                TotalImporte = expendio.TotalImporte,
                SectoresDisponibles = ObtenerSectores(),
                Lineas = (expendio.LineasVenta ?? new List<Entidades.LineaVenta>()).Select(l => new PuntoExpendioLineaVm
                {
                    IdCorte = l.Corte != null ? l.Corte.IdCorte : 0,
                    Codigo = l.Corte != null ? l.Corte.codigo : 0,
                    Producto = l.Corte != null ? (!string.IsNullOrWhiteSpace(l.Corte.corte) ? l.Corte.corte : l.Corte.CorteDesc) : "",
                    CantKg = l.CantKg,
                    PrecioKg = l.PrecioKg,
                    PesoBalanza = l.PesoBalanza,
                    Total = l.CantKg * l.PrecioKg
                }).ToList()
            };

            return model;
        }

        private string ValidarModelo(PuntoExpendioEditVm model)
        {
            if (model == null)
                return "No se recibieron datos del punto de expendio.";

            model.Sector = (model.Sector ?? "").Trim();
            if (string.IsNullOrWhiteSpace(model.Sector))
                return "Debe seleccionar un sector para el punto de expendio.";

            if (!ObtenerSectores().Any(s => string.Equals(s, model.Sector, StringComparison.OrdinalIgnoreCase)))
                return "El sector seleccionado no existe o ya no está disponible.";

            if (model.Lineas == null || model.Lineas.Count == 0)
                return "Debe cargar al menos un producto en el punto de expendio.";

            for (int i = 0; i < model.Lineas.Count; i++)
            {
                PuntoExpendioLineaVm linea = model.Lineas[i];
                if (linea == null || linea.IdCorte <= 0)
                    return "Hay una línea sin producto válido.";

                if (linea.CantKg <= 0)
                    return "La cantidad en kilos debe ser mayor a cero en todas las líneas.";

                if (linea.PrecioKg <= 0)
                    return "El precio por kilo debe ser mayor a cero en todas las líneas.";
            }

            return "";
        }

        private void NormalizarLineas(PuntoExpendioEditVm model)
        {
            if (model == null || model.Lineas == null)
                return;

            foreach (var linea in model.Lineas)
            {
                if (linea == null)
                    continue;

                linea.Producto = (linea.Producto ?? "").Trim();
                linea.Total = linea.CantKg * linea.PrecioKg;
            }
        }

        private List<string> ObtenerSectores()
        {
            DataTable dt = oVentaN.obtenerSectores() ?? new DataTable();
            return dt.Rows
                .Cast<DataRow>()
                .Select(r => Convert.ToString(r["sector"] ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
        }

        private void CargarSectores(SectorAbmVm model)
        {
            if (model == null)
                return;

            model.Sectores = ObtenerSectores()
                .Select(s => new SectorResumenVm
                {
                    Nombre = s,
                    EnUso = oVentaN.sectorEstaEnUso(s)
                })
                .ToList();
        }

        private byte[] GenerarPdfPuntoExpendio(Entidades.Venta expendio)
        {
            var lineas = expendio.LineasVenta ?? new List<Entidades.LineaVenta>();

            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                doc.Add(new Paragraph("Punto de Expendio", titleFont));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Nro: " + expendio.IdExpendio, normalFont));
                doc.Add(new Paragraph("Sector: " + (expendio.Sector ?? "-"), normalFont));
                doc.Add(new Paragraph("Fecha: " + expendio.FechaVenta.ToString("dd/MM/yyyy HH:mm"), normalFont));
                doc.Add(new Paragraph("Cliente: " + (!string.IsNullOrWhiteSpace(expendio.IdentificacionExpendio) ? expendio.IdentificacionExpendio : "-"), normalFont));
                doc.Add(new Paragraph("Sucursal: " + (expendio.Sucursal != null ? expendio.Sucursal.SucursalNombre : "-"), normalFont));
                doc.Add(new Paragraph("Vendedor: " + (expendio.Vendedor != null ? expendio.Vendedor.Nombre : "-"), normalFont));
                doc.Add(new Paragraph(" "));

                var table = new PdfPTable(4) { WidthPercentage = 100f };
                table.SetWidths(new float[] { 2.5f, 6f, 2f, 2f });
                table.AddCell(new Phrase("Código", boldFont));
                table.AddCell(new Phrase("Producto", boldFont));
                table.AddCell(new Phrase("Kgs.", boldFont));
                table.AddCell(new Phrase("Total", boldFont));

                foreach (var linea in lineas)
                {
                    string nombreProducto = linea.Corte != null
                        ? (!string.IsNullOrWhiteSpace(linea.Corte.corte) ? linea.Corte.corte : linea.Corte.CorteDesc)
                        : "";

                    table.AddCell(new Phrase(linea.Corte != null ? linea.Corte.Codigo.ToString() : "", normalFont));
                    table.AddCell(new Phrase(nombreProducto, normalFont));
                    table.AddCell(new Phrase(linea.CantKg.ToString("F3", CultureInfo.InvariantCulture), normalFont));
                    table.AddCell(new Phrase((linea.CantKg * linea.PrecioKg).ToString("$ #,##0.00", new CultureInfo("es-AR")), normalFont));
                }

                doc.Add(table);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Total items: " + lineas.Count, boldFont));
                doc.Add(new Paragraph("Total kilos: " + lineas.Sum(x => x.CantKg).ToString("F3", CultureInfo.InvariantCulture), boldFont));
                doc.Add(new Paragraph("Total importe: " + expendio.TotalImporte.ToString("$ #,##0.00", new CultureInfo("es-AR")), boldFont));
                doc.Close();

                return ms.ToArray();
            }
        }

        private void AsegurarSucursalUsuario(Entidades.Usuario user)
        {
            if (user == null)
                return;

            if (user.IdSucursal > 0 &&
                (user.Sucursal == null || user.Sucursal.IdSucursal != user.IdSucursal))
            {
                user.Sucursal = oSucursalN.findById(user.IdSucursal);
                Session["Usuario"] = user;
            }
        }
    }
}
