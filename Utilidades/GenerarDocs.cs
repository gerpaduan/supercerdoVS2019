using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Font = iTextSharp.text.Font;
using Formatting = Newtonsoft.Json.Formatting;
using Rectangle = iTextSharp.text.Rectangle;

// Asegúrate de instalar el paquete NuGet "QRCoder" en tu proyecto.
// Puedes hacerlo desde la consola del administrador de paquetes con:
// Install-Package QRCoder

namespace Utilidades
{
    public class GenerarDocs
    {
        public static byte[] GenerarPdfCtaCtePersona(DataTable dt,  DateTime fechaDesde)
        {
            using (MemoryStream ms = new MemoryStream())
            {

                // Obtener persona y saldo igual que en la vista
                string persona = "";
                decimal saldo = 0;

                if (dt.Rows.Count > 0)
                {
                    persona = dt.Rows[0]["razonSocial"].ToString();
                    saldo = Convert.ToDecimal(dt.Rows[dt.Rows.Count - 1]["Saldo"]);
                }


                var doc = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // =====================================================================
                // FUENTES
                // =====================================================================
                var fontTitulo = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
                var fontNormal = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL);
                var fontHeader = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD, new BaseColor(80, 80, 80));
                var fontImportePos = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL, new BaseColor(0, 120, 0));
                var fontImporteNeg = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL, new BaseColor(180, 0, 0));

                // =====================================================================
                // CABECERA PERSONA + SALDO
                // =====================================================================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 60f, 40f });

                PdfPCell personaCell = new PdfPCell(new Phrase(persona, fontTitulo));
                personaCell.Border = Rectangle.NO_BORDER;
                personaCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                headerTable.AddCell(personaCell);

                BaseColor grisClaro = new BaseColor(180, 180, 180); // gris suave
                BaseColor colorSaldo = saldo >= 0 ? new BaseColor(0, 120, 0) : new BaseColor(180, 0, 0);

                // Frase compuesta
                Phrase fraseSaldo = new Phrase();

                // Parte 1: "Saldo:" en gris claro
                fraseSaldo.Add(new Chunk(
                    "Saldo: ",
                    new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, grisClaro)
                ));

                // Parte 2: el valor, mantiene tu color original
                fraseSaldo.Add(new Chunk(
                    "$ "+saldo.ToString("N2"),
                    new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, colorSaldo)
                ));

                PdfPCell saldoCell = new PdfPCell(fraseSaldo);
                saldoCell.Border = Rectangle.NO_BORDER;
                saldoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                saldoCell.VerticalAlignment = Element.ALIGN_MIDDLE;

                headerTable.AddCell(saldoCell);


                doc.Add(headerTable);
                doc.Add(new Paragraph("\n", fontNormal));

                // =====================================================================
                // FECHA DESDE
                // =====================================================================
                doc.Add(new Paragraph($"Desde: {fechaDesde:dd/MM/yyyy}\n\n", fontNormal));


                // =====================================================================
                // TABLA PRINCIPAL (sin bordes, solo líneas suaves entre filas)
                // =====================================================================
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;

                table.SetWidths(new float[]
                {
            14f, // Fecha
            14f, // Tabla
            36f, // Detalle
            14f, // Importe (16 caracteres aprox)
            14f, // Saldo   (16 caracteres aprox)
            12f  // Sucursal
                });

                string[] cols = { "Fecha", "Operacion", "Detalle", "Importe", "Saldo", "Sucursal" };

                // -------- ENCABEZADOS --------
                foreach (var c in cols)
                {
                    PdfPCell h = new PdfPCell(new Phrase(c, fontHeader));
                    h.BackgroundColor = new BaseColor(240, 240, 240); // gris clarito
                    h.HorizontalAlignment = c.Equals("Importe") || c.Equals("Saldo") ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;
                    h.Padding = 5;

                    // Sin líneas alrededor
                    h.BorderWidth = 0;
                    h.BorderWidthBottom = 0.5f;
                    h.BorderColorBottom = new BaseColor(200, 200, 200);

                    table.AddCell(h);
                }

                // =====================================================================
                // FILAS
                // =====================================================================
                foreach (DataRow row in dt.Rows)
                {
                    BaseColor lineColor = new BaseColor(220, 220, 220);

                    // FECHA
                    PdfPCell celF = new PdfPCell(new Phrase(
                        Convert.ToDateTime(row["fecha"]).ToString("dd/MM/yyyy"), fontNormal));
                    celF.Padding = 5;
                    celF.Border = Rectangle.NO_BORDER;
                    celF.BorderWidthBottom = 0.5f;
                    celF.BorderColorBottom = lineColor;
                    table.AddCell(celF);

                    // TABLA
                    PdfPCell celT = new PdfPCell(new Phrase(row["tabla"].ToString(), fontNormal));
                    celT.Padding = 5;
                    celT.Border = Rectangle.NO_BORDER;
                    celT.BorderWidthBottom = 0.5f;
                    celT.BorderColorBottom = lineColor;
                    table.AddCell(celT);

                    // DETALLE (permite salto de línea)
                    PdfPCell celDet = new PdfPCell(new Phrase((string.IsNullOrEmpty(row["nroDoc"].ToString()) ? "" : (row["nroDoc"].ToString() + " | ")) + 
                        row["detalle"].ToString(), fontNormal));
                    celDet.Padding = 5;
                    celDet.NoWrap = false; // permitir salto
                    celDet.Border = Rectangle.NO_BORDER;
                    celDet.BorderWidthBottom = 0.5f;
                    celDet.BorderColorBottom = lineColor;
                    table.AddCell(celDet);

                    // IMPORTE (sin salto, derecha)
                    decimal imp = Convert.ToDecimal(row["importe"]);
                    PdfPCell celImp = new PdfPCell(new Phrase(
                        imp.ToString("N2"),
                        imp >= 0 ? fontImportePos : fontImporteNeg
                    ));
                    celImp.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celImp.NoWrap = true;
                    celImp.Padding = 5;
                    celImp.Border = Rectangle.NO_BORDER;
                    celImp.BorderWidthBottom = 0.5f;
                    celImp.BorderColorBottom = lineColor;
                    table.AddCell(celImp);

                    // SALDO (sin salto, derecha)
                    PdfPCell celSal = new PdfPCell(new Phrase(
                        Convert.ToDecimal(row["Saldo"]).ToString("N2"),
                        fontNormal
                    ));
                    celSal.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celSal.NoWrap = true;
                    celSal.Padding = 5;
                    celSal.Border = Rectangle.NO_BORDER;
                    celSal.BorderWidthBottom = 0.5f;
                    celSal.BorderColorBottom = lineColor;
                    table.AddCell(celSal);

                    // SUCURSAL
                    PdfPCell celSuc = new PdfPCell(new Phrase(row["Sucursal"].ToString(), fontNormal));
                    celSuc.Padding = 5;
                    celSuc.Border = Rectangle.NO_BORDER;
                    celSuc.BorderWidthBottom = 0.5f;
                    celSuc.BorderColorBottom = lineColor;
                    table.AddCell(celSuc);
                }

                doc.Add(table);

                doc.Close();
                return ms.ToArray();
            }
        }


        //#region imprimirVenta
        //public byte[] GenerarFacturaX(Entidades.Venta modelo)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        Document doc = new Document(PageSize.A4, 30, 30, 20, 20);
        //        PdfWriter.GetInstance(doc, ms);
        //        doc.Open();

        //        // ===== FUENTES =====
        //        var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 25, new BaseColor(174, 0, 0));
        //        var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        //        var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        //        var fuenteX = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35);
        //        var fuenteFooter = FontFactory.GetFont(FontFactory.HELVETICA, 7);

        //        // ===== CABECERA =====
        //        PdfPTable cabecera = new PdfPTable(3) { WidthPercentage = 100 };
        //        cabecera.SetWidths(new float[] { 33, 34, 33 });

        //        cabecera.AddCell(CeldaLibre("Nombre Negocio", fuenteTitulo));

        //        PdfPCell centro = new PdfPCell { Border = 0 };
        //        var p = new Paragraph { Alignment = Element.ALIGN_CENTER };
        //        p.Add(new Chunk("X\n", fuenteX));
        //        p.Add(new Chunk("- Documento no válido como factura -", fuenteFooter));
        //        centro.AddElement(p);
        //        cabecera.AddCell(centro);

        //        cabecera.AddCell(CeldaLibre(
        //            $"N°Comprobante: {modelo.IdVenta}\nFecha: {modelo.FechaVenta:dd/MM/yyyy}",
        //            fuenteNegrita,
        //            Element.ALIGN_RIGHT
        //        ));

        //        doc.Add(cabecera);
        //        doc.Add(new LineSeparator());

        //        // ===== CLIENTE =====
        //        PdfPTable cliente = new PdfPTable(4) { WidthPercentage = 100 };
        //        cliente.SetWidths(new float[] { 15, 45, 10, 30 });

        //        cliente.AddCell(CeldaSimple("Sr. (es):", fuenteNegrita));
        //        cliente.AddCell(CeldaSimple(modelo.Persona.razonSocial, fuenteNormal));
        //        cliente.AddCell(CeldaSimple("CUIT:", fuenteNegrita));
        //        cliente.AddCell(CeldaSimple(modelo.Persona.Cuit, fuenteNormal));

        //        doc.Add(cliente);

        //        // ===== PRODUCTOS =====
        //        PdfPTable prod = new PdfPTable(4) { WidthPercentage = 100 };
        //        prod.SetWidths(new float[] { 6, 2, 2, 2 });

        //        foreach (var l in modelo.LineasVenta)
        //        {
        //            prod.AddCell(CeldaSimple(l.Corte.corte, fuenteNormal));
        //            prod.AddCell(CeldaDerecha(l.CantKg.ToString("F3"), fuenteNormal));
        //            prod.AddCell(CeldaDerecha(l.PrecioKg.ToString("#,##0.00"), fuenteNormal));
        //            prod.AddCell(CeldaDerecha((l.CantKg * l.PrecioKg).ToString("#,##0.00"), fuenteNormal));
        //        }

        //        doc.Add(prod);

        //        // ===== TOTAL =====
        //        doc.Add(new LineSeparator());
        //        PdfPTable total = new PdfPTable(3) { WidthPercentage = 100 };
        //        total.SetWidths(new float[] { 5, 1, 1 });

        //        total.AddCell(CeldaLibre("", fuenteNormal));
        //        total.AddCell(CeldaDerecha("Total:", fuenteNegrita));
        //        total.AddCell(CeldaDerecha(modelo.TotalImporte.ToString("#,##0.00"), fuenteNegrita));

        //        doc.Add(total);

        //        // ===== OBS =====
        //        if (!string.IsNullOrEmpty(modelo.Observaciones))
        //            doc.Add(new Paragraph(modelo.Observaciones, FontFactory.GetFont(FontFactory.HELVETICA, 8)));

        //        doc.Close();
        //        return ms.ToArray();
        //    }
        //}

        //private PdfPCell CeldaSimple(string t, Font f) =>
        //    new PdfPCell(new Phrase(t, f)) { Border = 0, Padding = 4 };

        //private PdfPCell CeldaDerecha(string t, Font f) =>
        //    new PdfPCell(new Phrase(t, f)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT };

        //private PdfPCell CeldaLibre(string t, Font f, int align = Element.ALIGN_LEFT) =>
        //    new PdfPCell(new Phrase(t, f)) { Border = 0, HorizontalAlignment = align };
        //#endregion

        public byte[] GenerarFacturaPDF(Entidades.Venta venta, Entidades.FacturaElectronica factura = null)
        {
            using (var ms = new MemoryStream())
            {
                // ===== FUENTES =====
                var colorRojo = new BaseColor(174, 0, 0);
                var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 30, colorRojo);
                var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var fuenteComentarios = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fuenteX = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35);
                var fuenteFooter = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                var fuenteHeaderTabla = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

                // ===== MÁRGENES =====
                float margenIzquierdo = 30f;
                float margenDerecho = 30f;
                float margenSuperior = 20f;
                float margenInferior = 20f;

                // Si hay factura electrónica, reservamos espacio abajo para el footer fijo
                if (factura != null)
                {
                    margenInferior = CalcularMargenInferiorFactura(
                        venta,
                        factura,
                        fuenteComentarios,
                        fuenteNegrita,
                        fuenteFooter,
                        PageSize.A4.Width - margenIzquierdo - margenDerecho
                    );
                }

                Document doc = new Document(PageSize.A4, margenIzquierdo, margenDerecho, margenSuperior, margenInferior);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var linea = new LineSeparator(1.5f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -6);

                // ===== CABECERA =====
                doc.Add(GenerarCabecera(venta, factura, fuenteTitulo, fuenteNormal, fuenteNegrita, fuenteX, fuenteFooter));
                doc.Add(new Chunk(linea));
                doc.Add(new Paragraph(" "));

                // ===== CLIENTE =====
                doc.Add(GenerarCliente(venta, factura, fuenteNormal, fuenteNegrita));
                doc.Add(new Chunk(linea));

                // ===== PRODUCTOS =====
                doc.Add(GenerarProductos(venta, factura, fuenteNormal, fuenteHeaderTabla));
                
                // ===== TOTALES SOLO PARA COMPROBANTE X / SIN FACTURA =====
                if (factura == null)
                {
                    doc.Add(new Chunk(linea));
                    doc.Add(new Paragraph(" "));
                    doc.Add(GenerarTotal(venta, factura, fuenteNormal, fuenteNegrita));
                    doc.Add(new Chunk(linea));
                }

                doc.Close();

                byte[] pdfGenerado = ms.ToArray();

                // ===== SI HAY FACTURA, AGREGAMOS EL PIE FIJO SOLO EN LA ÚLTIMA PÁGINA =====
                if (factura != null)
                {
                    pdfGenerado = AgregarPieFijoUltimaPagina(
                        pdfGenerado,
                        venta,
                        factura,
                        fuenteComentarios,
                        fuenteNegrita,
                        fuenteFooter,
                        margenIzquierdo,
                        margenDerecho
                    );
                }

                return pdfGenerado;
            }
        }

        #region CABECERA

        private PdfPTable GenerarCabecera(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteTitulo,
            Font fuenteNormal,
            Font fuenteNegrita,
            Font fuenteX,
            Font fuenteFooter)
        {
            PdfPTable cabecera = new PdfPTable(3) { WidthPercentage = 100 };
            cabecera.SetWidths(new float[] { 33f, 34f, 33f });
            var empresaFactura = ObtenerEmpresaFactura(venta, factura);

            // IZQUIERDA
            PdfPCell izq = new PdfPCell { Border = 0 };
            izq.AddElement(new Paragraph((empresaFactura != null ? empresaFactura.NombreFantasia : "") + "\n", fuenteTitulo));

            if (venta.TipoComprobante != 'X')
            {
                izq.AddElement(new Paragraph("\nRazón Social: " + (empresaFactura != null ? empresaFactura.RazonSocialAfip : "") + "\n", fuenteNormal));
                izq.AddElement(new Paragraph(
                    ((empresaFactura != null ? empresaFactura.Domicilio : "") ?? "") +
                    (string.IsNullOrWhiteSpace(empresaFactura != null ? empresaFactura.Ciudad : "") ? "" : " - " + empresaFactura.Ciudad) + "\n",
                    fuenteNormal));
                izq.AddElement(new Paragraph("Cond. IVA: " + (empresaFactura != null ? empresaFactura.CondicionIVA : "") + "\n", fuenteNormal));
            }

            cabecera.AddCell(izq);

            // CENTRO
            PdfPCell centro = new PdfPCell { Border = 0 };
            Paragraph p = new Paragraph { Alignment = Element.ALIGN_CENTER };

            if (venta.TipoComprobante == 'X')
            {
                p.Add(new Chunk("X\n", fuenteX));
                p.Add(new Chunk("- Documento no válido como factura -", fuenteFooter));
            }
            else
            {
                string codFactura = "COD." + (factura.CodTipoCbteAfip < 10
                    ? ("0" + factura.CodTipoCbteAfip.ToString())
                    : factura.CodTipoCbteAfip.ToString());

                p.Add(new Chunk(venta.TipoComprobante.ToString() + "\n", fuenteX));
                p.Add(new Chunk(codFactura, fuenteFooter));
            }

            centro.AddElement(p);
            cabecera.AddCell(centro);

            // DERECHA
            PdfPCell der = new PdfPCell
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            if (venta.TipoComprobante == 'X')
            {
                der.AddElement(new Paragraph($"N° Comprobante: {venta.IdVenta}", fuenteNegrita));
                der.AddElement(new Paragraph($"Fecha: {venta.FechaVenta:dd/MM/yyyy}", fuenteNormal));
            }
            else
            {
                string descComprobante = QuitarUltimoCaracterSiCorresponde(factura != null ? factura.DescTipoCbteAfip : "");
                der.AddElement(new Paragraph("\n" + descComprobante.ToUpper() + "\n", fuenteNegrita));
                der.AddElement(new Paragraph("Nro.Comp.: " + (factura != null ? factura.PtoVtaAfip : "") + "-" + (factura != null ? factura.NroCbteAfip : "") + "\n", fuenteNormal));
                der.AddElement(new Paragraph("Fecha de Emisión: " + ((factura != null && factura.FechaEmisionAfip.HasValue) ? factura.FechaEmisionAfip.Value.Date.ToString("dd/MM/yyyy") : venta.FechaVenta.ToString("dd/MM/yyyy")) + "\n", fuenteNormal));
                der.AddElement(new Paragraph("IIBB: " + (empresaFactura != null ? empresaFactura.Iibb.ToString() : "") + "\n", fuenteNormal));
                der.AddElement(new Paragraph("CUIT: " + (empresaFactura != null ? empresaFactura.Cuit.ToString() : "") + "\n", fuenteNormal));
                der.AddElement(new Paragraph("Inicio Act.: " + ((empresaFactura != null && empresaFactura.InicioActividad != DateTime.MinValue) ? empresaFactura.InicioActividad.Date.ToString("dd/MM/yyyy") : "") + "\n", fuenteNormal));
            }

            cabecera.AddCell(der);

            return cabecera;
        }

        #endregion

        #region CLIENTE

        private PdfPTable GenerarCliente(Entidades.Venta venta, Entidades.FacturaElectronica factura, Font normal, Font negrita)
        {
            PdfPTable cliente = new PdfPTable(4) { WidthPercentage = 100 };
            cliente.SetWidths(new float[] { 15, 45, 15, 25 });

            cliente.AddCell(Celda("Cliente:", negrita));
            cliente.AddCell(Celda(factura != null ? factura.RazonSocialAFIP : venta.Persona.razonSocial, normal));//venta.Persona.razonSocial, normal));
            cliente.AddCell(Celda("CUIT:", negrita));
            cliente.AddCell(Celda(factura != null ? factura.NroDocAfip : venta.Persona.Cuit, normal)); //venta.Persona.Cuit, normal));

            cliente.AddCell(Celda("Domicilio:", negrita));
            cliente.AddCell(Celda(factura != null ? factura.DomicilioAFIP : venta.Persona.Domicilio, normal));
            cliente.AddCell(Celda("Cond. IVA:", negrita));
            cliente.AddCell(Celda(factura != null ? factura.CondicionIvaAFIP : venta.Persona.Iva, normal));

            cliente.AddCell(Celda("Forma pago:", negrita));
            cliente.AddCell(Celda(factura != null ? factura.FormaPago : venta.FormaPago, normal));
            cliente.AddCell(Celda(
                factura != null && !string.IsNullOrWhiteSpace(factura.ComprobanteAsociadoInfo) ? "Cbte Asoc:" : "",
                negrita,
                Element.ALIGN_RIGHT));
            cliente.AddCell(Celda(
                factura != null ? (factura.ComprobanteAsociadoInfo ?? "") : "",
                normal,
                Element.ALIGN_RIGHT));

            return cliente;
        }

        #endregion

        #region PRODUCTOS

        private PdfPTable GenerarProductos(Entidades.Venta venta, Entidades.FacturaElectronica factura, Font normal, Font header)
        {
            bool esFacturaA = venta.TipoComprobante == 'A';
            bool agruparItemUnitario = factura != null && !string.IsNullOrWhiteSpace(factura.DescItemUnitario);

            int columnas = esFacturaA ? 5 : 4;
            PdfPTable tabla = new PdfPTable(columnas) { WidthPercentage = 100 };

            tabla.SetWidths(esFacturaA
                ? new float[] { 6, 2, 2, 2, 2 }
                : new float[] { 6, 2, 2, 2 });

            // IMPORTANTE: permite cortar mejor la tabla entre páginas
            tabla.SplitLate = false;
            tabla.SplitRows = true;
            tabla.HeaderRows = 1;

            // HEADER
            tabla.AddCell(CeldaHeader("Descripción", header));
            tabla.AddCell(CeldaHeader("Cantidad", header));
            tabla.AddCell(CeldaHeader("Precio Un.", header));

            if (esFacturaA)
                tabla.AddCell(CeldaHeader("IVA", header));

            tabla.AddCell(CeldaHeader("Importe", header));

            // ITEMS
            if (agruparItemUnitario)
            {
                decimal totalAgrupado = esFacturaA
                    ? Convert.ToDecimal(factura.ImporteNetoGravado)
                    : Convert.ToDecimal(factura.ImporteTotal);

                tabla.AddCell(Celda(factura.DescItemUnitario, normal));
                tabla.AddCell(Celda("1,000", normal, Element.ALIGN_RIGHT));
                tabla.AddCell(Celda(totalAgrupado.ToString("#,##0.00", new CultureInfo("es-AR")), normal, Element.ALIGN_RIGHT));

                if (esFacturaA)
                    tabla.AddCell(Celda("", normal, Element.ALIGN_RIGHT));

                tabla.AddCell(Celda(
                    totalAgrupado.ToString("#,##0.00", new CultureInfo("es-AR")),
                    normal,
                    Element.ALIGN_RIGHT));
            }
            else
            {
                foreach (var l in venta.LineasVenta)
                {
                    tabla.AddCell(Celda($"[Cód. {l.Corte.Codigo}] {l.Corte.corte}", normal));
                    tabla.AddCell(Celda(l.CantKg.ToString("F3"), normal, Element.ALIGN_RIGHT));
                    tabla.AddCell(Celda(l.PrecioKg.ToString("#,##0.00", new CultureInfo("es-AR")), normal, Element.ALIGN_RIGHT));

                    if (esFacturaA)
                        tabla.AddCell(Celda(l.AlicuotaIva.ToString("#,##0.00"), normal, Element.ALIGN_RIGHT));

                    tabla.AddCell(Celda(
                        (l.CantKg * l.PrecioKg).ToString("#,##0.00", new CultureInfo("es-AR")),
                        normal,
                        Element.ALIGN_RIGHT));
                }
            }

            return tabla;
        }

        #endregion

        #region TOTALES

        private PdfPTable GenerarTotal(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteNormal,
            Font negrita)
        {
            PdfPTable total = new PdfPTable(3) { WidthPercentage = 100 };
            total.SetWidths(new float[] { 5, 1, 1 });

            string importeTexto = ConvertirMontoEnTexto(venta, factura);
            string observacionesComprobante = ObtenerObservacionesComprobante(venta, factura);

            string importe_obs = string.IsNullOrEmpty(observacionesComprobante) ? importeTexto :
                importeTexto + "\n-------\n" + observacionesComprobante;

            total.AddCell(Celda(importe_obs, fuenteNormal, Element.ALIGN_LEFT));
            total.AddCell(Celda("TOTAL:", negrita, Element.ALIGN_RIGHT));
            total.AddCell(Celda(
                (factura != null ? factura.ImporteTotal : venta.TotalImporte).ToString("#,##0.00", new CultureInfo("es-AR")),
                negrita,
                Element.ALIGN_RIGHT));

            return total;
        }

        private PdfPTable GenerarTotales(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteNormal,
            Font negrita,
            Font fuenteFooter)
        {
            PdfPTable total = new PdfPTable(3) { WidthPercentage = 100 };
            total.SetWidths(new float[] { 5, 1, 1 });

            bool esFacturaA = venta.TipoComprobante == 'A';

            string importeTexto = ConvertirMontoEnTexto(venta, factura);
            string observacionesComprobante = ObtenerObservacionesComprobante(venta, factura);
            total.AddCell(Celda(importeTexto, fuenteNormal, Element.ALIGN_LEFT));

            //total.AddCell(Celda(venta.Observaciones, fuenteNormal, Element.ALIGN_LEFT));

            if (esFacturaA)
            {
                total.AddCell(Celda("Neto s/iva: $", negrita, Element.ALIGN_RIGHT));
                total.AddCell(Celda(
                    factura.ImporteNetoGravado.ToString("#,##0.00", new CultureInfo("es-AR")),
                    negrita,
                    Element.ALIGN_RIGHT));

                foreach (Entidades.AlicuotaIva item in factura.ListaAlicuota)
                {
                    if (item.Importe > 0)
                    {
                        total.AddCell(Celda("", negrita, Element.ALIGN_RIGHT));
                        total.AddCell(Celda("Iva " + item.Iva + "%: $", negrita, Element.ALIGN_RIGHT));
                        total.AddCell(Celda(
                            item.Importe.ToString("#,##0.00", new CultureInfo("es-AR")),
                            negrita,
                            Element.ALIGN_RIGHT));
                    }
                }
            }
            else
            {
                total.AddCell(Celda("Subtotal: $", negrita, Element.ALIGN_RIGHT));
                total.AddCell(Celda(
                    factura.ImporteTotal.ToString("#,##0.00", new CultureInfo("es-AR")),
                    negrita,
                    Element.ALIGN_RIGHT));
            }

            string obs = string.IsNullOrEmpty(observacionesComprobante) ? "" :
                "Obs: " + observacionesComprobante;
            total.AddCell(Celda(obs, fuenteNormal, Element.ALIGN_LEFT));
            total.AddCell(Celda("Total: $", negrita, Element.ALIGN_RIGHT));
            total.AddCell(Celda(
                factura.ImporteTotal.ToString("#,##0.00", new CultureInfo("es-AR")),
                negrita,
                Element.ALIGN_RIGHT));

            return total;
        }

        private static string ObtenerObservacionesComprobante(Entidades.Venta venta, Entidades.FacturaElectronica factura)
        {
            if (factura != null)
                return factura.Observaciones ?? "";

            return venta != null ? (venta.Observaciones ?? "") : "";
        }

        #endregion

        #region INFO_CAE

        private PdfPTable InfoRegimenFiscal(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteNormal,
            Font fuenteNegrita,
            Font fuenteFooter)
        {
            PdfPTable infoRegimenFiscal = new PdfPTable(1);
            infoRegimenFiscal.WidthPercentage = 100;
            infoRegimenFiscal.SetWidths(new float[] { 1f });

            infoRegimenFiscal.AddCell(Celda("Régimen de Transparencia Fiscal Al Consumidor (Ley 27.743)", fuenteFooter, Element.ALIGN_RIGHT));
            infoRegimenFiscal.AddCell(Celda($"IVA Contenido: {factura.Iva:N2}", fuenteFooter, Element.ALIGN_RIGHT));
            infoRegimenFiscal.AddCell(Celda(" ", fuenteFooter, Element.ALIGN_RIGHT));

            return infoRegimenFiscal;
        }

        private PdfPTable InfoCAE(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteNormal,
            Font fuenteNegrita,
            Font fuenteFooter)
        {
            PdfPTable infoCAE = new PdfPTable(1);
            infoCAE.WidthPercentage = 100;
            infoCAE.SetWidths(new float[] { 1f });

            infoCAE.AddCell(Celda($"CAE: {factura.CAE1}", fuenteFooter, Element.ALIGN_RIGHT));
            infoCAE.AddCell(Celda($"Fecha de Vencimiento del CAE: {factura.FecVtoCAE}", fuenteFooter, Element.ALIGN_RIGHT));

            return infoCAE;
        }

        #endregion

        #region FOOTER ÚLTIMA PÁGINA

        private float CalcularMargenInferiorFactura(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteNormal,
            Font fuenteNegrita,
            Font fuenteFooter,
            float anchoDisponible)
        {
            var tablaTotales = GenerarTotales(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter);
            tablaTotales.TotalWidth = anchoDisponible;
            tablaTotales.LockedWidth = true;

            var tablaRegimen = InfoRegimenFiscal(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter);
            tablaRegimen.TotalWidth = anchoDisponible;
            tablaRegimen.LockedWidth = true;

            var tablaCAE = InfoCAE(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter);
            tablaCAE.TotalWidth = anchoDisponible - 115f;
            tablaCAE.LockedWidth = true;

            float qrBottom = 20f;
            float qrHeight = 100f;

            // Este valor baja SOLO el bloque superior (totales + línea + régimen)
            float ajusteTotales = -65f; // probá también con -10f o -20f

            // Parte inferior: QR + CAE (queda igual)
            float bloqueInferiorTop = qrBottom + Math.Max(qrHeight, tablaCAE.TotalHeight);

            // Parte superior: régimen + línea + totales (baja con el ajuste)
            float regimenTop = bloqueInferiorTop + 10f + tablaRegimen.TotalHeight + ajusteTotales;
            float lineaY = regimenTop + 6f;
            float totalesTop = lineaY + 8f + tablaTotales.TotalHeight;

            return totalesTop + 10f;
        }

        private byte[] AgregarPieFijoUltimaPagina(
            byte[] pdfOriginal,
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura,
            Font fuenteNormal,
            Font fuenteNegrita,
            Font fuenteFooter,
            float margenIzquierdo,
            float margenDerecho)
        {
            using (var output = new MemoryStream())
            using (var reader = new PdfReader(pdfOriginal))
            using (var stamper = new PdfStamper(reader, output))
            {
                int ultimaPagina = reader.NumberOfPages;
                PdfContentByte canvas = stamper.GetOverContent(ultimaPagina);
                Rectangle pageSize = reader.GetPageSize(ultimaPagina);

                float left = margenIzquierdo;
                float right = pageSize.Width - margenDerecho;
                float width = right - left;

                // ===== TOTALES =====
                var tablaTotales = GenerarTotales(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter);
                tablaTotales.TotalWidth = width;
                tablaTotales.LockedWidth = true;

                // ===== REGIMEN =====
                var tablaRegimen = InfoRegimenFiscal(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter);
                tablaRegimen.TotalWidth = width;
                tablaRegimen.LockedWidth = true;

                // ===== CAE =====
                float anchoCAE = width - 115f;
                var tablaCAE = InfoCAE(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter);
                tablaCAE.TotalWidth = anchoCAE;
                tablaCAE.LockedWidth = true;

                // ===== POSICIONES FIJAS =====
                float qrBottom = 20f;
                float qrHeight = 100f;
                float qrWidth = 100f;
                float qrLeft = left;

                float caeLeft = left + 115f;
                float caeTop = qrBottom + tablaCAE.TotalHeight + 8f;

                // Este valor baja SOLO el bloque superior
                float ajusteTotales = -65f; // probá también con -10f o -20f

                // QR y CAE quedan igual
                float bloqueInferiorTop = qrBottom + Math.Max(qrHeight, tablaCAE.TotalHeight);

                // Régimen, línea y totales bajan
                float regimenTop = bloqueInferiorTop + 10f + tablaRegimen.TotalHeight + ajusteTotales;
                float lineaY = regimenTop + 6f;
                float totalesTop = lineaY + 8f + tablaTotales.TotalHeight;

                // ===== DIBUJAR TOTALES =====
                tablaTotales.WriteSelectedRows(0, -1, left, totalesTop, canvas);

                // ===== LÍNEA =====
                canvas.SetLineWidth(1f);
                canvas.SetColorStroke(BaseColor.GRAY);
                canvas.MoveTo(left, lineaY);
                canvas.LineTo(right, lineaY);
                canvas.Stroke();

                // ===== DIBUJAR REGIMEN =====
                tablaRegimen.WriteSelectedRows(0, -1, left, regimenTop, canvas);

                // ===== QR =====
                byte[] qrBytes = GenerateQRCode(factura, venta);
                if (qrBytes != null && qrBytes.Length > 0)
                {
                    iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(qrBytes);
                    qrImage.ScaleAbsolute(qrWidth, qrHeight);
                    qrImage.SetAbsolutePosition(qrLeft, qrBottom);
                    canvas.AddImage(qrImage);
                }

                // ===== CAE =====
                tablaCAE.WriteSelectedRows(0, -1, caeLeft, caeTop, canvas);

                stamper.Close();
                reader.Close();

                return output.ToArray();
            }
        }

        #endregion

        #region HELPERS

        private PdfPCell Celda(string texto, Font fuente, int align = Element.ALIGN_LEFT)
        {
            return new PdfPCell(new Phrase(texto, fuente))
            {
                Border = 0,
                Padding = 4,
                HorizontalAlignment = align
            };
        }

        private PdfPCell CeldaHeader(string texto, Font fuente)
        {
            return new PdfPCell(new Phrase(texto, fuente))
            {
                BackgroundColor = new BaseColor(255, 200, 200),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER,
                Padding = 4
            };
        }

        private string ConvertirMontoEnTexto(
            Entidades.Venta venta,
            Entidades.FacturaElectronica factura)
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

        private string ConvertirCentena(int numero, string[] unidades, string[] decenas, string[] especiales, string[] centenas)
        {
            if (numero == 0) return "";

            string texto = "";

            if (numero > 99)
            {
                if (numero == 100)
                {
                    texto = "cien";
                    return texto;
                }
                else
                {
                    texto = centenas[numero / 100] + " ";
                    numero %= 100;
                }
            }

            if (numero > 19)
            {
                texto += decenas[numero / 10] + (numero % 10 > 0 ? " y " + unidades[numero % 10] : "");
            }
            else if (numero >= 10)
            {
                texto += especiales[numero - 10];
            }
            else if (numero > 0)
            {
                texto += unidades[numero];
            }

            return texto.Trim();
        }

        public byte[] GenerateQRCode(Entidades.FacturaElectronica factura, Entidades.Venta venta = null)
        {
            try
            {
                string urlBase = "https://www.afip.gob.ar/fe/qr/";
                string payload = GenerarJSON(factura, venta);
                if (string.IsNullOrWhiteSpace(payload))
                    return null;

                string data = urlBase + "?p=" + payload;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                using (MemoryStream stream = new MemoryStream())
                {
                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        private string GenerarJSON(Entidades.FacturaElectronica factura, Entidades.Venta venta = null)
        {
            if (factura == null)
                return "";

            string fechaEmision = factura.FechaEmisionAfip?.ToString("yyyy-MM-dd");
            var empresaFactura = ObtenerEmpresaFactura(venta, factura);

            long _cuitEmisor = ParseLongSeguro(empresaFactura != null ? empresaFactura.Cuit.ToString() : "");
            long _nroCmp = ParseLongSeguro(factura.NroCbteAfip);
            long _nroDocRec = ParseLongSeguro(factura.NroDocAfip);
            long _codAut = ParseLongSeguro(factura.CAE1);
            int _ptoVta = ParseIntSeguro(factura.PtoVtaAfip);

            if (_cuitEmisor <= 0 || _nroCmp <= 0 || _codAut <= 0 || _ptoVta <= 0)
                return "";

            int _tipoDocRec = factura.TipoDocAfip.Equals("CUIT") ? 80 :
                factura.TipoDocAfip.Equals("DNI") ? 86 : 99;

            decimal _importe = Convert.ToDecimal(factura.ImporteTotal);

            var qrData = new
            {
                ver = 1,
                fecha = fechaEmision,
                cuit = _cuitEmisor,
                ptoVta = _ptoVta,
                tipoCmp = factura.CodTipoCbteAfip,
                nroCmp = _nroCmp,
                importe = _importe,
                moneda = "PES",
                ctz = 1,
                tipoDocRec = _tipoDocRec,
                nroDocRec = _nroDocRec,
                tipoCodAut = "E",
                codAut = _codAut
            };

            string jsonData = JsonConvert.SerializeObject(qrData, Formatting.Indented);
            string jsonDataBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));

            return jsonDataBase64;
        }

        private Entidades.Empresa ObtenerEmpresaFactura(Entidades.Venta venta, Entidades.FacturaElectronica factura)
        {
            return (venta != null && venta.Sucursal != null ? venta.Sucursal.Empresa : null)
                ?? (factura != null && factura.Venta != null && factura.Venta.Sucursal != null ? factura.Venta.Sucursal.Empresa : null);
        }

        private string QuitarUltimoCaracterSiCorresponde(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "FACTURA";

            texto = texto.Trim();
            return texto.Length > 1 ? texto.Substring(0, texto.Length - 1) : texto;
        }

        private long ParseLongSeguro(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return 0;

            string soloDigitos = new string(texto.Where(char.IsDigit).ToArray());
            long valor;
            return long.TryParse(soloDigitos, out valor) ? valor : 0;
        }

        private int ParseIntSeguro(string texto)
        {
            int valor;
            return int.TryParse(texto, out valor) ? valor : 0;
        }

        #endregion

        //#region PUBLICO

        //public byte[] GenerarFacturaPDF(Entidades.Venta venta, Entidades.FacturaElectronica factura = null)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        Document doc = new Document(PageSize.A4, 30, 30, 20, 20);
        //        PdfWriter.GetInstance(doc, ms);
        //        doc.Open();

        //        // ===== FUENTES =====
        //        var colorRojo = new BaseColor(174, 0, 0);
        //        var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 30, colorRojo);
        //        var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        //        var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        //        var fuenteX = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35);
        //        var fuenteFooter = FontFactory.GetFont(FontFactory.HELVETICA, 7);
        //        var fuenteHeaderTabla = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

        //        var linea = new LineSeparator(1.5f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -6);

        //        // ===== CABECERA =====
        //        doc.Add(GenerarCabecera(venta, factura, fuenteTitulo, fuenteNormal, fuenteNegrita, fuenteX, fuenteFooter));
        //        doc.Add(new Chunk(linea));
        //        doc.Add(new Paragraph(" "));

        //        // ===== CLIENTE =====
        //        doc.Add(GenerarCliente(venta, factura, fuenteNormal, fuenteNegrita));
        //        doc.Add(new Chunk(linea));
        //        //doc.Add(new Paragraph(" "));

        //        // ===== PRODUCTOS =====
        //        doc.Add(GenerarProductos(venta, factura, fuenteNormal, fuenteHeaderTabla));

        //        // ===== TOTALES =====
        //        doc.Add(ImporteTexto(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter));

        //        doc.Add(new Chunk(linea));
        //        doc.Add(new Paragraph(" "));

        //        if (factura == null)
        //        {
        //            doc.Add(GenerarTotal(venta, factura, fuenteNormal, fuenteNegrita));
        //            doc.Add(new Chunk(linea));
        //        }
        //        else
        //        {
        //            doc.Add(GenerarTotales(venta, factura, fuenteNormal, fuenteNegrita));
        //            doc.Add(new Chunk(linea));

        //            // ===== INFO PIE PAGINA =====
        //            doc.Add(InfoRegimenFiscal(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter));

        //            // Crear imagen QR
        //            iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(GenerateQRCode(factura));
        //            // Escalar a 100x100 px
        //            qrImage.ScaleAbsolute(100, 100);
        //            // Posicionar en la esquina inferior izquierda dentro de márgenes
        //            float xPosition = doc.LeftMargin;          // borde izquierdo del área imprimible
        //            float yPosition = doc.BottomMargin;        // borde inferior del área imprimible
        //            qrImage.SetAbsolutePosition(xPosition, yPosition);
        //            // Agregar al documento
        //            doc.Add(qrImage);

        //            doc.Add(InfoCAE(venta, factura, fuenteNormal, fuenteNegrita, fuenteFooter));
        //        }

        //        doc.Close();
        //        return ms.ToArray();
        //    }
        //}

        //#endregion

        //#region CABECERA

        //private PdfPTable GenerarCabecera(
        //    Entidades.Venta venta,
        //    Entidades.FacturaElectronica factura,
        //    Font fuenteTitulo,
        //    Font fuenteNormal,
        //    Font fuenteNegrita,
        //    Font fuenteX,
        //    Font fuenteFooter)
        //{
        //    PdfPTable cabecera = new PdfPTable(3) { WidthPercentage = 100 };
        //    cabecera.SetWidths(new float[] { 33f, 34f, 33f });

        //    // IZQUIERDA
        //    PdfPCell izq = new PdfPCell { Border = 0 };
        //    izq.AddElement(new Paragraph(venta.Sucursal.Empresa.NombreFantasia+"\n", fuenteTitulo));
        //    if (venta.TipoComprobante != 'X')
        //    { 
        //        izq.AddElement(new Paragraph("\nRazón Social: " + factura.Venta.Sucursal.Empresa.RazonSocialAfip + "\n", fuenteNormal));
        //        izq.AddElement(new Paragraph(factura.Venta.Sucursal.Empresa.Domicilio + " - " +
        //                                factura.Venta.Sucursal.Empresa.Ciudad + "\n", fuenteNormal)); 
        //        izq.AddElement(new Paragraph("Cond. IVA: " + factura.Venta.Sucursal.Empresa.CondicionIVA + "\n", fuenteNormal));                
        //    }

        //    cabecera.AddCell(izq);

        //    // CENTRO
        //    PdfPCell centro = new PdfPCell { Border = 0 };
        //    Paragraph p = new Paragraph { Alignment = Element.ALIGN_CENTER };

        //    if (venta.TipoComprobante == 'X')
        //    {
        //        p.Add(new Chunk("X\n", fuenteX));
        //        p.Add(new Chunk("- Documento no válido como factura -", fuenteFooter));
        //    }
        //    else
        //    {
        //        String codFactura = "COD." + (factura.CodTipoCbteAfip < 10 ? 
        //            ("0" + factura.CodTipoCbteAfip.ToString()) : factura.CodTipoCbteAfip.ToString());

        //        p.Add(new Chunk(venta.TipoComprobante.ToString()+"\n", fuenteX));
        //        p.Add(new Chunk(codFactura, fuenteFooter));
        //    }

        //    centro.AddElement(p);
        //    cabecera.AddCell(centro);

        //    // DERECHA
        //    PdfPCell der = new PdfPCell
        //    {
        //        Border = 0,
        //        HorizontalAlignment = Element.ALIGN_RIGHT
        //    };

        //    if (venta.TipoComprobante == 'X')
        //    {
        //        der.AddElement(new Paragraph($"N° Comprobante: {venta.IdVenta}", fuenteNegrita));
        //        der.AddElement(new Paragraph($"Fecha: {venta.FechaVenta:dd/MM/yyyy}", fuenteNormal));
        //    }
        //    else
        //    {
        //        string descComprobante = factura.DescTipoCbteAfip.Substring(0, factura.DescTipoCbteAfip.Length - 1);
        //        der.AddElement(new Paragraph("\n" + descComprobante.ToUpper() + "\n", fuenteNegrita));
        //        der.AddElement(new Paragraph("Nro.Comp.: " + factura.PtoVtaAfip + "-" + factura.NroCbteAfip + "\n", fuenteNormal));
        //        der.AddElement(new Paragraph("Fecha de Emisión: " + factura.FechaEmisionAfip.Value.Date.ToString("dd/MM/yyyy") + "\n", fuenteNormal));
        //        der.AddElement(new Paragraph("IIBB: " + factura.Venta.Sucursal.Empresa.Iibb.ToString() + "\n", fuenteNormal));
        //        der.AddElement(new Paragraph("CUIT: " + factura.Venta.Sucursal.Empresa.Cuit.ToString() + "\n", fuenteNormal));
        //        der.AddElement(new Paragraph("Inicio Act.: " + factura.Venta.Sucursal.Empresa.InicioActividad.Date.ToString("dd/MM/yyyy") + "\n", fuenteNormal));

        //    }

        //    cabecera.AddCell(der);

        //    return cabecera;
        //}

        //#endregion

        //#region CLIENTE

        //private PdfPTable GenerarCliente(Entidades.Venta venta, Entidades.FacturaElectronica factura, Font normal, Font negrita)
        //{
        //    PdfPTable cliente = new PdfPTable(4) { WidthPercentage = 100 };
        //    cliente.SetWidths(new float[] { 15, 45, 15, 25 });

        //    cliente.AddCell(Celda("Cliente:", negrita));
        //    cliente.AddCell(Celda(venta.Persona.razonSocial, normal));
        //    cliente.AddCell(Celda("CUIT:", negrita));
        //    cliente.AddCell(Celda(venta.Persona.Cuit, normal));

        //    cliente.AddCell(Celda("Domicilio:", negrita));
        //    cliente.AddCell(Celda(venta.Persona.Domicilio, normal));
        //    cliente.AddCell(Celda("Cond. IVA:", negrita));
        //    cliente.AddCell(Celda(venta.Persona.Iva, normal));

        //    cliente.AddCell(Celda("Forma pago:", negrita));
        //    cliente.AddCell(Celda(venta.FormaPago, normal));
        //    cliente.AddCell(Celda("", negrita));
        //    cliente.AddCell(Celda("", normal));

        //    return cliente;
        //}

        //#endregion

        //#region PRODUCTOS

        //private PdfPTable GenerarProductos(Entidades.Venta venta, Entidades.FacturaElectronica factura, Font normal, Font header)
        //{
        //    bool esFacturaA = venta.TipoComprobante == 'A';

        //    int columnas = esFacturaA ? 5 : 4;
        //    PdfPTable tabla = new PdfPTable(columnas) { WidthPercentage = 100 };

        //    tabla.SetWidths(esFacturaA
        //        ? new float[] { 6, 2, 2, 2, 2 }
        //        : new float[] { 6, 2, 2, 2 });

        //    // HEADER
        //    tabla.AddCell(CeldaHeader("Descripción", header));
        //    tabla.AddCell(CeldaHeader("Cantidad", header));
        //    tabla.AddCell(CeldaHeader("Precio Un.", header));

        //    if (esFacturaA)
        //        tabla.AddCell(CeldaHeader("IVA", header));

        //    tabla.AddCell(CeldaHeader("Importe", header));

        //    // ITEMS
        //    foreach (var l in venta.LineasVenta)
        //    {
        //        tabla.AddCell(Celda(l.Corte.corte, normal));
        //        tabla.AddCell(Celda(l.CantKg.ToString("F3"), normal, Element.ALIGN_RIGHT));
        //        tabla.AddCell(Celda(l.PrecioKg.ToString("#,##0.00", new CultureInfo("es-AR")), normal, Element.ALIGN_RIGHT));

        //        if (esFacturaA)
        //            tabla.AddCell(Celda(l.AlicuotaIva.ToString("#,##0.00"), normal, Element.ALIGN_RIGHT));

        //        tabla.AddCell(Celda(
        //            (l.CantKg * l.PrecioKg).ToString("#,##0.00", new CultureInfo("es-AR")),
        //            normal,
        //            Element.ALIGN_RIGHT));
        //    }

        //    return tabla;
        //}

        //#endregion

        //#region TOTALES


        //private PdfPTable ImporteTexto(Entidades.Venta venta, Entidades.FacturaElectronica factura,
        //    Font fuenteNormal,
        //    Font fuenteNegrita,
        //    Font fuenteFooter)
        //{
        //    // Totales
        //    PdfPTable importeTextoTable = new PdfPTable(1);
        //    importeTextoTable.WidthPercentage = 100;
        //    importeTextoTable.SetWidths(new float[] { 1f });
        //    float importeFloat = factura != null && factura.Id > 0 ? factura.ImporteTotal : venta.TotalImporte;
        //    string importeTexto = ConvertirMontoEnTexto(Convert.ToDecimal(importeFloat));
        //    importeTextoTable.AddCell(Celda(importeTexto, fuenteFooter, Element.ALIGN_LEFT));
        //    return importeTextoTable;
        //}

        //private PdfPTable GenerarTotal(
        //    Entidades.Venta venta,
        //    Entidades.FacturaElectronica factura,
        //    Font fuenteNormal,
        //    Font negrita)
        //{
        //    PdfPTable total = new PdfPTable(3) { WidthPercentage = 100 };
        //    total.SetWidths(new float[] { 5, 1, 1 });

        //    bool esFacturaA = venta.TipoComprobante == 'A';

        //    total.AddCell(Celda(venta.Observaciones, fuenteNormal, Element.ALIGN_LEFT));

        //    //total.AddCell(Celda("", negrita));
        //    total.AddCell(Celda("TOTAL:", negrita, Element.ALIGN_RIGHT));
        //    total.AddCell(Celda(
        //        venta.TotalImporte.ToString("#,##0.00", new CultureInfo("es-AR")),
        //        negrita,
        //        Element.ALIGN_RIGHT));

        //    return total;
        //}

        //private PdfPTable GenerarTotales(
        //    Entidades.Venta venta,
        //    Entidades.FacturaElectronica factura,
        //    Font fuenteNormal,
        //    Font negrita)
        //{
        //    PdfPTable total = new PdfPTable(3) { WidthPercentage = 100 };
        //    total.SetWidths(new float[] { 5, 1, 1 });

        //    bool esFacturaA = venta.TipoComprobante == 'A';

        //    total.AddCell(Celda(venta.Observaciones, fuenteNormal, Element.ALIGN_LEFT));

        //    if (esFacturaA)
        //    {
        //        //total.AddCell(Celda("", negrita));
        //        total.AddCell(Celda("Neto s/iva: $", negrita, Element.ALIGN_RIGHT));
        //        total.AddCell(Celda(factura.ImporteNetoGravado.ToString("#,##0.00", new CultureInfo("es-AR")), negrita, Element.ALIGN_RIGHT));


        //        foreach (Entidades.AlicuotaIva item in factura.ListaAlicuota)
        //        {
        //            if (item.Importe > 0)
        //            {
        //                total.AddCell(Celda("", negrita, Element.ALIGN_RIGHT));
        //                total.AddCell(Celda("Iva " + item.Iva + "%: $", negrita, Element.ALIGN_RIGHT));
        //                total.AddCell(Celda(item.Importe.ToString("#,##0.00", new CultureInfo("es-AR")), negrita, Element.ALIGN_RIGHT));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //total.AddCell(Celda("", negrita, Element.ALIGN_RIGHT));
        //        total.AddCell(Celda("Subtotal: $", negrita, Element.ALIGN_RIGHT));
        //        total.AddCell(Celda(factura.ImporteTotal.ToString("#,##0.00", new CultureInfo("es-AR")), negrita, Element.ALIGN_RIGHT));
        //    }
        //    total.AddCell(Celda("", negrita, Element.ALIGN_RIGHT));
        //    total.AddCell(Celda("Total: $", negrita, Element.ALIGN_RIGHT));
        //    total.AddCell(Celda(factura.ImporteTotal.ToString("#,##0.00", new CultureInfo("es-AR")), negrita, Element.ALIGN_RIGHT));

        //    return total;
        //}

        //#endregion

        //#region INFO_CAE

        //private PdfPTable InfoRegimenFiscal(Entidades.Venta venta, Entidades.FacturaElectronica factura,
        //    Font fuenteNormal,
        //    Font fuenteNegrita,
        //    Font fuenteFooter)
        //{
        //    // Totales
        //    PdfPTable infoRegimenFiscal = new PdfPTable(1);
        //    infoRegimenFiscal.WidthPercentage = 100;
        //    infoRegimenFiscal.SetWidths(new float[] { 1f });
        //    infoRegimenFiscal.AddCell(Celda($"Régimen de Transparencia Fiscal Al Consumidor(Ley 27.743)", fuenteFooter, Element.ALIGN_RIGHT));
        //    infoRegimenFiscal.AddCell(Celda($"IVA Contenido: {factura.Iva:N2}", fuenteFooter, Element.ALIGN_RIGHT));
        //    infoRegimenFiscal.AddCell(Celda($"    ", fuenteFooter, Element.ALIGN_RIGHT));

        //    return infoRegimenFiscal;
        //}

        //private PdfPTable InfoCAE(Entidades.Venta venta, Entidades.FacturaElectronica factura,
        //    Font fuenteNormal,
        //    Font fuenteNegrita,
        //    Font fuenteFooter)
        //{
        //    // Totales
        //    PdfPTable infoCAE = new PdfPTable(1);
        //    infoCAE.WidthPercentage = 100;
        //    infoCAE.SetWidths(new float[] { 1f });
        //    infoCAE.AddCell(Celda($"CAE: {factura.CAE1}", fuenteFooter, Element.ALIGN_RIGHT));
        //    infoCAE.AddCell(Celda($"Fecha de Vencimiento del CAE: {factura.FecVtoCAE}", fuenteFooter, Element.ALIGN_RIGHT));

        //    return infoCAE;
        //}

        //#endregion
        //#region HELPERS

        //private PdfPCell Celda(string texto, Font fuente, int align = Element.ALIGN_LEFT)
        //{
        //    return new PdfPCell(new Phrase(texto, fuente))
        //    {
        //        Border = 0,
        //        Padding = 4,
        //        HorizontalAlignment = align
        //    };
        //}

        //private PdfPCell CeldaHeader(string texto, Font fuente)
        //{
        //    return new PdfPCell(new Phrase(texto, fuente))
        //    {
        //        BackgroundColor = new BaseColor(255, 200, 200),
        //        HorizontalAlignment = Element.ALIGN_CENTER,
        //        Border = Rectangle.NO_BORDER,
        //        Padding = 4
        //    };
        //}


        //private string ConvertirMontoEnTexto(decimal monto)
        //{
        //    if (monto == 0)
        //        return "Cero";

        //    string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
        //    string[] decenas = { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
        //    string[] especiales = { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
        //    string[] centenas = { "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

        //    int millones = (int)(monto / 1000000);
        //    monto %= 1000000;
        //    int miles = (int)(monto / 1000);
        //    monto %= 1000;
        //    int cientos = (int)monto;
        //    monto -= cientos;

        //    int centavos = (int)((monto - Math.Truncate(monto)) * 100);

        //    string resultado = "Son pesos ";

        //    if (millones > 0)
        //        resultado += (millones > 1 ? ConvertirCentena(millones, unidades, decenas, especiales, centenas) + " millones " : "un millón ");

        //    if (miles > 0)
        //        resultado += (miles > 1 ? ConvertirCentena(miles, unidades, decenas, especiales, centenas) + " mil " : "mil ");

        //    if (cientos > 0)
        //        resultado += ConvertirCentena(cientos, unidades, decenas, especiales, centenas);

        //    if (centavos > 0)
        //        resultado += " con " + ConvertirCentena(centavos, unidades, decenas, especiales, centenas) + " centavos";

        //    return resultado.Trim();
        //}


        //private string ConvertirCentena(int numero, string[] unidades, string[] decenas, string[] especiales, string[] centenas)
        //{
        //    if (numero == 0) return "";

        //    string texto = "";

        //    if (numero > 99)
        //    {
        //        if (numero == 100)
        //        {
        //            texto = "cien";
        //            return texto;
        //        }
        //        else
        //        {
        //            texto = centenas[numero / 100] + " ";
        //            numero %= 100;
        //        }
        //    }

        //    if (numero > 19)
        //    {
        //        texto += decenas[numero / 10] + (numero % 10 > 0 ? " y " + unidades[numero % 10] : "");
        //    }
        //    else if (numero >= 10)
        //    {
        //        texto += especiales[numero - 10];
        //    }
        //    else if (numero > 0)
        //    {
        //        texto += unidades[numero];
        //    }

        //    return texto.Trim();
        //}

        //public byte[] GenerateQRCode(Entidades.FacturaElectronica factura)
        //{
        //    //string data = GenerarJSON();
        //    string urlBase = "https://www.afip.gob.ar/fe/qr/";
        //    string data = urlBase + "?p=" + GenerarJSON(factura);

        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        //    QRCode qrCode = new QRCode(qrCodeData);
        //    Bitmap qrCodeImage = qrCode.GetGraphic(20);
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //        return stream.ToArray();
        //    }
        //}
        //private string GenerarJSON(Entidades.FacturaElectronica factura)
        //{
        //    //Entidades.FacturaElectronica factura = notaCredito ? oNotaCredito : oFactuElec;

        //    string fechaEmision = factura.FechaEmisionAfip?.ToString("yyyy-MM-dd");
        //    // Crear la estructura del JSON utilizando la variable
        //    long _cuitEmisor = long.Parse(factura.Venta.Sucursal.Empresa.Cuit.ToString());
        //    long _nroCmp = long.Parse(factura.NroCbteAfip);
        //    long _nroDocRec = string.IsNullOrEmpty(factura.NroDocAfip) ? 0 : long.Parse(factura.NroDocAfip);
        //    long _codAut = long.Parse(factura.CAE1);
        //    int _ptoVta = Convert.ToInt32(factura.PtoVtaAfip);

        //    //TODO: mejorar la obtenendo del idTipoDoc (CUIT, DNI, DOC)
        //    //80 CUIT
        //    //96 DNI
        //    //99 Doc. (otro)
        //    int _tipoDocRec = factura.TipoDocAfip.Equals("CUIT") ? 80 :
        //        factura.TipoDocAfip.Equals("DNI") ? 86 : 99; //= Convert.ToInt32(TipoDocCMB.SelectedValue.ToString());

        //    decimal _importe = Convert.ToDecimal(factura.ImporteTotal);
        //    var qrData = new
        //    {
        //        ver = 1,
        //        fecha = fechaEmision,
        //        cuit = _cuitEmisor,
        //        ptoVta = _ptoVta,
        //        tipoCmp = factura.CodTipoCbteAfip,
        //        nroCmp = _nroCmp,
        //        importe = _importe,
        //        moneda = "PES",
        //        ctz = 1,
        //        tipoDocRec = _tipoDocRec,
        //        nroDocRec = _nroDocRec,
        //        tipoCodAut = "E",
        //        codAut = _codAut
        //    };

        //    // Serializar el objeto a JSON
        //    string jsonData = JsonConvert.SerializeObject(qrData, Formatting.Indented);
        //    //Codificar el JSON en Base64
        //    string jsonDataBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));

        //    return jsonDataBase64;
        //}

        //#endregion
    }
}
