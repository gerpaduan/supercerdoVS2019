// Bloque "Saldo" del ticket termico de VENTA (58 / 80 mm): saldo anterior del cliente y total con saldo.
// UNA sola fuente de texto: la usan las dos copias del ticket de venta que se mantienen a mano
// (VentasController.ConstruirLineasTicketVenta para el agente ESC/POS y Views/Ventas/_TicketHTML.cshtml
// para el navegador), asi el bloque no puede desalinearse entre ambas. Ver docs/DECISIONS.md
// ("Saldo del cliente en el POS y en el ticket", 2026-10-19).
using System;
using System.Collections.Generic;

namespace WebCore.Services
{
    public static class TicketSaldoLineas
    {
        // Una linea del bloque. EsTotal = la linea final, que el ticket resalta (negrita en ESC/POS,
        // clase ticket-total en HTML).
        public sealed class Linea
        {
            public Linea(string texto, bool esTotal)
            {
                Texto = texto;
                EsTotal = esTotal;
            }

            public string Texto { get; }
            public bool EsTotal { get; }
        }

        // Arma el bloque que va debajo del total de la venta. La venta se SUMA al saldo solo si es a
        // cuenta corriente (`sumarVenta`): ahi la deuda realmente crece. Si se pago de otra forma, la
        // cuenta no cambia y el ticket muestra solo el saldo anterior (pedido del usuario, 2026-10-19).
        //
        //   sumarVenta = true                          sumarVenta = false
        //   Saldo anterior (debe)       5.000,00       Saldo anterior (debe)       5.000,00
        //                       -------
        //   TOTAL c/saldo               7.000,00       (nada mas)
        //
        // En "debe" el total es venta + deuda; con saldo a favor se resta y, si el saldo supera la venta,
        // la ultima linea dice "A favor" (no se imprime un total negativo).
        //
        // `saldoPrevio` es el saldo ANTES de esta venta, con signo (negativo = debe; ver
        // Negocio.CalculoSaldoCliente). `totalVenta` es el mismo total que imprime el ticket arriba.
        // Los importes salen con "N2" y sin simbolo, igual que el resto de las lineas del ticket.
        public static List<Linea> Construir(decimal totalVenta, decimal saldoPrevio, int ancho, bool sumarVenta)
        {
            decimal saldo = Negocio.CalculoSaldoCliente.Redondear(saldoPrevio);

            // 58 mm (32 caracteres) usa "Saldo ant.": con "Saldo anterior (a favor)" + un importe de
            // 12 caracteres la linea se pasaria del ancho y la impresora la partiria.
            string etiquetaSaldo = (ancho >= 40 ? "Saldo anterior" : "Saldo ant.") + (saldo < 0m ? " (debe)" : " (a favor)");

            var lineas = new List<Linea> { new Linea(Renglon(etiquetaSaldo, Math.Abs(saldo), ancho), false) };

            if (sumarVenta)
            {
                decimal total = Negocio.CalculoSaldoCliente.Redondear(Negocio.CalculoSaldoCliente.TotalConSaldo(totalVenta, saldo));
                lineas.Add(new Linea("-------".PadLeft(ancho), false));
                lineas.Add(new Linea(Renglon(total >= 0m ? "TOTAL c/saldo" : "A favor", Math.Abs(total), ancho), true));
            }

            return lineas;
        }

        // "etiqueta ........ importe" en una linea de `ancho` caracteres. Mismo armado que formatearTotal
        // del ticket de venta (etiqueta a la izquierda, importe a la derecha, minimo un espacio).
        private static string Renglon(string etiqueta, decimal importe, int ancho)
        {
            string derecha = importe.ToString("N2");
            int espacios = ancho - (etiqueta.Length + derecha.Length);
            if (espacios < 1) espacios = 1;
            return etiqueta + new string(' ', espacios) + derecha;
        }
    }
}
