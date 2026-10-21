// Tickets termicos (58 / 80 mm) de un pedido: UN ticket por sucursal, con el mismo formato que el ticket de Movimientos
// (MovimientosController.ConstruirLineasTicketMovimiento): encabezado, columnas Producto / Cant. / Kgs., el codigo debajo de cada producto
// en fuente B y un renglon de total. NO lleva ningun importe (ni precios ni total estimado): es para preparar y controlar lo que pide cada
// sucursal. El papel termico es angosto: por eso una sucursal por ticket y, si el pedido cubre varias, se corta entre una y otra.
// UNA sola fuente para el payload del agente ESC/POS (formatoEscPos = true, con negritas y cortes incrustados) y para el HTML del fallback
// por navegador (formatoEscPos = false, texto plano con un salto de pagina entre tickets), asi no pueden desalinearse (mismo criterio que
// ComprobanteTickets, cuyos helpers de formato se reusan). Los precios siguen en la pantalla, el PDF, el Excel y el mail.
// Detalle y porques: docs/DECISIONS.md (2026-10-20, "Ticket de pedido por sucursal").
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Entidades;

namespace WebCore.Services
{
    public static class PedidoTicket
    {
        // Linea en blanco que no se pierde en la termica (NBSP, igual que el resto de los tickets).
        private const string Blanco = " ";

        // Separador entre tickets en el HTML de respaldo (formatoEscPos = false): _TicketTexto.cshtml parte el texto en este caracter y deja un
        // salto de pagina; la impresora corta al final de cada pagina. En el payload ESC/POS el separador es el comando de corte.
        public const string SaltoDePagina = "\f";

        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        // Anchos de las columnas: los mismos que usa el ticket de Movimientos (58 mm = 32 caracteres, 80 mm = 43).
        private sealed class Columnas
        {
            public int Ancho;
            public int Producto;
            public int Cantidad;
            public int Kilos;
        }

        private static Columnas ColumnasPara(int mm)
        {
            bool angosto = ComprobanteTickets.NormalizarMm(mm) == 58;
            return new Columnas
            {
                Ancho = ComprobanteTickets.CaracteresPorLinea(mm),
                Producto = angosto ? 15 : 22,
                Cantidad = angosto ? 5 : 6,
                Kilos = angosto ? 7 : 8
            };
        }

        // Todas las lineas del pedido para imprimir de una vez: los tickets de cada sucursal uno detras de otro, con el corte (ESC/POS) o el
        // salto de pagina (HTML) en el medio. Despues del ultimo no hay corte: lo agrega el agente al final de todo trabajo de impresion.
        public static List<string> ConstruirLineas(Pedido pedido, int mm, bool formatoEscPos)
        {
            var tickets = ConstruirTickets(pedido, mm, formatoEscPos);
            var lineas = new List<string>();
            for (int i = 0; i < tickets.Count; i++)
            {
                if (i > 0) lineas.Add(formatoEscPos ? EscPosFormato.Corte() : SaltoDePagina);
                lineas.AddRange(tickets[i]);
            }
            return lineas;
        }

        // Un ticket por sucursal con productos, ordenadas por nombre (el mismo orden que el detalle, el PDF y el Excel). Un pedido sin lineas
        // (no deberia existir: se rechaza al guardar) devuelve un unico ticket con el encabezado y el aviso.
        public static List<List<string>> ConstruirTickets(Pedido pedido, int mm, bool formatoEscPos)
        {
            var lineasPedido = pedido != null && pedido.Lineas != null ? pedido.Lineas : new List<PedidoLinea>();
            var grupos = lineasPedido
                .GroupBy(l => l.IdSucursal)
                .Select(g => new { IdSucursal = g.Key, Nombre = g.First().Sucursal ?? "", Lineas = g.ToList() })
                .OrderBy(g => g.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(g => g.IdSucursal)
                .ToList();

            var tickets = new List<List<string>>();
            if (grupos.Count == 0)
            {
                tickets.Add(ConstruirTicket(pedido, "-", new List<PedidoLinea>(), 1, 1, mm, formatoEscPos));
                return tickets;
            }

            for (int i = 0; i < grupos.Count; i++)
                tickets.Add(ConstruirTicket(pedido, grupos[i].Nombre, grupos[i].Lineas, i + 1, grupos.Count, mm, formatoEscPos));
            return tickets;
        }

        // El ticket de UNA sucursal. numero/cantidad: "1 de 2", para no perder de vista que hay mas tickets del mismo pedido.
        private static List<string> ConstruirTicket(Pedido pedido, string sucursal, List<PedidoLinea> lineasSucursal, int numero, int cantidad, int mm, bool formatoEscPos)
        {
            var c = ColumnasPara(mm);
            var resultado = new List<string>();
            Func<string, string> negrita = linea => formatoEscPos ? EscPosFormato.Negrita(linea) : linea;
            Func<string, string> fuenteB = linea => formatoEscPos ? EscPosFormato.FuenteB(linea) : linea;

            // Encabezado: titulo y sucursal en negrita, como "Movimiento", "Origen" y "Destino".
            string numeroPedido = "N°: " + (pedido != null ? pedido.Id : 0) + (cantidad > 1 ? "  (" + numero + " de " + cantidad + ")" : "");
            resultado.Add(negrita(ComprobanteTickets.Centrar("Pedido", c.Ancho)));
            resultado.Add(ComprobanteTickets.Truncar(numeroPedido, c.Ancho));
            resultado.Add(negrita(ComprobanteTickets.Truncar("Sucursal: " + sucursal, c.Ancho)));
            resultado.Add(ComprobanteTickets.Truncar("Fecha: " + (pedido != null ? pedido.Fecha : DateTime.Now).ToString("dd/MM/yyyy HH:mm", CulturaAr), c.Ancho));
            resultado.Add(ComprobanteTickets.Truncar("Usuario: " + (pedido != null ? pedido.Usuario ?? "" : ""), c.Ancho));
            resultado.Add(Blanco);
            resultado.Add(Renglon("Producto", "Cant.", "Kgs.", c));
            resultado.Add(new string('-', c.Ancho));

            // Detalle: una linea por producto (nombre cortado al ancho de la columna, como en Movimientos) y otra con su codigo en fuente B
            // (la termica es monocromo: no hay gris). El codigo usa todo el ancho: un codigo de barras de 13 digitos no entra en la columna del nombre.
            decimal totalUnidades = 0m, totalKilos = 0m;
            foreach (var linea in lineasSucursal.OrderBy(l => l.Codigo).ThenBy(l => l.Producto, StringComparer.CurrentCultureIgnoreCase))
            {
                // Un producto por peso se pide en kilos (columna Kgs.); el resto, en unidades (columna Cant.). Los dos totales no se mezclan.
                string unidades = "", kilos = "";
                if (linea.Pesable) { kilos = Numero(linea.Cantidad, c.Kilos); totalKilos += linea.Cantidad; }
                else { unidades = Numero(linea.Cantidad, c.Cantidad); totalUnidades += linea.Cantidad; }

                resultado.Add(Renglon(linea.Producto, unidades, kilos, c));
                resultado.Add(fuenteB(ComprobanteTickets.Truncar("Cod:" + linea.Codigo, c.Ancho)));
            }

            if (lineasSucursal.Count == 0) resultado.Add("Sin productos");

            resultado.Add(new string('-', c.Ancho));
            resultado.Add(Renglon("Total", Numero(totalUnidades, c.Cantidad), Numero(totalKilos, c.Kilos), c));

            if (pedido != null && !string.IsNullOrWhiteSpace(pedido.Nota))
            {
                resultado.Add("");
                resultado.Add("Observaciones:");
                resultado.AddRange(ComprobanteTickets.Envolver(pedido.Nota, c.Ancho));
            }

            // Unas lineas en blanco al final para poder cortar el papel (igual que el resto de los tickets).
            for (int i = 0; i < 3; i++) resultado.Add(Blanco);
            return resultado;
        }

        // "Producto  Cant.   Kgs.": nombre a la izquierda y los dos numeros a la derecha, con el mismo espaciado que el ticket de Movimientos.
        private static string Renglon(string nombre, string unidades, string kilos, Columnas c)
        {
            return Ajustar(nombre, c.Producto, false) + " " + Ajustar(unidades, c.Cantidad, true) + "    " + Ajustar(kilos, c.Kilos, true);
        }

        // Corta el texto al ancho de la columna y lo alinea. Solo se usa con nombres y rotulos: las cantidades pasan antes por Numero, que
        // nunca corta un digito.
        private static string Ajustar(string texto, int ancho, bool derecha)
        {
            texto = ComprobanteTickets.Truncar(texto, ancho);
            return derecha ? texto.PadLeft(ancho) : texto.PadRight(ancho);
        }

        // Cantidad sin ceros de relleno ("12,5"). Si no entra en la columna se le sacan decimales: jamas se corta un digito (un 12345 no
        // puede salir como 1234).
        private static string Numero(decimal valor, int ancho)
        {
            foreach (string formato in new[] { "0.###", "0.##", "0.#", "0" })
            {
                string texto = valor.ToString(formato, CulturaAr);
                if (texto.Length <= ancho) return texto;
            }
            return valor.ToString("0", CulturaAr);
        }
    }
}
