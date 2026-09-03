// Port parcial de Web/Controllers/PuntosExpendioController.cs (1286 lineas, 20 acciones) para el
// Modulo 8 (Ventas y POS) -- ver docs/DECISIONS.md y docs/10-migracion-aspnet-core/README.md. Este
// slice porta SOLO: ExpendiosGenerados/ExpendiosGeneradosData (listado de solo lectura de
// expendios ya generados) y Sectores/GuardarSector/EliminarSector (catalogo simple de sectores,
// mismo perfil de riesgo que TiposEgresoCaja de Modulo 7 -- CRUD chico, sin dinero involucrado).
//
// AGREGADO (2026-09-03, ver docs/DECISIONS.md): ImprimirPdf (PDF con QuestPDF, reemplazo de
// iTextSharp elegido por el usuario) y ObtenerDatosEmailExpendio/EnviarComprobanteEmailExpendio
// (envio real de email, autorizado explicitamente) -- portados, ver mas abajo.
//
// Las 12 acciones restantes del original siguen sin portar en este slice:
//  - POS transaccional (crea una Venta/expendio real, acoplado al estado de caja de Modulo 7,
//    requiere su propio plan y juez de paridad antes de tocarlo, CLAUDE.md seccion 11.1):
//    Abrir, POS, AutorizarOperadorPOS, CerrarOperadorPOS, Guardar, FinalizarPOS,
//    BuscarProducto, BuscarProductoPorCodigo, BuscarProductoPOS, MisExpendiosPOS (este ultimo
//    ademas depende de ResolverOperadorPOS, infraestructura de POS que no existe sin Session).
//  - Impresion de tickets ESC/POS (agente de impresion local, sin relacion con el bloqueante de
//    PDF): ImprimirTicket, ImprimirTicketPayload, DescargarAgenteImpresion.
//
// Consecuencia visible en la vista: el boton "Imprimir" (ticket) de cada card de
// ExpendiosGenerados sigue excluido; se agregan botones nuevos "PDF" y "Email" en su lugar.
//
// Bypass de permisos, mismo criterio de toda la migracion: el usuario stub (Admin=true) hace que
// PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, ...) del original resuelva
// siempre "sin restriccion" -- se omite directamente en las acciones portadas.
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class PuntosExpendioController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;

        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Usuario _oUsuarioN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public PuntosExpendioController()
        {
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oVentaN = new Negocio.Venta(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oUsuarioN = new Negocio.Usuario(_empresa, _param);

            _usuarioActual.Sucursal = _oSucursalN.findById(_usuarioActual.IdSucursal);
        }

        public IActionResult ExpendiosGenerados()
        {
            var sectoresDt = _oVentaN.obtenerSectores();
            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            _oUsuarioN.obtenerUsuarios(true);
            var usuarios = (_oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(x => x != null && x.Activo)
                .OrderBy(x => x.Nombre ?? "")
                .ToList();

            ViewBag.Title = "Expendios generados";
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewBag.SectoresExpendio = (sectoresDt != null
                ? sectoresDt.AsEnumerable()
                    .Select(r => Convert.ToString(r["sector"] ?? ""))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .Select(s => new SelectListItem { Value = s, Text = s })
                    .ToList()
                : new List<SelectListItem>());
            ViewBag.SucursalesExpendio = sucursales
                .Where(s => s != null && s.idSucursal > 0)
                .OrderBy(s => s.sucursal ?? "")
                .Select(s => new SelectListItem { Value = s.sucursal ?? "", Text = s.sucursal ?? "" })
                .ToList();
            ViewBag.UsuariosExpendio = usuarios
                .Where(u => u.Id > 0)
                .Select(u => new SelectListItem { Value = u.Nombre ?? "", Text = u.Nombre ?? "" })
                .ToList();

            return View("~/Views/PuntosExpendio/ExpendiosGenerados.cshtml");
        }

        [HttpGet]
        public IActionResult ExpendiosGeneradosData(string fechaDesde = null, string fechaHasta = null, int top = 300)
        {
            var user = _usuarioActual;
            if (user.IdSucursal == 0)
                return Json(new { ok = false, mensaje = "Sesión inválida o sucursal no seleccionada." });

            try
            {
                DateTime fechaDesdeValue;
                DateTime fechaHastaValue;
                DateTime? fechaDesdeFiltro = DateTime.TryParse(fechaDesde, out fechaDesdeValue) ? (DateTime?)fechaDesdeValue : null;
                DateTime? fechaHastaFiltro = null;
                if (DateTime.TryParse(fechaHasta, out fechaHastaValue))
                {
                    fechaHastaFiltro = fechaHastaValue.TimeOfDay == TimeSpan.Zero
                        ? fechaHastaValue.AddDays(1).AddSeconds(-1)
                        : fechaHastaValue;
                }
                DataTable dt = _oVentaN.obtenerExpendiosEmpresa(top <= 0 ? 300 : top, fechaDesdeFiltro, fechaHastaFiltro);

                var items = dt.AsEnumerable()
                    .Select(row =>
                    {
                        DateTime fechaExpendio = row["fechaExpendio"] != DBNull.Value
                            ? Convert.ToDateTime(row["fechaExpendio"])
                            : DateTime.MinValue;

                        int idExpendio = row["idExpendio"] != DBNull.Value ? Convert.ToInt32(row["idExpendio"]) : 0;
                        int idVenta = row["idVenta"] != DBNull.Value ? Convert.ToInt32(row["idVenta"]) : 0;
                        var expendio = idExpendio > 0 ? _oVentaN.getExpedioById(idExpendio) : null;
                        var lineas = (expendio != null ? expendio.LineasVenta : null) ?? new List<Entidades.LineaVenta>();

                        return new
                        {
                            fechaExpendio = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("yyyy-MM-ddTHH:mm:ss") : "",
                            fecha = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("dd/MM/yyyy") : "",
                            hora = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("HH:mm") : "",
                            idExpendio = idExpendio,
                            identificacionExpendio = Convert.ToString(row["identificacionExpendio"] ?? ""),
                            sucursal = Convert.ToString(row["sucursal"] ?? ""),
                            sector = Convert.ToString(row["sector"] ?? ""),
                            usuario = Convert.ToString(row["vendedor"] ?? ""),
                            cantItems = Convert.ToString(row["cantItems"] ?? "0"),
                            totalKg = row["totalKg"] != DBNull.Value ? Convert.ToDecimal(row["totalKg"]) : 0m,
                            totalImporte = row["importe"] != DBNull.Value ? Convert.ToDecimal(row["importe"]) : 0m,
                            idVenta = idVenta,
                            estado = idVenta > 0 && idVenta != idExpendio ? "Asignado" : "Pendiente",
                            pdfUrl = idExpendio > 0 ? Url.Action("ImprimirPdf", "PuntosExpendio", new { id = idExpendio }) : "",
                            emailUrl = idExpendio > 0 ? Url.Action("ObtenerDatosEmailExpendio", "PuntosExpendio", new { idExpendio }) : "",
                            lineas = lineas.Select(l => new
                            {
                                codigo = l.Corte != null ? l.Corte.Codigo : 0,
                                producto = l.Corte != null
                                    ? (!string.IsNullOrWhiteSpace(l.Corte.corte) ? l.Corte.corte : l.Corte.CorteDesc)
                                    : "",
                                cantKg = l.CantKg,
                                precioKg = l.PrecioKg,
                                total = l.CantKg * l.PrecioKg
                            }).ToList()
                        };
                    })
                    .ToList();

                return Json(new { ok = true, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "No se pudieron consultar los expendios: " + ex.Message });
            }
        }

        public IActionResult Sectores(string editar = "")
        {
            var model = new SectorAbmVm
            {
                SectorOriginal = editar ?? "",
                Nombre = editar ?? ""
            };

            CargarSectores(model);
            ViewBag.Title = "Sectores";
            return View("~/Views/PuntosExpendio/Sectores.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarSector(SectorAbmVm model)
        {
            string nombre = (model != null ? model.Nombre : "") ?? "";
            string nombreNormalizado = nombre.Trim();
            string sectorOriginal = (model != null ? model.SectorOriginal : "") ?? "";

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Debe ingresar un nombre de sector.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (_oVentaN.existeSector(nombreNormalizado, sectorOriginal))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Ya existe otro sector con ese nombre en esta empresa.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (string.IsNullOrWhiteSpace(sectorOriginal))
            {
                _oVentaN.agregarSector(nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se creó correctamente.";
            }
            else
            {
                _oVentaN.modificarSector(sectorOriginal, nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se actualizó correctamente.";
            }

            return RedirectToAction("Sectores");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarSector(string sector)
        {
            string nombre = (sector ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se recibió un sector válido para eliminar.";
                return RedirectToAction("Sectores");
            }

            if (_oVentaN.sectorEstaEnUso(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se puede eliminar el sector porque está en uso en puntos de expendio.";
                return RedirectToAction("Sectores");
            }

            _oVentaN.eliminarSector(nombre);
            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Sectores";
            TempData["AlertMsg"] = "El sector se eliminó correctamente.";
            return RedirectToAction("Sectores");
        }

        [HttpGet]
        public IActionResult ImprimirPdf(int id)
        {
            var expendio = _oVentaN.getExpedioById(id);
            if (expendio == null || expendio.IdExpendio <= 0)
                return NotFound();

            byte[] bytes = GenerarPdfPuntoExpendio(expendio);
            return File(bytes, "application/pdf", "PuntoExpendio_" + id + ".pdf");
        }

        // Reemplazo de "Enviar por WhatsApp" en el modal post-expendio (ver docs/DECISIONS.md).
        // El punto de expendio no tiene factura electronica ni nota de credito, y el cliente es
        // texto libre (sin Persona.Email confiable) -- por eso el email destino arranca vacio, a
        // cargar a mano.
        [HttpGet]
        public IActionResult ObtenerDatosEmailExpendio(int idExpendio)
        {
            try
            {
                var expendio = _oVentaN.getExpedioById(idExpendio);
                if (expendio == null || expendio.IdExpendio <= 0)
                    return Json(new { ok = false, msg = "Punto de expendio no encontrado." });

                string nombreEmpresa = ObtenerNombreEmpresaExpendio(expendio);
                string asunto = "Punto de expendio " + expendio.IdExpendio + " - " + nombreEmpresa;
                string cuerpo =
                    "Hola:\n\n" +
                    "Adjuntamos el comprobante del punto de expendio Nro " + expendio.IdExpendio + ".\n\n" +
                    "Este correo fue enviado automáticamente. Por favor, no responda a este mensaje.\n\n" +
                    "Atentamente,\n" +
                    nombreEmpresa;

                return Json(new { ok = true, email = "", asunto, mensaje = cuerpo });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EnviarComprobanteEmailExpendio(int idExpendio, string emailDestino, string asunto, string mensaje)
        {
            try
            {
                var expendio = _oVentaN.getExpedioById(idExpendio);
                if (expendio == null || expendio.IdExpendio <= 0)
                    return Json(new { ok = false, msg = "Punto de expendio no encontrado." });

                emailDestino = (emailDestino ?? "").Trim();
                asunto = (asunto ?? "").Trim();
                mensaje = (mensaje ?? "").Trim();

                if (string.IsNullOrWhiteSpace(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email destino." });

                if (!SmtpMailHelper.IsValidEmail(emailDestino))
                    return Json(new { ok = false, msg = "Ingrese un email válido." });

                if (string.IsNullOrWhiteSpace(asunto))
                    return Json(new { ok = false, msg = "Ingrese un asunto." });

                string nombreEmpresa = ObtenerNombreEmpresaExpendio(expendio);
                var empresaExpendio = ObtenerEmpresaExpendio(expendio);
                byte[] pdfBytes = GenerarPdfPuntoExpendio(expendio);
                string nombreAdjunto = "PuntoExpendio_" + expendio.IdExpendio + ".pdf";
                string fromName = "CarniSys - " + nombreEmpresa;
                string replyToEmail = empresaExpendio != null ? (empresaExpendio.Email ?? "").Trim() : "";

                SmtpMailHelper.SendMail(
                    toEmail: emailDestino,
                    toName: "",
                    subject: asunto,
                    bodyHtml: ConvertirTextoAHtmlExpendio(mensaje),
                    attachmentFileName: nombreAdjunto,
                    attachmentBytes: pdfBytes,
                    attachmentContentType: "application/pdf",
                    fromNameOverride: fromName,
                    replyToEmail: SmtpMailHelper.IsValidEmail(replyToEmail) ? replyToEmail : null,
                    replyToName: nombreEmpresa
                );

                return Json(new { ok = true, msg = "El comprobante se envió correctamente." });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { ok = false, msg = "No se pudo enviar el email. " + ex.Message });
            }
        }

        private Entidades.Empresa ObtenerEmpresaExpendio(Entidades.Venta expendio)
        {
            return (expendio != null && expendio.Sucursal != null ? expendio.Sucursal.Empresa : null)
                ?? _usuarioActual.Empresa;
        }

        private string ObtenerNombreEmpresaExpendio(Entidades.Venta expendio)
        {
            var empresaExpendio = ObtenerEmpresaExpendio(expendio);
            string nombre = empresaExpendio != null
                ? (!string.IsNullOrWhiteSpace(empresaExpendio.NombreFantasia) ? empresaExpendio.NombreFantasia : empresaExpendio.RazonSocialAfip)
                : "";
            return !string.IsNullOrWhiteSpace(nombre)
                ? nombre.Trim()
                : "CarniSys";
        }

        private string ConvertirTextoAHtmlExpendio(string texto)
        {
            string safe = System.Net.WebUtility.HtmlEncode(texto ?? "");
            safe = safe.Replace("\r\n", "\n").Replace("\r", "\n");
            string cuerpoHtml = "<p>" + safe.Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
            string pieHtml =
                "<div style=\"margin-top:24px; padding-top:12px; border-top:1px solid #ddd; font-size:11px; color:#777; line-height:1.4;\">" +
                "<p>CarniSys es un software de gestión comercial para pequeños y medianos comercios, diseñado para administrar ventas, stock y facturación, con integración a balanzas para agilizar la atención en productos pesables.</p>" +
                "</div>";

            return cuerpoHtml + pieHtml;
        }

        // Port de GenerarPdfPuntoExpendio (Web/Controllers/PuntosExpendioController.cs) de
        // iTextSharp a QuestPDF -- mismo contenido/orden de campos, sintaxis nueva.
        private byte[] GenerarPdfPuntoExpendio(Entidades.Venta expendio)
        {
            var lineas = expendio.LineasVenta ?? new List<Entidades.LineaVenta>();
            var culturaAr = new CultureInfo("es-AR");

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(4);
                        col.Item().Text("Punto de Expendio").FontSize(16).Bold();
                        col.Item().PaddingTop(6);
                        col.Item().Text("Nro: " + expendio.IdExpendio);
                        col.Item().Text("Sector: " + (expendio.Sector ?? "-"));
                        col.Item().Text("Fecha: " + expendio.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                        col.Item().Text("Cliente: " + (!string.IsNullOrWhiteSpace(expendio.IdentificacionExpendio) ? expendio.IdentificacionExpendio : "-"));
                        col.Item().Text("Sucursal: " + (expendio.Sucursal != null ? expendio.Sucursal.SucursalNombre : "-"));
                        col.Item().Text("Vendedor: " + (expendio.Vendedor != null ? expendio.Vendedor.Nombre : "-"));
                        col.Item().PaddingTop(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(6f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Código").Bold();
                                header.Cell().Text("Producto").Bold();
                                header.Cell().Text("Kgs.").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            foreach (var linea in lineas)
                            {
                                string nombreProducto = linea.Corte != null
                                    ? (!string.IsNullOrWhiteSpace(linea.Corte.corte) ? linea.Corte.corte : linea.Corte.CorteDesc)
                                    : "";

                                table.Cell().Text(linea.Corte != null ? linea.Corte.Codigo.ToString() : "");
                                table.Cell().Text(nombreProducto);
                                table.Cell().Text(linea.CantKg.ToString("F3", CultureInfo.InvariantCulture));
                                table.Cell().Text((linea.CantKg * linea.PrecioKg).ToString("$ #,##0.00", culturaAr));
                            }
                        });

                        col.Item().PaddingTop(10);
                        col.Item().Text("Total items: " + lineas.Count).Bold();
                        col.Item().Text("Total kilos: " + lineas.Sum(x => x.CantKg).ToString("F3", CultureInfo.InvariantCulture)).Bold();
                        col.Item().Text("Total importe: " + expendio.TotalImporte.ToString("$ #,##0.00", culturaAr)).Bold();
                    });
                });
            });

            return documento.GeneratePdf();
        }

        private List<string> ObtenerSectores()
        {
            DataTable dt = _oVentaN.obtenerSectores() ?? new DataTable();
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
                    EnUso = _oVentaN.sectorEstaEnUso(s)
                })
                .ToList();
        }
    }
}
