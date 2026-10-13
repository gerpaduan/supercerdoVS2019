// Excel "compacto para imprimir" de Ventas > Lineas (2026-10-13, ver docs/DECISIONS.md "Excel compacto de Lineas de
// venta"): sin colores ni fondos, letra de 8 pt, ordenado por fecha y hora, y las lineas repartidas en DOS BLOQUES
// lado a lado (A:E y G:K) para que cada hoja A4 vertical lleve el doble de lineas. Es la alternativa "sin estilo" al
// Excel con formato (GenerarExcelCore.GenerarExcelLineasVenta). Depende de ClosedXML y de LineasVentaExportHelper.
//
// Excel no tiene "columnas de diario", asi que el reparto lo calcula este codigo: filas de alto fijo, FilasPorHoja filas
// por bloque y un salto de pagina manual cada FilasPorHoja filas (el bloque izquierdo se llena de arriba abajo, sigue el
// derecho y despues la hoja siguiente). La fila 1 (titulos de columna de los dos bloques) se repite en cada hoja.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using WebCore.Models;

namespace WebCore.Services
{
    // Datos opcionales que elige el usuario antes de descargar. Vendedor y sucursal van en el renglon principal de
    // cada venta; el resto, en un renglon extra debajo (que no se genera si no se elige ninguno de ellos).
    public sealed class OpcionesExcelLineasCompacto
    {
        public bool Vendedor { get; set; }
        public bool Sucursal { get; set; }
        public bool Cliente { get; set; }
        public bool FormaPago { get; set; }
        public bool VentaId { get; set; }
        public bool Comprobante { get; set; }

        public bool HayDatosExtra => Cliente || FormaPago || VentaId || Comprobante;

        // Lee el CSV que manda la pantalla ("vendedor,cliente,..."). Es input externo: lo desconocido se ignora.
        public static OpcionesExcelLineasCompacto DesdeCsv(string csv)
        {
            var claves = new HashSet<string>(
                (csv ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

            return new OpcionesExcelLineasCompacto
            {
                Vendedor = claves.Contains("vendedor"),
                Sucursal = claves.Contains("sucursal"),
                Cliente = claves.Contains("cliente"),
                FormaPago = claves.Contains("formaPago"),
                VentaId = claves.Contains("ventaId"),
                Comprobante = claves.Contains("comprobante")
            };
        }
    }

    public static class ExcelLineasVentaCompacto
    {
        // Filas de datos por hoja (sin contar la fila 1 de titulos). Calculo: A4 vertical = 842 pt; margenes superior
        // 0,65" e inferior 0,55" => ~756 pt utiles; con filas de 11,25 pt y la fila de titulos entran ~66; se usan 64
        // para dejar margen a las diferencias de redondeo entre Excel y otros programas. Si se cambian margenes, alto de
        // fila o papel, recalcular: si es mayor a lo que entra, Excel corta antes y las hojas quedan desparejas.
        private const int FilasPorHoja = 64;
        private const double AlturaFila = 11.25;
        private const double TamanoFuente = 8;

        // Una venta de hasta esta cantidad de filas no se parte entre dos bloques: si no entra en lo que queda, pasa
        // entera al bloque siguiente. Las mas largas se parten y se repite el renglon principal ("continua").
        private const int MaxFilasVentaSinPartir = 12;

        // Caracteres que entran en un renglon de un bloque (merge A:E, 8 pt). Aproximado: el ancho real depende de las
        // letras. Si el texto extra supera esto, ocupa mas renglones; si se queda corto, se recorta (no se pierde).
        private const int CaracteresPorRenglonExtra = 62;

        // Columnas de la hoja: bloque izquierdo A:E (1-5), separador F (6), bloque derecho G:K (7-11).
        private const int ColumnasBloque = 5;
        private const int ColumnaInicioIzquierdo = 1;
        private const int ColumnaInicioDerecho = 7;
        private const int UltimaColumna = 11;

        private const string FormatoKg = "#,##0.000";
        private const string FormatoImporte = "#,##0.00";
        private const string FormatoMoneda = "\"$\" #,##0.00";

        private static readonly XLColor ColorSeparador = XLColor.FromHtml("#999999");

        // Devuelve los bytes del .xlsx. Sin efectos secundarios. Asume que cada venta del modelo tiene al menos una
        // linea (ConstruirModeloLineas omite las que no).
        public static byte[] Generar(VentaLineasIndexVm model, string sucursalNombre, OpcionesExcelLineasCompacto opciones)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add(LineasVentaExportHelper.Titulo);
            hoja.Style.Font.FontSize = TamanoFuente;

            // Anchos fijos (AdjustToContents necesita fuentes del sistema). Suman ~547 pt por los dos bloques y el
            // separador: entran en el ancho util del A4 vertical con margenes laterales de 0,25".
            double[] anchos = { 4.5, 15.5, 7.5, 9.5, 10.5 };
            for (int i = 0; i < ColumnasBloque; i++)
            {
                hoja.Column(ColumnaInicioIzquierdo + i).Width = anchos[i];
                hoja.Column(ColumnaInicioDerecho + i).Width = anchos[i];
            }
            hoja.Column(ColumnaInicioIzquierdo + ColumnasBloque).Width = 1.5;

            EscribirTitulosColumnas(hoja, ColumnaInicioIzquierdo);
            EscribirTitulosColumnas(hoja, ColumnaInicioDerecho);

            // Posicion actual dentro del flujo: pagina, bloque (0 izquierdo / 1 derecho) y fila dentro del bloque.
            int pagina = 0;
            int bloque = 0;
            int fila = 0;
            var filasUsadasPorPagina = new Dictionary<int, int>();

            int FilaHoja(int paginaActual, int filaActual) => 2 + paginaActual * FilasPorHoja + filaActual;
            int ColumnaBloque(int bloqueActual) => bloqueActual == 0 ? ColumnaInicioIzquierdo : ColumnaInicioDerecho;

            void Avanzar()
            {
                if (bloque == 0)
                {
                    bloque = 1;
                }
                else
                {
                    bloque = 0;
                    pagina++;
                }
                fila = 0;
            }

            // Registra cuantas filas lleva ocupadas la pagina (para ubicar los totales al final).
            void RegistrarUso(int cantidadFilas)
            {
                fila += cantidadFilas;
                filasUsadasPorPagina.TryGetValue(pagina, out int previo);
                filasUsadasPorPagina[pagina] = Math.Max(previo, fila);
            }

            if (model.Ventas.Count == 0)
            {
                var sinVentas = hoja.Range(2, 1, 2, UltimaColumna);
                sinVentas.Merge();
                hoja.Cell(2, 1).Value = LineasVentaExportHelper.MensajeSinVentas;
                sinVentas.Style.Font.Italic = true;
            }

            // Cronologico: de la venta mas vieja a la mas nueva (la pantalla las muestra al reves).
            foreach (var venta in model.Ventas.OrderBy(v => v.FechaVenta).ThenBy(v => v.IdVenta))
            {
                string textoExtra = TextoExtra(venta, opciones);
                int renglonesExtra = textoExtra.Length == 0
                    ? 0
                    : (int)Math.Ceiling(textoExtra.Length / (double)CaracteresPorRenglonExtra);
                int filasVenta = 1 + renglonesExtra + venta.Lineas.Count;

                // Si no entra en lo que queda del bloque: se pasa entera al siguiente (ventas cortas) o se parte (largas).
                // Nunca queda el renglon principal (con su extra) separado de la primera linea.
                int restantes = FilasPorHoja - fila;
                int minimo = 1 + renglonesExtra + 1;
                if (filasVenta > restantes && (restantes < minimo || filasVenta <= MaxFilasVentaSinPartir))
                    Avanzar();

                EscribirRenglonPrincipal(hoja, FilaHoja(pagina, fila), ColumnaBloque(bloque), venta, opciones, continuacion: false);
                RegistrarUso(1);

                if (renglonesExtra > 0)
                {
                    EscribirRenglonExtra(hoja, FilaHoja(pagina, fila), ColumnaBloque(bloque), textoExtra, renglonesExtra);
                    RegistrarUso(renglonesExtra);
                }

                foreach (var linea in venta.Lineas)
                {
                    if (fila >= FilasPorHoja)
                    {
                        Avanzar();
                        EscribirRenglonPrincipal(hoja, FilaHoja(pagina, fila), ColumnaBloque(bloque), venta, opciones, continuacion: true);
                        RegistrarUso(1);
                    }

                    EscribirLinea(hoja, FilaHoja(pagina, fila), ColumnaBloque(bloque), linea);
                    RegistrarUso(1);
                }
            }

            // Totales generales (mismos que las tarjetas de la pantalla), debajo de lo ultimo; si no entra, en la hoja siguiente.
            int paginaFinal = pagina;
            if (model.Ventas.Count > 0)
            {
                var totales = LineasVentaExportHelper.Totales(model);
                filasUsadasPorPagina.TryGetValue(pagina, out int usadas);
                int filaTotales = usadas + 1;
                if (filaTotales >= FilasPorHoja)
                {
                    paginaFinal++;
                    filaTotales = 0;
                }

                int filaHojaTotales = FilaHoja(paginaFinal, filaTotales);
                var rangoTotales = hoja.Range(filaHojaTotales, 1, filaHojaTotales, UltimaColumna);
                rangoTotales.Merge();
                hoja.Cell(filaHojaTotales, 1).Value = "Total Kg: " + LineasVentaExportHelper.Kg(totales.TotalKg)
                    + "  ·  Total pesos: " + LineasVentaExportHelper.Moneda(totales.TotalPesos)
                    + "  ·  " + LineasVentaExportHelper.ResumenCantidades(totales.CantidadLineas, totales.CantidadRegistros);
                rangoTotales.Style.Font.Bold = true;
                rangoTotales.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                filasUsadasPorPagina[paginaFinal] = Math.Max(filasUsadasPorPagina.GetValueOrDefault(paginaFinal), filaTotales + 1);
            }

            // Alto fijo en todas las filas usadas (el reparto por hoja depende de que sea exacto).
            int ultimaFila = FilaHoja(paginaFinal, filasUsadasPorPagina.GetValueOrDefault(paginaFinal)) - 1;
            hoja.Rows(1, Math.Max(ultimaFila, 2)).Height = AlturaFila;

            ConfigurarImpresion(hoja, model, sucursalNombre, paginaFinal);

            using var salida = new MemoryStream();
            libro.SaveAs(salida);
            return salida.ToArray();
        }

        // Fila 1: titulos de cada bloque (la fila se repite en todas las hojas al imprimir).
        private static void EscribirTitulosColumnas(IXLWorksheet hoja, int columnaInicio)
        {
            string[] titulos = { "Cód.", "Descripción", "Cant.", "Precio", "Total" };
            for (int i = 0; i < titulos.Length; i++)
            {
                var celda = hoja.Cell(1, columnaInicio + i);
                celda.Value = titulos[i];
                celda.Style.Font.Bold = true;
                celda.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                if (i >= 2)
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }
        }

        // Renglon principal: fecha y hora (+ vendedor y sucursal si se eligieron) a la izquierda y el total de LA VENTA
        // a la derecha. Una linea fina gris arriba separa las ventas. En la continuacion (venta partida entre bloques)
        // solo se repite la fecha con "(continua)" y no el total.
        private static void EscribirRenglonPrincipal(IXLWorksheet hoja, int fila, int columnaInicio, VentaLineasGrupoVm venta,
            OpcionesExcelLineasCompacto opciones, bool continuacion)
        {
            var textoPrincipal = hoja.Range(fila, columnaInicio, fila, columnaInicio + ColumnasBloque - 2);
            textoPrincipal.Merge();

            string fecha = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm", LineasVentaExportHelper.CulturaAr);
            var partes = new List<string> { fecha };
            if (continuacion)
            {
                partes.Add("(continúa)");
            }
            else
            {
                if (opciones.Vendedor)
                    partes.Add(Campo(venta, "Vendedor"));
                if (opciones.Sucursal)
                    partes.Add(Campo(venta, "Sucursal"));
            }
            hoja.Cell(fila, columnaInicio).Value = string.Join("  ·  ", partes);

            var renglon = hoja.Range(fila, columnaInicio, fila, columnaInicio + ColumnasBloque - 1);
            renglon.Style.Font.Bold = true;
            renglon.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            renglon.Style.Border.TopBorderColor = ColorSeparador;

            if (!continuacion)
            {
                var celdaTotal = hoja.Cell(fila, columnaInicio + ColumnasBloque - 1);
                EscribirNumero(celdaTotal, venta.TotalImporte, FormatoMoneda);
            }
        }

        // Renglon extra (opcional): cliente, forma de pago, Venta ID y comprobante, en uno o mas renglones con ajuste.
        private static void EscribirRenglonExtra(IXLWorksheet hoja, int fila, int columnaInicio, string texto, int renglones)
        {
            var rango = hoja.Range(fila, columnaInicio, fila + renglones - 1, columnaInicio + ColumnasBloque - 1);
            rango.Merge();
            hoja.Cell(fila, columnaInicio).Value = texto;
            rango.Style.Alignment.WrapText = true;
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        }

        private static void EscribirLinea(IXLWorksheet hoja, int fila, int columnaInicio, VentaLineaDetalleVm linea)
        {
            // El codigo se guarda como numero cuando lo es (se puede ordenar/filtrar); "-" queda como texto.
            var celdaCodigo = hoja.Cell(fila, columnaInicio);
            if (int.TryParse(linea.Codigo, NumberStyles.None, CultureInfo.InvariantCulture, out int codigo))
                celdaCodigo.Value = codigo;
            else
                celdaCodigo.Value = linea.Codigo ?? "";
            celdaCodigo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            hoja.Cell(fila, columnaInicio + 1).Value = linea.Producto ?? "";
            EscribirNumero(hoja.Cell(fila, columnaInicio + 2), linea.CantidadKg, FormatoKg);
            EscribirNumero(hoja.Cell(fila, columnaInicio + 3), linea.Precio, FormatoImporte);
            EscribirNumero(hoja.Cell(fila, columnaInicio + 4), linea.Total, FormatoImporte);
        }

        private static void EscribirNumero(IXLCell celda, decimal valor, string formato)
        {
            celda.Value = valor;
            celda.Style.NumberFormat.Format = formato;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }

        // Valor de un dato de la venta (los "Campos" del modelo, ver VentasController.ConstruirModeloLineas) o "-".
        private static string Campo(VentaLineasGrupoVm venta, string etiqueta)
        {
            var campo = venta.Campos.FirstOrDefault(x => x.Etiqueta == etiqueta);
            return campo != null && !string.IsNullOrWhiteSpace(campo.Valor) ? campo.Valor : "-";
        }

        // "Cliente: X  ·  Pago: Y  ·  Venta: N  ·  Comp.: Z" con lo que el usuario eligio; "" si nada. Las etiquetas son
        // cortas a proposito: con datos normales el renglon extra entra en una sola fila del bloque.
        private static string TextoExtra(VentaLineasGrupoVm venta, OpcionesExcelLineasCompacto opciones)
        {
            var partes = new List<string>();
            if (opciones.Cliente)
                partes.Add("Cliente: " + Campo(venta, "Cliente"));
            if (opciones.FormaPago)
                partes.Add("Pago: " + Campo(venta, "Forma de pago"));
            if (opciones.VentaId)
                partes.Add("Venta: " + venta.IdVenta.ToString(CultureInfo.InvariantCulture));
            if (opciones.Comprobante)
                partes.Add("Comp.: " + Campo(venta, "Nro/comprobante"));
            return string.Join("  ·  ", partes);
        }

        // Papel, margenes, fila de titulos repetida, encabezado/pie de pagina y saltos de pagina manuales.
        private static void ConfigurarImpresion(IXLWorksheet hoja, VentaLineasIndexVm model, string sucursalNombre, int ultimaPagina)
        {
            var impresion = hoja.PageSetup;
            impresion.PaperSize = XLPaperSize.A4Paper;
            impresion.PageOrientation = XLPageOrientation.Portrait;
            impresion.Scale = 100;
            impresion.Margins.Top = 0.65;
            impresion.Margins.Bottom = 0.55;
            impresion.Margins.Left = 0.25;
            impresion.Margins.Right = 0.25;
            impresion.Margins.Header = 0.25;
            impresion.Margins.Footer = 0.25;
            impresion.CenterHorizontally = true;
            impresion.SetRowsToRepeatAtTop(1, 1);

            // En el encabezado/pie de Excel, "&" es un caracter especial: se duplica para escribirlo literal.
            string Literal(string texto) => (texto ?? "").Replace("&", "&&");

            string encabezado = LineasVentaExportHelper.Titulo + "  ·  " + LineasVentaExportHelper.Periodo(model, sucursalNombre);
            string filtros = LineasVentaExportHelper.Filtros(model);
            if (filtros.Length > 0)
                encabezado += "\n" + filtros;
            impresion.Header.Left.AddText(Literal(encabezado), XLHFOccurrence.AllPages).SetFontSize(8);

            impresion.Footer.Center.AddText("Página ", XLHFOccurrence.AllPages).SetFontSize(8);
            impresion.Footer.Center.AddText(XLHFPredefinedText.PageNumber, XLHFOccurrence.AllPages);
            impresion.Footer.Center.AddText(" de ", XLHFOccurrence.AllPages).SetFontSize(8);
            impresion.Footer.Center.AddText(XLHFPredefinedText.NumberOfPages, XLHFOccurrence.AllPages);

            // Salto manual al final de cada hoja (fila 1 de titulos + FilasPorHoja filas de datos por hoja).
            for (int p = 0; p < ultimaPagina; p++)
                impresion.AddHorizontalPageBreak(1 + (p + 1) * FilasPorHoja);
        }
    }
}
