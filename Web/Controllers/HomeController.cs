using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Datos;
using System.Globalization;
using System.IO;
using System.Text;
using Web.Helpers;
using Web.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oUsuarioN = new Negocio.Usuario(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
        }

        public ActionResult Index()
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            ViewBag.PuedeVerCierreCaja = oUsuarioN.tienePermiso(user, Permisos.Caja.CierresDeCaja, DateTime.Today, -1);

            var sucursales = oSucursalN.findAll();
            ViewBag.Sucursales = sucursales;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Utilidades()
        {
            var model = new UtilitiesIndexVm
            {
                Agentes = new List<UtilityItemVm>
                {
                    BuildUtilityItem(
                        "Agente de balanza",
                        "Agente local",
                        "Lee la balanza desde la PC local y expone la API en 127.0.0.1 para POS y otras pantallas web.",
                        "~/Content/downloads/Carnisys.Balanza.Agent.zip",
                        "Carnisys.Balanza.Agent.zip",
                        "Incluye ejecutable, configuración inicial y script de instalación local."),
                    BuildUtilityItem(
                        "Agente de impresión",
                        "Agente local",
                        "Permite imprimir tickets desde la terminal local sin depender del servidor web.",
                        "~/Content/downloads/CarniSys.PrintAgent.zip",
                        "CarniSys.PrintAgent.zip",
                        "Instalar en la terminal donde está conectada la impresora térmica.")
                },
                OtrasUtilidades = new List<UtilityItemVm>
                {
                    new UtilityItemVm
                    {
                        Nombre = "Próximas utilidades",
                        Categoria = "Catálogo",
                        Descripcion = "Este sector queda preparado para sumar nuevas herramientas locales o instaladores del sistema.",
                        Estado = "Próximamente",
                        Version = "-",
                        Disponible = false,
                        NotaInstalacion = "Aquí podremos ir agregando nuevas utilidades sin tocar el resto del menú."
                    }
                }
            };

            return View(model);
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ImprimirCalculadoraBilletesPayload(CalculadoraBilletesPrintVm request)
        {
            try
            {
                if (request == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos para imprimir." });

                int ticketMm = request.TicketMm == 58 ? 58 : 80;
                return Json(new
                {
                    ok = true,
                    ticketMm = ticketMm,
                    ticketLines = ConstruirLineasCalculadoraBilletes(request, ticketMm)
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DescargarCalculadoraBilletesPdf(CalculadoraBilletesPrintVm request)
        {
            try
            {
                if (request == null)
                    return Json(new { ok = false, mensaje = "No se recibieron datos para generar el PDF." });

                byte[] bytes = GenerarPdfCalculadoraBilletes(request);
                return Json(new
                {
                    ok = true,
                    fileName = "DetalleBilletes.pdf",
                    base64 = System.Convert.ToBase64String(bytes)
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult DescargarAgenteImpresion()
        {
            string path = Server.MapPath("~/Content/downloads/CarniSys.PrintAgent.zip");
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            return File(path, "application/zip", "CarniSys.PrintAgent.zip");
        }

        private UtilityItemVm BuildUtilityItem(string nombre, string categoria, string descripcion, string virtualPath, string archivoNombre, string notaInstalacion)
        {
            string physicalPath = Server.MapPath(virtualPath);
            bool disponible = System.IO.File.Exists(physicalPath);
            var info = disponible ? new FileInfo(physicalPath) : null;

            return new UtilityItemVm
            {
                Nombre = nombre,
                Categoria = categoria,
                Descripcion = descripcion,
                Estado = disponible ? "Disponible" : "No disponible",
                Version = info != null ? info.LastWriteTime.ToString("dd/MM/yyyy HH:mm") : "-",
                ArchivoUrl = disponible ? Url.Content(virtualPath) : string.Empty,
                ArchivoNombre = archivoNombre,
                ArchivoTamano = info != null ? FormatFileSize(info.Length) : "-",
                NotaInstalacion = notaInstalacion,
                Disponible = disponible
            };
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "0 KB";

            double kb = bytes / 1024d;
            if (kb < 1024d)
            {
                return kb.ToString("0.#", CultureInfo.InvariantCulture) + " KB";
            }

            double mb = kb / 1024d;
            return mb.ToString("0.##", CultureInfo.InvariantCulture) + " MB";
        }

        private List<string> ConstruirLineasCalculadoraBilletes(CalculadoraBilletesPrintVm request, int ticketMm)
        {
            int cantMaxChar = ticketMm == 58 ? 32 : 43;
            string titulo = string.IsNullOrWhiteSpace(request.Titulo) ? "Detalle billetes" : request.Titulo.Trim();

            var user = Session["Usuario"] as Entidades.Usuario;
            string empresaNombre = user != null && user.Empresa != null
                ? (user.Empresa.NombreFantasia ?? user.Empresa.RazonSocialAfip ?? "CarniSys")
                : "CarniSys";

            System.Func<string, int, string> truncar = (texto, maximo) =>
            {
                texto = texto ?? "";
                return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
            };

            System.Func<string, int, string> centrar = (texto, ancho) =>
            {
                texto = truncar(texto, ancho);
                int espaciosIzquierda = (ancho - texto.Length) / 2;
                if (espaciosIzquierda < 0) espaciosIzquierda = 0;
                return new string(' ', espaciosIzquierda) + texto;
            };

            var lineas = new List<string>();
            lineas.Add(centrar(titulo, cantMaxChar));
            lineas.Add(centrar(empresaNombre, cantMaxChar));
            lineas.Add(truncar("Fecha: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), cantMaxChar));
            lineas.Add(new string('-', cantMaxChar));
            lineas.Add("Total $: " + request.Total.ToString("N2"));
            lineas.Add("Detalles:");
            lineas.Add(NormalizarDetalleCalculadoraBilletes(request));
            lineas.Add("\u00A0");
            lineas.Add(".");
            lineas.Add("&nbsp;");
            lineas.Add("br");
            lineas.Add("<br>");
            lineas.Add("<br>");

            return lineas;
        }

        private byte[] GenerarPdfCalculadoraBilletes(CalculadoraBilletesPrintVm request)
        {
            using (var ms = new MemoryStream())
            {
                using (var document = new Document(PageSize.A4, 36f, 36f, 36f, 36f))
                {
                    PdfWriter.GetInstance(document, ms);
                    document.Open();

                    var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16f);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11f);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);

                    string titulo = string.IsNullOrWhiteSpace(request.Titulo) ? "Detalle de billetes" : request.Titulo.Trim();
                    document.Add(new Paragraph(titulo, tituloFont));
                    document.Add(new Paragraph(System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), normalFont));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("Total $: " + request.Total.ToString("N2"), boldFont));
                    document.Add(new Paragraph("Detalles:", boldFont));
                    document.Add(new Paragraph(NormalizarDetalleCalculadoraBilletes(request), normalFont));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));

                    document.Close();
                }

                return ms.ToArray();
            }
        }

        private string NormalizarDetalleCalculadoraBilletes(CalculadoraBilletesPrintVm request)
        {
            if (request == null)
                return "";

            if (request.Denominaciones != null && request.Denominaciones.Count > 0)
            {
                var partes = request.Denominaciones
                    .Where(x => x != null && x.Denominacion > 0)
                    .Select(x => x.Cantidad.ToString() + " x " + x.Denominacion.ToString("N0"))
                    .ToList();

                if (request.Monedas > 0)
                    partes.Add("Monedas " + request.Monedas.ToString("N2"));

                return string.Join(" + ", partes);
            }

            return request.DetalleTexto ?? "";
        }
    }
}
