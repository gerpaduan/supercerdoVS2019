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
        //
        // Cobro junto con la venta (2026-10-21): si el cliente pago en el momento (`cobros`), el bloque sigue con
        // un renglon por cada pago y cierra con el saldo con el que queda la cuenta (el "Saldo actual", en negrita):
        //
        //   Saldo anterior (debe)       5.000,00
        //                       -------
        //   TOTAL c/saldo               7.000,00
        //   Su pago Efectivo            3.000,00
        //                       -------
        //   Saldo actual (debe)         4.000,00      (o "Saldo actual 0,00" si salda la cuenta)
        //
        // Sin cobros el bloque es el de siempre.
        public static List<Linea> Construir(decimal totalVenta, decimal saldoPrevio, int ancho, bool sumarVenta,
            IReadOnlyList<Cobro> cobros = null)
        {
            decimal saldo = Negocio.CalculoSaldoCliente.Redondear(saldoPrevio);
            bool hayCobros = cobros != null && cobros.Count > 0;

            // 58 mm (32 caracteres) usa "Saldo ant.": con "Saldo anterior (a favor)" + un importe de
            // 12 caracteres la linea se pasaria del ancho y la impresora la partiria.
            string etiquetaSaldo = (ancho >= 40 ? "Saldo anterior" : "Saldo ant.") + (saldo < 0m ? " (debe)" : " (a favor)");

            var lineas = new List<Linea> { new Linea(Renglon(etiquetaSaldo, Math.Abs(saldo), ancho), false) };

            if (sumarVenta)
            {
                decimal total = Negocio.CalculoSaldoCliente.Redondear(Negocio.CalculoSaldoCliente.TotalConSaldo(totalVenta, saldo));
                lineas.Add(new Linea("-------".PadLeft(ancho), false));
                // Con cobros la linea resaltada es el saldo actual del final, no este total.
                lineas.Add(new Linea(Renglon(total >= 0m ? "TOTAL c/saldo" : "A favor", Math.Abs(total), ancho), !hayCobros));
            }

            if (hayCobros)
            {
                decimal totalCobros = 0m;
                foreach (var cobro in cobros)
                {
                    decimal importe = Negocio.CalculoSaldoCliente.Redondear(cobro.Importe);
                    totalCobros += importe;
                    // 58 mm: "Pago Transferencia" + importe entra en 32 caracteres; "Su pago Transferencia" no.
                    lineas.Add(new Linea(Renglon((ancho >= 40 ? "Su pago " : "Pago ") + cobro.FormaPago, importe, ancho), false));
                }

                decimal saldoFinal = Negocio.CalculoSaldoCliente.Redondear(
                    Negocio.CalculoSaldoCliente.SaldoTrasCobros(saldo, totalVenta, sumarVenta, totalCobros));
                string etiquetaFinal = (ancho >= 40 ? "Saldo actual" : "Saldo act.")
                    + (saldoFinal < 0m ? " (debe)" : (saldoFinal > 0m ? " (a favor)" : ""));

                lineas.Add(new Linea("-------".PadLeft(ancho), false));
                lineas.Add(new Linea(Renglon(etiquetaFinal, Math.Abs(saldoFinal), ancho), true));
            }

            return lineas;
        }

        // Un cobro que el cliente pago junto con la venta (forma de pago tal como se muestra y su importe).
        public sealed class Cobro
        {
            public Cobro(string formaPago, decimal importe)
            {
                FormaPago = formaPago;
                Importe = importe;
            }

            public string FormaPago { get; }
            public decimal Importe { get; }
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
