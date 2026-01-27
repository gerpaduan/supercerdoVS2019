using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data;
using iTextSharp.text.pdf.draw;
using System.Configuration;
using System.Diagnostics;

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
        #region PUBLICO

        public byte[] GenerarFacturaX(Entidades.Venta venta)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 30, 30, 20, 20);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ===== FUENTES =====
                var colorRojo = new BaseColor(174, 0, 0);
                var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 25, colorRojo);
                var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fuenteX = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35);
                var fuenteFooter = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                var fuenteHeaderTabla = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

                var linea = new LineSeparator(1.5f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -6);

                // ===== CABECERA =====
                doc.Add(GenerarCabecera(venta, fuenteTitulo, fuenteNormal, fuenteNegrita, fuenteX, fuenteFooter));
                doc.Add(new Chunk(linea));
                doc.Add(new Paragraph(" "));

                // ===== CLIENTE =====
                doc.Add(GenerarCliente(venta, fuenteNormal, fuenteNegrita));
                doc.Add(new Chunk(linea));
                doc.Add(new Paragraph(" "));

                // ===== PRODUCTOS =====
                doc.Add(GenerarProductos(venta, fuenteNormal, fuenteHeaderTabla));

                // ===== TOTALES =====
                doc.Add(new Chunk(linea));
                doc.Add(new Paragraph(" "));
                doc.Add(GenerarTotales(venta, fuenteNegrita));
                doc.Add(new Chunk(linea));

                // ===== OBS =====
                if (!string.IsNullOrEmpty(venta.Observaciones))
                {
                    doc.Add(new Paragraph("\nObs: " + venta.Observaciones,
                        FontFactory.GetFont(FontFactory.HELVETICA, 8)));
                }

                doc.Close();
                return ms.ToArray();
            }
        }

        #endregion

        #region CABECERA

        private PdfPTable GenerarCabecera(
            Entidades.Venta venta,
            Font fuenteTitulo,
            Font fuenteNormal,
            Font fuenteNegrita,
            Font fuenteX,
            Font fuenteFooter)
        {
            PdfPTable cabecera = new PdfPTable(3) { WidthPercentage = 100 };
            cabecera.SetWidths(new float[] { 33f, 34f, 33f });

            // IZQUIERDA
            PdfPCell izq = new PdfPCell { Border = 0 };
            izq.AddElement(new Paragraph("NOMBRE DEL NEGOCIO\n", fuenteTitulo));
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
                p.Add(new Chunk(venta.TipoComprobante.ToString(), fuenteX));
            }

            centro.AddElement(p);
            cabecera.AddCell(centro);

            // DERECHA
            PdfPCell der = new PdfPCell
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            der.AddElement(new Paragraph($"N° Comprobante: {venta.IdVenta}", fuenteNegrita));
            der.AddElement(new Paragraph($"Fecha: {venta.FechaVenta:dd/MM/yyyy}", fuenteNormal));
            cabecera.AddCell(der);

            return cabecera;
        }

        #endregion

        #region CLIENTE

        private PdfPTable GenerarCliente(Entidades.Venta venta, Font normal, Font negrita)
        {
            PdfPTable cliente = new PdfPTable(4) { WidthPercentage = 100 };
            cliente.SetWidths(new float[] { 15, 45, 15, 25 });

            cliente.AddCell(Celda("Sr.(es):", negrita));
            cliente.AddCell(Celda(venta.Persona.razonSocial, normal));
            cliente.AddCell(Celda("CUIT:", negrita));
            cliente.AddCell(Celda(venta.Persona.Cuit, normal));

            cliente.AddCell(Celda("Cond. IVA:", negrita));
            cliente.AddCell(Celda(venta.Persona.Iva, normal));
            cliente.AddCell(Celda("Forma pago:", negrita));
            cliente.AddCell(Celda(venta.FormaPago, normal));

            return cliente;
        }

        #endregion

        #region PRODUCTOS

        private PdfPTable GenerarProductos(Entidades.Venta venta, Font normal, Font header)
        {
            bool esFacturaA = venta.TipoComprobante == 'A';

            int columnas = esFacturaA ? 5 : 4;
            PdfPTable tabla = new PdfPTable(columnas) { WidthPercentage = 100 };

            tabla.SetWidths(esFacturaA
                ? new float[] { 6, 2, 2, 2, 2 }
                : new float[] { 6, 2, 2, 2 });

            // HEADER
            tabla.AddCell(CeldaHeader("Descripción", header));
            tabla.AddCell(CeldaHeader("Cantidad", header));
            tabla.AddCell(CeldaHeader("Precio Un.", header));

            if (esFacturaA)
                tabla.AddCell(CeldaHeader("IVA", header));

            tabla.AddCell(CeldaHeader("Importe", header));

            // ITEMS
            foreach (var l in venta.LineasVenta)
            {
                tabla.AddCell(Celda(l.Corte.corte, normal));
                tabla.AddCell(Celda(l.CantKg.ToString("F3"), normal, Element.ALIGN_RIGHT));
                tabla.AddCell(Celda(l.PrecioKg.ToString("#,##0.00", new CultureInfo("es-AR")), normal, Element.ALIGN_RIGHT));

                if (esFacturaA)
                    tabla.AddCell(Celda(l.AlicuotaIva.ToString("#,##0.00"), normal, Element.ALIGN_RIGHT));

                tabla.AddCell(Celda(
                    (l.CantKg * l.PrecioKg).ToString("#,##0.00", new CultureInfo("es-AR")),
                    normal,
                    Element.ALIGN_RIGHT));
            }

            return tabla;
        }

        #endregion

        #region TOTALES

        private PdfPTable GenerarTotales(Entidades.Venta venta, Font negrita)
        {
            PdfPTable total = new PdfPTable(3) { WidthPercentage = 100 };
            total.SetWidths(new float[] { 5, 1, 1 });

            total.AddCell(Celda("", negrita));
            total.AddCell(Celda("TOTAL:", negrita, Element.ALIGN_RIGHT));
            total.AddCell(Celda(
                venta.TotalImporte.ToString("#,##0.00", new CultureInfo("es-AR")),
                negrita,
                Element.ALIGN_RIGHT));

            return total;
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
                Padding = 4
            };
        }

        #endregion
    }
}
