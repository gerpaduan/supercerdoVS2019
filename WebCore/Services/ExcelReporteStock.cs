// Excel (.xlsx real, ClosedXML) de los reportes de stock de Reportes (Stock Actual, Cierre Stock y Stock Retroactivo,
// que comparten tabla). Lo arma ReportesController.ExportarStockExcel con los mismos filtros que la pantalla.
// Una hoja: titulo, periodo/sucursales/filtros, encabezados con filtro automatico y una fila por producto (y sucursal).
// Detalle: docs/11-manual-de-uso/pantallas/Reportes.Index.md. Depende de ClosedXML (MIT) y de ReportesViewModel.
//
// Los anchos de columna son FIJOS a proposito: AdjustToContents usa SixLabors.Fonts, que necesita fuentes del
// sistema y no esta garantizado en el contenedor Linux (mismo criterio que GenerarExcelCore).
using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using WebCore.Models;

namespace WebCore.Services
{
    public static class ExcelReporteStock
    {
        private const string FormatoCantidad = "#,##0.000";
        // Faltante con signo explicito (+ falto / - vendido de mas), igual que la pantalla.
        private const string FormatoFaltante = "+#,##0.000;-#,##0.000;0.000";

        private static readonly XLColor ColorEncabezado = XLColor.FromHtml("#E9ECEF");
        private static readonly XLColor ColorSecundario = XLColor.FromHtml("#6C757D");
        private static readonly XLColor ColorStockCierre = XLColor.FromHtml("#1D4670");
        private static readonly XLColor ColorFaltaMercaderia = XLColor.FromHtml("#A61E2D");
        private static readonly XLColor ColorVendidoDeMas = XLColor.FromHtml("#8A5300");
        private static readonly XLColor ColorCoincide = XLColor.FromHtml("#1E6B3A");

        // Una columna del Excel: titulo, ancho, y como sacar su valor de la fila. Numerica = se escribe como numero
        // con formato (se puede sumar y ordenar); si no, como texto.
        private sealed class Columna
        {
            public string Titulo = "";
            public double Ancho;
            public Func<ReporteStockFilaVm, object> Valor = _ => "";
            public string Formato;
        }

        // Genera el .xlsx de las filas ya cargadas en el modelo (el controller ya aplico los filtros, incluido el de
        // estado). incluirDetalle agrega las columnas de ingresos/egresos desglosados (el switch "Mostrar detalles").
        // Devuelve los bytes del archivo; sin efectos secundarios.
        public static byte[] Generar(ReportesViewModel model, bool incluirDetalle, string sucursalesTexto)
        {
            bool esCierreStock = string.Equals(model.TipoReporteSeleccionado, "Cierre Stock", StringComparison.OrdinalIgnoreCase);
            var columnas = ConstruirColumnas(model.UsaCierresPorSucursal, esCierreStock, incluirDetalle);

            using var libro = new XLWorkbook();
            // El nombre de una hoja admite hasta 31 caracteres y no "/": el tipo de reporte cumple ambas cosas.
            var hoja = libro.Worksheets.Add(model.TipoReporteSeleccionado);

            int fila = 1;
            hoja.Cell(fila, 1).Value = model.TipoReporteSeleccionado;
            hoja.Cell(fila, 1).Style.Font.Bold = true;
            hoja.Cell(fila, 1).Style.Font.FontSize = 14;
            fila++;

            foreach (var linea in LineasEncabezado(model, esCierreStock, sucursalesTexto))
            {
                hoja.Cell(fila, 1).Value = linea;
                hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
                fila++;
            }
            fila++;

            // Encabezados de columna.
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

            // Una fila por producto (y sucursal, con 2+ sucursales).
            int primeraFilaDatos = fila;
            foreach (var item in model.FilasStockActual)
            {
                for (int c = 0; c < columnas.Count; c++)
                {
                    var celda = hoja.Cell(fila, c + 1);
                    object valor = columnas[c].Valor(item);
                    if (valor is decimal numero)
                        celda.Value = numero;
                    else if (valor is long entero)
                        celda.Value = entero;
                    else
                        celda.Value = valor?.ToString() ?? "";
                    if (columnas[c].Formato != null)
                        celda.Style.NumberFormat.Format = columnas[c].Formato;
                }

                if (esCierreStock)
                    PintarColumnasCierre(hoja, fila, columnas, item);
                fila++;
            }

            if (model.FilasStockActual.Count == 0)
            {
                hoja.Cell(fila, 1).Value = "Sin resultados para los filtros elegidos.";
                hoja.Cell(fila, 1).Style.Font.Italic = true;
                hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
            }
            else
            {
                // Filtro automatico sobre los encabezados y fila de encabezados fija al desplazarse.
                hoja.Range(filaEncabezado, 1, fila - 1, columnas.Count).SetAutoFilter();
                hoja.SheetView.FreezeRows(filaEncabezado);
                hoja.SheetView.FreezeColumns(1);
            }

            // Hoja para imprimir apaisada y ajustada al ancho de la pagina.
            hoja.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            hoja.PageSetup.FitToPages(1, 0);
            hoja.PageSetup.SetRowsToRepeatAtTop(filaEncabezado, filaEncabezado);

            using var memoria = new System.IO.MemoryStream();
            libro.SaveAs(memoria);
            return memoria.ToArray();
        }

        // Faltante y Stock Cierre se colorean como en la pantalla (el color nunca va solo: el signo y el formato
        // numerico ya distinguen los casos, y la columna Estado lo repite en texto).
        private static void PintarColumnasCierre(IXLWorksheet hoja, int fila, List<Columna> columnas, ReporteStockFilaVm item)
        {
            int idxCierre = columnas.FindIndex(c => c.Titulo == "Stock Cierre");
            int idxFaltante = columnas.FindIndex(c => c.Titulo == "Faltante");
            if (idxCierre >= 0)
            {
                hoja.Cell(fila, idxCierre + 1).Style.Font.FontColor = ColorStockCierre;
                hoja.Cell(fila, idxCierre + 1).Style.Font.Bold = true;
            }
            if (idxFaltante >= 0)
            {
                // Se clasifica sobre el valor redondeado a 3 decimales (los que se muestran), como la pantalla.
                decimal faltante = Math.Round(item.StockActual, 3);
                var celda = hoja.Cell(fila, idxFaltante + 1);
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontColor = faltante > 0 ? ColorFaltaMercaderia : faltante < 0 ? ColorVendidoDeMas : ColorCoincide;
            }
        }

        private static List<Columna> ConstruirColumnas(bool multiSucursal, bool esCierreStock, bool incluirDetalle)
        {
            var columnas = new List<Columna>
            {
                new Columna { Titulo = "Código", Ancho = 9, Valor = f => f.Codigo > 0 ? (object)f.Codigo : "-" },
                new Columna { Titulo = "Producto", Ancho = 34, Valor = f => f.Producto }
            };

            if (multiSucursal)
                columnas.Add(new Columna { Titulo = "Sucursal", Ancho = 18, Valor = f => f.Sucursal });

            columnas.Add(Numero("St. inicial", f => f.StockInicial));

            if (incluirDetalle)
            {
                columnas.Add(Numero("Ingr. compra", f => f.Compras));
                columnas.Add(Numero("Ingr. elab.", f => f.IngresoElaborado));
                columnas.Add(Numero("Ingr. stock", f => f.IngresoStock));
                columnas.Add(Numero("Ingr. mov.", f => f.IngresoMovimiento));
                columnas.Add(Numero("Ajustes", f => f.AjusteStock));
            }

            columnas.Add(Numero("Total ingresos", f => f.TotalIngresos));

            if (incluirDetalle)
            {
                columnas.Add(Numero("Egr. stock", f => f.EgresoStock));
                columnas.Add(Numero("Egr. mov.", f => f.EgresoMovimiento));
                columnas.Add(Numero("Egr. elab.", f => f.EgresoElaborado));
                columnas.Add(Numero("Egr. venta", f => f.Ventas));
            }

            columnas.Add(Numero("Total egresos", f => f.TotalEgresos));

            if (esCierreStock)
            {
                columnas.Add(Numero("Stock Cierre", f => f.StockCierre));
                var faltante = Numero("Faltante", f => f.StockActual);
                faltante.Formato = FormatoFaltante;
                columnas.Add(faltante);
            }
            else
            {
                columnas.Add(Numero("Stock actual", f => f.StockActual));
            }

            columnas.Add(Numero("Punto stock", f => f.PuntoStock));
            columnas.Add(new Columna { Titulo = "Estado", Ancho = 12, Valor = f => f.EstadoStock });

            return columnas;
        }

        private static Columna Numero(string titulo, Func<ReporteStockFilaVm, decimal> valor)
        {
            return new Columna { Titulo = titulo, Ancho = 13, Formato = FormatoCantidad, Valor = f => valor(f) };
        }

        // Renglones de contexto bajo el titulo: periodo, sucursales y filtros activos (solo los que no estan en "todos").
        private static IEnumerable<string> LineasEncabezado(ReportesViewModel model, bool esCierreStock, string sucursalesTexto)
        {
            if (model.UsaCierresPorSucursal)
            {
                yield return esCierreStock
                    ? "Cada sucursal con sus propios cierres: del anteúltimo al último cierre."
                    : string.Equals(model.TipoReporteSeleccionado, "Stock Actual", StringComparison.OrdinalIgnoreCase)
                        ? "Cada sucursal con sus propios cierres: desde el último cierre hasta ahora."
                        : "Cada sucursal con sus propios cierres: desde el último cierre hasta la fecha elegida.";
            }
            else
            {
                yield return "Desde " + model.FechaDesde.ToString("dd/MM/yyyy HH:mm") + " hasta " + model.FechaHasta.ToString("dd/MM/yyyy HH:mm");
            }

            if (!string.IsNullOrWhiteSpace(sucursalesTexto))
                yield return "Sucursales: " + sucursalesTexto;

            var filtros = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.BusquedaProducto))
                filtros.Add("Producto: " + model.BusquedaProducto.Trim());
            if (!string.IsNullOrWhiteSpace(model.TipoProducto))
                filtros.Add("Tipo: " + model.TipoProducto.Trim());
            string marca = model.Marcas?.FirstOrDefault(m => m.Selected && m.Value != "0" && !string.IsNullOrEmpty(m.Value))?.Text;
            if (!string.IsNullOrWhiteSpace(marca))
                filtros.Add("Marca: " + marca);
            if (!string.IsNullOrWhiteSpace(model.EstadoStock) && !string.Equals(model.EstadoStock, "Todos", StringComparison.OrdinalIgnoreCase))
                filtros.Add("Estado: " + model.EstadoStock);
            if (filtros.Count > 0)
                yield return "Filtros: " + string.Join(" · ", filtros);

            if (model.Avisos != null)
            {
                foreach (var aviso in model.Avisos)
                    yield return "Aviso: " + aviso;
            }
        }
    }
}
