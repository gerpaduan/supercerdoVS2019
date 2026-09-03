// Port de Utilidades/GenerarDocs.cs (iTextSharp) a QuestPDF -- ver docs/10-migracion-aspnet-core/
// README.md. Solo porta lo que usa VentasController: GenerarFacturaPDF (factura/ticket A4) y el QR
// oficial de AFIP (RG 4892/2020). GenerarPdfCtaCtePersona (Finanzas) no se porta en este slice.
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
    }
}
