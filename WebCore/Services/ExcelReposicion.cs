// Excel (.xlsx real, ClosedXML) de "Qué pedir": una fila por producto y sucursal, ordenada por proveedor, con la cantidad
// sugerida y el contexto para decidir (stock, punto, consumo, días que alcanza, otras sucursales, último precio).
// Lo arma ReposicionController.ExportarExcel con los mismos filtros que la pantalla. Detalle y reglas:
// docs/11-manual-de-uso/pantallas/Reposicion.Index.md. Depende de ClosedXML (MIT) y de Entidades.ReposicionVm.
//
// Anchos de columna FIJOS a propósito (mismo criterio que ExcelReporteStock): AdjustToContents necesita fuentes del
// sistema y no está garantizado en el contenedor Linux.
using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using Entidades;

namespace WebCore.Services
{
    public static class ExcelReposicion
    {
        private const string FormatoCantidad = "#,##0.0##";
        private const string FormatoImporte = "$ #,##0.00";

        private static readonly XLColor ColorEncabezado = XLColor.FromHtml("#E9ECEF");
        private static readonly XLColor ColorSecundario = XLColor.FromHtml("#6C757D");

        // Una columna: título, ancho, valor de la fila y formato numérico (null = texto).
        private sealed class Columna
        {
            public string Titulo = "";
            public double Ancho;
            public Func<ReposicionFilaVm, object> Valor = _ => "";
            public string Formato;
        }

        // Texto del motivo, igual al de la pantalla.
        public static string TextoMotivo(ReposicionFilaVm f)
        {
            switch (f.Motivo)
            {
                case MotivoReposicion.BajoPuntoStock: return "Bajo el punto de stock";
                case MotivoReposicion.EnCeroConMovimiento: return "En cero (con movimiento)";
                default: return "En cero (sin movimiento)";
            }
        }

        // Avisos para revisar a mano antes de pedir.
        public static string TextoObservaciones(ReposicionFilaVm f)
        {
            var partes = new List<string>();
            if (f.StockNegativo) partes.Add("Stock negativo: revisar ingresos sin cargar");
            if (f.PuntoPareceAlto) partes.Add("El punto de stock parece alto para lo que se mueve");
            if (f.Motivo == MotivoReposicion.EnCeroConMovimiento && !f.Pedir.HasValue) partes.Add("Sin punto de stock cargado");
            if (f.Pedir.HasValue && f.StockOtrasSucursales >= f.Pedir.Value) partes.Add("Se puede cubrir con un movimiento entre sucursales");
            return string.Join(" · ", partes);
        }

        // Genera el .xlsx del resultado ya calculado. Sin efectos secundarios; devuelve los bytes del archivo.
        public static byte[] Generar(ReposicionVm model, string sucursalesTexto)
        {
            var columnas = ConstruirColumnas();

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Qué pedir");

                int fila = 1;
                hoja.Cell(fila, 1).Value = "Qué pedir";
                hoja.Cell(fila, 1).Style.Font.Bold = true;
                hoja.Cell(fila, 1).Style.Font.FontSize = 14;
                fila++;

                foreach (var linea in LineasEncabezado(model, sucursalesTexto))
                {
                    hoja.Cell(fila, 1).Value = linea;
                    hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
                    fila++;
                }
                fila++;

                // Encabezados con filtro automático.
                int filaEncabezado = fila;
                for (int c = 0; c < columnas.Count; c++)
                {
                    var celda = hoja.Cell(fila, c + 1);
                    celda.Value = columnas[c].Titulo;
                    celda.Style.Font.Bold = true;
                    celda.Style.Fill.BackgroundColor = ColorEncabezado;
                    celda.Style.Alignment.WrapText = true;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    if (columnas[c].Formato != null)
                        celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    hoja.Column(c + 1).Width = columnas[c].Ancho;
                }
                fila++;

                foreach (var item in model.Filas)
                {
                    for (int c = 0; c < columnas.Count; c++)
                    {
                        var celda = hoja.Cell(fila, c + 1);
                        object valor = columnas[c].Valor(item);
                        if (valor is decimal) celda.Value = (decimal)valor;
                        else if (valor is double) celda.Value = (double)valor;
                        else if (valor is long) celda.Value = (long)valor;
                        else if (valor is DateTime) celda.Value = (DateTime)valor;
                        else celda.Value = valor == null ? "" : valor.ToString();
                        if (columnas[c].Formato != null)
                            celda.Style.NumberFormat.Format = columnas[c].Formato;
                    }

                    // La cantidad a pedir es lo que importa: en negrita.
                    int idxPedir = columnas.FindIndex(c => c.Titulo == "Pedir");
                    if (idxPedir >= 0) hoja.Cell(fila, idxPedir + 1).Style.Font.Bold = true;
                    int idxProveedor = columnas.FindIndex(c => c.Titulo == "Proveedor");
                    if (idxProveedor >= 0 && string.IsNullOrEmpty(item.Proveedor))
                        hoja.Cell(fila, idxProveedor + 1).Style.Font.FontColor = ColorSecundario;
                    fila++;
                }

                if (model.Filas.Count == 0)
                {
                    hoja.Cell(fila, 1).Value = "No hay nada para pedir con los filtros elegidos.";
                    hoja.Cell(fila, 1).Style.Font.Italic = true;
                    hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
                }
                else
                {
                    hoja.Range(filaEncabezado, 1, fila - 1, columnas.Count).SetAutoFilter();
                    hoja.SheetView.FreezeRows(filaEncabezado);
                    hoja.SheetView.FreezeColumns(3);

                    // Total estimado debajo de la última fila, en la columna del importe.
                    int idxImporte = columnas.FindIndex(c => c.Titulo == "Importe estimado");
                    if (idxImporte >= 0)
                    {
                        hoja.Cell(fila, idxImporte).Value = "Total estimado";
                        hoja.Cell(fila, idxImporte).Style.Font.Bold = true;
                        hoja.Cell(fila, idxImporte).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        hoja.Cell(fila, idxImporte + 1).Value = model.ImporteEstimadoTotal;
                        hoja.Cell(fila, idxImporte + 1).Style.NumberFormat.Format = FormatoImporte;
                        hoja.Cell(fila, idxImporte + 1).Style.Font.Bold = true;
                    }
                }

                // Apaisada y ajustada al ancho de la página.
                hoja.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                hoja.PageSetup.FitToPages(1, 0);
                hoja.PageSetup.SetRowsToRepeatAtTop(filaEncabezado, filaEncabezado);

                using (var memoria = new System.IO.MemoryStream())
                {
                    libro.SaveAs(memoria);
                    return memoria.ToArray();
                }
            }
        }

        private static IEnumerable<string> LineasEncabezado(ReposicionVm model, string sucursalesTexto)
        {
            var f = model.Filtro;
            yield return "Calculado el " + model.FechaCalculo.ToString("dd/MM/yyyy HH:mm");
            yield return "Sucursales: " + (string.IsNullOrWhiteSpace(sucursalesTexto) ? "Todas" : sucursalesTexto);
            string coberturaTexto = f.DiasCoberturaSinPunto > 0
                ? "; sin punto de stock se sugiere para " + f.DiasCoberturaSinPunto + " días de venta"
                : "; sin punto de stock solo se avisa (sin cantidad)";
            yield return "Se considera movimiento: salidas (ventas o elaboración) en los últimos " + f.DiasVentana + " días" + coberturaTexto;
            yield return "Bajo el punto: " + model.CantBajoPuntoStock + " · En cero con movimiento: " + model.CantEnCeroConMovimiento
                + " · En cero sin movimiento (no se avisan): " + model.CantEnCeroSinMovimiento;
            foreach (var aviso in model.AvisosCierre.Where(a => a.Viejo))
            {
                yield return "Atención: " + aviso.Sucursal + " tiene el último cierre de stock "
                    + (aviso.FechaUltimoCierre.HasValue
                        ? "del " + aviso.FechaUltimoCierre.Value.ToString("dd/MM/yyyy") + " (hace " + aviso.DiasDesdeCierre + " días)"
                        : "sin registrar")
                    + ": el stock puede estar desactualizado.";
            }
            yield return "Importe estimado con el último precio de compra de cada producto; no incluye los que no tienen precio.";
        }

        private static List<Columna> ConstruirColumnas()
        {
            return new List<Columna>
            {
                new Columna { Titulo = "Proveedor", Ancho = 26, Valor = f => string.IsNullOrEmpty(f.Proveedor) ? "Sin proveedor" : f.Proveedor },
                new Columna { Titulo = "Código", Ancho = 14, Valor = f => f.Codigo },
                new Columna { Titulo = "Producto", Ancho = 32, Valor = f => f.Producto },
                new Columna { Titulo = "Sucursal", Ancho = 15, Valor = f => f.Sucursal },
                new Columna { Titulo = "Stock", Ancho = 10, Formato = FormatoCantidad, Valor = f => (decimal)f.StockActual },
                new Columna { Titulo = "Punto de stock", Ancho = 10, Formato = FormatoCantidad, Valor = f => (decimal)f.PuntoStock },
                new Columna { Titulo = "Pedir", Ancho = 10, Formato = FormatoCantidad, Valor = f => f.Pedir.HasValue ? (object)(decimal)f.Pedir.Value : "" },
                new Columna { Titulo = "Unidad", Ancho = 7, Valor = f => f.Pesable ? "kg" : "un." },
                new Columna { Titulo = "Equivale a (un.)", Ancho = 11, Formato = FormatoCantidad, Valor = f => UnidadesEquivalentes(f) },
                new Columna { Titulo = "Motivo", Ancho = 24, Valor = f => TextoMotivo(f) },
                new Columna { Titulo = "Consumo por día", Ancho = 10, Formato = FormatoCantidad, Valor = f => (decimal)f.ConsumoDiario },
                new Columna { Titulo = "Días que alcanza", Ancho = 10, Formato = "#,##0.0", Valor = f => f.CoberturaDias.HasValue ? (object)(decimal)f.CoberturaDias.Value : "" },
                new Columna { Titulo = "Última salida", Ancho = 12, Formato = "dd/mm/yyyy", Valor = f => f.UltimaSalida.HasValue ? (object)f.UltimaSalida.Value.Date : "" },
                new Columna { Titulo = "Stock en otras sucursales", Ancho = 28, Valor = f => string.Join(" · ", f.OtrasSucursales.Select(o => o.Sucursal + ": " + o.Stock.ToString("0.###", System.Globalization.CultureInfo.GetCultureInfo("es-AR")))) },
                new Columna { Titulo = "Último precio", Ancho = 13, Formato = FormatoImporte, Valor = f => f.UltimoPrecio.HasValue ? (object)f.UltimoPrecio.Value : "" },
                new Columna { Titulo = "Importe estimado", Ancho = 15, Formato = FormatoImporte, Valor = f => f.ImporteEstimado.HasValue ? (object)f.ImporteEstimado.Value : "" },
                new Columna { Titulo = "Observaciones", Ancho = 46, Valor = f => TextoObservaciones(f) }
            };
        }

        // Pedir en kg -> unidades aproximadas con el peso promedio del producto (solo pesables con promedio cargado).
        private static object UnidadesEquivalentes(ReposicionFilaVm f)
        {
            if (!f.Pedir.HasValue || !f.Pesable || f.Promedio <= 0f) return "";
            return Math.Round((decimal)f.Pedir.Value / (decimal)f.Promedio, 1);
        }
    }
}
