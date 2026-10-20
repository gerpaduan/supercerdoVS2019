// Excel (.xlsx real, ClosedXML) de Existencia por sucursales: lo que se ve en pantalla, con el filtro y el orden actuales.
// Columnas: Cod. | Producto | [Stock | Estado] por sucursal | [Stock | Estado] Total. Lo arma StockController.ExportarExistencia.
// Anchos de columna FIJOS a proposito (mismo criterio que ExcelReposicion): AdjustToContents necesita fuentes del sistema y
// no esta garantizado en el contenedor Linux. Detalle: docs/11-manual-de-uso/pantallas/Stock.ExistenciaPorSucursales.md.
using System.Collections.Generic;
using ClosedXML.Excel;

namespace WebCore.Services
{
    public static class ExcelExistenciaSucursales
    {
        private const string FormatoCantidad = "#,##0.000";
        private static readonly XLColor ColorEncabezado = XLColor.FromHtml("#E9ECEF");
        private static readonly XLColor ColorSecundario = XLColor.FromHtml("#6C757D");
        private static readonly XLColor ColorTinte = XLColor.FromHtml("#F4F5F7");
        private static readonly XLColor ColorRojo = XLColor.FromHtml("#B02A37");
        private static readonly XLColor ColorAmbar = XLColor.FromHtml("#9A6700");
        private static readonly XLColor ColorVerde = XLColor.FromHtml("#146C43");

        // Devuelve los bytes del archivo. Sin efectos secundarios.
        public static byte[] Generar(ExistenciaExportVm vm)
        {
            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Existencia");

                int fila = 1;
                hoja.Cell(fila, 1).Value = vm.Titulo;
                hoja.Cell(fila, 1).Style.Font.Bold = true;
                hoja.Cell(fila, 1).Style.Font.FontSize = 14;
                fila++;
                foreach (string linea in vm.Encabezado)
                {
                    hoja.Cell(fila, 1).Value = linea;
                    hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
                    fila++;
                }
                fila++;

                // Encabezado en dos filas: nombre del grupo (sucursal / Total) y debajo Stock / Estado.
                int porGrupo = vm.MostrarEstado ? 2 : 1;
                int filaGrupo = fila;
                int filaSub = fila + 1;
                int totalColumnas = 2 + (vm.Sucursales.Count + (vm.MostrarTotal ? 1 : 0)) * porGrupo;

                Encabezado(hoja, filaGrupo, 1, "Cód.");
                Encabezado(hoja, filaGrupo, 2, "Producto");
                hoja.Range(filaGrupo, 1, filaSub, 1).Merge();
                hoja.Range(filaGrupo, 2, filaSub, 2).Merge();

                var grupos = new List<string>(vm.Sucursales);
                if (vm.MostrarTotal) grupos.Add("Total");
                int col = 3;
                foreach (string grupo in grupos)
                {
                    Encabezado(hoja, filaGrupo, col, grupo);
                    if (porGrupo > 1) hoja.Range(filaGrupo, col, filaGrupo, col + porGrupo - 1).Merge();
                    hoja.Range(filaGrupo, col, filaGrupo, col + porGrupo - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    Encabezado(hoja, filaSub, col, "Stock");
                    if (vm.MostrarEstado) Encabezado(hoja, filaSub, col + 1, "Estado");
                    col += porGrupo;
                }

                hoja.Column(1).Width = 10;
                hoja.Column(2).Width = 40;
                for (int c = 3; c <= totalColumnas; c++)
                {
                    bool esEstado = vm.MostrarEstado && (c - 3) % 2 == 1;
                    hoja.Column(c).Width = esEstado ? 22 : 14;
                }

                fila = filaSub + 1;
                int primeraDatos = fila;
                foreach (var f in vm.Filas)
                {
                    hoja.Cell(fila, 1).Value = f.Codigo;
                    hoja.Cell(fila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    hoja.Cell(fila, 2).Value = f.Producto;

                    int columna = 3;
                    for (int i = 0; i < f.Celdas.Count; i++)
                    {
                        Celda(hoja, fila, columna, f.Celdas[i], vm.MostrarEstado, i % 2 == 1);
                        columna += porGrupo;
                    }
                    if (f.Total != null) Celda(hoja, fila, columna, f.Total, vm.MostrarEstado, false);
                    fila++;
                }

                if (vm.Filas.Count == 0)
                {
                    hoja.Cell(fila, 1).Value = "No hay productos para exportar con los filtros elegidos.";
                    hoja.Cell(fila, 1).Style.Font.Italic = true;
                    hoja.Cell(fila, 1).Style.Font.FontColor = ColorSecundario;
                }
                else
                {
                    hoja.Range(filaSub, 1, fila - 1, totalColumnas).SetAutoFilter();
                    hoja.SheetView.FreezeRows(filaSub);
                    hoja.SheetView.FreezeColumns(2);
                }

                using (var memoria = new System.IO.MemoryStream())
                {
                    libro.SaveAs(memoria);
                    return memoria.ToArray();
                }
            }
        }

        private static void Encabezado(IXLWorksheet hoja, int fila, int columna, string texto)
        {
            var celda = hoja.Cell(fila, columna);
            celda.Value = texto;
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = ColorEncabezado;
            celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        // Stock (numero real, ordenable en Excel) y, si corresponde, el estado como texto con el punto de stock entre parentesis.
        private static void Celda(IXLWorksheet hoja, int fila, int columna, ExistenciaExportCelda celda, bool mostrarEstado, bool tinte)
        {
            var stock = hoja.Cell(fila, columna);
            stock.Value = celda.Stock;
            stock.Style.NumberFormat.Format = FormatoCantidad;
            stock.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            if (tinte) stock.Style.Fill.BackgroundColor = ColorTinte;

            if (!mostrarEstado) return;

            var estado = hoja.Cell(fila, columna + 1);
            estado.Value = celda.EstadoTexto;
            estado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            if (tinte) estado.Style.Fill.BackgroundColor = ColorTinte;
            switch (celda.EstadoCodigo)
            {
                case "NEGATIVO":
                case "FALTA":
                    estado.Style.Font.FontColor = ColorRojo;
                    estado.Style.Font.Bold = true;
                    break;
                case "EN_PUNTO":
                    estado.Style.Font.FontColor = ColorAmbar;
                    estado.Style.Font.Bold = true;
                    break;
                case "OK":
                    estado.Style.Font.FontColor = ColorVerde;
                    break;
                default:
                    estado.Style.Font.FontColor = ColorSecundario;
                    break;
            }
        }
    }
}
