// Generadores de Excel (.xlsx real, ClosedXML) de WebCore. Hoy solo Ventas > Lineas (2026-10-13, ver docs/DECISIONS.md
// "Exportar Lineas de venta a PDF y Excel"); el "Excel" de Finanzas sigue siendo un CSV (ver docs/DECISIONS.md).
// Depende de ClosedXML (MIT) y de LineasVentaExportHelper (textos y totales compartidos con el PDF).
//
// Los anchos de columna son FIJOS a proposito: AdjustToContents usa SixLabors.Fonts, que necesita fuentes del
// sistema y no esta garantizado en el contenedor Linux.
using System;
using System.IO;
using ClosedXML.Excel;
using WebCore.Models;

namespace WebCore.Services
{
    public static class GenerarExcelCore
    {
        // Cantidad de columnas de la hoja: A (codigo), B (producto), C (cantidad/kg), D (precio), E (total linea).
        private const int ColumnasHoja = 5;

        // Formatos de celda (culturalmente neutros: Excel los muestra segun la region de quien abre el archivo).
        private const string FormatoKg = "#,##0.000";
        private const string FormatoImporte = "#,##0.00";
        private const string FormatoMoneda = "\"$\" #,##0.00";

        // Linea de datos de la venta (A:E combinadas, texto de 9 pt con ajuste): Excel no autoajusta la altura de las
        // celdas combinadas, asi que se estima cuantos renglones ocupa el texto y se fija la altura. Es una
        // aproximacion (el ancho real depende de las letras): si queda corta, el texto se recorta, no se pierde.
        private const int CaracteresPorRenglonDatosVenta = 130;
        private const double AlturaRenglonDatosVenta = 13;

        private static readonly XLColor ColorBandaVenta = XLColor.FromHtml("#F0F0F0");
        private static readonly XLColor ColorEncabezadoColumnas = XLColor.FromHtml("#E9ECEF");
        private static readonly XLColor ColorTextoSecundario = XLColor.FromHtml("#6C757D");
        private static readonly XLColor ColorTotalVenta = XLColor.FromHtml("#146C43");
        private static readonly XLColor ColorLineaSeparadora = XLColor.FromHtml("#DEE2E6");

        // Excel de Ventas > Lineas: una sola hoja con un bloque por venta (banda con fecha, Venta ID y total;
        // linea gris con los datos de la venta; titulos de columna; una fila por linea), igual que el PDF
        // (GenerarDocsCore.GenerarPdfLineasVenta). Cantidades e importes se escriben como NUMEROS con formato,
        // no como texto, para que se puedan sumar y ordenar. Devuelve los bytes del .xlsx; no tiene efectos secundarios.
        public static byte[] GenerarExcelLineasVenta(VentaLineasIndexVm model, string sucursalNombre)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add(LineasVentaExportHelper.Titulo);

            hoja.Column(1).Width = 16;
            hoja.Column(2).Width = 44;
            hoja.Column(3).Width = 16;
            hoja.Column(4).Width = 16;
            hoja.Column(5).Width = 18;

            int fila = 1;

            // Encabezado del reporte: titulo, periodo/sucursal y filtros activos (si hay).
            var celdaTitulo = TextoCombinado(hoja, fila++, LineasVentaExportHelper.Titulo);
            celdaTitulo.Style.Font.Bold = true;
            celdaTitulo.Style.Font.FontSize = 14;

            TextoCombinado(hoja, fila++, LineasVentaExportHelper.Periodo(model, sucursalNombre))
                .Style.Font.FontColor = ColorTextoSecundario;

            string filtros = LineasVentaExportHelper.Filtros(model);
            if (filtros.Length > 0)
                TextoCombinado(hoja, fila++, filtros).Style.Font.FontColor = ColorTextoSecundario;

            fila++;

            if (model.Ventas.Count == 0)
            {
                var sinVentas = TextoCombinado(hoja, fila, LineasVentaExportHelper.MensajeSinVentas);
                sinVentas.Style.Font.Italic = true;
                sinVentas.Style.Font.FontColor = ColorTextoSecundario;
            }

            // Un bloque por venta (el "corte" de la pantalla).
            foreach (var venta in model.Ventas)
            {
                // Banda de la venta: fecha y Venta ID combinados en A:D; el total (de toda la venta) en E.
                var bandaVenta = hoja.Range(fila, 1, fila, ColumnasHoja);
                bandaVenta.Style.Fill.BackgroundColor = ColorBandaVenta;
                bandaVenta.Style.Font.Bold = true;

                hoja.Range(fila, 1, fila, ColumnasHoja - 1).Merge();
                hoja.Cell(fila, 1).Value = (venta.ResumenCompacto ?? "") + "    " + (venta.ResumenSecundario ?? "");

                var celdaTotalVenta = hoja.Cell(fila, ColumnasHoja);
                celdaTotalVenta.Value = venta.TotalImporte;
                celdaTotalVenta.Style.NumberFormat.Format = FormatoMoneda;
                celdaTotalVenta.Style.Font.FontColor = ColorTotalVenta;
                celdaTotalVenta.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                fila++;

                // Datos de la venta (los que la pantalla oculta tras "Ver detalles").
                string datosVenta = LineasVentaExportHelper.DatosVenta(venta);
                var celdaDatos = TextoCombinado(hoja, fila, datosVenta);
                celdaDatos.Style.Font.FontSize = 9;
                celdaDatos.Style.Font.FontColor = ColorTextoSecundario;
                celdaDatos.Style.Alignment.WrapText = true;
                int renglonesDatosVenta = (int)Math.Ceiling(datosVenta.Length / (double)CaracteresPorRenglonDatosVenta);
                if (renglonesDatosVenta > 1)
                    hoja.Row(fila).Height = renglonesDatosVenta * AlturaRenglonDatosVenta;
                fila++;

                // Titulos de columna (los 3 ultimos, numericos, alineados a la derecha).
                string[] titulos = LineasVentaExportHelper.EncabezadosColumnas;
                for (int i = 0; i < titulos.Length; i++)
                {
                    var celdaTitulos = hoja.Cell(fila, i + 1);
                    celdaTitulos.Value = titulos[i];
                    celdaTitulos.Style.Font.Bold = true;
                    celdaTitulos.Style.Fill.BackgroundColor = ColorEncabezadoColumnas;
                    if (i >= 2)
                        celdaTitulos.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                fila++;

                // Una fila por linea de la venta.
                foreach (var linea in venta.Lineas)
                {
                    // El codigo se guarda como numero cuando lo es (se puede ordenar/filtrar); "-" queda como texto.
                    var celdaCodigo = hoja.Cell(fila, 1);
                    if (int.TryParse(linea.Codigo, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int codigo))
                        celdaCodigo.Value = codigo;
                    else
                        celdaCodigo.Value = linea.Codigo ?? "";
                    celdaCodigo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    hoja.Cell(fila, 2).Value = linea.Producto ?? "";

                    EscribirNumero(hoja.Cell(fila, 3), linea.CantidadKg, FormatoKg);
                    EscribirNumero(hoja.Cell(fila, 4), linea.Precio, FormatoImporte);
                    EscribirNumero(hoja.Cell(fila, 5), linea.Total, FormatoImporte);

                    var bordeFila = hoja.Range(fila, 1, fila, ColumnasHoja).Style.Border;
                    bordeFila.BottomBorder = XLBorderStyleValues.Hair;
                    bordeFila.BottomBorderColor = ColorLineaSeparadora;
                    fila++;
                }

                // Renglon en blanco entre ventas.
                fila++;
            }

            // Totales generales (mismos que las tarjetas de la pantalla).
            if (model.Ventas.Count > 0)
            {
                var totales = LineasVentaExportHelper.Totales(model);

                var filaTotales = hoja.Range(fila, 1, fila, ColumnasHoja);
                filaTotales.Style.Font.Bold = true;
                filaTotales.Style.Fill.BackgroundColor = ColorEncabezadoColumnas;

                hoja.Range(fila, 1, fila, 2).Merge();
                hoja.Cell(fila, 1).Value = "TOTAL GENERAL";
                EscribirNumero(hoja.Cell(fila, 3), totales.TotalKg, FormatoKg);
                EscribirNumero(hoja.Cell(fila, 5), totales.TotalPesos, FormatoMoneda);
                fila++;

                TextoCombinado(hoja, fila, LineasVentaExportHelper.ResumenCantidades(totales.CantidadLineas, totales.CantidadRegistros))
                    .Style.Font.FontColor = ColorTextoSecundario;
            }

            using var salida = new MemoryStream();
            libro.SaveAs(salida);
            return salida.ToArray();
        }

        // Escribe un texto en toda la fila (A:E combinadas) y devuelve el rango para darle estilo.
        private static IXLRange TextoCombinado(IXLWorksheet hoja, int fila, string texto)
        {
            var rango = hoja.Range(fila, 1, fila, ColumnasHoja);
            rango.Merge();
            hoja.Cell(fila, 1).Value = texto;
            return rango;
        }

        // Escribe un numero real (no texto) con su formato, alineado a la derecha.
        private static void EscribirNumero(IXLCell celda, decimal valor, string formato)
        {
            celda.Value = valor;
            celda.Style.NumberFormat.Format = formato;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }
    }
}
