// Excel (.xlsx real, ClosedXML) de un pedido: una fila por producto y una columna de cantidad por sucursal, mas el total pedido, el ultimo
// precio de compra y el importe estimado; abajo el total estimado. Anchos de columna FIJOS a proposito (mismo criterio que ExcelReposicion:
// AdjustToContents necesita fuentes del sistema y no esta garantizado en el contenedor Linux).
using System;
using System.Linq;
using ClosedXML.Excel;
using Entidades;

namespace WebCore.Services
{
    public static class ExcelPedido
    {
        private const string FormatoCantidad = "#,##0.###";
        private const string FormatoCantidadEntera = "#,##0";
        private const string FormatoImporte = "$ #,##0.00";
        private static readonly XLColor ColorEncabezado = XLColor.FromHtml("#E9ECEF");
        private static readonly XLColor ColorSecundario = XLColor.FromHtml("#6C757D");
        private static readonly XLColor ColorPrecioViejo = XLColor.FromHtml("#D9730D");

        public static byte[] Generar(Pedido pedido)
        {
            var pivot = PedidoPivot.Construir(pedido);

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Pedido " + pedido.Id);

                int fila = 1;
                hoja.Cell(fila, 1).Value = "Pedido N° " + pedido.Id;
                hoja.Cell(fila, 1).Style.Font.Bold = true;
                hoja.Cell(fila, 1).Style.Font.FontSize = 14;
                fila++;
                Secundaria(hoja, ref fila, "Fecha: " + pedido.Fecha.ToString("dd/MM/yyyy HH:mm") + " · Usuario: " + pedido.Usuario);
                if (!string.IsNullOrWhiteSpace(pedido.Nota)) Secundaria(hoja, ref fila, "Nota: " + pedido.Nota);
                // De donde sale el importe: el mismo texto que la pantalla, el ticket, el PDF y el mail (Negocio.PedidoTextos).
                Secundaria(hoja, ref fila, global::Negocio.PedidoTextos.AclaracionPrecio);
                if (pivot.HayPreciosViejos) Secundaria(hoja, ref fila, global::Negocio.PedidoTextos.AvisoPrecioViejo(pivot.DiasPrecioViejo) + " (columna \"Fecha del precio\" en naranja).");
                fila++;

                int colCodigo = 1, colProducto = 2, primeraSucursal = 3;
                int colTotal = primeraSucursal + pivot.Sucursales.Count;
                int colPrecio = colTotal + 1;
                int colImporte = colTotal + 2;
                // De quien y de cuando es el precio de cada producto (columnas informativas al final, para no mover las de siempre).
                int colProveedor = colTotal + 3;
                int colFechaPrecio = colTotal + 4;
                int ultimaColumna = colFechaPrecio;

                int filaEncabezado = fila;
                Encabezado(hoja, filaEncabezado, colCodigo, "Cód.");
                Encabezado(hoja, filaEncabezado, colProducto, "Producto");
                for (int i = 0; i < pivot.Sucursales.Count; i++)
                    Encabezado(hoja, filaEncabezado, primeraSucursal + i, pivot.Sucursales[i].Nombre);
                Encabezado(hoja, filaEncabezado, colTotal, "Total pedido");
                Encabezado(hoja, filaEncabezado, colPrecio, "$ últ. compra");
                Encabezado(hoja, filaEncabezado, colImporte, "Importe est.");
                Encabezado(hoja, filaEncabezado, colProveedor, "Proveedor de la compra");
                Encabezado(hoja, filaEncabezado, colFechaPrecio, "Fecha del precio");

                hoja.Column(colProveedor).Width = 26;
                hoja.Column(colFechaPrecio).Width = 16;
                hoja.Column(colCodigo).Width = 14;
                hoja.Column(colProducto).Width = 40;
                for (int c = primeraSucursal; c <= colTotal; c++) hoja.Column(c).Width = 15;
                hoja.Column(colPrecio).Width = 16;
                hoja.Column(colImporte).Width = 16;
                fila++;

                foreach (var item in pivot.Filas)
                {
                    hoja.Cell(fila, colCodigo).Value = item.Codigo;
                    hoja.Cell(fila, colCodigo).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    hoja.Cell(fila, colProducto).Value = item.Producto;

                    for (int i = 0; i < pivot.Sucursales.Count; i++)
                    {
                        var celda = hoja.Cell(fila, primeraSucursal + i);
                        if (item.Cantidades[i].HasValue)
                        {
                            celda.Value = item.Cantidades[i].Value;
                            celda.Style.NumberFormat.Format = FormatoCantidadPara(item.Cantidades[i].Value);
                        }
                    }

                    var total = hoja.Cell(fila, colTotal);
                    total.Value = item.Total;
                    total.Style.NumberFormat.Format = FormatoCantidadPara(item.Total);
                    total.Style.Font.Bold = true;

                    var precio = hoja.Cell(fila, colPrecio);
                    var importe = hoja.Cell(fila, colImporte);
                    if (item.Precio.HasValue)
                    {
                        precio.Value = item.Precio.Value;
                        precio.Style.NumberFormat.Format = FormatoImporte;
                    }
                    else
                    {
                        precio.Value = "sin precio";
                        precio.Style.Font.FontColor = ColorSecundario;
                        precio.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }

                    if (item.Importe.HasValue)
                    {
                        importe.Value = item.Importe.Value;
                        importe.Style.NumberFormat.Format = FormatoImporte;
                    }
                    else
                    {
                        importe.Value = "—";
                        importe.Style.Font.FontColor = ColorSecundario;
                        importe.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }

                    if (item.Precio.HasValue)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Proveedor)) hoja.Cell(fila, colProveedor).Value = item.Proveedor.Trim();
                        if (item.FechaPrecio.HasValue)
                        {
                            var fechaPrecio = hoja.Cell(fila, colFechaPrecio);
                            fechaPrecio.Value = item.FechaPrecio.Value.Date;
                            fechaPrecio.Style.DateFormat.Format = "dd/mm/yyyy";
                            fechaPrecio.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            // Precio viejo respecto de la fecha del pedido: fecha en naranja y en negrita.
                            if (item.PrecioViejo)
                            {
                                fechaPrecio.Style.Font.FontColor = ColorPrecioViejo;
                                fechaPrecio.Style.Font.Bold = true;
                            }
                        }
                    }
                    fila++;
                }

                int ultimaFila = fila - 1;
                if (pivot.Filas.Count > 0)
                {
                    hoja.Range(filaEncabezado, 1, ultimaFila, ultimaColumna).SetAutoFilter();
                    hoja.SheetView.FreezeRows(filaEncabezado);
                    hoja.SheetView.FreezeColumns(colProducto);
                }

                hoja.Cell(fila, colPrecio).Value = global::Negocio.PedidoTextos.EtiquetaTotal(pivot.LineasSinPrecio);
                hoja.Cell(fila, colPrecio).Style.Font.Bold = true;
                hoja.Cell(fila, colPrecio).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                hoja.Cell(fila, colImporte).Value = pivot.TotalEstimado;
                hoja.Cell(fila, colImporte).Style.NumberFormat.Format = FormatoImporte;
                hoja.Cell(fila, colImporte).Style.Font.Bold = true;
                if (pivot.LineasSinPrecio > 0)
                {
                    fila++;
                    hoja.Cell(fila, colImporte).Value = global::Negocio.PedidoTextos.AvisoSinPrecio(pivot.LineasSinPrecio);
                    hoja.Cell(fila, colImporte).Style.Font.FontColor = ColorSecundario;
                    hoja.Cell(fila, colImporte).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                using (var memoria = new System.IO.MemoryStream())
                {
                    libro.SaveAs(memoria);
                    return memoria.ToArray();
                }
            }
        }

        // "#,##0.###" muestra "12." (con el punto decimal colgado) cuando la cantidad es entera: para enteros se usa un formato sin decimales.
        private static string FormatoCantidadPara(decimal valor)
        {
            return valor == decimal.Truncate(valor) ? FormatoCantidadEntera : FormatoCantidad;
        }

        private static void Secundaria(IXLWorksheet hoja, ref int fila, string texto)
        {
            hoja.Cell(fila, 1).Value = texto;
            hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
            fila++;
        }

        private static void Encabezado(IXLWorksheet hoja, int fila, int columna, string texto)
        {
            var celda = hoja.Cell(fila, columna);
            celda.Value = texto;
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = ColorEncabezado;
            celda.Style.Alignment.WrapText = true;
            celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
    }
}
