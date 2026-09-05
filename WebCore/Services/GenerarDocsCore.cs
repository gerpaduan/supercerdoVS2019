// Port de Utilidades/GenerarDocs.cs (iTextSharp) a QuestPDF -- ver docs/10-migracion-aspnet-core/
// README.md. Porta GenerarFacturaPDF (factura/ticket A4, VentasController) con el QR oficial de
// AFIP (RG 4892/2020) y GenerarPdfCtaCtePersona (extracto de cuenta corriente, FinanzasController).
//
// Simplificacion deliberada: el original fija el bloque de totales/regimen fiscal/QR/CAE a una
// posicion absoluta en la ULTIMA pagina via PdfStamper/PdfContentByte (especifico de iTextSharp,
// sin equivalente directo en QuestPDF, que compone paginas de forma declarativa). Aca ese bloque
// fluye al final del contenido en vez de anclarse al pie de la ultima pagina -- mismo contenido y
// orden, sin el ajuste de posicion fino; para el caso tipico (pocas lineas, 1 pagina) el resultado
// visual es equivalente.
//
// GenerateQRCode: el original usa QRCodeGenerator + System.Drawing.Bitmap (bloqueante ya señalado
// desde el plan original de la migracion -- GDI+ no corre en Linux desde .NET 6+). Aca se usa
// PngByteQRCode (bytes PNG directos, sin System.Drawing), mismo payload/URL de AFIP.
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WebCore.Services
{
    public static class GenerarDocsCore
    {
        // Port de Utilidades/GenerarDocs.cs GenerarPdfCtaCtePersona -- mismo contenido/orden,
        // sintaxis QuestPDF en vez de iTextSharp.
        public static byte[] GenerarPdfCtaCtePersona(DataTable dt, DateTime fechaDesde)
        {
            var culturaAr = new CultureInfo("es-AR");
            string persona = "";
            decimal saldo = 0;

            if (dt.Rows.Count > 0)
            {
                persona = dt.Rows[0]["razonSocial"].ToString();
                saldo = Convert.ToDecimal(dt.Rows[dt.Rows.Count - 1]["Saldo"]);
            }

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        col.Spacing(4);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(persona).FontSize(12).Bold();
                            row.RelativeItem().AlignRight().Text(text =>
                            {
                                text.Span("Saldo: ").FontSize(12).Bold().FontColor(Colors.Grey.Medium);
                                text.Span("$ " + saldo.ToString("N2", culturaAr)).FontSize(12).Bold()
                                    .FontColor(saldo >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            });
                        });

                        col.Item().PaddingTop(4).Text("Desde: " + fechaDesde.ToString("dd/MM/yyyy"));

                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.4f);
                                c.RelativeColumn(1.4f);
                                c.RelativeColumn(3.6f);
                                c.RelativeColumn(1.4f);
                                c.RelativeColumn(1.4f);
                                c.RelativeColumn(1.2f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#F0F0F0").Padding(5).Text("Fecha").Bold();
                                h.Cell().Background("#F0F0F0").Padding(5).Text("Operacion").Bold();
                                h.Cell().Background("#F0F0F0").Padding(5).Text("Detalle").Bold();
                                h.Cell().Background("#F0F0F0").Padding(5).AlignRight().Text("Importe").Bold();
                                h.Cell().Background("#F0F0F0").Padding(5).AlignRight().Text("Saldo").Bold();
                                h.Cell().Background("#F0F0F0").Padding(5).Text("Sucursal").Bold();
                            });

                            foreach (DataRow row in dt.Rows)
                            {
                                decimal importe = Convert.ToDecimal(row["importe"]);
                                string detalle = (string.IsNullOrEmpty(row["nroDoc"].ToString()) ? "" : (row["nroDoc"].ToString() + " | ")) + row["detalle"].ToString();

                                table.Cell().Padding(5).Text(Convert.ToDateTime(row["fecha"]).ToString("dd/MM/yyyy"));
                                table.Cell().Padding(5).Text(row["tabla"].ToString());
                                table.Cell().Padding(5).Text(detalle);
                                table.Cell().Padding(5).AlignRight().Text(importe.ToString("N2", culturaAr))
                                    .FontColor(importe >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                table.Cell().Padding(5).AlignRight().Text(Convert.ToDecimal(row["Saldo"]).ToString("N2", culturaAr));
                                table.Cell().Padding(5).Text(row["Sucursal"].ToString());
                            }
                        });
                    });
                });
            });

            return documento.GeneratePdf();
        }

        // Port de Web/Controllers/FinanzasController.cs GenerarPdfPago (recibo de pago/cobro de
        // cuenta corriente, ver docs/10-migracion-aspnet-core/PLAN-ADDOREDITPAGO.md) -- mismo
        // contenido/orden, sintaxis QuestPDF en vez de iTextSharp (que usaba directo, sin port
        // previo a diferencia de GenerarPdfCtaCtePersona). Simplificacion deliberada: el detalle
        // linea-por-linea de cheques del original (formato tabular con AjustarString/columnas
        // alineadas a mano) no se porta -- esta iteracion de AddOrEditPago no admite "Cheque"/
        // "EftvoCheque" como forma de pago (ver PLAN-ADDOREDITPAGO.md, excluido del MVP), asi que
        // ese bloque nunca se ejercita hoy. Si se agrega soporte de cheques mas adelante, agregar
        // la tabla de detalle aca tambien.
        public static byte[] GenerarPdfPago(WebCore.Models.ReciboPagoVm model)
        {
            var culturaAr = new CultureInfo("es-AR");
            string negocio = model.Empresa != null
                ? (model.Empresa.NombreFantasia ?? model.Empresa.RazonSocialAfip ?? "CarniSys")
                : "CarniSys";

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);

                        // ===== CABECERA =====
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(izq =>
                            {
                                izq.Item().Text(negocio).FontSize(20).FontColor("#AE0000");
                                izq.Item().PaddingTop(4).Text("Razón Social: " + (model.Empresa?.RazonSocialAfip ?? "")).FontSize(8);
                                izq.Item().Text(((model.Empresa?.Domicilio ?? "")) + " - " + (model.Empresa?.Ciudad ?? "")).FontSize(8);
                                izq.Item().Text("Cond.IVA: " + (model.Empresa?.CondicionIVA ?? "")).FontSize(8);
                            });

                            row.RelativeItem().AlignCenter().Column(centro =>
                            {
                                centro.Item().AlignCenter().Text("X").FontSize(35).Bold();
                                centro.Item().AlignCenter().Text("- Documento no válido como factura -").FontSize(7);
                            });

                            row.RelativeItem().AlignRight().Column(der =>
                            {
                                der.Item().AlignRight().Text("N°Recibo: " + (model.Pago.NroRecibo ?? "")).Bold();
                                der.Item().AlignRight().Text("Fecha: " + model.Pago.Fecha.ToString("dd/MM/yyyy"));
                                der.Item().AlignRight().Text(model.Empresa != null ? model.Empresa.Iibb.ToString() : "");
                                der.Item().AlignRight().Text("CUIT: " + (model.Empresa != null ? model.Empresa.Cuit.ToString() : ""));
                                der.Item().AlignRight().Text("Inicio Act.: " + (model.Empresa != null ? model.Empresa.InicioActividad.ToString("dd/MM/yyyy") : ""));
                            });
                        });

                        col.Item().LineHorizontal(1.5f).LineColor(Colors.Grey.Medium);

                        // ===== PERSONA =====
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(4.5f);
                                c.RelativeColumn(1f);
                                c.RelativeColumn(2f);
                            });

                            table.Cell().Text(model.PersonaEtiqueta + ":").Bold();
                            table.Cell().Text((model.Pago.Persona?.RazonSocial ?? "").ToUpperInvariant());
                            table.Cell().Text("Cond. IVA:").Bold();
                            table.Cell().Text(model.Pago.Persona?.Iva ?? "");
                            table.Cell().Text("Domicilio:").Bold();
                            table.Cell().Text((model.Pago.Persona?.Domicilio ?? "").ToUpperInvariant());
                            table.Cell().Text("CUIT:").Bold();
                            table.Cell().Text(model.Pago.Persona?.Cuit ?? "");
                        });

                        col.Item().LineHorizontal(1.5f).LineColor(Colors.Grey.Medium);

                        // ===== FORMA DE PAGO / DETALLE / IMPORTE =====
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(20);
                                c.RelativeColumn(60);
                                c.RelativeColumn(20);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#FFC8C8").Padding(4).AlignCenter().Text("Forma Pago").Bold();
                                h.Cell().Background("#FFC8C8").Padding(4).AlignCenter().Text("Detalle").Bold();
                                h.Cell().Background("#FFC8C8").Padding(4).AlignCenter().Text("Importe").Bold();
                            });

                            table.Cell().Padding(4).AlignCenter().Text(model.Pago.FormaPago ?? "");
                            table.Cell().Padding(4).Text(model.Pago.Observaciones ?? "");
                            table.Cell().Padding(4).AlignRight().Text(model.Pago.Importe.ToString("F2", CultureInfo.InvariantCulture));
                        });

                        col.Item().AlignRight().PaddingTop(4).Text("Total: $ " + model.Pago.Importe.ToString("#,##0.00", culturaAr)).Bold();

                        if (model.TieneSaldo)
                        {
                            col.Item().AlignRight().Text("[ Saldo: $ " + model.Saldo.ToString("N2", culturaAr) + " ]");
                        }

                        col.Item().PaddingTop(6).Text(model.TipoOperacion.ToUpperInvariant() + " - " + model.DetalleOperacion).Bold();

                        if (model.Pago.Sucursal != null)
                            col.Item().Text("Sucursal: " + (model.Pago.Sucursal.SucursalNombre ?? ""));
                        if (model.Pago.CreadoPor != null)
                            col.Item().Text("Usuario: " + (model.Pago.CreadoPor.Nombre ?? ""));
                    });
                });
            });

            return documento.GeneratePdf();
        }

        // Port de Web/Controllers/HomeController.cs GenerarPdfCalculadoraBilletes -- mismo
        // contenido/orden, sintaxis QuestPDF en vez de iTextSharp (batch 7 POS, ver
        // docs/10-migracion-aspnet-core/PLAN-POS-UI.md). El armado del titulo/detalle
        // (NormalizarDetalleCalculadoraBilletes) queda en el controller, igual que el original.
        public static byte[] GenerarPdfCalculadoraBilletes(string titulo, decimal total, string detalle)
        {
            var culturaAr = new CultureInfo("es-AR");
            string tituloFinal = string.IsNullOrWhiteSpace(titulo) ? "Detalle de billetes" : titulo.Trim();

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);
                        col.Item().Text(tituloFinal).FontSize(16).Bold();
                        col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        col.Item().PaddingTop(6).Text("Total $: " + total.ToString("N2", culturaAr)).FontSize(12).Bold();
                        col.Item().Text("Detalles:").FontSize(12).Bold();
                        col.Item().Text(detalle ?? "");
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public static byte[] GenerarFacturaPDF(Entidades.Venta venta, Entidades.FacturaElectronica factura = null)
        {
            var culturaAr = new CultureInfo("es-AR");
            bool esFacturaA = venta.TipoComprobante == 'A';
            bool agruparItemUnitario = factura != null && !string.IsNullOrWhiteSpace(factura.DescItemUnitario);
            var empresaFactura = ObtenerEmpresaFactura(venta, factura);
            byte[] qrBytes = factura != null ? GenerateQRCode(factura, venta) : null;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);

                        // ===== CABECERA =====
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(izq =>
                            {
                                izq.Item().Text(empresaFactura != null ? empresaFactura.NombreFantasia ?? "" : "").FontSize(20).FontColor("#AE0000").Bold();

                                if (venta.TipoComprobante != 'X')
                                {
                                    izq.Item().PaddingTop(6).Text("Razón Social: " + (empresaFactura != null ? empresaFactura.RazonSocialAfip : ""));
                                    izq.Item().Text(((empresaFactura != null ? empresaFactura.Domicilio : "") ?? "")
                                        + (string.IsNullOrWhiteSpace(empresaFactura != null ? empresaFactura.Ciudad : "") ? "" : " - " + empresaFactura.Ciudad));
                                    izq.Item().Text("Cond. IVA: " + (empresaFactura != null ? empresaFactura.CondicionIVA : ""));
                                }
                            });

                            row.RelativeItem().AlignCenter().Column(centro =>
                            {
                                if (venta.TipoComprobante == 'X')
                                {
                                    centro.Item().AlignCenter().Text("X").FontSize(34).Bold();
                                    centro.Item().AlignCenter().Text("- Documento no válido como factura -").FontSize(7);
                                }
                                else
                                {
                                    string codFactura = "COD." + (factura.CodTipoCbteAfip < 10 ? "0" + factura.CodTipoCbteAfip : factura.CodTipoCbteAfip.ToString());
                                    centro.Item().AlignCenter().Text(venta.TipoComprobante.ToString()).FontSize(34).Bold();
                                    centro.Item().AlignCenter().Text(codFactura).FontSize(7);
                                }
                            });

                            row.RelativeItem().AlignRight().Column(der =>
                            {
                                if (venta.TipoComprobante == 'X')
                                {
                                    der.Item().AlignRight().Text("N° Comprobante: " + venta.IdVenta).Bold();
                                    der.Item().AlignRight().Text("Fecha: " + venta.FechaVenta.ToString("dd/MM/yyyy"));
                                }
                                else
                                {
                                    string descComprobante = QuitarUltimoCaracterSiCorresponde(factura != null ? factura.DescTipoCbteAfip : "");
                                    der.Item().AlignRight().Text(descComprobante.ToUpper()).Bold();
                                    der.Item().AlignRight().Text("Nro.Comp.: " + (factura != null ? factura.PtoVtaAfip : "") + "-" + (factura != null ? factura.NroCbteAfip : ""));
                                    der.Item().AlignRight().Text("Fecha de Emisión: " + ((factura != null && factura.FechaEmisionAfip.HasValue) ? factura.FechaEmisionAfip.Value.Date.ToString("dd/MM/yyyy") : venta.FechaVenta.ToString("dd/MM/yyyy")));
                                    der.Item().AlignRight().Text("IIBB: " + (empresaFactura != null ? empresaFactura.Iibb.ToString() : ""));
                                    der.Item().AlignRight().Text("CUIT: " + (empresaFactura != null ? empresaFactura.Cuit.ToString() : ""));
                                    der.Item().AlignRight().Text("Inicio Act.: " + ((empresaFactura != null && empresaFactura.InicioActividad != DateTime.MinValue) ? empresaFactura.InicioActividad.Date.ToString("dd/MM/yyyy") : ""));
                                }
                            });
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // ===== CLIENTE =====
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(4.5f);
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(2.5f);
                            });

                            table.Cell().Text("Cliente:").Bold();
                            table.Cell().Text(factura != null ? factura.RazonSocialAFIP : venta.Persona.razonSocial);
                            table.Cell().Text("CUIT:").Bold();
                            table.Cell().Text(factura != null ? factura.NroDocAfip : venta.Persona.Cuit);

                            table.Cell().Text("Domicilio:").Bold();
                            table.Cell().Text(factura != null ? factura.DomicilioAFIP : venta.Persona.Domicilio);
                            table.Cell().Text("Cond. IVA:").Bold();
                            table.Cell().Text(factura != null ? factura.CondicionIvaAFIP : venta.Persona.Iva);

                            table.Cell().Text("Forma pago:").Bold();
                            table.Cell().Text(factura != null ? factura.FormaPago : venta.FormaPago);
                            table.Cell().AlignRight().Text(factura != null && !string.IsNullOrWhiteSpace(factura.ComprobanteAsociadoInfo) ? "Cbte Asoc:" : "").Bold();
                            table.Cell().AlignRight().Text(factura != null ? (factura.ComprobanteAsociadoInfo ?? "") : "");
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // ===== PRODUCTOS =====
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(6);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                if (esFacturaA) c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#FFC8C8").Padding(4).Text("Descripción").Bold();
                                h.Cell().Background("#FFC8C8").Padding(4).AlignRight().Text("Cantidad").Bold();
                                h.Cell().Background("#FFC8C8").Padding(4).AlignRight().Text("Precio Un.").Bold();
                                if (esFacturaA) h.Cell().Background("#FFC8C8").Padding(4).AlignRight().Text("IVA").Bold();
                                h.Cell().Background("#FFC8C8").Padding(4).AlignRight().Text("Importe").Bold();
                            });

                            if (agruparItemUnitario)
                            {
                                decimal totalAgrupado = esFacturaA ? Convert.ToDecimal(factura.ImporteNetoGravado) : Convert.ToDecimal(factura.ImporteTotal);

                                table.Cell().Padding(4).Text(factura.DescItemUnitario);
                                table.Cell().Padding(4).AlignRight().Text("1,000");
                                table.Cell().Padding(4).AlignRight().Text(totalAgrupado.ToString("#,##0.00", culturaAr));
                                if (esFacturaA) table.Cell().Padding(4).AlignRight().Text("");
                                table.Cell().Padding(4).AlignRight().Text(totalAgrupado.ToString("#,##0.00", culturaAr));
                            }
                            else
                            {
                                foreach (var l in venta.LineasVenta)
                                {
                                    table.Cell().Padding(4).Text("[Cód. " + l.Corte.Codigo + "] " + l.Corte.corte);
                                    table.Cell().Padding(4).AlignRight().Text(l.CantKg.ToString("F3"));
                                    table.Cell().Padding(4).AlignRight().Text(l.PrecioKg.ToString("#,##0.00", culturaAr));
                                    if (esFacturaA) table.Cell().Padding(4).AlignRight().Text(l.AlicuotaIva.ToString("#,##0.00"));
                                    table.Cell().Padding(4).AlignRight().Text((l.CantKg * l.PrecioKg).ToString("#,##0.00", culturaAr));
                                }
                            }
                        });

                        string importeTexto = ConvertirMontoEnTexto(venta, factura);
                        string observaciones = ObtenerObservacionesComprobante(venta, factura);

                        if (factura == null)
                        {
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem(5).Text(string.IsNullOrEmpty(observaciones) ? importeTexto : importeTexto + "\n-------\n" + observaciones);
                                row.RelativeItem(1).AlignRight().Text("TOTAL:").Bold();
                                row.RelativeItem(1).AlignRight().Text(venta.TotalImporte.ToString("#,##0.00", culturaAr)).Bold();
                            });
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        }
                        else
                        {
                            col.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text(importeTexto);
                                row.RelativeItem().Column(totCol =>
                                {
                                    if (esFacturaA)
                                    {
                                        totCol.Item().AlignRight().Text("Neto s/iva: $ " + factura.ImporteNetoGravado.ToString("#,##0.00", culturaAr)).Bold();
                                        foreach (var item in (factura.ListaAlicuota ?? new System.Collections.Generic.List<Entidades.AlicuotaIva>()).Where(a => a.Importe > 0))
                                            totCol.Item().AlignRight().Text("Iva " + item.Iva + "%: $ " + item.Importe.ToString("#,##0.00", culturaAr)).Bold();
                                    }
                                    else
                                    {
                                        totCol.Item().AlignRight().Text("Subtotal: $ " + factura.ImporteTotal.ToString("#,##0.00", culturaAr)).Bold();
                                    }

                                    totCol.Item().AlignRight().Text("Total: $ " + factura.ImporteTotal.ToString("#,##0.00", culturaAr)).Bold();
                                });
                            });

                            if (!string.IsNullOrWhiteSpace(observaciones))
                                col.Item().Text("Obs: " + observaciones);

                            col.Item().PaddingTop(8).AlignRight().Text("Régimen de Transparencia Fiscal Al Consumidor (Ley 27.743)").FontSize(7);
                            col.Item().AlignRight().Text("IVA Contenido: " + factura.Iva.ToString("N2")).FontSize(7);

                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.ConstantItem(100).Column(qrCol =>
                                {
                                    if (qrBytes != null && qrBytes.Length > 0)
                                        qrCol.Item().Width(100).Height(100).Image(qrBytes);
                                });

                                row.RelativeItem().PaddingLeft(15).Column(caeCol =>
                                {
                                    caeCol.Item().AlignRight().Text("CAE: " + factura.CAE1).FontSize(7);
                                    caeCol.Item().AlignRight().Text("Fecha de Vencimiento del CAE: " + factura.FecVtoCAE).FontSize(7);
                                });
                            });
                        }
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public static byte[] GenerateQRCode(Entidades.FacturaElectronica factura, Entidades.Venta venta = null)
        {
            try
            {
                string data = GenerarQrUrl(factura, venta);
                if (string.IsNullOrWhiteSpace(data))
                    return null;

                var qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                return qrCode.GetGraphic(20);
            }
            catch
            {
                return null;
            }
        }

        // URL completa del QR oficial de AFIP (RG 4892/2020) para un comprobante ya autorizado.
        public static string GenerarQrUrl(Entidades.FacturaElectronica factura, Entidades.Venta venta = null)
        {
            try
            {
                string payload = GenerarJSON(factura, venta);
                if (string.IsNullOrWhiteSpace(payload))
                    return "";

                return "https://www.afip.gob.ar/fe/qr/?p=" + payload;
            }
            catch
            {
                return "";
            }
        }

        private static string GenerarJSON(Entidades.FacturaElectronica factura, Entidades.Venta venta = null)
        {
            if (factura == null)
                return "";

            string fechaEmision = factura.FechaEmisionAfip?.ToString("yyyy-MM-dd");
            var empresaFactura = ObtenerEmpresaFactura(venta, factura);

            long cuitEmisor = ParseLongSeguro(empresaFactura != null ? empresaFactura.Cuit.ToString() : "");
            long nroCmp = ParseLongSeguro(factura.NroCbteAfip);
            long nroDocRec = ParseLongSeguro(factura.NroDocAfip);
            long codAut = ParseLongSeguro(factura.CAE1);
            int ptoVta = ParseIntSeguro(factura.PtoVtaAfip);

            if (cuitEmisor <= 0 || nroCmp <= 0 || codAut <= 0 || ptoVta <= 0)
                return "";

            // factura.TipoDocAfip guarda el codigo numerico de AFIP como string (ej. "80"=CUIT,
            // "96"=DNI, "99"=consumidor final/sin identificar).
            int tipoDocRec;
            if (!int.TryParse(factura.TipoDocAfip, out tipoDocRec) || tipoDocRec <= 0)
                tipoDocRec = 99;

            decimal importe = Convert.ToDecimal(factura.ImporteTotal);

            var qrData = new
            {
                ver = 1,
                fecha = fechaEmision,
                cuit = cuitEmisor,
                ptoVta,
                tipoCmp = factura.CodTipoCbteAfip,
                nroCmp,
                importe,
                moneda = "PES",
                ctz = 1,
                tipoDocRec,
                nroDocRec,
                tipoCodAut = "E",
                codAut
            };

            string jsonData = JsonSerializer.Serialize(qrData);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));
        }

        private static Entidades.Empresa ObtenerEmpresaFactura(Entidades.Venta venta, Entidades.FacturaElectronica factura)
        {
            return (venta != null && venta.Sucursal != null ? venta.Sucursal.Empresa : null)
                ?? (factura != null && factura.Venta != null && factura.Venta.Sucursal != null ? factura.Venta.Sucursal.Empresa : null);
        }

        private static string QuitarUltimoCaracterSiCorresponde(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "FACTURA";

            texto = texto.Trim();
            return texto.Length > 1 ? texto.Substring(0, texto.Length - 1) : texto;
        }

        private static string ObtenerObservacionesComprobante(Entidades.Venta venta, Entidades.FacturaElectronica factura)
        {
            if (factura != null)
                return factura.Observaciones ?? "";

            return venta != null ? (venta.Observaciones ?? "") : "";
        }

        private static long ParseLongSeguro(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return 0;

            string soloDigitos = new string(texto.Where(char.IsDigit).ToArray());
            long valor;
            return long.TryParse(soloDigitos, out valor) ? valor : 0;
        }

        private static int ParseIntSeguro(string texto)
        {
            int valor;
            return int.TryParse(texto, out valor) ? valor : 0;
        }

        private static string ConvertirMontoEnTexto(Entidades.Venta venta, Entidades.FacturaElectronica factura)
        {
            float importeFloat = factura != null && factura.Id > 0 ? factura.ImporteTotal : venta.TotalImporte;
            decimal monto = Convert.ToDecimal(importeFloat);
            if (monto == 0)
                return "Cero";

            string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            string[] decenas = { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            string[] especiales = { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
            string[] centenas = { "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

            int millones = (int)(monto / 1000000);
            monto %= 1000000;
            int miles = (int)(monto / 1000);
            monto %= 1000;
            int cientos = (int)monto;
            monto -= cientos;

            int centavos = (int)((monto - Math.Truncate(monto)) * 100);

            string resultado = "Son pesos ";

            if (millones > 0)
                resultado += (millones > 1 ? ConvertirCentena(millones, unidades, decenas, especiales, centenas) + " millones " : "un millón ");

            if (miles > 0)
                resultado += (miles > 1 ? ConvertirCentena(miles, unidades, decenas, especiales, centenas) + " mil " : "mil ");

            if (cientos > 0)
                resultado += ConvertirCentena(cientos, unidades, decenas, especiales, centenas);

            if (centavos > 0)
                resultado += " con " + ConvertirCentena(centavos, unidades, decenas, especiales, centenas) + " centavos";

            return resultado.Trim();
        }

        private static string ConvertirCentena(int numero, string[] unidades, string[] decenas, string[] especiales, string[] centenas)
        {
            if (numero == 0) return "";

            string texto = "";

            if (numero > 99)
            {
                if (numero == 100)
                    return "cien";

                texto = centenas[numero / 100] + " ";
                numero %= 100;
            }

            if (numero > 19)
                texto += decenas[numero / 10] + (numero % 10 > 0 ? " y " + unidades[numero % 10] : "");
            else if (numero >= 10)
                texto += especiales[numero - 10];
            else if (numero > 0)
                texto += unidades[numero];

            return texto.Trim();
        }

        // Port de Web/Controllers/ProductosController.cs GenerarPdfEtiquetas/DibujarEtiqueta
        // (Modulo 3, etiquetas de producto) -- mismas proporciones/3 tamanos que el original
        // (iTextSharp, posicionamiento absoluto con ColumnText/PdfContentByte), reescrito
        // declarativo con QuestPDF: en vez de calcular filas/columnas por pagina y llamar
        // document.NewPage() a mano, se arma una fila de ancho fijo por cada "fila" de etiquetas
        // (ConstantItem, no Grid -- Grid de QuestPDF reparte el ancho disponible en columnas
        // relativas, lo que estiraria cada etiqueta mas alla de su tamano fisico real, rompiendo
        // la alineacion con la hoja de stickers pre-cortada) y se deja que QuestPDF autopagine
        // cuando el contenido no entra, mismo resultado visual con menos codigo manual.
        //
        // Codigo de barras (EAN-13/EAN-8/Code128, con el mismo digito verificador que valida el JS
        // de esta vista) via ZXing.Net + SVG -- QuestPDF no genera codigos de barra nativamente
        // (su propia documentacion recomienda ZXing.Net via SVG), y SVG evita System.Drawing
        // (mismo motivo que QRCoder/PngByteQRCode en el resto de este archivo).
        private sealed class TamanoEtiqueta
        {
            public float AnchoMm;
            public float AltoMm;
            public bool MostrarLogo;
            public bool MostrarFecha;
            public float FuenteNombreGrande;
            public float FuenteNombreMedia;
            public float FuenteNombreChica;
            public float FuentePrecio;
            public float FuentePrecioLabel;
            public float FuenteFecha;
        }

        private static TamanoEtiqueta ResolverTamanoEtiquetaInterno(string tamano)
        {
            switch ((tamano ?? "").Trim().ToLowerInvariant())
            {
                case "chica":
                    return new TamanoEtiqueta
                    {
                        AnchoMm = 40,
                        AltoMm = 30,
                        MostrarLogo = false,
                        MostrarFecha = true,
                        FuenteNombreGrande = 6.3f,
                        FuenteNombreMedia = 5.6f,
                        FuenteNombreChica = 4.9f,
                        FuentePrecio = 20,
                        FuentePrecioLabel = 4,
                        FuenteFecha = 3
                    };
                case "grande":
                    return new TamanoEtiqueta
                    {
                        AnchoMm = 100,
                        AltoMm = 50,
                        MostrarLogo = true,
                        MostrarFecha = true,
                        FuenteNombreGrande = 14f,
                        FuenteNombreMedia = 11.9f,
                        FuenteNombreChica = 10.5f,
                        FuentePrecio = 46,
                        FuentePrecioLabel = 7,
                        FuenteFecha = 6
                    };
                case "mediana":
                default:
                    return new TamanoEtiqueta
                    {
                        AnchoMm = 60,
                        AltoMm = 35,
                        MostrarLogo = true,
                        MostrarFecha = true,
                        FuenteNombreGrande = 9.1f,
                        FuenteNombreMedia = 7.7f,
                        FuenteNombreChica = 7f,
                        FuentePrecio = 30,
                        FuentePrecioLabel = 5,
                        FuenteFecha = 4
                    };
            }
        }

        // logoBytes: leido por el controller (IWebHostEnvironment.WebRootPath), no aca -- mismo
        // criterio ya establecido en VentasController para AFIP (_env.ContentRootPath), este
        // servicio se mantiene libre de acceso a disco/DI.
        public static byte[] GenerarPdfEtiquetas(List<Entidades.Corte> productos, string tamano, byte[] logoBytes)
        {
            var tam = ResolverTamanoEtiquetaInterno(tamano);
            string fechaImpresion = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            float espacioMm = 2f;
            float margenMm = 10f;
            float anchoDisponibleMm = 210f - 2 * margenMm;
            int etiquetasPorFila = Math.Max(1, (int)((anchoDisponibleMm + espacioMm) / (tam.AnchoMm + espacioMm)));

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(margenMm, Unit.Millimetre);

                    page.Content().Column(mainCol =>
                    {
                        mainCol.Spacing(espacioMm, Unit.Millimetre);

                        for (int i = 0; i < productos.Count; i += etiquetasPorFila)
                        {
                            var fila = productos.GetRange(i, Math.Min(etiquetasPorFila, productos.Count - i));

                            mainCol.Item().Row(row =>
                            {
                                row.Spacing(espacioMm, Unit.Millimetre);

                                foreach (var producto in fila)
                                {
                                    // ShowEntire: cada etiqueta es un bloque fisico fijo (60x35mm,
                                    // etc.) que tiene que quedar completo en una sola pagina -- sin
                                    // esto, QuestPDF puede partir el contenido de una etiqueta a la
                                    // mitad entre 2 paginas cuando no entra el resto de la fila
                                    // (bug real encontrado al verificar con datos reales: 2
                                    // productos generaban 2 paginas, nombre/precio en una y
                                    // codigo de barras/fecha en la otra).
                                    row.ConstantItem(tam.AnchoMm, Unit.Millimetre)
                                        .ShowEntire()
                                        .Height(tam.AltoMm, Unit.Millimetre)
                                        .Element(c => DibujarEtiqueta(c, producto, tam, logoBytes, fechaImpresion));
                                }
                            });
                        }
                    });
                });
            });

            return documento.GeneratePdf();
        }

        private static void DibujarEtiqueta(IContainer container, Entidades.Corte producto, TamanoEtiqueta tam, byte[] logoBytes, string fechaImpresion)
        {
            float padMm = tam.AnchoMm * 0.05f;
            string nombre = (producto.CorteDesc ?? "").ToUpperInvariant();
            float fuenteNombre = nombre.Length > 40 ? tam.FuenteNombreChica : (nombre.Length > 25 ? tam.FuenteNombreMedia : tam.FuenteNombreGrande);

            container
                .Border(0.4f)
                .BorderColor(Colors.Grey.Lighten1)
                // Padding mayormente horizontal (no en las 4 direcciones parejo): el original
                // (iTextSharp) usa "pad" como margen de contenido horizontal, con las bandas
                // verticales (headerH/precioH/footerH) ya ocupando casi el 100% del alto real --
                // aplicar el mismo valor tambien arriba/abajo le resta a la etiqueta ~2*padMm de
                // alto disponible antes de dibujar una sola linea, y sumado al interlineado propio
                // de QuestPDF (ver mas abajo) es lo que tiraba DocumentLayoutException bajo
                // ShowEntire() con datos reales (5 productos, "mediana").
                .PaddingHorizontal(padMm, Unit.Millimetre)
                .PaddingVertical(padMm * 0.25f, Unit.Millimetre)
                .Column(col =>
                {
                    // --- Encabezado: nombre (izquierda) + logo (derecha) ---
                    // Sin .Height() fijo (ver comentario de padding arriba, mismo motivo): se deja
                    // que cada seccion mida su alto natural -- el tamaño de fuente (ya calibrado
                    // por tam.Fuente*, ver ResolverTamanoEtiquetaInterno) sigue siendo el mismo que
                    // el original, la etiqueta completa sigue entrando en su tamaño fisico fijo.
                    // LineHeight(1): QuestPDF reserva interlineado extra por defecto (no aplica en
                    // el posicionamiento absoluto glifo-por-glifo del original) -- sin achicarlo,
                    // cada linea de texto pesa mas de lo que el layout original asumia.
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().AlignMiddle().Text(nombre).FontSize(fuenteNombre).LineHeight(1f);

                        if (tam.MostrarLogo && logoBytes != null)
                        {
                            // Causa real de un DocumentLayoutException encontrado con datos
                            // reales: sin un alto explicito, FitArea calcula el alto natural del
                            // logo a partir de SU PROPIO ancho + relacion de aspecto (aca, ancho
                            // generoso / aspecto 1.4 = ~12.5mm) en vez de respetar la proporcion
                            // del encabezado -- eso solo ya superaba el 30% de alto pensado para
                            // esta fila, e inflaba toda la etiqueta por encima de tam.AltoMm. El
                            // original tambien acota ambas dimensiones (logoMaxW/logoMaxH), no
                            // solo el ancho.
                            row.ConstantItem(tam.AnchoMm * 0.30f, Unit.Millimetre)
                                .Height(tam.AltoMm * 0.30f * 0.85f, Unit.Millimetre)
                                .AlignMiddle().AlignRight()
                                .Image(logoBytes).FitArea();
                        }
                    });

                    // --- Linea divisoria ---
                    col.Item().PaddingVertical(0.2f, Unit.Millimetre).LineHorizontal(0.75f).LineColor(Colors.Grey.Darken2);

                    // --- Precio grande + etiqueta "precio por kg" / "precio unitario" ---
                    col.Item().Column(precioCol =>
                    {
                        precioCol.Item().AlignRight().Text(text =>
                        {
                            text.DefaultTextStyle(s => s.LineHeight(1f));
                            text.Span("$").FontSize(tam.FuentePrecio * 0.45f).Bold();
                            text.Span(producto.PrecioKg.ToString("#,0.00", CultureInfo.InvariantCulture)).FontSize(tam.FuentePrecio).Bold();
                        });

                        precioCol.Item().AlignRight().Text(producto.Pesable ? "PRECIO POR KG" : "PRECIO UNITARIO")
                            .FontSize(tam.FuentePrecioLabel).Bold().FontColor(Colors.Grey.Darken3).LineHeight(1f);
                    });

                    // --- Pie: codigo de barras (izquierda) + fecha de emision (derecha) ---
                    // Alto acotado explicitamente (mismo motivo que el logo del encabezado, ver
                    // comentario de arriba): sin un limite de alto, el SVG del codigo de barras
                    // puede pedir mas espacio del pensado para esta fila.
                    col.Item().Row(row =>
                    {
                        string svg = GenerarBarcodeSvg(producto.Codigo);
                        if (svg != null)
                        {
                            row.RelativeItem().Height(tam.AltoMm * 0.24f, Unit.Millimetre)
                                .AlignLeft().AlignMiddle().Svg(svg).FitArea();
                        }
                        else
                        {
                            row.RelativeItem().AlignLeft().AlignMiddle().Text("Cód: " + producto.Codigo).FontSize(tam.FuenteFecha).LineHeight(1f);
                        }

                        if (tam.MostrarFecha)
                        {
                            row.ConstantItem(tam.AnchoMm * 0.42f, Unit.Millimetre).Column(fechaCol =>
                            {
                                fechaCol.Item().AlignRight().Text("FECHA DE EMISIÓN").FontSize(tam.FuenteFecha).Bold().FontColor(Colors.Grey.Darken3).LineHeight(1f);
                                fechaCol.Item().AlignRight().Text(fechaImpresion).FontSize(tam.FuenteFecha).FontColor(Colors.Grey.Darken3).LineHeight(1f);
                            });
                        }
                    });
                });
        }

        // Codifica producto.Codigo como EAN-13/EAN-8 cuando el digito verificador da valido
        // (mismo criterio que isValidEAN13/isValidEAN8 del JS de Productos/Index.cshtml -- una
        // sola definicion de "EAN valido" en todo el proyecto), con padding de ceros a la
        // izquierda porque Codigo se guarda como long y pierde el cero inicial de un EAN real. Si
        // no valida como EAN, cae a Code128 (cualquier largo numerico) -- sigue siendo escaneable,
        // igual que el original.
        private static string GenerarBarcodeSvg(long codigo)
        {
            try
            {
                string digitos = codigo.ToString(CultureInfo.InvariantCulture);
                string ean13 = digitos.PadLeft(13, '0');
                string ean8 = digitos.PadLeft(8, '0');

                ZXing.BarcodeFormat formato;
                string valor;

                if (digitos.Length <= 13 && EsEan13Valido(ean13))
                {
                    formato = ZXing.BarcodeFormat.EAN_13;
                    valor = ean13;
                }
                else if (digitos.Length <= 8 && EsEan8Valido(ean8))
                {
                    formato = ZXing.BarcodeFormat.EAN_8;
                    valor = ean8;
                }
                else
                {
                    formato = ZXing.BarcodeFormat.CODE_128;
                    valor = digitos;
                }

                var writer = new ZXing.MultiFormatWriter();
                var matriz = writer.encode(valor, formato, 300, 100);
                var renderer = new ZXing.Rendering.SvgRenderer();
                return renderer.Render(matriz, formato, valor).Content;
            }
            catch
            {
                return null;
            }
        }

        private static bool EsEan13Valido(string code13)
        {
            if (code13 == null || code13.Length != 13 || !code13.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 12; i++)
            {
                int digito = code13[i] - '0';
                suma += (i % 2 == 0) ? digito : digito * 3;
            }
            int check = (10 - (suma % 10)) % 10;
            return check == (code13[12] - '0');
        }

        private static bool EsEan8Valido(string code8)
        {
            if (code8 == null || code8.Length != 8 || !code8.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 7; i++)
            {
                int digito = code8[i] - '0';
                suma += (i % 2 == 0) ? digito * 3 : digito;
            }
            int check = (10 - (suma % 10)) % 10;
            return check == (code8[7] - '0');
        }

        // Port de MovimientosController.GenerarPdfMovimiento (iTextSharp) a QuestPDF -- mismo
        // contenido/orden (cabecera + tabla de lineas + totales), Modulo Movimientos.
        public static byte[] GenerarPdfMovimiento(Entidades.Movimiento movimiento, List<Entidades.CortePorMovimiento> lineas)
        {
            var culturaInv = CultureInfo.InvariantCulture;
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24, Unit.Point);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(4);

                        col.Item().Text("Movimiento").FontSize(16).Bold();
                        col.Item().Text("ID: " + movimiento.IdMovimiento);
                        col.Item().Text("Origen: " + (movimiento.SucursalOrigen != null ? movimiento.SucursalOrigen.SucursalNombre : "-"));
                        col.Item().Text("Destino: " + (movimiento.SucursalDestino != null ? movimiento.SucursalDestino.SucursalNombre : "-"));
                        col.Item().Text("Fecha: " + movimiento.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"));

                        if (!string.IsNullOrWhiteSpace(movimiento.Observaciones))
                            col.Item().Text("Observaciones: " + movimiento.Observaciones);

                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2.5f);
                                c.RelativeColumn(6f);
                                c.RelativeColumn(2f);
                                c.RelativeColumn(2f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Código").Bold();
                                h.Cell().Text("Producto").Bold();
                                h.Cell().Text("Cant. Un.").Bold();
                                h.Cell().Text("Kgs.").Bold();
                            });

                            foreach (var linea in lineas)
                            {
                                table.Cell().Text(linea.Corte != null ? linea.Corte.Codigo.ToString() : "");
                                table.Cell().Text(linea.Corte != null ? linea.Corte.CorteDesc : "");
                                table.Cell().Text(linea.CantUnidad.ToString());
                                table.Cell().Text(linea.CantKg.ToString("F3", culturaInv));
                            }
                        });

                        col.Item().PaddingTop(6).Text("Total unidades: " + lineas.Sum(x => x.CantUnidad)).Bold();
                        col.Item().Text("Total kilos: " + lineas.Sum(x => x.CantKg).ToString("F3", culturaInv)).Bold();
                    });
                });
            });

            return documento.GeneratePdf();
        }
    }
}
