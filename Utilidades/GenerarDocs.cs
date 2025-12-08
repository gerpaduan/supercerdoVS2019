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
    }
}
