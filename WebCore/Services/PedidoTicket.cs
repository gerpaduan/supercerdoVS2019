// Lineas de texto del ticket termico (58 / 80 mm) de un pedido. UNA sola fuente para el payload del agente ESC/POS (formatoEscPos = true, con
// negritas incrustadas) y para el HTML del fallback por navegador (formatoEscPos = false, texto plano), asi no pueden desalinearse (mismo
// criterio que ComprobanteTickets, cuyos helpers de formato se reusan). Vive en un archivo aparte de ComprobanteTickets.cs a proposito.
// Los importes son una ESTIMACION con el precio de la ultima compra (se rotula asi); las lineas sin precio no suman y se avisa cuantas son.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Entidades;

namespace WebCore.Services
{
    public static class PedidoTicket
    {
        private const string Blanco = " ";
        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        public static List<string> ConstruirLineas(Pedido pedido, int mm, bool formatoEscPos)
        {
            int ancho = ComprobanteTickets.CaracteresPorLinea(mm);
            var pivot = PedidoPivot.Construir(pedido);
            var lineas = new List<string>();

            Func<string, string> negrita = linea => formatoEscPos ? EscPosFormato.Negrita(linea) : linea;

            lineas.Add(negrita(ComprobanteTickets.Centrar("PEDIDO N° " + pedido.Id, ancho)));
            lineas.Add("");
            lineas.Add(ComprobanteTickets.Truncar("Fecha: " + pedido.Fecha.ToString("dd/MM/yyyy  HH:mm", CulturaAr), ancho));
            lineas.Add(ComprobanteTickets.Truncar("Usuario: " + (pedido.Usuario ?? ""), ancho));
            lineas.AddRange(ComprobanteTickets.Envolver("Sucursales: " + string.Join(", ", pivot.Sucursales.Select(s => s.Nombre)), ancho, 3));
            lineas.Add(new string('-', ancho));

            foreach (var fila in pivot.Filas)
            {
                lineas.AddRange(ComprobanteTickets.Envolver(fila.Codigo + " " + fila.Producto, ancho, 5).Select(l => negrita(l)));

                for (int i = 0; i < pivot.Sucursales.Count; i++)
                {
                    decimal? cantidad = fila.Cantidades[i];
                    if (!cantidad.HasValue) continue;
                    lineas.Add(ComprobanteTickets.Extremos("  " + pivot.Sucursales[i].Nombre, Cantidad(cantidad.Value, fila.Pesable), ancho));
                }

                string total = Cantidad(fila.Total, fila.Pesable);
                if (fila.Pesable && fila.Promedio.HasValue && fila.Promedio.Value > 0m)
                    total += " (~" + (fila.Total / fila.Promedio.Value).ToString("N1", CulturaAr) + " un.)";
                lineas.Add(negrita(ComprobanteTickets.Extremos("  Total", total, ancho)));

                lineas.Add(ComprobanteTickets.Extremos("  Importe est.", fila.Importe.HasValue ? Dinero(fila.Importe.Value) : "sin precio", ancho));
                // Fecha de la compra que dio el precio; "(!)" si es vieja respecto del pedido (el simbolo de aviso no sale en una termica).
                // La marca va a la derecha, junto al precio: en 58 mm (32 caracteres) a la izquierda se perdia al recortar el texto.
                if (fila.Precio.HasValue && fila.FechaPrecio.HasValue)
                    lineas.Add(ComprobanteTickets.Extremos("  Precio " + fila.FechaPrecio.Value.ToString("dd/MM/yy", CulturaAr), (fila.PrecioViejo ? "(!) " : "") + Dinero(fila.Precio.Value), ancho));
            }

            lineas.Add(new string('-', ancho));
            lineas.Add(ComprobanteTickets.Truncar("Productos: " + pivot.Filas.Count + "  Líneas: " + pivot.Lineas, ancho));
            // Con productos sin precio el total es parcial: se rotula asi (en el de 58 mm la etiqueta larga se achica para que entre el importe).
            string etiquetaTotal = global::Negocio.PedidoTextos.EtiquetaTotal(pivot.LineasSinPrecio);
            if (etiquetaTotal.Length + Dinero(pivot.TotalEstimado).Length + 1 > ancho) etiquetaTotal = pivot.LineasSinPrecio > 0 ? "Total parcial" : "Total";
            lineas.Add(negrita(ComprobanteTickets.Extremos(etiquetaTotal, Dinero(pivot.TotalEstimado), ancho)));
            string avisoSinPrecio = global::Negocio.PedidoTextos.AvisoSinPrecio(pivot.LineasSinPrecio);
            if (avisoSinPrecio.Length > 0)
                lineas.AddRange(ComprobanteTickets.Envolver(avisoSinPrecio, ancho));

            if (!string.IsNullOrWhiteSpace(pedido.Nota))
                lineas.AddRange(ComprobanteTickets.Envolver("Nota: " + pedido.Nota, ancho, 6));

            // De donde sale el importe: el mismo texto que la pantalla, el PDF, el Excel y el mail (Negocio.PedidoTextos).
            lineas.AddRange(ComprobanteTickets.Envolver(global::Negocio.PedidoTextos.AclaracionPrecio, ancho));
            if (pivot.HayPreciosViejos)
                lineas.AddRange(ComprobanteTickets.Envolver("(!) " + global::Negocio.PedidoTextos.AvisoPrecioViejo(pivot.DiasPrecioViejo), ancho));

            // Unas lineas en blanco al final para poder cortar el papel (igual que el resto de los tickets).
            for (int i = 0; i < 3; i++) lineas.Add(Blanco);
            return lineas;
        }

        private static string Cantidad(decimal valor, bool pesable)
        {
            return valor.ToString("0.###", CulturaAr) + (pesable ? " kg" : "");
        }

        private static string Dinero(decimal valor)
        {
            return "$ " + valor.ToString("N2", CulturaAr);
        }
    }
}
