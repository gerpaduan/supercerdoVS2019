using System;
using System.Data;
using System.Web.Mvc;
using Negocio;
using Entidades;
using Usuario = Negocio.Usuario;
using Web.Helpers;
using System.Globalization;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Web.Controllers
{
    public class FinanzasController : Controller
    {
        private CuentaCorriente oCtaCteN = new CuentaCorriente();
        private Usuario oUsuarioN = new Usuario();
        private readonly Negocio.Persona oPersonasN = new Negocio.Persona();

        // ***********************************************************
        //  Estos valores se pasan como QueryString o TempData
        // ***********************************************************
        public bool DesdePOS
        {
            get { return (bool)(TempData["DesdePOS"] ?? false); }
            set { TempData["DesdePOS"] = value; }
        }

        public Entidades.CierreCaja OCierreCajaE
        {
            get { return TempData["OCierreCajaE"] as Entidades.CierreCaja; }
            set { TempData["OCierreCajaE"] = value; }
        }


        // ============================================================
        // GET: /Finanzas/CtasCtes
        // ============================================================
        public ActionResult CtasCtes(string buscar = "", string ordenSaldo = "DESC")
        {
            try
            {
                // ==============================
                // Validación de permisos IGUAL que WinForms
                // ==============================
                if (!DesdePOS)
                {
                    var user = Session["Usuario"] as Entidades.Usuario;
                    if (!PermisosHelper.TienePermiso(Session, Permisos.Finanza.VerCtasCtes, null))
                    {
                        ViewBag.Seccion = "Agregar/Modificar Productos";
                        return View("~/Views/Shared/AccesoDenegado.cshtml");
                    }
                }

                ViewBag.Buscar = buscar;
                ViewBag.DesdePOS = DesdePOS;
                ViewBag.OrdenSaldo = ordenSaldo;

                // ==============================
                // Obtener DataTable como WinForms
                // ==============================
                DataTable dt = oCtaCteN.obtenerCtasCtes(buscar, null);


                // Ordenar por saldo
                DataView dv = dt.DefaultView;
                dv.Sort = $"Saldo {ordenSaldo}";
                dt = dv.ToTable();

                return View("CtasCtes", dt); // usa la vista que generamos antes
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }


        // GET: Finanzas/CtaCtePersona
        public ActionResult CtaCtePersona(int idPersona, DateTime? fechaDesde)
        {
            try
            {
                // Fecha por defecto = hoy
                if (!fechaDesde.HasValue)
                    fechaDesde = DateTime.Now.Date;

                // Obtengo movimientos desde la base
                DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesde.Value);

                decimal saldo = 0;
                if (dtMov != null && dtMov.Rows.Count > 0)
                {
                    DataRow ultimaFila = dtMov.Rows[dtMov.Rows.Count - 1];

                    if (ultimaFila["Saldo"] != DBNull.Value)
                        saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
                }

                // Datos para la vista
                ViewBag.IdPersona = idPersona;
                ViewBag.Persona = oPersonasN.findById(idPersona);  // opcional
                ViewBag.SaldoPersona = saldo;
                ViewBag.FechaDesde = fechaDesde.Value.ToString("yyyy-MM-dd"); ;

                return View(dtMov);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar cuenta corriente: " + ex.Message;
                return View(new DataTable());
            }
}

        // POST: Finanzas/CtaCtePersona
        [HttpPost]
        public ActionResult CtaCtePersonaPost(int idPersona, DateTime fechaDesde)
        {
            // Redirecciono al GET con los parámetros
            return RedirectToAction("CtaCtePersona", new { idPersona = idPersona, fechaDesde = fechaDesde });
        }

        // ============================================================
        //  EXPORTAR A EXCEL
        // ============================================================
        public ActionResult ExportarExcelPersona(int idPersona, string fechaDesde)
        {
            DateTime fecha = DateTime.ParseExact(fechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            DataTable dt = oCtaCteN.getCtaCteByIdPersona(idPersona, fecha);

            // Calcular saldos
            decimal saldo = 0;
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow ultimaFila = dt.Rows[dt.Rows.Count - 1];

                if (ultimaFila["Saldo"] != DBNull.Value)
                    saldo = Convert.ToDecimal(ultimaFila["Saldo"]);
            }

            // EXPORTACIÓN A EXCEL SIN LIBRERÍAS (CSV)
            string csv = "Fecha;Tabla;Detalle;Importe;Saldo;Sucursal\n";

            foreach (DataRow r in dt.Rows)
            {
                csv += $"{Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy")};" +
                       $"{r["Tabla"]};" +
                       $"{r["Detalle"]};" +
                       $"{Convert.ToDecimal(r["Importe"]).ToString("N2")};" +
                       $"{Convert.ToDecimal(r["Saldo"]).ToString("N2")};" +
                       $"{r["Sucursal"]}\n";
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(csv);

            return File(buffer, "text/csv", "CuentaCorriente.csv");
        }


        // ============================================================
        //  EXPORTAR A PDF
        // ============================================================
        public ActionResult ExportarPdfPersona(int idPersona, string fechaDesde)
        {
            DateTime fechaD;
            if (!DateTime.TryParse(fechaDesde, out fechaD))
                fechaD = DateTime.Today.AddMonths(-1);

            DataTable dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaD);

            // Obtener persona y saldo igual que en la vista
            string persona = "";

            if (dtMov.Rows.Count > 0)
            {
                persona = dtMov.Rows[0]["razonSocial"].ToString();
            }

            byte[] pdfBytes = Utilidades.GenerarDocs.GenerarPdfCtaCtePersona(dtMov, fechaD); // GenerarPdfPersona(dtMov, persona, saldo, fechaD);

            return File(pdfBytes, "application/pdf", $"{persona}_CuentaCorriente.pdf");
        }



        private byte[] GenerarPdfPersona(DataTable dt, string persona, decimal saldo, DateTime fechaDesde)
        {
            using (MemoryStream ms = new MemoryStream())
            {
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

                BaseColor colorSaldo = saldo >= 0 ? new BaseColor(0, 120, 0) : new BaseColor(180, 0, 0);
                PdfPCell saldoCell = new PdfPCell(new Phrase("Saldo: " + saldo.ToString("N2"),
                                         new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, colorSaldo)));
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

                string[] cols = { "Fecha", "Tabla", "Detalle", "Importe", "Saldo", "Sucursal" };

                // -------- ENCABEZADOS --------
                foreach (var c in cols)
                {
                    PdfPCell h = new PdfPCell(new Phrase(c, fontHeader));
                    h.BackgroundColor = new BaseColor(240, 240, 240); // gris clarito
                    h.HorizontalAlignment = Element.ALIGN_CENTER;
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
                    PdfPCell celDet = new PdfPCell(new Phrase(row["nroDoc"].ToString() +" "+ row["detalle"].ToString(), fontNormal));
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

        // ============================================================
        //  DETALLE DE MOVIMIENTO (placeholder)
        // ============================================================
        public ActionResult MovimientoDetalle(int id)
        {
            // Acá mostrás detalle del movimiento si querés
            ViewBag.IdMovimiento = id;
            return View();
        }

        // ============================================================
        //  AGREGAR COBRO / PAGO (placeholder)
        // ============================================================
        public ActionResult AgregarPagoCobro(int idPersona)
        {
            ViewBag.IdPersona = idPersona;
            return View();
        }
    }
}
