// PDF (QuestPDF) de un pedido, A4: una fila por producto con la cantidad de cada sucursal, el total pedido, el ultimo precio de compra y el
// importe estimado; abajo el total estimado. Lo usan la descarga y el adjunto del mail de Pedidos. La licencia de QuestPDF se fija en Program.cs.
using System.Globalization;
using System.Linq;
using Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WebCore.Services
{
    public static class PdfPedido
    {
        private const float TamanoLetra = 8.5f;
        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        public static byte[] Generar(Pedido pedido)
        {
            return Crear(pedido).GeneratePdf();
        }

        // Documento listo para renderizar (publico para poder revisarlo como imagen).
        public static IDocument Crear(Pedido pedido)
        {
            var pivot = PedidoPivot.Construir(pedido);
            int sucursales = pivot.Sucursales.Count;

            return Document.Create(contenedor =>
            {
                contenedor.Page(pagina =>
                {
                    // Con 4 o mas sucursales la tabla pasa a apaisado para que las columnas respiren.
                    pagina.Size(sucursales >= 4 ? PageSizes.A4.Landscape() : PageSizes.A4);
                    pagina.Margin(24, Unit.Point);
                    pagina.DefaultTextStyle(x => x.FontSize(TamanoLetra));

                    pagina.Header().Column(col =>
                    {
                        col.Item().Text("Pedido N° " + pedido.Id).FontSize(15).Bold();
                        col.Item().Text("Fecha: " + pedido.Fecha.ToString("dd/MM/yyyy HH:mm", CulturaAr) + "  ·  Usuario: " + pedido.Usuario).FontColor(Colors.Grey.Darken1);
                        if (!string.IsNullOrWhiteSpace(pedido.Nota))
                            col.Item().Text("Nota: " + pedido.Nota).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(6);
                    });

                    pagina.Content().Column(col =>
                    {
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(58);
                                c.RelativeColumn(3f);
                                for (int i = 0; i < sucursales; i++) c.RelativeColumn(1f);
                                c.RelativeColumn(1.1f);
                                c.RelativeColumn(2f);   // precio + proveedor y fecha de esa compra (chico, debajo)
                                c.RelativeColumn(1.4f);
                            });

                            tabla.Header(h =>
                            {
                                h.Cell().Element(Encabezado).Text("Cód.").Bold();
                                h.Cell().Element(Encabezado).Text("Producto").Bold();
                                foreach (var s in pivot.Sucursales) h.Cell().Element(Encabezado).AlignRight().Text(s.Nombre).Bold();
                                h.Cell().Element(Encabezado).AlignRight().Text("Total").Bold();
                                h.Cell().Element(Encabezado).AlignRight().Text("$ últ. compra").Bold();
                                h.Cell().Element(Encabezado).AlignRight().Text("Importe est.").Bold();
                            });

                            foreach (var fila in pivot.Filas)
                            {
                                tabla.Cell().Element(Celda).Text(fila.Codigo.ToString());
                                tabla.Cell().Element(Celda).Text(fila.Producto);
                                for (int i = 0; i < sucursales; i++)
                                    tabla.Cell().Element(Celda).AlignRight().Text(fila.Cantidades[i].HasValue ? Cantidad(fila.Cantidades[i].Value) : "");

                                string total = Cantidad(fila.Total);
                                tabla.Cell().Element(Celda).AlignRight().Text(total).Bold();
                                // Precio + de quien y de cuando es (chico debajo); en naranja con los dias si es viejo respecto de la fecha del pedido.
                                tabla.Cell().Element(Celda).AlignRight().Column(c =>
                                {
                                    c.Item().AlignRight().Text(fila.Precio.HasValue ? Dinero(fila.Precio.Value) : "sin precio")
                                        .FontColor(fila.Precio.HasValue ? Colors.Black : Colors.Grey.Medium);
                                    string origen = fila.Precio.HasValue ? global::Negocio.PedidoTextos.DescribirOrigenPrecio(fila.Proveedor, fila.FechaPrecio) : "";
                                    if (fila.PrecioViejo) origen = (origen.Length > 0 ? origen + " · " : "") + "hace " + fila.DiasDelPrecio + " días";
                                    if (origen.Length > 0)
                                        c.Item().AlignRight().Text(origen).FontSize(6.5f)
                                            .FontColor(fila.PrecioViejo ? Colors.Orange.Darken2 : Colors.Grey.Darken1);
                                });
                                tabla.Cell().Element(Celda).AlignRight().Text(fila.Importe.HasValue ? Dinero(fila.Importe.Value) : "—")
                                    .FontColor(fila.Importe.HasValue ? Colors.Black : Colors.Grey.Medium);
                            }
                        });

                        col.Item().PaddingTop(8).AlignRight().Text(t =>
                        {
                            t.Span(global::Negocio.PedidoTextos.EtiquetaTotal(pivot.LineasSinPrecio) + ": ").FontSize(11);
                            t.Span(Dinero(pivot.TotalEstimado)).FontSize(11).Bold();
                        });

                        string aviso = global::Negocio.PedidoTextos.AvisoSinPrecio(pivot.LineasSinPrecio);
                        if (aviso.Length > 0)
                            col.Item().AlignRight().Text(aviso).FontColor(Colors.Grey.Darken1);

                        // De donde sale el importe: el mismo texto que la pantalla, el Excel y el mail (Negocio.PedidoTextos).
                        col.Item().PaddingTop(4).Text(global::Negocio.PedidoTextos.AclaracionPrecio
                            + (pivot.HayPreciosViejos ? " " + global::Negocio.PedidoTextos.AvisoPrecioViejo(pivot.DiasPrecioViejo) : ""))
                            .FontColor(Colors.Grey.Darken1).FontSize(8f);
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
        }

        private static IContainer Encabezado(IContainer c)
        {
            return c.Background("#E9ECEF").BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingVertical(3).PaddingHorizontal(3);
        }

        private static IContainer Celda(IContainer c)
        {
            return c.BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).PaddingHorizontal(3);
        }

        private static string Cantidad(decimal valor)
        {
            return valor.ToString("0.###", CulturaAr);
        }

        private static string Dinero(decimal valor)
        {
            return "$ " + valor.ToString("N2", CulturaAr);
        }
    }
}
