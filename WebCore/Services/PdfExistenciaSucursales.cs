// PDF (QuestPDF) de Existencia por sucursales: lo que se ve en pantalla, con el filtro y el orden actuales, en A4 apaisado con
// el encabezado repetido en cada hoja. Columnas: Cod. | Producto | [Stock | Estado] por sucursal | [Stock | Estado] Total.
// Lo arma StockController.ExportarExistencia. La licencia de QuestPDF se fija una vez en Program.cs.
using System.Globalization;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WebCore.Services
{
    public static class PdfExistenciaSucursales
    {
        private const float TamanoLetra = 7.5f;
        private static readonly string ColorEncabezado = "#E9ECEF";
        private static readonly string ColorTinte = "#F4F5F7";

        public static byte[] Generar(ExistenciaExportVm vm)
        {
            return Crear(vm).GeneratePdf();
        }

        // Documento listo para renderizar (publico para poder revisarlo como imagen sin pasar por el PDF).
        public static IDocument Crear(ExistenciaExportVm vm)
        {
            var cultura = new CultureInfo("es-AR");
            int porGrupo = vm.MostrarEstado ? 2 : 1;
            int grupos = vm.Sucursales.Count + (vm.MostrarTotal ? 1 : 0);

            var documento = Document.Create(contenedor =>
            {
                contenedor.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4.Landscape());
                    pagina.Margin(20, Unit.Point);
                    pagina.DefaultTextStyle(x => x.FontSize(TamanoLetra));

                    pagina.Header().Column(col =>
                    {
                        col.Item().Text(vm.Titulo).FontSize(13).Bold();
                        foreach (string linea in vm.Encabezado)
                            col.Item().Text(linea).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(4);
                    });

                    pagina.Content().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(66);       // Cod. (un codigo de barras entra en una linea)
                            c.RelativeColumn(3.2f);     // Producto
                            for (int g = 0; g < grupos; g++)
                            {
                                c.RelativeColumn(1.1f); // Stock
                                if (vm.MostrarEstado) c.RelativeColumn(1.6f); // Estado
                            }
                        });

                        // Encabezado en dos filas (grupo y Stock/Estado), repetido en cada hoja.
                        tabla.Header(h =>
                        {
                            h.Cell().RowSpan(2).Element(Encabezado).Text("Cód.").Bold();
                            h.Cell().RowSpan(2).Element(Encabezado).Text("Producto").Bold();
                            for (int g = 0; g < grupos; g++)
                            {
                                string nombre = g < vm.Sucursales.Count ? vm.Sucursales[g] : "Total";
                                h.Cell().ColumnSpan((uint)porGrupo).Element(c => InicioGrupo(c)).Element(Encabezado).AlignCenter().Text(nombre).Bold();
                            }
                            for (int g = 0; g < grupos; g++)
                            {
                                h.Cell().Element(c => InicioGrupo(c)).Element(Encabezado).AlignRight().Text("Stock").Bold();
                                if (vm.MostrarEstado) h.Cell().Element(Encabezado).AlignRight().Text("Estado").Bold();
                            }
                        });

                        foreach (var fila in vm.Filas)
                        {
                            tabla.Cell().Element(Celda).Text(fila.Codigo.ToString());
                            tabla.Cell().Element(Celda).Text(fila.Producto);

                            var celdas = fila.Celdas.ToList();
                            if (fila.Total != null) celdas.Add(fila.Total);
                            for (int i = 0; i < celdas.Count; i++)
                            {
                                var c = celdas[i];
                                bool tinte = i < vm.Sucursales.Count && i % 2 == 1;
                                tabla.Cell().Element(x => InicioGrupo(x)).Element(x => Celda(x, tinte)).AlignRight().Text(c.Stock.ToString("N3", cultura));
                                if (vm.MostrarEstado)
                                    tabla.Cell().Element(x => Celda(x, tinte)).AlignRight().Text(c.EstadoTexto).FontColor(ColorEstado(c.EstadoCodigo));
                            }
                        }

                        if (vm.Filas.Count == 0)
                            tabla.Cell().ColumnSpan((uint)(2 + grupos * porGrupo)).Padding(6).Text("No hay productos para exportar con los filtros elegidos.").Italic();
                    });

                    pagina.Footer().AlignRight().Text(t =>
                    {
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                });
            });

            return documento;
        }

        private static IContainer Encabezado(IContainer c)
        {
            return c.Background(ColorEncabezado).BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(2).PaddingHorizontal(3);
        }

        // Doble separacion visual entre sucursales: linea gruesa a la izquierda de la primera columna de cada grupo.
        private static IContainer InicioGrupo(IContainer c)
        {
            return c.BorderLeft(1.2f).BorderColor(Colors.Grey.Darken1);
        }

        private static IContainer Celda(IContainer c)
        {
            return Celda(c, false);
        }

        private static IContainer Celda(IContainer c, bool tinte)
        {
            var celda = tinte ? c.Background(ColorTinte) : c;
            return celda.BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(1.5f).PaddingHorizontal(3);
        }

        private static string ColorEstado(string codigo)
        {
            switch (codigo)
            {
                case "NEGATIVO":
                case "FALTA": return Colors.Red.Darken2;
                case "EN_PUNTO": return Colors.Orange.Darken3;
                case "OK": return Colors.Green.Darken2;
                default: return Colors.Grey.Darken1;
            }
        }
    }
}
